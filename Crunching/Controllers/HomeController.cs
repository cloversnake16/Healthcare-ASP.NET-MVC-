using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GTPTracker.repos;
using GTPTracker.VM;
using System.Web.Security;
using GTPTracker.Helpers;
using Crunching.Models;
using System.Diagnostics;

namespace GTPTrackers.Controllers
{
    public class HomeController : Controller
    {
        repoTickets repTicket = new repoTickets();
        repoUsers repUsers = new repoUsers();

        public ActionResult DirectIndex()
        {
            var c1 = ControllerContext.HttpContext.Request.Cookies["GTPTRACKER"];
            if (c1 == null) return RedirectToAction("signIn", "Account");
            int idTenant = Convert.ToInt32(c1.Values["IDTENANT"]);
            int idUser = Convert.ToInt32(c1.Values["IDUSER"]);
            Session["IDTENANT"] = idTenant;
            Session["IDUSER"] = idUser;
            Session["TENANT"] = c1.Values["TENANT"];
            Users currentUser = repUsers.Get(idUser);
            Session["USERNAME"] = currentUser.firstName + " " + currentUser.lastName;
            ViewBag.idUser = (int)Session["IDUSER"];
            int idCustomer = (int)Session["IDTENANT"];
            string password = (string)c1.Values["PASSWORD"];

            if (Membership.ValidateUser(currentUser.email, password))
            {
                FormsAuthentication.SetAuthCookie(currentUser.firstName + " " + currentUser.lastName, true);
            }

            // Milestone 2. Use items
            return RedirectToAction("Index", "Items");

            /*if (HttpContext.User.IsInRole("GTP"))
                return RedirectToAction("IndexGTPUser", new { sort = 1 });

            //int idTenant = (int)Session["IDTENANT"];
            IndexStandardUserVM vm = new IndexStandardUserVM(idTenant, (int)Session["IDUSER"], false, 1);
            //return View(vm);
            return RedirectToAction("Index");*/
        }

        [SessionExpireFilter]
        public ActionResult Index(int? sort)
        {
            return RedirectToAction("Index", "Catalog");            
        }
        
        [SessionExpireFilter]
        public ActionResult IndexGTPUser(int? sort)
        {
            int idUser = (int)Session["IDUSER"];
            int idCustomer = (int)Session["IDTENANT"];
            IndexGTPUserVM vm = new IndexGTPUserVM(idUser, idCustomer, sort);
            return View(vm);
        }

        public ActionResult Terms(string Selected)
        {
            ViewBag.SelectedTab = Selected;
            ViewBag.HideUserPanel = !(System.Web.HttpContext.Current.User.Identity.IsAuthenticated && Session["IDUSER"]!=null);
            return View();
        }

        public void BG(string Selected)
        {
            GTPTracker.BL.BackgroundMgr bgMgr = new GTPTracker.BL.BackgroundMgr();
            bgMgr.dailyTasks();
           // return View();
        }

        public ActionResult getSessionStatus()
        {
            ViewBag.status = Session.Timeout;
            return View();
        }
    }
}
