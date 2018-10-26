
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
        private CPClass cp;
        //
        private CoreController core;
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
        //====================================================================================================
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
                    var adminData = new AdminDataModel(core);
                    core.db.sqlCommandTimeout = 300;
                    adminData.ButtonObjectCount = 0;
                    adminData.JavaScriptString = "";
                    adminData.ContentWatchLoaded = false;
                    //
                    if (string.Compare(core.siteProperties.dataBuildVersion, cp.Version) < 0) {
                        LogController.handleWarn(core, new ApplicationException("Application code version (" + cp.Version + ") is newer than Db version (" + core.siteProperties.dataBuildVersion + "). Upgrade site code."));
                    }
                    //
                    //// Get Requests, initialize adminContext.content and editRecord objects 
                    //contextConstructor(ref adminContext, ref adminContext.content, ref editRecord);
                    //
                    // Process SourceForm/Button into Action/Form, and process
                    if (adminData.requestButton == ButtonCancelAll) {
                        adminData.AdminForm = AdminFormRoot;
                    } else {
                        ProcessForms(adminData);
                        ProcessActions(adminData, core.siteProperties.useContentWatchLink);
                    }
                    //
                    // Normalize values to be needed
                    if (adminData.editRecord.id != 0) {
                        core.workflow.ClearEditLock(adminData.adminContent.name, adminData.editRecord.id);
                    }
                    if (adminData.AdminForm < 1) {
                        //
                        // No form was set, use default form
                        if (adminData.adminContent.id <= 0) {
                            adminData.AdminForm = AdminFormRoot;
                        } else {
                            adminData.AdminForm = AdminFormIndex;
                        }
                    }
                    int addonId = core.docProperties.getInteger("addonid");
                    string AddonGuid = core.docProperties.getText("addonguid");
                    if (adminData.AdminForm == AdminFormLegacyAddonManager) {
                        //
                        // patch out any old links to the legacy addon manager
                        adminData.AdminForm = 0;
                        AddonGuid = addonGuidAddonManager;
                    }
                    //
                    //-------------------------------------------------------------------------------
                    // Edit form but not valid record case
                    // Put this here so we can display the error without being stuck displaying the edit form
                    // Putting the error on the edit form is confusing because there are fields to fill in
                    //-------------------------------------------------------------------------------
                    //
                    if (adminData.AdminSourceForm == AdminFormEdit) {
                        if ( string.IsNullOrEmpty(core.doc.debug_iUserError) && (adminData.requestButton.Equals(ButtonOK) || adminData.requestButton.Equals(ButtonCancel) || adminData.requestButton.Equals(ButtonDelete))) {
                            string EditReferer = core.docProperties.getText("EditReferer");
                            string CurrentLink = GenericController.modifyLinkQuery(core.webServer.requestUrl, "editreferer", "", false);
                            CurrentLink = GenericController.vbLCase(CurrentLink);
                            //
                            // check if this editreferer includes cid=thisone and id=thisone -- if so, go to index form for this cid
                            //
                            if ((!string.IsNullOrEmpty(EditReferer)) && (EditReferer.ToLowerInvariant() != CurrentLink)) {
                                //
                                // return to the page it came from
                                //
                                return core.webServer.redirect(EditReferer, "Admin Edit page returning to the EditReferer setting");
                            } else {
                                //
                                // return to the index page for this content
                                //
                                adminData.AdminForm = AdminFormIndex;
                            }
                        }
                        if (adminData.BlockEditForm) {
                            adminData.AdminForm = AdminFormIndex;
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
                    if (adminData.adminContent.id != 0) {
                        core.doc.addRefreshQueryString("cid", GenericController.encodeText(adminData.adminContent.id));
                    }
                    if (adminData.editRecord.id != 0) {
                        core.doc.addRefreshQueryString("id", GenericController.encodeText(adminData.editRecord.id));
                    }
                    if (adminData.TitleExtension != "") {
                        core.doc.addRefreshQueryString(RequestNameTitleExtension, GenericController.encodeRequestVariable(adminData.TitleExtension));
                    }
                    if (adminData.RecordTop != 0) {
                        core.doc.addRefreshQueryString("rt", GenericController.encodeText(adminData.RecordTop));
                    }
                    if (adminData.RecordsPerPage != Constants.RecordsPerPageDefault) {
                        core.doc.addRefreshQueryString("rs", GenericController.encodeText(adminData.RecordsPerPage));
                    }
                    if (adminData.AdminForm != 0) {
                        core.doc.addRefreshQueryString(rnAdminForm, GenericController.encodeText(adminData.AdminForm));
                    }
                    if (adminData.ignore_legacyMenuDepth != 0) {
                        core.doc.addRefreshQueryString(RequestNameAdminDepth, GenericController.encodeText(adminData.ignore_legacyMenuDepth));
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
                    } else if (adminData.AdminForm != 0) {
                        //
                        // No content so far, try the forms
                        // todo - convert this to switch
                        if (adminData.AdminForm == AdminFormBuilderCollection) {
                            adminBody = GetForm_BuildCollection();
                        } else if (adminData.AdminForm == AdminFormSecurityControl) {
                            AddonGuid = Constants.AddonGuidPreferences;
                        } else if (adminData.AdminForm == AdminFormMetaKeywordTool) {
                            adminBody = (new Contensive.Addons.Tools.MetakeywordToolClass()).Execute( cp ) as string;
                        } else if ((adminData.AdminForm == AdminFormMobileBrowserControl) || (adminData.AdminForm == AdminFormPageControl) || (adminData.AdminForm == AdminFormEmailControl)) {
                            adminBody = core.addon.execute(Constants.AddonGuidPreferences, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorContextMessage = "get Preferences for Admin"
                            });
                        } else if (adminData.AdminForm == AdminFormClearCache) {
                            adminBody = ToolClearCache.GetForm_ClearCache(core);
                        } else if (adminData.AdminForm == AdminFormSpiderControl) {
                            adminBody = core.addon.execute(AddonModel.createByUniqueName(core, "Content Spider Control"), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorContextMessage = "get Content Spider Control for Admin"
                            });
                        } else if (adminData.AdminForm == AdminFormResourceLibrary) {
                            adminBody = core.html.getResourceLibrary("", false, "", "", true);
                        } else if (adminData.AdminForm == AdminFormQuickStats) {
                            adminBody = (FormQuickStats.GetForm_QuickStats(core));
                        } else if (adminData.AdminForm == AdminFormIndex) {
                            adminBody = FormIndex.get( core, adminData, (adminData.adminContent.tableName.ToLowerInvariant() == "ccemail"));
                        } else if (adminData.AdminForm == AdminFormEdit) {
                            adminBody = FormEdit.get(core, adminData);
                        } else if (adminData.AdminForm == AdminFormClose) {
                            Stream.Add("<Script Language=\"JavaScript\" type=\"text/javascript\"> window.close(); </Script>");
                        } else if (adminData.AdminForm == AdminFormContentChildTool) {
                            adminBody = (GetContentChildTool());
                        } else if (adminData.AdminForm == AdminformHousekeepingControl) {
                            adminBody = (GetForm_HouseKeepingControl());
                        } else if ((adminData.AdminForm == AdminFormTools) || (adminData.AdminForm >= 100 && adminData.AdminForm <= 199)) {
                            legacyToolsClass Tools = new legacyToolsClass(core);
                            adminBody = Tools.getToolsList();
                        } else if (adminData.AdminForm == AdminFormDownloads) {
                            adminBody = (ToolDownloads.GetForm_Downloads(core));
                        } else if (adminData.AdminForm == AdminformRSSControl) {
                            adminBody = core.webServer.redirect("?cid=" + CdefController.getContentId(core, "RSS Feeds"), "RSS Control page is not longer supported. RSS Feeds are controlled from the RSS feed records.");
                        } else if (adminData.AdminForm == AdminFormImportWizard) {
                            adminBody = core.addon.execute(addonGuidImportWizard, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorContextMessage = "get Import Wizard for Admin"
                            });
                        } else if (adminData.AdminForm == AdminFormCustomReports) {
                            adminBody = ToolCustomReports.GetForm_CustomReports(core);
                        } else if (adminData.AdminForm == AdminFormFormWizard) {
                            adminBody = core.addon.execute(addonGuidFormWizard, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorContextMessage = "get Form Wizard for Admin"
                            });
                        } else if (adminData.AdminForm == AdminFormLegacyAddonManager) {
                            adminBody = AddonController.getAddonManager(core);
                        } else if (adminData.AdminForm == AdminFormEditorConfig) {
                            adminBody = FormEditConfig.getForm_EditConfig(core);
                        } else {
                            adminBody = "<p>The form requested is not supported</p>";
                        }
                    } else if ((addonId != 0) || (!string.IsNullOrEmpty(AddonGuid)) || (!string.IsNullOrEmpty(AddonName))) {
                        //
                        // execute an addon
                        //
                        if ((AddonGuid == addonGuidAddonManager) || (AddonName.ToLowerInvariant() == "add-on manager") || (AddonName.ToLowerInvariant() == "addon manager")) {
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
                                adminBody = FormRoot.GetForm_Root(core);
                            }

                        }
                    } else {
                        //
                        // nothing so far, display desktop
                        adminBody = FormRoot.GetForm_Root(core);
                    }
                    //
                    // include fancybox if it was needed
                    if (adminData.includeFancyBox) {
                        core.addon.executeDependency(AddonModel.create(core, addonGuidjQueryFancyBox), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                            addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                            errorContextMessage = "adding fancybox dependency in Admin"
                        });
                        core.html.addScriptCode_onLoad(adminData.fancyBoxHeadJS, "");
                    }
                    //
                    // add user errors
                    if (!string.IsNullOrEmpty(core.doc.debug_iUserError)) {
                        adminBody = HtmlController.div(Processor.Controllers.ErrorController.getUserError(core), "ccAdminMsg") + adminBody;
                    }
                    Stream.Add(getAdminHeader(adminData));
                    Stream.Add(adminBody);
                    Stream.Add(adminData.adminFooter);
                    adminData.JavaScriptString += "ButtonObjectCount = " + adminData.ButtonObjectCount + ";";
                    core.html.addScriptCode(adminData.JavaScriptString, "Admin Site");
                    result = Stream.Text;
                }
                if (core.session.user.Developer) {
                    result = Processor.Controllers.ErrorController.getDocExceptionHtmlList(core) + result;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
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
                //string SQL = null;
                string IncludeHelp = "";
                //int IncludeID = 0;
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
                        foreach (var addonon in core.addonCache.getDependsOnList( HelpAddonID )) {
                            IncludeHelp += GetAddonHelp(addonon.id, HelpAddonID + "," + addonon.id.ToString());
                        }
                        //SQL = "select IncludedAddonID from ccAddonIncludeRules where AddonID=" + HelpAddonID;
                        //CS = core.db.csOpenSql(SQL, "Default");
                        //while (core.db.csOk(CS)) {
                        //    IncludeID = core.db.csGetInteger(CS, "IncludedAddonID");
                        //    IncludeHelp = IncludeHelp + GetAddonHelp(IncludeID, HelpAddonID + "," + IncludeID.ToString());
                        //    core.db.csGoNext(CS);
                        //}
                        //core.db.csClose(ref CS);
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
        //====================================================================================================
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
        /// <summary>
        /// If this field has no help message, check the field with the same name from it's inherited parent
        /// </summary>
        /// <param name="ContentID"></param>
        /// <param name="FieldName"></param>
        /// <param name="return_Default"></param>
        /// <param name="return_Custom"></param>
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
        private void ProcessActions(AdminDataModel adminData, bool UseContentWatchLink) {
            try {
                int CS = 0;
                int RecordID = 0;
                string ContentName = null;
                int CSEditRecord = 0;
                int EmailToConfirmationMemberID = 0;
                int RowCnt = 0;
                int RowPtr = 0;
                //
                if (adminData.Admin_Action != Constants.AdminActionNop) {
                    if (!adminData.UserAllowContentEdit) {
                        //
                        // Action blocked by BlockCurrentRecord
                        //
                    } else {
                        //
                        // Process actions
                        //
                        switch (adminData.Admin_Action) {
                            case Constants.AdminActionEditRefresh:
                                //
                                // Load the record as if it will be saved, but skip the save
                                //
                                adminData.LoadEditRecord(core);
                                adminData.LoadEditRecord_Request(core);
                                break;
                            case Constants.AdminActionMarkReviewed:
                                //
                                // Mark the record reviewed without making any changes
                                //
                                core.doc.markRecordReviewed(adminData.adminContent.name, adminData.editRecord.id);
                                break;
                            case Constants.AdminActionDelete:
                                if (adminData.editRecord.Read_Only) {
                                    Processor.Controllers.ErrorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    adminData.LoadEditRecord(core);
                                    core.db.deleteTableRecord(adminData.editRecord.id,adminData.adminContent.tableName,  adminData.adminContent.dataSourceName);
                                    core.doc.processAfterSave(true, adminData.editRecord.contentControlId_Name, adminData.editRecord.id, adminData.editRecord.nameLc, adminData.editRecord.parentID, UseContentWatchLink);
                                }
                                adminData.Admin_Action = Constants.AdminActionNop;
                                break;
                            case Constants.AdminActionSave:
                                //
                                // ----- Save Record
                                //
                                if (adminData.editRecord.Read_Only) {
                                    Processor.Controllers.ErrorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    adminData.LoadEditRecord(core);
                                    adminData.LoadEditRecord_Request(core);
                                    ProcessActionSave(adminData, UseContentWatchLink);
                                    core.doc.processAfterSave(false, adminData.adminContent.name, adminData.editRecord.id, adminData.editRecord.nameLc, adminData.editRecord.parentID, UseContentWatchLink);
                                }
                                adminData.Admin_Action = Constants.AdminActionNop; // convert so action can be used in as a refresh
                                                                                              //
                                break;
                            case Constants.AdminActionSaveAddNew:
                                //
                                // ----- Save and add a new record
                                //
                                if (adminData.editRecord.Read_Only) {
                                    Processor.Controllers.ErrorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    adminData.LoadEditRecord(core);
                                    adminData.LoadEditRecord_Request(core);
                                    ProcessActionSave(adminData, UseContentWatchLink);
                                    core.doc.processAfterSave(false, adminData.adminContent.name, adminData.editRecord.id, adminData.editRecord.nameLc, adminData.editRecord.parentID, UseContentWatchLink);
                                    adminData.editRecord.id = 0;
                                    adminData.editRecord.Loaded = false;
                                    //If adminContext.content.fields.Count > 0 Then
                                    //    ReDim EditRecordValuesObject(adminContext.content.fields.Count)
                                    //    ReDim EditRecordDbValues(adminContext.content.fields.Count)
                                    //End If
                                }
                                adminData.Admin_Action = Constants.AdminActionNop; // convert so action can be used in as a refresh
                                                                                              //
                                break;
                            case Constants.AdminActionDuplicate:
                                //
                                // ----- Save Record
                                //
                                ProcessActionDuplicate(adminData);
                                adminData.Admin_Action = Constants.AdminActionNop;
                                break;
                            case Constants.AdminActionSendEmail:
                                //
                                // ----- Send (Group Email Only)
                                //
                                if (adminData.editRecord.Read_Only) {
                                    Processor.Controllers.ErrorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    adminData.LoadEditRecord(core);
                                    adminData.LoadEditRecord_Request(core);
                                    ProcessActionSave(adminData, UseContentWatchLink);
                                    core.doc.processAfterSave(false, adminData.adminContent.name, adminData.editRecord.id, adminData.editRecord.nameLc, adminData.editRecord.parentID, UseContentWatchLink);
                                    if (!(core.doc.debug_iUserError != "")) {
                                        if (!CdefController.isWithinContent(core, adminData.editRecord.contentControlId, CdefController.getContentId(core, "Group Email"))) {
                                            Processor.Controllers.ErrorController.addUserError(core, "The send action only supports Group Email.");
                                        } else {
                                            CS = core.db.csOpenRecord("Group Email", adminData.editRecord.id);
                                            if (!core.db.csOk(CS)) {
                                                //throw new ApplicationException("Unexpected exception"); // //throw new ApplicationException("Unexpected exception")' core.handleLegacyError23("Email ID [" &  adminContext.editRecord.id & "] could not be found in Group Email.")
                                            } else if (core.db.csGet(CS, "FromAddress") == "") {
                                                Processor.Controllers.ErrorController.addUserError(core, "A 'From Address' is required before sending an email.");
                                            } else if (core.db.csGet(CS, "Subject") == "") {
                                                Processor.Controllers.ErrorController.addUserError(core, "A 'Subject' is required before sending an email.");
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
                                adminData.Admin_Action = Constants.AdminActionNop; // convert so action can be used in as a refresh
                                                                                              //
                                break;
                            case Constants.AdminActionDeactivateEmail:
                                //
                                // ----- Deactivate (Conditional Email Only)
                                //
                                if (adminData.editRecord.Read_Only) {
                                    Processor.Controllers.ErrorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    // no save, page was read only - Call ProcessActionSave
                                    adminData.LoadEditRecord(core);
                                    if (!(core.doc.debug_iUserError != "")) {
                                        if (!CdefController.isWithinContent(core, adminData.editRecord.contentControlId, CdefController.getContentId(core, "Conditional Email"))) {
                                            Processor.Controllers.ErrorController.addUserError(core, "The deactivate action only supports Conditional Email.");
                                        } else {
                                            CS = core.db.csOpenRecord("Conditional Email", adminData.editRecord.id);
                                            if (!core.db.csOk(CS)) {
                                                //throw new ApplicationException("Unexpected exception"); // //throw new ApplicationException("Unexpected exception")' core.handleLegacyError23("Email ID [" & editRecord.id & "] could not be opened.")
                                            } else {
                                                core.db.csSet(CS, "submitted", false);
                                            }
                                            core.db.csClose(ref CS);
                                        }
                                    }
                                }
                                adminData.Admin_Action = Constants.AdminActionNop; // convert so action can be used in as a refresh
                                break;
                            case Constants.AdminActionActivateEmail:
                                //
                                // ----- Activate (Conditional Email Only)
                                //
                                if (adminData.editRecord.Read_Only) {
                                    Processor.Controllers.ErrorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    adminData.LoadEditRecord(core);
                                    adminData.LoadEditRecord_Request(core);
                                    ProcessActionSave(adminData, UseContentWatchLink);
                                    core.doc.processAfterSave(false, adminData.adminContent.name, adminData.editRecord.id, adminData.editRecord.nameLc, adminData.editRecord.parentID, UseContentWatchLink);
                                    if (!(core.doc.debug_iUserError != "")) {
                                        if (!CdefController.isWithinContent(core, adminData.editRecord.contentControlId, CdefController.getContentId(core, "Conditional Email"))) {
                                            Processor.Controllers.ErrorController.addUserError(core, "The activate action only supports Conditional Email.");
                                        } else {
                                            CS = core.db.csOpenRecord("Conditional Email", adminData.editRecord.id);
                                            if (!core.db.csOk(CS)) {
                                                //throw new ApplicationException("Unexpected exception"); // //throw new ApplicationException("Unexpected exception")' core.handleLegacyError23("Email ID [" & editRecord.id & "] could not be opened.")
                                            } else if (core.db.csGetInteger(CS, "ConditionID") == 0) {
                                                Processor.Controllers.ErrorController.addUserError(core, "A condition must be set.");
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
                                adminData.Admin_Action = Constants.AdminActionNop; // convert so action can be used in as a refresh
                                break;
                            case Constants.AdminActionSendEmailTest:
                                if (adminData.editRecord.Read_Only) {
                                    Processor.Controllers.ErrorController.addUserError(core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                } else {
                                    //
                                    adminData.LoadEditRecord(core);
                                    adminData.LoadEditRecord_Request(core);
                                    ProcessActionSave(adminData, UseContentWatchLink);
                                    core.doc.processAfterSave(false, adminData.adminContent.name, adminData.editRecord.id, adminData.editRecord.nameLc, adminData.editRecord.parentID, UseContentWatchLink);
                                    //
                                    if (!(core.doc.debug_iUserError != "")) {
                                        //
                                        EmailToConfirmationMemberID = 0;
                                        if (adminData.editRecord.fieldsLc.ContainsKey("testmemberid")) {
                                            EmailToConfirmationMemberID = GenericController.encodeInteger(adminData.editRecord.fieldsLc["testmemberid"].value);
                                            EmailController.queueConfirmationTestEmail(core, adminData.editRecord.id, EmailToConfirmationMemberID);
                                            //
                                            if (adminData.editRecord.fieldsLc.ContainsKey("lastsendtestdate")) {
                                                adminData.editRecord.fieldsLc["lastsendtestdate"].value = core.doc.profileStartTime;
                                                core.db.executeQuery("update ccemail Set lastsendtestdate=" + core.db.encodeSQLDate(core.doc.profileStartTime) + " where id=" + adminData.editRecord.id);
                                            }
                                        }
                                    }
                                }
                                adminData.Admin_Action = Constants.AdminActionNop; // convert so action can be used in as a refresh
                                                                                              // end case
                                break;
                            case Constants.AdminActionDeleteRows:
                                //
                                // Delete Multiple Rows
                                //
                                RowCnt = core.docProperties.getInteger("rowcnt");
                                if (RowCnt > 0) {
                                    for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                        if (core.docProperties.getBoolean("row" + RowPtr)) {
                                            CSEditRecord = core.db.csOpen2(adminData.adminContent.name, core.docProperties.getInteger("rowid" + RowPtr), true, true);
                                            if (core.db.csOk(CSEditRecord)) {
                                                RecordID = core.db.csGetInteger(CSEditRecord, "ID");
                                                core.db.csDeleteRecord(CSEditRecord);
                                                if (!false) {
                                                    //
                                                    // non-Workflow Delete
                                                    //
                                                    ContentName = CdefController.getContentNameByID(core, core.db.csGetInteger(CSEditRecord, "ContentControlID"));
                                                    core.cache.invalidateDbRecord(RecordID, adminData.adminContent.tableName);
                                                    core.doc.processAfterSave(true, ContentName, RecordID, "", 0, UseContentWatchLink);
                                                }
                                                //
                                                // Page Content special cases
                                                //
                                                if (GenericController.vbLCase(adminData.adminContent.tableName) == "ccpagecontent") {
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
                            case Constants.AdminActionReloadCDef:
                                //
                                // ccContent - save changes and reload content definitions
                                //
                                if (adminData.editRecord.Read_Only) {
                                    Processor.Controllers.ErrorController.addUserError(core, "Your request was blocked because the record you specified Is now locked by another authcontext.user.");
                                } else {
                                    adminData.LoadEditRecord(core);
                                    adminData.LoadEditRecord_Request(core);
                                    ProcessActionSave(adminData, UseContentWatchLink);
                                    core.cache.invalidateAll();
                                    core.doc.clearMetaData();
                                }
                                adminData.Admin_Action = Constants.AdminActionNop; // convert so action can be used in as a refresh
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
                Processor.Controllers.ErrorController.addUserError(core, "There was an unknown error processing this page at " + core.doc.profileStartTime + ". Please try again, Or report this error To the site administrator.");
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
        //========================================================================
        //   Save Whats New values if present
        //
        //   does NOT check AuthoringLocked -- you must check before calling
        //========================================================================
        //
        private void SaveContentTracking(AdminDataModel adminData) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                int ContentID = 0;
                int CSPointer = 0;
                int CSRules = 0;
                int CSContentWatch = 0;
                int ContentWatchID = 0;
                //
                if (adminData.adminContent.allowContentTracking & (!editRecord.Read_Only)) {
                    //
                    // ----- Set default content watch link label
                    //
                    if ((adminData.ContentWatchListIDCount > 0) && (adminData.ContentWatchLinkLabel == "")) {
                        if (editRecord.menuHeadline != "") {
                            adminData.ContentWatchLinkLabel = editRecord.menuHeadline;
                        } else if (editRecord.nameLc != "") {
                            adminData.ContentWatchLinkLabel = editRecord.nameLc;
                        } else {
                            adminData.ContentWatchLinkLabel = "Click Here";
                        }
                    }
                    // ----- update/create the content watch record for this content record
                    //
                    ContentID = (editRecord.contentControlId.Equals(0)) ? adminData.adminContent.id : editRecord.contentControlId;
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
                        LogController.handleError(core, new ApplicationException("SaveContentTracking, can Not create New record"));
                    } else {
                        ContentWatchID = core.db.csGetInteger(CSContentWatch, "ID");
                        core.db.csSet(CSContentWatch, "LinkLabel", adminData.ContentWatchLinkLabel);
                        core.db.csSet(CSContentWatch, "WhatsNewDateExpires", adminData.ContentWatchExpires);
                        core.db.csSet(CSContentWatch, "Link", adminData.ContentWatchLink);
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
                        if (adminData.ContentWatchListIDCount > 0) {
                            for (ListPointer = 0; ListPointer < adminData.ContentWatchListIDCount; ListPointer++) {
                                CSRules = core.db.csInsertRecord("Content Watch List Rules");
                                if (core.db.csOk(CSRules)) {
                                    core.db.csSet(CSRules, "ContentWatchID", ContentWatchID);
                                    core.db.csSet(CSRules, "ContentWatchListID", adminData.ContentWatchListID[ListPointer]);
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
        //   Save Link Alias field if it supported, and is non-authoring
        //   if it is authoring, it will be saved by the userfield routines
        //   if not, it appears in the LinkAlias tab, and must be saved here
        //========================================================================
        //
        private void SaveLinkAlias(AdminDataModel adminData) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
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
                            core.db.executeQuery("update " + adminData.adminContent.tableName + " set linkalias=null where ( linkalias=" + core.db.encodeSQLText(linkAlias) + ") and (id<>" + editRecord.id + ")");
                        } else {
                            int CS = core.db.csOpen(adminData.adminContent.name, "( linkalias=" + core.db.encodeSQLText(linkAlias) + ")and(id<>" + editRecord.id + ")");
                            if (core.db.csOk(CS)) {
                                isDupError = true;
                                Processor.Controllers.ErrorController.addUserError(core, "The Link Alias you entered can not be used because another record uses this value [" + linkAlias + "]. Enter a different Link Alias, or check the Override Duplicates checkbox in the Link Alias tab.");
                            }
                            core.db.csClose(ref CS);
                        }
                        if (!isDupError) {
                            DupCausesWarning = true;
                            int CS = core.db.csOpen2(adminData.adminContent.name, editRecord.id, true, true);
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
        //
        private void SaveEditRecord(AdminDataModel adminData) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                int SaveCCIDValue = 0;
                int ActivityLogOrganizationID = -1;
                if (core.doc.debug_iUserError != "") {
                    //
                    // -- If There is an error, block the save
                    adminData.Admin_Action = Constants.AdminActionNop;
                } else if (!core.session.isAuthenticatedContentManager(core, adminData.adminContent.name)) {
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
                        csEditRecord = core.db.csInsertRecord(adminData.adminContent.name);
                    } else {
                        NewRecord = false;
                        csEditRecord = core.db.csOpen2(adminData.adminContent.name, editRecord.id, true, true);
                    }
                    if (!core.db.csOk(csEditRecord)) {
                        //
                        // ----- Error: new record could not be created
                        //
                        if (NewRecord) {
                            //
                            // Could not insert record
                            //
                            LogController.handleError(core, new ApplicationException("A new record could not be inserted for content [" + adminData.adminContent.name + "]. Verify the Database table and field DateAdded, CreateKey, and ID."));
                        } else {
                            //
                            // Could not locate record you requested
                            //
                            LogController.handleError(core, new ApplicationException("The record you requested (ID=" + editRecord.id + ") could not be found for content [" + adminData.adminContent.name + "]"));
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
                        foreach (var keyValuePair in adminData.adminContent.fields) {
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
                                                if (adminData.ContentWatchExpires <= DateTime.MinValue) {
                                                    adminData.ContentWatchExpires = saveValue;
                                                } else if (adminData.ContentWatchExpires > saveValue) {
                                                    adminData.ContentWatchExpires = saveValue;
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
                                                if ((adminData.ContentWatchExpires) <= DateTime.MinValue) {
                                                    adminData.ContentWatchExpires = saveValue;
                                                } else if (adminData.ContentWatchExpires > saveValue) {
                                                    adminData.ContentWatchExpires = saveValue;
                                                }
                                            }
                                        }
                                        break;
                                    }
                            }
                            //
                            // ----- Put the field in the SQL to be saved
                            //
                            if (AdminDataModel.IsVisibleUserField(core, field.adminOnly, field.developerOnly, field.active, field.authorable, field.nameLc, adminData.adminContent.tableName) && (NewRecord || (!field.readOnly)) && (NewRecord || (!field.notEditable))) {
                                //
                                // ----- save the value by field type
                                //
                                switch (field.fieldTypeId) {
                                    case _fieldTypeIdAutoIdIncrement:
                                    case _fieldTypeIdRedirect: {
                                            //
                                            // do nothing with these
                                            //
                                            break;
                                        }
                                    case _fieldTypeIdFile:
                                    case _fieldTypeIdFileImage: {
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
                                                string unixPathFilename = core.db.csGetFieldFilename(csEditRecord, fieldName, filename, adminData.adminContent.name);
                                                string dosPathFilename = GenericController.convertToDosSlash(unixPathFilename);
                                                string dosPath = GenericController.getPath(dosPathFilename);
                                                core.cdnFiles.upload(fieldName, dosPath, ref filename);
                                                core.db.csSet(csEditRecord, fieldName, unixPathFilename);
                                                recordChanged = true;
                                                fieldChanged = true;
                                            }
                                            break;
                                        }
                                    case _fieldTypeIdBoolean: {
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
                                    case _fieldTypeIdCurrency:
                                    case _fieldTypeIdFloat: {
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
                                    case _fieldTypeIdDate: {
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
                                    case _fieldTypeIdInteger:
                                    case _fieldTypeIdLookup: {
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
                                    case _fieldTypeIdLongText:
                                    case _fieldTypeIdText:
                                    case _fieldTypeIdFileText:
                                    case _fieldTypeIdFileCSS:
                                    case _fieldTypeIdFileXML:
                                    case _fieldTypeIdFileJavascript:
                                    case _fieldTypeIdHTML:
                                    case _fieldTypeIdFileHTML: {
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
                                    case _fieldTypeIdManyToMany: {
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
                                switch (GenericController.vbLCase(adminData.adminContent.tableName)) {
                                    case "cclibraryfiles":
                                        //
                                        if (core.docProperties.getText("filename") != "") {
                                            core.db.csSet(csEditRecord, "altsizelist", "");
                                        }
                                        break;
                                }
                                if (!NewRecord) {
                                    switch (GenericController.vbLCase(adminData.adminContent.tableName)) {
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
                                tableName = CdefController.getContentTablename(core, adminData.adminContent.name);
                            } else {
                                tableName = CdefController.getContentTablename(core, editRecord.contentControlId_Name);
                            }
                            //todo  NOTE: The following VB 'Select Case' included either a non-ordinal switch expression or non-ordinal, range-type, or non-constant 'Case' expressions and was converted to C# 'if-else' logic:
                            //							Select Case tableName.ToLowerInvariant()
                            var tempVar = tableName.ToLowerInvariant();
                            //ORIGINAL LINE: Case linkAliasModel.contentTableName.ToLowerInvariant()
                            if (tempVar == LinkAliasModel.contentTableName.ToLowerInvariant()) {
                                //
                                LinkAliasModel.invalidateRecordCache(core, editRecord.id);
                                //Models.Complex.routeDictionaryModel.invalidateCache(core)
                            }
                            //ORIGINAL LINE: Case addonModel.contentTableName.ToLowerInvariant()
                            else if (tempVar == AddonModel.contentTableName.ToLowerInvariant()) {
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
                        core.workflow.ClearEditLock(adminData.adminContent.name, editRecord.id);
                        //
                        // ----- if admin content is changed, reload the adminContext.content data in case this is a save, and not an OK
                        //
                        if (recordChanged && SaveCCIDValue != 0) {
                            CdefController.setContentControlId(core, (editRecord.contentControlId.Equals(0)) ? adminData.adminContent.id : editRecord.contentControlId, editRecord.id, SaveCCIDValue);
                            editRecord.contentControlId_Name = CdefController.getContentNameByID(core, SaveCCIDValue);
                            adminData.adminContent = CDefModel.create(core, editRecord.contentControlId_Name);
                            adminData.adminContent.id = adminData.adminContent.id;
                            adminData.adminContent.name = adminData.adminContent.name;
                        }
                    }
                    editRecord.Saved = true;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        ////
        //========================================================================
        // GetForm_Top
        //   Prints the admin page before the content form window.
        //   After this, print the content window, then PrintFormBottom()
        //========================================================================
        //
        private string getAdminHeader(AdminDataModel adminData, string BackgroundColor = "") {
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
                if ((adminData.ignore_legacyMenuDepth == 0) && (adminData.AdminMenuModeID == AdminMenuModeTop)) {
                    Stream.Add(GetMenuTopMode(adminData));
                }
                //
                // --- Rule to separate content
                //
                //Stream.Add("\r<div style=\"border-top:1px solid white;border-bottom:1px solid black;height:2px\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=1 height=1></div>");
                //
                // --- Content Definition
                adminData.adminFooter = "";
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
                adminData.adminFooter = adminData.adminFooter + "</td></tr></table>";
                //
                result = Stream.Text;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
            return tempGetMenuLink;
        }
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adminData.content"></param>
        /// <param name="editRecord"></param>
        //
        private void ProcessForms(AdminDataModel adminData) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                //
                if (adminData.AdminSourceForm != 0) {
                    int CS = 0;
                    string EditorStyleRulesFilename = null;
                    switch (adminData.AdminSourceForm) {
                        case AdminFormReports:
                            //
                            // Reports form cancel button
                            //
                            switch (adminData.requestButton) {
                                case ButtonCancel:
                                    adminData.Admin_Action = Constants.AdminActionNop;
                                    adminData.AdminForm = AdminFormRoot;
                                    break;
                            }
                            break;
                        case AdminFormQuickStats:
                            switch (adminData.requestButton) {
                                case ButtonCancel:
                                    adminData.Admin_Action = Constants.AdminActionNop;
                                    adminData.AdminForm = AdminFormRoot;
                                    break;
                            }
                            break;
                        case AdminFormPublishing:
                            //
                            // Publish Form
                            //
                            switch (adminData.requestButton) {
                                case ButtonCancel:
                                    adminData.Admin_Action = Constants.AdminActionNop;
                                    adminData.AdminForm = AdminFormRoot;
                                    break;
                            }
                            break;
                        case AdminFormIndex:
                            switch (adminData.requestButton) {
                                case ButtonCancel:
                                    adminData.Admin_Action = Constants.AdminActionNop;
                                    adminData.AdminForm = AdminFormRoot;
                                    adminData.adminContent = new CDefModel();
                                    break;
                                case ButtonClose:
                                    adminData.Admin_Action = Constants.AdminActionNop;
                                    adminData.AdminForm = AdminFormRoot;
                                    adminData.adminContent = new CDefModel();
                                    break;
                                case ButtonAdd:
                                    adminData.Admin_Action = Constants.AdminActionNop;
                                    adminData.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonFind:
                                    adminData.Admin_Action = Constants.AdminActionFind;
                                    adminData.AdminForm = adminData.AdminSourceForm;
                                    break;
                                case ButtonFirst:
                                    adminData.RecordTop = 0;
                                    adminData.AdminForm = adminData.AdminSourceForm;
                                    break;
                                case ButtonPrevious:
                                    adminData.RecordTop = adminData.RecordTop - adminData.RecordsPerPage;
                                    if (adminData.RecordTop < 0) {
                                        adminData.RecordTop = 0;
                                    }
                                    adminData.Admin_Action = Constants.AdminActionNop;
                                    adminData.AdminForm = adminData.AdminSourceForm;
                                    break;
                                case ButtonNext:
                                    adminData.Admin_Action = Constants.AdminActionNext;
                                    adminData.AdminForm = adminData.AdminSourceForm;
                                    break;
                                case ButtonDelete:
                                    adminData.Admin_Action = Constants.AdminActionDeleteRows;
                                    adminData.AdminForm = adminData.AdminSourceForm;
                                    break;
                            }
                            // end case
                            break;
                        case AdminFormEdit:
                            //
                            // Edit Form
                            //
                            switch (adminData.requestButton) {
                                case ButtonRefresh:
                                    //
                                    // this is a test operation. need this so the user can set editor preferences without saving the record
                                    //   during refresh, the edit page is redrawn just was it was, but no save
                                    //
                                    adminData.Admin_Action = Constants.AdminActionEditRefresh;
                                    adminData.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonMarkReviewed:
                                    adminData.Admin_Action = Constants.AdminActionMarkReviewed;
                                    adminData.AdminForm = GetForm_Close(adminData.ignore_legacyMenuDepth, adminData.adminContent.name, editRecord.id);
                                    break;
                                case ButtonSaveandInvalidateCache:
                                    adminData.Admin_Action = Constants.AdminActionReloadCDef;
                                    adminData.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonDelete:
                                    adminData.Admin_Action = Constants.AdminActionDelete;
                                    adminData.AdminForm = GetForm_Close(adminData.ignore_legacyMenuDepth, adminData.adminContent.name, editRecord.id);
                                    break;
                                case ButtonSave:
                                    adminData.Admin_Action = Constants.AdminActionSave;
                                    adminData.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonSaveAddNew:
                                    adminData.Admin_Action = Constants.AdminActionSaveAddNew;
                                    adminData.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonOK:
                                    adminData.Admin_Action = Constants.AdminActionSave;
                                    adminData.AdminForm = GetForm_Close(adminData.ignore_legacyMenuDepth, adminData.adminContent.name, editRecord.id);
                                    break;
                                case ButtonCancel:
                                    adminData.Admin_Action = Constants.AdminActionNop;
                                    adminData.AdminForm = GetForm_Close(adminData.ignore_legacyMenuDepth, adminData.adminContent.name, editRecord.id);
                                    break;
                                case ButtonSend:
                                    //
                                    // Send a Group Email
                                    //
                                    adminData.Admin_Action = Constants.AdminActionSendEmail;
                                    adminData.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonActivate:
                                    //
                                    // Activate (submit) a conditional Email
                                    //
                                    adminData.Admin_Action = Constants.AdminActionActivateEmail;
                                    adminData.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonDeactivate:
                                    //
                                    // Deactivate (clear submit) a conditional Email
                                    //
                                    adminData.Admin_Action = Constants.AdminActionDeactivateEmail;
                                    adminData.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonSendTest:
                                    //
                                    // Test an Email (Group, System, or Conditional)
                                    //
                                    adminData.Admin_Action = Constants.AdminActionSendEmailTest;
                                    adminData.AdminForm = AdminFormEdit;
                                    //                Case ButtonSpellCheck
                                    //                    SpellCheckRequest = True
                                    //                    adminContextClass.AdminAction = adminContextClass.AdminActionSave
                                    //                    adminContext.AdminForm = AdminFormEdit
                                    break;
                                case ButtonCreateDuplicate:
                                    //
                                    // Create a Duplicate record (for email)
                                    //
                                    adminData.Admin_Action = Constants.AdminActionDuplicate;
                                    adminData.AdminForm = AdminFormEdit;
                                    break;
                            }
                            break;
                        case AdminFormStyleEditor:
                            //
                            // Process actions
                            //
                            switch (adminData.requestButton) {
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
                            switch (adminData.requestButton) {
                                case ButtonCancel:
                                case ButtonOK:
                                    adminData.AdminForm = AdminFormRoot;
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
        private void ProcessActionSave(AdminDataModel adminData, bool UseContentWatchLink) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
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
                        if (GenericController.vbUCase(adminData.adminContent.tableName) == GenericController.vbUCase("ccMembers")) {
                            //
                            //
                            //

                            SaveEditRecord(adminData);
                            SaveMemberRules(editRecord.id);
                            //Call SaveTopicRules
                        }
                        //ORIGINAL LINE: Case "CCEMAIL"
                        else if (GenericController.vbUCase(adminData.adminContent.tableName) == "CCEMAIL") {
                            //
                            //
                            //
                            SaveEditRecord(adminData);
                            // NO - ignore wwwroot styles, and create it on the fly during send
                            //If core.main_GetSiteProperty2("BuildVersion") >= "3.3.291" Then
                            //    Call core.app.executeSql( "update ccEmail set InlineStyles=" & encodeSQLText(core.main_GetStyleSheetProcessed) & " where ID=" & EditRecord.ID)
                            //End If
                            core.html.processCheckList("EmailGroups", "Group Email", GenericController.encodeText(editRecord.id), "Groups", "Email Groups", "EmailID", "GroupID");
                            core.html.processCheckList("EmailTopics", "Group Email", GenericController.encodeText(editRecord.id), "Topics", "Email Topics", "EmailID", "TopicID");
                        }
                        //ORIGINAL LINE: Case "CCCONTENT"
                        else if (GenericController.vbUCase(adminData.adminContent.tableName) == "CCCONTENT") {
                            //
                            //
                            //
                            SaveEditRecord(adminData);
                            LoadAndSaveGroupRules(editRecord);
                        }
                        //ORIGINAL LINE: Case "CCPAGECONTENT"
                        else if (GenericController.vbUCase(adminData.adminContent.tableName) == "CCPAGECONTENT") {
                            //
                            //
                            //
                            SaveEditRecord(adminData);
                            adminData.LoadContentTrackingDataBase(core);
                            adminData.LoadContentTrackingResponse(core);
                            //Call LoadAndSaveMetaContent()
                            SaveLinkAlias(adminData);
                            //Call SaveTopicRules
                            SaveContentTracking(adminData);
                        }
                        //ORIGINAL LINE: Case "CCLIBRARYFOLDERS"
                        else if (GenericController.vbUCase(adminData.adminContent.tableName) == "CCLIBRARYFOLDERS") {
                            //
                            //
                            //
                            SaveEditRecord(adminData);
                            adminData.LoadContentTrackingDataBase(core);
                            adminData.LoadContentTrackingResponse(core);
                            //Call LoadAndSaveCalendarEvents
                            //Call LoadAndSaveMetaContent()
                            core.html.processCheckList("LibraryFolderRules", adminData.adminContent.name, GenericController.encodeText(editRecord.id), "Groups", "Library Folder Rules", "FolderID", "GroupID");
                            //call SaveTopicRules
                            SaveContentTracking(adminData);
                        }
                        //ORIGINAL LINE: Case "CCSETUP"
                        else if (GenericController.vbUCase(adminData.adminContent.tableName) == "CCSETUP") {
                            //
                            // Site Properties
                            //
                            SaveEditRecord(adminData);
                            if (editRecord.nameLc.ToLowerInvariant() == "allowlinkalias") {
                                if (core.siteProperties.getBoolean("AllowLinkAlias")) {
                                    TurnOnLinkAlias(UseContentWatchLink);
                                }
                            }
                        }
                        //ORIGINAL LINE: Case genericController.vbUCase("ccGroups")
                        else if (GenericController.vbUCase(adminData.adminContent.tableName) == GenericController.vbUCase("ccGroups")) {
                            //Case "CCGROUPS"
                            //
                            //
                            //
                            SaveEditRecord(adminData);
                            adminData.LoadContentTrackingDataBase(core);
                            adminData.LoadContentTrackingResponse(core);
                            LoadAndSaveContentGroupRules(editRecord.id);
                            //Call LoadAndSaveCalendarEvents
                            //Call LoadAndSaveMetaContent()
                            //call SaveTopicRules
                            SaveContentTracking(adminData);
                            //Dim EditorStyleRulesFilename As String
                        }
                        //ORIGINAL LINE: Case "CCTEMPLATES"
                        else if (GenericController.vbUCase(adminData.adminContent.tableName) == "CCTEMPLATES") {
                            //
                            // save and clear editorstylerules for this template
                            //
                            SaveEditRecord(adminData);
                            adminData.LoadContentTrackingDataBase(core);
                            adminData.LoadContentTrackingResponse(core);
                            //Call LoadAndSaveCalendarEvents
                            //Call LoadAndSaveMetaContent()
                            //call SaveTopicRules
                            SaveContentTracking(adminData);
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
                            SaveEditRecord(adminData);
                            adminData.LoadContentTrackingDataBase(core);
                            adminData.LoadContentTrackingResponse(core);
                            //Call LoadAndSaveCalendarEvents
                            //Call LoadAndSaveMetaContent()
                            //call SaveTopicRules
                            SaveContentTracking(adminData);
                        }
                    }
                }
                //
                // If the content supports datereviewed, mark it
                //
                if (core.doc.debug_iUserError != "") {
                    adminData.AdminForm = adminData.AdminSourceForm;
                }
                adminData.Admin_Action = Constants.AdminActionNop; // convert so action can be used in as a refresh
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //
        //
        //=============================================================================================
        //   Create a duplicate record
        //=============================================================================================
        //
        private void ProcessActionDuplicate(AdminDataModel adminData) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                // converted array to dictionary - Dim FieldPointer As Integer
                //
                if (!(core.doc.debug_iUserError != "")) {
                    switch (GenericController.vbUCase(adminData.adminContent.tableName)) {
                        case "CCEMAIL":
                            //
                            // --- preload array with values that may not come back in response
                            //
                            adminData.LoadEditRecord(core);
                            adminData.LoadEditRecord_Request(core);
                            //
                            if (!(core.doc.debug_iUserError != "")) {
                                //
                                // ----- Convert this to the Duplicate
                                //
                                if (adminData.adminContent.fields.ContainsKey("submitted")) {
                                    editRecord.fieldsLc["submitted"].value = false;
                                }
                                if (adminData.adminContent.fields.ContainsKey("sent")) {
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
                            adminData.LoadEditRecord(core);
                            adminData.LoadEditRecord_Request(core);
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
                                foreach (KeyValuePair<string, Contensive.Processor.Models.Domain.CDefFieldModel> keyValuePair in adminData.adminContent.fields) {
                                    CDefFieldModel field = keyValuePair.Value;
                                    if (GenericController.vbLCase(field.nameLc) == "email") {
                                        if ((adminData.adminContent.tableName.ToLowerInvariant() == "ccmembers") && (GenericController.encodeBoolean(core.siteProperties.getBoolean("allowemaillogin", false)))) {
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
                    adminData.AdminForm = adminData.AdminSourceForm;
                    adminData.Admin_Action = Constants.AdminActionNop; // convert so action can be used in as a refresh
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
        private string GetMenuTopMode(AdminDataModel adminData) {
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
                if (adminData.AdminMenuModeID == AdminMenuModeTop) {
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
                                core.db.deleteTableRecord(MemberRuleID,"ccMemberRules",  "Default");
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
                            Processor.Controllers.ErrorController.addUserError(core, "You must select a parent and provide a child name.");
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
                        string tableBody = "";
                        //
                        FieldValue = "<select size=\"1\" name=\"ParentContentID\" ID=\"\"><option value=\"\">Select One</option>";
                        FieldValue = FieldValue + GetContentChildTool_Options(0, ParentContentID);
                        FieldValue = FieldValue + "</select>";
                        tableBody += AdminUIController.getEditRowLegacy(core, FieldValue, "Parent Content Name", "", false, false, "");
                        //
                        FieldValue = HtmlController.inputText(core, "ChildContentName", ChildContentName, 1, 40);
                        tableBody += AdminUIController.getEditRowLegacy(core, FieldValue, "New Child Content Name", "", false, false, "");
                        //
                        FieldValue = ""
                            + core.html.inputRadio("NewGroup", false.ToString(), NewGroup.ToString()) + core.html.selectFromContent("GroupID", GroupID, "Groups", "", "", "", ref IsEmptyList) + "(Select a current group)"
                            + "<br>" + core.html.inputRadio("NewGroup", true.ToString(), NewGroup.ToString()) + HtmlController.inputText(core, "NewGroupName", NewGroupName) + "(Create a new group)";
                        tableBody += AdminUIController.getEditRowLegacy(core, FieldValue, "Content Manager Group", "", false, false, "");
                        //
                        Content.Add(AdminUIController.editTable(tableBody));
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
                    string tableBody = "";
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
                    tableBody += HtmlController.tableRowStart() + "<td colspan=\"3\" class=\"ccPanel3D ccAdminEditSubHeader\"><b>Status</b>" + tableCellEnd + kmaEndTableRow;
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
                    tableBody += AdminUIController.getEditRowLegacy(core, SpanClassAdminNormal + PagesTotal, "Visits Found", "", false, false, "");
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
                    tableBody += (AdminUIController.getEditRowLegacy(core, SpanClassAdminNormal + Copy + " (" + AgeInDays + " days)", "Oldest Visit", "", false, false, ""));
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
                    tableBody += (AdminUIController.getEditRowLegacy(core, SpanClassAdminNormal + PagesTotal, "Viewings Found", "", false, false, ""));
                    //
                    tableBody += (HtmlController.tableRowStart() + "<td colspan=\"3\" class=\"ccPanel3D ccAdminEditSubHeader\"><b>Options</b>" + tableCellEnd + kmaEndTableRow);
                    //
                    Caption = "Archive Age";
                    Copy = HtmlController.inputText(core, "ArchiveRecordAgeDays", ArchiveRecordAgeDays.ToString(), -1, 20) + "&nbsp;Number of days to keep visit records. 0 disables housekeeping.";
                    tableBody += (AdminUIController.getEditRowLegacy(core, Copy, Caption));
                    //
                    Caption = "Housekeeping Time";
                    Copy = HtmlController.inputText(core, "ArchiveTimeOfDay", ArchiveTimeOfDay, -1, 20) + "&nbsp;The time of day when record deleting should start.";
                    tableBody += (AdminUIController.getEditRowLegacy(core, Copy, Caption));
                    //
                    Caption = "Purge Content Files";
                    Copy = HtmlController.checkbox("ArchiveAllowFileClean", ArchiveAllowFileClean) + "&nbsp;Delete Contensive content files with no associated database record.";
                    tableBody += (AdminUIController.getEditRowLegacy(core, Copy, Caption));
                    //
                    Content.Add(AdminUIController.editTable(tableBody));
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
                    Processor.Controllers.ErrorController.addUserError(core, "Existing pages could not be checked for Link Alias names because there was another error on this page. Correct this error, and turn Link Alias on again to rerun the verification.");
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
                        ErrorList = Processor.Controllers.ErrorController.getUserError(core);
                        ErrorList = GenericController.vbReplace(ErrorList, UserErrorHeadline, "", 1, 99, 1);
                        Processor.Controllers.ErrorController.addUserError(core, "The following errors occurred while verifying Link Alias entries for your existing pages." + ErrorList);
                        //Call core.htmldoc.main_AddUserError(ErrorList)
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
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
                    string tableBody = "";
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
                    tableBody += (AdminUIController.getEditRowLegacy(core, Copy, "Allow Auto Login", "", false, false, ""));
                    //
                    // Buttons
                    //
                    ButtonList = ButtonCancel + "," + ButtonSave + "," + ButtonOK;
                    //
                    // Close Tables
                    //
                    Content.Add(AdminUIController.editTable(tableBody));
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
                if ((!string.IsNullOrEmpty(findWord.Name)) && (findWord.MatchOption != FindWordMatchEnum.MatchIgnore)) {
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
            core.visitProperty.setProperty(AdminDataModel.IndexConfigPrefix + encodeText(IndexConfig.ContentID), FilterText);
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
            core.userProperty.setProperty(AdminDataModel.IndexConfigPrefix + encodeText(IndexConfig.ContentID), FilterText);
            //

        }
    }
}
