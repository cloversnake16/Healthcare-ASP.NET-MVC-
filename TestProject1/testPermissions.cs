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
    public class testPermissions
    {
        [TestMethod]
        public void TestPermissions()
        {
            using (UsersManager um = new UsersManager())
            {
                // TODO check email exists and is not blocked
                string email = "luis@imasmas64.com";
                // create a new user with fake data
                Users user = um.Create(email, "Luis", "Pando", true, "Sr", "Fake Inc.", "CEO", "987654321", "patata16", true); // call the create fn using the test=true parameter
           
                // KAM. Can not have seeDrafts
                user.businessRole = "GTP KAM";
                user.seeDraftProducts = true;
                Assert.AreEqual(um.checkPermisions(user), false, "KAM can not see draft");
                // customer. can not see Drafts, general admin, assign products, see internal files
                user.businessRole = "customer";
                user.seeDraftProducts = true;
                Assert.AreEqual(um.checkPermisions(user), false, "customer can not see drafts");
                user.seeDraftProducts = false;
                user.generalAdmin = true;
                Assert.AreEqual(um.checkPermisions(user), false, "customer can not have general Admin");
                user.generalAdmin = false;
                user.assignCustomersToProducts = true;
                Assert.AreEqual(um.checkPermisions(user), false, "customer can not assign products");
                user.assignCustomersToProducts = false;
                user.downloadInternalFiles = true;
                Assert.AreEqual(um.checkPermisions(user), false, "customer can not download internal files");
                user.downloadInternalFiles = false;

                // GTP Team. can not see Drafts, general admin, assign products, see internal files, custom products
                user.businessRole = "GTP Team";
                user.seeDraftProducts = true;
                Assert.AreEqual(um.checkPermisions(user), false, "GTP Team can not see drafts");
                user.seeDraftProducts = false;
                user.generalAdmin = true;
                Assert.AreEqual(um.checkPermisions(user), false, "GTP Team can not have general Admin");
                user.generalAdmin = false;
                user.assignCustomersToProducts = true;
                Assert.AreEqual(um.checkPermisions(user), false, "GTP Team can not assign products");
                user.assignCustomersToProducts = false;
                user.downloadInternalFiles = true;
                Assert.AreEqual(um.checkPermisions(user), false, "GTP Team can not download internal files");
                user.downloadInternalFiles = false;
                user.seeCustomProducts = true;
                Assert.AreEqual(um.checkPermisions(user), false, "GTP Team can not see custom products");
                user.seeCustomProducts = false;

                // visitor
                user.businessRole = "visitor";
                user.seeDraftProducts = true;
                Assert.AreEqual(um.checkPermisions(user), false, "visitor can not see drafts");
                user.seeDraftProducts = false;
                user.generalAdmin = true;
                Assert.AreEqual(um.checkPermisions(user), false, "visitor can not have general Admin");
                user.generalAdmin = false;
                user.assignCustomersToProducts = true;
                Assert.AreEqual(um.checkPermisions(user), false, "visitor can not assign products");
                user.assignCustomersToProducts = false;
                user.downloadInternalFiles = true;
                Assert.AreEqual(um.checkPermisions(user), false, "visitor can not download internal files");
                user.downloadInternalFiles = false;
                user.seeCustomProducts = true;
                Assert.AreEqual(um.checkPermisions(user), false, "visitor can not see custom products");
                user.seeCustomProducts = false;
                user.download2d = true;
                Assert.AreEqual(um.checkPermisions(user), false, "visitor can not download 2d products");
                user.download2d = false;
                user.download3d = true;
                Assert.AreEqual(um.checkPermisions(user), false, "visitor can not download 3d products");
                user.download3d = false;
                user.accessTickets = true;
                Assert.AreEqual(um.checkPermisions(user), false, "visitor can not access tickets");
                user.accessTickets = false;
                user.downloadCustomDocuments = true;
                Assert.AreEqual(um.checkPermisions(user), false, "visitor can not download Custom ocuments");
                user.downloadCustomDocuments = false;
                // remove the fake user 
                um.Delete(email);
                um.Delete("luis2@unifydev.com");
            }
        }
    }
}
