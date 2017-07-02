using Crunching.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Data;

namespace GTPTracker.repos
{
    public class repoCustomers : IDisposable
    {
        public void AcceptUserAgreement(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                Customers item = db.Customers.FirstOrDefault(p => p.id == id);
                item.userAgreementSigned = true;
                db.Customers.ApplyCurrentValues(item);
                db.SaveChanges();
            }
        }

        public IEnumerable<Countries> getCountries()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Countries.ToList();
            }
        }

        public IEnumerable<string> getCurrentCountries()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vCustomers.Select(p=>p.CountryName).Distinct().ToList();
            }
        }

        internal bool SaveBulk(List<Customers> bulk)
        {
            using (var db = new GTPTrackerEntities())
            {
                foreach (var item in bulk)
                {
                    if (item.id != 0)
                    {
                        db.Customers.Attach(item);
                        db.ObjectStateManager.ChangeObjectState(item, EntityState.Modified);
                    }
                    else
                    {
                        db.Customers.AddObject(item);
                    }
                }
                var updates = db.SaveChanges();
                return updates > 0;
            }
        }

        internal bool SaveCustomersProductsBulk(List<CustomerProducts> bulk)
        {
            using (var db = new GTPTrackerEntities())
            {
                foreach (var item in bulk)
                {
                    if (item.id != 0)
                    {
                        db.CustomerProducts.Attach(item);
                        db.ObjectStateManager.ChangeObjectState(item, EntityState.Modified);
                    }
                    else
                    {
                        db.CustomerProducts.AddObject(item);
                    }
                }
                var updates = db.SaveChanges();
                return updates > 0;
            }
        }

        /************************ KAMS *****************/
        public IEnumerable<string> getCurrentKAMs()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vKAMs.OrderBy(p=>p.KAMName).Select(p => p.KAMName).Distinct().ToList();
            }
        }

        public IEnumerable<vKAMs> getKAMsByTenant(int idTenant)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vKAMs.Where(p => p.idCustomer == idTenant).ToList();
            }
        }

        public void DeleteKAMFromTenant(int idUser, int idTenant)
        {
            using (var db = new GTPTrackerEntities())
            {
                CustomersKAMs des = db.CustomersKAMs.FirstOrDefault(e => e.idKAM == idUser && e.idCustomer == idTenant);
                if (des != null)
                {
                    db.CustomersKAMs.DeleteObject(des);
                    db.SaveChanges();
                }
            }
        }

        public void DeleteAllCustomersKAMsByUser(int idUser)
        {
            using (var db = new GTPTrackerEntities())
            {
                IEnumerable<CustomersKAMs> lCustomesrsKams = db.CustomersKAMs.Where(p => p.idKAM == idUser).ToList();
                foreach (CustomersKAMs des in lCustomesrsKams)
                {
                    if (des != null)
                    {
                        db.CustomersKAMs.DeleteObject(des);
                        db.SaveChanges();
                    }
                }
            }
        }

        public void AddKAMToTenant(int idCustomer, int idKAM)
        {
            using (var db = new GTPTrackerEntities())
            {
                CustomersKAMs item = new CustomersKAMs();
                item.idKAM = idKAM;
                item.idCustomer = idCustomer;
                db.CustomersKAMs.AddObject(item);
                db.SaveChanges();
            }
        }

        public IEnumerable<vCustomers> getvByKAM(int idUser)
        {
            using (var db = new GTPTrackerEntities())
            {
                IEnumerable<CustomersKAMs> lKams = db.CustomersKAMs.Where(p => p.idKAM == idUser);
                IEnumerable<vCustomers> lCustomers = db.vCustomers.Where(p => lKams.Select(q => q.idCustomer).Contains(p.id)).ToList();
                return lCustomers;
              //  return db.vCustomers.Where(p => p.idKeyAccountManager == idUser).ToList();
            }
        }

        /************* END KAMS *************************/

        public IEnumerable<vCustomers> getvByProduct(int idProduct)
        {
            using (var db = new GTPTrackerEntities())
            {
                IEnumerable<CustomerProducts> lCustomerProducts = db.CustomerProducts.Where(p => p.idProduct == idProduct);
                IEnumerable<vCustomers> lCustomers = db.vCustomers.Where(p => lCustomerProducts.Select(q => q.idCustomer).Contains(p.id)).ToList();
                return lCustomers;
            }
        }

        public IEnumerable<ContactsCustomers> getContacts(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.ContactsCustomers.Where(p => p.idTenant == id).ToList(); 
            }
        }

        public IEnumerable<vCustomers> getList()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vCustomers.Where(p => p.id != 1).ToList(); // take out id=1 that is GTP
            }
        }

        

        public Customers Get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Customers.FirstOrDefault(p => p.id == id);
            }
        }

        public Customers GetByName(string name)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Customers.FirstOrDefault(p => p.name == name);
            }
        }

        public int getIdTenantByLinkID(string linkID)
        {
            using (var db = new GTPTrackerEntities())
            {
                Customers customer = db.Customers.FirstOrDefault(p => p.linkID == linkID);
                return customer != null ? customer.id : 0;
            }
        }

        public Customers Create(Customers item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Customers.AddObject(item);
                db.SaveChanges();
                return item;
            }
        }

        public void Edit(Customers item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Customers.FirstOrDefault(p => p.id == item.id);
                db.Customers.ApplyCurrentValues(item);
                db.SaveChanges();
            }
        }

        public void Delete(Customers item)
        {
            using (var db = new GTPTrackerEntities())
            {
                Customers des = db.Customers.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.Customers.DeleteObject(des);
                    db.SaveChanges();
                }
            }
        }

        public void SetAsInactive(Customers item)
        {
            using (var db = new GTPTrackerEntities())
            {
                item.active = false;
                db.Customers.FirstOrDefault(p => p.id == item.id);
                db.Customers.ApplyCurrentValues(item);
                db.SaveChanges();
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