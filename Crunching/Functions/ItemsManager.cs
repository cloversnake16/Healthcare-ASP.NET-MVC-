using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GTPTracker.repos;
using Crunching.Models;

namespace GTPTracker.Functions
{
    public class ItemsManager
    {
        repoItems repItems = new repoItems();
        repoSubtasks repTasks = new repoSubtasks();
        repoTickets repTickets = new repoTickets();
        repoUsers repUsers = new repoUsers();
        repoNews repNews = new repoNews();
       
        public void updateTaksItem(int idTask)
        {
            Items item = repItems.getByLink(2, idTask);
            Update(item.id);
        }

        public void updateTicketItem(int idTicket)
        {
            Items item = repItems.getByLink(1, idTicket);
            Update(item.id);
        }


        public void updateNewsItem(int idElement)
        {
            Items item = repItems.getByLink(3, idElement);
            Update(item.id);
        }

        public void updateMilestoneItem(int idElement)
        {
            Items item = repItems.getByLink(4, idElement);
            if (item != null)
                Update(item.id);
        }

        private void Update(int idItem)
        {
            Items item = repItems.Get(idItem);
            item.cDate = DateTime.UtcNow;
            repItems.Edit(item);
        }

        public IEnumerable<vWatchListItems> getMyItems(int idUser)
        {
            return repItems.getMyItems(idUser);

           /* Users user = repUsers.Get(idUser);
            IEnumerable<int> myTickets = repTickets.getMyTickets(idUser, false, user.idTenant).Select(p=>p.id);
            IEnumerable<int> myNews = repNews.getMyNews(idUser).Select(p=>p.idNews);
            IEnumerable<int> myTasks = repTasks.geTasksICanSee(idUser).Select(p=>p.id);
            using (var db = new GTPTrackerEntities())
            {
                IEnumerable<Items> lItemsFromTickets = db.Items.Where(p => myTickets.Contains((int)p.link) && p.idType == 1);
                IEnumerable<Items> lItemsFromTasks = db.Items.Where(p => myTasks.Contains((int)p.link) && p.idType == 2);
                IEnumerable<Items> lItemsFromNews = db.Items.Where(p => myNews.Contains((int)p.link) && p.idType == 3);
                return lItemsFromNews.Concat(lItemsFromTasks.Concat(lItemsFromTickets));
            }*/
        }
    }
}