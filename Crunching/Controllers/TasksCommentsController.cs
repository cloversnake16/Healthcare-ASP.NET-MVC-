using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GTPTracker.repos;
using Crunching.Models;
using GTPTracker.Helpers;
using System.IO;
using GTPTracker.Functions;

namespace GTPTracker.Controllers
{
    public class TasksCommentsController : Controller
    {
        repoTasksComments repTasksComments = new repoTasksComments();

        [SessionExpireFilter]
        public ActionResult Create(int idTask)
        {
            ViewBag.idTask = idTask;
            TasksComments taskComment = new TasksComments();
            taskComment.idTask = idTask;
            return View(taskComment);
        }

        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Create(TasksComments taskComment)
        {
            try
            {
                taskComment.cDate = DateTime.UtcNow;
                taskComment.idCreator = (int)Session["IDUSER"];
                repTasksComments.Create(taskComment);
                using (repoSubtasks repTasks = new repoSubtasks())
                {
                    SubTasks task = repTasks.get(taskComment.idTask);
                    ItemsManager itemsMgr = new ItemsManager();
                    itemsMgr.updateTicketItem(task.idTicket);
                    return RedirectToAction("Details", "SubTasks", new { id = taskComment.idTask });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(taskComment);
            }
        }
        public JsonResult CreateJSON(int idTask, string Comment, bool gtpInternal = false)
        {
            try
            {
                TasksComments taskComment = new TasksComments();
                taskComment.idTask = idTask;
                taskComment.cDate = DateTime.UtcNow;
                taskComment.idCreator = (int)Session["IDUSER"];
                taskComment.text = Comment;
                taskComment.gtpInternal = gtpInternal;
                repTasksComments.Create(taskComment);

                using (repoSubtasks repTasks = new repoSubtasks())
                {
                    SubTasks task = repTasks.get(idTask);
                    ItemsManager itemsMgr = new ItemsManager();
                    itemsMgr.updateTicketItem(task.idTicket);

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
