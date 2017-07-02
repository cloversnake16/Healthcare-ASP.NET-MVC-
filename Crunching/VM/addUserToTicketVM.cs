using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.VM
{
    public class addUserToTicketVM
    {
        public IEnumerable<Users> lUsers;
        public Tickets ticket;
        public IEnumerable<vTicketDistributionList> lDistributionList;

        repoTickets repTickets = new repoTickets();
        repoUsers repUsers = new repoUsers();

        public addUserToTicketVM(int idTicket, int idTenant)
        {
            ticket = repTickets.get(idTicket);
            lUsers = repUsers.getListByTenant(idTenant);
            lDistributionList = repTickets.getDistributionList(idTicket);
        }
    }
}