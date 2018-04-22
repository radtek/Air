using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirFlight.Models
{
    public class Passenger
    {
        public int id { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Middlename { get; set; }
        public DateTime? Date_Birth { get; set; }
        public string Passport { get; set; }
        public string EmployeeNum { get; set; }
        public string PhoneNum { get; set; }
        public bool SendSms { get; set; }
        public string Residence { get; set; }
        public string Email { get; set; }
        public bool SendEmail { get; set; }
        public string Email2 { get; set; }       
        public bool SendEmail2 { get; set; }
        public string Department { get; set; }
        public string Site { get; set; }
        public string Company { get; set; }
        public string Post { get; set; }
        public int? ShiftNum { get; set; }

        public virtual ICollection<RegistrationList> RegistrationLists { get; set; }
        public virtual ICollection<TemplatesPassenger> TemplatesPassengers { get; set; }
        public Passenger()
        {
            RegistrationLists = new List<RegistrationList>();
            TemplatesPassengers = new List<TemplatesPassenger>();
        }
    }
}