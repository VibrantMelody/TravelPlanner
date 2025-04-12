using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Xamarin.Forms;

namespace TravelPlanner
{
    public class DatabaseHelper
    {
        private String connectionString;

        /// <summary>
        /// This is a helper class that helps with all the database related processes, its asyncronous and doesnt block the main UI Thread.
        /// </summary>
        public DatabaseHelper()
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

            //connectionString = "Data Source=storeandmobiledevelopment.database.windows.net;Initial Catalog=WanderWise;User ID=Jeol;Password=rememberme?910@;Connect Timeout=60;Encrypt=True";
        }
        
        public async Task<List<T>> getFromDatabase<T>(string query, Func<IDataReader, T> map)
        {
            List<T> dataList = new List<T>();
            string q = query;

            try
            {
                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    await sqlConnection.OpenAsync();

                    using (SqlCommand sqlCommand = new SqlCommand(q, sqlConnection))
                    {

                        using (SqlDataReader reader = await sqlCommand.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                T item = map(reader);
                                dataList.Add(item);
                            }
                        }
                    }
                }
                return dataList;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error Fetching Data", ex.Message, "OK");
                return dataList;
            }
        }

        public async Task<bool> uploadToDatabase(string query, Dictionary<string, object> parameters)
        {
            try
            {
                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    await sqlConnection.OpenAsync();
                    using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                sqlCommand.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }

                        int rowsAffected = await sqlCommand.ExecuteNonQueryAsync();

                        // Return true if at least one row was affected
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error Uploading Data", ex.Message, "OK");
                return false;
            }
        }

        public async Task<bool> DeleteSingleRecordFromDatabase(string tableName, string columnName, int value)
        {
            try
            {
                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    await sqlConnection.OpenAsync();
                    string query = $"DELETE FROM {tableName} WHERE {columnName} = @Id";
                    using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@Id", value);

                        int rowsAffected = await sqlCommand.ExecuteNonQueryAsync();

                        // Return true if at least one row was affected
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error Deleting Record", ex.Message, "OK");
                return false;
            }
        }

        public async Task<int> UpdateRecordInDatabase(string tableName, string columnName, String ID, Dictionary<string, object> updateParameters)
        {
            try
            {
                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    await sqlConnection.OpenAsync();

                    // Constructing the UPDATE query dynamically
                    string updateClause = string.Join(", ", updateParameters.Keys.Select(key => $"{key} = @{key}"));
                    string query = $"UPDATE {tableName} SET {updateClause} WHERE {columnName} = @Id";

                    using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                    {
                      
                        foreach (var param in updateParameters)
                        {
                            sqlCommand.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                        }

                        sqlCommand.Parameters.AddWithValue("@Id", ID);

                        int rowsAffected = await sqlCommand.ExecuteNonQueryAsync();

                        return rowsAffected;
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error Updating Record", ex.Message, "OK");
                return 0;
            }
        }

        public async Task<bool> DeleteAllFromDatabase(string tableName)
        {
            try
            {
                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    await sqlConnection.OpenAsync();
                    string query = $"DELETE FROM {tableName}";
                    using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                    {
                        int rowsAffected = await sqlCommand.ExecuteNonQueryAsync();

                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error Deleting Record", ex.Message, "OK");
                return false;
            }
        }
        public async Task<int> AddUserAndGetId(string userName, string fullName, string email, string profilePicture, DateTime dateRegistered, string password, string role)
        {
            try
            {
                string query = @"INSERT INTO Users (UserName, FullName, Email, ProfilePicture, DateRegistered, Password, Role) 
                        VALUES (@UserName, @FullName, @Email, @ProfilePicture, @DateRegistered, @Password, @Role);
                        SELECT SCOPE_IDENTITY();";

                var parameters = new Dictionary<string, object>
        {
            { "@UserName", userName },
            { "@FullName", fullName },
            { "@Email", email },
            { "@ProfilePicture", profilePicture },
            { "@DateRegistered", dateRegistered },
            { "@Password", password },
            { "@Role", role }
        };

                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    await sqlConnection.OpenAsync();
                    using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                    {
                        foreach (var param in parameters)
                        {
                            sqlCommand.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }

                        var result = await sqlCommand.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error Adding User", ex.Message, "OK");
                return -1;
            }
        }

        public async Task<bool> AddTripParticipant(int tripId, int userId, string participantStatus = "Invited", bool isOrganizer = false)
        {
            try
            {
                string query = @"INSERT INTO TripParticipants (TripID, UserID, ParticipantStatus, IsOrganizer) 
                        VALUES (@TripID, @UserID, @ParticipantStatus, @IsOrganizer)";

                var parameters = new Dictionary<string, object>
        {
            { "@TripID", tripId },
            { "@UserID", userId },
            { "@ParticipantStatus", participantStatus },
            { "@IsOrganizer", isOrganizer }
        };

                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    await sqlConnection.OpenAsync();
                    using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                    {
                        foreach (var param in parameters)
                        {
                            sqlCommand.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }

                        int rowsAffected = await sqlCommand.ExecuteNonQueryAsync();

                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error Adding Trip Participant", ex.Message, "OK");
                return false;
            }
        }

        public async Task<bool> AddTriptoDatabase(string tripName, string description, string location, DateTime startDate, DateTime endDate, int totalDays, string image, int categoryID, string organizer)
        {
            try
            {
                string query = @"INSERT INTO Trips (TripName, Description, Location, StartDate, EndDate, TotalDays, Image, CategoryID, UserEmail) 
                        VALUES (@TripName, @Description, @Location, @StartDate, @EndDate, @TotalDays, @Image, @CategoryID, @Organizer)";

                var parameters = new Dictionary<string, object>
        {
            { "@TripName", tripName },
            { "@Description", description },
            { "@Location", location },
            { "@StartDate", startDate },
            {"@EndDate", endDate },
            {"@TotalDays", totalDays },
            {"@Image", image },
            {"@CategoryID", categoryID },
            {"@Organizer", organizer }
        };

                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    await sqlConnection.OpenAsync();
                    using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                    {
                        foreach (var param in parameters)
                        {
                            sqlCommand.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }

                        int rowsAffected = await sqlCommand.ExecuteNonQueryAsync();

                        return (rowsAffected > 0 ) ?  true :  false;
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error Adding Trip Participant", ex.Message, "OK");
                return false;
            }
        }
    }

}
