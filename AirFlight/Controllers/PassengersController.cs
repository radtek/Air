using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AirFlight.Models;
using System.Globalization;
using AirFlight.Filters;

namespace AirFlight.Controllers
{
    [Log]
    [Authorize(Roles = "registrator")]   
    public class PassengersController : Controller
    {
  
        private AirFlightContext db = new AirFlightContext();

        // GET: Passengers
        public ActionResult AddPassenger()
        {
            //Так как список с местом жительства мы формируем из базы, то я не стал добавлять туда отдельные колонки Value и Text (необходимые для отображения DropDownList из SelectList)
            //то сделал тут запрос, если имя равно "Не указано" то присваиваем ему значение value в списке "" (пустую строку), а текст выводим как "Не указано"
           SelectList residense = new SelectList(db.Residenses.Select(d => new { text = d.Name.Trim(), value = d.Name.Trim() == "Не указано" ? "" : d.Name.Trim() }), "value", "text");
    
            ViewBag.Residenses = residense; //тут мы формируем список для отображения flighnum - это то, что получили из базы,  
            return View();
        }

        //Сохранение при добавлении пассажира          
        public async Task<ActionResult> SaveAddPassenger(Passenger passenger, string confirmphone="no")
        {
            var passport = db.Passengers.Where(s => s.Passport == passenger.Passport); //проверяем есть ли такие пасспортные данные
            //проверяем пасспорныте данные
            if (passport.Count() >0)
            {                         
                return Json("Паспортные данные " + passenger.Passport + " совпадают с данными сотрудника "
                   + passport.FirstOrDefault().Surname + " " + passport.FirstOrDefault().Name + " " + passport.FirstOrDefault().Middlename
                   + ". Пожалуйста, проверьте правильность заполнения!");
            }
            //проверяем табельный номер
            var emlno = db.Passengers.Where(s => s.EmployeeNum == passenger.EmployeeNum); //проверяем есть ли такой табельный номер 
            
            if (emlno.Count() >0 && passenger.EmployeeNum!=null)
            {                              
                return Json("Табельный номер " + passenger.EmployeeNum + " совпадает с данными сотрудника "
                        + emlno.FirstOrDefault().Surname + " " + emlno.FirstOrDefault().Name + " " + emlno.FirstOrDefault().Middlename
                        + ". Пожалуйста, проверьте правильность заполнения!");
            }

            var phone = db.Passengers.Where(p => p.PhoneNum == passenger.PhoneNum);

            if(phone.Count()>0 && passenger.PhoneNum!=null)
            {
                if (confirmphone == "yes")
                {
                    if (ModelState.IsValid)
                    {
                        db.Passengers.Add(passenger);
                        await db.SaveChangesAsync();
                        return Json("Сохранено");
                    }

                    return View(passenger);
                }
                else
                {
                    return Json("Номер телефона " + passenger.PhoneNum + " совпадает с номеро телефона сотрудника "+ phone.FirstOrDefault().Surname + " "
                        + phone.FirstOrDefault().Name + " " + phone.FirstOrDefault().Middlename + ". Вы действительно хотите сохранить его?");
                }
            }

            if (ModelState.IsValid)
            {
                db.Passengers.Add(passenger);
                await db.SaveChangesAsync();
                return Json("Сохранено");
            }

            return View(passenger);
        }

        //Редактирование пассажира
        public ActionResult EditPassenger()
        {
            //Так как список с местом жительства мы формируем из базы, то я не стал добавлять туда отдельные колонки Value и Text (необходимые для отображения DropDownList из SelectList)
            //то сделал тут запрос, если имя равно "Не указано" то присваиваем ему значение value в списке "" (пустую строку), а текст выводим как "Не указано"
             SelectList residense = new SelectList(db.Residenses.Select(d=> new {text =d.Name.Trim(), value=d.Name.Trim()=="Не указано"?"":d.Name.Trim()}), "value", "text");
            
            ViewBag.Residenses = residense;
            return View();
        }

