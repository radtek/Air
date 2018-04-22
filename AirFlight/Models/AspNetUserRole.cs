using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AirFlight.Models
{
    public class AspNetUserRole
    {
        [Key]
        public string  UserId  { get; set; }    
        public string RoleId { get; set; }

        //public AspNetRole AspNetRole { get; set; }
        //public AspNetUser AspNetUser { get; set; }


    }
}