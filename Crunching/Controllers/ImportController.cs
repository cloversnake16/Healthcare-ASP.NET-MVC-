using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Crunching.Models;
using System.Diagnostics;
using GTPTracker.Helpers;
using System.Data;
using System.IO;
using GTPTracker.repos;
using System.Text.RegularExpressions;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;

namespace GTPTracker.Controllers
{
    public class ImportController : Controller
    {
        public List<string> nonProcessedList = new List<string>();
        private List<string> existingInDiskList = new List<string>();

        public int newFiles = 0;
        public int processedFiles = 0;
        public int replacedFiles = 0;
        public int deletedFiles = 0;

        public bool flagParsingRunning = false; // from management.summary every X minutes via ajax a call will be made to get the status of this flag, to tell the user if the parsing is running.
        private const string basePath = "~/Content/images";
        
        /// <summary>
        /// Called from Management.Summary
        /// 
        /// It is a batch process run by admin once a ftp upload of the files occur.        
        /// Loops recursively through the files directory and update the ProductsFiles table
        /// </summary>
        public void ParseFilesInHosting()
        {
            flagParsingRunning = true;
            DateTime ini = DateTime.UtcNow;
            DirSearch(Server.MapPath(basePath));
           // DirSearch(@"C:\Users\Nuevo\Documents\gtp_backend_version_feria\Crunching\Content\images");
            Debug.WriteLine("Time for updating the files = {0} ", (DateTime.Now - ini));
            flagParsingRunning = false;
            // save the result of the parsing in the db.
            FileBatchUploads batchUploadResult = new FileBatchUploads();
            batchUploadResult.cDate = DateTime.UtcNow;
            batchUploadResult.deletedFiles = deletedFiles;
            batchUploadResult.newFiles = newFiles;
            batchUploadResult.noMatchFiles = nonProcessedList.Count();
            batchUploadResult.processedFiles = processedFiles;
            batchUploadResult.replacedFiles = replacedFiles;
            batchUploadResult.status = "completed";
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            {
                db.FileBatchUploads.AddObject(batchUploadResult);
                db.SaveChanges(); // needed here to get the id of the batchUploadResult record
                // save the non processed files in the table, to show them in a list
                createNonProcessedRecord(batchUploadResult.id);
            }
            // update lastUpdate in table productFiles
            updateLastUpdateField();
            // delete records in productFiles corresponding to files not existing in disk anymore
            deleteRecordsWithPhysicalFileRemoved();
            // TODO Send an email                
        }

        private void deleteRecordsWithPhysicalFileRemoved()
        {
            //IEnumerable<ProductsFiles> lNonExistingInDisk;
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            {
                List<ProductsFiles> lNonExistingInDisk = db.ProductsFiles.ToList();
                lNonExistingInDisk.RemoveAll(x => existingInDiskList.Contains(x.fileName));
                foreach (ProductsFiles item in lNonExistingInDisk)
                {
                    ProductsFiles des = db.ProductsFiles.FirstOrDefault(e => e.id == item.id);
                    if (des != null)
                    {
                        db.ProductsFiles.DeleteObject(des);
                    }
                }
                db.SaveChanges();
            }
        }

        private void createNonProcessedRecord(int idBatchUpload)
        {
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            {
                foreach (var fileName in nonProcessedList)
                {
                    NonProcessedFiles nonProcessedFile = new NonProcessedFiles();
                    nonProcessedFile.idBatchUpdate = idBatchUpload;
                    nonProcessedFile.fileName = fileName;
                    db.NonProcessedFiles.AddObject(nonProcessedFile);
                }
                db.SaveChanges();
            }
        }

        private void updateLastUpdateField()
        {
            // Get the connection string
            ConnectionStringSettings mySetting = ConfigurationManager.ConnectionStrings["ApplicationServices"];
            if (mySetting == null || string.IsNullOrEmpty(mySetting.ConnectionString))
                throw new Exception("Fatal error: missing connecting string in web.config file");
           string conString = mySetting.ConnectionString;
           using (SqlConnection connection = new SqlConnection(conString))
           {
               connection.Open();
               using (SqlCommand command = connection.CreateCommand())
               {
                   command.CommandText = "UPDATE ProductsFiles SET lastUpdate=@NewDate";
                   SqlParameter parameter = new SqlParameter("@NewDate", SqlDbType.DateTime);
                   parameter.Value = DateTime.UtcNow;
                   command.Parameters.Add(parameter);
                   command.ExecuteNonQuery() ;
               }
           }
        }

