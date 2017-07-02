using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GTPTracker.VM;
using Crunching.Models;

namespace GTPTracker.Controllers
{
    public class ManagementController : Controller
    {
        
        public ActionResult Summary()
        {
            ManagementSummaryVM vm = new ManagementSummaryVM();
            return View(vm);
        }

        public ActionResult listNotProcessedFiles(int? actionToTake)
        {
            ViewBag.action = actionToTake;
            IEnumerable<NonProcessedFiles> lNonProcessedFiles = Enumerable.Empty<NonProcessedFiles>();
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            {
                FileBatchUploads lastBatchUpload = db.FileBatchUploads.Where(p => p.status == "completed").OrderByDescending(p => p.id).FirstOrDefault();
                if (lastBatchUpload != null)
                {
                    int lastBatchUploadId = lastBatchUpload.id;
                    lNonProcessedFiles = db.NonProcessedFiles.Where(p => p.idBatchUpdate == lastBatchUploadId).ToList();
                }
            }
            return View(lNonProcessedFiles);
        }
    }
}
