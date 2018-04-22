using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AirFlight.Models
{
    public class AspNetUser
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Surneme { get; set; }
        public string Login { get; set; }
        public string IdZam { get; set; }
        public string LoginId { get; set; }
        public string ZamID { get; set; }
        public string ZamLogin { get; set; }
        public string Signature { get; set; }
        public string EmailSend { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public AspNetUser()
        {
            Notifications = new List<Notification>();
        }

    }
}