using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GTPTracker.repos;
using Crunching.Models;

namespace GTPTracker.VM
{
    public class TaskDetailsVM
    {
        repoSubtasks repSubTasks = new repoSubtasks();
        repoTaskFiles repoFiles = new repoTaskFiles();
        repoTasksComments repComments = new repoTasksComments();
        repoTickets repTickets = new repoTickets();

        public vSubTasks subTask;
        public IEnumerable<TasksFiles> lFiles;
        public IEnumerable<vTasksComments> lComments;
        public IEnumerable<vTicketDistributionList> lDistributionList;

        public TaskDetailsVM(int idTask)
        {
            subTask = repSubTasks.getV(idTask);
            lFiles = repoFiles.getList(idTask);
            lComments = repComments.getvList(idTask);
            lDistributionList = repTickets.getDistributionList(subTask.idTicket);
        }
    }
}