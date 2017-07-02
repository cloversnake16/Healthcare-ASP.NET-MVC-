using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using System.Data;

namespace GTPTracker.repos
{
    public class repoProducts : IDisposable
    {
        /* used in the filtering of products */
        public IEnumerable<string> getModulus()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Products.Select(p => p.modulus).Distinct();
            }
        }

        public IEnumerable<string> getVolumes()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Products.Select(p => p.volume).Distinct();
            }
        }

        public IEnumerable<string> getHeights()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Products.Select(p => p.Height).Distinct();
            }
        }

        public IEnumerable<string> getOuterDiameters()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Products.Select(p => p.Outer_diameter).Distinct();
            }
        }

        public IEnumerable<string> getContactAreas()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Products.Select(p => p.Contact_area).Distinct();
            }
        }

        /***********************************************/

        public IEnumerable<Products> getAll()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Products.ToList();
            }
        }

        public IEnumerable<vProducts> getvAll()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vProducts.ToList();
            }
        }

        public IEnumerable<Products> getCustomBySerie(int idSerie)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Products.Where(p => p.idSerie == idSerie && p.status == "custom").ToList();
            }
        }

        /*************** to be used by the buttons that work in batch ***/

        public void addProductToTenant(string refNumber, int idTenant, int idCreator)
        {
            using (var db = new GTPTrackerEntities())
            {
                Products product = db.Products.Where(p => p.refNumber == refNumber).FirstOrDefault();
                if (product != null)
                {
                    CustomerProducts customerProduct = db.CustomerProducts.FirstOrDefault(p => p.idCustomer == idTenant && p.idProduct == product.id);
                    if (customerProduct == null) // no existe previamente
                    {
                        CustomerProducts relation =
                            new CustomerProducts
                            {
                                idCustomer = idTenant,
                                idCreator = idCreator,
                                idProduct = product.id,
                                cDate = DateTime.Today
                            };
                        db.CustomerProducts.AddObject(relation);
                        db.SaveChanges();
                    }
                }
            }
        }

        public bool categoryExists(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Categories.Any(p => p.id == id);
            }
        }

        public Products ProductByRefNumber(string refNumber)
        {
            using (var db = new GTPTrackerEntities())
            {
                Products product = db.Products.FirstOrDefault(p => p.refNumber == refNumber);
                return product;
            }
        }

        internal bool SaveBulk(List<Products> bulkProducts)
        {
            using (var db = new GTPTrackerEntities())
            {
                foreach (var item in bulkProducts)
                {
                    if (item.id != 0)
                    {
                        db.Products.Attach(item);
                        db.ObjectStateManager.ChangeObjectState(item, EntityState.Modified);
                    }
                    else
                    {
                        db.Products.AddObject(item);
                    }
                }

                var updates = db.SaveChanges();
                return updates > 0;
            }
        }

        public void removeFromTenant(string refNumber, int idTenant)
        {
            using (var db = new GTPTrackerEntities())
            {
                Products product = db.Products.Where(p => p.refNumber == refNumber).FirstOrDefault();
                if (product != null)
                {
                    CustomerProducts customerProduct = db.CustomerProducts.FirstOrDefault(p => p.idCustomer == idTenant && p.idProduct == product.id);
                    if (customerProduct != null) // no existe previamente
                    {
                        db.CustomerProducts.DeleteObject(customerProduct);
                        db.SaveChanges();
                    }
                }
            }
        }

        /****************/

        public void addProductToTenant(int idProduct, int idTenant, int idCreator)
        {
            using (var db = new GTPTrackerEntities())
            {
                CustomerProducts customerProduct = db.CustomerProducts.FirstOrDefault(p => p.idCustomer == idTenant && p.idProduct == idProduct);
                if (customerProduct == null) // no existe previamente
                {
                    CustomerProducts relation =
                        new CustomerProducts
                        {
                            idCustomer = idTenant,
                            idCreator = idCreator,
                            idProduct = idProduct,
                            cDate = DateTime.Today
                        };
                    db.CustomerProducts.AddObject(relation);
                    db.SaveChanges();
                }
            }
        }

        public void removeFromTenant(int idProduct, int idTenant)
        {
            using (var db = new GTPTrackerEntities())
            {
                CustomerProducts customerProduct = db.CustomerProducts.FirstOrDefault(p => p.idCustomer == idTenant && p.idProduct == idProduct);
                if (customerProduct != null) // no existe previamente
                {
                    db.CustomerProducts.DeleteObject(customerProduct);
                    db.SaveChanges();
                }
            }
        }

        public DateTime getDateAddToCustomer(int idProduct, int idTenant)
        {
            if (idTenant == 1) return DateTime.Today; // GTP
            else
            {
                using (var db = new GTPTrackerEntities())
                {
                    CustomerProducts cp = db.CustomerProducts.Where(p => p.idCustomer == idTenant && p.idProduct == idProduct).FirstOrDefault();
                    if (cp == null) return DateTime.Today;
                    else return cp.cDate;
                }
            }
        }

        public IEnumerable<Products> getList(int idTenant)
        {
            using (var db = new GTPTrackerEntities())
            {
                if (idTenant == 1 || idTenant == 0)
                    return db.Products.ToList();

                IEnumerable<int> lMyProducts = db.CustomerProducts.Where(p => p.idCustomer == idTenant).Select(p => p.idProduct).ToList();
                //return db.Products.Where(p => lMyProducts.Contains(p.id) /*&& p.showInCatalog == true*/).ToList();
                IEnumerable<Products> result = db.Products.Where(p => lMyProducts.Contains(p.id) );
                return result.ToList();
            }
        }

        public IEnumerable<vProducts> getvList(int idTenant)
        {
            using (var db = new GTPTrackerEntities())
            {
                if (idTenant == 1)
                    return db.vProducts.ToList();
                IEnumerable<int> lMyProducts = db.CustomerProducts.Where(p => p.idCustomer == idTenant).Select(p => p.idProduct).ToList();
                IEnumerable<vProducts> result = db.vProducts.Where(p => lMyProducts.Contains(p.id));
                return result.ToList();
            }
        }

        public IEnumerable<int> getCustomersByProductsList(int[] productsList)
        {
            if (productsList != null)
            {
                using (var db = new GTPTrackerEntities())
                {
                    IEnumerable<int> lCustomers = db.CustomerProducts.Where(p => productsList.Contains(p.idProduct)).Select(p => p.idCustomer).Distinct().ToList();
                    return lCustomers;
                }
            }
            else return null;
        }

        public void removeFromCustomerProductsByProduct(int idProduct)
        {
            using (var db = new GTPTrackerEntities())
            {
                IEnumerable<CustomerProducts> lCustomerProduct = db.CustomerProducts.Where(p => p.idProduct == idProduct);
                foreach (CustomerProducts item in lCustomerProduct)
                    db.CustomerProducts.DeleteObject(item);
                db.SaveChanges();
            }
        }

        public IEnumerable<Products> getListBySerie(int idSerie)
        {
            using (var db = new GTPTrackerEntities())
            {
                Series serie = db.Series.FirstOrDefault(p => p.id == idSerie);
                return serie != null
                    ? db.Products.Where(p => p.category == serie.category && p.serie == serie.serie && p.showInCatalog == true).ToList()
                    : null;
            }
        }

        public Products Get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Products.FirstOrDefault(p => p.id == id);
            }
        }

        public vProducts Getv(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vProducts.FirstOrDefault(p => p.id == id);
            }
        }

        public Products GetByType(string type)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Products.FirstOrDefault(p => p.type == type);
            }
        }

        public Products GetByRefNumber(string refNumber)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Products.FirstOrDefault(p => p.refNumber == refNumber);
            }
        }

        public void Create(Products item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Products.AddObject(item);
                db.SaveChanges();
            }
        }

        public void Edit(Products item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Products.FirstOrDefault(p => p.id == item.id);
                db.Products.ApplyCurrentValues(item);
                db.SaveChanges();
            }
        }

        public void DeleteByTenant(int idTenant) // used in customers.edit. We have to delete all the products in the productsCustomers table and later add only the ones selected by the admin
        {
            using (var db = new GTPTrackerEntities())
            {
                var list = db.CustomerProducts.Where(p => p.idCustomer == idTenant);
                foreach (var item in list)
                {
                    db.CustomerProducts.DeleteObject(item);
                }
                db.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                Products des = db.Products.FirstOrDefault(e => e.id == id);
                if (des != null)
                {
                    db.Products.DeleteObject(des);
                    db.SaveChanges();
                }
            }
        }

        public IEnumerable<ProductsFiles> getFiles(String refNumber)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.ProductsFiles.Where(p => p.refNumber == refNumber).ToList();
            }
        }

        public IEnumerable<vProductsFiles> getFilesBySerie(int? serie, int? category)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vProductsFiles.Where(p=>p.serie == serie && p.category == category && p.internalGTP==false).ToList();
            }
        }

        public IEnumerable<vProductsFiles> getGTPFilesBySerie(int? serie, int? category)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vProductsFiles.Where(p => p.serie == serie && p.category == category).ToList();
            }
        }

        public IEnumerable<vProductsFiles> getGTPFilesByIdSerie(int idSerie)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vProductsFiles.Where(p => p.idSerie == idSerie).ToList();
            }
        }

        public IEnumerable<vProductsFiles> getFiles()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vProductsFiles.ToList();
            }
        }

        public IEnumerable<ProductsFiles> getAllFiles()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.ProductsFiles.ToList();
            }
        }
        public IEnumerable<ProductsFiles> getFilesOfThisProducts(IEnumerable<string> lProducts)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.ProductsFiles.Where(p => lProducts.Contains(p.refNumber)).ToList();
            }
        }
        public IEnumerable<ProductsFiles> getFilesOfThisProductsById(IEnumerable<int> lProducts)
        {
            using (var db = new GTPTrackerEntities())
            {
                IEnumerable<int?> lIntsProducts = lProducts.Cast<int?>();
                return db.ProductsFiles.Where(p => lIntsProducts.Contains(p.idProduct)).ToList();
            }
        }

        public void addFile(ProductsFiles pFile)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.ProductsFiles.AddObject(pFile);
                db.SaveChanges();
            }
        }

        public ProductsFiles GetFile(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.ProductsFiles.FirstOrDefault(p=>p.id == id);
            }
        }

        public void DeleteFile(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                ProductsFiles des = db.ProductsFiles.FirstOrDefault(e => e.id == id);
                if (des != null)
                {
                    db.ProductsFiles.DeleteObject(des);
                    db.SaveChanges();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
            }
            // free native resources if there are any.
        }
    }
}