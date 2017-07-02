using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Crunching.Models;
using GTPTracker.repos;
using GTPTracker.Helpers;
using GTPTracker.VM;

namespace GTPTrackers.Controllers
{
    public class ContactsController : Controller
    {
        repoContacts repContacts = new repoContacts();

        public ActionResult Index()
        {
            return View(repContacts.getListByTenant((int)Session["IDTENANT"]));
        }

        public ActionResult Add(int idContact, int idTenant, string headline)
        {
            ContactsCustomers contact = new ContactsCustomers();
            contact.idContact = idContact;
            contact.idTenant = idTenant;
            contact.Headline = headline;
            repContacts.Create(contact);
            return RedirectToAction("ManageContacts", "Customers", new { idTenant = idTenant });
        }

        public ActionResult Remove(int idContact, int idTenant)
        {
            ContactsCustomers contact = new ContactsCustomers();
            contact.idContact = idContact;
            contact.idTenant = idTenant;
            repContacts.Delete(contact);
            return RedirectToAction("ManageContacts", "Customers", new { idTenant = idTenant });
        }
    }
}
