using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.VM
{
    public class ManageDocumentsVM
    {
        public IEnumerable<vGenDocs> lDocs;
        public IEnumerable<GenDocCategories> lCategories;
        public string category;
        public string bulkAction;
        public string status;
        public int tab;
        public string action;

        public ManageDocumentsVM(string category, string bulkAction, string status, string whatToDo, int tab = 1)
        {
            using (repoGeneralDocs repDocs = new repoGeneralDocs())
            {
                lDocs = repDocs.getvList().OrderByDescending(p=>p.id); // first last coming
                lCategories = repDocs.getCategories().OrderBy(p=>p.category);
                if (status != "All status" && status != null) lDocs = lDocs.Where(p => p.status == status);
                if (category != "All categories" && category != null) lDocs = lDocs.Where(p => p.CategoryName == category);
                this.category = category;
                this.bulkAction = bulkAction;
                this.status = status;
                this.tab = tab;
                this.action = whatToDo;
            }
        }
    }
}