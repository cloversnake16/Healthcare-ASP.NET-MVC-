using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;

namespace GTPTracker.repos
{
    public class repoCategories : IDisposable
    {
        public IEnumerable<Categories> getList()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Categories.ToList();
            }
        }

        public Categories get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Categories.FirstOrDefault(p => p.id == id);
            }
        }

        public void Create(Categories item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Categories.AddObject(item);
                db.SaveChanges();
            }
        }

        public void Edit(Categories item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Categories.FirstOrDefault(p => p.id == item.id);
                db.Categories.ApplyCurrentValues(item);
                db.SaveChanges();
            }
        }

        public void Delete(Categories item)
        {
            using (var db = new GTPTrackerEntities())
            {
                Categories des = db.Categories.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.Categories.DeleteObject(des);
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