using System;
using System.Collections.Generic;
using System.Text;

namespace TravelPlanner.Model
{
    public class UserRegistration
        {
            public int ID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
        }
}
