using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;

namespace GTPTracker.repos
{
    public class repoVisitReportsFiles : IDisposable
    {
        public int Create(VisitReportsFiles item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.VisitReportsFiles.AddObject(item);
                db.SaveChanges();
            }
            return item.id;
        }

        public IEnumerable<VisitReportsFiles> getAllByidVisitReport(int idVisitReport)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.VisitReportsFiles.Where(p=>p.idVisitReport == idVisitReport).ToList();
            }
        }

        /**********************************************/
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
            }
            // free native resources if there are any.
        }
    }
}