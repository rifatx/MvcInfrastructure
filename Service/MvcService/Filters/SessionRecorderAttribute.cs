using MvcService.Data;
using MvcService.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcService.Filters
{
    public class SessionRecorderAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.Result is JsonResult)
            {
                JsonResult jr = filterContext.Result as JsonResult;

                if (jr.Data is BaseEntity)
                {
                    BaseEntity data = jr.Data as BaseEntity;

                    if (data.Session == null)
                    {
                        data.Session = SessionManager.Current[HttpContext.Current.Session.SessionID];
                    }
                }
            }
        }
    }
}
