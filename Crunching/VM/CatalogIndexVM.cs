using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.VM
{
    public class CatalogIndexVM
    {
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
        public string specific;
        public string general;
        public int? category;

        public string role;
        public int? tenant;
        public IEnumerable<vCustomers> lTenants;
        public bool includePublicProductsValue;
        public bool canShowIncludePublicProducts = false;

        public CatalogIndexVM(int? category, int? idTenant, string modulusFrom, string modulusTo, string volumeFrom, string volumeTo, string heightFrom, string heightTo, string outerDiameterFrom, string outerDiameterTo, string contactAreaFrom, string contactAreaTo, string specific, string general, Users user = null, string includePublicProducts = "off")
        {
            this.category = category;
            if (includePublicProducts == "true") this.includePublicProductsValue = true;
            else this.includePublicProductsValue = false;

            if (idTenant != null && user!=null && user.businessRole != "Customer") // I'm an admin using the dropdown and the checkbox
                canShowIncludePublicProducts = true;

            if (user != null)
            {
                role = user.businessRole;
                if (user.businessRole == "GTP General Manager")
                {
                    using (repoCustomers repCustomers = new repoCustomers())
                        lTenants = repCustomers.getList().OrderBy(p => p.name);
                }
                if (user.businessRole == "GTP KAM")
                {
                    using (repoCustomers repCustomers = new repoCustomers())
                        lTenants = repCustomers.getvByKAM(user.id).OrderBy(p => p.name);
                }
                role = user.businessRole;
            }
            else role = null;

            tenant = idTenant;

            this.selectContactFrom = contactAreaFrom;
            this.selectContactTo = contactAreaTo;
            this.selectHeightFrom = heightFrom;
            this.selectHeightTo = heightTo;
            this.selectModulusFrom = modulusFrom;
            this.selectModulusTo = modulusTo;
            this.selectOuterFrom = outerDiameterFrom;
            this.selectOuterTo = outerDiameterTo;
            this.selectVolumeFrom = volumeFrom;
            this.selectVolumeTo = volumeTo;
            this.specific = specific;
            this.general = general;
        }
    }
}