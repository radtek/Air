using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AirFlight.Models;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using AirFlight.Filters;
using System.Web.Helpers;
using System.IO;

namespace AirFlight.Controllers
{
    [Authorize(Roles = "registrator, viewpdf")]
    public class RegistrationController : Controller
    {       
        private AirFlightContext db = new AirFlightContext();

        //Список рейсов
        #region Start       
        public ActionResult Start(DateTime? StartDate, DateTime? EndDate, DateTime? CurrentDate, int? choosDate, int? selectRoute, int? findpassenger)
        {
             if (StartDate == null)
                 { StartDate = TempData["StartDate"] != null ? (DateTime)TempData["StartDate"] : DateTime.Now.Date; }
             if (EndDate == null)
                {EndDate = TempData["EndDate"] != null ? (DateTime)TempData["EndDate"] : DateTime.Now.Date.AddDays(5);}
             if (CurrentDate == null)
                {CurrentDate = TempData["CurrentDate"] != null ? (DateTime)TempData["CurrentDate"] : DateTime.Now.Date;}
             if (choosDate == null)
                {choosDate = TempData["choosDate"] != null ? (int)TempData["choosDate"] : 0;}
             if (selectRoute == null)
                {selectRoute = TempData["selectRoute"] != null ? (int)TempData["selectRoute"] : 0; }
             if (findpassenger == null)
                { findpassenger = TempData["findpassenger"] != null ? (int)TempData["findpassenger"] : 0; }

            //Для сохранения выбранной даты при переходе по страницам сохраняем её в TempData
            TempData["StartDate"] = StartDate;
            TempData["EndDate"] = EndDate;
            TempData["CurrentDate"] = CurrentDate;
            TempData["choosDate"] = choosDate;
            TempData["selectRoute"] = selectRoute;
            TempData["findpassenger"] = findpassenger;
            //Так как переменная FlightDate является неопределённым типом, то записываем её сначала в ViewBag
            ViewBag.StartDate1 = StartDate;
            ViewBag.EndDate1 = EndDate;
            ViewBag.CurrentDate1 = CurrentDate;
            ViewBag.ChoosDate = choosDate;
            ViewBag.selectRoute = selectRoute;
            ViewBag.findpassenger = findpassenger;
            string Departure;
            string Arrival;
            //Выводим дату рейса с названием месяца
            ViewBag.StartDate = ViewBag.StartDate1.ToLongDateString();
            ViewBag.EndDate = ViewBag.EndDate1.ToLongDateString();
            ViewBag.CurrentDate = ViewBag.CurrentDate1.ToLongDateString();
            //Данная переменная необходима для отображения выбранной даты в datepicker
            ViewBag.SD = ViewBag.StartDate1.Date.ToString("yyyy-MM-dd");
            ViewBag.ED = ViewBag.EndDate1.Date.ToString("yyyy-MM-dd");
            ViewBag.CD = ViewBag.CurrentDate1.ToString("yyyy-MM-dd");
            //для определения подписок пользователя на рейс
            string UId = User.Identity.GetUserId();
            ViewBag.Notification = db.Notifications.Where(u => u.AspNetUserID == UId).Select(f=>f.FlightID).ToList();
            //Для определения количества зарегистрированных пользователей  в спсике
            ViewBag.registrationlist = choosDate == 0 ? db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate >= StartDate && s.Flight.FlightDate <= EndDate).Select(s => s.FlightID).ToList() :
            db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate == CurrentDate).Select(s => s.FlightID).ToList();

