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
    public class SeriesVM
    {
        public List<SerieVM> lSerieVM = new List<SerieVM>();
        public IEnumerable<string> lModulus;
        public IEnumerable<string> lVolumes;
        public IEnumerable<string> lHeights;
        public IEnumerable<string> lOuterDiameters;
        public IEnumerable<string> lContactAreas;

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

        public string selectModulusFrom;
        public string selectModulusTo;
        public string selectVolumeFrom;
        public string selectVolumeTo;

        repoSeries repSerie = new repoSeries();
        repoProducts repProducts = new repoProducts();

        public SeriesVM(int? category, int idTenant)
        {
            IEnumerable<Series> lSeries;
            if (category != null) lSeries = repSerie.getList().Where(p=>p.category == category);
            else lSeries = repSerie.getList();
            foreach (var item in lSeries)
            {
                SerieVM vm = new SerieVM(item.id, idTenant);
                lSerieVM.Add(vm);
            }
        }

     /*   public SeriesVM(int? category, int idTenant, string modulus, string volume, string height, string outerDiameter, string contactArea, string specific, string general)
        {
            IEnumerable<Series> lSeries;
            if (category != null) lSeries = repSerie.getList().Where(p => p.category == category);
            else lSeries = repSerie.getList();
            if (general != null) lSeries = lSeries.Where(p => (p.applications != null && p.applications.ToLower().Contains(general.ToLower())) 
                                                         || (p.characteristics != null && p.characteristics.ToLower().Contains(general.ToLower())) || (p.text != null && p.text.ToLower().Contains(general.ToLower())));

            IEnumerable<Products> productsList= repProducts.getAll();

            lModulus = productsList.Where(p => p.showInCatalog != false && p.modulus != null).OrderBy(p=>p.modulus.Length).ThenBy(p=>p.modulus).Select(p => p.modulus).Distinct();
            lVolumes = productsList.Where(p => p.showInCatalog != false && p.volume != null).OrderBy(c => c.volume.Length).ThenBy(c => c.volume).Select(p => p.volume).Distinct();
            lHeights = productsList.Where(p => p.showInCatalog != false && p.Height != null).OrderBy(p=>p.Height.Length).ThenBy(p=>p.Height).Select(p => p.Height).Distinct();
            lOuterDiameters = productsList.Where(p => p.showInCatalog != false && p.Outer_diameter != null).OrderBy(p=>p.Outer_diameter.Length).ThenBy(p=>p.Outer_diameter).Select(p => p.Outer_diameter).Distinct();
            lContactAreas = productsList.Where(p => p.showInCatalog != false && p.Contact_area != null).OrderBy(p=>p.Contact_area.Length).ThenBy(p=>p.Contact_area).Select(p => p.Contact_area).Distinct();

            filterContactArea = contactArea;
            filterGeneral = general;
            filterHeight = height;
            filterModulus = modulus;
            filterOuterDiameter = outerDiameter;
            filterSpecific = specific;
            filterVolume = volume;

            foreach (var item in lSeries)
            {
                SerieVM vm = new SerieVM(item.id, idTenant, modulus, volume, height, outerDiameter, contactArea, specific);
                if (vm.lProducts.Count() > 0)
                    lSerieVM.Add(vm);
            }
        }*/

        public class SemiNumericComparer : IComparer<string>
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
                try
                {
                    if (value == null) return false;
                    double i = Convert.ToDouble(value.ToString(), culture.NumberFormat);
                    return true;
                }
                catch (FormatException) {return false;}
            }
        }

        public SeriesVM(int? category, int idTenant, string modulusFrom, string modulusTo, string volumeFrom, string volumeTo, string heightFrom, string heightTo, string outerDiameterFrom, string outerDiameterTo, string contactAreaFrom, string contactAreaTo, string specific, string general)
        {
            DateTime ini = DateTime.Now;

            IEnumerable<Series> lSeries;
            if (category != null) lSeries = repSerie.getList().Where(p => p.category == category);
            else lSeries = repSerie.getList();
            if (general != null) lSeries = lSeries.Where(p => (p.applications != null && p.applications.ToLower().Contains(general.ToLower())) 
                                                         || (p.characteristics != null && p.characteristics.ToLower().Contains(general.ToLower())) || (p.text != null && p.text.ToLower().Contains(general.ToLower())));

            IEnumerable<Products> productsList = repProducts.getAll();

            //lModulus = productsList.Where(p => p.showInCatalog != false && p.modulus != null).OrderBy(p => p.modulus.Length).ThenBy(p => p.modulus).Select(p => p.modulus).Distinct();
            lModulus = productsList.Where(p => p.showInCatalog != false && p.modulus != null).OrderBy(x => x.modulus, new SemiNumericComparer()).Select(p => p.modulus).Distinct();
            lVolumes = productsList.Where(p => p.showInCatalog != false && p.volume != null).OrderBy(c => c.volume.Length).ThenBy(c => c.volume).Select(p => p.volume).Distinct();
            lHeights = productsList.Where(p => p.showInCatalog != false && p.Height != null).OrderBy(p => p.Height.Length).ThenBy(p => p.Height).Select(p => p.Height).Distinct();
            lOuterDiameters = productsList.Where(p => p.showInCatalog != false && p.Outer_diameter != null).OrderBy(p => p.Outer_diameter.Length).ThenBy(p => p.Outer_diameter).Select(p => p.Outer_diameter).Distinct();
            lContactAreas = productsList.Where(p => p.showInCatalog != false && p.Contact_area != null).OrderBy(p => p.Contact_area.Length).ThenBy(p => p.Contact_area).Select(p => p.Contact_area).Distinct();

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
            foreach (var item in lVolumes)
            {
                if (item != filterVolumeFrom)
                    selectVolumeFrom += HttpUtility.HtmlDecode("<option value=\"" + item + "\">" + item + "</option>");
                if (item == filterVolumeTo)
                    selectVolumeFrom += HttpUtility.HtmlDecode("<option selected value=\"" + item + "\">" + item + "</option>");
                if (item != filterVolumeTo)
                    selectVolumeTo += HttpUtility.HtmlDecode("<option value=\"" + item + "\">" + item + "</option>");
                if (item == filterVolumeTo)
                    selectVolumeTo += HttpUtility.HtmlDecode("<option selected value=\"" + item + "\">" + item + "</option>");
            }

            Debug.WriteLine("******************************");
            Debug.WriteLine("Time that takes generating the filters {0}", DateTime.Now - ini);

            IEnumerable<Products> lProducts = productsList; //repProducts.getAll();

            foreach (var item in lSeries.OrderBy(p=>p.serie).OrderBy(p=>p.category)) 
            {
                SerieVM vm = new SerieVM(item.id, idTenant, modulusFrom, modulusTo, volumeFrom, volumeTo, heightFrom, heightTo, outerDiameterFrom, outerDiameterTo, contactAreaFrom, contactAreaTo, specific, item, lProducts);
                if (vm.lProducts.Count() > 0)
                    lSerieVM.Add(vm);
            }
        }  
    }
}