using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GTPTracker.Helpers;
using GTPTracker.VM;
using Crunching.Models;
using GTPTracker.repos;
using System.Diagnostics;


namespace GTPTracker.Controllers
{
    public class DocumentsController : Controller
    {        
        [LogFilter]
        public string getCustomersFromProducts(String productsList, String customerList)
        {
            string resultTxt = "";
            List<Customers> result = new List<Customers>();
            List<Customers> originCustomers = new List<Customers>();
            List<Customers> originProducts = new List<Customers>();
            List<int> idCustomersList = new List<int>();
            try
            {
                using (var db = new GTPTrackerEntities())
                {
                    char[] delimiterChars = { ';' };

                    string[] customersOriginCustomer = customerList.Split(delimiterChars);
                    if (customersOriginCustomer.Count() > 0)
                    {
                        foreach (var customer in customersOriginCustomer) // step 1. get the customersID assigned to that products, but dont put them on resultTxt yet. Wait for the customer/product
                        {
                            if (customer != "")
                            {
                                int parsingResult;
                                bool isInt = Int32.TryParse(customer, out parsingResult);
                                if (isInt)
                                    originCustomers.Add(db.Customers.FirstOrDefault(p => p.id == parsingResult));
                                else
                                    originCustomers.Add(db.Customers.FirstOrDefault(p => p.name == customer));
                            }
                        }
                    }
                    char[] delimiterCharsForProducts = { ',' };
                    string[] words = productsList.Split(delimiterCharsForProducts);
                    if (words.Count() > 0)
                    {
                        foreach (var productType in words) // step 1. get the customersID assigned to that products
                        {
                            Products product = db.Products.FirstOrDefault(p => p.type == productType);
                            if (product != null)
                            {
                                IEnumerable<int> lCustomer = db.CustomerProducts.Where(p => p.idProduct == product.id).Select(p => p.idCustomer);
                                foreach (int idCustomer in lCustomer)
                                    idCustomersList.Add(idCustomer);
                            }
                        }
                        foreach (var idCustomer in idCustomersList) // step 2. Get the product info
                        {
                            Customers customer = db.Customers.FirstOrDefault(p => p.id == idCustomer);
                            if (customer != null)
                            {
                                if (originProducts.Find(p => p.id == idCustomer) == null) // if duplicate from products dont add it.
                                    originProducts.Add(customer);                                
                            }
                        }
                    }
                    originCustomers = originCustomers.Distinct().ToList();
                    originProducts = originProducts.Distinct().ToList();

                    if (originCustomers.Count() > 0 || originProducts.Count > 0)
                    {
                        resultTxt = "<b>Reason List</b></br>The document is visible to all customers in this list</br><table id=\"reasonsList\" class=\"table\"><thead><th>Name</th><th>Reason</th></thead><tbody>";
                        foreach (var customer in originCustomers) // step 3. 
                        {
                            if (originProducts.Find(p => p.id == customer.id) == null)
                                resultTxt += "<tr><td>" + customer.name + "</td><td>Customer</td></tr>";
                            else resultTxt += "<tr><td>" + customer.name + "</td><td>Customer / Product</td></tr>";
                        }
                        foreach (var customer in originProducts) // step 3. 
                        {
                            if (originCustomers.Find(p => p.id == customer.id) == null)
                                resultTxt += "<tr><td>" + customer.name + "</td><td>Product</td></tr>";
                        }
                        resultTxt += "</tbody></table>";
                    }
                    else resultTxt = "Custom document need to be associated with at least one customer. Please associate through products or associate a customer. Otherwise pick another status for the document.";
                }
                
                return resultTxt;
            }
            catch 
            { return "0";}
        }

