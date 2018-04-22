using AirFlight.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace QuartzApp.Jobs
{
    public class ApprovedClose : IJob
    { //Автоматическое закрытие регистрации прошлых дат если они были открыты
        public AirFlightContext db = new AirFlightContext();
        public void Execute(IJobExecutionContext context)
        {
            DateTime last = DateTime.Now.Date.AddDays(-1);           
            var notarchive = db.Flights.Where(f=>f.Approved != "В архиве" && f.FlightDate <= last).ToList();
            var notclose = notarchive.Where(f => f.Approved.Trim() != "Закрыта").ToList();
            if (notclose.Count() > 0)
            {
                foreach (var c in notclose)
                {
                    c.Approved = "Закрыта";
                    db.Entry(c).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }
    }
}





   
        
       

       