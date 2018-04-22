using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirFlight.Models
{
    public class Flight
    {
        public int id { get; set; }
        public DateTime FlightDate { get; set; }
        public TimeSpan FlightTime { get; set; }
        public short FlightNum { get; set; }
        public string Departure { get; set; }
        public string Arrival { get; set; }
        public string AirType { get; set; }
        public string Approved { get; set; }
        public int Number { get; set; }
        public string Note { get; set; }
        public bool SendingEmail { get; set; }
        public bool? SendingSms { get; set; }       
        public bool HaveChange { get; set; }
        public TimeSpan? FlightTimeOld { get; set; }
        public string AirTypeOld { get; set; }
        public string NoteOld { get; set; } 
        public string DocsName { get; set; }

        public virtual ICollection<RegistrationList> RegistrationLists { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }

        public Flight()
        {
            RegistrationLists = new List<RegistrationList>();
            Notifications = new List<Notification>();
        }
     
    }
   
}