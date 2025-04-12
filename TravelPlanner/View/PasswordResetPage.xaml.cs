using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class PasswordResetPage : ContentPage
    {
        DatabaseHelper helper;
        public PasswordResetPage()
        {
            InitializeComponent();
            helper = new DatabaseHelper();
        }

        private async void ResetButtonClicked(object sender, EventArgs e)
        {
            String email = EmailEntry.Text;
            String newPassword = NewPasswordEntry.Text;
            String confirmPassword = ConfirmPasswordEntry.Text;
            String securityAnswer = SecurityAnswer.Text;

            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(newPassword) || String.IsNullOrEmpty(confirmPassword))
            {
                await DisplayAlert("Missing Fields", "Please fill in all the fields", "ok");
                Console.WriteLine("Missing Fields");
                return;
            }

            if (confirmPassword != newPassword)
            {
                await DisplayAlert("Match didn't match", "Youre two password didn't match", "ok");
                Console.WriteLine("password's didn't match");
                return;
            }

            String query = $"SELECT FullName, UserName, Email, Password, Role FROM dbo.Users WHERE Email = '{email}'";

            var usersList = await helper.getFromDatabase(query, reader => new TripUsers
            {
                FullName = reader["FullName"].ToString(),
                UserName = reader["Username"].ToString(),
                Email = reader["Email"].ToString(),
                Password = reader["Password"].ToString(),
                Role = reader["Role"].ToString(),
            });

            if (usersList.Count == 0)
            {
                await DisplayAlert("Not Found", "User with this email not found.", "Ok");
                Console.WriteLine("User with this email not found.");
                return;
            }

            TripUsers user = usersList[0];
            if (String.Equals(user.UserName, securityAnswer, StringComparison.OrdinalIgnoreCase)) 
            {
                Dictionary<string, object> updateParameters = new Dictionary<string, object>
                    {
                        { "Password", newPassword }
                    };

                int rowsAffected = await helper.UpdateRecordInDatabase("Users", "Email", email, updateParameters);

                if (rowsAffected > 0)
                {
                    await DisplayAlert("Password Reset", "Your password has been reset successfully.", "OK");
                 
                }
                else
                {
                    await DisplayAlert("Password Reset Failed", "Failed to update the password.", "OK");
                }
            }
            

        }

        private async void BackButtonClicked(object sender, EventArgs e)
        {
            var LoginPage = new View.LoginPage();
            LoginPage.Opacity = 0;
            Application.Current.MainPage = LoginPage;
            await LoginPage.FadeTo(1, 500, Easing.SinIn);
        }
    }
}