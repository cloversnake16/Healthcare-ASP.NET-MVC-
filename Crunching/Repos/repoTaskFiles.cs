using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.Functions;

namespace GTPTracker.repos
{
    public class repoTaskFiles : IDisposable
    {
        repoSubtasks repTasks = new repoSubtasks();

        public IEnumerable<TasksFiles> getList(int idTask)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.TasksFiles.Where(p => p.idTask == idTask).ToList();
            }
        }

        public TasksFiles Get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.TasksFiles.FirstOrDefault(p => p.id == id);
            }
        }

        public int Create(TasksFiles item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.TasksFiles.AddObject(item);
                db.SaveChanges();
            }
            ItemsManager itemsMgr = new ItemsManager();
            itemsMgr.updateTaksItem(item.idTask);
            SubTasks task = repTasks.get(item.idTask);
            itemsMgr.updateTicketItem(task.idTicket);
            return item.id;
        }

        public void Edit(TasksFiles item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.TasksFiles.FirstOrDefault(p => p.id == item.id);
                db.TasksFiles.ApplyCurrentValues(item);
                db.SaveChanges();
                ItemsManager itemsMgr = new ItemsManager();
                itemsMgr.updateTaksItem(item.idTask);
                SubTasks task = repTasks.get(item.idTask);
                itemsMgr.updateTicketItem(task.idTicket);
            }
        }

        public void Delete(TasksFiles item)
        {
            using (var db = new GTPTrackerEntities())
            {
                TasksFiles des = db.TasksFiles.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.TasksFiles.DeleteObject(des);
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