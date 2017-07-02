using Crunching.Models;
using System.Collections.Generic;
using System.Linq;

namespace GTPTracker.repos
{
    public class repoTicketsFiles
    {
        public IEnumerable<TicketsFiles> getList(int idTicket)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.TicketsFiles.Where(p => p.idTicket == idTicket).ToList();
            }
        }

        public TicketsFiles Get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.TicketsFiles.FirstOrDefault(p => p.id == id);
            }
        }

        public int Create(TicketsFiles item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.TicketsFiles.AddObject(item);
                db.SaveChanges();
            }
            return item.id;
        }

        public void Edit(TicketsFiles item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.TicketsFiles.FirstOrDefault(p => p.id == item.id);
                db.TicketsFiles.ApplyCurrentValues(item);
                db.SaveChanges();
            }
        }

        public void Delete(TicketsFiles item)
        {
            using (var db = new GTPTrackerEntities())
            {
                TicketsFiles des = db.TicketsFiles.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.TicketsFiles.DeleteObject(des);
                    db.SaveChanges();
                }
            }
        }

    }
}