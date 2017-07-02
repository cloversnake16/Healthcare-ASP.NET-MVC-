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

namespace TestProject1
{
    [TestClass]
    public class testProducts
    {
        [TestMethod]
        public void AllProducts()
        {
            SeriesIndexVM vm = new SeriesIndexVM(null, 1, null, null, null, null, null, null, null, null, null, null, null, null);
            Assert.IsNotNull(vm);
        }

        [TestMethod]
        public void FilterModulus()
        {
            SeriesIndexVM vm = new SeriesIndexVM(null, 1, "0,75", "1", null, null, null, null, null, null, null, null, null, null);
            Assert.IsNotNull(vm);
        }

        [TestMethod]
        public void FilterSpecific()
        {
            SeriesIndexVM vm = new SeriesIndexVM(null, 1, null, null, null, null, null, null, null, null, null, null, "TG 50", null);
            Assert.IsNotNull(vm);
            Debug.WriteLine(vm.filterSpecific);
        }

        [TestMethod]
        public void FilterAll()
        {
            SeriesIndexVM vm = new SeriesIndexVM(null, 1, "0,75", "1,3", "8", "100", "43", "100", "44", "78", "15", "72", null, "THERMO");
            Assert.IsNotNull(vm);
            Debug.WriteLine(vm.totalCount);
            Assert.AreEqual(vm.totalCount, 16);
        } 
    }
}
