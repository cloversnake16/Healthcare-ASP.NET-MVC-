using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GTPTrackers.Controllers;

namespace GTPTracker.Controllers
{
    public class FilterTimeOut : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.Controller is AccountController || context.Controller is APIController))
            {
                try
                {
                    int idEntidad = (int)context.HttpContext.Session["IDTENANT"]; // trato de obtener el objeto session
                }
                catch { context.HttpContext.Response.Redirect("~/Account/signIn"); } // si falla es que la sesion caduco, ir a login
                base.OnActionExecuting(context);
            }
        }
    }
}