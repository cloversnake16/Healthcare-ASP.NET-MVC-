using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Crunching.Models;
using GTPTracker.repos;
using GTPTracker.VM;
using GTPTracker.Functions;
using GTPTracker.Helpers;

namespace GTPTrackerControllers
{
    public class SubTasksController : Controller
    {
        repoSubtasks repSubTasks = new repoSubtasks();
        repoTickets repTickets = new repoTickets();
        EmailManager emailMgr = new EmailManager();
        repoUsers repUsers = new repoUsers();
        
        public ActionResult Index()
        {
            var c1 = ControllerContext.HttpContext.Request.Cookies["GTPTRACKER"];
            if (c1 == null) return RedirectToAction("signIn", "Account");
            int idTenant = Convert.ToInt32(c1.Values["IDTENANT"]);
            int idUser = Convert.ToInt32(c1.Values["IDUSER"]);
            Session["IDTENANT"] = idTenant;
            Session["IDUSER"] = idUser;
            Session["TENANT"] = c1.Values["TENANT"];
            Users currentUser = repUsers.Get(idUser);
            Session["USERNAME"] = currentUser.firstName+ " " + currentUser.lastName;
            ViewBag.idUser = (int)Session["IDUSER"];
            int idCustomer = (int)Session["IDTENANT"];
            //IndexGTPUserVM vm = new IndexGTPUserVM(idUser, idCustomer, null); /* The tasks needs the same than the Index for GTP Users, because it has tasks and tickets */
            IEnumerable<vSubTasks> vm = repSubTasks.getAll();
            if (HttpContext.User.IsInRole("KAM")) // a KAM can see his tasks and the ones related with him
                vm = vm.Where(p => p.idKeyAccountManager == idUser || p.idUser == idUser);
            return View(vm);
        }

