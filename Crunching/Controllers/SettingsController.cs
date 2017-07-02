using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.Controllers
{
    public class SettingsController : Controller
    {
        repoCustomers repCustomers = new repoCustomers();

        public FileResult DownloadFile(string URI)
        {
            return File(Url.Content("~/Content/downloads/" + URI), System.Net.Mime.MediaTypeNames.Application.Octet, URI);
        }

        public ActionResult App()
        {
            int idCustomer = (int)Session["IDTENANT"];
            Customers customer = repCustomers.Get(idCustomer);
            ViewBag.LINKID = customer.linkID;
            return View();
        }
    }
}
