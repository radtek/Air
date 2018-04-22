using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AirFlight.Models;
using System.Collections.Generic;
using ClosedXML.Excel;
using System.IO;
using System.Web.Security;

namespace AirFlight.Controllers
{ 
    [Authorize(Roles = "report")]
    public class ReportController : Controller
    {
        private AirFlightContext db = new AirFlightContext();
        public ActionResult Start(DateTime? StartDate, DateTime? EndDate, DateTime? CurrentDate, int? choosDate, string TypeReport, int[] idfiop)
        {
            if (TypeReport == null)
            { TypeReport = TempData["TypeReport"] != null ? (string)TempData["TypeReport"] : "#ReportPassenger"; }
            if (StartDate == null)
            { StartDate = TempData["StartDate"] != null ? (DateTime)TempData["StartDate"] : DateTime.Now.Date; }
            if (EndDate == null)
            { EndDate = TempData["EndDate"] != null ? (DateTime)TempData["EndDate"] : DateTime.Now.Date.AddDays(5); }
            if (CurrentDate == null)
            { CurrentDate = TempData["CurrentDate"] != null ? (DateTime)TempData["CurrentDate"] : DateTime.Now.Date; }
            if (choosDate == null)
            { choosDate = TempData["choosDate"] != null ? (int)TempData["choosDate"] : 0; }

            TempData["TypeReport"] = TypeReport;
            TempData["StartDate"] = StartDate;
            TempData["EndDate"] = EndDate;
            TempData["CurrentDate"] = CurrentDate;
            TempData["choosDate"] = choosDate;

            ViewBag.TypeReport = TypeReport;
            ViewBag.ReportVisible = "hidden";
            //Инициализируем пустой строготипизированный лист 
            List<RegistrationList> flight = new List<RegistrationList>();            
            if (idfiop!=null)
            {
                ViewBag.ReportVisible = "visible";
                flight = choosDate == 0 ? db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate >= StartDate && s.Flight.FlightDate <= EndDate && idfiop.Contains(s.Passenger.id)).ToList() :
                            db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate == CurrentDate && idfiop.Contains(s.Passenger.id)).ToList();
               //Разные реализации метода Contains
                //var pass = db.RegistrationLists.Include(f=>f.Flight).Where(s => s.Flight.FlightDate >= StartDate && s.Flight.FlightDate <= EndDate && idfio.Contains((int)s.PassengerID)).ToList();
             
            }
                     
