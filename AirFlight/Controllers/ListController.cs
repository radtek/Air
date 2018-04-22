using AirFlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Microsoft.AspNet.Identity;

namespace AirFlight.Controllers
{
    public class ListController : Controller
    {
        // GET: List
              
        private AirFlightContext db = new AirFlightContext();

        //Отображение списка пассажиров
        #region PassengersList
        public ActionResult PassengersList(DateTime? FlightDate, int? selectRoute, int? findpassenger)
        {
            //ListFly
            if (FlightDate == null)
            { FlightDate = TempData["FlightDate"] != null ? (DateTime)TempData["FlightDate"] : DateTime.Now.Date; }           
            if (selectRoute == null)
            { selectRoute = TempData["selectRoute"] != null ? (int)TempData["selectRoute"] : 1; }
            if (findpassenger == null)
            { findpassenger = TempData["findpassenger"] != null ? (int)TempData["findpassenger"] : 0; }

            TempData["FlightDate"] = FlightDate;           
            TempData["selectRoute"] = selectRoute;
            TempData["findpassenger"] = findpassenger;
            //Так как переменная FlightDate является неопределённым типом, то записываем её сначала в ViewBag
            ViewBag.FlightDate1 = FlightDate;           
            ViewBag.selectRoute = selectRoute;
            ViewBag.findpassenger = findpassenger;
            string Departure;
            string Arrival;
            //Выводим дату рейса с названием месяца
            ViewBag.FlightDate = ViewBag.FlightDate1.ToLongDateString();            
            //Данная переменная необходима для отображения выбранной даты в datepicker
            ViewBag.FD = ViewBag.FlightDate1.Date.ToString("yyyy-MM-dd");
            DateTime ED = FlightDate.Value.AddDays(5);
            ViewBag.ED = ED.ToLongDateString();
            //если мы выбрали поиск пассажира, то нам придёт его ID иначе он равен 0
            if (findpassenger != 0)
            {

                ViewBag.findepassenger = db.Passengers.Where(i => i.id == findpassenger).Select(o => o.Surname + " " + o.Name + " " + o.Middlename).FirstOrDefault();

                //Ищем пассажира на всех рейсах и выбираем ID рейсов
              
                var findreglist = db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate >= FlightDate &&
                s.Flight.FlightDate <= ED && s.Passenger.id== findpassenger).ToList();

                return View(findreglist);
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
                   
                var reglist = Departure==""? db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate == FlightDate)
                .OrderByDescending(f => f.Flight.Departure == "Мирный" && f.Flight.Arrival == "Накын")
                .ThenByDescending(f => f.Flight.Departure == "Накын" && f.Flight.Arrival == "Мирный").ThenByDescending(f => f.Flight.Departure == "Нюрба" && f.Flight.Arrival == "Накын")
                .ThenByDescending(f => f.Flight.Departure == "Накын" && f.Flight.Arrival == "Нюрба").ThenByDescending(f => f.Flight.Departure == "Накын" && f.Flight.Arrival == "Вилюйск")
                .ThenByDescending(f => f.Flight.Departure == "Вилюйск" && f.Flight.Arrival == "Накын").ThenBy(f => f.Flight.FlightNum).ThenBy(s => s.Passenger.Surname).ToList() :
                db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate == FlightDate && s.Flight.Departure == Departure && s.Flight.Arrival == Arrival)
                            .OrderBy(n => n.Flight.FlightNum).ThenBy(s => s.Passenger.Surname).ToList();
                return View(reglist);

            }
        }
        #endregion

        //Поиск пассажира
        #region AutocompleteFindPassenger
        public ActionResult AutocompleteFindPassenger(string term)
        {
            var fio = db.Passengers.Where(p => (p.Surname + " " + p.Name + " " + p.Middlename).StartsWith(term))
                 .Select(p => new {
                     label = p.Surname + " " + p.Name.Trim() + " " + p.Middlename + " (Таб. №  " + p.EmployeeNum + ")  ",
                     value = p.Surname + " " + p.Name.Trim() + " " + p.Middlename,
                     id = p.id,
                 }).ToList();
            return Json(fio, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //Список рейсов
        #region Start       
        public ActionResult Start(DateTime? StartDate, DateTime? EndDate, DateTime? CurrentDate, int? choosDate, int? selectRoute, int? findpassenger)
        {
            if (StartDate == null)
            { StartDate = TempData["StartDate"] != null ? (DateTime)TempData["StartDate"] : DateTime.Now.Date; }
            if (EndDate == null)
            { EndDate = TempData["EndDate"] != null ? (DateTime)TempData["EndDate"] : DateTime.Now.Date.AddDays(5); }
            if (CurrentDate == null)
            { CurrentDate = TempData["CurrentDate"] != null ? (DateTime)TempData["CurrentDate"] : DateTime.Now.Date; }
            if (choosDate == null)
            { choosDate = TempData["choosDate"] != null ? (int)TempData["choosDate"] : 0; }
            if (selectRoute == null)
            { selectRoute = TempData["selectRoute"] != null ? (int)TempData["selectRoute"] : 0; }
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
            ViewBag.Notification = db.Notifications.Where(u => u.AspNetUserID == UId).Select(f => f.FlightID).ToList();
            //Для определения количества зарегистрированных пользователей  в спсике
            ViewBag.registrationlist = choosDate == 0 ? db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate >= StartDate && s.Flight.FlightDate <= EndDate).Select(s => s.FlightID).ToList() :
            db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate == CurrentDate).Select(s => s.FlightID).ToList();

            //если мы выбрали поиск пассажира, то нам придёт его ID иначе он равен 0
            if (findpassenger != 0)
            {

                ViewBag.findepassenger = db.Passengers.Where(i => i.id == findpassenger).Select(o => o.Surname + " " + o.Name + " " + o.Middlename).FirstOrDefault();

                //Ищем пассажира на всех рейсах и выбираем ID рейсов
                var find = choosDate == 0 ? db.RegistrationLists.Include(f => f.Flight).Where(s => s.Flight.FlightDate >= StartDate && s.Flight.FlightDate <= EndDate && s.PassengerID == findpassenger)
                    .Select(f => f.FlightID).ToList() : db.RegistrationLists.Include(f => f.Flight).Where(s => s.Flight.FlightDate == CurrentDate && s.PassengerID == findpassenger).Select(f => f.FlightID).ToList();

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
        public ActionResult Registration(int FlightID, string all = "no")
        {
            var flight = db.Flights.FirstOrDefault(d => d.id == FlightID);
            ViewBag.FlightDate = flight.FlightDate.ToLongDateString();        
            ViewBag.Departure = flight.Departure.Trim();
            ViewBag.Arrival = flight.Arrival.Trim();
            ViewBag.FD = flight.FlightDate;
            //Для изменения номера рейса при редактировании пользователя определяем список доступных рейсов в указанный день и выводим их в список
            DateTime fd = flight.FlightDate;
            string dp = ViewBag.Departure;
            string ar = ViewBag.Arrival;
            int fl = flight.FlightNum;         
            SelectList num = new SelectList(db.Flights.Where(d => d.FlightDate == fd && d.Departure == dp && d.Arrival == ar)
                                            .OrderBy(d => d.FlightNum).Select(d => new { id = d.id, flynumed = d.FlightNum }), "id", "flynumed", FlightID);
            ViewBag.FlightNumbers = num; //тут мы формируем список для отображения flighnum - это то, что получили из базы,         
                                         
           
            //Определяем список пассажиров на выбранном рейсе. Если мы выводим весь список то сортировать будем не по ID рейса, а по дате вылета и выводить всех пассажиров                                  
            var registrationlist = db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => all == "no" ? s.Flight.id == FlightID : s.Flight.FlightDate == fd && s.Flight.Departure == dp && s.Flight.Arrival == ar).OrderBy(s => s.Passenger.Surname);
           
            ViewBag.FlightInfo = db.Flights.Where(s => all == "no" ? s.id == FlightID : s.FlightDate == fd && s.Departure == dp && s.Arrival == ar).OrderBy(s => s.id).ToList();
            //Данная переменная необходима для редактирования выбранной строки на странице
            ViewBag.ID = FlightID;
            ViewBag.All = all;            
          
            //Для прикола проверка запроса AJAX
            if (Request.IsAjaxRequest())
            {
                return View(registrationlist);
            }

            return View(registrationlist);
        }
        #endregion

        //информация о пассажире
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

        //телефонный справочник
        #region PhoneBook
        public ActionResult Phonebook(int id = 0, int create =0)
        {
            ViewBag.id = id;
            ViewBag.Create = create;
            var phonebook = db.PhoneBooks.ToList();
            return View(phonebook);
        }
        #endregion

        //Добавление нового телефона
        #region AddPhone
        public JsonResult AddPhone(PhoneBook PhoneBooks, string confirm="no")
        {
            var phone = db.PhoneBooks.Where(p => p.TelNum == PhoneBooks.TelNum);
            if (phone.Count()!=0 && confirm=="no")
            {
                return Json("Введённый Вами телефон уже содержится в базе данных! Добавить его?", JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    db.PhoneBooks.Add(PhoneBooks);
                    db.SaveChanges();
                    return Json("Ok", JsonRequestBehavior.AllowGet);
                }
                return Json("Err", JsonRequestBehavior.AllowGet);
            }            
        }
        #endregion

        //редактирование телефона
        #region EditPhones
        public ActionResult EditPhones (PhoneBook phone)
        {
            if (ModelState.IsValid)
            {
                db.Entry(phone).State = EntityState.Modified;
                db.SaveChanges();
                return Json("Ok", JsonRequestBehavior.AllowGet);
            }
            return Json("Ошибка при редактировании справочника", JsonRequestBehavior.AllowGet);
        }
#endregion

        //удаление телефона
        #region DelPhone
        public JsonResult DelPhone(int? id)
        {
            if(id==null)
            {
                return Json("Ошибка определения ID номера телефона");
            }
            PhoneBook phone = db.PhoneBooks.Find(id);  
            if(phone==null)
            {
                return Json("Номер в базе данных не найден");
            }
            db.PhoneBooks.Remove(phone);
            db.SaveChanges();
            return Json("Ok");
        }
        #endregion

        //автозаполнение должности
        #region AutocompleteDepartment
        public ActionResult AutocompleteDepartment(string term)
        {
            var Department = db.PhoneBooks.Where(p => p.Department.StartsWith(term))
                .Select(p => new {
                    value = p.Department.Trim()
                }).Distinct().ToList();
            return Json(Department, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //автозаполнение по городу
        #region AutocompleteCity
        public ActionResult AutocompleteCity(string term)
        {
            var City = db.PhoneBooks.Where(p => p.City.StartsWith(term))
                .Select(p => new {
                    value = p.City.Trim()
                }).Distinct().ToList();
            return Json(City, JsonRequestBehavior.AllowGet);
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