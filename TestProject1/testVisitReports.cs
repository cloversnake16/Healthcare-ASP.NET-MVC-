using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class testVisitReports
    {
        [TestMethod]
        public void VisitReportsNotVisibleForCustomerIfTicketIsInternal()
        {
        }

        [TestMethod]
        public void KAMsCanOnlySeeTicketsAssignedToTheirCustomersOrTaskAssignedDirectly()
        {
        }

        [TestMethod]
        public void mandatoryFieldsCantBeEmpty()
        {
            // idCreator is not null
            // visit date is not null
            // idStatus is not null
            // internal is not null
            // description is not null
        }

        [TestMethod]
        public void creatorIsKAMOrManager()
        {
        }

    }
}
