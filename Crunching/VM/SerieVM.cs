using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;
using System.Diagnostics;
using System.Globalization;

namespace GTPTracker.VM // NOT USED
{
    public class SerieVM
    {
        public Series serie;
        public IEnumerable<Products> lProducts;
        public IEnumerable<ProductsFiles> lFilesProducts;
        public IEnumerable<Products> lMyProducts;

        
        repoSeries repSerie = new repoSeries();
        repoProducts repProducts = new repoProducts();

        public SerieVM(int idSerie, int idTenant)
        {
            DateTime ini = DateTime.Now;
            serie = repSerie.get(idSerie);
            lProducts = repProducts.getListBySerie(idSerie).Where(p => p.status == "public");
            Debug.WriteLine("Time for calculating the Products ={0} ", DateTime.Now - ini);
            ini = DateTime.Now;
            if (idTenant == 1) // gtp
                lMyProducts = lProducts;
            else
            {
                lMyProducts = repProducts.getList(idTenant);
                lMyProducts = lMyProducts.Where(p => p.serie == serie.serie && p.category == serie.category);
            }
            Debug.WriteLine("Time for doing the real filter ={0} ", DateTime.Now - ini);
            ini = DateTime.Now;
            using (GTPTrackerEntities db= new GTPTrackerEntities()){
            lFilesProducts = db.ProductsFiles.ToList();
        }
            // these two list include an object composed by the product and the date that was assigned to that customer. If the customer is GTP the date is today
            Debug.WriteLine("Time for calculating the Files ={0} ", DateTime.Now - ini);
        }

     /*   public SerieVM(int idSerie, int idTenant, string modulus, string volume, string height, string outerDiameter, string contactArea, string specific)
        {

            DateTime ini = DateTime.Now;
            serie = repSerie.get(idSerie);
            lProducts = repProducts.getListBySerie(idSerie).Where(p => p.status == "public");

            Debug.WriteLine("Time for calculating the Products ={0} ", DateTime.Now - ini);
            ini = DateTime.Now;

            // Filter
            if (modulus != "All" && modulus != null) lProducts = lProducts.Where(p => p.modulus == modulus);
            if (volume != "All" && volume != null) lProducts = lProducts.Where(p => p.volume == volume);
            if (height != "All" && height != null) lProducts = lProducts.Where(p => p.Height == height);
            if (outerDiameter != "All" && outerDiameter != null) lProducts = lProducts.Where(p => p.Outer_diameter == modulus);
            if (contactArea != "All" && contactArea != null) lProducts = lProducts.Where(p => p.Contact_area == contactArea);
            if (specific != null) lProducts = lProducts.Where(p => p.type.ToLower().Contains(specific.ToLower()));
            if (idTenant == 1) // gtp
                lMyProducts = lProducts;
            else
            {
                lMyProducts = repProducts.getList(idTenant);
                lMyProducts = lMyProducts.Where(p => p.serie == serie.serie && p.category == serie.category);
            }
            Debug.WriteLine("Time for doing the real filter ={0} ", DateTime.Now - ini);
            ini = DateTime.Now;
            if (idTenant == 1) lFilesProducts = repProducts.getGTPFilesByIdSerie(idSerie);
            else lFilesProducts = repProducts.getFilesBySerie(serie.serie, serie.category);
            // these two list include an object composed by the product and the date that was assigned to that customer. If the customer is GTP the date is today
            Debug.WriteLine("Time for calculating the Files ={0} ", DateTime.Now - ini);

        }*/

        public class SemiNumericComparer : IComparer<string>
        {
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
                try
                {
                    CultureInfo culture = new CultureInfo("es-ES");
                    if (value == null) return false;
                    double i = Convert.ToDouble(value.ToString(),culture.NumberFormat);
                    return true;
                }
                catch (FormatException) { return false; }
            }
        }

