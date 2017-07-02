using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GTPTracker.repos;
using Crunching.Models;
using System.Web.Security;
using GTPTracker.Functions;
using System.Diagnostics;

namespace GTPTracker.BL
{
    public class UsersManager : IDisposable
    {
        /* Delete the user, both in users table and membership */
        public bool Delete(string email)
        {
            try
            {
                using (repoUsers repUsers = new repoUsers())
                {
                    Users user = repUsers.getUserByEmail(email);
                    if (user != null)
                    {
                        repUsers.Delete(user);
                        return Membership.DeleteUser(user.email);
                    }
                    return false;
                }
            }
            catch 
            { return false; }
        }

        /* Create the user both in users table and membership. if the user already exists delete it. Used in testing */
        public Users Create(string email, string firstName, string lastName, bool AcceptTerms, string title, string company, string position, string phone, string password, bool test = false)
        {
            try
            {
                Membership.DeleteUser(email, true);
                using (repoUsers repUsers = new repoUsers())
                {
                    if (repUsers.getUserByEmail(email) != null) // already exists. Delete him
                    {
                        Delete(email);
                        Membership.DeleteUser(email, true);
                    }
                    password = "patata16";
                    MembershipCreateStatus createStatus;
                    Membership.CreateUser(email, password, email, null, null, true, null, out createStatus);
                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        Users user = new Users();
                        user.email = email;
                        user.firstName = firstName;
                        user.lastName = lastName;
                        user.cDate = DateTime.UtcNow;
                        user.status = "non-active";
                        user.userAgreementSigned = AcceptTerms;
                        user.treatment = title;
                        user.position = position;
                        user.phone = phone;
                        user.company = company;
                        user.idType = 1;
                        repUsers.Create(user);
                        using (SecurityMgr securityMgr = new SecurityMgr())
                            securityMgr.setActivationLink(user.email, 1, test); // here the hash is set, so we have to refresh from db the user object. Otherwise hash will be null
                        user = repUsers.getUserByEmail(email); // to get the hash, hash date and status, saved on db 
                        return user;
                    }
                    Delete(email);
                    return null;
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.InnerException); Delete(email); return null; }
        }

