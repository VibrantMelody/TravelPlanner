using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using TravelPlanner.Model;
using Xamarin.Forms;

namespace TravelPlanner.View
{
    public partial class AddTripsPopup : PopupPage
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly Trips _trip;
        public const string TripAddedMessage = "TripAdded";
        public const string TripUpdatedMessage = "TripUpdated";
        private string _organizer;

        public AddTripsPopup(string organizer)
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            HasKeyboardOffset = false;
            
            StartDateDatePicker.MinimumDate = DateTime.Now;
            EndDateDatePicker.MinimumDate = DateTime.Now;

            _organizer = organizer;

            PopupTitle.Text = "Add New Trip";
            SaveButton.Text = "Create Trip";
        }

        public AddTripsPopup(Trips trip, string organizer)
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _trip = trip;
            HasKeyboardOffset = false;

            StartDateDatePicker.MinimumDate = DateTime.Now;
            EndDateDatePicker.MinimumDate = DateTime.Now;

            TripNameEntry.Text = trip.TripName;
            TripDescriptionEntry.Text = trip.Description;
            TripLocationEntry.Text = trip.Location;
            StartDateDatePicker.Date = trip.StartDate;
            EndDateDatePicker.Date = trip.EndDate;
            ImageEntry.Text = trip.Image;

            _organizer = organizer;

            PopupTitle.Text = "Edit Trip";
            SaveButton.Text = "Update Trip";
        }

        private async void SaveButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TripNameEntry.Text) ||
                    string.IsNullOrWhiteSpace(TripLocationEntry.Text))
                {
                    await DisplayAlert("Required Fields", "Please fill in all required fields", "OK");
                    return;
                }

                if (EndDateDatePicker.Date < StartDateDatePicker.Date)
                {
                    await DisplayAlert("Invalid Dates", "End date must be after start date", "OK");
                    return;
                }

                TimeSpan difference = EndDateDatePicker.Date - StartDateDatePicker.Date;
                int totalDays = (int)difference.TotalDays;

                bool success;
                if (_trip == null)
                {
                    success = await _dbHelper.AddTriptoDatabase(
                        TripNameEntry.Text.Trim(),
                        TripDescriptionEntry.Text?.Trim() ?? "",
                        TripLocationEntry.Text.Trim(),
                        StartDateDatePicker.Date,
                        EndDateDatePicker.Date,
                        totalDays,
                        ImageEntry.Text?.Trim() ?? "default_trip_image",
                        4,
                        _organizer
                    );

                    if (success)
                    {
                        MessagingCenter.Send(this, TripAddedMessage);
                        await DisplayAlert("Success", "Trip created successfully", "OK");
                        await PopupNavigation.Instance.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to create trip", "OK");
                    }
                }
                else // Updating existing trip
                {
                    var updateParams = new Dictionary<string, object>
                    {
                        { "TripName", TripNameEntry.Text.Trim() },
                        { "Description", TripDescriptionEntry.Text?.Trim() ?? "" },
                        { "Location", TripLocationEntry.Text.Trim() },
                        { "StartDate", StartDateDatePicker.Date },
                        { "EndDate", EndDateDatePicker.Date },
                        { "Image", ImageEntry.Text?.Trim() ?? _trip.Image }
                    };

                    int result = await _dbHelper.UpdateRecordInDatabase("Trips", "TripID", _trip.TripID.ToString(), updateParams);
                    if (result > 0)
                    {
                        MessagingCenter.Send(this, TripUpdatedMessage);
                        await DisplayAlert("Success", "Trip updated successfully", "OK");
                        await PopupNavigation.Instance.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to update trip", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void CancelButtonClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Confirm", 
                "Are you sure you want to cancel? Any unsaved changes will be lost.", 
                "Yes", "No");
            
            if (answer)
            {
                await PopupNavigation.Instance.PopAsync();
            }
        }

        private void OnImageUrlChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                TripImage.Source = e.NewTextValue;
            }
            else
            {
                TripImage.Source = "default_trip_image";
            }
        }

        protected override bool OnBackgroundClicked()
        {
            return false; // Prevent closing on background click
        }
    }
}
