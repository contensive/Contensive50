﻿
using System;
using System.Collections.Generic;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Domain {
    /// <summary>
    /// Dictionary of Routes
    /// </summary>
    public class RouteMapModel {
        //
        private const string cacheNameRouteMap = "routeMap";
        //
        //====================================================================================================
        //
        public class routeClass {
            public string virtualRoute;
            public string physicalRoute;
            public routeTypeEnum routeType;
            public int remoteMethodAddonId;
            public int linkAliasId;
            public int linkForwardId;
        }
        //
        public enum routeTypeEnum {
            admin,
            remoteMethod,
            linkAlias,
            linkForward
        }
        //
        public Dictionary<string, routeClass> routeDictionary;
        //
        //===================================================================================================
        /// <summary>
        /// Create a list of routes
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static RouteMapModel create(CoreController core) {
            RouteMapModel result = new RouteMapModel();
            try {
                result = getCache(core);
                if (result == null) {
                    result = new RouteMapModel {
                        dateCreated = DateTime.Now,
                        routeDictionary = new Dictionary<string, routeClass>()
                    };
                    string physicalFile = "~/" + core.siteProperties.serverPageDefault;
                    //
                    // -- admin route
                    string adminRoute = GenericController.normalizeRoute(core.appConfig.adminRoute);
                    if (!string.IsNullOrWhiteSpace(adminRoute)) {
                        result.routeDictionary.Add(adminRoute, new routeClass() {
                            physicalRoute = physicalFile,
                            virtualRoute = adminRoute,
                            routeType = routeTypeEnum.admin
                        });
                    }
                    //
                    // -- remote methods
                    foreach (var remoteMethod in core.addonCache.getRemoteMethodAddonList()) {
                        string route = GenericController.normalizeRoute(remoteMethod.name);
                        if (!string.IsNullOrWhiteSpace(route)) {
                            if (result.routeDictionary.ContainsKey(route)) {
                                LogController.handleWarn(core, new ApplicationException("Route [" + route + "] cannot be added because it is a matches the Admin Route or another Remote Method."));
                            } else {
                                result.routeDictionary.Add(route, new routeClass() {
                                    physicalRoute = physicalFile,
                                    virtualRoute = route,
                                    routeType = routeTypeEnum.remoteMethod,
                                    remoteMethodAddonId = remoteMethod.id
                                });
                            }
                        }
                    }
                    //foreach (var remoteMethod in AddonModel.createList_RemoteMethods(core, new List<string>())) {
                    //    string route = GenericController.normalizeRoute(remoteMethod.name);
                    //    if (!string.IsNullOrWhiteSpace(route)) {
                    //        if (result.routeDictionary.ContainsKey(route)) {
                    //            LogController.handleWarn( core,new ApplicationException("Route [" + route + "] cannot be added because it is a matches the Admin Route or another Remote Method."));
                    //        } else {
                    //            result.routeDictionary.Add(route, new routeClass() {
                    //                physicalRoute = physicalFile,
                    //                virtualRoute = route,
                    //                routeType = routeTypeEnum.remoteMethod,
                    //                remoteMethodAddonId = remoteMethod.id
                    //            });
                    //        }
                    //    }
                    //}
                    //
                    // -- link forwards
                    foreach (var linkForward in LinkForwardModel.createList(core, "name Is Not null")) {
                        string route = GenericController.normalizeRoute(linkForward.name);
                        if (!string.IsNullOrEmpty(route)) {
                            if (result.routeDictionary.ContainsKey(route)) {
                                LogController.handleError( core,new ApplicationException("Link Foward Route [" + route + "] cannot be added because it is a matches the Admin Route, a Remote Method or another Link Forward."));
                            } else {
                                result.routeDictionary.Add(route, new routeClass() {
                                    physicalRoute = physicalFile,
                                    virtualRoute = route,
                                    routeType = routeTypeEnum.linkForward,
                                    linkForwardId = linkForward.id
                                });
                            }
                        }
                    }
                    //
                    // -- link aliases
                    foreach (var linkAlias in LinkAliasModel.createList(core, "name Is Not null")) {
                        string route = GenericController.normalizeRoute(linkAlias.name);
                        if (!string.IsNullOrEmpty(route)) {
                            if (result.routeDictionary.ContainsKey(route)) {
                                LogController.handleError( core,new ApplicationException("Link Alias route [" + route + "] cannot be added because it is a matches the Admin Route, a Remote Method, a Link Forward o another Link Alias."));
                            } else {
                                result.routeDictionary.Add(route, new routeClass() {
                                    physicalRoute = physicalFile,
                                    virtualRoute = route,
                                    routeType = routeTypeEnum.linkAlias,
                                    linkAliasId = linkAlias.id
                                });
                            }
                        }
                    }
                    setCache(core, result);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public static void setCache(CoreController core, RouteMapModel routeDictionary) {
            var dependentKeyList = new List<string> {
                CacheController.getCacheKey_forDbTable( AddonModel.contentTableName),
                CacheController.getCacheKey_forDbTable(LinkAliasModel.contentTableName),
                CacheController.getCacheKey_forDbTable(LinkForwardModel.contentTableName)
            };
            core.cache.setObject(cacheNameRouteMap, routeDictionary,dependentKeyList);
        }
        //
        //====================================================================================================
        public static RouteMapModel getCache(CoreController core) {
            return core.cache.getObject<RouteMapModel>(cacheNameRouteMap);
        }
        //
        //====================================================================================================
        public static void invalidateCache(CoreController core) {
            core.cache.invalidate(cacheNameRouteMap);
        }
        //
        //====================================================================================================
        /// <summary>
        /// The date and time when this route dictionary was created. Used by iis app to detect if the route table needs to be updated.
        /// </summary>
        public DateTime dateCreated { get; set; }
    }
}

