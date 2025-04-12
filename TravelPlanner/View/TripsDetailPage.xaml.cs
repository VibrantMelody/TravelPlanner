using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelPlanner.Model;
using TravelPlanner.ViewModel;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.CommunityToolkit.UI.Views.Options;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TravelPlanner.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TripsDetailPage : ContentPage
    {
        Trips _trip;
        TripUsers _currentUser;
        List<TripParticipants> participant;
        TripDetailViewModel _viewModel;
        HomePageViewModel _homePageViewModel;
        DatabaseHelper helper;
        public TripsDetailPage(Trips trip, HomePageViewModel homePageViewModel)
        {
            InitializeComponent();
            _trip = trip;
            _currentUser = homePageViewModel.CurrentUser;
            _homePageViewModel = homePageViewModel;
            _viewModel = new TripDetailViewModel(_trip);
            BindingContext = _viewModel;
            helper = new DatabaseHelper();
            _ = CheckUserJoined();

            if(_trip.Organizer != _currentUser.Email)
            {
                DeleteTripButton.IsVisible = false;
                EditTripButton.IsVisible = false;
            }
        }

        private void DeleteTripButton_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private async Task CheckUserJoined()
        {
            string query = $"select * from TripParticipants where TripID = {_trip.TripID} and UserID = {_currentUser.UserID}";
             participant= await helper.getFromDatabase(query, reader => new TripParticipants
            {
                TripParticipantID = Convert.ToInt32(reader["TripParticipantID"]),
                TripID = Convert.ToInt32(reader["TripID"]),
                UserID = Convert.ToInt32(reader["UserID"]),
                ParticipantStatus = reader["ParticipantStatus"].ToString(),
                IsOrganizer = Convert.ToBoolean(reader["IsOrganizer"])

            });
            Device.BeginInvokeOnMainThread(() =>
            {
                JoinTripButton.Clicked -= JoinTripClicked;
                JoinTripButton.Clicked -= LeaveTripClicked;

                if (participant.Count > 0)
                {
                    JoinTripButton.Text = "Leave Trip";
                    JoinTripButton.Clicked += LeaveTripClicked;
                } else
                {
                    JoinTripButton.Text = "Join Trip";
                    JoinTripButton.Clicked += JoinTripClicked;
                }
            });
        }

        private async void JoinTripClicked(object sender, EventArgs e)
        {
            Boolean isSuccess = await helper.AddTripParticipant(_trip.TripID, _currentUser.UserID, "Confirmed", false);
            if (isSuccess)
            {
                await _viewModel.RefreshUsersList();
                await this.CheckUserJoined();
                await this.DisplayToastAsync("Successfully Joined");

            }
            else
            {
                await this.DisplayToastAsync("Something wrong happened");
            }
        }

        private async void LeaveTripClicked(object sender, EventArgs e)
        {
            Boolean isSuccess = await helper.DeleteSingleRecordFromDatabase("TripParticipants", "TripParticipantID", participant[0].TripParticipantID);
            if (isSuccess)
            {
                await _viewModel.RefreshUsersList();
                await this.CheckUserJoined();
                await this.DisplayToastAsync("Successfully Left");

            }
            else
            {
                await this.DisplayToastAsync("Something wrong happened");
            }
        }

        private async void ShowUserInfo(object sender, EventArgs e)
        {
            var label = sender as StackLayout;
            if (label?.BindingContext is TripUsers user)
            {
                await PopupNavigation.Instance.PushAsync(new View.Popups.UserInfoPopup(user));
            }
        }

        private async void DeleteTripClicked(object sender, EventArgs e)
        {
            Boolean isSuccess =  await helper.DeleteSingleRecordFromDatabase("Trips", "TripID", _trip.TripID);
            if(isSuccess)
            {
                await _homePageViewModel.LoadTrips();
                await this.DisplayToastAsync("Successfully Deleted the trip");
            } else
            {
                await this.DisplayToastAsync("Successfully Deleted the trip");
            }

        }

        private async void EditTripClicked(object sender, EventArgs e)
        {
            MessagingCenter.Subscribe<AddTripsPopup>(this, AddTripsPopup.TripUpdatedMessage,
                async (popup) =>
                {
                    await this.DisplayToastAsync("Updating trip details");
                    await  _viewModel.UpdateTrip();
                    await _homePageViewModel.LoadTrips();
                    MessagingCenter.Unsubscribe<AddTripsPopup>(this, AddTripsPopup.TripUpdatedMessage);
                });

            await PopupNavigation.Instance.PushAsync(new AddTripsPopup(_trip, _currentUser.Email)); 

        }
    }
}