        public ActionResult PartialView(int? idTenant, int? idCategory, bool showOnlyMyDocuments)
        {
            bool showOnlyInternals = false;
            bool isCustomer = false;
            bool adminSimulatingCustomer = false;
            DateTime ini = DateTime.Now;
            using (repoGeneralDocs repDocs = new repoGeneralDocs())
            {
                // the list has a field called category to be grouped by. Also is filtered by the idCustomer
                IEnumerable<vGenDocs> list = repDocs.getvList();
                List<vGenDocs> listCustomDocs = new List<vGenDocs>();
                IEnumerable<vGenDocs> listAllPublicDocs = Enumerable.Empty<vGenDocs>();
                IEnumerable<vGenDocs> listAllInternalDocs = Enumerable.Empty<vGenDocs>();
                bool canDownPublicDocs = false;
                int idCustomer;

                string businessRole = (string)Session["BUSINESSROLE"];
                if (businessRole.TrimEnd().TrimStart() == "Customer")
                    isCustomer = true;
                ViewBag.isCustomer = isCustomer;
                if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated == false || Session["IDUSER"] == null) // user visitor. Can see public docs only
                    canDownPublicDocs = true;
                else // authenticated user. Read the values from session (set when login in)                
                    canDownPublicDocs = (bool)Session["PERMDOCUMENTS"];                

                //There is no internal docs permission, so show based on business role

                if ((int)Session["IDTENANT"] == 0) // GTP user
                    listAllInternalDocs = list.Where(p => p.status == "internal");
                if (canDownPublicDocs == true)
                {
                    listAllPublicDocs = list.Where(p => p.status == "public");

                    bool showMeAll = false;
                    if (idTenant == null)
                    {
                        idCustomer = (int)Session["IDTENANT"]; // I'm a person with the business role customer
                        if (idCustomer == 0) showMeAll = true;
                    }
                    else
                    {
                        if (idTenant == 0) // I'm an admin using the dropdown to show only internal docs
                        {
                            showOnlyInternals = true;
                            idCustomer = (int)Session["IDTENANT"]; // to avoid an error I get all the docs
                        }
                        else
                        {
                            idCustomer = (int)idTenant; // I'm an admin using the dropdown to select a tenant
                            adminSimulatingCustomer = true;
                        }
                    }
                    // 1st step. Get all the docs with origin customer
                    if (showMeAll == false)
                    {
                        IEnumerable<GenDocsCustomers> lOriginCustomer = repDocs.GetDocByCustomer(idCustomer);
                        foreach (var docCustomer in lOriginCustomer)
                        {
                            vGenDocs doc = repDocs.Getv(docCustomer.idGenDoc);
                            listCustomDocs.Add(doc);
                        }
                        // 2nd step. Get all the docs with origin product.
                        //2.1 Get all products assigned to this idCustomer
                        List<vDocsProducts> lOriginProduct = new List<vDocsProducts>();
                        using (repoProducts repProducts = new repoProducts())
                        {
                            IEnumerable<vProducts> lProducts = repProducts.getvList(idCustomer);
                            foreach (var product in lProducts)
                            {
                                IEnumerable<vDocsProducts> lDocsByProducts = repDocs.GetDocsByProducts(product.id);
                                foreach (var docProduct in lDocsByProducts)
                                    lOriginProduct.Add(docProduct);
                            }
                            foreach (var docProduct in lOriginProduct)
                            {
                                vGenDocs doc = repDocs.Getv(docProduct.idDoc);
                                if (listCustomDocs.Find(p => p.id == docProduct.idDoc) == null)
                                    listCustomDocs.Add(doc);
                            }
                            // 3rd step. concat and remove duplicates. Be sure the status is custom
                            listCustomDocs = listCustomDocs.Where(p => p != null && p.status == "custom").Distinct().ToList();
                        }
                    }
                    else listCustomDocs = list.Where(p => p.status == "custom").ToList();
                }
                if (adminSimulatingCustomer)
                    list = listAllPublicDocs.Concat(listCustomDocs);
                else list = listAllPublicDocs.Concat(listCustomDocs).Concat(listAllInternalDocs);
                if (showOnlyInternals) list = list.Where(p => p.status == "internal");
                if (idCategory != null) list = list.Where(p => p.category == idCategory);

                if (showOnlyMyDocuments == true && isCustomer == true) list = list.Where(p => p.status == "custom");
                // case user unassigned or other. Show only public documents
                if (businessRole.TrimEnd().TrimStart() == "Unassigned" || businessRole.TrimEnd().TrimStart() == "GTP Team")
                    list = list.Where(p => p.status == "public");
                list = list.OrderBy(p => p.filename);
                Debug.WriteLine("Time for getting the docs = {0} ", DateTime.Now - ini);
                return View(list);
            }
        }

