﻿
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Contensive.Processor.Models.Db;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.BaseClasses;

namespace Contensive.Processor.Controllers {
    /// <summary>
    /// persistence for the document under construction
    /// </summary>
    public class DocController {
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="core"></param>
        public DocController(CoreController core) {
            this.core = core;
            //
            pageController = new PageContentController();
            domain = new DomainModel();
            wysiwygAddonList = new Dictionary<CPHtmlBaseClass.EditorContentType, string>();
        }
        /// <summary>
        /// parent object
        /// </summary>
        private CoreController core;
        /// <summary>
        /// this documents unique guid (created on the fly)
        /// </summary>
        public string docGuid { get; set; }
        /// <summary>
        /// boolean that tracks if the current document is html. set true if any addon executed is set htmlDocument=true. When true, the initial addon executed is returned in the html wrapper (html with head)
        /// If no addon executed is an html addon, the result is returned as-is.
        /// </summary>
        public bool isHtml { get; set; } = false;
        /// <summary>
        /// head tags, script tags, style tags, etc
        /// </summary>
        public List<HtmlAssetClass> htmlAssetList { get; set; } = new List<HtmlAssetClass>();
        /// <summary>
        /// head meta tag list (convert to list object)
        /// </summary>
        public List<HtmlMetaClass> htmlMetaContent_OtherTags { get; set; } = new List<HtmlMetaClass>();
        /// <summary>
        /// html title elements
        /// </summary>
        public List<HtmlMetaClass> htmlMetaContent_TitleList { get; set; } = new List<HtmlMetaClass>();
        /// <summary>
        /// html meta description
        /// </summary>
        public List<HtmlMetaClass> htmlMetaContent_Description { get; set; } = new List<HtmlMetaClass>();
        /// <summary>
        /// html meta keywords
        /// </summary>
        public List<HtmlMetaClass> htmlMetaContent_KeyWordList { get; set; } = new List<HtmlMetaClass>();
        /// <summary>
        /// current domain for website documents. For all others this is the primary domain for the application.
        /// </summary>
        public DomainModel domain { get; set; }
        /// <summary>
        /// if this document is composed of page content records and templates, this object provides supporting properties and methods
        /// </summary>
        public PageContentController pageController { get; }
        /// <summary>
        /// Anything that needs to be written to the Page during main_GetClosePage
        /// </summary>
        public string htmlForEndOfBody { get; set; } = "";
        //
        public Dictionary<CPHtmlBaseClass.EditorContentType, string> wysiwygAddonList;
        //
        // -- others to be sorted
        public int editWrapperCnt { get; set; } = 0;
        //
        // -- todo
        public string docBodyFilter { get; set; } = "";
        //
        // -- todo
        public bool legacySiteStyles_Loaded { get; set; } = false;
        //
        // -- todo
        public int menuSystemCloseCount { get; set; } = 0;
        //
        // -- todo
        internal class HelpStuff {
            public String code;
            public String caption;
        }
        //
        // -- In advanced edit, each addon edit header has several help-bubble popups. This is a list for them.
        internal List<HelpStuff> helpCodes { get; set; } = new List<HelpStuff>();
        //
        // -- todo
        internal int helpDialogCnt { get; set; } = 0;
        /// <summary>
        /// querystring required to return to the current state (perform a refresh)
        /// </summary>
        public string refreshQueryString { get; set; } = "";
        //
        // -- todo
        public int redirectContentID { get; set; } = 0;
        //
        // -- todo
        public int redirectRecordID { get; set; } = 0;
        /// <summary>
        /// when true (default), stream is buffered until page is done
        /// </summary>
        public bool outputBufferEnabled { get; set; } = true;
        /// <summary>
        /// Message - when set displays in an admin hint box in the page
        /// </summary>
        public string adminWarning { get; set; } = "";
        /// <summary>
        /// PageID that goes with the warning
        /// </summary>
        public int adminWarningPageID { get; set; } = 0;
        //
        // -- todo
        public int checkListCnt { get; set; } = 0; // cnt of the main_GetFormInputCheckList calls - used for javascript
        //
        // -- todo
        public int inputDateCnt { get; set; } = 0;
        //
        // -- todo
        public List<CacheInputSelectClass> inputSelectCache = new List<CacheInputSelectClass>();
        //
        // -- todo
        public int formInputTextCnt { get; set; } = 0;
        //
        // -- todo
        public string quickEditCopy { get; set; } = "";
        //
        // -- todo
        public string siteStructure { get; set; } = "";
        //
        // -- todo
        public bool siteStructure_LocalLoaded { get; set; } = false;
        //
        // -- todo
        public string bodyContent { get; set; } = ""; // stored here so cp.doc.content valid during bodyEnd event
        //
        // -- todo
        public int landingPageID { get; set; } = 0;
        //
        // -- todo
        public string redirectLink { get; set; } = "";
        //
        // -- todo
        public string redirectReason { get; set; } = "";
        //
        // -- todo
        public bool redirectBecausePageNotFound { get; set; } = false;
        //
        // -- todo
        internal List<string> errList { get; set; } // exceptions collected during document construction
        //
        // -- todo
        public int errorCount { get; set; } = 0;
        //
        // -- todo
        internal List<string> userErrorList = new List<string>();
        //
        // -- todo
        public string debug_iUserError { get; set; } = ""; // User Error String
        //
        // -- todo
        public string testPointMessage { get; set; } = "";
        //
        // -- todo
        public bool visitPropertyAllowDebugging { get; set; } = false; // if true, send main_TestPoint messages to the stream
        //
        // -- todo
        internal Stopwatch appStopWatch { get; set; } = Stopwatch.StartNew();
        //
        // -- todo
        public DateTime profileStartTime { get; set; } // set in constructor
        //
        // -- todo
        public bool allowDebugLog { get; set; } = false; // turn on in script -- use to write /debug.log in content files for whatever is needed
        //
        // -- todo
        public bool blockExceptionReporting { get; set; } = false; // used so error reporting can not call itself
        //
        // -- todo
        public bool continueProcessing { get; set; } = false; // when false, routines should not add to the output and immediately exit
        //
        // -- todo
        internal List<int> addonIdListRunInThisDoc { get; set; } = new List<int>();
        /// <summary>
        /// If ContentId in this list, they are not a content manager
        /// </summary>
        internal List<int> contentAccessRights_NotList { get; set; } = new List<int>();
        /// <summary>
        /// If ContentId in this list, they are a content manager
        /// </summary>
        internal List<int> contentAccessRights_List { get; set; } = new List<int>();
        /// <summary>
        /// If in _List, test this for allowAdd
        /// </summary>
        internal List<int> contentAccessRights_AllowAddList { get; set; } = new List<int>();
        /// <summary>
        /// If in _List, test this for allowDelete
        /// </summary>
        internal List<int> contentAccessRights_AllowDeleteList { get; set; } = new List<int>();
        /// <summary>
        /// list of content names that have been verified editable by this user
        /// </summary>
        internal List<string> contentIsEditingList = new List<string>();
        /// <summary>
        /// list of content names verified that this user CAN NOT edit
        /// </summary>
        internal List<string> contentNotEditingList = new List<string>();
        //
        /// <summary>
        /// Dictionary of addons running to track recursion, addonId and count of recursive entries. When executing an addon, check if it is in the list, if so, check if the recursion count is under the limit (addonRecursionDepthLimit). If not add it or increment the count. On exit, decrement the count and remove if 0.
        /// </summary>
        internal Dictionary<int, int> addonRecursionDepth { get; set; } = new Dictionary<int, int>();
        //
        public Stack<Models.Db.AddonModel> addonModelStack = new Stack<AddonModel>();
        //
        // -- todo
        public int addonInstanceCnt { get; set; } = 0;
        //
        // -- Email Block List - these are people who have asked to not have email sent to them from this site, Loaded ondemand by csv_GetEmailBlockList
        public string emailBlockListStore { get; set; } = "";
        public bool emailBlockListStoreLoaded { get; set; }
        //
        //====================================================================================================
        //
        internal string landingLink {
            get {
                if (_landingLink == "") {
                    _landingLink = core.siteProperties.getText("SectionLandingLink", "/" + core.siteProperties.serverPageDefault);
                    _landingLink = GenericController.ConvertLinkToShortLink(_landingLink, core.webServer.requestDomain, core.appConfig.cdnFileUrl);
                    _landingLink = GenericController.encodeVirtualPath(_landingLink, core.appConfig.cdnFileUrl, appRootPath, core.webServer.requestDomain);
                }
                return _landingLink;
            }
        }
        private string _landingLink = "";
        //
        //
        public void sendPublishSubmitNotice(string ContentName, int RecordID, string RecordName) {
            try {
                Models.Domain.MetaModel CDef = null;
                string Copy = null;
                string Link = null;
                string FromAddress = null;
                //
                FromAddress = core.siteProperties.getText("EmailPublishSubmitFrom", core.siteProperties.emailAdmin);
                CDef = Models.Domain.MetaModel.createByUniqueName(core, ContentName);
                Link = "/" + core.appConfig.adminRoute + "?af=" + AdminFormPublishing;
                Copy = Msg_AuthoringSubmittedNotification;
                Copy = GenericController.vbReplace(Copy, "<DOMAINNAME>", "<a href=\"" + HtmlController.encodeHtml(Link) + "\">" + core.webServer.requestDomain + "</a>");
                Copy = GenericController.vbReplace(Copy, "<RECORDNAME>", RecordName);
                Copy = GenericController.vbReplace(Copy, "<CONTENTNAME>", ContentName);
                Copy = GenericController.vbReplace(Copy, "<RECORDID>", RecordID.ToString());
                Copy = GenericController.vbReplace(Copy, "<SUBMITTEDDATE>", core.doc.profileStartTime.ToString());
                Copy = GenericController.vbReplace(Copy, "<SUBMITTEDNAME>", core.session.user.name);
                //
                EmailController.queueGroupEmail(core, core.siteProperties.getText("WorkflowEditorGroup", "Site Managers"), FromAddress, "Authoring Submitted Notification", Copy, false, true);
                //
                return;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            //ErrorTrap:
            ////throw new GenericException("Unexpected exception"); // Call core.handleLegacyError18(MethodName)
            //
        }
        //
        //=============================================================================
        //   main_Get the link for a Content Record by the ContentName and RecordID
        //=============================================================================
        //
        public string getContentWatchLinkByName(string ContentName, int RecordID, string DefaultLink = "", bool IncrementClicks = true) {
            string result = "";
            try {
                string ContentRecordKey = Models.Domain.MetaModel.getContentId(core, GenericController.encodeText(ContentName)) + "." + GenericController.encodeInteger(RecordID);
                result = getContentWatchLinkByKey(ContentRecordKey, DefaultLink, IncrementClicks);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //=============================================================================
        /// <summary>
        /// Get the link for a Content Record by its ContentRecordKey
        /// </summary>
        /// <param name="ContentRecordKey"></param>
        /// <param name="DefaultLink"></param>
        /// <param name="IncrementClicks"></param>
        /// <returns></returns>
        public string getContentWatchLinkByKey(string ContentRecordKey, string DefaultLink, bool IncrementClicks) {
            string result = "";
            try {
                using (var csXfer = new CsModel(core)) {
                    if (csXfer.csOpen("Content Watch", "ContentRecordKey=" + DbController.encodeSQLText(ContentRecordKey), "", false, 0, "Link,Clicks")) {
                        result = csXfer.csGetText("Link");
                        if (GenericController.encodeBoolean(IncrementClicks)) { csXfer.csSet("Clicks", csXfer.csGetInteger("clicks") + 1); }
                    } else {
                        result = GenericController.encodeText(DefaultLink);
                    }
                }
                return GenericController.encodeVirtualPath(result, core.appConfig.cdnFileUrl, appRootPath, core.webServer.requestDomain);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        public string getContentWatchLinkByKey(string ContentRecordKey, string DefaultLink) => getContentWatchLinkByKey(ContentRecordKey, DefaultLink, false);
        //
        public string getContentWatchLinkByKey(string ContentRecordKey) => getContentWatchLinkByKey(ContentRecordKey, "", false);
        //
        //====================================================================================================
        // Replace with main_GetPageArgs()
        //
        // Used Interally by main_GetPageLink to main_Get the SectionID of the parents
        // Dim siteSectionRootPageIndex As Dictionary(Of Integer, Integer) = siteSectionModel.getRootPageIdIndex(Me)
        //====================================================================================================
        //
        internal int getPageSectionId(int PageID, ref List<int> UsedIDList, Dictionary<int, int> siteSectionRootPageIndex) {
            int sectionId = 0;
            try {
                PageContentModel page = PageContentModel.create(core, PageID);
                if (page != null) {
                    if ((page.parentID == 0) && (!UsedIDList.Contains(page.parentID))) {
                        UsedIDList.Add(page.parentID);
                        if (siteSectionRootPageIndex.ContainsKey(page.parentID)) {
                            sectionId = siteSectionRootPageIndex[page.parentID];
                        }
                    } else {
                        sectionId = getPageSectionId(page.parentID, ref UsedIDList, siteSectionRootPageIndex);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return sectionId;
        }
        //
        //====================================================================================================
        // Replace with main_GetPageArgs()
        //
        // Used Interally by main_GetPageLink to main_Get the TemplateID of the parents
        //====================================================================================================
        //
        internal int getPageDynamicLink_GetTemplateID(int PageID, string UsedIDList) {
            int result = 0;
            try {
                int ParentID = 0;
                int templateId = 0;
                using (var csXfer = new CsModel(core)) {
                    if (csXfer.csOpenRecord("Page Content", PageID, "TemplateID,ParentID")) {
                        templateId = csXfer.csGetInteger("TemplateID");
                        ParentID = csXfer.csGetInteger("ParentID");
                    }
                }
                //
                // Chase page tree to main_Get templateid
                if (templateId == 0 && ParentID != 0) {
                    if (!GenericController.isInDelimitedString(UsedIDList, ParentID.ToString(), ",")) {
                        result = getPageDynamicLink_GetTemplateID(ParentID, UsedIDList + "," + ParentID);
                    }
                }
                return result;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        // main_Get a page link if you know nothing about the page
        //   If you already have all the info, lik the parents templateid, etc, call the ...WithArgs call
        //====================================================================================================
        //
        public string main_GetPageDynamicLink(int PageID, bool UseContentWatchLink) {
            //
            int CCID = 0;
            string DefaultLink = null;
            int SectionID = 0;
            bool IsRootPage = false;
            int templateId = 0;
            string MenuLinkOverRide = "";
            //
            //
            // Convert default page to default link
            //
            DefaultLink = core.siteProperties.serverPageDefault;
            if (DefaultLink.Left(1) != "/") {
                DefaultLink = "/" + core.siteProperties.serverPageDefault;
            }
            //
            return main_GetPageDynamicLinkWithArgs(CCID, PageID, DefaultLink, IsRootPage, templateId, SectionID, MenuLinkOverRide, UseContentWatchLink);
        }
        //====================================================================================================
        /// <summary>
        /// main_GetPageDynamicLinkWithArgs
        /// </summary>
        /// <param name="ContentControlID"></param>
        /// <param name="PageID"></param>
        /// <param name="DefaultLink"></param>
        /// <param name="IsRootPage"></param>
        /// <param name="templateId"></param>
        /// <param name="SectionID"></param>
        /// <param name="MenuLinkOverRide"></param>
        /// <param name="UseContentWatchLink"></param>
        /// <returns></returns>
        internal string main_GetPageDynamicLinkWithArgs(int ContentControlID, int PageID, string DefaultLink, bool IsRootPage, int templateId, int SectionID, string MenuLinkOverRide, bool UseContentWatchLink) {
            string resultLink = "";
            try {
                if (!string.IsNullOrEmpty(MenuLinkOverRide)) {
                    //
                    // -- redirect to this page record
                    resultLink = "?rc=" + ContentControlID + "&ri=" + PageID;
                } else {
                    if (UseContentWatchLink) {
                        //
                        // -- Legacy method - lookup link from a table set during the last page hit
                        resultLink = getContentWatchLinkByID(ContentControlID, PageID, DefaultLink, false);
                    } else {
                        //
                        // -- Current method - all pages are in the Template, Section, Page structure
                        if (templateId != 0) {
                            PageTemplateModel template = PageTemplateModel.create(core, templateId);
                            if (template != null) {
                                resultLink = ""; // template.Link
                            }
                        }
                        if (string.IsNullOrEmpty(resultLink)) {
                            //
                            // -- not found, use default
                            if (!string.IsNullOrEmpty(DefaultLink)) {
                                //
                                // if default given, use that
                                resultLink = DefaultLink;
                            } else {
                                //
                                // -- fallback, use content watch
                                resultLink = getContentWatchLinkByID(ContentControlID, PageID, "", false);
                            }
                        }
                        if ((PageID == 0) || (IsRootPage)) {
                            //
                            // -- Link to Root Page, no bid, and include sectionid if not 0
                            if (IsRootPage && (SectionID != 0)) {
                                resultLink = GenericController.modifyLinkQuery(resultLink, "sid", SectionID.ToString(), true);
                            }
                            resultLink = GenericController.modifyLinkQuery(resultLink, rnPageId, "", false);
                        } else {
                            resultLink = GenericController.modifyLinkQuery(resultLink, rnPageId, GenericController.encodeText(PageID), true);
                            if (PageID != 0) {
                                resultLink = GenericController.modifyLinkQuery(resultLink, "sid", "", false);
                            }
                        }
                    }
                }
                resultLink = GenericController.encodeVirtualPath(resultLink, core.appConfig.cdnFileUrl, appRootPath, core.webServer.requestDomain);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return resultLink;
        }
        //
        //=============================================================================
        //   main_Get the link for a Content Record by the ContentID and RecordID
        //=============================================================================
        //
        public string getContentWatchLinkByID(int ContentID, int RecordID, string DefaultLink = "", bool IncrementClicks = true) {
            return getContentWatchLinkByKey(GenericController.encodeText(ContentID) + "." + GenericController.encodeText(RecordID), DefaultLink, IncrementClicks);
        }
        //
        //=============================================================================
        //
        public void verifyRegistrationFormPage(CoreController core) {
            try {
                MetaController.deleteContentRecords(core, "Form Pages", "name=" + DbController.encodeSQLText("Registration Form"));
                using (var csXfer = new CsModel(core)) {
                    if (!csXfer.csOpen("Form Pages", "name=" + DbController.encodeSQLText("Registration Form"))) {
                        //
                        // create Version 1 template - just to main_Get it started
                        //
                        if (csXfer.insert("Form Pages")) {
                            csXfer.csSet("name", "Registration Form");
                            string Copy = ""
                                + "\r\n<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" width=\"100%\">"
                                + "\r\n{{REPEATSTART}}<tr><td align=right style=\"height:22px;\">{{CAPTION}}&nbsp;</td><td align=left>{{FIELD}}</td></tr>{{REPEATEND}}"
                                + "\r\n<tr><td align=right><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=135 height=1></td><td width=\"100%\">&nbsp;</td></tr>"
                                + "\r\n<tr><td colspan=2>&nbsp;<br>" + core.html.getPanelButtons(ButtonRegister, "Button") + "</td></tr>"
                                + "\r\n</table>";
                            csXfer.csSet("Body", Copy);
                            Copy = ""
                                + "1"
                                + "\r\nRegistered\r\ntrue"
                                + "\r\n1,First Name,true,FirstName"
                                + "\r\n1,Last Name,true,LastName"
                                + "\r\n1,Email Address,true,Email"
                                + "\r\n1,Phone,true,Phone"
                                + "\r\n2,Please keep me informed of news and events,false,Subscribers"
                                + "";
                            csXfer.csSet("Instructions", Copy);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //---------------------------------------------------------------------------
        //   Create the default landing page if it is missing
        //---------------------------------------------------------------------------
        //
        public int createPageGetID(string PageName, string ContentName, int CreatedBy, string pageGuid) {
            int Id = 0;
            using (var csXfer = new CsModel(core)) {
                if (csXfer.csInsert(ContentName, CreatedBy)) {
                    Id = csXfer.csGetInteger("ID");
                    csXfer.csSet("name", PageName);
                    csXfer.csSet("active", "1");
                    if (true) {
                        csXfer.csSet("ccGuid", pageGuid);
                    }
                    csXfer.csSave();
                }
            }
            return Id;
        }
        //
        //
        //
        public void addRefreshQueryString(string Name, string Value = "") {
            try {
                string[] temp = null;
                //
                if (Name.IndexOf("=") + 1 > 0) {
                    temp = Name.Split('=');
                    refreshQueryString = GenericController.modifyQueryString(core.doc.refreshQueryString, temp[0], temp[1], true);
                } else {
                    refreshQueryString = GenericController.modifyQueryString(core.doc.refreshQueryString, Name, Value, true);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }

        }
        //
        //=============================================================================
        /// <summary>
        /// Sets the MetaContent subsystem so the next call to main_GetLastMeta... returns the correct value
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="recordId"></param>
        public void setMetaContent(int contentId, int recordId) {
            if ((contentId != 0) && (recordId != 0)) {
                //
                // -- open meta content record
                string Criteria = "(ContentID=" + contentId + ")and(RecordID=" + recordId + ")";
                string FieldList = "ID,Name,MetaDescription,OtherHeadTags,MetaKeywordList";
                string keywordList = "";
                int MetaContentID = 0;
                using (var csXfer = new CsModel(core)) {
                    if (csXfer.csOpen("Meta Content", Criteria, "", false, 0, FieldList)) {
                        MetaContentID = csXfer.csGetInteger("ID");
                        core.html.addTitle(HtmlController.encodeHtml(csXfer.csGetText("Name")), "page content");
                        core.html.addMetaDescription(HtmlController.encodeHtml(csXfer.csGetText("MetaDescription")), "page content");
                        core.html.addHeadTag(csXfer.csGetText("OtherHeadTags"), "page content");
                        keywordList = csXfer.csGetText("MetaKeywordList").Replace("\r\n", ",");
                    }
                    csXfer.close();
                }
                //
                // open Keyword List
                using (var csXfer = new CsModel(core)) {
                    string SQL = "select ccMetaKeywords.Name"
                        + " From ccMetaKeywords"
                        + " LEFT JOIN ccMetaKeywordRules on ccMetaKeywordRules.MetaKeywordID=ccMetaKeywords.ID"
                        + " Where ccMetaKeywordRules.MetaContentID=" + MetaContentID;
                    csXfer.csOpenSql(SQL);
                    while (csXfer.csOk()) {
                        keywordList = keywordList + "," + csXfer.csGetText("Name");
                        csXfer.csGoNext();
                    }
                    if (!string.IsNullOrEmpty(keywordList)) {
                        if (keywordList.Left(1) == ",") {
                            keywordList = keywordList.Substring(1);
                        }
                        keywordList = HtmlController.encodeHtml(keywordList);
                        core.html.addMetaKeywordList(keywordList, "page content");
                    }
                }
            }
        }
    }
}

