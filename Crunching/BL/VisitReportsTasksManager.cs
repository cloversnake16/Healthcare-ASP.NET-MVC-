using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GTPTracker.repos;
using Crunching.Models;
using GTPTracker.VM;
using GTPTracker.Functions;

namespace GTPTracker.BL
{
    public class VisitReportsTasksManager
    {
        public SubTasks setAssignTo(SubTasks task)
        {
            if (task.assignTo == "Customer"|| task.assignTo == "customer")
            {
                task.idUser = 0;
            }
            else
            {
                using (repoUsers repUsers = new repoUsers())
                {
                    IEnumerable<vUsers> lGTPUsers = repUsers.getvGTPUsers();
                    vUsers selected = lGTPUsers.FirstOrDefault(p => p.fullName == task.assignTo);
                    if (selected != null)
                    {
                        task.idUser = selected.id;
                    }
                    else
                    {
                        task.idUser = 0;
                    }
                }
            }
            return task;
        }

        public IEnumerable<vVisitReportsTasks> getTasksAssignedToMe(int idUser, bool showDoneTasks)
        {
            IEnumerable<vVisitReportsTasks> result = Enumerable.Empty<vVisitReportsTasks>();
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            using (repoUsers repUsers = new repoUsers())
            using (repoSubtasks repTasks = new repoSubtasks())
            using (repoCustomers repCustomers = new repoCustomers())
            {
                vUsers user = db.vUsers.FirstOrDefault(p => p.id == idUser);
                if (user != null)
                {
                    string role = user.businessRole;
                    if (role == "GTP General Manager" || role == "GTP KAM" || role == "GTP Team") 
                        result = db.vVisitReportsTasks.Where(p=>p.idUser == idUser).ToList();
                    if (role == "Customer")
                        result = db.vVisitReportsTasks.Where(p => p.idTenant == user.idTenant && p.isInternal == false && (p.assignTo == "Customer" || p.assignTo == user.tenantName)).ToList();
                }
                if (showDoneTasks == true)
                    return result;
                else return result.Where(p => p.solved == false);
            }
        }

        /// <summary>
        /// Will return a ienumerable with the list of tasks that correspond to this user according with the rules explained here : https://firstangel.atlassian.net/wiki/display/GT/Visit+Report
        /// </summary>
        /// <param name="idUser">int with the user id. Comes from the session</param>
        /// <param name="showDoneTasks">bool to filter and returning also the completed tasks</param>
        /// <returns>A list with the tasks that correspond to the user. Empty set if something happens</returns>
        public IEnumerable<vVisitReportsTasks> getMyTasks(int idUser, bool showDoneTasks)
        {
            IEnumerable<vVisitReportsTasks> result = Enumerable.Empty<vVisitReportsTasks>(); 
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            using (repoUsers repUsers = new repoUsers())
            using (repoSubtasks repTasks = new repoSubtasks())
            using (repoCustomers repCustomers = new repoCustomers())
            {
                Users user = repUsers.Get(idUser);
                if (user != null)
                {
                    string role = user.businessRole;
                    if (role == "GTP General Manager") 
                        result = db.vVisitReportsTasks.ToList();
                    if (role == "GTP KAM")
                    {
                        IEnumerable<int> lIdTenants = repCustomers.getvByKAM(idUser).Select(p => p.id);
                        result = db.vVisitReportsTasks.Where(p => lIdTenants.Contains(p.idTenant)).Union(db.vVisitReportsTasks.Where(p => p.idUser == idUser)).Distinct().ToList();
                    }
                    if (role == "GTP Team") 
                        result = db.vVisitReportsTasks.Where(p=>p.idUser == idUser).ToList();
                    if (role == "Customer")
                        result = db.vVisitReportsTasks.Where(p => p.idTenant == user.idTenant && p.isInternal == false).ToList();
                }
                if (showDoneTasks == true)
                    return result;
                else return result.Where(p => p.solved == false);
            }
        }
    }
}