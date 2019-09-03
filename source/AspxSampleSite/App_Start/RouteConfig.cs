using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Contensive.Processor.Controllers;
using Microsoft.AspNet.FriendlyUrls;

namespace AspxSampleSite
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes) {
            try {
                LogController.logRaw("Global.asax, Application_Start [" + ConfigurationClass.getAppName() + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Trace);
                using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(ConfigurationClass.getAppName())) {
                    AspxSampleSite.ConfigurationClass.loadRouteMap(cp);
                }
            } catch (Exception ex) {
                LogController.logRaw("Global.asax, Application_Start exception [" + ConfigurationClass.getAppName() + "]" + getAppDescription("Application_Start ERROR exit") + ", ex [" + ex.ToString() + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Fatal);
            }
            var settings = new FriendlyUrlSettings();
            settings.AutoRedirectMode = RedirectMode.Permanent;
            routes.EnableFriendlyUrls(settings);
        }

    }
}