            //если мы выбрали поиск пассажира, то нам придёт его ID иначе он равен 0
            if (findpassenger != 0)
            {
                ViewBag.findepassenger = db.Passengers.Where(i=>i.id==findpassenger).Select(o=>o.Surname +" "+o.Name + " "+ o.Middlename).FirstOrDefault();

                //Ищем пассажира на всех рейсах и выбираем ID рейсов
                var find =  choosDate == 0 ? db.RegistrationLists.Include(f => f.Flight).Where(s => s.Flight.FlightDate >= StartDate && s.Flight.FlightDate <= EndDate && s.PassengerID == findpassenger)
                    .Select(f => f.FlightID).ToList(): db.RegistrationLists.Include(f => f.Flight).Where(s => s.Flight.FlightDate == CurrentDate && s.PassengerID == findpassenger).Select(f =>f.FlightID).ToList();

                //Ищем все найденные Id рейсов в типизированной базе Flight
                var flyghts = db.Flights.Where(f => find.Contains(f.id));
            
                return View(flyghts);
            }
            else
            {
                ViewBag.findepassenger = "";
                switch (selectRoute)
                {
                    case 0:
                        Departure = ""; Arrival = ""; break;
                    case 1:
                        Departure = "Мирный"; Arrival = "Накын"; break;
                    case 2:
                        Departure = "Накын"; Arrival = "Мирный"; break;
                    case 3:
                        Departure = "Нюрба"; Arrival = "Накын"; break;
                    case 4:
                        Departure = "Накын"; Arrival = "Нюрба"; break;
                    case 5:
                        Departure = "Вилюйск"; Arrival = "Накын"; break;
                    case 6:
                        Departure = "Накын"; Arrival = "Вилюйск"; break;
                    case 7:
                        Departure = "Накын"; Arrival = "Чукар"; break;
                    case 8:
                        Departure = "Чукар"; Arrival = "Накын"; break;
                    case 9:
                        Departure = "Кюндядя"; Arrival = "Накын"; break;
                    case 10:
                        Departure = "Накын"; Arrival = "Кюндядя"; break;
                    case 11:
                        Departure = "Малыкай"; Arrival = "Накын"; break;
                    case 12:
                        Departure = "Накын"; Arrival = "Малыкай"; break;
                    default:
                        Departure = ""; Arrival = ""; break;

                }
                var flyghts = choosDate == 0 ? db.Flights.Where(s => Departure == "" ? s.FlightDate >= StartDate && s.FlightDate <= EndDate : s.FlightDate >= StartDate && s.FlightDate <= EndDate && s.Departure == Departure && s.Arrival == Arrival).OrderBy(s => s.FlightDate).ToList() :
                                          db.Flights.Where(s => Departure == "" ? s.FlightDate == CurrentDate : s.FlightDate == CurrentDate && s.Departure == Departure && s.Arrival == Arrival).ToList();
                return View(flyghts);

            }
        }
        #endregion

        //лист регистрации
        #region Registration       
        public ActionResult Registration(int FlightID, int AddPassenger = 0, int idi = 0, int edit = 0, int createtmpl = 0, string all = "no")
        {
            var flight = db.Flights.FirstOrDefault(d => d.id == FlightID);
            ViewBag.FlightDate = flight.FlightDate.ToLongDateString();
            ViewBag.FD = flight.FlightDate;
            ViewBag.FlightNum = flight.FlightNum;
            ViewBag.Departure = flight.Departure.Trim();
            ViewBag.Arrival = flight.Arrival.Trim();
            ViewBag.FlyTime = flight.FlightTime;
            ViewBag.Approved = flight.Approved;
            ViewBag.AirType = flight.AirType;            

            //Для изменения номера рейса при редактировании пользователя определяем список доступных рейсов в указанный день и выводим их в список
            DateTime fd = ViewBag.FD;
            string dp = ViewBag.Departure;
            string ar = ViewBag.Arrival;
            int fl = ViewBag.FlightNum;
            ViewBag.NumFly = db.Flights.Where(d => d.FlightDate == fd && d.Departure == dp && d.Arrival == ar).Select(d => d.FlightNum).Distinct().ToList();

            SelectList num = new SelectList(db.Flights.Where(d => d.FlightDate == fd && d.Departure == dp && d.Arrival == ar)
                                            .OrderBy(d => d.FlightNum).Select(d => new { id = d.id, flynumed = d.FlightNum }), "id", "flynumed", FlightID);

           
            ViewBag.FlightNumbers = num; //тут мы формируем список для отображения flighnum - это то, что получили из базы,         
            //Если не выбрано добавить пассажира, то скрываем поля на странице
              ViewBag.AddPassenger =AddPassenger == 0?0:1;
            //Определяем список пассажиров на выбранном рейсе. Если мы выводим весь список то сортировать будем не по ID рейса, а по дате вылета и выводить всех пассажиров                                  
            var registrationlist = db.RegistrationLists.Include(f =>f.Flight).Include(p => p.Passenger).Where(s => all=="no"?s.Flight.id == FlightID:s.Flight.FlightDate == fd&& s.Flight.Departure == dp&& s.Flight.Arrival == ar).OrderBy(s =>s.Passenger.Surname);
            //ViewBag.FlightNumS  -данная переменная необходима для перечисления рейсов в цикле foreach
            //ViewBag.FlightNumS = db.Flights.Where(s => s.FlightDate == fd && s.Departure == dp && s.Arrival==ar).Select(s => all == "yes" ?s.FlightNum:fl).OrderBy(s => s).ToList().Distinct();
            ViewBag.FlightInfo = db.Flights.Where(s => all == "no" ? s.id == FlightID : s.FlightDate == fd && s.Departure == dp && s.Arrival == ar).OrderBy(s => s.id).ToList();
            //Данная переменная необходима для редактирования выбранной строки на странице
            ViewBag.ID = FlightID;           
            ViewBag.All = all;
            //если редактирование не выбрано
            ViewBag.Edit=edit == 0 ? 0: 1;
            ViewBag.IDI= edit == 0 ? 0:idi;
            ViewBag.createtmpl = createtmpl;
            string UId = User.Identity.GetUserId();
            List<SelectListItem> ListTmltPass = db.TemplatesNamePassengers.Where(u => u.UserId == UId).Select(s => new SelectListItem { Value = s.Num.ToString(), Text = s.Description }).OrderBy(u => u.Value).ToList();
            ListTmltPass.Insert(0, new SelectListItem { Value = "0", Text = "Выберите шаблон" });
            ViewBag.TmplListPass = new SelectList(ListTmltPass, "Value", "Text");
           
            using (ApplicationDbContext dba = new ApplicationDbContext())
            {
                ViewBag.ZamId = dba.Users.Find(User.Identity.GetUserId()).ZamID;
                ViewBag.UserId = User.Identity.GetUserId();
            }
            //Находим все ID у которых есть роль авиадиспетчер. При выводе списка пассажиров запрещаем редактировать пассажиров которых добавил авиадиспетчер
            //(это необходимо если у пользователя установлены права редактирование любого пассжира)
             ViewBag.UR = db.AspNetUserRoles.Where(r => r.RoleId == "2").Select(i => i.UserId).ToList();
        
            //Для прикола проверка запроса AJAX
            if (Request.IsAjaxRequest())
            {
                return View(registrationlist);
            }            

            return View(registrationlist);           
        }
        #endregion

        //Добавление нового пассажира
        #region Create       
        [Log]
        public ActionResult Create(RegistrationList registrationList, string confirm = "no")
        {
            //Определяем информацию о рейса
            var date = db.Flights.FirstOrDefault(s => s.id == registrationList.FlightID);
            DateTime DF = date.FlightDate;
            string dp = date.Departure;
            string ar = date.Arrival;
            var pfromto = db.RegistrationLists.Include(f => f.Flight);
            
            //Проверяем есть ли такой пассажир на других рейсах в выбранную дату            
            var pany = pfromto.Where(s => s.Flight.FlightDate == DF && s.Flight.Departure == dp && s.Flight.Arrival == ar && s.PassengerID == registrationList.PassengerID);

            if (pany.Count() > 0)
            {               
                return Json("Этот пассажир уже зарегистрирован на рейсе № " + pany.FirstOrDefault().Flight.FlightNum);
            }
            var total = db.RegistrationLists.Where(s => s.Flight.id == registrationList.FlightID);
            int qt = db.Flights.Find(registrationList.FlightID).Number;

            if (total.Count() >= qt)
            {
                return Json("На этом рейсе нет свободных мест для регистрации пассажира!");
            }
            //Проверяем есть ли такой пассажир на встречных рейсах в выбранную дату           
            var passengerany = pfromto.Where(s => s.Flight.FlightDate == DF && s.PassengerID == registrationList.PassengerID);
            if (passengerany.Count() > 0)
            {//если нажали ок
                if (confirm == "yes")
                {//если мы нажали Ок на вопрос о добавлении, то пассажир будет добавлен на рейс
                    if (ModelState.IsValid)
                    {
                        db.RegistrationLists.Add(registrationList);
                        db.SaveChanges();
                    }
                    return Json("Ok");
                }
                else
                {//отправляем запрос на добавить его в список или нет                   
                    return Json("Этот пассажир уже зарегистрирован на рейсе " + passengerany.FirstOrDefault().Flight.Departure.Trim() + " - " + passengerany.FirstOrDefault().Flight.Arrival.Trim() + "! Добавить его?");
                }
            }

            //Проверяем количество зарегистрированных пассажиров и количество мест
                if (ModelState.IsValid)
                {
                    db.RegistrationLists.Add(registrationList);
                    db.SaveChanges();

                }
                return Json("Ok");
            
        }
        #endregion

        //Добавление пассажиров по шаблону
        #region CreateFlyTmpl
        [Log]
        public JsonResult AddTmplPass(int? Tmplnum, int FlightID, int?[] selectPass, string confirm = "No")
        {
            string UId = User.Identity.GetUserId();

            //Если выборано сохранение пользователей вручную
            if (confirm == "SaveCheckPass")
            {
                using (AirFlightContext db = new AirFlightContext())
                {
                    foreach (var selectpass in selectPass)
                    {
                        RegistrationList reglist = new RegistrationList()
                        {
                            CreatorId = UId,
                            EditorId = UId,
                            FlightID = FlightID,
                            PassengerID = selectpass
                        };
                        db.RegistrationLists.Add(reglist);
                        db.SaveChanges();
                    }
                    return Json("Ok");
                }
            }
                var tmplpass = db.TemplatesPassengers.Where(u => u.UserId == UId && u.Num == Tmplnum).ToList();
              //если количество пассажиров в шаблоне 0 возвращаем ошибку
                if (tmplpass.Count() == 0)
                {
                    var result = new
                    {
                        status = "ErrNoItem",
                        msg = "В выбранном шаблоне нет пассажиров для добавления!"
                    };

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

            //Определяем информацию о рейсе
            var date = db.Flights.FirstOrDefault(s => s.id == FlightID);
            DateTime DF = date.FlightDate;
            string dp = date.Departure;
            string ar = date.Arrival;
            int total = db.RegistrationLists.Where(s => s.Flight.id == FlightID).Count();
            int qt = db.Flights.Find(FlightID).Number;
            var passenger = db.RegistrationLists.Include(f => f.Flight);
            List<string> ConfirmPassengerList = new List<string>();
            List<int> ConfirmPassengerId = new List<int>();
            List<bool> AllowCheckPassenger = new List<bool>();
            List<string> ExistPassenger = new List<string>();
            List<int> CountAlowPassenger = new List<int>();

            //считаем какое количество пользователей мы добавим по факту (то есть если количество мест для регистрации доступно 2 а мы добавляем 5 пассажиров)
            //и из них 3 уже есть на рейсе, то нам нет смысла предлагать пользователю добавить их вручную, поэтому сначала считаем
            foreach (var allow in tmplpass)
            {
                var allowaddpassenegr = db.Passengers.Find(allow.PassengerID);
                //Проверяем есть ли такой пассажир на других рейсах в выбранную дату                  
                var pflyinfo = passenger.Where(s => s.Flight.FlightDate == DF && s.PassengerID == allow.PassengerID);
                if (pflyinfo.Count() > 0)
                {
                    continue;
                }
                else
                {//добавляем в список только тех кого нет на рейсах на дату вылета
                    CountAlowPassenger.Add(allowaddpassenegr.id);
                }
            }

            //если количество мест для регистрации меньше чем количество добавляемых пассажиров, отправляем запрос на добавление в ручную  
            if (CountAlowPassenger.Count() > qt - total && confirm == "No")
            {
                var result = new
                {
                    status = "ConfirmAdd",
                    msg = "Добавляемое вами количество пассажиров (" + tmplpass.Count() + "), превышает доступное количество мест (" +
                    (qt - total) + ") на рейсе! Хотите выбрать пассажиров для добавления?"
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            //если мы выбрали что хотим добавить пассажиров в ручную (или сразу отметили что добавлять будем из списка), то формируем список пассажиров из шаблона 

            if (confirm == "Yes")
            {
                foreach (var c in tmplpass)
                {
                    var selectpassenegr = db.Passengers.Find(c.PassengerID);
                    //Проверяем есть ли такой пассажир на других рейсах в выбранную дату                  
                    var passengerflyinfo = passenger.Where(s => s.Flight.FlightDate == DF && s.PassengerID == c.PassengerID);
                    if (passengerflyinfo.Count() > 0)
                    {
                        //если пользователь зарегистрирован на встречных рейсах до добавляем об этом информацию
                        ConfirmPassengerList.Add(string.Format("{0} {1} {2} - зарегистрирован на рейс №{3} {4} - {5}", selectpassenegr.Surname, selectpassenegr.Name, selectpassenegr.Middlename,
                            passengerflyinfo.FirstOrDefault().Flight.FlightNum, passengerflyinfo.FirstOrDefault().Flight.Departure, passengerflyinfo.FirstOrDefault().Flight.Arrival));
                        //добавляем в список ID пользователя
                        ConfirmPassengerId.Add(selectpassenegr.id);
                        //добавляем в список флаг о запрете его выделения
                        AllowCheckPassenger.Add(false);
                    }
                    else
                    {
                        ConfirmPassengerList.Add(string.Format("{0} {1} {2}", selectpassenegr.Surname, selectpassenegr.Name, selectpassenegr.Middlename));
                        ConfirmPassengerId.Add(selectpassenegr.id);
                        AllowCheckPassenger.Add(true);
                    }

                }

                if (!AllowCheckPassenger.Contains(true))
                {
                    var result = new
                    {
                        status = "NoConfirmAddLis",
                        ConfirmPassengerList = ConfirmPassengerList
                    };

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //после того как список сформирован высылаем его пользователю
                    var result = new
                    {
                        status = "ConfirmAddLis",
                        Count = qt - total,
                        ConfirmPassenger = ConfirmPassengerList,
                        ConfirmPassengerId = ConfirmPassengerId,
                        AllowCheckPassenger = AllowCheckPassenger
                    };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            
        
            using (AirFlightContext db = new AirFlightContext())
            {               
                foreach (var listtmplpass in tmplpass)
                {
                    //Проверяем есть ли такой пассажир на рейсах в выбранную дату           
                    var passconfirmadd = passenger.Where(s => s.Flight.FlightDate == DF && s.PassengerID == listtmplpass.PassengerID);
                    if (passconfirmadd.Count() > 0)
                    {
                        var passinfo = db.Passengers.Find(listtmplpass.PassengerID);
                        ExistPassenger.Add(string.Format("{0} {1} {2} - рейс №{3} {4} - {5}", passinfo.Surname, passinfo.Name, passinfo.Middlename,
                            passconfirmadd.FirstOrDefault().Flight.FlightNum, passconfirmadd.FirstOrDefault().Flight.Departure, passconfirmadd.FirstOrDefault().Flight.Arrival));
                        continue;
                    }
                    RegistrationList reglist = new RegistrationList()
                    {
                        CreatorId = UId,
                        EditorId = UId,
                        FlightID = FlightID,
                        PassengerID = listtmplpass.PassengerID
                    };

                    db.RegistrationLists.Add(reglist);
                    db.SaveChanges();
                }

                if (ExistPassenger.Count() > 0)
                {
                    var result = new
                    {
                        status = "ErrAdd",
                        ExistPassenger = ExistPassenger
                    };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Ok", JsonRequestBehavior.AllowGet);
                }                

            }          
        }
        
        #endregion

        //поиск пассажира
        #region AutocompleteFindPassenger
        public ActionResult AutocompleteFindPassenger(string term)
        {
            var fio = db.Passengers.Where(p => (p.Surname+" "+p.Name+" "+p.Middlename).StartsWith(term))
                 .Select(p => new {                     
                     label = p.Surname + " " + p.Name.Trim() + " " + p.Middlename + " (Таб. №  " + p.EmployeeNum + ")  ",
                     value = p.Surname + " " + p.Name.Trim() + " " + p.Middlename,
                     id = p.id,
                 }).ToList();
            return Json(fio, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //автозаполнение поля по фамилии
        #region AutocompleteFIO

        public ActionResult AutocompleteFIO(string term)
        { //Если необходимо найти любой символ в последовательности то можно использовать Contains вместо StartsWith
            var fio = db.Passengers.Where(p => p.Surname.StartsWith(term))
                .Select(p => new {
                  id = p.id,
                  value = p.Surname.Trim() + " " + p.Name.Trim() + " " + p.Middlename.Trim(),
                  EmployeeNum =p.EmployeeNum,
                  Passport =p.Passport,
                  Site = p.Site
                }).ToList();
            return Json(fio, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //автозаполнение поля по табельному номеру
        #region AutocompleteEmplNum
        public ActionResult AutocompleteEmplNum(string term)
        {
            var EmplNum = db.Passengers.Where(p => p.EmployeeNum.StartsWith(term))
                .Select(p => new {
                    id = p.id,
                    fio = p.Surname.Trim() + " " + p.Name.Trim() + " " + p.Middlename.Trim(),
                    value = p.EmployeeNum,
                    Passport = p.Passport,
                    Site = p.Site
                }).ToList();
            return Json(EmplNum, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //автозаполнение поля по паспортным данным
        #region AutocompletePassport
        public ActionResult AutocompletePassport(string term)
        {

            var Passport = db.Passengers.Where(p => p.Passport.StartsWith(term))
                .Select(p => new {
                    id = p.id,
                    fio = p.Surname.Trim() + " " + p.Name.Trim() + " " + p.Middlename.Trim(),
                    EmployeeNum = p.EmployeeNum,
                    Site = p.Site,
                    value = p.Passport
                }).ToList();

            return Json(Passport, JsonRequestBehavior.AllowGet);
        }
        
        #endregion

        //изменение пассажира  
        #region Edit
        [Log]
        public ActionResult Edit(int id, string EditorID, string Note, int FlightID)
        {
            //проверяем количество пассажиров зарегестрированных на рейс и количество для регистрации
            var total = db.RegistrationLists.Where(s => s.Flight.id == FlightID);
            var passenger = db.RegistrationLists.Find(id);
            var qt = db.Flights.Find(FlightID);
            if (total.Count() >= qt.Number)
            {
                return Json("На этом рейсе нет свободных мест для регистрации пассажира!");
            }
                        
            if(qt.Approved.Trim()=="Ограничена")
            {
                if(User.IsInRole(qt.FlightDate.ToString("dddd")) || (User.IsInRole("allday")))
                {
                    var registrationList = db.RegistrationLists.Find(id);                   
                    registrationList.EditorId = EditorID;                   
                    registrationList.Note = Note;
                    registrationList.FlightID = FlightID; //если рейс был изменён, то сохраняем его
                    if (ModelState.IsValid)
                    {
                        db.Entry(registrationList).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    return Json("Ok");
                }
                else
                {
                    return Json("Вы не можете переместить пассажира на Рейс №"+ qt.FlightNum+ "! Регистрация на выбранный рейс ограничена и недоступна для Вас.");
                }              
             }            

            else
            {
                var registrationList = db.RegistrationLists.Find(id);               
                registrationList.EditorId = EditorID;               
                registrationList.Note = Note;
                registrationList.FlightID = FlightID; //если рейс был изменён, то сохраняем его
                if (ModelState.IsValid)
                {
                    db.Entry(registrationList).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return Json("Ok");
            }
        }

        #endregion
        //Удаление пассажира с рейса
        #region Delete
        [Log]
        public ActionResult Delete(int id, int FlightID, string  all)
        {
            RegistrationList registrationList = db.RegistrationLists.Find(id);

            if (registrationList == null)
            {
                return HttpNotFound();
            }

            db.RegistrationLists.Remove(registrationList);
            db.SaveChanges();

            return Json(Url.Action("Registration", new {FlightID = FlightID, all=all }), JsonRequestBehavior.AllowGet);            
        }

        #endregion

        //Информация о пассажире и кто его добавил
        #region InfoPassenger
        public JsonResult InfoPassenger(int? ItemID)
        {
            var infopassenger = db.RegistrationLists.Include(p => p.Passenger).FirstOrDefault(i => i.id == ItemID);
            string CID, EID, PhoneCreator, PhoneEditor;

            using (ApplicationDbContext dba = new ApplicationDbContext())
            {
                var infouseradd = dba.Users.FirstOrDefault(u => u.Id == infopassenger.CreatorId);
                CID = infouseradd.Surneme + " " + infouseradd.Name + " " + infouseradd.LastName;
                PhoneCreator = infouseradd.PhoneNumber;
                var infouseredit = dba.Users.FirstOrDefault(u => u.Id == infopassenger.EditorId);
                EID = infouseredit.Surneme + " " + infouseredit.Name + " " + infouseredit.LastName;
                PhoneEditor = infouseredit.PhoneNumber;
            };

            var result = new
            {
                CreatorID = CID,
                PhoneCID = PhoneCreator ?? "Не указан",
                EditorID = EID,
                PhoneEID = PhoneEditor ?? "Не указан",
                UserName = infopassenger.Passenger.Surname + " " + infopassenger.Passenger.Name + " " + infopassenger.Passenger.Middlename,
                Work = infopassenger.Passenger.Site + ", " + infopassenger.Passenger.Department,
                Phone = infopassenger.Passenger.PhoneNum ?? "Не указан",
                Residense = infopassenger.Passenger.Residence
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //печать списков
        #region ChoosePrintFlights
        public ActionResult ChoosePrintFlights(DateTime? StartDate, DateTime? EndDate, DateTime? CurrentDate, int? choosDate, int? sortDate, string Site)
        {
            //инициализация дат при загрузке страницы. Если они null (что возникает при обновлении страницы), то проверяем есть ли сохранённая дата в TempData, если пусто то присваиваем сегодняшняядата-5 дней
            if (StartDate == null)
            { StartDate = TempData["StartDate"] != null ? (DateTime)TempData["StartDate"] : DateTime.Now.Date; }
            if (EndDate == null)
            { EndDate = TempData["EndDate"] != null ? (DateTime)TempData["EndDate"] : DateTime.Now.Date.AddDays(5); }
            if (CurrentDate == null)
            { CurrentDate = TempData["CurrentDate"] != null ? (DateTime)TempData["CurrentDate"] : DateTime.Now.Date; }
            if (choosDate == null)
            { choosDate = TempData["choosDate"] != null ? (int)TempData["choosDate"] : 0; }
            if (sortDate == null)
            { sortDate = TempData["sortDate"] != null ? (int)TempData["sortDate"] : 0; }
            if (Site==null)
            { Site = TempData["Site"] != null ? (string)TempData["Site"] : "Все подразделения"; }


            TempData["StartDate"] = StartDate;
            TempData["EndDate"] = EndDate;
            TempData["CurrentDate"] = CurrentDate;
            TempData["choosDate"] = choosDate;
            TempData["sortDate"] = sortDate;
            TempData["Site"] = Site;

            //Так как переменная FlightDate является неопределённым типом, то записываем её сначала в ViewBag
            ViewBag.StartDate1 = StartDate;
            ViewBag.EndDate1 = EndDate;
            ViewBag.CurrentDate1 = CurrentDate;
            ViewBag.ChoosDate = choosDate;
            ViewBag.SortDate = sortDate;
            ViewBag.SiteList = Site;

            //Выводим дату рейса с названием месяца
            ViewBag.StartDate = ViewBag.StartDate1.ToLongDateString();
            ViewBag.EndDate = ViewBag.EndDate1.ToLongDateString();
            ViewBag.CurrentDate = ViewBag.CurrentDate1.ToLongDateString();
            //Данная переменная необходима для отображения выбранной даты в datepicker
            ViewBag.SD = ViewBag.StartDate1.Date.ToString("yyyy-MM-dd");
            ViewBag.ED = ViewBag.EndDate1.Date.ToString("yyyy-MM-dd");
            ViewBag.CD = ViewBag.CurrentDate1.ToString("yyyy-MM-dd");

           
            List<SelectListItem> SiteList = db.Passengers.Select(u => new SelectListItem {Value=u.Site.Trim()??"Не указано", Text =u.Site.Trim()??"Не указано"}).Distinct().OrderBy(u=>u).ToList();
            //Вставляем в список на первую позицию (отсчет идет с 0) Строку "Все пользователи" и "Не указан"
            SiteList.Insert(0, new SelectListItem { Value = "Все подразделения", Text = "Все подразделения" });
            ViewBag.Site = new SelectList(SiteList, "Value", "Text", Site);
                      

            if (Site == "Все подразделения")
            {
                //Для определения количества зарегистрированных пользователей  в спсике
                ViewBag.registrationlist = choosDate == 0 ? db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate >= StartDate && s.Flight.FlightDate <= EndDate).Select(s => s.FlightID).ToList() :
                    db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate == CurrentDate).Select(s => s.FlightID).ToList();
               
                var flyghts = choosDate == 0 ? db.RegistrationLists.Include(f => f.Flight).Where(s => s.Flight.FlightDate >= StartDate && s.Flight.FlightDate <= EndDate).OrderBy(s => s.Flight.FlightDate).ToList() :
                                             db.RegistrationLists.Include(f => f.Flight).Where(s => s.Flight.FlightDate == CurrentDate).ToList();

                return View(flyghts);
            }
            else
            {
                //Для определения количества зарегистрированных пользователей  в спсике
                ViewBag.registrationlist = choosDate == 0 ? db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate >= StartDate && s.Flight.FlightDate <= EndDate && s.Passenger.Site == Site).Select(s => s.FlightID).ToList() :
                    db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate == CurrentDate && s.Passenger.Site == Site).Select(s => s.FlightID).ToList();

                var flyghts = choosDate == 0 ? db.RegistrationLists.Include(f => f.Flight).Include(p=>p.Passenger).Where(s => s.Flight.FlightDate >= StartDate && s.Flight.FlightDate <= EndDate && s.Passenger.Site==Site).OrderBy(s => s.Flight.FlightDate).ToList() :
                                             db.RegistrationLists.Include(f => f.Flight).Where(s => s.Flight.FlightDate == CurrentDate && s.Passenger.Site == Site).ToList();

                return View(flyghts);
            }

        }
        #endregion

        //вывод списков на печать
        #region PrintFlights
        public ActionResult PrintFlights(int? sortDate, DateTime? FlightDate, int? FlightID, string Departure, string Arrival, string Site)
        {
            //для изменения подписи в списках на вылет определяем текущего пользователя и из поля подпись берём значение
            //using (ApplicationDbContext dba = new ApplicationDbContext())
            //{
            //    ViewBag.Signature = dba.Users.Find(User.Identity.GetUserId()).Signature;
            //}


            if (Site == "Все подразделения")
            {

                var printlist = sortDate == 0? db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.FlightID == FlightID).OrderBy(s => s.Passenger.Surname):               
                                               db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate == FlightDate &&
                                               s.Flight.Departure == Departure && s.Flight.Arrival == Arrival).OrderBy(s => s.Passenger.Surname);
               
                ViewBag.FlightNumS = printlist.Select(s => s.FlightID).Distinct().ToList();
                          
                
                return View(printlist);
            }

            else
            {
                var printlist = sortDate == 0 ? db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.FlightID == FlightID && s.Passenger.Site==Site).OrderBy(s => s.Passenger.Surname) :
                                               db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate == FlightDate && s.Flight.Departure == Departure
                                               && s.Flight.Arrival == Arrival && s.Passenger.Site == Site).OrderBy(s => s.Passenger.Surname);

                ViewBag.FlightNumS = printlist.Select(s => s.FlightID).Distinct().ToList();


                return View(printlist);
            }

        }
        #endregion

        //Добавление уведомлений о рейсах
        #region NotificationsAdd
        public JsonResult NotificationsAdd(Notification Notifications)
        {
            using (AirFlightContext db = new AirFlightContext())
            {
                if(String.IsNullOrWhiteSpace(db.AspNetUsers.Find(Notifications.AspNetUserID).EmailSend))
                {
                    return Json("NoEmail", JsonRequestBehavior.AllowGet);
                }
                    
                if (db.Notifications.Where(u => u.AspNetUserID == Notifications.AspNetUserID && u.FlightID == Notifications.FlightID).Count() > 0)
                {
                    return Json("Вы уже добавили этот рейс для уведомления!", JsonRequestBehavior.AllowGet);
                }


                if (ModelState.IsValid)
                {
                    db.Notifications.Add(Notifications);
                    db.SaveChanges();
                    return Json("Ok", JsonRequestBehavior.AllowGet);
                }

            }
            return Json("Ошибка", JsonRequestBehavior.AllowGet);
        }
        #endregion

        //Удаление уведомлений о рейсах
        #region NotificationsDel
        public JsonResult NotificationsDel(int FlightId)
        {
           string UId = User.Identity.GetUserId();
           Notification notifications = db.Notifications.Where(u=>u.AspNetUserID==UId && u.FlightID==FlightId).FirstOrDefault();

            if (notifications == null)
            {
                return Json("Ошибка, уведомление не найдено!", JsonRequestBehavior.AllowGet);
            }

            db.Notifications.Remove(notifications);
            db.SaveChanges();
            
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }
        #endregion

        //Шаблоны пользователей
        #region TemplatesPassengers
        public ActionResult TemplatesPassengers(int? NumTab, int id = 0, int edit = 0, int creat = 0, int edittmpl = 0)
        {
            if (NumTab == null)
            { NumTab = TempData["NumTab"] != null ? (int)TempData["NumTab"] : 1; }
            TempData["NumTab"] = NumTab;
            ViewBag.NumTab = NumTab;

            //edit == 1 ? присваиваем ViewBag.ID =0 если не было команды на редакитрование
            ViewBag.ID = edit == 1 ? id : 0;
            ViewBag.Create = creat;
            ViewBag.edittmpl = edittmpl;
            string UId = User.Identity.GetUserId();
            ViewBag.NameTabTmpl = db.TemplatesNamePassengers.Where(u => u.UserId == UId).ToList();

            int NameTabNum = db.TemplatesNamePassengers.Where(u => u.UserId == UId).Select(n => n.Num).ToList().Count();
            ViewBag.NameTabNum = NameTabNum + 1;

            var temlates = db.TemplatesPassengers.Include(p=>p.Passenger).Where(u => u.UserId == UId).OrderBy(u => u.Passenger.Surname).ToList();

            return View(temlates);
        }
        #endregion

        //Добавление названия шаблона (закладки)
        #region AddTmplNamePassenger
        public JsonResult AddTmplNamePassenger(TemplatesNamePassenger TmplNamePass)
        {
            string UId = User.Identity.GetUserId();
            if (db.TemplatesNamePassengers.Where(u => u.UserId == UId && u.Description == TmplNamePass.Description).Count() > 0)
            {
                return Json("Такой шаблон уже существует, введите другое название!", JsonRequestBehavior.AllowGet);
            }

            if (ModelState.IsValid)
            {
                TmplNamePass.UserId = UId;
                db.TemplatesNamePassengers.Add(TmplNamePass);
                db.SaveChanges();
                return Json("Ok", JsonRequestBehavior.AllowGet);
            }

            return Json("Ошибка сохранения!", JsonRequestBehavior.AllowGet);

        }
        #endregion

        //Удаляем шаблон
        #region DelTmplNamePassenger
        public JsonResult DelTmplNamePassenger(int Id)
        {
            //сначала удалим всё содержимое в шаблонах
            string UId = User.Identity.GetUserId();
            TemplatesNamePassenger TmplNamePass = db.TemplatesNamePassengers.Find(Id);
            int Num = TmplNamePass.Num;
            var tmpl = db.TemplatesPassengers.Where(u => u.Num == Num && u.UserId == UId);
            //если есть содержимое шаблона, то удаляем сначала его
            if (tmpl != null)
            {
                foreach (var remtml in tmpl)
                {
                    TemplatesPassenger template = db.TemplatesPassengers.Find(remtml.Id);
                    db.TemplatesPassengers.Remove(template);
                }
                db.SaveChanges();
            }
              //Находим сам шаблон
            
            if (TmplNamePass == null)
            {
                return Json("Не найден шаблон для удаления!", JsonRequestBehavior.AllowGet);
            }

            db.TemplatesNamePassengers.Remove(TmplNamePass);
            db.SaveChanges();
            return Json("Ok", JsonRequestBehavior.AllowGet);

        }
        #endregion

        //Сохранение при изменении шаблона
        #region SaveTmplNamePassenger
        public JsonResult SaveTmplNamePassenger(TemplatesNamePassenger TmplNamePass)
        {
            var editnametmpl = db.TemplatesNamePassengers.Find(TmplNamePass.Id);
            editnametmpl.Description = TmplNamePass.Description;

            if (ModelState.IsValid)
            {
                db.Entry(editnametmpl).State = EntityState.Modified;
                db.SaveChanges();
                return Json("Ok", JsonRequestBehavior.AllowGet);
            }

            return Json("Ошибка", JsonRequestBehavior.AllowGet);

        }
        #endregion

        //Добавление рейса в шаблон
        #region AddTemplatesPassenger
        public JsonResult AddTemplatesPassenger(TemplatesPassenger TmplatesPass)
        {
            string UId = User.Identity.GetUserId();
            if (db.TemplatesPassengers.Where(s => s.UserId == UId && s.Num == TmplatesPass.Num && s.PassengerID == TmplatesPass.PassengerID).Count() > 0)
            {
                return Json("Такой пассажир уже существует в этом шаблоне!", JsonRequestBehavior.AllowGet);
            }

            if (ModelState.IsValid)
            {
                TmplatesPass.UserId = UId;
                db.TemplatesPassengers.Add(TmplatesPass);
                db.SaveChanges();
                return Json("Ok", JsonRequestBehavior.AllowGet);
            }

            return Json("Ошибка", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region DeleteTemplatesPassenger
        public JsonResult DeleteTemplatesPassenger(int id)
        {
            TemplatesPassenger tmplpass = db.TemplatesPassengers.Find(id);
            if (tmplpass == null)
            {
                return Json("Не найден пассажир для удаления!", JsonRequestBehavior.AllowGet);
            }
            db.TemplatesPassengers.Remove(tmplpass);
            db.SaveChanges();

            return Json("Ok", JsonRequestBehavior.AllowGet);
        }
        #endregion
      
        //Просмотр документа
        #region showfiles
        public FilePathResult ShowFiles(int flightid)
        {
            var fly = db.Flights.Find(flightid);
            if (!string.IsNullOrWhiteSpace(fly.DocsName))
            {
                string file = Path.Combine("D:/DB/AirFlightFiles/FlyPdf/", fly.DocsName);
                string filetype = "application/pdf";
                //если раскоментировать строку ниже то файл будет открываться в новой вкладке в браузере
                //Response.AppendHeader("Content-Disposition", "inline; filename=" + fly.DocsName);
                return File(file, filetype, fly.DocsName);
            }
            else
            {
                return File("Файл не найден!", "application/pdf");
            }

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




