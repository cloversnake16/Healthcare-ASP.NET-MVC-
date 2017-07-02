using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.VM
{
    public class DocumentsEditAddVM
    {
        public vGenDocs doc;
        public IEnumerable<GenDocCategories> lCategories;
        public IEnumerable<Series> lSeries;
        public IEnumerable<vDocsCustomers> lAssignedCustomers;
        public IEnumerable<vCustomers> lCustomers;
        public IEnumerable<vDocsProducts> lAssignedProducts;
        public IEnumerable<Products> lProducts;

        private IEnumerable<int> lIntAssignedProducts;
        private IEnumerable<int> lIntAssignedCustomers;

        public DocumentsEditAddVM(int? id)
        {
            using (repoGeneralDocs repDocs = new repoGeneralDocs())
            using (repoSeries repSeries = new repoSeries())
            using (repoCustomers repCustomers = new repoCustomers())
            using (repoProducts repProducts = new repoProducts())
            {
                if (id != null) // edit
                {
                    doc = repDocs.Getv((int)id);
                    lAssignedCustomers = repDocs.GetCustomersByDoc((int)id);
                    lAssignedProducts = repDocs.GetProductsByDoc((int)id);
                    lIntAssignedCustomers = lAssignedCustomers.Select(p => p.idCustomer).Distinct();
                    lIntAssignedProducts = lAssignedProducts.Select(p => p.idProduct).Distinct();
                }
                else
                {
                    doc = new vGenDocs();
                    lAssignedCustomers = Enumerable.Empty<vDocsCustomers>();
                    lAssignedProducts = Enumerable.Empty<vDocsProducts>();
                    lIntAssignedCustomers = Enumerable.Empty<int>();
                    lIntAssignedProducts = Enumerable.Empty<int>();
                }
                lCategories = repDocs.getCategories();
                lCustomers = repCustomers.getList().Where(p=> !lIntAssignedCustomers.Contains(p.id)).OrderBy(p=>p.name);
                lProducts = repProducts.getAll().Where(p=> !lIntAssignedProducts.Contains(p.id)).OrderBy(p=>p.type);
                lSeries = repSeries.getList().OrderBy(p=>p.text);
            }
        }
    }
}