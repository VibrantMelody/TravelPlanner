using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelPlanner.Model;
using TravelPlanner.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.CommunityToolkit.Extensions;
namespace TravelPlanner.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignUpPage : ContentPage
    {
        private AuthViewModel authViewModel;
        public SignUpPage()
        {
            InitializeComponent();
            authViewModel = new AuthViewModel();

        }

        async private void OnSignInTapped(object sender, EventArgs e)
        {
            var LoginPage = new View.LoginPage();
            LoginPage.Opacity = 0;
            Application.Current.MainPage = LoginPage;
            await LoginPage.FadeTo(1, 500, Easing.SinIn);
        }

        private async void SignUpButtonClicked(object sender, EventArgs e)
        {
            String fullName = FullNameEntry.Text;
            String userName = UserNameEntry.Text;
            String email = EmailEntry.Text;
            String password = PasswordEntry.Text;
            String role = (RBAdmin.IsChecked == true) ? "Admin" : "User";

            try
            {

                if (ValidateFields())
                {
                    await authViewModel.RegisterUser(fullName, userName, email, password, role);
                }

            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Input Validation Failed", ex.Message, "Ok");
            }
        }
        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
            {
                DisplayAlert("Error", "Full name is required.", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(UserNameEntry.Text))
            {
                DisplayAlert("Error", "User name is required.", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                DisplayAlert("Error", "Email is required.", "OK");
                return false;
            }

            if (!IsValidEmail(EmailEntry.Text))
            {
                DisplayAlert("Error", "Invalid email format.", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                DisplayAlert("Error", "Password is required.", "OK");
                return false;
            }

            if (PasswordEntry.Text.Length < 8)
            {
                DisplayAlert("Error", "Password must be at least 8 characters long.", "OK");
                return false;
            }

            if (PasswordEntry.Text != ReTypePasswordEntry.Text)
            {
                DisplayAlert("Error", "Passwords do not match.", "OK");
                return false;
            }

            if (!RBAdmin.IsChecked && !RBUser.IsChecked)
            {
                DisplayAlert("Error", "Please select a role.", "OK");
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}