        private void DirSearch(string dir)
        {
            try
            {
                string removeString = Server.MapPath(basePath);//@"C:\Users\Nuevo\Documents\gtp_backend_version_feria\Crunching\Content\images";
                foreach (string f in System.IO.Directory.GetFiles(dir))
                {
                    ++processedFiles;
                   // Debug.WriteLine(f);
                    string fileName = System.IO.Path.GetFileName(f);
                    Debug.WriteLine("Read from disk = " +fileName);
                    existingInDiskList.Add(fileName);
                    char[] delimiters = new char[] { '_', '.' };
                    string[] parts = fileName.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    Boolean isInternal = false;
                    string product = parts[1];
                    Debug.WriteLine("The product is :" + product);
                    if (parts.Length > 3 && parts[2] == "int") { isInternal = true;}
                    using (GTPTrackerEntities db = new GTPTrackerEntities())
                    {
                        Products productExists = db.Products.Where(p => p.refNumber == product).FirstOrDefault();
                        if (productExists != null)
                        {
                            // check if the file already exists on productFiles.
                            ProductsFiles alreadyExists = db.ProductsFiles.FirstOrDefault(p => p.fileName == fileName);
                            if (alreadyExists == null) // only add if not exists
                            {
                                ProductsFiles file = new ProductsFiles();
                                file.cDate = DateTime.UtcNow;
                                file.fileName = fileName;
                                int index = f.IndexOf(removeString);
                                string cleanPath = (index < 0)
                                    ? f
                                    : f.Remove(index, removeString.Length);
                                file.URI = cleanPath.Remove(0, 1).Replace("\\", "/");  // remove all the path except the folders of the product type. Remove also the first "/"
                                file.refNumber = product;
                                file.internalGTP = isInternal;
                                file.idProduct = productExists.id;
                                Debug.WriteLine("saving file :" + product);
                                db.ProductsFiles.AddObject(file);
                                db.SaveChanges(); 
                                ++newFiles;
                            }
                            else ++replacedFiles;
                        }
                        else
                        {
                            Debug.WriteLine("Product " + product + " did not exists");
                            int index = f.IndexOf(removeString);
                            string cleanPath = (index < 0) ? f : f.Remove(index, removeString.Length);
                            nonProcessedList.Add(cleanPath.Remove(0, 1).Replace("\\", "/"));
                        }
                    }              
                }
                foreach (string d in System.IO.Directory.GetDirectories(dir))
                    DirSearch(d);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("Error : " +ex.Message);
            }
        }

        public ActionResult deleteFileFromDisk(string file, int id)
        {
            try
            {
                string fullPathFile = Server.MapPath(basePath + "/" + file);
                if (System.IO.File.Exists(fullPathFile))
                {
                    System.IO.File.Delete(fullPathFile);
                    using (GTPTrackerEntities db = new GTPTrackerEntities())
                    {
                        NonProcessedFiles nonProcessedFile = db.NonProcessedFiles.FirstOrDefault(e => e.id == id);
                        if (nonProcessedFile != null)
                        {
                            db.NonProcessedFiles.DeleteObject(nonProcessedFile);
                            db.SaveChanges();
                        }
                    }
                    return RedirectToAction("listNotProcessedFiles", "Management", new { actionToTake = 1 });
                }
                else return RedirectToAction("listNotProcessedFiles", "Management", new { actionToTake = 2 });
            }
            catch (Exception ex)
            {
                return RedirectToAction("listNotProcessedFiles", "Management", new { actionToTake = 3 });
            }
        }

        /************************* these methods were used at the beginning of the project to fill the customers, products, etc. ****************/
        public void setIdProductInProductFiles()
        {
            DateTime ini = DateTime.Now;
            Debug.WriteLine("Starting setIdProductInProductFiles");
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            {
                IEnumerable<ProductsFiles> files = db.ProductsFiles.Where(p => p.idProduct == null).ToList();
                foreach (var file in files)
                {
                    Products product = db.Products.FirstOrDefault(p => p.refNumber == file.refNumber);
                    if (product != null)
                    {
                        ProductsFiles editFile = db.ProductsFiles.FirstOrDefault(p => p.id == file.id);
                        editFile.idProduct = product.id;
                        //db.ProductsFiles.Attach(editFile);
                        db.ObjectStateManager.ChangeObjectState(editFile, EntityState.Modified);
                    }
                }
                db.SaveChanges();
            }
            Debug.WriteLine("Time for doing setIdProductInProductFiles ={0} ", DateTime.Now - ini);
        }

