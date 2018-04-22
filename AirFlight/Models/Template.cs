using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AirFlight.Models
{
    public class Template
    {
        public int Id { get; set; }
        public int Num { get; set; }   
        public TimeSpan FlightTime { get; set; }
        public int FlightNum { get; set; }
        public string Departure { get; set; }
        public string Arrival { get; set; }
        public string AirType { get; set; }
        public string Approved { get; set; }
        public int Number { get; set; } 
        public string UserId { get; set; }
    }

    public class NameTemplate
    {
        public int Id { get; set; }
        public int Num { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }       
    }


    public class TemplatesPassenger
    {
        public int Id { get; set; }
        public int Num { get; set; }
        public string UserId { get; set; }      
        public int PassengerID { get; set; }       
        public Passenger Passenger { get; set; }
    }


    public class TemplatesNamePassenger
    {
        public int Id { get; set; }  
        public int Num { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }       

    }



}