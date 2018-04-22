using AirFlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AirFlight.Controllers
{
   
    public class GadgetController : Controller
    {
        public ActionResult TempGadget()
        {
            ViewBag.Temperatura = FromController();
            return View();
        }

        
        public string FromKiasu()
        {
            DateTime datestart = DateTime.Now.AddMinutes(-5);
            DateTime dateend = DateTime.Now;
            using (KiasuContext kiasu = new KiasuContext())
            {
                var temperatura = kiasu.Vals.Where(t => t.tag_id == 2213 && t.time > datestart && t.time < dateend).OrderBy(t => t.time).ToList().LastOrDefault();

                //если полученные значение без сотых
                if (temperatura != null)
                {

                    if (temperatura.ToString().Length > 4)
                    {
                        ViewBag.Temperatura = temperatura.val.ToString().Substring(0, 4);
                    }
                    else
                    {
                        ViewBag.Temperatura = temperatura.val.ToString();
                    }
                    ViewBag.TimeUpdate = temperatura.time.ToShortDateString() + " в " + temperatura.time.ToShortTimeString();
                }
                else
                {
                    ViewBag.Temperatura = "Временно не доступна";
                }
               

            }

            return ViewBag.Temperatura;
        }




        public string FromController()
        {
            libnodave.daveOSserialType fds; //Тип соединения
            libnodave.daveInterface di; //Интерфейс соединения
            libnodave.daveConnection dc; //Соединение  
            int res = 0;
            fds.rfd = libnodave.openSocket(102, "192.168.10.70"); //102-сокет по умолчанию, и адрес контроллера

            fds.wfd = fds.rfd;
            if (fds.rfd > 0)
            {
                di = new libnodave.daveInterface(fds, "IF1", 0, libnodave.daveProtoISOTCP, libnodave.daveSpeed187k);
                di.setTimeout(1000);
                dc = new libnodave.daveConnection(di, 2, 0, 2); //последние цифры 0-стойка 2-номер слота
                if (0 == dc.connectPLC())
                {
                    res = dc.readBytes(libnodave.daveDB, 28, 78, 4, null); //28-номер DB,78-смещение (адрес переменной), 4-длина, null - буфер записии

                    if (res == 0) //conection OK 
                    {
                        float temperatura = dc.getFloat();
                        ViewBag.Temperatura = Math.Round(temperatura, 2).ToString();
                    }
                    else
                    {
                        ViewBag.Err = res + " " + libnodave.daveStrerror(res);
                    }
                }
                dc.disconnectPLC();
                libnodave.closeSocket(fds.rfd);
            }

            else
            {
                ViewBag.Temperatura = "Врменно не доступна";
            }

            return ViewBag.Temperatura;
            
        }


    }
}