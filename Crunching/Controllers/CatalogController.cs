using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Crunching.Models;
using GTPTracker.repos;
using GTPTracker.Helpers;
using GTPTracker.VM;
using System.Diagnostics;

namespace Crunching.Controllers
{
    public class CatalogController : Controller
    {
        public ActionResult Index(int? category, string modulusFrom, string ModulusTo, string volumeFrom, string volumeTo, string heightFrom, string heightTo, string outerDiameterFrom, string outerDiameterTo, string contactAreaFrom, string contactAreaTo, string specific, string general, string idTenant, string alert, string includePublicProducts = "off")
        {
            ViewBag.alert = alert;
            ViewBag.category = category;
            Debug.WriteLine("Starting IndexItems ***********************" + DateTime.Now);
         
            ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
            using (repoUsers repUsers = new repoUsers())
            {
                CatalogIndexVM vm;
                if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated == false || Session["IDUSER"] == null) // visitor
                    vm = new CatalogIndexVM(category, null, modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general);
                else
                {
                    Users user = repUsers.Get((int)Session["IDUSER"]);
                    if (user.businessRole == "Customer") vm = new CatalogIndexVM(category, user.idTenant, modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general, user); // admin selected one tenant on dropdown
                    else
                        if (idTenant == null || idTenant == "") vm = new CatalogIndexVM(category, null, modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general, user); //standard
                        else vm = new CatalogIndexVM(category, Int32.Parse(idTenant), modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general, user, includePublicProducts); // admin selected one tenant on dropdown
                }
                return View(vm);
            }
        }

        public ActionResult PartialViewFirstSerie(int? category, string modulusFrom, string ModulusTo, string volumeFrom, string volumeTo, string heightFrom, string heightTo, string outerDiameterFrom, string outerDiameterTo, string contactAreaFrom, string contactAreaTo, string specific, string general, string idTenant, string alert, string includePublicProducts = "off")
        {
            Debug.WriteLine("Starting PartialView " + DateTime.Now);
            ViewBag.alert = alert;
            ViewBag.category = category;
            DateTime ini = DateTime.Now;
            ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
            //using (repoUsers repUsers = new repoUsers())
            using (repoSeries repSeries = new repoSeries())
            using (repoUsers repUsers = new repoUsers())            
            {
                int idSerie = repSeries.getList().First().id;
                Debug.WriteLine("Time before creating VM ={0} ", DateTime.Now - ini);
                FirstSerie vm;
                if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated == false || Session["IDUSER"] == null) // visitor
                    vm = new FirstSerie(category, null, modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general);
                else
                {
                    Users user = repUsers.Get((int)Session["IDUSER"]);
                    if (user.businessRole == "Customer") vm = new FirstSerie(category, user.idTenant, modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general, user); // admin selected one tenant on dropdown
                    else
                        if (idTenant == null || idTenant == "") vm = new FirstSerie(category, null, modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general, user); //standard
                        else vm = new FirstSerie(category, Int32.Parse(idTenant), modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general, user, includePublicProducts); // admin selected one tenant on dropdown
                }
                Debug.WriteLine("Time after PartialViewFirstSerie ={0} ", DateTime.Now - ini);
                return View(vm);
            }
        }

        public ActionResult PartialViewCDRSerie(int? category, string modulusFrom, string ModulusTo, string volumeFrom, string volumeTo, string heightFrom, string heightTo, string outerDiameterFrom, string outerDiameterTo, string contactAreaFrom, string contactAreaTo, string specific, string general, string idTenant, string alert, string includePublicProducts = "off")
        {
            Debug.WriteLine("Starting PartialViewCDRSerie " + DateTime.Now);
            ViewBag.alert = alert;
            ViewBag.category = category;
            DateTime ini = DateTime.Now;
            ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
            using (repoUsers repUsers = new repoUsers())
            {
                SeriesIndexVM vm;
                if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated == false || Session["IDUSER"] == null) // visitor
                    vm = new SeriesIndexVM(category, null, modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general);
                else
                {
                    Users user = repUsers.Get((int)Session["IDUSER"]);
                    if (user.businessRole == "Customer") vm = new SeriesIndexVM(category, user.idTenant, modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general, user); // admin selected one tenant on dropdown
                    else
                        if (idTenant == null || idTenant == "") vm = new SeriesIndexVM(category, null, modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general, user); //standard
                        else vm = new SeriesIndexVM(category, Int32.Parse(idTenant), modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general, user, includePublicProducts); // admin selected one tenant on dropdown
                }
                Debug.WriteLine("Time for PartialViewCDRSerie ={0} ", DateTime.Now - ini);
                return View(vm);
            }
        }
    }
}
