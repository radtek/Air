using AirFlight.Models;
using Quartz;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;

namespace QuartzApp.Jobs
{
    public class SmsSender : IJob
    {
        public AirFlightContext db = new AirFlightContext();
        DateTime SD = DateTime.Now.Date;
        DateTime ED = DateTime.Now.Date.AddDays(2);
        public void Execute(IJobExecutionContext context)
        {
            SendSmsRegistration();
            SendSmsChangeFlight();
            SendSmsChangeFlightNum();
            SendEmailPassener();

        }

        //Рассылка Смс
        public void SendSmsRegistration()
        {
            //Получаем список: Текущая дата и Текущая дата+2 дня, регистрация закрыта и сообщение не отправлено
            var sendsmslist = db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate > SD && s.Flight.FlightDate <= ED && s.Flight.Approved.Trim() == "Закрыта"
                              && s.SendingSms != true && s.Passenger.SendSms == true && s.Passenger.PhoneNum != null).ToList();

            int counterr = 0;
            if (sendsmslist != null)
            {
                foreach (var sms in sendsmslist)
                {
                    try
                    {
                        var sendsms = new WebClient();
                        sendsms.DownloadString("http://www.mcommunicator.ru/m2m/m2m_api.asmx/SendMessage?login=79142518694&password=3b4e5b2d9297880bf800e4260e6db603&naming=NakynFly&msid=" + sms.Passenger.PhoneNum.Trim() + "&message=" + sms.Passenger.Name.Trim()
                            +(string.IsNullOrWhiteSpace(sms.Passenger.Middlename)?"": " " + sms.Passenger.Middlename.Trim()) + ", Ваш вылет по маршруту " + sms.Flight.Departure + "-" + sms.Flight.Arrival + ": " + sms.Flight.FlightDate.ToLongDateString() + " (" + sms.Flight.FlightDate.ToString("dddd") + ") в " + sms.Flight.FlightTime.ToString().Substring(0, 5) +
                            ". Рейс №" + sms.Flight.FlightNum + " (" + sms.Flight.AirType.ToLower() + ").");

                        //Сохраняем признак того, что рассылка пользователю сделана
                        var sendlist = db.RegistrationLists.Find(sms.id);
                        sendlist.ChangeNum = false;
                        sendlist.SendingSms = true;
                        db.Entry(sendlist).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    catch
                    {
                        counterr++;
                        continue;
                    }
                }

                //Для оповещения о рассылке выставляем флаг в таблице рейсы, что рассылка произведена
                foreach (var FlightID in sendsmslist.Select(s => s.FlightID).Distinct())
                {
                    var FlightSendSms = db.Flights.Find(FlightID);
                    //узнаём количество пассажиров на рейсе, которое должно было получить смс
                    var countpass = sendsmslist.Where(c => c.FlightID == FlightID);
                    //узнаём сколько смс по факту мы отправили
                    var countsmssend = countpass.Where(s => s.SendingSms == true);
                    //если кличество смс для отправки равно количеству отправленных смс по факту то пише true иначе false
                    if (countpass.Count() == countsmssend.Count())
                    {
                        FlightSendSms.SendingSms = true;                       
                        db.Entry(FlightSendSms).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        FlightSendSms.SendingSms = false;
                        db.Entry(FlightSendSms).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
        }

        //рассылка смс при изменении на рейсе
        public void SendSmsChangeFlight()
        {
            var passengerlist = db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate >= SD && s.Flight.FlightDate <= ED && s.SendingSms==true && s.HaveChange == true && s.Flight.HaveChange==true).ToList();
            int counterr = 0;
            if (passengerlist != null)
            {
                foreach (var sms in passengerlist)
                {
                    try
                    {
                        string time = sms.Flight.FlightTimeOld != sms.Flight.FlightTime ? " изменено время вылета на " + sms.Flight.FlightTime.ToString().Substring(0, 5) : "";
                        string airtype = sms.Flight.AirTypeOld.Trim() != sms.Flight.AirType.Trim() ? " изменён тип ВС на " + sms.Flight.AirType.ToLower().Trim() : "";
                        //Сначала проверяем не являются ли поля пустыми, с пробелами или с NULL
                        string note =  sms.Flight.NoteOld != sms.Flight.Note ? sms.Flight.Note == null? " примечание было удалено" : string.IsNullOrWhiteSpace(sms.Flight.NoteOld)?" добавлено примечание: " + sms.Flight.Note : " изменено примечание: " + sms.Flight.Note : ""; 
                        var sendsms = new WebClient();
                        sendsms.DownloadString("http://www.mcommunicator.ru/m2m/m2m_api.asmx/SendMessage?login=79142518694&password=3b4e5b2d9297880bf800e4260e6db603&naming=NakynFly&msid=" + sms.Passenger.PhoneNum.Trim() + "&message=" +
                        "В Вашем рейсе " +sms.Flight.FlightDate.ToString("d")+ time+ airtype+ note);
                        //Сохраняем признак того, что рассылка пользователю сделана
                        var sendlist = db.RegistrationLists.Find(sms.id);
                        sendlist.HaveChange = false;
                        db.Entry(sendlist).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    catch
                    {
                        counterr++;
                        continue;
                    }
                }

                foreach (var FlightID in passengerlist.Select(s => s.FlightID).Distinct())
                {
                    var ChangeSendSms = db.Flights.Find(FlightID);
                    //узнаём список пассажиров на рейсе должно было получить смс
                    var countpass = passengerlist.Where(c => c.FlightID == FlightID);
                    //узнаём сколько смс по факту мы отправили
                    var countsmssend = countpass.Where(s => s.HaveChange == false);
                    //если кличество смс для отправки равно количеству отправленных смс по факту то пишем true иначе false
                    if (countpass.Count() == countsmssend.Count())
                    {
                        ChangeSendSms.HaveChange = false;
                        ChangeSendSms.FlightTimeOld = ChangeSendSms.FlightTime;
                        ChangeSendSms.AirTypeOld = ChangeSendSms.AirType;
                        ChangeSendSms.NoteOld = ChangeSendSms.Note;
    
                        db.Entry(ChangeSendSms).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        ChangeSendSms.HaveChange = true;
                        db.Entry(ChangeSendSms).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
        }

        //рассылка смс при изменении рейса у пользователя
        public void SendSmsChangeFlightNum()
        {
            var sendsmschangenum = db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate >= SD && s.Flight.FlightDate <= ED && s.Flight.Approved.Trim() == "Закрыта" && s.SendingSms == true && s.ChangeNum == true).ToList();

            if (sendsmschangenum != null)
            {
                foreach (var sendsmschange in sendsmschangenum)
                {
                    try
                    {
                        var sendsms = new WebClient();
                        sendsms.DownloadString("http://www.mcommunicator.ru/m2m/m2m_api.asmx/SendMessage?login=79142518694&password=3b4e5b2d9297880bf800e4260e6db603&naming=NakynFly&msid=" + sendsmschange.Passenger.PhoneNum.Trim() + "&message=" +
                            "Ваш номер рейса изменён на рейс №" + sendsmschange.Flight.FlightNum + ", вылет "+ sendsmschange.Flight.FlightDate.ToString("d") + " в "  + sendsmschange.Flight.FlightTime.ToString().Substring(0, 5));

                        var savechange = db.RegistrationLists.Find(sendsmschange.id);
                        savechange.ChangeNum = false;
                        db.Entry(savechange).State = EntityState.Modified;
                        db.SaveChanges();

                    }
                    catch
                    {
                        continue;
                    }

                }

                //Для оповещения о рассылке выставляем флаг в таблице рейсы, что рассылка произведена
                foreach (var FlightID in sendsmschangenum.Select(s => s.FlightID).Distinct())
                {
                    var FlightSendSms = db.Flights.Find(FlightID);
                    //узнаём список пассажиров на рейсе должно было получить смс
                    FlightSendSms.HaveChange = false;
                    if (FlightSendSms.SendingSms == null)
                    {
                        FlightSendSms.SendingSms = true;
                    }
                        db.Entry(FlightSendSms).State = EntityState.Modified;
                        db.SaveChanges();                    
                }

            }
        }

        //email рассылка
        public void SendEmailPassener()
        {
            //Получаем список: Текущая дата и Текущая дата+7 дней, регистрация закрыта и сообщение не отправлено
            var sendlist = db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate >= SD && s.Flight.FlightDate <= ED &&
                                 s.Flight.Approved.Trim() == "Закрыта" && s.SendingEmail == false).ToList();

            var sendemaillist = sendlist.Where(s => s.Passenger.SendEmail == true && s.Passenger.Email != null && s.Passenger.Email.Trim().Length > 10);

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
                    try
                    {
                        smtp.Send(m);
                        //Сохраняем признак того, что рассылка пользователю сделана
                        var sendeamil = db.RegistrationLists.Find(email.id);
                        sendeamil.SendingEmail = true;
                        db.Entry(sendeamil).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    catch
                    {
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


    }
}



/* sendsms.DownloadString("http://www.mcommunicator.ru/m2m/m2m_api.asmx/SendMessage?login=79142518694&password=3b4e5b2d9297880bf800e4260e6db603&naming=NakynFly&msid=" + sms.Passenger.PhoneNum.Trim() + "&message=" + sms.Passenger.Name.Trim()
                           + " " + sms.Passenger.Middlename.Trim() + ", Ваш вылет: " + sms.Flight.FlightDate.ToString("d") + " в " + sms.Flight.FlightTime.ToString().Substring(0, 5) + ". Рейс №" + sms.Flight.FlightNum + ".");*/
//http://api.prostor-sms.ru/messages/v2/send/?&login=t89142529081&password=876980&sender=NakynFly&phone= - ProstoR Доступные подписи: NakynFly, Nakyn,NakynNet,NGOK-Fly
//https://lcab.smsprofi.ru/lcabApi/sendSms.php?login=lanjeronium&password=Qwe767rtY&source=NakynFly&txt=&to= - это смс профи Доступные подписи: NakynFly, Nakyn,NakynNet,NGOK-Fly
//http://www.mcommunicator.ru/m2m/m2m_api.asmx/SendMessage?msid=79142523321&message=Проверка&naming=NakynFly&login=79142523321&password=918625c8ac35cb69897ed4bca054fc0e  - это МТС мой номер                       
//http://www.mcommunicator.ru/m2m/m2m_api.asmx/SendMessage?login=79142518694&password=3b4e5b2d9297880bf800e4260e6db603&naming=NakynFly&msid=&message= - это МТС НГОК доступная подпись только NakynFly