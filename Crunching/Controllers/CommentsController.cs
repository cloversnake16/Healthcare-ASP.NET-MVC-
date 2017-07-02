using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GTPTracker.repos;
using Crunching.Models;
using GTPTracker.Helpers;
using GTPTracker.VM;

namespace GTPTracker.Controllers
{
    public class CommentsController : Controller
    {
        repoComments repComments = new repoComments();
        repoCommentFiles repFiles = new repoCommentFiles();

        [SessionExpireFilter]
        public ActionResult Index(int idTicket)
        {
            return View();
        }

        [SessionExpireFilter]
        public ActionResult Details(int id)
        {
            CommentDetailsVM vm = new CommentDetailsVM(id);
            return View(vm);
        }

        [SessionExpireFilter]
        public ActionResult Create(int idTicket)
        {
            Comments comment = new Comments();
            comment.idCreator = (int)Session["IDUSER"];
            comment.idTicket = idTicket;
            if (HttpContext.User.IsInRole("GTP"))
                comment.gtpInternal = true;
            return View(comment);
        }

        [HttpPost]
        [SessionExpireFilter]
        public ActionResult Create(Comments comment)
        {
            try
            {
                comment.idCreator = (int)Session["IDUSER"];
                comment.cDate = DateTime.Now;
                repComments.Create(comment);
                return RedirectToAction("Details", "Tickets", new { id = comment.idTicket, tab=1, goToAnchor = 1 });
            }
            catch
            {
                return View();
            }
        }

        public JsonResult Create(int idTicket, string text, bool gtpInternal=false)
        {
            try
            {
                Comments comment = new Comments();
                comment.idCreator = (int)Session["IDUSER"];
                comment.idTicket = idTicket;
         /*       if (HttpContext.User.IsInRole("GTP"))
                    comment.gtpInternal = true;*/
                comment.idCreator = (int)Session["IDUSER"];
                comment.cDate = DateTime.Now;
                comment.text = text;
                comment.gtpInternal = gtpInternal;
                repComments.Create(comment);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch 
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CreateJSON(int idTicket, string Comment, bool gtpInternal = false)
        {
            try
            {
                Comments comment = new Comments();
                comment.idCreator = (int)Session["IDUSER"];
                comment.idTicket = idTicket;
          /*      if (HttpContext.User.IsInRole("GTP"))
                    comment.gtpInternal = true;*/
                comment.idCreator = (int)Session["IDUSER"];
                comment.cDate = DateTime.Now;
                comment.text = Comment;
                comment.gtpInternal = gtpInternal;
                repComments.Create(comment);
                //return Json(true, JsonRequestBehavior.AllowGet);
                return RedirectToAction("Details", "Tickets", new { id = idTicket});
            }
            catch 
            {
                return RedirectToAction("Details", "Tickets", new { id = idTicket });
            }
        }

        [SessionExpireFilter]
        public ActionResult CreateFile(int idComment)
        {
            ViewBag.idComment = idComment;
            Comments comment = repComments.get(idComment);
            ViewBag.idTicket = comment.idTicket;
            return View();
        }

        [SessionExpireFilter]
        [HttpPost]
        public ActionResult CreateFile(int idComment, HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    if (file.ContentLength > 20000000)
                    {
                        ModelState.AddModelError("", "File is too big. Max size allowed 20 MB");
                        ViewBag.idComment = idComment;
                        return View(idComment);
                    }
                    var fileName = "FileComment_" + Convert.ToString(idComment) + "__" + DateTime.UtcNow.Ticks.ToString() + "-" + System.IO.Path.GetFileName(file.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    file.SaveAs(path);

                    //if (Path.GetExtension(fileName) == ".pdf") // if is a pdf save it in the files table
                    {
                        CommentsFiles tFile = new CommentsFiles();
                        tFile.name = fileName;
                        tFile.idComment = idComment;
                        tFile.idCreator = 1001;
                        tFile.cDate = DateTime.UtcNow;
                        repFiles.Create(tFile);
                    }
                }
                
                return RedirectToAction("Details", "Comments", new { id = idComment });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.idComment = idComment;
                return View(idComment);
            }
        }

        public JsonResult CreateFileJSON(int idComment, HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    if (file.ContentLength > 20000000)
                    {
                        ModelState.AddModelError("", "File is too big. Max size allowed 20 MB");
                        ViewBag.idComment = idComment;
                        return Json("File is too big. Max size allowed 5 MB", JsonRequestBehavior.AllowGet);
                    }
                    var fileName = "FileComment_" + Convert.ToString(idComment) + "__" + DateTime.UtcNow.Ticks.ToString() + "-" + System.IO.Path.GetFileName(file.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    file.SaveAs(path);

                    //if (Path.GetExtension(fileName) == ".pdf") // if is a pdf save it in the files table
                    {
                        CommentsFiles tFile = new CommentsFiles();
                        tFile.name = fileName;
                        tFile.idComment = idComment;
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
                ViewBag.idComment = idComment;
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [SessionExpireFilter]
        public ActionResult Delete(int id)
        {
            Comments comment = repComments.get(id);
            return View(comment);
        }

        [SessionExpireFilter]
        [HttpPost]
        [Authorize(Roles = "GTP")]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                Comments comment = repComments.get(id);
                repComments.Delete(comment);
                return RedirectToAction("Details", "Tickets", new { id = comment.idTicket, tab = 1, goToAnchor = 1 });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(id);
            }
        }

        
    }
}
