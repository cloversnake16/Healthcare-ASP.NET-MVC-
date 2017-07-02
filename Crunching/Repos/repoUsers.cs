using Crunching.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using GTPTracker.Helpers;
using System.Web.Security;


namespace GTPTracker.repos
{
    public class repoUsers : IDisposable
    {
        public void AcceptUserAgreement(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                Users item = db.Users.FirstOrDefault(p => p.id == id);
                item.userAgreementSigned = true;
                db.Users.ApplyCurrentValues(item);
                db.SaveChanges();
            }
        }

        public IEnumerable<vUsers> getKAMs()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vUsers.Where(p=>p.businessRole == "GTP KAM").ToList();
            }
        }

        // Used in the daily activation report. Return all active users but still without a business role
        public IEnumerable<Users> getNotAssignedUsers()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Users.Where(p=>p.businessRole == null && p.status == "non-active").ToList();
            }
        }

        public IEnumerable<Users> getList()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Users.ToList();
            }
        }

        public IEnumerable<vUsers> getvList()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vUsers.ToList();
            }
        }

        public IEnumerable<Users> getListByTenant(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Users.Where(p => p.idTenant == id).ToList();
            }
        }
        /****** for the predefined tasks ****/
        // returns a list of users that belongs to GTP and are not customers. In addition dev users like andreas, Luis, etc are not in the returned list
        public IEnumerable<vUsers> getvGTPUsers()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vUsers.Where(p => p.businessRole != null && p.idTenant == 0 && p.devUser == false).ToList();
            }
        }

        public IEnumerable<Users> getGTPPeople()
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Users.Where(p => p.idType == 2 || p.idType == 3).ToList();
            }
        }

        /// <summary>
        /// Retuns the list of GTP people who should be added to the distribution list, depending of the customer and the tipe
        /// </summary>
        /// <param name="idTicket">idTicket</param>
        /// <returns>an Ienumerable with the GTP users that has to be added to the distribution list</returns>
        public IEnumerable<Users> getGTPPeopleForThisTicket(int idTicket)
        {
            IEnumerable<Users> lAdmins;
            IEnumerable<Users> lKAMs;

            using (var db = new GTPTrackerEntities())
            {
                Tickets ticket = db.Tickets.FirstOrDefault(p => p.id == idTicket);

                // admins who has the notif of the type active + KAM of this customer
                switch (ticket.idType)
                {
                    case 1:
                        lAdmins = db.Users.Where(p => p.idType == 2 && p.notifTechnical != false && p.administrator == true).ToList();
                        break;
                    case 2:
                        lAdmins = db.Users.Where(p => p.idType == 2 && p.notifClaims != false && p.administrator == true).ToList();
                        break;
                    default:
                        lAdmins = db.Users.Where(p => p.idType == 2 && p.notifSamples != false && p.administrator == true).ToList();
                        break;
                }
                lKAMs = db.Users.Where(p => p.id == ticket.idKeyAccountManager).ToList();
            }

            return lKAMs.Union(lAdmins).ToList();
        }

        public void CreateGTP(Users item)
        {
            item.idType = 2;
            using (var db = new GTPTrackerEntities())
            {
                db.Users.AddObject(item);
                db.SaveChanges();
            }
        }

        public void CreateSalesRep(Users item)
        {
            item.idType = 3;
            using (var db = new GTPTrackerEntities())
            {
                db.Users.AddObject(item);
                db.SaveChanges();
            }
        }

        public Users Get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Users.FirstOrDefault(p => p.id == id);
            }
        }

        public void Create(Users item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Users.AddObject(item);
                db.SaveChanges();
            }
        }

        public void Edit(Users item)
        {
            string currentUsername = string.Empty;
            bool emailChanged = false;
            using (var db = new GTPTrackerEntities())
            {
                var result = db.Users.FirstOrDefault(p => p.id == item.id);
                currentUsername = result.email;
                emailChanged = currentUsername != item.email;
                if (emailChanged)
                {
                    // need to make sure no other users have same email
                    if (db.Users.Any(m => m.email.Equals(item.email) && m.id != item.id))
                    {
                        emailChanged = false;
                        item.email = currentUsername;
                    }
                }

                db.Users.ApplyCurrentValues(item);
                db.SaveChanges();
            }

            if (emailChanged)
                this.ChangeUsername(currentUsername, item.email);
        }

        public void ChangeUsername(string oldUsername, string newUsername)
        {
            GTPMembershipProvider p = (GTPMembershipProvider)Membership.Provider;
            p.ChangeUsername(oldUsername, newUsername);
         }

        public void Delete(Users item)
        {
            using (var db = new GTPTrackerEntities())
            {
                Users des = db.Users.FirstOrDefault(e => e.id == item.id);
                if (des != null)
                {
                    db.Users.DeleteObject(des);
                    db.SaveChanges();
                }
            }
        }

        public Users getUserByHash(string hash)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Users.FirstOrDefault(p => p.hash == hash);
            }
        }

        public Users getUserByEmail(string email)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Users.FirstOrDefault(p => p.email == email);
            }
        }

        public vUsers getKAMByName(string fullName)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.vUsers.FirstOrDefault(p => p.fullName == fullName);
            }
        }

        public void DeleteAll()
        {
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            {
                db.ExecuteStoreCommand("truncate table tickets");
                db.ExecuteStoreCommand("truncate table comments");
                db.ExecuteStoreCommand("truncate table subtasks");
                db.ExecuteStoreCommand("truncate table items");
                db.ExecuteStoreCommand("truncate table news");
                db.ExecuteStoreCommand("truncate table milestones");
                db.ExecuteStoreCommand("truncate table commentsFiles");
                db.ExecuteStoreCommand("truncate table tasksComments");
                db.ExecuteStoreCommand("truncate table tasksFiles");
                db.ExecuteStoreCommand("truncate table ticketsFiles");
                db.ExecuteStoreCommand("truncate table ticketsMailingList");
                db.ExecuteStoreCommand("truncate table photos");
            }
        }

        public void SetCleanInstallation()
        {
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            {
                db.ExecuteStoreCommand("DELETE NewsWatchList");
                db.ExecuteStoreCommand("DELETE Comments WHERE id>2");
                db.ExecuteStoreCommand("TRUNCATE TABLE CommentsFiles");
                db.ExecuteStoreCommand("DELETE customers WHERE id >1012");
                db.ExecuteStoreCommand("DELETE CustomerProducts WHERE id>46");
                db.ExecuteStoreCommand("DELETE GeneralProductsDocuments WHERE id>7");
                db.ExecuteStoreCommand("DELETE items WHERE id>3");
                db.ExecuteStoreCommand("TRUNCATE TABLE Milestones");
                db.ExecuteStoreCommand("TRUNCATE TABLE MilestonesWatchList");
                db.ExecuteStoreCommand("TRUNCATE TABLE News");
                db.ExecuteStoreCommand("TRUNCATE TABLE NewsCustomers");
                db.ExecuteStoreCommand("TRUNCATE TABLE NewsProducts");
                db.ExecuteStoreCommand("TRUNCATE TABLE NewsWatchList");
                db.ExecuteStoreCommand("DELETE photos WHERE id>2");
                db.ExecuteStoreCommand("DELETE products WHERE id>654");
                db.ExecuteStoreCommand("DELETE ProductsFiles WHERE id>27642");
                db.ExecuteStoreCommand("DELETE series WHERE id>1009");
                db.ExecuteStoreCommand("DELETE subtasks WHERE id>2");
                db.ExecuteStoreCommand("DELETE tasksComments WHERE id>3");
                db.ExecuteStoreCommand("DELETE tasksFiles WHERE id>1");
                db.ExecuteStoreCommand("DELETE tickets WHERE id>1");
                db.ExecuteStoreCommand("TRUNCATE TABLE ticketsFiles");
                db.ExecuteStoreCommand("DELETE TicketsMailingList WHERE id>2");
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