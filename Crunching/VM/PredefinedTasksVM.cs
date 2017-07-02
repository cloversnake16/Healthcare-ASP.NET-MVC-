using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.VM
{
    public class PredefinedTasksVM
    {
        public PredefinedTasks predefinedTask { get; set; }
        public IEnumerable<PredefinedTasksCategories> lCategories;
        public IEnumerable<vUsers> lUsers;

        public PredefinedTasksVM()
        {
            predefinedTask = new PredefinedTasks();
            using (repoPredefinedTasks repPredefinedTasks = new repoPredefinedTasks())
                lCategories = repPredefinedTasks.getCategories();
            using (repoUsers repUsers = new repoUsers())
                lUsers = repUsers.getvGTPUsers();            
        }

        public PredefinedTasksVM(int id)
        {
            using (repoPredefinedTasks repPredefinedTasks = new repoPredefinedTasks())
            using (repoUsers repUsers = new repoUsers())
            {
                predefinedTask = repPredefinedTasks.get(id);
                lCategories = repPredefinedTasks.getCategories();
                lUsers = repUsers.getvGTPUsers();
            }
        }
    }
}