            return View(flight);
        }

        public ActionResult DateRange(DateTime? StartDate, DateTime? EndDate, DateTime? CurrentDate, int? choosDate)
        {

            if (StartDate == null)
            { StartDate = TempData["StartDate"] != null ? (DateTime)TempData["StartDate"] : DateTime.Now.Date; }
            if (EndDate == null)
            { EndDate = TempData["EndDate"] != null ? (DateTime)TempData["EndDate"] : DateTime.Now.Date.AddDays(5); }
            if (CurrentDate == null)
            { CurrentDate = TempData["CurrentDate"] != null ? (DateTime)TempData["CurrentDate"] : DateTime.Now.Date; }
            if (choosDate == null)
            { choosDate = TempData["choosDate"] != null ? (int)TempData["choosDate"] : 0; }
            TempData["StartDate"] = StartDate;
            TempData["EndDate"] = EndDate;
            TempData["CurrentDate"] = CurrentDate;
            TempData["choosDate"] = choosDate;

            ViewBag.StartDate1 = StartDate;
            ViewBag.EndDate1 = EndDate;
            ViewBag.CurrentDate1 = CurrentDate;
            ViewBag.ChoosDate = choosDate;


            ViewBag.StartDate = ViewBag.StartDate1.ToLongDateString();
            ViewBag.EndDate = ViewBag.EndDate1.ToLongDateString();
            ViewBag.CurrentDate = ViewBag.CurrentDate1.ToLongDateString();
            ViewBag.SD = ViewBag.StartDate1.Date.ToString("yyyy-MM-dd");
            ViewBag.ED = ViewBag.EndDate1.Date.ToString("yyyy-MM-dd");
            ViewBag.CD = ViewBag.CurrentDate1.ToString("yyyy-MM-dd");
                       
            //return Json(result, JsonRequestBehavior.AllowGet);
            return PartialView("DataReport");
        }

        //Экспорт в Excel
        #region ExportExcel
        public ActionResult ExportExcel( DateTime? StartDate, DateTime? EndDate, DateTime? CurrentDate, int[] passlistid, int choosDate = 0, int open =0)
        {
            int i = 1;
            int j = 0;
            var flight = choosDate == 0 ? db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate >= StartDate && s.Flight.FlightDate <= EndDate && passlistid.Contains(s.Passenger.id)).ToList() :
                        db.RegistrationLists.Include(f => f.Flight).Include(p => p.Passenger).Where(s => s.Flight.FlightDate == CurrentDate && passlistid.Contains(s.Passenger.id)).ToList();
            DateTime SD = (DateTime)StartDate;
            DateTime ED = (DateTime)EndDate;
            DateTime CD = (DateTime)EndDate;
            string FileName = "Вылеты пассажиров.xlsx";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Список пассажиров");
            //Формируем выводимую таблицу, тут можно использовать несколько вариантов, например https://www.aspsnippets.com/Articles/ClosedXML-MVC-Example-Export-to-Excel-using-ClosedXML-in-ASPNet-MVC.aspx
            //я использую вот этот https://github.com/closedxml/closedxml/wiki/Inserting-Tables
            //WsEx - описан ниже

            var dt = new List<WsEx>();

            ws.Cell(1, 1).Value = choosDate == 0 ? "Количество вылетов с " + SD.ToShortDateString() + " по " + ED.ToShortDateString() : "Количество вылетов на " + CD.ToShortDateString();
            ws.Row(1).Height = 25;
            //Оформить стиль ячейки можно в один шот - Вариант 1
            ws.Cell(1, 1).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment.SetVertical(XLAlignmentVerticalValues.Center).Font.SetFontSize(14);
            //Объеденяем ячейки с названием таблицы           
            ws.Range(1, 1, 1, 7).Merge().AddToNamed("Titles");
            //присваиваем имена заголовкам - если будем использовать ws.Cell(2, 1).InsertTable(dt) тогда это необходимо закоментировать
            ws.Cell(2, 1).Value = "№";
            ws.Cell(2, 2).Value = "Дата";
            ws.Cell(2, 3).Value = "Время";
            ws.Cell(2, 4).Value = "№ рейса";
            ws.Cell(2, 5).Value = "От куда";
            ws.Cell(2, 6).Value = "Куда";
            ws.Cell(2, 7).Value = "Тип В/С";
            //Или оформить содержимое ячейки можно в несколько шотов - вариант 2
            var rngColumns = ws.Range("A2:G2");
            rngColumns.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngColumns.Style.Font.Bold = true;
            //Присваиваем цвет ячейке
            rngColumns.Style.Fill.BackgroundColor = XLColor.FromArgb(83, 141, 213);
            //обводим ячейки с названием столбцов
            rngColumns.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rngColumns.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            foreach (var p in flight.Select(p => new { p.Passenger.Surname, p.Passenger.Name, p.Passenger.Middlename, p.Passenger.id }).Distinct())
            {
                //ws.Cell(3 + j, 1).Style.Font.SetBold();                   
                ws.Range(3 + j, 1, 3 + j, 7).Merge().Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Fill.SetBackgroundColor(XLColor.FromArgb(255,192,0)).Font.SetBold()
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment.SetVertical(XLAlignmentVerticalValues.Center).Font.SetFontSize(12);
                ws.Cell(3 + j, 1).Value = String.Format("{0} {1} {2}", p.Surname, p.Name, p.Middlename);


                foreach (var col in flight.Where(c => c.PassengerID == p.id))
                {
                    //Можно записывать значения напрямую в ячейки, как это сделано с фамилией пассажира
                    j++; //кол-во рейсов пассажира
                    dt.Clear();
                    WsEx e = new WsEx();
                    e.Num = i++;
                    e.Date = col.Flight.FlightDate.ToShortDateString().ToString();
                    e.Time = col.Flight.FlightTime.ToString().Substring(0, 5);
                    e.NumFly = col.Flight.FlightNum;
                    e.Deparure = col.Flight.Departure;
                    e.Arrival = col.Flight.Arrival;
                    e.AirType = col.Flight.AirType;

                    dt.Add(e);
                    //** InsertData возвращает таблицу без имён столбцов**//  
                    ws.Cell(3 + j, 1).InsertData(dt.AsEnumerable()).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border.SetInsideBorder(XLBorderStyleValues.Thin).
                        Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    // InsertTable вернёт таблицу с сортировкой столбцов, в таком случае нам не нужно прописывать заголовки для столбцов                  

                }
                j++;
                ws.Cell(3 + j, 1).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right).Alignment.SetVertical(XLAlignmentVerticalValues.Center).Font.SetFontSize(12);                                    
                ws.Range(3 + j, 1, 3 + j, 7).Merge().Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Fill.SetBackgroundColor(XLColor.FromArgb(0,176,240));                
                ws.Cell(3 + j, 1).Value = "Всего выполнено перелётов: "+(--i);
                i = 1;
                j++;
            }
   
            //**выравниваем содержимое ячеек по содержимому**//
                //ws.Columns().AdjustToContents(); - выровнять все ячейки по содержимому            
                ws.Columns().AdjustToContents();
                //изменить ширину отдельного столбца
                //ws.Column(2).Width = 65;

                 string handle = Guid.NewGuid().ToString();
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        stream.Position = 0;
                        TempData[handle] = stream.ToArray();                      
                    }
            //Загрузка файла происходит в 2 этапа, так как это Ajax (JQuery post) запрос. При прямом запросе данные извращения не требуются https://forums.asp.net/t/2128872.aspx?Cannot+download+excel+from+MVC+using+AJAX+call                   
            return new JsonResult(){ Data = new { FileGuid = handle, FileName }};
        }

        //Непосредственно сама загрузка
        public virtual ActionResult DownloadReport(string fileGuid, string fileName)
        {
            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];
                return File(data, "application/vnd.ms-excel", fileName);
            }
            else
            {
                return new EmptyResult();
            }
        }
        //класс для таблицы экспорта в excel
        class WsEx
        {
            public int Num { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
            public int NumFly { get; set; }
            public string Deparure { get; set; }
            public string Arrival { get; set; }
            public string AirType { get; set; }
        }
             


        #endregion
    }
}