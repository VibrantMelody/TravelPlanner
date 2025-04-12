using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;
using TravelPlanner.Model;
using System.Collections.Generic;

namespace TravelPlanner.ViewModel
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper helper;
        private ObservableCollection<Trips> _trips;
        private TripUsers _currentUser;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Trips> TripsList
        {
            get => _trips;
            set
            {
                if (_trips != value)
                {
                    _trips = value;
                    OnPropertyChanged();
                }
            }
        }

        public TripUsers CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged();
                }
            }
        }

        public HomePageViewModel(TripUsers currentUser)
        {
            helper = new DatabaseHelper();
             _trips= new ObservableCollection<Trips>();
            _currentUser = currentUser;
            _ = LoadTrips();
            _ = LoadUser();
        }

        public async Task LoadUser()
        {
            string currentUserQuery = $"Select * from Users where Email = '{CurrentUser.Email}'";


            await Task.Run(async () =>
            {
                List<TripUsers> usersList = await helper.getFromDatabase(currentUserQuery, reader => new TripUsers
                {
                    UserID = Convert.ToInt32(reader["UserID"]),
                    FullName = reader["FullName"].ToString(),
                    UserName = reader["UserName"].ToString(),
                    Email = reader["Email"].ToString(),
                    ProfilePicture = reader["ProfilePicture"].ToString(),
                    Password = reader["Password"].ToString(),
                    DateRegistered = Convert.ToDateTime(reader["DateRegistered"])
                });

                Device.BeginInvokeOnMainThread(() =>
                {
                CurrentUser = usersList[0];
                });
            });
        }

        public async Task LoadTrips()
        {
            string query = "select * from Trips order by TripName asc;";

            await Task.Run(async () =>
            {
                var tripsFromDb = await helper.getFromDatabase(query, reader => new Trips
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

                Device.BeginInvokeOnMainThread(() =>
                {
                    TripsList.Clear();
                    foreach (var trip in tripsFromDb)
                    { TripsList.Add(trip); }
                });
            });
        }


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}