using Quartz;
using System.Net;
using System.Net.Mail;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AirFlight.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Web.Providers.Entities;


namespace QuartzApp.Jobs
{
    public class EmailSender : IJob
    {
        AirFlightContext db = new AirFlightContext();
        DateTime SD = DateTime.Now.Date;
        DateTime ED = DateTime.Now.Date.AddDays(2);
        public void Execute(IJobExecutionContext context)
        {
           // SendEmailPassener();
            //ChangeTime();
            //ChangeNumFly();            
        }


        public void SendEmailPassener()
        {     
            //Получаем список: Текущая дата и Текущая дата+7 дней, регистрация закрыта и сообщение не отправлено
            var sendlist = db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate >= SD && s.Flight.FlightDate <= ED &&
                                 s.Flight.Approved.Trim() == "Закрыта" && s.SendingEmail == false).ToList();

            var sendemaillist = sendlist.Where(s => s.Passenger.SendEmail == true && s.Passenger.Email!=null && s.Passenger.Email.Trim().Length > 10);
            
            var sendemail2list = sendlist.Where(s => s.Passenger.SendEmail2 == true && s.Passenger.Email2 != null && s.Passenger.Email2.Trim().Length > 10);
            
            int countsend = 0;

            if (sendemaillist != null)
            {
                // отправитель - устанавливаем адрес и отображаемое в письме имя
                MailAddress from = new MailAddress("fly@nakyn.ru", "Регистрация НГОК");
                // адрес smtp-сервера и порт, с которого будем отправлять письмо
                SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587);
                //SmtpClient smtp = new SmtpClient("mir-rpc.alrosa.ru", 587); - компанейская почта
                //// логин и пароль
                smtp.Credentials = new NetworkCredential("fly@nakyn.ru", "3tlg8XmM");
                smtp.EnableSsl = true;

                foreach (var email in sendemaillist)
                {
                    // if (email.Passenger.SendEmail == true && email.Passenger.Email != null && email.Passenger.Email.Trim().Length > 6)
                    // кому отправляем
                    MailAddress to = new MailAddress(email.Passenger.Email.Trim());
                    // создаем объект сообщения
                    MailMessage m = new MailMessage(from, to)
                    {
                        // тема письма            
                        Subject = "Регистрация",
                        // текст письма
                        Body = "<h2>" + email.Passenger.Name.Trim() + " " + email.Passenger.Middlename.Trim() + ", здравствуйте!" +
                        "<h2>Вы были зарегистрированы на рейс, вылетающий " + email.Flight.FlightDate.ToLongDateString() + " по маршруту " + email.Flight.Departure.Trim() + " - " + email.Flight.Arrival.Trim() + "." +
                        "<h2>Рейс номер " + email.Flight.FlightNum + ", время вылета " + email.Flight.FlightTime.ToString().Substring(0, email.Flight.FlightTime.ToString().Length - 3) +
                        "<h2>Тип воздушного судна " + email.Flight.AirType.Trim() + ".<h2>",
                        //// письмо представляет код html
                        IsBodyHtml = true
                    };

                        // адрес smtp-сервера и порт, с которого будем отправлять письмо
                    try {
                        smtp.Send(m);
                         //Сохраняем признак того, что рассылка пользователю сделана
                        var sendeamil = db.RegistrationLists.Find(email.id);
                        sendeamil.SendingEmail = true;
                        db.Entry(sendeamil).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    catch  {
                        countsend++;
                        continue;
                    }
                }       
            }
            //рассылка на личный email2
            if (sendemail2list != null)
            {               
                MailAddress from = new MailAddress("fly@nakyn.ru", "Регистрация НГОК");              
                SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587);               
                smtp.Credentials = new NetworkCredential("fly@nakyn.ru", "3tlg8XmM");
                smtp.EnableSsl = true;

                foreach (var email2 in sendemail2list)
                {
                   
                    MailAddress to = new MailAddress(email2.Passenger.Email2.Trim());                    
                    MailMessage m2 = new MailMessage(from, to)
                    {                            
                        Subject = "Регистрация",
                        Body = "<h2>" + email2.Passenger.Name.Trim() + " " + email2.Passenger.Middlename.Trim() + ", здравствуйте!" +
                        "<h2>Вы были зарегистрированы на рейс, вылетающий " + email2.Flight.FlightDate.ToLongDateString() + " по маршруту " + email2.Flight.Departure.Trim() + " - " + email2.Flight.Arrival.Trim() + "." +
                        "<h2>Рейс номер " + email2.Flight.FlightNum + ", время вылета " + email2.Flight.FlightTime.ToString().Substring(0, email2.Flight.FlightTime.ToString().Length - 3) +
                        "<h2>Тип воздушного судна " + email2.Flight.AirType.Trim() + ".<h2>",                    
                        IsBodyHtml = true
                    };
                                     
                    try
                    {    //Это на случай если у пользователя указан только личный email
                        smtp.Send(m2);
                        //Сохраняем признак того, что рассылка пользователю сделана   
                        var sendeamil2 = db.RegistrationLists.Find(email2.id);
                        sendeamil2.SendingEmail = true;
                        db.Entry(sendeamil2).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    catch
                    {
                        countsend++;
                        continue;
                    }
                 }
            }
            //Для оповещения о рассылке выставляем флаг в таблице рейсы, что рассылка произведена
            foreach (var SendEmail in sendlist.Select(s => s.FlightID).Distinct())
            {
                try
                {
                    var FlightSendEmail = db.Flights.Find(SendEmail);
                    //если ещё флаг не выставляли, то записываем
                    FlightSendEmail.SendingEmail = true;
                    db.Entry(FlightSendEmail).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch
                {
                    continue;
                }
            }
        }

