
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
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
using Contensive.Processor.Models.Domain;
using Contensive.Addons.Tools;
using static Contensive.Processor.AdminUIController;
//
namespace Contensive.Addons.AdminSite {
    public class GetHtmlBodyClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        // objects passed in - do not dispose, sets cp from argument For use In calls To other objects, Then core because cp cannot be used since that would be a circular depenancy
        //
        private CPClass cp; // local cp set in constructor
        private CoreController core; // core -- short term, this is the migration solution from a built-in tool, to an addon
        //
        //====================================================================================================
        /// <summary>
        /// REFACTOR - Constructor for addon instances. Until refactoring, calls into other methods must be constructed with (coreClass) variation.
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public GetHtmlBodyClass() : base() {
        }
        //
        //====================================================================================================
        /// <summary>
        /// get the admin content
        /// </summary>
        /// <param name="core"></param>
        public GetHtmlBodyClass(Contensive.Processor.CPClass cp) : base() {
            this.cp = cp;
            core = this.cp.core;
        }
        //
        //====================================================================================================
        /// <summary>
        /// addon method, deliver complete Html admin site
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string result = "";
            try {
                //
                // -- ok to cast cpbase to cp because they build from the same solution
                this.cp = (CPClass)cp;
                core = this.cp.core;
                //
                // -- log request
                string logContent = "Admin Site Request:"
                        + "\r\n" + DateTime.Now
                        + "\r\nmember.name:" + core.session.user.name
                        + "\r\nmember.id:" + core.session.user.id
                        + "\r\nvisit.id:" + core.session.visit.id
                        + "\r\nurl:" + core.webServer.requestUrl
                        + "\r\nurl source:" + core.webServer.requestUrlSource + "\r\n----------"
                        + "\r\nform post:";
                foreach (string key in core.docProperties.getKeyList()) {
                    docPropertiesClass docProperty = core.docProperties.getProperty(key);
                    if (docProperty.IsForm) {
                        logContent += "\r\n" + docProperty.NameValue;
                    }
                }
                if (!(core.webServer.requestFormBinaryHeader == null)) {
                    byte[] BinaryHeader = core.webServer.requestFormBinaryHeader;
                    string BinaryHeaderString = GenericController.byteArrayToString(BinaryHeader);
                    logContent += ""
                            + "\r\n----------"
                            + "\r\nbinary header:"
                            + "\r\n" + BinaryHeaderString + "\r\n";
                }
                LogController.addSiteActivity(core, logContent, core.session.user.id, core.session.user.OrganizationID);
                //
                if (!core.session.isAuthenticated) {
                    //
                    // --- must be authenticated to continue. Force a local login
                    result = core.addon.execute(addonGuidLoginPage, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                        errorContextMessage = "get Login Page for Html Body",
                        addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextPage
                    });
                } else if (!core.session.isAuthenticatedContentManager(core)) {
                    //
                    // --- member must have proper access to continue
                    result = ""
                        + "<p>You are attempting to enter an area which your account does not have access.</p>"
                        + "<ul class=\"ccList\">"
                        + "<li class=\"ccListItem\">To return to the public web site, use your back button, or <a href=\"" + "/" + "\">Click Here</A>."
                        + "<li class=\"ccListItem\">To login under a different account, <a href=\"/" + core.appConfig.adminRoute + "?method=logout\" rel=\"nofollow\">Click Here</A>"
                        + "<li class=\"ccListItem\">To have your account access changed to include this area, please contact the <a href=\"mailto:" + core.siteProperties.getText("EmailAdmin") + "\">system administrator</A>. "
                        + "\r</ul>"
                        + "";
                    result = ""
                        + core.html.getPanelHeader("Unauthorized Access")
                        + core.html.getPanel(result, "ccPanel", "ccPanelHilite", "ccPanelShadow", "400", 15);
                    result = ""
                        + "\r<div style=\"display:table;padding:100px 0 0 0;margin:0 auto;\">"
                        + GenericController.nop(result) + "\r</div>";
                    core.doc.setMetaContent(0, 0);
                    core.html.addTitle("Unauthorized Access", "adminSite");
                    result = HtmlController.div(result, "container-fluid ccBodyAdmin ccCon");
                } else {
                    //
                    // get admin content
                    result = getHtmlBody();
                    result = HtmlController.div(result, "container-fluid ccBodyAdmin ccCon");
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// return the html body for the admin site
        /// </summary>
        /// <param name="forceAdminContent"></param>
        /// <returns></returns>
        private string getHtmlBody(string forceAdminContent = "") {
            string result = "";
            try {
                // todo convert to jquery bind
                core.html.addScriptCode_onLoad("document.getElementsByTagName('BODY')[0].onclick = BodyOnClick;", "Contensive");
                core.doc.setMetaContent(0, 0);
                //
                // turn off chrome protection against submitting html content
                core.webServer.addResponseHeader("X-XSS-Protection", "0");
                //
                // check for member login, if logged in and no admin, lock out, Do CheckMember here because we need to know who is there to create proper blocked menu
                if (core.doc.continueProcessing) {
                    var adminContext = new AdminInfoDomainModel(core);
                    core.db.sqlCommandTimeout = 300;
                    adminContext.ButtonObjectCount = 0;
                    adminContext.JavaScriptString = "";
                    adminContext.ContentWatchLoaded = false;
                    //
                    if (string.Compare(core.siteProperties.dataBuildVersion, cp.Version) < 0) {
                        LogController.handleWarn(core, new ApplicationException("Application code version (" + cp.Version + ") is newer than Db version (" + core.siteProperties.dataBuildVersion + "). Upgrade site code."));
                    }
                    //
                    //// Get Requests, initialize adminContext.content and editRecord objects 
                    //contextConstructor(ref adminContext, ref adminContext.content, ref editRecord);
                    //
                    // Process SourceForm/Button into Action/Form, and process
                    if (adminContext.requestButton == ButtonCancelAll) {
                        adminContext.AdminForm = AdminFormRoot;
                    } else {
                        ProcessForms(adminContext);
                        ProcessActions(adminContext, core.siteProperties.useContentWatchLink);
                    }
                    //
                    // Normalize values to be needed
                    if (adminContext.editRecord.id != 0) {
                        core.workflow.ClearEditLock(adminContext.adminContent.name, adminContext.editRecord.id);
                    }
                    if (adminContext.AdminForm < 1) {
                        //
                        // No form was set, use default form
                        if (adminContext.adminContent.id <= 0) {
                            adminContext.AdminForm = AdminFormRoot;
                        } else {
                            adminContext.AdminForm = AdminFormIndex;
                        }
                    }
                    int addonId = core.docProperties.getInteger("addonid");
                    string AddonGuid = core.docProperties.getText("addonguid");
                    if (adminContext.AdminForm == AdminFormLegacyAddonManager) {
                        //
                        // patch out any old links to the legacy addon manager
                        adminContext.AdminForm = 0;
                        AddonGuid = addonGuidAddonManager;
                    }
                    //
                    //-------------------------------------------------------------------------------
                    // Edit form but not valid record case
                    // Put this here so we can display the error without being stuck displaying the edit form
                    // Putting the error on the edit form is confusing because there are fields to fill in
                    //-------------------------------------------------------------------------------
                    //
                    if (adminContext.AdminSourceForm == AdminFormEdit) {
                        if ((!(core.doc.debug_iUserError != "")) & ((adminContext.requestButton == ButtonOK) || (adminContext.requestButton == ButtonCancel) || (adminContext.requestButton == ButtonDelete))) {
                            string EditReferer = core.docProperties.getText("EditReferer");
                            string CurrentLink = GenericController.modifyLinkQuery(core.webServer.requestUrl, "editreferer", "", false);
                            CurrentLink = GenericController.vbLCase(CurrentLink);
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
                                adminContext.AdminForm = AdminFormIndex;
                            }
                        }
                        if (adminContext.BlockEditForm) {
                            adminContext.AdminForm = AdminFormIndex;
                        }
                    }
                    int HelpLevel = core.docProperties.getInteger("helplevel");
                    int HelpAddonID = core.docProperties.getInteger("helpaddonid");
                    int HelpCollectionID = core.docProperties.getInteger("helpcollectionid");
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
                    if (adminContext.adminContent.id != 0) {
                        core.doc.addRefreshQueryString("cid", GenericController.encodeText(adminContext.adminContent.id));
                    }
                    if (adminContext.editRecord.id != 0) {
                        core.doc.addRefreshQueryString("id", GenericController.encodeText(adminContext.editRecord.id));
                    }
                    if (adminContext.TitleExtension != "") {
                        core.doc.addRefreshQueryString(RequestNameTitleExtension, GenericController.encodeRequestVariable(adminContext.TitleExtension));
                    }
                    if (adminContext.RecordTop != 0) {
                        core.doc.addRefreshQueryString("rt", GenericController.encodeText(adminContext.RecordTop));
                    }
                    if (adminContext.RecordsPerPage != AdminInfoDomainModel.RecordsPerPageDefault) {
                        core.doc.addRefreshQueryString("rs", GenericController.encodeText(adminContext.RecordsPerPage));
                    }
                    if (adminContext.AdminForm != 0) {
                        core.doc.addRefreshQueryString(rnAdminForm, GenericController.encodeText(adminContext.AdminForm));
                    }
                    if (adminContext.ignore_legacyMenuDepth != 0) {
                        core.doc.addRefreshQueryString(RequestNameAdminDepth, GenericController.encodeText(adminContext.ignore_legacyMenuDepth));
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
                    string adminBody = "";
                    StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                    string AddonName = "";
                    if (!string.IsNullOrEmpty(forceAdminContent)) {
                        //
                        // Use content passed in as an argument
                        //
                        adminBody = forceAdminContent;
                    } else if (HelpAddonID != 0) {
                        //
                        // display Addon Help
                        //
                        core.doc.addRefreshQueryString("helpaddonid", HelpAddonID.ToString());
                        adminBody = GetAddonHelp(HelpAddonID, "");
                    } else if (HelpCollectionID != 0) {
                        //
                        // display Collection Help
                        //
                        core.doc.addRefreshQueryString("helpcollectionid", HelpCollectionID.ToString());
                        adminBody = GetCollectionHelp(HelpCollectionID, "");
                    } else if (adminContext.AdminForm != 0) {
                        //
                        // No content so far, try the forms
                        // todo - convert this to switch
                        if (adminContext.AdminForm == AdminFormBuilderCollection) {
                            adminBody = GetForm_BuildCollection();
                        } else if (adminContext.AdminForm == AdminFormSecurityControl) {
                            AddonGuid = AddonGuidPreferences;
                        } else if (adminContext.AdminForm == AdminFormMetaKeywordTool) {
                            adminBody = (new Contensive.Addons.Tools.MetakeywordToolClass()).Execute( cp ) as string;
                        } else if ((adminContext.AdminForm == AdminFormMobileBrowserControl) || (adminContext.AdminForm == AdminFormPageControl) || (adminContext.AdminForm == AdminFormEmailControl)) {
                            adminBody = core.addon.execute(AddonGuidPreferences, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorContextMessage = "get Preferences for Admin"
                            });
                        } else if (adminContext.AdminForm == AdminFormClearCache) {
                            adminBody = ToolClearCache.GetForm_ClearCache(core);
                        } else if (adminContext.AdminForm == AdminFormSpiderControl) {
                            adminBody = core.addon.execute(AddonModel.createByUniqueName(core, "Content Spider Control"), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorContextMessage = "get Content Spider Control for Admin"
                            });
                        } else if (adminContext.AdminForm == AdminFormResourceLibrary) {
                            adminBody = core.html.getResourceLibrary("", false, "", "", true);
                        } else if (adminContext.AdminForm == AdminFormQuickStats) {
                            adminBody = (GetForm_QuickStats());
                        } else if (adminContext.AdminForm == AdminFormIndex) {
                            adminBody = BodyIndexClass.get( core, adminContext, (adminContext.adminContent.tableName.ToLower() == "ccemail"));
                        } else if (adminContext.AdminForm == AdminFormEdit) {
                            adminBody = GetForm_Edit(adminContext);
                        } else if (adminContext.AdminForm == AdminFormClose) {
                            Stream.Add("<Script Language=\"JavaScript\" type=\"text/javascript\"> window.close(); </Script>");
                        } else if (adminContext.AdminForm == AdminFormContentChildTool) {
                            adminBody = (GetContentChildTool());
                        } else if (adminContext.AdminForm == AdminformPageContentMap) {
                            adminBody = (GetForm_PageContentMap());
                        } else if (adminContext.AdminForm == AdminformHousekeepingControl) {
                            adminBody = (GetForm_HouseKeepingControl());
                        } else if ((adminContext.AdminForm == AdminFormTools) || (adminContext.AdminForm >= 100 && adminContext.AdminForm <= 199)) {
                            legacyToolsClass Tools = new legacyToolsClass(core);
                            adminBody = Tools.getToolsList();
                        } else if (adminContext.AdminForm == AdminFormStyleEditor) {
                            adminBody = (admin_GetForm_StyleEditor());
                        } else if (adminContext.AdminForm == AdminFormDownloads) {
                            adminBody = (GetForm_Downloads());
                        } else if (adminContext.AdminForm == AdminformRSSControl) {
                            adminBody = core.webServer.redirect("?cid=" + CdefController.getContentId(core, "RSS Feeds"), "RSS Control page is not longer supported. RSS Feeds are controlled from the RSS feed records.");
                        } else if (adminContext.AdminForm == AdminFormImportWizard) {
                            adminBody = core.addon.execute(addonGuidImportWizard, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorContextMessage = "get Import Wizard for Admin"
                            });
                        } else if (adminContext.AdminForm == AdminFormCustomReports) {
                            adminBody = GetForm_CustomReports();
                        } else if (adminContext.AdminForm == AdminFormFormWizard) {
                            adminBody = core.addon.execute(addonGuidFormWizard, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorContextMessage = "get Form Wizard for Admin"
                            });
                        } else if (adminContext.AdminForm == AdminFormLegacyAddonManager) {
                            adminBody = AddonController.getAddonManager(core);
                        } else if (adminContext.AdminForm == AdminFormEditorConfig) {
                            adminBody = GetForm_EditConfig();
                        } else {
                            adminBody = "<p>The form requested is not supported</p>";
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
                            adminBody = AddonController.getAddonManager(core);
                        } else {
                            AddonModel addon = null;
                            string executeContextErrorCaption = "unknown";
                            if (addonId != 0) {
                                executeContextErrorCaption = " addon id:" + addonId + " for Admin";
                                core.doc.addRefreshQueryString("addonid", addonId.ToString());
                                addon = AddonModel.create(core, addonId);
                            } else if (!string.IsNullOrEmpty(AddonGuid)) {
                                executeContextErrorCaption = "addon guid:" + AddonGuid + " for Admin";
                                core.doc.addRefreshQueryString("addonguid", AddonGuid);
                                addon = AddonModel.create(core, AddonGuid);
                            } else if (!string.IsNullOrEmpty(AddonName)) {
                                executeContextErrorCaption = "addon name:" + AddonName + " for Admin";
                                core.doc.addRefreshQueryString("addonname", AddonName);
                                addon = AddonModel.createByUniqueName(core, AddonName);
                            }
                            if (addon != null) {
                                addonId = addon.id;
                                AddonName = addon.name;
                                string AddonHelpCopy = addon.help;
                                core.doc.addRefreshQueryString(RequestNameRunAddon, addonId.ToString());
                            }
                            string InstanceOptionString = core.userProperty.getText("Addon [" + AddonName + "] Options", "");
                            int DefaultWrapperID = -1;
                            adminBody = core.addon.execute(addon, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                instanceGuid = adminSiteInstanceId,
                                instanceArguments = GenericController.convertAddonArgumentstoDocPropertiesList(core, InstanceOptionString),
                                wrapperID = DefaultWrapperID,
                                errorContextMessage = executeContextErrorCaption
                            });
                            if (string.IsNullOrEmpty(adminBody)) {
                                //
                                // empty returned, display desktop
                                adminBody = GetForm_Root();
                            }

                        }
                    } else {
                        //
                        // nothing so far, display desktop
                        adminBody = GetForm_Root();
                    }
                    //
                    // include fancybox if it was needed
                    if (adminContext.includeFancyBox) {
                        core.addon.executeDependency(AddonModel.create(core, addonGuidjQueryFancyBox), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                            addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                            errorContextMessage = "adding fancybox dependency in Admin"
                        });
                        core.html.addScriptCode_onLoad(adminContext.fancyBoxHeadJS, "");
                    }
                    //
                    // add user errors
                    if (!string.IsNullOrEmpty(core.doc.debug_iUserError)) {
                        adminBody = HtmlController.div(ErrorController.getUserError(core), "ccAdminMsg") + adminBody;
                    }
                    Stream.Add(getAdminHeader(adminContext));
                    Stream.Add(adminBody);
                    Stream.Add(adminContext.adminFooter);
                    adminContext.JavaScriptString += "ButtonObjectCount = " + adminContext.ButtonObjectCount + ";";
                    core.html.addScriptCode(adminContext.JavaScriptString, "Admin Site");
                    result = Stream.Text;
                }
                if (core.session.user.Developer) {
                    result = ErrorController.getDocExceptionHtmlList(core) + result;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
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
                if (GenericController.vbInstr(1, "," + UsedIDString + ",", "," + HelpAddonID.ToString() + ",") == 0) {
                    CS = core.db.csOpenRecord(cnAddons, HelpAddonID);
                    if (core.db.csOk(CS)) {
                        FoundAddon = true;
                        AddonName = core.db.csGet(CS, "Name");
                        AddonHelpCopy = core.db.csGet(CS, "help");
                        AddonDateAdded = core.db.csGetDate(CS, "dateadded");
                        if (CdefController.isContentFieldSupported(core, cnAddons, "lastupdated")) {
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
                        IconImg = AddonController.getAddonIconImg("/" + core.appConfig.adminRoute, IconWidth, IconHeight, IconSprites, IconIsInline, "", IconFilename, core.appConfig.cdnFileUrl, AddonName, AddonName, "", 0);
                        helpLink = core.db.csGet(CS, "helpLink");
                    }
                    core.db.csClose(ref CS);
                    //
                    if (FoundAddon) {
                        //
                        // Included Addons
                        //
                        SQL = "select IncludedAddonID from ccAddonIncludeRules where AddonID=" + HelpAddonID;
                        CS = core.db.csOpenSql(SQL, "Default");
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
                LogController.handleError(core, ex);
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
                if (GenericController.vbInstr(1, "," + UsedIDString + ",", "," + HelpCollectionID.ToString() + ",") == 0) {
                    CS = core.db.csOpenRecord("Add-on Collections", HelpCollectionID);
                    if (core.db.csOk(CS)) {
                        Collectionname = core.db.csGet(CS, "Name");
                        CollectionHelpCopy = core.db.csGet(CS, "help");
                        CollectionDateAdded = core.db.csGetDate(CS, "dateadded");
                        if (CdefController.isContentFieldSupported(core, "Add-on Collections", "lastupdated")) {
                            CollectionLastUpdated = core.db.csGetDate(CS, "lastupdated");
                        }
                        if (CdefController.isContentFieldSupported(core, "Add-on Collections", "helplink")) {
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
                LogController.handleError(core, ex);
                throw;
            }
            return returnHelp;
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
                LogController.handleError(core, ex);
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
            return AdminUIController.getBody(core, Caption, ButtonListLeft, ButtonListRight, AllowAdd, AllowDelete, Description, ContentSummary, ContentPadding, Content);
        }
        ////
        ////========================================================================
        ///// <summary>
        ///// Print the index form, values and all creates a sql with leftjoins, and renames lookups as TableLookupxName where x is the TarGetFieldPtr of the field that is FieldTypeLookup
        ///// </summary>
        ///// <param name="adminContext.content"></param>
        ///// <param name="editRecord"></param>
        ///// <param name="IsEmailContent"></param>
        ///// <returns></returns>
        //private string getBody_indexForm(adminInfoDomainModel adminContext, bool IsEmailContent) {
        //    string result = "";
        //    try {
        //        //
        //        // --- make sure required fields are present
        //        StringBuilderLegacyController Stream = new StringBuilderLegacyController();
        //        if (adminContext.adminContent.id == 0) {
        //            //
        //            // Bad content id
        //            Stream.Add(GetForm_Error("This form requires a valid content definition, and one was not found for content ID [" + adminContext.adminContent.id + "].", "No content definition was specified [ContentID=0]. Please contact your application developer for more assistance."));
        //        } else if (string.IsNullOrEmpty(adminContext.adminContent.name)) {
        //            //
        //            // Bad content name
        //            Stream.Add(GetForm_Error("No content definition could be found for ContentID [" + adminContext.adminContent.id + "]. This could be a menu error. Please contact your application developer for more assistance.", "No content definition for ContentID [" + adminContext.adminContent.id + "] could be found."));
        //        } else if (adminContext.adminContent.tableName == "") {
        //            //
        //            // No tablename
        //            Stream.Add(GetForm_Error("The content definition [" + adminContext.adminContent.name + "] is not associated with a valid database table. Please contact your application developer for more assistance.", "Content [" + adminContext.adminContent.name + "] ContentTablename is empty."));
        //        } else if (adminContext.adminContent.fields.Count == 0) {
        //            //
        //            // No Fields
        //            Stream.Add(GetForm_Error("This content [" + adminContext.adminContent.name + "] cannot be accessed because it has no fields. Please contact your application developer for more assistance.", "Content [" + adminContext.adminContent.name + "] has no field records."));
        //        } else if (adminContext.adminContent.developerOnly & (!core.session.isAuthenticatedDeveloper(core))) {
        //            //
        //            // Developer Content and not developer
        //            Stream.Add(GetForm_Error("Access to this content [" + adminContext.adminContent.name + "] requires developer permissions. Please contact your application developer for more assistance.", "Content [" + adminContext.adminContent.name + "] has no field records."));
        //        } else {
        //            List<string> tmp = new List<string> { };
        //            DataSourceModel datasource = DataSourceModel.create(core, adminContext.adminContent.dataSourceId, ref tmp);
        //            //
        //            // get access rights
        //            bool allowCMEdit = false;
        //            bool allowCMAdd = false;
        //            bool allowCMDelete = false;
        //            core.session.getContentAccessRights(core, adminContext.adminContent.name, ref allowCMEdit, ref allowCMAdd, ref allowCMDelete);
        //            //
        //            // detemine which subform to disaply
        //            string Copy = "";
        //            int SubForm = core.docProperties.getInteger(RequestNameAdminSubForm);
        //            if (SubForm != 0) {
        //                switch (SubForm) {
        //                    case AdminFormIndex_SubFormExport:
        //                        Copy = BodyIndexExportClass.get( core, adminContext);
        //                        break;
        //                    case AdminFormIndex_SubFormSetColumns:
        //                        Copy = toolSetListColumnsClass.GetForm_Index_SetColumns(core, adminContext);
        //                        break;
        //                    case AdminFormIndex_SubFormAdvancedSearch:
        //                        Copy = BodyIndexAdvancedSearchClass.get( core, adminContext);
        //                        break;
        //                }
        //            }
        //            Stream.Add(Copy);
        //            if (string.IsNullOrEmpty(Copy)) {
        //                //
        //                // If subforms return empty, go to parent form
        //                //
        //                // -- Load Index page customizations
        //                IndexConfigClass IndexConfig = IndexConfigClass.get(core, adminContext);
        //                SetIndexSQL_ProcessIndexConfigRequests(adminContext, ref IndexConfig);
        //                setIndexSQL_SaveIndexConfig(core, IndexConfig);
        //                //
        //                // Get the SQL parts
        //                bool AllowAccessToContent = false;
        //                string ContentAccessLimitMessage = "";
        //                bool IsLimitedToSubContent = false;
        //                string sqlWhere = "";
        //                string sqlOrderBy = "";
        //                string sqlFieldList = "";
        //                string sqlFrom = "";
        //                Dictionary<string, bool> FieldUsedInColumns = new Dictionary<string, bool>(); // used to prevent select SQL from being sorted by a field that does not appear
        //                Dictionary<string, bool> IsLookupFieldValid = new Dictionary<string, bool>();
        //                SetIndexSQL(adminContext, IndexConfig, ref AllowAccessToContent, ref sqlFieldList, ref sqlFrom, ref sqlWhere, ref sqlOrderBy, ref IsLimitedToSubContent, ref ContentAccessLimitMessage, ref FieldUsedInColumns, IsLookupFieldValid);
        //                bool AllowAdd = adminContext.adminContent.allowAdd & (!IsLimitedToSubContent) && (allowCMAdd);
        //                bool AllowDelete = (adminContext.adminContent.allowDelete) && (allowCMDelete);
        //                if ((!allowCMEdit) || (!AllowAccessToContent)) {
        //                    //
        //                    // two conditions should be the same -- but not time to check - This user does not have access to this content
        //                    ErrorController.addUserError(core, "Your account does not have access to any records in '" + adminContext.adminContent.name + "'.");
        //                } else {
        //                    //
        //                    // Get the total record count
        //                    string SQL = "select count(" + adminContext.adminContent.tableName + ".ID) as cnt from " + sqlFrom;
        //                    if (!string.IsNullOrEmpty(sqlWhere)) {
        //                        SQL += " where " + sqlWhere;
        //                    }
        //                    int recordCnt = 0;
        //                    int CS = core.db.csOpenSql(SQL, datasource.name);
        //                    if (core.db.csOk(CS)) {
        //                        recordCnt = core.db.csGetInteger(CS, "cnt");
        //                    }
        //                    core.db.csClose(ref CS);
        //                    //
        //                    // Assumble the SQL
        //                    //
        //                    SQL = "select";
        //                    if (datasource.type != DataSourceTypeODBCMySQL) {
        //                        SQL += " Top " + (IndexConfig.RecordTop + IndexConfig.RecordsPerPage);
        //                    }
        //                    SQL += " " + sqlFieldList + " From " + sqlFrom;
        //                    if (!string.IsNullOrEmpty(sqlWhere)) {
        //                        SQL += " WHERE " + sqlWhere;
        //                    }
        //                    if (!string.IsNullOrEmpty(sqlOrderBy)) {
        //                        SQL += " Order By" + sqlOrderBy;
        //                    }
        //                    if (datasource.type == DataSourceTypeODBCMySQL) {
        //                        SQL += " Limit " + (IndexConfig.RecordTop + IndexConfig.RecordsPerPage);
        //                    }
        //                    //
        //                    // Refresh Query String
        //                    //
        //                    core.doc.addRefreshQueryString("tr", IndexConfig.RecordTop.ToString());
        //                    core.doc.addRefreshQueryString("asf", adminContext.AdminForm.ToString());
        //                    core.doc.addRefreshQueryString("cid", adminContext.adminContent.id.ToString());
        //                    core.doc.addRefreshQueryString(RequestNameTitleExtension, GenericController.encodeRequestVariable(adminContext.TitleExtension));
        //                    if (adminContext.WherePairCount > 0) {
        //                        for (int WhereCount = 0; WhereCount < adminContext.WherePairCount; WhereCount++) {
        //                            core.doc.addRefreshQueryString("wl" + WhereCount, adminContext.WherePair[0, WhereCount]);
        //                            core.doc.addRefreshQueryString("wr" + WhereCount, adminContext.WherePair[1, WhereCount]);
        //                        }
        //                    }
        //                    //
        //                    // ----- Filter Data Table
        //                    //
        //                    string IndexFilterContent = "";
        //                    string IndexFilterHead = "";
        //                    string IndexFilterJS = "";
        //                    //
        //                    // Filter Nav - if enabled, just add another cell to the row
        //                    if (core.visitProperty.getBoolean("IndexFilterOpen", false)) {
        //                        //
        //                        // Ajax Filter Open
        //                        //
        //                        IndexFilterHead = ""
        //                            + "\r\n<div class=\"ccHeaderCon\">"
        //                            + "\r\n<div id=\"IndexFilterHeCursorTypeEnum.ADOPENed\" class=\"opened\">"
        //                            + "\r<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>"
        //                            + "\r<td valign=Middle class=\"left\">Filters</td>"
        //                            + "\r<td valign=Middle class=\"right\"><a href=\"#\" onClick=\"CloseIndexFilter();return false\">" + iconClose + "</i></a></td>"
        //                            + "\r</tr></table>"
        //                            + "\r\n</div>"
        //                            + "\r\n<div id=\"IndexFilterHeadClosed\" class=\"closed\" style=\"display:none;\">"
        //                            + "\r<a href=\"#\" onClick=\"OpenIndexFilter();return false\">" + iconOpen + "</i></a>"
        //                            + "\r\n</div>"
        //                            + "\r\n</div>"
        //                            + "";
        //                        IndexFilterContent = ""
        //                            + "\r\n<div class=\"ccContentCon\">"
        //                            + "\r\n<div id=\"IndexFilterContentOpened\" class=\"opened\">" + getForm_IndexFilterContent(adminContext) + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"200\" height=\"1\" style=\"clear:both\"></div>"
        //                            + "\r\n<div id=\"IndexFilterContentClosed\" class=\"closed\" style=\"display:none;\">" + FilterClosedLabel + "</div>"
        //                            + "\r\n</div>";
        //                        IndexFilterJS = ""
        //                            + "\r\n<script Language=\"JavaScript\" type=\"text/javascript\">"
        //                            + "\r\nfunction CloseIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','none');SetDisplay('IndexFilterContentOpened','none');SetDisplay('IndexFilterHeadClosed','block');SetDisplay('IndexFilterContentClosed','block');cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxCloseIndexFilter + "','','')}"
        //                            + "\r\nfunction OpenIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','block');SetDisplay('IndexFilterContentOpened','block');SetDisplay('IndexFilterHeadClosed','none');SetDisplay('IndexFilterContentClosed','none');cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxOpenIndexFilter + "','','')}"
        //                            + "\r\n</script>";
        //                    } else {
        //                        //
        //                        // Ajax Filter Closed
        //                        //
        //                        IndexFilterHead = ""
        //                            + "\r\n<div class=\"ccHeaderCon\">"
        //                            + "\r\n<div id=\"IndexFilterHeCursorTypeEnum.ADOPENed\" class=\"opened\" style=\"display:none;\">"
        //                            + "\r<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>"
        //                            + "\r<td valign=Middle class=\"left\">Filter</td>"
        //                            + "\r<td valign=Middle class=\"right\"><a href=\"#\" onClick=\"CloseIndexFilter();return false\"><i title=\"close\" class=\"fa fa-remove\" style=\"color:#f00\"></i></a></td>"
        //                            + "\r</tr></table>"
        //                            + "\r\n</div>"
        //                            + "\r\n<div id=\"IndexFilterHeadClosed\" class=\"closed\">"
        //                            + "\r<a href=\"#\" onClick=\"OpenIndexFilter();return false\"><i title=\"open\" class=\"fa fa-angle-double-right\" style=\"color:#fff\"></i></a>"
        //                            + "\r\n</div>"
        //                            + "\r\n</div>"
        //                            + "";
        //                        IndexFilterContent = ""
        //                            + "\r\n<div class=\"ccContentCon\">"
        //                            + "\r\n<div id=\"IndexFilterContentOpened\" class=\"opened\" style=\"display:none;\"><div style=\"text-align:center;\"><img src=\"/ccLib/images/ajax-loader-small.gif\" width=16 height=16></div></div>"
        //                            + "\r\n<div id=\"IndexFilterContentClosed\" class=\"closed\">" + FilterClosedLabel + "</div>"
        //                            + "\r\n<div id=\"IndexFilterContentMinWidth\" style=\"display:none;\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"200\" height=\"1\" style=\"clear:both\"></div>"
        //                            + "\r\n</div>";
        //                        string AjaxQS = GenericController.modifyQueryString(core.doc.refreshQueryString, RequestNameAjaxFunction, AjaxOpenIndexFilterGetContent);
        //                        IndexFilterJS = ""
        //                            + "\r\n<script Language=\"JavaScript\" type=\"text/javascript\">"
        //                            + "\r\nvar IndexFilterPop=false;"
        //                            + "\r\nfunction CloseIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','none');SetDisplay('IndexFilterHeadClosed','block');SetDisplay('IndexFilterContentOpened','none');SetDisplay('IndexFilterContentMinWidth','none');SetDisplay('IndexFilterContentClosed','block');cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxCloseIndexFilter + "','','')}"
        //                            + "\r\nfunction OpenIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','block');SetDisplay('IndexFilterHeadClosed','none');SetDisplay('IndexFilterContentOpened','block');SetDisplay('IndexFilterContentMinWidth','block');SetDisplay('IndexFilterContentClosed','none');if(!IndexFilterPop){cj.ajax.qs('" + AjaxQS + "','','IndexFilterContentOpened');IndexFilterPop=true;}else{cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxOpenIndexFilter + "','','');}}"
        //                            + "\r\n</script>";
        //                    }
        //                    //
        //                    // Calculate total width
        //                    int ColumnWidthTotal = 0;
        //                    foreach (var column in IndexConfig.columns) {
        //                        if (column.Width < 1) {
        //                            column.Width = 1;
        //                        }
        //                        ColumnWidthTotal += column.Width;
        //                    }
        //                    string DataTable_HdrRow = "<tr>";
        //                    //
        //                    // Edit Column
        //                    DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"small ccAdminListCaption\">Edit</td>";
        //                    //
        //                    // Row Number Column
        //                    //DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"small ccAdminListCaption\">Row</td>";
        //                    //
        //                    // Delete Select Box Columns
        //                    if (!AllowDelete) {
        //                        DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"small ccAdminListCaption\"><input TYPE=CheckBox disabled=\"disabled\"></td>";
        //                    } else {
        //                        DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"small ccAdminListCaption\"><input TYPE=CheckBox OnClick=\"CheckInputs('DelCheck',this.checked);\"></td>";
        //                    }
        //                    //
        //                    // field columns
        //                    foreach (var column in IndexConfig.columns) {
        //                        //
        //                        // ----- print column headers - anchored so they sort columns
        //                        //
        //                        int ColumnWidth = encodeInteger((100 * column.Width) / (double)ColumnWidthTotal);
        //                        //fieldId = column.FieldId
        //                        string FieldName = column.Name;
        //                        //
        //                        //if this is a current sort ,add the reverse flag
        //                        //
        //                        string ButtonHref = "/" + core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormIndex + "&SetSortField=" + FieldName + "&RT=0&" + RequestNameTitleExtension + "=" + GenericController.encodeRequestVariable(adminContext.TitleExtension) + "&cid=" + adminContext.adminContent.id + "&ad=" + adminContext.ignore_legacyMenuDepth;
        //                        foreach (var sortKvp in IndexConfig.Sorts) {
        //                            IndexConfigSortClass sort = sortKvp.Value;

        //                        }
        //                        if (!IndexConfig.Sorts.ContainsKey(FieldName)) {
        //                            ButtonHref += "&SetSortDirection=1";
        //                        } else {
        //                            switch (IndexConfig.Sorts[FieldName].direction) {
        //                                case 1:
        //                                    ButtonHref += "&SetSortDirection=2";
        //                                    break;
        //                                case 2:
        //                                    ButtonHref += "&SetSortDirection=0";
        //                                    break;
        //                                default:
        //                                    break;
        //                            }
        //                        }
        //                        //
        //                        //----- column header includes WherePairCount
        //                        //
        //                        if (adminContext.WherePairCount > 0) {
        //                            for (int WhereCount = 0; WhereCount < adminContext.WherePairCount; WhereCount++) {
        //                                if (adminContext.WherePair[0, WhereCount] != "") {
        //                                    ButtonHref += "&wl" + WhereCount + "=" + GenericController.encodeRequestVariable(adminContext.WherePair[0, WhereCount]);
        //                                    ButtonHref += "&wr" + WhereCount + "=" + GenericController.encodeRequestVariable(adminContext.WherePair[1, WhereCount]);
        //                                }
        //                            }
        //                        }
        //                        string ButtonFace = adminContext.adminContent.fields[FieldName.ToLower()].caption;
        //                        ButtonFace = GenericController.vbReplace(ButtonFace, " ", "&nbsp;");
        //                        string SortTitle = "Sort A-Z";
        //                        //
        //                        if (IndexConfig.Sorts.ContainsKey(FieldName)) {
        //                            string sortSuffix = ((IndexConfig.Sorts.Count < 2) ? "" : IndexConfig.Sorts[FieldName].order.ToString()) ;
        //                            switch (IndexConfig.Sorts[FieldName].direction) {
        //                                case 1:
        //                                    ButtonFace = iconArrowDown + sortSuffix + "&nbsp;" + ButtonFace;
        //                                    SortTitle = "Sort Z-A";
        //                                    break;
        //                                case 2:
        //                                    ButtonFace = iconArrowUp + sortSuffix + "&nbsp;" + ButtonFace;
        //                                    SortTitle = "Remove Sort";
        //                                    break;
        //                            }
        //                        }
        //                        //ButtonObject = "Button" + ButtonObjectCount;
        //                        adminContext.ButtonObjectCount += 1;
        //                        DataTable_HdrRow += "<td width=\"" + ColumnWidth + "%\" valign=bottom align=left class=\"small ccAdminListCaption\">";
        //                        DataTable_HdrRow += ("<a title=\"" + SortTitle + "\" href=\"" + HtmlController.encodeHtml(ButtonHref) + "\" class=\"ccAdminListCaption\">" + ButtonFace + "</A>");
        //                        DataTable_HdrRow += ("</td>");
        //                    }
        //                    DataTable_HdrRow += ("</tr>");
        //                    //
        //                    //   select and print Records
        //                    //
        //                    string DataTable_DataRows = "";
        //                    string RowColor = "";
        //                    int RecordPointer = 0;
        //                    int RecordLast = 0;
        //                    CS = core.db.csOpenSql(SQL, datasource.name, IndexConfig.RecordsPerPage, IndexConfig.PageNumber);
        //                    if (core.db.csOk(CS)) {
        //                        RecordPointer = IndexConfig.RecordTop;
        //                        RecordLast = IndexConfig.RecordTop + IndexConfig.RecordsPerPage;
        //                        //
        //                        // --- Print out the records
        //                        while ((core.db.csOk(CS)) && (RecordPointer < RecordLast)) {
        //                            int RecordID = core.db.csGetInteger(CS, "ID");
        //                            //RecordName = core.db.csGetText(CS, "name");
        //                            //IsLandingPage = IsPageContent And (RecordID = LandingPageID)
        //                            if (RowColor == "class=\"ccAdminListRowOdd\"") {
        //                                RowColor = "class=\"ccAdminListRowEven\"";
        //                            } else {
        //                                RowColor = "class=\"ccAdminListRowOdd\"";
        //                            }
        //                            DataTable_DataRows += "\r\n<tr>";
        //                            //
        //                            // --- Edit button column
        //                            DataTable_DataRows += "<td align=center " + RowColor + ">";
        //                            string URI = "\\" + core.appConfig.adminRoute + "?" + rnAdminAction + "=" + adminInfoDomainModel.AdminActionNop + "&cid=" + adminContext.adminContent.id + "&id=" + RecordID + "&" + RequestNameTitleExtension + "=" + GenericController.encodeRequestVariable(adminContext.TitleExtension) + "&ad=" + adminContext.ignore_legacyMenuDepth + "&" + rnAdminSourceForm + "=" + adminContext.AdminForm + "&" + rnAdminForm + "=" + AdminFormEdit;
        //                            if (adminContext.WherePairCount > 0) {
        //                                for (int WhereCount = 0; WhereCount < adminContext.WherePairCount; WhereCount++) {
        //                                    URI = URI + "&wl" + WhereCount + "=" + GenericController.encodeRequestVariable(adminContext.WherePair[0, WhereCount]) + "&wr" + WhereCount + "=" + GenericController.encodeRequestVariable(adminContext.WherePair[1, WhereCount]);
        //                                }
        //                            }
        //                            DataTable_DataRows += AdminUIController.getIconEditLink(URI);
        //                            DataTable_DataRows += ("</td>");
        //                            //
        //                            // --- Record Number column
        //                            //DataTable_DataRows += "<td align=right " + RowColor + ">" + SpanClassAdminSmall + "[" + (RecordPointer + 1) + "]</span></td>";
        //                            //
        //                            // --- Delete Checkbox Columns
        //                            if (AllowDelete) {
        //                                DataTable_DataRows += "<td align=center " + RowColor + "><input TYPE=CheckBox NAME=row" + RecordPointer + " VALUE=1 ID=\"DelCheck\"><input type=hidden name=rowid" + RecordPointer + " VALUE=" + RecordID + "></span></td>";
        //                            } else {
        //                                DataTable_DataRows += "<td align=center " + RowColor + "><input TYPE=CheckBox disabled=\"disabled\" NAME=row" + RecordPointer + " VALUE=1><input type=hidden name=rowid" + RecordPointer + " VALUE=" + RecordID + "></span></td>";
        //                            }
        //                            //
        //                            // --- field columns
        //                            foreach (var column in IndexConfig.columns) {
        //                                string columnNameLc = column.Name.ToLower();
        //                                if (FieldUsedInColumns.ContainsKey(columnNameLc)) {
        //                                    if (FieldUsedInColumns[columnNameLc]) {
        //                                        DataTable_DataRows += ("\r\n<td valign=\"middle\" " + RowColor + " align=\"left\">" + SpanClassAdminNormal);
        //                                        DataTable_DataRows += GetForm_Index_GetCell(adminContext, column.Name, CS, IsLookupFieldValid[columnNameLc], GenericController.vbLCase(adminContext.adminContent.tableName) == "ccemail");
        //                                        DataTable_DataRows += ("&nbsp;</span></td>");
        //                                    }
        //                                }
        //                            }
        //                            DataTable_DataRows += ("\n    </tr>");
        //                            core.db.csGoNext(CS);
        //                            RecordPointer = RecordPointer + 1;
        //                        }
        //                        DataTable_DataRows += "<input type=hidden name=rowcnt value=" + RecordPointer + ">";
        //                        //
        //                        // --- print out the stuff at the bottom
        //                        //
        //                        int RecordTop_NextPage = IndexConfig.RecordTop;
        //                        if (core.db.csOk(CS)) {
        //                            RecordTop_NextPage = RecordPointer;
        //                        }
        //                        int RecordTop_PreviousPage = IndexConfig.RecordTop - IndexConfig.RecordsPerPage;
        //                        if (RecordTop_PreviousPage < 0) {
        //                            RecordTop_PreviousPage = 0;
        //                        }
        //                    }
        //                    core.db.csClose(ref CS);
        //                    //
        //                    // Header at bottom
        //                    //
        //                    if (RowColor == "class=\"ccAdminListRowOdd\"") {
        //                        RowColor = "class=\"ccAdminListRowEven\"";
        //                    } else {
        //                        RowColor = "class=\"ccAdminListRowOdd\"";
        //                    }
        //                    if (RecordPointer == 0) {
        //                        //
        //                        // No records found
        //                        //
        //                        DataTable_DataRows += ("<tr><td " + RowColor + " align=center>-</td><td " + RowColor + " align=center>-</td><td colspan=" + IndexConfig.columns.Count + " " + RowColor + " style=\"text-align:left ! important;\">no records were found</td></tr>");
        //                    } else {
        //                        if (RecordPointer < RecordLast) {
        //                            //
        //                            // End of list
        //                            //
        //                            DataTable_DataRows += ("<tr><td " + RowColor + " align=center>-</td><td " + RowColor + " align=center>-</td><td colspan=" + IndexConfig.columns.Count + " " + RowColor + " style=\"text-align:left ! important;\">----- end of list</td></tr>");
        //                        }
        //                        //
        //                        // Add another header to the data rows
        //                        //
        //                        DataTable_DataRows += DataTable_HdrRow;
        //                    }
        //                    //
        //                    // ----- DataTable_FindRow
        //                    //
        //                    string DataTable_FindRow = "<tr><td colspan=" + (2 + IndexConfig.columns.Count) + " style=\"background-color:black;height:1;\"></td></tr>";
        //                    DataTable_FindRow += "<tr>";
        //                    DataTable_FindRow += "<td valign=\"middle\" colspan=2 width=\"60\" class=\"ccPanel\" align=center style=\"vertical-align:middle;padding:8px;text-align:center ! important;\">";
        //                    DataTable_FindRow += "\r\n<script language=\"javascript\" type=\"text/javascript\">"
        //                        + "\r\nfunction KeyCheck(e){"
        //                        + "\r\n  var code = e.keyCode;"
        //                        + "\r\n  if(code==13){"
        //                        + "\r\n    document.getElementById('FindButton').focus();"
        //                        + "\r\n    document.getElementById('FindButton').click();"
        //                        + "\r\n    return false;"
        //                        + "\r\n  }"
        //                        + "\r\n} "
        //                        + "\r\n</script>";
        //                    DataTable_FindRow += AdminUIController.getButtonPrimary(ButtonFind) + "</td>";
        //                    int ColumnPointer = 0;
        //                    foreach (var column in IndexConfig.columns) {
        //                        int ColumnWidth = column.Width;
        //                        string FieldName = GenericController.vbLCase(column.Name);
        //                        string FindWordValue = "";
        //                        if (IndexConfig.FindWords.ContainsKey(FieldName)) {
        //                            var tempVar = IndexConfig.FindWords[FieldName];
        //                            if ((tempVar.MatchOption == FindWordMatchEnum.matchincludes) || (tempVar.MatchOption == FindWordMatchEnum.MatchEquals)) {
        //                                FindWordValue = tempVar.Value;
        //                            } else if (tempVar.MatchOption == FindWordMatchEnum.MatchTrue) {
        //                                FindWordValue = "true";
        //                            } else if (tempVar.MatchOption == FindWordMatchEnum.MatchFalse) {
        //                                FindWordValue = "false";
        //                            }
        //                        }
        //                        DataTable_FindRow += "\r\n<td valign=\"middle\" align=\"center\" class=\"ccPanel3DReverse\" style=\"padding:8px;\">"
        //                            + "<input type=hidden name=\"FindName" + ColumnPointer + "\" value=\"" + FieldName + "\">"
        //                            + "<input class=\"form-control\"  onkeypress=\"KeyCheck(event);\"  type=text id=\"F" + ColumnPointer + "\" name=\"FindValue" + ColumnPointer + "\" value=\"" + FindWordValue + "\" style=\"width:98%\">"
        //                            + "</td>";
        //                        ColumnPointer += 1;
        //                    }
        //                    DataTable_FindRow += "</tr>";
        //                    //
        //                    // Assemble DataTable
        //                    //
        //                    string grid = ""
        //                        + "<table ID=\"DataTable\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"Background-Color:white;\">"
        //                        + DataTable_HdrRow + DataTable_DataRows + DataTable_FindRow + "</table>";
        //                    //DataTable = BodyIndexAdvancedSearchClass.get( core, )
        //                    //
        //                    // Assemble DataFilterTable
        //                    //
        //                    //string filterCell = "";
        //                    //if (!string.IsNullOrEmpty(IndexFilterContent)) {
        //                    //    filterCell = "<td valign=top style=\"border-right:1px solid black;\" class=\"ccToolsCon\">" + IndexFilterJS + IndexFilterHead + IndexFilterContent + "</td>";
        //                    //    //FilterColumn = "<td valign=top class=""ccPanel3DReverse ccAdminEditBody"" style=""border-right:1px solid black;"">" & IndexFilterJS & IndexFilterHead & IndexFilterContent & "</td>"
        //                    //}
        //                    string formContent = ""
        //                        + "<table ID=\"DataFilterTable\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"Background-Color:white;\">"
        //                        + "<tr>"
        //                        + "<td valign=top style=\"border-right:1px solid black;\" class=\"ccToolsCon\">" + IndexFilterJS + IndexFilterHead + IndexFilterContent + "</td>"
        //                        + "<td width=\"99%\" valign=top>" + grid + "</td>"
        //                        + "</tr>"
        //                        + "</table>";
        //                    //
        //                    // ----- ButtonBar
        //                    //
        //                    string ButtonBar = AdminUIController.getForm_Index_ButtonBar(core, AllowAdd, AllowDelete, IndexConfig.PageNumber, IndexConfig.RecordsPerPage, recordCnt, adminContext.adminContent.name);
        //                    string titleRow = AdminUIController.getForm_Index_Header(core, IndexConfig, adminContext.adminContent, recordCnt, ContentAccessLimitMessage);
        //                    //
        //                    // Assemble LiveWindowTable
        //                    //
        //                    Stream.Add(ButtonBar);
        //                    Stream.Add(AdminUIController.getTitleBar(core, titleRow, ""));
        //                    Stream.Add(formContent);
        //                    Stream.Add(ButtonBar);
        //                    //Stream.Add(core.html.getPanel("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"1\", height=\"10\" >"));
        //                    Stream.Add(HtmlController.inputHidden(rnAdminSourceForm, AdminFormIndex));
        //                    Stream.Add(HtmlController.inputHidden("cid", adminContext.adminContent.id));
        //                    Stream.Add(HtmlController.inputHidden("indexGoToPage", ""));
        //                    Stream.Add(HtmlController.inputHidden("Columncnt", IndexConfig.columns.Count));
        //                    core.html.addTitle(adminContext.adminContent.name);
        //                }
        //            }
        //            //End If
        //            //
        //        }
        //        result = HtmlController.form(core, Stream.Text, "", "adminForm");
        //        //
        //    } catch (Exception ex) {
        //        LogController.handleError(core, ex);
        //        throw;
        //    }
        //    return result;
        //}
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
                    UcaseName = GenericController.vbUCase(Name);
                    foreach (XmlAttribute NodeAttribute in Node.Attributes) {
                        if (GenericController.vbUCase(NodeAttribute.Name) == UcaseName) {
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
                LogController.handleError(core, ex);
            }
            return tempGetXMLAttribute;
        }
        //
        //
        //========================================================================
        //   read the input request
        //       If RequestBlocked get adminContext.content.id, adminContextClass.AdminAction, FormID
        //       and adminContext.AdminForm are the only variables accessible before reading
        //       the upl collection
        //========================================================================
        //
        private void contextConstructor(ref AdminInfoDomainModel adminContext) {
            adminContext = new AdminInfoDomainModel(core);
            //try {
            //    if (adminContext == null) {
            //        adminContext = new adminContextClass();
            //    }
            //    //
            //    // Tab Control
            //    allowAdminTabs = genericController.encodeBoolean(core.userProperty.getText("AllowAdminTabs", "1"));
            //    if (core.docProperties.getText("tabs") != "") {
            //        if (core.docProperties.getBoolean("tabs") != allowAdminTabs) {
            //            allowAdminTabs = !allowAdminTabs;
            //            core.userProperty.setProperty("AllowAdminTabs", allowAdminTabs.ToString());
            //        }
            //    }
            //    //
            //    // adminContext.content init
            //    requestedContentId = core.docProperties.getInteger("cid");
            //    if (requestedContentId != 0) {
            //        adminContext.content = cdefModel.getCdef(core, requestedContentId);
            //        if (adminContext.content == null) {
            //            adminContext.content = new cdefModel();
            //            adminContext.content.id = 0;
            //            errorController.addUserError(core, "There is no content with the requested id [" + requestedContentId + "]");
            //            requestedContentId = 0;
            //        }
            //    }
            //    if (adminContext.content == null) {
            //        adminContext.content = new cdefModel();
            //    }
            //    //
            //    // determine user rights to this content
            //    UserAllowContentEdit = true;
            //    if (!core.session.isAuthenticatedAdmin(core)) {
            //        if (adminContext.content.id > 0) {
            //            UserAllowContentEdit = userHasContentAccess(adminContext.content.id);
            //        }
            //    }
            //    //
            //    // editRecord init
            //    //
            //    editRecord = new editRecordClass {
            //        Loaded = false
            //    };
            //    requestedRecordId = core.docProperties.getInteger("id");
            //    if ((UserAllowContentEdit) & (requestedRecordId != 0) && (adminContext.content.id > 0)) {
            //        //
            //        // set adminContext.content to the content definition of the requested record
            //        //
            //        int CS = core.db.csOpenRecord(adminContext.content.name, requestedRecordId, false, false, "ContentControlID");
            //        if (core.db.csOk(CS)) {
            //            editRecord.id = requestedRecordId;
            //            adminContext.content.id = core.db.csGetInteger(CS, "ContentControlID");
            //            if (adminContext.content.id <= 0) {
            //                adminContext.content.id = requestedContentId;
            //            } else if (adminContext.content.id != requestedContentId) {
            //                adminContext.content = cdefModel.getCdef(core, adminContext.content.id);
            //            }
            //        }
            //        core.db.csClose(ref CS);
            //    }
            //    //
            //    // Other page control fields
            //    //
            //    TitleExtension = core.docProperties.getText(RequestNameTitleExtension);
            //    RecordTop = core.docProperties.getInteger("RT");
            //    RecordsPerPage = core.docProperties.getInteger("RS");
            //    if (RecordsPerPage == 0) {
            //        RecordsPerPage = RecordsPerPageDefault;
            //    }
            //    //
            //    // Read WherePairCount
            //    //
            //    WherePairCount = 99;
            //    int WCount = 0;
            //    for (WCount = 0; WCount <= 99; WCount++) {
            //        WherePair[0, WCount] = genericController.encodeText(core.docProperties.getText("WL" + WCount));
            //        if (WherePair[0, WCount] == "") {
            //            WherePairCount = WCount;
            //            break;
            //        } else {
            //            WherePair[1, WCount] = genericController.encodeText(core.docProperties.getText("WR" + WCount));
            //            core.doc.addRefreshQueryString("wl" + WCount, genericController.encodeRequestVariable(WherePair[0, WCount]));
            //            core.doc.addRefreshQueryString("wr" + WCount, genericController.encodeRequestVariable(WherePair[1, WCount]));
            //        }
            //    }
            //    //
            //    // Read WhereClauseContent to WherePairCount
            //    //
            //    string WhereClauseContent = genericController.encodeText(core.docProperties.getText("wc"));
            //    if (!string.IsNullOrEmpty(WhereClauseContent)) {
            //        //
            //        // ***** really needs a server.URLDecode() function
            //        //
            //        core.doc.addRefreshQueryString("wc", WhereClauseContent);
            //        //WhereClauseContent = genericController.vbReplace(WhereClauseContent, "%3D", "=")
            //        //WhereClauseContent = genericController.vbReplace(WhereClauseContent, "%26", "&")
            //        if (!string.IsNullOrEmpty(WhereClauseContent)) {
            //            string[] QSSplit = WhereClauseContent.Split(',');
            //            int QSPointer = 0;
            //            for (QSPointer = 0; QSPointer <= QSSplit.GetUpperBound(0); QSPointer++) {
            //                string NameValue = QSSplit[QSPointer];
            //                if (!string.IsNullOrEmpty(NameValue)) {
            //                    if ((NameValue.Left(1) == "(") && (NameValue.Substring(NameValue.Length - 1) == ")") && (NameValue.Length > 2)) {
            //                        NameValue = NameValue.Substring(1, NameValue.Length - 2);
            //                    }
            //                    string[] NVSplit = NameValue.Split('=');
            //                    WherePair[0, WherePairCount] = NVSplit[0];
            //                    if (NVSplit.GetUpperBound(0) > 0) {
            //                        WherePair[1, WherePairCount] = NVSplit[1];
            //                    }
            //                    WherePairCount = WherePairCount + 1;
            //                }
            //            }
            //        }
            //    }
            //    //
            //    // --- If AdminMenuMode is not given locally, use the Members Preferences
            //    //
            //    AdminMenuModeID = core.docProperties.getInteger("mm");
            //    if (AdminMenuModeID == 0) {
            //        AdminMenuModeID = core.session.user.AdminMenuModeID;
            //    }
            //    if (AdminMenuModeID == 0) {
            //        AdminMenuModeID = AdminMenuModeLeft;
            //    }
            //    if (core.session.user.AdminMenuModeID != AdminMenuModeID) {
            //        core.session.user.AdminMenuModeID = AdminMenuModeID;
            //        core.session.user.save(core);
            //    }
            //    //    '
            //    //    ' ----- FieldName
            //    //    '
            //    //    InputFieldName = core.main_GetStreamText2(RequestNameFieldName)
            //    //
            //    // ----- Other
            //    //
            //    adminContextClass.AdminAction = core.docProperties.getInteger(rnAdminAction);
            //    AdminSourceForm = core.docProperties.getInteger(rnAdminSourceForm);
            //    adminContext.AdminForm = core.docProperties.getInteger(rnAdminForm);
            //    adminContext.requestButton = core.docProperties.getText(RequestNameButton);
            //    if (adminContext.AdminForm == AdminFormEdit) {
            //        ignore_legacyMenuDepth = 0;
            //    } else {
            //        ignore_legacyMenuDepth = core.docProperties.getInteger(RequestNameAdminDepth);
            //    }
            //    //
            //    // ----- convert fieldEditorPreference change to a refresh action
            //    //
            //    if (adminContext.content.id != 0) {
            //        fieldEditorPreference = core.docProperties.getText("fieldEditorPreference");
            //        if (fieldEditorPreference != "") {
            //            //
            //            // Editor Preference change attempt. Set new preference and set this as a refresh
            //            //
            //            adminContext.requestButton = "";
            //            adminContextClass.AdminAction = adminContextClass.AdminActionEditRefresh;
            //            adminContext.AdminForm = AdminFormEdit;
            //            int Pos = genericController.vbInstr(1, fieldEditorPreference, ":");
            //            if (Pos > 0) {
            //                int fieldEditorFieldId = genericController.encodeInteger(fieldEditorPreference.Left(Pos - 1));
            //                int fieldEditorAddonId = genericController.encodeInteger(fieldEditorPreference.Substring(Pos));
            //                if (fieldEditorFieldId != 0) {
            //                    bool editorOk = true;
            //                    string SQL = "select id from ccfields where (active<>0) and id=" + fieldEditorFieldId;
            //                    DataTable dtTest = core.db.executeQuery(SQL);
            //                    if (dtTest.Rows.Count == 0) {
            //                        editorOk = false;
            //                    }
            //                    //RS = core.app.executeSql(SQL)
            //                    //If (not isdatatableok(rs)) Then
            //                    //    editorOk = False
            //                    //ElseIf rs.rows.count=0 Then
            //                    //    editorOk = False
            //                    //End If
            //                    //If (isDataTableOk(rs)) Then
            //                    //    If false Then
            //                    //        'RS.Close()
            //                    //    End If
            //                    //    'RS = Nothing
            //                    //End If
            //                    if (editorOk && (fieldEditorAddonId != 0)) {
            //                        SQL = "select id from ccaggregatefunctions where (active<>0) and id=" + fieldEditorAddonId;
            //                        dtTest = core.db.executeQuery(SQL);
            //                        if (dtTest.Rows.Count == 0) {
            //                            editorOk = false;
            //                        }
            //                        //RS = core.app.executeSql(SQL)
            //                        //If (not isdatatableok(rs)) Then
            //                        //    editorOk = False
            //                        //ElseIf rs.rows.count=0 Then
            //                        //    editorOk = False
            //                        //End If
            //                        //If (isDataTableOk(rs)) Then
            //                        //    If false Then
            //                        //        'RS.Close()
            //                        //    End If
            //                        //    'RS = Nothing
            //                        //End If
            //                    }
            //                    if (editorOk) {
            //                        string Key = "editorPreferencesForContent:" + adminContext.content.id;
            //                        //
            //                        string editorpreferences = core.userProperty.getText(Key, "");
            //                        if (!string.IsNullOrEmpty(editorpreferences)) {
            //                            //
            //                            // remove current preferences for this field
            //                            //
            //                            string[] Parts = ("," + editorpreferences).Split(new[] { "," + fieldEditorFieldId.ToString() + ":" }, StringSplitOptions.None);
            //                            int Cnt = Parts.GetUpperBound(0) + 1;
            //                            if (Cnt > 0) {
            //                                int Ptr = 0;
            //                                for (Ptr = 1; Ptr < Cnt; Ptr++) {
            //                                    Pos = genericController.vbInstr(1, Parts[Ptr], ",");
            //                                    if (Pos == 0) {
            //                                        Parts[Ptr] = "";
            //                                    } else if (Pos > 0) {
            //                                        Parts[Ptr] = Parts[Ptr].Substring(Pos);
            //                                    }
            //                                }
            //                            }
            //                            editorpreferences = string.Join("", Parts);
            //                        }
            //                        editorpreferences = editorpreferences + "," + fieldEditorFieldId + ":" + fieldEditorAddonId;
            //                        core.userProperty.setProperty(Key, editorpreferences);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    //
            //    // --- Spell Check
            //    //SpellCheckSupported = false;
            //    //SpellCheckRequest = false;
            //} catch (Exception ex) {
            //    logController.handleError(core, ex);
            //}
            //return;
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
        //       adminContextClass.AdminAction = action to be performed, defined below, required except for very first call to edit
        //   adminContextClass.AdminAction Definitions
        //       edit - edit the record defined by ID, If ID="", edit a new record
        //       Save - saves an edit record and returns to the index
        //       Delete - hmmm.
        //       Cancel - returns to index
        //       Change Filex - uploads a file to a FieldTypeFile, x is a number 0...adminContext.content.FieldMax
        //       Delete Filex - clears a file name for a FieldTypeFile, x is a number 0...adminContext.content.FieldMax
        //       Upload - The action that actually uploads the file
        //       Email - (not done) Sends "body" field to "email" field in adminContext.content.id
        //========================================================================
        //
        private void ProcessActions(AdminInfoDomainModel adminContext, bool UseContentWatchLink) {
            try {
                int CS = 0;
                int RecordID = 0;
                string ContentName = null;
                int CSEditRecord = 0;
                int EmailToConfirmationMemberID = 0;
                int RowCnt = 0;
                int RowPtr = 0;
                //
                if (adminContext.Admin_Action != AdminInfoDomainModel.AdminActionNop) {
                    if (!adminContext.UserAllowContentEdit) {
                        //
                        // Action blocked by BlockCurrentRecord
                        //
                    } else {
                        //
                        // Process actions
                        //
                        switch (adminContext.Admin_Action) {
                            case AdminInfoDomainModel.AdminActionEditRefresh:
                                //
                                // Load the record as if it will be saved, but skip the save
                                //
                                LoadEditRecord(adminContext);
                                LoadEditRecord_Request(adminContext);
                                break;
                            case AdminInfoDomainModel.AdminActionMarkReviewed:
                                //
                                // Mark the record reviewed without making any changes
                                //
                                core.doc.markRecordReviewed(adminContext.adminContent.name, adminContext.editRecord.id);
                                break;
                            case AdminInfoDomainModel.AdminActionDelete:
                                if (adminContext.editRecord.Read_Only) {
                                    ErrorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    LoadEditRecord(adminContext);
                                    core.db.deprecate_argsreversed_deleteTableRecord(adminContext.adminContent.tableName, adminContext.editRecord.id, adminContext.adminContent.dataSourceName);
                                    core.doc.processAfterSave(true, adminContext.editRecord.contentControlId_Name, adminContext.editRecord.id, adminContext.editRecord.nameLc, adminContext.editRecord.parentID, UseContentWatchLink);
                                }
                                adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop;
                                break;
                            case AdminInfoDomainModel.AdminActionSave:
                                //
                                // ----- Save Record
                                //
                                if (adminContext.editRecord.Read_Only) {
                                    ErrorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    LoadEditRecord(adminContext);
                                    LoadEditRecord_Request(adminContext);
                                    ProcessActionSave(adminContext, UseContentWatchLink);
                                    core.doc.processAfterSave(false, adminContext.adminContent.name, adminContext.editRecord.id, adminContext.editRecord.nameLc, adminContext.editRecord.parentID, UseContentWatchLink);
                                }
                                adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop; // convert so action can be used in as a refresh
                                                                                              //
                                break;
                            case AdminInfoDomainModel.AdminActionSaveAddNew:
                                //
                                // ----- Save and add a new record
                                //
                                if (adminContext.editRecord.Read_Only) {
                                    ErrorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    LoadEditRecord(adminContext);
                                    LoadEditRecord_Request(adminContext);
                                    ProcessActionSave(adminContext, UseContentWatchLink);
                                    core.doc.processAfterSave(false, adminContext.adminContent.name, adminContext.editRecord.id, adminContext.editRecord.nameLc, adminContext.editRecord.parentID, UseContentWatchLink);
                                    adminContext.editRecord.id = 0;
                                    adminContext.editRecord.Loaded = false;
                                    //If adminContext.content.fields.Count > 0 Then
                                    //    ReDim EditRecordValuesObject(adminContext.content.fields.Count)
                                    //    ReDim EditRecordDbValues(adminContext.content.fields.Count)
                                    //End If
                                }
                                adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop; // convert so action can be used in as a refresh
                                                                                              //
                                break;
                            case AdminInfoDomainModel.AdminActionDuplicate:
                                //
                                // ----- Save Record
                                //
                                ProcessActionDuplicate(adminContext);
                                adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop;
                                break;
                            case AdminInfoDomainModel.AdminActionSendEmail:
                                //
                                // ----- Send (Group Email Only)
                                //
                                if (adminContext.editRecord.Read_Only) {
                                    ErrorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    LoadEditRecord(adminContext);
                                    LoadEditRecord_Request(adminContext);
                                    ProcessActionSave(adminContext, UseContentWatchLink);
                                    core.doc.processAfterSave(false, adminContext.adminContent.name, adminContext.editRecord.id, adminContext.editRecord.nameLc, adminContext.editRecord.parentID, UseContentWatchLink);
                                    if (!(core.doc.debug_iUserError != "")) {
                                        if (!CdefController.isWithinContent(core, adminContext.editRecord.contentControlId, CdefController.getContentId(core, "Group Email"))) {
                                            ErrorController.addUserError(core, "The send action only supports Group Email.");
                                        } else {
                                            CS = core.db.csOpenRecord("Group Email", adminContext.editRecord.id);
                                            if (!core.db.csOk(CS)) {
                                                //throw new ApplicationException("Unexpected exception"); // //throw new ApplicationException("Unexpected exception")' core.handleLegacyError23("Email ID [" &  adminContext.editRecord.id & "] could not be found in Group Email.")
                                            } else if (core.db.csGet(CS, "FromAddress") == "") {
                                                ErrorController.addUserError(core, "A 'From Address' is required before sending an email.");
                                            } else if (core.db.csGet(CS, "Subject") == "") {
                                                ErrorController.addUserError(core, "A 'Subject' is required before sending an email.");
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
                                adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop; // convert so action can be used in as a refresh
                                                                                              //
                                break;
                            case AdminInfoDomainModel.AdminActionDeactivateEmail:
                                //
                                // ----- Deactivate (Conditional Email Only)
                                //
                                if (adminContext.editRecord.Read_Only) {
                                    ErrorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    // no save, page was read only - Call ProcessActionSave
                                    LoadEditRecord(adminContext);
                                    if (!(core.doc.debug_iUserError != "")) {
                                        if (!CdefController.isWithinContent(core, adminContext.editRecord.contentControlId, CdefController.getContentId(core, "Conditional Email"))) {
                                            ErrorController.addUserError(core, "The deactivate action only supports Conditional Email.");
                                        } else {
                                            CS = core.db.csOpenRecord("Conditional Email", adminContext.editRecord.id);
                                            if (!core.db.csOk(CS)) {
                                                //throw new ApplicationException("Unexpected exception"); // //throw new ApplicationException("Unexpected exception")' core.handleLegacyError23("Email ID [" & editRecord.id & "] could not be opened.")
                                            } else {
                                                core.db.csSet(CS, "submitted", false);
                                            }
                                            core.db.csClose(ref CS);
                                        }
                                    }
                                }
                                adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop; // convert so action can be used in as a refresh
                                break;
                            case AdminInfoDomainModel.AdminActionActivateEmail:
                                //
                                // ----- Activate (Conditional Email Only)
                                //
                                if (adminContext.editRecord.Read_Only) {
                                    ErrorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    LoadEditRecord(adminContext);
                                    LoadEditRecord_Request(adminContext);
                                    ProcessActionSave(adminContext, UseContentWatchLink);
                                    core.doc.processAfterSave(false, adminContext.adminContent.name, adminContext.editRecord.id, adminContext.editRecord.nameLc, adminContext.editRecord.parentID, UseContentWatchLink);
                                    if (!(core.doc.debug_iUserError != "")) {
                                        if (!CdefController.isWithinContent(core, adminContext.editRecord.contentControlId, CdefController.getContentId(core, "Conditional Email"))) {
                                            ErrorController.addUserError(core, "The activate action only supports Conditional Email.");
                                        } else {
                                            CS = core.db.csOpenRecord("Conditional Email", adminContext.editRecord.id);
                                            if (!core.db.csOk(CS)) {
                                                //throw new ApplicationException("Unexpected exception"); // //throw new ApplicationException("Unexpected exception")' core.handleLegacyError23("Email ID [" & editRecord.id & "] could not be opened.")
                                            } else if (core.db.csGetInteger(CS, "ConditionID") == 0) {
                                                ErrorController.addUserError(core, "A condition must be set.");
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
                                adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop; // convert so action can be used in as a refresh
                                break;
                            case AdminInfoDomainModel.AdminActionSendEmailTest:
                                if (adminContext.editRecord.Read_Only) {
                                    ErrorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    //
                                    LoadEditRecord(adminContext);
                                    LoadEditRecord_Request(adminContext);
                                    ProcessActionSave(adminContext, UseContentWatchLink);
                                    core.doc.processAfterSave(false, adminContext.adminContent.name, adminContext.editRecord.id, adminContext.editRecord.nameLc, adminContext.editRecord.parentID, UseContentWatchLink);
                                    //
                                    if (!(core.doc.debug_iUserError != "")) {
                                        //
                                        EmailToConfirmationMemberID = 0;
                                        if (adminContext.editRecord.fieldsLc.ContainsKey("testmemberid")) {
                                            EmailToConfirmationMemberID = GenericController.encodeInteger(adminContext.editRecord.fieldsLc["testmemberid"].value);
                                            EmailController.queueConfirmationTestEmail(core, adminContext.editRecord.id, EmailToConfirmationMemberID);
                                            //
                                            if (adminContext.editRecord.fieldsLc.ContainsKey("lastsendtestdate")) {
                                                adminContext.editRecord.fieldsLc["lastsendtestdate"].value = core.doc.profileStartTime;
                                                core.db.executeQuery("update ccemail Set lastsendtestdate=" + core.db.encodeSQLDate(core.doc.profileStartTime) + " where id=" + adminContext.editRecord.id);
                                            }
                                        }
                                    }
                                }
                                adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop; // convert so action can be used in as a refresh
                                                                                              // end case
                                break;
                            case AdminInfoDomainModel.AdminActionDeleteRows:
                                //
                                // Delete Multiple Rows
                                //
                                RowCnt = core.docProperties.getInteger("rowcnt");
                                if (RowCnt > 0) {
                                    for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                        if (core.docProperties.getBoolean("row" + RowPtr)) {
                                            CSEditRecord = core.db.csOpen2(adminContext.adminContent.name, core.docProperties.getInteger("rowid" + RowPtr), true, true);
                                            if (core.db.csOk(CSEditRecord)) {
                                                RecordID = core.db.csGetInteger(CSEditRecord, "ID");
                                                core.db.csDeleteRecord(CSEditRecord);
                                                if (!false) {
                                                    //
                                                    // non-Workflow Delete
                                                    //
                                                    ContentName = CdefController.getContentNameByID(core, core.db.csGetInteger(CSEditRecord, "ContentControlID"));
                                                    core.cache.invalidateDbRecord(RecordID, adminContext.adminContent.tableName);
                                                    core.doc.processAfterSave(true, ContentName, RecordID, "", 0, UseContentWatchLink);
                                                }
                                                //
                                                // Page Content special cases
                                                //
                                                if (GenericController.vbLCase(adminContext.adminContent.tableName) == "ccpagecontent") {
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
                            case AdminInfoDomainModel.AdminActionReloadCDef:
                                //
                                // ccContent - save changes and reload content definitions
                                //
                                if (adminContext.editRecord.Read_Only) {
                                    ErrorController.addUserError(core, "Your request was blocked because the record you specified Is now locked by another authcontext.user.");
                                } else {
                                    LoadEditRecord(adminContext);
                                    LoadEditRecord_Request(adminContext);
                                    ProcessActionSave(adminContext, UseContentWatchLink);
                                    core.cache.invalidateAll();
                                    core.doc.clearMetaData();
                                }
                                adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop; // convert so action can be used in as a refresh
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
                ErrorController.addUserError(core, "There was an unknown error processing this page at " + core.doc.profileStartTime + ". Please try again, Or report this error To the site administrator.");
                LogController.handleError(core, ex);
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
                        core.db.csGoFirst(CSPointer);
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
                    GroupRuleModel.invalidateTableCache(core);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //========================================================================
        // LoadAndSaveGroupRules
        //   read groups from the edit form and modify Group Rules to match
        //========================================================================
        //
        private void LoadAndSaveGroupRules(EditRecordClass editRecord) {
            try {
                //
                if (editRecord.id != 0) {
                    LoadAndSaveGroupRules_ForContentAndChildren(editRecord.id, "");
                }
                //
                return;
                //
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                LogController.handleError(core, ex);
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
                        core.db.csGoFirst(CSPointer);
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
                    GroupRuleModel.invalidateTableCache(core);
                }
                return;
                //
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //========================================================================
        // Load Array
        //   Get defaults if no record ID
        //   Then load in any response elements
        //========================================================================
        //
        private void LoadEditRecord(AdminInfoDomainModel adminContext, bool CheckUserErrors = false) {
            try {
                // todo refactor out
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                if (string.IsNullOrEmpty(adminContext.adminContent.name)) {
                    //
                    // Can not load edit record because bad content definition
                    //
                    if (adminContext.adminContent.id == 0) {
                        throw (new Exception("The record can Not be edited because no content definition was specified."));
                    } else {
                        throw (new Exception("The record can Not be edited because a content definition For ID [" + adminContext.adminContent.id + "] was not found."));
                    }
                } else {
                    //
                    if (adminContext.editRecord.id == 0) {
                        //
                        // ----- New record, just load defaults
                        //
                        LoadEditRecord_Default(adminContext);
                        LoadEditRecord_WherePairs(adminContext);
                    } else {
                        //
                        // ----- Load the Live Record specified
                        //
                        LoadEditRecord_Dbase(adminContext, CheckUserErrors);
                        LoadEditRecord_WherePairs(adminContext);
                    }
                    //        '
                    //        ' ----- Test for a change of adminContext.content (the record is a child of adminContext.content )
                    //        '
                    //        If EditRecord.ContentID <> adminContext.content.Id Then
                    //            adminContext.content = core.app.getCdef(EditRecord.ContentName)
                    //        End If
                    //
                    // ----- Capture core fields needed for processing
                    //
                    adminContext.editRecord.menuHeadline = "";
                    if (adminContext.editRecord.fieldsLc.ContainsKey("menuheadline")) {
                        adminContext.editRecord.menuHeadline = GenericController.encodeText(adminContext.editRecord.fieldsLc["menuheadline"].value);
                    }
                    //
                    //adminContext.editRecord.menuHeadline = "";
                    if (adminContext.editRecord.fieldsLc.ContainsKey("name")) {
                        //Dim editRecordField As editRecordFieldClass = editRecord.fieldsLc["name")
                        //editRecord.nameLc = editRecordField.value.ToString()
                        adminContext.editRecord.nameLc = GenericController.encodeText(adminContext.editRecord.fieldsLc["name"].value);
                    }
                    //
                    //adminContext.editRecord.menuHeadline = "";
                    if (adminContext.editRecord.fieldsLc.ContainsKey("active")) {
                        adminContext.editRecord.active = GenericController.encodeBoolean(adminContext.editRecord.fieldsLc["active"].value);
                    }
                    //
                    //adminContext.editRecord.menuHeadline = "";
                    if (adminContext.editRecord.fieldsLc.ContainsKey("contentcontrolid")) {
                        adminContext.editRecord.contentControlId = GenericController.encodeInteger(adminContext.editRecord.fieldsLc["contentcontrolid"].value);
                    }
                    //
                    //adminContext.editRecord.menuHeadline = "";
                    if (adminContext.editRecord.fieldsLc.ContainsKey("parentid")) {
                        adminContext.editRecord.parentID = GenericController.encodeInteger(adminContext.editRecord.fieldsLc["parentid"].value);
                    }
                    //
                    //adminContext.editRecord.menuHeadline = "";
                    //if (adminContext.editRecord.fieldsLc.ContainsKey("rootpageid")) {
                    //    adminContext.editRecord.RootPageID = genericController.encodeInteger(adminContext.editRecord.fieldsLc["rootpageid"].value);
                    //}
                    //
                    // ----- Set the local global copy of Edit Record Locks
                    //
                    core.doc.getAuthoringStatus(adminContext.adminContent.name, adminContext.editRecord.id, ref adminContext.editRecord.SubmitLock, ref editRecord.ApproveLock, ref adminContext.editRecord.SubmittedName, ref adminContext.editRecord.ApprovedName, ref editRecord.IsInserted, ref editRecord.IsDeleted, ref editRecord.IsModified, ref editRecord.LockModifiedName, ref editRecord.LockModifiedDate, ref editRecord.SubmittedDate, ref editRecord.ApprovedDate);
                    //
                    // ----- Set flags used to determine the Authoring State
                    //
                    core.doc.getAuthoringPermissions(adminContext.adminContent.name, editRecord.id, ref editRecord.AllowInsert, ref editRecord.AllowCancel, ref editRecord.AllowSave, ref editRecord.AllowDelete, ref editRecord.AllowPublish, ref editRecord.AllowAbort, ref editRecord.AllowSubmit, ref editRecord.AllowApprove, ref editRecord.Read_Only);
                    //
                    // ----- Set Edit Lock
                    //
                    if (editRecord.id != 0) {
                        editRecord.EditLock = core.workflow.GetEditLockStatus(adminContext.adminContent.name, editRecord.id);
                        if (editRecord.EditLock) {
                            editRecord.EditLockMemberName = core.workflow.GetEditLockMemberName(adminContext.adminContent.name, editRecord.id);
                            editRecord.EditLockExpires = core.workflow.GetEditLockDateExpires(adminContext.adminContent.name, editRecord.id);
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
                    if (GenericController.vbUCase(adminContext.adminContent.tableName) == GenericController.vbUCase("ccMembers")) {
                        if (!core.session.isAuthenticatedDeveloper(core)) {
                            if (editRecord.fieldsLc.ContainsKey("developer")) {
                                if (GenericController.encodeBoolean(editRecord.fieldsLc["developer"].value)) {
                                    editRecord.Read_Only = true;
                                    ErrorController.addUserError(core, "You Do Not have access rights To edit this record.");
                                    adminContext.BlockEditForm = true;
                                }
                            }
                        }
                    }
                    //
                    // ----- Now make sure this record is locked from anyone else
                    //
                    if (!(editRecord.Read_Only)) {
                        core.workflow.SetEditLock(adminContext.adminContent.name, editRecord.id);
                    }
                    editRecord.Loaded = true;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //========================================================================
        //   Load both Live and Edit Record values from definition defaults
        //========================================================================
        //
        private void LoadEditRecord_Default(AdminInfoDomainModel adminContext) {
            try {
                // todo
                AdminUIController.EditRecordClass editrecord = adminContext.editRecord;
                //
                string DefaultValueText = null;
                string LookupContentName = null;
                string UCaseDefaultValueText = null;
                string[] lookups = null;
                int Ptr = 0;
                string defaultValue = null;
                EditRecordFieldClass editRecordField = null;
                CDefFieldModel field = null;
                editrecord.active = true;
                editrecord.contentControlId = adminContext.adminContent.id;
                editrecord.contentControlId_Name = adminContext.adminContent.name;
                editrecord.EditLock = false;
                editrecord.Loaded = false;
                editrecord.Saved = false;
                foreach (var keyValuePair in adminContext.adminContent.fields) {
                    field = keyValuePair.Value;
                    if (!(editrecord.fieldsLc.ContainsKey(field.nameLc))) {
                        editRecordField = new EditRecordFieldClass();
                        editrecord.fieldsLc.Add(field.nameLc, editRecordField);
                    }
                    defaultValue = field.defaultValue;
                    //    End If
                    if (field.active & !GenericController.IsNull(defaultValue)) {
                        switch (field.fieldTypeId) {
                            case FieldTypeIdInteger:
                            case FieldTypeIdAutoIdIncrement:
                            case FieldTypeIdMemberSelect:
                                //
                                editrecord.fieldsLc[field.nameLc].value = GenericController.encodeInteger(defaultValue);
                                break;
                            case FieldTypeIdCurrency:
                            case FieldTypeIdFloat:
                                //
                                editrecord.fieldsLc[field.nameLc].value = GenericController.encodeNumber(defaultValue);
                                break;
                            case FieldTypeIdBoolean:
                                //
                                editrecord.fieldsLc[field.nameLc].value = GenericController.encodeBoolean(defaultValue);
                                break;
                            case FieldTypeIdDate:
                                //
                                editrecord.fieldsLc[field.nameLc].value = GenericController.encodeDate(defaultValue);
                                break;
                            case FieldTypeIdLookup:

                                DefaultValueText = GenericController.encodeText(field.defaultValue);
                                if (!string.IsNullOrEmpty(DefaultValueText)) {
                                    if (DefaultValueText.IsNumeric()) {
                                        editrecord.fieldsLc[field.nameLc].value = DefaultValueText;
                                    } else {
                                        if (field.lookupContentID != 0) {
                                            LookupContentName = CdefController.getContentNameByID(core, field.lookupContentID);
                                            if (!string.IsNullOrEmpty(LookupContentName)) {
                                                editrecord.fieldsLc[field.nameLc].value = core.db.getRecordID(LookupContentName, DefaultValueText);
                                            }
                                        } else if (field.lookupList != "") {
                                            UCaseDefaultValueText = GenericController.vbUCase(DefaultValueText);
                                            lookups = field.lookupList.Split(',');
                                            for (Ptr = 0; Ptr <= lookups.GetUpperBound(0); Ptr++) {
                                                if (UCaseDefaultValueText == GenericController.vbUCase(lookups[Ptr])) {
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
                                editrecord.fieldsLc[field.nameLc].value = GenericController.encodeText(defaultValue);
                                break;
                        }
                    }
                    //
                    // process reserved fields (set defaults just makes it look good)
                    // (also, this presets readonly/devonly/adminonly fields not set to member)
                    //
                    switch (GenericController.vbUCase(field.nameLc)) {
                        //Case "ID"
                        //    .readonlyfield = True
                        //    .Required = False
                        case "MODIFIEDBY":
                            editrecord.fieldsLc[field.nameLc].value = core.session.user.id;
                            //    .readonlyfield = True
                            //    .Required = False
                            break;
                        case "CREATEDBY":
                            editrecord.fieldsLc[field.nameLc].value = core.session.user.id;
                            //    .readonlyfield = True
                            //    .Required = False
                            //Case "DATEADDED"
                            //    .readonlyfield = True
                            //    .Required = False
                            break;
                        case "CONTENTCONTROLID":
                            editrecord.fieldsLc[field.nameLc].value = adminContext.adminContent.id;
                            //Case "SORTORDER"
                            // default to ID * 100, but must be done later
                            break;
                    }
                    editrecord.fieldsLc[field.nameLc].dbValue = editrecord.fieldsLc[field.nameLc].value;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        //   Load both Live and Edit Record values from definition defaults
        //========================================================================
        //
        private void LoadEditRecord_WherePairs(AdminInfoDomainModel adminContext) {
            try {
                // todo refactor out
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                string DefaultValueText = null;
                foreach (var keyValuePair in adminContext.adminContent.fields) {
                    CDefFieldModel field = keyValuePair.Value;
                    DefaultValueText = adminContext.GetWherePairValue(field.nameLc);
                    if (field.active & (!string.IsNullOrEmpty(DefaultValueText))) {
                        switch (field.fieldTypeId) {
                            case FieldTypeIdInteger:
                            case FieldTypeIdLookup:
                            case FieldTypeIdAutoIdIncrement:
                                //
                                editRecord.fieldsLc[field.nameLc].value = GenericController.encodeInteger(DefaultValueText);
                                break;
                            case FieldTypeIdCurrency:
                            case FieldTypeIdFloat:
                                //
                                editRecord.fieldsLc[field.nameLc].value = GenericController.encodeNumber(DefaultValueText);
                                break;
                            case FieldTypeIdBoolean:
                                //
                                editRecord.fieldsLc[field.nameLc].value = GenericController.encodeBoolean(DefaultValueText);
                                break;
                            case FieldTypeIdDate:
                                //
                                editRecord.fieldsLc[field.nameLc].value = GenericController.encodeDate(DefaultValueText);
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
                LogController.handleError(core, ex);
            }
        }
        //
        //========================================================================
        //   Load Records from the database
        //========================================================================
        //
        private void LoadEditRecord_Dbase(AdminInfoDomainModel adminContext, bool CheckUserErrors = false) {
            try {
                // todo
                AdminUIController.EditRecordClass editrecord = adminContext.editRecord;
                //
                //
                object DBValueVariant = null;
                int CSEditRecord = 0;
                object NullVariant = null;
                int CSPointer = 0;
                // todo refactor these out
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                CDefModel adminContent = adminContext.adminContent;
                //Dim WorkflowAuthoring As Boolean
                //
                // ----- test for content problem
                //
                if (editrecord.id == 0) {
                    //
                    // ----- Skip load, this is a new record
                    //
                } else if (adminContent.id == 0) {
                    //
                    // ----- Error: no content ID
                    //
                    adminContext.BlockEditForm = true;
                    ErrorController.addUserError(core, "No content definition was found For Content ID [" + editrecord.id + "]. Please contact your application developer For more assistance.");
                    handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", "No content definition was found For Content ID [" + editrecord.id + "].");
                } else if (string.IsNullOrEmpty(adminContent.name)) {
                    //
                    // ----- Error: no content name
                    //
                    adminContext.BlockEditForm = true;
                    ErrorController.addUserError(core, "No content definition could be found For ContentID [" + adminContent.id + "]. This could be a menu Error. Please contact your application developer For more assistance.");
                    handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", "No content definition For ContentID [" + adminContent.id + "] could be found.");
                } else if (adminContent.tableName == "") {
                    //
                    // ----- Error: no content table
                    //
                    adminContext.BlockEditForm = true;
                    ErrorController.addUserError(core, "The content definition [" + adminContent.name + "] Is Not associated With a valid database table. Please contact your application developer For more assistance.");
                    handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", "No content definition For ContentID [" + adminContent.id + "] could be found.");
                    //
                    // move block to the edit and listing pages - to handle content editor cases - so they can edit 'pages', and just get the records they are allowed
                    //
                    //    ElseIf Not UserAllowContentEdit Then
                    //        '
                    //        ' ----- Error: load blocked by UserAllowContentEdit
                    //        '
                    //        BlockEditForm = True
                    //        Call core.htmldoc.main_AddUserError("Your account On this system does not have access rights To edit this content.")
                    //        Call HandleInternalError("AdminClass.LoadEditRecord_Dbase", "User does not have access To this content")
                } else if (adminContent.fields.Count == 0) {
                    //
                    // ----- Error: content definition is not complete
                    //
                    adminContext.BlockEditForm = true;
                    ErrorController.addUserError(core, "The content definition [" + adminContent.name + "] has no field records defined. Please contact your application developer For more assistance.");
                    handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", "Content [" + adminContent.name + "] has no fields defined.");
                } else {
                    //
                    //   Open Content Sets with the data
                    //
                    CSEditRecord = core.db.csOpen2(adminContent.name, editrecord.id, true, true);
                    //
                    //
                    // store fieldvalues in RecordValuesVariant
                    //
                    if (!(core.db.csOk(CSEditRecord))) {
                        //
                        //   Live or Edit records were not found
                        //
                        adminContext.BlockEditForm = true;
                        ErrorController.addUserError(core, "The information you have requested could not be found. The record could have been deleted, Or there may be a system Error.");
                        // removed because it was throwing too many false positives (1/14/04 - tried to do it again)
                        // If a CM hits the edit tag for a deleted record, this is hit. It should not cause the Developers to spend hours running down.
                        //Call HandleInternalError("AdminClass.LoadEditRecord_Dbase", "Content edit record For [" & adminContent.Name & "." & EditRecord.ID & "] was not found.")
                    } else {
                        //
                        // Read database values into RecordValuesVariant array
                        //
                        NullVariant = null;
                        foreach (var keyValuePair in adminContent.fields) {
                            CDefFieldModel adminContentcontent = keyValuePair.Value;
                            string fieldNameLc = adminContentcontent.nameLc;
                            EditRecordFieldClass editRecordField = null;
                            //
                            // set editRecord.field to editRecordField and set values
                            //
                            if (!editrecord.fieldsLc.ContainsKey(fieldNameLc)) {
                                editRecordField = new EditRecordFieldClass();
                                editrecord.fieldsLc.Add(fieldNameLc, editRecordField);
                            } else {
                                editRecordField = editrecord.fieldsLc[fieldNameLc];
                            }
                            //
                            // 1/21/2007 - added clause if required and null, set to default value
                            //
                            object fieldValue = NullVariant;
                            if (adminContentcontent.readOnly | adminContentcontent.notEditable) {
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
                            switch (adminContentcontent.fieldTypeId) {
                                case FieldTypeIdRedirect:
                                case FieldTypeIdManyToMany:
                                    DBValueVariant = "";
                                    break;
                                case FieldTypeIdFileText:
                                case FieldTypeIdFileCSS:
                                case FieldTypeIdFileXML:
                                case FieldTypeIdFileJavascript:
                                case FieldTypeIdFileHTML:
                                    DBValueVariant = core.db.csGet(CSPointer, adminContentcontent.nameLc);
                                    break;
                                default:
                                    DBValueVariant = core.db.csGetValue(CSPointer, adminContentcontent.nameLc);
                                    break;
                            }
                            //
                            // Check for required and null case loading error
                            //
                            if (CheckUserErrors && adminContentcontent.required & (GenericController.IsNull(DBValueVariant))) {
                                //
                                // if required and null
                                //
                                if (string.IsNullOrEmpty(adminContentcontent.defaultValue)) {
                                    //
                                    // default is null
                                    //
                                    if (adminContentcontent.editTabName == "") {
                                        ErrorController.addUserError(core, "The value for [" + adminContentcontent.caption + "] was empty but is required. This must be set before you can save this record.");
                                    } else {
                                        ErrorController.addUserError(core, "The value for [" + adminContentcontent.caption + "] in tab [" + adminContentcontent.editTabName + "] was empty but is required. This must be set before you can save this record.");
                                    }
                                } else {
                                    //
                                    // if required and null, set value to the default
                                    //
                                    DBValueVariant = adminContentcontent.defaultValue;
                                    if (adminContentcontent.editTabName == "") {
                                        ErrorController.addUserError(core, "The value for [" + adminContentcontent.caption + "] was null but is required. The default value Is shown, And will be saved if you save this record.");
                                    } else {
                                        ErrorController.addUserError(core, "The value for [" + adminContentcontent.caption + "] in tab [" + adminContentcontent.editTabName + "] was null but is required. The default value Is shown, And will be saved if you save this record.");
                                    }
                                }
                            }
                            //
                            // Save EditRecord values
                            //
                            switch (GenericController.vbUCase(adminContentcontent.nameLc)) {
                                case "DATEADDED":
                                    editrecord.dateAdded = core.db.csGetDate(CSEditRecord, adminContentcontent.nameLc);
                                    break;
                                case "MODIFIEDDATE":
                                    editrecord.modifiedDate = core.db.csGetDate(CSEditRecord, adminContentcontent.nameLc);
                                    break;
                                case "CREATEDBY":
                                    int createdByPersonId = core.db.csGetInteger(CSEditRecord, adminContentcontent.nameLc);
                                    if (createdByPersonId == 0) {
                                        editrecord.createdBy = new PersonModel() { name = "system" };
                                    } else {
                                        editrecord.createdBy = PersonModel.create(core, createdByPersonId);
                                        if (editrecord.createdBy == null) {
                                            editrecord.createdBy = new PersonModel() { name = "deleted #" + createdByPersonId.ToString() };
                                        }
                                    }
                                    break;
                                case "MODIFIEDBY":
                                    int modifiedByPersonId = core.db.csGetInteger(CSEditRecord, adminContentcontent.nameLc);
                                    if (modifiedByPersonId == 0) {
                                        editrecord.modifiedBy = new PersonModel() { name = "system" };
                                    } else {
                                        editrecord.modifiedBy = PersonModel.create(core, modifiedByPersonId);
                                        if (editrecord.modifiedBy == null) {
                                            editrecord.modifiedBy = new PersonModel() { name = "deleted #" + modifiedByPersonId.ToString() };
                                        }
                                    }
                                    break;
                                case "ACTIVE":
                                    editrecord.active = core.db.csGetBoolean(CSEditRecord, adminContentcontent.nameLc);
                                    break;
                                case "CONTENTCONTROLID":
                                    editrecord.contentControlId = core.db.csGetInteger(CSEditRecord, adminContentcontent.nameLc);
                                    if (editrecord.contentControlId.Equals(0)) {
                                        editrecord.contentControlId = adminContent.id;
                                    }
                                    editrecord.contentControlId_Name = CdefController.getContentNameByID(core, editrecord.contentControlId);
                                    break;
                                case "ID":
                                    editrecord.id = core.db.csGetInteger(CSEditRecord, adminContentcontent.nameLc);
                                    break;
                                case "MENUHEADLINE":
                                    editrecord.menuHeadline = core.db.csGetText(CSEditRecord, adminContentcontent.nameLc);
                                    break;
                                case "NAME":
                                    editrecord.nameLc = core.db.csGetText(CSEditRecord, adminContentcontent.nameLc);
                                    break;
                                case "PARENTID":
                                    editrecord.parentID = core.db.csGetInteger(CSEditRecord, adminContentcontent.nameLc);
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
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        //   Read the Form into the fields array
        //========================================================================
        //
        private void LoadEditRecord_Request(AdminInfoDomainModel adminContext) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
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
                    ErrorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [no field list].");
                } else if (AllowAdminFieldCheck() && (FormEmptyFieldLcList.Count == 0)) {
                    //
                    // The field list was not returned
                    ErrorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [no empty field list].");
                } else {
                    //
                    // fixup the string so it can be reduced by each field found, leaving and empty string if all correct
                    //
                    var tmpList = new List<string>();
                    DataSourceModel datasource = DataSourceModel.create(core, adminContext.adminContent.dataSourceId, ref tmpList);
                    //DataSourceName = core.db.getDataSourceNameByID(adminContext.content.dataSourceId)
                    foreach (var keyValuePair in adminContext.adminContent.fields) {
                        CDefFieldModel field = keyValuePair.Value;
                        LoadEditRecord_RequestField(adminContext, field, datasource.name, FormFieldLcListToBeLoaded, FormEmptyFieldLcList);
                    }
                    //
                    // If there are any form fields that were no loaded, flag the error now
                    //
                    if (AllowAdminFieldCheck() & (FormFieldLcListToBeLoaded.Count > 0)) {
                        ErrorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The following fields where Not found [" + string.Join(",", FormFieldLcListToBeLoaded) + "].");
                        throw (new ApplicationException("Unexpected exception")); // core.handleLegacyError2("AdminClass", "LoadEditResponse", core.appConfig.name & ", There were fields In the fieldlist sent out To the browser that did Not Return, [" & Mid(FormFieldListToBeLoaded, 2, Len(FormFieldListToBeLoaded) - 2) & "]")
                    } else {
                        //
                        // if page content, check for the 'pagenotfound','landingpageid' checkboxes in control tab
                        //
                        //if (genericController.vbLCase(adminContext.adminContent.contentTableName) == "ccpagecontent") {
                        //    ////
                        //    //PageNotFoundPageID = (core.siteProperties.getInteger("PageNotFoundPageID", 0));
                        //    //if (core.docProperties.getBoolean("PageNotFound")) {
                        //    //    editRecord.SetPageNotFoundPageID = true;
                        //    //} else if (editRecord.id == PageNotFoundPageID) {
                        //    //    core.siteProperties.setProperty("PageNotFoundPageID", "0");
                        //    //}
                        //    //
                        //    //if (core.docProperties.getBoolean("LandingPageID")) {
                        //    //    editRecord.SetLandingPageID = true;
                        //    //} else 
                        //    //if (editRecord.id == 0) {
                        //    //    //
                        //    //    // New record, allow it to be set, but do not compare it to LandingPageID
                        //    //    //
                        //    //} else if (editRecord.id == core.siteProperties.landingPageID) {
                        //    //    //
                        //    //    // Do not reset the LandingPageID from here -- set another instead
                        //    //    //
                        //    //    errorController.addUserError(core, "This page was marked As the Landing Page For the website, And the checkbox has been cleared. This Is Not allowed. To remove this page As the Landing Page, locate a New landing page And Select it, Or go To Settings &gt; Page Settings And Select a New Landing Page.");
                        //    //}
                        //}
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
        //    DataSourceName = core.db.getDataSourceNameByID(adminContext.content.DataSourceID)
        //    If (FieldName <> "") Then
        //        UcaseFieldName = genericController.vbUCase(FieldName)
        //        If adminContext.content.fields.count > 0 Then
        //            For FieldPointer = 0 To adminContext.content.fields.count - 1
        //                If genericController.vbUCase(adminContext.content.fields(FieldPointer).Name) = UcaseFieldName Then
        //                    Call LoadEditResponseByPointer(FormID, FieldPointer, DataSourceName)
        //                    FieldFound = True
        //                    Exit For
        //                    End If
        //                Next
        //            End If
        //        End If
        //    If Not FieldFound Then
        //        Call HandleInternalError("AdminClass.LoadEditResponseByName", "Field [" & FieldName & "] was not found In content [" & adminContext.content.Name & "]")
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
        private void LoadEditRecord_RequestField(AdminInfoDomainModel adminContext, CDefFieldModel field, string ignore, List<string> FormFieldLcListToBeLoaded, List<string> FormEmptyFieldLcList) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                if (field.active) {
                    const int LoopPtrMax = 100;
                    bool blockDuplicateUsername = false;
                    bool blockDuplicateEmail = false;
                    string lcaseCopy = null;
                    int EditorPixelHeight = 0;
                    int EditorRowHeight = 0;
                    int CSPointer = 0;
                    bool ResponseFieldIsEmpty = false;
                    HtmlParserController HTML = new HtmlParserController(core);
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
                        TabCopy = " in the " + field.editTabName + " tab";
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
                                        ErrorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try again, taking care Not To submit the page until your browser has finished loading. If this Error occurs again, please report this problem To your site administrator. The first Error was [" + field.nameLc + " Not found]. There may have been others.");
                                    }
                                    throw (new ApplicationException("Unexpected exception")); // core.handleLegacyError2("AdminClass", "LoadEditResponse", core.appConfig.name & ", Field [" & FieldName & "] was In the forms field list, but Not found In the response stream.")
                                }
                            }
                            if (GenericController.encodeInteger(ResponseFieldValueText) != GenericController.encodeInteger(editRecord.fieldsLc[field.nameLc].value)) {
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
                                ErrorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [" + field.nameLc + " Not found].");
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
                                    ErrorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [" + field.nameLc + " Not found].");
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
                            } else if ((field.adminOnly) & (!core.session.isAuthenticatedAdmin(core))) {
                                //
                                // non-admin and admin only field, leave current value
                                //
                                ResponseFieldValueIsOKToSave = false;
                            } else if ((field.developerOnly) & (!core.session.isAuthenticatedDeveloper(core))) {
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
                                string errorMessage = "There has been an error reading the response from your browser. The field[" + field.caption + "]" + TabCopy + " was missing. Please Try your change again. If this error happens repeatedly, please report this problem to your site administrator.";
                                ErrorController.addUserError(core, errorMessage);
                                LogController.handleError(core, new ApplicationException(errorMessage));
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
                                                ErrorController.addUserError(core, "The record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be a numeric value.");
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
                                                ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be a numeric value.");
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
                                                ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " had an invalid selection.");
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
                                            if (!DateController.IsDate(ResponseFieldValueText)) {
                                                ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be a date And/Or time in the form mm/dd/yy 0000 AM(PM).");
                                                ResponseFieldValueIsOKToSave = false;
                                            }
                                        }
                                        //End Case
                                        break;
                                    case FieldTypeIdBoolean:
                                        //
                                        // ----- translate to boolean
                                        //
                                        ResponseFieldValueText = GenericController.encodeBoolean(ResponseFieldValueText).ToString();
                                        break;
                                    case FieldTypeIdLink:
                                        //
                                        // ----- Link field - if it starts with 'www.', add the http:// automatically
                                        //
                                        ResponseFieldValueText = GenericController.encodeText(ResponseFieldValueText);
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
                                            core.userProperty.setProperty(adminContext.adminContent.name + "." + field.nameLc + ".RowHeight", EditorRowHeight);
                                        }
                                        EditorPixelHeight = core.docProperties.getInteger(field.nameLc + "PixelHeight");
                                        if (EditorPixelHeight != 0) {
                                            core.userProperty.setProperty(adminContext.adminContent.name + "." + field.nameLc + ".PixelHeight", EditorPixelHeight);
                                        }
                                        //
                                        if (!field.htmlContent) {
                                            lcaseCopy = GenericController.vbLCase(ResponseFieldValueText);
                                            lcaseCopy = GenericController.vbReplace(lcaseCopy, "\r", "");
                                            lcaseCopy = GenericController.vbReplace(lcaseCopy, "\n", "");
                                            lcaseCopy = lcaseCopy.Trim(' ');
                                            if ((lcaseCopy == HTMLEditorDefaultCopyNoCr) || (lcaseCopy == HTMLEditorDefaultCopyNoCr2)) {
                                                //
                                                // if the editor was left blank, remote the default copy
                                                //
                                                ResponseFieldValueText = "";
                                            } else {
                                                if (GenericController.vbInstr(1, ResponseFieldValueText, HTMLEditorDefaultCopyStartMark) != 0) {
                                                    //
                                                    // if the default copy was editing, remote the markers
                                                    //
                                                    ResponseFieldValueText = GenericController.vbReplace(ResponseFieldValueText, HTMLEditorDefaultCopyStartMark, "");
                                                    ResponseFieldValueText = GenericController.vbReplace(ResponseFieldValueText, HTMLEditorDefaultCopyEndMark, "");
                                                    //ResponseValueVariant = ResponseValueText
                                                }
                                                //
                                                // If the response is only white space, remove it
                                                // this is a fix for when Site Managers leave white space in the editor, and do not realize it
                                                //   then cannot fixgure out how to remove it
                                                //
                                                ResponseFieldValueText = ActiveContentController.processWysiwygResponseForSave(core, ResponseFieldValueText);
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
                                            core.userProperty.setProperty(adminContext.adminContent.name + "." + field.nameLc + ".RowHeight", EditorRowHeight);
                                        }
                                        EditorPixelHeight = core.docProperties.getInteger(field.nameLc + "PixelHeight");
                                        if (EditorPixelHeight != 0) {
                                            core.userProperty.setProperty(adminContext.adminContent.name + "." + field.nameLc + ".PixelHeight", EditorPixelHeight);
                                        }
                                        break;
                                }
                                if (field.nameLc == "parentid") {
                                    //
                                    // check circular reference on all parentid fields
                                    //

                                    ParentID = GenericController.encodeInteger(ResponseFieldValueText);
                                    LoopPtr = 0;
                                    UsedIDs = editRecord.id.ToString();
                                    while ((LoopPtr < LoopPtrMax) && (ParentID != 0) && (("," + UsedIDs + ",").IndexOf("," + ParentID.ToString() + ",") == -1)) {
                                        UsedIDs = UsedIDs + "," + ParentID.ToString();
                                        CS = core.db.csOpen(adminContext.adminContent.name, "ID=" + ParentID, "", true, 0, false, false, "ParentID");
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
                                        ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " creates a relationship between records that Is too large. Please limit the depth of this relationship to " + LoopPtrMax + " records.");
                                        ResponseFieldValueIsOKToSave = false;
                                    } else if ((editRecord.id != 0) && (editRecord.id == ParentID)) {
                                        //
                                        // Reference to iteslf
                                        //
                                        ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " contains a circular reference. This record points back to itself. This Is Not allowed.");
                                        ResponseFieldValueIsOKToSave = false;
                                    } else if (ParentID != 0) {
                                        //
                                        // Circular reference
                                        //
                                        ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " contains a circular reference. This field either points to other records which then point back to this record. This Is Not allowed.");
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
                                    ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " Is required but has no value.");
                                    ResponseFieldValueIsOKToSave = false;
                                }
                                //
                                // special case - people records without Allowduplicateusername require username to be unique
                                //
                                if (GenericController.vbLCase(adminContext.adminContent.tableName) == "ccmembers") {
                                    if (GenericController.vbLCase(field.nameLc) == "username") {
                                        blockDuplicateUsername = !(core.siteProperties.getBoolean("allowduplicateusername", false));
                                    }
                                    if (GenericController.vbLCase(field.nameLc) == "email") {
                                        blockDuplicateEmail = (core.siteProperties.getBoolean("allowemaillogin", false));
                                    }
                                }
                                if ((blockDuplicateUsername || blockDuplicateEmail || field.uniqueName) && (!ResponseFieldIsEmpty)) {
                                    //
                                    // ----- Do the unique check for this field
                                    //
                                    string SQLUnique = "select id from " + adminContext.adminContent.tableName + " where (" + field.nameLc + "=" + core.db.encodeSQL(ResponseFieldValueText, field.fieldTypeId) + ")and(" + CdefController.getContentControlCriteria(core, adminContext.adminContent.name) + ")";
                                    if (editRecord.id > 0) {
                                        //
                                        // --editing record
                                        SQLUnique = SQLUnique + "and(id<>" + editRecord.id + ")";
                                    }
                                    CSPointer = core.db.csOpenSql(SQLUnique,adminContext.adminContent.dataSourceName);
                                    if (core.db.csOk(CSPointer)) {
                                        //
                                        // field is not unique, skip it and flag error
                                        //
                                        if (blockDuplicateUsername) {
                                            //
                                            //
                                            //
                                            ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be unique and there Is another record with [" + ResponseFieldValueText + "]. This must be unique because the preference 'Allow Duplicate Usernames' is Not checked.");
                                        } else if (blockDuplicateEmail) {
                                            //
                                            //
                                            //
                                            ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be unique and there is another record with [" + ResponseFieldValueText + "]. This must be unique because the preference 'Allow Email Login' is checked.");
                                        } else if (false) {
                                        } else {
                                            //
                                            // non-workflow
                                            //
                                            ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be unique and there is another record with [" + ResponseFieldValueText + "].");
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
                LogController.handleError(core, ex);
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
        private void SaveContentTracking(AdminInfoDomainModel adminContext) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
                int ContentID = 0;
                int CSPointer = 0;
                int CSRules = 0;
                int CSContentWatch = 0;
                int ContentWatchID = 0;
                //
                if (adminContext.adminContent.allowContentTracking & (!editRecord.Read_Only)) {
                    //
                    // ----- Set default content watch link label
                    //
                    if ((adminContext.ContentWatchListIDCount > 0) && (adminContext.ContentWatchLinkLabel == "")) {
                        if (editRecord.menuHeadline != "") {
                            adminContext.ContentWatchLinkLabel = editRecord.menuHeadline;
                        } else if (editRecord.nameLc != "") {
                            adminContext.ContentWatchLinkLabel = editRecord.nameLc;
                        } else {
                            adminContext.ContentWatchLinkLabel = "Click Here";
                        }
                    }
                    // ----- update/create the content watch record for this content record
                    //
                    ContentID = (editRecord.contentControlId.Equals(0)) ? adminContext.adminContent.id : editRecord.contentControlId;
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
                        core.db.csSet(CSContentWatch, "LinkLabel", adminContext.ContentWatchLinkLabel);
                        core.db.csSet(CSContentWatch, "WhatsNewDateExpires", adminContext.ContentWatchExpires);
                        core.db.csSet(CSContentWatch, "Link", adminContext.ContentWatchLink);
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
                        if (adminContext.ContentWatchListIDCount > 0) {
                            for (ListPointer = 0; ListPointer < adminContext.ContentWatchListIDCount; ListPointer++) {
                                CSRules = core.db.csInsertRecord("Content Watch List Rules");
                                if (core.db.csOk(CSRules)) {
                                    core.db.csSet(CSRules, "ContentWatchID", ContentWatchID);
                                    core.db.csSet(CSRules, "ContentWatchListID", adminContext.ContentWatchListID[ListPointer]);
                                }
                                core.db.csClose(ref CSRules);
                            }
                        }
                    }
                    core.db.csClose(ref CSContentWatch);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //========================================================================
        //   Read in Whats New values if present
        //========================================================================
        //
        private void LoadContentTrackingResponse(AdminInfoDomainModel adminContext) {
            try {
                //
                int CSContentWatchList = 0;
                int RecordID = 0;
                //
                adminContext.ContentWatchListIDCount = 0;
                if ((core.docProperties.getText("WhatsNewResponse") != "") & (adminContext.adminContent.allowContentTracking)) {
                    //
                    // ----- set single fields
                    //
                    adminContext.ContentWatchLinkLabel = core.docProperties.getText("ContentWatchLinkLabel");
                    adminContext.ContentWatchExpires = core.docProperties.getDate("ContentWatchExpires");
                    //
                    // ----- Update ContentWatchListRules for all checked boxes
                    //
                    CSContentWatchList = core.db.csOpen("Content Watch Lists");
                    while (core.db.csOk(CSContentWatchList)) {
                        RecordID = (core.db.csGetInteger(CSContentWatchList, "ID"));
                        if (core.docProperties.getBoolean("ContentWatchList." + RecordID)) {
                            if (adminContext.ContentWatchListIDCount >= adminContext.ContentWatchListIDSize) {
                                adminContext.ContentWatchListIDSize = adminContext.ContentWatchListIDSize + 50;
                                Array.Resize(ref adminContext.ContentWatchListID, adminContext.ContentWatchListIDSize);
                            }
                            adminContext.ContentWatchListID[adminContext.ContentWatchListIDCount] = RecordID;
                            adminContext.ContentWatchListIDCount = adminContext.ContentWatchListIDCount + 1;
                        }
                        core.db.csGoNext(CSContentWatchList);
                    }
                    core.db.csClose(ref CSContentWatchList);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //========================================================================
        //   Save Link Alias field if it supported, and is non-authoring
        //   if it is authoring, it will be saved by the userfield routines
        //   if not, it appears in the LinkAlias tab, and must be saved here
        //========================================================================
        //
        private void SaveLinkAlias(AdminInfoDomainModel adminContext) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
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
                            core.db.executeQuery("update " + adminContext.adminContent.tableName + " set linkalias=null where ( linkalias=" + core.db.encodeSQLText(linkAlias) + ") and (id<>" + editRecord.id + ")");
                        } else {
                            int CS = core.db.csOpen(adminContext.adminContent.name, "( linkalias=" + core.db.encodeSQLText(linkAlias) + ")and(id<>" + editRecord.id + ")");
                            if (core.db.csOk(CS)) {
                                isDupError = true;
                                ErrorController.addUserError(core, "The Link Alias you entered can not be used because another record uses this value [" + linkAlias + "]. Enter a different Link Alias, or check the Override Duplicates checkbox in the Link Alias tab.");
                            }
                            core.db.csClose(ref CS);
                        }
                        if (!isDupError) {
                            DupCausesWarning = true;
                            int CS = core.db.csOpen2(adminContext.adminContent.name, editRecord.id, true, true);
                            if (core.db.csOk(CS)) {
                                core.db.csSet(CS, "linkalias", linkAlias);
                            }
                            core.db.csClose(ref CS);
                            //
                            // Update the Link Aliases
                            //
                            LinkAliasController.addLinkAlias(core, linkAlias, editRecord.id, "", OverRideDuplicate, DupCausesWarning);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //========================================================================
        //   Read in Whats New values if present
        //   Field values must be loaded
        //========================================================================
        //
        private void LoadContentTrackingDataBase(AdminInfoDomainModel adminInfo) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminInfo.editRecord;
                //
                int ContentID = 0;
                int CSPointer = 0;
                // converted array to dictionary - Dim FieldPointer As Integer
                //
                // ----- check if admin record is present
                //
                if ((editRecord.id != 0) & (adminInfo.adminContent.allowContentTracking)) {
                    //
                    // ----- Open the content watch record for this content record
                    //
                    ContentID = ((adminInfo.editRecord.contentControlId.Equals(0)) ? adminInfo.adminContent.id : adminInfo.editRecord.contentControlId);
                    CSPointer = core.db.csOpen("Content Watch", "(ContentID=" + core.db.encodeSQLNumber(ContentID) + ")AND(RecordID=" + core.db.encodeSQLNumber(editRecord.id) + ")");
                    if (core.db.csOk(CSPointer)) {
                        adminInfo.ContentWatchLoaded = true;
                        adminInfo.ContentWatchRecordID = (core.db.csGetInteger(CSPointer, "ID"));
                        adminInfo.ContentWatchLink = (core.db.csGet(CSPointer, "Link"));
                        adminInfo.ContentWatchClicks = (core.db.csGetInteger(CSPointer, "Clicks"));
                        adminInfo.ContentWatchLinkLabel = (core.db.csGet(CSPointer, "LinkLabel"));
                        adminInfo.ContentWatchExpires = (core.db.csGetDate(CSPointer, "WhatsNewDateExpires"));
                        core.db.csClose(ref CSPointer);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //========================================================================
        //
        private void SaveEditRecord(AdminInfoDomainModel adminInfo) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminInfo.editRecord;
                //
                int SaveCCIDValue = 0;
                int ActivityLogOrganizationID = -1;
                if (core.doc.debug_iUserError != "") {
                    //
                    // -- If There is an error, block the save
                    adminInfo.Admin_Action = AdminInfoDomainModel.AdminActionNop;
                } else if (!core.session.isAuthenticatedContentManager(core, adminInfo.adminContent.name)) {
                    //
                    // -- must be content manager
                } else if (editRecord.Read_Only) {
                    //
                    // -- read only block
                } else {
                    //
                    // -- Record will be saved, create a new one if this is an add
                    bool NewRecord = false;
                    bool recordChanged = false;
                    int csEditRecord = -1;
                    if (editRecord.id == 0) {
                        NewRecord = true;
                        recordChanged = true;
                        csEditRecord = core.db.csInsertRecord(adminInfo.adminContent.name);
                    } else {
                        NewRecord = false;
                        csEditRecord = core.db.csOpen2(adminInfo.adminContent.name, editRecord.id, true, true);
                    }
                    if (!core.db.csOk(csEditRecord)) {
                        //
                        // ----- Error: new record could not be created
                        //
                        if (NewRecord) {
                            //
                            // Could not insert record
                            //
                            LogController.handleError(core, new ApplicationException("A new record could not be inserted for content [" + adminInfo.adminContent.name + "]. Verify the Database table and field DateAdded, CreateKey, and ID."));
                        } else {
                            //
                            // Could not locate record you requested
                            //
                            LogController.handleError(core, new ApplicationException("The record you requested (ID=" + editRecord.id + ") could not be found for content [" + adminInfo.adminContent.name + "]"));
                        }
                    } else {
                        //
                        // ----- Get the ID of the current record
                        //
                        editRecord.id = core.db.csGetInteger(csEditRecord, "ID");
                        //
                        // ----- Create the update sql
                        //
                        bool fieldChanged = false;
                        foreach (var keyValuePair in adminInfo.adminContent.fields) {
                            CDefFieldModel field = keyValuePair.Value;
                            EditRecordFieldClass editRecordField = editRecord.fieldsLc[field.nameLc];
                            object fieldValueObject = editRecordField.value;
                            string FieldValueText = GenericController.encodeText(fieldValueObject);
                            string fieldName = field.nameLc;
                            string UcaseFieldName = GenericController.vbUCase(fieldName);
                            //
                            // ----- Handle special case fields
                            //
                            switch (UcaseFieldName) {
                                case "NAME": {
                                        //
                                        editRecord.nameLc = GenericController.encodeText(fieldValueObject);
                                        break;
                                    }
                                case "CCGUID": {
                                        if (NewRecord & string.IsNullOrEmpty(FieldValueText)) {
                                            //
                                            // if new record and edit form returns empty, preserve the guid used to create the record.
                                        } else {
                                            //
                                            // save the value in the request
                                            if (core.db.csGetText(csEditRecord, fieldName) != FieldValueText) {
                                                fieldChanged = true;
                                                recordChanged = true;
                                                core.db.csSet(csEditRecord, fieldName, FieldValueText);
                                            }
                                        }
                                        break;
                                    }
                                case "CONTENTCONTROLID": {
                                        //
                                        // run this after the save, so it will be blocked if the save fails
                                        // block the change from this save
                                        // Update the content control ID here, for all the children, and all the edit and archive records of both
                                        //
                                        int saveValue = GenericController.encodeInteger(fieldValueObject);
                                        if (editRecord.contentControlId != saveValue) {
                                            SaveCCIDValue = saveValue;
                                            recordChanged = true;
                                        }
                                        break;
                                    }
                                case "ACTIVE": {
                                        bool saveValue = GenericController.encodeBoolean(fieldValueObject);
                                        if (core.db.csGetBoolean(csEditRecord, fieldName) != saveValue) {
                                            fieldChanged = true;
                                            recordChanged = true;
                                            core.db.csSet(csEditRecord, fieldName, saveValue);
                                        }
                                        break;
                                    }
                                case "DATEEXPIRES": {
                                        //
                                        // ----- make sure content watch expires before content expires
                                        //
                                        if (!GenericController.IsNull(fieldValueObject)) {
                                            if (DateController.IsDate(fieldValueObject)) {
                                                DateTime saveValue = GenericController.encodeDate(fieldValueObject);
                                                if (adminInfo.ContentWatchExpires <= DateTime.MinValue) {
                                                    adminInfo.ContentWatchExpires = saveValue;
                                                } else if (adminInfo.ContentWatchExpires > saveValue) {
                                                    adminInfo.ContentWatchExpires = saveValue;
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
                                        if (!GenericController.IsNull(fieldValueObject)) {
                                            if (DateController.IsDate(fieldValueObject)) {
                                                DateTime saveValue = GenericController.encodeDate(fieldValueObject);
                                                if ((adminInfo.ContentWatchExpires) <= DateTime.MinValue) {
                                                    adminInfo.ContentWatchExpires = saveValue;
                                                } else if (adminInfo.ContentWatchExpires > saveValue) {
                                                    adminInfo.ContentWatchExpires = saveValue;
                                                }
                                            }
                                        }
                                        break;
                                    }
                            }
                            //
                            // ----- Put the field in the SQL to be saved
                            //
                            if (IsVisibleUserField(field.adminOnly, field.developerOnly, field.active, field.authorable, field.nameLc, adminInfo.adminContent.tableName) & (NewRecord || (!field.readOnly)) & (NewRecord || (!field.notEditable))) {
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
                                            if (core.docProperties.getBoolean(fieldName + ".DeleteFlag")) {
                                                recordChanged = true;
                                                fieldChanged = true;
                                                core.db.csSet(csEditRecord, fieldName, "");
                                            }
                                            string filename = GenericController.encodeText(fieldValueObject);
                                            if (!string.IsNullOrWhiteSpace(filename)) {
                                                filename = FileController.encodeDosFilename(filename);
                                                string unixPathFilename = core.db.csGetFieldFilename(csEditRecord, fieldName, filename, adminInfo.adminContent.name);
                                                string dosPathFilename = GenericController.convertToDosSlash(unixPathFilename);
                                                string dosPath = GenericController.getPath(dosPathFilename);
                                                core.cdnFiles.upload(fieldName, dosPath, ref filename);
                                                core.db.csSet(csEditRecord, fieldName, unixPathFilename);
                                                recordChanged = true;
                                                fieldChanged = true;
                                            }
                                            break;
                                        }
                                    case FieldTypeIdBoolean: {
                                            //
                                            // boolean
                                            //
                                            bool saveValue = GenericController.encodeBoolean(fieldValueObject);
                                            if (core.db.csGetBoolean(csEditRecord, fieldName) != saveValue) {
                                                recordChanged = true;
                                                fieldChanged = true;
                                                core.db.csSet(csEditRecord, fieldName, saveValue);
                                            }
                                            break;
                                        }
                                    case FieldTypeIdCurrency:
                                    case FieldTypeIdFloat: {
                                            //
                                            // Floating pointer numbers
                                            //
                                            double saveValue = GenericController.encodeNumber(fieldValueObject);
                                            if (core.db.csGetNumber(csEditRecord, fieldName) != saveValue) {
                                                recordChanged = true;
                                                fieldChanged = true;
                                                core.db.csSet(csEditRecord, fieldName, saveValue);
                                            }
                                            break;
                                        }
                                    case FieldTypeIdDate: {
                                            //
                                            // Date
                                            //
                                            DateTime saveValue = GenericController.encodeDate(fieldValueObject);
                                            if (core.db.csGetDate(csEditRecord, fieldName) != saveValue) {
                                                fieldChanged = true;
                                                recordChanged = true;
                                                core.db.csSet(csEditRecord, fieldName, saveValue);
                                            }
                                            break;
                                        }
                                    case FieldTypeIdInteger:
                                    case FieldTypeIdLookup: {
                                            //
                                            // Integers
                                            //
                                            int saveValue = GenericController.encodeInteger(fieldValueObject);
                                            if (saveValue != core.db.csGetInteger(csEditRecord, fieldName)) {
                                                fieldChanged = true;
                                                recordChanged = true;
                                                core.db.csSet(csEditRecord, fieldName, saveValue);
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
                                            string saveValue = GenericController.encodeText(fieldValueObject);
                                            if (core.db.csGet(csEditRecord, fieldName) != saveValue) {
                                                fieldChanged = true;
                                                recordChanged = true;
                                                core.db.csSet(csEditRecord, fieldName, saveValue);
                                            }
                                            break;
                                        }
                                    case FieldTypeIdManyToMany: {
                                            //
                                            // Many to Many checklist
                                            //
                                            //MTMContent0 = CdefController.getContentNameByID(core,.contentId)
                                            //MTMContent1 = CdefController.getContentNameByID(core,.manyToManyContentID)
                                            //MTMRuleContent = CdefController.getContentNameByID(core,.manyToManyRuleContentID)
                                            //MTMRuleField0 = .ManyToManyRulePrimaryField
                                            //MTMRuleField1 = .ManyToManyRuleSecondaryField
                                            core.html.processCheckList("field" + field.id, CdefController.getContentNameByID(core, field.contentId), encodeText(editRecord.id), CdefController.getContentNameByID(core, field.manyToManyContentID), CdefController.getContentNameByID(core, field.manyToManyRuleContentID), field.ManyToManyRulePrimaryField, field.ManyToManyRuleSecondaryField);
                                            break;
                                        }
                                    default: {
                                            //
                                            // Unknown other types
                                            //

                                            string saveValue = GenericController.encodeText(fieldValueObject);
                                            fieldChanged = true;
                                            recordChanged = true;
                                            core.db.csSet(csEditRecord, UcaseFieldName, saveValue);
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
                            if (fieldChanged) {
                                switch (GenericController.vbLCase(adminInfo.adminContent.tableName)) {
                                    case "cclibraryfiles":
                                        //
                                        if (core.docProperties.getText("filename") != "") {
                                            core.db.csSet(csEditRecord, "altsizelist", "");
                                        }
                                        break;
                                }
                                if (!NewRecord) {
                                    switch (GenericController.vbLCase(adminInfo.adminContent.tableName)) {
                                        case "ccmembers":
                                            //
                                            if (ActivityLogOrganizationID < 0) {
                                                PersonModel person = PersonModel.create(core, editRecord.id);
                                                if (person != null) {
                                                    ActivityLogOrganizationID = person.OrganizationID;
                                                }
                                            }
                                            LogController.addSiteActivity(core, "modifying field " + fieldName, editRecord.id, ActivityLogOrganizationID);
                                            break;
                                        case "organizations":
                                            //
                                            LogController.addSiteActivity(core, "modifying field " + fieldName, 0, editRecord.id);
                                            break;
                                    }
                                }
                            }
                        }
                        //
                        core.db.csClose(ref csEditRecord);
                        if (recordChanged) {
                            //
                            // -- clear cache
                            string tableName = "";
                            if (editRecord.contentControlId == 0) {
                                tableName = CdefController.getContentTablename(core, adminInfo.adminContent.name);
                            } else {
                                tableName = CdefController.getContentTablename(core, editRecord.contentControlId_Name);
                            }
                            //todo  NOTE: The following VB 'Select Case' included either a non-ordinal switch expression or non-ordinal, range-type, or non-constant 'Case' expressions and was converted to C# 'if-else' logic:
                            //							Select Case tableName.ToLower()
                            var tempVar = tableName.ToLower();
                            //ORIGINAL LINE: Case linkAliasModel.contentTableName.ToLower()
                            if (tempVar == LinkAliasModel.contentTableName.ToLower()) {
                                //
                                LinkAliasModel.invalidateRecordCache(core, editRecord.id);
                                //Models.Complex.routeDictionaryModel.invalidateCache(core)
                            }
                            //ORIGINAL LINE: Case addonModel.contentTableName.ToLower()
                            else if (tempVar == AddonModel.contentTableName.ToLower()) {
                                //
                                AddonModel.invalidateRecordCache(core, editRecord.id);
                                //Models.Complex.routeDictionaryModel.invalidateCache(core)
                            }
                            //ORIGINAL LINE: Case Else
                            else {
                                LinkAliasModel.invalidateRecordCache(core, editRecord.id);
                            }

                        }
                        ////
                        //// ----- Clear/Set PageNotFound
                        ////
                        //if (editRecord.SetPageNotFoundPageID) {
                        //    core.siteProperties.setProperty("PageNotFoundPageID", genericController.encodeText(editRecord.id));
                        //}
                        ////
                        //// ----- Clear/Set LandingPageID
                        ////
                        //if (editRecord.SetLandingPageID) {
                        //    core.siteProperties.setProperty("LandingPageID", genericController.encodeText(editRecord.id));
                        //}
                        //
                        // ----- clear/set authoring controls
                        //
                        core.workflow.ClearEditLock(adminInfo.adminContent.name, editRecord.id);
                        //
                        // ----- if admin content is changed, reload the adminContext.content data in case this is a save, and not an OK
                        //
                        if (recordChanged && SaveCCIDValue != 0) {
                            CdefController.setContentControlId(core, (editRecord.contentControlId.Equals(0)) ? adminInfo.adminContent.id : editRecord.contentControlId, editRecord.id, SaveCCIDValue);
                            editRecord.contentControlId_Name = CdefController.getContentNameByID(core, SaveCCIDValue);
                            adminInfo.adminContent = CDefModel.create(core, editRecord.contentControlId_Name);
                            adminInfo.adminContent.id = adminInfo.adminContent.id;
                            adminInfo.adminContent.name = adminInfo.adminContent.name;
                        }
                    }
                    editRecord.Saved = true;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                    tempGetJustTableName = tempGetJustTableName.Substring(GenericController.vbInstr(tempGetJustTableName, " "));
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetJustTableName;
        }
        //
        // ====================================================================================================
        /// <summary>
        ///edit record
        /// </summary>
        /// <param name="adminInfo.content"></param>
        /// <param name="editRecord"></param>
        /// <returns></returns>
        private string GetForm_Edit(AdminInfoDomainModel adminInfo) {
            string returnHtml = "";
            try {
                //
                cp.Utils.AppendLog("GetForm_Edit, enter");
                //
                bool AllowajaxTabs = (core.siteProperties.getBoolean("AllowAjaxEditTabBeta", false));
                //
                if ((core.doc.debug_iUserError != "") & adminInfo.editRecord.Loaded) {
                    //
                    // block load if there was a user error and it is already loaded (assume error was from response )
                } else if (adminInfo.adminContent.id <= 0) {
                    //
                    // Invalid Content
                    ErrorController.addUserError(core, "There was a problem identifying the content you requested. Please return to the previous form and verify your selection.");
                    return "";
                } else if (adminInfo.editRecord.Loaded & !adminInfo.editRecord.Saved) {
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
                    foreach (var keyValuePair in adminInfo.adminContent.fields) {
                        CDefFieldModel field = keyValuePair.Value;
                        switch (field.fieldTypeId) {
                            case FieldTypeIdFile:
                            case FieldTypeIdFileImage:
                                adminInfo.editRecord.fieldsLc[field.nameLc].value = adminInfo.editRecord.fieldsLc[field.nameLc].dbValue;
                                break;
                        }
                    }
                } else {
                    //
                    // otherwise, load the record, even if it was loaded during a previous form process
                    LoadEditRecord(adminInfo, true);
                    // -- allow for record to have no content control id
                    //if (adminInfo.editRecord.contentControlId == 0) {
                    //    if (core.doc.debug_iUserError != "") {
                    //        //
                    //        // known user error, just return
                    //    } else {
                    //        //
                    //        // unknown error, set userError and return
                    //        ErrorController.addUserError(core, "There was an unknown error in your request for data. Please let the site administrator know.");
                    //    }
                    //    return "";
                    //}
                }
                //
                // Test if this editors has access to this record
                
                if (!AdminInfoDomainModel.userHasContentAccess(core, ((adminInfo.editRecord.contentControlId.Equals(0)) ? adminInfo.adminContent.id : adminInfo.editRecord.contentControlId))) {
                    ErrorController.addUserError(core, "Your account on this system does not have access rights to edit this content.");
                    return "";
                }
                //
                // Setup Edit Referer
                string EditReferer = core.docProperties.getText(RequestNameEditReferer);
                if (string.IsNullOrEmpty(EditReferer)) {
                    EditReferer = core.webServer.requestReferer;
                    if (!string.IsNullOrEmpty(EditReferer)) {
                        //
                        // special case - if you are coming from the advanced search, go back to the list page
                        EditReferer = GenericController.vbReplace(EditReferer, "&af=39", "");
                        //
                        // if referer includes AdminWarningMsg (admin hint message), remove it -- this edit may fix the problem
                        int Pos = EditReferer.IndexOf("AdminWarningMsg=");
                        if (Pos >= 0) {
                            EditReferer = EditReferer.Left(Pos - 2);
                        }
                    }
                }
                core.doc.addRefreshQueryString(RequestNameEditReferer, EditReferer);
                //
                // Print common form elements
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                Stream.Add(GetForm_EditFormStart(adminInfo, AdminFormEdit));
                bool IsLandingPageParent = false;
                int TemplateIDForStyles = 0;
                bool IsTemplateTable = (adminInfo.adminContent.tableName.ToLower() == Processor.Models.Db.PageTemplateModel.contentTableName);
                bool IsPageContentTable = (adminInfo.adminContent.tableName.ToLower() == Processor.Models.Db.PageContentModel.contentTableName);
                bool IsEmailTable = (adminInfo.adminContent.tableName.ToLower() == Processor.Models.Db.EmailModel.contentTableName);
                int emailIdForStyles = IsEmailTable ? adminInfo.editRecord.id : 0;
                bool IsLandingPage = false;
                bool IsRootPage = false;
                if (IsPageContentTable && (adminInfo.editRecord.id != 0)) {
                    //
                    // landing page case
                    if (core.siteProperties.landingPageID != 0) {
                        IsLandingPage = (adminInfo.editRecord.id == core.siteProperties.landingPageID);
                        IsRootPage = IsPageContentTable && (adminInfo.editRecord.parentID == 0);
                    }
                }
                //
                //bool IsLandingSection = false;
                //bool IsLandingPageTemp = false;
                ////
                //// ----- special case messages
                ////
                //string CustomDescription = "";
                //if (IsLandingSection) {
                //    CustomDescription = "<div>This is the default Landing Section for this website. This section is displayed when no specific page is requested. It should not be deleted, renamed, marked inactive, blocked or hidden.</div>";
                //} else if (IsLandingPageTemp) {
                //    CustomDescription = "<div>This page is being used as the default Landing Page for this website, although it has not been set. This may be because a landing page has not been created, or it has been deleted. To make this page the permantent landing page, check the appropriate box in the control tab.</div>";
                //} else if (IsLandingPage) {
                //    CustomDescription = "<div>This is the default Landing Page for this website. It should not be deleted. You can not mark this record inactive, or use the Publish Date, Expire Date or Archive Date features.</div>";
                //} else if (IsLandingPageParent) {
                //    CustomDescription = "<div>This page is a parent of the default Landing Page for this website. It should not be deleted. You can not mark this record inactive, or use the Publish Date, Expire Date or Archive Date features.</div>";
                //} else if (IsRootPage) {
                //    CustomDescription = "<div>This page is a Root Page. A Root Page is the primary page of a section. If you delete or inactivate this page, the section will create a new blank page in its place.</div>";
                //}
                //
                // ----- Determine TemplateIDForStyles
                if (IsTemplateTable) {
                    TemplateIDForStyles = adminInfo.editRecord.id;
                } else if (IsPageContentTable) {
                    //Call core.pages.getPageArgs(adminInfo.editRecord.id, false, False, ignoreInteger, TemplateIDForStyles, ignoreInteger, IgnoreString, IgnoreBoolean, ignoreInteger, IgnoreBoolean, "")
                }
                var headerInfo = new RecordEditHeaderInfoClass() {
                    recordId = adminInfo.editRecord.id,
                    //recordAddedById = adminInfo.editRecord.createdBy.id,
                    //recordDateAdded = adminInfo.editRecord.dateAdded,
                    //recordDateModified = adminInfo.editRecord.modifiedDate,
                    recordLockById = adminInfo.editRecord.EditLockMemberID,
                    recordLockExpiresDate = adminInfo.editRecord.EditLockExpires,
                    //recordModifiedById = adminInfo.editRecord.modifiedBy.id,
                    recordName = adminInfo.editRecord.nameLc
                };
                string titleBarDetails = AdminUIController.getEditForm_TitleBarDetails(core, headerInfo, adminInfo.editRecord);
                //
                // ----- determine access details
                //
                bool allowCMEdit = false;
                bool allowCMAdd = false;
                bool allowCMDelete = false;
                core.session.getContentAccessRights(core, adminInfo.adminContent.name, ref allowCMEdit, ref allowCMAdd, ref allowCMDelete);
                bool allowAdd = adminInfo.adminContent.allowAdd && allowCMAdd;
                bool AllowDelete = adminInfo.adminContent.allowDelete & allowCMDelete & (adminInfo.editRecord.id != 0);
                bool allowSave = allowCMEdit;
                bool AllowRefresh = true;
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
                string fieldEditorList = EditorController.getFieldTypeDefaultEditorAddonIdList(core);
                string[] fieldTypeDefaultEditors = fieldEditorList.Split(',');
                //
                // load user's editor preferences to fieldEditorPreferences() - this is the editor this user has picked when there are >1
                //   fieldId:addonId,fieldId:addonId,etc
                //   with custom FancyBox form in edit window with button "set editor preference"
                //   this button causes a 'refresh' action, reloads fields with stream without save
                //
                string fieldEditorPreferencesList = core.userProperty.getText("editorPreferencesForContent:" + adminInfo.adminContent.id, "");
                //
                // add the addon editors assigned to each field
                // !!!!! this should be added to metaData load
                //
                string SQL = "select"
                    + " f.id,f.editorAddonID"
                    + " from ccfields f"
                    + " where"
                    + " f.ContentID=" + adminInfo.adminContent.id + " and f.editorAddonId is not null";
                DataTable dt = core.db.executeQuery(SQL);

                string[,] Cells = core.db.convertDataTabletoArray(dt);
                for (int Ptr = 0; Ptr < Cells.GetLength(1); Ptr++) {
                    int fieldId = GenericController.encodeInteger(Cells[0, Ptr]);
                    if (fieldId > 0) {
                        fieldEditorPreferencesList = fieldEditorPreferencesList + "," + fieldId + ":" + Cells[1, Ptr];
                    }
                }
                //
                // load fieldEditorOptions - these are all the editors available for each field
                //
                Dictionary<string, int> fieldEditorOptions = new Dictionary<string, int>();
                int fieldEditorOptionCnt = 0;
                SQL = "select r.contentFieldTypeId,a.Id"
                    + " from ccAddonContentFieldTypeRules r"
                    + " left join ccaggregatefunctions a on a.id=r.addonid"
                    + " where (r.active<>0)and(a.active<>0)and(a.id is not null) order by r.contentFieldTypeID";
                dt = core.db.executeQuery(SQL);
                Cells = core.db.convertDataTabletoArray(dt);
                fieldEditorOptionCnt = Cells.GetUpperBound(1) + 1;
                for (int Ptr = 0; Ptr < fieldEditorOptionCnt; Ptr++) {
                    int fieldId = GenericController.encodeInteger(Cells[0, Ptr]);
                    if ((fieldId > 0) && (!fieldEditorOptions.ContainsKey(fieldId.ToString()))) {
                        fieldEditorOptions.Add(fieldId.ToString(), GenericController.encodeInteger(Cells[1, Ptr]));
                    }
                }
                //
                // ----- determine contentType for editor
                //
                ContentTypeEnum ContentType;
                if (GenericController.vbLCase(adminInfo.adminContent.name) == "email templates") {
                    ContentType = ContentTypeEnum.contentTypeEmailTemplate;
                } else if (GenericController.vbLCase(adminInfo.adminContent.tableName) == "cctemplates") {
                    ContentType = ContentTypeEnum.contentTypeWebTemplate;
                } else if (GenericController.vbLCase(adminInfo.adminContent.tableName) == "ccemail") {
                    ContentType = ContentTypeEnum.contentTypeEmail;
                } else {
                    ContentType = ContentTypeEnum.contentTypeWeb;
                }
                //
                //-----Create edit page
                string styleOptionList = "";
                string editorAddonListJSON = core.html.getWysiwygAddonList(ContentType);
                string styleList = "";
                string adminContentTableNameLower = adminInfo.adminContent.tableName.ToLower();
                //
                LogController.logTrace(core, "getFormEdit, adminInfo.editRecord.contentControlId [" + adminInfo.editRecord.contentControlId + "]");
                //
                if (adminContentTableNameLower == PersonModel.contentTableName.ToLower()) {
                    //
                    // -- people
                    if (!(core.session.isAuthenticatedAdmin(core))) {
                        //
                        // Must be admin
                        Stream.Add(BodyErrorClass.get( core, "This edit form requires administrator access.", ""));
                    } else {
                        string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = false,
                            allowAdd = (allowAdd && adminInfo.adminContent.allowAdd & adminInfo.editRecord.AllowInsert),
                            allowCancel = adminInfo.editRecord.AllowCancel,
                            allowCreateDuplicate = (allowSave && adminInfo.editRecord.AllowSave & (adminInfo.editRecord.id != 0)),
                            allowDelete = AllowDelete && adminInfo.editRecord.AllowDelete,
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = (allowSave && adminInfo.editRecord.AllowSave),
                            allowSend = false,
                            allowSendTest = false,
                            hasChildRecords = false,
                            isPageContent = false
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getTitleBar(core, GetForm_EditTitle(adminInfo), titleBarDetails));
                        Stream.Add(GetForm_Edit_Tabs(adminInfo, adminInfo.editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(GetForm_Edit_AddTab("Groups", GetForm_Edit_MemberGroups(adminInfo), adminInfo.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Control&nbsp;Info", GetForm_Edit_Control(adminInfo), adminInfo.allowAdminTabs));
                        if (adminInfo.allowAdminTabs) Stream.Add(core.doc.menuComboTab.GetTabs(core));
                        Stream.Add(EditSectionButtonBar);
                    }
                } else if (adminContentTableNameLower == EmailModel.contentTableName.ToLower()) {
                    //
                    LogController.logTrace(core, "getFormEdit, treat as email, adminContentTableNameLower [" + adminContentTableNameLower + "]");
                    //
                    // -- email
                    bool EmailSubmitted = false;
                    bool EmailSent = false;
                    int SystemEmailCID = CdefController.getContentId(core, "System Email");
                    int ConditionalEmailCID = CdefController.getContentId(core, "Conditional Email");
                    DateTime LastSendTestDate = DateTime.MinValue;
                    bool AllowEmailSendWithoutTest = (core.siteProperties.getBoolean("AllowEmailSendWithoutTest", false));
                    if (adminInfo.editRecord.fieldsLc.ContainsKey("lastsendtestdate")) {
                        LastSendTestDate = GenericController.encodeDate(adminInfo.editRecord.fieldsLc["lastsendtestdate"].value);
                    }
                    if (!(core.session.isAuthenticatedAdmin(core))) {
                        //
                        // Must be admin
                        Stream.Add(BodyErrorClass.get( core, "This edit form requires Member Administration access.", "This edit form requires Member Administration access."));
                    } else if (CdefController.isWithinContent(core, adminInfo.editRecord.contentControlId, SystemEmailCID)) {
                        //
                        LogController.logTrace(core, "getFormEdit, System email");
                        //
                        // System Email
                        EmailSubmitted = false;
                        if (adminInfo.editRecord.id != 0) {
                            if (adminInfo.editRecord.fieldsLc.ContainsKey("testmemberid")) {
                                adminInfo.editRecord.fieldsLc["testmemberid"].value = core.session.user.id;
                            }
                        }
                        string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = false,
                            allowAdd = (allowAdd && adminInfo.adminContent.allowAdd & adminInfo.editRecord.AllowInsert),
                            allowCancel = adminInfo.editRecord.AllowCancel,
                            allowCreateDuplicate = (allowSave && adminInfo.editRecord.AllowSave & (adminInfo.editRecord.id != 0)),
                            allowDelete = AllowDelete && adminInfo.editRecord.AllowDelete && core.session.isAuthenticatedDeveloper(core),
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = (allowSave && adminInfo.editRecord.AllowSave && (!EmailSubmitted) && (!EmailSent)),
                            allowSend = false,
                            allowSendTest = ((!EmailSubmitted) && (!EmailSent)),
                            hasChildRecords = false,
                            isPageContent = false
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getTitleBar(core, GetForm_EditTitle(adminInfo), titleBarDetails));
                        Stream.Add(GetForm_Edit_Tabs(adminInfo, adminInfo.editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(GetForm_Edit_AddTab("Send&nbsp;To&nbsp;Groups", GetForm_Edit_EmailRules(adminInfo, adminInfo.editRecord.Read_Only & (!core.session.isAuthenticatedDeveloper(core))), adminInfo.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Send&nbsp;To&nbsp;Topics", GetForm_Edit_EmailTopics(adminInfo, adminInfo.editRecord.Read_Only & (!core.session.isAuthenticatedDeveloper(core))), adminInfo.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Bounce&nbsp;Control", GetForm_Edit_EmailBounceStatus(adminInfo), adminInfo.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Control&nbsp;Info", GetForm_Edit_Control(adminInfo), adminInfo.allowAdminTabs));
                        if (adminInfo.allowAdminTabs) Stream.Add(core.doc.menuComboTab.GetTabs(core));
                        Stream.Add(EditSectionButtonBar);
                    } else if (CdefController.isWithinContent(core, adminInfo.editRecord.contentControlId, ConditionalEmailCID)) {
                        //
                        // Conditional Email
                        EmailSubmitted = false;
                        if (adminInfo.editRecord.id != 0) {
                            if (adminInfo.editRecord.fieldsLc.ContainsKey("submitted")) EmailSubmitted = GenericController.encodeBoolean(adminInfo.editRecord.fieldsLc["submitted"].value);
                        }
                        //
                        cp.Utils.AppendLog("GetForm_Edit, Conditional Email, EmailSubmitted [" + EmailSubmitted + "], LastSendTestDate [" + LastSendTestDate + "], AllowEmailSendWithoutTest [" + AllowEmailSendWithoutTest + "]");
                        //
                        string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = !EmailSubmitted & ((LastSendTestDate != DateTime.MinValue) | AllowEmailSendWithoutTest),
                            allowDeactivate = EmailSubmitted,
                            allowAdd = allowAdd && adminInfo.adminContent.allowAdd & adminInfo.editRecord.AllowInsert,
                            allowCancel = adminInfo.editRecord.AllowCancel,
                            allowCreateDuplicate = allowAdd && (adminInfo.editRecord.id != 0),
                            allowDelete = AllowDelete && adminInfo.editRecord.AllowDelete && core.session.isAuthenticatedDeveloper(core),
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = allowSave && adminInfo.editRecord.AllowSave && !EmailSubmitted,
                            allowSend = false,
                            allowSendTest = !EmailSubmitted,
                            hasChildRecords = false,
                            isPageContent = false
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getTitleBar(core, GetForm_EditTitle(adminInfo), titleBarDetails));
                        Stream.Add(GetForm_Edit_Tabs(adminInfo, adminInfo.editRecord.Read_Only || EmailSubmitted, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(GetForm_Edit_AddTab("Condition&nbsp;Groups", GetForm_Edit_EmailRules(adminInfo, adminInfo.editRecord.Read_Only || EmailSubmitted), adminInfo.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Bounce&nbsp;Control", GetForm_Edit_EmailBounceStatus(adminInfo), adminInfo.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Control&nbsp;Info", GetForm_Edit_Control(adminInfo), adminInfo.allowAdminTabs));
                        if (adminInfo.allowAdminTabs) Stream.Add(core.doc.menuComboTab.GetTabs(core));
                        Stream.Add(EditSectionButtonBar);
                    } else {
                        //
                        cp.Utils.AppendLog("GetForm_Edit, Group Email");
                        //
                        // Group Email
                        if (adminInfo.editRecord.id != 0) {
                            EmailSubmitted = encodeBoolean(adminInfo.editRecord.fieldsLc["submitted"].value);
                            EmailSent = encodeBoolean(adminInfo.editRecord.fieldsLc["sent"].value);
                        }
                        string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = false,
                            allowDeactivate = false,
                            allowAdd = allowAdd && adminInfo.adminContent.allowAdd & adminInfo.editRecord.AllowInsert,
                            allowCancel = adminInfo.editRecord.AllowCancel,
                            allowCreateDuplicate = allowAdd && (adminInfo.editRecord.id != 0),
                            allowDelete = !EmailSubmitted & (AllowDelete && adminInfo.editRecord.AllowDelete),
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = !EmailSubmitted & (allowSave && adminInfo.editRecord.AllowSave),
                            allowSend = !EmailSubmitted & ((LastSendTestDate != DateTime.MinValue) | AllowEmailSendWithoutTest),
                            allowSendTest = !EmailSubmitted,
                            hasChildRecords = false,
                            isPageContent = false
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getTitleBar(core, GetForm_EditTitle(adminInfo), titleBarDetails));
                        Stream.Add(GetForm_Edit_Tabs(adminInfo, adminInfo.editRecord.Read_Only | EmailSubmitted || EmailSent, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(GetForm_Edit_AddTab("Send&nbsp;To&nbsp;Groups", GetForm_Edit_EmailRules(adminInfo, adminInfo.editRecord.Read_Only | EmailSubmitted || EmailSent), adminInfo.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Send&nbsp;To&nbsp;Topics", GetForm_Edit_EmailTopics(adminInfo, adminInfo.editRecord.Read_Only | EmailSubmitted || EmailSent), adminInfo.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Bounce&nbsp;Control", GetForm_Edit_EmailBounceStatus(adminInfo), adminInfo.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Control&nbsp;Info", GetForm_Edit_Control(adminInfo), adminInfo.allowAdminTabs));
                        if (adminInfo.allowAdminTabs) Stream.Add(core.doc.menuComboTab.GetTabs(core));
                        Stream.Add(EditSectionButtonBar);
                    }
                } else if (adminInfo.adminContent.tableName.ToLower() == ContentModel.contentTableName.ToLower()) {
                    if (!(core.session.isAuthenticatedAdmin(core))) {
                        //
                        // Must be admin
                        //
                        Stream.Add(BodyErrorClass.get( core, "This edit form requires Member Administration access.", "This edit form requires Member Administration access."));
                    } else {
                        string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = false,
                            allowAdd = (allowAdd && adminInfo.adminContent.allowAdd & adminInfo.editRecord.AllowInsert),
                            allowCancel = adminInfo.editRecord.AllowCancel,
                            allowCreateDuplicate = (allowSave && adminInfo.editRecord.AllowSave & (adminInfo.editRecord.id != 0)),
                            allowDelete = AllowDelete && adminInfo.editRecord.AllowDelete,
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = (allowSave && adminInfo.editRecord.AllowSave),
                            allowSend = false,
                            allowSendTest = false,
                            hasChildRecords = false,
                            isPageContent = false
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getTitleBar(core, GetForm_EditTitle(adminInfo), titleBarDetails));
                        Stream.Add(GetForm_Edit_Tabs(adminInfo, adminInfo.editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(GetForm_Edit_AddTab("Authoring Permissions", GetForm_Edit_GroupRules(adminInfo), adminInfo.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab("Control&nbsp;Info", GetForm_Edit_Control(adminInfo), adminInfo.allowAdminTabs));
                        if (adminInfo.allowAdminTabs) {
                            Stream.Add(core.doc.menuComboTab.GetTabs(core));
                            //Call Stream.Add("<div class=""ccPanelBackground"">" & core.main_GetComboTabs() & "</div>")
                        }
                        Stream.Add(EditSectionButtonBar);
                    }
                    //
                } else if (adminContentTableNameLower == PageContentModel.contentTableName.ToLower()) {
                    //
                    // Page Content
                    //
                    int TableID = core.db.getRecordID("Tables", "ccPageContent");
                    string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                        allowActivate = false,
                        allowAdd = (allowAdd && adminInfo.adminContent.allowAdd & adminInfo.editRecord.AllowInsert),
                        allowCancel = adminInfo.editRecord.AllowCancel,
                        allowCreateDuplicate = (allowSave && adminInfo.editRecord.AllowSave & (adminInfo.editRecord.id != 0)),
                        allowDelete = AllowDelete && adminInfo.editRecord.AllowDelete,
                        allowMarkReviewed = false,
                        allowRefresh = AllowRefresh,
                        allowSave = (allowSave && adminInfo.editRecord.AllowSave),
                        allowSend = false,
                        allowSendTest = false,
                        hasChildRecords = false,
                        isPageContent = false
                    });
                    Stream.Add(EditSectionButtonBar);
                    Stream.Add(AdminUIController.getTitleBar(core, GetForm_EditTitle(adminInfo), titleBarDetails));
                    Stream.Add(GetForm_Edit_Tabs(adminInfo, adminInfo.editRecord.Read_Only, IsLandingPage || IsLandingPageParent, IsRootPage, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    Stream.Add(GetForm_Edit_AddTab("Link Aliases", GetForm_Edit_LinkAliases(adminInfo, adminInfo.editRecord.Read_Only), adminInfo.allowAdminTabs));
                    Stream.Add(GetForm_Edit_AddTab("Content Watch", GetForm_Edit_ContentTracking(adminInfo), adminInfo.allowAdminTabs));
                    Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminInfo), adminInfo.allowAdminTabs));
                    if (adminInfo.allowAdminTabs) {
                        Stream.Add(core.doc.menuComboTab.GetTabs(core));
                    }
                    Stream.Add(EditSectionButtonBar);
                    //else if (adminContentTableNameLower == sectionModel.contentTableName.ToLower()) {
                    //    '
                    //    ' Site Sections
                    //    '
                    //    EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContext.content, editRecord, (Not IsLandingSection) And AllowDelete, allowSave, AllowAdd)
                    //    EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
                    //    Call Stream.Add(EditSectionButtonBar)
                    //    Call Stream.Add(adminUIController.GetTitleBar(core,GetForm_EditTitle(adminContext.content, editRecord), HeaderDescription))
                    //    Call Stream.Add(GetForm_Edit_Tabs(adminContext.content, editRecord, adminInfo.editRecord.Read_Only, IsLandingSection, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                    //    Call Stream.Add(GetForm_Edit_AddTab("Select Menus", GetForm_Edit_SectionDynamicMenuRules(adminContext.content, editRecord), allowAdminTabs))
                    //    Call Stream.Add(GetForm_Edit_AddTab("Section Blocking", GetForm_Edit_SectionBlockRules(adminContext.content, editRecord), allowAdminTabs))
                    //    Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContext.content, editRecord), allowAdminTabs))
                    //    If allowAdminTabs Then
                    //        Call Stream.Add(core.htmlDoc.menu_GetComboTabs())
                    //        'Call Stream.Add("<div class=""ccPanelBackground"">" & core.main_GetComboTabs() & "</div>")
                    //    End If
                    //    Call Stream.Add(EditSectionButtonBar)
                    //Case "CCDYNAMICMENUS"
                    //    '
                    //    ' Edit Dynamic Sections
                    //    '
                    //    EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContext.content, editRecord, AllowDelete, allowSave, AllowAdd)
                    //    EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
                    //    Call Stream.Add(EditSectionButtonBar)
                    //    Call Stream.Add(adminUIController.GetTitleBar(core,GetForm_EditTitle(adminContext.content, editRecord), HeaderDescription))
                    //    Call Stream.Add(GetForm_Edit_Tabs(adminContext.content, editRecord, adminInfo.editRecord.Read_Only, False, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                    //    Call Stream.Add(GetForm_Edit_AddTab("Select Sections", GetForm_Edit_DynamicMenuSectionRules(adminContext.content, editRecord), allowAdminTabs))
                    //    Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContext.content, editRecord), allowAdminTabs))
                    //    If allowAdminTabs Then
                    //        Call Stream.Add(core.htmlDoc.menu_GetComboTabs())
                    //        'Call Stream.Add("<div class=""ccPanelBackground"">" & core.main_GetComboTabs() & "</div>")
                    //    End If
                    //    Call Stream.Add(EditSectionButtonBar)

                    //} else if (adminContentTableNameLower == libraryFoldersModel.contentTableName.ToLower()) {
                    //    //
                    //    // Library Folders
                    //    //
                    //    EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContext.content, editRecord, AllowDelete, allowSave, AllowAdd);
                    //    EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord);
                    //    Stream.Add(EditSectionButtonBar);
                    //    Stream.Add(adminUIController.GetTitleBar(core, GetForm_EditTitle(adminContext.content, editRecord), HeaderDescription));
                    //    Stream.Add(GetForm_Edit_Tabs(adminContext.content, editRecord, adminInfo.editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    //    Stream.Add(GetForm_Edit_AddTab("Authoring Access", GetForm_Edit_LibraryFolderRules(adminContext.content, editRecord), allowAdminTabs));
                    //    Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContext.content, editRecord), allowAdminTabs));
                    //    if (allowAdminTabs) {
                    //        Stream.Add(core.doc.menuComboTab.GetTabs(core));
                    //    }
                    //    Stream.Add(EditSectionButtonBar);
                    //
                    //ORIGINAL LINE: Case genericController.vbUCase("ccGroups")
                    //} else if (adminContentTableNameLower == groupModel.contentTableName.ToLower()) {
                    //    //
                    //    // Groups
                    //    //
                    //    string EditSectionButtonBar = adminUIController.getButtonBarForEdit(core, new editButtonBarInfoClass() {
                    //        allowActivate = false,
                    //        allowAdd = (allowAdd && adminContext.adminContent.allowAdd & adminInfo.editRecord.AllowInsert),
                    //        allowCancel = adminInfo.editRecord.AllowCancel,
                    //        allowCreateDuplicate = (allowSave && adminInfo.editRecord.AllowSave & (adminInfo.editRecord.id != 0)),
                    //        allowDelete = AllowDelete && adminInfo.editRecord.AllowDelete,
                    //        allowMarkReviewed = false,
                    //        allowRefresh = AllowRefresh,
                    //        allowSave = (allowSave && adminInfo.editRecord.AllowSave),
                    //        allowSend = false,
                    //        allowSendTest = false,
                    //        hasChildRecords = false,
                    //        isPageContent = false
                    //    });
                    //    Stream.Add(EditSectionButtonBar);
                    //    Stream.Add(adminUIController.GetTitleBar(core, GetForm_EditTitle(adminContext), titleBarDetails));
                    //    Stream.Add(GetForm_Edit_Tabs(adminContext, adminInfo.editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    //    Stream.Add(GetForm_Edit_AddTab("Authoring Permissions", GetForm_Edit_ContentGroupRules(adminContext), adminContext.allowAdminTabs));
                    //    Stream.Add(GetForm_Edit_AddTab("Content Watch", GetForm_Edit_ContentTracking(adminContext), adminContext.allowAdminTabs));
                    //    Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContext), adminContext.allowAdminTabs));
                    //    if (adminContext.allowAdminTabs) {
                    //        Stream.Add(core.doc.menuComboTab.GetTabs(core));
                    //    }
                    //    Stream.Add(EditSectionButtonBar);
                    //} else if (adminContentTableNameLower == layoutModel.contentTableName.ToLower()) {
                    //    //
                    //    // LAYOUTS
                    //    string EditSectionButtonBar = adminUIController.getButtonBarForEdit(core, new editButtonBarInfoClass() {
                    //        allowActivate = false,
                    //        allowAdd = (allowAdd && adminContext.adminContent.allowAdd & adminInfo.editRecord.AllowInsert),
                    //        allowCancel = adminInfo.editRecord.AllowCancel,
                    //        allowCreateDuplicate = (allowSave && adminInfo.editRecord.AllowSave & (adminInfo.editRecord.id != 0)),
                    //        allowDelete = AllowDelete && adminInfo.editRecord.AllowDelete,
                    //        allowMarkReviewed = false,
                    //        allowRefresh = AllowRefresh,
                    //        allowSave = (allowSave && adminInfo.editRecord.AllowSave),
                    //        allowSend = false,
                    //        allowSendTest = false,
                    //        hasChildRecords = false,
                    //        isPageContent = false
                    //    });
                    //    Stream.Add(EditSectionButtonBar);
                    //    Stream.Add(adminUIController.GetTitleBar(core, GetForm_EditTitle(adminContext), titleBarDetails));
                    //    Stream.Add(GetForm_Edit_Tabs(adminContext, adminInfo.editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    //    Stream.Add(GetForm_Edit_AddTab("Reports", GetForm_Edit_LayoutReports(adminContext), adminContext.allowAdminTabs));
                    //    Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContext), adminContext.allowAdminTabs));
                    //    if (adminContext.allowAdminTabs) {
                    //        Stream.Add(core.doc.menuComboTab.GetTabs(core));
                    //    }
                    //    Stream.Add(EditSectionButtonBar);
                } else {
                    //
                    // All other tables (User definined)
                    bool IsPageContent = CdefController.isWithinContent(core, adminInfo.adminContent.id, CdefController.getContentId(core, "Page Content"));
                    bool HasChildRecords = CdefController.isContentFieldSupported(core, adminInfo.adminContent.name, "parentid");
                    bool AllowMarkReviewed = core.db.isSQLTableField("default", adminInfo.adminContent.tableName, "DateReviewed");
                    string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                        allowActivate = false,
                        allowAdd = (allowAdd && adminInfo.adminContent.allowAdd & adminInfo.editRecord.AllowInsert),
                        allowCancel = adminInfo.editRecord.AllowCancel,
                        allowCreateDuplicate = (allowSave && adminInfo.editRecord.AllowSave & (adminInfo.editRecord.id != 0)),
                        allowDelete = AllowDelete && adminInfo.editRecord.AllowDelete,
                        allowMarkReviewed = AllowMarkReviewed,
                        allowRefresh = AllowRefresh,
                        allowSave = (allowSave && adminInfo.editRecord.AllowSave),
                        allowSend = false,
                        allowSendTest = false,
                        hasChildRecords = HasChildRecords,
                        isPageContent = IsPageContent
                    });
                    Stream.Add(EditSectionButtonBar);
                    Stream.Add(AdminUIController.getTitleBar(core, GetForm_EditTitle(adminInfo), titleBarDetails));
                    Stream.Add(GetForm_Edit_Tabs(adminInfo, adminInfo.editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    Stream.Add(GetForm_Edit_AddTab("Content Watch", GetForm_Edit_ContentTracking(adminInfo), adminInfo.allowAdminTabs));
                    Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminInfo), adminInfo.allowAdminTabs));
                    if (adminInfo.allowAdminTabs) Stream.Add(core.doc.menuComboTab.GetTabs(core));
                    Stream.Add(EditSectionButtonBar);
                }
                Stream.Add("</form>");
                returnHtml = Stream.Text;
                if (adminInfo.editRecord.id == 0) {
                    core.html.addTitle("Add " + adminInfo.adminContent.name);
                } else if (adminInfo.editRecord.nameLc == "") {
                    core.html.addTitle("Edit #" + adminInfo.editRecord.id + " in " + adminInfo.editRecord.contentControlId_Name);
                } else {
                    core.html.addTitle("Edit " + adminInfo.editRecord.nameLc + " in " + adminInfo.editRecord.contentControlId_Name);
                }
                //
                cp.Utils.AppendLog("GetForm_Edit, exit");
                //
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnHtml;
        }
        ////
        ////========================================================================
        //// ----- Print the Normal Content Edit form
        ////
        ////   Print the content fields and Topic Groups section
        ////========================================================================
        ////
        //private string GetForm_Publish() {
        //    string tempGetForm_Publish = null;
        //    try {
        //        //
        //        string FieldList = null;
        //        string ModifiedDateString = null;
        //        string SubmittedDateString = null;
        //        string ApprovedDateString = null;
        //        //adminUIController Adminui = new adminUIController(core);
        //        string ButtonList = "";
        //        string Caption = null;
        //        int CS = 0;
        //        string SQL = null;
        //        string RowColor = null;
        //        int RecordCount = 0;
        //        int RecordLast = 0;
        //        int RecordNext = 0;
        //        int RecordPrevious = 0;
        //        string RecordName = null;
        //        string Copy = null;
        //        int ContentID = 0;
        //        string ContentName = null;
        //        int RecordID = 0;
        //        string Link = null;
        //        int CSAuthoringRecord = 0;
        //        string TableName = null;
        //        bool IsInserted = false;
        //        bool IsDeleted = false;
        //        bool IsModified = false;
        //        string ModifiedName = "";
        //        DateTime ModifiedDate = default(DateTime);
        //        bool IsSubmitted = false;
        //        string SubmitName = "";
        //        DateTime SubmittedDate = default(DateTime);
        //        bool IsApproved = false;
        //        string ApprovedName = "";
        //        DateTime ApprovedDate = default(DateTime);
        //        stringBuilderLegacyController Stream = new stringBuilderLegacyController();
        //        string Body = "";
        //        string Description = null;
        //        string Button = null;
        //        string BR = "";
        //        //
        //        Button = core.docProperties.getText(RequestNameButton);
        //        if (Button == ButtonCancel) {
        //            //
        //            //
        //            //
        //            return core.webServer.redirect("/" + core.appConfig.adminRoute, "Admin Publish, Cancel Button Pressed");
        //        } else if (!core.session.isAuthenticatedAdmin(core)) {
        //            //
        //            //
        //            //
        //            ButtonList = ButtonCancel;
        //            Body += adminUIController.GetFormBodyAdminOnly();
        //        } else {
        //            //
        //            // ----- Page Body
        //            //
        //            BR = "<br>";
        //            Body += "\r<table border=\"0\" cellpadding=\"2\" cellspacing=\"2\" width=\"100%\">";
        //            Body += "\r<tr>";
        //            Body += "\r<td width=\"50\" class=\"ccPanel\" align=\"center\" class=\"ccAdminSmall\">Pub" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"42\" height=\"1\" ></td>";
        //            Body += "\r<td width=\"50\" class=\"ccPanel\" align=\"center\" class=\"ccAdminSmall\">Sub'd" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"42\" height=\"1\" ></td>";
        //            Body += "\r<td width=\"50\" class=\"ccPanel\" align=\"center\" class=\"ccAdminSmall\">Appr'd" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"42\" height=\"1\" ></td>";
        //            Body += "\r<td width=\"50\" class=\"ccPanel\" class=\"ccAdminSmall\">Edit" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"42\" height=\"1\" ></td>";
        //            Body += "\r<td width=\"200\" class=\"ccPanel\" class=\"ccAdminSmall\">Name" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"192\" height=\"1\" ></td>";
        //            Body += "\r<td width=\"100\" class=\"ccPanel\" class=\"ccAdminSmall\">Content" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"92\" height=\"1\" ></td>";
        //            Body += "\r<td width=\"50\" class=\"ccPanel\" class=\"ccAdminSmall\">#" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"92\" height=\"1\" ></td>";
        //            Body += "\r<td width=\"100\" class=\"ccPanel\" class=\"ccAdminSmall\">Public" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"92\" height=\"1\" ></td>";
        //            Body += "\r<td width=\"100%\" class=\"ccPanel\" class=\"ccAdminSmall\">Status" + BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"1\" ></td>";
        //            Body += "\r</tr>";
        //            //
        //            // ----- select modified,submitted,approved records (all non-editing controls)
        //            //
        //            SQL = "SELECT DISTINCT top 100 ccAuthoringControls.ContentID AS ContentID, ccContent.Name AS ContentName, ccAuthoringControls.RecordID, ccContentWatch.Link AS Link, ccContent.AllowWorkflowAuthoring AS ContentAllowWorkflowAuthoring,min(ccAuthoringControls.ID)"
        //                + " FROM (ccAuthoringControls"
        //                + " LEFT JOIN ccContent ON ccAuthoringControls.ContentID = ccContent.ID)"
        //                + " LEFT JOIN ccContentWatch ON ccAuthoringControls.ContentRecordKey = ccContentWatch.ContentRecordKey"
        //                + " Where (ccAuthoringControls.ControlType > 1)"
        //                + " GROUP BY ccAuthoringControls.ContentID, ccContent.Name, ccAuthoringControls.RecordID, ccContentWatch.Link, ccContent.AllowWorkflowAuthoring"
        //                + " order by min(ccAuthoringControls.ID) desc";
        //            //PageNumber = 1 + (RecordTop / RecordsPerPage)
        //            //SQL = "SELECT DISTINCT ccContent.ID AS ContentID, ccContent.Name AS ContentName, ccAuthoringControls.RecordID, ccContentWatch.Link AS Link, ccContent.AllowWorkflowAuthoring AS ContentAllowWorkflowAuthoring,max(ccAuthoringControls.DateAdded) as DateAdded" _
        //            //    & " FROM (ccAuthoringControls LEFT JOIN ccContent ON ccAuthoringControls.ContentID = ccContent.ID) LEFT JOIN ccContentWatch ON ccAuthoringControls.ContentRecordKey = ccContentWatch.ContentRecordKey" _
        //            //    & " GROUP BY ccAuthoringControls.ID,ccContent.ID, ccContent.Name, ccAuthoringControls.RecordID, ccContentWatch.Link, ccContent.AllowWorkflowAuthoring, ccAuthoringControls.ControlType" _
        //            //    & " HAVING (ccAuthoringControls.ControlType>1)" _
        //            //    & " order by max(ccAuthoringControls.DateAdded) Desc"
        //            CS = core.db.csOpenSql(SQL, "Default");
        //            //CS = core.app_openCsSql_Rev_Internal("Default", SQL, RecordsPerPage, PageNumber)
        //            RecordCount = 0;
        //            if (core.db.csOk(CS)) {
        //                RowColor = "";
        //                RecordLast = RecordTop + RecordsPerPage;
        //                //
        //                // --- Print out the records
        //                //
        //                while (core.db.csOk(CS) && RecordCount < 100) {
        //                    ContentID = core.db.csGetInteger(CS, "contentID");
        //                    ContentName = core.db.csGetText(CS, "contentname");
        //                    RecordID = core.db.csGetInteger(CS, "recordid");
        //                    Link = pageContentController.getPageLink(core, RecordID, "", true, false);
        //                    //Link = core.main_GetPageLink3(RecordID, "", True)
        //                    //If Link = "" Then
        //                    //    Link = core.db.cs_getText(CS, "Link")
        //                    //End If
        //                    if ((ContentID == 0) || (string.IsNullOrEmpty(ContentName)) || (RecordID == 0)) {
        //                        //
        //                        // This control is not valid, delete it
        //                        //
        //                        SQL = "delete from ccAuthoringControls where ContentID=" + ContentID + " and RecordID=" + RecordID;
        //                        core.db.executeQuery(SQL);
        //                    } else {
        //                        TableName = cdefModel.GetContentProperty(core, ContentName, "ContentTableName");
        //                        if (!(core.db.csGetBoolean(CS, "ContentAllowWorkflowAuthoring"))) {
        //                            //
        //                            // Authoring bug -- This record should not be here, the content does not support workflow authoring
        //                            //
        //                            handleLegacyClassError2("GetForm_Publish", "Admin Workflow Publish selected an authoring control record [" + ContentID + "." + RecordID + "] for a content definition [" + ContentName + "] that does not AllowWorkflowAuthoring.");
        //                            //Call HandleInternalError("GetForm_Publish", "Admin Workflow Publish selected an authoring control record [" & ContentID & "." & RecordID & "] for a content definition [" & ContentName & "] that does not AllowWorkflowAuthoring.")
        //                        } else {

        //                            core.doc.getAuthoringStatus(ContentName, RecordID, ref IsSubmitted, ref IsApproved, ref SubmitName, ref ApprovedName, ref IsInserted, ref IsDeleted, ref IsModified, ref ModifiedName, ref ModifiedDate, ref SubmittedDate, ref ApprovedDate);
        //                            if (RowColor == "class=\"ccPanelRowOdd\"") {
        //                                RowColor = "class=\"ccPanelRowEven\"";
        //                            } else {
        //                                RowColor = "class=\"ccPanelRowOdd\"";
        //                            }
        //                            //
        //                            // make sure the record exists
        //                            //
        //                            if (genericController.vbUCase(TableName) == "CCPAGECONTENT") {
        //                                FieldList = "ID,Name,Headline,MenuHeadline";
        //                                //SQL = "SELECT ID,Name,Headline,MenuHeadline from " & TableName & " WHERE ID=" & RecordID
        //                            } else {
        //                                FieldList = "ID,Name,Name as Headline,Name as MenuHeadline";
        //                                //SQL = "SELECT ID,Name,Name as Headline,Name as MenuHeadline from " & TableName & " WHERE ID=" & RecordID
        //                            }
        //                            CSAuthoringRecord = core.db.csOpenRecord(ContentName, RecordID, true, true, FieldList);
        //                            //CSAuthoringRecord = core.app_openCsSql_Rev_Internal("Default", SQL, 1)
        //                            if (!core.db.csOk(CSAuthoringRecord)) {
        //                                //
        //                                // This authoring control is not valid, delete it
        //                                //
        //                                SQL = "delete from ccAuthoringControls where ContentID=" + ContentID + " and RecordID=" + RecordID;
        //                                core.db.executeQuery(SQL);
        //                            } else {
        //                                RecordName = core.db.csGet(CSAuthoringRecord, "name");
        //                                if (string.IsNullOrEmpty(RecordName)) {
        //                                    RecordName = core.db.csGet(CSAuthoringRecord, "headline");
        //                                    if (string.IsNullOrEmpty(RecordName)) {
        //                                        RecordName = core.db.csGet(CSAuthoringRecord, "headline");
        //                                        if (string.IsNullOrEmpty(RecordName)) {
        //                                            RecordName = "Record " + core.db.csGet(CSAuthoringRecord, "ID");
        //                                        }
        //                                    }
        //                                }
        //                                if (true) {
        //                                    if (string.IsNullOrEmpty(Link)) {
        //                                        Link = "unknown";
        //                                    } else {
        //                                        Link = "<a href=\"" + htmlController.encodeHtml(Link) + "\" target=\"_blank\">" + Link + "</a>";
        //                                    }
        //                                    //
        //                                    // get approved status of the submitted record
        //                                    //
        //                                    Body += ("\n<tr>");
        //                                    //
        //                                    // Publish Checkbox
        //                                    //
        //                                    Body += ("<td align=\"center\" valign=\"top\" " + RowColor + ">" + htmlController.checkbox("row" + RecordCount, false) + htmlController.inputHidden("rowid" + RecordCount, RecordID) + htmlController.inputHidden("rowcontentname" + RecordCount, ContentName) + "</td>");
        //                                    //
        //                                    // Submitted
        //                                    //
        //                                    if (IsSubmitted) {
        //                                        Copy = "yes";
        //                                    } else {
        //                                        Copy = "no";
        //                                    }
        //                                    Body += ("<td align=\"center\" valign=\"top\" " + RowColor + " class=\"ccAdminSmall\">" + Copy + "</td>");
        //                                    //
        //                                    // Approved
        //                                    //
        //                                    if (IsApproved) {
        //                                        Copy = "yes";
        //                                    } else {
        //                                        Copy = "no";
        //                                    }
        //                                    Body += ("<td align=\"center\" valign=\"top\" " + RowColor + " class=\"ccAdminSmall\">" + Copy + "</td>");
        //                                    //
        //                                    // Edit
        //                                    //
        //                                    Body = Body + "<td align=\"left\" valign=\"top\" " + RowColor + " class=\"ccAdminSmall\">"
        //                                        + "<a href=\"?" + rnAdminForm + "=" + AdminFormEdit + "&cid=" + ContentID + "&id=" + RecordID + "&" + RequestNameAdminDepth + "=1\">Edit</a>"
        //                                        + "</td>";
        //                                    //
        //                                    // Name
        //                                    //
        //                                    Body += ("<td align=\"left\" valign=\"top\" " + RowColor + " class=\"ccAdminSmall\"  style=\"white-space:nowrap;\">" + RecordName + "</td>");
        //                                    //
        //                                    // Content
        //                                    //
        //                                    Body += ("<td align=\"left\" valign=\"top\" " + RowColor + " class=\"ccAdminSmall\">" + ContentName + "</td>");
        //                                    //
        //                                    // RecordID
        //                                    //
        //                                    Body += ("<td align=\"left\" valign=\"top\" " + RowColor + " class=\"ccAdminSmall\">" + RecordID + "</td>");
        //                                    //
        //                                    // Public
        //                                    //
        //                                    if (IsInserted) {
        //                                        Link = Link + "*";
        //                                    } else if (IsDeleted) {
        //                                        Link = Link + "**";
        //                                    }
        //                                    Body += ("<td align=\"left\" valign=\"top\" " + RowColor + " class=\"ccAdminSmall\" style=\"white-space:nowrap;\">" + Link + "</td>");
        //                                    //
        //                                    // Description
        //                                    //
        //                                    //Call core.app.closeCS(CSLink)
        //                                    Body += ("<td align=\"left\" valign=\"top\" " + RowColor + ">" + SpanClassAdminNormal);
        //                                    //
        //                                    //If RecordName <> "" Then
        //                                    //    Body &=  (core.htmldoc.main_encodeHTML(RecordName) & ", ")
        //                                    //End If
        //                                    //Body &=  ("Content: " & ContentName & ", RecordID: " & RecordID & "" & br & "")
        //                                    if (ModifiedDate == DateTime.MinValue) {
        //                                        ModifiedDateString = "unknown";
        //                                    } else {
        //                                        ModifiedDateString = encodeText(ModifiedDate);
        //                                    }
        //                                    if (string.IsNullOrEmpty(ModifiedName)) {
        //                                        ModifiedName = "unknown";
        //                                    }
        //                                    if (string.IsNullOrEmpty(SubmitName)) {
        //                                        SubmitName = "unknown";
        //                                    }
        //                                    if (string.IsNullOrEmpty(ApprovedName)) {
        //                                        ApprovedName = "unknown";
        //                                    }
        //                                    if (IsInserted) {
        //                                        Body += ("Added: " + ModifiedDateString + " by " + ModifiedName + "" + BR + "");
        //                                    } else if (IsDeleted) {
        //                                        Body += ("Deleted: " + ModifiedDateString + " by " + ModifiedName + "" + BR + "");
        //                                    } else {
        //                                        Body += ("Modified: " + ModifiedDateString + " by " + ModifiedName + "" + BR + "");
        //                                    }
        //                                    if (IsSubmitted) {
        //                                        if (SubmittedDate == DateTime.MinValue) {
        //                                            SubmittedDateString = "unknown";
        //                                        } else {
        //                                            SubmittedDateString = encodeText(SubmittedDate);
        //                                        }
        //                                        Body += ("Submitted: " + SubmittedDateString + " by " + SubmitName + "" + BR + "");
        //                                    }
        //                                    if (IsApproved) {
        //                                        if (ApprovedDate == DateTime.MinValue) {
        //                                            ApprovedDateString = "unknown";
        //                                        } else {
        //                                            ApprovedDateString = encodeText(ApprovedDate);
        //                                        }
        //                                        Body += ("Approved: " + ApprovedDate + " by " + ApprovedName + "" + BR + "");
        //                                    }
        //                                    //Body &=  ("Admin Site: <a href=""?" & RequestNameAdminForm & "=" & AdminFormEdit & "&cid=" & ContentID & "&id=" & RecordID & "&" & RequestNameAdminDepth & "=1"" target=""_blank"">Open in New Window</a>" & br & "")
        //                                    //Body &=  ("Public Site: " & Link & "" & br & "")
        //                                    //
        //                                    Body += ("</td>");
        //                                    //
        //                                    Body += ("\n</tr>");
        //                                    RecordCount = RecordCount + 1;
        //                                }
        //                            }
        //                            core.db.csClose(ref CSAuthoringRecord);
        //                        }
        //                    }
        //                    core.db.csGoNext(CS);
        //                }
        //                //
        //                // --- print out the stuff at the bottom
        //                //
        //                RecordNext = RecordTop;
        //                if (core.db.csOk(CS)) {
        //                    RecordNext = RecordCount;
        //                }
        //                RecordPrevious = RecordTop - RecordsPerPage;
        //                if (RecordPrevious < 0) {
        //                    RecordPrevious = 0;
        //                }
        //            }
        //            core.db.csClose(ref CS);
        //            if (RecordCount == 0) {
        //                //
        //                // No records printed
        //                //
        //                Body += "\r<tr><td width=\"100%\" colspan=\"9\" class=\"ccAdminSmall\" style=\"padding-top:10px;\">There are no modified records to review</td></tr>";
        //            } else {
        //                Body += "\r<tr><td width=\"100%\" colspan=\"9\" class=\"ccAdminSmall\" style=\"padding-top:10px;\">* To view these records on the public site you must enable Rendering Mode because they are new records that have not been published.</td></tr>";
        //                Body += "\r<tr><td width=\"100%\" colspan=\"9\" class=\"ccAdminSmall\">** To view these records on the public site you must disable Rendering Mode because they are deleted records that have not been published.</td></tr>";
        //            }
        //            Body += "\r</table>";
        //            Body += htmlController.inputHidden("RowCnt", RecordCount);
        //            Body = "<div style=\"Background-color:white;\">" + Body + "</div>";
        //            //
        //            // Headers, etc
        //            //
        //            ButtonList = "";
        //            if (adminContext.ignore_legacyMenuDepth > 0) {
        //                ButtonList = ButtonList + "," + ButtonClose;
        //            } else {
        //                ButtonList = ButtonList + "," + ButtonCancel;
        //            }
        //            //ButtonList = ButtonList & "," & ButtonWorkflowPublishApproved & "," & ButtonWorkflowPublishSelected
        //            ButtonList = ButtonList.Substring(1);
        //            //
        //            // Assemble Page
        //            //
        //            Body += htmlController.inputHidden(rnAdminSourceForm, AdminFormPublishing);
        //        }
        //        //
        //        Caption = SpanClassAdminNormal + "<strong>Workflow Publishing</strong></span>";
        //        Description = "Monitor and Approve Workflow Publishing Changes";
        //        if (RecordCount >= 100) {
        //            Description = Description + BR + BR + "Only the first 100 record are displayed";
        //        }
        //        tempGetForm_Publish = adminUIController.GetBody(core, Caption, ButtonList, "", true, true, Description, "", 0, Body);
        //        core.html.addTitle("Workflow Publishing");
        //    } catch (Exception ex) {
        //        logController.handleError(core, ex);
        //    }
        //    return tempGetForm_Publish;
        //}
        //
        //========================================================================
        //   Generate the content of a tab in the Edit Screen
        //========================================================================
        //
        private string GetForm_Edit_Tab(AdminInfoDomainModel adminContext, int RecordID, int ContentID, bool record_readOnly, bool IsLandingPage, bool IsRootPage, string EditTab, ContentTypeEnum EditorContext, ref string return_NewFieldList, int TemplateIDForStyles, int HelpCnt, int[] HelpIDCache, string[] helpDefaultCache, string[] HelpCustomCache, bool AllowHelpMsgCustom, KeyPtrController helpIdIndex, string[] fieldTypeDefaultEditors, string fieldEditorPreferenceList, string styleList, string styleOptionList, int emailIdForStyles, bool IsTemplateTable, string editorAddonListJSON) {
            string returnHtml = "";
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
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
                string WhyReadOnlyMsg = null;
                bool IsLongHelp = false;
                bool IsEmptyHelp = false;
                string HelpMsg = null;
                int CS = 0;
                string HelpClosedContentID = null;
                string EditorHelp = null;
                string HelpEditorID = null;
                string HelpOpenedReadID = null;
                string HelpOpenedEditID = null;
                string HelpClosedID = null;
                string HelpID = null;
                string HelpMsgClosed = null;
                string HelpMsgOpenedRead = null;
                string HelpMsgOpenedEdit = null;
                //string RecordName = null;
                //string GroupName = null;
                bool IsBaseField = false;
                bool field_readOnly = false;
                string NonEncodedLink = null;
                string EncodedLink = null;
                string fieldCaption = null;
                //string[] lookups = null;
                //int CSPointer = 0;
                string fieldValue_text = null;
                //int FieldValueInteger = 0;
                double FieldValueNumber = 0;
                int fieldTypeId = 0;
                object fieldValue_object = null;
                string RedirectPath = null;
                StringBuilderLegacyController resultBody = new StringBuilderLegacyController();
                int FieldRows = 0;
                string EditorString = null;
                //string MTMContent0 = null;
                //string MTMContent1 = null;
                //string MTMRuleContent = null;
                //string MTMRuleField0 = null;
                //string MTMRuleField1 = null;
                string AlphaSort = null;
                bool needUniqueEmailMessage = false;
                //
                // ----- Open the panel
                if (adminContext.adminContent.fields.Count <= 0) {
                    //
                    // There are no visible fiels, return empty
                    LogController.handleError(core, new ApplicationException("There is no metadata for this field."));
                } else {
                    //
                    // ----- Build an index to sort the fields by EditSortOrder
                    Dictionary<string, CDefFieldModel> sortingFields = new Dictionary<string, CDefFieldModel>();
                    foreach (var keyValuePair in adminContext.adminContent.fields) {
                        CDefFieldModel field = keyValuePair.Value;
                        if (field.editTabName.ToLower() == EditTab.ToLower()) {
                            if (IsVisibleUserField(field.adminOnly, field.developerOnly, field.active, field.authorable, field.nameLc, adminContext.adminContent.tableName)) {
                                AlphaSort = GenericController.getIntegerString(field.editSortPriority, 10) + "-" + GenericController.getIntegerString(field.id, 10);
                                sortingFields.Add(AlphaSort, field);
                            }
                        }
                    }
                    //
                    // ----- display the record fields
                    //
                    AllowHelpIcon = core.visitProperty.getBoolean("AllowHelpIcon");
                    foreach (var kvp in sortingFields) {
                        CDefFieldModel field = kvp.Value;
                        fieldId = field.id;
                        WhyReadOnlyMsg = "";
                        fieldTypeId = field.fieldTypeId;
                        fieldValue_object = editRecord.fieldsLc[field.nameLc].value;
                        fieldValue_text = GenericController.encodeText(fieldValue_object);
                        FieldRows = 1;
                        string fieldHtmlId = field.nameLc + field.id.ToString();
                        //
                        fieldCaption = field.caption;
                        if (field.uniqueName) {
                            fieldCaption = "&nbsp;**" + fieldCaption;
                        } else {
                            if (field.nameLc.ToLower() == "email") {
                                if ((adminContext.adminContent.tableName.ToLower() == "ccmembers") && ((core.siteProperties.getBoolean("allowemaillogin", false)))) {
                                    fieldCaption = "&nbsp;***" + fieldCaption;
                                    needUniqueEmailMessage = true;
                                }
                            }
                        }
                        if (field.required) {
                            fieldCaption = "&nbsp;*" + fieldCaption;
                        }
                        IsBaseField = field.blockAccess; // field renamed
                        adminContext.FormInputCount = adminContext.FormInputCount + 1;
                        field_readOnly = false;
                        //
                        // Read only Special Cases
                        //
                        if (IsLandingPage) {
                            switch (GenericController.vbLCase(field.nameLc)) {
                                case "active":
                                    //
                                    // if active, it is read only -- if inactive, let them set it active.
                                    //
                                    field_readOnly = (GenericController.encodeBoolean(fieldValue_object));
                                    if (field_readOnly) {
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
                                    field_readOnly = true;
                                    WhyReadOnlyMsg = "&nbsp;(disabled for the landing page)";
                                    break;
                            }
                        }
                        //
                        if (IsRootPage) {
                            switch (GenericController.vbLCase(field.nameLc)) {
                                case "dateexpires":
                                case "pubdate":
                                case "datearchive":
                                case "archiveparentid":
                                    field_readOnly = true;
                                    WhyReadOnlyMsg = "&nbsp;(disabled for root pages)";
                                    break;
                                case "allowinmenus":
                                case "allowinchildlists":
                                    //FieldValueBoolean = true;
                                    fieldValue_object = "1";
                                    field_readOnly = true;
                                    WhyReadOnlyMsg = "&nbsp;(disabled for root pages)";
                                    break;
                            }
                        }
                        //
                        // Special Case - ccemail table Alloweid should be disabled if siteproperty AllowLinkLogin is false
                        //
                        if (GenericController.vbLCase(adminContext.adminContent.tableName) == "ccemail" && GenericController.vbLCase(field.nameLc) == "allowlinkeid") {
                            if (!(core.siteProperties.getBoolean("AllowLinkLogin", true))) {
                                //.ValueVariant = "0"
                                fieldValue_object = "0";
                                field_readOnly = true;
                                //FieldValueBoolean = false;
                                fieldValue_text = "0";
                            }
                        }
                        //EditorStyleModifier = genericController.vbLCase(core.db.getFieldTypeNameFromFieldTypeId(fieldTypeId));
                        EditorString = "";
                        editorReadOnly = (record_readOnly || field.readOnly | (editRecord.id != 0 & field.notEditable) || (field_readOnly));
                        //
                        // Determine the editor: Contensive editor, field type default, or add-on preference
                        //
                        editorAddonID = 0;
                        //editorPreferenceAddonId = 0
                        fieldIdPos = GenericController.vbInstr(1, "," + fieldEditorPreferenceList, "," + fieldId.ToString() + ":");
                        while ((editorAddonID == 0) && (fieldIdPos > 0)) {
                            fieldIdPos = fieldIdPos + 1 + fieldId.ToString().Length;
                            Pos = GenericController.vbInstr(fieldIdPos, fieldEditorPreferenceList + ",", ",");
                            if (Pos > 0) {
                                editorAddonID = GenericController.encodeInteger(fieldEditorPreferenceList.Substring(fieldIdPos - 1, Pos - fieldIdPos));
                                //editorPreferenceAddonId = genericController.EncodeInteger(Mid(fieldEditorPreferenceList, fieldIdPos, Pos - fieldIdPos))
                                //editorAddonID = editorPreferenceAddonId
                            }
                            fieldIdPos = GenericController.vbInstr(fieldIdPos + 1, "," + fieldEditorPreferenceList, "," + fieldId.ToString() + ":");
                        }
                        if (editorAddonID == 0) {
                            fieldTypeDefaultEditorAddonId = GenericController.encodeInteger(fieldTypeDefaultEditors[fieldTypeId]);
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
                            core.docProperties.setProperty("editorName", field.nameLc);
                            core.docProperties.setProperty("editorValue", fieldValue_text);
                            core.docProperties.setProperty("editorFieldId", fieldId);
                            core.docProperties.setProperty("editorFieldType", fieldTypeId);
                            core.docProperties.setProperty("editorReadOnly", editorReadOnly);
                            core.docProperties.setProperty("editorWidth", "");
                            core.docProperties.setProperty("editorHeight", "");
                            if (GenericController.encodeBoolean((fieldTypeId == FieldTypeIdHTML) || (fieldTypeId == FieldTypeIdFileHTML))) {
                                //
                                // include html related arguments
                                core.docProperties.setProperty("editorAllowActiveContent", "1");
                                core.docProperties.setProperty("editorAddonList", editorAddonListJSON);
                                core.docProperties.setProperty("editorStyles", styleList);
                                core.docProperties.setProperty("editorStyleOptions", styleOptionList);
                            }
                            EditorString = core.addon.execute(editorAddonID, new BaseClasses.CPUtilsBaseClass.addonExecuteContext {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextEditor,
                                errorContextMessage = "field editor id:" + editorAddonID
                            });
                            useEditorAddon = !string.IsNullOrEmpty(EditorString);
                            if (useEditorAddon) {
                                //
                                // -- editor worked
                                return_NewFieldList += "," + field.nameLc;
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
                                    string tmpList = core.userProperty.getText("editorPreferencesForContent:" + adminContext.adminContent.id, "");
                                    int PosStart = GenericController.vbInstr(1, "," + tmpList, "," + fieldId + ":");
                                    if (PosStart > 0) {
                                        int PosEnd = GenericController.vbInstr(PosStart + 1, "," + tmpList, ",");
                                        if (PosEnd == 0) {
                                            tmpList = tmpList.Left(PosStart - 1);
                                        } else {
                                            tmpList = tmpList.Left(PosStart - 1) + tmpList.Substring(PosEnd - 1);
                                        }
                                        core.userProperty.setProperty("editorPreferencesForContent:" + adminContext.adminContent.id, tmpList);
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
                                //
                                // ----- Default Editor, Redirect fields (the same for normal/readonly/spelling)
                                RedirectPath = core.appConfig.adminRoute;
                                if (field.redirectPath != "") {
                                    RedirectPath = field.redirectPath;
                                }
                                RedirectPath = RedirectPath + "?" + RequestNameTitleExtension + "=" + GenericController.encodeRequestVariable(" For " + editRecord.nameLc + adminContext.TitleExtension) + "&" + RequestNameAdminDepth + "=" + (adminContext.ignore_legacyMenuDepth + 1) + "&wl0=" + field.redirectID + "&wr0=" + editRecord.id;
                                if (field.redirectContentID != 0) {
                                    RedirectPath = RedirectPath + "&cid=" + field.redirectContentID;
                                } else {
                                    RedirectPath = RedirectPath + "&cid=" + ((editRecord.contentControlId.Equals(0)) ? adminContext.adminContent.id : editRecord.contentControlId);
                                }
                                if (editRecord.id == 0) {
                                    EditorString += ("[available after save]");
                                } else {
                                    RedirectPath = GenericController.vbReplace(RedirectPath, "'", "\\'");
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
                                //EditorStyleModifier = "";
                                switch (fieldTypeId) {
                                    case FieldTypeIdText:
                                    case FieldTypeIdLink:
                                    case FieldTypeIdResourceLink:
                                        //
                                        // ----- Text Type
                                        EditorString += AdminUIController.getDefaultEditor_Text(core, field.nameLc, fieldValue_text, true, fieldHtmlId);
                                        return_NewFieldList += "," + field.nameLc;
                                        break;
                                    case FieldTypeIdBoolean:
                                        //
                                        // ----- Boolean ReadOnly
                                        EditorString += AdminUIController.getDefaultEditor_Bool(core, field.nameLc, GenericController.encodeBoolean(fieldValue_object), true, fieldHtmlId);
                                        return_NewFieldList += "," + field.nameLc;
                                        break;
                                    case FieldTypeIdLookup:
                                        //
                                        // ----- Lookup, readonly
                                        if (field.lookupContentID != 0) {
                                            EditorString = AdminUIController.getDefaultEditor_LookupContent(core, field.nameLc, encodeInteger(fieldValue_object), field.lookupContentID, ref IsEmptyList, field.readOnly, fieldHtmlId, WhyReadOnlyMsg, field.required);
                                            return_NewFieldList += "," + field.nameLc;
                                        } else if (field.lookupList != "") {
                                            EditorString = AdminUIController.getDefaultEditor_LookupList(core, field.nameLc, encodeInteger(fieldValue_object), field.lookupList.Split(','), field.readOnly, fieldHtmlId, WhyReadOnlyMsg, field.required);
                                            return_NewFieldList += "," + field.nameLc;
                                        } else {
                                            //
                                            // -- log exception but dont throw
                                            LogController.handleWarn(core, new ApplicationException("Field [" + adminContext.adminContent.name + "." + field.nameLc + "] is a Lookup field, but no LookupContent or LookupList has been configured"));
                                            EditorString += "[Selection not configured]";
                                        }
                                        break;
                                    case FieldTypeIdDate:
                                        //
                                        // ----- date, readonly
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = AdminUIController.getDefaultEditor_Date(core, field.nameLc, GenericController.encodeDate(fieldValue_object), field.readOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                        break;
                                    case FieldTypeIdMemberSelect:
                                        //
                                        // ----- Member Select ReadOnly
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = AdminUIController.getDefaultEditor_memberSelect(core, field.nameLc, encodeInteger(fieldValue_object), field.memberSelectGroupId_get(core), field.memberSelectGroupName_get(core), field.readOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                        //
                                        break;
                                    case FieldTypeIdManyToMany:
                                        //
                                        //   Placeholder
                                        //
                                        EditorString = AdminUIController.getDefaultEditor_manyToMany(core, field, "field" + field.id, fieldValue_text, editRecord.id, editorReadOnly, WhyReadOnlyMsg);
                                        //MTMContent0 = CdefController.getContentNameByID(core, field.contentId);
                                        //MTMContent1 = CdefController.getContentNameByID(core, field.manyToManyContentID);
                                        //MTMRuleContent = CdefController.getContentNameByID(core, field.manyToManyRuleContentID);
                                        //MTMRuleField0 = field.ManyToManyRulePrimaryField;
                                        //MTMRuleField1 = field.ManyToManyRuleSecondaryField;
                                        //EditorString += core.html.getCheckList("ManyToMany" + field.id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1);
                                        //EditorString += WhyReadOnlyMsg;
                                        ////
                                        break;
                                    case FieldTypeIdCurrency:
                                        //
                                        // ----- Currency ReadOnly
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        FieldValueNumber = GenericController.encodeNumber(fieldValue_object);
                                        EditorString += (HtmlController.inputHidden(field.nameLc, GenericController.encodeText(FieldValueNumber)));
                                        EditorString += (HtmlController.inputText(core, field.nameLc, FieldValueNumber.ToString(), -1, -1, "", false, true, "text form-control"));
                                        EditorString += (string.Format("{0:C}", FieldValueNumber));
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    case FieldTypeIdAutoIdIncrement:
                                    case FieldTypeIdFloat:
                                    case FieldTypeIdInteger:
                                        //
                                        // ----- number readonly
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString += (HtmlController.inputHidden(field.nameLc, fieldValue_text));
                                        EditorString += (HtmlController.inputText(core, field.nameLc, fieldValue_text, -1, -1, "", false, true, "number form-control"));
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    case FieldTypeIdHTML:
                                    case FieldTypeIdFileHTML:
                                        //
                                        // ----- HTML types readonly
                                        //
                                        if (field.htmlContent) {
                                            //
                                            // edit html as html (see the code)
                                            //
                                            return_NewFieldList += "," + field.nameLc;
                                            EditorString += HtmlController.inputHidden(field.nameLc, fieldValue_text);
                                            //EditorStyleModifier = "textexpandable";
                                            FieldRows = (core.userProperty.getInteger(adminContext.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                            EditorString += HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, field.nameLc, false, true, "form-control");
                                        } else {
                                            //
                                            // edit html as wysiwyg readonly
                                            //
                                            return_NewFieldList += "," + field.nameLc;
                                            EditorString += AdminUIController.getDefaultEditor_Html(core, field.nameLc, fieldValue_text, editorAddonListJSON, styleList, styleOptionList, true);
                                        }
                                        break;
                                    case FieldTypeIdLongText:
                                    case FieldTypeIdFileText:
                                        //
                                        // ----- LongText, TextFile
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString += HtmlController.inputHidden(field.nameLc, fieldValue_text);
                                        //EditorStyleModifier = "textexpandable";
                                        FieldRows = (core.userProperty.getInteger(adminContext.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                        EditorString += HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, field.nameLc, false, true, " form-control");
                                        break;
                                    case FieldTypeIdFile:
                                    case FieldTypeIdFileImage:
                                        //
                                        // ----- File ReadOnly
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        NonEncodedLink = GenericController.getCdnFileLink(core, fieldValue_text);
                                        //NonEncodedLink = core.webServer.requestDomain + genericController.getCdnFileLink(core, FieldValueText);
                                        EncodedLink = GenericController.encodeURL(NonEncodedLink);
                                        EditorString += (HtmlController.inputHidden(field.nameLc, ""));
                                        if (string.IsNullOrEmpty(fieldValue_text)) {
                                            EditorString += ("[no file]");
                                        } else {
                                            string filename = "";
                                            string path = "";
                                            core.cdnFiles.splitDosPathFilename(fieldValue_text, ref path, ref filename);
                                            EditorString += ("&nbsp;<a href=\"http://" + EncodedLink + "\" target=\"_blank\">" + SpanClassAdminSmall + "[" + filename + "]</A>");
                                        }
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    default:
                                        //
                                        // ----- Legacy text type -- not used unless something was missed
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString += HtmlController.inputHidden(field.nameLc, fieldValue_text);
                                        if (field.password) {
                                            //
                                            // Password forces simple text box
                                            //
                                            EditorString += HtmlController.inputText(core, field.nameLc, "*****", 0, 0, "", true, true, "password form-control");
                                        } else if (!field.htmlContent) {
                                            //
                                            // not HTML capable, textarea with resizing
                                            //
                                            if ((fieldTypeId == FieldTypeIdText) && (fieldValue_text.IndexOf("\n") == -1) && (fieldValue_text.Length < 40)) {
                                                //
                                                // text field shorter then 40 characters without a CR
                                                //
                                                EditorString += HtmlController.inputText(core, field.nameLc, fieldValue_text, 1, 0, "", false, true, "text form-control");
                                            } else {
                                                //
                                                // longer text data, or text that contains a CR
                                                //
                                                //EditorStyleModifier = "textexpandable";
                                                EditorString += HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, 10, -1, "", false, true, " form-control");
                                            }
                                        } else if (field.htmlContent) {
                                            //
                                            // HTMLContent true, and prefered
                                            //
                                            //EditorStyleModifier = "text";
                                            FieldRows = (core.userProperty.getInteger(adminContext.adminContent.name + "." + field.nameLc + ".PixelHeight", 500));
                                            EditorString += core.html.getFormInputHTML(field.nameLc, fieldValue_text, "500", "", false, true, editorAddonListJSON, styleList, styleOptionList);
                                            //innovaEditor = New innovaEditorAddonClassFPO
                                            //EditorString &=  innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, True, TemplateIDForStyles, emailIdForStyles)
                                            EditorString = "<div style=\"width:95%\">" + EditorString + "</div>";
                                        } else {
                                            //
                                            // HTMLContent true, but text editor selected
                                            //
                                            //EditorStyleModifier = "textexpandable";
                                            FieldRows = (core.userProperty.getInteger(adminContext.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                            EditorString += HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, field.nameLc, false, true);
                                            //EditorString = core.main_GetFormInputTextExpandable(FormFieldLCaseName, encodeHTML(FieldValueText), FieldRows, "600px", FormFieldLCaseName, False)
                                        }
                                        break;
                                }
                            } else {
                                //
                                // -- Not Read Only - Display fields as form elements to be modified
                                switch (fieldTypeId) {
                                    case FieldTypeIdText:
                                        //
                                        // ----- Text Type
                                        if (field.password) {
                                            EditorString += AdminUIController.getDefaultEditor_Password(core, field.nameLc, fieldValue_text, false, fieldHtmlId);
                                        } else {
                                            EditorString += AdminUIController.getDefaultEditor_Text(core, field.nameLc, fieldValue_text, false, fieldHtmlId);
                                        }
                                        return_NewFieldList += "," + field.nameLc;
                                        break;
                                    case FieldTypeIdBoolean:
                                        //
                                        // ----- Boolean
                                        EditorString += AdminUIController.getDefaultEditor_Bool(core, field.nameLc, GenericController.encodeBoolean(fieldValue_object), false, fieldHtmlId);
                                        return_NewFieldList += "," + field.nameLc;
                                        break;
                                    case FieldTypeIdLookup:
                                        //
                                        // ----- Lookup
                                        if (field.lookupContentID != 0) {
                                            EditorString = AdminUIController.getDefaultEditor_LookupContent(core, field.nameLc, encodeInteger(fieldValue_object), field.lookupContentID, ref IsEmptyList, field.readOnly, fieldHtmlId, WhyReadOnlyMsg, field.required);
                                            return_NewFieldList += "," + field.nameLc;
                                        } else if (field.lookupList != "") {
                                            EditorString = AdminUIController.getDefaultEditor_LookupList(core, field.nameLc, encodeInteger(fieldValue_object), field.lookupList.Split(','), field.readOnly, fieldHtmlId, WhyReadOnlyMsg, field.required);
                                            return_NewFieldList += "," + field.nameLc;
                                        } else {
                                            //
                                            // -- log exception but dont throw
                                            LogController.handleWarn(core, new ApplicationException("Field [" + adminContext.adminContent.name + "." + field.nameLc + "] is a Lookup field, but no LookupContent or LookupList has been configured"));
                                            EditorString += "[Selection not configured]";
                                        }
                                        break;
                                    case FieldTypeIdDate:
                                        //
                                        // ----- Date
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = AdminUIController.getDefaultEditor_Date(core, field.nameLc, GenericController.encodeDate(fieldValue_object), field.readOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                        break;
                                    case FieldTypeIdMemberSelect:
                                        //
                                        // ----- Member Select
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = AdminUIController.getDefaultEditor_memberSelect(core, field.nameLc, encodeInteger(fieldValue_object), field.memberSelectGroupId_get(core), field.memberSelectGroupName_get(core), field.readOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                        break;
                                    case FieldTypeIdManyToMany:
                                        //
                                        //   Placeholder
                                        EditorString = AdminUIController.getDefaultEditor_manyToMany(core, field, "field" + field.id, fieldValue_text, editRecord.id, false, WhyReadOnlyMsg);
                                        //MTMContent0 = CdefController.getContentNameByID(core, field.contentId);
                                        //MTMContent1 = CdefController.getContentNameByID(core, field.manyToManyContentID);
                                        //MTMRuleContent = CdefController.getContentNameByID(core, field.manyToManyRuleContentID);
                                        //MTMRuleField0 = field.ManyToManyRulePrimaryField;
                                        //MTMRuleField1 = field.ManyToManyRuleSecondaryField;
                                        //EditorString += core.html.getCheckList("ManyToMany" + field.id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, "", "", false, false, fieldValue_text);
                                        ////EditorString &= (core.html.getInputCheckListCategories("ManyToMany" & .id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , False, MTMContent1, FieldValueText))
                                        break;
                                    case FieldTypeIdFile:
                                    case FieldTypeIdFileImage:
                                        //
                                        // ----- File
                                        return_NewFieldList += "," + field.nameLc;
                                        if (string.IsNullOrEmpty(fieldValue_text)) {
                                            EditorString += (core.html.inputFile(field.nameLc, "", "file form-control"));
                                        } else {
                                            NonEncodedLink = GenericController.getCdnFileLink(core, fieldValue_text);
                                            //NonEncodedLink = core.webServer.requestDomain + genericController.getCdnFileLink(core, FieldValueText);
                                            EncodedLink = HtmlController.encodeHtml(NonEncodedLink);
                                            string filename = "";
                                            string path = "";
                                            core.cdnFiles.splitDosPathFilename(fieldValue_text, ref path, ref filename);
                                            EditorString += ("&nbsp;<a href=\"" + EncodedLink + "\" target=\"_blank\">" + SpanClassAdminSmall + "[" + filename + "]</A>");
                                            EditorString += ("&nbsp;&nbsp;&nbsp;Delete:&nbsp;" + HtmlController.checkbox(field.nameLc + ".DeleteFlag", false));
                                            EditorString += ("&nbsp;&nbsp;&nbsp;Change:&nbsp;" + core.html.inputFile(field.nameLc, "", "file form-control"));
                                        }
                                        //
                                        break;
                                    case FieldTypeIdAutoIdIncrement:
                                    case FieldTypeIdCurrency:
                                    case FieldTypeIdFloat:
                                    case FieldTypeIdInteger:
                                        //
                                        // ----- Others that simply print
                                        return_NewFieldList += "," + field.nameLc;
                                        if (field.password) {
                                            EditorString += (HtmlController.inputText(core, field.nameLc, fieldValue_text, -1, -1, "", true, false, "password form-control"));
                                        } else {
                                            if (string.IsNullOrEmpty(fieldValue_text)) {
                                                EditorString += (HtmlController.inputText(core, field.nameLc, "", -1, -1, "", false, false, "text form-control"));
                                            } else {
                                                if (encodeBoolean(fieldValue_text.IndexOf("\n") + 1) || (fieldValue_text.Length > 40)) {
                                                    EditorString += (HtmlController.inputText(core, field.nameLc, fieldValue_text, -1, -1, "", false, false, "text form-control"));
                                                } else {
                                                    EditorString += (HtmlController.inputText(core, field.nameLc, fieldValue_text, 1, -1, "", false, false, "text form-control"));
                                                }
                                            }
                                        }
                                        break;
                                    case FieldTypeIdLink:
                                        //
                                        // ----- Link (href value
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = ""
                                            + HtmlController.inputText(core, field.nameLc, fieldValue_text, 1, 80, field.nameLc, false, false, "link form-control") + "&nbsp;<a href=\"#\" onClick=\"OpenResourceLinkWindow( '" + field.nameLc + "' ) ;return false;\"><img src=\"/ccLib/images/ResourceLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a resource\" title=\"Link to a resource\"></a>"
                                            + "&nbsp;<a href=\"#\" onClick=\"OpenSiteExplorerWindow( '" + field.nameLc + "' ) ;return false;\"><img src=\"/ccLib/images/PageLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a page\" title=\"Link to a page\"></a>";
                                        break;
                                    case FieldTypeIdResourceLink:
                                        //
                                        // ----- Resource Link (src value)
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = HtmlController.inputText(core, field.nameLc, fieldValue_text, 1, 80, field.nameLc, false, false, "resourceLink form-control") + "&nbsp;<a href=\"#\" onClick=\"OpenResourceLinkWindow( '" + field.nameLc + "' ) ;return false;\"><img src=\"/ccLib/images/ResourceLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a resource\" title=\"Link to a resource\"></a>";
                                        break;
                                    case FieldTypeIdHTML:
                                    case FieldTypeIdFileHTML:
                                        //
                                        // content is html
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        if (field.htmlContent) {
                                            //
                                            // View the content as Html, not wysiwyg
                                            EditorString = AdminUIController.getDefaultEditor_TextArea(core, field.nameLc, fieldValue_text, editorReadOnly);
                                            //EditorString = htmlController.inputTextarea( core,field.nameLc, fieldValue_text, 10, -1, "", false, false, "text form-control");
                                        } else {
                                            //
                                            // wysiwyg editor
                                            EditorString = AdminUIController.getDefaultEditor_Html(core, field.nameLc, fieldValue_text, editorAddonListJSON, styleList, styleOptionList, editorReadOnly);

                                            //if (string.IsNullOrEmpty(fieldValue_text)) {
                                            //    //
                                            //    // editor needs a starting p tag to setup correctly
                                            //    fieldValue_text = HTMLEditorDefaultCopyNoCr;
                                            //}
                                            //FieldRows = (core.userProperty.getInteger(adminContext.content.name + "." + field.nameLc + ".PixelHeight", 500));
                                            //EditorString += core.html.getFormInputHTML(field.nameLc, fieldValue_text, "500", "", false, true, editorAddonListJSON, styleList, styleOptionList);
                                            //EditorString = "<div style=\"width:95%\">" + EditorString + "</div>";
                                        }
                                        //
                                        break;
                                    case FieldTypeIdLongText:
                                    case FieldTypeIdFileText:
                                        //
                                        // -- Long Text, use text editor
                                        return_NewFieldList += "," + field.nameLc;
                                        FieldRows = (core.userProperty.getInteger(adminContext.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                        EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, field.nameLc, false, false, "text form-control");
                                        //
                                        break;
                                    case FieldTypeIdFileCSS:
                                        //
                                        // ----- CSS field
                                        return_NewFieldList += "," + field.nameLc;
                                        FieldRows = (core.userProperty.getInteger(adminContext.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                        EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, 10, -1, "", false, false, "styles form-control");
                                        break;
                                    case FieldTypeIdFileJavascript:
                                        //
                                        // ----- Javascript field
                                        return_NewFieldList += "," + field.nameLc;
                                        FieldRows = (core.userProperty.getInteger(adminContext.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                        EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, field.nameLc, false, false, "text form-control");
                                        //
                                        break;
                                    case FieldTypeIdFileXML:
                                        //
                                        // ----- xml field
                                        return_NewFieldList += "," + field.nameLc;
                                        FieldRows = (core.userProperty.getInteger(adminContext.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                        EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, field.nameLc, false, false, "text form-control");
                                        //
                                        break;
                                    default:
                                        //
                                        // ----- Legacy text type -- not used unless something was missed
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        if (field.password) {
                                            //
                                            // Password forces simple text box
                                            EditorString = HtmlController.inputText(core, field.nameLc, fieldValue_text, -1, -1, "", true, false, "password form-control");
                                        } else if (!field.htmlContent) {
                                            //
                                            // not HTML capable, textarea with resizing
                                            //
                                            if ((fieldTypeId == FieldTypeIdText) && (fieldValue_text.IndexOf("\n") == -1) && (fieldValue_text.Length < 40)) {
                                                //
                                                // text field shorter then 40 characters without a CR
                                                //
                                                EditorString = HtmlController.inputText(core, field.nameLc, fieldValue_text, 1, -1, "", false, false, "text form-control");
                                            } else {
                                                //
                                                // longer text data, or text that contains a CR
                                                //
                                                //EditorStyleModifier = "textexpandable";
                                                EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, 10, -1, "", false, false, "text form-control");
                                            }
                                        } else if (field.htmlContent) {
                                            //
                                            // HTMLContent true, and prefered
                                            //
                                            if (string.IsNullOrEmpty(fieldValue_text)) {
                                                //
                                                // editor needs a starting p tag to setup correctly
                                                //
                                                fieldValue_text = HTMLEditorDefaultCopyNoCr;
                                            }
                                            //EditorStyleModifier = "htmleditor";
                                            FieldRows = (core.userProperty.getInteger(adminContext.adminContent.name + "." + field.nameLc + ".PixelHeight", 500));
                                            EditorString += core.html.getFormInputHTML(field.nameLc, fieldValue_text, "500", "", false, true, editorAddonListJSON, styleList, styleOptionList);
                                            //innovaEditor = New innovaEditorAddonClassFPO
                                            //EditorString = innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, False, TemplateIDForStyles, emailIdForStyles)
                                            EditorString = "<div style=\"width:95%\">" + EditorString + "</div>";
                                        } else {
                                            //
                                            // HTMLContent true, but text editor selected
                                            //
                                            //EditorStyleModifier = "textexpandable";
                                            FieldRows = (core.userProperty.getInteger(adminContext.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                            EditorString = HtmlController.inputTextarea(core, field.nameLc, HtmlController.encodeHtml(fieldValue_text), FieldRows, -1, field.nameLc, false, false, "text");
                                        }
                                        //s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & "</span></nobr></td>")
                                        break;
                                }
                            }
                        }
                        //
                        // Build Help Line Below editor
                        //
                        adminContext.includeFancyBox = true;
                        HelpMsgDefault = "";
                        HelpMsgCustom = "";
                        EditorHelp = "";
                        LcaseName = GenericController.vbLCase(field.nameLc);
                        if (AllowHelpMsgCustom) {
                            HelpMsgDefault = field.helpDefault;
                            HelpMsgCustom = field.helpCustom;
                        }
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
                        //AllowHelpRow = true;
                        //
                        //------------------------------------------------------------------------------------------------------------
                        // editor preferences form - a fancybox popup that interfaces with a hardcoded ajax function in init() to set a member property
                        //------------------------------------------------------------------------------------------------------------
                        //
                        AjaxQS = RequestNameAjaxFunction + "=" + ajaxGetFieldEditorPreferenceForm + "&fieldid=" + fieldId + "&currentEditorAddonId=" + editorAddonID + "&fieldTypeDefaultEditorAddonId=" + fieldTypeDefaultEditorAddonId;
                        fancyBoxLinkId = "fbl" + adminContext.fancyBoxPtr;
                        fancyBoxContentId = "fbc" + adminContext.fancyBoxPtr;
                        adminContext.fancyBoxHeadJS = adminContext.fancyBoxHeadJS + "\r\njQuery('#" + fancyBoxLinkId + "').fancybox({"
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
                        adminContext.fancyBoxPtr = adminContext.fancyBoxPtr + 1;
                        //
                        //------------------------------------------------------------------------------------------------------------
                        // field help
                        //------------------------------------------------------------------------------------------------------------
                        //
                        if (core.session.isAuthenticatedAdmin(core)) {
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
                            string jsUpdate = "updateFieldHelp('" + fieldId + "','" + HelpEditorID + "','" + HelpClosedContentID + "');cj.hide('" + HelpOpenedEditID + "');cj.show('" + HelpClosedID + "');return false";
                            string jsCancel = "cj.hide('" + HelpOpenedEditID + "');cj.show('" + HelpClosedID + "');return false";
                            HelpMsgOpenedEdit = ""
                                    + "<div class=\"header\">Default Help</div>"
                                    + "<div class=\"body\">" + HelpMsgDefault + "</div>"
                                    + "<div class=\"header\">Custom Help</div>"
                                    + "<div class=\"body\"><textarea id=\"" + HelpEditorID + "\" ROWS=\"10\" style=\"width:100%;\">" + HelpMsgCustom + "</TEXTAREA></div>"
                                    + "<div class=\"\">"
                                        + AdminUIController.getButtonPrimary("Update", jsUpdate)
                                        + AdminUIController.getButtonPrimary("Cancel", jsCancel)
                                    + "</div>"
                                + "";
                            //HelpMsgOpenedEdit = ""
                            //    + "<div class=\"header\">Default Help</div>"
                            //    + "<div class=\"body\">" + HelpMsgDefault + "</div>"
                            //    + "<div class=\"header\">Custom Help</div>"
                            //    + "<div class=\"body\"><textarea id=\"" + HelpEditorID + "\" ROWS=\"10\" style=\"width:100%;\">" + HelpMsgCustom + "</TEXTAREA></div>"
                            //    + "<div class=\"\">"
                            //        + "<input type=\"submit\" name=\"button\" value=\"Update\" onClick=\"updateFieldHelp('" + fieldId + "','" + HelpEditorID + "','" + HelpClosedContentID + "');cj.hide('" + HelpOpenedEditID + "');cj.show('" + HelpClosedID + "');return false\">"
                            //        + "<input type=\"submit\" name=\"button\" value=\"Cancel\" onClick=\"cj.hide('" + HelpOpenedEditID + "');cj.show('" + HelpClosedID + "');return false\">"
                            //    + "</div>"
                            //+ "";
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
                                //AllowHelpRow = false;
                                HelpMsgClosed = ""
                                    + "";
                            }
                            EditorHelp = EditorHelp + "<div id=\"" + HelpOpenedReadID + "\" class=\"opened\">" + HelpMsgOpenedRead + "</div>"
                                + "<div id=\"" + HelpClosedID + "\" class=\"closed\">" + HelpMsgClosed + "</div>"
                                + "";
                        }
                        //
                        // assemble the editor row
                        //
                        //string fieldHelp = htmlController.div(htmlController.small(field.helpDefault), "form-text text-muted","emailHelp");
                        string editorRow = AdminUIController.getEditRow(core, EditorString, fieldCaption, field.helpDefault, field.required, false, fieldHtmlId);
                        resultBody.Add("<tr><td colspan=2>" + editorRow + "</td></tr>");
                    }
                    //
                    // ----- add the *Required Fields footer
                    //
                    resultBody.Add("<tr><td colspan=2 style=\"padding-top:10px;font-size:70%\"><div>* Field is required.</div><div>** Field must be unique.</div>");
                    if (needUniqueEmailMessage) {
                        resultBody.Add("<div>*** Field must be unique because this site allows login by email.</div>");
                    }
                    resultBody.Add("</td></tr>");
                    //
                    // ----- close the panel
                    //
                    if (string.IsNullOrEmpty(EditTab)) {
                        fieldCaption = "Content Fields";
                    } else {
                        fieldCaption = "Content Fields - " + EditTab;
                    }
                    adminContext.EditSectionPanelCount = adminContext.EditSectionPanelCount + 1;
                    returnHtml = AdminUIController.getEditPanel(core, (!adminContext.allowAdminTabs), fieldCaption, "", AdminUIController.editTableOpen + resultBody.Text + AdminUIController.editTableClose);
                    adminContext.EditSectionPanelCount = adminContext.EditSectionPanelCount + 1;
                    resultBody = null;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnHtml;
        }
        //
        //========================================================================
        //   Display field in the admin/edit
        //========================================================================
        //
        private string GetForm_Edit_ContentTracking(AdminInfoDomainModel adminContext) {
            string tempGetForm_Edit_ContentTracking = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
                //
                int CSRules = 0;
                string HTMLFieldString = null;
                int CSLists = 0;
                int RecordCount = 0;
                int ContentWatchListID = 0;
                StringBuilderLegacyController FastString = null;
                //string Copy = null;
                //adminUIController Adminui = new adminUIController(core);
                //
                if (adminContext.adminContent.allowContentTracking) {
                    FastString = new StringBuilderLegacyController();
                    //
                    if (!adminContext.ContentWatchLoaded) {
                        //
                        // ----- Load in the record to print
                        //
                        LoadContentTrackingDataBase(adminContext);
                        LoadContentTrackingResponse(adminContext);
                        //        Call LoadAndSaveCalendarEvents
                    }
                    CSLists = core.db.csOpen("Content Watch Lists", "name<>" + core.db.encodeSQLText(""), "ID");
                    if (core.db.csOk(CSLists)) {
                        //
                        // ----- Open the panel
                        //
                        //Call core.main_PrintPanelTop("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                        //Call FastString.Add(adminUIController.EditTableOpen)
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
                            if (adminContext.ContentWatchRecordID != 0) {
                                CSRules = core.db.csOpen("Content Watch List Rules", "(ContentWatchID=" + adminContext.ContentWatchRecordID + ")AND(ContentWatchListID=" + ContentWatchListID + ")");
                                if (editRecord.Read_Only) {
                                    HTMLFieldString = GenericController.encodeText(core.db.csOk(CSRules));
                                } else {
                                    HTMLFieldString = HtmlController.checkbox("ContentWatchList." + core.db.csGet(CSLists, "ID"), core.db.csOk(CSRules));
                                }
                                core.db.csClose(ref CSRules);
                            } else {
                                if (editRecord.Read_Only) {
                                    HTMLFieldString = GenericController.encodeText(false);
                                } else {
                                    HTMLFieldString = HtmlController.checkbox("ContentWatchList." + core.db.csGet(CSLists, "ID"), false);
                                }
                            }
                            //
                            FastString.Add(AdminUIController.getEditRowLegacy(core, HTMLFieldString, "Include in " + core.db.csGet(CSLists, "name"), "When true, this Content Record can be included in the '" + core.db.csGet(CSLists, "name") + "' list", false, false, ""));
                            core.db.csGoNext(CSLists);
                            RecordCount = RecordCount + 1;
                        }
                        //
                        // ----- Whats New Headline (editable)
                        //
                        if (editRecord.Read_Only) {
                            HTMLFieldString = HtmlController.encodeHtml(adminContext.ContentWatchLinkLabel);
                        } else {
                            HTMLFieldString = HtmlController.inputText(core, "ContentWatchLinkLabel", adminContext.ContentWatchLinkLabel, 1, core.siteProperties.defaultFormInputWidth);
                            //HTMLFieldString = "<textarea rows=""1"" name=""ContentWatchLinkLabel"" cols=""" & core.app.SiteProperty_DefaultFormInputWidth & """>" & ContentWatchLinkLabel & "</textarea>"
                        }
                        FastString.Add(AdminUIController.getEditRowLegacy(core, HTMLFieldString, "Caption", "This caption is displayed on all Content Watch Lists, linked to the location on the web site where this content is displayed. RSS feeds created from Content Watch Lists will use this caption as the record title if not other field is selected in the Content Definition.", false, true, "ContentWatchLinkLabel"));
                        //
                        // ----- Whats New Expiration
                        //
                        if (editRecord.Read_Only) {
                            HTMLFieldString = AdminUIController.getDefaultEditor_Date(core, "ContentWatchExpires", adminContext.ContentWatchExpires, true, "", false, "");
                        } else {
                            HTMLFieldString = AdminUIController.getDefaultEditor_Date(core, "ContentWatchExpires", adminContext.ContentWatchExpires, false, "", false, "");
                        }
                        FastString.Add(AdminUIController.getEditRowLegacy(core, HTMLFieldString, "Expires", "When this record is included in a What's New list, this record is blocked from the list after this date.", false, false, ""));
                        //
                        // ----- Public Link (read only)
                        //
                        HTMLFieldString = adminContext.ContentWatchLink;
                        if (string.IsNullOrEmpty(HTMLFieldString)) {
                            HTMLFieldString = "(must first be viewed on public site)";
                        }
                        FastString.Add(AdminUIController.getEditRowLegacy(core, HTMLFieldString, "Location on Site", "The public site URL where this content was last viewed.", false, false, ""));
                        //
                        // removed 11/27/07 - RSS clicks not counted, rc/ri method of counting link clicks is not reliable.
                        //            '
                        //            ' ----- Clicks (read only)
                        //            '
                        //            HTMLFieldString = ContentWatchClicks
                        //            If HTMLFieldString = "" Then
                        //                HTMLFieldString = 0
                        //                End If
                        //            Call FastString.Add(adminUIController.GetEditRow(core, HTMLFieldString, "Clicks", "The number of site users who have clicked this link in what's new lists", False, False, ""))
                        //
                        // ----- close the panel
                        //
                        string s = ""
                        + AdminUIController.editTableOpen + FastString.Text + AdminUIController.editTableClose + HtmlController.inputHidden("WhatsNewResponse", "-1") + HtmlController.inputHidden("contentwatchrecordid", adminContext.ContentWatchRecordID.ToString());
                        tempGetForm_Edit_ContentTracking = AdminUIController.getEditPanel(core, (!adminContext.allowAdminTabs), "Content Tracking", "Include in Content Watch Lists", s);
                        adminContext.EditSectionPanelCount = adminContext.EditSectionPanelCount + 1;
                        //
                    }
                    core.db.csClose(ref CSLists);
                    FastString = null;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_Edit_ContentTracking;
        }
        //
        //========================================================================
        //
        private string GetForm_Edit_Control(AdminInfoDomainModel adminContext) {
            string result = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
                var tabPanel = new StringBuilderLegacyController();
                if (string.IsNullOrEmpty(adminContext.adminContent.name)) {
                    //
                    // Content not found or not loaded
                    if (adminContext.adminContent.id == 0) {
                        //
                        handleLegacyClassError("GetForm_Edit_Control", "No content definition was specified for this page");
                        return HtmlController.p("No content was specified.");
                    } else {
                        //
                        // Content Definition was not specified
                        handleLegacyClassError("GetForm_Edit_Control", "The content definition specified for this page [" + adminContext.adminContent.id + "] was not found");
                        return HtmlController.p("No content was specified.");
                    }
                }
                //
                // ----- Authoring status
                bool FieldRequired = false;
                //
                // ----- RecordID
                {
                    string fieldValue = (editRecord.id == 0) ? "(available after save)" : editRecord.id.ToString();
                    string fieldEditor = AdminUIController.getDefaultEditor_Text(core, "ignore", fieldValue, true, "");
                    string fieldHelp = "This is the unique number that identifies this record within this content.";
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Record Number", fieldHelp, true, false, ""));
                }
                //
                // -- Active
                {
                    string fieldEditor = HtmlController.checkbox("active", editRecord.active);
                    string fieldHelp = "When unchecked, add-ons can ignore this record as if it was temporarily deleted.";
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Active", fieldHelp, false, false, ""));
                }
                ////
                //// ----- If Page Content , check if this is the default PageNotFound page
                //if (adminContext.adminContent.contentTableName.ToLower() == "ccpagecontent") {
                //    //
                //    // Landing Page
                //    {
                //        string fieldHelp = "If selected, this page will be displayed when a user comes to your website with just your domain name and no other page is requested. This is called your default Landing Page. Only one page on the site can be the default Landing Page. If you want a unique Landing Page for a specific domain name, add it in the 'Domains' content and the default will not be used for that docore.main_";
                //        bool Checked = ((editRecord.id != 0) && (editRecord.id == (core.siteProperties.getInteger("LandingPageID", 0))));
                //        string fieldEditor = (core.session.isAuthenticatedAdmin(core)) ? htmlController.checkbox("LandingPageID", Checked) : "<b>" + genericController.getYesNo(Checked) + "</b>" + htmlController.inputHidden("LandingPageID", Checked);
                //        tabPanel.Add(adminUIController.getEditRow(core, fieldEditor, "Default Landing Page", fieldHelp, false, false, ""));
                //    }
                //    //
                //    // Page Not Found
                //    {
                //        string fieldHelp = "If selected, this content will be displayed when a page can not be found. Only one page on the site can be marked.";
                //        bool isPageNotFoundRecord = ((editRecord.id != 0) && (editRecord.id == (core.siteProperties.getInteger("PageNotFoundPageID", 0))));
                //        string fieldEditor = (core.session.isAuthenticatedAdmin(core)) ? htmlController.checkbox("PageNotFound", isPageNotFoundRecord) : "<b>" + genericController.getYesNo(isPageNotFoundRecord) + "</b>" + htmlController.inputHidden("PageNotFound", isPageNotFoundRecord);
                //        tabPanel.Add(adminUIController.getEditRow(core, fieldEditor, "Default Page Not Found", fieldHelp, false, false, ""));
                //    }
                //    //
                //    // ----- Last Known Public Site URL
                //    {
                //        string FieldHelp = "This is the URL where this record was last displayed on the site. It may be blank if the record has not been displayed yet.";
                //        string fieldValue = linkAliasController.getLinkAlias(core, editRecord.id, "", "");
                //        string fieldEditor = (string.IsNullOrEmpty(fieldValue)) ? "unknown" : "<a href=\"" + htmlController.encodeHtml(fieldValue) + "\" target=\"_blank\">" + fieldValue + "</a>";
                //        tabPanel.Add(adminUIController.getEditRow(core, fieldEditor, "Last Known Public URL", FieldHelp, false, false, ""));
                //    }
                //}
                //
                // -- GUID
                {
                    string fieldValue = GenericController.encodeText(editRecord.fieldsLc["ccguid"].value);
                    string FieldHelp = "This is a unique number that identifies this record globally. A GUID is not required, but when set it should never be changed. GUIDs are used to synchronize records. When empty, you can create a new guid. Only Developers can modify the guid.";
                    string fieldEditor = "";
                    if (string.IsNullOrEmpty(fieldValue)) {
                        //
                        // add a set button
                        string fieldId = "setGuid" + GenericController.GetRandomInteger(core).ToString();
                        string buttonCell = HtmlController.div(AdminUIController.getButtonPrimary("Set", "var e=document.getElementById('" + fieldId + "');if(e){e.value='{" + GenericController.getGUIDString() + "}';this.disabled=true;}"), "col-xs-1");
                        string inputCell = HtmlController.div(AdminUIController.getDefaultEditor_Text(core, "ccguid", "", false, fieldId), "col-xs-11");
                        fieldEditor = HtmlController.div(HtmlController.div(buttonCell + inputCell, "row"), "container-fluid");
                    } else {
                        //
                        // field is read-only except for developers
                        fieldEditor = AdminUIController.getDefaultEditor_Text(core, "ccguid", fieldValue, !core.session.isAuthenticatedDeveloper(core), "");
                    }
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "GUID", FieldHelp, false, false, ""));
                }
                //
                // ----- EID (Encoded ID)
                {
                    if (GenericController.vbUCase(adminContext.adminContent.tableName) == GenericController.vbUCase("ccMembers")) {
                        bool AllowEID = (core.siteProperties.getBoolean("AllowLinkLogin", true)) | (core.siteProperties.getBoolean("AllowLinkRecognize", true));
                        string fieldHelp = "";
                        string fieldEditor = "";
                        if (!AllowEID) {
                            fieldEditor = "(link login and link recognize are disabled in security preferences)";
                        } else if (editRecord.id == 0) {
                            fieldEditor = "(available after save)";
                        } else {
                            string eidQueryString = "eid=" + Processor.Controllers.SecurityController.encodeToken(core, editRecord.id, core.doc.profileStartTime);
                            string sampleUrl = cp.Request.Protocol + cp.Request.Host + cp.Site.AppRootPath + cp.Site.PageDefault + "?" + eidQueryString;
                            if (core.siteProperties.getBoolean("AllowLinkLogin", true)) {
                                fieldHelp = "If " + eidQueryString + " is added to a url querystring for this site, the user be logged in as this person.";
                            } else {
                                fieldHelp = "If " + eidQueryString + " is added to a url querystring for this site, the user be recognized in as this person, but not logged in.";
                            }
                            fieldHelp += " To enable, disable or modify this feature, use the security tab on the Preferences page.";
                            fieldHelp += "<br>For example: " + sampleUrl;
                            fieldEditor = AdminUIController.getDefaultEditor_Text(core, "ignore_eid", eidQueryString, true, "");
                        }
                        tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Member Link Login Querystring", fieldHelp, true, false, ""));
                    }
                }
                //
                // ----- Controlling Content
                {
                    string HTMLFieldString = "";
                    string FieldHelp = "The content in which this record is stored. This is similar to a database table.";
                    CDefFieldModel field = null;
                    if (adminContext.adminContent.fields.ContainsKey("contentcontrolid")) {
                        field = adminContext.adminContent.fields["contentcontrolid"];
                        //
                        // if this record has a parent id, only include CDefs compatible with the parent record - otherwise get all for the table
                        FieldHelp = GenericController.encodeText(field.helpMessage);
                        FieldRequired = GenericController.encodeBoolean(field.required);
                        int FieldValueInteger = (editRecord.contentControlId.Equals(0)) ? adminContext.adminContent.id : editRecord.contentControlId;
                        if (!core.session.isAuthenticatedAdmin(core)) {
                            HTMLFieldString = HTMLFieldString + HtmlController.inputHidden("ContentControlID", FieldValueInteger);
                        } else {
                            string RecordContentName = editRecord.contentControlId_Name;
                            string TableName2 = CdefController.getContentTablename(core, RecordContentName);
                            int TableID = core.db.getRecordID("Tables", TableName2);
                            //
                            // Test for parentid
                            int ParentID = 0;
                            bool ContentSupportsParentID = false;
                            if (editRecord.id > 0) {
                                int CS = core.db.csOpenRecord(RecordContentName, editRecord.id);
                                if (core.db.csOk(CS)) {
                                    ContentSupportsParentID = core.db.csIsFieldSupported(CS, "ParentID");
                                    if (ContentSupportsParentID) {
                                        ParentID = core.db.csGetInteger(CS, "ParentID");
                                    }
                                }
                                core.db.csClose(ref CS);
                            }
                            //
                            int LimitContentSelectToThisID = 0;
                            if (ContentSupportsParentID) {
                                //
                                // Parentid - restrict CDefs to those compatible with the parentid
                                if (ParentID != 0) {
                                    //
                                    // This record has a parent, set LimitContentSelectToThisID to the parent's CID
                                    int CSPointer = core.db.csOpenRecord(RecordContentName, ParentID, false, false, "ContentControlID");
                                    if (core.db.csOk(CSPointer)) {
                                        LimitContentSelectToThisID = core.db.csGetInteger(CSPointer, "ContentControlID");
                                    }
                                    core.db.csClose(ref CSPointer);
                                }

                            }
                            bool IsEmptyList = false;
                            if (core.session.isAuthenticatedAdmin(core) && (LimitContentSelectToThisID == 0)) {
                                //
                                // administrator, and either ( no parentid or does not support it), let them select any content compatible with the table
                                string sqlFilter = "(ContentTableID=" + TableID + ")";
                                int contentCID = core.db.getRecordID(Processor.Models.Db.ContentModel.contentName, Processor.Models.Db.ContentModel.contentName);
                                HTMLFieldString += AdminUIController.getDefaultEditor_LookupContent(core, "contentcontrolid", FieldValueInteger, contentCID, ref IsEmptyList, false, "", "", true, sqlFilter);
                                FieldHelp = FieldHelp + " (Only administrators have access to this control. Changing the Controlling Content allows you to change who can author the record, as well as how it is edited.)";
                            } else {
                                //
                                // Limit the list to only those cdefs that are within the record's parent contentid
                                RecordContentName = editRecord.contentControlId_Name;
                                TableName2 = CdefController.getContentTablename(core, RecordContentName);
                                TableID = core.db.getRecordID("Tables", TableName2);
                                int CSPointer = core.db.csOpen("Content", "ContentTableID=" + TableID, "", true, 0, false, false, "ContentControlID");
                                string CIDList = "";
                                while (core.db.csOk(CSPointer)) {
                                    int ChildCID = core.db.csGetInteger(CSPointer, "ID");
                                    if (CdefController.isWithinContent(core, ChildCID, LimitContentSelectToThisID)) {
                                        if ((core.session.isAuthenticatedAdmin(core)) | (core.session.isAuthenticatedContentManager(core, CdefController.getContentNameByID(core, ChildCID)))) {
                                            CIDList = CIDList + "," + ChildCID;
                                        }
                                    }
                                    core.db.csGoNext(CSPointer);
                                }
                                core.db.csClose(ref CSPointer);
                                if (!string.IsNullOrEmpty(CIDList)) {
                                    CIDList = CIDList.Substring(1);
                                    string sqlFilter = "(id in (" + CIDList + "))";
                                    int contentCID = core.db.getRecordID(Processor.Models.Db.ContentModel.contentName, Processor.Models.Db.ContentModel.contentName);
                                    HTMLFieldString += AdminUIController.getDefaultEditor_LookupContent(core, "contentcontrolid", FieldValueInteger, contentCID, ref IsEmptyList, false, "", "", true, sqlFilter);
                                    FieldHelp = FieldHelp + " (Only administrators have access to this control. Changing the Controlling Content allows you to change who can author the record, as well as how it is edited. This record includes a Parent field, so your choices for controlling content are limited to those compatible with the parent of this record.)";
                                }
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(HTMLFieldString)) {
                        HTMLFieldString = editRecord.contentControlId_Name;
                    }
                    tabPanel.Add(AdminUIController.getEditRow(core, HTMLFieldString, "Controlling Content", FieldHelp, FieldRequired, false, ""));
                }
                //
                // ----- Created By
                {
                    string FieldHelp = "The people account of the user who created this record.";
                    string fieldValue = "";
                    if (editRecord.id == 0) {
                        fieldValue = "(available after save)";
                    } else {
                        int FieldValueInteger = editRecord.createdBy.id;
                        if (FieldValueInteger == 0) {
                            fieldValue = "(not set)";
                        } else {
                            int CSPointer = core.db.csOpen("people", "(id=" + FieldValueInteger + ")","name,active",false);
                            if (!core.db.csOk(CSPointer)) {
                                fieldValue = "#" + FieldValueInteger + ", (deleted)";
                            } else {
                                fieldValue = "#" + FieldValueInteger + ", " + core.db.csGet(CSPointer, "name");
                                if (!core.db.csGetBoolean(CSPointer, "active")) {
                                    fieldValue += " (inactive)";
                                }
                            }
                            core.db.csClose(ref CSPointer);
                        }
                    }
                    string fieldEditor = AdminUIController.getDefaultEditor_Text(core, "ignore_createdBy", fieldValue, true, "");
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Created By", FieldHelp, FieldRequired, false, ""));
                }
                //
                // ----- Created Date
                {
                    string FieldHelp = "The date and time when this record was originally created.";
                    string fieldValue = "";
                    if (editRecord.id == 0) {
                        fieldValue = "(available after save)";
                    } else {
                        if (encodeDateMinValue(editRecord.dateAdded) == DateTime.MinValue) {
                            fieldValue = "(not set)";
                        } else {
                            fieldValue = editRecord.dateAdded.ToString();
                        }
                    }
                    string fieldEditor = AdminUIController.getDefaultEditor_Text(core, "ignore_createdDate", fieldValue, true, "");
                    //string fieldEditor = htmlController.inputText( core,"ignore", fieldValue, -1, -1, "", false, true);
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Created Date", FieldHelp, FieldRequired, false, ""));
                }
                //
                // ----- Modified By
                {
                    string FieldHelp = "The people account of the last user who modified this record.";
                    string fieldValue = "";
                    if (editRecord.id == 0) {
                        fieldValue = "(available after save)";
                    } else {
                        int FieldValueInteger = editRecord.modifiedBy.id;
                        if (FieldValueInteger == 0) {
                            fieldValue = "(not set)";
                        } else {
                            int CSPointer = core.db.csOpen("people", "(id=" + FieldValueInteger + ")", "name,active", false);
                            if (!core.db.csOk(CSPointer)) {
                                fieldValue = "#" + FieldValueInteger + ", (deleted)";
                            } else {
                                fieldValue = "#" + FieldValueInteger + ", " + core.db.csGet(CSPointer, "name");
                                if (!core.db.csGetBoolean(CSPointer, "active")) {
                                    fieldValue += " (inactive)";
                                }
                            }
                            core.db.csClose(ref CSPointer);
                        }
                    }
                    string fieldEditor = AdminUIController.getDefaultEditor_Text(core, "ignore_modifiedBy", fieldValue, true, "");
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Modified By", FieldHelp, FieldRequired, false, ""));
                }
                //
                // ----- Modified Date
                {
                    string FieldHelp = "The date and time when this record was last modified.";
                    string fieldValue = "";
                    if (editRecord.id == 0) {
                        fieldValue = "(available after save)";
                    } else {
                        if (encodeDateMinValue(editRecord.modifiedDate)==DateTime.MinValue) {
                            fieldValue = "(not set)";
                        } else {
                            fieldValue = editRecord.modifiedDate.ToString();
                        }
                    }
                    string fieldEditor = AdminUIController.getDefaultEditor_Text(core, "ignore_modifiedBy", fieldValue, true, "");
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Modified Date", FieldHelp, false, false, ""));
                }
                string s = AdminUIController.editTableOpen + tabPanel.Text + AdminUIController.editTableClose;
                result = AdminUIController.getEditPanel(core, (!adminContext.allowAdminTabs), "Control Information", "", s);
                adminContext.EditSectionPanelCount = adminContext.EditSectionPanelCount + 1;
                tabPanel = null;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //========================================================================
        //   Display field in the admin/edit
        //========================================================================
        //
        private string GetForm_Edit_SiteProperties(AdminInfoDomainModel adminContext) {
            string tempGetForm_Edit_SiteProperties = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
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
                StringBuilderLegacyController FastString = null;
                string Copy = "";
                //adminUIController Adminui = new adminUIController(core);
                string SitePropertyName = null;
                string SitePropertyValue = null;
                string selector = null;
                string FieldName = null;
                //
                FastString = new StringBuilderLegacyController();
                //
                SitePropertyName = "";
                SitePropertyValue = "";
                selector = "";
                foreach (KeyValuePair<string, CDefFieldModel> keyValuePair in adminContext.adminContent.fields) {
                    CDefFieldModel field = keyValuePair.Value;
                    //
                    FieldName = field.nameLc;
                    if (GenericController.vbLCase(FieldName) == "name") {
                        SitePropertyName = GenericController.encodeText(editRecord.fieldsLc[field.nameLc].value);
                    } else if (FieldName.ToLower() == "fieldvalue") {
                        SitePropertyValue = GenericController.encodeText(editRecord.fieldsLc[field.nameLc].value);
                    } else if (FieldName.ToLower() == "selector") {
                        selector = GenericController.encodeText(editRecord.fieldsLc[field.nameLc].value);
                    }
                }
                if (string.IsNullOrEmpty(SitePropertyName)) {
                    HTMLFieldString = "This Site Property is not defined";
                } else {
                    HTMLFieldString = HtmlController.inputHidden("name", SitePropertyName);
                    Dictionary<string, string> instanceOptions = new Dictionary<string, string>();
                    Dictionary<string, string> addonInstanceProperties = new Dictionary<string, string>();
                    instanceOptions.Add(SitePropertyName, SitePropertyValue);
                    core.addon.buildAddonOptionLists(ref addonInstanceProperties, ref ExpandedSelector, SitePropertyName + "=" + selector, instanceOptions, "0", true);

                    //--------------

                    Pos = GenericController.vbInstr(1, ExpandedSelector, "[");
                    if (Pos != 0) {
                        //
                        // List of Options, might be select, radio or checkbox
                        //
                        LCaseOptionDefault = GenericController.vbLCase(ExpandedSelector.Left(Pos - 1));
                        LCaseOptionDefault = GenericController.decodeNvaArgument(LCaseOptionDefault);

                        ExpandedSelector = ExpandedSelector.Substring(Pos);
                        Pos = GenericController.vbInstr(1, ExpandedSelector, "]");
                        if (Pos > 0) {
                            if (Pos < ExpandedSelector.Length) {
                                OptionSuffix = GenericController.vbLCase((ExpandedSelector.Substring(Pos)).Trim(' '));
                            }
                            ExpandedSelector = ExpandedSelector.Left(Pos - 1);
                        }
                        OptionValues = ExpandedSelector.Split('|');
                        HTMLFieldString = "";
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
                                        // Create checkbox HTMLFieldString
                                        //
                                        if (GenericController.vbInstr(1, "," + LCaseOptionDefault + ",", "," + GenericController.vbLCase(OptionValue) + ",") != 0) {
                                            HTMLFieldString = HTMLFieldString + "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + SitePropertyName + OptionPtr + "\" value=\"" + OptionValue + "\" checked=\"checked\">" + OptionCaption + "</div>";
                                        } else {
                                            HTMLFieldString = HTMLFieldString + "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + SitePropertyName + OptionPtr + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
                                        }
                                        break;
                                    case "radio":
                                        //
                                        // Create Radio HTMLFieldString
                                        //
                                        if (GenericController.vbLCase(OptionValue) == LCaseOptionDefault) {
                                            HTMLFieldString = HTMLFieldString + "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + SitePropertyName + "\" value=\"" + OptionValue + "\" checked=\"checked\" >" + OptionCaption + "</div>";
                                        } else {
                                            HTMLFieldString = HTMLFieldString + "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + SitePropertyName + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
                                        }
                                        break;
                                    default:
                                        //
                                        // Create select HTMLFieldString
                                        //
                                        if (GenericController.vbLCase(OptionValue) == LCaseOptionDefault) {
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

                        selector = GenericController.decodeNvaArgument(selector);
                        HTMLFieldString = HtmlController.inputText(core, SitePropertyName, selector, 1, 20);
                    }
                    //--------------

                    //HTMLFieldString = core.main_GetFormInputText2( genericController.vbLCase(FieldName), VAlue)
                }
                FastString.Add(AdminUIController.getEditRowLegacy(core, HTMLFieldString, SitePropertyName, "", false, false, ""));
                tempGetForm_Edit_SiteProperties = AdminUIController.getEditPanel(core, (!adminContext.allowAdminTabs), "Control Information", "", AdminUIController.editTableOpen + FastString.Text + AdminUIController.editTableClose);
                adminContext.EditSectionPanelCount = adminContext.EditSectionPanelCount + 1;
                FastString = null;
                return tempGetForm_Edit_SiteProperties;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                int addonId = 0;
                string AddonIDText = null;
                //
                // This is really messy -- there must be a better way
                //
                addonId = 0;
                if (core.session.visit.id == core.docProperties.getInteger(RequestNameDashboardReset)) {
                    //$$$$$ cache this
                    CS = core.db.csOpen(cnAddons, "ccguid=" + core.db.encodeSQLText(addonGuidDashboard));
                    if (core.db.csOk(CS)) {
                        addonId = core.db.csGetInteger(CS, "id");
                        core.siteProperties.setProperty("AdminRootAddonID", GenericController.encodeText(addonId));
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
                        addonId = GenericController.encodeInteger(AddonIDText);
                        //
                        // Verify it so there is no error when it runs
                        if (AddonModel.create(core, addonId)==null) {
                            addonId = -1;
                            core.siteProperties.setProperty("AdminRootAddonID", "");
                        }
                    }
                    if (addonId == -1) {
                        //
                        // This has never been set, try to get the dashboard ID
                        var addon = AddonModel.create(core, addonGuidDashboard);
                        if ( addon != null ) {
                            addonId = addon.id;
                            core.siteProperties.setProperty("AdminRootAddonID", addonId);
                        }
                    }
                }
                if (addonId != 0) {
                    //
                    // Display the Addon
                    //
                    if (core.doc.debug_iUserError != "") {
                        returnHtml = returnHtml + "<div style=\"clear:both;margin-top:20px;\">&nbsp;</div>"
                        + "<div style=\"clear:both;margin-top:20px;\">" + ErrorController.getUserError(core) + "</div>";
                    }
                    returnHtml += core.addon.execute(AddonModel.create(core, addonId), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                        addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                        errorContextMessage = "executing addon id:" + addonId + " set as Admin Root addon"
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
                    + "\r\n<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\">Login Member Name</div><div style=\"float:left;\">" + core.session.user.name + "</div></div>"
                    + "\r\n<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\">Quick Reports</div><div style=\"float:left;\"><a Href=\"?" + rnAdminForm + "=" + AdminFormQuickStats + "\">Real-Time Activity</A></div></div>"
                    + "\r\n<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\"><a Href=\"?" + RequestNameDashboardReset + "=" + core.session.visit.id + "\">Run Dashboard</A></div></div>"
                    + "\r\n<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\"><a Href=\"?addonguid=" + addonGuidAddonManager + "\">Add-on Manager</A></div></div>";
                    //
                    if (core.doc.debug_iUserError != "") {
                        returnHtml = returnHtml + "<div style=\"clear:both;margin-top:20px;\">&nbsp;</div>"
                        + "<div style=\"clear:both;margin-top:20px;\">" + ErrorController.getUserError(core) + "</div>";
                    }
                    //
                    returnHtml = returnHtml + "\r\n</div>"
                    + "";
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                //
                // --- Start a form to make a refresh button
                //
                //Stream.Add(htmlController.form_start(core));
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
                    Stream.Add("<td style=\"width:150px;border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + HtmlController.encodeHtml(core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormReports + "&rid=3&DateFrom=" + core.doc.profileStartTime + "&DateTo=" + core.doc.profileStartTime.ToShortDateString()) + "\">" + VisitCount + "</A>, " + string.Format("{0:N2}", PageCount) + " pages/visit.</span></td>");
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
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + HtmlController.encodeHtml(core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormReports + "&rid=3&DateFrom=" + core.doc.profileStartTime.ToShortDateString() + "&DateTo=" + core.doc.profileStartTime.ToShortDateString()) + "\">" + VisitCount + "</A>, " + string.Format("{0:N2}", PageCount) + " pages/visit.</span></td>");
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
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + HtmlController.encodeHtml(core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormReports + "&rid=3&ExcludeOldVisitors=1&DateFrom=" + core.doc.profileStartTime.ToShortDateString() + "&DateTo=" + core.doc.profileStartTime.ToShortDateString()) + "\">" + VisitCount + "</A>, " + string.Format("{0:N2}", PageCount) + " pages/visit.</span></td>");
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
                            Panel = Panel + "<td align=\"left\">" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + HtmlController.encodeHtml(core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormReports + "&rid=16&MemberID=" + core.db.csGetInteger(CS, "MemberID")) + "\">" + core.db.csGet(CS, "MemberName") + "</A></span></td>";
                            Panel = Panel + "<td align=\"left\">" + SpanClassAdminNormal + core.db.csGet(CS, "Remote_Addr") + "</span></td>";
                            Panel = Panel + "<td align=\"left\">" + SpanClassAdminNormal + core.db.csGetDate(CS, "LastVisitTime").ToString("") + "</span></td>";
                            Panel = Panel + "<td align=\"right\">" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormReports + "&rid=10&VisitID=" + VisitID + "\">" + core.db.csGet(CS, "PageVisits") + "</A></span></td>";
                            Panel = Panel + "<td align=\"right\">" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormReports + "&rid=17&VisitID=" + VisitID + "\">" + VisitID + "</A></span></td>";
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
                //Stream.Add(htmlController.form_end());
                //
                tempGetForm_QuickStats = HtmlController.form(core, Stream.Text);
                core.html.addTitle("Quick Stats");
                return tempGetForm_QuickStats;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
        //    If adminContext.content.AllowTopicRules Then
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
        //            SQL = "SELECT ccTopicRules.TopicID AS TopicID FROM (ccContent LEFT JOIN ccTopicRules ON ccContent.ID = ccTopicRules.ContentID) LEFT JOIN ccTables ON ccContent.ContentTableID = ccTables.ID WHERE (((ccTables.Name)=" & encodeSQLText(adminContext.content.ContentTableName) & ") AND ((ccTopicRules.RecordID)=" & EditRecord.ID & ") AND ((ccContent.Active)<>0) AND ((ccTopicRules.Active)<>0));"
        //
        //            'SQL = "SELECT ccTopicRules.TopicID as ID" _
        //             '   & " FROM ccContent LEFT JOIN ccTopicRules ON ccContent.ID = ccTopicRules.ContentID" _
        //              '  & " WHERE (((ccContent.ContentTablename)=" & encodeSQLText(adminContext.content.ContentTableName) & ") AND ((ccTopicRules.RecordID)=" & EditRecord.ID & ") AND ((ccContent.Active)<>0) AND ((ccTopicRules.Active)<>0))"
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
        //                Call f.Add(adminUIController.EditTableOpen)
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
        //                f.Add( adminUIController.EditTableClose
        //                '
        //                ' ----- close the panel
        //                '
        //                GetForm_Edit_TopicRules = adminUIController.GetEditPanel(core, (Not AllowAdminTabs), "Topic Rules", "This content is associated with the following topics", f.Text)
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
        private string GetForm_Edit_LinkAliases(AdminInfoDomainModel adminContext, bool readOnlyField) {
            string tempGetForm_Edit_LinkAliases = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
                //
                int LinkCnt = 0;
                string LinkList = "";
                StringBuilderLegacyController f = new StringBuilderLegacyController();
                //adminUIController Adminui = new adminUIController(core);
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
                    if (adminContext.adminContent.fields.ContainsKey("linkalias")) {
                        linkAlias = GenericController.encodeText(editRecord.fieldsLc["linkalias"].value);
                    }
                    f.Add("<tr><td class=\"ccAdminEditCaption\">" + SpanClassAdminSmall + "Link Alias</td>");
                    f.Add("<td class=\"ccAdminEditField\" align=\"left\" colspan=\"2\">" + SpanClassAdminNormal);
                    if (readOnlyField) {
                        f.Add(linkAlias);
                    } else {
                        f.Add(HtmlController.inputText(core, "LinkAlias", linkAlias));
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
                        f.Add(HtmlController.checkbox("OverrideDuplicate", false));
                    }
                    f.Add("</span></td></tr>");
                    //
                    // Table of old Link Aliases
                    //
                    Link = core.doc.main_GetPageDynamicLink(editRecord.id, false);
                    CS = core.db.csOpen("Link Aliases", "pageid=" + editRecord.id, "ID Desc", true, 0, false, false, "name");
                    while (core.db.csOk(CS)) {
                        LinkList = LinkList + "<div style=\"margin-left:4px;margin-bottom:4px;\">" + HtmlController.encodeHtml(core.db.csGetText(CS, "name")) + "</div>";
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
                    tabContent = AdminUIController.editTableOpen + f.Text + AdminUIController.editTableClose;
                }
                //
                tempGetForm_Edit_LinkAliases = AdminUIController.getEditPanel(core, (!adminContext.allowAdminTabs), "Link Aliases", TabDescription, tabContent);
                adminContext.EditSectionPanelCount = adminContext.EditSectionPanelCount + 1;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
        private string GetForm_Edit_EmailRules(AdminInfoDomainModel adminContext, bool readOnlyField) {
            string s = "";
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
                //
                StringBuilderLegacyController f = new StringBuilderLegacyController();
                //adminUIController Adminui = new adminUIController(core);
                //
                s = core.html.getCheckList("EmailGroups", "Group Email", editRecord.id, "Groups", "Email Groups", "EmailID", "GroupID", "", "Caption");
                s = "<tr>"
                    + "<td class=\"ccAdminEditCaption\">Groups</td>"
                    + "<td class=\"ccAdminEditField\" colspan=2>" + SpanClassAdminNormal + s + "</span></td>"
                    + "</tr><tr>"
                    + "<td class=\"ccAdminEditCaption\">&nbsp;</td>"
                    + "<td class=\"ccAdminEditField\" colspan=2>" + SpanClassAdminNormal + "[<a href=?cid=" + CdefController.getContentId(core, "Groups") + " target=_blank>Manage Groups</a>]</span></td>"
                    + "</tr>";
                s = AdminUIController.editTableOpen + s + AdminUIController.editTableClose;
                s = AdminUIController.getEditPanel(core, (!adminContext.allowAdminTabs), "Email Rules", "Send email to people in these groups", s);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
        private string GetForm_Edit_EmailTopics(AdminInfoDomainModel adminContext, bool readOnlyField) {
            string s = "";
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
                //
                StringBuilderLegacyController f = new StringBuilderLegacyController();
                //adminUIController Adminui = new adminUIController(core);
                //
                s = core.html.getCheckList("EmailTopics", "Group Email", editRecord.id, "Topics", "Email Topics", "EmailID", "TopicID", "", "Name");
                s = "<tr>"
                    + "<td class=\"ccAdminEditCaption\">Topics</td>"
                    + "<td class=\"ccAdminEditField\" colspan=2>" + SpanClassAdminNormal + s + "</span></td>"
                    + "</tr><tr>"
                    + "<td class=\"ccAdminEditCaption\">&nbsp;</td>"
                    + "<td class=\"ccAdminEditField\" colspan=2>" + SpanClassAdminNormal + "[<a href=?cid=" + CdefController.getContentId(core, "Topics") + " target=_blank>Manage Topics</a>]</span></td>"
                    + "</tr>";
                s = AdminUIController.editTableOpen + s + AdminUIController.editTableClose;
                s = AdminUIController.getEditPanel(core, (!adminContext.allowAdminTabs), "Email Rules", "Send email to people in these groups", s);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return s;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private string GetForm_Edit_EmailBounceStatus(AdminInfoDomainModel adminContext) {
            string tempGetForm_Edit_EmailBounceStatus = null;
            try {
                //
                StringBuilderLegacyController f = new StringBuilderLegacyController();
                string Copy = null;
                //adminUIController Adminui = new adminUIController(core);
                //
                f.Add(AdminUIController.getEditRowLegacy(core, "<a href=?" + rnAdminForm + "=28 target=_blank>Open in New Window</a>", "Email Control", "The settings in this section can be modified with the Email Control page."));
                f.Add(AdminUIController.getEditRowLegacy(core, core.siteProperties.getText("EmailBounceAddress", ""), "Bounce Email Address", "All bounced emails will be sent to this address automatically. This must be a valid email account, and you should either use Contensive Bounce processing to capture the emails, or manually remove them from the account yourself."));
                f.Add(AdminUIController.getEditRowLegacy(core, GenericController.getYesNo(GenericController.encodeBoolean(core.siteProperties.getBoolean("AllowEmailBounceProcessing", false))), "Allow Bounce Email Processing", "If checked, Contensive will periodically retrieve all the email from the POP email account and take action on the membefr account that sent the email."));
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
                f.Add(AdminUIController.getEditRowLegacy(core, Copy, "Bounce Email Action", "When an email is determined to be a bounce, this action will taken against member with that email address."));
                f.Add(AdminUIController.getEditRowLegacy(core, core.siteProperties.getText("POPServerStatus"), "Last Email Retrieve Status", "This is the status of the last POP email retrieval attempted."));
                //
                tempGetForm_Edit_EmailBounceStatus = AdminUIController.getEditPanel(core, (!adminContext.allowAdminTabs), "Bounced Email Handling", "", AdminUIController.editTableOpen + f.Text + AdminUIController.editTableClose);
                adminContext.EditSectionPanelCount = adminContext.EditSectionPanelCount + 1;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
        private string GetForm_Edit_MemberGroups(AdminInfoDomainModel adminContext) {
            string result = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
                //
                StringBuilderLegacyController body = new StringBuilderLegacyController();
                //
                // ----- Gather all the SecondaryContent that associates to the PrimaryContent
                int PeopleContentID = CdefController.getContentId(core, "People");
                int GroupContentID = CdefController.getContentId(core, "Groups");
                //
                int MembershipCount = 0;
                int MembershipSize = 0;
                int GroupCount = 0;
                {
                    //
                    // ----- read in the groups that this member has subscribed (exclude new member records)
                    int[] Membership = { };
                    DateTime[] DateExpires = { };
                    bool[] Active = { };
                    if (editRecord.id != 0) {
                        string SQL = "SELECT Active,GroupID,DateExpires"
                            + " FROM ccMemberRules"
                            + " WHERE MemberID=" + editRecord.id;
                        int CS = core.db.csOpenSql(SQL, "Default");
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
                    string SQL2 = "SELECT ccGroups.ID AS ID, ccContent.Name AS SectionName, ccGroups.Caption AS GroupCaption, ccGroups.name AS GroupName, ccGroups.SortOrder"
                        + " FROM ccGroups LEFT JOIN ccContent ON ccGroups.ContentControlID = ccContent.ID"
                        + " Where (((ccGroups.Active) <> " + SQLFalse + ") And ((ccContent.Active) <> " + SQLFalse + "))";
                    SQL2 += ""
                        + " GROUP BY ccGroups.ID, ccContent.Name, ccGroups.Caption, ccGroups.name, ccGroups.SortOrder"
                        + " ORDER BY ccGroups.Caption";
                    int CS2 = core.db.csOpenSql(SQL2, "Default");
                    //
                    // Output all the groups, with the active and dateexpires from those joined
                    //body.Add(adminUIController.EditTableOpen);
                    bool CanSeeHiddenGroups = core.session.isAuthenticatedDeveloper(core);
                    while (core.db.csOk(CS2)) {
                        string GroupName = core.db.csGet(CS2, "GroupName");
                        if ((GroupName.Left(1) != "_") || CanSeeHiddenGroups) {
                            string GroupCaption = core.db.csGet(CS2, "GroupCaption");
                            int GroupID = core.db.csGetInteger(CS2, "ID");
                            if (string.IsNullOrEmpty(GroupCaption)) {
                                GroupCaption = GroupName;
                                if (string.IsNullOrEmpty(GroupCaption)) {
                                    GroupCaption = "Group&nbsp;" + GroupID;
                                }
                            }
                            bool GroupActive = false;
                            string DateExpireValue = "";
                            if (MembershipCount != 0) {
                                for (int MembershipPointer = 0; MembershipPointer < MembershipCount; MembershipPointer++) {
                                    if (Membership[MembershipPointer] == GroupID) {
                                        GroupActive = Active[MembershipPointer];
                                        if (DateExpires[MembershipPointer] > DateTime.MinValue) {
                                            DateExpireValue = GenericController.encodeText(DateExpires[MembershipPointer]);
                                        }
                                        break;
                                    }
                                }
                            }
                            string ReportLink = "";
                            ReportLink = ReportLink + "[<a href=\"?af=4&cid=" + GroupContentID + "&id=" + GroupID + "\">Edit&nbsp;Group</a>]";
                            if (GroupID > 0) {
                                ReportLink = ReportLink + "&nbsp;[<a href=\"?" + rnAdminForm + "=12&rid=35&recordid=" + GroupID + "\">Group&nbsp;Report</a>]";
                            }
                            //
                            string Caption = "";
                            if (GroupCount == 0) {
                                Caption = SpanClassAdminSmall + "Groups</span>";
                            } else {
                                Caption = "&nbsp;";
                            }
                            body.Add("<tr><td class=\"ccAdminEditCaption\">" + Caption + "</td>");
                            body.Add("<td class=\"ccAdminEditField\">");
                            body.Add("<table border=0 cellpadding=0 cellspacing=0 width=\"100%\" ><tr>");
                            body.Add("<td width=\"40%\">" + HtmlController.inputHidden("Memberrules." + GroupCount + ".ID", GroupID) + HtmlController.checkbox("MemberRules." + GroupCount, GroupActive) + GroupCaption + "</td>");
                            body.Add("<td width=\"30%\"> Expires " + HtmlController.inputText(core, "MemberRules." + GroupCount + ".DateExpires", DateExpireValue, 1, 20) + "</td>");
                            body.Add("<td width=\"30%\">" + ReportLink + "</td>");
                            body.Add("</tr></table>");
                            body.Add("</td></tr>");
                            GroupCount = GroupCount + 1;
                        }
                        core.db.csGoNext(CS2);
                    }
                    core.db.csClose(ref CS2);
                }
                if (GroupCount == 0) {
                    body.Add("<tr><td valign=middle align=right>" + SpanClassAdminSmall + "Groups</span></td><td>" + SpanClassAdminNormal + "There are currently no groups defined</span></td></tr>");
                } else {
                    body.Add("<input type=\"hidden\" name=\"MemberRules.RowCount\" value=\"" + GroupCount + "\">");
                }
                body.Add("<tr>");
                body.Add("<td class=\"ccAdminEditCaption\">&nbsp;</td>");
                body.Add("<td class=\"ccAdminEditField\">" + SpanClassAdminNormal + "[<a href=?cid=" + CdefController.getContentId(core, "Groups") + " target=_blank>Manage Groups</a>]</span></td>");
                body.Add("</tr>");

                result = AdminUIController.getEditPanel(core, (!adminContext.allowAdminTabs), "Group Membership", "This person is a member of these groups", AdminUIController.editTableOpen + body.Text + AdminUIController.editTableClose);
                adminContext.EditSectionPanelCount = adminContext.EditSectionPanelCount + 1;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //========================================================================
        //   Special case tab for Layout records
        //========================================================================
        //
        private string GetForm_Edit_LayoutReports(AdminInfoDomainModel adminContext) {
            string tempGetForm_Edit_LayoutReports = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
                //
                StringBuilderLegacyController FastString = null;
                //adminUIController Adminui = new adminUIController(core);
                //
                FastString = new StringBuilderLegacyController();
                FastString.Add("<tr>");
                FastString.Add("<td valign=\"top\" align=\"right\">&nbsp;</td>");
                FastString.Add("<td colspan=\"2\" class=\"ccAdminEditField\" align=\"left\">" + SpanClassAdminNormal);
                FastString.Add("<ul class=\"ccList\">");
                FastString.Add("<li class=\"ccListItem\"><a target=\"_blank\" href=\"/preview?layout=" + editRecord.id + "\">Preview this layout</A></LI>");
                FastString.Add("</ul>");
                FastString.Add("</span></td></tr>");
                tempGetForm_Edit_LayoutReports = AdminUIController.getEditPanel(core, (!adminContext.allowAdminTabs), "Contensive Reporting", "", AdminUIController.editTableOpen + FastString.Text + AdminUIController.editTableClose);
                adminContext.EditSectionPanelCount = adminContext.EditSectionPanelCount + 1;
                FastString = null;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_Edit_LayoutReports;
        }
        //
        //========================================================================
        //   Special case tab for People records
        //========================================================================
        //
        private string GetForm_Edit_MemberReports(AdminInfoDomainModel adminContext) {
            string tempGetForm_Edit_MemberReports = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
                //
                StringBuilderLegacyController FastString = null;
                //adminUIController Adminui = new adminUIController(core);
                //
                FastString = new StringBuilderLegacyController();
                FastString.Add("<tr>");
                FastString.Add("<td valign=\"top\" align=\"right\">&nbsp;</td>");
                FastString.Add("<td colspan=\"2\" class=\"ccAdminEditField\" align=\"left\">" + SpanClassAdminNormal);
                FastString.Add("<ul class=\"ccList\">");
                FastString.Add("<li class=\"ccListItem\"><a target=\"_blank\" href=\"/" + core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormReports + "&rid=3&MemberID=" + editRecord.id + "&DateTo=" + encodeInteger(Math.Floor(encodeNumber(core.doc.profileStartTime.ToOADate()))) + "&DateFrom=" + (encodeInteger(Math.Floor(encodeNumber(core.doc.profileStartTime.ToOADate()))) - 365) + "\">All visits from this person</A></LI>");
                FastString.Add("<li class=\"ccListItem\"><a target=\"_blank\" href=\"/" + core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormReports + "&rid=13&MemberID=" + editRecord.id + "&DateTo=" + Math.Floor(encodeNumber(core.doc.profileStartTime.ToOADate())) + "&DateFrom=" + Math.Floor(encodeNumber(core.doc.profileStartTime.ToOADate()) - 365) + "\">All orders from this person</A></LI>");
                FastString.Add("</ul>");
                FastString.Add("</span></td></tr>");
                tempGetForm_Edit_MemberReports = AdminUIController.getEditPanel(core, (!adminContext.allowAdminTabs), "Contensive Reporting", "", AdminUIController.editTableOpen + FastString.Text + AdminUIController.editTableClose);
                adminContext.EditSectionPanelCount = adminContext.EditSectionPanelCount + 1;
                FastString = null;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_Edit_MemberReports;
        }
        //
        //========================================================================
        //   Print the path Rules section of the path edit form
        //========================================================================
        //
        private string GetForm_Edit_PageContentBlockRules(AdminInfoDomainModel adminContext) {
            string tempGetForm_Edit_PageContentBlockRules = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
                //
                StringBuilderLegacyController f = new StringBuilderLegacyController();
                string GroupList = null;
                string[] GroupSplit = null;
                int Ptr = 0;
                int IDPtr = 0;
                int IDEndPtr = 0;
                int GroupID = 0;
                string ReportLink = null;
                //adminUIController Adminui = new adminUIController(core);
                //
                GroupList = core.html.getCheckList2("PageContentBlockRules", adminContext.adminContent.name, editRecord.id, "Groups", "Page Content Block Rules", "RecordID", "GroupID", "", "Caption", false);
                GroupSplit = GroupList.Split(new[] { "<br>" }, StringSplitOptions.None);
                for (Ptr = 0; Ptr <= GroupSplit.GetUpperBound(0); Ptr++) {
                    GroupID = 0;
                    IDPtr = GroupSplit[Ptr].IndexOf("value=");
                    if (IDPtr > 0) {
                        IDEndPtr = GenericController.vbInstr(IDPtr, GroupSplit[Ptr], ">");
                        if (IDEndPtr > 0) {
                            GroupID = GenericController.encodeInteger(GroupSplit[Ptr].Substring(IDPtr + 5, IDEndPtr - IDPtr - 6));
                        }
                    }
                    if (GroupID > 0) {
                        ReportLink = "[<a href=\"?" + rnAdminForm + "=12&rid=35&recordid=" + GroupID + "\" target=_blank>Group&nbsp;Report</a>]";
                    } else {
                        ReportLink = "&nbsp;";
                    }
                    f.Add("<tr><td>&nbsp;</td><td class=\"ccAdminEditField\" align=left>" + SpanClassAdminNormal + GroupSplit[Ptr] + "</span></td><td class=\"ccAdminEditField\" align=center>" + ReportLink + "</td></tr>");
                }
                tempGetForm_Edit_PageContentBlockRules = AdminUIController.getEditPanel(core, (!adminContext.allowAdminTabs), "Content Blocking", "If content is blocked, select groups that have access to this content", AdminUIController.editTableOpen + f.Text + AdminUIController.editTableClose);
                adminContext.EditSectionPanelCount = adminContext.EditSectionPanelCount + 1;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_Edit_PageContentBlockRules;
        }
        //
        //========================================================================
        //   Print the path Rules section of the path edit form
        //========================================================================
        //
        private string GetForm_Edit_LibraryFolderRules(AdminInfoDomainModel adminContext) {
            string tempGetForm_Edit_LibraryFolderRules = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
                //
                string Copy = null;
                StringBuilderLegacyController f = new StringBuilderLegacyController();
                string GroupList = null;
                string[] GroupSplit = null;
                int Ptr = 0;
                int IDPtr = 0;
                int IDEndPtr = 0;
                int GroupID = 0;
                string ReportLink = null;
                //adminUIController Adminui = new adminUIController(core);
                //
                GroupList = core.html.getCheckList2("LibraryFolderRules", adminContext.adminContent.name, editRecord.id, "Groups", "Library Folder Rules", "FolderID", "GroupID", "", "Caption");
                GroupSplit = stringSplit(GroupList, "<br>");
                for (Ptr = 0; Ptr <= GroupSplit.GetUpperBound(0); Ptr++) {
                    GroupID = 0;
                    IDPtr = GroupSplit[Ptr].IndexOf("value=");
                    if (IDPtr > 0) {
                        IDEndPtr = GenericController.vbInstr(IDPtr, GroupSplit[Ptr], ">");
                        if (IDEndPtr > 0) {
                            GroupID = GenericController.encodeInteger(GroupSplit[Ptr].Substring(IDPtr + 5, IDEndPtr - IDPtr - 6));
                        }
                    }
                    if (GroupID > 0) {
                        ReportLink = "[<a href=\"?" + rnAdminForm + "=12&rid=35&recordid=" + GroupID + "\" target=_blank>Group&nbsp;Report</a>]";
                    } else {
                        ReportLink = "&nbsp;";
                    }
                    f.Add("<tr><td>&nbsp;</td><td class=\"ccAdminEditField\" align=left>" + SpanClassAdminNormal + GroupSplit[Ptr] + "</span></td><td class=\"ccAdminEditField\" align=center>" + ReportLink + "</td></tr>");
                }
                Copy = "Select groups who have authoring access within this folder. This means if you are in this group you can upload files, delete files, create folders and delete folders within this folder and any subfolders.";
                tempGetForm_Edit_LibraryFolderRules = AdminUIController.getEditPanel(core, (!adminContext.allowAdminTabs), "Folder Permissions", Copy, AdminUIController.editTableOpen + f.Text + AdminUIController.editTableClose);
                adminContext.EditSectionPanelCount = adminContext.EditSectionPanelCount + 1;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_Edit_LibraryFolderRules;
        }
        //
        //========================================================================
        // Print the Group Rules section for Content Edit form
        //   Group rules show which groups have authoring rights to a content
        //
        //   adminContext.content.id is the ContentID of the Content Definition being edited
        //   EditRecord.ContentID is the ContentControlID of the Edit Record
        //========================================================================
        //
        private string GetForm_Edit_GroupRules(AdminInfoDomainModel adminContext) {
            string tempGetForm_Edit_GroupRules = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
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
                AdminInfoDomainModel.GroupRuleType[] GroupRules = { };
                StringBuilderLegacyController FastString = null;
                //adminUIController Adminui = new adminUIController(core);
                //
                // ----- Open the panel
                //
                FastString = new StringBuilderLegacyController();
                //
                //Call core.main_PrintPanelTop("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                //Call call FastString.Add(adminUIController.EditTableOpen)
                //
                // ----- Gather all the groups which have authoring rights to the content
                //
                GroupRulesCount = 0;
                GroupRulesSize = 0;
                if (editRecord.id != 0) {
                    SQL = "SELECT ccGroups.ID AS ID, ccGroupRules.AllowAdd as allowadd, ccGroupRules.AllowDelete as allowdelete"
                        + " FROM ccGroups LEFT JOIN ccGroupRules ON ccGroups.ID = ccGroupRules.GroupID"
                        + " WHERE (((ccGroupRules.ContentID)=" + editRecord.id + ") AND ((ccGroupRules.Active)<>0) AND ((ccGroups.Active)<>0))";
                    CS = core.db.csOpenSql(SQL, "Default");
                    if (core.db.csOk(CS)) {
                        if (true) {
                            GroupRulesSize = 100;
                            GroupRules = new AdminInfoDomainModel.GroupRuleType[GroupRulesSize + 1];
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
                CS = core.db.csOpenSql(SQL, "Default");
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
                                FastString.Add("<td width=\"200\">" + SpanClassAdminSmall + HtmlController.checkbox("Group" + GroupCount, true) + GroupName + "</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + HtmlController.checkbox("GroupRuleAllowAdd" + GroupCount, GroupRules[GroupRulesPointer].AllowAdd) + " Allow Add</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + HtmlController.checkbox("GroupRuleAllowDelete" + GroupCount, GroupRules[GroupRulesPointer].AllowDelete) + " Allow Delete</span></td>");
                            } else {
                                FastString.Add("<td width=\"200\">" + SpanClassAdminSmall + HtmlController.checkbox("Group" + GroupCount, false) + GroupName + "</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + HtmlController.checkbox("GroupRuleAllowAdd" + GroupCount, false) + " Allow Add</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + HtmlController.checkbox("GroupRuleAllowDelete" + GroupCount, false) + " Allow Delete</span></td>");
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
                //Call FastString.Add(adminUIController.EditTableClose)
                //Call core.main_PrintPanelBottom("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                //
                tempGetForm_Edit_GroupRules = AdminUIController.getEditPanel(core, (!adminContext.allowAdminTabs), "Authoring Permissions", "The following groups can edit this content.", AdminUIController.editTableOpen + FastString.Text + AdminUIController.editTableClose);
                adminContext.EditSectionPanelCount = adminContext.EditSectionPanelCount + 1;
                FastString = null;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_Edit_GroupRules;
        }
        //
        //========================================================================
        //   Get all content authorable by the current group
        //========================================================================
        //
        private string GetForm_Edit_ContentGroupRules(AdminInfoDomainModel adminContext) {
            string tempGetForm_Edit_ContentGroupRules = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
                //
                string SQL = null;
                int CS = 0;
                int ContentGroupRulesCount = 0;
                int ContentGroupRulesSize = 0;
                int ContentGroupRulesPointer = 0;
                string ContentName = null;
                int ContentCount = 0;
                bool ContentFound = false;
                AdminInfoDomainModel.ContentGroupRuleType[] ContentGroupRules = { };
                StringBuilderLegacyController FastString = null;
                //adminUIController Adminui = new adminUIController(core);
                //
                if (core.session.isAuthenticatedAdmin(core)) {
                    //
                    // ----- Open the panel
                    //
                    FastString = new StringBuilderLegacyController();
                    //
                    //Call core.main_PrintPanelTop("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                    //Call call FastString.Add(adminUIController.EditTableOpen)
                    //
                    // ----- Gather all the groups which have authoring rights to the content
                    //
                    ContentGroupRulesCount = 0;
                    ContentGroupRulesSize = 0;
                    if (editRecord.id != 0) {
                        SQL = "SELECT ccContent.ID AS ID, ccGroupRules.AllowAdd as allowadd, ccGroupRules.AllowDelete as allowdelete"
                            + " FROM ccContent LEFT JOIN ccGroupRules ON ccContent.ID = ccGroupRules.ContentID"
                            + " WHERE (((ccGroupRules.GroupID)=" + editRecord.id + ") AND ((ccGroupRules.Active)<>0) AND ((ccContent.Active)<>0))";
                        CS = core.db.csOpenSql(SQL, "Default");
                        if (core.db.csOk(CS)) {
                            ContentGroupRulesSize = 100;
                            ContentGroupRules = new AdminInfoDomainModel.ContentGroupRuleType[ContentGroupRulesSize + 1];
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
                    CS = core.db.csOpenSql(SQL, "Default");
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
                                FastString.Add("<td width=\"200\">" + SpanClassAdminSmall + HtmlController.checkbox("Content" + ContentCount, true) + ContentName + "</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + HtmlController.checkbox("ContentGroupRuleAllowAdd" + ContentCount, ContentGroupRules[ContentGroupRulesPointer].AllowAdd) + " Allow Add</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + HtmlController.checkbox("ContentGroupRuleAllowDelete" + ContentCount, ContentGroupRules[ContentGroupRulesPointer].AllowDelete) + " Allow Delete</span></td>");
                            } else {
                                FastString.Add("<td width=\"200\">" + SpanClassAdminSmall + HtmlController.checkbox("Content" + ContentCount, false) + ContentName + "</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + HtmlController.checkbox("ContentGroupRuleAllowAdd" + ContentCount, false) + " Allow Add</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + HtmlController.checkbox("ContentGroupRuleAllowDelete" + ContentCount, false) + " Allow Delete</span></td>");
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
                    //Call FastString.Add(adminUIController.EditTableClose)
                    //Call core.main_PrintPanelBottom("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                    //
                    tempGetForm_Edit_ContentGroupRules = AdminUIController.getEditPanel(core, (!adminContext.allowAdminTabs), "Authoring Permissions", "This group can edit the following content.", AdminUIController.editTableOpen + FastString.Text + AdminUIController.editTableClose);
                    adminContext.EditSectionPanelCount = adminContext.EditSectionPanelCount + 1;
                    FastString = null;
                }
                return tempGetForm_Edit_ContentGroupRules;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                LogController.handleError(core, ex);
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
        private string getAdminHeader(AdminInfoDomainModel adminContext, string BackgroundColor = "") {
            string result = "";
            try {
                string LeftSide = core.siteProperties.getText("AdminHeaderHTML", "Contensive Administration Site");
                string RightSide = core.doc.profileStartTime + "&nbsp;" + getIconRefreshLink("?" + core.doc.refreshQueryString);
                //
                // Assemble header
                //
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                Stream.Add(AdminUIController.getHeader(core, LeftSide, RightSide));
                //
                // Menuing
                //
                if ((adminContext.ignore_legacyMenuDepth == 0) && (adminContext.AdminMenuModeID == AdminMenuModeTop)) {
                    Stream.Add(GetMenuTopMode(adminContext));
                }
                //
                // --- Rule to separate content
                //
                //Stream.Add("\r<div style=\"border-top:1px solid white;border-bottom:1px solid black;height:2px\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=1 height=1></div>");
                //
                // --- Content Definition
                adminContext.adminFooter = "";
                //
                // -- Admin Navigator
                string AdminNavFull = core.addon.execute(AddonModel.create(core, AdminNavigatorGuid), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                    addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                    errorContextMessage = "executing Admin Navigator in Admin"
                });
                //
                // -- shortterm fix - make navigator changes, long term pull it into project
                string src = "<img title=\"Open Navigator\" alt=\"Open Navigator\" src=\"/cclib/images/OpenRightRev1313.gif\" width=13 height=13 border=0 style=\"text-align:right;\">";
                AdminNavFull = AdminNavFull.Replace(src, iconOpen);
                src = "<img alt=\"Close Navigator\" title=\"Close Navigator\" src=\"/cclib/images/ClosexRev1313.gif\" width=13 height=13 border=0>";
                AdminNavFull = AdminNavFull.Replace(src, iconClose);
                Stream.Add("<table border=0 cellpadding=0 cellspacing=0><tr>\r<td class=\"ccToolsCon\" valign=top>" + GenericController.nop(AdminNavFull) + "\r</td>\r<td id=\"desktop\" class=\"ccContentCon\" valign=top>");
                adminContext.adminFooter = adminContext.adminFooter + "</td></tr></table>";
                //
                result = Stream.Text;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return result;
        }


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
                    Criteria = Criteria + "AND" + CdefController.getContentControlCriteria(core, MenuContentName);
                }
                iParentCriteria = GenericController.encodeEmpty(ParentCriteria, "");
                if (core.session.isAuthenticatedDeveloper(core)) {
                    //
                    // ----- Developer
                    //
                } else if (core.session.isAuthenticatedAdmin(core)) {
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

                    editableCdefIdList = CdefController.getEditableCdefIdList(core);
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
                LogController.handleError(core, ex);
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
            string iParentCriteria = GenericController.encodeEmpty(ParentCriteria, "");
            if (!string.IsNullOrEmpty(iParentCriteria)) {
                iParentCriteria = "(" + iParentCriteria + ")";
            }
            return core.db.csOpenSql( GetMenuSQL(iParentCriteria, cnNavigatorEntries));
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
                    ContentID = GenericController.encodeInteger(LinkCID);
                    if (ContentID != 0) {
                        tempGetMenuLink = GenericController.modifyLinkQuery(tempGetMenuLink, "cid", ContentID.ToString(), true);
                    }
                }
                return tempGetMenuLink;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            //ErrorTrap:
            handleLegacyClassError3("GetMenuLink");
            //
            return tempGetMenuLink;
        }
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adminContext.content"></param>
        /// <param name="editRecord"></param>
        //
        private void ProcessForms(AdminInfoDomainModel adminContext) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
                //
                if (adminContext.AdminSourceForm != 0) {
                    int CS = 0;
                    string EditorStyleRulesFilename = null;
                    switch (adminContext.AdminSourceForm) {
                        case AdminFormReports:
                            //
                            // Reports form cancel button
                            //
                            switch (adminContext.requestButton) {
                                case ButtonCancel:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop;
                                    adminContext.AdminForm = AdminFormRoot;
                                    break;
                            }
                            break;
                        case AdminFormQuickStats:
                            switch (adminContext.requestButton) {
                                case ButtonCancel:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop;
                                    adminContext.AdminForm = AdminFormRoot;
                                    break;
                            }
                            break;
                        case AdminFormPublishing:
                            //
                            // Publish Form
                            //
                            switch (adminContext.requestButton) {
                                case ButtonCancel:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop;
                                    adminContext.AdminForm = AdminFormRoot;
                                    break;
                            }
                            break;
                        case AdminFormIndex:
                            switch (adminContext.requestButton) {
                                case ButtonCancel:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop;
                                    adminContext.AdminForm = AdminFormRoot;
                                    adminContext.adminContent = new CDefModel();
                                    break;
                                case ButtonClose:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop;
                                    adminContext.AdminForm = AdminFormRoot;
                                    adminContext.adminContent = new CDefModel();
                                    break;
                                case ButtonAdd:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop;
                                    adminContext.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonFind:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionFind;
                                    adminContext.AdminForm = adminContext.AdminSourceForm;
                                    break;
                                case ButtonFirst:
                                    adminContext.RecordTop = 0;
                                    adminContext.AdminForm = adminContext.AdminSourceForm;
                                    break;
                                case ButtonPrevious:
                                    adminContext.RecordTop = adminContext.RecordTop - adminContext.RecordsPerPage;
                                    if (adminContext.RecordTop < 0) {
                                        adminContext.RecordTop = 0;
                                    }
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop;
                                    adminContext.AdminForm = adminContext.AdminSourceForm;
                                    break;
                                case ButtonNext:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNext;
                                    adminContext.AdminForm = adminContext.AdminSourceForm;
                                    break;
                                case ButtonDelete:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionDeleteRows;
                                    adminContext.AdminForm = adminContext.AdminSourceForm;
                                    break;
                            }
                            // end case
                            break;
                        case AdminFormEdit:
                            //
                            // Edit Form
                            //
                            switch (adminContext.requestButton) {
                                case ButtonRefresh:
                                    //
                                    // this is a test operation. need this so the user can set editor preferences without saving the record
                                    //   during refresh, the edit page is redrawn just was it was, but no save
                                    //
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionEditRefresh;
                                    adminContext.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonMarkReviewed:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionMarkReviewed;
                                    adminContext.AdminForm = GetForm_Close(adminContext.ignore_legacyMenuDepth, adminContext.adminContent.name, editRecord.id);
                                    break;
                                case ButtonSaveandInvalidateCache:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionReloadCDef;
                                    adminContext.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonDelete:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionDelete;
                                    adminContext.AdminForm = GetForm_Close(adminContext.ignore_legacyMenuDepth, adminContext.adminContent.name, editRecord.id);
                                    break;
                                case ButtonSave:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionSave;
                                    adminContext.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonSaveAddNew:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionSaveAddNew;
                                    adminContext.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonOK:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionSave;
                                    adminContext.AdminForm = GetForm_Close(adminContext.ignore_legacyMenuDepth, adminContext.adminContent.name, editRecord.id);
                                    break;
                                case ButtonCancel:
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop;
                                    adminContext.AdminForm = GetForm_Close(adminContext.ignore_legacyMenuDepth, adminContext.adminContent.name, editRecord.id);
                                    break;
                                case ButtonSend:
                                    //
                                    // Send a Group Email
                                    //
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionSendEmail;
                                    adminContext.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonActivate:
                                    //
                                    // Activate (submit) a conditional Email
                                    //
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionActivateEmail;
                                    adminContext.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonDeactivate:
                                    //
                                    // Deactivate (clear submit) a conditional Email
                                    //
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionDeactivateEmail;
                                    adminContext.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonSendTest:
                                    //
                                    // Test an Email (Group, System, or Conditional)
                                    //
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionSendEmailTest;
                                    adminContext.AdminForm = AdminFormEdit;
                                    //                Case ButtonSpellCheck
                                    //                    SpellCheckRequest = True
                                    //                    adminContextClass.AdminAction = adminContextClass.AdminActionSave
                                    //                    adminContext.AdminForm = AdminFormEdit
                                    break;
                                case ButtonCreateDuplicate:
                                    //
                                    // Create a Duplicate record (for email)
                                    //
                                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionDuplicate;
                                    adminContext.AdminForm = AdminFormEdit;
                                    break;
                            }
                            break;
                        case AdminFormStyleEditor:
                            //
                            // Process actions
                            //
                            switch (adminContext.requestButton) {
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
                                    EditorStyleRulesFilename = GenericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", "0", 1, 99, 1);
                                    core.cdnFiles.deleteFile(EditorStyleRulesFilename);
                                    //
                                    CS = core.db.csOpenSql( "select id from cctemplates");
                                    while (core.db.csOk(0)) {
                                        EditorStyleRulesFilename = GenericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", core.db.csGet(0, "ID"), 1, 99, 1);
                                        core.cdnFiles.deleteFile(EditorStyleRulesFilename);
                                        core.db.csGoNext(0);
                                    }
                                    core.db.csClose(ref CS);
                                    break;
                            }
                            //
                            // Process redirects
                            //
                            switch (adminContext.requestButton) {
                                case ButtonCancel:
                                case ButtonOK:
                                    adminContext.AdminForm = AdminFormRoot;
                                    break;
                            }
                            break;
                        default:
                            // end case
                            break;
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private string GetForm_EditTitle(AdminInfoDomainModel adminContext) {
            return "";
            // this info moved to description block
            //string tempGetForm_EditTitle = "";
            //try {
            //    //
            //    if (editRecord.id == 0) {
            //        tempGetForm_EditTitle = "Add an entry to " + editRecord.contentControlId_Name + TitleExtension;
            //    } else {
            //        tempGetForm_EditTitle = "Editing Record " + editRecord.id + " in " + editRecord.contentControlId_Name + " " + TitleExtension;
            //    }
            //} catch (Exception ex) {
            //    logController.handleError(core, ex);
            //}
            //return tempGetForm_EditTitle;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private string GetForm_EditTitleBar(AdminInfoDomainModel adminContext) {
            // todo
            AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
            //
            //adminUIController Adminui = new adminUIController(core);
            return AdminUIController.getTitleBar(core, GetForm_EditTitle(adminContext), "");
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private string GetForm_EditFormStart(AdminInfoDomainModel adminContext, int AdminFormID) {
            string s = "";
            try {
                core.html.addScriptCode("var docLoaded=false", "Form loader");
                core.html.addScriptCode_onLoad("docLoaded=true;", "Form loader");
                s = HtmlController.formMultipart_start(core, core.doc.refreshQueryString, "", "ccForm", "adminEditForm");
                s = GenericController.vbReplace(s, ">", " onSubmit=\"cj.admin.saveEmptyFieldList('FormEmptyFieldList');\" autocomplete=\"off\">");
                s += "\r\n<!-- block --><div class=\"d-none\"><input type=password name=\"password_block\" value=\"\"><input type=text name=\"username_block\" value=\"\"></div><!-- end block -->";
                s +=  "\r\n<input TYPE=\"hidden\" NAME=\"" + rnAdminSourceForm + "\" VALUE=\"" + AdminFormID.ToString() + "\">";
                s +=  "\r\n<input TYPE=\"hidden\" NAME=\"" + RequestNameTitleExtension + "\" VALUE=\"" + adminContext.TitleExtension + "\">";
                s +=  "\r\n<input TYPE=\"hidden\" NAME=\"" + RequestNameAdminDepth + "\" VALUE=\"" + adminContext.ignore_legacyMenuDepth + "\">";
                s +=  "\r\n<input TYPE=\"hidden\" NAME=\"FormEmptyFieldList\" ID=\"FormEmptyFieldList\" VALUE=\",\">";
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                    switch (GenericController.vbUCase(Name)) {
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
                                if (!core.session.user.Developer) {
                                    if (!core.session.user.Admin) {
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
                LogController.handleError(core, ex);
            }
            return tempIsVisibleUserField;
        }
        //
        //=============================================================================================
        // true if the field is an editable user field (can edit on edit form and save to database)
        //=============================================================================================
        //
        private bool IsFieldEditable(AdminInfoDomainModel adminContext, CDefFieldModel Field) {
            bool tempIsFieldEditable = false;
            try {
                //
                tempIsFieldEditable = IsVisibleUserField(Field.adminOnly, Field.developerOnly, Field.active, Field.authorable, Field.nameLc, adminContext.adminContent.tableName) && (!Field.readOnly) & (!Field.notEditable);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                LogController.handleError(core, ex);
            }
            return tempGetForm_Close;
        }
        //
        //=============================================================================================
        //
        //=============================================================================================
        //
        private void ProcessActionSave(AdminInfoDomainModel adminContext, bool UseContentWatchLink) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
                string EditorStyleRulesFilename = null;
                //
                if (true) {
                    //
                    //
                    //
                    if (!(core.doc.debug_iUserError != "")) {
                        //todo  NOTE: The following VB 'Select Case' included either a non-ordinal switch expression or non-ordinal, range-type, or non-constant 'Case' expressions and was converted to C# 'if-else' logic:
                        //						Select Case genericController.vbUCase(adminContext.content.ContentTableName)
                        //ORIGINAL LINE: Case genericController.vbUCase("ccMembers")
                        if (GenericController.vbUCase(adminContext.adminContent.tableName) == GenericController.vbUCase("ccMembers")) {
                            //
                            //
                            //

                            SaveEditRecord(adminContext);
                            SaveMemberRules(editRecord.id);
                            //Call SaveTopicRules
                        }
                        //ORIGINAL LINE: Case "CCEMAIL"
                        else if (GenericController.vbUCase(adminContext.adminContent.tableName) == "CCEMAIL") {
                            //
                            //
                            //
                            SaveEditRecord(adminContext);
                            // NO - ignore wwwroot styles, and create it on the fly during send
                            //If core.main_GetSiteProperty2("BuildVersion") >= "3.3.291" Then
                            //    Call core.app.executeSql( "update ccEmail set InlineStyles=" & encodeSQLText(core.main_GetStyleSheetProcessed) & " where ID=" & EditRecord.ID)
                            //End If
                            core.html.processCheckList("EmailGroups", "Group Email", GenericController.encodeText(editRecord.id), "Groups", "Email Groups", "EmailID", "GroupID");
                            core.html.processCheckList("EmailTopics", "Group Email", GenericController.encodeText(editRecord.id), "Topics", "Email Topics", "EmailID", "TopicID");
                        }
                        //ORIGINAL LINE: Case "CCCONTENT"
                        else if (GenericController.vbUCase(adminContext.adminContent.tableName) == "CCCONTENT") {
                            //
                            //
                            //
                            SaveEditRecord(adminContext);
                            LoadAndSaveGroupRules(editRecord);
                        }
                        //ORIGINAL LINE: Case "CCPAGECONTENT"
                        else if (GenericController.vbUCase(adminContext.adminContent.tableName) == "CCPAGECONTENT") {
                            //
                            //
                            //
                            SaveEditRecord(adminContext);
                            LoadContentTrackingDataBase(adminContext);
                            LoadContentTrackingResponse(adminContext);
                            //Call LoadAndSaveMetaContent()
                            SaveLinkAlias(adminContext);
                            //Call SaveTopicRules
                            SaveContentTracking(adminContext);
                        }
                        //ORIGINAL LINE: Case "CCLIBRARYFOLDERS"
                        else if (GenericController.vbUCase(adminContext.adminContent.tableName) == "CCLIBRARYFOLDERS") {
                            //
                            //
                            //
                            SaveEditRecord(adminContext);
                            LoadContentTrackingDataBase(adminContext);
                            LoadContentTrackingResponse(adminContext);
                            //Call LoadAndSaveCalendarEvents
                            //Call LoadAndSaveMetaContent()
                            core.html.processCheckList("LibraryFolderRules", adminContext.adminContent.name, GenericController.encodeText(editRecord.id), "Groups", "Library Folder Rules", "FolderID", "GroupID");
                            //call SaveTopicRules
                            SaveContentTracking(adminContext);
                        }
                        //ORIGINAL LINE: Case "CCSETUP"
                        else if (GenericController.vbUCase(adminContext.adminContent.tableName) == "CCSETUP") {
                            //
                            // Site Properties
                            //
                            SaveEditRecord(adminContext);
                            if (editRecord.nameLc.ToLower() == "allowlinkalias") {
                                if (core.siteProperties.getBoolean("AllowLinkAlias")) {
                                    TurnOnLinkAlias(UseContentWatchLink);
                                }
                            }
                        }
                        //ORIGINAL LINE: Case genericController.vbUCase("ccGroups")
                        else if (GenericController.vbUCase(adminContext.adminContent.tableName) == GenericController.vbUCase("ccGroups")) {
                            //Case "CCGROUPS"
                            //
                            //
                            //
                            SaveEditRecord(adminContext);
                            LoadContentTrackingDataBase(adminContext);
                            LoadContentTrackingResponse(adminContext);
                            LoadAndSaveContentGroupRules(editRecord.id);
                            //Call LoadAndSaveCalendarEvents
                            //Call LoadAndSaveMetaContent()
                            //call SaveTopicRules
                            SaveContentTracking(adminContext);
                            //Dim EditorStyleRulesFilename As String
                        }
                        //ORIGINAL LINE: Case "CCTEMPLATES"
                        else if (GenericController.vbUCase(adminContext.adminContent.tableName) == "CCTEMPLATES") {
                            //
                            // save and clear editorstylerules for this template
                            //
                            SaveEditRecord(adminContext);
                            LoadContentTrackingDataBase(adminContext);
                            LoadContentTrackingResponse(adminContext);
                            //Call LoadAndSaveCalendarEvents
                            //Call LoadAndSaveMetaContent()
                            //call SaveTopicRules
                            SaveContentTracking(adminContext);
                            //
                            EditorStyleRulesFilename = GenericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", editRecord.id.ToString(), 1, 99, 1);
                            core.privateFiles.deleteFile(EditorStyleRulesFilename);
                            //Case "CCSHAREDSTYLES"
                            //    '
                            //    ' save and clear editorstylerules for any template
                            //    '
                            //    Call SaveEditRecord(adminContext.content, editRecord)
                            //    Call LoadContentTrackingDataBase(adminContext.content, editRecord)
                            //    Call LoadContentTrackingResponse(adminContext.content, editRecord)
                            //    'Call LoadAndSaveCalendarEvents
                            //    Call LoadAndSaveMetaContent()
                            //    'call SaveTopicRules
                            //    Call SaveContentTracking(adminContext.content, editRecord)
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
                            SaveEditRecord(adminContext);
                            LoadContentTrackingDataBase(adminContext);
                            LoadContentTrackingResponse(adminContext);
                            //Call LoadAndSaveCalendarEvents
                            //Call LoadAndSaveMetaContent()
                            //call SaveTopicRules
                            SaveContentTracking(adminContext);
                        }
                    }
                }
                //
                // If the content supports datereviewed, mark it
                //
                if (core.doc.debug_iUserError != "") {
                    adminContext.AdminForm = adminContext.AdminSourceForm;
                }
                adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop; // convert so action can be used in as a refresh
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
        //            Content.Add( adminUIController.GetFormBodyAdminOnly()
        //        Else
        //            '
        //            ' Process Requests
        //            '
        //            SaveAction = (Button = ButtonSave) Or (Button = ButtonOK)
        //            '
        //            ButtonList = ButtonCancel & "," & ButtonSave & "," & ButtonOK
        //            Content.Add( adminUIController.EditTableOpen)
        //            '
        //            ' Common email addresses
        //            '
        //            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>General Email Addresses</b>" & kmaEndTableCell & kmaEndTableRow)
        //            '
        //            HelpCopy = "This is the Email address displayed throughout the site when a visitor is prompted to contact the site administrator."
        //            Copy = (GetPropertyControl("EmailAdmin", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "Admin Email Address", HelpCopy, False, False))
        //            '
        //            HelpCopy = "This is the Email address displayed throughout the site when a visitor is prompted to send site comments."
        //            Copy = (GetPropertyControl("EmailComments", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "Comment Email Address", HelpCopy, False, False))
        //            '
        //            HelpCopy = "This is the Email address used on out-going Emails when no other address is available. For your Email to get to its destination, this Email address must be a valid Email account on a mail server."
        //            Copy = (GetPropertyControl("EmailFromAddress", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "General Email From Address", HelpCopy, False, False))
        //            '
        //            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>Trap Email Handling</b>" & kmaEndTableCell & kmaEndTableRow)
        //            '
        //            HelpCopy = "When checked, all system errors (called traps errors) generate an Email to the Trap Email address."
        //            Copy = (GetPropertyControl("AllowTrapemail", FieldTypeBoolean, SaveAction, ""))
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "Allow Trap Error Email", HelpCopy, False, False))
        //            '
        //            HelpCopy = "This is the Email address to which all systems errors (called trap errors) are sent when Allow Trap Error Email is checked."
        //            Copy = (GetPropertyControl("TrapEmail", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "Trap Error Email Address", HelpCopy, False, False))
        //            '
        //            ' Email Sending
        //            '
        //            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>Sending Email</b>" & kmaEndTableCell & kmaEndTableRow)
        //            '
        //            HelpCopy = "This is the domain name or IP address of the SMTP mail server you will use to send. If you are using the MS SMTP in IIS on this machine, use 127.0.0.1."
        //            Copy = (GetPropertyControl("SMTPServer", FieldTypeText, SaveAction, "127.0.0.1"))
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "SMTP Email Server", HelpCopy, False, False))
        //            '
        //            HelpCopy = "When checked, the login box includes a section for users to enter their Email addresses. When submitted, all username and password matches for that Email address are sent to the Email address."
        //            Copy = (GetPropertyControl("AllowPasswordEmail", FieldTypeBoolean, SaveAction, ""))
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "Allow Password Email", HelpCopy, False, False))
        //    '
        //    ' read-only - no longer user configurable
        //    '
        //    '        '
        //            HelpCopy = "This text is included at the bottom of each group, system, and conditional email. It contains a link that the Email recipient can click to block them from future emails from this site. Only site developers can modify this text."
        //            If core.main_IsDeveloper Then
        //                HelpCopy = "<br><br>Developer: This text should conform to standards set by both local and federal law, as well as those required by your email server administrator. To create the clickable link, include link tags around your text (&lt%link&gt;click here&lt%/link&gt;). If you omit the link tag, a (click here) will be added to the end."
        //            End If
        //            Copy = (GetPropertyHTMLControl("EmailSpamFooter", SaveAction, DefaultSpamFooter, (Not core.main_IsDeveloper)))
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "Email SpamFooter", HelpCopy, False, True))
        //            '
        //            HelpCopy = "Group and Conditional Email are delivered from another program that checks in about every minute. This is the time and date of the last check."
        //            Copy = core.main_GetSiteProperty2("EmailServiceLastCheck")
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "Last Send Email Status", HelpCopy, False, False))
        //            '
        //            ' Bounce Email Handling
        //            '
        //            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>Bounce Email Handling</b>" & kmaEndTableCell & kmaEndTableRow)
        //            '
        //            HelpCopy = "If present, all outbound Emails that can not be delivered will be returned to this address. This should be a valid Email address on an Email server."
        //            Copy = (GetPropertyControl("EmailBounceAddress", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "Bounce Email Address", HelpCopy, False, False))
        //            '
        //            HelpCopy = "When checked, the system will attempt to retrieve bounced Emails from the following Email account and mark the members according to the processing rules included here."
        //            Copy = (GetPropertyControl("AllowEmailBounceProcessing", FieldTypeBoolean, SaveAction, ""))
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "Process Bounced Emails", HelpCopy, False, False))
        //            '
        //            HelpCopy = "The POP Email server where Emails will be retrieved and processed."
        //            Copy = (GetPropertyControl("POPServer", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "POP Email Server", HelpCopy, False, False))
        //            '
        //            HelpCopy = "The account username to retrieve Emails for processing."
        //            Copy = (GetPropertyControl("POPServerUsername", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "POP Email Username", HelpCopy, False, False))
        //            '
        //            HelpCopy = "The account password to retrieve Emails for processing."
        //            Copy = (GetPropertyControl("POPServerPassword", FieldTypeText, SaveAction, ""))
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "POP Email Password", HelpCopy, False, False))
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
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "Bounce Email Action", HelpCopy, False, False))
        //            '
        //            HelpCopy = "Bounce emails are retrieved about every minute. This is the status of the last check."
        //            Copy = core.main_GetSiteProperty2("POPServerStatus")
        //            Call Content.Add(adminUIController.GetEditRow(core, Copy, "Last Receive Email Status", HelpCopy, False, False))
        //            '
        //            Content.Add( adminUIController.EditTableClose)
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
        //        GetForm_EmailControl = adminUIController.GetBody(core, "Email Control", ButtonList, "", True, True, Description, "", 0, Content.Text)
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
                StringBuilderLegacyController Tab0 = new StringBuilderLegacyController();
                StringBuilderLegacyController Tab1 = new StringBuilderLegacyController();
                string Content = "";
                string Cell = null;
                //adminUIController Adminui = new adminUIController(core);
                string SQLFieldName = null;
                //
                const int ColumnCnt = 5;
                //
                Button = core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    return core.webServer.redirect("/" + core.appConfig.adminRoute, "Downloads, Cancel Button Pressed");
                }
                //
                if (!core.session.isAuthenticatedAdmin(core)) {
                    //
                    // Must be a developer
                    //
                    ButtonListLeft = ButtonCancel;
                    ButtonListRight = "";
                    Content = Content + AdminUIController.getFormBodyAdminOnly();
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
                                                    if (GenericController.vbLCase(core.db.csGetText(CSSrc, "command")) == "xml") {
                                                        core.db.csSet(CSDst, "Filename", "DupDownload_" + encodeText(GenericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(GenericController.GetRandomInteger(core)) + ".xml");
                                                        core.db.csSet(CSDst, "Command", "BUILDXML");
                                                    } else {
                                                        core.db.csSet(CSDst, "Filename", "DupDownload_" + encodeText(GenericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(GenericController.GetRandomInteger(core)) + ".csv");
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
                                        ContentName = CdefController.getContentNameByID(core, ContentID);
                                        TableName = CdefController.getContentTablename(core, ContentName);
                                        Criteria = CdefController.getContentControlCriteria(core, ContentName);
                                        Name = "CSV Download, " + ContentName;
                                        Filename = GenericController.vbReplace(ContentName, " ", "") + "_" + encodeText(GenericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(GenericController.GetRandomInteger(core)) + ".csv";
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
                                        ContentName = CdefController.getContentNameByID(core, ContentID);
                                        TableName = CdefController.getContentTablename(core, ContentName);
                                        Criteria = CdefController.getContentControlCriteria(core, ContentName);
                                        Name = "XML Download, " + ContentName;
                                        Filename = GenericController.vbReplace(ContentName, " ", "") + "_" + encodeText(GenericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(GenericController.GetRandomInteger(core)) + ".xml";
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
                    CS = core.db.csOpenSql(SQL, "Default", PageSize, PageNumber);
                    RowPointer = 0;
                    if (!core.db.csOk(CS)) {
                        Cells[0, 1] = "There are no download requests";
                        RowPointer = 1;
                    } else {
                        DataRowCount = core.db.csGetRowCount(CS);
                        LinkPrefix = "<a href=\"" + core.appConfig.cdnFileUrl;
                        LinkSuffix = "\" target=_blank>Available</a>";
                        while (core.db.csOk(CS) && (RowPointer < PageSize)) {
                            RecordID = core.db.csGetInteger(CS, "ID");
                            DateCompleted = core.db.csGetDate(CS, "DateCompleted");
                            ResultMessage = core.db.csGetText(CS, "ResultMessage");
                            Cells[RowPointer, 0] = HtmlController.checkbox("Row" + RowPointer) + HtmlController.inputHidden("RowID" + RowPointer, RecordID);
                            Cells[RowPointer, 1] = core.db.csGetText(CS, "name");
                            Cells[RowPointer, 2] = core.db.csGetText(CS, "CreatedByName");
                            Cells[RowPointer, 3] = core.db.csGetDate(CS, "DateAdded").ToShortDateString();
                            if (DateCompleted == DateTime.MinValue) {
                                RemoteKey = RemoteQueryController.main_GetRemoteQueryKey(core, "select DateCompleted,filename,resultMessage from cctasks where id=" + RecordID, "default", 1);
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
                                Cells[RowPointer, 4] = "<div id=\"pending" + RowPointer + "\"><a href=\"javascript:alert('" + GenericController.EncodeJavascriptStringSingleQuote(ResultMessage) + ";return false');\">error</a></div>";
                            }
                            RowPointer = RowPointer + 1;
                            core.db.csGoNext(CS);
                        }
                    }
                    core.db.csClose(ref CS);
                    Tab0.Add(HtmlController.inputHidden("RowCnt", RowPointer));
                    Cell = AdminUIController.getReport(core, RowPointer, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, "ccPanel");
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
                    Content = Content + HtmlController.inputHidden(rnAdminSourceForm, AdminFormDownloads);
                }
                //
                Caption = "Download Manager";
                Description = ""
                    + "<p>The Download Manager holds all downloads requested from anywhere on the website. It also provides tools to request downloads from any Content.</p>"
                    + "<p>To add a new download of any content in Contensive, click Export on the filter tab of the content listing page. To add a new download from a SQL statement, use Custom Reports under Reports on the Navigator.</p>";
                ContentPadding = 0;
                tempGetForm_Downloads = AdminUIController.getBody(core, Caption, ButtonListLeft, ButtonListRight, true, true, Description, ContentSummary, ContentPadding, Content);
                //
                core.html.addTitle(Caption);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                        core.doc.menuComboTab.AddEntry(Caption.Replace(" ", "&nbsp;"), "", "", Content, false, "ccAdminTab");
                        //Call core.htmldoc.main_AddLiveTabEntry(Replace(Caption, " ", "&nbsp;"), Content, "ccAdminTab")
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                    core.doc.menuComboTab.AddEntry(Caption.Replace(" ", "&nbsp;"), "", AjaxLink, "", false, "ccAdminTab");
                } else {
                    //
                    // Live Tab
                    //
                    core.doc.menuComboTab.AddEntry(Caption.Replace(" ", "&nbsp;"), "", "", Content, false, "ccAdminTab");
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
        private string GetForm_Edit_Tabs(AdminInfoDomainModel adminContext, bool readOnlyField, bool IsLandingPage, bool IsRootPage, ContentTypeEnum EditorContext, bool allowAjaxTabs, int TemplateIDForStyles, string[] fieldTypeDefaultEditors, string fieldEditorPreferenceList, string styleList, string styleOptionList, int emailIdForStyles, bool IsTemplateTable, string editorAddonListJSON) {
            string returnHtml = "";
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
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
                KeyPtrController helpIdIndex = new KeyPtrController();
                string fieldNameLc = null;
                //
                // ----- read in help
                //
                IDList = "";
                foreach (KeyValuePair<string, CDefFieldModel> keyValuePair in adminContext.adminContent.fields) {
                    CDefFieldModel field = keyValuePair.Value;
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
                        fieldId = GenericController.encodeInteger(TempVar[0, HelpPtr]);
                        if (fieldId != LastFieldID) {
                            LastFieldID = fieldId;
                            HelpIDCache[HelpPtr] = fieldId;
                            helpIdIndex.setPtr(fieldId.ToString(), HelpPtr);
                            helpDefaultCache[HelpPtr] = GenericController.encodeText(TempVar[1, HelpPtr]);
                            HelpCustomCache[HelpPtr] = GenericController.encodeText(TempVar[2, HelpPtr]);
                        }
                    }
                    AllowHelpMsgCustom = true;
                }
                //
                FormFieldList = ",";
                foreach (KeyValuePair<string, CDefFieldModel> keyValuePair in adminContext.adminContent.fields) {
                    CDefFieldModel field = keyValuePair.Value;
                    if ((field.authorable) & (field.active) && (!TabsFound.Contains(field.editTabName.ToLower()))) {
                        TabsFound.Add(field.editTabName.ToLower());
                        fieldNameLc = field.nameLc;
                        editTabCaption = field.editTabName;
                        if (string.IsNullOrEmpty(editTabCaption)) {
                            editTabCaption = "Details";
                        }
                        NewFormFieldList = "";
                        if ((!adminContext.allowAdminTabs) | (!allowAjaxTabs) || (editTabCaption.ToLower() == "details")) {
                            //
                            // Live Tab (non-tab mode, non-ajax mode, or details tab
                            //
                            tabContent = GetForm_Edit_Tab(adminContext, editRecord.id, adminContext.adminContent.id, readOnlyField, IsLandingPage, IsRootPage, field.editTabName, EditorContext, ref NewFormFieldList, TemplateIDForStyles, HelpCnt, HelpIDCache, helpDefaultCache, HelpCustomCache, AllowHelpMsgCustom, helpIdIndex, fieldTypeDefaultEditors, fieldEditorPreferenceList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON);
                            if (!string.IsNullOrEmpty(tabContent)) {
                                returnHtml += GetForm_Edit_AddTab2(editTabCaption, tabContent, adminContext.allowAdminTabs, "");
                            }
                        } else {
                            //
                            // Ajax Tab
                            //
                            //AjaxLink = "/admin/index.asp?"
                            AjaxLink = "/" + core.appConfig.adminRoute + "?"
                            + RequestNameAjaxFunction + "=" + AjaxGetFormEditTabContent + "&ID=" + editRecord.id + "&CID=" + adminContext.adminContent.id + "&ReadOnly=" + readOnlyField + "&IsLandingPage=" + IsLandingPage + "&IsRootPage=" + IsRootPage + "&EditTab=" + GenericController.encodeRequestVariable(field.editTabName) + "&EditorContext=" + EditorContext + "&NewFormFieldList=" + GenericController.encodeRequestVariable(NewFormFieldList);
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
                returnHtml += HtmlController.inputHidden("FormFieldList", FormFieldList);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                StringBuilderLegacyController Tab0 = new StringBuilderLegacyController();
                StringBuilderLegacyController Tab1 = new StringBuilderLegacyController();
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
                Description = "Custom Reports are a way for you to create a snapshot of data to view or download. To request a report, select the Custom Reports tab, check the report(s) you want, and click the [Request Download] Button. When your report is ready, it will be available in the <a href=\"?" + rnAdminForm + "=30\">Download Manager</a>. To create a new custom report, select the Request New Report tab, enter a name and SQL statement, and click the Apply button.";
                ContentPadding = 0;
                ButtonListLeft = ButtonCancel + "," + ButtonDelete + "," + ButtonRequestDownload;
                //ButtonListLeft = ButtonCancel & "," & ButtonDelete & "," & ButtonRequestDownload & "," & ButtonApply
                ButtonListRight = "";
                SQLFieldName = "SQLQuery";
                //
                if (!core.session.isAuthenticatedAdmin(core)) {
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
                                        ErrorController.addUserError(core, "A name and SQL Query are required to save a new custom report.");
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
                                                Filename = "CustomReport_" + encodeText(GenericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(GenericController.GetRandomInteger(core)) + ".csv";
                                                core.db.csSet(CS, "Name", RecordName);
                                                core.db.csSet(CS, "Filename", Filename);
                                                if (Format == "XML") {
                                                    core.db.csSet(CS, "Command", "BUILDXML");
                                                } else {
                                                    core.db.csSet(CS, "Command", "BUILDCSV");
                                                }
                                                core.db.csSet(CS, SQLFieldName, SQL);
                                                Description = Description + "<p>Your Download [" + Name + "] has been requested, and will be available in the <a href=\"?" + rnAdminForm + "=30\">Download Manager</a> when it is complete. This may take a few minutes depending on the size of the report.</p>";
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
                            Cells[RowPointer, 0] = HtmlController.checkbox("Row" + RowPointer) + HtmlController.inputHidden("RowID" + RowPointer, RecordID);
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
                    Tab0.Add(HtmlController.inputHidden("RowCnt", RowPointer));
                    //adminUIController Adminui = new adminUIController(core);
                    Cell = AdminUIController.getReport(core, RowPointer, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, "ccPanel");
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
                    Tab1.Add("<td>" + HtmlController.inputText(core, "Name", "", 1, 40) + "</td>");
                    Tab1.Add("</tr>");
                    //
                    Tab1.Add("<tr>");
                    Tab1.Add("<td align=right>SQL Query</td>");
                    Tab1.Add("<td>" + HtmlController.inputText(core, SQLFieldName, "", 8, 40) + "</td>");
                    Tab1.Add("</tr>");
                    //
                    Tab1.Add("<tr><td width=\"120\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"120\" height=\"1\"></td><td width=\"100%\">&nbsp;</td></tr></table>");
                    //
                    // Build and add tabs
                    //
                    core.doc.menuLiveTab.AddEntry("Custom&nbsp;Reports", Tab0.Text, "ccAdminTab");
                    core.doc.menuLiveTab.AddEntry("Request&nbsp;New&nbsp;Report", Tab1.Text, "ccAdminTab");
                    Content = core.doc.menuLiveTab.GetTabs(core);
                    //
                }
                //
                tempGetForm_CustomReports = admin_GetAdminFormBody(Caption, ButtonListLeft, ButtonListRight, true, true, Description, ContentSummary, ContentPadding, Content);
                //
                core.html.addTitle("Custom Reports");
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_CustomReports;
        }
        //
        //
        //=============================================================================================
        //   Create a duplicate record
        //=============================================================================================
        //
        private void ProcessActionDuplicate(AdminInfoDomainModel adminContext) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminContext.editRecord;
                //
                // converted array to dictionary - Dim FieldPointer As Integer
                //
                if (!(core.doc.debug_iUserError != "")) {
                    switch (GenericController.vbUCase(adminContext.adminContent.tableName)) {
                        case "CCEMAIL":
                            //
                            // --- preload array with values that may not come back in response
                            //
                            LoadEditRecord(adminContext);
                            LoadEditRecord_Request(adminContext);
                            //
                            if (!(core.doc.debug_iUserError != "")) {
                                //
                                // ----- Convert this to the Duplicate
                                //
                                if (adminContext.adminContent.fields.ContainsKey("submitted")) {
                                    editRecord.fieldsLc["submitted"].value = false;
                                }
                                if (adminContext.adminContent.fields.ContainsKey("sent")) {
                                    editRecord.fieldsLc["sent"].value = false;
                                }
                                //
                                editRecord.id = 0;
                                core.doc.addRefreshQueryString("id", GenericController.encodeText(editRecord.id));
                            }
                            break;
                        default:
                            //
                            //
                            //
                            //
                            // --- preload array with values that may not come back in response
                            //
                            LoadEditRecord(adminContext);
                            LoadEditRecord_Request(adminContext);
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
                                foreach (KeyValuePair<string, Contensive.Processor.Models.Domain.CDefFieldModel> keyValuePair in adminContext.adminContent.fields) {
                                    CDefFieldModel field = keyValuePair.Value;
                                    if (GenericController.vbLCase(field.nameLc) == "email") {
                                        if ((adminContext.adminContent.tableName.ToLower() == "ccmembers") && (GenericController.encodeBoolean(core.siteProperties.getBoolean("allowemaillogin", false)))) {
                                            editRecord.fieldsLc[field.nameLc].value = "";
                                        }
                                    }
                                    if (field.uniqueName) {
                                        editRecord.fieldsLc[field.nameLc].value = "";
                                    }
                                }
                                //
                                core.doc.addRefreshQueryString("id", GenericController.encodeText(editRecord.id));
                            }
                            //Call core.htmldoc.main_AddUserError("The create duplicate action is not supported for this content.")
                            break;
                    }
                    adminContext.AdminForm = adminContext.AdminSourceForm;
                    adminContext.Admin_Action = AdminInfoDomainModel.AdminActionNop; // convert so action can be used in as a refresh
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //========================================================================
        // PrintMenuTop()
        //   Prints the menu section of the admin page
        //========================================================================
        //
        private string GetMenuTopMode(AdminInfoDomainModel adminContext) {
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
                if (adminContext.AdminMenuModeID == AdminMenuModeTop) {
                    //
                    // ----- Get the baked version
                    //
                    BakeName = "AdminMenu" + core.session.user.id.ToString("00000000");
                    tempGetMenuTopMode = GenericController.encodeText(core.cache.getObject<string>(BakeName));
                    MenuDelimiterPosition = GenericController.vbInstr(1, tempGetMenuTopMode, MenuDelimiter, 1);
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
                            IsAdminLocal = core.session.isAuthenticatedAdmin(core);
                            if (!IsAdminLocal) {
                                //
                                // content managers, need the ContentManagementList
                                //
                                editableCdefIdList = CdefController.getEditableCdefIdList(core);
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
                                    if (GenericController.vbInstr(1, Link, "?") == 1) {
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
                                core.menuFlyout.menu_AddEntry(GenericController.encodeText(Id), ParentID.ToString(), ImageLink, ImageOverLink, Link, LinkLabel, StyleSheet, StyleSheetHover, NewWindow);

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
                                MenuHeader = core.menuFlyout.getMenu(GenericController.encodeText(Id), 0);
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
                        var depList = new List<string> { };
                        depList.Add(PersonModel.getTableInvalidationKey(core));
                        depList.Add(ContentModel.getTableInvalidationKey(core));
                        depList.Add(NavigatorEntryModel.getTableInvalidationKey(core));
                        depList.Add(GroupModel.getTableInvalidationKey(core));
                        depList.Add(GroupRuleModel.getTableInvalidationKey(core));
                        core.cache.setObject(BakeName, tempGetMenuTopMode + MenuDelimiter + MenuClose, depList);
                    }
                    core.doc.htmlForEndOfBody = core.doc.htmlForEndOfBody + MenuClose;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                                core.db.deprecate_argsreversed_deleteTableRecord("ccMemberRules", MemberRuleID, "Default");
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
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
                StringBuilderLegacyController Content = new StringBuilderLegacyController();
                string FieldValue = null;
                bool NewGroup = false;
                int GroupID = 0;
                string NewGroupName = "";
                string Button = null;
                //adminUIController Adminui = new adminUIController(core);
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
                } else if (!core.session.isAuthenticatedAdmin(core)) {
                    //
                    //
                    //
                    ButtonList = ButtonCancel;
                    Content.Add(AdminUIController.getFormBodyAdminOnly());
                } else {
                    //
                    if (Button != ButtonOK) {
                        //
                        // Load defaults
                        //
                        ParentContentID = core.docProperties.getInteger("ParentContentID");
                        if (ParentContentID == 0) {
                            ParentContentID = CdefController.getContentId(core, "Page Content");
                        }
                        AddAdminMenuEntry = true;
                        GroupID = 0;
                    } else {
                        //
                        // Process input
                        //
                        ParentContentID = core.docProperties.getInteger("ParentContentID");
                        ParentContentName = CdefController.getContentNameByID(core, ParentContentID);
                        ChildContentName = core.docProperties.getText("ChildContentName");
                        AddAdminMenuEntry = core.docProperties.getBoolean("AddAdminMenuEntry");
                        GroupID = core.docProperties.getInteger("GroupID");
                        NewGroup = core.docProperties.getBoolean("NewGroup");
                        NewGroupName = core.docProperties.getText("NewGroupName");
                        //
                        if ((string.IsNullOrEmpty(ParentContentName)) || (string.IsNullOrEmpty(ChildContentName))) {
                            ErrorController.addUserError(core, "You must select a parent and provide a child name.");
                        } else {
                            //
                            // Create Definition
                            //
                            Description = Description + "<div>&nbsp;</div>"
                                + "<div>Creating content [" + ChildContentName + "] from [" + ParentContentName + "]</div>";
                            CdefController.createContentChild(core, ChildContentName, ParentContentName, core.session.user.id);
                            ChildContentID = CdefController.getContentId(core, ChildContentName);
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
                                + "<div>Your new content is ready. <a href=\"?" + rnAdminForm + "=22\">Click here</a> to create another Content Definition, or hit [Cancel] to return to the main menu.</div>";
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
                        Content.Add(AdminUIController.editTableOpen);
                        //
                        FieldValue = "<select size=\"1\" name=\"ParentContentID\" ID=\"\"><option value=\"\">Select One</option>";
                        FieldValue = FieldValue + GetContentChildTool_Options(0, ParentContentID);
                        FieldValue = FieldValue + "</select>";
                        //FieldValue = core.htmldoc.main_GetFormInputSelect2("ParentContentID", CStr(ParentContentID), "Content", "(AllowContentChildTool<>0)")

                        Content.Add(AdminUIController.getEditRowLegacy(core, FieldValue, "Parent Content Name", "", false, false, ""));
                        //
                        FieldValue = HtmlController.inputText(core, "ChildContentName", ChildContentName, 1, 40);
                        Content.Add(AdminUIController.getEditRowLegacy(core, FieldValue, "New Child Content Name", "", false, false, ""));
                        //
                        FieldValue = core.html.inputRadio("NewGroup", false.ToString(), NewGroup.ToString()) + core.html.selectFromContent("GroupID", GroupID, "Groups", "", "", "", ref IsEmptyList) + "(Select a current group)"
                            + "<br>" + core.html.inputRadio("NewGroup", true.ToString(), NewGroup.ToString()) + HtmlController.inputText(core, "NewGroupName", NewGroupName) + "(Create a new group)";
                        Content.Add(AdminUIController.getEditRowLegacy(core, FieldValue, "Content Manager Group", "", false, false, ""));
                        //            '
                        //            FieldValue = core.main_GetFormInputCheckBox2("AddAdminMenuEntry", AddAdminMenuEntry) & "(Add Navigator Entry under Manager Site Content &gt; Advanced)"
                        //            Call Content.Add(adminUIController.GetEditRow(core, FieldValue, "Add Menu Entry", "", False, False, ""))
                        //
                        Content.Add(AdminUIController.editTableClose);
                        Content.Add("</td></tr>" + kmaEndTable);
                        //
                        ButtonList = ButtonOK + "," + ButtonCancel;
                    }
                    Content.Add(HtmlController.inputHidden(rnAdminSourceForm, AdminFormContentChildTool));
                }
                //
                Caption = "Create Content Definition";
                Description = "<div>This tool is used to create content definitions that help segregate your content into authorable segments.</div>" + Description;
                result = AdminUIController.getBody(core, Caption, ButtonList, "", false, false, Description, "", 0, Content.Text);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                CS = core.db.csOpenSql(SQL, "Default");
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
                LogController.handleError(core, ex);
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
                StringBuilderLegacyController Content = new StringBuilderLegacyController();
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
                //adminUIController Adminui = new adminUIController(core);
                string ButtonList = "";
                string Description = null;
                //
                Button = core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    //
                    //
                    //
                    return core.webServer.redirect("/" + core.appConfig.adminRoute, "HouseKeepingControl, Cancel Button Pressed");
                } else if (!core.session.isAuthenticatedAdmin(core)) {
                    //
                    //
                    //
                    ButtonList = ButtonCancel;
                    Content.Add(AdminUIController.getFormBodyAdminOnly());
                } else {
                    //
                    Content.Add(AdminUIController.editTableOpen);
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
                            core.siteProperties.setProperty("ArchiveRecordAgeDays", GenericController.encodeText(ArchiveRecordAgeDays));
                            //
                            ArchiveTimeOfDay = core.docProperties.getText("ArchiveTimeOfDay");
                            core.siteProperties.setProperty("ArchiveTimeOfDay", ArchiveTimeOfDay);
                            //
                            ArchiveAllowFileClean = core.docProperties.getBoolean("ArchiveAllowFileClean");
                            core.siteProperties.setProperty("ArchiveAllowFileClean", GenericController.encodeText(ArchiveAllowFileClean));
                            break;
                    }
                    //
                    if (Button == ButtonOK) {
                        return core.webServer.redirect("/" + core.appConfig.adminRoute, "StaticPublishControl, OK Button Pressed");
                    }
                    //
                    // ----- Status
                    //
                    Content.Add(HtmlController.tableRowStart() + "<td colspan=\"3\" class=\"ccPanel3D ccAdminEditSubHeader\"><b>Status</b>" + tableCellEnd + kmaEndTableRow);
                    //
                    // ----- Visits Found
                    //
                    PagesTotal = 0;
                    SQL = "SELECT Count(ID) as Result FROM ccVisits;";
                    CSServers = core.db.csOpenSql(SQL, "Default");
                    if (core.db.csOk(CSServers)) {
                        PagesTotal = core.db.csGetInteger(CSServers, "Result");
                    }
                    core.db.csClose(ref CSServers);
                    Content.Add(AdminUIController.getEditRowLegacy(core, SpanClassAdminNormal + PagesTotal, "Visits Found", "", false, false, ""));
                    //
                    // ----- Oldest Visit
                    //
                    Copy = "unknown";
                    AgeInDays = "unknown";
                    SQL = core.db.getSQLSelect("default", "ccVisits", "DateAdded", "", "ID", "", 1);
                    CSServers = core.db.csOpenSql(SQL, "Default");
                    //SQL = "SELECT Top 1 DateAdded FROM ccVisits order by ID;"
                    //CSServers = core.app_openCsSql_Rev_Internal("Default", SQL)
                    if (core.db.csOk(CSServers)) {
                        DateValue = core.db.csGetDate(CSServers, "DateAdded");
                        if (DateValue != DateTime.MinValue) {
                            Copy = GenericController.encodeText(DateValue);
                            AgeInDays = GenericController.encodeText(encodeInteger(Math.Floor(encodeNumber(core.doc.profileStartTime - DateValue))));
                        }
                    }
                    core.db.csClose(ref CSServers);
                    Content.Add(AdminUIController.getEditRowLegacy(core, SpanClassAdminNormal + Copy + " (" + AgeInDays + " days)", "Oldest Visit", "", false, false, ""));
                    //
                    // ----- Viewings Found
                    //
                    PagesTotal = 0;
                    SQL = "SELECT Count(ID) as result  FROM ccViewings;";
                    CSServers = core.db.csOpenSql(SQL, "Default");
                    if (core.db.csOk(CSServers)) {
                        PagesTotal = core.db.csGetInteger(CSServers, "Result");
                    }
                    core.db.csClose(ref CSServers);
                    Content.Add(AdminUIController.getEditRowLegacy(core, SpanClassAdminNormal + PagesTotal, "Viewings Found", "", false, false, ""));
                    //
                    Content.Add(HtmlController.tableRowStart() + "<td colspan=\"3\" class=\"ccPanel3D ccAdminEditSubHeader\"><b>Options</b>" + tableCellEnd + kmaEndTableRow);
                    //
                    Caption = "Archive Age";
                    Copy = HtmlController.inputText(core, "ArchiveRecordAgeDays", ArchiveRecordAgeDays.ToString(), -1, 20) + "&nbsp;Number of days to keep visit records. 0 disables housekeeping.";
                    Content.Add(AdminUIController.getEditRowLegacy(core, Copy, Caption));
                    //
                    Caption = "Housekeeping Time";
                    Copy = HtmlController.inputText(core, "ArchiveTimeOfDay", ArchiveTimeOfDay, -1, 20) + "&nbsp;The time of day when record deleting should start.";
                    Content.Add(AdminUIController.getEditRowLegacy(core, Copy, Caption));
                    //
                    Caption = "Purge Content Files";
                    Copy = HtmlController.checkbox("ArchiveAllowFileClean", ArchiveAllowFileClean) + "&nbsp;Delete Contensive content files with no associated database record.";
                    Content.Add(AdminUIController.getEditRowLegacy(core, Copy, Caption));
                    //
                    Content.Add(AdminUIController.editTableClose);
                    Content.Add(HtmlController.inputHidden(rnAdminSourceForm, AdminformHousekeepingControl));
                    ButtonList = ButtonCancel + ",Refresh," + ButtonSave + "," + ButtonOK;
                }
                //
                Caption = "Data Housekeeping Control";
                Description = "This tool is used to control the database record housekeeping process. This process deletes visit history records, so care should be taken before making any changes.";
                tempGetForm_HouseKeepingControl = AdminUIController.getBody(core, Caption, ButtonList, "", false, false, Description, "", 0, Content.Text);
                //
                core.html.addTitle(Caption);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                    tempGetPropertyHTMLControl = "<div style=\"border:1px solid #808080; padding:20px;\">" + HtmlController.decodeHtml(core.siteProperties.getText(Name, DefaultValue)) + "</div>";
                } else if (ProcessRequest) {
                    CurrentValue = core.docProperties.getText(Name);
                    core.siteProperties.setProperty(Name, CurrentValue);
                    tempGetPropertyHTMLControl = core.html.getFormInputHTML(Name, CurrentValue);
                } else {
                    CurrentValue = core.siteProperties.getText(Name, DefaultValue);
                    tempGetPropertyHTMLControl = core.html.getFormInputHTML(Name, CurrentValue);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
            //                Content.Add(adminUIController.GetFormBodyAdminOnly())
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
            //            admin_GetForm_StyleEditor = adminUIController.GetBody(core,"Site Styles", ButtonList, "", True, True, Description, "", 0, Content.Text)
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
        ////
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
                    ErrorController.addUserError(core, "Existing pages could not be checked for Link Alias names because there was another error on this page. Correct this error, and turn Link Alias on again to rerun the verification.");
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
                            LinkAliasController.addLinkAlias(core, linkAlias, core.db.csGetInteger(CS, "ID"), "", false, true);
                        } else {
                            //
                            // Add the name
                            //
                            linkAlias = core.db.csGetText(CS, "name");
                            if (!string.IsNullOrEmpty(linkAlias)) {
                                LinkAliasController.addLinkAlias(core, linkAlias, core.db.csGetInteger(CS, "ID"), "", false, false);
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
                        ErrorList = ErrorController.getUserError(core);
                        ErrorList = GenericController.vbReplace(ErrorList, UserErrorHeadline, "", 1, 99, 1);
                        ErrorController.addUserError(core, "The following errors occurred while verifying Link Alias entries for your existing pages." + ErrorList);
                        //Call core.htmldoc.main_AddUserError(ErrorList)
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                StringBuilderLegacyController Content = new StringBuilderLegacyController();
                string Button = null;
                string Copy = null;
                string ButtonList = null;
                //adminUIController Adminui = new adminUIController(core);
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
                    if (!core.session.isAuthenticatedAdmin(core)) {
                        //
                        // Does not have permission
                        //
                        ButtonList = ButtonCancel;
                        Content.Add(AdminUIController.getFormBodyAdminOnly());
                        core.html.addTitle("Style Editor");
                        tempGetForm_EditConfig = AdminUIController.getBody(core, "Site Styles", ButtonList, "", true, true, Description, "", 0, Content.Text);
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
                                if (GenericController.vbLCase(FeatureName) == "styleandformatting") {
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
                            EditorStyleRulesFilename = GenericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", "0", 1, 99, 1);
                            core.privateFiles.deleteFile(EditorStyleRulesFilename);
                            //
                            CS = core.db.csOpenSql( "select id from cctemplates");
                            while (core.db.csOk(CS)) {
                                EditorStyleRulesFilename = GenericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", core.db.csGet(CS, "ID"), 1, 99, 1);
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
                            FeatureList = core.cdnFiles.readFileText(InnovaEditorFeaturefilename);
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
                                if (GenericController.vbLCase(FeatureName) == "styleandformatting") {
                                    //
                                    // hide and force on during process - editor bug I think.
                                    //
                                } else {
                                    TDLeft = HtmlController.tableCellStart("", 0, encodeBoolean(RowPtr % 2), "left");
                                    TDCenter = HtmlController.tableCellStart("", 0, encodeBoolean(RowPtr % 2), "center");
                                    AllowAdmin = GenericController.encodeBoolean("," + AdminList + ",".IndexOf("," + FeatureName + ",", System.StringComparison.OrdinalIgnoreCase) + 1);
                                    AllowCM = GenericController.encodeBoolean("," + CMList + ",".IndexOf("," + FeatureName + ",", System.StringComparison.OrdinalIgnoreCase) + 1);
                                    AllowPublic = GenericController.encodeBoolean("," + PublicList + ",".IndexOf("," + FeatureName + ",", System.StringComparison.OrdinalIgnoreCase) + 1);
                                    Copy += "\r\n<tr>"
                                        + TDLeft + FeatureName + "</td>"
                                        + TDCenter + HtmlController.checkbox(FeatureName + ".admin", AllowAdmin) + "</td>"
                                        + TDCenter + HtmlController.checkbox(FeatureName + ".cm", AllowCM) + "</td>"
                                        + TDCenter + HtmlController.checkbox(FeatureName + ".public", AllowPublic) + "</td>"
                                        + "</tr>";
                                    RowPtr = RowPtr + 1;
                                }
                            }
                            Copy = ""
                                + "\r\n<div><b>body background style color</b> (default='white')</div>"
                                + "\r\n<div>" + HtmlController.inputText(core, "editorbackgroundcolor", core.siteProperties.getText("Editor Background Color", "white")) + "</div>"
                                + "\r\n<div>&nbsp;</div>"
                                + "\r\n<div><b>Toolbar features available</b></div>"
                                + "\r\n<table border=\"0\" cellpadding=\"4\" cellspacing=\"0\" width=\"500px\" align=left>" + GenericController.nop(Copy) + "\r\n" + kmaEndTable;
                            Copy = "\r\n" + HtmlController.tableStart(20, 0, 0) + "<tr><td>" + GenericController.nop(Copy) + "</td></tr>\r\n" + kmaEndTable;
                            Content.Add(Copy);
                            ButtonList = ButtonCancel + "," + ButtonRefresh + "," + ButtonSave + "," + ButtonOK;
                            Content.Add(HtmlController.inputHidden(rnAdminSourceForm, AdminFormEditorConfig));
                            core.html.addTitle("Editor Settings");
                            tempGetForm_EditConfig = AdminUIController.getBody(core, "Editor Configuration", ButtonList, "", true, true, Description, "", 0, Content.Text);
                        }
                    }
                    //
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                StringBuilderLegacyController Content = new StringBuilderLegacyController();
                string Button = null;
                //adminUIController Adminui = new adminUIController(core);
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
                } else if (!core.session.isAuthenticatedAdmin(core)) {
                    //
                    // Not Admin Error
                    //
                    ButtonList = ButtonCancel;
                    Content.Add(AdminUIController.getFormBodyAdminOnly());
                } else {
                    Content.Add(AdminUIController.editTableOpen);
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
                            core.siteProperties.setProperty("AllowAutoLogin", GenericController.encodeText(AllowAutoLogin));
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

                    Copy = HtmlController.checkbox("AllowAutoLogin", AllowAutoLogin);
                    Copy += "<div>When checked, returning users are automatically logged-in, without requiring a username or password. This is very convenient, but creates a high security risk. Each time you login, you will be given the option to not allow Auto-Login from that computer.</div>";
                    Content.Add(AdminUIController.getEditRowLegacy(core, Copy, "Allow Auto Login", "", false, false, ""));
                    //
                    // Buttons
                    //
                    ButtonList = ButtonCancel + "," + ButtonSave + "," + ButtonOK;
                    //
                    // Close Tables
                    //
                    Content.Add(AdminUIController.editTableClose);
                    Content.Add(HtmlController.inputHidden(rnAdminSourceForm, AdminFormBuilderCollection));
                }
                //
                Description = "Use this tool to modify the site security settings";
                tempGetForm_BuildCollection = AdminUIController.getBody(core, "Security Settings", ButtonList, "", true, true, Description, "", 0, Content.Text);
                Content = null;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_BuildCollection;
        }
        //
        //=================================================================================
        //
        //=================================================================================
        //
        public static void setIndexSQL_SaveIndexConfig(CoreController core, IndexConfigClass IndexConfig) {
            //
            // --Find words
            string SubList = "";
            foreach (var kvp in IndexConfig.FindWords) {
                IndexConfigClass.IndexConfigFindWordClass findWord = kvp.Value;
                if ((!string.IsNullOrEmpty(findWord.Name)) & (findWord.MatchOption != FindWordMatchEnum.MatchIgnore)) {
                    SubList = SubList + "\r\n" + findWord.Name + "\t" + findWord.Value + "\t" + (int)findWord.MatchOption;
                }
            }
            string FilterText = "";
            if (!string.IsNullOrEmpty(SubList)) {
                FilterText += "\r\nFindWordList" + SubList + "\r\n";
            }
            //
            // --CDef List
            if (IndexConfig.SubCDefID > 0) {
                FilterText += "\r\nCDefList\r\n" + IndexConfig.SubCDefID + "\r\n";
            }
            //
            // -- Group List
            SubList = "";
            if (IndexConfig.GroupListCnt > 0) {
                //
                int Ptr = 0;
                for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++) {
                    if (!string.IsNullOrEmpty(IndexConfig.GroupList[Ptr])) {
                        SubList = SubList + "\r\n" + IndexConfig.GroupList[Ptr];
                    }
                }
            }
            if (!string.IsNullOrEmpty(SubList)) {
                FilterText += "\r\nGroupList" + SubList + "\r\n";
            }
            //
            // PageNumber and Records Per Page
            FilterText += "\r\n"
                + "\r\npagenumber"
                + "\r\n" + IndexConfig.PageNumber;
            FilterText += "\r\n"
                + "\r\nrecordsperpage"
                + "\r\n" + IndexConfig.RecordsPerPage;
            //
            // misc filters
            if (IndexConfig.ActiveOnly) {
                FilterText += "\r\n"
                    + "\r\nIndexFilterActiveOnly";
            }
            if (IndexConfig.LastEditedByMe) {
                FilterText += "\r\n"
                    + "\r\nIndexFilterLastEditedByMe";
            }
            if (IndexConfig.LastEditedToday) {
                FilterText += "\r\n"
                    + "\r\nIndexFilterLastEditedToday";
            }
            if (IndexConfig.LastEditedPast7Days) {
                FilterText += "\r\n"
                    + "\r\nIndexFilterLastEditedPast7Days";
            }
            if (IndexConfig.LastEditedPast30Days) {
                FilterText += "\r\n"
                    + "\r\nIndexFilterLastEditedPast30Days";
            }
            if (IndexConfig.Open) {
                FilterText += "\r\n"
                    + "\r\nIndexFilterOpen";
            }
            //
            core.visitProperty.setProperty(AdminInfoDomainModel.IndexConfigPrefix + encodeText(IndexConfig.ContentID), FilterText);
            //
            //   Member Properties (persistant)
            //
            // Save Admin Column
            SubList = "";
            foreach (var column in IndexConfig.columns) {
                if (!string.IsNullOrEmpty(column.Name)) {
                    SubList = SubList + "\r\n" + column.Name + "\t" + column.Width;
                }
            }
            FilterText = "";
            if (!string.IsNullOrEmpty(SubList)) {
                FilterText += "\r\nColumns" + SubList + "\r\n";
            }
            //
            // Sorts
            //
            SubList = "";
            foreach (var kvp in IndexConfig.Sorts) {
                IndexConfigClass.IndexConfigSortClass sort = kvp.Value;
                if (!string.IsNullOrEmpty(sort.fieldName)) {
                    SubList = SubList + "\r\n" + sort.fieldName + "\t" + sort.direction;
                }
            }
            if (!string.IsNullOrEmpty(SubList)) {
                FilterText += "\r\nSorts" + SubList + "\r\n";
            }
            core.userProperty.setProperty(AdminInfoDomainModel.IndexConfigPrefix + encodeText(IndexConfig.ContentID), FilterText);
            //

        }
    }
}
