using Crunching.Models;
using System.Collections.Generic;
using System.Linq;
using GTPTracker.Functions;

namespace GTPTracker.repos
{
    public class repoComments
    {
        public IEnumerable<vComments> getvList(int idTicket)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vComments.Where(p => p.idTicket == idTicket).ToList();
            }
        }

        public Comments get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Comments.FirstOrDefault(p => p.id == id);
            }
        }

        public void Create(Comments item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Comments.AddObject(item);
                db.SaveChanges();
            }
            ItemsManager itemsMgr = new ItemsManager();
            itemsMgr.updateTicketItem(item.idTicket);
        }

        public void Edit(Comments item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Comments.FirstOrDefault(p => p.id == item.id);
                db.Comments.ApplyCurrentValues(item);
                db.SaveChanges();
            }
            ItemsManager itemsMgr = new ItemsManager();
            itemsMgr.updateTicketItem(item.idTicket);
        }

        public void Delete(Comments item)
        {
            using (var db = new GTPTrackerEntities())
            {
                Comments des = db.Comments.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.Comments.DeleteObject(des);
                    db.SaveChanges();
                }
            }
            ItemsManager itemsMgr = new ItemsManager();
            itemsMgr.updateTicketItem(item.idTicket);
        }
    }
}