using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Crunching.Models;
using GTPTracker.repos;
using System.Web.Routing;


namespace GTPTracker.Helpers
{
    public class LogFilterAttribute : ActionFilterAttribute
    {
        protected DateTime start_time;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            start_time = DateTime.Now;
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            RouteData route_data = filterContext.RouteData;
            TimeSpan duration = (DateTime.Now - start_time);
            string controller = (string)route_data.Values["controller"];
            string action = (string)route_data.Values["action"];
            DateTime created_at = DateTime.Now;
            string serverName = (HttpContext.Current.Request.ServerVariables["SERVER_NAME"]);
            //Save all your required values, including user id and whatnot here.
            //The duration variable will allow you to see expensive page loads on the controller, this can be useful when clients complain about something being slow.
            Logs log = new Logs();
            log.action = action;
            log.controller = controller;
            log.createdAt = created_at;
            log.duration = duration;
            log.userName = filterContext.HttpContext.User.Identity.Name;
            log.serverName = serverName;
            using (repoLogs repLogs = new repoLogs())
            {
                repLogs.Create(log);
            }
        }
    }
}