        [HttpGet]
        public ActionResult importCustomers()
        {
            return View();
        }

        [LogFilter]
        [HttpPost]
        public ActionResult importCustomers(ImportProductsViewModel model)
        {
            ImportResult result = new ImportResult();
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
                        using (repoCustomers repCustomers = new repoCustomers())
                        {
                            if (dt.Rows.Count > 0)
                            {
                                List<Customers> bulk = new List<Customers>();
                                foreach (DataRow item in dt.Rows)
                                {
                                    string nameInCSV = (string)item[0];
                                    string countryInCSV = (string)item[1];
                                    Customers row = repCustomers.GetByName(nameInCSV);
                                    if (row == null)                                    
                                    {
                                        row = new Customers();
                                        GTPTrackerEntities db = new GTPTrackerEntities();
                                        Countries country = db.Countries.FirstOrDefault(p => p.Name == countryInCSV);
                                        if (country == null)
                                        {
                                            result.FailCount++;
                                            result.result.Add(new ImportResultRow
                                                {
                                                    RefNumber = countryInCSV,
                                                    Message = "Country "+ countryInCSV + " did not exists on db " + nameInCSV
                                                });
                                        }
                                        else
                                        {
                                            row.idCountry = country.id;
                                            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                                            var random = new Random();
                                            var randomResult = new string(
                                                Enumerable.Repeat(chars, 10)
                                                          .Select(s => s[random.Next(s.Length)])
                                                          .ToArray());
                                            row.linkID = randomResult;
                                            row.name = nameInCSV;
                                            row.active = true;
                                            row.cDate = DateTime.Today;
                                            row.userAgreementSigned = false;

                                            bulk.Add(row);
                                            result.SuccessCount++;
                                        }
                                    }
                                }
                                if (repCustomers.SaveBulk(bulk))
                                {
                                    result.Success = true;
                                    result.Message = bulk.Count().ToString() + " Customers saved successfully";
                                }
                                else
                                {
                                    result.Success = false;
                                    result.Message = "Error while saving customers, please contact the administrator";
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
                    }
                    catch (Exception ex)
                    {
                        result.Success = false;
                        result.Message = ex.Message;
                    }
                }
            }

            ViewBag.ImportResult = result;
            return View();
        }


        [HttpGet]
        public ActionResult importSeries()
        {
            return View();
        }

        [LogFilter]
        [HttpPost]
        public ActionResult importSeries(ImportProductsViewModel model)
        {
            ImportResult result = new ImportResult();
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
                        using (repoSeries repSeries = new repoSeries())
                        {
                            if (dt.Rows.Count > 0)
                            {
                                List<Series> bulk = new List<Series>();
                                foreach (DataRow item in dt.Rows)
                                {
                                    string nameInCSV = (string)item[0];
                                    string categoryInCSV = (string)item[1];
                                    string serieInCSV = (string)item[2];
                                    if (categoryInCSV != "" && categoryInCSV != null)
                                    {
                                        Series row = repSeries.GetSerieBySerieAndCategory(Int32.Parse(serieInCSV), Int32.Parse(categoryInCSV));
                                        if (row == null)
                                        {
                                            row = new Series();
                                            //GTPTrackerEntities db = new GTPTrackerEntities();
                                            row.text = nameInCSV;
                                            row.serie = Int32.Parse(serieInCSV);
                                            row.category = Int32.Parse(categoryInCSV);
                                            row.imageFileName = "gtp-icon-02a-256.png";
                                            bulk.Add(row);
                                            result.SuccessCount++;
                                        }
                                    }
                                }
                                if (repSeries.SaveBulk(bulk))
                                {
                                    result.Success = true;
                                    result.Message = bulk.Count().ToString() + " Series Saved successfully";
                                }
                                else
                                {
                                    result.Success = false;
                                    result.Message = "Error while saving series, please contact the administrator";
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
                    }
                    catch (Exception ex)
                    {
                        result.Success = false;
                        result.Message = ex.Message;
                    }
                }
            }

            ViewBag.ImportResult = result;
            return View();
        }

