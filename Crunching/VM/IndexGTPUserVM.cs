using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.VM
{
    public class IndexGTPUserVM
    {
        repoTickets repTickets = new repoTickets();
        repoSubtasks repSubTasks = new repoSubtasks();
        repoUsers repUsers = new repoUsers();
        repoCustomers repCustomers = new repoCustomers();

        public IEnumerable<vTickets> lTickets;
        public List<vTickets> lTicketsAux = new List<vTickets>();
        public IEnumerable<vSubTasks> lSubTasks;
        public int idUser;
        public List<Users> lUsersTasks = new List<Users>();
        public IEnumerable<vCustomers> lCustomers;

        public IndexGTPUserVM(int userID, int idCustomer, int? sort)
        {
            idUser = userID;
            if (sort == 2)
                lTickets = repTickets.getvList(idUser).Where(p => p.idStatus == 3).OrderByDescending(p=>p.id);
            if (sort == 1)
                lTickets = repTickets.getvList(idUser).Where(p => p.idStatus != 3).OrderByDescending(p=>p.id);
            if (sort != 1 && sort != 2)
                lTickets = repTickets.getvList(idUser).OrderByDescending(p => p.id);

            if (HttpContext.Current.User.IsInRole("KAM"))
                lTickets = lTickets.Where(p=>p.idKeyAccountManager == userID);
            
           /* lSubTasks = repSubTasks.getAllTasksFromCustomer(idCustomer);
            foreach (var item in lSubTasks)
            {
                Users user = repUsers.Get(item.idUser);
                if (lUsersTasks.Find(p => p.id == user.id) == null)
                    lUsersTasks.Add(user);
            }*/
            
        }
    }
}