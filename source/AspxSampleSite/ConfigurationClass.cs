using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Web.SessionState;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using System.Web.Routing;
using Contensive.Processor.Models.Domain;
using Contensive;
using System.Configuration;

namespace AspxSampleSite {
    public class ConfigurationClass {
        // 
        // ====================================================================================================
        /// <summary>
        ///     ''' determine the Contensive application name from the webconfig or iis sitename
        ///     ''' </summary>
        ///     ''' <returns></returns>
        public static string getAppName() {
            // 
            // -- app name matches iis site name unless overridden by aspx app setting "ContensiveAppName"
            string appName = ConfigurationManager.AppSettings["ContensiveAppName"];
            if ((string.IsNullOrEmpty(appName)))
                appName = System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteName();
            return appName;
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///     ''' Create serverConfig object from appConfig or webConfig. This is alternative to configuration with config.json file
        ///     ''' </summary>
        ///     ''' <returns></returns>
        public static ServerConfigModel getServerConfig() {
            ServerConfigModel serverConfig = new ServerConfigModel();
            try {
                serverConfig.allowTaskRunnerService = false;
                serverConfig.allowTaskSchedulerService = false;
                serverConfig.awsBucketName = ConfigurationManager.AppSettings["ContensiveAwsBucketName"];
                serverConfig.awsRegionName = ConfigurationManager.AppSettings["ContensiveAwsRegionName"];
                serverConfig.awsAccessKey = ConfigurationManager.AppSettings["ContensiveAwsAccessKey"];
                serverConfig.awsSecretAccessKey = ConfigurationManager.AppSettings["ContensiveAwsSecretAccessKey"];
                serverConfig.defaultDataSourceAddress = ConfigurationManager.AppSettings["ContensiveDefaultDataSourceAddress"];
                serverConfig.defaultDataSourcePassword = ConfigurationManager.AppSettings["ContensiveDefaultDataSourcePassword"];
                serverConfig.defaultDataSourceUsername = ConfigurationManager.AppSettings["ContensiveDefaultDataSourceUsername"];
                serverConfig.enableLocalMemoryCache = GenericController.encodeBoolean(ConfigurationManager.AppSettings["ContensiveIsLocalCache"]);
                serverConfig.isLocalFileSystem = GenericController.encodeBoolean(ConfigurationManager.AppSettings["ContensiveIsLocalFileSystem"]);
                serverConfig.localDataDriveLetter = ConfigurationManager.AppSettings["ContensiveLocalDataDriveLetter"];
                serverConfig.name = ConfigurationManager.AppSettings["ContensiveServerGroupName"];
                // serverConfig.password = ConfigurationManager.AppSettings["ContensiveServerGroupPassword"]
                serverConfig.programFilesPath = "";
                // serverConfig.username = ConfigurationManager.AppSettings["ContensiveServerGroupUsername"]
                AppConfigModel appConfig = new AppConfigModel();
                appConfig.name = getAppName();
                appConfig.adminRoute = ConfigurationManager.AppSettings["ContensiveAdminRoute"];
                appConfig.localWwwPath = ConfigurationManager.AppSettings["ContensiveAppRootFilesPath"];
                appConfig.remoteFilePath = ConfigurationManager.AppSettings["ContensiveCdnFilesNetprefix"];
                appConfig.localFilesPath = ConfigurationManager.AppSettings["ContensiveCdnFilesPath"];
                appConfig.domainList.Add(ConfigurationManager.AppSettings["ContensivePrimaryDomain"]);
                appConfig.enabled = true;
                appConfig.localPrivatePath = ConfigurationManager.AppSettings["ContensivePrivateFilesPath"];
                appConfig.privateKey = ConfigurationManager.AppSettings["ContensivePrivateKey"];
                serverConfig.apps.Add(appConfig.name.ToLowerInvariant(), appConfig);
            } catch (Exception ex) {
            }
            return serverConfig;
        }
        // 
        public static void loadRouteMap(CPClass cp) {
            // 
            // -- if application var does not equal routemap.datecreated rebuild
            bool routeMapDateInValid = HttpContext.Current.Application("RouteMapDateCreated"] == null;
            if ((routeMapDateInValid || (cp.routeMap.dateCreated != (DateTime)HttpContext.Current.Application("RouteMapDateCreated"]))) {
                // 
                if (routeMapDateInValid)
                    LogController.logRaw("configurationClass, loadRouteMap, [" + cp.Site.Name + "], rebuild because HttpContext.Current.Application(RouteMapDateCreated) is not valid", BaseClasses.CPLogBaseClass.LogLevel.Info);
                else
                    LogController.logRaw("configurationClass, loadRouteMap, [" + cp.Site.Name + "], rebuild because not equal, cp.routeMap.dateCreated [" + cp.routeMap.dateCreated.ToString() + "], HttpContext.Current.Application(RouteMapDateCreated) [" + HttpContext.Current.Application["RouteMapDateCreated"].ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Info);
                // 
                HttpContext.Current.Application["routeMapDateCreated"] = cp.routeMap.dateCreated;
                lock (RouteTable.Routes) {
                    // 20180307, added clear to resolve error 
                    RouteTable.Routes.Clear();
                    foreach (var newRouteKeyValuePair in cp.routeMap.routeDictionary) {
                        try {
                            // 
                            LogController.logRaw("configurationClass, loadRouteMap, [" + cp.Site.Name + "] [" + newRouteKeyValuePair.Value.virtualRoute + "], [" + newRouteKeyValuePair.Value.physicalRoute + "]", BaseClasses.CPLogBaseClass.LogLevel.Trace);
                            // 
                            RouteTable.Routes.Remove(RouteTable.Routes(newRouteKeyValuePair.Key));
                            RouteTable.Routes.MapPageRoute(newRouteKeyValuePair.Value.virtualRoute, newRouteKeyValuePair.Value.virtualRoute, newRouteKeyValuePair.Value.physicalRoute);
                        } catch (Exception ex) {
                            cp.Site.ErrorReport(ex, "Unexpected exception adding virtualRoute [" + newRouteKeyValuePair.Key + "]"];
                        }
                    }
                }
            }
        }
    }
}


