using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GTPTracker.VM;
using GTPTracker.Functions;
using GTPTracker.repos;
using GTPTracker.BL;
using System.IO;
using Crunching.Models;
using GTPTracker.Helpers;
using GTPTracker.Functions;

namespace Crunching.Controllers
{
    public class VisitReportsController : Controller
    {
        [SessionExpireFilter]
        [LogFilter]
        public ActionResult Index(int? idCountry, int? customer, int? responsible, int? idShow, string showDoneReports)
        {
            VisitReportsIndexVM vm = new VisitReportsIndexVM(idCountry, customer, responsible, showDoneReports, (int)Session["IDUSER"]);
            return View(vm);
        }

        [SessionExpireFilter]
        [LogFilter]
        public ActionResult Details(int id, string message)
        {
            VisitReportsVM vm = new VisitReportsVM(id, (int) Session["IDUSER"], message);
            return View(vm);
        }

        public ActionResult addFile(int id)
        {
            ViewBag.idVisitReport = id;
            return View();
        }

        [LogFilter]
        [HttpPost]
        public ActionResult addFile(int idVisitReport, IEnumerable<HttpPostedFileBase> files)
        {
            try
            {
                string message = "";
                var directory = Server.MapPath("~/Content/VisitReportsFiles"); //TODO put the VR files in a separate folder in Content (test and production hosting servers)
                using (repoVisitReportsFiles repVisitReportsFiles = new repoVisitReportsFiles())
                {
                    foreach (HttpPostedFileBase file in files)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            if (file.ContentLength < 20000000)
                            {
                                // save the file                
                                var path = System.IO.Path.Combine(directory, file.FileName);
                                file.SaveAs(path);
                                // write a record in visitReportsFiles table
                                VisitReportsFiles visitReportFile = new VisitReportsFiles();
                                visitReportFile.idVisitReport = idVisitReport;
                                visitReportFile.fileName = file.FileName;
                                visitReportFile.fullPath = path;
                                visitReportFile.cDate = DateTime.UtcNow;
                                repVisitReportsFiles.Create(visitReportFile);
                            }
                            else message = "The maximum size of a file is 20 Mb";
                        }
                    }
                }
                return RedirectToAction("Details", "VisitReports", new { id = idVisitReport, message = message });
            }
            catch (Exception ex)
            {
                ViewBag.idVisitReport = idVisitReport;
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("Details", "VisitReports", new { id = idVisitReport, message = ex.Message });
                //return View();
            }
        }

        [SessionExpireFilter]
        public ActionResult Create()
        {
            VisitReportsVM vm = new VisitReportsVM((int) Session["IDUSER"]);
            return View(vm);
        }

        [LogFilter]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Create(VisitReportsVM vm, IEnumerable<HttpPostedFileBase> files, string visitDate, string isInternal, int idStatus = 0) {
            VRSecurityMgr vrSecurityMgr = new VRSecurityMgr();
            vm.ticket.description =  vrSecurityMgr.sanitizeXSSAttacks(vm.content);
            if (visitDate != null && visitDate!="")
            {
                try
                {
                    vm.visitReport.visitDate = DateTime.Parse(visitDate, System.Globalization.CultureInfo.GetCultureInfo("es-es"));
                    vm.ticket.idStatus = idStatus;
                    if (isInternal == "on") vm.ticket.isInternal = true;
                    else vm.ticket.isInternal = false;
                }
                catch
                {
                    try
                    {
                        vm.visitReport.visitDate = DateTime.Parse(visitDate, System.Globalization.CultureInfo.GetCultureInfo("en-us"));
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                        return View(vm);
                    }
                }
            }
            var directory = Server.MapPath("~/Content/VisitReportsFiles"); 
            VisitReportsManager visitReportsMgr = new VisitReportsManager();
            string result = visitReportsMgr.Create(vm, (int)Session["IDUSER"], directory, files);
            if (result == "")
                return RedirectToAction("Index");
            else {
                ModelState.AddModelError("",  result);
                return View(vm);
            }
        }

        [SessionExpireFilter]
        public ActionResult Edit(int id, string message)
        {
            VisitReportsVM vm = new VisitReportsVM(id, (int) Session["IDUSER"], message);
            return View(vm);
        }

        [SessionExpireFilter]
        [HttpPost]
        [LogFilter]
        public ActionResult Edit(VisitReportsVM vm, string visitDate, string isInternal, int idStatus = 0)
        {
            VRSecurityMgr vrSecurityMgr = new VRSecurityMgr();
            vm.ticket.description = vrSecurityMgr.sanitizeXSSAttacks(vm.content);
            if (visitDate != null && visitDate != "")
            {
                vm.ticket.idStatus = idStatus;
                if (isInternal == "on") vm.ticket.isInternal = true;
                else vm.ticket.isInternal = false;
                try
                {
                    vm.visitReport.visitDate = DateTime.Parse(visitDate, System.Globalization.CultureInfo.GetCultureInfo("es-es"));
                }
                catch
                {
                    try { vm.visitReport.visitDate = DateTime.Parse(visitDate, System.Globalization.CultureInfo.GetCultureInfo("en-us")); }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                        return View(vm);
                    }
                }
            }
            else vm.visitReport.visitDate = DateTime.Today;
            VisitReportsManager visitReportsMgr = new VisitReportsManager();
            string result = visitReportsMgr.Edit(vm, (int)Session["IDUSER"]);
            if (result == "")
                return RedirectToAction("Details", new { id = vm.visitReport.id });
            else
            {
                ModelState.AddModelError("", result);
                return View(vm);
            }
        }

        
        public string ajaxChangeStatus(int idVisitReport, int newStatus)
        {
            VisitReportsManager visitReportsMgr = new VisitReportsManager();
            string result = visitReportsMgr.changeStatus(idVisitReport, newStatus, (int)Session["IDUSER"]);
            return result;
        }

        [SessionExpireFilter]
        [LogFilter]
        public ActionResult changeStatus(int idVisitReport, int newStatus, string returningAction)
        {
            VisitReportsManager visitReportsMgr = new VisitReportsManager();
            string result = visitReportsMgr.changeStatus(idVisitReport, newStatus, (int)Session["IDUSER"]);
            if (returningAction == "Details")
                return RedirectToAction("Details", "VisitReports", new { id = idVisitReport });
            if (returningAction == "Index")
                return RedirectToAction("Index", "VisitReports");
            if (returningAction == "Edit")
                return RedirectToAction("Edit", "VisitReports", new { id = idVisitReport });
            return RedirectToAction("Index", "VisitReports");
        }
    }
}
