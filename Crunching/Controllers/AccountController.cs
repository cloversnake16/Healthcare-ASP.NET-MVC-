using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Crunching.Models;
using GTPTracker.repos;
using GTPTracker.Helpers;
using System.Diagnostics;
using GTPTracker.BL;
using System.Text;


namespace GTPTracker.Controllers
{
    public class AccountController : Controller
    {
        repoUsers repUsers = new repoUsers();
        bool TEST_MODE = false;

        public ActionResult signIn()
        {
            LogOnModel logonModel = new LogOnModel();
            bool userLoggedIn = (System.Web.HttpContext.Current.User.Identity.IsAuthenticated && Session["IDUSER"] != null);
            ViewBag.HideUserPanel = !userLoggedIn;
            if (userLoggedIn)
            {
               // return RedirectToAction("IndexItems", "Series");
                return RedirectToAction("Index", "Catalog");
            }
            return View(logonModel);
        }

        [LogFilter]
        [HttpPost]
        public ActionResult signIn(LogOnModel model, string returnUrl)
        {
            bool userLoggedIn = (System.Web.HttpContext.Current.User.Identity.IsAuthenticated && Session["IDUSER"] != null);
            ViewBag.HideUserPanel = !userLoggedIn;

            if (Membership.ValidateUser(model.UserName, model.Password))
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    Users user = repUsers.getUserByEmail(model.UserName);
                    // if (user == null) user = repUsers.getUserByEmail(model.UserName); // sometimes the email is disabled, so use the name
                    if (user != null)
                    {
                        if (user.status == "blocked")
                        {
                            ModelState.AddModelError("", "Account not active. Please contact GTP");
                            return View(model);
                        }
                        if (user.status == "non-active")
                        {
                            ModelState.AddModelError("", "Account not active. Reset your password to get a new activation link.");
                            return View(model);
                        }
                        using (repoCustomers repCustomers = new repoCustomers())
                        {
                            Session["IDTENANT"] = user.idTenant;
                            Session["IDUSER"] = user.id;
                            Session["USERNAME"] = user.firstName + " " + user.lastName;

                            Session["PERMADMIN"] = user.generalAdmin;
                            Session["PERMSEEPUBLICCATALOG"] = user.seePublicCatalog;
                            Session["PERMSEEDRAFTPRODUCTS"] = user.seeDraftProducts;
                            Session["PERMSEECUSTOMPRODUCTS"] = user.seeCustomProducts;
                            Session["PERMDOWNLOAD2D"] = user.download2d;
                            Session["PERMSDOWNLOAD3D"] = user.download3d;
                            Session["PERMDOWNLOADINTERNALFILES"] = user.downloadInternalFiles;
                            Session["PERMDOCUMENTS"] = user.downloadPublicDocuments;
                            Session["PERMASSIGNCUSTOMERSTOPRODUCTS"] = user.assignCustomersToProducts;
                            Session["PERMACCESSTICKETS"] = user.accessTickets;
                            Session["PERMACCESSCALC"] = user.accessCalculator;
                            Session["PERMVIEWVISITREPORTS"] = user.viewVisitReports;
                            Session["PERMFILTERVISITREPORTS"] = user.filterVisitReports;

                            if (user.businessRole == null) 
                                Session["BUSINESSROLE"] = "Unassigned";
                            else
                            {
                                Session["BUSINESSROLE"] = user.businessRole;
                                if (user.businessRole == "GTP General Manager" || user.businessRole == "GTP KAM")
                                {
                                    //Session["PERMSEEDRAFTPRODUCTS"] = true;
                                    Session["IDTENANT"] = 0;
                                }
                            }
                            
                            Customers tenant = repCustomers.Get(user.idTenant);
                            if (tenant != null)
                            {
                                if (tenant.idKeyAccountManager != null)
                                {
                                    Users kam = repUsers.Get((int)tenant.idKeyAccountManager);
                                    Session["KAM"] = kam.email;
                                }
                                Session["TENANT"] = tenant.name;
                            }
                            if (user.administrator == true) Session["ADMIN"] = "true";
                            else Session["ADMIN"] = "false";


                            var cookie = new HttpCookie("GTPTRACKER1");
                            using (Functions.SecurityMgr secMgr = new Functions.SecurityMgr())
                            {
                                try
                                {
                                    string cryptedIDUSER = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes("IDUSER"));
                                    cookie.Values.Add(cryptedIDUSER, Functions.AES.AesEncryption(user.id.ToString()));
                                    cookie.Values.Add("U", Functions.AES.AesEncryption(user.firstName + " " + user.lastName));
                                    if (tenant != null)
                                        cookie.Values.Add("T", Functions.AES.AesEncryption(tenant.name));
                                    cookie.Values.Add("C", Functions.AES.AesEncryption(model.Password));
                                    cookie.Values.Add("A", Functions.AES.AesEncryption((string)Session["ADMIN"]));
                                    Response.Cookies.Add(cookie);
                                    Debug.WriteLine("Cookie = " + Functions.AES.AesEncryption(user.id.ToString()));
                                    Debug.WriteLine("Decrupt Cookie = " + Functions.AES.AesDecryption(Functions.AES.AesEncryption(user.id.ToString())));
                                }
                                catch (Exception e)
                                {
                                    Debug.WriteLine("Error: {0}", e.Message);
                                }
                            }
                            


                            if (user.userAgreementSigned == true) // already signed
                            {
                                Session["TERMSSIGNED"] = true;
                                return RedirectToAction("Index", "Home");
                            }
                            else return RedirectToAction("showUserAgreement", "Users");
                        }
                    }
                    else
                    {//RedirectToAction("LogOn", "Account");
                        ModelState.AddModelError("", "No user found, please signup or contact GTP.");
                        return View(model);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "The user name or password provided is incorrect.");
                return View(model);
            }
        }


        public ActionResult forgotPassword()
        {
            bool userLoggedIn = (System.Web.HttpContext.Current.User.Identity.IsAuthenticated && Session["IDUSER"] != null);
            ViewBag.HideUserPanel = !userLoggedIn;
            ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
            return View();
        }

        public ActionResult forgotPasswordSuccess()
        {
            bool userLoggedIn = (System.Web.HttpContext.Current.User.Identity.IsAuthenticated && Session["IDUSER"] != null);
            ViewBag.HideUserPanel = !userLoggedIn;
            ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
            return View();
        }

        public ActionResult signUp()
        {
            bool userLoggedIn = (System.Web.HttpContext.Current.User.Identity.IsAuthenticated && Session["IDUSER"] != null);
            ViewBag.HideUserPanel = !userLoggedIn;
            ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated); ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
            return View();
        }

        [HttpPost]
        public ActionResult signUp(string companyName, string username, string email, string password)
        {
            RegisterModel model = new RegisterModel();
            model.Email = email;
            model.Password = password;
            model.ConfirmPassword = password;

            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                Membership.CreateUser(model.Email, model.Password, model.Email, null, null, true, null, out createStatus);
                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsAuthentication.SetAuthCookie(model.Email, true /* createPersistentCookie */);
                    // Create here the tenant record and the user record
                    return RedirectToAction("Index", "");
                }
                else ModelState.AddModelError("", ErrorCodeToString(createStatus));
            }
            ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
            return View(model); // If we got this far, something failed, redisplay form
        }

        public string Register(string email, string firstName, string lastName, string title, string company, string position, string phone, string terms)
        {
            string result = "";
            bool isValid = true;
            bool isValidEmail = Helpers.formatter.isValidEmail(email);
            if (!isValidEmail) { result += "Please provide a valid E-Mail address<br>"; isValid = false; }
            if (firstName == "") { result += "First Name is a mandatory field<br>"; isValid = false; }
            if (lastName == "") { result += "Last Name is a mandatory field<br>"; isValid = false; }
            if (title == "") { result += "Title is a mandatory field<br>"; isValid = false; }
            if (terms != "true") { result += "You have to accept the terms and conditions<br>"; isValid = false; }

            if (!isValid) return result; // if there where errors, show all of them now
            using (repoUsers repUsers = new repoUsers())
            {
                Users user = repUsers.getUserByEmail(email);
                if (user != null)
                    if (user.status == "blocked")
                        return "User already exists. Please contact GTP.";
                    else return "User already exists. Login or reset your password.";
            }

            using (UsersManager um = new UsersManager())
            {
                um.Create(email, firstName, lastName, true, title, company, position, phone, Membership.GeneratePassword(12, 1), TEST_MODE); // generate a random password.
                return "1";
            }
        }

        public ActionResult ThankYouForRegister()
        {
            ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
            return View();
        }


        [LogFilter]
        public ActionResult CheckForTermsAndConditions(int idUser)
        {
            using (repoUsers repUsers = new repoUsers())
            {
                Users user = repUsers.Get(idUser);
                if (user != null)
                {
                    using (repoCustomers repCustomers = new repoCustomers())
                    {
                        Customers tenant = repCustomers.Get(user.idTenant);
                        if (tenant.userAgreementSigned == true) // already signed
                            return RedirectToAction("Index", "Home");
                        else return RedirectToAction("showUserAgreement", "Customers");
                    }
                }
                return RedirectToAction("showUserAgreement", "Customers");
            }
        }


        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();

            // clear authentication cookie
            HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie1.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie1);

            // clear session cookie (not necessary for your current problem but i would recommend you do it anyway)
            /*     HttpCookie cookie2 = new HttpCookie("ASP.NET_SessionId", "");
                 cookie2.Expires = DateTime.Now.AddYears(-1);
                 Response.Cookies.Add(cookie2);*/

            return RedirectToAction("signIn", "Account");
        }


        [Authorize]
        public ActionResult ChangePassword()
        {
            ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
            return View();
        }

        [LogFilter]
        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("Index", "Catalog", new { alert = 2 });
                    //return RedirectToAction("IndexItems", "Series", new { alert = 2 });
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
            return View(model);
        }

        public ActionResult ChangePasswordSuccess()
        {
            ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
            return View();
        }

        [Authorize]
        [SessionExpireFilter]
        public ActionResult EditProfile()
        {
            int userId = (int)Session["IDUSER"];
            var userRow = this.repUsers.Get(userId);
            if (userRow != null)
            {
                string TenantName = string.Empty;
                if (userRow.idTenant != 0)
                {
                    using (var repCustomers = new repoCustomers())
                    {

                        Customers customer = repCustomers.Get(userRow.idTenant);
                        if (customer != null)
                        {
                            TenantName = customer.name;
                        }
                    }
                }

                var modelSalutation = new List<string>();
                modelSalutation.Add("Mr.");
                modelSalutation.Add("Mrs.");
                ViewBag.lSalutation = modelSalutation;

                ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
                return View(new ProfileModel
                {
                    BusinessRole = userRow.businessRole,
                    Email = userRow.email,
                    FirstName = userRow.firstName,
                    LastName = userRow.lastName,
                    TenantName = TenantName,
                    Treatment = userRow.treatment,
                    UserId = userId
                });
            }
            else
            {
                return Redirect("/");
            }
        }


        [LogFilter]
        [Authorize]
        [HttpPost]
        public ActionResult EditProfile(ProfileModel model)
        {
            int userId = (int)Session["IDUSER"];
            model.UserId = userId;
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool saveSuccessful;
                try
                {
                    Users user = repUsers.Get(userId);
                    user.firstName = model.FirstName;
                    user.lastName = model.LastName;
                    user.treatment = model.Treatment;
                    repUsers.Edit(user);
                    saveSuccessful = true;
                }
                catch (Exception)
                {
                    saveSuccessful = false;
                }

                if (saveSuccessful)
                {
                    return RedirectToAction("Index", "Catalog", new { alert = 1 });
                    //return RedirectToAction("IndexItems", "Series", new { alert = 1 });
                }
                else
                {
                    ModelState.AddModelError("", "Error while saving profile, please fill all fields.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }




        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
