using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Crunching.Models;
using GTPTracker.repos;
using System.Diagnostics;
using GTPTracker.Functions;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace GTPTracker.Functions
{
    public class SecurityMgr : IDisposable
    {
        repoUsers repUsers = new repoUsers();
        repoCustomers repCustomer = new repoCustomers();

        
        internal static string GetStringSha256Hash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        public bool setActivationLink(string email, int emailType, bool test = false) // 1 - new user, 2 - forgot password
        {
            Users user = repUsers.getUserByEmail(email);
            if (user != null)
            {
                // generate token
                string token = GetStringSha256Hash(email);
                user.hash = token;
                user.hashDate = DateTime.UtcNow;
                user.status = "non-active";
                repUsers.Edit(user);
                // send email
                if (!test)
                {
                    using (EmailManager emailMgr = new EmailManager())
                    {
                        if (emailType == 1) emailMgr.sendActivationLinkNewUser(user, token);
                        if (emailType == 2) emailMgr.sendActivationLinkForgotPassword(user, token);
                    }
                }
                return true;
            }
            return false;
        }

        public int processActivationLink(string token)
        {
            Users user = repUsers.getUserByHash(token);
            if (user != null)
            {
                if (user.status == "active") return -3; // user already changed password using this link
                if (user.status == "blocked") return -4;
                DateTime? when = user.hashDate;
                if (when < DateTime.UtcNow.AddHours(-24))
                    return -1;// too old
                else return user.id;                
            }
            return -2;
        }

        public int? getMyKAM(int idUser)
        {
            Users user = repUsers.Get(idUser);
            Customers customer = repCustomer.Get(user.idTenant);
            if (user.idTenant == 0) return 1;
            return customer.idKeyAccountManager;
        }

        public int? getMyKAMByLINKID(string linkID)
        {
            int idCustomer = repCustomer.getIdTenantByLinkID(linkID);
            Customers customer = repCustomer.Get(idCustomer);
            return customer.idKeyAccountManager;
        }

        public void AddUserToCustomerRole(string UserName)
        {
            Roles.AddUserToRole(UserName, "Customer");
        }

        public void AddUserToGTPRole(string UserName)
        {
            Roles.AddUserToRole(UserName, "GTP");
        }

        public void AddUserToKAMRole(string UserName)
        {
            Roles.AddUserToRole(UserName, "KAM");
        }

        public void DeleteUserFromKAMRole(string UserName)
        {
            Roles.RemoveUserFromRole(UserName, "KAM");
        }

        public void AddUserToManagerRole(string UserName)
        {
            if (!Roles.RoleExists("MANAGER"))
                Roles.CreateRole("MANAGER");
            Roles.AddUserToRole(UserName, "MANAGER");
        }

        public void DeleteUserFromManagerRole(string UserName)
        {
            if (!Roles.RoleExists("MANAGER"))
                Roles.CreateRole("MANAGER");
            Roles.RemoveUserFromRole(UserName, "MANAGER");
        }

        public static bool Delete(string UserName)
        {
            return Membership.DeleteUser(UserName);
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
                repCustomer.Dispose();
                repUsers.Dispose();
            }
            // free native resources if there are any.
        }
    }
}