        [LogFilter]
        [SessionExpireFilter]
        public ActionResult Index(int? idTenant, int? idCategory, string showOnlyMyDocuments)
        {
            Debug.WriteLine("Documents.Index = {0} ", DateTime.Now);
            if (showOnlyMyDocuments == "on") ViewBag.showOnlyMyDocuments = true;
            else ViewBag.showOnlyMyDocuments = false;
            bool isCustomer = false;
            string businessRole = (string)Session["BUSINESSROLE"];
            if (businessRole.TrimEnd().TrimStart() == "Customer")
                isCustomer = true;
            ViewBag.isCustomer = isCustomer;
            using (repoGeneralDocs repDocs = new repoGeneralDocs())
            using (repoCustomers repCustomers = new repoCustomers())
            {
                List<vCustomers> lCustomers = new List<vCustomers>();
                vCustomers showInternal = new vCustomers();
                showInternal.id = 0;
                showInternal.name = "internal documents";
                lCustomers.Add(showInternal);
                foreach (vCustomers vCustomer in repCustomers.getList())
                    lCustomers.Add(vCustomer);

                ViewBag.lTenants = lCustomers;

                IEnumerable<GenDocCategories> lCategories = repDocs.getCategories();
                ViewBag.lCategories = lCategories;
                ViewBag.idCategory = idCategory;
            }
            ViewBag.tenant = idTenant;
            if (Session["BUSINESSROLE"] != null)
            {
                string role = ((string)Session["BUSINESSROLE"]).TrimStart().TrimEnd();
                if (role == "GTP General Manager" || role == "GTP KAM")
                    ViewBag.showTenantsList = true;
                if (role == "GTP KAM")
                { // get only my customers
                    using (repoCustomers repCustomers = new repoCustomers())
                        ViewBag.lTenants = repCustomers.getvByKAM((int)Session["IDUSER"]).OrderBy(p => p.name);
                }
            }
            ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
            return View();
        }

