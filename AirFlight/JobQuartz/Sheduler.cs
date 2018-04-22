using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace QuartzApp.Jobs
{
    public class Scheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            //триггер рассылки смс            
            IJobDetail sms = JobBuilder.Create<SmsSender>().Build();
            ITrigger trigger1 = TriggerBuilder.Create()  // создаем триггер https://www.quartz-scheduler.net/documentation/quartz-2.x/tutorial/crontriggers.html
                .WithIdentity("sms", "group1")
                .WithCronSchedule("0 0/10 7-20 * * ?")      //Запуск каждые 10 минут по времени между 7 и 20 "0-секунды 10/5 минуты(запуск в 8:00 каждые 10 минут
                .Build();
            scheduler.ScheduleJob(sms, trigger1);

            //триггер рассылки email - временно перенёс рассылку email в отправку смс
            //IJobDetail email = JobBuilder.Create<EmailSender>().Build();
            //ITrigger trigger2 = TriggerBuilder.Create()  // создаем триггер https://www.quartz-scheduler.net/documentation/quartz-2.x/tutorial/crontriggers.html
            //    .WithIdentity("sending", "group1")        
            //    .WithCronSchedule("0 0/1 7-20 * * ?")      //Запуск каждые 5 минут по времени между 7 и 20 "0-секунды 10/5 минуты(запуск в 8:10 каждые 5 минут
            //    .Build(); 
            ////8-20 часы * день месяца * месяц ? день недели)" 
            //scheduler.ScheduleJob(email, trigger2);

            //второй триггер для отправки уведомлений об открытии регистрации
            IJobDetail emailnotification = JobBuilder.Create<EmailNotification>().Build();
            ITrigger trigger3 = TriggerBuilder.Create()
                .WithIdentity("emailnotific", "group2")
                .WithCronSchedule("0 0/5 7-20 * * ?")
                .Build();
            scheduler.ScheduleJob(emailnotification, trigger3);

            //триггер закрытия регистраций, которые не были закрыты
            IJobDetail closeapproved = JobBuilder.Create<ApprovedClose>().Build();
            ITrigger closeapr = TriggerBuilder.Create()
                .WithIdentity("closeaprove", "group3")                
                .WithCronSchedule("0 0 19 * * ?")  //Закрытие регистрации в 19:00 ежедневно
                .Build();
            scheduler.ScheduleJob(closeapproved, closeapr);

            //*****Работа триггера крглые сутки
            //ITrigger trigger1 = TriggerBuilder.Create()  // создаем триггер
            //    .WithIdentity("emailnotific", "user")     // идентифицируем триггер с именем и группой
            //    .StartNow()                            // запуск сразу после начала выполнения
            //    .WithSimpleSchedule(x => x            // настраиваем выполнение действия 
            //    .WithIntervalInMinutes(1)          // через 1 минуту WithIntervalInHours(hours): интервал в часах https://metanit.com/sharp/mvc5/24.1.php
            //    .RepeatForever())                   // бесконечное повторение
            //    .Build();                              // создаем триггер
            //scheduler.ScheduleJob(emailnotification, trigger1);        // начинаем выполнение работы

            //*****Работ триггера п расписанию простой триггер  https://www.quartz-scheduler.net/documentation/quartz-2.x/tutorial/simpletriggers.html
            //ITrigger trigger3 = TriggerBuilder.Create()  
            //   .WithIdentity("sendsms", "all")     
            //   .StartAt(DateBuilder.DateOf(7, 0, 0)) //старт часы минуты секунды
            //   .WithSimpleSchedule(x => x
            //   .WithIntervalInMinutes(5)
            //   .RepeatForever())
            //   .EndAt(DateBuilder.DateOf(20, 0, 0)) //стоп часы минуты секунды              
            //   .Build();                                                 
            //scheduler.ScheduleJob(jobjob, trigger3);


        }
    }
}