using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;

namespace GTPTracker.repos
{
    public class repoItems
    {
        public IEnumerable<Items> getList()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Items.ToList();
            }
        }

        public Items getByLink(int type, int link)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Items.FirstOrDefault(p => p.link == link && p.idType == type);
            }
        }

        public Items Get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Items.FirstOrDefault(p => p.id == id);
            }
        }

        public int Create(Items item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Items.AddObject(item);
                db.SaveChanges();
            }
            return item.id;
        }

        public void Edit(Items item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Items.FirstOrDefault(p => p.id == item.id);
                db.Items.ApplyCurrentValues(item);
                db.SaveChanges();
            }
        }

        public void Delete(Items item)
        {
            using (var db = new GTPTrackerEntities())
            {
                Items des = db.Items.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.Items.DeleteObject(des);
                    db.SaveChanges();
                }
            }
        }
        //Items marked as internal are not showed if you are not a GTP user
        public IEnumerable<vWatchListItems> getMyItems(int idUser)
        {
            // User GTP sees all the items. Including internal
            // USer not GTP :
            //      assigned or creator : see the ticket no matter if internal
            using (var db = new GTPTrackerEntities())
            {
                if ((int)HttpContext.Current.Session["IDTENANT"] == 1) //.User.IsInRole("GTP"))
                    return db.vWatchListItems.Where(p => p.idUser == idUser).OrderByDescending(p => p.cDate).ToList();
                else
                {
                    IEnumerable<int> idticketsList = db.vWatchListItems.Where(p => p.idUser == idUser && p.idType == 1).Select(p => (int)p.link);
                    using (repoTickets repTickets = new repoTickets())
                    {
                        IEnumerable<Tickets> ticketsList = repTickets.getList().Where(p => idticketsList.Contains(p.id)); // tickets corresponding to that items where I'm in the watchlist
                        ticketsList = ticketsList.Where(p => p.isInternal != true || (p.isInternal == true && p.idResponsible == idUser) || (p.isInternal == true && p.idCreator == idUser));
                        IEnumerable<int> idTicketsForMe = ticketsList.Select(p => p.id);
                       // IEnumerable<vWatchListItems> MyItemsTickets = db.vWatchListItems.Where(p => (p.idType == 1 && p.idUser == idUser && idTicketsForMe.Contains((int)p.link)));
                        IEnumerable<vWatchListItems> result = db.vWatchListItems.Where(p => (p.idUser == idUser && p.idType != 1) || (p.idType == 1 && p.idUser == idUser && idTicketsForMe.Contains((int)p.link))).ToList();
                        return result;
                    }
                   // return db.vWatchListItems.Where(p => p.idUser == idUser && p.isInternal == false).OrderByDescending(p => p.cDate).ToList();
                }
            }
        }

        public void ArchiveItemForTicket(int idTicket)
        {
            using (var db = new GTPTrackerEntities())
            {
                Items item = db.Items.FirstOrDefault(e => e.link == idTicket && e.idType == 1);
                item.archived = true;
                db.Items.ApplyCurrentValues(item);
                db.SaveChanges();
            }
        }

        public void UnarchiveItemForTicket(int idTicket)
        {
            using (var db = new GTPTrackerEntities())
            {
                Items item = db.Items.FirstOrDefault(e => e.link == idTicket && e.idType == 1);
                item.archived = false;
                db.Items.ApplyCurrentValues(item);
                db.SaveChanges();
            }
        }
    }
}