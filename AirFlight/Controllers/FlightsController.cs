using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AirFlight.Models;
using System.Threading.Tasks;
using AirFlight.Filters;
using Microsoft.AspNet.Identity;
using System.IO;
using ClosedXML.Excel;

namespace AirFlight.Controllers
{
    [Authorize(Roles = "admin, aviadispatcher")]
    public class FlightsController : Controller
    {
        private AirFlightContext db = new AirFlightContext();
        //Регистрация рейсов
        #region FlightsList
        public ActionResult FlightsList(DateTime? StartDate, DateTime? EndDate, DateTime? CurrentDate, int? choosDate, int? selectRoute, int? findpassenger, int create = 0, int edit = 0, int idi = 0, int createtmpl = 0)
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
            if (selectRoute == null)
            { selectRoute = TempData["selectRoute"] != null ? (int)TempData["selectRoute"] : 0; }
            if (findpassenger == null)
            { findpassenger = TempData["findpassenger"] != null ? (int)TempData["findpassenger"] : 0; }

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
            //Флаг для добавления рейса и отображения частичного представления

            ViewBag.createtmpl = createtmpl;
            ViewBag.Create = create;
            ViewBag.IdI = edit == 1 ? idi : 0;

            //Для определения количества зарегистрированных пользователей  в спсике
            ViewBag.registrationlist = choosDate == 0 ? db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate >= StartDate && s.Flight.FlightDate <= EndDate).Select(s => s.FlightID).ToList() :
              db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate == CurrentDate).Select(s => s.FlightID).ToList();

            string UId = User.Identity.GetUserId();
            List<SelectListItem> ListTmlt = db.NameTemplates.Where(u => u.UserId == UId).Select(s => new SelectListItem { Value = s.Num.ToString(), Text = s.Description }).OrderBy(u => u.Value).ToList();
            ListTmlt.Insert(0, new SelectListItem { Value = "0", Text = "Выберите шаблон" });
            ViewBag.TmplList = new SelectList(ListTmlt, "Value", "Text");

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

        //Регистрация пассажиров
        #region Registration 
        public ActionResult Registration(int FlightID, int AddPassenger = 0, int idi = 0, int edit = 0, int createtmpl = 0, string all = "no")
        {
            var flight = db.Flights.Find(FlightID);

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
            SelectList num = new SelectList(db.Flights.Where(d => d.FlightDate == fd && d.Departure == dp && d.Arrival == ar)
                                            .OrderBy(d => d.FlightNum).Select(d => new { id = d.id, flynumed = d.FlightNum }), "id", "flynumed", FlightID);

            ViewBag.FlightNumbers = num;
            ViewBag.AddPassenger = AddPassenger == 0 ? 0 : 1;
            //Определяем список пассажиров на выбранном рейсе. Если мы выводим весь список то сортировать будем не по ID рейса, а по дате вылета и выводить всех пассажиров                                  
            var registrationlist = db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => all == "no" ? s.Flight.id == FlightID : s.Flight.FlightDate == fd && s.Flight.Departure == dp && s.Flight.Arrival == ar).OrderBy(s => s.Passenger.Surname);
            //ViewBag.FlightNumS (ViewBag.FlightInfo)  -данная переменная необходима для перечисления рейсов в цикле foreach
            //ViewBag.FlightNumS = db.Flights.Where(s => s.FlightDate == fd && s.Departure == dp && s.Arrival == ar).Select(s => all == "yes" ? s.FlightNum : fl).OrderBy(s => s).ToList().Distinct();
            //Данная переменная необходима для редактирования выбранной строки на странице           
            ViewBag.FlightInfo = db.Flights.Where(s => all == "no" ? s.id == FlightID : s.FlightDate == fd && s.Departure == dp && s.Arrival == ar).OrderBy(s => s.id).ToList();

            ViewBag.ID = FlightID;
            ViewBag.All = all;
            //если редактирование не выбрано
            ViewBag.Edit = edit == 0 ? 0 : 1;
            ViewBag.IDI = edit == 0 ? 0 : idi;
            ViewBag.createtmpl = createtmpl;
            string UId = User.Identity.GetUserId();
            List<SelectListItem> ListTmltPass = db.TemplatesNamePassengers.Where(u => u.UserId == UId).Select(s => new SelectListItem { Value = s.Num.ToString(), Text = s.Description }).OrderBy(u => u.Value).ToList();
            ListTmltPass.Insert(0, new SelectListItem { Value = "0", Text = "Выберите шаблон" });
            ViewBag.TmplListPass = new SelectList(ListTmltPass, "Value", "Text");


            return View(registrationlist);

        }
        #endregion

        //Добавление рейса
        #region CreateFly
        [Log]
        public ActionResult CreateFly(Flight flight)
        {
            if (db.Flights.Where(s => s.FlightDate == flight.FlightDate && s.Departure == flight.Departure && s.Arrival == flight.Arrival && s.FlightNum == flight.FlightNum).Count() > 0)
            {
                return Json("Такой рейс уже существует!");
            }

            if (ModelState.IsValid)
            {
                db.Flights.Add(flight);
                db.SaveChanges();
                return Json("Ok");
            }
            return Json("Err");
        }
        #endregion

        //Добавление рейсов по шаблону
        #region CreateFlyTmpl
        [Log]
        public JsonResult CreateFlyTmpl(DateTime FlightDate, int Tmplnum)
        {
            int skip = 0; //счётчик числа пропущенных рейсов

            string UId = User.Identity.GetUserId();
            var tmpl = db.Templates.Where(u => u.UserId == UId && u.Num == Tmplnum);
            if (tmpl.Count() == 0)
            {
                return Json("В выбранной шаблоне нет рейсов для добавления!", JsonRequestBehavior.AllowGet);
            }
            using (AirFlightContext db = new AirFlightContext())
            {
                Flight flight = new Flight();
                foreach (var listtmpl in tmpl)
                {
                    var flyerr = db.Flights.Where(s => s.FlightDate == FlightDate && s.Departure == listtmpl.Departure && s.Arrival == listtmpl.Arrival && s.FlightNum == listtmpl.FlightNum);
                    if (flyerr.Count() != 0)
                    {
                        skip++; //увеличиваем счтчик пропусков
                        continue;
                    }
                    else
                    {
                        flight.FlightDate = FlightDate;
                        flight.FlightTime = listtmpl.FlightTime;
                        flight.FlightNum = (short)listtmpl.FlightNum;
                        flight.Departure = listtmpl.Departure;
                        flight.Arrival = listtmpl.Arrival;
                        flight.AirType = listtmpl.AirType;
                        flight.Approved = listtmpl.Approved;
                        flight.Number = listtmpl.Number;
                        flight.FlightTimeOld = listtmpl.FlightTime;
                        flight.AirTypeOld = listtmpl.AirType;

                        db.Flights.Add(flight);
                        db.SaveChanges();
                    }
                }
            }

            if (skip == 0)
            {
                return Json("Ok", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Из-за совпадения не доавлено рейсов: " + skip, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        //Добавление пассажира
        #region Create       
        public ActionResult Create(RegistrationList registrationList, string confirm = "no")
        {
            //Определяем информацию о рейсе
            var date = db.Flights.Find(registrationList.FlightID);
            DateTime DF = date.FlightDate;
            string dp = date.Departure;
            string ar = date.Arrival;
            //Находим все рейсы на найденную дату
            var fl = db.Flights.Where(s => s.FlightDate == DF);
            //Проверяем есть ли такой пассажир на других рейсах в выбранную дату            
            var pfromto = db.RegistrationLists.Include(f => f.Flight).Where(s => s.Flight.FlightDate == DF && s.Flight.Departure == dp && s.Flight.Arrival == ar && s.PassengerID == registrationList.PassengerID);

            if (pfromto.Count() > 0)
            {
                var pany = db.RegistrationLists.Include(f => f.Flight).FirstOrDefault(s => s.Flight.FlightDate == DF && s.Flight.Departure == dp && s.Flight.Arrival == ar && s.PassengerID == registrationList.PassengerID);
                return Json("Этот пассажир уже зарегистрирован на рейсе № " + pany.Flight.FlightNum);

            }

            //Проверяем есть ли такой пассажир на встречных рейсах в выбранную дату           
            var passengerany = db.RegistrationLists.Include(f => f.Flight).Where(s => s.Flight.FlightDate == DF && s.PassengerID == registrationList.PassengerID);
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
                    var passengerfromto = db.RegistrationLists.Include(f => f.Flight).FirstOrDefault(s => s.Flight.FlightDate == DF && s.PassengerID == registrationList.PassengerID);
                    return Json("Этот пассажир уже зарегистрирован на рейсе " + passengerfromto.Flight.Departure.Trim() + " - " + passengerfromto.Flight.Arrival.Trim() + ", рейс №" + passengerfromto.Flight.FlightNum + " ! Добавить его?");
                }
            }

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
                    (qt - total) + ") на рейсе! Хотите выбрать пассажиров для добавления или всеравно добавить всех?"
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

        //Сохранение при изменении рейса
        #region EditFly        
        [Log]
        public ActionResult EditFly(int id, TimeSpan FlightTime, int? FlightNum, string Departure, string Arrival, string AirType, string Approved, int? Number, string Note)
        {
            var Flights = db.Flights.Find(id);
            //Проверяем примечание на пустую строку
            string NoteSave = string.IsNullOrWhiteSpace(Note) ? null : Note;

            //Если регистрация была закрыта и прилетают изменения
            if (Flights.Approved.Trim() == "Закрыта")
            {
                if (Flights.SendingSms == null)
                {
                    Flights.FlightTime = FlightTime;
                    Flights.AirType = AirType;
                    Flights.Approved = Approved;
                    Flights.Note = NoteSave;

                    if (ModelState.IsValid)
                    {
                        db.Entry(Flights).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    return Json("Ok", JsonRequestBehavior.AllowGet);
                }
                //если смс была выслана
                else
                {
                    //сохраняем сначала старые значения потом новые
                    Flights.FlightTimeOld = Flights.FlightTime == FlightTime ? FlightTime : Flights.FlightTime;
                    Flights.FlightTime = FlightTime;
                    Flights.AirTypeOld = Flights.AirType == AirType ? AirType : Flights.AirType;
                    Flights.AirType = AirType;
                    Flights.Approved = Approved;
                    Flights.NoteOld = Flights.Note == NoteSave ? NoteSave : Flights.Note;
                    Flights.Note = NoteSave;

                    //Если прилетели изменения то выставляем флаг
                    if (Flights.FlightTimeOld != FlightTime || Flights.AirTypeOld != AirType || Flights.NoteOld != NoteSave)
                    {
                        Flights.HaveChange = true;

                        db.Entry(Flights).State = EntityState.Modified;
                        db.SaveChanges();

                        //выставляем флаг в таблице регистрации что у пассажира на рейсе возникли изменения
                        var sendchange = db.RegistrationLists.Where(f => f.FlightID == id && f.SendingSms == true);
                        foreach (var havechange in sendchange)
                        {
                            using (AirFlightContext db = new AirFlightContext())
                            {
                                var savelist = db.RegistrationLists.Find(havechange.id);
                                savelist.HaveChange = true;
                                db.Entry(savelist).State = EntityState.Modified;
                                db.SaveChanges();
                            }

                        }

                        return Json("Ok", JsonRequestBehavior.AllowGet);
                    }

                    db.Entry(Flights).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json("Ok", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                Flights.FlightTime = FlightTime;
                Flights.FlightNum = (short)FlightNum;
                Flights.Departure = Departure;
                Flights.Arrival = Arrival;
                Flights.AirType = AirType;
                Flights.Approved = Approved;
                Flights.Number = (int)Number;
                Flights.Note = NoteSave;


                if (ModelState.IsValid)
                {
                    db.Entry(Flights).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json("Ok", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Err", JsonRequestBehavior.AllowGet);
                }
            }

        }
        #endregion

        //закрытие регистрации из контекстного меню
        #region EditRegistration        
        [Log]
        public JsonResult EditRegistration(int id, string Approved)
        {
            var Flights = db.Flights.Find(id);
            Flights.Approved = Approved;

            if (ModelState.IsValid)
            {
                db.Entry(Flights).State = EntityState.Modified;
                db.SaveChanges();
                return Json("Ok", JsonRequestBehavior.AllowGet);
            }

            return Json("Ошибка закрытия регистрации!", JsonRequestBehavior.AllowGet);
        }
        #endregion

        //Редактирование пассажира
        #region Edit
        [Log]
        public ActionResult Edit(int id, string EditorID, string Note, int FlightID)
        {
            var registrationList = db.RegistrationLists.Find(id);
            registrationList.ChangeNum = registrationList.FlightID != FlightID ? true : false;
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
        #endregion

        //Удаление рейса
        #region DeleteFly       
        [Log]
        public ActionResult DeleteFly(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flight flight = db.Flights.Find(id);
            if (flight == null)
            {
                return HttpNotFound();
            }

            db.Flights.Remove(flight);
            db.SaveChanges();

            return Json("Ok", JsonRequestBehavior.AllowGet);
        }
        #endregion

        //удаление пассажира
        #region Delete       
        [Log]
        public ActionResult Delete(int id, int FlightID, string all)
        {
            RegistrationList registrationList = db.RegistrationLists.Find(id);

            if (registrationList == null)
            {
                return HttpNotFound();
            }


            db.RegistrationLists.Remove(registrationList);
            db.SaveChanges();

            return Json(Url.Action("Registration", new { FlightID = FlightID, all = all }), JsonRequestBehavior.AllowGet);
        }
        #endregion

        //Выбор рейсов для печати
        #region ChoosePrintFlight       
        public ActionResult ChoosePrintFlights(DateTime? StartDate, DateTime? EndDate, DateTime? CurrentDate, int? choosDate, int? sortDate)
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

            TempData["StartDate"] = StartDate;
            TempData["EndDate"] = EndDate;
            TempData["CurrentDate"] = CurrentDate;
            TempData["choosDate"] = choosDate;
            TempData["sortDate"] = sortDate;

            //Так как переменная FlightDate является неопределённым типом, то записываем её сначала в ViewBag
            ViewBag.StartDate1 = StartDate;
            ViewBag.EndDate1 = EndDate;
            ViewBag.CurrentDate1 = CurrentDate;
            ViewBag.ChoosDate = choosDate;
            ViewBag.SortDate = sortDate;
            //Выводим дату рейса с названием месяца
            ViewBag.StartDate = ViewBag.StartDate1.ToLongDateString();
            ViewBag.EndDate = ViewBag.EndDate1.ToLongDateString();
            ViewBag.CurrentDate = ViewBag.CurrentDate1.ToLongDateString();
            //Данная переменная необходима для отображения выбранной даты в datepicker
            ViewBag.SD = ViewBag.StartDate1.Date.ToString("yyyy-MM-dd");
            ViewBag.ED = ViewBag.EndDate1.Date.ToString("yyyy-MM-dd");
            ViewBag.CD = ViewBag.CurrentDate1.ToString("yyyy-MM-dd");

            //Для определения количества зарегистрированных пользователей  в спсике
            ViewBag.registrationlist = choosDate == 0 ? db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate >= StartDate && s.Flight.FlightDate <= EndDate).Select(s => s.FlightID).ToList() :
                db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate == CurrentDate).Select(s => s.FlightID).ToList();

            //Определяем все открытые регистрации за выбранный период 
            ViewBag.openreg = choosDate == 0 ? db.Flights.Where(s => s.FlightDate >= StartDate && s.FlightDate <= EndDate && (s.Approved.Trim() == "Открыта" || s.Approved.Trim() == "Ограничена")).ToList() :
                                                       db.Flights.Where(s => s.FlightDate == CurrentDate && (s.Approved.Trim() == "Открыта" || s.Approved.Trim() == "Ограничена")).ToList();

            //Этоу модель используем для сгруппированных рейсов
            var flyghts = choosDate == 0 ? db.RegistrationLists.Include(f => f.Flight).Where(s => s.Flight.FlightDate >= StartDate && s.Flight.FlightDate <= EndDate).OrderBy(s => s.Flight.FlightDate).ToList() :
                                         db.RegistrationLists.Include(f => f.Flight).Where(s => s.Flight.FlightDate == CurrentDate).ToList();

            return View(flyghts);
        }
        #endregion

        //печать рейсов
        #region PrintFlight
        public ActionResult PrintFlights(int? sortDate, DateTime? FlightDate, int? FlightID, string Departure, string Arrival)
        {
            //для изменения подписи в списках на вылет определяем текущего пользователя и из поля подпись берём значение
            using (ApplicationDbContext dba = new ApplicationDbContext())
            {
                ViewBag.Signature = dba.Users.Find(User.Identity.GetUserId()).Signature;
            }
            if (sortDate == 0)
            {
                var printlist = db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.FlightID == FlightID).OrderBy(s => s.Passenger.Surname);
                ViewBag.FlightNumS = printlist.Select(s => s.FlightID).Distinct().ToList();
                // ViewBag.FlightInfo = db.Flights.Where(s => s.id == FlightID).ToList();
                ViewBag.count = printlist.Select(s => s.FlightID).Distinct().Count();
                return View(printlist);
            }

            else
            {
                var printlist = db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate == FlightDate && s.Flight.Departure == Departure && s.Flight.Arrival == Arrival).OrderBy(s => s.Passenger.Surname);
                ViewBag.FlightNumS = printlist.Select(s => s.FlightID).Distinct().ToList();
                //ViewBag.FlightInfo = db.Flights.Where(s => s.FlightDate == FlightDate && s.Departure == Departure && s.Arrival == Arrival).OrderBy(s => s.id).ToList();
                ViewBag.count = printlist.Select(s => s.FlightID).Distinct().Count();
                return View(printlist);
            }

        }
        #endregion

        //автозаполнение при поиске пассажира
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

        //автозаполнение по ФИО
        #region AutocompleteFIO
        public ActionResult AutocompleteFIO(string term)
        { //Если необходимо найти любой символ в последовательности но модно использовать Contains вместо StartsWith
            var fio = db.Passengers.Where(p => p.Surname.StartsWith(term))
                .Select(p => new
                {
                    id = p.id,
                    value = p.Surname.Trim() + " " + p.Name.Trim() + " " + p.Middlename.Trim(),
                    EmployeeNum = p.EmployeeNum,
                    Passport = p.Passport
                }).ToList();
            return Json(fio, JsonRequestBehavior.AllowGet);
        }

        #endregion

        //автозаполнение поля по табельному номеру
        #region AutocompleteEmplNum  
        public ActionResult AutocompleteEmplNum(string term)
        {

            var EmplNum = db.Passengers.Where(p => p.EmployeeNum.StartsWith(term))
                .Select(p => new
                {
                    id = p.id,
                    fio = p.Surname.Trim() + " " + p.Name.Trim() + " " + p.Middlename.Trim(),
                    value = p.EmployeeNum,
                    Passport = p.Passport
                }).ToList();
            return Json(EmplNum, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //автозаполнение поля по паспортным данным
        #region AutocompletePassport       
        public ActionResult AutocompletePassport(string term)
        {
            var Passport = db.Passengers.Where(p => p.Passport.StartsWith(term))
                .Select(p => new
                {
                    id = p.id,
                    fio = p.Surname.Trim() + " " + p.Name.Trim() + " " + p.Middlename.Trim(),
                    EmployeeNum = p.EmployeeNum,
                    value = p.Passport
                }).ToList();

            return Json(Passport, JsonRequestBehavior.AllowGet);
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

        //шаблоны авиадиспетчера с рейсами
        #region Templates

        public ActionResult Templates(int? NumTab, int id = 0, int edit = 0, int create = 0, int edittmpl = 0)
        {
            if (NumTab == null)
            { NumTab = TempData["NumTab"] != null ? (int)TempData["NumTab"] : 1; }
            TempData["NumTab"] = NumTab;
            ViewBag.NumTab = NumTab;

            //edit == 1 ? присваиваем ViewBag.ID =0 если не было команды на редакитрование
            ViewBag.ID = edit == 1 ? id : 0;
            ViewBag.Create = create;
            ViewBag.edittmpl = edittmpl;
            string UId = User.Identity.GetUserId();
            ViewBag.NameTabTmpl = db.NameTemplates.Where(u => u.UserId == UId).ToList();

            int NameTabNum = db.NameTemplates.Where(u => u.UserId == UId).Select(n => n.Num) == null ? 0 : db.NameTemplates.Where(u => u.UserId == UId).Select(n => n.Num).OrderBy(n => n).ToList().LastOrDefault();
            ViewBag.NameTabNum = NameTabNum + 1;

            var temlates = db.Templates.Where(u => u.UserId == UId).ToList();

            return View(temlates);
        }
        #endregion

        //Добавление названия шаблона (закладки)
        #region AddTmplName

        public JsonResult AddTmplName(NameTemplate NameTmpl)
        {
            if (db.NameTemplates.Where(u => u.UserId == NameTmpl.UserId && u.Description == NameTmpl.Description).Count() > 0)
            {
                return Json("Такой шаблон уже существует, введите другое название!", JsonRequestBehavior.AllowGet);
            }

            if (ModelState.IsValid)
            {
                db.NameTemplates.Add(NameTmpl);
                db.SaveChanges();
                return Json("Ok", JsonRequestBehavior.AllowGet);
            }

            return Json("Ошибка сохранения!", JsonRequestBehavior.AllowGet);

        }

        #endregion

        //Удаляем шаблон с рейсами
        #region DelTmplName

        public JsonResult DelTmplName(int Id)
        {
            //сначала удалим всё содержимое в шаблонах
            string UId = User.Identity.GetUserId();
            int Num = db.NameTemplates.Find(Id).Num;
            var tmpl = db.Templates.Where(u => u.Num == Num && u.UserId == UId);
            //если есть содержимое шаблона, то удаляем сначала его
            if (tmpl != null)
            {
                foreach (var remtml in tmpl)
                {
                    Template template = db.Templates.Find(remtml.Id);
                    db.Templates.Remove(template);
                }
                db.SaveChanges();

            }
            //Находим сам шаблон
            NameTemplate NameTmpl = db.NameTemplates.Find(Id);
            if (NameTmpl == null)
            {
                return Json("Не найден шаблон для удаления!", JsonRequestBehavior.AllowGet);
            }

            db.NameTemplates.Remove(NameTmpl);
            db.SaveChanges();

            return Json("Ok", JsonRequestBehavior.AllowGet);

        }
        #endregion

        //Сохранение при изменении шаблона
        #region SaveTmplName       
        public JsonResult SaveTmplName(NameTemplate NameTmpl)
        {
            var editnametmpl = db.NameTemplates.Find(NameTmpl.Id);
            editnametmpl.Description = NameTmpl.Description;

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
        #region AddTemplates

        public JsonResult AddTemplates(Template Teplates)
        {
            string UId = User.Identity.GetUserId();
            if (db.Templates.Where(s => s.UserId == UId && s.Num == Teplates.Num && s.Departure == Teplates.Departure && s.Arrival == Teplates.Arrival && s.FlightNum == Teplates.FlightNum).Count() > 0)
            {
                return Json("Такой рейс уже существует!", JsonRequestBehavior.AllowGet);
            }

            if (ModelState.IsValid)
            {
                db.Templates.Add(Teplates);
                db.SaveChanges();
                return Json("Ok", JsonRequestBehavior.AllowGet);
            }

            return Json("Ошибка", JsonRequestBehavior.AllowGet);
        }
        #endregion

        //удаление шаблонов рейсов
        #region DeleteTemplates
        public JsonResult DeleteTemplates(int id)
        {
            Template tmpl = db.Templates.Find(id);
            if (tmpl == null)
            {
                return Json("Не найден рейс для удаления!", JsonRequestBehavior.AllowGet);
            }
            db.Templates.Remove(tmpl);
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

            var temlates = db.TemplatesPassengers.Include(p => p.Passenger).Where(u => u.UserId == UId).OrderBy(u => u.Passenger.Surname).ToList();

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

        //удалить шаблон с пассажирами
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

        //редактирование шаблона с рейсами
        #region EditTemplates 
        public JsonResult EditTemplates(Template tmpl)
        {
            var edittmpl = db.Templates.Find(tmpl.Id);
            edittmpl.FlightTime = tmpl.FlightTime;
            edittmpl.FlightNum = tmpl.FlightNum;
            edittmpl.Departure = tmpl.Departure;
            edittmpl.Arrival = tmpl.Arrival;
            edittmpl.AirType = tmpl.AirType;
            edittmpl.Approved = tmpl.Approved;
            edittmpl.Number = tmpl.Number;

            if (ModelState.IsValid)
            {
                db.Entry(edittmpl).State = EntityState.Modified;
                db.SaveChanges();
                return Json("Ok", JsonRequestBehavior.AllowGet);
            }

            return Json("Ошибка", JsonRequestBehavior.AllowGet);
        }

        #endregion

        //Загрузка файлов в базу
        #region UploadFiles
        public JsonResult UploadDocs(int flightid, HttpPostedFileBase uploadfile)
        {
            if (flightid != 0 && uploadfile != null && uploadfile.ContentLength > 0)
            {
                var fly = db.Flights.Find(flightid);
                //если в базе уже имеется документ то мы выводим ошибку
                if (!string.IsNullOrWhiteSpace(fly.DocsName))
                {
                    return Json("На данный рейс файл уже загружен!", JsonRequestBehavior.AllowGet);
                }
                //сохранение документа
                else
                {
                    try
                    {
                        string filename = string.Format("{0} {1}-{2} рейс №{3}.pdf", fly.FlightDate.ToShortDateString(), fly.Departure, fly.Arrival, fly.FlightNum);
                        string path = Path.Combine("D:/DB/AirFlightFiles/FlyPdf/", filename);
                        uploadfile.SaveAs(path);

                        //сохраняем в таблице flyght имя файла
                        fly.DocsName = filename;
                        db.Entry(fly).State = EntityState.Modified;
                        db.SaveChanges();

                        return Json("Ok", JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception)
                    {
                        return Json("Err", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                return Json("Файл не выбран!", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        //получаем имя файло до его сохранения
        #region  GetNewFileName
        public JsonResult GetNewFileName(int flightid = 0, int filename = 0)
        {
            if (filename != 0 && flightid != 0)
            {
                var fly = db.Flights.Find(flightid);
                string newname = string.Format("{0} {1}-{2} рейс №{3}.pdf", fly.FlightDate.ToShortDateString(), fly.Departure, fly.Arrival, fly.FlightNum);
                return Json(newname, JsonRequestBehavior.AllowGet);
            }
            return Json("Ошибка получения нового имени файла", JsonRequestBehavior.AllowGet);
        }
        #endregion

        //удаление файла
        #region DeleteFile
        public JsonResult DeleteFile(int flightid)
        {
            var fly = db.Flights.Find(flightid);
            if (fly != null)
            {
                string path = Path.Combine("D:/DB/AirFlightFiles/FlyPdf/", fly.DocsName);
                System.IO.File.Delete(path);

                fly.DocsName = null;
                db.Entry(fly).State = EntityState.Modified;
                db.SaveChanges();
                return Json("Ok", JsonRequestBehavior.AllowGet);
            }
            return Json("Файл не найден!", JsonRequestBehavior.AllowGet);
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
                Response.AppendHeader("Content-Disposition", "inline; filename=" + fly.DocsName);
                return File(file, filetype, fly.DocsName);
            }
            else
            {
                return File("Файл не найден!", "application/pdf");
            }

        }
        #endregion

        //Экспорт в Excel
        #region ExportExcel
        public FileResult ExportExcel(int? sortDate, DateTime? FlightDate, int? FlightID, string Departure, string Arrival)
        {
            var passengerlist = db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger);
            string FileName;
            int i = 1;
            if (sortDate == 0)
            {
               passengerlist = passengerlist.Where(s => s.FlightID == FlightID).OrderBy(s => s.Passenger.Surname);
               FileName = passengerlist.FirstOrDefault().Flight.FlightDate.ToString("d") + " " + passengerlist.FirstOrDefault().Flight.Departure + "-" + passengerlist.FirstOrDefault().Flight.Arrival+" Рейс №"+
                    passengerlist.FirstOrDefault().Flight.FlightNum+".xlsx";
            }
            else
            {
                passengerlist = passengerlist.Where(s => s.Flight.FlightDate == FlightDate && s.Flight.Departure == Departure && s.Flight.Arrival == Arrival).OrderBy(s => s.Passenger.Surname);
                FileName = passengerlist.FirstOrDefault().Flight.FlightDate.ToString("d") + " " + passengerlist.FirstOrDefault().Flight.Departure + "-" + passengerlist.FirstOrDefault().Flight.Arrival + " Все рейсы.xlsx";
            }
            //Формируем выводимую таблицу, тут можно использовать несколько вариантов, например https://www.aspsnippets.com/Articles/ClosedXML-MVC-Example-Export-to-Excel-using-ClosedXML-in-ASPNet-MVC.aspx
            //я использую вот этот https://github.com/closedxml/closedxml/wiki/Inserting-Tables
            //WsEx - описан ниже
            var dt = new List<WsEx>();
            foreach (var passenger in passengerlist)
            {
                dt.Add(new WsEx() { Num = i++, Fio = passenger.Passenger.Surname + " " + passenger.Passenger.Name + " " + passenger.Passenger.Middlename, Passprt = passenger.Passenger.Passport });
            }
          
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Список пассажиров");
          
            ws.Cell(1, 1).Value = "Список пассажиров вылетающих "+ FileName.Substring(0,FileName.Length-5);
            ws.Row(1).Height = 30;
            //Оформить стиль ячейки можно в один шот - Вариант 1
            ws.Cell(1, 1).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment.SetVertical(XLAlignmentVerticalValues.Center).Font.SetFontSize(14);
            //Объеденяем ячейки с названием таблицы           
            ws.Range(1, 1, 1, 3).Merge().AddToNamed("Titles");     
            //присваиваем имена заголовкам - если будем использовать ws.Cell(2, 1).InsertTable(dt) тогда это необходимо закоментировать
            ws.Cell(2, 1).Value = "№";
            ws.Cell(2, 2).Value = "Фамилия Имя Отчество";            
            ws.Cell(2, 3).Value = "Документ";
            //Или оформить содержимое ячейки можно в несколько шотов - вариант 2
            var rngColumns = ws.Range("A2:C2");
            rngColumns.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            rngColumns.Style.Font.Bold = true;
            //Присваиваем цвет ячейке
            rngColumns.Style.Fill.BackgroundColor = XLColor.BallBlue;
            //обводим ячейки с названием столбцов
            rngColumns.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rngColumns.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            //**возвращает таблицу без имён столбцов**//  
            //Обводим каждую ячейку в таблице бордюром
            ws.Cell(3, 1).InsertData(dt.AsEnumerable()).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border.SetInsideBorder(XLBorderStyleValues.Thin);
            //вернёт таблицу с сортировкой столбцов, в таком случае нам не нужно прописывать заголовки для столбцов
            //ws.Cell(2, 1).InsertTable(dt); 
            //**выравниваем содержимое ячеек по содержимому**//
            //ws.Columns().AdjustToContents(); - выровнять все ячейки по содержимому            
            ws.Columns().AdjustToContents();
            //изменить ширину отдельного столбца
            ws.Column(2).Width = 65;
                         
            using (MemoryStream stream = new MemoryStream())
            {
                wb.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", FileName);
            }
            
        }
        //класс для таблицы экспорта в excel
        class WsEx
        {
            public int Num { get; set; }
            public string Fio { get; set; }
            public string Passprt { get; set; }
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
