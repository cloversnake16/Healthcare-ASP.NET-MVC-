using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTracker.Helpers
{
    public class LogMgr
    {
        public void Log(string notes)
        {
            RequestContext context = HttpContext.Current.Request.RequestContext;
            RouteData route_data = context.RouteData;
           // TimeSpan duration = (DateTime.Now - start_time);
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
         //   log.duration = duration;
            log.userName = context.HttpContext.User.Identity.Name;
            log.serverName = serverName;
            log.notes = notes;
            using (repoLogs repLogs = new repoLogs())            
                repLogs.Create(log);
            
        }

        public void BackgroundLog(string notes)
        {       
            DateTime created_at = DateTime.UtcNow;
            //Save all your required values, including user id and whatnot here.
            //The duration variable will allow you to see expensive page loads on the controller, this can be useful when clients complain about something being slow.
            Logs log = new Logs();
            log.createdAt = created_at;
            //   log.duration = duration;
            log.notes = notes;
            using (repoLogs repLogs = new repoLogs())
                repLogs.Create(log);
        }
    }
}