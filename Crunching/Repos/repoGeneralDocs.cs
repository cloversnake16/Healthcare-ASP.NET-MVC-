using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;

namespace GTPTracker.repos
{
    public class repoGeneralDocs: IDisposable
    {
        //categories 
        public IEnumerable<GenDocCategories> getCategories()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.GenDocCategories.ToList();
            }
        }

        public GenDocCategories GetCategory(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.GenDocCategories.FirstOrDefault(p=>p.id == id);
            }
        }

        public int CreateCategory(GenDocCategories item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.GenDocCategories.AddObject(item);
                db.SaveChanges();
            }
            return item.id;
        }

        public void EditCategory(GenDocCategories item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.GenDocCategories.FirstOrDefault(p => p.id == item.id);
                db.GenDocCategories.ApplyCurrentValues(item);
                db.SaveChanges();
            }
        }

        public void DeleteCategory(GenDocCategories item)
        {
            using (var db = new GTPTrackerEntities())
            {
                GenDocCategories des = db.GenDocCategories.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.GenDocCategories.DeleteObject(des);
                    db.SaveChanges();
                }
            }
        }

        // Assign / unassign documents to customers

        public IEnumerable<vDocsCustomers> GetvDocByCustomer(int idCustomer)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vDocsCustomers.Where(p => p.idCustomer == idCustomer).ToList();
            }
        }

        public IEnumerable<GenDocsCustomers> GetDocByCustomer(int idCustomer)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.GenDocsCustomers.Where(p => p.idCustomer == idCustomer).ToList();
            }
        }

        public GenDocsCustomers GetCustomerByDoc(int idDoc)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.GenDocsCustomers.Where(p => p.idGenDoc == idDoc).FirstOrDefault();
            }
        }

        public IEnumerable<vDocsCustomers> GetCustomersByDoc(int idDoc)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vDocsCustomers.Where(p => p.idGenDoc == idDoc).ToList();
            }
        }

        public void AssignToCustomer(int idGenDoc, int idCustomer)
        {
            using (var db = new GTPTrackerEntities())
            {
                GenDocsCustomers item = new GenDocsCustomers();
                item.idCustomer = idCustomer;
                item.idGenDoc = idGenDoc;
                db.GenDocsCustomers.AddObject(item);
                db.SaveChanges();
            }
        }

        public void RemoveFromCustomer(int idDoc)
        {
            using (var db = new GTPTrackerEntities())
            {
                IEnumerable<GenDocsCustomers> list = db.GenDocsCustomers.Where(e => e.idGenDoc == idDoc);
                foreach (GenDocsCustomers des in list)
                if (des != null)
                    db.GenDocsCustomers.DeleteObject(des);
                db.SaveChanges();
            }
        }

        public void deleteByCustomer(int idCustomer)
        {
            using (var db = new GTPTrackerEntities())
            {
                IEnumerable<GenDocsCustomers> list = db.GenDocsCustomers.Where(e => e.idCustomer == idCustomer);
                foreach (GenDocsCustomers des in list)
                    if (des != null)
                        db.GenDocsCustomers.DeleteObject(des);
                db.SaveChanges();
            }
        }
        // Assign / unassign documents to products

        public IEnumerable<vDocsProducts> GetDocsByProducts(int idProduct)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vDocsProducts.Where(p => p.idProduct == idProduct).ToList();
            }
        }

        public IEnumerable<vDocsProducts> GetProductsByDoc(int idDoc)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vDocsProducts.Where(p => p.idDoc == idDoc).ToList();
            }
        }

        public void AssignDocLinkToProduct(int idGenDoc, int idProduct)
        {
            using (var db = new GTPTrackerEntities())
            {
                GenDocsProducts item = new GenDocsProducts();
                item.idProduct = idProduct;
                item.idDoc = idGenDoc;
                db.GenDocsProducts.AddObject(item);
                db.SaveChanges();
            }
        }

        public void RemoveProductsLinkByDoc(int idDoc)
        {
            using (var db = new GTPTrackerEntities())
            {
                IEnumerable<GenDocsProducts> list = db.GenDocsProducts.Where(e => e.idDoc == idDoc);
                foreach (var des in list)
                {
                    if (des != null)
                        db.GenDocsProducts.DeleteObject(des);
                }
                db.SaveChanges();
            }
        }

        public void DeleteByProduct(int idProduct)
        {
            using (var db = new GTPTrackerEntities())
            {
                IEnumerable<GenDocsProducts> list = db.GenDocsProducts.Where(e => e.idProduct == idProduct);
                foreach (var des in list)
                {
                    if (des != null)
                        db.GenDocsProducts.DeleteObject(des);
                }
                db.SaveChanges();
            }
        }

        // General documents
        public IEnumerable<GeneralProductsDocuments> GetByCategory(int category)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.GeneralProductsDocuments.Where(p=>p.category == category).ToList();
            }
        }

        public IEnumerable<GeneralProductsDocuments> getList()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.GeneralProductsDocuments.ToList();
            }
        }

        public IEnumerable<vGenDocs> getvList()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vGenDocs.ToList();
            }
        }

        public GeneralProductsDocuments Get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.GeneralProductsDocuments.FirstOrDefault(p => p.id == id);
            }
        }

        public GeneralProductsDocuments GetByFilename(string filename)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.GeneralProductsDocuments.FirstOrDefault(p => p.filename == filename);
            }
        }

        public vGenDocs Getv(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vGenDocs.FirstOrDefault(p => p.id == id);
            }
        }

        public int Create(GeneralProductsDocuments item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.GeneralProductsDocuments.AddObject(item);
                db.SaveChanges();
            }
            return item.id;
        }

        public int Edit(GeneralProductsDocuments item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.GeneralProductsDocuments.FirstOrDefault(p => p.id == item.id);
                db.GeneralProductsDocuments.ApplyCurrentValues(item);
                db.SaveChanges();
                return item.id;
            }
        }

        public void Delete(GeneralProductsDocuments item)
        {
            using (var db = new GTPTrackerEntities())
            {
                GeneralProductsDocuments des = db.GeneralProductsDocuments.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.GeneralProductsDocuments.DeleteObject(des);
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