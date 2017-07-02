using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Crunching.Models;
using GTPTracker.repos;
using GTPTracker.Helpers;
using GTPTracker.VM;
using GTPTracker.BL;
using System.Diagnostics;

namespace GTPTracker.Controllers
{
    public class CustomersController : Controller
    {
        repoCustomers repCustomers = new repoCustomers();
        repoUsers repUsers = new repoUsers();

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
            using (repoCustomers repCustomers = new repoCustomers()) 
            {
                repCustomers.AcceptUserAgreement((int)Session["IDTENANT"]);
                Session["TERMSSIGNED"] = true;
            }
            return RedirectToAction("Index", "Home");
        }


        [SessionExpireFilter]
        public ActionResult ManageContacts(int idTenant)
        {
            ContactsVM vm = new ContactsVM(idTenant);
            return View(vm);
        }

        [SessionExpireFilter]
        public ActionResult setKeyAccountManager(int id)
        {
            Customers customer = repCustomers.Get(id);
            ViewBag.name = customer.name;
            ViewBag.idCustomer = id;
            ViewBag.currentIdKAM = customer.idKeyAccountManager;
            IEnumerable<Users> lUsers = repUsers.getGTPPeople();
            return View(lUsers);
        }

        [SessionExpireFilter]
        public ActionResult saveKeyAccountManager(int idcustomer, int idUser)
        {
            Customers customer = repCustomers.Get(idcustomer);
            customer.idKeyAccountManager = idUser;
            repCustomers.Edit(customer);
            Users user = repUsers.Get(idUser);
            user.isKAM = true;
            repUsers.Edit(user);
            return RedirectToAction("Index", "Customers");
        }

        [LogFilter]
        [SessionExpireFilter]
        public ActionResult Manager(int? whatToDo, string country, string KAM)
        {
            using (repoCustomers repCustomers = new repoCustomers())
            {
                ViewBag.action = whatToDo;
                ViewBag.lCountries = repCustomers.getCurrentCountries();
                ViewBag.lKAMs = repCustomers.getCurrentKAMs();
                ViewBag.country = country;
                ViewBag.KAM = KAM;
                IEnumerable<vCustomers> lCustomers = lCustomers = repCustomers.getList();
                if ((string)Session["BUSINESSROLE"] == "GTP KAM")
                {
                    IEnumerable<int> lMyCustomers = repCustomers.getvByKAM((int)Session["IDUSER"]).Select(p=>p.id);
                    lCustomers = lCustomers.Where(p => lMyCustomers.Contains(p.id));
                    ViewBag.lKAMs = new List<string>() { (string)Session["USERNAME"] };
                    ViewBag.KAM = (string)Session["USERNAME"];
                }
                if (country != null && country != "All countries") lCustomers = lCustomers.Where(p => p.CountryName == country);
                if (KAM != null && KAM != "All KAMs")
                {
                    lCustomers = lCustomers.Where(p => p.kamsNames != null);
                    lCustomers = lCustomers.Where(p => p.kamsNames.Contains(KAM));
                }
                return View(lCustomers.OrderByDescending(p=>p.cDate));
            }
        }

        [SessionExpireFilter]
        public ActionResult Index(int? whatToDo)
        {
            return RedirectToAction("Manager", new { whatToDo = whatToDo });
        }

        [LogFilter]
        [SessionExpireFilter]
        public ActionResult Create()
        {
            using (repoCustomers repCustomers = new repoCustomers())
            {
                ViewBag.lCountries = repCustomers.getCountries();
                Customers customer = new Customers();
                using (repoSeries repSeries = new repoSeries())
                using (repoProducts repProducts = new repoProducts())
                using (repoGeneralDocs repDocs = new repoGeneralDocs()) 
                {
                    ViewBag.lSeries = repSeries.getList().OrderBy(p=>p.text);
                    ViewBag.lCountries = repCustomers.getCountries();

                    ViewBag.lProducts = repProducts.getAll().OrderBy(p=>p.type);
                    using (repoUsers repUsers = new repoUsers())
                    {
                        IEnumerable<vUsers> lKAMs = repUsers.getKAMs();
                        ViewBag.lKAMs = lKAMs.OrderBy(p=>p.fullName);
                    }
                    IEnumerable<vDocsCustomers> lAssignedDocs = Enumerable.Empty<vDocsCustomers>();
                    IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                    IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                    ViewBag.lAssignedDocs = lAssignedDocs;
                    ViewBag.lDocsCategories = lDocsCategories.OrderBy(p=>p.category);
                    ViewBag.lDocs = lDocs.OrderBy(p=>p.filename);
                }

                return View(customer);
            }
        }

