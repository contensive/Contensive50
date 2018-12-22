
using System;
using System.Reflection;
using System.Xml;
using System.Collections.Generic;
using Contensive.Processor.Models.Db;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.BaseClasses;
using System.IO;
using System.Data;
using System.Linq;
using Contensive.Processor.Exceptions;
using Contensive.Addons.AdminSite.Controllers;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// run addons
    /// - first routine should be constructor
    /// - disposable region at end
    /// - if disposable is not needed add: not IDisposable - not contained classes that need to be disposed
    /// </summary>
    public class AddonController : IDisposable {
        //
        // ----- objects passed in constructor, do not dispose
        //
        private CoreController core;
        //
        public enum ScriptLanguages {
            VBScript = 1,
            Javascript = 2
        }
        //
        // ====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="core"></param>
        public AddonController(CoreController core) : base() {
            this.core = core;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Execute an addon because it is a dependency of another addon/page/template. A dependancy is only run once in a page.
        /// </summary>
        /// <param name="addonId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// 
        public string executeDependency(Models.Db.AddonModel addon, CPUtilsBaseClass.addonExecuteContext context) {
            bool saveContextIsIncludeAddon = context.isIncludeAddon;
            context.isIncludeAddon = true;
            string result = execute(addon, context);
            context.isIncludeAddon = saveContextIsIncludeAddon;
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute an addon by its guid
        /// </summary>
        /// <param name="addonGuid"></param>
        /// <param name="executeContext"></param>
        /// <returns></returns>
        public string execute(string addonGuid, CPUtilsBaseClass.addonExecuteContext executeContext) {
            var addon = AddonModel.create(core, addonGuid);
            if (addon == null) {
                //
                // -- addon not found
                LogController.handleError(core, new ArgumentException("AddonExecute called without valid guid [" + addonGuid + "] from context [" + executeContext.errorContextMessage + "]."));
                return "";
            } else {
                return execute(addon, executeContext);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute addon by is id
        /// </summary>
        /// <param name="addonId"></param>
        /// <param name="executeContext"></param>
        /// <returns></returns>
        public string execute(int addonId, CPUtilsBaseClass.addonExecuteContext executeContext) {
            var addon = AddonModel.create(core, addonId);
            if (addon == null) {
                //
                // -- addon not found
                LogController.handleError(core, new ArgumentException("AddonExecute called without valid id [" + addonId + "] from context [" + executeContext.errorContextMessage + "]."));
                return "";
            } else {
                return execute(addon, executeContext);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute addon
        /// </summary>
        /// <param name="addon"></param>
        /// <param name="executeContext"></param>
        /// <returns></returns>
        public string execute(Models.Db.AddonModel addon, CPUtilsBaseClass.addonExecuteContext executeContext) {
            string result = "";
            //
            // -- setup values that have to be in finalize
            bool rootLevelAddon = core.doc.addonRecursionDepth.Count.Equals(0);
            bool save_forceJavascriptToHead = executeContext.forceJavascriptToHead;
            long addonStart = core.doc.appStopWatch.ElapsedMilliseconds;
            if (addon == null) {
                //
                // -- addon not found
                LogController.handleError(core, new ArgumentException("AddonExecute called without valid addon [" + executeContext.errorContextMessage + "]."));
            } else {
                try {
                    //
                    // -- save the addon details in a fifo stack to popoff during exit. The top of the stack represents the addon being executed
                    core.doc.addonModelStack.Push(addon);
                    if (executeContext == null) {
                        //
                        // -- context not configured 
                        LogController.handleError(core, new ArgumentException("The Add-on executeContext was not configured for addon [#" + addon.id + ", " + addon.name + "]."));
                    } else if (!string.IsNullOrEmpty(addon.objectProgramID)) {
                        //
                        // -- addons with activeX components are deprecated
                        string addonDescription = getAddonDescription(core, addon);
                        throw new GenericException("Addon is no longer supported because it contains an active-X component, add-on " + addonDescription + ".");
                    } else {
                        //
                        // -- check for addon recursion beyond limit (addonRecursionLimit)
                        bool blockRecursion = false;
                        bool inRecursionList = core.doc.addonRecursionDepth.ContainsKey(addon.id);
                        if (inRecursionList) blockRecursion = (core.doc.addonRecursionDepth[addon.id] > addonRecursionLimit);
                        if (blockRecursion) {
                            //
                            // -- cannot call an addon within an addon
                            throw new GenericException("Addon recursion limit exceeded. An addon [#" + addon.id + ", " + addon.name + "] cannot be called by itself more than " + addonRecursionLimit + " times.");
                        } else {
                            //
                            // -- track recursion and continue
                            if (!inRecursionList) {
                                core.doc.addonRecursionDepth.Add(addon.id, 1);
                            } else {
                                core.doc.addonRecursionDepth[addon.id] += 1;
                            }
                            string parentInstanceId = core.docProperties.getText("instanceId");
                            core.docProperties.setProperty("instanceId", executeContext.instanceGuid);
                            //
                            // -- if the addon's javascript is required in the head, set it in the executeContext now so it will propigate into the dependant addons as well
                            executeContext.forceJavascriptToHead = executeContext.forceJavascriptToHead || addon.javascriptForceHead;
                            //
                            // -- run included add-ons before their parent
                            foreach (var dependentAddon in core.addonCache.getDependsOnList(addon.id)) {
                                if (dependentAddon == null) {
                                    LogController.handleError(core, new GenericException("Addon not found. An included addon of [" + addon.name + "] was not found. The included addon may have been deleted. Recreate or reinstall the missing addon, then reinstall [" + addon.name + "] or manually correct the included addon selection."));
                                } else {
                                    executeContext.errorContextMessage = "adding dependent addon [" + dependentAddon.name + "] for addon [" + addon.name + "] called within context [" + executeContext.errorContextMessage + "]";
                                    result += executeDependency(dependentAddon, executeContext);
                                }
                            }
                            //List<int> addonIncludeRuleList = core.doc.getAddonIncludeRuleList(addon.id);
                            //foreach ( int includedAddonID in addonIncludeRuleList) {
                            //    AddonModel dependentAddon = AddonModel.create(core, includedAddonID);
                            //    if (dependentAddon == null) {
                            //        LogController.handleError(core, new GenericException("Addon not found. An included addon of [" + addon.name + "] was not found. The included addon may have been deleted. Recreate or reinstall the missing addon, then reinstall [" + addon.name + "] or manually correct the included addon selection."));
                            //    } else {
                            //        executeContext.errorContextMessage = "adding dependent addon [" + dependentAddon.name + "] for addon [" + addon.name + "] called within context [" + executeContext.errorContextMessage + "]";
                            //        result += executeDependency(dependentAddon, executeContext);
                            //    }
                            //}
                            //List<Models.Db.AddonIncludeRuleModel> addonIncludeRules = AddonIncludeRuleModel.createList(core, "(addonid=" + addon.id + ")");
                            //if (addonIncludeRules.Count > 0) {
                            //    string addonContextMessage = executeContext.errorContextMessage;
                            //    foreach (Models.Db.AddonIncludeRuleModel addonRule in addonIncludeRules) {
                            //        if (addonRule.includedAddonID > 0) {
                            //            AddonModel dependentAddon = AddonModel.create(core, addonRule.includedAddonID);
                            //            if (dependentAddon == null) {
                            //                LogController.handleError(core, new GenericException("Addon not found. An included addon of [" + addon.name + "] was not found. The included addon may have been deleted. Recreate or reinstall the missing addon, then reinstall [" + addon.name + "] or manually correct the included addon selection."));
                            //            } else {
                            //                executeContext.errorContextMessage = "adding dependent addon [" + dependentAddon.name + "] for addon [" + addon.name + "] called within context [" + addonContextMessage + "]";
                            //                result += executeDependency(dependentAddon, executeContext);
                            //            }
                            //        }
                            //    }
                            //    executeContext.errorContextMessage = addonContextMessage;
                            //}
                            //
                            // -- properties referenced multiple time 
                            bool allowAdvanceEditor = core.visitProperty.getBoolean("AllowAdvancedEditor");
                            //
                            // -- add addon record arguments to doc properties
                            if (!string.IsNullOrWhiteSpace(addon.argumentList)) {
                                foreach (var addon_argument in addon.argumentList.Replace("\r\n", "\r").Replace("\n", "\r").Split(Convert.ToChar("\r"))) {
                                    if (!string.IsNullOrEmpty(addon_argument)) {
                                        string[] nvp = addon_argument.Split('=');
                                        if (!string.IsNullOrEmpty(nvp[0])) {
                                            string nvpValue = "";
                                            if (nvp.Length > 1) {
                                                nvpValue = nvp[1];
                                            }
                                            if (nvpValue.IndexOf("[") >= 0) {
                                                nvpValue = nvpValue.Left(nvpValue.IndexOf("["));
                                            }
                                            core.docProperties.setProperty(nvp[0], nvpValue);
                                        }
                                    }
                                }
                            }
                            //
                            // -- add instance properties to doc properties
                            string ContainerCssID = "";
                            string ContainerCssClass = "";
                            foreach (var kvp in executeContext.argumentKeyValuePairs) {
                                switch (kvp.Key.ToLowerInvariant()) {
                                    case "wrapper":
                                        executeContext.wrapperID = GenericController.encodeInteger(kvp.Value);
                                        break;
                                    case "as ajax":
                                        addon.asAjax = GenericController.encodeBoolean(kvp.Value);
                                        break;
                                    case "css container id":
                                        ContainerCssID = kvp.Value;
                                        break;
                                    case "css container class":
                                        ContainerCssClass = kvp.Value;
                                        break;
                                }
                                core.docProperties.setProperty(kvp.Key, kvp.Value);
                            }
                            //
                            // Preprocess arguments into OptionsForCPVars, and set generic instance values wrapperid and asajax
                            if (addon.inFrame & (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) && (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodJson)) {
                                //
                                // -- inframe execution, deliver iframe with link back to remote method
                                result = "TBD - inframe";
                                //Link = core.webServer.requestProtocol & core.webServer.requestDomain & requestAppRootPath & core.siteProperties.serverPageDefault
                                //If genericController.vbInstr(1, Link, "?") = 0 Then
                                //    Link = Link & "?"
                                //Else
                                //    Link = Link & "&"
                                //End If
                                //Link = Link _
                                //        & "nocache=" & Rnd() _
                                //        & "&HostContentName=" & EncodeRequestVariable(HostContentName) _
                                //        & "&HostRecordID=" & HostRecordID _
                                //        & "&remotemethodaddon=" & EncodeURL(addon.id.ToString) _
                                //        & "&optionstring=" & EncodeRequestVariable(WorkingOptionString) _
                                //        & ""
                                //FrameID = "frame" & GetRandomInteger(core)
                                //returnVal = "<iframe src=""" & Link & """ id=""" & FrameID & """ onload=""cj.setFrameHeight('" & FrameID & "');"" class=""ccAddonFrameCon"" frameborder=""0"" scrolling=""no"">This content is not visible because your browser does not support iframes</iframe>" _
                                //        & cr & "<script language=javascript type=""text/javascript"">" _
                                //        & cr & "// Safari and Opera need a kick-start." _
                                //        & cr & "var e=document.getElementById('" & FrameID & "');if(e){var iSource=e.src;e.src='';e.src = iSource;}" _
                                //        & cr & "</script>"
                            } else if (addon.asAjax & (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) && (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodJson)) {
                                //
                                // -- asajax execution, deliver div with ajax callback
                                //
                                result = "TBD - asajax";
                                //-----------------------------------------------------------------
                                // AsAjax and this is NOT the callback - setup the ajax callback
                                // js,styles and other features from the addon record are added to the host page
                                // during the remote method, these are blocked, but if any are added during
                                //   DLL processing, they have to be handled
                                //-----------------------------------------------------------------
                                //
                                //If True Then
                                //    AsAjaxID = "asajax" & GetRandomInteger(core)
                                //    QS = "" _
                                //& RequestNameRemoteMethodAddon & "=" & EncodeRequestVariable(addon.id.ToString()) _
                                //& "&HostContentName=" & EncodeRequestVariable(HostContentName) _
                                //& "&HostRecordID=" & HostRecordID _
                                //& "&HostRQS=" & EncodeRequestVariable(core.doc.refreshQueryString) _
                                //& "&HostQS=" & EncodeRequestVariable(core.webServer.requestQueryString) _
                                //& "&optionstring=" & EncodeRequestVariable(WorkingOptionString) _
                                //& ""
                                //    '
                                //    ' -- exception made here. AsAjax is not used often, and this can create a QS too long
                                //    '& "&HostForm=" & EncodeRequestVariable(core.webServer.requestFormString) _
                                //    If IsInline Then
                                //        returnVal = cr & "<div ID=" & AsAjaxID & " Class=""ccAddonAjaxCon"" style=""display:inline;""><img src=""/ContensiveBase/images/ajax-loader-small.gif"" width=""16"" height=""16""></div>"
                                //    Else
                                //        returnVal = cr & "<div ID=" & AsAjaxID & " Class=""ccAddonAjaxCon""><img src=""/ContensiveBase/images/ajax-loader-small.gif"" width=""16"" height=""16""></div>"
                                //    End If
                                //    returnVal = returnVal _
                                //& cr & "<script Language=""javaScript"" type=""text/javascript"">" _
                                //& cr & "cj.ajax.qs('" & QS & "','','" & AsAjaxID & "');AdminNavPop=true;" _
                                //& cr & "</script>"
                                //    '
                                //    ' Problem - AsAjax addons must add styles, js and meta to the head
                                //    '   Adding them to the host page covers most cases, but sometimes the DLL itself
                                //    '   adds styles, etc during processing. These have to be added during the remote method processing.
                                //    '   appending the .innerHTML of the head works for FF, but ie blocks it.
                                //    '   using .createElement works in ie, but the tag system right now not written
                                //    '   to save links, etc, it is written to store the entire tag.
                                //    '   Also, OtherHeadTags can not be added this was.
                                //    '
                                //    ' Short Term Fix
                                //    '   For Ajax, Add javascript and style features to head of host page
                                //    '   Then during remotemethod, clear these strings before dll processing. Anything
                                //    '   that is added must have come from the dll. So far, the only addons we have that
                                //    '   do this load styles, so instead of putting in the the head (so ie fails), add styles inline.
                                //    '
                                //    '   This is because ie does not allow innerHTML updates to head tag
                                //    '   scripts and js could be handled with .createElement if only the links were saved, but
                                //    '   otherhead could not.
                                //    '   The case this does not cover is if the addon itself manually adds one of these entries.
                                //    '   In no case can ie handle the OtherHead, however, all the others can be done with .createElement.
                                //    ' Long Term Fix
                                //    '   Convert js, style, and meta tag system to use .createElement during remote method processing
                                //    '
                                //    Call core.html.doc_AddPagetitle2(PageTitle, AddedByName)
                                //    Call core.html.doc_addMetaDescription2(MetaDescription, AddedByName)
                                //    Call core.html.doc_addMetaKeywordList2(MetaKeywordList, AddedByName)
                                //    Call core.html.doc_AddHeadTag2(OtherHeadTags, AddedByName)
                                //    If Not blockJavascriptAndCss Then
                                //        '
                                //        ' add javascript and styles if it has not run already
                                //        '
                                //        Call core.html.addOnLoadJavascript(JSOnLoad, AddedByName)
                                //        Call core.html.addBodyJavascriptCode(JSBodyEnd, AddedByName)
                                //        Call core.html.addJavaScriptLinkHead(JSFilename, AddedByName)
                                //        If addon.StylesFilename.filename <> "" Then
                                //            Call core.html.addStyleLink(core.webServer.requestProtocol & core.webServer.requestDomain & genericController.getCdnFileLink(core, addon.StylesFilename.filename), addon.name & " default")
                                //        End If
                                //        'If CustomStylesFilename <> "" Then
                                //        '    Call core.html.addStyleLink(core.webServer.requestProtocol & core.webServer.requestDomain & genericController.getCdnFileLink(core, CustomStylesFilename), AddonName & " custom")
                                //        'End If
                                //    End If
                                //End If
                            } else {
                                //
                                //-----------------------------------------------------------------
                                // otherwise - produce the content from the addon
                                //   setup RQS as needed - RQS provides the querystring for add-ons to create links that return to the same page
                                //-----------------------------------------------------------------------------------------------------
                                //
                                if (addon.inFrame && (executeContext.addonType == CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml)) {
                                    //
                                    // -- remote method called from inframe execution
                                    result = "TBD - remotemethod inframe";
                                    // Add-on setup for InFrame, running the call-back - this page must think it is just the remotemethod
                                    //If True Then
                                    //    Call core.doc.addRefreshQueryString(RequestNameRemoteMethodAddon, addon.id.ToString)
                                    //    Call core.doc.addRefreshQueryString("optionstring", WorkingOptionString)
                                    //End If
                                } else if (addon.asAjax && (executeContext.addonType == CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml)) {
                                    //
                                    // -- remotemethod called from asajax execution
                                    result = "TBD - remotemethod ajax";
                                    //
                                    // Add-on setup for AsAjax, running the call-back - put the referring page's QS as the RQS
                                    // restore form values
                                    //
                                    //If True Then
                                    //    QS = core.docProperties.getText("Hostform")
                                    //    If QS <> "" Then
                                    //        Call core.docProperties.addQueryString(QS)
                                    //    End If
                                    //    '
                                    //    ' restore refresh querystring values
                                    //    '
                                    //    QS = core.docProperties.getText("HostRQS")
                                    //    QSSplit = Split(QS, "&")
                                    //    For Ptr = 0 To UBound(QSSplit)
                                    //        NVPair = QSSplit[Ptr]
                                    //        If NVPair <> "" Then
                                    //            NVSplit = Split(NVPair, "=")
                                    //            If UBound(NVSplit) > 0 Then
                                    //                Call core.doc.addRefreshQueryString(NVSplit(0), NVSplit(1))
                                    //            End If
                                    //        End If
                                    //    Next
                                    //    '
                                    //    ' restore query string
                                    //    '
                                    //    QS = core.docProperties.getText("HostQS")
                                    //    Call core.docProperties.addQueryString(QS)
                                    //    '
                                    //    ' Clear the style,js and meta features that were delivered to the host page
                                    //    ' After processing, if these strings are not empty, they must have been added by the DLL
                                    //    '
                                    //    '
                                    //    JSOnLoad = ""
                                    //    JSBodyEnd = ""
                                    //    PageTitle = ""
                                    //    MetaDescription = ""
                                    //    MetaKeywordList = ""
                                    //    OtherHeadTags = ""
                                    //    addon.StylesFilename.filename = ""
                                    //    '  CustomStylesFilename = ""
                                    //End If
                                }
                                //
                                //-----------------------------------------------------------------
                                // Do replacements from Option String and Pick out WrapperID, and AsAjax
                                //-----------------------------------------------------------------
                                //
                                string testString = (addon.copy + addon.copyText + addon.pageTitle + addon.metaDescription + addon.metaKeywordList + addon.otherHeadTags + addon.formXML).ToLowerInvariant();
                                if (!string.IsNullOrWhiteSpace(testString)) {
                                    foreach (var key in core.docProperties.getKeyList()) {
                                        if (testString.Contains(("$" + key + "$").ToLowerInvariant())) {
                                            string ReplaceSource = "$" + key + "$";
                                            string ReplaceValue = core.docProperties.getText(key);
                                            addon.copy = addon.copy.Replace(ReplaceSource, ReplaceValue, StringComparison.CurrentCultureIgnoreCase);
                                            addon.copyText = addon.copyText.Replace(ReplaceSource, ReplaceValue, StringComparison.CurrentCultureIgnoreCase);
                                            addon.pageTitle = addon.pageTitle.Replace(ReplaceSource, ReplaceValue, StringComparison.CurrentCultureIgnoreCase);
                                            addon.metaDescription = addon.metaDescription.Replace(ReplaceSource, ReplaceValue, StringComparison.CurrentCultureIgnoreCase);
                                            addon.metaKeywordList = addon.metaKeywordList.Replace(ReplaceSource, ReplaceValue, StringComparison.CurrentCultureIgnoreCase);
                                            addon.otherHeadTags = addon.otherHeadTags.Replace(ReplaceSource, ReplaceValue, StringComparison.CurrentCultureIgnoreCase);
                                            addon.formXML = addon.formXML.Replace(ReplaceSource, ReplaceValue, StringComparison.CurrentCultureIgnoreCase);
                                        }
                                    }
                                }
                                //
                                // -- text components
                                string contentParts = addon.copyText + addon.copy;
                                if (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextEditor) {
                                    //
                                    // not editor, encode the content parts of the addon
                                    //
                                    switch (executeContext.addonType) {
                                        case CPUtilsBaseClass.addonContext.ContextEditor:
                                            contentParts = ActiveContentController.renderHtmlForWysiwygEditor(core, contentParts);
                                            break;
                                        case CPUtilsBaseClass.addonContext.ContextEmail:
                                            contentParts = ActiveContentController.renderHtmlForEmail(core, contentParts, executeContext.personalizationPeopleId, "");
                                            break;
                                        case CPUtilsBaseClass.addonContext.ContextFilter:
                                        case CPUtilsBaseClass.addonContext.ContextOnBodyEnd:
                                        case CPUtilsBaseClass.addonContext.ContextOnBodyStart:
                                        case CPUtilsBaseClass.addonContext.ContextOnPageEnd:
                                        case CPUtilsBaseClass.addonContext.ContextOnPageStart:
                                        case CPUtilsBaseClass.addonContext.ContextPage:
                                        case CPUtilsBaseClass.addonContext.ContextTemplate:
                                        case CPUtilsBaseClass.addonContext.ContextAdmin:
                                        case CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml:
                                            contentParts = ActiveContentController.renderHtmlForWeb(core, contentParts, executeContext.hostRecord.contentName, executeContext.hostRecord.recordId, executeContext.personalizationPeopleId, "", 0, executeContext.addonType);
                                            break;
                                        case CPUtilsBaseClass.addonContext.ContextOnContentChange:
                                        case CPUtilsBaseClass.addonContext.ContextSimple:
                                            contentParts = ActiveContentController.renderHtmlForWeb(core, contentParts, "", 0, executeContext.personalizationPeopleId, "", 0, executeContext.addonType);
                                            break;
                                        case CPUtilsBaseClass.addonContext.ContextRemoteMethodJson:
                                            contentParts = ActiveContentController.renderJSONForRemoteMethod(core, contentParts, "", 0, executeContext.personalizationPeopleId, "", 0, "", executeContext.addonType);
                                            break;
                                        default:
                                            contentParts = ActiveContentController.renderHtmlForWeb(core, contentParts, "", 0, executeContext.personalizationPeopleId, "", 0, executeContext.addonType);
                                            break;
                                    }
                                }
                                result += contentParts;
                                //
                                // -- Scripting code
                                if (addon.scriptingCode != "") {
                                    try {
                                        if (addon.scriptingLanguageID == (int)ScriptLanguages.Javascript ) {
                                            result += execute_Script_JScript(ref addon);
                                        } else {
                                            result += execute_Script_VBScript(ref addon);
                                        }
                                    } catch (Exception ex) {
                                        string addonDescription = getAddonDescription(core, addon);
                                        throw new GenericException("There was an error executing the script component of Add-on " + addonDescription + ". The details of this error follow.</p><p>" + ex.InnerException.Message + "");
                                    }
                                }
                                //
                                // -- DotNet
                                if (addon.dotNetClass != "") {
                                    result += execute_assembly(executeContext, addon, AddonCollectionModel.create<AddonCollectionModel>(core, addon.collectionID));
                                }
                                //
                                // -- RemoteAssetLink
                                if (addon.remoteAssetLink != "") {
                                    string RemoteAssetLink = addon.remoteAssetLink;
                                    if (RemoteAssetLink.IndexOf("://") < 0) {
                                        //
                                        // use request object to build link
                                        if (RemoteAssetLink.Left(1) == "/") {
                                            // asset starts with a slash, add to appRoot
                                            RemoteAssetLink = core.webServer.requestProtocol + core.webServer.requestDomain + RemoteAssetLink;
                                        } else {
                                            // asset is public files
                                            RemoteAssetLink = core.webServer.requestProtocol + core.webServer.requestDomain + core.appConfig.cdnFileUrl + RemoteAssetLink;
                                        }
                                    }
                                    int PosStart = 0;
                                    HttpRequestController kmaHTTP = new HttpRequestController();
                                    string RemoteAssetContent = kmaHTTP.getURL(ref RemoteAssetLink);
                                    int Pos = GenericController.vbInstr(1, RemoteAssetContent, "<body", 1);
                                    if (Pos > 0) {
                                        Pos = GenericController.vbInstr(Pos, RemoteAssetContent, ">");
                                        if (Pos > 0) {
                                            PosStart = Pos + 1;
                                            Pos = GenericController.vbInstr(Pos, RemoteAssetContent, "</body", 1);
                                            if (Pos > 0) {
                                                RemoteAssetContent = RemoteAssetContent.Substring(PosStart - 1, Pos - PosStart);
                                            }
                                        }
                                    }
                                    result += RemoteAssetContent;
                                }
                                //
                                // --  FormXML
                                if (addon.formXML != "") {
                                    bool ExitAddonWithBlankResponse = false;
                                    result += execute_formContent(null, addon.formXML, ref ExitAddonWithBlankResponse, "addon [" + addon.name + "]");
                                    if (ExitAddonWithBlankResponse) {
                                        return string.Empty;
                                    }
                                }
                                //
                                // -- Script Callback
                                if (addon.link != "") {
                                    string callBackLink = encodeVirtualPath(addon.link, core.appConfig.cdnFileUrl, appRootPath, core.webServer.requestDomain);
                                    foreach (var key in core.docProperties.getKeyList()) {
                                        callBackLink = modifyLinkQuery(callBackLink, encodeRequestVariable(key), encodeRequestVariable(core.docProperties.getText(key)), true);
                                    }
                                    foreach (var kvp in executeContext.argumentKeyValuePairs) {
                                        callBackLink = modifyLinkQuery(callBackLink, encodeRequestVariable(kvp.Key), encodeRequestVariable(core.docProperties.getText(kvp.Value)), true);
                                    }
                                    result += "<SCRIPT LANGUAGE=\"JAVASCRIPT\" SRC=\"" + callBackLink + "\"></SCRIPT>";
                                }
                                string AddedByName = addon.name + " addon";
                                //
                                // -- js head links
                                if (addon.jsHeadScriptSrc != "") {
                                    core.html.addScriptLinkSrc(addon.jsHeadScriptSrc, AddedByName + " Javascript Head Src", (executeContext.forceJavascriptToHead || addon.javascriptForceHead), addon.id);
                                }
                                //
                                // -- js head code
                                if (addon.jsFilename.filename != "") {
                                    string scriptFilename = GenericController.getCdnFileLink(core, addon.jsFilename.filename);
                                    //string scriptFilename = core.webServer.requestProtocol + core.webServer.requestDomain + genericController.getCdnFileLink(core, addon.JSFilename.filename);
                                    core.html.addScriptLinkSrc(scriptFilename, AddedByName + " Javascript Head Code", (executeContext.forceJavascriptToHead || addon.javascriptForceHead), addon.id);
                                }
                                //
                                // -- non-js html assets (styles,head tags), set flag to block duplicates 
                                if (!core.doc.addonIdListRunInThisDoc.Contains(addon.id)) {
                                    core.doc.addonIdListRunInThisDoc.Add(addon.id);
                                    core.html.addTitle(addon.pageTitle, AddedByName);
                                    core.html.addMetaDescription(addon.metaDescription, AddedByName);
                                    core.html.addMetaKeywordList(addon.metaKeywordList, AddedByName);
                                    core.html.addHeadTag(addon.otherHeadTags, AddedByName);
                                    ////
                                    //// -- js body links
                                    //if (addon.JSBodyScriptSrc != "") {
                                    //    core.html.addScriptLink_Body(addon.JSBodyScriptSrc, AddedByName + " Javascript Body Src");
                                    //}
                                    ////
                                    //// -- js body code
                                    //core.html.addScriptCode_body(addon.JavaScriptBodyEnd, AddedByName + " Javascript Body Code");
                                    //
                                    // -- styles
                                    if (addon.stylesFilename.filename != "") {
                                        core.html.addStyleLink(GenericController.getCdnFileLink(core, addon.stylesFilename.filename), addon.name + " Stylesheet");
                                    }
                                    //
                                    // -- link to stylesheet
                                    if (addon.stylesLinkHref != "") {
                                        core.html.addStyleLink(addon.stylesLinkHref, addon.name + " Stylesheet Link");
                                    }
                                }
                                //
                                // -- Add Css containers
                                if (!string.IsNullOrEmpty(ContainerCssID) || !string.IsNullOrEmpty(ContainerCssClass)) {
                                    if (addon.isInline) {
                                        result = "\r<span id=\"" + ContainerCssID + "\" class=\"" + ContainerCssClass + "\" style=\"display:inline;\">" + result + "</span>";
                                    } else {
                                        result = "\r<div id=\"" + ContainerCssID + "\" class=\"" + ContainerCssClass + "\">" + nop(result) + "\r</div>";
                                    }
                                }
                            }
                            //
                            //   Add Wrappers to content
                            if (addon.inFrame && (executeContext.addonType == CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml)) {
                                //
                                // -- iFrame content, framed in content, during the remote method call, add in the rest of the html page
                                core.doc.setMetaContent(0, 0);
                                result = ""
                                    + core.siteProperties.docTypeDeclaration + "\r\n<html>"
                                    + "\r\n<head>"
                                    + core.html.getHtmlHead()
                                    + "\r\n</head>"
                                    + "\r\n" + TemplateDefaultBodyTag
                                    + "\r\n</body>"
                                    + "\r\n</html>";
                            } else if (addon.asAjax && (executeContext.addonType == CPUtilsBaseClass.addonContext.ContextRemoteMethodJson)) {
                                //
                                // -- as ajax content, AsAjax addon, during the Ajax callback, need to create an onload event that runs everything appended to onload within this content
                            } else if ((executeContext.addonType == CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) || (executeContext.addonType == CPUtilsBaseClass.addonContext.ContextRemoteMethodJson)) {
                                //
                                // -- non-ajax/non-Iframe remote method content (no wrapper)
                            } else if (executeContext.addonType == CPUtilsBaseClass.addonContext.ContextEmail) {
                                //
                                // -- return Email context (no wrappers)
                            } else if (executeContext.addonType == CPUtilsBaseClass.addonContext.ContextSimple) {
                                //
                                // -- add-on called by another add-on, subroutine style (no wrappers)
                            } else {
                                //
                                // -- Return all other types, Enable Edit Wrapper for Page Content edit mode
                                bool IncludeEditWrapper = (!addon.blockEditTools) && (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextEditor) && (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextEmail) && (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodJson) && (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) && (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextSimple) && (!executeContext.isIncludeAddon);
                                if (IncludeEditWrapper) {
                                    IncludeEditWrapper = IncludeEditWrapper && (allowAdvanceEditor && ((executeContext.addonType == CPUtilsBaseClass.addonContext.ContextAdmin) || core.session.isEditing(executeContext.hostRecord.contentName)));
                                    if (IncludeEditWrapper) {
                                        //
                                        // Edit Icon
                                        string EditWrapperHTMLID = "eWrapper" + core.doc.addonInstanceCnt;
                                        string DialogList = "";
                                        string HelpIcon = getHelpBubble(addon.id, addon.help, addon.collectionID, ref DialogList);
                                        if (core.visitProperty.getBoolean("AllowAdvancedEditor")) {
                                            string addonArgumentListPassToBubbleEditor = ""; // comes from method in this class the generates it from addon and instance properites - lost it in the shuffle
                                            string AddonEditIcon = getIconSprite("", 0, "/ContensiveBase/images/tooledit.png", 22, 22, "Edit the " + addon.name + " Add-on", "Edit the " + addon.name + " Add-on", "", true, "");
                                            AddonEditIcon = "<a href=\"/" + core.appConfig.adminRoute + "?cid=" + CdefController.getContentId(core, Models.Db.AddonModel.contentName) + "&id=" + addon.id + "&af=4&aa=2&ad=1\" tabindex=\"-1\">" + AddonEditIcon + "</a>";
                                            string InstanceSettingsEditIcon = getInstanceBubble(addon.name, addonArgumentListPassToBubbleEditor, executeContext.hostRecord.contentName, executeContext.hostRecord.recordId, executeContext.hostRecord.fieldName, executeContext.instanceGuid, executeContext.addonType, ref DialogList);
                                            string HTMLViewerEditIcon = getHTMLViewerBubble(addon.id, "editWrapper" + core.doc.editWrapperCnt, ref DialogList);
                                            string SiteStylesEditIcon = ""; // ?????
                                            string ToolBar = InstanceSettingsEditIcon + AddonEditIcon + getAddonStylesBubble(addon.id, ref DialogList) + SiteStylesEditIcon + HTMLViewerEditIcon + HelpIcon;
                                            ToolBar = GenericController.vbReplace(ToolBar, "&nbsp;", "", 1, 99, 1);
                                            result = AdminUIController.getEditWrapper(core, "<div class=\"ccAddonEditTools\">" + ToolBar + "&nbsp;" + addon.name + DialogList + "</div>", result);
                                        } else if (core.visitProperty.getBoolean("AllowEditing")) {
                                            result = AdminUIController.getEditWrapper(core, "<div class=\"ccAddonEditCaption\">" + addon.name + "&nbsp;" + HelpIcon + "</div>", result);
                                        }
                                    }
                                }
                                //
                                // -- Add Comment wrapper, to help debugging except email, remote methods and admin (empty is used to detect no result)
                                if (true && (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextAdmin) && (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextEmail) && (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) && (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodJson) && (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextSimple)) {
                                    if (core.visitProperty.getBoolean("AllowDebugging")) {
                                        string AddonCommentName = GenericController.vbReplace(addon.name, "-->", "..>");
                                        if (addon.isInline) {
                                            result = "<!-- Add-on " + AddonCommentName + " -->" + result + "<!-- /Add-on " + AddonCommentName + " -->";
                                        } else {
                                            result = "\r<!-- Add-on " + AddonCommentName + " -->" + nop(result) + "\r<!-- /Add-on " + AddonCommentName + " -->";
                                        }
                                    }
                                }
                                //
                                // -- Add Design Wrapper
                                if ((!string.IsNullOrEmpty(result)) && (!addon.isInline) && (executeContext.wrapperID > 0)) {
                                    result = addWrapperToResult(result, executeContext.wrapperID, "for Add-on " + addon.name);
                                }
                                // -- restore the parent's instanceId
                                core.docProperties.setProperty("instanceId", parentInstanceId);
                            }
                            //
                            // -- unwind recursion count
                            if (core.doc.addonRecursionDepth.ContainsKey(addon.id)) {
                                if( --core.doc.addonRecursionDepth[addon.id] <=0 ) {
                                    core.doc.addonRecursionDepth.Remove(addon.id);
                                }
                            }
                        }
                    }
                } catch (Exception ex) {
                    LogController.handleError(core, ex);
                } finally {
                    //
                    // -- this completes the execute of this core.addon. remove it from the 'running' list
                    core.doc.addonInstanceCnt = core.doc.addonInstanceCnt + 1;
                    //
                    // -- restore the forceJavascriptToHead value of the caller
                    executeContext.forceJavascriptToHead = save_forceJavascriptToHead;
                    //
                    // -- if root level addon, and the addon is an html document, create the html document around it and uglify if not debugging
                    if ((executeContext.forceHtmlDocument) || ((rootLevelAddon) && (addon.htmlDocument))) {
                        result = core.html.getHtmlDoc(result, "<body>");
                        if ((!core.doc.visitPropertyAllowDebugging) && (core.siteProperties.getBoolean("Allow Html Minify", true))) {
                            result = NUglify.Uglify.Html(result).Code;
                        }
                    }
                    //
                    // -- pop modelstack and test point message
                    core.doc.addonModelStack.Pop();
                }
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute the xml part of an addon, return html
        /// </summary>
        /// <param name="nothingObject"></param>
        /// <param name="FormXML"></param>
        /// <param name="return_ExitAddonBlankWithResponse"></param>
        /// <returns></returns>
        private string execute_formContent(object nothingObject, string FormXML, ref bool return_ExitAddonBlankWithResponse, string contextErrorMessage) {
            string result = "";
            try {
                // todo - move locals
                string fieldfilename = null;
                string FieldDataSource = null;
                string FieldSQL = null;
                StringBuilderLegacyController Content = new StringBuilderLegacyController();
                string Copy = null;
                string Button = null;
                string ButtonList = "";
                string Filename = null;
                string NonEncodedLink = null;
                string EncodedLink = null;
                string VirtualFilePath = null;
                string TabName = null;
                string TabDescription = null;
                string TabHeading = null;
                int TabCnt = 0;
                StringBuilderLegacyController TabCell = null;
                bool loadOK = true;
                string FieldValue = "";
                string FieldDescription = null;
                string FieldDefaultValue = null;
                bool IsFound = false;
                string Name = "";
                string Description = "";
                XmlDocument Doc = new XmlDocument();
                int CS = 0;
                string FieldName = null;
                string FieldCaption = null;
                string FieldAddon = null;
                bool FieldReadOnly = false;
                bool FieldHTML = false;
                string fieldType = null;
                string FieldSelector = null;
                string DefaultFilename = null;
                var adminMenu = new TabController();
                //
                Button = core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    //
                    // Cancel just exits with no content
                    //
                    return_ExitAddonBlankWithResponse = true;
                    return string.Empty;
                } else if (!core.session.isAuthenticatedAdmin(core)) {
                    //
                    // Not Admin Error
                    //
                    ButtonList = ButtonCancel;
                    Content.Add(AdminUIController.getFormBodyAdminOnly());
                } else {
                    if (true) {
                        loadOK = true;
                        try {
                            Doc.LoadXml(FormXML);
                        } catch (Exception) {
                            ButtonList = ButtonCancel;
                            Content.Add("<div class=\"ccError\" style=\"margin:10px;padding:10px;background-color:white;\">There was a problem with the Setting Page you requested.</div>");
                            loadOK = false;
                        }
                        if (loadOK) {
                            //
                            // data is OK
                            //
                            if (GenericController.vbLCase(Doc.DocumentElement.Name) != "form") {
                                //
                                // error - Need a way to reach the user that submitted the file
                                //
                                ButtonList = ButtonCancel;
                                Content.Add("<div class=\"ccError\" style=\"margin:10px;padding:10px;background-color:white;\">There was a problem with the Setting Page you requested.</div>");
                            } else {
                                //
                                // ----- Process Requests
                                //
                                if ((Button == ButtonSave) || (Button == ButtonOK)) {
                                    foreach (XmlNode SettingNode in Doc.DocumentElement.ChildNodes) {
                                        switch (GenericController.vbLCase(SettingNode.Name)) {
                                            case "tab":
                                                foreach (XmlNode TabNode in SettingNode.ChildNodes) {
                                                    switch (GenericController.vbLCase(TabNode.Name)) {
                                                        case "siteproperty":
                                                            //
                                                            FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
                                                            FieldValue = core.docProperties.getText(FieldName);
                                                            fieldType = xml_GetAttribute(IsFound, TabNode, "type", "");
                                                            switch (GenericController.vbLCase(fieldType)) {
                                                                case "integer":
                                                                    //
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        FieldValue = GenericController.encodeInteger(FieldValue).ToString();
                                                                    }
                                                                    core.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                case "boolean":
                                                                    //
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        FieldValue = GenericController.encodeBoolean(FieldValue).ToString();
                                                                    }
                                                                    core.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                case "float":
                                                                    //
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        FieldValue = encodeNumber(FieldValue).ToString();
                                                                    }
                                                                    core.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                case "date":
                                                                    //
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        FieldValue = GenericController.encodeDate(FieldValue).ToString();
                                                                    }
                                                                    core.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                case "file":
                                                                case "imagefile":
                                                                    //
                                                                    if (core.docProperties.getBoolean(FieldName + ".DeleteFlag")) {
                                                                        core.siteProperties.setProperty(FieldName, "");
                                                                    }
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        Filename = FieldValue;
                                                                        VirtualFilePath = "Settings/" + FieldName + "/";
                                                                        core.cdnFiles.upload(FieldName, VirtualFilePath, ref Filename);
                                                                        core.siteProperties.setProperty(FieldName, VirtualFilePath + Filename);
                                                                    }
                                                                    break;
                                                                case "textfile":
                                                                    //
                                                                    DefaultFilename = "Settings/" + FieldName + ".txt";
                                                                    Filename = core.siteProperties.getText(FieldName, DefaultFilename);
                                                                    if (string.IsNullOrEmpty(Filename)) {
                                                                        Filename = DefaultFilename;
                                                                        core.siteProperties.setProperty(FieldName, DefaultFilename);
                                                                    }
                                                                    core.appRootFiles.saveFile(Filename, FieldValue);
                                                                    break;
                                                                case "cssfile":
                                                                    //
                                                                    DefaultFilename = "Settings/" + FieldName + ".css";
                                                                    Filename = core.siteProperties.getText(FieldName, DefaultFilename);
                                                                    if (string.IsNullOrEmpty(Filename)) {
                                                                        Filename = DefaultFilename;
                                                                        core.siteProperties.setProperty(FieldName, DefaultFilename);
                                                                    }
                                                                    core.appRootFiles.saveFile(Filename, FieldValue);
                                                                    break;
                                                                case "xmlfile":
                                                                    //
                                                                    DefaultFilename = "Settings/" + FieldName + ".xml";
                                                                    Filename = core.siteProperties.getText(FieldName, DefaultFilename);
                                                                    if (string.IsNullOrEmpty(Filename)) {
                                                                        Filename = DefaultFilename;
                                                                        core.siteProperties.setProperty(FieldName, DefaultFilename);
                                                                    }
                                                                    core.appRootFiles.saveFile(Filename, FieldValue);
                                                                    break;
                                                                case "currency":
                                                                    //
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        FieldValue = encodeNumber(FieldValue).ToString();
                                                                        FieldValue = String.Format("C", FieldValue);
                                                                    }
                                                                    core.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                case "link":
                                                                    core.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                default:
                                                                    core.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                            }
                                                            break;
                                                        case "copycontent":
                                                            //
                                                            // A Copy Content block
                                                            //
                                                            FieldReadOnly = GenericController.encodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
                                                            if (!FieldReadOnly) {
                                                                FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
                                                                FieldHTML = GenericController.encodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", "false"));
                                                                if (FieldHTML) {
                                                                    //
                                                                    // treat html as active content for now.
                                                                    //
                                                                    FieldValue = core.docProperties.getRenderedActiveContent(FieldName);
                                                                } else {
                                                                    FieldValue = core.docProperties.getText(FieldName);
                                                                }

                                                                CS = core.db.csOpen("Copy Content", "name=" + DbController.encodeSQLText(FieldName), "ID");
                                                                if (!core.db.csOk(CS)) {
                                                                    core.db.csClose(ref CS);
                                                                    CS = core.db.csInsertRecord("Copy Content", core.session.user.id);
                                                                }
                                                                if (core.db.csOk(CS)) {
                                                                    core.db.csSet(CS, "name", FieldName);
                                                                    //
                                                                    // Set copy
                                                                    //
                                                                    core.db.csSet(CS, "copy", FieldValue);
                                                                    //
                                                                    // delete duplicates
                                                                    //
                                                                    core.db.csGoNext(CS);
                                                                    while (core.db.csOk(CS)) {
                                                                        core.db.csDeleteRecord(CS);
                                                                        core.db.csGoNext(CS);
                                                                    }
                                                                }
                                                                core.db.csClose(ref CS);
                                                            }

                                                            break;
                                                        case "filecontent":
                                                            //
                                                            // A File Content block
                                                            //
                                                            FieldReadOnly = GenericController.encodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
                                                            if (!FieldReadOnly) {
                                                                FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
                                                                fieldfilename = xml_GetAttribute(IsFound, TabNode, "filename", "");
                                                                FieldValue = core.docProperties.getText(FieldName);
                                                                core.appRootFiles.saveFile(fieldfilename, FieldValue);
                                                            }
                                                            break;
                                                        case "dbquery":
                                                            //
                                                            // dbquery has no results to process
                                                            //
                                                            break;
                                                    }
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                                if (Button == ButtonOK) {
                                    //
                                    // Exit on OK or cancel
                                    //
                                    return_ExitAddonBlankWithResponse = true;
                                    return string.Empty;
                                }
                                //
                                // ----- Display Form
                                //
                                //Content.Add(AdminUIController.editTableOpen);
                                Name = xml_GetAttribute(IsFound, Doc.DocumentElement, "name", "");
                                foreach (XmlNode SettingNode in Doc.DocumentElement.ChildNodes) {
                                    switch (GenericController.vbLCase(SettingNode.Name)) {
                                        case "description":
                                            Description = SettingNode.InnerText;
                                            break;
                                        case "tab":
                                            TabCnt = TabCnt + 1;
                                            TabName = xml_GetAttribute(IsFound, SettingNode, "name", "");
                                            TabDescription = xml_GetAttribute(IsFound, SettingNode, "description", "");
                                            TabHeading = xml_GetAttribute(IsFound, SettingNode, "heading", "");
                                            if (TabHeading == "Debug and Trace Settings") {
                                                //TabHeading = TabHeading;
                                            }
                                            TabCell = new StringBuilderLegacyController();
                                            foreach (XmlNode TabNode in SettingNode.ChildNodes) {
                                                int SQLPageSize = 0;
                                                int ErrorNumber = 0;
                                                switch (GenericController.vbLCase(TabNode.Name)) {
                                                    case "heading":
                                                        //
                                                        // Heading
                                                        //
                                                        FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "");
                                                        TabCell.Add(AdminUIController.getEditSubheadRow(core, FieldCaption));
                                                        break;
                                                    case "siteproperty":
                                                        //
                                                        // Site property
                                                        //
                                                        FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
                                                        if (!string.IsNullOrEmpty(FieldName)) {
                                                            FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "");
                                                            if (string.IsNullOrEmpty(FieldCaption)) {
                                                                FieldCaption = FieldName;
                                                            }
                                                            FieldReadOnly = GenericController.encodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
                                                            FieldHTML = GenericController.encodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", ""));
                                                            fieldType = xml_GetAttribute(IsFound, TabNode, "type", "");
                                                            FieldSelector = xml_GetAttribute(IsFound, TabNode, "selector", "");
                                                            FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "");
                                                            FieldAddon = xml_GetAttribute(IsFound, TabNode, "EditorAddon", "");
                                                            FieldDefaultValue = TabNode.InnerText;
                                                            FieldValue = core.siteProperties.getText(FieldName, FieldDefaultValue);
                                                            if (!string.IsNullOrEmpty(FieldAddon)) {
                                                                //
                                                                // Use Editor Addon
                                                                //
                                                                Dictionary<string, string> arguments = new Dictionary<string, string> {
                                                                    { "FieldName", FieldName },
                                                                    { "FieldValue", core.siteProperties.getText(FieldName, FieldDefaultValue) }
                                                                };
                                                                //OptionString = "FieldName=" & FieldName & "&FieldValue=" & encodeNvaArgument(core.siteProperties.getText(FieldName, FieldDefaultValue))
                                                                AddonModel addon = AddonModel.createByUniqueName(core, FieldAddon);
                                                                Copy = core.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext() {
                                                                    addonType = CPUtilsBaseClass.addonContext.ContextAdmin,
                                                                    argumentKeyValuePairs = arguments,
                                                                    errorContextMessage = "executing field addon [" + FieldAddon + "] for " + contextErrorMessage
                                                                });
                                                                //Copy = execute_legacy5(0, FieldAddon, OptionString, CPUtilsBaseClass.addonContext.ContextAdmin, "", 0, "", 0)
                                                            } else if (!string.IsNullOrEmpty(FieldSelector)) {
                                                                //
                                                                // Use Selector
                                                                //
                                                                Copy = AdminUIController.getDefaultEditor_SelectorString(core, FieldName, FieldValue, FieldSelector);
                                                            } else {
                                                                //
                                                                // Use default editor for each field type
                                                                //
                                                                switch (GenericController.vbLCase(fieldType)) {
                                                                    case "integer":
                                                                        //
                                                                        Copy = AdminUIController.getDefaultEditor_Text(core, FieldName, FieldValue, FieldReadOnly);
                                                                        //if (FieldReadOnly) {
                                                                        //    Copy = FieldValue + htmlController.inputHidden(FieldName, FieldValue);
                                                                        //} else {
                                                                        //    Copy = htmlController.inputText(core, FieldName, FieldValue);
                                                                        //}
                                                                        break;
                                                                    case "boolean":
                                                                        Copy = AdminUIController.getDefaultEditor_Bool(core, FieldName, GenericController.encodeBoolean(FieldValue), FieldReadOnly);
                                                                        //if (FieldReadOnly) {
                                                                        //    Copy = core.html.inputCheckbox(FieldName, genericController.encodeBoolean(FieldValue));
                                                                        //    Copy = genericController.vbReplace(Copy, ">", " disabled>");
                                                                        //    Copy += htmlController.inputHidden(FieldName, FieldValue);
                                                                        //} else {
                                                                        //    Copy = core.html.inputCheckbox(FieldName, genericController.encodeBoolean(FieldValue));
                                                                        //}
                                                                        break;
                                                                    case "float":
                                                                        Copy = AdminUIController.getDefaultEditor_Text(core, FieldName, FieldValue, FieldReadOnly);
                                                                        //if (FieldReadOnly) {
                                                                        //    Copy = FieldValue + htmlController.inputHidden(FieldName, FieldValue);
                                                                        //} else {
                                                                        //    Copy = htmlController.inputText(core, FieldName, FieldValue);
                                                                        //}
                                                                        break;
                                                                    case "date":
                                                                        Copy = AdminUIController.getDefaultEditor_DateTime(core, FieldName, encodeDate(FieldValue), FieldReadOnly);

                                                                        //if (FieldReadOnly) {
                                                                        //    Copy = FieldValue + htmlController.inputHidden(FieldName, FieldValue);
                                                                        //} else {
                                                                        //    Copy = htmlController.inputDate(core, FieldName, encodeDate(FieldValue));
                                                                        //}
                                                                        break;
                                                                    case "file":
                                                                    case "imagefile":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + HtmlController.inputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            if (string.IsNullOrEmpty(FieldValue)) {
                                                                                Copy = core.html.inputFile(FieldName);
                                                                            } else {
                                                                                NonEncodedLink = GenericController.getCdnFileLink(core, FieldValue);
                                                                                //NonEncodedLink = core.webServer.requestDomain + genericController.getCdnFileLink(core, FieldValue);
                                                                                EncodedLink = encodeURL(NonEncodedLink);
                                                                                string FieldValuefilename = "";
                                                                                string FieldValuePath = "";
                                                                                core.privateFiles.splitDosPathFilename(FieldValue, ref FieldValuePath, ref FieldValuefilename);
                                                                                Copy = ""
                                                                                + "<a href=\"http://" + EncodedLink + "\" target=\"_blank\">[" + FieldValuefilename + "]</A>"
                                                                                + "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" + HtmlController.checkbox(FieldName + ".DeleteFlag", false) + "&nbsp;&nbsp;&nbsp;Change:&nbsp;" + core.html.inputFile(FieldName);
                                                                            }
                                                                        }
                                                                        //Call s.Add("&nbsp;</span></nobr></td>")
                                                                        break;
                                                                    case "currency":
                                                                        //
                                                                        if (!string.IsNullOrEmpty(FieldValue)) {
                                                                            FieldValue = String.Format("C", FieldValue);
                                                                        }
                                                                        Copy = AdminUIController.getDefaultEditor_Text(core, FieldName, FieldValue, FieldReadOnly);
                                                                        //if (FieldReadOnly) {
                                                                        //    Copy = FieldValue + htmlController.inputHidden(FieldName, FieldValue);
                                                                        //} else {
                                                                        //    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        //        FieldValue = String.Format("C", FieldValue);
                                                                        //    }
                                                                        //    Copy = htmlController.inputText(core, FieldName, FieldValue);
                                                                        //}
                                                                        break;
                                                                    case "textfile":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + HtmlController.inputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            //FieldValue = core.cdnFiles.readFileText(FieldValue);
                                                                            Copy = AdminUIController.getDefaultEditor_TextArea(core, FieldName, FieldValue, FieldReadOnly);
                                                                            //if (FieldHTML) {
                                                                            //    Copy = core.html.getFormInputHTML(FieldName, FieldValue);
                                                                            //} else {
                                                                            //    Copy = htmlController.inputTextarea(core, FieldName, FieldValue, 5);
                                                                            //}
                                                                        }
                                                                        break;
                                                                    case "cssfile":
                                                                        //
                                                                        Copy = AdminUIController.getDefaultEditor_TextArea(core, FieldName, FieldValue, FieldReadOnly);
                                                                        //if (FieldReadOnly) {
                                                                        //    Copy = FieldValue + htmlController.inputHidden(FieldName, FieldValue);
                                                                        //} else {
                                                                        //    Copy = htmlController.inputTextarea(core, FieldName, FieldValue, 5);
                                                                        //}
                                                                        break;
                                                                    case "xmlfile":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + HtmlController.inputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = HtmlController.inputTextarea(core, FieldName, FieldValue, 5);
                                                                        }
                                                                        break;
                                                                    case "link":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + HtmlController.inputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = HtmlController.inputText(core, FieldName, FieldValue);
                                                                        }
                                                                        break;
                                                                    default:
                                                                        //
                                                                        // text
                                                                        //
                                                                        if (FieldHTML) {
                                                                            Copy = AdminUIController.getDefaultEditor_Html(core, FieldName, FieldValue, "", "", "", FieldReadOnly);
                                                                            //Copy = core.html.getFormInputHTML(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = AdminUIController.getDefaultEditor_Text(core, FieldName, FieldValue, FieldReadOnly);
                                                                            //Copy = htmlController.inputText(core, FieldName, FieldValue);
                                                                        }
                                                                        //if (FieldReadOnly) {
                                                                        //    string tmp = htmlController.inputHidden(FieldName, FieldValue);
                                                                        //    Copy = FieldValue + tmp;
                                                                        //} else {
                                                                        //    if (FieldHTML) {
                                                                        //        Copy = adminUIController.getDefaultEditor_Html(core, FieldName, FieldValue, "","","",FieldReadOnly);
                                                                        //        //Copy = core.html.getFormInputHTML(FieldName, FieldValue);
                                                                        //    } else {
                                                                        //        Copy = adminUIController.getDefaultEditor_Text(core, FieldName, FieldValue, FieldReadOnly);
                                                                        //        //Copy = htmlController.inputText(core, FieldName, FieldValue);
                                                                        //    }
                                                                        //}
                                                                        break;
                                                                }
                                                            }
                                                            TabCell.Add(AdminUIController.getEditRowLegacy(core, Copy, FieldCaption, FieldDescription, false, false, ""));
                                                        }
                                                        break;
                                                    case "copycontent":
                                                        //
                                                        // Content Copy field
                                                        //
                                                        FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
                                                        if (!string.IsNullOrEmpty(FieldName)) {
                                                            FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "");
                                                            if (string.IsNullOrEmpty(FieldCaption)) {
                                                                FieldCaption = FieldName;
                                                            }
                                                            FieldReadOnly = GenericController.encodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
                                                            FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "");
                                                            FieldHTML = GenericController.encodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", ""));
                                                            //
                                                            CS = core.db.csOpen("Copy Content", "Name=" + DbController.encodeSQLText(FieldName), "ID", false, 0, false, false, "id,name,Copy");
                                                            if (!core.db.csOk(CS)) {
                                                                core.db.csClose(ref CS);
                                                                CS = core.db.csInsertRecord("Copy Content", core.session.user.id);
                                                                if (core.db.csOk(CS)) {
                                                                    int RecordID = core.db.csGetInteger(CS, "ID");
                                                                    core.db.csSet(CS, "name", FieldName);
                                                                    core.db.csSet(CS, "copy", GenericController.encodeText(TabNode.InnerText));
                                                                    core.db.csSave(CS);
                                                                    // Call WorkflowController.publishEdit("Copy Content", RecordID)
                                                                }
                                                            }
                                                            if (core.db.csOk(CS)) {
                                                                FieldValue = core.db.csGetText(CS, "copy");
                                                            }
                                                            if (FieldHTML) {
                                                                Copy = AdminUIController.getDefaultEditor_Html(core, FieldName, FieldValue, "", "", "", FieldReadOnly);
                                                                //Copy = core.html.getFormInputHTML(FieldName, FieldValue);
                                                            } else {
                                                                Copy = AdminUIController.getDefaultEditor_Text(core, FieldName, FieldValue, FieldReadOnly);
                                                                //Copy = htmlController.inputText(core, FieldName, FieldValue);
                                                            }
                                                            //if (FieldReadOnly) {
                                                            //    //
                                                            //    // Read only
                                                            //    //
                                                            //    Copy = FieldValue;
                                                            //} else if (FieldHTML) {
                                                            //    //
                                                            //    // HTML
                                                            //    //
                                                            //    Copy = core.html.getFormInputHTML(FieldName, FieldValue);
                                                            //    //Copy = core.main_GetFormInputActiveContent( FieldName, FieldValue)
                                                            //} else {
                                                            //    //
                                                            //    // Text edit
                                                            //    //
                                                            //    Copy = htmlController.inputTextarea(core, FieldName, FieldValue);
                                                            //}
                                                            TabCell.Add(AdminUIController.getEditRowLegacy(core, Copy, FieldCaption, FieldDescription, false, false, ""));
                                                        }
                                                        break;
                                                    case "filecontent":
                                                        //
                                                        // Content from a flat file
                                                        //
                                                        FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
                                                        FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "");
                                                        fieldfilename = xml_GetAttribute(IsFound, TabNode, "filename", "");
                                                        FieldReadOnly = GenericController.encodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
                                                        FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "");
                                                        FieldDefaultValue = TabNode.InnerText;
                                                        FieldValue = TabNode.InnerText;
                                                        FieldHTML = GenericController.encodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", ""));
                                                        if (!string.IsNullOrEmpty(fieldfilename)) {
                                                            if (core.appRootFiles.fileExists(fieldfilename)) {
                                                                FieldValue = core.cdnFiles.readFileText(fieldfilename);
                                                            }
                                                        }
                                                        if (FieldHTML) {
                                                            Copy = AdminUIController.getDefaultEditor_Html(core, FieldName, FieldValue, "", "", "", FieldReadOnly);
                                                            //Copy = core.html.getFormInputHTML(FieldName, FieldValue);
                                                        } else {
                                                            Copy = AdminUIController.getDefaultEditor_Text(core, FieldName, FieldValue, FieldReadOnly);
                                                            //Copy = htmlController.inputText(core, FieldName, FieldValue);
                                                        }
                                                        TabCell.Add(AdminUIController.getEditRowLegacy(core, Copy, FieldCaption, FieldDescription, false, false, ""));
                                                        break;
                                                    case "dbquery":
                                                    case "querydb":
                                                    case "query":
                                                    case "db":
                                                        //
                                                        // Display the output of a query
                                                        //
                                                        Copy = "";
                                                        FieldDataSource = xml_GetAttribute(IsFound, TabNode, "DataSourceName", "");
                                                        FieldSQL = TabNode.InnerText;
                                                        FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "");
                                                        FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "");
                                                        SQLPageSize = GenericController.encodeInteger(xml_GetAttribute(IsFound, TabNode, "rowmax", ""));
                                                        if (SQLPageSize == 0) {
                                                            SQLPageSize = 100;
                                                        }
                                                        //
                                                        // Run the SQL
                                                        //
                                                        DataTable dt = null;
                                                        if (!string.IsNullOrEmpty(FieldSQL)) {
                                                            try {
                                                                dt = core.db.executeQuery(FieldSQL, FieldDataSource, 0, SQLPageSize);
                                                                //RS = app.csv_ExecuteSQLCommand(FieldDataSource, FieldSQL, 30, SQLPageSize, 1)

                                                            } catch (Exception) {
                                                                ErrorNumber = 0;
                                                                loadOK = false;
                                                            }
                                                        }
                                                        if (dt != null) {
                                                            if (string.IsNullOrEmpty(FieldSQL)) {
                                                                //
                                                                // ----- Error
                                                                //
                                                                Copy = "No Result";
                                                            } else if (ErrorNumber != 0) {
                                                                //
                                                                // ----- Error
                                                                //
                                                                //todo  TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                                                                Copy = "Error: ";
                                                            } else if (!DbController.isDataTableOk(dt)) {
                                                                //
                                                                // ----- no result
                                                                //
                                                                Copy = "No Results";
                                                            } else if (dt.Rows.Count == 0) {
                                                                //
                                                                // ----- no result
                                                                //
                                                                Copy = "No Results";
                                                            } else {
                                                                //
                                                                // ----- print results
                                                                //
                                                                if (dt.Rows.Count > 0) {
                                                                    object[,] something = { { } };
                                                                    if (dt.Rows.Count == 1 && dt.Columns.Count == 1) {
                                                                        Copy = HtmlController.inputText(core, "result", GenericController.encodeText(something[0, 0]), 0, 0, "", false, true);
                                                                    } else {
                                                                        foreach (DataRow dr in dt.Rows) {
                                                                            //
                                                                            //Const LoginMode_None = 1
                                                                            //Const LoginMode_AutoRecognize = 2
                                                                            //Const LoginMode_AutoLogin = 3
                                                                            //
                                                                            // Build headers
                                                                            //
                                                                            int FieldCount = dr.ItemArray.Length;
                                                                            Copy += ("\r<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-bottom:1px solid #444;border-right:1px solid #444;background-color:white;color:#444;\">");
                                                                            Copy += ("\r\t<tr>");
                                                                            foreach (DataColumn dc in dr.ItemArray) {
                                                                                Copy += ("\r\t\t<td class=\"ccadminsmall\" style=\"border-top:1px solid #444;border-left:1px solid #444;color:black;padding:2px;padding-top:4px;padding-bottom:4px;\">" + dr[dc].ToString() + "</td>");
                                                                            }
                                                                            Copy += ("\r\t</tr>");
                                                                            //
                                                                            // Build output table
                                                                            //
                                                                            string RowStart = null;
                                                                            string RowEnd = null;
                                                                            string ColumnStart = null;
                                                                            string ColumnEnd = null;
                                                                            RowStart = "\r\t<tr>";
                                                                            RowEnd = "\r\t</tr>";
                                                                            ColumnStart = "\r\t\t<td class=\"ccadminnormal\" style=\"border-top:1px solid #444;border-left:1px solid #444;background-color:white;color:#444;padding:2px\">";
                                                                            ColumnEnd = "</td>";
                                                                            int RowPointer = 0;
                                                                            int RowMax = 0;
                                                                            for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                                                                                Copy += (RowStart);
                                                                                int ColumnPointer = 0;
                                                                                int ColumnMax = 0;
                                                                                for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                                                                    object CellData = something[ColumnPointer, RowPointer];
                                                                                    if (IsNull(CellData)) {
                                                                                        Copy += (ColumnStart + "[null]" + ColumnEnd);
                                                                                    } else if ((CellData == null)) {
                                                                                        Copy += (ColumnStart + "[empty]" + ColumnEnd);
                                                                                    } else if (Microsoft.VisualBasic.Information.IsArray(CellData)) {
                                                                                        Copy += ColumnStart + "[array]";
                                                                                        //Dim Cnt As Integer
                                                                                        //Cnt = UBound(CellData)
                                                                                        //Dim Ptr As Integer
                                                                                        //For Ptr = 0 To Cnt - 1
                                                                                        //    Copy = Copy & ("<br>(" & Ptr & ")&nbsp;[" & CellData[Ptr] & "]")
                                                                                        //Next
                                                                                        //Copy = Copy & (ColumnEnd)
                                                                                    } else if (GenericController.encodeText(CellData) == "") {
                                                                                        Copy += (ColumnStart + "[empty]" + ColumnEnd);
                                                                                    } else {
                                                                                        Copy += (ColumnStart + HtmlController.encodeHtml(GenericController.encodeText(CellData)) + ColumnEnd);
                                                                                    }
                                                                                }
                                                                                Copy += (RowEnd);
                                                                            }
                                                                            Copy += ("\r</table>");
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        TabCell.Add(AdminUIController.getEditRow(core, Copy, FieldCaption, FieldDescription, false, false, ""));
                                                        //TabCell.Add(adminUIController.getEditRowLegacy(core, Copy, FieldCaption, FieldDescription, false, false, ""));
                                                        break;
                                                }
                                            }
                                            Copy = AdminUIController.getEditPanel(core, true, TabHeading, TabDescription, AdminUIController.editTable( TabCell.Text ));
                                            if (!string.IsNullOrEmpty(Copy)) {
                                                adminMenu.addEntry(TabName.Replace(" ", "&nbsp;"), Copy, "ccAdminTab");
                                            }
                                            //Content.Add( GetForm_Edit_AddTab(core,TabName, Copy, True))
                                            TabCell = null;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                //
                                // Buttons
                                //
                                ButtonList = ButtonCancel + "," + ButtonSave + "," + ButtonOK;
                                //
                                // Close Tables
                                //
                                //Content.Add( core.main_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormMobileBrowserControl))
                                //
                                //
                                //
                                if (TabCnt > 0) {
                                    Content.Add(adminMenu.getTabs(core));
                                }
                            }
                        }
                    }
                }
                //
                result = AdminUIController.getBody(core, Name, ButtonList, "", true, true, Description, "", 0, Content.Text);
                Content = null;

            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute vb script
        /// </summary>
        /// <param name="addon"></param>
        /// <param name="Code"></param>
        /// <param name="EntryPoint"></param>
        /// <param name="ScriptingTimeout"></param>
        /// <param name="ScriptName"></param>
        /// <returns></returns>
        private string execute_Script_VBScript(ref AddonModel addon) {
            string returnText = "";
            try {
                // todo - move locals
                var engine = new Microsoft.ClearScript.Windows.VBScriptEngine();
                string[] Args = { };
                string WorkingCode = addon.scriptingCode;
                string entryPoint = addon.scriptingEntryPoint;
                if (string.IsNullOrEmpty(entryPoint)) {
                    //
                    // -- compatibility mode, if no entry point given, if the code starts with "function myFuncton()" and add "call myFunction()"
                    int pos = WorkingCode.IndexOf("function", StringComparison.CurrentCultureIgnoreCase);
                    if (pos >= 0) {
                        entryPoint = WorkingCode.Substring(pos + 9);
                        pos = entryPoint.IndexOf("\r");
                        if (pos > 0) {
                            entryPoint = entryPoint.Substring(0, pos);
                        }
                        pos = entryPoint.IndexOf("\n");
                        if (pos > 0) {
                            entryPoint = entryPoint.Substring(0, pos);
                        }
                        pos = entryPoint.IndexOf("(");
                        if (pos > 0) {
                            entryPoint = entryPoint.Substring(0, pos);
                        }
                    }
                } else {
                    //
                    // -- etnry point provided, remove "()" if included and add to code
                    int pos = entryPoint.IndexOf("(");
                    if (pos > 0) {
                        entryPoint = entryPoint.Substring(0, pos);
                    }
                }
                try {
                    mainCsvScriptCompatibilityClass mainCsv = new mainCsvScriptCompatibilityClass(core);
                    engine.AddHostObject("ccLib", mainCsv);
                } catch (Exception) {
                    throw;
                }
                try {
                    engine.AddHostObject("cp", core.cp_forAddonExecutionOnly);
                } catch (Exception) {
                    throw;
                }
                try {
                    engine.Execute(WorkingCode);
                    object returnObj = engine.Evaluate(entryPoint);
                    if (returnObj != null) {
                        if (returnObj.GetType() == typeof(String)) {
                            returnText = (String)returnObj;
                        }
                    }
                } catch (Exception ex) {
                    string addonDescription = getAddonDescription(core, addon);
                    string errorMessage = "Error executing addon script, " + addonDescription;
                    throw new GenericException(errorMessage, ex);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnText;
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute jscript script
        /// </summary>
        /// <param name="addon"></param>
        /// <param name="Code"></param>
        /// <param name="EntryPoint"></param>
        /// <param name="ScriptingTimeout"></param>
        /// <param name="ScriptName"></param>
        /// <returns></returns>
        private string execute_Script_JScript(ref AddonModel addon) {
            string returnText = "";
            try {
                // todo - move locals
                var engine = new Microsoft.ClearScript.Windows.JScriptEngine();
                string[] Args = { };
                string WorkingCode = addon.scriptingCode;
                //
                string entryPoint = addon.scriptingEntryPoint;
                if (string.IsNullOrEmpty(entryPoint)) {
                    //
                    // -- compatibility mode, if no entry point given, if the code starts with "function myFuncton()" and add "call myFunction()"
                    int pos = WorkingCode.IndexOf("function", StringComparison.CurrentCultureIgnoreCase);
                    if (pos >= 0) {
                        entryPoint = WorkingCode.Substring(pos + 9);
                        pos = entryPoint.IndexOf("\r");
                        if (pos > 0) {
                            entryPoint = entryPoint.Substring(0, pos);
                        }
                        pos = entryPoint.IndexOf("\n");
                        if (pos > 0) {
                            entryPoint = entryPoint.Substring(0, pos);
                        }
                        pos = entryPoint.IndexOf("(");
                        if (pos > 0) {
                            entryPoint = entryPoint.Substring(0, pos);
                        }
                    }
                } else {
                    //
                    // -- etnry point provided, remove "()" if included and add to code
                    int pos = entryPoint.IndexOf("(");
                    if (pos > 0) {
                        entryPoint = entryPoint.Substring(0, pos);
                    }
                }
                try {
                    mainCsvScriptCompatibilityClass mainCsv = new mainCsvScriptCompatibilityClass(core);
                    engine.AddHostObject("ccLib", mainCsv);
                } catch (Exception) {
                    throw;
                }
                try {
                    engine.AddHostObject("cp", core.cp_forAddonExecutionOnly);
                } catch (Exception) {
                    throw;
                }
                try {
                    engine.Execute(WorkingCode);
                    object returnObj = engine.Evaluate(entryPoint);
                    //object returnObj = engine.Evaluate(entryPoint);
                    if (returnObj != null) {
                        returnText = returnObj.ToString();
                        //if (returnObj.GetType() == typeof(String)) {
                        //    returnText = (String)returnObj;
                        //}
                    }
                } catch (Exception ex) {
                    string addonDescription = getAddonDescription(core, addon);
                    string errorMessage = "Error executing addon script, " + addonDescription;
                    throw new GenericException(errorMessage, ex);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnText;
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute dotnet code
        /// </summary>
        /// <param name="executeContext"></param>
        /// <param name="addon"></param>
        /// <param name="addonCollection"></param>
        /// <returns></returns>
        private string execute_assembly(CPUtilsBaseClass.addonExecuteContext executeContext, Models.Db.AddonModel addon, AddonCollectionModel addonCollection) {
            string result = "";
            try {
                LogController.logTrace(core, "execute_assembly dotNetClass [" + addon.dotNetClass + "], enter");
                // todo - move locals
                bool AddonFound = false;
                string warningMessage = "The addon [" + addon.name + "] dotnet code could not be executed because no assembly was found with namespace [" + addon.dotNetClass + "].";
                //
                // -- development bypass folder (addonAssemblyBypass)
                // -- purpose is to provide a path that can be hardcoded in visual studio after-build event to make development easier
                string commonAssemblyPath = core.programDataFiles.localAbsRootPath + "AddonAssemblyBypass\\";
                if (!Directory.Exists(commonAssemblyPath)) {
                    Directory.CreateDirectory(commonAssemblyPath);
                } else {
                    result = execute_assembly_byFilePath(addon, commonAssemblyPath, true, ref AddonFound);
                }
                if (!AddonFound) {
                    //
                    // -- application path (background from program files, forground from appRoot)
                    // -- purpose is to allow add-ons to be included in the website's (wwwRoot) assembly. So a website's custom addons are within the wwwRoot build, not separate
                    string appPath = "";
                    if (executeContext.backgroundProcess) {
                        //
                        // -- background - program files installation folder
                        appPath = core.programFiles.localAbsRootPath;
                    } else {
                        //
                        // -- foreground - appRootPath
                        appPath = core.privateFiles.joinPath(core.appRootFiles.localAbsRootPath, "bin\\");
                    }


                    string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                    UriBuilder uri = new UriBuilder(codeBase);
                    string path = Uri.UnescapeDataString(uri.Path);
                    appPath = Path.GetDirectoryName(path);


                    result = execute_assembly_byFilePath(addon, appPath, true, ref AddonFound);
                    if (!AddonFound) {
                        //
                        // -- try addon folder
                        // -- purpose is to have a repository where addons can be stored for now web and non-web apps, and allow permissions to be installed with online upload
                        if (addonCollection == null) {
                            throw new GenericException(warningMessage + " Not found in developer path [" + commonAssemblyPath + "] and application path [" + appPath + "]. The collection path was not checked because the addon has no collection set.");
                        } else if (string.IsNullOrEmpty(addonCollection.ccguid)) {
                            throw new GenericException(warningMessage + " Not found in developer path [" + commonAssemblyPath + "] and application path [" + appPath + "]. The collection path was not checked because the addon collection [" + addonCollection.name + "] has no guid.");
                        } else {
                            string AddonVersionPath = "";
                            var tmpDate = new DateTime();
                            string tmpName = "";
                            CollectionController.getCollectionConfig(core, addonCollection.ccguid, ref AddonVersionPath, ref tmpDate, ref tmpName);
                            if (string.IsNullOrEmpty(AddonVersionPath)) {
                                throw new GenericException(warningMessage + " Not found in developer path [" + commonAssemblyPath + "] and application path [" + appPath + "]. The collection path was not checked because the collection [" + addonCollection.name + "] was not found in the \\private\\addons\\Collections.xml file. Try re-installing the collection");
                            } else {
                                string AddonPath = core.privateFiles.joinPath(getPrivateFilesAddonPath(), AddonVersionPath);
                                string appAddonPath = core.privateFiles.joinPath(core.privateFiles.localAbsRootPath, AddonPath);
                                result = execute_assembly_byFilePath(addon, appAddonPath, false, ref AddonFound);
                                if (!AddonFound) {
                                    throw new GenericException(warningMessage + " Not found in developer path [" + commonAssemblyPath + "] and application path [" + appPath + "] or collection path [" + appAddonPath + "].");
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            } finally {
                LogController.logTrace(core, "execute_assembly dotNetClass [" + addon.dotNetClass + "], exit");
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute an assembly in a path
        /// </summary>
        /// <param name="addon"></param>
        /// <param name="fullPath"></param>
        /// <param name="IsDevAssembliesFolder"></param>
        /// <param name="AddonFound"></param>
        /// <returns></returns>
        private string execute_assembly_byFilePath(Models.Db.AddonModel addon, string fullPath, bool IsDevAssembliesFolder, ref bool AddonFound) {
            string returnValue = "";
            try {
                AddonFound = false;
                if (Directory.Exists(fullPath)) {
                    foreach (var TestFilePathname in Directory.GetFileSystemEntries(fullPath, "*.dll")) {
                        if (!core.assemblySkipList.Contains(TestFilePathname)) {
                            bool testFileIsValidAddonAssembly = true;
                            Assembly testAssembly = null;
                            try {
                                //
                                // ##### consider using refectiononlyload first, then if it is right, do the loadfrom - so Dependencies are not loaded.
                                //
                                testAssembly = System.Reflection.Assembly.LoadFrom(TestFilePathname);
                                //testAssemblyName = testAssembly.FullName
                            } catch (Exception ex) {
                                LogController.logInfo(core, "Assembly.LoadFrom failure, adding DLL [" + TestFilePathname + "] to assemblySkipList, ex [" + ex.Message + "]");
                                core.assemblySkipList.Add(TestFilePathname);
                                testFileIsValidAddonAssembly = false;
                            }
                            try {
                                if (testFileIsValidAddonAssembly) {
                                    //
                                    // problem loading types, use try to debug
                                    //
                                    try {
                                        bool isAddonAssembly = false;
                                        var typeMap = testAssembly.GetTypes().ToDictionary(t => t.FullName, t => t, StringComparer.OrdinalIgnoreCase);
                                        if (typeMap.TryGetValue(addon.dotNetClass, out Type addonType)) {
                                            if ((addonType.IsPublic) && (!((addonType.Attributes & TypeAttributes.Abstract) == TypeAttributes.Abstract)) && (addonType.BaseType != null)) {
                                                //
                                                // -- assembly is public, not abstract, based on a base type
                                                if (addonType.BaseType.FullName != null) {
                                                    //
                                                    // -- assembly has a baseType fullname
                                                    if ((addonType.BaseType.FullName.ToLowerInvariant() == "addonbaseclass") || (addonType.BaseType.FullName.ToLowerInvariant() == "contensive.baseclasses.addonbaseclass")) {
                                                        //
                                                        // -- valid addon assembly
                                                        isAddonAssembly = true;
                                                        AddonFound = true;
                                                    }
                                                }
                                            }
                                        }
                                        if (AddonFound) {
                                            try {
                                                //
                                                // -- Create the object from the Assembly
                                                AddonBaseClass AddonObj = (AddonBaseClass)testAssembly.CreateInstance(addonType.FullName);
                                                try {
                                                    //
                                                    // -- Call Execute
                                                    object AddonReturnObj = AddonObj.Execute(core.cp_forAddonExecutionOnly);
                                                    if (AddonReturnObj != null) {
                                                        switch (AddonReturnObj.GetType().ToString()) {
                                                            case "System.Object[,]":
                                                                //
                                                                //   a 2-D Array of objects
                                                                //   each cell can contain 
                                                                //   return array for internal use constructing data/layout merge
                                                                //   return xml as dataset to another computer
                                                                //   return json as dataset for browser
                                                                //
                                                                break;
                                                            case "System.String[,]":
                                                                //
                                                                //   return array for internal use constructing data/layout merge
                                                                //   return xml as dataset to another computer
                                                                //   return json as dataset for browser
                                                                //
                                                                break;
                                                            default:
                                                                returnValue = AddonReturnObj.ToString();
                                                                break;
                                                        }
                                                    }
                                                } catch (Exception Ex) {
                                                    //
                                                    // Error in the addon
                                                    //
                                                    string detailedErrorMessage = "There was an error in the addon [" + addon.name + "]. It could not be executed because there was an error in the addon assembly [" + TestFilePathname + "], in class [" + addonType.FullName.Trim().ToLowerInvariant() + "]. The error was [" + Ex.ToString() + "]";
                                                    LogController.handleError(core, Ex, detailedErrorMessage);
                                                    //Throw new GenericException(detailedErrorMessage)
                                                }
                                            } catch (Exception Ex) {
                                                string detailedErrorMessage = addon.name + " could not be executed because there was an error creating an object from the assembly, DLL [" + addonType.FullName + "]. The error was [" + Ex.ToString() + "]";
                                                LogController.handleError(core, Ex, detailedErrorMessage);
                                                throw new GenericException(detailedErrorMessage);
                                            }
                                            //
                                            // -- addon was found, no need to look for more
                                            break;
                                        }
                                        if (!isAddonAssembly) {
                                            //
                                            // -- not an addon assembly
                                            //core.assemblySkipList.Add(TestFilePathname);
                                        }
                                    } catch (ReflectionTypeLoadException ex) {
                                        //
                                        // exceptin thrown out of application bin folder when xunit library included -- ignore
                                        //
                                        LogController.logDebug(core, "Assembly ReflectionTypeLoadException, [" + TestFilePathname + "], adding to assemblySkipList, ex [" + ex.Message + "]");
                                        core.assemblySkipList.Add(TestFilePathname);
                                    } catch (Exception ex) {
                                        //
                                        // problem loading types
                                        //
                                        LogController.logDebug(core, "Assembly exception, [" + TestFilePathname + "], adding to assemblySkipList, ex [" + ex.Message + "]");
                                        core.assemblySkipList.Add(TestFilePathname);
                                        string detailedErrorMessage = "While locating assembly for addon [" + addon.name + "], there was an error loading types for assembly [" + TestFilePathname + "]. This assembly was skipped and should be removed from the folder [" + fullPath + "]";
                                        throw new GenericException(detailedErrorMessage);
                                    }
                                }
                            } catch (System.Reflection.ReflectionTypeLoadException ex) {
                                LogController.logDebug(core, "Assembly ReflectionTypeLoadException-2, [" + TestFilePathname + "], adding to assemblySkipList, ex [" + ex.ToString() + "]");
                                core.assemblySkipList.Add(TestFilePathname);
                                string detailedErrorMessage = "A load exception occured for addon [" + addon.name + "], DLL [" + TestFilePathname + "]. The error was [" + ex.ToString() + "] Any internal exception follow:";
                                foreach (Exception exLoader in ex.LoaderExceptions) {
                                    detailedErrorMessage += "\r\n--LoaderExceptions: " + exLoader.Message;
                                }
                                throw new GenericException(detailedErrorMessage);
                            } catch (Exception ex) {
                                //
                                // ignore these errors
                                //
                                LogController.logDebug(core, "Assembly Exception-2, [" + TestFilePathname + "], adding to assemblySkipList, ex [" + ex.Message + "]");
                                core.assemblySkipList.Add(TestFilePathname);
                                string detailedErrorMessage = "A non-load exception occured while loading the addon [" + addon.name + "], DLL [" + TestFilePathname + "]. The error was [" + ex.ToString() + "].";
                                LogController.handleError(core, new GenericException(detailedErrorMessage));
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                //
                // -- this exception should interrupt the caller
                LogController.handleError(core, ex);
                throw;
            }
            return returnValue;
        }
        //
        //====================================================================================================================
        //
        public void executeAsync(AddonModel addon, Dictionary<string, string> arguments) {
            try {
                if (addon == null) {
                    //
                    // -- addon not found
                    LogController.logError(core, "executeAsync, addon not valid");
                } else {
                    //
                    // -- build arguments from the execute context on top of docProperties
                    var compositeArgs = new Dictionary<string, string>(arguments);
                    foreach (var key in core.docProperties.getKeyList()) {
                        if (!compositeArgs.ContainsKey(key)) { compositeArgs.Add(key, core.docProperties.getText(key)); }
                    }
                    var cmdDetail = new TaskModel.CmdDetailClass {
                        addonId = addon.id,
                        addonName = addon.name,
                        args = compositeArgs
                    };
                    TaskSchedulerControllerx.addTaskToQueue(core, cmdDetail, false);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex, "executeAsync");
            }
        }
        //
        //====================================================================================================================
        /// <summary>
        /// execute an addon with the default context
        /// </summary>
        /// <param name="addon"></param>
        public void executeAsync(AddonModel addon) => executeAsync(addon, new Dictionary<string, string>());
        //
        //====================================================================================================================
        //
        public void executeAsync(string addonGuid, string OptionString = "") {
            executeAsync(core.addonCache.getAddonByGuid(addonGuid), convertQSNVAArgumentstoDocPropertiesList(core, OptionString));
        }
        //
        //====================================================================================================================
        //
        public void executeAsyncByName(string addonName, string OptionString = "") {
            executeAsync(core.addonCache.getAddonByName(addonName), convertQSNVAArgumentstoDocPropertiesList(core, OptionString));
        }
        //
        //===============================================================================================================================================
        /// <summary>
        /// popup menu used on pages
        /// </summary>
        /// <param name="AddonName"></param>
        /// <param name="Option_String"></param>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <param name="FieldName"></param>
        /// <param name="ACInstanceID"></param>
        /// <param name="Context"></param>
        /// <param name="return_DialogList"></param>
        /// <returns></returns>
        public string getInstanceBubble(string AddonName, string Option_String, string ContentName, int RecordID, string FieldName, string ACInstanceID, CPUtilsBaseClass.addonContext Context, ref string return_DialogList) {
            string tempgetInstanceBubble = null;
            try {
                //
                string OptionDefault = null;
                string OptionSuffix = null;
                int OptionCnt = 0;
                string OptionValue_AddonEncoded = null;
                string OptionValue = null;
                string OptionCaption = null;
                string LCaseOptionDefault = null;
                string[] OptionValues = null;
                string FormInput = null;
                int OptionPtr = 0;
                string QueryString = null;
                string LocalCode = "";
                string CopyHeader = "";
                string CopyContent = "";
                string BubbleJS = null;
                string[] OptionSplit = null;
                string OptionName = null;
                string OptionSelector = null;
                int Ptr = 0;
                int Pos = 0;
                //
                if (core.session.isAuthenticated & ((ACInstanceID == "-2") || (ACInstanceID == "-1") || (ACInstanceID == "0") || (RecordID != 0))) {
                    if (core.session.isEditingAnything()) {
                        CopyHeader = CopyHeader + "<div class=\"ccHeaderCon\">"
                            + "<table border=0 cellpadding=0 cellspacing=0 width=\"100%\">"
                            + "<tr>"
                            + "<td align=left class=\"bbLeft\">Options for this instance of " + AddonName + "</td>"
                            + "<td align=right class=\"bbRight\"><a href=\"#\" onClick=\"HelpBubbleOff('HelpBubble" + core.doc.helpCodes.Count + "');return false;\">" + iconClose_White + "</i></a></td>"
                            + "</tr>"
                            + "</table>"
                            + "</div>";
                        if (string.IsNullOrEmpty(Option_String)) {
                            //
                            // no option string - no settings to display
                            //
                            CopyContent = "This Add-on has no instance options.";
                            CopyContent = "<div style=\"width:400px;background-color:transparent\" class=\"ccAdminSmall\">" + CopyContent + "</div>";
                        } else if ((ACInstanceID == "0") || (ACInstanceID == "-1")) {
                            //
                            // This addon does not support bubble option setting
                            //
                            CopyContent = "This addon does not support instance options.";
                            CopyContent = "<div style=\"width:400px;background-color:transparent;\" class=\"ccAdminSmall\">" + CopyContent + "</div>";
                            //ElseIf (Context <> CPUtilsBaseClass.addonContext.ContextAdmin) And (core.siteProperties.allowWorkflowAuthoring And Not core.visitProperty.getBoolean("AllowWorkflowRendering")) Then
                            //    '
                            //    ' workflow with no rendering (or within admin site)
                            //    '
                            //    CopyContent = "With Workflow editing enabled, you can not edit Add-on settings for live records. To make changes to the editable version of this page, turn on Render Workflow Authoring Changes and Advanced Edit together."
                            //    CopyContent = "<div style=""width:400px;background-color:transparent;"" class=""ccAdminSmall"">" & CopyContent & "</div>"
                        } else if (string.IsNullOrEmpty(ACInstanceID)) {
                            //
                            // No instance ID - must be edited and saved
                            //
                            CopyContent = "You can not edit instance options for Add-ons on this page until the page is upgraded. To upgrade, edit and save the page.";
                            CopyContent = "<div style=\"width:400px;background-color:transparent;\" class=\"ccAdminSmall\">" + CopyContent + "</div>";
                        } else {
                            //
                            // ACInstanceID is -2 (Admin Root), or Rnd (from an instance on a page) Editable Form
                            //
                            CopyContent = CopyContent + "<table border=0 cellpadding=5 cellspacing=0 width=\"100%\">"
                                + "";
                            OptionSplit = GenericController.stringSplit(Option_String, "\r\n");
                            for (Ptr = 0; Ptr <= OptionSplit.GetUpperBound(0); Ptr++) {
                                //
                                // Process each option row
                                //
                                OptionName = OptionSplit[Ptr];
                                OptionSuffix = "";
                                OptionDefault = "";
                                LCaseOptionDefault = "";
                                OptionSelector = "";
                                Pos = GenericController.vbInstr(1, OptionName, "=");
                                if (Pos != 0) {
                                    if (Pos < OptionName.Length) {
                                        OptionSelector = (OptionName.Substring(Pos)).Trim(' ');
                                    }
                                    OptionName = (OptionName.Left(Pos - 1)).Trim(' ');
                                }
                                OptionName = GenericController.decodeNvaArgument(OptionName);
                                Pos = GenericController.vbInstr(1, OptionSelector, "[");
                                if (Pos != 0) {
                                    //
                                    // List of Options, might be select, radio, checkbox, resourcelink
                                    //
                                    OptionDefault = OptionSelector.Left(Pos - 1);
                                    OptionDefault = GenericController.decodeNvaArgument(OptionDefault);
                                    LCaseOptionDefault = GenericController.vbLCase(OptionDefault);
                                    //LCaseOptionDefault = genericController.decodeNvaArgument(LCaseOptionDefault)

                                    OptionSelector = OptionSelector.Substring(Pos);
                                    Pos = GenericController.vbInstr(1, OptionSelector, "]");
                                    if (Pos > 0) {
                                        if (Pos < OptionSelector.Length) {
                                            OptionSuffix = GenericController.vbLCase((OptionSelector.Substring(Pos)).Trim(' '));
                                        }
                                        OptionSelector = OptionSelector.Left(Pos - 1);
                                    }
                                    OptionValues = OptionSelector.Split('|');
                                    FormInput = "";
                                    OptionCnt = OptionValues.GetUpperBound(0) + 1;
                                    for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++) {
                                        OptionValue_AddonEncoded = OptionValues[OptionPtr].Trim(' ');
                                        if (!string.IsNullOrEmpty(OptionValue_AddonEncoded)) {
                                            Pos = GenericController.vbInstr(1, OptionValue_AddonEncoded, ":");
                                            if (Pos == 0) {
                                                OptionValue = GenericController.decodeNvaArgument(OptionValue_AddonEncoded);
                                                OptionCaption = OptionValue;
                                            } else {
                                                OptionCaption = GenericController.decodeNvaArgument(OptionValue_AddonEncoded.Left(Pos - 1));
                                                OptionValue = GenericController.decodeNvaArgument(OptionValue_AddonEncoded.Substring(Pos));
                                            }
                                            switch (OptionSuffix) {
                                                case "checkbox":
                                                    //
                                                    // Create checkbox FormInput
                                                    //
                                                    if (GenericController.vbInstr(1, "," + LCaseOptionDefault + ",", "," + GenericController.vbLCase(OptionValue) + ",") != 0) {
                                                        FormInput = FormInput + "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + OptionName + OptionPtr + "\" value=\"" + OptionValue + "\" checked=\"checked\">" + OptionCaption + "</div>";
                                                    } else {
                                                        FormInput = FormInput + "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + OptionName + OptionPtr + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
                                                    }
                                                    break;
                                                case "radio":
                                                    //
                                                    // Create Radio FormInput
                                                    //
                                                    if (GenericController.vbLCase(OptionValue) == LCaseOptionDefault) {
                                                        FormInput = FormInput + "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + OptionName + "\" value=\"" + OptionValue + "\" checked=\"checked\" >" + OptionCaption + "</div>";
                                                    } else {
                                                        FormInput = FormInput + "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + OptionName + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
                                                    }
                                                    break;
                                                default:
                                                    //
                                                    // Create select FormInput
                                                    //
                                                    if (GenericController.vbLCase(OptionValue) == LCaseOptionDefault) {
                                                        FormInput = FormInput + "<option value=\"" + OptionValue + "\" selected>" + OptionCaption + "</option>";
                                                    } else {
                                                        OptionCaption = GenericController.vbReplace(OptionCaption, "\r\n", " ");
                                                        FormInput = FormInput + "<option value=\"" + OptionValue + "\">" + OptionCaption + "</option>";
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                    switch (OptionSuffix) {
                                        //                            Case FieldTypeLink
                                        //                                '
                                        //                                ' ----- Link (href value
                                        //                                '
                                        //                                Return_NewFieldList = Return_NewFieldList & "," & FieldName
                                        //                                FieldValueText = genericController.encodeText(FieldValueVariant)
                                        //                                EditorString = "" _
                                        //                                    & core.main_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80, FormFieldLCaseName) _
                                        //                                    & "&nbsp;<a href=""#"" onClick=""OpenResourceLinkWindow( '" & FormFieldLCaseName & "' ) ;return false;""><img src=""/ContensiveBase/images/ResourceLink1616.gif"" width=16 height=16 border=0 alt=""Link to a resource"" title=""Link to a resource""></a>" _
                                        //                                    & "&nbsp;<a href=""#"" onClick=""OpenSiteExplorerWindow( '" & FormFieldLCaseName & "' ) ;return false;""><img src=""/ContensiveBase/images/PageLink1616.gif"" width=16 height=16 border=0 alt=""Link to a page"" title=""Link to a page""></a>"
                                        //                                s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & "</span></nobr></td>")
                                        //                            Case FieldTypeResourceLink
                                        //                                '
                                        //                                ' ----- Resource Link (src value)
                                        //                                '
                                        //                                Return_NewFieldList = Return_NewFieldList & "," & FieldName
                                        //                                FieldValueText = genericController.encodeText(FieldValueVariant)
                                        //                                EditorString = "" _
                                        //                                    & core.main_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80, FormFieldLCaseName) _
                                        //                                    & "&nbsp;<a href=""#"" onClick=""OpenResourceLinkWindow( '" & FormFieldLCaseName & "' ) ;return false;""><img src=""/ContensiveBase/images/ResourceLink1616.gif"" width=16 height=16 border=0 alt=""Link to a resource"" title=""Link to a resource""></a>"
                                        //                                'EditorString = core.main_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80)
                                        //                                s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & "</span></nobr></td>")
                                        case "resourcelink":
                                            //
                                            // Create text box linked to resource library
                                            //
                                            OptionDefault = GenericController.decodeNvaArgument(OptionDefault);
                                            FormInput = ""
                                                + HtmlController.inputText(core, OptionName, OptionDefault, 1, 20) + "&nbsp;<a href=\"#\" onClick=\"OpenResourceLinkWindow( '" + OptionName + "' ) ;return false;\"><img src=\"/ContensiveBase/images/ResourceLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a resource\" title=\"Link to a resource\"></a>";
                                            //EditorString = core.main_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80)
                                            break;
                                        case "checkbox":
                                            //
                                            //
                                            CopyContent = CopyContent + "<input type=\"hidden\" name=\"" + OptionName + "CheckBoxCnt\" value=\"" + OptionCnt + "\" >";
                                            break;
                                        case "radio":
                                            //
                                            // Create Radio FormInput
                                            //
                                            break;
                                        default:
                                            //
                                            // Create select FormInput
                                            //
                                            FormInput = "<select name=\"" + OptionName + "\">" + FormInput + "</select>";
                                            break;
                                    }
                                } else {
                                    //
                                    // Create Text FormInput
                                    //

                                    OptionSelector = GenericController.decodeNvaArgument(OptionSelector);
                                    FormInput = HtmlController.inputText(core, OptionName, OptionSelector, 1, 20);
                                }
                                CopyContent = CopyContent + "<tr>"
                                    + "<td class=\"bbLeft\">" + OptionName + "</td>"
                                    + "<td class=\"bbRight\">" + FormInput + "</td>"
                                    + "</tr>";
                            }
                            CopyContent = ""
                                + CopyContent + "</table>"
                                + HtmlController.inputHidden("Type", FormTypeAddonSettingsEditor) + HtmlController.inputHidden("ContentName", ContentName) + HtmlController.inputHidden("RecordID", RecordID) + HtmlController.inputHidden("FieldName", FieldName) + HtmlController.inputHidden("ACInstanceID", ACInstanceID);
                        }
                        //
                        BubbleJS = " onClick=\"HelpBubbleOn( 'HelpBubble" + core.doc.helpCodes.Count + "',this);return false;\"";
                        QueryString = core.doc.refreshQueryString;
                        QueryString = GenericController.modifyQueryString(QueryString, RequestNameHardCodedPage, "", false);
                        //QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
                        return_DialogList = return_DialogList 
                            + "<div class=\"ccCon helpDialogCon\">"
                            + HtmlController.formMultipart_start(core, core.doc.refreshQueryString,"", "ccForm") 
                            + "<table border=0 cellpadding=0 cellspacing=0 class=\"ccBubbleCon\" id=\"HelpBubble" + core.doc.helpCodes.Count + "\" style=\"display:none;visibility:hidden;\">"
                            + "<tr><td class=\"ccHeaderCon\">" + CopyHeader + "</td></tr>"
                            + "<tr><td class=\"ccButtonCon\">" + HtmlController.getHtmlInputSubmit("Update", "HelpBubbleButton") + "</td></tr>"
                            + "<tr><td class=\"ccContentCon\">" + CopyContent + "</td></tr>"
                            + "</table>"
                            + "</form>"
                            + "</div>";
                        tempgetInstanceBubble = ""
                            + "&nbsp;<a href=\"#\" tabindex=-1 target=\"_blank\"" + BubbleJS + ">"
                            + getIconSprite("", 0, "/ContensiveBase/images/toolsettings.png", 22, 22, "Edit options used just for this instance of the " + AddonName + " Add-on", "Edit options used just for this instance of the " + AddonName + " Add-on", "", true, "") + "</a>"
                            + ""
                            + "";
                        core.doc.helpCodes.Add(new DocController.HelpStuff() {
                            caption = AddonName,
                            code = LocalCode
                        });
                        if (core.doc.helpDialogCnt == 0) {
                            core.html.addScriptCode_onLoad("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs");
                        }
                        core.doc.helpDialogCnt = core.doc.helpDialogCnt + 1;
                    }
                }
                //
                return tempgetInstanceBubble;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            //ErrorTrap:
            //throw new GenericException("Unexpected exception"); // Call core.handleLegacyError18("addon_execute_GetInstanceBubble")
            return tempgetInstanceBubble;
        }
        //
        //===============================================================================================================================================
        /// <summary>
        /// Get styles for help popup
        /// </summary>
        /// <param name="addonId"></param>
        /// <param name="return_DialogList"></param>
        /// <returns></returns>
        public string getAddonStylesBubble(int addonId, ref string return_DialogList) {
            string result = "";
            try {
                //Dim DefaultStylesheet As String = ""
                //Dim StyleSheet As String = ""
                string QueryString = null;
                string LocalCode = "";
                string CopyHeader = "";
                string CopyContent = null;
                string BubbleJS = null;
                //Dim AddonName As String = ""
                //
                if (core.session.isAuthenticated && true) {
                    if (core.session.isEditingAnything()) {
                        AddonModel addon = AddonModel.create(core, addonId);
                        CopyHeader = CopyHeader + "<div class=\"ccHeaderCon\">"
                            + "<table border=0 cellpadding=0 cellspacing=0 width=\"100%\">"
                            + "<tr>"
                            + "<td align=left class=\"bbLeft\">Stylesheet for " + addon.name + "</td>"
                            + "<td align=right class=\"bbRight\"><a href=\"#\" onClick=\"HelpBubbleOff('HelpBubble" + core.doc.helpCodes.Count + "');return false;\">" + iconClose_White + "</i></a></td>"
                            + "</tr>"
                            + "</table>"
                            + "</div>";
                        CopyContent = ""
                            + ""
                            + "<table border=0 cellpadding=5 cellspacing=0 width=\"100%\">"
                            + "<tr><td style=\"width:400px;background-color:transparent;\" class=\"ccContentCon ccAdminSmall\">These stylesheets will be added to all pages that include this add-on. The default stylesheet comes with the add-on, and can not be edited.</td></tr>"
                            + "<tr><td style=\"padding-bottom:5px;\" class=\"ccContentCon ccAdminSmall\"><b>Custom Stylesheet</b>" + HtmlController.inputTextarea(core, "CustomStyles", addon.stylesFilename.content, 10) + "</td></tr>";
                        //If DefaultStylesheet = "" Then
                        //    CopyContent = CopyContent & "<tr><td style=""padding-bottom:5px;"" class=""ccContentCon ccAdminSmall""><b>Default Stylesheet</b><br>There are no default styles for this add-on.</td></tr>"
                        //Else
                        //    CopyContent = CopyContent & "<tr><td style=""padding-bottom:5px;"" class=""ccContentCon ccAdminSmall""><b>Default Stylesheet</b><br>" & core.html.html_GetFormInputTextExpandable2("DefaultStyles", DefaultStylesheet, 10, "400px", , , True) & "</td></tr>"
                        //End If
                        CopyContent = ""
                        + CopyContent + "</tr>"
                        + "</table>"
                        + HtmlController.inputHidden("Type", FormTypeAddonStyleEditor) + HtmlController.inputHidden("AddonID", addonId) + "";
                        //
                        BubbleJS = " onClick=\"HelpBubbleOn( 'HelpBubble" + core.doc.helpCodes.Count + "',this);return false;\"";
                        QueryString = core.doc.refreshQueryString;
                        QueryString = GenericController.modifyQueryString(QueryString, RequestNameHardCodedPage, "", false);
                        //QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
                        string Dialog = "";

                        Dialog = Dialog 
                            + "<div class=\"ccCon helpDialogCon\">"
                            + HtmlController.formMultipart_start(core, core.doc.refreshQueryString,"", "ccForm") 
                            + "<table border=0 cellpadding=0 cellspacing=0 class=\"ccBubbleCon\" id=\"HelpBubble" + core.doc.helpCodes.Count + "\" style=\"display:none;visibility:hidden;\">"
                            + "<tr><td class=\"ccHeaderCon\">" + CopyHeader + "</td></tr>"
                            + "<tr><td class=\"ccButtonCon\">" + HtmlController.getHtmlInputSubmit("Update", "HelpBubbleButton") + "</td></tr>"
                            + "<tr><td class=\"ccContentCon\">" + CopyContent + "</td></tr>"
                            + "</table>"
                            + "</form>"
                            + "</div>";
                        return_DialogList = return_DialogList + Dialog;
                        result = ""
                            + "&nbsp;<a href=\"#\" tabindex=-1 target=\"_blank\"" + BubbleJS + ">"
                            + getIconSprite("", 0, "/ContensiveBase/images/toolstyles.png", 22, 22, "Edit " + addon.name + " Stylesheets", "Edit " + addon.name + " Stylesheets", "", true, "") + "</a>";
                        core.doc.helpCodes.Add(new DocController.HelpStuff {
                            caption = addon.name,
                            code = LocalCode
                        });
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //===============================================================================================================================================
        /// <summary>
        /// help popup
        /// </summary>
        /// <param name="addonId"></param>
        /// <param name="helpCopy"></param>
        /// <param name="CollectionID"></param>
        /// <param name="return_DialogList"></param>
        /// <returns></returns>
        public string getHelpBubble(int addonId, string helpCopy, int CollectionID, ref string return_DialogList) {
            string result = "";
            string QueryString = null;
            string LocalCode = "";
            string CopyContent = null;
            string BubbleJS = null;
            string AddonName = "";
            int StyleSN = 0;
            string InnerCopy = null;
            string CollectionCopy = "";
            //
            if (core.session.isAuthenticated) {
                if (core.session.isEditingAnything()) {
                    StyleSN = GenericController.encodeInteger(core.siteProperties.getText("StylesheetSerialNumber", "0"));
                    //core.html.html_HelpViewerButtonID = "HelpBubble" & doccontroller.htmlDoc_HelpCodeCount
                    InnerCopy = helpCopy;
                    if (string.IsNullOrEmpty(InnerCopy)) {
                        InnerCopy = "<p style=\"text-align:center\">No help is available for this add-on.</p>";
                    }
                    //
                    if (CollectionID != 0) {
                        CollectionCopy = core.db.getRecordName("Add-on Collections", CollectionID);
                        if (!string.IsNullOrEmpty(CollectionCopy)) {
                            CollectionCopy = "This add-on is a member of the " + CollectionCopy + " collection.";
                        } else {
                            CollectionID = 0;
                        }
                    }
                    if (CollectionID == 0) {
                        CollectionCopy = "This add-on is not a member of any collection.";
                    }
                    string CopyHeader = "";
                    CopyHeader = CopyHeader + "<div class=\"ccHeaderCon\">"
                        + "<table border=0 cellpadding=0 cellspacing=0 width=\"100%\">"
                        + "<tr>"
                        + "<td align=left class=\"bbLeft\">Help Viewer</td>"
                        + "<td align=right class=\"bbRight\"><a href=\"#\" onClick=\"HelpBubbleOff('HelpBubble" + core.doc.helpCodes.Count + "');return false;\">" + iconClose_White + "</i></a></td>"
                        + "</tr>"
                        + "</table>"
                        + "</div>";
                    CopyContent = ""
                        + "<table border=0 cellpadding=5 cellspacing=0 width=\"100%\">"
                        + "<tr><td style=\"width:400px;background-color:transparent;\" class=\"ccAdminSmall\"><p>" + CollectionCopy + "</p></td></tr>"
                        + "<tr><td style=\"width:400px;background-color:transparent;border:1px solid #fff;padding:10px;margin:5px;\">" + InnerCopy + "</td></tr>"
                        + "</tr>"
                        + "</table>"
                        + "";
                    //
                    QueryString = core.doc.refreshQueryString;
                    QueryString = GenericController.modifyQueryString(QueryString, RequestNameHardCodedPage, "", false);
                    //QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
                    return_DialogList = return_DialogList + "<div class=\"ccCon helpDialogCon\">"
                        + "<table border=0 cellpadding=0 cellspacing=0 class=\"ccBubbleCon\" id=\"HelpBubble" + core.doc.helpCodes.Count + "\" style=\"display:none;visibility:hidden;\">"
                        + "<tr><td class=\"ccHeaderCon\">" + CopyHeader + "</td></tr>"
                        + "<tr><td class=\"ccContentCon\">" + CopyContent + "</td></tr>"
                        + "</table>"
                        + "</div>";
                    BubbleJS = " onClick=\"HelpBubbleOn( 'HelpBubble" + core.doc.helpCodes.Count + "',this);return false;\"";
                    core.doc.helpCodes.Add(new DocController.HelpStuff {
                        code = LocalCode,
                        caption = AddonName
                    });
                    //
                    if (core.doc.helpDialogCnt == 0) {
                        core.html.addScriptCode_onLoad("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs");
                    }
                    core.doc.helpDialogCnt = core.doc.helpDialogCnt + 1;
                    result = ""
                        + "&nbsp;<a href=\"#\" tabindex=-1 tarGet=\"_blank\"" + BubbleJS + " >"
                        + getIconSprite("", 0, "/ContensiveBase/images/toolhelp.png", 22, 22, "View help resources for this Add-on", "View help resources for this Add-on", "", true, "") + "</a>";
                }
            }
            return result;
        }
        //
        //===============================================================================================================================================
        /// <summary>
        /// help buble
        /// </summary>
        /// <param name="addonId"></param>
        /// <param name="HTMLSourceID"></param>
        /// <param name="return_DialogList"></param>
        /// <returns></returns>
        public string getHTMLViewerBubble(int addonId, string HTMLSourceID, ref string return_DialogList) {
            string tempgetHTMLViewerBubble = null;
            try {
                //
                string QueryString = null;
                string LocalCode = "";
                string CopyHeader = "";
                string CopyContent = null;
                string BubbleJS = null;
                string AddonName = "";
                int StyleSN = 0;
                string HTMLViewerBubbleID = null;
                //
                if (core.session.isAuthenticated) {
                    if (core.session.isEditingAnything()) {
                        StyleSN = GenericController.encodeInteger(core.siteProperties.getText("StylesheetSerialNumber", "0"));
                        HTMLViewerBubbleID = "HelpBubble" + core.doc.helpCodes.Count;
                        //
                        CopyHeader = CopyHeader + "<div class=\"ccHeaderCon\">"
                            + "<table border=0 cellpadding=0 cellspacing=0 width=\"100%\">"
                            + "<tr>"
                            + "<td align=left class=\"bbLeft\">HTML viewer</td>"
                            + "<td align=right class=\"bbRight\"><a href=\"#\" onClick=\"HelpBubbleOff('" + HTMLViewerBubbleID + "');return false;\">" + iconClose_White + "</i></A></td>"
                            + "</tr>"
                            + "</table>"
                            + "</div>";
                        CopyContent = ""
                            + "<table border=0 cellpadding=5 cellspacing=0 width=\"100%\">"
                            + "<tr><td style=\"width:400px;background-color:transparent;\" class=\"ccAdminSmall\">This is the HTML produced by this add-on. Carrage returns and tabs have been added or modified to enhance readability.</td></tr>"
                            + "<tr><td style=\"width:400px;background-color:transparent;\" class=\"ccAdminSmall\">" + HtmlController.inputTextarea(core, "DefaultStyles", "", 10, -1, HTMLViewerBubbleID + "_dst", false, false) + "</td></tr>"
                            + "</tr>"
                            + "</table>"
                            + "";
                        //
                        QueryString = core.doc.refreshQueryString;
                        QueryString = GenericController.modifyQueryString(QueryString, RequestNameHardCodedPage, "", false);
                        //QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
                        return_DialogList = return_DialogList + "<div class=\"ccCon helpDialogCon\">"
                            + "<table border=0 cellpadding=0 cellspacing=0 class=\"ccBubbleCon\" id=\"" + HTMLViewerBubbleID + "\" style=\"display:none;visibility:hidden;\">"
                            + "<tr><td class=\"ccHeaderCon\">" + CopyHeader + "</td></tr>"
                            + "<tr><td class=\"ccContentCon\">" + CopyContent + "</td></tr>"
                            + "</table>"
                            + "</div>";
                        BubbleJS = " onClick=\"var d=document.getElementById('" + HTMLViewerBubbleID + "_dst');if(d){var s=document.getElementById('" + HTMLSourceID + "');if(s){d.value=s.innerHTML;HelpBubbleOn( '" + HTMLViewerBubbleID + "',this)}};return false;\" ";
                        core.doc.helpCodes.Add(new DocController.HelpStuff {
                            code = LocalCode,
                            caption = AddonName
                        });
                        //
                        if (core.doc.helpDialogCnt == 0) {
                            core.html.addScriptCode_onLoad("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs");
                        }
                        core.doc.helpDialogCnt = core.doc.helpDialogCnt + 1;
                        tempgetHTMLViewerBubble = ""
                            + "&nbsp;<a href=\"#\" tabindex=-1 target=\"_blank\"" + BubbleJS + " >"
                            + getIconSprite("", 0, "/ContensiveBase/images/toolhtml.png", 22, 22, "View the source HTML produced by this Add-on", "View the source HTML produced by this Add-on", "", true, "") + "</A>";
                    }
                }
                //
                return tempgetHTMLViewerBubble;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            //ErrorTrap:
            //throw new GenericException("Unexpected exception"); // Call core.handleLegacyError18("addon_execute_GetHTMLViewerBubble")
            return tempgetHTMLViewerBubble;
        }
        //
        //===================================================================================================
        //   Build AddonOptionLists
        //
        //   On entry:
        //       AddonOptionConstructor = the addon-encoded version of the list that comes from the Addon Record
        //           It is crlf delimited and all escape characters converted
        //       AddonOptionString = addonencoded version of the list that comes from the HTML AC tag
        //           that means & delimited
        //
        //   On Exit:
        //       OptionString_ForObjectCall
        //               pass this string to the addon when it is run, crlf delimited name=value pair.
        //               This should include just the name=values pairs, with no selectors
        //               it should include names from both Addon and Instance
        //               If the Instance has a value, include it. Otherwise include Addon value
        //       AddonOptionExpandedConstructor = pass this to the bubble editor to create the the selectr
        //===================================================================================================
        //
        public void buildAddonOptionLists2(ref Dictionary<string, string> addonInstanceProperties, ref string addonArgumentListPassToBubbleEditor, string addonArgumentListFromRecord, Dictionary<string, string> instanceOptions, string InstanceID, bool IncludeSettingsBubbleOptions) {
            try {
                //
                int SavePtr = 0;
                string[] ConstructorTypes = null;
                string ConstructorValue = null;
                string ConstructorSelector = null;
                string ConstructorName = null;
                int ConstructorPtr = 0;
                int Pos = 0;
                string InstanceName = null;
                string InstanceValue = null;
                //
                string[] ConstructorNameValues = { };
                string[] ConstructorNames = { };
                string[] ConstructorSelectors = { };
                string[] ConstructorValues = { };
                //
                int ConstructorCnt = 0;


                if (!string.IsNullOrEmpty(addonArgumentListFromRecord)) {
                    //
                    // Initially Build Constructor from AddonOptions
                    //
                    ConstructorNameValues = GenericController.stringSplit(addonArgumentListFromRecord, "\r\n");
                    ConstructorCnt = ConstructorNameValues.GetUpperBound(0) + 1;
                    ConstructorNames = new string[ConstructorCnt + 1];
                    ConstructorSelectors = new string[ConstructorCnt + 1];
                    ConstructorValues = new string[ConstructorCnt + 1];
                    ConstructorTypes = new string[ConstructorCnt + 1];
                    SavePtr = 0;
                    for (ConstructorPtr = 0; ConstructorPtr < ConstructorCnt; ConstructorPtr++) {
                        ConstructorName = ConstructorNameValues[ConstructorPtr];
                        ConstructorSelector = "";
                        ConstructorValue = "";
                        Pos = GenericController.vbInstr(1, ConstructorName, "=");
                        if (Pos > 1) {
                            ConstructorValue = ConstructorName.Substring(Pos);
                            ConstructorName = (ConstructorName.Left(Pos - 1)).Trim(' ');
                            Pos = GenericController.vbInstr(1, ConstructorValue, "[");
                            if (Pos > 0) {
                                ConstructorSelector = ConstructorValue.Substring(Pos - 1);
                                ConstructorValue = ConstructorValue.Left(Pos - 1);
                            }
                        }
                        if (!string.IsNullOrEmpty(ConstructorName)) {
                            //Pos = genericController.vbInstr(1, ConstructorName, ",")
                            //If Pos > 1 Then
                            //    ConstructorType = Mid(ConstructorName, Pos + 1)
                            //    ConstructorName = Left(ConstructorName, Pos - 1)
                            //End If

                            ConstructorNames[SavePtr] = ConstructorName;
                            ConstructorValues[SavePtr] = ConstructorValue;
                            ConstructorSelectors[SavePtr] = ConstructorSelector;
                            //ConstructorTypes(ConstructorPtr) = ConstructorType
                            SavePtr = SavePtr + 1;
                        }
                    }
                    ConstructorCnt = SavePtr;
                }
                //
                foreach (var kvp in instanceOptions) {
                    InstanceName = kvp.Key;
                    InstanceValue = kvp.Value;
                    if (!string.IsNullOrEmpty(InstanceName)) {
                        //
                        // if the name is not in the Constructor, add it
                        if (ConstructorCnt > 0) {
                            for (ConstructorPtr = 0; ConstructorPtr < ConstructorCnt; ConstructorPtr++) {
                                if (GenericController.vbLCase(InstanceName) == GenericController.vbLCase(ConstructorNames[ConstructorPtr])) {
                                    break;
                                }
                            }
                        }
                        if (ConstructorPtr >= ConstructorCnt) {
                            //
                            // not found, add this instance name and value to the Constructor values
                            //
                            Array.Resize(ref ConstructorNames, ConstructorCnt + 1);
                            Array.Resize(ref ConstructorValues, ConstructorCnt + 1);
                            Array.Resize(ref ConstructorSelectors, ConstructorCnt + 1);
                            ConstructorNames[ConstructorCnt] = InstanceName;
                            ConstructorValues[ConstructorCnt] = InstanceValue;
                            ConstructorCnt = ConstructorCnt + 1;
                        } else {
                            //
                            // found, set the ConstructorValue to the instance value
                            //
                            ConstructorValues[ConstructorPtr] = InstanceValue;
                        }
                        SavePtr = SavePtr + 1;
                    }
                }
                addonArgumentListPassToBubbleEditor = "";
                //
                // Build output strings from name and value found
                //
                for (ConstructorPtr = 0; ConstructorPtr < ConstructorCnt; ConstructorPtr++) {
                    ConstructorName = ConstructorNames[ConstructorPtr];
                    ConstructorValue = ConstructorValues[ConstructorPtr];
                    ConstructorSelector = ConstructorSelectors[ConstructorPtr];
                    // here goes nothing!!
                    addonInstanceProperties.Add(ConstructorName, ConstructorValue);
                    //OptionString_ForObjectCall = OptionString_ForObjectCall & csv_DecodeAddonOptionArgument(ConstructorName) & "=" & csv_DecodeAddonOptionArgument(ConstructorValue) & vbCrLf
                    if (IncludeSettingsBubbleOptions) {
                        addonArgumentListPassToBubbleEditor = addonArgumentListPassToBubbleEditor + "\r\n" + core.html.getAddonSelector(ConstructorName, ConstructorValue, ConstructorSelector);
                    }
                }
                addonInstanceProperties.Add("InstanceID", InstanceID);
                //If OptionString_ForObjectCall <> "" Then
                //    OptionString_ForObjectCall = Mid(OptionString_ForObjectCall, 1, Len(OptionString_ForObjectCall) - 1)
                //    'OptionString_ForObjectCall = Mid(OptionString_ForObjectCall, 1, Len(OptionString_ForObjectCall) - 2)
                //End If
                if (!string.IsNullOrEmpty(addonArgumentListPassToBubbleEditor)) {
                    addonArgumentListPassToBubbleEditor = addonArgumentListPassToBubbleEditor.Substring(2);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //===================================================================================================
        //   Build AddonOptionLists
        //
        //   On entry:
        //       AddonOptionConstructor = the addon-encoded Version of the list that comes from the Addon Record
        //           It is line-delimited with &, and all escape characters converted
        //       InstanceOptionList = addonencoded Version of the list that comes from the HTML AC tag
        //           that means crlf line-delimited
        //
        //   On Exit:
        //       AddonOptionNameValueList
        //               pass this string to the addon when it is run, crlf delimited name=value pair.
        //               This should include just the name=values pairs, with no selectors
        //               it should include names from both Addon and Instance
        //               If the Instance has a value, include it. Otherwise include Addon value
        //       AddonOptionExpandedConstructor = pass this to the bubble editor to create the the selectr
        //===================================================================================================
        //
        public void buildAddonOptionLists(ref Dictionary<string, string> addonInstanceProperties, ref string addonArgumentListPassToBubbleEditor, string addonArgumentListFromRecord, Dictionary<string, string> InstanceOptionList, string InstanceID, bool IncludeEditWrapper) {
            buildAddonOptionLists2(ref addonInstanceProperties, ref addonArgumentListPassToBubbleEditor, addonArgumentListFromRecord, InstanceOptionList, InstanceID, IncludeEditWrapper);
        }
        //
        //====================================================================================================
        //
        public string getPrivateFilesAddonPath() {
            return "addons\\";
        }
        //
        //====================================================================================================
        //   Apply a wrapper to content
        // todo -- wrapper should be an addon !!!
        private string addWrapperToResult(string Content, int WrapperID, string WrapperSourceForComment = "") {
            string s = "";
            try {
                //
                int Pos = 0;
                int CS = 0;
                string JSFilename = null;
                string Copy = null;
                string SelectFieldList = null;
                string Wrapper = null;
                string wrapperName = null;
                string SourceComment = null;
                string TargetString = null;
                //
                s = Content;
                SelectFieldList = "name,copytext,javascriptonload,javascriptbodyend,stylesfilename,otherheadtags,JSFilename,targetString";
                CS = core.db.csOpenRecord("Wrappers", WrapperID, false, false, SelectFieldList);
                if (core.db.csOk(CS)) {
                    Wrapper = core.db.csGetText(CS, "copytext");
                    wrapperName = core.db.csGetText(CS, "name");
                    TargetString = core.db.csGetText(CS, "targetString");
                    //
                    SourceComment = "wrapper " + wrapperName;
                    if (!string.IsNullOrEmpty(WrapperSourceForComment)) {
                        SourceComment = SourceComment + " for " + WrapperSourceForComment;
                    }
                    core.html.addScriptCode_onLoad(core.db.csGetText(CS, "javascriptonload"), SourceComment);
                    core.html.addScriptCode(core.db.csGetText(CS, "javascriptbodyend"), SourceComment);
                    core.html.addHeadTag(core.db.csGetText(CS, "OtherHeadTags"), SourceComment);
                    //
                    JSFilename = core.db.csGetText(CS, "jsfilename");
                    if (!string.IsNullOrEmpty(JSFilename)) {
                        JSFilename = GenericController.getCdnFileLink(core, JSFilename);
                        core.html.addScriptLinkSrc(JSFilename, SourceComment);
                    }
                    Copy = core.db.csGetText(CS, "stylesfilename");
                    if (!string.IsNullOrEmpty(Copy)) {
                        if (GenericController.vbInstr(1, Copy, "://") != 0) {
                        } else if (Copy.Left(1) == "/") {
                        } else {
                            Copy = GenericController.getCdnFileLink(core, Copy);
                        }
                        core.html.addStyleLink(Copy, SourceComment);
                    }
                    //
                    if (!string.IsNullOrEmpty(Wrapper)) {
                        Pos = GenericController.vbInstr(1, Wrapper, TargetString, 1);
                        if (Pos != 0) {
                            s = GenericController.vbReplace(Wrapper, TargetString, s, 1, 99, 1);
                        } else {
                            s = ""
                                + "<!-- the selected wrapper does not include the Target String marker to locate the position of the content. -->"
                                + Wrapper + s;
                        }
                    }
                }
                core.db.csClose(ref CS);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return s;
        }
        //
        //====================================================================================================
        // main_Get an XML nodes attribute based on its name
        //
        public string xml_GetAttribute(bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
            string result = "";
            try {
                //
                //todo  NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlAttribute NodeAttribute = null;
                XmlNode ResultNode = null;
                string UcaseName = null;
                //
                Found = false;
                ResultNode = Node.Attributes.GetNamedItem(Name);
                if (ResultNode == null) {
                    UcaseName = GenericController.vbUCase(Name);
                    foreach (XmlAttribute NodeAttribute in Node.Attributes) {
                        if (GenericController.vbUCase(NodeAttribute.Name) == UcaseName) {
                            result = NodeAttribute.Value;
                            Found = true;
                            break;
                        }
                    }
                } else {
                    result = ResultNode.Value;
                    Found = true;
                }
                if (!Found) {
                    result = DefaultIfNotFound;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get an option from an argument list
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ArgumentList"></param>
        /// <param name="AddonGuid"></param>
        /// <param name="IsInline"></param>
        /// <returns></returns>
        public static string getDefaultAddonOptions(CoreController core, string ArgumentList, string AddonGuid, bool IsInline) {
            string result = "";
            //
            string NameValuePair = null;
            int Pos = 0;
            string OptionName = null;
            string OptionValue = null;
            string OptionSelector = null;
            string[] QuerySplit = null;
            string NameValue = null;
            int Ptr = 0;
            //
            ArgumentList = GenericController.vbReplace(ArgumentList, "\r\n", "\r");
            ArgumentList = GenericController.vbReplace(ArgumentList, "\n", "\r");
            ArgumentList = GenericController.vbReplace(ArgumentList, "\r", "\r\n");
            if (ArgumentList.IndexOf("wrapper", System.StringComparison.OrdinalIgnoreCase) == -1) {
                //
                // Add in default constructors, like wrapper
                //
                if (!string.IsNullOrEmpty(ArgumentList)) {
                    ArgumentList = ArgumentList + "\r\n";
                }
                if (GenericController.vbLCase(AddonGuid) == GenericController.vbLCase(addonGuidContentBox)) {
                    ArgumentList = ArgumentList + AddonOptionConstructor_BlockNoAjax;
                } else if (IsInline) {
                    ArgumentList = ArgumentList + AddonOptionConstructor_Inline;
                } else {
                    ArgumentList = ArgumentList + AddonOptionConstructor_Block;
                }
            }
            if (!string.IsNullOrEmpty(ArgumentList)) {
                //
                // Argument list is present, translate from AddonConstructor to AddonOption format (see main_executeAddon for details)
                //
                QuerySplit = GenericController.splitNewLine(ArgumentList);
                result = "";
                for (Ptr = 0; Ptr <= QuerySplit.GetUpperBound(0); Ptr++) {
                    NameValue = QuerySplit[Ptr];
                    if (!string.IsNullOrEmpty(NameValue)) {
                        //
                        // Execute list functions
                        //
                        OptionName = "";
                        OptionValue = "";
                        OptionSelector = "";
                        //
                        // split on equal
                        //
                        NameValue = GenericController.vbReplace(NameValue, "\\=", "\r\n");
                        Pos = GenericController.vbInstr(1, NameValue, "=");
                        if (Pos == 0) {
                            OptionName = NameValue;
                        } else {
                            OptionName = NameValue.Left(Pos - 1);
                            OptionValue = NameValue.Substring(Pos);
                        }
                        OptionName = GenericController.vbReplace(OptionName, "\r\n", "\\=");
                        OptionValue = GenericController.vbReplace(OptionValue, "\r\n", "\\=");
                        //
                        // split optionvalue on [
                        //
                        OptionValue = GenericController.vbReplace(OptionValue, "\\[", "\r\n");
                        Pos = GenericController.vbInstr(1, OptionValue, "[");
                        if (Pos != 0) {
                            OptionSelector = OptionValue.Substring(Pos - 1);
                            OptionValue = OptionValue.Left(Pos - 1);
                        }
                        OptionValue = GenericController.vbReplace(OptionValue, "\r\n", "\\[");
                        OptionSelector = GenericController.vbReplace(OptionSelector, "\r\n", "\\[");
                        //
                        // Decode AddonConstructor format
                        //
                        OptionName = GenericController.DecodeAddonConstructorArgument(OptionName);
                        OptionValue = GenericController.DecodeAddonConstructorArgument(OptionValue);
                        //
                        // Encode AddonOption format
                        //
                        //main_GetAddonSelector expects value to be encoded, but not name
                        //OptionName = encodeNvaArgument(OptionName)
                        OptionValue = GenericController.encodeNvaArgument(OptionValue);
                        //
                        // rejoin
                        //
                        NameValuePair = core.html.getAddonSelector(OptionName, OptionValue, OptionSelector);
                        NameValuePair = GenericController.EncodeJavascriptStringSingleQuote(NameValuePair);
                        result += "&" + NameValuePair;
                        if (GenericController.vbInstr(1, NameValuePair, "=") == 0) {
                            result += "=";
                        }
                    }
                }
                if (!string.IsNullOrEmpty(result)) {
                    // remove leading "&"
                    result = result.Substring(1);
                }
            }
            return result;
        }
        //
        //====================================================================================================
        //   csv_GetAddonOption
        //
        //   returns the value matching a given name in an AddonOptionConstructor
        //
        //   AddonOptionConstructor is a crlf delimited name=value[selector]descriptor list
        //
        //   See coreClass.ExecuteAddon for a full description of:
        //       AddonOptionString
        //       AddonOptionConstructor
        //       AddonOptionNameValueList
        //       AddonOptionExpandedConstructor
        //====================================================================================================
        //
        public static string getAddonOption(string OptionName, string OptionString) {
            string result = "";
            string WorkingString = null;
            string[] Options = null;
            int Ptr = 0;
            int Pos = 0;
            string TestName = null;
            string TargetName = null;
            //
            WorkingString = OptionString;
            result = "";
            if (!string.IsNullOrEmpty(WorkingString)) {
                TargetName = GenericController.vbLCase(OptionName);
                Options = OptionString.Split('&');
                for (Ptr = 0; Ptr <= Options.GetUpperBound(0); Ptr++) {
                    Pos = GenericController.vbInstr(1, Options[Ptr], "=");
                    if (Pos > 0) {
                        TestName = GenericController.vbLCase((Options[Ptr].Left(Pos - 1)).Trim(' '));
                        while ((!string.IsNullOrEmpty(TestName)) && (TestName.Left(1) == "\t")) {
                            TestName = TestName.Substring(1).Trim(' ');
                        }
                        while ((!string.IsNullOrEmpty(TestName)) && (TestName.Substring(TestName.Length - 1) == "\t")) {
                            TestName = (TestName.Left(TestName.Length - 1)).Trim(' ');
                        }
                        if (TestName == TargetName) {
                            result = GenericController.decodeNvaArgument((Options[Ptr].Substring(Pos)).Trim(' '));
                            break;
                        }
                    }
                }
            }
            return result;
        }
        //
        //====================================================================================================
        //
        private string getAddonDescription(CoreController core, AddonModel addon) {
            string addonDescription = "[invalid addon]";
            if (addon != null) {
                string collectionName = "invalid collection or collection not set";
                AddonCollectionModel collection = AddonCollectionModel.create(core, addon.collectionID);
                if (collection != null) {
                    collectionName = collection.name;
                }
                addonDescription = "[#" + addon.id.ToString() + ", " + addon.name + "], collection [" + collectionName + "]";
            }
            return addonDescription;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Special case addon as it is a required core service. This method attempts the addon call and it if fails, calls the safe-mode version, tested for this build
        /// </summary>
        /// <returns></returns>
        public static string getAddonManager(CoreController core) {
            string result = "";
            try {
                bool AddonStatusOK = true;
                try {
                    AddonModel addon = AddonModel.create(core, addonGuidAddonManager);
                    if (addon != null) {
                        result = core.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext() {
                            addonType = CPUtilsBaseClass.addonContext.ContextAdmin,
                            errorContextMessage = "calling addon manager guid for GetAddonManager method"
                        });
                    }
                } catch (Exception ex) {
                    LogController.handleError(core, new Exception("Error calling ExecuteAddon with AddonManagerGuid, will attempt Safe Mode Addon Manager. Exception=[" + ex.ToString() + "]"));
                    AddonStatusOK = false;
                }
                if (string.IsNullOrEmpty(result)) {
                    LogController.handleError(core, new Exception("AddonManager returned blank, calling Safe Mode Addon Manager."));
                    AddonStatusOK = false;
                }
                if (!AddonStatusOK) {
                    Addons.SafeAddonManager.AddonManagerClass AddonMan = new Addons.SafeAddonManager.AddonManagerClass(core);
                    result = AddonMan.getForm_SafeModeAddonManager();
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Throw an addon event, which will call all addons registered to handle it
        /// </summary>
        /// <param name="eventNameIdOrGuid"></param>
        /// <returns></returns>
        public string throwEvent(string eventNameIdOrGuid) {
            string returnString = "";
            try {
                string sql = null;
                var cs = new CsController(core);
                int addonid = 0;
                //
                sql = "select e.id,c.addonId"
                    + " from (ccAddonEvents e"
                    + " left join ccAddonEventCatchers c on c.eventId=e.id)"
                    + " where ";
                if (eventNameIdOrGuid.IsNumeric()) {
                    sql += "e.id=" + DbController.encodeSQLNumber(double.Parse(eventNameIdOrGuid));
                } else if (GenericController.isGuid(eventNameIdOrGuid)) {
                    sql += "e.ccGuid=" + DbController.encodeSQLText(eventNameIdOrGuid);
                } else {
                    sql += "e.name=" + DbController.encodeSQLText(eventNameIdOrGuid);
                }
                if (!cs.openSQL(sql)) {
                    //
                    // event not found
                    //
                    if (eventNameIdOrGuid.IsNumeric()) {
                        //
                        // can not create an id
                        //
                    } else if (GenericController.isGuid(eventNameIdOrGuid)) {
                        //
                        // create event with Guid and id for name
                        //
                        cs.close();
                        cs.insert("add-on Events");
                        cs.setField("ccguid", eventNameIdOrGuid);
                        cs.setField("name", "Event " + cs.getInteger("id").ToString());
                    } else if (!string.IsNullOrEmpty(eventNameIdOrGuid)) {
                        //
                        // create event with name
                        //
                        cs.close();
                        cs.insert("add-on Events");
                        cs.setField("name", eventNameIdOrGuid);
                    }
                } else {
                    while (cs.ok()) {
                        addonid = cs.getInteger("addonid");
                        if (addonid != 0) {
                            var addon = AddonModel.create(core, addonid);
                            returnString += core.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext {
                                addonType = CPUtilsBaseClass.addonContext.ContextSimple,
                                errorContextMessage = "calling handler addon id [" + addonid + "] for event [" + eventNameIdOrGuid + "]"
                            });
                        }
                        cs.goNext();
                    }
                }
                cs.close();
                //
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return returnString;
        }
        //
        //====================================================================================================
        /// <summary>
        /// If an addon assembly references a system assembly that is not in the gac (system.io.compression.filesystem), it does not look in the folder I did the loadfrom.
        /// Problem is knowing where to look. No argument to pass a path...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Assembly myAssemblyResolve(object sender, ResolveEventArgs args) {
            string sample_folderPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string assemblyPath = Path.Combine(sample_folderPath, (new AssemblyName(args.Name)).Name + ".dll");
            if (!File.Exists(assemblyPath)) {
                return null;
            }
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }
        //
        //========================================================================================================
        /// <summary>
        /// create an addon icon image for the desktop
        /// </summary>
        /// <param name="AdminURL"></param>
        /// <param name="IconWidth"></param>
        /// <param name="IconHeight"></param>
        /// <param name="IconSprites"></param>
        /// <param name="IconIsInline"></param>
        /// <param name="IconImgID"></param>
        /// <param name="IconFilename"></param>
        /// <param name="serverFilePath"></param>
        /// <param name="IconAlt"></param>
        /// <param name="IconTitle"></param>
        /// <param name="ACInstanceID"></param>
        /// <param name="IconSpriteColumn"></param>
        /// <returns></returns>
        public static string getAddonIconImg(string AdminURL, int IconWidth, int IconHeight, int IconSprites, bool IconIsInline, string IconImgID, string IconFilename, string serverFilePath, string IconAlt, string IconTitle, string ACInstanceID, int IconSpriteColumn) {
            string tempGetAddonIconImg = "";
            try {
                if (string.IsNullOrEmpty(IconAlt)) {
                    IconAlt = "Add-on";
                }
                if (string.IsNullOrEmpty(IconTitle)) {
                    IconTitle = "Rendered as Add-on";
                }
                if (string.IsNullOrEmpty(IconFilename)) {
                    //
                    // No icon given, use the default
                    //
                    if (IconIsInline) {
                        IconFilename = "/ContensiveBase/images/IconAddonInlineDefault.png";
                        IconWidth = 62;
                        IconHeight = 17;
                        IconSprites = 0;
                    } else {
                        IconFilename = "/ContensiveBase/images/IconAddonBlockDefault.png";
                        IconWidth = 57;
                        IconHeight = 59;
                        IconSprites = 4;
                    }
                } else if (vbInstr(1, IconFilename, "://") != 0) {
                    //
                    // icon is an Absolute URL - leave it
                    //
                } else if (IconFilename.Left(1) == "/") {
                    //
                    // icon is Root Relative, leave it
                    //
                } else {
                    //
                    // icon is a virtual file, add the serverfilepath
                    //
                    IconFilename = serverFilePath + IconFilename;
                }
                //IconFilename = encodeJavascript(IconFilename)
                if ((IconWidth == 0) || (IconHeight == 0)) {
                    IconSprites = 0;
                }

                if (IconSprites == 0) {
                    //
                    // just the icon
                    //
                    tempGetAddonIconImg = "<img"
                        + " border=0"
                        + " id=\"" + IconImgID + "\""
                        + " onDblClick=\"window.parent.OpenAddonPropertyWindow(this,'" + AdminURL + "');\""
                        + " alt=\"" + IconAlt + "\""
                        + " title=\"" + IconTitle + "\""
                        + " src=\"" + IconFilename + "\"";
                    if (IconWidth != 0) {
                        tempGetAddonIconImg += " width=\"" + IconWidth + "px\"";
                    }
                    if (IconHeight != 0) {
                        tempGetAddonIconImg += " height=\"" + IconHeight + "px\"";
                    }
                    if (IconIsInline) {
                        tempGetAddonIconImg += " style=\"vertical-align:middle;display:inline;\" ";
                    } else {
                        tempGetAddonIconImg += " style=\"display:block\" ";
                    }
                    if (!string.IsNullOrEmpty(ACInstanceID)) {
                        tempGetAddonIconImg += " ACInstanceID=\"" + ACInstanceID + "\"";
                    }
                    tempGetAddonIconImg += ">";
                } else {
                    //
                    // Sprite Icon
                    //
                    tempGetAddonIconImg = getIconSprite(IconImgID, IconSpriteColumn, IconFilename, IconWidth, IconHeight, IconAlt, IconTitle, "window.parent.OpenAddonPropertyWindow(this,'" + AdminURL + "');", IconIsInline, ACInstanceID);
                }
            } catch (Exception) {
                throw;
            }
            return tempGetAddonIconImg;
        }
        //
        //========================================================================================================
        /// <summary>
        /// get addon sprite img
        /// </summary>
        /// <param name="TagID"></param>
        /// <param name="SpriteColumn"></param>
        /// <param name="IconSrc"></param>
        /// <param name="IconWidth"></param>
        /// <param name="IconHeight"></param>
        /// <param name="IconAlt"></param>
        /// <param name="IconTitle"></param>
        /// <param name="onDblClick"></param>
        /// <param name="IconIsInline"></param>
        /// <param name="ACInstanceID"></param>
        /// <returns></returns>
        public static string getIconSprite(string TagID, int SpriteColumn, string IconSrc, int IconWidth, int IconHeight, string IconAlt, string IconTitle, string onDblClick, bool IconIsInline, string ACInstanceID) {
            string tempGetIconSprite = "";
            try {
                tempGetIconSprite = "<img"
                    + " border=0"
                    + " id=\"" + TagID + "\""
                    + " onMouseOver=\"this.style.backgroundPosition='" + (-1 * SpriteColumn * IconWidth) + "px -" + (2 * IconHeight) + "px';\""
                    + " onMouseOut=\"this.style.backgroundPosition='" + (-1 * SpriteColumn * IconWidth) + "px 0px'\""
                    + " onDblClick=\"" + onDblClick + "\""
                    + " alt=\"" + IconAlt + "\""
                    + " title=\"" + IconTitle + "\""
                    + " src=\"/ContensiveBase/images/spacer.gif\"";
                string ImgStyle = "background:url(" + IconSrc + ") " + (-1 * SpriteColumn * IconWidth) + "px 0px no-repeat;";
                ImgStyle += "width:" + IconWidth + "px;";
                ImgStyle = ImgStyle + "height:" + IconHeight + "px;";
                if (IconIsInline) {
                    ImgStyle += "vertical-align:middle;display:inline;";
                } else {
                    ImgStyle += "display:block;";
                }
                if (!string.IsNullOrEmpty(ACInstanceID)) {
                    tempGetIconSprite += " ACInstanceID=\"" + ACInstanceID + "\"";
                }
                tempGetIconSprite += " style=\"" + ImgStyle + "\">";
            } catch (Exception) {
                throw;
            }
            return tempGetIconSprite;
        }
        //====================================================================================================
        #region  IDisposable Support 
        //
        // this class must implement System.IDisposable
        // never throw an exception in dispose
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        //====================================================================================================
        //
        protected bool disposed = false;
        //
        public void Dispose() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~AddonController() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(false);
            
            
        }
        //
        //====================================================================================================
        /// <summary>
        /// dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    //If Not (AddonObj Is Nothing) Then AddonObj.Dispose()
                }
                //
                // cleanup non-managed objects
                //
            }
        }
        #endregion
    }
}