        //Сохранение при изменении
        public async Task<ActionResult> SaveEditPassenger(Passenger passenger, string confirmphone = "no")
        {
            var passported = db.Passengers.Where(s => s.Passport == passenger.Passport); //ищем пассажира по паспортным данным
            if (passported.Count() >0 && passported.FirstOrDefault().id != passenger.id)
            {
                return Json("Паспортные данные " + passenger.Passport + " совпадают с данными пассажира "
                    + passported.FirstOrDefault().Surname + " " + passported.FirstOrDefault().Name + " " + passported.FirstOrDefault().Middlename
                    + ". Пожалуйста, проверьте правильность заполнения!");
            }

            if (passenger.EmployeeNum != null)            {
                var emplnum = db.Passengers.Where(s => s.EmployeeNum == passenger.EmployeeNum); //ищем пассажира по табельному номеру

                if (emplnum.Count() > 0 && emplnum.FirstOrDefault().id != passenger.id)
                {
                    return Json("Табельный номер " + passenger.EmployeeNum + " совпадает с данными пассажира "
                        + emplnum.FirstOrDefault().Surname + " " + emplnum.FirstOrDefault().Name + " " + emplnum.FirstOrDefault().Middlename
                        + ". Пожалуйста, проверьте правильность заполнения!");
                }
            }

            if(passenger.PhoneNum!=null)
            { 
            var phone = db.Passengers.Where(p => p.PhoneNum == passenger.PhoneNum);

                if (phone.Count() > 0 && phone.FirstOrDefault().id != passenger.id)
                {
                    if (confirmphone == "yes")
                    {
                        using (AirFlightContext db = new AirFlightContext())
                        {
                            if (ModelState.IsValid)
                            {
                                db.Entry(passenger).State = EntityState.Modified;
                                await db.SaveChangesAsync();
                                return Json("Изменено");
                            }
                        }

                        return View(passenger);

                    }
                    else
                    {
                        return Json("Номер телефона " + passenger.PhoneNum + " совпадает с номеро телефона сотрудника " + phone.FirstOrDefault().Surname + " "
                            + phone.FirstOrDefault().Name + " " + phone.FirstOrDefault().Middlename + ". Вы действительно хотите сохранить его?");
                    }
                }

            }
            //using необходим для сохранения изменений. Так как мы делаем проверку данных выше, то сущность блокируется и выставить ей
            //статус Modified не получится. Поэтому тут явно указываем dbContext для сохранения изменений
            using (AirFlightContext db = new AirFlightContext())
            {
                if (ModelState.IsValid)
                {
                    db.Entry(passenger).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return Json("Изменено");
                }
            }

            return Json("Ошибка заполнения полей данных!");
         
        }

        //удаление пассажира
        public async Task<ActionResult> DeletePassenger(int id)
        {
            Passenger passenger = await db.Passengers.FindAsync(id);
            db.Passengers.Remove(passenger);
            await db.SaveChangesAsync();
            return Json("Удалено");
        }


        //автозаполнение по участку
        public ActionResult AutocompleteDepartment(string term)
        { 
            var department = db.Passengers.Where(p => p.Department.StartsWith(term))
                .Select(p => new {                   
                    label = p.Department.Trim()+" - "+p.Site.Trim()+" - "+p.Company.Trim(), //label используется для отображения в выпадающем списке
                    value = p.Department.Trim(), //value заполняет поле при выборе                   
                    Site = p.Site.Trim(),
                    Company = p.Company.Trim()
                }).Distinct().ToList();
            return Json(department, JsonRequestBehavior.AllowGet);
        }

        //автозаполнение по цеху
        public ActionResult AutocompleteSite(string term)
        {
            var site = db.Passengers.Where(p => p.Site.StartsWith(term))
                .Select(p => new {
                    label = p.Site.Trim() + " - " + p.Company.Trim(), //label используется для отображения в выпадающем списке
                    value = p.Site.Trim(), //value заполняет поле при выборе                
                    Company = p.Company.Trim()
                }).Distinct().ToList();
            return Json(site, JsonRequestBehavior.AllowGet);
        }

