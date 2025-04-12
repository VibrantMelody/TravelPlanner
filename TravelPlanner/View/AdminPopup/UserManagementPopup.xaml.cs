using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TravelPlanner.Model;
using TravelPlanner.View.Popups;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;

namespace TravelPlanner.View.AdminPopup
{
    public partial class UserManagementPopup : PopupPage
    {
        private readonly DatabaseHelper _dbHelper;
        private ObservableCollection<TripUsers> _allUsers;
        private ObservableCollection<SelectableUser> _selectableUsers;

        public UserManagementPopup()
        {
            InitializeComponent();
            HasKeyboardOffset = false;
            _dbHelper = new DatabaseHelper();
            _allUsers = new ObservableCollection<TripUsers>();
            _selectableUsers = new ObservableCollection<SelectableUser>();
            RoleFilter.SelectedIndex = 0;
            LoadUsers();
        }

        private async void LoadUsers()
        {
            try
            {
                string query = "SELECT * FROM Users ORDER BY FullName";
                var users = await _dbHelper.getFromDatabase(query, reader => new TripUsers
                {
                    UserID = Convert.ToInt32(reader["UserID"]),
                    UserName = reader["UserName"].ToString(),
                    FullName = reader["FullName"].ToString(),
                    Email = reader["Email"].ToString(),
                    ProfilePicture = reader["ProfilePicture"].ToString(),
                    Role = reader["Role"].ToString(),
                    DateRegistered = Convert.ToDateTime(reader["DateRegistered"]),
                    Password = reader["Password"].ToString()
                });

                _allUsers = new ObservableCollection<TripUsers>(users);
                _selectableUsers.Clear();
                foreach (var user in _allUsers)
                {
                    _selectableUsers.Add(new SelectableUser { User = user });
                }
                UsersCollection.ItemsSource = _selectableUsers;
                FilterUsers(); 
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load users: " + ex.Message, "OK");
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            FilterUsers();
        }

        private void OnRoleFilterChanged(object sender, EventArgs e)
        {
            FilterUsers();
        }

        private void FilterUsers()
        {
            var searchText = SearchBar.Text?.ToLowerInvariant() ?? "";
            var roleFilter = RoleFilter.SelectedItem?.ToString();

            var filteredUsers = _selectableUsers.Where(su =>
                (string.IsNullOrEmpty(searchText) ||
                 su.User.FullName.ToLowerInvariant().Contains(searchText) ||
                 su.User.Email.ToLowerInvariant().Contains(searchText) ||
                 su.User.UserName.ToLowerInvariant().Contains(searchText)) &&
                (roleFilter == "All" || su.User.Role == roleFilter)).ToList(); 

            _selectableUsers.Clear();
            foreach(var user in filteredUsers)
            {
                 _selectableUsers.Add(user);
            }
        }

        private async void OnAddUserClicked(object sender, EventArgs e)
        {
            MessagingCenter.Subscribe<AddUserPopupPage>(this, AddUserPopupPage.UserAddedMessage,
                async (popup) =>
                {
                    LoadUsers();
                    MessagingCenter.Unsubscribe<AddUserPopupPage>(this, AddUserPopupPage.UserAddedMessage);
                });

            await PopupNavigation.Instance.PushAsync(new AddUserPopupPage());
        }

        private async void OnBulkDeleteClicked(object sender, EventArgs e)
        {
            var selectedUsers = _selectableUsers.Where(u => u.IsSelected).Select(u => u.User).ToList();

            if (selectedUsers.Count == 0)
            {
                await DisplayAlert("Warning", "Please select users to delete.", "OK");
                return;
            }

            bool confirm = await DisplayAlert("Confirm Delete",
                $"Are you sure you want to delete {selectedUsers.Count} users?",
                "Delete", "Cancel");

            if (confirm)
            {
                try
                {
                    var deletedCount = 0;
                    foreach (var user in selectedUsers.ToList())
                    {
                        bool success = await _dbHelper.DeleteSingleRecordFromDatabase("Users", "UserID", user.UserID);
                        if (success)
                        {
                            var userToRemove = _selectableUsers.FirstOrDefault(su => su.User.UserID == user.UserID);
                            if (userToRemove != null)
                            {
                                _selectableUsers.Remove(userToRemove);
                            }
                            _allUsers.Remove(_allUsers.First(u => u.UserID == user.UserID));
                            deletedCount++;
                        }
                        else
                        {
                            await DisplayAlert("Error", $"Failed to delete user: {user.FullName}", "OK");
                        }
                    }

                    await DisplayAlert("Success", $"{deletedCount} user(s) deleted successfully.", "OK");
                    LoadUsers(); 
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", "Failed to delete users: " + ex.Message, "OK");
                }
            }
        }

        private async void OnExportClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Export", "Export functionality to be implemented", "OK");
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }

        private async void EditUserClicked(object sender, EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            var selectableUser = (swipeItem?.BindingContext as SelectableUser);
            var user = selectableUser?.User;
            if (user != null)
            {
                MessagingCenter.Subscribe<EditUserPopup, int>(this, EditUserPopup.UserUpdatedMessage,
                    async (popup, userId) =>
                    {
                        LoadUsers();
                        MessagingCenter.Unsubscribe<EditUserPopup, int>(this, EditUserPopup.UserUpdatedMessage);
                    });

                await PopupNavigation.Instance.PushAsync(new EditUserPopup(user));
            }
        }

        private async void DeleteUserClicked(object sender, EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            var selectableUser = (swipeItem?.BindingContext as SelectableUser);
            var user = selectableUser?.User;
            if (user != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete",
                    $"Are you sure you want to delete user {user.FullName}?",
                    "Delete", "Cancel");

                if (confirm)
                {
                    try
                    {
                        bool success = await _dbHelper.DeleteSingleRecordFromDatabase("Users", "UserID", user.UserID);
                        if (success)
                        {
                            _selectableUsers.Remove(selectableUser);
                            _allUsers.Remove(user);
                            await DisplayAlert("Success", "User deleted successfully", "OK");
                            LoadUsers(); //reload
                        }
                        else
                        {
                            await DisplayAlert("Error", "Failed to delete user", "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", "Failed to delete user: " + ex.Message, "OK");
                    }
                }
            }
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }
    }

    public class SelectableUser
    {
        public TripUsers User { get; set; }
        public bool IsSelected { get; set; }
    }
}

