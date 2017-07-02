using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.Functions;

namespace GTPTracker.repos
{
    public class repoTasksComments: IDisposable
    {
        repoSubtasks repTasks = new repoSubtasks();

        public IEnumerable<TasksComments> getList(int idTask)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.TasksComments.Where(p => p.idTask == idTask).ToList();
            }
        }

        public IEnumerable<vTasksComments> getvList(int idTask)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vTasksComments.Where(p => p.idTask == idTask).ToList();
            }
        }

        public TasksComments Get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.TasksComments.FirstOrDefault(p => p.id == id);
            }
        }

        public int Create(TasksComments item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.TasksComments.AddObject(item);
                db.SaveChanges();
            }
            ItemsManager itemsMgr = new ItemsManager();
            itemsMgr.updateTaksItem(item.idTask);
            SubTasks task = repTasks.get(item.idTask);
            itemsMgr.updateTicketItem(task.idTicket);
            return item.id;
        }

        public void Edit(TasksComments item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.TasksComments.FirstOrDefault(p => p.id == item.id);
                db.TasksComments.ApplyCurrentValues(item);
                db.SaveChanges();
                ItemsManager itemsMgr = new ItemsManager();
                itemsMgr.updateTaksItem(item.idTask);
                SubTasks task = repTasks.get(item.idTask);
                itemsMgr.updateTicketItem(task.idTicket);
            }
        }

        public void Delete(TasksComments item)
        {
            using (var db = new GTPTrackerEntities())
            {
                TasksComments des = db.TasksComments.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.TasksComments.DeleteObject(des);
                    db.SaveChanges();
                    ItemsManager itemsMgr = new ItemsManager();
                    itemsMgr.updateTaksItem(item.idTask);
                    SubTasks task = repTasks.get(item.idTask);
                    itemsMgr.updateTicketItem(task.idTicket);
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
                repTasks.Dispose();
            }
            // free native resources if there are any.
        }
    }
}