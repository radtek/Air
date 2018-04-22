using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using AirFlight.Models;
using System.Net;
using System.Web.UI;

namespace AirFlight.Filters
{   //логирование действий. Установлен глобальный фильтр данных LogAttribute
    public class LogAttribute : ActionFilterAttribute
    {    //переопределяем метод вызова
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var request = filterContext.HttpContext.Request;

            Log Logs = new Log()
            {
                VisitData = DateTime.Now,
                UserName = HttpContext.Current.User.Identity.IsAuthenticated? HttpContext.Current.User.Identity.GetUserName():null,
                IP = request.UserHostAddress,
               // HostName = Dns.GetHostEntry(request.UserHostAddress).HostName,
                UserBrowser = request.Browser.Browser,
                Url = request.RawUrl              
            };
            try
            {
                Logs.HostName = Dns.GetHostEntry(request.UserHostAddress).HostName;
            }
            catch
            {
                Logs.HostName = null;
            }
            using (AirFlightContext db = new AirFlightContext())
            {
                db.Logs.Add(Logs);
                db.SaveChanges();
            }

            base.OnActionExecuted(filterContext);
        }
           
    }   
   
}