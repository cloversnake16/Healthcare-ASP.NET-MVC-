using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Crunching.Models;
using GTPTracker.repos;
using System.Diagnostics;
using GTPTracker.Functions;

namespace GTPTracker.Functions
{
    public class VRSecurityMgr : IDisposable
    {
        private bool tenantsBelongsToMe(int idUser, int idElement)
        {
            using (repoCustomers repCustomers = new repoCustomers())
            using (repoVisitReports repVisitReports = new repoVisitReports())
            {
                IEnumerable<vCustomers> lAssignedTenants = repCustomers.getvByKAM(idUser);
                vVisitReports visitReport = repVisitReports.getV(idElement);
                if (visitReport == null) return false;
                IEnumerable<int> lIdsAssignedTenants = lAssignedTenants.Select(p => p.id);
                if (lIdsAssignedTenants.Contains(visitReport.idTenant)) return true;
                else return false;
            }
        }

        private bool thisTenantBelongsToMe(int idUser, int idTenant)
        {
            using (repoCustomers repCustomers = new repoCustomers())
            using (repoVisitReports repVisitReports = new repoVisitReports())
            {
                IEnumerable<vCustomers> lAssignedTenants = repCustomers.getvByKAM(idUser);
                vCustomers exists = lAssignedTenants.FirstOrDefault(p => p.id == idTenant);
                if (exists == null) return false;
                else return true;
            }
        }

        private bool canISeeAsCustomerThisVisitReport(Users user, int idElement)
        {
            using (repoVisitReports repVisitReports = new repoVisitReports())
            {
                vVisitReports visitReport = repVisitReports.getVByTicket(idElement);
                if (visitReport == null) return false;
                if (visitReport.isInternal == false && visitReport.idTenant == user.idTenant) return true;
                return false;
            }
        }

        private bool canISeeAsCustomerThisVisitReportTask(Users user, int idElement)
        {
            using (repoVisitReports repVisitReports = new repoVisitReports())
            using(repoSubtasks repTasks = new repoSubtasks())
            {
                SubTasks task = repTasks.get(idElement);
                vVisitReports visitReport = repVisitReports.getV(idElement);
                if (visitReport == null) return false;
                if (visitReport.isInternal == false && visitReport.idTenant == user.idTenant) return true;
                return false;
            }
        }

        /// <summary>
        /// Called in controllers when an user tries to access to an action. Permissions defined here : https://docs.google.com/spreadsheets/d/1JXYlR-QzK-ow3a7XeGCJ2jtEavfXUEhCnr2RYjIyHIg/edit#gid=1098398490
        /// </summary>
        /// <param name="action">View | Create | Edit | Delete | Change Status</param>
        /// <param name="element">PredefinedTasks | Visit Report | Visit Report Task</param>
        /// <param name="idUser">idUser, got from the Session Object</param>
        /// <param name="idElement">idTask or idReport, will be used to check if the tenant belongs to a KAM, etc.</param>
        /// <returns>true if the user has permission to execute the action over the element (for example, true if the GTP Manager can View a Visit Report)</returns>
        public bool canI(string action, string element, int idUser, int idElement = 0)
        {
            using (repoUsers repUsers = new repoUsers())
            {
                Users user = repUsers.Get(idUser);
                if (user != null)
                {   // predefined tasks. Only users with admin permissions has access
                    if (element == "PredefinedTasks" && action == "View")
                        return user.administrator == true;
                    if (element == "PredefinedTasks" && action == "Create")
                        return user.administrator == true;
                    if (element == "PredefinedTasks" && action == "Edit")
                        return user.administrator == true;
                    if (element == "PredefinedTasks" && action == "Delete")
                        return user.administrator == true;

                    if (element == "Visit Report" && action == "View")
                        return (user.viewVisitReports == true);
                    if (element == "Visit Report" && action == "Create")
                        return ((user.businessRole == "GTP KAM" && thisTenantBelongsToMe(user.id, idElement)) // here idElement == idTenant
                            || user.businessRole == "GTP General Manager") && user.createEditVisitReports == true;
                    if (element == "Visit Report" && action == "Edit")
                        return ((user.businessRole == "GTP KAM" && tenantsBelongsToMe(user.id, idElement))
                            || user.businessRole == "GTP General Manager") && user.createEditVisitReports == true;    
                
                    if (element == "Visit Report Task" && action == "View")
                        return ((user.businessRole == "GTP KAM" && tenantsBelongsToMe(user.id, idElement))
                            || user.businessRole == "GTP General Manager"
                            || (user.businessRole == "Customer" && canISeeAsCustomerThisVisitReportTask(user, idElement)))
                            && user.createEditVisitReports == true;
                    if (element == "Visit Report Task" && action == "Create")
                        return ((user.businessRole == "GTP KAM" && tenantsBelongsToMe(user.id, idElement)) 
                            || user.businessRole == "GTP General Manager") && user.createEditVisitReports == true;
                    if (element == "Visit Report Task" && action == "Edit")
                        return ((user.businessRole == "GTP KAM" && tenantsBelongsToMe(user.id, idElement))
                            || user.businessRole == "GTP General Manager") && user.createEditVisitReports == true;   
                    if (element == "Visit Report Task" && action == "Change Status")
                        return ((user.businessRole == "GTP KAM" && tenantsBelongsToMe(user.id, idElement))
                            || user.businessRole == "GTP General Manager" || (user.businessRole == "Customer") || (user.businessRole == "GTP TEAM")) 
                            && user.createEditVisitReports == true;                      
                }
            }
            return false;
        }

        public string sanitizeXSSAttacks(string description)
        {
            if (description != null && description.IndexOf("<script") == -1) return description;
            else return "attack detected";
        }

        /****************************************************************************************/
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            // free native resources if there are any.
        }
    }
}