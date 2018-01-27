﻿
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
using Contensive.Core.Models.Complex;
using Contensive.Core.Addons.Tools;
//
namespace Contensive.Core.Addons.AdminSite {
    public class getHtmlBodyClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        // objects passed in - do not dispose
        //   sets cp from argument For use In calls To other objects, Then core because cp cannot be used since that would be a circular depenancy
        //====================================================================================================
        //
        private CPClass cp; // local cp set in constructor
        private coreController core; // core -- short term, this is the migration solution from a built-in tool, to an addon
        //
        //====================================================================================================
        /// <summary>
        /// addon method, deliver complete Html admin site
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string returnHtml = "";
            try {
                //
                // -- ok to cast cpbase to cp because they build from the same solution
                this.cp = (CPClass)cp;
                core = this.cp.core;
                //
                // -- log request
                string SaveContent = ""
                        + DateTime.Now + "\r\nmember.name:" + core.doc.sessionContext.user.name + "\r\nmember.id:" + core.doc.sessionContext.user.id + "\r\nvisit.id:" + core.doc.sessionContext.visit.id + "\r\nurl:" + core.webServer.requestUrl + "\r\nurl source:" + core.webServer.requestUrlSource + "\r\n----------"
                        + "\r\nform post:";
                foreach (string key in core.docProperties.getKeyList()) {
                    docPropertiesClass docProperty = core.docProperties.getProperty(key);
                    if (docProperty.IsForm) {
                        SaveContent += "\r\n" + docProperty.NameValue;
                    }
                }
                if (!(core.webServer.requestFormBinaryHeader == null)) {
                    byte[] BinaryHeader = core.webServer.requestFormBinaryHeader;
                    string BinaryHeaderString = genericController.kmaByteArrayToString(BinaryHeader);
                    SaveContent += ""
                            + "\r\n----------"
                            + "\r\nbinary header:"
                            + "\r\n" + BinaryHeaderString + "\r\n";
                }
                logController.appendLog(core, SaveContent, "admin", core.appConfig.name + "-request-");
                //
                if (!core.doc.sessionContext.isAuthenticated) {
                    //
                    // --- must be authenticated to continue. Force a local login
                    //
                    returnHtml = core.addon.execute(Contensive.Core.Models.DbModels.addonModel.create(core, addonGuidLoginPage), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                        errorCaption = "Login Page",
                        addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextPage
                    });
                } else if (!core.doc.sessionContext.isAuthenticatedContentManager(core)) {
                    //
                    // --- member must have proper access to continue
                    //
                    returnHtml = ""
                        + "<p>" + SpanClassAdminNormal + "You are attempting to enter an area which your account does not have access."
                        + "\r<ul class=\"ccList\">"
                        + "\r<li class=\"ccListItem\">To return to the public web site, use your back button, or <a href=\"" + requestAppRootPath + "\">Click Here</A>."
                        + "\r<li class=\"ccListItem\">To login under a different account, <a href=\"/" + core.appConfig.adminRoute + "?method=logout\" rel=\"nofollow\">Click Here</A>"
                        + "\r<li class=\"ccListItem\">To have your account access changed to include this area, please contact the <a href=\"mailto:" + core.siteProperties.getText("EmailAdmin") + "\">system administrator</A>. "
                        + "\r</ul>"
                        + "</span></p>";
                    returnHtml = ""
                        + core.html.getPanelHeader("Unauthorized Access") + core.html.getPanel(returnHtml, "ccPanel", "ccPanelHilite", "ccPanelShadow", "400", 15);
                    returnHtml = ""
                        + "\r<div style=\"display:table;margin:100px auto auto auto;\">"
                        + genericController.htmlIndent(returnHtml) + "\r</div>";
                    //
                    core.doc.setMetaContent(0, 0);
                    core.html.addTitle("Unauthorized Access", "adminSite");
                    returnHtml = "<div class=\"ccBodyAdmin ccCon\">" + returnHtml + "</div>";
                    //returnHtml = core.html.getHtmlDoc(returnHtml, "<body class=""ccBodyAdmin ccCon"">", True, True, False)
                } else {
                    //
                    // get admin content
                    //
                    returnHtml = "<div class=\"ccBodyAdmin ccCon\">" + getAdminBody() + "</div>";
                    //returnHtml = core.html.getHtmlDoc(adminBody, "<body class=""ccBodyAdmin ccCon"">", True, True, False)
                }
                //
                // Log response
                //
                SaveContent += ""
                        + DateTime.Now + "\r\nmember.name:" + core.doc.sessionContext.user.name + "\r\nmember.id:" + core.doc.sessionContext.user.id + "\r\nvisit.id:" + core.doc.sessionContext.visit.id + "\r\nurl:" + core.webServer.requestUrl + "\r\nurl source:" + core.webServer.requestUrlSource + "\r\n----------"
                        + "\r\nresponse:"
                        + "\r\n" + returnHtml;
                DateTime rightNow = DateTime.Now;
                //logController.appendLog(core, SaveContent, "admin", rightNow.Year + rightNow.Month.ToString("00") + rightNow.Day.ToString("00") + rightNow.Hour.ToString("00") + rightNow.Minute.ToString("00") + rightNow.Second.ToString("00"));
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return returnHtml;
        }
        //
        //====================================================================================================
        /// <summary>
        /// REFACTOR - Constructor for addon instances. Until refactoring, calls into other methods must be constructed with (coreClass) variation.
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public getHtmlBodyClass() : base() {
        }
        //
        //====================================================================================================
        /// <summary>
        /// REFACTOR - Constructor for non-addon instances. (REFACTOR - work-around for pre-refactoring of admin remote methods currently in core classes)
        /// </summary>
        /// <param name="core"></param>
        public getHtmlBodyClass(Contensive.Core.CPClass cp) : base() {
            this.cp = cp;
            core = this.cp.core;
        }
        //
        //========================================================================
        //
        private string getAdminBody(string ContentArgFromCaller = "") {
            string result = "";
            try {
                int DefaultWrapperID = 0;
                string AddonHelpCopy = null;
                string InstanceOptionString = null;
                int HelpLevel = 0;
                int HelpAddonID = 0;
                int HelpCollectionID = 0;
                string CurrentLink = null;
                string EditReferer = null;
                string ContentCell = null;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                int addonId = 0;
                string AddonGuid = null;
                string AddonName = "";
                bool UseContentWatchLink = false;
                editRecordClass editRecord = new editRecordClass();
                cdefModel AdminContent = new cdefModel();
                //
                //-------------------------------------------------------------------------------
                // Setup defaults
                //-------------------------------------------------------------------------------
                //
                core.db.sqlCommandTimeout = 300;
                ButtonObjectCount = 0;
                JavaScriptString = "";
                ContentWatchLoaded = false;
                editRecord.Loaded = false;
                UseContentWatchLink = core.siteProperties.useContentWatchLink;
                core.html.addScriptCode_onLoad("document.getElementsByTagName('BODY')[0].onclick = BodyOnClick;", "Contensive");
                core.doc.setMetaContent(0, 0);
                //
                //-------------------------------------------------------------------------------
                // check for member login, if logged in and no admin, lock out
                // Do CheckMember here because we need to know who is there to create proper blocked menu
                //-------------------------------------------------------------------------------
                //
                if (!core.doc.continueProcessing) {
                    //
                    // ----- no stream anyway, do nothing
                    //
                    //ElseIf Not core.doc.authContext.isAuthenticated Then
                    //    '
                    //    ' --- must be authenticated to continue
                    //    '
                    //    Dim loginAddon As New Addons.addon_loginClass(core)
                    //    Stream.Add(loginAddon.getLoginForm())
                } else {
                    //
                    // -- add exception if build verison does not match code
                    if (core.siteProperties.dataBuildVersion != cp.Version) {
                        if (string.Compare(core.siteProperties.dataBuildVersion, cp.Version) < 0) {
                            core.handleException(new ApplicationException("Application code version is older than Db version. Run command line upgrade method on this site."));
                        } else {
                            core.handleException(new ApplicationException("Application code version is newer than Db version. Upgrade site code."));
                        }
                    }
                    //
                    //-------------------------------------------------------------------------------
                    // Get Requests
                    //   initialize adminContent and editRecord objects 
                    //-------------------------------------------------------------------------------
                    //
                    GetForm_LoadControl(ref AdminContent, editRecord);
                    addonId = core.docProperties.getInteger("addonid");
                    AddonGuid = core.docProperties.getText("addonguid");
                    //
                    //-------------------------------------------------------------------------------
                    //
                    //-------------------------------------------------------------------------------
                    //
                    //If AdminContent.fields.Count > 0 Then
                    //    ReDim EditRecordValuesObject(AdminContent.fields.Count)
                    //    ReDim EditRecordDbValues(AdminContent.fields.Count)
                    //End If
                    //
                    //-------------------------------------------------------------------------------
                    // Process SourceForm/Button into Action/Form, and process
                    //-------------------------------------------------------------------------------
                    //
                    if (core.docProperties.getText("Button") == ButtonCancelAll) {
                        AdminForm = AdminFormRoot;
                    } else {
                        ProcessForms(AdminContent, editRecord);
                        ProcessActions(AdminContent, editRecord, UseContentWatchLink);
                    }
                    //
                    //-------------------------------------------------------------------------------
                    // Normalize values to be needed
                    //-------------------------------------------------------------------------------
                    //
                    if (editRecord.id != 0) {
                        core.workflow.ClearEditLock(AdminContent.Name, editRecord.id);
                    }
                    //
                    if (AdminForm < 1) {
                        //
                        // No form was set, use default form
                        //
                        if (AdminContent.Id <= 0) {
                            AdminForm = AdminFormRoot;
                        } else {
                            AdminForm = AdminFormIndex;
                        }
                    }
                    //
                    if (AdminForm == AdminFormLegacyAddonManager) {
                        //
                        // patch out any old links to the legacy addon manager
                        //
                        AdminForm = 0;
                        AddonGuid = addonGuidAddonManager;
                    }
                    //
                    //-------------------------------------------------------------------------------
                    // Edit form but not valid record case
                    // Put this here so we can display the error without being stuck displaying the edit form
                    // Putting the error on the edit form is confusing because there are fields to fill in
                    //-------------------------------------------------------------------------------
                    //
                    if (AdminSourceForm == AdminFormEdit) {
                        if ((!(core.doc.debug_iUserError != "")) & ((AdminButton == ButtonOK) || (AdminButton == ButtonCancel) || (AdminButton == ButtonDelete))) {
                            EditReferer = core.docProperties.getText("EditReferer");
                            CurrentLink = genericController.modifyLinkQuery(core.webServer.requestUrl, "editreferer", "", false);
                            CurrentLink = genericController.vbLCase(CurrentLink);
                            //
                            // check if this editreferer includes cid=thisone and id=thisone -- if so, go to index form for this cid
                            //
                            if ((!string.IsNullOrEmpty(EditReferer)) & (EditReferer.ToLower() != CurrentLink)) {
                                //
                                // return to the page it came from
                                //
                                return core.webServer.redirect(EditReferer, "Admin Edit page returning to the EditReferer setting");
                            } else {
                                //
                                // return to the index page for this content
                                //
                                AdminForm = AdminFormIndex;
                            }
                        }
                        if (BlockEditForm) {
                            AdminForm = AdminFormIndex;
                        }
                    }
                    HelpLevel = core.docProperties.getInteger("helplevel");
                    HelpAddonID = core.docProperties.getInteger("helpaddonid");
                    HelpCollectionID = core.docProperties.getInteger("helpcollectionid");
                    if (HelpCollectionID == 0) {
                        HelpCollectionID = core.visitProperty.getInteger("RunOnce HelpCollectionID");
                        if (HelpCollectionID != 0) {
                            core.visitProperty.setProperty("RunOnce HelpCollectionID", "");
                        }
                    }
                    //
                    //-------------------------------------------------------------------------------
                    // build refresh string
                    //-------------------------------------------------------------------------------
                    //
                    if (AdminContent.Id != 0) {
                        core.doc.addRefreshQueryString("cid", genericController.encodeText(AdminContent.Id));
                    }
                    if (editRecord.id != 0) {
                        core.doc.addRefreshQueryString("id", genericController.encodeText(editRecord.id));
                    }
                    if (TitleExtension != "") {
                        core.doc.addRefreshQueryString(RequestNameTitleExtension, genericController.EncodeRequestVariable(TitleExtension));
                    }
                    if (RecordTop != 0) {
                        core.doc.addRefreshQueryString("rt", genericController.encodeText(RecordTop));
                    }
                    if (RecordsPerPage != RecordsPerPageDefault) {
                        core.doc.addRefreshQueryString("rs", genericController.encodeText(RecordsPerPage));
                    }
                    if (AdminForm != 0) {
                        core.doc.addRefreshQueryString(RequestNameAdminForm, genericController.encodeText(AdminForm));
                    }
                    if (MenuDepth != 0) {
                        core.doc.addRefreshQueryString(RequestNameAdminDepth, genericController.encodeText(MenuDepth));
                    }
                    //
                    // normalize guid
                    //
                    if (!string.IsNullOrEmpty(AddonGuid)) {
                        if ((AddonGuid.Length == 38) && (AddonGuid.Left(1) == "{") && (AddonGuid.Substring(AddonGuid.Length - 1) == "}")) {
                            //
                            // Good to go
                            //
                        } else if (AddonGuid.Length == 36) {
                            //
                            // might be valid with the brackets, add them
                            //
                            AddonGuid = "{" + AddonGuid + "}";
                        } else if (AddonGuid.Length == 32) {
                            //
                            // might be valid with the brackets and the dashes, add them
                            //
                            AddonGuid = "{" + AddonGuid.Left(8) + "-" + AddonGuid.Substring(8, 4) + "-" + AddonGuid.Substring(12, 4) + "-" + AddonGuid.Substring(16, 4) + "-" + AddonGuid.Substring(20) + "}";
                        } else {
                            //
                            // not valid
                            //
                            AddonGuid = "";
                        }
                    }
                    //
                    //-------------------------------------------------------------------------------
                    // Create the content
                    //-------------------------------------------------------------------------------
                    //
                    ContentCell = "";
                    if (!string.IsNullOrEmpty(ContentArgFromCaller)) {
                        //
                        // Use content passed in as an argument
                        //
                        ContentCell = ContentArgFromCaller;
                    } else if (HelpAddonID != 0) {
                        //
                        // display Addon Help
                        //
                        core.doc.addRefreshQueryString("helpaddonid", HelpAddonID.ToString());
                        ContentCell = GetAddonHelp(HelpAddonID, "");
                    } else if (HelpCollectionID != 0) {
                        //
                        // display Collection Help
                        //
                        core.doc.addRefreshQueryString("helpcollectionid", HelpCollectionID.ToString());
                        ContentCell = GetCollectionHelp(HelpCollectionID, "");
                    } else if (AdminForm != 0) {
                        //
                        // No content so far, try the forms
                        // todo - convert this to switch
                        if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormBuilderCollection) {
                            ContentCell = GetForm_BuildCollection();
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormSecurityControl) {
                            AddonGuid = AddonGuidPreferences;
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormMetaKeywordTool) {
                            ContentCell = GetForm_MetaKeywordTool();
                        } else if ((encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormMobileBrowserControl) || (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormPageControl) || (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormEmailControl)) {
                            ContentCell = core.addon.execute(addonModel.create(core, AddonGuidPreferences), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorCaption = "Preferences"
                            });
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormClearCache) {
                            ContentCell = GetForm_ClearCache();
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormSpiderControl) {
                            ContentCell = core.addon.execute(addonModel.createByName(core, "Content Spider Control"), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorCaption = "Content Spider Control"
                            });
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormResourceLibrary) {
                            ContentCell = core.html.getResourceLibrary2("", false, "", "", true);
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormQuickStats) {
                            ContentCell = (GetForm_QuickStats());
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormIndex) {
                            ContentCell = (GetForm_Index(AdminContent, editRecord, (AdminContent.ContentTableName.ToLower() == "ccemail")));
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormEdit) {
                            ContentCell = GetForm_Edit(AdminContent, editRecord);
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormClose) {
                            Stream.Add("<Script Language=\"JavaScript\" type=\"text/javascript\"> window.close(); </Script>");
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormPublishing) {
                            ContentCell = (GetForm_Publish());
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormContentChildTool) {
                            ContentCell = (GetContentChildTool());
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminformPageContentMap) {
                            ContentCell = (GetForm_PageContentMap());
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminformHousekeepingControl) {
                            ContentCell = (GetForm_HouseKeepingControl());
                        } else if ((encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormTools) || (encodeInteger(Math.Floor(encodeNumber(AdminForm))) >= 100 && encodeInteger(Math.Floor(encodeNumber(AdminForm))) <= 199)) {
                            legacyToolsClass Tools = new legacyToolsClass(core);
                            ContentCell = Tools.GetForm();
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormStyleEditor) {
                            ContentCell = (admin_GetForm_StyleEditor());
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormDownloads) {
                            ContentCell = (GetForm_Downloads());
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminformRSSControl) {
                            ContentCell = core.webServer.redirect("?cid=" + cdefModel.getContentId(core, "RSS Feeds"), "RSS Control page is not longer supported. RSS Feeds are controlled from the RSS feed records.");
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormImportWizard) {
                            ContentCell = core.addon.execute(addonModel.create(core, addonGuidImportWizard), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorCaption = "Import Wizard"
                            });
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormCustomReports) {
                            ContentCell = GetForm_CustomReports();
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormFormWizard) {
                            ContentCell = core.addon.execute(addonModel.create(core, addonGuidFormWizard), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorCaption = "Form Wizard"
                            });
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormLegacyAddonManager) {
                            ContentCell = addonController.GetAddonManager(core);
                        } else if (encodeInteger(Math.Floor(encodeNumber(AdminForm))) == AdminFormEditorConfig) {
                            ContentCell = GetForm_EditConfig();
                        } else {
                            ContentCell = "<p>The form requested is not supported</p>";
                        }
                    } else if ((addonId != 0) | (!string.IsNullOrEmpty(AddonGuid)) | (!string.IsNullOrEmpty(AddonName))) {
                        //
                        // execute an addon
                        //
                        if ((AddonGuid == addonGuidAddonManager) || (AddonName.ToLower() == "add-on manager") || (AddonName.ToLower() == "addon manager")) {
                            //
                            // Special case, call the routine that provides a backup
                            //
                            core.doc.addRefreshQueryString("addonguid", addonGuidAddonManager);
                            ContentCell = addonController.GetAddonManager(core);
                        } else {
                            addonModel addon = null;
                            string executeContextErrorCaption = "unknown";
                            if (addonId != 0) {
                                executeContextErrorCaption = "id:" + addonId;
                                core.doc.addRefreshQueryString("addonid", addonId.ToString());
                                addon = addonModel.create(core, addonId);
                            } else if (!string.IsNullOrEmpty(AddonGuid)) {
                                executeContextErrorCaption = "guid:" + AddonGuid;
                                core.doc.addRefreshQueryString("addonguid", AddonGuid);
                                addon = addonModel.create(core, AddonGuid);
                            } else if (!string.IsNullOrEmpty(AddonName)) {
                                executeContextErrorCaption = AddonName;
                                core.doc.addRefreshQueryString("addonname", AddonName);
                                addon = addonModel.createByName(core, AddonName);
                            }
                            if (addon != null) {
                                addonId = addon.id;
                                AddonName = addon.name;
                                AddonHelpCopy = addon.Help;
                                core.doc.addRefreshQueryString(RequestNameRunAddon, addonId.ToString());
                            }
                            InstanceOptionString = core.userProperty.getText("Addon [" + AddonName + "] Options", "");
                            DefaultWrapperID = -1;
                            ContentCell = core.addon.execute(addon, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                instanceGuid = adminSiteInstanceId,
                                instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(core, InstanceOptionString),
                                wrapperID = DefaultWrapperID,
                                errorCaption = executeContextErrorCaption
                            });
                            if (string.IsNullOrEmpty(ContentCell)) {
                                //
                                // empty returned, display desktop
                                ContentCell = GetForm_Root();
                            }

                        }
                    } else {
                        //
                        // nothing so far, display desktop
                        //
                        ContentCell = GetForm_Root();
                    }
                    //
                    // include fancybox if it was needed
                    //
                    if (includeFancyBox) {
                        core.addon.executeDependency(addonModel.create(core, addonGuidjQueryFancyBox), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() { addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin });
                        //Call core.addon.execute_legacy4(addonGuidjQueryFancyBox)
                        core.html.addScriptCode_onLoad(fancyBoxHeadJS, "");
                    }
                    //
                    // Pickup user errors
                    //
                    if (core.doc.debug_iUserError != "") {
                        ContentCell = "<div class=\"ccAdminMsg\">" + errorController.getUserError(core) + "</div>" + ContentCell;
                    }
                    //
                    Stream.Add('\r' + GetForm_Top());
                    Stream.Add(genericController.htmlIndent(ContentCell));
                    Stream.Add('\r' + AdminFormBottom);
                    JavaScriptString += "\rButtonObjectCount = " + ButtonObjectCount + ";";
                    core.html.addScriptCode(JavaScriptString, "Admin Site");
                }
                result = errorController.getDocExceptionHtmlList(core) + Stream.Text;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        //   Display field in the admin/edit
        //========================================================================
        //
        //Private Function GetForm_Edit_RSSFeeds(ContentName As String, ContentID as integer, RecordID as integer, PageLink As String) As String
        //    On Error GoTo //ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.GetForm_Edit_RSSFeeds")
        //    '
        //    Dim DateExpiresText As String
        //    Dim DatePublishText As String
        //    Dim FeedEditLink As String
        //    Dim RSSFeedCID as integer
        //    Dim Caption As String
        //    Dim AttachID as integer
        //    Dim AttachName As String
        //    Dim AttachLink As String
        //    Dim CS as integer
        //    Dim HTMLFieldString As String
        //    ' converted array to dictionary - Dim FieldPointer As Integer
        //    Dim CSPointer as integer
        //    Dim CSFeeds as integer
        //    Dim Cnt as integer
        //    Dim FeedID as integer
        //    Dim s As New fastStringClass
        //    Dim Copy As String
        //    Dim Adminui As New adminUIclass(core)
        //    Dim FeedName As String
        //    Dim DefaultValue As Boolean
        //    Dim ItemID as integer
        //    Dim ItemName As String
        //    Dim ItemDescription As String
        //    Dim ItemLink As String
        //    Dim ItemDateExpires As Date
        //    Dim ItemDatePublish As Date
        //    '
        //    if true then ' 3.3.816" Then
        //        '
        //        ' Get the RSS Items (Name, etc)
        //        '
        //        CS = core.app.csOpen("RSS Feed Items", "(ContentID=" & ContentID & ")and(RecordID=" & RecordID & ")", "ID")
        //        If Not core.app.csv_IsCSOK(CS) Then
        //            '
        //            ' Default Value
        //            '
        //            ItemID = 0
        //            ItemName = ""
        //            ItemDescription = ""
        //            ItemLink = PageLink
        //            ItemDateExpires = Date.MinValue
        //            ItemDatePublish = Date.MinValue
        //        Else
        //            ItemID = core.app.cs_getInteger(CS, "ID")
        //            ItemName = core.db.cs_getText(CS, "Name")
        //            ItemDescription = core.db.cs_getText(CS, "Description")
        //            ItemLink = core.db.cs_getText(CS, "Link")
        //            ItemDateExpires = core.db.cs_getDate(CS, "DateExpires")
        //            ItemDatePublish = core.db.cs_getDate(CS, "DatePublish")
        //        End If
        //        Call core.app.closeCS(CS)
        //        '
        //        ' List out the Feeds, lookup the rules top find a match between items and feeds
        //        '
        //        RSSFeedCID = core.main_GetContentID("RSS Feeds")
        //        CSFeeds = core.app.csOpen("RSS Feeds", , "name")
        //        If core.app.csv_IsCSOK(CSFeeds) Then
        //            Cnt = 0
        //            Do While core.app.csv_IsCSOK(CSFeeds)
        //                FeedID = core.app.cs_getInteger(CSFeeds, "id")
        //                FeedName = core.db.cs_getText(CSFeeds, "name")
        //                '
        //                DefaultValue = False
        //                If ItemID <> 0 Then
        //                    CS = core.app.csOpen("RSS Feed Rules", "(RSSFeedID=" & FeedID & ")AND(RSSFeedItemID=" & ItemID & ")", , , True)
        //                    If core.app.csv_IsCSOK(CS) Then
        //                        DefaultValue = True
        //                    End If
        //                    Call core.app.closeCS(CS)
        //                End If
        //                '
        //                If Cnt = 0 Then
        //                    s.Add( "<tr><td class=""ccAdminEditCaption"">Include in RSS Feed</td>"
        //                Else
        //                    s.Add( "<tr><td class=""ccAdminEditCaption"">&nbsp;</td>"
        //                End If
        //                FeedEditLink = "[<a href=""?af=4&cid=" & RSSFeedCID & "&id=" & FeedID & """>Edit RSS Feed</a>]"
        //                s.Add( "<td class=""ccAdminEditField"">"
        //                    s.Add( "<table border=0 cellpadding=0 cellspacing=0 width=""100%"" ><tr>"
        //                    If editrecord.read_only Then
        //                        s.Add( "<td width=""50%"">" & genericController.encodeText(DefaultValue) & "&nbsp;" & FeedName & "</td>"
        //                    Else
        //                        s.Add( "<td width=""50%"">" & core.main_GetFormInputHidden("RSSFeedWas." & Cnt, DefaultValue) & core.main_GetFormInputHidden("RSSFeedID." & Cnt, FeedID) & core.main_GetFormInputCheckBox2("RSSFeed." & Cnt, DefaultValue) & FeedName & "</td>"
        //                    End If
        //                    s.Add( "<td width=""50%"">" & FeedEditLink & "</td>"
        //                    s.Add( "</tr></table>"
        //                s.Add( "</td></tr>"
        //                Call core.app.nextCSRecord(CSFeeds)
        //                Cnt = Cnt + 1
        //            Loop
        //            If Cnt = 0 Then
        //                s.Add( "<tr><td class=""ccAdminEditCaption"">Include in RSS Feed</td>"
        //            Else
        //                s.Add( "<tr><td class=""ccAdminEditCaption"">&nbsp;</td>"
        //            End If
        //            FeedEditLink = "[<a href=""?af=4&cid=" & RSSFeedCID & """>Add New RSS Feed</a>]&nbsp;[<a href=""?cid=" & RSSFeedCID & """>RSS Feeds</a>]"
        //            s.Add( "<td class=""ccAdminEditField"">"
        //                s.Add( "<table border=0 cellpadding=0 cellspacing=0 width=""100%"" ><tr>"
        //                s.Add( "<td width=""50%"">&nbsp;</td>"
        //                s.Add( "<td width=""50%"">" & FeedEditLink & "</td>"
        //                s.Add( "</tr></table>"
        //            s.Add( "</td></tr>"
        //
        //
        //        End If
        //        Call core.app.closeCS(CSFeeds)
        //        s.Add( core.main_GetFormInputHidden("RSSFeedCnt", Cnt)
        //        '
        //        ' ----- RSS Item fields
        //        '
        //        If ItemDateExpires = Date.MinValue Then
        //            DateExpiresText = ""
        //        Else
        //            DateExpiresText = CStr(ItemDateExpires)
        //        End If
        //        If ItemDatePublish = Date.MinValue Then
        //            DatePublishText = ""
        //        Else
        //            DatePublishText = CStr(ItemDatePublish)
        //        End If
        //        If editrecord.read_only Then
        //            s.Add( "<tr><td class=""ccAdminEditCaption"">Title</td><td class=""ccAdminEditField"">" & ItemName & "</td></tr>"
        //            s.Add( "<tr><td class=""ccAdminEditCaption"">Description</td><td class=""ccAdminEditField"">" & ItemDescription & "</td></tr>"
        //            s.Add( "<tr><td class=""ccAdminEditCaption"">Link</td><td class=""ccAdminEditField"">" & ItemLink & "</td></tr>"
        //            s.Add( "<tr><td class=""ccAdminEditCaption"">Publish</td><td class=""ccAdminEditField"">" & DatePublishText & "</td></tr>"
        //            s.Add( "<tr><td class=""ccAdminEditCaption"">Expire</td><td class=""ccAdminEditField"">" & DateExpiresText & "</td></tr>"
        //        Else
        //            s.Add( "<tr><td class=""ccAdminEditCaption"">Title*</td><td class=""ccAdminEditField"">" & core.main_GetFormInputText2("RSSFeedItemName", ItemName, 1, 60) & "</td></tr>"
        //            s.Add( "<tr><td class=""ccAdminEditCaption"">Description*</td><td class=""ccAdminEditField"">" & core.main_GetFormInputTextExpandable("RSSFeedItemDescription", ItemDescription, 5) & "</td></tr>"
        //            s.Add( "<tr><td class=""ccAdminEditCaption"">Link*</td><td class=""ccAdminEditField"">" & core.main_GetFormInputText2("RSSFeedItemLink", ItemLink, 1, 60) & "</td></tr>"
        //            s.Add( "<tr><td class=""ccAdminEditCaption"">Publish</td><td class=""ccAdminEditField"">" & core.main_GetFormInputDate("RSSFeedItemDatePublish", DatePublishText, 40) & "</td></tr>"
        //            s.Add( "<tr><td class=""ccAdminEditCaption"">Expire</td><td class=""ccAdminEditField"">" & core.main_GetFormInputDate("RSSFeedItemDateExpires", DateExpiresText, 40) & "</td></tr>"
        //        End If
        //        '
        //        ' ----- Add Attachements to Feeds
        //        '
        //        Caption = "Add Podcast Media Link"
        //        Cnt = 0
        //        CS = core.app.csOpen("Attachments", "(ContentID=" & ContentID & ")AND(RecordID=" & RecordID & ")", , , True)
        //        If core.app.csv_IsCSOK(CS) Then
        //            '
        //            ' ----- List all Attachements
        //            '
        //            Cnt = 0
        //            Do While core.app.csv_IsCSOK(CS)
        //
        //                AttachID = core.app.cs_getInteger(CS, "id")
        //                AttachName = core.db.cs_getText(CS, "name")
        //                AttachLink = core.db.cs_getText(CS, "link")
        //                '
        //                s.Add( "<tr><td class=""ccAdminEditCaption"">" & Caption & "</td>"
        //                If Cnt = 0 Then
        //                    Caption = "&nbsp;"
        //                End If
        //                s.Add( "<td class=""ccAdminEditField"">"
        //                    s.Add( "<table border=0 cellpadding=0 cellspacing=0 width=""100%"" ><tr>"
        //                    If editrecord.read_only Then
        //                        s.Add( "<td>" & AttachLink & "</td>"
        //                        's.Add( "<td width=""30%"">Caption " & AttachName & "</td>"
        //                    Else
        //                        s.Add( "<td>" & core.main_GetFormInputText2("AttachLink." & Cnt, AttachLink, 1, 60) & core.main_GetFormInputHidden("AttachLinkID." & Cnt, AttachID) & "</td>"
        //                        's.Add( "<td width=""30%"">Caption " & core.main_GetFormInputText2("AttachCaption." & Cnt, AttachName, 20) & "</td>"
        //                    End If
        //                    s.Add( "</tr></table>"
        //                s.Add( "<td width=""30%"">&nbsp;</td>"
        //                s.Add( "</td></tr>"
        //                Call core.app.nextCSRecord(CS)
        //                Cnt = Cnt + 1
        //            Loop
        //            End If
        //        Call core.app.closeCS(CS)
        //        '
        //        ' ----- Add Attachment link (only allow one for now)
        //        '
        //        If (Cnt = 0) And (Not editrecord.read_only) Then
        //            s.Add( "<tr><td class=""ccAdminEditCaption"">" & Caption & "</td>"
        //            s.Add( "<td class=""ccAdminEditField"">"
        //                s.Add( "<table border=0 cellpadding=0 cellspacing=0 width=""100%"" ><tr>"
        //                s.Add( "<td width=""70%"">" & core.main_GetFormInputText2("AttachLink." & Cnt, AttachLink, 1, 60) & "</td>"
        //                s.Add( "<td width=""30%"">&nbsp;</td>"
        //                s.Add( "</tr></table>"
        //            s.Add( "</td></tr>"
        //            Cnt = Cnt + 1
        //        End If
        //        s.Add( core.main_GetFormInputHidden("RSSAttachCnt", Cnt)
        //        '
        //        ' ----- add the *Required Fields footer
        //        '
        //        Call s.Add("" _
        //            & "<tr><td colspan=2 style=""padding-top:10px;font-size:70%"">" _
        //            & "<div>* Fields marked with an asterisk are required if any RSS Feed is selected.</div>" _
        //            & "</td></tr>")
        //        '
        //        ' ----- close the panel
        //        '
        //        GetForm_Edit_RSSFeeds = AdminUI.GetEditPanel( (Not AllowAdminTabs), "RSS Feeds / Podcasts", "Include in RSS Feeds / Podcasts", AdminUI.EditTableOpen & s.Text & AdminUI.EditTableClose)
        //        EditSectionPanelCount = EditSectionPanelCount + 1
        //        '
        //        s = Nothing
        //    End If
        //    '''Dim th as integer: Exit Function
        //    '
        ////ErrorTrap:
        //    s = Nothing
        //    Call HandleClassTrapErrorBubble("GetForm_Edit_RSSFeeds")
        //End Function
        //
        //========================================================================
        //   Load and Save RSS Feeds Tab
        //========================================================================
        //
        //Private Sub LoadAndSaveRSSFeeds(ContentName As String, ContentID as integer, RecordID as integer, ItemLink As String)
        //    On Error GoTo //ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.LoadAndSaveRSSFeeds")
        //    '
        //    Dim AttachID as integer
        //    Dim AttachLink As String
        //    Dim CS as integer
        //    Dim Cnt as integer
        //    Dim Ptr as integer
        //    Dim FeedChecked As Boolean
        //    Dim FeedWasChecked As Boolean
        //    Dim FeedID as integer
        //    Dim DateExpires As Date
        //    Dim RecordLink As String
        //    Dim ItemID as integer
        //    Dim ItemName As String
        //    Dim ItemDescription As String
        //    Dim ItemDateExpires As Date
        //    Dim ItemDatePublish As Date
        //    Dim FeedChanged As Boolean
        //    '
        //    ' Process Feeds
        //    '
        //    Cnt = core.main_GetStreamInteger2("RSSFeedCnt")
        //    If Cnt > 0 Then
        //        '
        //        ' Test if any feed checked -- then check Feed Item fields for required
        //        '
        //        ItemName = core.main_GetStreamText2("RSSFeedItemName")
        //        ItemDescription = core.main_GetStreamText2("RSSFeedItemDescription")
        //        ItemLink = core.main_GetStreamText2("RSSFeedItemLink")
        //        ItemDateExpires = core.main_GetStreamDate("RSSFeedItemDateExpires")
        //        ItemDatePublish = core.main_GetStreamDate("RSSFeedItemDatePublish")
        //        For Ptr = 0 To Cnt - 1
        //            FeedChecked = core.main_GetStreamBoolean2("RSSFeed." & Ptr)
        //            If FeedChecked Then
        //                Exit For
        //            End If
        //        Next
        //        If FeedChecked Then
        //            '
        //            ' check required fields
        //            '
        //            If Trim(ItemName) = "" Then
        //                Call core.htmldoc.main_AddUserError("In the RSS/Podcasts tab, a Title is required if any RSS Feed is checked.")
        //            End If
        //            If Trim(ItemDescription) = "" Then
        //                Call core.htmldoc.main_AddUserError("In the RSS/Podcasts tab, a Description is required if any RSS Feed is checked.")
        //            End If
        //            If Trim(ItemLink) = "" Then
        //                Call core.htmldoc.main_AddUserError("In the RSS/Podcasts tab, a Link is required if any RSS Feed is checked.")
        //            End If
        //        End If
        //        If FeedChecked Or (ItemName <> "") Or (ItemDescription <> "") Or (ItemLink <> "") Then
        //            '
        //            '
        //            '
        //            CS = core.app.csOpen("RSS Feed Items", "(ContentID=" & ContentID & ")and(RecordID=" & RecordID & ")", "ID")
        //            If Not core.app.csv_IsCSOK(CS) Then
        //                Call core.app.closeCS(CS)
        //                CS = core.main_InsertCSContent("RSS Feed Items")
        //            End If
        //            If ItemDatePublish = Date.MinValue Then
        //                ItemDatePublish = nt(core.main_PageStartTime.toshortdateString
        //            End If
        //            If core.app.csv_IsCSOK(CS) Then
        //                ItemID = core.app.cs_getInteger(CS, "ID")
        //                Call core.app.SetCS(CS, "ContentID", ContentID)
        //                Call core.app.SetCS(CS, "RecordID", RecordID)
        //                Call core.app.SetCS(CS, "Name", ItemName)
        //                Call core.app.SetCS(CS, "Description", ItemDescription)
        //                Call core.app.SetCS(CS, "Link", ItemLink)
        //                Call core.app.SetCS(CS, "DateExpires", ItemDateExpires)
        //                Call core.app.SetCS(CS, "DatePublish", ItemDatePublish)
        //            End If
        //            Call core.app.closeCS(CS)
        //            FeedChanged = True
        //        End If
        //        '
        //        ' ----- Now process the RSS Feed checkboxes
        //        '
        //        For Ptr = 0 To Cnt - 1
        //            FeedChecked = core.main_GetStreamBoolean2("RSSFeed." & Ptr)
        //            FeedWasChecked = core.main_GetStreamBoolean2("RSSFeedWas." & Ptr)
        //            FeedID = core.main_GetStreamInteger2("RSSFeedID." & Ptr)
        //            If FeedChecked And Not FeedWasChecked Then
        //                '
        //                ' Create rule
        //                '
        //                CS = core.main_InsertCSContent("RSS Feed Rules")
        //                If core.app.csv_IsCSOK(CS) Then
        //                    Call core.app.SetCS(CS, "Name", "RSS Feed for " & EditRecord.Name)
        //                    Call core.app.SetCS(CS, "RSSFeedID", FeedID)
        //                    Call core.app.SetCS(CS, "RSSFeedItemID", ItemID)
        //                End If
        //                Call core.app.closeCS(CS)
        //            ElseIf FeedWasChecked And Not FeedChecked Then
        //                '
        //                ' Delete Rule
        //                '
        //                FeedID = core.main_GetStreamInteger2("RSSFeedID." & Ptr)
        //                Call core.app.DeleteContentRecords("RSS Feed Rules", "(RSSFeedID=" & FeedID & ")and(ItemContentID=" & ContentID & ")and(RSSFeedItemID=" & ItemID & ")")
        //            End If
        //        Next
        //    End If
        //    '
        //    ' Attachments
        //    '
        //    Cnt = core.main_GetStreamInteger2("RSSAttachCnt")
        //    If Cnt > 0 Then
        //        For Ptr = 0 To Cnt - 1
        //            AttachID = core.main_GetStreamInteger2("AttachLinkID." & Ptr)
        //            AttachLink = core.main_GetStreamText2("AttachLink." & Ptr)
        //            If AttachID <> 0 And AttachLink <> "" Then
        //                '
        //                ' Update Attachment
        //                '
        //                CS = core.main_OpenCSContentRecord("Attachments", AttachID)
        //                If core.app.csv_IsCSOK(CS) Then
        //                    Call core.app.SetCS(CS, "Name", "Podcast attachment for " & EditRecord.Name)
        //                    Call core.app.SetCS(CS, "Link", AttachLink)
        //                    Call core.app.SetCS(CS, "ContentID", ContentID)
        //                    Call core.app.SetCS(CS, "RecordID", RecordID)
        //                End If
        //                Call core.app.closeCS(CS)
        //                FeedChanged = True
        //            ElseIf AttachID = 0 And AttachLink <> "" Then
        //                '
        //                ' Create Attachment
        //                '
        //                CS = core.main_InsertCSContent("Attachments")
        //                If core.app.csv_IsCSOK(CS) Then
        //                    Call core.app.SetCS(CS, "Name", "Podcast attachment for " & EditRecord.Name)
        //                    Call core.app.SetCS(CS, "Link", AttachLink)
        //                    Call core.app.SetCS(CS, "AttachContentID", ContentID)
        //                    Call core.app.SetCS(CS, "AttachRecordID", RecordID)
        //                End If
        //                Call core.app.closeCS(CS)
        //                FeedChanged = True
        //            ElseIf AttachID <> 0 And AttachLink = "" Then
        //                '
        //                ' delete attachment
        //                '
        //                Call core.app.DeleteContentRecords("Attachments", "(AttachContentID=" & ContentID & ")and(AttachRecordID=" & RecordID & ")")
        //                FeedChanged = True
        //            End If
        //        Next
        //    End If
        //    '
        //    '
        //    '
        //    If FeedChanged Then
        //Dim Cmd As String
        //        Cmd = getAppPath() & "\ccProcessRSS.exe"
        //        Call Shell(Cmd)
        //    End If
        //
        //    '
        //    '''Dim th as integer: Exit Sub
        //    '
        //    ' ----- Error Trap
        //    '
        ////ErrorTrap:
        //    Call HandleClassTrapErrorBubble("LoadAndSaveRSSFeeds")
        //    '
        //End Sub
        //
        //
        //========================================================================
        //
        //========================================================================
        //
        private string GetForm_ClearCache() {
            string returnHtml = "";
            try {
                stringBuilderLegacyController Content = new stringBuilderLegacyController();
                string Button = null;
                adminUIController Adminui = new adminUIController(core);
                string Description = null;
                string ButtonList = null;
                //
                Button = core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    //
                    // Cancel just exits with no content
                    //
                    return "";
                } else if (!core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                    //
                    // Not Admin Error
                    //
                    ButtonList = ButtonCancel;
                    Content.Add(Adminui.GetFormBodyAdminOnly());
                } else {
                    Content.Add(Adminui.EditTableOpen);
                    //
                    // Set defaults
                    //
                    //
                    // Process Requests
                    //
                    switch (Button) {
                        case ButtonApply:
                        case ButtonOK:
                            //
                            // Clear the cache
                            //
                            core.cache.invalidateAll();
                            break;
                    }
                    if (Button == ButtonOK) {
                        //
                        // Exit on OK or cancel
                        //
                        return "";
                    }
                    //
                    // Buttons
                    //
                    ButtonList = ButtonCancel + "," + ButtonApply + "," + ButtonOK;
                    //
                    // Close Tables
                    //
                    Content.Add(Adminui.EditTableClose);
                    Content.Add(core.html.inputHidden(rnAdminSourceForm, AdminFormClearCache));
                }
                //
                Description = "Hit Apply or OK to clear all current content caches";
                returnHtml = Adminui.GetBody("Clear Cache", ButtonList, "", true, true, Description, "", 0, Content.Text);
                Content = null;
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnHtml;
        }
        //
        //========================================================================
        // Tool to enter multiple Meta Keywords
        //========================================================================
        //
        private string GetForm_MetaKeywordTool() {
            string tempGetForm_MetaKeywordTool = null;
            try {
                stringBuilderLegacyController Content = new stringBuilderLegacyController();
                string Copy = null;
                string Button = null;
                adminUIController Adminui = new adminUIController(core);
                string Description = null;
                string ButtonList = null;
                string KeywordList = null;
                //
                Button = core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    //
                    // Cancel just exits with no content
                    //
                    return tempGetForm_MetaKeywordTool;
                } else if (!core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                    //
                    // Not Admin Error
                    //
                    ButtonList = ButtonCancel;
                    Content.Add(Adminui.GetFormBodyAdminOnly());
                } else {
                    Content.Add(Adminui.EditTableOpen);
                    //
                    // Process Requests
                    //
                    switch (Button) {
                        case ButtonSave:
                        case ButtonOK:
                            //
                            string[] Keywords = null;
                            string Keyword = null;
                            int Cnt = 0;
                            int Ptr = 0;
                            DataTable dt = null;
                            int CS = 0;
                            KeywordList = core.docProperties.getText("KeywordList");
                            if (!string.IsNullOrEmpty(KeywordList)) {
                                KeywordList = genericController.vbReplace(KeywordList, "\r\n", ",");
                                Keywords = KeywordList.Split(',');
                                Cnt = Keywords.GetUpperBound(0) + 1;
                                for (Ptr = 0; Ptr < Cnt; Ptr++) {
                                    Keyword = Keywords[Ptr].Trim(' ');
                                    if (!string.IsNullOrEmpty(Keyword)) {
                                        //Dim dt As DataTable

                                        dt = core.db.executeQuery("select top 1 ID from ccMetaKeywords where name=" + core.db.encodeSQLText(Keyword));
                                        if (dt.Rows.Count == 0) {
                                            CS = core.db.csInsertRecord("Meta Keywords");
                                            if (core.db.csOk(CS)) {
                                                core.db.csSet(CS, "name", Keyword);
                                            }
                                            core.db.csClose(ref CS);
                                        }
                                    }
                                }
                            }
                            break;
                    }
                    if (Button == ButtonOK) {
                        //
                        // Exit on OK or cancel
                        //
                        return tempGetForm_MetaKeywordTool;
                    }
                    //
                    // KeywordList
                    //
                    Copy = core.html.inputTextExpandable("KeywordList", "", 10);
                    Copy += "<div>Paste your Meta Keywords into this text box, separated by either commas or enter keys. When you hit Save or OK, Meta Keyword records will be made out of each word. These can then be checked on any content page.</div>";
                    Content.Add(Adminui.GetEditRow(Copy, "Paste Meta Keywords", "", false, false, ""));
                    //
                    // Buttons
                    //
                    ButtonList = ButtonCancel + "," + ButtonSave + "," + ButtonOK;
                    //
                    // Close Tables
                    //
                    Content.Add(Adminui.EditTableClose);
                    Content.Add(core.html.inputHidden(rnAdminSourceForm, AdminFormSecurityControl));
                }
                //
                Description = "Use this tool to enter multiple Meta Keywords";
                tempGetForm_MetaKeywordTool = Adminui.GetBody("Meta Keyword Entry Tool", ButtonList, "", true, true, Description, "", 0, Content.Text);
                Content = null;
                //
                ///Dim th as integer: Exit Function
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_MetaKeywordTool;
        }
        //
        //
        //
        private bool AllowAdminFieldCheck() {
            if (!AllowAdminFieldCheck_LocalLoaded) {
                AllowAdminFieldCheck_LocalLoaded = true;
                AllowAdminFieldCheck_Local = (core.siteProperties.getBoolean("AllowAdminFieldCheck", true));
            }
            return AllowAdminFieldCheck_Local;
        }
        //
        //
        //
        private string GetAddonHelp(int HelpAddonID, string UsedIDString) {
            string addonHelp = "";
            try {
                string IconFilename = null;
                int IconWidth = 0;
                int IconHeight = 0;
                int IconSprites = 0;
                bool IconIsInline = false;
                int CS = 0;
                string AddonName = "";
                string AddonHelpCopy = "";
                DateTime AddonDateAdded = default(DateTime);
                DateTime AddonLastUpdated = default(DateTime);
                string SQL = null;
                string IncludeHelp = "";
                int IncludeID = 0;
                string IconImg = "";
                string helpLink = "";
                bool FoundAddon = false;
                //
                if (genericController.vbInstr(1, "," + UsedIDString + ",", "," + HelpAddonID.ToString() + ",") == 0) {
                    CS = core.db.csOpenRecord(cnAddons, HelpAddonID);
                    if (core.db.csOk(CS)) {
                        FoundAddon = true;
                        AddonName = core.db.csGet(CS, "Name");
                        AddonHelpCopy = core.db.csGet(CS, "help");
                        AddonDateAdded = core.db.csGetDate(CS, "dateadded");
                        if (cdefModel.isContentFieldSupported(core, cnAddons, "lastupdated")) {
                            AddonLastUpdated = core.db.csGetDate(CS, "lastupdated");
                        }
                        if (AddonLastUpdated == DateTime.MinValue) {
                            AddonLastUpdated = AddonDateAdded;
                        }
                        IconFilename = core.db.csGet(CS, "Iconfilename");
                        IconWidth = core.db.csGetInteger(CS, "IconWidth");
                        IconHeight = core.db.csGetInteger(CS, "IconHeight");
                        IconSprites = core.db.csGetInteger(CS, "IconSprites");
                        IconIsInline = core.db.csGetBoolean(CS, "IsInline");
                        IconImg = genericController.GetAddonIconImg("/" + core.appConfig.adminRoute, IconWidth, IconHeight, IconSprites, IconIsInline, "", IconFilename, core.appConfig.cdnFilesNetprefix, AddonName, AddonName, "", 0);
                        helpLink = core.db.csGet(CS, "helpLink");
                    }
                    core.db.csClose(ref CS);
                    //
                    if (FoundAddon) {
                        //
                        // Included Addons
                        //
                        SQL = "select IncludedAddonID from ccAddonIncludeRules where AddonID=" + HelpAddonID;
                        CS = core.db.csOpenSql_rev("default", SQL);
                        while (core.db.csOk(CS)) {
                            IncludeID = core.db.csGetInteger(CS, "IncludedAddonID");
                            IncludeHelp = IncludeHelp + GetAddonHelp(IncludeID, HelpAddonID + "," + IncludeID.ToString());
                            core.db.csGoNext(CS);
                        }
                        core.db.csClose(ref CS);
                        //
                        if (!string.IsNullOrEmpty(helpLink)) {
                            if (!string.IsNullOrEmpty(AddonHelpCopy)) {
                                AddonHelpCopy = AddonHelpCopy + "<p>For additional help with this add-on, please visit <a href=\"" + helpLink + "\">" + helpLink + "</a>.</p>";
                            } else {
                                AddonHelpCopy = AddonHelpCopy + "<p>For help with this add-on, please visit <a href=\"" + helpLink + "\">" + helpLink + "</a>.</p>";
                            }
                        }
                        if (string.IsNullOrEmpty(AddonHelpCopy)) {
                            AddonHelpCopy = AddonHelpCopy + "<p>Please refer to the help resources available for this collection. More information may also be available in the Contensive online Learning Center <a href=\"http://support.contensive.com/Learning-Center\">http://support.contensive.com/Learning-Center</a> or contact Contensive Support support@contensive.com for more information.</p>";
                        }
                        addonHelp = ""
                            + "<div class=\"ccHelpCon\">"
                            + "<div class=\"title\"><div style=\"float:right;\"><a href=\"?addonid=" + HelpAddonID + "\">" + IconImg + "</a></div>" + AddonName + " Add-on</div>"
                            + "<div class=\"byline\">"
                                + "<div>Installed " + AddonDateAdded + "</div>"
                                + "<div>Last Updated " + AddonLastUpdated + "</div>"
                            + "</div>"
                            + "<div class=\"body\" style=\"clear:both;\">" + AddonHelpCopy + "</div>"
                            + "</div>";
                        addonHelp = addonHelp + IncludeHelp;
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return addonHelp;
        }
        //
        //
        //
        private string GetCollectionHelp(int HelpCollectionID, string UsedIDString) {
            string returnHelp = "";
            try {
                int CS = 0;
                string Collectionname = "";
                string CollectionHelpCopy = "";
                string CollectionHelpLink = "";
                DateTime CollectionDateAdded = default(DateTime);
                DateTime CollectionLastUpdated = default(DateTime);
                string IncludeHelp = "";
                //
                if (genericController.vbInstr(1, "," + UsedIDString + ",", "," + HelpCollectionID.ToString() + ",") == 0) {
                    CS = core.db.csOpenRecord("Add-on Collections", HelpCollectionID);
                    if (core.db.csOk(CS)) {
                        Collectionname = core.db.csGet(CS, "Name");
                        CollectionHelpCopy = core.db.csGet(CS, "help");
                        CollectionDateAdded = core.db.csGetDate(CS, "dateadded");
                        if (cdefModel.isContentFieldSupported(core, "Add-on Collections", "lastupdated")) {
                            CollectionLastUpdated = core.db.csGetDate(CS, "lastupdated");
                        }
                        if (cdefModel.isContentFieldSupported(core, "Add-on Collections", "helplink")) {
                            CollectionHelpLink = core.db.csGet(CS, "helplink");
                        }
                        if (CollectionLastUpdated == DateTime.MinValue) {
                            CollectionLastUpdated = CollectionDateAdded;
                        }
                    }
                    core.db.csClose(ref CS);
                    //
                    // Add-ons
                    //
                    CS = core.db.csOpen(cnAddons, "CollectionID=" + HelpCollectionID, "name");
                    while (core.db.csOk(CS)) {
                        IncludeHelp = IncludeHelp + "<div style=\"clear:both;\">" + GetAddonHelp(core.db.csGetInteger(CS, "ID"), "") + "</div>";
                        core.db.csGoNext(CS);
                    }
                    core.db.csClose(ref CS);
                    //
                    if ((string.IsNullOrEmpty(CollectionHelpLink)) && (string.IsNullOrEmpty(CollectionHelpCopy))) {
                        CollectionHelpCopy = "<p>No help information could be found for this collection. Please use the online resources at <a href=\"http://support.contensive.com/Learning-Center\">http://support.contensive.com/Learning-Center</a> or contact Contensive Support support@contensive.com by email.</p>";
                    } else if (!string.IsNullOrEmpty(CollectionHelpLink)) {
                        CollectionHelpCopy = ""
                            + "<p>For information about this collection please visit <a href=\"" + CollectionHelpLink + "\">" + CollectionHelpLink + "</a>.</p>"
                            + CollectionHelpCopy;
                    }
                    //
                    returnHelp = ""
                        + "<div class=\"ccHelpCon\">"
                        + "<div class=\"title\">" + Collectionname + " Collection</div>"
                        + "<div class=\"byline\">"
                            + "<div>Installed " + CollectionDateAdded + "</div>"
                            + "<div>Last Updated " + CollectionLastUpdated + "</div>"
                        + "</div>"
                        + "<div class=\"body\">" + CollectionHelpCopy + "</div>";
                    if (!string.IsNullOrEmpty(IncludeHelp)) {
                        returnHelp = returnHelp + IncludeHelp;
                    }
                    returnHelp = returnHelp + "</div>";
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnHelp;
        }
        //
        //
        //
        private void SetIndexSQL(cdefModel adminContent, editRecordClass editRecord, indexConfigClass IndexConfig, ref bool Return_AllowAccess, ref string return_sqlFieldList, ref string return_sqlFrom, ref string return_SQLWhere, ref string return_SQLOrderBy, ref bool return_IsLimitedToSubContent, ref string return_ContentAccessLimitMessage, ref Dictionary<string, bool> FieldUsedInColumns, Dictionary<string, bool> IsLookupFieldValid) {
            try {
                string LookupQuery = null;
                string ContentName = null;
                string SortFieldName = null;
                //
                int LookupPtr = 0;
                string[] lookups = null;
                string FindWordName = null;
                string FindWordValue = null;
                int FindMatchOption = 0;
                int WCount = 0;
                string SubContactList = "";
                int ContentID = 0;
                int Pos = 0;
                int Cnt = 0;
                string[] ListSplit = null;
                int SubContentCnt = 0;
                string list = null;
                string SubQuery = null;
                int GroupID = 0;
                string GroupName = null;
                string JoinTablename = null;
                //Dim FieldName As String
                int Ptr = 0;
                bool IncludedInLeftJoin = false;
                //  Dim SupportWorkflowFields As Boolean
                int FieldPtr = 0;
                bool IncludedInColumns = false;
                string LookupContentName = null;
                //Dim arrayOfFields() As appServices_metaDataClass.CDefFieldClass
                //
                Return_AllowAccess = true;
                //
                // ----- Workflow Fields
                //
                return_sqlFieldList = return_sqlFieldList + adminContent.ContentTableName + ".ID";
                //
                // ----- From Clause - build joins for Lookup fields in columns, in the findwords, and in sorts
                //
                return_sqlFrom = adminContent.ContentTableName;
                foreach (KeyValuePair<string, cdefFieldModel> keyValuePair in adminContent.fields) {
                    cdefFieldModel field = keyValuePair.Value;
                    FieldPtr = field.id; // quick fix for a replacement for the old fieldPtr (so multiple for loops will always use the same "table"+ptr string
                    IncludedInColumns = false;
                    IncludedInLeftJoin = false;
                    if (!IsLookupFieldValid.ContainsKey(field.nameLc)) {
                        IsLookupFieldValid.Add(field.nameLc, false);
                    }
                    if (!FieldUsedInColumns.ContainsKey(field.nameLc)) {
                        FieldUsedInColumns.Add(field.nameLc, false);
                    }
                    //
                    // test if this field is one of the columns we are displaying
                    //
                    IncludedInColumns = IndexConfig.Columns.ContainsKey(field.nameLc);
                    //
                    // disallow IncludedInColumns if a non-supported field type
                    //
                    switch (field.fieldTypeId) {
                        case FieldTypeIdFileCSS:
                        case FieldTypeIdFile:
                        case FieldTypeIdFileImage:
                        case FieldTypeIdFileJavascript:
                        case FieldTypeIdLongText:
                        case FieldTypeIdManyToMany:
                        case FieldTypeIdRedirect:
                        case FieldTypeIdFileText:
                        case FieldTypeIdFileXML:
                        case FieldTypeIdHTML:
                        case FieldTypeIdFileHTML:
                            IncludedInColumns = false;
                            break;
                    }
                    //FieldName = genericController.vbLCase(.Name)
                    if ((field.fieldTypeId == FieldTypeIdMemberSelect) || ((field.fieldTypeId == FieldTypeIdLookup) && (field.lookupContentID != 0))) {
                        //
                        // This is a lookup field -- test if IncludedInLeftJoins
                        //
                        JoinTablename = "";
                        if (field.fieldTypeId == FieldTypeIdMemberSelect) {
                            LookupContentName = "people";
                        } else {
                            LookupContentName = cdefModel.getContentNameByID(core, field.lookupContentID);
                        }
                        if (!string.IsNullOrEmpty(LookupContentName)) {
                            JoinTablename = cdefModel.getContentTablename(core, LookupContentName);
                        }
                        IncludedInLeftJoin = IncludedInColumns;
                        if (IndexConfig.FindWords.Count > 0) {
                            //
                            // test findwords
                            //
                            if (IndexConfig.FindWords.ContainsKey(field.nameLc)) {
                                if (IndexConfig.FindWords[field.nameLc].MatchOption != FindWordMatchEnum.MatchIgnore) {
                                    IncludedInLeftJoin = true;
                                }
                            }
                        }
                        if ((!IncludedInLeftJoin) && IndexConfig.Sorts.Count > 0) {
                            //
                            // test sorts
                            //
                            if (IndexConfig.Sorts.ContainsKey(field.nameLc.ToLower())) {
                                IncludedInLeftJoin = true;
                            }
                        }
                        if (IncludedInLeftJoin) {
                            //
                            // include this lookup field
                            //
                            FieldUsedInColumns[field.nameLc] = true;
                            if (!string.IsNullOrEmpty(JoinTablename)) {
                                IsLookupFieldValid[field.nameLc] = true;
                                return_sqlFieldList = return_sqlFieldList + ", LookupTable" + FieldPtr + ".Name AS LookupTable" + FieldPtr + "Name";
                                return_sqlFrom = "(" + return_sqlFrom + " LEFT JOIN " + JoinTablename + " AS LookupTable" + FieldPtr + " ON " + adminContent.ContentTableName + "." + field.nameLc + " = LookupTable" + FieldPtr + ".ID)";
                            }
                            //End If
                        }
                    }
                    if (IncludedInColumns) {
                        //
                        // This field is included in the columns, so include it in the select
                        //
                        return_sqlFieldList = return_sqlFieldList + " ," + adminContent.ContentTableName + "." + field.nameLc;
                        FieldUsedInColumns[field.nameLc] = true;
                    }
                }
                //
                // Sub CDef filter
                //
                if (IndexConfig.SubCDefID > 0) {
                    ContentName = cdefModel.getContentNameByID(core, IndexConfig.SubCDefID);
                    return_SQLWhere += "AND(" + cdefModel.getContentControlCriteria(core, ContentName) + ")";
                }
                //
                // Return_sqlFrom and Where Clause for Groups filter
                //
                DateTime rightNow = DateTime.Now;
                string sqlRightNow = core.db.encodeSQLDate(rightNow);
                if (adminContent.ContentTableName.ToLower() == "ccmembers") {
                    if (IndexConfig.GroupListCnt > 0) {
                        for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++) {
                            GroupName = IndexConfig.GroupList[Ptr];
                            if (!string.IsNullOrEmpty(GroupName)) {
                                GroupID = core.db.getRecordID("Groups", GroupName);
                                if (GroupID == 0 && GroupName.IsNumeric()) {
                                    GroupID = genericController.encodeInteger(GroupName);
                                }
                                string groupTableAlias = "GroupFilter" + Ptr;
                                return_SQLWhere += "AND(" + groupTableAlias + ".GroupID=" + GroupID + ")and((" + groupTableAlias + ".dateExpires is null)or(" + groupTableAlias + ".dateExpires>" + sqlRightNow + "))";
                                return_sqlFrom = "(" + return_sqlFrom + " INNER JOIN ccMemberRules AS GroupFilter" + Ptr + " ON GroupFilter" + Ptr + ".MemberID=ccMembers.ID)";
                                //Return_sqlFrom = "(" & Return_sqlFrom & " INNER JOIN ccMemberRules AS GroupFilter" & Ptr & " ON GroupFilter" & Ptr & ".MemberID=ccmembers.ID)"
                            }
                        }
                    }
                }
                //
                // Add Name into Return_sqlFieldList
                //
                //If Not SQLSelectIncludesName Then
                // SQLSelectIncludesName is declared, but not initialized
                return_sqlFieldList = return_sqlFieldList + " ," + adminContent.ContentTableName + ".Name";
                //End If
                //
                // paste sections together and do where clause
                //
                if (userHasContentAccess(adminContent.Id)) {
                    //
                    // This person can see all the records
                    //
                    return_SQLWhere += "AND(" + cdefModel.getContentControlCriteria(core, adminContent.Name) + ")";
                } else {
                    //
                    // Limit the Query to what they can see
                    //
                    return_IsLimitedToSubContent = true;
                    SubQuery = "";
                    list = adminContent.ContentControlCriteria;
                    adminContent.Id = adminContent.Id;
                    SubContentCnt = 0;
                    if (!string.IsNullOrEmpty(list)) {
                        Console.WriteLine("console - adminContent.contentControlCriteria=" + list);
                        ////Debug.WriteLine("debug - adminContent.contentControlCriteria=" + list);
                        logController.appendLog(core, "appendlog - adminContent.contentControlCriteria=" + list);
                        ListSplit = list.Split('=');
                        Cnt = ListSplit.GetUpperBound(0) + 1;
                        if (Cnt > 0) {
                            for (Ptr = 0; Ptr < Cnt; Ptr++) {
                                Pos = genericController.vbInstr(1, ListSplit[Ptr], ")");
                                if (Pos > 0) {
                                    ContentID = genericController.encodeInteger(ListSplit[Ptr].Left(Pos - 1));
                                    if (ContentID > 0 && (ContentID != adminContent.Id) & userHasContentAccess(ContentID)) {
                                        SubQuery = SubQuery + "OR(" + adminContent.ContentTableName + ".ContentControlID=" + ContentID + ")";
                                        return_ContentAccessLimitMessage = return_ContentAccessLimitMessage + ", '<a href=\"?cid=" + ContentID + "\">" + cdefModel.getContentNameByID(core, ContentID) + "</a>'";
                                        SubContactList += "," + ContentID;
                                        SubContentCnt = SubContentCnt + 1;
                                    }
                                }
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(SubQuery)) {
                        //
                        // Person has no access
                        //
                        Return_AllowAccess = false;
                        return;
                    } else {
                        return_SQLWhere += "AND(" + SubQuery.Substring(2) + ")";
                        return_ContentAccessLimitMessage = "Your access to " + adminContent.Name + " is limited to Sub-content(s) " + return_ContentAccessLimitMessage.Substring(2);
                    }
                }
                //
                // Where Clause: Active Only
                //
                if (IndexConfig.ActiveOnly) {
                    return_SQLWhere += "AND(" + adminContent.ContentTableName + ".active<>0)";
                }
                //
                // Where Clause: edited by me
                //
                if (IndexConfig.LastEditedByMe) {
                    return_SQLWhere += "AND(" + adminContent.ContentTableName + ".ModifiedBy=" + core.doc.sessionContext.user.id + ")";
                }
                //
                // Where Clause: edited today
                //
                if (IndexConfig.LastEditedToday) {
                    return_SQLWhere += "AND(" + adminContent.ContentTableName + ".ModifiedDate>=" + core.db.encodeSQLDate(core.doc.profileStartTime.Date) + ")";
                }
                //
                // Where Clause: edited past week
                //
                if (IndexConfig.LastEditedPast7Days) {
                    return_SQLWhere += "AND(" + adminContent.ContentTableName + ".ModifiedDate>=" + core.db.encodeSQLDate(core.doc.profileStartTime.Date.AddDays(-7)) + ")";
                }
                //
                // Where Clause: edited past month
                //
                if (IndexConfig.LastEditedPast30Days) {
                    return_SQLWhere += "AND(" + adminContent.ContentTableName + ".ModifiedDate>=" + core.db.encodeSQLDate(core.doc.profileStartTime.Date.AddDays(-30)) + ")";
                }
                //
                // Where Clause: Where Pairs
                //
                for (WCount = 0; WCount <= 9; WCount++) {
                    if (!string.IsNullOrEmpty(WherePair[1, WCount])) {
                        //
                        // Verify that the fieldname called out is in this table
                        //
                        if (adminContent.fields.Count > 0) {
                            foreach (KeyValuePair<string, cdefFieldModel> keyValuePair in adminContent.fields) {
                                cdefFieldModel field = keyValuePair.Value;
                                if (genericController.vbUCase(field.nameLc) == genericController.vbUCase(WherePair[0, WCount])) {
                                    //
                                    // found it, add it in the sql
                                    //
                                    return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + WherePair[0, WCount] + "=";
                                    if (WherePair[1, WCount].IsNumeric()) {
                                        return_SQLWhere += WherePair[1, WCount] + ")";
                                    } else {
                                        return_SQLWhere += "'" + WherePair[1, WCount] + "')";
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                //
                // Where Clause: findwords
                //
                if (IndexConfig.FindWords.Count > 0) {
                    foreach (var kvp in IndexConfig.FindWords) {
                        indexConfigFindWordClass findword = kvp.Value;
                        FindMatchOption = (int)findword.MatchOption;
                        if (FindMatchOption != (int)FindWordMatchEnum.MatchIgnore) {
                            FindWordName = genericController.vbLCase(findword.Name);
                            FindWordValue = findword.Value;
                            //
                            // Get FieldType
                            //
                            if (adminContent.fields.Count > 0) {
                                foreach (KeyValuePair<string, cdefFieldModel> keyValuePair in adminContent.fields) {
                                    cdefFieldModel field = keyValuePair.Value;
                                    // quick fix for a replacement for the old fieldPtr (so multiple for loops will always use the same "table"+ptr string
                                    FieldPtr = field.id;
                                    if (genericController.vbLCase(field.nameLc) == FindWordName) {
                                        switch (field.fieldTypeId) {
                                            case FieldTypeIdAutoIdIncrement:
                                            case FieldTypeIdInteger:
                                                //
                                                // integer
                                                //
                                                int FindWordValueInteger = genericController.encodeInteger(FindWordValue);
                                                switch (FindMatchOption) {
                                                    case (int)FindWordMatchEnum.MatchEmpty:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + " is null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchNotEmpty:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + " is not null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchEquals:
                                                    case (int)FindWordMatchEnum.matchincludes:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + "=" + core.db.encodeSQLNumber(FindWordValueInteger) + ")";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchGreaterThan:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + ">" + core.db.encodeSQLNumber(FindWordValueInteger) + ")";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchLessThan:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + "<" + core.db.encodeSQLNumber(FindWordValueInteger) + ")";
                                                        break;
                                                }
                                                //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                //ORIGINAL LINE: Exit For
                                                goto ExitLabel1;

                                            case FieldTypeIdCurrency:
                                            case FieldTypeIdFloat:
                                                //
                                                // double
                                                //
                                                double FindWordValueDouble = genericController.encodeNumber(FindWordValue);
                                                switch (FindMatchOption) {
                                                    case (int)FindWordMatchEnum.MatchEmpty:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + " is null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchNotEmpty:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + " is not null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchEquals:
                                                    case (int)FindWordMatchEnum.matchincludes:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + "=" + core.db.encodeSQLNumber(FindWordValueDouble) + ")";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchGreaterThan:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + ">" + core.db.encodeSQLNumber(FindWordValueDouble) + ")";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchLessThan:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + "<" + core.db.encodeSQLNumber(FindWordValueDouble) + ")";
                                                        break;
                                                }
                                                //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                //ORIGINAL LINE: Exit For
                                                goto ExitLabel1;
                                            case FieldTypeIdFile:
                                            case FieldTypeIdFileImage:
                                                //
                                                // Date
                                                //
                                                switch (FindMatchOption) {
                                                    case (int)FindWordMatchEnum.MatchEmpty:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + " is null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchNotEmpty:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + " is not null)";
                                                        break;
                                                }
                                                //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                //ORIGINAL LINE: Exit For
                                                goto ExitLabel1;
                                            case FieldTypeIdDate:
                                                //
                                                // Date
                                                //
                                                DateTime findDate = DateTime.MinValue;
                                                if (dateController.IsDate(FindWordValue)) {
                                                    findDate = DateTime.Parse(FindWordValue);
                                                }
                                                switch (FindMatchOption) {
                                                    case (int)FindWordMatchEnum.MatchEmpty:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + " is null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchNotEmpty:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + " is not null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchEquals:
                                                    case (int)FindWordMatchEnum.matchincludes:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + "=" + core.db.encodeSQLDate(findDate) + ")";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchGreaterThan:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + ">" + core.db.encodeSQLDate(findDate) + ")";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchLessThan:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + "<" + core.db.encodeSQLDate(findDate) + ")";
                                                        break;
                                                }
                                                //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                //ORIGINAL LINE: Exit For
                                                goto ExitLabel1;
                                            case FieldTypeIdLookup:
                                            case FieldTypeIdMemberSelect:
                                                //
                                                // Lookup
                                                //
                                                if (IsLookupFieldValid[field.nameLc]) {
                                                    //
                                                    // Content Lookup
                                                    //
                                                    switch (FindMatchOption) {
                                                        case (int)FindWordMatchEnum.MatchEmpty:
                                                            return_SQLWhere += "AND(LookupTable" + FieldPtr + ".ID is null)";
                                                            break;
                                                        case (int)FindWordMatchEnum.MatchNotEmpty:
                                                            return_SQLWhere += "AND(LookupTable" + FieldPtr + ".ID is not null)";
                                                            break;
                                                        case (int)FindWordMatchEnum.MatchEquals:
                                                            return_SQLWhere += "AND(LookupTable" + FieldPtr + ".Name=" + core.db.encodeSQLText(FindWordValue) + ")";
                                                            break;
                                                        case (int)FindWordMatchEnum.matchincludes:
                                                            return_SQLWhere += "AND(LookupTable" + FieldPtr + ".Name LIKE " + core.db.encodeSQLText("%" + FindWordValue + "%") + ")";
                                                            break;
                                                    }
                                                } else if (field.lookupList != "") {
                                                    //
                                                    // LookupList
                                                    //
                                                    switch (FindMatchOption) {
                                                        case (int)FindWordMatchEnum.MatchEmpty:
                                                            return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + " is null)";
                                                            break;
                                                        case (int)FindWordMatchEnum.MatchNotEmpty:
                                                            return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + " is not null)";
                                                            break;
                                                        case (int)FindWordMatchEnum.MatchEquals:
                                                        case (int)FindWordMatchEnum.matchincludes:
                                                            lookups = field.lookupList.Split(',');
                                                            LookupQuery = "";
                                                            for (LookupPtr = 0; LookupPtr <= lookups.GetUpperBound(0); LookupPtr++) {
                                                                if (!lookups[LookupPtr].Contains(FindWordValue)) {
                                                                    LookupQuery = LookupQuery + "OR(" + adminContent.ContentTableName + "." + FindWordName + "=" + core.db.encodeSQLNumber(LookupPtr + 1) + ")";
                                                                }
                                                                //if (genericController.vbInstr(1, lookups[LookupPtr], FindWordValue, 1) != 0) {
                                                                //    LookupQuery = LookupQuery + "OR(" + adminContent.ContentTableName + "." + FindWordName + "=" + core.db.encodeSQLNumber(LookupPtr + 1) + ")";
                                                                //}
                                                            }
                                                            if (!string.IsNullOrEmpty(LookupQuery)) {
                                                                return_SQLWhere += "AND(" + LookupQuery.Substring(2) + ")";
                                                            }
                                                            break;
                                                    }
                                                }
                                                //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                //ORIGINAL LINE: Exit For
                                                goto ExitLabel1;
                                            case FieldTypeIdBoolean:
                                                //
                                                // Boolean
                                                //
                                                switch (FindMatchOption) {
                                                    case (int)FindWordMatchEnum.matchincludes:
                                                        if (genericController.encodeBoolean(FindWordValue)) {
                                                            return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + "<>0)";
                                                        } else {
                                                            return_SQLWhere += "AND((" + adminContent.ContentTableName + "." + FindWordName + "=0)or(" + adminContent.ContentTableName + "." + FindWordName + " is null))";
                                                        }
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchTrue:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + "<>0)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchFalse:
                                                        return_SQLWhere += "AND((" + adminContent.ContentTableName + "." + FindWordName + "=0)or(" + adminContent.ContentTableName + "." + FindWordName + " is null))";
                                                        break;
                                                }
                                                //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                //ORIGINAL LINE: Exit For
                                                goto ExitLabel1;
                                            default:
                                                //
                                                // Text (and the rest)
                                                //
                                                switch (FindMatchOption) {
                                                    case (int)FindWordMatchEnum.MatchEmpty:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + " is null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchNotEmpty:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + " is not null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.matchincludes:
                                                        FindWordValue = core.db.encodeSQLText(FindWordValue);
                                                        FindWordValue = FindWordValue.Substring(1, FindWordValue.Length - 2);
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + " LIKE '%" + FindWordValue + "%')";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchEquals:
                                                        return_SQLWhere += "AND(" + adminContent.ContentTableName + "." + FindWordName + "=" + core.db.encodeSQLText(FindWordValue) + ")";
                                                        break;
                                                }
                                                //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                //ORIGINAL LINE: Exit For
                                                goto ExitLabel1;
                                        }
                                        //break;
                                    }
                                }
                                ExitLabel1:;
                            }
                        }
                    }
                }
                return_SQLWhere = return_SQLWhere.Substring(3);
                //
                // SQL Order by
                //
                return_SQLOrderBy = "";
                string orderByDelim = " ";
                foreach (var kvp in IndexConfig.Sorts) {
                    indexConfigSortClass sort = kvp.Value;
                    SortFieldName = genericController.vbLCase(sort.fieldName);
                    //
                    // Get FieldType
                    //
                    if (adminContent.fields.ContainsKey(sort.fieldName)) {
                        var tempVar = adminContent.fields[sort.fieldName];
                        FieldPtr = tempVar.id; // quick fix for a replacement for the old fieldPtr (so multiple for loops will always use the same "table"+ptr string
                        if ((tempVar.fieldTypeId == FieldTypeIdLookup) && IsLookupFieldValid[sort.fieldName]) {
                            return_SQLOrderBy += orderByDelim + "LookupTable" + FieldPtr + ".Name";
                        } else {
                            return_SQLOrderBy += orderByDelim + adminContent.ContentTableName + "." + SortFieldName;
                        }
                    }
                    if (sort.direction > 1) {
                        return_SQLOrderBy = return_SQLOrderBy + " Desc";
                    }
                    orderByDelim = ",";
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================
        //   If this field has no help message, check the field with the same name from it's inherited parent
        //==============================================================================================
        //
        private void getFieldHelpMsgs(int ContentID, string FieldName, ref string return_Default, ref string return_Custom) {
            try {
                //
                string SQL = null;
                int CS = 0;
                bool Found = false;
                int ParentID = 0;
                //
                Found = false;
                SQL = "select h.HelpDefault,h.HelpCustom from ccfieldhelp h left join ccfields f on f.id=h.fieldid where f.contentid=" + ContentID + " and f.name=" + core.db.encodeSQLText(FieldName);
                CS = core.db.csOpenSql(SQL);
                if (core.db.csOk(CS)) {
                    Found = true;
                    return_Default = core.db.csGetText(CS, "helpDefault");
                    return_Custom = core.db.csGetText(CS, "helpCustom");
                }
                core.db.csClose(ref CS);
                //
                if (!Found) {
                    ParentID = 0;
                    SQL = "select parentid from cccontent where id=" + ContentID;
                    CS = core.db.csOpenSql(SQL);
                    if (core.db.csOk(CS)) {
                        ParentID = core.db.csGetInteger(CS, "parentid");
                    }
                    core.db.csClose(ref CS);
                    if (ParentID != 0) {
                        getFieldHelpMsgs(ParentID, FieldName, ref return_Default, ref return_Custom);
                    }
                }
                //
                return;
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //===========================================================================
        /// <summary>
        /// handle legacy errors in this class, v3
        /// </summary>
        /// <param name="MethodName"></param>
        /// <param name="Context"></param>
        /// <remarks></remarks>
        private void handleLegacyClassError3(string MethodName, string Context = "") {
            //
            throw (new Exception("error in method [" + MethodName + "], contect [" + Context + "]"));
            //
        }
        //
        //===========================================================================
        /// <summary>
        /// handle legacy errors in this class, v2
        /// </summary>
        /// <param name="MethodName"></param>
        /// <param name="Context"></param>
        /// <remarks></remarks>
        private void handleLegacyClassError2(string MethodName, string Context = "") {
            throw (new Exception("error in method [" + MethodName + "], Context [" + Context + "]"));
        }
        //
        //===========================================================================
        /// <summary>
        /// handle legacy errors in this class, v1
        /// </summary>
        /// <param name="MethodName"></param>
        /// <param name="ErrDescription"></param>
        /// <remarks></remarks>
        private void handleLegacyClassError(string MethodName, string ErrDescription) {
            throw (new Exception("error in method [" + MethodName + "], ErrDescription [" + ErrDescription + "]"));
        }
        //Private Sub pattern1()
        //    Dim admincontent As coreMetaDataClass.CDefClass
        //    For Each keyValuePair As KeyValuePair(Of String, coreMetaDataClass.CDefFieldClass) In admincontent.fields
        //        Dim field As coreMetaDataClass.CDefFieldClass = keyValuePair.Value
        //        '
        //    Next
        //End Sub
        //
        //====================================================================================================
        // properties
        //====================================================================================================
        //
        // ----- ccGroupRules storage for list of Content that a group can author
        //
        private struct ContentGroupRuleType {
            public int ContentID;
            //public int GroupID;
            public bool AllowAdd;
            public bool AllowDelete;
        }
        ////
        //// ----- generic id/name dictionary
        ////
        //private struct StorageType {
        //    public int Id;
        //    public string Name;
        //}
        //
        // ----- Group Rules
        //
        private struct GroupRuleType {
            public int GroupID;
            public bool AllowAdd;
            public bool AllowDelete;
        }
        //
        // ----- Used within Admin site to create fancyBox popups
        //
        private bool includeFancyBox;
        private int fancyBoxPtr;
        private string fancyBoxHeadJS;
        private const bool allowSaveBeforeDuplicate = false;
        //
        // ----- To interigate Add-on Collections to check for re-use
        //
        //private struct DeleteType {
        //    public string Name;
        //    public int ParentID;
        //}
        //private struct NavigatorType {
        //    public string Name;
        //    public string menuNameSpace;
        //}
        //private struct Collection2Type {
        //    public int AddOnCnt;
        //    public string[] AddonGuid;
        //    public string[] AddonName;
        //    public int MenuCnt;
        //    public string[] Menus;
        //    public int NavigatorCnt;
        //    public NavigatorType[] Navigators;
        //}
        //private int CollectionCnt;
        //private Collection2Type[] Collections;
        //
        // ----- Target Data Storage
        //
        private int requestedContentId;
        private int requestedRecordId;
        //Private false As Boolean    ' set if content and site support workflow authoring
        private bool BlockEditForm; // true if there was an error loading the edit record - use to block the edit form
                                    //
                                    // ----- Storage for current EditRecord, loaded in LoadEditRecord
                                    //
        public class editRecordFieldClass {
            public object dbValue;
            public object value;
        }
        //
        public class editRecordClass {
            public Dictionary<string, editRecordFieldClass> fieldsLc = new Dictionary<string, editRecordFieldClass>();
            public int id; // ID field of edit record (Record to be edited)
            public int parentID; // ParentID field of edit record (Record to be edited)
            public string nameLc; // name field of edit record
            public bool active; // active field of the edit record
            public int contentControlId; // ContentControlID of the edit record
            public string contentControlId_Name;
            public string menuHeadline; // Used for Content Watch Link Label if default
            public DateTime modifiedDate; // Used for control section display
            public int modifiedByMemberID; // =
            public DateTime dateAdded; // =
            public int createByMemberId; // =

            public int RootPageID;
            public bool SetPageNotFoundPageID;
            public bool SetLandingPageID;

            //
            public bool Loaded; // true/false - set true when the field array values are loaded
            public bool Saved; // true if edit record was saved during this page
            public bool Read_Only; // set if this record can not be edited, for various reasons
                                   //
                                   // From core.main_GetAuthoringStatus
                                   //
            public bool IsDeleted; // true means the edit record has been deleted
            public bool IsInserted; // set if Workflow authoring insert
            public bool IsModified; // record has been modified since last published
            public string LockModifiedName; // member who first edited the record
            public DateTime LockModifiedDate; // Date when member modified record
            public bool SubmitLock; // set if a submit Lock, even if the current user is admin
            public string SubmittedName; // member who submitted the record
            public DateTime SubmittedDate; // Date when record was submitted
            public bool ApproveLock; // set if an approve Lock
            public string ApprovedName; // member who approved the record
            public DateTime ApprovedDate; // Date when record was approved
                                          //
                                          // From core.main_GetAuthoringPermissions
                                          //
            public bool AllowInsert;
            public bool AllowCancel;
            public bool AllowSave;
            public bool AllowDelete;
            public bool AllowPublish;
            public bool AllowAbort;
            public bool AllowSubmit;
            public bool AllowApprove;
            //
            // From core.main_GetEditLock
            //
            public bool EditLock; // set if an edit Lock by anyone else besides the current user
            public int EditLockMemberID; // Member who edit locked the record
            public string EditLockMemberName; // Member who edit locked the record
            public DateTime EditLockExpires; // Time when the edit lock expires

        }
        //Private EditRecordValuesObject() As Object      ' Storage for Edit Record values
        //Private EditRecordDbValues() As Object         ' Storage for last values read from Defaults+Db, added b/c file fields need Db value to display
        //Private EditRecord.ID As Integer                    ' ID field of edit record (Record to be edited)
        //Private EditRecord.ParentID As Integer              ' ParentID field of edit record (Record to be edited)
        //Private EditRecord.Name As String                ' name field of edit record
        //Private EditRecord.Active As Boolean             ' active field of the edit record
        //Private EditRecord.ContentID As Integer             ' ContentControlID of the edit record
        //Private EditRecord.ContentName As String         '
        //Private EditRecord.MenuHeadline As String        ' Used for Content Watch Link Label if default
        //Private EditRecord.ModifiedDate As Date          ' Used for control section display
        //Private EditRecord.ModifiedByMemberID As Integer    '   =
        //Private EditRecord.AddedDate As Date             '   =
        //Private EditRecord.AddedByMemberID As Integer       '   =
        //Private EditRecord.ContentCategoryID As Integer
        //Private EditRecordRootPageID As Integer
        //Private EditRecord.SetPageNotFoundPageID As Boolean
        //Private EditRecord.SetLandingPageID As Boolean

        //
        //Private EditRecord.Loaded As Boolean            ' true/false - set true when the field array values are loaded
        //Private EditRecord.Saved As Boolean              ' true if edit record was saved during this page
        //Private editrecord.read_only As Boolean           ' set if this record can not be edited, for various reasons
        //
        // From core.main_GetAuthoringStatus
        //
        //Private EditRecord.IsDeleted As Boolean          ' true means the edit record has been deleted
        //Private EditRecord.IsInserted As Boolean         ' set if Workflow authoring insert
        //Private EditRecord.IsModified As Boolean         ' record has been modified since last published
        //Private EditRecord.LockModifiedName As String        ' member who first edited the record
        //Private EditRecord.LockModifiedDate As Date          ' Date when member modified record
        //Private EditRecord.SubmitLock As Boolean         ' set if a submit Lock, even if the current user is admin
        //Private EditRecord.SubmittedName As String       ' member who submitted the record
        //Private EditRecordSubmittedDate As Date         ' Date when record was submitted
        //Private EditRecord.ApproveLock As Boolean        ' set if an approve Lock
        //Private EditRecord.ApprovedName As String        ' member who approved the record
        //Private EditRecordApprovedDate As Date          ' Date when record was approved
        //
        // From core.main_GetAuthoringPermissions
        //
        //Private EditRecord.AllowInsert As Boolean
        //Private EditRecord.AllowCancel As Boolean
        //Private EditRecord.AllowSave As Boolean
        //Private EditRecord.AllowDelete As Boolean
        //Private EditRecord.AllowPublish As Boolean
        //Private EditRecord.AllowAbort As Boolean
        //Private EditRecord.AllowSubmit As Boolean
        //Private EditRecord.AllowApprove As Boolean
        //
        // From core.main_GetEditLock
        //
        //Private EditRecord.EditLock As Boolean           ' set if an edit Lock by anyone else besides the current user
        //Private EditRecord.EditLockMemberID As Integer      ' Member who edit locked the record
        //Private EditRecord.EditLockMemberName As String  ' Member who edit locked the record
        //Private EditRecord.EditLockExpires As Date       ' Time when the edit lock expires
        //
        //
        //=============================================================================
        // ----- Control Response
        //=============================================================================
        //
        private string AdminButton; // Value returned from a submit button, process into action/form
        private int AdminAction; // The action to be performed before the next form
        private int AdminForm; // The next form to print
        private int AdminSourceForm; // The form that submitted that the button to process
        private string[,] WherePair = new string[3, 11]; // for passing where clause values from page to page
        private int WherePairCount; // the current number of WherePairCount in use
                                    //Private OrderByFieldPointer as integer
        private const int OrderByFieldPointerDefault = -1;
        //Private Direction as integer
        private int RecordTop;
        private int RecordsPerPage;
        private const int RecordsPerPageDefault = 50;
        //Private InputFieldName As String   ' Input FieldName used for DHTMLEdit

        private int MenuDepth; // The number of windows open (below this one)
        private string TitleExtension; // String that adds on to the end of the title
        //
        // SpellCheck Features
        //
        private bool SpellCheckSupported; // if true, spell checking is supported
        private bool SpellCheckRequest; // If true, send the spell check form to the browser
        //
        //=============================================================================
        // preferences
        //=============================================================================
        //
        private int AdminMenuModeID; // Controls the menu mode, set from core.main_MemberAdminMenuModeID
        private bool allowAdminTabs; // true uses tab system
        private string fieldEditorPreference; // this is a hidden on the edit form. The popup editor preferences sets this hidden and submits
        //
        //=============================================================================
        //   Content Tracking Editing
        //
        //   These values are read from Edit form response, and are used to populate then
        //   ContentWatch and ContentWatchListRules records.
        //
        //   They are read in before the current record is processed, then processed and
        //   Saved back to ContentWatch and ContentWatchRules after the current record is
        //   processed, so changes to the record can be reflected in the ContentWatch records.
        //   For instance, if the record is marked inactive, the ContentWatchLink is cleared
        //   and all ContentWatchListRules are deleted.
        //
        //=============================================================================
        //
        private bool ContentWatchLoaded; // flag set that shows the rest are valid
        //
        private int ContentWatchRecordID;
        private string ContentWatchLink;
        private int ContentWatchClicks;
        private string ContentWatchLinkLabel;
        private DateTime ContentWatchExpires;
        private int[] ContentWatchListID; // list of all ContentWatchLists for this Content, read from response, then later saved to Rules
        private int ContentWatchListIDSize; // size of ContentWatchListID() array
        private int ContentWatchListIDCount; // number of valid entries in ContentWatchListID()
        //
        //=============================================================================
        // Other
        //=============================================================================
        // Count of Buttons in use
        private int ButtonObjectCount; 
        private string[,] ImagePreloads = new string[3, 101]; 
        private string JavaScriptString; // Collected string of Javascript functions to print at end
        private string AdminFormBottom; // the HTML needed to complete the Admin Form after contents
        private bool UserAllowContentEdit; 
        private int FormInputCount; // used to generate labels for form input
        private int EditSectionPanelCount;

        private const string OpenLiveWindowTable = "<div ID=\"LiveWindowTable\">";
        private const string CloseLiveWindowTable = "</div>";
        //Const OpenLiveWindowTable = "<table ID=""LiveWindowTable"" border=0 cellpadding=0 cellspacing=0 width=""100%""><tr><td>"
        //Const CloseLiveWindowTable = "</td></tr></table>"
        //
        //Const adminui.EditTableClose = "<tr>" _
        //        & "<td width=20%><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1"" ></td>" _
        //        & "<td width=""70%""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1"" ></td>" _
        //        & "<td width=""10%""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1"" ></td>" _
        //        & "</tr>" _
        //        & "</table>"
        private const string AdminFormErrorOpen = "<table border=\"0\" cellpadding=\"20\" cellspacing=\"0\" width=\"100%\"><tr><td align=\"left\">";
        private const string AdminFormErrorClose = "</td></tr></table>";
        //
        // these were defined different in csv
        //
        //Private Const ContentTypeMember = 1
        //Private Const ContentTypePaths = 2
        //Private Const csv_contenttypeenum.contentTypeEmail = 3
        //Private Const ContentTypeContent = 4
        //Private Const ContentTypeSystem = 5
        //Private Const ContentTypeNormal = 6
        //
        //
        //
        private const string RequestNameAdminDepth = "ad";
        private const string RequestNameAdminForm = "af";
        private const string rnAdminSourceForm = "asf";
        private const string rnAdminAction = "aa";
        //Private Const RequestNameFieldName = "fn"
        private const string RequestNameTitleExtension = "tx";
        //
        //
        //
        //Private AdminContentCellBackgroundColor As String
        //
        public enum NodeTypeEnum {
            NodeTypeEntry = 0,
            NodeTypeCollection = 1,
            NodeTypeAddon = 2,
            NodeTypeContent = 3
        }
        //
        private const string IndexConfigPrefix = "IndexConfig:";
        //
        public enum FindWordMatchEnum {
            MatchIgnore = 0,
            MatchEmpty = 1,
            MatchNotEmpty = 2,
            MatchGreaterThan = 3,
            MatchLessThan = 4,
            matchincludes = 5,
            MatchEquals = 6,
            MatchTrue = 7,
            MatchFalse = 8
        }
        //
        //
        //
        public class indexConfigSortClass {
            //Dim FieldPtr As Integer
            public string fieldName;
            public int direction; // 1=forward, 2=reverse, 0=ignore/remove this sort
        }
        //
        public class indexConfigFindWordClass {
            public string Name;
            public string Value;
            public int Type;
            public FindWordMatchEnum MatchOption;
        }
        //
        public class indexConfigColumnClass {
            public string Name;
            //Public FieldId As Integer
            public int Width;
            public int SortPriority;
            public int SortDirection;
        }
        //
        public class indexConfigClass {
            public bool Loaded;
            public int ContentID;
            public int PageNumber;
            public int RecordsPerPage;
            public int RecordTop;

            //FindWordList As String
            public Dictionary<string, indexConfigFindWordClass> FindWords = new Dictionary<string, indexConfigFindWordClass>();
            //Public FindWordCnt As Integer
            public bool ActiveOnly;
            public bool LastEditedByMe;
            public bool LastEditedToday;
            public bool LastEditedPast7Days;
            public bool LastEditedPast30Days;
            public bool Open;
            //public SortCnt As Integer
            public Dictionary<string, indexConfigSortClass> Sorts = new Dictionary<string, indexConfigSortClass>();
            public int GroupListCnt;
            public string[] GroupList;
            //public ColumnCnt As Integer
            public Dictionary<string, indexConfigColumnClass> Columns = new Dictionary<string, indexConfigColumnClass>();
            //SubCDefs() as integer
            //SubCDefCnt as integer
            public int SubCDefID;
        }
        //
        // Temp
        //
        private const int ToolsActionMenuMove = 1;
        private const int ToolsActionAddField = 2; // Add a field to the Index page
        private const int ToolsActionRemoveField = 3;
        private const int ToolsActionMoveFieldRight = 4;
        private const int ToolsActionMoveFieldLeft = 5;
        private const int ToolsActionSetAZ = 6;
        private const int ToolsActionSetZA = 7;
        private const int ToolsActionExpand = 8;
        private const int ToolsActionContract = 9;
        private const int ToolsActionEditMove = 10;
        private const int ToolsActionRunQuery = 11;
        private const int ToolsActionDuplicateDataSource = 12;
        private const int ToolsActionDefineContentFieldFromTableFieldsFromTable = 13;
        private const int ToolsActionFindAndReplace = 14;
        //
        private bool AllowAdminFieldCheck_Local;
        private bool AllowAdminFieldCheck_LocalLoaded;
        //
        private const string AddonGuidPreferences = "{D9C2D64E-9004-4DBE-806F-60635B9F52C8}";
        //
        //========================================================================
        //
        //========================================================================
        //
        public string admin_GetAdminFormBody(string Caption, string ButtonListLeft, string ButtonListRight, bool AllowAdd, bool AllowDelete, string Description, string ContentSummary, int ContentPadding, string Content) {
            return (new adminUIController(core)).GetBody(Caption, ButtonListLeft, ButtonListRight, AllowAdd, AllowDelete, Description, ContentSummary, ContentPadding, Content);
        }
        //
        //
        //========================================================================
        //   Print the index form, values and all
        //       creates a sql with leftjoins, and renames lookups as TableLookupxName
        //       where x is the TarGetFieldPtr of the field that is FieldTypeLookup
        //
        //   Input:
        //       AdminContent.contenttablename is required
        //       OrderByFieldPtr
        //       OrderByDirection
        //       RecordTop
        //       RecordsPerPage
        //       Findstring( ColumnPointer )
        //========================================================================
        //
        private string GetForm_Index(cdefModel adminContent, editRecordClass editRecord, bool IsEmailContent) {
            string returnForm = "";
            try {
                const string FilterClosedLabel = "<div style=\"font-size:9px;text-align:center;\">&nbsp;<br>F<br>i<br>l<br>t<br>e<br>r<br>s</div>";
                //
                string Copy = "";
                string RightCopy = null;
                int TitleRows = 0;
                // refactor -- is was using page manager code, and it only detected if the page is the current domain's 
                //Dim LandingPageID As Integer
                //Dim IsPageContent As Boolean
                //Dim IsLandingPage As Boolean
                int PageCount = 0;
                bool AllowAdd = false;
                bool AllowDelete = false;
                int recordCnt = 0;
                bool AllowAccessToContent = false;
                string ContentName = null;
                string ContentAccessLimitMessage = "";
                bool IsLimitedToSubContent = false;
                string GroupList = "";
                string[] Groups = null;
                string FieldCaption = null;
                string SubTitle = null;
                string SubTitlePart = null;
                string Title = null;
                string AjaxQS = null;
                string FilterColumn = "";
                string DataColumn = null;
                string DataTable_DataRows = "";
                string FilterDataTable = "";
                string DataTable_FindRow = "";
                string DataTable = null;
                string DataTable_HdrRow = "";
                string IndexFilterContent = "";
                string IndexFilterHead = "";
                string IndexFilterJS = "";
                bool IndexFilterOpen = false;
                indexConfigClass IndexConfig = null;
                int Ptr = 0;
                string SortTitle = null;
                string HeaderDescription = "";
                bool AllowFilterNav = false;
                int ColumnPointer = 0;
                int WhereCount = 0;
                string sqlWhere = "";
                string sqlOrderBy = "";
                string sqlFieldList = "";
                string sqlFrom = "";
                int CS = 0;
                string SQL = null;
                string RowColor = "";
                int RecordPointer = 0;
                int RecordLast = 0;
                int RecordTop_NextPage = 0;
                int RecordTop_PreviousPage = 0;
                int ColumnWidth = 0;

                string TitleBar = null;
                string FindWordValue = null;
                string ButtonObject = null;
                string ButtonFace = null;
                string ButtonHref = null;
                string URI = null;
                //Dim DataSourceName As String
                //Dim DataSourceType As Integer
                string FieldName = null;
                Dictionary<string, bool> FieldUsedInColumns = new Dictionary<string, bool>(); // used to prevent select SQL from being sorted by a field that does not appear
                int ColumnWidthTotal = 0;
                int SubForm = 0;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                int RecordID = 0;
                string RecordName = null;
                string LeftButtons = "";
                string RightButtons = "";
                adminUIController Adminui = new adminUIController(core);
                Dictionary<string, bool> IsLookupFieldValid = new Dictionary<string, bool>();
                bool allowCMEdit = false;
                bool allowCMAdd = false;
                bool allowCMDelete = false;
                //
                // --- make sure required fields are present
                //
                if (adminContent.Id == 0) {
                    //
                    // Bad content id
                    //
                    Stream.Add(GetForm_Error("This form requires a valid content definition, and one was not found for content ID [" + adminContent.Id + "].", "No content definition was specified [ContentID=0]. Please contact your application developer for more assistance."));
                } else if (string.IsNullOrEmpty(adminContent.Name)) {
                    //
                    // Bad content name
                    //
                    Stream.Add(GetForm_Error("No content definition could be found for ContentID [" + adminContent.Id + "]. This could be a menu error. Please contact your application developer for more assistance.", "No content definition for ContentID [" + adminContent.Id + "] could be found."));
                } else if (adminContent.ContentTableName == "") {
                    //
                    // No tablename
                    //
                    Stream.Add(GetForm_Error("The content definition [" + adminContent.Name + "] is not associated with a valid database table. Please contact your application developer for more assistance.", "Content [" + adminContent.Name + "] ContentTablename is empty."));
                } else if (adminContent.fields.Count == 0) {
                    //
                    // No Fields
                    //
                    Stream.Add(GetForm_Error("This content [" + adminContent.Name + "] cannot be accessed because it has no fields. Please contact your application developer for more assistance.", "Content [" + adminContent.Name + "] has no field records."));
                } else if (adminContent.DeveloperOnly & (!core.doc.sessionContext.isAuthenticatedDeveloper(core))) {
                    //
                    // Developer Content and not developer
                    //
                    Stream.Add(GetForm_Error("Access to this content [" + adminContent.Name + "] requires developer permissions. Please contact your application developer for more assistance.", "Content [" + adminContent.Name + "] has no field records."));
                } else {
                    List<string> tmp = new List<string> { };
                    dataSourceModel datasource = dataSourceModel.create(core, adminContent.dataSourceId, ref tmp);
                    //
                    // get access rights
                    //
                    core.doc.sessionContext.getContentAccessRights(core, adminContent.Name, ref allowCMEdit, ref allowCMAdd, ref allowCMDelete);
                    //
                    // detemine which subform to disaply
                    //
                    SubForm = core.docProperties.getInteger(RequestNameAdminSubForm);
                    if (SubForm != 0) {
                        switch (SubForm) {
                            case AdminFormIndex_SubFormExport:
                                Copy = GetForm_Index_Export(adminContent, editRecord);
                                break;
                            case AdminFormIndex_SubFormSetColumns:
                                Copy = GetForm_Index_SetColumns(adminContent, editRecord);
                                break;
                            case AdminFormIndex_SubFormAdvancedSearch:
                                Copy = GetForm_Index_AdvancedSearch(adminContent, editRecord);
                                break;
                        }
                    }
                    Stream.Add(Copy);
                    if (string.IsNullOrEmpty(Copy)) {
                        //
                        // If subforms return empty, go to parent form
                        //
                        AllowFilterNav = true;
                        //
                        // -- Load Index page customizations
                        IndexConfig = LoadIndexConfig(adminContent);
                        SetIndexSQL_ProcessIndexConfigRequests(adminContent, editRecord, ref IndexConfig);
                        SetIndexSQL_SaveIndexConfig(IndexConfig);
                        //
                        // Get the SQL parts
                        //
                        SetIndexSQL(adminContent, editRecord, IndexConfig, ref AllowAccessToContent, ref sqlFieldList, ref sqlFrom, ref sqlWhere, ref sqlOrderBy, ref IsLimitedToSubContent, ref ContentAccessLimitMessage, ref FieldUsedInColumns, IsLookupFieldValid);
                        if ((!allowCMEdit) || (!AllowAccessToContent)) {
                            //
                            // two conditions should be the same -- but not time to check - This user does not have access to this content
                            //
                            errorController.addUserError(core, "Your account does not have access to any records in '" + adminContent.Name + "'.");
                        } else {
                            //
                            // Get the total record count
                            //
                            SQL = "select count(" + adminContent.ContentTableName + ".ID) as cnt from " + sqlFrom;
                            if (!string.IsNullOrEmpty(sqlWhere)) {
                                SQL += " where " + sqlWhere;
                            }
                            CS = core.db.csOpenSql_rev(datasource.Name, SQL);
                            if (core.db.csOk(CS)) {
                                recordCnt = core.db.csGetInteger(CS, "cnt");
                            }
                            core.db.csClose(ref CS);
                            //
                            // Assumble the SQL
                            //
                            SQL = "select";
                            if (datasource.type != DataSourceTypeODBCMySQL) {
                                SQL += " Top " + (IndexConfig.RecordTop + IndexConfig.RecordsPerPage);
                            }
                            SQL += " " + sqlFieldList + " From " + sqlFrom;
                            if (!string.IsNullOrEmpty(sqlWhere)) {
                                SQL += " WHERE " + sqlWhere;
                            }
                            if (!string.IsNullOrEmpty(sqlOrderBy)) {
                                SQL += " Order By" + sqlOrderBy;
                            }
                            if (datasource.type == DataSourceTypeODBCMySQL) {
                                SQL += " Limit " + (IndexConfig.RecordTop + IndexConfig.RecordsPerPage);
                            }
                            //
                            // Refresh Query String
                            //
                            core.doc.addRefreshQueryString("tr", IndexConfig.RecordTop.ToString());
                            core.doc.addRefreshQueryString("asf", AdminForm.ToString());
                            core.doc.addRefreshQueryString("cid", adminContent.Id.ToString());
                            core.doc.addRefreshQueryString(RequestNameTitleExtension, genericController.EncodeRequestVariable(TitleExtension));
                            if (WherePairCount > 0) {
                                for (WhereCount = 0; WhereCount < WherePairCount; WhereCount++) {
                                    core.doc.addRefreshQueryString("wl" + WhereCount, WherePair[0, WhereCount]);
                                    core.doc.addRefreshQueryString("wr" + WhereCount, WherePair[1, WhereCount]);
                                }
                            }
                            //
                            // ----- ButtonBar
                            //
                            AllowAdd = adminContent.AllowAdd & (!IsLimitedToSubContent) && (allowCMAdd);
                            if (MenuDepth > 0) {
                                LeftButtons = LeftButtons + core.html.button(ButtonClose, "", "", "window.close();");
                            } else {
                                LeftButtons = LeftButtons + core.html.button(ButtonCancel);
                                //LeftButtons = LeftButtons & core.main_GetFormButton(ButtonCancel, , , "return processSubmit(this)")
                            }
                            if (AllowAdd) {
                                LeftButtons = LeftButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonAdd + "\">";
                                //LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonAdd & """ onClick=""return processSubmit(this);"">"
                            } else {
                                LeftButtons = LeftButtons + "<input TYPE=SUBMIT NAME=BUTTON DISABLED VALUE=\"" + ButtonAdd + "\">";
                                //LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON DISABLED VALUE=""" & ButtonAdd & """ onClick=""return processSubmit(this);"">"
                            }
                            AllowDelete = (adminContent.AllowDelete) && (allowCMDelete);
                            if (AllowDelete) {
                                LeftButtons = LeftButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonDelete + "\" onClick=\"if(!DeleteCheck())return false;\">";
                            } else {
                                LeftButtons = LeftButtons + "<input TYPE=SUBMIT NAME=BUTTON DISABLED VALUE=\"" + ButtonDelete + "\" onClick=\"if(!DeleteCheck())return false;\">";
                            }
                            if (IndexConfig.PageNumber == 1) {
                                RightButtons = RightButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonFirst + "\" DISABLED>";
                                RightButtons = RightButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonPrevious + "\" DISABLED>";
                            } else {
                                RightButtons = RightButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonFirst + "\">";
                                //RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonFirst & """ onClick=""return processSubmit(this);"">"
                                RightButtons = RightButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonPrevious + "\">";
                                //RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonPrevious & """ onClick=""return processSubmit(this);"">"
                            }
                            //RightButtons = RightButtons & core.main_GetFormButton(ButtonFirst)
                            //RightButtons = RightButtons & core.main_GetFormButton(ButtonPrevious)
                            if (recordCnt > (IndexConfig.PageNumber * IndexConfig.RecordsPerPage)) {
                                RightButtons = RightButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonNext + "\">";
                                //RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonNext & """ onClick=""return processSubmit(this);"">"
                            } else {
                                RightButtons = RightButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonNext + "\" DISABLED>";
                            }
                            RightButtons = RightButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonRefresh + "\">";
                            if (recordCnt <= 1) {
                                PageCount = 1;
                            } else {
                                PageCount = encodeInteger(1 + encodeInteger(Math.Floor(encodeNumber((recordCnt - 1) / IndexConfig.RecordsPerPage))));
                            }
                            string ButtonBar = Adminui.GetButtonBarForIndex(LeftButtons, RightButtons, IndexConfig.PageNumber, IndexConfig.RecordsPerPage, PageCount);
                            //ButtonBar = AdminUI.GetButtonBar(LeftButtons, RightButtons)
                            //
                            // ----- TitleBar
                            //
                            Title = "";
                            SubTitle = "";
                            SubTitlePart = "";
                            if (IndexConfig.ActiveOnly) {
                                SubTitle = SubTitle + ", active records";
                            }
                            SubTitlePart = "";
                            if (IndexConfig.LastEditedByMe) {
                                SubTitlePart = SubTitlePart + " by " + core.doc.sessionContext.user.name;
                            }
                            if (IndexConfig.LastEditedPast30Days) {
                                SubTitlePart = SubTitlePart + " in the past 30 days";
                            }
                            if (IndexConfig.LastEditedPast7Days) {
                                SubTitlePart = SubTitlePart + " in the week";
                            }
                            if (IndexConfig.LastEditedToday) {
                                SubTitlePart = SubTitlePart + " today";
                            }
                            if (!string.IsNullOrEmpty(SubTitlePart)) {
                                SubTitle = SubTitle + ", last edited" + SubTitlePart;
                            }
                            foreach (var kvp in IndexConfig.FindWords) {
                                indexConfigFindWordClass findWord = kvp.Value;
                                if (!string.IsNullOrEmpty(findWord.Name)) {
                                    FieldCaption = genericController.encodeText(cdefModel.GetContentFieldProperty(core, adminContent.Name, findWord.Name, "caption"));
                                    switch (findWord.MatchOption) {
                                        case FindWordMatchEnum.MatchEmpty:
                                            SubTitle = SubTitle + ", " + FieldCaption + " is empty";
                                            break;
                                        case FindWordMatchEnum.MatchEquals:
                                            SubTitle = SubTitle + ", " + FieldCaption + " = '" + findWord.Value + "'";
                                            break;
                                        case FindWordMatchEnum.MatchFalse:
                                            SubTitle = SubTitle + ", " + FieldCaption + " is false";
                                            break;
                                        case FindWordMatchEnum.MatchGreaterThan:
                                            SubTitle = SubTitle + ", " + FieldCaption + " &gt; '" + findWord.Value + "'";
                                            break;
                                        case FindWordMatchEnum.matchincludes:
                                            SubTitle = SubTitle + ", " + FieldCaption + " includes '" + findWord.Value + "'";
                                            break;
                                        case FindWordMatchEnum.MatchLessThan:
                                            SubTitle = SubTitle + ", " + FieldCaption + " &lt; '" + findWord.Value + "'";
                                            break;
                                        case FindWordMatchEnum.MatchNotEmpty:
                                            SubTitle = SubTitle + ", " + FieldCaption + " is not empty";
                                            break;
                                        case FindWordMatchEnum.MatchTrue:
                                            SubTitle = SubTitle + ", " + FieldCaption + " is true";
                                            break;
                                    }

                                }
                            }
                            if (IndexConfig.SubCDefID > 0) {
                                ContentName = cdefModel.getContentNameByID(core, IndexConfig.SubCDefID);
                                if (!string.IsNullOrEmpty(ContentName)) {
                                    SubTitle = SubTitle + ", in Sub-content '" + ContentName + "'";
                                }
                            }
                            //
                            // add groups to caption
                            //
                            if ((adminContent.ContentTableName.ToLower() == "ccmembers") && (IndexConfig.GroupListCnt > 0)) {
                                //If (LCase(AdminContent.ContentTableName) = "ccmembers") And (.GroupListCnt > 0) Then
                                SubTitlePart = "";
                                for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++) {
                                    if (IndexConfig.GroupList[Ptr] != "") {
                                        GroupList = GroupList + "\t" + IndexConfig.GroupList[Ptr];
                                    }
                                }
                                if (!string.IsNullOrEmpty(GroupList)) {
                                    Groups = GroupList.Split('\t');
                                    if (Groups.GetUpperBound(0) == 0) {
                                        SubTitle = SubTitle + ", in group '" + Groups[0] + "'";
                                    } else if (Groups.GetUpperBound(0) == 1) {
                                        SubTitle = SubTitle + ", in groups '" + Groups[0] + "' and '" + Groups[1] + "'";
                                    } else {
                                        for (Ptr = 0; Ptr < Groups.GetUpperBound(0); Ptr++) {
                                            SubTitlePart = SubTitlePart + ", '" + Groups[Ptr] + "'";
                                        }
                                        SubTitle = SubTitle + ", in groups" + SubTitlePart.Substring(1) + " and '" + Groups[Ptr] + "'";
                                    }

                                }
                            }
                            //
                            // add sort details to caption
                            //
                            SubTitlePart = "";
                            foreach (var kvp in IndexConfig.Sorts) {
                                indexConfigSortClass sort = kvp.Value;
                                if (sort.direction > 0) {
                                    SubTitlePart = SubTitlePart + " and " + adminContent.fields[sort.fieldName].caption;
                                    if (sort.direction > 1) {
                                        SubTitlePart += " reverse";
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(SubTitlePart)) {
                                SubTitle += ", sorted by" + SubTitlePart.Substring(4);
                            }
                            //
                            Title = adminContent.Name;
                            if (TitleExtension != "") {
                                Title = Title + " " + TitleExtension;
                            }
                            switch (recordCnt) {
                                case 0:
                                    RightCopy = "no records found";
                                    break;
                                case 1:
                                    RightCopy = "1 record found";
                                    break;
                                default:
                                    RightCopy = recordCnt + " records found";
                                    break;
                            }
                            RightCopy = RightCopy + ", page " + IndexConfig.PageNumber;
                            Title = "<div>"
                                + "<span style=\"float:left;\"><strong>" + Title + "</strong></span>"
                                + "<span style=\"float:right;\">" + RightCopy + "</span>"
                                + "</div>";
                            TitleRows = 0;
                            if (!string.IsNullOrEmpty(SubTitle)) {
                                Title = Title + "<div style=\"clear:both\">Filter: " + genericController.encodeHTML(SubTitle.Substring(2)) + "</div>";
                                TitleRows = TitleRows + 1;
                            }
                            if (!string.IsNullOrEmpty(ContentAccessLimitMessage)) {
                                Title = Title + "<div style=\"clear:both\">" + ContentAccessLimitMessage + "</div>";
                                TitleRows = TitleRows + 1;
                            }
                            if (TitleRows == 0) {
                                Title = Title + "<div style=\"clear:both\">&nbsp;</div>";
                            }
                            //
                            TitleBar = SpanClassAdminNormal + Title + "</span>";
                            //TitleBar = TitleBar & core.main_GetHelpLink(46, "Using the Admin Index Page", BubbleCopy_AdminIndexPage)
                            //
                            // ----- Filter Data Table
                            //
                            if (AllowFilterNav) {
                                //
                                // Filter Nav - if enabled, just add another cell to the row
                                //
                                IndexFilterOpen = core.visitProperty.getBoolean("IndexFilterOpen", false);
                                if (IndexFilterOpen) {
                                    //
                                    // Ajax Filter Open
                                    //
                                    IndexFilterHead = ""
                                        + "\r\n<div class=\"ccHeaderCon\">"
                                        + "\r\n<div id=\"IndexFilterHeCursorTypeEnum.ADOPENed\" class=\"opened\">"
                                        + "\r<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>"
                                        + "\r<td valign=Middle class=\"left\">Filters</td>"
                                        + "\r<td valign=Middle class=\"right\"><a href=\"#\" onClick=\"CloseIndexFilter();return false\"><img alt=\"Close Filters\" title=\"Close Filters\" src=\"/ccLib/images/ClosexRev1313.gif\" width=13 height=13 border=0></a></td>"
                                        + "\r</tr></table>"
                                        + "\r\n</div>"
                                        + "\r\n<div id=\"IndexFilterHeadClosed\" class=\"closed\" style=\"display:none;\">"
                                        + "\r<a href=\"#\" onClick=\"OpenIndexFilter();return false\"><img title=\"Open Navigator\" alt=\"Open Filter\" src=\"/ccLib/images/OpenRightRev1313.gif\" width=13 height=13 border=0 style=\"text-align:right;\"></a>"
                                        + "\r\n</div>"
                                        + "\r\n</div>"
                                        + "";
                                    IndexFilterContent = ""
                                        + "\r\n<div class=\"ccContentCon\">"
                                        + "\r\n<div id=\"IndexFilterContentOpened\" class=\"opened\">" + GetForm_IndexFilterContent(adminContent) + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"200\" height=\"1\" style=\"clear:both\"></div>"
                                        + "\r\n<div id=\"IndexFilterContentClosed\" class=\"closed\" style=\"display:none;\">" + FilterClosedLabel + "</div>"
                                        + "\r\n</div>";
                                    IndexFilterJS = ""
                                        + "\r\n<script Language=\"JavaScript\" type=\"text/javascript\">"
                                        + "\r\nfunction CloseIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','none');SetDisplay('IndexFilterContentOpened','none');SetDisplay('IndexFilterHeadClosed','block');SetDisplay('IndexFilterContentClosed','block');cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxCloseIndexFilter + "','','')}"
                                        + "\r\nfunction OpenIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','block');SetDisplay('IndexFilterContentOpened','block');SetDisplay('IndexFilterHeadClosed','none');SetDisplay('IndexFilterContentClosed','none');cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxOpenIndexFilter + "','','')}"
                                        + "\r\n</script>";
                                } else {
                                    //
                                    // Ajax Filter Closed
                                    //
                                    IndexFilterHead = ""
                                        + "\r\n<div class=\"ccHeaderCon\">"
                                        + "\r\n<div id=\"IndexFilterHeCursorTypeEnum.ADOPENed\" class=\"opened\" style=\"display:none;\">"
                                        + "\r<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>"
                                        + "\r<td valign=Middle class=\"left\">Filter</td>"
                                        + "\r<td valign=Middle class=\"right\"><a href=\"#\" onClick=\"CloseIndexFilter();return false\"><img alt=\"Close Filter\" title=\"Close Navigator\" src=\"/ccLib/images/ClosexRev1313.gif\" width=13 height=13 border=0></a></td>"
                                        + "\r</tr></table>"
                                        + "\r\n</div>"
                                        + "\r\n<div id=\"IndexFilterHeadClosed\" class=\"closed\">"
                                        + "\r<a href=\"#\" onClick=\"OpenIndexFilter();return false\"><img title=\"Open Navigator\" alt=\"Open Navigator\" src=\"/ccLib/images/OpenRightRev1313.gif\" width=13 height=13 border=0 style=\"text-align:right;\"></a>"
                                        + "\r\n</div>"
                                        + "\r\n</div>"
                                        + "";
                                    IndexFilterContent = ""
                                        + "\r\n<div class=\"ccContentCon\">"
                                        + "\r\n<div id=\"IndexFilterContentOpened\" class=\"opened\" style=\"display:none;\"><div style=\"text-align:center;\"><img src=\"/ccLib/images/ajax-loader-small.gif\" width=16 height=16></div></div>"
                                        + "\r\n<div id=\"IndexFilterContentClosed\" class=\"closed\">" + FilterClosedLabel + "</div>"
                                        + "\r\n<div id=\"IndexFilterContentMinWidth\" style=\"display:none;\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"200\" height=\"1\" style=\"clear:both\"></div>"
                                        + "\r\n</div>";
                                    AjaxQS = core.doc.refreshQueryString;
                                    AjaxQS = genericController.ModifyQueryString(AjaxQS, RequestNameAjaxFunction, AjaxOpenIndexFilterGetContent);
                                    IndexFilterJS = ""
                                        + "\r\n<script Language=\"JavaScript\" type=\"text/javascript\">"
                                        + "\r\nvar IndexFilterPop=false;"
                                        + "\r\nfunction CloseIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','none');SetDisplay('IndexFilterHeadClosed','block');SetDisplay('IndexFilterContentOpened','none');SetDisplay('IndexFilterContentMinWidth','none');SetDisplay('IndexFilterContentClosed','block');cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxCloseIndexFilter + "','','')}"
                                        + "\r\nfunction OpenIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','block');SetDisplay('IndexFilterHeadClosed','none');SetDisplay('IndexFilterContentOpened','block');SetDisplay('IndexFilterContentMinWidth','block');SetDisplay('IndexFilterContentClosed','none');if(!IndexFilterPop){cj.ajax.qs('" + AjaxQS + "','','IndexFilterContentOpened');IndexFilterPop=true;}else{cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxOpenIndexFilter + "','','');}}"
                                        + "\r\n</script>";
                                }
                            }
                            //
                            // Dual Window Right - Data
                            //
                            FilterDataTable += "<td valign=top class=\"ccPanel\">";
                            //
                            DataTable_HdrRow += "<tr>";
                            //
                            // Row Number Column
                            //
                            DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"ccAdminListCaption\">Row</td>";
                            //
                            // Delete Select Box Columns
                            //
                            if (!AllowDelete) {
                                DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"ccAdminListCaption\"><input TYPE=CheckBox disabled=\"disabled\"></td>";
                            } else {
                                DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"ccAdminListCaption\"><input TYPE=CheckBox OnClick=\"CheckInputs('DelCheck',this.checked);\"></td>";
                            }
                            //
                            // Calculate total width
                            //
                            ColumnWidthTotal = 0;
                            foreach (var kvp in IndexConfig.Columns) {
                                indexConfigColumnClass column = kvp.Value;
                                if (column.Width < 1) {
                                    column.Width = 1;
                                }
                                ColumnWidthTotal = ColumnWidthTotal + column.Width;
                            }
                            //
                            // Edit Column
                            //
                            DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"ccAdminListCaption\">Edit</td>";
                            foreach (var kvp in IndexConfig.Columns) {
                                indexConfigColumnClass column = kvp.Value;
                                //
                                // ----- print column headers - anchored so they sort columns
                                //
                                ColumnWidth = encodeInteger((100 * column.Width) / (double)ColumnWidthTotal);
                                //fieldId = column.FieldId
                                FieldName = column.Name;
                                //
                                //if this is a current sort ,add the reverse flag
                                //
                                ButtonHref = "/" + core.appConfig.adminRoute + "?" + RequestNameAdminForm + "=" + AdminFormIndex + "&SetSortField=" + FieldName + "&RT=0&" + RequestNameTitleExtension + "=" + genericController.EncodeRequestVariable(TitleExtension) + "&cid=" + adminContent.Id + "&ad=" + MenuDepth;
                                foreach (var sortKvp in IndexConfig.Sorts) {
                                    indexConfigSortClass sort = sortKvp.Value;

                                }
                                if (!IndexConfig.Sorts.ContainsKey(FieldName)) {
                                    ButtonHref += "&SetSortDirection=1";
                                } else {
                                    switch (IndexConfig.Sorts[FieldName].direction) {
                                        case 1:
                                            ButtonHref += "&SetSortDirection=2";
                                            break;
                                        case 2:
                                            ButtonHref += "&SetSortDirection=0";
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                //
                                //----- column header includes WherePairCount
                                //
                                if (WherePairCount > 0) {
                                    for (WhereCount = 0; WhereCount < WherePairCount; WhereCount++) {
                                        if (WherePair[0, WhereCount] != "") {
                                            ButtonHref += "&wl" + WhereCount + "=" + genericController.EncodeRequestVariable(WherePair[0, WhereCount]);
                                            ButtonHref += "&wr" + WhereCount + "=" + genericController.EncodeRequestVariable(WherePair[1, WhereCount]);
                                        }
                                    }
                                }
                                ButtonFace = adminContent.fields[FieldName.ToLower()].caption;
                                ButtonFace = genericController.vbReplace(ButtonFace, " ", "&nbsp;");
                                SortTitle = "Sort A-Z";
                                //
                                if (IndexConfig.Sorts.ContainsKey(FieldName)) {
                                    switch (IndexConfig.Sorts[FieldName].direction) {
                                        case 1:
                                            ButtonFace = ButtonFace + "<img src=\"/ccLib/images/arrowdown.gif\" width=8 height=8 border=0>";
                                            SortTitle = "Sort Z-A";
                                            break;
                                        case 2:
                                            ButtonFace = ButtonFace + "<img src=\"/ccLib/images/arrowup.gif\" width=8 height=8 border=0>";
                                            SortTitle = "Remove Sort";
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                ButtonObject = "Button" + ButtonObjectCount;
                                ButtonObjectCount = ButtonObjectCount + 1;
                                DataTable_HdrRow += "<td width=\"" + ColumnWidth + "%\" valign=bottom align=left class=\"ccAdminListCaption\">";
                                DataTable_HdrRow += ("<a title=\"" + SortTitle + "\" href=\"" + genericController.encodeHTML(ButtonHref) + "\" class=\"ccAdminListCaption\">" + ButtonFace + "</A>");
                                DataTable_HdrRow += ("</td>");
                            }
                            DataTable_HdrRow += ("</tr>");
                            //
                            //   select and print Records
                            //
                            //DataSourceName = core.db.getDataSourceNameByID(adminContent.dataSourceId)
                            CS = core.db.csOpenSql(SQL, datasource.Name, IndexConfig.RecordsPerPage, IndexConfig.PageNumber);
                            if (core.db.csOk(CS)) {
                                RowColor = "";
                                RecordPointer = IndexConfig.RecordTop;
                                RecordLast = IndexConfig.RecordTop + IndexConfig.RecordsPerPage;
                                //
                                // --- Print out the records
                                //
                                //IsPageContent = (LCase(adminContent.ContentTableName) = "ccpagecontent")
                                //If IsPageContent Then
                                //    LandingPageID = core.main_GetLandingPageID
                                //End If
                                while ((core.db.csOk(CS)) && (RecordPointer < RecordLast)) {
                                    RecordID = core.db.csGetInteger(CS, "ID");
                                    RecordName = core.db.csGetText(CS, "name");
                                    //IsLandingPage = IsPageContent And (RecordID = LandingPageID)
                                    if (RowColor == "class=\"ccAdminListRowOdd\"") {
                                        RowColor = "class=\"ccAdminListRowEven\"";
                                    } else {
                                        RowColor = "class=\"ccAdminListRowOdd\"";
                                    }
                                    DataTable_DataRows += "\r\n<tr>";
                                    //
                                    // --- Record Number column
                                    //
                                    DataTable_DataRows += "<td align=right " + RowColor + ">" + SpanClassAdminSmall + "[" + (RecordPointer + 1) + "]</span></td>";
                                    //
                                    // --- Delete Checkbox Columns
                                    //
                                    if (AllowDelete) {
                                        //If AllowDelete And Not IsLandingPage Then
                                        //If AdminContent.AllowDelete And Not IsLandingPage Then
                                        DataTable_DataRows += "<td align=center " + RowColor + "><input TYPE=CheckBox NAME=row" + RecordPointer + " VALUE=1 ID=\"DelCheck\"><input type=hidden name=rowid" + RecordPointer + " VALUE=" + RecordID + "></span></td>";
                                    } else {
                                        DataTable_DataRows += "<td align=center " + RowColor + "><input TYPE=CheckBox disabled=\"disabled\" NAME=row" + RecordPointer + " VALUE=1><input type=hidden name=rowid" + RecordPointer + " VALUE=" + RecordID + "></span></td>";
                                    }
                                    //
                                    // --- Edit button column
                                    //
                                    DataTable_DataRows += "<td align=center " + RowColor + ">";
                                    URI = "\\" + core.appConfig.adminRoute + "?" + rnAdminAction + "=" + AdminActionNop + "&cid=" + adminContent.Id + "&id=" + RecordID + "&" + RequestNameTitleExtension + "=" + genericController.EncodeRequestVariable(TitleExtension) + "&ad=" + MenuDepth + "&" + rnAdminSourceForm + "=" + AdminForm + "&" + RequestNameAdminForm + "=" + AdminFormEdit;
                                    if (WherePairCount > 0) {
                                        for (WhereCount = 0; WhereCount < WherePairCount; WhereCount++) {
                                            URI = URI + "&wl" + WhereCount + "=" + genericController.EncodeRequestVariable(WherePair[0, WhereCount]) + "&wr" + WhereCount + "=" + genericController.EncodeRequestVariable(WherePair[1, WhereCount]);
                                        }
                                    }
                                    DataTable_DataRows += ("<a href=\"" + genericController.encodeHTML(URI) + "\"><img src=\"/ccLib/images/IconContentEdit.gif\" border=\"0\"></a>");
                                    DataTable_DataRows += ("</td>");
                                    //
                                    // --- field columns
                                    //
                                    foreach (var columnKvp in IndexConfig.Columns) {
                                        indexConfigColumnClass column = columnKvp.Value;
                                        string columnNameLc = column.Name.ToLower();
                                        if (FieldUsedInColumns.ContainsKey(columnNameLc)) {
                                            if (FieldUsedInColumns[columnNameLc]) {
                                                DataTable_DataRows += ("\r\n<td valign=\"middle\" " + RowColor + " align=\"left\">" + SpanClassAdminNormal);
                                                DataTable_DataRows += GetForm_Index_GetCell(adminContent, editRecord, column.Name, CS, IsLookupFieldValid[columnNameLc], genericController.vbLCase(adminContent.ContentTableName) == "ccemail");
                                                DataTable_DataRows += ("&nbsp;</span></td>");
                                            }
                                        }
                                    }
                                    DataTable_DataRows += ("\n    </tr>");
                                    core.db.csGoNext(CS);
                                    RecordPointer = RecordPointer + 1;
                                }
                                DataTable_DataRows += "<input type=hidden name=rowcnt value=" + RecordPointer + ">";
                                //
                                // --- print out the stuff at the bottom
                                //
                                RecordTop_NextPage = IndexConfig.RecordTop;
                                if (core.db.csOk(CS)) {
                                    RecordTop_NextPage = RecordPointer;
                                }
                                RecordTop_PreviousPage = IndexConfig.RecordTop - IndexConfig.RecordsPerPage;
                                if (RecordTop_PreviousPage < 0) {
                                    RecordTop_PreviousPage = 0;
                                }
                            }
                            core.db.csClose(ref CS);
                            //
                            // Header at bottom
                            //
                            if (RowColor == "class=\"ccAdminListRowOdd\"") {
                                RowColor = "class=\"ccAdminListRowEven\"";
                            } else {
                                RowColor = "class=\"ccAdminListRowOdd\"";
                            }
                            if (RecordPointer == 0) {
                                //
                                // No records found
                                //
                                DataTable_DataRows += ("<tr><td " + RowColor + " align=center>-</td><td " + RowColor + " align=center>-</td><td " + RowColor + " align=center>-</td><td colspan=" + IndexConfig.Columns.Count + " " + RowColor + " style=\"text-align:left ! important;\">no records were found</td></tr>");
                            } else {
                                if (RecordPointer < RecordLast) {
                                    //
                                    // End of list
                                    //
                                    DataTable_DataRows += ("<tr><td " + RowColor + " align=center>-</td><td " + RowColor + " align=center>-</td><td " + RowColor + " align=center>-</td><td colspan=" + IndexConfig.Columns.Count + " " + RowColor + " style=\"text-align:left ! important;\">----- end of list</td></tr>");
                                }
                                //
                                // Add another header to the data rows
                                //
                                DataTable_DataRows += DataTable_HdrRow;
                            }
                            //
                            // ----- DataTable_FindRow
                            //
                            //ReDim Findstring(IndexConfig.Columns.Count)
                            //For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
                            //    FieldName = IndexConfig.Columns(ColumnPointer).Name
                            //    If genericController.vbLCase(FieldName) = FindWordName Then
                            //        Findstring(ColumnPointer) = FindWordValue
                            //    End If
                            //Next
                            //        ReDim Findstring(CustomAdminColumnCount)
                            //        For ColumnPointer = 0 To CustomAdminColumnCount - 1
                            //            FieldPtr = CustomAdminColumn(ColumnPointer).FieldPointer
                            //            With AdminContent.fields(FieldPtr)
                            //                If genericController.vbLCase(.Name) = FindWordName Then
                            //                    Findstring(ColumnPointer) = FindWordValue
                            //                End If
                            //            End With
                            //        Next
                            //
                            DataTable_FindRow = DataTable_FindRow + "<tr><td colspan=" + (3 + IndexConfig.Columns.Count) + " style=\"background-color:black;height:1;\"></td></tr>";
                            //DataTable_FindRow = DataTable_FindRow & "<tr><td colspan=" & (3 + CustomAdminColumnCount) & " style=""background-color:black;height:1;""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td></tr>"
                            DataTable_FindRow = DataTable_FindRow + "<tr>";
                            DataTable_FindRow = DataTable_FindRow + "<td colspan=3 width=\"60\" class=\"ccPanel\" align=center style=\"text-align:center ! important;\">";
                            DataTable_FindRow = DataTable_FindRow + "\r\n<script language=\"javascript\" type=\"text/javascript\">"
                                + "\r\nfunction KeyCheck(e){"
                                + "\r\n  var code = e.keyCode;"
                                + "\r\n  if(code==13){"
                                + "\r\n    document.getElementById('FindButton').focus();"
                                + "\r\n    document.getElementById('FindButton').click();"
                                + "\r\n    return false;"
                                + "\r\n  }"
                                + "\r\n} "
                                + "\r\n</script>";
                            DataTable_FindRow = DataTable_FindRow + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"60\" height=\"1\" ><br>" + core.html.button(ButtonFind, "", "FindButton") + "</td>";
                            ColumnPointer = 0;
                            foreach (var kvp in IndexConfig.Columns) {
                                indexConfigColumnClass column = kvp.Value;
                                //For ColumnPointer = 0 To CustomAdminColumnCount - 1
                                ColumnWidth = column.Width;
                                //fieldId = .FieldId
                                FieldName = genericController.vbLCase(column.Name);
                                FindWordValue = "";
                                if (IndexConfig.FindWords.ContainsKey(FieldName)) {
                                    var tempVar = IndexConfig.FindWords[FieldName];
                                    if ((tempVar.MatchOption == FindWordMatchEnum.matchincludes) || (tempVar.MatchOption == FindWordMatchEnum.MatchEquals)) {
                                        FindWordValue = tempVar.Value;
                                    }
                                }
                                DataTable_FindRow = DataTable_FindRow + "\r\n<td valign=\"top\" align=\"center\" class=\"ccPanel3DReverse\" style=\"padding-top:2px;padding-bottom:2px;\">"
                                    + "<input type=hidden name=\"FindName" + ColumnPointer + "\" value=\"" + FieldName + "\">"
                                    + "<input onkeypress=\"KeyCheck(event);\"  type=text id=\"F" + ColumnPointer + "\" name=\"FindValue" + ColumnPointer + "\" value=\"" + FindWordValue + "\" style=\"width:98%\">"
                                    + "</td>";
                                ColumnPointer += 1;
                            }
                            DataTable_FindRow = DataTable_FindRow + "</tr>";
                            //
                            // Assemble DataTable
                            //
                            DataTable = ""
                                + "<table ID=\"DataTable\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"Background-Color:white;\">"
                                + DataTable_HdrRow + DataTable_DataRows + DataTable_FindRow + "</table>";
                            //DataTable = GetForm_Index_AdvancedSearch()
                            //
                            // Assemble DataFilterTable
                            //
                            if (!string.IsNullOrEmpty(IndexFilterContent)) {
                                FilterColumn = "<td valign=top style=\"border-right:1px solid black;\" class=\"ccToolsCon\">" + IndexFilterJS + IndexFilterHead + IndexFilterContent + "</td>";
                                //FilterColumn = "<td valign=top class=""ccPanel3DReverse ccAdminEditBody"" style=""border-right:1px solid black;"">" & IndexFilterJS & IndexFilterHead & IndexFilterContent & "</td>"
                            }
                            DataColumn = "<td width=\"99%\" valign=top>" + DataTable + "</td>";
                            FilterDataTable = ""
                                + "<table ID=\"DataFilterTable\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"Background-Color:white;\">"
                                + "<tr>"
                                + FilterColumn + DataColumn + "</tr>"
                                + "</table>";
                            //
                            // Assemble LiveWindowTable
                            //
                            // Stream.Add( OpenLiveWindowTable)
                            Stream.Add(core.html.formStart("", "adminForm"));
                            Stream.Add(ButtonBar);
                            Stream.Add(Adminui.GetTitleBar(TitleBar, HeaderDescription));
                            Stream.Add(FilterDataTable);
                            Stream.Add(ButtonBar);
                            Stream.Add(core.html.getPanel("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"1\", height=\"10\" >"));
                            Stream.Add(core.html.inputHidden(rnAdminSourceForm, AdminFormIndex));
                            Stream.Add(core.html.inputHidden("cid", adminContent.Id));
                            Stream.Add(core.html.inputHidden("indexGoToPage", ""));
                            Stream.Add(core.html.inputHidden("Columncnt", IndexConfig.Columns.Count));
                            Stream.Add("</form>");
                            core.html.addTitle(adminContent.Name);
                        }
                    }
                    //End If
                    //
                }
                returnForm = Stream.Text;
                //
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnForm;
        }
        //
        //========================================================================
        // ----- Get an XML nodes attribute based on its name
        //========================================================================
        //
        private string GetXMLAttribute(bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
            string tempGetXMLAttribute = null;
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
                    UcaseName = genericController.vbUCase(Name);
                    foreach (XmlAttribute NodeAttribute in Node.Attributes) {
                        if (genericController.vbUCase(NodeAttribute.Name) == UcaseName) {
                            tempGetXMLAttribute = NodeAttribute.Value;
                            Found = true;
                            break;
                        }
                    }
                } else {
                    tempGetXMLAttribute = ResultNode.Value;
                    Found = true;
                }
                if (!Found) {
                    tempGetXMLAttribute = DefaultIfNotFound;
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetXMLAttribute;
        }
        //
        // REFACTOR -- THIS SHOULD BE A REMOTE METHOD AND NOT CALLED FROM core.
        //==========================================================================================================================================
        /// <summary>
        /// Get index view filter content - remote method
        /// </summary>
        /// <param name="adminContent"></param>
        /// <returns></returns>
        public string GetForm_IndexFilterContent(cdefModel adminContent) {
            string returnContent = "";
            try {
                int RecordID = 0;
                string Name = null;
                string TableName = null;
                string FieldCaption = null;
                string ContentName = null;
                int CS = 0;
                string SQL = null;
                string Caption = null;
                string Link = null;
                string RQS = null;
                string QS = null;
                int Ptr = 0;
                string SubFilterList = null;
                indexConfigClass IndexConfig = null;
                string list = null;
                string[] ListSplit = null;
                int Cnt = 0;
                int Pos = 0;
                int subContentID = 0;
                //
                IndexConfig = LoadIndexConfig(adminContent);
                //
                ContentName = cdefModel.getContentNameByID(core, adminContent.Id);
                RQS = "cid=" + adminContent.Id + "&af=1";
                //
                //-------------------------------------------------------------------------------------
                // Remove filters
                //-------------------------------------------------------------------------------------
                //
                if ((IndexConfig.SubCDefID > 0) || (IndexConfig.GroupListCnt != 0) | (IndexConfig.FindWords.Count != 0) | IndexConfig.ActiveOnly | IndexConfig.LastEditedByMe | IndexConfig.LastEditedToday | IndexConfig.LastEditedPast7Days | IndexConfig.LastEditedPast30Days) {
                    //
                    // Remove Filters
                    //
                    returnContent += "<div class=\"ccFilterHead\">Remove&nbsp;Filters</div>";
                    QS = RQS;
                    QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveAll", "1");
                    Link = "/" + core.appConfig.adminRoute + "?" + QS;
                    returnContent += "<div class=\"ccFilterSubHead\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;Remove All</a></div>";
                    //
                    // Last Edited Edited by me
                    //
                    SubFilterList = "";
                    if (IndexConfig.LastEditedByMe) {
                        QS = RQS;
                        QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedByMe", 0.ToString(), true);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList = SubFilterList + "<div class=\"ccFilterIndent ccFilterList\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">By&nbsp;Me</a></div>";
                    }
                    if (IndexConfig.LastEditedToday) {
                        QS = RQS;
                        QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedToday", 0.ToString(), true);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList = SubFilterList + "<div class=\"ccFilterIndent ccFilterList\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">Today</a></div>";
                    }
                    if (IndexConfig.LastEditedPast7Days) {
                        QS = RQS;
                        QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast7Days", 0.ToString(), true);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList = SubFilterList + "<div class=\"ccFilterIndent ccFilterList\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">Past Week</a></div>";
                    }
                    if (IndexConfig.LastEditedPast30Days) {
                        QS = RQS;
                        QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast30Days", 0.ToString(), true);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList = SubFilterList + "<div class=\"ccFilterIndent ccFilterList\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">Past 30 Days</a></div>";
                    }
                    if (!string.IsNullOrEmpty(SubFilterList)) {
                        returnContent += "<div class=\"ccFilterSubHead\">Last&nbsp;Edited</div>" + SubFilterList;
                    }
                    //
                    // Sub Content definitions
                    //
                    string SubContentName = null;
                    SubFilterList = "";
                    if (IndexConfig.SubCDefID > 0) {
                        SubContentName = cdefModel.getContentNameByID(core, IndexConfig.SubCDefID);
                        QS = RQS;
                        QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveCDef", encodeText(IndexConfig.SubCDefID));
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">" + SubContentName + "</a></div>";
                    }
                    if (!string.IsNullOrEmpty(SubFilterList)) {
                        returnContent += "<div class=\"ccFilterSubHead\">In Sub-content</div>" + SubFilterList;
                    }
                    //
                    // Group Filter List
                    //
                    string GroupName = null;
                    SubFilterList = "";
                    if (IndexConfig.GroupListCnt > 0) {
                        for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++) {
                            GroupName = IndexConfig.GroupList[Ptr];
                            if (IndexConfig.GroupList[Ptr] != "") {
                                if (GroupName.Length > 30) {
                                    GroupName = GroupName.Left(15) + "..." + GroupName.Substring(GroupName.Length - 15);
                                }
                                QS = RQS;
                                QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveGroup", IndexConfig.GroupList[Ptr]);
                                Link = "/" + core.appConfig.adminRoute + "?" + QS;
                                SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">" + GroupName + "</a></div>";
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(SubFilterList)) {
                        returnContent += "<div class=\"ccFilterSubHead\">In Group(s)</div>" + SubFilterList;
                    }
                    //
                    // Other Filter List
                    //
                    SubFilterList = "";
                    if (IndexConfig.ActiveOnly) {
                        QS = RQS;
                        QS = genericController.ModifyQueryString(QS, "IndexFilterActiveOnly", 0.ToString(), true);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList = SubFilterList + "<div class=\"ccFilterIndent ccFilterList\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">Active&nbsp;Only</a></div>";
                    }
                    if (!string.IsNullOrEmpty(SubFilterList)) {
                        returnContent += "<div class=\"ccFilterSubHead\">Other</div>" + SubFilterList;
                    }
                    //
                    // FindWords
                    //
                    foreach (var findWordKvp in IndexConfig.FindWords) {
                        indexConfigFindWordClass findWord = findWordKvp.Value;
                        FieldCaption = genericController.encodeText(cdefModel.GetContentFieldProperty(core, ContentName, findWord.Name, "caption"));
                        QS = RQS;
                        QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveFind", findWord.Name);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        switch (findWord.MatchOption) {
                            case FindWordMatchEnum.matchincludes:
                                returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;includes&nbsp;'" + findWord.Value + "'</a></div>";
                                break;
                            case FindWordMatchEnum.MatchEmpty:
                                returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;is&nbsp;empty</a></div>";
                                break;
                            case FindWordMatchEnum.MatchEquals:
                                returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;=&nbsp;'" + findWord.Value + "'</a></div>";
                                break;
                            case FindWordMatchEnum.MatchFalse:
                                returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;is&nbsp;false</a></div>";
                                break;
                            case FindWordMatchEnum.MatchGreaterThan:
                                returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;&gt;&nbsp;'" + findWord.Value + "'</a></div>";
                                break;
                            case FindWordMatchEnum.MatchLessThan:
                                returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;&lt;&nbsp;'" + findWord.Value + "'</a></div>";
                                break;
                            case FindWordMatchEnum.MatchNotEmpty:
                                returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;is&nbsp;not&nbsp;empty</a></div>";
                                break;
                            case FindWordMatchEnum.MatchTrue:
                                returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;is&nbsp;true</a></div>";
                                break;
                        }
                    }
                    //
                    returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
                }
                //
                //-------------------------------------------------------------------------------------
                // Add filters
                //-------------------------------------------------------------------------------------
                //
                returnContent += "<div class=\"ccFilterHead\">Add&nbsp;Filters</div>";
                //
                // Last Edited
                //
                SubFilterList = "";
                if (!IndexConfig.LastEditedByMe) {
                    QS = RQS;
                    QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedByMe", "1", true);
                    Link = "/" + core.appConfig.adminRoute + "?" + QS;
                    SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">By&nbsp;Me</a></div>";
                }
                if (!IndexConfig.LastEditedToday) {
                    QS = RQS;
                    QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedToday", "1", true);
                    Link = "/" + core.appConfig.adminRoute + "?" + QS;
                    SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Today</a></div>";
                }
                if (!IndexConfig.LastEditedPast7Days) {
                    QS = RQS;
                    QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast7Days", "1", true);
                    Link = "/" + core.appConfig.adminRoute + "?" + QS;
                    SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Past Week</a></div>";
                }
                if (!IndexConfig.LastEditedPast30Days) {
                    QS = RQS;
                    QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast30Days", "1", true);
                    Link = "/" + core.appConfig.adminRoute + "?" + QS;
                    SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Past 30 Days</a></div>";
                }
                if (!string.IsNullOrEmpty(SubFilterList)) {
                    returnContent += "<div class=\"ccFilterSubHead\">Last&nbsp;Edited</div>" + SubFilterList;
                }
                //
                // Sub Content Definitions
                //
                SubFilterList = "";
                list = cdefModel.getContentControlCriteria(core, ContentName);
                if (!string.IsNullOrEmpty(list)) {
                    ListSplit = list.Split('=');
                    Cnt = ListSplit.GetUpperBound(0) + 1;
                    if (Cnt > 0) {
                        for (Ptr = 0; Ptr < Cnt; Ptr++) {
                            Pos = genericController.vbInstr(1, ListSplit[Ptr], ")");
                            if (Pos > 0) {
                                subContentID = genericController.encodeInteger(ListSplit[Ptr].Left(Pos - 1));
                                if (subContentID > 0 && (subContentID != adminContent.Id) & (subContentID != IndexConfig.SubCDefID)) {
                                    Caption = "<span style=\"white-space:nowrap;\">" + cdefModel.getContentNameByID(core, subContentID) + "</span>";
                                    QS = RQS;
                                    QS = genericController.ModifyQueryString(QS, "IndexFilterAddCDef", subContentID.ToString(), true);
                                    Link = "/" + core.appConfig.adminRoute + "?" + QS;
                                    SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">" + Caption + "</a></div>";
                                }
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(SubFilterList)) {
                    returnContent += "<div class=\"ccFilterSubHead\">In Sub-content</div>" + SubFilterList;
                }
                //
                // people filters
                //
                TableName = cdefModel.getContentTablename(core, ContentName);
                SubFilterList = "";
                if (genericController.vbLCase(TableName) == genericController.vbLCase("ccMembers")) {
                    SQL = core.db.GetSQLSelect("default", "ccGroups", "ID,Caption,Name", "(active<>0)", "Caption,Name");
                    CS = core.db.csOpenSql_rev("default", SQL);
                    while (core.db.csOk(CS)) {
                        Name = core.db.csGetText(CS, "Name");
                        Ptr = 0;
                        if (IndexConfig.GroupListCnt > 0) {
                            for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++) {
                                if (Name == IndexConfig.GroupList[Ptr]) {
                                    break;
                                }
                            }
                        }
                        if (Ptr == IndexConfig.GroupListCnt) {
                            RecordID = core.db.csGetInteger(CS, "ID");
                            Caption = core.db.csGetText(CS, "Caption");
                            if (string.IsNullOrEmpty(Caption)) {
                                Caption = Name;
                                if (string.IsNullOrEmpty(Caption)) {
                                    Caption = "Group " + RecordID;
                                }
                            }
                            if (Caption.Length > 30) {
                                Caption = Caption.Left(15) + "..." + Caption.Substring(Caption.Length - 15);
                            }
                            Caption = "<span style=\"white-space:nowrap;\">" + Caption + "</span>";
                            QS = RQS;
                            if (!string.IsNullOrEmpty(Name.Trim(' '))) {
                                QS = genericController.ModifyQueryString(QS, "IndexFilterAddGroup", Name, true);
                            } else {
                                QS = genericController.ModifyQueryString(QS, "IndexFilterAddGroup", RecordID.ToString(), true);
                            }
                            Link = "/" + core.appConfig.adminRoute + "?" + QS;
                            SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">" + Caption + "</a></div>";
                        }
                        core.db.csGoNext(CS);
                    }
                }
                if (!string.IsNullOrEmpty(SubFilterList)) {
                    returnContent += "<div class=\"ccFilterSubHead\">In Group(s)</div>" + SubFilterList;
                }
                //
                // Active Only
                //
                SubFilterList = "";
                if (!IndexConfig.ActiveOnly) {
                    QS = RQS;
                    QS = genericController.ModifyQueryString(QS, "IndexFilterActiveOnly", "1", true);
                    Link = "/" + core.appConfig.adminRoute + "?" + QS;
                    SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Active&nbsp;Only</a></div>";
                }
                if (!string.IsNullOrEmpty(SubFilterList)) {
                    returnContent += "<div class=\"ccFilterSubHead\">Other</div>" + SubFilterList;
                }
                //
                returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
                //
                // Advanced Search Link
                //
                QS = RQS;
                QS = genericController.ModifyQueryString(QS, RequestNameAdminSubForm, AdminFormIndex_SubFormAdvancedSearch, true);
                Link = "/" + core.appConfig.adminRoute + "?" + QS;
                returnContent += "<div class=\"ccFilterHead\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Advanced&nbsp;Search</a></div>";
                //
                returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
                //
                // Set Column Link
                //
                QS = RQS;
                QS = genericController.ModifyQueryString(QS, RequestNameAdminSubForm, AdminFormIndex_SubFormSetColumns, true);
                Link = "/" + core.appConfig.adminRoute + "?" + QS;
                returnContent += "<div class=\"ccFilterHead\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Set&nbsp;Columns</a></div>";
                //
                returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
                //
                // Import Link
                //
                QS = RQS;
                QS = genericController.ModifyQueryString(QS, RequestNameAdminForm, AdminFormImportWizard, true);
                Link = "/" + core.appConfig.adminRoute + "?" + QS;
                returnContent += "<div class=\"ccFilterHead\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Import</a></div>";
                //
                returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
                //
                // Export Link
                //
                QS = RQS;
                QS = genericController.ModifyQueryString(QS, RequestNameAdminSubForm, AdminFormIndex_SubFormExport, true);
                Link = "/" + core.appConfig.adminRoute + "?" + QS;
                returnContent += "<div class=\"ccFilterHead\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Export</a></div>";
                //
                returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
                //
                returnContent = "<div style=\"padding-left:10px;padding-right:10px;\">" + returnContent + "</div>";
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnContent;
        }
        //
        //
        //
        //========================================================================
        //   read the input request
        //       If RequestBlocked get adminContent.id, AdminAction, FormID
        //       and AdminForm are the only variables accessible before reading
        //       the upl collection
        //========================================================================
        //
        private void GetForm_LoadControl(ref cdefModel adminContent, editRecordClass editRecord) {
            try {
                //
                string editorpreferences = null;
                int Pos = 0;
                string SQL = null;
                string Key = null;
                string[] Parts = null;
                int Ptr = 0;
                int Cnt = 0;
                int fieldEditorFieldId = 0;
                int fieldEditorAddonId = 0;
                bool editorOk = false;
                int CS = 0;
                string[] QSSplit = null;
                int QSPointer = 0;
                string[] NVSplit = null;
                string NameValue = null;
                int WCount = 0;
                string WhereClauseContent = null;
                DataTable dtTest = null;
                //
                // Tab Control
                //
                allowAdminTabs = genericController.encodeBoolean(core.userProperty.getText("AllowAdminTabs", "1"));
                if (core.docProperties.getText("tabs") != "") {
                    if (core.docProperties.getBoolean("tabs") != allowAdminTabs) {
                        allowAdminTabs = !allowAdminTabs;
                        core.userProperty.setProperty("AllowAdminTabs", allowAdminTabs.ToString());
                    }
                }
                //
                // AdminContent init
                //
                requestedContentId = core.docProperties.getInteger("cid");
                if (requestedContentId != 0) {
                    adminContent = cdefModel.getCdef(core, requestedContentId);
                    if (adminContent == null) {
                        adminContent = new cdefModel();
                        adminContent.Id = 0;
                        errorController.addUserError(core, "There is no content with the requested id [" + requestedContentId + "]");
                        requestedContentId = 0;
                    }
                }
                if (adminContent == null) {
                    adminContent = new cdefModel();
                }
                //
                // determine user rights to this content
                //
                UserAllowContentEdit = true;
                if (!core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                    if (adminContent.Id > 0) {
                        UserAllowContentEdit = userHasContentAccess(adminContent.Id);
                    }
                }
                //
                // editRecord init
                //
                requestedRecordId = core.docProperties.getInteger("id");
                if ((UserAllowContentEdit) & (requestedRecordId != 0) && (adminContent.Id > 0)) {
                    //
                    // set AdminContent to the content definition of the requested record
                    //
                    CS = core.db.csOpenRecord(adminContent.Name, requestedRecordId, false, false, "ContentControlID");
                    if (core.db.csOk(CS)) {
                        editRecord.id = requestedRecordId;
                        adminContent.Id = core.db.csGetInteger(CS, "ContentControlID");
                        if (adminContent.Id <= 0) {
                            adminContent.Id = requestedContentId;
                        } else if (adminContent.Id != requestedContentId) {
                            adminContent = cdefModel.getCdef(core, adminContent.Id);
                        }
                    }
                    core.db.csClose(ref CS);
                }
                //
                // Other page control fields
                //
                TitleExtension = core.docProperties.getText(RequestNameTitleExtension);
                RecordTop = core.docProperties.getInteger("RT");
                RecordsPerPage = core.docProperties.getInteger("RS");
                if (RecordsPerPage == 0) {
                    RecordsPerPage = RecordsPerPageDefault;
                }
                //
                // Read WherePairCount
                //
                WherePairCount = 99;
                for (WCount = 0; WCount <= 99; WCount++) {
                    WherePair[0, WCount] = genericController.encodeText(core.docProperties.getText("WL" + WCount));
                    if (WherePair[0, WCount] == "") {
                        WherePairCount = WCount;
                        break;
                    } else {
                        WherePair[1, WCount] = genericController.encodeText(core.docProperties.getText("WR" + WCount));
                        core.doc.addRefreshQueryString("wl" + WCount, genericController.EncodeRequestVariable(WherePair[0, WCount]));
                        core.doc.addRefreshQueryString("wr" + WCount, genericController.EncodeRequestVariable(WherePair[1, WCount]));
                    }
                }
                //
                // Read WhereClauseContent to WherePairCount
                //
                WhereClauseContent = genericController.encodeText(core.docProperties.getText("wc"));
                if (!string.IsNullOrEmpty(WhereClauseContent)) {
                    //
                    // ***** really needs a server.URLDecode() function
                    //
                    core.doc.addRefreshQueryString("wc", WhereClauseContent);
                    //WhereClauseContent = genericController.vbReplace(WhereClauseContent, "%3D", "=")
                    //WhereClauseContent = genericController.vbReplace(WhereClauseContent, "%26", "&")
                    if (!string.IsNullOrEmpty(WhereClauseContent)) {
                        QSSplit = WhereClauseContent.Split(',');
                        for (QSPointer = 0; QSPointer <= QSSplit.GetUpperBound(0); QSPointer++) {
                            NameValue = QSSplit[QSPointer];
                            if (!string.IsNullOrEmpty(NameValue)) {
                                if ((NameValue.Left(1) == "(") && (NameValue.Substring(NameValue.Length - 1) == ")") && (NameValue.Length > 2)) {
                                    NameValue = NameValue.Substring(1, NameValue.Length - 2);
                                }
                                NVSplit = NameValue.Split('=');
                                WherePair[0, WherePairCount] = NVSplit[0];
                                if (NVSplit.GetUpperBound(0) > 0) {
                                    WherePair[1, WherePairCount] = NVSplit[1];
                                }
                                WherePairCount = WherePairCount + 1;
                            }
                        }
                    }
                }
                //
                // --- If AdminMenuMode is not given locally, use the Members Preferences
                //
                AdminMenuModeID = core.docProperties.getInteger("mm");
                if (AdminMenuModeID == 0) {
                    AdminMenuModeID = core.doc.sessionContext.user.AdminMenuModeID;
                }
                if (AdminMenuModeID == 0) {
                    AdminMenuModeID = AdminMenuModeLeft;
                }
                if (core.doc.sessionContext.user.AdminMenuModeID != AdminMenuModeID) {
                    core.doc.sessionContext.user.AdminMenuModeID = AdminMenuModeID;
                    core.doc.sessionContext.user.save(core);
                }
                //    '
                //    ' ----- FieldName
                //    '
                //    InputFieldName = core.main_GetStreamText2(RequestNameFieldName)
                //
                // ----- Other
                //
                AdminAction = core.docProperties.getInteger(rnAdminAction);
                AdminSourceForm = core.docProperties.getInteger(rnAdminSourceForm);
                AdminForm = core.docProperties.getInteger(RequestNameAdminForm);
                AdminButton = core.docProperties.getText(RequestNameButton);
                //
                // ----- Convert misc Deletes to just delete for later processing
                //
                if ((AdminButton == ButtonDeleteEmail) || (AdminButton == ButtonDeletePage) || (AdminButton == ButtonDeletePerson) || (AdminButton == ButtonDeleteRecord)) {
                    AdminButton = ButtonDelete;
                }
                if (AdminForm == AdminFormEdit) {
                    MenuDepth = 0;
                } else {
                    MenuDepth = core.docProperties.getInteger(RequestNameAdminDepth);
                }
                //
                // ----- convert fieldEditorPreference change to a refresh action
                //
                if (adminContent.Id != 0) {
                    fieldEditorPreference = core.docProperties.getText("fieldEditorPreference");
                    if (fieldEditorPreference != "") {
                        //
                        // Editor Preference change attempt. Set new preference and set this as a refresh
                        //
                        AdminButton = "";
                        AdminAction = AdminActionEditRefresh;
                        AdminForm = AdminFormEdit;
                        Pos = genericController.vbInstr(1, fieldEditorPreference, ":");
                        if (Pos > 0) {
                            fieldEditorFieldId = genericController.encodeInteger(fieldEditorPreference.Left(Pos - 1));
                            fieldEditorAddonId = genericController.encodeInteger(fieldEditorPreference.Substring(Pos));
                            if (fieldEditorFieldId != 0) {
                                editorOk = true;
                                SQL = "select id from ccfields where (active<>0) and id=" + fieldEditorFieldId;
                                dtTest = core.db.executeQuery(SQL);
                                if (dtTest.Rows.Count == 0) {
                                    editorOk = false;
                                }
                                //RS = core.app.executeSql(SQL)
                                //If (not isdatatableok(rs)) Then
                                //    editorOk = False
                                //ElseIf rs.rows.count=0 Then
                                //    editorOk = False
                                //End If
                                //If (isDataTableOk(rs)) Then
                                //    If false Then
                                //        'RS.Close()
                                //    End If
                                //    'RS = Nothing
                                //End If
                                if (editorOk && (fieldEditorAddonId != 0)) {
                                    SQL = "select id from ccaggregatefunctions where (active<>0) and id=" + fieldEditorAddonId;
                                    dtTest = core.db.executeQuery(SQL);
                                    if (dtTest.Rows.Count == 0) {
                                        editorOk = false;
                                    }
                                    //RS = core.app.executeSql(SQL)
                                    //If (not isdatatableok(rs)) Then
                                    //    editorOk = False
                                    //ElseIf rs.rows.count=0 Then
                                    //    editorOk = False
                                    //End If
                                    //If (isDataTableOk(rs)) Then
                                    //    If false Then
                                    //        'RS.Close()
                                    //    End If
                                    //    'RS = Nothing
                                    //End If
                                }
                                if (editorOk) {
                                    Key = "editorPreferencesForContent:" + adminContent.Id;
                                    editorpreferences = core.userProperty.getText(Key, "");
                                    if (!string.IsNullOrEmpty(editorpreferences)) {
                                        //
                                        // remove current preferences for this field
                                        //
                                        Parts = ("," + editorpreferences).Split(new[] { "," + fieldEditorFieldId.ToString() + ":" }, StringSplitOptions.None);
                                        Cnt = Parts.GetUpperBound(0) + 1;
                                        if (Cnt > 0) {
                                            for (Ptr = 1; Ptr < Cnt; Ptr++) {
                                                Pos = genericController.vbInstr(1, Parts[Ptr], ",");
                                                if (Pos == 0) {
                                                    Parts[Ptr] = "";
                                                } else if (Pos > 0) {
                                                    Parts[Ptr] = Parts[Ptr].Substring(Pos);
                                                }
                                            }
                                        }
                                        editorpreferences = string.Join("", Parts);
                                    }
                                    editorpreferences = editorpreferences + "," + fieldEditorFieldId + ":" + fieldEditorAddonId;
                                    core.userProperty.setProperty(Key, editorpreferences);
                                }
                            }
                        }
                    }
                }
                //
                // --- Spell Check
                SpellCheckSupported = false;
                SpellCheckRequest = false;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return;
        }
        //
        //
        //
        //========================================================================
        // ProcessActions
        //   perform the action called from the previous form
        //   when action is complete, replace the action code with one that will refresh
        //
        //   Request Variables
        //       ID = ID of record to edit
        //       AdminAction = action to be performed, defined below, required except for very first call to edit
        //   AdminAction Definitions
        //       edit - edit the record defined by ID, If ID="", edit a new record
        //       Save - saves an edit record and returns to the index
        //       Delete - hmmm.
        //       Cancel - returns to index
        //       Change Filex - uploads a file to a FieldTypeFile, x is a number 0...AdminContent.FieldMax
        //       Delete Filex - clears a file name for a FieldTypeFile, x is a number 0...AdminContent.FieldMax
        //       Upload - The action that actually uploads the file
        //       Email - (not done) Sends "body" field to "email" field in adminContent.id
        //========================================================================
        //
        private void ProcessActions(cdefModel adminContent, editRecordClass editRecord, bool UseContentWatchLink) {
            try {
                int CS = 0;
                int RecordID = 0;
                string ContentName = null;
                int CSEditRecord = 0;
                int EmailToConfirmationMemberID = 0;
                int RowCnt = 0;
                int RowPtr = 0;
                //
                if (AdminAction != AdminActionNop) {
                    if (!UserAllowContentEdit) {
                        //
                        // Action blocked by BlockCurrentRecord
                        //
                    } else {
                        //
                        // Process actions
                        //
                        switch (AdminAction) {
                            case AdminActionEditRefresh:
                                //
                                // Load the record as if it will be saved, but skip the save
                                //
                                LoadEditRecord(adminContent, editRecord);
                                LoadEditRecord_Request(adminContent, editRecord);
                                break;
                            case AdminActionMarkReviewed:
                                //
                                // Mark the record reviewed without making any changes
                                //
                                core.doc.markRecordReviewed(adminContent.Name, editRecord.id);
                                break;
                            case AdminActionDelete:
                                if (editRecord.Read_Only) {
                                    errorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    LoadEditRecord(adminContent, editRecord);
                                    core.db.deleteTableRecord(adminContent.ContentTableName, editRecord.id, adminContent.ContentDataSourceName);
                                    core.doc.processAfterSave(true, editRecord.contentControlId_Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink);
                                }
                                AdminAction = AdminActionNop; 
                                break;
                            case AdminActionSave:
                                //
                                // ----- Save Record
                                //
                                if (editRecord.Read_Only) {
                                    errorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    LoadEditRecord(adminContent, editRecord);
                                    LoadEditRecord_Request(adminContent, editRecord);
                                    ProcessActionSave(adminContent, editRecord, UseContentWatchLink);
                                    core.doc.processAfterSave(false, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink);
                                }
                                AdminAction = AdminActionNop; // convert so action can be used in as a refresh
                                                              //
                                break;
                            case AdminActionSaveAddNew:
                                //
                                // ----- Save and add a new record
                                //
                                if (editRecord.Read_Only) {
                                    errorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    LoadEditRecord(adminContent, editRecord);
                                    LoadEditRecord_Request(adminContent, editRecord);
                                    ProcessActionSave(adminContent, editRecord, UseContentWatchLink);
                                    core.doc.processAfterSave(false, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink);
                                    editRecord.id = 0;
                                    editRecord.Loaded = false;
                                    //If AdminContent.fields.Count > 0 Then
                                    //    ReDim EditRecordValuesObject(AdminContent.fields.Count)
                                    //    ReDim EditRecordDbValues(AdminContent.fields.Count)
                                    //End If
                                }
                                AdminAction = AdminActionNop; // convert so action can be used in as a refresh
                                                              //
                                break;
                            case AdminActionDuplicate:
                                //
                                // ----- Save Record
                                //
                                ProcessActionDuplicate(adminContent, editRecord);
                                AdminAction = AdminActionNop;
                                break;
                            case AdminActionSendEmail:
                                //
                                // ----- Send (Group Email Only)
                                //
                                if (editRecord.Read_Only) {
                                    errorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    LoadEditRecord(adminContent, editRecord);
                                    LoadEditRecord_Request(adminContent, editRecord);
                                    ProcessActionSave(adminContent, editRecord, UseContentWatchLink);
                                    core.doc.processAfterSave(false, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink);
                                    if (!(core.doc.debug_iUserError != "")) {
                                        if (!cdefModel.isWithinContent(core, editRecord.contentControlId, cdefModel.getContentId(core, "Group Email"))) {
                                            errorController.addUserError(core, "The send action only supports Group Email.");
                                        } else {
                                            CS = core.db.csOpenRecord("Group Email", editRecord.id);
                                            if (!core.db.csOk(CS)) {
                                                //throw new ApplicationException("Unexpected exception"); // //throw new ApplicationException("Unexpected exception")' core.handleLegacyError23("Email ID [" & editRecord.id & "] could not be found in Group Email.")
                                            } else if (core.db.csGet(CS, "FromAddress") == "") {
                                                errorController.addUserError(core, "A 'From Address' is required before sending an email.");
                                            } else if (core.db.csGet(CS, "Subject") == "") {
                                                errorController.addUserError(core, "A 'Subject' is required before sending an email.");
                                            } else {
                                                core.db.csSet(CS, "submitted", true);
                                                core.db.csSet(CS, "ConditionID", 0);
                                                if (core.db.csGetDate(CS, "ScheduleDate") == DateTime.MinValue) {
                                                    core.db.csSet(CS, "ScheduleDate", core.doc.profileStartTime);
                                                }
                                            }
                                            core.db.csClose(ref CS);
                                        }
                                    }
                                }
                                AdminAction = AdminActionNop; // convert so action can be used in as a refresh
                                                              //
                                break;
                            case AdminActionDeactivateEmail:
                                //
                                // ----- Deactivate (Conditional Email Only)
                                //
                                if (editRecord.Read_Only) {
                                    errorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    // no save, page was read only - Call ProcessActionSave
                                    LoadEditRecord(adminContent, editRecord);
                                    if (!(core.doc.debug_iUserError != "")) {
                                        if (!cdefModel.isWithinContent(core, editRecord.contentControlId, cdefModel.getContentId(core, "Conditional Email"))) {
                                            errorController.addUserError(core, "The deactivate action only supports Conditional Email.");
                                        } else {
                                            CS = core.db.csOpenRecord("Conditional Email", editRecord.id);
                                            if (!core.db.csOk(CS)) {
                                                //throw new ApplicationException("Unexpected exception"); // //throw new ApplicationException("Unexpected exception")' core.handleLegacyError23("Email ID [" & editRecord.id & "] could not be opened.")
                                            } else {
                                                core.db.csSet(CS, "submitted", false);
                                            }
                                            core.db.csClose(ref CS);
                                        }
                                    }
                                }
                                AdminAction = AdminActionNop; // convert so action can be used in as a refresh
                                break;
                            case AdminActionActivateEmail:
                                //
                                // ----- Activate (Conditional Email Only)
                                //
                                if (editRecord.Read_Only) {
                                    errorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    LoadEditRecord(adminContent, editRecord);
                                    LoadEditRecord_Request(adminContent, editRecord);
                                    ProcessActionSave(adminContent, editRecord, UseContentWatchLink);
                                    core.doc.processAfterSave(false, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink);
                                    if (!(core.doc.debug_iUserError != "")) {
                                        if (!cdefModel.isWithinContent(core, editRecord.contentControlId, cdefModel.getContentId(core, "Conditional Email"))) {
                                            errorController.addUserError(core, "The activate action only supports Conditional Email.");
                                        } else {
                                            CS = core.db.csOpenRecord("Conditional Email", editRecord.id);
                                            if (!core.db.csOk(CS)) {
                                                //throw new ApplicationException("Unexpected exception"); // //throw new ApplicationException("Unexpected exception")' core.handleLegacyError23("Email ID [" & editRecord.id & "] could not be opened.")
                                            } else if (core.db.csGetInteger(CS, "ConditionID") == 0) {
                                                errorController.addUserError(core, "A condition must be set.");
                                            } else {
                                                core.db.csSet(CS, "submitted", true);
                                                if (core.db.csGetDate(CS, "ScheduleDate") == DateTime.MinValue) {
                                                    core.db.csSet(CS, "ScheduleDate", core.doc.profileStartTime);
                                                }
                                            }
                                            core.db.csClose(ref CS);
                                        }
                                    }
                                }
                                AdminAction = AdminActionNop; // convert so action can be used in as a refresh
                                break;
                            case AdminActionSendEmailTest:
                                if (editRecord.Read_Only) {
                                    errorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    //
                                    LoadEditRecord(adminContent, editRecord);
                                    LoadEditRecord_Request(adminContent, editRecord);
                                    ProcessActionSave(adminContent, editRecord, UseContentWatchLink);
                                    core.doc.processAfterSave(false, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink);
                                    //
                                    if (!(core.doc.debug_iUserError != "")) {
                                        //
                                        EmailToConfirmationMemberID = 0;
                                        if (editRecord.fieldsLc.ContainsKey("testmemberid")) {
                                            EmailToConfirmationMemberID = genericController.encodeInteger(editRecord.fieldsLc["testmemberid"].value);
                                            emailController.sendConfirmationTest(core, editRecord.id, EmailToConfirmationMemberID);
                                            //
                                            if (editRecord.fieldsLc.ContainsKey("lastsendtestdate")) {
                                                editRecord.fieldsLc["lastsendtestdate"].value = core.doc.profileStartTime;
                                                core.db.executeQuery("update ccemail Set lastsendtestdate=" + core.db.encodeSQLDate(core.doc.profileStartTime) + " where id=" + editRecord.id);
                                            }
                                        }
                                    }
                                }
                                AdminAction = AdminActionNop; // convert so action can be used in as a refresh
                                                              // end case
                                break;
                            case AdminActionDeleteRows:
                                //
                                // Delete Multiple Rows
                                //
                                RowCnt = core.docProperties.getInteger("rowcnt");
                                if (RowCnt > 0) {
                                    for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                        if (core.docProperties.getBoolean("row" + RowPtr)) {
                                            CSEditRecord = core.db.cs_open2(adminContent.Name, core.docProperties.getInteger("rowid" + RowPtr), true, true);
                                            if (core.db.csOk(CSEditRecord)) {
                                                RecordID = core.db.csGetInteger(CSEditRecord, "ID");
                                                core.db.csDeleteRecord(CSEditRecord);
                                                if (!false) {
                                                    //
                                                    // non-Workflow Delete
                                                    //
                                                    ContentName = cdefModel.getContentNameByID(core, core.db.csGetInteger(CSEditRecord, "ContentControlID"));
                                                    core.cache.invalidate(cacheController.getCacheKey_Entity(adminContent.ContentTableName, RecordID));
                                                    core.doc.processAfterSave(true, ContentName, RecordID, "", 0, UseContentWatchLink);
                                                }
                                                //
                                                // Page Content special cases
                                                //
                                                if (genericController.vbLCase(adminContent.ContentTableName) == "ccpagecontent") {
                                                    //Call core.pages.cache_pageContent_removeRow(RecordID, False, False)
                                                    if (RecordID == (core.siteProperties.getInteger("PageNotFoundPageID", 0))) {
                                                        core.siteProperties.getText("PageNotFoundPageID", "0");
                                                    }
                                                    if (RecordID == (core.siteProperties.getInteger("LandingPageID", 0))) {
                                                        core.siteProperties.getText("LandingPageID", "0");
                                                    }
                                                }
                                            }
                                            core.db.csClose(ref CSEditRecord);
                                        }
                                    }
                                }
                                break;
                            case AdminActionReloadCDef:
                                //
                                // ccContent - save changes and reload content definitions
                                //
                                if (editRecord.Read_Only) {
                                    errorController.addUserError(core, "Your request was blocked because the record you specified Is now locked by another authcontext.user.");
                                } else {
                                    LoadEditRecord(adminContent, editRecord);
                                    LoadEditRecord_Request(adminContent, editRecord);
                                    ProcessActionSave(adminContent, editRecord, UseContentWatchLink);
                                    core.cache.invalidateAll();
                                    core.doc.clearMetaData();
                                }
                                AdminAction = AdminActionNop; // convert so action can be used in as a refresh
                                break;
                            default:
                                //
                                // Nop action or anything unrecognized - read in database
                                //
                                break;
                        }
                    }
                }
                //
                return;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                errorController.addUserError(core, "There was an unknown error processing this page at " + core.doc.profileStartTime + ". Please try again, Or report this error To the site administrator.");
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        // LoadAndSaveContentGroupRules
        //
        //   For a particular content, remove previous GroupRules, and Create new ones
        //========================================================================
        //
        private void LoadAndSaveContentGroupRules(int GroupID) {
            try {
                //
                int ContentCount = 0;
                int ContentPointer = 0;
                int CSPointer = 0;
                int ContentID = 0;
                bool AllowAdd = false;
                bool AllowDelete = false;
                int CSNew = 0;
                bool RecordChanged = false;
                bool RuleNeeded = false;
                bool RuleFound = false;
                string SQL = null;
                string DeleteIdList = "";
                int RuleId = 0;
                //
                // ----- Delete duplicate Group Rules
                //
                SQL = "Select distinct DuplicateRules.ID"
                    + " from ccgrouprules"
                    + " Left join ccgrouprules As DuplicateRules On DuplicateRules.ContentID=ccGroupRules.ContentID"
                    + " where ccGroupRules.ID < DuplicateRules.ID"
                    + " And ccGroupRules.GroupID=DuplicateRules.GroupID";
                SQL = "Delete from ccGroupRules where ID In (" + SQL + ")";
                core.db.executeQuery(SQL);
                //
                // --- create GroupRule records for all selected
                //
                CSPointer = core.db.csOpen("Group Rules", "GroupID=" + GroupID, "ContentID, ID", true);
                ContentCount = core.docProperties.getInteger("ContentCount");
                if (ContentCount > 0) {
                    for (ContentPointer = 0; ContentPointer < ContentCount; ContentPointer++) {
                        RuleNeeded = core.docProperties.getBoolean("Content" + ContentPointer);
                        ContentID = core.docProperties.getInteger("ContentID" + ContentPointer);
                        AllowAdd = core.docProperties.getBoolean("ContentGroupRuleAllowAdd" + ContentPointer);
                        AllowDelete = core.docProperties.getBoolean("ContentGroupRuleAllowDelete" + ContentPointer);
                        //
                        RuleFound = false;
                        core.db.cs_goFirst(CSPointer);
                        if (core.db.csOk(CSPointer)) {
                            while (core.db.csOk(CSPointer)) {
                                if (core.db.csGetInteger(CSPointer, "ContentID") == ContentID) {
                                    RuleId = core.db.csGetInteger(CSPointer, "id");
                                    RuleFound = true;
                                    break;
                                }
                                core.db.csGoNext(CSPointer);
                            }
                        }
                        if (RuleNeeded && !RuleFound) {
                            CSNew = core.db.csInsertRecord("Group Rules");
                            if (core.db.csOk(CSNew)) {
                                core.db.csSet(CSNew, "GroupID", GroupID);
                                core.db.csSet(CSNew, "ContentID", ContentID);
                                core.db.csSet(CSNew, "AllowAdd", AllowAdd);
                                core.db.csSet(CSNew, "AllowDelete", AllowDelete);
                            }
                            core.db.csClose(ref CSNew);
                            RecordChanged = true;
                        } else if (RuleFound && !RuleNeeded) {
                            DeleteIdList += ", " + RuleId;
                            //Call core.main_DeleteCSRecord(CSPointer)
                            RecordChanged = true;
                        } else if (RuleFound && RuleNeeded) {
                            if (AllowAdd != core.db.csGetBoolean(CSPointer, "AllowAdd")) {
                                core.db.csSet(CSPointer, "AllowAdd", AllowAdd);
                                RecordChanged = true;
                            }
                            if (AllowDelete != core.db.csGetBoolean(CSPointer, "AllowDelete")) {
                                core.db.csSet(CSPointer, "AllowDelete", AllowDelete);
                                RecordChanged = true;
                            }
                        }
                    }
                }
                core.db.csClose(ref CSPointer);
                if (!string.IsNullOrEmpty(DeleteIdList)) {
                    SQL = "delete from ccgrouprules where id In (" + DeleteIdList.Substring(1) + ")";
                    core.db.executeQuery(SQL);
                }
                if (RecordChanged) {
                    core.cache.invalidateAllInContent("Group Rules");
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        // LoadAndSaveGroupRules
        //   read groups from the edit form and modify Group Rules to match
        //========================================================================
        //
        private void LoadAndSaveGroupRules(editRecordClass editRecord) {
            try {
                //
                if (editRecord.id != 0) {
                    LoadAndSaveGroupRules_ForContentAndChildren(editRecord.id, "");
                }
                //
                return;
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        // LoadAndSaveGroupRules_ForContentAndChildren
        //   read groups from the edit form and modify Group Rules to match
        //========================================================================
        //
        private void LoadAndSaveGroupRules_ForContentAndChildren(int ContentID, string ParentIDString) {
            try {
                //
                int CSPointer = 0;
                string MyParentIDString = null;
                //
                // --- Create Group Rules for this content
                //
                if (encodeBoolean(ParentIDString.IndexOf("," + ContentID + ",") + 1)) {
                    throw (new Exception("Child ContentID [" + ContentID + "] Is its own parent"));
                } else {
                    MyParentIDString = ParentIDString + "," + ContentID + ",";
                    LoadAndSaveGroupRules_ForContent(ContentID);
                    //
                    // --- Create Group Rules for all child content
                    //
                    CSPointer = core.db.csOpen("Content", "ParentID=" + ContentID);
                    while (core.db.csOk(CSPointer)) {
                        LoadAndSaveGroupRules_ForContentAndChildren(core.db.csGetInteger(CSPointer, "id"), MyParentIDString);
                        core.db.csGoNext(CSPointer);
                    }
                    core.db.csClose(ref CSPointer);
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        // LoadAndSaveGroupRules_ForContent
        //
        //   For a particular content, remove previous GroupRules, and Create new ones
        //========================================================================
        //
        private void LoadAndSaveGroupRules_ForContent(int ContentID) {
            try {
                //
                int GroupCount = 0;
                int GroupPointer = 0;
                int CSPointer = 0;
                int GroupID = 0;
                bool AllowAdd = false;
                bool AllowDelete = false;
                int CSNew = 0;
                bool RecordChanged = false;
                bool RuleNeeded = false;
                bool RuleFound = false;
                string SQL;
                //
                // ----- Delete duplicate Group Rules
                //

                SQL = "Delete from ccGroupRules where ID In ("
                    + "Select distinct DuplicateRules.ID from ccgrouprules Left join ccgrouprules As DuplicateRules On DuplicateRules.GroupID=ccGroupRules.GroupID where ccGroupRules.ID < DuplicateRules.ID  And ccGroupRules.ContentID=DuplicateRules.ContentID"
                    + ")";
                core.db.executeQuery(SQL);
                //
                // --- create GroupRule records for all selected
                //
                CSPointer = core.db.csOpen("Group Rules", "ContentID=" + ContentID, "GroupID,ID", true);
                GroupCount = core.docProperties.getInteger("GroupCount");
                if (GroupCount > 0) {
                    for (GroupPointer = 0; GroupPointer < GroupCount; GroupPointer++) {
                        RuleNeeded = core.docProperties.getBoolean("Group" + GroupPointer);
                        GroupID = core.docProperties.getInteger("GroupID" + GroupPointer);
                        AllowAdd = core.docProperties.getBoolean("GroupRuleAllowAdd" + GroupPointer);
                        AllowDelete = core.docProperties.getBoolean("GroupRuleAllowDelete" + GroupPointer);
                        //
                        RuleFound = false;
                        core.db.cs_goFirst(CSPointer);
                        if (core.db.csOk(CSPointer)) {
                            while (core.db.csOk(CSPointer)) {
                                if (core.db.csGetInteger(CSPointer, "GroupID") == GroupID) {
                                    RuleFound = true;
                                    break;
                                }
                                core.db.csGoNext(CSPointer);
                            }
                        }
                        if (RuleNeeded && !RuleFound) {
                            CSNew = core.db.csInsertRecord("Group Rules");
                            if (core.db.csOk(CSNew)) {
                                core.db.csSet(CSNew, "ContentID", ContentID);
                                core.db.csSet(CSNew, "GroupID", GroupID);
                                core.db.csSet(CSNew, "AllowAdd", AllowAdd);
                                core.db.csSet(CSNew, "AllowDelete", AllowDelete);
                            }
                            core.db.csClose(ref CSNew);
                            RecordChanged = true;
                        } else if (RuleFound && !RuleNeeded) {
                            core.db.csDeleteRecord(CSPointer);
                            RecordChanged = true;
                        } else if (RuleFound && RuleNeeded) {
                            if (AllowAdd != core.db.csGetBoolean(CSPointer, "AllowAdd")) {
                                core.db.csSet(CSPointer, "AllowAdd", AllowAdd);
                                RecordChanged = true;
                            }
                            if (AllowDelete != core.db.csGetBoolean(CSPointer, "AllowDelete")) {
                                core.db.csSet(CSPointer, "AllowDelete", AllowDelete);
                                RecordChanged = true;
                            }
                        }
                    }
                }
                core.db.csClose(ref CSPointer);
                if (RecordChanged) {
                    core.cache.invalidateAllInContent("Group Rules");
                }
                return;
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        // Load Array
        //   Get defaults if no record ID
        //   Then load in any response elements
        //========================================================================
        //
        private void LoadEditRecord(cdefModel adminContent, editRecordClass editRecord, bool CheckUserErrors = false) {
            try {
                if (string.IsNullOrEmpty(adminContent.Name)) {
                    //
                    // Can not load edit record because bad content definition
                    //
                    if (adminContent.Id == 0) {
                        throw (new Exception("The record can Not be edited because no content definition was specified."));
                    } else {
                        throw (new Exception("The record can Not be edited because a content definition For ID [" + adminContent.Id + "] was Not found."));
                    }
                } else {
                    //
                    if (editRecord.id == 0) {
                        //
                        // ----- New record, just load defaults
                        //
                        LoadEditRecord_Default(adminContent, editRecord);
                        LoadEditRecord_WherePairs(adminContent, editRecord);
                    } else {
                        //
                        // ----- Load the Live Record specified
                        //
                        LoadEditRecord_Dbase(adminContent, ref editRecord, CheckUserErrors);
                        LoadEditRecord_WherePairs(adminContent, editRecord);
                    }
                    //        '
                    //        ' ----- Test for a change of admincontent (the record is a child of admincontent )
                    //        '
                    //        If EditRecord.ContentID <> AdminContent.Id Then
                    //            AdminContent = core.app.getCdef(EditRecord.ContentName)
                    //        End If
                    //
                    // ----- Capture core fields needed for processing
                    //
                    editRecord.menuHeadline = "";
                    if (editRecord.fieldsLc.ContainsKey("menuheadline")) {
                        editRecord.menuHeadline = genericController.encodeText(editRecord.fieldsLc["menuheadline"].value);
                    }
                    //
                    editRecord.menuHeadline = "";
                    if (editRecord.fieldsLc.ContainsKey("name")) {
                        //Dim editRecordField As editRecordFieldClass = editRecord.fieldsLc["name")
                        //editRecord.nameLc = editRecordField.value.ToString()
                        editRecord.nameLc = genericController.encodeText(editRecord.fieldsLc["name"].value);
                    }
                    //
                    editRecord.menuHeadline = "";
                    if (editRecord.fieldsLc.ContainsKey("active")) {
                        editRecord.active = genericController.encodeBoolean(editRecord.fieldsLc["active"].value);
                    }
                    //
                    editRecord.menuHeadline = "";
                    if (editRecord.fieldsLc.ContainsKey("contentcontrolid")) {
                        editRecord.contentControlId = genericController.encodeInteger(editRecord.fieldsLc["contentcontrolid"].value);
                    }
                    //
                    editRecord.menuHeadline = "";
                    if (editRecord.fieldsLc.ContainsKey("parentid")) {
                        editRecord.parentID = genericController.encodeInteger(editRecord.fieldsLc["parentid"].value);
                    }
                    //
                    editRecord.menuHeadline = "";
                    if (editRecord.fieldsLc.ContainsKey("rootpageid")) {
                        editRecord.RootPageID = genericController.encodeInteger(editRecord.fieldsLc["rootpageid"].value);
                    }
                    //
                    // ----- Set the local global copy of Edit Record Locks
                    //
                    core.doc.getAuthoringStatus(adminContent.Name, editRecord.id, ref editRecord.SubmitLock, ref editRecord.ApproveLock, ref editRecord.SubmittedName, ref editRecord.ApprovedName, ref editRecord.IsInserted, ref editRecord.IsDeleted, ref editRecord.IsModified, ref editRecord.LockModifiedName, ref editRecord.LockModifiedDate, ref editRecord.SubmittedDate, ref editRecord.ApprovedDate);
                    //
                    // ----- Set flags used to determine the Authoring State
                    //
                    core.doc.getAuthoringPermissions(adminContent.Name, editRecord.id, ref editRecord.AllowInsert, ref editRecord.AllowCancel, ref editRecord.AllowSave, ref editRecord.AllowDelete, ref editRecord.AllowPublish, ref editRecord.AllowAbort, ref editRecord.AllowSubmit, ref editRecord.AllowApprove, ref editRecord.Read_Only);
                    //
                    // ----- Set Edit Lock
                    //
                    if (editRecord.id != 0) {
                        editRecord.EditLock = core.workflow.GetEditLockStatus(adminContent.Name, editRecord.id);
                        if (editRecord.EditLock) {
                            editRecord.EditLockMemberName = core.workflow.GetEditLockMemberName(adminContent.Name, editRecord.id);
                            editRecord.EditLockExpires = core.workflow.GetEditLockDateExpires(adminContent.Name, editRecord.id);
                        }
                    }
                    //
                    // ----- Set Read Only: for edit lock
                    //
                    if (editRecord.EditLock) {
                        editRecord.Read_Only = true;
                    }
                    //
                    // ----- Set Read Only: if non-developer tries to edit a developer record
                    //
                    if (genericController.vbUCase(adminContent.ContentTableName) == genericController.vbUCase("ccMembers")) {
                        if (!core.doc.sessionContext.isAuthenticatedDeveloper(core)) {
                            if (editRecord.fieldsLc.ContainsKey("developer")) {
                                if (genericController.encodeBoolean(editRecord.fieldsLc["developer"].value)) {
                                    editRecord.Read_Only = true;
                                    errorController.addUserError(core, "You Do Not have access rights To edit this record.");
                                    BlockEditForm = true;
                                }
                            }
                        }
                    }
                    //
                    // ----- Now make sure this record is locked from anyone else
                    //
                    if (!(editRecord.Read_Only)) {
                        core.workflow.SetEditLock(adminContent.Name, editRecord.id);
                    }
                    editRecord.Loaded = true;
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        //   Get the Wherepair value for a fieldname
        //       If there is a match with the left side, return the right
        //       If no match, return ""
        //========================================================================
        //
        private string GetWherePairValue(string FieldName) {
            string tempGetWherePairValue = null;
            try {
                //
                int WhereCount = 0;
                //
                FieldName = genericController.vbUCase(FieldName);
                //
                tempGetWherePairValue = "";
                if (WherePairCount > 0) {
                    for (WhereCount = 0; WhereCount < WherePairCount; WhereCount++) {
                        if (FieldName == genericController.vbUCase(WherePair[0, WhereCount])) {
                            tempGetWherePairValue = WherePair[1, WhereCount];
                            break;
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetWherePairValue;
        }
        //
        //========================================================================
        //   Load both Live and Edit Record values from definition defaults
        //========================================================================
        //
        private void LoadEditRecord_Default(cdefModel adminContent, editRecordClass editrecord) {
            try {
                string DefaultValueText = null;
                string LookupContentName = null;
                string UCaseDefaultValueText = null;
                string[] lookups = null;
                int Ptr = 0;
                string defaultValue = null;
                editRecordFieldClass editRecordField = null;
                cdefFieldModel field = null;
                editrecord.active = true;
                editrecord.contentControlId = adminContent.Id;
                editrecord.contentControlId_Name = adminContent.Name;
                editrecord.EditLock = false;
                editrecord.Loaded = false;
                editrecord.Saved = false;
                foreach (var keyValuePair in adminContent.fields) {
                    field = keyValuePair.Value;
                    if (!(editrecord.fieldsLc.ContainsKey(field.nameLc))) {
                        editRecordField = new editRecordFieldClass();
                        editrecord.fieldsLc.Add(field.nameLc, editRecordField);
                    }
                    defaultValue = field.defaultValue;
                    //    End If
                    if (field.active & !genericController.IsNull(defaultValue)) {
                        switch (field.fieldTypeId) {
                            case FieldTypeIdInteger:
                            case FieldTypeIdAutoIdIncrement:
                            case FieldTypeIdMemberSelect:
                                //
                                editrecord.fieldsLc[field.nameLc].value = genericController.encodeInteger(defaultValue);
                                break;
                            case FieldTypeIdCurrency:
                            case FieldTypeIdFloat:
                                //
                                editrecord.fieldsLc[field.nameLc].value = genericController.encodeNumber(defaultValue);
                                break;
                            case FieldTypeIdBoolean:
                                //
                                editrecord.fieldsLc[field.nameLc].value = genericController.encodeBoolean(defaultValue);
                                break;
                            case FieldTypeIdDate:
                                //
                                editrecord.fieldsLc[field.nameLc].value = genericController.encodeDate(defaultValue);
                                break;
                            case FieldTypeIdLookup:

                                DefaultValueText = genericController.encodeText(field.defaultValue);
                                if (!string.IsNullOrEmpty(DefaultValueText)) {
                                    if (DefaultValueText.IsNumeric()) {
                                        editrecord.fieldsLc[field.nameLc].value = DefaultValueText;
                                    } else {
                                        if (field.lookupContentID != 0) {
                                            LookupContentName = cdefModel.getContentNameByID(core, field.lookupContentID);
                                            if (!string.IsNullOrEmpty(LookupContentName)) {
                                                editrecord.fieldsLc[field.nameLc].value = core.db.getRecordID(LookupContentName, DefaultValueText);
                                            }
                                        } else if (field.lookupList != "") {
                                            UCaseDefaultValueText = genericController.vbUCase(DefaultValueText);
                                            lookups = field.lookupList.Split(',');
                                            for (Ptr = 0; Ptr <= lookups.GetUpperBound(0); Ptr++) {
                                                if (UCaseDefaultValueText == genericController.vbUCase(lookups[Ptr])) {
                                                    editrecord.fieldsLc[field.nameLc].value = Ptr + 1;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                break;
                            default:
                                //
                                editrecord.fieldsLc[field.nameLc].value = genericController.encodeText(defaultValue);
                                break;
                        }
                    }
                    //
                    // process reserved fields (set defaults just makes it look good)
                    // (also, this presets readonly/devonly/adminonly fields not set to member)
                    //
                    switch (genericController.vbUCase(field.nameLc)) {
                        //Case "ID"
                        //    .readonlyfield = True
                        //    .Required = False
                        case "MODIFIEDBY":
                            editrecord.fieldsLc[field.nameLc].value = core.doc.sessionContext.user.id;
                            //    .readonlyfield = True
                            //    .Required = False
                            break;
                        case "CREATEDBY":
                            editrecord.fieldsLc[field.nameLc].value = core.doc.sessionContext.user.id;
                            //    .readonlyfield = True
                            //    .Required = False
                            //Case "DATEADDED"
                            //    .readonlyfield = True
                            //    .Required = False
                            break;
                        case "CONTENTCONTROLID":
                            editrecord.fieldsLc[field.nameLc].value = adminContent.Id;
                            //Case "SORTORDER"
                            // default to ID * 100, but must be done later
                            break;
                    }
                    editrecord.fieldsLc[field.nameLc].dbValue = editrecord.fieldsLc[field.nameLc].value;
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //========================================================================
        //   Load both Live and Edit Record values from definition defaults
        //========================================================================
        //
        private void LoadEditRecord_WherePairs(cdefModel Admincontent, editRecordClass editRecord) {
            try {
                string DefaultValueText = null;
                foreach (var keyValuePair in Admincontent.fields) {
                    cdefFieldModel field = keyValuePair.Value;
                    DefaultValueText = GetWherePairValue(field.nameLc);
                    if (field.active & (!string.IsNullOrEmpty(DefaultValueText))) {
                        switch (field.fieldTypeId) {
                            case FieldTypeIdInteger:
                            case FieldTypeIdLookup:
                            case FieldTypeIdAutoIdIncrement:
                                //
                                editRecord.fieldsLc[field.nameLc].value = genericController.encodeInteger(DefaultValueText);
                                break;
                            case FieldTypeIdCurrency:
                            case FieldTypeIdFloat:
                                //
                                editRecord.fieldsLc[field.nameLc].value = genericController.encodeNumber(DefaultValueText);
                                break;
                            case FieldTypeIdBoolean:
                                //
                                editRecord.fieldsLc[field.nameLc].value = genericController.encodeBoolean(DefaultValueText);
                                break;
                            case FieldTypeIdDate:
                                //
                                editRecord.fieldsLc[field.nameLc].value = genericController.encodeDate(DefaultValueText);
                                break;
                            case FieldTypeIdManyToMany:
                                //
                                // Many to Many can capture a list of ID values representing the 'secondary' values in the Many-To-Many Rules table
                                //
                                editRecord.fieldsLc[field.nameLc].value = DefaultValueText;
                                break;
                            default:
                                //
                                editRecord.fieldsLc[field.nameLc].value = DefaultValueText;
                                break;
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        //   Load Records from the database
        //========================================================================
        //
        private void LoadEditRecord_Dbase(cdefModel adminContent, ref editRecordClass editrecord, bool CheckUserErrors = false) {
            try {
                //
                object DBValueVariant = null;
                int CSEditRecord = 0;
                object NullVariant = null;
                int CSPointer = 0;
                //Dim WorkflowAuthoring As Boolean
                //
                // ----- test for content problem
                //
                if (editrecord.id == 0) {
                    //
                    // ----- Skip load, this is a new record
                    //
                } else if (adminContent.Id == 0) {
                    //
                    // ----- Error: no content ID
                    //
                    BlockEditForm = true;
                    errorController.addUserError(core, "No content definition was found For Content ID [" + editrecord.id + "]. Please contact your application developer For more assistance.");
                    handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", "No content definition was found For Content ID [" + editrecord.id + "].");
                } else if (string.IsNullOrEmpty(adminContent.Name)) {
                    //
                    // ----- Error: no content name
                    //
                    BlockEditForm = true;
                    errorController.addUserError(core, "No content definition could be found For ContentID [" + adminContent.Id + "]. This could be a menu Error. Please contact your application developer For more assistance.");
                    handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", "No content definition For ContentID [" + adminContent.Id + "] could be found.");
                } else if (adminContent.ContentTableName == "") {
                    //
                    // ----- Error: no content table
                    //
                    BlockEditForm = true;
                    errorController.addUserError(core, "The content definition [" + adminContent.Name + "] Is Not associated With a valid database table. Please contact your application developer For more assistance.");
                    handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", "No content definition For ContentID [" + adminContent.Id + "] could be found.");
                    //
                    // move block to the edit and listing pages - to handle content editor cases - so they can edit 'pages', and just get the records they are allowed
                    //
                    //    ElseIf Not UserAllowContentEdit Then
                    //        '
                    //        ' ----- Error: load blocked by UserAllowContentEdit
                    //        '
                    //        BlockEditForm = True
                    //        Call core.htmldoc.main_AddUserError("Your account On this system does Not have access rights To edit this content.")
                    //        Call HandleInternalError("AdminClass.LoadEditRecord_Dbase", "User does Not have access To this content")
                } else if (adminContent.fields.Count == 0) {
                    //
                    // ----- Error: content definition is not complete
                    //
                    BlockEditForm = true;
                    errorController.addUserError(core, "The content definition [" + adminContent.Name + "] has no field records defined. Please contact your application developer For more assistance.");
                    handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", "Content [" + adminContent.Name + "] has no fields defined.");
                } else {
                    //
                    //   Open Content Sets with the data
                    //
                    CSEditRecord = core.db.cs_open2(adminContent.Name, editrecord.id, true, true);
                    //
                    //
                    // store fieldvalues in RecordValuesVariant
                    //
                    if (!(core.db.csOk(CSEditRecord))) {
                        //
                        //   Live or Edit records were not found
                        //
                        BlockEditForm = true;
                        errorController.addUserError(core, "The information you have requested could not be found. The record could have been deleted, Or there may be a system Error.");
                        // removed because it was throwing too many false positives (1/14/04 - tried to do it again)
                        // If a CM hits the edit tag for a deleted record, this is hit. It should not cause the Developers to spend hours running down.
                        //Call HandleInternalError("AdminClass.LoadEditRecord_Dbase", "Content edit record For [" & AdminContent.Name & "." & EditRecord.ID & "] was Not found.")
                    } else {
                        //
                        // Read database values into RecordValuesVariant array
                        //
                        NullVariant = null;
                        foreach (var keyValuePair in adminContent.fields) {
                            cdefFieldModel adminContentField = keyValuePair.Value;
                            string fieldNameLc = adminContentField.nameLc;
                            editRecordFieldClass editRecordField = null;
                            //
                            // set editRecord.field to editRecordField and set values
                            //
                            if (!editrecord.fieldsLc.ContainsKey(fieldNameLc)) {
                                editRecordField = new editRecordFieldClass();
                                editrecord.fieldsLc.Add(fieldNameLc, editRecordField);
                            } else {
                                editRecordField = editrecord.fieldsLc[fieldNameLc];
                            }
                            //
                            // 1/21/2007 - added clause if required and null, set to default value
                            //
                            object fieldValue = NullVariant;
                            if (adminContentField.readOnly | adminContentField.notEditable) {
                                //
                                // 202-31245: quick fix. The CS should handle this instead.
                                // Workflowauthoring, If read only, use the live record data
                                //
                                CSPointer = CSEditRecord;
                            } else {
                                CSPointer = CSEditRecord;
                            }
                            //
                            // Load the current Database value
                            //
                            switch (adminContentField.fieldTypeId) {
                                case FieldTypeIdRedirect:
                                case FieldTypeIdManyToMany:
                                    DBValueVariant = "";
                                    break;
                                case FieldTypeIdFileText:
                                case FieldTypeIdFileCSS:
                                case FieldTypeIdFileXML:
                                case FieldTypeIdFileJavascript:
                                case FieldTypeIdFileHTML:
                                    DBValueVariant = core.db.csGet(CSPointer, adminContentField.nameLc);
                                    break;
                                default:
                                    DBValueVariant = core.db.cs_getValue(CSPointer, adminContentField.nameLc);
                                    break;
                            }
                            //
                            // Check for required and null case loading error
                            //
                            if (CheckUserErrors && adminContentField.required & (genericController.IsNull(DBValueVariant))) {
                                //
                                // if required and null
                                //
                                if (string.IsNullOrEmpty(adminContentField.defaultValue)) {
                                    //
                                    // default is null
                                    //
                                    if (adminContentField.editTabName == "") {
                                        errorController.addUserError(core, "The value for [" + adminContentField.caption + "] was empty but is required. This must be set before you can save this record.");
                                    } else {
                                        errorController.addUserError(core, "The value for [" + adminContentField.caption + "] in tab [" + adminContentField.editTabName + "] was empty but is required. This must be set before you can save this record.");
                                    }
                                } else {
                                    //
                                    // if required and null, set value to the default
                                    //
                                    DBValueVariant = adminContentField.defaultValue;
                                    if (adminContentField.editTabName == "") {
                                        errorController.addUserError(core, "The value for [" + adminContentField.caption + "] was null but is required. The default value Is shown, And will be saved if you save this record.");
                                    } else {
                                        errorController.addUserError(core, "The value for [" + adminContentField.caption + "] in tab [" + adminContentField.editTabName + "] was null but is required. The default value Is shown, And will be saved if you save this record.");
                                    }
                                }
                            }
                            //
                            // Save EditRecord values
                            //
                            switch (genericController.vbUCase(adminContentField.nameLc)) {
                                case "DATEADDED":
                                    editrecord.dateAdded = core.db.csGetDate(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "MODIFIEDDATE":
                                    editrecord.modifiedDate = core.db.csGetDate(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "CREATEDBY":
                                    editrecord.createByMemberId = core.db.csGetInteger(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "MODIFIEDBY":
                                    editrecord.modifiedByMemberID = core.db.csGetInteger(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "ACTIVE":
                                    editrecord.active = core.db.csGetBoolean(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "CONTENTCONTROLID":
                                    editrecord.contentControlId = core.db.csGetInteger(CSEditRecord, adminContentField.nameLc);
                                    editrecord.contentControlId_Name = cdefModel.getContentNameByID(core, editrecord.contentControlId);
                                    break;
                                case "ID":
                                    editrecord.id = core.db.csGetInteger(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "MENUHEADLINE":
                                    editrecord.menuHeadline = core.db.csGetText(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "NAME":
                                    editrecord.nameLc = core.db.csGetText(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "PARENTID":
                                    editrecord.parentID = core.db.csGetInteger(CSEditRecord, adminContentField.nameLc);
                                    //Case Else
                                    //    EditRecordValuesVariant(FieldPointer) = DBValueVariant
                                    break;
                            }
                            //
                            editRecordField.dbValue = DBValueVariant;
                            editRecordField.value = DBValueVariant;
                        }
                    }
                    core.db.csClose(ref CSEditRecord);
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //========================================================================
        //   Read the Form into the fields array
        //========================================================================
        //
        private void LoadEditRecord_Request(cdefModel adminContent, editRecordClass editRecord) {
            try {
                int PageNotFoundPageID = 0;
                //
                // List of fields that were created for the form, and should be verified (starts and ends with a comma)
                var FormFieldLcListToBeLoaded = new List<string> { };
                string formFieldList = core.docProperties.getText("FormFieldList");
                if (!string.IsNullOrWhiteSpace(formFieldList)) {
                    FormFieldLcListToBeLoaded.AddRange(formFieldList.ToLower().Split(','));
                    // -- remove possible front and end spaces
                    if (FormFieldLcListToBeLoaded.Contains("")) {
                        FormFieldLcListToBeLoaded.Remove("");
                        if (FormFieldLcListToBeLoaded.Contains("")) {
                            FormFieldLcListToBeLoaded.Remove("");
                        }
                    }
                }
                //
                // List of fields coming from the form that are empty -- and should not be in stream (starts and ends with a comma)
                var FormEmptyFieldLcList = new List<string> { };
                string emptyFieldList = core.docProperties.getText("FormEmptyFieldList");
                if (!string.IsNullOrWhiteSpace(emptyFieldList)) {
                    FormEmptyFieldLcList.AddRange(emptyFieldList.ToLower().Split(','));
                    // -- remove possible front and end spaces
                    if (FormEmptyFieldLcList.Contains("")) {
                        FormEmptyFieldLcList.Remove("");
                        if (FormEmptyFieldLcList.Contains("")) {
                            FormEmptyFieldLcList.Remove("");
                        }
                    }
                }
                //
                if (AllowAdminFieldCheck() && (FormFieldLcListToBeLoaded.Count == 0)) {
                    //
                    // The field list was not returned
                    errorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [no field list].");
                } else if (AllowAdminFieldCheck() && (FormEmptyFieldLcList.Count == 0)) {
                    //
                    // The field list was not returned
                    errorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [no empty field list].");
                } else {
                    //
                    // fixup the string so it can be reduced by each field found, leaving and empty string if all correct
                    //
                    var tmpList = new List<string>();
                    dataSourceModel datasource = dataSourceModel.create(core, adminContent.dataSourceId, ref tmpList);
                    //DataSourceName = core.db.getDataSourceNameByID(adminContent.dataSourceId)
                    foreach (var keyValuePair in adminContent.fields) {
                        cdefFieldModel field = keyValuePair.Value;
                        LoadEditRecord_RequestField(adminContent, editRecord, field, datasource.Name, FormFieldLcListToBeLoaded, FormEmptyFieldLcList);
                    }
                    //
                    // If there are any form fields that were no loaded, flag the error now
                    //
                    if (AllowAdminFieldCheck() & (FormFieldLcListToBeLoaded.Count > 0)) {
                        errorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The following fields where Not found [" + string.Join(",", FormFieldLcListToBeLoaded) + "].");
                        throw (new ApplicationException("Unexpected exception")); // core.handleLegacyError2("AdminClass", "LoadEditResponse", core.appConfig.name & ", There were fields In the fieldlist sent out To the browser that did Not Return, [" & Mid(FormFieldListToBeLoaded, 2, Len(FormFieldListToBeLoaded) - 2) & "]")
                    } else {
                        //
                        // if page content, check for the 'pagenotfound','landingpageid' checkboxes in control tab
                        //
                        if (genericController.vbLCase(adminContent.ContentTableName) == "ccpagecontent") {
                            //
                            PageNotFoundPageID = (core.siteProperties.getInteger("PageNotFoundPageID", 0));
                            if (core.docProperties.getBoolean("PageNotFound")) {
                                editRecord.SetPageNotFoundPageID = true;
                            } else if (editRecord.id == PageNotFoundPageID) {
                                core.siteProperties.setProperty("PageNotFoundPageID", "0");
                            }
                            //
                            if (core.docProperties.getBoolean("LandingPageID")) {
                                editRecord.SetLandingPageID = true;
                            } else if (editRecord.id == 0) {
                                //
                                // New record, allow it to be set, but do not compare it to LandingPageID
                                //
                            } else if (editRecord.id == core.siteProperties.landingPageID) {
                                //
                                // Do not reset the LandingPageID from here -- set another instead
                                //
                                errorController.addUserError(core, "This page was marked As the Landing Page For the website, And the checkbox has been cleared. This Is Not allowed. To remove this page As the Landing Page, locate a New landing page And Select it, Or go To Settings &gt; Page Settings And Select a New Landing Page.");
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        //   Read in a Response value by name
        //========================================================================
        //
        //Private Sub LoadEditResponseByName(FieldName As String)
        //    On Error GoTo //ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.LoadEditResponseByName")
        //    '
        //    ' converted array to dictionary - Dim FieldPointer As Integer
        //    Dim FieldFound As Boolean
        //    Dim UcaseFieldName As String
        //    Dim DataSourceName As String
        //    Dim FormID As String
        //    '
        //    FieldFound = False
        //    DataSourceName = core.db.getDataSourceNameByID(AdminContent.DataSourceID)
        //    If (FieldName <> "") Then
        //        UcaseFieldName = genericController.vbUCase(FieldName)
        //        If AdminContent.fields.count > 0 Then
        //            For FieldPointer = 0 To AdminContent.fields.count - 1
        //                If genericController.vbUCase(AdminContent.fields(FieldPointer).Name) = UcaseFieldName Then
        //                    Call LoadEditResponseByPointer(FormID, FieldPointer, DataSourceName)
        //                    FieldFound = True
        //                    Exit For
        //                    End If
        //                Next
        //            End If
        //        End If
        //    If Not FieldFound Then
        //        Call HandleInternalError("AdminClass.LoadEditResponseByName", "Field [" & FieldName & "] was Not found In content [" & AdminContent.Name & "]")
        //        End If
        //    '
        //    '''Dim th as integer: Exit Sub
        //    '
        //    ' ----- Error Trap
        //    '
        ////ErrorTrap:
        //    Call HandleClassTrapErrorBubble("LoadEditResponseByName")
        //    '
        //End Sub
        //
        //========================================================================
        //   Read the Form into the fields array
        //========================================================================
        //
        private void LoadEditRecord_RequestField(cdefModel adminContent, editRecordClass editRecord, cdefFieldModel field, string ignore, List<string> FormFieldLcListToBeLoaded, List<string> FormEmptyFieldLcList) {
            try {
                if (field.active) {
                    const int LoopPtrMax = 100;
                    bool blockDuplicateUsername = false;
                    bool blockDuplicateEmail = false;
                    string lcaseCopy = null;
                    int EditorPixelHeight = 0;
                    int EditorRowHeight = 0;
                    int CSPointer = 0;
                    bool ResponseFieldIsEmpty = false;
                    htmlParserController HTML = new htmlParserController(core);
                    int ParentID = 0;
                    string UsedIDs = null;
                    int LoopPtr = 0;
                    int CS = 0;
                    bool ResponseFieldValueIsOKToSave = true;
                    bool InEmptyFieldList = FormEmptyFieldLcList.Contains(field.nameLc);
                    bool InLoadedFieldList = FormFieldLcListToBeLoaded.Contains(field.nameLc);
                    if (InLoadedFieldList) {
                        FormFieldLcListToBeLoaded.Remove(field.nameLc);
                    }
                    bool InResponse = core.docProperties.containsKey(field.nameLc);
                    string ResponseFieldValueText = core.docProperties.getText(field.nameLc);
                    ResponseFieldIsEmpty = string.IsNullOrEmpty(ResponseFieldValueText);
                    string TabCopy = "";
                    if (field.editTabName != "") {
                        TabCopy = " In the " + field.editTabName + " tab";
                    }
                    //
                    // -- process reserved fields
                    switch (field.nameLc) {
                        case "contentcontrolid":
                            //
                            //
                            if (AllowAdminFieldCheck()) {
                                if (!core.docProperties.containsKey(field.nameLc.ToUpper())) {
                                    if (!(core.doc.debug_iUserError != "")) {
                                        //
                                        // Add user error only for the first missing field
                                        //
                                        errorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try again, taking care Not To submit the page until your browser has finished loading. If this Error occurs again, please report this problem To your site administrator. The first Error was [" + field.nameLc + " Not found]. There may have been others.");
                                    }
                                    throw (new ApplicationException("Unexpected exception")); // core.handleLegacyError2("AdminClass", "LoadEditResponse", core.appConfig.name & ", Field [" & FieldName & "] was In the forms field list, but Not found In the response stream.")
                                }
                            }
                            if (genericController.encodeInteger(ResponseFieldValueText) != genericController.encodeInteger(editRecord.fieldsLc[field.nameLc].value)) {
                                //
                                // new value
                                editRecord.fieldsLc[field.nameLc].value = ResponseFieldValueText;
                                ResponseFieldIsEmpty = false;
                            }
                            break;
                        case "active":
                            //
                            //
                            if (AllowAdminFieldCheck() && (!InResponse) && (!InEmptyFieldList)) {
                                errorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [" + field.nameLc + " Not found].");
                                return;
                            }
                            //
                            bool responseValue = core.docProperties.getBoolean(field.nameLc);

                            if (!responseValue.Equals(encodeBoolean(editRecord.fieldsLc[field.nameLc].value))) {
                                //
                                // new value
                                //
                                editRecord.fieldsLc[field.nameLc].value = responseValue;
                                ResponseFieldIsEmpty = false;
                            }
                            break;
                        case "ccguid":
                            //
                            //
                            //
                            InEmptyFieldList = FormEmptyFieldLcList.Contains(field.nameLc);
                            InResponse = core.docProperties.containsKey(field.nameLc);
                            if (AllowAdminFieldCheck()) {
                                if ((!InResponse) && (!InEmptyFieldList)) {
                                    errorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [" + field.nameLc + " Not found].");
                                    return;
                                }
                            }
                            if (ResponseFieldValueText != editRecord.fieldsLc[field.nameLc].value.ToString()) {
                                //
                                // new value
                                editRecord.fieldsLc[field.nameLc].value = ResponseFieldValueText;
                                ResponseFieldIsEmpty = false;
                            }
                            break;
                        case "id":
                        case "modifiedby":
                        case "modifieddate":
                        case "createdby":
                        case "dateadded":
                            //
                            // -----Control fields that cannot be edited
                            ResponseFieldValueIsOKToSave = false;
                            break;
                        default:
                            //
                            // ----- Read response for user fields
                            //       9/24/2009 - if fieldname is not in FormFieldListToBeLoaded, go with what is there (Db value or default value)
                            //
                            if (!field.authorable) {
                                //
                                // Is blocked from authoring, leave current value
                                //
                                ResponseFieldValueIsOKToSave = false;
                            } else if ((field.fieldTypeId == FieldTypeIdAutoIdIncrement) || (field.fieldTypeId == FieldTypeIdRedirect) || (field.fieldTypeId == FieldTypeIdManyToMany)) {
                                //
                                // These fields types have no values to load, leave current value
                                // (many to many is handled during save)
                                //
                                ResponseFieldValueIsOKToSave = false;
                            } else if ((field.adminOnly) & (!core.doc.sessionContext.isAuthenticatedAdmin(core))) {
                                //
                                // non-admin and admin only field, leave current value
                                //
                                ResponseFieldValueIsOKToSave = false;
                            } else if ((field.developerOnly) & (!core.doc.sessionContext.isAuthenticatedDeveloper(core))) {
                                //
                                // non-developer and developer only field, leave current value
                                //
                                ResponseFieldValueIsOKToSave = false;
                            } else if ((field.readOnly) | (field.notEditable & (editRecord.id != 0))) {
                                //
                                // read only field, leave current
                                //
                                ResponseFieldValueIsOKToSave = false;
                            } else if (!InLoadedFieldList) {
                                //
                                // Was not sent out, so just go with the current value. Also, if the loaded field list is not returned, and the field is not returned, this is the bestwe can do.
                                ResponseFieldValueIsOKToSave = false;
                            } else if (AllowAdminFieldCheck() && (!InResponse) && (!InEmptyFieldList)) {
                                //
                                // Was sent out non-blank, and no response back, flag error and leave the current value to a retry
                                string errorMessage = "There has been an Error reading the response from your browser.The field[" + field.caption + "]" + TabCopy + " was missing. Please Try your change again.If this Error happens repeatedly, please report this problem To your site administrator.";
                                errorController.addUserError(core, errorMessage);
                                core.handleException(new ApplicationException(errorMessage));
                                ResponseFieldValueIsOKToSave = false;
                            } else {
                                //
                                // Test input value for valid data
                                //
                                switch (field.fieldTypeId) {
                                    case FieldTypeIdInteger:
                                        //
                                        // ----- Integer
                                        //
                                        ResponseFieldIsEmpty = ResponseFieldIsEmpty || (string.IsNullOrEmpty(ResponseFieldValueText));
                                        if (!ResponseFieldIsEmpty) {
                                            if (ResponseFieldValueText.IsNumeric()) {
                                                //ResponseValueVariant = genericController.EncodeInteger(ResponseValueVariant)
                                            } else {
                                                errorController.addUserError(core, "The record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be a numeric value.");
                                                ResponseFieldValueIsOKToSave = false;
                                            }
                                        }
                                        break;
                                    case FieldTypeIdCurrency:
                                    case FieldTypeIdFloat:
                                        //
                                        // ----- Floating point number
                                        //
                                        ResponseFieldIsEmpty = ResponseFieldIsEmpty || (string.IsNullOrEmpty(ResponseFieldValueText));
                                        if (!ResponseFieldIsEmpty) {
                                            if (ResponseFieldValueText.IsNumeric()) {
                                                //ResponseValueVariant = EncodeNumber(ResponseValueVariant)
                                            } else {
                                                errorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be a numeric value.");
                                                ResponseFieldValueIsOKToSave = false;
                                            }
                                        }
                                        break;
                                    case FieldTypeIdLookup:
                                        //
                                        // ----- Must be a recordID
                                        //
                                        ResponseFieldIsEmpty = ResponseFieldIsEmpty || (string.IsNullOrEmpty(ResponseFieldValueText));
                                        if (!ResponseFieldIsEmpty) {
                                            if (ResponseFieldValueText.IsNumeric()) {
                                                //ResponseValueVariant = genericController.EncodeInteger(ResponseValueVariant)
                                            } else {
                                                errorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " had an invalid selection.");
                                                ResponseFieldValueIsOKToSave = false;
                                            }
                                        }
                                        break;
                                    case FieldTypeIdDate:
                                        //
                                        // ----- Must be a Date value
                                        //
                                        ResponseFieldIsEmpty = ResponseFieldIsEmpty || (string.IsNullOrEmpty(ResponseFieldValueText));
                                        if (!ResponseFieldIsEmpty) {
                                            if (!dateController.IsDate(ResponseFieldValueText)) {
                                                errorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be a date And/Or time in the form mm/dd/yy 0000 AM(PM).");
                                                ResponseFieldValueIsOKToSave = false;
                                            }
                                        }
                                        //End Case
                                        break;
                                    case FieldTypeIdBoolean:
                                        //
                                        // ----- translate to boolean
                                        //
                                        ResponseFieldValueText = genericController.encodeBoolean(ResponseFieldValueText).ToString();
                                        break;
                                    case FieldTypeIdLink:
                                        //
                                        // ----- Link field - if it starts with 'www.', add the http:// automatically
                                        //
                                        ResponseFieldValueText = genericController.encodeText(ResponseFieldValueText);
                                        if (ResponseFieldValueText.ToLower().Left(4) == "www.") {
                                            ResponseFieldValueText = "http//" + ResponseFieldValueText;
                                        }
                                        break;
                                    case FieldTypeIdHTML:
                                    case FieldTypeIdFileHTML:
                                        //
                                        // ----- Html fields
                                        //
                                        EditorRowHeight = core.docProperties.getInteger(field.nameLc + "Rows");
                                        if (EditorRowHeight != 0) {
                                            core.userProperty.setProperty(adminContent.Name + "." + field.nameLc + ".RowHeight", EditorRowHeight);
                                        }
                                        EditorPixelHeight = core.docProperties.getInteger(field.nameLc + "PixelHeight");
                                        if (EditorPixelHeight != 0) {
                                            core.userProperty.setProperty(adminContent.Name + "." + field.nameLc + ".PixelHeight", EditorPixelHeight);
                                        }
                                        //
                                        if (!field.htmlContent) {
                                            lcaseCopy = genericController.vbLCase(ResponseFieldValueText);
                                            lcaseCopy = genericController.vbReplace(lcaseCopy, "\r", "");
                                            lcaseCopy = genericController.vbReplace(lcaseCopy, "\n", "");
                                            lcaseCopy = lcaseCopy.Trim(' ');
                                            if ((lcaseCopy == HTMLEditorDefaultCopyNoCr) || (lcaseCopy == HTMLEditorDefaultCopyNoCr2)) {
                                                //
                                                // if the editor was left blank, remote the default copy
                                                //
                                                ResponseFieldValueText = "";
                                            } else {
                                                if (genericController.vbInstr(1, ResponseFieldValueText, HTMLEditorDefaultCopyStartMark) != 0) {
                                                    //
                                                    // if the default copy was editing, remote the markers
                                                    //
                                                    ResponseFieldValueText = genericController.vbReplace(ResponseFieldValueText, HTMLEditorDefaultCopyStartMark, "");
                                                    ResponseFieldValueText = genericController.vbReplace(ResponseFieldValueText, HTMLEditorDefaultCopyEndMark, "");
                                                    //ResponseValueVariant = ResponseValueText
                                                }
                                                //
                                                // If the response is only white space, remove it
                                                // this is a fix for when Site Managers leave white space in the editor, and do not realize it
                                                //   then cannot fixgure out how to remove it
                                                //
                                                ResponseFieldValueText = activeContentController.processWysiwygResponseForSave(core, ResponseFieldValueText);
                                                if (string.IsNullOrEmpty(ResponseFieldValueText.ToLower().Replace(' '.ToString(), "").Replace("&nbsp;", ""))) {
                                                    ResponseFieldValueText = "";
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        //
                                        // ----- text types
                                        //
                                        EditorRowHeight = core.docProperties.getInteger(field.nameLc + "Rows");
                                        if (EditorRowHeight != 0) {
                                            core.userProperty.setProperty(adminContent.Name + "." + field.nameLc + ".RowHeight", EditorRowHeight);
                                        }
                                        EditorPixelHeight = core.docProperties.getInteger(field.nameLc + "PixelHeight");
                                        if (EditorPixelHeight != 0) {
                                            core.userProperty.setProperty(adminContent.Name + "." + field.nameLc + ".PixelHeight", EditorPixelHeight);
                                        }
                                        break;
                                }
                                if (field.nameLc == "parentid") {
                                    //
                                    // check circular reference on all parentid fields
                                    //

                                    ParentID = genericController.encodeInteger(ResponseFieldValueText);
                                    LoopPtr = 0;
                                    UsedIDs = editRecord.id.ToString();
                                    while ((LoopPtr < LoopPtrMax) && (ParentID != 0) && (("," + UsedIDs + ",").IndexOf("," + ParentID.ToString() + ",") == -1)) {
                                        UsedIDs = UsedIDs + "," + ParentID.ToString();
                                        CS = core.db.csOpen(adminContent.Name, "ID=" + ParentID, "", true, 0, false, false, "ParentID");
                                        if (!core.db.csOk(CS)) {
                                            ParentID = 0;
                                        } else {
                                            ParentID = core.db.csGetInteger(CS, "ParentID");
                                        }
                                        core.db.csClose(ref CS);
                                        LoopPtr = LoopPtr + 1;
                                    }
                                    if (LoopPtr == LoopPtrMax) {
                                        //
                                        // Too deep
                                        //
                                        errorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " creates a relationship between records that Is too large. Please limit the depth of this relationship to " + LoopPtrMax + " records.");
                                        ResponseFieldValueIsOKToSave = false;
                                    } else if ((editRecord.id != 0) && (editRecord.id == ParentID)) {
                                        //
                                        // Reference to iteslf
                                        //
                                        errorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " contains a circular reference. This record points back to itself. This Is Not allowed.");
                                        ResponseFieldValueIsOKToSave = false;
                                    } else if (ParentID != 0) {
                                        //
                                        // Circular reference
                                        //
                                        errorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " contains a circular reference. This field either points to other records which then point back to this record. This Is Not allowed.");
                                        ResponseFieldValueIsOKToSave = false;
                                    }
                                }
                                if (field.textBuffered) {
                                    //
                                    // text buffering
                                    //
                                    //ResponseFieldValueText = genericController.main_RemoveControlCharacters(ResponseFieldValueText);
                                }
                                if ((field.required) && (ResponseFieldIsEmpty)) {
                                    //
                                    // field is required and is not given
                                    //
                                    errorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " Is required but has no value.");
                                    ResponseFieldValueIsOKToSave = false;
                                }
                                //
                                // special case - people records without Allowduplicateusername require username to be unique
                                //
                                if (genericController.vbLCase(adminContent.ContentTableName) == "ccmembers") {
                                    if (genericController.vbLCase(field.nameLc) == "username") {
                                        blockDuplicateUsername = !(core.siteProperties.getBoolean("allowduplicateusername", false));
                                    }
                                    if (genericController.vbLCase(field.nameLc) == "email") {
                                        blockDuplicateEmail = (core.siteProperties.getBoolean("allowemaillogin", false));
                                    }
                                }
                                if ((blockDuplicateUsername || blockDuplicateEmail || field.uniqueName) && (!ResponseFieldIsEmpty)) {
                                    //
                                    // ----- Do the unique check for this field
                                    //
                                    string SQLUnique = "select id from " + adminContent.ContentTableName + " where (" + field.nameLc + "=" + core.db.EncodeSQL(ResponseFieldValueText, field.fieldTypeId) + ")and(" + cdefModel.getContentControlCriteria(core, adminContent.Name) + ")";
                                    if (editRecord.id > 0) {
                                        //
                                        // --editing record
                                        SQLUnique = SQLUnique + "and(id<>" + editRecord.id + ")";
                                    }
                                    CSPointer = core.db.csOpenSql_rev(adminContent.ContentDataSourceName, SQLUnique);
                                    if (core.db.csOk(CSPointer)) {
                                        //
                                        // field is not unique, skip it and flag error
                                        //
                                        if (blockDuplicateUsername) {
                                            //
                                            //
                                            //
                                            errorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be unique And there Is another record with [" + ResponseFieldValueText + "]. This must be unique because the preference Allow Duplicate Usernames Is Not checked.");
                                        } else if (blockDuplicateEmail) {
                                            //
                                            //
                                            //
                                            errorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be unique And there Is another record with [" + ResponseFieldValueText + "]. This must be unique because the preference Allow Email Login Is checked.");
                                        } else if (false) {
                                        } else {
                                            //
                                            // non-workflow
                                            //
                                            errorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be unique And there Is another record with [" + ResponseFieldValueText + "].");
                                        }
                                        ResponseFieldValueIsOKToSave = false;
                                    }
                                    core.db.csClose(ref CSPointer);
                                }
                            }
                            // end case
                            break;
                    }
                    //
                    // Save response if it is valid
                    //
                    if (ResponseFieldValueIsOKToSave) {
                        editRecord.fieldsLc[field.nameLc].value = ResponseFieldValueText;
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //
        //
        //========================================================================
        //   Save Whats New values if present
        //
        //   does NOT check AuthoringLocked -- you must check before calling
        //========================================================================
        //
        private void SaveContentTracking(cdefModel adminContent, editRecordClass editRecord) {
            try {
                //
                int ContentID = 0;
                int CSPointer = 0;
                int CSRules = 0;
                int CSContentWatch = 0;
                int ContentWatchID = 0;
                //
                if (adminContent.AllowContentTracking & (!editRecord.Read_Only)) {
                    //
                    // ----- Set default content watch link label
                    //
                    if ((ContentWatchListIDCount > 0) && (ContentWatchLinkLabel == "")) {
                        if (editRecord.menuHeadline != "") {
                            ContentWatchLinkLabel = editRecord.menuHeadline;
                        } else if (editRecord.nameLc != "") {
                            ContentWatchLinkLabel = editRecord.nameLc;
                        } else {
                            ContentWatchLinkLabel = "Click Here";
                        }
                    }
                    // ----- update/create the content watch record for this content record
                    //
                    ContentID = editRecord.contentControlId;
                    CSContentWatch = core.db.csOpen("Content Watch", "(ContentID=" + core.db.encodeSQLNumber(ContentID) + ")And(RecordID=" + core.db.encodeSQLNumber(editRecord.id) + ")");
                    if (!core.db.csOk(CSContentWatch)) {
                        core.db.csClose(ref CSContentWatch);
                        CSContentWatch = core.db.csInsertRecord("Content Watch");
                        core.db.csSet(CSContentWatch, "contentid", ContentID);
                        core.db.csSet(CSContentWatch, "recordid", editRecord.id);
                        core.db.csSet(CSContentWatch, "ContentRecordKey", ContentID + "." + editRecord.id);
                        core.db.csSet(CSContentWatch, "clicks", 0);
                    }
                    if (!core.db.csOk(CSContentWatch)) {
                        handleLegacyClassError("", "SaveContentTracking, can Not create New record");
                    } else {
                        ContentWatchID = core.db.csGetInteger(CSContentWatch, "ID");
                        core.db.csSet(CSContentWatch, "LinkLabel", ContentWatchLinkLabel);
                        core.db.csSet(CSContentWatch, "WhatsNewDateExpires", ContentWatchExpires);
                        core.db.csSet(CSContentWatch, "Link", ContentWatchLink);
                        //
                        // ----- delete all rules for this ContentWatch record
                        //
                        //Call core.app.DeleteContentRecords("Content Watch List Rules", "(ContentWatchID=" & ContentWatchID & ")")
                        CSPointer = core.db.csOpen("Content Watch List Rules", "(ContentWatchID=" + ContentWatchID + ")");
                        while (core.db.csOk(CSPointer)) {
                            core.db.csDeleteRecord(CSPointer);
                            core.db.csGoNext(CSPointer);
                        }
                        core.db.csClose(ref CSPointer);
                        //
                        // ----- Update ContentWatchListRules for all entries in ContentWatchListID( ContentWatchListIDCount )
                        //
                        int ListPointer = 0;
                        if (ContentWatchListIDCount > 0) {
                            for (ListPointer = 0; ListPointer < ContentWatchListIDCount; ListPointer++) {
                                CSRules = core.db.csInsertRecord("Content Watch List Rules");
                                if (core.db.csOk(CSRules)) {
                                    core.db.csSet(CSRules, "ContentWatchID", ContentWatchID);
                                    core.db.csSet(CSRules, "ContentWatchListID", ContentWatchListID[ListPointer]);
                                }
                                core.db.csClose(ref CSRules);
                            }
                        }
                    }
                    core.db.csClose(ref CSContentWatch);
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        //   Read in Whats New values if present
        //========================================================================
        //
        private void LoadContentTrackingResponse(cdefModel adminContent, editRecordClass editRecord) {
            try {
                //
                int CSContentWatchList = 0;
                int RecordID = 0;
                //
                ContentWatchListIDCount = 0;
                if ((core.docProperties.getText("WhatsNewResponse") != "") & (adminContent.AllowContentTracking)) {
                    //
                    // ----- set single fields
                    //
                    ContentWatchLinkLabel = core.docProperties.getText("ContentWatchLinkLabel");
                    ContentWatchExpires = core.docProperties.getDate("ContentWatchExpires");
                    //
                    // ----- Update ContentWatchListRules for all checked boxes
                    //
                    CSContentWatchList = core.db.csOpen("Content Watch Lists");
                    while (core.db.csOk(CSContentWatchList)) {
                        RecordID = (core.db.csGetInteger(CSContentWatchList, "ID"));
                        if (core.docProperties.getBoolean("ContentWatchList." + RecordID)) {
                            if (ContentWatchListIDCount >= ContentWatchListIDSize) {
                                ContentWatchListIDSize = ContentWatchListIDSize + 50;
                                Array.Resize(ref ContentWatchListID, ContentWatchListIDSize);
                            }
                            ContentWatchListID[ContentWatchListIDCount] = RecordID;
                            ContentWatchListIDCount = ContentWatchListIDCount + 1;
                        }
                        core.db.csGoNext(CSContentWatchList);
                    }
                    core.db.csClose(ref CSContentWatchList);
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        //   Save Link Alias field if it supported, and is non-authoring
        //   if it is authoring, it will be saved by the userfield routines
        //   if not, it appears in the LinkAlias tab, and must be saved here
        //========================================================================
        //
        private void SaveLinkAlias(cdefModel adminContent, editRecordClass editRecord) {
            try {
                //
                // --use field ptr to test if the field is supported yet
                if (core.siteProperties.allowLinkAlias) {
                    bool isDupError = false;
                    string linkAlias = core.docProperties.getText("linkalias");
                    bool OverRideDuplicate = core.docProperties.getBoolean("OverRideDuplicate");
                    bool DupCausesWarning = false;
                    if (string.IsNullOrEmpty(linkAlias)) {
                        //
                        // Link Alias is blank, use the record name
                        //
                        linkAlias = editRecord.nameLc;
                        DupCausesWarning = true;
                    }
                    if (!string.IsNullOrEmpty(linkAlias)) {
                        if (OverRideDuplicate) {
                            core.db.executeQuery("update " + adminContent.ContentTableName + " set linkalias=null where ( linkalias=" + core.db.encodeSQLText(linkAlias) + ") and (id<>" + editRecord.id + ")");
                        } else {
                            int CS = core.db.csOpen(adminContent.Name, "( linkalias=" + core.db.encodeSQLText(linkAlias) + ")and(id<>" + editRecord.id + ")");
                            if (core.db.csOk(CS)) {
                                isDupError = true;
                                errorController.addUserError(core, "The Link Alias you entered can not be used because another record uses this value [" + linkAlias + "]. Enter a different Link Alias, or check the Override Duplicates checkbox in the Link Alias tab.");
                            }
                            core.db.csClose(ref CS);
                        }
                        if (!isDupError) {
                            DupCausesWarning = true;
                            int CS = core.db.cs_open2(adminContent.Name, editRecord.id, true, true);
                            if (core.db.csOk(CS)) {
                                core.db.csSet(CS, "linkalias", linkAlias);
                            }
                            core.db.csClose(ref CS);
                            //
                            // Update the Link Aliases
                            //
                            linkAliasController.addLinkAlias(core, linkAlias, editRecord.id, "", OverRideDuplicate, DupCausesWarning);
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        //   Read in Whats New values if present
        //   Field values must be loaded
        //========================================================================
        //
        private void LoadContentTrackingDataBase(cdefModel adminContent, editRecordClass editRecord) {
            try {
                //
                int ContentID = 0;
                int CSPointer = 0;
                // converted array to dictionary - Dim FieldPointer As Integer
                //
                // ----- check if admin record is present
                //
                if ((editRecord.id != 0) & (adminContent.AllowContentTracking)) {
                    //
                    // ----- Open the content watch record for this content record
                    //
                    ContentID = editRecord.contentControlId;
                    CSPointer = core.db.csOpen("Content Watch", "(ContentID=" + core.db.encodeSQLNumber(ContentID) + ")AND(RecordID=" + core.db.encodeSQLNumber(editRecord.id) + ")");
                    if (core.db.csOk(CSPointer)) {
                        ContentWatchLoaded = true;
                        ContentWatchRecordID = (core.db.csGetInteger(CSPointer, "ID"));
                        ContentWatchLink = (core.db.csGet(CSPointer, "Link"));
                        ContentWatchClicks = (core.db.csGetInteger(CSPointer, "Clicks"));
                        ContentWatchLinkLabel = (core.db.csGet(CSPointer, "LinkLabel"));
                        ContentWatchExpires = (core.db.csGetDate(CSPointer, "WhatsNewDateExpires"));
                        core.db.csClose(ref CSPointer);
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        //
        private void SaveEditRecord(cdefModel adminContent, editRecordClass editRecord) {
            try {
                int SaveCCIDValue = 0;
                int ActivityLogOrganizationID = -1;
                if (core.doc.debug_iUserError != "") {
                    //
                    // -- If There is an error, block the save
                    AdminAction = AdminActionNop;
                } else if (!core.doc.sessionContext.isAuthenticatedContentManager(core, adminContent.Name)) {
                    //
                    // -- must be content manager
                } else if (editRecord.Read_Only) {
                    //
                    // -- read only block
                } else {
                    //
                    // -- Record will be saved, create a new one if this is an add
                    bool NewRecord = false;
                    bool RecordChanged = false;
                    int CSEditRecord = -1;
                    if (editRecord.id == 0) {
                        NewRecord = true;
                        RecordChanged = true;
                        CSEditRecord = core.db.csInsertRecord(adminContent.Name);
                    } else {
                        NewRecord = false;
                        CSEditRecord = core.db.cs_open2(adminContent.Name, editRecord.id, true, true);
                    }
                    if (!core.db.csOk(CSEditRecord)) {
                        //
                        // ----- Error: new record could not be created
                        //
                        if (NewRecord) {
                            //
                            // Could not insert record
                            //
                            core.handleException(new ApplicationException("A new record could not be inserted for content [" + adminContent.Name + "]. Verify the Database table and field DateAdded, CreateKey, and ID."));
                        } else {
                            //
                            // Could not locate record you requested
                            //
                            core.handleException(new ApplicationException("The record you requested (ID=" + editRecord.id + ") could not be found for content [" + adminContent.Name + "]"));
                        }
                    } else {
                        //
                        // ----- Get the ID of the current record
                        //
                        editRecord.id = core.db.csGetInteger(CSEditRecord, "ID");
                        //
                        // ----- Create the update sql
                        //
                        bool FieldChanged = false;
                        foreach (var keyValuePair in adminContent.fields) {
                            cdefFieldModel field = keyValuePair.Value;
                            editRecordFieldClass editRecordField = editRecord.fieldsLc[field.nameLc];
                            object fieldValueObject = editRecordField.value;
                            string FieldValueText = genericController.encodeText(fieldValueObject);
                            string FieldName = field.nameLc;
                            string UcaseFieldName = genericController.vbUCase(FieldName);
                            //
                            // ----- Handle special case fields
                            //
                            switch (UcaseFieldName) {
                                case "NAME": {
                                        //
                                        editRecord.nameLc = genericController.encodeText(fieldValueObject);
                                        break;
                                    }
                                case "CCGUID": {
                                        string saveValue = genericController.encodeText(fieldValueObject);
                                        if (core.db.csGetText(CSEditRecord, FieldName) != saveValue) {
                                            FieldChanged = true;
                                            RecordChanged = true;
                                            core.db.csSet(CSEditRecord, FieldName, saveValue);
                                        }
                                        //RecordChanged = True
                                        //Call core.app.SetCS(CSEditRecord, FieldName, FieldValueVariant)
                                        break;
                                    }
                                case "CONTENTCONTROLID": {
                                        //
                                        // run this after the save, so it will be blocked if the save fails
                                        // block the change from this save
                                        // Update the content control ID here, for all the children, and all the edit and archive records of both
                                        //
                                        int saveValue = genericController.encodeInteger(fieldValueObject);
                                        if (editRecord.contentControlId != saveValue) {
                                            SaveCCIDValue = saveValue;
                                            RecordChanged = true;
                                        }
                                        break;
                                    }
                                case "ACTIVE": {
                                        bool saveValue = genericController.encodeBoolean(fieldValueObject);
                                        if (core.db.csGetBoolean(CSEditRecord, FieldName) != saveValue) {
                                            FieldChanged = true;
                                            RecordChanged = true;
                                            core.db.csSet(CSEditRecord, FieldName, saveValue);
                                        }
                                        break;
                                    }
                                case "DATEEXPIRES": {
                                        //
                                        // ----- make sure content watch expires before content expires
                                        //
                                        if (!genericController.IsNull(fieldValueObject)) {
                                            if (dateController.IsDate(fieldValueObject)) {
                                                DateTime saveValue = genericController.encodeDate(fieldValueObject);
                                                if (ContentWatchExpires <= DateTime.MinValue) {
                                                    ContentWatchExpires = saveValue;
                                                } else if (ContentWatchExpires > saveValue) {
                                                    ContentWatchExpires = saveValue;
                                                }
                                            }
                                        }
                                        //
                                        break;
                                    }
                                case "DATEARCHIVE": {
                                        //
                                        // ----- make sure content watch expires before content archives
                                        //
                                        if (!genericController.IsNull(fieldValueObject)) {
                                            if (dateController.IsDate(fieldValueObject)) {
                                                DateTime saveValue = genericController.encodeDate(fieldValueObject);
                                                if ((ContentWatchExpires) <= DateTime.MinValue) {
                                                    ContentWatchExpires = saveValue;
                                                } else if (ContentWatchExpires > saveValue) {
                                                    ContentWatchExpires = saveValue;
                                                }
                                            }
                                        }
                                        break;
                                    }
                            }
                            //
                            // ----- Put the field in the SQL to be saved
                            //
                            if (IsVisibleUserField(field.adminOnly, field.developerOnly, field.active, field.authorable, field.nameLc, adminContent.ContentTableName) & (NewRecord || (!field.readOnly)) & (NewRecord || (!field.notEditable))) {
                                //
                                // ----- save the value by field type
                                //
                                switch (field.fieldTypeId) {
                                    case FieldTypeIdAutoIdIncrement:
                                    case FieldTypeIdRedirect: {
                                            //
                                            // do nothing with these
                                            //
                                            break;
                                        }
                                    case FieldTypeIdFile:
                                    case FieldTypeIdFileImage: {
                                            //
                                            // filenames, upload to cdnFiles
                                            //
                                            if (core.docProperties.getBoolean(FieldName + ".DeleteFlag")) {
                                                RecordChanged = true;
                                                FieldChanged = true;
                                                core.db.csSet(CSEditRecord, FieldName, "");
                                            }
                                            FieldValueText = genericController.encodeText(fieldValueObject);
                                            if (!string.IsNullOrEmpty(FieldValueText)) {
                                                string Filename = encodeFilename(FieldValueText);
                                                string unixPathFilename = core.db.csGetFieldFilename(CSEditRecord, FieldName, Filename, adminContent.Name);
                                                string dosPathFilename = genericController.convertToDosSlash(unixPathFilename);
                                                string dosPath = genericController.getPath(dosPathFilename);
                                                core.cdnFiles.upload(FieldName, dosPath, ref Filename);
                                                core.db.csSet(CSEditRecord, FieldName, unixPathFilename);
                                                RecordChanged = true;
                                                FieldChanged = true;
                                            }
                                            break;
                                        }
                                    case FieldTypeIdBoolean: {
                                            //
                                            // boolean
                                            //
                                            bool saveValue = genericController.encodeBoolean(fieldValueObject);
                                            if (core.db.csGetBoolean(CSEditRecord, FieldName) != saveValue) {
                                                RecordChanged = true;
                                                FieldChanged = true;
                                                core.db.csSet(CSEditRecord, FieldName, saveValue);
                                            }
                                            break;
                                        }
                                    case FieldTypeIdCurrency:
                                    case FieldTypeIdFloat: {
                                            //
                                            // Floating pointer numbers
                                            //
                                            double saveValue = genericController.encodeNumber(fieldValueObject);
                                            if (core.db.csGetNumber(CSEditRecord, FieldName) != saveValue) {
                                                RecordChanged = true;
                                                FieldChanged = true;
                                                core.db.csSet(CSEditRecord, FieldName, saveValue);
                                            }
                                            break;
                                        }
                                    case FieldTypeIdDate: {
                                            //
                                            // Date
                                            //
                                            DateTime saveValue = genericController.encodeDate(fieldValueObject);
                                            if (core.db.csGetDate(CSEditRecord, FieldName) != saveValue) {
                                                FieldChanged = true;
                                                RecordChanged = true;
                                                core.db.csSet(CSEditRecord, FieldName, saveValue);
                                            }
                                            break;
                                        }
                                    case FieldTypeIdInteger:
                                    case FieldTypeIdLookup: {
                                            //
                                            // Integers
                                            //
                                            int saveValue = genericController.encodeInteger(fieldValueObject);
                                            if (saveValue != core.db.csGetInteger(CSEditRecord, FieldName)) {
                                                FieldChanged = true;
                                                RecordChanged = true;
                                                core.db.csSet(CSEditRecord, FieldName, saveValue);
                                            }
                                            break;
                                        }
                                    case FieldTypeIdLongText:
                                    case FieldTypeIdText:
                                    case FieldTypeIdFileText:
                                    case FieldTypeIdFileCSS:
                                    case FieldTypeIdFileXML:
                                    case FieldTypeIdFileJavascript:
                                    case FieldTypeIdHTML:
                                    case FieldTypeIdFileHTML: {
                                            //
                                            // Text
                                            //
                                            string saveValue = genericController.encodeText(fieldValueObject);
                                            if (core.db.csGet(CSEditRecord, FieldName) != saveValue) {
                                                FieldChanged = true;
                                                RecordChanged = true;
                                                core.db.csSet(CSEditRecord, FieldName, saveValue);
                                            }
                                            break;
                                        }
                                    case FieldTypeIdManyToMany: {
                                            //
                                            // Many to Many checklist
                                            //
                                            //MTMContent0 = cdefmodel.getContentNameByID(core,.contentId)
                                            //MTMContent1 = cdefmodel.getContentNameByID(core,.manyToManyContentID)
                                            //MTMRuleContent = cdefmodel.getContentNameByID(core,.manyToManyRuleContentID)
                                            //MTMRuleField0 = .ManyToManyRulePrimaryField
                                            //MTMRuleField1 = .ManyToManyRuleSecondaryField
                                            core.html.processCheckList("ManyToMany" + field.id, cdefModel.getContentNameByID(core, field.contentId), encodeText(editRecord.id), cdefModel.getContentNameByID(core, field.manyToManyContentID), cdefModel.getContentNameByID(core, field.manyToManyRuleContentID), field.ManyToManyRulePrimaryField, field.ManyToManyRuleSecondaryField);
                                            break;
                                        }
                                    default: {
                                            //
                                            // Unknown other types
                                            //

                                            string saveValue = genericController.encodeText(fieldValueObject);
                                            FieldChanged = true;
                                            RecordChanged = true;
                                            core.db.csSet(CSEditRecord, UcaseFieldName, saveValue);
                                            //sql &=  "," & .Name & "=" & core.app.EncodeSQL(FieldValueVariant, .FieldType)
                                            break;
                                        }
                                }
                            }
                            //
                            // -- put any changes back in array for the next page to display
                            editRecordField.value = fieldValueObject;
                            //
                            // -- Log Activity for changes to people and organizattions
                            if (FieldChanged) {
                                switch (genericController.vbLCase(adminContent.ContentTableName)) {
                                    case "ccmembers":
                                        //
                                        if (ActivityLogOrganizationID < 0) {
                                            personModel person = personModel.create(core, editRecord.id);
                                            if (person != null) {
                                                ActivityLogOrganizationID = person.OrganizationID;
                                            }
                                        }
                                        logController.logActivity2(core, "modifying field " + FieldName, editRecord.id, ActivityLogOrganizationID);
                                        break;
                                    case "organizations":
                                        //
                                        logController.logActivity2(core, "modifying field " + FieldName, 0, editRecord.id);
                                        break;
                                    case "cclibraryfiles":
                                        //
                                        if (core.docProperties.getText("filename") != "") {
                                            core.db.csSet(CSEditRecord, "altsizelist", "");
                                        }
                                        break;
                                }
                            }
                        }
                        //
                        core.db.csClose(ref CSEditRecord);
                        if (RecordChanged) {
                            //
                            // -- clear cache
                            string tableName = "";
                            if (editRecord.contentControlId == 0) {
                                tableName = cdefModel.getContentTablename(core, adminContent.Name);
                            } else {
                                tableName = cdefModel.getContentTablename(core, editRecord.contentControlId_Name);
                            }
                            //todo  NOTE: The following VB 'Select Case' included either a non-ordinal switch expression or non-ordinal, range-type, or non-constant 'Case' expressions and was converted to C# 'if-else' logic:
                            //							Select Case tableName.ToLower()
                            var tempVar = tableName.ToLower();
                            //ORIGINAL LINE: Case linkAliasModel.contentTableName.ToLower()
                            if (tempVar == linkAliasModel.contentTableName.ToLower()) {
                                //
                                linkAliasModel.invalidateCache(core, editRecord.id);
                                //Models.Complex.routeDictionaryModel.invalidateCache(core)
                            }
                            //ORIGINAL LINE: Case addonModel.contentTableName.ToLower()
                            else if (tempVar == addonModel.contentTableName.ToLower()) {
                                //
                                addonModel.invalidateCache(core, editRecord.id);
                                //Models.Complex.routeDictionaryModel.invalidateCache(core)
                            }
                            //ORIGINAL LINE: Case Else
                            else {
                                linkAliasModel.invalidateCache(core, editRecord.id);
                            }

                        }
                        //
                        // ----- Clear/Set PageNotFound
                        //
                        if (editRecord.SetPageNotFoundPageID) {
                            core.siteProperties.setProperty("PageNotFoundPageID", genericController.encodeText(editRecord.id));
                        }
                        //
                        // ----- Clear/Set LandingPageID
                        //
                        if (editRecord.SetLandingPageID) {
                            core.siteProperties.setProperty("LandingPageID", genericController.encodeText(editRecord.id));
                        }
                        //
                        // ----- clear/set authoring controls
                        //
                        core.workflow.ClearEditLock(adminContent.Name, editRecord.id);
                        //
                        // ----- if admin content is changed, reload the admincontent data in case this is a save, and not an OK
                        //
                        if (RecordChanged && SaveCCIDValue != 0) {
                            cdefModel.setContentControlId(core, editRecord.contentControlId, editRecord.id, SaveCCIDValue);
                            editRecord.contentControlId_Name = cdefModel.getContentNameByID(core, SaveCCIDValue);
                            adminContent = cdefModel.getCdef(core, editRecord.contentControlId_Name);
                            adminContent.Id = adminContent.Id;
                            adminContent.Name = adminContent.Name;
                            // false = core.siteProperties.allowWorkflowAuthoring And adminContent.AllowWorkflowAuthoring
                        }
                    }
                    editRecord.Saved = true;
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        // Get Just the tablename from a sql statement
        //   This is to be compatible with the old way of setting up FieldTypeLookup
        //========================================================================
        //
        private string GetJustTableName(string SQL) {
            string tempGetJustTableName = null;
            try {
                //
                tempGetJustTableName = SQL.ToUpper().Trim(' ');
                while ((!string.IsNullOrEmpty(tempGetJustTableName)) & (tempGetJustTableName.IndexOf(" ") != -1)) {
                    tempGetJustTableName = tempGetJustTableName.Substring(genericController.vbInstr(tempGetJustTableName, " "));
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetJustTableName;
        }
        //
        //========================================================================
        // Test Content Access -- return based on Admin/Developer/MemberRules
        //   if developer, let all through
        //   if admin, block if table is developeronly
        //   if member, run blocking query (which also traps adminonly and developer only)
        //       if blockin query has a null RecordID, this member gets everything
        //       if not null recordid in blocking query, use RecordIDs in result for Where clause on this lookup
        //========================================================================
        //
        private bool userHasContentAccess(int ContentID) {
            bool returnHas = false;
            try {
                string ContentName;
                //
                ContentName = cdefModel.getContentNameByID(core, ContentID);
                if (!string.IsNullOrEmpty(ContentName)) {
                    returnHas = core.doc.sessionContext.isAuthenticatedContentManager(core, ContentName);
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnHas;
        }
        //
        //========================================================================
        //   Display a field in the admin index form
        //========================================================================
        //
        private string GetForm_Index_GetCell(cdefModel adminContent, editRecordClass editRecord, string fieldName, int CS, bool IsLookupFieldValid, bool IsEmailContent) {
            string return_formIndexCell = "";
            try {
                string FieldText = null;
                string Filename = null;
                string Copy = null;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string[] lookups = null;
                int LookupPtr = 0;
                int Pos = 0;
                int lookupTableCnt = 0;
                //
                var tempVar = adminContent.fields[fieldName.ToLower()];
                lookupTableCnt = tempVar.id; // workaround for universally creating the left join tablename for each field
                switch (tempVar.fieldTypeId) {
                    //Case FieldTypeImage
                    //    Stream.Add( Mid(core.app.cs_get(CS, .Name), 7 + Len(.Name) + Len(AdminContent.ContentTableName)))
                    case FieldTypeIdFile:
                    case FieldTypeIdFileImage:
                        Filename = core.db.csGet(CS, tempVar.nameLc);
                        Filename = genericController.vbReplace(Filename, "\\", "/");
                        Pos = Filename.LastIndexOf("/") + 1;
                        if (Pos != 0) {
                            Filename = Filename.Substring(Pos);
                        }
                        Stream.Add(Filename);
                        break;
                    case FieldTypeIdLookup:
                        if (IsLookupFieldValid) {
                            Stream.Add(core.db.csGet(CS, "LookupTable" + lookupTableCnt + "Name"));
                            lookupTableCnt += 1;
                        } else if (tempVar.lookupList != "") {
                            lookups = tempVar.lookupList.Split(',');
                            LookupPtr = core.db.csGetInteger(CS, tempVar.nameLc) - 1;
                            if (LookupPtr <= lookups.GetUpperBound(0)) {
                                if (LookupPtr < 0) {
                                    //Stream.Add( "-1")
                                } else {
                                    Stream.Add(lookups[LookupPtr]);
                                }
                            } else {
                                //Stream.Add( "-2")
                            }

                        } else {
                            //Stream.Add( "-3")
                            Stream.Add(" ");
                        }
                        break;
                    case FieldTypeIdMemberSelect:
                        if (IsLookupFieldValid) {
                            Stream.Add(core.db.csGet(CS, "LookupTable" + lookupTableCnt + "Name"));
                            lookupTableCnt += 1;
                        } else {
                            Stream.Add(core.db.csGet(CS, tempVar.nameLc));
                        }
                        break;
                    case FieldTypeIdBoolean:
                        if (core.db.csGetBoolean(CS, tempVar.nameLc)) {
                            Stream.Add("yes");
                        } else {
                            Stream.Add("no");
                        }
                        break;
                    case FieldTypeIdCurrency:
                        Stream.Add(string.Format("{0:C}", core.db.csGetNumber(CS, tempVar.nameLc)));
                        break;
                    case FieldTypeIdLongText:
                    case FieldTypeIdHTML:
                        FieldText = core.db.csGet(CS, tempVar.nameLc);
                        if (FieldText.Length > 50) {
                            FieldText = FieldText.Left(50) + "[more]";
                        }
                        Stream.Add(FieldText);
                        break;
                    case FieldTypeIdFileText:
                    case FieldTypeIdFileCSS:
                    case FieldTypeIdFileXML:
                    case FieldTypeIdFileJavascript:
                    case FieldTypeIdFileHTML:
                        // rw( "n/a" )
                        Filename = core.db.csGet(CS, tempVar.nameLc);
                        if (!string.IsNullOrEmpty(Filename)) {
                            Copy = core.cdnFiles.readFile(Filename);
                            // 20171103 - dont see why this is being converted, not html
                            //Copy = core.html.convertActiveContent_internal(Copy, core.doc.authContext.user.id, "", 0, 0, True, False, False, False, True, False, "", "", IsEmailContent, 0, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, core.doc.authContext.isAuthenticated, Nothing, core.doc.authContext.isEditingAnything())
                            Stream.Add(Copy);
                        }
                        break;
                    case FieldTypeIdRedirect:
                    case FieldTypeIdManyToMany:
                        Stream.Add("n/a");
                        break;
                    default:
                        if (tempVar.password) {
                            Stream.Add("****");
                        } else {
                            Stream.Add(core.db.csGet(CS, tempVar.nameLc));
                        }
                        break;
                }
                //
                return_formIndexCell = genericController.encodeHTML(Stream.Text);
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return return_formIndexCell;
        }
        //
        //========================================================================
        // Get the Normal Edit Button Bar String
        //
        //   used on Normal Edit and others
        //========================================================================
        //
        private string GetForm_Edit_ButtonBar(cdefModel adminContent, editRecordClass editRecord, bool AllowDelete, bool allowSave, bool AllowAdd, bool AllowRefresh = false) {
            string result = "";
            try {
                //
                adminUIController Adminui = new adminUIController(core);
                bool IncludeCDefReload = false;
                bool IsPageContent = false;
                bool HasChildRecords = false;
                int CS = 0;
                bool AllowMarkReviewed = false;
                //
                IsPageContent = cdefModel.isWithinContent(core, adminContent.Id, cdefModel.getContentId(core, "Page Content"));
                if (cdefModel.isContentFieldSupported(core, adminContent.Name, "parentid")) {
                    CS = core.db.csOpen(adminContent.Name, "parentid=" + editRecord.id, "", true, 0, false, false, "ID");
                    HasChildRecords = core.db.csOk(CS);
                    core.db.csClose(ref CS);
                }
                IncludeCDefReload = (adminContent.ContentTableName.ToLower() == "cccontent") || (adminContent.ContentTableName.ToLower() == "ccdatasources") || (adminContent.ContentTableName.ToLower() == "cctables") || (adminContent.ContentTableName.ToLower() == "ccfields");
                AllowMarkReviewed = core.db.isSQLTableField("default", adminContent.ContentTableName, "DateReviewed");
                //
                return Adminui.GetEditButtonBar2(MenuDepth, AllowDelete && editRecord.AllowDelete, editRecord.AllowCancel, (allowSave && editRecord.AllowSave), (SpellCheckSupported & (!SpellCheckRequest)), editRecord.AllowPublish, editRecord.AllowAbort, editRecord.AllowSubmit, editRecord.AllowApprove, (AllowAdd && adminContent.AllowAdd & editRecord.AllowInsert), IncludeCDefReload, HasChildRecords, IsPageContent, AllowMarkReviewed, AllowRefresh, (allowSave && editRecord.AllowSave & (editRecord.id != 0)));
                //GetForm_Edit_ButtonBar = AdminUI.GetEditButtonBar2( MenuDepth, iAllowDelete And EditRecord.AllowDelete, EditRecord.AllowCancel, (iAllowSave And EditRecord.AllowSave), (SpellCheckSupported And (Not SpellCheckRequest)), EditRecord.AllowPublish, EditRecord.AllowAbort, EditRecord.AllowSubmit, EditRecord.AllowApprove, (AdminContent.allowAdd And EditRecord.AllowInsert), IncludeCDefReload, HasChildRecords, IsPageContent, AllowMarkReviewed)
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        // ----- Print the edit form
        //
        //   Prints the correct form based on the current AdminContent.contenttablename
        //   AdminContent.type is not longer used
        //========================================================================
        //
        private string GetForm_Edit(cdefModel adminContent, editRecordClass editRecord) {
            string returnHtml = "";
            try {
                csv_contentTypeEnum ContentType = csv_contentTypeEnum.contentTypeWeb;
                string editorAddonListJSON = null;
                bool Active = false;
                string Name = null;
                string styleList = null;
                string styleOptionList = "";
                string fieldEditorList = null;
                string[] fieldTypeDefaultEditors = null;
                string fieldEditorPreferencesList = null;
                DataTable dt = null;
                string[,] Cells = null;
                int fieldId = 0;
                int TableID = 0;
                DateTime LastSendTestDate = default(DateTime);
                bool AllowEmailSendWithoutTest = false;
                Dictionary<string, int> fieldEditorOptions = new Dictionary<string, int>();
                int Ptr = 0;
                int fieldEditorOptionCnt = 0;
                string SQL = null;
                bool IsTemplateTable = false;
                int TemplateIDForStyles = 0;
                int emailIdForStyles = 0;
                // Dim RootPageSectionID As Integer
                bool AllowajaxTabs = false;
                collectionXmlController XMLTools = new collectionXmlController(core);
                bool IsPageContentTable = false;
                bool IsSectionTable = false;
                bool IsEmailTable = false;
                bool IsLandingPageTemp = false;
                int Pos = 0;
                bool IsLandingPageParent = false;
                bool IsLandingSection = false;
                string CreatedCopy = null;
                string ModifiedCopy = null;
                int CS = 0;
                string EditReferer = null;
                string CustomDescription = null;
                string EditSectionButtonBar = null;
                bool EmailSent = false;
                bool EmailSubmitted = false;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                int SystemEmailCID = 0;
                int ConditionalEmailCID = 0;
                string HeaderDescription = null;
                adminUIController Adminui = new adminUIController(core);
                bool IsLandingPage = false;
                bool IsRootPage = false;
                string CreatedBy = null;
                bool allowCMEdit = false;
                bool allowCMAdd = false;
                bool allowCMDelete = false;
                bool AllowAdd = false;
                bool AllowDelete = false;
                bool allowSave = false;
                //
                CustomDescription = "";
                AllowajaxTabs = (core.siteProperties.getBoolean("AllowAjaxEditTabBeta", false));
                //
                if ((core.doc.debug_iUserError != "") & editRecord.Loaded) {
                    //
                    // block load if there was a user error and it is already loaded (assume error was from response )
                    //
                } else if (adminContent.Id <= 0) {
                    //
                    // Invalid Content
                    //
                    errorController.addUserError(core, "There was a problem identifying the content you requested. Please return to the previous form and verify your selection.");
                    return "";
                } else if (editRecord.Loaded & !editRecord.Saved) {
                    //
                    //   File types need to be reloaded from the Db, because...
                    //       LoadDb - sets them to the path-page
                    //       LoadResponse - sets the blank if no change, filename if there is an upload
                    //       SaveEditRecord - if blank, no change. If a filename it saves the uploaded file
                    //       GetForm_Edit - expects the Db value to be in EditRecordValueVariants (path-page)
                    //
                    // xx This was added to bypass the load for the editrefresh case (reload the response so the editor preference can change)
                    // xx  I do not know why the following section says "reload even if it is loaded", but lets try this
                    //
                    foreach (var keyValuePair in adminContent.fields) {
                        cdefFieldModel field = keyValuePair.Value;
                        switch (field.fieldTypeId) {
                            case FieldTypeIdFile:
                            case FieldTypeIdFileImage:
                                editRecord.fieldsLc[field.nameLc].value = editRecord.fieldsLc[field.nameLc].dbValue;
                                break;
                        }
                    }
                    //For Ptr = 0 To adminContent.fields.Count - 1
                    //    fieldType = arrayOfFields[Ptr].fieldType
                    //    Select Case fieldType
                    //        Case FieldTypeFile, FieldTypeImage
                    //            EditRecordValuesObject[Ptr] = EditRecordDbValues[Ptr]
                    //    End Select
                    //Next
                } else {
                    //
                    // otherwise, load the record, even if it was loaded during a previous form process
                    //
                    LoadEditRecord(adminContent, editRecord, true);
                    if (editRecord.contentControlId == 0) {
                        if (core.doc.debug_iUserError != "") {
                            //
                            // known user error, just return
                            //
                        } else {
                            //
                            // unknown error, set userError and return
                            //
                            errorController.addUserError(core, "There was an unknown error in your request for data. Please let the site administrator know.");
                        }
                        return "";
                    }
                }
                //
                // Test if this editors has access to this record
                //
                if (!userHasContentAccess(editRecord.contentControlId)) {
                    errorController.addUserError(core, "Your account on this system does not have access rights to edit this content.");
                    return "";
                }
                //if (false) {
                //    //
                //    // Test for 100Mb available in Content Files drive
                //    //
                //    if (core.appRootFiles.getDriveFreeSpace() < 1.0E+8F) {
                //        errorController.addUserError(core, "The server drive holding data for this site may not have enough free space to complete this edit operation. If you attempt to save, your data may be lost. Please contact the site administrator.");
                //    }
                //    if (core.privateFiles.getDriveFreeSpace() < 1.0E+8F) {
                //        errorController.addUserError(core, "The server drive holding data for this site may not have enough free space to complete this edit operation. If you attempt to save, your data may be lost. Please contact the site administrator.");
                //    }
                //}
                //
                // Setup Edit Referer
                //
                EditReferer = core.docProperties.getText(RequestNameEditReferer);
                if (string.IsNullOrEmpty(EditReferer)) {
                    EditReferer = core.webServer.requestReferer;
                    if (!string.IsNullOrEmpty(EditReferer)) {
                        //
                        // special case - if you are coming from the advanced search, go back to the list page
                        //
                        EditReferer = genericController.vbReplace(EditReferer, "&af=39", "");
                        //
                        // if referer includes AdminWarningMsg (admin hint message), remove it -- this edit may fix the problem
                        //
                        Pos = EditReferer.IndexOf("AdminWarningMsg=");
                        if (Pos >= 0) {
                            EditReferer = EditReferer.Left(Pos - 2);
                        }
                    }
                }
                core.doc.addRefreshQueryString(RequestNameEditReferer, EditReferer);
                //
                // Print common form elements
                //
                Stream.Add(GetForm_EditFormStart(AdminFormEdit));
                //
                IsLandingPageParent = false;
                //LandingPageID = 0
                IsLandingPage = false;
                IsRootPage = false;
                TemplateIDForStyles = 0;
                IsTemplateTable = (adminContent.ContentTableName.ToLower() == "cctemplates");
                IsPageContentTable = (adminContent.ContentTableName.ToLower() == "ccpagecontent");
                IsSectionTable = (adminContent.ContentTableName.ToLower() == "ccsections");
                IsEmailTable = (adminContent.ContentTableName.ToLower() == "ccemail");
                //
                if (IsEmailTable) {
                    //
                    // ----- special case - email
                    //
                    emailIdForStyles = editRecord.id;
                }
                //
                if (IsPageContentTable) {
                    //
                    // ----- special case - page content
                    //
                    if (editRecord.id != 0) {
                        //
                        // landing page case
                        //
                        //$$$$$ problem -- could be landing page based on domain, not property
                        //LandingPageID = (core.siteProperties.getInteger("LandingPageID", 0))
                        if (core.siteProperties.landingPageID == 0) {
                            //
                            // The call generated a user error because the landingpageid could not be determined
                            // block it
                            //
                            //Call core.main_GetUserError
                        } else {
                            IsLandingPage = (editRecord.id == core.siteProperties.landingPageID);
                            //If IsLandingPage Then
                            //    If genericController.EncodeInteger(core.main_GetSiteProperty2("LandingPageID", "", True)) <> LandingPageID Then
                            //        IsLandingPageTemp = True
                            //    End If
                            //End If
                            IsRootPage = IsPageContentTable && (editRecord.parentID == 0);
                            //If IsRootPage Then
                            //    '$$$$$ cache
                            //    CS = core.db.cs_open("Site Sections", "RootPageID=" & editRecord.id, , , , , , "ID")
                            //    IsRootPage = core.db.cs_ok(CS)
                            //    If IsRootPage Then
                            //        RootPageSectionID = core.db.cs_getInteger(CS, "ID")
                            //    End If
                            //    Call core.db.cs_Close(CS)
                            //End If
                        }
                    }
                }
                //
                if ((!IsLandingPage) && (IsPageContentTable || IsSectionTable)) {
                    //
                    // ----- special case, Is this page a LandingPageParent (Parent of the landing page), or is this section the landing page section
                    //
                    //TestPageID = core.siteProperties.landingPageID
                    //Do While LoopPtr < 20 And (TestPageID <> 0)
                    //    IsLandingPageParent = IsPageContentTable And (editRecord.id = TestPageID)
                    //    IsLandingSection = IsSectionTable And (EditRecordRootPageID = TestPageID)
                    //    If IsLandingPageParent Or IsLandingSection Then
                    //        Exit Do
                    //    End If
                    //    PCCPtr = core.pages.cache_pageContent_getPtr(TestPageID, False, False)
                    //    If PCCPtr >= 0 Then
                    //        TestPageID = genericController.EncodeInteger(PCC(PCC_ParentID, PCCPtr))
                    //    End If
                    //    LoopPtr = LoopPtr + 1
                    //Loop
                }
                //
                // ----- special case messages
                //
                if (IsLandingSection) {
                    CustomDescription = "<div>This is the default Landing Section for this website. This section is displayed when no specific page is requested. It should not be deleted, renamed, marked inactive, blocked or hidden.</div>";
                } else if (IsLandingPageTemp) {
                    CustomDescription = "<div>This page is being used as the default Landing Page for this website, although it has not been set. This may be because a landing page has not been created, or it has been deleted. To make this page the permantent landing page, check the appropriate box in the control tab.</div>";
                } else if (IsLandingPage) {
                    CustomDescription = "<div>This is the default Landing Page for this website. It should not be deleted. You can not mark this record inactive, or use the Publish Date, Expire Date or Archive Date features.</div>";
                } else if (IsLandingPageParent) {
                    CustomDescription = "<div>This page is a parent of the default Landing Page for this website. It should not be deleted. You can not mark this record inactive, or use the Publish Date, Expire Date or Archive Date features.</div>";
                } else if (IsRootPage) {
                    CustomDescription = "<div>This page is a Root Page. A Root Page is the primary page of a section. If you delete or inactivate this page, the section will create a new blank page in its place.</div>";
                }
                //
                // ----- Determine TemplateIDForStyles
                //
                if (IsTemplateTable) {
                    TemplateIDForStyles = editRecord.id;
                } else if (IsPageContentTable) {
                    //Call core.pages.getPageArgs(editRecord.id, false, False, ignoreInteger, TemplateIDForStyles, ignoreInteger, IgnoreString, IgnoreBoolean, ignoreInteger, IgnoreBoolean, "")
                }
                //
                // ----- create page headers
                //
                if (editRecord.id == 0) {
                    HeaderDescription = "<div>New Record</div>";
                } else {
                    HeaderDescription = ""
                    + "<table border=0 cellpadding=0 cellspacing=0 style=\"width:90%\">";
                    if (!string.IsNullOrEmpty(CustomDescription)) {
                        HeaderDescription = HeaderDescription + "<tr><td colspan=2>" + CustomDescription + "<div>&nbsp;</div></td></tr>";
                    }
                    HeaderDescription = HeaderDescription + "<tr><td width=\"50%\">"
                    + "Name: " + editRecord.nameLc + "<br>Record ID: " + editRecord.id + "</td><td width=\"50%\">";
                    //
                    CreatedCopy = "";
                    DateTime editRecordDateAdded = genericController.encodeDate(editRecord.fieldsLc["dateadded"].value);
                    if (editRecord.dateAdded != DateTime.MinValue) {
                        CreatedCopy = CreatedCopy + " " + editRecordDateAdded.ToString(); // editRecord.dateAdded
                    }
                    //
                    CreatedBy = "the system";
                    if (editRecord.createByMemberId != 0) {
                        CS = core.db.csOpenSql_rev("default", "select Name,Active from ccMembers where id=" + editRecord.createByMemberId);
                        //CS = core.app.openCsSql_rev("default", "select Name,Active from ccmembers where id=" & EditRecord.AddedByMemberID)
                        if (core.db.csOk(CS)) {
                            Name = core.db.csGetText(CS, "name");
                            Active = core.db.csGetBoolean(CS, "active");
                            if (!Active && (!string.IsNullOrEmpty(Name))) {
                                CreatedBy = "Inactive user " + Name;
                            } else if (!Active) {
                                CreatedBy = "Inactive user #" + editRecord.createByMemberId;
                            } else if (string.IsNullOrEmpty(Name)) {
                                CreatedBy = "Unnamed user #" + editRecord.createByMemberId;
                            } else {
                                CreatedBy = Name;
                            }
                        } else {
                            CreatedBy = "deleted user #" + editRecord.createByMemberId;
                        }
                        core.db.csClose(ref CS);
                    }
                    if (!string.IsNullOrEmpty(CreatedBy)) {
                        CreatedCopy = CreatedCopy + " by " + CreatedBy;
                    } else {
                    }
                    HeaderDescription = HeaderDescription + "Created:" + CreatedCopy;
                    //
                    ModifiedCopy = "";
                    if (editRecord.modifiedDate == DateTime.MinValue) {
                        ModifiedCopy = CreatedCopy;
                    } else {
                        ModifiedCopy = ModifiedCopy + " " + editRecord.modifiedDate;
                        CreatedBy = "the system";
                        if (editRecord.modifiedByMemberID != 0) {
                            CS = core.db.csOpenSql_rev("default", "select Name,Active from ccMembers where id=" + editRecord.modifiedByMemberID);
                            //CS = core.app.openCsSql_rev("default", "select Name,Active from ccmembers where id=" & EditRecord.ModifiedByMemberID)
                            if (core.db.csOk(CS)) {
                                Name = core.db.csGetText(CS, "name");
                                Active = core.db.csGetBoolean(CS, "active");
                                if (!Active && (!string.IsNullOrEmpty(Name))) {
                                    CreatedBy = "Inactive user " + Name;
                                } else if (!Active) {
                                    CreatedBy = "Inactive user #" + editRecord.modifiedByMemberID;
                                } else if (string.IsNullOrEmpty(Name)) {
                                    CreatedBy = "Unnamed user #" + editRecord.modifiedByMemberID;
                                } else {
                                    CreatedBy = Name;
                                }
                            } else {
                                CreatedBy = "deleted user #" + editRecord.modifiedByMemberID;
                            }
                            core.db.csClose(ref CS);
                        }
                        if (!string.IsNullOrEmpty(CreatedBy)) {
                            ModifiedCopy = ModifiedCopy + " by " + CreatedBy;
                        } else {
                        }
                    }
                    if (false) {
                        //if (editRecord.IsInserted) {
                        //    HeaderDescription = HeaderDescription + "<br>Last Published: not published";
                        //} else {
                        //    HeaderDescription = HeaderDescription + "<br>Last Published:" + ModifiedCopy;
                        //}
                    } else {
                        HeaderDescription = HeaderDescription + "<br>Last Modified:" + ModifiedCopy;
                    }
                    //
                    // Add Edit Locking to right panel
                    //
                    if (editRecord.EditLock) {
                        HeaderDescription = HeaderDescription + "<br><b>Record is locked by " + editRecord.EditLockMemberName + " until " + editRecord.EditLockExpires + "</b>";
                    }
                    //
                    HeaderDescription = HeaderDescription + "</td></tr>";
                    //
                    //If Not False Then
                    //    HeaderDescription = HeaderDescription & "<tr><td colspan=2>Authoring Mode: Immediate</td></tr>"
                    //Else
                    //    HeaderDescription = HeaderDescription & "<tr><td style=""vertical-align:top;"">Authoring Mode: Workflow</td>"
                    //    If editRecord.EditLock Then
                    //        WFMessage = WFMessage & "<div>Locked: Currently being edited by " & editRecord.EditLockMemberName & "</div>"
                    //    End If
                    //    If editRecord.LockModifiedDate <> Date.MinValue Then
                    //        WFMessage = WFMessage & "<div>Modified: " & editRecord.LockModifiedDate & " by " & editRecord.LockModifiedName & " and has not been published</div>"
                    //    End If
                    //    If editRecord.SubmitLock Then
                    //        WFMessage = WFMessage & "<div>Submitted for Publishing: " & editRecord.SubmittedDate & " by " & editRecord.SubmittedName & "</div>"
                    //    End If
                    //    If editRecord.ApproveLock Then
                    //        WFMessage = WFMessage & "<div>Approved for Publishing: " & editRecord.SubmittedDate & " by " & editRecord.SubmittedName & "</div>"
                    //    End If
                    //    If WFMessage <> "" Then
                    //        HeaderDescription = HeaderDescription & "<td>" & WFMessage & "</td></tr>"
                    //    Else
                    //        HeaderDescription = HeaderDescription & "<td>&nbsp;</td></tr>"
                    //    End If
                    //End If
                    //
                    HeaderDescription = HeaderDescription + "</table>";
                }
                //
                // ----- determine access details
                //
                core.doc.sessionContext.getContentAccessRights(core, adminContent.Name, ref allowCMEdit, ref allowCMAdd, ref allowCMDelete);
                AllowAdd = adminContent.AllowAdd && allowCMAdd;
                AllowDelete = adminContent.AllowDelete & allowCMDelete & (editRecord.id != 0);
                allowSave = allowCMEdit;
                //
                // ----- custom fieldEditors
                //
                //
                //   Editor Preference
                //   any addon can be an editor for a fieldtype with a checkbox in the addon
                //   the editor in any field can be over-ridden by just a single member with a popup next to the editor
                //       that popup (fancybox) sets the hidden fieldEditorPreference to fieldid:addonid and submits the form
                //       the edit form does a refresh action after setting the members property "editorPreferencesForContent:99"
                //   if no editor preference, the default editor is used from a drop-down selection in fieldtypes
                //   if nothing in field types, Contensive handles it internally
                //
                Stream.Add("\r<input type=\"hidden\" name=\"fieldEditorPreference\" id=\"fieldEditorPreference\" value=\"\">");
                //
                fieldEditorList = editorController.getFieldTypeDefaultEditorAddonIdList(core);
                fieldTypeDefaultEditors = fieldEditorList.Split(',');
                //
                // load user's editor preferences to fieldEditorPreferences() - this is the editor this user has picked when there are >1
                //   fieldId:addonId,fieldId:addonId,etc
                //   with custom FancyBox form in edit window with button "set editor preference"
                //   this button causes a 'refresh' action, reloads fields with stream without save
                //
                fieldEditorPreferencesList = core.userProperty.getText("editorPreferencesForContent:" + adminContent.Id, "");
                //
                // add the addon editors assigned to each field
                // !!!!! this should be added to metaData load
                //
                int Cnt = 0;
                SQL = "select"
                    + " f.id,f.editorAddonID"
                    + " from ccfields f"
                    + " where"
                    + " f.ContentID=" + adminContent.Id + " and f.editorAddonId is not null";
                dt = core.db.executeQuery(SQL);

                Cells = core.db.convertDataTabletoArray(dt);
                Cnt = Cells.GetLength(1);
                //If CBool(Cells.GetLength(0)) Then
                //    Cnt = 0
                //Else
                //    Cnt = UBound(Cells, 2) + 1
                //End If
                for (Ptr = 0; Ptr < Cnt; Ptr++) {
                    fieldId = genericController.encodeInteger(Cells[0, Ptr]);
                    if (fieldId > 0) {
                        fieldEditorPreferencesList = fieldEditorPreferencesList + "," + fieldId + ":" + Cells[1, Ptr];
                    }
                }
                //
                // load fieldEditorOptions - these are all the editors available for each field
                //
                fieldEditorOptionCnt = 0;
                SQL = "select r.contentFieldTypeId,a.Id"
                    + " from ccAddonContentFieldTypeRules r"
                    + " left join ccaggregatefunctions a on a.id=r.addonid"
                    + " where (r.active<>0)and(a.active<>0)and(a.id is not null) order by r.contentFieldTypeID";
                dt = core.db.executeQuery(SQL);
                Cells = core.db.convertDataTabletoArray(dt);
                fieldEditorOptionCnt = Cells.GetUpperBound(1) + 1;
                for (Ptr = 0; Ptr < fieldEditorOptionCnt; Ptr++) {
                    fieldId = genericController.encodeInteger(Cells[0, Ptr]);
                    if ((fieldId > 0) && (!fieldEditorOptions.ContainsKey(fieldId.ToString()))) {
                        fieldEditorOptions.Add(fieldId.ToString(), genericController.encodeInteger(Cells[1, Ptr]));
                    }
                }
                //
                // ----- determine contentType for editor
                //
                if (genericController.vbLCase(adminContent.Name) == "email templates") {
                    ContentType = csv_contentTypeEnum.contentTypeEmailTemplate;
                } else if (genericController.vbLCase(adminContent.ContentTableName) == "cctemplates") {
                    ContentType = csv_contentTypeEnum.contentTypeWebTemplate;
                } else if (genericController.vbLCase(adminContent.ContentTableName) == "ccemail") {
                    ContentType = csv_contentTypeEnum.contentTypeEmail;
                } else {
                    ContentType = csv_contentTypeEnum.contentTypeWeb;
                }
                //
                // ----- editor strings needed - needs to be on-demand
                //
                editorAddonListJSON = core.html.JSONeditorAddonList(ContentType);
                styleList = ""; // core.html.main_GetStyleSheet2(ContentType, TemplateIDForStyles, emailIdForStyles)
                                //
                                // ----- Create edit page
                                //
                                //todo  NOTE: The following VB 'Select Case' included either a non-ordinal switch expression or non-ordinal, range-type, or non-constant 'Case' expressions and was converted to C# 'if-else' logic:
                                //				Select Case genericController.vbUCase(adminContent.ContentTableName)
                                //ORIGINAL LINE: Case genericController.vbUCase("ccMembers")
                if (genericController.vbUCase(adminContent.ContentTableName) == genericController.vbUCase("ccMembers")) {
                    if (!(core.doc.sessionContext.isAuthenticatedAdmin(core))) {
                        //
                        // Must be admin
                        //
                        Stream.Add(GetForm_Error("This edit form requires Member Administration access.", ""));
                    } else {
                        EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd);
                        EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeletePerson);
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
                        Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(GetForm_Edit_AddTab("Groups", GetForm_Edit_MemberGroups(adminContent, editRecord), allowAdminTabs));
                        //Call Stream.Add(GetForm_Edit_AddTab("Topics", GetForm_Edit_TopicRules, AllowAdminTabs))
                        //Call Stream.Add(GetForm_Edit_AddTab("Calendar", GetForm_Edit_CalendarEvents, AllowAdminTabs))
                        Stream.Add(GetForm_Edit_AddTab("Reports", GetForm_Edit_MemberReports(adminContent, editRecord), allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Control&nbsp;Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
                        if (allowAdminTabs) {
                            Stream.Add(core.html.getComboTabs());
                        }
                        Stream.Add(EditSectionButtonBar);
                    }
                }
                //ORIGINAL LINE: Case "CCEMAIL"
                else if (adminContent.ContentTableName.ToLower() == "ccemail") {
                    //
                    // ----- Email table
                    //
                    SystemEmailCID = cdefModel.getContentId(core, "System Email");
                    ConditionalEmailCID = cdefModel.getContentId(core, "Conditional Email");
                    LastSendTestDate = DateTime.MinValue;
                    if (true) // 3.4.201" Then
                    {
                        AllowEmailSendWithoutTest = (core.siteProperties.getBoolean("AllowEmailSendWithoutTest", false));
                        if (editRecord.fieldsLc.ContainsKey("lastsendtestdate")) {
                            LastSendTestDate = genericController.encodeDate(editRecord.fieldsLc["lastsendtestdate"].value);
                        }
                    }
                    if (!(core.doc.sessionContext.isAuthenticatedAdmin(core))) {
                        //
                        // Must be admin
                        //
                        Stream.Add(GetForm_Error("This edit form requires Member Administration access.", "This edit form requires Member Administration access."));
                    } else if (cdefModel.isWithinContent(core, editRecord.contentControlId, SystemEmailCID)) {
                        //
                        // System Email
                        //
                        EmailSubmitted = false;
                        if (editRecord.id != 0) {
                            if (editRecord.fieldsLc.ContainsKey("testmemberid")) {
                                editRecord.fieldsLc["testmemberid"].value = core.doc.sessionContext.user.id;
                            }
                        }
                        EditSectionButtonBar = "";
                        if (MenuDepth > 0) {
                            EditSectionButtonBar += core.html.button(ButtonClose, "", "", "window.close();");
                        } else {
                            EditSectionButtonBar += core.html.button(ButtonCancel, "", "", "Return processSubmit(this)");
                        }
                        if ((AllowDelete) && (core.doc.sessionContext.isAuthenticatedDeveloper(core))) {
                            EditSectionButtonBar += core.html.button(ButtonDeleteEmail, "", "", "If(!DeleteCheck())Return False;");
                        }
                        if ((!EmailSubmitted) && (!EmailSent)) {
                            EditSectionButtonBar += core.html.button(ButtonSave, "", "", "Return processSubmit(this)");
                            EditSectionButtonBar += core.html.button(ButtonOK, "", "", "Return processSubmit(this)");
                            EditSectionButtonBar += core.html.button(ButtonSendTest, "", "", "Return processSubmit(this)");
                        } else if (AllowAdd) {
                            EditSectionButtonBar += core.html.button(ButtonCreateDuplicate, "", "", "Return processSubmit(this)");
                        }
                        EditSectionButtonBar = htmlController.div(EditSectionButtonBar, "", "ccButtonCon");
                        //
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
                        Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(GetForm_Edit_AddTab("Send&nbsp;To&nbsp;Groups", GetForm_Edit_EmailRules(adminContent, editRecord, editRecord.Read_Only & (!core.doc.sessionContext.isAuthenticatedDeveloper(core))), allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Send&nbsp;To&nbsp;Topics", GetForm_Edit_EmailTopics(adminContent, editRecord, editRecord.Read_Only & (!core.doc.sessionContext.isAuthenticatedDeveloper(core))), allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Bounce&nbsp;Control", GetForm_Edit_EmailBounceStatus(), allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Control&nbsp;Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
                        if (allowAdminTabs) {
                            Stream.Add(core.html.getComboTabs());
                            //Call Stream.Add("<div Class=""ccPanelBackground"">" & core.main_GetComboTabs() & "</div>")
                        }
                        Stream.Add(EditSectionButtonBar);
                    } else if (cdefModel.isWithinContent(core, editRecord.contentControlId, ConditionalEmailCID)) {
                        //
                        // Conditional Email
                        //
                        EmailSubmitted = false;
                        if (editRecord.id != 0) {
                            if (editRecord.fieldsLc.ContainsKey("testmemberid")) {
                                editRecord.fieldsLc["testmemberid"].value = core.doc.sessionContext.user.id;
                            }
                            if (editRecord.fieldsLc.ContainsKey("submitted")) {
                                EmailSubmitted = genericController.encodeBoolean(editRecord.fieldsLc["submitted"].value);
                            }
                        }
                        EditSectionButtonBar = "";
                        if (MenuDepth > 0) {
                            EditSectionButtonBar += core.html.button(ButtonClose, "", "", "window.close();");
                        } else {
                            EditSectionButtonBar += core.html.button(ButtonCancel, "", "", "Return processSubmit(this)");
                        }
                        if (AllowDelete) {
                            EditSectionButtonBar += core.html.button(ButtonDeleteEmail, "", "", "If(!DeleteCheck())Return False;");
                        }
                        if (!EmailSubmitted) {
                            //
                            // Not Submitted
                            //
                            EditSectionButtonBar += core.html.button(ButtonSave, "", "", "Return processSubmit(this)");
                            EditSectionButtonBar += core.html.button(ButtonOK, "", "", "Return processSubmit(this)");
                            EditSectionButtonBar += core.html.button(ButtonActivate, "", "", "Return processSubmit(this)", (LastSendTestDate == DateTime.MinValue) && (!AllowEmailSendWithoutTest));
                            EditSectionButtonBar += core.html.button(ButtonSendTest, "", "", "Return processSubmit(this)");
                        } else {
                            //
                            // Submitted
                            //
                            if (AllowAdd) {
                                EditSectionButtonBar += core.html.button(ButtonCreateDuplicate, "", "", "Return processSubmit(this)");
                            }
                            EditSectionButtonBar += core.html.button(ButtonDeactivate, "", "", "Return processSubmit(this)");
                        }
                        EditSectionButtonBar = htmlController.div(EditSectionButtonBar, "", "ccButtonCon");
                        //
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
                        Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only || EmailSubmitted, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(GetForm_Edit_AddTab("Condition&nbsp;Groups", GetForm_Edit_EmailRules(adminContent, editRecord, editRecord.Read_Only || EmailSubmitted), allowAdminTabs));
                        //Call Stream.Add(GetForm_Edit_AddTab("Send&nbsp;To&nbsp;Topics", GetForm_Edit_EmailTopics(editrecord.read_only Or EmailSubmitted), AllowAdminTabs))
                        Stream.Add(GetForm_Edit_AddTab("Bounce&nbsp;Control", GetForm_Edit_EmailBounceStatus(), allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Control&nbsp;Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
                        if (allowAdminTabs) {
                            Stream.Add(core.html.getComboTabs());
                            //Call Stream.Add("<div Class=""ccPanelBackground"">" & core.main_GetComboTabs() & "</div>")
                        }
                        Stream.Add(EditSectionButtonBar);
                    } else {
                        //
                        // Group Email
                        //
                        EmailSubmitted = false;
                        EmailSent = false;
                        if (editRecord.id != 0) {
                            if (editRecord.fieldsLc.ContainsKey("testmemberid")) {
                                editRecord.fieldsLc["testmemberid"].value = core.doc.sessionContext.user.id;
                            }
                            if (editRecord.fieldsLc.ContainsKey("submitted")) {
                                EmailSubmitted = genericController.encodeBoolean(editRecord.fieldsLc["submitted"].value);
                            }
                            if (editRecord.fieldsLc.ContainsKey("sent")) {
                                EmailSent = genericController.encodeBoolean(editRecord.fieldsLc["sent"].value);
                            }
                        }
                        EditSectionButtonBar = "";
                        if (MenuDepth > 0) {
                            EditSectionButtonBar += core.html.button(ButtonClose, "", "", "window.close();");
                        } else {
                            EditSectionButtonBar += core.html.button(ButtonCancel, "", "", "Return processSubmit(this)");
                        }
                        if (editRecord.id != 0) {
                            EditSectionButtonBar += core.html.button(ButtonDeleteEmail, "", "", "If(!DeleteCheck())Return False;");
                        }
                        if ((!EmailSubmitted) && (!EmailSent)) {
                            EditSectionButtonBar += core.html.button(ButtonSave, "", "", "Return processSubmit(this)");
                            EditSectionButtonBar += core.html.button(ButtonOK, "", "", "Return processSubmit(this)");
                            EditSectionButtonBar += core.html.button(ButtonSend, "", "", "Return processSubmit(this)", (LastSendTestDate == DateTime.MinValue) && (!AllowEmailSendWithoutTest));
                            EditSectionButtonBar += core.html.button(ButtonSendTest, "", "", "Return processSubmit(this)");
                        } else {
                            //
                            // Submitted
                            //
                            EditSectionButtonBar += core.html.button(ButtonCreateDuplicate, "", "", "Return processSubmit(this)");
                        }
                        EditSectionButtonBar = htmlController.div(EditSectionButtonBar, "", "ccButtonCon");
                        //
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
                        Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only | EmailSubmitted || EmailSent, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(GetForm_Edit_AddTab("Send&nbsp;To&nbsp;Groups", GetForm_Edit_EmailRules(adminContent, editRecord, editRecord.Read_Only | EmailSubmitted || EmailSent), allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Send&nbsp;To&nbsp;Topics", GetForm_Edit_EmailTopics(adminContent, editRecord, editRecord.Read_Only | EmailSubmitted || EmailSent), allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Bounce&nbsp;Control", GetForm_Edit_EmailBounceStatus(), allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Control&nbsp;Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
                        if (allowAdminTabs) {
                            Stream.Add(core.html.getComboTabs());
                            //Call Stream.Add("<div Class=""ccPanelBackground"">" & core.main_GetComboTabs() & "</div>")
                        }
                        Stream.Add(EditSectionButtonBar);
                    }
                }
                //ORIGINAL LINE: Case "CCCONTENT"
                else if (genericController.vbUCase(adminContent.ContentTableName) == "CCCONTENT") {
                    if (!(core.doc.sessionContext.isAuthenticatedAdmin(core))) {
                        //
                        // Must be admin
                        //
                        Stream.Add(GetForm_Error("This edit form requires Member Administration access.", "This edit form requires Member Administration access."));
                    } else {
                        EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd);
                        EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord);
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
                        Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(GetForm_Edit_AddTab("Authoring Permissions", GetForm_Edit_GroupRules(adminContent, editRecord), allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Control&nbsp;Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
                        if (allowAdminTabs) {
                            Stream.Add(core.html.getComboTabs());
                            //Call Stream.Add("<div class=""ccPanelBackground"">" & core.main_GetComboTabs() & "</div>")
                        }
                        Stream.Add(EditSectionButtonBar);
                    }
                    //
                }
                //ORIGINAL LINE: Case "CCPAGECONTENT"
                else if (genericController.vbUCase(adminContent.ContentTableName) == "CCPAGECONTENT") {
                    //
                    // Page Content
                    //
                    TableID = core.db.getRecordID("Tables", "ccPageContent");
                    EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, (!IsLandingPage) && (!IsLandingPageParent) && AllowDelete, allowSave, AllowAdd, true);
                    EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeletePage);
                    Stream.Add(EditSectionButtonBar);
                    Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
                    Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, IsLandingPage || IsLandingPageParent, IsRootPage, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    //Call Stream.Add(GetForm_Edit_AddTab("Meta Content", GetForm_Edit_MetaContent(adminContent, editRecord, editRecord.Read_Only), allowAdminTabs))
                    Stream.Add(GetForm_Edit_AddTab("Link Aliases", GetForm_Edit_LinkAliases(adminContent, editRecord, editRecord.Read_Only), allowAdminTabs));
                    //Call Stream.Add(GetForm_Edit_AddTab("Topics", GetForm_Edit_TopicRules, AllowAdminTabs))
                    //Call Stream.Add(GetForm_Edit_AddTab("RSS/Podcasts", GetForm_Edit_RSSFeeds(EditRecord.ContentName, EditRecord.ContentID, EditRecord.ID, core.main_GetPageLink(EditRecord.ID)), AllowAdminTabs))
                    Stream.Add(GetForm_Edit_AddTab("Content Watch", GetForm_Edit_ContentTracking(adminContent, editRecord), allowAdminTabs));
                    //Call Stream.Add(GetForm_Edit_AddTab("Calendar", GetForm_Edit_CalendarEvents, AllowAdminTabs))
                    Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
                    if (allowAdminTabs) {
                        Stream.Add(core.html.getComboTabs());
                    }
                    Stream.Add(EditSectionButtonBar);
                    //Case "CCSECTIONS"
                    //    '
                    //    ' Site Sections
                    //    '
                    //    EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, (Not IsLandingSection) And AllowDelete, allowSave, AllowAdd)
                    //    EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
                    //    Call Stream.Add(EditSectionButtonBar)
                    //    Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                    //    Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, IsLandingSection, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                    //    Call Stream.Add(GetForm_Edit_AddTab("Select Menus", GetForm_Edit_SectionDynamicMenuRules(adminContent, editRecord), allowAdminTabs))
                    //    Call Stream.Add(GetForm_Edit_AddTab("Section Blocking", GetForm_Edit_SectionBlockRules(adminContent, editRecord), allowAdminTabs))
                    //    Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
                    //    If allowAdminTabs Then
                    //        Call Stream.Add(core.htmlDoc.menu_GetComboTabs())
                    //        'Call Stream.Add("<div class=""ccPanelBackground"">" & core.main_GetComboTabs() & "</div>")
                    //    End If
                    //    Call Stream.Add(EditSectionButtonBar)
                    //Case "CCDYNAMICMENUS"
                    //    '
                    //    ' Edit Dynamic Sections
                    //    '
                    //    EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd)
                    //    EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
                    //    Call Stream.Add(EditSectionButtonBar)
                    //    Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                    //    Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, False, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                    //    Call Stream.Add(GetForm_Edit_AddTab("Select Sections", GetForm_Edit_DynamicMenuSectionRules(adminContent, editRecord), allowAdminTabs))
                    //    Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
                    //    If allowAdminTabs Then
                    //        Call Stream.Add(core.htmlDoc.menu_GetComboTabs())
                    //        'Call Stream.Add("<div class=""ccPanelBackground"">" & core.main_GetComboTabs() & "</div>")
                    //    End If
                    //    Call Stream.Add(EditSectionButtonBar)
                }
                //ORIGINAL LINE: Case "CCLIBRARYFOLDERS"
                else if (genericController.vbUCase(adminContent.ContentTableName) == "CCLIBRARYFOLDERS") {
                    //
                    // Library Folders
                    //
                    EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd);
                    EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord);
                    Stream.Add(EditSectionButtonBar);
                    Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
                    Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    Stream.Add(GetForm_Edit_AddTab("Authoring Access", GetForm_Edit_LibraryFolderRules(adminContent, editRecord), allowAdminTabs));
                    Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
                    if (allowAdminTabs) {
                        Stream.Add(core.html.getComboTabs());
                    }
                    Stream.Add(EditSectionButtonBar);
                }
                //ORIGINAL LINE: Case genericController.vbUCase("ccGroups")
                else if (genericController.vbUCase(adminContent.ContentTableName) == genericController.vbUCase("ccGroups")) {
                    //Case "CCGROUPS"
                    //
                    // Groups
                    //
                    EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd);
                    EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord);
                    Stream.Add(EditSectionButtonBar);
                    Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
                    Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    Stream.Add(GetForm_Edit_AddTab("Authoring Permissions", GetForm_Edit_ContentGroupRules(adminContent, editRecord), allowAdminTabs));
                    //Call Stream.Add(GetForm_Edit_AddTab("Meta Content", GetForm_Edit_MetaContent(adminContent, editRecord, editRecord.Read_Only), allowAdminTabs))
                    //Call Stream.Add(GetForm_Edit_AddTab("Topics", GetForm_Edit_TopicRules, AllowAdminTabs))
                    Stream.Add(GetForm_Edit_AddTab("Content Watch", GetForm_Edit_ContentTracking(adminContent, editRecord), allowAdminTabs));
                    //Call Stream.Add(GetForm_Edit_AddTab("Calendar", GetForm_Edit_CalendarEvents, AllowAdminTabs))
                    Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
                    if (allowAdminTabs) {
                        Stream.Add(core.html.getComboTabs());
                    }
                    Stream.Add(EditSectionButtonBar);
                    //
                    // This is the beginnings of a good idea. use a selector string to create the value input. The problem is
                    // both the selector and value appear on the same page. if you screw up the selector, you can not save it
                    // again without also saving the 'bad' value it creates.
                    //
                    // For now, skip this and put the higher-level interface in control pages (an add-on type).
                    //
                    //        Case "CCSETUP"
                    //            '
                    //            '   Site Properties
                    //            '
                    //            EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord,)
                    //            EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
                    //            Call Stream.Add(EditSectionButtonBar)
                    //            Call Stream.Add(Adminui.GetTitleBar( GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                    //            Call Stream.Add(GetForm_Edit_UserFieldTabs(adminContent, editRecord,FormID, editrecord.read_only, False, False, ContentType, AllowAjaxTabs))
                    //            Call Stream.Add(GetForm_Edit_AddTab("Site Property", GetForm_Edit_SiteProperties(FormID), AllowAdminTabs))
                    //            Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editrecord), AllowAdminTabs))
                    //            If AllowAdminTabs Then
                    //                Call Stream.Add(core.main_GetComboTabs())
                    //            End If
                    //            Call Stream.Add(EditSectionButtonBar)
                }
                //ORIGINAL LINE: Case "CCLAYOUTS"
                else if (genericController.vbUCase(adminContent.ContentTableName) == "CCLAYOUTS") {
                    //
                    // LAYOUTS
                    //
                    EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd);
                    EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord);
                    Stream.Add(EditSectionButtonBar);
                    Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
                    Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    Stream.Add(GetForm_Edit_AddTab("Reports", GetForm_Edit_LayoutReports(adminContent, editRecord), allowAdminTabs));
                    Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
                    if (allowAdminTabs) {
                        Stream.Add(core.html.getComboTabs());
                    }
                    Stream.Add(EditSectionButtonBar);
                }
                //ORIGINAL LINE: Case Else
                else {
                    //
                    // All other tables (User definined)
                    //
                    EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd);
                    EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord);
                    Stream.Add(EditSectionButtonBar);
                    Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
                    Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    //Call Stream.Add(GetForm_Edit_AddTab("Meta Content", GetForm_Edit_MetaContent(adminContent, editRecord, editRecord.Read_Only), allowAdminTabs))
                    Stream.Add(GetForm_Edit_AddTab("Content Watch", GetForm_Edit_ContentTracking(adminContent, editRecord), allowAdminTabs));
                    Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
                    if (allowAdminTabs) {
                        Stream.Add(core.html.getComboTabs());
                    }
                    Stream.Add(EditSectionButtonBar);
                }
                Stream.Add("</form>");
                returnHtml = Stream.Text;
                if (editRecord.id == 0) {
                    core.html.addTitle("Add " + adminContent.Name);
                } else if (editRecord.nameLc == "") {
                    core.html.addTitle("Edit #" + editRecord.id + " in " + editRecord.contentControlId_Name);
                } else {
                    core.html.addTitle("Edit " + editRecord.nameLc + " in " + editRecord.contentControlId_Name);
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnHtml;
        }
        //
        //========================================================================
        // ----- Print the Normal Content Edit form
        //
        //   Print the content fields and Topic Groups section
        //========================================================================
        //
        private string GetForm_Publish() {
            string tempGetForm_Publish = null;
            try {
                //
                string FieldList = null;
                string ModifiedDateString = null;
                string SubmittedDateString = null;
                string ApprovedDateString = null;
                adminUIController Adminui = new adminUIController(core);
                string ButtonList = "";
                string Caption = null;
                int CS = 0;
                string SQL = null;
                string RowColor = null;
                int RecordCount = 0;
                int RecordLast = 0;
                int RecordNext = 0;
                int RecordPrevious = 0;
                string RecordName = null;
                string Copy = null;
                int ContentID = 0;
                string ContentName = null;
                int RecordID = 0;
                string Link = null;
                int CSAuthoringRecord = 0;
                string TableName = null;
                bool IsInserted = false;
                bool IsDeleted = false;
                bool IsModified = false;
                string ModifiedName = "";
                DateTime ModifiedDate = default(DateTime);
                bool IsSubmitted = false;
                string SubmitName = "";
                DateTime SubmittedDate = default(DateTime);
                bool IsApproved = false;
                string ApprovedName = "";
                DateTime ApprovedDate = default(DateTime);
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string Body = "";
                string Description = null;
                string Button = null;
                string BR = "";
                //
                Button = core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    //
                    //
                    //
                    return core.webServer.redirect("/" + core.appConfig.adminRoute, "Admin Publish, Cancel Button Pressed");
                } else if (!core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                    //
                    //
                    //
                    ButtonList = ButtonCancel;
                    Body += Adminui.GetFormBodyAdminOnly();
                } else {
                    //
                    // ----- Page Body
                    //
                    BR = "<br>";
                    Body += "\r<table border=\"0\" cellpadding=\"2\" cellspacing=\"2\" width=\"100%\">";
                    Body += "\r<tr>";
                    Body += "\r<td width=\"50\" class=\"ccPanel\" align=\"center\" class=\"ccAdminSmall\">Pub" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"42\" height=\"1\" ></td>";
                    Body += "\r<td width=\"50\" class=\"ccPanel\" align=\"center\" class=\"ccAdminSmall\">Sub'd" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"42\" height=\"1\" ></td>";
                    Body += "\r<td width=\"50\" class=\"ccPanel\" align=\"center\" class=\"ccAdminSmall\">Appr'd" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"42\" height=\"1\" ></td>";
                    Body += "\r<td width=\"50\" class=\"ccPanel\" class=\"ccAdminSmall\">Edit" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"42\" height=\"1\" ></td>";
                    Body += "\r<td width=\"200\" class=\"ccPanel\" class=\"ccAdminSmall\">Name" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"192\" height=\"1\" ></td>";
                    Body += "\r<td width=\"100\" class=\"ccPanel\" class=\"ccAdminSmall\">Content" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"92\" height=\"1\" ></td>";
                    Body += "\r<td width=\"50\" class=\"ccPanel\" class=\"ccAdminSmall\">#" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"92\" height=\"1\" ></td>";
                    Body += "\r<td width=\"100\" class=\"ccPanel\" class=\"ccAdminSmall\">Public" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"92\" height=\"1\" ></td>";
                    Body += "\r<td width=\"100%\" class=\"ccPanel\" class=\"ccAdminSmall\">Status" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"1\" ></td>";
                    Body += "\r</tr>";
                    //
                    // ----- select modified,submitted,approved records (all non-editing controls)
                    //
                    SQL = "SELECT DISTINCT top 100 ccAuthoringControls.ContentID AS ContentID, ccContent.Name AS ContentName, ccAuthoringControls.RecordID, ccContentWatch.Link AS Link, ccContent.AllowWorkflowAuthoring AS ContentAllowWorkflowAuthoring,min(ccAuthoringControls.ID)"
                        + " FROM (ccAuthoringControls"
                        + " LEFT JOIN ccContent ON ccAuthoringControls.ContentID = ccContent.ID)"
                        + " LEFT JOIN ccContentWatch ON ccAuthoringControls.ContentRecordKey = ccContentWatch.ContentRecordKey"
                        + " Where (ccAuthoringControls.ControlType > 1)"
                        + " GROUP BY ccAuthoringControls.ContentID, ccContent.Name, ccAuthoringControls.RecordID, ccContentWatch.Link, ccContent.AllowWorkflowAuthoring"
                        + " order by min(ccAuthoringControls.ID) desc";
                    //PageNumber = 1 + (RecordTop / RecordsPerPage)
                    //SQL = "SELECT DISTINCT ccContent.ID AS ContentID, ccContent.Name AS ContentName, ccAuthoringControls.RecordID, ccContentWatch.Link AS Link, ccContent.AllowWorkflowAuthoring AS ContentAllowWorkflowAuthoring,max(ccAuthoringControls.DateAdded) as DateAdded" _
                    //    & " FROM (ccAuthoringControls LEFT JOIN ccContent ON ccAuthoringControls.ContentID = ccContent.ID) LEFT JOIN ccContentWatch ON ccAuthoringControls.ContentRecordKey = ccContentWatch.ContentRecordKey" _
                    //    & " GROUP BY ccAuthoringControls.ID,ccContent.ID, ccContent.Name, ccAuthoringControls.RecordID, ccContentWatch.Link, ccContent.AllowWorkflowAuthoring, ccAuthoringControls.ControlType" _
                    //    & " HAVING (ccAuthoringControls.ControlType>1)" _
                    //    & " order by max(ccAuthoringControls.DateAdded) Desc"
                    CS = core.db.csOpenSql_rev("Default", SQL);
                    //CS = core.app_openCsSql_Rev_Internal("Default", SQL, RecordsPerPage, PageNumber)
                    RecordCount = 0;
                    if (core.db.csOk(CS)) {
                        RowColor = "";
                        RecordLast = RecordTop + RecordsPerPage;
                        //
                        // --- Print out the records
                        //
                        while (core.db.csOk(CS) && RecordCount < 100) {
                            ContentID = core.db.csGetInteger(CS, "contentID");
                            ContentName = core.db.csGetText(CS, "contentname");
                            RecordID = core.db.csGetInteger(CS, "recordid");
                            Link = pageContentController.getPageLink(core, RecordID, "", true, false);
                            //Link = core.main_GetPageLink3(RecordID, "", True)
                            //If Link = "" Then
                            //    Link = core.db.cs_getText(CS, "Link")
                            //End If
                            if ((ContentID == 0) || (string.IsNullOrEmpty(ContentName)) || (RecordID == 0)) {
                                //
                                // This control is not valid, delete it
                                //
                                SQL = "delete from ccAuthoringControls where ContentID=" + ContentID + " and RecordID=" + RecordID;
                                core.db.executeQuery(SQL);
                            } else {
                                TableName = cdefModel.GetContentProperty(core, ContentName, "ContentTableName");
                                if (!(core.db.csGetBoolean(CS, "ContentAllowWorkflowAuthoring"))) {
                                    //
                                    // Authoring bug -- This record should not be here, the content does not support workflow authoring
                                    //
                                    handleLegacyClassError2("GetForm_Publish", "Admin Workflow Publish selected an authoring control record [" + ContentID + "." + RecordID + "] for a content definition [" + ContentName + "] that does not AllowWorkflowAuthoring.");
                                    //Call HandleInternalError("GetForm_Publish", "Admin Workflow Publish selected an authoring control record [" & ContentID & "." & RecordID & "] for a content definition [" & ContentName & "] that does not AllowWorkflowAuthoring.")
                                } else {

                                    core.doc.getAuthoringStatus(ContentName, RecordID, ref IsSubmitted, ref IsApproved, ref SubmitName, ref ApprovedName, ref IsInserted, ref IsDeleted, ref IsModified, ref ModifiedName, ref ModifiedDate, ref SubmittedDate, ref ApprovedDate);
                                    if (RowColor == "class=\"ccPanelRowOdd\"") {
                                        RowColor = "class=\"ccPanelRowEven\"";
                                    } else {
                                        RowColor = "class=\"ccPanelRowOdd\"";
                                    }
                                    //
                                    // make sure the record exists
                                    //
                                    if (genericController.vbUCase(TableName) == "CCPAGECONTENT") {
                                        FieldList = "ID,Name,Headline,MenuHeadline";
                                        //SQL = "SELECT ID,Name,Headline,MenuHeadline from " & TableName & " WHERE ID=" & RecordID
                                    } else {
                                        FieldList = "ID,Name,Name as Headline,Name as MenuHeadline";
                                        //SQL = "SELECT ID,Name,Name as Headline,Name as MenuHeadline from " & TableName & " WHERE ID=" & RecordID
                                    }
                                    CSAuthoringRecord = core.db.csOpenRecord(ContentName, RecordID, true, true, FieldList);
                                    //CSAuthoringRecord = core.app_openCsSql_Rev_Internal("Default", SQL, 1)
                                    if (!core.db.csOk(CSAuthoringRecord)) {
                                        //
                                        // This authoring control is not valid, delete it
                                        //
                                        SQL = "delete from ccAuthoringControls where ContentID=" + ContentID + " and RecordID=" + RecordID;
                                        core.db.executeQuery(SQL);
                                    } else {
                                        RecordName = core.db.csGet(CSAuthoringRecord, "name");
                                        if (string.IsNullOrEmpty(RecordName)) {
                                            RecordName = core.db.csGet(CSAuthoringRecord, "headline");
                                            if (string.IsNullOrEmpty(RecordName)) {
                                                RecordName = core.db.csGet(CSAuthoringRecord, "headline");
                                                if (string.IsNullOrEmpty(RecordName)) {
                                                    RecordName = "Record " + core.db.csGet(CSAuthoringRecord, "ID");
                                                }
                                            }
                                        }
                                        if (true) {
                                            if (string.IsNullOrEmpty(Link)) {
                                                Link = "unknown";
                                            } else {
                                                Link = "<a href=\"" + genericController.encodeHTML(Link) + "\" target=\"_blank\">" + Link + "</a>";
                                            }
                                            //
                                            // get approved status of the submitted record
                                            //
                                            Body += ("\n<tr>");
                                            //
                                            // Publish Checkbox
                                            //
                                            Body += ("<td align=\"center\" valign=\"top\" " + RowColor + ">" + core.html.inputCheckbox("row" + RecordCount, false) + core.html.inputHidden("rowid" + RecordCount, RecordID) + core.html.inputHidden("rowcontentname" + RecordCount, ContentName) + "</td>");
                                            //
                                            // Submitted
                                            //
                                            if (IsSubmitted) {
                                                Copy = "yes";
                                            } else {
                                                Copy = "no";
                                            }
                                            Body += ("<td align=\"center\" valign=\"top\" " + RowColor + " class=\"ccAdminSmall\">" + Copy + "</td>");
                                            //
                                            // Approved
                                            //
                                            if (IsApproved) {
                                                Copy = "yes";
                                            } else {
                                                Copy = "no";
                                            }
                                            Body += ("<td align=\"center\" valign=\"top\" " + RowColor + " class=\"ccAdminSmall\">" + Copy + "</td>");
                                            //
                                            // Edit
                                            //
                                            Body = Body + "<td align=\"left\" valign=\"top\" " + RowColor + " class=\"ccAdminSmall\">"
                                                + "<a href=\"?" + RequestNameAdminForm + "=" + AdminFormEdit + "&cid=" + ContentID + "&id=" + RecordID + "&" + RequestNameAdminDepth + "=1\">Edit</a>"
                                                + "</td>";
                                            //
                                            // Name
                                            //
                                            Body += ("<td align=\"left\" valign=\"top\" " + RowColor + " class=\"ccAdminSmall\"  style=\"white-space:nowrap;\">" + RecordName + "</td>");
                                            //
                                            // Content
                                            //
                                            Body += ("<td align=\"left\" valign=\"top\" " + RowColor + " class=\"ccAdminSmall\">" + ContentName + "</td>");
                                            //
                                            // RecordID
                                            //
                                            Body += ("<td align=\"left\" valign=\"top\" " + RowColor + " class=\"ccAdminSmall\">" + RecordID + "</td>");
                                            //
                                            // Public
                                            //
                                            if (IsInserted) {
                                                Link = Link + "*";
                                            } else if (IsDeleted) {
                                                Link = Link + "**";
                                            }
                                            Body += ("<td align=\"left\" valign=\"top\" " + RowColor + " class=\"ccAdminSmall\" style=\"white-space:nowrap;\">" + Link + "</td>");
                                            //
                                            // Description
                                            //
                                            //Call core.app.closeCS(CSLink)
                                            Body += ("<td align=\"left\" valign=\"top\" " + RowColor + ">" + SpanClassAdminNormal);
                                            //
                                            //If RecordName <> "" Then
                                            //    Body &=  (core.htmldoc.main_encodeHTML(RecordName) & ", ")
                                            //End If
                                            //Body &=  ("Content: " & ContentName & ", RecordID: " & RecordID & "" & br & "")
                                            if (ModifiedDate == DateTime.MinValue) {
                                                ModifiedDateString = "unknown";
                                            } else {
                                                ModifiedDateString = encodeText(ModifiedDate);
                                            }
                                            if (string.IsNullOrEmpty(ModifiedName)) {
                                                ModifiedName = "unknown";
                                            }
                                            if (string.IsNullOrEmpty(SubmitName)) {
                                                SubmitName = "unknown";
                                            }
                                            if (string.IsNullOrEmpty(ApprovedName)) {
                                                ApprovedName = "unknown";
                                            }
                                            if (IsInserted) {
                                                Body += ("Added: " + ModifiedDateString + " by " + ModifiedName + "" + BR + "");
                                            } else if (IsDeleted) {
                                                Body += ("Deleted: " + ModifiedDateString + " by " + ModifiedName + "" + BR + "");
                                            } else {
                                                Body += ("Modified: " + ModifiedDateString + " by " + ModifiedName + "" + BR + "");
                                            }
                                            if (IsSubmitted) {
                                                if (SubmittedDate == DateTime.MinValue) {
                                                    SubmittedDateString = "unknown";
                                                } else {
                                                    SubmittedDateString = encodeText(SubmittedDate);
                                                }
                                                Body += ("Submitted: " + SubmittedDateString + " by " + SubmitName + "" + BR + "");
                                            }
                                            if (IsApproved) {
                                                if (ApprovedDate == DateTime.MinValue) {
                                                    ApprovedDateString = "unknown";
                                                } else {
                                                    ApprovedDateString = encodeText(ApprovedDate);
                                                }
                                                Body += ("Approved: " + ApprovedDate + " by " + ApprovedName + "" + BR + "");
                                            }
                                            //Body &=  ("Admin Site: <a href=""?" & RequestNameAdminForm & "=" & AdminFormEdit & "&cid=" & ContentID & "&id=" & RecordID & "&" & RequestNameAdminDepth & "=1"" target=""_blank"">Open in New Window</a>" & br & "")
                                            //Body &=  ("Public Site: " & Link & "" & br & "")
                                            //
                                            Body += ("</td>");
                                            //
                                            Body += ("\n</tr>");
                                            RecordCount = RecordCount + 1;
                                        }
                                    }
                                    core.db.csClose(ref CSAuthoringRecord);
                                }
                            }
                            core.db.csGoNext(CS);
                        }
                        //
                        // --- print out the stuff at the bottom
                        //
                        RecordNext = RecordTop;
                        if (core.db.csOk(CS)) {
                            RecordNext = RecordCount;
                        }
                        RecordPrevious = RecordTop - RecordsPerPage;
                        if (RecordPrevious < 0) {
                            RecordPrevious = 0;
                        }
                    }
                    core.db.csClose(ref CS);
                    if (RecordCount == 0) {
                        //
                        // No records printed
                        //
                        Body += "\r<tr><td width=\"100%\" colspan=\"9\" class=\"ccAdminSmall\" style=\"padding-top:10px;\">There are no modified records to review</td></tr>";
                    } else {
                        Body += "\r<tr><td width=\"100%\" colspan=\"9\" class=\"ccAdminSmall\" style=\"padding-top:10px;\">* To view these records on the public site you must enable Rendering Mode because they are new records that have not been published.</td></tr>";
                        Body += "\r<tr><td width=\"100%\" colspan=\"9\" class=\"ccAdminSmall\">** To view these records on the public site you must disable Rendering Mode because they are deleted records that have not been published.</td></tr>";
                    }
                    Body += "\r</table>";
                    Body += core.html.inputHidden("RowCnt", RecordCount);
                    Body = "<div style=\"Background-color:white;\">" + Body + "</div>";
                    //
                    // Headers, etc
                    //
                    ButtonList = "";
                    if (MenuDepth > 0) {
                        ButtonList = ButtonList + "," + ButtonClose;
                    } else {
                        ButtonList = ButtonList + "," + ButtonCancel;
                    }
                    //ButtonList = ButtonList & "," & ButtonWorkflowPublishApproved & "," & ButtonWorkflowPublishSelected
                    ButtonList = ButtonList.Substring(1);
                    //
                    // Assemble Page
                    //
                    Body += core.html.inputHidden(rnAdminSourceForm, AdminFormPublishing);
                }
                //
                Caption = SpanClassAdminNormal + "<strong>Workflow Publishing</strong></span>";
                Description = "Monitor and Approve Workflow Publishing Changes";
                if (RecordCount >= 100) {
                    Description = Description + BR + BR + "Only the first 100 record are displayed";
                }
                tempGetForm_Publish = Adminui.GetBody(Caption, ButtonList, "", true, true, Description, "", 0, Body);
                core.html.addTitle("Workflow Publishing");
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Publish;
        }
        //
        //========================================================================
        //   Generate the content of a tab in the Edit Screen
        //========================================================================
        //
        private string GetForm_Edit_Tab(cdefModel adminContent, editRecordClass editRecord, int RecordID, int ContentID, bool ForceReadOnly, bool IsLandingPage, bool IsRootPage, string EditTab, csv_contentTypeEnum EditorContext, ref string return_NewFieldList, int TemplateIDForStyles, int HelpCnt, int[] HelpIDCache, string[] helpDefaultCache, string[] HelpCustomCache, bool AllowHelpMsgCustom, keyPtrController helpIdIndex, string[] fieldTypeDefaultEditors, string fieldEditorPreferenceList, string styleList, string styleOptionList, int emailIdForStyles, bool IsTemplateTable, string editorAddonListJSON) {
            string returnHtml = "";
            try {
                //
                string AjaxQS = null;
                string fancyBoxLinkId = null;
                string fancyBoxContentId = null;
                int fieldTypeDefaultEditorAddonId = 0;
                int fieldIdPos = 0;
                int Pos = 0;
                int editorAddonID = 0;
                bool editorReadOnly = false;
                bool AllowHelpIcon = false;
                int fieldId = 0;
                string LcaseName = null;
                bool IsEmptyList = false;
                string HelpMsgCustom = null;
                string HelpMsgDefault = null;
                //
                DateTime FieldValueDate = default(DateTime);
                string WhyReadOnlyMsg = null;
                bool IsLongHelp = false;
                bool IsEmptyHelp = false;
                string HelpMsg = null;
                int CS = 0;
                string EditorStyleModifier = null;
                string HelpClosedContentID = null;
                bool AllowHelpRow = false;
                string EditorHelp = null;
                string HelpEditorID = null;
                string HelpOpenedReadID = null;
                string HelpOpenedEditID = null;
                string HelpClosedID = null;
                string HelpID = null;
                string HelpMsgClosed = null;
                string HelpMsgOpenedRead = null;
                string HelpMsgOpenedEdit = null;
                string RecordName = null;
                string GroupName = null;
                bool IsBaseField = false;
                bool FieldReadOnly = false;
                string NonEncodedLink = null;
                string EncodedLink = null;
                string Caption = null;
                string[] lookups = null;
                int CSPointer = 0;
                string FieldName = null;
                string FieldValueText = null;
                int FieldValueInteger = 0;
                double FieldValueNumber = 0;
                bool FieldValueBoolean = false;
                int fieldTypeId = 0;
                object FieldValueObject = null;
                bool FieldPreferenceHTML = false;
                int CSLookup = 0;
                string RedirectPath = null;
                string LookupContentName = null;
                stringBuilderLegacyController s = new stringBuilderLegacyController();
                bool RecordReadOnly = false;
                string FormFieldLCaseName = null;
                int FieldRows = 0;
                string EditorString = null;
                string MTMContent0 = null;
                string MTMContent1 = null;
                string MTMRuleContent = null;
                string MTMRuleField0 = null;
                string MTMRuleField1 = null;
                string AlphaSort = null;
                adminUIController Adminui = new adminUIController(core);
                bool needUniqueEmailMessage = false;
                //
                // ----- Open the panel
                //
                if (adminContent.fields.Count <= 0) {
                    //
                    // There are no visible fiels, return empty
                    //
                    core.handleException(new ApplicationException("The content definition for this record is invalid. It contains no valid fields."));
                } else {
                    RecordReadOnly = ForceReadOnly;
                    //
                    // ----- Build an index to sort the fields by EditSortOrder
                    //
                    Dictionary<string, cdefFieldModel> sortingFields = new Dictionary<string, cdefFieldModel>();
                    //
                    foreach (var keyValuePair in adminContent.fields) {
                        cdefFieldModel field = keyValuePair.Value;
                        if (field.editTabName.ToLower() == EditTab.ToLower()) {
                            if (IsVisibleUserField(field.adminOnly, field.developerOnly, field.active, field.authorable, field.nameLc, adminContent.ContentTableName)) {
                                AlphaSort = genericController.GetIntegerString(field.editSortPriority, 10) + "-" + genericController.GetIntegerString(field.id, 10);
                                sortingFields.Add(AlphaSort, field);
                            }
                        }
                    }
                    //
                    // ----- display the record fields
                    //
                    AllowHelpIcon = core.visitProperty.getBoolean("AllowHelpIcon");
                    foreach (var kvp in sortingFields) {
                        cdefFieldModel field = kvp.Value;
                        fieldId = field.id;
                        WhyReadOnlyMsg = "";
                        FieldName = field.nameLc;
                        FormFieldLCaseName = genericController.vbLCase(FieldName);
                        fieldTypeId = field.fieldTypeId;
                        FieldValueObject = editRecord.fieldsLc[field.nameLc].value;
                        FieldValueText = genericController.encodeText(FieldValueObject);
                        FieldRows = 1;
                        FieldPreferenceHTML = field.htmlContent;
                        //
                        Caption = field.caption;
                        if (field.uniqueName) {
                            Caption = "&nbsp;**" + Caption;
                        } else {
                            if (field.nameLc.ToLower() == "email") {
                                if ((adminContent.ContentTableName.ToLower() == "ccmembers") && ((core.siteProperties.getBoolean("allowemaillogin", false)))) {
                                    Caption = "&nbsp;***" + Caption;
                                    needUniqueEmailMessage = true;
                                }
                            }
                        }
                        if (field.required) {
                            Caption = "&nbsp;*" + Caption;
                        }
                        IsBaseField = field.blockAccess; // field renamed
                        FormInputCount = FormInputCount + 1;
                        FieldReadOnly = false;
                        //
                        // Read only Special Cases
                        //
                        if (IsLandingPage) {
                            switch (genericController.vbLCase(field.nameLc)) {
                                case "active":
                                    //
                                    // if active, it is read only -- if inactive, let them set it active.
                                    //
                                    FieldReadOnly = (genericController.encodeBoolean(FieldValueObject));
                                    if (FieldReadOnly) {
                                        WhyReadOnlyMsg = "&nbsp;(disabled because you can not mark the landing page inactive)";
                                    }
                                    break;
                                case "dateexpires":
                                case "pubdate":
                                case "datearchive":
                                case "blocksection":
                                case "hidemenu":
                                    //
                                    // These fields are read only on landing pages
                                    //
                                    FieldReadOnly = true;
                                    WhyReadOnlyMsg = "&nbsp;(disabled for the landing page)";
                                    break;
                            }
                        }
                        //
                        if (IsRootPage) {
                            switch (genericController.vbLCase(field.nameLc)) {
                                case "dateexpires":
                                case "pubdate":
                                case "datearchive":
                                case "archiveparentid":
                                    FieldReadOnly = true;
                                    WhyReadOnlyMsg = "&nbsp;(disabled for root pages)";
                                    break;
                                case "allowinmenus":
                                case "allowinchildlists":
                                    FieldValueBoolean = true;
                                    FieldValueObject = "1";
                                    FieldReadOnly = true;
                                    WhyReadOnlyMsg = "&nbsp;(disabled for root pages)";
                                    break;
                            }
                        }
                        //
                        // Special Case - ccemail table Alloweid should be disabled if siteproperty AllowLinkLogin is false
                        //
                        if (genericController.vbLCase(adminContent.ContentTableName) == "ccemail" && genericController.vbLCase(FieldName) == "allowlinkeid") {
                            if (!(core.siteProperties.getBoolean("AllowLinkLogin", true))) {
                                //.ValueVariant = "0"
                                FieldValueObject = "0";
                                FieldReadOnly = true;
                                FieldValueBoolean = false;
                                FieldValueText = "0";
                            }
                        }
                        EditorStyleModifier = genericController.vbLCase(core.db.getFieldTypeNameFromFieldTypeId(fieldTypeId));
                        EditorString = "";
                        editorReadOnly = (RecordReadOnly || field.readOnly | (editRecord.id != 0 & field.notEditable) || (FieldReadOnly));
                        //
                        // Determine the editor: Contensive editor, field type default, or add-on preference
                        //
                        editorAddonID = 0;
                        //editorPreferenceAddonId = 0
                        fieldIdPos = genericController.vbInstr(1, "," + fieldEditorPreferenceList, "," + fieldId.ToString() + ":");
                        while ((editorAddonID == 0) && (fieldIdPos > 0)) {
                            fieldIdPos = fieldIdPos + 1 + fieldId.ToString().Length;
                            Pos = genericController.vbInstr(fieldIdPos, fieldEditorPreferenceList + ",", ",");
                            if (Pos > 0) {
                                editorAddonID = genericController.encodeInteger(fieldEditorPreferenceList.Substring(fieldIdPos - 1, Pos - fieldIdPos));
                                //editorPreferenceAddonId = genericController.EncodeInteger(Mid(fieldEditorPreferenceList, fieldIdPos, Pos - fieldIdPos))
                                //editorAddonID = editorPreferenceAddonId
                            }
                            fieldIdPos = genericController.vbInstr(fieldIdPos + 1, "," + fieldEditorPreferenceList, "," + fieldId.ToString() + ":");
                        }
                        if (editorAddonID == 0) {
                            fieldTypeDefaultEditorAddonId = genericController.encodeInteger(fieldTypeDefaultEditors[fieldTypeId]);
                            editorAddonID = fieldTypeDefaultEditorAddonId;
                        }
                        bool useEditorAddon = false;
                        if (editorAddonID != 0) {
                            //
                            //--------------------------------------------------------------------------------------------
                            // ----- Custom Editor
                            //--------------------------------------------------------------------------------------------
                            //
                            // generate the style list on demand
                            // note: &editorFieldType should be deprecated
                            //
                            core.docProperties.setProperty("editorName", FormFieldLCaseName);
                            core.docProperties.setProperty("editorValue", FieldValueText);
                            core.docProperties.setProperty("editorFieldId", fieldId);
                            core.docProperties.setProperty("editorFieldType", fieldTypeId);
                            core.docProperties.setProperty("editorReadOnly", editorReadOnly);
                            core.docProperties.setProperty("editorWidth", "");
                            core.docProperties.setProperty("editorHeight", "");

                            //addonOptionString = "" _
                            //    & "editorName=" & genericController.encodeNvaArgument(FormFieldLCaseName) _
                            //    & "&editorValue=" & genericController.encodeNvaArgument(FieldValueText) _
                            //    & "&editorFieldId=" & fieldId _
                            //    & "&editorFieldType=" & fieldTypeId _
                            //    & "&editorReadOnly=" & editorReadOnly _
                            //    & "&editorWidth=" _
                            //    & "&editorHeight=" _
                            //    & ""
                            if (genericController.encodeBoolean((fieldTypeId == FieldTypeIdHTML) || (fieldTypeId == FieldTypeIdFileHTML))) {
                                //
                                // include html related arguments
                                //
                                core.docProperties.setProperty("editorAllowActiveContent", "1");
                                core.docProperties.setProperty("editorAddonList", editorAddonListJSON);
                                core.docProperties.setProperty("editorStyles", styleList);
                                core.docProperties.setProperty("editorStyleOptions", styleOptionList);
                                //                            ac = New innovaEditorAddonClassFPO
                                //                            Call ac.Init()
                                //                            editorAddonListJSON = ac.GetEditorAddonListJSON(IsTemplateTable, EditorContext)

                                //addonOptionString = addonOptionString _
                                //    & "&editorAllowActiveContent=1" _
                                //    & "&editorAddonList=" & genericController.encodeNvaArgument(editorAddonListJSON) _
                                //    & "&editorStyles=" & genericController.encodeNvaArgument(styleList) _
                                //    & "&editorStyleOptions=" & genericController.encodeNvaArgument(styleOptionList) _
                                //    & ""
                            }
                            EditorString = core.addon.execute(addonModel.create(core, editorAddonID), new BaseClasses.CPUtilsBaseClass.addonExecuteContext {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextEditor,
                                errorCaption = "field editor id:" + editorAddonID
                            });
                            //EditorString = core.addon.execute_legacy6(editorAddonID, "", addonOptionString, Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextEditor, "", 0, "", "", False, 0, "", False, Nothing, "", Nothing, "", 0, False)
                            useEditorAddon = !string.IsNullOrEmpty(EditorString);
                            if (useEditorAddon) {
                                //
                                // -- editor worked
                                return_NewFieldList = return_NewFieldList + "," + FieldName;
                            } else {
                                //
                                // -- editor failed, determine if it is missing (or inactive). If missing, remove it from the members preferences
                                string SQL = "select id from ccaggregatefunctions where id=" + editorAddonID;
                                CS = core.db.csOpenSql(SQL);
                                if (!core.db.csOk(CS)) {
                                    //
                                    // -- missing, not just inactive
                                    EditorString = "";
                                    //
                                    // load user's editor preferences to fieldEditorPreferences() - this is the editor this user has picked when there are >1
                                    //   fieldId:addonId,fieldId:addonId,etc
                                    //   with custom FancyBox form in edit window with button "set editor preference"
                                    //   this button causes a 'refresh' action, reloads fields with stream without save
                                    //
                                    string tmpList = core.userProperty.getText("editorPreferencesForContent:" + adminContent.Id, "");
                                    int PosStart = genericController.vbInstr(1, "," + tmpList, "," + fieldId + ":");
                                    if (PosStart > 0) {
                                        int PosEnd = genericController.vbInstr(PosStart + 1, "," + tmpList, ",");
                                        if (PosEnd == 0) {
                                            tmpList = tmpList.Left(PosStart - 1);
                                        } else {
                                            tmpList = tmpList.Left(PosStart - 1) + tmpList.Substring(PosEnd - 1);
                                        }
                                        core.userProperty.setProperty("editorPreferencesForContent:" + adminContent.Id, tmpList);
                                    }
                                }
                                core.db.csClose(ref CS);
                            }
                        }
                        if (!useEditorAddon) {
                            //
                            // if custom editor not used or if it failed
                            //
                            if (fieldTypeId == FieldTypeIdRedirect) {
                                //ElseIf (FieldType = FieldTypeRedirect) Then
                                //
                                //--------------------------------------------------------------------------------------------
                                // ----- Default Editor, Redirect fields (the same for normal/readonly/spelling)
                                //--------------------------------------------------------------------------------------------
                                //
                                RedirectPath = core.appConfig.adminRoute;
                                if (field.redirectPath != "") {
                                    RedirectPath = field.redirectPath;
                                }
                                RedirectPath = RedirectPath + "?" + RequestNameTitleExtension + "=" + genericController.EncodeRequestVariable(" For " + editRecord.nameLc + TitleExtension) + "&" + RequestNameAdminDepth + "=" + (MenuDepth + 1) + "&wl0=" + field.redirectID + "&wr0=" + editRecord.id;
                                if (field.redirectContentID != 0) {
                                    RedirectPath = RedirectPath + "&cid=" + field.redirectContentID;
                                } else {
                                    RedirectPath = RedirectPath + "&cid=" + editRecord.contentControlId;
                                }
                                if (editRecord.id == 0) {
                                    EditorString += ("[available after save]");
                                } else {
                                    RedirectPath = genericController.vbReplace(RedirectPath, "'", "\\'");
                                    EditorString += ("<a href=\"#\"");
                                    EditorString += (" onclick=\" window.open('" + RedirectPath + "', '_blank', 'scrollbars=yes,toolbar=no,status=no,resizable=yes'); return false;\"");
                                    EditorString += (">");
                                    EditorString += ("Open in New Window</A>");
                                }
                                //s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & "&nbsp;</span></nobr></td>")
                            } else if (editorReadOnly) {
                                //
                                //--------------------------------------------------------------------------------------------
                                // ----- Display fields as read only
                                //--------------------------------------------------------------------------------------------
                                //
                                if (!string.IsNullOrEmpty(WhyReadOnlyMsg)) {
                                    WhyReadOnlyMsg = "<span class=\"ccDisabledReason\">" + WhyReadOnlyMsg + "</span>";
                                }
                                EditorStyleModifier = "";
                                switch (fieldTypeId) {
                                    case FieldTypeIdBoolean:
                                        //
                                        // ----- Boolean ReadOnly
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueBoolean = genericController.encodeBoolean(FieldValueObject);
                                        EditorString += (core.html.inputHidden(FormFieldLCaseName, genericController.encodeText(FieldValueBoolean)));
                                        EditorString += (core.html.inputCheckbox(FormFieldLCaseName, FieldValueBoolean, "", true, "checkBox"));
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    case FieldTypeIdFile:
                                    case FieldTypeIdFileImage:
                                        //
                                        // ----- File ReadOnly
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        NonEncodedLink = core.webServer.requestDomain + genericController.getCdnFileLink(core, FieldValueText);
                                        EncodedLink = genericController.EncodeURL(NonEncodedLink);
                                        EditorString += (core.html.inputHidden(FormFieldLCaseName, ""));
                                        if (string.IsNullOrEmpty(FieldValueText)) {
                                            EditorString += ("[no file]");
                                        } else {
                                            string filename = "";
                                            string path = "";
                                            core.cdnFiles.splitPathFilename(FieldValueText, ref path, ref filename);
                                            EditorString += ("&nbsp;<a href=\"http://" + EncodedLink + "\" target=\"_blank\">" + SpanClassAdminSmall + "[" + filename + "]</A>");
                                        }
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    case FieldTypeIdLookup:
                                        //
                                        // ----- Lookup ReadOnly
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueInteger = genericController.encodeInteger(FieldValueObject);
                                        EditorString += (core.html.inputHidden(FormFieldLCaseName, genericController.encodeText(FieldValueInteger)));
                                        //Call s.Add("<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                        LookupContentName = "";
                                        if (field.lookupContentID != 0) {
                                            LookupContentName = genericController.encodeText(cdefModel.getContentNameByID(core, field.lookupContentID));
                                        }
                                        if (!string.IsNullOrEmpty(LookupContentName)) {
                                            CSLookup = core.db.cs_open2(LookupContentName, FieldValueInteger, false, false, "Name,ContentControlID");
                                            if (core.db.csOk(CSLookup)) {
                                                if (core.db.csGet(CSLookup, "Name") == "") {
                                                    EditorString += ("No Name");
                                                } else {
                                                    EditorString += (core.html.encodeHTML(core.db.csGet(CSLookup, "Name")));
                                                }
                                                EditorString += ("&nbsp;[<a TabIndex=-1 href=\"?" + RequestNameAdminForm + "=4&cid=" + field.lookupContentID + "&id=" + FieldValueObject.ToString() + "\" target=\"_blank\">View details in new window</a>]");
                                            } else {
                                                EditorString += ("None");
                                            }
                                            core.db.csClose(ref CSLookup);
                                            EditorString += ("&nbsp;[<a TabIndex=-1 href=\"?cid=" + field.lookupContentID + "\" target=\"_blank\">See all " + LookupContentName + "</a>]");
                                        } else if (field.lookupList != "") {
                                            lookups = field.lookupList.Split(',');
                                            if (FieldValueInteger < 1) {
                                                EditorString += ("None");
                                            } else if (FieldValueInteger > (lookups.GetUpperBound(0) + 1)) {
                                                EditorString += ("None");
                                            } else {
                                                EditorString += (lookups[FieldValueInteger - 1]);
                                            }
                                        }
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    case FieldTypeIdMemberSelect:
                                        //
                                        // ----- Member Select ReadOnly
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueInteger = genericController.encodeInteger(FieldValueObject);
                                        EditorString += (core.html.inputHidden(FormFieldLCaseName, genericController.encodeText(FieldValueInteger)));
                                        if (FieldValueInteger == 0) {
                                            EditorString += ("None");
                                        } else {
                                            RecordName = core.db.getRecordName("people", FieldValueInteger);
                                            if (string.IsNullOrEmpty(RecordName)) {
                                                EditorString += ("No Name");
                                            } else {
                                                EditorString += (core.html.encodeHTML(RecordName));
                                            }
                                            EditorString += ("&nbsp;[<a TabIndex=-1 href=\"?af=4&cid=" + cdefModel.getContentId(core, "people") + "&id=" + FieldValueObject.ToString() + "\" target=\"_blank\">View details in new window</a>]");
                                        }
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    case FieldTypeIdManyToMany:
                                        //
                                        //   Placeholder
                                        //
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        MTMContent0 = cdefModel.getContentNameByID(core, field.contentId);
                                        MTMContent1 = cdefModel.getContentNameByID(core, field.manyToManyContentID);
                                        MTMRuleContent = cdefModel.getContentNameByID(core, field.manyToManyRuleContentID);
                                        MTMRuleField0 = field.ManyToManyRulePrimaryField;
                                        MTMRuleField1 = field.ManyToManyRuleSecondaryField;
                                        EditorString += core.html.getCheckList("ManyToMany" + field.id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1);
                                        //EditorString &= (core.html.getInputCheckListCategories("ManyToMany" & .id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , True, MTMContent1))
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    case FieldTypeIdCurrency:
                                        //
                                        // ----- Currency ReadOnly
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueNumber = genericController.encodeNumber(FieldValueObject);
                                        EditorString += (core.html.inputHidden(FormFieldLCaseName, genericController.encodeText(FieldValueNumber)));
                                        EditorString += (core.html.inputText(FormFieldLCaseName, FieldValueNumber.ToString(), -1, -1, "", false, true, "text"));
                                        EditorString += (string.Format("{0:C}", FieldValueNumber));
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    case FieldTypeIdDate:
                                        //
                                        // ----- date
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueDate = genericController.encodeDateMinValue(genericController.encodeDate(FieldValueObject));
                                        if (FieldValueDate == DateTime.MinValue) {
                                            FieldValueText = "";
                                        } else {
                                            FieldValueText = encodeText(FieldValueDate);
                                        }
                                        EditorString += (core.html.inputHidden(FormFieldLCaseName, FieldValueText));
                                        EditorString += (core.html.inputText(FormFieldLCaseName, FieldValueText, -1, -1, "", false, true, "date"));
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    case FieldTypeIdAutoIdIncrement:
                                    case FieldTypeIdFloat:
                                    case FieldTypeIdInteger:
                                        //
                                        // ----- number
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        EditorString += (core.html.inputHidden(FormFieldLCaseName, FieldValueText));
                                        EditorString += (core.html.inputText(FormFieldLCaseName, FieldValueText, -1, -1, "", false, true, "number"));
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    case FieldTypeIdHTML:
                                    case FieldTypeIdFileHTML:
                                        //
                                        // ----- HTML types
                                        //
                                        if (field.htmlContent) {
                                            //
                                            // edit html as html (see the code)
                                            //
                                            return_NewFieldList = return_NewFieldList + "," + FieldName;
                                            FieldValueText = genericController.encodeText(FieldValueObject);
                                            EditorString += core.html.inputHidden(FormFieldLCaseName, FieldValueText);
                                            EditorStyleModifier = "textexpandable";
                                            FieldRows = (core.userProperty.getInteger(adminContent.Name + "." + FieldName + ".RowHeight", 10));
                                            EditorString += core.html.inputTextExpandable(FormFieldLCaseName, FieldValueText, FieldRows, "", FormFieldLCaseName, false, true);
                                        } else {
                                            //
                                            // edit html as wysiwyg
                                            //
                                            return_NewFieldList = return_NewFieldList + "," + FieldName;
                                            FieldValueText = genericController.encodeText(FieldValueObject);
                                            EditorString += core.html.inputHidden(FormFieldLCaseName, FieldValueText);
                                            //
                                            EditorStyleModifier = "text";
                                            FieldRows = (core.userProperty.getInteger(adminContent.Name + "." + FieldName + ".PixelHeight", 500));
                                            //EditorString &=  core.main_GetFormInputHTML(FormFieldLCaseName, FieldValueText)
                                            //
                                            EditorString += core.html.getFormInputHTML(FormFieldLCaseName, FieldValueText, "500", "", true, true, editorAddonListJSON, styleList, styleOptionList);
                                            //innovaEditor = New innovaEditorAddonClassFPO
                                            //EditorString &=  innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, True, TemplateIDForStyles, emailIdForStyles)
                                            EditorString = "<div style=\"width:95%\">" + EditorString + "</div>";
                                        }
                                        break;
                                    case FieldTypeIdText:
                                    case FieldTypeIdLink:
                                    case FieldTypeIdResourceLink:
                                        //
                                        // ----- FieldTypeText
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        EditorString += core.html.inputHidden(FormFieldLCaseName, FieldValueText);
                                        if (field.password) {
                                            //
                                            // Password forces simple text box
                                            //
                                            EditorString += core.html.inputText(FormFieldLCaseName, "*****", 0, 0, "", true, true, "password");
                                        } else {
                                            //
                                            // non-password
                                            //
                                            EditorString += core.html.inputText(FormFieldLCaseName, FieldValueText, 1, 0, "", false, true, "text");
                                        }
                                        break;
                                    case FieldTypeIdLongText:
                                    case FieldTypeIdFileText:
                                        //
                                        // ----- LongText, TextFile
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        EditorString += core.html.inputHidden(FormFieldLCaseName, FieldValueText);
                                        EditorStyleModifier = "textexpandable";
                                        FieldRows = (core.userProperty.getInteger(adminContent.Name + "." + FieldName + ".RowHeight", 10));
                                        EditorString += core.html.inputTextExpandable(FormFieldLCaseName, FieldValueText, FieldRows, "", FormFieldLCaseName, false, true);
                                        break;
                                    default:
                                        //
                                        // ----- Legacy text type -- not used unless something was missed
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        EditorString += core.html.inputHidden(FormFieldLCaseName, FieldValueText);
                                        if (field.password) {
                                            //
                                            // Password forces simple text box
                                            //
                                            EditorString += core.html.inputText(FormFieldLCaseName, "*****", 0, 0, "", true, true, "password");
                                        } else if (!field.htmlContent) {
                                            //
                                            // not HTML capable, textarea with resizing
                                            //
                                            if ((fieldTypeId == FieldTypeIdText) && (FieldValueText.IndexOf("\n") == -1) && (FieldValueText.Length < 40)) {
                                                //
                                                // text field shorter then 40 characters without a CR
                                                //
                                                EditorString += core.html.inputText(FormFieldLCaseName, FieldValueText, 1, 0, "", false, true, "text");
                                            } else {
                                                //
                                                // longer text data, or text that contains a CR
                                                //
                                                EditorStyleModifier = "textexpandable";
                                                EditorString += core.html.inputTextExpandable(FormFieldLCaseName, FieldValueText, 10, "", "", false, true);
                                            }
                                        } else if (field.htmlContent && FieldPreferenceHTML) {
                                            //
                                            // HTMLContent true, and prefered
                                            //
                                            EditorStyleModifier = "text";
                                            FieldRows = (core.userProperty.getInteger(adminContent.Name + "." + FieldName + ".PixelHeight", 500));
                                            EditorString += core.html.getFormInputHTML(FormFieldLCaseName, FieldValueText, "500", "", false, true, editorAddonListJSON, styleList, styleOptionList);
                                            //innovaEditor = New innovaEditorAddonClassFPO
                                            //EditorString &=  innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, True, TemplateIDForStyles, emailIdForStyles)
                                            EditorString = "<div style=\"width:95%\">" + EditorString + "</div>";
                                        } else {
                                            //
                                            // HTMLContent true, but text editor selected
                                            //
                                            EditorStyleModifier = "textexpandable";
                                            FieldRows = (core.userProperty.getInteger(adminContent.Name + "." + FieldName + ".RowHeight", 10));
                                            EditorString += core.html.inputTextExpandable(FormFieldLCaseName, FieldValueText, FieldRows, "100%", FormFieldLCaseName, false, true);
                                            //EditorString = core.main_GetFormInputTextExpandable(FormFieldLCaseName, encodeHTML(FieldValueText), FieldRows, "600px", FormFieldLCaseName, False)
                                        }
                                        break;
                                }
                            } else {
                                //
                                //--------------------------------------------------------------------------------------------
                                //   Not Read Only - Display fields as form elements to be modified
                                //--------------------------------------------------------------------------------------------
                                //
                                switch (fieldTypeId) {
                                    case FieldTypeIdBoolean:
                                        //
                                        // ----- Boolean
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueBoolean = genericController.encodeBoolean(FieldValueObject);
                                        //s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                        EditorString += (core.html.inputCheckbox(FormFieldLCaseName, FieldValueBoolean, "", false, "checkBox"));
                                        //s.Add( "&nbsp;</span></nobr></td>")
                                        //
                                        break;
                                    case FieldTypeIdFile:
                                    case FieldTypeIdFileImage:
                                        //
                                        // ----- File
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        //Call s.Add("<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                        if (string.IsNullOrEmpty(FieldValueText)) {
                                            EditorString += (core.html.inputFile(FormFieldLCaseName, "", "file"));
                                        } else {
                                            NonEncodedLink = core.webServer.requestDomain + genericController.getCdnFileLink(core, FieldValueText);
                                            EncodedLink = genericController.encodeHTML(NonEncodedLink);
                                            string filename = "";
                                            string path = "";
                                            core.cdnFiles.splitPathFilename(FieldValueText, ref path, ref filename);
                                            EditorString += ("&nbsp;<a href=\"http://" + EncodedLink + "\" target=\"_blank\">" + SpanClassAdminSmall + "[" + filename + "]</A>");
                                            EditorString += ("&nbsp;&nbsp;&nbsp;Delete:&nbsp;" + core.html.inputCheckbox(FormFieldLCaseName + ".DeleteFlag", false));
                                            EditorString += ("&nbsp;&nbsp;&nbsp;Change:&nbsp;" + core.html.inputFile(FormFieldLCaseName, "", "file"));
                                        }
                                        //
                                        break;
                                    case FieldTypeIdLookup:
                                        //
                                        // ----- Lookup
                                        //
                                        FieldValueInteger = genericController.encodeInteger(FieldValueObject);
                                        LookupContentName = "";
                                        if (field.lookupContentID != 0) {
                                            LookupContentName = genericController.encodeText(cdefModel.getContentNameByID(core, field.lookupContentID));
                                        }
                                        if (!string.IsNullOrEmpty(LookupContentName)) {
                                            return_NewFieldList = return_NewFieldList + "," + FieldName;
                                            if (!field.required) {
                                                EditorString += (core.html.selectFromContent(FormFieldLCaseName, FieldValueInteger, LookupContentName, "", "None", "", ref IsEmptyList, "select"));
                                            } else {
                                                EditorString += (core.html.selectFromContent(FormFieldLCaseName, FieldValueInteger, LookupContentName, "", "", "", ref IsEmptyList, "select"));
                                            }
                                            if (FieldValueInteger != 0) {
                                                CSPointer = core.db.cs_open2(LookupContentName, FieldValueInteger, false, false, "ID");
                                                if (core.db.csOk(CSPointer)) {
                                                    EditorString += ("&nbsp;[<a TabIndex=-1 href=\"?" + RequestNameAdminForm + "=4&cid=" + field.lookupContentID + "&id=" + FieldValueObject.ToString() + "\" target=\"_blank\">Details</a>]");
                                                }
                                                core.db.csClose(ref CSPointer);
                                            }
                                            EditorString += ("&nbsp;[<a TabIndex=-1 href=\"?cid=" + field.lookupContentID + "\" target=\"_blank\">See all " + LookupContentName + "</a>]");
                                        } else if (field.lookupList != "") {
                                            return_NewFieldList = return_NewFieldList + "," + FieldName;
                                            if (!field.required) {
                                                EditorString += core.html.selectFromList(FormFieldLCaseName, FieldValueInteger, field.lookupList, "Select One", "", "select");
                                            } else {
                                                EditorString += core.html.selectFromList(FormFieldLCaseName, FieldValueInteger, field.lookupList, "", "", "select");
                                            }
                                        } else {
                                            //
                                            // -- log exception but dont throw
                                            core.handleException(new ApplicationException("Field [" + FieldName + "] is a Lookup field, but no LookupContent or LookupList has been configured"));
                                            EditorString += "[Selection not configured]";
                                        }
                                        //
                                        break;
                                    case FieldTypeIdMemberSelect:
                                        //
                                        // ----- Member Select
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueInteger = genericController.encodeInteger(FieldValueObject);
                                        //s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                        if (!field.required) {
                                            EditorString += (core.html.selectUserFromGroup(FormFieldLCaseName, FieldValueInteger, field.memberSelectGroupId_get(core), "", "None", "select"));
                                        } else {
                                            EditorString += (core.html.selectUserFromGroup(FormFieldLCaseName, FieldValueInteger, field.memberSelectGroupId_get(core), "", "", "select"));
                                        }
                                        if (FieldValueInteger != 0) {
                                            CSPointer = core.db.cs_open2("people", FieldValueInteger, false, false, "ID");
                                            if (core.db.csOk(CSPointer)) {
                                                EditorString += ("&nbsp;[<a TabIndex=-1 href=\"?" + RequestNameAdminForm + "=4&cid=" + cdefModel.getContentId(core, "people") + "&id=" + FieldValueObject.ToString() + "\" target=\"_blank\">Details</a>]");
                                            }
                                            core.db.csClose(ref CSPointer);
                                        }
                                        GroupName = field.memberSelectGroupName_get(core);
                                        EditorString += ("&nbsp;[<a TabIndex=-1 href=\"?cid=" + cdefModel.getContentId(core, "groups") + "\" target=\"_blank\">Select from members of " + GroupName + "</a>]");
                                        //s.Add( "</span></nobr></td>")
                                        //
                                        break;
                                    case FieldTypeIdManyToMany:
                                        //
                                        //   Placeholder
                                        //
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        //Call s.Add("<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                        //
                                        MTMContent0 = cdefModel.getContentNameByID(core, field.contentId);
                                        MTMContent1 = cdefModel.getContentNameByID(core, field.manyToManyContentID);
                                        MTMRuleContent = cdefModel.getContentNameByID(core, field.manyToManyRuleContentID);
                                        MTMRuleField0 = field.ManyToManyRulePrimaryField;
                                        MTMRuleField1 = field.ManyToManyRuleSecondaryField;
                                        EditorString += core.html.getCheckList("ManyToMany" + field.id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, "", "", false, false, FieldValueText);
                                        //EditorString &= (core.html.getInputCheckListCategories("ManyToMany" & .id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , False, MTMContent1, FieldValueText))
                                        break;
                                    case FieldTypeIdDate:
                                        //
                                        // ----- Date
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueDate = genericController.encodeDateMinValue(genericController.encodeDate(FieldValueObject));
                                        if (FieldValueDate == DateTime.MinValue) {
                                            FieldValueText = "";
                                        } else {
                                            FieldValueText = encodeText(FieldValueDate);
                                        }
                                        EditorString += (core.html.inputDate(FormFieldLCaseName, FieldValueText));
                                        //s.Add( "&nbsp;</span></nobr></td>")
                                        break;
                                    case FieldTypeIdAutoIdIncrement:
                                    case FieldTypeIdCurrency:
                                    case FieldTypeIdFloat:
                                    case FieldTypeIdInteger:
                                        //
                                        // ----- Others that simply print
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        //s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                        if (field.password) {
                                            EditorString += (core.html.inputText(FormFieldLCaseName, FieldValueText, -1, -1, "", true, false, "password"));
                                        } else {
                                            if (string.IsNullOrEmpty(FieldValueText)) {
                                                EditorString += (core.html.inputText(FormFieldLCaseName, "", -1, -1, "", false, false, "text"));
                                            } else {
                                                if (encodeBoolean(FieldValueText.IndexOf("\n") + 1) || (FieldValueText.Length > 40)) {
                                                    EditorString += (core.html.inputText(FormFieldLCaseName, FieldValueText, -1, -1, "", false, false, "text"));
                                                } else {
                                                    EditorString += (core.html.inputText(FormFieldLCaseName, FieldValueText, 1, -1, "", false, false, "text"));
                                                }
                                            }
                                            //s.Add( "&nbsp;</span></nobr></td>")
                                        }
                                        //
                                        break;
                                    case FieldTypeIdLink:
                                        //
                                        // ----- Link (href value
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        EditorString = ""
                                            + core.html.inputText(FormFieldLCaseName, FieldValueText, 1, 80, FormFieldLCaseName, false, false, "link") + "&nbsp;<a href=\"#\" onClick=\"OpenResourceLinkWindow( '" + FormFieldLCaseName + "' ) ;return false;\"><img src=\"/ccLib/images/ResourceLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a resource\" title=\"Link to a resource\"></a>"
                                            + "&nbsp;<a href=\"#\" onClick=\"OpenSiteExplorerWindow( '" + FormFieldLCaseName + "' ) ;return false;\"><img src=\"/ccLib/images/PageLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a page\" title=\"Link to a page\"></a>";
                                        //s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & "</span></nobr></td>")
                                        break;
                                    case FieldTypeIdResourceLink:
                                        //
                                        // ----- Resource Link (src value)
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        EditorString = ""
                                            + core.html.inputText(FormFieldLCaseName, FieldValueText, 1, 80, FormFieldLCaseName, false, false, "resourceLink") + "&nbsp;<a href=\"#\" onClick=\"OpenResourceLinkWindow( '" + FormFieldLCaseName + "' ) ;return false;\"><img src=\"/ccLib/images/ResourceLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a resource\" title=\"Link to a resource\"></a>";
                                        //
                                        break;
                                    case FieldTypeIdText:
                                        //
                                        // ----- Text Type
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        if (field.password) {
                                            //
                                            // Password forces simple text box
                                            //
                                            EditorString = core.html.inputText(FormFieldLCaseName, FieldValueText, -1, -1, "", true, false, "password");
                                        } else {
                                            //
                                            // non-password
                                            //
                                            if ((FieldValueText.IndexOf("\n") == -1) && (FieldValueText.Length < 40)) {
                                                //
                                                // text field shorter then 40 characters without a CR
                                                //
                                                EditorString = core.html.inputText(FormFieldLCaseName, FieldValueText, 1, -1, "", false, false, "text");
                                            } else {
                                                //
                                                // longer text data, or text that contains a CR
                                                //
                                                EditorStyleModifier = "textexpandable";
                                                EditorString = core.html.inputTextExpandable(FormFieldLCaseName, FieldValueText, 10, "100%", "", false, false, "text");
                                            }
                                        }
                                        //
                                        break;
                                    case FieldTypeIdHTML:
                                    case FieldTypeIdFileHTML:
                                        //
                                        // content is html
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        //
                                        // 9/7/2012 -- added this to support:
                                        //   html fields types mean they hold html
                                        //   .htmlContent means edit it with text editor (so you edit the html)
                                        //
                                        if (field.htmlContent && FieldPreferenceHTML) {
                                            //
                                            // View the content as Html, not wysiwyg
                                            //
                                            EditorStyleModifier = "textexpandable";
                                            EditorString = core.html.inputTextExpandable(FormFieldLCaseName, FieldValueText, 10, "100%", "", false, false, "text");
                                        } else {
                                            //
                                            // wysiwyg editor
                                            //
                                            if (string.IsNullOrEmpty(FieldValueText)) {
                                                //
                                                // editor needs a starting p tag to setup correctly
                                                //
                                                FieldValueText = HTMLEditorDefaultCopyNoCr;
                                            }
                                            EditorStyleModifier = "htmleditor";
                                            FieldRows = (core.userProperty.getInteger(adminContent.Name + "." + FieldName + ".PixelHeight", 500));
                                            EditorString += core.html.getFormInputHTML(FormFieldLCaseName, FieldValueText, "500", "", false, true, editorAddonListJSON, styleList, styleOptionList);
                                            //innovaEditor = New innovaEditorAddonClassFPO
                                            //EditorString = innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, False, TemplateIDForStyles, emailIdForStyles)
                                            EditorString = "<div style=\"width:95%\">" + EditorString + "</div>";
                                        }
                                        //
                                        break;
                                    case FieldTypeIdLongText:
                                    case FieldTypeIdFileText:
                                        //
                                        // -- Long Text, use text editor
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        //
                                        EditorStyleModifier = "textexpandable";
                                        FieldRows = (core.userProperty.getInteger(adminContent.Name + "." + FieldName + ".RowHeight", 10));
                                        EditorString = core.html.inputTextExpandable(FormFieldLCaseName, FieldValueText, FieldRows, "100%", FormFieldLCaseName, false, false, "text");
                                        //
                                        break;
                                    case FieldTypeIdFileCSS:
                                        //
                                        // ----- CSS field
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        EditorStyleModifier = "textexpandable";
                                        FieldRows = (core.userProperty.getInteger(adminContent.Name + "." + FieldName + ".RowHeight", 10));
                                        EditorString = core.html.inputTextExpandable(FormFieldLCaseName, FieldValueText, 10, "100%", "", false, false, "styles");
                                        break;
                                    case FieldTypeIdFileJavascript:
                                        //
                                        // ----- Javascript field
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        EditorStyleModifier = "textexpandable";
                                        FieldRows = (core.userProperty.getInteger(adminContent.Name + "." + FieldName + ".RowHeight", 10));
                                        EditorString = core.html.inputTextExpandable(FormFieldLCaseName, FieldValueText, FieldRows, "100%", FormFieldLCaseName, false, false, "text");
                                        //
                                        break;
                                    case FieldTypeIdFileXML:
                                        //
                                        // ----- xml field
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        EditorStyleModifier = "textexpandable";
                                        FieldRows = (core.userProperty.getInteger(adminContent.Name + "." + FieldName + ".RowHeight", 10));
                                        EditorString = core.html.inputTextExpandable(FormFieldLCaseName, FieldValueText, FieldRows, "100%", FormFieldLCaseName, false, false, "text");
                                        //
                                        break;
                                    default:
                                        //
                                        // ----- Legacy text type -- not used unless something was missed
                                        //
                                        return_NewFieldList = return_NewFieldList + "," + FieldName;
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        if (field.password) {
                                            //
                                            // Password forces simple text box
                                            //
                                            EditorString = core.html.inputText(FormFieldLCaseName, FieldValueText, -1, -1, "", true, false, "password");
                                        } else if (!field.htmlContent) {
                                            //
                                            // not HTML capable, textarea with resizing
                                            //
                                            if ((fieldTypeId == FieldTypeIdText) && (FieldValueText.IndexOf("\n") == -1) && (FieldValueText.Length < 40)) {
                                                //
                                                // text field shorter then 40 characters without a CR
                                                //
                                                EditorString = core.html.inputText(FormFieldLCaseName, FieldValueText, 1, -1, "", false, false, "text");
                                            } else {
                                                //
                                                // longer text data, or text that contains a CR
                                                //
                                                EditorStyleModifier = "textexpandable";
                                                EditorString = core.html.inputTextExpandable(FormFieldLCaseName, FieldValueText, 10, "100%", "", false, false, "text");
                                            }
                                        } else if (field.htmlContent && FieldPreferenceHTML) {
                                            //
                                            // HTMLContent true, and prefered
                                            //
                                            if (string.IsNullOrEmpty(FieldValueText)) {
                                                //
                                                // editor needs a starting p tag to setup correctly
                                                //
                                                FieldValueText = HTMLEditorDefaultCopyNoCr;
                                            }
                                            EditorStyleModifier = "htmleditor";
                                            FieldRows = (core.userProperty.getInteger(adminContent.Name + "." + FieldName + ".PixelHeight", 500));
                                            EditorString += core.html.getFormInputHTML(FormFieldLCaseName, FieldValueText, "500", "", false, true, editorAddonListJSON, styleList, styleOptionList);
                                            //innovaEditor = New innovaEditorAddonClassFPO
                                            //EditorString = innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, False, TemplateIDForStyles, emailIdForStyles)
                                            EditorString = "<div style=\"width:95%\">" + EditorString + "</div>";
                                        } else {
                                            //
                                            // HTMLContent true, but text editor selected
                                            //
                                            EditorStyleModifier = "textexpandable";
                                            FieldRows = (core.userProperty.getInteger(adminContent.Name + "." + FieldName + ".RowHeight", 10));
                                            EditorString = core.html.inputTextExpandable(FormFieldLCaseName, genericController.encodeHTML(FieldValueText), FieldRows, "600px", FormFieldLCaseName, false, false, "text");
                                        }
                                        //s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & "</span></nobr></td>")
                                        break;
                                }
                            }
                        }
                        //
                        // Build Help Line Below editor
                        //
                        includeFancyBox = true;
                        HelpMsgDefault = "";
                        HelpMsgCustom = "";
                        EditorHelp = "";
                        LcaseName = genericController.vbLCase(field.nameLc);
                        if (AllowHelpMsgCustom) {
                            HelpMsgDefault = field.HelpDefault;
                            HelpMsgCustom = field.HelpCustom;
                            //HelpPtr = helpIdIndex.getPtr(CStr(.id))
                            //If HelpPtr >= 0 Then
                            //    FieldHelpFound = True
                            //    HelpMsgDefault = helpDefaultCache(HelpPtr)
                            //    HelpMsgCustom = HelpCustomCache(HelpPtr)
                            //End If
                        }
                        //
                        // 12/4/2016 - REFACTOR - this is from the old system. Delete this after we varify it is no longer needed
                        //
                        //If Not FieldHelpFound Then
                        //    Call getFieldHelpMsgs(adminContent.parentID, .nameLc, HelpMsgDefault, HelpMsgCustom)
                        //    CS = core.app.csInsertRecord("Content Field Help")
                        //    If core.app.csOk(CS) Then
                        //        Call core.app.setCS(CS, "fieldid", fieldId)
                        //        Call core.app.setCS(CS, "name", adminContent.Name & "." & .nameLc)
                        //        Call core.app.setCS(CS, "HelpDefault", HelpMsgDefault)
                        //        Call core.app.setCS(CS, "HelpCustom", HelpMsgCustom)
                        //    End If
                        //    Call core.app.csClose(ref CS)
                        //End If
                        if (!string.IsNullOrEmpty(HelpMsgCustom)) {
                            HelpMsg = HelpMsgCustom;
                        } else {
                            HelpMsg = HelpMsgDefault;
                        }
                        HelpMsgOpenedRead = HelpMsg;
                        HelpMsgClosed = HelpMsg;
                        IsEmptyHelp = HelpMsgClosed.Length == 0;
                        IsLongHelp = (HelpMsgClosed.Length > 100);
                        if (IsLongHelp) {
                            HelpMsgClosed = HelpMsgClosed.Left(100) + "...";
                        }
                        //
                        HelpID = "helpId" + fieldId;
                        HelpEditorID = "helpEditorId" + fieldId;
                        HelpOpenedReadID = "HelpOpenedReadID" + fieldId;
                        HelpOpenedEditID = "HelpOpenedEditID" + fieldId;
                        HelpClosedID = "helpClosedId" + fieldId;
                        HelpClosedContentID = "helpClosedContentId" + fieldId;
                        AllowHelpRow = true;
                        //
                        //------------------------------------------------------------------------------------------------------------
                        // editor preferences form - a fancybox popup that interfaces with a hardcoded ajax function in init() to set a member property
                        //------------------------------------------------------------------------------------------------------------
                        //
                        AjaxQS = RequestNameAjaxFunction + "=" + ajaxGetFieldEditorPreferenceForm + "&fieldid=" + fieldId + "&currentEditorAddonId=" + editorAddonID + "&fieldTypeDefaultEditorAddonId=" + fieldTypeDefaultEditorAddonId;
                        fancyBoxLinkId = "fbl" + fancyBoxPtr;
                        fancyBoxContentId = "fbc" + fancyBoxPtr;
                        fancyBoxHeadJS = fancyBoxHeadJS + "\r\njQuery('#" + fancyBoxLinkId + "').fancybox({"
                            + "'titleShow':false,"
                            + "'transitionIn':'elastic',"
                            + "'transitionOut':'elastic',"
                            + "'overlayOpacity':'.2',"
                            + "'overlayColor':'#000000',"
                            + "'onStart':function(){cj.ajax.qs('" + AjaxQS + "','','" + fancyBoxContentId + "')}"
                            + "});";
                        EditorHelp = EditorHelp + "\r<div style=\"float:right;\">"
                            + cr2 + "<a id=\"" + fancyBoxLinkId + "\" href=\"#" + fancyBoxContentId + "\" title=\"select an alternate editor for this field.\" tabindex=\"-1\"><img src=\"/ccLib/images/NavAltEditor.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"select an alternate editor for this field.\"></a>"
                            + cr2 + "<div style=\"display:none;\">"
                            + cr3 + "<div class=\"ccEditorPreferenceCon\" id=\"" + fancyBoxContentId + "\"><div style=\"margin:20px auto auto auto;\"><img src=\"/ccLib/images/ajax-loader-big.gif\" width=\"32\" height=\"32\"></div></div>"
                            + cr2 + "</div>"
                            + "\r</div>"
                            + "";
                        fancyBoxPtr = fancyBoxPtr + 1;
                        //
                        //------------------------------------------------------------------------------------------------------------
                        // field help
                        //------------------------------------------------------------------------------------------------------------
                        //
                        if (core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                            //
                            // Admin view
                            //
                            if (string.IsNullOrEmpty(HelpMsgDefault)) {
                                HelpMsgDefault = "Admin: No default help is available for this field.";
                            }
                            HelpMsgOpenedRead = ""
                                    + "<!-- close icon --><div class=\"\" style=\"float:right\"><a href=\"javascript:cj.hide('" + HelpOpenedReadID + "');cj.show('" + HelpClosedID + "');\"><img src=\"/ccLib/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"close\"></a></div>"
                                    + "<div class=\"header\">Default Help</div>"
                                    + "<div class=\"body\">" + HelpMsgDefault + "</div>"
                                    + "<div class=\"header\">Custom Help</div>"
                                    + "<div class=\"body\">" + HelpMsgCustom + "</div>"
                                + "";
                            HelpMsgOpenedEdit = ""
                                    + "<div class=\"header\">Default Help</div>"
                                    + "<div class=\"body\">" + HelpMsgDefault + "</div>"
                                    + "<div class=\"header\">Custom Help</div>"
                                    + "<div class=\"body\"><textarea id=\"" + HelpEditorID + "\" ROWS=\"10\" style=\"width:100%;\">" + HelpMsgCustom + "</TEXTAREA></div>"
                                    + "<div class=\"\">"
                                        + "<input type=\"submit\" name=\"button\" value=\"Update\" onClick=\"updateFieldHelp('" + fieldId + "','" + HelpEditorID + "','" + HelpClosedContentID + "');cj.hide('" + HelpOpenedEditID + "');cj.show('" + HelpClosedID + "');return false\">"
                                        + "<input type=\"submit\" name=\"button\" value=\"Cancel\" onClick=\"cj.hide('" + HelpOpenedEditID + "');cj.show('" + HelpClosedID + "');return false\">"
                                    + "</div>"
                                + "";
                            if (IsLongHelp) {
                                //
                                // Long help, closed gets MoreHelpIcon (opens to HelpMsgOpenedRead) and HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                //
                                HelpMsgClosed = ""
                                        + "<!-- open read icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedID + "');cj.show('" + HelpOpenedReadID + "');\" tabindex=\"-1\"><img src=\"/ccLib/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"more help\"></a></div>"
                                        + "<!-- open edit icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedID + "');cj.show('" + HelpOpenedEditID + "');\" tabindex=\"-1\"><img src=\"/ccLib/images/NavHelpEdit.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"edit help\"></a></div>"
                                        + "<div id=\"" + HelpClosedContentID + "\">" + HelpMsgClosed + "</div>"
                                    + "";
                            } else if (!IsEmptyHelp) {
                                //
                                // short help, closed gets helpmsgclosed + HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                //
                                HelpMsgClosed = ""
                                        + "<!-- open edit icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedID + "');cj.show('" + HelpOpenedEditID + "');\" tabindex=\"-1\"><img src=\"/ccLib/images/NavHelpEdit.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"edit help\"></a></div>"
                                        + "<div id=\"" + HelpClosedContentID + "\">" + HelpMsgClosed + "</div>"
                                    + "";
                            } else {
                                //
                                // Empty help, closed gets helpmsgclosed + HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                //
                                HelpMsgClosed = ""
                                        + "<!-- open edit icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedID + "');cj.show('" + HelpOpenedEditID + "');\" tabindex=\"-1\"><img src=\"/ccLib/images/NavHelpEdit.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"edit help\"></a></div>"
                                        + "<div id=\"" + HelpClosedContentID + "\">" + HelpMsgClosed + "</div>"
                                    + "";
                            }
                            EditorHelp = EditorHelp + "<div id=\"" + HelpOpenedReadID + "\" class=\"opened\">" + HelpMsgOpenedRead + "</div>"
                                + "<div id=\"" + HelpOpenedEditID + "\" class=\"opened\">" + HelpMsgOpenedEdit + "</div>"
                                + "<div id=\"" + HelpClosedID + "\" class=\"closed\">" + HelpMsgClosed + "</div>"
                                + "";
                        } else {
                            //
                            // Non-admin view
                            //
                            HelpMsgOpenedRead = ""
                                    + "<div class=\"body\">"
                                    + "<!-- close icon --><a href=\"javascript:cj.hide('" + HelpOpenedReadID + "');cj.show('" + HelpClosedID + "');\"><img src=\"/ccLib/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;float:right\" title=\"close\"></a>"
                                    + HelpMsg + "</div>"
                                + "";
                            //HelpMsgOpenedRead = "" _
                            //        & "<!-- close icon --><div class="""" style=""float:right""><a href=""javascript:cj.hide('" & HelpOpenedReadID & "');cj.show('" & HelpClosedID & "');""><img src=""/ccLib/images/NavHelp.gif"" width=18 height=18 border=0 style=""vertical-align:middle;"" title=""close""></a></div>" _
                            //        & "<div class=""body"">" & HelpMsg & "</div>" _
                            //    & ""
                            HelpMsgOpenedEdit = ""
                                + "";
                            if (IsLongHelp) {
                                //
                                // Long help
                                //
                                HelpMsgClosed = ""
                                    + "<div class=\"body\">"
                                    + "<!-- open read icon --><a href=\"javascript:cj.hide('" + HelpClosedID + "');cj.show('" + HelpOpenedReadID + "');\"><img src=\"/ccLib/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;float:right;\" title=\"more help\"></a>"
                                    + HelpMsgClosed + "</div>"
                                    + "";
                            } else if (!IsEmptyHelp) {
                                //
                                // short help
                                //
                                HelpMsgClosed = ""
                                    + "<div class=\"body\">"
                                        + HelpMsg + "</div>"
                                    + "";
                            } else {
                                //
                                // no help
                                //
                                AllowHelpRow = false;
                                HelpMsgClosed = ""
                                    + "";
                            }
                            EditorHelp = EditorHelp + "<div id=\"" + HelpOpenedReadID + "\" class=\"opened\">" + HelpMsgOpenedRead + "</div>"
                                + "<div id=\"" + HelpClosedID + "\" class=\"closed\">" + HelpMsgClosed + "</div>"
                                + "";
                        }
                        //
                        // assemble the help line
                        //
                        s.Add("<tr><td class=\"ccEditCaptionCon\"><div class=\"" + EditorStyleModifier + "\">" + Caption + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"1\" height=\"15\" ></div></td><td class=\"ccEditFieldCon\"><div class=\"ccEditorCon\">" + EditorString + "</div>");
                        if (AllowHelpRow) {
                            s.Add("<div class=\"ccEditorHelpCon\">" + EditorHelp + "</div>");
                        }
                        s.Add("</td></tr>");
                    }
                    //
                    // ----- add the *Required Fields footer
                    //
                    s.Add("<tr><td colspan=2 style=\"padding-top:10px;font-size:70%\"><div>* Field is required.</div><div>** Field must be unique.</div>");
                    if (needUniqueEmailMessage) {
                        s.Add("<div>*** Field must be unique because this site allows login by email.</div>");
                    }
                    s.Add("</td></tr>");
                    //
                    // ----- close the panel
                    //
                    if (string.IsNullOrEmpty(EditTab)) {
                        Caption = "Content Fields";
                    } else {
                        Caption = "Content Fields - " + EditTab;
                    }
                    EditSectionPanelCount = EditSectionPanelCount + 1;
                    returnHtml = Adminui.GetEditPanel((!allowAdminTabs), Caption, "", Adminui.EditTableOpen + s.Text + Adminui.EditTableClose);
                    EditSectionPanelCount = EditSectionPanelCount + 1;
                    s = null;
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnHtml;
        }
        //
        //========================================================================
        //   Display field in the admin/edit
        //========================================================================
        //
        private string GetForm_Edit_ContentTracking(cdefModel adminContent, editRecordClass editRecord) {
            string tempGetForm_Edit_ContentTracking = null;
            try {
                //
                int CSRules = 0;
                string HTMLFieldString = null;
                int CSLists = 0;
                int RecordCount = 0;
                int ContentWatchListID = 0;
                stringBuilderLegacyController FastString = null;
                string Copy = null;
                adminUIController Adminui = new adminUIController(core);
                //
                if (adminContent.AllowContentTracking) {
                    FastString = new stringBuilderLegacyController();
                    //
                    if (!ContentWatchLoaded) {
                        //
                        // ----- Load in the record to print
                        //
                        LoadContentTrackingDataBase(adminContent, editRecord);
                        LoadContentTrackingResponse(adminContent, editRecord);
                        //        Call LoadAndSaveCalendarEvents
                    }
                    CSLists = core.db.csOpen("Content Watch Lists", "name<>" + core.db.encodeSQLText(""), "ID");
                    if (core.db.csOk(CSLists)) {
                        //
                        // ----- Open the panel
                        //
                        //Call core.main_PrintPanelTop("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                        //Call FastString.Add(adminui.EditTableOpen)
                        //Call FastString.Add(vbCrLf & "<tr><td colspan=""3"" class=""ccAdminEditSubHeader"">Content Tracking</td></tr>")
                        //            '
                        //            ' ----- Print matching Content Watch fields
                        //            '
                        //            Call FastString.Add(core.main_GetFormInputHidden("WhatsNewResponse", -1))
                        //            Call FastString.Add(core.main_GetFormInputHidden("contentwatchrecordid", ContentWatchRecordID))
                        //
                        // ----- Content Watch Lists, checking the ones that have active rules
                        //
                        RecordCount = 0;
                        while (core.db.csOk(CSLists)) {
                            ContentWatchListID = core.db.csGetInteger(CSLists, "id");
                            //
                            if (ContentWatchRecordID != 0) {
                                CSRules = core.db.csOpen("Content Watch List Rules", "(ContentWatchID=" + ContentWatchRecordID + ")AND(ContentWatchListID=" + ContentWatchListID + ")");
                                if (editRecord.Read_Only) {
                                    HTMLFieldString = genericController.encodeText(core.db.csOk(CSRules));
                                } else {
                                    HTMLFieldString = core.html.inputCheckbox("ContentWatchList." + core.db.csGet(CSLists, "ID"), core.db.csOk(CSRules));
                                }
                                core.db.csClose(ref CSRules);
                            } else {
                                if (editRecord.Read_Only) {
                                    HTMLFieldString = genericController.encodeText(false);
                                } else {
                                    HTMLFieldString = core.html.inputCheckbox("ContentWatchList." + core.db.csGet(CSLists, "ID"), false);
                                }
                            }
                            //
                            FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Include in " + core.db.csGet(CSLists, "name"), "When true, this Content Record can be included in the '" + core.db.csGet(CSLists, "name") + "' list", false, false, ""));
                            core.db.csGoNext(CSLists);
                            RecordCount = RecordCount + 1;
                        }
                        //
                        // ----- Whats New Headline (editable)
                        //
                        if (editRecord.Read_Only) {
                            HTMLFieldString = core.html.encodeHTML(ContentWatchLinkLabel);
                        } else {
                            HTMLFieldString = core.html.inputText("ContentWatchLinkLabel", ContentWatchLinkLabel, 1, core.siteProperties.defaultFormInputWidth);
                            //HTMLFieldString = "<textarea rows=""1"" name=""ContentWatchLinkLabel"" cols=""" & core.app.SiteProperty_DefaultFormInputWidth & """>" & ContentWatchLinkLabel & "</textarea>"
                        }
                        FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Caption", "This caption is displayed on all Content Watch Lists, linked to the location on the web site where this content is displayed. RSS feeds created from Content Watch Lists will use this caption as the record title if not other field is selected in the Content Definition.", false, true, "ContentWatchLinkLabel"));
                        //
                        // ----- Whats New Expiration
                        //
                        Copy = ContentWatchExpires.ToString();
                        if (Copy == "12:00:00 AM") {
                            Copy = "";
                        }
                        if (editRecord.Read_Only) {
                            HTMLFieldString = core.html.encodeHTML(Copy);
                        } else {
                            HTMLFieldString = core.html.inputDate("ContentWatchExpires", Copy, core.siteProperties.defaultFormInputWidth.ToString());
                            //HTMLFieldString = "<textarea rows=""1"" name=""ContentWatchExpires"" cols=""" & core.app.SiteProperty_DefaultFormInputWidth & """>" & Copy & "</textarea>"
                        }
                        FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Expires", "When this record is included in a What's New list, this record is blocked from the list after this date.", false, false, ""));
                        //
                        // ----- Public Link (read only)
                        //
                        HTMLFieldString = ContentWatchLink;
                        if (string.IsNullOrEmpty(HTMLFieldString)) {
                            HTMLFieldString = "(must first be viewed on public site)";
                        }
                        FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Location on Site", "The public site URL where this content was last viewed.", false, false, ""));
                        //
                        // removed 11/27/07 - RSS clicks not counted, rc/ri method of counting link clicks is not reliable.
                        //            '
                        //            ' ----- Clicks (read only)
                        //            '
                        //            HTMLFieldString = ContentWatchClicks
                        //            If HTMLFieldString = "" Then
                        //                HTMLFieldString = 0
                        //                End If
                        //            Call FastString.Add(AdminUI.GetEditRow( HTMLFieldString, "Clicks", "The number of site users who have clicked this link in what's new lists", False, False, ""))
                        //
                        // ----- close the panel
                        //
                        string s = ""
                        + Adminui.EditTableOpen + FastString.Text + Adminui.EditTableClose + core.html.inputHidden("WhatsNewResponse", "-1") + core.html.inputHidden("contentwatchrecordid", ContentWatchRecordID.ToString());
                        tempGetForm_Edit_ContentTracking = Adminui.GetEditPanel((!allowAdminTabs), "Content Tracking", "Include in Content Watch Lists", s);
                        EditSectionPanelCount = EditSectionPanelCount + 1;
                        //
                    }
                    core.db.csClose(ref CSLists);
                    FastString = null;
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Edit_ContentTracking;
        }
        //
        //========================================================================
        //   Display field in the admin/edit
        //========================================================================
        //
        private string GetForm_Edit_Control(cdefModel adminContent, editRecordClass editRecord) {
            string tempGetForm_Edit_Control = null;
            try {
                //
                string s = null;
                bool AllowEID = false;
                string EID = null;
                bool IsEmptyList = false;
                int ParentID = 0;
                int LimitContentSelectToThisID = 0;
                int TableID = 0;
                int ChildCID = 0;
                string CIDList = "";
                string TableName2 = null;
                string RecordContentName = null;
                bool ContentSupportsParentID = false;
                int CS = 0;
                string HTMLFieldString = null;
                int CSPointer = 0;
                string hiddenInputs = "";
                stringBuilderLegacyController FastString = new stringBuilderLegacyController();
                int FieldValueInteger = 0;
                bool FieldRequired = false;
                string FieldHelp = null;
                string Copy = null;
                adminUIController Adminui = new adminUIController(core);
                //
                if (string.IsNullOrEmpty(adminContent.Name)) {
                    //
                    // Content not found or not loaded
                    //
                    if (adminContent.Id == 0) {
                        //
                        // Content Definition was not found
                        //
                        handleLegacyClassError("GetForm_Edit_Control", "No content definition was specified for this page");
                    } else {
                        //
                        // Content Definition was not specified
                        //
                        handleLegacyClassError("GetForm_Edit_Control", "The content definition specified for this page [" + adminContent.Id + "] was not found");
                    }
                } else {
                }
                //
                bool Checked = false;
                //
                // ----- Authoring status
                //
                //FieldHelp = "In immediate authoring mode, the live site is changed when each record is saved. In Workflow authoring mode, there are several steps to publishing a change. This field displays the current stage of this record."
                //FieldRequired = False
                //AuthoringStatusMessage = core.doc.authContext.main_GetAuthoringStatusMessage(core, false, editRecord.EditLock, editRecord.EditLockMemberName, editRecord.EditLockExpires, editRecord.ApproveLock, editRecord.ApprovedName, editRecord.SubmitLock, editRecord.SubmittedName, editRecord.IsDeleted, editRecord.IsInserted, editRecord.IsModified, editRecord.LockModifiedName)
                //Call FastString.Add(Adminui.GetEditRow(AuthoringStatusMessage, "Authoring Status", FieldHelp, FieldRequired, False, ""))
                //Call FastString.Add(AdminUI.GetEditRow( AuthoringStatusMessage, "Authoring Status", FieldHelp, FieldRequired, False, ""))
                //
                // ----- RecordID
                //
                FieldHelp = "This is the unique number that identifies this record within this content.";
                if (editRecord.id == 0) {
                    HTMLFieldString = "(available after save)";
                } else {
                    HTMLFieldString = genericController.encodeText(editRecord.id);
                }
                HTMLFieldString = core.html.inputText("ignore", HTMLFieldString, -1, -1, "", false, true);
                FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Record Number", FieldHelp, true, false, ""));
                //
                // -- Active
                Copy = "When unchecked, add-ons can ignore this record as if it was temporarily deleted.";
                HTMLFieldString = core.html.inputCheckbox("active", editRecord.active);
                FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Active", Copy, false, false, ""));
                //
                // ----- If Page Content , check if this is the default PageNotFound page
                //
                if (genericController.vbLCase(adminContent.ContentTableName) == "ccpagecontent") {
                    //
                    // Landing Page
                    //
                    Copy = "If selected, this page will be displayed when a user comes to your website with just your domain name and no other page is requested. This is called your default Landing Page. Only one page on the site can be the default Landing Page. If you want a unique Landing Page for a specific domain name, add it in the 'Domains' content and the default will not be used for that docore.main_";
                    Checked = ((editRecord.id != 0) && (editRecord.id == (core.siteProperties.getInteger("LandingPageID", 0))));
                    if (core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                        HTMLFieldString = core.html.inputCheckbox("LandingPageID", Checked);
                    } else {
                        HTMLFieldString = "<b>" + genericController.GetYesNo(Checked) + "</b>" + core.html.inputHidden("LandingPageID", Checked);
                    }
                    //HTMLFieldString = HTMLFieldString;
                    FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Set Default Landing Page", Copy, false, false, ""));
                    //
                    // Page Not Found
                    //
                    Copy = "If selected, this content will be displayed when a page can not be found. Only one page on the site can be marked.";
                    Checked = ((editRecord.id != 0) && (editRecord.id == (core.siteProperties.getInteger("PageNotFoundPageID", 0))));
                    if (core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                        HTMLFieldString = core.html.inputCheckbox("PageNotFound", Checked);
                    } else {
                        HTMLFieldString = "<b>" + genericController.GetYesNo(Checked) + "</b>" + core.html.inputHidden("PageNotFound", Checked);
                    }
                    //            If (EditRecord.ID <> 0) And (EditRecord.ID = core.main_GetSiteProperty2("PageNotFoundPageID", "0", True)) Then
                    //                HTMLFieldString = core.main_GetFormInputCheckBox2("PageNotFound", True)
                    //            Else
                    //                HTMLFieldString = core.main_GetFormInputCheckBox2("PageNotFound", False)
                    //            End If
                    //HTMLFieldString = HTMLFieldString;
                    FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Set Page Not Found", Copy, false, false, ""));
                }
                //
                // ----- Last Known Public Site URL
                //
                if ((adminContent.ContentTableName.ToUpper() == "CCPAGECONTENT") || (adminContent.ContentTableName.ToUpper() == "ITEMS")) {
                    FieldHelp = "This is the URL where this record was last displayed on the site. It may be blank if the record has not been displayed yet.";
                    Copy = core.doc.getContentWatchLinkByKey(editRecord.contentControlId + "." + editRecord.id, "", false);
                    if (string.IsNullOrEmpty(Copy)) {
                        HTMLFieldString = "unknown";
                    } else {
                        HTMLFieldString = "<a href=\"" + genericController.encodeHTML(Copy) + "\" target=\"_blank\">" + Copy + "</a>";
                    }
                    FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Last Known Public URL", FieldHelp, false, false, ""));
                }
                //
                // ----- Widget Code
                //
                if (genericController.vbLCase(adminContent.ContentTableName) == "ccaggregatefunctions") {
                    //
                    // ----- Add-ons
                    //
                    bool AllowWidget = false;
                    if (editRecord.fieldsLc.ContainsKey("remotemethod")) {
                        AllowWidget = genericController.encodeBoolean(editRecord.fieldsLc["remotemethod"].value);
                    }
                    if (!AllowWidget) {
                        FieldHelp = "If you wish to use this add-on as a widget, enable 'Is Remote Method' on the 'Placement' tab and save the record. The necessary html code, or 'embed code' will be created here for you to cut-and-paste into the website.";
                        HTMLFieldString = "";
                        HTMLFieldString = core.html.inputTextExpandable("ignore", HTMLFieldString, 1, "100%", "", false, true);
                        FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Widget Code", FieldHelp, true, false, ""));
                    } else {
                        FieldHelp = "If you wish to use this add-on as a widget, cut and paste the 'Widget Code' into the website content. If any code appears in the 'Widget Head', this will need to be pasted into the head section of the website.";
                        HTMLFieldString = ""
                            + "<SCRIPT type=text/javascript>"
                            + "\r\nvar ccProto=(('https:'==document.location.protocol) ? 'https:// : 'http://);"
                            + "\r\ndocument.write(unescape(\"%3Cscript src='\" + ccProto + \"" + core.webServer.requestDomain + "/ccLib/ClientSide/Core.js' type='text/javascript'%3E%3C/script%3E\"));"
                            + "\r\ndocument.write(unescape(\"%3Cscript src='\" + ccProto + \"" + core.webServer.requestDomain + "/" + genericController.EncodeURL(editRecord.nameLc) + "?requestjsform=1' type='text/javascript'%3E%3C/script%3E\"));"
                            + "\r\n</SCRIPT>";
                        //<SCRIPT type=text/javascript>
                        //var gaJsHost = (("https:" == document.location.protocol) ? "https://ssl." : "http://www.");
                        //document.write(unescape("%3Cscript src='" + gaJsHost + "google-analytics.com/ga.js' type='text/javascript'%3E%3C/script%3E"));
                        //</SCRIPT>
                        //                HTMLFieldString = "" _
                        //                    & "<script language=""javascript"" type=""text/javascript"" src=""http://" & core.main_ServerDomain & "/ccLib/ClientSide/Core.js""></script>" _
                        //                    & vbCrLf & "<script language=""javascript"" type=""text/javascript"" src=""http://" & core.main_ServerDomain & "/" & EditRecord.Name & "?requestjsform=1""></script>" _
                        //                    & ""
                        HTMLFieldString = core.html.inputTextExpandable("ignore", HTMLFieldString, 8);
                        FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Widget Code", FieldHelp, true, false, ""));
                    }
                }
                //
                // ----- GUID
                //
                if (editRecord.fieldsLc.ContainsKey("ccguid")) {
                    cdefFieldModel contentField = adminContent.fields["ccguid"];
                    HTMLFieldString = genericController.encodeText(editRecord.fieldsLc["ccguid"].value);
                    FieldHelp = "This is a unique number that identifies this record globally. A GUID is not required, but when set it should never be changed. GUIDs are used to synchronize records. When empty, you can create a new guid. Only Developers can modify the guid.";
                    if (string.IsNullOrEmpty(HTMLFieldString)) {
                        //
                        // add a set button
                        //
                        string ccGuid = "{" + Guid.NewGuid().ToString() + "}";
                        HTMLFieldString = core.html.inputText("ccguid", HTMLFieldString, -1, -1, "ccguid", false, false) + "<input type=button value=set onclick=\"var e=document.getElementById('ccguid');e.value='" + ccGuid + "';this.disabled=true;\">";
                    } else {
                        //
                        // field is read-only except for developers
                        //
                        if (core.doc.sessionContext.isAuthenticatedDeveloper(core)) {
                            HTMLFieldString = core.html.inputText("ccguid", HTMLFieldString, -1, -1, "", false, false) + "";
                        } else {
                            HTMLFieldString = core.html.inputText("ccguid", HTMLFieldString, -1, -1, "", false, true) + core.html.inputHidden("ccguid", HTMLFieldString);
                        }
                    }
                    FastString.Add(Adminui.GetEditRow(HTMLFieldString, "GUID", FieldHelp, false, false, ""));
                }
                //
                // ----- EID (Encoded ID)
                //
                FieldHelp = "";
                if (genericController.vbUCase(adminContent.ContentTableName) == genericController.vbUCase("ccMembers")) {
                    AllowEID = (core.siteProperties.getBoolean("AllowLinkLogin", true)) | (core.siteProperties.getBoolean("AllowLinkRecognize", true));
                    if (!AllowEID) {
                        HTMLFieldString = "(link login and link recognize are disabled in security preferences)";
                    } else if (editRecord.id == 0) {
                        HTMLFieldString = "(available after save)";
                    } else {
                        EID = genericController.encodeText(core.security.encodeToken(editRecord.id, core.doc.profileStartTime));
                        if (core.siteProperties.getBoolean("AllowLinkLogin", true)) {
                            HTMLFieldString = EID;
                            //HTMLFieldString = EID _
                            //            & "<div>Any visitor who hits the site with eid=" & EID & " will be logged in as this member.</div>"
                            FieldHelp = "Any visitor who hits the site with eid=" + EID + " will be logged in as this member.";
                        } else {
                            FieldHelp = "Any visitor who hits the site with eid=" + EID + " will be recognized as this member, but not logged in.";
                            HTMLFieldString = EID;
                            //HTMLFieldString = EID _
                            //            & "<div>Any visitor who hits the site with eid=" & EID & " will be recognized as this member, but not logged in</div>"
                        }
                        FieldHelp = FieldHelp + " To enable, disable or modify this feature, use the security tab on the Preferences page.";
                    }
                    HTMLFieldString = core.html.inputText("ignore", HTMLFieldString);
                    FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Member Link Login EID", FieldHelp, true, false, ""));
                }
                //
                // ----- Controlling Content
                //
                HTMLFieldString = "";
                FieldHelp = "The content in which this record is stored. This is similar to a database table.";
                cdefFieldModel field = null;
                if (adminContent.fields.ContainsKey("contentcontrolid")) {
                    field = adminContent.fields["contentcontrolid"];
                    //
                    // if this record has a parent id, only include CDefs compatible with the parent record - otherwise get all for the table
                    //
                    FieldHelp = genericController.encodeText(field.helpMessage);
                    FieldRequired = genericController.encodeBoolean(field.required);
                    FieldValueInteger = editRecord.contentControlId;
                    //
                    //
                    //
                    if (!core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                        HTMLFieldString = HTMLFieldString + core.html.inputHidden("ContentControlID", FieldValueInteger);
                    } else {
                        RecordContentName = editRecord.contentControlId_Name;
                        TableName2 = cdefModel.getContentTablename(core, RecordContentName);
                        TableID = core.db.getRecordID("Tables", TableName2);
                        //
                        // Test for parentid
                        //
                        ParentID = 0;
                        ContentSupportsParentID = false;
                        if (editRecord.id > 0) {
                            CS = core.db.csOpenRecord(RecordContentName, editRecord.id);
                            if (core.db.csOk(CS)) {
                                ContentSupportsParentID = core.db.cs_isFieldSupported(CS, "ParentID");
                                if (ContentSupportsParentID) {
                                    ParentID = core.db.csGetInteger(CS, "ParentID");
                                }
                            }
                            core.db.csClose(ref CS);
                        }
                        //
                        LimitContentSelectToThisID = 0;
                        if (ContentSupportsParentID) {
                            //
                            // Parentid - restrict CDefs to those compatible with the parentid
                            //
                            if (ParentID != 0) {
                                //
                                // This record has a parent, set LimitContentSelectToThisID to the parent's CID
                                //
                                CSPointer = core.db.csOpenRecord(RecordContentName, ParentID, false, false, "ContentControlID");
                                if (core.db.csOk(CSPointer)) {
                                    LimitContentSelectToThisID = core.db.csGetInteger(CSPointer, "ContentControlID");
                                }
                                core.db.csClose(ref CSPointer);
                            }

                        }
                        if (core.doc.sessionContext.isAuthenticatedAdmin(core) && (LimitContentSelectToThisID == 0)) {
                            //
                            // administrator, and either ( no parentid or does not support it), let them select any content compatible with the table
                            //
                            HTMLFieldString = HTMLFieldString + core.html.selectFromContent("ContentControlID", FieldValueInteger, "Content", "ContentTableID=" + TableID, "", "", ref IsEmptyList);
                            FieldHelp = FieldHelp + " (Only administrators have access to this control. Changing the Controlling Content allows you to change who can author the record, as well as how it is edited.)";
                        } else {
                            //
                            // Limit the list to only those cdefs that are within the record's parent contentid
                            //
                            RecordContentName = editRecord.contentControlId_Name;
                            TableName2 = cdefModel.getContentTablename(core, RecordContentName);
                            TableID = core.db.getRecordID("Tables", TableName2);
                            CSPointer = core.db.csOpen("Content", "ContentTableID=" + TableID, "", true, 0, false, false, "ContentControlID");
                            while (core.db.csOk(CSPointer)) {
                                ChildCID = core.db.csGetInteger(CSPointer, "ID");
                                if (cdefModel.isWithinContent(core, ChildCID, LimitContentSelectToThisID)) {
                                    if ((core.doc.sessionContext.isAuthenticatedAdmin(core)) | (core.doc.sessionContext.isAuthenticatedContentManager(core, cdefModel.getContentNameByID(core, ChildCID)))) {
                                        CIDList = CIDList + "," + ChildCID;
                                    }
                                }
                                core.db.csGoNext(CSPointer);
                            }
                            core.db.csClose(ref CSPointer);
                            if (!string.IsNullOrEmpty(CIDList)) {
                                CIDList = CIDList.Substring(1);
                                HTMLFieldString = core.html.selectFromContent("ContentControlID", FieldValueInteger, "Content", "id in (" + CIDList + ")", "", "", ref IsEmptyList);
                                FieldHelp = FieldHelp + " (Only administrators have access to this control. Changing the Controlling Content allows you to change who can author the record, as well as how it is edited. This record includes a Parent field, so your choices for controlling content are limited to those compatible with the parent of this record.)";
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(HTMLFieldString)) {
                    HTMLFieldString = editRecord.contentControlId_Name;
                    //HTMLFieldString = cdefmodel.getContentNameByID(core,EditRecord.ContentID)
                }
                FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Controlling Content", FieldHelp, FieldRequired, false, ""));
                //
                // ----- Created By
                //
                FieldHelp = "The people account of the user who created this record.";
                if (editRecord.id == 0) {
                    HTMLFieldString = "(available after save)";
                } else {
                    FieldValueInteger = editRecord.createByMemberId;
                    if (FieldValueInteger == 0) {
                        HTMLFieldString = "unknown";
                    } else {
                        CSPointer = core.db.cs_open2("people", FieldValueInteger, true);
                        if (!core.db.csOk(CSPointer)) {
                            HTMLFieldString = "unknown";
                        } else {
                            HTMLFieldString = core.db.csGet(CSPointer, "name");
                        }
                        core.db.csClose(ref CSPointer);
                    }
                }
                HTMLFieldString = core.html.inputText("ignore", HTMLFieldString, -1, -1, "", false, true);
                FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Created By", FieldHelp, FieldRequired, false, ""));
                //
                // ----- Created Date
                //
                FieldHelp = "The date and time when this record was originally created.";
                if (editRecord.id == 0) {
                    HTMLFieldString = "(available after save)";
                } else {
                    HTMLFieldString = genericController.encodeText(genericController.encodeDate(editRecord.dateAdded));
                    if (HTMLFieldString == "12:00:00 AM") {
                        HTMLFieldString = "unknown";
                    }
                }
                HTMLFieldString = core.html.inputText("ignore", HTMLFieldString, -1, -1, "", false, true);
                FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Created Date", FieldHelp, FieldRequired, false, ""));
                //
                // ----- Modified By
                //
                FieldHelp = "The people account of the last user who modified this record.";
                if (editRecord.id == 0) {
                    HTMLFieldString = "(available after save)";
                } else {
                    FieldValueInteger = editRecord.modifiedByMemberID;
                    HTMLFieldString = "unknown";
                    if (FieldValueInteger > 0) {
                        CSPointer = core.db.cs_open2("people", FieldValueInteger, true, false, "name");
                        if (core.db.csOk(CSPointer)) {
                            HTMLFieldString = core.db.csGet(CSPointer, "name");
                        }
                        core.db.csClose(ref CSPointer);
                    }
                }
                HTMLFieldString = core.html.inputText("ignore", HTMLFieldString, -1, -1, "", false, true);
                FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Modified By", FieldHelp, FieldRequired, false, ""));
                //
                // ----- Modified Date
                //
                FieldHelp = "The date and time when this record was last modified";
                if (editRecord.id == 0) {
                    HTMLFieldString = "(available after save)";
                } else {
                    HTMLFieldString = genericController.encodeText(genericController.encodeDate(editRecord.modifiedDate));
                    if (HTMLFieldString == "12:00:00 AM") {
                        HTMLFieldString = "unknown";
                    }
                }
                HTMLFieldString = core.html.inputText("ignore", HTMLFieldString, -1, -1, "", false, true);
                FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Modified Date", FieldHelp, false, false, ""));
                s = ""
                    + Adminui.EditTableOpen + FastString.Text + Adminui.EditTableClose + hiddenInputs + "";
                tempGetForm_Edit_Control = Adminui.GetEditPanel((!allowAdminTabs), "Control Information", "", s);
                EditSectionPanelCount = EditSectionPanelCount + 1;
                FastString = null;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Edit_Control;
        }
        //
        //========================================================================
        //   Display field in the admin/edit
        //========================================================================
        //
        private string GetForm_Edit_SiteProperties(cdefModel adminContent, editRecordClass editRecord) {
            string tempGetForm_Edit_SiteProperties = null;
            try {
                //
                string ExpandedSelector = "";
                string OptionCaption = null;
                string OptionValue = null;
                string OptionValue_AddonEncoded = null;
                int OptionPtr = 0;
                int OptionCnt = 0;
                string[] OptionValues = null;
                string OptionSuffix = "";
                string LCaseOptionDefault = null;
                int Pos = 0;
                string HTMLFieldString = null;
                stringBuilderLegacyController FastString = null;
                string Copy = "";
                adminUIController Adminui = new adminUIController(core);
                string SitePropertyName = null;
                string SitePropertyValue = null;
                string selector = null;
                string FieldName = null;
                //
                FastString = new stringBuilderLegacyController();
                //
                SitePropertyName = "";
                SitePropertyValue = "";
                selector = "";
                foreach (KeyValuePair<string, cdefFieldModel> keyValuePair in adminContent.fields) {
                    cdefFieldModel field = keyValuePair.Value;
                    //
                    FieldName = field.nameLc;
                    if (genericController.vbLCase(FieldName) == "name") {
                        SitePropertyName = genericController.encodeText(editRecord.fieldsLc[field.nameLc].value);
                    } else if (FieldName.ToLower() == "fieldvalue") {
                        SitePropertyValue = genericController.encodeText(editRecord.fieldsLc[field.nameLc].value);
                    } else if (FieldName.ToLower() == "selector") {
                        selector = genericController.encodeText(editRecord.fieldsLc[field.nameLc].value);
                    }
                }
                if (string.IsNullOrEmpty(SitePropertyName)) {
                    HTMLFieldString = "This Site Property is not defined";
                } else {
                    HTMLFieldString = core.html.inputHidden("name", SitePropertyName);
                    Dictionary<string, string> instanceOptions = new Dictionary<string, string>();
                    Dictionary<string, string> addonInstanceProperties = new Dictionary<string, string>();
                    instanceOptions.Add(SitePropertyName, SitePropertyValue);
                    core.addon.buildAddonOptionLists(ref addonInstanceProperties, ref ExpandedSelector, SitePropertyName + "=" + selector, instanceOptions, "0", true);

                    //--------------

                    Pos = genericController.vbInstr(1, ExpandedSelector, "[");
                    if (Pos != 0) {
                        //
                        // List of Options, might be select, radio or checkbox
                        //
                        LCaseOptionDefault = genericController.vbLCase(ExpandedSelector.Left(Pos - 1));
                        LCaseOptionDefault = genericController.decodeNvaArgument(LCaseOptionDefault);

                        ExpandedSelector = ExpandedSelector.Substring(Pos);
                        Pos = genericController.vbInstr(1, ExpandedSelector, "]");
                        if (Pos > 0) {
                            if (Pos < ExpandedSelector.Length) {
                                OptionSuffix = genericController.vbLCase((ExpandedSelector.Substring(Pos)).Trim(' '));
                            }
                            ExpandedSelector = ExpandedSelector.Left(Pos - 1);
                        }
                        OptionValues = ExpandedSelector.Split('|');
                        HTMLFieldString = "";
                        OptionCnt = OptionValues.GetUpperBound(0) + 1;
                        for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++) {
                            OptionValue_AddonEncoded = OptionValues[OptionPtr].Trim(' ');
                            if (!string.IsNullOrEmpty(OptionValue_AddonEncoded)) {
                                Pos = genericController.vbInstr(1, OptionValue_AddonEncoded, ":");
                                if (Pos == 0) {
                                    OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded);
                                    OptionCaption = OptionValue;
                                } else {
                                    OptionCaption = genericController.decodeNvaArgument(OptionValue_AddonEncoded.Left(Pos - 1));
                                    OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded.Substring(Pos));
                                }
                                switch (OptionSuffix) {
                                    case "checkbox":
                                        //
                                        // Create checkbox HTMLFieldString
                                        //
                                        if (genericController.vbInstr(1, "," + LCaseOptionDefault + ",", "," + genericController.vbLCase(OptionValue) + ",") != 0) {
                                            HTMLFieldString = HTMLFieldString + "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + SitePropertyName + OptionPtr + "\" value=\"" + OptionValue + "\" checked=\"checked\">" + OptionCaption + "</div>";
                                        } else {
                                            HTMLFieldString = HTMLFieldString + "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + SitePropertyName + OptionPtr + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
                                        }
                                        break;
                                    case "radio":
                                        //
                                        // Create Radio HTMLFieldString
                                        //
                                        if (genericController.vbLCase(OptionValue) == LCaseOptionDefault) {
                                            HTMLFieldString = HTMLFieldString + "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + SitePropertyName + "\" value=\"" + OptionValue + "\" checked=\"checked\" >" + OptionCaption + "</div>";
                                        } else {
                                            HTMLFieldString = HTMLFieldString + "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + SitePropertyName + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
                                        }
                                        break;
                                    default:
                                        //
                                        // Create select HTMLFieldString
                                        //
                                        if (genericController.vbLCase(OptionValue) == LCaseOptionDefault) {
                                            HTMLFieldString = HTMLFieldString + "<option value=\"" + OptionValue + "\" selected>" + OptionCaption + "</option>";
                                        } else {
                                            HTMLFieldString = HTMLFieldString + "<option value=\"" + OptionValue + "\">" + OptionCaption + "</option>";
                                        }
                                        break;
                                }
                            }
                        }
                        switch (OptionSuffix) {
                            case "checkbox":
                                //
                                // FormID-SitePropertyName-Cnt is the count of Options used with checkboxes
                                // This is used
                                //
                                Copy += "<input type=\"hidden\" name=\"" + SitePropertyName + "CheckBoxCnt\" value=\"" + OptionCnt + "\" >";
                                break;
                            case "radio":
                                //
                                // Create Radio HTMLFieldString
                                //
                                //HTMLFieldString = "<div>" & genericController.vbReplace(HTMLFieldString, "><", "></div><div><") & "</div>"
                                break;
                            default:
                                //
                                // Create select HTMLFieldString
                                //
                                HTMLFieldString = "<select name=\"" + SitePropertyName + "\">" + HTMLFieldString + "</select>";
                                break;
                        }
                    } else {
                        //
                        // Create Text HTMLFieldString
                        //

                        selector = genericController.decodeNvaArgument(selector);
                        HTMLFieldString = core.html.inputText(SitePropertyName, selector, 1, 20);
                    }
                    //--------------

                    //HTMLFieldString = core.main_GetFormInputText2( genericController.vbLCase(FieldName), VAlue)
                }
                FastString.Add(Adminui.GetEditRow(HTMLFieldString, SitePropertyName, "", false, false, ""));
                tempGetForm_Edit_SiteProperties = Adminui.GetEditPanel((!allowAdminTabs), "Control Information", "", Adminui.EditTableOpen + FastString.Text + Adminui.EditTableClose);
                EditSectionPanelCount = EditSectionPanelCount + 1;
                FastString = null;
                return tempGetForm_Edit_SiteProperties;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Edit_SiteProperties;
        }
        //
        //
        //========================================================================
        //   Print the root form
        //========================================================================
        //
        private string GetForm_Root() {
            string returnHtml = "";
            try {
                int CS = 0;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                int addonId = 0;
                string AddonIDText = null;
                //
                // This is really messy -- there must be a better way
                //
                addonId = 0;
                if (core.doc.sessionContext.visit.id == core.docProperties.getInteger(RequestNameDashboardReset)) {
                    //$$$$$ cache this
                    CS = core.db.csOpen(cnAddons, "ccguid=" + core.db.encodeSQLText(addonGuidDashboard));
                    if (core.db.csOk(CS)) {
                        addonId = core.db.csGetInteger(CS, "id");
                        core.siteProperties.setProperty("AdminRootAddonID", genericController.encodeText(addonId));
                    }
                    core.db.csClose(ref CS);
                }
                if (addonId == 0) {
                    //
                    // Get AdminRootAddon
                    //
                    AddonIDText = core.siteProperties.getText("AdminRootAddonID", "");
                    if (string.IsNullOrEmpty(AddonIDText)) {
                        //
                        // the desktop is likely unset, auto set it to dashboard
                        //
                        addonId = -1;
                    } else if (AddonIDText == "0") {
                        //
                        // the desktop has been set to none - go with default desktop
                        //
                        addonId = 0;
                    } else if (AddonIDText.IsNumeric()) {
                        //
                        // it has been set to a non-zero number
                        //
                        addonId = genericController.encodeInteger(AddonIDText);
                        //
                        // Verify it so there is no error when it runs
                        //
                        CS = core.db.csOpenRecord(cnAddons, addonId);
                        if (!core.db.csOk(CS)) {
                            //
                            // it was set, but the add-on is not available, auto set to dashboard
                            //
                            addonId = -1;
                            core.siteProperties.setProperty("AdminRootAddonID", "");
                        }
                        core.db.csClose(ref CS);
                    }
                    if (addonId == -1) {
                        //
                        // This has never been set, try to get the dashboard ID
                        //
                        //$$$$$ cache this
                        CS = core.db.csOpen(cnAddons, "ccguid=" + core.db.encodeSQLText(addonGuidDashboard));
                        if (core.db.csOk(CS)) {
                            addonId = core.db.csGetInteger(CS, "id");
                            core.siteProperties.setProperty("AdminRootAddonID", genericController.encodeText(addonId));
                        }
                        core.db.csClose(ref CS);
                    }
                }
                if (addonId != 0) {
                    //
                    // Display the Addon
                    //
                    if (core.doc.debug_iUserError != "") {
                        returnHtml = returnHtml + "<div style=\"clear:both;margin-top:20px;\">&nbsp;</div>"
                        + "<div style=\"clear:both;margin-top:20px;\">" + errorController.getUserError(core) + "</div>";
                    }
                    returnHtml += core.addon.execute(addonModel.create(core, addonId), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                        addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                        errorCaption = "id:" + addonId
                    });
                    //returnHtml = returnHtml & core.addon.execute_legacy4(CStr(addonId), "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin)
                }
                if (string.IsNullOrEmpty(returnHtml)) {
                    //
                    // Nothing Displayed, show default root page
                    //
                    returnHtml = returnHtml + "\r\n<div style=\"padding:20px;height:450px\">"
                    + "\r\n<div><a href=http://www.Contensive.com target=_blank><img style=\"border:1px solid #000;\" src=\"/ccLib/images/ContensiveAdminLogo.GIF\" border=0 ></A></div>"
                    + "\r\n<div><strong>Contensive/" + core.codeVersion() + "</strong></div>"
                    + "\r\n<div style=\"clear:both;height:18px;margin-top:10px\"><div style=\"float:left;width:200px;\">Domain Name</div><div style=\"float:left;\">" + core.webServer.requestDomain + "</div></div>"
                    + "\r\n<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\">Login Member Name</div><div style=\"float:left;\">" + core.doc.sessionContext.user.name + "</div></div>"
                    + "\r\n<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\">Quick Reports</div><div style=\"float:left;\"><a Href=\"?" + RequestNameAdminForm + "=" + AdminFormQuickStats + "\">Real-Time Activity</A></div></div>"
                    + "\r\n<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\"><a Href=\"?" + RequestNameDashboardReset + "=" + core.doc.sessionContext.visit.id + "\">Run Dashboard</A></div></div>"
                    + "\r\n<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\"><a Href=\"?addonguid=" + addonGuidAddonManager + "\">Add-on Manager</A></div></div>";
                    //
                    if (core.doc.debug_iUserError != "") {
                        returnHtml = returnHtml + "<div style=\"clear:both;margin-top:20px;\">&nbsp;</div>"
                        + "<div style=\"clear:both;margin-top:20px;\">" + errorController.getUserError(core) + "</div>";
                    }
                    //
                    returnHtml = returnHtml + "\r\n</div>"
                    + "";
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnHtml;
        }
        //
        //========================================================================
        //   Print the root form
        //========================================================================
        //
        private string GetForm_QuickStats() {
            string tempGetForm_QuickStats = null;
            try {
                //
                string SQL = null;
                int CS = 0;
                string RowColor = null;
                string Panel = null;
                int VisitID = 0;
                int VisitCount = 0;
                double PageCount = 0;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                //
                // --- Start a form to make a refresh button
                //
                Stream.Add(core.html.formStart());
                Stream.Add(core.html.getPanelButtons(ButtonCancel + "," + ButtonRefresh, "" + RequestNameButton + ""));
                Stream.Add("<input TYPE=\"hidden\" NAME=\"asf\" VALUE=\"" + AdminFormQuickStats + "\">");
                Stream.Add(core.html.getPanel(" "));
                //
                // --- Indented part (Title Area plus page)
                //
                Stream.Add("<table border=\"0\" cellpadding=\"20\" cellspacing=\"0\" width=\"100%\"><tr><td>" + SpanClassAdminNormal);
                Stream.Add("<h1>Real-Time Activity Report</h1>");
                //
                // --- set column width
                //
                Stream.Add("<h2>Visits Today</h2>");
                Stream.Add("<table border=\"0\" cellpadding=\"3\" cellspacing=\"0\" width=\"100%\" style=\"background-color:white;border-top:1px solid #888;\">");
                //Stream.Add( "<tr"">")
                //Stream.Add( "<td width=""150""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""140"" height=""1"" ></td>")
                //Stream.Add( "<td width=""150""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""140"" height=""1"" ></td>")
                //Stream.Add( "<td width=""100%""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1"" ></td>")
                //Stream.Add( "</tr>")
                //
                // ----- All Visits Today
                //
                SQL = "SELECT Count(ccVisits.ID) AS VisitCount, Avg(ccVisits.PageVisits) AS PageCount FROM ccVisits WHERE ((ccVisits.StartTime)>" + core.db.encodeSQLDate(core.doc.profileStartTime.Date) + ");";
                CS = core.db.csOpenSql(SQL);
                if (core.db.csOk(CS)) {
                    VisitCount = core.db.csGetInteger(CS, "VisitCount");
                    PageCount = core.db.csGetNumber(CS, "pageCount");
                    Stream.Add("<tr>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "All Visits</span></td>");
                    Stream.Add("<td style=\"width:150px;border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + genericController.encodeHTML(core.appConfig.adminRoute + "?" + RequestNameAdminForm + "=" + AdminFormReports + "&rid=3&DateFrom=" + core.doc.profileStartTime + "&DateTo=" + core.doc.profileStartTime.ToShortDateString()) + "\">" + VisitCount + "</A>, " + string.Format("{0:N2}", PageCount) + " pages/visit.</span></td>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "This includes all visitors to the website, including guests, bots and administrators. Pages/visit includes page hits and not ajax or remote method hits.</span></td>");
                    Stream.Add("</tr>");
                }
                core.db.csClose(ref CS);
                //
                // ----- Non-Bot Visits Today
                //
                SQL = "SELECT Count(ccVisits.ID) AS VisitCount, Avg(ccVisits.PageVisits) AS PageCount FROM ccVisits WHERE (ccVisits.CookieSupport=1)and((ccVisits.StartTime)>" + core.db.encodeSQLDate(core.doc.profileStartTime.Date) + ");";
                CS = core.db.csOpenSql(SQL);
                if (core.db.csOk(CS)) {
                    VisitCount = core.db.csGetInteger(CS, "VisitCount");
                    PageCount = core.db.csGetNumber(CS, "pageCount");
                    Stream.Add("<tr>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "Non-bot Visits</span></td>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + genericController.encodeHTML(core.appConfig.adminRoute + "?" + RequestNameAdminForm + "=" + AdminFormReports + "&rid=3&DateFrom=" + core.doc.profileStartTime.ToShortDateString() + "&DateTo=" + core.doc.profileStartTime.ToShortDateString()) + "\">" + VisitCount + "</A>, " + string.Format("{0:N2}", PageCount) + " pages/visit.</span></td>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "This excludes hits from visitors identified as bots. Pages/visit includes page hits and not ajax or remote method hits.</span></td>");
                    Stream.Add("</tr>");
                }
                core.db.csClose(ref CS);
                //
                // ----- Visits Today by new visitors
                //
                SQL = "SELECT Count(ccVisits.ID) AS VisitCount, Avg(ccVisits.PageVisits) AS PageCount FROM ccVisits WHERE (ccVisits.CookieSupport=1)and(ccVisits.StartTime>" + core.db.encodeSQLDate(core.doc.profileStartTime.Date) + ")AND(ccVisits.VisitorNew<>0);";
                CS = core.db.csOpenSql(SQL);
                if (core.db.csOk(CS)) {
                    VisitCount = core.db.csGetInteger(CS, "VisitCount");
                    PageCount = core.db.csGetNumber(CS, "pageCount");
                    Stream.Add("<tr>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "Visits by New Visitors</span></td>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + genericController.encodeHTML(core.appConfig.adminRoute + "?" + RequestNameAdminForm + "=" + AdminFormReports + "&rid=3&ExcludeOldVisitors=1&DateFrom=" + core.doc.profileStartTime.ToShortDateString() + "&DateTo=" + core.doc.profileStartTime.ToShortDateString()) + "\">" + VisitCount + "</A>, " + string.Format("{0:N2}", PageCount) + " pages/visit.</span></td>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "This includes only new visitors not identified as bots. Pages/visit includes page hits and not ajax or remote method hits.</span></td>");
                    Stream.Add("</tr>");
                }
                core.db.csClose(ref CS);
                //
                Stream.Add("</table>");
                //
                // ----- Visits currently online
                //
                if (true) {
                    Panel = "";
                    Stream.Add("<h2>Current Visits</h2>");
                    SQL = "SELECT ccVisits.HTTP_REFERER as referer,ccVisits.remote_addr as Remote_Addr, ccVisits.LastVisitTime as LastVisitTime, ccVisits.PageVisits as PageVisits, ccMembers.Name as MemberName, ccVisits.ID as VisitID, ccMembers.ID as MemberID"
                        + " FROM ccVisits LEFT JOIN ccMembers ON ccVisits.MemberID = ccMembers.ID"
                        + " WHERE (((ccVisits.LastVisitTime)>" + core.db.encodeSQLDate(core.doc.profileStartTime.AddHours(-1)) + "))"
                        + " ORDER BY ccVisits.LastVisitTime DESC;";
                    CS = core.db.csOpenSql(SQL);
                    if (core.db.csOk(CS)) {
                        Panel = Panel + "<table width=\"100%\" border=\"0\" cellspacing=\"1\" cellpadding=\"2\">";
                        Panel = Panel + "<tr bgcolor=\"#B0B0B0\">";
                        Panel = Panel + "<td width=\"20%\" align=\"left\">" + SpanClassAdminNormal + "User</td>";
                        Panel = Panel + "<td width=\"20%\" align=\"left\">" + SpanClassAdminNormal + "IP&nbsp;Address</td>";
                        Panel = Panel + "<td width=\"20%\" align=\"left\">" + SpanClassAdminNormal + "Last&nbsp;Page&nbsp;Hit</td>";
                        Panel = Panel + "<td width=\"10%\" align=\"right\">" + SpanClassAdminNormal + "Page&nbsp;Hits</td>";
                        Panel = Panel + "<td width=\"10%\" align=\"right\">" + SpanClassAdminNormal + "Visit</td>";
                        Panel = Panel + "<td width=\"30%\" align=\"left\">" + SpanClassAdminNormal + "Referer</td>";
                        Panel = Panel + "</tr>";
                        RowColor = "ccPanelRowEven";
                        while (core.db.csOk(CS)) {
                            VisitID = core.db.csGetInteger(CS, "VisitID");
                            Panel = Panel + "<tr class=\"" + RowColor + "\">";
                            Panel = Panel + "<td align=\"left\">" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + genericController.encodeHTML(core.appConfig.adminRoute + "?" + RequestNameAdminForm + "=" + AdminFormReports + "&rid=16&MemberID=" + core.db.csGetInteger(CS, "MemberID")) + "\">" + core.db.csGet(CS, "MemberName") + "</A></span></td>";
                            Panel = Panel + "<td align=\"left\">" + SpanClassAdminNormal + core.db.csGet(CS, "Remote_Addr") + "</span></td>";
                            Panel = Panel + "<td align=\"left\">" + SpanClassAdminNormal + core.db.csGetDate(CS, "LastVisitTime").ToString("") + "</span></td>";
                            Panel = Panel + "<td align=\"right\">" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + core.appConfig.adminRoute + "?" + RequestNameAdminForm + "=" + AdminFormReports + "&rid=10&VisitID=" + VisitID + "\">" + core.db.csGet(CS, "PageVisits") + "</A></span></td>";
                            Panel = Panel + "<td align=\"right\">" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + core.appConfig.adminRoute + "?" + RequestNameAdminForm + "=" + AdminFormReports + "&rid=17&VisitID=" + VisitID + "\">" + VisitID + "</A></span></td>";
                            Panel = Panel + "<td align=\"left\">" + SpanClassAdminNormal + "&nbsp;" + core.db.csGetText(CS, "referer") + "</span></td>";
                            Panel = Panel + "</tr>";
                            if (RowColor == "ccPanelRowEven") {
                                RowColor = "ccPanelRowOdd";
                            } else {
                                RowColor = "ccPanelRowEven";
                            }
                            core.db.csGoNext(CS);
                        }
                        Panel = Panel + "</table>";
                    }
                    core.db.csClose(ref CS);
                    Stream.Add(core.html.getPanel(Panel, "ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 0));
                }
                Stream.Add("</td></tr></table>");
                Stream.Add(core.html.formEnd());
                //
                tempGetForm_QuickStats = Stream.Text;
                core.html.addTitle("Quick Stats");
                return tempGetForm_QuickStats;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            handleLegacyClassError3("GetForm_QuickStats");
            //
            return tempGetForm_QuickStats;
        }
        //
        //========================================================================
        //   Print the Topic Rules section of any edit form
        //========================================================================
        //
        //Private Function GetForm_Edit_TopicRules() As String
        //    On Error GoTo //ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.GetForm_Edit_TopicRules")
        //    '
        //    Dim SQL As String
        //    Dim CS as integer
        //    Dim MembershipCount as integer
        //    Dim MembershipSize as integer
        //    Dim MembershipPointer as integer
        //    Dim SectionName As String
        //    Dim TopicCount as integer
        //    Dim Membership() as integer
        //    Dim f As New fastStringClass
        //    Dim Checked As Boolean
        //    Dim TableID as integer
        //    Dim Adminui As New adminUIclass(core)
        //    '
        //    If AdminContent.AllowTopicRules Then
        //        '
        //        ' ----- can not use common call
        //        '       problem, TopicRules has 2 primary content keys (ContentID and RecordID)
        //        '       if we changed it to only use ContentRecordKey, we could use that as the only primary key.
        //        '
        //        ' ----- Gather all the topics to which this member belongs
        //        '
        //        MembershipCount = 0
        //        MembershipSize = 0
        //        If EditRecord.ID <> 0 Then
        //            SQL = "SELECT ccTopicRules.TopicID AS TopicID FROM (ccContent LEFT JOIN ccTopicRules ON ccContent.ID = ccTopicRules.ContentID) LEFT JOIN ccTables ON ccContent.ContentTableID = ccTables.ID WHERE (((ccTables.Name)=" & encodeSQLText(AdminContent.ContentTableName) & ") AND ((ccTopicRules.RecordID)=" & EditRecord.ID & ") AND ((ccContent.Active)<>0) AND ((ccTopicRules.Active)<>0));"
        //
        //            'SQL = "SELECT ccTopicRules.TopicID as ID" _
        //             '   & " FROM ccContent LEFT JOIN ccTopicRules ON ccContent.ID = ccTopicRules.ContentID" _
        //              '  & " WHERE (((ccContent.ContentTablename)=" & encodeSQLText(AdminContent.ContentTableName) & ") AND ((ccTopicRules.RecordID)=" & EditRecord.ID & ") AND ((ccContent.Active)<>0) AND ((ccTopicRules.Active)<>0))"
        //            CS = core.app_openCsSql_Rev_Internal("Default", SQL)
        //            If core.app.csv_IsCSOK(CS) Then
        //                If True Then
        //                    MembershipSize = 10
        //                    ReDim Membership(MembershipSize)
        //                    Do While core.app.csv_IsCSOK(CS)
        //                        If MembershipCount >= MembershipSize Then
        //                            MembershipSize = MembershipSize + 10
        //                            ReDim Preserve Membership(MembershipSize)
        //                            End If
        //                        Membership(MembershipCount) = core.app.cs_getInteger(CS, "TopicID")
        //                        MembershipCount = MembershipCount + 1
        //                        Call core.app.nextCSRecord(CS)
        //                        Loop
        //                    End If
        //                End If
        //            core.main_CloseCS (CS)
        //            End If
        //        '
        //        ' ----- Gather all the topics, sorted by ContentName (no topics, skip section)
        //        '
        //        SQL = "SELECT ccTopics.ID AS ID, ccContent.Name AS SectionName, ccTopics.Name AS TopicName, ccTopics.SortOrder" _
        //            & " FROM ccTopics LEFT JOIN ccContent ON ccTopics.ContentControlID = ccContent.ID" _
        //            & " Where (((ccTopics.Active) <> " & SQLFalse & ") And ((ccContent.Active) <> " & SQLFalse & "))" _
        //            & " GROUP BY ccTopics.ID, ccContent.Name, ccTopics.Name, ccTopics.SortOrder" _
        //            & " ORDER BY ccContent.Name, ccTopics.SortOrder"
        //        CS = core.app_openCsSql_Rev_Internal("Default", SQL)
        //        If core.app.csv_IsCSOK(CS) Then
        //            If True Then
        //                '
        //                ' ----- Open the panel
        //                '
        //                Call f.Add(AdminUI.EditTableOpen)
        //                SectionName = ""
        //                TopicCount = 0
        //                Do While core.app.csv_IsCSOK(CS)
        //                    f.Add( "<tr>"
        //                    If SectionName <> core.app.cs_get(CS, "SectionName") Then
        //                        '
        //                        ' ----- create the next content Topic row
        //                        '
        //                        SectionName = core.app.cs_get(CS, "SectionName")
        //                        Call f.Add("<td class=""ccAdminEditCaption"">" & SectionName & "</td>")
        //                    Else
        //                        Call f.Add("<td class=""ccAdminEditCaption"">&nbsp;</td>")
        //                    End If
        //                    Call f.Add("<td class=""ccAdminEditField"">")
        //                    Checked = False
        //                    If MembershipCount <> 0 Then
        //                        For MembershipPointer = 0 To MembershipCount - 1
        //                            If Membership(MembershipPointer) = core.app.cs_getInteger(CS, "ID") Then
        //                                Checked = True
        //                                Exit For
        //                            End If
        //                        Next
        //                    End If
        //                    If editrecord.read_only And Not Checked Then
        //                        f.Add( "<input type=""checkbox"" disabled>"
        //                    ElseIf editrecord.read_only Then
        //                        f.Add( "<input type=""checkbox"" disabled checked>"
        //                        f.Add( "<input type=hidden name=""Topic" & TopicCount & """ value=1>"
        //                    ElseIf Checked Then
        //                        f.Add( "<input type=""checkbox"" name=""Topic" & TopicCount & """ checked>"
        //                    Else
        //                        f.Add( "<input type=""checkbox"" name=""Topic" & TopicCount & """>"
        //                    End If
        //                    f.Add( "<input type=""hidden"" name=""TopicID" & TopicCount & """ value=""" & core.app.cs_get(CS, "ID") & """>"
        //                    f.Add( SpanClassAdminNormal & core.app.cs_get(CS, "TopicName") & "</span></td>"
        //                    f.Add( "</tr>"
        //                    '
        //                    TopicCount = TopicCount + 1
        //                    Call core.app.nextCSRecord(CS)
        //                Loop
        //                f.Add( vbCrLf & "<input type=""hidden"" name=""TopicCount"" value=""" & TopicCount & """>"
        //                f.Add( AdminUI.EditTableClose
        //                '
        //                ' ----- close the panel
        //                '
        //                GetForm_Edit_TopicRules = AdminUI.GetEditPanel( (Not AllowAdminTabs), "Topic Rules", "This content is associated with the following topics", f.Text)
        //                EditSectionPanelCount = EditSectionPanelCount + 1
        //                '
        //                End If
        //            End If
        //        Call core.app.closeCS(CS)
        //    End If
        //    '''Dim th as integer: Exit Function
        //    '
        ////ErrorTrap:
        //    Call HandleClassTrapErrorBubble("GetForm_Edit_TopicRules")
        //End Function
        //
        //========================================================================
        //   Print the Topic Rules section of any edit form
        //========================================================================
        //
        private string GetForm_Edit_LinkAliases(cdefModel adminContent, editRecordClass editRecord, bool readOnlyField) {
            string tempGetForm_Edit_LinkAliases = null;
            try {
                //
                int LinkCnt = 0;
                string LinkList = "";
                stringBuilderLegacyController f = new stringBuilderLegacyController();
                adminUIController Adminui = new adminUIController(core);
                string linkAlias = null;
                string Link = null;
                int CS = 0;
                string tabContent = null;
                string TabDescription;
                //
                //
                // Link Alias value from the admin data
                //
                TabDescription = "Link Aliases are URLs used for this content that are more friendly to users and search engines. If you set the Link Alias field, this name will be used on the URL for this page. If you leave the Link Alias blank, the page name will be used. Below is a list of names that have been used previously and are still active. All of these entries when used in the URL will resolve to this page. The first entry in this list will be used to create menus on the site. To move an entry to the top, type it into the Link Alias field and save.";
                if (!core.siteProperties.allowLinkAlias) {
                    //
                    // Disabled
                    //
                    tabContent = "&nbsp;";
                    TabDescription = "<p>The Link Alias feature is currently disabled. To enable Link Aliases, check the box marked 'Allow Link Alias' on the Page Settings page found on the Navigator under 'Settings'.</p><p>" + TabDescription + "</p>";
                } else {
                    //
                    // Link Alias Field
                    //
                    linkAlias = "";
                    if (adminContent.fields.ContainsKey("linkalias")) {
                        linkAlias = genericController.encodeText(editRecord.fieldsLc["linkalias"].value);
                    }
                    f.Add("<tr><td class=\"ccAdminEditCaption\">" + SpanClassAdminSmall + "Link Alias</td>");
                    f.Add("<td class=\"ccAdminEditField\" align=\"left\" colspan=\"2\">" + SpanClassAdminNormal);
                    if (readOnlyField) {
                        f.Add(linkAlias);
                    } else {
                        f.Add(core.html.inputText("LinkAlias", linkAlias));
                    }
                    f.Add("</span></td></tr>");
                    //
                    // Override Duplicates
                    //
                    f.Add("<tr><td class=\"ccAdminEditCaption\">" + SpanClassAdminSmall + "Override Duplicates</td>");
                    f.Add("<td class=\"ccAdminEditField\" align=\"left\" colspan=\"2\">" + SpanClassAdminNormal);
                    if (readOnlyField) {
                        f.Add("No");
                    } else {
                        f.Add(core.html.inputCheckbox("OverrideDuplicate", false));
                    }
                    f.Add("</span></td></tr>");
                    //
                    // Table of old Link Aliases
                    //
                    Link = core.doc.main_GetPageDynamicLink(editRecord.id, false);
                    CS = core.db.csOpen("Link Aliases", "pageid=" + editRecord.id, "ID Desc", true, 0, false, false, "name");
                    while (core.db.csOk(CS)) {
                        LinkList = LinkList + "<div style=\"margin-left:4px;margin-bottom:4px;\">" + genericController.encodeHTML(core.db.csGetText(CS, "name")) + "</div>";
                        LinkCnt = LinkCnt + 1;
                        core.db.csGoNext(CS);
                    }
                    core.db.csClose(ref CS);
                    if (LinkCnt > 0) {
                        f.Add("<tr><td class=\"ccAdminEditCaption\">" + SpanClassAdminSmall + "Previous Link Alias List</td>");
                        f.Add("<td class=\"ccAdminEditField\" align=\"left\" colspan=\"2\">" + SpanClassAdminNormal);
                        f.Add(LinkList);
                        f.Add("</span></td></tr>");
                    }
                    tabContent = Adminui.EditTableOpen + f.Text + Adminui.EditTableClose;
                }
                //
                tempGetForm_Edit_LinkAliases = Adminui.GetEditPanel((!allowAdminTabs), "Link Aliases", TabDescription, tabContent);
                EditSectionPanelCount = EditSectionPanelCount + 1;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Edit_LinkAliases;
        }
        //
        //========================================================================
        // Print the Email form Group associations
        //
        //   Content must conform to ccMember fields
        //========================================================================
        //
        private string GetForm_Edit_EmailRules(cdefModel adminContent, editRecordClass editRecord, bool readOnlyField) {
            string s = "";
            try {
                //
                stringBuilderLegacyController f = new stringBuilderLegacyController();
                adminUIController Adminui = new adminUIController(core);
                //
                s = core.html.getCheckList("EmailGroups", "Group Email", editRecord.id, "Groups", "Email Groups", "EmailID", "GroupID", "", "Caption");
                s = "<tr>"
                    + "<td class=\"ccAdminEditCaption\">Groups</td>"
                    + "<td class=\"ccAdminEditField\" colspan=2>" + SpanClassAdminNormal + s + "</span></td>"
                    + "</tr><tr>"
                    + "<td class=\"ccAdminEditCaption\">&nbsp;</td>"
                    + "<td class=\"ccAdminEditField\" colspan=2>" + SpanClassAdminNormal + "[<a href=?cid=" + cdefModel.getContentId(core, "Groups") + " target=_blank>Manage Groups</a>]</span></td>"
                    + "</tr>";
                s = Adminui.EditTableOpen + s + Adminui.EditTableClose;
                s = Adminui.GetEditPanel((!allowAdminTabs), "Email Rules", "Send email to people in these groups", s);
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return s;
        }
        //
        //========================================================================
        // Print the Email for Topic associations
        //
        //   Content must conform to ccMember fields
        //========================================================================
        //
        private string GetForm_Edit_EmailTopics(cdefModel adminContent, editRecordClass editRecord, bool readOnlyField) {
            string s = "";
            try {
                //
                stringBuilderLegacyController f = new stringBuilderLegacyController();
                adminUIController Adminui = new adminUIController(core);
                //
                s = core.html.getCheckList("EmailTopics", "Group Email", editRecord.id, "Topics", "Email Topics", "EmailID", "TopicID", "", "Name");
                s = "<tr>"
                    + "<td class=\"ccAdminEditCaption\">Topics</td>"
                    + "<td class=\"ccAdminEditField\" colspan=2>" + SpanClassAdminNormal + s + "</span></td>"
                    + "</tr><tr>"
                    + "<td class=\"ccAdminEditCaption\">&nbsp;</td>"
                    + "<td class=\"ccAdminEditField\" colspan=2>" + SpanClassAdminNormal + "[<a href=?cid=" + cdefModel.getContentId(core, "Topics") + " target=_blank>Manage Topics</a>]</span></td>"
                    + "</tr>";
                s = Adminui.EditTableOpen + s + Adminui.EditTableClose;
                s = Adminui.GetEditPanel((!allowAdminTabs), "Email Rules", "Send email to people in these groups", s);
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return s;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private string GetForm_Edit_EmailBounceStatus() {
            string tempGetForm_Edit_EmailBounceStatus = null;
            try {
                //
                stringBuilderLegacyController f = new stringBuilderLegacyController();
                string Copy = null;
                adminUIController Adminui = new adminUIController(core);
                //
                f.Add(Adminui.GetEditRow("<a href=?" + RequestNameAdminForm + "=28 target=_blank>Open in New Window</a>", "Email Control", "The settings in this section can be modified with the Email Control page."));
                f.Add(Adminui.GetEditRow(core.siteProperties.getText("EmailBounceAddress", ""), "Bounce Email Address", "All bounced emails will be sent to this address automatically. This must be a valid email account, and you should either use Contensive Bounce processing to capture the emails, or manually remove them from the account yourself."));
                f.Add(Adminui.GetEditRow(genericController.GetYesNo(genericController.encodeBoolean(core.siteProperties.getBoolean("AllowEmailBounceProcessing", false))), "Allow Bounce Email Processing", "If checked, Contensive will periodically retrieve all the email from the POP email account and take action on the membefr account that sent the email."));
                switch (core.siteProperties.getText("EMAILBOUNCEPROCESSACTION", "0")) {
                    case "1":
                        Copy = "Clear the Allow Group Email field for all members with a matching Email address";
                        break;
                    case "2":
                        Copy = "Clear all member Email addresses that match the Email address";
                        break;
                    case "3":
                        Copy = "Delete all Members with a matching Email address";
                        break;
                    default:
                        Copy = "Do Nothing";
                        break;
                }
                f.Add(Adminui.GetEditRow(Copy, "Bounce Email Action", "When an email is determined to be a bounce, this action will taken against member with that email address."));
                f.Add(Adminui.GetEditRow(core.siteProperties.getText("POPServerStatus"), "Last Email Retrieve Status", "This is the status of the last POP email retrieval attempted."));
                //
                tempGetForm_Edit_EmailBounceStatus = Adminui.GetEditPanel((!allowAdminTabs), "Bounced Email Handling", "", Adminui.EditTableOpen + f.Text + Adminui.EditTableClose);
                EditSectionPanelCount = EditSectionPanelCount + 1;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Edit_EmailBounceStatus;
        }
        //
        //========================================================================
        // Print the Member Edit form
        //
        //   Content must conform to ccMember fields
        //========================================================================
        //
        private string GetForm_Edit_MemberGroups(cdefModel adminContent, editRecordClass editRecord) {
            string tempGetForm_Edit_MemberGroups = null;
            try {
                //
                stringBuilderLegacyController f = new stringBuilderLegacyController();
                string SQL = null;
                int CS = 0;
                int MembershipCount = 0;
                int MembershipSize = 0;
                int MembershipPointer = 0;
                int PeopleContentID = 0;
                int GroupContentID = 0;
                bool CanSeeHiddenGroups = false;
                string DateExpireValue = null;
                int GroupCount = 0;
                int GroupID = 0;
                string GroupName = null;
                string GroupCaption = null;
                bool GroupActive = false;
                int[] Membership = { };
                DateTime[] DateExpires = { };
                bool[] Active = { };
                string Caption = null;
                string ReportLink = null;
                adminUIController Adminui = new adminUIController(core);
                //
                // ----- Gather all the SecondaryContent that associates to the PrimaryContent
                //
                PeopleContentID = cdefModel.getContentId(core, "People");
                GroupContentID = cdefModel.getContentId(core, "Groups");
                //
                MembershipCount = 0;
                MembershipSize = 0;
                if (true) {
                    //If EditRecord.ID <> 0 Then
                    //
                    // ----- read in the groups that this member has subscribed (exclude new member records)
                    //
                    if (editRecord.id != 0) {
                        SQL = "SELECT Active,GroupID,DateExpires"
                            + " FROM ccMemberRules"
                            + " WHERE MemberID=" + editRecord.id;
                        CS = core.db.csOpenSql_rev("Default", SQL);
                        while (core.db.csOk(CS)) {
                            if (MembershipCount >= MembershipSize) {
                                MembershipSize = MembershipSize + 10;
                                Array.Resize(ref Membership, MembershipSize + 1);
                                Array.Resize(ref Active, MembershipSize + 1);
                                Array.Resize(ref DateExpires, MembershipSize + 1);
                            }
                            Membership[MembershipCount] = core.db.csGetInteger(CS, "GroupID");
                            DateExpires[MembershipCount] = core.db.csGetDate(CS, "DateExpires");
                            Active[MembershipCount] = core.db.csGetBoolean(CS, "Active");
                            MembershipCount = MembershipCount + 1;
                            core.db.csGoNext(CS);
                        }
                        core.db.csClose(ref CS);
                    }
                    //
                    // ----- read in all the groups, sorted by ContentName
                    //
                    SQL = "SELECT ccGroups.ID AS ID, ccContent.Name AS SectionName, ccGroups.Caption AS GroupCaption, ccGroups.name AS GroupName, ccGroups.SortOrder"
                        + " FROM ccGroups LEFT JOIN ccContent ON ccGroups.ContentControlID = ccContent.ID"
                        + " Where (((ccGroups.Active) <> " + SQLFalse + ") And ((ccContent.Active) <> " + SQLFalse + "))";
                    SQL += ""
                        + " GROUP BY ccGroups.ID, ccContent.Name, ccGroups.Caption, ccGroups.name, ccGroups.SortOrder"
                        + " ORDER BY ccGroups.Caption";
                    //sql &= "" _
                    //    & " GROUP BY ccGroups.ID, ccContent.Name, ccGroups.Caption, ccGroups.name, ccGroups.SortOrder" _
                    //    & " ORDER BY ccContent.Name, ccGroups.Caption"
                    CS = core.db.csOpenSql_rev("Default", SQL);
                    //
                    // Output all the groups, with the active and dateexpires from those joined
                    //
                    f.Add(Adminui.EditTableOpen);
                    GroupCount = 0;
                    CanSeeHiddenGroups = core.doc.sessionContext.isAuthenticatedDeveloper(core);
                    while (core.db.csOk(CS)) {
                        GroupName = core.db.csGet(CS, "GroupName");
                        if ((GroupName.Left(1) != "_") || CanSeeHiddenGroups) {
                            GroupCaption = core.db.csGet(CS, "GroupCaption");
                            GroupID = core.db.csGetInteger(CS, "ID");
                            if (string.IsNullOrEmpty(GroupCaption)) {
                                GroupCaption = GroupName;
                                if (string.IsNullOrEmpty(GroupCaption)) {
                                    GroupCaption = "Group&nbsp;" + GroupID;
                                }
                            }
                            GroupActive = false;
                            DateExpireValue = "";
                            if (MembershipCount != 0) {
                                for (MembershipPointer = 0; MembershipPointer < MembershipCount; MembershipPointer++) {
                                    if (Membership[MembershipPointer] == GroupID) {
                                        GroupActive = Active[MembershipPointer];
                                        if (DateExpires[MembershipPointer] > DateTime.MinValue) {
                                            DateExpireValue = genericController.encodeText(DateExpires[MembershipPointer]);
                                        }
                                        break;
                                    }
                                }
                            }
                            ReportLink = "";
                            ReportLink = ReportLink + "[<a href=\"?af=4&cid=" + GroupContentID + "&id=" + GroupID + "\">Edit&nbsp;Group</a>]";
                            if (GroupID > 0) {
                                ReportLink = ReportLink + "&nbsp;[<a href=\"?" + RequestNameAdminForm + "=12&rid=35&recordid=" + GroupID + "\">Group&nbsp;Report</a>]";
                            }
                            //
                            if (GroupCount == 0) {
                                Caption = SpanClassAdminSmall + "Groups</span>";
                            } else {
                                Caption = "&nbsp;";
                            }
                            f.Add("<tr><td class=\"ccAdminEditCaption\">" + Caption + "</td>");
                            f.Add("<td class=\"ccAdminEditField\">");
                            f.Add("<table border=0 cellpadding=0 cellspacing=0 width=\"100%\" ><tr>");
                            f.Add("<td width=\"40%\">" + core.html.inputHidden("Memberrules." + GroupCount + ".ID", GroupID) + core.html.inputCheckbox("MemberRules." + GroupCount, GroupActive) + GroupCaption + "</td>");
                            f.Add("<td width=\"30%\"> Expires " + core.html.inputText("MemberRules." + GroupCount + ".DateExpires", DateExpireValue, 1, 20) + "</td>");
                            f.Add("<td width=\"30%\">" + ReportLink + "</td>");
                            f.Add("</tr></table>");
                            f.Add("</td></tr>");
                            GroupCount = GroupCount + 1;
                        }
                        core.db.csGoNext(CS);
                    }
                    core.db.csClose(ref CS);
                }
                if (GroupCount == 0) {
                    //If EditRecord.ID = 0 Then
                    //    F.Add( "<tr>" _
                    //        & "<td valign=middle align=right>" & SpanClassAdminSmall & "Groups</span></td>" _
                    //        & "<td>" & SpanClassAdminNormal & "Groups will be available after this record is saved</span></td>" _
                    //        & "</tr>"
                    //ElseIf GroupCount = 0 Then
                    f.Add("<tr><td valign=middle align=right>" + SpanClassAdminSmall + "Groups</span></td><td>" + SpanClassAdminNormal + "There are currently no groups defined</span></td></tr>");
                } else {
                    f.Add("<input type=\"hidden\" name=\"MemberRules.RowCount\" value=\"" + GroupCount + "\">");
                }
                f.Add("<tr>");
                f.Add("<td class=\"ccAdminEditCaption\">&nbsp;</td>");
                f.Add("<td class=\"ccAdminEditField\">" + SpanClassAdminNormal + "[<a href=?cid=" + cdefModel.getContentId(core, "Groups") + " target=_blank>Manage Groups</a>]</span></td>");
                f.Add("</tr>");

                tempGetForm_Edit_MemberGroups = Adminui.GetEditPanel((!allowAdminTabs), "Group Membership", "This person is a member of these groups", Adminui.EditTableOpen + f.Text + Adminui.EditTableClose);
                EditSectionPanelCount = EditSectionPanelCount + 1;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Edit_MemberGroups;
        }
        //
        //========================================================================
        //   Special case tab for Layout records
        //========================================================================
        //
        private string GetForm_Edit_LayoutReports(cdefModel adminContent, editRecordClass editRecord) {
            string tempGetForm_Edit_LayoutReports = null;
            try {
                //
                stringBuilderLegacyController FastString = null;
                adminUIController Adminui = new adminUIController(core);
                //
                FastString = new stringBuilderLegacyController();
                FastString.Add("<tr>");
                FastString.Add("<td valign=\"top\" align=\"right\">&nbsp;</td>");
                FastString.Add("<td colspan=\"2\" class=\"ccAdminEditField\" align=\"left\">" + SpanClassAdminNormal);
                FastString.Add("<ul class=\"ccList\">");
                FastString.Add("<li class=\"ccListItem\"><a target=\"_blank\" href=\"/preview?layout=" + editRecord.id + "\">Preview this layout</A></LI>");
                FastString.Add("</ul>");
                FastString.Add("</span></td></tr>");
                tempGetForm_Edit_LayoutReports = Adminui.GetEditPanel((!allowAdminTabs), "Contensive Reporting", "", Adminui.EditTableOpen + FastString.Text + Adminui.EditTableClose);
                EditSectionPanelCount = EditSectionPanelCount + 1;
                FastString = null;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Edit_LayoutReports;
        }
        //
        //========================================================================
        //   Special case tab for People records
        //========================================================================
        //
        private string GetForm_Edit_MemberReports(cdefModel adminContent, editRecordClass editRecord) {
            string tempGetForm_Edit_MemberReports = null;
            try {
                //
                stringBuilderLegacyController FastString = null;
                adminUIController Adminui = new adminUIController(core);
                //
                FastString = new stringBuilderLegacyController();
                FastString.Add("<tr>");
                FastString.Add("<td valign=\"top\" align=\"right\">&nbsp;</td>");
                FastString.Add("<td colspan=\"2\" class=\"ccAdminEditField\" align=\"left\">" + SpanClassAdminNormal);
                FastString.Add("<ul class=\"ccList\">");
                FastString.Add("<li class=\"ccListItem\"><a target=\"_blank\" href=\"/" + core.appConfig.adminRoute + "?" + RequestNameAdminForm + "=" + AdminFormReports + "&rid=3&MemberID=" + editRecord.id + "&DateTo=" + encodeInteger(Math.Floor(encodeNumber(core.doc.profileStartTime.ToOADate()))) + "&DateFrom=" + (encodeInteger(Math.Floor(encodeNumber(core.doc.profileStartTime.ToOADate()))) - 365) + "\">All visits from this person</A></LI>");
                FastString.Add("<li class=\"ccListItem\"><a target=\"_blank\" href=\"/" + core.appConfig.adminRoute + "?" + RequestNameAdminForm + "=" + AdminFormReports + "&rid=13&MemberID=" + editRecord.id + "&DateTo=" + Math.Floor(encodeNumber(core.doc.profileStartTime.ToOADate())) + "&DateFrom=" + Math.Floor(encodeNumber(core.doc.profileStartTime.ToOADate()) - 365) + "\">All orders from this person</A></LI>");
                FastString.Add("</ul>");
                FastString.Add("</span></td></tr>");
                tempGetForm_Edit_MemberReports = Adminui.GetEditPanel((!allowAdminTabs), "Contensive Reporting", "", Adminui.EditTableOpen + FastString.Text + Adminui.EditTableClose);
                EditSectionPanelCount = EditSectionPanelCount + 1;
                FastString = null;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Edit_MemberReports;
        }
        //
        //========================================================================
        //   Print the path Rules section of the path edit form
        //========================================================================
        //
        private string GetForm_Edit_PageContentBlockRules(cdefModel adminContent, editRecordClass editRecord) {
            string tempGetForm_Edit_PageContentBlockRules = null;
            try {
                //
                stringBuilderLegacyController f = new stringBuilderLegacyController();
                string GroupList = null;
                string[] GroupSplit = null;
                int Ptr = 0;
                int IDPtr = 0;
                int IDEndPtr = 0;
                int GroupID = 0;
                string ReportLink = null;
                adminUIController Adminui = new adminUIController(core);
                //
                GroupList = core.html.getCheckList2("PageContentBlockRules", adminContent.Name, editRecord.id, "Groups", "Page Content Block Rules", "RecordID", "GroupID", "", "Caption", false);
                GroupSplit = GroupList.Split(new[] { "<br>" }, StringSplitOptions.None);
                for (Ptr = 0; Ptr <= GroupSplit.GetUpperBound(0); Ptr++) {
                    GroupID = 0;
                    IDPtr = GroupSplit[Ptr].IndexOf("value=");
                    if (IDPtr > 0) {
                        IDEndPtr = genericController.vbInstr(IDPtr, GroupSplit[Ptr], ">");
                        if (IDEndPtr > 0) {
                            GroupID = genericController.encodeInteger(GroupSplit[Ptr].Substring(IDPtr + 5, IDEndPtr - IDPtr - 6));
                        }
                    }
                    if (GroupID > 0) {
                        ReportLink = "[<a href=\"?" + RequestNameAdminForm + "=12&rid=35&recordid=" + GroupID + "\" target=_blank>Group&nbsp;Report</a>]";
                    } else {
                        ReportLink = "&nbsp;";
                    }
                    f.Add("<tr><td>&nbsp;</td><td class=\"ccAdminEditField\" align=left>" + SpanClassAdminNormal + GroupSplit[Ptr] + "</span></td><td class=\"ccAdminEditField\" align=center>" + ReportLink + "</td></tr>");
                }
                tempGetForm_Edit_PageContentBlockRules = Adminui.GetEditPanel((!allowAdminTabs), "Content Blocking", "If content is blocked, select groups that have access to this content", Adminui.EditTableOpen + f.Text + Adminui.EditTableClose);
                EditSectionPanelCount = EditSectionPanelCount + 1;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Edit_PageContentBlockRules;
        }
        //
        //========================================================================
        //   Print the path Rules section of the path edit form
        //========================================================================
        //
        private string GetForm_Edit_LibraryFolderRules(cdefModel adminContent, editRecordClass editRecord) {
            string tempGetForm_Edit_LibraryFolderRules = null;
            try {
                //
                string Copy = null;
                stringBuilderLegacyController f = new stringBuilderLegacyController();
                string GroupList = null;
                string[] GroupSplit = null;
                int Ptr = 0;
                int IDPtr = 0;
                int IDEndPtr = 0;
                int GroupID = 0;
                string ReportLink = null;
                adminUIController Adminui = new adminUIController(core);
                //
                GroupList = core.html.getCheckList2("LibraryFolderRules", adminContent.Name, editRecord.id, "Groups", "Library Folder Rules", "FolderID", "GroupID", "", "Caption");
                GroupSplit = stringSplit(GroupList, "<br>");
                for (Ptr = 0; Ptr <= GroupSplit.GetUpperBound(0); Ptr++) {
                    GroupID = 0;
                    IDPtr = GroupSplit[Ptr].IndexOf("value=");
                    if (IDPtr > 0) {
                        IDEndPtr = genericController.vbInstr(IDPtr, GroupSplit[Ptr], ">");
                        if (IDEndPtr > 0) {
                            GroupID = genericController.encodeInteger(GroupSplit[Ptr].Substring(IDPtr + 5, IDEndPtr - IDPtr - 6));
                        }
                    }
                    if (GroupID > 0) {
                        ReportLink = "[<a href=\"?" + RequestNameAdminForm + "=12&rid=35&recordid=" + GroupID + "\" target=_blank>Group&nbsp;Report</a>]";
                    } else {
                        ReportLink = "&nbsp;";
                    }
                    f.Add("<tr><td>&nbsp;</td><td class=\"ccAdminEditField\" align=left>" + SpanClassAdminNormal + GroupSplit[Ptr] + "</span></td><td class=\"ccAdminEditField\" align=center>" + ReportLink + "</td></tr>");
                }
                Copy = "Select groups who have authoring access within this folder. This means if you are in this group you can upload files, delete files, create folders and delete folders within this folder and any subfolders.";
                tempGetForm_Edit_LibraryFolderRules = Adminui.GetEditPanel((!allowAdminTabs), "Folder Permissions", Copy, Adminui.EditTableOpen + f.Text + Adminui.EditTableClose);
                EditSectionPanelCount = EditSectionPanelCount + 1;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Edit_LibraryFolderRules;
        }
        //
        //========================================================================
        // Print the Group Rules section for Content Edit form
        //   Group rules show which groups have authoring rights to a content
        //
        //   adminContent.id is the ContentID of the Content Definition being edited
        //   EditRecord.ContentID is the ContentControlID of the Edit Record
        //========================================================================
        //
        private string GetForm_Edit_GroupRules(cdefModel adminContent, editRecordClass editRecord) {
            string tempGetForm_Edit_GroupRules = null;
            try {
                //
                string SQL = null;
                int CS = 0;
                int GroupRulesCount = 0;
                int GroupRulesSize = 0;
                int GroupRulesPointer = 0;
                string SectionName = null;
                string GroupName = null;
                int GroupCount = 0;
                bool GroupFound = false;
                GroupRuleType[] GroupRules = { };
                stringBuilderLegacyController FastString = null;
                adminUIController Adminui = new adminUIController(core);
                //
                // ----- Open the panel
                //
                FastString = new stringBuilderLegacyController();
                //
                //Call core.main_PrintPanelTop("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                //Call call FastString.Add(adminui.EditTableOpen)
                //
                // ----- Gather all the groups which have authoring rights to the content
                //
                GroupRulesCount = 0;
                GroupRulesSize = 0;
                if (editRecord.id != 0) {
                    SQL = "SELECT ccGroups.ID AS ID, ccGroupRules.AllowAdd as allowadd, ccGroupRules.AllowDelete as allowdelete"
                        + " FROM ccGroups LEFT JOIN ccGroupRules ON ccGroups.ID = ccGroupRules.GroupID"
                        + " WHERE (((ccGroupRules.ContentID)=" + editRecord.id + ") AND ((ccGroupRules.Active)<>0) AND ((ccGroups.Active)<>0))";
                    CS = core.db.csOpenSql_rev("Default", SQL);
                    if (core.db.csOk(CS)) {
                        if (true) {
                            GroupRulesSize = 100;
                            GroupRules = new GroupRuleType[GroupRulesSize + 1];
                            while (core.db.csOk(CS)) {
                                if (GroupRulesCount >= GroupRulesSize) {
                                    GroupRulesSize = GroupRulesSize + 100;
                                    Array.Resize(ref GroupRules, GroupRulesSize + 1);
                                }
                                GroupRules[GroupRulesCount].GroupID = core.db.csGetInteger(CS, "ID");
                                GroupRules[GroupRulesCount].AllowAdd = core.db.csGetBoolean(CS, "AllowAdd");
                                GroupRules[GroupRulesCount].AllowDelete = core.db.csGetBoolean(CS, "AllowDelete");
                                GroupRulesCount = GroupRulesCount + 1;
                                core.db.csGoNext(CS);
                            }
                        }
                    }
                }
                core.db.csClose(ref CS);
                //
                // ----- Gather all the groups, sorted by ContentName
                //
                SQL = "SELECT ccGroups.ID AS ID, ccContent.Name AS SectionName, ccGroups.Name AS GroupName, ccGroups.Caption AS GroupCaption, ccGroups.SortOrder"
                    + " FROM ccGroups LEFT JOIN ccContent ON ccGroups.ContentControlID = ccContent.ID"
                    + " Where (((ccGroups.Active) <> " + SQLFalse + ") And ((ccContent.Active) <> " + SQLFalse + "))"
                    + " GROUP BY ccGroups.ID, ccContent.Name, ccGroups.Name, ccGroups.Caption, ccGroups.SortOrder"
                    + " ORDER BY ccContent.Name, ccGroups.Caption, ccGroups.SortOrder";
                CS = core.db.csOpenSql_rev("Default", SQL);
                if (!core.db.csOk(CS)) {
                    FastString.Add("\r\n<tr><td colspan=\"3\">" + SpanClassAdminSmall + "There are no active groups</span></td></tr>");
                } else {
                    if (true) {
                        //Call FastString.Add(vbCrLf & "<tr><td colspan=""3"" class=""ccAdminEditSubHeader"">Groups with authoring access</td></tr>")
                        SectionName = "";
                        GroupCount = 0;
                        while (core.db.csOk(CS)) {
                            GroupName = core.db.csGet(CS, "GroupCaption");
                            if (string.IsNullOrEmpty(GroupName)) {
                                GroupName = core.db.csGet(CS, "GroupName");
                            }
                            FastString.Add("<tr>");
                            if (SectionName != core.db.csGet(CS, "SectionName")) {
                                //
                                // ----- create the next section
                                //
                                SectionName = core.db.csGet(CS, "SectionName");
                                FastString.Add("<td valign=\"top\" align=\"right\">" + SpanClassAdminSmall + SectionName + "</td>");
                            } else {
                                FastString.Add("<td valign=\"top\" align=\"right\">&nbsp;</td>");
                            }
                            FastString.Add("<td class=\"ccAdminEditField\" align=\"left\" colspan=\"2\">" + SpanClassAdminSmall);
                            GroupFound = false;
                            if (GroupRulesCount != 0) {
                                for (GroupRulesPointer = 0; GroupRulesPointer < GroupRulesCount; GroupRulesPointer++) {
                                    if (GroupRules[GroupRulesPointer].GroupID == core.db.csGetInteger(CS, "ID")) {
                                        GroupFound = true;
                                        break;
                                    }
                                }
                            }
                            FastString.Add("<input type=\"hidden\" name=\"GroupID" + GroupCount + "\" value=\"" + core.db.csGet(CS, "ID") + "\">");
                            FastString.Add("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"400\"><tr>");
                            if (GroupFound) {
                                FastString.Add("<td width=\"200\">" + SpanClassAdminSmall + core.html.inputCheckbox("Group" + GroupCount, true) + GroupName + "</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + core.html.inputCheckbox("GroupRuleAllowAdd" + GroupCount, GroupRules[GroupRulesPointer].AllowAdd) + " Allow Add</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + core.html.inputCheckbox("GroupRuleAllowDelete" + GroupCount, GroupRules[GroupRulesPointer].AllowDelete) + " Allow Delete</span></td>");
                            } else {
                                FastString.Add("<td width=\"200\">" + SpanClassAdminSmall + core.html.inputCheckbox("Group" + GroupCount, false) + GroupName + "</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + core.html.inputCheckbox("GroupRuleAllowAdd" + GroupCount, false) + " Allow Add</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + core.html.inputCheckbox("GroupRuleAllowDelete" + GroupCount, false) + " Allow Delete</span></td>");
                            }
                            FastString.Add("</tr></table>");
                            FastString.Add("</span></td>");
                            FastString.Add("</tr>");
                            GroupCount = GroupCount + 1;
                            core.db.csGoNext(CS);
                        }
                        FastString.Add("\r\n<input type=\"hidden\" name=\"GroupCount\" value=\"" + GroupCount + "\">");
                    }
                }
                core.db.csClose(ref CS);
                //
                // ----- close the panel
                //
                //Call FastString.Add(adminui.EditTableClose)
                //Call core.main_PrintPanelBottom("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                //
                tempGetForm_Edit_GroupRules = Adminui.GetEditPanel((!allowAdminTabs), "Authoring Permissions", "The following groups can edit this content.", Adminui.EditTableOpen + FastString.Text + Adminui.EditTableClose);
                EditSectionPanelCount = EditSectionPanelCount + 1;
                FastString = null;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Edit_GroupRules;
        }
        //
        //========================================================================
        //   Get all content authorable by the current group
        //========================================================================
        //
        private string GetForm_Edit_ContentGroupRules(cdefModel adminContent, editRecordClass editRecord) {
            string tempGetForm_Edit_ContentGroupRules = null;
            try {
                //
                string SQL = null;
                int CS = 0;
                int ContentGroupRulesCount = 0;
                int ContentGroupRulesSize = 0;
                int ContentGroupRulesPointer = 0;
                string ContentName = null;
                int ContentCount = 0;
                bool ContentFound = false;
                ContentGroupRuleType[] ContentGroupRules = { };
                stringBuilderLegacyController FastString = null;
                adminUIController Adminui = new adminUIController(core);
                //
                if (core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                    //
                    // ----- Open the panel
                    //
                    FastString = new stringBuilderLegacyController();
                    //
                    //Call core.main_PrintPanelTop("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                    //Call call FastString.Add(adminui.EditTableOpen)
                    //
                    // ----- Gather all the groups which have authoring rights to the content
                    //
                    ContentGroupRulesCount = 0;
                    ContentGroupRulesSize = 0;
                    if (editRecord.id != 0) {
                        SQL = "SELECT ccContent.ID AS ID, ccGroupRules.AllowAdd as allowadd, ccGroupRules.AllowDelete as allowdelete"
                            + " FROM ccContent LEFT JOIN ccGroupRules ON ccContent.ID = ccGroupRules.ContentID"
                            + " WHERE (((ccGroupRules.GroupID)=" + editRecord.id + ") AND ((ccGroupRules.Active)<>0) AND ((ccContent.Active)<>0))";
                        CS = core.db.csOpenSql_rev("Default", SQL);
                        if (core.db.csOk(CS)) {
                            ContentGroupRulesSize = 100;
                            ContentGroupRules = new ContentGroupRuleType[ContentGroupRulesSize + 1];
                            while (core.db.csOk(CS)) {
                                if (ContentGroupRulesCount >= ContentGroupRulesSize) {
                                    ContentGroupRulesSize = ContentGroupRulesSize + 100;
                                    Array.Resize(ref ContentGroupRules, ContentGroupRulesSize + 1);
                                }
                                ContentGroupRules[ContentGroupRulesCount].ContentID = core.db.csGetInteger(CS, "ID");
                                ContentGroupRules[ContentGroupRulesCount].AllowAdd = core.db.csGetBoolean(CS, "AllowAdd");
                                ContentGroupRules[ContentGroupRulesCount].AllowDelete = core.db.csGetBoolean(CS, "AllowDelete");
                                ContentGroupRulesCount = ContentGroupRulesCount + 1;
                                core.db.csGoNext(CS);
                            }
                        }
                    }
                    core.db.csClose(ref CS);
                    //
                    // ----- Gather all the content, sorted by ContentName
                    //
                    SQL = "SELECT ccContent.ID AS ID, ccContent.Name AS ContentName, ccContent.SortOrder"
                        + " FROM ccContent"
                        + " Where ccContent.Active<>0"
                        + " ORDER BY ccContent.Name";
                    CS = core.db.csOpenSql_rev("Default", SQL);
                    if (!core.db.csOk(CS)) {
                        FastString.Add("\r\n<tr><td colspan=\"3\">" + SpanClassAdminSmall + "There are no active groups</span></td></tr>");
                    } else {
                        ContentCount = 0;
                        while (core.db.csOk(CS)) {
                            ContentName = core.db.csGet(CS, "ContentName");
                            FastString.Add("<tr>");
                            FastString.Add("<td valign=\"top\" align=\"right\">&nbsp;</td>");
                            FastString.Add("<td class=\"ccAdminEditField\" align=\"left\" colspan=\"2\">" + SpanClassAdminSmall);
                            ContentFound = false;
                            if (ContentGroupRulesCount != 0) {
                                for (ContentGroupRulesPointer = 0; ContentGroupRulesPointer < ContentGroupRulesCount; ContentGroupRulesPointer++) {
                                    if (ContentGroupRules[ContentGroupRulesPointer].ContentID == core.db.csGetInteger(CS, "ID")) {
                                        ContentFound = true;
                                        break;
                                    }
                                }
                            }
                            FastString.Add("<input type=\"hidden\" name=\"ContentID" + ContentCount + "\" value=\"" + core.db.csGet(CS, "ID") + "\">");
                            FastString.Add("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"400\"><tr>");
                            if (ContentFound) {
                                FastString.Add("<td width=\"200\">" + SpanClassAdminSmall + core.html.inputCheckbox("Content" + ContentCount, true) + ContentName + "</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + core.html.inputCheckbox("ContentGroupRuleAllowAdd" + ContentCount, ContentGroupRules[ContentGroupRulesPointer].AllowAdd) + " Allow Add</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + core.html.inputCheckbox("ContentGroupRuleAllowDelete" + ContentCount, ContentGroupRules[ContentGroupRulesPointer].AllowDelete) + " Allow Delete</span></td>");
                            } else {
                                FastString.Add("<td width=\"200\">" + SpanClassAdminSmall + core.html.inputCheckbox("Content" + ContentCount, false) + ContentName + "</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + core.html.inputCheckbox("ContentGroupRuleAllowAdd" + ContentCount, false) + " Allow Add</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + core.html.inputCheckbox("ContentGroupRuleAllowDelete" + ContentCount, false) + " Allow Delete</span></td>");
                            }
                            FastString.Add("</tr></table>");
                            FastString.Add("</span></td>");
                            FastString.Add("</tr>");
                            ContentCount = ContentCount + 1;
                            core.db.csGoNext(CS);
                        }
                        FastString.Add("\r\n<input type=\"hidden\" name=\"ContentCount\" value=\"" + ContentCount + "\">");
                    }
                    core.db.csClose(ref CS);
                    //
                    // ----- close the panel
                    //
                    //Call FastString.Add(adminui.EditTableClose)
                    //Call core.main_PrintPanelBottom("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                    //
                    tempGetForm_Edit_ContentGroupRules = Adminui.GetEditPanel((!allowAdminTabs), "Authoring Permissions", "This group can edit the following content.", Adminui.EditTableOpen + FastString.Text + Adminui.EditTableClose);
                    EditSectionPanelCount = EditSectionPanelCount + 1;
                    FastString = null;
                }
                return tempGetForm_Edit_ContentGroupRules;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Edit_ContentGroupRules;
        }
        //
        //========================================================================
        // MakeButton
        //   Prints the currently selected Button Type
        //   ButtonName is the ID field name given to the button object
        //   ButtonLabel is the words that appear on the button
        //   ButtonHref is the Link for the button
        //   ButtonWidth, if provided, is the width of a trans spacer.gif put under the ButtonLabel
        //   ButtonColors, colors used for the button, duh.
        //========================================================================
        //
        private string MakeButton(string ButtonName, string ButtonLabel, string ButtonHref, string ButtonWidth, string ButtonColorBase, string ButtonColorHilite, string ButtonColorShadow, bool NewWindow) {
            return MakeButtonFlat(ButtonName, ButtonLabel, ButtonHref, ButtonWidth, ButtonColorBase, ButtonColorHilite, ButtonColorShadow, NewWindow);
        }
        //
        //========================================================================
        // MakeButtonFlat
        //   Returns a Flat button string
        //   Button is a normal color, rollover changes background color only
        //========================================================================
        //
        private string MakeButtonFlat(string ButtonName, string ButtonLabel, string ButtonHref, string ButtonWidth, string ButtonColorBase, string ButtonColorHilite, string ButtonColorShadow, bool NewWindow) {
            string tempMakeButtonFlat = null;
            try {
                //
                bool IncludeWidth = false;
                //
                tempMakeButtonFlat = "";
                tempMakeButtonFlat = tempMakeButtonFlat + "<div"
                    + " ID=\"" + ButtonName + "\""
                    + " class=\"ccAdminButton\""
                    + ">";
                //
                // --- the IncludeWidth test
                //
                IncludeWidth = false;
                if (!string.IsNullOrEmpty(ButtonWidth)) {
                    if (ButtonWidth.IsNumeric()) {
                        IncludeWidth = true;
                    }
                }
                //
                // --- put guts in layer so Netscape can change colors (with mouseover and mouseout)
                //
                tempMakeButtonFlat = tempMakeButtonFlat + "<a"
                    + " href=\"" + ButtonHref + "\""
                    + " class=\"ccAdminButton\""
                    + "";
                if (NewWindow) {
                    tempMakeButtonFlat = tempMakeButtonFlat + " target=\"_blank\"";
                }
                tempMakeButtonFlat = tempMakeButtonFlat + ">";
                tempMakeButtonFlat = tempMakeButtonFlat + ButtonLabel + "</A>";
                if (IncludeWidth) {
                    tempMakeButtonFlat = tempMakeButtonFlat + "<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"" + ButtonWidth + "\" height=\"1\" >";
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempMakeButtonFlat;
        }
        //
        //========================================================================
        // GetForm_Top
        //   Prints the admin page before the content form window.
        //   After this, print the content window, then PrintFormBottom()
        //========================================================================
        //
        private string GetForm_Top(string BackgroundColor = "") {
            string return_formTop = "";
            try {
                const string AdminNavigatorGuid = "{5168964F-B6D2-4E9F-A5A8-BB1CF908A2C9}";
                string AdminNavFull = null;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string LeftSide = null;
                string RightSide = null;
                string QS = null;
                adminUIController Adminui = new adminUIController(core);
                //
                // create the with-menu version
                //
                LeftSide = core.siteProperties.getText("AdminHeaderHTML", "Contensive Administration Site");
                RightSide = core.doc.profileStartTime + "&nbsp;";
                //
                // AdminTabs
                //
                QS = core.doc.refreshQueryString;
                if (allowAdminTabs) {
                    QS = genericController.ModifyQueryString(QS, "tabs", "0", true);
                    RightSide = RightSide + getActiveImage(core.appConfig.adminRoute + "?" + QS, "Disable Tabs", "LibButtonNoTabs.GIF", "LibButtonNoTabsRev.GIF", "Disable Tabs", "16", "16", "", "", "");
                } else {
                    QS = genericController.ModifyQueryString(QS, "tabs", "1", true);
                    RightSide = RightSide + getActiveImage(core.appConfig.adminRoute + "?" + QS, "Enable Tabs", "LibButtonTabs.GIF", "LibButtonTabsRev.GIF", "Enable Tabs", "16", "16", "", "", "");
                }
                //
                // Menu Mode
                //
                QS = core.doc.refreshQueryString;
                if (MenuDepth == 0) {
                    RightSide = RightSide + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"1\" height=\"16\" >";
                    if (AdminMenuModeID == AdminMenuModeTop) {
                        QS = genericController.ModifyQueryString(QS, "mm", "1", true);
                        RightSide = RightSide + getActiveImage(core.appConfig.adminRoute + "?" + QS, "Use Navigator", "LibButtonMenuTop.GIF", "LibButtonMenuTopOver.GIF", "Use Navigator", "16", "16", "", "", "");
                    } else {
                        QS = genericController.ModifyQueryString(QS, "mm", "2", true);
                        RightSide = RightSide + getActiveImage(core.appConfig.adminRoute + "?" + QS, "Use Dropdown Menus", "LibButtonMenuLeft.GIF", "LibButtonMenuLeftOver.GIF", "Use Dropdown Menus", "16", "16", "", "", "");
                    }
                }
                //
                // Refresh Button
                //
                RightSide = RightSide + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"1\" height=\"16\" >";
                RightSide = RightSide + getActiveImage(core.appConfig.adminRoute + "?" + core.doc.refreshQueryString, "Refresh", "LibButtonRefresh.GIF", "LibButtonRefreshOver.GIF", "Refresh", "16", "16", "", "", "");
                //
                // Assemble header
                //
                Stream.Add(Adminui.GetHeader(LeftSide, RightSide));
                //
                // Menuing
                //
                if ((MenuDepth == 0) && (AdminMenuModeID == AdminMenuModeTop)) {
                    Stream.Add(GetMenuTopMode());
                }
                //
                // --- Rule to separate content
                //
                Stream.Add("\r<div style=\"border-top:1px solid white;border-bottom:1px solid black;height:2px\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=1 height=1></div>");
                //
                // --- Content Definition
                //
                AdminFormBottom = "";
                if (!((MenuDepth == 0) && (AdminMenuModeID == AdminMenuModeLeft))) {
                    //
                    // #Content is full width, no Navigator
                    //
                    Stream.Add("\r<div id=\"desktop\" class=\"ccContentCon\">");
                    //Stream.Add( "<div id=""ccContentCon"">")
                    AdminFormBottom = AdminFormBottom + "\r</div>";
                } else {
                    //
                    // -- Admin Navigator
                    AdminNavFull = core.addon.execute(addonModel.create(core, AdminNavigatorGuid), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                        addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                        errorCaption = "Admin Navigator"
                    });
                    //AdminNavFull = core.addon.execute_legacy4(AdminNavigatorGuid)
                    Stream.Add("\r<table border=0 cellpadding=0 cellspacing=0><tr>\r<td class=\"ccToolsCon\" valign=top>" + genericController.htmlIndent(AdminNavFull) + "\r</td>\r<td id=\"desktop\" class=\"ccContentCon\" valign=top>");
                    AdminFormBottom = AdminFormBottom + "</td></tr></table>";
                }
                //
                return_formTop = Stream.Text;
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return return_formTop;
        }
        //
        //========================================================================
        // Create a string with an admin style button
        //========================================================================
        //
        private string getActiveImage(string HRef, string StatusText, string Image, string ImageOver, string AltText, string Width, string Height, string BGColor, string BGColorOver, string OnClick) {
            string result = "";
            try {
                string ButtonObject = "Button" + ButtonObjectCount;
                ButtonObjectCount = ButtonObjectCount + 1;
                //
                // ----- Output the button image
                string Panel = "";
                if (!string.IsNullOrEmpty(HRef)) {
                    Panel = Panel + "<a href=\"" + HRef + "\" ";
                    if (!string.IsNullOrEmpty(OnClick)) {
                        Panel = Panel + " onclick=\"" + OnClick + "\"";
                    }
                    Panel = Panel + " onmouseOver=\""
                        + " document['" + ButtonObject + "'].imgRolln=document['" + ButtonObject + "'].src;"
                        + " document['" + ButtonObject + "'].src=document['" + ButtonObject + "'].lowsrc;"
                        + " window.status='" + StatusText + "';"
                        + " return true;\"";
                    Panel = Panel + " onmouseOut=\""
                        + " document['" + ButtonObject + "'].src=document['" + ButtonObject + "'].imgRolln;"
                        + " window.status='';"
                        + " return true;\">";
                }
                Panel = Panel + "<img"
                    + " src=\"/ccLib/images/" + Image + "\""
                    + " alt=\"" + AltText + "\""
                    + " title=\"" + AltText + "\""
                    + " id=\"" + ButtonObject + "\""
                    + " name=\"" + ButtonObject + "\""
                    + " lowsrc=\"/ccLib/images/" + ImageOver + "\""
                    + " border=0"
                    + " width=\"" + Width + "\""
                    + " height=\"" + Height + "\" >";
                if (!string.IsNullOrEmpty(HRef)) {
                    Panel = Panel + "</A>";
                }
                result = Panel;
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        // Get sql for menu
        //   if MenuContentName is blank, it will select values from either cdef
        //========================================================================
        //
        private string GetMenuSQL(string ParentCriteria, string MenuContentName) {
            string result = "";
            try {
                string iParentCriteria = null;
                string Criteria = null;
                string SelectList = null;
                List<int> editableCdefIdList = null;
                //
                Criteria = "(Active<>0)";
                if (!string.IsNullOrEmpty(MenuContentName)) {
                    //ContentControlCriteria = core.csv_GetContentControlCriteria(MenuContentName)
                    Criteria = Criteria + "AND" + cdefModel.getContentControlCriteria(core, MenuContentName);
                }
                iParentCriteria = genericController.encodeEmptyText(ParentCriteria, "");
                if (core.doc.sessionContext.isAuthenticatedDeveloper(core)) {
                    //
                    // ----- Developer
                    //
                } else if (core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                    //
                    // ----- Administrator
                    //
                    Criteria = Criteria + "AND((DeveloperOnly is null)or(DeveloperOnly=0))"
                        + "AND(ID in ("
                        + " SELECT AllowedEntries.ID"
                        + " FROM CCMenuEntries AllowedEntries LEFT JOIN ccContent ON AllowedEntries.ContentID = ccContent.ID"
                        + " Where ((ccContent.Active<>0)And((ccContent.DeveloperOnly is null)or(ccContent.DeveloperOnly=0)))"
                            + "OR(ccContent.ID Is Null)"
                        + "))";
                } else {
                    //
                    // ----- Content Manager
                    //
                    string CMCriteria = null;

                    editableCdefIdList = cdefModel.getEditableCdefIdList(core);
                    if (editableCdefIdList.Count == 0) {
                        CMCriteria = "(1=0)";
                    } else if (editableCdefIdList.Count == 1) {
                        CMCriteria = "(ccContent.ID=" + editableCdefIdList[0] + ")";
                    } else {
                        CMCriteria = "(ccContent.ID in (" + string.Join(",", editableCdefIdList) + "))";
                    }

                    Criteria = Criteria + "AND((DeveloperOnly is null)or(DeveloperOnly=0))"
                        + "AND((AdminOnly is null)or(AdminOnly=0))"
                        + "AND(ID in ("
                        + " SELECT AllowedEntries.ID"
                        + " FROM CCMenuEntries AllowedEntries LEFT JOIN ccContent ON AllowedEntries.ContentID = ccContent.ID"
                        + " Where (" + CMCriteria + "and(ccContent.Active<>0)And((ccContent.DeveloperOnly is null)or(ccContent.DeveloperOnly=0))And((ccContent.AdminOnly is null)or(ccContent.AdminOnly=0)))"
                            + "OR(ccContent.ID Is Null)"
                        + "))";
                }
                if (!string.IsNullOrEmpty(iParentCriteria)) {
                    Criteria = "(" + iParentCriteria + ")AND" + Criteria;
                }
                SelectList = "ccMenuEntries.contentcontrolid, ccMenuEntries.Name, ccMenuEntries.ID, ccMenuEntries.LinkPage, ccMenuEntries.ContentID, ccMenuEntries.NewWindow, ccMenuEntries.ParentID, ccMenuEntries.AddonID, ccMenuEntries.NavIconType, ccMenuEntries.NavIconTitle, HelpAddonID,HelpCollectionID,0 as collectionid";
                result = "select " + SelectList + " from ccMenuEntries where " + Criteria + " order by ccMenuEntries.Name";
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        // Get sql for menu
        //========================================================================
        //
        private int GetMenuCSPointer(string ParentCriteria) {
            //
            string iParentCriteria = genericController.encodeEmptyText(ParentCriteria, "");
            if (!string.IsNullOrEmpty(iParentCriteria)) {
                iParentCriteria = "(" + iParentCriteria + ")";
            }
            return core.db.csOpenSql_rev("default", GetMenuSQL(iParentCriteria, cnNavigatorEntries));
        }
        //
        //========================================================================
        // Get Menu Link
        //========================================================================
        //
        private string GetMenuLink(string LinkPage, int LinkCID) {
            string tempGetMenuLink = null;
            try {
                //
                int ContentID = 0;
                //
                if (!string.IsNullOrEmpty(LinkPage) | LinkCID != 0) {
                    tempGetMenuLink = LinkPage;
                    if (!string.IsNullOrEmpty(tempGetMenuLink)) {
                        if (tempGetMenuLink.Left(1) == "?" || tempGetMenuLink.Left(1) == "#") {
                            tempGetMenuLink = "/" + core.appConfig.adminRoute + tempGetMenuLink;
                        }
                    } else {
                        tempGetMenuLink = "/" + core.appConfig.adminRoute;
                    }
                    ContentID = genericController.encodeInteger(LinkCID);
                    if (ContentID != 0) {
                        tempGetMenuLink = genericController.modifyLinkQuery(tempGetMenuLink, "cid", ContentID.ToString(), true);
                    }
                }
                return tempGetMenuLink;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            handleLegacyClassError3("GetMenuLink");
            //
            return tempGetMenuLink;
        }
        //
        //
        //
        private void ProcessForms(cdefModel adminContent, editRecordClass editRecord) {
            try {
                //Dim th as integer
                //th = profileLogAdminMethodEnter("ProcessForms")
                //
                //Dim innovaEditor As innovaEditorAddonClassFPO
                int CS = 0;
                string EditorStyleRulesFilename = null;
                //
                if (AdminSourceForm != 0) {
                    switch (AdminSourceForm) {
                        case AdminFormReports:
                            //
                            // Reports form cancel button
                            //
                            switch (AdminButton) {
                                case ButtonCancel:
                                    AdminAction = AdminActionNop;
                                    AdminForm = AdminFormRoot;
                                    break;
                            }
                            break;
                        case AdminFormQuickStats:
                            switch (AdminButton) {
                                case ButtonCancel:
                                    AdminAction = AdminActionNop;
                                    AdminForm = AdminFormRoot;
                                    break;
                            }
                            break;
                        case AdminFormPublishing:
                            //
                            // Publish Form
                            //
                            switch (AdminButton) {
                                case ButtonCancel:
                                    AdminAction = AdminActionNop;
                                    AdminForm = AdminFormRoot;
                                    break;
                            }
                            break;
                        case AdminFormIndex:
                            switch (AdminButton) {
                                case ButtonCancel:
                                    AdminAction = AdminActionNop;
                                    AdminForm = AdminFormRoot;
                                    adminContent.Id = 0;
                                    break;
                                case ButtonClose:
                                    AdminAction = AdminActionNop;
                                    AdminForm = AdminFormRoot;
                                    adminContent.Id = 0;
                                    break;
                                case ButtonAdd:
                                    AdminAction = AdminActionNop;
                                    AdminForm = AdminFormEdit;
                                    break;
                                case ButtonFind:
                                    AdminAction = AdminActionFind;
                                    AdminForm = AdminSourceForm;
                                    break;
                                case ButtonFirst:
                                    RecordTop = 0;
                                    AdminForm = AdminSourceForm;
                                    break;
                                case ButtonPrevious:
                                    RecordTop = RecordTop - RecordsPerPage;
                                    if (RecordTop < 0) {
                                        RecordTop = 0;
                                    }
                                    AdminAction = AdminActionNop;
                                    AdminForm = AdminSourceForm;
                                    break;
                                case ButtonNext:
                                    AdminAction = AdminActionNext;
                                    AdminForm = AdminSourceForm;
                                    break;
                                case ButtonDelete:
                                    AdminAction = AdminActionDeleteRows;
                                    AdminForm = AdminSourceForm;
                                    break;
                            }
                            // end case
                            break;
                        case AdminFormEdit:
                            //
                            // Edit Form
                            //
                            switch (AdminButton) {
                                case ButtonRefresh:
                                    //
                                    // this is a test operation. need this so the user can set editor preferences without saving the record
                                    //   during refresh, the edit page is redrawn just was it was, but no save
                                    //
                                    AdminAction = AdminActionEditRefresh;
                                    AdminForm = AdminFormEdit;
                                    break;
                                case ButtonMarkReviewed:
                                    AdminAction = AdminActionMarkReviewed;
                                    AdminForm = GetForm_Close(MenuDepth, adminContent.Name, editRecord.id);
                                    break;
                                case ButtonSaveandInvalidateCache:
                                    AdminAction = AdminActionReloadCDef;
                                    AdminForm = AdminFormEdit;
                                    break;
                                case ButtonDelete:
                                case ButtonDeletePage:
                                case ButtonDeletePerson:
                                case ButtonDeleteRecord:
                                case ButtonDeleteEmail:
                                    AdminAction = AdminActionDelete;
                                    AdminForm = GetForm_Close(MenuDepth, adminContent.Name, editRecord.id);
                                    //                Case ButtonSetHTMLEdit
                                    //                    AdminAction = AdminActionSetHTMLEdit
                                    //                Case ButtonSetTextEdit
                                    //                    AdminAction = AdminActionSetTextEdit
                                    break;
                                case ButtonSave:
                                    AdminAction = AdminActionSave;
                                    AdminForm = AdminFormEdit;
                                    break;
                                case ButtonSaveAddNew:
                                    AdminAction = AdminActionSaveAddNew;
                                    AdminForm = AdminFormEdit;
                                    break;
                                case ButtonOK:
                                    AdminAction = AdminActionSave;
                                    AdminForm = GetForm_Close(MenuDepth, adminContent.Name, editRecord.id);
                                    break;
                                case ButtonCancel:
                                    AdminAction = AdminActionNop;
                                    AdminForm = GetForm_Close(MenuDepth, adminContent.Name, editRecord.id);
                                    break;
                                case ButtonSend:
                                    //
                                    // Send a Group Email
                                    //
                                    AdminAction = AdminActionSendEmail;
                                    AdminForm = AdminFormEdit;
                                    break;
                                case ButtonActivate:
                                    //
                                    // Activate (submit) a conditional Email
                                    //
                                    AdminAction = AdminActionActivateEmail;
                                    AdminForm = AdminFormEdit;
                                    break;
                                case ButtonDeactivate:
                                    //
                                    // Deactivate (clear submit) a conditional Email
                                    //
                                    AdminAction = AdminActionDeactivateEmail;
                                    AdminForm = AdminFormEdit;
                                    break;
                                case ButtonSendTest:
                                    //
                                    // Test an Email (Group, System, or Conditional)
                                    //
                                    AdminAction = AdminActionSendEmailTest;
                                    AdminForm = AdminFormEdit;
                                    //                Case ButtonSpellCheck
                                    //                    SpellCheckRequest = True
                                    //                    AdminAction = AdminActionSave
                                    //                    AdminForm = AdminFormEdit
                                    break;
                                case ButtonCreateDuplicate:
                                    //
                                    // Create a Duplicate record (for email)
                                    //
                                    AdminAction = AdminActionDuplicate;
                                    AdminForm = AdminFormEdit;
                                    break;
                            }
                            break;
                        case AdminFormStyleEditor:
                            //
                            // Process actions
                            //
                            switch (AdminButton) {
                                case ButtonSave:
                                case ButtonOK:
                                    //
                                    core.siteProperties.setProperty("Allow CSS Reset", core.docProperties.getBoolean(RequestNameAllowCSSReset));
                                    core.cdnFiles.saveFile(DynamicStylesFilename, core.docProperties.getText("StyleEditor"));
                                    if (core.docProperties.getBoolean(RequestNameInlineStyles)) {
                                        //
                                        // Inline Styles
                                        //
                                        core.siteProperties.setProperty("StylesheetSerialNumber", "0");
                                    } else {
                                        // mark to rebuild next fetch
                                        core.siteProperties.setProperty("StylesheetSerialNumber", "-1");
                                        //
                                        // Linked Styles
                                        // Bump the Style Serial Number so next fetch is not cached
                                        //
                                        //StyleSN = genericController.EncodeInteger(core.main_GetSiteProperty2("StylesheetSerialNumber", "0"))
                                        //StyleSN = StyleSN + 1
                                        //Call core.app.setSiteProperty("StylesheetSerialNumber", genericController.encodeText(StyleSN))
                                        //
                                        // Save new public stylesheet
                                        //
                                        // 11/24/3009 - style sheet processing deprecated
                                        //Call core.app.virtualFiles.SaveFile("templates\Public" & StyleSN & ".css", core.main_GetStyleSheet)
                                        //Call core.app.virtualFiles.SaveFile("templates\Public" & StyleSN & ".css", core.main_GetStyleSheetProcessed)
                                        //Call core.app.virtualFiles.SaveFile("templates\Admin" & StyleSN & ".css", core.main_GetStyleSheetDefault)
                                    }
                                    //
                                    // delete all templateid based editorstylerule files, build on-demand
                                    //
                                    EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", "0", 1, 99, 1);
                                    core.cdnFiles.deleteFile(EditorStyleRulesFilename);
                                    //
                                    CS = core.db.csOpenSql_rev("default", "select id from cctemplates");
                                    while (core.db.csOk(CS)) {
                                        EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", core.db.csGet(CS, "ID"), 1, 99, 1);
                                        core.cdnFiles.deleteFile(EditorStyleRulesFilename);
                                        core.db.csGoNext(CS);
                                    }
                                    core.db.csClose(ref CS);
                                    break;
                            }
                            //
                            // Process redirects
                            //
                            switch (AdminButton) {
                                case ButtonCancel:
                                case ButtonOK:
                                    AdminForm = AdminFormRoot;
                                    break;
                            }
                            break;
                        default:
                            // end case
                            break;
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private string GetForm_EditTitle(cdefModel adminContent, editRecordClass editRecord) {
            string tempGetForm_EditTitle = "";
            try {
                //
                if (editRecord.id == 0) {
                    tempGetForm_EditTitle = "Add an entry to " + editRecord.contentControlId_Name + TitleExtension;
                } else {
                    tempGetForm_EditTitle = "Editing Record " + editRecord.id + " in " + editRecord.contentControlId_Name + " " + TitleExtension;
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_EditTitle;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private string GetForm_EditTitleBar(cdefModel adminContent, editRecordClass editRecord) {
            adminUIController Adminui = new adminUIController(core);
            return Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), "");
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private string GetForm_EditFormStart(int AdminFormID) {
            string s = "";
            try {
                core.html.addScriptCode("var docLoaded=false", "Form loader");
                core.html.addScriptCode_onLoad("docLoaded=true;", "Form loader");
                s = core.html.formStartMultipart();
                s = genericController.vbReplace(s, ">", " onSubmit=\"cj.admin.saveEmptyFieldList('FormEmptyFieldList');\">");
                s = genericController.vbReplace(s, ">", " autocomplete=\"off\">");
                s = genericController.vbReplace(s, ">", " id=\"adminEditForm\">");
                s = s + "\r\n<input TYPE=\"hidden\" NAME=\"" + rnAdminSourceForm + "\" VALUE=\"" + AdminFormID.ToString() + "\">";
                s = s + "\r\n<input TYPE=\"hidden\" NAME=\"" + RequestNameTitleExtension + "\" VALUE=\"" + TitleExtension + "\">";
                s = s + "\r\n<input TYPE=\"hidden\" NAME=\"" + RequestNameAdminDepth + "\" VALUE=\"" + MenuDepth + "\">";
                s = s + "\r\n<input TYPE=\"hidden\" NAME=\"FormEmptyFieldList\" ID=\"FormEmptyFieldList\" VALUE=\",\">";
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return s;
        }
        //
        // true if the field is a visible user field (can display on edit form)
        //
        private bool IsVisibleUserField(bool AdminOnly, bool DeveloperOnly, bool Active, bool Authorable, string Name, string TableName) {
            bool tempIsVisibleUserField = false;
            try {
                //Private Function IsVisibleUserField( Field as CDefFieldClass, AdminOnly As Boolean, DeveloperOnly As Boolean, Active As Boolean, Authorable As Boolean) As Boolean
                //
                bool HasEditRights = false;
                //
                tempIsVisibleUserField = false;
                if ((TableName.ToLower() == "ccpagecontent") && (Name.ToLower() == "linkalias")) {
                    //
                    // ccpagecontent.linkalias is a control field that is not in control tab
                    //
                } else {
                    switch (genericController.vbUCase(Name)) {
                        case "ACTIVE":
                        case "ID":
                        case "CONTENTCONTROLID":
                        case "CREATEDBY":
                        case "DATEADDED":
                        case "MODIFIEDBY":
                        case "MODIFIEDDATE":
                        case "CREATEKEY":
                        case "CCGUID":
                            //
                            // ----- control fields are not editable user fields
                            //
                            break;
                        default:
                            //
                            // ----- test access
                            //
                            HasEditRights = true;
                            if (AdminOnly || DeveloperOnly) {
                                //
                                // field has some kind of restriction
                                //
                                if (!core.doc.sessionContext.user.Developer) {
                                    if (!core.doc.sessionContext.user.Admin) {
                                        //
                                        // you are not admin
                                        //
                                        HasEditRights = false;
                                    } else if (DeveloperOnly) {
                                        //
                                        // you are admin, and the record is developer
                                        //
                                        HasEditRights = false;
                                    }
                                }
                            }
                            if ((HasEditRights) && (Active) && (Authorable)) {
                                tempIsVisibleUserField = true;
                            }
                            break;
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempIsVisibleUserField;
        }
        //
        //=============================================================================================
        // true if the field is an editable user field (can edit on edit form and save to database)
        //=============================================================================================
        //
        private bool IsFieldEditable(cdefModel adminContent, editRecordClass editRecord, cdefFieldModel Field) {
            bool tempIsFieldEditable = false;
            try {
                //
                tempIsFieldEditable = IsVisibleUserField(Field.adminOnly, Field.developerOnly, Field.active, Field.authorable, Field.nameLc, adminContent.ContentTableName) && (!Field.readOnly) & (!Field.notEditable);
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempIsFieldEditable;
        }
        //
        //=============================================================================================
        //   Get
        //=============================================================================================
        //
        private int GetForm_Close(int MenuDepth, string ContentName, int RecordID) {
            int tempGetForm_Close = 0;
            try {
                //
                if (MenuDepth > 0) {
                    tempGetForm_Close = AdminFormClose;
                } else {
                    tempGetForm_Close = AdminFormIndex;
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Close;
        }
        //
        //=============================================================================================
        //
        //=============================================================================================
        //
        private void ProcessActionSave(cdefModel adminContent, editRecordClass editRecord, bool UseContentWatchLink) {
            try {
                string EditorStyleRulesFilename = null;
                //
                if (true) {
                    //
                    //
                    //
                    if (!(core.doc.debug_iUserError != "")) {
                        //todo  NOTE: The following VB 'Select Case' included either a non-ordinal switch expression or non-ordinal, range-type, or non-constant 'Case' expressions and was converted to C# 'if-else' logic:
                        //						Select Case genericController.vbUCase(adminContent.ContentTableName)
                        //ORIGINAL LINE: Case genericController.vbUCase("ccMembers")
                        if (genericController.vbUCase(adminContent.ContentTableName) == genericController.vbUCase("ccMembers")) {
                            //
                            //
                            //

                            SaveEditRecord(adminContent, editRecord);
                            SaveMemberRules(editRecord.id);
                            //Call SaveTopicRules
                        }
                        //ORIGINAL LINE: Case "CCEMAIL"
                        else if (genericController.vbUCase(adminContent.ContentTableName) == "CCEMAIL") {
                            //
                            //
                            //
                            SaveEditRecord(adminContent, editRecord);
                            // NO - ignore wwwroot styles, and create it on the fly during send
                            //If core.main_GetSiteProperty2("BuildVersion") >= "3.3.291" Then
                            //    Call core.app.executeSql( "update ccEmail set InlineStyles=" & encodeSQLText(core.main_GetStyleSheetProcessed) & " where ID=" & EditRecord.ID)
                            //End If
                            core.html.processCheckList("EmailGroups", "Group Email", genericController.encodeText(editRecord.id), "Groups", "Email Groups", "EmailID", "GroupID");
                            core.html.processCheckList("EmailTopics", "Group Email", genericController.encodeText(editRecord.id), "Topics", "Email Topics", "EmailID", "TopicID");
                        }
                        //ORIGINAL LINE: Case "CCCONTENT"
                        else if (genericController.vbUCase(adminContent.ContentTableName) == "CCCONTENT") {
                            //
                            //
                            //
                            SaveEditRecord(adminContent, editRecord);
                            LoadAndSaveGroupRules(editRecord);
                        }
                        //ORIGINAL LINE: Case "CCPAGECONTENT"
                        else if (genericController.vbUCase(adminContent.ContentTableName) == "CCPAGECONTENT") {
                            //
                            //
                            //
                            SaveEditRecord(adminContent, editRecord);
                            LoadContentTrackingDataBase(adminContent, editRecord);
                            LoadContentTrackingResponse(adminContent, editRecord);
                            //Call LoadAndSaveMetaContent()
                            SaveLinkAlias(adminContent, editRecord);
                            //Call SaveTopicRules
                            SaveContentTracking(adminContent, editRecord);
                        }
                        //ORIGINAL LINE: Case "CCLIBRARYFOLDERS"
                        else if (genericController.vbUCase(adminContent.ContentTableName) == "CCLIBRARYFOLDERS") {
                            //
                            //
                            //
                            SaveEditRecord(adminContent, editRecord);
                            LoadContentTrackingDataBase(adminContent, editRecord);
                            LoadContentTrackingResponse(adminContent, editRecord);
                            //Call LoadAndSaveCalendarEvents
                            //Call LoadAndSaveMetaContent()
                            core.html.processCheckList("LibraryFolderRules", adminContent.Name, genericController.encodeText(editRecord.id), "Groups", "Library Folder Rules", "FolderID", "GroupID");
                            //call SaveTopicRules
                            SaveContentTracking(adminContent, editRecord);
                        }
                        //ORIGINAL LINE: Case "CCSETUP"
                        else if (genericController.vbUCase(adminContent.ContentTableName) == "CCSETUP") {
                            //
                            // Site Properties
                            //
                            SaveEditRecord(adminContent, editRecord);
                            if (editRecord.nameLc.ToLower() == "allowlinkalias") {
                                if (core.siteProperties.getBoolean("AllowLinkAlias")) {
                                    TurnOnLinkAlias(UseContentWatchLink);
                                }
                            }
                        }
                        //ORIGINAL LINE: Case genericController.vbUCase("ccGroups")
                        else if (genericController.vbUCase(adminContent.ContentTableName) == genericController.vbUCase("ccGroups")) {
                            //Case "CCGROUPS"
                            //
                            //
                            //
                            SaveEditRecord(adminContent, editRecord);
                            LoadContentTrackingDataBase(adminContent, editRecord);
                            LoadContentTrackingResponse(adminContent, editRecord);
                            LoadAndSaveContentGroupRules(editRecord.id);
                            //Call LoadAndSaveCalendarEvents
                            //Call LoadAndSaveMetaContent()
                            //call SaveTopicRules
                            SaveContentTracking(adminContent, editRecord);
                            //Dim EditorStyleRulesFilename As String
                        }
                        //ORIGINAL LINE: Case "CCTEMPLATES"
                        else if (genericController.vbUCase(adminContent.ContentTableName) == "CCTEMPLATES") {
                            //
                            // save and clear editorstylerules for this template
                            //
                            SaveEditRecord(adminContent, editRecord);
                            LoadContentTrackingDataBase(adminContent, editRecord);
                            LoadContentTrackingResponse(adminContent, editRecord);
                            //Call LoadAndSaveCalendarEvents
                            //Call LoadAndSaveMetaContent()
                            //call SaveTopicRules
                            SaveContentTracking(adminContent, editRecord);
                            //
                            EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", editRecord.id.ToString(), 1, 99, 1);
                            core.privateFiles.deleteFile(EditorStyleRulesFilename);
                            //Case "CCSHAREDSTYLES"
                            //    '
                            //    ' save and clear editorstylerules for any template
                            //    '
                            //    Call SaveEditRecord(adminContent, editRecord)
                            //    Call LoadContentTrackingDataBase(adminContent, editRecord)
                            //    Call LoadContentTrackingResponse(adminContent, editRecord)
                            //    'Call LoadAndSaveCalendarEvents
                            //    Call LoadAndSaveMetaContent()
                            //    'call SaveTopicRules
                            //    Call SaveContentTracking(adminContent, editRecord)
                            //    '
                            //    EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", "0", 1, 99, vbTextCompare)
                            //    Call core.cdnFiles.deleteFile(EditorStyleRulesFilename)
                            //    '
                            //    CS = core.db.cs_openCsSql_rev("default", "select id from cctemplates")
                            //    Do While core.db.cs_ok(CS)
                            //        EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", core.db.cs_get(CS, "ID"), 1, 99, vbTextCompare)
                            //        Call core.cdnFiles.deleteFile(EditorStyleRulesFilename)
                            //        Call core.db.cs_goNext(CS)
                            //    Loop
                            //    Call core.db.cs_Close(CS)


                        }
                        //ORIGINAL LINE: Case Else
                        else {
                            //
                            //
                            //
                            SaveEditRecord(adminContent, editRecord);
                            LoadContentTrackingDataBase(adminContent, editRecord);
                            LoadContentTrackingResponse(adminContent, editRecord);
                            //Call LoadAndSaveCalendarEvents
                            //Call LoadAndSaveMetaContent()
                            //call SaveTopicRules
                            SaveContentTracking(adminContent, editRecord);
                        }
                    }
                }
                //
                // If the content supports datereviewed, mark it
                //
                if (core.doc.debug_iUserError != "") {
                    AdminForm = AdminSourceForm;
                }
                AdminAction = AdminActionNop; // convert so action can be used in as a refresh
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //
        //
        //========================================================================
        //
        //========================================================================
        //
        //Private Function GetForm_EmailControl() As String
        //    On Error GoTo //ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.GetForm_EmailControl")
        //    '
        //    Dim Content As New fastStringClass
        //    Dim Copy As String
        //    Dim Button As String
        //    Dim ButtonList As String
        //    Dim SaveAction As Boolean
        //    Dim HelpCopy As String
        //    Dim FieldValue As String
        //    Dim PaymentProcessMethod as integer
        //    Dim Adminui As New adminUIclass(core)
        //    Dim Description As String
        //    '
        //    if true then ' 3.3.009" Then
        //        SettingPageID = core.htmldoc.main_GetRecordID_Internal("Setting Pages", "Email Settings")
        //    End If
        //    If SettingPageID <> 0 Then
        //        Call core.htmldoc.main_AddRefreshQueryString(RequestNameOpenSettingPage, SettingPageID)
        //        GetForm_EmailControl = GetSettingPage(SettingPageID)
        //    Else
        //        Button = core.main_GetStreamText2(RequestNameButton)
        //        If Button = ButtonCancel Then
        //            '
        //            '
        //            '
        //            Call core.main_Redirect2(core.app.SiteProperty_AdminURL, "Email Control, Cancel Button Pressed", False)
        //        ElseIf Not core.main_IsAdmin Then
        //            '
        //            '
        //            '
        //            ButtonList = ButtonCancel
        //            Content.Add( AdminUI.GetFormBodyAdminOnly()
        //        Else
        //            '
        //            ' Process Requests
        //            '
        //            SaveAction = (Button = ButtonSave) Or (Button = ButtonOK)
        //            '
        //            ButtonList = ButtonCancel & "," & ButtonSave & "," & ButtonOK
        //            Content.Add( AdminUI.EditTableOpen)
        //            '
        //            ' Common email addresses
        //            '
        //            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>General Email Addresses</b>" & kmaEndTableCell & kmaEndTableRow)
        //            '
        //            HelpCopy = "This is the Email address displayed throughout the site when a visitor is prompted to contact the site administrator."
        //            Copy = (GetPropertyControl("EmailAdmin", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "Admin Email Address", HelpCopy, False, False))
        //            '
        //            HelpCopy = "This is the Email address displayed throughout the site when a visitor is prompted to send site comments."
        //            Copy = (GetPropertyControl("EmailComments", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "Comment Email Address", HelpCopy, False, False))
        //            '
        //            HelpCopy = "This is the Email address used on out-going Emails when no other address is available. For your Email to get to its destination, this Email address must be a valid Email account on a mail server."
        //            Copy = (GetPropertyControl("EmailFromAddress", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "General Email From Address", HelpCopy, False, False))
        //            '
        //            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>Trap Email Handling</b>" & kmaEndTableCell & kmaEndTableRow)
        //            '
        //            HelpCopy = "When checked, all system errors (called traps errors) generate an Email to the Trap Email address."
        //            Copy = (GetPropertyControl("AllowTrapemail", FieldTypeBoolean, SaveAction, ""))
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "Allow Trap Error Email", HelpCopy, False, False))
        //            '
        //            HelpCopy = "This is the Email address to which all systems errors (called trap errors) are sent when Allow Trap Error Email is checked."
        //            Copy = (GetPropertyControl("TrapEmail", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "Trap Error Email Address", HelpCopy, False, False))
        //            '
        //            ' Email Sending
        //            '
        //            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>Sending Email</b>" & kmaEndTableCell & kmaEndTableRow)
        //            '
        //            HelpCopy = "This is the domain name or IP address of the SMTP mail server you will use to send. If you are using the MS SMTP in IIS on this machine, use 127.0.0.1."
        //            Copy = (GetPropertyControl("SMTPServer", FieldTypeText, SaveAction, "127.0.0.1"))
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "SMTP Email Server", HelpCopy, False, False))
        //            '
        //            HelpCopy = "When checked, the login box includes a section for users to enter their Email addresses. When submitted, all username and password matches for that Email address are sent to the Email address."
        //            Copy = (GetPropertyControl("AllowPasswordEmail", FieldTypeBoolean, SaveAction, ""))
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "Allow Password Email", HelpCopy, False, False))
        //    '
        //    ' read-only - no longer user configurable
        //    '
        //    '        '
        //            HelpCopy = "This text is included at the bottom of each group, system, and conditional email. It contains a link that the Email recipient can click to block them from future emails from this site. Only site developers can modify this text."
        //            If core.main_IsDeveloper Then
        //                HelpCopy = "<br><br>Developer: This text should conform to standards set by both local and federal law, as well as those required by your email server administrator. To create the clickable link, include link tags around your text (&lt%link&gt;click here&lt%/link&gt;). If you omit the link tag, a (click here) will be added to the end."
        //            End If
        //            Copy = (GetPropertyHTMLControl("EmailSpamFooter", SaveAction, DefaultSpamFooter, (Not core.main_IsDeveloper)))
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "Email SpamFooter", HelpCopy, False, True))
        //            '
        //            HelpCopy = "Group and Conditional Email are delivered from another program that checks in about every minute. This is the time and date of the last check."
        //            Copy = core.main_GetSiteProperty2("EmailServiceLastCheck")
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "Last Send Email Status", HelpCopy, False, False))
        //            '
        //            ' Bounce Email Handling
        //            '
        //            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>Bounce Email Handling</b>" & kmaEndTableCell & kmaEndTableRow)
        //            '
        //            HelpCopy = "If present, all outbound Emails that can not be delivered will be returned to this address. This should be a valid Email address on an Email server."
        //            Copy = (GetPropertyControl("EmailBounceAddress", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "Bounce Email Address", HelpCopy, False, False))
        //            '
        //            HelpCopy = "When checked, the system will attempt to retrieve bounced Emails from the following Email account and mark the members according to the processing rules included here."
        //            Copy = (GetPropertyControl("AllowEmailBounceProcessing", FieldTypeBoolean, SaveAction, ""))
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "Process Bounced Emails", HelpCopy, False, False))
        //            '
        //            HelpCopy = "The POP Email server where Emails will be retrieved and processed."
        //            Copy = (GetPropertyControl("POPServer", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "POP Email Server", HelpCopy, False, False))
        //            '
        //            HelpCopy = "The account username to retrieve Emails for processing."
        //            Copy = (GetPropertyControl("POPServerUsername", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "POP Email Username", HelpCopy, False, False))
        //            '
        //            HelpCopy = "The account password to retrieve Emails for processing."
        //            Copy = (GetPropertyControl("POPServerPassword", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "POP Email Password", HelpCopy, False, False))
        //            '
        //            HelpCopy = "Set the action to be performed when an Email address is identified as invalid by the bounce process."
        //            If Not SaveAction Then
        //                FieldValue = genericController.EncodeInteger(core.main_GetSiteProperty2("EMAILBOUNCEPROCESSACTION"))
        //            Else
        //                FieldValue = genericController.EncodeInteger(core.main_GetStreamText2("EMAILBOUNCEPROCESSACTION"))
        //                Call core.app.setSiteProperty("EMAILBOUNCEPROCESSACTION", FieldValue)
        //            End If
        //            Copy = "<select size=1 name=EMAILBOUNCEPROCESSACTION>" _
        //                & "<option value=0>Do Nothing</option>" _
        //                & "<option value=1>Clear the Allow Group Email field for all members with a matching Email address</option>" _
        //                & "<option value=2>Clear all member Email addresses that match the Email address</option>" _
        //                & "<option value=3>Delete all Members with a matching Email address</option>" _
        //                & "</select>"
        //            Copy = genericController.vbReplace(Copy, "value=" & FieldValue, "selected value=" & FieldValue)
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "Bounce Email Action", HelpCopy, False, False))
        //            '
        //            HelpCopy = "Bounce emails are retrieved about every minute. This is the status of the last check."
        //            Copy = core.main_GetSiteProperty2("POPServerStatus")
        //            Call Content.Add(AdminUI.GetEditRow( Copy, "Last Receive Email Status", HelpCopy, False, False))
        //            '
        //            Content.Add( AdminUI.EditTableClose)
        //            '
        //            ' Close form
        //            '
        //            If Button = ButtonOK Then
        //                Call core.main_Redirect2(core.app.SiteProperty_AdminURL, "EmailControl, OK Button Pressed", False)
        //                'Call core.main_Redirect2(encodeAppRootPath(core.main_GetSiteProperty2("AdminURL"), core.main_ServerVirtualPath, core.app.RootPath, core.main_ServerHost))
        //            End If
        //            Content.Add( core.main_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormEmailControl))
        //        End If
        //        '
        //        Description = "This tool is used to control the Contensive Email processes."
        //        GetForm_EmailControl = AdminUI.GetBody( "Email Control", ButtonList, "", True, True, Description, "", 0, Content.Text)
        //    End If
        //    '
        //    '''Dim th as integer: Exit Function
        //    '
        //    ' ----- Error Trap
        //    '
        ////ErrorTrap:
        //    Call HandleClassTrapErrorBubble("GetForm_EmailControl")
        //End Function
        //
        //========================================================================
        //
        //========================================================================
        //
        private string GetForm_Downloads() {
            string tempGetForm_Downloads = null;
            try {
                string ResultMessage = null;
                string LinkPrefix = null;
                string LinkSuffix = null;
                string RemoteKey = null;
                string Button = null;
                int CS = 0;
                string ContentName = null;
                int RecordID = 0;
                string SQL = null;
                string RQS = null;
                string Criteria = null;
                int PageSize = 0;
                int PageNumber = 0;
                int TopCount = 0;
                int RowPointer = 0;
                int DataRowCount = 0;
                string PreTableCopy = "";
                string PostTableCopy = "";
                int ColumnPtr = 0;
                string[] ColCaption = null;
                string[] ColAlign = null;
                string[] ColWidth = null;
                string[,] Cells = null;
                string AdminURL = null;
                DateTime DateCompleted = default(DateTime);
                int RowCnt = 0;
                int RowPtr = 0;
                int ContentID = 0;
                string Format = null;
                string TableName = null;
                string Filename = null;
                string Name = null;
                string Caption = null;
                string Description = "";
                string ButtonListLeft = null;
                string ButtonListRight = null;
                int ContentPadding = 0;
                string ContentSummary = "";
                stringBuilderLegacyController Tab0 = new stringBuilderLegacyController();
                stringBuilderLegacyController Tab1 = new stringBuilderLegacyController();
                string Content = "";
                string Cell = null;
                adminUIController Adminui = new adminUIController(core);
                string SQLFieldName = null;
                //
                const int ColumnCnt = 5;
                //
                Button = core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    return core.webServer.redirect("/" + core.appConfig.adminRoute, "Downloads, Cancel Button Pressed");
                }
                //
                if (!core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                    //
                    // Must be a developer
                    //
                    ButtonListLeft = ButtonCancel;
                    ButtonListRight = "";
                    Content = Content + Adminui.GetFormBodyAdminOnly();
                } else {
                    ContentID = core.docProperties.getInteger("ContentID");
                    Format = core.docProperties.getText("Format");
                    SQLFieldName = "SQLQuery";
                    //
                    // Process Requests
                    //
                    if (!string.IsNullOrEmpty(Button)) {
                        switch (Button) {
                            case ButtonDelete:
                                RowCnt = core.docProperties.getInteger("RowCnt");
                                if (RowCnt > 0) {
                                    for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                        if (core.docProperties.getBoolean("Row" + RowPtr)) {
                                            core.db.deleteContentRecord("Tasks", core.docProperties.getInteger("RowID" + RowPtr));
                                        }
                                    }
                                }
                                break;
                            case ButtonRequestDownload:
                                //
                                // Request the download again
                                //
                                RowCnt = core.docProperties.getInteger("RowCnt");
                                if (RowCnt > 0) {
                                    for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                        if (core.docProperties.getBoolean("Row" + RowPtr)) {
                                            int CSSrc = 0;
                                            int CSDst = 0;

                                            CSSrc = core.db.csOpenRecord("Tasks", core.docProperties.getInteger("RowID" + RowPtr));
                                            if (core.db.csOk(CSSrc)) {
                                                CSDst = core.db.csInsertRecord("Tasks");
                                                if (core.db.csOk(CSDst)) {
                                                    core.db.csSet(CSDst, "Name", core.db.csGetText(CSSrc, "name"));
                                                    core.db.csSet(CSDst, SQLFieldName, core.db.csGetText(CSSrc, SQLFieldName));
                                                    if (genericController.vbLCase(core.db.csGetText(CSSrc, "command")) == "xml") {
                                                        core.db.csSet(CSDst, "Filename", "DupDownload_" + encodeText(genericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(genericController.GetRandomInteger(core)) + ".xml");
                                                        core.db.csSet(CSDst, "Command", "BUILDXML");
                                                    } else {
                                                        core.db.csSet(CSDst, "Filename", "DupDownload_" + encodeText(genericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(genericController.GetRandomInteger(core)) + ".csv");
                                                        core.db.csSet(CSDst, "Command", "BUILDCSV");
                                                    }
                                                }
                                                core.db.csClose(ref CSDst);
                                            }
                                            core.db.csClose(ref CSSrc);
                                        }
                                    }
                                }
                                //
                                //
                                //
                                if ((!string.IsNullOrEmpty(Format)) && (ContentID == 0)) {
                                    Description = Description + "<p>Please select a Content before requesting a download</p>";
                                } else if ((string.IsNullOrEmpty(Format)) && (ContentID != 0)) {
                                    Description = Description + "<p>Please select a Format before requesting a download</p>";
                                } else if (Format == "CSV") {
                                    CS = core.db.csInsertRecord("Tasks");
                                    if (core.db.csOk(CS)) {
                                        ContentName = cdefModel.getContentNameByID(core, ContentID);
                                        TableName = cdefModel.getContentTablename(core, ContentName);
                                        Criteria = cdefModel.getContentControlCriteria(core, ContentName);
                                        Name = "CSV Download, " + ContentName;
                                        Filename = genericController.vbReplace(ContentName, " ", "") + "_" + encodeText(genericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(genericController.GetRandomInteger(core)) + ".csv";
                                        core.db.csSet(CS, "Name", Name);
                                        core.db.csSet(CS, "Filename", Filename);
                                        core.db.csSet(CS, "Command", "BUILDCSV");
                                        core.db.csSet(CS, SQLFieldName, "SELECT * from " + TableName + " where " + Criteria);
                                        Description = Description + "<p>Your CSV Download has been requested.</p>";
                                    }
                                    core.db.csClose(ref CS);
                                    Format = "";
                                    ContentID = 0;
                                } else if (Format == "XML") {
                                    CS = core.db.csInsertRecord("Tasks");
                                    if (core.db.csOk(CS)) {
                                        ContentName = cdefModel.getContentNameByID(core, ContentID);
                                        TableName = cdefModel.getContentTablename(core, ContentName);
                                        Criteria = cdefModel.getContentControlCriteria(core, ContentName);
                                        Name = "XML Download, " + ContentName;
                                        Filename = genericController.vbReplace(ContentName, " ", "") + "_" + encodeText(genericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(genericController.GetRandomInteger(core)) + ".xml";
                                        core.db.csSet(CS, "Name", Name);
                                        core.db.csSet(CS, "Filename", Filename);
                                        core.db.csSet(CS, "Command", "BUILDXML");
                                        core.db.csSet(CS, SQLFieldName, "SELECT * from " + TableName + " where " + Criteria);
                                        Description = Description + "<p>Your XML Download has been requested.</p>";
                                    }
                                    core.db.csClose(ref CS);
                                    Format = "";
                                    ContentID = 0;
                                }
                                break;
                        }
                    }
                    //
                    // Build Tab0
                    //
                    //Tab0.Add( "<p>The following is a list of available downloads</p>")
                    //
                    RQS = core.doc.refreshQueryString;
                    PageSize = core.docProperties.getInteger(RequestNamePageSize);
                    if (PageSize == 0) {
                        PageSize = 50;
                    }
                    PageNumber = core.docProperties.getInteger(RequestNamePageNumber);
                    if (PageNumber == 0) {
                        PageNumber = 1;
                    }
                    AdminURL = "/" + core.appConfig.adminRoute;
                    TopCount = PageNumber * PageSize;
                    //
                    // Setup Headings
                    //
                    ColCaption = new string[ColumnCnt + 1];
                    ColAlign = new string[ColumnCnt + 1];
                    ColWidth = new string[ColumnCnt + 1];
                    Cells = new string[PageSize + 1, ColumnCnt + 1];
                    //
                    ColCaption[ColumnPtr] = "Select<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=10 height=1>";
                    ColAlign[ColumnPtr] = "center";
                    ColWidth[ColumnPtr] = "10";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    ColCaption[ColumnPtr] = "Name";
                    ColAlign[ColumnPtr] = "left";
                    ColWidth[ColumnPtr] = "100%";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    ColCaption[ColumnPtr] = "For<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=100 height=1>";
                    ColAlign[ColumnPtr] = "left";
                    ColWidth[ColumnPtr] = "100";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    ColCaption[ColumnPtr] = "Requested<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=150 height=1>";
                    ColAlign[ColumnPtr] = "left";
                    ColWidth[ColumnPtr] = "150";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    ColCaption[ColumnPtr] = "File<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=100 height=1>";
                    ColAlign[ColumnPtr] = "Left";
                    ColWidth[ColumnPtr] = "100";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    //   Get Data
                    //
                    SQL = "select M.Name as CreatedByName, T.* from ccTasks T left join ccMembers M on M.ID=T.CreatedBy where (T.Command='BuildCSV')or(T.Command='BuildXML') order by T.DateAdded Desc";
                    //Call core.main_TestPoint("Selection SQL=" & SQL)
                    CS = core.db.csOpenSql_rev("default", SQL, PageSize, PageNumber);
                    RowPointer = 0;
                    if (!core.db.csOk(CS)) {
                        Cells[0, 1] = "There are no download requests";
                        RowPointer = 1;
                    } else {
                        DataRowCount = core.db.csGetRowCount(CS);
                        LinkPrefix = "<a href=\"" + core.appConfig.cdnFilesNetprefix;
                        LinkSuffix = "\" target=_blank>Available</a>";
                        while (core.db.csOk(CS) && (RowPointer < PageSize)) {
                            RecordID = core.db.csGetInteger(CS, "ID");
                            DateCompleted = core.db.csGetDate(CS, "DateCompleted");
                            ResultMessage = core.db.csGetText(CS, "ResultMessage");
                            Cells[RowPointer, 0] = core.html.inputCheckbox("Row" + RowPointer) + core.html.inputHidden("RowID" + RowPointer, RecordID);
                            Cells[RowPointer, 1] = core.db.csGetText(CS, "name");
                            Cells[RowPointer, 2] = core.db.csGetText(CS, "CreatedByName");
                            Cells[RowPointer, 3] = core.db.csGetDate(CS, "DateAdded").ToShortDateString();
                            if (DateCompleted == DateTime.MinValue) {
                                RemoteKey = remoteQueryController.main_GetRemoteQueryKey(core, "select DateCompleted,filename,resultMessage from cctasks where id=" + RecordID, "default", 1);
                                Cell = "";
                                Cell = Cell + "\r\n<div id=\"pending" + RowPointer + "\">Pending <img src=\"/ccLib/images/ajax-loader-small.gif\" width=16 height=16></div>";
                                //
                                Cell = Cell + "\r\n<script>";
                                Cell = Cell + "\r\nfunction statusHandler" + RowPointer + "(results) {";
                                Cell = Cell + "\r\n var jo,isDone=false;";
                                Cell = Cell + "\r\n eval('jo='+results);";
                                Cell = Cell + "\r\n if (jo){";
                                Cell = Cell + "\r\n  if(jo.DateCompleted) {";
                                Cell = Cell + "\r\n    var dst=document.getElementById('pending" + RowPointer + "');";
                                Cell = Cell + "\r\n    isDone=true;";
                                Cell = Cell + "\r\n    if(jo.resultMessage=='OK') {";
                                Cell = Cell + "\r\n      dst.innerHTML='" + LinkPrefix + "'+jo.filename+'" + LinkSuffix + "';";
                                Cell = Cell + "\r\n    }else{";
                                Cell = Cell + "\r\n      dst.innerHTML='error';";
                                Cell = Cell + "\r\n    }";
                                Cell = Cell + "\r\n  }";
                                Cell = Cell + "\r\n }";
                                Cell = Cell + "\r\n if(!isDone) setTimeout(\"requestStatus" + RowPointer + "()\",5000)";
                                Cell = Cell + "\r\n}";
                                //
                                Cell = Cell + "\r\nfunction requestStatus" + RowPointer + "() {";
                                Cell = Cell + "\r\n  cj.ajax.getNameValue(statusHandler" + RowPointer + ",'" + RemoteKey + "');";
                                Cell = Cell + "\r\n}";
                                Cell = Cell + "\r\nrequestStatus" + RowPointer + "();";
                                Cell = Cell + "\r\n</script>";
                                //
                                Cells[RowPointer, 4] = Cell;
                            } else if (ResultMessage == "ok") {
                                Cells[RowPointer, 4] = "<div id=\"pending" + RowPointer + "\">" + LinkPrefix + core.db.csGetText(CS, "filename") + LinkSuffix + "</div>";
                            } else {
                                Cells[RowPointer, 4] = "<div id=\"pending" + RowPointer + "\"><a href=\"javascript:alert('" + genericController.EncodeJavascript(ResultMessage) + ";return false');\">error</a></div>";
                            }
                            RowPointer = RowPointer + 1;
                            core.db.csGoNext(CS);
                        }
                    }
                    core.db.csClose(ref CS);
                    Tab0.Add(core.html.inputHidden("RowCnt", RowPointer));
                    Cell = Adminui.GetReport(RowPointer, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, "ccPanel");
                    Tab0.Add(Cell);
                    //Tab0.Add( "<div style=""height:200px;"">" & Cell & "</div>"
                    //        '
                    //        ' Build RequestContent Form
                    //        '
                    //        Tab1.Add( "<p>Use this form to request a download. Select the criteria for the download and click the [Request Download] button. The request should then appear on the requested download list in the other tab. When the download has been created, it will be become available.</p>")
                    //        '
                    //        Tab1.Add( "<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%"">")
                    //        '
                    //        Call Tab1.Add("<tr>")
                    //        Call Tab1.Add("<td align=right>Content</td>")
                    //        Call Tab1.Add("<td>" & core.htmldoc.main_GetFormInputSelect2("ContentID", ContentID, "Content", "", "", "", IsEmptyList) & "</td>")
                    //        Call Tab1.Add("</tr>")
                    //        '
                    //        Call Tab1.Add("<tr>")
                    //        Call Tab1.Add("<td align=right>Format</td>")
                    //        Call Tab1.Add("<td><select name=Format value=""" & Format & """><option value=CSV>CSV</option><option name=XML value=XML>XML</option></select></td>")
                    //        Call Tab1.Add("</tr>")
                    //        '
                    //        Call Tab1.Add("" _
                    //            & "<tr>" _
                    //            & "<td width=""120""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""120"" height=""1""></td>" _
                    //            & "<td width=""100%"">&nbsp;</td>" _
                    //            & "</tr>" _
                    //            & "</table>")
                    //        '
                    //        ' Build and add tabs
                    //        '
                    //        Call core.htmldoc.main_AddLiveTabEntry("Current&nbsp;Downloads", Tab0.Text, "ccAdminTab")
                    //        Call core.htmldoc.main_AddLiveTabEntry("Request&nbsp;New&nbsp;Download", Tab1.Text, "ccAdminTab")
                    //        Content = core.htmldoc.main_GetLiveTabs()
                    Content = Tab0.Text;
                    //
                    ButtonListLeft = ButtonCancel + "," + ButtonRefresh + "," + ButtonDelete;
                    //ButtonListLeft = ButtonCancel & "," & ButtonRefresh & "," & ButtonDelete & "," & ButtonRequestDownload
                    ButtonListRight = "";
                    Content = Content + core.html.inputHidden(rnAdminSourceForm, AdminFormDownloads);
                }
                //
                Caption = "Download Manager";
                Description = ""
                    + "<p>The Download Manager holds all downloads requested from anywhere on the website. It also provides tools to request downloads from any Content.</p>"
                    + "<p>To add a new download of any content in Contensive, click Export on the filter tab of the content listing page. To add a new download from a SQL statement, use Custom Reports under Reports on the Navigator.</p>";
                ContentPadding = 0;
                tempGetForm_Downloads = Adminui.GetBody(Caption, ButtonListLeft, ButtonListRight, true, true, Description, ContentSummary, ContentPadding, Content);
                //
                core.html.addTitle(Caption);
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Downloads;
        }
        //
        //========================================================================
        //   Display field in the admin/edit
        //========================================================================
        //
        private string GetForm_Edit_AddTab(string Caption, string Content, bool AllowAdminTabs) {
            string tempGetForm_Edit_AddTab = null;
            try {
                //
                if (!string.IsNullOrEmpty(Content)) {
                    if (!AllowAdminTabs) {
                        tempGetForm_Edit_AddTab = Content;
                    } else {
                        core.html.addComboTabEntry(Caption.Replace(" ", "&nbsp;"), "", "", Content, false, "ccAdminTab");
                        //Call core.htmldoc.main_AddLiveTabEntry(Replace(Caption, " ", "&nbsp;"), Content, "ccAdminTab")
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Edit_AddTab;
        }
        //
        //========================================================================
        //   Creates Tabbed content that is either Live (all content on page) or Ajax (click and ajax in the content)
        //========================================================================
        //
        private string GetForm_Edit_AddTab2(string Caption, string Content, bool AllowAdminTabs, string AjaxLink) {
            string tempGetForm_Edit_AddTab2 = null;
            try {
                //
                if (!AllowAdminTabs) {
                    //
                    // non-tab mode
                    //
                    tempGetForm_Edit_AddTab2 = Content;
                } else if (!string.IsNullOrEmpty(AjaxLink)) {
                    //
                    // Ajax Tab
                    //
                    core.html.addComboTabEntry(Caption.Replace(" ", "&nbsp;"), "", AjaxLink, "", false, "ccAdminTab");
                } else {
                    //
                    // Live Tab
                    //
                    core.html.addComboTabEntry(Caption.Replace(" ", "&nbsp;"), "", "", Content, false, "ccAdminTab");
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Edit_AddTab2;
        }
        //
        //=============================================================================
        // Create a child content
        //=============================================================================
        //
        private string GetForm_PageContentMap() {
            return "<p>The Page Content Map has been replaced with the Site Explorer, available as an Add-on through the Add-on Manager.</p>";
        }
        //
        //
        //
        private string GetForm_Edit_Tabs(cdefModel adminContent, editRecordClass editRecord, bool readOnlyField, bool IsLandingPage, bool IsRootPage, csv_contentTypeEnum EditorContext, bool allowAjaxTabs, int TemplateIDForStyles, string[] fieldTypeDefaultEditors, string fieldEditorPreferenceList, string styleList, string styleOptionList, int emailIdForStyles, bool IsTemplateTable, string editorAddonListJSON) {
            string returnHtml = "";
            try {
                //
                string tabContent = null;
                string AjaxLink = null;
                List<string> TabsFound = new List<string>();
                string editTabCaption = null;
                string NewFormFieldList = null;
                string FormFieldList = null;
                bool AllowHelpMsgCustom = false;
                string IDList = null;
                DataTable dt = null;
                string[,] TempVar = null;
                int HelpCnt = 0;
                int fieldId = 0;
                int LastFieldID = 0;
                int HelpPtr = 0;
                int[] HelpIDCache = { };
                string[] helpDefaultCache = { };
                string[] HelpCustomCache = { };
                keyPtrController helpIdIndex = new keyPtrController();
                string fieldNameLc = null;
                //
                // ----- read in help
                //
                IDList = "";
                foreach (KeyValuePair<string, cdefFieldModel> keyValuePair in adminContent.fields) {
                    cdefFieldModel field = keyValuePair.Value;
                    IDList = IDList + "," + field.id;
                }
                if (!string.IsNullOrEmpty(IDList)) {
                    IDList = IDList.Substring(1);
                }
                //
                dt = core.db.executeQuery("select fieldid,helpdefault,helpcustom from ccfieldhelp where fieldid in (" + IDList + ") order by fieldid,id");
                TempVar = core.db.convertDataTabletoArray(dt);
                if (TempVar.GetLength(0) > 0) {
                    HelpCnt = TempVar.GetUpperBound(1) + 1;
                    HelpIDCache = new int[HelpCnt + 1];
                    helpDefaultCache = new string[HelpCnt + 1];
                    HelpCustomCache = new string[HelpCnt + 1];
                    fieldId = -1;
                    for (HelpPtr = 0; HelpPtr < HelpCnt; HelpPtr++) {
                        fieldId = genericController.encodeInteger(TempVar[0, HelpPtr]);
                        if (fieldId != LastFieldID) {
                            LastFieldID = fieldId;
                            HelpIDCache[HelpPtr] = fieldId;
                            helpIdIndex.setPtr(fieldId.ToString(), HelpPtr);
                            helpDefaultCache[HelpPtr] = genericController.encodeText(TempVar[1, HelpPtr]);
                            HelpCustomCache[HelpPtr] = genericController.encodeText(TempVar[2, HelpPtr]);
                        }
                    }
                    AllowHelpMsgCustom = true;
                }
                //
                FormFieldList = ",";
                foreach (KeyValuePair<string, cdefFieldModel> keyValuePair in adminContent.fields) {
                    cdefFieldModel field = keyValuePair.Value;
                    if ((field.authorable) & (field.active) && (!TabsFound.Contains(field.editTabName.ToLower()))) {
                        TabsFound.Add(field.editTabName.ToLower());
                        fieldNameLc = field.nameLc;
                        editTabCaption = field.editTabName;
                        if (string.IsNullOrEmpty(editTabCaption)) {
                            editTabCaption = "Details";
                        }
                        NewFormFieldList = "";
                        if ((!allowAdminTabs) | (!allowAjaxTabs) || (editTabCaption.ToLower() == "details")) {
                            //
                            // Live Tab (non-tab mode, non-ajax mode, or details tab
                            //
                            tabContent = GetForm_Edit_Tab(adminContent, editRecord, editRecord.id, adminContent.Id, readOnlyField, IsLandingPage, IsRootPage, field.editTabName, EditorContext, ref NewFormFieldList, TemplateIDForStyles, HelpCnt, HelpIDCache, helpDefaultCache, HelpCustomCache, AllowHelpMsgCustom, helpIdIndex, fieldTypeDefaultEditors, fieldEditorPreferenceList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON);
                            if (!string.IsNullOrEmpty(tabContent)) {
                                returnHtml += GetForm_Edit_AddTab2(editTabCaption, tabContent, allowAdminTabs, "");
                            }
                        } else {
                            //
                            // Ajax Tab
                            //
                            //AjaxLink = "/admin/index.asp?"
                            AjaxLink = "/" + core.appConfig.adminRoute + "?"
                            + RequestNameAjaxFunction + "=" + AjaxGetFormEditTabContent + "&ID=" + editRecord.id + "&CID=" + adminContent.Id + "&ReadOnly=" + readOnlyField + "&IsLandingPage=" + IsLandingPage + "&IsRootPage=" + IsRootPage + "&EditTab=" + genericController.EncodeRequestVariable(field.editTabName) + "&EditorContext=" + EditorContext + "&NewFormFieldList=" + genericController.EncodeRequestVariable(NewFormFieldList);
                            returnHtml += GetForm_Edit_AddTab2(editTabCaption, "", true, AjaxLink);
                        }
                        if (!string.IsNullOrEmpty(NewFormFieldList)) {
                            FormFieldList = NewFormFieldList + FormFieldList;
                        }
                    }
                }
                //
                // ----- add the FormFieldList hidden - used on read to make sure all fields are returned
                //       this may not be needed, but we are having a problem with forms coming back without values
                //
                //
                // moved this to GetEditTabContent - so one is added for each tab.
                //
                returnHtml += core.html.inputHidden("FormFieldList", FormFieldList);
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnHtml;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private string GetForm_CustomReports() {
            string tempGetForm_CustomReports = null;
            try {
                //
                string Button = null;
                int CS = 0;
                string RecordName = null;
                int RecordID = 0;
                string SQL = null;
                string RQS = null;
                int PageSize = 0;
                int PageNumber = 0;
                int TopCount = 0;
                int RowPointer = 0;
                int DataRowCount = 0;
                string PreTableCopy = "";
                string PostTableCopy = "";
                int ColumnPtr = 0;
                string[] ColCaption = null;
                string[] ColAlign = null;
                string[] ColWidth = null;
                string[,] Cells = null;
                string AdminURL = null;
                int RowCnt = 0;
                int RowPtr = 0;
                int ContentID = 0;
                string Format = null;
                string Filename = null;
                string Name = null;
                string Caption = null;
                string Description = null;
                string ButtonListLeft = null;
                string ButtonListRight = null;
                int ContentPadding = 0;
                string ContentSummary = "";
                stringBuilderLegacyController Tab0 = new stringBuilderLegacyController();
                stringBuilderLegacyController Tab1 = new stringBuilderLegacyController();
                string Content = "";
                string SQLFieldName = null;
                //
                const int ColumnCnt = 4;
                //
                Button = core.docProperties.getText(RequestNameButton);
                ContentID = core.docProperties.getInteger("ContentID");
                Format = core.docProperties.getText("Format");
                //
                Caption = "Custom Report Manager";
                Description = "Custom Reports are a way for you to create a snapshot of data to view or download. To request a report, select the Custom Reports tab, check the report(s) you want, and click the [Request Download] Button. When your report is ready, it will be available in the <a href=\"?" + RequestNameAdminForm + "=30\">Download Manager</a>. To create a new custom report, select the Request New Report tab, enter a name and SQL statement, and click the Apply button.";
                ContentPadding = 0;
                ButtonListLeft = ButtonCancel + "," + ButtonDelete + "," + ButtonRequestDownload;
                //ButtonListLeft = ButtonCancel & "," & ButtonDelete & "," & ButtonRequestDownload & "," & ButtonApply
                ButtonListRight = "";
                SQLFieldName = "SQLQuery";
                //
                if (!core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                    //
                    // Must be a developer
                    //
                    Description = Description + "You can not access the Custom Report Manager because your account is not configured as an administrator.";
                } else {
                    //
                    // Process Requests
                    //
                    if (!string.IsNullOrEmpty(Button)) {
                        switch (Button) {
                            case ButtonCancel:
                                return core.webServer.redirect("/" + core.appConfig.adminRoute, "CustomReports, Cancel Button Pressed");
                            //Call core.main_Redirect2(encodeAppRootPath(core.main_GetSiteProperty2("AdminURL"), core.main_ServerVirtualPath, core.app.RootPath, core.main_ServerHost))
                            case ButtonDelete:
                                RowCnt = core.docProperties.getInteger("RowCnt");
                                if (RowCnt > 0) {
                                    for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                        if (core.docProperties.getBoolean("Row" + RowPtr)) {
                                            core.db.deleteContentRecord("Custom Reports", core.docProperties.getInteger("RowID" + RowPtr));
                                        }
                                    }
                                }
                                break;
                            case ButtonRequestDownload:
                            case ButtonApply:
                                //
                                Name = core.docProperties.getText("name");
                                SQL = core.docProperties.getText(SQLFieldName);
                                if (!string.IsNullOrEmpty(Name) | !string.IsNullOrEmpty(SQL)) {
                                    if ((string.IsNullOrEmpty(Name)) || (string.IsNullOrEmpty(SQL))) {
                                        errorController.addUserError(core, "A name and SQL Query are required to save a new custom report.");
                                    } else {
                                        CS = core.db.csInsertRecord("Custom Reports");
                                        if (core.db.csOk(CS)) {
                                            core.db.csSet(CS, "Name", Name);
                                            core.db.csSet(CS, SQLFieldName, SQL);
                                        }
                                        core.db.csClose(ref CS);
                                    }
                                }
                                //
                                RowCnt = core.docProperties.getInteger("RowCnt");
                                if (RowCnt > 0) {
                                    for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                        if (core.docProperties.getBoolean("Row" + RowPtr)) {
                                            RecordID = core.docProperties.getInteger("RowID" + RowPtr);
                                            CS = core.db.csOpenRecord("Custom Reports", RecordID);
                                            if (core.db.csOk(CS)) {
                                                SQL = core.db.csGetText(CS, SQLFieldName);
                                                Name = core.db.csGetText(CS, "Name");
                                            }
                                            core.db.csClose(ref CS);
                                            //
                                            CS = core.db.csInsertRecord("Tasks");
                                            if (core.db.csOk(CS)) {
                                                RecordName = "CSV Download, Custom Report [" + Name + "]";
                                                Filename = "CustomReport_" + encodeText(genericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(genericController.GetRandomInteger(core)) + ".csv";
                                                core.db.csSet(CS, "Name", RecordName);
                                                core.db.csSet(CS, "Filename", Filename);
                                                if (Format == "XML") {
                                                    core.db.csSet(CS, "Command", "BUILDXML");
                                                } else {
                                                    core.db.csSet(CS, "Command", "BUILDCSV");
                                                }
                                                core.db.csSet(CS, SQLFieldName, SQL);
                                                Description = Description + "<p>Your Download [" + Name + "] has been requested, and will be available in the <a href=\"?" + RequestNameAdminForm + "=30\">Download Manager</a> when it is complete. This may take a few minutes depending on the size of the report.</p>";
                                            }
                                            core.db.csClose(ref CS);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    //
                    // Build Tab0
                    //
                    Tab0.Add("<p>The following is a list of available custom reports.</p>");
                    //
                    RQS = core.doc.refreshQueryString;
                    PageSize = core.docProperties.getInteger(RequestNamePageSize);
                    if (PageSize == 0) {
                        PageSize = 50;
                    }
                    PageNumber = core.docProperties.getInteger(RequestNamePageNumber);
                    if (PageNumber == 0) {
                        PageNumber = 1;
                    }
                    AdminURL = "/" + core.appConfig.adminRoute;
                    TopCount = PageNumber * PageSize;
                    //
                    // Setup Headings
                    //
                    ColCaption = new string[ColumnCnt + 1];
                    ColAlign = new string[ColumnCnt + 1];
                    ColWidth = new string[ColumnCnt + 1];
                    Cells = new string[PageSize + 1, ColumnCnt + 1];
                    //
                    ColCaption[ColumnPtr] = "Select<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=10 height=1>";
                    ColAlign[ColumnPtr] = "center";
                    ColWidth[ColumnPtr] = "10";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    ColCaption[ColumnPtr] = "Name";
                    ColAlign[ColumnPtr] = "left";
                    ColWidth[ColumnPtr] = "100%";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    ColCaption[ColumnPtr] = "Created By<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=100 height=1>";
                    ColAlign[ColumnPtr] = "left";
                    ColWidth[ColumnPtr] = "100";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    ColCaption[ColumnPtr] = "Date Created<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=150 height=1>";
                    ColAlign[ColumnPtr] = "left";
                    ColWidth[ColumnPtr] = "150";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    //ColCaption(ColumnPtr) = "?<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=100 height=1>"
                    //ColAlign(ColumnPtr) = "Left"
                    //ColWidth(ColumnPtr) = "100"
                    //ColumnPtr = ColumnPtr + 1
                    //
                    //   Get Data
                    //
                    CS = core.db.csOpen("Custom Reports");
                    RowPointer = 0;
                    if (!core.db.csOk(CS)) {
                        Cells[0, 1] = "There are no custom reports defined";
                        RowPointer = 1;
                    } else {
                        DataRowCount = core.db.csGetRowCount(CS);
                        while (core.db.csOk(CS) && (RowPointer < PageSize)) {
                            RecordID = core.db.csGetInteger(CS, "ID");
                            //DateCompleted = core.db.cs_getDate(CS, "DateCompleted")
                            Cells[RowPointer, 0] = core.html.inputCheckbox("Row" + RowPointer) + core.html.inputHidden("RowID" + RowPointer, RecordID);
                            Cells[RowPointer, 1] = core.db.csGetText(CS, "name");
                            Cells[RowPointer, 2] = core.db.csGet(CS, "CreatedBy");
                            Cells[RowPointer, 3] = core.db.csGetDate(CS, "DateAdded").ToShortDateString();
                            //Cells(RowPointer, 4) = "&nbsp;"
                            RowPointer = RowPointer + 1;
                            core.db.csGoNext(CS);
                        }
                    }
                    core.db.csClose(ref CS);
                    string Cell = null;
                    Tab0.Add(core.html.inputHidden("RowCnt", RowPointer));
                    adminUIController Adminui = new adminUIController(core);
                    Cell = Adminui.GetReport(RowPointer, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, "ccPanel");
                    Tab0.Add("<div>" + Cell + "</div>");
                    //
                    // Build RequestContent Form
                    //
                    Tab1.Add("<p>Use this form to create a new custom report. Enter the SQL Query for the report, and a name that will be used as a caption.</p>");
                    //
                    Tab1.Add("<table border=\"0\" cellpadding=\"3\" cellspacing=\"0\" width=\"100%\">");
                    //
                    Tab1.Add("<tr>");
                    Tab1.Add("<td align=right>Name</td>");
                    Tab1.Add("<td>" + core.html.inputText("Name", "", 1, 40) + "</td>");
                    Tab1.Add("</tr>");
                    //
                    Tab1.Add("<tr>");
                    Tab1.Add("<td align=right>SQL Query</td>");
                    Tab1.Add("<td>" + core.html.inputText(SQLFieldName, "", 8, 40) + "</td>");
                    Tab1.Add("</tr>");
                    //
                    Tab1.Add("<tr><td width=\"120\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"120\" height=\"1\"></td><td width=\"100%\">&nbsp;</td></tr></table>");
                    //
                    // Build and add tabs
                    //
                    core.html.addLiveTabEntry("Custom&nbsp;Reports", Tab0.Text, "ccAdminTab");
                    core.html.addLiveTabEntry("Request&nbsp;New&nbsp;Report", Tab1.Text, "ccAdminTab");
                    Content = core.html.getLiveTabs();
                    //
                }
                //
                tempGetForm_CustomReports = admin_GetAdminFormBody(Caption, ButtonListLeft, ButtonListRight, true, true, Description, ContentSummary, ContentPadding, Content);
                //
                core.html.addTitle("Custom Reports");
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_CustomReports;
        }
        //
        //
        //=============================================================================================
        //   Create a duplicate record
        //=============================================================================================
        //
        private void ProcessActionDuplicate(cdefModel adminContent, editRecordClass editRecord) {
            try {
                // converted array to dictionary - Dim FieldPointer As Integer
                //
                if (!(core.doc.debug_iUserError != "")) {
                    switch (genericController.vbUCase(adminContent.ContentTableName)) {
                        case "CCEMAIL":
                            //
                            // --- preload array with values that may not come back in response
                            //
                            LoadEditRecord(adminContent, editRecord);
                            LoadEditRecord_Request(adminContent, editRecord);
                            //
                            if (!(core.doc.debug_iUserError != "")) {
                                //
                                // ----- Convert this to the Duplicate
                                //
                                if (adminContent.fields.ContainsKey("submitted")) {
                                    editRecord.fieldsLc["submitted"].value = false;
                                }
                                if (adminContent.fields.ContainsKey("sent")) {
                                    editRecord.fieldsLc["sent"].value = false;
                                }
                                //
                                editRecord.id = 0;
                                core.doc.addRefreshQueryString("id", genericController.encodeText(editRecord.id));
                            }
                            break;
                        default:
                            //
                            //
                            //
                            //
                            // --- preload array with values that may not come back in response
                            //
                            LoadEditRecord(adminContent, editRecord);
                            LoadEditRecord_Request(adminContent, editRecord);
                            //
                            if (!(core.doc.debug_iUserError != "")) {
                                //
                                // ----- Convert this to the Duplicate
                                //
                                editRecord.id = 0;
                                //
                                // block fields that should not duplicate
                                //
                                if (editRecord.fieldsLc.ContainsKey("ccguid")) {
                                    editRecord.fieldsLc["ccguid"].value = "";
                                }
                                //
                                if (editRecord.fieldsLc.ContainsKey("dateadded")) {
                                    editRecord.fieldsLc["dateadded"].value = DateTime.MinValue;
                                }
                                //
                                if (editRecord.fieldsLc.ContainsKey("modifieddate")) {
                                    editRecord.fieldsLc["modifieddate"].value = DateTime.MinValue;
                                }
                                //
                                if (editRecord.fieldsLc.ContainsKey("modifiedby")) {
                                    editRecord.fieldsLc["modifiedby"].value = 0;
                                }
                                //
                                // block fields that must be unique
                                //
                                foreach (KeyValuePair<string, Contensive.Core.Models.Complex.cdefFieldModel> keyValuePair in adminContent.fields) {
                                    cdefFieldModel field = keyValuePair.Value;
                                    if (genericController.vbLCase(field.nameLc) == "email") {
                                        if ((adminContent.ContentTableName.ToLower() == "ccmembers") && (genericController.encodeBoolean(core.siteProperties.getBoolean("allowemaillogin", false)))) {
                                            editRecord.fieldsLc[field.nameLc].value = "";
                                        }
                                    }
                                    if (field.uniqueName) {
                                        editRecord.fieldsLc[field.nameLc].value = "";
                                    }
                                }
                                //
                                core.doc.addRefreshQueryString("id", genericController.encodeText(editRecord.id));
                            }
                            //Call core.htmldoc.main_AddUserError("The create duplicate action is not supported for this content.")
                            break;
                    }
                    AdminForm = AdminSourceForm;
                    AdminAction = AdminActionNop; // convert so action can be used in as a refresh
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        // PrintMenuTop()
        //   Prints the menu section of the admin page
        //========================================================================
        //
        private string GetMenuTopMode() {
            string tempGetMenuTopMode = null;
            try {
                //
                //Const MenuEntryContentName = cnNavigatorEntries
                //
                int CSMenus = 0;
                string Name = null;
                int Id = 0;
                int ParentID = 0;
                bool NewWindow = false;
                string Link = null;
                string LinkLabel = null;
                string StyleSheet = "";
                string StyleSheetHover = "";
                string ImageLink = null;
                string ImageOverLink = null;
                string BakeName = null;
                string MenuHeader = null;
                List<int> editableCdefIdList = null;
                int ContentID = 0;
                bool IsAdminLocal = false;
                string MenuClose = null;
                int MenuDelimiterPosition = 0;
                bool AccessOK = false;
                //
                const string MenuDelimiter = "\r\n<!-- Menus -->\r\n";
                //
                // Create the menu
                //
                if (AdminMenuModeID == AdminMenuModeTop) {
                    //
                    // ----- Get the baked version
                    //
                    BakeName = "AdminMenu" + core.doc.sessionContext.user.id.ToString("00000000");
                    tempGetMenuTopMode = genericController.encodeText(core.cache.getObject<string>(BakeName));
                    MenuDelimiterPosition = genericController.vbInstr(1, tempGetMenuTopMode, MenuDelimiter, 1);
                    if (MenuDelimiterPosition > 1) {
                        MenuClose = tempGetMenuTopMode.Substring((MenuDelimiterPosition + MenuDelimiter.Length) - 1);
                        tempGetMenuTopMode = tempGetMenuTopMode.Left(MenuDelimiterPosition - 1);
                    } else {
                        //If GetMenuTopMode = "" Then
                        //
                        // ----- Bake the menu
                        //
                        CSMenus = GetMenuCSPointer("");
                        //CSMenus = core.app_openCsSql_Rev_Internal("default", GetMenuSQLNew())
                        if (core.db.csOk(CSMenus)) {
                            //
                            // There are menu items to bake
                            //
                            IsAdminLocal = core.doc.sessionContext.isAuthenticatedAdmin(core);
                            if (!IsAdminLocal) {
                                //
                                // content managers, need the ContentManagementList
                                //
                                editableCdefIdList = cdefModel.getEditableCdefIdList(core);
                            } else {
                                editableCdefIdList = new List<int>();
                            }
                            ImageLink = "";
                            ImageOverLink = "";
                            while (core.db.csOk(CSMenus)) {
                                ContentID = core.db.csGetInteger(CSMenus, "ContentID");
                                if (IsAdminLocal || ContentID == 0) {
                                    AccessOK = true;
                                } else if (editableCdefIdList.Contains(ContentID)) {
                                    AccessOK = true;
                                } else {
                                    AccessOK = false;
                                }
                                Id = core.db.csGetInteger(CSMenus, "ID");
                                ParentID = core.db.csGetInteger(CSMenus, "ParentID");
                                if (AccessOK) {
                                    Link = GetMenuLink(core.db.csGet(CSMenus, "LinkPage"), ContentID);
                                    if (genericController.vbInstr(1, Link, "?") == 1) {
                                        Link = core.appConfig.adminRoute + Link;
                                    }
                                } else {
                                    Link = "";
                                }
                                LinkLabel = core.db.csGet(CSMenus, "Name");
                                //If LinkLabel = "Calendar" Then
                                //    Link = Link
                                //    End If
                                NewWindow = core.db.csGetBoolean(CSMenus, "NewWindow");
                                core.menuFlyout.menu_AddEntry(genericController.encodeText(Id), ParentID.ToString(), ImageLink, ImageOverLink, Link, LinkLabel, StyleSheet, StyleSheetHover, NewWindow);

                                core.db.csGoNext(CSMenus);
                            }
                        }
                        core.db.csClose(ref CSMenus);
                        //            '
                        //            ' Add in top level node for "switch to navigator"
                        //            '
                        //            Call core.htmldoc.main_AddMenuEntry("GoToNav", 0, "?" & core.main_RefreshQueryString & "&mm=1", "", "", "Switch To Navigator", StyleSheet, StyleSheetHover, False)
                        //
                        // Create menus
                        //
                        int ButtonCnt = 0;
                        CSMenus = GetMenuCSPointer("(ParentID is null)or(ParentID=0)");
                        if (core.db.csOk(CSMenus)) {
                            tempGetMenuTopMode = "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr>";
                            ButtonCnt = 0;
                            while (core.db.csOk(CSMenus)) {
                                Name = core.db.csGet(CSMenus, "Name");
                                Id = core.db.csGetInteger(CSMenus, "ID");
                                NewWindow = core.db.csGetBoolean(CSMenus, "NewWindow");
                                MenuHeader = core.menuFlyout.getMenu(genericController.encodeText(Id), 0);
                                if (!string.IsNullOrEmpty(MenuHeader)) {
                                    if (ButtonCnt > 0) {
                                        tempGetMenuTopMode = tempGetMenuTopMode + "<td class=\"ccFlyoutDelimiter\">|</td>";
                                    }
                                    //GetMenuTopMode = GetMenuTopMode & "<td width=""1"" class=""ccPanelShadow""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td>"
                                    //GetMenuTopMode = GetMenuTopMode & "<td width=""1"" class=""ccPanelHilite""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td>"
                                    //
                                    // --- Add New GetMenuTopMode Button and leave the column open
                                    //
                                    Link = "";
                                    tempGetMenuTopMode = tempGetMenuTopMode + "<td class=\"ccFlyoutButton\">" + MenuHeader + "</td>";
                                    // GetMenuTopMode = GetMenuTopMode & "<td><nobr>&nbsp;" & MenuHeader & "&nbsp;</nobr></td>"
                                }
                                ButtonCnt = ButtonCnt + 1;
                                core.db.csGoNext(CSMenus);
                            }
                            tempGetMenuTopMode = tempGetMenuTopMode + "</tr></table>";
                            tempGetMenuTopMode = core.html.getPanel(tempGetMenuTopMode, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 1);
                        }
                        core.db.csClose(ref CSMenus);
                        //
                        // Save the Baked Menu
                        //
                        MenuClose = core.menuFlyout.menu_GetClose();
                        //GetMenuTopMode = GetMenuTopMode & core.main_GetMenuClose
                        core.cache.setObject(BakeName, tempGetMenuTopMode + MenuDelimiter + MenuClose, "Navigator Entries,People,Content,Groups,Group Rules");
                    }
                    core.doc.htmlForEndOfBody = core.doc.htmlForEndOfBody + MenuClose;
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetMenuTopMode;
        }
        //
        //========================================================================
        // Read and save a GetForm_InputCheckList
        //   see GetForm_InputCheckList for an explaination of the input
        //========================================================================
        //
        private void SaveMemberRules(int PeopleID) {
            try {
                //
                int GroupCount = 0;
                int GroupPointer = 0;
                int GroupID = 0;
                bool RuleNeeded = false;
                int CSRule = 0;
                DateTime DateExpires = default(DateTime);
                object DateExpiresVariant = null;
                bool RuleActive = false;
                DateTime RuleDateExpires = default(DateTime);
                int MemberRuleID = 0;
                //
                // --- create MemberRule records for all selected
                //
                GroupCount = core.docProperties.getInteger("MemberRules.RowCount");
                if (GroupCount > 0) {
                    for (GroupPointer = 0; GroupPointer < GroupCount; GroupPointer++) {
                        //
                        // ----- Read Response
                        //
                        GroupID = core.docProperties.getInteger("MemberRules." + GroupPointer + ".ID");
                        RuleNeeded = core.docProperties.getBoolean("MemberRules." + GroupPointer);
                        DateExpires = core.docProperties.getDate("MemberRules." + GroupPointer + ".DateExpires");
                        if (DateExpires == DateTime.MinValue) {
                            DateExpiresVariant = DBNull.Value;
                        } else {
                            DateExpiresVariant = DateExpires;
                        }
                        //
                        // ----- Update Record
                        //
                        CSRule = core.db.csOpen("Member Rules", "(MemberID=" + PeopleID + ")and(GroupID=" + GroupID + ")", "", false, 0, false, false, "Active,MemberID,GroupID,DateExpires");
                        if (!core.db.csOk(CSRule)) {
                            //
                            // No record exists
                            //
                            if (RuleNeeded) {
                                //
                                // No record, Rule needed, add it
                                //
                                core.db.csClose(ref CSRule);
                                CSRule = core.db.csInsertRecord("Member Rules");
                                if (core.db.csOk(CSRule)) {
                                    core.db.csSet(CSRule, "Active", true);
                                    core.db.csSet(CSRule, "MemberID", PeopleID);
                                    core.db.csSet(CSRule, "GroupID", GroupID);
                                    core.db.csSet(CSRule, "DateExpires", DateExpires);
                                }
                                core.db.csClose(ref CSRule);
                            } else {
                                //
                                // No record, no Rule needed, ignore it
                                //
                                core.db.csClose(ref CSRule);
                            }
                        } else {
                            //
                            // Record exists
                            //
                            if (RuleNeeded) {
                                //
                                // record exists, and it is needed, update the DateExpires if changed
                                //
                                RuleActive = core.db.csGetBoolean(CSRule, "active");
                                RuleDateExpires = core.db.csGetDate(CSRule, "DateExpires");
                                if ((!RuleActive) || (RuleDateExpires != DateExpires)) {
                                    core.db.csSet(CSRule, "Active", true);
                                    core.db.csSet(CSRule, "DateExpires", DateExpires);
                                }
                                core.db.csClose(ref CSRule);
                            } else {
                                //
                                // record exists and it is not needed, delete it
                                //
                                MemberRuleID = core.db.csGetInteger(CSRule, "ID");
                                core.db.csClose(ref CSRule);
                                core.db.deleteTableRecord("ccMemberRules", MemberRuleID, "Default");
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //===========================================================================
        //
        //===========================================================================
        //
        private string GetForm_Error(string UserError, string DeveloperError) {
            string tempGetForm_Error = null;
            try {
                //
                if (!string.IsNullOrEmpty(DeveloperError)) {
                    throw (new Exception("error [" + DeveloperError + "], user error [" + UserError + "]"));
                }
                if (!string.IsNullOrEmpty(UserError)) {
                    errorController.addUserError(core, UserError);
                    tempGetForm_Error = AdminFormErrorOpen + errorController.getUserError(core) + AdminFormErrorClose;
                }
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Error;
        }
        //
        //=============================================================================
        // Create a child content
        //=============================================================================
        //
        private string GetContentChildTool() {
            string result = "";
            try {
                //
                bool IsEmptyList = false;
                int ParentContentID = 0;
                string ParentContentName = null;
                string ChildContentName = "";
                int ChildContentID = 0;
                bool AddAdminMenuEntry = false;
                int CS = 0;
                stringBuilderLegacyController Content = new stringBuilderLegacyController();
                string FieldValue = null;
                bool NewGroup = false;
                int GroupID = 0;
                string NewGroupName = "";
                string Button = null;
                adminUIController Adminui = new adminUIController(core);
                string Caption = null;
                string Description = "";
                string ButtonList = "";
                bool BlockForm = false;
                //
                Button = core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    //
                    //
                    //
                    return core.webServer.redirect("/" + core.appConfig.adminRoute, "GetContentChildTool, Cancel Button Pressed");
                } else if (!core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                    //
                    //
                    //
                    ButtonList = ButtonCancel;
                    Content.Add(Adminui.GetFormBodyAdminOnly());
                } else {
                    //
                    if (Button != ButtonOK) {
                        //
                        // Load defaults
                        //
                        ParentContentID = core.docProperties.getInteger("ParentContentID");
                        if (ParentContentID == 0) {
                            ParentContentID = cdefModel.getContentId(core, "Page Content");
                        }
                        AddAdminMenuEntry = true;
                        GroupID = 0;
                    } else {
                        //
                        // Process input
                        //
                        ParentContentID = core.docProperties.getInteger("ParentContentID");
                        ParentContentName = cdefModel.getContentNameByID(core, ParentContentID);
                        ChildContentName = core.docProperties.getText("ChildContentName");
                        AddAdminMenuEntry = core.docProperties.getBoolean("AddAdminMenuEntry");
                        GroupID = core.docProperties.getInteger("GroupID");
                        NewGroup = core.docProperties.getBoolean("NewGroup");
                        NewGroupName = core.docProperties.getText("NewGroupName");
                        //
                        if ((string.IsNullOrEmpty(ParentContentName)) || (string.IsNullOrEmpty(ChildContentName))) {
                            errorController.addUserError(core, "You must select a parent and provide a child name.");
                        } else {
                            //
                            // Create Definition
                            //
                            Description = Description + "<div>&nbsp;</div>"
                                + "<div>Creating content [" + ChildContentName + "] from [" + ParentContentName + "]</div>";
                            cdefModel.createContentChild(core, ChildContentName, ParentContentName, core.doc.sessionContext.user.id);
                            ChildContentID = cdefModel.getContentId(core, ChildContentName);
                            //
                            // Create Group and Rule
                            //
                            if (NewGroup && (!string.IsNullOrEmpty(NewGroupName))) {
                                CS = core.db.csOpen("Groups", "name=" + core.db.encodeSQLText(NewGroupName));
                                if (core.db.csOk(CS)) {
                                    Description = Description + "<div>Group [" + NewGroupName + "] already exists, using existing group.</div>";
                                    GroupID = core.db.csGetInteger(CS, "ID");
                                } else {
                                    Description = Description + "<div>Creating new group [" + NewGroupName + "]</div>";
                                    core.db.csClose(ref CS);
                                    CS = core.db.csInsertRecord("Groups");
                                    if (core.db.csOk(CS)) {
                                        GroupID = core.db.csGetInteger(CS, "ID");
                                        core.db.csSet(CS, "Name", NewGroupName);
                                        core.db.csSet(CS, "Caption", NewGroupName);
                                    }
                                }
                                core.db.csClose(ref CS);
                            }
                            if (GroupID != 0) {
                                CS = core.db.csInsertRecord("Group Rules");
                                if (core.db.csOk(CS)) {
                                    Description = Description + "<div>Assigning group [" + core.db.getRecordName("Groups", GroupID) + "] to edit content [" + ChildContentName + "].</div>";
                                    core.db.csSet(CS, "GroupID", GroupID);
                                    core.db.csSet(CS, "ContentID", ChildContentID);
                                }
                                core.db.csClose(ref CS);
                            }
                            //
                            // Add Admin Menu Entry
                            //
                            if (AddAdminMenuEntry) {
                                //
                                // Add Navigator entries
                                //
                                //                    cmc = core.main_cs_getv()
                                //                    MenuContentName = cnNavigatorEntries
                                //                    SupportAddonID = core.csv_IsContentFieldSupported(MenuContentName, "AddonID")
                                //                    SupportGuid = core.csv_IsContentFieldSupported(MenuContentName, "ccGuid")
                                //                    CS = core.app.csOpen(cnNavigatorEntries, "ContentID=" & ParentContentID)
                                //                    Do While core.app.csv_IsCSOK(CS)
                                //                        ParentID = core.app.csv_cs_getText(CS, "ID")
                                //                        ParentName = core.app.csv_cs_getText(CS, "name")
                                //                        AdminOnly = core.db.cs_getBoolean(CS, "AdminOnly")
                                //                        DeveloperOnly = core.db.cs_getBoolean(CS, "DeveloperOnly")
                                //                        CSEntry = core.app.csv_InsertCSRecord(MenuContentName, SystemMemberID)
                                //                        If core.app.csv_IsCSOK(CSEntry) Then
                                //                            If ParentID = 0 Then
                                //                                Call core.app.csv_SetCS(CSEntry, "ParentID", Null)
                                //                            Else
                                //                                Call core.app.csv_SetCS(CSEntry, "ParentID", ParentID)
                                //                            End If
                                //                            Call core.app.csv_SetCS(CSEntry, "ContentID", ChildContentID)
                                //                            Call core.app.csv_SetCS(CSEntry, "name", ChildContentName)
                                //                            Call core.app.csv_SetCS(CSEntry, "LinkPage", "")
                                //                            Call core.app.csv_SetCS(CSEntry, "SortOrder", "")
                                //                            Call core.app.csv_SetCS(CSEntry, "AdminOnly", AdminOnly)
                                //                            Call core.app.csv_SetCS(CSEntry, "DeveloperOnly", DeveloperOnly)
                                //                            Call core.app.csv_SetCS(CSEntry, "NewWindow", False)
                                //                            Call core.app.csv_SetCS(CSEntry, "Active", True)
                                //                            If SupportAddonID Then
                                //                                Call core.app.csv_SetCS(CSEntry, "AddonID", "")
                                //                            End If
                                //                            If SupportGuid Then
                                //                                GuidGenerator = New guidClass
                                //                                ccGuid = Guid.NewGuid.ToString()
                                //                                GuidGenerator = Nothing
                                //                                Call core.app.csv_SetCS(CSEntry, "ccGuid", ccGuid)
                                //                            End If
                                //                        End If
                                //                        Call core.app.csv_CloseCS(CSEntry)
                                //                        'Call core.csv_VerifyNavigatorEntry2(ccGuid, menuNameSpace, MenuName, ChildContenName, "", "", AdminOnly, DeveloperOnly, False, True, cnNavigatorEntries, "")
                                //                        'Call core.main_CreateAdminMenu(MenuName, ChildContentName, ChildContentName, "", ChildContentName, AdminOnly, DeveloperOnly, False)
                                //                        Description = Description _
                                //                            & "<div>Creating navigator entry for [" & ChildContentName & "] under entry [" & ParentName & "].</div>"
                                //                        core.main_NextCSRecord (CS)
                                //                    Loop
                                //                    Call core.app.closeCS(CS)
                                //
                                // Add Legacy Navigator Entries
                                //
                                // -- deprecated
                                //CS = core.db.cs_open(cnNavigatorEntries, "ContentID=" & ParentContentID)
                                //Do While core.db.cs_ok(CS)
                                //    MenuName = core.db.cs_get(CS, "name")
                                //    AdminOnly = core.db.cs_getBoolean(CS, "AdminOnly")
                                //    DeveloperOnly = core.db.cs_getBoolean(CS, "DeveloperOnly")
                                //    If MenuName = "" Then
                                //        MenuName = "Site Content"
                                //    End If
                                //    Call Controllers.appBuilderController.admin_VerifyAdminMenu(core, MenuName, ChildContentName, ChildContentName, "", ChildContentName, AdminOnly, DeveloperOnly, False)
                                //    Description = Description _
                                //        & "<div>Creating Legacy site menu for [" & ChildContentName & "] under entry [" & MenuName & "].</div>"
                                //    core.db.cs_goNext(CS)
                                //Loop
                                //Call core.db.cs_Close(CS)
                            }
                            //
                            Description = Description + "<div>&nbsp;</div>"
                                + "<div>Your new content is ready. <a href=\"?" + RequestNameAdminForm + "=22\">Click here</a> to create another Content Definition, or hit [Cancel] to return to the main menu.</div>";
                            ButtonList = ButtonCancel;
                            BlockForm = true;
                        }
                        core.doc.clearMetaData();
                        core.cache.invalidateAll();
                    }
                    //
                    // Get the form
                    //
                    if (!BlockForm) {
                        Content.Add(Adminui.EditTableOpen);
                        //
                        FieldValue = "<select size=\"1\" name=\"ParentContentID\" ID=\"\"><option value=\"\">Select One</option>";
                        FieldValue = FieldValue + GetContentChildTool_Options(0, ParentContentID);
                        FieldValue = FieldValue + "</select>";
                        //FieldValue = core.htmldoc.main_GetFormInputSelect2("ParentContentID", CStr(ParentContentID), "Content", "(AllowContentChildTool<>0)")

                        Content.Add(Adminui.GetEditRow(FieldValue, "Parent Content Name", "", false, false, ""));
                        //
                        FieldValue = core.html.inputText("ChildContentName", ChildContentName, 1, 40);
                        Content.Add(Adminui.GetEditRow(FieldValue, "New Child Content Name", "", false, false, ""));
                        //
                        FieldValue = core.html.inputRadio("NewGroup", false.ToString(), NewGroup.ToString()) + core.html.selectFromContent("GroupID", GroupID, "Groups", "", "", "", ref IsEmptyList) + "(Select a current group)"
                            + "<br>" + core.html.inputRadio("NewGroup", true.ToString(), NewGroup.ToString()) + core.html.inputText("NewGroupName", NewGroupName) + "(Create a new group)";
                        Content.Add(Adminui.GetEditRow(FieldValue, "Content Manager Group", "", false, false, ""));
                        //            '
                        //            FieldValue = core.main_GetFormInputCheckBox2("AddAdminMenuEntry", AddAdminMenuEntry) & "(Add Navigator Entry under Manager Site Content &gt; Advanced)"
                        //            Call Content.Add(AdminUI.GetEditRow( FieldValue, "Add Menu Entry", "", False, False, ""))
                        //
                        Content.Add(Adminui.EditTableClose);
                        Content.Add("</td></tr>" + kmaEndTable);
                        //
                        ButtonList = ButtonOK + "," + ButtonCancel;
                    }
                    Content.Add(core.html.inputHidden(rnAdminSourceForm, AdminFormContentChildTool));
                }
                //
                Caption = "Create Content Definition";
                Description = "<div>This tool is used to create content definitions that help segregate your content into authorable segments.</div>" + Description;
                result = Adminui.GetBody(Caption, ButtonList, "", false, false, Description, "", 0, Content.Text);
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //=============================================================================
        // Create a child content
        //=============================================================================
        //
        private string GetContentChildTool_Options(int ParentID, int DefaultValue) {
            string returnOptions = "";
            try {
                //
                string SQL = null;
                int CS = 0;
                int RecordID = 0;
                string RecordName = null;
                //
                if (ParentID == 0) {
                    SQL = "select Name, ID from ccContent where ((ParentID<1)or(Parentid is null)) and (AllowContentChildTool<>0);";
                } else {
                    SQL = "select Name, ID from ccContent where ParentID=" + ParentID + " and (AllowContentChildTool<>0) and not (allowcontentchildtool is null);";
                }
                CS = core.db.csOpenSql_rev("Default", SQL);
                while (core.db.csOk(CS)) {
                    RecordName = core.db.csGet(CS, "Name");
                    RecordID = core.db.csGetInteger(CS, "ID");
                    if (RecordID == DefaultValue) {
                        returnOptions = returnOptions + "<option value=\"" + RecordID + "\" selected>" + core.db.csGet(CS, "name") + "</option>";
                    } else {
                        returnOptions = returnOptions + "<option value=\"" + RecordID + "\" >" + core.db.csGet(CS, "name") + "</option>";
                    }
                    returnOptions = returnOptions + GetContentChildTool_Options(RecordID, DefaultValue);
                    core.db.csGoNext(CS);
                }
                core.db.csClose(ref CS);
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnOptions;
        }
        //
        //=============================================================================
        // Create a child content
        //=============================================================================
        //
        //Private Function GetForm_PageContentMap_OpenNodeList(Criteria As String) As String
        //    On Error GoTo //ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.GetForm_PageContentMap_OpenNodeList")
        //    '
        //    Dim CS as integer
        //    Dim ParentID as integer
        //    '
        //    CS = core.app.csOpen("Page Content", Criteria, , False, , , "ID,ParentID")
        //    If core.app.csv_IsCSOK(CS) Then
        //        Do While core.app.csv_IsCSOK(CS)
        //            ParentID = core.app.cs_getInteger(CS, "ParentID")
        //            If ParentID <> 0 Then
        //                GetForm_PageContentMap_OpenNodeList = GetForm_PageContentMap_OpenNodeList("ID=" & ParentID)
        //                End If
        //            GetForm_PageContentMap_OpenNodeList = GetForm_PageContentMap_OpenNodeList & "," & core.app.cs_getInteger(CS, "ID")
        //            core.main_NextCSRecord (CS)
        //            Loop
        //        End If
        //    Call core.app.closeCS(CS)
        //    If GetForm_PageContentMap_OpenNodeList <> "" Then
        //        GetForm_PageContentMap_OpenNodeList = Mid(GetForm_PageContentMap_OpenNodeList, 2)
        //        End If
        //    '''Dim th as integer: Exit Function
        //    '
        //    ' ----- Error Trap
        //    '
        ////ErrorTrap:
        //    Call HandleClassTrapErrorBubble("GetForm_PageContentMap_OpenNodeList")
        //End Function
        //
        //========================================================================
        //
        //========================================================================
        //
        private string GetForm_HouseKeepingControl() {
            string tempGetForm_HouseKeepingControl = null;
            try {
                //
                stringBuilderLegacyController Content = new stringBuilderLegacyController();
                int CSServers = 0;
                string Copy = null;
                string SQL = null;
                string Button = null;
                int PagesTotal = 0;
                string Caption = null;
                DateTime DateValue = default(DateTime);
                string AgeInDays = null;
                int ArchiveRecordAgeDays = 0;
                string ArchiveTimeOfDay = null;
                bool ArchiveAllowFileClean = false;
                adminUIController Adminui = new adminUIController(core);
                string ButtonList = "";
                string Description = null;
                //
                Button = core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    //
                    //
                    //
                    return core.webServer.redirect("/" + core.appConfig.adminRoute, "HouseKeepingControl, Cancel Button Pressed");
                } else if (!core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                    //
                    //
                    //
                    ButtonList = ButtonCancel;
                    Content.Add(Adminui.GetFormBodyAdminOnly());
                } else {
                    //
                    Content.Add(Adminui.EditTableOpen);
                    //
                    // Set defaults
                    //
                    ArchiveRecordAgeDays = (core.siteProperties.getInteger("ArchiveRecordAgeDays", 0));
                    ArchiveTimeOfDay = core.siteProperties.getText("ArchiveTimeOfDay", "12:00:00 AM");
                    ArchiveAllowFileClean = (core.siteProperties.getBoolean("ArchiveAllowFileClean", false));
                    //ArchiveAllowLogClean = genericController.EncodeBoolean(core.main_GetSiteProperty2("ArchiveAllowLogClean", False))

                    //
                    // Process Requests
                    //
                    switch (Button) {
                        case ButtonOK:
                        case ButtonSave:
                            //
                            ArchiveRecordAgeDays = core.docProperties.getInteger("ArchiveRecordAgeDays");
                            core.siteProperties.setProperty("ArchiveRecordAgeDays", genericController.encodeText(ArchiveRecordAgeDays));
                            //
                            ArchiveTimeOfDay = core.docProperties.getText("ArchiveTimeOfDay");
                            core.siteProperties.setProperty("ArchiveTimeOfDay", ArchiveTimeOfDay);
                            //
                            ArchiveAllowFileClean = core.docProperties.getBoolean("ArchiveAllowFileClean");
                            core.siteProperties.setProperty("ArchiveAllowFileClean", genericController.encodeText(ArchiveAllowFileClean));
                            break;
                    }
                    //
                    if (Button == ButtonOK) {
                        return core.webServer.redirect("/" + core.appConfig.adminRoute, "StaticPublishControl, OK Button Pressed");
                    }
                    //
                    // ----- Status
                    //
                    Content.Add(genericController.StartTableRow() + "<td colspan=\"3\" class=\"ccPanel3D ccAdminEditSubHeader\"><b>Status</b>" + kmaEndTableCell + kmaEndTableRow);
                    //
                    // ----- Visits Found
                    //
                    PagesTotal = 0;
                    SQL = "SELECT Count(ID) as Result FROM ccVisits;";
                    CSServers = core.db.csOpenSql_rev("Default", SQL);
                    if (core.db.csOk(CSServers)) {
                        PagesTotal = core.db.csGetInteger(CSServers, "Result");
                    }
                    core.db.csClose(ref CSServers);
                    Content.Add(Adminui.GetEditRow(SpanClassAdminNormal + PagesTotal, "Visits Found", "", false, false, ""));
                    //
                    // ----- Oldest Visit
                    //
                    Copy = "unknown";
                    AgeInDays = "unknown";
                    SQL = core.db.GetSQLSelect("default", "ccVisits", "DateAdded", "", "ID", "", 1);
                    CSServers = core.db.csOpenSql_rev("Default", SQL);
                    //SQL = "SELECT Top 1 DateAdded FROM ccVisits order by ID;"
                    //CSServers = core.app_openCsSql_Rev_Internal("Default", SQL)
                    if (core.db.csOk(CSServers)) {
                        DateValue = core.db.csGetDate(CSServers, "DateAdded");
                        if (DateValue != DateTime.MinValue) {
                            Copy = genericController.encodeText(DateValue);
                            AgeInDays = genericController.encodeText(encodeInteger(Math.Floor(encodeNumber(core.doc.profileStartTime - DateValue))));
                        }
                    }
                    core.db.csClose(ref CSServers);
                    Content.Add(Adminui.GetEditRow(SpanClassAdminNormal + Copy + " (" + AgeInDays + " days)", "Oldest Visit", "", false, false, ""));
                    //
                    // ----- Viewings Found
                    //
                    PagesTotal = 0;
                    SQL = "SELECT Count(ID) as result  FROM ccViewings;";
                    CSServers = core.db.csOpenSql_rev("Default", SQL);
                    if (core.db.csOk(CSServers)) {
                        PagesTotal = core.db.csGetInteger(CSServers, "Result");
                    }
                    core.db.csClose(ref CSServers);
                    Content.Add(Adminui.GetEditRow(SpanClassAdminNormal + PagesTotal, "Viewings Found", "", false, false, ""));
                    //
                    Content.Add(genericController.StartTableRow() + "<td colspan=\"3\" class=\"ccPanel3D ccAdminEditSubHeader\"><b>Options</b>" + kmaEndTableCell + kmaEndTableRow);
                    //
                    Caption = "Archive Age";
                    Copy = core.html.inputText("ArchiveRecordAgeDays", ArchiveRecordAgeDays.ToString(), -1, 20) + "&nbsp;Number of days to keep visit records. 0 disables housekeeping.";
                    Content.Add(Adminui.GetEditRow(Copy, Caption));
                    //
                    Caption = "Housekeeping Time";
                    Copy = core.html.inputText("ArchiveTimeOfDay", ArchiveTimeOfDay, -1, 20) + "&nbsp;The time of day when record deleting should start.";
                    Content.Add(Adminui.GetEditRow(Copy, Caption));
                    //
                    Caption = "Purge Content Files";
                    Copy = core.html.inputCheckbox("ArchiveAllowFileClean", ArchiveAllowFileClean) + "&nbsp;Delete Contensive content files with no associated database record.";
                    Content.Add(Adminui.GetEditRow(Copy, Caption));
                    //
                    Content.Add(Adminui.EditTableClose);
                    Content.Add(core.html.inputHidden(rnAdminSourceForm, AdminformHousekeepingControl));
                    ButtonList = ButtonCancel + ",Refresh," + ButtonSave + "," + ButtonOK;
                }
                //
                Caption = "Data Housekeeping Control";
                Description = "This tool is used to control the database record housekeeping process. This process deletes visit history records, so care should be taken before making any changes.";
                tempGetForm_HouseKeepingControl = Adminui.GetBody(Caption, ButtonList, "", false, false, Description, "", 0, Content.Text);
                //
                core.html.addTitle(Caption);
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_HouseKeepingControl;
        }
        //
        //
        //
        //Private Function GetPropertyControl(Name As String, FieldType as integer, ProcessRequest As Boolean, DefaultValue As String) As String
        //    On Error GoTo //ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.GetPropertyControl")
        //    '
        //    Dim CurrentValue As String
        //    '
        //    If ProcessRequest Then
        //        CurrentValue = core.main_GetStreamText2(Name)
        //        Call core.app.setSiteProperty(Name, CurrentValue)
        //    Else
        //        CurrentValue = core.main_GetSiteProperty2(Name, DefaultValue)
        //    End If
        //    Select Case FieldType
        //        Case FieldTypeBoolean
        //            GetPropertyControl = core.main_GetFormInputCheckBox2(Name, genericController.EncodeBoolean(CurrentValue))
        //        Case Else
        //            GetPropertyControl = core.main_GetFormInputText2(Name, CurrentValue)
        //    End Select
        //    '''Dim th as integer: Exit Function
        //    '
        //    ' ----- Error Trap
        //    '
        ////ErrorTrap:
        //    Call HandleClassTrapErrorBubble("GetPropertyControl")
        //End Function
        //
        //
        //
        private string GetPropertyHTMLControl(string Name, bool ProcessRequest, string DefaultValue, bool readOnlyField) {
            string tempGetPropertyHTMLControl = null;
            try {
                //
                string CurrentValue = null;
                //
                if (readOnlyField) {
                    tempGetPropertyHTMLControl = "<div style=\"border:1px solid #808080; padding:20px;\">" + genericController.decodeHtml(core.siteProperties.getText(Name, DefaultValue)) + "</div>";
                } else if (ProcessRequest) {
                    CurrentValue = core.docProperties.getText(Name);
                    core.siteProperties.setProperty(Name, CurrentValue);
                    tempGetPropertyHTMLControl = core.html.getFormInputHTML(Name, CurrentValue);
                } else {
                    CurrentValue = core.siteProperties.getText(Name, DefaultValue);
                    tempGetPropertyHTMLControl = core.html.getFormInputHTML(Name, CurrentValue);
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetPropertyHTMLControl;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private string admin_GetForm_StyleEditor() {
            return "<div><p>Site Styles are not longer supported. Instead add your styles to addons and add them with template dependencies.</p></div>";
            //            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_StyleEditor")
            //            '
            //            Dim Content As New stringBuilderLegacyController
            //            Dim Button As String
            //            Dim Copy As String
            //            Dim ButtonList As String = ""
            //            Dim Adminui As New adminUIController(core)
            //            Dim Caption As String
            //            Dim Description As String
            //            'Dim StyleSN as integer
            //            Dim AllowCSSReset As Boolean
            //            '
            //            Button = core.docProperties.getText(RequestNameButton)
            //            If Button = ButtonCancel Then
            //                '
            //                '
            //                '
            //                Call core.webServer.redirect("/" & core.appConfig.adminRoute, "StyleEditor, Cancel Button Pressed", False)
            //            ElseIf Not core.doc.authContext.isAuthenticatedAdmin(core) Then
            //                '
            //                '
            //                '
            //                ButtonList = ButtonCancel
            //                Content.Add(Adminui.GetFormBodyAdminOnly())
            //            Else
            //                'StyleSN = genericController.EncodeInteger(core.main_GetSiteProperty2("StylesheetSerialNumber", false ))
            //                AllowCSSReset = False
            //                If True Then ' 4.1.101" Then
            //                    AllowCSSReset = (core.siteProperties.getBoolean("Allow CSS Reset", False))
            //                End If
            //                '
            //                Copy = core.html.html_GetFormInputTextExpandable("StyleEditor", core.cdnFiles.readFile(DynamicStylesFilename), 20)
            //                Copy = genericController.vbReplace(Copy, " cols=""100""", " style=""width:100%;""", 1, 99, vbTextCompare)
            //                Copy = "" _
            //                    & "<div style=""padding:10px;"">" & core.html.html_GetFormInputCheckBox2(RequestNameAllowCSSReset, AllowCSSReset) & "&nbsp;Include Contensive reset styles</div>" _
            //                    & "<div style=""padding:10px;"">" & Copy & "</div>"

            //                '& "<div style=""padding:10px;"">" & core.main_GetFormInputCheckBox2(RequestNameInlineStyles, (StyleSN = 0)) & "&nbsp;Force site styles inline</div>"

            //                Content.Add(Copy)
            //                ButtonList = ButtonCancel & "," & ButtonRefresh & "," & ButtonSave & "," & ButtonOK
            //                Content.Add(core.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormStyleEditor))
            //            End If
            //            '
            //            Description = "" _
            //                & "<p>This tool is used to edit the site styles. When a public page is rendered, the head tag includes styles in this order:" _
            //                & "<ol>" _
            //                & "<li>Contensive reset styles (optional)</li>" _
            //                & "<li>Contensive styles</li>" _
            //                & "<li>These site styles (optionally inline)</li>" _
            //                & "<li>Shared styles from the template in use</li>" _
            //                & "<li>Exclusive styles from the template in use</li>" _
            //                & "<li>Add-on styles, first the default styles, then any custom styles included.</li>" _
            //                & "</ul>" _
            //                & ""
            //            admin_GetForm_StyleEditor = Adminui.GetBody("Site Styles", ButtonList, "", True, True, Description, "", 0, Content.Text)
            //            '
            //            Call core.html.main_AddPagetitle("Style Editor")
            //            Exit Function
            //            '
            //            ' ----- Error Trap
            //            '
            ////ErrorTrap:
            //            Call handleLegacyClassError3("GetForm_StyleEditor")
            //            '
        }
        //
        //
        //
        private string GetForm_ControlPage_CopyContent(string Caption, string CopyName) {
            string tempGetForm_ControlPage_CopyContent = null;
            try {
                //
                int CS = 0;
                int RecordID = 0;
                string EditIcon = null;
                string Copy = "";
                //
                const string ContentName = "Copy Content";
                //
                CS = core.db.csOpen(ContentName, "Name=" + core.db.encodeSQLText(CopyName));
                if (core.db.csOk(CS)) {
                    RecordID = core.db.csGetInteger(CS, "ID");
                    Copy = core.db.csGetText(CS, "copy");
                }
                core.db.csClose(ref CS);
                //
                if (RecordID != 0) {
                    EditIcon = "<a href=\"?cid=" + cdefModel.getContentId(core, ContentName) + "&id=" + RecordID + "&" + RequestNameAdminForm + "=4\" target=_blank><img src=\"/ccLib/images/IconContentEdit.gif\" border=0 alt=\"Edit content\" valign=absmiddle></a>";
                } else {
                    EditIcon = "<a href=\"?cid=" + cdefModel.getContentId(core, ContentName) + "&" + RequestNameAdminForm + "=4&" + rnAdminAction + "=2&ad=1&wc=" + genericController.EncodeURL("name=" + CopyName) + "\" target=_blank><img src=\"/ccLib/images/IconContentEdit.gif\" border=0 alt=\"Edit content\" valign=absmiddle></a>";
                }
                if (string.IsNullOrEmpty(Copy)) {
                    Copy = "&nbsp;";
                }
                //
                tempGetForm_ControlPage_CopyContent = ""
                    + genericController.StartTable(4, 0, 1) + "<tr>"
                    + "<td width=150 align=right>" + Caption + "<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=150 height=1></td>"
                    + "<td width=20 align=center>" + EditIcon + "</td>"
                    + "<td>" + Copy + "&nbsp;</td>"
                    + "</tr></table>";
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_ControlPage_CopyContent;
        }
        //
        //=============================================================================
        //   Export the Admin List form results
        //=============================================================================
        //
        private string GetForm_Index_Export(cdefModel adminContent, editRecordClass editRecord) {
            string tempGetForm_Index_Export = null;
            try {
                //
                bool AllowContentAccess = false;
                string ButtonList = "";
                string ExportName = null;
                adminUIController Adminui = new adminUIController(core);
                string Description = null;
                string Content = "";
                int ExportType = 0;
                string Button = null;
                int RecordLimit = 0;
                int recordCnt = 0;
                //Dim DataSourceName As String
                //Dim DataSourceType As Integer
                string sqlFieldList = "";
                string SQLFrom = "";
                string SQLWhere = "";
                string SQLOrderBy = "";
                bool IsLimitedToSubContent = false;
                string ContentAccessLimitMessage = "";
                Dictionary<string, bool> FieldUsedInColumns = new Dictionary<string, bool>();
                Dictionary<string, bool> IsLookupFieldValid = new Dictionary<string, bool>();
                indexConfigClass IndexConfig = null;
                string SQL = null;
                int CS = 0;
                //Dim RecordTop As Integer
                //Dim RecordsPerPage As Integer
                bool IsRecordLimitSet = false;
                string RecordLimitText = null;
                bool allowContentEdit = false;
                bool allowContentAdd = false;
                bool allowContentDelete = false;
                var tmpList = new List<string>();
                dataSourceModel datasource = dataSourceModel.create(core, adminContent.dataSourceId, ref tmpList);
                //
                // ----- Process Input
                //
                Button = core.docProperties.getText("Button");
                if (Button == ButtonCancelAll) {
                    //
                    // Cancel out to the main page
                    //
                    return core.webServer.redirect("?", "CancelAll button pressed on Index Export");
                } else if (Button != ButtonCancel) {
                    //
                    // get content access rights
                    //
                    core.doc.sessionContext.getContentAccessRights(core, adminContent.Name, ref allowContentEdit, ref allowContentAdd, ref allowContentDelete);
                    if (!allowContentEdit) {
                        //If Not core.doc.authContext.user.main_IsContentManager2(AdminContent.Name) Then
                        //
                        // You must be a content manager of this content to use this tool
                        //
                        Content = ""
                            + "<p>You must be a content manager of " + adminContent.Name + " to use this tool. Hit Cancel to return to main admin page.</p>"
                            + core.html.inputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) + "";
                        ButtonList = ButtonCancelAll;
                    } else {
                        IsRecordLimitSet = false;
                        if (string.IsNullOrEmpty(Button)) {
                            //
                            // Set Defaults
                            //
                            ExportName = "";
                            ExportType = 1;
                            RecordLimit = 0;
                            RecordLimitText = "";
                        } else {
                            ExportName = core.docProperties.getText("ExportName");
                            ExportType = core.docProperties.getInteger("ExportType");
                            RecordLimitText = core.docProperties.getText("RecordLimit");
                            if (!string.IsNullOrEmpty(RecordLimitText)) {
                                IsRecordLimitSet = true;
                                RecordLimit = genericController.encodeInteger(RecordLimitText);
                            }
                        }
                        if (string.IsNullOrEmpty(ExportName)) {
                            ExportName = adminContent.Name + " export for " + core.doc.sessionContext.user.name;
                        }
                        //
                        // Get the SQL parts
                        //
                        //DataSourceName = core.db.getDataSourceNameByID(adminContent.dataSourceId)
                        //DataSourceType = core.db.getDataSourceType(DataSourceName)
                        IndexConfig = LoadIndexConfig(adminContent);
                        //RecordTop = IndexConfig.RecordTop
                        //RecordsPerPage = IndexConfig.RecordsPerPage
                        SetIndexSQL(adminContent, editRecord, IndexConfig, ref AllowContentAccess, ref sqlFieldList, ref SQLFrom, ref SQLWhere, ref SQLOrderBy, ref IsLimitedToSubContent, ref ContentAccessLimitMessage, ref FieldUsedInColumns, IsLookupFieldValid);
                        if (!AllowContentAccess) {
                            //
                            // This should be caught with check earlier, but since I added this, and I never make mistakes, I will leave this in case there is a mistake in the earlier code
                            //
                            errorController.addUserError(core, "Your account does not have access to any records in '" + adminContent.Name + "'.");
                        } else {
                            //
                            // Get the total record count
                            //
                            SQL = "select count(" + adminContent.ContentTableName + ".ID) as cnt from " + SQLFrom + " where " + SQLWhere;
                            CS = core.db.csOpenSql_rev(datasource.Name, SQL);
                            if (core.db.csOk(CS)) {
                                recordCnt = core.db.csGetInteger(CS, "cnt");
                            }
                            core.db.csClose(ref CS);
                            //
                            // Build the SQL
                            //
                            SQL = "select";
                            if (IsRecordLimitSet && (datasource.type != DataSourceTypeODBCMySQL)) {
                                SQL += " Top " + RecordLimit;
                            }
                            SQL += " " + adminContent.ContentTableName + ".* From " + SQLFrom + " WHERE " + SQLWhere;
                            if (!string.IsNullOrEmpty(SQLOrderBy)) {
                                SQL += " Order By" + SQLOrderBy;
                            }
                            if (IsRecordLimitSet && (datasource.type == DataSourceTypeODBCMySQL)) {
                                SQL += " Limit " + RecordLimit;
                            }
                            //
                            // Assumble the SQL
                            //
                            if (recordCnt == 0) {
                                //
                                // There are no records to request
                                //
                                Content = ""
                                    + "<p>This selection has no records.. Hit Cancel to return to the " + adminContent.Name + " list page.</p>"
                                    + core.html.inputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) + "";
                                ButtonList = ButtonCancel;
                            } else if (Button == ButtonRequestDownload) {
                                //
                                // Request the download
                                //
                                switch (ExportType) {
                                    case 1:
                                        var ExportCSVAddon = Models.DbModels.addonModel.create(core, addonGuidExportCSV);
                                        if (ExportCSVAddon == null) {
                                            core.handleException(new ApplicationException("ExportCSV addon not found. Task could not be added to task queue."));
                                        } else {
                                            var docProperties = new Dictionary<string, string>();
                                            docProperties.Add("sql", SQL);
                                            docProperties.Add("ExportName", ExportName);
                                            docProperties.Add("filename", "Export-" + genericController.GetRandomInteger(core).ToString() + ".csv");
                                            var cmdDetail = new cmdDetailClass() {
                                                addonId = ExportCSVAddon.id,
                                                addonName = ExportCSVAddon.name,
                                                docProperties = docProperties
                                            };
                                            taskSchedulerController.addTaskToQueue(core, taskCommandBuildCsv, cmdDetail, false);
                                        }
                                        break;
                                    default:
                                        var ExportXMLAddon = Models.DbModels.addonModel.create(core, addonGuidExportXML);
                                        if (ExportXMLAddon == null) {
                                            core.handleException(new ApplicationException("ExportXML addon not found. Task could not be added to task queue."));
                                        } else {
                                            var docProperties = new Dictionary<string, string>();
                                            docProperties.Add("sql", SQL);
                                            docProperties.Add("ExportName", ExportName);
                                            docProperties.Add("filename", "Export-" + genericController.GetRandomInteger(core).ToString() + ".xml");
                                            var cmdDetail = new cmdDetailClass() {
                                                addonId = ExportXMLAddon.id,
                                                addonName = ExportXMLAddon.name,
                                                docProperties = docProperties
                                            };
                                            taskSchedulerController.addTaskToQueue(core, taskCommandBuildXml, cmdDetail, false);
                                        }
                                        break;
                                }
                                //
                                Content = ""
                                    + "<p>Your export has been requested and will be available shortly in the <a href=\"?" + RequestNameAdminForm + "=" + AdminFormDownloads + "\">Download Manager</a>. Hit Cancel to return to the " + adminContent.Name + " list page.</p>"
                                    + core.html.inputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) + "";
                                //
                                ButtonList = ButtonCancel;
                            } else {
                                //
                                // no button or refresh button, Ask are you sure
                                //
                                Content = Content + "\r<tr>"
                                    + cr2 + "<td class=\"exportTblCaption\">Export Name</td>"
                                    + cr2 + "<td class=\"exportTblInput\">" + core.html.inputText("ExportName", ExportName) + "</td>"
                                    + "\r</tr>";
                                Content = Content + "\r<tr>"
                                    + cr2 + "<td class=\"exportTblCaption\">Export Format</td>"
                                    + cr2 + "<td class=\"exportTblInput\">" + core.html.selectFromList("ExportType", ExportType, "Comma Delimited,XML", "", "") + "</td>"
                                    + "\r</tr>";
                                Content = Content + "\r<tr>"
                                    + cr2 + "<td class=\"exportTblCaption\">Records Found</td>"
                                    + cr2 + "<td class=\"exportTblInput\">" + core.html.inputText("RecordCnt", recordCnt.ToString(), -1, -1, "", false, true) + "</td>"
                                    + "\r</tr>";
                                Content = Content + "\r<tr>"
                                    + cr2 + "<td class=\"exportTblCaption\">Record Limit</td>"
                                    + cr2 + "<td class=\"exportTblInput\">" + core.html.inputText("RecordLimit", RecordLimitText) + "</td>"
                                    + "\r</tr>";
                                if (core.doc.sessionContext.isAuthenticatedDeveloper(core)) {
                                    Content = Content + "\r<tr>"
                                        + cr2 + "<td class=\"exportTblCaption\">Results SQL</td>"
                                        + cr2 + "<td class=\"exportTblInput\"><div style=\"border:1px dashed #ccc;background-color:#fff;padding:10px;\">" + SQL + "</div></td>"
                                        + "\r</tr>"
                                        + "";
                                }
                                //
                                Content = ""
                                    + "\r<table>"
                                    + genericController.htmlIndent(Content) + "\r</table>"
                                    + "";
                                //
                                Content = ""
                                    + "\r<style>"
                                    + cr2 + ".exportTblCaption {width:100px;}"
                                    + cr2 + ".exportTblInput {}"
                                    + "\r</style>"
                                    + Content + core.html.inputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) + "";
                                ButtonList = ButtonCancel + "," + ButtonRequestDownload;
                                if (core.doc.sessionContext.isAuthenticatedDeveloper(core)) {
                                    ButtonList = ButtonList + "," + ButtonRefresh;
                                }
                            }
                        }
                    }
                    //
                    Description = "<p>This tool creates an export of the current admin list page results. If you would like to download the current results, select a format and press OK. Your search results will be submitted for export. Your download will be ready shortly in the download manager. To exit without requesting an output, hit Cancel.</p>";
                    tempGetForm_Index_Export = ""
                        + Adminui.GetBody(adminContent.Name + " Export", ButtonList, "", false, false, Description, "", 10, Content);
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Index_Export;
        }
        //
        //=============================================================================
        //   Print the Configure Index Form
        //=============================================================================
        //
        private string GetForm_Index_SetColumns(cdefModel adminContent, editRecordClass editRecord) {
            string tempGetForm_Index_SetColumns = null;
            try {
                //
                string Button = null;
                string Description = null;
                bool NeedToReloadCDef = false;
                string Title = null;
                string Content = null;
                adminUIController Adminui = new adminUIController(core);
                int ColumnPtr = 0;
                int ColumnWidth = 0;
                string AStart = null;
                int CSPointer = 0;
                int ContentID = 0;
                cdefModel CDef = null;
                string ContentName = null;
                int TargetFieldID = 0;
                string TargetFieldName = null;
                int ColumnWidthTotal = 0;
                int ColumnPointer = 0;
                int fieldId = 0;
                bool AllowContentAutoLoad = false;
                string FieldNameToAdd = null;
                int FieldIDToAdd = 0;
                int CSSource = 0;
                int CSTarget = 0;
                int SourceContentID = 0;
                string SourceName = null;
                bool NeedToReloadConfig = false;
                int InheritedFieldCount = 0;
                string Caption = null;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                int ToolsAction = 0;
                indexConfigClass IndexConfig = null;
                //
                const string RequestNameAddField = "addfield";
                const string RequestNameAddFieldID = "addfieldID";
                //
                //
                //--------------------------------------------------------------------------------
                //   Process Button
                //--------------------------------------------------------------------------------
                //
                Button = core.docProperties.getText(RequestNameButton);
                if (Button == ButtonOK) {
                    return tempGetForm_Index_SetColumns;
                }
                //
                //--------------------------------------------------------------------------------
                //   Load Request
                //--------------------------------------------------------------------------------
                //
                ContentID = adminContent.Id;
                ContentName = cdefModel.getContentNameByID(core, ContentID);
                if (Button == ButtonReset) {
                    core.userProperty.setProperty(IndexConfigPrefix + ContentID.ToString(), "");
                }
                IndexConfig = LoadIndexConfig(adminContent);
                Title = adminContent.Name + " Columns";
                Description = "Use the icons to add, remove and modify your personal column prefernces for this content (" + ContentName + "). Hit OK when complete. Hit Reset to restore your column preferences for this content to the site's default column preferences.";
                ToolsAction = core.docProperties.getInteger("dta");
                TargetFieldID = core.docProperties.getInteger("fi");
                TargetFieldName = core.docProperties.getText("FieldName");
                ColumnPointer = core.docProperties.getInteger("dtcn");
                FieldNameToAdd = genericController.vbUCase(core.docProperties.getText(RequestNameAddField));
                FieldIDToAdd = core.docProperties.getInteger(RequestNameAddFieldID);
                //ButtonList = ButtonCancel & "," & ButtonSelect
                NeedToReloadConfig = core.docProperties.getBoolean("NeedToReloadConfig");
                //
                //--------------------------------------------------------------------------------
                // Process actions
                //--------------------------------------------------------------------------------
                //
                if (ContentID != 0) {
                    CDef = cdefModel.getCdef(core, ContentName);
                    if (ToolsAction != 0) {
                        //
                        // Block contentautoload, then force a load at the end
                        //
                        AllowContentAutoLoad = (core.siteProperties.getBoolean("AllowContentAutoLoad", true));
                        core.siteProperties.setProperty("AllowContentAutoLoad", false);
                        //
                        // Make sure the FieldNameToAdd is not-inherited, if not, create new field
                        //
                        if (FieldIDToAdd != 0) {
                            foreach (KeyValuePair<string, cdefFieldModel> keyValuePair in adminContent.fields) {
                                cdefFieldModel field = keyValuePair.Value;
                                if (field.id == FieldIDToAdd) {
                                    //If CDef.fields(FieldPtr).Name = FieldNameToAdd Then
                                    if (field.inherited) {
                                        SourceContentID = field.contentId;
                                        SourceName = field.nameLc;
                                        CSSource = core.db.csOpen("Content Fields", "(ContentID=" + SourceContentID + ")and(Name=" + core.db.encodeSQLText(SourceName) + ")");
                                        if (core.db.csOk(CSSource)) {
                                            CSTarget = core.db.csInsertRecord("Content Fields");
                                            if (core.db.csOk(CSTarget)) {
                                                core.db.csCopyRecord(CSSource, CSTarget);
                                                core.db.csSet(CSTarget, "ContentID", ContentID);
                                                NeedToReloadCDef = true;
                                            }
                                            core.db.csClose(ref CSTarget);
                                        }
                                        core.db.csClose(ref CSSource);
                                    }
                                    break;
                                }
                            }
                        }
                        //
                        // Make sure all fields are not-inherited, if not, create new fields
                        //
                        foreach (var kvp in IndexConfig.Columns) {
                            indexConfigColumnClass column = kvp.Value;
                            cdefFieldModel field = adminContent.fields[column.Name.ToLower()];
                            if (field.inherited) {
                                SourceContentID = field.contentId;
                                SourceName = field.nameLc;
                                CSSource = core.db.csOpen("Content Fields", "(ContentID=" + SourceContentID + ")and(Name=" + core.db.encodeSQLText(SourceName) + ")");
                                if (core.db.csOk(CSSource)) {
                                    CSTarget = core.db.csInsertRecord("Content Fields");
                                    if (core.db.csOk(CSTarget)) {
                                        core.db.csCopyRecord(CSSource, CSTarget);
                                        core.db.csSet(CSTarget, "ContentID", ContentID);
                                        NeedToReloadCDef = true;
                                    }
                                    core.db.csClose(ref CSTarget);
                                }
                                core.db.csClose(ref CSSource);
                            }
                        }
                        //
                        // get current values for Processing
                        //
                        foreach (var kvp in IndexConfig.Columns) {
                            indexConfigColumnClass column = kvp.Value;
                            ColumnWidthTotal += column.Width;
                        }
                        //
                        // ----- Perform any actions first
                        //
                        switch (ToolsAction) {
                            case ToolsActionAddField: {
                                    //
                                    // Add a field to the index form
                                    //
                                    if (FieldIDToAdd != 0) {
                                        indexConfigColumnClass column = null;
                                        foreach (var kvp in IndexConfig.Columns) {
                                            column = kvp.Value;
                                            column.Width = encodeInteger((column.Width * 80) / (double)ColumnWidthTotal);
                                        }
                                        column = new indexConfigColumnClass();
                                        CSPointer = core.db.csOpenRecord("Content Fields", FieldIDToAdd, false, false);
                                        if (core.db.csOk(CSPointer)) {
                                            column.Name = core.db.csGet(CSPointer, "name");
                                            column.Width = 20;
                                        }
                                        core.db.csClose(ref CSPointer);
                                        IndexConfig.Columns.Add(column.Name.ToLower(), column);
                                        NeedToReloadConfig = true;
                                    }
                                    //
                                    break;
                                }
                            case ToolsActionRemoveField: {
                                    //
                                    // Remove a field to the index form
                                    //
                                    indexConfigColumnClass column = null;
                                    if (IndexConfig.Columns.ContainsKey(TargetFieldName.ToLower())) {
                                        column = IndexConfig.Columns[TargetFieldName.ToLower()];
                                        ColumnWidthTotal = ColumnWidthTotal + column.Width;
                                        IndexConfig.Columns.Remove(TargetFieldName.ToLower());
                                        //
                                        // Normalize the widths of the remaining columns
                                        //
                                        foreach (var kvp in IndexConfig.Columns) {
                                            column = kvp.Value;
                                            column.Width = encodeInteger((1000 * column.Width) / (double)ColumnWidthTotal);
                                        }
                                        NeedToReloadConfig = true;
                                    }
                                    break;
                                }
                            case ToolsActionMoveFieldLeft: {
                                    //
                                    // Move column field left
                                    //
                                    //If IndexConfig.Columns.Count > 1 Then
                                    //    MoveNextColumn = False
                                    //    For ColumnPointer = 1 To IndexConfig.Columns.Count - 1
                                    //        If TargetFieldName = IndexConfig.Columns(ColumnPointer).Name Then
                                    //            With IndexConfig.Columns(ColumnPointer)
                                    //                FieldPointerTemp = .FieldId
                                    //                NameTemp = .Name
                                    //                WidthTemp = .Width
                                    //                .FieldId = IndexConfig.Columns(ColumnPointer - 1).FieldId
                                    //                .Name = IndexConfig.Columns(ColumnPointer - 1).Name
                                    //                .Width = IndexConfig.Columns(ColumnPointer - 1).Width
                                    //            End With
                                    //            With IndexConfig.Columns(ColumnPointer - 1)
                                    //                .FieldId = FieldPointerTemp
                                    //                .Name = NameTemp
                                    //                .Width = WidthTemp
                                    //            End With
                                    //        End If
                                    //    Next
                                    //    NeedToReloadConfig = True
                                    //End If
                                    // end case
                                    break;
                                }
                            case ToolsActionMoveFieldRight: {
                                    //
                                    // Move Index column field right
                                    //
                                    //If IndexConfig.Columns.Count > 1 Then
                                    //    MoveNextColumn = False
                                    //    For ColumnPointer = 0 To IndexConfig.Columns.Count - 2
                                    //        If TargetFieldName = IndexConfig.Columns(ColumnPointer).Name Then
                                    //            With IndexConfig.Columns(ColumnPointer)
                                    //                FieldPointerTemp = .FieldId
                                    //                NameTemp = .Name
                                    //                WidthTemp = .Width
                                    //                .FieldId = IndexConfig.Columns(ColumnPointer + 1).FieldId
                                    //                .Name = IndexConfig.Columns(ColumnPointer + 1).Name
                                    //                .Width = IndexConfig.Columns(ColumnPointer + 1).Width
                                    //            End With
                                    //            With IndexConfig.Columns(ColumnPointer + 1)
                                    //                .FieldId = FieldPointerTemp
                                    //                .Name = NameTemp
                                    //                .Width = WidthTemp
                                    //            End With
                                    //        End If
                                    //    Next
                                    //    NeedToReloadConfig = True
                                    //End If
                                    // end case
                                    break;
                                }
                            case ToolsActionExpand: {
                                    //
                                    // Expand column
                                    //
                                    //ColumnWidthBalance = 0
                                    //If IndexConfig.Columns.Count > 1 Then
                                    //    '
                                    //    ' Calculate the total width of the non-target columns
                                    //    '
                                    //    ColumnWidthIncrease = CInt(ColumnWidthTotal * 0.1)
                                    //    For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
                                    //        If TargetFieldName <> IndexConfig.Columns(ColumnPointer).Name Then
                                    //            ColumnWidthBalance = ColumnWidthBalance + IndexConfig.Columns(ColumnPointer).Width
                                    //        End If
                                    //    Next
                                    //    '
                                    //    ' Adjust all columns
                                    //    '
                                    //    If ColumnWidthBalance > 0 Then
                                    //        For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
                                    //            With IndexConfig.Columns(ColumnPointer)
                                    //                If TargetFieldName = .Name Then
                                    //                    '
                                    //                    ' Target gets 10% increase
                                    //                    '
                                    //                    .Width = Int(.Width + ColumnWidthIncrease)
                                    //                Else
                                    //                    '
                                    //                    ' non-targets get their share of the shrinkage
                                    //                    '
                                    //                    .Width = CInt(.Width - ((ColumnWidthIncrease * .Width) / ColumnWidthBalance))
                                    //                End If
                                    //            End With
                                    //        Next
                                    //        NeedToReloadConfig = True
                                    //    End If
                                    //End If

                                    // end case
                                    break;
                                }
                            case ToolsActionContract: {
                                    //
                                    // Contract column
                                    //
                                    //ColumnWidthBalance = 0
                                    //If IndexConfig.Columns.Count > 0 Then
                                    //    '
                                    //    ' Calculate the total width of the non-target columns
                                    //    '
                                    //    ColumnWidthIncrease = CInt(-(ColumnWidthTotal * 0.1))
                                    //    For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
                                    //        With IndexConfig.Columns(ColumnPointer)
                                    //            If TargetFieldName <> .Name Then
                                    //                ColumnWidthBalance = ColumnWidthBalance + IndexConfig.Columns(ColumnPointer).Width
                                    //            End If
                                    //        End With
                                    //    Next
                                    //    '
                                    //    ' Adjust all columns
                                    //    '
                                    //    If (ColumnWidthBalance > 0) And (ColumnWidthIncrease <> 0) Then
                                    //        For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
                                    //            With IndexConfig.Columns(ColumnPointer)
                                    //                If TargetFieldName = .Name Then
                                    //                    '
                                    //                    ' Target gets 10% increase
                                    //                    '
                                    //                    .Width = Int(.Width + ColumnWidthIncrease)
                                    //                Else
                                    //                    '
                                    //                    ' non-targets get their share of the shrinkage
                                    //                    '
                                    //                    .Width = CInt(.Width - ((ColumnWidthIncrease * FieldWidth) / ColumnWidthBalance))
                                    //                End If
                                    //            End With
                                    //        Next
                                    //        NeedToReloadConfig = True
                                    //    End If
                                    //End If
                                    break;
                                }
                        }
                        //
                        // Reload CDef if it changed
                        //
                        if (NeedToReloadCDef) {
                            core.doc.clearMetaData();
                            core.cache.invalidateAll();
                            CDef = cdefModel.getCdef(core, ContentName);
                        }
                        //
                        // save indexconfig
                        //
                        if (NeedToReloadConfig) {
                            SetIndexSQL_SaveIndexConfig(IndexConfig);
                            IndexConfig = LoadIndexConfig(adminContent);
                        }
                    }
                    //
                    //--------------------------------------------------------------------------------
                    //   Display the form
                    //--------------------------------------------------------------------------------
                    //
                    Stream.Add("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"99%\"><tr>");
                    Stream.Add("<td width=\"5%\">&nbsp;</td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>10%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>20%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>30%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>40%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>50%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>60%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>70%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>80%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>90%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>100%</nobr></td>");
                    Stream.Add("<td width=\"4%\" align=\"center\">&nbsp;</td>");
                    Stream.Add("</tr></table>");
                    //
                    Stream.Add("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"99%\"><tr>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("</tr></table>");
                    //
                    // print the column headers
                    //
                    ColumnWidthTotal = 0;
                    if (IndexConfig.Columns.Count > 0) {
                        //
                        // Calc total width
                        //
                        foreach (var kvp in IndexConfig.Columns) {
                            indexConfigColumnClass column = kvp.Value;
                            ColumnWidthTotal += column.Width;
                        }
                        if (ColumnWidthTotal > 0) {
                            Stream.Add("<table border=\"0\" cellpadding=\"5\" cellspacing=\"0\" width=\"90%\">");
                            foreach (var kvp in IndexConfig.Columns) {
                                indexConfigColumnClass column = kvp.Value;
                                //
                                // print column headers - anchored so they sort columns
                                //
                                ColumnWidth = encodeInteger(100 * (column.Width / (double)ColumnWidthTotal));
                                cdefFieldModel field = adminContent.fields[column.Name.ToLower()];
                                fieldId = field.id;
                                Caption = field.caption;
                                if (field.inherited) {
                                    Caption = Caption + "*";
                                    InheritedFieldCount = InheritedFieldCount + 1;
                                }
                                AStart = "<a href=\"?" + core.doc.refreshQueryString + "&FieldName=" + genericController.encodeHTML(field.nameLc) + "&fi=" + fieldId + "&dtcn=" + ColumnPtr + "&" + RequestNameAdminSubForm + "=" + AdminFormIndex_SubFormSetColumns;
                                Stream.Add("<td width=\"" + ColumnWidth + "%\" valign=\"top\" align=\"left\">" + SpanClassAdminNormal + Caption + "<br>");
                                Stream.Add("<img src=\"/ccLib/images/black.GIF\" width=\"100%\" height=\"1\" >");
                                Stream.Add(AStart + "&dta=" + ToolsActionRemoveField + "\"><img src=\"/ccLib/images/LibButtonDeleteUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionMoveFieldRight + "\"><img src=\"/ccLib/images/LibButtonMoveRightUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionMoveFieldLeft + "\"><img src=\"/ccLib/images/LibButtonMoveLeftUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><br>");
                                //Call Stream.Add(AStart & "&dta=" & ToolsActionSetAZ & """><img src=""/ccLib/images/LibButtonSortazUp.gif"" width=""50"" height=""15"" border=""0"" ></A><br>")
                                //Call Stream.Add(AStart & "&dta=" & ToolsActionSetZA & """><img src=""/ccLib/images/LibButtonSortzaUp.gif"" width=""50"" height=""15"" border=""0"" ></A><br>")
                                Stream.Add(AStart + "&dta=" + ToolsActionExpand + "\"><img src=\"/ccLib/images/LibButtonOpenUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionContract + "\"><img src=\"/ccLib/images/LibButtonCloseUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A>");
                                Stream.Add("</span></td>");
                            }
                            Stream.Add("</tr>");
                            Stream.Add("</table>");
                        }
                    }
                    //
                    // ----- If anything was inherited, put up the message
                    //
                    if (InheritedFieldCount > 0) {
                        Stream.Add("<p class=\"ccNormal\">* This field was inherited from the Content Definition's Parent. Inherited fields will automatically change when the field in the parent is changed. If you alter these settings, this connection will be broken, and the field will no longer inherit it's properties.</P class=\"ccNormal\">");
                    }
                    //
                    // ----- now output a list of fields to add
                    //
                    if (CDef.fields.Count == 0) {
                        Stream.Add(SpanClassAdminNormal + "This Content Definition has no fields</span><br>");
                    } else {
                        Stream.Add(SpanClassAdminNormal + "<br>");
                        foreach (KeyValuePair<string, cdefFieldModel> keyValuePair in adminContent.fields) {
                            cdefFieldModel field = keyValuePair.Value;
                            //
                            // display the column if it is not in use
                            //
                            if (!IndexConfig.Columns.ContainsKey(field.nameLc)) {
                                if (field.fieldTypeId == FieldTypeIdFile) {
                                    //
                                    // file can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileText) {
                                    //
                                    // filename can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (text file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileHTML) {
                                    //
                                    // filename can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (html file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileCSS) {
                                    //
                                    // css filename can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (css file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileXML) {
                                    //
                                    // xml filename can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (xml file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileJavascript) {
                                    //
                                    // javascript filename can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (javascript file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdLongText) {
                                    //
                                    // long text can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (long text field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdHTML) {
                                    //
                                    // long text can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (long text field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileImage) {
                                    //
                                    // long text can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (image field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdRedirect) {
                                    //
                                    // long text can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (redirect field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdManyToMany) {
                                    //
                                    // many to many can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (many-to-many field)<br>");
                                } else {
                                    //
                                    // can be used as column header
                                    //
                                    Stream.Add("<a href=\"?" + core.doc.refreshQueryString + "&fi=" + field.id + "&dta=" + ToolsActionAddField + "&" + RequestNameAddFieldID + "=" + field.id + "&" + RequestNameAdminSubForm + "=" + AdminFormIndex_SubFormSetColumns + "\"><img src=\"/ccLib/images/LibButtonAddUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A> " + field.caption + "<br>");
                                }
                            }
                        }
                    }
                }
                //
                //--------------------------------------------------------------------------------
                // print the content tables that have index forms to Configure
                //--------------------------------------------------------------------------------
                //
                //FormPanel = FormPanel & SpanClassAdminNormal & "Select a Content Definition to Configure its index form<br>"
                //FormPanel = FormPanel & core.main_GetFormInputHidden("af", AdminFormToolConfigureIndex)
                //FormPanel = FormPanel & core.htmldoc.main_GetFormInputSelect2("ContentID", ContentID, "Content")
                //Call Stream.Add(core.htmldoc.main_GetPanel(FormPanel))
                //
                core.siteProperties.setProperty("AllowContentAutoLoad", genericController.encodeText(AllowContentAutoLoad));
                //Stream.Add( core.main_GetFormInputHidden("NeedToReloadConfig", NeedToReloadConfig))

                Content = ""
                    + Stream.Text + core.html.inputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormSetColumns) + "";
                tempGetForm_Index_SetColumns = Adminui.GetBody(Title, ButtonOK + "," + ButtonReset, "", false, false, Description, "", 10, Content);
                //
                //
                //    ButtonBar = AdminUI.GetButtonsFromList( ButtonList, True, True, "button")
                //    ButtonBar = AdminUI.GetButtonBar(ButtonBar, "")
                //    Stream = New FastStringClass
                //
                //    GetForm_Index_SetColumns = "" _
                //        & ButtonBar _
                //        & AdminUI.EditTableOpen _
                //        & Stream.Text _
                //        & AdminUI.EditTableClose _
                //        & ButtonBar _
                //    '
                //    '
                //    ' Assemble LiveWindowTable
                //    '
                //    Stream.Add( OpenLiveWindowTable)
                //    Stream.Add( vbCrLf & core.main_GetFormStart()
                //    Stream.Add( ButtonBar)
                //    Stream.Add( TitleBar)
                //    Stream.Add( Content)
                //    Stream.Add( ButtonBar)
                //    Stream.Add( "<input type=hidden name=asf VALUE=" & AdminFormIndex_SubFormSetColumns & ">")
                //    Stream.Add( "</form>")
                //    Stream.Add( CloseLiveWindowTable)
                //    '
                //    GetForm_Index_SetColumns = Stream.Text
                core.html.addTitle(Title);
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_Index_SetColumns;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private void TurnOnLinkAlias(bool UseContentWatchLink) {
            try {
                //
                int CS = 0;
                string ErrorList = null;
                string linkAlias = null;
                //
                if (core.doc.debug_iUserError != "") {
                    errorController.addUserError(core, "Existing pages could not be checked for Link Alias names because there was another error on this page. Correct this error, and turn Link Alias on again to rerun the verification.");
                } else {
                    CS = core.db.csOpen("Page Content");
                    while (core.db.csOk(CS)) {
                        //
                        // Add the link alias
                        //
                        linkAlias = core.db.csGetText(CS, "LinkAlias");
                        if (!string.IsNullOrEmpty(linkAlias)) {
                            //
                            // Add the link alias
                            //
                            linkAliasController.addLinkAlias(core, linkAlias, core.db.csGetInteger(CS, "ID"), "", false, true);
                        } else {
                            //
                            // Add the name
                            //
                            linkAlias = core.db.csGetText(CS, "name");
                            if (!string.IsNullOrEmpty(linkAlias)) {
                                linkAliasController.addLinkAlias(core, linkAlias, core.db.csGetInteger(CS, "ID"), "", false, false);
                            }
                        }
                        //
                        core.db.csGoNext(CS);
                    }
                    core.db.csClose(ref CS);
                    if (core.doc.debug_iUserError != "") {
                        //
                        // Throw out all the details of what happened, and add one simple error
                        //
                        ErrorList = errorController.getUserError(core);
                        ErrorList = genericController.vbReplace(ErrorList, UserErrorHeadline, "", 1, 99, 1);
                        errorController.addUserError(core, "The following errors occurred while verifying Link Alias entries for your existing pages." + ErrorList);
                        //Call core.htmldoc.main_AddUserError(ErrorList)
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        //   Editor features are stored in the \config\EditorFeatures.txt file
        //   This is a crlf delimited list, with each row including:
        //       admin:featurelist
        //       contentmanager:featurelist
        //       public:featurelist
        //========================================================================
        //
        private string GetForm_EditConfig() {
            string tempGetForm_EditConfig = null;
            try {
                //
                int CS = 0;
                string EditorStyleRulesFilename = null;
                bool AllowAdmin = false;
                bool AllowCM = false;
                bool AllowPublic = false;
                int RowPtr = 0;
                string AdminList = "";
                string CMList = "";
                string PublicList = "";
                string TDLeft = null;
                string TDCenter = null;
                int Ptr = 0;
                stringBuilderLegacyController Content = new stringBuilderLegacyController();
                string Button = null;
                string Copy = null;
                string ButtonList = null;
                adminUIController Adminui = new adminUIController(core);
                string Description = null;
                string[] DefaultFeatures = null;
                string FeatureName = null;
                string FeatureList = null;
                string[] Features = null;
                //
                DefaultFeatures = InnovaEditorFeatureList.Split(',');
                Description = "This tool is used to configure the wysiwyg content editor for different uses. Check the Administrator column if you want administrators to have access to this feature when editing a page. Check the Content Manager column to allow non-admins to have access to this feature. Check the Public column if you want those on the public site to have access to the feature when the editor is used for public forms.";
                //
                Button = core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    //
                    // Cancel button pressed, return with nothing goes to root form
                    //
                    //Call core.main_Redirect2(core.app.SiteProperty_AdminURL, "EditConfig, Cancel Button Pressed")
                } else {
                    //
                    // From here down will return a form
                    //
                    if (!core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                        //
                        // Does not have permission
                        //
                        ButtonList = ButtonCancel;
                        Content.Add(Adminui.GetFormBodyAdminOnly());
                        core.html.addTitle("Style Editor");
                        tempGetForm_EditConfig = Adminui.GetBody("Site Styles", ButtonList, "", true, true, Description, "", 0, Content.Text);
                    } else {
                        //
                        // OK to see and use this form
                        //
                        if (Button == ButtonSave || Button == ButtonOK) {
                            //
                            // Save the Previous edits
                            //
                            core.siteProperties.setProperty("Editor Background Color", core.docProperties.getText("editorbackgroundcolor"));
                            //
                            for (Ptr = 0; Ptr <= DefaultFeatures.GetUpperBound(0); Ptr++) {
                                FeatureName = DefaultFeatures[Ptr];
                                if (genericController.vbLCase(FeatureName) == "styleandformatting") {
                                    //
                                    // must always be on or it throws js error (editor bug I guess)
                                    //
                                    AdminList = AdminList + "," + FeatureName;
                                    CMList = CMList + "," + FeatureName;
                                    PublicList = PublicList + "," + FeatureName;
                                } else {
                                    if (core.docProperties.getBoolean(FeatureName + ".admin")) {
                                        AdminList = AdminList + "," + FeatureName;
                                    }
                                    if (core.docProperties.getBoolean(FeatureName + ".cm")) {
                                        CMList = CMList + "," + FeatureName;
                                    }
                                    if (core.docProperties.getBoolean(FeatureName + ".public")) {
                                        PublicList = PublicList + "," + FeatureName;
                                    }
                                }
                            }
                            core.privateFiles.saveFile(InnovaEditorFeaturefilename, "admin:" + AdminList + "\r\ncontentmanager:" + CMList + "\r\npublic:" + PublicList);
                            //
                            // Clear the editor style rules template cache so next edit gets new background color
                            //
                            EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", "0", 1, 99, 1);
                            core.privateFiles.deleteFile(EditorStyleRulesFilename);
                            //
                            CS = core.db.csOpenSql_rev("default", "select id from cctemplates");
                            while (core.db.csOk(CS)) {
                                EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", core.db.csGet(CS, "ID"), 1, 99, 1);
                                core.privateFiles.deleteFile(EditorStyleRulesFilename);
                                core.db.csGoNext(CS);
                            }
                            core.db.csClose(ref CS);

                        }
                        //
                        if (Button == ButtonOK) {
                            //
                            // exit with blank page
                            //
                        } else {
                            //
                            // Draw the form
                            //
                            FeatureList = core.cdnFiles.readFile(InnovaEditorFeaturefilename);
                            //If FeatureList = "" Then
                            //    FeatureList = core.cluster.localClusterFiles.readFile("ccLib\" & "Config\DefaultEditorConfig.txt")
                            //    Call core.privateFiles.saveFile(InnovaEditorFeaturefilename, FeatureList)
                            //End If
                            if (string.IsNullOrEmpty(FeatureList)) {
                                FeatureList = "admin:" + InnovaEditorFeatureList + "\r\ncontentmanager:" + InnovaEditorFeatureList + "\r\npublic:" + InnovaEditorPublicFeatureList;
                            }
                            if (!string.IsNullOrEmpty(FeatureList)) {
                                Features = stringSplit(FeatureList, "\r\n");
                                AdminList = Features[0].Replace("admin:", "");
                                if (Features.GetUpperBound(0) > 0) {
                                    CMList = Features[1].Replace("contentmanager:", "");
                                    if (Features.GetUpperBound(0) > 1) {
                                        PublicList = Features[2].Replace("public:", "");
                                    }
                                }
                            }
                            //
                            Copy = "\r\n<tr class=\"ccAdminListCaption\">"
                                + "<td align=left style=\"width:200;\">Feature</td>"
                                + "<td align=center style=\"width:100;\">Administrators</td>"
                                + "<td align=center style=\"width:100;\">Content&nbsp;Managers</td>"
                                + "<td align=center style=\"width:100;\">Public</td>"
                                + "</tr>";
                            RowPtr = 0;
                            for (Ptr = 0; Ptr <= DefaultFeatures.GetUpperBound(0); Ptr++) {
                                FeatureName = DefaultFeatures[Ptr];
                                if (genericController.vbLCase(FeatureName) == "styleandformatting") {
                                    //
                                    // hide and force on during process - editor bug I think.
                                    //
                                } else {
                                    TDLeft = genericController.StartTableCell("", 0, encodeBoolean(RowPtr % 2), "left");
                                    TDCenter = genericController.StartTableCell("", 0, encodeBoolean(RowPtr % 2), "center");
                                    AllowAdmin = genericController.encodeBoolean("," + AdminList + ",".IndexOf("," + FeatureName + ",", System.StringComparison.OrdinalIgnoreCase) + 1);
                                    AllowCM = genericController.encodeBoolean("," + CMList + ",".IndexOf("," + FeatureName + ",", System.StringComparison.OrdinalIgnoreCase) + 1);
                                    AllowPublic = genericController.encodeBoolean("," + PublicList + ",".IndexOf("," + FeatureName + ",", System.StringComparison.OrdinalIgnoreCase) + 1);
                                    Copy += "\r\n<tr>"
                                        + TDLeft + FeatureName + "</td>"
                                        + TDCenter + core.html.inputCheckbox(FeatureName + ".admin", AllowAdmin) + "</td>"
                                        + TDCenter + core.html.inputCheckbox(FeatureName + ".cm", AllowCM) + "</td>"
                                        + TDCenter + core.html.inputCheckbox(FeatureName + ".public", AllowPublic) + "</td>"
                                        + "</tr>";
                                    RowPtr = RowPtr + 1;
                                }
                            }
                            Copy = ""
                                + "\r\n<div><b>body background style color</b> (default='white')</div>"
                                + "\r\n<div>" + core.html.inputText("editorbackgroundcolor", core.siteProperties.getText("Editor Background Color", "white")) + "</div>"
                                + "\r\n<div>&nbsp;</div>"
                                + "\r\n<div><b>Toolbar features available</b></div>"
                                + "\r\n<table border=\"0\" cellpadding=\"4\" cellspacing=\"0\" width=\"500px\" align=left>" + genericController.htmlIndent(Copy) + "\r\n" + kmaEndTable;
                            Copy = "\r\n" + genericController.StartTable(20, 0, 0) + "<tr><td>" + genericController.htmlIndent(Copy) + "</td></tr>\r\n" + kmaEndTable;
                            Content.Add(Copy);
                            ButtonList = ButtonCancel + "," + ButtonRefresh + "," + ButtonSave + "," + ButtonOK;
                            Content.Add(core.html.inputHidden(rnAdminSourceForm, AdminFormEditorConfig));
                            core.html.addTitle("Editor Settings");
                            tempGetForm_EditConfig = Adminui.GetBody("Editor Configuration", ButtonList, "", true, true, Description, "", 0, Content.Text);
                        }
                    }
                    //
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_EditConfig;
        }
        //
        //========================================================================
        // Page Content Settings Page
        //========================================================================
        //
        private string GetForm_BuildCollection() {
            string tempGetForm_BuildCollection = null;
            try {
                //
                string Description = null;
                stringBuilderLegacyController Content = new stringBuilderLegacyController();
                string Button = null;
                adminUIController Adminui = new adminUIController(core);
                string ButtonList = null;
                bool AllowAutoLogin = false;
                string Copy = null;
                //
                Button = core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    //
                    // Cancel just exits with no content
                    //
                    return tempGetForm_BuildCollection;
                } else if (!core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                    //
                    // Not Admin Error
                    //
                    ButtonList = ButtonCancel;
                    Content.Add(Adminui.GetFormBodyAdminOnly());
                } else {
                    Content.Add(Adminui.EditTableOpen);
                    //
                    // Set defaults
                    //
                    AllowAutoLogin = (core.siteProperties.getBoolean("AllowAutoLogin", true));
                    //
                    // Process Requests
                    //
                    switch (Button) {
                        case ButtonSave:
                        case ButtonOK:
                            //
                            //
                            //
                            AllowAutoLogin = core.docProperties.getBoolean("AllowAutoLogin");
                            //
                            core.siteProperties.setProperty("AllowAutoLogin", genericController.encodeText(AllowAutoLogin));
                            break;
                    }
                    if (Button == ButtonOK) {
                        //
                        // Exit on OK or cancel
                        //
                        return tempGetForm_BuildCollection;
                    }
                    //
                    // List Add-ons to include
                    //

                    Copy = core.html.inputCheckbox("AllowAutoLogin", AllowAutoLogin);
                    Copy += "<div>When checked, returning users are automatically logged-in, without requiring a username or password. This is very convenient, but creates a high security risk. Each time you login, you will be given the option to not allow Auto-Login from that computer.</div>";
                    Content.Add(Adminui.GetEditRow(Copy, "Allow Auto Login", "", false, false, ""));
                    //
                    // Buttons
                    //
                    ButtonList = ButtonCancel + "," + ButtonSave + "," + ButtonOK;
                    //
                    // Close Tables
                    //
                    Content.Add(Adminui.EditTableClose);
                    Content.Add(core.html.inputHidden(rnAdminSourceForm, AdminFormBuilderCollection));
                }
                //
                Description = "Use this tool to modify the site security settings";
                tempGetForm_BuildCollection = Adminui.GetBody("Security Settings", ButtonList, "", true, true, Description, "", 0, Content.Text);
                Content = null;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetForm_BuildCollection;
        }
        //
        //=================================================================================
        //   Load the index configig
        //       if it is empty, setup defaults
        //=================================================================================
        //
        private indexConfigClass LoadIndexConfig(cdefModel adminContent) {
            indexConfigClass returnIndexConfig = new indexConfigClass();
            try {
                //
                string ConfigListLine = null;
                string Line = null;
                int Ptr = 0;
                string ConfigList = null;
                string[] ConfigListLines = null;
                string[] LineSplit = null;
                //
                //
                // Setup defaults
                //
                returnIndexConfig.ContentID = adminContent.Id;
                returnIndexConfig.ActiveOnly = false;
                returnIndexConfig.LastEditedByMe = false;
                returnIndexConfig.LastEditedToday = false;
                returnIndexConfig.LastEditedPast7Days = false;
                returnIndexConfig.LastEditedPast30Days = false;
                returnIndexConfig.Loaded = true;
                returnIndexConfig.Open = false;
                returnIndexConfig.PageNumber = 1;
                returnIndexConfig.RecordsPerPage = RecordsPerPageDefault;
                returnIndexConfig.RecordTop = 0;
                //
                // Setup Member Properties
                //
                ConfigList = core.userProperty.getText(IndexConfigPrefix + encodeText(adminContent.Id), "");
                if (!string.IsNullOrEmpty(ConfigList)) {
                    //
                    // load values
                    //
                    ConfigList = ConfigList + "\r\n";
                    ConfigListLines = genericController.SplitCRLF(ConfigList);
                    Ptr = 0;
                    while (Ptr < ConfigListLines.GetUpperBound(0)) {
                        //
                        // check next line
                        //
                        ConfigListLine = genericController.vbLCase(ConfigListLines[Ptr]);
                        if (!string.IsNullOrEmpty(ConfigListLine)) {
                            switch (ConfigListLine) {
                                case "columns":
                                    Ptr = Ptr + 1;
                                    while (!string.IsNullOrEmpty(ConfigListLines[Ptr])) {
                                        Line = ConfigListLines[Ptr];
                                        LineSplit = Line.Split('\t');
                                        if (LineSplit.GetUpperBound(0) > 0) {
                                            indexConfigColumnClass column = new indexConfigColumnClass();
                                            column.Name = LineSplit[0].Trim();
                                            column.Width = genericController.encodeInteger(LineSplit[1]);
                                            returnIndexConfig.Columns.Add(column.Name.ToLower(), column);
                                        }
                                        Ptr = Ptr + 1;
                                    }
                                    break;
                                case "sorts":
                                    Ptr = Ptr + 1;
                                    while (!string.IsNullOrEmpty(ConfigListLines[Ptr])) {
                                        //ReDim Preserve .Sorts(.SortCnt)
                                        Line = ConfigListLines[Ptr];
                                        LineSplit = Line.Split('\t');
                                        if (LineSplit.GetUpperBound(0) == 1) {
                                            returnIndexConfig.Sorts.Add(LineSplit[0].ToLower(), new indexConfigSortClass {
                                                fieldName = LineSplit[0],
                                                direction = (genericController.encodeBoolean(LineSplit[1]) ? 1 : 2)
                                            });
                                        }
                                        Ptr = Ptr + 1;
                                    }
                                    break;
                            }
                        }
                        Ptr = Ptr + 1;
                    }
                    if (returnIndexConfig.RecordsPerPage <= 0) {
                        returnIndexConfig.RecordsPerPage = RecordsPerPageDefault;
                    }
                    //.PageNumber = 1 + Int(.RecordTop / .RecordsPerPage)
                }
                //
                // Setup Visit Properties
                //
                ConfigList = core.visitProperty.getText(IndexConfigPrefix + encodeText(adminContent.Id), "");
                if (!string.IsNullOrEmpty(ConfigList)) {
                    //
                    // load values
                    //
                    ConfigList = ConfigList + "\r\n";
                    ConfigListLines = genericController.SplitCRLF(ConfigList);
                    Ptr = 0;
                    while (Ptr < ConfigListLines.GetUpperBound(0)) {
                        //
                        // check next line
                        //
                        ConfigListLine = genericController.vbLCase(ConfigListLines[Ptr]);
                        if (!string.IsNullOrEmpty(ConfigListLine)) {
                            switch (ConfigListLine) {
                                case "findwordlist":
                                    Ptr = Ptr + 1;
                                    while (!string.IsNullOrEmpty(ConfigListLines[Ptr])) {
                                        //ReDim Preserve .FindWords(.FindWords.Count)
                                        Line = ConfigListLines[Ptr];
                                        LineSplit = Line.Split('\t');
                                        if (LineSplit.GetUpperBound(0) > 1) {
                                            indexConfigFindWordClass findWord = new indexConfigFindWordClass();
                                            findWord.Name = LineSplit[0];
                                            findWord.Value = LineSplit[1];
                                            findWord.MatchOption = (FindWordMatchEnum)genericController.encodeInteger(LineSplit[2]);
                                            returnIndexConfig.FindWords.Add(findWord.Name, findWord);
                                        }
                                        Ptr = Ptr + 1;
                                    }
                                    break;
                                case "grouplist":
                                    Ptr = Ptr + 1;
                                    while (!string.IsNullOrEmpty(ConfigListLines[Ptr])) {
                                        Array.Resize(ref returnIndexConfig.GroupList, returnIndexConfig.GroupListCnt + 1);
                                        returnIndexConfig.GroupList[returnIndexConfig.GroupListCnt] = ConfigListLines[Ptr];
                                        returnIndexConfig.GroupListCnt = returnIndexConfig.GroupListCnt + 1;
                                        Ptr = Ptr + 1;
                                    }
                                    break;
                                case "cdeflist":
                                    Ptr = Ptr + 1;
                                    returnIndexConfig.SubCDefID = genericController.encodeInteger(ConfigListLines[Ptr]);
                                    break;
                                case "indexfiltercategoryid":
                                    // -- remove deprecated value
                                    Ptr = Ptr + 1;
                                    int ignore = genericController.encodeInteger(ConfigListLines[Ptr]);
                                    break;
                                case "indexfilteractiveonly":
                                    returnIndexConfig.ActiveOnly = true;
                                    break;
                                case "indexfilterlasteditedbyme":
                                    returnIndexConfig.LastEditedByMe = true;
                                    break;
                                case "indexfilterlasteditedtoday":
                                    returnIndexConfig.LastEditedToday = true;
                                    break;
                                case "indexfilterlasteditedpast7days":
                                    returnIndexConfig.LastEditedPast7Days = true;
                                    break;
                                case "indexfilterlasteditedpast30days":
                                    returnIndexConfig.LastEditedPast30Days = true;
                                    break;
                                case "indexfilteropen":
                                    returnIndexConfig.Open = true;
                                    break;
                                case "recordsperpage":
                                    Ptr = Ptr + 1;
                                    returnIndexConfig.RecordsPerPage = genericController.encodeInteger(ConfigListLines[Ptr]);
                                    if (returnIndexConfig.RecordsPerPage <= 0) {
                                        returnIndexConfig.RecordsPerPage = 50;
                                    }
                                    returnIndexConfig.RecordTop = ((returnIndexConfig.PageNumber - 1) * returnIndexConfig.RecordsPerPage);
                                    break;
                                case "pagenumber":
                                    Ptr = Ptr + 1;
                                    returnIndexConfig.PageNumber = genericController.encodeInteger(ConfigListLines[Ptr]);
                                    if (returnIndexConfig.PageNumber <= 0) {
                                        returnIndexConfig.PageNumber = 1;
                                    }
                                    returnIndexConfig.RecordTop = ((returnIndexConfig.PageNumber - 1) * returnIndexConfig.RecordsPerPage);
                                    break;
                            }
                        }
                        Ptr = Ptr + 1;
                    }
                    if (returnIndexConfig.RecordsPerPage <= 0) {
                        returnIndexConfig.RecordsPerPage = RecordsPerPageDefault;
                    }
                    //.PageNumber = 1 + Int(.RecordTop / .RecordsPerPage)
                }
                //
                // Setup defaults if not loaded
                //
                if ((returnIndexConfig.Columns.Count == 0) && (adminContent.adminColumns.Count > 0)) {
                    //.Columns.Count = adminContent.adminColumns.Count
                    //ReDim .Columns(.Columns.Count - 1)
                    //Ptr = 0
                    foreach (var keyValuepair in adminContent.adminColumns) {
                        cdefModel.CDefAdminColumnClass cdefAdminColumn = keyValuepair.Value;
                        indexConfigColumnClass column = new indexConfigColumnClass();
                        column.Name = cdefAdminColumn.Name;
                        column.Width = cdefAdminColumn.Width;
                        returnIndexConfig.Columns.Add(column.Name.ToLower(), column);
                    }
                }
                //
                // Set field pointers for columns and sorts
                //
                // dont knwo what this does
                //For Each keyValuePair As KeyValuePair(Of String, appServices_metaDataClass.CDefFieldClass) In adminContent.fields
                //    Dim field As appServices_metaDataClass.CDefFieldClass = keyValuePair.Value
                //    If .Columns.Count > 0 Then
                //        For Ptr = 0 To .Columns.Count - 1
                //            With .Columns[Ptr]
                //                If genericController.vbLCase(.Name) = field.Name.ToLower() Then
                //                    .FieldId = SrcPtr
                //                    Exit For
                //                End If
                //            End With
                //        Next
                //    End If
                //    '
                //    If .SortCnt > 0 Then
                //        For Ptr = 0 To .SortCnt - 1
                //            With .Sorts[Ptr]
                //                If genericController.vbLCase(.FieldName) = field.Name Then
                //                    .FieldPtr = SrcPtr
                //                    Exit For
                //                End If
                //            End With
                //        Next
                //    End If
                //Next
                //        '
                //        ' set Column Field Ptr for later
                //        '
                //        If .columns.count > 0 Then
                //            For Ptr = 0 To .columns.count - 1
                //                With .Columns[Ptr]
                //                    For SrcPtr = 0 To AdminContent.fields.count - 1
                //                        If .Name = AdminContent.fields[SrcPtr].Name Then
                //                            .FieldPointer = SrcPtr
                //                            Exit For
                //                        End If
                //                    Next
                //                End With
                //            Next
                //        End If
                //        '
                //        ' set Sort Field Ptr for later
                //        '
                //        If .SortCnt > 0 Then
                //            For Ptr = 0 To .SortCnt - 1
                //                With .Sorts[Ptr]
                //                    For SrcPtr = 0 To AdminContent.fields.count - 1
                //                        If genericController.vbLCase(.FieldName) = genericController.vbLCase(AdminContent.fields[SrcPtr].Name) Then
                //                            .FieldPtr = SrcPtr
                //                            Exit For
                //                        End If
                //                    Next
                //                End With
                //            Next
                //        End If
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnIndexConfig;
        }
        //
        //========================================================================================
        //   Process request input on the IndexConfig
        //========================================================================================
        //
        private void SetIndexSQL_ProcessIndexConfigRequests(cdefModel adminContent, editRecordClass editRecord, ref indexConfigClass IndexConfig) {
            try {
                //
                int TestInteger = 0;
                string VarText = null;
                string FindName = null;
                string FindValue = null;
                int Ptr = 0;
                int ColumnCnt = 0;
                int ColumnPtr = 0;
                string Button = null;
                if (!IndexConfig.Loaded) {
                    IndexConfig = LoadIndexConfig(adminContent);
                }
                //
                // ----- Page number
                //
                VarText = core.docProperties.getText("rt");
                if (!string.IsNullOrEmpty(VarText)) {
                    IndexConfig.RecordTop = genericController.encodeInteger(VarText);
                }
                //
                VarText = core.docProperties.getText("RS");
                if (!string.IsNullOrEmpty(VarText)) {
                    IndexConfig.RecordsPerPage = genericController.encodeInteger(VarText);
                }
                if (IndexConfig.RecordsPerPage <= 0) {
                    IndexConfig.RecordsPerPage = RecordsPerPageDefault;
                }
                IndexConfig.PageNumber = encodeInteger(1 + Math.Floor(IndexConfig.RecordTop / (double)IndexConfig.RecordsPerPage));
                //
                // ----- Process indexGoToPage value
                //
                TestInteger = core.docProperties.getInteger("indexGoToPage");
                if (TestInteger > 0) {
                    IndexConfig.PageNumber = TestInteger;
                    IndexConfig.RecordTop = ((IndexConfig.PageNumber - 1) * IndexConfig.RecordsPerPage);
                } else {
                    //
                    // ----- Read filter changes and First/Next/Previous from form
                    //
                    Button = core.docProperties.getText(RequestNameButton);
                    if (!string.IsNullOrEmpty(Button)) {
                        switch (AdminButton) {
                            case ButtonFirst:
                                //
                                // Force to first page
                                //
                                IndexConfig.PageNumber = 1;
                                IndexConfig.RecordTop = ((IndexConfig.PageNumber - 1) * IndexConfig.RecordsPerPage);
                                break;
                            case ButtonNext:
                                //
                                // Go to next page
                                //
                                IndexConfig.PageNumber = IndexConfig.PageNumber + 1;
                                IndexConfig.RecordTop = ((IndexConfig.PageNumber - 1) * IndexConfig.RecordsPerPage);
                                break;
                            case ButtonPrevious:
                                //
                                // Go to previous page
                                //
                                IndexConfig.PageNumber = IndexConfig.PageNumber - 1;
                                if (IndexConfig.PageNumber <= 0) {
                                    IndexConfig.PageNumber = 1;
                                }
                                IndexConfig.RecordTop = ((IndexConfig.PageNumber - 1) * IndexConfig.RecordsPerPage);
                                break;
                            case ButtonFind:
                                //
                                // Find (change search criteria and go to first page)
                                //
                                IndexConfig.PageNumber = 1;
                                IndexConfig.RecordTop = ((IndexConfig.PageNumber - 1) * IndexConfig.RecordsPerPage);
                                ColumnCnt = core.docProperties.getInteger("ColumnCnt");
                                if (ColumnCnt > 0) {
                                    for (ColumnPtr = 0; ColumnPtr < ColumnCnt; ColumnPtr++) {
                                        FindName = core.docProperties.getText("FindName" + ColumnPtr).ToLower();
                                        if (!string.IsNullOrEmpty(FindName)) {
                                            if (adminContent.fields.ContainsKey(FindName.ToLower())) {
                                                FindValue = encodeText(core.docProperties.getText("FindValue" + ColumnPtr)).Trim(' ');
                                                if (string.IsNullOrEmpty(FindValue)) {
                                                    //
                                                    // -- find blank, if name in list, remove it
                                                    if (IndexConfig.FindWords.ContainsKey(FindName)) {
                                                        IndexConfig.FindWords.Remove(FindName);
                                                    }
                                                } else {
                                                    //
                                                    // -- nonblank find, store it
                                                    if (IndexConfig.FindWords.ContainsKey(FindName)) {
                                                        IndexConfig.FindWords[FindName].Value = FindValue;
                                                    } else {
                                                        cdefFieldModel field = adminContent.fields[FindName.ToLower()];
                                                        indexConfigFindWordClass findWord = new indexConfigFindWordClass();
                                                        findWord.Name = FindName;
                                                        findWord.Value = FindValue;
                                                        switch (field.fieldTypeId) {
                                                            case FieldTypeIdAutoIdIncrement:
                                                            case FieldTypeIdCurrency:
                                                            case FieldTypeIdFloat:
                                                            case FieldTypeIdInteger:
                                                            case FieldTypeIdLookup:
                                                            case FieldTypeIdMemberSelect:
                                                                findWord.MatchOption = FindWordMatchEnum.MatchEquals;
                                                                break;
                                                            case FieldTypeIdDate:
                                                                findWord.MatchOption = FindWordMatchEnum.MatchEquals;
                                                                break;
                                                            case FieldTypeIdBoolean:
                                                                findWord.MatchOption = FindWordMatchEnum.MatchEquals;
                                                                break;
                                                            default:
                                                                findWord.MatchOption = FindWordMatchEnum.matchincludes;
                                                                break;
                                                        }
                                                        IndexConfig.FindWords.Add(FindName, findWord);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
                //
                // Process Filter form
                //
                if (core.docProperties.getBoolean("IndexFilterRemoveAll")) {
                    //
                    // Remove all filters
                    //
                    IndexConfig.FindWords = new Dictionary<string, indexConfigFindWordClass>();
                    IndexConfig.GroupListCnt = 0;
                    IndexConfig.SubCDefID = 0;
                    IndexConfig.ActiveOnly = false;
                    IndexConfig.LastEditedByMe = false;
                    IndexConfig.LastEditedToday = false;
                    IndexConfig.LastEditedPast7Days = false;
                    IndexConfig.LastEditedPast30Days = false;
                } else {
                    int VarInteger;
                    //
                    // Add CDef
                    //
                    VarInteger = core.docProperties.getInteger("IndexFilterAddCDef");
                    if (VarInteger != 0) {
                        IndexConfig.SubCDefID = VarInteger;
                        IndexConfig.PageNumber = 1;
                        //                If .SubCDefCnt > 0 Then
                        //                    For Ptr = 0 To .SubCDefCnt - 1
                        //                        If VarInteger = .SubCDefs[Ptr] Then
                        //                            Exit For
                        //                        End If
                        //                    Next
                        //                End If
                        //                If Ptr = .SubCDefCnt Then
                        //                    ReDim Preserve .SubCDefs(.SubCDefCnt)
                        //                    .SubCDefs(.SubCDefCnt) = VarInteger
                        //                    .SubCDefCnt = .SubCDefCnt + 1
                        //                    .PageNumber = 1
                        //                End If
                    }
                    //
                    // Remove CDef
                    //
                    VarInteger = core.docProperties.getInteger("IndexFilterRemoveCDef");
                    if (VarInteger != 0) {
                        IndexConfig.SubCDefID = 0;
                        IndexConfig.PageNumber = 1;
                        //                If .SubCDefCnt > 0 Then
                        //                    For Ptr = 0 To .SubCDefCnt - 1
                        //                        If .SubCDefs[Ptr] = VarInteger Then
                        //                            .SubCDefs[Ptr] = 0
                        //                            .PageNumber = 1
                        //                            Exit For
                        //                        End If
                        //                    Next
                        //                End If
                    }
                    //
                    // Add Groups
                    //
                    VarText = core.docProperties.getText("IndexFilterAddGroup").ToLower();
                    if (!string.IsNullOrEmpty(VarText)) {
                        if (IndexConfig.GroupListCnt > 0) {
                            for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++) {
                                if (VarText == IndexConfig.GroupList[Ptr]) {
                                    break;
                                }
                            }
                        }
                        if (Ptr == IndexConfig.GroupListCnt) {
                            Array.Resize(ref IndexConfig.GroupList, IndexConfig.GroupListCnt + 1);
                            IndexConfig.GroupList[IndexConfig.GroupListCnt] = VarText;
                            IndexConfig.GroupListCnt = IndexConfig.GroupListCnt + 1;
                            IndexConfig.PageNumber = 1;
                        }
                    }
                    //
                    // Remove Groups
                    //
                    VarText = core.docProperties.getText("IndexFilterRemoveGroup").ToLower();
                    if (!string.IsNullOrEmpty(VarText)) {
                        if (IndexConfig.GroupListCnt > 0) {
                            for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++) {
                                if (IndexConfig.GroupList[Ptr] == VarText) {
                                    IndexConfig.GroupList[Ptr] = "";
                                    IndexConfig.PageNumber = 1;
                                    break;
                                }
                            }
                        }
                    }
                    //
                    // Remove FindWords
                    //
                    VarText = core.docProperties.getText("IndexFilterRemoveFind").ToLower();
                    if (!string.IsNullOrEmpty(VarText)) {
                        if (IndexConfig.FindWords.ContainsKey(VarText)) {
                            IndexConfig.FindWords.Remove(VarText);
                        }
                        //If .FindWords.Count > 0 Then
                        //    For Ptr = 0 To .FindWords.Count - 1
                        //        If .FindWords[Ptr].Name = VarText Then
                        //            .FindWords[Ptr].MatchOption = FindWordMatchEnum.MatchIgnore
                        //            .FindWords[Ptr].Value = ""
                        //            .PageNumber = 1
                        //            Exit For
                        //        End If
                        //    Next
                        //End If
                    }
                    //
                    // Read ActiveOnly
                    //
                    VarText = core.docProperties.getText("IndexFilterActiveOnly");
                    if (!string.IsNullOrEmpty(VarText)) {
                        IndexConfig.ActiveOnly = genericController.encodeBoolean(VarText);
                        IndexConfig.PageNumber = 1;
                    }
                    //
                    // Read LastEditedByMe
                    //
                    VarText = core.docProperties.getText("IndexFilterLastEditedByMe");
                    if (!string.IsNullOrEmpty(VarText)) {
                        IndexConfig.LastEditedByMe = genericController.encodeBoolean(VarText);
                        IndexConfig.PageNumber = 1;
                    }
                    //
                    // Last Edited Past 30 Days
                    //
                    VarText = core.docProperties.getText("IndexFilterLastEditedPast30Days");
                    if (!string.IsNullOrEmpty(VarText)) {
                        IndexConfig.LastEditedPast30Days = genericController.encodeBoolean(VarText);
                        IndexConfig.LastEditedPast7Days = false;
                        IndexConfig.LastEditedToday = false;
                        IndexConfig.PageNumber = 1;
                    } else {
                        //
                        // Past 7 Days
                        //
                        VarText = core.docProperties.getText("IndexFilterLastEditedPast7Days");
                        if (!string.IsNullOrEmpty(VarText)) {
                            IndexConfig.LastEditedPast30Days = false;
                            IndexConfig.LastEditedPast7Days = genericController.encodeBoolean(VarText);
                            IndexConfig.LastEditedToday = false;
                            IndexConfig.PageNumber = 1;
                        } else {
                            //
                            // Read LastEditedToday
                            //
                            VarText = core.docProperties.getText("IndexFilterLastEditedToday");
                            if (!string.IsNullOrEmpty(VarText)) {
                                IndexConfig.LastEditedPast30Days = false;
                                IndexConfig.LastEditedPast7Days = false;
                                IndexConfig.LastEditedToday = genericController.encodeBoolean(VarText);
                                IndexConfig.PageNumber = 1;
                            }
                        }
                    }
                    //
                    // Read IndexFilterOpen
                    //
                    VarText = core.docProperties.getText("IndexFilterOpen");
                    if (!string.IsNullOrEmpty(VarText)) {
                        IndexConfig.Open = genericController.encodeBoolean(VarText);
                        IndexConfig.PageNumber = 1;
                    }
                    //
                    // SortField
                    //
                    VarText = core.docProperties.getText("SetSortField").ToLower();
                    if (!string.IsNullOrEmpty(VarText)) {
                        if (IndexConfig.Sorts.ContainsKey(VarText)) {
                            IndexConfig.Sorts.Remove(VarText);
                        }
                        int sortDirection = core.docProperties.getInteger("SetSortDirection");
                        if (sortDirection > 0) {
                            IndexConfig.Sorts.Add(VarText, new indexConfigSortClass {
                                fieldName = VarText,
                                direction = sortDirection
                            });
                        }
                    }
                    //
                    // Build FindWordList
                    //
                    //.FindWordList = ""
                    //If .findwords.count > 0 Then
                    //    For Ptr = 0 To .findwords.count - 1
                    //        If .FindWords[Ptr].Value <> "" Then
                    //            .FindWordList = .FindWordList & vbCrLf & .FindWords[Ptr].Name & "=" & .FindWords[Ptr].Value
                    //        End If
                    //    Next
                    //End If
                }
                //            Criteria = "(active<>0)and(ContentID=" & core.main_GetContentID("people") & ")and(authorable<>0)"
                //            CS = core.app.csOpen("Content Fields", Criteria, "EditSortPriority")
                //            Do While core.app.csv_IsCSOK(CS)
                //                FieldName = core.db.cs_getText(CS, "name")
                //                FieldValue = core.main_GetStreamText2(FieldName)
                //                FieldType = core.app.cs_getInteger(CS, "Type")
                //                Select Case FieldType
                //                    Case FieldTypeCurrency, FieldTypeFloat, FieldTypeInteger
                //                        NumericOption = core.main_GetStreamText2(FieldName & "_N")
                //                        If NumericOption <> "" Then
                //                            '.FindWords(0).MatchOption = 1
                //                            ContactSearchCriteria = ContactSearchCriteria _
                //                                & vbCrLf _
                //                                & FieldName & vbTab _
                //                                & FieldType & vbTab _
                //                                & FieldValue & vbTab _
                //                                & NumericOption
                //                        End If
                //                    Case FieldTypeBoolean
                //                        If FieldValue <> "" Then
                //                            ContactSearchCriteria = ContactSearchCriteria _
                //                                & vbCrLf _
                //                                & FieldName & vbTab _
                //                                & FieldType & vbTab _
                //                                & FieldValue & vbTab _
                //                                & ""
                //                        End If
                //                    Case FieldTypeText
                //                        TextOption = core.main_GetStreamText2(FieldName & "_T")
                //                        If TextOption <> "" Then
                //                            ContactSearchCriteria = ContactSearchCriteria _
                //                                & vbCrLf _
                //                                & FieldName & vbTab _
                //                                & CStr(FieldType) & vbTab _
                //                                & FieldValue & vbTab _
                //                                & TextOption
                //                        End If
                //                    Case FieldTypeLookup
                //                        If FieldValue <> "" Then
                //                            ContactSearchCriteria = ContactSearchCriteria _
                //                                & vbCrLf _
                //                                & FieldName & vbTab _
                //                                & FieldType & vbTab _
                //                                & FieldValue & vbTab _
                //                                & ""
                //                        End If
                //                End Select
                //                Call core.app.nextCSRecord(CS)
                //            Loop
                //            Call core.app.closeCS(CS)
                //            Call core.main_SetMemberProperty("ContactSearchCriteria", ContactSearchCriteria)
                //        End If


                //
                // Set field pointers for columns and sorts
                //
                //Dim SrcPtr As Integer
                //If .Columns.Count > 0 Or .SortCnt > 0 Then
                //    For Each keyValuePair As KeyValuePair(Of String, appServices_metaDataClass.CDefFieldClass) In adminContent.fields
                //        Dim field As appServices_metaDataClass.CDefFieldClass = keyValuePair.Value
                //        If .Columns.Count > 0 Then
                //            For Ptr = 0 To .Columns.Count - 1
                //                With .Columns[Ptr]
                //                    If genericController.vbLCase(.Name) = field.Name Then
                //                        .FieldId = SrcPtr
                //                        Exit For
                //                    End If
                //                End With
                //            Next
                //        End If
                //        '
                //        If .SortCnt > 0 Then
                //            For Ptr = 0 To .SortCnt - 1
                //                With .Sorts[Ptr]
                //                    If genericController.vbLCase(.FieldName) = field.Name Then
                //                        .FieldPtr = SrcPtr
                //                        Exit For
                //                    End If
                //                End With
                //            Next
                //        End If
                //    Next
                //End If
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //=================================================================================
        //
        //=================================================================================
        //
        private void SetIndexSQL_SaveIndexConfig(indexConfigClass IndexConfig) {
            //
            string FilterText = null;
            string SubList = null;
            int Ptr = 0;
            //
            // ----- Save filter state to the visit property
            //
            //
            // -----------------------------------------------------------------------------------------------
            //   Visit Properties (non-persistant)
            // -----------------------------------------------------------------------------------------------
            //
            FilterText = "";
            //
            // Find words
            //
            SubList = "";
            foreach (var kvp in IndexConfig.FindWords) {
                indexConfigFindWordClass findWord = kvp.Value;
                if ((!string.IsNullOrEmpty(findWord.Name)) & (findWord.MatchOption != FindWordMatchEnum.MatchIgnore)) {
                    SubList = SubList + "\r\n" + findWord.Name + "\t" + findWord.Value + "\t" + (int)findWord.MatchOption;
                }
            }
            if (!string.IsNullOrEmpty(SubList)) {
                FilterText = FilterText + "\r\nFindWordList" + SubList + "\r\n";
            }
            //
            // CDef List
            //
            if (IndexConfig.SubCDefID > 0) {
                FilterText = FilterText + "\r\nCDefList\r\n" + IndexConfig.SubCDefID + "\r\n";
            }
            //        SubList = ""
            //        If .SubCDefCnt > 0 Then
            //            For Ptr = 0 To .SubCDefCnt - 1
            //                If .SubCDefs[Ptr] <> 0 Then
            //                    SubList = SubList & vbCrLf & .SubCDefs[Ptr]
            //                End If
            //            Next
            //        End If
            //        If SubList <> "" Then
            //            FilterText = FilterText & vbCrLf & "CDefList" & SubList & vbCrLf
            //        End If
            //
            // Group List
            //
            SubList = "";
            if (IndexConfig.GroupListCnt > 0) {
                for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++) {
                    if (!string.IsNullOrEmpty(IndexConfig.GroupList[Ptr])) {
                        SubList = SubList + "\r\n" + IndexConfig.GroupList[Ptr];
                    }
                }
            }
            if (!string.IsNullOrEmpty(SubList)) {
                FilterText = FilterText + "\r\nGroupList" + SubList + "\r\n";
            }
            //
            // PageNumber and Records Per Page
            //
            FilterText = FilterText + "\r\n"
                + "\r\npagenumber"
                + "\r\n" + IndexConfig.PageNumber;
            FilterText = FilterText + "\r\n"
                + "\r\nrecordsperpage"
                + "\r\n" + IndexConfig.RecordsPerPage;
            //
            // misc filters
            //
            if (IndexConfig.ActiveOnly) {
                FilterText = FilterText + "\r\n"
                    + "\r\nIndexFilterActiveOnly";
            }
            if (IndexConfig.LastEditedByMe) {
                FilterText = FilterText + "\r\n"
                    + "\r\nIndexFilterLastEditedByMe";
            }
            if (IndexConfig.LastEditedToday) {
                FilterText = FilterText + "\r\n"
                    + "\r\nIndexFilterLastEditedToday";
            }
            if (IndexConfig.LastEditedPast7Days) {
                FilterText = FilterText + "\r\n"
                    + "\r\nIndexFilterLastEditedPast7Days";
            }
            if (IndexConfig.LastEditedPast30Days) {
                FilterText = FilterText + "\r\n"
                    + "\r\nIndexFilterLastEditedPast30Days";
            }
            if (IndexConfig.Open) {
                FilterText = FilterText + "\r\n"
                    + "\r\nIndexFilterOpen";
            }
            //
            core.visitProperty.setProperty(IndexConfigPrefix + encodeText(IndexConfig.ContentID), FilterText);
            //
            // -----------------------------------------------------------------------------------------------
            //   Member Properties (persistant)
            // -----------------------------------------------------------------------------------------------
            //
            FilterText = "";
            //
            // Save Admin Column
            //
            SubList = "";
            foreach (var kvp in IndexConfig.Columns) {
                indexConfigColumnClass column = kvp.Value;
                if (!string.IsNullOrEmpty(column.Name)) {
                    SubList = SubList + "\r\n" + column.Name + "\t" + column.Width;
                }
            }
            if (!string.IsNullOrEmpty(SubList)) {
                FilterText = FilterText + "\r\nColumns" + SubList + "\r\n";
            }
            //
            // Sorts
            //
            SubList = "";
            foreach (var kvp in IndexConfig.Sorts) {
                indexConfigSortClass sort = kvp.Value;
                if (!string.IsNullOrEmpty(sort.fieldName)) {
                    SubList = SubList + "\r\n" + sort.fieldName + "\t" + sort.direction;
                }
            }
            if (!string.IsNullOrEmpty(SubList)) {
                FilterText = FilterText + "\r\nSorts" + SubList + "\r\n";
            }
            core.userProperty.setProperty(IndexConfigPrefix + encodeText(IndexConfig.ContentID), FilterText);
            //

        }
        //
        //
        //
        private string GetFormInputWithFocus2(string ElementName, string CurrentValue = "", int Height = -1, int Width = -1, string ElementID = "", string OnFocusJavascript = "", string HtmlClass = "") {
            string tempGetFormInputWithFocus2 = null;
            tempGetFormInputWithFocus2 = core.html.inputText(ElementName, CurrentValue, Height, Width, ElementID);
            if (!string.IsNullOrEmpty(OnFocusJavascript)) {
                tempGetFormInputWithFocus2 = genericController.vbReplace(tempGetFormInputWithFocus2, ">", " onFocus=\"" + OnFocusJavascript + "\">");
            }
            if (!string.IsNullOrEmpty(HtmlClass)) {
                tempGetFormInputWithFocus2 = genericController.vbReplace(tempGetFormInputWithFocus2, ">", " class=\"" + HtmlClass + "\">");
            }
            return tempGetFormInputWithFocus2;
        }
        //
        //
        //
        private string GetFormInputWithFocus(string ElementName, string CurrentValue, int Height, int Width, string ElementID, string OnFocus) {
            return GetFormInputWithFocus2(ElementName, CurrentValue, Height, Width, ElementID, OnFocus);
        }
        //
        //
        //
        private string GetFormInputDateWithFocus2(string ElementName, string CurrentValue = "", string Width = "", string ElementID = "", string OnFocusJavascript = "", string HtmlClass = "") {
            string tempGetFormInputDateWithFocus2 = null;
            tempGetFormInputDateWithFocus2 = core.html.inputDate(ElementName, CurrentValue, Width, ElementID);
            if (!string.IsNullOrEmpty(OnFocusJavascript)) {
                tempGetFormInputDateWithFocus2 = genericController.vbReplace(tempGetFormInputDateWithFocus2, ">", " onFocus=\"" + OnFocusJavascript + "\">");
            }
            if (!string.IsNullOrEmpty(HtmlClass)) {
                tempGetFormInputDateWithFocus2 = genericController.vbReplace(tempGetFormInputDateWithFocus2, ">", " class=\"" + HtmlClass + "\">");
            }
            return tempGetFormInputDateWithFocus2;
        }
        //
        //
        //
        private string GetFormInputDateWithFocus(string ElementName, string CurrentValue, string Width, string ElementID, string OnFocus) {
            return GetFormInputDateWithFocus2(ElementName, CurrentValue, Width, ElementID, OnFocus);
        }
        //
        //=================================================================================
        //
        //=================================================================================
        //
        private string GetForm_Index_AdvancedSearch(cdefModel adminContent, editRecordClass editRecord) {
            string returnForm = "";
            try {
                //
                string SearchValue = null;
                FindWordMatchEnum MatchOption = 0;
                int FormFieldPtr = 0;
                int FormFieldCnt = 0;
                cdefModel CDef = null;
                string FieldName = null;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                int FieldPtr = 0;
                bool RowEven = false;
                string Button = null;
                string RQS = null;
                string[] FieldNames = null;
                string[] FieldCaption = null;
                int[] fieldId = null;
                int[] fieldTypeId = null;
                string[] FieldValue = null;
                int[] FieldMatchOptions = null;
                int FieldMatchOption = 0;
                string[] FieldLookupContentName = null;
                string[] FieldLookupList = null;
                int ContentID = 0;
                int FieldCnt = 0;
                int FieldSize = 0;
                int RowPointer = 0;
                adminUIController Adminui = new adminUIController(core);
                string LeftButtons = "";
                string ButtonBar = null;
                string Title = null;
                string TitleBar = null;
                string Content = null;
                string TitleDescription = null;
                indexConfigClass IndexConfig = null;
                //
                if (!(false)) {
                    //
                    // Process last form
                    //
                    Button = core.docProperties.getText("button");
                    if (!string.IsNullOrEmpty(Button)) {
                        switch (Button) {
                            case ButtonSearch:
                                IndexConfig = LoadIndexConfig(adminContent);
                                FormFieldCnt = core.docProperties.getInteger("fieldcnt");
                                if (FormFieldCnt > 0) {
                                    for (FormFieldPtr = 0; FormFieldPtr < FormFieldCnt; FormFieldPtr++) {
                                        FieldName = genericController.vbLCase(core.docProperties.getText("fieldname" + FormFieldPtr));
                                        MatchOption = (FindWordMatchEnum)core.docProperties.getInteger("FieldMatch" + FormFieldPtr);
                                        switch (MatchOption) {
                                            case FindWordMatchEnum.MatchEquals:
                                            case FindWordMatchEnum.MatchGreaterThan:
                                            case FindWordMatchEnum.matchincludes:
                                            case FindWordMatchEnum.MatchLessThan:
                                                SearchValue = core.docProperties.getText("FieldValue" + FormFieldPtr);
                                                break;
                                            default:
                                                SearchValue = "";
                                                break;
                                        }
                                        if (!IndexConfig.FindWords.ContainsKey(FieldName)) {
                                            //
                                            // fieldname not found, save if not FindWordMatchEnum.MatchIgnore
                                            //
                                            if (MatchOption != FindWordMatchEnum.MatchIgnore) {
                                                indexConfigFindWordClass findWord = new indexConfigFindWordClass();
                                                findWord.Name = FieldName;
                                                findWord.MatchOption = MatchOption;
                                                findWord.Value = SearchValue;
                                                IndexConfig.FindWords.Add(FieldName, findWord);
                                            }
                                        } else {
                                            //
                                            // fieldname was found
                                            //
                                            IndexConfig.FindWords[FieldName].MatchOption = MatchOption;
                                            IndexConfig.FindWords[FieldName].Value = SearchValue;
                                        }
                                    }
                                }
                                SetIndexSQL_SaveIndexConfig(IndexConfig);
                                return string.Empty;
                            case ButtonCancel:
                                return string.Empty;
                        }
                    }
                    IndexConfig = LoadIndexConfig(adminContent);
                    Button = "CriteriaSelect";
                    RQS = core.doc.refreshQueryString;
                    //
                    // ----- ButtonBar
                    //
                    if (MenuDepth > 0) {
                        LeftButtons += core.html.button(ButtonClose, "", "", "window.close();");
                    } else {
                        LeftButtons += core.html.button(ButtonCancel);
                        //LeftButtons &= core.main_GetFormButton(ButtonCancel, , , "return processSubmit(this)")
                    }
                    LeftButtons += core.html.button(ButtonSearch);
                    //LeftButtons &= core.main_GetFormButton(ButtonSearch, , , "return processSubmit(this)")
                    ButtonBar = Adminui.GetButtonBar(LeftButtons, "");
                    //
                    // ----- TitleBar
                    //
                    Title = adminContent.Name;
                    Title = Title + " Advanced Search";
                    Title = "<strong>" + Title + "</strong>";
                    Title = SpanClassAdminNormal + Title + "</span>";
                    //Title = Title & core.main_GetHelpLink(46, "Using the Advanced Search Page", BubbleCopy_AdminIndexPage)
                    TitleDescription = "<div>Enter criteria for each field to identify and select your results. The results of a search will have to have all of the criteria you enter.</div>";
                    TitleBar = Adminui.GetTitleBar(Title, TitleDescription);
                    //
                    // ----- List out all fields
                    //
                    CDef = cdefModel.getCdef(core, adminContent.Name);
                    FieldSize = 100;
                    Array.Resize(ref FieldNames, FieldSize + 1);
                    Array.Resize(ref FieldCaption, FieldSize + 1);
                    Array.Resize(ref fieldId, FieldSize + 1);
                    Array.Resize(ref fieldTypeId, FieldSize + 1);
                    Array.Resize(ref FieldValue, FieldSize + 1);
                    Array.Resize(ref FieldMatchOptions, FieldSize + 1);
                    Array.Resize(ref FieldLookupContentName, FieldSize + 1);
                    Array.Resize(ref FieldLookupList, FieldSize + 1);
                    foreach (KeyValuePair<string, cdefFieldModel> keyValuePair in adminContent.fields) {
                        cdefFieldModel field = keyValuePair.Value;
                        if (FieldPtr >= FieldSize) {
                            FieldSize = FieldSize + 100;
                            Array.Resize(ref FieldNames, FieldSize + 1);
                            Array.Resize(ref FieldCaption, FieldSize + 1);
                            Array.Resize(ref fieldId, FieldSize + 1);
                            Array.Resize(ref fieldTypeId, FieldSize + 1);
                            Array.Resize(ref FieldValue, FieldSize + 1);
                            Array.Resize(ref FieldMatchOptions, FieldSize + 1);
                            Array.Resize(ref FieldLookupContentName, FieldSize + 1);
                            Array.Resize(ref FieldLookupList, FieldSize + 1);
                        }
                        FieldName = genericController.vbLCase(field.nameLc);
                        FieldNames[FieldPtr] = FieldName;
                        FieldCaption[FieldPtr] = field.caption;
                        fieldId[FieldPtr] = field.id;
                        fieldTypeId[FieldPtr] = field.fieldTypeId;
                        if (fieldTypeId[FieldPtr] == FieldTypeIdLookup) {
                            ContentID = field.lookupContentID;
                            if (ContentID > 0) {
                                FieldLookupContentName[FieldPtr] = cdefModel.getContentNameByID(core, ContentID);
                            }
                            FieldLookupList[FieldPtr] = field.lookupList;
                        }
                        //
                        // set prepoplate value from indexconfig
                        //
                        if (IndexConfig.FindWords.ContainsKey(FieldName)) {
                            FieldValue[FieldPtr] = IndexConfig.FindWords[FieldName].Value;
                            FieldMatchOptions[FieldPtr] = (int)IndexConfig.FindWords[FieldName].MatchOption;
                        }
                        FieldPtr += 1;
                    }
                    //        Criteria = "(active<>0)and(ContentID=" & adminContent.id & ")and(authorable<>0)"
                    //        CS = core.app.csOpen("Content Fields", Criteria, "EditSortPriority")
                    //        FieldPtr = 0
                    //        Do While core.app.csv_IsCSOK(CS)
                    //            If FieldPtr >= FieldSize Then
                    //                FieldSize = FieldSize + 100
                    //                ReDim Preserve FieldNames(FieldSize)
                    //                ReDim Preserve FieldCaption(FieldSize)
                    //                ReDim Preserve FieldID(FieldSize)
                    //                ReDim Preserve FieldType(FieldSize)
                    //                ReDim Preserve FieldValue(FieldSize)
                    //                ReDim Preserve FieldMatchOptions(FieldSize)
                    //                ReDim Preserve FieldLookupContentName(FieldSize)
                    //                ReDim Preserve FieldLookupList(FieldSize)
                    //            End If
                    //            FieldName = genericController.vbLCase(core.db.cs_getText(CS, "name"))
                    //            FieldNames(FieldPtr) = FieldName
                    //            FieldCaption(FieldPtr) = core.db.cs_getText(CS, "Caption")
                    //            FieldID(FieldPtr) = core.app.cs_getInteger(CS, "ID")
                    //            FieldType(FieldPtr) = core.app.cs_getInteger(CS, "Type")
                    //            If FieldType(FieldPtr) = 7 Then
                    //                ContentID = core.app.cs_getInteger(CS, "LookupContentID")
                    //                If ContentID > 0 Then
                    //                    FieldLookupContentName(FieldPtr) = cdefmodel.getContentNameByID(core,ContentID)
                    //                End If
                    //                FieldLookupList(FieldPtr) = core.db.cs_getText(CS, "LookupList")
                    //            End If
                    //            '
                    //            ' set prepoplate value from indexconfig
                    //            '
                    //            With IndexConfig
                    //                If .findwords.count > 0 Then
                    //                    For Ptr = 0 To .findwords.count - 1
                    //                        If .FindWords[Ptr].Name = FieldName Then
                    //                            FieldValue(FieldPtr) = .FindWords[Ptr].Value
                    //                            FieldMatchOptions(FieldPtr) = .FindWords[Ptr].MatchOption
                    //                            Exit For
                    //                        End If
                    //                    Next
                    //                End If
                    //            End With
                    //            If CriteriaCount > 0 Then
                    //                For CriteriaPointer = 0 To CriteriaCount - 1
                    //                    FieldMatchOptions(FieldPtr) = 0
                    //                    If genericController.vbInstr(1, CriteriaValues(CriteriaPointer), FieldNames(FieldPtr) & "=", vbTextCompare) = 1 Then
                    //                        NameValues = Split(CriteriaValues(CriteriaPointer), "=")
                    //                        FieldValue(FieldPtr) = NameValues(1)
                    //                        FieldMatchOptions(FieldPtr) = 1
                    //                    ElseIf genericController.vbInstr(1, CriteriaValues(CriteriaPointer), FieldNames(FieldPtr) & ">", vbTextCompare) = 1 Then
                    //                        NameValues = Split(CriteriaValues(CriteriaPointer), ">")
                    //                        FieldValue(FieldPtr) = NameValues(1)
                    //                        FieldMatchOptions(FieldPtr) = 2
                    //                    ElseIf genericController.vbInstr(1, CriteriaValues(CriteriaPointer), FieldNames(FieldPtr) & "<", vbTextCompare) = 1 Then
                    //                        NameValues = Split(CriteriaValues(CriteriaPointer), "<")
                    //                        FieldValue(FieldPtr) = NameValues(1)
                    //                        FieldMatchOptions(FieldPtr) = 3
                    //                    End If
                    //                Next
                    //            End If
                    //            FieldPtr = FieldPtr + 1
                    //            Call core.app.nextCSRecord(CS)
                    //        Loop
                    //        Call core.app.closeCS(CS)
                    FieldCnt = FieldPtr;
                    //
                    // Add headers to stream
                    //
                    returnForm = returnForm + "<table border=0 width=100% cellspacing=0 cellpadding=4>";
                    //
                    RowPointer = 0;
                    for (FieldPtr = 0; FieldPtr < FieldCnt; FieldPtr++) {
                        returnForm = returnForm + core.html.inputHidden("fieldname" + FieldPtr, FieldNames[FieldPtr]);
                        RowEven = ((RowPointer % 2) == 0);
                        FieldMatchOption = FieldMatchOptions[FieldPtr];
                        switch (fieldTypeId[FieldPtr]) {
                            case FieldTypeIdDate:
                                //
                                // Date
                                //
                                returnForm = returnForm + "<tr>"
                                + "<td class=\"ccAdminEditCaption\">" + FieldCaption[FieldPtr] + "</td>"
                                + "<td class=\"ccAdminEditField\">"
                                + "<div style=\"display:block;float:left;width:800px;\">"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchIgnore).ToString(), FieldMatchOption.ToString(), "") + "ignore</div>"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchEmpty).ToString(), FieldMatchOption.ToString(), "") + "empty</div>"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchNotEmpty).ToString(), FieldMatchOption.ToString(), "") + "not&nbsp;empty</div>"
                                + "<div style=\"display:block;float:left;width:50px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchEquals).ToString(), FieldMatchOption.ToString(), "") + "=</div>"
                                + "<div style=\"display:block;float:left;width:50px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchGreaterThan).ToString(), FieldMatchOption.ToString(), "") + "&gt;</div>"
                                + "<div style=\"display:block;float:left;width:50px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchLessThan).ToString(), FieldMatchOption.ToString(), "") + "&lt;</div>"
                                + "<div style=\"display:block;float:left;width:300px;\">" + GetFormInputDateWithFocus2("fieldvalue" + FieldPtr, FieldValue[FieldPtr], "5", "", "", "ccAdvSearchText") + "</div>"
                                + "</div>"
                                + "</td>"
                                + "</tr>";
                                break;
                            case FieldTypeIdCurrency:
                            case FieldTypeIdFloat:
                            case FieldTypeIdInteger:
                            case FieldTypeIdAutoIdIncrement:
                                //
                                // Numeric
                                //
                                // changed FindWordMatchEnum.MatchEquals to MatchInclude to be compatible with Find Search
                                returnForm = returnForm + "<tr>"
                                + "<td class=\"ccAdminEditCaption\">" + FieldCaption[FieldPtr] + "</td>"
                                + "<td class=\"ccAdminEditField\">"
                                + "<div style=\"display:block;float:left;width:800px;\">"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchIgnore).ToString(), FieldMatchOption.ToString(), "") + "ignore</div>"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchEmpty).ToString(), FieldMatchOption.ToString(), "") + "empty</div>"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchNotEmpty).ToString(), FieldMatchOption.ToString(), "") + "not&nbsp;empty</div>"
                                + "<div style=\"display:block;float:left;width:50px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.matchincludes).ToString(), FieldMatchOption.ToString(), "n" + FieldPtr) + "=</div>"
                                + "<div style=\"display:block;float:left;width:50px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchGreaterThan).ToString(), FieldMatchOption.ToString(), "") + "&gt;</div>"
                                + "<div style=\"display:block;float:left;width:50px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchLessThan).ToString(), FieldMatchOption.ToString(), "") + "&lt;</div>"
                                + "<div style=\"display:block;float:left;width:300px;\">" + GetFormInputWithFocus2("fieldvalue" + FieldPtr, FieldValue[FieldPtr], 1, 5, "", "var e=getElementById('n" + FieldPtr + "');e.checked=1;", "ccAdvSearchText") + "</div>"
                                + "</div>"
                                + "</td>"
                                + "</tr>";

                                //                    s = s _
                                //                        & "<tr>" _
                                //                        & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                                //                        & "<td class=""ccAdminEditField"">" _
                                //                        & "<table border=0 width=100% cellspacing=0 cellpadding=0><tr>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchIgnore, FieldMatchOption, "") & "</td><td align=left width=100>ignore</td>" _
                                //                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchEmpty, FieldMatchOption, "") & "</td><td align=left width=100>empty</td>" _
                                //                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchNotEmpty, FieldMatchOption, "") & "</td><td align=left width=100>not&nbsp;empty</td>" _
                                //                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchEquals, FieldMatchOption, "") & "</td><td align=left width=100>=</td>" _
                                //                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchGreaterThan, FieldMatchOption, "") & "</td><td align=left width=100>&gt;</td>" _
                                //                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchLessThan, FieldMatchOption, "") & "</td><td align=left width=100>&lt;</td>" _
                                //                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                //                            & "<td align=left width=99%>" & GetFormInputWithFocus("fieldvalue" & FieldPtr, FieldValue(FieldPtr), 1, 5, "", "") & "</td>" _
                                //                        & "</tr></table>" _
                                //                        & "</td>" _
                                //                        & "</tr>"

                                RowPointer = RowPointer + 1;
                                break;
                            case FieldTypeIdFile:
                            case FieldTypeIdFileImage:
                                //
                                // File
                                //
                                returnForm = returnForm + "<tr>"
                                + "<td class=\"ccAdminEditCaption\">" + FieldCaption[FieldPtr] + "</td>"
                                + "<td class=\"ccAdminEditField\">"
                                + "<div style=\"display:block;float:left;width:800px;\">"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchIgnore).ToString(), FieldMatchOption.ToString(), "") + "ignore</div>"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchEmpty).ToString(), FieldMatchOption.ToString(), "") + "empty</div>"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchNotEmpty).ToString(), FieldMatchOption.ToString(), "") + "not&nbsp;empty</div>"
                                + "</div>"
                                + "</td>"
                                + "</tr>";
                                //s = s _
                                //    & "<tr>" _
                                //    & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                                //    & "<td class=""ccAdminEditField"">" _
                                //    & "<table border=0 width=100% cellspacing=0 cellpadding=0><tr>" _
                                //        & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchIgnore, FieldMatchOption, "") & "</td><td align=left width=100>ignore</td>" _
                                //        & "<td width=10>&nbsp;&nbsp;</td>" _
                                //        & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchEmpty, FieldMatchOption, "") & "</td><td align=left width=100>empty</td>" _
                                //        & "<td width=10>&nbsp;&nbsp;</td>" _
                                //        & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchNotEmpty, FieldMatchOption, "") & "</td><td align=left width=100>not&nbsp;empty</td>" _
                                //        & "<td align=left width=99%>&nbsp;</td>" _
                                //    & "</tr></table>" _
                                //    & "</td>" _
                                //    & "</tr>"
                                RowPointer = RowPointer + 1;
                                break;
                            case FieldTypeIdBoolean:
                                //
                                // Boolean
                                //
                                returnForm = returnForm + "<tr>"
                                + "<td class=\"ccAdminEditCaption\">" + FieldCaption[FieldPtr] + "</td>"
                                + "<td class=\"ccAdminEditField\">"
                                + "<div style=\"display:block;float:left;width:800px;\">"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchIgnore).ToString(), FieldMatchOption.ToString(), "") + "ignore</div>"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchTrue).ToString(), FieldMatchOption.ToString(), "") + "true</div>"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchFalse).ToString(), FieldMatchOption.ToString(), "") + "false</div>"
                                + "</div>"
                                + "</td>"
                                + "</tr>";
                                //                    s = s _
                                //                        & "<tr>" _
                                //                        & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                                //                        & "<td class=""ccAdminEditField"">" _
                                //                        & "<table border=0 width=100% cellspacing=0 cellpadding=0><tr>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchIgnore, FieldMatchOption, "") & "</td><td align=left width=100>  ignore</td>" _
                                //                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchTrue, FieldMatchOption, "") & "</td><td align=left width=100>true</td>" _
                                //                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchFalse, FieldMatchOption, "") & "</td><td align=left width=100>false</td>" _
                                //                            & "<td width=99%>&nbsp;</td>" _
                                //                        & "</tr></table>" _
                                //                        & "</td>" _
                                //                        & "</tr>"
                                break;
                            case FieldTypeIdText:
                            case FieldTypeIdLongText:
                            case FieldTypeIdHTML:
                            case FieldTypeIdFileHTML:
                            case FieldTypeIdFileCSS:
                            case FieldTypeIdFileJavascript:
                            case FieldTypeIdFileXML:
                                //
                                // Text
                                //
                                returnForm = returnForm + "<tr>"
                                + "<td class=\"ccAdminEditCaption\">" + FieldCaption[FieldPtr] + "</td>"
                                + "<td class=\"ccAdminEditField\">"
                                + "<div style=\"display:block;float:left;width:800px;\">"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchIgnore).ToString(), FieldMatchOption.ToString(), "") + "ignore</div>"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchEmpty).ToString(), FieldMatchOption.ToString(), "") + "empty</div>"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchNotEmpty).ToString(), FieldMatchOption.ToString(), "") + "not&nbsp;empty</div>"
                                + "<div style=\"display:block;float:left;width:150px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.matchincludes).ToString(), FieldMatchOption.ToString(), "t" + FieldPtr) + "includes</div>"
                                + "<div style=\"display:block;float:left;width:300px;\">" + GetFormInputWithFocus2("fieldvalue" + FieldPtr, FieldValue[FieldPtr], 1, 5, "", "var e=getElementById('t" + FieldPtr + "');e.checked=1;", "ccAdvSearchText") + "</div>"
                                + "</div>"
                                + "</td>"
                                + "</tr>";
                                //
                                //
                                //                    s = s _
                                //                        & "<tr>" _
                                //                        & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                                //                        & "<td class=""ccAdminEditField"" valign=absmiddle>" _
                                //                        & "<table border=0 width=100% cellspacing=0 cellpadding=0><tr>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchIgnore, FieldMatchOption, "") & "</td><td align=left width=100>&nbsp;&nbsp;ignore</td>" _
                                //                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchEmpty, FieldMatchOption, "") & "</td><td align=left width=100>empty</td>" _
                                //                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchNotEmpty, FieldMatchOption, "") & "</td><td align=left width=100>not&nbsp;empty</td>" _
                                //                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.matchincludes, FieldMatchOption, "t" & FieldPtr) & "</td><td align=center width=100>includes&nbsp;</td>" _
                                //                            & "<td align=left width=99%>" & GetFormInputWithFocus("FieldValue" & FieldPtr, FieldValue(FieldPtr), 1, 20, "", "var e=getElementById('t" & FieldPtr & "');e.checked=1;") & "</td>" _
                                //                        & "</tr></table>" _
                                //                        & "</td>" _
                                //                        & "</tr>"
                                RowPointer = RowPointer + 1;
                                break;
                            case FieldTypeIdLookup:
                            case FieldTypeIdMemberSelect:
                                //
                                // Lookup
                                //
                                //Dim SelectOption As String
                                //Dim CurrentValue As String
                                //If FieldLookupContentName(FieldPtr) <> "" Then
                                //    ContentName = FieldLookupContentName(FieldPtr)
                                //    DataSourceName = cdefmodel.getContentDataSource(core,ContentName)
                                //    TableName = core.main_GetContentTablename(ContentName)
                                //    SQL = "select distinct Name from " & TableName & " where (name is not null) order by name"
                                //    CS = core.app.openCsSql_rev(DataSourceName, SQL)
                                //    If Not core.app.csv_IsCSOK(CS) Then
                                //        selector = "no options"
                                //    Else
                                //        selector = vbCrLf & "<select name=""FieldValue" & FieldPtr & """ onFocus=""var e=getElementById('t" & FieldPtr & "');e.checked=1;"">"
                                //        CurrentValue = FieldValue(FieldPtr)
                                //        Do While core.app.csv_IsCSOK(CS)
                                //            SelectOption = core.db.cs_getText(CS, "name")
                                //            If CurrentValue = SelectOption Then
                                //                selector = selector & vbCrLf & "<option selected>" & SelectOption & "</option>"
                                //            Else
                                //                selector = selector & vbCrLf & "<option>" & SelectOption & "</option>"
                                //            End If
                                //            Call core.app.nextCSRecord(CS)
                                //        Loop
                                //        selector = selector & vbCrLf & "</select>"
                                //    End If
                                //    Call core.app.closeCS(CS)
                                //    'selector = core.htmldoc.main_GetFormInputSelect2("FieldValue" & FieldPtr, FieldValue(FieldPtr), FieldLookupContentName(FieldPtr))
                                //Else
                                //    selector = core.htmldoc.main_GetFormInputSelectList2("FieldValue" & FieldPtr, FieldValue(FieldPtr), FieldLookupList(FieldPtr))
                                //End If
                                //selector = genericController.vbReplace(selector, ">", "onFocus=""var e=getElementById('t" & FieldPtr & "');e.checked=1;"">")
                                returnForm = returnForm + "<tr>"
                                + "<td class=\"ccAdminEditCaption\">" + FieldCaption[FieldPtr] + "</td>"
                                + "<td class=\"ccAdminEditField\">"
                                + "<div style=\"display:block;float:left;width:800px;\">"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchIgnore).ToString(), FieldMatchOption.ToString(), "") + "ignore</div>"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchEmpty).ToString(), FieldMatchOption.ToString(), "") + "empty</div>"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchNotEmpty).ToString(), FieldMatchOption.ToString(), "") + "not&nbsp;empty</div>"
                                + "<div style=\"display:block;float:left;width:150px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.matchincludes).ToString(), FieldMatchOption.ToString(), "t" + FieldPtr) + "includes</div>"
                                + "<div style=\"display:block;float:left;width:300px;\">" + GetFormInputWithFocus2("fieldvalue" + FieldPtr, FieldValue[FieldPtr], 1, 5, "", "var e=getElementById('t" + FieldPtr + "');e.checked=1;", "ccAdvSearchText") + "</div>"
                                + "</div>"
                                + "</td>"
                                + "</tr>";

                                //& "<div style=""display:block;float:left;width:150px;"">" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchEquals, FieldMatchOption, "t" & FieldPtr) & "=&nbsp;</div>" _
                                //& "<div style=""display:block;float:left;width:300px;"">" & selector & "</div>" _

                                //                    s = s _
                                //                        & "<tr>" _
                                //                        & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                                //                        & "<td class=""ccAdminEditField"" valign=absmiddle>" _
                                //                        & "<table border=0 width=100% cellspacing=0 cellpadding=0><tr>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchIgnore, FieldMatchOption, "") & "</td><td align=left width=100>&nbsp;&nbsp;ignore</td>" _
                                //                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchEmpty, FieldMatchOption, "") & "</td><td align=left width=100>empty</td>" _
                                //                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchNotEmpty, FieldMatchOption, "") & "</td><td align=left width=100>not&nbsp;empty</td>" _
                                //                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                //                            & "<td width=10 align=right>" & core.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchEquals, FieldMatchOption, "t" & FieldPtr) & "</td><td align=center width=100>=&nbsp;</td>" _
                                //                            & "<td align=left width=99%>" & selector & "</td>" _
                                //                        & "</tr></table>" _
                                //                        & "</td>" _
                                //                        & "</tr>"



                                //s = s & "<tr><td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>"
                                //If FieldLookupContentName(FieldPtr) <> "" Then
                                //    s = s _
                                //        & "<td class=""ccAdminEditField"">" _
                                //        & core.htmldoc.main_GetFormInputSelect2(FieldNames(FieldPtr), FieldValue(FieldPtr), FieldLookupContentName(FieldPtr), , "Any") & "</td>"
                                //Else
                                //    s = s _
                                //        & "<td class=""ccAdminEditField"">" _
                                //        & core.htmldoc.main_GetFormInputSelectList2(FieldNames(FieldPtr), FieldValue(FieldPtr), FieldLookupList(FieldPtr), , "Any") & "</td>"
                                //End If
                                //s = s & "</tr>"
                                RowPointer = RowPointer + 1;
                                break;
                        }
                    }
                    returnForm = returnForm + genericController.StartTableRow();
                    returnForm = returnForm + genericController.StartTableCell("120", 1, RowEven, "right") + "<img src=/ccLib/images/spacer.gif width=120 height=1></td>";
                    returnForm = returnForm + genericController.StartTableCell("99%", 1, RowEven, "left") + "<img src=/ccLib/images/spacer.gif width=1 height=1></td>";
                    returnForm = returnForm + kmaEndTableRow;
                    returnForm = returnForm + "</table>";
                    Content = returnForm;
                    //
                    // Assemble LiveWindowTable
                    //
                    //        Stream.Add( OpenLiveWindowTable)
                    Stream.Add("\r\n" + core.html.formStart());
                    Stream.Add(ButtonBar);
                    Stream.Add(TitleBar);
                    Stream.Add(Content);
                    Stream.Add(ButtonBar);
                    Stream.Add("<input type=hidden name=fieldcnt VALUE=" + FieldCnt + ">");
                    //Stream.Add( "<input type=hidden name=af VALUE=" & AdminFormIndex & ">")
                    Stream.Add("<input type=hidden name=" + RequestNameAdminSubForm + " VALUE=" + AdminFormIndex_SubFormAdvancedSearch + ">");
                    Stream.Add("</form>");
                    //        Stream.Add( CloseLiveWindowTable)
                    //
                    returnForm = Stream.Text;
                    core.html.addTitle(adminContent.Name + " Advanced Search");
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnForm;
        }
    }
}