using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GTPTracker.repos;
using Crunching.Models;
using GTPTracker.Helpers;
using System.IO;

namespace GTPTrackers.Controllers
{
    public class TaskFilesController : Controller
    {
        repoTaskFiles repFiles = new repoTaskFiles();

        [SessionExpireFilter]
        public ActionResult Create(int idTask)
        {
            ViewBag.idTask = idTask;
            return View();
        }

        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Create(int idTask, HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    if (file.ContentLength > 20000000)
                    {
                        ModelState.AddModelError("", "File is too big. Max size allowed 20 MB");
                        ViewBag.idTask = idTask;
                        return View(idTask);
                    }
                    var fileName = "FileTask_" + Convert.ToString(idTask) + "__" + DateTime.UtcNow.Ticks.ToString() + "-" + System.IO.Path.GetFileName(file.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    file.SaveAs(path);

                    //if (Path.GetExtension(fileName) == ".pdf") // if is a pdf save it in the files table
                    {
                        TasksFiles tFile = new TasksFiles();
                        tFile.name = fileName;
                        tFile.idTask = idTask;
                        tFile.idCreator = 1001;
                        tFile.cDate = DateTime.UtcNow;
                        repFiles.Create(tFile);
                    }
                }
                return RedirectToAction("Details", "SubTasks", new { id = idTask });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.idticket = idTask;
                return View(idTask);
            }
        }

        public JsonResult CreateJSON(int idTask, HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    if (file.ContentLength > 5000000)
                    {
                        ModelState.AddModelError("", "File is too big. Max size allowed 20 MB");
                        ViewBag.idTask = idTask;
                        return Json("File is too big. Max size allowed 20 MB", JsonRequestBehavior.AllowGet);
                    }
                    var fileName = "FileTask_" + Convert.ToString(idTask) + "__" + DateTime.UtcNow.Ticks.ToString() + "-" + System.IO.Path.GetFileName(file.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    file.SaveAs(path);

                    //if (Path.GetExtension(fileName) == ".pdf") // if is a pdf save it in the files table
                    {
                        TasksFiles tFile = new TasksFiles();
                        tFile.name = fileName;
                        tFile.idTask = idTask;
                        tFile.idCreator = 1001;
                        tFile.cDate = DateTime.UtcNow;
                        repFiles.Create(tFile);
                    }
                }
                return Json(1, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.idticket = idTask;
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [SessionExpireFilter]
        [HttpPost]
        public ActionResult CreateAndReturnToTicket(int idTask, HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    if (file.ContentLength > 20000000)
                    {
                        ModelState.AddModelError("", "File is too big. Max size allowed 20 MB");
                        ViewBag.idTask = idTask;
                        return View(idTask);
                    }
                    var fileName = "FileTask_" + Convert.ToString(idTask) + "__" + DateTime.UtcNow.Ticks.ToString() + "-" + System.IO.Path.GetFileName(file.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    file.SaveAs(path);

                    //if (Path.GetExtension(fileName) == ".pdf") // if is a pdf save it in the files table
                    {
                        TasksFiles tFile = new TasksFiles();
                        tFile.name = fileName;
                        tFile.idTask = idTask;
                        tFile.idCreator = 1001;
                        tFile.cDate = DateTime.UtcNow;
                        repFiles.Create(tFile);
                    }
                }
                using (repoSubtasks repTasks = new repoSubtasks())
                {
                    SubTasks task = repTasks.get(idTask);
                    return RedirectToAction("Details", "Tickets", new { id = task.idTicket });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.idticket = idTask;
                return View(idTask);
            }
        }

    }
}
