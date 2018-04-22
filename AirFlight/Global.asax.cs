using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using QuartzApp.Jobs;   // пространство имен работы и триггера
using System;
using AirFlight.Models;
using static AirFlight.Models.ActiveSessions;
using System.Web;
using System.Net;

namespace AirFlight
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //Запускаем планировщик для отправки писем
            Scheduler.Start();          

        }
       //Считаем количество пользователей на сайте. При начале сессии записываем в базу данные о посетителе
        protected void Session_Start(object sender, EventArgs e)
        {
            Visit Visits = new Visit()
            {
                VisitData = DateTime.Now,
                IP = HttpContext.Current.Request.UserHostAddress,
               // HostName = Dns.GetHostEntry(HttpContext.Current.Request.UserHostAddress).HostName,
                //UserAgent = request.UserAgent, //временно убрал сбор статистики об агенте пользователя
                UserBrowser = HttpContext.Current.Request.Browser.Browser,
            };
            try
            {
                Visits.HostName = Dns.GetHostEntry(HttpContext.Current.Request.UserHostAddress).HostName;
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

            Sessions.Add(Session.SessionID);
        }

        protected void Session_End(object sender, EventArgs e)
        {            
           Sessions.Remove(Session.SessionID);
        }
        protected void Page_Load(object source, EventArgs e)
        {
           
        }

    }
}
