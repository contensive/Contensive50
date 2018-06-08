
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
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
using Contensive.BaseClasses;
//
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// build page content system. Persistence is the docController.
    /// </summary>
    public class pageContentController : IDisposable {
        /// <summary>
        /// Constructor
        /// </summary>
        public pageContentController() {
            page = new pageContentModel();
            pageToRootList = new List<pageContentModel>();
            template = new pageTemplateModel();
        }
        /// <summary>
        /// current page to it's root
        /// </summary>
        public List<Models.DbModels.pageContentModel> pageToRootList { get; set; }
        /// <summary>
        /// current template
        /// </summary>
        public pageTemplateModel template { get; set; }
        /// <summary>
        /// the reason the current template was selected, displayed only as comment for debug mode
        /// </summary>
        public string templateReason { get; set; } = "";
        /// <summary>
        /// current page for website documents, blank for others
        /// </summary>
        public pageContentModel page { get; set; }
        //
        //========================================================================
        //
        public static string getHtmlBodyTemplate(coreController core) {
            string returnBody = "";
            try {
                //
                // -- OnBodyStart add-ons
                foreach (addonModel addon in core.addonCache.getOnBodyStartAddonList()) {
                    CPUtilsBaseClass.addonExecuteContext bodyStartContext = new CPUtilsBaseClass.addonExecuteContext() {
                        addonType = CPUtilsBaseClass.addonContext.ContextOnBodyStart,
                        errorContextMessage = "calling onBodyStart addon [" + addon.name + "] in HtmlBodyTemplate"
                    };
                    returnBody += core.addon.execute(addon, bodyStartContext);
                }
                //
                // ----- main_Get Content (Already Encoded)
                //
                bool blockSiteWithLogin = false;
                string PageContent = getHtmlBodyTemplateContent(core, true, true, false, ref blockSiteWithLogin);
                if (blockSiteWithLogin) {
                    //
                    // section blocked, just return the login box in the page content
                    //
                    returnBody = ""
                        + "\r<div class=\"ccLoginPageCon\">"
                        + genericController.nop(PageContent) + "\r</div>"
                        + "";
                } else if (!core.doc.continueProcessing) {
                    //
                    // exit if stream closed during main_GetSectionpage
                    //
                    returnBody = "";
                } else {
                    string LocalTemplateBody = core.doc.pageController.template.bodyHTML;
                    if (string.IsNullOrEmpty(LocalTemplateBody)) {
                        LocalTemplateBody = TemplateDefaultBody;
                    }
                    //
                    // -- no section block, continue
                    string LocalTemplateName = core.doc.pageController.template.name;
                    int LocalTemplateID = core.doc.pageController.template.id;
                    if (string.IsNullOrEmpty(LocalTemplateName)) {
                        LocalTemplateName = "Template " + LocalTemplateID;
                    }
                    //
                    // ----- Encode Template
                    returnBody += activeContentController.renderHtmlForWeb(core, LocalTemplateBody, "Page Templates", LocalTemplateID, 0, core.webServer.requestProtocol + core.webServer.requestDomain, core.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextTemplate);
                    //
                    //
                    if (returnBody.IndexOf(fpoContentBox)  != -1) {
                        returnBody = genericController.vbReplace(returnBody, fpoContentBox, PageContent);
                    } else {
                        // If Content was not found, add it to the end
                        returnBody = returnBody + PageContent;
                    }
                    //
                    // ----- Add tools Panel
                    if (!core.session.isAuthenticated) {
                        //
                        // not logged in
                    } else {
                        //
                        // Add template editing
                        if (core.visitProperty.getBoolean("AllowAdvancedEditor") & core.session.isEditing("Page Templates")) {
                            returnBody = adminUIController.getEditWrapper( core, "Page Template [" + LocalTemplateName + "]", adminUIController.getRecordEditLink(core, "Page Templates", LocalTemplateID, false, LocalTemplateName, core.session.isEditing("Page Templates")) + returnBody);
                        }
                    }
                    //
                    // ----- OnBodyEnd add-ons
                    foreach (var addon in core.addonCache.getOnBodyEndAddonList()) {
                        CPUtilsBaseClass.addonExecuteContext bodyEndContext = new CPUtilsBaseClass.addonExecuteContext() {
                            addonType = CPUtilsBaseClass.addonContext.ContextFilter,
                            errorContextMessage = "calling onBodyEnd addon [" + addon.name + "] in HtmlBodyTemplate"
                        };
                        core.doc.docBodyFilter = returnBody;
                        string AddonReturn = core.addon.execute(addon, bodyEndContext);
                        returnBody = core.doc.docBodyFilter + AddonReturn;
                    }
               }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return returnBody;
        }
        //
        //=============================================================================
        //
        public static string getHtmlBodyTemplateContent(coreController core, bool AllowChildPageList, bool AllowReturnLink, bool AllowEditWrapper, ref bool return_blockSiteWithLogin) {
            string returnHtml = "";
            try {
                //
                // -- validate domain
                if (core.doc.domain == null) {
                    //
                    // -- domain not listed, this is now an error
                    logController.logError(core, "Domain not recognized:" + core.webServer.requestUrlSource);
                    return "<div style=\"width:300px; margin: 100px auto auto auto;text-align:center;\">The domain name is not configured for this site.</div>";
                }
                //
                // -- validate page
                if (core.doc.pageController.page.id == 0) {
                    //
                    // -- landing page is not valid -- display error
                    logController.logInfo(core, "Requested page/document not found:" + core.webServer.requestUrlSource);
                    core.doc.redirectBecausePageNotFound = true;
                    core.doc.redirectReason = "Redirecting because the page selected could not be found.";
                    core.doc.redirectLink = pageContentController.main_ProcessPageNotFound_GetLink(core, core.doc.redirectReason, "", "", 0, 0);
                    return core.webServer.redirect(core.doc.redirectLink, core.doc.redirectReason, core.doc.redirectBecausePageNotFound);
                }
                //
                // -- get contentbox
                returnHtml = getContentBox(core, "", AllowChildPageList, AllowReturnLink, false, 0, core.siteProperties.useContentWatchLink, false);
                if (core.doc.redirectLink == "" && (returnHtml.IndexOf(html_quickEdit_fpo)  != -1)) {
                    int FieldRows = genericController.encodeInteger(core.userProperty.getText("Page Content.copyFilename.PixelHeight", "500"));
                    if (FieldRows < 50) {
                        FieldRows = 50;
                        core.userProperty.setProperty("Page Content.copyFilename.PixelHeight", 50);
                    }
                    //
                    // -- if fpo_QuickEdit it there, replace it out
                    string addonListJSON = core.html.getWysiwygAddonList(contentTypeEnum.contentTypeWeb);
                    string Editor = core.html.getFormInputHTML("copyFilename", core.doc.quickEditCopy, FieldRows.ToString(), "100%", false, true, addonListJSON, "", "");
                    returnHtml = genericController.vbReplace(returnHtml, html_quickEdit_fpo, Editor);
                }
                //
                // -- Add admin warning to the top of the content
                if (core.session.isAuthenticatedAdmin(core) & core.doc.adminWarning != "") {
                    //
                    // Display Admin Warnings with Edits for record errors
                    if (core.doc.adminWarningPageID != 0) {
                        core.doc.adminWarning = core.doc.adminWarning + "</p>" + adminUIController.getRecordEditLink(core, "Page Content", core.doc.adminWarningPageID, true, "Page " + core.doc.adminWarningPageID, core.session.isAuthenticatedAdmin(core)) + "&nbsp;Edit the page<p>";
                        core.doc.adminWarningPageID = 0;
                    }
                    //
                    returnHtml = ""
                    + core.html.getAdminHintWrapper(core.doc.adminWarning) + returnHtml + "";
                    core.doc.adminWarning = "";
                }
                //
                // -- handle redirect and edit wrapper
                if (core.doc.redirectLink != "") {
                    return core.webServer.redirect(core.doc.redirectLink, core.doc.redirectReason, core.doc.redirectBecausePageNotFound);
                } else if (AllowEditWrapper) {
                    returnHtml = adminUIController.getEditWrapper( core, "Page Content", returnHtml);
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return returnHtml;
        }
        //
        //=============================================================================
        //   Add content padding around content
        //
        internal static string getContentBoxWrapper(coreController core, string Content, int ContentPadding) {
            string result = Content;
            if (core.siteProperties.getBoolean("Compatibility ContentBox Pad With Table")) {
                //
                result = "<div class=\"contentBox\">" + result + "</div>";
                if (ContentPadding > 0) {
                    result = ""
                        + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0>"
                        + "<tr>"
                        + "<td style=\"padding:" + ContentPadding + "px\">" + result + "</td>"
                        + "</tr>"
                        + "</table>";
                }
            } else {
                result = "<div class=\"contentBox\" style=\"padding:" + ContentPadding + "px\">" + result + "</div>";
            }
            return result;
        }
        //
        //
        //
        internal static string getDefaultBlockMessage(coreController core, bool UseContentWatchLink) {
            string result = "";
            try {
                var copyRecord = copyContentModel.createByName(core, ContentBlockCopyName);  
                if (copyRecord!=null) {
                    result = copyRecord.copy;
                }
                //int CS = core.db.csOpen("Copy Content", "name=" + core.db.encodeSQLText(ContentBlockCopyName), "ID", false, 0, false, false, "Copy,ID");
                //if (core.db.csOk(CS)) {
                //    result = core.db.csGet(CS, "Copy");
                //}
                //core.db.csClose(ref CS);
                //
                // ----- Do not allow blank message - if still nothing, create default
                if (string.IsNullOrEmpty(result)) {
                    result = "<p>The content on this page has restricted access. If you have a username and password for this system, <a href=\"?method=login\" rel=\"nofollow\">Click Here</a>. For more information, please contact the administrator.</p>";
                    copyRecord = copyContentModel.add(core);
                    copyRecord.name = ContentBlockCopyName;
                    copyRecord.copy = result;
                    copyRecord.save(core);
                    ////
                    //// ----- Create Copy Content Record for future
                    //CS = core.db.csInsertRecord("Copy Content");
                    //if (core.db.csOk(CS)) {
                    //    core.db.csSet(CS, "Name", ContentBlockCopyName);
                    //    core.db.csSet(CS, "Copy", result);
                    //}
                    //core.db.csClose(ref CS);
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //
        //
        public static string getMoreInfoHtml(coreController core, int PeopleID) {
            string result = "";
            try {
                string Copy = "";
                int CS = core.db.csOpenContentRecord("People", PeopleID, 0, false, false, "Name,Phone,Email");
                if (core.db.csOk(CS)) {
                    string ContactName = core.db.csGetText(CS, "Name");
                    string ContactPhone = core.db.csGetText(CS, "Phone");
                    string ContactEmail = core.db.csGetText(CS, "Email");
                    if (!string.IsNullOrEmpty(ContactName)) {
                        Copy += "For more information, please contact " + ContactName;
                        if (string.IsNullOrEmpty(ContactEmail)) {
                            if (!string.IsNullOrEmpty(ContactPhone)) {
                                Copy += " by phone at " + ContactPhone;
                            }
                        } else {
                            Copy += " by <A href=\"mailto:" + ContactEmail + "\">email</A>";
                            if (!string.IsNullOrEmpty(ContactPhone)) {
                                Copy += " or by phone at " + ContactPhone;
                            }
                        }
                    } else {
                        if (string.IsNullOrEmpty(ContactEmail)) {
                            if (!string.IsNullOrEmpty(ContactPhone)) {
                                Copy += "For more information, please call " + ContactPhone;
                            }
                        } else {
                            Copy += "For more information, please <A href=\"mailto:" + ContactEmail + "\">email</A>";
                            if (!string.IsNullOrEmpty(ContactPhone)) {
                                Copy += ", or call " + ContactPhone;
                            }
                        }
                    }
                }
                core.db.csClose(ref CS);
                result = Copy;
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //
        //
        internal static string get2ColumnTableRow(string Caption, string Result, bool EvenRow) {
            string CopyCaption = Caption;
            if (string.IsNullOrEmpty(CopyCaption)) {
                CopyCaption = "&nbsp;";
            }
            string CopyResult = Result;
            if (string.IsNullOrEmpty(CopyResult)) {
                CopyResult = "&nbsp;";
            }
            return "<tr>" 
                + htmlController.td("<nobr>" + CopyCaption + "</nobr>", "150", 0, EvenRow, "right") 
                + htmlController.td(CopyResult, "100%", 0, EvenRow, "left") 
                + kmaEndTableRow;
        }
        //
        public static void loadPage(coreController core, int requestedPageId, domainModel domain) {
            try {
                if (domain == null) {
                    //
                    // -- domain is not valid
                    logController.handleError( core,new ApplicationException("Page could not be determined because the domain was not recognized."));
                } else {
                    //
                    // -- attempt requested page
                    pageContentModel requestedPage = null;
                    if (!requestedPageId.Equals(0)) {
                        requestedPage = pageContentModel.create(core, requestedPageId);
                        if (requestedPage == null) {
                            //
                            // -- requested page not found
                            requestedPage = pageContentModel.create(core, getPageNotFoundPageId(core));
                        }
                    }
                    if (requestedPage == null) {
                        //
                        // -- use the Landing Page
                        requestedPage = getLandingPage(core, domain);
                    }
                    core.doc.addRefreshQueryString(rnPageId, encodeText(requestedPage.id));
                    //
                    // -- build parentpageList (first = current page, last = root)
                    // -- add a 0, then repeat until another 0 is found, or there is a repeat
                    core.doc.pageController.pageToRootList = new List<Models.DbModels.pageContentModel>();
                    List<int> usedPageIdList = new List<int>();
                    int targetPageId = requestedPage.id;
                    usedPageIdList.Add(0);
                    while (!usedPageIdList.Contains(targetPageId)) {
                        usedPageIdList.Add(targetPageId);
                        pageContentModel targetpage = pageContentModel.create(core, targetPageId );
                        if (targetpage == null) {
                            break;
                        } else {
                            core.doc.pageController.pageToRootList.Add(targetpage);
                            targetPageId = targetpage.ParentID;
                        }
                    }
                    if (core.doc.pageController.pageToRootList.Count == 0) {
                        //
                        // -- attempt failed, create default page
                        core.doc.pageController.page = pageContentModel.add(core);
                        core.doc.pageController.page.name = DefaultNewLandingPageName + ", " + domain.name;
                        core.doc.pageController.page.Copyfilename.content = landingPageDefaultHtml;
                        core.doc.pageController.page.save(core);
                        core.doc.pageController.pageToRootList.Add(core.doc.pageController.page);
                    } else {
                        core.doc.pageController.page = core.doc.pageController.pageToRootList.First();
                    }
                    //
                    // -- get template from pages
                    core.doc.pageController.template = null;
                    foreach (Models.DbModels.pageContentModel page in core.doc.pageController.pageToRootList) {
                        if (page.TemplateID > 0) {
                            core.doc.pageController.template = pageTemplateModel.create(core, page.TemplateID );
                            if (core.doc.pageController.template == null) {
                                //
                                // -- templateId is not valid
                                page.TemplateID = 0;
                                page.save(core);
                            } else {
                                if (page == core.doc.pageController.pageToRootList.First()) {
                                    core.doc.pageController.templateReason = "This template was used because it is selected by the current page.";
                                } else {
                                    core.doc.pageController.templateReason = "This template was used because it is selected one of this page's parents [" + page.name + "].";
                                }
                                break;
                            }
                        }
                    }
                    //
                    if (core.doc.pageController.template == null) {
                        //
                        // -- get template from domain
                        if (domain != null) {
                            if (domain.defaultTemplateId > 0) {
                                core.doc.pageController.template = pageTemplateModel.create(core, domain.defaultTemplateId );
                                if (core.doc.pageController.template == null) {
                                    //
                                    // -- domain templateId is not valid
                                    domain.defaultTemplateId = 0;
                                    domain.save(core);
                                }
                            }
                        }
                        if (core.doc.pageController.template == null) {
                            //
                            // -- get template named Default
                            core.doc.pageController.template = pageTemplateModel.createByName(core, defaultTemplateName);
                            if (core.doc.pageController.template == null) {
                                //
                                // -- ceate new template named Default
                                core.doc.pageController.template = pageTemplateModel.add(core );
                                core.doc.pageController.template.name = defaultTemplateName;
                                core.doc.pageController.template.bodyHTML = core.appRootFiles.readFileText(defaultTemplateHomeFilename);
                                core.doc.pageController.template.save(core);
                            }
                            //
                            // -- set this new template to all domains without a template
                            foreach (domainModel d in domainModel.createList(core, "((DefaultTemplateId=0)or(DefaultTemplateId is null))")) {
                                d.defaultTemplateId = core.doc.pageController.template.id;
                                d.save(core);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
        }
        //
        public static int getPageNotFoundPageId(coreController core) {
            int pageId = core.domain.pageNotFoundPageId;
            if (pageId == 0) {
                //
                // no domain page not found, use site default
                pageId = core.siteProperties.getInteger("PageNotFoundPageID", 0);
            }
            return pageId;
        }
        //
        //---------------------------------------------------------------------------
        //
        public static pageContentModel getLandingPage(coreController core, domainModel domain) {
            pageContentModel landingPage = null;
            try {
                if (domain == null) {
                    //
                    // -- domain not available
                    logController.handleError( core,new ApplicationException("Landing page could not be determined because the domain was not recognized."));
                } else {
                    //
                    // -- attempt domain landing page
                    if (!domain.rootPageId.Equals(0)) {
                        landingPage = pageContentModel.create(core, domain.rootPageId);
                        if (landingPage == null) {
                            domain.rootPageId = 0;
                            domain.save(core);
                        }
                    }
                    if (landingPage == null) {
                        //
                        // -- attempt site landing page
                        int siteLandingPageID = core.siteProperties.getInteger("LandingPageID", 0);
                        if (!siteLandingPageID.Equals(0)) {
                            landingPage = pageContentModel.create(core, siteLandingPageID);
                            if (landingPage == null) {
                                core.siteProperties.setProperty("LandingPageID", 0);
                                domain.rootPageId = 0;
                                domain.save(core);
                            }
                        }
                        if (landingPage == null) {
                            //
                            // -- create detault landing page
                            landingPage = pageContentModel.add(core);
                            landingPage.name = DefaultNewLandingPageName + ", " + domain.name;
                            landingPage.Copyfilename.content = landingPageDefaultHtml;
                            landingPage.save(core);
                            core.doc.landingPageID = landingPage.id;
                        }
                        //
                        // -- save new page to the domain
                        domain.rootPageId = landingPage.id;
                        domain.save(core);
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return landingPage;
        }
        //
        // Verify a link from the template link field to be used as a Template Link
        //
        internal static string verifyTemplateLink(coreController core, string linkSrc) {
            string result = null;
            result = linkSrc;
            if (!string.IsNullOrEmpty(result)) {
                if (genericController.vbInstr(1, result, "://") != 0) {
                    //
                    // protocol provided, do not fixup
                    result = genericController.encodeVirtualPath(result, core.appConfig.cdnFileUrl, appRootPath, core.webServer.requestDomain);
                } else {
                    //
                    // no protocol, convert to short link
                    if (result.Left( 1) != "/") {
                        //
                        // page entered without path, assume it is in root path
                        result = "/" + result;
                    }
                    result = genericController.ConvertLinkToShortLink(result, core.webServer.requestDomain, core.appConfig.cdnFileUrl);
                    result = genericController.encodeVirtualPath(result, core.appConfig.cdnFileUrl, appRootPath, core.webServer.requestDomain);
                }
            }
            return result;
        }
        //
        //
        //
        internal static string main_ProcessPageNotFound_GetLink(coreController core, string adminMessage, string BackupPageNotFoundLink = "", string PageNotFoundLink = "", int EditPageID = 0, int EditSectionID = 0) {
            string result = "";
            try {
                int PageNotFoundPageID = getPageNotFoundPageId(core);
                string Link = null;
                if (PageNotFoundPageID == 0) {
                    //
                    // No PageNotFound was set -- use the backup link
                    if (string.IsNullOrEmpty(BackupPageNotFoundLink)) {
                        adminMessage = adminMessage + " The Site Property 'PageNotFoundPageID' is not set so the Landing Page was used.";
                        Link = core.doc.landingLink;
                    } else {
                        adminMessage = adminMessage + " The Site Property 'PageNotFoundPageID' is not set.";
                        Link = BackupPageNotFoundLink;
                    }
                } else {
                    //
                    // Set link
                    Link = getPageLink(core, PageNotFoundPageID, "", true, false);
                    string DefaultLink = getPageLink(core, 0, "", true, false);
                    if (Link != DefaultLink) {
                    } else {
                        adminMessage = adminMessage + "</p><p>The current 'Page Not Found' could not be used. It is not valid, or it is not associated with a valid site section. To configure a valid 'Page Not Found' page, first create the page as a child page on your site and check the 'Page Not Found' checkbox on it's control tab. The Landing Page was used.";
                    }
                }
                //
                // Add the Admin Message to the link
                if (core.session.isAuthenticatedAdmin(core)) {
                    if (string.IsNullOrEmpty(PageNotFoundLink)) {
                        PageNotFoundLink = core.webServer.requestUrl;
                    }
                    //
                    // Add the Link to the Admin Msg
                    adminMessage = adminMessage + "<p>The URL was " + PageNotFoundLink + ".";
                    //
                    // Add the Referrer to the Admin Msg
                    if (core.webServer.requestReferer != "") {
                        int Pos = genericController.vbInstr(1, core.webServer.requestReferrer, "main_AdminWarningPageID=", 1);
                        if (Pos != 0) {
                            core.webServer.requestReferrer = core.webServer.requestReferrer.Left( Pos - 2);
                        }
                        Pos = genericController.vbInstr(1, core.webServer.requestReferrer, "main_AdminWarningMsg=", 1);
                        if (Pos != 0) {
                            core.webServer.requestReferrer = core.webServer.requestReferrer.Left( Pos - 2);
                        }
                        Pos = genericController.vbInstr(1, core.webServer.requestReferrer, "blockcontenttracking=", 1);
                        if (Pos != 0) {
                            core.webServer.requestReferrer = core.webServer.requestReferrer.Left( Pos - 2);
                        }
                        adminMessage = adminMessage + " The referring page was " + core.webServer.requestReferrer + ".";
                    }
                    //
                    adminMessage = adminMessage + "</p>";
                    //
                    if (EditPageID != 0) {
                        Link = genericController.modifyLinkQuery(Link, "main_AdminWarningPageID", EditPageID.ToString(), true);
                    }
                    //
                    if (EditSectionID != 0) {
                        Link = genericController.modifyLinkQuery(Link, "main_AdminWarningSectionID", EditSectionID.ToString(), true);
                    }
                    //
                    Link = genericController.modifyLinkQuery(Link, RequestNameBlockContentTracking, "1", true);
                    Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "<p>" + adminMessage + "</p>", true);
                }
                //
                result = Link;
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public static string getPageLink(coreController core, int PageID, string QueryStringSuffix, bool AllowLinkAliasIfEnabled = true, bool UseContentWatchNotDefaultPage = false) {
            string result = "";
            try {
                //
                // -- set linkPathPath to linkAlias
                string linkPathPage = null;
                if (AllowLinkAliasIfEnabled && core.siteProperties.allowLinkAlias) {
                    linkPathPage = linkAliasController.getLinkAlias(core, PageID, QueryStringSuffix, "");
                }
                if (string.IsNullOrEmpty(linkPathPage)) {
                    //
                    // -- if not linkAlis, set default page and qs
                    linkPathPage = core.siteProperties.serverPageDefault;
                    if (string.IsNullOrEmpty(linkPathPage)) {
                        linkPathPage = "/" + getDefaultScript();
                    } else {
                        int Pos = genericController.vbInstr(1, linkPathPage, "?");
                        if (Pos != 0) {
                            linkPathPage = linkPathPage.Left( Pos - 1);
                        }
                        if (linkPathPage.Left( 1) != "/") {
                            linkPathPage = "/" + linkPathPage;
                        }
                    }
                    //
                    // -- calc linkQS (cleared in come cases later)
                    linkPathPage += "?" + rnPageId + "=" + PageID;
                    if (!string.IsNullOrEmpty(QueryStringSuffix)) {
                        linkPathPage += "&" + QueryStringSuffix;
                    }
                }
                //
                // -- domain -- determine if the domain has any template requirements, and if so, is this template allowed
                string SqlCriteria = "(domainId=" + core.doc.domain.id + ")";
                List<Models.DbModels.TemplateDomainRuleModel> allowTemplateRuleList = TemplateDomainRuleModel.createList(core, SqlCriteria);
                bool templateAllowed = false;
                foreach (TemplateDomainRuleModel rule in allowTemplateRuleList) {
                    if (rule.templateId == core.doc.pageController.template.id) {
                        templateAllowed = true;
                        break;
                    }
                }
                string linkDomain = "";
                if (allowTemplateRuleList.Count == 0) {
                    //
                    // this template has no domain preference, use current domain
                    //
                    linkDomain = core.webServer.requestDomain;
                } else if (core.domain.id == 0) {
                    //
                    // the current domain is not recognized, or is default - use it
                    //
                    linkDomain = core.webServer.requestDomain;
                } else if (templateAllowed) {
                    //
                    // current domain is in the allowed domain list
                    //
                    linkDomain = core.webServer.requestDomain;
                } else {
                    //
                    // there is an allowed domain list and current domain is not on it, or use first
                    //
                    int setdomainId = allowTemplateRuleList.First().domainId;
                    linkDomain = core.db.getRecordName("domains", setdomainId);
                    if (string.IsNullOrEmpty(linkDomain)) {
                        linkDomain = core.webServer.requestDomain;
                    }
                }
                //
                // -- protocol
                string linkprotocol = "";
                if (core.doc.pageController.page.IsSecure | core.doc.pageController.template.isSecure) {
                    linkprotocol = "https://";
                } else {
                    linkprotocol = "http://";
                }
                //
                // -- assemble
                result = linkprotocol + linkDomain + linkPathPage;
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        // main_Get a page link if you know nothing about the page
        //   If you already have all the info, lik the parents templateid, etc, call the ...WithArgs call
        //====================================================================================================
        //
        public static string main_GetPageLink3(coreController core, int PageID, string QueryStringSuffix, bool AllowLinkAlias) {
            return getPageLink(core, PageID, QueryStringSuffix, AllowLinkAlias, false);
        }
        //
        //
        internal static string getDefaultScript() { return "default.aspx"; }
        //
        //========================================================================
        //   main_IsChildRecord
        //
        //   Tests if this record is in the ParentID->ID chain for this content
        //========================================================================
        //
        public static bool isChildRecord(coreController core, string ContentName, int ChildRecordID, int ParentRecordID) {
            bool result = false;
            try {
                result = (ChildRecordID == ParentRecordID);
                if (!result) {
                    Models.Complex.cdefModel CDef = Models.Complex.cdefModel.getCdef(core, ContentName);
                    if (genericController.isInDelimitedString(CDef.selectCommaList.ToUpper(), "PARENTID", ",")) {
                        result = main_IsChildRecord_Recurse(core, CDef.contentDataSourceName, CDef.contentTableName, ChildRecordID, ParentRecordID, "");
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //========================================================================
        //   main_IsChildRecord
        //
        //   Tests if this record is in the ParentID->ID chain for this content
        //========================================================================
        //
        internal static bool main_IsChildRecord_Recurse(coreController core, string DataSourceName, string TableName, int ChildRecordID, int ParentRecordID, string History) {
            bool result = false;
            try {
                string SQL ="select ParentID from " + TableName + " where id=" + ChildRecordID;
                int CS = core.db.csOpenSql(SQL);
                int ChildRecordParentID = 0;
                if (core.db.csOk(CS)) {
                    ChildRecordParentID = core.db.csGetInteger(CS, "ParentID");
                }
                core.db.csClose(ref CS);
                if ((ChildRecordParentID != 0) & (!genericController.isInDelimitedString(History, ChildRecordID.ToString(), ","))) {
                    result = (ParentRecordID == ChildRecordParentID);
                    if (!result) {
                        result = main_IsChildRecord_Recurse(core, DataSourceName, TableName, ChildRecordParentID, ParentRecordID, History + "," + ChildRecordID.ToString());
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //
        //
        //========================================================================
        //   Returns the entire HTML page based on the bid/sid stream values
        //
        //   This code is based on the GoMethod site script
        //========================================================================
        //
        public static string getHtmlBody(coreController core) {
            string result = "";
            try {
                bool IsPageNotFound = false;
                //
                if (core.doc.continueProcessing) {
                    //
                    // -- setup domain
                    string domainTest = core.webServer.requestDomain.Trim().ToLower().Replace("..", ".");
                    core.doc.domain = null;
                    if (!string.IsNullOrEmpty(domainTest)) {
                        int posDot = 0;
                        int loopCnt = 10;
                        do {
                            core.doc.domain = domainModel.createByName(core, domainTest);
                            posDot = domainTest.IndexOf('.');
                            if ((posDot >= 0) && (domainTest.Length > 1)) {
                                domainTest = domainTest.Substring(posDot + 1);
                            }
                            loopCnt -= 1;
                        } while ((core.doc.domain == null) && (posDot >= 0) && (loopCnt > 0));
                    }


                    if (core.doc.domain == null) {

                    }
                    //
                    // -- load requested page/template
                    pageContentController.loadPage(core, core.docProperties.getInteger(rnPageId), core.doc.domain);
                    //
                    // -- execute context for included addons
                    CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                        addonType = CPUtilsBaseClass.addonContext.ContextSimple,
                        cssContainerClass = "",
                        cssContainerId = "",
                        hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                            contentName = pageContentModel.contentName,
                            fieldName = "copyfilename",
                            recordId = core.doc.pageController.page.id
                        },
                        isIncludeAddon = false,
                        personalizationAuthenticated = core.session.visit.VisitAuthenticated,
                        personalizationPeopleId = core.session.user.id
                    };

                    //
                    // -- execute template Dependencies
                    List<Models.DbModels.addonModel> templateAddonList = addonModel.createList_templateDependencies(core, core.doc.pageController.template.id);
                    if (templateAddonList.Count > 0) {
                        string addonContextMessage = executeContext.errorContextMessage;
                        foreach (addonModel addon in templateAddonList) {
                            executeContext.errorContextMessage = "executing template dependency [" + addon.name + "]";
                            result += core.addon.executeDependency(addon, executeContext);
                        }
                        executeContext.errorContextMessage = addonContextMessage;
                    }
                    //
                    // -- execute page Dependencies
                    List<Models.DbModels.addonModel> pageAddonList = addonModel.createList_pageDependencies(core, core.doc.pageController.page.id);
                    if ( pageAddonList.Count> 0 ) {
                        string addonContextMessage = executeContext.errorContextMessage;
                        foreach (addonModel addon in pageAddonList) {
                            executeContext.errorContextMessage = "executing page dependency [" + addon.name + "]";
                            result += core.addon.executeDependency(addon, executeContext);
                        }
                        executeContext.errorContextMessage = addonContextMessage;
                    }
                    //
                    core.doc.adminWarning = core.docProperties.getText("main_AdminWarningMsg");
                    core.doc.adminWarningPageID = core.docProperties.getInteger("main_AdminWarningPageID");
                    //
                    // todo move cookie test to htmlDoc controller
                    // -- Add cookie test
                    bool AllowCookieTest = core.siteProperties.allowVisitTracking && (core.session.visit.PageVisits == 1);
                    if (AllowCookieTest) {
                        core.html.addScriptCode_onLoad("if (document.cookie && document.cookie != null){cj.ajax.qs('f92vo2a8d=" + securityController.encodeToken( core,core.session.visit.id, core.doc.profileStartTime) + "')};", "Cookie Test");
                    }
                    //
                    //--------------------------------------------------------------------------
                    //   User form processing
                    //       if a form is created in the editor, process it by emailing and saving to the User Form Response content
                    //--------------------------------------------------------------------------
                    //
                    if (core.docProperties.getInteger("ContensiveUserForm") == 1) {
                        string FromAddress = core.siteProperties.getText("EmailFromAddress", "info@" + core.webServer.requestDomain);
                        emailController.queueFormEmail(core, core.siteProperties.emailAdmin, FromAddress, "Form Submitted on " + core.webServer.requestReferer);
                        int cs = core.db.csInsertRecord("User Form Response");
                        if (core.db.csOk(cs)) {
                            core.db.csSet(cs, "name", "Form " + core.webServer.requestReferrer);
                            string Copy = "";

                            foreach (string key in core.docProperties.getKeyList()) {
                                docPropertiesClass docProperty = core.docProperties.getProperty(key);
                                if (key.ToLower() != "contensiveuserform") {
                                    Copy += docProperty.Name + "=" + docProperty.Value + "\r\n";
                                }
                            }
                            core.db.csSet(cs, "copy", Copy);
                            core.db.csSet(cs, "VisitId", core.session.visit.id);
                        }
                        core.db.csClose(ref cs);
                    }
                    //
                    //--------------------------------------------------------------------------
                    //   Contensive Form Page Processing
                    //--------------------------------------------------------------------------
                    //
                    if (core.docProperties.getInteger("ContensiveFormPageID") != 0) {
                        processForm(core, core.docProperties.getInteger("ContensiveFormPageID"));
                    }
                    //
                    //--------------------------------------------------------------------------
                    // ----- Automatic Redirect to a full URL
                    //   If the link field of the record is an absolution address
                    //       rc = redirect contentID
                    //       ri = redirect content recordid
                    //--------------------------------------------------------------------------
                    //
                    core.doc.redirectContentID = (core.docProperties.getInteger(rnRedirectContentId));
                    if (core.doc.redirectContentID != 0) {
                        core.doc.redirectRecordID = (core.docProperties.getInteger(rnRedirectRecordId));
                        if (core.doc.redirectRecordID != 0) {
                            string contentName = Models.Complex.cdefModel.getContentNameByID(core, core.doc.redirectContentID);
                            if (!string.IsNullOrEmpty(contentName)) {
                                if (iisController.main_RedirectByRecord_ReturnStatus(core, contentName, core.doc.redirectRecordID)) {
                                    //
                                    //Call AppendLog("main_init(), 3210 - exit for rc/ri redirect ")
                                    //
                                    core.doc.continueProcessing = false; //--- should be disposed by caller --- Call dispose
                                    return string.Empty;
                                } else {
                                    core.doc.adminWarning = "<p>The site attempted to automatically jump to another page, but there was a problem with the page that included the link.<p>";
                                    core.doc.adminWarningPageID = core.doc.redirectRecordID;
                                }
                            }
                        }
                    }
                    //
                    //--------------------------------------------------------------------------
                    // ----- Active Download hook
                    string RecordEID = core.docProperties.getText(RequestNameLibraryFileID);
                    if (!string.IsNullOrEmpty(RecordEID)) {
                        DateTime tokenDate = default(DateTime);
                        int downloadId = 0;
                        securityController.decodeToken(core, RecordEID, ref downloadId, ref tokenDate);
                        if (downloadId != 0) {
                            //
                            // -- lookup record and set clicks
                            libraryFilesModel file = libraryFilesModel.create(core, downloadId);
                            if (file != null) {
                                file.clicks += 1;
                                file.save(core);
                                if (file.filename != "") {
                                    //
                                    // -- create log entry
                                    libraryFileLogModel log = libraryFileLogModel.add(core);
                                    if (log != null) {
                                        log.FileID = file.id;
                                        log.VisitID = core.session.visit.id;
                                        log.MemberID = core.session.user.id;
                                    }
                                    //
                                    // -- and go
                                    string link = genericController.getCdnFileLink(core, file.filename);
                                    //string link = core.webServer.requestProtocol + core.webServer.requestDomain + genericController.getCdnFileLink(core, file.Filename);
                                    return core.webServer.redirect(link, "Redirecting because the active download request variable is set to a valid Library Files record. Library File Log has been appended.");
                                }
                            }
                            //
                        }
                    }
                    //
                    //--------------------------------------------------------------------------
                    //   Process clipboard cut/paste
                    //--------------------------------------------------------------------------
                    //
                    string Clip = core.docProperties.getText(RequestNameCut);
                    if (!string.IsNullOrEmpty(Clip)) {
                        //
                        // if a cut, load the clipboard
                        //
                        core.visitProperty.setProperty("Clipboard", Clip);
                        genericController.modifyLinkQuery(core.doc.refreshQueryString, RequestNameCut, "");
                    }
                    int ClipParentContentID = core.docProperties.getInteger(RequestNamePasteParentContentID);
                    int ClipParentRecordID = core.docProperties.getInteger(RequestNamePasteParentRecordID);
                    string ClipParentFieldList = core.docProperties.getText(RequestNamePasteFieldList);
                    if ((ClipParentContentID != 0) & (ClipParentRecordID != 0)) {
                        //
                        // Request for a paste, clear the cliboard
                        //
                        string ClipBoard = core.visitProperty.getText("Clipboard", "");
                        core.visitProperty.setProperty("Clipboard", "");
                        genericController.modifyQueryString(core.doc.refreshQueryString, RequestNamePasteParentContentID, "");
                        genericController.modifyQueryString(core.doc.refreshQueryString, RequestNamePasteParentRecordID, "");
                        string ClipParentContentName = Models.Complex.cdefModel.getContentNameByID(core, ClipParentContentID);
                        if (string.IsNullOrEmpty(ClipParentContentName)) {
                            // state not working...
                        } else if (string.IsNullOrEmpty(ClipBoard)) {
                            // state not working...
                        } else {
                            if (!core.session.isAuthenticatedContentManager(core, ClipParentContentName)) {
                                errorController.addUserError(core, "The paste operation failed because you are not a content manager of the Clip Parent");
                            } else {
                                //
                                // Current identity is a content manager for this content
                                //
                                int Position = genericController.vbInstr(1, ClipBoard, ".");
                                if (Position == 0) {
                                    errorController.addUserError(core, "The paste operation failed because the clipboard data is configured incorrectly.");
                                } else {
                                    string[] ClipBoardArray = ClipBoard.Split('.');
                                    if (ClipBoardArray.GetUpperBound(0) == 0) {
                                        errorController.addUserError(core, "The paste operation failed because the clipboard data is configured incorrectly.");
                                    } else {
                                        int ClipChildContentID = genericController.encodeInteger(ClipBoardArray[0]);
                                        int ClipChildRecordID = genericController.encodeInteger(ClipBoardArray[1]);
                                        if (!Models.Complex.cdefModel.isWithinContent(core, ClipChildContentID, ClipParentContentID)) {
                                            errorController.addUserError(core, "The paste operation failed because the destination location is not compatible with the clipboard data.");
                                        } else {
                                            //
                                            // the content definition relationship is OK between the child and parent record
                                            //
                                            string ClipChildContentName = Models.Complex.cdefModel.getContentNameByID(core, ClipChildContentID);
                                            if (!(!string.IsNullOrEmpty(ClipChildContentName))) {
                                                errorController.addUserError(core, "The paste operation failed because the clipboard data content is undefined.");
                                            } else {
                                                if (ClipParentRecordID == 0) {
                                                    errorController.addUserError(core, "The paste operation failed because the clipboard data record is undefined.");
                                                } else if (pageContentController.isChildRecord(core, ClipChildContentName, ClipParentRecordID, ClipChildRecordID)) {
                                                    errorController.addUserError(core, "The paste operation failed because the destination location is a child of the clipboard data record.");
                                                } else {
                                                    //
                                                    // the parent record is not a child of the child record (circular check)
                                                    //
                                                    string ClipChildRecordName = "record " + ClipChildRecordID;
                                                    int CSClip = core.db.csOpen2(ClipChildContentName, ClipChildRecordID, true, true);
                                                    if (!core.db.csOk(CSClip)) {
                                                        errorController.addUserError(core, "The paste operation failed because the data record referenced by the clipboard could not found.");
                                                    } else {
                                                        //
                                                        // Paste the edit record record
                                                        //
                                                        ClipChildRecordName = core.db.csGetText(CSClip, "name");
                                                        if (string.IsNullOrEmpty(ClipParentFieldList)) {
                                                            //
                                                            // Legacy paste - go right to the parent id
                                                            //
                                                            if (!core.db.csIsFieldSupported(CSClip, "ParentID")) {
                                                                errorController.addUserError(core, "The paste operation failed because the record you are pasting does not   support the necessary parenting feature.");
                                                            } else {
                                                                core.db.csSet(CSClip, "ParentID", ClipParentRecordID);
                                                            }
                                                        } else {
                                                            //
                                                            // Fill in the Field List name values
                                                            //
                                                            string[] Fields = ClipParentFieldList.Split(',');
                                                            int FieldCount = Fields.GetUpperBound(0) + 1;
                                                            for (var FieldPointer = 0; FieldPointer < FieldCount; FieldPointer++) {
                                                                string Pair = Fields[FieldPointer];
                                                                if (Pair.Left( 1) == "(" && Pair.Substring(Pair.Length - 1, 1) == ")") {
                                                                    Pair = Pair.Substring(1, Pair.Length - 2);
                                                                }
                                                                string[] NameValues = Pair.Split('=');
                                                                if (NameValues.GetUpperBound(0) == 0) {
                                                                    errorController.addUserError(core, "The paste operation failed because the clipboard data Field List is not configured correctly.");
                                                                } else {
                                                                    if (!core.db.csIsFieldSupported(CSClip, encodeText(NameValues[0]))) {
                                                                        errorController.addUserError(core, "The paste operation failed because the clipboard data Field [" + encodeText(NameValues[0]) + "] is not supported by the location data.");
                                                                    } else {
                                                                        core.db.csSet(CSClip, encodeText(NameValues[0]), encodeText(NameValues[1]));
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        //
                                                        // Fixup Content Watch
                                                        //
                                                        //ShortLink = main_ServerPathPage
                                                        //ShortLink = ConvertLinkToShortLink(ShortLink, main_ServerHost, main_ServerVirtualPath)
                                                        //ShortLink = genericController.modifyLinkQuery(ShortLink, rnPageId, CStr(ClipChildRecordID), True)
                                                        //Call main_TrackContentSet(CSClip, ShortLink)
                                                    }
                                                    core.db.csClose(ref CSClip);
                                                    //
                                                    // Set Child Pages Found and clear caches
                                                    //
                                                    CSClip = core.db.csOpenRecord(ClipParentContentName, ClipParentRecordID,false,false, "ChildPagesFound");
                                                    if (core.db.csOk(CSClip)) {
                                                        core.db.csSet(CSClip, "ChildPagesFound", true.ToString());
                                                    }
                                                    core.db.csClose(ref CSClip);
                                                    //
                                                    // Live Editing
                                                    //
                                                    core.cache.invalidateAllInContent(ClipChildContentName);
                                                    core.cache.invalidateAllInContent(ClipParentContentName);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Clip = core.docProperties.getText(RequestNameCutClear);
                    if (!string.IsNullOrEmpty(Clip)) {
                        //
                        // if a cut clear, clear the clipboard
                        //
                        core.visitProperty.setProperty("Clipboard", "");
                        Clip = core.visitProperty.getText("Clipboard", "");
                        genericController.modifyLinkQuery(core.doc.refreshQueryString, RequestNameCutClear, "");
                    }
                    //
                    // link alias and link forward
                    string linkAliasTest1 = core.webServer.requestPathPage;
                    if (linkAliasTest1.Left( 1) == "/") {
                        linkAliasTest1 = linkAliasTest1.Substring(1);
                    }
                    if (linkAliasTest1.Length > 0) {
                        if (linkAliasTest1.Substring(linkAliasTest1.Length - 1, 1) == "/") {
                            linkAliasTest1 = linkAliasTest1.Left( linkAliasTest1.Length - 1);
                        }
                    }
                    string linkAliasTest2 = linkAliasTest1 + "/";
                    string Sql = "";
                    if ((!IsPageNotFound) && (core.webServer.requestPathPage != "")) {
                        //
                        // build link variations needed later
                        int Pos = genericController.vbInstr(1, core.webServer.requestPathPage, "://", 1);
                        string LinkNoProtocol = "";
                        string LinkFullPath = "";
                        string LinkFullPathNoSlash = "";
                        if (Pos != 0) {
                            LinkNoProtocol = core.webServer.requestPathPage.Substring(Pos + 2);
                            Pos = genericController.vbInstr(Pos + 3, core.webServer.requestPathPage, "/", 2);
                            if (Pos != 0) {
                                string linkDomain = core.webServer.requestPathPage.Left(Pos - 1);
                                LinkFullPath = core.webServer.requestPathPage.Substring(Pos - 1);
                                //
                                // strip off leading or trailing slashes, and return only the string between the leading and secton slash
                                //
                                if (genericController.vbInstr(1, LinkFullPath, "/") != 0) {
                                    string[] LinkSplit = LinkFullPath.Split('/');
                                    LinkFullPathNoSlash = LinkSplit[0];
                                    if (string.IsNullOrEmpty(LinkFullPathNoSlash)) {
                                        if (LinkSplit.GetUpperBound(0) > 0) {
                                            LinkFullPathNoSlash = LinkSplit[1];
                                        }
                                    }
                                }
                                linkAliasTest1 = LinkFullPath;
                                linkAliasTest2 = LinkFullPathNoSlash;
                            }
                        }
                        //
                        //   if this has not already been recognized as a pagenot found, and the custom404source is present, try all these
                        //   Build LinkForwardCritia and LinkAliasCriteria
                        //   sample: http://www.a.com/kb/test
                        //   LinkForwardCriteria = (Sourcelink='http://www.a.com/kb/test')or(Sourcelink='http://www.a.com/kb/test/')
                        //
                        string LinkForwardCriteria = ""
                            + "(active<>0)"
                            + "and("
                            + "(SourceLink=" + core.db.encodeSQLText(core.webServer.requestPathPage) + ")"
                            + "or(SourceLink=" + core.db.encodeSQLText(LinkNoProtocol) + ")"
                            + "or(SourceLink=" + core.db.encodeSQLText(LinkFullPath) + ")"
                            + "or(SourceLink=" + core.db.encodeSQLText(LinkFullPathNoSlash) + ")"
                            + ")";
                        Sql = core.db.GetSQLSelect("", "ccLinkForwards", "ID,DestinationLink,Viewings,GroupID", LinkForwardCriteria, "ID", "", 1);
                        int CSPointer = core.db.csOpenSql(Sql);
                        bool IsInLinkForwardTable = false;
                        bool isLinkForward = false;
                        if (core.db.csOk(CSPointer)) {
                            //
                            // Link Forward found - update count
                            //
                            string tmpLink = null;
                            int GroupID = 0;
                            string groupName = null;
                            //
                            IsInLinkForwardTable = true;
                            int Viewings = core.db.csGetInteger(CSPointer, "Viewings") + 1;
                            Sql = "update ccLinkForwards set Viewings=" + Viewings + " where ID=" + core.db.csGetInteger(CSPointer, "ID");
                            core.db.executeQuery(Sql);
                            tmpLink = core.db.csGetText(CSPointer, "DestinationLink");
                            if (!string.IsNullOrEmpty(tmpLink)) {
                                //
                                // Valid Link Forward (without link it is just a record created by the autocreate function
                                //
                                isLinkForward = true;
                                tmpLink = core.db.csGetText(CSPointer, "DestinationLink");
                                GroupID = core.db.csGetInteger(CSPointer, "GroupID");
                                if (GroupID != 0) {
                                    groupName = groupController.group_GetGroupName(core, GroupID);
                                    if (!string.IsNullOrEmpty(groupName)) {
                                        groupController.group_AddGroupMember(core, groupName);
                                    }
                                }
                                if (!string.IsNullOrEmpty(tmpLink)) {
                                    core.doc.redirectLink = tmpLink;
                                }
                            }
                        }
                        core.db.csClose(ref CSPointer);
                        //
                        if ((string.IsNullOrEmpty(core.doc.redirectLink)) && !isLinkForward) {
                            //
                            // Test for Link Alias
                            //
                            if (!string.IsNullOrEmpty(linkAliasTest1 + linkAliasTest2)) {
                                string sqlLinkAliasCriteria = "(name=" + core.db.encodeSQLText(linkAliasTest1) + ")or(name=" + core.db.encodeSQLText(linkAliasTest2) + ")";
                                List<Models.DbModels.linkAliasModel> linkAliasList = linkAliasModel.createList(core, sqlLinkAliasCriteria, "id desc");
                                if (linkAliasList.Count > 0) {
                                    linkAliasModel linkAlias = linkAliasList.First();
                                    string LinkQueryString = rnPageId + "=" + linkAlias.PageID + "&" + linkAlias.QueryStringSuffix;
                                    core.docProperties.setProperty(rnPageId, linkAlias.PageID.ToString(), false);
                                    string[] nameValuePairs = linkAlias.QueryStringSuffix.Split('&');
                                    //Dim nameValuePairs As String() = Split(core.cache_linkAlias(linkAliasCache_queryStringSuffix, Ptr), "&")
                                    foreach (string nameValuePair in nameValuePairs) {
                                        string[] nameValueThing = nameValuePair.Split('=');
                                        if (nameValueThing.GetUpperBound(0) == 0) {
                                            core.docProperties.setProperty(nameValueThing[0], "", false);
                                        } else {
                                            core.docProperties.setProperty(nameValueThing[0], nameValueThing[1], false);
                                        }
                                    }
                                }
                            }
                            //
                            // No Link Forward, no Link Alias, no RemoteMethodFromPage, not Robots.txt
                            //
                            if ((core.doc.errorCount == 0) && core.siteProperties.getBoolean("LinkForwardAutoInsert") && (!IsInLinkForwardTable)) {
                                //
                                // Add a new Link Forward entry
                                //
                                CSPointer = core.db.csInsertRecord("Link Forwards");
                                if (core.db.csOk(CSPointer)) {
                                    core.db.csSet(CSPointer, "Name", core.webServer.requestPathPage);
                                    core.db.csSet(CSPointer, "sourcelink", core.webServer.requestPathPage);
                                    core.db.csSet(CSPointer, "Viewings", 1);
                                }
                                core.db.csClose(ref CSPointer);
                            }
                        }
                    }
                    //
                    // ----- do anonymous access blocking
                    if (!core.session.isAuthenticated) {
                        if ((core.webServer.requestPath != "/") & genericController.vbInstr(1, "/" + core.appConfig.adminRoute, core.webServer.requestPath, 1) != 0) {
                            //
                            // admin page is excluded from custom blocking
                        } else {
                            int AnonymousUserResponseID = genericController.encodeInteger(core.siteProperties.getText("AnonymousUserResponseID", "0"));
                            switch (AnonymousUserResponseID) {
                                case 1:
                                    //
                                    // -- block with login
                                    core.doc.continueProcessing = false;
                                    return core.addon.execute(addonModel.create(core, addonGuidLoginPage), new CPUtilsBaseClass.addonExecuteContext() {
                                        addonType = CPUtilsBaseClass.addonContext.ContextPage,
                                        errorContextMessage = "calling login page addon [" + addonGuidLoginPage + "] because page is blocked"
                                    });
                                case 2:
                                    //
                                    // -- block with custom content
                                    core.doc.continueProcessing = false;
                                    core.doc.setMetaContent(0, 0);
                                    core.html.addScriptCode_onLoad("document.body.style.overflow='scroll'", "Anonymous User Block");
                                    return core.html.getHtmlDoc('\r' + core.html.getContentCopy("AnonymousUserResponseCopy", "<p style=\"width:250px;margin:100px auto auto auto;\">The site is currently not available for anonymous access.</p>", core.session.user.id, true, core.session.isAuthenticated), TemplateDefaultBodyTag, true, true);
                            }
                        }
                    }
                    //
                    // -- build document
                    string htmlDocBody = getHtmlBodyTemplate(core);
                    //
                    // -- check secure certificate required
                    bool SecureLink_Template_Required = core.doc.pageController.template.isSecure;
                    bool SecureLink_Page_Required = false;
                    foreach (Models.DbModels.pageContentModel page in core.doc.pageController.pageToRootList) {
                        if (core.doc.pageController.page.IsSecure) {
                            SecureLink_Page_Required = true;
                            break;
                        }
                    }
                    bool SecureLink_Required = SecureLink_Template_Required || SecureLink_Page_Required;
                    bool SecureLink_CurrentURL = (core.webServer.requestUrl.ToLower().Left( 8) == "https://");
                    if (SecureLink_CurrentURL && (!SecureLink_Required)) {
                        //
                        // -- redirect to non-secure
                        core.doc.redirectLink = genericController.vbReplace(core.webServer.requestUrl, "https://", "http://");
                        core.doc.redirectReason = "Redirecting because neither the page or the template requires a secure link.";
                        core.doc.redirectBecausePageNotFound = false;
                        return core.webServer.redirect(core.doc.redirectLink, core.doc.redirectReason, core.doc.redirectBecausePageNotFound);
                        //return "";
                    } else if ((!SecureLink_CurrentURL) && SecureLink_Required) {
                        //
                        // -- redirect to secure
                        core.doc.redirectLink = genericController.vbReplace(core.webServer.requestUrl, "http://", "https://");
                        if (SecureLink_Page_Required) {
                            core.doc.redirectReason = "Redirecting because this page [" + core.doc.pageController.pageToRootList[0].name + "] requires a secure link.";
                        } else {
                            core.doc.redirectReason = "Redirecting because this template [" + core.doc.pageController.template.name + "] requires a secure link.";
                        }
                        core.doc.redirectBecausePageNotFound = false;
                        return core.webServer.redirect(core.doc.redirectLink, core.doc.redirectReason, core.doc.redirectBecausePageNotFound);
                        //return "";
                    }
                    //
                    // -- check that this template exists on this domain
                    // -- if endpoint is just domain -> the template is automatically compatible by default (domain determined the landing page)
                    // -- if endpoint is domain + route (link alias), the route determines the page, which may determine the core.doc.pageController.template. If this template is not allowed for this domain, redirect to the domain's landingcore.doc.pageController.page.
                    //
                    Sql = "(domainId=" + core.doc.domain.id + ")";
                    List<Models.DbModels.TemplateDomainRuleModel> allowTemplateRuleList = TemplateDomainRuleModel.createList(core, Sql);
                    if (allowTemplateRuleList.Count == 0) {
                        //
                        // -- current template has no domain preference, use current
                    } else {
                        bool allowTemplate = false;
                        foreach (TemplateDomainRuleModel rule in allowTemplateRuleList) {
                            if (rule.templateId == core.doc.pageController.template.id) {
                                allowTemplate = true;
                                break;
                            }
                        }
                        if (!allowTemplate) {
                            //
                            // -- must redirect to a domain's landing page
                            core.doc.redirectLink = core.webServer.requestProtocol + core.doc.domain.name;
                            core.doc.redirectBecausePageNotFound = false;
                            core.doc.redirectReason = "Redirecting because this domain has template requiements set, and this template is not configured [" + core.doc.pageController.template.name + "].";
                            core.doc.redirectBecausePageNotFound = false;
                            return core.webServer.redirect(core.doc.redirectLink, core.doc.redirectReason, core.doc.redirectBecausePageNotFound);
                            //return "";
                        }
                    }
                    result += htmlDocBody;
                }
                //--------------------------------------------------------------------------
                // ----- Process Early page-not-found
                //--------------------------------------------------------------------------
                //
                if (IsPageNotFound) {
                    //
                    // new way -- if a (real) 404 page is received, just convert this hit to the page-not-found page, do not redirect to it
                    //
                    logController.addSiteWarning(core, "Page Not Found", "Page Not Found", "", 0, "Page Not Found from [" + core.webServer.requestUrlSource + "]", "Page Not Found", "Page Not Found");
                    core.webServer.setResponseStatus(iisController.httpResponseStatus404);
                    core.docProperties.setProperty(rnPageId, getPageNotFoundPageId(core));
                    //Call main_mergeInStream(rnPageId & "=" & main_GetPageNotFoundPageId())
                    if (core.session.isAuthenticatedAdmin(core)) {
                        //string RedirectLink = "";
                        string PageNotFoundReason = "";
                        core.doc.adminWarning = PageNotFoundReason;
                        core.doc.adminWarningPageID = 0;
                    }
                }
                //
                // add exception list header
                if (core.session.user.Developer) {
                    result = errorController.getDocExceptionHtmlList(core) + result;
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //
        //
        private static void processForm(coreController core, int FormPageID) {
            try {
                //
                // main_Get the instructions from the record
                //
                int CS = core.db.csOpenRecord("Form Pages", FormPageID);
                string Formhtml = "";
                string FormInstructions = "";
                if (core.db.csOk(CS)) {
                    Formhtml = core.db.csGetText(CS, "Body");
                    FormInstructions = core.db.csGetText(CS, "Instructions");
                }
                core.db.csClose(ref CS);
                main_FormPagetype f;
                if (!string.IsNullOrEmpty(FormInstructions)) {
                    //
                    // Load the instructions
                    //
                    f = loadFormPageInstructions(core, FormInstructions, Formhtml);
                    if (f.AuthenticateOnFormProcess & !core.session.isAuthenticated & core.session.isRecognized(core)) {
                        //
                        // If this form will authenticate when done, and their is a current, non-authenticated account -- logout first
                        //
                        core.session.logout(core);
                    }
                    int CSPeople = -1;
                    bool Success = true;
                    int Ptr = 0;
                    string PeopleFirstName = "";
                    string PeopleLastName = "";
                    string PeopleName = "";
                    for (Ptr = 0; Ptr <= f.Inst.GetUpperBound(0); Ptr++) {
                        var tempVar = f.Inst[Ptr];
                        bool IsInGroup = false;
                        bool WasInGroup = false;
                        string FormValue = null;
                        switch (tempVar.Type) {
                            case 1:
                                //
                                // People Record
                                //
                                FormValue = core.docProperties.getText(tempVar.PeopleField);
                                if ((!string.IsNullOrEmpty(FormValue)) & genericController.encodeBoolean(Models.Complex.cdefModel.GetContentFieldProperty(core, "people", tempVar.PeopleField, "uniquename"))) {
                                    string SQL = "select count(*) from ccMembers where " + tempVar.PeopleField + "=" + core.db.encodeSQLText(FormValue);
                                    CS = core.db.csOpenSql(SQL);
                                    if (core.db.csOk(CS)) {
                                        Success = core.db.csGetInteger(CS, "cnt") == 0;
                                    }
                                    core.db.csClose(ref CS);
                                    if (!Success) {
                                        errorController.addUserError(core, "The field [" + tempVar.Caption + "] must be unique, and the value [" + htmlController.encodeHtml(FormValue) + "] has already been used.");
                                    }
                                }
                                if ((tempVar.REquired | genericController.encodeBoolean(Models.Complex.cdefModel.GetContentFieldProperty(core, "people", tempVar.PeopleField, "required"))) && string.IsNullOrEmpty(FormValue)) {
                                    Success = false;
                                    errorController.addUserError(core, "The field [" + htmlController.encodeHtml(tempVar.Caption) + "] is required.");
                                } else {
                                    if (!core.db.csOk(CSPeople)) {
                                        CSPeople = core.db.csOpenRecord("people", core.session.user.id);
                                    }
                                    if (core.db.csOk(CSPeople)) {
                                        string PeopleUsername = null;
                                        string PeoplePassword = null;
                                        string PeopleEmail = "";
                                        switch (genericController.vbUCase(tempVar.PeopleField)) {
                                            case "NAME":
                                                PeopleName = FormValue;
                                                core.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
                                                break;
                                            case "FIRSTNAME":
                                                PeopleFirstName = FormValue;
                                                core.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
                                                break;
                                            case "LASTNAME":
                                                PeopleLastName = FormValue;
                                                core.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
                                                break;
                                            case "EMAIL":
                                                PeopleEmail = FormValue;
                                                core.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
                                                break;
                                            case "USERNAME":
                                                PeopleUsername = FormValue;
                                                core.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
                                                break;
                                            case "PASSWORD":
                                                PeoplePassword = FormValue;
                                                core.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
                                                break;
                                            default:
                                                core.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
                                                break;
                                        }
                                    }
                                }
                                break;
                            case 2:
                                //
                                // Group main_MemberShip
                                //
                                IsInGroup = core.docProperties.getBoolean("Group" + tempVar.GroupName);
                                WasInGroup = core.session.isMemberOfGroup(core, tempVar.GroupName);
                                if (WasInGroup && !IsInGroup) {
                                    groupController.group_DeleteGroupMember(core, tempVar.GroupName);
                                } else if (IsInGroup && !WasInGroup) {
                                    groupController.group_AddGroupMember(core, tempVar.GroupName);
                                }
                                break;
                        }
                    }
                    //
                    // Create People Name
                    //
                    if (string.IsNullOrEmpty(PeopleName) && !string.IsNullOrEmpty(PeopleFirstName) & !string.IsNullOrEmpty(PeopleLastName)) {
                        if (core.db.csOk(CSPeople)) {
                            core.db.csSet(CSPeople, "name", PeopleFirstName + " " + PeopleLastName);
                        }
                    }
                    core.db.csClose(ref CSPeople);
                    //
                    // AuthenticationOnFormProcess requires Username/Password and must be valid
                    //
                    if (Success) {
                        //
                        // Authenticate
                        //
                        if (f.AuthenticateOnFormProcess) {
                            sessionController.authenticateById(core, core.session.user.id, core.session);
                        }
                        //
                        // Join Group requested by page that created form
                        //
                        DateTime tokenDate = default(DateTime);
                        int GroupIDToJoinOnSuccess = 0;
                        securityController.decodeToken(core, core.docProperties.getText("SuccessID"), ref GroupIDToJoinOnSuccess, ref tokenDate);
                        //GroupIDToJoinOnSuccess = main_DecodeKeyNumber(main_GetStreamText2("SuccessID"))
                        if (GroupIDToJoinOnSuccess != 0) {
                            groupController.group_AddGroupMember(core, groupController.group_GetGroupName(core, GroupIDToJoinOnSuccess));
                        }
                        //
                        // Join Groups requested by pageform
                        //
                        if (f.AddGroupNameList != "") {
                            string[] Groups = (encodeText(f.AddGroupNameList).Trim(' ')).Split(',');
                            for (Ptr = 0; Ptr <= Groups.GetUpperBound(0); Ptr++) {
                                string GroupName = Groups[Ptr].Trim(' ');
                                if (!string.IsNullOrEmpty(GroupName)) {
                                    groupController.group_AddGroupMember(core, GroupName);
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        // Instruction Format
        //   Line 1 - Version FormBuilderVersion
        //   Line 2+ instruction lines
        //   blank line
        //   FormHTML
        //
        // Instruction Line Format
        //   Type,Caption,Required,arguments
        //
        //   Types
        //       1 = People Content field
        //           arguments = FieldName
        //       2 = Group main_MemberShip
        //           arguments = GroupName
        //
        // FormHTML
        //   All HTML with the following:
        //   ##REPEAT></REPEAT> tags -- repeated for each instruction line
        //   {{CAPTION}} tags -- main_Gets the caption for each instruction line
        //   {{FIELD}} tags -- main_Gets the form field for each instruction line
        //
        internal static main_FormPagetype loadFormPageInstructions(coreController core, string FormInstructions, string Formhtml) {
            main_FormPagetype result = new main_FormPagetype();
            try {
                //
                if (true) {
                    int PtrFront = genericController.vbInstr(1, Formhtml, "{{REPEATSTART", 1);
                    if (PtrFront > 0) {
                        int PtrBack = genericController.vbInstr(PtrFront, Formhtml, "}}");
                        if (PtrBack > 0) {
                            result.PreRepeat = Formhtml.Left( PtrFront - 1);
                            PtrFront = genericController.vbInstr(PtrBack, Formhtml, "{{REPEATEND", 1);
                            if (PtrFront > 0) {
                                result.RepeatCell = Formhtml.Substring(PtrBack + 1, PtrFront - PtrBack - 2);
                                PtrBack = genericController.vbInstr(PtrFront, Formhtml, "}}");
                                if (PtrBack > 0) {
                                    result.PostRepeat = Formhtml.Substring(PtrBack + 1);
                                    //
                                    // Decode instructions and build output
                                    //
                                    string[] i = genericController.splitNewLine(FormInstructions);
                                    if (i.GetUpperBound(0) > 0) {
                                        int IStart = 0;
                                        if (string.CompareOrdinal(i[0].Trim(' '), "1") >= 0) {
                                            //
                                            // decode Version 1 arguments, then start instructions line at line 1
                                            //
                                            result.AddGroupNameList = genericController.encodeText(i[1]);
                                            result.AuthenticateOnFormProcess = genericController.encodeBoolean(i[2]);
                                            IStart = 3;
                                        }
                                        //
                                        // read in and compose the repeat lines
                                        //
                                        Array.Resize(ref result.Inst, i.GetUpperBound(0));
                                        int IPtr = 0;
                                        for (IPtr = 0; IPtr <= i.GetUpperBound(0) - IStart; IPtr++) {
                                            var tempVar = result.Inst[IPtr];
                                            string[] IArgs = i[IPtr + IStart].Split(',');
                                            if (IArgs.GetUpperBound(0) >= main_IPosMax) {
                                                tempVar.Caption = IArgs[main_IPosCaption];
                                                tempVar.Type = genericController.encodeInteger(IArgs[main_IPosType]);
                                                tempVar.REquired = genericController.encodeBoolean(IArgs[main_IPosRequired]);
                                                switch (tempVar.Type) {
                                                    case 1:
                                                        //
                                                        // People Record
                                                        //
                                                        tempVar.PeopleField = IArgs[main_IPosPeopleField];
                                                        break;
                                                    case 2:
                                                        //
                                                        // Group main_MemberShip
                                                        //
                                                        tempVar.GroupName = IArgs[main_IPosGroupName];
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //
        //
        //
        internal static string getFormPage(coreController core, string FormPageName, int GroupIDToJoinOnSuccess) {
            string result = null;
            try {
                main_FormPagetype f;
                bool IsRetry =  (core.docProperties.getInteger("ContensiveFormPageID") != 0);
                int CS = core.db.csOpen("Form Pages", "name=" + core.db.encodeSQLText(FormPageName));
                string Formhtml = "";
                string FormInstructions = "";
                int FormPageID = 0;
                if (core.db.csOk(CS)) {
                    FormPageID = core.db.csGetInteger(CS, "ID");
                    Formhtml = core.db.csGetText(CS, "Body");
                    FormInstructions = core.db.csGetText(CS, "Instructions");
                }
                core.db.csClose(ref CS);
                f = loadFormPageInstructions(core, FormInstructions, Formhtml);
                string RepeatBody = "";
                int CSPeople = -1;
                int IPtr = 0;
                string Body = null;
                bool HasRequiredFields = false;
                for (IPtr = 0; IPtr <= f.Inst.GetUpperBound(0); IPtr++) {
                    var tempVar = f.Inst[IPtr];
                    bool GroupValue = false;
                    int GroupRowPtr = 0;
                    string CaptionSpan = null;
                    string Caption = null;
                    switch (tempVar.Type) {
                        case 1:
                            //
                            // People Record
                            //
                            if (IsRetry && core.docProperties.getText(tempVar.PeopleField) == "") {
                                CaptionSpan = "<span class=\"ccError\">";
                            } else {
                                CaptionSpan = "<span>";
                            }
                            if (!core.db.csOk(CSPeople)) {
                                CSPeople = core.db.csOpenRecord("people", core.session.user.id);
                            }
                            Caption = tempVar.Caption;
                            if (tempVar.REquired | genericController.encodeBoolean(Models.Complex.cdefModel.GetContentFieldProperty(core, "People", tempVar.PeopleField, "Required"))) {
                                Caption = "*" + Caption;
                            }
                            if (core.db.csOk(CSPeople)) {
                                Body = f.RepeatCell;
                                Body = genericController.vbReplace(Body, "{{CAPTION}}", CaptionSpan + Caption + "</span>", 1, 99, 1);
                                Body = genericController.vbReplace(Body, "{{FIELD}}", core.html.inputCs(CSPeople, "People", tempVar.PeopleField), 1, 99, 1);
                                RepeatBody = RepeatBody + Body;
                                HasRequiredFields = HasRequiredFields || tempVar.REquired;
                            }
                            break;
                        case 2:
                            //
                            // Group main_MemberShip
                            //
                            GroupValue = core.session.isMemberOfGroup(core, tempVar.GroupName);
                            Body = f.RepeatCell;
                            Body = genericController.vbReplace(Body, "{{CAPTION}}", htmlController.checkbox("Group" + tempVar.GroupName, GroupValue), 1, 99, 1);
                            Body = genericController.vbReplace(Body, "{{FIELD}}", tempVar.Caption);
                            RepeatBody = RepeatBody + Body;
                            GroupRowPtr = GroupRowPtr + 1;
                            HasRequiredFields = HasRequiredFields || tempVar.REquired;
                            break;
                    }
                }
                core.db.csClose(ref CSPeople);
                if (HasRequiredFields) {
                    Body = f.RepeatCell;
                    Body = genericController.vbReplace(Body, "{{CAPTION}}", "&nbsp;", 1, 99, 1);
                    Body = genericController.vbReplace(Body, "{{FIELD}}", "*&nbsp;Required Fields");
                    RepeatBody = RepeatBody + Body;
                }
                //
                result = ""
                    + errorController.getUserError(core) 
                    + htmlController.formMultipart_start(core, core.doc.refreshQueryString,"", "ccForm") 
                    + htmlController.inputHidden("ContensiveFormPageID", FormPageID) 
                    + htmlController.inputHidden("SuccessID", securityController.encodeToken( core,GroupIDToJoinOnSuccess, core.doc.profileStartTime)) + f.PreRepeat + RepeatBody + f.PostRepeat 
                    + htmlController.form_end();
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   getContentBox
        //
        //   PageID is the page to display. If it is 0, the root page is displayed
        //   RootPageID has to be the ID of the root page for PageID
        //=============================================================================
        //
        public static string getContentBox(coreController core, string OrderByClause, bool AllowChildPageList, bool AllowReturnLink, bool ArchivePages, int ignoreme, bool UseContentWatchLink, bool allowPageWithoutSectionDisplay) {
            string returnHtml = "";
            try {
                core.html.addHeadTag("<meta name=\"contentId\" content=\"" + core.doc.pageController.page.id + "\" >", "page content");
                returnHtml = getContentBox_content(core, OrderByClause, AllowChildPageList, AllowReturnLink, ArchivePages, ignoreme, UseContentWatchLink, allowPageWithoutSectionDisplay);
                //
                // ----- If Link field populated, do redirect
                if (core.doc.pageController.page.PageLink != "") {
                    core.doc.pageController.page.Clicks += 1;
                    core.doc.pageController.page.save(core);
                    core.doc.redirectLink = core.doc.pageController.page.PageLink;
                    core.doc.redirectReason = "Redirect required because this page (PageRecordID=" + core.doc.pageController.page.id + ") has a Link Override [" + core.doc.pageController.page.PageLink + "].";
                    core.doc.redirectBecausePageNotFound = false;
                    return core.webServer.redirect(core.doc.redirectLink, core.doc.redirectReason, core.doc.redirectBecausePageNotFound);
                }
                //
                // -- build list of blocked pages
                string BlockedRecordIDList = "";
                if ((!string.IsNullOrEmpty(returnHtml)) && (core.doc.redirectLink == "")) {
                    foreach (pageContentModel testPage in core.doc.pageController.pageToRootList) {
                        if (testPage.BlockContent | testPage.BlockPage) {
                            BlockedRecordIDList = BlockedRecordIDList + "," + testPage.id;
                        }
                    }
                    if (!string.IsNullOrEmpty(BlockedRecordIDList)) {
                        BlockedRecordIDList = BlockedRecordIDList.Substring(1);
                    }
                }
                int CS = 0;
                bool ContentBlocked = false;
                //
                // ----- Content Blocking
                if (!string.IsNullOrEmpty(BlockedRecordIDList)) {
                    if (core.session.isAuthenticatedAdmin(core)) {
                        //
                        // Administrators are never blocked
                        //
                    } else if (!core.session.isAuthenticated) {
                        //
                        // non-authenticated are always blocked
                        //
                        ContentBlocked = true;
                    } else {
                        //
                        // Check Access Groups, if in access groups, remove group from BlockedRecordIDList
                        //
                        string SQL = "SELECT DISTINCT ccPageContentBlockRules.RecordID"
                            + " FROM (ccPageContentBlockRules"
                            + " LEFT JOIN ccgroups ON ccPageContentBlockRules.GroupID = ccgroups.ID)"
                            + " LEFT JOIN ccMemberRules ON ccgroups.ID = ccMemberRules.GroupID"
                            + " WHERE (((ccMemberRules.MemberID)=" + core.db.encodeSQLNumber(core.session.user.id) + ")"
                            + " AND ((ccPageContentBlockRules.RecordID) In (" + BlockedRecordIDList + "))"
                            + " AND ((ccPageContentBlockRules.Active)<>0)"
                            + " AND ((ccgroups.Active)<>0)"
                            + " AND ((ccMemberRules.Active)<>0)"
                            + " AND ((ccMemberRules.DateExpires) Is Null Or (ccMemberRules.DateExpires)>" + core.db.encodeSQLDate(core.doc.profileStartTime) + "));";
                        CS = core.db.csOpenSql(SQL);
                        BlockedRecordIDList = "," + BlockedRecordIDList;
                        while (core.db.csOk(CS)) {
                            BlockedRecordIDList = genericController.vbReplace(BlockedRecordIDList, "," + core.db.csGetText(CS, "RecordID"), "");
                            core.db.csGoNext(CS);
                        }
                        core.db.csClose(ref CS);
                        if (!string.IsNullOrEmpty(BlockedRecordIDList)) {
                            //
                            // ##### remove the leading comma
                            BlockedRecordIDList = BlockedRecordIDList.Substring(1);
                            // Check the remaining blocked records against the members Content Management
                            // ##### removed hardcoded mistakes from the sql
                            SQL = "SELECT DISTINCT ccPageContent.ID as RecordID"
                                + " FROM ((ccPageContent"
                                + " LEFT JOIN ccGroupRules ON ccPageContent.ContentControlID = ccGroupRules.ContentID)"
                                + " LEFT JOIN ccgroups AS ManagementGroups ON ccGroupRules.GroupID = ManagementGroups.ID)"
                                + " LEFT JOIN ccMemberRules AS ManagementMemberRules ON ManagementGroups.ID = ManagementMemberRules.GroupID"
                                + " WHERE (((ccPageContent.ID) In (" + BlockedRecordIDList + "))"
                                + " AND ((ccGroupRules.Active)<>0)"
                                + " AND ((ManagementGroups.Active)<>0)"
                                + " AND ((ManagementMemberRules.Active)<>0)"
                                + " AND ((ManagementMemberRules.DateExpires) Is Null Or (ManagementMemberRules.DateExpires)>" + core.db.encodeSQLDate(core.doc.profileStartTime) + ")"
                                + " AND ((ManagementMemberRules.MemberID)=" + core.session.user.id + " ));";
                            CS = core.db.csOpenSql(SQL);
                            while (core.db.csOk(CS)) {
                                BlockedRecordIDList = genericController.vbReplace(BlockedRecordIDList, "," + core.db.csGetText(CS, "RecordID"), "");
                                core.db.csGoNext(CS);
                            }
                            core.db.csClose(ref CS);
                        }
                        if (!string.IsNullOrEmpty(BlockedRecordIDList)) {
                            ContentBlocked = true;
                        }
                        core.db.csClose(ref CS);
                    }
                }
                int PageRecordID = 0;
                //
                //
                //
                if (ContentBlocked) {
                    string CustomBlockMessageFilename = "";
                    int BlockSourceID = main_BlockSourceDefaultMessage;
                    int ContentPadding = 20;
                    string[] BlockedPages = BlockedRecordIDList.Split(',');
                    int BlockedPageRecordID = genericController.encodeInteger(BlockedPages[BlockedPages.GetUpperBound(0)]);
                    int RegistrationGroupID = 0;
                    if (BlockedPageRecordID != 0) {
                        CS = core.db.csOpenRecord("Page Content", BlockedPageRecordID, false, false, "CustomBlockMessage,BlockSourceID,RegistrationGroupID,ContentPadding");
                        if (core.db.csOk(CS)) {
                            BlockSourceID = core.db.csGetInteger(CS, "BlockSourceID");
                            ContentPadding = core.db.csGetInteger(CS, "ContentPadding");
                            CustomBlockMessageFilename = core.db.csGetText(CS, "CustomBlockMessage");
                            RegistrationGroupID = core.db.csGetInteger(CS, "RegistrationGroupID");
                        }
                        core.db.csClose(ref CS);
                    }
                    //
                    // Block Appropriately
                    //
                    switch (BlockSourceID) {
                        case main_BlockSourceCustomMessage: {
                                //
                                // ----- Custom Message
                                //
                                returnHtml = core.cdnFiles.readFileText(CustomBlockMessageFilename);
                                break;
                            }
                        case main_BlockSourceLogin: {
                                //
                                // ----- Login page
                                //
                                string BlockForm = "";
                                if (!core.session.isAuthenticated) {
                                    if (!core.session.isRecognized(core)) {
                                        //
                                        // -- not recognized
                                        BlockForm = ""
                                            + "<p>This content has limited access. If you have an account, please login using this form.</p>"
                                            + core.addon.execute(addonModel.create(core, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext {
                                                addonType = CPUtilsBaseClass.addonContext.ContextPage,
                                                errorContextMessage = "calling login form addon [" + addonGuidLoginPage + "] because content box is blocked and user not recognized"
                                            });
                                    } else {
                                        //
                                        // -- recognized, not authenticated
                                        BlockForm = ""
                                            + "<p>This content has limited access. You were recognized as \"<b>" + core.session.user.name + "</b>\", but you need to login to continue. To login to this account or another, please use this form.</p>"
                                            + core.addon.execute(addonModel.create(core, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext {
                                                addonType = CPUtilsBaseClass.addonContext.ContextPage,
                                                errorContextMessage = "calling login form addon [" + addonGuidLoginPage + "] because content box is blocked and user not authenticated"
                                            });
                                    }
                                } else {
                                    //
                                    // -- authenticated
                                    BlockForm = ""
                                        + "<p>You are currently logged in as \"<b>" + core.session.user.name + "</b>\". If this is not you, please <a href=\"?" + core.doc.refreshQueryString + "&method=logout\" rel=\"nofollow\">Click Here</a>.</p>"
                                        + "<p>This account does not have access to this content. If you want to login with a different account, please use this form.</p>"
                                        + core.addon.execute(addonModel.create(core, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext {
                                            addonType = CPUtilsBaseClass.addonContext.ContextPage,
                                            errorContextMessage = "calling login form addon [" + addonGuidLoginPage + "] because content box is blocked and user does not have access to content"
                                        });
                                }
                                returnHtml = ""
                                    + "<div style=\"margin: 100px, auto, auto, auto;text-align:left;\">"
                                    + errorController.getUserError(core) + BlockForm + "</div>";
                                break;
                            }
                        case main_BlockSourceRegistration: {
                                //
                                // ----- Registration
                                //
                                string BlockForm = "";
                                if (core.docProperties.getInteger("subform") == main_BlockSourceLogin) {
                                    //
                                    // login subform form
                                    BlockForm = ""
                                        + "<p>This content has limited access. If you have an account, please login using this form.</p>"
                                        + "<p>If you do not have an account, <a href=?" + core.doc.refreshQueryString + "&subform=0>click here to register</a>.</p>"
                                        + core.addon.execute(addonModel.create(core, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext {
                                            addonType = CPUtilsBaseClass.addonContext.ContextPage,
                                            errorContextMessage = "calling login form addon [" + addonGuidLoginPage + "] because content box is blocked for registration"
                                        });
                                } else {
                                    //
                                    // Register Form
                                    //
                                    if (!core.session.isAuthenticated & core.session.isRecognized(core)) {
                                        //
                                        // -- Can not take the chance, if you go to a registration page, and you are recognized but not auth -- logout first
                                        core.session.logout(core);
                                    }
                                    if (!core.session.isAuthenticated) {
                                        //
                                        // -- Not Authenticated
                                        core.doc.verifyRegistrationFormPage(core);
                                        BlockForm = ""
                                            + "<p>This content has limited access. If you have an account, <a href=?" + core.doc.refreshQueryString + "&subform=" + main_BlockSourceLogin + ">Click Here to login</a>.</p>"
                                            + "<p>To view this content, please complete this form.</p>"
                                            + getFormPage(core, "Registration Form", RegistrationGroupID) + "";
                                    } else {
                                        //
                                        // -- Authenticated
                                        core.doc.verifyRegistrationFormPage(core);
                                        string BlockCopy = ""
                            + "<p>You are currently logged in as \"<b>" + core.session.user.name + "</b>\". If this is not you, please <a href=\"?" + core.doc.refreshQueryString + "&method=logout\" rel=\"nofollow\">Click Here</a>.</p>"
                            + "<p>This account does not have access to this content. To view this content, please complete this form.</p>"
                            + getFormPage(core, "Registration Form", RegistrationGroupID) + "";
                                    }
                                }
                                returnHtml = ""
                                    + "<div style=\"margin: 100px, auto, auto, auto;text-align:left;\">"
                                    + errorController.getUserError(core) + BlockForm + "</div>";
                                break;
                            }
                        default: {
                                //
                                // ----- Content as blocked - convert from site property to content page
                                //
                                returnHtml = getDefaultBlockMessage(core, UseContentWatchLink);
                                break;
                            }
                    }
                    //
                    // If the output is blank, put default message in
                    //
                    if (string.IsNullOrEmpty(returnHtml)) {
                        returnHtml = getDefaultBlockMessage(core, UseContentWatchLink);
                    }
                    //
                    // Encode the copy
                    //
                    //returnHtml = contentCmdController.executeContentCommands(core, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, core.sessionContext.user.id, core.sessionContext.isAuthenticated, ref layoutError);
                    returnHtml = activeContentController.renderHtmlForWeb(core, returnHtml, pageContentModel.contentName, PageRecordID, core.doc.pageController.page.ContactMemberID, "http://" + core.webServer.requestDomain, core.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage);
                    if (core.doc.refreshQueryString != "") {
                        returnHtml = genericController.vbReplace(returnHtml, "?method=login", "?method=Login&" + core.doc.refreshQueryString, 1, 99, 1);
                    }
                    //
                    // Add in content padding required for integration with the template
                    returnHtml = getContentBoxWrapper(core, returnHtml, ContentPadding);
                }
                //
                // ----- Encoding, Tracking and Triggers
                if (!ContentBlocked) {
                    if (core.visitProperty.getBoolean("AllowQuickEditor")) {
                        //
                        // Quick Editor, no encoding or tracking
                        //
                    } else {
                        int pageViewings = core.doc.pageController.page.Viewings;
                        if (core.session.isEditing(pageContentModel.contentName) | core.visitProperty.getBoolean("AllowWorkflowRendering")) {
                            //
                            // Link authoring, workflow rendering -> do encoding, but no tracking
                            //
                            //returnHtml = contentCmdController.executeContentCommands(core, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, core.sessionContext.user.id, core.sessionContext.isAuthenticated, ref layoutError);
                            returnHtml = activeContentController.renderHtmlForWeb(core, returnHtml, pageContentModel.contentName, PageRecordID, core.doc.pageController.page.ContactMemberID, "http://" + core.webServer.requestDomain, core.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage);
                        } else {
                            //
                            // Live content
                            //returnHtml = contentCmdController.executeContentCommands(core, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, core.sessionContext.user.id, core.sessionContext.isAuthenticated, ref layoutError);
                            returnHtml = activeContentController.renderHtmlForWeb(core, returnHtml, pageContentModel.contentName, PageRecordID, core.doc.pageController.page.ContactMemberID, "http://" + core.webServer.requestDomain, core.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage);
                            core.db.executeQuery("update ccpagecontent set viewings=" + (pageViewings + 1) + " where id=" + core.doc.pageController.page.id);
                        }
                        //
                        // Page Hit Notification
                        //
                        if ((!core.session.visit.ExcludeFromAnalytics) & (core.doc.pageController.page.ContactMemberID != 0) && (core.webServer.requestBrowser.IndexOf("kmahttp", System.StringComparison.OrdinalIgnoreCase)  == -1)) {
                            personModel person = personModel.create(core, core.doc.pageController.page.ContactMemberID);
                            if ( person != null ) {
                                if (core.doc.pageController.page.AllowHitNotification) {
                                    string PageName = core.doc.pageController.page.name;
                                    if (string.IsNullOrEmpty(PageName)) {
                                        PageName = core.doc.pageController.page.MenuHeadline;
                                        if (string.IsNullOrEmpty(PageName)) {
                                            PageName = core.doc.pageController.page.Headline;
                                            if (string.IsNullOrEmpty(PageName)) {
                                                PageName = "[no name]";
                                            }
                                        }
                                    }
                                    string Body = "";
                                    Body = Body + "<p><b>Page Hit Notification.</b></p>";
                                    Body = Body + "<p>This email was sent to you by the Contensive Server as a notification of the following content viewing details.</p>";
                                    Body = Body + htmlController.tableStart(4, 1, 1);
                                    Body = Body + "<tr><td align=\"right\" width=\"150\" Class=\"ccPanelHeader\">Description<br><img alt=\"image\" src=\"http://" + core.webServer.requestDomain + "/ccLib/images/spacer.gif\" width=\"150\" height=\"1\"></td><td align=\"left\" width=\"100%\" Class=\"ccPanelHeader\">Value</td></tr>";
                                    Body = Body + get2ColumnTableRow("Domain", core.webServer.requestDomain, true);
                                    Body = Body + get2ColumnTableRow("Link", core.webServer.requestUrl, false);
                                    Body = Body + get2ColumnTableRow("Page Name", PageName, true);
                                    Body = Body + get2ColumnTableRow("Member Name", core.session.user.name, false);
                                    Body = Body + get2ColumnTableRow("Member #", encodeText(core.session.user.id), true);
                                    Body = Body + get2ColumnTableRow("Visit Start Time", encodeText(core.session.visit.StartTime), false);
                                    Body = Body + get2ColumnTableRow("Visit #", encodeText(core.session.visit.id), true);
                                    Body = Body + get2ColumnTableRow("Visit IP", core.webServer.requestRemoteIP, false);
                                    Body = Body + get2ColumnTableRow("Browser ", core.webServer.requestBrowser, true);
                                    Body = Body + get2ColumnTableRow("Visitor #", encodeText(core.session.visitor.id), false);
                                    Body = Body + get2ColumnTableRow("Visit Authenticated", encodeText(core.session.visit.VisitAuthenticated), true);
                                    Body = Body + get2ColumnTableRow("Visit Referrer", core.session.visit.HTTP_REFERER, false);
                                    Body = Body + kmaEndTable;
                                    string queryStringForLinkAppend = "";
                                    string emailStatus = "";
                                    emailController.queuePersonEmail(core, person, core.siteProperties.getText("EmailFromAddress", "info@" + core.webServer.requestDomain), "Page Hit Notification", Body, "", "", false, true, 0, "", false, ref emailStatus, queryStringForLinkAppend);
                                }
                            }
                        }
                        //
                        // -- Process Trigger Conditions
                        int ConditionID = core.doc.pageController.page.TriggerConditionID;
                        int ConditionGroupID = core.doc.pageController.page.TriggerConditionGroupID;
                        int main_AddGroupID = core.doc.pageController.page.TriggerAddGroupID;
                        int RemoveGroupID = core.doc.pageController.page.TriggerRemoveGroupID;
                        int SystemEMailID = core.doc.pageController.page.TriggerSendSystemEmailID;
                        switch (ConditionID) {
                            case 1:
                                //
                                // Always
                                //
                                if (SystemEMailID != 0) {
                                    emailController.queueSystemEmail(core, core.db.getRecordName("System Email", SystemEMailID), "", core.session.user.id);
                                }
                                if (main_AddGroupID != 0) {
                                    groupController.group_AddGroupMember(core, groupController.group_GetGroupName(core, main_AddGroupID));
                                }
                                if (RemoveGroupID != 0) {
                                    groupController.group_DeleteGroupMember(core, groupController.group_GetGroupName(core, RemoveGroupID));
                                }
                                break;
                            case 2:
                                //
                                // If in Condition Group
                                //
                                if (ConditionGroupID != 0) {
                                    if (core.session.isMemberOfGroup(core, groupController.group_GetGroupName(core, ConditionGroupID))) {
                                        if (SystemEMailID != 0) {
                                            emailController.queueSystemEmail(core, core.db.getRecordName("System Email", SystemEMailID), "", core.session.user.id);
                                        }
                                        if (main_AddGroupID != 0) {
                                            groupController.group_AddGroupMember(core, groupController.group_GetGroupName(core, main_AddGroupID));
                                        }
                                        if (RemoveGroupID != 0) {
                                            groupController.group_DeleteGroupMember(core, groupController.group_GetGroupName(core, RemoveGroupID));
                                        }
                                    }
                                }
                                break;
                            case 3:
                                //
                                // If not in Condition Group
                                //
                                if (ConditionGroupID != 0) {
                                    if (!core.session.isMemberOfGroup(core, groupController.group_GetGroupName(core, ConditionGroupID))) {
                                        if (main_AddGroupID != 0) {
                                            groupController.group_AddGroupMember(core, groupController.group_GetGroupName(core, main_AddGroupID));
                                        }
                                        if (RemoveGroupID != 0) {
                                            groupController.group_DeleteGroupMember(core, groupController.group_GetGroupName(core, RemoveGroupID));
                                        }
                                        if (SystemEMailID != 0) {
                                            emailController.queueSystemEmail(core, core.db.getRecordName("System Email", SystemEMailID), "", core.session.user.id);
                                        }
                                    }
                                }
                                break;
                        }
                        //End If
                        //Call app.closeCS(CS)
                    }
                    //
                    //---------------------------------------------------------------------------------
                    // ----- Add in ContentPadding (a table around content with the appropriate padding added)
                    //---------------------------------------------------------------------------------
                    //
                    returnHtml = getContentBoxWrapper(core, returnHtml, core.doc.pageController.page.ContentPadding);
                    DateTime DateModified = default(DateTime);
                    //
                    //---------------------------------------------------------------------------------
                    // ----- Set Headers
                    //---------------------------------------------------------------------------------
                    //
                    if (DateModified != DateTime.MinValue) {
                        core.webServer.addResponseHeader("LAST-MODIFIED", genericController.GetRFC1123PatternDateFormat(DateModified));
                    }
                    //
                    //---------------------------------------------------------------------------------
                    // ----- Store page javascript
                    //---------------------------------------------------------------------------------
                    // todo -- assets should all come from addons !!!
                    //
                    core.html.addScriptCode_onLoad(core.doc.pageController.page.JSOnLoad, "page content");
                    core.html.addScriptCode(core.doc.pageController.page.JSHead, "page content");
                    if (core.doc.pageController.page.JSFilename != "") {
                        core.html.addScriptLinkSrc(genericController.getCdnFileLink(core, core.doc.pageController.page.JSFilename), "page content");
                    }
                    core.html.addScriptCode(core.doc.pageController.page.JSEndBody, "page content");
                    //
                    //---------------------------------------------------------------------------------
                    // Set the Meta Content flag
                    //---------------------------------------------------------------------------------
                    //
                    core.html.addTitle(htmlController.encodeHtml(core.doc.pageController.page.pageTitle), "page content");
                    core.html.addMetaDescription(htmlController.encodeHtml(core.doc.pageController.page.metaDescription), "page content");
                    core.html.addHeadTag(core.doc.pageController.page.OtherHeadTags, "page content");
                    core.html.addMetaKeywordList(core.doc.pageController.page.MetaKeywordList, "page content");
                    //
                    Dictionary<string, string> instanceArguments = new Dictionary<string, string> {
                        { "CSPage", "-1" }
                    };
                    //
                    // -- OnPageStartEvent
                    core.doc.bodyContent = returnHtml;
                    List<addonModel> addonList = addonModel.createList_OnPageStartEvent(core, new List<string>());
                    foreach (Models.DbModels.addonModel addon in addonList) {
                        CPUtilsBaseClass.addonExecuteContext pageStartContext = new CPUtilsBaseClass.addonExecuteContext() {
                            instanceGuid = "-1",
                            instanceArguments = instanceArguments,
                            addonType = CPUtilsBaseClass.addonContext.ContextOnPageStart,
                            errorContextMessage = "calling start page addon [" + addon.name + "]"
                        };
                        core.doc.bodyContent = core.addon.execute(addon, pageStartContext) + core.doc.bodyContent;
                    }
                    returnHtml = core.doc.bodyContent;
                    //
                    // -- OnPageEndEvent / filter
                    core.doc.bodyContent = returnHtml;
                    foreach (addonModel addon in core.addonCache.getOnPageEndAddonList()) {
                        CPUtilsBaseClass.addonExecuteContext pageEndContext = new CPUtilsBaseClass.addonExecuteContext() {
                            instanceGuid = "-1",
                            instanceArguments = instanceArguments,
                            addonType = CPUtilsBaseClass.addonContext.ContextOnPageEnd,
                            errorContextMessage = "calling start page addon [" + addon.name + "]"
                        };

                        core.doc.bodyContent += core.addon.execute(addon, pageEndContext);
                    }
                    returnHtml = core.doc.bodyContent;
                    //
                }
                //
                // -- title
                core.html.addTitle(core.doc.pageController.page.name);
                //
                // -- add contentid and sectionid
                core.html.addHeadTag("<meta name=\"contentId\" content=\"" + core.doc.pageController.page.id + "\" >", "page content");
                //
                // Display Admin Warnings with Edits for record errors
                //
                if (core.doc.adminWarning != "") {
                    //
                    if (core.doc.adminWarningPageID != 0) {
                        core.doc.adminWarning = core.doc.adminWarning + "</p>" + adminUIController.getRecordEditLink(core, "Page Content", core.doc.adminWarningPageID, true, "Page " + core.doc.adminWarningPageID, core.session.isAuthenticatedAdmin(core)) + "&nbsp;Edit the page<p>";
                        core.doc.adminWarningPageID = 0;
                    }
                    returnHtml = ""
                    + core.html.getAdminHintWrapper(core.doc.adminWarning) + returnHtml + "";
                    core.doc.adminWarning = "";
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return returnHtml;
        }
        //
        //====================================================================================================
        /// <summary>
        /// GetHtmlBody_GetSection_GetContentBox
        /// </summary>
        /// <param name="PageID"></param>
        /// <param name="rootPageId"></param>
        /// <param name="RootPageContentName"></param>
        /// <param name="OrderByClause"></param>
        /// <param name="AllowChildPageList"></param>
        /// <param name="AllowReturnLink"></param>
        /// <param name="ArchivePages"></param>
        /// <param name="ignoreMe"></param>
        /// <param name="UseContentWatchLink"></param>
        /// <param name="allowPageWithoutSectionDisplay"></param>
        /// <returns></returns>
        //
        internal static string getContentBox_content(coreController core, string OrderByClause, bool AllowChildPageList, bool AllowReturnLink, bool ArchivePages, int ignoreMe, bool UseContentWatchLink, bool allowPageWithoutSectionDisplay) {
            string result = "";
            try {
                if (core.doc.continueProcessing) {
                    if (core.doc.redirectLink == "") {
                        bool isEditing = core.session.isEditing(pageContentModel.contentName);
                        //
                        // ----- Render the Body
                        string LiveBody = getContentBox_content_Body(core, OrderByClause, AllowChildPageList, false, core.doc.pageController.pageToRootList.Last().id, AllowReturnLink, pageContentModel.contentName, ArchivePages);
                        bool isRootPage = (core.doc.pageController.pageToRootList.Count == 1);
                        if (core.session.isAdvancedEditing(core, "")) {
                            result += adminUIController.getRecordEditLink(core, pageContentModel.contentName, core.doc.pageController.page.id, (!isRootPage)) + LiveBody;
                        } else if (isEditing) {
                            result += adminUIController.getEditWrapper( core, "", adminUIController.getRecordEditLink(core, pageContentModel.contentName, core.doc.pageController.page.id, (!isRootPage)) + LiveBody);
                        } else {
                            result += LiveBody;
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// render the page content
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="ContentID"></param>
        /// <param name="OrderByClause"></param>
        /// <param name="AllowChildList"></param>
        /// <param name="Authoring"></param>
        /// <param name="rootPageId"></param>
        /// <param name="AllowReturnLink"></param>
        /// <param name="RootPageContentName"></param>
        /// <param name="ArchivePage"></param>
        /// <returns></returns>

        internal static string getContentBox_content_Body(coreController core, string OrderByClause, bool AllowChildList, bool Authoring, int rootPageId, bool AllowReturnLink, string RootPageContentName, bool ArchivePage) {
            string result = "";
            try {
                bool allowChildListComposite = AllowChildList && core.doc.pageController.page.AllowChildListDisplay;
                bool allowReturnLinkComposite = AllowReturnLink && core.doc.pageController.page.AllowReturnLinkDisplay;
                string bodyCopy = core.doc.pageController.page.Copyfilename.content;
                string breadCrumb = "";
                string BreadCrumbDelimiter = null;
                string BreadCrumbPrefix = null;
                bool isRootPage = core.doc.pageController.pageToRootList.Count.Equals(1);
                //
                if (allowReturnLinkComposite && (!isRootPage)) {
                    //
                    // ----- Print Heading if not at root Page
                    //
                    BreadCrumbPrefix = core.siteProperties.getText("BreadCrumbPrefix", "Return to");
                    BreadCrumbDelimiter = core.siteProperties.getText("BreadCrumbDelimiter", " &gt; ");
                    breadCrumb = core.doc.getReturnBreadcrumb(RootPageContentName, core.doc.pageController.page.ParentID, rootPageId, "", ArchivePage, BreadCrumbDelimiter);
                    if (!string.IsNullOrEmpty(breadCrumb)) {
                        breadCrumb = "\r<p class=\"ccPageListNavigation\">" + BreadCrumbPrefix + " " + breadCrumb + "</p>";
                    }
                }
                result += breadCrumb;
                //
                if (true) {
                    string IconRow = "";
                    if ((!core.session.visit.Bot) & (core.doc.pageController.page.AllowPrinterVersion | core.doc.pageController.page.AllowEmailPage)) {
                        //
                        // not a bot, and either print or email allowed
                        //
                        if (core.doc.pageController.page.AllowPrinterVersion) {
                            string QueryString = core.doc.refreshQueryString;
                            QueryString = genericController.modifyQueryString(QueryString, rnPageId, genericController.encodeText(core.doc.pageController.page.id), true);
                            QueryString = genericController.modifyQueryString(QueryString, RequestNameHardCodedPage, HardCodedPagePrinterVersion, true);
                            string Caption = core.siteProperties.getText("PagePrinterVersionCaption", "Printer Version");
                            Caption = genericController.vbReplace(Caption, " ", "&nbsp;");
                            IconRow = IconRow + "\r&nbsp;&nbsp;<a href=\"" + htmlController.encodeHtml(core.webServer.requestPage + "?" + QueryString) + "\" target=\"_blank\"><img alt=\"image\" src=\"/ccLib/images/IconSmallPrinter.gif\" width=\"13\" height=\"13\" border=\"0\" align=\"absmiddle\"></a>&nbsp<a href=\"" + htmlController.encodeHtml(core.webServer.requestPage + "?" + QueryString) + "\" target=\"_blank\" style=\"text-decoration:none! important;font-family:sanserif,verdana,helvetica;font-size:11px;\">" + Caption + "</a>";
                        }
                        if (core.doc.pageController.page.AllowEmailPage) {
                            string QueryString = core.doc.refreshQueryString;
                            if (!string.IsNullOrEmpty(QueryString)) {
                                QueryString = "?" + QueryString;
                            }
                            string EmailBody = core.webServer.requestProtocol + core.webServer.requestDomain + core.webServer.requestPathPage + QueryString;
                            string Caption = core.siteProperties.getText("PageAllowEmailCaption", "Email This Page");
                            Caption = genericController.vbReplace(Caption, " ", "&nbsp;");
                            IconRow = IconRow + "\r&nbsp;&nbsp;<a HREF=\"mailto:?SUBJECT=You might be interested in this&amp;BODY=" + EmailBody + "\"><img alt=\"image\" src=\"/ccLib/images/IconSmallEmail.gif\" width=\"13\" height=\"13\" border=\"0\" align=\"absmiddle\"></a>&nbsp;<a HREF=\"mailto:?SUBJECT=You might be interested in this&amp;BODY=" + EmailBody + "\" style=\"text-decoration:none! important;font-family:sanserif,verdana,helvetica;font-size:11px;\">" + Caption + "</a>";
                        }
                    }
                    if (!string.IsNullOrEmpty(IconRow)) {
                        result += "\r<div style=\"text-align:right;\">"
                        + genericController.nop(IconRow) + "\r</div>";
                    }
                }
                //
                // ----- Start Text Search
                //
                string Cell = "";
                if (core.session.isQuickEditing(core, pageContentModel.contentName)) {
                    Cell = Cell + quickEditController.getQuickEditing(core, rootPageId, OrderByClause, AllowChildList, AllowReturnLink, ArchivePage, core.doc.pageController.page.ContactMemberID, core.doc.pageController.page.ChildListSortMethodID, allowChildListComposite, ArchivePage);
                } else {
                    //
                    // ----- Headline
                    //
                    if (core.doc.pageController.page.Headline != "") {
                        string headline = htmlController.encodeHtml(core.doc.pageController.page.Headline);
                        Cell = Cell + "\r<h1>" + headline + "</h1>";
                        //
                        // Add AC end here to force the end of any left over AC tags (like language)
                        Cell = Cell + ACTagEnd;
                    }
                    //
                    // ----- Page Copy
                    if (string.IsNullOrEmpty(bodyCopy)) {
                        //
                        // Page copy is empty if  Links Enabled put in a blank line to separate edit from add tag
                        if (core.session.isEditing(pageContentModel.contentName)) {
                            bodyCopy = "\r<p><!-- Empty Content Placeholder --></p>";
                        }
                    } else {
                        bodyCopy = bodyCopy + "\r" + ACTagEnd;
                    }
                    //
                    // ----- Wrap content body
                    Cell = Cell + "\r<!-- ContentBoxBodyStart -->"
                        + genericController.nop(bodyCopy) + "\r<!-- ContentBoxBodyEnd -->";
                    //
                    // ----- Child pages
                    if (allowChildListComposite || core.session.isEditingAnything()) {
                        if (!allowChildListComposite) {
                            Cell = Cell + core.html.getAdminHintWrapper("Automatic Child List display is disabled for this page. It is displayed here because you are in editing mode. To enable automatic child list display, see the features tab for this page.");
                        }
                        addonModel addon = addonModel.create(core, addonGuidChildList);
                        CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                            addonType = CPUtilsBaseClass.addonContext.ContextPage,
                            hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                                contentName = pageContentModel.contentName,
                                fieldName = "",
                                recordId = core.doc.pageController.page.id
                            },
                            instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(core, core.doc.pageController.page.ChildListInstanceOptions),
                            instanceGuid = PageChildListInstanceID,
                            wrapperID = core.siteProperties.defaultWrapperID,
                            errorContextMessage = "executing child list addon for page [" + core.doc.pageController.page.id + "]"
                        };
                        Cell += core.addon.execute(addon, executeContext);
                        //Cell = Cell & core.addon.execute_legacy2(core.siteProperties.childListAddonID, "", core.doc.pageController.page.ChildListInstanceOptions, CPUtilsBaseClass.addonContext.ContextPage, pageContentModel.contentName, core.doc.pageController.page.id, "", PageChildListInstanceID, False, core.siteProperties.defaultWrapperID, "", AddonStatusOK, Nothing)
                    }
                }
                //
                // ----- End Text Search
                result += "\r<!-- TextSearchStart -->"
                    + genericController.nop(Cell) + "\r<!-- TextSearchEnd -->";
                //
                // ----- Page See Also
                if (core.doc.pageController.page.AllowSeeAlso) {
                    result += "\r<div>"
                        + genericController.nop(getSeeAlso(core, pageContentModel.contentName, core.doc.pageController.page.id)) + "\r</div>";
                }
                //
                // ----- Allow More Info
                if ((core.doc.pageController.page.ContactMemberID != 0) & core.doc.pageController.page.AllowMoreInfo) {
                    result += "\r<ac TYPE=\"" + ACTypeContact + "\">";
                }
                //
                // ----- Feedback
                if ((core.doc.pageController.page.ContactMemberID != 0) & core.doc.pageController.page.AllowFeedback) {
                    result += "\r<ac TYPE=\"" + ACTypeFeedback + "\">";
                }
                //
                // ----- Last Modified line
                if ((core.doc.pageController.page.modifiedDate != DateTime.MinValue) & core.doc.pageController.page.AllowLastModifiedFooter) {
                    result += "\r<p>This page was last modified " + core.doc.pageController.page.modifiedDate.ToString("G");
                    if (core.session.isAuthenticatedAdmin(core)) {
                        if (core.doc.pageController.page.modifiedBy == 0) {
                            result += " (admin only: modified by unknown)";
                        } else {
                            string personName = core.db.getRecordName("people", core.doc.pageController.page.modifiedBy);
                            if (string.IsNullOrEmpty(personName)) {
                                result += " (admin only: modified by person with unnamed or deleted record #" + core.doc.pageController.page.modifiedBy + ")";
                            } else {
                                result += " (admin only: modified by " + personName + ")";
                            }
                        }
                    }
                    result += "</p>";
                }
                //
                // ----- Last Reviewed line
                if ((core.doc.pageController.page.DateReviewed != DateTime.MinValue) & core.doc.pageController.page.AllowReviewedFooter) {
                    result += "\r<p>This page was last reviewed " + core.doc.pageController.page.DateReviewed.ToString("");
                    if (core.session.isAuthenticatedAdmin(core)) {
                        if (core.doc.pageController.page.ReviewedBy == 0) {
                            result += " (by unknown)";
                        } else {
                            string personName = core.db.getRecordName("people", core.doc.pageController.page.ReviewedBy);
                            if (string.IsNullOrEmpty(personName)) {
                                result += " (by person with unnamed or deleted record #" + core.doc.pageController.page.ReviewedBy + ")";
                            } else {
                                result += " (by " + personName + ")";
                            }
                        }
                        result += ".</p>";
                    }
                }
                //
                // ----- Page Content Message Footer
                if (core.doc.pageController.page.AllowMessageFooter) {
                    string pageContentMessageFooter = core.siteProperties.getText("PageContentMessageFooter", "");
                    if (!string.IsNullOrEmpty(pageContentMessageFooter)) {
                        result += "\r<p>" + pageContentMessageFooter + "</p>";
                    }
                }
                //Call core.db.cs_Close(CS)
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        // Print the See Also listing
        //   ContentName is the name of the parent table
        //   RecordID is the parent RecordID
        //=============================================================================
        //
        public static string getSeeAlso(coreController core, string ContentName, int RecordID) {
            string result = "";
            try {
                if (RecordID > 0) {
                    int ContentID = Models.Complex.cdefModel.getContentId(core, ContentName);
                    bool IsEditingLocal = false;
                    if (ContentID > 0) {
                        //
                        // ----- Set authoring only for valid ContentName
                        IsEditingLocal = core.session.isEditing(ContentName);
                    } else {
                        //
                        // ----- if iContentName was bad, maybe they put table in, no authoring
                        ContentID = Models.Complex.cdefModel.getContentIDByTablename(core, ContentName);
                    }
                    int SeeAlsoCount = 0;
                    if (ContentID > 0) {
                        //
                        int CS = core.db.csOpen("See Also", "((active<>0)AND(ContentID=" + ContentID + ")AND(RecordID=" + RecordID + "))");
                        while (core.db.csOk(CS)) {
                            string SeeAlsoLink = (core.db.csGetText(CS, "Link"));
                            if (!string.IsNullOrEmpty(SeeAlsoLink)) {
                                result += "\r<li class=\"ccListItem\">";
                                if (genericController.vbInstr(1, SeeAlsoLink, "://") == 0) {
                                    SeeAlsoLink = core.webServer.requestProtocol + SeeAlsoLink;
                                }
                                if (IsEditingLocal) {
                                    result += adminUIController.getRecordEditLink(core, "See Also", (core.db.csGetInteger(CS, "ID")), false, "", core.session.isEditing("See Also"));
                                }
                                result += "<a href=\"" + htmlController.encodeHtml(SeeAlsoLink) + "\" target=\"_blank\">" + (core.db.csGetText(CS, "Name")) + "</A>";
                                string Copy = (core.db.csGetText(CS, "Brief"));
                                if (!string.IsNullOrEmpty(Copy)) {
                                    result += "<br>" + htmlController.span(Copy, "ccListCopy");
                                }
                                SeeAlsoCount = SeeAlsoCount + 1;
                                result += "</li>";
                            }
                            core.db.csGoNext(CS);
                        }
                        core.db.csClose(ref CS);
                        //
                        if (IsEditingLocal) {
                            SeeAlsoCount = SeeAlsoCount + 1;
                            result += "\r<li class=\"ccListItem\">" + adminUIController.getRecordAddLink(core, "See Also", "RecordID=" + RecordID + ",ContentID=" + ContentID) + "</LI>";
                        }
                    }
                    //
                    if (SeeAlsoCount == 0) {
                        result = "";
                    } else {
                        result = "<p>See Also\r<ul class=\"ccList\">" + genericController.nop(result) + "\r</ul></p>";
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //========================================================================
        // ----- prints a link to the feedback popup form
        //
        //   Creates a sub-form that when submitted, is logged by the notes
        //   system (in MembersLib right now). When submitted, it prints a thank you
        //   message.
        //
        //========================================================================
        //
        public static string getFeedbackForm(coreController core, string ContentName, int RecordID, int ToMemberID, string headline = "") {
            string result = "";
            try {
                const string FeedbackButtonSubmit = "Submit";
                //
                string FeedbackButton = core.docProperties.getText("fbb");
                string Panel = null;
                string Copy = null;
                string NoteCopy = "";
                string NoteFromEmail = null;
                string NoteFromName = null;
                int CS = 0;
                switch (FeedbackButton) {
                    case FeedbackButtonSubmit:
                        //
                        // ----- form was submitted, save the note, send it and say thanks
                        //
                        NoteFromName = core.docProperties.getText("NoteFromName");
                        NoteFromEmail = core.docProperties.getText("NoteFromEmail");
                        //
                        NoteCopy = NoteCopy + "Feedback Submitted" + BR;
                        NoteCopy = NoteCopy + "From " + NoteFromName + " at " + NoteFromEmail + BR;
                        NoteCopy = NoteCopy + "Replying to:" + BR;
                        if (!string.IsNullOrEmpty(headline)) {
                            NoteCopy = NoteCopy + "    Article titled [" + headline + "]" + BR;
                        }
                        NoteCopy = NoteCopy + "    Record [" + RecordID + "] in Content Definition [" + ContentName + "]" + BR;
                        NoteCopy = NoteCopy + BR;
                        NoteCopy = NoteCopy + "<b>Comments</b>" + BR;
                        //
                        Copy = core.docProperties.getText("NoteCopy");
                        if (string.IsNullOrEmpty(Copy)) {
                            NoteCopy = NoteCopy + "[no comments entered]" + BR;
                        } else {
                            NoteCopy = NoteCopy + core.html.convertNewLineToHtmlBreak(Copy) + BR;
                        }
                        //
                        NoteCopy = NoteCopy + BR;
                        NoteCopy = NoteCopy + "<b>Content on which the comments are based</b>" + BR;
                        //
                        CS = core.db.csOpen(ContentName, "ID=" + RecordID);
                        Copy = "[the content of this page is not available]" + BR;
                        if (core.db.csOk(CS)) {
                            Copy = (core.db.csGet(CS, "copyFilename"));
                            //Copy = main_EncodeContent5(Copy, c.authcontext.user.userid, iContentName, iRecordID, 0, False, False, True, True, False, True, "", "", False, 0)
                        }
                        NoteCopy = NoteCopy + Copy + BR;
                        core.db.csClose(ref CS);
                        //
                        personModel person = personModel.create(core, ToMemberID);
                        if (person != null) {
                            string sendStatus = "";
                            string queryStringForLinkAppend = "";
                            emailController.queuePersonEmail(core, person, NoteFromEmail, "Feedback Form Submitted", NoteCopy, "", "", false, true, 0, "", false, ref sendStatus, queryStringForLinkAppend);
                        }
                        //
                        // ----- Note sent, say thanks
                        //
                        result += "<p>Thank you. Your feedback was received.</p>";
                        break;
                    default:
                        //
                        // ----- print the feedback submit form
                        //
                        Panel = "<form Action=\"" + core.webServer.serverFormActionURL + "?" + core.doc.refreshQueryString + "\" Method=\"post\">";
                        Panel = Panel + "<table border=\"0\" cellpadding=\"4\" cellspacing=\"0\" width=\"100%\">";
                        Panel = Panel + "<tr>";
                        Panel = Panel + "<td colspan=\"2\"><p>Your feedback is welcome</p></td>";
                        Panel = Panel + "</tr><tr>";
                        //
                        // ----- From Name
                        //
                        Copy = core.session.user.name;
                        Panel = Panel + "<td align=\"right\" width=\"100\"><p>Your Name</p></td>";
                        Panel = Panel + "<td align=\"left\"><input type=\"text\" name=\"NoteFromName\" value=\"" + htmlController.encodeHtml(Copy) + "\"></span></td>";
                        Panel = Panel + "</tr><tr>";
                        //
                        // ----- From Email address
                        //
                        Copy = core.session.user.Email;
                        Panel = Panel + "<td align=\"right\" width=\"100\"><p>Your Email</p></td>";
                        Panel = Panel + "<td align=\"left\"><input type=\"text\" name=\"NoteFromEmail\" value=\"" + htmlController.encodeHtml(Copy) + "\"></span></td>";
                        Panel = Panel + "</tr><tr>";
                        //
                        // ----- Message
                        //
                        Copy = "";
                        Panel = Panel + "<td align=\"right\" width=\"100\" valign=\"top\"><p>Feedback</p></td>";
                        Panel = Panel + "<td>" + htmlController.inputText(core, "NoteCopy", Copy, 4, 40, "TextArea", false) + "</td>";
                        //Panel = Panel & "<td><textarea ID=""TextArea"" rows=""4"" cols=""40"" name=""NoteCopy"">" & Copy & "</textarea></td>"
                        Panel = Panel + "</tr><tr>";
                        //
                        // ----- submit button
                        //
                        Panel = Panel + "<td>&nbsp;</td>";
                        Panel = Panel + "<td>" + htmlController.getHtmlInputSubmit(FeedbackButtonSubmit, "fbb") + "</td>";
                        //Panel = Panel + "<td><input type=\"submit\" name=\"fbb\" value=\"" + FeedbackButtonSubmit + "\"></td>";
                        Panel = Panel + "</tr></table>";
                        Panel = Panel + "</form>";
                        //
                        result = Panel;
                        break;
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   Creates the child page list used by PageContent
        //
        //   RequestedListName is the name of the ChildList (ActiveContent Child Page List)
        //       ----- New
        //       RequestedListName = "", same as "ORPHAN", same as "NONE"
        //           prints orphan list (child pages that have not printed so far (orphan list))
        //       AllowChildListDisplay - if false, no Child Page List is displayed, but authoring tags are still there
        //       Changed to friend, not public
        //       ----- Old
        //       "NONE" returns child pages with no RequestedListName
        //       "" same as "NONE"
        //       "ORPHAN" returns all child pages that have not been printed on this page
        //           - uses ChildPageListTracking to track what has been seen
        //=============================================================================
        //
        public static string getChildPageList( coreController core,  string RequestedListName, string ContentName, int parentPageID, bool allowChildListDisplay, bool ArchivePages = false) {
            string result = "";
            try {
                if (string.IsNullOrEmpty(ContentName)) {
                    ContentName = pageContentModel.contentName;
                }
                bool isAuthoring = core.session.isEditing(ContentName);
                //
                int ChildListCount = 0;
                string UcaseRequestedListName = genericController.vbUCase(RequestedListName);
                if ((UcaseRequestedListName == "NONE") || (UcaseRequestedListName == "ORPHAN")) {
                    UcaseRequestedListName = "";
                }
                //
                string archiveLink = core.webServer.requestPathPage;
                archiveLink = genericController.ConvertLinkToShortLink(archiveLink, core.webServer.requestDomain, core.appConfig.cdnFileUrl);
                archiveLink = genericController.encodeVirtualPath(archiveLink, core.appConfig.cdnFileUrl, appRootPath, core.webServer.requestDomain);
                //
                //string sqlCriteria = "(parentId=" + parentPageID + ")";
                //string sqlCriteria = "(parentId=" + core.doc.pageController.page.id + ")";
                //string sqlOrderBy = "sortOrder";
                List<pageContentModel> childPageList = pageContentModel.createList(core, "(parentId=" + parentPageID + ")", "sortOrder");
                string inactiveList = "";
                string activeList = "";
                foreach (pageContentModel childPage in childPageList) {
                    string PageLink = pageContentController.getPageLink(core, childPage.id, "", true, false);
                    string pageMenuHeadline = childPage.MenuHeadline;
                    if (string.IsNullOrEmpty(pageMenuHeadline)) {
                        pageMenuHeadline = childPage.name.Trim(' ');
                        if (string.IsNullOrEmpty(pageMenuHeadline)) {
                            pageMenuHeadline = "Related Page";
                        }
                    }
                    string pageEditLink = "";
                    if (core.session.isEditing(ContentName)) {
                        pageEditLink = adminUIController.getRecordEditLink(core, ContentName, childPage.id, true, childPage.name, true);
                    }
                    //
                    string link = PageLink;
                    if (ArchivePages) {
                        link = genericController.modifyLinkQuery(archiveLink, rnPageId, encodeText(childPage.id), true);
                    }
                    bool blockContentComposite = false;
                    if (childPage.BlockContent | childPage.BlockPage) {
                        blockContentComposite = !core.doc.bypassContentBlock(childPage.contentControlID, childPage.id);
                    }
                    string LinkedText = genericController.csv_GetLinkedText("<a href=\"" + htmlController.encodeHtml(link) + "\">", pageMenuHeadline);
                    if ((string.IsNullOrEmpty(UcaseRequestedListName)) && (childPage.ParentListName != "") & (!isAuthoring)) {
                        //
                        // ----- Requested orphan list, and this record is in a named list, and not editing, do not display
                        //
                    } else if ((string.IsNullOrEmpty(UcaseRequestedListName)) && (childPage.ParentListName != "")) {
                        //
                        // ----- Requested orphan list, and this record is in a named list, but authoring, list it
                        //
                        if (isAuthoring) {
                            inactiveList = inactiveList + "\r<li name=\"page" + childPage.id + "\" name=\"page" + childPage.id + "\"  id=\"page" + childPage.id + "\" class=\"ccListItem\">";
                            inactiveList = inactiveList + pageEditLink;
                            inactiveList = inactiveList + "[from Child Page List '" + childPage.ParentListName + "': " + LinkedText + "]";
                            inactiveList = inactiveList + "</li>";
                        }
                    } else if ((string.IsNullOrEmpty(UcaseRequestedListName)) && (!allowChildListDisplay) && (!isAuthoring)) {
                        //
                        // ----- Requested orphan List, Not AllowChildListDisplay, not Authoring, do not display
                        //
                    } else if ((!string.IsNullOrEmpty(UcaseRequestedListName)) & (UcaseRequestedListName != genericController.vbUCase(childPage.ParentListName))) {
                        //
                        // ----- requested named list and wrong RequestedListName, do not display
                        //
                    } else if (!childPage.AllowInChildLists) {
                        //
                        // ----- Allow in Child Page Lists is false, display hint to authors
                        //
                        if (isAuthoring) {
                            inactiveList = inactiveList + "\r<li name=\"page" + childPage.id + "\"  id=\"page" + childPage.id + "\" class=\"ccListItem\">";
                            inactiveList = inactiveList + pageEditLink;
                            inactiveList = inactiveList + "[Hidden (Allow in Child Lists is not checked): " + LinkedText + "]";
                            inactiveList = inactiveList + "</li>";
                        }
                    } else if (!childPage.active) {
                        //
                        // ----- Not active record, display hint if authoring
                        //
                        if (isAuthoring) {
                            inactiveList = inactiveList + "\r<li name=\"page" + childPage.id + "\"  id=\"page" + childPage.id + "\" class=\"ccListItem\">";
                            inactiveList = inactiveList + pageEditLink;
                            inactiveList = inactiveList + "[Hidden (Inactive): " + LinkedText + "]";
                            inactiveList = inactiveList + "</li>";
                        }
                    } else if ((childPage.PubDate != DateTime.MinValue) && (childPage.PubDate > core.doc.profileStartTime)) {
                        //
                        // ----- Child page has not been published
                        //
                        if (isAuthoring) {
                            inactiveList = inactiveList + "\r<li name=\"page" + childPage.id + "\"  id=\"page" + childPage.id + "\" class=\"ccListItem\">";
                            inactiveList = inactiveList + pageEditLink;
                            inactiveList = inactiveList + "[Hidden (To be published " + childPage.PubDate + "): " + LinkedText + "]";
                            inactiveList = inactiveList + "</li>";
                        }
                    } else if ((childPage.DateExpires != DateTime.MinValue) && (childPage.DateExpires < core.doc.profileStartTime)) {
                        //
                        // ----- Child page has expired
                        //
                        if (isAuthoring) {
                            inactiveList = inactiveList + "\r<li name=\"page" + childPage.id + "\"  id=\"page" + childPage.id + "\" class=\"ccListItem\">";
                            inactiveList = inactiveList + pageEditLink;
                            inactiveList = inactiveList + "[Hidden (Expired " + childPage.DateExpires + "): " + LinkedText + "]";
                            inactiveList = inactiveList + "</li>";
                        }
                    } else {
                        //
                        // ----- display list (and authoring links)
                        //
                        activeList = activeList + "\r<li name=\"page" + childPage.id + "\"  id=\"page" + childPage.id + "\" class=\"ccListItem\">";
                        if (!string.IsNullOrEmpty(pageEditLink)) {
                            activeList = activeList + pageEditLink + "&nbsp;";
                        }
                        activeList = activeList + LinkedText;
                        //
                        // include authoring mark for content block
                        //
                        if (isAuthoring) {
                            if (childPage.BlockContent) {
                                activeList = activeList + "&nbsp;[Content Blocked]";
                            }
                            if (childPage.BlockPage) {
                                activeList = activeList + "&nbsp;[Page Blocked]";
                            }
                        }
                        //
                        // include overview
                        // if AllowBrief is false, BriefFilename is not loaded
                        //
                        if ((childPage.BriefFilename != "") & (childPage.AllowBrief)) {
                            string Brief = encodeText(core.cdnFiles.readFileText(childPage.BriefFilename)).Trim(' ');
                            if (!string.IsNullOrEmpty(Brief)) {
                                activeList = activeList + "<div class=\"ccListCopy\">" + Brief + "</div>";
                            }
                        }
                        activeList = activeList + "</li>";
                        ChildListCount = ChildListCount + 1;
                        //.IsDisplayed = True
                    }
                }
                //
                // ----- Add Link
                //
                if (!ArchivePages) {
                    string AddLink = adminUIController.getRecordAddLink(core, ContentName, "parentid=" + parentPageID + ",ParentListName=" + UcaseRequestedListName, true);
                    if (!string.IsNullOrEmpty(AddLink)) {
                        inactiveList = inactiveList + "\r<li class=\"ccListItem\">" + AddLink + "</LI>";
                    }
                }
                //
                // ----- If there is a list, add the list start and list end
                //
                result = "";
                if (!string.IsNullOrEmpty(activeList)) {
                    result += "\r<ul id=\"childPageList_" + parentPageID + "_" + RequestedListName + "\" class=\"ccChildList\">" + genericController.nop(activeList) + "\r</ul>";
                }
                if (!string.IsNullOrEmpty(inactiveList)) {
                    result += "\r<ul id=\"childPageList_" + parentPageID + "_" + RequestedListName + "\" class=\"ccChildListInactive\">" + genericController.nop(inactiveList) + "\r</ul>";
                }
                //
                // ----- if non-orphan list, authoring and none found, print none message
                //
                if ((!string.IsNullOrEmpty(UcaseRequestedListName)) & (ChildListCount == 0) & isAuthoring) {
                    result = "[Child Page List with no pages]</p><p>" + result;
                }
            } catch (Exception ex) {
                logController.handleError(core, ex);
            }
            return result;
        }
        //
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
        ~pageContentController() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
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
                    //If (cacheClient IsNot Nothing) Then
                    //    cacheClient.Dispose()
                    //End If
                }
                //
                // cleanup non-managed objects
                //
            }
        }
        #endregion
    }
    //
}