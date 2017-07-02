using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Crunching.Models;
using GTPTracker.repos;
using GTPTracker.VM;
using GTPTracker.Helpers;
using System.IO;
using System.Web.UI.WebControls;
using System.Data;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;

namespace GTPTracker.Controllers
{
    [TermsFilterAttributes]
    public class ProductsController : Controller
    {
        repoProducts repProducts = new repoProducts();
        repoCustomers repCustomers = new repoCustomers();
        repoSettings repSettings = new repoSettings();

        public FileResult DownloadFile(string URI)
        {
            return File(Url.Content("~/Content/images/" + URI), System.Net.Mime.MediaTypeNames.Application.Octet, URI);
        }

        [SessionExpireFilter]
        public ActionResult Manager(int? whatToDo, string serie, string category, string status, string numFiles, int? tab = 1)
        {
            using (repoProducts repProducts = new repoProducts())
            using (repoSeries repSeries= new repoSeries())
            using (repoCategories repCategories = new repoCategories())
            {
                ViewBag.tab = tab;
                ViewBag.whatToDo = whatToDo;
                ViewBag.lSeries = repSeries.getList();
                ViewBag.lCategories = repCategories.getList();
                ViewBag.serie = serie;
                ViewBag.category = category;
                ViewBag.status = status;
                ViewBag.numFiles = numFiles;
                IEnumerable<vProducts> lProducts = repProducts.getvAll();
                IEnumerable<vProducts> lMyProducts = Enumerable.Empty<vProducts>();
                if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() == "GTP KAM")
                {
                    IEnumerable<int> lMyProductsInt = Enumerable.Empty<int>();
                    using (GTPTrackerEntities db = new GTPTrackerEntities())
                    {
                        int idKAM = (int) Session["IDUSER"];
                        IEnumerable<int> lTenantsForThisKAM = db.CustomersKAMs.Where(p => p.idKAM == idKAM).Select(p => p.idCustomer);
                        foreach (int idTenant in lTenantsForThisKAM)
                        {
                            lMyProductsInt.Concat(db.CustomerProducts.Where(p => p.idCustomer == idTenant).Select(p => p.idProduct).ToList());
                        }
                        lMyProducts = lProducts.Where(p => lMyProductsInt.Contains(p.id));
                    }
                }
                if (status != null && status != "All status") lProducts = lProducts.Where(p => p.status == status);
                if (numFiles != null && numFiles != "Any number of files")
                {
                    if (numFiles == "No files") lProducts = lProducts.Where(p => p.numFiles == 0);
                    if (numFiles == "Some files") lProducts = lProducts.Where(p => p.numFiles > 0);
                }
                if (serie != null && serie != "All series") lProducts = lProducts.Where(p => p.text == serie);
                if (category != null && category != "All categories") lProducts = lProducts.Where(p => p.categoryName == category);
                ProductsVM vm = new ProductsVM(lProducts, lMyProducts); // returns all the products if the idtenant == 1 (GTP)
                return View(vm);
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
                    using (repoProducts repProducts = new repoProducts())
                    {
                        if (bulkOptions == "Delete")
                        {
                            foreach (var refNumber in words)
                            {
                                Products product = repProducts.GetByRefNumber(refNumber);
                                if (product != null )
                                   repProducts.Delete(product.id);
                            }
                            return RedirectToAction("Manager", new { whatToDo = 2 });
                        }
                        if (bulkOptions == "Change status to public")
                        {
                            foreach (var refNumber in words)
                            {
                                Products product = repProducts.GetByRefNumber(refNumber);
                                if (product != null)
                                {
                                    product.status = "public";
                                    repProducts.Edit(product);
                                }
                            }
                            return RedirectToAction("Manager", new { whatToDo = 3 });
                        }
                        if (bulkOptions == "Change status to custom")
                        {
                            foreach (var refNumber in words)
                            {
                                Products product = repProducts.GetByRefNumber(refNumber);
                                if (product != null)
                                {
                                    product.status = "custom";
                                    repProducts.Edit(product);
                                }
                            }
                            return RedirectToAction("Manager", new { whatToDo = 3 });
                        }
                        if (bulkOptions == "Change status to draft")
                        {
                            foreach (var refNumber in words)
                            {
                                Products product = repProducts.GetByRefNumber(refNumber);
                                if (product != null)
                                {
                                    product.status = "draft";
                                    repProducts.Edit(product);
                                }
                            }
                            return RedirectToAction("Manager", new { whatToDo = 3 });
                        }
                    }
                }
                return RedirectToAction("Manager", new { tab = 1 });
            }
            catch
            {
                return RedirectToAction("Manager", new { whatToDo = 4 });
            }
        }

        public ActionResult assignCustomers(int idProduct)
        {
            using (repoProducts repProducts = new repoProducts())
            using (repoCustomers repCustomers = new repoCustomers())
            {
                vProducts product = repProducts.Getv(idProduct);
                if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() != "GTP KAM")
                {
                    IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(idProduct).Select(p => p.id);
                    ViewBag.lTenants = repCustomers.getList().Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                } // Manager. Show all the tenants
                if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() == "GTP KAM") // KAM. Show only my tenants
                {
                    int idKAM = (int)Session["IDUSER"];
                    IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(idProduct).Select(p => p.id);

                    ViewBag.lTenants = repCustomers.getvByKAM(idKAM).Where(p=> !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p=>p.name);
                }
                ViewBag.lAssignedTenants = repCustomers.getvByProduct(idProduct);
                return View(product);
            }
        }

        [LogFilter]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult assignCustomers(vProducts product)
        {
            try
            {
                string[] assignedCustomers;
                if (!string.IsNullOrEmpty(Request.Params["assignedCustomers"]))
                    assignedCustomers = Request.Params["assignedCustomers"].Split(',');
                else assignedCustomers = new string[0];
                // remove all the entries in customerProducts of the product   
                using (repoProducts repProducts = new repoProducts())
                using (repoCustomers repCustomers = new repoCustomers())
                {
                    repProducts.removeFromCustomerProductsByProduct(product.id);
                    foreach (var item in assignedCustomers)
                    {
                        int result;
                        bool isInt = Int32.TryParse(item, out result);
                        if (isInt)
                            repProducts.addProductToTenant(product.id, Int32.Parse(item), (int)Session["IDUSER"]);
                        else
                        {
                            Customers customer = repCustomers.GetByName(item);
                            repProducts.addProductToTenant(product.id, customer.id, (int)Session["IDUSER"]);
                        }
                    }
                }

                return RedirectToAction("Manager", "Products", new { whatToDo = 1 });
            }
            catch 
            {
                using (repoProducts repProducts = new repoProducts())
                using (repoCustomers repCustomers = new repoCustomers())
                {
                    ViewBag.lTenants = repCustomers.getList().Where(p => p.active != false).OrderBy(p => p.name);
                    ViewBag.lAssignedTenants = repCustomers.getvByProduct(product.id);
                    return View(product);
                }
            }
        }


        [SessionExpireFilter]
        public ActionResult AllGTPProducts()
        {
            return RedirectToAction("Index", "Series");
        }

        [SessionExpireFilter]
        public ActionResult Index(int? whatToDo)
        {
            ViewBag.whatToDo = whatToDo;
            return RedirectToAction("Manager", new { whatToDo = whatToDo, tab=1 });
        }

        [SessionExpireFilter]
        public ActionResult ImportProducts()
        {
            return View();
        }

        [LogFilter]
        [HttpPost]
        public ActionResult ImportProducts(ImportProductsViewModel model)
        {
            ImportResult result = new ImportResult();
            DateTime start = DateTime.UtcNow;
            if (ModelState.IsValid)
            {
                DataTable dt = new DataTable();
                var postedFile = Request.Files["File"];
                if (postedFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(postedFile.FileName);
                    string path = Path.Combine(Server.MapPath("~/App_Data"), fileName);
                    try
                    {
                        postedFile.SaveAs(path);
                        dt = ProcessCSV(path);

                        List<string> validStatuses = new List<string> {
                            "custom", "public" , "draft"
                        };
                        Dictionary<int,string> statuses = new Dictionary<int, string> {
                            { 1, "draft" },
                            { 2, "public" },
                            { 3, "custom" },

                        };

                        if (dt.Rows.Count > 0)
                        {
                            string lastUpdateDateString = repSettings.Get("ImportProducts-LastUpdate");
                            DateTime? lastUpdate = null;
                            DateTime d;
                            repoSeries repSerie = new repoSeries();                            

                            if (DateTime.TryParse(lastUpdateDateString, out d))
                                lastUpdate = d;

                            List<Products> bulkProducts = new List<Products>();

                            foreach (DataRow item in dt.Rows)
                            {
                                if (/*item.ItemArray.Count() == 21 &&*/ ConvertFromDBVal<string>(item[2]) != null && ConvertFromDBVal<string>(item[2]) != null)
                                {

                                    // importing
                                    string rowLastUpdate = (ConvertFromDBVal<string>(item[2])).Trim();
                                    string refNumber = (ConvertFromDBVal<string>(item[0])).Trim();
                                    bool hasToUpdate = false;
                                    
                                    if (!string.IsNullOrEmpty(rowLastUpdate))
                                    {
                                        DateTime rowLastUpdateDate;
                                        // 01.03.2016
                                        if (DateTime.TryParseExact(rowLastUpdate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out rowLastUpdateDate))
                                        {
                                            if (rowLastUpdateDate > lastUpdate || lastUpdate == null)
                                                hasToUpdate = true;                                            
                                        }
                                    }
                                    else
                                    {
                                        hasToUpdate = true;
                                    }
                                    bool exists = true;
                                    // checking if row exists
                                    Products row = repProducts.ProductByRefNumber(refNumber);
                                    if (row == null)
                                    {
                                        row = new Products();
                                        exists = false;
                                    }
                                    row.refNumber = refNumber;

                                  /*  if (exists && !validUpdateDate)
                                    {
                                        result.FailCount++;
                                        result.result.Add(new ImportResultRow
                                        {
                                            RefNumber = refNumber,
                                            Message = "Product exists in database but no update date in CSV."
                                        });
                                    }*/
                                    if (!hasToUpdate && exists)
                                    {
                                        // no need to update
                                        continue;
                                    }
                                    else
                                    {
                                        // parsing status
                                        string rowStatus = (ConvertFromDBVal<string>(item[3])).Trim().ToLower();

                                        if (!statuses.Values.Contains(rowStatus))
                                        {
                                            // check if its a number
                                            int statusNumber;
                                            if (int.TryParse(rowStatus, out statusNumber) && statuses.Keys.Contains(statusNumber))
                                            {
                                                rowStatus = statuses[statusNumber];
                                            }
                                            else
                                            {
                                                result.FailCount++;
                                                result.result.Add(new ImportResultRow
                                                {
                                                    RefNumber = refNumber,
                                                    Message = "Product doesn't have valid status."
                                                });
                                                Debug.WriteLine("Error : Product doesn't have valid status. " + row.refNumber);
                                                continue;
                                            }
                                        }

                                        row.status = rowStatus;

                                        // getting category and serie

                                        string serieString = ConvertFromDBVal<string>(item[5]);
                                        int serie;
                                        string categoryString = ConvertFromDBVal<string>(item[4]);
                                        int category;
                                        if (int.TryParse(serieString, out serie) && int.TryParse(categoryString, out category)) // serie / category are numbers?
                                        {
                                            Series serieFound = repSerie.GetSerieBySerieAndCategory(serie, category);
                                            if (serieFound != null) // exists that serie?
                                            {
                                                row.serie = serie;
                                                row.category = category;
                                                row.idSerie = serieFound.id;
                                            }
                                            else
                                            {
                                                result.FailCount++;
                                                result.result.Add(new ImportResultRow
                                                {
                                                    RefNumber = refNumber,
                                                    Message = string.Format("Invalid category / serie values : ({0}, {1})", categoryString, serieString)
                                                });
                                                Debug.WriteLine("Error : Invalid category / serie values. {0}", row.refNumber);
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            result.FailCount++;
                                            result.result.Add(new ImportResultRow
                                            {
                                                RefNumber = refNumber,
                                                Message = string.Format("Invalid category / serie format : ({0}, {1})", categoryString, serieString)
                                            });
                                            Debug.WriteLine("Error : Invalid category / serie format. {0}", row.refNumber);
                                            continue;
                                        }                    

                                        row.type = (string)item[6];
                                        DateTime cDate; // if cdate is a date save it. otherwise do nothing
                                        if (DateTime.TryParseExact(ConvertFromDBVal<string>(item[1]), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out cDate))
                                            row.createdAt = cDate;
                                        row.modulus = ParseNumber(ConvertFromDBVal<string>(item[7]), 2);
                                        row.volume = ParseNumber(ConvertFromDBVal<string>(item[8]), 0);
                                        row.Contact_area = ParseNumber(ConvertFromDBVal<string>(item[9]), 0);
                                        row.contact1 = ParseNumber(ConvertFromDBVal<string>(item[9]), 0);
                                        row.contact2 = ParseNumber(ConvertFromDBVal<string>(item[10]), 0);
                                        row.Outer_diameter = ParseNumber(ConvertFromDBVal<string>(item[11]), 0);
                                        row.outer1 = ParseNumber(ConvertFromDBVal<string>(item[11]), 0);
                                        row.outer2 = ParseNumber(ConvertFromDBVal<string>(item[12]), 0);
                                        row.Height = ParseNumber(ConvertFromDBVal<string>(item[13]), 0);
                                        bulkProducts.Add(row);
                                        Debug.WriteLine("Success " + row.refNumber);
                                        result.SuccessCount++;
                                    }

                                }
                                else
                                {
                                    result.FailCount++;
                                    result.result.Add(new ImportResultRow
                                    {
                                        RefNumber = "N/A",
                                        Message = "CSV row not matching required columns number"
                                    });
                                }
                            }

                            if (repProducts.SaveBulk(bulkProducts))
                            {
                                result.Success = true;
                                result.Message = bulkProducts.Count().ToString() + " Products Saved successfully";
                            }
                            else
                            {
                                result.Success = false;
                                result.Message = "Error while saving products, please contact the administrator";
                            }

                            repSettings.UpdateSetting("ImportProducts-LastUpdate", DateTime.Now.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Success = false;
                        result.Message = ex.Message;
                    }
                }
            }
            Debug.WriteLine("End. Total Time = " + (DateTime.UtcNow - start));
            ViewBag.ImportResult = result;
            return View();
        }

        public static T ConvertFromDBVal<T>(object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return default(T); // returns the default value for the type
            }
            else
            {
                return (T)obj;
            }
        }

        private static string ParseNumber(string str, int digits = 0)
        {
            str = str.Replace("\"", "");
            if (string.IsNullOrEmpty(str)) return string.Empty;
            try
            {
                decimal number = decimal.Parse(str);
                return Math.Round(number, digits, MidpointRounding.AwayFromZero).ToString("N" + digits);
            }
            catch {
                return string.Empty;
            }
        }

        private static DataTable ProcessCSV(string fileName)
        {
            string Feedback = string.Empty;
            string line = string.Empty;
            string[] strArray;
            DataTable dt = new DataTable();
            DataRow row;
            Regex r = new Regex(";(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            StreamReader sr = new StreamReader(fileName);
            line = sr.ReadLine();
            strArray = r.Split(line);
            Array.ForEach(strArray, s => dt.Columns.Add(new DataColumn()));
            while ((line = sr.ReadLine()) != null)
            {
                row = dt.NewRow();
                row.ItemArray = r.Split(line);
                dt.Rows.Add(row);
            }
            sr.Dispose();
            return dt;
        }

        [SessionExpireFilter]
        public ActionResult IndexAsGTP(int? whatToDo, int? tab = 1)
        {
            return RedirectToAction("Manager", new { whatToDo = whatToDo, tab=1 });          
        }

        [SessionExpireFilter]
        public ActionResult IndexByTenant(int id)
        {
            Customers tenant = repCustomers.Get(id);
            ViewBag.tenant = tenant.name;
            ViewBag.idTenant = id;
            IEnumerable<Products> lProducts = repProducts.getList(id);
            return View(lProducts);
        }

        [SessionExpireFilter]
        public ActionResult getListBySerie(int id)
        {
            using (repoSeries repSerie = new repoSeries())
            {
                int idTenant = (int)Session["IDTENANT"];
                IEnumerable<Products> lMyProducts = repProducts.getList(idTenant);
                IEnumerable<Products> lProducts = repProducts.getListBySerie(id).Where(p => p.showInCatalog == true);
                List<Products> result = new List<Products>();
                foreach (var item in lProducts)
                {  // take only products the customer did not have
                    /*         Products product = lMyProducts.Where(p => p.id == item.id ).FirstOrDefault();
                             if (product == null) */
                    // according to joerg, now all the products should be shown on the products list
                    result.Add(item);
                }
                Series serie = repSerie.get(id);
                ViewBag.Category = repSerie.get(id).text;
                ViewBag.idCategory = id;
                ViewBag.image = serie.imageFileName;
                ViewBag.applications = serie.applications;
                ViewBag.characteristics = serie.characteristics.Replace("\r\n", "<br>");
                ViewBag.serie = serie.serie;
                return View(result);
            }
        }

        [SessionExpireFilter]
        public ActionResult Files(int id, int? whatToDo)
        {
            ViewBag.whatToDo = whatToDo;
            ProductVM vm = new ProductVM(id);
            return View(vm);
        }     

        [SessionExpireFilter]
        public ActionResult Details(int id, int? origen=0 )
        {
            ViewBag.origen = origen;
            ProductVM vm = new ProductVM(id);
            return View(vm);
        }

        public JsonResult DetailsJSON(int id)
        {
            ProductVM vm = new ProductVM(id);
            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        [SessionExpireFilter]
        public ActionResult AddFile(int id)
        {
            ViewBag.id = id;
            Products product = repProducts.Get(id);
            ViewBag.type = product.type;
            ViewBag.refNumber = product.refNumber;
            return View();
        }

        [LogFilter]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult AddFile(int id, HttpPostedFileBase file, string internalGTP)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    if (file.ContentLength > 20000000)
                    {
                        ModelState.AddModelError("", "File is too big. Max size allowed 20 MB");
                        ViewBag.id = id;
                        return View(id);
                    }
                    Products product = repProducts.Get(id);
                    var fileName = Convert.ToString(product.refNumber) + "_Product_" + DateTime.UtcNow.Ticks.ToString() + "-" + System.IO.Path.GetFileName(file.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    file.SaveAs(path);
                    ProductsFiles pFile = new ProductsFiles();
                    pFile.fileName = file.FileName;
                    pFile.URI = fileName;
                    pFile.refNumber = product.refNumber;
                    pFile.cDate = DateTime.Today;
                    pFile.idProduct = id;
                    pFile.lastUpdate = DateTime.UtcNow;
                    if (internalGTP == "on") pFile.internalGTP = true;
                    else pFile.internalGTP = false;
                    repProducts.addFile(pFile);
                }
                return RedirectToAction("Edit", new { id = id, whatToDo=1});
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.id = id;
                return View(id);
            }
        }

        [LogFilter]
        public ActionResult DeleteFile(int id, int idProduct)
        {
            try
            {                
                ProductsFiles file = repProducts.GetFile(id);
                string fullPath = Request.MapPath("~/Content/images/" + file.URI);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    repProducts.DeleteFile(file.id);
                }
                return RedirectToAction("Edit", new { id = idProduct, whatToDo = 2 });
            }
            catch
            {
                return RedirectToAction("Edit", new { id = idProduct, whatToDo = 3 });
            }
        }

        [SessionExpireFilter]
        public ActionResult Create()
        {
            Products product = new Products();
            using (repoSeries repSeries = new repoSeries())
            {
                ViewBag.lSeries = repSeries.getList().OrderBy(p=>p.text);
                using (repoGeneralDocs repDocs = new repoGeneralDocs())
                {
                    IEnumerable<vDocsProducts> lAssignedDocs = Enumerable.Empty<vDocsProducts>();
                    IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                    IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                    ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p=>p.filename);
                    ViewBag.lDocsCategories = lDocsCategories.OrderBy(p=>p.category);
                    ViewBag.lDocs = lDocs.OrderBy(p=>p.filename);
                }
                return View(product);
            }
        }

        [LogFilter]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Create(Products product)
        {
            try
            {
                if (product.outer1 != null)
                    try { product.outer1 = product.outer1.Replace(".", ","); float.Parse(product.outer1); }
                    catch 
                    {
                        using (repoSeries repSeries = new repoSeries())
                        {
                            ViewBag.lSeries = repSeries.getList().OrderBy(p => p.text);
                            using (repoGeneralDocs repDocs = new repoGeneralDocs())
                            {
                                IEnumerable<vDocsProducts> lAssignedDocs = Enumerable.Empty<vDocsProducts>();
                                IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                                IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();
                                
                                ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                                ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                                ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                            }
                            ViewBag.lFiles = repProducts.getFiles(product.refNumber);

                            ViewBag.error = "Outer diameter 1 must be a number";
                            ModelState.AddModelError("", "Outer diameter 1 must be a number");
                            return View(product);
                        }
                    }
                if (product.outer2 != null)
                    try { product.outer2 = product.outer2.Replace(".", ","); float.Parse(product.outer2); }
                    catch 
                    {
                        using (repoSeries repSeries = new repoSeries())
                        {
                            ViewBag.lSeries = repSeries.getList().OrderBy(p => p.text);
                            using (repoGeneralDocs repDocs = new repoGeneralDocs())
                            {
                                IEnumerable<vDocsProducts> lAssignedDocs = Enumerable.Empty<vDocsProducts>();
                                IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                                IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                                ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                                ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                                ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                            }
                            ViewBag.lFiles = repProducts.getFiles(product.refNumber);

                            ViewBag.error = "Outer diameter 2 must be a number";
                            ModelState.AddModelError("", "Outer diameter 2 must be a number");
                            return View(product);
                        }
                    }
                if (product.contact1 != null)
                    try { product.contact1 = product.contact1.Replace(".", ","); float.Parse(product.contact1); }
                    catch 
                    {
                        using (repoSeries repSeries = new repoSeries())
                        {
                            ViewBag.lSeries = repSeries.getList().OrderBy(p => p.text);
                            using (repoGeneralDocs repDocs = new repoGeneralDocs())
                            {
                                IEnumerable<vDocsProducts> lAssignedDocs = Enumerable.Empty<vDocsProducts>();
                                IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                                IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                                ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                                ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                                ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                            }
                            ViewBag.lFiles = repProducts.getFiles(product.refNumber);

                            ViewBag.error = "Contact area 1 must be a number";
                            ModelState.AddModelError("", "Contact area 1 must be a number");
                            return View(product);
                        }
                    }
                if (product.contact2 != null)
                    try { product.contact2 = product.contact2.Replace(".", ","); float.Parse(product.contact2); }
                    catch 
                    {
                        using (repoSeries repSeries = new repoSeries())
                        {
                            ViewBag.lSeries = repSeries.getList().OrderBy(p => p.text);
                            using (repoGeneralDocs repDocs = new repoGeneralDocs())
                            {
                                IEnumerable<vDocsProducts> lAssignedDocs = Enumerable.Empty<vDocsProducts>();
                                IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                                IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                                ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                                ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                                ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                            }
                            ViewBag.lFiles = repProducts.getFiles(product.refNumber);

                            ViewBag.error = "Contact area 2 must be a number";
                            ModelState.AddModelError("", "Contact area 2 must be a number");
                            return View(product);
                        }
                    }
                using (repoSeries repSeries = new repoSeries())
                {
                    if (repProducts.GetByRefNumber(product.refNumber) != null)
                    {
                        ViewBag.lSeries = repSeries.getList().OrderBy(p => p.text);
                        using (repoGeneralDocs repDocs = new repoGeneralDocs())
                        {
                            IEnumerable<vDocsProducts> lAssignedDocs = Enumerable.Empty<vDocsProducts>();
                            IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                            IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                            ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                            ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                            ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                        }
                        ViewBag.lFiles = repProducts.getFiles(product.refNumber);

                        ViewBag.error = "Already exists a product with this reference number";
                        return View(product);
                    }
                    if (product.refNumber == null)
                    {
                        ViewBag.lSeries = repSeries.getList().OrderBy(p => p.text);
                        using (repoGeneralDocs repDocs = new repoGeneralDocs())
                        {
                            IEnumerable<vDocsProducts> lAssignedDocs = Enumerable.Empty<vDocsProducts>();
                            IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                            IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                            ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                            ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                            ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                        }
                        ViewBag.lFiles = repProducts.getFiles(product.refNumber);

                        ViewBag.error = "Reference number is a mandatory field";
                        return View(product);
                    }
                    if (product.type == null)
                    {
                        ViewBag.lSeries = repSeries.getList().OrderBy(p => p.text);
                        using (repoGeneralDocs repDocs = new repoGeneralDocs())
                        {
                            IEnumerable<vDocsProducts> lAssignedDocs = Enumerable.Empty<vDocsProducts>();
                            IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                            IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                            ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                            ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                            ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                        }
                        ViewBag.lFiles = repProducts.getFiles(product.refNumber);

                        ViewBag.error = "Type is a mandatory field";
                        return View(product);
                    }
                }
                product.createdAt = DateTime.UtcNow;
                repProducts.Create(product);

                // documents
                string[] assignedDocs;
                if (!string.IsNullOrEmpty(Request.Params["assignedDocs"]))
                    assignedDocs = Request.Params["assignedDocs"].Split(',');
                else
                    assignedDocs = new string[0];
                // delete the previous assigned products on bd
                using (repoGeneralDocs repDocs = new repoGeneralDocs())
                {
                    repDocs.DeleteByProduct(product.id);
                    foreach (var item in assignedDocs)
                    {
                        int result;
                        bool isInt = Int32.TryParse(item, out result);
                        if (isInt)
                            repDocs.AssignDocLinkToProduct(Int32.Parse(item), product.id);
                        else
                        {
                            GeneralProductsDocuments doc = repDocs.GetByFilename(item.TrimStart().TrimEnd());
                            repDocs.AssignDocLinkToProduct(doc.id, product.id);
                        }
                    }
                }
                ViewBag.lFiles = repProducts.getFiles(product.refNumber);

                return RedirectToAction("Manager", new { whatToDo = 5 });
            }
            catch (Exception ex)
            {
                using (repoSeries repSeries = new repoSeries())
                {
                    ViewBag.lSeries = repSeries.getList().OrderBy(p => p.text);
                    using (repoGeneralDocs repDocs = new repoGeneralDocs())
                    {
                        IEnumerable<vDocsProducts> lAssignedDocs = Enumerable.Empty<vDocsProducts>();
                        IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                        IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                        ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                        ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                        ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                    }
                    ViewBag.error = ex.Message;
                    ModelState.AddModelError("", ex.Message);
                    return View(product);
                }
            }
        }

        [SessionExpireFilter]
        public ActionResult Edit(int id)
        {
            Products product = repProducts.Get(id);
            using (repoSeries repSeries = new repoSeries())
            {
                if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() != "GTP KAM")
                {
                    IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(id).Select(p => p.id);
                    ViewBag.lTenants = repCustomers.getList().Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                } // Manager. Show all the tenants
                if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() == "GTP KAM") // KAM. Show only my tenants
                {
                    int idKAM = (int)Session["IDUSER"];
                    IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(id).Select(p => p.id);
                    ViewBag.lTenants = repCustomers.getvByKAM(idKAM).Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                }
                ViewBag.lAssignedTenants = repCustomers.getvByProduct(id).OrderBy(p=>p.name);
                ViewBag.lSeries = repSeries.getList().OrderBy(p=>p.text);
                ViewBag.lFiles = repProducts.getFiles(product.refNumber);

                using (repoGeneralDocs repDocs = new repoGeneralDocs())
                {
                    IEnumerable<vDocsProducts> lAssignedDocs = repDocs.GetDocsByProducts(product.id);
                    IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                    IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                    ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                    ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                    ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                }

                return View(product);
            }           
        }

        [LogFilter]
        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Edit(Products product)
        {
            try
            {
                ViewBag.lFiles = repProducts.getFiles(product.refNumber);
                if (product.outer1 != null)
                    try { product.outer1 = product.outer1.Replace(".", ","); float.Parse(product.outer1); }
                    catch 
                    {
                        using (repoSeries repSeries = new repoSeries())
                        {
                            if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() != "GTP KAM")
                            {
                                IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(product.id).Select(p => p.id);
                                ViewBag.lTenants = repCustomers.getList().Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                            } // Manager. Show all the tenants
                            if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() == "GTP KAM") // KAM. Show only my tenants
                            {
                                int idKAM = (int)Session["IDUSER"];
                                IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(product.id).Select(p => p.id);
                                ViewBag.lTenants = repCustomers.getvByKAM(idKAM).Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                            }
                            ViewBag.lAssignedTenants = repCustomers.getvByProduct(product.id).OrderBy(p => p.name);
                            ViewBag.lSeries = repSeries.getList().OrderBy(p => p.text);

                            using (repoGeneralDocs repDocs = new repoGeneralDocs())
                            {
                                IEnumerable<vDocsProducts> lAssignedDocs = repDocs.GetDocsByProducts(product.id);
                                IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                                IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                                ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                                ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                                ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                            }

                            ViewBag.error = "Outer diameter 1 must be a number";
                            ModelState.AddModelError("", "Outer diameter 1 must be a number");
                            return View(product);
                        }
                    }
                if (product.outer2 != null)
                    try { product.outer2 = product.outer2.Replace(".", ","); float.Parse(product.outer2); }
                    catch 
                    {
                        using (repoSeries repSeries = new repoSeries())
                        {
                            if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() != "GTP KAM")
                            {
                                IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(product.id).Select(p => p.id);
                                ViewBag.lTenants = repCustomers.getList().Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                            } // Manager. Show all the tenants
                            if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() == "GTP KAM") // KAM. Show only my tenants
                            {
                                int idKAM = (int)Session["IDUSER"];
                                IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(product.id).Select(p => p.id);
                                ViewBag.lTenants = repCustomers.getvByKAM(idKAM).Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                            }
                            ViewBag.lAssignedTenants = repCustomers.getvByProduct(product.id).OrderBy(p => p.name);
                            ViewBag.lSeries = repSeries.getList().OrderBy(p => p.text);
                            ViewBag.error = "Outer diameter 2 must be a number";
                            using (repoGeneralDocs repDocs = new repoGeneralDocs())
                            {
                                IEnumerable<vDocsProducts> lAssignedDocs = repDocs.GetDocsByProducts(product.id);
                                IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                                IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                                ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                                ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                                ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                            }
                            ModelState.AddModelError("", "Outer diameter 2 must be a number");
                            return View(product);
                        }
                    }
                if (product.contact1 != null)
                    try { product.contact1 = product.contact1.Replace(".", ","); float.Parse(product.contact1); }
                    catch 
                    {
                        using (repoSeries repSeries = new repoSeries())
                        {
                            if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() != "GTP KAM")
                            {
                                IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(product.id).Select(p => p.id);
                                ViewBag.lTenants = repCustomers.getList().Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                            } // Manager. Show all the tenants
                            if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() == "GTP KAM") // KAM. Show only my tenants
                            {
                                int idKAM = (int)Session["IDUSER"];
                                IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(product.id).Select(p => p.id);
                                ViewBag.lTenants = repCustomers.getvByKAM(idKAM).Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                            }
                            ViewBag.lAssignedTenants = repCustomers.getvByProduct(product.id).OrderBy(p => p.name);
                            ViewBag.lSeries = repSeries.getList().OrderBy(p => p.text);
                            ViewBag.error = "Contact area 1 must be a number";
                            using (repoGeneralDocs repDocs = new repoGeneralDocs())
                            {
                                IEnumerable<vDocsProducts> lAssignedDocs = repDocs.GetDocsByProducts(product.id);
                                IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                                IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                                ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                                ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                                ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                            }
                            ModelState.AddModelError("", "Contact area 1 must be a number");
                            return View(product);
                        }
                    }
                if (product.contact2 != null)
                    try { product.contact2 = product.contact2.Replace(".", ","); float.Parse(product.contact2); }
                    catch 
                    {
                        using (repoSeries repSeries = new repoSeries())
                        {
                            if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() != "GTP KAM")
                            {
                                IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(product.id).Select(p => p.id);
                                ViewBag.lTenants = repCustomers.getList().Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                            } // Manager. Show all the tenants
                            if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() == "GTP KAM") // KAM. Show only my tenants
                            {
                                int idKAM = (int)Session["IDUSER"];
                                IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(product.id).Select(p => p.id);
                                ViewBag.lTenants = repCustomers.getvByKAM(idKAM).Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                            }
                            ViewBag.lAssignedTenants = repCustomers.getvByProduct(product.id).OrderBy(p => p.name);
                            ViewBag.lSeries = repSeries.getList().OrderBy(p => p.text);
                            ViewBag.error = "Contact area 2 must be a number";
                            using (repoGeneralDocs repDocs = new repoGeneralDocs())
                            {
                                IEnumerable<vDocsProducts> lAssignedDocs = repDocs.GetDocsByProducts(product.id);
                                IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                                IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                                ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                                ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                                ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                            }
                            ModelState.AddModelError("", "Contact area 2 must be a number");
                            return View(product);
                        }
                    }
                using (repoSeries repSeries = new repoSeries())
                {
                    if (product.refNumber == null)
                    {
                        if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() != "GTP KAM")
                        {
                            IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(product.id).Select(p => p.id);
                            ViewBag.lTenants = repCustomers.getList().Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                        } // Manager. Show all the tenants
                        if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() == "GTP KAM") // KAM. Show only my tenants
                        {
                            int idKAM = (int)Session["IDUSER"];
                            IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(product.id).Select(p => p.id);
                            ViewBag.lTenants = repCustomers.getvByKAM(idKAM).Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                        }
                        ViewBag.lAssignedTenants = repCustomers.getvByProduct(product.id).OrderBy(p => p.name);
                        ViewBag.lSeries = repSeries.getList().OrderBy(p => p.text);
                        using (repoGeneralDocs repDocs = new repoGeneralDocs())
                        {
                            IEnumerable<vDocsProducts> lAssignedDocs = repDocs.GetDocsByProducts(product.id);
                            IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                            IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                            ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                            ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                            ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                        }
                        ViewBag.error = "Reference number is a mandatory field";
                        return View(product);
                    }
                    if (product.type == null)
                    {
                        if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() != "GTP KAM")
                        {
                            IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(product.id).Select(p => p.id);
                            ViewBag.lTenants = repCustomers.getList().Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                        } // Manager. Show all the tenants
                        if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() == "GTP KAM") // KAM. Show only my tenants
                        {
                            int idKAM = (int)Session["IDUSER"];
                            IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(product.id).Select(p => p.id);
                            ViewBag.lTenants = repCustomers.getvByKAM(idKAM).Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                        }
                        ViewBag.lAssignedTenants = repCustomers.getvByProduct(product.id).OrderBy(p => p.name);
                        ViewBag.lSeries = repSeries.getList().OrderBy(p => p.text);
                        using (repoGeneralDocs repDocs = new repoGeneralDocs())
                        {
                            IEnumerable<vDocsProducts> lAssignedDocs = repDocs.GetDocsByProducts(product.id);
                            IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                            IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                            ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                            ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                            ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                        }
                        ViewBag.error = "Type is a mandatory field";
                        return View(product);
                    }
                }
                // Save the customers assigned to that product

                string[] assignedCustomers;
                if (!string.IsNullOrEmpty(Request.Params["assignedCustomers"]))
                    assignedCustomers = Request.Params["assignedCustomers"].Split(',');
                else assignedCustomers = new string[0];
                // remove all the entries in customerProducts of the product   
                using (repoCustomers repCustomers = new repoCustomers())
                {
                    repProducts.removeFromCustomerProductsByProduct(product.id);
                    foreach (var item in assignedCustomers)
                    {
                        int result;
                        bool isInt = Int32.TryParse(item, out result);
                        if (isInt)
                            repProducts.addProductToTenant(product.id, Int32.Parse(item), (int)Session["IDUSER"]);
                        else
                        {
                            Customers customer = repCustomers.GetByName(item);
                            repProducts.addProductToTenant(product.id, customer.id, (int)Session["IDUSER"]);
                        }
                    }
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
                    repDocs.DeleteByProduct(product.id);
                    foreach (var item in assignedDocs)
                    {
                        int result;
                        bool isInt = Int32.TryParse(item, out result);
                        if (isInt)
                            repDocs.AssignDocLinkToProduct(Int32.Parse(item), product.id);
                        else
                        {
                            GeneralProductsDocuments doc = repDocs.GetByFilename(item.TrimStart().TrimEnd());
                            repDocs.AssignDocLinkToProduct(doc.id, product.id);
                        }
                    }
                }

                repProducts.Edit(product);
                return RedirectToAction("Manager", new { whatToDo = 1 });
            }
            catch (Exception ex)
            {
                using (repoSeries repSeries = new repoSeries())
                {
                    if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() != "GTP KAM")
                    {
                        IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(product.id).Select(p => p.id);
                        ViewBag.lTenants = repCustomers.getList().Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                    } // Manager. Show all the tenants
                    if (Session["BUSINESSROLE"] != null && Session["BUSINESSROLE"].ToString().Trim() == "GTP KAM") // KAM. Show only my tenants
                    {
                        int idKAM = (int)Session["IDUSER"];
                        IEnumerable<int> lIdsAssignedTenants = repCustomers.getvByProduct(product.id).Select(p => p.id);
                        ViewBag.lTenants = repCustomers.getvByKAM(idKAM).Where(p => !(lIdsAssignedTenants.Contains(p.id))).OrderBy(p => p.name);
                    }
                    ViewBag.lAssignedTenants = repCustomers.getvByProduct(product.id).OrderBy(p => p.name);
                    ViewBag.lSeries = repSeries.getList().OrderBy(p => p.text);

                    using (repoGeneralDocs repDocs = new repoGeneralDocs())
                    {
                        IEnumerable<vDocsProducts> lAssignedDocs = repDocs.GetDocsByProducts(product.id);
                        IEnumerable<GenDocCategories> lDocsCategories = repDocs.getCategories();
                        IEnumerable<GeneralProductsDocuments> lDocs = repDocs.getList();

                        ViewBag.lAssignedDocs = lAssignedDocs.OrderBy(p => p.filename);
                        ViewBag.lDocsCategories = lDocsCategories.OrderBy(p => p.category);
                        ViewBag.lDocs = lDocs.OrderBy(p => p.filename);
                    }
                    ViewBag.error = ex.Message;
                    ModelState.AddModelError("", ex.Message);
                    return View(product);
                }
            }
        }

        [SessionExpireFilter]
        public ActionResult Delete(int id)
        {
            Products product = repProducts.Get(id);
            return View(product);
        }

        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                repProducts.Delete(id);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
