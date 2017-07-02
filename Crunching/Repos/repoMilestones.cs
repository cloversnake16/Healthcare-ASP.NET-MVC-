using Crunching.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using GTPTracker.Functions;

namespace GTPTracker.repos
{
    public class repoMilestones:IDisposable
    {
        public IEnumerable<Milestones> getList()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Milestones.ToList();
            }
        }

        public Milestones get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Milestones.FirstOrDefault(p => p.id == id);
            }
        }

        public void Create(Milestones item)
        {
            using (var db = new GTPTrackerEntities())
            {
                item.cDate = DateTime.UtcNow;
                db.Milestones.AddObject(item);
                db.SaveChanges();
                repoItems repItems = new repoItems();
                Items element = new Items();
                element.cDate = DateTime.UtcNow;
                element.title = "Milestone " + item.activationDate;
                element.idType = 4;
                element.link = item.id;
                element.isInternal = false;
                repItems.Create(element);
            }
        }

        public void Edit(Milestones item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Milestones.Where(p => p.id == item.id).FirstOrDefault();
                db.Milestones.ApplyCurrentValues(item);
                db.SaveChanges();
                ItemsManager itemsMgr = new ItemsManager();
                itemsMgr.updateMilestoneItem(item.id);
            }
        }

        public void addUser(int idMilestone, int idUser)
        {
            using (var db = new GTPTrackerEntities())
            {
                MilestonesWatchList element = new MilestonesWatchList();
                element.idMilestone = idMilestone;
                element.idUser = idUser;
                db.MilestonesWatchList.AddObject(element);
                db.SaveChanges();
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