        public SerieVM(int idSerie, int idTenant, string modulusFrom, string modulusTo, string volumeFrom, string volumeTo, string heightFrom, string heightTo, string outerDiameterFrom, string outerDiameterTo, string contactAreaFrom, string contactAreaTo, string specific, Series serieVal, IEnumerable<Products> lAllProducts)
        {
            
            DateTime start = DateTime.Now;
            DateTime startserie = DateTime.Now;
            Debug.WriteLine("__________________________");
            serie = serieVal;//repSerie.get(idSerie);
            using (GTPTrackerEntities db = new GTPTrackerEntities())
            {

                IEnumerable<int> lMyProductsInt = db.CustomerProducts.Where(p => p.idCustomer == idTenant).Select(p => p.idProduct).ToList();
                IEnumerable<Products> resultMyProducts = lAllProducts.Where(p => lMyProductsInt.Contains(p.id) && p.serie == serie.serie && p.category == serie.category); // db.Products.Where(p => lMyProductsInt.Contains(p.id) && p.serie == serieVal && p.category == categoryVal);

                IEnumerable<Products> lProducts;
                IEnumerable<vProductsFiles> lFilesProducts;
                IEnumerable<Products> lMyProducts;

                lProducts = lAllProducts.Where(p => p.category == serie.category && p.serie == serie.serie && p.showInCatalog == true).Concat(resultMyProducts).ToList(); //repProducts.getListBySerie(idSerie).Where(p => p.showInCatalog == true).Concat(resultMyProducts).ToList();
              //  lProducts = lAllProducts.Where(p => p.idSerie == idSerie && p.showInCatalog == true).Concat(resultMyProducts).ToList(); //repProducts.getListBySerie(idSerie).Where(p => p.showInCatalog == true).Concat(resultMyProducts).ToList();
                // Filter
                if (modulusFrom != "All" && modulusFrom != null && modulusTo != "All" && modulusTo != null) lProducts = lProducts.Where(p => p.modulus != null && float.Parse(p.modulus) >= float.Parse(modulusFrom) && float.Parse(p.modulus) <= float.Parse(modulusTo));
                else if ((modulusFrom == "All" || modulusFrom == null) && modulusTo != "All" && modulusTo != null) lProducts = lProducts.Where(p => p.modulus != null && float.Parse(p.modulus) <= float.Parse(modulusTo));
                else if (modulusFrom != "All" && modulusFrom != null && (modulusTo == "All" || modulusTo == null)) lProducts = lProducts.Where(p => p.modulus != null && float.Parse(p.modulus) >= float.Parse(modulusFrom));

                if (volumeFrom != "All" && volumeFrom != null && volumeTo != "All" && volumeTo != null) lProducts = lProducts.Where(p => p.volume != null && float.Parse(p.volume) >= float.Parse(volumeFrom) && float.Parse(p.volume) <= float.Parse(volumeTo));
                else if ((volumeFrom == "All" || volumeFrom == null) && volumeTo != "All" && volumeTo != null) lProducts = lProducts.Where(p => p.volume != null && float.Parse(p.volume) <= float.Parse(volumeTo));
                else if (volumeFrom != "All" && volumeFrom != null && (volumeTo == "All" || volumeTo == null)) lProducts = lProducts.Where(p => p.volume != null && float.Parse(p.volume) >= float.Parse(volumeFrom));

                if (heightFrom != "All" && heightFrom != null && heightTo != "All" && heightTo != null) lProducts = lProducts.Where(p => p.Height != null && float.Parse(p.Height) >= float.Parse(heightFrom) && float.Parse(p.Height) <= float.Parse(heightTo));
                else if ((heightFrom == "All" || heightFrom == null) && heightTo != "All" && heightTo != null) lProducts = lProducts.Where(p => p.Height != null && float.Parse(p.Height) <= float.Parse(heightTo));
                else if (heightFrom != "All" && heightFrom != null && (heightTo == "All" || heightTo == null)) lProducts = lProducts.Where(p => p.Height != null && float.Parse(p.Height) >= float.Parse(heightFrom));

                if (outerDiameterFrom != "All" && outerDiameterFrom != null && outerDiameterTo != "All" && outerDiameterTo != null) lProducts = lProducts.Where(p => p.Outer_diameter != null && float.Parse(p.Outer_diameter) >= float.Parse(outerDiameterFrom) && float.Parse(p.Outer_diameter) <= float.Parse(outerDiameterTo));
                else if ((outerDiameterFrom == "All" || outerDiameterFrom == null) && outerDiameterTo != "All" && outerDiameterTo != null) lProducts = lProducts.Where(p => p.Outer_diameter != null && float.Parse(p.Outer_diameter) <= float.Parse(outerDiameterTo));
                else if (outerDiameterFrom != "All" && outerDiameterFrom != null && (outerDiameterTo == "All" || outerDiameterTo == null)) lProducts = lProducts.Where(p => p.Outer_diameter != null && float.Parse(p.Outer_diameter) >= float.Parse(outerDiameterFrom));

                if (contactAreaFrom != "All" && contactAreaFrom != null && contactAreaTo != "All" && contactAreaTo != null) lProducts = lProducts.Where(p => p.Contact_area != null && float.Parse(p.Contact_area) >= float.Parse(contactAreaFrom) && float.Parse(p.Contact_area) <= float.Parse(contactAreaTo));
                else if ((contactAreaFrom == "All" || contactAreaFrom == null) && contactAreaTo != "All" && contactAreaTo != null) lProducts = lProducts.Where(p => p.Contact_area != null && float.Parse(p.Contact_area) <= float.Parse(contactAreaTo));
                else if (contactAreaFrom != "All" && contactAreaFrom != null && (contactAreaTo == "All" || contactAreaTo == null)) lProducts = lProducts.Where(p => p.Contact_area != null && float.Parse(p.Contact_area) >= float.Parse(contactAreaFrom));

                if (specific != null) lProducts = lProducts.Where(p => p.type.ToLower().Contains(specific.ToLower()));

                lProducts = lProducts.OrderBy(p => p.modulus, new SemiNumericComparer()).ThenBy(p => p.volume, new SemiNumericComparer());

                if (idTenant == 1) // gtp
                    lMyProducts = lProducts;
                else
                {
                    lMyProducts = repProducts.getList(idTenant);
                    lMyProducts = lMyProducts.Where(p => p.serie == serie.serie && p.category == serie.category);
                }

                Debug.WriteLine("Serie {1}. {0} Time for getting only the products", DateTime.Now - start, idSerie);
                start = DateTime.Now;

                if (idTenant == 1) lFilesProducts = repProducts.getGTPFilesByIdSerie(serie.id);//repProducts.getGTPFilesBySerie(serie.serie, serie.category);
                else lFilesProducts = repProducts.getFilesBySerie(serie.serie, serie.category);

                Debug.WriteLine("Serie {1}. {0} Time for fileproducts simple", DateTime.Now - start, idSerie);
                start = DateTime.Now;

                // add the update flag to the lfilesProducts and save it on the lfilesProductswithupdate list
                int me = (int) HttpContext.Current.Session["IDUSER"];
                
                
            }
            Debug.WriteLine("Time Serie {1} : {0}", DateTime.Now - startserie, idSerie);
        }    
    }
}