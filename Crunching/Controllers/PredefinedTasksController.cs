using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GTPTracker.VM;
using Crunching.Models;
using GTPTracker.repos;
using GTPTracker.Functions;

namespace Crunching.Controllers
{
    public class PredefinedTasksController : Controller
    {

        public ActionResult Index()
        {
            using (VRSecurityMgr vrSecurityMgr = new VRSecurityMgr())
            {
                if (vrSecurityMgr.canI("View", "PredefinedTasks", (int)Session["IDUSER"]))
                {
                    using (repoPredefinedTasks repPredefinedTasks = new repoPredefinedTasks())
                    {
                        IEnumerable<vPredefinedTasks> lPredefinedTasks = repPredefinedTasks.getAll();
                        return View(lPredefinedTasks);
                    }
                }
                else return View(); // TODO addModelError
            }
        }

        public ActionResult Create()
        {
            using (VRSecurityMgr vrSecurityMgr = new VRSecurityMgr())
            {
                if (vrSecurityMgr.canI("Create", "PredefinedTasks", (int)Session["IDUSER"]))
                    return View(new PredefinedTasksVM());
                else return View(); // TODO addModelError
            }
        } 

        [HttpPost]
        public ActionResult Create(PredefinedTasksVM vm)
        {
            try
            {
                using (VRSecurityMgr vrSecurityMgr = new VRSecurityMgr())
                {
                    if (vrSecurityMgr.canI("Create", "PredefinedTasks", (int)Session["IDUSER"]))
                        using (repoPredefinedTasks repPredefinedTasks = new repoPredefinedTasks())
                        {
                            repPredefinedTasks.Create(vm.predefinedTask);
                        }
                    else
                    {
                        ModelState.AddModelError("", "You don't have permissions to create a predefined task");
                        return View(vm); 
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }
        
        public ActionResult Edit(int id)
        {
            using (VRSecurityMgr vrSecurityMgr = new VRSecurityMgr())
            {
                if (vrSecurityMgr.canI("Edit", "PredefinedTasks", (int)Session["IDUSER"]))
                {
                    PredefinedTasksVM predefinedTasksVM = new PredefinedTasksVM(id);
                    return View(predefinedTasksVM);
                }
                else return View(); // TODO addModelError
            }
        }

        [HttpPost]
        public ActionResult Edit(PredefinedTasksVM predefinedTasksVM)
        {
            try
            {
                using (VRSecurityMgr vrSecurityMgr = new VRSecurityMgr())
                {
                    if (vrSecurityMgr.canI("Edit", "PredefinedTasks", (int)Session["IDUSER"]))
                        using (repoPredefinedTasks repPredefinedTasks = new repoPredefinedTasks())
                        {
                            repPredefinedTasks.Edit(predefinedTasksVM.predefinedTask);
                        }
                    else return View(); // TODO addModelError
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }        
        }

        public ActionResult Delete(int id)
        {
            using (VRSecurityMgr vrSecurityMgr = new VRSecurityMgr())
            {
                if (vrSecurityMgr.canI("Delete", "PredefinedTasks", (int)Session["IDUSER"]))
                    return View(new PredefinedTasksVM(id));
                else return View(); // TODO addModelError
            }            
        }

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