        //Рассылка при изменениях в рейсе
        public void ChangeTime()
        {
            //Получаем список: Текущая дата и Текущая дата+7 дней, регистрация закрыта и сообщение не отправлено
            var flights = db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate >= SD && s.Flight.FlightDate <= ED
                                               && s.Flight.Approved.Trim() == "Закрыта" && s.SendingEmail == true && s.Flight.HaveChange == true).ToList();
            
            var sendemaillist = flights.Where(s => s.Passenger.SendEmail == true && s.Passenger.Email != null && s.Passenger.Email.Trim().Length > 10);

            var sendemail2list = flights.Where(s => s.Passenger.SendEmail2 == true && s.Passenger.Email2 != null && s.Passenger.Email2.Trim().Length > 10);
            int counterr = 0;

            if (sendemaillist != null)
            {
                MailAddress from = new MailAddress("fly@nakyn.ru", "Регистрация НГОК");
                SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587);
                smtp.Credentials = new NetworkCredential("fly@nakyn.ru", "3tlg8XmM");
                smtp.EnableSsl = true;

                foreach (var email in sendemaillist)
                {
                    string time = email.Flight.FlightTimeOld != email.Flight.FlightTime ? "<h3>время вылета было изменено c " + email.Flight.FlightTimeOld.ToString().Substring(0, 5) + " на " + email.Flight.FlightTime.ToString().Substring(0, 5) : "";
                    string airtype = email.Flight.AirTypeOld.Trim() != email.Flight.AirType.Trim() ? "<h3>тип воздушного судна изменился с " + email.Flight.AirTypeOld.Trim() + " на " + email.Flight.AirType.Trim() : "";
                    string note = email.Flight.NoteOld != email.Flight.Note ? "<h3> Было добавлено примечание: " + email.Flight.Note : "";
                    MailAddress to = new MailAddress(email.Passenger.Email.Trim());
                    MailMessage m = new MailMessage(from, to)
                    {
                        Subject = "Изменения в рейсе",
                        Body = "<h2>" + email.Passenger.Name.Trim() + " " + email.Passenger.Middlename.Trim() + "!" +
                        "<h3> в рейсе, на который Вы были зарегистрированы, вылетающий " + email.Flight.FlightDate.ToLongDateString() + " по маршруту " + email.Flight.Departure.Trim() + " - " + email.Flight.Arrival.Trim() + ", " +
                        "произошли изменения: " +
                        time +
                        airtype +
                        note +
                       "<h3> Остальное осталось без изменений<h2>",
                        //// письмо представляет код html
                        IsBodyHtml = true
                    };
                    try { smtp.Send(m); }
                    catch {
                        counterr++;
                        break;
                    }
                }                
                
            }

            if (sendemail2list != null)
            {
                MailAddress from = new MailAddress("fly@nakyn.ru", "Регистрация НГОК");
                SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587);
                smtp.Credentials = new NetworkCredential("fly@nakyn.ru", "3tlg8XmM");
                smtp.EnableSsl = true;

