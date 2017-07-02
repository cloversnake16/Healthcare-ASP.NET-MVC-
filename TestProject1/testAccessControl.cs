using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GTPTracker.VM;
using Crunching.Models;
using System.Configuration;
using System.Collections.Specialized;
using System.IO;
using System.Diagnostics;
using GTPTracker.BL;
using System.Web.Security;
using GTPTracker.repos;

namespace TestProject1
{
    [TestClass]
    public class testAccessControl
    {
        /// <summary>
        /// Confirms that filtering by tennant ID is working
        /// </summary>
        [TestMethod]
        public void FilterByTenantId()
        {
       /*     repoUsers repUsers = new repoUsers();
            Users user = repUsers.Get(1109);
            SeriesIndexVM vm1 = new SeriesIndexVM(null, 1109, null, null, null, null, null, null, null, null, null, null, null, null, user);
            SeriesIndexVM vm2 = new SeriesIndexVM(null, 1135, null, null, null, null, null, null, null, null, null, null, null, null);
            Debug.WriteLine("Products = " + vm1.lProducts.Count());
            Debug.WriteLine("Products2 = " + vm2.lProducts.Count());
            Assert.IsNotNull(vm1.lMyProducts);
            var Ids = vm1.lMyProducts.Select(m => m.id).ToList();
            Assert.IsFalse(vm2.lMyProducts.Any(m=> Ids.Contains(m.id)), "Items in first tenant list exist in second tenant list");*/
        }

        /*
        /// <summary>
        /// Confirms that filtering by tennant ID is working
        /// </summary>
        [TestMethod]
        public void UserAccessToProducts()
        {
            repoUsers repUsers = new repoUsers();
            using (UsersManager um = new UsersManager())
            {
                string email = "amrosama2020@gmail.com";
                string password = "patata16";
                Users user = um.Create(email, "Luis", true, "Sr", "Fake Inc.", "CEO", "987654321", password, true);
                Assert.IsNotNull(user.name);
                user.idTenant = 1036;
                try
                {
                    if (Membership.ValidateUser(email, password))
                    {
                        Users loggedInUser = repUsers.getUserByEmail(email);
                        loggedInUser.idTenant = 1036;
                    }
                }
                finally
                {
                    um.Delete(email); // even if test failed user should be deleted
                }

            }
        }
        */

    }
}
