using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GTPTracker.Functions;
using GTPTracker.repos;
using Crunching.Models;
using Crunching.Controllers;
using System.Web.Mvc;
using GTPTracker.VM;

namespace TestProject1
{
    [TestClass]
    public class testPredefinedTasks
    {
        VRSecurityMgr vrSecurityMgr = new VRSecurityMgr();

        [TestMethod]
        public void onlyAdminCanView()
        {
            bool JoergHaveAccess = vrSecurityMgr.canI("View", "PredefinedTasks", 1010);
            Assert.IsTrue(JoergHaveAccess);
            bool DariuszHaveAccess = vrSecurityMgr.canI("View", "PredefinedTasks", 1071);
            Assert.IsFalse(DariuszHaveAccess);
        }

        [TestMethod]
        public void correctDataReturnsRedirectToAction()
        {
            /*var controller = new PredefinedTasksController();
            var context = MockRepository.GenerateStub<ControllerContext>();
            context.Expect(x => x.HttpContext.Session["MyKey"]).Return("MyValue");
            controller.ControllerContext = context;
            controller.ModelState.Clear();
            PredefinedTasksVM vm = new PredefinedTasksVM();
            vm.predefinedTask.idTicketType = 4;
            vm.predefinedTask.idResponsible = 1;
            vm.predefinedTask.idCategory = 1;
            var result = controller.Create(vm);

            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));*/

        }

        
    }
}
