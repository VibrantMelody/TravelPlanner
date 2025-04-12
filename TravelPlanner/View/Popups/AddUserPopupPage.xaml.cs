using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using TravelPlanner.Model;
using Xamarin.Forms;

namespace TravelPlanner.View.Popups
{
    public partial class AddUserPopupPage : PopupPage
    {
        private readonly DatabaseHelper _dbHelper;
        public const string UserAddedMessage = "UserAdded";

        public AddUserPopupPage()
        {
            InitializeComponent();
            HasKeyboardOffset = false;
            _dbHelper = new DatabaseHelper();
            RolePicker.SelectedIndex = 0;
        }

        private void OnImageUrlChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                ProfileImage.Source = e.NewTextValue;
            }
            else
            {
                ProfileImage.Source = "user_placeholder";
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(UserNameEntry.Text) ||
                    string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                    string.IsNullOrWhiteSpace(FullNameEntry.Text) ||
                    string.IsNullOrWhiteSpace(PasswordEntry.Text))
                {
                    await DisplayAlert("Required Fields", "Please fill in all required fields", "OK");
                    return;
                }

                if (!IsValidEmail(EmailEntry.Text))
                {
                    await DisplayAlert("Invalid Email", "Please enter a valid email address", "OK");
                    return;
                }

                string selectedRole = RolePicker.SelectedItem as string;
                // Add user to database
                int userId = await _dbHelper.AddUserAndGetId(
                    UserNameEntry.Text.Trim(),
                    FullNameEntry.Text.Trim(),
                    EmailEntry.Text.Trim(),
                    ImageEntry.Text?.Trim() ?? "user_placeholder",
                    DateTime.Now,
                    PasswordEntry.Text,
                    selectedRole
                );

                if (userId > 0)
                {
                    // Notify the UserManagementPopup that a user was added
                    MessagingCenter.Send(this, UserAddedMessage);
                    await DisplayAlert("Success", "User added successfully", "OK");
                    await PopupNavigation.Instance.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Failed to add user", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Confirm", 
                "Are you sure you want to cancel? Any unsaved changes will be lost.", 
                "Yes", "No");
            
            if (answer)
            {
                await PopupNavigation.Instance.PopAsync();
            }
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

        protected override bool OnBackgroundClicked()
        {
            return true; 
        }
    }
}
