using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;

namespace GTPTracker.repos
{
    public class repoContacts
    {
        public IEnumerable<vContactCustomers> getListByTenant(int idTenant)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vContactCustomers.Where(p => p.idTenant == idTenant).ToList(); // take out id=1 that is GTP
            }
        }

        public ContactsCustomers Get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.ContactsCustomers.FirstOrDefault(p => p.id == id);
            }
        }

        public IEnumerable<ContactsCustomers> getList()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.ContactsCustomers.Where(p => p.id != 1).ToList(); // take out id=1 that is GTP
            }
        }


        public void Create(ContactsCustomers item)
        {
            using (var db = new GTPTrackerEntities())
            {
                ContactsCustomers contact = db.ContactsCustomers.Where(p => p.idTenant == item.idTenant && p.idContact == item.idContact).FirstOrDefault();
                if (contact == null)
                {
                    db.ContactsCustomers.AddObject(item);
                    db.SaveChanges();
                }
            }
        }

        public void Edit(ContactsCustomers item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.ContactsCustomers.FirstOrDefault(p => p.id == item.id);
                db.ContactsCustomers.ApplyCurrentValues(item);
                db.SaveChanges();
            }
        }

        public void Delete(ContactsCustomers item)
        {
            using (var db = new GTPTrackerEntities())
            {
                ContactsCustomers des = db.ContactsCustomers.FirstOrDefault(e => e.idTenant == item.idTenant && e.idContact == item.idContact);
                if (des != null)
                {
                    db.ContactsCustomers.DeleteObject(des);
                    db.SaveChanges();
                }
            }
        }
    }
}