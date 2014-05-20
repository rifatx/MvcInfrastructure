using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using MvcService.Data;

namespace MvcService.Controllers
{
    public class MainController : Controller
    {
        public ActionResult Index()
        {
            return new JsonResult() { Data = new { Message = "Error" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult Method1(TestData d)
        {
            return new JsonResult() { Data = d };
        }

        [HttpPost]
        public ActionResult Method2(User u)
        {
            System.Threading.Thread.Sleep(3000);
            return new JsonResult() { Data = u };
        }

        [HttpPost]
        public ActionResult Method3(string o)
        {
            dynamic d = System.Web.Helpers.Json.Decode(o);

            var v = GetDynamicMember(d, "id");

            return new JsonResult() { Data = v };
        }

        private static object GetDynamicMember(object obj, string memberName)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, memberName, obj.GetType(), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
            return callsite.Target(callsite, obj);
        }
    }
}
