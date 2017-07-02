using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.Helpers
{
    public class SessionExpireFilterAttribute : ActionFilterAttribute
    {


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = HttpContext.Current;

            string variables = (ctx.Request.ServerVariables["SERVER_NAME"]);

            if (variables.Trim().ToLower() == "calc.gtp-toolbox.com")
                ctx.Response.Redirect("calc.gtp-toolbox.com", true);

            if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                ctx.Response.Redirect("~/Account/signIn/?1");
            }
            else
            {
                // check if session is supported
                if (ctx.Session != null)
                {
                    // check if a new session id was generated
                    if (ctx.Session.IsNewSession)
                    {
                        // If it says it is a new session, but an existing cookie exists, then it must
                        // have timed out

                        string sessionCookie = ctx.Request.Headers["Cookie"];
                        if (null != sessionCookie && sessionCookie.IndexOf("ASP.NET_SessionId") >= 0)
                        {
                            if (ctx.Session["IDUSER"] != null) ctx.Response.Redirect("~/Account/signIn/?" + ctx.Session["IDUSER"]);
                            else
                            {
                                ctx.Response.Redirect("~/Account/signIn/?2");
                                Logs log = new Logs();
                                log.action = "Session Timeout";
                                log.createdAt = DateTime.UtcNow;
                                log.userName = System.Web.HttpContext.Current.User.Identity.Name;
                                log.controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                                using (repoLogs repLogs = new repoLogs())
                                {
                                    repLogs.Create(log);
                                }
                            }
                         }
                    }
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }

}