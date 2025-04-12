using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelPlanner.ViewModel;
using TravelPlanner.Model;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;
using Xamarin.CommunityToolkit.Extensions;

namespace TravelPlanner.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        HomePageViewModel _homePageViewModel;
        public HomePage(TripUsers currentUser)
        {
            InitializeComponent();
            _homePageViewModel = new HomePageViewModel(currentUser);
            BindingContext = _homePageViewModel;
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private async void LogoutClicked(object sender, EventArgs e)
        {
            var SignInPage = new View.LoginPage();
            SignInPage.Opacity = 0;
            Application.Current.MainPage = SignInPage;
            await SignInPage.FadeTo(1, 500, Easing.SinIn);
        }


        private void FrameCardTapped(object sender, EventArgs e)
        {
            var frame = sender as Frame;
            if (frame?.BindingContext is Trips selectedTrip)
            {
                Navigation.PushAsync(new TripsDetailPage(selectedTrip, _homePageViewModel));
            }
        }

        private async void EditProfileClicked(object sender, EventArgs e)
        {
            MessagingCenter.Subscribe<EditUserPopup, int>(this, EditUserPopup.UserUpdatedMessage,
                async (popup, userId) =>
                {
                    await _homePageViewModel.LoadUser();
                    MessagingCenter.Unsubscribe<EditUserPopup, int>(this, EditUserPopup.UserUpdatedMessage);
                });

            await PopupNavigation.Instance.PushAsync(new EditUserPopup(_homePageViewModel.CurrentUser));
        }

        private async void CreateTripClicked(object sender, EventArgs e)
        {
            MessagingCenter.Subscribe<AddTripsPopup>(this, AddTripsPopup.TripAddedMessage,
                async (popup) =>
                {
                    await _homePageViewModel.LoadTrips();
                    MessagingCenter.Unsubscribe<AddTripsPopup>(this, AddTripsPopup.TripAddedMessage);
                });


            await PopupNavigation.Instance.PushAsync(new AddTripsPopup(_homePageViewModel.CurrentUser.Email));
        }

        private async void RefreshTripsClicked(object sender, EventArgs e)
        {
            await _homePageViewModel.LoadTrips();
            await this.DisplayToastAsync("Refreshing UI with new daata");
        }
    }
}