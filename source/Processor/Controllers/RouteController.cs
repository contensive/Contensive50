﻿
using System;
using System.Reflection;
using Contensive.BaseClasses;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Models.Db;
using System.Collections.Generic;
using static Contensive.Processor.Constants;
using static Contensive.Processor.Controllers.GenericController;
using System.Diagnostics;
using Contensive.Processor.Exceptions;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class RouteController {
        //
        //=============================================================================
        /// <summary>
        /// Executes the current route. To determine the route:
        /// route can be from URL, or from routeOverride
        /// how to process route
        /// -- urlParameters - /urlParameter(0)/urlParameter(1)/etc.
        /// -- first try full url, then remove them from the left and test until last, try just urlParameter(0)
        /// ---- so url /a/b/c, with addon /a and addon /a/b -> would run addon /a/b
        /// 
        /// </summary>
        /// <returns>The doc created by the default addon. (html, json, etc)</returns>
        public static string executeRoute(CoreController core,  string routeOverride = "") {
            string result = "";
            var sw = new Stopwatch();
            sw.Start();
            LogController.logRaw("CoreController executeRoute, enter", LogController.LogLevel.Trace);
            try {
                if (core.appConfig != null) {
                    //
                    // -- test fix for 404 response during routing - could it be a response left over from processing before we are called
                    core.webServer.setResponseStatus(WebServerController.httpResponseStatus200_Success);
                    //
                    // -- execute intercept methods first, like login, that run before the route that returns the page
                    // -- intercept routes should be addons alos
                    //
                    // -- determine the route: try routeOverride
                    string normalizedRoute = GenericController.normalizeRoute(routeOverride);
                    if (string.IsNullOrEmpty(normalizedRoute)) {
                        //
                        // -- no override, try argument route (remoteMethodAddon=)
                        normalizedRoute = GenericController.normalizeRoute(core.docProperties.getText(RequestNameRemoteMethodAddon));
                        if (string.IsNullOrEmpty(normalizedRoute)) {
                            //
                            // -- no override or argument, use the url as the route
                            normalizedRoute = GenericController.normalizeRoute(core.webServer.requestPathPage.ToLowerInvariant());
                        }
                    }
                    //
                    // -- legacy ajaxfn methods
                    string AjaxFunction = core.docProperties.getText(RequestNameAjaxFunction);
                    if (!string.IsNullOrEmpty(AjaxFunction)) {
                        //
                        // -- Need to be converted to Url parameter addons
                        switch ((AjaxFunction)) {
                            case ajaxGetFieldEditorPreferenceForm:
                                //
                                // moved to Addons.AdminSite
                                core.doc.continueProcessing = false;
                                return (new Addons.AdminSite.GetFieldEditorPreference()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case AjaxGetDefaultAddonOptionString:
                                //
                                // moved to Addons.AdminSite
                                core.doc.continueProcessing = false;
                                return (new Addons.AdminSite.GetAjaxDefaultAddonOptionStringClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case AjaxSetVisitProperty:
                                //
                                // moved to Addons.AdminSite
                                core.doc.continueProcessing = false;
                                return (new Addons.AdminSite.SetAjaxVisitPropertyClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case AjaxGetVisitProperty:
                                //
                                // moved to Addons.AdminSite
                                core.doc.continueProcessing = false;
                                return (new Addons.AdminSite.GetAjaxVisitPropertyClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case AjaxData:
                                //
                                // moved to Addons.AdminSite
                                core.doc.continueProcessing = false;
                                return (new Addons.AdminSite.ProcessAjaxDataClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case AjaxPing:
                                //
                                // moved to Addons.AdminSite
                                core.doc.continueProcessing = false;
                                return (new Addons.AdminSite.GetOKClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case AjaxOpenIndexFilter:
                                //
                                // moved to Addons.AdminSite
                                core.doc.continueProcessing = false;
                                return (new Addons.AdminSite.OpenAjaxIndexFilterClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case AjaxOpenIndexFilterGetContent:
                                //
                                // moved to Addons.AdminSite
                                core.doc.continueProcessing = false;
                                return (new Addons.AdminSite.OpenAjaxIndexFilterGetContentClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case AjaxCloseIndexFilter:
                                //
                                // moved to Addons.AdminSite
                                core.doc.continueProcessing = false;
                                return (new Addons.AdminSite.CloseAjaxIndexFilterClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case AjaxOpenAdminNav:
                                //
                                // moved to Addons.AdminSite
                                core.doc.continueProcessing = false;
                                return (new Addons.AdminSite.OpenAjaxAdminNavClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            default:
                                //
                                // -- unknown method, log warning
                                core.doc.continueProcessing = false;
                                return string.Empty;
                        }
                    }
                    //
                    // -- legacy email intercept methods
                    if (core.docProperties.getInteger(rnEmailOpenFlag) > 0) {
                        //
                        // -- Process Email Open
                        core.doc.continueProcessing = false;
                        return (new Addons.Primitives.OpenEmailClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                    }
                    if (core.docProperties.getInteger(rnEmailClickFlag) > 0) {
                        //
                        // -- Process Email click
                        core.doc.continueProcessing = false;
                        return (new Addons.Primitives.ClickEmailClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                    }
                    if (core.docProperties.getInteger(rnEmailBlockRecipientEmail) > 0) {
                        //
                        // -- Process Email block
                        core.doc.continueProcessing = false;
                        return (new Addons.Primitives.BlockEmailClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                    }
                    //
                    // -- legacy form process methods 
                    string formType = core.docProperties.getText(core.docProperties.getText("ccformsn") + "type");
                    if (!string.IsNullOrEmpty(formType)) {
                        //
                        // set the meta content flag to show it is not needed for the head tag
                        switch (formType) {
                            case FormTypeAddonStyleEditor:
                                //
                                result = (new Addons.Primitives.ProcessAddonStyleEditorClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeAddonSettingsEditor:
                                //
                                result = (new Addons.Primitives.ProcessAddonSettingsEditorClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeSendPassword:
                                //
                                result = (new Addons.Primitives.processSendPasswordFormClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeLogin:
                            case "l09H58a195":
                                //
                                result = (new Addons.Primitives.ProcessLoginDefaultClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeToolsPanel:
                                //
                                result = (new Addons.Primitives.processFormToolsPanelClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypePageAuthoring:
                                //
                                result = (new Addons.Primitives.processFormQuickEditingClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeActiveEditor:
                                //
                                result = (new Addons.Primitives.ProcessActiveEditorClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeSiteStyleEditor:
                                //
                                result = (new Addons.Primitives.processSiteStyleEditorClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeHelpBubbleEditor:
                                //
                                result = (new Addons.Primitives.processHelpBubbleEditorClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeJoin:
                                //
                                result = (new Addons.Primitives.processJoinFormClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                                break;
                        }
                    }
                    //
                    // -- legacy methods=
                    string HardCodedPage = core.docProperties.getText(RequestNameHardCodedPage);
                    if (!string.IsNullOrEmpty(HardCodedPage)) {
                        switch (GenericController.vbLCase(HardCodedPage)) {
                            case HardCodedPageLogout:
                                //
                                // -- logout intercept -- after logout continue
                                (new Addons.Primitives.ProcessLogoutMethodClass()).Execute(core.cp_forAddonExecutionOnly);
                                break;
                            case HardCodedPageSendPassword:
                                //
                                return (new Addons.Primitives.ProcessSendPasswordMethodClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageResourceLibrary:
                                //
                                return (new Addons.Primitives.ProcessResourceLibraryMethodClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageLoginDefault:
                                //
                                return (new Addons.Primitives.ProcessLoginDefaultMethodClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageLogin:
                                //
                                return (new Addons.Primitives.ProcessLoginMethodClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageLogoutLogin:
                                //
                                return (new Addons.Primitives.ProcessLogoutLoginMethodClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageSiteExplorer:
                                //
                                return (new Addons.Primitives.ProcessSiteExplorerMethodClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageStatus:
                                //
                                return (new Addons.Diagnostics.StatusClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageRedirect:
                                //
                                return (new Addons.Primitives.ProcessRedirectMethodClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageExportAscii:
                                //
                                return (new Addons.Primitives.ProcessExportAsciiMethodClass()).Execute(core.cp_forAddonExecutionOnly).ToString();
                        }
                    }
                    //
                    // -- try route Dictionary (addons, admin, link forwards, link alias), from full route to first segment one at a time
                    // -- so route /this/and/that would first test /this/and/that, then test /this/and, then test /this
                    string routeTest = normalizedRoute;
                    bool routeFound = false;
                    int routeCnt = 100;
                    do {
                        routeFound = core.routeMap.routeDictionary.ContainsKey(routeTest);
                        if (routeFound) {
                            break;
                        }
                        if (routeTest.IndexOf("/") < 0) {
                            break;
                        }
                        routeTest = routeTest.Left(routeTest.LastIndexOf("/"));
                        routeCnt -= 1;
                    } while ((routeCnt > 0) && (!routeFound));
                    //
                    // -- execute route
                    if (routeFound) {
                        RouteMapModel.RouteClass route = core.routeMap.routeDictionary[routeTest];
                        switch (route.routeType) {
                            case RouteMapModel.RouteTypeEnum.admin: {
                                    //
                                    // -- admin site
                                    AddonModel addon = AddonModel.create(core, addonGuidAdminSite);
                                    if (addon == null) {
                                        LogController.logError(core, new GenericException("The admin site addon could not be found by guid [" + addonGuidAdminSite + "]."));
                                        return "The default admin site addon could not be found. Please run an upgrade on this application to restore default services (command line> cc -a appName -r )";
                                    } else {
                                        return core.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext() {
                                            addonType = CPUtilsBaseClass.addonContext.ContextAdmin,
                                            errorContextMessage = "calling admin route [" + addonGuidAdminSite + "] during execute route method"
                                        });
                                    }
                                }
                            case RouteMapModel.RouteTypeEnum.remoteMethod: {
                                    //
                                    // -- remote method
                                    AddonModel addon = core.addonCache.getAddonById(route.remoteMethodAddonId);
                                    if (addon == null) {
                                        LogController.logError(core, new GenericException("The addon for remoteMethodAddonId [" + route.remoteMethodAddonId + "] could not be opened."));
                                        return "";
                                    } else {
                                        CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                                            addonType = CPUtilsBaseClass.addonContext.ContextRemoteMethodJson,
                                            cssContainerClass = "",
                                            cssContainerId = "",
                                            hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                                                contentName = core.docProperties.getText("hostcontentname"),
                                                fieldName = "",
                                                recordId = core.docProperties.getInteger("HostRecordID")
                                            },
                                            errorContextMessage = "calling remote method addon [" + route.remoteMethodAddonId + "] during execute route method"
                                        };
                                        return core.addon.execute(addon, executeContext);
                                    }
                                }
                            case RouteMapModel.RouteTypeEnum.linkAlias:
                                //
                                // - link alias
                                // -- all the query string values have already been added to doc properties, so do not over write them.
                                // -- consensus is that since the link alias (permalink, long-tail url, etc) comes first on the left, that the querystring should override
                                // -- so http://www.mySite.com/My-Blog-Post?bid=9 means use the bid not the bid from the link-alias
                                LinkAliasModel linkAlias = LinkAliasModel.create(core, route.linkAliasId);
                                if (linkAlias != null) {
                                    // -- set the link alias page number, unless it has been overridden
                                    if (!core.docProperties.containsKey("bid")) { core.docProperties.setProperty("bid", linkAlias.pageID); }
                                    if (!string.IsNullOrWhiteSpace(linkAlias.queryStringSuffix)) {
                                        string[] keyValuePairs = linkAlias.queryStringSuffix.Split('&');
                                        // -- iterate through all the key=value pairs
                                        foreach (var keyEqualsValue in keyValuePairs) {
                                            string[] keyValue = keyEqualsValue.Split('=');
                                            if (!string.IsNullOrEmpty(keyValue[0])) {
                                                if (!core.docProperties.containsKey(keyValue[0])) {
                                                    if (keyValue.Length > 1) {
                                                        core.docProperties.setProperty(keyValue[0], keyValue[1]);
                                                    } else {
                                                        core.docProperties.setProperty(keyValue[0], string.Empty);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                }
                                break;
                            case RouteMapModel.RouteTypeEnum.linkForward:
                                //
                                // -- link forward
                                LinkForwardModel linkForward = LinkForwardModel.create(core, route.linkForwardId);
                                return core.webServer.redirect(linkForward.DestinationLink, "Link Forward #" + linkForward.id + ", " + linkForward.name);
                        }
                    }
                    //
                    // -- default route 
                    int defaultAddonId = 0;
                    if (core.doc.domain != null) {
                        defaultAddonId = core.doc.domain.defaultRouteId;
                    }
                    if (defaultAddonId == 0) {
                        defaultAddonId = core.siteProperties.defaultRouteId;
                    }
                    if (defaultAddonId > 0) {
                        //
                        // -- default route is run if no other route is found, which includes the route=defaultPage (default.aspx)
                        CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                            addonType = CPUtilsBaseClass.addonContext.ContextPage,
                            cssContainerClass = "",
                            cssContainerId = "",
                            hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                                contentName = "",
                                fieldName = "",
                                recordId = 0
                            },
                            errorContextMessage = "calling default route addon [" + defaultAddonId + "] during execute route method"
                        };
                        return core.addon.execute(Models.Db.AddonModel.create(core, defaultAddonId), executeContext);
                    }
                    //
                    // -- no route
                    LogController.logWarn(core, "executeRoute called with an unknown route [" + normalizedRoute + "], and no default route is set to handle it. Go to the admin site, open preferences and set a detault route. Typically this is Page Manager for websites or an authorization error for remote applications.");
                    result = "<p>This site is not configured for website traffic. Please set the default route.</p>";
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
            } finally {
                // if (core.doc.routeDictionaryChanges) { DefaultSite.configurationClass.loadRouteMap(cp))}
                LogController.logRaw("CoreController executeRoute, exit", LogController.LogLevel.Trace);
            }
            return result;
        }
    }
 }