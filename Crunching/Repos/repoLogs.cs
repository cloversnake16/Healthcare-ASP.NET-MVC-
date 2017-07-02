using System;
using System.Collections.Generic;
using System.Linq;
using Crunching.Models;

namespace GTPTracker.repos
{
    public class repoLogs : IDisposable
    {
        public IEnumerable<Logs> getList()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Logs.ToList();
            }
        }

        public Logs get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Logs.FirstOrDefault(p => p.id == id);
            }
        }

        public void Create(Logs item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Logs.AddObject(item);
                db.SaveChanges();
            }
        }

        public void Edit(Logs item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Logs.FirstOrDefault(p => p.id == item.id);
                db.Logs.ApplyCurrentValues(item);
                db.SaveChanges();
            }
        }

        public void Delete(Logs item)
        {
            using (var db = new GTPTrackerEntities())
            {
                Logs des = db.Logs.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.Logs.DeleteObject(des);
                    db.SaveChanges();
                }
            }
        }

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