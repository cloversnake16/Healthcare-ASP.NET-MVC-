using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace GTPTracker.VM // NOT USED
{
    public class FirstSerie
    {
        public int totalCount = 0;

        public string role;
        public int? tenant;

        public IEnumerable<Products> lProducts;
        public List<ProductsFiles> lFiles = new List<ProductsFiles>();
        public Series serie;
        public IEnumerable<Products> lMyProducts;

        repoSeries repSerie = new repoSeries();
        repoProducts repProducts = new repoProducts();

        public List<string> lModulus = new List<string>();
        public List<string> lVolumes = new List<string>();
        public List<string> lHeights = new List<string>();
        public List<Products> lOuterDiameters = new List<Products>();
        public List<Products> lContactAreas = new List<Products>();
        

        public IEnumerable<vCustomers> lTenants;

        public string filterSpecific;
        public string filterModulus;
        public string filterVolume;
        public string filterHeight;
        public string filterOuterDiameter;
        public string filterContactArea;
        public string filterGeneral;
        public string filterModulusFrom;
        public string filterVolumeFrom;
        public string filterHeightFrom;
        public string filterOuterDiameterFrom;
        public string filterContactAreaFrom;
        public string filterModulusTo;
        public string filterVolumeTo;
        public string filterHeightTo;
        public string filterOuterDiameterTo;
        public string filterContactAreaTo;

        public bool filterLastProducts;

        public string selectModulusFrom;
        public string selectModulusTo;
        public string selectVolumeFrom;
        public string selectVolumeTo;
        public string selectHeightFrom;
        public string selectHeightTo;
        public string selectOuterFrom;
        public string selectOuterTo;
        public string selectContactFrom;
        public string selectContactTo;

        public bool includePublicProductsValue;
        public bool canShowIncludePublicProducts = false;

        public static bool IsNumeric(object value)
            {
                double d;
                if (value == null) return false;
                if (Double.TryParse(value.ToString(), out d)) // if done, then is a number
                    return true;
                else return false;
            }

        private class SemiNumericComparer : IComparer<string>
        {
            static CultureInfo culture = new CultureInfo("es-ES");
            public int Compare(string s1, string s2)
            {
                if (IsNumeric(s1) && IsNumeric(s2))
                {
                    if (Convert.ToDouble(s1) > Convert.ToDouble(s2)) return 1;
                    if (Convert.ToDouble(s1) < Convert.ToDouble(s2)) return -1;
                    if (Convert.ToDouble(s1) == Convert.ToDouble(s2)) return 0;
                }
                if (IsNumeric(s1) && !IsNumeric(s2))
                    return -1;
                if (!IsNumeric(s1) && IsNumeric(s2))
                    return 1;
                return string.Compare(s1, s2, true);
            }            
        }

        public FirstSerie(int? category, int? idTenant, string modulusFrom, string modulusTo, string volumeFrom, string volumeTo, string heightFrom, string heightTo, string outerDiameterFrom, string outerDiameterTo, string contactAreaFrom, string contactAreaTo, string specific, string general, Users user = null, string includePublicProducts = "off")
        {
            DateTime ini = DateTime.Now;
            if (includePublicProducts.ToLower() == "true") this.includePublicProductsValue = true;
            else this.includePublicProductsValue = false;
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            {
                if (category != null) serie = repSerie.getList().OrderBy(p => p.serie).OrderBy(p => p.category).Where(p => p.category == category).FirstOrDefault();
                else serie = repSerie.getList().OrderBy(p => p.serie).OrderBy(p => p.category).FirstOrDefault();
                if (general != null) serie = repSerie.getList().OrderBy(p => p.serie).OrderBy(p => p.category).FirstOrDefault(p => (p.applications != null && p.applications.ToLower().Contains(general.ToLower()))
                                                             || (p.characteristics != null && p.characteristics.ToLower().Contains(general.ToLower())) || (p.text != null && p.text.ToLower().Contains(general.ToLower())));

                Debug.WriteLine("Time for calculating the Series ={0} ", DateTime.Now - ini);
                ini = DateTime.Now;
                IEnumerable<Products> lAllProducts = repProducts.getAll().Where(p=>p.idSerie == serie.id);

                if (idTenant != null && user.businessRole != "Customer") // I'm an admin using the dropdown and the checkbox
                {
                    canShowIncludePublicProducts = true;
                    if (includePublicProducts.ToLower() == "true") lProducts = lAllProducts.Where(p => p.status == "public"); //  I want all the product
                    else
                    {
                        IEnumerable<int> lMyPublicProductsInt = db.CustomerProducts.Where(p => p.idCustomer == idTenant).Select(p => p.idProduct).ToList();
                        lProducts = lAllProducts.Where(p => lMyPublicProductsInt.Contains(p.id) && p.status == "public");
                        //lProducts = Enumerable.Empty<Products>(); // I want only the assigned to the customer (default)
                    }
                }
                else lProducts = lAllProducts.Where(p => p.status == "public");

                IEnumerable<ProductsFiles> lFilesAux ;
                IEnumerable<string> lProductsRef = Enumerable.Empty<string>();
                IEnumerable<string> lMyProductsRef = Enumerable.Empty<string>();

                Debug.WriteLine("Time for doing the real filter ={0} ", DateTime.Now - ini);
                ini = DateTime.Now;

                if (user != null) // check permission. 
                {
                    IEnumerable<int> lMyProductsInt = Enumerable.Empty<int>();
                    if (user.seeCustomProducts == true)
                    {
                        if (idTenant != null)
                        {
                            lMyProductsInt = db.CustomerProducts.Where(p => p.idCustomer == idTenant).Select(p => p.idProduct).ToList();
                            lMyProducts = lAllProducts.Where(p => lMyProductsInt.Contains(p.id));
                            IEnumerable<Products> lCustomProducts = lAllProducts.Where(p => p.status == "custom").Where(p => lMyProductsInt.Contains(p.id));
                            lMyProductsRef = lCustomProducts.Select(p => p.refNumber);                            
                            lProducts = lProducts.Concat(lCustomProducts);
                        }
                        else
                        {
                            lMyProductsInt = db.CustomerProducts.Select(p => p.idProduct).ToList(); // I'm an admin so I want all the custom products.
                            //lMyProducts = lAllProducts.Where(p => lMyProductsInt.Contains(p.id));

                            IEnumerable<Products> lMyCustomProducts = lAllProducts.Where(p => p.status == "custom").Where(p => lMyProductsInt.Contains(p.id));
                            lMyProductsRef = lMyCustomProducts.Select(p=>p.refNumber);
                            lProducts = lProducts.Concat(lMyCustomProducts).ToList();
                        }

                        Debug.WriteLine("Time for calculating the Products ={0} ", DateTime.Now - ini);
                        ini = DateTime.Now;
                    }


                    //if (user.seeDraftProducts == true)
                    if (user.businessRole == "GTP General Manager" || user.businessRole == "GTP KAM")                                
                    {
                        if (idTenant == null)
                            lProducts = lProducts.Concat(lAllProducts.Where(p => p.status == "draft"));
                    }

                    lProductsRef = lProducts.Select(p => p.refNumber);

                    Debug.WriteLine("Files Step1 ={0} ", DateTime.Now - ini);
                    ini = DateTime.Now;

                   // lFilesAux = repProducts.getFilesOfThisProducts(lProductsRef);
                    IEnumerable<int> lProductsIds = lProducts.Select(p => p.id);
                    lFilesAux = repProducts.getFilesOfThisProductsById(lProductsIds);

                    Debug.WriteLine("Files Step2 ={0} ", DateTime.Now - ini);
                    ini = DateTime.Now;

                    // filter lFiles depending on the permissions
                    if (user.download2d == true)
                    {
                        foreach (var item in lFilesAux)
                            if (Path.GetExtension(item.fileName).ToLower() == ".pdf" && item.internalGTP == false)
                                lFiles.Add(item);
                    }
                    if (user.download3d == true)
                    {
                        foreach (var item in lFilesAux)
                            if (Path.GetExtension(item.fileName).ToLower() != ".pdf" && item.internalGTP == false)
                                if (user.businessRole == "Customer")
                                {
                                    lMyProductsRef = lMyProducts.Select(p => p.refNumber);
                                    if (item.refNumber != null && lMyProductsRef.Contains(item.refNumber))
                                        lFiles.Add(item);
                                }
                                else lFiles.Add(item);
                    }
                    if (user.downloadInternalFiles == true)
                    {
                        foreach (var item in lFilesAux.Where(p => p.internalGTP == true))
                            lFiles.Add(item);
                    }

                    Debug.WriteLine("Time for calculating the Files ={0} ", DateTime.Now - ini);

                    role = user.businessRole;
                    if (user.businessRole == "GTP General Manager")
                    {
                        using (repoCustomers repCustomers = new repoCustomers())
                            lTenants = repCustomers.getList().OrderBy(p=>p.name);
                    }
                    if (user.businessRole == "GTP KAM")
                    {
                        using (repoCustomers repCustomers = new repoCustomers())
                            lTenants = repCustomers.getvByKAM(user.id).OrderBy(p=>p.name);
                    }
                }
                else
                {                    
                    //if user == null the user is unathenticated
                    lProductsRef = lProducts.Select(p => p.refNumber);
                    lFilesAux = repProducts.getFilesOfThisProducts(lProductsRef);
                    foreach (var item in lFilesAux)
                        if (Path.GetExtension(item.fileName).ToLower() == ".pdf" && item.internalGTP == false)
                            lFiles.Add(item);
                    role = null;
                }
                tenant = idTenant;
                // by default the dropdowns will have all the possible values.
                lModulus = lProducts.Where(p => p.modulus != null).OrderBy(x => x.modulus, new SemiNumericComparer()).Select(p => p.modulus).Distinct().ToList();
                lVolumes = lProducts.Where(p => p.volume != null).OrderBy(x => x.volume, new SemiNumericComparer()).Select(p => p.volume).Distinct().ToList();
                lHeights = lProducts.Where(p => p.Height != null).OrderBy(p => p.Height, new SemiNumericComparer()).Select(p => p.Height).Distinct().ToList();
                lOuterDiameters = lProducts.Where(p => p.outer1 != null).OrderBy(p => p.outer1, new SemiNumericComparer()).Distinct().ToList();
                lContactAreas = lProducts.Where(p => p.contact1 != null).OrderBy(p => p.contact1, new SemiNumericComparer()).Distinct().ToList();

                if (modulusFrom != "All" && modulusFrom != null && modulusTo != "All" && modulusTo != null) lProducts = lProducts.Where(p => p.modulus != null && p.modulus != "" && float.Parse(p.modulus) >= float.Parse(modulusFrom) && float.Parse(p.modulus) <= float.Parse(modulusTo));
                else if ((modulusFrom == "All" || modulusFrom == null) && modulusTo != "All" && modulusTo != null) lProducts = lProducts.Where(p => p.modulus != null && p.modulus != "" && float.Parse(p.modulus) <= float.Parse(modulusTo));
                else if (modulusFrom != "All" && modulusFrom != null && (modulusTo == "All" || modulusTo == null)) lProducts = lProducts.Where(p => p.modulus != null && p.modulus != "" && float.Parse(p.modulus) >= float.Parse(modulusFrom));

                if (volumeFrom != "All" && volumeFrom != null && volumeTo != "All" && volumeTo != null) lProducts = lProducts.Where(p => p.volume != null && p.volume != "" && float.Parse(p.volume) >= float.Parse(volumeFrom) && float.Parse(p.volume) <= float.Parse(volumeTo));
                else if ((volumeFrom == "All" || volumeFrom == null) && volumeTo != "All" && volumeTo != null) lProducts = lProducts.Where(p => p.volume != null && p.volume != "" && float.Parse(p.volume) <= float.Parse(volumeTo));
                else if (volumeFrom != "All" && volumeFrom != null && (volumeTo == "All" || volumeTo == null)) lProducts = lProducts.Where(p => p.volume != null && p.volume != "" && float.Parse(p.volume) >= float.Parse(volumeFrom));

                if (heightFrom != "All" && heightFrom != null && heightTo != "All" && heightTo != null) lProducts = lProducts.Where(p => p.Height != null && p.Height != "" && float.Parse(p.Height) >= float.Parse(heightFrom) && float.Parse(p.Height) <= float.Parse(heightTo));
                else if ((heightFrom == "All" || heightFrom == null) && heightTo != "All" && heightTo != null) lProducts = lProducts.Where(p => p.Height != null && p.Height != "" && float.Parse(p.Height) <= float.Parse(heightTo));
                else if (heightFrom != "All" && heightFrom != null && (heightTo == "All" || heightTo == null)) lProducts = lProducts.Where(p => p.Height != null && p.Height != "" && float.Parse(p.Height) >= float.Parse(heightFrom));

                if (outerDiameterFrom != "All" && outerDiameterFrom != null && outerDiameterTo != "All" && outerDiameterTo != null) lProducts = lProducts.Where(p => p.outer1 != null && p.outer1 != "" && float.Parse(p.outer1) >= float.Parse(outerDiameterFrom) && float.Parse(p.outer1) <= float.Parse(outerDiameterTo));
                else if ((outerDiameterFrom == "All" || outerDiameterFrom == null) && outerDiameterTo != "All" && outerDiameterTo != null) lProducts = lProducts.Where(p => p.outer1 != null && p.outer1 != "" && float.Parse(p.outer1) <= float.Parse(outerDiameterTo));
                else if (outerDiameterFrom != "All" && outerDiameterFrom != null && (outerDiameterTo == "All" || outerDiameterTo == null)) lProducts = lProducts.Where(p => p.outer1 != null && p.outer1 != "" && float.Parse(p.outer1) >= float.Parse(outerDiameterFrom));

                if (contactAreaFrom != "All" && contactAreaFrom != null && contactAreaTo != "All" && contactAreaTo != null) lProducts = lProducts.Where(p => p.contact1 != null && p.contact1 != "" && float.Parse(p.contact1) >= float.Parse(contactAreaFrom) && float.Parse(p.contact1) <= float.Parse(contactAreaTo));
                else if ((contactAreaFrom == "All" || contactAreaFrom == null) && contactAreaTo != "All" && contactAreaTo != null) lProducts = lProducts.Where(p => p.contact1 != null && p.contact1 != null && float.Parse(p.contact1) <= float.Parse(contactAreaTo));
                else if (contactAreaFrom != "All" && contactAreaFrom != null && (contactAreaTo == "All" || contactAreaTo == null)) lProducts = lProducts.Where(p => p.contact1 != null && p.contact1 != null && float.Parse(p.contact1) >= float.Parse(contactAreaFrom));

                if (specific != null) lProducts = lProducts.Where(p => p.type.ToLower().Contains(specific.ToLower()));                

                // Filters ********************************************** Order the values of the fields to be showed on the selects of the view
                if (lProducts.Count() > 0) // if there are products in the filter, adapt the dropdowns on the view
                {
                    lModulus = lProducts.Where(p => p.modulus != null).OrderBy(x => x.modulus, new SemiNumericComparer()).Select(p => p.modulus).Distinct().ToList();
                    lVolumes = lProducts.Where(p => p.volume != null).OrderBy(x => x.volume, new SemiNumericComparer()).Select(p => p.volume).Distinct().ToList();
                    lHeights = lProducts.Where(p => p.Height != null).OrderBy(p => p.Height, new SemiNumericComparer()).Select(p => p.Height).Distinct().ToList();
                    lOuterDiameters = lProducts.Where(p => p.outer1 != null).OrderBy(p => p.outer1, new SemiNumericComparer()).Distinct().ToList();
                    lContactAreas = lProducts.Where(p => p.contact1 != null).OrderBy(p => p.contact1, new SemiNumericComparer()).Distinct().ToList();
                    // to avoid setting "All" in some searches instead of the selected filter, if not exists add manually the selected value to the list
                    
                    if (modulusFrom!=null && modulusFrom != "All" && lModulus.FirstOrDefault(p => p.Contains(modulusFrom)) == null) lModulus.Add(modulusFrom);
                    if (modulusTo != null && modulusTo != "All" && lModulus.FirstOrDefault(p => p.Contains(modulusTo)) == null) lModulus.Add(modulusTo);
                    if (volumeFrom != null && volumeFrom != "All" && lVolumes.FirstOrDefault(p => p.Contains(volumeFrom)) == null) lVolumes.Add(volumeFrom);
                    if (volumeTo != null && volumeTo != "All" && lVolumes.FirstOrDefault(p => p.Contains(volumeTo)) == null) lVolumes.Add(volumeTo);
                    if (heightFrom != null && heightFrom != "All" && lHeights.FirstOrDefault(p => p.Contains(heightFrom)) == null) lVolumes.Add(heightFrom);
                    if (heightTo != null && heightTo != "All" && lHeights.FirstOrDefault(p => p.Contains(heightTo)) == null) lVolumes.Add(heightTo);

                    lModulus = lModulus.OrderBy(p => p, new SemiNumericComparer()).ToList();
                    lVolumes = lVolumes.OrderBy(p => p, new SemiNumericComparer()).ToList();
                    lHeights = lHeights.OrderBy(p => p, new SemiNumericComparer()).ToList();
                }
                

                var dContacts = lContactAreas.Select(m => new { m.contact1, m.contact2 }).Distinct().ToList();
                var dOuters = lOuterDiameters.Select(m => new { m.outer1, m.outer2 }).Distinct().ToList();

                Debug.WriteLine("Time for getting the list of modulus, volumes, etc ={0} ", DateTime.Now - ini);
                ini = DateTime.Now;

                // create the select options values. In the view will be injected into the select using html.raw
                filterContactAreaFrom = contactAreaFrom;
                filterGeneral = general;
                filterHeightFrom = heightFrom;
                filterModulusFrom = modulusFrom;
                filterOuterDiameterFrom = outerDiameterFrom;
                filterSpecific = specific;
                filterVolumeFrom = volumeFrom;
                filterContactAreaTo = contactAreaTo;
                filterHeightTo = heightTo;
                filterModulusTo = modulusTo;
                filterOuterDiameterTo = outerDiameterTo;
                filterVolumeTo = volumeTo;

                foreach (var item in lModulus)
                {
                    if (item != filterModulusFrom)
                        selectModulusFrom += HttpUtility.HtmlDecode("<option value=\"" + item + "\">" + item + "</option>");
                    if (item == filterModulusFrom)
                        selectModulusFrom += HttpUtility.HtmlDecode("<option selected value=\"" + item + "\">" + item + "</option>");
                    if (item != filterModulusTo)
                        selectModulusTo += HttpUtility.HtmlDecode("<option value=\"" + item + "\">" + item + "</option>");
                    if (item == filterModulusTo)
                        selectModulusTo += HttpUtility.HtmlDecode("<option selected value=\"" + item + "\">" + item + "</option>");
                }
                Debug.WriteLine("Time for calculating the text of the select Modulus ={0} ", DateTime.Now - ini);
                ini = DateTime.Now;
                foreach (var item in lVolumes)
                {
                    if (item != filterVolumeFrom)
                        selectVolumeFrom += HttpUtility.HtmlDecode("<option value=\"" + item + "\">" + item + "</option>");
                    if (item == filterVolumeFrom)
                        selectVolumeFrom += HttpUtility.HtmlDecode("<option selected value=\"" + item + "\">" + item + "</option>");
                    if (item != filterVolumeTo)
                        selectVolumeTo += HttpUtility.HtmlDecode("<option value=\"" + item + "\">" + item + "</option>");
                    if (item == filterVolumeTo)
                        selectVolumeTo += HttpUtility.HtmlDecode("<option selected value=\"" + item + "\">" + item + "</option>");
                }
                Debug.WriteLine("Time for calculating the text of the select Volume ={0}", DateTime.Now - ini);
                ini = DateTime.Now;
                foreach (var item in lHeights)
                {
                    if (item != filterHeightFrom)
                        selectHeightFrom += HttpUtility.HtmlDecode("<option value=\"" + item + "\">" + item + "</option>");
                    if (item == filterHeightFrom)
                        selectHeightFrom += HttpUtility.HtmlDecode("<option selected value=\"" + item + "\">" + item + "</option>");
                    if (item != filterHeightTo)
                        selectHeightTo += HttpUtility.HtmlDecode("<option value=\"" + item + "\">" + item + "</option>");
                    if (item == filterHeightTo)
                        selectHeightTo += HttpUtility.HtmlDecode("<option selected value=\"" + item + "\">" + item + "</option>");
                }
                Debug.WriteLine("Time for calculating the text of the select Heights ={0} ", DateTime.Now - ini);
                ini = DateTime.Now;
                foreach (var item in dOuters)
                {
                    if (item.outer1 != filterOuterDiameterFrom)
                        selectOuterFrom += HttpUtility.HtmlDecode("<option value=\"" + item.outer1 + "\">" + Helpers.formatter.getContact(item.outer1, item.outer2) + "</option>");
                    if (item.outer1 == filterOuterDiameterFrom)
                        selectOuterFrom += HttpUtility.HtmlDecode("<option selected value=\"" + item.outer1 + "\">" + Helpers.formatter.getContact(item.outer1, item.outer2) + "</option>");
                    if (item.outer1 != filterOuterDiameterTo)
                        selectOuterTo += HttpUtility.HtmlDecode("<option value=\"" + item.outer1 + "\">" + Helpers.formatter.getContact(item.outer1, item.outer2) + "</option>");
                    if (item.outer1 == filterOuterDiameterTo)
                        selectOuterTo += HttpUtility.HtmlDecode("<option selected value=\"" + item.outer1 + "\">" + Helpers.formatter.getContact(item.outer1, item.outer2) + "</option>");
                }
                Debug.WriteLine("Time for generating Outerdiameter ={0} ", DateTime.Now - ini);
                ini = DateTime.Now;
                foreach (var item in dContacts)
                {
                    if (item.contact1 != filterContactAreaFrom)
                        selectContactFrom += HttpUtility.HtmlDecode("<option value=\"" + item.contact1 + "\">" + Helpers.formatter.getContact(item.contact1, item.contact2) + "</option>");
                    if (item.contact1 == filterContactAreaFrom)
                        selectContactFrom += HttpUtility.HtmlDecode("<option selected value=\"" + item.contact1 + "\">" + Helpers.formatter.getContact(item.contact1, item.contact2) + "</option>");
                    if (item.contact1 != filterContactAreaTo)
                        selectContactTo += HttpUtility.HtmlDecode("<option value=\"" + item.contact1 + "\">" + Helpers.formatter.getContact(item.contact1, item.contact2) + "</option>");
                    if (item.contact1 == filterContactAreaTo)
                        selectContactTo += HttpUtility.HtmlDecode("<option selected value=\"" + item.contact1 + "\">" + Helpers.formatter.getContact(item.contact1, item.contact2) + "</option>");
                }

                Debug.WriteLine("Time for calculating the text of the select ContactArea ={0} ", DateTime.Now - ini);
                ini = DateTime.Now;
                lProducts = lProducts.OrderBy(p => p.id);
                totalCount = lProducts.Count();
            }
        }
    }
}