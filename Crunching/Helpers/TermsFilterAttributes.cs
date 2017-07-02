using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.Helpers
{
    public class TermsFilterAttributes : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string userName = null;
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                userName = filterContext.HttpContext.User.Identity.Name;
                using (repoUsers rep = new repoUsers())
                {
                    Users user = rep.getUserByEmail(userName);
                    if (user != null)
                    {
                        HttpContext ctx = HttpContext.Current;
                        bool signed = user.userAgreementSigned == true;
                        if (signed != true) ctx.Response.Redirect("~/Customers/showUserAgreement");
                    }
                    base.OnActionExecuting(filterContext);
                }
            }            
        }
    }
}