using MvcService.Filters;
using System.Web;
using System.Web.Mvc;

namespace MvcService
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new LogActionActivityAttribute());
            filters.Add(new SessionRecorderAttribute());
        }
    }
}