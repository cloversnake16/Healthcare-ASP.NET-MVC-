using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Crunching.Models;
using GTPTracker.repos;
using GTPTracker.Helpers;

namespace GTPTracker.Controllers
{
    public class MilestonesController : Controller
    {
        repoMilestones repMilestones = new repoMilestones();
        repoProducts repProducts = new repoProducts();
        repoCustomers repCustomers = new repoCustomers();
        repoUsers repUsers = new repoUsers();

        [SessionExpireFilter]
        public ActionResult Index()
        {
            IEnumerable<Milestones> lMilestones = repMilestones.getList();
            return View(lMilestones);
        }

        [SessionExpireFilter]
        public ActionResult Create()
        {
            ViewBag.lProducts = new MultiSelectList(repProducts.getAll(), "id", "refNumber");
            ViewBag.lCustomers = new MultiSelectList(repCustomers.getList(), "id", "name");
           
            Milestones element = new Milestones();
            return View(element);
        }

        public JsonResult DetailsJSON(int id)
        {
            Milestones element = repMilestones.get(id);
            return Json(element, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(int id)
        {
            Milestones element = repMilestones.get(id);
            return View(element);
        }

        public ActionResult PartialView(int id)
        {
            Milestones element = repMilestones.get(id);
            return View(element);
        }

        [SessionExpireFilter]
        [HttpPost]
        [Authorize(Roles = "GTP")]
        public ActionResult Create(string activationDate, string text, int[] productsID, int[] customersID)
        {
            try
            {
                Milestones milestone = new Milestones();
                milestone.activationDate = DateTime.Parse(activationDate);
                milestone.cDate = DateTime.UtcNow;
                milestone.text = text;
                repMilestones.Create(milestone);

                if (milestone.everyone == true)
                {
                    IEnumerable<Users> lUsers = repUsers.getList();
                    foreach (var user in lUsers)
                        repMilestones.addUser(milestone.id, user.id);
                }
                else
                {
                    repMilestones.addUser(milestone.id, (int)Session["IDUSER"]); // add creator to the watchlist
                    // users from selected customers
                    IEnumerable<Users> lUsers = repUsers.getList().Where(p => customersID.Contains(p.idTenant));
                    if (lUsers != null && customersID != null)
                        foreach (var user in lUsers)
                            repMilestones.addUser(milestone.id, user.id);
                    IEnumerable<int> lCustomers = repProducts.getCustomersByProductsList(productsID);
                    if (lCustomers != null && productsID != null)
                    {
                        IEnumerable<Users> lAllUsers = repUsers.getList();
                        IEnumerable<Users> lUsersFromProducts = lAllUsers.Where(p => lCustomers.Contains(p.idTenant));
                        foreach (var user in lUsersFromProducts)
                            repMilestones.addUser(milestone.id, user.id);
                    }
                }

                //return RedirectToAction("Index", "Milestones");
                return RedirectToAction("Index", "Items");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        [SessionExpireFilter]
        public ActionResult Edit(int id)
        {
            Milestones element = repMilestones.get(id);
            return View(element);
        }

        [SessionExpireFilter]
        [HttpPost]
        [Authorize(Roles = "GTP")]
        public ActionResult Edit(int id, string activationDate, string text)
        {
            try
            {
                Milestones milestone = repMilestones.get(id);
                milestone.activationDate = DateTime.Parse( activationDate);
                milestone.text = text;
                repMilestones.Edit(milestone);
                return RedirectToAction("Index", "Milestones");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                Milestones element = repMilestones.get(id);
                return View(element);
            }
        }
    }
}
