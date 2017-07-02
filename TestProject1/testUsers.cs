using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GTPTracker.BL;
using Crunching.Models;

namespace TestProject1
{
    [TestClass]
    public class testUsers
    {
        [TestMethod]
        public void NewCreatedUserOutput()
        {
            using (UsersManager um = new UsersManager())
            {
             /*   um.Delete("luis1@unifydev.com");
                um.Delete("info@efficontrol.com");
                um.Delete("luis@efficontrol.com");*/

             //   um.Delete("andreas@firstangel.co");
             //   um.Delete("andreas.diehl@gmail.com");
                // TODO check email exists and is not blocked
                string email = "luis@imasmas64.com";
                // create a new user with fake data
                Users user = um.Create(email, "Luis", "Pando", true, "Sr", "Fake Inc.", "CEO", "987654321", "patata16", true); // call the create fn using the test=true parameter
                // assert status = "non-active", acceptTerms = true, name != null, country != null and email != null
                Assert.IsNotNull(user.firstName);
                Assert.IsNotNull(user.lastName);
                Assert.IsNotNull(user.email);
                Assert.AreEqual(user.userAgreementSigned, true);
                Assert.AreEqual(user.status, "non-active");
                Assert.IsNull(user.businessRole);

                Assert.IsNotNull(user.hash);                            // assert hash != null. That implies an activation link was generated
                // run the daily activation report. Check the user is there
                //   Assert.AreEqual(um.getNotAssignedUsers(), 56);

                user.businessRole = "GTP KAM";                          // assign him kam status.
                Assert.AreEqual(um.checkBusinessRole(user), false);     // check fails as not having tenants assigned
                
                um.assignTenantToKAM(user, 1042);                       // assign user (with KAM role) to tenant 1035 (Prueba)
                Assert.AreEqual(um.checkBusinessRole(user), true);      // check works when tenant assigned
                um.unassignTenantToKAM(user, 1042);                           // unassign user kam from tenant 1035

                user.businessRole = "Customer";                         // assign business role customer
                Assert.AreEqual(um.checkBusinessRole(user), false);     // check fails as not having idTenant
                user.idTenant = 1035;                                   // assign an idTenant
                Assert.AreEqual(um.checkBusinessRole(user), true);      // check not fails
                user.idTenant = 0;
                // remove the fake user 
                um.Delete(email);
               // um.Delete("luis2@unifydev.com");
               
            }
        }
    }
}
