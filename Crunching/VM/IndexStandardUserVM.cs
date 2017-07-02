using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.VM
{
    public class IndexStandardUserVM
    {
        repoTickets repTickets = new repoTickets();
    
        public IEnumerable<vTickets> lTickets;
    
        public IndexStandardUserVM(int idTenant, int idUser, bool isGTP, int? sort)
        {
            if (sort == 2)
                lTickets = repTickets.getMyTickets(idUser, isGTP, idTenant).Where(p=>p.idStatus == 3);
            if (sort == 1)
                lTickets = repTickets.getMyTickets(idUser, isGTP, idTenant).Where(p => p.idStatus !=3 );
            if (sort != 1 && sort != 2)
                lTickets = repTickets.getMyTickets(idUser, isGTP, idTenant);
        }
    }
}