        [LogFilter]
        [HttpPost]
        [SessionExpireFilter]
        public ActionResult Create(Customers customer)
        {
            try
            {
                customer.cDate = DateTime.Today;
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var random = new Random();
                var result = new string(
                    Enumerable.Repeat(chars, 10)
                              .Select(s => s[random.Next(s.Length)])
                              .ToArray());
                customer.linkID = result;
                customer.cDate = DateTime.Today;

                customer = repCustomers.Create(customer);

                // documents
                string[] assignedDocs;
                if (!string.IsNullOrEmpty(Request.Params["assignedDocs"]))
                    assignedDocs = Request.Params["assignedDocs"].Split(',');
                else
                    assignedDocs = new string[0];
                // delete the previous assigned products on bd
                using (repoGeneralDocs repDocs = new repoGeneralDocs())
                {
                    repDocs.deleteByCustomer(customer.id);
                    foreach (var item in assignedDocs)
                    {
                        int resultParse;
                        bool isInt = Int32.TryParse(item, out resultParse);
                        if (isInt)
                            repDocs.AssignToCustomer(Int32.Parse(item), customer.id);
                        else
                        {
                            GeneralProductsDocuments doc = repDocs.GetByFilename(item.TrimStart().TrimEnd());
                            repDocs.AssignToCustomer(doc.id, customer.id);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(Request.Params["assignedProducts"]))
                {
                    var assignedProducts = Request.Params["assignedProducts"].Split(',');
                    // delete the previous assigned products on bd
                    using (repoProducts repProducts = new repoProducts())
                    {
                        repProducts.DeleteByTenant(customer.id);
                        foreach (var item in assignedProducts)
                        {
                            int resultInt;
                            bool isInt = Int32.TryParse(item, out resultInt);
                            if (isInt)
                                repProducts.addProductToTenant(Int32.Parse(item), customer.id, (int)Session["IDUSER"]);
                            else
                            {
                                Products product = repProducts.GetByType(item.TrimStart().TrimEnd());
                                repProducts.addProductToTenant(product.id, customer.id, (int)Session["IDUSER"]);
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(Request.Params["assignedKAMs"]))
                {
                    var assignedKAMs = Request.Params["assignedKAMs"].Split(',');
                    IEnumerable<vKAMs> lKAMs = repCustomers.getKAMsByTenant(customer.id);
                    foreach (var item in lKAMs)
                        repCustomers.DeleteKAMFromTenant(item.idKAM, customer.id);
                    foreach (var item in assignedKAMs)
                    {
                        int resultInt;
                        bool isInt = Int32.TryParse(item, out resultInt);
                        if (isInt)
                            repCustomers.AddKAMToTenant(customer.id, Int32.Parse(item));
                        else
                        {
                            vUsers kam = repUsers.getKAMByName(item.TrimStart().TrimEnd());
                            repCustomers.AddKAMToTenant(customer.id, kam.id);
                        }
                    }
                }                
                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.lCountries = repCustomers.getCountries();
                using (repoSeries repSeries = new repoSeries())
                using (repoProducts repProducts = new repoProducts())
                using (repoGeneralDocs repDocs = new repoGeneralDocs())  
                {
                    ViewBag.lSeries = repSeries.getList().OrderBy(p=>p.text);
                    ViewBag.lCountries = repCustomers.getCountries();

                    ViewBag.lProducts = repProducts.getAll().OrderBy(p=>p.type);
                    using (repoUsers repUsers = new repoUsers())
                    {
                        IEnumerable<vUsers> lKAMs = repUsers.getKAMs();
                        ViewBag.lKAMs = lKAMs.OrderBy(p=>p.fullName);
                    }
                    IEnumerable<vDocsCustomers> lAssignedDocs = Enumerable.Empty<vDocsCustomers>();
                    IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                    IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                    ViewBag.lAssignedDocs = lAssignedDocs;
                    ViewBag.lDocsCategories = lDocsCategories.OrderBy(p=>p.category);
                    ViewBag.lDocs = lDocs.OrderBy(p=>p.filename);
                }
                return View();
            }
        }

        [SessionExpireFilter]
        public ActionResult Edit(int id)
        {
            using (repoCustomers repCustomers = new repoCustomers())
            using (repoSeries repSeries = new repoSeries())  
            using (repoProducts repProducts = new repoProducts()) 
            using (repoGeneralDocs repDocs = new repoGeneralDocs())  
            {
                ViewBag.lSeries = repSeries.getList().OrderBy(p=>p.text);
                ViewBag.lCountries = repCustomers.getCountries();
                IEnumerable<Products> productsList = repProducts.getList(id);
                ViewBag.assignedProducts = productsList.OrderBy(p=>p.type);//productsList.Select(p => p.type);
                
                IEnumerable<int> lIdsAssignedProducts = productsList.Select(p => p.id);
                IEnumerable<Products> lProducts = repProducts.getAll().Where(p => !(lIdsAssignedProducts.Contains(p.id))).OrderBy(p=>p.type);
                ViewBag.lProducts = lProducts;
                IEnumerable<vKAMs> assignedKAMs = repCustomers.getKAMsByTenant(id).OrderBy(p=>p.KAMName);
                ViewBag.assignedKAMs = assignedKAMs;
                IEnumerable<int> lIdsAssignedKAMs = repCustomers.getKAMsByTenant(id).Select(p=>p.idKAM);
                using (repoUsers repUsers = new repoUsers())
                {
                    IEnumerable<vUsers> lKAMs = repUsers.getKAMs();
                    IEnumerable<vUsers> lKAMsNot = lKAMs.Where(p => !(lIdsAssignedKAMs.Contains(p.id)));
                    ViewBag.lKAMs = lKAMsNot.OrderBy(p=>p.fullName);
                }

                IEnumerable<vDocsCustomers> lAssignedDocs = repDocs.GetvDocByCustomer(id);
                IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList().OrderBy(p=>p.filename);

                ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p=>p.filename);
                ViewBag.lDocsCategories = lDocsCategories.OrderBy(p=>p.category);
                ViewBag.lDocs = lDocs;

                Customers customer = repCustomers.Get(id);

                Helpers.LogMgr logMgr = new LogMgr();
                logMgr.Log("GET " + string.Join(",", assignedKAMs.Select(p=>p.KAMName).ToArray()));
                return View(customer);
            }
        }

        [LogFilter]
        [HttpPost]
        [SessionExpireFilter]
        public ActionResult Edit(Customers customer)
        {
            try
            {
                Helpers.LogMgr logMgr = new LogMgr();
                logMgr.Log("POST " + Request.Params["assignedKAMs"].ToString());
                using (repoCustomers repCustomers = new repoCustomers())
                {
                    repCustomers.Edit(customer);
                }

                // documents
                string[] assignedDocs;
                if (!string.IsNullOrEmpty(Request.Params["assignedDocs"]))
                    assignedDocs = Request.Params["assignedDocs"].Split(',');
                else
                    assignedDocs = new string[0];
                // delete the previous assigned products on bd
                using (repoGeneralDocs repDocs = new repoGeneralDocs())
                {
                    repDocs.deleteByCustomer(customer.id);
                    foreach (var item in assignedDocs)
                    {
                        int result;
                        bool isInt = Int32.TryParse(item, out result);
                        if (isInt)
                            repDocs.AssignToCustomer(Int32.Parse(item), customer.id);
                        else
                        {
                            GeneralProductsDocuments doc = repDocs.GetByFilename(item.TrimEnd());
                            if (doc != null)
                                repDocs.AssignToCustomer(doc.id, customer.id);
                        }
                    }
                }

                // products
                string[] assignedProducts;
                if (!string.IsNullOrEmpty(Request.Params["assignedProducts"]))
                    assignedProducts = Request.Params["assignedProducts"].Split(',');
                else 
                    assignedProducts = new string[0];
                    // delete the previous assigned products on bd
                using (repoProducts repProducts = new repoProducts())
                {
                    repProducts.DeleteByTenant(customer.id);
                    foreach (var item in assignedProducts)
                    {
                        int result;
                        bool isInt = Int32.TryParse(item, out result);
                        if (isInt)
                            repProducts.addProductToTenant(Int32.Parse(item), customer.id, (int)Session["IDUSER"]);
                        else
                        {
                            var itemString = item.TrimEnd();
                            Products product = repProducts.GetByType(itemString);
                            if (product != null)
                                repProducts.addProductToTenant(product.id, customer.id, (int)Session["IDUSER"]);
                            else Debug.WriteLine("Product not found " + itemString);
                        }
                    }
                }

                string businessRole = (string)Session["BUSINESSROLE"];
                if (businessRole != "GTP KAM")
                {
                    string[] assignedKAMs;
                    if (!string.IsNullOrEmpty(Request.Params["assignedKAMs"]))
                    {
                        assignedKAMs = Request.Params["assignedKAMs"].Split(',');
                    }
                    else
                        assignedKAMs = new string[0];
                    using (repoCustomers repCustomers = new repoCustomers())
                    {
                        IEnumerable<vKAMs> lKAMs = repCustomers.getKAMsByTenant(customer.id);
                        foreach (var item in lKAMs)
                            repCustomers.DeleteKAMFromTenant(item.idKAM, customer.id);
                        foreach (string item in assignedKAMs)
                        {
                            int result;
                            bool isInt = Int32.TryParse(item, out result);
                            if (isInt)
                                repCustomers.AddKAMToTenant(customer.id, Int32.Parse(item));
                            else
                            {
                                vUsers kam = repUsers.getKAMByName(item.TrimStart().TrimEnd());
                                if (kam != null)
                                    repCustomers.AddKAMToTenant(customer.id, kam.id);
                                else Debug.WriteLine("KAM not found " + item.TrimStart().TrimEnd());
                            }
                        }
                    }
                }           

                return RedirectToAction("Manager", new { whatToDo = 1 });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                using (repoCustomers repCustomers = new repoCustomers())
                using (repoSeries repSeries = new repoSeries())
                using (repoProducts repProducts = new repoProducts())
                using (repoGeneralDocs repDocs = new repoGeneralDocs())  
                {
                    ViewBag.lSeries = repSeries.getList().OrderBy(p=>p.text);
                    ViewBag.lCountries = repCustomers.getCountries();
                    IEnumerable<Products> productsList = repProducts.getList(customer.id);
                    ViewBag.assignedProducts = productsList.OrderBy(p=>p.type);

                    IEnumerable<int> lIdsAssignedProducts = productsList.Select(p => p.id);
                    ViewBag.lProducts = repProducts.getAll().Where(p => !(lIdsAssignedProducts.Contains(p.id))).OrderBy(p=>p.type);

                    ViewBag.assignedKAMs = repCustomers.getKAMsByTenant(customer.id).OrderBy(p=>p.KAMName);
                    IEnumerable<int> lIdsAssignedKAMs = repCustomers.getKAMsByTenant(customer.id).Select(p => p.idKAM);
                    using (repoUsers repUsers = new repoUsers())
                    {
                        IEnumerable<vUsers> lKAMs = repUsers.getKAMs();
                        ViewBag.lKAMs = lKAMs.Where(p => !(lIdsAssignedKAMs.Contains(p.id))).OrderBy(p=>p.fullName);
                    }

                    IEnumerable<vDocsCustomers> lAssignedDocs = repDocs.GetvDocByCustomer(customer.id).OrderBy(p=>p.filename);
                    IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories().OrderBy(p=>p.category);
                    IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList().OrderBy(p=>p.filename);

                    ViewBag.lAssignedDocs = lAssignedDocs;
                    ViewBag.lDocsCategories = lDocsCategories;
                    ViewBag.lDocs = lDocs;

                    return View(customer);
                }
            }
        }
        /* called via ajax on Edit Document. Returns html with a table */
        public JsonResult getAllProducts(string idSerie)
        {
            // TODO Filter depending on the user. So a KAM should not see draft products
            int id = Int32.Parse(idSerie);
            using (BL.UsersManager userManager = new UsersManager())
            {
                IEnumerable<Products> result = userManager.getMyProducts((int)Session["IDUSER"]).Where(p => p.idSerie == id).OrderBy(p=>p.type).ToList();  //db.Products.Where(p=>p.idSerie == id).ToList();
                var json = result.Select(p => new { p.id, p.type });

                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = json;

                return jsonResult;
            }
        }
        
        [LogFilter]
        [SessionExpireFilter]
        public ActionResult applyBulkActions(String customersList, string bulkOptions, string deleteUsersToo, string country, string KAM)
        {
            try
            {
                char[] delimiterChars = { ';' };

                string[] words = customersList.Split(delimiterChars);
                if (words.Count() > 0)
                {
                    using (repoCustomers repCustomers = new repoCustomers())
                    using (repoUsers repUsers = new repoUsers())
                    using (UsersManager userManager = new UsersManager())
                    {
                        if (bulkOptions == "Delete customer")
                        {
                            foreach (var customerName in words)
                            {
                                Customers customer = repCustomers.GetByName(customerName);
                                if (customer != null)
                                {
                                    IEnumerable<Users> lUsers = repUsers.getListByTenant(customer.id);
                                    foreach (var user in lUsers)
                                    {
                                        if (user.id != (int)Session["IDUSER"])
                                            if (deleteUsersToo == "0")
                                            {
                                                userManager.changeStatus(user.id, "blocked");
                                                userManager.AssignRole("GTP Team", user);
                                            }
                                            else userManager.Delete(user.email);
                                    }
                                    repCustomers.Delete(customer);
                                }
                            }
                            return RedirectToAction("Manager", new { whatToDo = 2, country = country, KAM = KAM });
                        }
                    }
                }
                return RedirectToAction("Manager", new { tab = 1, country = country, KAM = KAM });
            }
            catch
            {
                return RedirectToAction("Manager", new { whatToDo = 0, country = country, KAM = KAM });
            }
        }


        [SessionExpireFilter]
        public ActionResult Delete(int id)
        {
            Customers customer = repCustomers.Get(id);
            return View(customer);
        }

        [HttpPost]
        [SessionExpireFilter]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                Customers customer = repCustomers.Get(id);
                repCustomers.Delete(customer);
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
