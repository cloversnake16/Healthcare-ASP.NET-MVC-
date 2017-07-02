using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.VM
{
    public class ContactsVM : IDisposable
    {
        public IEnumerable<Users> GTPUsersList;
        public IEnumerable<ContactsCustomers> lContactsCustomers;
        public int idTenant;

        repoUsers repUsers = new repoUsers();
        repoCustomers repCustomers = new repoCustomers();

        public ContactsVM(int idCustomer)
        {
            idTenant = idCustomer;
            GTPUsersList = repUsers.getGTPPeople();
            lContactsCustomers = repCustomers.getContacts(idCustomer);
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
                repUsers.Dispose();
                repCustomers.Dispose();
            }
            // free native resources if there are any.
        }
    }
}