                foreach (var email2 in sendemail2list)
                {
                    string time = email2.Flight.FlightTimeOld != email2.Flight.FlightTime ? "<h3>время вылета было изменено c " + email2.Flight.FlightTimeOld.ToString().Substring(0, 5) + " на " + email2.Flight.FlightTime.ToString().Substring(0, 5) : "";
                    string airtype = email2.Flight.AirTypeOld.Trim() != email2.Flight.AirType.Trim() ? "<h3>тип воздушного судна изменился с " + email2.Flight.AirTypeOld.Trim() + " на " + email2.Flight.AirType.Trim() : "";
                    string note = email2.Flight.NoteOld != email2.Flight.Note ? "<h3> Было добавлено примечание: " + email2.Flight.Note : "";
                    MailAddress to = new MailAddress(email2.Passenger.Email2.Trim());
                    MailMessage m = new MailMessage(from, to)
                    {
                        Subject = "Изменения в рейсе",
                        Body = "<h2>" + email2.Passenger.Name.Trim() + " " + email2.Passenger.Middlename.Trim() + "!" +
                        "<h3> в рейсе, на который Вы были зарегистрированы, вылетающий " + email2.Flight.FlightDate.ToLongDateString() + " по маршруту " + email2.Flight.Departure.Trim() + " - " + email2.Flight.Arrival.Trim() + ", " +
                        "произошли изменения: " +
                        time +
                        airtype +
                        note +
                       "<h3> Остальное осталось без изменений<h2>",
                        //// письмо представляет код html
                        IsBodyHtml = true
                    };
                   
                    try
                    {
                        smtp.Send(m);
                        }
                    catch
                    {
                        counterr++;
                        break;
                    }

                }                  
            }
            //Если рассылка произошла или список рассылки был пуст (некому посылать сообщения), то снимаем флаг что были изменения в рейсе

            //foreach (var old in flights Select(s => s.FlightID).Distinct()) - можно и так)) Тогда  var flight = db.Flights.Find(old);
            foreach (var old in flights.Distinct())
            {
                var flight = db.Flights.Find(old.Flight.id);
                flight.FlightTimeOld = flight.FlightTime;
                flight.AirTypeOld = flight.AirType;
                flight.NoteOld = flight.Note;
                flight.HaveChange = false;
                db.Entry(flight).State = EntityState.Modified;
                db.SaveChanges(); 
            }            
        }

        //Рассылка при изменении рейса у пользовталя
        public void ChangeNumFly()
        {
            var numfly = db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate >= SD && s.Flight.FlightDate <= ED && s.Flight.Approved.Trim() == "Закрыта" && s.SendingEmail == true && s.Flight.SendingEmail == true && s.ChangeNum == true).ToList();
           
            var sendemaillist = numfly.Where(s => s.Passenger.SendEmail == true && s.Passenger.Email != null && s.Passenger.Email.Trim().Length > 10);

            var sendemail2list = numfly.Where(s => s.Passenger.SendEmail2 == true && s.Passenger.Email2 != null && s.Passenger.Email2.Trim().Length > 10);

            int counterr=0;
            
            if (sendemaillist != null)
                {
                MailAddress from = new MailAddress("fly@nakyn.ru", "Регистрация НГОК");
                SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587);
                smtp.Credentials = new NetworkCredential("fly@nakyn.ru", "3tlg8XmM");
                smtp.EnableSsl = true;

                foreach (var email in sendemaillist)
                {                
                        MailAddress to = new MailAddress(email.Passenger.Email.Trim());
                        MailMessage m = new MailMessage(from, to)
                        {
                            Subject = "Изменения в рейсе",
                            Body = "<h2>" + email.Passenger.Name.Trim() + " " + email.Passenger.Middlename.Trim() + "!" +
                            "<h3>Ваш номер рейса был изменён. Вы летите " + email.Flight.FlightDate.ToLongDateString() + " по маршруту " + email.Flight.Departure.Trim() + " - " + email.Flight.Arrival.Trim() + ", " +
                            "<h3>рейс номер " + email.Flight.FlightNum + ", время вылета " + email.Flight.FlightTime.ToString().Substring(0, 5) +
                            "<h3>Тип воздушного судна " + email.Flight.AirType.Trim() + ".<h2>",
                            //// письмо представляет код html
                            IsBodyHtml = true
                        };
                      
                        try
                        {
                            smtp.Send(m);                            
                         }

                        catch
                        {
                           counterr++;
                            break;
                        }                    
                }
            }

            if (sendemail2list != null)
            {
                MailAddress from = new MailAddress("fly@nakyn.ru", "Регистрация НГОК");
                SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587);
                smtp.Credentials = new NetworkCredential("fly@nakyn.ru", "3tlg8XmM");
                smtp.EnableSsl = true;

                foreach (var email2 in sendemail2list)
                {                   
                    MailAddress to = new MailAddress(email2.Passenger.Email.Trim());
                    MailMessage m2 = new MailMessage(from, to)
                    {
                        Subject = "Изменения в рейсе",
                        Body = "<h2>" + email2.Passenger.Name.Trim() + " " + email2.Passenger.Middlename.Trim() + "!" +
                        "<h3>Ваш номер рейса был изменён. Вы летите " + email2.Flight.FlightDate.ToLongDateString() + " по маршруту " + email2.Flight.Departure.Trim() + " - " + email2.Flight.Arrival.Trim() + ", " +
                        "<h3>рейс номер " + email2.Flight.FlightNum + ", время вылета " + email2.Flight.FlightTime.ToString().Substring(0, 5) +
                        "<h3>Тип воздушного судна " + email2.Flight.AirType.Trim() + ".<h2>",
                        IsBodyHtml = true
                    };

                    try
                    {
                        smtp.Send(m2);                        
                    }

                    catch
                    {
                            counterr++;
                            break;
                    }                    
                }
            }

                foreach (var oldnum in numfly)
                {
                    var num = db.RegistrationLists.FirstOrDefault(s => s.FlightID == oldnum.FlightID && s.PassengerID == oldnum.PassengerID);                    
                    num.ChangeNum = false;
                    db.Entry(num).State = EntityState.Modified;
                    db.SaveChanges();
                }
            
        }

    } 
}

