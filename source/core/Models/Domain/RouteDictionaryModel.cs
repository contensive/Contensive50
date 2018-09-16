
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.Domain {
    /// <summary>
    /// Dictionary of Routes
    /// </summary>
    public class RouteDictionaryModel {
        //
        private const string cacheNameRouteDictionary = "routeDictionary";
        //
        //===================================================================================================
        /// <summary>
        /// Create a list of routes
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static Dictionary<string, BaseClasses.CPSiteBaseClass.routeClass> create(CoreController core) {
            Dictionary<string, BaseClasses.CPSiteBaseClass.routeClass> result = new Dictionary<string, BaseClasses.CPSiteBaseClass.routeClass>();
            try {
                result = getCache(core);
                if (result == null) {
                    result = new Dictionary<string, BaseClasses.CPSiteBaseClass.routeClass>();
                    string physicalFile = "~/" + core.siteProperties.serverPageDefault;
                    //
                    // -- admin route
                    string adminRoute = genericController.normalizeRoute(core.appConfig.adminRoute);
                    if (!string.IsNullOrWhiteSpace(adminRoute)) {
                        result.Add(adminRoute, new BaseClasses.CPSiteBaseClass.routeClass() {
                            physicalRoute = physicalFile,
                            virtualRoute = adminRoute,
                            routeType = BaseClasses.CPSiteBaseClass.routeTypeEnum.admin
                        });
                    }
                    //
                    // -- remote methods
                    List<Contensive.Processor.Models.Db.AddonModel> remoteMethods = Contensive.Processor.Models.Db.AddonModel.createList_RemoteMethods(core, new List<string>());
                    foreach (Contensive.Processor.Models.Db.AddonModel remoteMethod in remoteMethods) {
                        string route = genericController.normalizeRoute(remoteMethod.name);
                        if (!string.IsNullOrWhiteSpace(route)) {
                            if (result.ContainsKey(route)) {
                                logController.handleWarn( core,new ApplicationException("Route [" + route + "] cannot be added because it is a matches the Admin Route or another Remote Method."));
                            } else {
                                result.Add(route, new BaseClasses.CPSiteBaseClass.routeClass() {
                                    physicalRoute = physicalFile,
                                    virtualRoute = route,
                                    routeType = BaseClasses.CPSiteBaseClass.routeTypeEnum.remoteMethod,
                                    remoteMethodAddonId = remoteMethod.id
                                });
                            }
                        }
                    }
                    //
                    // -- link forwards
                    List<Models.Db.linkForwardModel> linkForwards = linkForwardModel.createList(core, "name Is Not null");
                    foreach (Models.Db.linkForwardModel linkForward in linkForwards) {
                        string route = genericController.normalizeRoute(linkForward.name);
                        if (!string.IsNullOrEmpty(route)) {
                            if (result.ContainsKey(route)) {
                                logController.handleError( core,new ApplicationException("Link Foward Route [" + route + "] cannot be added because it is a matches the Admin Route, a Remote Method or another Link Forward."));
                            } else {
                                result.Add(route, new BaseClasses.CPSiteBaseClass.routeClass() {
                                    physicalRoute = physicalFile,
                                    virtualRoute = route,
                                    routeType = BaseClasses.CPSiteBaseClass.routeTypeEnum.linkForward,
                                    linkForwardId = linkForward.id
                                });
                            }
                        }
                    }
                    //
                    // -- link aliases
                    List<Models.Db.linkAliasModel> linkAliasList = linkAliasModel.createList(core, "name Is Not null");
                    foreach (Models.Db.linkAliasModel linkAlias in linkAliasList) {
                        string route = genericController.normalizeRoute(linkAlias.name);
                        if (!string.IsNullOrEmpty(route)) {
                            if (result.ContainsKey(route)) {
                                logController.handleError( core,new ApplicationException("Link Alias route [" + route + "] cannot be added because it is a matches the Admin Route, a Remote Method, a Link Forward o another Link Alias."));
                            } else {
                                result.Add(route, new BaseClasses.CPSiteBaseClass.routeClass() {
                                    physicalRoute = physicalFile,
                                    virtualRoute = route,
                                    routeType = BaseClasses.CPSiteBaseClass.routeTypeEnum.linkAlias,
                                    linkAliasId = linkAlias.id
                                });
                            }
                        }
                    }
                    setCache(core, result);
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        public static void setCache(CoreController core, Dictionary<string, BaseClasses.CPSiteBaseClass.routeClass> routeDictionary) {
            var dependentKeyList = new List<string>();
            dependentKeyList.Add(Models.Db.AddonModel.contentTableName);
            dependentKeyList.Add(Models.Db.linkAliasModel.contentTableName);
            dependentKeyList.Add(Models.Db.linkForwardModel.contentTableName);
            core.cache.setObject(cacheNameRouteDictionary, routeDictionary,dependentKeyList);
        }
        //
        //====================================================================================================
        public static Dictionary<string, BaseClasses.CPSiteBaseClass.routeClass> getCache(CoreController core) {
            return core.cache.getObject<Dictionary<string, BaseClasses.CPSiteBaseClass.routeClass>>(cacheNameRouteDictionary);
        }
        //
        //====================================================================================================
        public static void invalidateCache(CoreController core) {
            core.cache.invalidate(cacheNameRouteDictionary);
        }
    }
}

