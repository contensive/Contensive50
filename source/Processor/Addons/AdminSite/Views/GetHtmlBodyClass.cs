
using System;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using Contensive.Addons.Tools;
using static Contensive.Addons.AdminSite.Controllers.AdminUIController;
using Contensive.Processor.Exceptions;
using Contensive.Addons.AdminSite.Controllers;
using Contensive.BaseClasses;

namespace Contensive.Addons.AdminSite {
    public class GetHtmlBodyClass : Contensive.BaseClasses.AddonBaseClass {
        ////
        //private CPClass cp;
        ////
        //private CoreController core;
        //
        //====================================================================================================
        /// <summary>
        /// addon method, deliver complete Html admin site
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cpBase) {
            string result = "";
            CPClass cp = (CPClass)cpBase;
            try {
                //
                // todo - convert admin addon to use cpbase to help understand cp api requirements
                //
                if (!cp.core.session.isAuthenticated) {
                    //
                    // --- must be authenticated to continue. Force a local login
                    result = cp.core.addon.execute(addonGuidLoginPage, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                        errorContextMessage = "get Login Page for Html Body",
                        addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextPage
                    });
                } else if (!cp.core.session.isAuthenticatedContentManager(cp.core)) {
                    //
                    // --- member must have proper access to continue
                    result = ""
                        + "<p>You are attempting to enter an area which your account does not have access.</p>"
                        + "<ul class=\"ccList\">"
                        + "<li class=\"ccListItem\">To return to the public web site, use your back button, or <a href=\"" + "/" + "\">Click Here</A>."
                        + "<li class=\"ccListItem\">To login under a different account, <a href=\"/" + cp.core.appConfig.adminRoute + "?method=logout\" rel=\"nofollow\">Click Here</A>"
                        + "<li class=\"ccListItem\">To have your account access changed to include this area, please contact the <a href=\"mailto:" + cp.core.siteProperties.getText("EmailAdmin") + "\">system administrator</A>. "
                        + "\r</ul>"
                        + "";
                    result = ""
                        + cp.core.html.getPanelHeader("Unauthorized Access")
                        + cp.core.html.getPanel(result, "ccPanel", "ccPanelHilite", "ccPanelShadow", "400", 15);
                    result = ""
                        + "\r<div style=\"display:table;padding:100px 0 0 0;margin:0 auto;\">"
                        + GenericController.nop(result) + "\r</div>";
                    cp.core.doc.setMetaContent(0, 0);
                    cp.core.html.addTitle("Unauthorized Access", "adminSite");
                    result = HtmlController.div(result, "container-fluid ccBodyAdmin ccCon");
                } else {
                    //
                    // get admin content
                    result = getHtmlBody(cp);
                    result = HtmlController.div(result, "container-fluid ccBodyAdmin ccCon");
                }
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
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
        private string getHtmlBody(CPClass cp, string forceAdminContent = "") {
            string result = "";
            try {
                // todo convert to jquery bind
                cp.core.doc.setMetaContent(0, 0);
                //
                // turn off chrome protection against submitting html content
                cp.core.webServer.addResponseHeader("X-XSS-Protection", "0");
                //
                // check for member login, if logged in and no admin, lock out, Do CheckMember here because we need to know who is there to create proper blocked menu
                if (cp.core.doc.continueProcessing) {
                    var adminData = new AdminDataModel(cp.core);
                    cp.core.db.sqlCommandTimeout = 300;
                    adminData.ButtonObjectCount = 0;
                    adminData.JavaScriptString = "";
                    adminData.ContentWatchLoaded = false;
                    //
                    if (string.Compare(cp.core.siteProperties.dataBuildVersion, cp.Version) < 0) {
                        LogController.handleWarn(cp.core, new GenericException("Application code version (" + cp.Version + ") is newer than Db version (" + cp.core.siteProperties.dataBuildVersion + "). Upgrade site code."));
                    }
                    //
                    //// Get Requests, initialize adminContext.content and editRecord objects 
                    //contextConstructor(ref adminContext, ref adminContext.content, ref editRecord);
                    //
                    // Process SourceForm/Button into Action/Form, and process
                    if (adminData.requestButton == ButtonCancelAll) {
                        adminData.AdminForm = AdminFormRoot;
                    } else {
                        ProcessForms(cp, adminData);
                        ProcessActions(cp, adminData, cp.core.siteProperties.useContentWatchLink);
                    }
                    //
                    // Normalize values to be needed
                    if (adminData.editRecord.id != 0) {
                        var table = TableModel.createByUniqueName(cp.core, adminData.adminContent.tableName);
                        if (table != null) {
                            WorkflowController.clearEditLock(cp.core, table.id, adminData.editRecord.id);
                        }
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
                    int addonId = cp.core.docProperties.getInteger("addonid");
                    string AddonGuid = cp.core.docProperties.getText("addonguid");
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
                        if (string.IsNullOrEmpty(cp.core.doc.debug_iUserError) && (adminData.requestButton.Equals(ButtonOK) || adminData.requestButton.Equals(ButtonCancel) || adminData.requestButton.Equals(ButtonDelete))) {
                            string EditReferer = cp.core.docProperties.getText("EditReferer");
                            string CurrentLink = GenericController.modifyLinkQuery(cp.core.webServer.requestUrl, "editreferer", "", false);
                            CurrentLink = GenericController.vbLCase(CurrentLink);
                            //
                            // check if this editreferer includes cid=thisone and id=thisone -- if so, go to index form for this cid
                            //
                            if ((!string.IsNullOrEmpty(EditReferer)) && (EditReferer.ToLowerInvariant() != CurrentLink)) {
                                //
                                // return to the page it came from
                                //
                                return cp.core.webServer.redirect(EditReferer, "Admin Edit page returning to the EditReferer setting");
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
                    int HelpLevel = cp.core.docProperties.getInteger("helplevel");
                    int HelpAddonID = cp.core.docProperties.getInteger("helpaddonid");
                    int HelpCollectionID = cp.core.docProperties.getInteger("helpcollectionid");
                    if (HelpCollectionID == 0) {
                        HelpCollectionID = cp.core.visitProperty.getInteger("RunOnce HelpCollectionID");
                        if (HelpCollectionID != 0) {
                            cp.core.visitProperty.setProperty("RunOnce HelpCollectionID", "");
                        }
                    }
                    //
                    //-------------------------------------------------------------------------------
                    // build refresh string
                    //-------------------------------------------------------------------------------
                    //
                    if (adminData.adminContent.id != 0) {
                        cp.core.doc.addRefreshQueryString("cid", GenericController.encodeText(adminData.adminContent.id));
                    }
                    if (adminData.editRecord.id != 0) {
                        cp.core.doc.addRefreshQueryString("id", GenericController.encodeText(adminData.editRecord.id));
                    }
                    if (adminData.TitleExtension != "") {
                        cp.core.doc.addRefreshQueryString(RequestNameTitleExtension, GenericController.encodeRequestVariable(adminData.TitleExtension));
                    }
                    if (adminData.RecordTop != 0) {
                        cp.core.doc.addRefreshQueryString("rt", GenericController.encodeText(adminData.RecordTop));
                    }
                    if (adminData.RecordsPerPage != Constants.RecordsPerPageDefault) {
                        cp.core.doc.addRefreshQueryString("rs", GenericController.encodeText(adminData.RecordsPerPage));
                    }
                    if (adminData.AdminForm != 0) {
                        cp.core.doc.addRefreshQueryString(rnAdminForm, GenericController.encodeText(adminData.AdminForm));
                    }
                    if (adminData.ignore_legacyMenuDepth != 0) {
                        cp.core.doc.addRefreshQueryString(RequestNameAdminDepth, GenericController.encodeText(adminData.ignore_legacyMenuDepth));
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
                        cp.core.doc.addRefreshQueryString("helpaddonid", HelpAddonID.ToString());
                        adminBody = GetAddonHelp(cp, HelpAddonID, "");
                    } else if (HelpCollectionID != 0) {
                        //
                        // display Collection Help
                        //
                        cp.core.doc.addRefreshQueryString("helpcollectionid", HelpCollectionID.ToString());
                        adminBody = GetCollectionHelp(cp, HelpCollectionID, "");
                    } else if (adminData.AdminForm != 0) {
                        //
                        // No content so far, try the forms
                        // todo - convert this to switch
                        if (adminData.AdminForm == AdminFormBuilderCollection) {
                            adminBody = GetForm_BuildCollection(cp);
                        } else if (adminData.AdminForm == AdminFormSecurityControl) {
                            AddonGuid = Constants.AddonGuidPreferences;
                        } else if (adminData.AdminForm == AdminFormMetaKeywordTool) {
                            adminBody = (new Contensive.Addons.Tools.MetakeywordToolClass()).Execute(cp) as string;
                        } else if ((adminData.AdminForm == AdminFormMobileBrowserControl) || (adminData.AdminForm == AdminFormPageControl) || (adminData.AdminForm == AdminFormEmailControl)) {
                            adminBody = cp.core.addon.execute(Constants.AddonGuidPreferences, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorContextMessage = "get Preferences for Admin"
                            });
                        } else if (adminData.AdminForm == AdminFormClearCache) {
                            adminBody = ToolClearCache.GetForm_ClearCache(cp.core);
                        } else if (adminData.AdminForm == AdminFormSpiderControl) {
                            adminBody = cp.core.addon.execute(AddonModel.createByUniqueName(cp.core, "Content Spider Control"), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorContextMessage = "get Content Spider Control for Admin"
                            });
                        } else if (adminData.AdminForm == AdminFormResourceLibrary) {
                            adminBody = cp.core.html.getResourceLibrary("", false, "", "", true);
                        } else if (adminData.AdminForm == AdminFormQuickStats) {
                            adminBody = (FormQuickStats.GetForm_QuickStats(cp.core));
                        } else if (adminData.AdminForm == AdminFormIndex) {
                            adminBody = FormIndex.get(cp, cp.core, adminData, (adminData.adminContent.tableName.ToLowerInvariant() == "ccemail"));
                        } else if (adminData.AdminForm == AdminFormEdit) {
                            adminBody = FormEdit.get(cp.core, adminData);
                        } else if (adminData.AdminForm == AdminFormClose) {
                            Stream.Add("<Script Language=\"JavaScript\" type=\"text/javascript\"> window.close(); </Script>");
                        } else if (adminData.AdminForm == AdminFormContentChildTool) {
                            adminBody = (GetContentChildTool(cp));
                        } else if (adminData.AdminForm == AdminformHousekeepingControl) {
                            adminBody = (GetForm_HouseKeepingControl(cp));
                        } else if ((adminData.AdminForm == AdminFormTools) || (adminData.AdminForm >= 100 && adminData.AdminForm <= 199)) {
                            LegacyToolsClass Tools = new LegacyToolsClass(cp.core);
                            adminBody = Tools.getToolsList();
                        } else if (adminData.AdminForm == AdminFormDownloads) {
                            adminBody = (ToolDownloads.GetForm_Downloads(cp.core));
                        } else if (adminData.AdminForm == AdminformRSSControl) {
                            adminBody = cp.core.webServer.redirect("?cid=" + ContentMetadataModel.getContentId(cp.core, "RSS Feeds"), "RSS Control page is not longer supported. RSS Feeds are controlled from the RSS feed records.");
                        } else if (adminData.AdminForm == AdminFormImportWizard) {
                            adminBody = cp.core.addon.execute(addonGuidImportWizard, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorContextMessage = "get Import Wizard for Admin"
                            });
                        } else if (adminData.AdminForm == AdminFormCustomReports) {
                            adminBody = ToolCustomReports.getForm_CustomReports(cp.core);
                        } else if (adminData.AdminForm == AdminFormFormWizard) {
                            adminBody = cp.core.addon.execute(addonGuidFormWizard, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                errorContextMessage = "get Form Wizard for Admin"
                            });
                        } else if (adminData.AdminForm == AdminFormLegacyAddonManager) {
                            adminBody = AddonController.getAddonManager(cp.core);
                        } else if (adminData.AdminForm == AdminFormEditorConfig) {
                            adminBody = FormEditConfig.getForm_EditConfig(cp.core);
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
                            cp.core.doc.addRefreshQueryString("addonguid", addonGuidAddonManager);
                            adminBody = AddonController.getAddonManager(cp.core);
                        } else {
                            AddonModel addon = null;
                            string executeContextErrorCaption = "unknown";
                            if (addonId != 0) {
                                executeContextErrorCaption = " addon id:" + addonId + " for Admin";
                                cp.core.doc.addRefreshQueryString("addonid", addonId.ToString());
                                addon = AddonModel.create(cp.core, addonId);
                            } else if (!string.IsNullOrEmpty(AddonGuid)) {
                                executeContextErrorCaption = "addon guid:" + AddonGuid + " for Admin";
                                cp.core.doc.addRefreshQueryString("addonguid", AddonGuid);
                                addon = AddonModel.create(cp.core, AddonGuid);
                            } else if (!string.IsNullOrEmpty(AddonName)) {
                                executeContextErrorCaption = "addon name:" + AddonName + " for Admin";
                                cp.core.doc.addRefreshQueryString("addonname", AddonName);
                                addon = AddonModel.createByUniqueName(cp.core, AddonName);
                            }
                            if (addon != null) {
                                addonId = addon.id;
                                AddonName = addon.name;
                                string AddonHelpCopy = addon.help;
                                cp.core.doc.addRefreshQueryString(RequestNameRunAddon, addonId.ToString());
                            }
                            string InstanceOptionString = cp.core.userProperty.getText("Addon [" + AddonName + "] Options", "");
                            int DefaultWrapperID = -1;
                            adminBody = cp.core.addon.execute(addon, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                addonType = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                instanceGuid = adminSiteInstanceId,
                                argumentKeyValuePairs = GenericController.convertQSNVAArgumentstoDocPropertiesList(cp.core, InstanceOptionString),
                                wrapperID = DefaultWrapperID,
                                errorContextMessage = executeContextErrorCaption
                            });
                            if (string.IsNullOrEmpty(adminBody)) {
                                //
                                // empty returned, display desktop
                                adminBody = FormRoot.GetForm_Root(cp.core);
                            }

                        }
                    } else {
                        //
                        // nothing so far, display desktop
                        adminBody = FormRoot.GetForm_Root(cp.core);
                    }
                    //
                    // include fancybox if it was needed
                    if (adminData.includeFancyBox) {
                        cp.core.addon.executeDependency(AddonModel.create(cp.core, addonGuidjQueryFancyBox), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                            addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                            errorContextMessage = "adding fancybox dependency in Admin"
                        });
                        cp.core.html.addScriptCode_onLoad(adminData.fancyBoxHeadJS, "");
                    }
                    //
                    // add user errors
                    if (!string.IsNullOrEmpty(cp.core.doc.debug_iUserError)) {
                        adminBody = HtmlController.div(Processor.Controllers.ErrorController.getUserError(cp.core), "ccAdminMsg") + adminBody;
                    }
                    Stream.Add(getAdminHeader(cp, adminData));
                    Stream.Add(adminBody);
                    Stream.Add(adminData.adminFooter);
                    adminData.JavaScriptString += "ButtonObjectCount = " + adminData.ButtonObjectCount + ";";
                    cp.core.html.addScriptCode(adminData.JavaScriptString, "Admin Site");
                    result = Stream.Text;
                }
                if (cp.core.session.user.developer) {
                    result = Processor.Controllers.ErrorController.getDocExceptionHtmlList(cp.core) + result;
                }
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        private string GetAddonHelp(CPClass cp, int HelpAddonID, string UsedIDString) {
            string addonHelp = "";
            try {
                string IconFilename = null;
                int IconWidth = 0;
                int IconHeight = 0;
                int IconSprites = 0;
                bool IconIsInline = false;
                string AddonName = "";
                string AddonHelpCopy = "";
                DateTime AddonDateAdded = default(DateTime);
                DateTime AddonLastUpdated = default(DateTime);
                string IncludeHelp = "";
                string IconImg = "";
                string helpLink = "";
                bool FoundAddon = false;
                //
                if (GenericController.vbInstr(1, "," + UsedIDString + ",", "," + HelpAddonID.ToString() + ",") == 0) {
                    using (var csData = new CsModel(cp.core)) {
                        csData.openRecord(Processor.Models.Db.AddonModel.contentName, HelpAddonID);
                        if (csData.ok()) {
                            FoundAddon = true;
                            AddonName = csData.getText("Name");
                            AddonHelpCopy = csData.getText("help");
                            AddonDateAdded = csData.getDate("dateadded");
                            AddonLastUpdated = csData.getDate("lastupdated");
                            if (AddonLastUpdated == DateTime.MinValue) {
                                AddonLastUpdated = AddonDateAdded;
                            }
                            IconFilename = csData.getText("Iconfilename");
                            IconWidth = csData.getInteger("IconWidth");
                            IconHeight = csData.getInteger("IconHeight");
                            IconSprites = csData.getInteger("IconSprites");
                            IconIsInline = csData.getBoolean("IsInline");
                            IconImg = AddonController.getAddonIconImg("/" + cp.core.appConfig.adminRoute, IconWidth, IconHeight, IconSprites, IconIsInline, "", IconFilename, cp.core.appConfig.cdnFileUrl, AddonName, AddonName, "", 0);
                            helpLink = csData.getText("helpLink");
                        }
                    }
                    //
                    if (FoundAddon) {
                        //
                        // Included Addons
                        //
                        foreach (var addonon in cp.core.addonCache.getDependsOnList(HelpAddonID)) {
                            IncludeHelp += GetAddonHelp(cp, addonon.id, HelpAddonID + "," + addonon.id.ToString());
                        }
                        //SQL = "select IncludedAddonID from ccAddonIncludeRules where AddonID=" + HelpAddonID;
                        //CS = csData.csOpenSql(SQL, "Default");
                        //while (csData.csOk()) {
                        //    IncludeID = csData.csGetInteger("IncludedAddonID");
                        //    IncludeHelp = IncludeHelp + GetAddonHelp(cp, IncludeID, HelpAddonID + "," + IncludeID.ToString());
                        //    csData.csGoNext();
                        //}
                        //csData.csClose();
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
                LogController.handleError(cp.core, ex);
                throw;
            }
            return addonHelp;
        }
        //
        //====================================================================================================
        //
        private string GetCollectionHelp(CPClass cp, int HelpCollectionID, string UsedIDString) {
            string returnHelp = "";
            try {
                string Collectionname = "";
                string CollectionHelpCopy = "";
                string CollectionHelpLink = "";
                DateTime CollectionDateAdded = default(DateTime);
                DateTime CollectionLastUpdated = default(DateTime);
                string IncludeHelp = "";
                //
                if (GenericController.vbInstr(1, "," + UsedIDString + ",", "," + HelpCollectionID.ToString() + ",") == 0) {
                    using (var csData = new CsModel(cp.core)) {
                        csData.openRecord("Add-on Collections", HelpCollectionID);
                        if (csData.ok()) {
                            Collectionname = csData.getText("Name");
                            CollectionHelpCopy = csData.getText("help");
                            CollectionDateAdded = csData.getDate("dateadded");
                            CollectionLastUpdated = csData.getDate("lastupdated");
                            CollectionHelpLink = csData.getText("helplink");
                            if (CollectionLastUpdated == DateTime.MinValue) {
                                CollectionLastUpdated = CollectionDateAdded;
                            }
                        }
                    }
                    //
                    // Add-ons
                    //
                    using (var csData = new CsModel(cp.core)) {
                        csData.open(Processor.Models.Db.AddonModel.contentName, "CollectionID=" + HelpCollectionID, "name");
                        while (csData.ok()) {
                            IncludeHelp = IncludeHelp + "<div style=\"clear:both;\">" + GetAddonHelp(cp, csData.getInteger("ID"), "") + "</div>";
                            csData.goNext();
                        }
                    }
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
                LogController.handleError(cp.core, ex);
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
        private void getFieldHelpMsgs(CPClass cp, int ContentID, string FieldName, ref string return_Default, ref string return_Custom) {
            try {
                //
                string SQL = null;
                bool Found = false;
                int ParentID = 0;
                //
                Found = false;
                using (var csData = new CsModel(cp.core)) {
                    SQL = "select h.HelpDefault,h.HelpCustom from ccfieldhelp h left join ccfields f on f.id=h.fieldid where f.contentid=" + ContentID + " and f.name=" + DbController.encodeSQLText(FieldName);
                    csData.openSql(SQL);
                    if (csData.ok()) {
                        Found = true;
                        return_Default = csData.getText("helpDefault");
                        return_Custom = csData.getText("helpCustom");
                    }
                }
                //
                if (!Found) {
                    ParentID = 0;
                    using (var csData = new CsModel(cp.core)) {
                        SQL = "select parentid from cccontent where id=" + ContentID;
                        csData.openSql(SQL);
                        if (csData.ok()) {
                            ParentID = csData.getInteger("parentid");
                        }
                    }
                    if (ParentID != 0) {
                        getFieldHelpMsgs(cp, ParentID, FieldName, ref return_Default, ref return_Custom);
                    }
                }
                //
                return;
                //
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
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
        private void ProcessActions(CPClass cp, AdminDataModel adminData, bool UseContentWatchLink) {
            try {
                int RecordID = 0;
                string ContentName = null;
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
                        using (var db = new DbController(cp.core, adminData.adminContent.dataSourceName)) {
                            switch (adminData.Admin_Action) {
                                case Constants.AdminActionEditRefresh:
                                    //
                                    // Load the record as if it will be saved, but skip the save
                                    //
                                    adminData.LoadEditRecord(cp.core);
                                    adminData.LoadEditRecord_Request(cp.core);
                                    break;
                                case Constants.AdminActionMarkReviewed:
                                    //
                                    // Mark the record reviewed without making any changes
                                    //
                                    Processor.Models.Db.PageContentModel.markReviewed(cp.core, adminData.editRecord.id);
                                    break;
                                case Constants.AdminActionDelete:
                                    if (adminData.editRecord.userReadOnly) {
                                        Processor.Controllers.ErrorController.addUserError(cp.core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                    } else {
                                        adminData.LoadEditRecord(cp.core);
                                        db.deleteTableRecord(adminData.editRecord.id, adminData.adminContent.tableName);
                                        cp.core.processAfterSave(true, adminData.editRecord.contentControlId_Name, adminData.editRecord.id, adminData.editRecord.nameLc, adminData.editRecord.parentID, UseContentWatchLink);
                                    }
                                    adminData.Admin_Action = Constants.AdminActionNop;
                                    break;
                                case Constants.AdminActionSave:
                                    //
                                    // ----- Save Record
                                    //
                                    if (adminData.editRecord.userReadOnly) {
                                        Processor.Controllers.ErrorController.addUserError(cp.core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                    } else {
                                        adminData.LoadEditRecord(cp.core);
                                        adminData.LoadEditRecord_Request(cp.core);
                                        ProcessActionSave(cp, adminData, UseContentWatchLink);
                                        cp.core.processAfterSave(false, adminData.adminContent.name, adminData.editRecord.id, adminData.editRecord.nameLc, adminData.editRecord.parentID, UseContentWatchLink);
                                    }
                                    adminData.Admin_Action = Constants.AdminActionNop; // convert so action can be used in as a refresh
                                                                                       //
                                    break;
                                case Constants.AdminActionSaveAddNew:
                                    //
                                    // ----- Save and add a new record
                                    //
                                    if (adminData.editRecord.userReadOnly) {
                                        Processor.Controllers.ErrorController.addUserError(cp.core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                    } else {
                                        adminData.LoadEditRecord(cp.core);
                                        adminData.LoadEditRecord_Request(cp.core);
                                        ProcessActionSave(cp, adminData, UseContentWatchLink);
                                        cp.core.processAfterSave(false, adminData.adminContent.name, adminData.editRecord.id, adminData.editRecord.nameLc, adminData.editRecord.parentID, UseContentWatchLink);
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
                                    ProcessActionDuplicate(cp, adminData);
                                    adminData.Admin_Action = Constants.AdminActionNop;
                                    break;
                                case Constants.AdminActionSendEmail:
                                    //
                                    // ----- Send (Group Email Only)
                                    //
                                    if (adminData.editRecord.userReadOnly) {
                                        Processor.Controllers.ErrorController.addUserError(cp.core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                    } else {
                                        adminData.LoadEditRecord(cp.core);
                                        adminData.LoadEditRecord_Request(cp.core);
                                        ProcessActionSave(cp, adminData, UseContentWatchLink);
                                        cp.core.processAfterSave(false, adminData.adminContent.name, adminData.editRecord.id, adminData.editRecord.nameLc, adminData.editRecord.parentID, UseContentWatchLink);
                                        if (string.IsNullOrEmpty(cp.core.doc.debug_iUserError)) {
                                            using (var csData = new CsModel(cp.core)) {
                                                csData.openRecord("Group Email", adminData.editRecord.id);
                                                if (!csData.ok()) {
                                                    //throw new GenericException("Unexpected exception"); // //throw new GenericException("Unexpected exception")' cp.core.handleLegacyError23("Email ID [" &  adminContext.editRecord.id & "] could not be found in Group Email.")
                                                } else if (csData.getText("FromAddress") == "") {
                                                    Processor.Controllers.ErrorController.addUserError(cp.core, "A 'From Address' is required before sending an email.");
                                                } else if (csData.getText("Subject") == "") {
                                                    Processor.Controllers.ErrorController.addUserError(cp.core, "A 'Subject' is required before sending an email.");
                                                } else {
                                                    csData.set("submitted", true);
                                                    csData.set("ConditionID", 0);
                                                    if (csData.getDate("ScheduleDate") == DateTime.MinValue) {
                                                        csData.set("ScheduleDate", cp.core.doc.profileStartTime);
                                                    }
                                                }
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
                                    if (adminData.editRecord.userReadOnly) {
                                        Processor.Controllers.ErrorController.addUserError(cp.core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                    } else {
                                        // no save, page was read only - Call ProcessActionSave
                                        adminData.LoadEditRecord(cp.core);
                                        if (string.IsNullOrEmpty(cp.core.doc.debug_iUserError)) {
                                            using (var csData = new CsModel(cp.core)) {
                                                if (csData.openRecord("Conditional Email", adminData.editRecord.id)) { csData.set("submitted", false); }
                                                csData.close();
                                            }
                                        }
                                    }
                                    adminData.Admin_Action = Constants.AdminActionNop; // convert so action can be used in as a refresh
                                    break;
                                case Constants.AdminActionActivateEmail:
                                    //
                                    // ----- Activate (Conditional Email Only)
                                    //
                                    if (adminData.editRecord.userReadOnly) {
                                        Processor.Controllers.ErrorController.addUserError(cp.core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                    } else {
                                        adminData.LoadEditRecord(cp.core);
                                        adminData.LoadEditRecord_Request(cp.core);
                                        ProcessActionSave(cp, adminData, UseContentWatchLink);
                                        cp.core.processAfterSave(false, adminData.adminContent.name, adminData.editRecord.id, adminData.editRecord.nameLc, adminData.editRecord.parentID, UseContentWatchLink);
                                        if (string.IsNullOrEmpty(cp.core.doc.debug_iUserError)) {
                                            using (var csData = new CsModel(cp.core)) {
                                                csData.openRecord("Conditional Email", adminData.editRecord.id);
                                                if (!csData.ok()) {
                                                    //throw new GenericException("Unexpected exception"); // //throw new GenericException("Unexpected exception")' cp.core.handleLegacyError23("Email ID [" & editRecord.id & "] could not be opened.")
                                                } else if (csData.getInteger("ConditionID") == 0) {
                                                    Processor.Controllers.ErrorController.addUserError(cp.core, "A condition must be set.");
                                                } else {
                                                    csData.set("submitted", true);
                                                    if (csData.getDate("ScheduleDate") == DateTime.MinValue) {
                                                        csData.set("ScheduleDate", cp.core.doc.profileStartTime);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    adminData.Admin_Action = Constants.AdminActionNop; // convert so action can be used in as a refresh
                                    break;
                                case Constants.AdminActionSendEmailTest:
                                    if (adminData.editRecord.userReadOnly) {
                                        Processor.Controllers.ErrorController.addUserError(cp.core, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                    } else {
                                        //
                                        adminData.LoadEditRecord(cp.core);
                                        adminData.LoadEditRecord_Request(cp.core);
                                        ProcessActionSave(cp, adminData, UseContentWatchLink);
                                        cp.core.processAfterSave(false, adminData.adminContent.name, adminData.editRecord.id, adminData.editRecord.nameLc, adminData.editRecord.parentID, UseContentWatchLink);
                                        //
                                        if (!(cp.core.doc.debug_iUserError != "")) {
                                            //
                                            EmailToConfirmationMemberID = 0;
                                            if (adminData.editRecord.fieldsLc.ContainsKey("testmemberid")) {
                                                EmailToConfirmationMemberID = GenericController.encodeInteger(adminData.editRecord.fieldsLc["testmemberid"].value);
                                                EmailController.queueConfirmationTestEmail(cp.core, adminData.editRecord.id, EmailToConfirmationMemberID);
                                                //
                                                if ((cp.core.doc.userErrorList.Count == 0) && (adminData.editRecord.fieldsLc.ContainsKey("lastsendtestdate"))) {
                                                    //
                                                    // -- if there were no errors, and the table supports lastsendtestdate, update it
                                                    adminData.editRecord.fieldsLc["lastsendtestdate"].value = cp.core.doc.profileStartTime;
                                                    db.executeQuery("update ccemail Set lastsendtestdate=" + DbController.encodeSQLDate(cp.core.doc.profileStartTime) + " where id=" + adminData.editRecord.id);
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
                                    RowCnt = cp.core.docProperties.getInteger("rowcnt");
                                    if (RowCnt > 0) {
                                        for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                            if (cp.core.docProperties.getBoolean("row" + RowPtr)) {
                                                using (var csData = new CsModel(cp.core)) {
                                                    csData.openRecord(adminData.adminContent.name, cp.core.docProperties.getInteger("rowid" + RowPtr));
                                                    if (csData.ok()) {
                                                        RecordID = csData.getInteger("ID");
                                                        csData.deleteRecord();
                                                        //
                                                        // non-Workflow Delete
                                                        //
                                                        ContentName = MetadataController.getContentNameByID(cp.core, csData.getInteger("contentControlId"));
                                                        cp.core.cache.invalidateDbRecord(RecordID, adminData.adminContent.tableName);
                                                        cp.core.processAfterSave(true, ContentName, RecordID, "", 0, UseContentWatchLink);
                                                        //
                                                        // Page Content special cases
                                                        //
                                                        if (GenericController.vbLCase(adminData.adminContent.tableName) == "ccpagecontent") {
                                                            //Call cp.core.pages.cache_pageContent_removeRow(RecordID, False, False)
                                                            if (RecordID == (cp.core.siteProperties.getInteger("PageNotFoundPageID", 0))) {
                                                                cp.core.siteProperties.getText("PageNotFoundPageID", "0");
                                                            }
                                                            if (RecordID == (cp.core.siteProperties.getInteger("LandingPageID", 0))) {
                                                                cp.core.siteProperties.getText("LandingPageID", "0");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case Constants.AdminActionReloadCDef:
                                    //
                                    // ccContent - save changes and reload content definitions
                                    //
                                    if (adminData.editRecord.userReadOnly) {
                                        Processor.Controllers.ErrorController.addUserError(cp.core, "Your request was blocked because the record you specified Is now locked by another authcontext.user.");
                                    } else {
                                        adminData.LoadEditRecord(cp.core);
                                        adminData.LoadEditRecord_Request(cp.core);
                                        ProcessActionSave(cp, adminData, UseContentWatchLink);
                                        cp.core.cache.invalidateAll();
                                        cp.core.clearMetaData();
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
                }
                //
                return;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                Processor.Controllers.ErrorController.addUserError(cp.core, "There was an unknown error processing this page at " + cp.core.doc.profileStartTime + ". Please try again, Or report this error To the site administrator.");
                LogController.handleError(cp.core, ex);
            }
        }
        //
        //========================================================================
        // LoadAndSaveContentGroupRules
        //
        //   For a particular content, remove previous GroupRules, and Create new ones
        //========================================================================
        //
        private void LoadAndSaveContentGroupRules(CPClass cp, int GroupID) {
            try {
                //
                int ContentCount = 0;
                int ContentPointer = 0;
                int ContentID = 0;
                bool AllowAdd = false;
                bool AllowDelete = false;
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
                cp.core.db.executeQuery(SQL);
                //
                // --- create GroupRule records for all selected
                //
                using (var csData = new CsModel(cp.core)) {
                    csData.open("Group Rules", "GroupID=" + GroupID, "ContentID, ID", true);
                    ContentCount = cp.core.docProperties.getInteger("ContentCount");
                    if (ContentCount > 0) {
                        for (ContentPointer = 0; ContentPointer < ContentCount; ContentPointer++) {
                            RuleNeeded = cp.core.docProperties.getBoolean("Content" + ContentPointer);
                            ContentID = cp.core.docProperties.getInteger("ContentID" + ContentPointer);
                            AllowAdd = cp.core.docProperties.getBoolean("ContentGroupRuleAllowAdd" + ContentPointer);
                            AllowDelete = cp.core.docProperties.getBoolean("ContentGroupRuleAllowDelete" + ContentPointer);
                            //
                            RuleFound = false;
                            csData.goFirst();
                            if (csData.ok()) {
                                while (csData.ok()) {
                                    if (csData.getInteger("ContentID") == ContentID) {
                                        RuleId = csData.getInteger("id");
                                        RuleFound = true;
                                        break;
                                    }
                                    csData.goNext();
                                }
                            }
                            if (RuleNeeded && !RuleFound) {
                                using (var CSNew = new CsModel(cp.core)) {
                                    CSNew.insert("Group Rules");
                                    if (CSNew.ok()) {
                                        CSNew.set("GroupID", GroupID);
                                        CSNew.set("ContentID", ContentID);
                                        CSNew.set("AllowAdd", AllowAdd);
                                        CSNew.set("AllowDelete", AllowDelete);
                                    }
                                }
                                RecordChanged = true;
                            } else if (RuleFound && !RuleNeeded) {
                                DeleteIdList += ", " + RuleId;
                                //Call cp.core.main_DeleteCSRecord(CSPointer)
                                RecordChanged = true;
                            } else if (RuleFound && RuleNeeded) {
                                if (AllowAdd != csData.getBoolean("AllowAdd")) {
                                    csData.set("AllowAdd", AllowAdd);
                                    RecordChanged = true;
                                }
                                if (AllowDelete != csData.getBoolean("AllowDelete")) {
                                    csData.set("AllowDelete", AllowDelete);
                                    RecordChanged = true;
                                }
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(DeleteIdList)) {
                    SQL = "delete from ccgrouprules where id In (" + DeleteIdList.Substring(1) + ")";
                    cp.core.db.executeQuery(SQL);
                }
                if (RecordChanged) {
                    GroupRuleModel.invalidateTableCache(cp.core);
                }
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
        }
        //
        //========================================================================
        // LoadAndSaveGroupRules
        //   read groups from the edit form and modify Group Rules to match
        //========================================================================
        //
        private void LoadAndSaveGroupRules(CPClass cp, EditRecordClass editRecord) {
            try {
                //
                if (editRecord.id != 0) {
                    LoadAndSaveGroupRules_ForContentAndChildren(cp, editRecord.id, "");
                }
                //
                return;
                //
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
        }
        //
        //========================================================================
        // LoadAndSaveGroupRules_ForContentAndChildren
        //   read groups from the edit form and modify Group Rules to match
        //========================================================================
        //
        private void LoadAndSaveGroupRules_ForContentAndChildren(CPClass cp, int ContentID, string ParentIDString) {
            try {
                string MyParentIDString = null;
                //
                // --- Create Group Rules for this content
                //
                if (encodeBoolean(ParentIDString.IndexOf("," + ContentID + ",") + 1)) {
                    throw (new Exception("Child ContentID [" + ContentID + "] Is its own parent"));
                } else {
                    MyParentIDString = ParentIDString + "," + ContentID + ",";
                    LoadAndSaveGroupRules_ForContent(cp, ContentID);
                    //
                    // --- Create Group Rules for all child content
                    //
                    using (var csData = new CsModel(cp.core)) {
                        csData.open("Content", "ParentID=" + ContentID);
                        while (csData.ok()) {
                            LoadAndSaveGroupRules_ForContentAndChildren(cp, csData.getInteger("id"), MyParentIDString);
                            csData.goNext();
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
        }
        //
        //========================================================================
        // LoadAndSaveGroupRules_ForContent
        //
        //   For a particular content, remove previous GroupRules, and Create new ones
        //========================================================================
        //
        private void LoadAndSaveGroupRules_ForContent(CPClass cp, int ContentID) {
            try {
                //
                int GroupCount = 0;
                int GroupPointer = 0;
                int GroupID = 0;
                bool AllowAdd = false;
                bool AllowDelete = false;
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
                cp.core.db.executeQuery(SQL);
                //
                // --- create GroupRule records for all selected
                //
                using (var csData = new CsModel(cp.core)) {
                    csData.open("Group Rules", "ContentID=" + ContentID, "GroupID,ID", true);
                    GroupCount = cp.core.docProperties.getInteger("GroupCount");
                    if (GroupCount > 0) {
                        for (GroupPointer = 0; GroupPointer < GroupCount; GroupPointer++) {
                            RuleNeeded = cp.core.docProperties.getBoolean("Group" + GroupPointer);
                            GroupID = cp.core.docProperties.getInteger("GroupID" + GroupPointer);
                            AllowAdd = cp.core.docProperties.getBoolean("GroupRuleAllowAdd" + GroupPointer);
                            AllowDelete = cp.core.docProperties.getBoolean("GroupRuleAllowDelete" + GroupPointer);
                            //
                            RuleFound = false;
                            csData.goFirst();
                            if (csData.ok()) {
                                while (csData.ok()) {
                                    if (csData.getInteger("GroupID") == GroupID) {
                                        RuleFound = true;
                                        break;
                                    }
                                    csData.goNext();
                                }
                            }
                            if (RuleNeeded && !RuleFound) {
                                using (var CSNew = new CsModel(cp.core)) {
                                    CSNew.insert("Group Rules");
                                    if (CSNew.ok()) {
                                        CSNew.set("ContentID", ContentID);
                                        CSNew.set("GroupID", GroupID);
                                        CSNew.set("AllowAdd", AllowAdd);
                                        CSNew.set("AllowDelete", AllowDelete);
                                    }
                                }
                                RecordChanged = true;
                            } else if (RuleFound && !RuleNeeded) {
                                csData.deleteRecord();
                                RecordChanged = true;
                            } else if (RuleFound && RuleNeeded) {
                                if (AllowAdd != csData.getBoolean("AllowAdd")) {
                                    csData.set("AllowAdd", AllowAdd);
                                    RecordChanged = true;
                                }
                                if (AllowDelete != csData.getBoolean("AllowDelete")) {
                                    csData.set("AllowDelete", AllowDelete);
                                    RecordChanged = true;
                                }
                            }
                        }
                    }
                }
                if (RecordChanged) {
                    GroupRuleModel.invalidateTableCache(cp.core);
                }
                return;
                //
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
        }
        //========================================================================
        //   Save Whats New values if present
        //
        //   does NOT check AuthoringLocked -- you must check before calling
        //========================================================================
        //
        private void SaveContentTracking(CPClass cp, AdminDataModel adminData) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                int ContentID = 0;
                int ContentWatchID = 0;
                //
                if (adminData.adminContent.allowContentTracking & (!editRecord.userReadOnly)) {
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
                    using (var csData = new CsModel(cp.core)) {
                        csData.open("Content Watch", "(ContentID=" + DbController.encodeSQLNumber(ContentID) + ")And(RecordID=" + DbController.encodeSQLNumber(editRecord.id) + ")");
                        if (!csData.ok()) {
                            csData.insert("Content Watch");
                            csData.set("contentid", ContentID);
                            csData.set("recordid", editRecord.id);
                            csData.set("ContentRecordKey", ContentID + "." + editRecord.id);
                            csData.set("clicks", 0);
                        }
                        if (!csData.ok()) {
                            LogController.handleError(cp.core, new GenericException("SaveContentTracking, can Not create New record"));
                        } else {
                            ContentWatchID = csData.getInteger("ID");
                            csData.set("LinkLabel", adminData.ContentWatchLinkLabel);
                            csData.set("WhatsNewDateExpires", adminData.ContentWatchExpires);
                            csData.set("Link", adminData.ContentWatchLink);
                            //
                            // ----- delete all rules for this ContentWatch record
                            //
                            using (var CSPointer = new CsModel(cp.core)) {
                                CSPointer.open("Content Watch List Rules", "(ContentWatchID=" + ContentWatchID + ")");
                                while (CSPointer.ok()) {
                                    CSPointer.deleteRecord();
                                    CSPointer.goNext();
                                }
                                CSPointer.close();
                            }
                            //
                            // ----- Update ContentWatchListRules for all entries in ContentWatchListID( ContentWatchListIDCount )
                            //
                            int ListPointer = 0;
                            if (adminData.ContentWatchListIDCount > 0) {
                                for (ListPointer = 0; ListPointer < adminData.ContentWatchListIDCount; ListPointer++) {
                                    using (var CSRules = new CsModel(cp.core)) {
                                        CSRules.insert("Content Watch List Rules");
                                        if (CSRules.ok()) {
                                            CSRules.set("ContentWatchID", ContentWatchID);
                                            CSRules.set("ContentWatchListID", adminData.ContentWatchListID[ListPointer]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
        }
        //
        //========================================================================
        //   Save Link Alias field if it supported, and is non-authoring
        //   if it is authoring, it will be saved by the userfield routines
        //   if not, it appears in the LinkAlias tab, and must be saved here
        //========================================================================
        //
        private void SaveLinkAlias(CPClass cp, AdminDataModel adminData) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                // --use field ptr to test if the field is supported yet
                if (cp.core.siteProperties.allowLinkAlias) {
                    bool isDupError = false;
                    string linkAlias = cp.core.docProperties.getText("linkalias");
                    bool OverRideDuplicate = cp.core.docProperties.getBoolean("OverRideDuplicate");
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
                            cp.core.db.executeQuery("update " + adminData.adminContent.tableName + " set linkalias=null where ( linkalias=" + DbController.encodeSQLText(linkAlias) + ") and (id<>" + editRecord.id + ")");
                        } else {
                            using (var csData = new CsModel(cp.core)) {
                                csData.open(adminData.adminContent.name, "( linkalias=" + DbController.encodeSQLText(linkAlias) + ")and(id<>" + editRecord.id + ")");
                                if (csData.ok()) {
                                    isDupError = true;
                                    ErrorController.addUserError(cp.core, "The Link Alias you entered can not be used because another record uses this value [" + linkAlias + "]. Enter a different Link Alias, or check the Override Duplicates checkbox in the Link Alias tab.");
                                }
                                csData.close();
                            }
                        }
                        if (!isDupError) {
                            DupCausesWarning = true;
                            using (var csData = new CsModel(cp.core)) {
                                csData.openRecord(adminData.adminContent.name, editRecord.id);
                                if (csData.ok()) {
                                    csData.set("linkalias", linkAlias);
                                }
                            }
                            //
                            // Update the Link Aliases
                            //
                            LinkAliasController.addLinkAlias(cp.core, linkAlias, editRecord.id, "", OverRideDuplicate, DupCausesWarning);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
        }
        //
        //========================================================================
        //
        private void SaveEditRecord(CPClass cp, AdminDataModel adminData) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                int SaveCCIDValue = 0;
                int ActivityLogOrganizationID = -1;
                if (cp.core.doc.debug_iUserError != "") {
                    //
                    // -- If There is an error, block the save
                    adminData.Admin_Action = Constants.AdminActionNop;
                } else if (!cp.core.session.isAuthenticatedContentManager(cp.core, adminData.adminContent.name)) {
                    //
                    // -- must be content manager
                } else if (editRecord.userReadOnly) {
                    //
                    // -- read only block
                } else {
                    //
                    // -- Record will be saved, create a new one if this is an add
                    bool NewRecord = false;
                    bool recordChanged = false;
                    using (var csData = new CsModel(cp.core)) {
                        if (editRecord.id == 0) {
                            NewRecord = true;
                            recordChanged = true;
                            csData.insert(adminData.adminContent.name);
                        } else {
                            NewRecord = false;
                            csData.openRecord(adminData.adminContent.name, editRecord.id);
                        }
                        if (!csData.ok()) {
                            //
                            // ----- Error: new record could not be created
                            //
                            if (NewRecord) {
                                //
                                // Could not insert record
                                //
                                LogController.handleError(cp.core, new GenericException("A new record could not be inserted for content [" + adminData.adminContent.name + "]. Verify the Database table and field DateAdded, CreateKey, and ID."));
                            } else {
                                //
                                // Could not locate record you requested
                                //
                                LogController.handleError(cp.core, new GenericException("The record you requested (ID=" + editRecord.id + ") could not be found for content [" + adminData.adminContent.name + "]"));
                            }
                        } else {
                            //
                            // ----- Get the ID of the current record
                            //
                            editRecord.id = csData.getInteger("ID");
                            //
                            // ----- Create the update sql
                            //
                            bool fieldChanged = false;
                            foreach (var keyValuePair in adminData.adminContent.fields) {
                                ContentFieldMetadataModel field = keyValuePair.Value;
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
                                                if (csData.getText(fieldName) != FieldValueText) {
                                                    fieldChanged = true;
                                                    recordChanged = true;
                                                    csData.set(fieldName, FieldValueText);
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
                                            if (csData.getBoolean(fieldName) != saveValue) {
                                                fieldChanged = true;
                                                recordChanged = true;
                                                csData.set(fieldName, saveValue);
                                            }
                                            break;
                                        }
                                    case "DATEEXPIRES": {
                                            //
                                            // ----- make sure content watch expires before content expires
                                            //
                                            if (!GenericController.IsNull(fieldValueObject)) {
                                                if (GenericController.IsDate(fieldValueObject)) {
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
                                                if (GenericController.IsDate(fieldValueObject)) {
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
                                if (AdminDataModel.IsVisibleUserField(cp.core, field.adminOnly, field.developerOnly, field.active, field.authorable, field.nameLc, adminData.adminContent.tableName) && (NewRecord || (!field.readOnly)) && (NewRecord || (!field.notEditable))) {
                                    //
                                    // ----- save the value by field type
                                    //
                                    switch (field.fieldTypeId) {
                                        case CPContentBaseClass.fileTypeIdEnum.AutoIdIncrement:
                                        case CPContentBaseClass.fileTypeIdEnum.Redirect: {
                                                //
                                                // do nothing with these
                                                //
                                                break;
                                            }
                                        case CPContentBaseClass.fileTypeIdEnum.File:
                                        case CPContentBaseClass.fileTypeIdEnum.FileImage: {
                                                //
                                                // filenames, upload to cdnFiles
                                                //
                                                if (cp.core.docProperties.getBoolean(fieldName + ".DeleteFlag")) {
                                                    recordChanged = true;
                                                    fieldChanged = true;
                                                    csData.set(fieldName, "");
                                                }
                                                string filename = GenericController.encodeText(fieldValueObject);
                                                if (!string.IsNullOrWhiteSpace(filename)) {
                                                    filename = FileController.encodeDosFilename(filename);
                                                    string unixPathFilename = csData.getFieldFilename(fieldName, filename);
                                                    string dosPathFilename = GenericController.convertToDosSlash(unixPathFilename);
                                                    string dosPath = GenericController.getPath(dosPathFilename);
                                                    cp.core.cdnFiles.upload(fieldName, dosPath, ref filename);
                                                    csData.set(fieldName, unixPathFilename);
                                                    recordChanged = true;
                                                    fieldChanged = true;
                                                }
                                                break;
                                            }
                                        case CPContentBaseClass.fileTypeIdEnum.Boolean: {
                                                //
                                                // boolean
                                                //
                                                bool saveValue = GenericController.encodeBoolean(fieldValueObject);
                                                if (csData.getBoolean(fieldName) != saveValue) {
                                                    recordChanged = true;
                                                    fieldChanged = true;
                                                    csData.set(fieldName, saveValue);
                                                }
                                                break;
                                            }
                                        case CPContentBaseClass.fileTypeIdEnum.Currency:
                                        case CPContentBaseClass.fileTypeIdEnum.Float: {
                                                //
                                                // Floating pointer numbers
                                                //
                                                double saveValue = GenericController.encodeNumber(fieldValueObject);
                                                if (csData.getNumber(fieldName) != saveValue) {
                                                    recordChanged = true;
                                                    fieldChanged = true;
                                                    csData.set(fieldName, saveValue);
                                                }
                                                break;
                                            }
                                        case CPContentBaseClass.fileTypeIdEnum.Date: {
                                                //
                                                // Date
                                                //
                                                DateTime saveValue = GenericController.encodeDate(fieldValueObject);
                                                if (csData.getDate(fieldName) != saveValue) {
                                                    fieldChanged = true;
                                                    recordChanged = true;
                                                    csData.set(fieldName, saveValue);
                                                }
                                                break;
                                            }
                                        case CPContentBaseClass.fileTypeIdEnum.Integer:
                                        case CPContentBaseClass.fileTypeIdEnum.Lookup: {
                                                //
                                                // Integers
                                                //
                                                int saveValue = GenericController.encodeInteger(fieldValueObject);
                                                if (saveValue != csData.getInteger(fieldName)) {
                                                    fieldChanged = true;
                                                    recordChanged = true;
                                                    csData.set(fieldName, saveValue);
                                                }
                                                break;
                                            }
                                        case CPContentBaseClass.fileTypeIdEnum.LongText:
                                        case CPContentBaseClass.fileTypeIdEnum.Text:
                                        case CPContentBaseClass.fileTypeIdEnum.FileText:
                                        case CPContentBaseClass.fileTypeIdEnum.FileCSS:
                                        case CPContentBaseClass.fileTypeIdEnum.FileXML:
                                        case CPContentBaseClass.fileTypeIdEnum.FileJavascript:
                                        case CPContentBaseClass.fileTypeIdEnum.HTML:
                                        case CPContentBaseClass.fileTypeIdEnum.FileHTML: {
                                                //
                                                // Text
                                                //
                                                string saveValue = GenericController.encodeText(fieldValueObject);
                                                if (csData.getText(fieldName) != saveValue) {
                                                    fieldChanged = true;
                                                    recordChanged = true;
                                                    csData.set(fieldName, saveValue);
                                                }
                                                break;
                                            }
                                        case CPContentBaseClass.fileTypeIdEnum.ManyToMany: {
                                                //
                                                // Many to Many checklist
                                                //
                                                //MTMContent0 = CdefController.getContentNameByID(cp.core,.contentId)
                                                //MTMContent1 = CdefController.getContentNameByID(cp.core,.manyToManyContentID)
                                                //MTMRuleContent = CdefController.getContentNameByID(cp.core,.manyToManyRuleContentID)
                                                //MTMRuleField0 = .ManyToManyRulePrimaryField
                                                //MTMRuleField1 = .ManyToManyRuleSecondaryField
                                                cp.core.html.processCheckList("field" + field.id, MetadataController.getContentNameByID(cp.core, field.contentId), encodeText(editRecord.id), MetadataController.getContentNameByID(cp.core, field.manyToManyContentID), MetadataController.getContentNameByID(cp.core, field.manyToManyRuleContentID), field.manyToManyRulePrimaryField, field.manyToManyRuleSecondaryField);
                                                break;
                                            }
                                        default: {
                                                //
                                                // Unknown other types
                                                //

                                                string saveValue = GenericController.encodeText(fieldValueObject);
                                                fieldChanged = true;
                                                recordChanged = true;
                                                csData.set(UcaseFieldName, saveValue);
                                                //sql &=  "," & .Name & "=" & cp.core.app.EncodeSQL(FieldValueVariant, .FieldType)
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
                                            if (cp.core.docProperties.getText("filename") != "") {
                                                csData.set("altsizelist", "");
                                            }
                                            break;
                                    }
                                    if (!NewRecord) {
                                        switch (GenericController.vbLCase(adminData.adminContent.tableName)) {
                                            case "ccmembers":
                                                //
                                                if (ActivityLogOrganizationID < 0) {
                                                    PersonModel person = PersonModel.create(cp.core, editRecord.id);
                                                    if (person != null) {
                                                        ActivityLogOrganizationID = person.organizationID;
                                                    }
                                                }
                                                LogController.addSiteActivity(cp.core, "modifying field " + fieldName, editRecord.id, ActivityLogOrganizationID);
                                                break;
                                            case "organizations":
                                                //
                                                LogController.addSiteActivity(cp.core, "modifying field " + fieldName, 0, editRecord.id);
                                                break;
                                        }
                                    }
                                }
                            }
                            if (recordChanged) {
                                //
                                // -- clear cache
                                string tableName;
                                if (editRecord.contentControlId == 0) {
                                    tableName = MetadataController.getContentTablename(cp.core, adminData.adminContent.name).ToLowerInvariant();
                                } else {
                                    tableName = MetadataController.getContentTablename(cp.core, editRecord.contentControlId_Name).ToLowerInvariant();
                                }
                                if (tableName == LinkAliasModel.contentTableName.ToLowerInvariant()) {
                                    LinkAliasModel.invalidateRecordCache(cp.core, editRecord.id);
                                } else if (tableName == AddonModel.contentTableName.ToLowerInvariant()) {
                                    AddonModel.invalidateRecordCache(cp.core, editRecord.id);
                                } else {
                                    LinkAliasModel.invalidateRecordCache(cp.core, editRecord.id);
                                }
                            }
                            //
                            // ----- clear/set authoring controls
                            var contentTable = TableModel.createByUniqueName(cp.core, adminData.adminContent.tableName);
                            if (contentTable != null) WorkflowController.clearEditLock(cp.core, contentTable.id, editRecord.id);
                            //
                            // ----- if admin content is changed, reload the adminContext.content data in case this is a save, and not an OK
                            if (recordChanged && SaveCCIDValue != 0) {
                                adminData.adminContent.setContentControlId(cp.core, editRecord.id, SaveCCIDValue);
                                editRecord.contentControlId_Name = MetadataController.getContentNameByID(cp.core, SaveCCIDValue);
                                adminData.adminContent = ContentMetadataModel.createByUniqueName(cp.core, editRecord.contentControlId_Name);
                                adminData.adminContent.id = adminData.adminContent.id;
                                adminData.adminContent.name = adminData.adminContent.name;
                            }
                        }
                    }
                    editRecord.Saved = true;
                }
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
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
        private string getAdminHeader(CPClass cp, AdminDataModel adminData, string BackgroundColor = "") {
            string result = "";
            try {
                string LeftSide = cp.core.siteProperties.getText("AdminHeaderHTML", "Contensive Administration Site");
                string RightSide = cp.core.doc.profileStartTime + "&nbsp;" + getIconRefreshLink("?" + cp.core.doc.refreshQueryString);
                //
                // Assemble header
                //
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                Stream.Add(AdminUIController.getHeader(cp.core, LeftSide, RightSide));
                //
                // --- Content Definition
                adminData.adminFooter = "";
                //
                // -- Admin Navigator
                string AdminNavFull = cp.core.addon.execute(AddonModel.create(cp.core, AdminNavigatorGuid), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                    addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                    errorContextMessage = "executing Admin Navigator in Admin"
                });
                //
                // -- shortterm fix - make navigator changes, long term pull it into project
                // "<ximg title=\"Open Navigator\" alt=\"Open Navigator\" src=\"/ContensiveBase/images/OpenRightRev1313.gif\" width=13 height=13 border=0 style=\"text-align:right;\">";
                //string src = HtmlController.img("/ContensiveBase/images/OpenRightRev1313.gif", "Open Navigator", 13, 13 ).Replace( ">", "style=\"text-align:right;\">");
                //AdminNavFull = AdminNavFull.Replace(src, iconOpen_White);
                //// "<ximg alt=\"Close Navigator\" title=\"Close Navigator\" src=\"/ContensiveBase/images/ClosexRev1313.gif\" width=13 height=13 border=0>";
                //src = HtmlController.img("/ContensiveBase/images/ClosexRev1313.gif", "Close Navigator", 13, 13);
                //AdminNavFull = AdminNavFull.Replace(src, iconClose_White);
                Stream.Add("<table border=0 cellpadding=0 cellspacing=0><tr>\r<td class=\"ccToolsCon\" valign=top>" + AdminNavFull + "</td>\r<td id=\"desktop\" class=\"ccContentCon\" valign=top>");
                adminData.adminFooter = adminData.adminFooter + "</td></tr></table>";
                //
                result = Stream.Text;
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
                throw;
            }
            return result;
        }
        ////
        ////========================================================================
        //// Get sql for menu
        ////   if MenuContentName is blank, it will select values from either cdef
        ////========================================================================
        ////
        //private string GetMenuSQL(CPClass cp, string ParentCriteria, string MenuContentName) {
        //    string result = "";
        //    try {
        //        string iParentCriteria = null;
        //        string Criteria = null;
        //        string SelectList = null;
        //        List<int> editableCdefIdList = null;
        //        //
        //        Criteria = "(Active<>0)";
        //        if (!string.IsNullOrEmpty(MenuContentName)) {
        //            //ContentControlCriteria = cp.core.csv_GetContentControlCriteria(MenuContentName)
        //            Criteria = Criteria + "AND" + MetadataController.getContentControlCriteria(cp.core, MenuContentName);
        //        }
        //        iParentCriteria = GenericController.encodeEmpty(ParentCriteria, "");
        //        if (cp.core.session.isAuthenticatedDeveloper(cp.core)) {
        //            //
        //            // ----- Developer
        //            //
        //        } else if (cp.core.session.isAuthenticatedAdmin(cp.core)) {
        //            //
        //            // ----- Administrator
        //            //
        //            Criteria = Criteria + "AND((DeveloperOnly is null)or(DeveloperOnly=0))"
        //                + "AND(ID in ("
        //                + " SELECT AllowedEntries.ID"
        //                + " FROM CCMenuEntries AllowedEntries LEFT JOIN ccContent ON AllowedEntries.ContentID = ccContent.ID"
        //                + " Where ((ccContent.Active<>0)And((ccContent.DeveloperOnly is null)or(ccContent.DeveloperOnly=0)))"
        //                    + "OR(ccContent.ID Is Null)"
        //                + "))";
        //        } else {
        //            //
        //            // ----- Content Manager
        //            //
        //            string CMCriteria = null;

        //            editableCdefIdList = MetadataController.getEditableMetaDataIdList(cp.core);
        //            if (editableCdefIdList.Count == 0) {
        //                CMCriteria = "(1=0)";
        //            } else if (editableCdefIdList.Count == 1) {
        //                CMCriteria = "(ccContent.ID=" + editableCdefIdList[0] + ")";
        //            } else {
        //                CMCriteria = "(ccContent.ID in (" + string.Join(",", editableCdefIdList) + "))";
        //            }

        //            Criteria = Criteria + "AND((DeveloperOnly is null)or(DeveloperOnly=0))"
        //                + "AND((AdminOnly is null)or(AdminOnly=0))"
        //                + "AND(ID in ("
        //                + " SELECT AllowedEntries.ID"
        //                + " FROM CCMenuEntries AllowedEntries LEFT JOIN ccContent ON AllowedEntries.ContentID = ccContent.ID"
        //                + " Where (" + CMCriteria + "and(ccContent.Active<>0)And((ccContent.DeveloperOnly is null)or(ccContent.DeveloperOnly=0))And((ccContent.AdminOnly is null)or(ccContent.AdminOnly=0)))"
        //                    + "OR(ccContent.ID Is Null)"
        //                + "))";
        //        }
        //        if (!string.IsNullOrEmpty(iParentCriteria)) {
        //            Criteria = "(" + iParentCriteria + ")AND" + Criteria;
        //        }
        //        SelectList = "ccMenuEntries.contentcontrolid, ccMenuEntries.Name, ccMenuEntries.ID, ccMenuEntries.LinkPage, ccMenuEntries.ContentID, ccMenuEntries.NewWindow, ccMenuEntries.ParentID, ccMenuEntries.AddonID, ccMenuEntries.NavIconType, ccMenuEntries.NavIconTitle, HelpAddonID,HelpCollectionID,0 as collectionid";
        //        result = "select " + SelectList + " from ccMenuEntries where " + Criteria + " order by ccMenuEntries.Name";
        //    } catch (Exception ex) {
        //        LogController.handleError(cp.core, ex);
        //    }
        //    return result;
        //}
        ////
        //========================================================================
        // Get Menu Link
        //========================================================================
        //
        private string GetMenuLink(CPClass cp, string LinkPage, int LinkCID) {
            string tempGetMenuLink = null;
            try {
                //
                int ContentID = 0;
                //
                if (!string.IsNullOrEmpty(LinkPage) || (LinkCID != 0)) {
                    tempGetMenuLink = LinkPage;
                    if (!string.IsNullOrEmpty(tempGetMenuLink)) {
                        if (tempGetMenuLink.Left(1) == "?" || tempGetMenuLink.Left(1) == "#") {
                            tempGetMenuLink = "/" + cp.core.appConfig.adminRoute + tempGetMenuLink;
                        }
                    } else {
                        tempGetMenuLink = "/" + cp.core.appConfig.adminRoute;
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
                LogController.handleError(cp.core, ex);
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
        private void ProcessForms(CPClass cp, AdminDataModel adminData) {
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                //
                if (adminData.AdminSourceForm != 0) {
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
                                    adminData.adminContent = new ContentMetadataModel();
                                    break;
                                case ButtonClose:
                                    adminData.Admin_Action = Constants.AdminActionNop;
                                    adminData.AdminForm = AdminFormRoot;
                                    adminData.adminContent = new ContentMetadataModel();
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
                                    adminData.AdminForm = GetForm_Close(cp, adminData.ignore_legacyMenuDepth, adminData.adminContent.name, editRecord.id);
                                    break;
                                case ButtonSaveandInvalidateCache:
                                    adminData.Admin_Action = Constants.AdminActionReloadCDef;
                                    adminData.AdminForm = AdminFormEdit;
                                    break;
                                case ButtonDelete:
                                    adminData.Admin_Action = Constants.AdminActionDelete;
                                    adminData.AdminForm = GetForm_Close(cp, adminData.ignore_legacyMenuDepth, adminData.adminContent.name, editRecord.id);
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
                                    adminData.AdminForm = GetForm_Close(cp, adminData.ignore_legacyMenuDepth, adminData.adminContent.name, editRecord.id);
                                    break;
                                case ButtonCancel:
                                    adminData.Admin_Action = Constants.AdminActionNop;
                                    adminData.AdminForm = GetForm_Close(cp, adminData.ignore_legacyMenuDepth, adminData.adminContent.name, editRecord.id);
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
                                    cp.core.siteProperties.setProperty("Allow CSS Reset", cp.core.docProperties.getBoolean(RequestNameAllowCSSReset));
                                    cp.core.cdnFiles.saveFile(DynamicStylesFilename, cp.core.docProperties.getText("StyleEditor"));
                                    if (cp.core.docProperties.getBoolean(RequestNameInlineStyles)) {
                                        //
                                        // Inline Styles
                                        //
                                        cp.core.siteProperties.setProperty("StylesheetSerialNumber", "0");
                                    } else {
                                        // mark to rebuild next fetch
                                        cp.core.siteProperties.setProperty("StylesheetSerialNumber", "-1");
                                        //
                                        // Linked Styles
                                        // Bump the Style Serial Number so next fetch is not cached
                                        //
                                        //StyleSN = genericController.EncodeInteger(cp.core.main_GetSiteProperty2("StylesheetSerialNumber", "0"))
                                        //StyleSN = StyleSN + 1
                                        //Call cp.core.app.setSiteProperty("StylesheetSerialNumber", genericController.encodeText(StyleSN))
                                        //
                                        // Save new public stylesheet
                                        //
                                        // 11/24/3009 - style sheet processing deprecated
                                        //Call cp.core.app.virtualFiles.SaveFile("templates\Public" & StyleSN & ".css", cp.core.main_GetStyleSheet)
                                        //Call cp.core.app.virtualFiles.SaveFile("templates\Public" & StyleSN & ".css", cp.core.main_GetStyleSheetProcessed)
                                        //Call cp.core.app.virtualFiles.SaveFile("templates\Admin" & StyleSN & ".css", cp.core.main_GetStyleSheetDefault)
                                    }
                                    //
                                    // delete all templateid based editorstylerule files, build on-demand
                                    //
                                    EditorStyleRulesFilename = GenericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", "0", 1, 99, 1);
                                    cp.core.cdnFiles.deleteFile(EditorStyleRulesFilename);
                                    //
                                    using (var csData = new CsModel(cp.core)) {
                                        csData.openSql("select id from cctemplates");
                                        while (csData.ok()) {
                                            EditorStyleRulesFilename = GenericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", csData.getText("ID"), 1, 99, 1);
                                            cp.core.cdnFiles.deleteFile(EditorStyleRulesFilename);
                                            csData.goNext();
                                        }
                                        csData.close();
                                    }
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
                LogController.handleError(cp.core, ex);
            }
        }
        //
        //       
        //=============================================================================================
        //   Get
        //=============================================================================================
        //
        private int GetForm_Close(CPClass cp, int MenuDepth, string ContentName, int RecordID) {
            int tempGetForm_Close = 0;
            try {
                //
                if (MenuDepth > 0) {
                    tempGetForm_Close = AdminFormClose;
                } else {
                    tempGetForm_Close = AdminFormIndex;
                }
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
            return tempGetForm_Close;
        }
        //
        //=============================================================================================
        //
        //=============================================================================================
        //
        private void ProcessActionSave(CPClass cp, AdminDataModel adminData, bool UseContentWatchLink) {
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
                    if (!(cp.core.doc.debug_iUserError != "")) {
                        if (GenericController.vbUCase(adminData.adminContent.tableName) == GenericController.vbUCase("ccMembers")) {
                            //
                            //
                            SaveEditRecord(cp, adminData);
                            SaveMemberRules(cp, editRecord.id);
                        } else if (GenericController.vbUCase(adminData.adminContent.tableName) == "CCEMAIL") {
                            //
                            //
                            SaveEditRecord(cp, adminData);
                        } else if (GenericController.vbUCase(adminData.adminContent.tableName) == "CCCONTENT") {
                            //
                            //
                            SaveEditRecord(cp, adminData);
                            LoadAndSaveGroupRules(cp, editRecord);
                        } else if (GenericController.vbUCase(adminData.adminContent.tableName) == "CCPAGECONTENT") {
                            //
                            //
                            SaveEditRecord(cp, adminData);
                            adminData.LoadContentTrackingDataBase(cp.core);
                            adminData.LoadContentTrackingResponse(cp.core);
                            SaveLinkAlias(cp, adminData);
                            SaveContentTracking(cp, adminData);
                        } else if (GenericController.vbUCase(adminData.adminContent.tableName) == "CCLIBRARYFOLDERS") {
                            //
                            //
                            SaveEditRecord(cp, adminData);
                            adminData.LoadContentTrackingDataBase(cp.core);
                            adminData.LoadContentTrackingResponse(cp.core);
                            cp.core.html.processCheckList("LibraryFolderRules", adminData.adminContent.name, GenericController.encodeText(editRecord.id), "Groups", "Library Folder Rules", "FolderID", "GroupID");
                            SaveContentTracking(cp, adminData);
                        } else if (GenericController.vbUCase(adminData.adminContent.tableName) == "CCSETUP") {
                            //
                            // Site Properties
                            SaveEditRecord(cp, adminData);
                            if (editRecord.nameLc.ToLowerInvariant() == "allowlinkalias") {
                                if (cp.core.siteProperties.getBoolean("AllowLinkAlias")) {
                                    TurnOnLinkAlias(cp, UseContentWatchLink);
                                }
                            }
                        } else if (GenericController.vbUCase(adminData.adminContent.tableName) == GenericController.vbUCase("ccGroups")) {
                            //
                            //
                            SaveEditRecord(cp, adminData);
                            adminData.LoadContentTrackingDataBase(cp.core);
                            adminData.LoadContentTrackingResponse(cp.core);
                            LoadAndSaveContentGroupRules(cp, editRecord.id);
                            SaveContentTracking(cp, adminData);
                        } else if (GenericController.vbUCase(adminData.adminContent.tableName) == "CCTEMPLATES") {
                            //
                            // save and clear editorstylerules for this template
                            SaveEditRecord(cp, adminData);
                            adminData.LoadContentTrackingDataBase(cp.core);
                            adminData.LoadContentTrackingResponse(cp.core);
                            SaveContentTracking(cp, adminData);
                            EditorStyleRulesFilename = GenericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", editRecord.id.ToString(), 1, 99, 1);
                            cp.core.privateFiles.deleteFile(EditorStyleRulesFilename);
                        } else {
                            //
                            //
                            SaveEditRecord(cp, adminData);
                            adminData.LoadContentTrackingDataBase(cp.core);
                            adminData.LoadContentTrackingResponse(cp.core);
                            SaveContentTracking(cp, adminData);
                        }
                    }
                }
                //
                // If the content supports datereviewed, mark it
                //
                if (cp.core.doc.debug_iUserError != "") {
                    adminData.AdminForm = adminData.AdminSourceForm;
                }
                adminData.Admin_Action = Constants.AdminActionNop; // convert so action can be used in as a refresh
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
        }
        //
        //=============================================================================================
        //
        private void ProcessActionDuplicate(CPClass cp, AdminDataModel adminData) {
            try {
                if (!(cp.core.doc.debug_iUserError != "")) {
                    switch (adminData.adminContent.tableName.ToLower()) {
                        case "ccemail":
                            //
                            // --- preload array with values that may not come back in response
                            //
                            adminData.LoadEditRecord(cp.core);
                            adminData.LoadEditRecord_Request(cp.core);
                            //
                            if (!(cp.core.doc.debug_iUserError != "")) {
                                //
                                // ----- Convert this to the Duplicate
                                //
                                if (adminData.adminContent.fields.ContainsKey("submitted")) {
                                    adminData.editRecord.fieldsLc["submitted"].value = false;
                                }
                                if (adminData.adminContent.fields.ContainsKey("sent")) {
                                    adminData.editRecord.fieldsLc["sent"].value = false;
                                }
                                if (adminData.adminContent.fields.ContainsKey("lastsendtestdate")) {
                                    adminData.editRecord.fieldsLc["lastsendtestdate"].value = "";
                                }
                                //
                                adminData.editRecord.id = 0;
                                cp.core.doc.addRefreshQueryString("id", GenericController.encodeText(adminData.editRecord.id));
                            }
                            break;
                        default:
                            //
                            // --- preload array with values that may not come back in response
                            adminData.LoadEditRecord(cp.core);
                            adminData.LoadEditRecord_Request(cp.core);
                            //
                            if (!(cp.core.doc.debug_iUserError != "")) {
                                //
                                // ----- Convert this to the Duplicate
                                adminData.editRecord.id = 0;
                                //
                                // block fields that should not duplicate
                                if (adminData.editRecord.fieldsLc.ContainsKey("ccguid")) {
                                    adminData.editRecord.fieldsLc["ccguid"].value = "";
                                }
                                //
                                if (adminData.editRecord.fieldsLc.ContainsKey("dateadded")) {
                                    adminData.editRecord.fieldsLc["dateadded"].value = DateTime.MinValue;
                                }
                                //
                                if (adminData.editRecord.fieldsLc.ContainsKey("modifieddate")) {
                                    adminData.editRecord.fieldsLc["modifieddate"].value = DateTime.MinValue;
                                }
                                //
                                if (adminData.editRecord.fieldsLc.ContainsKey("modifiedby")) {
                                    adminData.editRecord.fieldsLc["modifiedby"].value = 0;
                                }
                                //
                                // block fields that must be unique
                                foreach (KeyValuePair<string, Contensive.Processor.Models.Domain.ContentFieldMetadataModel> keyValuePair in adminData.adminContent.fields) {
                                    ContentFieldMetadataModel field = keyValuePair.Value;
                                    if (GenericController.vbLCase(field.nameLc) == "email") {
                                        if ((adminData.adminContent.tableName.ToLowerInvariant() == "ccmembers") && (GenericController.encodeBoolean(cp.core.siteProperties.getBoolean("allowemaillogin", false)))) {
                                            adminData.editRecord.fieldsLc[field.nameLc].value = "";
                                        }
                                    }
                                    if (field.uniqueName) {
                                        adminData.editRecord.fieldsLc[field.nameLc].value = "";
                                    }
                                }
                                //
                                cp.core.doc.addRefreshQueryString("id", GenericController.encodeText(adminData.editRecord.id));
                            }
                            break;
                    }
                    adminData.AdminForm = adminData.AdminSourceForm;
                    //
                    // convert so action can be used in as a refresh
                    adminData.Admin_Action = Constants.AdminActionNop;
                }
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
        }
        //
        //========================================================================
        // Read and save a GetForm_InputCheckList
        //   see GetForm_InputCheckList for an explaination of the input
        //========================================================================
        //
        private void SaveMemberRules(CPClass cp, int PeopleID) {
            try {
                //
                int GroupCount = 0;
                int GroupPointer = 0;
                int GroupID = 0;
                bool RuleNeeded = false;
                DateTime DateExpires = default(DateTime);
                object DateExpiresVariant = null;
                bool RuleActive = false;
                DateTime RuleDateExpires = default(DateTime);
                int MemberRuleID = 0;
                //
                // --- create MemberRule records for all selected
                //
                GroupCount = cp.core.docProperties.getInteger("MemberRules.RowCount");
                if (GroupCount > 0) {
                    for (GroupPointer = 0; GroupPointer < GroupCount; GroupPointer++) {
                        //
                        // ----- Read Response
                        //
                        GroupID = cp.core.docProperties.getInteger("MemberRules." + GroupPointer + ".ID");
                        RuleNeeded = cp.core.docProperties.getBoolean("MemberRules." + GroupPointer);
                        DateExpires = cp.core.docProperties.getDate("MemberRules." + GroupPointer + ".DateExpires");
                        if (DateExpires == DateTime.MinValue) {
                            DateExpiresVariant = DBNull.Value;
                        } else {
                            DateExpiresVariant = DateExpires;
                        }
                        //
                        // ----- Update Record
                        //
                        using (var csData = new CsModel(cp.core)) {
                            csData.open("Member Rules", "(MemberID=" + PeopleID + ")and(GroupID=" + GroupID + ")", "", false, 0, "Active,MemberID,GroupID,DateExpires");
                            if (!csData.ok()) {
                                //
                                // No record exists
                                //
                                if (RuleNeeded) {
                                    //
                                    // No record, Rule needed, add it
                                    //
                                    csData.insert("Member Rules");
                                    if (csData.ok()) {
                                        csData.set("Active", true);
                                        csData.set("MemberID", PeopleID);
                                        csData.set("GroupID", GroupID);
                                        csData.set("DateExpires", DateExpires);
                                    }
                                }
                            } else {
                                //
                                // Record exists
                                //
                                if (RuleNeeded) {
                                    //
                                    // record exists, and it is needed, update the DateExpires if changed
                                    //
                                    RuleActive = csData.getBoolean("active");
                                    RuleDateExpires = csData.getDate("DateExpires");
                                    if ((!RuleActive) || (RuleDateExpires != DateExpires)) {
                                        csData.set("Active", true);
                                        csData.set("DateExpires", DateExpires);
                                    }
                                } else {
                                    //
                                    // record exists and it is not needed, delete it
                                    //
                                    MemberRuleID = csData.getInteger("ID");
                                    cp.core.db.deleteTableRecord(MemberRuleID, "ccMemberRules");
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
        }
        //
        //=============================================================================
        // Create a child content
        //=============================================================================
        //
        private string GetContentChildTool(CPClass cp) {
            string result = "";
            try {
                //
                bool IsEmptyList = false;
                int ParentContentID = 0;
                //string ParentContentName = null;
                string ChildContentName = "";
                int ChildContentID = 0;
                bool AddAdminMenuEntry = false;
                StringBuilderLegacyController Content = new StringBuilderLegacyController();
                string FieldValue = null;
                bool NewGroup = false;
                int GroupID = 0;
                string NewGroupName = "";
                string Button = null;
                string Caption = null;
                string Description = "";
                string ButtonList = "";
                bool BlockForm = false;
                //
                Button = cp.core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    //
                    //
                    //
                    return cp.core.webServer.redirect("/" + cp.core.appConfig.adminRoute, "GetContentChildTool, Cancel Button Pressed");
                } else if (!cp.core.session.isAuthenticatedAdmin(cp.core)) {
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
                        ParentContentID = cp.core.docProperties.getInteger("ParentContentID");
                        if (ParentContentID == 0) {
                            ParentContentID = ContentMetadataModel.getContentId(cp.core, "Page Content");
                        }
                        AddAdminMenuEntry = true;
                        GroupID = 0;
                    } else {
                        //
                        // Process input
                        //
                        ParentContentID = cp.core.docProperties.getInteger("ParentContentID");
                        var parentContentMetadata = ContentMetadataModel.create(cp.core, ParentContentID);
                        ChildContentName = cp.core.docProperties.getText("ChildContentName");
                        AddAdminMenuEntry = cp.core.docProperties.getBoolean("AddAdminMenuEntry");
                        GroupID = cp.core.docProperties.getInteger("GroupID");
                        NewGroup = cp.core.docProperties.getBoolean("NewGroup");
                        NewGroupName = cp.core.docProperties.getText("NewGroupName");
                        //
                        if ((parentContentMetadata == null) || (string.IsNullOrEmpty(ChildContentName))) {
                            Processor.Controllers.ErrorController.addUserError(cp.core, "You must select a parent and provide a child name.");
                        } else {
                            //
                            // Create Definition
                            //
                            Description = Description + "<div>&nbsp;</div>"
                                + "<div>Creating content [" + ChildContentName + "] from [" + parentContentMetadata.name + "]</div>";
                            var childContentMetadata = parentContentMetadata.createContentChild(cp.core, ChildContentName, cp.core.session.user.id);

                            ChildContentID = ContentMetadataModel.getContentId(cp.core, ChildContentName);
                            //
                            // Create Group and Rule
                            //
                            if (NewGroup && (!string.IsNullOrEmpty(NewGroupName))) {
                                using (var csData = new CsModel(cp.core)) {
                                    csData.open("Groups", "name=" + DbController.encodeSQLText(NewGroupName));
                                    if (csData.ok()) {
                                        Description = Description + "<div>Group [" + NewGroupName + "] already exists, using existing group.</div>";
                                        GroupID = csData.getInteger("ID");
                                    } else {
                                        Description = Description + "<div>Creating new group [" + NewGroupName + "]</div>";
                                        csData.close();
                                        csData.insert("Groups");
                                        if (csData.ok()) {
                                            GroupID = csData.getInteger("ID");
                                            csData.set("Name", NewGroupName);
                                            csData.set("Caption", NewGroupName);
                                        }
                                    }
                                }
                            }
                            if (GroupID != 0) {
                                using (var csData = new CsModel(cp.core)) {
                                    csData.insert("Group Rules");
                                    if (csData.ok()) {
                                        Description = Description + "<div>Assigning group [" + MetadataController.getRecordName(cp.core, "Groups", GroupID) + "] to edit content [" + ChildContentName + "].</div>";
                                        csData.set("GroupID", GroupID);
                                        csData.set("ContentID", ChildContentID);
                                    }
                                }
                            }
                            //
                            // Add Admin Menu Entry
                            //
                            if (AddAdminMenuEntry) {
                                //
                                // Add Navigator entries
                                //
                                //                    cmc = cp.core.main_cs_getv()
                                //                    MenuContentName = Processor.Models.Db.NavigatorEntryModel.contentName
                                //                    SupportAddonID = cp.core.csv_IsContentFieldSupported(MenuContentName, "AddonID")
                                //                    SupportGuid = cp.core.csv_IsContentFieldSupported(MenuContentName, "ccGuid")
                                //                    CS = cp.core.app.csOpen(Processor.Models.Db.NavigatorEntryModel.contentName, "ContentID=" & ParentContentID)
                                //                    Do While cp.core.app.csv_IsCSOK(CS)
                                //                        ParentID = cp.core.app.csv_cs_getText(CS, "ID")
                                //                        ParentName = cp.core.app.csv_cs_getText(CS, "name")
                                //                        AdminOnly = csData.cs_getBoolean(CS, "AdminOnly")
                                //                        DeveloperOnly = csData.cs_getBoolean(CS, "DeveloperOnly")
                                //                        CSEntry = cp.core.app.csv_InsertCSRecord(MenuContentName)
                                //                        If cp.core.app.csv_IsCSOK(CSEntry) Then
                                //                            If ParentID = 0 Then
                                //                                Call cp.core.app.csv_SetCS(CSEntry, "ParentID", Null)
                                //                            Else
                                //                                Call cp.core.app.csv_SetCS(CSEntry, "ParentID", ParentID)
                                //                            End If
                                //                            Call cp.core.app.csv_SetCS(CSEntry, "ContentID", ChildContentID)
                                //                            Call cp.core.app.csv_SetCS(CSEntry, "name", ChildContentName)
                                //                            Call cp.core.app.csv_SetCS(CSEntry, "LinkPage", "")
                                //                            Call cp.core.app.csv_SetCS(CSEntry, "SortOrder", "")
                                //                            Call cp.core.app.csv_SetCS(CSEntry, "AdminOnly", AdminOnly)
                                //                            Call cp.core.app.csv_SetCS(CSEntry, "DeveloperOnly", DeveloperOnly)
                                //                            Call cp.core.app.csv_SetCS(CSEntry, "NewWindow", False)
                                //                            Call cp.core.app.csv_SetCS(CSEntry, "Active", True)
                                //                            If SupportAddonID Then
                                //                                Call cp.core.app.csv_SetCS(CSEntry, "AddonID", "")
                                //                            End If
                                //                            If SupportGuid Then
                                //                                GuidGenerator = New guidClass
                                //                                ccGuid = Guid.NewGuid.ToString()
                                //                                GuidGenerator = Nothing
                                //                                Call cp.core.app.csv_SetCS(CSEntry, "ccGuid", ccGuid)
                                //                            End If
                                //                        End If
                                //                        Call cp.core.app.csv_CloseCS(CSEntry)
                                //                        'Call cp.core.csv_VerifyNavigatorEntry2(ccGuid, menuNameSpace, MenuName, ChildContenName, "", "", AdminOnly, DeveloperOnly, False, True, Processor.Models.Db.NavigatorEntryModel.contentName, "")
                                //                        'Call cp.core.main_CreateAdminMenu(MenuName, ChildContentName, ChildContentName, "", ChildContentName, AdminOnly, DeveloperOnly, False)
                                //                        Description = Description _
                                //                            & "<div>Creating navigator entry for [" & ChildContentName & "] under entry [" & ParentName & "].</div>"
                                //                        cp.core.main_NextCSRecord (CS)
                                //                    Loop
                                //                    Call cp.core.app.closeCS(CS)
                                //
                                // Add Legacy Navigator Entries
                                //
                                // -- deprecated
                                //CS = csData.cs_open(Processor.Models.Db.NavigatorEntryModel.contentName, "ContentID=" & ParentContentID)
                                //Do While csData.cs_ok(CS)
                                //    MenuName = csData.cs_get(CS, "name")
                                //    AdminOnly = csData.cs_getBoolean(CS, "AdminOnly")
                                //    DeveloperOnly = csData.cs_getBoolean(CS, "DeveloperOnly")
                                //    If MenuName = "" Then
                                //        MenuName = "Site Content"
                                //    End If
                                //    Call Controllers.appBuilderController.admin_VerifyAdminMenu(cp.core, MenuName, ChildContentName, ChildContentName, "", ChildContentName, AdminOnly, DeveloperOnly, False)
                                //    Description = Description _
                                //        & "<div>Creating Legacy site menu for [" & ChildContentName & "] under entry [" & MenuName & "].</div>"
                                //    csData.cs_goNext(CS)
                                //Loop
                                //Call csData.cs_Close(CS)
                            }
                            //
                            Description = Description + "<div>&nbsp;</div>"
                                + "<div>Your new content is ready. <a href=\"?" + rnAdminForm + "=22\">Click here</a> to create another Content Definition, or hit [Cancel] to return to the main menu.</div>";
                            ButtonList = ButtonCancel;
                            BlockForm = true;
                        }
                        cp.core.clearMetaData();
                        cp.core.cache.invalidateAll();
                    }
                    //
                    // Get the form
                    //
                    if (!BlockForm) {
                        string tableBody = "";
                        //
                        FieldValue = "<select size=\"1\" name=\"ParentContentID\" ID=\"\"><option value=\"\">Select One</option>";
                        FieldValue = FieldValue + GetContentChildTool_Options(cp, 0, ParentContentID);
                        FieldValue = FieldValue + "</select>";
                        tableBody += AdminUIController.getEditRowLegacy(cp.core, FieldValue, "Parent Content Name", "", false, false, "");
                        //
                        FieldValue = HtmlController.inputText(cp.core, "ChildContentName", ChildContentName, 1, 40);
                        tableBody += AdminUIController.getEditRowLegacy(cp.core, FieldValue, "New Child Content Name", "", false, false, "");
                        //
                        FieldValue = ""
                            + cp.core.html.inputRadio("NewGroup", false.ToString(), NewGroup.ToString()) + cp.core.html.selectFromContent("GroupID", GroupID, "Groups", "", "", "", ref IsEmptyList) + "(Select a current group)"
                            + "<br>" + cp.core.html.inputRadio("NewGroup", true.ToString(), NewGroup.ToString()) + HtmlController.inputText(cp.core, "NewGroupName", NewGroupName) + "(Create a new group)";
                        tableBody += AdminUIController.getEditRowLegacy(cp.core, FieldValue, "Content Manager Group", "", false, false, "");
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
                result = AdminUIController.getBody(cp.core, Caption, ButtonList, "", false, false, Description, "", 0, Content.Text);
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
            return result;
        }
        //
        //=============================================================================
        // Create a child content
        //=============================================================================
        //
        private string GetContentChildTool_Options(CPClass cp, int ParentID, int DefaultValue) {
            string returnOptions = "";
            try {
                //
                string SQL = null;
                int RecordID = 0;
                string RecordName = null;
                //
                if (ParentID == 0) {
                    SQL = "select Name, ID from ccContent where ((ParentID<1)or(Parentid is null)) and (AllowContentChildTool<>0);";
                } else {
                    SQL = "select Name, ID from ccContent where ParentID=" + ParentID + " and (AllowContentChildTool<>0) and not (allowcontentchildtool is null);";
                }
                using (var csData = new CsModel(cp.core)) {
                    csData.openSql(SQL, "Default");
                    while (csData.ok()) {
                        RecordName = csData.getText("Name");
                        RecordID = csData.getInteger("ID");
                        if (RecordID == DefaultValue) {
                            returnOptions = returnOptions + "<option value=\"" + RecordID + "\" selected>" + csData.getText("name") + "</option>";
                        } else {
                            returnOptions = returnOptions + "<option value=\"" + RecordID + "\" >" + csData.getText("name") + "</option>";
                        }
                        returnOptions = returnOptions + GetContentChildTool_Options(cp, RecordID, DefaultValue);
                        csData.goNext();
                    }
                    csData.close();
                }
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
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
        private string GetForm_HouseKeepingControl(CPClass cp) {
            string tempGetForm_HouseKeepingControl = null;
            try {
                //
                StringBuilderLegacyController Content = new StringBuilderLegacyController();
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
                string ButtonList = "";
                string Description = null;
                //
                Button = cp.core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    //
                    //
                    //
                    return cp.core.webServer.redirect("/" + cp.core.appConfig.adminRoute, "HouseKeepingControl, Cancel Button Pressed");
                } else if (!cp.core.session.isAuthenticatedAdmin(cp.core)) {
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
                    ArchiveRecordAgeDays = (cp.core.siteProperties.getInteger("ArchiveRecordAgeDays", 0));
                    ArchiveTimeOfDay = cp.core.siteProperties.getText("ArchiveTimeOfDay", "12:00:00 AM");
                    ArchiveAllowFileClean = (cp.core.siteProperties.getBoolean("ArchiveAllowFileClean", false));
                    //ArchiveAllowLogClean = genericController.EncodeBoolean(cp.core.main_GetSiteProperty2("ArchiveAllowLogClean", False))

                    //
                    // Process Requests
                    //
                    switch (Button) {
                        case ButtonOK:
                        case ButtonSave:
                            //
                            ArchiveRecordAgeDays = cp.core.docProperties.getInteger("ArchiveRecordAgeDays");
                            cp.core.siteProperties.setProperty("ArchiveRecordAgeDays", GenericController.encodeText(ArchiveRecordAgeDays));
                            //
                            ArchiveTimeOfDay = cp.core.docProperties.getText("ArchiveTimeOfDay");
                            cp.core.siteProperties.setProperty("ArchiveTimeOfDay", ArchiveTimeOfDay);
                            //
                            ArchiveAllowFileClean = cp.core.docProperties.getBoolean("ArchiveAllowFileClean");
                            cp.core.siteProperties.setProperty("ArchiveAllowFileClean", GenericController.encodeText(ArchiveAllowFileClean));
                            break;
                    }
                    //
                    if (Button == ButtonOK) {
                        return cp.core.webServer.redirect("/" + cp.core.appConfig.adminRoute, "StaticPublishControl, OK Button Pressed");
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
                    using (var csData = new CsModel(cp.core)) {
                        csData.openSql(SQL, "Default");
                        if (csData.ok()) {
                            PagesTotal = csData.getInteger("Result");
                        }
                    }
                    tableBody += AdminUIController.getEditRowLegacy(cp.core, SpanClassAdminNormal + PagesTotal, "Visits Found", "", false, false, "");
                    //
                    // ----- Oldest Visit
                    //
                    Copy = "unknown";
                    AgeInDays = "unknown";
                    using (var csData = new CsModel(cp.core)) {
                        SQL = cp.core.db.getSQLSelect("ccVisits", "DateAdded", "", "ID", "", 1);
                        csData.openSql(SQL, "Default");
                        if (csData.ok()) {
                            DateValue = csData.getDate("DateAdded");
                            if (DateValue != DateTime.MinValue) {
                                Copy = GenericController.encodeText(DateValue);
                                AgeInDays = GenericController.encodeText(encodeInteger(Math.Floor(encodeNumber(cp.core.doc.profileStartTime - DateValue))));
                            }
                        }
                    }
                    tableBody += (AdminUIController.getEditRowLegacy(cp.core, SpanClassAdminNormal + Copy + " (" + AgeInDays + " days)", "Oldest Visit", "", false, false, ""));
                    //
                    // ----- Viewings Found
                    //
                    PagesTotal = 0;
                    SQL = "SELECT Count(ID) as result  FROM ccViewings;";
                    using (var csData = new CsModel(cp.core)) {
                        csData.openSql(SQL, "Default");
                        if (csData.ok()) {
                            PagesTotal = csData.getInteger("Result");
                        }
                        csData.close();
                    }
                    tableBody += (AdminUIController.getEditRowLegacy(cp.core, SpanClassAdminNormal + PagesTotal, "Viewings Found", "", false, false, ""));
                    //
                    tableBody += (HtmlController.tableRowStart() + "<td colspan=\"3\" class=\"ccPanel3D ccAdminEditSubHeader\"><b>Options</b>" + tableCellEnd + kmaEndTableRow);
                    //
                    Caption = "Archive Age";
                    Copy = HtmlController.inputText(cp.core, "ArchiveRecordAgeDays", ArchiveRecordAgeDays.ToString(), -1, 20) + "&nbsp;Number of days to keep visit records. 0 disables housekeeping.";
                    tableBody += (AdminUIController.getEditRowLegacy(cp.core, Copy, Caption));
                    //
                    Caption = "Housekeeping Time";
                    Copy = HtmlController.inputText(cp.core, "ArchiveTimeOfDay", ArchiveTimeOfDay, -1, 20) + "&nbsp;The time of day when record deleting should start.";
                    tableBody += (AdminUIController.getEditRowLegacy(cp.core, Copy, Caption));
                    //
                    Caption = "Purge Content Files";
                    Copy = HtmlController.checkbox("ArchiveAllowFileClean", ArchiveAllowFileClean) + "&nbsp;Delete Contensive content files with no associated database record.";
                    tableBody += (AdminUIController.getEditRowLegacy(cp.core, Copy, Caption));
                    //
                    Content.Add(AdminUIController.editTable(tableBody));
                    Content.Add(HtmlController.inputHidden(rnAdminSourceForm, AdminformHousekeepingControl));
                    ButtonList = ButtonCancel + ",Refresh," + ButtonSave + "," + ButtonOK;
                }
                //
                Caption = "Data Housekeeping Control";
                Description = "This tool is used to control the database record housekeeping process. This process deletes visit history records, so care should be taken before making any changes.";
                tempGetForm_HouseKeepingControl = AdminUIController.getBody(cp.core, Caption, ButtonList, "", false, false, Description, "", 0, Content.Text);
                //
                cp.core.html.addTitle(Caption);
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
            return tempGetForm_HouseKeepingControl;
        }
        //
        ////
        //========================================================================
        //
        //========================================================================
        //
        private void TurnOnLinkAlias(CPClass cp, bool UseContentWatchLink) {
            try {
                //
                string ErrorList = null;
                string linkAlias = null;
                //
                if (cp.core.doc.debug_iUserError != "") {
                    Processor.Controllers.ErrorController.addUserError(cp.core, "Existing pages could not be checked for Link Alias names because there was another error on this page. Correct this error, and turn Link Alias on again to rerun the verification.");
                } else {
                    using (var csData = new CsModel(cp.core)) {
                        csData.open("Page Content");
                        while (csData.ok()) {
                            //
                            // Add the link alias
                            //
                            linkAlias = csData.getText("LinkAlias");
                            if (!string.IsNullOrEmpty(linkAlias)) {
                                //
                                // Add the link alias
                                //
                                LinkAliasController.addLinkAlias(cp.core, linkAlias, csData.getInteger("ID"), "", false, true);
                            } else {
                                //
                                // Add the name
                                //
                                linkAlias = csData.getText("name");
                                if (!string.IsNullOrEmpty(linkAlias)) {
                                    LinkAliasController.addLinkAlias(cp.core, linkAlias, csData.getInteger("ID"), "", false, false);
                                }
                            }
                            //
                            csData.goNext();
                        }
                    }
                    if (cp.core.doc.debug_iUserError != "") {
                        //
                        // Throw out all the details of what happened, and add one simple error
                        //
                        ErrorList = Processor.Controllers.ErrorController.getUserError(cp.core);
                        ErrorList = GenericController.vbReplace(ErrorList, UserErrorHeadline, "", 1, 99, 1);
                        Processor.Controllers.ErrorController.addUserError(cp.core, "The following errors occurred while verifying Link Alias entries for your existing pages." + ErrorList);
                        //Call cp.core.htmldoc.main_AddUserError(ErrorList)
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
        }
        //
        //========================================================================
        // Page Content Settings Page
        //========================================================================
        //
        private string GetForm_BuildCollection(CPClass cp) {
            string tempGetForm_BuildCollection = null;
            try {
                //
                string Description = null;
                StringBuilderLegacyController Content = new StringBuilderLegacyController();
                string Button = null;
                //adminUIController Adminui = new adminUIController(cp.core);
                string ButtonList = null;
                bool AllowAutoLogin = false;
                string Copy = null;
                //
                Button = cp.core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    //
                    // Cancel just exits with no content
                    //
                    return tempGetForm_BuildCollection;
                } else if (!cp.core.session.isAuthenticatedAdmin(cp.core)) {
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
                    AllowAutoLogin = (cp.core.siteProperties.getBoolean("AllowAutoLogin", true));
                    //
                    // Process Requests
                    //
                    switch (Button) {
                        case ButtonSave:
                        case ButtonOK:
                            //
                            //
                            //
                            AllowAutoLogin = cp.core.docProperties.getBoolean("AllowAutoLogin");
                            //
                            cp.core.siteProperties.setProperty("AllowAutoLogin", GenericController.encodeText(AllowAutoLogin));
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
                    tableBody += (AdminUIController.getEditRowLegacy(cp.core, Copy, "Allow Auto Login", "", false, false, ""));
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
                tempGetForm_BuildCollection = AdminUIController.getBody(cp.core, "Security Settings", ButtonList, "", true, true, Description, "", 0, Content.Text);
                Content = null;
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
            }
            return tempGetForm_BuildCollection;
        }
        //
        //=================================================================================
        //
        //=================================================================================
        //
        public static void setIndexSQL_SaveIndexConfig(CPClass cp, CoreController core, IndexConfigClass IndexConfig) {
            //
            // --Find words
            string SubList = "";
            foreach (var kvp in IndexConfig.findWords) {
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
            if (IndexConfig.subCDefID > 0) {
                FilterText += "\r\nCDefList\r\n" + IndexConfig.subCDefID + "\r\n";
            }
            //
            // -- Group List
            SubList = "";
            if (IndexConfig.groupListCnt > 0) {
                //
                int Ptr = 0;
                for (Ptr = 0; Ptr < IndexConfig.groupListCnt; Ptr++) {
                    if (!string.IsNullOrEmpty(IndexConfig.groupList[Ptr])) {
                        SubList = SubList + "\r\n" + IndexConfig.groupList[Ptr];
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
                + "\r\n" + IndexConfig.pageNumber;
            FilterText += "\r\n"
                + "\r\nrecordsperpage"
                + "\r\n" + IndexConfig.recordsPerPage;
            //
            // misc filters
            if (IndexConfig.activeOnly) {
                FilterText += "\r\n"
                    + "\r\nIndexFilterActiveOnly";
            }
            if (IndexConfig.lastEditedByMe) {
                FilterText += "\r\n"
                    + "\r\nIndexFilterLastEditedByMe";
            }
            if (IndexConfig.lastEditedToday) {
                FilterText += "\r\n"
                    + "\r\nIndexFilterLastEditedToday";
            }
            if (IndexConfig.lastEditedPast7Days) {
                FilterText += "\r\n"
                    + "\r\nIndexFilterLastEditedPast7Days";
            }
            if (IndexConfig.lastEditedPast30Days) {
                FilterText += "\r\n"
                    + "\r\nIndexFilterLastEditedPast30Days";
            }
            if (IndexConfig.open) {
                FilterText += "\r\n"
                    + "\r\nIndexFilterOpen";
            }
            //
            cp.core.visitProperty.setProperty(AdminDataModel.IndexConfigPrefix + encodeText(IndexConfig.contentID), FilterText);
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
            foreach (var kvp in IndexConfig.sorts) {
                IndexConfigClass.IndexConfigSortClass sort = kvp.Value;
                if (!string.IsNullOrEmpty(sort.fieldName)) {
                    SubList = SubList + "\r\n" + sort.fieldName + "\t" + sort.direction;
                }
            }
            if (!string.IsNullOrEmpty(SubList)) {
                FilterText += "\r\nSorts" + SubList + "\r\n";
            }
            cp.core.userProperty.setProperty(AdminDataModel.IndexConfigPrefix + encodeText(IndexConfig.contentID), FilterText);
            //

        }
    }
}
