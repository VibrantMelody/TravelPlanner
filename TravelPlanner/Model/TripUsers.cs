using System;
using System.Collections.Generic;
using System.Text;

namespace TravelPlanner.Model
{
    public class TripUsers
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
        public string Password { get; set; }
        public string Role  { get; set; }
        public DateTime DateRegistered { get; set; }
    }
}
