using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;
using System.Diagnostics;
using System.Globalization;

namespace GTPTracker.VM
{
    public class MyProductsVM
    {
        public IEnumerable<ProductsFiles> lFiles;
        public IEnumerable<Series> lSeries;
        public IEnumerable<Products> lMyProducts;

        repoSeries repSerie = new repoSeries();
        repoProducts repProducts = new repoProducts();

        public List<string> lModulus= new List<string>();
        public List<string> lVolumes = new List<string>();
        public List<string> lHeights = new List<string>();
        public List<string> lOuterDiameters = new List<string>();
        public List<string> lContactAreas =  new List<string>();

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

        private IEnumerable<Products> removeStrangeProducts()
        {
            double valor = 0;
            var result = lMyProducts.Where(str => double.TryParse(str.Outer_diameter, out valor) && double.TryParse(str.Contact_area, out valor));
            return result;
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

            public static bool IsNumeric(object value)
            {
                double d;
                if (value == null) return false;
                if (Double.TryParse(value.ToString(), out d)) // if done, then is a number
                    return true;
                else return false;
            }
        }

        public MyProductsVM(int? category, int idTenant, string modulusFrom, string modulusTo, string volumeFrom, string volumeTo, string heightFrom, string heightTo, string outerDiameterFrom, string outerDiameterTo, string contactAreaFrom, string contactAreaTo, string specific, string general, bool lastProducts = false)
        {
            DateTime ini = DateTime.Now;
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            {
                if (category != null) lSeries = repSerie.getList().Where(p => p.category == category);
                else lSeries = repSerie.getList();
                if (general != null) lSeries = lSeries.Where(p => (p.applications != null && p.applications.ToLower().Contains(general.ToLower()))
                                                             || (p.characteristics != null && p.characteristics.ToLower().Contains(general.ToLower())) || (p.text != null && p.text.ToLower().Contains(general.ToLower())));

                lSeries = lSeries.OrderBy(p => p.serie).OrderBy(p => p.category);
                Debug.WriteLine("Time for calculating the Series of MyProducts ={0} ", DateTime.Now - ini);
                ini = DateTime.Now;

                IEnumerable<int> lMyProductsInt = db.CustomerProducts.Where(p => p.idCustomer == idTenant).Select(p => p.idProduct).ToList();
                lMyProducts = repProducts.getAll().Where(p => lMyProductsInt.Contains(p.id));

                // Filters ********************************************** Order the values of the fields to be showed on the selects of the view
                lModulus = lMyProducts.Where(p => p.modulus != null).OrderBy(x => x.modulus, new SemiNumericComparer()).Select(p => p.modulus).Distinct().ToList();
                lVolumes = lMyProducts.Where(p => p.volume != null).OrderBy(x => x.volume, new SemiNumericComparer()).Select(p => p.volume).Distinct().ToList();
                lHeights = lMyProducts.Where(p => p.Height != null).OrderBy(p => p.Height, new SemiNumericComparer()).Select(p => p.Height).Distinct().ToList();
                lOuterDiameters = lMyProducts.Where(p => p.outer1 != null).OrderBy(p => p.outer1, new SemiNumericComparer()).Select(p => p.outer1).Distinct().ToList();
                lContactAreas = lMyProducts.Where(p => p.contact1 != null).OrderBy(p => p.contact1, new SemiNumericComparer()).Select(p => p.contact1).Distinct().ToList();

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

                if (modulusFrom != "All" && modulusFrom != null && modulusTo != "All" && modulusTo != null) lMyProducts = lMyProducts.Where(p => p.modulus != null && p.modulus != "" && float.Parse(p.modulus) >= float.Parse(modulusFrom) && float.Parse(p.modulus) <= float.Parse(modulusTo));
                else if ((modulusFrom == "All" || modulusFrom == null) && modulusTo != "All" && modulusTo != null) lMyProducts = lMyProducts.Where(p => p.modulus != null && p.modulus != "" && float.Parse(p.modulus) <= float.Parse(modulusTo));
                else if (modulusFrom != "All" && modulusFrom != null && (modulusTo == "All" || modulusTo == null)) lMyProducts = lMyProducts.Where(p => p.modulus != null && p.modulus != "" && float.Parse(p.modulus) >= float.Parse(modulusFrom));

                if (volumeFrom != "All" && volumeFrom != null && volumeTo != "All" && volumeTo != null) lMyProducts = lMyProducts.Where(p => p.volume != null && p.volume != "" && float.Parse(p.volume) >= float.Parse(volumeFrom) && float.Parse(p.volume) <= float.Parse(volumeTo));
                else if ((volumeFrom == "All" || volumeFrom == null) && volumeTo != "All" && volumeTo != null) lMyProducts = lMyProducts.Where(p => p.volume != null && p.volume != "" && float.Parse(p.volume) <= float.Parse(volumeTo));
                else if (volumeFrom != "All" && volumeFrom != null && (volumeTo == "All" || volumeTo == null)) lMyProducts = lMyProducts.Where(p => p.volume != null && p.volume != "" && float.Parse(p.volume) >= float.Parse(volumeFrom));

                if (heightFrom != "All" && heightFrom != null && heightTo != "All" && heightTo != null) lMyProducts = lMyProducts.Where(p => p.Height != null && p.Height != "" && float.Parse(p.Height) >= float.Parse(heightFrom) && float.Parse(p.Height) <= float.Parse(heightTo));
                else if ((heightFrom == "All" || heightFrom == null) && heightTo != "All" && heightTo != null) lMyProducts = lMyProducts.Where(p => p.Height != null && p.Height != "" && float.Parse(p.Height) <= float.Parse(heightTo));
                else if (heightFrom != "All" && heightFrom != null && (heightTo == "All" || heightTo == null)) lMyProducts = lMyProducts.Where(p => p.Height != null && p.Height != "" && float.Parse(p.Height) >= float.Parse(heightFrom));

                if (outerDiameterFrom != "All" && outerDiameterFrom != null && outerDiameterTo != "All" && outerDiameterTo != null) lMyProducts = lMyProducts.Where(p => p.outer1 != null && p.outer1 != "" && float.Parse(p.outer1) >= float.Parse(outerDiameterFrom) && float.Parse(p.outer1) <= float.Parse(outerDiameterTo));
                else if ((outerDiameterFrom == "All" || outerDiameterFrom == null) && outerDiameterTo != "All" && outerDiameterTo != null) lMyProducts = lMyProducts.Where(p => p.outer1 != null && p.outer1 != "" && float.Parse(p.outer1) <= float.Parse(outerDiameterTo));
                else if (outerDiameterFrom != "All" && outerDiameterFrom != null && (outerDiameterTo == "All" || outerDiameterTo == null)) lMyProducts = lMyProducts.Where(p => p.outer1 != null && p.outer1 != "" && float.Parse(p.outer1) >= float.Parse(outerDiameterFrom));

                if (contactAreaFrom != "All" && contactAreaFrom != null && contactAreaTo != "All" && contactAreaTo != null) lMyProducts = lMyProducts.Where(p => p.contact1 != null && p.contact1 != "" && float.Parse(p.contact1) >= float.Parse(contactAreaFrom) && float.Parse(p.contact1) <= float.Parse(contactAreaTo));
                else if ((contactAreaFrom == "All" || contactAreaFrom == null) && contactAreaTo != "All" && contactAreaTo != null) lMyProducts = lMyProducts.Where(p => p.contact1 != null && p.contact1 != null && float.Parse(p.contact1) <= float.Parse(contactAreaTo));
                else if (contactAreaFrom != "All" && contactAreaFrom != null && (contactAreaTo == "All" || contactAreaTo == null)) lMyProducts = lMyProducts.Where(p => p.contact1 != null && p.contact1 != null && float.Parse(p.contact1) >= float.Parse(contactAreaFrom));

                if (specific != null) lMyProducts = lMyProducts.Where(p => p.type.ToLower().Contains(specific.ToLower()));

                if (lMyProducts.Count() > 0) // if there are products in the filter, adapt the dropdowns on the view
                {
                    lModulus = lMyProducts.Where(p => p.modulus != null).OrderBy(x => x.modulus, new SemiNumericComparer()).Select(p => p.modulus).Distinct().ToList();
                    lVolumes = lMyProducts.Where(p => p.volume != null).OrderBy(x => x.volume, new SemiNumericComparer()).Select(p => p.volume).Distinct().ToList();
                    lHeights = lMyProducts.Where(p => p.Height != null).OrderBy(p => p.Height, new SemiNumericComparer()).Select(p => p.Height).Distinct().ToList();
                    lOuterDiameters = lMyProducts.Where(p => p.outer1 != null).OrderBy(p => p.outer1, new SemiNumericComparer()).Select(p=>p.outer1).Distinct().ToList();
                    lContactAreas = lMyProducts.Where(p => p.contact1 != null).OrderBy(p => p.contact1, new SemiNumericComparer()).Select(p=>p.contact1).Distinct().ToList();
                    // to avoid setting "All" in some searches instead of the selected filter, if not exists add manually the selected value to the list

                    if (modulusFrom != null && modulusFrom != "All" && lModulus.FirstOrDefault(p => p.Contains(modulusFrom)) == null) lModulus.Add(modulusFrom);
                    if (modulusTo != null && modulusTo != "All" && lModulus.FirstOrDefault(p => p.Contains(modulusTo)) == null) lModulus.Add(modulusTo);
                    if (volumeFrom != null && volumeFrom != "All" && lVolumes.FirstOrDefault(p => p.Contains(volumeFrom)) == null) lVolumes.Add(volumeFrom);
                    if (volumeTo != null && volumeTo != "All" && lVolumes.FirstOrDefault(p => p.Contains(volumeTo)) == null) lVolumes.Add(volumeTo);
                    if (heightFrom != null && heightFrom != "All" && lHeights.FirstOrDefault(p => p.Contains(heightFrom)) == null) lVolumes.Add(heightFrom);
                    if (heightTo != null && heightTo != "All" && lHeights.FirstOrDefault(p => p.Contains(heightTo)) == null) lVolumes.Add(heightTo);

                    lModulus = lModulus.OrderBy(p => p, new SemiNumericComparer()).ToList();
                    lVolumes = lVolumes.OrderBy(p => p, new SemiNumericComparer()).ToList();
                    lHeights = lHeights.OrderBy(p => p, new SemiNumericComparer()).ToList();
                }

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
                foreach (var item in lOuterDiameters)
                {
                    if (item != filterOuterDiameterFrom)
                        selectOuterFrom += HttpUtility.HtmlDecode("<option value=\"" + item + "\">" + item + "</option>");
                    if (item == filterOuterDiameterFrom)
                        selectOuterFrom += HttpUtility.HtmlDecode("<option selected value=\"" + item + "\">" + item + "</option>");
                    if (item != filterOuterDiameterTo)
                        selectOuterTo += HttpUtility.HtmlDecode("<option value=\"" + item + "\">" + item + "</option>");
                    if (item == filterOuterDiameterTo)
                        selectOuterTo += HttpUtility.HtmlDecode("<option selected value=\"" + item + "\">" + item + "</option>");
                }
                foreach (var item in lContactAreas)
                {
                    if (item != filterContactAreaFrom)
                        selectContactFrom += HttpUtility.HtmlDecode("<option value=\"" + item + "\">" + item + "</option>");
                    if (item == filterContactAreaFrom)
                        selectContactFrom += HttpUtility.HtmlDecode("<option selected value=\"" + item + "\">" + item + "</option>");
                    if (item != filterContactAreaTo)
                        selectContactTo += HttpUtility.HtmlDecode("<option value=\"" + item + "\">" + item + "</option>");
                    if (item == filterContactAreaTo)
                        selectContactTo += HttpUtility.HtmlDecode("<option selected value=\"" + item + "\">" + item + "</option>");
                }


                Debug.WriteLine("Time for calculating the myProducts ={0} ", DateTime.Now - ini);
                ini = DateTime.Now;
                lFiles = repProducts.getFilesOfThisProducts(lMyProducts.Select(p=>p.refNumber));
                Debug.WriteLine("Time for calculating the Files of MyProducts ={0} ", DateTime.Now - ini);
            }
        }
    }
}