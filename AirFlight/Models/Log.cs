using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirFlight.Models
{
    public class Log
    {
        public int Id { get; set; }
        public DateTime VisitData { get; set; }
        public string UserName { get; set; }
        public string IP { get; set; }
        public string HostName { get; set; }
        public string UserBrowser { get; set; }
        public string Url { get; set; }
    }
}