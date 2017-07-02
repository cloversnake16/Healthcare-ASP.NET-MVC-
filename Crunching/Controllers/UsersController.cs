using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Crunching.Models;
using GTPTracker.repos;
using System.Web.Security;
using GTPTracker.Functions;
using GTPTracker.Helpers;
using GTPTracker.BL;
using System.Diagnostics;

namespace GTPTracker.Controllers
{
    public class UsersController : Controller
    {
       // bool TEST_MODE = false;

        repoUsers repUsers = new repoUsers();
        repoCustomers repCustomers = new repoCustomers();

        SecurityMgr securityMgr = new SecurityMgr();
        EmailManager emailMgr = new EmailManager();

        [SessionExpireFilter]
        public ActionResult showImprint()
        {
            return View();
        }

        [SessionExpireFilter]
        public ActionResult showUserAgreement()
        {
            return View();
        }

        [HttpPost]
        [SessionExpireFilter]
        public ActionResult showUserAgreement(bool? accepted)
        {
            using (repoUsers rep = new repoUsers()) 
            {
                rep.AcceptUserAgreement((int)Session["IDUSER"]);
                Session["TERMSSIGNED"] = true;
            }
            return RedirectToAction("Index", "Home");
        }

        [SessionExpireFilter]
        public ActionResult Index(int? whatToDo)
        {
            return RedirectToAction("usersManager", new { whatToDo = whatToDo }); // NEW
          /*  ViewBag.action = whatToDo;
            IEnumerable<Users> lUsers = repUsers.getList();
            if (HttpContext.User.IsInRole("GTP"))
                return View(repUsers.getGTPPeople());
            else return View(lUsers.Where(p => p.idTenant == (int)Session["IDTENANT"])); // show only users of my company*/
        }

        [SessionExpireFilter]
        public ActionResult IndexByTenant(int id)
        {
            ViewBag.idTenant = id;
            Customers customer = repCustomers.Get(id);
            ViewBag.customerName = customer.name;
            IEnumerable<Users> lUsers = repUsers.getListByTenant(id);
            return View(lUsers);
        }

        [SessionExpireFilter]
        public ActionResult IndexGTP()
        {
            int idEntidad = (int)Session["IDTENANT"];
            Customers customer = repCustomers.Get(idEntidad);
            ViewBag.idKAM = customer.idKeyAccountManager;
            IEnumerable<Users> lUsers = repUsers.getGTPPeople().Where(p => p.showInGallery == true && !(p.isKAM == true && p.showOnlyToCustomers == true && p.id != customer.idKeyAccountManager)); // TODO from here remove the kam with showonlytocustomers false and not current kam
            return View("IndexGTPGallery", lUsers);
        }

        [SessionExpireFilter]
        public ActionResult CreateGTPUser()
        {
            Users user = new Users();
            return View(user);
        }

        [SessionExpireFilter]
        public ActionResult Details(int id)
        {
            return View();
        }

        [SessionExpireFilter]
        public ActionResult Create(int? idTenant)
        {
            if (idTenant == null) idTenant = (int)Session["IDTENANT"];
            Users user = new Users();
            user.idTenant = (int)idTenant;
            return View(user);
        }

        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Create(Users user, string isManager)
        {
            try
            {
                if (isManager == "yes")
                {
                    user.isCustomerManager = true;
                    securityMgr.AddUserToManagerRole(user.email);
                }
                else
                {
                    user.isCustomerManager = false;
                  //  securityMgr.DeleteUserFromManagerRole(user.email);
                }
       
                user.cDate = DateTime.Today;
                if (ModelState.IsValid)
                {
                    // Attempt to register the user
                    string password = Membership.GeneratePassword(12, 1);
                    MembershipCreateStatus createStatus;
                    Membership.CreateUser(user.email, password, user.email, null, null, true, null, out createStatus);
                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        user.idType = 1;
                        user.status = "non-active";
                        repUsers.Create(user);
                        securityMgr.AddUserToCustomerRole(user.email);
                      //  emailMgr.emailNewUser(user, user.email, password);
                        securityMgr.setActivationLink(user.email, 1);
                        return RedirectToAction("IndexByTenant", new { id = user.idTenant });
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorCodeToString(createStatus));
                    }
                }

