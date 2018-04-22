using AirFlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Net;

namespace AirFlight.Filters
{
    //Статистика посещений
    public class StatisticAttribute : ActionFilterAttribute
    {    //переопределяем метод вызова
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;

            Visit Visits = new Visit()
            {
                VisitData = DateTime.Now,
                IP = request.UserHostAddress,              
           // HostName = Dns.GetHostEntry(request.UserHostAddress).HostName,
            //UserAgent = request.UserAgent, //временно убрал сбор статистики об агенте пользователя
            UserBrowser = request.Browser.Browser,
        };

            try
            {
                Visits.HostName = Dns.GetHostEntry(Visits.IP).HostName;
                }
            catch
            {
                Visits.HostName = null;
            }

            using (AirFlightContext db = new AirFlightContext())
            {
                db.Visits.Add(Visits);
                db.SaveChanges();
            }

            base.OnActionExecuting(filterContext);            
        }       
    }
}