        [HttpGet]
        public ActionResult importCustomersProducts()
        {
            return View();
        }

        [LogFilter]
        [HttpPost]
        public ActionResult importCustomersProducts(ImportProductsViewModel model)
        {
            ImportResult result = new ImportResult();
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
                        using (repoCustomers repCustomers = new repoCustomers())
                        using (repoProducts repProducts = new repoProducts())
                        {
                            if (dt.Rows.Count > 0)
                            {
                                List<CustomerProducts> bulk = new List<CustomerProducts>();
                                foreach (DataRow item in dt.Rows)
                                {
                                    string companyInCSV = (string)item[0];
                                    string refInCSV = (string)item[1];
                                    Customers customer = repCustomers.GetByName(companyInCSV);
                                    if (customer == null)
                                    {
                                        result.FailCount++;
                                        result.result.Add(new ImportResultRow
                                        {
                                            RefNumber = companyInCSV,
                                            Message = "Company did not exists on db."
                                        });
                                    }
                                    else {
                                        Products product = repProducts.GetByRefNumber(refInCSV);
                                        if (product == null)
                                        {
                                            result.FailCount++;
                                            result.result.Add(new ImportResultRow
                                            {
                                                RefNumber = refInCSV,
                                                Message = "Product did not exists on db."
                                            });
                                        }
                                        else // customer and products exists
                                        {
                                            using (var db = new GTPTrackerEntities())
                                            {
                                                CustomerProducts row = db.CustomerProducts.FirstOrDefault(p => p.idCustomer == customer.id && p.idProduct == product.id);
                                                if (row == null)
                                                {
                                                    row = new CustomerProducts();
                                                    row.cDate = DateTime.Today;
                                                    row.idCreator = (int)Session["IDUSER"];
                                                    row.idCustomer = customer.id;
                                                    row.idProduct = product.id;

                                                    bulk.Add(row);
                                                    result.SuccessCount++;
                                                }
                                            }
                                        }
                                    }
                                    
                                }
                                if (repCustomers.SaveCustomersProductsBulk(bulk))
                                {
                                    result.Success = true;
                                    result.Message = bulk.Count().ToString() + " CustomerProducts Saved successfully";
                                }
                                else
                                {
                                    result.Success = false;
                                    result.Message = "Error while saving CustomerProducts, please contact the administrator";
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
                    }
                    catch (Exception ex)
                    {
                        result.Success = false;
                        result.Message = ex.Message;
                    }
                }
            }

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
            catch 
            {
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
          //  Regex r = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            Regex r = new Regex(";(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            StreamReader sr = new StreamReader(fileName, Encoding.Default, true);
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

   /*     public void UpdateFromTodos()
        {
            GTPTrackerEntities db = new GTPTrackerEntities();
            IEnumerable<Todos> origin = db.Todos;
            foreach (Todos input in origin.Where(p => p.category != null && p.serie != null)) // read from eocrisers the one from the excel. Remove the ones without category and serie because are blank lines
            {
                string value = input.refNumber;
                Products exists = db.Products.Where(p => p.refNumber == value).FirstOrDefault();
                if (exists == null) // did not exists, so create it on series
                {
                    Products target = new Products();
                    target.serie = (int)input.serie;
                    target.category = (int)input.category;
                    target.refNumber = input.refNumber.ToString();
                    target.type = input.type;
                    target.volume = input.volume;
                    target.modulus = input.modulus;
                    target.Contact_area = input.contact_area;
                    target.Outer_diameter = input.outer_diameter;
                    target.Height = input.height;
                    target.showInCatalog = (input.specific_client == null);
                    db.Products.AddObject(target);
                    Debug.WriteLine("Adding new product : {0}", target.type);
                }
                else
                {
                    exists.serie = (int) input.serie;
                    exists.category = (int)input.category;
                    exists.refNumber = input.refNumber.ToString();
                    exists.type = input.type;
                    exists.volume = input.volume;
                    exists.modulus = input.modulus;
                    exists.Contact_area = input.contact_area;
                    exists.Outer_diameter = input.outer_diameter;
                    exists.Height = input.height;
                    db.Products.ApplyCurrentValues(exists);
                }
            }
            db.SaveChanges();
            Debug.WriteLine("End");
        }*/
        
    }
}