        [SessionExpireFilter]
        public ActionResult manageDocuments(string category, string bulkAction, string status, string whatToDo, int tab = 1)
        {
            ManageDocumentsVM vm = new ManageDocumentsVM(category, bulkAction, status, whatToDo, tab);
            return View(vm);
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
                    using (repoGeneralDocs repDocs = new repoGeneralDocs())
                    {
                        if (bulkOptions == "Delete document")
                        {
                            foreach (var id in words)
                            {
                                GeneralProductsDocuments doc = repDocs.Get(int.Parse(id));
                                if (doc != null)
                                    repDocs.Delete(doc);
                            }
                            return RedirectToAction("manageDocuments", new { tab = 1, whatToDo = 2 });
                        }
                        
                    }
                }
                return RedirectToAction("manageDocuments", new { tab = 1 });
            }
            catch
            {
                return RedirectToAction("manageDocuments", new { whatToDo = 0 });
            }
        }
        
        //
        // GET: /Documents/Create

        public ActionResult Create()
        {
            DocumentsEditAddVM vm = new DocumentsEditAddVM(null);
            return View(vm);
        } 

        //
        // POST: /Documents/Create

        [HttpPost]
        public ActionResult Create(GeneralProductsDocuments doc, HttpPostedFileBase file)
        {
            try
            {
                using (repoGeneralDocs repDocs = new repoGeneralDocs())
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = "Doc_" + System.IO.Path.GetFileName(file.FileName);
                        var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                        file.SaveAs(path);
                        doc.URI = fileName;
                    }
                    else
                    {
                        ModelState.AddModelError("", "You have not uploaded any file");
                        DocumentsEditAddVM vm = new DocumentsEditAddVM(null);
                        return View(vm);
                    }

                    if (doc.filename == "" || doc.filename == null)
                    {
                        ModelState.AddModelError("", "Name is a mandatory field");
                        DocumentsEditAddVM vm = new DocumentsEditAddVM(null);
                        return View(vm);
                    }
                    int id = repDocs.Create(doc);

                    string[] assignedCustomers;
                    if (!string.IsNullOrEmpty(Request.Params["assignedCustomers"]))
                        assignedCustomers = Request.Params["assignedCustomers"].Split(',');
                    else
                        assignedCustomers = new string[0];
                    foreach (var item in assignedCustomers)
                    {
                        int result;
                        bool isInt = Int32.TryParse(item, out result);
                        if (isInt)
                            repDocs.AssignToCustomer(id, Int32.Parse(item));
                        else
                        {
                            using (repoCustomers repCustomers = new repoCustomers())
                            {
                                Customers customer = repCustomers.GetByName(item.TrimStart().TrimEnd());
                                repDocs.AssignToCustomer(id, customer.id);
                            }
                        }
                    }

                    string[] assignedProducts;
                    if (!string.IsNullOrEmpty(Request.Params["assignedProducts"]))
                        assignedProducts = Request.Params["assignedProducts"].Split(',');
                    else
                        assignedProducts = new string[0];
                    foreach (var item in assignedProducts)
                    {
                        int result;
                        bool isInt = Int32.TryParse(item, out result);
                        if (isInt)
                            repDocs.AssignDocLinkToProduct(id, Int32.Parse(item));
                        else
                        {
                            using (repoProducts repProducts = new repoProducts())
                            {
                                Products product = repProducts.GetByType(item.TrimStart().TrimEnd());
                                repDocs.AssignDocLinkToProduct(id, product.id);
                            }
                        }
                    }
                    
                }
                return RedirectToAction("ManageDocuments", new { whatTodo = 1, tab = 1 });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                DocumentsEditAddVM vm = new DocumentsEditAddVM(null);
                return View(vm);
            }
        }
        
        
 
        public ActionResult Edit(int id)
        {
            DocumentsEditAddVM vm = new DocumentsEditAddVM(id);
            return View(vm);
        }

        [HttpPost]
        public ActionResult Edit(int id, GeneralProductsDocuments doc, HttpPostedFileBase file)
        {
            try
            {
                using (repoGeneralDocs repDocs = new repoGeneralDocs())
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = "_GeneralProductDoc_" + DateTime.UtcNow.Ticks.ToString() + "-" + System.IO.Path.GetFileName(file.FileName);
                        var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                        file.SaveAs(path);
                        doc.URI = fileName;
                    }

                    if (doc.filename == "" || doc.filename == null)
                    {
                        ModelState.AddModelError("", "Name is a mandatory field");
                        DocumentsEditAddVM vm = new DocumentsEditAddVM(null);
                        return View(vm);
                    }

                    string[] assignedCustomers;
                    if (!string.IsNullOrEmpty(Request.Params["assignedCustomers"]))
                        assignedCustomers = Request.Params["assignedCustomers"].Split(',');
                    else
                        assignedCustomers = new string[0];
                    // delete the previous assigned products on bd

                    repDocs.RemoveFromCustomer(doc.id);
                    foreach (var item in assignedCustomers)
                    {
                        int result;
                        bool isInt = Int32.TryParse(item, out result);
                        if (isInt)
                            repDocs.AssignToCustomer(doc.id, Int32.Parse(item));
                        else
                        {
                            using (repoCustomers repCustomers = new repoCustomers())
                            {
                                Customers customer = repCustomers.GetByName(item.TrimStart().TrimEnd());
                                repDocs.AssignToCustomer(doc.id, customer.id);
                            }
                        }
                    }

                    string[] assignedProducts;
                    if (!string.IsNullOrEmpty(Request.Params["assignedProducts"]))
                        assignedProducts = Request.Params["assignedProducts"].Split(',');
                    else
                        assignedProducts = new string[0];
                    // delete the previous assigned products on bd

                    repDocs.RemoveProductsLinkByDoc(doc.id);
                    foreach (var item in assignedProducts)
                    {
                        int result;
                        bool isInt = Int32.TryParse(item, out result);
                        if (isInt)
                            repDocs.AssignDocLinkToProduct(doc.id, Int32.Parse(item));
                        else
                        {
                            using (repoProducts repProducts = new repoProducts())
                            {
                                Products product = repProducts.GetByType(item.TrimStart().TrimEnd());
                                repDocs.AssignDocLinkToProduct(doc.id, product.id);
                            }
                        }
                    }
                    repDocs.Edit(doc);
                }                
                return RedirectToAction("ManageDocuments", new { whatTodo = 1, tab = 1 });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                DocumentsEditAddVM vm = new DocumentsEditAddVM(id);
                return View(vm);
            }
        }

        //
        // GET: /Documents/Delete/5
 
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Documents/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [SessionExpireFilter]
        public ActionResult CreateCategory()
        {
            using (repoGeneralDocs repDocs = new repoGeneralDocs())
            {
                GenDocCategories doc = new GenDocCategories();
                return View(doc);
            }
        }

        [SessionExpireFilter]
        [HttpPost]

        public ActionResult CreateCategory(string category)
        {
            try
            {
                using (repoGeneralDocs repDocs = new repoGeneralDocs())
                {
                    GenDocCategories docCategory = new GenDocCategories();
                    docCategory.category = category;
                    int idDoc = repDocs.CreateCategory(docCategory);
                    return RedirectToAction("ManageDocuments", "Documents", new { whatTodo = 3, tab = 2 });
                }
            }
            catch
            {
                return View();
            }
        }

        [SessionExpireFilter]
        public ActionResult EditCategory(int id)
        {
            using (repoGeneralDocs repDocs = new repoGeneralDocs())
            {
                GenDocCategories category = repDocs.GetCategory(id);
                return View(category);
            }
        }

        [SessionExpireFilter]
        [HttpPost]

        public ActionResult EditCategory(string category, int id)
        {
            try
            {
                using (repoGeneralDocs repDocs = new repoGeneralDocs())
                {
                    GenDocCategories docCategory = repDocs.GetCategory(id);
                    docCategory.category = category;
                    repDocs.EditCategory(docCategory);
                    return RedirectToAction("ManageDocuments", "Documents", new { whatTodo = 1, tab = 2 });
                }
            }
            catch
            {
                return View();
            }
        }

        [SessionExpireFilter]
        public ActionResult DeleteCategory(int id)
        {
            using (repoGeneralDocs repDocs = new repoGeneralDocs())            
                return View(repDocs.GetCategory(id));
        }

        [SessionExpireFilter]
        [HttpPost]

        public ActionResult DeleteCategory(GenDocCategories element)
        {
            try
            {
                using (repoGeneralDocs repDocs = new repoGeneralDocs())
                {
                    repDocs.DeleteCategory(element);
                    return RedirectToAction("ManageDocuments", "Documents", new { whatTodo = 4, tab = 2 });
                }
            }
            catch
            {
                return View();
            }
        }
        /* called in customers.edit */
        public JsonResult getDocsByCategory(int idCategory)
        {
            using (repoGeneralDocs repDocs = new repoGeneralDocs())
            {
                IEnumerable<GeneralProductsDocuments> result = repDocs.GetByCategory(idCategory).OrderBy(p=>p.filename);
                
                var json = result.Select(p => new { p.id, p.filename });

                JsonResult jsonResult = new JsonResult();
                jsonResult.Data = json;

                return jsonResult;
            }
        }
            
    }
}
