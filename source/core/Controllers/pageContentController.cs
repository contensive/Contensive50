
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
//
using Contensive.BaseClasses;
//
namespace Contensive.Core.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// build page content system. Persistence is the docController.
    /// </summary>
    public class pageContentController : IDisposable {
        //
        //========================================================================
        //   Returns the HTML body
        //
        //   This code is based on the GoMethod site script
        //========================================================================
        //
        public static string getHtmlBodyTemplate(coreController core) {
            string returnBody = "";
            try {
                string AddonReturn = null;
                stringBuilderLegacyController Result = new stringBuilderLegacyController();
                string PageContent = null;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                int LocalTemplateID = 0;
                string LocalTemplateName = null;
                string LocalTemplateBody = null;
                bool blockSiteWithLogin = false;
                //
                // -- OnBodyStart add-ons
                CPUtilsBaseClass.addonExecuteContext bodyStartContext = new CPUtilsBaseClass.addonExecuteContext() { addonType = CPUtilsBaseClass.addonContext.ContextOnBodyStart };
                foreach (addonModel addon in core.addonCache.getOnBodyStartAddonList()) {
                    returnBody += core.addon.execute(addon, bodyStartContext);
                }
                //
                // ----- main_Get Content (Already Encoded)
                //
                blockSiteWithLogin = false;
                PageContent = getHtmlBodyTemplateContent(core, true, true, false, ref blockSiteWithLogin);
                if (blockSiteWithLogin) {
                    //
                    // section blocked, just return the login box in the page content
                    //
                    returnBody = ""
                        + "\r<div class=\"ccLoginPageCon\">"
                        + genericController.htmlIndent(PageContent) + "\r</div>"
                        + "";
                } else if (!core.doc.continueProcessing) {
                    //
                    // exit if stream closed during main_GetSectionpage
                    //
                    returnBody = "";
                } else {
                    //
                    // -- no section block, continue
                    LocalTemplateID = core.doc.template.id;
                    LocalTemplateBody = core.doc.template.bodyHTML;
                    if (string.IsNullOrEmpty(LocalTemplateBody)) {
                        LocalTemplateBody = TemplateDefaultBody;
                    }
                    LocalTemplateName = core.doc.template.name;
                    if (string.IsNullOrEmpty(LocalTemplateName)) {
                        LocalTemplateName = "Template " + LocalTemplateID;
                    }
                    //
                    // ----- Encode Template
                    //
                    //LocalTemplateBody = contentCmdController.executeContentCommands(core, LocalTemplateBody, CPUtilsBaseClass.addonContext.ContextTemplate, core.sessionContext.user.id, core.sessionContext.isAuthenticated, ref layoutError);
                    returnBody += activeContentController.renderHtmlForWeb(core, LocalTemplateBody, "Page Templates", LocalTemplateID, 0, core.webServer.requestProtocol + core.webServer.requestDomain, core.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextTemplate);
                    //
                    // If Content was not found, add it to the end
                    //
                    if (returnBody.IndexOf(fpoContentBox)  != -1) {
                        returnBody = genericController.vbReplace(returnBody, fpoContentBox, PageContent);
                    } else {
                        returnBody = returnBody + PageContent;
                    }
                    //
                    // ----- Add tools Panel
                    //
                    if (!core.session.isAuthenticated) {
                        //
                        // not logged in
                        //
                    } else {
                        //
                        // Add template editing
                        //
                        if (core.visitProperty.getBoolean("AllowAdvancedEditor") & core.session.isEditing("Page Templates")) {
                            returnBody = core.html.getEditWrapper("Page Template [" + LocalTemplateName + "]", core.html.getRecordEditLink2("Page Templates", LocalTemplateID, false, LocalTemplateName, core.session.isEditing("Page Templates")) + returnBody);
                        }
                    }
                    //
                    // ----- OnBodyEnd add-ons
                    //
                    CPUtilsBaseClass.addonExecuteContext bodyEndContext = new CPUtilsBaseClass.addonExecuteContext() { addonType = CPUtilsBaseClass.addonContext.ContextFilter };
                    foreach (var addon in core.addonCache.getOnBodyEndAddonList()) {
                        core.doc.docBodyFilter = returnBody;
                        AddonReturn = core.addon.execute(addon, bodyEndContext);
                        //AddonReturn = core.addon.execute_legacy2(addon.id, "", "", CPUtilsBaseClass.addonContext.ContextFilter, "", 0, "", "", False, 0, "", False, Nothing)
                        returnBody = core.doc.docBodyFilter + AddonReturn;
                    }
               }
                //
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnBody;
        }
        //
        //=============================================================================
        //   main_Get Section
        //       Two modes
        //           pre 3.3.613 - SectionName = RootPageName
        //           else - (IsSectionRootPageIDMode) SectionRecord has a RootPageID field
        //=============================================================================
        //
        public static string getHtmlBodyTemplateContent(coreController core, bool AllowChildPageList, bool AllowReturnLink, bool AllowEditWrapper, ref bool return_blockSiteWithLogin) {
            string returnHtml = "";
            try {
                bool allowPageWithoutSectionDislay = false;
                int FieldRows = 0;
                int PageID = 0;
                bool UseContentWatchLink = core.siteProperties.useContentWatchLink;
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
                if (core.doc.page.id == 0) {
                    //
                    // -- landing page is not valid -- display error
                    logController.logInfo(core, "Requested page/document not found:" + core.webServer.requestUrlSource);
                    core.doc.redirectBecausePageNotFound = true;
                    core.doc.redirectReason = "Redirecting because the page selected could not be found.";
                    core.doc.redirectLink = pageContentController.main_ProcessPageNotFound_GetLink(core, core.doc.redirectReason, "", "", PageID, 0);
                    core.handleException(new ApplicationException("Page could not be determined. Error message displayed."));
                    return "<div style=\"width:300px; margin: 100px auto auto auto;text-align:center;\">This page is not valid.</div>";
                }
                //PageID = core.docProperties.getInteger(rnPageId)
                //If (PageID = 0) Then
                //    '
                //    ' -- Nothing specified, use the Landing Page
                //    PageID = main_GetLandingPageID()
                //    If PageID = 0 Then
                //        '
                //        ' -- landing page is not valid -- display error
                //        Call logController.log_appendLogPageNotFound(core, core.webServer.requestUrlSource)
                //        RedirectBecausePageNotFound = True
                //        RedirectReason = "Redirecting because the page selected could not be found."
                //        redirectLink = main_ProcessPageNotFound_GetLink(RedirectReason, , , PageID, 0)

                //        core.handleExceptionAndContinue(New ApplicationException("Page could not be determined. Error message displayed."))
                //        Return "<div style=""width:300px; margin: 100px auto auto auto;text-align:center;"">This page is not valid.</div>"
                //    End If
                //End If
                //Call core.htmlDoc.webServerIO_addRefreshQueryString(rnPageId, CStr(PageID))
                //templateReason = "The reason this template was selected could not be determined."
                //
                // -- build parentpageList (first = current page, last = root)
                // -- add a 0, then repeat until another 0 is found, or there is a repeat
                //pageToRootList = New List(Of pageContentModel)()
                //Dim usedPageIdList As New List(Of Integer)()
                //Dim targetPageId = PageID
                //usedPageIdList.Add(0)
                //Do While (Not usedPageIdList.Contains(targetPageId))
                //    usedPageIdList.Add(targetPageId)
                //    Dim targetpage As pageContentModel = pageContentModel.create(core, targetPageId, New List(Of String))
                //    If (targetpage Is Nothing) Then
                //        Exit Do
                //    Else
                //        pageToRootList.Add(targetpage)
                //        targetPageId = targetpage.id
                //    End If
                //Loop
                //If (pageToRootList.Count = 0) Then
                //    '
                //    ' -- page is not valid -- display error
                //    core.handleExceptionAndContinue(New ApplicationException("Page could not be determined. Error message displayed."))
                //    Return "<div style=""width:300px; margin: 100px auto auto auto;text-align:center;"">This page is not valid.</div>"
                //End If
                //page = pageToRootList.First
                //
                // -- get template from pages
                //Dim template As pageTemplateModel = Nothing
                //For Each page As pageContentModel In pageToRootList
                //    If page.TemplateID > 0 Then
                //        template = pageTemplateModel.create(core, page.TemplateID, New List(Of String))
                //        If (template IsNot Nothing) Then
                //            If (page Is page) Then
                //                templateReason = "This template was used because it is selected by the current page."
                //            Else
                //                templateReason = "This template was used because it is selected one of this page's parents [" & page.name & "]."
                //            End If
                //            Exit For
                //        End If
                //    End If
                //Next
                //
                //If (template Is Nothing) Then
                //    '
                //    ' -- get template from domain
                //    If (domain IsNot Nothing) Then
                //        template = pageTemplateModel.create(core, domain.DefaultTemplateId, New List(Of String))
                //    End If
                //    If (template Is Nothing) Then
                //        '
                //        ' -- get template named Default
                //        template = pageTemplateModel.createByName(core, "default", New List(Of String))
                //    End If
                //End If
                //
                // -- get contentbox
                returnHtml = getContentBox(core, "", AllowChildPageList, AllowReturnLink, false, 0, UseContentWatchLink, allowPageWithoutSectionDislay);
                //
                // -- if fpo_QuickEdit it there, replace it out
                string Editor = null;
                string styleOptionList = "";
                string addonListJSON = null;
                if (core.doc.redirectLink == "" && (returnHtml.IndexOf(html_quickEdit_fpo)  != -1)) {
                    FieldRows = genericController.encodeInteger(core.userProperty.getText("Page Content.copyFilename.PixelHeight", "500"));
                    if (FieldRows < 50) {
                        FieldRows = 50;
                        core.userProperty.setProperty("Page Content.copyFilename.PixelHeight", 50);
                    }
                    string stylesheetCommaList = ""; //core.html.main_GetStyleSheet2(csv_contentTypeEnum.contentTypeWeb, templateId, 0)
                    addonListJSON = core.html.JSONeditorAddonList(csv_contentTypeEnum.contentTypeWeb);
                    Editor = core.html.getFormInputHTML("copyFilename", core.doc.quickEditCopy, FieldRows.ToString(), "100%", false, true, addonListJSON, stylesheetCommaList, styleOptionList);
                    returnHtml = genericController.vbReplace(returnHtml, html_quickEdit_fpo, Editor);
                }
                //
                // -- Add admin warning to the top of the content
                if (core.session.isAuthenticatedAdmin(core) & core.doc.adminWarning != "") {
                    //
                    // Display Admin Warnings with Edits for record errors
                    //
                    if (core.doc.adminWarningPageID != 0) {
                        core.doc.adminWarning = core.doc.adminWarning + "</p>" + core.html.getRecordEditLink2("Page Content", core.doc.adminWarningPageID, true, "Page " + core.doc.adminWarningPageID, core.session.isAuthenticatedAdmin(core)) + "&nbsp;Edit the page<p>";
                        core.doc.adminWarningPageID = 0;
                    }
                    //
                    returnHtml = ""
                    + core.html.getAdminHintWrapper(core.doc.adminWarning) + returnHtml + "";
                    core.doc.adminWarning = "";
                }
                //
                // -- handle redirect and edit wrapper
                //------------------------------------------------------------------------------------
                //
                if (core.doc.redirectLink != "") {
                    return core.webServer.redirect(core.doc.redirectLink, core.doc.redirectReason, core.doc.redirectBecausePageNotFound);
                } else if (AllowEditWrapper) {
                    returnHtml = core.html.getEditWrapper("Page Content", returnHtml);
                }
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return returnHtml;
        }
        //
        //=============================================================================
        //   Add content padding around content
        //       is called from main_GetPageRaw, as well as from higher up when blocking is turned on
        //=============================================================================
        //
        internal static string getContentBoxWrapper(coreController core, string Content, int ContentPadding) {
            string tempgetContentBoxWrapper = null;
            //dim buildversion As String
            //
            // BuildVersion = app.dataBuildVersion
            tempgetContentBoxWrapper = Content;
            if (core.siteProperties.getBoolean("Compatibility ContentBox Pad With Table")) {
                //
                if (ContentPadding > 0) {
                    //
                    tempgetContentBoxWrapper = ""
                        + "\r<table border=0 width=\"100%\" cellspacing=0 cellpadding=0>"
                        + cr2 + "<tr>"
                        + cr3 + "<td style=\"padding:" + ContentPadding + "px\">"
                        + genericController.htmlIndent(genericController.htmlIndent(genericController.htmlIndent(tempgetContentBoxWrapper))) + cr3 + "</td>"
                        + cr2 + "</tr>"
                        + "\r</table>";
                    //            main_GetContentBoxWrapper = "" _
                    //                & cr & "<table border=0 width=""100%"" cellspacing=0 cellpadding=" & ContentPadding & ">" _
                    //                & cr2 & "<tr>" _
                    //                & cr3 & "<td>" _
                    //                & genericController.kmaIndent(KmaIndent(KmaIndent(main_GetContentBoxWrapper))) _
                    //                & cr3 & "</td>" _
                    //                & cr2 & "</tr>" _
                    //                & cr & "</table>"
                }
                tempgetContentBoxWrapper = ""
                    + "\r<div class=\"contentBox\">"
                    + genericController.htmlIndent(tempgetContentBoxWrapper) + "\r</div>";
            } else {
                //
                tempgetContentBoxWrapper = ""
                    + "\r<div class=\"contentBox\" style=\"padding:" + ContentPadding + "px\">"
                    + genericController.htmlIndent(tempgetContentBoxWrapper) + "\r</div>";
            }
            return tempgetContentBoxWrapper;
        }
        //
        //
        //
        internal static string getDefaultBlockMessage(coreController core, bool UseContentWatchLink) {
            string tempgetDefaultBlockMessage = null;
            try {
                tempgetDefaultBlockMessage = "";
                int CS = core.db.csOpen("Copy Content", "name=" + core.db.encodeSQLText(ContentBlockCopyName), "ID", false, 0, false, false, "Copy,ID");
                if (core.db.csOk(CS)) {
                    tempgetDefaultBlockMessage = core.db.csGet(CS, "Copy");
                }
                core.db.csClose(ref CS);
                //
                // ----- Do not allow blank message - if still nothing, create default
                //
                if (string.IsNullOrEmpty(tempgetDefaultBlockMessage)) {
                    tempgetDefaultBlockMessage = "<p>The content on this page has restricted access. If you have a username and password for this system, <a href=\"?method=login\" rel=\"nofollow\">Click Here</a>. For more information, please contact the administrator.</p>";
                }
                //
                // ----- Create Copy Content Record for future
                //
                CS = core.db.csInsertRecord("Copy Content");
                if (core.db.csOk(CS)) {
                    core.db.csSet(CS, "Name", ContentBlockCopyName);
                    core.db.csSet(CS, "Copy", tempgetDefaultBlockMessage);
                    core.db.csSave(CS);
                    //Call core.workflow.publishEdit("Copy Content", genericController.EncodeInteger(core.db.cs_get(CS, "ID")))
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempgetDefaultBlockMessage;
        }
        //
        //
        //
        //
        public static string getMoreInfoHtml(coreController core, int PeopleID) {
            string result = "";
            try {
                int CS = 0;
                string ContactName = null;
                string ContactPhone = null;
                string ContactEmail = null;
                string Copy;
                //
                Copy = "";
                CS = core.db.csOpenContentRecord("People", PeopleID, 0, false, false, "Name,Phone,Email");
                if (core.db.csOk(CS)) {
                    ContactName = (core.db.csGetText(CS, "Name"));
                    ContactPhone = (core.db.csGetText(CS, "Phone"));
                    ContactEmail = (core.db.csGetText(CS, "Email"));
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
                        //Copy = Copy;
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
                            //Copy = Copy;
                        }
                    }
                }
                core.db.csClose(ref CS);
                //
                result = Copy;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //
        //
        internal static string getTableRow(string Caption, string Result, bool EvenRow) {
            //
            string CopyCaption = null;
            string CopyResult = null;
            //
            CopyCaption = Caption;
            if (string.IsNullOrEmpty(CopyCaption)) {
                CopyCaption = "&nbsp;";
            }
            //
            CopyResult = Result;
            if (string.IsNullOrEmpty(CopyResult)) {
                CopyResult = "&nbsp;";
            }
            //
            return genericController.GetTableCell("<nobr>" + CopyCaption + "</nobr>", "150", 0, EvenRow, "right") + genericController.GetTableCell(CopyResult, "100%", 0, EvenRow, "left") + kmaEndTableRow;
        }
        //
        public static void loadPage(coreController core, int requestedPageId, domainModel domain) {
            try {
                if (domain == null) {
                    //
                    // -- domain is not valid
                    core.handleException(new ApplicationException("Page could not be determined because the domain was not recognized."));
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
                    core.doc.pageToRootList = new List<Models.DbModels.pageContentModel>();
                    List<int> usedPageIdList = new List<int>();
                    int targetPageId = requestedPage.id;
                    usedPageIdList.Add(0);
                    while (!usedPageIdList.Contains(targetPageId)) {
                        usedPageIdList.Add(targetPageId);
                        pageContentModel targetpage = pageContentModel.create(core, targetPageId );
                        if (targetpage == null) {
                            break;
                        } else {
                            core.doc.pageToRootList.Add(targetpage);
                            targetPageId = targetpage.ParentID;
                        }
                    }
                    if (core.doc.pageToRootList.Count == 0) {
                        //
                        // -- attempt failed, create default page
                        core.doc.page = pageContentModel.add(core);
                        core.doc.page.name = DefaultNewLandingPageName + ", " + domain.name;
                        core.doc.page.Copyfilename.content = landingPageDefaultHtml;
                        core.doc.page.save(core);
                        core.doc.pageToRootList.Add(core.doc.page);
                    } else {
                        core.doc.page = core.doc.pageToRootList.First();
                    }
                    //
                    // -- get template from pages
                    core.doc.template = null;
                    foreach (Models.DbModels.pageContentModel page in core.doc.pageToRootList) {
                        if (page.TemplateID > 0) {
                            core.doc.template = pageTemplateModel.create(core, page.TemplateID );
                            if (core.doc.template == null) {
                                //
                                // -- templateId is not valid
                                page.TemplateID = 0;
                                page.save(core);
                            } else {
                                if (page == core.doc.pageToRootList.First()) {
                                    core.doc.templateReason = "This template was used because it is selected by the current page.";
                                } else {
                                    core.doc.templateReason = "This template was used because it is selected one of this page's parents [" + page.name + "].";
                                }
                                break;
                            }
                        }
                    }
                    //
                    if (core.doc.template == null) {
                        //
                        // -- get template from domain
                        if (domain != null) {
                            if (domain.defaultTemplateId > 0) {
                                core.doc.template = pageTemplateModel.create(core, domain.defaultTemplateId );
                                if (core.doc.template == null) {
                                    //
                                    // -- domain templateId is not valid
                                    domain.defaultTemplateId = 0;
                                    domain.save(core);
                                }
                            }
                        }
                        if (core.doc.template == null) {
                            //
                            // -- get template named Default
                            core.doc.template = pageTemplateModel.createByName(core, defaultTemplateName);
                            if (core.doc.template == null) {
                                //
                                // -- ceate new template named Default
                                core.doc.template = pageTemplateModel.add(core );
                                core.doc.template.name = defaultTemplateName;
                                core.doc.template.bodyHTML = core.appRootFiles.readFileText(defaultTemplateHomeFilename);
                                core.doc.template.save(core);
                            }
                            //
                            // -- set this new template to all domains without a template
                            foreach (domainModel d in domainModel.createList(core, "((DefaultTemplateId=0)or(DefaultTemplateId is null))")) {
                                d.defaultTemplateId = core.doc.template.id;
                                d.save(core);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        public static int getPageNotFoundPageId(coreController core) {
            int pageId = 0;
            try {
                pageId = core.domain.pageNotFoundPageId;
                if (pageId == 0) {
                    //
                    // no domain page not found, use site default
                    //
                    pageId = core.siteProperties.getInteger("PageNotFoundPageID", 0);
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return pageId;
        }
        //
        //---------------------------------------------------------------------------
        //
        //---------------------------------------------------------------------------
        //
        public static pageContentModel getLandingPage(coreController core, domainModel domain) {
            pageContentModel landingPage = null;
            try {
                if (domain == null) {
                    //
                    // -- domain not available
                    core.handleException(new ApplicationException("Landing page could not be determined because the domain was not recognized."));
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
                core.handleException(ex);
                throw;
            }
            return landingPage;
        }
        //
        // Verify a link from the template link field to be used as a Template Link
        //
        internal static string verifyTemplateLink(coreController core, string linkSrc) {
            string tempverifyTemplateLink = null;
            //
            //
            // ----- Check Link Format
            //
            tempverifyTemplateLink = linkSrc;
            if (!string.IsNullOrEmpty(tempverifyTemplateLink)) {
                if (genericController.vbInstr(1, tempverifyTemplateLink, "://") != 0) {
                    //
                    // protocol provided, do not fixup
                    //
                    tempverifyTemplateLink = genericController.EncodeAppRootPath(tempverifyTemplateLink, core.webServer.requestVirtualFilePath, requestAppRootPath, core.webServer.requestDomain);
                } else {
                    //
                    // no protocol, convert to short link
                    //
                    if (tempverifyTemplateLink.Left( 1) != "/") {
                        //
                        // page entered without path, assume it is in root path
                        //
                        tempverifyTemplateLink = "/" + tempverifyTemplateLink;
                    }
                    tempverifyTemplateLink = genericController.ConvertLinkToShortLink(tempverifyTemplateLink, core.webServer.requestDomain, core.webServer.requestVirtualFilePath);
                    tempverifyTemplateLink = genericController.EncodeAppRootPath(tempverifyTemplateLink, core.webServer.requestVirtualFilePath, requestAppRootPath, core.webServer.requestDomain);
                }
            }
            return tempverifyTemplateLink;
        }
        //
        //
        //
        internal static string main_ProcessPageNotFound_GetLink(coreController core, string adminMessage, string BackupPageNotFoundLink = "", string PageNotFoundLink = "", int EditPageID = 0, int EditSectionID = 0) {
            string result = "";
            try {
                int Pos = 0;
                string DefaultLink = null;
                int PageNotFoundPageID = 0;
                string Link = null;
                //
                PageNotFoundPageID = getPageNotFoundPageId(core);
                if (PageNotFoundPageID == 0) {
                    //
                    // No PageNotFound was set -- use the backup link
                    //
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
                    //
                    Link = getPageLink(core, PageNotFoundPageID, "", true, false);
                    DefaultLink = getPageLink(core, 0, "", true, false);
                    if (Link != DefaultLink) {
                    } else {
                        adminMessage = adminMessage + "</p><p>The current 'Page Not Found' could not be used. It is not valid, or it is not associated with a valid site section. To configure a valid 'Page Not Found' page, first create the page as a child page on your site and check the 'Page Not Found' checkbox on it's control tab. The Landing Page was used.";
                    }
                }
                //
                // Add the Admin Message to the link
                //
                if (core.session.isAuthenticatedAdmin(core)) {
                    if (string.IsNullOrEmpty(PageNotFoundLink)) {
                        PageNotFoundLink = core.webServer.requestUrl;
                    }
                    //
                    // Add the Link to the Admin Msg
                    //
                    adminMessage = adminMessage + "<p>The URL was " + PageNotFoundLink + ".";
                    //
                    // Add the Referrer to the Admin Msg
                    //
                    if (core.webServer.requestReferer != "") {
                        Pos = genericController.vbInstr(1, core.webServer.requestReferrer, "main_AdminWarningPageID=", 1);
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
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public static string getPageLink(coreController core, int PageID, string QueryStringSuffix, bool AllowLinkAliasIfEnabled = true, bool UseContentWatchNotDefaultPage = false) {
            string result = "";
            try {
                string linkPathPage = null;
                //
                // -- set linkPathPath to linkAlias
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
                    if (rule.templateId == core.doc.template.id) {
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
                if (core.doc.page.IsSecure | core.doc.template.isSecure) {
                    linkprotocol = "https://";
                } else {
                    linkprotocol = "http://";
                }
                //
                // -- assemble
                result = linkprotocol + linkDomain + linkPathPage;
            } catch (Exception ex) {
                core.handleException(ex);
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
        internal static string getDefaultScript() {
            return "default.aspx";
        }
        //
        //========================================================================
        //   main_IsChildRecord
        //
        //   Tests if this record is in the ParentID->ID chain for this content
        //========================================================================
        //
        public static bool isChildRecord(coreController core, string ContentName, int ChildRecordID, int ParentRecordID) {
            bool tempisChildRecord = false;
            try {
                //
                Models.Complex.cdefModel CDef = null;
                //
                tempisChildRecord = (ChildRecordID == ParentRecordID);
                if (!tempisChildRecord) {
                    CDef = Models.Complex.cdefModel.getCdef(core, ContentName);
                    if (genericController.IsInDelimitedString(CDef.selectCommaList.ToUpper(), "PARENTID", ",")) {
                        tempisChildRecord = main_IsChildRecord_Recurse(core, CDef.contentDataSourceName, CDef.contentTableName, ChildRecordID, ParentRecordID, "");
                    }
                }
                return tempisChildRecord;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18("coreClass.IsChildRecord")
                                                                    //
            return tempisChildRecord;
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
                string SQL = null;
                int CS = 0;
                int ChildRecordParentID = 0;
                //
                SQL = "select ParentID from " + TableName + " where id=" + ChildRecordID;
                CS = core.db.csOpenSql(SQL);
                if (core.db.csOk(CS)) {
                    ChildRecordParentID = core.db.csGetInteger(CS, "ParentID");
                }
                core.db.csClose(ref CS);
                if ((ChildRecordParentID != 0) & (!genericController.IsInDelimitedString(History, ChildRecordID.ToString(), ","))) {
                    result = (ParentRecordID == ChildRecordParentID);
                    if (!result) {
                        result = main_IsChildRecord_Recurse(core, DataSourceName, TableName, ChildRecordParentID, ParentRecordID, History + "," + ChildRecordID.ToString());
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
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
            string returnHtml = "";
            try {
                int downloadId = 0;
                int Pos = 0;
                string htmlDocBody = null;
                string Clip = null;
                int ClipParentRecordID = 0;
                int ClipParentContentID = 0;
                string ClipParentContentName = null;
                int ClipChildContentID = 0;
                string ClipChildContentName = null;
                int ClipChildRecordID = 0;
                string ClipChildRecordName = null;
                int CSClip = 0;
                string[] ClipBoardArray = null;
                string ClipBoard = null;
                string ClipParentFieldList = null;
                string[] Fields = null;
                int FieldCount = 0;
                string[] NameValues = null;
                string RedirectLink = "";
                string PageNotFoundReason = "";
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
                            recordId = core.doc.page.id
                        },
                        isIncludeAddon = false,
                        personalizationAuthenticated = core.session.visit.VisitAuthenticated,
                        personalizationPeopleId = core.session.user.id
                    };

                    //
                    // -- execute template Dependencies
                    List<Models.DbModels.addonModel> templateAddonList = addonModel.createList_templateDependencies(core, core.doc.template.id);
                    foreach (addonModel addon in templateAddonList) {
                        returnHtml += core.addon.executeDependency(addon, executeContext);
                     }
                    //
                    // -- execute page Dependencies
                    List<Models.DbModels.addonModel> pageAddonList = addonModel.createList_pageDependencies(core, core.doc.page.id);
                    foreach (addonModel addon in pageAddonList) {
                        returnHtml += core.addon.executeDependency(addon, executeContext);
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
                        emailController.sendForm(core, core.siteProperties.emailAdmin, FromAddress, "Form Submitted on " + core.webServer.requestReferer);
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
                        securityController.decodeToken(core,RecordEID, ref downloadId, ref tokenDate);
                        if (downloadId != 0) {
                            //
                            // -- lookup record and set clicks
                            libraryFilesModel file = libraryFilesModel.create(core, downloadId);
                            if (file != null) {
                                file.Clicks += 1;
                                file.save(core);
                                if (file.Filename != "") {
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
                                    string link = genericController.getCdnFileLink(core, file.Filename);
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
                    Clip = core.docProperties.getText(RequestNameCut);
                    if (!string.IsNullOrEmpty(Clip)) {
                        //
                        // if a cut, load the clipboard
                        //
                        core.visitProperty.setProperty("Clipboard", Clip);
                        genericController.modifyLinkQuery(core.doc.refreshQueryString, RequestNameCut, "");
                    }
                    ClipParentContentID = core.docProperties.getInteger(RequestNamePasteParentContentID);
                    ClipParentRecordID = core.docProperties.getInteger(RequestNamePasteParentRecordID);
                    ClipParentFieldList = core.docProperties.getText(RequestNamePasteFieldList);
                    if ((ClipParentContentID != 0) & (ClipParentRecordID != 0)) {
                        //
                        // Request for a paste, clear the cliboard
                        //
                        ClipBoard = core.visitProperty.getText("Clipboard", "");
                        core.visitProperty.setProperty("Clipboard", "");
                        genericController.ModifyQueryString(core.doc.refreshQueryString, RequestNamePasteParentContentID, "");
                        genericController.ModifyQueryString(core.doc.refreshQueryString, RequestNamePasteParentRecordID, "");
                        ClipParentContentName = Models.Complex.cdefModel.getContentNameByID(core, ClipParentContentID);
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
                                    ClipBoardArray = ClipBoard.Split('.');
                                    if (ClipBoardArray.GetUpperBound(0) == 0) {
                                        errorController.addUserError(core, "The paste operation failed because the clipboard data is configured incorrectly.");
                                    } else {
                                        ClipChildContentID = genericController.encodeInteger(ClipBoardArray[0]);
                                        ClipChildRecordID = genericController.encodeInteger(ClipBoardArray[1]);
                                        if (!Models.Complex.cdefModel.isWithinContent(core, ClipChildContentID, ClipParentContentID)) {
                                            errorController.addUserError(core, "The paste operation failed because the destination location is not compatible with the clipboard data.");
                                        } else {
                                            //
                                            // the content definition relationship is OK between the child and parent record
                                            //
                                            ClipChildContentName = Models.Complex.cdefModel.getContentNameByID(core, ClipChildContentID);
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
                                                    ClipChildRecordName = "record " + ClipChildRecordID;
                                                    CSClip = core.db.csOpen2(ClipChildContentName, ClipChildRecordID, true, true);
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
                                                            Fields = ClipParentFieldList.Split(',');
                                                            FieldCount = Fields.GetUpperBound(0) + 1;
                                                            for (var FieldPointer = 0; FieldPointer < FieldCount; FieldPointer++) {
                                                                string Pair = Fields[FieldPointer];
                                                                if (Pair.Left( 1) == "(" && Pair.Substring(Pair.Length - 1, 1) == ")") {
                                                                    Pair = Pair.Substring(1, Pair.Length - 2);
                                                                }
                                                                NameValues = Pair.Split('=');
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
                    //
                    string linkAliasTest1 = "";
                    string linkAliasTest2 = null;
                    string LinkNoProtocol = "";
                    string linkDomain = "";
                    string LinkFullPath = "";
                    string LinkFullPathNoSlash = "";
                    bool isLinkForward = false;
                    string LinkForwardCriteria = "";
                    string Sql = "";
                    int CSPointer = -1;
                    bool IsInLinkForwardTable = false;
                    int Viewings = 0;
                    string[] LinkSplit = null;
                    bool IsLinkAlias = false;
                    //
                    //--------------------------------------------------------------------------
                    // try link alias
                    //--------------------------------------------------------------------------
                    //
                    linkAliasTest1 = core.webServer.requestPathPage;
                    if (linkAliasTest1.Left( 1) == "/") {
                        linkAliasTest1 = linkAliasTest1.Substring(1);
                    }
                    if (linkAliasTest1.Length > 0) {
                        if (linkAliasTest1.Substring(linkAliasTest1.Length - 1, 1) == "/") {
                            linkAliasTest1 = linkAliasTest1.Left( linkAliasTest1.Length - 1);
                        }
                    }

                    linkAliasTest2 = linkAliasTest1 + "/";
                    if ((!IsPageNotFound) && (core.webServer.requestPathPage != "")) {
                        //
                        // build link variations needed later
                        //
                        //
                        Pos = genericController.vbInstr(1, core.webServer.requestPathPage, "://", 1);
                        if (Pos != 0) {
                            LinkNoProtocol = core.webServer.requestPathPage.Substring(Pos + 2);
                            Pos = genericController.vbInstr(Pos + 3, core.webServer.requestPathPage, "/", 2);
                            if (Pos != 0) {
                                linkDomain = core.webServer.requestPathPage.Left( Pos - 1);
                                LinkFullPath = core.webServer.requestPathPage.Substring(Pos - 1);
                                //
                                // strip off leading or trailing slashes, and return only the string between the leading and secton slash
                                //
                                if (genericController.vbInstr(1, LinkFullPath, "/") != 0) {
                                    LinkSplit = LinkFullPath.Split('/');
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
                        LinkForwardCriteria = ""
                            + "(active<>0)"
                            + "and("
                            + "(SourceLink=" + core.db.encodeSQLText(core.webServer.requestPathPage) + ")"
                            + "or(SourceLink=" + core.db.encodeSQLText(LinkNoProtocol) + ")"
                            + "or(SourceLink=" + core.db.encodeSQLText(LinkFullPath) + ")"
                            + "or(SourceLink=" + core.db.encodeSQLText(LinkFullPathNoSlash) + ")"
                            + ")";
                        isLinkForward = false;
                        Sql = core.db.GetSQLSelect("", "ccLinkForwards", "ID,DestinationLink,Viewings,GroupID", LinkForwardCriteria, "ID","", 1);
                        CSPointer = core.db.csOpenSql(Sql);
                        if (core.db.csOk(CSPointer)) {
                            //
                            // Link Forward found - update count
                            //
                            string tmpLink = null;
                            int GroupID = 0;
                            string groupName = null;
                            //
                            IsInLinkForwardTable = true;
                            Viewings = core.db.csGetInteger(CSPointer, "Viewings") + 1;
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
                                    RedirectLink = tmpLink;
                                }
                            }
                        }
                        core.db.csClose(ref CSPointer);
                        //
                        if ((string.IsNullOrEmpty(RedirectLink)) && !isLinkForward) {
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
                            if (!IsLinkAlias) {
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
                                //
                                // real 404
                                //
                                //IsPageNotFound = True
                                //PageNotFoundSource = core.webServer.requestPathPage
                                //PageNotFoundReason = "The page could Not be displayed because the URL Is Not a valid page, Link Forward, Link Alias Or RemoteMethod."
                            }
                        }
                    }
                    //
                    // ----- do anonymous access blocking
                    //
                    if (!core.session.isAuthenticated) {
                        if ((core.webServer.requestPath != "/") & genericController.vbInstr(1, "/" + core.appConfig.adminRoute, core.webServer.requestPath, 1) != 0) {
                            //
                            // admin page is excluded from custom blocking
                            //
                        } else {
                            int AnonymousUserResponseID = genericController.encodeInteger(core.siteProperties.getText("AnonymousUserResponseID", "0"));
                            switch (AnonymousUserResponseID) {
                                case 1:
                                    //
                                    // -- block with login
                                    core.doc.continueProcessing = false;
                                    return core.addon.execute(addonModel.create(core, addonGuidLoginPage), new CPUtilsBaseClass.addonExecuteContext() { addonType = CPUtilsBaseClass.addonContext.ContextPage });
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
                    htmlDocBody = getHtmlBodyTemplate(core);
                    //
                    // -- check secure certificate required
                    bool SecureLink_Template_Required = core.doc.template.isSecure;
                    bool SecureLink_Page_Required = false;
                    foreach (Models.DbModels.pageContentModel page in core.doc.pageToRootList) {
                        if (core.doc.page.IsSecure) {
                            SecureLink_Page_Required = true;
                            break;
                        }
                    }
                    bool SecureLink_Required = SecureLink_Template_Required || SecureLink_Page_Required;
                    bool SecureLink_CurrentURL = (core.webServer.requestUrl.ToLower().Left( 8) == "https://");
                    if (SecureLink_CurrentURL && (!SecureLink_Required)) {
                        //
                        // -- redirect to non-secure
                        RedirectLink = genericController.vbReplace(core.webServer.requestUrl, "https://", "http://");
                        core.doc.redirectReason = "Redirecting because neither the page or the template requires a secure link.";
                        return "";
                    } else if ((!SecureLink_CurrentURL) && SecureLink_Required) {
                        //
                        // -- redirect to secure
                        RedirectLink = genericController.vbReplace(core.webServer.requestUrl, "http://", "https://");
                        if (SecureLink_Page_Required) {
                            core.doc.redirectReason = "Redirecting because this page [" + core.doc.pageToRootList[0].name + "] requires a secure link.";
                        } else {
                            core.doc.redirectReason = "Redirecting because this template [" + core.doc.template.name + "] requires a secure link.";
                        }
                        return "";
                    }
                    //
                    // -- check that this template exists on this domain
                    // -- if endpoint is just domain -> the template is automatically compatible by default (domain determined the landing page)
                    // -- if endpoint is domain + route (link alias), the route determines the page, which may determine the core.doc.template. If this template is not allowed for this domain, redirect to the domain's landingcore.doc.page.
                    //
                    Sql = "(domainId=" + core.doc.domain.id + ")";
                    List<Models.DbModels.TemplateDomainRuleModel> allowTemplateRuleList = TemplateDomainRuleModel.createList(core, Sql);
                    if (allowTemplateRuleList.Count == 0) {
                        //
                        // -- current template has no domain preference, use current
                    } else {
                        bool allowTemplate = false;
                        foreach (TemplateDomainRuleModel rule in allowTemplateRuleList) {
                            if (rule.templateId == core.doc.template.id) {
                                allowTemplate = true;
                                break;
                            }
                        }
                        if (!allowTemplate) {
                            //
                            // -- must redirect to a domain's landing page
                            RedirectLink = core.webServer.requestProtocol + core.doc.domain.name;
                            core.doc.redirectBecausePageNotFound = false;
                            core.doc.redirectReason = "Redirecting because this domain has template requiements set, and this template is not configured [" + core.doc.template.name + "].";
                            return "";
                        }
                    }
                    returnHtml += htmlDocBody;
                }
                //
                // all other routes should be handled here.
                //   - this code is in initApp right now but should be migrated here.
                //   - if all other routes fail, use the defaultRoute (page manager at first)
                //
                if (true) {
                    // --- not reall sure what to do with this - was in appInit() and I am just sure it does not go there.
                    //
                    //--------------------------------------------------------------------------
                    // ----- check if the custom404pathpage matches the defaultdoc
                    //       in this case, the 404 hit is a direct result of a 404 I justreturned to IIS
                    //       currently, I am redirecting to the page-not-found page with a 404 - wrong
                    //       I should realize here that this is a 404 caused by the page in the 404 custom string
                    //           and display the 404 page. Even if all I can say is "the page was not found"
                    //
                    //--------------------------------------------------------------------------
                    //
                    if (genericController.vbLCase(core.webServer.requestPathPage) == genericController.vbLCase(requestAppRootPath + core.siteProperties.serverPageDefault)) {
                        //
                        // This is a 404 caused by Contensive returning a 404
                        //   possibly because the pageid was not found or was inactive.
                        //   contensive returned a 404 error, and the IIS custom error handler is hitting now
                        //   what we returned as an error cause is lost
                        //   ( because the Custom404Source page is the default page )
                        //   send it to the 404 page
                        //
                        core.webServer.requestPathPage = core.webServer.requestPathPage;
                        IsPageNotFound = true;
                        PageNotFoundReason = "The page could not be displayed. The record may have been deleted, marked inactive. The page's parent pages or section may be invalid.";
                    }
                }
                //if (false) {
                //    //
                //    //todo consider if we will keep this. It is not straightforward, and and more straightforward method may exist
                //    //
                //    // Determine where to go next
                //    //   If the current page is not the referring page, redirect to the referring page
                //    //   Because...
                //    //   - the page with the form (the referrer) was a link alias page. You can not post to a link alias, so internally we post to the default page, and redirect back.
                //    //   - This only acts on internal Contensive forms, so developer pages are not effected
                //    //   - This way, if the form post comes from a main_GetJSPage Remote Method, it posts to the Content Server,
                //    //       then redirects back to the static site (with the new changed content)
                //    //
                //    if (core.webServer.requestReferrer != "") {
                //        string main_ServerReferrerURL = null;
                //        string main_ServerReferrerQs = null;
                //        int Position = 0;
                //        main_ServerReferrerURL = core.webServer.requestReferrer;
                //        main_ServerReferrerQs = "";
                //        Position = genericController.vbInstr(1, main_ServerReferrerURL, "?");
                //        if (Position != 0) {
                //            main_ServerReferrerQs = main_ServerReferrerURL.Substring(Position);
                //            main_ServerReferrerURL = main_ServerReferrerURL.Left( Position - 1);
                //        }
                //        if (main_ServerReferrerURL.Substring(main_ServerReferrerURL.Length - 1) == "/") {
                //            //
                //            // Referer had no page, figure out what it should have been
                //            //
                //            if (core.webServer.requestPage != "") {
                //                //
                //                // If the referer had no page, and there is one here now, it must have been from an IIS redirect, use the current page as the default page
                //                //
                //                main_ServerReferrerURL = main_ServerReferrerURL + core.webServer.requestPage;
                //            } else {
                //                main_ServerReferrerURL = main_ServerReferrerURL + core.siteProperties.serverPageDefault;
                //            }
                //        }
                //        string linkDst = null;
                //        //main_ServerPage = main_ServerPage
                //        if (main_ServerReferrerURL != core.webServer.serverFormActionURL) {
                //            //
                //            // remove any methods from referrer
                //            //
                //            string Copy = "Redirecting because a Contensive Form was detected, source URL [" + main_ServerReferrerURL + "] does not equal the current URL [" + core.webServer.serverFormActionURL + "]. This may be from a Contensive Add-on that now needs to redirect back to the host page.";
                //            linkDst = core.webServer.requestReferer;
                //            if (!string.IsNullOrEmpty(main_ServerReferrerQs)) {
                //                linkDst = main_ServerReferrerURL;
                //                main_ServerReferrerQs = genericController.ModifyQueryString(main_ServerReferrerQs, "method", "");
                //                if (!string.IsNullOrEmpty(main_ServerReferrerQs)) {
                //                    linkDst = linkDst + "?" + main_ServerReferrerQs;
                //                }
                //            }
                //            return core.webServer.redirect(linkDst, Copy);
                //            core.doc.continueProcessing = false; //--- should be disposed by caller --- Call dispose
                //        }
                //    }
                //}
                // - same here, this was in appInit() to prcess the pagenotfounds - maybe here (at the end, maybe in page Manager)
                //--------------------------------------------------------------------------
                // ----- Process Early page-not-found
                //--------------------------------------------------------------------------
                //
                if (IsPageNotFound) {
                    //
                    // new way -- if a (real) 404 page is received, just convert this hit to the page-not-found page, do not redirect to it
                    //
                    logController.addSiteWarning(core, "Page Not Found", "Page Not Found", "", 0, "Page Not Found from [" + core.webServer.requestUrlSource + "]", "Page Not Found", "Page Not Found");
                    core.webServer.setResponseStatus("404 Not Found");
                    core.docProperties.setProperty(rnPageId, getPageNotFoundPageId(core));
                    //Call main_mergeInStream(rnPageId & "=" & main_GetPageNotFoundPageId())
                    if (core.session.isAuthenticatedAdmin(core)) {
                        core.doc.adminWarning = PageNotFoundReason;
                        core.doc.adminWarningPageID = 0;
                    }
                }
                //
                // add exception list header
                if (core.session.user.Developer) {
                    returnHtml = errorController.getDocExceptionHtmlList(core) + returnHtml;
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return returnHtml;
        }
        //
        //
        //
        private static void processForm(coreController core, int FormPageID) {
            try {
                //
                int CS = 0;
                string SQL = null;
                string Formhtml = "";
                string FormInstructions = "";
                main_FormPagetype f ;
                int Ptr = 0;
                int CSPeople = 0;
                bool IsInGroup = false;
                bool WasInGroup = false;
                string FormValue = null;
                bool Success = false;
                string PeopleFirstName = "";
                string PeopleLastName = "";
                string PeopleUsername = null;
                string PeoplePassword = null;
                string PeopleName = "";
                string PeopleEmail = "";
                string[] Groups = null;
                string GroupName = null;
                int GroupIDToJoinOnSuccess = 0;
                //
                // main_Get the instructions from the record
                //
                CS = core.db.csOpenRecord("Form Pages", FormPageID);
                if (core.db.csOk(CS)) {
                    Formhtml = core.db.csGetText(CS, "Body");
                    FormInstructions = core.db.csGetText(CS, "Instructions");
                }
                core.db.csClose(ref CS);
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
                    CSPeople = -1;
                    Success = true;
                    for (Ptr = 0; Ptr <= f.Inst.GetUpperBound(0); Ptr++) {
                        var tempVar = f.Inst[Ptr];
                        switch (tempVar.Type) {
                            case 1:
                                //
                                // People Record
                                //
                                FormValue = core.docProperties.getText(tempVar.PeopleField);
                                if ((!string.IsNullOrEmpty(FormValue)) & genericController.encodeBoolean(Models.Complex.cdefModel.GetContentFieldProperty(core, "people", tempVar.PeopleField, "uniquename"))) {
                                    SQL = "select count(*) from ccMembers where " + tempVar.PeopleField + "=" + core.db.encodeSQLText(FormValue);
                                    CS = core.db.csOpenSql(SQL);
                                    if (core.db.csOk(CS)) {
                                        Success = core.db.csGetInteger(CS, "cnt") == 0;
                                    }
                                    core.db.csClose(ref CS);
                                    if (!Success) {
                                        errorController.addUserError(core, "The field [" + tempVar.Caption + "] must be unique, and the value [" + genericController.encodeHTML(FormValue) + "] has already been used.");
                                    }
                                }
                                if ((tempVar.REquired | genericController.encodeBoolean(Models.Complex.cdefModel.GetContentFieldProperty(core, "people", tempVar.PeopleField, "required"))) && string.IsNullOrEmpty(FormValue)) {
                                    Success = false;
                                    errorController.addUserError(core, "The field [" + genericController.encodeHTML(tempVar.Caption) + "] is required.");
                                } else {
                                    if (!core.db.csOk(CSPeople)) {
                                        CSPeople = core.db.csOpenRecord("people", core.session.user.id);
                                    }
                                    if (core.db.csOk(CSPeople)) {
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
                        securityController.decodeToken(core,core.docProperties.getText("SuccessID"), ref GroupIDToJoinOnSuccess, ref tokenDate);
                        //GroupIDToJoinOnSuccess = main_DecodeKeyNumber(main_GetStreamText2("SuccessID"))
                        if (GroupIDToJoinOnSuccess != 0) {
                            groupController.group_AddGroupMember(core, groupController.group_GetGroupName(core, GroupIDToJoinOnSuccess));
                        }
                        //
                        // Join Groups requested by pageform
                        //
                        if (f.AddGroupNameList != "") {
                            Groups = (encodeText(f.AddGroupNameList).Trim(' ')).Split(',');
                            for (Ptr = 0; Ptr <= Groups.GetUpperBound(0); Ptr++) {
                                GroupName = Groups[Ptr].Trim(' ');
                                if (!string.IsNullOrEmpty(GroupName)) {
                                    groupController.group_AddGroupMember(core, GroupName);
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
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
                int PtrFront = 0;
                int PtrBack = 0;
                string[] i = null;
                int IPtr = 0;
                int IStart = 0;
                string[] IArgs = null;
                //
                if (true) {
                    PtrFront = genericController.vbInstr(1, Formhtml, "{{REPEATSTART", 1);
                    if (PtrFront > 0) {
                        PtrBack = genericController.vbInstr(PtrFront, Formhtml, "}}");
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
                                    i = genericController.SplitCRLF(FormInstructions);
                                    if (i.GetUpperBound(0) > 0) {
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
                                        for (IPtr = 0; IPtr <= i.GetUpperBound(0) - IStart; IPtr++) {
                                            var tempVar = result.Inst[IPtr];
                                            IArgs = i[IPtr + IStart].Split(',');
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
                core.handleException(ex);
            }
            return result;
        }
        //
        //
        //
        //
        internal static string getFormPage(coreController core, string FormPageName, int GroupIDToJoinOnSuccess) {
            string tempgetFormPage = null;
            try {
                //
                string RepeatBody = null;
                int IPtr = 0;
                int CSPeople = 0;
                string Body = null;
                string Formhtml = "";
                string FormInstructions = "";
                int CS = 0;
                bool HasRequiredFields = false;
                bool GroupValue = false;
                int GroupRowPtr = 0;
                int FormPageID = 0;
                main_FormPagetype f;
                bool IsRetry = false;
                string CaptionSpan = null;
                string Caption = null;
                //
                IsRetry = (core.docProperties.getInteger("ContensiveFormPageID") != 0);
                //
                CS = core.db.csOpen("Form Pages", "name=" + core.db.encodeSQLText(FormPageName));
                if (core.db.csOk(CS)) {
                    FormPageID = core.db.csGetInteger(CS, "ID");
                    Formhtml = core.db.csGetText(CS, "Body");
                    FormInstructions = core.db.csGetText(CS, "Instructions");
                }
                core.db.csClose(ref CS);
                f = loadFormPageInstructions(core, FormInstructions, Formhtml);
                //
                //
                //
                RepeatBody = "";
                CSPeople = -1;
                for (IPtr = 0; IPtr <= f.Inst.GetUpperBound(0); IPtr++) {
                    var tempVar = f.Inst[IPtr];
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
                            Body = genericController.vbReplace(Body, "{{CAPTION}}", core.html.inputCheckbox("Group" + tempVar.GroupName, GroupValue), 1, 99, 1);
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
                tempgetFormPage = ""
                + errorController.getUserError(core) + core.html.formStartMultipart() + core.html.inputHidden("ContensiveFormPageID", FormPageID) + core.html.inputHidden("SuccessID", securityController.encodeToken( core,GroupIDToJoinOnSuccess, core.doc.profileStartTime)) + f.PreRepeat + RepeatBody + f.PostRepeat + core.html.formEnd();
                //
                return tempgetFormPage;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError13("main_GetFormPage")
            return tempgetFormPage;
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
                DateTime DateModified = default(DateTime);
                int PageRecordID = 0;
                string PageName = null;
                int CS = 0;
                string SQL = null;
                bool ContentBlocked = false;
                int SystemEMailID = 0;
                int ConditionID = 0;
                int ConditionGroupID = 0;
                int main_AddGroupID = 0;
                int RemoveGroupID = 0;
                int RegistrationGroupID = 0;
                string[] BlockedPages = null;
                int BlockedPageRecordID = 0;
                string BlockCopy = null;
                int pageViewings = 0;
                //
                core.html.addHeadTag("<meta name=\"contentId\" content=\"" + core.doc.page.id + "\" >", "page content");
                //
                returnHtml = getContentBox_content(core, OrderByClause, AllowChildPageList, AllowReturnLink, ArchivePages, ignoreme, UseContentWatchLink, allowPageWithoutSectionDisplay);
                //
                // ----- If Link field populated, do redirect
                if (core.doc.page.PageLink != "") {
                    core.doc.page.Clicks += 1;
                    core.doc.page.save(core);
                    core.doc.redirectLink = core.doc.page.PageLink;
                    core.doc.redirectReason = "Redirect required because this page (PageRecordID=" + core.doc.page.id + ") has a Link Override [" + core.doc.page.PageLink + "].";
                    return "";
                }
                //
                // -- build list of blocked pages
                string BlockedRecordIDList = "";
                if ((!string.IsNullOrEmpty(returnHtml)) && (core.doc.redirectLink == "")) {
                    foreach (pageContentModel testPage in core.doc.pageToRootList) {
                        if (testPage.BlockContent | testPage.BlockPage) {
                            BlockedRecordIDList = BlockedRecordIDList + "," + testPage.id;
                        }
                    }
                    if (!string.IsNullOrEmpty(BlockedRecordIDList)) {
                        BlockedRecordIDList = BlockedRecordIDList.Substring(1);
                    }
                }
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
                        SQL = "SELECT DISTINCT ccPageContentBlockRules.RecordID"
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
                //
                //
                //
                if (ContentBlocked) {
                    string CustomBlockMessageFilename = "";
                    int BlockSourceID = main_BlockSourceDefaultMessage;
                    int ContentPadding = 20;
                    BlockedPages = BlockedRecordIDList.Split(',');
                    BlockedPageRecordID = genericController.encodeInteger(BlockedPages[BlockedPages.GetUpperBound(0)]);
                    if (BlockedPageRecordID != 0) {
                        CS = core.db.csOpenRecord("Page Content", BlockedPageRecordID,false, false, "CustomBlockMessage,BlockSourceID,RegistrationGroupID,ContentPadding");
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
                                            + core.addon.execute(addonModel.create(core, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext { addonType = CPUtilsBaseClass.addonContext.ContextPage }) + "";
                                    } else {
                                        //
                                        // -- recognized, not authenticated
                                        BlockForm = ""
                                            + "<p>This content has limited access. You were recognized as \"<b>" + core.session.user.name + "</b>\", but you need to login to continue. To login to this account or another, please use this form.</p>"
                                            + core.addon.execute(addonModel.create(core, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext { addonType = CPUtilsBaseClass.addonContext.ContextPage }) + "";
                                    }
                                } else {
                                    //
                                    // -- authenticated
                                    BlockForm = ""
                                        + "<p>You are currently logged in as \"<b>" + core.session.user.name + "</b>\". If this is not you, please <a href=\"?" + core.doc.refreshQueryString + "&method=logout\" rel=\"nofollow\">Click Here</a>.</p>"
                                        + "<p>This account does not have access to this content. If you want to login with a different account, please use this form.</p>"
                                        + core.addon.execute(addonModel.create(core, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext { addonType = CPUtilsBaseClass.addonContext.ContextPage }) + "";
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
                                        + core.addon.execute(addonModel.create(core, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext { addonType = CPUtilsBaseClass.addonContext.ContextPage }) + "";
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
                                        BlockCopy = ""
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
                    returnHtml = activeContentController.renderHtmlForWeb(core, returnHtml, pageContentModel.contentName, PageRecordID, core.doc.page.ContactMemberID, "http://" + core.webServer.requestDomain, core.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage);
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
                        pageViewings = core.doc.page.Viewings;
                        if (core.session.isEditing(pageContentModel.contentName) | core.visitProperty.getBoolean("AllowWorkflowRendering")) {
                            //
                            // Link authoring, workflow rendering -> do encoding, but no tracking
                            //
                            //returnHtml = contentCmdController.executeContentCommands(core, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, core.sessionContext.user.id, core.sessionContext.isAuthenticated, ref layoutError);
                            returnHtml = activeContentController.renderHtmlForWeb(core, returnHtml, pageContentModel.contentName, PageRecordID, core.doc.page.ContactMemberID, "http://" + core.webServer.requestDomain, core.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage);
                        } else {
                            //
                            // Live content
                            //returnHtml = contentCmdController.executeContentCommands(core, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, core.sessionContext.user.id, core.sessionContext.isAuthenticated, ref layoutError);
                            returnHtml = activeContentController.renderHtmlForWeb(core, returnHtml, pageContentModel.contentName, PageRecordID, core.doc.page.ContactMemberID, "http://" + core.webServer.requestDomain, core.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage);
                            core.db.executeQuery("update ccpagecontent set viewings=" + (pageViewings + 1) + " where id=" + core.doc.page.id);
                        }
                        //
                        // Page Hit Notification
                        //
                        if ((!core.session.visit.ExcludeFromAnalytics) & (core.doc.page.ContactMemberID != 0) && (core.webServer.requestBrowser.IndexOf("kmahttp", System.StringComparison.OrdinalIgnoreCase)  == -1)) {
                            personModel person = personModel.create(core, core.doc.page.ContactMemberID);
                            if ( person != null ) {
                                if (core.doc.page.AllowHitNotification) {
                                    PageName = core.doc.page.name;
                                    if (string.IsNullOrEmpty(PageName)) {
                                        PageName = core.doc.page.MenuHeadline;
                                        if (string.IsNullOrEmpty(PageName)) {
                                            PageName = core.doc.page.Headline;
                                            if (string.IsNullOrEmpty(PageName)) {
                                                PageName = "[no name]";
                                            }
                                        }
                                    }
                                    string Body = "";
                                    Body = Body + "<p><b>Page Hit Notification.</b></p>";
                                    Body = Body + "<p>This email was sent to you by the Contensive Server as a notification of the following content viewing details.</p>";
                                    Body = Body + genericController.StartTable(4, 1, 1);
                                    Body = Body + "<tr><td align=\"right\" width=\"150\" Class=\"ccPanelHeader\">Description<br><img alt=\"image\" src=\"http://" + core.webServer.requestDomain + "/ccLib/images/spacer.gif\" width=\"150\" height=\"1\"></td><td align=\"left\" width=\"100%\" Class=\"ccPanelHeader\">Value</td></tr>";
                                    Body = Body + getTableRow("Domain", core.webServer.requestDomain, true);
                                    Body = Body + getTableRow("Link", core.webServer.requestUrl, false);
                                    Body = Body + getTableRow("Page Name", PageName, true);
                                    Body = Body + getTableRow("Member Name", core.session.user.name, false);
                                    Body = Body + getTableRow("Member #", encodeText(core.session.user.id), true);
                                    Body = Body + getTableRow("Visit Start Time", encodeText(core.session.visit.StartTime), false);
                                    Body = Body + getTableRow("Visit #", encodeText(core.session.visit.id), true);
                                    Body = Body + getTableRow("Visit IP", core.webServer.requestRemoteIP, false);
                                    Body = Body + getTableRow("Browser ", core.webServer.requestBrowser, true);
                                    Body = Body + getTableRow("Visitor #", encodeText(core.session.visitor.id), false);
                                    Body = Body + getTableRow("Visit Authenticated", encodeText(core.session.visit.VisitAuthenticated), true);
                                    Body = Body + getTableRow("Visit Referrer", core.session.visit.HTTP_REFERER, false);
                                    Body = Body + kmaEndTable;
                                    string queryStringForLinkAppend = "";
                                    string emailStatus = "";
                                    emailController.sendPerson(core, person, core.siteProperties.getText("EmailFromAddress", "info@" + core.webServer.requestDomain), "Page Hit Notification", Body, false, true, 0, "", false, ref emailStatus, queryStringForLinkAppend);
                                }
                            }
                        }
                        //
                        // -- Process Trigger Conditions
                        ConditionID = core.doc.page.TriggerConditionID;
                        ConditionGroupID = core.doc.page.TriggerConditionGroupID;
                        main_AddGroupID = core.doc.page.TriggerAddGroupID;
                        RemoveGroupID = core.doc.page.TriggerRemoveGroupID;
                        SystemEMailID = core.doc.page.TriggerSendSystemEmailID;
                        switch (ConditionID) {
                            case 1:
                                //
                                // Always
                                //
                                if (SystemEMailID != 0) {
                                    emailController.sendSystem(core, core.db.getRecordName("System Email", SystemEMailID), "", core.session.user.id);
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
                                            emailController.sendSystem(core, core.db.getRecordName("System Email", SystemEMailID), "", core.session.user.id);
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
                                            emailController.sendSystem(core, core.db.getRecordName("System Email", SystemEMailID), "", core.session.user.id);
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
                    returnHtml = getContentBoxWrapper(core, returnHtml, core.doc.page.ContentPadding);
                    //
                    //---------------------------------------------------------------------------------
                    // ----- Set Headers
                    //---------------------------------------------------------------------------------
                    //
                    if (DateModified != DateTime.MinValue) {
                        core.webServer.addResponseHeader("LAST-MODIFIED", genericController.GetGMTFromDate(DateModified));
                    }
                    //
                    //---------------------------------------------------------------------------------
                    // ----- Store page javascript
                    //---------------------------------------------------------------------------------
                    // todo -- assets should all come from addons !!!
                    //
                    core.html.addScriptCode_onLoad(core.doc.page.JSOnLoad, "page content");
                    core.html.addScriptCode(core.doc.page.JSHead, "page content");
                    if (core.doc.page.JSFilename != "") {
                        core.html.addScriptLinkSrc(genericController.getCdnFileLink(core, core.doc.page.JSFilename), "page content");
                    }
                    core.html.addScriptCode(core.doc.page.JSEndBody, "page content");
                    //
                    //---------------------------------------------------------------------------------
                    // Set the Meta Content flag
                    //---------------------------------------------------------------------------------
                    //
                    core.html.addTitle(genericController.encodeHTML(core.doc.page.pageTitle), "page content");
                    core.html.addMetaDescription(genericController.encodeHTML(core.doc.page.metaDescription), "page content");
                    core.html.addHeadTag(core.doc.page.OtherHeadTags, "page content");
                    core.html.addMetaKeywordList(core.doc.page.MetaKeywordList, "page content");
                    //
                    Dictionary<string, string> instanceArguments = new Dictionary<string, string>();
                    instanceArguments.Add("CSPage", "-1");
                    CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                        instanceGuid = "-1",
                        instanceArguments = instanceArguments
                    };
                    //
                    // -- OnPageStartEvent
                    core.doc.bodyContent = returnHtml;
                    executeContext.addonType = CPUtilsBaseClass.addonContext.ContextOnPageStart;
                    List<addonModel> addonList = addonModel.createList_OnPageStartEvent(core, new List<string>());
                    foreach (Models.DbModels.addonModel addon in addonList) {
                        core.doc.bodyContent = core.addon.execute(addon, executeContext) + core.doc.bodyContent;
                        //AddonContent = core.addon.execute_legacy5(addon.id, addon.name, "CSPage=-1", CPUtilsBaseClass.addonContext.ContextOnPageStart, "", 0, "", -1)
                    }
                    returnHtml = core.doc.bodyContent;
                    //
                    // -- OnPageEndEvent / filter
                    core.doc.bodyContent = returnHtml;
                    executeContext.addonType = CPUtilsBaseClass.addonContext.ContextOnPageEnd;
                    foreach (addonModel addon in core.addonCache.getOnPageEndAddonList()) {
                        core.doc.bodyContent += core.addon.execute(addon, executeContext);
                        //core.doc.bodyContent &= core.addon.execute_legacy5(addon.id, addon.name, "CSPage=-1", CPUtilsBaseClass.addonContext.ContextOnPageStart, "", 0, "", -1)
                    }
                    returnHtml = core.doc.bodyContent;
                    //
                }
                //
                // -- title
                core.html.addTitle(core.doc.page.name);
                //
                // -- add contentid and sectionid
                core.html.addHeadTag("<meta name=\"contentId\" content=\"" + core.doc.page.id + "\" >", "page content");
                //
                // Display Admin Warnings with Edits for record errors
                //
                if (core.doc.adminWarning != "") {
                    //
                    if (core.doc.adminWarningPageID != 0) {
                        core.doc.adminWarning = core.doc.adminWarning + "</p>" + core.html.getRecordEditLink2("Page Content", core.doc.adminWarningPageID, true, "Page " + core.doc.adminWarningPageID, core.session.isAuthenticatedAdmin(core)) + "&nbsp;Edit the page<p>";
                        core.doc.adminWarningPageID = 0;
                    }
                    returnHtml = ""
                    + core.html.getAdminHintWrapper(core.doc.adminWarning) + returnHtml + "";
                    core.doc.adminWarning = "";
                }
            } catch (Exception ex) {
                core.handleException(ex);
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
                bool isEditing = false;
                string LiveBody = null;
                //
                if (core.doc.continueProcessing) {
                    if (core.doc.redirectLink == "") {
                        isEditing = core.session.isEditing(pageContentModel.contentName);
                        //
                        // ----- Render the Body
                        LiveBody = getContentBox_content_Body(core, OrderByClause, AllowChildPageList, false, core.doc.pageToRootList.Last().id, AllowReturnLink, pageContentModel.contentName, ArchivePages);
                        bool isRootPage = (core.doc.pageToRootList.Count == 1);
                        if (core.session.isAdvancedEditing(core, "")) {
                            result = result + core.html.getRecordEditLink(pageContentModel.contentName, core.doc.page.id, (!isRootPage)) + LiveBody;
                        } else if (isEditing) {
                            result = result + core.html.getEditWrapper("", core.html.getRecordEditLink(pageContentModel.contentName, core.doc.page.id, (!isRootPage)) + LiveBody);
                        } else {
                            result = result + LiveBody;
                        }
                    }
                }
                //
            } catch (Exception ex) {
                core.handleException(ex);
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
                bool allowChildListComposite = AllowChildList && core.doc.page.AllowChildListDisplay;
                bool allowReturnLinkComposite = AllowReturnLink && core.doc.page.AllowReturnLinkDisplay;
                string bodyCopy = core.doc.page.Copyfilename.content;
                string breadCrumb = "";
                string BreadCrumbDelimiter = null;
                string BreadCrumbPrefix = null;
                bool isRootPage = core.doc.pageToRootList.Count.Equals(1);
                //
                if (allowReturnLinkComposite && (!isRootPage)) {
                    //
                    // ----- Print Heading if not at root Page
                    //
                    BreadCrumbPrefix = core.siteProperties.getText("BreadCrumbPrefix", "Return to");
                    BreadCrumbDelimiter = core.siteProperties.getText("BreadCrumbDelimiter", " &gt; ");
                    breadCrumb = core.doc.getReturnBreadcrumb(RootPageContentName, core.doc.page.ParentID, rootPageId, "", ArchivePage, BreadCrumbDelimiter);
                    if (!string.IsNullOrEmpty(breadCrumb)) {
                        breadCrumb = "\r<p class=\"ccPageListNavigation\">" + BreadCrumbPrefix + " " + breadCrumb + "</p>";
                    }
                }
                result = result + breadCrumb;
                //
                if (true) {
                    string IconRow = "";
                    if ((!core.session.visit.Bot) & (core.doc.page.AllowPrinterVersion | core.doc.page.AllowEmailPage)) {
                        //
                        // not a bot, and either print or email allowed
                        //
                        if (core.doc.page.AllowPrinterVersion) {
                            string QueryString = core.doc.refreshQueryString;
                            QueryString = genericController.ModifyQueryString(QueryString, rnPageId, genericController.encodeText(core.doc.page.id), true);
                            QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, HardCodedPagePrinterVersion, true);
                            string Caption = core.siteProperties.getText("PagePrinterVersionCaption", "Printer Version");
                            Caption = genericController.vbReplace(Caption, " ", "&nbsp;");
                            IconRow = IconRow + "\r&nbsp;&nbsp;<a href=\"" + genericController.encodeHTML(core.webServer.requestPage + "?" + QueryString) + "\" target=\"_blank\"><img alt=\"image\" src=\"/ccLib/images/IconSmallPrinter.gif\" width=\"13\" height=\"13\" border=\"0\" align=\"absmiddle\"></a>&nbsp<a href=\"" + genericController.encodeHTML(core.webServer.requestPage + "?" + QueryString) + "\" target=\"_blank\" style=\"text-decoration:none! important;font-family:sanserif,verdana,helvetica;font-size:11px;\">" + Caption + "</a>";
                        }
                        if (core.doc.page.AllowEmailPage) {
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
                        result = result + "\r<div style=\"text-align:right;\">"
                        + genericController.htmlIndent(IconRow) + "\r</div>";
                    }
                }
                //
                // ----- Start Text Search
                //
                string Cell = "";
                if (core.session.isQuickEditing(core, pageContentModel.contentName)) {
                    Cell = Cell + core.doc.getQuickEditing(rootPageId, OrderByClause, AllowChildList, AllowReturnLink, ArchivePage, core.doc.page.ContactMemberID, core.doc.page.ChildListSortMethodID, allowChildListComposite, ArchivePage);
                } else {
                    //
                    // ----- Headline
                    //
                    if (core.doc.page.Headline != "") {
                        string headline = encodeHTML(core.doc.page.Headline);
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
                        + genericController.htmlIndent(bodyCopy) + "\r<!-- ContentBoxBodyEnd -->";
                    //
                    // ----- Child pages
                    if (allowChildListComposite || core.session.isEditingAnything()) {
                        if (!allowChildListComposite) {
                            Cell = Cell + core.html.getAdminHintWrapper("Automatic Child List display is disabled for this page. It is displayed here because you are in editing mode. To enable automatic child list display, see the features tab for this page.");
                        }
                        addonModel addon = addonModel.create(core, core.siteProperties.childListAddonID);
                        CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                            addonType = CPUtilsBaseClass.addonContext.ContextPage,
                            hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                                contentName = pageContentModel.contentName,
                                fieldName = "",
                                recordId = core.doc.page.id
                            },
                            instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(core, core.doc.page.ChildListInstanceOptions),
                            instanceGuid = PageChildListInstanceID,
                            wrapperID = core.siteProperties.defaultWrapperID
                        };
                        Cell += core.addon.execute(addon, executeContext);
                        //Cell = Cell & core.addon.execute_legacy2(core.siteProperties.childListAddonID, "", core.doc.page.ChildListInstanceOptions, CPUtilsBaseClass.addonContext.ContextPage, pageContentModel.contentName, core.doc.page.id, "", PageChildListInstanceID, False, core.siteProperties.defaultWrapperID, "", AddonStatusOK, Nothing)
                    }
                }
                //
                // ----- End Text Search
                result = result + "\r<!-- TextSearchStart -->"
                    + genericController.htmlIndent(Cell) + "\r<!-- TextSearchEnd -->";
                //
                // ----- Page See Also
                if (core.doc.page.AllowSeeAlso) {
                    result = result + "\r<div>"
                        + genericController.htmlIndent(getSeeAlso(core, pageContentModel.contentName, core.doc.page.id)) + "\r</div>";
                }
                //
                // ----- Allow More Info
                if ((core.doc.page.ContactMemberID != 0) & core.doc.page.AllowMoreInfo) {
                    result = result + "\r<ac TYPE=\"" + ACTypeContact + "\">";
                }
                //
                // ----- Feedback
                if ((core.doc.page.ContactMemberID != 0) & core.doc.page.AllowFeedback) {
                    result = result + "\r<ac TYPE=\"" + ACTypeFeedback + "\">";
                }
                //
                // ----- Last Modified line
                if ((core.doc.page.modifiedDate != DateTime.MinValue) & core.doc.page.AllowLastModifiedFooter) {
                    result = result + "\r<p>This page was last modified " + core.doc.page.modifiedDate.ToString("G");
                    if (core.session.isAuthenticatedAdmin(core)) {
                        if (core.doc.page.modifiedBy == 0) {
                            result = result + " (admin only: modified by unknown)";
                        } else {
                            string personName = core.db.getRecordName("people", core.doc.page.modifiedBy);
                            if (string.IsNullOrEmpty(personName)) {
                                result = result + " (admin only: modified by person with unnamed or deleted record #" + core.doc.page.modifiedBy + ")";
                            } else {
                                result = result + " (admin only: modified by " + personName + ")";
                            }
                        }
                    }
                    result = result + "</p>";
                }
                //
                // ----- Last Reviewed line
                if ((core.doc.page.DateReviewed != DateTime.MinValue) & core.doc.page.AllowReviewedFooter) {
                    result = result + "\r<p>This page was last reviewed " + core.doc.page.DateReviewed.ToString("");
                    if (core.session.isAuthenticatedAdmin(core)) {
                        if (core.doc.page.ReviewedBy == 0) {
                            result = result + " (by unknown)";
                        } else {
                            string personName = core.db.getRecordName("people", core.doc.page.ReviewedBy);
                            if (string.IsNullOrEmpty(personName)) {
                                result = result + " (by person with unnamed or deleted record #" + core.doc.page.ReviewedBy + ")";
                            } else {
                                result = result + " (by " + personName + ")";
                            }
                        }
                        result = result + ".</p>";
                    }
                }
                //
                // ----- Page Content Message Footer
                if (core.doc.page.AllowMessageFooter) {
                    string pageContentMessageFooter = core.siteProperties.getText("PageContentMessageFooter", "");
                    if (!string.IsNullOrEmpty(pageContentMessageFooter)) {
                        result = result + "\r<p>" + pageContentMessageFooter + "</p>";
                    }
                }
                //Call core.db.cs_Close(CS)
            } catch (Exception ex) {
                core.handleException(ex);
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
                int CS = 0;
                string SeeAlsoLink = null;
                int ContentID = 0;
                int SeeAlsoCount = 0;
                string Copy = null;
                string iContentName = null;
                int iRecordID = 0;
                bool IsEditingLocal = false;
                //
                iContentName = genericController.encodeText(ContentName);
                iRecordID = genericController.encodeInteger(RecordID);
                //
                SeeAlsoCount = 0;
                if (iRecordID > 0) {
                    ContentID = Models.Complex.cdefModel.getContentId(core, iContentName);
                    if (ContentID > 0) {
                        //
                        // ----- Set authoring only for valid ContentName
                        //
                        IsEditingLocal = core.session.isEditing(iContentName);
                    } else {
                        //
                        // ----- if iContentName was bad, maybe they put table in, no authoring
                        //
                        ContentID = Models.Complex.cdefModel.getContentIDByTablename(core, iContentName);
                    }
                    if (ContentID > 0) {
                        //
                        CS = core.db.csOpen("See Also", "((active<>0)AND(ContentID=" + ContentID + ")AND(RecordID=" + iRecordID + "))");
                        while (core.db.csOk(CS)) {
                            SeeAlsoLink = (core.db.csGetText(CS, "Link"));
                            if (!string.IsNullOrEmpty(SeeAlsoLink)) {
                                result = result + "\r<li class=\"ccListItem\">";
                                if (genericController.vbInstr(1, SeeAlsoLink, "://") == 0) {
                                    SeeAlsoLink = core.webServer.requestProtocol + SeeAlsoLink;
                                }
                                if (IsEditingLocal) {
                                    result = result + core.html.getRecordEditLink2("See Also", (core.db.csGetInteger(CS, "ID")), false, "", core.session.isEditing("See Also"));
                                }
                                result = result + "<a href=\"" + genericController.encodeHTML(SeeAlsoLink) + "\" target=\"_blank\">" + (core.db.csGetText(CS, "Name")) + "</A>";
                                Copy = (core.db.csGetText(CS, "Brief"));
                                if (!string.IsNullOrEmpty(Copy)) {
                                    result = result + "<br>" + genericController.AddSpan(Copy, "ccListCopy");
                                }
                                SeeAlsoCount = SeeAlsoCount + 1;
                                result = result + "</li>";
                            }
                            core.db.csGoNext(CS);
                        }
                        core.db.csClose(ref CS);
                        //
                        if (IsEditingLocal) {
                            SeeAlsoCount = SeeAlsoCount + 1;
                            result = result + "\r<li class=\"ccListItem\">" + core.html.getRecordAddLink("See Also", "RecordID=" + iRecordID + ",ContentID=" + ContentID) + "</LI>";
                        }
                    }
                    //
                    if (SeeAlsoCount == 0) {
                        result = "";
                    } else {
                        result = "<p>See Also\r<ul class=\"ccList\">" + genericController.htmlIndent(result) + "\r</ul></p>";
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        // Print the "for more information, please contact" line
        //
        //========================================================================
        //
        public string main_GetMoreInfo(coreController core, int contactMemberID) {
            string tempmain_GetMoreInfo = null;
            string result = "";
            try {
                tempmain_GetMoreInfo = getMoreInfoHtml(core, genericController.encodeInteger(contactMemberID));
            } catch (Exception ex) {
                core.handleException(ex);
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
        public static string main_GetFeedbackForm(coreController core, string ContentName, int RecordID, int ToMemberID, string headline = "") {
            string result = "";
            try {
                string Panel = null;
                string Copy = null;
                string FeedbackButton = null;
                string NoteCopy = "";
                string NoteFromEmail = null;
                string NoteFromName = null;
                int CS = 0;
                string iContentName = null;
                int iRecordID = 0;
                string iHeadline = null;
                //
                iContentName = genericController.encodeText(ContentName);
                iRecordID = genericController.encodeInteger(RecordID);
                iHeadline = genericController.encodeEmpty(headline, "");
                //
                const string FeedbackButtonSubmit = "Submit";
                //
                FeedbackButton = core.docProperties.getText("fbb");
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
                        if (!string.IsNullOrEmpty(iHeadline)) {
                            NoteCopy = NoteCopy + "    Article titled [" + iHeadline + "]" + BR;
                        }
                        NoteCopy = NoteCopy + "    Record [" + iRecordID + "] in Content Definition [" + iContentName + "]" + BR;
                        NoteCopy = NoteCopy + BR;
                        NoteCopy = NoteCopy + "<b>Comments</b>" + BR;
                        //
                        Copy = core.docProperties.getText("NoteCopy");
                        if (string.IsNullOrEmpty(Copy)) {
                            NoteCopy = NoteCopy + "[no comments entered]" + BR;
                        } else {
                            NoteCopy = NoteCopy + core.html.convertCRLFToHtmlBreak(Copy) + BR;
                        }
                        //
                        NoteCopy = NoteCopy + BR;
                        NoteCopy = NoteCopy + "<b>Content on which the comments are based</b>" + BR;
                        //
                        CS = core.db.csOpen(iContentName, "ID=" + iRecordID);
                        Copy = "[the content of this page is not available]" + BR;
                        if (core.db.csOk(CS)) {
                            Copy = (core.db.csGet(CS, "copyFilename"));
                            //Copy = main_EncodeContent5(Copy, c.authcontext.user.userid, iContentName, iRecordID, 0, False, False, True, True, False, True, "", "", False, 0)
                        }
                        NoteCopy = NoteCopy + Copy + BR;
                        core.db.csClose(ref CS);
                        //
                        personModel person = personModel.create(core, ToMemberID);
                        if ( person != null ) {
                            string sendStatus = "";
                            string queryStringForLinkAppend = "";
                            emailController.sendPerson(core, person, NoteFromEmail, "Feedback Form Submitted", NoteCopy, false, true, 0, "", false,ref sendStatus, queryStringForLinkAppend);
                        }
                        //
                        // ----- Note sent, say thanks
                        //
                        result = result + "<p>Thank you. Your feedback was received.</p>";
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
                        Panel = Panel + "<td align=\"left\"><input type=\"text\" name=\"NoteFromName\" value=\"" + genericController.encodeHTML(Copy) + "\"></span></td>";
                        Panel = Panel + "</tr><tr>";
                        //
                        // ----- From Email address
                        //
                        Copy = core.session.user.Email;
                        Panel = Panel + "<td align=\"right\" width=\"100\"><p>Your Email</p></td>";
                        Panel = Panel + "<td align=\"left\"><input type=\"text\" name=\"NoteFromEmail\" value=\"" + genericController.encodeHTML(Copy) + "\"></span></td>";
                        Panel = Panel + "</tr><tr>";
                        //
                        // ----- Message
                        //
                        Copy = "";
                        Panel = Panel + "<td align=\"right\" width=\"100\" valign=\"top\"><p>Feedback</p></td>";
                        Panel = Panel + "<td>" + core.html.inputText("NoteCopy", Copy, 4, 40, "TextArea", false) + "</td>";
                        //Panel = Panel & "<td><textarea ID=""TextArea"" rows=""4"" cols=""40"" name=""NoteCopy"">" & Copy & "</textarea></td>"
                        Panel = Panel + "</tr><tr>";
                        //
                        // ----- submit button
                        //
                        Panel = Panel + "<td>&nbsp;</td>";
                        Panel = Panel + "<td>" + htmlController.getButton(FeedbackButtonSubmit, "fbb") + "</td>";
                        //Panel = Panel + "<td><input type=\"submit\" name=\"fbb\" value=\"" + FeedbackButtonSubmit + "\"></td>";
                        Panel = Panel + "</tr></table>";
                        Panel = Panel + "</form>";
                        //
                        result = Panel;
                        break;
                }
            } catch (Exception ex) {
                core.handleException(ex);
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