        //автозаполнение по подразделению
        public ActionResult AutocompleteCompany(string term)
        {
            var company = db.Passengers.Where(p => p.Company.StartsWith(term))
                .Select(p => new {                   
                  value = p.Company.Trim()        
                }).Distinct().ToList();
            return Json(company, JsonRequestBehavior.AllowGet);
        }

        //автозаполнение должности
        public ActionResult AutocompletePost(string term)
        {//Если необходимо найти любой символ в последовательности то можно использовать Contains вместо StartsWith
            var post = db.Passengers.Where(p => p.Post.Contains(term))
                .Select(p => new {
                    value = p.Post.Trim()
                }).Distinct().ToList();
            return Json(post, JsonRequestBehavior.AllowGet);
        }

        //автозаполнение по фамилии
        public ActionResult AutocompleteSurname(string term)
        {         
            var surname = db.Passengers.Where(p => p.Surname.StartsWith(term))
                .Select(p => new {
                    label = p.Surname.Trim() + " " + p.Name.Trim() + " " + p.Middlename.Trim(), //label используется для отображения в выпадающем списке
                    value = p.Surname.Trim(), //value заполняет поле при выборе
                    id=p.id,
                    name = p.Name.Trim(),
                    middlename=p.Middlename.Trim(),
                    bd = p.Date_Birth.ToString(),               
                    passport =p.Passport.Trim(), 
                    emplnum=p.EmployeeNum.Trim(),
                    phonenum = p.PhoneNum.Trim(),
                    //для корректного отображения значения в dropdownlist преобразуем значение в string
                    sendsms = p.SendSms.ToString(),
                    residence = p.Residence.Trim(),
                    email=p.Email.Trim(),
                    sendemail=p.SendEmail.ToString(),
                    email2=p.Email2.Trim(),
                    sendemail2=p.SendEmail2.ToString(),
                    departament=p.Department.Trim(),
                    site = p.Site.Trim(),
                    company = p.Company.Trim(),
                    post=p.Post.Trim(),
                    shiftnum=p.ShiftNum
                }).ToList();        
               return Json(surname, JsonRequestBehavior.AllowGet);
        }

        //автозаполнение по паспорту
        public ActionResult AutocompletePassport(string term)
        {
            //Так как нам паспортные данные прилетают с маской ХХХ то мы сначала её заменим на пустые строки а потом обрежем имеющийся пробел между серией и номером ХХХХ ХХХХХХ
            string poisk = term.Replace("X","").Trim();           
            var passport = db.Passengers.Where(p => p.Passport.StartsWith(poisk))
                .Select(p => new {
                    //Тут мы разделяем пасспортные данные на серия номер (ХХХХ ХХХХ)
                    label = p.Passport.Trim().Substring(0, p.Passport.Trim().Length - 6) + " " + p.Passport.Trim().Substring(4, p.Passport.Trim().Length-4) + " "+p.Surname.Trim() + " " + p.Name.Trim() + " " + p.Middlename.Trim(), //label используется для отображения в выпадающем списке
                    value = p.Passport.Trim().Substring(0, p.Passport.Trim().Length - 6) + " " + p.Passport.Trim().Substring(4, p.Passport.Trim().Length-4), //value заполняет поле при выборе
                    id = p.id,
                    surname = p.Surname.Trim(),
                    name = p.Name.Trim(),
                    middlename = p.Middlename.Trim(),                    
                    bd = p.Date_Birth.ToString(),                   
                    emplnum = p.EmployeeNum.Trim(),
                    phonenum = p.PhoneNum.Trim(),
                    //для корректного отображения значения в dropdownlist преобразуем значение в string
                    sendsms = p.SendSms.ToString(),
                    residence = p.Residence.Trim(),
                    email = p.Email.Trim(),
                    sendemail = p.SendEmail.ToString(),
                    email2 = p.Email2.Trim(),
                    sendemail2 = p.SendEmail2.ToString(),
                    departament = p.Department.Trim(),
                    site = p.Site.Trim(),
                    company = p.Company.Trim(),
                    post = p.Post.Trim(),
                    shiftnum = p.ShiftNum
                }).ToList();
            return Json(passport, JsonRequestBehavior.AllowGet);
        }

