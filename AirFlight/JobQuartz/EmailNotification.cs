using AirFlight.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace QuartzApp.Jobs
{
    public class EmailNotification : IJob
    {
        public AirFlightContext db = new AirFlightContext();
        public void Execute(IJobExecutionContext context)
        {
            SendNotificationOpen();
            SendNotificationClose();
        }  
        
        //рассылка уведомлений об открытии регистрации
        public void SendNotificationOpen()
        {
            int countsend = 0;
            var sendnotificopen = db.Notifications.Include(f=>f.Flight).Include(a=>a.AspNetUser).Where(f => f.Flight.Approved == "Открыта");
            if (sendnotificopen != null)
            {
                MailAddress from = new MailAddress("fly@nakyn.ru", "Система регистрации пассажиров");
                SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587);
                smtp.Credentials = new NetworkCredential("fly@nakyn.ru", "3tlg8XmM");
                smtp.EnableSsl = true;

                foreach (var email in sendnotificopen)
                {
                    int countreg = db.RegistrationLists.Where(r => r.FlightID == email.FlightID).Count();
                    MailAddress to = new MailAddress(email.AspNetUser.EmailSend.Trim());
                    MailMessage m = new MailMessage(from, to)
                    {
                        // тема письма            
                        Subject = "Открытие регистрации",
                        // текст письма
                        Body = "<p>Сообщаем Вам, что регистрация на рейс " + email.Flight.FlightDate.ToLongDateString() + " по маршруту: " + email.Flight.Departure + " - " +
                        email.Flight.Arrival + ", рейс №" + email.Flight.FlightNum + ", время вылета: " + email.Flight.FlightTime.ToString().Substring(0, 5) +
                        ", тип воздушного судна " + email.Flight.AirType + " была открыта. На момент открытия доступно мест для регистрации: " + (email.Flight.Number - countreg) +
                        " из " + email.Flight.Number + ". " +
                        "<p>Уведомление по данной регистрации будет удалено из системы автоматически",
                        //// письмо представляет код html
                        IsBodyHtml = true
                    };
                    // адрес smtp-сервера и порт, с которого будем отправлять письмо
                    try
                    {
                        smtp.Send(m);
                        //Удаляем запись из рассылки 
                        using (AirFlightContext db = new AirFlightContext())
                        {
                            Notification delnotific = db.Notifications.Find(email.Id);
                            db.Notifications.Remove(delnotific);
                            db.SaveChanges();
                        }

                        }
                    catch
                    {
                        countsend++;
                        break;
                    }
                    
                }
            }           
        }

        public void SendNotificationClose()
        {
           
            int countsend = 0;
            var sendnotificclose = db.Notifications.Include(f => f.Flight).Include(a => a.AspNetUser).Where(f => f.Flight.Approved == "Закрыта");
            if (sendnotificclose != null)
            {
                MailAddress from = new MailAddress("fly@nakyn.ru", "Система регистрации пассажиров");
                SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587);
                smtp.Credentials = new NetworkCredential("fly@nakyn.ru", "3tlg8XmM");
                smtp.EnableSsl = true;

                foreach (var email in sendnotificclose)
                {
                    int countreg = db.RegistrationLists.Where(r => r.FlightID == email.FlightID).Count();
                    MailAddress to = new MailAddress(email.AspNetUser.EmailSend.Trim());
                    MailMessage m = new MailMessage(from, to)
                    {
                        // тема письма            
                        Subject = "Закрытие регистрации",
                        // текст письма
                        Body = "<p>Сообщаем Вам, что регистрация на рейс " + email.Flight.FlightDate.ToLongDateString() + " по маршруту: " + email.Flight.Departure + " - " +
                        email.Flight.Arrival + ", рейс №" + email.Flight.FlightNum + ", время вылета: " + email.Flight.FlightTime.ToString().Substring(0, 5) +
                        ", тип воздушного судна " + email.Flight.AirType + " была закрыта. Регистрация пассажиров на данный рейс более не доступна. " +
                        "Если Вы так и не смогли записать своего сотрудника на рейс, пожалуйста свяжитесь с авиадиспетчером." +                       
                        "<p>Уведомление по данной регистрации будет удалено из системы автоматически",
                        //// письмо представляет код html
                        IsBodyHtml = true
                    };
                    // адрес smtp-сервера и порт, с которого будем отправлять письмо
                    try
                    {
                        using (AirFlightContext db = new AirFlightContext())                    
                        {
                            smtp.Send(m);
                            //Удаляем запись из рассылки                           
                            db.Notifications.Remove(db.Notifications.Find(email.Id));
                            db.SaveChanges();
                        }
                    }
                    catch
                    {
                        countsend++;
                        break;
                    }
                    
                }
            }
        }           



    }
}