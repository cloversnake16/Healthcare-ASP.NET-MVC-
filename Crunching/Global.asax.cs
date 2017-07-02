using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Crunching.Models;
using Crunching.Controllers;
using FluentScheduler;

namespace Crunching
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
       // HiloBackground hilo;

        public static string SessionCookieName = "ASP.NET_SessionId";

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "404",
                "404",
                new { controller = "Error", action = "Index", Type = "404" }
            );

            routes.MapRoute(
                "500",
                "500",
                new { controller = "Error", action = "Index", Type = "500" }
            );
            
            routes.MapRoute(
                "Imprint",
                "imprint",
                new { controller = "Home", action = "Terms", Selected = "imprint" }
            );

            routes.MapRoute(
                "Terms",
                "Terms",
                new { controller = "Home", action = "Terms", Selected = "terms" }
            );

            routes.MapRoute(
                "dataProtection",
                "data-protection",
                new { controller = "Home", action = "Terms", Selected = "data-protection" }
            );


            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
              //  new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
              //  new { controller = "Account", action = "signIn", id = UrlParameter.Optional } // Parameter defaults
             //   new { controller = "Series", action = "IndexItems", id = UrlParameter.Optional }
                new { controller = "Catalog", action = "Index", id = UrlParameter.Optional }
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

           // hilo = new HiloBackground(20, 41, 59, null); // creamos el hilo demonio y lo ponemos a ejecutar
        }

        protected void Application_PreRequestHandlerExecute()
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[SessionCookieName];
            if (cookie != null)
            {
                HttpCookie respCookie = new HttpCookie(SessionCookieName, cookie.Value);
                respCookie.Expires = DateTime.Now.AddDays(20);
                Response.Cookies.Set(respCookie);
            }
        }

        protected void Application_EndRequest(Object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (!Request.Url.Host.StartsWith("www") && !Request.Url.IsLoopback)
            {
                UriBuilder builder = new UriBuilder(Request.Url);
                builder.Host = "www." + Request.Url.Host;
                Response.StatusCode = 301;
                Response.AddHeader("Location", builder.ToString());
                Response.End();
            }
        }

        //protected void Application_Error(object sender, EventArgs e) { Exception exception = Server.GetLastError(); Server.ClearError(); RouteData routeData = new RouteData(); routeData.Values.Add("controller", "Error"); routeData.Values.Add("action", "Index"); routeData.Values.Add("exception", exception); if (exception.GetType() == typeof(HttpException)) { routeData.Values.Add("statusCode", ((HttpException)exception).GetHttpCode()); } else { routeData.Values.Add("statusCode", 500); } IController controller = new ErrorController(); controller.Execute(new RequestContext(new HttpContextWrapper(Context), routeData)); Response.End(); }

    }
}