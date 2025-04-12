using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TravelPlanner.Model;
using Xamarin.Forms;

namespace TravelPlanner.View
{
    public partial class EditUserPopup : PopupPage
    {
        private readonly DatabaseHelper dbhelper;
        private readonly TripUsers _currentUser;
        public const string UserUpdatedMessage = "UserUpdated";

        public EditUserPopup(TripUsers currentUser)
        {
            InitializeComponent();
            HasKeyboardOffset = false;
            dbhelper = new DatabaseHelper();
            _currentUser = currentUser;
            BindingContext = _currentUser;
        }

        private async void CancelButtonClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FullNameEntry.Text) || 
                    string.IsNullOrWhiteSpace(UserNameEntry.Text) || 
                    string.IsNullOrWhiteSpace(EmailEntry.Text))
                {
                    await DisplayAlert("Validation Error", "Please fill in all required fields", "OK");
                    return;
                }

                if (!IsValidEmail(EmailEntry.Text))
                {
                    await DisplayAlert("Validation Error", "Please enter a valid email address", "OK");
                    return;
                }

                Dictionary<string, object> updateParameters = new Dictionary<string, object>
                {
                    { "UserName", UserNameEntry.Text },
                    { "FullName", FullNameEntry.Text },
                    { "Email", EmailEntry.Text },
                    { "ProfilePicture", ImageEntry.Text ?? "" }
                };

                if (!string.IsNullOrWhiteSpace(PasswordEntry.Text))
                {
                    updateParameters.Add("Password", PasswordEntry.Text);
                }

                int result = await dbhelper.UpdateRecordInDatabase("Users", "UserID", _currentUser.UserID.ToString(), updateParameters);

                if (result > 0)
                {
                    MessagingCenter.Send(this, UserUpdatedMessage, _currentUser.UserID);
                    await PopupNavigation.Instance.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Failed to update user information", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        protected override bool OnBackgroundClicked()
        {
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
