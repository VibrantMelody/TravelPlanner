using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TravelPlanner.View.AdminPopup
{
    public partial class StatisticsPopupPage : PopupPage
    {
        private readonly DatabaseHelper _dbHelper;

        // Statistics properties
        public int TotalUsers { get; private set; }
        public int AdminUsers { get; private set; }
        public int RegularUsers { get; private set; }
        public int NewUsers { get; private set; }
        public int TotalTrips { get; private set; }
        public int ActiveTrips { get; private set; }
        public int UpcomingTrips { get; private set; }
        public int CompletedTrips { get; private set; }

        public StatisticsPopupPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            BindingContext = this;
            LoadStatistics();
        }

        private async void LoadStatistics()
        {
            try
            {
                // Load Users Statistics
                string userQuery = @"
                    SELECT
                        COUNT(*) as TotalUsers,
                        COUNT(CASE WHEN Role = 'Admin' THEN 1 END) as AdminUsers,
                        COUNT(CASE WHEN Role = 'User' THEN 1 END) as RegularUsers,
                        COUNT(CASE WHEN DATEDIFF(day, DateRegistered, GETDATE()) <= 30 THEN 1 END) as NewUsers
                    FROM Users";

                var userStats = await _dbHelper.getFromDatabase(userQuery, reader => new
                {
                    TotalUsers = Convert.ToInt32(reader["TotalUsers"]),
                    AdminUsers = Convert.ToInt32(reader["AdminUsers"]),
                    RegularUsers = Convert.ToInt32(reader["RegularUsers"]),
                    NewUsers = Convert.ToInt32(reader["NewUsers"])
                });

                if (userStats.Count > 0)
                {
                    TotalUsers = userStats[0].TotalUsers;
                    AdminUsers = userStats[0].AdminUsers;
                    RegularUsers = userStats[0].RegularUsers;
                    NewUsers = userStats[0].NewUsers;
                }

                // Load Trips Statistics
                string tripQuery = @"
                    SELECT
                        COUNT(*) as TotalTrips,
                        COUNT(CASE WHEN StartDate <= GETDATE() AND EndDate >= GETDATE() THEN 1 END) as ActiveTrips,
                        COUNT(CASE WHEN StartDate > GETDATE() THEN 1 END) as UpcomingTrips,
                        COUNT(CASE WHEN EndDate < GETDATE() THEN 1 END) as CompletedTrips
                    FROM Trips";

                var tripStats = await _dbHelper.getFromDatabase(tripQuery, reader => new
                {
                    TotalTrips = Convert.ToInt32(reader["TotalTrips"]),
                    ActiveTrips = Convert.ToInt32(reader["ActiveTrips"]),
                    UpcomingTrips = Convert.ToInt32(reader["UpcomingTrips"]),
                    CompletedTrips = Convert.ToInt32(reader["CompletedTrips"])
                });

                if (tripStats.Count > 0)
                {
                    TotalTrips = tripStats[0].TotalTrips;
                    ActiveTrips = tripStats[0].ActiveTrips;
                    UpcomingTrips = tripStats[0].UpcomingTrips;
                    CompletedTrips = tripStats[0].CompletedTrips;
                }

                OnPropertyChanged(nameof(TotalUsers));
                OnPropertyChanged(nameof(AdminUsers));
                OnPropertyChanged(nameof(RegularUsers));
                OnPropertyChanged(nameof(NewUsers));
                OnPropertyChanged(nameof(TotalTrips));
                OnPropertyChanged(nameof(ActiveTrips));
                OnPropertyChanged(nameof(UpcomingTrips));
                OnPropertyChanged(nameof(CompletedTrips));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load statistics: " + ex.Message, "OK");
            }
        }

        private async void OnExportUsersClicked(object sender, EventArgs e)
        {
            try
            {
                string query = "SELECT UserID, UserName, FullName, Email, Role, DateRegistered FROM Users";
                var users = await _dbHelper.getFromDatabase(query, reader => new UserReportItem
                {
                    UserID = reader["UserID"].ToString(),
                    UserName = reader["UserName"].ToString(),
                    FullName = reader["FullName"].ToString(),
                    Email = reader["Email"].ToString(),
                    Role = reader["Role"].ToString(),
                    DateRegistered = Convert.ToDateTime(reader["DateRegistered"]).ToString("yyyy-MM-dd")
                });

                string textContent = GeneratePlainTextContentForUsers(users);
                await DisplayTextInPopup(textContent, "Users Report");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to export users: " + ex.Message, "OK");
            }
        }

        private async void OnExportTripsClicked(object sender, EventArgs e)
        {
            try
            {
                string query = "SELECT TripID, TripName, Location, Description, StartDate, EndDate, UserEmail FROM Trips";
                var trips = await _dbHelper.getFromDatabase(query, reader => new TripReportItem
                {
                    TripID = reader["TripID"].ToString(),
                    TripName = reader["TripName"].ToString(),
                    Location = reader["Location"].ToString(),
                    Description = reader["Description"].ToString(),
                    StartDate = Convert.ToDateTime(reader["StartDate"]).ToString("yyyy-MM-dd"),
                    EndDate = Convert.ToDateTime(reader["EndDate"]).ToString("yyyy-MM-dd"),
                    Organizer = reader["UserEmail"].ToString()
                });

                string textContent = GeneratePlainTextContentForTrips(trips);
                await DisplayTextInPopup(textContent, "Trips Report");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to export trips: " + ex.Message, "OK");
            }
        }

        private string GeneratePlainTextContentForUsers(List<UserReportItem> users)
        {
            StringBuilder sb = new StringBuilder();
            // Header Row
            sb.AppendLine(string.Format("{0,-10} {1,-20} {2,-30} {3,-30} {4,-10} {5,-12}",
                "User ID", "Username", "Full Name", "Email", "Role", "Date Registered"));
            sb.AppendLine(new string('-', 120)); // Separator Line

            foreach (var user in users)
            {
                sb.AppendLine(string.Format("{0,-10} {1,-20} {2,-30} {3,-30} {4,-10} {5,-12}",
                    user.UserID, user.UserName, user.FullName, user.Email, user.Role, user.DateRegistered));
            }
            sb.AppendLine(new string('-', 120));
            sb.AppendLine($"Total Users: {users.Count}");
            return sb.ToString();
        }

        private string GeneratePlainTextContentForTrips(List<TripReportItem> trips)
        {
            StringBuilder sb = new StringBuilder();
            // Header Row
            sb.AppendLine(string.Format("{0,-10} {1,-20} {2,-30} {3,-40} {4,-12} {5,-12} {6,-10}",
                "Trip ID", "Trip Name", "Location", "Description", "Start Date", "End Date", "Organizer"));
            sb.AppendLine(new string('-', 150)); // Separator Line

            foreach (var trip in trips)
            {
                sb.AppendLine(string.Format("{0,-10} {1,-20} {2,-30} {3,-40} {4,-12} {5,-12} {6,-10}",
                    trip.TripID, trip.TripName, trip.Location, trip.Description, trip.StartDate, trip.EndDate, trip.Organizer));
            }
            sb.AppendLine(new string('-', 150));
            sb.AppendLine($"Total Trips: {trips.Count}");
            return sb.ToString();
        }

        private async Task DisplayTextInPopup(string text, string title)
        {
            var displayPopup = new PopupPage
            {
                BackgroundColor = Color.White,
                Opacity = 1,
                Padding = new Thickness(10)
            };

            var scrollView = new ScrollView();
            var label = new Label
            {
                Text = text,
                Padding = new Thickness(0),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                FontFamily = Device.RuntimePlatform == Device.Android ? "monospace" : null,
                TextColor = Color.Black,
                FontSize = 14,
                LineHeight = 1.5
            };

            scrollView.Content = label;

            var layout = new StackLayout();
            layout.Children.Add(scrollView);

            displayPopup.Content = layout;

            await PopupNavigation.Instance.PushAsync(displayPopup);
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

    //  Model Classes
    public class UserReportItem
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string DateRegistered { get; set; }
    }

    public class TripReportItem
    {
        public string TripID { get; set; }
        public string TripName { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Organizer { get; set; }
    }
}
