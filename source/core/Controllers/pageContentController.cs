
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
using Contensive.Core.Models.Entity;
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
        public static string getHtmlBodyTemplate(coreClass cpCore) {
            string returnBody = "";
            try {
                string AddonReturn = null;
                string layoutError = "";
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
                foreach (addonModel addon in cpCore.addonCache.getOnBodyStartAddonList()) {
                    returnBody += cpCore.addon.execute(addon, bodyStartContext);
                }
                //
                // ----- main_Get Content (Already Encoded)
                //
                blockSiteWithLogin = false;
                PageContent = getHtmlBodyTemplateContent(cpCore, true, true, false, ref blockSiteWithLogin);
                if (blockSiteWithLogin) {
                    //
                    // section blocked, just return the login box in the page content
                    //
                    returnBody = ""
                        + "\r<div class=\"ccLoginPageCon\">"
                        + genericController.htmlIndent(PageContent) + "\r</div>"
                        + "";
                } else if (!cpCore.doc.continueProcessing) {
                    //
                    // exit if stream closed during main_GetSectionpage
                    //
                    returnBody = "";
                } else {
                    //
                    // -- no section block, continue
                    LocalTemplateID = cpCore.doc.template.id;
                    LocalTemplateBody = cpCore.doc.template.bodyHTML;
                    if (string.IsNullOrEmpty(LocalTemplateBody)) {
                        LocalTemplateBody = TemplateDefaultBody;
                    }
                    LocalTemplateName = cpCore.doc.template.name;
                    if (string.IsNullOrEmpty(LocalTemplateName)) {
                        LocalTemplateName = "Template " + LocalTemplateID;
                    }
                    //
                    // ----- Encode Template
                    //
                    LocalTemplateBody = contentCmdController.executeContentCommands(cpCore, LocalTemplateBody, CPUtilsBaseClass.addonContext.ContextTemplate, cpCore.doc.sessionContext.user.id, cpCore.doc.sessionContext.isAuthenticated, ref layoutError);
                    returnBody += activeContentController.convertActiveContentToHtmlForWebRender(cpCore, LocalTemplateBody, "Page Templates", LocalTemplateID, 0, cpCore.webServer.requestProtocol + cpCore.webServer.requestDomain, cpCore.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextTemplate);
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
                    if (!cpCore.doc.sessionContext.isAuthenticated) {
                        //
                        // not logged in
                        //
                    } else {
                        //
                        // Add template editing
                        //
                        if (cpCore.visitProperty.getBoolean("AllowAdvancedEditor") & cpCore.doc.sessionContext.isEditing("Page Templates")) {
                            returnBody = cpCore.html.getEditWrapper("Page Template [" + LocalTemplateName + "]", cpCore.html.getRecordEditLink2("Page Templates", LocalTemplateID, false, LocalTemplateName, cpCore.doc.sessionContext.isEditing("Page Templates")) + returnBody);
                        }
                    }
                    //
                    // ----- OnBodyEnd add-ons
                    //
                    CPUtilsBaseClass.addonExecuteContext bodyEndContext = new CPUtilsBaseClass.addonExecuteContext() { addonType = CPUtilsBaseClass.addonContext.ContextFilter };
                    foreach (var addon in cpCore.addonCache.getOnBodyEndAddonList()) {
                        cpCore.doc.docBodyFilter = returnBody;
                        AddonReturn = cpCore.addon.execute(addon, bodyEndContext);
                        //AddonReturn = cpCore.addon.execute_legacy2(addon.id, "", "", CPUtilsBaseClass.addonContext.ContextFilter, "", 0, "", "", False, 0, "", False, Nothing)
                        returnBody = cpCore.doc.docBodyFilter + AddonReturn;
                    }
                    //
                    // -- Make it pretty for those who care
                    returnBody = htmlReflowController.reflow(cpCore, returnBody);
                }
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
        public static string getHtmlBodyTemplateContent(coreClass cpCore, bool AllowChildPageList, bool AllowReturnLink, bool AllowEditWrapper, ref bool return_blockSiteWithLogin) {
            string returnHtml = "";
            try {
                bool allowPageWithoutSectionDislay = false;
                int FieldRows = 0;
                int PageID = 0;
                bool UseContentWatchLink = cpCore.siteProperties.useContentWatchLink;
                //
                // -- validate domain
                if (cpCore.doc.domain == null) {
                    //
                    // -- domain not listed, this is now an error
                    logController.appendLogPageNotFound(cpCore, cpCore.webServer.requestUrlSource);
                    return "<div style=\"width:300px; margin: 100px auto auto auto;text-align:center;\">The domain name is not configured for this site.</div>";
                }
                //
                // -- validate page
                if (cpCore.doc.page.id == 0) {
                    //
                    // -- landing page is not valid -- display error
                    logController.appendLogPageNotFound(cpCore, cpCore.webServer.requestUrlSource);
                    cpCore.doc.redirectBecausePageNotFound = true;
                    cpCore.doc.redirectReason = "Redirecting because the page selected could not be found.";
                    cpCore.doc.redirectLink = pageContentController.main_ProcessPageNotFound_GetLink(cpCore, cpCore.doc.redirectReason, "", "", PageID, 0);
                    cpCore.handleException(new ApplicationException("Page could not be determined. Error message displayed."));
                    return "<div style=\"width:300px; margin: 100px auto auto auto;text-align:center;\">This page is not valid.</div>";
                }
                //PageID = cpCore.docProperties.getInteger(rnPageId)
                //If (PageID = 0) Then
                //    '
                //    ' -- Nothing specified, use the Landing Page
                //    PageID = main_GetLandingPageID()
                //    If PageID = 0 Then
                //        '
                //        ' -- landing page is not valid -- display error
                //        Call logController.log_appendLogPageNotFound(cpCore, cpCore.webServer.requestUrlSource)
                //        RedirectBecausePageNotFound = True
                //        RedirectReason = "Redirecting because the page selected could not be found."
                //        redirectLink = main_ProcessPageNotFound_GetLink(RedirectReason, , , PageID, 0)

                //        cpCore.handleExceptionAndContinue(New ApplicationException("Page could not be determined. Error message displayed."))
                //        Return "<div style=""width:300px; margin: 100px auto auto auto;text-align:center;"">This page is not valid.</div>"
                //    End If
                //End If
                //Call cpCore.htmlDoc.webServerIO_addRefreshQueryString(rnPageId, CStr(PageID))
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
                //    Dim targetpage As pageContentModel = pageContentModel.create(cpCore, targetPageId, New List(Of String))
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
                //    cpCore.handleExceptionAndContinue(New ApplicationException("Page could not be determined. Error message displayed."))
                //    Return "<div style=""width:300px; margin: 100px auto auto auto;text-align:center;"">This page is not valid.</div>"
                //End If
                //page = pageToRootList.First
                //
                // -- get template from pages
                //Dim template As pageTemplateModel = Nothing
                //For Each page As pageContentModel In pageToRootList
                //    If page.TemplateID > 0 Then
                //        template = pageTemplateModel.create(cpCore, page.TemplateID, New List(Of String))
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
                //        template = pageTemplateModel.create(cpCore, domain.DefaultTemplateId, New List(Of String))
                //    End If
                //    If (template Is Nothing) Then
                //        '
                //        ' -- get template named Default
                //        template = pageTemplateModel.createByName(cpCore, "default", New List(Of String))
                //    End If
                //End If
                //
                // -- get contentbox
                returnHtml = getContentBox(cpCore, "", AllowChildPageList, AllowReturnLink, false, 0, UseContentWatchLink, allowPageWithoutSectionDislay);
                //
                // -- if fpo_QuickEdit it there, replace it out
                string Editor = null;
                string styleOptionList = "";
                string addonListJSON = null;
                if (cpCore.doc.redirectLink == "" && (returnHtml.IndexOf(html_quickEdit_fpo)  != -1)) {
                    FieldRows = genericController.encodeInteger(cpCore.userProperty.getText("Page Content.copyFilename.PixelHeight", "500"));
                    if (FieldRows < 50) {
                        FieldRows = 50;
                        cpCore.userProperty.setProperty("Page Content.copyFilename.PixelHeight", 50);
                    }
                    string stylesheetCommaList = ""; //cpCore.html.main_GetStyleSheet2(csv_contentTypeEnum.contentTypeWeb, templateId, 0)
                    addonListJSON = cpCore.html.JSONeditorAddonList(csv_contentTypeEnum.contentTypeWeb);
                    Editor = cpCore.html.getFormInputHTML("copyFilename", cpCore.doc.quickEditCopy, FieldRows.ToString(), "100%", false, true, addonListJSON, stylesheetCommaList, styleOptionList);
                    returnHtml = genericController.vbReplace(returnHtml, html_quickEdit_fpo, Editor);
                }
                //
                // -- Add admin warning to the top of the content
                if (cpCore.doc.sessionContext.isAuthenticatedAdmin(cpCore) & cpCore.doc.adminWarning != "") {
                    //
                    // Display Admin Warnings with Edits for record errors
                    //
                    if (cpCore.doc.adminWarningPageID != 0) {
                        cpCore.doc.adminWarning = cpCore.doc.adminWarning + "</p>" + cpCore.html.getRecordEditLink2("Page Content", cpCore.doc.adminWarningPageID, true, "Page " + cpCore.doc.adminWarningPageID, cpCore.doc.sessionContext.isAuthenticatedAdmin(cpCore)) + "&nbsp;Edit the page<p>";
                        cpCore.doc.adminWarningPageID = 0;
                    }
                    //
                    returnHtml = ""
                    + cpCore.html.getAdminHintWrapper(cpCore.doc.adminWarning) + returnHtml + "";
                    cpCore.doc.adminWarning = "";
                }
                //
                // -- handle redirect and edit wrapper
                //------------------------------------------------------------------------------------
                //
                if (cpCore.doc.redirectLink != "") {
                    return cpCore.webServer.redirect(cpCore.doc.redirectLink, cpCore.doc.redirectReason, cpCore.doc.redirectBecausePageNotFound);
                } else if (AllowEditWrapper) {
                    returnHtml = cpCore.html.getEditWrapper("Page Content", returnHtml);
                }
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return returnHtml;
        }
        //
        //=============================================================================
        //   Add content padding around content
        //       is called from main_GetPageRaw, as well as from higher up when blocking is turned on
        //=============================================================================
        //
        internal static string getContentBoxWrapper(coreClass cpcore, string Content, int ContentPadding) {
            string tempgetContentBoxWrapper = null;
            //dim buildversion As String
            //
            // BuildVersion = app.dataBuildVersion
            tempgetContentBoxWrapper = Content;
            if (cpcore.siteProperties.getBoolean("Compatibility ContentBox Pad With Table")) {
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
        internal static string getDefaultBlockMessage(coreClass cpcore, bool UseContentWatchLink) {
            string tempgetDefaultBlockMessage = null;
            try {
                tempgetDefaultBlockMessage = "";
                int CS = cpcore.db.csOpen("Copy Content", "name=" + cpcore.db.encodeSQLText(ContentBlockCopyName), "ID", false, 0, false, false, "Copy,ID");
                if (cpcore.db.csOk(CS)) {
                    tempgetDefaultBlockMessage = cpcore.db.csGet(CS, "Copy");
                }
                cpcore.db.csClose(ref CS);
                //
                // ----- Do not allow blank message - if still nothing, create default
                //
                if (string.IsNullOrEmpty(tempgetDefaultBlockMessage)) {
                    tempgetDefaultBlockMessage = "<p>The content on this page has restricted access. If you have a username and password for this system, <a href=\"?method=login\" rel=\"nofollow\">Click Here</a>. For more information, please contact the administrator.</p>";
                }
                //
                // ----- Create Copy Content Record for future
                //
                CS = cpcore.db.csInsertRecord("Copy Content");
                if (cpcore.db.csOk(CS)) {
                    cpcore.db.csSet(CS, "Name", ContentBlockCopyName);
                    cpcore.db.csSet(CS, "Copy", tempgetDefaultBlockMessage);
                    cpcore.db.csSave2(CS);
                    //Call cpcore.workflow.publishEdit("Copy Content", genericController.EncodeInteger(cpcore.db.cs_get(CS, "ID")))
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
            }
            return tempgetDefaultBlockMessage;
        }
        //
        //
        //
        //
        public static string getMoreInfoHtml(coreClass cpCore, int PeopleID) {
            string result = "";
            try {
                int CS = 0;
                string ContactName = null;
                string ContactPhone = null;
                string ContactEmail = null;
                string Copy;
                //
                Copy = "";
                CS = cpCore.db.cs_openContentRecord("People", PeopleID, 0, false, false, "Name,Phone,Email");
                if (cpCore.db.csOk(CS)) {
                    ContactName = (cpCore.db.csGetText(CS, "Name"));
                    ContactPhone = (cpCore.db.csGetText(CS, "Phone"));
                    ContactEmail = (cpCore.db.csGetText(CS, "Email"));
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
                cpCore.db.csClose(ref CS);
                //
                result = Copy;
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
        public static void loadPage(coreClass cpcore, int requestedPageId, domainModel domain) {
            try {
                if (domain == null) {
                    //
                    // -- domain is not valid
                    cpcore.handleException(new ApplicationException("Page could not be determined because the domain was not recognized."));
                } else {
                    //
                    // -- attempt requested page
                    pageContentModel requestedPage = null;
                    if (!requestedPageId.Equals(0)) {
                        requestedPage = pageContentModel.create(cpcore, requestedPageId);
                        if (requestedPage == null) {
                            //
                            // -- requested page not found
                            requestedPage = pageContentModel.create(cpcore, getPageNotFoundPageId(cpcore));
                        }
                    }
                    if (requestedPage == null) {
                        //
                        // -- use the Landing Page
                        requestedPage = getLandingPage(cpcore, domain);
                    }
                    cpcore.doc.addRefreshQueryString(rnPageId, encodeText(requestedPage.id));
                    //
                    // -- build parentpageList (first = current page, last = root)
                    // -- add a 0, then repeat until another 0 is found, or there is a repeat
                    cpcore.doc.pageToRootList = new List<Models.Entity.pageContentModel>();
                    List<int> usedPageIdList = new List<int>();
                    int targetPageId = requestedPage.id;
                    usedPageIdList.Add(0);
                    while (!usedPageIdList.Contains(targetPageId)) {
                        usedPageIdList.Add(targetPageId);
                        pageContentModel targetpage = pageContentModel.create(cpcore, targetPageId );
                        if (targetpage == null) {
                            break;
                        } else {
                            cpcore.doc.pageToRootList.Add(targetpage);
                            targetPageId = targetpage.ParentID;
                        }
                    }
                    if (cpcore.doc.pageToRootList.Count == 0) {
                        //
                        // -- attempt failed, create default page
                        cpcore.doc.page = pageContentModel.add(cpcore);
                        cpcore.doc.page.name = DefaultNewLandingPageName + ", " + domain.name;
                        cpcore.doc.page.Copyfilename.content = landingPageDefaultHtml;
                        cpcore.doc.page.save(cpcore);
                        cpcore.doc.pageToRootList.Add(cpcore.doc.page);
                    } else {
                        cpcore.doc.page = cpcore.doc.pageToRootList.First();
                    }
                    //
                    // -- get template from pages
                    cpcore.doc.template = null;
                    foreach (Models.Entity.pageContentModel page in cpcore.doc.pageToRootList) {
                        if (page.TemplateID > 0) {
                            cpcore.doc.template = pageTemplateModel.create(cpcore, page.TemplateID );
                            if (cpcore.doc.template == null) {
                                //
                                // -- templateId is not valid
                                page.TemplateID = 0;
                                page.save(cpcore);
                            } else {
                                if (page == cpcore.doc.pageToRootList.First()) {
                                    cpcore.doc.templateReason = "This template was used because it is selected by the current page.";
                                } else {
                                    cpcore.doc.templateReason = "This template was used because it is selected one of this page's parents [" + page.name + "].";
                                }
                                break;
                            }
                        }
                    }
                    //
                    if (cpcore.doc.template == null) {
                        //
                        // -- get template from domain
                        if (domain != null) {
                            if (domain.defaultTemplateId > 0) {
                                cpcore.doc.template = pageTemplateModel.create(cpcore, domain.defaultTemplateId );
                                if (cpcore.doc.template == null) {
                                    //
                                    // -- domain templateId is not valid
                                    domain.defaultTemplateId = 0;
                                    domain.save(cpcore);
                                }
                            }
                        }
                        if (cpcore.doc.template == null) {
                            //
                            // -- get template named Default
                            cpcore.doc.template = pageTemplateModel.createByName(cpcore, defaultTemplateName);
                            if (cpcore.doc.template == null) {
                                //
                                // -- ceate new template named Default
                                cpcore.doc.template = pageTemplateModel.add(cpcore );
                                cpcore.doc.template.name = defaultTemplateName;
                                cpcore.doc.template.bodyHTML = cpcore.appRootFiles.readFile(defaultTemplateHomeFilename);
                                cpcore.doc.template.save(cpcore);
                            }
                            //
                            // -- set this new template to all domains without a template
                            foreach (domainModel d in domainModel.createList(cpcore, "((DefaultTemplateId=0)or(DefaultTemplateId is null))")) {
                                d.defaultTemplateId = cpcore.doc.template.id;
                                d.save(cpcore);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
            }
        }
        public static int getPageNotFoundPageId(coreClass cpcore) {
            int pageId = 0;
            try {
                pageId = cpcore.domain.pageNotFoundPageId;
                if (pageId == 0) {
                    //
                    // no domain page not found, use site default
                    //
                    pageId = cpcore.siteProperties.getInteger("PageNotFoundPageID", 0);
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return pageId;
        }
        //
        //---------------------------------------------------------------------------
        //
        //---------------------------------------------------------------------------
        //
        public static pageContentModel getLandingPage(coreClass cpcore, domainModel domain) {
            pageContentModel landingPage = null;
            try {
                if (domain == null) {
                    //
                    // -- domain not available
                    cpcore.handleException(new ApplicationException("Landing page could not be determined because the domain was not recognized."));
                } else {
                    //
                    // -- attempt domain landing page
                    if (!domain.rootPageId.Equals(0)) {
                        landingPage = pageContentModel.create(cpcore, domain.rootPageId);
                        if (landingPage == null) {
                            domain.rootPageId = 0;
                            domain.save(cpcore);
                        }
                    }
                    if (landingPage == null) {
                        //
                        // -- attempt site landing page
                        int siteLandingPageID = cpcore.siteProperties.getInteger("LandingPageID", 0);
                        if (!siteLandingPageID.Equals(0)) {
                            landingPage = pageContentModel.create(cpcore, siteLandingPageID);
                            if (landingPage == null) {
                                cpcore.siteProperties.setProperty("LandingPageID", 0);
                                domain.rootPageId = 0;
                                domain.save(cpcore);
                            }
                        }
                        if (landingPage == null) {
                            //
                            // -- create detault landing page
                            landingPage = pageContentModel.add(cpcore);
                            landingPage.name = DefaultNewLandingPageName + ", " + domain.name;
                            landingPage.Copyfilename.content = landingPageDefaultHtml;
                            landingPage.save(cpcore);
                            cpcore.doc.landingPageID = landingPage.id;
                        }
                        //
                        // -- save new page to the domain
                        domain.rootPageId = landingPage.id;
                        domain.save(cpcore);
                    }
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return landingPage;
        }
        //
        // Verify a link from the template link field to be used as a Template Link
        //
        internal static string verifyTemplateLink(coreClass cpcore, string linkSrc) {
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
                    tempverifyTemplateLink = genericController.EncodeAppRootPath(tempverifyTemplateLink, cpcore.webServer.requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain);
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
                    tempverifyTemplateLink = genericController.ConvertLinkToShortLink(tempverifyTemplateLink, cpcore.webServer.requestDomain, cpcore.webServer.requestVirtualFilePath);
                    tempverifyTemplateLink = genericController.EncodeAppRootPath(tempverifyTemplateLink, cpcore.webServer.requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain);
                }
            }
            return tempverifyTemplateLink;
        }
        //
        //
        //
        internal static string main_ProcessPageNotFound_GetLink(coreClass cpcore, string adminMessage, string BackupPageNotFoundLink = "", string PageNotFoundLink = "", int EditPageID = 0, int EditSectionID = 0) {
            string result = "";
            try {
                int Pos = 0;
                string DefaultLink = null;
                int PageNotFoundPageID = 0;
                string Link = null;
                //
                PageNotFoundPageID = getPageNotFoundPageId(cpcore);
                if (PageNotFoundPageID == 0) {
                    //
                    // No PageNotFound was set -- use the backup link
                    //
                    if (string.IsNullOrEmpty(BackupPageNotFoundLink)) {
                        adminMessage = adminMessage + " The Site Property 'PageNotFoundPageID' is not set so the Landing Page was used.";
                        Link = cpcore.doc.landingLink;
                    } else {
                        adminMessage = adminMessage + " The Site Property 'PageNotFoundPageID' is not set.";
                        Link = BackupPageNotFoundLink;
                    }
                } else {
                    //
                    // Set link
                    //
                    Link = getPageLink(cpcore, PageNotFoundPageID, "", true, false);
                    DefaultLink = getPageLink(cpcore, 0, "", true, false);
                    if (Link != DefaultLink) {
                    } else {
                        adminMessage = adminMessage + "</p><p>The current 'Page Not Found' could not be used. It is not valid, or it is not associated with a valid site section. To configure a valid 'Page Not Found' page, first create the page as a child page on your site and check the 'Page Not Found' checkbox on it's control tab. The Landing Page was used.";
                    }
                }
                //
                // Add the Admin Message to the link
                //
                if (cpcore.doc.sessionContext.isAuthenticatedAdmin(cpcore)) {
                    if (string.IsNullOrEmpty(PageNotFoundLink)) {
                        PageNotFoundLink = cpcore.webServer.requestUrl;
                    }
                    //
                    // Add the Link to the Admin Msg
                    //
                    adminMessage = adminMessage + "<p>The URL was " + PageNotFoundLink + ".";
                    //
                    // Add the Referrer to the Admin Msg
                    //
                    if (cpcore.webServer.requestReferer != "") {
                        Pos = genericController.vbInstr(1, cpcore.webServer.requestReferrer, "main_AdminWarningPageID=", 1);
                        if (Pos != 0) {
                            cpcore.webServer.requestReferrer = cpcore.webServer.requestReferrer.Left( Pos - 2);
                        }
                        Pos = genericController.vbInstr(1, cpcore.webServer.requestReferrer, "main_AdminWarningMsg=", 1);
                        if (Pos != 0) {
                            cpcore.webServer.requestReferrer = cpcore.webServer.requestReferrer.Left( Pos - 2);
                        }
                        Pos = genericController.vbInstr(1, cpcore.webServer.requestReferrer, "blockcontenttracking=", 1);
                        if (Pos != 0) {
                            cpcore.webServer.requestReferrer = cpcore.webServer.requestReferrer.Left( Pos - 2);
                        }
                        adminMessage = adminMessage + " The referring page was " + cpcore.webServer.requestReferrer + ".";
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
                cpcore.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public static string getPageLink(coreClass cpcore, int PageID, string QueryStringSuffix, bool AllowLinkAliasIfEnabled = true, bool UseContentWatchNotDefaultPage = false) {
            string result = "";
            try {
                string linkPathPage = null;
                //
                // -- set linkPathPath to linkAlias
                if (AllowLinkAliasIfEnabled && cpcore.siteProperties.allowLinkAlias) {
                    linkPathPage = linkAliasController.getLinkAlias(cpcore, PageID, QueryStringSuffix, "");
                }
                if (string.IsNullOrEmpty(linkPathPage)) {
                    //
                    // -- if not linkAlis, set default page and qs
                    linkPathPage = cpcore.siteProperties.serverPageDefault;
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
                string SqlCriteria = "(domainId=" + cpcore.doc.domain.id + ")";
                List<Models.Entity.TemplateDomainRuleModel> allowTemplateRuleList = TemplateDomainRuleModel.createList(cpcore, SqlCriteria);
                bool templateAllowed = false;
                foreach (TemplateDomainRuleModel rule in allowTemplateRuleList) {
                    if (rule.templateId == cpcore.doc.template.id) {
                        templateAllowed = true;
                        break;
                    }
                }
                string linkDomain = "";
                if (allowTemplateRuleList.Count == 0) {
                    //
                    // this template has no domain preference, use current domain
                    //
                    linkDomain = cpcore.webServer.requestDomain;
                } else if (cpcore.domain.id == 0) {
                    //
                    // the current domain is not recognized, or is default - use it
                    //
                    linkDomain = cpcore.webServer.requestDomain;
                } else if (templateAllowed) {
                    //
                    // current domain is in the allowed domain list
                    //
                    linkDomain = cpcore.webServer.requestDomain;
                } else {
                    //
                    // there is an allowed domain list and current domain is not on it, or use first
                    //
                    int setdomainId = allowTemplateRuleList.First().domainId;
                    linkDomain = cpcore.db.getRecordName("domains", setdomainId);
                    if (string.IsNullOrEmpty(linkDomain)) {
                        linkDomain = cpcore.webServer.requestDomain;
                    }
                }
                //
                // -- protocol
                string linkprotocol = "";
                if (cpcore.doc.page.IsSecure | cpcore.doc.template.isSecure) {
                    linkprotocol = "https://";
                } else {
                    linkprotocol = "http://";
                }
                //
                // -- assemble
                result = linkprotocol + linkDomain + linkPathPage;
            } catch (Exception ex) {
                cpcore.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        // main_Get a page link if you know nothing about the page
        //   If you already have all the info, lik the parents templateid, etc, call the ...WithArgs call
        //====================================================================================================
        //
        public static string main_GetPageLink3(coreClass cpcore, int PageID, string QueryStringSuffix, bool AllowLinkAlias) {
            return getPageLink(cpcore, PageID, QueryStringSuffix, AllowLinkAlias, false);
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
        public static bool isChildRecord(coreClass cpcore, string ContentName, int ChildRecordID, int ParentRecordID) {
            bool tempisChildRecord = false;
            try {
                //
                Models.Complex.cdefModel CDef = null;
                //
                tempisChildRecord = (ChildRecordID == ParentRecordID);
                if (!tempisChildRecord) {
                    CDef = Models.Complex.cdefModel.getCdef(cpcore, ContentName);
                    if (genericController.IsInDelimitedString(CDef.SelectCommaList.ToUpper(), "PARENTID", ",")) {
                        tempisChildRecord = main_IsChildRecord_Recurse(cpcore, CDef.ContentDataSourceName, CDef.ContentTableName, ChildRecordID, ParentRecordID, "");
                    }
                }
                return tempisChildRecord;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                cpcore.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("cpCoreClass.IsChildRecord")
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
        internal static bool main_IsChildRecord_Recurse(coreClass cpcore, string DataSourceName, string TableName, int ChildRecordID, int ParentRecordID, string History) {
            bool result = false;
            try {
                string SQL = null;
                int CS = 0;
                int ChildRecordParentID = 0;
                //
                SQL = "select ParentID from " + TableName + " where id=" + ChildRecordID;
                CS = cpcore.db.csOpenSql(SQL);
                if (cpcore.db.csOk(CS)) {
                    ChildRecordParentID = cpcore.db.csGetInteger(CS, "ParentID");
                }
                cpcore.db.csClose(ref CS);
                if ((ChildRecordParentID != 0) & (!genericController.IsInDelimitedString(History, ChildRecordID.ToString(), ","))) {
                    result = (ParentRecordID == ChildRecordParentID);
                    if (!result) {
                        result = main_IsChildRecord_Recurse(cpcore, DataSourceName, TableName, ChildRecordParentID, ParentRecordID, History + "," + ChildRecordID.ToString());
                    }
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
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
        public static string getHtmlBody(coreClass cpCore) {
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
                if (cpCore.doc.continueProcessing) {
                    //
                    // -- setup domain
                    string domainTest = cpCore.webServer.requestDomain.Trim().ToLower().Replace("..", ".");
                    cpCore.doc.domain = null;
                    if (!string.IsNullOrEmpty(domainTest)) {
                        int posDot = 0;
                        int loopCnt = 10;
                        do {
                            cpCore.doc.domain = domainModel.createByName(cpCore, domainTest);
                            posDot = domainTest.IndexOf('.');
                            if ((posDot >= 0) && (domainTest.Length > 1)) {
                                domainTest = domainTest.Substring(posDot + 1);
                            }
                            loopCnt -= 1;
                        } while ((cpCore.doc.domain == null) && (posDot >= 0) && (loopCnt > 0));
                    }


                    if (cpCore.doc.domain == null) {

                    }
                    //
                    // -- load requested page/template
                    pageContentController.loadPage(cpCore, cpCore.docProperties.getInteger(rnPageId), cpCore.doc.domain);
                    //
                    // -- execute context for included addons
                    CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                        addonType = CPUtilsBaseClass.addonContext.ContextSimple,
                        cssContainerClass = "",
                        cssContainerId = "",
                        hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                            contentName = pageContentModel.contentName,
                            fieldName = "copyfilename",
                            recordId = cpCore.doc.page.id
                        },
                        isIncludeAddon = false,
                        personalizationAuthenticated = cpCore.doc.sessionContext.visit.VisitAuthenticated,
                        personalizationPeopleId = cpCore.doc.sessionContext.user.id
                    };

                    //
                    // -- execute template Dependencies
                    List<Models.Entity.addonModel> templateAddonList = addonModel.createList_templateDependencies(cpCore, cpCore.doc.template.id);
                    foreach (addonModel addon in templateAddonList) {
                        returnHtml += cpCore.addon.executeDependency(addon, executeContext);
                     }
                    //
                    // -- execute page Dependencies
                    List<Models.Entity.addonModel> pageAddonList = addonModel.createList_pageDependencies(cpCore, cpCore.doc.page.id);
                    foreach (addonModel addon in pageAddonList) {
                        returnHtml += cpCore.addon.executeDependency(addon, executeContext);
                    }
                    //
                    cpCore.doc.adminWarning = cpCore.docProperties.getText("main_AdminWarningMsg");
                    cpCore.doc.adminWarningPageID = cpCore.docProperties.getInteger("main_AdminWarningPageID");
                    //
                    // todo move cookie test to htmlDoc controller
                    // -- Add cookie test
                    bool AllowCookieTest = cpCore.siteProperties.allowVisitTracking && (cpCore.doc.sessionContext.visit.PageVisits == 1);
                    if (AllowCookieTest) {
                        cpCore.html.addScriptCode_onLoad("if (document.cookie && document.cookie != null){cj.ajax.qs('f92vo2a8d=" + cpCore.security.encodeToken(cpCore.doc.sessionContext.visit.id, cpCore.doc.profileStartTime) + "')};", "Cookie Test");
                    }
                    //
                    //--------------------------------------------------------------------------
                    //   User form processing
                    //       if a form is created in the editor, process it by emailing and saving to the User Form Response content
                    //--------------------------------------------------------------------------
                    //
                    if (cpCore.docProperties.getInteger("ContensiveUserForm") == 1) {
                        string FromAddress = cpCore.siteProperties.getText("EmailFromAddress", "info@" + cpCore.webServer.requestDomain);
                        emailController.sendForm(cpCore, cpCore.siteProperties.emailAdmin, FromAddress, "Form Submitted on " + cpCore.webServer.requestReferer);
                        int cs = cpCore.db.csInsertRecord("User Form Response");
                        if (cpCore.db.csOk(cs)) {
                            cpCore.db.csSet(cs, "name", "Form " + cpCore.webServer.requestReferrer);
                            string Copy = "";

                            foreach (string key in cpCore.docProperties.getKeyList()) {
                                docPropertiesClass docProperty = cpCore.docProperties.getProperty(key);
                                if (key.ToLower() != "contensiveuserform") {
                                    Copy += docProperty.Name + "=" + docProperty.Value + "\r\n";
                                }
                            }
                            cpCore.db.csSet(cs, "copy", Copy);
                            cpCore.db.csSet(cs, "VisitId", cpCore.doc.sessionContext.visit.id);
                        }
                        cpCore.db.csClose(ref cs);
                    }
                    //
                    //--------------------------------------------------------------------------
                    //   Contensive Form Page Processing
                    //--------------------------------------------------------------------------
                    //
                    if (cpCore.docProperties.getInteger("ContensiveFormPageID") != 0) {
                        processForm(cpCore, cpCore.docProperties.getInteger("ContensiveFormPageID"));
                    }
                    //
                    //--------------------------------------------------------------------------
                    // ----- Automatic Redirect to a full URL
                    //   If the link field of the record is an absolution address
                    //       rc = redirect contentID
                    //       ri = redirect content recordid
                    //--------------------------------------------------------------------------
                    //
                    cpCore.doc.redirectContentID = (cpCore.docProperties.getInteger(rnRedirectContentId));
                    if (cpCore.doc.redirectContentID != 0) {
                        cpCore.doc.redirectRecordID = (cpCore.docProperties.getInteger(rnRedirectRecordId));
                        if (cpCore.doc.redirectRecordID != 0) {
                            string contentName = Models.Complex.cdefModel.getContentNameByID(cpCore, cpCore.doc.redirectContentID);
                            if (!string.IsNullOrEmpty(contentName)) {
                                if (iisController.main_RedirectByRecord_ReturnStatus(cpCore, contentName, cpCore.doc.redirectRecordID)) {
                                    //
                                    //Call AppendLog("main_init(), 3210 - exit for rc/ri redirect ")
                                    //
                                    cpCore.doc.continueProcessing = false; //--- should be disposed by caller --- Call dispose
                                    return string.Empty;
                                } else {
                                    cpCore.doc.adminWarning = "<p>The site attempted to automatically jump to another page, but there was a problem with the page that included the link.<p>";
                                    cpCore.doc.adminWarningPageID = cpCore.doc.redirectRecordID;
                                }
                            }
                        }
                    }
                    //
                    //--------------------------------------------------------------------------
                    // ----- Active Download hook
                    string RecordEID = cpCore.docProperties.getText(RequestNameLibraryFileID);
                    if (!string.IsNullOrEmpty(RecordEID)) {
                        DateTime tokenDate = default(DateTime);
                        cpCore.security.decodeToken(RecordEID, ref downloadId, ref tokenDate);
                        if (downloadId != 0) {
                            //
                            // -- lookup record and set clicks
                            libraryFilesModel file = libraryFilesModel.create(cpCore, downloadId);
                            if (file != null) {
                                file.Clicks += 1;
                                file.save(cpCore);
                                if (file.Filename != "") {
                                    //
                                    // -- create log entry
                                    libraryFileLogModel log = libraryFileLogModel.add(cpCore);
                                    if (log != null) {
                                        log.FileID = file.id;
                                        log.VisitID = cpCore.doc.sessionContext.visit.id;
                                        log.MemberID = cpCore.doc.sessionContext.user.id;
                                    }
                                    //
                                    // -- and go
                                    string link = cpCore.webServer.requestProtocol + cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, file.Filename);
                                    return cpCore.webServer.redirect(link, "Redirecting because the active download request variable is set to a valid Library Files record. Library File Log has been appended.");
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
                    Clip = cpCore.docProperties.getText(RequestNameCut);
                    if (!string.IsNullOrEmpty(Clip)) {
                        //
                        // if a cut, load the clipboard
                        //
                        cpCore.visitProperty.setProperty("Clipboard", Clip);
                        genericController.modifyLinkQuery(cpCore.doc.refreshQueryString, RequestNameCut, "");
                    }
                    ClipParentContentID = cpCore.docProperties.getInteger(RequestNamePasteParentContentID);
                    ClipParentRecordID = cpCore.docProperties.getInteger(RequestNamePasteParentRecordID);
                    ClipParentFieldList = cpCore.docProperties.getText(RequestNamePasteFieldList);
                    if ((ClipParentContentID != 0) & (ClipParentRecordID != 0)) {
                        //
                        // Request for a paste, clear the cliboard
                        //
                        ClipBoard = cpCore.visitProperty.getText("Clipboard", "");
                        cpCore.visitProperty.setProperty("Clipboard", "");
                        genericController.ModifyQueryString(cpCore.doc.refreshQueryString, RequestNamePasteParentContentID, "");
                        genericController.ModifyQueryString(cpCore.doc.refreshQueryString, RequestNamePasteParentRecordID, "");
                        ClipParentContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ClipParentContentID);
                        if (string.IsNullOrEmpty(ClipParentContentName)) {
                            // state not working...
                        } else if (string.IsNullOrEmpty(ClipBoard)) {
                            // state not working...
                        } else {
                            if (!cpCore.doc.sessionContext.isAuthenticatedContentManager(cpCore, ClipParentContentName)) {
                                errorController.addUserError(cpCore, "The paste operation failed because you are not a content manager of the Clip Parent");
                            } else {
                                //
                                // Current identity is a content manager for this content
                                //
                                int Position = genericController.vbInstr(1, ClipBoard, ".");
                                if (Position == 0) {
                                    errorController.addUserError(cpCore, "The paste operation failed because the clipboard data is configured incorrectly.");
                                } else {
                                    ClipBoardArray = ClipBoard.Split('.');
                                    if (ClipBoardArray.GetUpperBound(0) == 0) {
                                        errorController.addUserError(cpCore, "The paste operation failed because the clipboard data is configured incorrectly.");
                                    } else {
                                        ClipChildContentID = genericController.encodeInteger(ClipBoardArray[0]);
                                        ClipChildRecordID = genericController.encodeInteger(ClipBoardArray[1]);
                                        if (!Models.Complex.cdefModel.isWithinContent(cpCore, ClipChildContentID, ClipParentContentID)) {
                                            errorController.addUserError(cpCore, "The paste operation failed because the destination location is not compatible with the clipboard data.");
                                        } else {
                                            //
                                            // the content definition relationship is OK between the child and parent record
                                            //
                                            ClipChildContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ClipChildContentID);
                                            if (!(!string.IsNullOrEmpty(ClipChildContentName))) {
                                                errorController.addUserError(cpCore, "The paste operation failed because the clipboard data content is undefined.");
                                            } else {
                                                if (ClipParentRecordID == 0) {
                                                    errorController.addUserError(cpCore, "The paste operation failed because the clipboard data record is undefined.");
                                                } else if (pageContentController.isChildRecord(cpCore, ClipChildContentName, ClipParentRecordID, ClipChildRecordID)) {
                                                    errorController.addUserError(cpCore, "The paste operation failed because the destination location is a child of the clipboard data record.");
                                                } else {
                                                    //
                                                    // the parent record is not a child of the child record (circular check)
                                                    //
                                                    ClipChildRecordName = "record " + ClipChildRecordID;
                                                    CSClip = cpCore.db.cs_open2(ClipChildContentName, ClipChildRecordID, true, true);
                                                    if (!cpCore.db.csOk(CSClip)) {
                                                        errorController.addUserError(cpCore, "The paste operation failed because the data record referenced by the clipboard could not found.");
                                                    } else {
                                                        //
                                                        // Paste the edit record record
                                                        //
                                                        ClipChildRecordName = cpCore.db.csGetText(CSClip, "name");
                                                        if (string.IsNullOrEmpty(ClipParentFieldList)) {
                                                            //
                                                            // Legacy paste - go right to the parent id
                                                            //
                                                            if (!cpCore.db.cs_isFieldSupported(CSClip, "ParentID")) {
                                                                errorController.addUserError(cpCore, "The paste operation failed because the record you are pasting does not   support the necessary parenting feature.");
                                                            } else {
                                                                cpCore.db.csSet(CSClip, "ParentID", ClipParentRecordID);
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
                                                                    errorController.addUserError(cpCore, "The paste operation failed because the clipboard data Field List is not configured correctly.");
                                                                } else {
                                                                    if (!cpCore.db.cs_isFieldSupported(CSClip, encodeText(NameValues[0]))) {
                                                                        errorController.addUserError(cpCore, "The paste operation failed because the clipboard data Field [" + encodeText(NameValues[0]) + "] is not supported by the location data.");
                                                                    } else {
                                                                        cpCore.db.csSet(CSClip, encodeText(NameValues[0]), encodeText(NameValues[1]));
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
                                                    cpCore.db.csClose(ref CSClip);
                                                    //
                                                    // Set Child Pages Found and clear caches
                                                    //
                                                    CSClip = cpCore.db.csOpenRecord(ClipParentContentName, ClipParentRecordID,false,false, "ChildPagesFound");
                                                    if (cpCore.db.csOk(CSClip)) {
                                                        cpCore.db.csSet(CSClip, "ChildPagesFound", true.ToString());
                                                    }
                                                    cpCore.db.csClose(ref CSClip);
                                                    //
                                                    // Live Editing
                                                    //
                                                    cpCore.cache.invalidateAllInContent(ClipChildContentName);
                                                    cpCore.cache.invalidateAllInContent(ClipParentContentName);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Clip = cpCore.docProperties.getText(RequestNameCutClear);
                    if (!string.IsNullOrEmpty(Clip)) {
                        //
                        // if a cut clear, clear the clipboard
                        //
                        cpCore.visitProperty.setProperty("Clipboard", "");
                        Clip = cpCore.visitProperty.getText("Clipboard", "");
                        genericController.modifyLinkQuery(cpCore.doc.refreshQueryString, RequestNameCutClear, "");
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
                    linkAliasTest1 = cpCore.webServer.requestPathPage;
                    if (linkAliasTest1.Left( 1) == "/") {
                        linkAliasTest1 = linkAliasTest1.Substring(1);
                    }
                    if (linkAliasTest1.Length > 0) {
                        if (linkAliasTest1.Substring(linkAliasTest1.Length - 1, 1) == "/") {
                            linkAliasTest1 = linkAliasTest1.Left( linkAliasTest1.Length - 1);
                        }
                    }

                    linkAliasTest2 = linkAliasTest1 + "/";
                    if ((!IsPageNotFound) && (cpCore.webServer.requestPathPage != "")) {
                        //
                        // build link variations needed later
                        //
                        //
                        Pos = genericController.vbInstr(1, cpCore.webServer.requestPathPage, "://", 1);
                        if (Pos != 0) {
                            LinkNoProtocol = cpCore.webServer.requestPathPage.Substring(Pos + 2);
                            Pos = genericController.vbInstr(Pos + 3, cpCore.webServer.requestPathPage, "/", 2);
                            if (Pos != 0) {
                                linkDomain = cpCore.webServer.requestPathPage.Left( Pos - 1);
                                LinkFullPath = cpCore.webServer.requestPathPage.Substring(Pos - 1);
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
                            + "(SourceLink=" + cpCore.db.encodeSQLText(cpCore.webServer.requestPathPage) + ")"
                            + "or(SourceLink=" + cpCore.db.encodeSQLText(LinkNoProtocol) + ")"
                            + "or(SourceLink=" + cpCore.db.encodeSQLText(LinkFullPath) + ")"
                            + "or(SourceLink=" + cpCore.db.encodeSQLText(LinkFullPathNoSlash) + ")"
                            + ")";
                        isLinkForward = false;
                        Sql = cpCore.db.GetSQLSelect("", "ccLinkForwards", "ID,DestinationLink,Viewings,GroupID", LinkForwardCriteria, "ID","", 1);
                        CSPointer = cpCore.db.csOpenSql(Sql);
                        if (cpCore.db.csOk(CSPointer)) {
                            //
                            // Link Forward found - update count
                            //
                            string tmpLink = null;
                            int GroupID = 0;
                            string groupName = null;
                            //
                            IsInLinkForwardTable = true;
                            Viewings = cpCore.db.csGetInteger(CSPointer, "Viewings") + 1;
                            Sql = "update ccLinkForwards set Viewings=" + Viewings + " where ID=" + cpCore.db.csGetInteger(CSPointer, "ID");
                            cpCore.db.executeQuery(Sql);
                            tmpLink = cpCore.db.csGetText(CSPointer, "DestinationLink");
                            if (!string.IsNullOrEmpty(tmpLink)) {
                                //
                                // Valid Link Forward (without link it is just a record created by the autocreate function
                                //
                                isLinkForward = true;
                                tmpLink = cpCore.db.csGetText(CSPointer, "DestinationLink");
                                GroupID = cpCore.db.csGetInteger(CSPointer, "GroupID");
                                if (GroupID != 0) {
                                    groupName = groupController.group_GetGroupName(cpCore, GroupID);
                                    if (!string.IsNullOrEmpty(groupName)) {
                                        groupController.group_AddGroupMember(cpCore, groupName);
                                    }
                                }
                                if (!string.IsNullOrEmpty(tmpLink)) {
                                    RedirectLink = tmpLink;
                                }
                            }
                        }
                        cpCore.db.csClose(ref CSPointer);
                        //
                        if ((string.IsNullOrEmpty(RedirectLink)) && !isLinkForward) {
                            //
                            // Test for Link Alias
                            //
                            if (!string.IsNullOrEmpty(linkAliasTest1 + linkAliasTest2)) {
                                string sqlLinkAliasCriteria = "(name=" + cpCore.db.encodeSQLText(linkAliasTest1) + ")or(name=" + cpCore.db.encodeSQLText(linkAliasTest2) + ")";
                                List<Models.Entity.linkAliasModel> linkAliasList = linkAliasModel.createList(cpCore, sqlLinkAliasCriteria, "id desc");
                                if (linkAliasList.Count > 0) {
                                    linkAliasModel linkAlias = linkAliasList.First();
                                    string LinkQueryString = rnPageId + "=" + linkAlias.PageID + "&" + linkAlias.QueryStringSuffix;
                                    cpCore.docProperties.setProperty(rnPageId, linkAlias.PageID.ToString(), false);
                                    string[] nameValuePairs = linkAlias.QueryStringSuffix.Split('&');
                                    //Dim nameValuePairs As String() = Split(cpCore.cache_linkAlias(linkAliasCache_queryStringSuffix, Ptr), "&")
                                    foreach (string nameValuePair in nameValuePairs) {
                                        string[] nameValueThing = nameValuePair.Split('=');
                                        if (nameValueThing.GetUpperBound(0) == 0) {
                                            cpCore.docProperties.setProperty(nameValueThing[0], "", false);
                                        } else {
                                            cpCore.docProperties.setProperty(nameValueThing[0], nameValueThing[1], false);
                                        }
                                    }
                                }
                            }
                            //
                            if (!IsLinkAlias) {
                                //
                                // No Link Forward, no Link Alias, no RemoteMethodFromPage, not Robots.txt
                                //
                                if ((cpCore.doc.errorCount == 0) && cpCore.siteProperties.getBoolean("LinkForwardAutoInsert") && (!IsInLinkForwardTable)) {
                                    //
                                    // Add a new Link Forward entry
                                    //
                                    CSPointer = cpCore.db.csInsertRecord("Link Forwards");
                                    if (cpCore.db.csOk(CSPointer)) {
                                        cpCore.db.csSet(CSPointer, "Name", cpCore.webServer.requestPathPage);
                                        cpCore.db.csSet(CSPointer, "sourcelink", cpCore.webServer.requestPathPage);
                                        cpCore.db.csSet(CSPointer, "Viewings", 1);
                                    }
                                    cpCore.db.csClose(ref CSPointer);
                                }
                                //
                                // real 404
                                //
                                //IsPageNotFound = True
                                //PageNotFoundSource = cpCore.webServer.requestPathPage
                                //PageNotFoundReason = "The page could Not be displayed because the URL Is Not a valid page, Link Forward, Link Alias Or RemoteMethod."
                            }
                        }
                    }
                    //
                    // ----- do anonymous access blocking
                    //
                    if (!cpCore.doc.sessionContext.isAuthenticated) {
                        if ((cpCore.webServer.requestPath != "/") & genericController.vbInstr(1, "/" + cpCore.serverConfig.appConfig.adminRoute, cpCore.webServer.requestPath, 1) != 0) {
                            //
                            // admin page is excluded from custom blocking
                            //
                        } else {
                            int AnonymousUserResponseID = genericController.encodeInteger(cpCore.siteProperties.getText("AnonymousUserResponseID", "0"));
                            switch (AnonymousUserResponseID) {
                                case 1:
                                    //
                                    // -- block with login
                                    cpCore.doc.continueProcessing = false;
                                    return cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginPage), new CPUtilsBaseClass.addonExecuteContext() { addonType = CPUtilsBaseClass.addonContext.ContextPage });
                                case 2:
                                    //
                                    // -- block with custom content
                                    cpCore.doc.continueProcessing = false;
                                    cpCore.doc.setMetaContent(0, 0);
                                    cpCore.html.addScriptCode_onLoad("document.body.style.overflow='scroll'", "Anonymous User Block");
                                    return cpCore.html.getHtmlDoc('\r' + cpCore.html.getContentCopy("AnonymousUserResponseCopy", "<p style=\"width:250px;margin:100px auto auto auto;\">The site is currently not available for anonymous access.</p>", cpCore.doc.sessionContext.user.id, true, cpCore.doc.sessionContext.isAuthenticated), TemplateDefaultBodyTag, true, true);
                            }
                        }
                    }
                    //
                    // -- build document
                    htmlDocBody = getHtmlBodyTemplate(cpCore);
                    //
                    // -- check secure certificate required
                    bool SecureLink_Template_Required = cpCore.doc.template.isSecure;
                    bool SecureLink_Page_Required = false;
                    foreach (Models.Entity.pageContentModel page in cpCore.doc.pageToRootList) {
                        if (cpCore.doc.page.IsSecure) {
                            SecureLink_Page_Required = true;
                            break;
                        }
                    }
                    bool SecureLink_Required = SecureLink_Template_Required || SecureLink_Page_Required;
                    bool SecureLink_CurrentURL = (cpCore.webServer.requestUrl.ToLower().Left( 8) == "https://");
                    if (SecureLink_CurrentURL && (!SecureLink_Required)) {
                        //
                        // -- redirect to non-secure
                        RedirectLink = genericController.vbReplace(cpCore.webServer.requestUrl, "https://", "http://");
                        cpCore.doc.redirectReason = "Redirecting because neither the page or the template requires a secure link.";
                        return "";
                    } else if ((!SecureLink_CurrentURL) && SecureLink_Required) {
                        //
                        // -- redirect to secure
                        RedirectLink = genericController.vbReplace(cpCore.webServer.requestUrl, "http://", "https://");
                        if (SecureLink_Page_Required) {
                            cpCore.doc.redirectReason = "Redirecting because this page [" + cpCore.doc.pageToRootList[0].name + "] requires a secure link.";
                        } else {
                            cpCore.doc.redirectReason = "Redirecting because this template [" + cpCore.doc.template.name + "] requires a secure link.";
                        }
                        return "";
                    }
                    //
                    // -- check that this template exists on this domain
                    // -- if endpoint is just domain -> the template is automatically compatible by default (domain determined the landing page)
                    // -- if endpoint is domain + route (link alias), the route determines the page, which may determine the cpCore.doc.template. If this template is not allowed for this domain, redirect to the domain's landingcpCore.doc.page.
                    //
                    Sql = "(domainId=" + cpCore.doc.domain.id + ")";
                    List<Models.Entity.TemplateDomainRuleModel> allowTemplateRuleList = TemplateDomainRuleModel.createList(cpCore, Sql);
                    if (allowTemplateRuleList.Count == 0) {
                        //
                        // -- current template has no domain preference, use current
                    } else {
                        bool allowTemplate = false;
                        foreach (TemplateDomainRuleModel rule in allowTemplateRuleList) {
                            if (rule.templateId == cpCore.doc.template.id) {
                                allowTemplate = true;
                                break;
                            }
                        }
                        if (!allowTemplate) {
                            //
                            // -- must redirect to a domain's landing page
                            RedirectLink = cpCore.webServer.requestProtocol + cpCore.doc.domain.name;
                            cpCore.doc.redirectBecausePageNotFound = false;
                            cpCore.doc.redirectReason = "Redirecting because this domain has template requiements set, and this template is not configured [" + cpCore.doc.template.name + "].";
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
                    if (genericController.vbLCase(cpCore.webServer.requestPathPage) == genericController.vbLCase(requestAppRootPath + cpCore.siteProperties.serverPageDefault)) {
                        //
                        // This is a 404 caused by Contensive returning a 404
                        //   possibly because the pageid was not found or was inactive.
                        //   contensive returned a 404 error, and the IIS custom error handler is hitting now
                        //   what we returned as an error cause is lost
                        //   ( because the Custom404Source page is the default page )
                        //   send it to the 404 page
                        //
                        cpCore.webServer.requestPathPage = cpCore.webServer.requestPathPage;
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
                //    if (cpCore.webServer.requestReferrer != "") {
                //        string main_ServerReferrerURL = null;
                //        string main_ServerReferrerQs = null;
                //        int Position = 0;
                //        main_ServerReferrerURL = cpCore.webServer.requestReferrer;
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
                //            if (cpCore.webServer.requestPage != "") {
                //                //
                //                // If the referer had no page, and there is one here now, it must have been from an IIS redirect, use the current page as the default page
                //                //
                //                main_ServerReferrerURL = main_ServerReferrerURL + cpCore.webServer.requestPage;
                //            } else {
                //                main_ServerReferrerURL = main_ServerReferrerURL + cpCore.siteProperties.serverPageDefault;
                //            }
                //        }
                //        string linkDst = null;
                //        //main_ServerPage = main_ServerPage
                //        if (main_ServerReferrerURL != cpCore.webServer.serverFormActionURL) {
                //            //
                //            // remove any methods from referrer
                //            //
                //            string Copy = "Redirecting because a Contensive Form was detected, source URL [" + main_ServerReferrerURL + "] does not equal the current URL [" + cpCore.webServer.serverFormActionURL + "]. This may be from a Contensive Add-on that now needs to redirect back to the host page.";
                //            linkDst = cpCore.webServer.requestReferer;
                //            if (!string.IsNullOrEmpty(main_ServerReferrerQs)) {
                //                linkDst = main_ServerReferrerURL;
                //                main_ServerReferrerQs = genericController.ModifyQueryString(main_ServerReferrerQs, "method", "");
                //                if (!string.IsNullOrEmpty(main_ServerReferrerQs)) {
                //                    linkDst = linkDst + "?" + main_ServerReferrerQs;
                //                }
                //            }
                //            return cpCore.webServer.redirect(linkDst, Copy);
                //            cpCore.doc.continueProcessing = false; //--- should be disposed by caller --- Call dispose
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
                    logController.appendLogPageNotFound(cpCore, cpCore.webServer.requestUrlSource);
                    cpCore.webServer.setResponseStatus("404 Not Found");
                    cpCore.docProperties.setProperty(rnPageId, getPageNotFoundPageId(cpCore));
                    //Call main_mergeInStream(rnPageId & "=" & main_GetPageNotFoundPageId())
                    if (cpCore.doc.sessionContext.isAuthenticatedAdmin(cpCore)) {
                        cpCore.doc.adminWarning = PageNotFoundReason;
                        cpCore.doc.adminWarningPageID = 0;
                    }
                }
                //
                // add exception list header
                returnHtml = errorController.getDocExceptionHtmlList(cpCore) + returnHtml;
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return returnHtml;
        }
        //
        //
        //
        private static void processForm(coreClass cpcore, int FormPageID) {
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
                CS = cpcore.db.csOpenRecord("Form Pages", FormPageID);
                if (cpcore.db.csOk(CS)) {
                    Formhtml = cpcore.db.csGetText(CS, "Body");
                    FormInstructions = cpcore.db.csGetText(CS, "Instructions");
                }
                cpcore.db.csClose(ref CS);
                if (!string.IsNullOrEmpty(FormInstructions)) {
                    //
                    // Load the instructions
                    //
                    f = loadFormPageInstructions(cpcore, FormInstructions, Formhtml);
                    if (f.AuthenticateOnFormProcess & !cpcore.doc.sessionContext.isAuthenticated & cpcore.doc.sessionContext.isRecognized(cpcore)) {
                        //
                        // If this form will authenticate when done, and their is a current, non-authenticated account -- logout first
                        //
                        cpcore.doc.sessionContext.logout(cpcore);
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
                                FormValue = cpcore.docProperties.getText(tempVar.PeopleField);
                                if ((!string.IsNullOrEmpty(FormValue)) & genericController.encodeBoolean(Models.Complex.cdefModel.GetContentFieldProperty(cpcore, "people", tempVar.PeopleField, "uniquename"))) {
                                    SQL = "select count(*) from ccMembers where " + tempVar.PeopleField + "=" + cpcore.db.encodeSQLText(FormValue);
                                    CS = cpcore.db.csOpenSql(SQL);
                                    if (cpcore.db.csOk(CS)) {
                                        Success = cpcore.db.csGetInteger(CS, "cnt") == 0;
                                    }
                                    cpcore.db.csClose(ref CS);
                                    if (!Success) {
                                        errorController.addUserError(cpcore, "The field [" + tempVar.Caption + "] must be unique, and the value [" + genericController.encodeHTML(FormValue) + "] has already been used.");
                                    }
                                }
                                if ((tempVar.REquired | genericController.encodeBoolean(Models.Complex.cdefModel.GetContentFieldProperty(cpcore, "people", tempVar.PeopleField, "required"))) && string.IsNullOrEmpty(FormValue)) {
                                    Success = false;
                                    errorController.addUserError(cpcore, "The field [" + genericController.encodeHTML(tempVar.Caption) + "] is required.");
                                } else {
                                    if (!cpcore.db.csOk(CSPeople)) {
                                        CSPeople = cpcore.db.csOpenRecord("people", cpcore.doc.sessionContext.user.id);
                                    }
                                    if (cpcore.db.csOk(CSPeople)) {
                                        switch (genericController.vbUCase(tempVar.PeopleField)) {
                                            case "NAME":
                                                PeopleName = FormValue;
                                                cpcore.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
                                                break;
                                            case "FIRSTNAME":
                                                PeopleFirstName = FormValue;
                                                cpcore.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
                                                break;
                                            case "LASTNAME":
                                                PeopleLastName = FormValue;
                                                cpcore.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
                                                break;
                                            case "EMAIL":
                                                PeopleEmail = FormValue;
                                                cpcore.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
                                                break;
                                            case "USERNAME":
                                                PeopleUsername = FormValue;
                                                cpcore.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
                                                break;
                                            case "PASSWORD":
                                                PeoplePassword = FormValue;
                                                cpcore.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
                                                break;
                                            default:
                                                cpcore.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
                                                break;
                                        }
                                    }
                                }
                                break;
                            case 2:
                                //
                                // Group main_MemberShip
                                //
                                IsInGroup = cpcore.docProperties.getBoolean("Group" + tempVar.GroupName);
                                WasInGroup = cpcore.doc.sessionContext.IsMemberOfGroup2(cpcore, tempVar.GroupName);
                                if (WasInGroup && !IsInGroup) {
                                    groupController.group_DeleteGroupMember(cpcore, tempVar.GroupName);
                                } else if (IsInGroup && !WasInGroup) {
                                    groupController.group_AddGroupMember(cpcore, tempVar.GroupName);
                                }
                                break;
                        }
                    }
                    //
                    // Create People Name
                    //
                    if (string.IsNullOrEmpty(PeopleName) && !string.IsNullOrEmpty(PeopleFirstName) & !string.IsNullOrEmpty(PeopleLastName)) {
                        if (cpcore.db.csOk(CSPeople)) {
                            cpcore.db.csSet(CSPeople, "name", PeopleFirstName + " " + PeopleLastName);
                        }
                    }
                    cpcore.db.csClose(ref CSPeople);
                    //
                    // AuthenticationOnFormProcess requires Username/Password and must be valid
                    //
                    if (Success) {
                        //
                        // Authenticate
                        //
                        if (f.AuthenticateOnFormProcess) {
                            cpcore.doc.sessionContext.authenticateById(cpcore, cpcore.doc.sessionContext.user.id, cpcore.doc.sessionContext);
                        }
                        //
                        // Join Group requested by page that created form
                        //
                        DateTime tokenDate = default(DateTime);
                        cpcore.security.decodeToken(cpcore.docProperties.getText("SuccessID"), ref GroupIDToJoinOnSuccess, ref tokenDate);
                        //GroupIDToJoinOnSuccess = main_DecodeKeyNumber(main_GetStreamText2("SuccessID"))
                        if (GroupIDToJoinOnSuccess != 0) {
                            groupController.group_AddGroupMember(cpcore, groupController.group_GetGroupName(cpcore, GroupIDToJoinOnSuccess));
                        }
                        //
                        // Join Groups requested by pageform
                        //
                        if (f.AddGroupNameList != "") {
                            Groups = (encodeText(f.AddGroupNameList).Trim(' ')).Split(',');
                            for (Ptr = 0; Ptr <= Groups.GetUpperBound(0); Ptr++) {
                                GroupName = Groups[Ptr].Trim(' ');
                                if (!string.IsNullOrEmpty(GroupName)) {
                                    groupController.group_AddGroupMember(cpcore, GroupName);
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
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
        internal static main_FormPagetype loadFormPageInstructions(coreClass cpcore, string FormInstructions, string Formhtml) {
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
                cpcore.handleException(ex);
            }
            return result;
        }
        //
        //
        //
        //
        internal static string getFormPage(coreClass cpcore, string FormPageName, int GroupIDToJoinOnSuccess) {
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
                IsRetry = (cpcore.docProperties.getInteger("ContensiveFormPageID") != 0);
                //
                CS = cpcore.db.csOpen("Form Pages", "name=" + cpcore.db.encodeSQLText(FormPageName));
                if (cpcore.db.csOk(CS)) {
                    FormPageID = cpcore.db.csGetInteger(CS, "ID");
                    Formhtml = cpcore.db.csGetText(CS, "Body");
                    FormInstructions = cpcore.db.csGetText(CS, "Instructions");
                }
                cpcore.db.csClose(ref CS);
                f = loadFormPageInstructions(cpcore, FormInstructions, Formhtml);
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
                            if (IsRetry && cpcore.docProperties.getText(tempVar.PeopleField) == "") {
                                CaptionSpan = "<span class=\"ccError\">";
                            } else {
                                CaptionSpan = "<span>";
                            }
                            if (!cpcore.db.csOk(CSPeople)) {
                                CSPeople = cpcore.db.csOpenRecord("people", cpcore.doc.sessionContext.user.id);
                            }
                            Caption = tempVar.Caption;
                            if (tempVar.REquired | genericController.encodeBoolean(Models.Complex.cdefModel.GetContentFieldProperty(cpcore, "People", tempVar.PeopleField, "Required"))) {
                                Caption = "*" + Caption;
                            }
                            if (cpcore.db.csOk(CSPeople)) {
                                Body = f.RepeatCell;
                                Body = genericController.vbReplace(Body, "{{CAPTION}}", CaptionSpan + Caption + "</span>", 1, 99, 1);
                                Body = genericController.vbReplace(Body, "{{FIELD}}", cpcore.html.inputCs(CSPeople, "People", tempVar.PeopleField), 1, 99, 1);
                                RepeatBody = RepeatBody + Body;
                                HasRequiredFields = HasRequiredFields || tempVar.REquired;
                            }
                            break;
                        case 2:
                            //
                            // Group main_MemberShip
                            //
                            GroupValue = cpcore.doc.sessionContext.IsMemberOfGroup2(cpcore, tempVar.GroupName);
                            Body = f.RepeatCell;
                            Body = genericController.vbReplace(Body, "{{CAPTION}}", cpcore.html.inputCheckbox("Group" + tempVar.GroupName, GroupValue), 1, 99, 1);
                            Body = genericController.vbReplace(Body, "{{FIELD}}", tempVar.Caption);
                            RepeatBody = RepeatBody + Body;
                            GroupRowPtr = GroupRowPtr + 1;
                            HasRequiredFields = HasRequiredFields || tempVar.REquired;
                            break;
                    }
                }
                cpcore.db.csClose(ref CSPeople);
                if (HasRequiredFields) {
                    Body = f.RepeatCell;
                    Body = genericController.vbReplace(Body, "{{CAPTION}}", "&nbsp;", 1, 99, 1);
                    Body = genericController.vbReplace(Body, "{{FIELD}}", "*&nbsp;Required Fields");
                    RepeatBody = RepeatBody + Body;
                }
                //
                tempgetFormPage = ""
                + errorController.getUserError(cpcore) + cpcore.html.formStartMultipart() + cpcore.html.inputHidden("ContensiveFormPageID", FormPageID) + cpcore.html.inputHidden("SuccessID", cpcore.security.encodeToken(GroupIDToJoinOnSuccess, cpcore.doc.profileStartTime)) + f.PreRepeat + RepeatBody + f.PostRepeat + cpcore.html.formEnd();
                //
                return tempgetFormPage;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                cpcore.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError13("main_GetFormPage")
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
        public static string getContentBox(coreClass cpCore, string OrderByClause, bool AllowChildPageList, bool AllowReturnLink, bool ArchivePages, int ignoreme, bool UseContentWatchLink, bool allowPageWithoutSectionDisplay) {
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
                string layoutError = "";
                //
                cpCore.html.addHeadTag("<meta name=\"contentId\" content=\"" + cpCore.doc.page.id + "\" >", "page content");
                //
                returnHtml = getContentBox_content(cpCore, OrderByClause, AllowChildPageList, AllowReturnLink, ArchivePages, ignoreme, UseContentWatchLink, allowPageWithoutSectionDisplay);
                //
                // ----- If Link field populated, do redirect
                if (cpCore.doc.page.PageLink != "") {
                    cpCore.doc.page.Clicks += 1;
                    cpCore.doc.page.save(cpCore);
                    cpCore.doc.redirectLink = cpCore.doc.page.PageLink;
                    cpCore.doc.redirectReason = "Redirect required because this page (PageRecordID=" + cpCore.doc.page.id + ") has a Link Override [" + cpCore.doc.page.PageLink + "].";
                    return "";
                }
                //
                // -- build list of blocked pages
                string BlockedRecordIDList = "";
                if ((!string.IsNullOrEmpty(returnHtml)) && (cpCore.doc.redirectLink == "")) {
                    foreach (pageContentModel testPage in cpCore.doc.pageToRootList) {
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
                    if (cpCore.doc.sessionContext.isAuthenticatedAdmin(cpCore)) {
                        //
                        // Administrators are never blocked
                        //
                    } else if (!cpCore.doc.sessionContext.isAuthenticated) {
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
                            + " WHERE (((ccMemberRules.MemberID)=" + cpCore.db.encodeSQLNumber(cpCore.doc.sessionContext.user.id) + ")"
                            + " AND ((ccPageContentBlockRules.RecordID) In (" + BlockedRecordIDList + "))"
                            + " AND ((ccPageContentBlockRules.Active)<>0)"
                            + " AND ((ccgroups.Active)<>0)"
                            + " AND ((ccMemberRules.Active)<>0)"
                            + " AND ((ccMemberRules.DateExpires) Is Null Or (ccMemberRules.DateExpires)>" + cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) + "));";
                        CS = cpCore.db.csOpenSql(SQL);
                        BlockedRecordIDList = "," + BlockedRecordIDList;
                        while (cpCore.db.csOk(CS)) {
                            BlockedRecordIDList = genericController.vbReplace(BlockedRecordIDList, "," + cpCore.db.csGetText(CS, "RecordID"), "");
                            cpCore.db.csGoNext(CS);
                        }
                        cpCore.db.csClose(ref CS);
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
                                + " AND ((ManagementMemberRules.DateExpires) Is Null Or (ManagementMemberRules.DateExpires)>" + cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) + ")"
                                + " AND ((ManagementMemberRules.MemberID)=" + cpCore.doc.sessionContext.user.id + " ));";
                            CS = cpCore.db.csOpenSql(SQL);
                            while (cpCore.db.csOk(CS)) {
                                BlockedRecordIDList = genericController.vbReplace(BlockedRecordIDList, "," + cpCore.db.csGetText(CS, "RecordID"), "");
                                cpCore.db.csGoNext(CS);
                            }
                            cpCore.db.csClose(ref CS);
                        }
                        if (!string.IsNullOrEmpty(BlockedRecordIDList)) {
                            ContentBlocked = true;
                        }
                        cpCore.db.csClose(ref CS);
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
                        CS = cpCore.db.csOpenRecord("Page Content", BlockedPageRecordID,false, false, "CustomBlockMessage,BlockSourceID,RegistrationGroupID,ContentPadding");
                        if (cpCore.db.csOk(CS)) {
                            BlockSourceID = cpCore.db.csGetInteger(CS, "BlockSourceID");
                            ContentPadding = cpCore.db.csGetInteger(CS, "ContentPadding");
                            CustomBlockMessageFilename = cpCore.db.csGetText(CS, "CustomBlockMessage");
                            RegistrationGroupID = cpCore.db.csGetInteger(CS, "RegistrationGroupID");
                        }
                        cpCore.db.csClose(ref CS);
                    }
                    //
                    // Block Appropriately
                    //
                    switch (BlockSourceID) {
                        case main_BlockSourceCustomMessage: {
                                //
                                // ----- Custom Message
                                //
                                returnHtml = cpCore.cdnFiles.readFile(CustomBlockMessageFilename);
                                break;
                            }
                        case main_BlockSourceLogin: {
                                //
                                // ----- Login page
                                //
                                string BlockForm = "";
                                if (!cpCore.doc.sessionContext.isAuthenticated) {
                                    if (!cpCore.doc.sessionContext.isRecognized(cpCore)) {
                                        //
                                        // -- not recognized
                                        BlockForm = ""
                                            + "<p>This content has limited access. If you have an account, please login using this form.</p>"
                                            + cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext { addonType = CPUtilsBaseClass.addonContext.ContextPage }) + "";
                                    } else {
                                        //
                                        // -- recognized, not authenticated
                                        BlockForm = ""
                                            + "<p>This content has limited access. You were recognized as \"<b>" + cpCore.doc.sessionContext.user.name + "</b>\", but you need to login to continue. To login to this account or another, please use this form.</p>"
                                            + cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext { addonType = CPUtilsBaseClass.addonContext.ContextPage }) + "";
                                    }
                                } else {
                                    //
                                    // -- authenticated
                                    BlockForm = ""
                                        + "<p>You are currently logged in as \"<b>" + cpCore.doc.sessionContext.user.name + "</b>\". If this is not you, please <a href=\"?" + cpCore.doc.refreshQueryString + "&method=logout\" rel=\"nofollow\">Click Here</a>.</p>"
                                        + "<p>This account does not have access to this content. If you want to login with a different account, please use this form.</p>"
                                        + cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext { addonType = CPUtilsBaseClass.addonContext.ContextPage }) + "";
                                }
                                returnHtml = ""
                                    + "<div style=\"margin: 100px, auto, auto, auto;text-align:left;\">"
                                    + errorController.getUserError(cpCore) + BlockForm + "</div>";
                                break;
                            }
                        case main_BlockSourceRegistration: {
                                //
                                // ----- Registration
                                //
                                string BlockForm = "";
                                if (cpCore.docProperties.getInteger("subform") == main_BlockSourceLogin) {
                                    //
                                    // login subform form
                                    BlockForm = ""
                                        + "<p>This content has limited access. If you have an account, please login using this form.</p>"
                                        + "<p>If you do not have an account, <a href=?" + cpCore.doc.refreshQueryString + "&subform=0>click here to register</a>.</p>"
                                        + cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext { addonType = CPUtilsBaseClass.addonContext.ContextPage }) + "";
                                } else {
                                    //
                                    // Register Form
                                    //
                                    if (!cpCore.doc.sessionContext.isAuthenticated & cpCore.doc.sessionContext.isRecognized(cpCore)) {
                                        //
                                        // -- Can not take the chance, if you go to a registration page, and you are recognized but not auth -- logout first
                                        cpCore.doc.sessionContext.logout(cpCore);
                                    }
                                    if (!cpCore.doc.sessionContext.isAuthenticated) {
                                        //
                                        // -- Not Authenticated
                                        cpCore.doc.verifyRegistrationFormPage(cpCore);
                                        BlockForm = ""
                                            + "<p>This content has limited access. If you have an account, <a href=?" + cpCore.doc.refreshQueryString + "&subform=" + main_BlockSourceLogin + ">Click Here to login</a>.</p>"
                                            + "<p>To view this content, please complete this form.</p>"
                                            + getFormPage(cpCore, "Registration Form", RegistrationGroupID) + "";
                                    } else {
                                        //
                                        // -- Authenticated
                                        cpCore.doc.verifyRegistrationFormPage(cpCore);
                                        BlockCopy = ""
                                            + "<p>You are currently logged in as \"<b>" + cpCore.doc.sessionContext.user.name + "</b>\". If this is not you, please <a href=\"?" + cpCore.doc.refreshQueryString + "&method=logout\" rel=\"nofollow\">Click Here</a>.</p>"
                                            + "<p>This account does not have access to this content. To view this content, please complete this form.</p>"
                                            + getFormPage(cpCore, "Registration Form", RegistrationGroupID) + "";
                                    }
                                }
                                returnHtml = ""
                                    + "<div style=\"margin: 100px, auto, auto, auto;text-align:left;\">"
                                    + errorController.getUserError(cpCore) + BlockForm + "</div>";
                                break;
                            }
                        default: {
                                //
                                // ----- Content as blocked - convert from site property to content page
                                //
                                returnHtml = getDefaultBlockMessage(cpCore, UseContentWatchLink);
                                break;
                            }
                    }
                    //
                    // If the output is blank, put default message in
                    //
                    if (string.IsNullOrEmpty(returnHtml)) {
                        returnHtml = getDefaultBlockMessage(cpCore, UseContentWatchLink);
                    }
                    //
                    // Encode the copy
                    //
                    returnHtml = contentCmdController.executeContentCommands(cpCore, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpCore.doc.sessionContext.user.id, cpCore.doc.sessionContext.isAuthenticated, ref layoutError);
                    returnHtml = activeContentController.convertActiveContentToHtmlForWebRender(cpCore, returnHtml, pageContentModel.contentName, PageRecordID, cpCore.doc.page.ContactMemberID, "http://" + cpCore.webServer.requestDomain, cpCore.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage);
                    if (cpCore.doc.refreshQueryString != "") {
                        returnHtml = genericController.vbReplace(returnHtml, "?method=login", "?method=Login&" + cpCore.doc.refreshQueryString, 1, 99, 1);
                    }
                    //
                    // Add in content padding required for integration with the template
                    returnHtml = getContentBoxWrapper(cpCore, returnHtml, ContentPadding);
                }
                //
                // ----- Encoding, Tracking and Triggers
                if (!ContentBlocked) {
                    if (cpCore.visitProperty.getBoolean("AllowQuickEditor")) {
                        //
                        // Quick Editor, no encoding or tracking
                        //
                    } else {
                        pageViewings = cpCore.doc.page.Viewings;
                        if (cpCore.doc.sessionContext.isEditing(pageContentModel.contentName) | cpCore.visitProperty.getBoolean("AllowWorkflowRendering")) {
                            //
                            // Link authoring, workflow rendering -> do encoding, but no tracking
                            //
                            returnHtml = contentCmdController.executeContentCommands(cpCore, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpCore.doc.sessionContext.user.id, cpCore.doc.sessionContext.isAuthenticated, ref layoutError);
                            returnHtml = activeContentController.convertActiveContentToHtmlForWebRender(cpCore, returnHtml, pageContentModel.contentName, PageRecordID, cpCore.doc.page.ContactMemberID, "http://" + cpCore.webServer.requestDomain, cpCore.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage);
                        } else {
                            //
                            // Live content
                            returnHtml = contentCmdController.executeContentCommands(cpCore, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpCore.doc.sessionContext.user.id, cpCore.doc.sessionContext.isAuthenticated, ref layoutError);
                            returnHtml = activeContentController.convertActiveContentToHtmlForWebRender(cpCore, returnHtml, pageContentModel.contentName, PageRecordID, cpCore.doc.page.ContactMemberID, "http://" + cpCore.webServer.requestDomain, cpCore.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage);
                            cpCore.db.executeQuery("update ccpagecontent set viewings=" + (pageViewings + 1) + " where id=" + cpCore.doc.page.id);
                        }
                        //
                        // Page Hit Notification
                        //
                        if ((!cpCore.doc.sessionContext.visit.ExcludeFromAnalytics) & (cpCore.doc.page.ContactMemberID != 0) && (cpCore.webServer.requestBrowser.IndexOf("kmahttp", System.StringComparison.OrdinalIgnoreCase)  == -1)) {
                            personModel person = personModel.create(cpCore, cpCore.doc.page.ContactMemberID);
                            if ( person != null ) {
                                if (cpCore.doc.page.AllowHitNotification) {
                                    PageName = cpCore.doc.page.name;
                                    if (string.IsNullOrEmpty(PageName)) {
                                        PageName = cpCore.doc.page.MenuHeadline;
                                        if (string.IsNullOrEmpty(PageName)) {
                                            PageName = cpCore.doc.page.Headline;
                                            if (string.IsNullOrEmpty(PageName)) {
                                                PageName = "[no name]";
                                            }
                                        }
                                    }
                                    string Body = "";
                                    Body = Body + "<p><b>Page Hit Notification.</b></p>";
                                    Body = Body + "<p>This email was sent to you by the Contensive Server as a notification of the following content viewing details.</p>";
                                    Body = Body + genericController.StartTable(4, 1, 1);
                                    Body = Body + "<tr><td align=\"right\" width=\"150\" Class=\"ccPanelHeader\">Description<br><img alt=\"image\" src=\"http://" + cpCore.webServer.requestDomain + "/ccLib/images/spacer.gif\" width=\"150\" height=\"1\"></td><td align=\"left\" width=\"100%\" Class=\"ccPanelHeader\">Value</td></tr>";
                                    Body = Body + getTableRow("Domain", cpCore.webServer.requestDomain, true);
                                    Body = Body + getTableRow("Link", cpCore.webServer.requestUrl, false);
                                    Body = Body + getTableRow("Page Name", PageName, true);
                                    Body = Body + getTableRow("Member Name", cpCore.doc.sessionContext.user.name, false);
                                    Body = Body + getTableRow("Member #", encodeText(cpCore.doc.sessionContext.user.id), true);
                                    Body = Body + getTableRow("Visit Start Time", encodeText(cpCore.doc.sessionContext.visit.StartTime), false);
                                    Body = Body + getTableRow("Visit #", encodeText(cpCore.doc.sessionContext.visit.id), true);
                                    Body = Body + getTableRow("Visit IP", cpCore.webServer.requestRemoteIP, false);
                                    Body = Body + getTableRow("Browser ", cpCore.webServer.requestBrowser, true);
                                    Body = Body + getTableRow("Visitor #", encodeText(cpCore.doc.sessionContext.visitor.id), false);
                                    Body = Body + getTableRow("Visit Authenticated", encodeText(cpCore.doc.sessionContext.visit.VisitAuthenticated), true);
                                    Body = Body + getTableRow("Visit Referrer", cpCore.doc.sessionContext.visit.HTTP_REFERER, false);
                                    Body = Body + kmaEndTable;
                                    string queryStringForLinkAppend = "";
                                    string emailStatus = "";
                                    emailController.sendPerson(cpCore, person, cpCore.siteProperties.getText("EmailFromAddress", "info@" + cpCore.webServer.requestDomain), "Page Hit Notification", Body, false, true, 0, "", false, ref emailStatus, queryStringForLinkAppend);
                                }
                            }
                        }
                        //
                        // -- Process Trigger Conditions
                        ConditionID = cpCore.doc.page.TriggerConditionID;
                        ConditionGroupID = cpCore.doc.page.TriggerConditionGroupID;
                        main_AddGroupID = cpCore.doc.page.TriggerAddGroupID;
                        RemoveGroupID = cpCore.doc.page.TriggerRemoveGroupID;
                        SystemEMailID = cpCore.doc.page.TriggerSendSystemEmailID;
                        switch (ConditionID) {
                            case 1:
                                //
                                // Always
                                //
                                if (SystemEMailID != 0) {
                                    emailController.sendSystem(cpCore, cpCore.db.getRecordName("System Email", SystemEMailID), "", cpCore.doc.sessionContext.user.id);
                                }
                                if (main_AddGroupID != 0) {
                                    groupController.group_AddGroupMember(cpCore, groupController.group_GetGroupName(cpCore, main_AddGroupID));
                                }
                                if (RemoveGroupID != 0) {
                                    groupController.group_DeleteGroupMember(cpCore, groupController.group_GetGroupName(cpCore, RemoveGroupID));
                                }
                                break;
                            case 2:
                                //
                                // If in Condition Group
                                //
                                if (ConditionGroupID != 0) {
                                    if (cpCore.doc.sessionContext.IsMemberOfGroup2(cpCore, groupController.group_GetGroupName(cpCore, ConditionGroupID))) {
                                        if (SystemEMailID != 0) {
                                            emailController.sendSystem(cpCore, cpCore.db.getRecordName("System Email", SystemEMailID), "", cpCore.doc.sessionContext.user.id);
                                        }
                                        if (main_AddGroupID != 0) {
                                            groupController.group_AddGroupMember(cpCore, groupController.group_GetGroupName(cpCore, main_AddGroupID));
                                        }
                                        if (RemoveGroupID != 0) {
                                            groupController.group_DeleteGroupMember(cpCore, groupController.group_GetGroupName(cpCore, RemoveGroupID));
                                        }
                                    }
                                }
                                break;
                            case 3:
                                //
                                // If not in Condition Group
                                //
                                if (ConditionGroupID != 0) {
                                    if (!cpCore.doc.sessionContext.IsMemberOfGroup2(cpCore, groupController.group_GetGroupName(cpCore, ConditionGroupID))) {
                                        if (main_AddGroupID != 0) {
                                            groupController.group_AddGroupMember(cpCore, groupController.group_GetGroupName(cpCore, main_AddGroupID));
                                        }
                                        if (RemoveGroupID != 0) {
                                            groupController.group_DeleteGroupMember(cpCore, groupController.group_GetGroupName(cpCore, RemoveGroupID));
                                        }
                                        if (SystemEMailID != 0) {
                                            emailController.sendSystem(cpCore, cpCore.db.getRecordName("System Email", SystemEMailID), "", cpCore.doc.sessionContext.user.id);
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
                    returnHtml = getContentBoxWrapper(cpCore, returnHtml, cpCore.doc.page.ContentPadding);
                    //
                    //---------------------------------------------------------------------------------
                    // ----- Set Headers
                    //---------------------------------------------------------------------------------
                    //
                    if (DateModified != DateTime.MinValue) {
                        cpCore.webServer.addResponseHeader("LAST-MODIFIED", genericController.GetGMTFromDate(DateModified));
                    }
                    //
                    //---------------------------------------------------------------------------------
                    // ----- Store page javascript
                    //---------------------------------------------------------------------------------
                    // todo -- assets should all come from addons !!!
                    //
                    cpCore.html.addScriptCode_onLoad(cpCore.doc.page.JSOnLoad, "page content");
                    cpCore.html.addScriptCode(cpCore.doc.page.JSHead, "page content");
                    if (cpCore.doc.page.JSFilename != "") {
                        cpCore.html.addScriptLinkSrc(genericController.getCdnFileLink(cpCore, cpCore.doc.page.JSFilename), "page content");
                    }
                    cpCore.html.addScriptCode(cpCore.doc.page.JSEndBody, "page content");
                    //
                    //---------------------------------------------------------------------------------
                    // Set the Meta Content flag
                    //---------------------------------------------------------------------------------
                    //
                    cpCore.html.addTitle(genericController.encodeHTML(cpCore.doc.page.pageTitle), "page content");
                    cpCore.html.addMetaDescription(genericController.encodeHTML(cpCore.doc.page.metaDescription), "page content");
                    cpCore.html.addHeadTag(cpCore.doc.page.OtherHeadTags, "page content");
                    cpCore.html.addMetaKeywordList(cpCore.doc.page.MetaKeywordList, "page content");
                    //
                    Dictionary<string, string> instanceArguments = new Dictionary<string, string>();
                    instanceArguments.Add("CSPage", "-1");
                    CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                        instanceGuid = "-1",
                        instanceArguments = instanceArguments
                    };
                    //
                    // -- OnPageStartEvent
                    cpCore.doc.bodyContent = returnHtml;
                    executeContext.addonType = CPUtilsBaseClass.addonContext.ContextOnPageStart;
                    List<addonModel> addonList = addonModel.createList_OnPageStartEvent(cpCore, new List<string>());
                    foreach (Models.Entity.addonModel addon in addonList) {
                        cpCore.doc.bodyContent = cpCore.addon.execute(addon, executeContext) + cpCore.doc.bodyContent;
                        //AddonContent = cpCore.addon.execute_legacy5(addon.id, addon.name, "CSPage=-1", CPUtilsBaseClass.addonContext.ContextOnPageStart, "", 0, "", -1)
                    }
                    returnHtml = cpCore.doc.bodyContent;
                    //
                    // -- OnPageEndEvent / filter
                    cpCore.doc.bodyContent = returnHtml;
                    executeContext.addonType = CPUtilsBaseClass.addonContext.ContextOnPageEnd;
                    foreach (addonModel addon in cpCore.addonCache.getOnPageEndAddonList()) {
                        cpCore.doc.bodyContent += cpCore.addon.execute(addon, executeContext);
                        //cpCore.doc.bodyContent &= cpCore.addon.execute_legacy5(addon.id, addon.name, "CSPage=-1", CPUtilsBaseClass.addonContext.ContextOnPageStart, "", 0, "", -1)
                    }
                    returnHtml = cpCore.doc.bodyContent;
                    //
                }
                //
                // -- title
                cpCore.html.addTitle(cpCore.doc.page.name);
                //
                // -- add contentid and sectionid
                cpCore.html.addHeadTag("<meta name=\"contentId\" content=\"" + cpCore.doc.page.id + "\" >", "page content");
                //
                // Display Admin Warnings with Edits for record errors
                //
                if (cpCore.doc.adminWarning != "") {
                    //
                    if (cpCore.doc.adminWarningPageID != 0) {
                        cpCore.doc.adminWarning = cpCore.doc.adminWarning + "</p>" + cpCore.html.getRecordEditLink2("Page Content", cpCore.doc.adminWarningPageID, true, "Page " + cpCore.doc.adminWarningPageID, cpCore.doc.sessionContext.isAuthenticatedAdmin(cpCore)) + "&nbsp;Edit the page<p>";
                        cpCore.doc.adminWarningPageID = 0;
                    }
                    returnHtml = ""
                    + cpCore.html.getAdminHintWrapper(cpCore.doc.adminWarning) + returnHtml + "";
                    cpCore.doc.adminWarning = "";
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
        internal static string getContentBox_content(coreClass cpcore, string OrderByClause, bool AllowChildPageList, bool AllowReturnLink, bool ArchivePages, int ignoreMe, bool UseContentWatchLink, bool allowPageWithoutSectionDisplay) {
            string result = "";
            try {
                bool isEditing = false;
                string LiveBody = null;
                //
                if (cpcore.doc.continueProcessing) {
                    if (cpcore.doc.redirectLink == "") {
                        isEditing = cpcore.doc.sessionContext.isEditing(pageContentModel.contentName);
                        //
                        // ----- Render the Body
                        LiveBody = getContentBox_content_Body(cpcore, OrderByClause, AllowChildPageList, false, cpcore.doc.pageToRootList.Last().id, AllowReturnLink, pageContentModel.contentName, ArchivePages);
                        bool isRootPage = (cpcore.doc.pageToRootList.Count == 1);
                        if (cpcore.doc.sessionContext.isAdvancedEditing(cpcore, "")) {
                            result = result + cpcore.html.getRecordEditLink(pageContentModel.contentName, cpcore.doc.page.id, (!isRootPage)) + LiveBody;
                        } else if (isEditing) {
                            result = result + cpcore.html.getEditWrapper("", cpcore.html.getRecordEditLink(pageContentModel.contentName, cpcore.doc.page.id, (!isRootPage)) + LiveBody);
                        } else {
                            result = result + LiveBody;
                        }
                    }
                }
                //
            } catch (Exception ex) {
                cpcore.handleException(ex);
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

        internal static string getContentBox_content_Body(coreClass cpcore, string OrderByClause, bool AllowChildList, bool Authoring, int rootPageId, bool AllowReturnLink, string RootPageContentName, bool ArchivePage) {
            string result = "";
            try {
                bool allowChildListComposite = AllowChildList && cpcore.doc.page.AllowChildListDisplay;
                bool allowReturnLinkComposite = AllowReturnLink && cpcore.doc.page.AllowReturnLinkDisplay;
                string bodyCopy = cpcore.doc.page.Copyfilename.content;
                string breadCrumb = "";
                string BreadCrumbDelimiter = null;
                string BreadCrumbPrefix = null;
                bool isRootPage = cpcore.doc.pageToRootList.Count.Equals(1);
                //
                if (allowReturnLinkComposite && (!isRootPage)) {
                    //
                    // ----- Print Heading if not at root Page
                    //
                    BreadCrumbPrefix = cpcore.siteProperties.getText("BreadCrumbPrefix", "Return to");
                    BreadCrumbDelimiter = cpcore.siteProperties.getText("BreadCrumbDelimiter", " &gt; ");
                    breadCrumb = cpcore.doc.getReturnBreadcrumb(RootPageContentName, cpcore.doc.page.ParentID, rootPageId, "", ArchivePage, BreadCrumbDelimiter);
                    if (!string.IsNullOrEmpty(breadCrumb)) {
                        breadCrumb = "\r<p class=\"ccPageListNavigation\">" + BreadCrumbPrefix + " " + breadCrumb + "</p>";
                    }
                }
                result = result + breadCrumb;
                //
                if (true) {
                    string IconRow = "";
                    if ((!cpcore.doc.sessionContext.visit.Bot) & (cpcore.doc.page.AllowPrinterVersion | cpcore.doc.page.AllowEmailPage)) {
                        //
                        // not a bot, and either print or email allowed
                        //
                        if (cpcore.doc.page.AllowPrinterVersion) {
                            string QueryString = cpcore.doc.refreshQueryString;
                            QueryString = genericController.ModifyQueryString(QueryString, rnPageId, genericController.encodeText(cpcore.doc.page.id), true);
                            QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, HardCodedPagePrinterVersion, true);
                            string Caption = cpcore.siteProperties.getText("PagePrinterVersionCaption", "Printer Version");
                            Caption = genericController.vbReplace(Caption, " ", "&nbsp;");
                            IconRow = IconRow + "\r&nbsp;&nbsp;<a href=\"" + genericController.encodeHTML(cpcore.webServer.requestPage + "?" + QueryString) + "\" target=\"_blank\"><img alt=\"image\" src=\"/ccLib/images/IconSmallPrinter.gif\" width=\"13\" height=\"13\" border=\"0\" align=\"absmiddle\"></a>&nbsp<a href=\"" + genericController.encodeHTML(cpcore.webServer.requestPage + "?" + QueryString) + "\" target=\"_blank\" style=\"text-decoration:none! important;font-family:sanserif,verdana,helvetica;font-size:11px;\">" + Caption + "</a>";
                        }
                        if (cpcore.doc.page.AllowEmailPage) {
                            string QueryString = cpcore.doc.refreshQueryString;
                            if (!string.IsNullOrEmpty(QueryString)) {
                                QueryString = "?" + QueryString;
                            }
                            string EmailBody = cpcore.webServer.requestProtocol + cpcore.webServer.requestDomain + cpcore.webServer.requestPathPage + QueryString;
                            string Caption = cpcore.siteProperties.getText("PageAllowEmailCaption", "Email This Page");
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
                if (cpcore.doc.sessionContext.isQuickEditing(cpcore, pageContentModel.contentName)) {
                    Cell = Cell + cpcore.doc.getQuickEditing(rootPageId, OrderByClause, AllowChildList, AllowReturnLink, ArchivePage, cpcore.doc.page.ContactMemberID, cpcore.doc.page.ChildListSortMethodID, allowChildListComposite, ArchivePage);
                } else {
                    //
                    // ----- Headline
                    //
                    if (cpcore.doc.page.Headline != "") {
                        string headline = cpcore.html.encodeHTML(cpcore.doc.page.Headline);
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
                        if (cpcore.doc.sessionContext.isEditing(pageContentModel.contentName)) {
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
                    if (allowChildListComposite || cpcore.doc.sessionContext.isEditingAnything()) {
                        if (!allowChildListComposite) {
                            Cell = Cell + cpcore.html.getAdminHintWrapper("Automatic Child List display is disabled for this page. It is displayed here because you are in editing mode. To enable automatic child list display, see the features tab for this page.");
                        }
                        addonModel addon = addonModel.create(cpcore, cpcore.siteProperties.childListAddonID);
                        CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                            addonType = CPUtilsBaseClass.addonContext.ContextPage,
                            hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                                contentName = pageContentModel.contentName,
                                fieldName = "",
                                recordId = cpcore.doc.page.id
                            },
                            instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpcore, cpcore.doc.page.ChildListInstanceOptions),
                            instanceGuid = PageChildListInstanceID,
                            wrapperID = cpcore.siteProperties.defaultWrapperID
                        };
                        Cell += cpcore.addon.execute(addon, executeContext);
                        //Cell = Cell & cpcore.addon.execute_legacy2(cpcore.siteProperties.childListAddonID, "", cpcore.doc.page.ChildListInstanceOptions, CPUtilsBaseClass.addonContext.ContextPage, pageContentModel.contentName, cpcore.doc.page.id, "", PageChildListInstanceID, False, cpcore.siteProperties.defaultWrapperID, "", AddonStatusOK, Nothing)
                    }
                }
                //
                // ----- End Text Search
                result = result + "\r<!-- TextSearchStart -->"
                    + genericController.htmlIndent(Cell) + "\r<!-- TextSearchEnd -->";
                //
                // ----- Page See Also
                if (cpcore.doc.page.AllowSeeAlso) {
                    result = result + "\r<div>"
                        + genericController.htmlIndent(getSeeAlso(cpcore, pageContentModel.contentName, cpcore.doc.page.id)) + "\r</div>";
                }
                //
                // ----- Allow More Info
                if ((cpcore.doc.page.ContactMemberID != 0) & cpcore.doc.page.AllowMoreInfo) {
                    result = result + "\r<ac TYPE=\"" + ACTypeContact + "\">";
                }
                //
                // ----- Feedback
                if ((cpcore.doc.page.ContactMemberID != 0) & cpcore.doc.page.AllowFeedback) {
                    result = result + "\r<ac TYPE=\"" + ACTypeFeedback + "\">";
                }
                //
                // ----- Last Modified line
                if ((cpcore.doc.page.modifiedDate != DateTime.MinValue) & cpcore.doc.page.AllowLastModifiedFooter) {
                    result = result + "\r<p>This page was last modified " + cpcore.doc.page.modifiedDate.ToString("G");
                    if (cpcore.doc.sessionContext.isAuthenticatedAdmin(cpcore)) {
                        if (cpcore.doc.page.modifiedBy == 0) {
                            result = result + " (admin only: modified by unknown)";
                        } else {
                            string personName = cpcore.db.getRecordName("people", cpcore.doc.page.modifiedBy);
                            if (string.IsNullOrEmpty(personName)) {
                                result = result + " (admin only: modified by person with unnamed or deleted record #" + cpcore.doc.page.modifiedBy + ")";
                            } else {
                                result = result + " (admin only: modified by " + personName + ")";
                            }
                        }
                    }
                    result = result + "</p>";
                }
                //
                // ----- Last Reviewed line
                if ((cpcore.doc.page.DateReviewed != DateTime.MinValue) & cpcore.doc.page.AllowReviewedFooter) {
                    result = result + "\r<p>This page was last reviewed " + cpcore.doc.page.DateReviewed.ToString("");
                    if (cpcore.doc.sessionContext.isAuthenticatedAdmin(cpcore)) {
                        if (cpcore.doc.page.ReviewedBy == 0) {
                            result = result + " (by unknown)";
                        } else {
                            string personName = cpcore.db.getRecordName("people", cpcore.doc.page.ReviewedBy);
                            if (string.IsNullOrEmpty(personName)) {
                                result = result + " (by person with unnamed or deleted record #" + cpcore.doc.page.ReviewedBy + ")";
                            } else {
                                result = result + " (by " + personName + ")";
                            }
                        }
                        result = result + ".</p>";
                    }
                }
                //
                // ----- Page Content Message Footer
                if (cpcore.doc.page.AllowMessageFooter) {
                    string pageContentMessageFooter = cpcore.siteProperties.getText("PageContentMessageFooter", "");
                    if (!string.IsNullOrEmpty(pageContentMessageFooter)) {
                        result = result + "\r<p>" + pageContentMessageFooter + "</p>";
                    }
                }
                //Call cpcore.db.cs_Close(CS)
            } catch (Exception ex) {
                cpcore.handleException(ex);
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
        public static string getSeeAlso(coreClass cpcore, string ContentName, int RecordID) {
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
                    ContentID = Models.Complex.cdefModel.getContentId(cpcore, iContentName);
                    if (ContentID > 0) {
                        //
                        // ----- Set authoring only for valid ContentName
                        //
                        IsEditingLocal = cpcore.doc.sessionContext.isEditing(iContentName);
                    } else {
                        //
                        // ----- if iContentName was bad, maybe they put table in, no authoring
                        //
                        ContentID = Models.Complex.cdefModel.getContentIDByTablename(cpcore, iContentName);
                    }
                    if (ContentID > 0) {
                        //
                        CS = cpcore.db.csOpen("See Also", "((active<>0)AND(ContentID=" + ContentID + ")AND(RecordID=" + iRecordID + "))");
                        while (cpcore.db.csOk(CS)) {
                            SeeAlsoLink = (cpcore.db.csGetText(CS, "Link"));
                            if (!string.IsNullOrEmpty(SeeAlsoLink)) {
                                result = result + "\r<li class=\"ccListItem\">";
                                if (genericController.vbInstr(1, SeeAlsoLink, "://") == 0) {
                                    SeeAlsoLink = cpcore.webServer.requestProtocol + SeeAlsoLink;
                                }
                                if (IsEditingLocal) {
                                    result = result + cpcore.html.getRecordEditLink2("See Also", (cpcore.db.csGetInteger(CS, "ID")), false, "", cpcore.doc.sessionContext.isEditing("See Also"));
                                }
                                result = result + "<a href=\"" + genericController.encodeHTML(SeeAlsoLink) + "\" target=\"_blank\">" + (cpcore.db.csGetText(CS, "Name")) + "</A>";
                                Copy = (cpcore.db.csGetText(CS, "Brief"));
                                if (!string.IsNullOrEmpty(Copy)) {
                                    result = result + "<br>" + genericController.AddSpan(Copy, "ccListCopy");
                                }
                                SeeAlsoCount = SeeAlsoCount + 1;
                                result = result + "</li>";
                            }
                            cpcore.db.csGoNext(CS);
                        }
                        cpcore.db.csClose(ref CS);
                        //
                        if (IsEditingLocal) {
                            SeeAlsoCount = SeeAlsoCount + 1;
                            result = result + "\r<li class=\"ccListItem\">" + cpcore.html.getRecordAddLink("See Also", "RecordID=" + iRecordID + ",ContentID=" + ContentID) + "</LI>";
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
                cpcore.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        // Print the "for more information, please contact" line
        //
        //========================================================================
        //
        public string main_GetMoreInfo(coreClass cpcore, int contactMemberID) {
            string tempmain_GetMoreInfo = null;
            string result = "";
            try {
                tempmain_GetMoreInfo = getMoreInfoHtml(cpcore, genericController.encodeInteger(contactMemberID));
            } catch (Exception ex) {
                cpcore.handleException(ex);
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
        public static string main_GetFeedbackForm(coreClass cpCore, string ContentName, int RecordID, int ToMemberID, string headline = "") {
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
                iHeadline = genericController.encodeEmptyText(headline, "");
                //
                const string FeedbackButtonSubmit = "Submit";
                //
                FeedbackButton = cpCore.docProperties.getText("fbb");
                switch (FeedbackButton) {
                    case FeedbackButtonSubmit:
                        //
                        // ----- form was submitted, save the note, send it and say thanks
                        //
                        NoteFromName = cpCore.docProperties.getText("NoteFromName");
                        NoteFromEmail = cpCore.docProperties.getText("NoteFromEmail");
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
                        Copy = cpCore.docProperties.getText("NoteCopy");
                        if (string.IsNullOrEmpty(Copy)) {
                            NoteCopy = NoteCopy + "[no comments entered]" + BR;
                        } else {
                            NoteCopy = NoteCopy + cpCore.html.convertCRLFToHtmlBreak(Copy) + BR;
                        }
                        //
                        NoteCopy = NoteCopy + BR;
                        NoteCopy = NoteCopy + "<b>Content on which the comments are based</b>" + BR;
                        //
                        CS = cpCore.db.csOpen(iContentName, "ID=" + iRecordID);
                        Copy = "[the content of this page is not available]" + BR;
                        if (cpCore.db.csOk(CS)) {
                            Copy = (cpCore.db.csGet(CS, "copyFilename"));
                            //Copy = main_EncodeContent5(Copy, c.authcontext.user.userid, iContentName, iRecordID, 0, False, False, True, True, False, True, "", "", False, 0)
                        }
                        NoteCopy = NoteCopy + Copy + BR;
                        cpCore.db.csClose(ref CS);
                        //
                        personModel person = personModel.create(cpCore, ToMemberID);
                        if ( person != null ) {
                            string sendStatus = "";
                            string queryStringForLinkAppend = "";
                            emailController.sendPerson(cpCore, person, NoteFromEmail, "Feedback Form Submitted", NoteCopy, false, true, 0, "", false,ref sendStatus, queryStringForLinkAppend);
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
                        Panel = "<form Action=\"" + cpCore.webServer.serverFormActionURL + "?" + cpCore.doc.refreshQueryString + "\" Method=\"post\">";
                        Panel = Panel + "<table border=\"0\" cellpadding=\"4\" cellspacing=\"0\" width=\"100%\">";
                        Panel = Panel + "<tr>";
                        Panel = Panel + "<td colspan=\"2\"><p>Your feedback is welcome</p></td>";
                        Panel = Panel + "</tr><tr>";
                        //
                        // ----- From Name
                        //
                        Copy = cpCore.doc.sessionContext.user.name;
                        Panel = Panel + "<td align=\"right\" width=\"100\"><p>Your Name</p></td>";
                        Panel = Panel + "<td align=\"left\"><input type=\"text\" name=\"NoteFromName\" value=\"" + genericController.encodeHTML(Copy) + "\"></span></td>";
                        Panel = Panel + "</tr><tr>";
                        //
                        // ----- From Email address
                        //
                        Copy = cpCore.doc.sessionContext.user.Email;
                        Panel = Panel + "<td align=\"right\" width=\"100\"><p>Your Email</p></td>";
                        Panel = Panel + "<td align=\"left\"><input type=\"text\" name=\"NoteFromEmail\" value=\"" + genericController.encodeHTML(Copy) + "\"></span></td>";
                        Panel = Panel + "</tr><tr>";
                        //
                        // ----- Message
                        //
                        Copy = "";
                        Panel = Panel + "<td align=\"right\" width=\"100\" valign=\"top\"><p>Feedback</p></td>";
                        Panel = Panel + "<td>" + cpCore.html.inputText("NoteCopy", Copy, 4, 40, "TextArea", false) + "</td>";
                        //Panel = Panel & "<td><textarea ID=""TextArea"" rows=""4"" cols=""40"" name=""NoteCopy"">" & Copy & "</textarea></td>"
                        Panel = Panel + "</tr><tr>";
                        //
                        // ----- submit button
                        //
                        Panel = Panel + "<td>&nbsp;</td>";
                        Panel = Panel + "<td><input type=\"submit\" name=\"fbb\" value=\"" + FeedbackButtonSubmit + "\"></td>";
                        Panel = Panel + "</tr></table>";
                        Panel = Panel + "</form>";
                        //
                        result = Panel;
                        break;
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
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