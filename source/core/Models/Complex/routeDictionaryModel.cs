
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.Complex {
    public class routeDictionaryModel {
        //
        private const string cacheNameRouteDictionary = "routeDictionary";
        //
        //===================================================================================================
        public static Dictionary<string, BaseClasses.CPSiteBaseClass.routeClass> create(coreClass cpCore) {
            Dictionary<string, BaseClasses.CPSiteBaseClass.routeClass> result = new Dictionary<string, BaseClasses.CPSiteBaseClass.routeClass>();
            try {
                result = getCache(cpCore);
                if (result == null) {
                    result = new Dictionary<string, BaseClasses.CPSiteBaseClass.routeClass>();
                    string physicalFile = "~/" + cpCore.siteProperties.getText("serverpagedefault", "default.aspx");
                    //
                    // -- admin route
                    string adminRoute = genericController.normalizeRoute(cpCore.serverConfig.appConfig.adminRoute);
                    if (!string.IsNullOrWhiteSpace(adminRoute)) {
                        result.Add(adminRoute, new BaseClasses.CPSiteBaseClass.routeClass() {
                            physicalRoute = physicalFile,
                            virtualRoute = adminRoute,
                            routeType = BaseClasses.CPSiteBaseClass.routeTypeEnum.admin
                        });
                    }
                    //
                    // -- remote methods
                    List<Contensive.Core.Models.DbModels.addonModel> remoteMethods = Contensive.Core.Models.DbModels.addonModel.createList_RemoteMethods(cpCore, new List<string>());
                    foreach (Contensive.Core.Models.DbModels.addonModel remoteMethod in remoteMethods) {
                        string route = genericController.normalizeRoute(remoteMethod.name);
                        if (!string.IsNullOrWhiteSpace(route)) {
                            if (result.ContainsKey(route)) {
                                cpCore.handleException(new ApplicationException("Route [" + route + "] cannot be added because it is a matches the Admin Route or another Remote Method."));
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
                    List<Models.DbModels.linkForwardModel> linkForwards = linkForwardModel.createList(cpCore, "name Is Not null");
                    foreach (Models.DbModels.linkForwardModel linkForward in linkForwards) {
                        string route = genericController.normalizeRoute(linkForward.name);
                        if (!string.IsNullOrEmpty(route)) {
                            if (result.ContainsKey(route)) {
                                cpCore.handleException(new ApplicationException("Link Foward Route [" + route + "] cannot be added because it is a matches the Admin Route, a Remote Method or another Link Forward."));
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
                    List<Models.DbModels.linkAliasModel> linkAliasList = linkAliasModel.createList(cpCore, "name Is Not null");
                    foreach (Models.DbModels.linkAliasModel linkAlias in linkAliasList) {
                        string route = genericController.normalizeRoute(linkAlias.name);
                        if (!string.IsNullOrEmpty(route)) {
                            if (result.ContainsKey(route)) {
                                cpCore.handleException(new ApplicationException("Link Alias route [" + route + "] cannot be added because it is a matches the Admin Route, a Remote Method, a Link Forward o another Link Alias."));
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
                    setCache(cpCore, result);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        public static void setCache(coreClass cpCore, Dictionary<string, BaseClasses.CPSiteBaseClass.routeClass> routeDictionary) {
            var dependentKeyList = new List<string>();
            dependentKeyList.Add(Models.DbModels.addonModel.contentTableName);
            dependentKeyList.Add(Models.DbModels.linkAliasModel.contentTableName);
            dependentKeyList.Add(Models.DbModels.linkForwardModel.contentTableName);
            cpCore.cache.setObject(cacheNameRouteDictionary, routeDictionary,dependentKeyList);
        }
        //
        //====================================================================================================
        public static Dictionary<string, BaseClasses.CPSiteBaseClass.routeClass> getCache(coreClass cpCore) {
            return cpCore.cache.getObject<Dictionary<string, BaseClasses.CPSiteBaseClass.routeClass>>(cacheNameRouteDictionary);
        }
        //
        //====================================================================================================
        public static void invalidateCache(coreClass cpCore) {
            cpCore.cache.invalidate(cacheNameRouteDictionary);
        }
    }
}

