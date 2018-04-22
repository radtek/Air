using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AirFlight.Models
{
    public class AirFlightContext : DbContext
    {
        public DbSet<Company> Companys { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<RegistrationList> RegistrationLists { get; set; }
        public DbSet<Residense> Residenses { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<AspNetRole> AspNetRoles { get; set; }
        public DbSet<AspNetUserRole> AspNetUserRoles { get; set; }
        public DbSet<AspNetUser> AspNetUsers { get; set; }
        public DbSet<New> News { get; set; }
        public DbSet<Visit> Visits { get; set; }   
        public DbSet<Log> Logs { get; set; }
        public DbSet<Signature> Signatures { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<NameTemplate> NameTemplates { get; set; }
        public DbSet <Notification> Notifications { get; set; }
        public DbSet<TemplatesPassenger> TemplatesPassengers { get; set; }
        public DbSet<TemplatesNamePassenger> TemplatesNamePassengers { get; set; }   
        public DbSet<PhoneBook> PhoneBooks { get; set; }
        public DbSet<ClickStatistic> ClickStatistics { get; set; }
    }


}