using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GTPTracker.repos;
using Crunching.Models;
using GTPTracker.BL;

namespace GTPTracker.VM
{
    public class VisitReportsIndexVM
    {
        public IEnumerable<vCustomers> lCustomers;
        public IEnumerable<Countries> lCountries;
        public IEnumerable<vVisitReports> lVisitReports;
        public IEnumerable<vUsers> lUsers;
        public bool showDoneReports = false;
        public int? selectedCustomer;
        public int? selectedUser;
        public int? selectedCountry;
        public Users currentUser;

        public VisitReportsIndexVM() { }

        public VisitReportsIndexVM(int? idCountry, int? idCustomer, int? idResponsible, string showDoneReportsParam, int idUser) 
        {
            if (showDoneReportsParam == "true") showDoneReports = true; else showDoneReports = false;
            using (repoVisitReports repVisitReports = new repoVisitReports())
            using (repoCustomers repCustomers = new repoCustomers())
            using (repoUsers repUsers = new repoUsers())
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            {
                this.currentUser = repUsers.Get(idUser);
                VisitReportsManager visitReportsMgr = new VisitReportsManager();
                lVisitReports =visitReportsMgr.Get(idUser);
                if (idCustomer != null) lVisitReports = lVisitReports.Where(p => p.idTenant == idCustomer);
                if (idResponsible != null) lVisitReports = lVisitReports.Where(p => p.idCreator == idResponsible);
                if (showDoneReports == false) lVisitReports = lVisitReports.Where(p => p.idStatus == 0);
                if (idCountry != null) lVisitReports= lVisitReports.Where(p => p.idCountry == idCountry);
                lCountries = db.Countries.ToList();
                lCustomers = repCustomers.getList().OrderBy(p=>p.name);
                if (currentUser.businessRole == "GTP KAM")                    
                    lCustomers = repCustomers.getvByKAM(currentUser.id);
                lUsers = repUsers.getvGTPUsers().OrderBy(p=>p.fullName);
            }
            this.selectedCustomer = idCustomer;
            this.selectedUser = idResponsible;
        }
    }
}