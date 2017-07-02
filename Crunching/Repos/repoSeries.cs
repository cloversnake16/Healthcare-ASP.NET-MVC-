using Crunching.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Data;

namespace GTPTracker.repos
{
    public class repoSeries : IDisposable
    {
        public bool serieExists(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Series.Any(p => p.id == id);
            }
        }

        public Series GetSerieBySerieAndCategory(int serie, int category)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Series.FirstOrDefault(p => p.serie == serie && p.category == category);
            }
        }

        public IEnumerable<Series> getList()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Series.ToList();
            }
        }

        public Series get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Series.FirstOrDefault(p => p.id == id);
            }
        }

        public void Create(Series item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Series.AddObject(item);
                db.SaveChanges();
            }
        }

        public void Edit(Series item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Series.FirstOrDefault(p => p.id == item.id);
                db.Series.ApplyCurrentValues(item);
                db.SaveChanges();
            }
        }

        public void Delete(Series item)
        {
            using (var db = new GTPTrackerEntities())
            {
                Series des = db.Series.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.Series.DeleteObject(des);
                    db.SaveChanges();
                }
            }
        }

        internal bool SaveBulk(List<Series> bulk)
        {
            using (var db = new GTPTrackerEntities())
            {
                foreach (var item in bulk)
                {
                    if (item.id != 0)
                    {
                        db.Series.Attach(item);
                        db.ObjectStateManager.ChangeObjectState(item, EntityState.Modified);
                    }
                    else
                    {
                        db.Series.AddObject(item);
                    }
                }

                var updates = db.SaveChanges();
                return updates > 0;
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