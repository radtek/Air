using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirFlight.Models
{
    public class AspNetRole
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        //public virtual ICollection<AspNetUserRole> AspNetUserRoles { get; set; }
        //public AspNetRole()
        //{
        //    AspNetUserRoles = new List<AspNetUserRole>();
        //}
    }
}