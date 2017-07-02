using Crunching.Models;
using System.Collections.Generic;
using System.Linq;
using GTPTracker.Functions;

namespace GTPTracker.repos
{
    public class repoPhotos
    {
        public bool alreadyExists(string fileName)
        {
            using (var db = new GTPTrackerEntities())
            {
                Photos photo = db.Photos.FirstOrDefault(p => p.name.EndsWith(fileName));
                return photo != null;
            }
        }

        public IEnumerable<Photos> getList(int idTicket)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Photos.Where(p => p.idTicket == idTicket).ToList();
            }
        }

        public Photos Get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Photos.FirstOrDefault(p => p.id == id);
            }
        }

        public int Create(Photos item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Photos.AddObject(item);
                db.SaveChanges();
            }
            ItemsManager itemsMgr = new ItemsManager();
            itemsMgr.updateTicketItem(item.idTicket);
            return item.id;
        }

        public void Edit(Photos item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Photos.FirstOrDefault(p => p.id == item.id);
                db.Photos.ApplyCurrentValues(item);
                db.SaveChanges();
            }
            ItemsManager itemsMgr = new ItemsManager();
            itemsMgr.updateTicketItem(item.idTicket);
        }

        public void Delete(Photos item)
        {
            using (var db = new GTPTrackerEntities())
            {
                Photos des = db.Photos.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.Photos.DeleteObject(des);
                    db.SaveChanges();
                }
            }
            ItemsManager itemsMgr = new ItemsManager();
            itemsMgr.updateTicketItem(item.idTicket);
        }

    }
}