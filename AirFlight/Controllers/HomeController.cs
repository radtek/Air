using AirFlight.Filters;
using AirFlight.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AirFlight.Controllers
{
    public class HomeController : Controller
    {
        private AirFlightContext db = new AirFlightContext();
        private KiasuContext kiasu = new KiasuContext();
        // [Statistic] - применить фильтр статистики только на выбранный контроллер. Так как пользователь может несколько 
        //раз заходиьт на страницу, то мы определяем счётчик по числу сессий в файле Global.asax
        public ActionResult Index()
        {           
            DateTime datestart = DateTime.Now.AddMinutes(-5);
            DateTime dateend = DateTime.Now;
            var temperatura = kiasu.Vals.Where(t => t.tag_id == 2213 && t.time > datestart && t.time < dateend).OrderBy(t => t.time).ToList().LastOrDefault();
            //если полученные значение без сотых
            try
            {                         
              ViewBag.Temperatura =Math.Round((float)temperatura.val, 1).ToString();   
              ViewBag.TimeUpdate = temperatura.time.ToString("dd.MM") +" "+ temperatura.time.ToString("HH:mm");
            }
            catch
            {
                ViewBag.Temperatura = "Временно не доступна";
            }
          
            return View();
        }

       
        public ActionResult NewsView()
        {
           return View(db.News.ToList());
        }

        public ActionResult temperatura()
        {
           return View();
        }

        public JsonResult StatisticsRadioClick(string ip)
        {
            DateTime ClDate = DateTime.Now;
            if (db.ClickStatistics.Where(i => i.UserIp == ip).ToList().Where(d=>d.ClickData.Date == ClDate.Date).Count() != 0)
            {
                return Json("Найдено");
            }
            else
            {
                ClickStatistic cs = new ClickStatistic()
                {
                    ClickData = ClDate,
                    UserIp = Request.UserHostAddress,
                    ResurseClick = "Radio",
                    UserName = User.Identity.IsAuthenticated ? db.AspNetUsers.Find(User.Identity.GetUserId()).UserName : "NoName",
                    Location = ip.Contains("10.160") ? "Накын" : "Мирный"
                };

                db.ClickStatistics.Add(cs);
                db.SaveChanges();

                return Json("Ok");
            }
        }
               
        public ActionResult Contact()
        {
           return View();
        }
    }
}

     

        
