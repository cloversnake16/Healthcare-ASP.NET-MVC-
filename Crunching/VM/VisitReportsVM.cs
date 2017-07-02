using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GTPTracker.repos;
using Crunching.Models;
using GTPTracker.BL;
using System.Web.Mvc;

namespace GTPTracker.VM
{
    public class VisitReportsVM
    {
        [AllowHtml]
        public string content { get; set; }
        public Tickets ticket { get; set; }
        public VisitReports visitReport { get; set; }
        public vVisitReports vVisitReport;
        public IEnumerable<vCustomers> lCustomers;
        public IEnumerable<vVisitReportsTasks> lTasks;
        public IEnumerable<VisitReportsFiles> lFiles;
        public Users currentUser;
        public IEnumerable<vUsers> lUsers;
        public string message;

        public VisitReportsVM()
        {
            ticket = new Tickets();
            visitReport = new VisitReports();
            using (repoCustomers repCustomers = new repoCustomers())
                lCustomers = repCustomers.getList();            
        }

        public VisitReportsVM(int idUser)
        {
            ticket = new Tickets();
            visitReport = new VisitReports();
            using (repoUsers repUser = new repoUsers())
            using (repoCustomers repCustomers = new repoCustomers())
            {
                currentUser = repUser.Get(idUser);
                if (currentUser.businessRole == "GTP KAM")
                    lCustomers = repCustomers.getvByKAM(currentUser.id);
                else lCustomers = repCustomers.getList();
                lCustomers = lCustomers.OrderBy(p => p.name);
            }
        }

        public VisitReportsVM(int id, int idUser, string message)
        {
            using (repoVisitReports repVisitReports = new repoVisitReports())
            using (repoVisitReportsFiles repFiles = new repoVisitReportsFiles())
            using (repoTickets repTickets = new repoTickets())
            using (repoCustomers repCustomers = new repoCustomers())
            using (repoSubtasks repTasks = new repoSubtasks())
            using (repoUsers repUser = new repoUsers())
            {
                visitReport = repVisitReports.get(id);
                vVisitReport = repVisitReports.getV(id);
                ticket = repTickets.get(visitReport.idTicket);
                VisitReportsTasksManager tasksMgr = new VisitReportsTasksManager();
                lTasks = tasksMgr.getMyTasks(idUser, true).Where(p => p.idTicket == ticket.id);                
               // lTasks = repTasks.getAll().Where(p => p.idTicket == ticket.id); // TODO. Get only my tasks
                lFiles = repFiles.getAllByidVisitReport(id);
                currentUser = repUser.Get(idUser);
                if (currentUser.businessRole == "GTP KAM")
                    lCustomers = repCustomers.getvByKAM(currentUser.id);
                else lCustomers = repCustomers.getList();
                lCustomers = lCustomers.OrderBy(p => p.name);
                lUsers = repUser.getvGTPUsers();
                this.message = message;
                content = ticket.description;
            }
        }

    }
}