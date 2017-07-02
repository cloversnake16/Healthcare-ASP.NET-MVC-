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

namespace GTPTracker.Controllers
{
    [TermsFilterAttributes]
    public class SeriesController : Controller
    {
        repoSeries repSeries = new repoSeries();
        repoProducts repProducts = new repoProducts();

        [SessionExpireFilter]
        public ActionResult Index()
        {
            IEnumerable<Series> lSeries = repSeries.getList();
            return View(lSeries);
        }

        /*[SessionExpireFilter]*/
    /*    public ActionResult Documents(int? idTenant, int? idCategory)
        {
            bool isCustomer = false;
            string businessRole = (string)Session["BUSINESSROLE"];
            if (businessRole.TrimEnd().TrimStart() == "Customer")
                isCustomer = true;
            ViewBag.isCustomer = isCustomer;
            using(repoGeneralDocs repDocs = new repoGeneralDocs())
            using (repoCustomers repCustomers = new repoCustomers())
            {
                List<vCustomers> lCustomers = new List<vCustomers>();
                vCustomers showInternal = new vCustomers();
                showInternal.id = 0;
                showInternal.name = "internal documents";
                lCustomers.Add(showInternal);
                foreach(vCustomers vCustomer in repCustomers.getList())
                    lCustomers.Add(vCustomer);

                ViewBag.lTenants = lCustomers;

                IEnumerable<GenDocCategories> lCategories = repDocs.getCategories();
                ViewBag.lCategories = lCategories;
                ViewBag.idCategory = idCategory;
            }
            ViewBag.tenant = idTenant;
            if (Session["BUSINESSROLE"] != null)
            {
                string role = ((string)Session["BUSINESSROLE"]).TrimStart().TrimEnd();
                if (role == "GTP General Manager" || role == "GTP KAM")
                    ViewBag.showTenantsList = true;
                if (role == "GTP KAM")
                { // get only my customers
                    using (repoCustomers repCustomers = new repoCustomers())
                        ViewBag.lTenants = repCustomers.getvByKAM((int)Session["IDUSER"]).OrderBy(p=>p.name);
                }
            }
            ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
            return View();
        }*/

        [LogFilter]
       /* [SessionExpireFilter]*/        
        public ActionResult IndexItems(int? category, string modulusFrom, string ModulusTo, string volumeFrom, string volumeTo, string heightFrom, string heightTo, string outerDiameterFrom, string outerDiameterTo, string contactAreaFrom, string contactAreaTo, string specific, string general, string idTenant, string alert, string includePublicProducts = "off")
        {
            Debug.WriteLine("Starting IndexItems "+ DateTime.Now );
            ViewBag.alert = alert;
            ViewBag.category = category;
            DateTime ini = DateTime.Now;
            ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
            using (repoUsers repUsers = new repoUsers())
            {
                SeriesIndexVM vm;
                if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated == false || Session["IDUSER"] == null) // visitor
                    vm = new SeriesIndexVM(category,null, modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general);
                else
                {
                    Users user = repUsers.Get((int)Session["IDUSER"]);
                    if (user.businessRole == "Customer") vm = new SeriesIndexVM(category, user.idTenant, modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general, user); // admin selected one tenant on dropdown
                    else 
                        if (idTenant == null || idTenant == "") vm = new SeriesIndexVM(category, null, modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general, user); //standard
                        else vm = new SeriesIndexVM(category, Int32.Parse(idTenant), modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general, user, includePublicProducts); // admin selected one tenant on dropdown
                }
                Debug.WriteLine("Time for calculating the VM products ={0} ", DateTime.Now - ini);
                return View(vm);
            }
        }

        [LogFilter]
        [SessionExpireFilter]
        public ActionResult IndexItemsMyProducts(int? category, string modulusFrom, string ModulusTo, string volumeFrom, string volumeTo, string heightFrom, string heightTo, string outerDiameterFrom, string outerDiameterTo, string contactAreaFrom, string contactAreaTo, string specific, string general)
        {
            ViewBag.category = category;
            MyProductsVM vm = new MyProductsVM(category, (int)Session["IDTENANT"], modulusFrom, ModulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, general);
            return View(vm);
        }

        [SessionExpireFilter]
        public ActionResult Details(int id)
        {
            return View();
        }

        [SessionExpireFilter]
        public ActionResult Create()
        {
            Series serie = new Series();
            return View(serie);
        }

        [ValidateInput(false)]
        [LogFilter]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Create(FormCollection collection, HttpPostedFileBase file)
        {
            try
            {
                Series serie = new Series();
                serie.applications = collection["applications"];
                serie.category = Int32.Parse( collection["category"]);
                serie.characteristics = collection["characteristics"];
                serie.serie = Int32.Parse( collection["serie"]);
                serie.text = collection["text"];
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Convert.ToString(serie.serie) + "_Serie_" + DateTime.UtcNow.Ticks.ToString(); // +"-" + System.IO.Path.GetFileName(file.FileName); to avoid problems with series named with "..."
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    file.SaveAs(path);
                    serie.imageFileName = fileName;
                }
                else
                {
                    var defaultName = "gtp-icon-02a-256.png";
                    serie.imageFileName = defaultName;
                }
                repSeries.Create(serie);
                return RedirectToAction("Manager", "Products", new { tab = 2, whatToDo = 6 });
            }
            catch
            {
                return View();
            }
        }

        [SessionExpireFilter]
        public ActionResult Edit(int id)
        {
            return View(repSeries.get(id));
        }

        [ValidateInput(false)]
        [LogFilter]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection, HttpPostedFileBase file)
        {
            try
            {
                Series serie = repSeries.get(id);
                serie.applications = collection["applications"];
                serie.category = Int32.Parse( collection["category"]);
                serie.characteristics = collection["characteristics"];
                serie.serie = Int32.Parse( collection["serie"]);
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Convert.ToString(serie.serie) + "_Serie_" + DateTime.UtcNow.Ticks.ToString() + "-" + System.IO.Path.GetFileName(file.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    file.SaveAs(path);
                    serie.imageFileName = fileName;
                }
                else
                {
                    if (serie.imageFileName == null) // only put the default image if the image field is null
                    {
                        var defaultName = "gtp-icon-02a-256.png";
                        serie.imageFileName = defaultName;
                    }
                }
                repSeries.Edit(serie);
                return RedirectToAction("Manager", "Products", new { tab = 2, whatToDo = 6 });
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View();
            }
        }

        [SessionExpireFilter]
        public ActionResult Delete(int id)
        {
            return View(repSeries.get(id));
        }

        [LogFilter]
        [SessionExpireFilter]
        [HttpPost]        
        public ActionResult Delete(Series serie)
        {
            try
            {
                repSeries.Delete(serie);
                return RedirectToAction("Manager", "Products", new { tab = 2, whatToDo = 6 });
            }
            catch
            {
                return View();
            }
        }
    }
}
