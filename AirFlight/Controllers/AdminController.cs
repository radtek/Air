using AirFlight.Filters;
using AirFlight.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace AirFlight.Controllers
{   [Log]
    [Authorize(Roles = "admin, aviadispatcher")]
    public class AdminController : Controller
    {
     
        private AirFlightContext db = new AirFlightContext();
        private ApplicationDbContext dba = new ApplicationDbContext();
        //добавление ролей пользоватлям
        #region AddRole
        public ActionResult AddRole(string UserId, string Message="")
        {
            ViewBag.UserId = UserId;

            //Загружаем список всех пользователей в List<SelectListItem>
            List<SelectListItem> UserList = dba.Users.Select(u => new SelectListItem { Value = u.Id, Text = u.Surneme.Trim() + " " + u.Name.Trim() + " " + u.LastName.Trim() }).OrderBy(u => u.Text).ToList();
            List<SelectListItem> ZList = dba.Users.Select(u => new SelectListItem { Value = u.Id, Text = u.Surneme.Trim() + " " + u.Name.Trim() + " " + u.LastName.Trim() }).OrderBy(u => u.Text).ToList();

            //Вставляем в список на первую позицию (отсчет идет с 0) Строку "Все пользователи" и "Не указан"
            UserList.Insert(0, new SelectListItem { Value = "0", Text = "Все пользователи" });
            ZList.Insert(0, new SelectListItem { Value = "0", Text = "Не указан" });

            //Отдаем на представление список пользователей
            ViewBag.UserList = new SelectList(UserList, "Value", "Text", UserId);
            //Отдаем на представление всех пользователей в списке Замов и если есть Зам у выбранного пользователя передаем его
            ViewBag.ZamList = new SelectList(ZList, "Value", "Text", ViewBag.Zam);

            if (UserId != null && UserId != "0")
            {
                var login = dba.Users.Find(UserId);
                ViewBag.Login = login.UserName;
            }

            //Все роли в системе
            var RoleList = db.AspNetRoles;

            var user = dba.Users.FirstOrDefault(u => u.Id == UserId);
            if (user != null)
            {
                ViewBag.Surname = user.Surneme;
                ViewBag.Name = user.Name;
                ViewBag.LastName = user.LastName;
                ViewBag.Phone = user.PhoneNumber;
                ViewBag.Zam = user.ZamID;
                ViewBag.Signature = user.Signature;
            }

            //Роли выбранного пользователя
            var RoleUserId = db.AspNetUserRoles.Where(r => r.UserId == UserId).Select(r => r.RoleId).ToList();
            ViewBag.RoleUserId = RoleUserId;

            return View(RoleList);

        }
#endregion

        //Сохранение ролей пользователей
        #region SaveRole
        public ActionResult SaveRole(string[] selectRole, string UserId, string Surname, string Name, string LastName, string Phone, string ZamId, string Signature)
        {
            //Сначала удаляем все роли у пользователя
            if (UserId != null)
            {
                foreach (var remove in db.AspNetUserRoles.Where(u => u.UserId == UserId))
                {
                    AspNetUserRole role = db.AspNetUserRoles.Find(remove.UserId);
                    db.AspNetUserRoles.Remove(role);
                }
                db.SaveChanges();
                if (selectRole != null)
                {
                    AspNetUserRole AspNetUserRoles = new AspNetUserRole();
                    //далее добавляем отмеченные роли
                    foreach (var role in selectRole)
                    {
                        AspNetUserRoles.UserId = UserId;
                        AspNetUserRoles.RoleId = role.ToString();
                        db.AspNetUserRoles.Add(AspNetUserRoles);
                        db.SaveChanges();
                    }
                }
                var usermanager = dba.Users.Find(UserId);
                usermanager.Surneme = Surname;
                usermanager.Name = Name;
                usermanager.LastName = LastName;
                usermanager.PhoneNumber = Phone;
                usermanager.Signature = String.IsNullOrWhiteSpace(Signature) ?"": usermanager.Signature = Signature;
                usermanager.ZamID = ZamId != "0" ? ZamId : null;
                usermanager.ZamLogin = ZamId != "0" ? dba.Users.Find(ZamId).Login : null;  

                if (ModelState.IsValid)
                {
                    dba.Entry(usermanager).State = EntityState.Modified;
                    dba.SaveChanges();
                }
                
                var signature =  db.Signatures.Where(s=>s.Sig==Signature).FirstOrDefault();
                if(signature==null && Signature!="")
                {
                    Signature Signatures = new Signature();
                    Signatures.Sig = Signature;

                    db.Signatures.Add(Signatures);                   
                    db.SaveChanges();
                    return Json("Ok");
                }


                return Json("Ok");
            }
            
            else
            {
                return Json("Выбереите пользователя!");
            }

        }
#endregion

        //Удаление ролей у пользователя
        #region DeleteRole
        public ActionResult DeleteRole(string UserId)
        {
            AspNetUser user = db.AspNetUsers.Find(UserId);
            if (user != null)
            {
                db.AspNetUsers.Remove(user);
                db.SaveChanges();
                return Json("Ok");
            }
            else
            {
                return Json("Пользователь не найден!");
            }
        }
#endregion

        //Новости
        #region News
        public ActionResult News(int create = 0, int id = 0)
        {
            ViewBag.Create = create == 0 ? "none" : "normal";
            ViewBag.IDI = id;

            return View(db.News.ToList());
        }
        #endregion

        //Сохранение при изменении новостей
        #region SaveEditNews
        public ActionResult SaveEditNews(New News)
        {
            var news = db.News.Find(News.Id);
            news.Header = News.Header;
            news.Content = News.Content;
            news.Add = News.Add;

            if (ModelState.IsValid)
            {
                db.Entry(news).State = EntityState.Modified;
                db.SaveChanges();
                return Json("Ok");
            }
            
            return Json("Ошибка сохранения!");
        }
#endregion

        //Сохранение новостей
        #region SaveNews
        public ActionResult SaveNews(New News)
        {
            News.Date = DateTime.Now;
            News.Header = News.Header;
            News.Content = News.Content;
            News.Add = News.Add;

            if (ModelState.IsValid)
            {
                db.News.Add(News);
                db.SaveChanges();
                return Json("Ok");
            }

            return Json("Ошибка добавления");
        }
#endregion

        //Удаление новостей
        #region DeleteNews
        public ActionResult DeleteNews(int? id)
        {
            New News = db.News.Find(id);
            
            if (News == null)
            {
                return Json("Новость не найдена");
            }

            db.News.Remove(News);
            db.SaveChanges();

            return Json("Ok");

        }
#endregion

        //Статистика по смс
        #region Statistics
        public ActionResult Statistics(DateTime? data, DateTime? day)
        {
            if (data == null)
            { data = TempData["data"] != null ? (DateTime)TempData["data"] : DateTime.Now.Date; }
            TempData["data"] = data;
            ViewBag.date = data;
            if (day == null)
            { day = TempData["day"] != null ? (DateTime)TempData["day"] : DateTime.Now.Date; }
            TempData["day"] = day;
            ViewBag.day = day;

            DateTime DayNow = DateTime.Now.Date;
            var statistics = db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).ToList();

            DateTime dt = (DateTime)data;
            DateTime d = (DateTime)day;
            //определяем номер месяца из даты
            int month = dt.Month;   
            //получаем название месяца из полученного номера
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ru-RU");            
            string monthname = ci.DateTimeFormat.GetMonthName(month);
                        
            var md = "За месяц "+ monthname + " " + dt.Year + " года: " + statistics.Where(s => s.SendingSms == true && s.Flight.FlightDate.Month == month).Count();
            ViewBag.month = md;
            var daysms = "На рейсы "+d.ToLongDateString().Replace("г.", "года") + ": " + statistics.Where(s => s.SendingSms == true && s.Flight.FlightDate == d).Count();           
            ViewBag.daysms = daysms;            

            //если пришёл запрос AJAX, то возвращаем только строку со счётчиком смс, а не всю модель
            if (Request.IsAjaxRequest())
            {
                var sms = new
                {
                    m = md,
                    d = daysms
                };

                return Json(sms, JsonRequestBehavior.AllowGet);
            }

            return View(statistics);
        }
        #endregion

        //Баланс
        #region Balance
        public JsonResult Balance()
        {
            try
            {    //https://lcab.smsprofi.ru/lcabApi/info.php?login=lanjeronium&password=Qwe767rtY - sms profi   
                //string balance = getbalance.Substring(getbalance.IndexOf("a") + 10, 6) + " руб.";
                //http://api.prostor-sms.ru/messages/v2/balance/?login=t89142529081&password=876980 - ProstoR                

                // string balance = getbalance.Substring(0, getbalance.LastIndexOf(";")).Replace("RUB;", "") + " руб.";
                //
                string getbalance = new WebClient().DownloadString("http://www.mcommunicator.ru/m2m/m2m_api.asmx/GetStatistics?login=79142518694&password=3b4e5b2d9297880bf800e4260e6db603");
                string balance = getbalance.Substring(getbalance.LastIndexOf("<Remainder>")+11, getbalance.IndexOf("</Remainder>")- getbalance.IndexOf("<Remainder>")-11) + " шт.";
                return Json(balance, JsonRequestBehavior.AllowGet);
            }
            
            catch 
            {
                return Json("Временно не доступен", JsonRequestBehavior.AllowGet);
            }         
        }
        #endregion

        //Автозаполнение подписи
        #region AutocompleteSignature
        //автозаполнение поля по подписи
        public ActionResult AutocompleteSignature(string term)
        {
            var Signature = db.Signatures.Where(s=>s.Sig.StartsWith(term)).Select(s => new {value = s.Sig }).ToList();

            return Json(Signature, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //Статистика для Администратора
        #region AdminStatistics
        public ActionResult AdminStatistics()
        {
            return View();
        }
        #endregion

        //Статистика по кликам (РАДИО)
        #region CountClick
        public string CountClick()
        {
            DateTime DTN = DateTime.Now.Date;

            var Click = db.ClickStatistics;
            string result = "<h5>Клики сегодня:</h5>" + "<h5>Мирный: " + Click.Where(s => s.ResurseClick == "Radio" && s.Location == "Мирный").ToList().Where(d => d.ClickData.Date == DTN).Count() + "</h5>" +
                "<h5>Накын: " + Click.Where(s => s.ResurseClick == "Radio" && s.Location == "Накын").ToList().Where(d => d.ClickData.Date == DTN).Count() + "</h5>" +
                "<h5>Всего кликов: " + "<h5>Мирный: " + Click.Where(s => s.ResurseClick == "Radio" && s.Location == "Мирный").Count() + "</h5>" +
                "<h5>Накын: " + Click.Where(s => s.ResurseClick == "Radio" && s.Location == "Накын").Count() + "</h5>" +
                "<h5>Всего: " + Click.Where(s => s.ResurseClick == "Radio").Count() + "</h5>";

            return result;
        }
#endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}


