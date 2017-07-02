using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.Functions;

namespace GTPTracker.repos
{
    public class repoCommentFiles
    {
        repoComments repComments = new repoComments();

        public IEnumerable<CommentsFiles> getList(int idComment)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.CommentsFiles.Where(p => p.idComment == idComment).ToList();
            }
        }

        public CommentsFiles Get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.CommentsFiles.FirstOrDefault(p => p.id == id);
            }
        }

        public int Create(CommentsFiles item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.CommentsFiles.AddObject(item);
                db.SaveChanges();
            }
            Comments comment = repComments.get(item.id);
            ItemsManager itemsMgr = new ItemsManager();
            itemsMgr.updateTicketItem(comment.idTicket);
            return item.id;
        }

        public void Edit(CommentsFiles item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.CommentsFiles.FirstOrDefault(p => p.id == item.id);
                db.CommentsFiles.ApplyCurrentValues(item);
                db.SaveChanges();
            }
            Comments comment = repComments.get(item.id);
            ItemsManager itemsMgr = new ItemsManager();
            itemsMgr.updateTicketItem(comment.idTicket);
        }

        public void Delete(CommentsFiles item)
        {
            using (var db = new GTPTrackerEntities())
            {
                CommentsFiles des = db.CommentsFiles.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.CommentsFiles.DeleteObject(des);
                    db.SaveChanges();
                }
            }
            Comments comment = repComments.get(item.id);
            ItemsManager itemsMgr = new ItemsManager();
            itemsMgr.updateTicketItem(comment.idTicket);
        }
    }
}