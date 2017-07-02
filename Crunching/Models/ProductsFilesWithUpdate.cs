using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Crunching.Models
{
    public class ProductsFilesWithUpdate
    {
        public bool updated = false;
        public vProductsFiles productFile;

        public ProductsFilesWithUpdate(bool updated, vProductsFiles productFile)
        {
            this.updated = updated;
            this.productFile = productFile;
        }
    }
}