        [SessionExpireFilter]
        public ActionResult Create(int idTicket)
        {
            SubTaskVM vm = new SubTaskVM(idTicket);
            return View(vm);
        }

        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                SubTasks subtask = new SubTasks();
                subtask.description = collection["subtask.description"];
                subtask.effectivenessEvaluation = collection["subtask.effectivenessEvaluation"];
                string valor = collection["subtask.implementedMeasures"];
                subtask.implementedMeasures = valor == "on" ? true : false;  //bool.Parse(collection["subtask.implementedMeasures"]);
                subtask.idUser = Int32.Parse(collection["subtask.idUser"]);
                subtask.idCreator = (int)Session["IDUSER"];
                subtask.idTicket = Int32.Parse(collection["subtask.idTicket"]);
                subtask.cDate = DateTime.Today;
                subtask.solved = false;
                if ((int)Session["IDTENANT"] == 1)
                {
                    string valorInternal = collection["subtask.internalTask"];
                    subtask.internalTask = valorInternal == "on" ? true : false;
                }
                else subtask.internalTask = false; // tasks created by customers are always external.
                repSubTasks.Create(subtask);
                emailMgr.emailNewTask(subtask);
                TasksManager taskMgr = new TasksManager();
                taskMgr.recalculateProgress(subtask.idTicket);
                Tickets ticket = repTickets.get(subtask.idTicket);
                if (ticket.idStatus == 1) // the first time a GTP user create a task update status from open to processing
                    repTickets.setStatusProcessing(ticket.id);
                return RedirectToAction("Details", "Tickets", new { id= subtask.idTicket, tab=2 });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(new SubTaskVM(Int32.Parse(collection["subtask.idTicket"])));
            }
        }

        [SessionExpireFilter]
        public ActionResult Solve(int id)
        {
            SubTasks task = repSubTasks.get(id);
            return View(task);
        }

        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Solve(SubTasks task)
        {
            task.dateSolved = DateTime.Today;
            task.solved = true;
            repSubTasks.Edit(task);
            TasksManager taskMgr = new TasksManager();
            taskMgr.recalculateProgress(task.idTicket);
           // return RedirectToAction("Index", "SubTasks");
           // return RedirectToAction("Index", "Items");
            return RedirectToAction("Details", "Tickets", new { id = task.idTicket});
        }

        [SessionExpireFilter]
        public ActionResult Reopen(int id)
        {
            SubTasks task = repSubTasks.get(id);
            task.solved = false;
            repSubTasks.Edit(task);
            TasksManager taskMgr = new TasksManager();
            taskMgr.recalculateProgress(task.idTicket);
            return RedirectToAction("Index", "SubTasks");
        }

        [SessionExpireFilter]
        public ActionResult Details(int id)
        {
            TaskDetailsVM vm = new TaskDetailsVM(id);
            return View(vm);
        }

        public JsonResult DetailsJSON(int id)
        {
            TaskDetailsVM vm = new TaskDetailsVM(id);
            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        [SessionExpireFilter]
        public ActionResult Edit(int id)
        {
            return View();
        }

        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [SessionExpireFilter]
        public ActionResult Delete(int id)
        {
            SubTasks task = repSubTasks.get(id);
            return View(task);
        }

        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                SubTasks task = repSubTasks.get(id);
                repSubTasks.Delete(task);
                TasksManager taskMgr = new TasksManager();
                taskMgr.recalculateProgress(task.idTicket);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                SubTasks task = repSubTasks.get(id);
                return View(task);
            }
        }

        public ActionResult MyTasks()
        {
            int me = (int)Session["IDUSER"];
            IEnumerable<vSubTasks> list = repSubTasks.getOpenTasksAssignedToMe(me);
            return View(list);
        }

        /********************* API *************************/

        public JsonResult getOpenTasksAssignedToMe()
        {
            int me = (int)Session["IDUSER"];
            IEnumerable<vSubTasks> list = repSubTasks.getOpenTasksAssignedToMe(me);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AssignTo(int idTask, int idUser)
        {
            bool success = true;
            try
            {
                SubTasks subTask = repSubTasks.get(idTask);
                subTask.idUser = idUser;
                repSubTasks.Edit(subTask);
            }
            catch { success = false; }
            return Json(success, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreateJSON(string internalTask , string description, int? idUser, int idTicket)
        {
            try
            {
                SubTasks subtask = new SubTasks();
                subtask.description = description;
                
                subtask.idUser = (idUser == null) ? (int)Session["IDUSER"]: (int) idUser;
                subtask.idCreator = (int)Session["IDUSER"];
                subtask.idTicket = idTicket;
                subtask.cDate = DateTime.Today;
                subtask.solved = false;
                if ((int)Session["IDTENANT"] == 1) subtask.internalTask = internalTask == "on" ? true : false;
                else subtask.internalTask = false; // tasks created by customers are always external.
                repSubTasks.Create(subtask);
                emailMgr.emailNewTask(subtask);
                TasksManager taskMgr = new TasksManager();
                taskMgr.recalculateProgress(subtask.idTicket);
                Tickets ticket = repTickets.get(subtask.idTicket);
                if (ticket.idStatus == 1) // the first time a GTP user create a task update status from open to processing
                    repTickets.setStatusProcessing(ticket.id);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CreateAndReturnToStream(string internalTask, string description, int? idUser, int idTicket)
        {
            try
            {
                SubTasks subtask = new SubTasks();
                subtask.description = description;

                subtask.idUser = (idUser == null) ? (int)Session["IDUSER"] : (int)idUser;
                subtask.idCreator = (int)Session["IDUSER"];
                subtask.idTicket = idTicket;
                subtask.cDate = DateTime.Today;
                subtask.solved = false;
                if ((int)Session["IDTENANT"] == 1) subtask.internalTask = internalTask == "on" ? true : false;
                else subtask.internalTask = false; // tasks created by customers are always external.
                repSubTasks.Create(subtask);
                emailMgr.emailNewTask(subtask);
                TasksManager taskMgr = new TasksManager();
                taskMgr.recalculateProgress(subtask.idTicket);
                Tickets ticket = repTickets.get(subtask.idTicket);
                if (ticket.idStatus == 1) // the first time a GTP user create a task update status from open to processing
                    repTickets.setStatusProcessing(ticket.id);
                return RedirectToAction("Index", "Items");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("Index", "Items");
            }
        }

        public ActionResult CreateAndReturnToTicket(string internalTask, string description, int? idUser, int idTicket)
        {
            try
            {
                Tickets ticket = repTickets.get(idTicket);

                SubTasks subtask = new SubTasks();
                subtask.description = ticket.title + " - " + description;

                subtask.idUser = (idUser == null) ? (int)Session["IDUSER"] : (int)idUser;
                subtask.idCreator = (int)Session["IDUSER"];
                subtask.idTicket = idTicket;
                subtask.cDate = DateTime.Today;
                subtask.solved = false;
                if ((int)Session["IDTENANT"] == 1) subtask.internalTask = internalTask == "on" ? true : false; //internalTask.HasValue ? (bool)internalTask : false;
                else subtask.internalTask = false; // tasks created by customers are always external.
                repSubTasks.Create(subtask);
                emailMgr.emailNewTask(subtask);
                TasksManager taskMgr = new TasksManager();
                taskMgr.recalculateProgress(subtask.idTicket);
                
                if (ticket.idStatus == 1) // the first time a GTP user create a task update status from open to processing
                    repTickets.setStatusProcessing(ticket.id);
                return RedirectToAction("Details", "Tickets", new  { id = ticket.id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("Details", "Tickets", new { id = idTicket });
            }
        }

        public ActionResult SolveAndReturnToTicket(int id)
        {
            SubTasks tarea = repSubTasks.get(id);
            if (tarea.solved == true) return RedirectToAction("ReopenAndReturnToTicket", new { id = id }); // if already solved then reopen
           
            tarea.dateSolved = DateTime.Today;
            tarea.solved = true;
            repSubTasks.Edit(tarea);
            TasksManager taskMgr = new TasksManager();
            taskMgr.recalculateProgress(tarea.idTicket);
            ItemsManager itemsMgr = new ItemsManager();
            itemsMgr.updateTicketItem(tarea.idTicket);
            return RedirectToAction("Details", "Tickets", new { id = tarea.idTicket });
        }

        public ActionResult ReopenAndReturnToTicket(int id)
        {
            SubTasks tarea = repSubTasks.get(id);
            tarea.dateSolved = null;
            tarea.solved = false;
            repSubTasks.Edit(tarea);
            TasksManager taskMgr = new TasksManager();
            taskMgr.recalculateProgress(tarea.idTicket);
            ItemsManager itemsMgr = new ItemsManager();
            itemsMgr.updateTicketItem(tarea.idTicket);
            return RedirectToAction("Details", "Tickets", new { id = tarea.idTicket });
        }

        public ActionResult Remove(int id) // https://app.asana.com/0/113967942082869/118188849785186
        {
            SubTasks task = repSubTasks.get(id);
            int idticket = task.idTicket;
            repSubTasks.Delete(task);
            TasksManager taskMgr = new TasksManager();
            taskMgr.recalculateProgress(task.idTicket);
            return RedirectToAction("Details", "Tickets", new { id = idticket });
        }
    }
}
