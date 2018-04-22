using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirFlight.Models
{
    public class Site
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? DepartmentId { get; set; }
        public int? CompanyId { get; set; }
        public Department Department { get; set; }
        public Company Company { get; set; }

    }
}