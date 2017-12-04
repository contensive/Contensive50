
using Controllers;
using System.Xml;
using Contensive.Core;
using Models.Entity;
// 

namespace Contensive.Addons.AdminSite {
    
     public partial class getAdminSiteClass : Contensive.BaseClasses.AddonBaseClass {
        
        // 
        // 
        // ====================================================================================================
        //  objects passed in - do not dispose
        //    sets cp from argument For use In calls To other objects, Then cpCore because cp cannot be used since that would be a circular depenancy
        // ====================================================================================================
        // 
        private CPClass cp;
        
        //  local cp set in constructor
        private coreClass cpCore;
        
        //  cpCore -- short term, this is the migration solution from a built-in tool, to an addon
        // 
        // ====================================================================================================
        // '' <summary>
        // '' addon method, deliver complete Html admin site
        // '' </summary>
        // '' <param name="cp"></param>
        // '' <returns></returns>
        public override object execute(Contensive.BaseClasses.CPBaseClass cp) {
            string returnHtml = "";
            try {
                // 
                //  -- ok to cast cpbase to cp because they build from the same solution
                cp;
                CPClass;
                cpCore = this.cp.core;
                // 
                //  -- log request
                string SaveContent = ("" 
                            + (Now() + ("\r\n" + ("member.name:" 
                            + (cpCore.doc.authContext.user.name + ("\r\n" + ("member.id:" 
                            + (cpCore.doc.authContext.user.id + ("\r\n" + ("visit.id:" 
                            + (cpCore.doc.authContext.visit.id + ("\r\n" + ("url:" 
                            + (cpCore.webServer.requestUrl + ("\r\n" + ("url source:" 
                            + (cpCore.webServer.requestUrlSource + ("\r\n" + ("----------" + ("\r\n" + "form post:"))))))))))))))))))));
                foreach (string key in cpCore.docProperties.getKeyList()) {
                    docPropertiesClass docProperty = cpCore.docProperties.getProperty(key);
                    if (docProperty.IsForm) {
                        ("\r\n" + docProperty.NameValue);
                    }
                    
                }
                
                if (!(cpCore.webServer.requestFormBinaryHeader == null)) {
                    byte[] BinaryHeader = cpCore.webServer.requestFormBinaryHeader;
                    string BinaryHeaderString = genericController.kmaByteArrayToString(BinaryHeader);
                    ("" + ("\r\n" + ("----------" + ("\r\n" + ("binary header:" + ("\r\n" 
                                + (BinaryHeaderString + "\r\n")))))));
                }
                
                logController.appendLog(cpCore, SaveContent, "admin", (cpCore.serverConfig.appConfig.name + "-request-"));
                // 
                if (!cpCore.doc.authContext.isAuthenticated) {
                    // 
                    //  --- must be authenticated to continue. Force a local login
                    // 
                    returnHtml = cpCore.addon.execute(Models.Entity.addonModel.create(cpCore, addonGuidLoginPage), new BaseClasses.CPUtilsBaseClass.addonExecuteContext(), With, {, ., errorCaption=Login Page, ., addonType=BaseClasses.CPUtilsBaseClass.addonContext.ContextPage);
                }
                else if (!cpCore.doc.authContext.isAuthenticatedContentManager(cpCore)) {
                    // 
                    //  --- member must have proper access to continue
                    // 
                    returnHtml = ("" + ("<p>" 
                                + (SpanClassAdminNormal + ("You are attempting to enter an area which your account does not have access." 
                                + (cr + ("<ul class=\"ccList\">" 
                                + (cr + ("<li class=\"ccListItem\">To return to the public web site, use your back button, or <a href=\"" 
                                + (requestAppRootPath + ("\">Click Here</A>." 
                                + (cr + ("<li class=\"ccListItem\">To login under a different account, <a href=\"/" 
                                + (cpCore.serverConfig.appConfig.adminRoute + ("?method=logout\" rel=\"nofollow\">Click Here</A>" 
                                + (cr + ("<li class=\"ccListItem\">To have your account access changed to include this area, please contact the <" +
                                "a href=\"mailto:" 
                                + (cpCore.siteProperties.getText("EmailAdmin") + ("\">system administrator</A>. " 
                                + (cr + ("</ul>" + "</span></p>"))))))))))))))))))));
                    returnHtml = ("" 
                                + (cpCore.html.main_GetPanelHeader("Unauthorized Access") + cpCore.html.main_GetPanel(returnHtml, "ccPanel", "ccPanelHilite", "ccPanelShadow", "400", 15)));
                    returnHtml = ("" 
                                + (cr + ("<div style=\"display:table;margin:100px auto auto auto;\">" 
                                + (genericController.htmlIndent(returnHtml) 
                                + (cr + "</div>")))));
                    cpCore.doc.setMetaContent(0, 0);
                    cpCore.html.addTitle("Unauthorized Access", "adminSite");
                    returnHtml = ("<div class=\"ccBodyAdmin ccCon\">" 
                                + (returnHtml + "</div>"));
                }
                else {
                    // 
                    //  get admin content
                    // 
                    returnHtml = ("<div class=\"ccBodyAdmin ccCon\">" 
                                + (this.getAdminBody() + "</div>"));
                }
                
                // 
                //  Log response
                // 
                ("" 
                            + (Now() + ("\r\n" + ("member.name:" 
                            + (cpCore.doc.authContext.user.name + ("\r\n" + ("member.id:" 
                            + (cpCore.doc.authContext.user.id + ("\r\n" + ("visit.id:" 
                            + (cpCore.doc.authContext.visit.id + ("\r\n" + ("url:" 
                            + (cpCore.webServer.requestUrl + ("\r\n" + ("url source:" 
                            + (cpCore.webServer.requestUrlSource + ("\r\n" + ("----------" + ("\r\n" + ("response:" + ("\r\n" + returnHtml))))))))))))))))))))));
                DateTime rightNow = DateTime.Now;
                logController.appendLog(cpCore, SaveContent, "admin", (rightNow.Year 
                                + (rightNow.Month.ToString("00") 
                                + (rightNow.Day.ToString("00") 
                                + (rightNow.Hour.ToString("00") 
                                + (rightNow.Minute.ToString("00") + rightNow.Second.ToString("00")))))));
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            
            return returnHtml;
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' REFACTOR - Constructor for addon instances. Until refactoring, calls into other methods must be constructed with (cpCoreClass) variation.
        // '' </summary>
        // '' <param name="cp"></param>
        // '' <remarks></remarks>
        public getAdminSiteClass() {
            ClassInitialized = false;
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' REFACTOR - Constructor for non-addon instances. (REFACTOR - work-around for pre-refactoring of admin remote methods currently in core classes)
        // '' </summary>
        // '' <param name="cpCore"></param>
        public getAdminSiteClass(Contensive.Core.CPClass cp) {
            this.cp = cp;
            cpCore = this.cp.core;
            ClassInitialized = false;
        }
        
        // 
        // ========================================================================
        // 
        private string getAdminBody(string ContentArgFromCaller, void =, void ) {
            string result = "";
            // Warning!!! Optional parameters not supported
            try {
                int DefaultWrapperID;
                string AddonHelpCopy;
                string InstanceOptionString;
                int HelpLevel;
                int HelpAddonID;
                int HelpCollectionID;
                string CurrentLink;
                string EditReferer;
                string ContentCell;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                int addonId;
                string AddonGuid;
                string AddonName = "";
                bool UseContentWatchLink;
                editRecordClass editRecord = new editRecordClass();
                Models.Complex.cdefModel AdminContent = new Models.Complex.cdefModel();
                // 
                // -------------------------------------------------------------------------------
                //  Setup defaults
                // -------------------------------------------------------------------------------
                // 
                cpCore.db.sqlCommandTimeout = 300;
                ButtonObjectCount = 0;
                ImagePreloadCount = 0;
                JavaScriptString = "";
                ContentWatchLoaded = false;
                editRecord.Loaded = false;
                UseContentWatchLink = cpCore.siteProperties.useContentWatchLink;
                cpCore.html.addScriptCode_onLoad("document.getElementsByTagName(\'BODY\')[0].onclick = BodyOnClick;", "Contensive");
                cpCore.doc.setMetaContent(0, 0);
                // 
                // -------------------------------------------------------------------------------
                //  check for member login, if logged in and no admin, lock out
                //  Do CheckMember here because we need to know who is there to create proper blocked menu
                // -------------------------------------------------------------------------------
                // 
                if (!cpCore.doc.continueProcessing) {
                    // 
                    //  ----- no stream anyway, do nothing
                    // 
                    // ElseIf Not cpCore.doc.authContext.isAuthenticated Then
                    //     '
                    //     ' --- must be authenticated to continue
                    //     '
                    //     Dim loginAddon As New Addons.addon_loginClass(cpCore)
                    //     Stream.Add(loginAddon.getLoginForm())
                }
                else {
                    // 
                    //  -- add exception if build verison does not match code
                    if ((cpCore.siteProperties.dataBuildVersion != cp.Version)) {
                        if ((cpCore.siteProperties.dataBuildVersion > cp.Version)) {
                            cpCore.handleException(new ApplicationException("Application code version is older than Db version. Run command line upgrade method on this site."));
                        }
                        else {
                            cpCore.handleException(new ApplicationException("Application code version is newer than Db version. Upgrade site code."));
                        }
                        
                    }
                    
                    // 
                    // -------------------------------------------------------------------------------
                    //  Get Requests
                    //    initialize adminContent and editRecord objects 
                    // -------------------------------------------------------------------------------
                    // 
                    this.GetForm_LoadControl(AdminContent, editRecord);
                    addonId = cpCore.docProperties.getInteger("addonid");
                    AddonGuid = cpCore.docProperties.getText("addonguid");
                    // '
                    // '-------------------------------------------------------------------------------
                    // '
                    // '-------------------------------------------------------------------------------
                    // '
                    // If AdminContent.fields.Count > 0 Then
                    //     ReDim EditRecordValuesObject(AdminContent.fields.Count)
                    //     ReDim EditRecordDbValues(AdminContent.fields.Count)
                    // End If
                    // 
                    // -------------------------------------------------------------------------------
                    //  Process SourceForm/Button into Action/Form, and process
                    // -------------------------------------------------------------------------------
                    // 
                    if ((cpCore.docProperties.getText("Button") == ButtonCancelAll)) {
                        AdminForm = AdminFormRoot;
                    }
                    else {
                        ProcessForms(AdminContent, editRecord);
                        ProcessActions(AdminContent, editRecord, UseContentWatchLink);
                    }
                    
                    // 
                    // -------------------------------------------------------------------------------
                    //  Normalize values to be needed
                    // -------------------------------------------------------------------------------
                    // 
                    if ((editRecord.id != 0)) {
                        cpCore.workflow.ClearEditLock(AdminContent.Name, editRecord.id);
                    }
                    
                    // 
                    if ((AdminForm < 1)) {
                        // 
                        //  No form was set, use default form
                        // 
                        if ((AdminContent.Id <= 0)) {
                            AdminForm = AdminFormRoot;
                        }
                        else {
                            AdminForm = AdminFormIndex;
                        }
                        
                    }
                    
                    // 
                    if ((AdminForm == AdminFormLegacyAddonManager)) {
                        // 
                        //  patch out any old links to the legacy addon manager
                        // 
                        AdminForm = 0;
                        AddonGuid = addonGuidAddonManager;
                    }
                    
                    // 
                    // -------------------------------------------------------------------------------
                    //  Edit form but not valid record case
                    //  Put this here so we can display the error without being stuck displaying the edit form
                    //  Putting the error on the edit form is confusing because there are fields to fill in
                    // -------------------------------------------------------------------------------
                    // 
                    if ((AdminSourceForm == AdminFormEdit)) {
                        if ((!(cpCore.doc.debug_iUserError != "") 
                                    && ((AdminButton == ButtonOK) 
                                    || ((AdminButton == ButtonCancel) 
                                    || (AdminButton == ButtonDelete))))) {
                            EditReferer = cpCore.docProperties.getText("EditReferer");
                            CurrentLink = genericController.modifyLinkQuery(cpCore.webServer.requestUrl, "editreferer", "", false);
                            CurrentLink = genericController.vbLCase(CurrentLink);
                            // 
                            //  check if this editreferer includes cid=thisone and id=thisone -- if so, go to index form for this cid
                            // 
                            if (((EditReferer != "") 
                                        && (EditReferer.ToLower() != CurrentLink))) {
                                // 
                                //  return to the page it came from
                                // 
                                return cpCore.webServer.redirect(EditReferer, "Admin Edit page returning to the EditReferer setting");
                            }
                            else {
                                // 
                                //  return to the index page for this content
                                // 
                                AdminForm = AdminFormIndex;
                            }
                            
                        }
                        
                        if (BlockEditForm) {
                            AdminForm = AdminFormIndex;
                        }
                        
                    }
                    
                    HelpLevel = cpCore.docProperties.getInteger("helplevel");
                    HelpAddonID = cpCore.docProperties.getInteger("helpaddonid");
                    HelpCollectionID = cpCore.docProperties.getInteger("helpcollectionid");
                    if ((HelpCollectionID == 0)) {
                        HelpCollectionID = cpCore.visitProperty.getInteger("RunOnce HelpCollectionID");
                        if ((HelpCollectionID != 0)) {
                            cpCore.visitProperty.setProperty("RunOnce HelpCollectionID", "");
                        }
                        
                    }
                    
                    // 
                    // -------------------------------------------------------------------------------
                    //  build refresh string
                    // -------------------------------------------------------------------------------
                    // 
                    if ((AdminContent.Id != 0)) {
                        cpCore.doc.addRefreshQueryString("cid", genericController.encodeText(AdminContent.Id));
                    }
                    
                    if ((editRecord.id != 0)) {
                        cpCore.doc.addRefreshQueryString("id", genericController.encodeText(editRecord.id));
                    }
                    
                    if ((TitleExtension != "")) {
                        cpCore.doc.addRefreshQueryString(RequestNameTitleExtension, genericController.EncodeRequestVariable(TitleExtension));
                    }
                    
                    if ((RecordTop != 0)) {
                        cpCore.doc.addRefreshQueryString("rt", genericController.encodeText(RecordTop));
                    }
                    
                    if ((RecordsPerPage != RecordsPerPageDefault)) {
                        cpCore.doc.addRefreshQueryString("rs", genericController.encodeText(RecordsPerPage));
                    }
                    
                    if ((AdminForm != 0)) {
                        cpCore.doc.addRefreshQueryString(RequestNameAdminForm, genericController.encodeText(AdminForm));
                    }
                    
                    if ((MenuDepth != 0)) {
                        cpCore.doc.addRefreshQueryString(RequestNameAdminDepth, genericController.encodeText(MenuDepth));
                    }
                    
                    // 
                    //  normalize guid
                    // 
                    if ((AddonGuid != "")) {
                        if (((AddonGuid.Length == 38) 
                                    && ((AddonGuid.Substring(0, 1) == "{") 
                                    && (AddonGuid.Substring((AddonGuid.Length - 1)) == "}")))) {
                            // 
                            //  Good to go
                            // 
                        }
                        else if ((AddonGuid.Length == 36)) {
                            // 
                            //  might be valid with the brackets, add them
                            // 
                            AddonGuid = ("{" 
                                        + (AddonGuid + "}"));
                        }
                        else if ((AddonGuid.Length == 32)) {
                            // 
                            //  might be valid with the brackets and the dashes, add them
                            // 
                            AddonGuid = ("{" 
                                        + (AddonGuid.Substring(0, 8) + ("-" 
                                        + (AddonGuid.Substring(8, 4) + ("-" 
                                        + (AddonGuid.Substring(12, 4) + ("-" 
                                        + (AddonGuid.Substring(16, 4) + ("-" 
                                        + (AddonGuid.Substring(20) + "}"))))))))));
                        }
                        else {
                            // 
                            //  not valid
                            // 
                            AddonGuid = "";
                        }
                        
                    }
                    
                    // 
                    // -------------------------------------------------------------------------------
                    //  Create the content
                    // -------------------------------------------------------------------------------
                    // 
                    ContentCell = "";
                    if ((ContentArgFromCaller != "")) {
                        // 
                        //  Use content passed in as an argument
                        // 
                        ContentCell = ContentArgFromCaller;
                    }
                    else if ((HelpAddonID != 0)) {
                        // 
                        //  display Addon Help
                        // 
                        cpCore.doc.addRefreshQueryString("helpaddonid", HelpAddonID.ToString);
                        ContentCell = GetAddonHelp(HelpAddonID, "");
                    }
                    else if ((HelpCollectionID != 0)) {
                        // 
                        //  display Collection Help
                        // 
                        cpCore.doc.addRefreshQueryString("helpcollectionid", HelpCollectionID.ToString);
                        ContentCell = GetCollectionHelp(HelpCollectionID, "");
                    }
                    else if ((AdminForm != 0)) {
                        // 
                        //  No content so far, try the forms
                        // 
                        switch (Int(AdminForm)) {
                            case AdminFormBuilderCollection:
                                ContentCell = GetForm_BuildCollection();
                                break;
                            case AdminFormSecurityControl:
                                AddonGuid = AddonGuidPreferences;
                                //     ContentCell = GetForm_SecurityControl()
                                break;
                            case AdminFormMetaKeywordTool:
                                ContentCell = GetForm_MetaKeywordTool();
                                break;
                            case AdminFormMobileBrowserControl:
                            case AdminFormPageControl:
                            case AdminFormEmailControl:
                                ContentCell = cpCore.addon.execute(addonModel.create(cpCore, AddonGuidPreferences), new BaseClasses.CPUtilsBaseClass.addonExecuteContext(), With, {, ., addonType=BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, ., errorCaption=Preferences);
                                // ContentCell = cpCore.addon.execute_legacy4(AddonGuidPreferences, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin)
                                break;
                            case AdminFormClearCache:
                                ContentCell = GetForm_ClearCache();
                                // Case AdminFormEDGControl
                                //     ContentCell = GetForm_StaticPublishControl()
                                break;
                            case AdminFormSpiderControl:
                                ContentCell = cpCore.addon.execute(addonModel.createByName(cpCore, "Content Spider Control"), new BaseClasses.CPUtilsBaseClass.addonExecuteContext(), With, {, ., addonType=BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, ., errorCaption=Content Spider Control);
                                // ContentCell = cpCore.addon.execute_legacy4("Content Spider Control", "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin)
                                break;
                            case AdminFormResourceLibrary:
                                ContentCell = cpCore.html.main_GetResourceLibrary2("", false, "", "", true);
                                break;
                            case AdminFormQuickStats:
                                ContentCell = GetForm_QuickStats();
                                break;
                            case AdminFormIndex:
                                ContentCell = GetForm_Index(AdminContent, editRecord, (AdminContent.ContentTableName.ToLower() == "ccemail"));
                                break;
                            case AdminFormEdit:
                                ContentCell = GetForm_Edit(AdminContent, editRecord);
                                break;
                            case AdminFormClose:
                                Stream.Add("<Script Language=\"JavaScript\" type=\"text/javascript\"> window.close(); </Script>");
                                break;
                            case AdminFormPublishing:
                                ContentCell = GetForm_Publish();
                                break;
                            case AdminFormContentChildTool:
                                ContentCell = GetContentChildTool();
                                break;
                            case AdminformPageContentMap:
                                ContentCell = GetForm_PageContentMap();
                                break;
                            case AdminformHousekeepingControl:
                                ContentCell = GetForm_HouseKeepingControl();
                                break;
                            case AdminFormTools:
                            case 100:
                                199;
                                coreToolsClass Tools = new coreToolsClass(cpCore);
                                ContentCell = Tools.GetForm();
                                break;
                            case AdminFormStyleEditor:
                                ContentCell = admin_GetForm_StyleEditor();
                                break;
                            case AdminFormDownloads:
                                ContentCell = GetForm_Downloads();
                                break;
                            case AdminformRSSControl:
                                ContentCell = cpCore.webServer.redirect(("?cid=" + Models.Complex.cdefModel.getContentId(cpCore, "RSS Feeds")), "RSS Control page is not longer supported. RSS Feeds are controlled from the RSS feed records.");
                                break;
                            case AdminFormImportWizard:
                                ContentCell = cpCore.addon.execute(addonModel.create(cpCore, addonGuidImportWizard), new BaseClasses.CPUtilsBaseClass.addonExecuteContext(), With, {, ., addonType=BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, ., errorCaption=Import Wizard);
                                // ContentCell = cpCore.addon.execute_legacy4(addonGuidImportWizard, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin)
                                break;
                            case AdminFormCustomReports:
                                ContentCell = GetForm_CustomReports();
                                break;
                            case AdminFormFormWizard:
                                ContentCell = cpCore.addon.execute(addonModel.create(cpCore, addonGuidFormWizard), new BaseClasses.CPUtilsBaseClass.addonExecuteContext(), With, {, ., addonType=BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, ., errorCaption=Form Wizard);
                                // ContentCell = cpCore.addon.execute_legacy4(addonGuidFormWizard, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin)
                                break;
                            case AdminFormLegacyAddonManager:
                                ContentCell = addonController.GetAddonManager(cpCore);
                                break;
                            case AdminFormEditorConfig:
                                ContentCell = GetForm_EditConfig();
                                break;
                            default:
                                ContentCell = "<p>The form requested is not supported</p>";
                                break;
                        }
                    }
                    else if (((addonId != 0) 
                                || ((AddonGuid != "") 
                                || (AddonName != "")))) {
                        // 
                        //  execute an addon
                        // 
                        if (((AddonGuid == addonGuidAddonManager) 
                                    || ((AddonName.ToLower() == "add-on manager") 
                                    || (AddonName.ToLower() == "addon manager")))) {
                            // 
                            //  Special case, call the routine that provides a backup
                            // 
                            cpCore.doc.addRefreshQueryString("addonguid", addonGuidAddonManager);
                            ContentCell = addonController.GetAddonManager(cpCore);
                        }
                        else {
                            addonModel addon = null;
                            string executeContextErrorCaption = "unknown";
                            if ((addonId != 0)) {
                                executeContextErrorCaption = ("id:" + addonId);
                                cpCore.doc.addRefreshQueryString("addonid", addonId.ToString());
                                addon = addonModel.create(cpCore, addonId);
                            }
                            else if ((AddonGuid != "")) {
                                executeContextErrorCaption = ("guid:" + AddonGuid);
                                cpCore.doc.addRefreshQueryString("addonguid", AddonGuid);
                                addon = addonModel.create(cpCore, AddonGuid);
                            }
                            else if ((AddonName != "")) {
                                executeContextErrorCaption = AddonName;
                                cpCore.doc.addRefreshQueryString("addonname", AddonName);
                                addon = addonModel.createByName(cpCore, AddonName);
                            }
                            
                            if (addon) {
                                IsNot;
                                null;
                                addonId = addon.id;
                                AddonName = addon.name;
                                AddonHelpCopy = addon.Help;
                                cpCore.doc.addRefreshQueryString(RequestNameRunAddon, addonId.ToString);
                            }
                            
                            InstanceOptionString = cpCore.userProperty.getText(("Addon [" 
                                            + (AddonName + "] Options")), "");
                            DefaultWrapperID = -1;
                            ContentCell = cpCore.addon.execute(addon, new BaseClasses.CPUtilsBaseClass.addonExecuteContext(), With, {, ., addonType=Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, ., instanceGuid=adminSiteInstanceId, ., instanceArguments=genericController.convertAddonArgumentstoDocPropertiesList(cpCore,InstanceOptionStringUnknown, ., wrapperID=DefaultWrapperID, ., errorCaption=executeContextErrorCaption);
                            if (string.IsNullOrEmpty(ContentCell)) {
                                // 
                                //  empty returned, display desktop
                                ContentCell = GetForm_Root();
                            }
                            
                        }
                        
                    }
                    else {
                        // 
                        //  nothing so far, display desktop
                        // 
                        ContentCell = GetForm_Root();
                    }
                    
                    // 
                    //  include fancybox if it was needed
                    // 
                    if (includeFancyBox) {
                        cpCore.addon.executeDependency(addonModel.create(cpCore, addonGuidjQueryFancyBox), new BaseClasses.CPUtilsBaseClass.addonExecuteContext(), With, {, ., addonType=BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin);
                        // Call cpCore.addon.execute_legacy4(addonGuidjQueryFancyBox)
                        cpCore.html.addScriptCode_head(("jQuery(document).ready(function() {" 
                                        + (fancyBoxHeadJS + "});")), "");
                    }
                    
                    // 
                    //  Pickup user errors
                    // 
                    if ((cpCore.doc.debug_iUserError != "")) {
                        ContentCell = ("<div class=\"ccAdminMsg\">" 
                                    + (errorController.error_GetUserError(cpCore) + ("</div>" + ContentCell)));
                    }
                    
                    // '
                    // ' If blank, must be an addon with a setting form that returned blank, do the dashboard again
                    // '
                    // If ContentCell = "" Then
                    //     '
                    //     ' must use the root as a default - bc forms and add-ons may return blank, meaning return to root
                    //     ' throw errors only if there is a user error
                    //     '
                    //     ContentCell = GetForm_Root()
                    //     'ContentCell = "<div class=""ccAdminMsg"">The form you requested did not return a valid response.</div>"
                    // End If
                    // 
                    Stream.Add((cr + GetForm_Top()));
                    Stream.Add(genericController.htmlIndent(ContentCell));
                    Stream.Add((cr + AdminFormBottom));
                    // Call Stream.Add(cr & "<script language=""javascript1.2"" type=""text/javascript"">" & JavaScriptString)
                    // Call Stream.Add(cr & "ButtonObjectCount = " & ButtonObjectCount & ";")
                    // Call Stream.Add(cr & "</script>")
                    (cr + ("ButtonObjectCount = " 
                                + (ButtonObjectCount + ";")));
                    cpCore.html.addScriptCode_body(JavaScriptString, "Admin Site");
                }
                
                result = (errorController.getDocExceptionHtmlList(cpCore) + Stream.Text);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
            return result;
        }
        
        // 
        // ========================================================================
        //    read the input request
        //        If RequestBlocked get adminContent.id, AdminAction, FormID
        //        and AdminForm are the only variables accessible before reading
        //        the upl collection
        // ========================================================================
        // 
        private void GetForm_LoadControl(ref Models.Complex.cdefModel adminContent, editRecordClass editRecord) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // Dim th as integer: th = profileLogAdminMethodEnter( "GetForm_LoadControl")
            // 
            string editorpreferences;
            int Pos;
            string SQL;
            string Key;
            string[] Parts;
            int Ptr;
            int Cnt;
            int fieldEditorFieldId;
            int fieldEditorAddonId;
            DataTable dt;
            bool editorOk;
            int CS;
            string[] QSSplit;
            int QSPointer;
            string[] NVSplit;
            string NameValue;
            int WCount;
            string FindTemp;
            int FieldCount;
            string StringTemp;
            string WhereClauseContent;
            string WherePairTemp;
            int Position;
            int Position2;
            string MethodName;
            string InputText;
            int Id;
            // dim buildversion As String
            DataTable dtTest;
            // 
            MethodName = "Admin.Method()";
            allowAdminTabs = genericController.EncodeBoolean(cpCore.userProperty.getText("AllowAdminTabs", "1"));
            if ((cpCore.docProperties.getText("tabs") != "")) {
                if ((cpCore.docProperties.getBoolean("tabs") != allowAdminTabs)) {
                    allowAdminTabs = !allowAdminTabs;
                    cpCore.userProperty.setProperty("AllowAdminTabs", allowAdminTabs.ToString);
                }
                
            }
            
            // 
            //  AdminContent init
            // 
            requestedContentId = cpCore.docProperties.getInteger("cid");
            if ((requestedContentId != 0)) {
                adminContent = Models.Complex.cdefModel.getCdef(cpCore, requestedContentId);
                if ((adminContent == null)) {
                    adminContent = new Models.Complex.cdefModel();
                    adminContent.Id = 0;
                    errorController.error_AddUserError(cpCore, ("There is no content with the requested id [" 
                                    + (requestedContentId + "]")));
                    requestedContentId = 0;
                }
                
            }
            
            if ((adminContent == null)) {
                adminContent = new Models.Complex.cdefModel();
            }
            
            // 
            //  determine user rights to this content
            // 
            UserAllowContentEdit = true;
            UserAllowContentAdd = true;
            UserAllowContentDelete = true;
            if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                if ((adminContent.Id > 0)) {
                    UserAllowContentEdit = userHasContentAccess(adminContent.Id);
                }
                
            }
            
            // 
            //  editRecord init
            // 
            requestedRecordId = cpCore.docProperties.getInteger("id");
            if ((UserAllowContentEdit 
                        && ((requestedRecordId != 0) 
                        && (adminContent.Id > 0)))) {
                // 
                //  set AdminContent to the content definition of the requested record
                // 
                CS = cpCore.db.csOpenRecord(adminContent.Name, requestedRecordId, ,, "ContentControlID");
                if (cpCore.db.csOk(CS)) {
                    editRecord.id = requestedRecordId;
                    adminContent.Id = cpCore.db.csGetInteger(CS, "ContentControlID");
                    if ((adminContent.Id <= 0)) {
                        adminContent.Id = requestedContentId;
                    }
                    else if ((adminContent.Id != requestedContentId)) {
                        adminContent = Models.Complex.cdefModel.getCdef(cpCore, adminContent.Id);
                    }
                    
                }
                
                cpCore.db.csClose(CS);
            }
            
            // 
            //  Other page control fields
            // 
            TitleExtension = cpCore.docProperties.getText(RequestNameTitleExtension);
            RecordTop = cpCore.docProperties.getInteger("RT");
            RecordsPerPage = cpCore.docProperties.getInteger("RS");
            if ((RecordsPerPage == 0)) {
                RecordsPerPage = RecordsPerPageDefault;
            }
            
            // 
            //  Read WherePairCount
            // 
            WherePairCount = 99;
            for (WCount = 0; (WCount <= 99); WCount++) {
                WherePair(0, WCount) = genericController.encodeText(cpCore.docProperties.getText(("WL" + WCount)));
                if ((WherePair(0, WCount) == "")) {
                    WherePairCount = WCount;
                    break;
                }
                else {
                    WherePair(1, WCount) = genericController.encodeText(cpCore.docProperties.getText(("WR" + WCount)));
                    cpCore.doc.addRefreshQueryString(("wl" + WCount), genericController.EncodeRequestVariable(WherePair(0, WCount)));
                    cpCore.doc.addRefreshQueryString(("wr" + WCount), genericController.EncodeRequestVariable(WherePair(1, WCount)));
                }
                
            }
            
            // 
            //  Read WhereClauseContent to WherePairCount
            // 
            WhereClauseContent = genericController.encodeText(cpCore.docProperties.getText("wc"));
            if ((WhereClauseContent != "")) {
                // 
                //  ***** really needs a server.URLDecode() function
                // 
                cpCore.doc.addRefreshQueryString("wc", WhereClauseContent);
                // WhereClauseContent = genericController.vbReplace(WhereClauseContent, "%3D", "=")
                // WhereClauseContent = genericController.vbReplace(WhereClauseContent, "%26", "&")
                if ((WhereClauseContent != "")) {
                    QSSplit = WhereClauseContent.Split(",");
                    for (QSPointer = 0; (QSPointer <= UBound(QSSplit)); QSPointer++) {
                        NameValue = QSSplit[QSPointer];
                        if ((NameValue != "")) {
                            if (((NameValue.Substring(0, 1) == "(") 
                                        && ((NameValue.Substring((NameValue.Length - 1)) == ")") 
                                        && (NameValue.Length > 2)))) {
                                NameValue = NameValue.Substring(1, (NameValue.Length - 2));
                            }
                            
                            NVSplit = NameValue.Split("=");
                            WherePair(0, WherePairCount) = NVSplit[0];
                            if ((UBound(NVSplit) > 0)) {
                                WherePair(1, WherePairCount) = NVSplit[1];
                            }
                            
                            WherePairCount = (WherePairCount + 1);
                        }
                        
                    }
                    
                }
                
            }
            
            // 
            //  --- If AdminMenuMode is not given locally, use the Members Preferences
            // 
            object MenuModeVariant;
            // 
            AdminMenuModeID = cpCore.docProperties.getInteger("mm");
            if ((AdminMenuModeID == 0)) {
                AdminMenuModeID = cpCore.doc.authContext.user.AdminMenuModeID;
            }
            
            if ((AdminMenuModeID == 0)) {
                AdminMenuModeID = AdminMenuModeLeft;
            }
            
            if ((cpCore.doc.authContext.user.AdminMenuModeID != AdminMenuModeID)) {
                cpCore.doc.authContext.user.AdminMenuModeID = AdminMenuModeID;
                cpCore.doc.authContext.user.save(cpCore);
            }
            
            //     '
            //     ' ----- FieldName
            //     '
            //     InputFieldName = cpCore.main_GetStreamText2(RequestNameFieldName)
            // 
            //  ----- Other
            // 
            AdminAction = cpCore.docProperties.getInteger(RequestNameAdminAction);
            AdminSourceForm = cpCore.docProperties.getInteger(RequestNameAdminSourceForm);
            AdminForm = cpCore.docProperties.getInteger(RequestNameAdminForm);
            AdminButton = cpCore.docProperties.getText(RequestNameButton);
            // 
            //  ----- Convert misc Deletes to just delete for later processing
            // 
            if (((AdminButton == ButtonDeleteEmail) 
                        || ((AdminButton == ButtonDeletePage) 
                        || ((AdminButton == ButtonDeletePerson) 
                        || (AdminButton == ButtonDeleteRecord))))) {
                AdminButton = ButtonDelete;
            }
            
            if ((AdminForm == AdminFormEdit)) {
                MenuDepth = 0;
            }
            else {
                MenuDepth = cpCore.docProperties.getInteger(RequestNameAdminDepth);
            }
            
            // 
            //  ----- convert fieldEditorPreference change to a refresh action
            // 
            if ((adminContent.Id != 0)) {
                fieldEditorPreference = cpCore.docProperties.getText("fieldEditorPreference");
                if ((fieldEditorPreference != "")) {
                    // 
                    //  Editor Preference change attempt. Set new preference and set this as a refresh
                    // 
                    AdminButton = "";
                    AdminAction = AdminActionEditRefresh;
                    AdminForm = AdminFormEdit;
                    Pos = genericController.vbInstr(1, fieldEditorPreference, ":");
                    if ((Pos > 0)) {
                        fieldEditorFieldId = genericController.EncodeInteger(fieldEditorPreference.Substring(0, (Pos - 1)));
                        fieldEditorAddonId = genericController.EncodeInteger(fieldEditorPreference.Substring(Pos));
                        if ((fieldEditorFieldId != 0)) {
                            editorOk = true;
                            SQL = ("select id from ccfields where (active<>0) and id=" + fieldEditorFieldId);
                            dtTest = cpCore.db.executeQuery(SQL);
                            if ((dtTest.Rows.Count == 0)) {
                                editorOk = false;
                            }
                            
                            // RS = cpCore.app.executeSql(SQL)
                            // If (not isdatatableok(rs)) Then
                            //     editorOk = False
                            // ElseIf rs.rows.count=0 Then
                            //     editorOk = False
                            // End If
                            // If (isDataTableOk(rs)) Then
                            //     If false Then
                            //         'RS.Close()
                            //     End If
                            //     'RS = Nothing
                            // End If
                            if ((editorOk 
                                        && (fieldEditorAddonId != 0))) {
                                SQL = ("select id from ccaggregatefunctions where (active<>0) and id=" + fieldEditorAddonId);
                                dtTest = cpCore.db.executeQuery(SQL);
                                if ((dtTest.Rows.Count == 0)) {
                                    editorOk = false;
                                }
                                
                                // RS = cpCore.app.executeSql(SQL)
                                // If (not isdatatableok(rs)) Then
                                //     editorOk = False
                                // ElseIf rs.rows.count=0 Then
                                //     editorOk = False
                                // End If
                                // If (isDataTableOk(rs)) Then
                                //     If false Then
                                //         'RS.Close()
                                //     End If
                                //     'RS = Nothing
                                // End If
                            }
                            
                            if (editorOk) {
                                Key = ("editorPreferencesForContent:" + adminContent.Id);
                                editorpreferences = cpCore.userProperty.getText(Key, "");
                                if ((editorpreferences != "")) {
                                    // 
                                    //  remove current preferences for this field
                                    // 
                                    Parts = ("," + editorpreferences).Split(("," 
                                                    + (fieldEditorFieldId.ToString() + ":")));
                                    Cnt = (UBound(Parts) + 1);
                                    if ((Cnt > 0)) {
                                        for (Ptr = 1; (Ptr 
                                                    <= (Cnt - 1)); Ptr++) {
                                            Pos = genericController.vbInstr(1, Parts[Ptr], ",");
                                            if ((Pos == 0)) {
                                                Parts[Ptr] = "";
                                            }
                                            else if ((Pos > 0)) {
                                                Parts[Ptr] = Parts[Ptr].Substring(Pos);
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                    editorpreferences = Join(Parts, "");
                                }
                                
                                editorpreferences = (editorpreferences + ("," 
                                            + (fieldEditorFieldId + (":" + fieldEditorAddonId))));
                                cpCore.userProperty.setProperty(Key, editorpreferences);
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            
            // 
            //  --- Spell Check
            // 
            //  BuildVersion = cpCore.app.GetSiteProperty("BuildVersion")
            if (true) {
                SpellCheckSupported = false;
                SpellCheckRequest = false;
                SpellCheckResponse = false;
                SpellCheckDictionaryFilename = "";
                SpellCheckIgnoreList = "";
            }
            else {
                
            }
            
            // 
            // ''Dim th as integer
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            handleLegacyClassError2("GetForm_LoadControl");
        }
    }
}