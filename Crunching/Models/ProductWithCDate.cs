using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Crunching.Models
{
    public class ProductWithCDate
    {
        public Products product;
        public DateTime cDate;
        public bool isMyProduct;

        public ProductWithCDate(Products product, DateTime cDate, bool isMyProduct = false)
        {
            this.product = product;
            this.cDate = cDate;
            this.isMyProduct = isMyProduct;
        }
    }
}