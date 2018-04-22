using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AirFlight.Models
{
    public class Signature
    {
        public int Id { get; set; }
        public string Sig { get; set; }
    }

    public class Notification
    {
        public int Id { get; set; }
        public string AspNetUserID { get; set; }
        public int? FlightID { get; set; }
        public AspNetUser AspNetUser { get; set; }
        public Flight Flight { get; set; }
    }

    public class PhoneBook
    {
        public int Id { get; set; }
        public string TelNum { get; set; } 
        public string Subscriber { get; set;}      
        public string Department { get; set; }       
        public string City { get; set;}      
    }  

    public class ClickStatistic
    {
        public int Id { get; set; }
        public DateTime ClickData { get; set; }
        public string UserName { get; set; }
        public string UserIp { get; set; }
        public string ResurseClick { get; set; }
        public string Location { get; set; }
    }
}