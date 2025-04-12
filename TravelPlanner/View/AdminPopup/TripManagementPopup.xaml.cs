using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TravelPlanner.Model;
using TravelPlanner.ViewModel;
using Xamarin.Forms;

namespace TravelPlanner.View.AdminPopup
{
    public partial class TripManagementPopup : PopupPage
    {
        private readonly DatabaseHelper _dbHelper;
        private ObservableCollection<SelectableTrip> _selectableTrips;
        private ObservableCollection<Trips> _allTrips;

        public TripManagementPopup()
        {
            InitializeComponent();
            HasKeyboardOffset = false;
            _dbHelper = new DatabaseHelper();
            _selectableTrips = new ObservableCollection<SelectableTrip>();
            _allTrips = new ObservableCollection<Trips>();
            StatusFilter.SelectedIndex = 0;
            LoadTrips();
        }

        private async void LoadTrips()
        {
            try
            {
                string query = "SELECT * FROM Trips ORDER BY StartDate DESC";
                var trips = await _dbHelper.getFromDatabase(query, reader => new Trips
                {
                    TripID = Convert.ToInt32(reader["TripID"]),
                    TripName = reader["TripName"].ToString(),
                    Description = reader["Description"].ToString(),
                    Location = reader["Location"].ToString(),
                    StartDate = Convert.ToDateTime(reader["StartDate"]),
                    EndDate = Convert.ToDateTime(reader["EndDate"]),
                    Image = reader["Image"].ToString(),
                    Organizer = reader["UserEmail"].ToString()
                });

                _allTrips = new ObservableCollection<Trips>(trips);
                _selectableTrips.Clear();
                foreach (var trip in _allTrips)
                {
                    _selectableTrips.Add(new SelectableTrip { Trip = trip });
                }
                TripsCollection.ItemsSource = _selectableTrips;
                FilterTrips();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load trips: " + ex.Message, "OK");
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            FilterTrips();
        }

        private void OnStatusFilterChanged(object sender, EventArgs e)
        {
            FilterTrips();
        }

        private void FilterTrips()
        {
            var searchText = SearchBar.Text?.ToLowerInvariant() ?? "";
            var statusFilter = StatusFilter.SelectedItem?.ToString();
            var now = DateTime.Now;

            var filteredTrips = _selectableTrips.Where(st =>
                (string.IsNullOrEmpty(searchText) ||
                 st.Trip.TripName.ToLowerInvariant().Contains(searchText) ||
                 st.Trip.Location.ToLowerInvariant().Contains(searchText)) &&
                (statusFilter == "All" ||
                 (statusFilter == "Upcoming" && st.Trip.StartDate > now) ||
                 (statusFilter == "Active" && st.Trip.StartDate <= now && st.Trip.EndDate >= now) ||
                 (statusFilter == "Past" && st.Trip.EndDate < now))).ToList();

            _selectableTrips.Clear();
            foreach (var trip in filteredTrips)
            {
                _selectableTrips.Add(trip);
            }
        }

        private async void OnAddTripClicked(object sender, EventArgs e)
        {
            MessagingCenter.Subscribe<AddTripsPopup>(this, AddTripsPopup.TripAddedMessage,
                async (popup) =>
                {
                    LoadTrips();
                    MessagingCenter.Unsubscribe<AddTripsPopup>(this, AddTripsPopup.TripAddedMessage);
                });

            await PopupNavigation.Instance.PushAsync(new AddTripsPopup("admin@admin.com")); 
        }

        private async void EditTripClicked(object sender, EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            var selectableTrip = swipeItem?.BindingContext as SelectableTrip;
            var trip = selectableTrip?.Trip;
            if (trip != null)
            {
                MessagingCenter.Subscribe<AddTripsPopup>(this, AddTripsPopup.TripUpdatedMessage,
                    async (popup) =>
                    {
                        LoadTrips();
                        MessagingCenter.Unsubscribe<AddTripsPopup>(this, AddTripsPopup.TripUpdatedMessage);
                    });

                await PopupNavigation.Instance.PushAsync(new AddTripsPopup(trip, "admin@admin.com"));
            }
        }

        private async void DeleteTripClicked(object sender, EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            var selectableTrip = swipeItem?.BindingContext as SelectableTrip;
            var trip = selectableTrip?.Trip;
            if (trip != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete",
                    $"Are you sure you want to delete trip {trip.TripName}?",
                    "Delete", "Cancel");

                if (confirm)
                {
                    try
                    {
                        bool success = await _dbHelper.DeleteSingleRecordFromDatabase("Trips", "TripID", trip.TripID);
                        if (success)
                        {
                            _selectableTrips.Remove(selectableTrip);
                            _allTrips.Remove(trip);
                            await DisplayAlert("Success", "Trip deleted successfully", "OK");
                            // LoadTrips(); No need to reload all, just update the collections
                        }
                        else
                        {
                            await DisplayAlert("Error", "Failed to delete trip", "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", "Failed to delete trip: " + ex.Message, "OK");
                    }
                }
            }
        }

        private async void OnBulkDeleteClicked(object sender, EventArgs e)
        {
            var selectedTrips = _selectableTrips.Where(t => t.IsSelected).Select(t => t.Trip).ToList();

            if (selectedTrips.Count == 0)
            {
                await DisplayAlert("Warning", "Please select trips to delete.", "OK");
                return;
            }

            bool confirm = await DisplayAlert("Confirm Delete",
                $"Are you sure you want to delete {selectedTrips.Count} trips?",
                "Delete", "Cancel");

            if (confirm)
            {
                try
                {
                    var deletedCount = 0;
                    foreach (var trip in selectedTrips.ToList()) // Iterate over a copy
                    {
                        bool success = await _dbHelper.DeleteSingleRecordFromDatabase("Trips", "TripID", trip.TripID);
                        if (success)
                        {
                            var tripToRemove = _selectableTrips.FirstOrDefault(st => st.Trip.TripID == trip.TripID);
                            if (tripToRemove != null)
                            {
                                _selectableTrips.Remove(tripToRemove);
                            }
                            _allTrips.Remove(_allTrips.First(t => t.TripID == trip.TripID));
                            deletedCount++;
                        }
                        else
                        {
                            await DisplayAlert("Error", $"Failed to delete trip: {trip.TripName}", "OK");
                        }
                    }

                    await DisplayAlert("Success", $"{deletedCount} trip(s) deleted successfully.", "OK");
                    // LoadTrips(); No need to reload all, collections are updated
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", "Failed to delete trips: " + ex.Message, "OK");
                }
            }
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }
    }

    public class SelectableTrip
    {
        public Trips Trip { get; set; }
        public bool IsSelected { get; set; }
    }
}
