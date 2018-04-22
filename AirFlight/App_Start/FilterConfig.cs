using System.Web;
using System.Web.Mvc;
using AirFlight.Filters;

namespace AirFlight
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //Логирование всех действий на сайте
            //filters.Add(new LogAttribute());
        }
    }
}
 