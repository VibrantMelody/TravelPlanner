using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System;
using TravelPlanner.Model;
using Xamarin.Forms;

namespace TravelPlanner.ViewModel
{
    class TripDetailViewModel : INotifyPropertyChanged
    {
        private Trips _trip;
        private ObservableCollection<TripUsers> _tripUsers;
        private readonly DatabaseHelper helper;

        public event PropertyChangedEventHandler PropertyChanged;

        public Trips Trip
        {
            get => _trip;
            set
            {
                if (_trip != value)
                {
                    _trip = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<TripUsers> TripUsers
        {
            get => _tripUsers;
            set
            {
                if (_tripUsers != value)
                {
                    _tripUsers = value;
                    OnPropertyChanged();
                }
            }
        }

        public TripDetailViewModel(Trips selectedTrip)
        {
            Trip = selectedTrip;
            helper = new DatabaseHelper();
            TripUsers = new ObservableCollection<TripUsers>();
            _ = LoadDataAsync(Trip);
        }

        private async Task LoadDataAsync(Trips trip)
        {
            string userQuery = $"SELECT * FROM Users WHERE UserID IN (SELECT UserID FROM TripParticipants WHERE TripID = {trip.TripID})";
            try
            {
                var participantsFromDB = await helper.getFromDatabase(userQuery, reader => new TripUsers
                {
                    UserID = Convert.ToInt32(reader["UserID"]),
                    UserName = reader["UserName"].ToString(),
                    FullName = reader["Fullname"].ToString(),
                    Email = reader["Email"].ToString(),
                    ProfilePicture = reader["ProfilePicture"].ToString(),
                    DateRegistered = Convert.ToDateTime(reader["DateRegistered"]),
                });

                Device.BeginInvokeOnMainThread(() =>
                {
                    TripUsers.Clear();
                    foreach (var user in participantsFromDB)
                    {
                        TripUsers.Add(user);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading trip users: {ex.Message}");
            }
        }

        public async Task RefreshUsersList()
        {
            await LoadDataAsync(Trip);
        }


public async Task UpdateTrip()
{
    string tripQuery = $"SELECT * FROM Trips WHERE TripID = {Trip.TripID}";
    try
    {
        var updatedTrips = await helper.getFromDatabase(tripQuery, reader => new Trips
        {
            TripID = Convert.ToInt32(reader["TripID"]),
            TripName = reader["TripName"].ToString(),
            Location = reader["Location"].ToString(),
            Description = reader["Description"].ToString(),
            StartDate = Convert.ToDateTime(reader["StartDate"]),
            EndDate = Convert.ToDateTime(reader["EndDate"]),
            Image = reader["Image"].ToString(),
            Organizer = reader["UserEmail"].ToString()
        });

        if (updatedTrips.Count > 0)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Trip = updatedTrips[0];
            });
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating trip data: {ex.Message}");
    }
}


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
