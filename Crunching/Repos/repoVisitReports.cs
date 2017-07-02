using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;

namespace GTPTracker.repos
{
    public class repoVisitReports : IDisposable
    {
        public IEnumerable<vVisitReports> getAll() 
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vVisitReports.ToList();
            }
        }

        public vVisitReports getV(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vVisitReports.FirstOrDefault(p => p.id == id);
            }
        }

        public vVisitReports getVByTicket(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vVisitReports.FirstOrDefault(p => p.idTicket == id);
            }
        }

        public VisitReports get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.VisitReports.FirstOrDefault(p => p.id == id);
            }
        }

        public int Create(VisitReports item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.VisitReports.AddObject(item);
                db.SaveChanges();                
            }
            return item.id;
        }

        public void Edit(VisitReports item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.VisitReports.FirstOrDefault(p => p.id == item.id);
                db.VisitReports.ApplyCurrentValues(item);
                db.SaveChanges();                
            }
        }

        public void Delete(Tickets item)
        {
            using (var db = new GTPTrackerEntities())
            {
                VisitReports des = db.VisitReports.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.VisitReports.DeleteObject(des);
                    db.SaveChanges();
                }
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