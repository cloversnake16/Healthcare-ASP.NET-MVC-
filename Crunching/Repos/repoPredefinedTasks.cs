using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;

namespace GTPTracker.repos
{
    public class repoPredefinedTasks : IDisposable
    {

        public IEnumerable<PredefinedTasksCategories> getCategories()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.PredefinedTasksCategories.ToList();
            }
        }

        public PredefinedTasks get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.PredefinedTasks.FirstOrDefault(p => p.id == id);
            }
        }

        public IEnumerable<vPredefinedTasks> getAll()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vPredefinedTasks.ToList();
            }
        }

        public int Create(PredefinedTasks item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.PredefinedTasks.AddObject(item);
                db.SaveChanges();
            }
            return item.id;
        }

        public void Edit(PredefinedTasks item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.PredefinedTasks.FirstOrDefault(p => p.id == item.id);
                db.PredefinedTasks.ApplyCurrentValues(item);
                db.SaveChanges();
            }
        }

        public void Delete(PredefinedTasks item)
        {
            using (var db = new GTPTrackerEntities())
            {
                PredefinedTasks des = db.PredefinedTasks.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.PredefinedTasks.DeleteObject(des);
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