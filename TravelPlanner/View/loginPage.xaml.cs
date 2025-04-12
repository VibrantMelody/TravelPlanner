using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelPlanner.Model;
using TravelPlanner.ViewModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TravelPlanner.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        AuthViewModel authViewModel;
        public LoginPage()
        {
            InitializeComponent();
            authViewModel = new AuthViewModel();
        }

        async private void OnSignUpTapped(object sender, EventArgs e)
        {
            var SignUpPage = new View.SignUpPage();
            SignUpPage.Opacity = 0;
            Application.Current.MainPage = SignUpPage;
            await SignUpPage.FadeTo(1, 500, Easing.SinIn);
        }

        private async void SignInButtonClicked(object sender, EventArgs e)
        {
            try
            {
                string email = EmailEntry.Text;
                string password = PasswordEntry.Text;
                string role = (RBAdmin.IsChecked == true) ? "Admin" : "User";

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    throw new Exception("Please fill in the username and password and select role");
                }

                if (RBAdmin.IsChecked == false && RBUser.IsChecked == false)
                {
                    throw new Exception("Please select the role");
                }

                TripUsers verification = await authViewModel.UserVerification(email, password, role);

                if (verification != null)
                {
                    if (RBUser.IsChecked == true)
                    {
                        NavigationPage rootPage = new NavigationPage(new View.HomePage(verification));
                        rootPage.Opacity = 0;
                        Application.Current.MainPage = rootPage;
                        await rootPage.FadeTo(1, 500, Easing.SinIn);
                        return;
                    }
                    else
                    {
                        NavigationPage rootPage = new NavigationPage(new View.AdminPage());
                        Application.Current.MainPage = rootPage;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Sign In Error", ex.Message, "ok");
            }
        }

        private async void ResetPasswordClicked(object sender, EventArgs e) {
            var passwordResetPage = new View.PasswordResetPage();
            passwordResetPage.Opacity = 0;
            Application.Current.MainPage = passwordResetPage;
            await passwordResetPage.FadeTo(1, 500, Easing.SinIn);
        }
    }
}