                // If we got this far, something failed, redisplay form
                return View(user);
            }
            catch
            {
                return View();
            }
        }

        [SessionExpireFilter]
        public ActionResult CreateGTP()
        {
            Users user = new Users();
            user.idTenant = 1;
            return View(user);
        }

        [SessionExpireFilter]
        [HttpPost]
        [Authorize(Roles = "GTP")]
        public ActionResult CreateGTP(Users user, HttpPostedFileBase file, string showInGallery, string showOnlyToCustomers, string admin, string notifTechnical, string notifClaims, string notifSamples)
        {

            user.cDate = DateTime.Today;
            // Attempt to register the user
            string password = Membership.GeneratePassword(12, 1);
            MembershipCreateStatus createStatus;
            Membership.CreateUser(user.email, password, user.email, null, null, true, null, out createStatus);
            try
            {
                if (createStatus == MembershipCreateStatus.Success)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        if (file.ContentLength > 20000000)
                        {
                            ModelState.AddModelError("", "File is too big. Max size allowed 20 MB");
                            return View(user);
                        }
                        var fileName = Convert.ToString(user.firstName) + "__" + DateTime.UtcNow.Ticks.ToString() + "-" + System.IO.Path.GetFileName(file.FileName);
                        var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                        file.SaveAs(path);
                        user.image = fileName;
                    }
                    else
                    {
                        var defaultName = "Andreas Diehl__635549669157821969-^0351CB4F03BE628D8922D4E4CA6EAAEE08E2455786529EBDBC^pimgpsh_fullsize_distr.jpg";
                        var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), defaultName);
                        //file.SaveAs(path);
                        user.image = defaultName;
                    }
                    user.idType = 2;
                    user.idTenant = 1;

                    if (showInGallery == "yes") user.showInGallery = true;
                    else user.showInGallery = false;
                    if (showOnlyToCustomers == "yes") user.showOnlyToCustomers = true;
                    else user.showOnlyToCustomers = false;
                    if (admin != "yes")
                    {
                        securityMgr.AddUserToKAMRole(user.email);
                        user.administrator = false;
                    }
                    else user.administrator = true;

                    if (notifTechnical == "yes") user.notifTechnical = true;
                    else user.notifTechnical = false;
                    if (notifClaims == "yes") user.notifClaims = true;
                    else user.notifClaims = false;
                    if (notifSamples == "yes") user.notifSamples = true;
                    else user.notifSamples = false;

                    repUsers.Create(user);
                    securityMgr.AddUserToGTPRole(user.email);
                    
                    //emailMgr.emailNewUser(user, user.email, password);
                    securityMgr.setActivationLink(user.email, 1);
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                ModelState.AddModelError("email", "Email address either taken or missing");
               // ModelState.AddModelError("", ErrorCodeToString(createStatus));
                return View(user);
            }
            return View(user);
        }

        [SessionExpireFilter]
        public ActionResult Edit(int id, int? whatToDo)
        {
            Users user = repUsers.Get(id);
            var modelSalutation = new List<string>();
            modelSalutation.Add("Mr.");
            modelSalutation.Add("Mrs.");
            ViewBag.lSalutation = modelSalutation;
            var modelRoles = new List<string>();
            modelRoles.Add("Unassigned");
            modelRoles.Add("GTP General Manager");
            modelRoles.Add("GTP KAM");
            modelRoles.Add("Customer");
            modelRoles.Add("GTP Team");            
            ViewBag.lRoles = modelRoles;
            using (repoCustomers repCustomers = new repoCustomers())
            {
                IEnumerable<vCustomers> lCustomers = repCustomers.getList().Where(p=>p.active != false);
                IEnumerable<vCustomers> lAssignedTenants = repCustomers.getvByKAM(id);
                IEnumerable<int> lIdsAssignedTenants = lAssignedTenants.Select(p=>p.id);
                IEnumerable<vCustomers> lTenants = lCustomers.Where(p=>!(lIdsAssignedTenants.Contains(p.id)));
                ViewBag.lTenants = lTenants.OrderBy(p => p.name);
                ViewBag.lAssignedTenants = lAssignedTenants;
            }
            ViewBag.whatToDo = whatToDo;
            return View(user);
        }

        /// <summary>
        /// Because of a bug in binding the permissions to the user, the permissions in lila in the gsheet are set here everytime the user is saved
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private Users setPermissionsByRole(Users user)
        {
            if (user.businessRole == "GTP General Manager")
            {
                user.generalAdmin = true;
                user.assignCustomersToProducts = true;
                user.seeCustomProducts = true;
                user.accessTickets = true;

                user.viewVisitReports = true;
                user.createEditVisitReports = true;
                user.viewTasks = true;
                user.createEditTasks = true;
                user.changeTaskStatus = true;
            }
            if (user.businessRole == "GTP KAM")
            {
                user.viewVisitReports = true;
                user.createEditVisitReports = true;
                user.viewTasks = true;
                user.createEditTasks = true;
                user.changeTaskStatus = true;
            }
            if (user.businessRole == "GTP Team")
            {
                user.viewVisitReports = true;
                user.viewTasks = true;
                user.changeTaskStatus = true;
            }
            if (user.businessRole == "Customer")
            {
                user.viewVisitReports = true;
                user.viewTasks = true;
                user.changeTaskStatus = true;
            }
            return user;
        }

        [LogFilter]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Edit(Users user)
        {
            user = setPermissionsByRole(user);
            try
            {
                if (user.businessRole == "Customer" && user.idTenant == 0)
                {
                    ModelState.AddModelError("", "An user with business role Customer has to have a tenant assigned");
                    var modelSalutation = new List<string>();
                    modelSalutation.Add("Mr.");
                    modelSalutation.Add("Mrs.");
                    ViewBag.lSalutation = modelSalutation;
                    var modelRoles = new List<string>();
                    modelRoles.Add("GTP General Manager");
                    modelRoles.Add("GTP KAM");
                    modelRoles.Add("Customer");
                    modelRoles.Add("GTP Team");
                    modelRoles.Add("Unassiged");
                    ViewBag.lRoles = modelRoles;
                    using (repoCustomers repCustomers = new repoCustomers())
                    {
                        IEnumerable<vCustomers> lCustomers = repCustomers.getList().Where(p => p.active != false);
                        IEnumerable<vCustomers> lAssignedTenants = repCustomers.getvByKAM(user.id);
                        IEnumerable<int> lIdsAssignedTenants = lAssignedTenants.Select(p => p.id);
                        IEnumerable<vCustomers> lTenants = lCustomers.Where(p => !(lIdsAssignedTenants.Contains(p.id)));
                        ViewBag.lTenants = lTenants.OrderBy(p => p.name);
                        ViewBag.lAssignedTenants = lAssignedTenants;
                    }
                    ViewBag.error = "An user with business role Customer has to have a tenant assigned";
                    return View(user);
                }
                Users oldUser = repUsers.Get(user.id);
                if (user.id == (int)Session["IDUSER"])
                {
                    if (user.businessRole != oldUser.businessRole) // you are trying to change your own business role. Forbidden
                    {
                        var modelSalutation = new List<string>();
                        modelSalutation.Add("Mr.");
                        modelSalutation.Add("Mrs.");
                        ViewBag.lSalutation = modelSalutation;
                        var modelRoles = new List<string>();
                        modelRoles.Add("Unassigned");
                        modelRoles.Add("GTP General Manager");
                        modelRoles.Add("GTP KAM");
                        modelRoles.Add("Customer");
                        modelRoles.Add("GTP Team");
                        ViewBag.lRoles = modelRoles;
                        using (repoCustomers repCustomers = new repoCustomers())
                        {
                            IEnumerable<vCustomers> lCustomers = repCustomers.getList().Where(p => p.active != false);
                            IEnumerable<vCustomers> lAssignedTenants = repCustomers.getvByKAM(user.id);
                            IEnumerable<int> lIdsAssignedTenants = lAssignedTenants.Select(p => p.id);
                            IEnumerable<vCustomers> lTenants = lCustomers.Where(p => !(lIdsAssignedTenants.Contains(p.id)));
                            ViewBag.lTenants = lTenants.OrderBy(p => p.name);
                            ViewBag.lAssignedTenants = lAssignedTenants;
                        }
                        ModelState.AddModelError("", "You can not change your own business role");
                        ViewBag.error = "You can not change your own business role";
                        return View(user);
                    }
                }
                if (user.businessRole == "Customer" && oldUser.businessRole != "Customer") // if now is customer and before was other thing, customer was assinged right now, so send an email
                {
                    using (EmailManager emailMgr = new EmailManager())
                    using (repoCustomers repCustomer = new repoCustomers())
                    {
                        Customers tenant = repCustomer.Get(user.idTenant);
                        if (tenant != null)
                            emailMgr.sendAssignCustomerBusinessRole(user, tenant.name);
                    }                    
                }
                // RULE : only customer business role can have idTenant != 0
                if (user.businessRole != "Customer")
                    user.idTenant = 0;
                
                repUsers.Edit(user);
                
                if (user.businessRole == "GTP KAM")
                {
                    if (!string.IsNullOrEmpty(Request.Params["assignedCustomers"]))
                    {
                        var assignedCustomers = Request.Params["assignedCustomers"].Split(',');
                        repCustomers.DeleteAllCustomersKAMsByUser(user.id);
                        foreach (var item in assignedCustomers)
                        {
                            int result;
                            bool isInt = Int32.TryParse(item, out result);
                            if (isInt)
                                repCustomers.AddKAMToTenant(Int32.Parse(item), user.id);
                            else
                            {
                                Customers customer = repCustomers.GetByName(item);
                                repCustomers.AddKAMToTenant(customer.id, user.id);
                            }
                        }
                    }
                }
                return RedirectToAction("usersManager", "Users", new { whatToDo = 1 });
            }
            catch (Exception ex)
            {
                var modelSalutation = new List<string>();
                modelSalutation.Add("Mr.");
                modelSalutation.Add("Mrs.");
                ViewBag.lSalutation = modelSalutation;
                var modelRoles = new List<string>();
                modelRoles.Add("Unassigned");
                modelRoles.Add("GTP General Manager");
                modelRoles.Add("GTP KAM");
                modelRoles.Add("Customer");
                modelRoles.Add("GTP Team");
                ViewBag.lRoles = modelRoles;
                using (repoCustomers repCustomers = new repoCustomers())
                {
                    IEnumerable<vCustomers> lCustomers = repCustomers.getList().Where(p => p.active != false);
                    IEnumerable<vCustomers> lAssignedTenants = repCustomers.getvByKAM(user.id);
                    IEnumerable<int> lIdsAssignedTenants = lAssignedTenants.Select(p => p.id);
                    IEnumerable<vCustomers> lTenants = lCustomers.Where(p => !(lIdsAssignedTenants.Contains(p.id)));
                    ViewBag.lTenants = lTenants.OrderBy(p => p.name);
                    ViewBag.lAssignedTenants = lAssignedTenants;
                }
                ModelState.AddModelError("", ex.Message);
                ViewBag.error = ex.Message;
                return View(user.id);
            }
        }

        [LogFilter]
        [SessionExpireFilter]
        public ActionResult applyBulkActions(String actionsList, string bulkOptions)
        {
            try
            {
                char[] delimiterChars = { ',' };

                string[] words = actionsList.Split(delimiterChars);
                if (words.Count() > 0)
                {
                    using (repoUsers repUsers = new repoUsers())
                    using(UsersManager userManager = new UsersManager())
                    {
                        if (bulkOptions == "Delete user")
                        {
                            foreach (var email in words)
                            {
                                Users user = repUsers.getUserByEmail(email);
                                if (user != null && user.id != (int)Session["IDUSER"])
                                    userManager.Delete(email);
                            }
                            return RedirectToAction("Index", new { whatToDo = 2 });
                        }
                        if (bulkOptions == "Block user")
                        {
                            foreach (var email in words)
                            {
                                Users user = repUsers.getUserByEmail(email);
                                if (user.id != (int)Session["IDUSER"])
                                    userManager.changeStatus(user.id, "blocked");
                            }
                            return RedirectToAction("Index", new { whatToDo = 3 });
                        }
                        if (bulkOptions == "Remove block")
                        {
                            foreach (var email in words)
                            {
                                Users user = repUsers.getUserByEmail(email);
                                if (user.status == "blocked" && user.id != (int)Session["IDUSER"])
                                    userManager.changeStatus(user.id, "active");
                            }
                            return RedirectToAction("Index", new { whatToDo = 4 });
                        }
                    }
                }
                return RedirectToAction("Index", new { tab = 1 });
            }
            catch
            {
                return RedirectToAction("Index", new { whatToDo = 0 });
            }
        }


        [SessionExpireFilter]
        [HttpPost]
        [Authorize(Roles = "GTP")]
        public ActionResult EditGTPUser(Users user, HttpPostedFileBase file, string showInGallery, string showOnlyToCustomers, string admin, string notifTechnical, string notifClaims, string notifSamples)
        {
            try
            {
                // validation email and name exists, and email is not repeated
                if (user.email == null || user.firstName == null)
                {
                    ModelState.AddModelError("email", "Email address either taken or missing");
                    return View(user);
                }
                else {
                    Users existeMail = repUsers.getUserByEmail(user.email);
                    if (existeMail != null)
                    {
                        Users theUser = repUsers.Get(user.id);
                        if (existeMail.email != theUser.email)
                        {
                            ModelState.AddModelError("email", "Email address either taken or missing");
                            return View(user);
                        }
                    }
                } 
                if (file != null && file.ContentLength > 0)
                {
                    if (file.ContentLength > 20000000)
                    {
                        ModelState.AddModelError("", "File is too big. Max size allowed 20 MB");
                        return View(user);
                    }
                    var fileName = Convert.ToString(user.firstName) + "__" + DateTime.UtcNow.Ticks.ToString() + "-" + System.IO.Path.GetFileName(file.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    file.SaveAs(path);
                    user.image = fileName;
                }
                if (showInGallery == "yes") user.showInGallery = true;
                else user.showInGallery = false;
                if (showOnlyToCustomers == "yes") user.showOnlyToCustomers = true;
                else user.showOnlyToCustomers = false;

                if (!Roles.RoleExists("KAM"))
                    // Create the role
                    Roles.CreateRole("KAM");

                if (admin != "yes")
                {
                    if (!Roles.IsUserInRole(user.email, "KAM"))
                        securityMgr.AddUserToKAMRole(user.email);
                    user.administrator = false;
                }
                if (admin == "yes")
                {
                    if (Roles.IsUserInRole(user.email, "KAM"))
                        securityMgr.DeleteUserFromKAMRole(user.email);
                    user.administrator = true;
                }

                if (notifTechnical == "yes") user.notifTechnical = true;
                else user.notifTechnical = false;
                if (notifClaims == "yes") user.notifClaims = true;
                else user.notifClaims = false;
                if (notifSamples == "yes") user.notifSamples = true;
                else user.notifSamples = false;

                repUsers.Edit(user);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(user);
            }
        }

        [SessionExpireFilter]
        public ActionResult Delete(int id)
        {
            Users user = repUsers.Get(id);
            return View(user);
        }

        [SessionExpireFilter]
        [HttpPost]
        [Authorize(Roles = "GTP")]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                Users user = repUsers.Get(id);
                repUsers.Delete(user);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        /******************* CHECK IF NEEDED ********************/
        public ActionResult ResetPassword(int id)
        {
            Users user = repUsers.Get(id);
            try
            {
                MembershipUser mu = Membership.GetUser(user.email);
                if (mu.IsLockedOut) mu.UnlockUser();
                else mu.ChangePassword(mu.ResetPassword(), "GTPToolBox2015");
                emailMgr.emailResetPassword(user, user.email, "GTPToolBox2015");
                if (user.idTenant == 1) return RedirectToAction("Index", new { id = user.idTenant, whatToDo = 1 });
                return RedirectToAction("IndexByTenant", new { id = user.idTenant });
            }
            catch (Exception ex)
            {
                return RedirectToAction("IndexByTenant", new { id = user.idTenant , msg = ex.Message });
            }
        }
/********** CHECK IF USED ********/
        public ActionResult resendPassword(int id) // used for GTP user admin
        {
            using (repoUsers repUser = new repoUsers())
            {
                Users user = repUser.Get(id);
                if (user != null)
                {
                    using (SecurityMgr secMgr = new SecurityMgr())
                    {
                        ViewBag.idTenant = user.idTenant;
                        if (secMgr.setActivationLink(user.email, 2))
                            return View();
                        else return View();
                    }
                }
                else return View();
            }
        }

        public ActionResult sendActivationLink(int id) // used for GTP user admin
        {
            using (repoUsers repUser = new repoUsers())
            {
                Users user = repUser.Get(id);
                if (user != null)
                {
                    using (SecurityMgr secMgr = new SecurityMgr())
                    {
                        ViewBag.idTenant = user.idTenant;
                        secMgr.setActivationLink(user.email, 2);
                    }
                }
                return RedirectToAction("Edit", new { id = id, whatToDo = 1 });
            }
        }

        public string resendPasswordByEmail(string email) // used for any user on the signIn page. Called via ajax
        {
            using (repoUsers repUser = new repoUsers())
            {
                Users user = repUser.getUserByEmail(email);
                if (user != null)
                {
                    using (SecurityMgr secMgr = new SecurityMgr())
                    {
                        ViewBag.idTenant = user.idTenant;
                        if (secMgr.setActivationLink(user.email, 2))
                            return "1";
                        else return "No user found, please contact GTP";
                    }
                }
                else return "No user found, please contact GTP";
            }
        }

        public ActionResult setPassword(string token)
        {
            ViewBag.HideUserPanel = !(System.Web.HttpContext.Current.User.Identity.IsAuthenticated && Session["IDUSER"] != null);
            using (SecurityMgr secMgr = new SecurityMgr())
            {
                string errorMsg;
                int value = secMgr.processActivationLink(token);
                if (value > 0)
                {
                    ViewBag.id = value;
                    return View();
                }
                else
                {
                    errorMsg = "Undefined Error";
                    if (value == -2) errorMsg = "No user found, please contact GTP";
                    if (value == -1) errorMsg = "This activation link is expired. You can request a new one <a href='http://www.gtptoolbox.com'>here</a>";
                    if (value == -3) errorMsg = "This activation link has been used already. If you want to create an account please go to register page or <a href='mailto:toolbox@gtp-schaefer.de'>contact GTP</a>";
                    if (value == -4) errorMsg = "Account not active. Please contact GTP";
                    ViewBag.errorMsg = errorMsg;
                    //return View("ErrorActivatingLink");
                    ModelState.AddModelError("", errorMsg);
                    return View();
                }
            }
        }

        [LogFilter]
        [HttpPost]
        public ActionResult setPassword(string id, ChangePasswordModel model)
        {
            try
            {
                using (repoUsers repUser = new repoUsers())
                {
                    Users user = repUser.Get(int.Parse(id));
                    if (user != null && user.status != "blocked")
                    {
                        MembershipUser mu = Membership.GetUser(user.email);
                        if (mu.IsLockedOut) mu.UnlockUser();
                        else mu.ChangePassword(mu.ResetPassword(), model.NewPassword);
                        user.status = "active";
                        repUser.Edit(user);
                        
                        using (repoCustomers repCustomers = new repoCustomers())
                        {
                            Session["IDTENANT"] = user.idTenant;
                            Session["IDUSER"] = user.id;
                            Session["USERNAME"] = user.firstName + " " +user.lastName;
                            if (user.businessRole == null) Session["BUSINESSROLE"] = "Unassigned";
                            else Session["BUSINESSROLE"] = user.businessRole;
                            Session["PERMADMIN"] = user.generalAdmin;
                            Session["PERMSEEPUBLICCATALOG"] = user.seePublicCatalog;
                            Session["PERMSEEDRAFTPRODUCTS"] = user.seeDraftProducts;
                            Session["PERMSEECUSTOMPRODUCTS"] = user.seeCustomProducts;
                            Session["PERMDOWNLOAD2D"] = user.download2d;
                            Session["PERMSDOWNLOAD3D"] = user.download3d;
                            Session["PERMDOWNLOADCUSTOMDOCUMENTS"] = user.downloadCustomDocuments;
                            Session["PERMDOWNLOADINTERNALFILES"] = user.downloadInternalFiles;
                            Session["PERMDOWNLOADPUBLICDOCUMENTS"] = user.downloadPublicDocuments;
                            Session["PERMASSIGNCUSTOMERSTOPRODUCTS"] = user.assignCustomersToProducts;
                            Session["PERMACCESSTICKETS"] = user.accessTickets;
                            Session["PERMACCESSCALC"] = user.accessCalculator;
                            Session["PERMDOCUMENTS"] = user.downloadPublicDocuments;
                            
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
                            FormsAuthentication.SetAuthCookie(user.email, true);                        
                        }
                       // return RedirectToAction("IndexItems", "Series", new { alert = 3 });  // end of automaticaly login
                        return RedirectToAction("Index", "Catalog", new { alert = 3 });
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            // If we got this far, something failed, redisplay form
            ViewBag.id = id;
            return View(model);
        }

        [SessionExpireFilter]
        public ActionResult ShowInGallery(int id)
        {
            Users user = repUsers.Get(id);
            user.showInGallery = true;
            repUsers.Edit(user);
            if (user.isKAM == true) return View(user);
            else return RedirectToAction("Index");
        }

        [SessionExpireFilter]
        public ActionResult DoNotShowInGallery(int id)
        {
            Users user = repUsers.Get(id);
            user.showInGallery = false;
            repUsers.Edit(user);
            return RedirectToAction("Index");
        }

        [SessionExpireFilter]
        public ActionResult KamShowedOnlyToHisCustomers(int id, bool answer)
        {
            Users user = repUsers.Get(id);
            user.showOnlyToCustomers = true;
            repUsers.Edit(user);
            return RedirectToAction("Index");
        }

        public ActionResult DeleteAll()
        {
            repUsers.DeleteAll();
            return RedirectToAction("Index", "Items");
        }

        public ActionResult SetCleanInstallation()
        {
            repUsers.SetCleanInstallation();
            return RedirectToAction("Index", "Items");
        }

        public ActionResult EditSuccess()
        {
            return View();
        }

        public ActionResult Welcome()
        {
            return View();
        }
/**************** CHECK IF USED ****************/
        [SessionExpireFilter]
        public ActionResult Block(int id)
        {
            using (repoUsers repUsers = new repoUsers())
            {
                Users user = repUsers.Get(id);
                return View(user);
            }
        }
        
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Block(int id, FormCollection collection)
        {
            try
            {
                using (UsersManager userManager = new UsersManager())
                {
                    userManager.changeStatus(id, "blocked"); ;
                    return RedirectToAction("Index" );
                }
            }
            catch
            {
                return View();
            }
        }

/**********************************************************/
        [SessionExpireFilter]
        public ActionResult changeStatus(int id, string status)
        {
            using (UsersManager userManager = new UsersManager())
            {
                userManager.changeStatus(id, status); ;
                return RedirectToAction("Edit", new { id = id });
            }            
        }


        public ActionResult usersManager(int? whatToDo, string status, string customers, string businessRole, string search) // NEW
        {
            if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                try
                {
                    var c1 = ControllerContext.HttpContext.Request.Cookies["GTPTRACKER1"];
                    if (c1 == null) return RedirectToAction("signIn", "Account");
                    string cryptedIDUSER = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes("IDUSER"));
                    int idUser = Convert.ToInt32(Functions.AES.AesDecryption(c1.Values[cryptedIDUSER]));
                    Session["IDUSER"] = idUser;
                    Session["TENANT"] = Functions.AES.AesDecryption(c1.Values["T"]);
                    Users currentUser = repUsers.Get(idUser);
                    Session["USERNAME"] = currentUser.firstName + " " + currentUser.lastName;
                    ViewBag.idUser = (int)Session["IDUSER"];
                    string password = (string)Functions.AES.AesDecryption(c1.Values["C"]);

                    if (Membership.ValidateUser(currentUser.email, password))
                    {
                        FormsAuthentication.SetAuthCookie(currentUser.firstName + " " + currentUser.lastName, true);
                    }
                    else return RedirectToAction("Index", "Home");

                    Users user = repUsers.Get(idUser);
                    // if (user == null) user = repUsers.getUserByEmail(model.UserName); // sometimes the email is disabled, so use the name
                    if (user != null)
                    {
                        if (user.status == "blocked")
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        if (user.status == "non-active")
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        if (user.generalAdmin != true)
                        {
                            return RedirectToAction("Index", "Home");
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
                        }
                    }
                }
                catch 
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.action = whatToDo;
            using (repoCustomers repCustomers = new repoCustomers())
            {
                ViewBag.lTenants = repCustomers.getList().OrderBy(p => p.name);
            }

            ViewBag.status = status;
            ViewBag.customers = customers;
            ViewBag.businessRole = businessRole;
            ViewBag.search = search;
            IEnumerable<vUsers> result = repUsers.getvList().OrderByDescending(p => p.id);
            if (status != "All status" && status != null) result = result.Where(p => p.status == status);
            if (customers != "All customers" && customers != null) result = result.Where(p => p.tenantName == customers);
            if (businessRole != "All roles" && businessRole != null) result = result.Where(p => p.businessRole == businessRole);
            /*if (search != null && search != "") result = result.Where(p => p.firstName.Contains("/" + search + "/") || (p.lastName != null) && p.lastName.Contains("/" + search + "/")); /*|| p.businessRole.Contains("/" + search + "/")
                || p.company.Contains("/" + search + "/") || p.status.Contains("/" + search + "/"));*/
            return View(result);
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
