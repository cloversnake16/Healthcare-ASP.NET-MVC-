using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.Functions;

namespace GTPTracker.repos
{
    public class repoTickets : IDisposable
    {
        repoUsers repUsers = new repoUsers();

        public IEnumerable<Tickets> getList()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Tickets.ToList();
            }
        }

        public IEnumerable<vTickets> getMyTickets(int idUser, bool isGTP, int idTenant)
        {
            using (var db = new GTPTrackerEntities())
            {
                // Si NO es gtp
                // obtener todos los tickets donde estoy en la lista de distribucion
                // si un ticket tiene la lista de distribucion vacia, se añade
                IEnumerable<int> lDistribution = db.TicketsMailingList.Where(p => p.idUser == idUser).Select(p => p.idTicket).ToList();
                IEnumerable<vTickets> lTickets = db.vTickets.Where(p => lDistribution.Contains(p.id)).ToList();

                // GTP. Return only tickets where I'm at distribution list
              //  if (isGTP)
                    return lTickets;
/*
                IEnumerable<int> lTicketsWithDistributionList = db.vTicketDistributionList.Where(p => p.idTenant == idTenant).Select(p => p.idTicket).ToList();
                IEnumerable<int> allTickets = db.Tickets.Where(p => p.idTenant == idTenant).Select(p => p.id).ToList();
                IEnumerable<int> lTicketsWithoutDistributionList = allTickets.Except(lTicketsWithDistributionList).ToList();
                if (!lTicketsWithoutDistributionList.Any())
                    return lTickets;

                return lTickets.Concat(lTicketsWithoutDistributionList.Select(getv));
 * */
            }
        }

        /// <summary>
        /// returns a list with the vTickets where I, as a GTP user, I'm in the distribution list. Users are entered in the distribution list in the API, where the ticket is created used the web and in the ticket details
        /// </summary>
        /// <param name="idUser">isUser. Always from GTP</param>
        /// <returns>an Ienumerable with the list of vTickets where I' at the distribution list</returns>
        public IEnumerable<vTickets> getvList(int idUser)
        {
            using (var db = new GTPTrackerEntities())
            {
                IEnumerable<int> lDistribution = db.TicketsMailingList.Where(p => p.idUser == idUser).Select(p => p.idTicket).ToList();
                return db.vTickets.Where(p => lDistribution.Contains(p.id)).ToList();
            }
        }

        public Tickets get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Tickets.FirstOrDefault(p => p.id == id);
            }
        }

        public vTickets getv(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vTickets.FirstOrDefault(p => p.id == id);
            }
        }

        public vTickets getVTicket(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vTickets.FirstOrDefault(p => p.id == id);
            }
        }


        public int Create(Tickets item)
        {
            using (var db = new GTPTrackerEntities())
            {
                if (item.idTenant == 0) item.idTenant = 1; 
                db.Tickets.AddObject(item);
                db.SaveChanges();
                repoItems repItem = new repoItems();
                Items i = new Items();
                i.cDate = DateTime.UtcNow;
                i.idType = 1; // ticket
                i.title = "Ticket " + item.id;
                i.link = item.id;
                i.isInternal = item.isInternal;
                repItem.Create(i);
            }
            return item.id;
        }

        public void Edit(Tickets item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Tickets.FirstOrDefault(p => p.id == item.id);
                db.Tickets.ApplyCurrentValues(item);
                db.SaveChanges();
                ItemsManager itemsMgr = new ItemsManager();
                itemsMgr.updateTicketItem(item.id);
            }
        }

        public void Delete(Tickets item)
        {
            using (var db = new GTPTrackerEntities())
            {
                Tickets des = db.Tickets.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.Tickets.DeleteObject(des);
                    db.SaveChanges();
                }
            }
        }

        public void setStatusProcessing(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                Tickets ticket = db.Tickets.FirstOrDefault(p => p.id == id);
                ticket.idStatus = 2;
                db.Tickets.ApplyCurrentValues(ticket);
                db.SaveChanges();
            }
        }

        /* Called from the mobile app via API  */
        public void AddUserToTicket(string email, int idTicket)
        {
            TicketsMailingList tMailingList =
                new TicketsMailingList
                {
                    email = email,
                    idTicket = idTicket
                };

            Users user = repUsers.getUserByEmail(email);
            if (user != null)
                tMailingList.idUser = user.id;

            using (var db = new GTPTrackerEntities())
            {
                db.TicketsMailingList.AddObject(tMailingList);
                db.SaveChanges();
            }
        }

        /* Called from the web, in the ticket page  */
        public int AddUserToTicket(int idUser, int idTicket)
        {
            try
            {
                Users user = repUsers.Get(idUser);

                TicketsMailingList tMailingList =
                    new TicketsMailingList
                    {
                        email = user.email,
                        idTicket = idTicket,
                        idUser = idUser
                    };

                using (var db = new GTPTrackerEntities())
                {
                    // check did not exist before, for avoiding duplicates
                    TicketsMailingList exists = db.TicketsMailingList.Where(p => p.idTicket == idTicket && p.idUser == idUser).FirstOrDefault();
                    if (exists == null)
                    {
                        db.TicketsMailingList.AddObject(tMailingList);
                        db.SaveChanges();
                    }
                }
                ItemsManager itemsMgr = new ItemsManager();
                itemsMgr.updateTicketItem(idTicket);
                return 1;
            }
            catch { return 0; }
        }

        public int RemoveUserFromTicket(int idUser, int idTicket)
        {
            try
            {
                using (var db = new GTPTrackerEntities())
                {
                    TicketsMailingList mailingList = db.TicketsMailingList.FirstOrDefault(p => p.idTicket == idTicket && p.idUser == idUser);
                    if (mailingList != null)
                    {
                        db.TicketsMailingList.DeleteObject(mailingList);
                        db.SaveChanges();
                    }
                }
                ItemsManager itemsMgr = new ItemsManager();
                itemsMgr.updateTicketItem(idTicket);
                return 1;
            }
            catch { return 0; }
        }

        public int RemoveUserFromTicket(string email, int idTicket)
        {
            try
            {
                using (var db = new GTPTrackerEntities())
                {
                    TicketsMailingList mailingList = db.TicketsMailingList.FirstOrDefault(p => p.idTicket == idTicket && p.email == email);
                    if (mailingList != null)
                    {
                        db.TicketsMailingList.DeleteObject(mailingList);
                        db.SaveChanges();
                    }
                }
                ItemsManager itemsMgr = new ItemsManager();
                itemsMgr.updateTicketItem(idTicket);
                return 1;
            }
            catch { return 0; }
        }

        public IEnumerable<vTicketDistributionList> getDistributionList(int idTicket)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vTicketDistributionList.Where(p => p.idTicket == idTicket).ToList();
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
                repUsers.Dispose();
            }
            // free native resources if there are any.
        }
    }
}