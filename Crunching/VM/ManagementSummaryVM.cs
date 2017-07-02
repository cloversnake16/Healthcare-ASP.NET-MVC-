using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;

namespace GTPTracker.VM
{
    public class ManagementSummaryVM
    {
        public int totalProducts;
        public int totalDocuments;
        public int totalUsers;
        public int totalProductsFilesInHosting;
        public int totalProductsFilesInDB;
        public IEnumerable<FileBatchUploads> lFilesParsing;

        public int timeSinceLastUpdate;
        public int filesThatCouldNotBeProcessed;
        public int productsThatHaveNoFiles;

        public ManagementSummaryVM()
        {
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            {
                totalProducts = db.Products.Count();
                totalDocuments = db.GeneralProductsDocuments.Count();
                totalUsers = db.Users.Count();
                totalProductsFilesInDB = db.ProductsFiles.Count();
                lFilesParsing = db.FileBatchUploads.OrderByDescending(p=>p.id).ToList();

                // get the days since last update. Try getting the last update from fileBatchUploads. Else from settings
                DateTime lastBatchUpdateDate;
                FileBatchUploads lastBatchUpload = db.FileBatchUploads.OrderByDescending(o => o.id).FirstOrDefault();
                if (lastBatchUpload != null && lastBatchUpload.cDate != null)
                {
                    lastBatchUpdateDate = lastBatchUpload.cDate;
                    filesThatCouldNotBeProcessed = lastBatchUpload.noMatchFiles; // get from the fileBatchUploads the # of non processed files
                    totalProductsFilesInHosting = lastBatchUpload.processedFiles;// the total # of processed files in last batch upload is the # of files in the hosting.
                }
                else // get info from settings
                {
                    Settings lastSetting = db.Settings.OrderByDescending(o => o.Id).FirstOrDefault();
                    if (lastSetting != null && lastSetting.LastUpdate != null)
                        lastBatchUpdateDate = (DateTime)lastSetting.LastUpdate;
                    else lastBatchUpdateDate = DateTime.MinValue;
                }
                timeSinceLastUpdate = (DateTime.UtcNow - lastBatchUpdateDate).Days;

                productsThatHaveNoFiles = db.vProducts.Where(p => p.numFiles == 0).Count();
            }
        }
    }
}