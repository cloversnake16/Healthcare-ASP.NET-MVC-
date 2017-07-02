using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.Functions;

namespace GTPTracker.repos
{
    public class repoSubtasks : IDisposable
    {
        public IEnumerable<vSubTasks> getAll()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vSubTasks.ToList();
            }
        }

        public IEnumerable<vSubTasks> geTasksICanSee(int idUser)
        {
            using (var db = new GTPTrackerEntities())
            {
                IEnumerable<int> lDistribution = db.TicketsMailingList.Where(p => p.idUser == idUser).Select(p => p.idTicket).ToList(); // tickets where I'm in the distribution list
                IEnumerable<vSubTasks> lTasksFromTickets = db.vSubTasks.Where(p => lDistribution.Contains(p.idTicket)).ToList();
                IEnumerable<vSubTasks> lTasksFromCreator = db.vSubTasks.Where(p => p.idCreator == idUser).ToList(); // I'm the creator
                IEnumerable<vSubTasks> lTasksFromAsignee = db.vSubTasks.Where(p => p.idUser == idUser).ToList(); // I'm the asignee
                return lTasksFromAsignee.Concat(lTasksFromCreator).Concat(lTasksFromTickets);
            }
        }

        public IEnumerable<vSubTasks> getOpenTasksAssignedToMe(int idUser)
        {
            using (var db = new GTPTrackerEntities())
            {
                IEnumerable<vSubTasks> lTasksFromAsignee = db.vSubTasks.Where(p => p.idUser == idUser && p.solved == false).ToList();
                return lTasksFromAsignee;
            }            
        }

        public IEnumerable<vSubTasks> getMyActiveList(int idUser)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vSubTasks.Where(p => p.idStatus != 3 && p.idUser == idUser).ToList();
            }
        }

        public IEnumerable<vSubTasks> getAllTasksFromCustomer(int idCustomer)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vSubTasks.Where(p => p.idStatus != 3 && p.idTenant == idCustomer).ToList();
            }
        }

        public IEnumerable<vSubTasks> getList(int idTicket)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vSubTasks.Where(p => p.idTicket == idTicket).ToList();
            }
        }

        public SubTasks get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.SubTasks.FirstOrDefault(p => p.id == id);
            }
        }

        public vSubTasks getV(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vSubTasks.FirstOrDefault(p => p.id == id);
            }
        }

        public void Create(SubTasks item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.SubTasks.AddObject(item);
                db.SaveChanges();
                repoItems repItem = new repoItems();
                Items i = new Items();
                i.cDate = DateTime.UtcNow;
                i.idType = 2; // ticket
                i.title = "Task " + item.id;
                i.link = item.id;
                i.isInternal = (bool) item.internalTask;
                repItem.Create(i);
               /* ItemsManager itemsMgr = new ItemsManager();
                itemsMgr.updateTicketItem(item.idTicket);*/
            }
        }

        public void Edit(SubTasks item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.SubTasks.FirstOrDefault(p => p.id == item.id);
                db.SubTasks.ApplyCurrentValues(item);
                db.SaveChanges();
                /*ItemsManager itemsMgr = new ItemsManager();
                itemsMgr.updateTaksItem(item.id);
                itemsMgr.updateTicketItem(item.idTicket);*/
            }
        }

        public void Delete(SubTasks item)
        {
            using (var db = new GTPTrackerEntities())
            {
                SubTasks des = db.SubTasks.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.SubTasks.DeleteObject(des);
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