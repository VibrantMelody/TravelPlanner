using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TravelPlanner.Model;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel;
using Xamarin.Forms;
using System.Diagnostics;
using Xamarin.Essentials;

namespace TravelPlanner.ViewModel
{
    public class AuthViewModel
    {
        private string connectionString;
        DatabaseHelper helper;

        public AuthViewModel()
        {
            connectionString = new SqlConnectionStringBuilder
            {
                DataSource = "storeandmobiledevelopment.database.windows.net",
                InitialCatalog = "WanderWise",
                UserID = "Jeol",
                Password = "rememberme?910@",
                // Important security settings for Azure SQL
                Encrypt = true,
                TrustServerCertificate = false,
                ConnectTimeout = 30
            }.ConnectionString;

            helper = new DatabaseHelper();
        }

        public async Task RegisterUser(string fullName, string userName, string email, string password, string role)
        {
            if (role == "Admin")
            {
                List<TripUsers> retrievedUserList = await RetrieveUser($"Role = '{role}'");
                if (retrievedUserList.Count > 0)
                {
                    throw new Exception("Admin Already Exists, Please Log In Instead");
                }
            }


            try
            {
                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    await sqlConnection.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Users (FullName, UserName, Email, Password, Role) VALUES (@FullName, @UserName, @Email, @Password, @Role)", sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@FullName", fullName);
                        cmd.Parameters.AddWithValue("@UserName", userName);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@Role", role);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                await App.Current.MainPage.DisplayAlert("Registration Successful", $"User {fullName} has been successfully registered with the role of {role}", "Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex}");
                await App.Current.MainPage.DisplayAlert("Registration Error", ex.Message, "OK");
            }
        }

        public async Task<List<TripUsers>> RetrieveUser(string clause)
        {
            List<TripUsers> userList = new List<TripUsers>();

            try
            {
                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    await sqlConnection.OpenAsync();
                    string query = $"SELECT UserID, FullName, UserName, Email, Password, Role, ProfilePicture FROM Users WHERE {clause}";

                    using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                    {
                        using (SqlDataReader reader = await sqlCommand.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                userList.Add(new TripUsers
                                {
                                    UserID = Convert.ToInt32(reader["UserID"]),
                                    FullName = reader["FullName"].ToString(),
                                    UserName = reader["UserName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Password = reader["Password"].ToString(),
                                    Role = reader["Role"].ToString(),
                                    ProfilePicture = (reader["ProfilePicture"] != null) ? reader["ProfilePicture"].ToString() : "ProfilePicture",
                                });
                            }
                        }
                    }
                }

                return userList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user: {ex}");
                await App.Current.MainPage.DisplayAlert("Retrieval Error", ex.Message, "OK");
                return null;
            }
        }

        async public Task<TripUsers> UserVerification(string email, string password, string role)
        {
            List<TripUsers> retrievedList = await RetrieveUser($"Email = '{email}'");
            try
            {
                if (retrievedList.Count <= 0)
                {
                    throw new Exception("User not found.");
                }

                if (retrievedList[0].Password != password)
                {
                    throw new Exception("Passwords didn't match");
                }
                if (retrievedList[0].Role != role)
                {
                    throw new Exception("Role didn't match");
                }

                return retrievedList[0];

            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error Verifying User", ex.Message, "OK");
                return null;
            }
        }
    }
}