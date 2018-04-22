using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirFlight.Models
{
    public class RegistrationList
    {

        public int id { get; set; }
        public string CreatorId { get; set; }
        public string EditorId { get; set; }
        public bool NightShift { get; set; }
        public string Note { get; set; }
        public bool SendingEmail { get; set; }       
        public bool SendingSms { get; set; }          
        public bool ChangeNum { get; set; }   
        public bool HaveChange { get; set; }
        public bool BusSms { get; set; }
        public int? FlightID { get; set; }
        public int? PassengerID { get; set; }

        public Flight Flight { get; set; }
        public Passenger Passenger { get; set; }
    }
}