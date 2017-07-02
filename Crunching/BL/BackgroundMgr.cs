using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GTPTracker.repos;
using GTPTracker.Helpers;
using GTPTracker.Functions;
using Crunching.Models;

namespace GTPTracker.BL
{
    public class BackgroundMgr
    {
        public void dailyTasks()
        {
            using (UsersManager um = new UsersManager())
            using (repoSubtasks repTasks = new repoSubtasks())
            using (repoMilestones repMilestones = new repoMilestones())
            using (repoUsers repUsers = new repoUsers())
            {
                ItemsManager itemMgr = new ItemsManager();

                um.getNotAssignedUsers();
                GTPTracker.Helpers.LogMgr logMgr = new LogMgr();
                logMgr.BackgroundLog("emailDailyActivationReport");

               
            }
        }
    }
}