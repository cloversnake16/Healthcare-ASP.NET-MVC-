using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.VM
{
    public class TicketVM
    {
        repoTickets repTickets = new repoTickets();
        repoComments repcomments = new repoComments();
        repoPhotos repPhotos = new repoPhotos();
        repoProducts repProducts = new repoProducts();
        repoSubtasks repSubtasks = new repoSubtasks();
        repoTicketsFiles repFiles = new repoTicketsFiles();

        public vTickets ticket;
        public IEnumerable<vComments> lcomments;
        public IEnumerable<Photos> lPhotos;
        public IEnumerable<Products> lProducts;
        public IEnumerable<vTicketDistributionList> lDistributionList;
        public IEnumerable<vSubTasks> lSubTasks;
        public IEnumerable<TicketsFiles> lFiles;
        public DateTime lastUpdate;
        public IEnumerable<Users> lAvailableUsers;
        public IEnumerable<vCustomers> lCustomers;

        public AsigneeList lAvailablesAsignees;

        public string TicketStatus
        {
            get
            {
                if (ticket == null)
                    return "None";

                switch (ticket.idStatus)
                {
                    case 1:
                        return "New";
                    case 2:
                        return "In Progress";
                    case 3:
                        return "Closed";
                    default:
                        return "None";
                }
            }
        }

        public string TicketType
        {
            get
            {
                if (ticket == null)
                    return "Unknown";

                switch (ticket.idType)
                {
                    case 1:
                        return "Technical question";
                    case 2:
                        return "Claim";
                    case 3:
                        return "Request sample";
                    default:
                        return "Unknown";
                }
            }
        }

        public string TicketMeasuresTaken
        {
            get
            {
                if (ticket == null)
                    return "Unknown";

                switch (ticket.idMeasuresTaken)
                {
                    case 0:
                        return "Material blocked";
                    case 1:
                        return "Material sorted";
                    case 2:
                        return "Material used";
                    case 3:
                        return string.Format("Others : {0}", ticket.others);
                    default:
                        return string.Empty;
                }
            }
        }


        public TicketVM(int idTenant)
        {
            ticket = new vTickets();
            lProducts = repProducts.getList(idTenant);
        }

        public TicketVM(int idTicket, string idTenant)
        {
            ticket = repTickets.getVTicket(idTicket);
            lcomments = repcomments.getvList(idTicket).OrderByDescending(p => p.cDate).ToList();
            lPhotos = repPhotos.getList(idTicket);
           // lProducts = repProducts.getList(1);
            lDistributionList = repTickets.getDistributionList(idTicket);
            lSubTasks = repSubtasks.getList(idTicket);
            lFiles = repFiles.getList(idTicket);
            DateTime CommentsLastDate = lcomments.Select(p => p.cDate).OrderByDescending(p => p.Date).FirstOrDefault();
            DateTime PhotosLastDate = lPhotos.Select(p => p.cDate).OrderByDescending(p => p.Date).FirstOrDefault();

            using (repoUsers repUsers = new repoUsers())
            {
                int Tenant = 0;
                if (Int32.TryParse(idTenant, out Tenant))
                    lAvailableUsers = repUsers.getListByTenant(Tenant);
            }
        }

        public class AsigneeRecord
        {
            public string id; // format "idCustomer,idUser"
            public string name;            
        }
        public class AsigneeList{
            public List<AsigneeRecord> lAvailables = new List<AsigneeRecord>();
            public AsigneeList(IEnumerable<vCustomers> lCustomers, IEnumerable<Users> lUsers)
            {
                if (lCustomers != null)
                {
                    foreach (var item in lCustomers)
                    {
                        AsigneeRecord record = new AsigneeRecord();
                        record.id = item.id.ToString();
                        record.name = item.name;
                        lAvailables.Add(record);
                        foreach (var user in lUsers.Where(p => p.idTenant == item.id))
                        {
                            AsigneeRecord recordPerson = new AsigneeRecord();
                            recordPerson.id = item.id.ToString() + ", " + user.id.ToString();
                            recordPerson.name = user.firstName + " " + user.lastName;
                            lAvailables.Add(recordPerson);
                        }
                    }
                }
                else 
                    foreach (var user in lUsers)
                    {
                        AsigneeRecord recordPerson = new AsigneeRecord();
                        recordPerson.id = user.idTenant.ToString() + ", " + user.id.ToString();
                        recordPerson.name = user.firstName + " " + user.lastName;
                        lAvailables.Add(recordPerson);
                    }
            }
        }

        public TicketVM(int idTicket, int idTenant)
        {
            ticket = repTickets.getVTicket(idTicket);
            if (ticket.idTenant == 0) ticket.idTenant = 1;
            lcomments = repcomments.getvList(idTicket).OrderByDescending(p => p.cDate).ToList();
            lPhotos = repPhotos.getList(idTicket);
            lProducts = repProducts.getList(idTenant);
            lDistributionList = repTickets.getDistributionList(idTicket);
            
            lFiles = repFiles.getList(idTicket);
            DateTime CommentsLastDate = lcomments.Select(p => p.cDate).OrderByDescending(p => p.Date).FirstOrDefault();
            DateTime PhotosLastDate = lPhotos.Select(p => p.cDate).OrderByDescending(p => p.Date).FirstOrDefault();
            if (HttpContext.Current.User.IsInRole("GTP") || idTenant == 0)
            {
              /*  if (ticket.idTenant == 1) // if it is an internal GTP task
                {
                    repoCustomers repCustomers = new repoCustomers(); // for GTP users
                    lCustomers = repCustomers.getList(); // all customers
                    repoUsers repUsers = new repoUsers();
                    lAvailableUsers = repUsers.getList();// all users
                    lAvailablesAsignees = new AsigneeList(lCustomers, lAvailableUsers);
                    lSubTasks = repSubtasks.getList(idTicket);
                }
                else*/ // requested by Joerg on 13/dec/2015
                {
                    IEnumerable<Users> lGtpUsers;
                    using (repoUsers repUsers = new repoUsers())
                    { // if non gtp only users from the customer
                        int me = (int)HttpContext.Current.Session["IDUSER"];
                        Users currentUser = repUsers.Get(me);
                        if (currentUser.administrator == true) lGtpUsers = repUsers.getGTPPeople(); // requested by Joerg on 22/04/2016. Admins can see on watchlist all gtpusers
                        else lGtpUsers = repUsers.getGTPPeopleForThisTicket(ticket.id);
                        if (ticket.idTenant == 1) lAvailableUsers = repUsers.getListByTenant(ticket.idTenant); // to avoid duplicate GTP users
                        else lAvailableUsers = repUsers.getListByTenant(ticket.idTenant).Union(lGtpUsers);
                        lAvailablesAsignees = new AsigneeList(lCustomers, lAvailableUsers);
                        lSubTasks = repSubtasks.getList(idTicket);
                    }
                }
            }
            else
            {
                using (repoUsers repUsers = new repoUsers())
                { // if non gtp only users from the customer
                    lAvailableUsers = repUsers.getListByTenant(ticket.idTenant);
                    lAvailablesAsignees = new AsigneeList(lCustomers, lAvailableUsers);
                    int me = (int)HttpContext.Current.Session["IDUSER"];
                    lSubTasks = repSubtasks.getList(idTicket).Where(p => p.internalTask != true || (p.internalTask == true && p.idUser == me));
                }
            }
        }
    }
}