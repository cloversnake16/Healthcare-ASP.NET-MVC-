using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.VM
{
    public class ProductsVM
    {
        repoProducts repProducts = new repoProducts();
        repoSeries repSeries = new repoSeries();

        public IEnumerable<vProducts> lProducts;
        public IEnumerable<Series> lSeries;
        public IEnumerable<vProducts> lMyProducts;
        
        public ProductsVM(IEnumerable<vProducts> lProducts, IEnumerable<vProducts> lMyProducts)
        {
            this.lProducts = lProducts;
            lSeries = repSeries.getList();
            this.lMyProducts = lMyProducts;
        }

       
    }
}