using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GTPTracker.repos;
using Crunching.Models;

namespace GTPTracker.VM
{
    public class SubTaskVM
    {
        repoSubtasks repSubTasks = new repoSubtasks();
        repoUsers repUsers = new repoUsers();

        public SubTasks subTask;
        public IEnumerable<Users> lGTPUsers;

        public SubTaskVM(int idTicket)
        {
            subTask = new SubTasks();
            subTask.idTicket = idTicket;
            subTask.implementedMeasures = false;
            subTask.internalTask = true;
            lGTPUsers = repUsers.getGTPPeople();
        }
    }
}