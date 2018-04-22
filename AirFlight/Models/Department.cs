using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirFlight.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? CompanyId { get; set; }
        public Company Company { get; set; }
        public virtual ICollection<Site> Sites { get; set; }
        public Department()
        {
            Sites = new List<Site>();
        }
    }
}