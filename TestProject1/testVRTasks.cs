using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class testVRTasks
    {
        [TestMethod]
        public void TaskNotVisibleForCustomerIfTicketIsInternal()
        {
        }

        [TestMethod]
        public void KAMsCanOnlySeeTasksOfTicketsAssignedToTheirCustomersOrDirectly()
        {
        }

        [TestMethod]
        public void mandatoryFieldsCantBeEmpty()
        {
            // idTicket is not null
            // creation date is not null
            // idStatus is not null           
            // description is not null
        }
    }
}
