using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GTPTracker.repos;
using Crunching.Models;
using GTPTracker.BL;

namespace GTPTracker.VM
{
    public class MyTasksVM
    {
        public IEnumerable<vVisitReportsTasks> lTasks;
        public IEnumerable<VisitReportsFiles> lFiles;
        public bool showDoneTasks;

        public MyTasksVM() { }

        public MyTasksVM(int idUser, bool showDoneTasks)
        {
            this.showDoneTasks = showDoneTasks;
            VisitReportsTasksManager tasksMgr = new VisitReportsTasksManager();
            lTasks = tasksMgr.getTasksAssignedToMe(idUser, showDoneTasks);
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            {
                lFiles = db.VisitReportsFiles.ToList();
            }
        }
    }
}