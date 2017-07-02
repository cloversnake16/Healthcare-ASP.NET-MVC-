using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.VM
{
    public class addProductToTenantVM
    {
        repoProducts repProducts = new repoProducts();
        repoCustomers repCustomers = new repoCustomers();

        public IEnumerable<Products> lAllProducts;
        public IEnumerable<Products> lExistingProducts;
        public int idTenant;
        public Customers customer;

        public addProductToTenantVM(int idCustomer)
        {
            idTenant = idCustomer;
            customer = repCustomers.Get(idCustomer);
            lAllProducts = repProducts.getAll();
            lExistingProducts = repProducts.getList(idTenant);
        }

    }
}