
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
using Contensive.BaseClasses;
//
//
namespace Contensive.Core.Controllers {
    //
    public class docController {
        /// <summary>
        /// Persistence for the doc. Maintain all the parts and output the results. Constructor initializes the object. Call initDoc() to setup pages
        /// </summary>
        // -- not sure if this is the best plan, buts lets try this and see if we can get out of it later (to make this an addon) 
        //
        private coreController core;
        //
        // -- this documents unique guid (created on the fly)
        public string docGuid { get; set; }
        //
        // -- set true if any addon executed is set  htmlDocument=true. When true, the initial addon executed is returned in the html wrapper (html with head)
        public bool htmlDocument { get; set; } = false;
        //
        // -- head tags, script tags, style tags, etc
        public List<htmlAssetClass> htmlAssetList { get; set; } = new List<htmlAssetClass>();
        //
        // -- head meta tag list (convert to list object)
        public List<htmlMetaClass> htmlMetaContent_OtherTags { get; set; } = new List<htmlMetaClass>();
        public List<htmlMetaClass> htmlMetaContent_TitleList { get; set; } = new List<htmlMetaClass>();
        public List<htmlMetaClass> htmlMetaContent_Description { get; set; } = new List<htmlMetaClass>();
        public List<htmlMetaClass> htmlMetaContent_KeyWordList { get; set; } = new List<htmlMetaClass>();
        //
        // -- current domain
        public domainModel domain { get; set; }
        //
        // -- current page
        public pageContentModel page { get; set; }
        //
        // todo docController properties relating to html page rending should go in a pageController property, and not spread out
        // -- current page to it's root
        public List<Models.DbModels.pageContentModel> pageToRootList { get; set; }
        //
        // -- current template
        public pageTemplateModel template { get; set; }
        public string templateReason { get; set; } = "";
        //
        // -- Anything that needs to be written to the Page during main_GetClosePage
        public string htmlForEndOfBody { get; set; } = "";
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
        internal class helpStuff {
            public String code;
            public String caption;
        }
        //
        // -- In advanced edit, each addon edit header has several help-bubble popups. This is a list for them.
        internal List<helpStuff> helpCodes { get; set; } = new List<helpStuff>();
        //
        // -- todo
        internal int helpDialogCnt { get; set; } = 0;
        //
        // -- todo
        public string refreshQueryString { get; set; } = ""; // the querystring required to return to the current state (perform a refresh)
        //
        // -- todo
        public int redirectContentID { get; set; } = 0;
        //
        // -- todo
        public int redirectRecordID { get; set; } = 0;
        //
        // -- todo
        public bool outputBufferEnabled { get; set; } = true; // when true (default), stream is buffered until page is done
        //
        // -- todo
        public menuComboTabController menuComboTab { get; set; }
        //
        // -- todo
        public menuLiveTabController menuLiveTab { get; set; }
        //
        // -- todo
        public string adminWarning { get; set; } = ""; // Message - when set displays in an admin hint box in the page
        //
        // -- todo
        public int adminWarningPageID { get; set; } = 0; // PageID that goes with the warning
        //
        // -- todo
        public int checkListCnt { get; set; } = 0; // cnt of the main_GetFormInputCheckList calls - used for javascript
        //
        // -- todo
        //public string includedAddonIDList { get; set; } = "";
        //
        // -- todo
        public int inputDateCnt { get; set; } = 0;
        //
        // -- todo
        public List<cacheInputSelectClass> inputSelectCache = new List<cacheInputSelectClass>() { };
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
        internal List<string> userErrorList = new List<string>() { };
        //
        // -- todo
        public string debug_iUserError { get; set; } = ""; // User Error String
        //
        // -- todo
        public string trapLogMessage { get; set; } = ""; // The content of the current traplog (keep for popups if no Csv)
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
        public int profileStartTickCount { get; set; } = 0;
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
        public bool upgradeInProgress { get; set; }
        //
        // -- todo
        internal List<int> addonIdListRunInThisDoc { get; set; } = new List<int>();
        //
        // -- todo
        internal List<int> addonsCurrentlyRunningIdList { get; set; } = new List<int>();
        //
        public Stack<Models.DbModels.addonModel> addonModelStack = new Stack<addonModel>();
        //
        // -- todo
        public int addonInstanceCnt { get; set; } = 0;
        //
        // -- persistant store for cdef complex model
        internal Dictionary<string, Models.Complex.cdefModel> cdefDictionary { get; set; }
        //
        // -- persistant store for tableSchema complex mode
        internal Dictionary<string, Models.Complex.tableSchemaModel> tableSchemaDictionary { get; set; }
        //
        // -- Email Block List - these are people who have asked to not have email sent to them from this site, Loaded ondemand by csv_GetEmailBlockList
        public string emailBlockList_Local { get; set; } = "";
        public bool emailBlockListLocalLoaded { get; set; }
        //
        // -- list of log files, managed in logController
        public Dictionary<string, TextWriterTraceListener> logList = new Dictionary<string, TextWriterTraceListener>();
        //
        //====================================================================================================
        // -- lookup contentId by contentName
        internal Dictionary<string, int> contentNameIdDictionary {
            get {
                if (_contentNameIdDictionary == null) {
                    _contentNameIdDictionary = new Dictionary<string, int>();
                    foreach (KeyValuePair<int, contentModel> kvp in contentIdDict) {
                        string key = kvp.Value.name.Trim().ToLower();
                        if (!string.IsNullOrEmpty(key)) {
                            if (!_contentNameIdDictionary.ContainsKey(key)) {
                                _contentNameIdDictionary.Add(key, kvp.Value.id);
                            }
                        }
                    }
                }
                return _contentNameIdDictionary;
            }
        }
        internal void contentNameIdDictionaryClear() {
            _contentNameIdDictionary = null;
        } private Dictionary<string, int> _contentNameIdDictionary = null;
        //
        //====================================================================================================
        // -- lookup contentModel by contentId
        internal Dictionary<int, contentModel> contentIdDict {
            get {
                if (_contentIdDict == null) {
                    _contentIdDict = contentModel.createDict(core, new List<string>());
                }
                return _contentIdDict;
            }
        }
        internal string landingLink {
            get {
                if (_landingLink == "") {
                    _landingLink = core.siteProperties.getText("SectionLandingLink", requestAppRootPath + core.siteProperties.serverPageDefault);
                    _landingLink = genericController.ConvertLinkToShortLink(_landingLink, core.webServer.requestDomain, core.webServer.requestVirtualFilePath);
                    _landingLink = genericController.EncodeAppRootPath(_landingLink, core.webServer.requestVirtualFilePath, requestAppRootPath, core.webServer.requestDomain);
                }
                return _landingLink;
            }
        }
        private string _landingLink { get; set; } = ""; // Default Landing page - managed through main_GetLandingLink() '
        internal void contentIdDictClear() {
            _contentIdDict = null;
        }
        private Dictionary<int, contentModel> _contentIdDict = null;
        //
        //====================================================================================================
        /// <summary>
        /// this will eventuall be an addon, but lets do this first to keep the converstion complexity down
        /// </summary>
        /// <param name="core"></param>
        public docController(coreController core) {
            this.core = core;
            //
            domain = new domainModel();
            page = new pageContentModel();
            pageToRootList = new List<pageContentModel>();
            template = new pageTemplateModel();
            cdefDictionary = new Dictionary<string, Models.Complex.cdefModel>();
            tableSchemaDictionary = null;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        public int main_OpenCSWhatsNew(coreController core, string SortFieldList = "", bool ActiveOnly = true, int PageSize = 1000, int PageNumber = 1) {
            int result = -1;
            try {
                result = main_OpenCSContentWatchList(core, "What's New", SortFieldList, ActiveOnly, PageSize, PageNumber);
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        //   Open a content set with the current whats new list
        //========================================================================
        //
        public int main_OpenCSContentWatchList(coreController core, string ListName, string SortFieldList = "", bool ActiveOnly = true, int PageSize = 1000, int PageNumber = 1) {
            int result = -1;
            try {
                string SQL = null;
                string iSortFieldList = null;
                string MethodName = null;
                int CS = 0;
                //
                iSortFieldList = encodeText(encodeEmptyText(SortFieldList, "")).Trim(' ');
                //iSortFieldList = encodeMissingText(SortFieldList, "DateAdded")
                if (string.IsNullOrEmpty(iSortFieldList)) {
                    iSortFieldList = "DateAdded";
                }
                //
                MethodName = "main_OpenCSContentWatchList( " + ListName + ", " + iSortFieldList + ", " + ActiveOnly + " )";
                //
                // ----- Add tablename to the front of SortFieldList fieldnames
                //
                iSortFieldList = " " + genericController.vbReplace(iSortFieldList, ",", " , ") + " ";
                iSortFieldList = genericController.vbReplace(iSortFieldList, " ID ", " ccContentWatch.ID ", 1, 99, 1);
                iSortFieldList = genericController.vbReplace(iSortFieldList, " Link ", " ccContentWatch.Link ", 1, 99, 1);
                iSortFieldList = genericController.vbReplace(iSortFieldList, " LinkLabel ", " ccContentWatch.LinkLabel ", 1, 99, 1);
                iSortFieldList = genericController.vbReplace(iSortFieldList, " SortOrder ", " ccContentWatch.SortOrder ", 1, 99, 1);
                iSortFieldList = genericController.vbReplace(iSortFieldList, " DateAdded ", " ccContentWatch.DateAdded ", 1, 99, 1);
                iSortFieldList = genericController.vbReplace(iSortFieldList, " ContentID ", " ccContentWatch.ContentID ", 1, 99, 1);
                iSortFieldList = genericController.vbReplace(iSortFieldList, " RecordID ", " ccContentWatch.RecordID ", 1, 99, 1);
                iSortFieldList = genericController.vbReplace(iSortFieldList, " ModifiedDate ", " ccContentWatch.ModifiedDate ", 1, 99, 1);
                //
                // ----- Special case
                //
                iSortFieldList = genericController.vbReplace(iSortFieldList, " name ", " ccContentWatch.LinkLabel ", 1, 99, 1);
                //
                SQL = "SELECT"
                    + " ccContentWatch.ID AS ID"
                    + ",ccContentWatch.Link as Link"
                    + ",ccContentWatch.LinkLabel as LinkLabel"
                    + ",ccContentWatch.SortOrder as SortOrder"
                    + ",ccContentWatch.DateAdded as DateAdded"
                    + ",ccContentWatch.ContentID as ContentID"
                    + ",ccContentWatch.RecordID as RecordID"
                    + ",ccContentWatch.ModifiedDate as ModifiedDate"
                + " FROM (ccContentWatchLists"
                    + " LEFT JOIN ccContentWatchListRules ON ccContentWatchLists.ID = ccContentWatchListRules.ContentWatchListID)"
                    + " LEFT JOIN ccContentWatch ON ccContentWatchListRules.ContentWatchID = ccContentWatch.ID"
                + " WHERE (((ccContentWatchLists.Name)=" + this.core.db.encodeSQLText(ListName) + ")"
                    + "AND ((ccContentWatchLists.Active)<>0)"
                    + "AND ((ccContentWatchListRules.Active)<>0)"
                    + "AND ((ccContentWatch.Active)<>0)"
                    + "AND (ccContentWatch.Link is not null)"
                    + "AND (ccContentWatch.LinkLabel is not null)"
                    + "AND ((ccContentWatch.WhatsNewDateExpires is null)or(ccContentWatch.WhatsNewDateExpires>" + this.core.db.encodeSQLDate(core.doc.profileStartTime) + "))"
                    + ")"
                + " ORDER BY " + iSortFieldList + ";";
                result = this.core.db.csOpenSql(SQL, "", PageSize, PageNumber);
                if (!this.core.db.csOk(result)) {
                    //
                    // Check if listname exists
                    //
                    CS = this.core.db.csOpen("Content Watch Lists", "name=" + this.core.db.encodeSQLText(ListName), "ID", false, 0, false, false, "ID");
                    if (!this.core.db.csOk(CS)) {
                        this.core.db.csClose(ref CS);
                        CS = this.core.db.csInsertRecord("Content Watch Lists");
                        if (this.core.db.csOk(CS)) {
                            this.core.db.csSet(CS, "name", ListName);
                        }
                    }
                    this.core.db.csClose(ref CS);
                }
            } catch (Exception ex) {
                this.core.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        // Print Whats New
        //   Prints a linked list of new content
        //========================================================================
        //
        public string main_GetWhatsNew(coreController core, string SortFieldList = "") {
            string result = "";
            try {
                int CSPointer = 0;
                int ContentID = 0;
                int RecordID = 0;
                string LinkLabel = null;
                string Link = null;
                //
                CSPointer = main_OpenCSWhatsNew(core, SortFieldList);
                //
                if (this.core.db.csOk(CSPointer)) {
                    ContentID = Models.Complex.cdefModel.getContentId(core, "Content Watch");
                    while (this.core.db.csOk(CSPointer)) {
                        Link = this.core.db.csGetText(CSPointer, "link");
                        LinkLabel = this.core.db.csGetText(CSPointer, "LinkLabel");
                        RecordID = this.core.db.csGetInteger(CSPointer, "ID");
                        if (!string.IsNullOrEmpty(LinkLabel)) {
                            result = result + "\r<li class=\"ccListItem\">";
                            if (!string.IsNullOrEmpty(Link)) {
                                result = result + genericController.csv_GetLinkedText("<a href=\"" + genericController.encodeHTML(core.webServer.requestPage + "?rc=" + ContentID + "&ri=" + RecordID) + "\">", LinkLabel);
                            } else {
                                result = result + LinkLabel;
                            }
                            result = result + "</li>";
                        }
                        this.core.db.csGoNext(CSPointer);
                    }
                    result = "\r<ul class=\"ccWatchList\">" + htmlIndent(result) + "\r</ul>";
                }
                this.core.db.csClose(ref CSPointer);
            } catch (Exception ex) {
                this.core.handleException(ex);
            }
            return result;
        }
        //
        //
        //
        public string main_GetWatchList(coreController core, string ListName, string SortField, bool SortReverse) {
            string result = "";
            try {
                int CS = 0;
                int ContentID = 0;
                int RecordID = 0;
                string Link = null;
                string LinkLabel = null;
                //
                if (SortReverse && (!string.IsNullOrEmpty(SortField))) {
                    CS = main_OpenCSContentWatchList(core, ListName, SortField + " Desc", true);
                } else {
                    CS = main_OpenCSContentWatchList(core, ListName, SortField, true);
                }
                //
                if (this.core.db.csOk(CS)) {
                    ContentID = Models.Complex.cdefModel.getContentId(core, "Content Watch");
                    while (this.core.db.csOk(CS)) {
                        Link = this.core.db.csGetText(CS, "link");
                        LinkLabel = this.core.db.csGetText(CS, "LinkLabel");
                        RecordID = this.core.db.csGetInteger(CS, "ID");
                        if (!string.IsNullOrEmpty(LinkLabel)) {
                            result = result + "\r<li id=\"main_ContentWatch" + RecordID + "\" class=\"ccListItem\">";
                            if (!string.IsNullOrEmpty(Link)) {
                                result = result + "<a href=\"http://" + this.core.webServer.requestDomain + requestAppRootPath + this.core.webServer.requestPage + "?rc=" + ContentID + "&ri=" + RecordID + "\">" + LinkLabel + "</a>";
                            } else {
                                result = result + LinkLabel;
                            }
                            result = result + "</li>";
                        }
                        this.core.db.csGoNext(CS);
                    }
                    if (!string.IsNullOrEmpty(result)) {
                        result = this.core.html.getContentCopy("Watch List Caption: " + ListName, ListName, this.core.sessionContext.user.id, true, this.core.sessionContext.isAuthenticated) + "\r<ul class=\"ccWatchList\">" + htmlIndent(result) + "\r</ul>";
                    }
                }
                this.core.db.csClose(ref CS);
                //
                if (this.core.visitProperty.getBoolean("AllowAdvancedEditor")) {
                    result = this.core.html.getEditWrapper("Watch List [" + ListName + "]", result);
                }
            } catch (Exception ex) {
                this.core.handleException(ex);
            }
            return result;
        }
        //
        //==========================================================================
        //   returns the site structure xml
        //==========================================================================
        //
        //todo  NOTE: C# does not support parameterized properties - the following property has been rewritten as a function:
        //ORIGINAL LINE: Public ReadOnly Property main_SiteStructure(core As coreClass) As String
        public string get_main_SiteStructure(coreController core) {
            if (!siteStructure_LocalLoaded) {
                addonModel addon = addonModel.create(core, addonGuidSiteStructureGuid);
                siteStructure = this.core.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext() { addonType = CPUtilsBaseClass.addonContext.ContextSimple });
                //siteStructure = Me.core.addon.execute_legacy2(0, addonGuidSiteStructureGuid, "", CPUtilsBaseClass.addonContext.ContextSimple, "", 0, "", "", False, -1, "", returnStatus, Nothing)
                siteStructure_LocalLoaded = true;
            }
            return siteStructure;

        }
        //
        //=============================================================================
        //   Content Page Authoring
        //
        //   Display Quick Editor for the first active record found
        //   Use for both Root and non-root pages
        //=============================================================================
        //
        internal string getQuickEditing(int rootPageId, string OrderByClause, bool AllowPageList, bool AllowReturnLink, bool ArchivePages, int contactMemberID, int childListSortMethodId, bool main_AllowChildListComposite, bool ArchivePage) {
            string result = "";
            try {
                string RootPageContentName = pageContentModel.contentName;
                string LiveRecordContentName = pageContentModel.contentName;
                string Link = null;
                int page_ParentID = 0;
                string PageList = null;
                string OptionsPanelAuthoringStatus = null;
                string ButtonList = null;
                bool AllowInsert = false;
                bool AllowCancel = false;
                bool allowSave = false;
                bool AllowDelete = false;
                bool AllowMarkReviewed = false;
                Models.Complex.cdefModel CDef = null;
                bool readOnlyField = false;
                bool IsEditLocked = false;
                string main_EditLockMemberName = "";
                DateTime main_EditLockDateExpires = default(DateTime);
                DateTime SubmittedDate = default(DateTime);
                DateTime ApprovedDate = default(DateTime);
                DateTime ModifiedDate = default(DateTime);
                //
                core.html.addStyleLink("/quickEditor/styles.css", "Quick Editor");
                //
                // ----- First Active Record - Output Quick Editor form
                //
                CDef = Models.Complex.cdefModel.getCdef(core, LiveRecordContentName);
                //
                // main_Get Authoring Status and permissions
                //
                IsEditLocked = core.workflow.GetEditLockStatus(LiveRecordContentName, page.id);
                if (IsEditLocked) {
                    main_EditLockMemberName = core.workflow.GetEditLockMemberName(LiveRecordContentName, page.id);
                    main_EditLockDateExpires = genericController.encodeDate(core.workflow.GetEditLockMemberName(LiveRecordContentName, page.id));
                }
                bool IsModified = false;
                bool IsSubmitted = false;
                bool IsApproved = false;
                string SubmittedMemberName = "";
                string ApprovedMemberName = "";
                string ModifiedMemberName = "";
                bool IsDeleted = false;
                bool IsInserted = false;
                bool IsRootPage = false;
                getAuthoringStatus(LiveRecordContentName, page.id, ref IsSubmitted, ref IsApproved, ref SubmittedMemberName, ref ApprovedMemberName, ref IsInserted, ref IsDeleted, ref IsModified, ref ModifiedMemberName, ref ModifiedDate, ref SubmittedDate, ref ApprovedDate);
                bool tempVar = false;
                bool tempVar2 = false;
                bool tempVar3 = false;
                bool tempVar4 = false;
                getAuthoringPermissions(LiveRecordContentName, page.id, ref AllowInsert, ref AllowCancel, ref allowSave, ref AllowDelete, ref tempVar, ref tempVar2, ref tempVar3, ref tempVar4, ref readOnlyField);
                AllowMarkReviewed = Models.Complex.cdefModel.isContentFieldSupported(core, pageContentModel.contentName, "DateReviewed");
                OptionsPanelAuthoringStatus = core.sessionContext.getAuthoringStatusMessage(core, false, IsEditLocked, main_EditLockMemberName, main_EditLockDateExpires, IsApproved, ApprovedMemberName, IsSubmitted, SubmittedMemberName, IsDeleted, IsInserted, IsModified, ModifiedMemberName);
                //
                // Set Editing Authoring Control
                //
                core.workflow.SetEditLock(LiveRecordContentName, page.id);
                //
                // SubPanel: Authoring Status
                //
                ButtonList = "";
                if (AllowCancel) {
                    ButtonList = ButtonList + "," + ButtonCancel;
                }
                if (allowSave) {
                    ButtonList = ButtonList + "," + ButtonSave + "," + ButtonOK;
                }
                if (AllowDelete && !IsRootPage) {
                    ButtonList = ButtonList + "," + ButtonDelete;
                }
                if (AllowInsert) {
                    ButtonList = ButtonList + "," + ButtonAddChildPage;
                }
                if ((page_ParentID != 0) && AllowInsert) {
                    ButtonList = ButtonList + "," + ButtonAddSiblingPage;
                }
                if (AllowMarkReviewed) {
                    ButtonList = ButtonList + "," + ButtonMarkReviewed;
                }
                if (!string.IsNullOrEmpty(ButtonList)) {
                    ButtonList = ButtonList.Substring(1);
                    ButtonList = core.html.getPanelButtons(ButtonList, "Button");
                }
                //If OptionsPanelAuthoringStatus <> "" Then
                //    result = result & "" _
                //        & cr & "<tr>" _
                //        & cr2 & "<td colspan=2 class=""qeRow""><div class=""qeHeadCon"">" & OptionsPanelAuthoringStatus & "</div></td>" _
                //        & cr & "</tr>"
                //End If
                if (core.doc.debug_iUserError != "") {
                    result = result + ""
                        + "\r<tr>"
                        + cr2 + "<td colspan=2 class=\"qeRow\"><div class=\"qeHeadCon\">" + errorController.getUserError(core) + "</div></td>"
                        + "\r</tr>";
                }
                if (readOnlyField) {
                    result = result + ""
                    + "\r<tr>"
                    + cr2 + "<td colspan=\"2\" class=\"qeRow\">" + getQuickEditingBody(LiveRecordContentName, OrderByClause, AllowPageList, true, rootPageId, readOnlyField, AllowReturnLink, RootPageContentName, ArchivePages, contactMemberID) + "</td>"
                    + "\r</tr>";
                } else {
                    result = result + ""
                    + "\r<tr>"
                    + cr2 + "<td colspan=\"2\" class=\"qeRow\">" + getQuickEditingBody(LiveRecordContentName, OrderByClause, AllowPageList, true, rootPageId, readOnlyField, AllowReturnLink, RootPageContentName, ArchivePages, contactMemberID) + "</td>"
                    + "\r</tr>";
                }
                result = result + "\r<tr>"
                    + cr2 + "<td class=\"qeRow qeLeft\" style=\"padding-top:10px;\">Name</td>"
                    + cr2 + "<td class=\"qeRow qeRight\">" + core.html.inputText("name", page.name, 1, 0, "", false, readOnlyField) + "</td>"
                    + "\r</tr>"
                    + "";
                //
                // ----- Parent pages
                //
                if (pageToRootList.Count == 1) {
                    PageList = "&nbsp;(there are no parent pages)";
                } else {
                    PageList = "<ul class=\"qeListUL\"><li class=\"qeListLI\">Current Page</li></ul>";
                    foreach (pageContentModel testPage in Enumerable.Reverse(pageToRootList)) {
                        Link = testPage.name;
                        if (string.IsNullOrEmpty(Link)) {
                            Link = "no name #" + genericController.encodeText(testPage.id);
                        }
                        Link = "<a href=\"" + testPage.PageLink + "\">" + Link + "</a>";
                        PageList = "<ul class=\"qeListUL\"><li class=\"qeListLI\">" + Link + PageList + "</li></ul>";
                    }
                }
                result = result + ""
                + "\r<tr>"
                + cr2 + "<td class=\"qeRow qeLeft\" style=\"padding-top:26px;\">Parent Pages</td>"
                + cr2 + "<td class=\"qeRow qeRight\"><div class=\"qeListCon\">" + PageList + "</div></td>"
                + "\r</tr>";
                //
                // ----- Child pages
                //
                addonModel addon = addonModel.create(core, core.siteProperties.childListAddonID);
                CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext {
                    addonType = CPUtilsBaseClass.addonContext.ContextPage,
                    hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                        contentName = pageContentModel.contentName,
                        fieldName = "",
                        recordId = page.id
                    },
                    instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(core, page.ChildListInstanceOptions),
                    instanceGuid = PageChildListInstanceID
                };
                PageList = core.addon.execute(addon, executeContext);
                //PageList = core.addon.execute_legacy2(core.siteProperties.childListAddonID, "", page.ChildListInstanceOptions, CPUtilsBaseClass.addonContext.ContextPage, pageContentModel.contentName, page.id, "", PageChildListInstanceID, False, -1, "", AddonStatusOK, Nothing)
                if (genericController.vbInstr(1, PageList, "<ul", 1) == 0) {
                    PageList = "(there are no child pages)";
                }
                result = result + "\r<tr>"
                    + cr2 + "<td class=\"qeRow qeLeft\" style=\"padding-top:36px;\">Child Pages</td>"
                    + cr2 + "<td class=\"qeRow qeRight\"><div class=\"qeListCon\">" + PageList + "</div></td>"
                    + "\r</tr>";
                result = ""
                    + "\r<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">"
                    + genericController.htmlIndent(result) + "\r</table>";
                result = ""
                    + ButtonList + result + ButtonList;
                result = core.html.getPanel(result);
                //
                // Form Wrapper
                //
                result = ""
                    + '\r' + core.html.formStartMultipart(core.webServer.requestQueryString) + '\r' + core.html.inputHidden("Type", FormTypePageAuthoring) + '\r' + core.html.inputHidden("ID", page.id) + '\r' + core.html.inputHidden("ContentName", LiveRecordContentName) + '\r' + result + "\r" + core.html.formEnd();

                //& cr & core.html.main_GetPanelHeader("Contensive Quick Editor") _

                result = ""
                    + "\r<div class=\"ccCon\">"
                    + genericController.htmlIndent(result) + "\r</div>";
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        internal string getQuickEditingBody(string ContentName, string OrderByClause, bool AllowChildList, bool Authoring, int rootPageId, bool readOnlyField, bool AllowReturnLink, string RootPageContentName, bool ArchivePage, int contactMemberID) {
            string pageCopy = page.Copyfilename.content;
            //If page.Copyfilename <> "" Then
            //    pageCopy = page.Copyfilename.copy(core)
            //    'pageCopy = core.cdnFiles.readFile(page.Copyfilename)
            //End If
            //
            // ----- Page Copy
            //
            int FieldRows = core.userProperty.getInteger(ContentName + ".copyFilename.PixelHeight", 500);
            if (FieldRows < 50) {
                FieldRows = 50;
                core.userProperty.setProperty(ContentName + ".copyFilename.PixelHeight", FieldRows);
            }
            //
            // At this point we do now know the the template so we can not main_Get the stylelist.
            // Put in main_fpo_QuickEditing to be replaced after template known
            //
            quickEditCopy = pageCopy;
            return html_quickEdit_fpo;
        }
        //
        //=============================================================================
        //
        internal string getReturnBreadcrumb(string RootPageContentName, int ignore, int rootPageId, string ParentIDPath, bool ArchivePage, string BreadCrumbDelimiter) {
            string returnHtml = "";
            //
            foreach (pageContentModel testpage in pageToRootList) {
                string pageCaption = testpage.MenuHeadline;
                if (string.IsNullOrEmpty(pageCaption)) {
                    pageCaption = genericController.encodeText(testpage.name);
                }
                if (string.IsNullOrEmpty(returnHtml)) {
                    returnHtml = pageCaption;
                } else {
                    returnHtml = "<a href=\"" + genericController.encodeHTML(pageContentController.getPageLink(core, testpage.id, "", true, false)) + "\">" + pageCaption + "</a>" + BreadCrumbDelimiter + returnHtml;
                }
            }
            return returnHtml;
        }
        //
        //========================================================================
        // ----- Process the reply from the Authoring Tools Panel form
        //========================================================================
        //
        public void processFormQuickEditing() {
            //
            int RecordParentID = 0;
            bool SaveButNoChanges = false;
            int ParentID = 0;
            string Link = null;
            int CSBlock = 0;
            string Copy = null;
            string Button = null;
            int RecordID = 0;
            string RecordName = "";
            bool IsEditLocked = false;
            bool IsSubmitted = false;
            bool IsApproved = false;
            bool IsInserted = false;
            bool IsDeleted = false;
            bool IsModified = false;
            string main_EditLockMemberName = null;
            DateTime main_EditLockDateExpires = default(DateTime);
            DateTime ModifiedDate = default(DateTime);
            DateTime SubmittedDate = default(DateTime);
            DateTime ApprovedDate = default(DateTime);
            bool allowSave = false;
            bool iIsAdmin = false;
            // Dim main_WorkflowSupport As Boolean
            //
            RecordID = (core.docProperties.getInteger("ID"));
            Button = core.docProperties.getText("Button");
            iIsAdmin = core.sessionContext.isAuthenticatedAdmin(core);
            //
            if ((!string.IsNullOrEmpty(Button)) & (RecordID != 0) & (pageContentModel.contentName != "") & (core.sessionContext.isAuthenticatedContentManager(core, pageContentModel.contentName))) {
                // main_WorkflowSupport = core.siteProperties.allowWorkflowAuthoring And core.workflow.isWorkflowAuthoringCompatible(pageContentModel.contentName)
                string SubmittedMemberName = "";
                string ApprovedMemberName = "";
                string ModifiedMemberName = "";
                getAuthoringStatus(pageContentModel.contentName, RecordID, ref IsSubmitted, ref IsApproved, ref SubmittedMemberName, ref ApprovedMemberName, ref IsInserted, ref IsDeleted, ref IsModified, ref ModifiedMemberName, ref ModifiedDate, ref SubmittedDate, ref ApprovedDate);
                IsEditLocked = core.workflow.GetEditLockStatus(pageContentModel.contentName, RecordID);
                main_EditLockMemberName = core.workflow.GetEditLockMemberName(pageContentModel.contentName, RecordID);
                main_EditLockDateExpires = core.workflow.GetEditLockDateExpires(pageContentModel.contentName, RecordID);
                core.workflow.ClearEditLock(pageContentModel.contentName, RecordID);
                //
                // tough case, in Quick mode, lets mark the record reviewed, no matter what button they push, except cancel
                //
                if (Button != ButtonCancel) {
                    core.doc.markRecordReviewed(pageContentModel.contentName, RecordID);
                }
                //
                // Determine is the record should be saved
                //
                if ((!IsApproved) && (!core.docProperties.getBoolean("RENDERMODE"))) {
                    if (iIsAdmin) {
                        //
                        // cases that admin can save
                        //
                        allowSave = false || (Button == ButtonAddChildPage) || (Button == ButtonAddSiblingPage) || (Button == ButtonSave) || (Button == ButtonOK);
                    } else {
                        //
                        // cases that CM can save
                        //
                        allowSave = false || (Button == ButtonAddChildPage) || (Button == ButtonAddSiblingPage) || (Button == ButtonSave) || (Button == ButtonOK);
                    }
                }
                if (allowSave) {
                    //
                    // ----- Save Changes
                    SaveButNoChanges = true;
                    pageContentModel page = pageContentModel.create(core, RecordID);
                    if (page != null) {
                        Copy = core.docProperties.getText("copyFilename");
                        Copy = activeContentController.processWysiwygResponseForSave(core, Copy);
                        if (Copy != page.Copyfilename.content) {
                            page.Copyfilename.content = Copy;
                            SaveButNoChanges = false;
                        }
                        RecordName = core.docProperties.getText("name");
                        if (RecordName != page.name) {
                            page.name = RecordName;
                            linkAliasController.addLinkAlias(core, RecordName, RecordID, "");
                            SaveButNoChanges = false;
                        }
                        RecordParentID = page.ParentID;
                        page.save(core);
                        //
                        core.workflow.SetEditLock(pageContentModel.contentName, page.id);
                        //
                        if (!SaveButNoChanges) {
                            core.doc.processAfterSave(false, pageContentModel.contentName, page.id, page.name, page.ParentID, false);
                            core.cache.invalidateAllInContent(pageContentModel.contentName);
                        }
                    }
                }
                if (Button == ButtonAddChildPage) {
                    //
                    //
                    //
                    CSBlock = core.db.csInsertRecord(pageContentModel.contentName);
                    if (core.db.csOk(CSBlock)) {
                        core.db.csSet(CSBlock, "active", true);
                        core.db.csSet(CSBlock, "ParentID", RecordID);
                        core.db.csSet(CSBlock, "contactmemberid", core.sessionContext.user.id);
                        core.db.csSet(CSBlock, "name", "New Page added " + core.doc.profileStartTime + " by " + core.sessionContext.user.name);
                        core.db.csSet(CSBlock, "copyFilename", "");
                        RecordID = core.db.csGetInteger(CSBlock, "ID");
                        core.db.csSave(CSBlock);
                        //
                        Link = pageContentController.getPageLink(core, RecordID, "", true, false);
                        //Link = main_GetPageLink(RecordID)
                        //If main_WorkflowSupport Then
                        //    If Not core.doc.authContext.isWorkflowRendering() Then
                        //        Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "This new unpublished page has been added and Workflow Rendering has been enabled so you can edit this page.", True)
                        //        Call core.siteProperties.setProperty("AllowWorkflowRendering", True)
                        //    End If
                        //End If
                        core.webServer.redirect(Link, "Redirecting because a new page has been added with the quick editor.", false, false);
                    }
                    core.db.csClose(ref CSBlock);
                    //
                    core.cache.invalidateAllInContent(pageContentModel.contentName);
                }
                if (Button == ButtonAddSiblingPage) {
                    //
                    //
                    //
                    CSBlock = core.db.csOpenRecord(pageContentModel.contentName, RecordID, false, false, "ParentID");
                    if (core.db.csOk(CSBlock)) {
                        ParentID = core.db.csGetInteger(CSBlock, "ParentID");
                    }
                    core.db.csClose(ref CSBlock);
                    if (ParentID != 0) {
                        CSBlock = core.db.csInsertRecord(pageContentModel.contentName);
                        if (core.db.csOk(CSBlock)) {
                            core.db.csSet(CSBlock, "active", true);
                            core.db.csSet(CSBlock, "ParentID", ParentID);
                            core.db.csSet(CSBlock, "contactmemberid", core.sessionContext.user.id);
                            core.db.csSet(CSBlock, "name", "New Page added " + core.doc.profileStartTime + " by " + core.sessionContext.user.name);
                            core.db.csSet(CSBlock, "copyFilename", "");
                            RecordID = core.db.csGetInteger(CSBlock, "ID");
                            core.db.csSave(CSBlock);
                            //
                            Link = pageContentController.getPageLink(core, RecordID, "", true, false);
                            core.webServer.redirect(Link, "Redirecting because a new page has been added with the quick editor.", false, false);
                        }
                        core.db.csClose(ref CSBlock);
                    }
                    core.cache.invalidateAllInContent(pageContentModel.contentName);
                }
                if (Button == ButtonDelete) {
                    CSBlock = core.db.csOpenRecord(pageContentModel.contentName, RecordID);
                    if (core.db.csOk(CSBlock)) {
                        ParentID = core.db.csGetInteger(CSBlock, "parentid");
                    }
                    core.db.csClose(ref CSBlock);
                    //
                    deleteChildRecords(pageContentModel.contentName, RecordID, false);
                    core.db.deleteContentRecord(pageContentModel.contentName, RecordID);
                    //
                    if (!false) {
                        core.cache.invalidateAllInContent(pageContentModel.contentName);
                    }
                    //
                    if (!false) {
                        Link = pageContentController.getPageLink(core, ParentID, "", true, false);
                        Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "The page has been deleted, and you have been redirected to the parent of the deleted page.", true);
                        core.webServer.redirect(Link, "Redirecting to the parent page because the page was deleted with the quick editor.", redirectBecausePageNotFound, false);
                        return;
                    }
                }
                //
                //If (Button = ButtonAbortEdit) Then
                //    Call core.workflow.abortEdit2(pageContentModel.contentName, RecordID, core.doc.authContext.user.id)
                //End If
                //If (Button = ButtonPublishSubmit) Then
                //    Call core.workflow.main_SubmitEdit(pageContentModel.contentName, RecordID)
                //    Call sendPublishSubmitNotice(pageContentModel.contentName, RecordID, "")
                //End If
                if ((!(core.doc.debug_iUserError != "")) & ((Button == ButtonOK) || (Button == ButtonCancel))) {
                    //
                    // ----- Turn off Quick Editor if not save or add child
                    //
                    core.visitProperty.setProperty("AllowQuickEditor", "0");
                }
            }
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
        public string getChildPageList(string RequestedListName, string ContentName, int parentPageID, bool allowChildListDisplay, bool ArchivePages = false) {
            string result = "";
            try {
                if (string.IsNullOrEmpty(ContentName)) {
                    ContentName = pageContentModel.contentName;
                }
                bool isAuthoring = core.sessionContext.isEditing(ContentName);
                //
                int ChildListCount = 0;
                string UcaseRequestedListName = genericController.vbUCase(RequestedListName);
                if ((UcaseRequestedListName == "NONE") || (UcaseRequestedListName == "ORPHAN")) {
                    UcaseRequestedListName = "";
                }
                //
                string archiveLink = core.webServer.requestPathPage;
                archiveLink = genericController.ConvertLinkToShortLink(archiveLink, core.webServer.requestDomain, core.webServer.requestVirtualFilePath);
                archiveLink = genericController.EncodeAppRootPath(archiveLink, core.webServer.requestVirtualFilePath, requestAppRootPath, core.webServer.requestDomain);
                //
                string sqlCriteria = "(parentId=" + page.id + ")";
                string sqlOrderBy = "sortOrder";
                List<pageContentModel> childPageList = pageContentModel.createList(core, sqlCriteria, sqlOrderBy);
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
                    if (core.sessionContext.isEditing(ContentName)) {
                        pageEditLink = core.html.getRecordEditLink2(ContentName, childPage.id, true, childPage.name, true);
                    }
                    //
                    string link = PageLink;
                    if (ArchivePages) {
                        link = genericController.modifyLinkQuery(archiveLink, rnPageId, encodeText(childPage.id), true);
                    }
                    bool blockContentComposite = false;
                    if (childPage.BlockContent | childPage.BlockPage) {
                        blockContentComposite = !bypassContentBlock(childPage.contentControlID, childPage.id);
                    }
                    string LinkedText = genericController.csv_GetLinkedText("<a href=\"" + genericController.encodeHTML(link) + "\">", pageMenuHeadline);
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
                    string AddLink = core.html.getRecordAddLink(ContentName, "parentid=" + parentPageID + ",ParentListName=" + UcaseRequestedListName, true);
                    if (!string.IsNullOrEmpty(AddLink)) {
                        inactiveList = inactiveList + "\r<li class=\"ccListItem\">" + AddLink + "</LI>";
                    }
                }
                //
                // ----- If there is a list, add the list start and list end
                //
                result = "";
                if (!string.IsNullOrEmpty(activeList)) {
                    result = result + "\r<ul id=\"childPageList_" + parentPageID + "_" + RequestedListName + "\" class=\"ccChildList\">" + genericController.htmlIndent(activeList) + "\r</ul>";
                }
                if (!string.IsNullOrEmpty(inactiveList)) {
                    result = result + "\r<ul id=\"childPageList_" + parentPageID + "_" + RequestedListName + "\" class=\"ccChildListInactive\">" + genericController.htmlIndent(inactiveList) + "\r</ul>";
                }
                //
                // ----- if non-orphan list, authoring and none found, print none message
                //
                if ((!string.IsNullOrEmpty(UcaseRequestedListName)) & (ChildListCount == 0) & isAuthoring) {
                    result = "[Child Page List with no pages]</p><p>" + result;
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   BypassContentBlock
        //       Is This member allowed through the content block
        //=============================================================================
        //
        public bool bypassContentBlock(int ContentID, int RecordID) {
            bool tempbypassContentBlock = false;
            try {
                //
                int CS = 0;
                string SQL = null;
                //
                if (core.sessionContext.isAuthenticatedAdmin(core)) {
                    tempbypassContentBlock = true;
                } else if (core.sessionContext.isAuthenticatedContentManager(core, Models.Complex.cdefModel.getContentNameByID(core, ContentID))) {
                    tempbypassContentBlock = true;
                } else {
                    SQL = "SELECT ccMemberRules.MemberID"
                        + " FROM (ccPageContentBlockRules LEFT JOIN ccgroups ON ccPageContentBlockRules.GroupID = ccgroups.ID) LEFT JOIN ccMemberRules ON ccgroups.ID = ccMemberRules.GroupID"
                        + " WHERE (((ccPageContentBlockRules.RecordID)=" + RecordID + ")"
                        + " AND ((ccPageContentBlockRules.Active)<>0)"
                        + " AND ((ccgroups.Active)<>0)"
                        + " AND ((ccMemberRules.Active)<>0)"
                        + " AND ((ccMemberRules.DateExpires) Is Null Or (ccMemberRules.DateExpires)>" + core.db.encodeSQLDate(core.doc.profileStartTime) + ")"
                        + " AND ((ccMemberRules.MemberID)=" + core.sessionContext.user.id + "));";
                    CS = core.db.csOpenSql(SQL);
                    tempbypassContentBlock = core.db.csOk(CS);
                    core.db.csClose(ref CS);
                }
                //
                return tempbypassContentBlock;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            ////throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError13("IsContentBlocked")
            return tempbypassContentBlock;
        }
        //
        //====================================================================================================
        //   main_GetTemplateLink
        //       Added to externals (aoDynamicMenu) can main_Get hard template links
        //====================================================================================================
        //
        //Public Function getTemplateLink(templateId As Integer) As String
        //    Dim template As pageTemplateModel = pageTemplateModel.create(core, templateId, New List(Of String))
        //    If template IsNot Nothing Then
        //        Return template.Link
        //    End If
        //    Return ""
        //End Function
        //
        //========================================================================
        // main_DeleteChildRecords
        //========================================================================
        //
        public string deleteChildRecords(string ContentName, int RecordID, bool ReturnListWithoutDelete = false) {
            string result = "";
            try {
                bool QuickEditing = false;
                string[] IDs = null;
                int IDCnt = 0;
                int Ptr = 0;
                int CS = 0;
                string ChildList = null;
                bool SingleEntry = false;
                //
                // For now, the child delete only works in non-workflow
                //
                CS = core.db.csOpen(ContentName, "parentid=" + RecordID, "", false, 0, false, false, "ID");
                while (core.db.csOk(CS)) {
                    result = result + "," + core.db.csGetInteger(CS, "ID");
                    core.db.csGoNext(CS);
                }
                core.db.csClose(ref CS);
                if (!string.IsNullOrEmpty(result)) {
                    result = result.Substring(1);
                    //
                    // main_Get a list of all pages, but do not delete anything yet
                    //
                    IDs = result.Split(',');
                    IDCnt = IDs.GetUpperBound(0) + 1;
                    SingleEntry = (IDCnt == 1);
                    for (Ptr = 0; Ptr < IDCnt; Ptr++) {
                        ChildList = deleteChildRecords(ContentName, genericController.encodeInteger(IDs[Ptr]), true);
                        if (!string.IsNullOrEmpty(ChildList)) {
                            result = result + "," + ChildList;
                            SingleEntry = false;
                        }
                    }
                    if (!ReturnListWithoutDelete) {
                        //
                        // Do the actual delete
                        //
                        IDs = result.Split(',');
                        IDCnt = IDs.GetUpperBound(0) + 1;
                        SingleEntry = (IDCnt == 1);
                        QuickEditing = core.sessionContext.isQuickEditing(core, "page content");
                        for (Ptr = 0; Ptr < IDCnt; Ptr++) {
                            core.db.deleteContentRecord("page content", genericController.encodeInteger(IDs[Ptr]));
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        public void getAuthoringStatus(string ContentName, int RecordID, ref bool IsSubmitted, ref bool IsApproved, ref string SubmittedName, ref string ApprovedName, ref bool IsInserted, ref bool IsDeleted, ref bool IsModified, ref string ModifiedName, ref DateTime ModifiedDate, ref DateTime SubmittedDate, ref DateTime ApprovedDate) {
            core.workflow.getAuthoringStatus(ContentName, RecordID, ref IsSubmitted, ref IsApproved, ref SubmittedName, ref ApprovedName, ref IsInserted, ref IsDeleted, ref IsModified, ref ModifiedName, ref ModifiedDate, ref SubmittedDate, ref ApprovedDate);
        }
        //
        //========================================================================
        //   main_Get athoring permissions to determine what buttons we display, and what authoring actions we can take
        //
        //       RecordID = 0 means it is an unsaved inserted record, or this pertains to the content, not a record
        //
        //       AllowCancel - OK to exit without any action
        //       AllowInsert - OK to create new records, display ADD button
        //       AllowSave - OK to save the record, display the Save and OK Buttons
        //       etc.
        //========================================================================
        //
        public void getAuthoringPermissions(string ContentName, int RecordID, ref bool AllowInsert, ref bool AllowCancel, ref bool allowSave, ref bool AllowDelete, ref bool ignore1, ref bool ignore2, ref bool ignore3, ref bool ignore4, ref bool readOnlyField) {
            try {
                bool IsEditing = false;
                bool IsSubmitted = false;
                bool IsApproved = false;
                bool IsInserted = false;
                bool IsDeleted = false;
                bool IsModified = false;
                string SubmittedName = "";
                string ApprovedName = "";
                string ModifiedName = "";
                Models.Complex.cdefModel CDef = null;
                DateTime ModifiedDate = default(DateTime);
                DateTime SubmittedDate = default(DateTime);
                DateTime ApprovedDate = default(DateTime);
                //
                if (RecordID != 0) {
                    core.workflow.getAuthoringStatus(ContentName, RecordID, ref IsSubmitted, ref IsApproved, ref SubmittedName, ref ApprovedName, ref IsInserted, ref IsDeleted, ref IsModified, ref ModifiedName, ref ModifiedDate, ref SubmittedDate, ref ApprovedDate);
                }
                //
                CDef = Models.Complex.cdefModel.getCdef(core, ContentName);
                //
                // Set Buttons based on Status
                //
                if (IsEditing) {
                    //
                    // Edit Locked
                    //
                    AllowCancel = true;
                    readOnlyField = true;
                } else if ((!false) || (!false)) {
                    //
                    // No Workflow Authoring
                    //
                    AllowCancel = true;
                    allowSave = true;
                    if ((CDef.allowDelete) & (!IsDeleted) & (RecordID != 0)) {
                        AllowDelete = true;
                    }
                    if ((CDef.allowAdd) && (!IsInserted)) {
                        AllowInsert = true;
                    }
                    //ElseIf core.doc.authContext.isAuthenticatedAdmin(core) Then
                    //    '
                    //    ' Workflow, Admin
                    //    '
                    //    If IsApproved Then
                    //        '
                    //        ' Workflow, Admin, Approved
                    //        '
                    //        AllowCancel = True
                    //        ignore1 = True
                    //        ignore2 = True
                    //        readOnlyField = True
                    //        AllowInsert = True
                    //    ElseIf IsSubmitted Then
                    //        '
                    //        ' Workflow, Admin, Submitted (not approved)
                    //        '
                    //        AllowCancel = True
                    //        If Not IsDeleted Then
                    //            allowSave = True
                    //        End If
                    //        ignore1 = True
                    //        ignore2 = True
                    //        ignore4 = True
                    //        If IsDeleted Then
                    //            readOnlyField = True
                    //        End If
                    //        AllowInsert = True
                    //    ElseIf IsInserted Then
                    //        '
                    //        ' Workflow, Admin, Inserted (not submitted, not approved)
                    //        '
                    //        AllowCancel = True
                    //        allowSave = True
                    //        ignore1 = True
                    //        ignore2 = True
                    //        ignore3 = True
                    //        ignore4 = True
                    //        AllowInsert = True
                    //    ElseIf IsDeleted Then
                    //        '
                    //        ' Workflow, Admin, Deleted record (not submitted, not approved)
                    //        '
                    //        AllowCancel = True
                    //        ignore1 = True
                    //        ignore2 = True
                    //        ignore3 = True
                    //        ignore4 = True
                    //        readOnlyField = True
                    //        AllowInsert = True
                    //    ElseIf IsModified Then
                    //        '
                    //        ' Workflow, Admin, Modified (not submitted, not approved, not inserted, not deleted)
                    //        '
                    //        AllowCancel = True
                    //        allowSave = True
                    //        ignore1 = True
                    //        ignore2 = True
                    //        ignore3 = True
                    //        ignore4 = True
                    //        AllowDelete = True
                    //        AllowInsert = True
                    //    Else
                    //        '
                    //        ' Workflow, Admin, Not Modified (not submitted, not approved, not inserted, not deleted)
                    //        '
                    //        AllowCancel = True
                    //        allowSave = True
                    //        ignore1 = True
                    //        ignore4 = True
                    //        ignore3 = True
                    //        AllowDelete = True
                    //        AllowInsert = True
                    //    End If
                    //Else
                    //    '
                    //    ' Workflow, Content Manager
                    //    '
                    //    If IsApproved Then
                    //        '
                    //        ' Workflow, Content Manager, Approved
                    //        '
                    //        AllowCancel = True
                    //        readOnlyField = True
                    //        AllowInsert = True
                    //    ElseIf IsSubmitted Then
                    //        '
                    //        ' Workflow, Content Manager, Submitted (not approved)
                    //        '
                    //        AllowCancel = True
                    //        readOnlyField = True
                    //        AllowInsert = True
                    //    ElseIf IsInserted Then
                    //        '
                    //        ' Workflow, Content Manager, Inserted (not submitted, not approved)
                    //        '
                    //        AllowCancel = True
                    //        allowSave = True
                    //        ignore2 = True
                    //        ignore3 = True
                    //        AllowInsert = True
                    //    ElseIf IsDeleted Then
                    //        '
                    //        ' Workflow, Content Manager, Deleted record (not submitted, not approved)
                    //        '
                    //        AllowCancel = True
                    //        ignore2 = True
                    //        ignore3 = True
                    //        readOnlyField = True
                    //        AllowInsert = True
                    //    ElseIf IsModified Then
                    //        '
                    //        ' Workflow, Content Manager, Modified (not submitted, not approved, not inserted, not deleted)
                    //        '
                    //        AllowCancel = True
                    //        allowSave = True
                    //        AllowDelete = True
                    //        ignore2 = True
                    //        ignore3 = True
                    //        AllowInsert = True
                    //    Else
                    //        '
                    //        ' Workflow, Content Manager, Not Modified (not submitted, not approved, not inserted, not deleted)
                    //        '
                    //        AllowCancel = True
                    //        allowSave = True
                    //        AllowDelete = True
                    //        ignore2 = True
                    //        ignore3 = True
                    //        AllowInsert = True
                    //    End If
                }
                //
                return;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            ////throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18(MethodName)
                                                                    //
        }
        //
        //
        //
        public void sendPublishSubmitNotice(string ContentName, int RecordID, string RecordName) {
            try {
                Models.Complex.cdefModel CDef = null;
                string Copy = null;
                string Link = null;
                string FromAddress = null;
                //
                FromAddress = core.siteProperties.getText("EmailPublishSubmitFrom", core.siteProperties.emailAdmin);
                CDef = Models.Complex.cdefModel.getCdef(core, ContentName);
                Link = "/" + core.appConfig.adminRoute + "?af=" + AdminFormPublishing;
                Copy = Msg_AuthoringSubmittedNotification;
                Copy = genericController.vbReplace(Copy, "<DOMAINNAME>", "<a href=\"" + genericController.encodeHTML(Link) + "\">" + core.webServer.requestDomain + "</a>");
                Copy = genericController.vbReplace(Copy, "<RECORDNAME>", RecordName);
                Copy = genericController.vbReplace(Copy, "<CONTENTNAME>", ContentName);
                Copy = genericController.vbReplace(Copy, "<RECORDID>", RecordID.ToString());
                Copy = genericController.vbReplace(Copy, "<SUBMITTEDDATE>", core.doc.profileStartTime.ToString());
                Copy = genericController.vbReplace(Copy, "<SUBMITTEDNAME>", core.sessionContext.user.name);
                //
                emailController.sendGroup(core, core.siteProperties.getText("WorkflowEditorGroup", "Site Managers"), FromAddress, "Authoring Submitted Notification", Copy, false, true);
                //
                return;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            ////throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18(MethodName)
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
                string ContentRecordKey = Models.Complex.cdefModel.getContentId(core, genericController.encodeText(ContentName)) + "." + genericController.encodeInteger(RecordID);
                result = getContentWatchLinkByKey(ContentRecordKey, DefaultLink, IncrementClicks);
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   main_Get the link for a Content Record by its ContentRecordKey
        //=============================================================================
        //
        public string getContentWatchLinkByKey(string ContentRecordKey, string DefaultLink = "", bool IncrementClicks = false) {
            string tempgetContentWatchLinkByKey = null;
            try {
                //
                //If Not (true) Then Exit Function
                //
                int CSPointer;
                //
                // Lookup link in main_ContentWatch
                //
                CSPointer = core.db.csOpen("Content Watch", "ContentRecordKey=" + core.db.encodeSQLText(ContentRecordKey), "", false, 0, false, false, "Link,Clicks");
                if (core.db.csOk(CSPointer)) {
                    tempgetContentWatchLinkByKey = core.db.csGetText(CSPointer, "Link");
                    if (genericController.encodeBoolean(IncrementClicks)) {
                        core.db.csSet(CSPointer, "Clicks", core.db.csGetInteger(CSPointer, "clicks") + 1);
                    }
                } else {
                    tempgetContentWatchLinkByKey = genericController.encodeText(DefaultLink);
                }
                core.db.csClose(ref CSPointer);
                //
                return genericController.EncodeAppRootPath(tempgetContentWatchLinkByKey, core.webServer.requestVirtualFilePath, requestAppRootPath, core.webServer.requestDomain);
                //
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            ////throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18("main_GetContentWatchLinkByKey")
            return tempgetContentWatchLinkByKey;
        }
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
                pageContentModel page = pageContentModel.create(core, PageID);
                if (page != null) {
                    if ((page.ParentID == 0) && (!UsedIDList.Contains(page.ParentID))) {
                        UsedIDList.Add(page.ParentID);
                        if (siteSectionRootPageIndex.ContainsKey(page.ParentID)) {
                            sectionId = siteSectionRootPageIndex[page.ParentID];
                        }
                    } else {
                        sectionId = getPageSectionId(page.ParentID, ref UsedIDList, siteSectionRootPageIndex);
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
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
        internal int main_GetPageDynamicLink_GetTemplateID(int PageID, string UsedIDList) {
            int tempmain_GetPageDynamicLink_GetTemplateID = 0;
            try {
                //
                int CS = 0;
                int ParentID = 0;
                int templateId = 0;
                //
                //
                CS = core.db.csOpenRecord("Page Content", PageID, false, false, "TemplateID,ParentID");
                if (core.db.csOk(CS)) {
                    templateId = core.db.csGetInteger(CS, "TemplateID");
                    ParentID = core.db.csGetInteger(CS, "ParentID");
                }
                core.db.csClose(ref CS);
                //
                // Chase page tree to main_Get templateid
                //
                if (templateId == 0 && ParentID != 0) {
                    if (!genericController.IsInDelimitedString(UsedIDList, ParentID.ToString(), ",")) {
                        tempmain_GetPageDynamicLink_GetTemplateID = main_GetPageDynamicLink_GetTemplateID(ParentID, UsedIDList + "," + ParentID);
                    }
                }
                //
                return tempmain_GetPageDynamicLink_GetTemplateID;
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            ////throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError13("main_GetPageDynamicLink_GetTemplateID")
            return tempmain_GetPageDynamicLink_GetTemplateID;
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
            if (DefaultLink.Left( 1) != "/") {
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
                            pageTemplateModel template = pageTemplateModel.create(core, templateId);
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
                                resultLink = genericController.modifyLinkQuery(resultLink, "sid", SectionID.ToString(), true);
                            }
                            resultLink = genericController.modifyLinkQuery(resultLink, rnPageId, "", false);
                        } else {
                            resultLink = genericController.modifyLinkQuery(resultLink, rnPageId, genericController.encodeText(PageID), true);
                            if (PageID != 0) {
                                resultLink = genericController.modifyLinkQuery(resultLink, "sid", "", false);
                            }
                        }
                    }
                }
                resultLink = genericController.EncodeAppRootPath(resultLink, core.webServer.requestVirtualFilePath, requestAppRootPath, core.webServer.requestDomain);
            } catch (Exception ex) {
                core.handleException(ex);
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
            return getContentWatchLinkByKey(genericController.encodeText(ContentID) + "." + genericController.encodeText(RecordID), DefaultLink, IncrementClicks);
        }
        //
        //=============================================================================
        //
        public void verifyRegistrationFormPage(coreController core) {
            try {
                //
                int CS = 0;
                string GroupNameList = null;
                string Copy = null;
                //
                core.db.deleteContentRecords("Form Pages", "name=" + core.db.encodeSQLText("Registration Form"));
                CS = core.db.csOpen("Form Pages", "name=" + core.db.encodeSQLText("Registration Form"));
                if (!core.db.csOk(CS)) {
                    //
                    // create Version 1 template - just to main_Get it started
                    //
                    core.db.csClose(ref CS);
                    GroupNameList = "Registered";
                    CS = core.db.csInsertRecord("Form Pages");
                    if (core.db.csOk(CS)) {
                        core.db.csSet(CS, "name", "Registration Form");
                        Copy = ""
                        + "\r\n<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" width=\"100%\">"
                        + "\r\n{{REPEATSTART}}<tr><td align=right style=\"height:22px;\">{{CAPTION}}&nbsp;</td><td align=left>{{FIELD}}</td></tr>{{REPEATEND}}"
                        + "\r\n<tr><td align=right><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=135 height=1></td><td width=\"100%\">&nbsp;</td></tr>"
                        + "\r\n<tr><td colspan=2>&nbsp;<br>" + core.html.getPanelButtons(ButtonRegister, "Button") + "</td></tr>"
                        + "\r\n</table>";
                        core.db.csSet(CS, "Body", Copy);
                        Copy = ""
                        + "1"
                        + "\r\n" + GroupNameList + "\r\ntrue"
                        + "\r\n1,First Name,true,FirstName"
                        + "\r\n1,Last Name,true,LastName"
                        + "\r\n1,Email Address,true,Email"
                        + "\r\n1,Phone,true,Phone"
                        + "\r\n2,Please keep me informed of news and events,false,Subscribers"
                        + "";
                        core.db.csSet(CS, "Instructions", Copy);
                    }
                }
                core.db.csClose(ref CS);
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //---------------------------------------------------------------------------
        //   Create the default landing page if it is missing
        //---------------------------------------------------------------------------
        //
        public int createPageGetID(string PageName, string ContentName, int CreatedBy, string pageGuid) {
            int Id = 0;
            //
            int CS = core.db.csInsertRecord(ContentName, CreatedBy);
            if (core.db.csOk(CS)) {
                Id = core.db.csGetInteger(CS, "ID");
                core.db.csSet(CS, "name", PageName);
                core.db.csSet(CS, "active", "1");
                if (true) {
                    core.db.csSet(CS, "ccGuid", pageGuid);
                }
                core.db.csSave(CS);
                //   Call core.workflow.publishEdit("Page Content", Id)
            }
            core.db.csClose(ref CS);
            //
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
                    refreshQueryString = genericController.ModifyQueryString(core.doc.refreshQueryString, temp[0], temp[1], true);
                } else {
                    refreshQueryString = genericController.ModifyQueryString(core.doc.refreshQueryString, Name, Value, true);
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }

        }
        //
        //========================================================================
        //   Process manual changes needed for Page Content Special Cases
        //       If workflow, only call this routine on a publish - it changes live records
        //========================================================================
        //
        public void processAfterSave(bool IsDelete, string ContentName, int RecordID, string RecordName, int RecordParentID, bool UseContentWatchLink) {
            int addonId = 0;
            string Option_String = null;
            string Filename = null;
            string FilenameExt = null;
            string FilenameNoExt = null;
            string FilePath = "";
            int Pos = 0;
            string AltSizeList = null;
            imageEditController sf = null;
            bool RebuildSizes = false;
            int CS = 0;
            string TableName = null;
            int ContentID = 0;
            int ActivityLogOrganizationID = 0;
            //
            ContentID = Models.Complex.cdefModel.getContentId(core, ContentName);
            TableName = Models.Complex.cdefModel.getContentTablename(core, ContentName);
            markRecordReviewed(ContentName, RecordID);
            //
            // -- invalidate the specific cache for this record
            core.cache.invalidateContent_Entity(core, TableName, RecordID);
            //
            // -- invalidate the tablename -- meaning any cache consumer that cannot itemize its entity records, can depend on this, which will invalidate anytime any record clears
            core.cache.invalidate(TableName);
            //
            switch (genericController.vbLCase(TableName)) {
                case linkForwardModel.contentTableName:
                    //
                    Models.Complex.routeDictionaryModel.invalidateCache(core);
                    break;
                case linkAliasModel.contentTableName:
                    //
                    Models.Complex.routeDictionaryModel.invalidateCache(core);
                    break;
                case addonModel.contentTableName:
                    //
                    Models.Complex.routeDictionaryModel.invalidateCache(core);
                    core.cache.invalidate("addonCache");
                    core.cache.invalidateContent_Entity(core, TableName, RecordID);
                    break;
                case personModel.contentTableName:
                    //
                    // Log Activity for changes to people and organizattions
                    //
                    //hint = hint & ",110"
                    CS = core.db.csOpen2("people", RecordID, false, false, "Name,OrganizationID");
                    if (core.db.csOk(CS)) {
                        ActivityLogOrganizationID = core.db.csGetInteger(CS, "OrganizationID");
                    }
                    core.db.csClose(ref CS);
                    if (IsDelete) {
                        logController.addSiteActivity(core, "deleting user #" + RecordID + " (" + RecordName + ")", RecordID, ActivityLogOrganizationID);
                    } else {
                        logController.addSiteActivity(core, "saving changes to user #" + RecordID + " (" + RecordName + ")", RecordID, ActivityLogOrganizationID);
                    }
                    break;
                case "organizations":
                    //
                    // Log Activity for changes to people and organizattions
                    //
                    //hint = hint & ",120"
                    if (IsDelete) {
                        logController.addSiteActivity(core, "deleting organization #" + RecordID + " (" + RecordName + ")", 0, RecordID);
                    } else {
                        logController.addSiteActivity(core, "saving changes to organization #" + RecordID + " (" + RecordName + ")", 0, RecordID);
                    }
                    break;
                case "ccsetup":
                    //
                    // Site Properties
                    //
                    //hint = hint & ",130"
                    switch (genericController.vbLCase(RecordName)) {
                        case "allowlinkalias":
                            core.cache.invalidateAllInContent("Page Content");
                            break;
                        case "sectionlandinglink":
                            core.cache.invalidateAllInContent("Page Content");
                            break;
                        case siteproperty_serverPageDefault_name:
                            core.cache.invalidateAllInContent("Page Content");
                            break;
                    }
                    break;
                case "ccpagecontent":
                    //
                    // set ChildPagesFound true for parent page
                    //
                    //hint = hint & ",140"
                    if (RecordParentID > 0) {
                        if (!IsDelete) {
                            core.db.executeQuery("update ccpagecontent set ChildPagesfound=1 where ID=" + RecordParentID);
                        }
                    }
                    //
                    // Page Content special cases for delete
                    //
                    if (IsDelete) {
                        //
                        // Clear the Landing page and page not found site properties
                        //
                        if (RecordID == genericController.encodeInteger(core.siteProperties.getText("PageNotFoundPageID", "0"))) {
                            core.siteProperties.setProperty("PageNotFoundPageID", "0");
                        }
                        if (RecordID == core.siteProperties.landingPageID) {
                            core.siteProperties.setProperty("landingPageId", "0");
                        }
                        //
                        // Delete Link Alias entries with this PageID
                        //
                        core.db.executeQuery("delete from cclinkAliases where PageID=" + RecordID);
                    }
                    core.cache.invalidateContent_Entity(core, TableName, RecordID);
                    //Case "cctemplates" ', "ccsharedstyles"
                    //    '
                    //    ' Attempt to update the PageContentCache (PCC) array stored in the PeristantVariants
                    //    '
                    //    'hint = hint & ",150"
                    //    If Not IsNothing(core.addonStyleRulesIndex) Then
                    //        Call core.addonStyleRulesIndex.clear()
                    //    End If

                    break;
                case "cclibraryfiles":
                    //
                    // if a AltSizeList is blank, make large,medium,small and thumbnails
                    //
                    //hint = hint & ",180"
                    if (core.siteProperties.getBoolean("ImageAllowSFResize", true)) {
                        if (!IsDelete) {
                            CS = core.db.csOpenRecord("library files", RecordID);
                            if (core.db.csOk(CS)) {
                                Filename = core.db.csGet(CS, "filename");
                                Pos = Filename.LastIndexOf("/") + 1;
                                if (Pos > 0) {
                                    FilePath = Filename.Left( Pos);
                                    Filename = Filename.Substring(Pos);
                                }
                                core.db.csSet(CS, "filesize", core.appRootFiles.getFileSize(FilePath + Filename));
                                Pos = Filename.LastIndexOf(".") + 1;
                                if (Pos > 0) {
                                    FilenameExt = Filename.Substring(Pos);
                                    FilenameNoExt = Filename.Left( Pos - 1);
                                    if (genericController.vbInstr(1, "jpg,gif,png", FilenameExt, 1) != 0) {
                                        sf = new imageEditController();
                                        if (sf.load(core.appRootFiles.localAbsRootPath + FilePath + Filename)) {
                                            //
                                            //
                                            //
                                            core.db.csSet(CS, "height", sf.height);
                                            core.db.csSet(CS, "width", sf.width);
                                            AltSizeList = core.db.csGetText(CS, "AltSizeList");
                                            RebuildSizes = (string.IsNullOrEmpty(AltSizeList));
                                            if (RebuildSizes) {
                                                AltSizeList = "";
                                                //
                                                // Attempt to make 640x
                                                //
                                                if (sf.width >= 640) {
                                                    sf.height = encodeInteger(sf.height * (640 / sf.width));
                                                    sf.width = 640;
                                                    sf.save(core.appRootFiles.localAbsRootPath + FilePath + FilenameNoExt + "-640x" + sf.height + "." + FilenameExt);
                                                    AltSizeList = AltSizeList + "\r\n640x" + sf.height;
                                                }
                                                //
                                                // Attempt to make 320x
                                                //
                                                if (sf.width >= 320) {
                                                    sf.height = encodeInteger(sf.height * (320 / sf.width));
                                                    sf.width = 320;
                                                    sf.save(core.appRootFiles.localAbsRootPath + FilePath + FilenameNoExt + "-320x" + sf.height + "." + FilenameExt);

                                                    AltSizeList = AltSizeList + "\r\n320x" + sf.height;
                                                }
                                                //
                                                // Attempt to make 160x
                                                //
                                                if (sf.width >= 160) {
                                                    sf.height = encodeInteger(sf.height * (160 / sf.width));
                                                    sf.width = 160;
                                                    sf.save(core.appRootFiles.localAbsRootPath + FilePath + FilenameNoExt + "-160x" + sf.height + "." + FilenameExt);
                                                    AltSizeList = AltSizeList + "\r\n160x" + sf.height;
                                                }
                                                //
                                                // Attempt to make 80x
                                                //
                                                if (sf.width >= 80) {
                                                    sf.height = encodeInteger(sf.height * (80 / sf.width));
                                                    sf.width = 80;
                                                    sf.save(core.appRootFiles.localAbsRootPath + FilePath + FilenameNoExt + "-180x" + sf.height + "." + FilenameExt);
                                                    AltSizeList = AltSizeList + "\r\n80x" + sf.height;
                                                }
                                                core.db.csSet(CS, "AltSizeList", AltSizeList);
                                            }
                                            sf.Dispose();
                                            sf = null;
                                        }
                                        //                                sf.Algorithm = genericController.EncodeInteger(main_GetSiteProperty("ImageResizeSFAlgorithm", "5"))
                                        //                                //On Error //Resume Next
                                        //                                sf.LoadFromFile (app.publicFiles.rootFullPath & FilePath & Filename)
                                        //                                If Err.Number = 0 Then
                                        //                                    Call app.SetCS(CS, "height", sf.Height)
                                        //                                    Call app.SetCS(CS, "width", sf.Width)
                                        //                                Else
                                        //                                    Err.Clear
                                        //                                End If
                                        //                                AltSizeList = cs_getText(CS, "AltSizeList")
                                        //                                RebuildSizes = (AltSizeList = "")
                                        //                                If RebuildSizes Then
                                        //                                    AltSizeList = ""
                                        //                                    '
                                        //                                    ' Attempt to make 640x
                                        //                                    '
                                        //                                    If sf.Width >= 640 Then
                                        //                                        sf.Width = 640
                                        //                                        Call sf.DoResize
                                        //                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-640x" & sf.Height & "." & FilenameExt)
                                        //                                        AltSizeList = AltSizeList & vbCrLf & "640x" & sf.Height
                                        //                                    End If
                                        //                                    '
                                        //                                    ' Attempt to make 320x
                                        //                                    '
                                        //                                    If sf.Width >= 320 Then
                                        //                                        sf.Width = 320
                                        //                                        Call sf.DoResize
                                        //                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-320x" & sf.Height & "." & FilenameExt)
                                        //                                        AltSizeList = AltSizeList & vbCrLf & "320x" & sf.Height
                                        //                                    End If
                                        //                                    '
                                        //                                    ' Attempt to make 160x
                                        //                                    '
                                        //                                    If sf.Width >= 160 Then
                                        //                                        sf.Width = 160
                                        //                                        Call sf.DoResize
                                        //                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-160x" & sf.Height & "." & FilenameExt)
                                        //                                        AltSizeList = AltSizeList & vbCrLf & "160x" & sf.Height
                                        //                                    End If
                                        //                                    '
                                        //                                    ' Attempt to make 80x
                                        //                                    '
                                        //                                    If sf.Width >= 80 Then
                                        //                                        sf.Width = 80
                                        //                                        Call sf.DoResize
                                        //                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-80x" & sf.Height & "." & FilenameExt)
                                        //                                        AltSizeList = AltSizeList & vbCrLf & "80x" & sf.Height
                                        //                                    End If
                                        //                                    Call app.SetCS(CS, "AltSizeList", AltSizeList)
                                        //                                End If
                                        //                                sf = Nothing
                                    }
                                }
                            }
                            core.db.csClose(ref CS);
                        }
                    }
                    break;
                default:
                    //
                    // -- edit and delete for records -- clear entity cache
                    core.cache.invalidateContent_Entity(core, TableName, RecordID);
                    break;
            }
            //
            // -- edit and delete for records -- clear entity cache
            core.cache.invalidateContent_Entity(core, TableName, RecordID);
            //
            // Process Addons marked to trigger a process call on content change
            //
            CS = core.db.csOpen("Add-on Content Trigger Rules", "ContentID=" + ContentID,"", false, 0, false, false, "addonid");
            if (IsDelete) {
                Option_String = ""
                    + "\r\naction=contentdelete"
                    + "\r\ncontentid=" + ContentID + "\r\nrecordid=" + RecordID + "";
            } else {
                Option_String = ""
                    + "\r\naction=contentchange"
                    + "\r\ncontentid=" + ContentID + "\r\nrecordid=" + RecordID + "";
            }
            while (core.db.csOk(CS)) {
                addonId = core.db.csGetInteger(CS, "Addonid");
                //hint = hint & ",210 addonid=[" & addonId & "]"
                core.addon.executeAsync(addonId.ToString(), Option_String);
                core.db.csGoNext(CS);
            }
            core.db.csClose(ref CS);
            //
            // -- ok, temp work-around for the damn cache not invalidating correctly -- the nuclear solution
            //core.cache.invalidateAll();
        }
        //
        public void markRecordReviewed(string ContentName, int RecordID) {
            try {
                if (Models.Complex.cdefModel.isContentFieldSupported(core, ContentName, "DateReviewed")) {
                    string DataSourceName = Models.Complex.cdefModel.getContentDataSource(core, ContentName);
                    string TableName = Models.Complex.cdefModel.getContentTablename(core, ContentName);
                    string SQL = "update " + TableName + " set DateReviewed=" + core.db.encodeSQLDate(core.doc.profileStartTime);
                    if (Models.Complex.cdefModel.isContentFieldSupported(core, ContentName, "ReviewedBy")) {
                        SQL += ",ReviewedBy=" + core.sessionContext.user.id;
                    }
                    //
                    // -- Mark the live record
                    core.db.executeQuery(SQL + " where id=" + RecordID, DataSourceName);
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //=============================================================================
        //   Sets the MetaContent subsystem so the next call to main_GetLastMeta... returns the correct value
        //       And neither takes much time
        //=============================================================================
        //
        public void setMetaContent(int ContentID, int RecordID) {
            string KeywordList = "";
            int CS = 0;
            string Criteria = null;
            string SQL = null;
            string FieldList = null;
            int iContentID = 0;
            int iRecordID = 0;
            int MetaContentID = 0;
            //
            iContentID = genericController.encodeInteger(ContentID);
            iRecordID = genericController.encodeInteger(RecordID);
            if ((iContentID != 0) & (iRecordID != 0)) {
                //
                // main_Get ID, Description, Title
                //
                Criteria = "(ContentID=" + iContentID + ")and(RecordID=" + iRecordID + ")";
                FieldList = "ID,Name,MetaDescription,OtherHeadTags,MetaKeywordList";
                CS = core.db.csOpen("Meta Content", Criteria, "", false, 0, false, false, FieldList);
                if (core.db.csOk(CS)) {
                    MetaContentID = core.db.csGetInteger(CS, "ID");
                    core.html.addTitle(genericController.encodeHTML(core.db.csGetText(CS, "Name")), "page content");
                    core.html.addMetaDescription(genericController.encodeHTML(core.db.csGetText(CS, "MetaDescription")), "page content");
                    core.html.addHeadTag(core.db.csGetText(CS, "OtherHeadTags"), "page content");
                    if (true) {
                        KeywordList = genericController.vbReplace(core.db.csGetText(CS, "MetaKeywordList"), "\r\n", ",");
                    }
                    //main_MetaContent_Title = encodeHTML(app.csv_cs_getText(CS, "Name"))
                    //htmldoc.main_MetaContent_Description = encodeHTML(app.csv_cs_getText(CS, "MetaDescription"))
                    //main_MetaContent_OtherHeadTags = app.csv_cs_getText(CS, "OtherHeadTags")
                }
                core.db.csClose(ref CS);
                //
                // main_Get Keyword List
                //
                SQL = "select ccMetaKeywords.Name"
                    + " From ccMetaKeywords"
                    + " LEFT JOIN ccMetaKeywordRules on ccMetaKeywordRules.MetaKeywordID=ccMetaKeywords.ID"
                    + " Where ccMetaKeywordRules.MetaContentID=" + MetaContentID;
                CS = core.db.csOpenSql(SQL);
                while (core.db.csOk(CS)) {
                    KeywordList = KeywordList + "," + core.db.csGetText(CS, "Name");
                    core.db.csGoNext(CS);
                }
                if (!string.IsNullOrEmpty(KeywordList)) {
                    if (KeywordList.Left( 1) == ",") {
                        KeywordList = KeywordList.Substring(1);
                    }
                    //KeyWordList = Mid(KeyWordList, 2)
                    KeywordList = genericController.encodeHTML(KeywordList);
                    core.html.addMetaKeywordList(KeywordList, "page content");
                }
                core.db.csClose(ref CS);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Clear all data from the metaData current instance. Next request will load from cache.
        /// </summary>
        public void clearMetaData() {
            try {
                if (core.doc.cdefDictionary != null) {
                    cdefDictionary.Clear();
                }
                if (tableSchemaDictionary != null) {
                    tableSchemaDictionary.Clear();
                }
                contentNameIdDictionaryClear();
                contentIdDictClear();
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }

    }
}

