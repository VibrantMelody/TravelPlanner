using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelPlanner.Model;
using TravelPlanner.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using TravelPlanner.View.AdminPopup;
using Xamarin.CommunityToolkit.Extensions;

namespace TravelPlanner.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdminPage : ContentPage
    {
        public AdminPage()
        {
            InitializeComponent();
        }
        private async void LogoutClicked(object sender, EventArgs e)
        {
            var SignInPage = new View.LoginPage();
            SignInPage.Opacity = 0;
            Application.Current.MainPage = SignInPage;
            await SignInPage.FadeTo(1, 500, Easing.SinIn);
        }

        private async void UsersClicked(object sender, EventArgs e)
        {

               await PopupNavigation.Instance.PushAsync(new AdminPopup.UserManagementPopup());
        }

        private async void TripsClicked(object sender, EventArgs e)
        {
               await PopupNavigation.Instance.PushAsync(new AdminPopup.TripManagementPopup());

        }

        private async void StatisticsClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new AdminPopup.StatisticsPopupPage());
        }
    }
}