        public void AssignRole(string role, Users user)
        {
            try
            {
                using (repoUsers repUsers = new repoUsers())
                {
                    if (user != null)
                    {
                        user.businessRole = role;
                        repUsers.Edit(user);
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        // send an email advicing that there are users with businessRole = null and active = true
        public int getNotAssignedUsers()
        {
            using (repoUsers repUsers = new repoUsers())
            {
                IEnumerable<Users> lUsers = repUsers.getNotAssignedUsers();
                int cont = lUsers.Count() ;
                if (cont == 0) return 0; // there is no active-not-assigned users. Do nothing
                else
                {
                    using (EmailManager mailManager = new EmailManager())
                    {
                        string hosting = System.Web.HttpContext.Current.Request.Url.ToString();
                        //var connectionToDB = System.Configuration.ConfigurationManager.ConnectionStrings["GTPTrackerEntities"].ConnectionString;
                        if (hosting.Contains("winhost") || hosting.Contains("localhost")) // in development
                            mailManager.emailDailyActivationReport(cont, "razonasistemas@gmail.com");
                        else if (hosting.Contains("gtp-toolbox"))
                            mailManager.emailDailyActivationReport(cont, "toolbox@gtp-schaefer.de");
                          //  mailManager.emailDailyActivationReport(cont, "razonasistemas@gmail.com");
                    }
                    return cont;
                }
            }
        }

        // check if the permissions of an user fulfill the rules
        public bool checkPermisions(Users user) // https://docs.google.com/spreadsheets/d/1JXYlR-QzK-ow3a7XeGCJ2jtEavfXUEhCnr2RYjIyHIg/edit#gid=1098398490
        {
            bool result = true;
            if (user != null)
            {
                if (user.businessRole == "visitor")
                {
                    if (user.generalAdmin == true || user.assignCustomersToProducts == true || user.seeCustomProducts == true || user.seeDraftProducts == true
                        || user.download2d == true || user.download3d == true || user.downloadInternalFiles == true || user.downloadCustomDocuments == true
                        || user.accessTickets == true)
                        return false;
                }
                if (user.businessRole == "GTP Team")
                {
                    if (user.generalAdmin == true || user.assignCustomersToProducts == true || user.seeCustomProducts == true || user.seeDraftProducts == true
                        || user.downloadInternalFiles == true )
                        return false;
                }
                if (user.businessRole == "customer")
                {
                    if (user.generalAdmin == true || user.assignCustomersToProducts == true || user.seeDraftProducts == true || user.downloadInternalFiles == true)
                        return false;
                }
                if (user.businessRole == "GTP KAM")
                {
                    if (user.seeDraftProducts == true)
                        return false;
                }
            }
            return result;
        }

        // check if the business role of the user fulfill the rules card #108
        public bool checkBusinessRole(Users user)
        {
            bool result = true;
            if (user != null)
            {
                if (user.businessRole == "GTP KAM") //at least one company assigned to him
                {
                    using (repoCustomers repCustomers = new repoCustomers())
                    {
                        int numMyCustomers = repCustomers.getvByKAM(user.id).Count();
                        if (numMyCustomers == 0) return false;
                    }
                } 
                if (user.businessRole == "Customer")
                {
                    if (user.idTenant == 0) return false;
                }
            }
            return result;
        }

        public void assignTenantToKAM(Users user, int idTenant)
        {
            using (repoCustomers repCustomers = new repoCustomers())
            {
                /*Customers customer = repCustomers.Get(idTenant);
                if (customer != null)
                {
                    customer.idKeyAccountManager = user.id;
                    repCustomers.Edit(customer);
                }*/
                repCustomers.AddKAMToTenant(idTenant, user.id);
            }
        }

        public void unassignTenantToKAM(Users user, int idTenant)
        {
            using (repoCustomers repCustomers = new repoCustomers())
            {
               /* Customers customer = repCustomers.Get(idTenant);
                if (customer != null)
                {
                    customer.idKeyAccountManager = null;
                    repCustomers.Edit(customer);
                }*/
                repCustomers.DeleteKAMFromTenant(user.id, idTenant);
            }
        }

        public void changeStatus(int idUser, string status)
        {
            using (repoUsers repUsers = new repoUsers())
            {
                Users user = repUsers.Get(idUser);
                user.status = status;
                repUsers.Edit(user);
            }
        }

        /* return the products assigned to an user. If the user is a manager return all
         * if the user is a KAM return the public + custom */
        public IEnumerable<Products> getMyProducts(int idUser)
        {
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            using (repoProducts repProducts = new repoProducts())
            using (repoUsers repUsers = new repoUsers())
            {
                Users user = repUsers.Get(idUser);
                IEnumerable<int> lMyProductsInt;
                IEnumerable<Products> lAllProducts = repProducts.getAll();
                IEnumerable<Products> lProducts = lAllProducts.Where(p => p.status == "public");

                if (user.businessRole == "GTP General Manager" || user.businessRole == "GTP KAM") 
                    lMyProductsInt = db.CustomerProducts.Select(p => p.idProduct).ToList();
                else lMyProductsInt = db.CustomerProducts.Where(p=>p.idCustomer == user.idTenant).Select(p => p.idProduct).ToList();

                IEnumerable<Products> lMyCustomProducts = lAllProducts.Where(p => p.status == "custom").Where(p => lMyProductsInt.Contains(p.id));
                lProducts = lProducts.Concat(lMyCustomProducts);
               
                if (user.businessRole == "GTP General Manager" )
                {                    
                        lProducts = lProducts.Concat(lAllProducts.Where(p => p.status == "draft"));
                }
                return lProducts;
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