using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace mvcDefaultSite {
    public class RouteConfig {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            //
            // -- HomeController
            routes.MapRoute(
                name: "Home",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            //
            // -- Default route picked up if no other route taken. Try to handle with Contensive
            routes.MapRoute(
                name: "Default",
                url: "{*url}",
                defaults: new { controller = "Contensive", action = "Index", url = "/" }
            );
        }
    }
}
