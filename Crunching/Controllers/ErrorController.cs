using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Crunching.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index(string Type)
        {
            if (!string.IsNullOrEmpty(Type))
            {
                ViewBag.ErrorCode = int.Parse(Type);

                Response.StatusCode = ViewBag.ErrorCode;
                Response.TrySkipIisCustomErrors = true;
            }
            return View();
        }

        /*
        public ActionResult Index(int statusCode, Exception exception) {
            Response.StatusCode = statusCode; return View();
        }*/

        public ActionResult Generate500Error()
        {
            return new HttpStatusCodeResult(500);
        }
    }

}
