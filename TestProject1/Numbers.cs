using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class Numbers
    {
        [TestMethod]
        public void Rounding()
        {
            double val1 = Math.Round(2.5);
            Assert.AreEqual(val1, 2);
            double val2 = Math.Round(2.5, 0, MidpointRounding.AwayFromZero);
            Assert.AreEqual(val2, 3);
        }
    }
}
