
using System;
using System.Reflection;
using System.Xml;
using Contensive.Core.Models.Entity;
using Contensive.BaseClasses;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
using System.Collections.Generic;

namespace Contensive.Core.Controllers {
    //
    // replace mscript with https://github.com/Microsoft/ClearScript
    //
    //
    //====================================================================================================
    /// <summary>
    /// classSummary
    /// - first routine should be constructor
    /// - disposable region at end
    /// - if disposable is not needed add: not IDisposable - not contained classes that need to be disposed
    /// </summary>
    public class addonController : IDisposable {
        //
        // ----- objects passed in constructor, do not dispose
        //
        private coreClass cpCore;
        //
        public addonController(coreClass cpCore) : base() {
            this.cpCore = cpCore;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Execute an addon because it is a dependency of another addon/page/template
        /// </summary>
        /// <param name="addonId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// 
        public string executeDependency(Models.Entity.addonModel addon, CPUtilsBaseClass.addonExecuteContext context) {
            bool saveContextIsIncludeAddon = context.isIncludeAddon;
            context.isIncludeAddon = true;
            string result = execute(addon, context);
            context.isIncludeAddon = saveContextIsIncludeAddon;
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute addon
        /// </summary>
        /// <param name="addon"></param>
        /// <param name="executeContext"></param>
        /// <returns></returns>
        public string execute(Models.Entity.addonModel addon, CPUtilsBaseClass.addonExecuteContext executeContext) {
            string result = string.Empty;
            bool rootLevelAddon = cpCore.doc.addonsCurrentlyRunningIdList.Count.Equals(0);
            try {
                if (addon == null) {
                    //
                    // -- addon not found
                    cpCore.handleException(new ArgumentException("AddonExecute called without valid addon."));
                } else if (executeContext == null) {
                    //
                    // -- context not configured 
                    cpCore.handleException(new ArgumentException("The Add-on executeContext was not configured for addon [#" + addon.id + ", " + addon.name + "]."));
                } else if (!string.IsNullOrEmpty(addon.ObjectProgramID)) {
                    //
                    // -- addons with activeX components are deprecated
                    string addonDescription = getAddonDescription(cpCore, addon);
                    throw new ApplicationException("Addon is no longer supported because it contains an active-X component, add-on " + addonDescription + ".");
                } else if (cpCore.doc.addonsCurrentlyRunningIdList.Contains(addon.id)) {
                    //
                    // -- cannot call an addon within an addon
                    throw new ApplicationException("Addon cannot be called by itself [#" + addon.id + ", " + addon.name + "].");
                } else {
                    //
                    // -- ok to execute
                    string parentInstanceId = cpCore.docProperties.getText("instanceId");
                    cpCore.docProperties.setProperty("instanceId", executeContext.instanceGuid);
                    cpCore.doc.addonsCurrentlyRunningIdList.Add(addon.id);
                    //
                    // -- run included add-ons before their parent
                    List<Models.Entity.addonIncludeRuleModel> addonIncludeRules = Models.Entity.addonIncludeRuleModel.createList(cpCore, "(addonid=" + addon.id + ")");
                    if (addonIncludeRules.Count > 0) {
                        foreach (Models.Entity.addonIncludeRuleModel addonRule in addonIncludeRules) {
                            if (addonRule.IncludedAddonID > 0) {
                                addonModel dependentAddon = addonModel.create(cpCore, addonRule.IncludedAddonID);
                                if (dependentAddon == null) {
                                    cpCore.handleException(new ApplicationException("Addon not found. An included addon of [" + addon.name + "] was not found. The included addon may have been deleted. Recreate or reinstall the missing addon, then reinstall [" + addon.name + "] or manually correct the included addon selection."));
                                } else {
                                    result += executeDependency(dependentAddon, executeContext);
                                }
                            }
                        }
                    }
                    //
                    // -- add test point message after dependancies so debug list shows them in the order they ran, not the order they were called.
                    debugController.testPoint(cpCore, "execute [#" + addon.id + ", " + addon.name + ", guid " + addon.ccguid + "]");
                    //
                    // -- properties referenced multiple time 
                    bool allowAdvanceEditor = cpCore.visitProperty.getBoolean("AllowAdvancedEditor");
                    //
                    // -- add addon record arguments to doc properties
                    foreach (var addon_argument in addon.ArgumentList.Replace(Environment.NewLine, "\r").Replace("\n", "\r").Split(Convert.ToChar("\r"))) {
                        if (!string.IsNullOrEmpty(addon_argument)) {
                            string[] nvp = addon_argument.Split('=');
                            if (!string.IsNullOrEmpty(nvp[0])) {
                                string nvpValue = "";
                                if (nvp.Length > 1) {
                                    nvpValue = nvp[1];
                                }
                                if (nvpValue.IndexOf("[") >= 0) {
                                    nvpValue = nvpValue.Substring(0, nvpValue.IndexOf("["));
                                }
                                cpCore.docProperties.setProperty(nvp[0], nvpValue);
                            }
                        }
                    }
                    //
                    // -- add instance properties to doc properties
                    string ContainerCssID = "";
                    string ContainerCssClass = "";
                    foreach (var kvp in executeContext.instanceArguments) {
                        switch (kvp.Key.ToLower()) {
                            case "wrapper":
                                executeContext.wrapperID = genericController.EncodeInteger(kvp.Value);
                                break;
                            case "as ajax":
                                addon.AsAjax = genericController.EncodeBoolean(kvp.Value);
                                break;
                            case "css container id":
                                ContainerCssID = kvp.Value;
                                break;
                            case "css container class":
                                ContainerCssClass = kvp.Value;
                                break;
                        }
                        cpCore.docProperties.setProperty(kvp.Key, kvp.Value);
                    }
                    //
                    // Preprocess arguments into OptionsForCPVars, and set generic instance values wrapperid and asajax
                    if (addon.InFrame & (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) & (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodJson)) {
                        //
                        // -- inframe execution, deliver iframe with link back to remote method
                        result = "TBD - inframe";
                        //Link = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & requestAppRootPath & cpCore.siteProperties.serverPageDefault
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
                        //FrameID = "frame" & GetRandomInteger()
                        //returnVal = "<iframe src=""" & Link & """ id=""" & FrameID & """ onload=""cj.setFrameHeight('" & FrameID & "');"" class=""ccAddonFrameCon"" frameborder=""0"" scrolling=""no"">This content is not visible because your browser does not support iframes</iframe>" _
                        //        & cr & "<script language=javascript type=""text/javascript"">" _
                        //        & cr & "// Safari and Opera need a kick-start." _
                        //        & cr & "var e=document.getElementById('" & FrameID & "');if(e){var iSource=e.src;e.src='';e.src = iSource;}" _
                        //        & cr & "</script>"
                    } else if (addon.AsAjax & (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) & (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodJson)) {
                        //
                        // -- asajax execution, deliver div with ajax callback
                        //
                        result = "TBD - asajax";
                        //'-----------------------------------------------------------------
                        //' AsAjax and this is NOT the callback - setup the ajax callback
                        //' js,styles and other features from the addon record are added to the host page
                        //' during the remote method, these are blocked, but if any are added during
                        //'   DLL processing, they have to be handled
                        //'-----------------------------------------------------------------
                        //'
                        //If True Then
                        //    AsAjaxID = "asajax" & GetRandomInteger()
                        //    QS = "" _
                        //& RequestNameRemoteMethodAddon & "=" & EncodeRequestVariable(addon.id.ToString()) _
                        //& "&HostContentName=" & EncodeRequestVariable(HostContentName) _
                        //& "&HostRecordID=" & HostRecordID _
                        //& "&HostRQS=" & EncodeRequestVariable(cpCore.doc.refreshQueryString) _
                        //& "&HostQS=" & EncodeRequestVariable(cpCore.webServer.requestQueryString) _
                        //& "&optionstring=" & EncodeRequestVariable(WorkingOptionString) _
                        //& ""
                        //    '
                        //    ' -- exception made here. AsAjax is not used often, and this can create a QS too long
                        //    '& "&HostForm=" & EncodeRequestVariable(cpCore.webServer.requestFormString) _
                        //    If IsInline Then
                        //        returnVal = cr & "<div ID=" & AsAjaxID & " Class=""ccAddonAjaxCon"" style=""display:inline;""><img src=""/ccLib/images/ajax-loader-small.gif"" width=""16"" height=""16""></div>"
                        //    Else
                        //        returnVal = cr & "<div ID=" & AsAjaxID & " Class=""ccAddonAjaxCon""><img src=""/ccLib/images/ajax-loader-small.gif"" width=""16"" height=""16""></div>"
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
                        //    Call cpCore.html.doc_AddPagetitle2(PageTitle, AddedByName)
                        //    Call cpCore.html.doc_addMetaDescription2(MetaDescription, AddedByName)
                        //    Call cpCore.html.doc_addMetaKeywordList2(MetaKeywordList, AddedByName)
                        //    Call cpCore.html.doc_AddHeadTag2(OtherHeadTags, AddedByName)
                        //    If Not blockJavascriptAndCss Then
                        //        '
                        //        ' add javascript and styles if it has not run already
                        //        '
                        //        Call cpCore.html.addOnLoadJavascript(JSOnLoad, AddedByName)
                        //        Call cpCore.html.addBodyJavascriptCode(JSBodyEnd, AddedByName)
                        //        Call cpCore.html.addJavaScriptLinkHead(JSFilename, AddedByName)
                        //        If addon.StylesFilename.filename <> "" Then
                        //            Call cpCore.html.addStyleLink(cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, addon.StylesFilename.filename), addon.name & " default")
                        //        End If
                        //        'If CustomStylesFilename <> "" Then
                        //        '    Call cpCore.html.addStyleLink(cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, CustomStylesFilename), AddonName & " custom")
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
                        if (addon.InFrame && (executeContext.addonType == CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml)) {
                            //
                            // -- remote method called from inframe execution
                            result = "TBD - remotemethod inframe";
                            // Add-on setup for InFrame, running the call-back - this page must think it is just the remotemethod
                            //If True Then
                            //    Call cpCore.doc.addRefreshQueryString(RequestNameRemoteMethodAddon, addon.id.ToString)
                            //    Call cpCore.doc.addRefreshQueryString("optionstring", WorkingOptionString)
                            //End If
                        } else if (addon.AsAjax && (executeContext.addonType == CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml)) {
                            //
                            // -- remotemethod called from asajax execution
                            result = "TBD - remotemethod ajax";
                            //'
                            //' Add-on setup for AsAjax, running the call-back - put the referring page's QS as the RQS
                            //' restore form values
                            //'
                            //If True Then
                            //    QS = cpCore.docProperties.getText("Hostform")
                            //    If QS <> "" Then
                            //        Call cpCore.docProperties.addQueryString(QS)
                            //    End If
                            //    '
                            //    ' restore refresh querystring values
                            //    '
                            //    QS = cpCore.docProperties.getText("HostRQS")
                            //    QSSplit = Split(QS, "&")
                            //    For Ptr = 0 To UBound(QSSplit)
                            //        NVPair = QSSplit[Ptr]
                            //        If NVPair <> "" Then
                            //            NVSplit = Split(NVPair, "=")
                            //            If UBound(NVSplit) > 0 Then
                            //                Call cpCore.doc.addRefreshQueryString(NVSplit(0), NVSplit(1))
                            //            End If
                            //        End If
                            //    Next
                            //    '
                            //    ' restore query string
                            //    '
                            //    QS = cpCore.docProperties.getText("HostQS")
                            //    Call cpCore.docProperties.addQueryString(QS)
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
                        string TestString = addon.Copy + addon.CopyText + addon.PageTitle + addon.MetaDescription + addon.MetaKeywordList + addon.OtherHeadTags + addon.FormXML;
                        if (!string.IsNullOrEmpty(TestString)) {
                            foreach (var key in cpCore.docProperties.getKeyList) {
                                string ReplaceSource = "$" + key + "$";
                                if (TestString.IndexOf(ReplaceSource) >= 0) {
                                    string ReplaceValue = cpCore.docProperties.getText(key);
                                    addon.Copy = addon.Copy.Replace(ReplaceSource, ReplaceValue);
                                    addon.CopyText = addon.CopyText.Replace(ReplaceSource, ReplaceValue);
                                    addon.PageTitle = addon.PageTitle.Replace(ReplaceSource, ReplaceValue);
                                    addon.MetaDescription = addon.MetaDescription.Replace(ReplaceSource, ReplaceValue);
                                    addon.MetaKeywordList = addon.MetaKeywordList.Replace(ReplaceSource, ReplaceValue);
                                    addon.OtherHeadTags = addon.OtherHeadTags.Replace(ReplaceSource, ReplaceValue);
                                    addon.FormXML = addon.FormXML.Replace(ReplaceSource, ReplaceValue);
                                }
                            }
                        }
                        //
                        // -- text components
                        result += addon.CopyText + addon.Copy;
                        if (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextEditor) {
                            //
                            // not editor, encode the content parts of the addon
                            //
                            result = addon.CopyText + addon.Copy;
                            if (!string.IsNullOrEmpty(result)) {
                                string ignoreLayoutErrors = string.Empty;
                                result = cpCore.html.executeContentCommands(null, result, CPUtilsBaseClass.addonContext.ContextAdmin, executeContext.personalizationPeopleId, executeContext.personalizationAuthenticated, ignoreLayoutErrors);
                            }
                            switch (executeContext.addonType) {
                                case CPUtilsBaseClass.addonContext.ContextEditor:
                                    result = cpCore.html.convertActiveContentToHtmlForWysiwygEditor(result);
                                    break;
                                case CPUtilsBaseClass.addonContext.ContextEmail:
                                    result = cpCore.html.convertActiveContentToHtmlForEmailSend(result, executeContext.personalizationPeopleId, "");
                                    break;
                                case CPUtilsBaseClass.addonContext.ContextFilter:
                                case CPUtilsBaseClass.addonContext.ContextOnBodyEnd:
                                case CPUtilsBaseClass.addonContext.ContextOnBodyStart:
                                case CPUtilsBaseClass.addonContext.ContextOnBodyEnd:
                                case CPUtilsBaseClass.addonContext.ContextOnPageEnd:
                                case CPUtilsBaseClass.addonContext.ContextOnPageStart:
                                case CPUtilsBaseClass.addonContext.ContextPage:
                                case CPUtilsBaseClass.addonContext.ContextTemplate:
                                case CPUtilsBaseClass.addonContext.ContextAdmin:
                                case CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml:
                                    result = cpCore.html.convertActiveContentToHtmlForWebRender(result, executeContext.hostRecord.contentName, executeContext.hostRecord.recordId, executeContext.personalizationPeopleId, "", 0, executeContext.addonType);
                                    break;
                                case CPUtilsBaseClass.addonContext.ContextOnContentChange:
                                case CPUtilsBaseClass.addonContext.ContextSimple:
                                    result = cpCore.html.convertActiveContentToHtmlForWebRender(result, "", 0, executeContext.personalizationPeopleId, "", 0, executeContext.addonType);
                                    break;
                                case CPUtilsBaseClass.addonContext.ContextRemoteMethodJson:
                                    result = cpCore.html.convertActiveContentToJsonForRemoteMethod(result, "", 0, executeContext.personalizationPeopleId, "", 0, "", executeContext.addonType);
                                    break;
                                default:
                                    result = cpCore.html.convertActiveContentToHtmlForWebRender(result, "", 0, executeContext.personalizationPeopleId, "", 0, executeContext.addonType);
                                    break;
                            }
                            //result = cpCore.html.convertActiveContent_internal(result, executeContext.personalizationPeopleId, executeContext.hostRecord.contentName, executeContext.hostRecord.recordId, 0, False, False, True, True, False, True, "", "", (executeContext.addonType = CPUtilsBaseClass.addonContext.ContextEmail), executeContext.wrapperID, "", executeContext.addonType, executeContext.personalizationAuthenticated, Nothing, False)
                        }
                        //
                        // -- Scripting code
                        if (addon.ScriptingCode != "") {
                            //
                            // Get Language
                            string ScriptingLanguage = string.Empty;
                            if (addon.ScriptingLanguageID != 0) {
                                ScriptingLanguage = cpCore.db.getRecordName("Scripting Languages", addon.ScriptingLanguageID);
                            }
                            if (string.IsNullOrEmpty(ScriptingLanguage)) {
                                ScriptingLanguage = "VBScript";
                            }
                            try {
                                result += execute_Script(ref addon, ScriptingLanguage, addon.ScriptingCode, addon.ScriptingEntryPoint, EncodeInteger(addon.ScriptingTimeout), "Addon [" + addon.name + "]");
                            } catch (Exception ex) {
                                string addonDescription = getAddonDescription(cpCore, addon);
                                throw new ApplicationException("There was an error executing the script component of Add-on " + addonDescription + ". The details of this error follow.</p><p>" + ex.InnerException.Message + "");
                            }
                        }
                        //
                        // -- DotNet
                        if (addon.DotNetClass != "") {
                            result += execute_Assembly(addon, Models.Entity.AddonCollectionModel.create(cpCore, addon.CollectionID));
                        }
                        //
                        // -- RemoteAssetLink
                        if (addon.RemoteAssetLink != "") {
                            string RemoteAssetLink = addon.RemoteAssetLink;
                            if (RemoteAssetLink.IndexOf("://") < 0) {
                                //
                                // use request object to build link
                                if (RemoteAssetLink.Substring(0, 1) == "/") {
                                    RemoteAssetLink = cpCore.webServer.requestProtocol + cpCore.webServer.requestDomain + RemoteAssetLink;
                                } else {
                                    RemoteAssetLink = cpCore.webServer.requestProtocol + cpCore.webServer.requestDomain + cpCore.webServer.requestVirtualFilePath + RemoteAssetLink;
                                }
                            }
                            int PosStart = 0;
                            httpRequestController kmaHTTP = new httpRequestController();
                            string RemoteAssetContent = kmaHTTP.getURL(RemoteAssetLink);
                            int Pos = genericController.vbInstr(1, RemoteAssetContent, "<body", Microsoft.VisualBasic.Constants.vbTextCompare);
                            if (Pos > 0) {
                                Pos = genericController.vbInstr(Pos, RemoteAssetContent, ">");
                                if (Pos > 0) {
                                    PosStart = Pos + 1;
                                    Pos = genericController.vbInstr(Pos, RemoteAssetContent, "</body", Microsoft.VisualBasic.Constants.vbTextCompare);
                                    if (Pos > 0) {
                                        RemoteAssetContent = RemoteAssetContent.Substring(PosStart - 1, Pos - PosStart);
                                    }
                                }
                            }
                            result += RemoteAssetContent;
                        }
                        //
                        // --  FormXML
                        if (addon.FormXML != "") {
                            bool ExitAddonWithBlankResponse = false;
                            result += execute_FormContent(null, addon.FormXML, ref ExitAddonWithBlankResponse);
                            if (ExitAddonWithBlankResponse) {
                                return string.Empty;
                            }
                        }
                        //
                        // -- Script Callback
                        if (addon.Link != "") {
                            string callBackLink = EncodeAppRootPath(addon.Link, cpCore.webServer.requestVirtualFilePath, requestAppRootPath, cpCore.webServer.requestDomain);
                            foreach (var key in cpCore.docProperties.getKeyList) {
                                callBackLink = modifyLinkQuery(callBackLink, EncodeRequestVariable(key), EncodeRequestVariable(cpCore.docProperties.getText(key)), true);
                            }
                            foreach (var kvp in executeContext.instanceArguments) {
                                callBackLink = modifyLinkQuery(callBackLink, EncodeRequestVariable(kvp.Key), EncodeRequestVariable(cpCore.docProperties.getText(kvp.Value)), true);
                            }
                            result += "<SCRIPT LANGUAGE=\"JAVASCRIPT\" SRC=\"" + callBackLink + "\"></SCRIPT>";
                        }
                        //
                        // -- html assets (js,styles,head tags), set flag to block duplicates 
                        if (!cpCore.doc.addonIdListRunInThisDoc.Contains(addon.id)) {
                            cpCore.doc.addonIdListRunInThisDoc.Add(addon.id);
                            string AddedByName = addon.name + " addon";
                            cpCore.html.addTitle(addon.PageTitle, AddedByName);
                            cpCore.html.addMetaDescription(addon.MetaDescription, AddedByName);
                            cpCore.html.addMetaKeywordList(addon.MetaKeywordList, AddedByName);
                            cpCore.html.addHeadTag(addon.OtherHeadTags, AddedByName);
                            //
                            // -- js head links
                            if (addon.JSHeadScriptSrc != "") {
                                cpCore.html.addScriptLink_Head(addon.JSHeadScriptSrc, AddedByName + " Javascript Head Src");
                            }
                            //
                            // -- js head code
                            if (addon.JSFilename.filename != "") {
                                cpCore.html.addScriptLink_Head(cpCore.webServer.requestProtocol + cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, addon.JSFilename.filename), AddedByName + " Javascript Head Code");
                            }
                            //
                            // -- js body links
                            if (addon.JSBodyScriptSrc != "") {
                                cpCore.html.addScriptLink_Body(addon.JSBodyScriptSrc, AddedByName + " Javascript Body Src");
                            }
                            //
                            // -- js body code
                            cpCore.html.addScriptCode_body(addon.JavaScriptBodyEnd, AddedByName + " Javascript Body Code");
                            //
                            // -- styles
                            if (addon.StylesFilename.filename != "") {
                                cpCore.html.addStyleLink(cpCore.webServer.requestProtocol + cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, addon.StylesFilename.filename), addon.name + " Stylesheet");
                            }
                            //
                            // -- link to stylesheet
                            if (addon.StylesLinkHref != "") {
                                cpCore.html.addStyleLink(addon.StylesLinkHref, addon.name + " Stylesheet Link");
                            }
                        }
                        //
                        // -- Add Css containers
                        if (!string.IsNullOrEmpty(ContainerCssID) | !string.IsNullOrEmpty(ContainerCssClass)) {
                            if (addon.IsInline) {
                                result = "\r" + "<span id=\"" + ContainerCssID + "\" class=\"" + ContainerCssClass + "\" style=\"display:inline;\">" + result + "</span>";
                            } else {
                                result = "\r" + "<div id=\"" + ContainerCssID + "\" class=\"" + ContainerCssClass + "\">" + htmlIndent(result) + "\r" + "</div>";
                            }
                        }
                    }
                    //
                    //   Add Wrappers to content
                    if (addon.InFrame && (executeContext.addonType == CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml)) {
                        //
                        // -- iFrame content, framed in content, during the remote method call, add in the rest of the html page
                        cpCore.doc.setMetaContent(0, 0);
                        result = ""
                            + cpCore.siteProperties.docTypeDeclaration + Environment.NewLine + "<html>"
                            + "\r" + "<head>"
                            + Environment.NewLine + htmlIndent(cpCore.html.getHtmlHead()) + "\r" + "</head>"
                            + '\r' + TemplateDefaultBodyTag + "\r" + "</body>"
                            + Environment.NewLine + "</html>";
                    } else if (addon.AsAjax && (executeContext.addonType == CPUtilsBaseClass.addonContext.ContextRemoteMethodJson)) {
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
                        bool IncludeEditWrapper = (!addon.BlockEditTools) & (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextEditor) & (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextEmail) & (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodJson) & (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) & (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextSimple) & (!executeContext.isIncludeAddon);
                        if (IncludeEditWrapper) {
                            IncludeEditWrapper = IncludeEditWrapper && (allowAdvanceEditor && ((executeContext.addonType == CPUtilsBaseClass.addonContext.ContextAdmin) || cpCore.doc.authContext.isEditing(executeContext.hostRecord.contentName)));
                            if (IncludeEditWrapper) {
                                //
                                // Edit Icon
                                string EditWrapperHTMLID = "eWrapper" + cpCore.doc.pageAddonCnt;
                                string DialogList = string.Empty;
                                string HelpIcon = getHelpBubble(addon.id, addon.Help, addon.CollectionID, DialogList);
                                if (cpCore.visitProperty.getBoolean("AllowAdvancedEditor")) {
                                    string addonArgumentListPassToBubbleEditor = ""; // comes from method in this class the generates it from addon and instance properites - lost it in the shuffle
                                    string AddonEditIcon = GetIconSprite("", 0, "/ccLib/images/tooledit.png", 22, 22, "Edit the " + addon.name + " Add-on", "Edit the " + addon.name + " Add-on", "", true, "");
                                    AddonEditIcon = "<a href=\"" + "/" + cpCore.serverConfig.appConfig.adminRoute + "?cid=" + models.complex.cdefmodel.getcontentid(cpCore, cnAddons) + "&id=" + addon.id + "&af=4&aa=2&ad=1\" tabindex=\"-1\">" + AddonEditIcon + "</a>";
                                    string InstanceSettingsEditIcon = getInstanceBubble(addon.name, addonArgumentListPassToBubbleEditor, executeContext.hostRecord.contentName, executeContext.hostRecord.recordId, executeContext.hostRecord.fieldName, executeContext.instanceGuid, executeContext.addonType, DialogList);
                                    string HTMLViewerEditIcon = getHTMLViewerBubble(addon.id, "editWrapper" + cpCore.doc.editWrapperCnt, DialogList);
                                    string SiteStylesEditIcon = string.Empty; // ?????
                                    string ToolBar = InstanceSettingsEditIcon + AddonEditIcon + getAddonStylesBubble(addon.id, DialogList) + SiteStylesEditIcon + HTMLViewerEditIcon + HelpIcon;
                                    ToolBar = genericController.vbReplace(ToolBar, "&nbsp;", "", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
                                    result = cpCore.html.getEditWrapper("<div class=\"ccAddonEditTools\">" + ToolBar + "&nbsp;" + addon.name + DialogList + "</div>", result);
                                } else if (cpCore.visitProperty.getBoolean("AllowEditing")) {
                                    result = cpCore.html.getEditWrapper("<div class=\"ccAddonEditCaption\">" + addon.name + "&nbsp;" + HelpIcon + "</div>", result);
                                }
                            }
                        }
                        //
                        // -- Add Comment wrapper, to help debugging except email, remote methods and admin (empty is used to detect no result)
                        if (true && (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextAdmin) & (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextEmail) & (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) & (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextRemoteMethodJson) & (executeContext.addonType != CPUtilsBaseClass.addonContext.ContextSimple)) {
                            if (cpCore.visitProperty.getBoolean("AllowDebugging")) {
                                string AddonCommentName = genericController.vbReplace(addon.name, "-->", "..>");
                                if (addon.IsInline) {
                                    result = "<!-- Add-on " + AddonCommentName + " -->" + result + "<!-- /Add-on " + AddonCommentName + " -->";
                                } else {
                                    result = "" + "\r" + "<!-- Add-on " + AddonCommentName + " -->" + htmlIndent(result) + "\r" + "<!-- /Add-on " + AddonCommentName + " -->";
                                }
                            }
                        }
                        //
                        // -- Add Design Wrapper
                        if ((!string.IsNullOrEmpty(result)) & (!addon.IsInline) && (executeContext.wrapperID > 0)) {
                            result = addWrapperToResult(result, executeContext.wrapperID, "for Add-on " + addon.name);
                        }
                    }
                    //
                    // -- this completes the execute of this cpcore.addon. remove it from the 'running' list
                    // -- restore the parent's instanceId
                    cpCore.docProperties.setProperty("instanceId", parentInstanceId);
                    cpCore.doc.addonsCurrentlyRunningIdList.Remove(addon.id);
                    cpCore.doc.pageAddonCnt = cpCore.doc.pageAddonCnt + 1;
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            } finally {
                if (addon != null) {
                    if ((executeContext.forceHtmlDocument) || ((rootLevelAddon) && (addon.htmlDocument))) {
                        result = cpCore.html.getHtmlDoc(result, "<body>"); // "<body class=""ccBodyAdmin ccCon"">"
                    }
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
        private string execute_FormContent(object nothingObject, string FormXML, ref bool return_ExitAddonBlankWithResponse) {
            string result = "";
            try {
                //
                //Const LoginMode_None = 1
                //Const LoginMode_AutoRecognize = 2
                //Const LoginMode_AutoLogin = 3
                int FieldCount = 0;
                int RowMax = 0;
                int ColumnMax = 0;
                int SQLPageSize = 0;
                int ErrorNumber = 0;
                string ErrorDescription = null;
                object[,] something = { { } };
                int RecordID = 0;
                string fieldfilename = null;
                string FieldDataSource = null;
                string FieldSQL = null;
                stringBuilderLegacyController Content = new stringBuilderLegacyController();
                string Copy = null;
                string Button = null;
                adminUIController Adminui = new adminUIController(cpCore);
                string ButtonList = string.Empty;
                string Filename = null;
                string NonEncodedLink = null;
                string EncodedLink = null;
                string VirtualFilePath = null;
                string TabName = null;
                string TabDescription = null;
                string TabHeading = null;
                int TabCnt = 0;
                stringBuilderLegacyController TabCell = null;
                bool loadOK = true;
                string FieldValue = string.Empty;
                string FieldDescription = null;
                string FieldDefaultValue = null;
                bool IsFound = false;
                string Name = string.Empty;
                string Description = string.Empty;
                XmlDocument Doc = new XmlDocument();
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode TabNode = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode SettingNode = null;
                int CS = 0;
                string FieldName = null;
                string FieldCaption = null;
                string FieldAddon = null;
                bool FieldReadOnly = false;
                bool FieldHTML = false;
                string fieldType = null;
                string FieldSelector = null;
                string DefaultFilename = null;
                //
                Button = cpCore.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    //
                    // Cancel just exits with no content
                    //
                    return_ExitAddonBlankWithResponse = true;
                    return string.Empty;
                } else if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                    //
                    // Not Admin Error
                    //
                    ButtonList = ButtonCancel;
                    Content.Add(Adminui.GetFormBodyAdminOnly());
                } else {
                    if (true) {
                        loadOK = true;
                        try {
                            Doc.LoadXml(FormXML);
                        } catch (Exception ex) {
                            ButtonList = ButtonCancel;
                            Content.Add("<div class=\"ccError\" style=\"margin:10px;padding:10px;background-color:white;\">There was a problem with the Setting Page you requested.</div>");
                            loadOK = false;
                        }
                        if (loadOK) {
                            //
                            // data is OK
                            //
                            if (genericController.vbLCase(Doc.DocumentElement.Name) != "form") {
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
                                        switch (genericController.vbLCase(SettingNode.Name)) {
                                            case "tab":
                                                foreach (XmlNode TabNode in SettingNode.ChildNodes) {
                                                    switch (genericController.vbLCase(TabNode.Name)) {
                                                        case "siteproperty":
                                                            //
                                                            FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
                                                            FieldValue = cpCore.docProperties.getText(FieldName);
                                                            fieldType = xml_GetAttribute(IsFound, TabNode, "type", "");
                                                            switch (genericController.vbLCase(fieldType)) {
                                                                case "integer":
                                                                    //
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        FieldValue = genericController.EncodeInteger(FieldValue).ToString();
                                                                    }
                                                                    cpCore.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                case "boolean":
                                                                    //
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        FieldValue = genericController.EncodeBoolean(FieldValue).ToString();
                                                                    }
                                                                    cpCore.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                case "float":
                                                                    //
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        FieldValue = EncodeNumber(FieldValue).ToString();
                                                                    }
                                                                    cpCore.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                case "date":
                                                                    //
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        FieldValue = genericController.EncodeDate(FieldValue).ToString();
                                                                    }
                                                                    cpCore.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                case "file":
                                                                case "imagefile":
                                                                    //
                                                                    if (cpCore.docProperties.getBoolean(FieldName + ".DeleteFlag")) {
                                                                        cpCore.siteProperties.setProperty(FieldName, "");
                                                                    }
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        Filename = FieldValue;
                                                                        VirtualFilePath = "Settings/" + FieldName + "/";
                                                                        cpCore.cdnFiles.upload(FieldName, VirtualFilePath, Filename);
                                                                        cpCore.siteProperties.setProperty(FieldName, VirtualFilePath + Filename);
                                                                    }
                                                                    break;
                                                                case "textfile":
                                                                    //
                                                                    DefaultFilename = "Settings/" + FieldName + ".txt";
                                                                    Filename = cpCore.siteProperties.getText(FieldName, DefaultFilename);
                                                                    if (string.IsNullOrEmpty(Filename)) {
                                                                        Filename = DefaultFilename;
                                                                        cpCore.siteProperties.setProperty(FieldName, DefaultFilename);
                                                                    }
                                                                    cpCore.appRootFiles.saveFile(Filename, FieldValue);
                                                                    break;
                                                                case "cssfile":
                                                                    //
                                                                    DefaultFilename = "Settings/" + FieldName + ".css";
                                                                    Filename = cpCore.siteProperties.getText(FieldName, DefaultFilename);
                                                                    if (string.IsNullOrEmpty(Filename)) {
                                                                        Filename = DefaultFilename;
                                                                        cpCore.siteProperties.setProperty(FieldName, DefaultFilename);
                                                                    }
                                                                    cpCore.appRootFiles.saveFile(Filename, FieldValue);
                                                                    break;
                                                                case "xmlfile":
                                                                    //
                                                                    DefaultFilename = "Settings/" + FieldName + ".xml";
                                                                    Filename = cpCore.siteProperties.getText(FieldName, DefaultFilename);
                                                                    if (string.IsNullOrEmpty(Filename)) {
                                                                        Filename = DefaultFilename;
                                                                        cpCore.siteProperties.setProperty(FieldName, DefaultFilename);
                                                                    }
                                                                    cpCore.appRootFiles.saveFile(Filename, FieldValue);
                                                                    break;
                                                                case "currency":
                                                                    //
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        FieldValue = EncodeNumber(FieldValue).ToString();
                                                                        FieldValue = Microsoft.VisualBasic.Strings.FormatCurrency(FieldValue, -1, Microsoft.VisualBasic.TriState.UseDefault, Microsoft.VisualBasic.TriState.UseDefault, Microsoft.VisualBasic.TriState.UseDefault);
                                                                    }
                                                                    cpCore.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                case "link":
                                                                    cpCore.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                default:
                                                                    cpCore.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                            }
                                                            break;
                                                        case "copycontent":
                                                            //
                                                            // A Copy Content block
                                                            //
                                                            FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
                                                            if (!FieldReadOnly) {
                                                                FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
                                                                FieldHTML = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", "false"));
                                                                if (FieldHTML) {
                                                                    //
                                                                    // treat html as active content for now.
                                                                    //
                                                                    FieldValue = cpCore.docProperties.getRenderedActiveContent(FieldName);
                                                                } else {
                                                                    FieldValue = cpCore.docProperties.getText(FieldName);
                                                                }

                                                                CS = cpCore.db.csOpen("Copy Content", "name=" + cpCore.db.encodeSQLText(FieldName), "ID");
                                                                if (!cpCore.db.csOk(CS)) {
                                                                    cpCore.db.csClose(ref CS);
                                                                    CS = cpCore.db.csInsertRecord("Copy Content", cpCore.doc.authContext.user.id);
                                                                }
                                                                if (cpCore.db.csOk(CS)) {
                                                                    cpCore.db.csSet(CS, "name", FieldName);
                                                                    //
                                                                    // Set copy
                                                                    //
                                                                    cpCore.db.csSet(CS, "copy", FieldValue);
                                                                    //
                                                                    // delete duplicates
                                                                    //
                                                                    cpCore.db.csGoNext(CS);
                                                                    while (cpCore.db.csOk(CS)) {
                                                                        cpCore.db.csDeleteRecord(CS);
                                                                        cpCore.db.csGoNext(CS);
                                                                    }
                                                                }
                                                                cpCore.db.csClose(ref CS);
                                                            }

                                                            break;
                                                        case "filecontent":
                                                            //
                                                            // A File Content block
                                                            //
                                                            FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
                                                            if (!FieldReadOnly) {
                                                                FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
                                                                fieldfilename = xml_GetAttribute(IsFound, TabNode, "filename", "");
                                                                FieldValue = cpCore.docProperties.getText(FieldName);
                                                                cpCore.appRootFiles.saveFile(fieldfilename, FieldValue);
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
                                Content.Add(Adminui.EditTableOpen);
                                Name = xml_GetAttribute(IsFound, Doc.DocumentElement, "name", "");
                                foreach (XmlNode SettingNode in Doc.DocumentElement.ChildNodes) {
                                    switch (genericController.vbLCase(SettingNode.Name)) {
                                        case "description":
                                            Description = SettingNode.InnerText;
                                            break;
                                        case "tab":
                                            TabCnt = TabCnt + 1;
                                            TabName = xml_GetAttribute(IsFound, SettingNode, "name", "");
                                            TabDescription = xml_GetAttribute(IsFound, SettingNode, "description", "");
                                            TabHeading = xml_GetAttribute(IsFound, SettingNode, "heading", "");
                                            if (TabHeading == "Debug and Trace Settings") {
                                                TabHeading = TabHeading;
                                            }
                                            TabCell = new stringBuilderLegacyController();
                                            foreach (XmlNode TabNode in SettingNode.ChildNodes) {
                                                switch (genericController.vbLCase(TabNode.Name)) {
                                                    case "heading":
                                                        //
                                                        // Heading
                                                        //
                                                        FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "");
                                                        TabCell.Add(Adminui.GetEditSubheadRow(FieldCaption));
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
                                                            FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
                                                            FieldHTML = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", ""));
                                                            fieldType = xml_GetAttribute(IsFound, TabNode, "type", "");
                                                            FieldSelector = xml_GetAttribute(IsFound, TabNode, "selector", "");
                                                            FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "");
                                                            FieldAddon = xml_GetAttribute(IsFound, TabNode, "EditorAddon", "");
                                                            FieldDefaultValue = TabNode.InnerText;
                                                            FieldValue = cpCore.siteProperties.getText(FieldName, FieldDefaultValue);
                                                            if (!string.IsNullOrEmpty(FieldAddon)) {
                                                                //
                                                                // Use Editor Addon
                                                                //
                                                                Dictionary<string, string> arguments = new Dictionary<string, string>();
                                                                arguments.Add("FieldName", FieldName);
                                                                arguments.Add("FieldValue", cpCore.siteProperties.getText(FieldName, FieldDefaultValue));
                                                                //OptionString = "FieldName=" & FieldName & "&FieldValue=" & encodeNvaArgument(cpCore.siteProperties.getText(FieldName, FieldDefaultValue))
                                                                addonModel addon = addonModel.createByName(cpCore, FieldAddon);
                                                                Copy = cpCore.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext() {
                                                                    addonType = CPUtilsBaseClass.addonContext.ContextAdmin,
                                                                    instanceArguments = arguments
                                                                });
                                                                //Copy = execute_legacy5(0, FieldAddon, OptionString, CPUtilsBaseClass.addonContext.ContextAdmin, "", 0, "", 0)
                                                            } else if (!string.IsNullOrEmpty(FieldSelector)) {
                                                                //
                                                                // Use Selector
                                                                //
                                                                Copy = getFormContent_decodeSelector(nothingObject, FieldName, FieldValue, FieldSelector);
                                                            } else {
                                                                //
                                                                // Use default editor for each field type
                                                                //
                                                                switch (genericController.vbLCase(fieldType)) {
                                                                    case "integer":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue);
                                                                        }
                                                                        break;
                                                                    case "boolean":
                                                                        if (FieldReadOnly) {
                                                                            Copy = cpCore.html.html_GetFormInputCheckBox2(FieldName, genericController.EncodeBoolean(FieldValue));
                                                                            Copy = genericController.vbReplace(Copy, ">", " disabled>");
                                                                            Copy = Copy + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = cpCore.html.html_GetFormInputCheckBox2(FieldName, genericController.EncodeBoolean(FieldValue));
                                                                        }
                                                                        break;
                                                                    case "float":
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue);
                                                                        }
                                                                        break;
                                                                    case "date":
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = cpCore.html.html_GetFormInputDate(FieldName, FieldValue);
                                                                        }
                                                                        break;
                                                                    case "file":
                                                                    case "imagefile":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            if (string.IsNullOrEmpty(FieldValue)) {
                                                                                Copy = cpCore.html.html_GetFormInputFile(FieldName);
                                                                            } else {
                                                                                NonEncodedLink = cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, FieldValue);
                                                                                EncodedLink = EncodeURL(NonEncodedLink);
                                                                                string FieldValuefilename = "";
                                                                                string FieldValuePath = "";
                                                                                cpCore.privateFiles.splitPathFilename(FieldValue, FieldValuePath, FieldValuefilename);
                                                                                Copy = ""
                                                                                + "<a href=\"http://" + EncodedLink + "\" target=\"_blank\">[" + FieldValuefilename + "]</A>"
                                                                                + "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" + cpCore.html.html_GetFormInputCheckBox2(FieldName + ".DeleteFlag", false) + "&nbsp;&nbsp;&nbsp;Change:&nbsp;" + cpCore.html.html_GetFormInputFile(FieldName);
                                                                            }
                                                                        }
                                                                        //Call s.Add("&nbsp;</span></nobr></td>")
                                                                        break;
                                                                    case "currency":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            if (!string.IsNullOrEmpty(FieldValue)) {
                                                                                FieldValue = Microsoft.VisualBasic.Strings.FormatCurrency(FieldValue, -1, Microsoft.VisualBasic.TriState.UseDefault, Microsoft.VisualBasic.TriState.UseDefault, Microsoft.VisualBasic.TriState.UseDefault);
                                                                            }
                                                                            Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue);
                                                                        }
                                                                        break;
                                                                    case "textfile":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            FieldValue = cpCore.cdnFiles.readFile(FieldValue);
                                                                            if (FieldHTML) {
                                                                                Copy = cpCore.html.getFormInputHTML(FieldName, FieldValue);
                                                                            } else {
                                                                                Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue, 5);
                                                                            }
                                                                        }
                                                                        break;
                                                                    case "cssfile":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue, 5);
                                                                        }
                                                                        break;
                                                                    case "xmlfile":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue, 5);
                                                                        }
                                                                        break;
                                                                    case "link":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue);
                                                                        }
                                                                        break;
                                                                    default:
                                                                        //
                                                                        // text
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            string tmp = cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                            Copy = FieldValue + tmp;
                                                                        } else {
                                                                            if (FieldHTML) {
                                                                                Copy = cpCore.html.getFormInputHTML(FieldName, FieldValue);
                                                                            } else {
                                                                                Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue);
                                                                            }
                                                                        }
                                                                        break;
                                                                }
                                                            }
                                                            TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, false, false, ""));
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
                                                            FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
                                                            FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "");
                                                            FieldHTML = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", ""));
                                                            //
                                                            CS = cpCore.db.csOpen("Copy Content", "Name=" + cpCore.db.encodeSQLText(FieldName), "ID",,,,, "id,name,Copy");
                                                            if (!cpCore.db.csOk(CS)) {
                                                                cpCore.db.csClose(ref CS);
                                                                CS = cpCore.db.csInsertRecord("Copy Content", cpCore.doc.authContext.user.id);
                                                                if (cpCore.db.csOk(CS)) {
                                                                    RecordID = cpCore.db.csGetInteger(CS, "ID");
                                                                    cpCore.db.csSet(CS, "name", FieldName);
                                                                    cpCore.db.csSet(CS, "copy", genericController.encodeText(TabNode.InnerText));
                                                                    cpCore.db.csSave2(CS);
                                                                    // Call cpCore.workflow.publishEdit("Copy Content", RecordID)
                                                                }
                                                            }
                                                            if (cpCore.db.csOk(CS)) {
                                                                FieldValue = cpCore.db.csGetText(CS, "copy");
                                                            }
                                                            if (FieldReadOnly) {
                                                                //
                                                                // Read only
                                                                //
                                                                Copy = FieldValue;
                                                            } else if (FieldHTML) {
                                                                //
                                                                // HTML
                                                                //
                                                                Copy = cpCore.html.getFormInputHTML(FieldName, FieldValue);
                                                                //Copy = cpcore.main_GetFormInputActiveContent( FieldName, FieldValue)
                                                            } else {
                                                                //
                                                                // Text edit
                                                                //
                                                                Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue);
                                                            }
                                                            TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, false, false, ""));
                                                        }
                                                        break;
                                                    case "filecontent":
                                                        //
                                                        // Content from a flat file
                                                        //
                                                        FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
                                                        FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "");
                                                        fieldfilename = xml_GetAttribute(IsFound, TabNode, "filename", "");
                                                        FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
                                                        FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "");
                                                        FieldDefaultValue = TabNode.InnerText;
                                                        Copy = "";
                                                        if (!string.IsNullOrEmpty(fieldfilename)) {
                                                            if (cpCore.appRootFiles.fileExists(fieldfilename)) {
                                                                Copy = FieldDefaultValue;
                                                            } else {
                                                                Copy = cpCore.cdnFiles.readFile(fieldfilename);
                                                            }
                                                            if (!FieldReadOnly) {
                                                                Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, Copy, 10);
                                                            }
                                                        }
                                                        TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, false, false, ""));
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
                                                        SQLPageSize = genericController.EncodeInteger(xml_GetAttribute(IsFound, TabNode, "rowmax", ""));
                                                        if (SQLPageSize == 0) {
                                                            SQLPageSize = 100;
                                                        }
                                                        //
                                                        // Run the SQL
                                                        //
                                                        DataTable dt = null;
                                                        if (!string.IsNullOrEmpty(FieldSQL)) {
                                                            try {
                                                                dt = cpCore.db.executeQuery(FieldSQL, FieldDataSource,, SQLPageSize);
                                                                //RS = app.csv_ExecuteSQLCommand(FieldDataSource, FieldSQL, 30, SQLPageSize, 1)

                                                            } catch (Exception ex) {

                                                                //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                                                                ErrorNumber = Microsoft.VisualBasic.Information.Err().Number;
                                                                //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                                                                ErrorDescription = Microsoft.VisualBasic.Information.Err().Description;
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
                                                                //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                                                                Copy = "Error: " + Microsoft.VisualBasic.Information.Err().Description;
                                                            } else if (!isDataTableOk(dt)) {
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
                                                                    if (dt.Rows.Count == 1 && dt.Columns.Count == 1) {
                                                                        Copy = cpCore.html.html_GetFormInputText2("result", genericController.encodeText(something[0, 0]),,,,, true);
                                                                    } else {
                                                                        foreach (DataRow dr in dt.Rows) {
                                                                            //
                                                                            // Build headers
                                                                            //
                                                                            FieldCount = dr.ItemArray.Count;
                                                                            Copy = Copy + ("\r" + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-bottom:1px solid #444;border-right:1px solid #444;background-color:white;color:#444;\">");
                                                                            Copy = Copy + ("\r" + "\t" + "<tr>");
                                                                            foreach (DataColumn dc in dr.ItemArray) {
                                                                                Copy = Copy + ("\r" + "\t" + "\t" + "<td class=\"ccadminsmall\" style=\"border-top:1px solid #444;border-left:1px solid #444;color:black;padding:2px;padding-top:4px;padding-bottom:4px;\">" + dr(dc).ToString() + "</td>");
                                                                            }
                                                                            Copy = Copy + ("\r" + "\t" + "</tr>");
                                                                            //
                                                                            // Build output table
                                                                            //
                                                                            string RowStart = null;
                                                                            string RowEnd = null;
                                                                            string ColumnStart = null;
                                                                            string ColumnEnd = null;
                                                                            RowStart = "\r" + "\t" + "<tr>";
                                                                            RowEnd = "\r" + "\t" + "</tr>";
                                                                            ColumnStart = "\r" + "\t" + "\t" + "<td class=\"ccadminnormal\" style=\"border-top:1px solid #444;border-left:1px solid #444;background-color:white;color:#444;padding:2px\">";
                                                                            ColumnEnd = "</td>";
                                                                            int RowPointer = 0;
                                                                            for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                                                                                Copy = Copy + (RowStart);
                                                                                int ColumnPointer = 0;
                                                                                for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                                                                    object CellData = something[ColumnPointer, RowPointer];
                                                                                    if (IsNull(CellData)) {
                                                                                        Copy = Copy + (ColumnStart + "[null]" + ColumnEnd);
                                                                                    } else if ((CellData == null)) {
                                                                                        Copy = Copy + (ColumnStart + "[empty]" + ColumnEnd);
                                                                                    } else if (Microsoft.VisualBasic.Information.IsArray(CellData)) {
                                                                                        Copy = Copy + ColumnStart + "[array]";
                                                                                        //Dim Cnt As Integer
                                                                                        //Cnt = UBound(CellData)
                                                                                        //Dim Ptr As Integer
                                                                                        //For Ptr = 0 To Cnt - 1
                                                                                        //    Copy = Copy & ("<br>(" & Ptr & ")&nbsp;[" & CellData[Ptr] & "]")
                                                                                        //Next
                                                                                        //Copy = Copy & (ColumnEnd)
                                                                                    } else if (genericController.encodeText(CellData) == "") {
                                                                                        Copy = Copy + (ColumnStart + "[empty]" + ColumnEnd);
                                                                                    } else {
                                                                                        Copy = Copy + (ColumnStart + genericController.encodeHTML(genericController.encodeText(CellData)) + ColumnEnd);
                                                                                    }
                                                                                }
                                                                                Copy = Copy + (RowEnd);
                                                                            }
                                                                            Copy = Copy + ("\r" + "</table>");
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, false, false, ""));
                                                        break;
                                                }
                                            }
                                            Copy = Adminui.GetEditPanel(true, TabHeading, TabDescription, Adminui.EditTableOpen + TabCell.Text + Adminui.EditTableClose);
                                            if (!string.IsNullOrEmpty(Copy)) {
                                                cpCore.html.main_AddLiveTabEntry(TabName.Replace(" ", "&nbsp;"), Copy, "ccAdminTab");
                                            }
                                            //Content.Add( GetForm_Edit_AddTab(TabName, Copy, True))
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
                                //Content.Add( cpcore.main_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormMobileBrowserControl))
                                //
                                //
                                //
                                if (TabCnt > 0) {
                                    Content.Add(cpCore.html.main_GetLiveTabs());
                                }
                            }
                        }
                    }
                }
                //
                result = Adminui.GetBody(Name, ButtonList, "", true, true, Description, "", 0, Content.Text);
                Content = null;

            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        //   Display field in the admin/edit
        //========================================================================
        //
        private string getFormContent_decodeSelector(object nothingObject, string SitePropertyName, string SitePropertyValue, string selector) {
            string result = "";
            try {
                string ExpandedSelector = "";
                Dictionary<string, string> addonInstanceProperties = new Dictionary<string, string>();
                string OptionCaption = null;
                string OptionValue = null;
                string OptionValue_AddonEncoded = null;
                int OptionPtr = 0;
                int OptionCnt = 0;
                string[] OptionValues = null;
                string OptionSuffix = string.Empty;
                string LCaseOptionDefault = null;
                int Pos = 0;
                stringBuilderLegacyController FastString = null;
                string Copy = string.Empty;
                //
                FastString = new stringBuilderLegacyController();
                //
                Dictionary<string, string> instanceOptions = new Dictionary<string, string>();
                instanceOptions.Add(SitePropertyName, SitePropertyValue);
                buildAddonOptionLists(addonInstanceProperties, ExpandedSelector, SitePropertyName + "=" + selector, instanceOptions, "0", true);
                Pos = genericController.vbInstr(1, ExpandedSelector, "[");
                if (Pos != 0) {
                    //
                    // List of Options, might be select, radio or checkbox
                    //
                    LCaseOptionDefault = genericController.vbLCase(ExpandedSelector.Substring(0, Pos - 1));
                    int PosEqual = genericController.vbInstr(1, LCaseOptionDefault, "=");

                    if (PosEqual > 0) {
                        LCaseOptionDefault = LCaseOptionDefault.Substring(PosEqual);
                    }

                    LCaseOptionDefault = genericController.decodeNvaArgument(LCaseOptionDefault);
                    ExpandedSelector = ExpandedSelector.Substring(Pos);
                    Pos = genericController.vbInstr(1, ExpandedSelector, "]");
                    if (Pos > 0) {
                        if (Pos < ExpandedSelector.Length) {
                            OptionSuffix = genericController.vbLCase((ExpandedSelector.Substring(Pos)).Trim(' '));
                        }
                        ExpandedSelector = ExpandedSelector.Substring(0, Pos - 1);
                    }
                    OptionValues = ExpandedSelector.Split('|');
                    result = "";
                    OptionCnt = OptionValues.GetUpperBound(0) + 1;
                    for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++) {
                        OptionValue_AddonEncoded = OptionValues[OptionPtr].Trim(' ');
                        if (!string.IsNullOrEmpty(OptionValue_AddonEncoded)) {
                            Pos = genericController.vbInstr(1, OptionValue_AddonEncoded, ":");
                            if (Pos == 0) {
                                OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded);
                                OptionCaption = OptionValue;
                            } else {
                                OptionCaption = genericController.decodeNvaArgument(OptionValue_AddonEncoded.Substring(0, Pos - 1));
                                OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded.Substring(Pos));
                            }
                            switch (OptionSuffix) {
                                case "checkbox":
                                    //
                                    // Create checkbox addon_execute_getFormContent_decodeSelector
                                    //
                                    if (genericController.vbInstr(1, "," + LCaseOptionDefault + ",", "," + genericController.vbLCase(OptionValue) + ",") != 0) {
                                        result = result + "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + SitePropertyName + OptionPtr + "\" value=\"" + OptionValue + "\" checked=\"checked\">" + OptionCaption + "</div>";
                                    } else {
                                        result = result + "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + SitePropertyName + OptionPtr + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
                                    }
                                    break;
                                case "radio":
                                    //
                                    // Create Radio addon_execute_getFormContent_decodeSelector
                                    //
                                    if (genericController.vbLCase(OptionValue) == LCaseOptionDefault) {
                                        result = result + "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + SitePropertyName + "\" value=\"" + OptionValue + "\" checked=\"checked\" >" + OptionCaption + "</div>";
                                    } else {
                                        result = result + "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + SitePropertyName + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
                                    }
                                    break;
                                default:
                                    //
                                    // Create select addon_execute_result
                                    //
                                    if (genericController.vbLCase(OptionValue) == LCaseOptionDefault) {
                                        result = result + "<option value=\"" + OptionValue + "\" selected>" + OptionCaption + "</option>";
                                    } else {
                                        result = result + "<option value=\"" + OptionValue + "\">" + OptionCaption + "</option>";
                                    }
                                    break;
                            }
                        }
                    }
                    switch (OptionSuffix) {
                        case "checkbox":
                            //
                            //
                            Copy = Copy + "<input type=\"hidden\" name=\"" + SitePropertyName + "CheckBoxCnt\" value=\"" + OptionCnt + "\" >";
                            break;
                        case "radio":
                            //
                            // Create Radio addon_execute_result
                            //
                            //addon_execute_result = "<div>" & genericController.vbReplace(addon_execute_result, "><", "></div><div><") & "</div>"
                            break;
                        default:
                            //
                            // Create select addon_execute_result
                            //
                            result = "<select name=\"" + SitePropertyName + "\">" + result + "</select>";
                            break;
                    }
                } else {
                    //
                    // Create Text addon_execute_result
                    //

                    selector = genericController.decodeNvaArgument(selector);
                    result = cpCore.html.html_GetFormInputText2(SitePropertyName, selector, 1, 20);
                }

                FastString = null;
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }

        //
        // ================================================================================================================
        /// <summary>
        /// execute the script section of addons. Must be 32-bit. 
        /// </summary>
        /// <param name="Language"></param>
        /// <param name="Code"></param>
        /// <param name="EntryPoint"></param>
        /// <param name="ignore"></param>
        /// <param name="ScriptingTimeout"></param>
        /// <param name="ScriptName"></param>
        /// <param name="ReplaceCnt"></param>
        /// <param name="ReplaceNames"></param>
        /// <param name="ReplaceValues"></param>
        /// <returns></returns>
        /// <remarks>long run, use either csscript.net, or use .net tools to build compile/run funtion</remarks>
        private string execute_Script(ref Models.Entity.addonModel addon, string Language, string Code, string EntryPoint, int ScriptingTimeout, string ScriptName) {
            string returnText = "";
            try {
                string[] Lines = null;
                string[] Args = { };
                string EntryPointArgs = string.Empty;
                //
                string WorkingEntryPoint = EntryPoint;
                string WorkingCode = Code;
                string EntryPointName = WorkingEntryPoint;
                int Pos = genericController.vbInstr(1, EntryPointName, "(");
                if (Pos == 0) {
                    Pos = genericController.vbInstr(1, EntryPointName, " ");
                }
                if (Pos > 1) {
                    EntryPointArgs = EntryPointName.Substring(Pos - 1).Trim(' ');
                    EntryPointName = (EntryPointName.Substring(0, Pos - 1)).Trim(' ');
                    if ((EntryPointArgs.Substring(0, 1) == "(") && (EntryPointArgs.Substring(EntryPointArgs.Length - 1, 1) == ")")) {
                        EntryPointArgs = EntryPointArgs.Substring(1, EntryPointArgs.Length - 2);
                    }
                    Args = SplitDelimited(EntryPointArgs, ",");
                }
                //
                MSScriptControl.ScriptControl sc = new MSScriptControl.ScriptControl();
                try {
                    sc.AllowUI = false;
                    sc.Timeout = ScriptingTimeout;
                    if (!string.IsNullOrEmpty(Language)) {
                        sc.Language = Language;
                    } else {
                        sc.Language = "VBScript";
                    }
                    sc.AddCode(WorkingCode);
                } catch (Exception ex) {
                    string errorMessage = "Error configuring scripting system";
                    if (sc.Error.Number != 0) {
                        errorMessage += ", #" + sc.Error.Number + ", " + sc.Error.Description + ", line " + sc.Error.Line + ", character " + sc.Error.Column;
                        if (sc.Error.Line != 0) {
                            Lines = Microsoft.VisualBasic.Strings.Split(WorkingCode, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
                            if (Lines.GetUpperBound(0) >= sc.Error.Line) {
                                errorMessage += ", code [" + Lines[sc.Error.Line - 1] + "]";
                            }
                        }
                    } else {
                        errorMessage += ", no scripting error";
                    }
                    throw new ApplicationException(errorMessage, ex);
                }
                if (true) {
                    try {
                        mainCsvScriptCompatibilityClass mainCsv = new mainCsvScriptCompatibilityClass(cpCore);
                        sc.AddObject("ccLib", mainCsv);
                    } catch (Exception ex) {
                        //
                        // Error adding cclib object
                        //
                        string errorMessage = "Error adding cclib compatibility object to script environment";
                        if (sc.Error.Number != 0) {
                            errorMessage = errorMessage + ", #" + sc.Error.Number + ", " + sc.Error.Description + ", line " + sc.Error.Line + ", character " + sc.Error.Column;
                            if (sc.Error.Line != 0) {
                                Lines = Microsoft.VisualBasic.Strings.Split(WorkingCode, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
                                if (Lines.GetUpperBound(0) >= sc.Error.Line) {
                                    errorMessage = errorMessage + ", code [" + Lines[sc.Error.Line - 1] + "]";
                                }
                            }
                        } else {
                            errorMessage += ", no scripting error";
                        }
                        throw new ApplicationException(errorMessage, ex);
                    }
                    if (true) {
                        try {
                            sc.AddObject("cp", cpCore.cp_forAddonExecutionOnly);
                        } catch (Exception ex) {
                            //
                            // Error adding cp object
                            //
                            string errorMessage = "Error adding cp object to script environment";
                            if (sc.Error.Number != 0) {
                                errorMessage = errorMessage + ", #" + sc.Error.Number + ", " + sc.Error.Description + ", line " + sc.Error.Line + ", character " + sc.Error.Column;
                                if (sc.Error.Line != 0) {
                                    Lines = Microsoft.VisualBasic.Strings.Split(WorkingCode, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
                                    if (Lines.GetUpperBound(0) >= sc.Error.Line) {
                                        errorMessage = errorMessage + ", code [" + Lines[sc.Error.Line - 1] + "]";
                                    }
                                }
                            } else {
                                errorMessage += ", no scripting error";
                            }
                            string addonDescription = getAddonDescription(cpCore, addon);
                            errorMessage += ", " + addonDescription;
                            throw new ApplicationException(errorMessage, ex);
                        }
                        if (true) {
                            //
                            if (string.IsNullOrEmpty(EntryPointName)) {
                                if (sc.Procedures.Count > 0) {
                                    EntryPointName = sc.Procedures(1).Name;
                                }
                            }
                            try {
                                if (string.IsNullOrEmpty(EntryPointArgs)) {
                                    returnText = genericController.encodeText(sc.Run(EntryPointName));

                                } else {
                                    switch (Args.GetUpperBound(0)) {
                                        case 0:
                                            returnText = genericController.encodeText(sc.Run(EntryPointName, Args[0]));
                                            break;
                                        case 1:
                                            returnText = genericController.encodeText(sc.Run(EntryPointName, Args[0], Args[1]));
                                            break;
                                        case 2:
                                            returnText = genericController.encodeText(sc.Run(EntryPointName, Args[0], Args[1], Args[2]));
                                            break;
                                        case 3:
                                            returnText = genericController.encodeText(sc.Run(EntryPointName, Args[0], Args[1], Args[2], Args[3]));
                                            break;
                                        case 4:
                                            returnText = genericController.encodeText(sc.Run(EntryPointName, Args[0], Args[1], Args[2], Args[3], Args[4]));
                                            break;
                                        case 5:
                                            returnText = genericController.encodeText(sc.Run(EntryPointName, Args[0], Args[1], Args[2], Args[3], Args[4], Args[5]));
                                            break;
                                        case 6:
                                            returnText = genericController.encodeText(sc.Run(EntryPointName, Args[0], Args[1], Args[2], Args[3], Args[4], Args[5], Args[6]));
                                            break;
                                        case 7:
                                            returnText = genericController.encodeText(sc.Run(EntryPointName, Args[0], Args[1], Args[2], Args[3], Args[4], Args[5], Args[6], Args[7]));
                                            break;
                                        case 8:
                                            returnText = genericController.encodeText(sc.Run(EntryPointName, Args[0], Args[1], Args[2], Args[3], Args[4], Args[5], Args[6], Args[7], Args[8]));
                                            break;
                                        case 9:
                                            returnText = genericController.encodeText(sc.Run(EntryPointName, Args[0], Args[1], Args[2], Args[3], Args[4], Args[5], Args[6], Args[7], Args[8], Args[9]));
                                            break;
                                        default:
                                            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError6("csv_ExecuteScript4", "Scripting only supports 10 arguments.")
                                    }
                                }
                            } catch (Exception ex) {
                                string addonDescription = getAddonDescription(cpCore, addon);
                                string errorMessage = "Error executing script [" + ScriptName + "], " + addonDescription;
                                if (sc.Error.Number != 0) {
                                    errorMessage = errorMessage + ", #" + sc.Error.Number + ", " + sc.Error.Description + ", line " + sc.Error.Line + ", character " + sc.Error.Column;
                                    if (sc.Error.Line != 0) {
                                        Lines = Microsoft.VisualBasic.Strings.Split(WorkingCode, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
                                        if (Lines.GetUpperBound(0) >= sc.Error.Line) {
                                            errorMessage = errorMessage + ", code [" + Lines[sc.Error.Line - 1] + "]";
                                        }
                                    }
                                } else {
                                    errorMessage = errorMessage + ", " + GetErrString();
                                }
                                throw new ApplicationException(errorMessage, ex);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnText;
        }
        //
        //
        //
        private string execute_Assembly(Models.Entity.addonModel addon, Models.Entity.AddonCollectionModel addonCollection) {
            string result = "";
            try {
                bool AddonFound = false;
                //
                // -- try appbase folder
                // ***** no -- if we convert to moving addons into the application's private path (wwwroot/bin)...
                // ***** because the addon solution has to be for both web apps and non-web apps running on the server at the same time. so - loadFrom(addon path) is required
                //
                // -- try development bypass folder (addonAssemblyBypass)
                // -- purpose is to provide a path that can be hardcoded in visual studio after-build event to make development easier
                string commonAssemblyPath = cpCore.programDataFiles.rootLocalPath + "AddonAssemblyBypass\\";
                if (!IO.Directory.Exists(commonAssemblyPath)) {
                    IO.Directory.CreateDirectory(commonAssemblyPath);
                } else {
                    result = executeAssembly_byFilePath(addon.id, addon.name, commonAssemblyPath, addon.DotNetClass, true, ref AddonFound);
                }
                if (!AddonFound) {
                    //
                    // -- try app /bin folder
                    // -- purpose is to allow add-ons to be included in the website's (wwwRoot) assembly. So a website's custom addons are within the wwwRoot build, not separate
                    string addonAppRootPath = cpCore.privateFiles.joinPath(cpCore.appRootFiles.rootLocalPath, "bin\\");
                    result = executeAssembly_byFilePath(addon.id, addon.name, addonAppRootPath, addon.DotNetClass, true, ref AddonFound);
                    if (!AddonFound) {
                        //
                        // -- try addon folder
                        // -- purpose is to have a repository where addons can be stored for now web and non-web apps, and allow permissions to be installed with online upload
                        if (string.IsNullOrEmpty(addonCollection.ccguid)) {
                            throw new ApplicationException("The assembly for addon [" + addon.name + "] could not be executed because it's collection has an invalid guid.");
                        } else {
                            string AddonVersionPath = "";
                            addonInstallClass.GetCollectionConfig(cpCore, addonCollection.ccguid, AddonVersionPath, new DateTime(), "");
                            if (string.IsNullOrEmpty(AddonVersionPath)) {
                                throw new ApplicationException("The assembly for addon [" + addon.name + "] could not be executed because it's assembly could not be found in cclibCommonAssemblies, and no collection folder was found.");
                            } else {
                                string AddonPath = cpCore.privateFiles.joinPath(getPrivateFilesAddonPath(), AddonVersionPath);
                                string appAddonPath = cpCore.privateFiles.joinPath(cpCore.privateFiles.rootLocalPath, AddonPath);
                                result = executeAssembly_byFilePath(addon.id, addon.name, appAddonPath, addon.DotNetClass, false, ref AddonFound);
                                if (!AddonFound) {
                                    //
                                    // assembly not found in addon path and in development path, if core collection, try in local /bin nm 
                                    //
                                    if (addonCollection.ccguid != CoreCollectionGuid) {
                                        //
                                        // assembly not found
                                        //
                                        throw new ApplicationException("The addon [" + addon.name + "] could not be executed because it's assembly could not be found in the server common assembly path [" + commonAssemblyPath + "], the application binary folder [" + addonAppRootPath + "], or in the legacy collection folder [" + appAddonPath + "].");
                                    } else {
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
                throw ex;
            }
            return result;
        }
        //
        //==================================================================================================
        //   This is the call from the COM csv code that executes a dot net addon from com.
        //   This is not in the CP BaseClass, because it is used by addons to call back into CP for
        //   services, and they should never call this.
        //==================================================================================================
        //
        private string executeAssembly_byFilePath(int AddonID, string AddonDisplayName, string fullPath, string typeFullName, bool IsDevAssembliesFolder, ref bool AddonFound) {
            string returnValue = "";
            try {
                AddonFound = false;
                if (IO.Directory.Exists(fullPath)) {
                    foreach (var TestFilePathname in IO.Directory.GetFileSystemEntries(fullPath, "*.dll")) {
                        if (!cpCore.assemblySkipList.Contains(TestFilePathname)) {
                            bool testFileIsValidAddonAssembly = true;
                            Assembly testAssembly = null;
                            try {
                                //
                                // ##### consider using refectiononlyload first, then if it is right, do the loadfrom - so Dependencies are not loaded.
                                //
                                testAssembly = System.Reflection.Assembly.LoadFrom(TestFilePathname);
                                //testAssemblyName = testAssembly.FullName
                            } catch (Exception ex) {
                                cpCore.assemblySkipList.Add(TestFilePathname);
                                testFileIsValidAddonAssembly = false;
                            }
                            try {
                                if (testFileIsValidAddonAssembly) {
                                    //
                                    // problem loading types, use try to debug
                                    //
                                    try {
                                        bool isAddonAssembly = false;
                                        //
                                        // -- find type in collection directly
                                        Type addonType = testAssembly.GetType(typeFullName);
                                        if (addonType != null) {
                                            if ((addonType.IsPublic) && (!((addonType.Attributes & TypeAttributes.Abstract) == TypeAttributes.Abstract)) && (addonType.BaseType != null)) {
                                                //
                                                // -- assembly is public, not abstract, based on a base type
                                                if (addonType.BaseType.FullName != null) {
                                                    //
                                                    // -- assembly has a baseType fullname
                                                    if ((addonType.BaseType.FullName.ToLower() == "addonbaseclass") || (addonType.BaseType.FullName.ToLower() == "contensive.baseclasses.addonbaseclass")) {
                                                        //
                                                        // -- valid addon assembly
                                                        isAddonAssembly = true;
                                                        AddonFound = true;
                                                    }
                                                }
                                            }
                                        } else {
                                            //
                                            // -- not found, interate through types to eliminate non-assemblies
                                            // -- consider removing all this, just go with test1
                                            foreach (var testType in testAssembly.GetTypes()) {
                                                //
                                                // Loop through each type in the Assembly looking for our typename, public, and non-abstract
                                                //
                                                if ((testType.IsPublic) & (!((testType.Attributes & TypeAttributes.Abstract) == TypeAttributes.Abstract)) && (testType.BaseType != null)) {
                                                    //
                                                    // -- assembly is public, not abstract, based on a base type
                                                    if (testType.BaseType.FullName != null) {
                                                        //
                                                        // -- assembly has a baseType fullname
                                                        if ((testType.BaseType.FullName.ToLower() == "addonbaseclass") || (testType.BaseType.FullName.ToLower() == "contensive.baseclasses.addonbaseclass")) {
                                                            //
                                                            // -- valid addon assembly
                                                            isAddonAssembly = true;
                                                            if ((testType.FullName.Trim().ToLower() == typeFullName.Trim().ToLower())) {
                                                                addonType = testType;
                                                                AddonFound = true;
                                                                break;
                                                            }
                                                        }
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
                                                    object AddonReturnObj = AddonObj.Execute(cpCore.cp_forAddonExecutionOnly);
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
                                                    string detailedErrorMessage = "There was an error in the addon [" + AddonDisplayName + "]. It could not be executed because there was an error in the addon assembly [" + TestFilePathname + "], in class [" + addonType.FullName.Trim().ToLower() + "]. The error was [" + Ex.ToString() + "]";
                                                    cpCore.handleException(Ex, detailedErrorMessage);
                                                    //Throw New ApplicationException(detailedErrorMessage)
                                                }
                                            } catch (Exception Ex) {
                                                string detailedErrorMessage = AddonDisplayName + " could not be executed because there was an error creating an object from the assembly, DLL [" + addonType.FullName + "]. The error was [" + Ex.ToString() + "]";
                                                throw new ApplicationException(detailedErrorMessage);
                                            }
                                            //
                                            // -- addon was found, no need to look for more
                                            break;
                                        }
                                        if (!isAddonAssembly) {
                                            //
                                            // -- not an addon assembly
                                            cpCore.assemblySkipList.Add(TestFilePathname);
                                        }
                                    } catch (ReflectionTypeLoadException ex) {
                                        //
                                        // exceptin thrown out of application bin folder when xunit library included -- ignore
                                        //
                                        cpCore.assemblySkipList.Add(TestFilePathname);
                                    } catch (Exception ex) {
                                        //
                                        // problem loading types
                                        //
                                        cpCore.assemblySkipList.Add(TestFilePathname);
                                        string detailedErrorMessage = "While locating assembly for addon [" + AddonDisplayName + "], there was an error loading types for assembly [" + TestFilePathname + "]. This assembly was skipped and should be removed from the folder [" + fullPath + "]";
                                        throw new ApplicationException(detailedErrorMessage);
                                    }
                                }
                            } catch (Reflection.ReflectionTypeLoadException ex) {
                                cpCore.assemblySkipList.Add(TestFilePathname);
                                string detailedErrorMessage = "A load exception occured for addon [" + AddonDisplayName + "], DLL [" + TestFilePathname + "]. The error was [" + ex.ToString() + "] Any internal exception follow:";
                                foreach (Exception exLoader in ex.LoaderExceptions) {
                                    detailedErrorMessage += Environment.NewLine + "--LoaderExceptions: " + exLoader.Message;
                                }
                                throw new ApplicationException(detailedErrorMessage);
                            } catch (Exception ex) {
                                //
                                // ignore these errors
                                //
                                cpCore.assemblySkipList.Add(TestFilePathname);
                                string detailedErrorMessage = "A non-load exception occured while loading the addon [" + AddonDisplayName + "], DLL [" + TestFilePathname + "]. The error was [" + ex.ToString() + "].";
                                cpCore.handleException(new ApplicationException(detailedErrorMessage));
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                //
                // -- this exception should interrupt the caller
                cpCore.handleException(ex);
                throw;
            }
            return returnValue;
        }
        //'
        //' 
        //'
        //Public Function csv_ExecuteActiveX(ByVal ProgramID As String, ByVal AddonCaption As String, ByVal OptionString_ForObjectCall As String, ByVal OptionStringForDisplay As String, ByRef return_AddonErrorMessage As String) As String
        //    Dim exMsg As String = "activex addons [" & ProgramID & "] are no longer supported"
        //    handleException(New ApplicationException(exMsg))
        //    Return exMsg
        //End Function
        //
        //====================================================================================================================
        //   Execte an Addon as a process
        //
        //   OptionString
        //       can be & delimited or crlf delimited
        //       must be addonencoded with call encodeNvaArgument
        //
        //   nothingObject
        //       cp should be set during csv_OpenConnection3 -- do not pass it around in the arguments
        //
        //   WaitForReturn
        //       if true, this routine calls the addon
        //       if false, the server is called remotely, which starts a cccmd process, gets the command and calls this routine with true
        //====================================================================================================================
        //
        public string executeAddonAsProcess(string AddonIDGuidOrName, string OptionString = "") {
            string result = "";
            try {
                Models.Entity.addonModel addon = null;
                if (EncodeInteger(AddonIDGuidOrName) > 0) {
                    addon = cpCore.addonCache.getAddonById(EncodeInteger(AddonIDGuidOrName));
                } else if (genericController.isGuid(AddonIDGuidOrName)) {
                    addon = cpCore.addonCache.getAddonByGuid(AddonIDGuidOrName);
                } else {
                    addon = cpCore.addonCache.getAddonByName(AddonIDGuidOrName);
                }
                if (addon != null) {
                    //
                    // -- addon found
                    //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                    logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "start: add process to background cmd queue, addon [" + addon.name + "/" + addon.id + "], optionstring [" + OptionString + "]", "dll", "cpCoreClass", "csv_ExecuteAddonAsProcess", Microsoft.VisualBasic.Information.Err().Number, Microsoft.VisualBasic.Information.Err().Source, Microsoft.VisualBasic.Information.Err().Description, false, true, "", "process", "");
                    //
                    string cmdQueryString = ""
                        + "appname=" + encodeNvaArgument(EncodeRequestVariable(cpCore.serverConfig.appConfig.name)) + "&AddonID=" + Convert.ToString(addon.id) + "&OptionString=" + encodeNvaArgument(EncodeRequestVariable(OptionString));
                    taskSchedulerController taskScheduler = new taskSchedulerController();
                    cmdDetailClass cmdDetail = new cmdDetailClass();
                    cmdDetail.addonId = addon.id;
                    cmdDetail.addonName = addon.name;
                    cmdDetail.docProperties = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, cmdQueryString);
                    taskScheduler.addTaskToQueue(cpCore, taskQueueCommandEnumModule.runAddon, cmdDetail, false);
                    //
                    //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                    logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "end: add process to background cmd queue, addon [" + addon.name + "/" + addon.id + "], optionstring [" + OptionString + "]", "dll", "cpCoreClass", "csv_ExecuteAddonAsProcess", Microsoft.VisualBasic.Information.Err().Number, Microsoft.VisualBasic.Information.Err().Source, Microsoft.VisualBasic.Information.Err().Description, false, true, "", "process", "");
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //===============================================================================================================================================
        //   cpcore.main_Get the editable options bubble
        //       ACInstanceID required
        //       ACInstanceID = -1 means this Add-on does not support instance options (like end-of-page scope, etc)
        // REFACTOR - unify interface, remove cpcore.main_ and csv_ class references
        //===============================================================================================================================================
        //
        public string getInstanceBubble(string AddonName, string Option_String, string ContentName, int RecordID, string FieldName, string ACInstanceID, CPUtilsBaseClass.addonContext Context, ref string return_DialogList) {
            string tempgetInstanceBubble = null;
            try {
                //
                string Dialog = null;
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
                string LocalCode = string.Empty;
                string CopyHeader = string.Empty;
                string CopyContent = string.Empty;
                string BubbleJS = null;
                string[] OptionSplit = null;
                string OptionName = null;
                string OptionSelector = null;
                int Ptr = 0;
                int Pos = 0;
                //
                if (cpCore.doc.authContext.isAuthenticated & ((ACInstanceID == "-2") || (ACInstanceID == "-1") || (ACInstanceID == "0") || (RecordID != 0))) {
                    if (cpCore.doc.authContext.isEditingAnything()) {
                        CopyHeader = CopyHeader + "<div class=\"ccHeaderCon\">"
                            + "<table border=0 cellpadding=0 cellspacing=0 width=\"100%\">"
                            + "<tr>"
                            + "<td align=left class=\"bbLeft\">Options for this instance of " + AddonName + "</td>"
                            + "<td align=right class=\"bbRight\"><a href=\"#\" onClick=\"HelpBubbleOff('HelpBubble" + cpCore.doc.helpCodeCount + "');return false;\"><img alt=\"close\" src=\"/ccLib/images/ClosexRev1313.gif\" width=13 height=13 border=0></a></td>"
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
                            //ElseIf (Context <> CPUtilsBaseClass.addonContext.ContextAdmin) And (cpCore.siteProperties.allowWorkflowAuthoring And Not cpCore.visitProperty.getBoolean("AllowWorkflowRendering")) Then
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
                            OptionSplit = Microsoft.VisualBasic.Strings.Split(Option_String, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
                            for (Ptr = 0; Ptr <= OptionSplit.GetUpperBound(0); Ptr++) {
                                //
                                // Process each option row
                                //
                                OptionName = OptionSplit[Ptr];
                                OptionSuffix = "";
                                OptionDefault = "";
                                LCaseOptionDefault = "";
                                OptionSelector = "";
                                Pos = genericController.vbInstr(1, OptionName, "=");
                                if (Pos != 0) {
                                    if (Pos < OptionName.Length) {
                                        OptionSelector = (OptionName.Substring(Pos)).Trim(' ');
                                    }
                                    OptionName = (OptionName.Substring(0, Pos - 1)).Trim(' ');
                                }
                                OptionName = genericController.decodeNvaArgument(OptionName);
                                Pos = genericController.vbInstr(1, OptionSelector, "[");
                                if (Pos != 0) {
                                    //
                                    // List of Options, might be select, radio, checkbox, resourcelink
                                    //
                                    OptionDefault = OptionSelector.Substring(0, Pos - 1);
                                    OptionDefault = genericController.decodeNvaArgument(OptionDefault);
                                    LCaseOptionDefault = genericController.vbLCase(OptionDefault);
                                    //LCaseOptionDefault = genericController.decodeNvaArgument(LCaseOptionDefault)

                                    OptionSelector = OptionSelector.Substring(Pos);
                                    Pos = genericController.vbInstr(1, OptionSelector, "]");
                                    if (Pos > 0) {
                                        if (Pos < OptionSelector.Length) {
                                            OptionSuffix = genericController.vbLCase((OptionSelector.Substring(Pos)).Trim(' '));
                                        }
                                        OptionSelector = OptionSelector.Substring(0, Pos - 1);
                                    }
                                    OptionValues = OptionSelector.Split('|');
                                    FormInput = "";
                                    OptionCnt = OptionValues.GetUpperBound(0) + 1;
                                    for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++) {
                                        OptionValue_AddonEncoded = OptionValues[OptionPtr].Trim(' ');
                                        if (!string.IsNullOrEmpty(OptionValue_AddonEncoded)) {
                                            Pos = genericController.vbInstr(1, OptionValue_AddonEncoded, ":");
                                            if (Pos == 0) {
                                                OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded);
                                                OptionCaption = OptionValue;
                                            } else {
                                                OptionCaption = genericController.decodeNvaArgument(OptionValue_AddonEncoded.Substring(0, Pos - 1));
                                                OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded.Substring(Pos));
                                            }
                                            switch (OptionSuffix) {
                                                case "checkbox":
                                                    //
                                                    // Create checkbox FormInput
                                                    //
                                                    if (genericController.vbInstr(1, "," + LCaseOptionDefault + ",", "," + genericController.vbLCase(OptionValue) + ",") != 0) {
                                                        FormInput = FormInput + "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + OptionName + OptionPtr + "\" value=\"" + OptionValue + "\" checked=\"checked\">" + OptionCaption + "</div>";
                                                    } else {
                                                        FormInput = FormInput + "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + OptionName + OptionPtr + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
                                                    }
                                                    break;
                                                case "radio":
                                                    //
                                                    // Create Radio FormInput
                                                    //
                                                    if (genericController.vbLCase(OptionValue) == LCaseOptionDefault) {
                                                        FormInput = FormInput + "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + OptionName + "\" value=\"" + OptionValue + "\" checked=\"checked\" >" + OptionCaption + "</div>";
                                                    } else {
                                                        FormInput = FormInput + "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + OptionName + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
                                                    }
                                                    break;
                                                default:
                                                    //
                                                    // Create select FormInput
                                                    //
                                                    if (genericController.vbLCase(OptionValue) == LCaseOptionDefault) {
                                                        FormInput = FormInput + "<option value=\"" + OptionValue + "\" selected>" + OptionCaption + "</option>";
                                                    } else {
                                                        OptionCaption = genericController.vbReplace(OptionCaption, Environment.NewLine, " ");
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
                                        //                                    & cpcore.main_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80, FormFieldLCaseName) _
                                        //                                    & "&nbsp;<a href=""#"" onClick=""OpenResourceLinkWindow( '" & FormFieldLCaseName & "' ) ;return false;""><img src=""/ccLib/images/ResourceLink1616.gif"" width=16 height=16 border=0 alt=""Link to a resource"" title=""Link to a resource""></a>" _
                                        //                                    & "&nbsp;<a href=""#"" onClick=""OpenSiteExplorerWindow( '" & FormFieldLCaseName & "' ) ;return false;""><img src=""/ccLib/images/PageLink1616.gif"" width=16 height=16 border=0 alt=""Link to a page"" title=""Link to a page""></a>"
                                        //                                s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & "</span></nobr></td>")
                                        //                            Case FieldTypeResourceLink
                                        //                                '
                                        //                                ' ----- Resource Link (src value)
                                        //                                '
                                        //                                Return_NewFieldList = Return_NewFieldList & "," & FieldName
                                        //                                FieldValueText = genericController.encodeText(FieldValueVariant)
                                        //                                EditorString = "" _
                                        //                                    & cpcore.main_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80, FormFieldLCaseName) _
                                        //                                    & "&nbsp;<a href=""#"" onClick=""OpenResourceLinkWindow( '" & FormFieldLCaseName & "' ) ;return false;""><img src=""/ccLib/images/ResourceLink1616.gif"" width=16 height=16 border=0 alt=""Link to a resource"" title=""Link to a resource""></a>"
                                        //                                'EditorString = cpcore.main_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80)
                                        //                                s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & "</span></nobr></td>")
                                        case "resourcelink":
                                            //
                                            // Create text box linked to resource library
                                            //
                                            OptionDefault = genericController.decodeNvaArgument(OptionDefault);
                                            FormInput = ""
                                                + cpCore.html.html_GetFormInputText2(OptionName, OptionDefault, 1, 20) + "&nbsp;<a href=\"#\" onClick=\"OpenResourceLinkWindow( '" + OptionName + "' ) ;return false;\"><img src=\"/ccLib/images/ResourceLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a resource\" title=\"Link to a resource\"></a>";
                                            //EditorString = cpcore.main_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80)
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

                                    OptionSelector = genericController.decodeNvaArgument(OptionSelector);
                                    FormInput = cpCore.html.html_GetFormInputText2(OptionName, OptionSelector, 1, 20);
                                }
                                CopyContent = CopyContent + "<tr>"
                                    + "<td class=\"bbLeft\">" + OptionName + "</td>"
                                    + "<td class=\"bbRight\">" + FormInput + "</td>"
                                    + "</tr>";
                            }
                            CopyContent = ""
                                + CopyContent + "</table>"
                                + cpCore.html.html_GetFormInputHidden("Type", FormTypeAddonSettingsEditor) + cpCore.html.html_GetFormInputHidden("ContentName", ContentName) + cpCore.html.html_GetFormInputHidden("RecordID", RecordID) + cpCore.html.html_GetFormInputHidden("FieldName", FieldName) + cpCore.html.html_GetFormInputHidden("ACInstanceID", ACInstanceID);
                        }
                        //
                        BubbleJS = " onClick=\"HelpBubbleOn( 'HelpBubble" + cpCore.doc.helpCodeCount + "',this);return false;\"";
                        QueryString = cpCore.doc.refreshQueryString;
                        QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", false);
                        //QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
                        return_DialogList = return_DialogList + "<div class=\"ccCon helpDialogCon\">"
                            + cpCore.html.html_GetUploadFormStart() + "<table border=0 cellpadding=0 cellspacing=0 class=\"ccBubbleCon\" id=\"HelpBubble" + cpCore.doc.helpCodeCount + "\" style=\"display:none;visibility:hidden;\">"
                            + "<tr><td class=\"ccHeaderCon\">" + CopyHeader + "</td></tr>"
                            + "<tr><td class=\"ccButtonCon\">" + cpCore.html.html_GetFormButton("Update", "HelpBubbleButton") + "</td></tr>"
                            + "<tr><td class=\"ccContentCon\">" + CopyContent + "</td></tr>"
                            + "</table>"
                            + "</form>"
                            + "</div>";
                        tempgetInstanceBubble = ""
                            + "&nbsp;<a href=\"#\" tabindex=-1 target=\"_blank\"" + BubbleJS + ">"
                            + GetIconSprite("", 0, "/ccLib/images/toolsettings.png", 22, 22, "Edit options used just for this instance of the " + AddonName + " Add-on", "Edit options used just for this instance of the " + AddonName + " Add-on", "", true, "") + "</a>"
                            + ""
                            + "";
                        cpCore.doc.helpCodes.Add( new docController.helpStuff() {
                            caption = AddonName,
                            code = LocalCode
                        });
                        if (cpCore.doc.helpDialogCnt == 0) {
                            cpCore.html.addScriptCode_onLoad("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs");
                        }
                        cpCore.doc.helpDialogCnt = cpCore.doc.helpDialogCnt + 1;
                    }
                }
                //
                return tempgetInstanceBubble;
            } catch (Exception ex) {
                cpCore.handleException( ex );
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("addon_execute_GetInstanceBubble")
            return tempgetInstanceBubble;
        }
        //
        //===============================================================================================================================================
        //   cpcore.main_Get Addon Styles Bubble Editor
        //===============================================================================================================================================
        //
        public string getAddonStylesBubble(int addonId, ref string return_DialogList) {
            string result = string.Empty;
            try {
                //Dim DefaultStylesheet As String = String.Empty
                //Dim StyleSheet As String = String.Empty
                string QueryString = null;
                string LocalCode = string.Empty;
                string CopyHeader = string.Empty;
                string CopyContent = null;
                string BubbleJS = null;
                //Dim AddonName As String = String.Empty
                //
                if (cpCore.doc.authContext.isAuthenticated && true) {
                    if (cpCore.doc.authContext.isEditingAnything()) {
                        Models.Entity.addonModel addon = Models.Entity.addonModel.create(cpCore, addonId);
                        CopyHeader = CopyHeader + "<div class=\"ccHeaderCon\">"
                            + "<table border=0 cellpadding=0 cellspacing=0 width=\"100%\">"
                            + "<tr>"
                            + "<td align=left class=\"bbLeft\">Stylesheet for " + addon.name + "</td>"
                            + "<td align=right class=\"bbRight\"><a href=\"#\" onClick=\"HelpBubbleOff('HelpBubble" + cpCore.doc.helpCodeCount + "');return false;\"><img alt=\"close\" src=\"/ccLib/images/ClosexRev1313.gif\" width=13 height=13 border=0></a></td>"
                            + "</tr>"
                            + "</table>"
                            + "</div>";
                        CopyContent = ""
                            + ""
                            + "<table border=0 cellpadding=5 cellspacing=0 width=\"100%\">"
                            + "<tr><td style=\"width:400px;background-color:transparent;\" class=\"ccContentCon ccAdminSmall\">These stylesheets will be added to all pages that include this add-on. The default stylesheet comes with the add-on, and can not be edited.</td></tr>"
                            + "<tr><td style=\"padding-bottom:5px;\" class=\"ccContentCon ccAdminSmall\"><b>Custom Stylesheet</b>" + cpCore.html.html_GetFormInputTextExpandable2("CustomStyles", addon.StylesFilename.content, 10, "400px") + "</td></tr>";
                        //If DefaultStylesheet = "" Then
                        //    CopyContent = CopyContent & "<tr><td style=""padding-bottom:5px;"" class=""ccContentCon ccAdminSmall""><b>Default Stylesheet</b><br>There are no default styles for this add-on.</td></tr>"
                        //Else
                        //    CopyContent = CopyContent & "<tr><td style=""padding-bottom:5px;"" class=""ccContentCon ccAdminSmall""><b>Default Stylesheet</b><br>" & cpCore.html.html_GetFormInputTextExpandable2("DefaultStyles", DefaultStylesheet, 10, "400px", , , True) & "</td></tr>"
                        //End If
                        CopyContent = ""
                        + CopyContent + "</tr>"
                        + "</table>"
                        + cpCore.html.html_GetFormInputHidden("Type", FormTypeAddonStyleEditor) + cpCore.html.html_GetFormInputHidden("AddonID", addonId) + "";
                        //
                        BubbleJS = " onClick=\"HelpBubbleOn( 'HelpBubble" + cpCore.doc.helpCodeCount + "',this);return false;\"";
                        QueryString = cpCore.doc.refreshQueryString;
                        QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", false);
                        //QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
                        string Dialog = string.Empty;

                        Dialog = Dialog + "<div class=\"ccCon helpDialogCon\">"
                            + cpCore.html.html_GetUploadFormStart() + "<table border=0 cellpadding=0 cellspacing=0 class=\"ccBubbleCon\" id=\"HelpBubble" + cpCore.doc.helpCodeCount + "\" style=\"display:none;visibility:hidden;\">"
                            + "<tr><td class=\"ccHeaderCon\">" + CopyHeader + "</td></tr>"
                            + "<tr><td class=\"ccButtonCon\">" + cpCore.html.html_GetFormButton("Update", "HelpBubbleButton") + "</td></tr>"
                            + "<tr><td class=\"ccContentCon\">" + CopyContent + "</td></tr>"
                            + "</table>"
                            + "</form>"
                            + "</div>";
                        return_DialogList = return_DialogList + Dialog;
                        result = ""
                            + "&nbsp;<a href=\"#\" tabindex=-1 target=\"_blank\"" + BubbleJS + ">"
                            + GetIconSprite("", 0, "/ccLib/images/toolstyles.png", 22, 22, "Edit " + addon.name + " Stylesheets", "Edit " + addon.name + " Stylesheets", "", true, "") + "</a>";
                        if (cpCore.doc.helpCodeCount >= cpCore.doc.helpCodeSize) {
                            cpCore.doc.helpCodeSize = cpCore.doc.helpCodeSize + 10;
                            //INSTANT C# TODO TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
                            Array.Resize( ref asdf,asdf) // redim preserve  cpCore.doc.helpCodes(cpCore.doc.helpCodeSize);
                            //INSTANT C# TODO TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
                            Array.Resize( ref asdf,asdf) // redim preserve  cpCore.doc.helpCaptions(cpCore.doc.helpCodeSize);
                        }
                        cpCore.doc.helpCodes[cpCore.doc.helpCodeCount] = LocalCode;
                        cpCore.doc.helpCaptions[cpCore.doc.helpCodeCount] = addon.name;
                        cpCore.doc.helpCodeCount = cpCore.doc.helpCodeCount + 1;
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //===============================================================================================================================================
        //   cpcore.main_Get inner HTML viewer Bubble
        //===============================================================================================================================================
        //

        public string getHelpBubble(int addonId, string helpCopy, int CollectionID, ref string return_DialogList) {
            string result = "";
            string QueryString = null;
            string LocalCode = string.Empty;
            string CopyContent = null;
            string BubbleJS = null;
            string AddonName = string.Empty;
            int StyleSN = 0;
            string InnerCopy = null;
            string CollectionCopy = string.Empty;
            //
            if (cpCore.doc.authContext.isAuthenticated) {
                if (cpCore.doc.authContext.isEditingAnything()) {
                    StyleSN = genericController.EncodeInteger(cpCore.siteProperties.getText("StylesheetSerialNumber", "0"));
                    //cpCore.html.html_HelpViewerButtonID = "HelpBubble" & doccontroller.htmlDoc_HelpCodeCount
                    InnerCopy = helpCopy;
                    if (string.IsNullOrEmpty(InnerCopy)) {
                        InnerCopy = "<p style=\"text-align:center\">No help is available for this add-on.</p>";
                    }
                    //
                    if (CollectionID != 0) {
                        CollectionCopy = cpCore.db.getRecordName("Add-on Collections", CollectionID);
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
                        + "<td align=right class=\"bbRight\"><a href=\"#\" onClick=\"HelpBubbleOff('HelpBubble" + cpCore.doc.helpCodeCount + "');return false;\"><img alt=\"close\" src=\"/ccLib/images/ClosexRev1313.gif\" width=13 height=13 border=0></a></td>"
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
                    QueryString = cpCore.doc.refreshQueryString;
                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", false);
                    //QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
                    return_DialogList = return_DialogList + "<div class=\"ccCon helpDialogCon\">"
                        + "<table border=0 cellpadding=0 cellspacing=0 class=\"ccBubbleCon\" id=\"HelpBubble" + cpCore.doc.helpCodeCount + "\" style=\"display:none;visibility:hidden;\">"
                        + "<tr><td class=\"ccHeaderCon\">" + CopyHeader + "</td></tr>"
                        + "<tr><td class=\"ccContentCon\">" + CopyContent + "</td></tr>"
                        + "</table>"
                        + "</div>";
                    BubbleJS = " onClick=\"HelpBubbleOn( 'HelpBubble" + cpCore.doc.helpCodes.Count + "',this);return false;\"";
                    cpCore.doc.helpCodes.Add(new docController.helpStuff {
                        code = LocalCode,
                        caption = AddonName
                    });
                    //
                    if (cpCore.doc.helpDialogCnt == 0) {
                        cpCore.html.addScriptCode_onLoad("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs");
                    }
                    cpCore.doc.helpDialogCnt = cpCore.doc.helpDialogCnt + 1;
                    result = ""
                        + "&nbsp;<a href=\"#\" tabindex=-1 tarGet=\"_blank\"" + BubbleJS + " >"
                        + GetIconSprite("", 0, "/ccLib/images/toolhelp.png", 22, 22, "View help resources for this Add-on", "View help resources for this Add-on", "", true, "") + "</a>";
                }
            }
            return result;
        }
        //
        //===============================================================================================================================================
        //   cpcore.main_Get inner HTML viewer Bubble
        //===============================================================================================================================================
        //
        public string getHTMLViewerBubble(int addonId, string HTMLSourceID, ref string return_DialogList) {
            string tempgetHTMLViewerBubble = null;
            try {
                //
                string DefaultStylesheet = null;
                string StyleSheet = null;
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
                string LocalCode = string.Empty;
                string CopyHeader = string.Empty;
                string CopyContent = null;
                string BubbleJS = null;
                string[] OptionSplit = null;
                string OptionName = null;
                string OptionSelector = null;
                int Ptr = 0;
                int Pos = 0;
                int CS = 0;
                string AddonName = string.Empty;
                int StyleSN = 0;
                string HTMLViewerBubbleID = null;
                //
                if (cpCore.doc.authContext.isAuthenticated) {
                    if (cpCore.doc.authContext.isEditingAnything()) {
                        StyleSN = genericController.EncodeInteger(cpCore.siteProperties.getText("StylesheetSerialNumber", "0"));
                        HTMLViewerBubbleID = "HelpBubble" + cpCore.doc.helpCodeCount;
                        //
                        CopyHeader = CopyHeader + "<div class=\"ccHeaderCon\">"
                            + "<table border=0 cellpadding=0 cellspacing=0 width=\"100%\">"
                            + "<tr>"
                            + "<td align=left class=\"bbLeft\">HTML viewer</td>"
                            + "<td align=right class=\"bbRight\"><a href=\"#\" onClick=\"HelpBubbleOff('" + HTMLViewerBubbleID + "');return false;\"><img alt=\"close\" src=\"/ccLib/images/ClosexRev1313.gif\" width=13 height=13 border=0></A></td>"
                            + "</tr>"
                            + "</table>"
                            + "</div>";
                        CopyContent = ""
                            + "<table border=0 cellpadding=5 cellspacing=0 width=\"100%\">"
                            + "<tr><td style=\"width:400px;background-color:transparent;\" class=\"ccAdminSmall\">This is the HTML produced by this add-on. Carrage returns and tabs have been added or modified to enhance readability.</td></tr>"
                            + "<tr><td style=\"width:400px;background-color:transparent;\" class=\"ccAdminSmall\">" + cpCore.html.html_GetFormInputTextExpandable2("DefaultStyles", "", 10, "400px", HTMLViewerBubbleID + "_dst",, false) + "</td></tr>"
                            + "</tr>"
                            + "</table>"
                            + "";
                        //
                        QueryString = cpCore.doc.refreshQueryString;
                        QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", false);
                        //QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
                        return_DialogList = return_DialogList + "<div class=\"ccCon helpDialogCon\">"
                            + "<table border=0 cellpadding=0 cellspacing=0 class=\"ccBubbleCon\" id=\"" + HTMLViewerBubbleID + "\" style=\"display:none;visibility:hidden;\">"
                            + "<tr><td class=\"ccHeaderCon\">" + CopyHeader + "</td></tr>"
                            + "<tr><td class=\"ccContentCon\">" + CopyContent + "</td></tr>"
                            + "</table>"
                            + "</div>";
                        BubbleJS = " onClick=\"var d=document.getElementById('" + HTMLViewerBubbleID + "_dst');if(d){var s=document.getElementById('" + HTMLSourceID + "');if(s){d.value=s.innerHTML;HelpBubbleOn( '" + HTMLViewerBubbleID + "',this)}};return false;\" ";
                        cpCore.doc.helpCodes.Add(new docController.helpStuff {
                            code = LocalCode,
                            caption = AddonName
                        });
                        //
                        if (cpCore.doc.helpDialogCnt == 0) {
                            cpCore.html.addScriptCode_onLoad("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs");
                        }
                        cpCore.doc.helpDialogCnt = cpCore.doc.helpDialogCnt + 1;
                        tempgetHTMLViewerBubble = ""
                            + "&nbsp;<a href=\"#\" tabindex=-1 target=\"_blank\"" + BubbleJS + " >"
                            + GetIconSprite("", 0, "/ccLib/images/toolhtml.png", 22, 22, "View the source HTML produced by this Add-on", "View the source HTML produced by this Add-on", "", true, "") + "</A>";
                    }
                }
                //
                return tempgetHTMLViewerBubble;
            } catch (Exception ex) {
                cpCore.handleException( ex );
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("addon_execute_GetHTMLViewerBubble")
            return tempgetHTMLViewerBubble;
        }
        //
        //
        //
        private string getFormContent(string FormXML, ref bool return_ExitRequest) {
            string tempgetFormContent = null;
            string result = "";
            try {
                int FieldCount = 0;
                int RowMax = 0;
                int ColumnMax = 0;
                int SQLPageSize = 0;
                int ErrorNumber = 0;
                string ErrorDescription = null;
                string[,] dataArray = null;
                int RecordID = 0;
                string fieldfilename = null;
                string FieldDataSource = null;
                string FieldSQL = null;
                stringBuilderLegacyController Content = new stringBuilderLegacyController();
                string Copy = null;
                string Button = null;
                adminUIController Adminui = new adminUIController(cpCore);
                string ButtonList = string.Empty;
                string Filename = null;
                string NonEncodedLink = null;
                string EncodedLink = null;
                string VirtualFilePath = null;
                string TabName = null;
                string TabDescription = null;
                string TabHeading = null;
                int TabCnt = 0;
                stringBuilderLegacyController TabCell = null;
                string FieldValue = string.Empty;
                string FieldDescription = null;
                string FieldDefaultValue = null;
                bool IsFound = false;
                string Name = string.Empty;
                string Description = string.Empty;
                XmlDocument Doc = new XmlDocument();
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode TabNode = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode SettingNode = null;
                int CS = 0;
                string FieldName = null;
                string FieldCaption = null;
                string FieldAddon = null;
                bool FieldReadOnly = false;
                bool FieldHTML = false;
                string fieldType = null;
                string FieldSelector = null;
                string DefaultFilename = null;
                //
                Button = cpCore.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    //
                    // Cancel just exits with no content
                    //
                    return_ExitRequest = true;
                    return string.Empty;
                } else if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                    //
                    // Not Admin Error
                    //
                    ButtonList = ButtonCancel;
                    Content.Add(Adminui.GetFormBodyAdminOnly());
                } else {
                    if (true) {
                        bool loadOK = true;
                        try {

                            Doc.LoadXml(FormXML);
                        } catch (Exception ex) {
                            // error
                            //
                            ButtonList = ButtonCancel;
                            Content.Add("<div class=\"ccError\" style=\"margin:10px;padding:10px;background-color:white;\">There was a problem with the Setting Page you requested.</div>");
                            loadOK = false;
                        }
                        //        CS = cpcore.main_OpenCSContentRecord("Setting Pages", SettingPageID)
                        //        If Not app.csv_IsCSOK(CS) Then
                        //            '
                        //            ' Setting Page was not found
                        //            '
                        //            ButtonList = ButtonCancel
                        //            Content.Add( "<div class=""ccError"" style=""margin:10px;padding:10px;background-color:white;"">The Setting Page you requested could not be found.</div>"
                        //        Else
                        //            XMLFile = app.cs_get(CS, "xmlfile")
                        //            Doc = New XmlDocument
                        //Doc.loadXML (XMLFile)
                        if (loadOK) {
                        } else {
                            //
                            // data is OK
                            //
                            if (genericController.vbLCase(Doc.DocumentElement.Name) != "form") {
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
                                        switch (genericController.vbLCase(SettingNode.Name)) {
                                            case "tab":
                                                foreach (XmlNode TabNode in SettingNode.ChildNodes) {
                                                    switch (genericController.vbLCase(TabNode.Name)) {
                                                        case "siteproperty":
                                                            //
                                                            FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
                                                            FieldValue = cpCore.docProperties.getText(FieldName);
                                                            fieldType = xml_GetAttribute(IsFound, TabNode, "type", "");
                                                            switch (genericController.vbLCase(fieldType)) {
                                                                case "integer":
                                                                    //
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        FieldValue = genericController.EncodeInteger(FieldValue).ToString();
                                                                    }
                                                                    cpCore.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                case "boolean":
                                                                    //
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        FieldValue = genericController.EncodeBoolean(FieldValue).ToString();
                                                                    }
                                                                    cpCore.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                case "float":
                                                                    //
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        FieldValue = EncodeNumber(FieldValue).ToString();
                                                                    }
                                                                    cpCore.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                case "date":
                                                                    //
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        FieldValue = genericController.EncodeDate(FieldValue).ToString();
                                                                    }
                                                                    cpCore.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                case "file":
                                                                case "imagefile":
                                                                    //
                                                                    if (cpCore.docProperties.getBoolean(FieldName + ".DeleteFlag")) {
                                                                        cpCore.siteProperties.setProperty(FieldName, "");
                                                                    }
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        Filename = FieldValue;
                                                                        VirtualFilePath = "Settings/" + FieldName + "/";
                                                                        cpCore.cdnFiles.upload(FieldName, VirtualFilePath, Filename);
                                                                        cpCore.siteProperties.setProperty(FieldName, VirtualFilePath + "/" + Filename);
                                                                    }
                                                                    break;
                                                                case "textfile":
                                                                    //
                                                                    DefaultFilename = "Settings/" + FieldName + ".txt";
                                                                    Filename = cpCore.siteProperties.getText(FieldName, DefaultFilename);
                                                                    if (string.IsNullOrEmpty(Filename)) {
                                                                        Filename = DefaultFilename;
                                                                        cpCore.siteProperties.setProperty(FieldName, DefaultFilename);
                                                                    }
                                                                    cpCore.appRootFiles.saveFile(Filename, FieldValue);
                                                                    break;
                                                                case "cssfile":
                                                                    //
                                                                    DefaultFilename = "Settings/" + FieldName + ".css";
                                                                    Filename = cpCore.siteProperties.getText(FieldName, DefaultFilename);
                                                                    if (string.IsNullOrEmpty(Filename)) {
                                                                        Filename = DefaultFilename;
                                                                        cpCore.siteProperties.setProperty(FieldName, DefaultFilename);
                                                                    }
                                                                    cpCore.appRootFiles.saveFile(Filename, FieldValue);
                                                                    break;
                                                                case "xmlfile":
                                                                    //
                                                                    DefaultFilename = "Settings/" + FieldName + ".xml";
                                                                    Filename = cpCore.siteProperties.getText(FieldName, DefaultFilename);
                                                                    if (string.IsNullOrEmpty(Filename)) {
                                                                        Filename = DefaultFilename;
                                                                        cpCore.siteProperties.setProperty(FieldName, DefaultFilename);
                                                                    }
                                                                    cpCore.appRootFiles.saveFile(Filename, FieldValue);
                                                                    break;
                                                                case "currency":
                                                                    //
                                                                    if (!string.IsNullOrEmpty(FieldValue)) {
                                                                        FieldValue = EncodeNumber(FieldValue).ToString();
                                                                        FieldValue = Microsoft.VisualBasic.Strings.FormatCurrency(FieldValue, -1, Microsoft.VisualBasic.TriState.UseDefault, Microsoft.VisualBasic.TriState.UseDefault, Microsoft.VisualBasic.TriState.UseDefault);
                                                                    }
                                                                    cpCore.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                case "link":
                                                                    cpCore.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                                default:
                                                                    cpCore.siteProperties.setProperty(FieldName, FieldValue);
                                                                    break;
                                                            }
                                                            break;
                                                        case "copycontent":
                                                            //
                                                            // A Copy Content block
                                                            //
                                                            FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
                                                            if (!FieldReadOnly) {
                                                                FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
                                                                FieldHTML = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", "false"));
                                                                if (FieldHTML) {
                                                                    //
                                                                    // treat html as active content for now.
                                                                    //
                                                                    FieldValue = cpCore.docProperties.getRenderedActiveContent(FieldName);
                                                                } else {
                                                                    FieldValue = cpCore.docProperties.getText(FieldName);
                                                                }

                                                                CS = cpCore.db.csOpen("Copy Content", "name=" + cpCore.db.encodeSQLText(FieldName), "ID");
                                                                if (!cpCore.db.csOk(CS)) {
                                                                    cpCore.db.csClose(ref CS);
                                                                    CS = cpCore.db.csInsertRecord("Copy Content");
                                                                }
                                                                if (cpCore.db.csOk(CS)) {
                                                                    cpCore.db.csSet(CS, "name", FieldName);
                                                                    //
                                                                    // Set copy
                                                                    //
                                                                    cpCore.db.csSet(CS, "copy", FieldValue);
                                                                    //
                                                                    // delete duplicates
                                                                    //
                                                                    cpCore.db.csGoNext(CS);
                                                                    while (cpCore.db.csOk(CS)) {
                                                                        cpCore.db.csDeleteRecord(CS);
                                                                        cpCore.db.csGoNext(CS);
                                                                    }
                                                                }
                                                                cpCore.db.csClose(ref CS);
                                                            }

                                                            break;
                                                        case "filecontent":
                                                            //
                                                            // A File Content block
                                                            //
                                                            FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
                                                            if (!FieldReadOnly) {
                                                                FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
                                                                fieldfilename = xml_GetAttribute(IsFound, TabNode, "filename", "");
                                                                FieldValue = cpCore.docProperties.getText(FieldName);
                                                                cpCore.appRootFiles.saveFile(fieldfilename, FieldValue);
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
                                    return_ExitRequest = true;
                                    return string.Empty;
                                }
                                //
                                // ----- Display Form
                                //
                                Content.Add(Adminui.EditTableOpen);
                                Name = xml_GetAttribute(IsFound, Doc.DocumentElement, "name", "");
                                foreach (XmlNode SettingNode in Doc.DocumentElement.ChildNodes) {
                                    switch (genericController.vbLCase(SettingNode.Name)) {
                                        case "description":
                                            Description = SettingNode.InnerText;
                                            break;
                                        case "tab":
                                            TabCnt = TabCnt + 1;
                                            TabName = xml_GetAttribute(IsFound, SettingNode, "name", "");
                                            TabDescription = xml_GetAttribute(IsFound, SettingNode, "description", "");
                                            TabHeading = xml_GetAttribute(IsFound, SettingNode, "heading", "");
                                            TabCell = new stringBuilderLegacyController();
                                            foreach (XmlNode TabNode in SettingNode.ChildNodes) {
                                                switch (genericController.vbLCase(TabNode.Name)) {
                                                    case "heading":
                                                        //
                                                        // Heading
                                                        //
                                                        FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "");
                                                        TabCell.Add(Adminui.GetEditSubheadRow(FieldCaption));
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
                                                            FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
                                                            FieldHTML = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", ""));
                                                            fieldType = xml_GetAttribute(IsFound, TabNode, "type", "");
                                                            FieldSelector = xml_GetAttribute(IsFound, TabNode, "selector", "");
                                                            FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "");
                                                            FieldAddon = xml_GetAttribute(IsFound, TabNode, "EditorAddon", "");
                                                            FieldDefaultValue = TabNode.InnerText;
                                                            FieldValue = cpCore.siteProperties.getText(FieldName, FieldDefaultValue);
                                                            //                                                    If FieldReadOnly Then
                                                            //                                                        '
                                                            //                                                        ' Read only = no editor
                                                            //                                                        '
                                                            //                                                        Copy = FieldValue & cpcore.main_GetFormInputHidden( FieldName, FieldValue)
                                                            //
                                                            //                                                    ElseIf FieldAddon <> "" Then
                                                            if (!string.IsNullOrEmpty(FieldAddon)) {
                                                                //
                                                                // Use Editor Addon
                                                                //
                                                                Dictionary<string, string> arguments = new Dictionary<string, string>();
                                                                arguments.Add("FieldName", FieldName);
                                                                arguments.Add("FieldValue", cpCore.siteProperties.getText(FieldName, FieldDefaultValue));
                                                                CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() { addonType = CPUtilsBaseClass.addonContext.ContextAdmin };
                                                                addonModel addon = addonModel.createByName(cpCore, FieldAddon);
                                                                Copy = cpCore.addon.execute(addon, executeContext);
                                                                //Option_String = "FieldName=" & FieldName & "&FieldValue=" & encodeNvaArgument(cpCore.siteProperties.getText(FieldName, FieldDefaultValue))
                                                                //Copy = execute_legacy5(0, FieldAddon, Option_String, CPUtilsBaseClass.addonContext.ContextAdmin, "", 0, "", 0)
                                                            } else if (!string.IsNullOrEmpty(FieldSelector)) {
                                                                //
                                                                // Use Selector
                                                                //
                                                                Copy = getFormContent_decodeSelector(FieldName, FieldValue, FieldSelector);
                                                            } else {
                                                                //
                                                                // Use default editor for each field type
                                                                //
                                                                switch (genericController.vbLCase(fieldType)) {
                                                                    case "integer":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue);
                                                                        }
                                                                        break;
                                                                    case "boolean":
                                                                        if (FieldReadOnly) {
                                                                            Copy = cpCore.html.html_GetFormInputCheckBox2(FieldName, genericController.EncodeBoolean(FieldValue));
                                                                            Copy = genericController.vbReplace(Copy, ">", " disabled>");
                                                                            Copy = Copy + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = cpCore.html.html_GetFormInputCheckBox2(FieldName, genericController.EncodeBoolean(FieldValue));
                                                                        }
                                                                        break;
                                                                    case "float":
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue);
                                                                        }
                                                                        break;
                                                                    case "date":
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = cpCore.html.html_GetFormInputDate(FieldName, FieldValue);
                                                                        }
                                                                        break;
                                                                    case "file":
                                                                    case "imagefile":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            if (string.IsNullOrEmpty(FieldValue)) {
                                                                                Copy = cpCore.html.html_GetFormInputFile(FieldName);
                                                                            } else {
                                                                                NonEncodedLink = cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, FieldValue);
                                                                                EncodedLink = EncodeURL(NonEncodedLink);
                                                                                string FieldValuefilename = "";
                                                                                string FieldValuePath = "";
                                                                                cpCore.privateFiles.splitPathFilename(FieldValue, FieldValuePath, FieldValuefilename);
                                                                                Copy = ""
                                                                                + "<a href=\"http://" + EncodedLink + "\" target=\"_blank\">[" + FieldValuefilename + "]</A>"
                                                                                + "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" + cpCore.html.html_GetFormInputCheckBox2(FieldName + ".DeleteFlag", false) + "&nbsp;&nbsp;&nbsp;Change:&nbsp;" + cpCore.html.html_GetFormInputFile(FieldName);
                                                                            }
                                                                        }
                                                                        //Call s.Add("&nbsp;</span></nobr></td>")
                                                                        break;
                                                                    case "currency":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            if (!string.IsNullOrEmpty(FieldValue)) {
                                                                                FieldValue = Microsoft.VisualBasic.Strings.FormatCurrency(FieldValue, -1, Microsoft.VisualBasic.TriState.UseDefault, Microsoft.VisualBasic.TriState.UseDefault, Microsoft.VisualBasic.TriState.UseDefault);
                                                                            }
                                                                            Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue);
                                                                        }
                                                                        break;
                                                                    case "textfile":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            FieldValue = cpCore.cdnFiles.readFile(FieldValue);
                                                                            if (FieldHTML) {
                                                                                Copy = cpCore.html.getFormInputHTML(FieldName, FieldValue);
                                                                            } else {
                                                                                Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue, 5);
                                                                            }
                                                                        }
                                                                        break;
                                                                    case "cssfile":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue, 5);
                                                                        }
                                                                        break;
                                                                    case "xmlfile":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue, 5);
                                                                        }
                                                                        break;
                                                                    case "link":
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue);
                                                                        }
                                                                        break;
                                                                    default:
                                                                        //
                                                                        // text
                                                                        //
                                                                        if (FieldReadOnly) {
                                                                            Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
                                                                        } else {
                                                                            if (FieldHTML) {
                                                                                Copy = cpCore.html.getFormInputHTML(FieldName, FieldValue);
                                                                            } else {
                                                                                Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue);
                                                                            }
                                                                        }
                                                                        break;
                                                                }
                                                            }
                                                            TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, false, false, ""));
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
                                                            FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
                                                            FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "");
                                                            FieldHTML = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", ""));
                                                            //
                                                            CS = cpCore.db.csOpen("Copy Content", "Name=" + cpCore.db.encodeSQLText(FieldName), "ID",,,,, "Copy");
                                                            if (!cpCore.db.csOk(CS)) {
                                                                cpCore.db.csClose(ref CS);
                                                                CS = cpCore.db.csInsertRecord("Copy Content");
                                                                if (cpCore.db.csOk(CS)) {
                                                                    RecordID = cpCore.db.csGetInteger(CS, "ID");
                                                                    cpCore.db.csSet(CS, "name", FieldName);
                                                                    cpCore.db.csSet(CS, "copy", genericController.encodeText(TabNode.InnerText));
                                                                    cpCore.db.csSave2(CS);
                                                                    //   Call cpCore.workflow.publishEdit("Copy Content", RecordID)
                                                                }
                                                            }
                                                            if (cpCore.db.csOk(CS)) {
                                                                FieldValue = cpCore.db.csGetText(CS, "copy");
                                                            }
                                                            if (FieldReadOnly) {
                                                                //
                                                                // Read only
                                                                //
                                                                Copy = FieldValue;
                                                            } else if (FieldHTML) {
                                                                //
                                                                // HTML
                                                                //
                                                                Copy = cpCore.html.getFormInputHTML(FieldName, FieldValue);
                                                                //Copy = cpcore.main_GetFormInputActiveContent( FieldName, FieldValue)
                                                            } else {
                                                                //
                                                                // Text edit
                                                                //
                                                                Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue);
                                                            }
                                                            TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, false, false, ""));
                                                        }
                                                        break;
                                                    case "filecontent":
                                                        //
                                                        // Content from a flat file
                                                        //
                                                        FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
                                                        FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "");
                                                        fieldfilename = xml_GetAttribute(IsFound, TabNode, "filename", "");
                                                        FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
                                                        FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "");
                                                        FieldDefaultValue = TabNode.InnerText;
                                                        Copy = "";
                                                        if (!string.IsNullOrEmpty(fieldfilename)) {
                                                            if (cpCore.appRootFiles.fileExists(fieldfilename)) {
                                                                Copy = FieldDefaultValue;
                                                            } else {
                                                                Copy = cpCore.cdnFiles.readFile(fieldfilename);
                                                            }
                                                            if (!FieldReadOnly) {
                                                                Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, Copy, 10);
                                                            }
                                                        }
                                                        TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, false, false, ""));
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
                                                        SQLPageSize = genericController.EncodeInteger(xml_GetAttribute(IsFound, TabNode, "rowmax", ""));
                                                        if (SQLPageSize == 0) {
                                                            SQLPageSize = 100;
                                                        }
                                                        //
                                                        // Run the SQL
                                                        //
                                                        DataTable dt = null;

                                                        if (!string.IsNullOrEmpty(FieldSQL)) {
                                                            try {
                                                                dt = cpCore.db.executeQuery(FieldSQL, FieldDataSource,, SQLPageSize);
                                                            } catch (Exception ex) {
                                                                ErrorDescription = ex.ToString();
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
                                                                //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                                                                Copy = "Error: " + Microsoft.VisualBasic.Information.Err().Description;
                                                            } else if (dt.Rows.Count <= 0) {
                                                                //
                                                                // ----- no result
                                                                //
                                                                Copy = "No Results";
                                                            } else {
                                                                //
                                                                // ----- print results
                                                                //
                                                                //PageSize = RS.PageSize
                                                                //
                                                                // --- Create the Fields for the new table
                                                                //
                                                                //
                                                                //Dim dtOk As Boolean = True
                                                                dataArray = cpCore.db.convertDataTabletoArray(dt);
                                                                //
                                                                RowMax = dataArray.GetUpperBound(1);
                                                                ColumnMax = dataArray.GetUpperBound(0);
                                                                if (RowMax == 0 && ColumnMax == 0) {
                                                                    //
                                                                    // Single result, display with no table
                                                                    //
                                                                    Copy = cpCore.html.html_GetFormInputText2("result", genericController.encodeText(dataArray[0, 0]),,,,, true);
                                                                } else {
                                                                    //
                                                                    // Build headers
                                                                    //
                                                                    FieldCount = dt.Columns.Count;
                                                                    Copy = Copy + ("\r" + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-bottom:1px solid #444;border-right:1px solid #444;background-color:white;color:#444;\">");
                                                                    Copy = Copy + (cr2 + "<tr>");
                                                                    foreach (DataColumn dc in dt.Columns) {
                                                                        Copy = Copy + (cr2 + "\t" + "<td class=\"ccadminsmall\" style=\"border-top:1px solid #444;border-left:1px solid #444;color:black;padding:2px;padding-top:4px;padding-bottom:4px;\">" + dc.ColumnName + "</td>");
                                                                    }
                                                                    Copy = Copy + (cr2 + "</tr>");
                                                                    //
                                                                    // Build output table
                                                                    //
                                                                    string RowStart = null;
                                                                    string RowEnd = null;
                                                                    string ColumnStart = null;
                                                                    string ColumnEnd = null;
                                                                    RowStart = cr2 + "<tr>";
                                                                    RowEnd = cr2 + "</tr>";
                                                                    ColumnStart = cr2 + "\t" + "<td class=\"ccadminnormal\" style=\"border-top:1px solid #444;border-left:1px solid #444;background-color:white;color:#444;padding:2px\">";
                                                                    ColumnEnd = "</td>";
                                                                    int RowPointer = 0;
                                                                    for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                                                                        Copy = Copy + (RowStart);
                                                                        int ColumnPointer = 0;
                                                                        for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                                                            object CellData = dataArray[ColumnPointer, RowPointer];
                                                                            if (IsNull(CellData)) {
                                                                                Copy = Copy + (ColumnStart + "[null]" + ColumnEnd);
                                                                            } else if ((CellData == null)) {
                                                                                Copy = Copy + (ColumnStart + "[empty]" + ColumnEnd);
                                                                            } else if (Microsoft.VisualBasic.Information.IsArray(CellData)) {
                                                                                Copy = Copy + ColumnStart + "[array]";
                                                                            } else if (genericController.encodeText(CellData) == "") {
                                                                                Copy = Copy + (ColumnStart + "[empty]" + ColumnEnd);
                                                                            } else {
                                                                                Copy = Copy + (ColumnStart + genericController.encodeHTML(genericController.encodeText(CellData)) + ColumnEnd);
                                                                            }
                                                                        }
                                                                        Copy = Copy + (RowEnd);
                                                                    }
                                                                    Copy = Copy + ("\r" + "</table>");
                                                                }
                                                            }
                                                        }
                                                        TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, false, false, ""));
                                                        break;
                                                }
                                            }
                                            Copy = Adminui.GetEditPanel(true, TabHeading, TabDescription, Adminui.EditTableOpen + TabCell.Text + Adminui.EditTableClose);
                                            if (!string.IsNullOrEmpty(Copy)) {
                                                cpCore.html.main_AddLiveTabEntry(TabName.Replace(" ", "&nbsp;"), Copy, "ccAdminTab");
                                            }
                                            //Content.Add( cpcore.main_GetForm_Edit_AddTab(TabName, Copy, True))
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
                                if (TabCnt > 0) {
                                    Content.Add(cpCore.html.main_GetLiveTabs());
                                }
                            }
                        }
                    }
                }
                //
                tempgetFormContent = Adminui.GetBody(Name, ButtonList, "", true, true, Description, "", 0, Content.Text);
                Content = null;
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        //   Display field in the admin/edit
        //========================================================================
        //
        private string getFormContent_decodeSelector(string SitePropertyName, string SitePropertyValue, string selector) {
            string tempgetFormContent_decodeSelector = null;
            try {
                //
                string ExpandedSelector = string.Empty;
                Dictionary<string, string> addonInstanceProperties = new Dictionary<string, string>();
                string OptionCaption = null;
                string OptionValue = null;
                string OptionValue_AddonEncoded = null;
                int OptionPtr = 0;
                int OptionCnt = 0;
                string[] OptionValues = null;
                string OptionSuffix = string.Empty;
                string LCaseOptionDefault = null;
                int Pos = 0;
                bool Checked = false;
                int ParentID = 0;
                int ParentCID = 0;
                string Criteria = null;
                int RootCID = 0;
                string SQL = null;
                int TableID = 0;
                int TableName = 0;
                int ChildCID = 0;
                string CIDList = null;
                string TableName2 = null;
                string RecordContentName = null;
                bool HasParentID = false;
                int CS = 0;
                // converted array to dictionary - Dim FieldPointer As Integer
                int CSPointer = 0;
                int RecordID = 0;
                stringBuilderLegacyController FastString = null;
                int FieldValueInteger = 0;
                bool FieldRequired = false;
                string FieldHelp = null;
                string AuthoringStatusMessage = null;
                string Delimiter = null;
                string Copy = string.Empty;
                adminUIController Adminui = new adminUIController(cpCore);
                //
                string FieldName = null;
                //
                FastString = new stringBuilderLegacyController();
                //
                Dictionary<string, string> instanceOptions = new Dictionary<string, string>();
                instanceOptions.Add(SitePropertyName, SitePropertyValue);
                buildAddonOptionLists(ref addonInstanceProperties, ref ExpandedSelector, SitePropertyName + "=" + selector, instanceOptions, "0", true);
                Pos = genericController.vbInstr(1, ExpandedSelector, "[");
                if (Pos != 0) {
                    //
                    // List of Options, might be select, radio or checkbox
                    //
                    LCaseOptionDefault = genericController.vbLCase(ExpandedSelector.Substring(0, Pos - 1));
                    int PosEqual = genericController.vbInstr(1, LCaseOptionDefault, "=");

                    if (PosEqual > 0) {
                        LCaseOptionDefault = LCaseOptionDefault.Substring(PosEqual);
                    }

                    LCaseOptionDefault = genericController.decodeNvaArgument(LCaseOptionDefault);
                    ExpandedSelector = ExpandedSelector.Substring(Pos);
                    Pos = genericController.vbInstr(1, ExpandedSelector, "]");
                    if (Pos > 0) {
                        if (Pos < ExpandedSelector.Length) {
                            OptionSuffix = genericController.vbLCase((ExpandedSelector.Substring(Pos)).Trim(' '));
                        }
                        ExpandedSelector = ExpandedSelector.Substring(0, Pos - 1);
                    }
                    OptionValues = ExpandedSelector.Split('|');
                    tempgetFormContent_decodeSelector = "";
                    OptionCnt = OptionValues.GetUpperBound(0) + 1;
                    for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++) {
                        OptionValue_AddonEncoded = OptionValues[OptionPtr].Trim(' ');
                        if (!string.IsNullOrEmpty(OptionValue_AddonEncoded)) {
                            Pos = genericController.vbInstr(1, OptionValue_AddonEncoded, ":");
                            if (Pos == 0) {
                                OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded);
                                OptionCaption = OptionValue;
                            } else {
                                OptionCaption = genericController.decodeNvaArgument(OptionValue_AddonEncoded.Substring(0, Pos - 1));
                                OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded.Substring(Pos));
                            }
                            switch (OptionSuffix) {
                                case "checkbox":
                                    //
                                    // Create checkbox
                                    //
                                    if (genericController.vbInstr(1, "," + LCaseOptionDefault + ",", "," + genericController.vbLCase(OptionValue) + ",") != 0) {
                                        tempgetFormContent_decodeSelector = tempgetFormContent_decodeSelector + "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + SitePropertyName + OptionPtr + "\" value=\"" + OptionValue + "\" checked=\"checked\">" + OptionCaption + "</div>";
                                    } else {
                                        tempgetFormContent_decodeSelector = tempgetFormContent_decodeSelector + "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + SitePropertyName + OptionPtr + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
                                    }
                                    break;
                                case "radio":
                                    //
                                    // Create Radio
                                    //
                                    if (genericController.vbLCase(OptionValue) == LCaseOptionDefault) {
                                        tempgetFormContent_decodeSelector = tempgetFormContent_decodeSelector + "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + SitePropertyName + "\" value=\"" + OptionValue + "\" checked=\"checked\" >" + OptionCaption + "</div>";
                                    } else {
                                        tempgetFormContent_decodeSelector = tempgetFormContent_decodeSelector + "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + SitePropertyName + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
                                    }
                                    break;
                                default:
                                    //
                                    // Create select 
                                    //
                                    if (genericController.vbLCase(OptionValue) == LCaseOptionDefault) {
                                        tempgetFormContent_decodeSelector = tempgetFormContent_decodeSelector + "<option value=\"" + OptionValue + "\" selected>" + OptionCaption + "</option>";
                                    } else {
                                        tempgetFormContent_decodeSelector = tempgetFormContent_decodeSelector + "<option value=\"" + OptionValue + "\">" + OptionCaption + "</option>";
                                    }
                                    break;
                            }
                        }
                    }
                    switch (OptionSuffix) {
                        case "checkbox":
                            //
                            //
                            Copy = Copy + "<input type=\"hidden\" name=\"" + SitePropertyName + "CheckBoxCnt\" value=\"" + OptionCnt + "\" >";
                            break;
                        case "radio":
                            //
                            // Create Radio 
                            //
                            //cpCore.htmldoc.main_Addon_execute_GetFormContent_decodeSelector = "<div>" & genericController.vbReplace(cpCore.htmldoc.main_Addon_execute_GetFormContent_decodeSelector, "><", "></div><div><") & "</div>"
                            break;
                        default:
                            //
                            // Create select 
                            //
                            tempgetFormContent_decodeSelector = "<select name=\"" + SitePropertyName + "\">" + tempgetFormContent_decodeSelector + "</select>";
                            break;
                    }
                } else {
                    //
                    // Create Text addon_execute_GetFormContent_decodeSelector
                    //

                    selector = genericController.decodeNvaArgument(selector);
                    tempgetFormContent_decodeSelector = cpCore.html.html_GetFormInputText2(SitePropertyName, selector, 1, 20);
                }

                FastString = null;
                return tempgetFormContent_decodeSelector;
                //
            } catch (Exception ex) {
                cpCore.handleException( ex );
            }
            //ErrorTrap:
            FastString = null;
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("addon_execute_GetFormContent_decodeSelector")
            return tempgetFormContent_decodeSelector;
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
                string ConstructorType = null;
                string ConstructorValue = null;
                string ConstructorSelector = null;
                string ConstructorName = null;
                int ConstructorPtr = 0;
                int Pos = 0;
                int InstanceCnt = 0;
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
                    ConstructorNameValues = Microsoft.VisualBasic.Strings.Split(addonArgumentListFromRecord, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
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
                        ConstructorType = "text";
                        Pos = genericController.vbInstr(1, ConstructorName, "=");
                        if (Pos > 1) {
                            ConstructorValue = ConstructorName.Substring(Pos);
                            ConstructorName = (ConstructorName.Substring(0, Pos - 1)).Trim(' ');
                            Pos = genericController.vbInstr(1, ConstructorValue, "[");
                            if (Pos > 0) {
                                ConstructorSelector = ConstructorValue.Substring(Pos - 1);
                                ConstructorValue = ConstructorValue.Substring(0, Pos - 1);
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
                InstanceCnt = 0;
                //
                // Now update the values with Instance - if a name is not found, add it
                //
                foreach (var kvp in instanceOptions) {
                    InstanceName = kvp.Key;
                    InstanceValue = kvp.Value;
                    if (!string.IsNullOrEmpty(InstanceName)) {
                        //
                        // if the name is not in the Constructor, add it
                        if (ConstructorCnt > 0) {
                            for (ConstructorPtr = 0; ConstructorPtr < ConstructorCnt; ConstructorPtr++) {
                                if (genericController.vbLCase(InstanceName) == genericController.vbLCase(ConstructorNames[ConstructorPtr])) {
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
                        addonArgumentListPassToBubbleEditor = addonArgumentListPassToBubbleEditor + Environment.NewLine + cpCore.html.getAddonSelector(ConstructorName, ConstructorValue, ConstructorSelector);
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
                cpCore.handleException(ex);
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
        //
        //
        public string getPrivateFilesAddonPath() {
            return "addons\\";
        }
        //
        //========================================================================
        //   Apply a wrapper to content
        //========================================================================
        //
        private string addWrapperToResult(string Content, int WrapperID, string WrapperSourceForComment = "") {
            try {
                //
                int Pos = 0;
                int CS = 0;
                string JSFilename = null;
                string Copy = null;
                string s = null;
                string SelectFieldList = null;
                string Wrapper = null;
                string wrapperName = null;
                string SourceComment = null;
                string TargetString = null;
                //
                s = Content;
                SelectFieldList = "name,copytext,javascriptonload,javascriptbodyend,stylesfilename,otherheadtags,JSFilename,targetString";
                CS = cpCore.db.csOpenRecord("Wrappers", WrapperID,,, SelectFieldList);
                if (cpCore.db.csOk(CS)) {
                    Wrapper = cpCore.db.csGetText(CS, "copytext");
                    wrapperName = cpCore.db.csGetText(CS, "name");
                    TargetString = cpCore.db.csGetText(CS, "targetString");
                    //
                    SourceComment = "wrapper " + wrapperName;
                    if (!string.IsNullOrEmpty(WrapperSourceForComment)) {
                        SourceComment = SourceComment + " for " + WrapperSourceForComment;
                    }
                    cpCore.html.addScriptCode_onLoad(cpCore.db.csGetText(CS, "javascriptonload"), SourceComment);
                    cpCore.html.addScriptCode_body(cpCore.db.csGetText(CS, "javascriptbodyend"), SourceComment);
                    cpCore.html.addHeadTag(cpCore.db.csGetText(CS, "OtherHeadTags"), SourceComment);
                    //
                    JSFilename = cpCore.db.csGetText(CS, "jsfilename");
                    if (!string.IsNullOrEmpty(JSFilename)) {
                        JSFilename = cpCore.webServer.requestProtocol + cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, JSFilename);
                        cpCore.html.addScriptLink_Head(JSFilename, SourceComment);
                    }
                    Copy = cpCore.db.csGetText(CS, "stylesfilename");
                    if (!string.IsNullOrEmpty(Copy)) {
                        if (genericController.vbInstr(1, Copy, "://") != 0) {
                        } else if (Copy.Substring(0, 1) == "/") {
                        } else {
                            Copy = cpCore.webServer.requestProtocol + cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, Copy);
                        }
                        cpCore.html.addStyleLink(Copy, SourceComment);
                    }
                    //
                    if (!string.IsNullOrEmpty(Wrapper)) {
                        Pos = genericController.vbInstr(1, Wrapper, TargetString, Microsoft.VisualBasic.Constants.vbTextCompare);
                        if (Pos != 0) {
                            s = genericController.vbReplace(Wrapper, TargetString, s, 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
                        } else {
                            s = ""
                                + "<!-- the selected wrapper does not include the Target String marker to locate the position of the content. -->"
                                + Wrapper + s;
                        }
                    }
                }
                cpCore.db.csClose(ref CS);
                //
                return s;
                //
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                cpCore.handleException( ex );
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("WrapContent")
        }
        //
        //========================================================================
        // ----- main_Get an XML nodes attribute based on its name
        //========================================================================
        //
        public string xml_GetAttribute(bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
            string result = string.Empty;
            try {
                //
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlAttribute NodeAttribute = null;
                XmlNode ResultNode = null;
                string UcaseName = null;
                //
                Found = false;
                ResultNode = Node.Attributes.GetNamedItem(Name);
                if (ResultNode == null) {
                    UcaseName = genericController.vbUCase(Name);
                    foreach (XmlAttribute NodeAttribute in Node.Attributes) {
                        if (genericController.vbUCase(NodeAttribute.Name) == UcaseName) {
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
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //
        public static string main_GetDefaultAddonOption_String(coreClass cpCore, string ArgumentList, string AddonGuid, bool IsInline) {
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
            ArgumentList = genericController.vbReplace(ArgumentList, Environment.NewLine, "\r");
            ArgumentList = genericController.vbReplace(ArgumentList, "\n", "\r");
            ArgumentList = genericController.vbReplace(ArgumentList, "\r", Environment.NewLine);
            if (ArgumentList.IndexOf("wrapper", System.StringComparison.OrdinalIgnoreCase) + 1 == 0) {
                //
                // Add in default constructors, like wrapper
                //
                if (!string.IsNullOrEmpty(ArgumentList)) {
                    ArgumentList = ArgumentList + Environment.NewLine;
                }
                if (genericController.vbLCase(AddonGuid) == genericController.vbLCase(addonGuidContentBox)) {
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
                QuerySplit = genericController.SplitCRLF(ArgumentList);
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
                        NameValue = genericController.vbReplace(NameValue, "\\=", Environment.NewLine);
                        Pos = genericController.vbInstr(1, NameValue, "=");
                        if (Pos == 0) {
                            OptionName = NameValue;
                        } else {
                            OptionName = NameValue.Substring(0, Pos - 1);
                            OptionValue = NameValue.Substring(Pos);
                        }
                        OptionName = genericController.vbReplace(OptionName, Environment.NewLine, "\\=");
                        OptionValue = genericController.vbReplace(OptionValue, Environment.NewLine, "\\=");
                        //
                        // split optionvalue on [
                        //
                        OptionValue = genericController.vbReplace(OptionValue, "\\[", Environment.NewLine);
                        Pos = genericController.vbInstr(1, OptionValue, "[");
                        if (Pos != 0) {
                            OptionSelector = OptionValue.Substring(Pos - 1);
                            OptionValue = OptionValue.Substring(0, Pos - 1);
                        }
                        OptionValue = genericController.vbReplace(OptionValue, Environment.NewLine, "\\[");
                        OptionSelector = genericController.vbReplace(OptionSelector, Environment.NewLine, "\\[");
                        //
                        // Decode AddonConstructor format
                        //
                        OptionName = genericController.DecodeAddonConstructorArgument(OptionName);
                        OptionValue = genericController.DecodeAddonConstructorArgument(OptionValue);
                        //
                        // Encode AddonOption format
                        //
                        //main_GetAddonSelector expects value to be encoded, but not name
                        //OptionName = encodeNvaArgument(OptionName)
                        OptionValue = genericController.encodeNvaArgument(OptionValue);
                        //
                        // rejoin
                        //
                        NameValuePair = cpCore.html.getAddonSelector(OptionName, OptionValue, OptionSelector);
                        NameValuePair = genericController.EncodeJavascript(NameValuePair);
                        result = result + "&" + NameValuePair;
                        if (genericController.vbInstr(1, NameValuePair, "=") == 0) {
                            result = result + "=";
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
        //=================================================================================================================
        //   csv_GetAddonOption
        //
        //   returns the value matching a given name in an AddonOptionConstructor
        //
        //   AddonOptionConstructor is a crlf delimited name=value[selector]descriptor list
        //
        //   See cpCoreClass.ExecuteAddon for a full description of:
        //       AddonOptionString
        //       AddonOptionConstructor
        //       AddonOptionNameValueList
        //       AddonOptionExpandedConstructor
        //=================================================================================================================
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
                TargetName = genericController.vbLCase(OptionName);
                Options = OptionString.Split('&');
                for (Ptr = 0; Ptr <= Options.GetUpperBound(0); Ptr++) {
                    Pos = genericController.vbInstr(1, Options[Ptr], "=");
                    if (Pos > 0) {
                        TestName = genericController.vbLCase((Options[Ptr].Substring(0, Pos - 1)).Trim(' '));
                        while ((!string.IsNullOrEmpty(TestName)) && (TestName.Substring(0, 1) == "\t")) {
                            TestName = TestName.Substring(1).Trim(' ');
                        }
                        while ((!string.IsNullOrEmpty(TestName)) && (TestName.Substring(TestName.Length - 1) == "\t")) {
                            TestName = (TestName.Substring(0, TestName.Length - 1)).Trim(' ');
                        }
                        if (TestName == TargetName) {
                            result = genericController.decodeNvaArgument((Options[Ptr].Substring(Pos)).Trim(' '));
                            break;
                        }
                    }
                }
            }
            return result;
        }
        //
        private string getAddonDescription(coreClass cpcore, Models.Entity.addonModel addon) {
            string addonDescription = "[invalid addon]";
            if (addon != null) {
                string collectionName = "invalid collection or collection not set";
                Models.Entity.AddonCollectionModel collection = Models.Entity.AddonCollectionModel.create(cpcore, addon.CollectionID);
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
        public static string GetAddonManager(coreClass cpCore) {
            string addonManager = "";
            try {
                bool AddonStatusOK = true;
                try {
                    addonModel addon = addonModel.create(cpCore, addonGuidAddonManager);
                    addonManager = cpCore.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext() { addonType = CPUtilsBaseClass.addonContext.ContextAdmin });
                    //addonManager = cpCore.addon.execute_legacy2(0, addonGuidAddonManager, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, "", 0, "", "0", False, -1, "", AddonStatusOK, Nothing)
                } catch (Exception ex) {
                    cpCore.handleException(new Exception("Error calling ExecuteAddon with AddonManagerGuid, will attempt Safe Mode Addon Manager. Exception=[" + ex.ToString() + "]"));
                    AddonStatusOK = false;
                }
                if (string.IsNullOrEmpty(addonManager)) {
                    cpCore.handleException(new Exception("AddonManager returned blank, calling Safe Mode Addon Manager."));
                    AddonStatusOK = false;
                }
                if (!AddonStatusOK) {
                    addon_AddonMngrSafeClass AddonMan = new addon_AddonMngrSafeClass(cpCore);
                    addonManager = AddonMan.GetForm_SafeModeAddonManager();
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return addonManager;
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
            string sample_folderPath = IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string assemblyPath = IO.Path.Combine(sample_folderPath, (new AssemblyName(args.Name)).Name + ".dll");
            if (!IO.File.Exists(assemblyPath)) {
                return null;
            }
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
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
        ~addonController() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(false);
            //INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
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