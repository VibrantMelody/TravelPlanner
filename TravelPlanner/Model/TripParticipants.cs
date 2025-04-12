using System;
using System.Collections.Generic;
using System.Text;

namespace TravelPlanner.Model
{
    public class TripParticipants
    {
        public int TripParticipantID { get; set; }
        public int TripID { get; set; }
        public int UserID { get; set; }
        public string ParticipantStatus { get; set; }
        public Boolean IsOrganizer { get; set; }
    }
}
