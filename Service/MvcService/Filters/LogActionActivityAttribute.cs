using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcService.Filters
{
    public class LogActionActivityAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                File.AppendAllText(@"C:\osman.txt",
                    "----------------"
                    + Environment.NewLine
                    + filterContext.Controller.ToString()
                    + Environment.NewLine
                    + filterContext.ActionDescriptor.ActionName
                    + Environment.NewLine
                    + string.Join(Environment.NewLine, filterContext.ActionParameters.Values.Select(o => o.ToString()))
                    + Environment.NewLine
                    + "---------------"
                    );
            }
            catch
            {
            }

            base.OnActionExecuting(filterContext);
        }
    }
}