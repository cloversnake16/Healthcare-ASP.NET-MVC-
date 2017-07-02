using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GTPTracker.VM;
using GTPTracker.repos;
using GTPTracker.BL;
using Crunching.Models;
using GTPTracker.Helpers;
using GTPTracker.Functions;

namespace GTPTracker.Controllers
{
    public class VisitReportsTasksController : Controller
    {

        [SessionExpireFilter]
        [LogFilter]
        public ActionResult Index(bool showDoneTasks = false)
        {
            int idUser = (int)Session["IDUSER"];
            MyTasksVM vm = new MyTasksVM(idUser, showDoneTasks);
            return View(vm);
        }

        //
        // GET: /VisitReportsTasks/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /VisitReportsTasks/Create

        public ActionResult Create(int idTicket, int idVisitReport)
        {
            VisitReportsTasksVM taskVM = new VisitReportsTasksVM(idTicket, idVisitReport);
            ViewBag.idTicket = idTicket;
            return View(taskVM);
        }

        [HttpPost]
        [LogFilter]
        public ActionResult Create(VisitReportsTasksVM vm, string deadline, string selectResponsible, int idTicket, int idVisitReport, bool solved=false)
        {
            try
            {
                using (repoSubtasks repTasks = new repoSubtasks())
                using (repoVisitReports repVisitReports = new repoVisitReports())
                {
                    SubTasks task = new SubTasks();
                    task.assignTo = selectResponsible;
                    VisitReportsTasksManager visitReportsTasksMgr = new VisitReportsTasksManager();
                    task = visitReportsTasksMgr.setAssignTo(task);
                    task.internalTask = false;
                    task.idTicket = idTicket;
                    VRSecurityMgr vrSecurityMgr = new VRSecurityMgr();
                    task.description = vrSecurityMgr.sanitizeXSSAttacks(vm.content);
                    task.solved = solved;
                    if (deadline != null && deadline != "")
                    {
                        try
                        {
                            task.deadline = DateTime.Parse(deadline, System.Globalization.CultureInfo.GetCultureInfo("es-es"));
                        }
                        catch
                        {
                            try
                            {
                                task.deadline = DateTime.Parse(deadline, System.Globalization.CultureInfo.GetCultureInfo("en-us"));
                            }
                            catch (Exception ex)
                            {
                                VisitReportsTasksVM taskVM = new VisitReportsTasksVM(idTicket, idVisitReport);
                                ModelState.AddModelError("", ex.Message);
                                return View(taskVM);
                            }
                        }
                    }
                    repTasks.Create(task);

                    vVisitReports visitReport = repVisitReports.getVByTicket(task.idTicket);
                    return RedirectToAction("Details", "VisitReports", new { id = visitReport.id });
                }
            }
            catch (Exception ex)
            {
                VisitReportsTasksVM taskVM = new VisitReportsTasksVM(idTicket, idVisitReport);
                ModelState.AddModelError("", ex.Message);
                return View(taskVM);
            }
        }

       /* [HttpPost]
        public ActionResult Create(VisitReportsTasksVM vm)
        {
            try
            {
                // TODO: Add insert logic here
                using (repoSubtasks repTasks = new repoSubtasks())
                using (repoVisitReports repVisitReports = new repoVisitReports())
                {
                    vm.task.internalTask = false;
                    VisitReportsTasksManager visitReportsTasksMgr = new VisitReportsTasksManager();
                    SubTasks task = visitReportsTasksMgr.setAssignTo(vm.task);
                    repTasks.Create(task);
                    
                    vVisitReports visitReport = repVisitReports.getVByTicket(vm.task.idTicket);
                    return RedirectToAction("Edit", "VisitReports", new { id = visitReport.id });
                }                
            }
            catch (Exception ex)
            {
                ViewBag.idTicket = vm.ticket.id;
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }*/

        [HttpPost]
        [LogFilter]
        [ValidateInput(false)]
        public ActionResult ajaxForm(string content, string deadline, string assignTo, int idTask, int idVisitReport)
        {
            using (repoSubtasks repTasks = new repoSubtasks())
            {
                try
                {
                    SubTasks task = repTasks.get(idTask);
                    if (task != null)
                    {
                        task.assignTo = assignTo;
                        VisitReportsTasksManager visitReportsTasksMgr = new VisitReportsTasksManager();
                        task = visitReportsTasksMgr.setAssignTo(task);
                        VRSecurityMgr vrSecurityMgr = new VRSecurityMgr();
                        task.description = vrSecurityMgr.sanitizeXSSAttacks(content);
                        if (deadline != null && deadline != "")
                            task.deadline = DateTime.Parse(deadline, System.Globalization.CultureInfo.GetCultureInfo("es-es"));
                        repTasks.Edit(task);
                        return RedirectToAction("Details", "VisitReports", new { id = idVisitReport });
                    }
                    else
                        return RedirectToAction("Details", "VisitReports", new { id = idVisitReport, message = "Task did not exists" });                    
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Details", "VisitReports", new { id = idVisitReport, message = ex.Message });
                }
            }
        }

        [HttpPost]
        public string ajaxEdit(string description, string deadline, string responsible, int id)
        {
            using (repoSubtasks repTasks = new repoSubtasks())
            {
                try
                {
                    SubTasks task = repTasks.get(id);
                    if (task != null)
                    {
                        task.assignTo = responsible;
                        VisitReportsTasksManager visitReportsTasksMgr = new VisitReportsTasksManager();
                        task = visitReportsTasksMgr.setAssignTo(task);
                        VRSecurityMgr vrSecurityMgr = new VRSecurityMgr();
                        task.description = vrSecurityMgr.sanitizeXSSAttacks(description);
                        if (deadline != null && deadline != "")
                            task.deadline = DateTime.Parse(deadline, System.Globalization.CultureInfo.GetCultureInfo("es-es"));
                        repTasks.Edit(task);
                        return "1";
                    }
                    else return "Task did not exists";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        [LogFilter]
        public string ajaxDelete(int id)
        {
            using (repoSubtasks repTasks = new repoSubtasks())
            {
                try
                {
                    SubTasks task = repTasks.get(id);
                    if (task != null)
                    {
                        repTasks.Delete(task);
                        return "1";
                    }
                    else return "Task did not exists";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        public string ajaxChangeStatus(int newStatus, int id)
        {
            using (repoSubtasks repTasks = new repoSubtasks())
            {
                try
                {
                    SubTasks task = repTasks.get(id);
                    if (task != null)
                    {
                        if (newStatus == 0) task.solved = false;
                        else task.solved = true;
                        repTasks.Edit(task);
                        return "1";
                    }
                    else return "Task did not exists";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }

        [LogFilter]
        public ActionResult changeStatus(int newStatus, int id, string returningAction)
        {
            using (repoSubtasks repTasks = new repoSubtasks())
            {
                try
                {
                    SubTasks task = repTasks.get(id);
                    if (task != null)
                    {
                        if (newStatus == 0) task.solved = false;
                        else task.solved = true;
                        repTasks.Edit(task);                        
                    }                    
                }
                catch (Exception ex)
                {                   
                }
                return RedirectToAction(returningAction);
            }
        }
    }
}