        //Автозаполнение по телефону
        public ActionResult AutocompletePhone(string term)
        {
            //Так как нам паспортные данные прилетают с маской ХХХ то мы сначала её заменим на пустые строки а потом обрежем имеющийся пробел между серией и номером ХХХХ ХХХХХХ
            string poisk = term.Replace(" ","").Replace("X", "").Replace("-", "").Trim();
                        var phone = db.Passengers.Where(p => p.PhoneNum.StartsWith(poisk))
                    .Select(p => new
                    {
                    //Тут мы разделяем пасспортные данные на серия номер (ХХХХ ХХХХ)
                    label = p.PhoneNum.Substring(0,2) + " " + p.PhoneNum.Substring(2, 3) + "-" + p.PhoneNum.Substring(5, 3)
                    + "-" + p.PhoneNum.Substring(8, 2) + "-" + p.PhoneNum.Substring(10, 2) + " " + p.Surname + " " + p.Name + " " + p.Middlename, //label используется для отображения в выпадающем списке
                    value = p.PhoneNum.Substring(0, 2) + " " + p.PhoneNum.Substring(2, 3) + "-" + p.PhoneNum.Substring(5, 3)
                    + "-" + p.PhoneNum.Substring(8, 2) + "-" + p.PhoneNum.Substring(10, 2),  //value заполняет поле при выборе
                        id = p.id,
                        surname = p.Surname,
                        name = p.Name,
                        middlename = p.Middlename,
                        bd = p.Date_Birth.ToString(),
                        emplnum = p.EmployeeNum,
                        passport=p.Passport,                     
                        sendsms = p.SendSms.ToString(),
                        residence = p.Residence,
                        email = p.Email,
                        sendemail = p.SendEmail.ToString(),
                        email2 = p.Email2,
                        sendemail2 = p.SendEmail2.ToString(),
                        departament = p.Department,
                        site = p.Site,
                        company = p.Company,
                        post = p.Post,
                        shiftnum = p.ShiftNum
                    }).ToList();              
       
                return Json(phone, JsonRequestBehavior.AllowGet);
   
        }

        //Автозаполнение по табельному номеру
        public ActionResult AutocompleteEmployeeNum(string term)
        {
            //Так как нам паспортные данные прилетают с маской ХХХ то мы сначала её заменим на пустые строки а потом обрежем имеющийся пробел между серией и номером ХХХХ ХХХХХХ
            string poisk = term.Replace("X", "").Trim();
            var emplnum = db.Passengers.Where(p => p.EmployeeNum.StartsWith(poisk))
                .Select(p => new {
                    label = p.EmployeeNum.Trim() + " " + p.Surname.Trim() + " " + p.Name.Trim() + " " + p.Middlename.Trim(), //label используется для отображения в выпадающем списке
                    value = p.EmployeeNum.Trim(), //value заполняет поле при выборе
                    id = p.id,
                    surname = p.Surname.Trim(),
                    name = p.Name.Trim(),
                    middlename = p.Middlename.Trim(),
                    bd = p.Date_Birth.ToString(),
                    passport = p.Passport.Trim(),                   
                    phonenum = p.PhoneNum.Trim(),
                    //для корректного отображения значения в dropdownlist преобразуем значение в string
                    sendsms = p.SendSms.ToString(),
                    residence = p.Residence.Trim(),
                    email = p.Email.Trim(),
                    sendemail = p.SendEmail.ToString(),
                    email2 = p.Email2.Trim(),
                    sendemail2 = p.SendEmail2.ToString(),
                    departament = p.Department.Trim(),
                    site = p.Site.Trim(),
                    company = p.Company.Trim(),
                    post = p.Post.Trim(),
                    shiftnum = p.ShiftNum
                }).ToList();
            return Json(emplnum, JsonRequestBehavior.AllowGet);
        }        
        
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
