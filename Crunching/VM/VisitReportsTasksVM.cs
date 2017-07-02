using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GTPTracker.repos;
using Crunching.Models;
using System.Web.Mvc;


namespace GTPTracker.VM
{
    public class VisitReportsTasksVM
    {
        public vSubTasks task {get; set;}
        public VisitReports visitReport { get; set; }
        public Tickets ticket { get; set; }
        [AllowHtml]
        public string description { get; set; }
        [AllowHtml]
        public string content { get; set; }
        public IEnumerable<vPredefinedTasks> lPredefinedTasks;
        public IEnumerable<vCustomers> lCustomers;
        public IEnumerable<vUsers> lUsers;
        public string customerName;
        
        public VisitReportsTasksVM()
        {
            task = new vSubTasks();
            ticket = new Tickets();
            visitReport = new VisitReports();
            using (repoPredefinedTasks repPredefinedTasks = new repoPredefinedTasks())
            using (repoCustomers repCustomers = new repoCustomers())
            using (repoUsers repUsers = new repoUsers())
            {
                lPredefinedTasks = repPredefinedTasks.getAll();
                lCustomers = repCustomers.getList();
                lUsers = repUsers.getvGTPUsers();
            }
        }

        public VisitReportsTasksVM(int id, int idVisitReport)
        {
            using (repoPredefinedTasks repPredefinedTasks = new repoPredefinedTasks())
            using (repoCustomers repCustomers = new repoCustomers())
            using (repoSubtasks repTasks = new repoSubtasks()) 
            using (repoUsers repUsers = new repoUsers())
            using (repoTickets repTickets = new repoTickets())
            using (repoVisitReports repVisitReports = new repoVisitReports())
            {
                lPredefinedTasks = repPredefinedTasks.getAll();
                lCustomers = repCustomers.getList();
                task = new vSubTasks();
                vTickets vTicket = repTickets.getv(id);
                if (vTicket != null) customerName=vTicket.customerName;
                ticket = repTickets.get(id);
                visitReport = repVisitReports.get(idVisitReport);
                lUsers = repUsers.getvGTPUsers();
            }
        }
    }
}