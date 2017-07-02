using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GTPTracker.repos;
using Crunching.Models;
using GTPTracker.Helpers;
using System.IO;

namespace GTPTracker.Controllers
{
    public class PhotosController : Controller
    {
        repoPhotos repPhotos = new repoPhotos();
        repoTicketsFiles repTicketFiles = new repoTicketsFiles();

        [SessionExpireFilter]
        public ActionResult Index()
        {
            return View();
        }

        [SessionExpireFilter]
        public ActionResult Details(int id)
        {
            return View();
        }

        [SessionExpireFilter]
        public ActionResult Create(int idTicket)
        {
            ViewBag.idticket = idTicket;
            return View();
        }

        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Create(int idTicket, HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    if (file.ContentLength > 20000000)
                    {
                        ModelState.AddModelError("", "File is too big. Max size allowed 20 MB");
                        ViewBag.idticket = idTicket;
                        return View(idTicket);
                    }
                    var fileName = "Photo_" + Convert.ToString(idTicket) + "__" + DateTime.UtcNow.Ticks.ToString() + "-" + System.IO.Path.GetFileName(file.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    file.SaveAs(path);

                    if (Path.GetExtension(fileName) == ".pdf") // if is a pdf save it in the files table
                    {
                        TicketsFiles tFile = new TicketsFiles();
                        tFile.fileName = file.FileName;
                        tFile.idTicket = idTicket;
                        tFile.URI = fileName;
                        repTicketFiles.Create(tFile);
                    }
                    else // is a photo, save it in the photo table
                    {
                        Photos photo = new Photos();
                        photo.cDate = DateTime.Now;
                        photo.idCreator = 1001;
                        photo.idTicket = idTicket;
                        photo.name = fileName;
                        repPhotos.Create(photo);
                    }
                }
                return RedirectToAction("Details", "Tickets", new { id = idTicket });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Error = ex.Message;
                ViewBag.idticket = idTicket;
                return View(idTicket);
            }
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
            return View();
        }

        [SessionExpireFilter]
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

        public JsonResult CreateJSON(int idTicket, HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    if (file.ContentLength > 20000000)
                    {
                        ModelState.AddModelError("", "File is too big. Max size allowed 5 MB");
                        ViewBag.idticket = idTicket;
                        return Json("File is too big. Max size allowed 20 MB", JsonRequestBehavior.AllowGet);
                    }
                    var fileName = "Photo_" + Convert.ToString(idTicket) + "__" + DateTime.UtcNow.Ticks.ToString() + "-" + System.IO.Path.GetFileName(file.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    file.SaveAs(path);

                    if (Path.GetExtension(fileName) == ".pdf") // if is a pdf save it in the files table
                    {
                        TicketsFiles tFile = new TicketsFiles();
                        tFile.fileName = file.FileName;
                        tFile.idTicket = idTicket;
                        tFile.URI = fileName;
                        repTicketFiles.Create(tFile);
                    }
                    else // is a photo, save it in the photo table
                    {
                        Photos photo = new Photos();
                        photo.cDate = DateTime.Now;
                        photo.idCreator = 1001;
                        photo.idTicket = idTicket;
                        photo.name = fileName;
                        repPhotos.Create(photo);
                    }
                }
                return Json(idTicket, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.idticket = idTicket;
                return Json("File is too big. Max size allowed 5 MB", JsonRequestBehavior.AllowGet);
            }
        }
    }
}
