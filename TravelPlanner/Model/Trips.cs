using System;
using Xamarin.Forms;

namespace TravelPlanner.Model
{
    public class Trips : BindableObject
    {
        public int TripID { get; set; }
        public string TripName { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Image {  get; set; }
        public string Organizer {  get; set; }
    }
}
