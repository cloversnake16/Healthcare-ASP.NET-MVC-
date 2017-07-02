using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.VM
{
    public class ProductVM
    {
        repoProducts repProducts = new repoProducts();

        public vProducts product;
        public IEnumerable<ProductsFiles> lFiles;
        
        public ProductVM(int id)
        {
            product = repProducts.Getv(id);
            lFiles = repProducts.getFiles(product.refNumber);
        }

       
    }
}