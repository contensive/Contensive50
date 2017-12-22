using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.BaseClasses;
using Contensive.Core;
using Models.Entity;
using Controllers;

//
namespace Controllers
{
	//
	public class docController
	{
		/// <summary>
		/// Persistence for the doc. Maintain all the parts and output the results. Constructor initializes the object. Call initDoc() to setup pages
		/// </summary>
		// -- not sure if this is the best plan, buts lets try this and see if we can get out of it later (to make this an addon) 
		//
		private coreClass cpcore;
		//
		// -- this documents unique guid (created on the fly)
		public string docGuid {get; set;}
		//
		// -- set true if any addon executed is set  htmlDocument=true. When true, the initial addon executed is returned in the html wrapper (html with head)
		public bool htmlDocument {get; set;} = false;
		//
		// -- head tags, script tags, style tags, etc
		public List<htmlAssetClass> htmlAssetList {get; set;} = new List<htmlAssetClass>();
		//
		// -- head meta tag list (convert to list object)
		public List<htmlMetaClass> htmlMetaContent_OtherTags {get; set;} = new List<htmlMetaClass>();
		public List<htmlMetaClass> htmlMetaContent_TitleList {get; set;} = new List<htmlMetaClass>();
		public List<htmlMetaClass> htmlMetaContent_Description {get; set;} = new List<htmlMetaClass>();
		public List<htmlMetaClass> htmlMetaContent_KeyWordList {get; set;} = new List<htmlMetaClass>();
		//
		// -- current domain
		public Models.Entity.domainModel domain {get; set;}
		//
		// -- current page
		public Models.Entity.pageContentModel page {get; set;}
		//
		// -- current page to it's root
		public List<Models.Entity.pageContentModel> pageToRootList {get; set;}
		//
		// -- current template
		public Models.Entity.pageTemplateModel template {get; set;}
		public string templateReason {get; set;} = "";
		//
		// -- Anything that needs to be written to the Page during main_GetClosePage
		public string htmlForEndOfBody {get; set;} = "";
		//
		// -- others to be sorted
		public int editWrapperCnt {get; set;} = 0;
		public string docBodyFilter {get; set;} = "";
		public bool legacySiteStyles_Loaded {get; set;} = false;
		public int menuSystemCloseCount {get; set;} = 0;
		internal int helpCodeCount {get; set;} = 0;
		internal int helpCodeSize {get; set;} = 0;
		internal string[] helpCodes {get; set;} = {};
		internal string[] helpCaptions {get; set;} = {};
		internal int helpDialogCnt {get; set;} = 0;
		public string refreshQueryString {get; set;} = ""; // the querystring required to return to the current state (perform a refresh)
		public int redirectContentID {get; set;} = 0;
		public int redirectRecordID {get; set;} = 0;
		//Public Property isStreamWritten As Boolean = False       ' true when anything has been writeAltBuffered.
		public bool outputBufferEnabled {get; set;} = true; // when true (default), stream is buffered until page is done
		// Public Property docBuffer As String = ""                   ' if any method calls writeAltBuffer, string concatinates here. If this is not empty at exit, it is used instead of returned string
		//Public Property metaContent_TemplateStyleSheetTag As String = ""
		public menuComboTabController menuComboTab {get; set;}
		public menuLiveTabController menuLiveTab {get; set;}
		public string adminWarning {get; set;} = ""; // Message - when set displays in an admin hint box in the page
		public int adminWarningPageID {get; set;} = 0; // PageID that goes with the warning
		public int checkListCnt {get; set;} = 0; // cnt of the main_GetFormInputCheckList calls - used for javascript
		public string includedAddonIDList {get; set;} = "";
		public int inputDateCnt {get; set;} = 0;
		public int inputSelectCacheCnt {get; set;} = 0;
		public main_InputSelectCacheType[] inputSelectCache {get; set;}
		public int formInputTextCnt {get; set;} = 0;
		public string quickEditCopy {get; set;} = "";
		public string siteStructure {get; set;} = "";
		public bool siteStructure_LocalLoaded {get; set;} = false;
		public string bodyContent {get; set;} = ""; // stored here so cp.doc.content valid during bodyEnd event
		public int landingPageID {get; set;} = 0;
		public string redirectLink {get; set;} = "";
		public string redirectReason {get; set;} = "";
		public bool redirectBecausePageNotFound {get; set;} = false;
		//'
		//' -- addon call depth. When an addon is called, it saves the value interanlly and increments. When level0 exits and htmlDocument is true, the output is wrapped with an html doc
		//Public Property addonDepth As Integer = 0
		internal List<string> errList {get; set;} // exceptions collected during document construction
		public int errorCount {get; set;} = 0;
		internal List<string> userErrorList {get; set;} // user messages
		public string debug_iUserError {get; set;} = ""; // User Error String
		public string trapLogMessage {get; set;} = ""; // The content of the current traplog (keep for popups if no Csv)
		public string testPointMessage {get; set;} = "";
		public bool visitPropertyAllowDebugging {get; set;} = false; // if true, send main_TestPoint messages to the stream
		public Models.Context.authContextModel authContext {get; set;}
		internal Stopwatch appStopWatch {get; set;} = Stopwatch.StartNew();
		public DateTime profileStartTime {get; set;} // set in constructor
		public int profileStartTickCount {get; set;} = 0;
		public bool allowDebugLog {get; set;} = false; // turn on in script -- use to write /debug.log in content files for whatever is needed
		public bool blockExceptionReporting {get; set;} = false; // used so error reporting can not call itself
		//Public Property pageErrorWithoutCsv As Boolean = False                  ' if true, the error occurred before Csv was available and main_TrapLogMessage needs to be saved and popedup
		//Public Property closePageCounter As Integer = 0
		public bool continueProcessing {get; set;} = false; // when false, routines should not add to the output and immediately exit
		public bool upgradeInProgress {get; set;}
		internal List<int> addonIdListRunInThisDoc {get; set;} = new List<int>();
		internal List<int> addonsCurrentlyRunningIdList {get; set;} = new List<int>();
		public int pageAddonCnt {get; set;} = 0;
		//
		// -- persistant store for cdef complex model
		internal Dictionary<string, Models.Complex.cdefModel> cdefDictionary {get; set;}
		//
		// -- persistant store for tableSchema complex mode
		internal Dictionary<string, Models.Complex.tableSchemaModel> tableSchemaDictionary {get; set;}
		//
		//====================================================================================================
		// -- lookup contentId by contentName
		internal Dictionary<string, int> contentNameIdDictionary
		{
			get
			{
				if (_contentNameIdDictionary == null)
				{
					_contentNameIdDictionary = new Dictionary<string, int>();
					foreach (KeyValuePair<int, contentModel> kvp in contentIdDict)
					{
						string key = kvp.Value.name.Trim().ToLower();
						if (!string.IsNullOrEmpty(key))
						{
							if (!_contentNameIdDictionary.ContainsKey(key))
							{
								_contentNameIdDictionary.Add(key, kvp.Value.id);
							}
						}
					}
				}
				return _contentNameIdDictionary;
			}
		}
		internal void contentNameIdDictionaryClear()
		{
			_contentNameIdDictionary = null;
		}
		private Dictionary<string, int> _contentNameIdDictionary = null;
		//
		//====================================================================================================
		// -- lookup contentModel by contentId
		internal Dictionary<int, contentModel> contentIdDict
		{
			get
			{
				if (_contentIdDict == null)
				{
					_contentIdDict = contentModel.createDict(cpcore, new List<string>());
				}
				return _contentIdDict;
			}
		}
		internal string landingLink
		{
			get
			{
				if (_landingLink == "")
				{
					_landingLink = cpcore.siteProperties.getText("SectionLandingLink", requestAppRootPath + cpcore.siteProperties.serverPageDefault);
					_landingLink = genericController.ConvertLinkToShortLink(_landingLink, cpcore.webServer.requestDomain, cpcore.webServer.requestVirtualFilePath);
					_landingLink = genericController.EncodeAppRootPath(_landingLink, cpcore.webServer.requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain);
				}
				return _landingLink;
			}
		}
		private string _landingLink {get; set;} = ""; // Default Landing page - managed through main_GetLandingLink() '
		internal void contentIdDictClear()
		{
			_contentIdDict = null;
		}
		private Dictionary<int, contentModel> _contentIdDict = null;
		//
		//====================================================================================================
		/// <summary>
		/// this will eventuall be an addon, but lets do this first to keep the converstion complexity down
		/// </summary>
		/// <param name="cpCore"></param>
		public docController(coreClass cpCore)
		{
			this.cpcore = cpCore;
			//
			domain = new Models.Entity.domainModel();
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
		public int main_OpenCSWhatsNew(coreClass cpCore, string SortFieldList = "", bool ActiveOnly = true, int PageSize = 1000, int PageNumber = 1)
		{
			int result = -1;
			try
			{
				result = main_OpenCSContentWatchList(cpCore, "What's New", SortFieldList, ActiveOnly, PageSize, PageNumber);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		//========================================================================
		//   Open a content set with the current whats new list
		//========================================================================
		//
		public int main_OpenCSContentWatchList(coreClass cpcore, string ListName, string SortFieldList = "", bool ActiveOnly = true, int PageSize = 1000, int PageNumber = 1)
		{
			int result = -1;
			try
			{
				string SQL = null;
				string iSortFieldList = null;
				string MethodName = null;
				int CS = 0;
				//
				iSortFieldList = Convert.ToString(encodeEmptyText(SortFieldList, "")).Trim(' ');
				//iSortFieldList = encodeMissingText(SortFieldList, "DateAdded")
				if (string.IsNullOrEmpty(iSortFieldList))
				{
					iSortFieldList = "DateAdded";
				}
				//
				MethodName = "main_OpenCSContentWatchList( " + ListName + ", " + iSortFieldList + ", " + ActiveOnly + " )";
				//
				// ----- Add tablename to the front of SortFieldList fieldnames
				//
				iSortFieldList = " " + genericController.vbReplace(iSortFieldList, ",", " , ") + " ";
				iSortFieldList = genericController.vbReplace(iSortFieldList, " ID ", " ccContentWatch.ID ", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
				iSortFieldList = genericController.vbReplace(iSortFieldList, " Link ", " ccContentWatch.Link ", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
				iSortFieldList = genericController.vbReplace(iSortFieldList, " LinkLabel ", " ccContentWatch.LinkLabel ", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
				iSortFieldList = genericController.vbReplace(iSortFieldList, " SortOrder ", " ccContentWatch.SortOrder ", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
				iSortFieldList = genericController.vbReplace(iSortFieldList, " DateAdded ", " ccContentWatch.DateAdded ", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
				iSortFieldList = genericController.vbReplace(iSortFieldList, " ContentID ", " ccContentWatch.ContentID ", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
				iSortFieldList = genericController.vbReplace(iSortFieldList, " RecordID ", " ccContentWatch.RecordID ", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
				iSortFieldList = genericController.vbReplace(iSortFieldList, " ModifiedDate ", " ccContentWatch.ModifiedDate ", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
				//
				// ----- Special case
				//
				iSortFieldList = genericController.vbReplace(iSortFieldList, " name ", " ccContentWatch.LinkLabel ", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
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
				+ " WHERE (((ccContentWatchLists.Name)=" + this.cpcore.db.encodeSQLText(ListName) + ")"
					+ "AND ((ccContentWatchLists.Active)<>0)"
					+ "AND ((ccContentWatchListRules.Active)<>0)"
					+ "AND ((ccContentWatch.Active)<>0)"
					+ "AND (ccContentWatch.Link is not null)"
					+ "AND (ccContentWatch.LinkLabel is not null)"
					+ "AND ((ccContentWatch.WhatsNewDateExpires is null)or(ccContentWatch.WhatsNewDateExpires>" + this.cpcore.db.encodeSQLDate(cpcore.doc.profileStartTime) + "))"
					+ ")"
				+ " ORDER BY " + iSortFieldList + ";";
				result = this.cpcore.db.csOpenSql(SQL,, PageSize, PageNumber);
				if (!this.cpcore.db.csOk(result))
				{
					//
					// Check if listname exists
					//
					CS = this.cpcore.db.csOpen("Content Watch Lists", "name=" + this.cpcore.db.encodeSQLText(ListName), "ID",,,,, "ID");
					if (!this.cpcore.db.csOk(CS))
					{
						this.cpcore.db.csClose(CS);
						CS = this.cpcore.db.csInsertRecord("Content Watch Lists");
						if (this.cpcore.db.csOk(CS))
						{
							this.cpcore.db.csSet(CS, "name", ListName);
						}
					}
					this.cpcore.db.csClose(CS);
				}
			}
			catch (Exception ex)
			{
				this.cpcore.handleException(ex);
			}
			return result;
		}
		//
		//========================================================================
		// Print Whats New
		//   Prints a linked list of new content
		//========================================================================
		//
		public string main_GetWhatsNew(coreClass cpcore, string SortFieldList = "")
		{
			string result = "";
			try
			{
				int CSPointer = 0;
				int ContentID = 0;
				int RecordID = 0;
				string LinkLabel = null;
				string Link = null;
				//
				CSPointer = main_OpenCSWhatsNew(cpcore, SortFieldList);
				//
				if (this.cpcore.db.csOk(CSPointer))
				{
					ContentID = Models.Complex.cdefModel.getContentId(cpcore, "Content Watch");
					while (this.cpcore.db.csOk(CSPointer))
					{
						Link = this.cpcore.db.csGetText(CSPointer, "link");
						LinkLabel = this.cpcore.db.csGetText(CSPointer, "LinkLabel");
						RecordID = this.cpcore.db.csGetInteger(CSPointer, "ID");
						if (!string.IsNullOrEmpty(LinkLabel))
						{
							result = result + "\r" + "<li class=\"ccListItem\">";
							if (!string.IsNullOrEmpty(Link))
							{
								result = result + genericController.csv_GetLinkedText("<a href=\"" + genericController.encodeHTML(cpcore.webServer.requestPage + "?rc=" + ContentID + "&ri=" + RecordID) + "\">", LinkLabel);
							}
							else
							{
								result = result + LinkLabel;
							}
							result = result + "</li>";
						}
						this.cpcore.db.csGoNext(CSPointer);
					}
					result = "\r" + "<ul class=\"ccWatchList\">" + htmlIndent(result) + "\r" + "</ul>";
				}
				this.cpcore.db.csClose(CSPointer);
			}
			catch (Exception ex)
			{
				this.cpcore.handleException(ex);
			}
			return result;
		}
		//
		//
		//
		public string main_GetWatchList(coreClass cpCore, string ListName, string SortField, bool SortReverse)
		{
			string result = "";
			try
			{
				int CS = 0;
				int ContentID = 0;
				int RecordID = 0;
				string Link = null;
				string LinkLabel = null;
				//
				if (SortReverse && (!string.IsNullOrEmpty(SortField)))
				{
					CS = main_OpenCSContentWatchList(cpCore, ListName, SortField + " Desc", true);
				}
				else
				{
					CS = main_OpenCSContentWatchList(cpCore, ListName, SortField, true);
				}
				//
				if (this.cpcore.db.csOk(CS))
				{
					ContentID = Models.Complex.cdefModel.getContentId(cpCore, "Content Watch");
					while (this.cpcore.db.csOk(CS))
					{
						Link = this.cpcore.db.csGetText(CS, "link");
						LinkLabel = this.cpcore.db.csGetText(CS, "LinkLabel");
						RecordID = this.cpcore.db.csGetInteger(CS, "ID");
						if (!string.IsNullOrEmpty(LinkLabel))
						{
							result = result + "\r" + "<li id=\"main_ContentWatch" + RecordID + "\" class=\"ccListItem\">";
							if (!string.IsNullOrEmpty(Link))
							{
								result = result + "<a href=\"http://" + this.cpcore.webServer.requestDomain + requestAppRootPath + this.cpcore.webServer.requestPage + "?rc=" + ContentID + "&ri=" + RecordID + "\">" + LinkLabel + "</a>";
							}
							else
							{
								result = result + LinkLabel;
							}
							result = result + "</li>";
						}
						this.cpcore.db.csGoNext(CS);
					}
					if (!string.IsNullOrEmpty(result))
					{
						result = this.cpcore.html.html_GetContentCopy("Watch List Caption: " + ListName, ListName, this.cpcore.doc.authContext.user.id, true, this.cpcore.doc.authContext.isAuthenticated) + "\r" + "<ul class=\"ccWatchList\">" + htmlIndent(result) + "\r" + "</ul>";
					}
				}
				this.cpcore.db.csClose(CS);
				//
				if (this.cpcore.visitProperty.getBoolean("AllowAdvancedEditor"))
				{
					result = this.cpcore.html.getEditWrapper("Watch List [" + ListName + "]", result);
				}
			}
			catch (Exception ex)
			{
				this.cpcore.handleException(ex);
			}
			return result;
		}
		//
		//==========================================================================
		//   returns the site structure xml
		//==========================================================================
		//
//INSTANT C# NOTE: C# does not support parameterized properties - the following property has been rewritten as a function:
//ORIGINAL LINE: Public ReadOnly Property main_SiteStructure(cpcore As coreClass) As String
		public string get_main_SiteStructure(coreClass cpcore)
		{
			bool returnStatus = false;
			if (!siteStructure_LocalLoaded)
			{
				addonModel addon = addonModel.create(cpcore, addonGuidSiteStructureGuid);
				siteStructure = this.cpcore.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext() {addonType = CPUtilsBaseClass.addonContext.ContextSimple});
				//siteStructure = Me.cpcore.addon.execute_legacy2(0, addonGuidSiteStructureGuid, "", CPUtilsBaseClass.addonContext.ContextSimple, "", 0, "", "", False, -1, "", returnStatus, Nothing)
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
		internal string getQuickEditing(int rootPageId, string OrderByClause, bool AllowPageList, bool AllowReturnLink, bool ArchivePages, int contactMemberID, int childListSortMethodId, bool main_AllowChildListComposite, bool ArchivePage)
		{
			string result = string.Empty;
			try
			{
				string RootPageContentName = Models.Entity.pageContentModel.contentName;
				string LiveRecordContentName = Models.Entity.pageContentModel.contentName;
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
				string main_EditLockMemberName = string.Empty;
				DateTime main_EditLockDateExpires = default(DateTime);
				DateTime SubmittedDate = default(DateTime);
				DateTime ApprovedDate = default(DateTime);
				DateTime ModifiedDate = default(DateTime);
				//
				cpcore.html.addStyleLink("/quickEditor/styles.css", "Quick Editor");
				//
				// ----- First Active Record - Output Quick Editor form
				//
				CDef = Models.Complex.cdefModel.getCdef(cpcore, LiveRecordContentName);
				//
				// main_Get Authoring Status and permissions
				//
				IsEditLocked = cpcore.workflow.GetEditLockStatus(LiveRecordContentName, page.id);
				if (IsEditLocked)
				{
					main_EditLockMemberName = cpcore.workflow.GetEditLockMemberName(LiveRecordContentName, page.id);
					main_EditLockDateExpires = genericController.EncodeDate(cpcore.workflow.GetEditLockMemberName(LiveRecordContentName, page.id));
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
				AllowMarkReviewed = Models.Complex.cdefModel.isContentFieldSupported(cpcore, Models.Entity.pageContentModel.contentName, "DateReviewed");
				OptionsPanelAuthoringStatus = cpcore.doc.authContext.main_GetAuthoringStatusMessage(cpcore, false, IsEditLocked, main_EditLockMemberName, main_EditLockDateExpires, IsApproved, ApprovedMemberName, IsSubmitted, SubmittedMemberName, IsDeleted, IsInserted, IsModified, ModifiedMemberName);
				//
				// Set Editing Authoring Control
				//
				cpcore.workflow.SetEditLock(LiveRecordContentName, page.id);
				//
				// SubPanel: Authoring Status
				//
				ButtonList = "";
				if (AllowCancel)
				{
					ButtonList = ButtonList + "," + ButtonCancel;
				}
				if (allowSave)
				{
					ButtonList = ButtonList + "," + ButtonSave + "," + ButtonOK;
				}
				if (AllowDelete && !IsRootPage)
				{
					ButtonList = ButtonList + "," + ButtonDelete;
				}
				if (AllowInsert)
				{
					ButtonList = ButtonList + "," + ButtonAddChildPage;
				}
				if ((page_ParentID != 0) && AllowInsert)
				{
					ButtonList = ButtonList + "," + ButtonAddSiblingPage;
				}
				if (AllowMarkReviewed)
				{
					ButtonList = ButtonList + "," + ButtonMarkReviewed;
				}
				if (!string.IsNullOrEmpty(ButtonList))
				{
					ButtonList = ButtonList.Substring(1);
					ButtonList = cpcore.html.main_GetPanelButtons(ButtonList, "Button");
				}
				//If OptionsPanelAuthoringStatus <> "" Then
				//    result = result & "" _
				//        & cr & "<tr>" _
				//        & cr2 & "<td colspan=2 class=""qeRow""><div class=""qeHeadCon"">" & OptionsPanelAuthoringStatus & "</div></td>" _
				//        & cr & "</tr>"
				//End If
				if (cpcore.doc.debug_iUserError != "")
				{
					result = result + ""
						+ "\r" + "<tr>"
						+ cr2 + "<td colspan=2 class=\"qeRow\"><div class=\"qeHeadCon\">" + errorController.error_GetUserError(cpcore) + "</div></td>"
						+ "\r" + "</tr>";
				}
				if (readOnlyField)
				{
					result = result + ""
					+ "\r" + "<tr>"
					+ cr2 + "<td colspan=\"2\" class=\"qeRow\">" + getQuickEditingBody(LiveRecordContentName, OrderByClause, AllowPageList, true, rootPageId, readOnlyField, AllowReturnLink, RootPageContentName, ArchivePages, contactMemberID) + "</td>"
					+ "\r" + "</tr>";
				}
				else
				{
					result = result + ""
					+ "\r" + "<tr>"
					+ cr2 + "<td colspan=\"2\" class=\"qeRow\">" + getQuickEditingBody(LiveRecordContentName, OrderByClause, AllowPageList, true, rootPageId, readOnlyField, AllowReturnLink, RootPageContentName, ArchivePages, contactMemberID) + "</td>"
					+ "\r" + "</tr>";
				}
				result = result + "\r" + "<tr>"
					+ cr2 + "<td class=\"qeRow qeLeft\" style=\"padding-top:10px;\">Name</td>"
					+ cr2 + "<td class=\"qeRow qeRight\">" + cpcore.html.html_GetFormInputText2("name", page.name, 1,,,, readOnlyField) + "</td>"
					+ "\r" + "</tr>"
					+ "";
				//
				// ----- Parent pages
				//
				if (pageToRootList.Count == 1)
				{
					PageList = "&nbsp;(there are no parent pages)";
				}
				else
				{
					PageList = "<ul class=\"qeListUL\"><li class=\"qeListLI\">Current Page</li></ul>";
					foreach (pageContentModel testPage in Enumerable.Reverse(pageToRootList))
					{
						Link = testPage.name;
						if (string.IsNullOrEmpty(Link))
						{
							Link = "no name #" + genericController.encodeText(testPage.id);
						}
						Link = "<a href=\"" + testPage.PageLink + "\">" + Link + "</a>";
						PageList = "<ul class=\"qeListUL\"><li class=\"qeListLI\">" + Link + PageList + "</li></ul>";
					}
				}
				result = result + ""
				+ "\r" + "<tr>"
				+ cr2 + "<td class=\"qeRow qeLeft\" style=\"padding-top:26px;\">Parent Pages</td>"
				+ cr2 + "<td class=\"qeRow qeRight\"><div class=\"qeListCon\">" + PageList + "</div></td>"
				+ "\r" + "</tr>";
				//
				// ----- Child pages
				//
				Models.Entity.addonModel addon = Models.Entity.addonModel.create(cpcore, cpcore.siteProperties.childListAddonID);
				CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext
				{
					addonType = CPUtilsBaseClass.addonContext.ContextPage,
					hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext()
					{
						contentName = Models.Entity.pageContentModel.contentName,
						fieldName = "",
						recordId = page.id
					},
					instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpcore, page.ChildListInstanceOptions),
					instanceGuid = PageChildListInstanceID
				};
				PageList = cpcore.addon.execute(addon, executeContext);
				//PageList = cpcore.addon.execute_legacy2(cpcore.siteProperties.childListAddonID, "", page.ChildListInstanceOptions, CPUtilsBaseClass.addonContext.ContextPage, Models.Entity.pageContentModel.contentName, page.id, "", PageChildListInstanceID, False, -1, "", AddonStatusOK, Nothing)
				if (genericController.vbInstr(1, PageList, "<ul", Microsoft.VisualBasic.Constants.vbTextCompare) == 0)
				{
					PageList = "(there are no child pages)";
				}
				result = result + "\r" + "<tr>"
					+ cr2 + "<td class=\"qeRow qeLeft\" style=\"padding-top:36px;\">Child Pages</td>"
					+ cr2 + "<td class=\"qeRow qeRight\"><div class=\"qeListCon\">" + PageList + "</div></td>"
					+ "\r" + "</tr>";
				result = ""
					+ "\r" + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">"
					+ genericController.htmlIndent(result) + "\r" + "</table>";
				result = ""
					+ ButtonList + result + ButtonList;
				result = cpcore.html.main_GetPanel(result);
				//
				// Form Wrapper
				//
				result = ""
					+ '\r' + cpcore.html.html_GetUploadFormStart(cpcore.webServer.requestQueryString) + '\r' + cpcore.html.html_GetFormInputHidden("Type", FormTypePageAuthoring) + '\r' + cpcore.html.html_GetFormInputHidden("ID", page.id) + '\r' + cpcore.html.html_GetFormInputHidden("ContentName", LiveRecordContentName) + '\r' + result + "\r" + cpcore.html.html_GetUploadFormEnd();

				//& cr & cpcore.html.main_GetPanelHeader("Contensive Quick Editor") _

				result = ""
					+ "\r" + "<div class=\"ccCon\">"
					+ genericController.htmlIndent(result) + "\r" + "</div>";
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
			}
			return result;
		}
		//
		//========================================================================
		//
		//========================================================================
		//
		internal string getQuickEditingBody(string ContentName, string OrderByClause, bool AllowChildList, bool Authoring, int rootPageId, bool readOnlyField, bool AllowReturnLink, string RootPageContentName, bool ArchivePage, int contactMemberID)
		{
			string pageCopy = page.Copyfilename.content;
			//If page.Copyfilename <> "" Then
			//    pageCopy = page.Copyfilename.copy(cpcore)
			//    'pageCopy = cpcore.cdnFiles.readFile(page.Copyfilename)
			//End If
			//
			// ----- Page Copy
			//
			int FieldRows = cpcore.userProperty.getInteger(ContentName + ".copyFilename.PixelHeight", 500);
			if (FieldRows < 50)
			{
				FieldRows = 50;
				cpcore.userProperty.setProperty(ContentName + ".copyFilename.PixelHeight", FieldRows);
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
		internal string getReturnBreadcrumb(string RootPageContentName, int ignore, int rootPageId, string ParentIDPath, bool ArchivePage, string BreadCrumbDelimiter)
		{
			string returnHtml = "";
			//
			foreach (pageContentModel testpage in pageToRootList)
			{
				string pageCaption = testpage.MenuHeadline;
				if (string.IsNullOrEmpty(pageCaption))
				{
					pageCaption = genericController.encodeText(testpage.name);
				}
				if (string.IsNullOrEmpty(returnHtml))
				{
					returnHtml = pageCaption;
				}
				else
				{
					returnHtml = "<a href=\"" + genericController.encodeHTML(pageContentController.getPageLink(cpcore, testpage.id, "", true, false)) + "\">" + pageCaption + "</a>" + BreadCrumbDelimiter + returnHtml;
				}
			}
			return returnHtml;
		}
		//
		//========================================================================
		// ----- Process the reply from the Authoring Tools Panel form
		//========================================================================
		//
		public void processFormQuickEditing()
		{
			//
			int RecordParentID = 0;
			bool SaveButNoChanges = false;
			int ParentID = 0;
			string Link = null;
			int CSBlock = 0;
			string Copy = null;
			string Button = null;
			int RecordID = 0;
			bool RecordModified = false;
			string RecordName = string.Empty;
			//
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
			RecordModified = false;
			RecordID = (cpcore.docProperties.getInteger("ID"));
			Button = cpcore.docProperties.getText("Button");
			iIsAdmin = cpcore.doc.authContext.isAuthenticatedAdmin(cpcore);
			//
			if ((!string.IsNullOrEmpty(Button)) & (RecordID != 0) & (pageContentModel.contentName != "") & (cpcore.doc.authContext.isAuthenticatedContentManager(cpcore, pageContentModel.contentName)))
			{
				// main_WorkflowSupport = cpcore.siteProperties.allowWorkflowAuthoring And cpcore.workflow.isWorkflowAuthoringCompatible(pageContentModel.contentName)
				string SubmittedMemberName = "";
				string ApprovedMemberName = "";
				string ModifiedMemberName = "";
				getAuthoringStatus(pageContentModel.contentName, RecordID, ref IsSubmitted, ref IsApproved, ref SubmittedMemberName, ref ApprovedMemberName, ref IsInserted, ref IsDeleted, ref IsModified, ref ModifiedMemberName, ref ModifiedDate, ref SubmittedDate, ref ApprovedDate);
				IsEditLocked = cpcore.workflow.GetEditLockStatus(pageContentModel.contentName, RecordID);
				main_EditLockMemberName = cpcore.workflow.GetEditLockMemberName(pageContentModel.contentName, RecordID);
				main_EditLockDateExpires = cpcore.workflow.GetEditLockDateExpires(pageContentModel.contentName, RecordID);
				cpcore.workflow.ClearEditLock(pageContentModel.contentName, RecordID);
				//
				// tough case, in Quick mode, lets mark the record reviewed, no matter what button they push, except cancel
				//
				if (Button != ButtonCancel)
				{
					cpcore.doc.markRecordReviewed(pageContentModel.contentName, RecordID);
				}
				//
				// Determine is the record should be saved
				//
				if ((!IsApproved) && (!cpcore.docProperties.getBoolean("RENDERMODE")))
				{
					if (iIsAdmin)
					{
						//
						// cases that admin can save
						//
						allowSave = false || (Button == ButtonAddChildPage) || (Button == ButtonAddSiblingPage) || (Button == ButtonSave) || (Button == ButtonOK);
					}
					else
					{
						//
						// cases that CM can save
						//
						allowSave = false || (Button == ButtonAddChildPage) || (Button == ButtonAddSiblingPage) || (Button == ButtonSave) || (Button == ButtonOK);
					}
				}
				if (allowSave)
				{
					//
					// ----- Save Changes
					SaveButNoChanges = true;
					pageContentModel page = pageContentModel.create(cpcore, RecordID);
					if (page != null)
					{
						Copy = cpcore.docProperties.getText("copyFilename");
						Copy = cpcore.html.convertEditorResponseToActiveContent(Copy);
						if (Copy != page.Copyfilename.content)
						{
							page.Copyfilename.content = Copy;
							SaveButNoChanges = false;
						}
						RecordName = cpcore.docProperties.getText("name");
						if (RecordName != page.name)
						{
							page.name = RecordName;
							docController.addLinkAlias(cpcore, RecordName, RecordID, "");
							SaveButNoChanges = false;
						}
						RecordParentID = page.ParentID;
						page.save(cpcore);
						//
						cpcore.workflow.SetEditLock(pageContentModel.contentName, page.id);
						//
						if (!SaveButNoChanges)
						{
							cpcore.doc.processAfterSave(false, pageContentModel.contentName, page.id, page.name, page.ParentID, false);
							cpcore.cache.invalidateAllObjectsInContent(pageContentModel.contentName);
						}
					}
				}
				if (Button == ButtonAddChildPage)
				{
					//
					//
					//
					CSBlock = cpcore.db.csInsertRecord(pageContentModel.contentName);
					if (cpcore.db.csOk(CSBlock))
					{
						cpcore.db.csSet(CSBlock, "active", true);
						cpcore.db.csSet(CSBlock, "ParentID", RecordID);
						cpcore.db.csSet(CSBlock, "contactmemberid", cpcore.doc.authContext.user.id);
						cpcore.db.csSet(CSBlock, "name", "New Page added " + cpcore.doc.profileStartTime + " by " + cpcore.doc.authContext.user.name);
						cpcore.db.csSet(CSBlock, "copyFilename", "");
						RecordID = cpcore.db.csGetInteger(CSBlock, "ID");
						cpcore.db.csSave2(CSBlock);
						//
						Link = pageContentController.getPageLink(cpcore, RecordID, "", true, false);
						//Link = main_GetPageLink(RecordID)
						//If main_WorkflowSupport Then
						//    If Not cpCore.doc.authContext.isWorkflowRendering() Then
						//        Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "This new unpublished page has been added and Workflow Rendering has been enabled so you can edit this page.", True)
						//        Call cpcore.siteProperties.setProperty("AllowWorkflowRendering", True)
						//    End If
						//End If
						cpcore.webServer.redirect(Link, "Redirecting because a new page has been added with the quick editor.",, false);
					}
					cpcore.db.csClose(CSBlock);
					//
					cpcore.cache.invalidateAllObjectsInContent(pageContentModel.contentName);
				}
				if (Button == ButtonAddSiblingPage)
				{
					//
					//
					//
					CSBlock = cpcore.db.csOpenRecord(pageContentModel.contentName, RecordID,,, "ParentID");
					if (cpcore.db.csOk(CSBlock))
					{
						ParentID = cpcore.db.csGetInteger(CSBlock, "ParentID");
					}
					cpcore.db.csClose(CSBlock);
					if (ParentID != 0)
					{
						CSBlock = cpcore.db.csInsertRecord(pageContentModel.contentName);
						if (cpcore.db.csOk(CSBlock))
						{
							cpcore.db.csSet(CSBlock, "active", true);
							cpcore.db.csSet(CSBlock, "ParentID", ParentID);
							cpcore.db.csSet(CSBlock, "contactmemberid", cpcore.doc.authContext.user.id);
							cpcore.db.csSet(CSBlock, "name", "New Page added " + cpcore.doc.profileStartTime + " by " + cpcore.doc.authContext.user.name);
							cpcore.db.csSet(CSBlock, "copyFilename", "");
							RecordID = cpcore.db.csGetInteger(CSBlock, "ID");
							cpcore.db.csSave2(CSBlock);
							//
							Link = pageContentController.getPageLink(cpcore, RecordID, "", true, false);
							cpcore.webServer.redirect(Link, "Redirecting because a new page has been added with the quick editor.", false, false);
						}
						cpcore.db.csClose(CSBlock);
					}
					cpcore.cache.invalidateAllObjectsInContent(pageContentModel.contentName);
				}
				if (Button == ButtonDelete)
				{
					CSBlock = cpcore.db.csOpenRecord(pageContentModel.contentName, RecordID);
					if (cpcore.db.csOk(CSBlock))
					{
						ParentID = cpcore.db.csGetInteger(CSBlock, "parentid");
					}
					cpcore.db.csClose(CSBlock);
					//
					deleteChildRecords(pageContentModel.contentName, RecordID, false);
					cpcore.db.deleteContentRecord(pageContentModel.contentName, RecordID);
					//
					if (!false)
					{
						cpcore.cache.invalidateAllObjectsInContent(pageContentModel.contentName);
					}
					//
					if (!false)
					{
						Link = pageContentController.getPageLink(cpcore, ParentID, "", true, false);
						Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "The page has been deleted, and you have been redirected to the parent of the deleted page.", true);
						cpcore.webServer.redirect(Link, "Redirecting to the parent page because the page was deleted with the quick editor.", redirectBecausePageNotFound, false);
						return;
					}
				}
				//
				//If (Button = ButtonAbortEdit) Then
				//    Call cpcore.workflow.abortEdit2(pageContentModel.contentName, RecordID, cpCore.doc.authContext.user.id)
				//End If
				//If (Button = ButtonPublishSubmit) Then
				//    Call cpcore.workflow.main_SubmitEdit(pageContentModel.contentName, RecordID)
				//    Call sendPublishSubmitNotice(pageContentModel.contentName, RecordID, "")
				//End If
				if ((!(cpcore.doc.debug_iUserError != "")) & ((Button == ButtonOK) || (Button == ButtonCancel)))
				{
					//
					// ----- Turn off Quick Editor if not save or add child
					//
					cpcore.visitProperty.setProperty("AllowQuickEditor", "0");
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
		public string getChildPageList(string RequestedListName, string ContentName, int parentPageID, bool allowChildListDisplay, bool ArchivePages = false)
		{
			string result = "";
			try
			{
				if (string.IsNullOrEmpty(ContentName))
				{
					ContentName = pageContentModel.contentName;
				}
				object isAuthoring = cpcore.doc.authContext.isEditing(ContentName);
				//
				int ChildListCount = 0;
				string UcaseRequestedListName = genericController.vbUCase(RequestedListName);
				if ((UcaseRequestedListName == "NONE") || (UcaseRequestedListName == "ORPHAN"))
				{
					UcaseRequestedListName = "";
				}
				//
				string archiveLink = cpcore.webServer.requestPathPage;
				archiveLink = genericController.ConvertLinkToShortLink(archiveLink, cpcore.webServer.requestDomain, cpcore.webServer.requestVirtualFilePath);
				archiveLink = genericController.EncodeAppRootPath(archiveLink, cpcore.webServer.requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain);
				//
				string sqlCriteria = "(parentId=" + page.id + ")";
				string sqlOrderBy = "sortOrder";
				List<pageContentModel> childPageList = pageContentModel.createList(cpcore, sqlCriteria, sqlOrderBy);
				string inactiveList = "";
				string activeList = "";
				foreach (pageContentModel childPage in childPageList)
				{
					string PageLink = pageContentController.getPageLink(cpcore, childPage.id, "", true, false);
					string pageMenuHeadline = childPage.MenuHeadline;
					if (string.IsNullOrEmpty(pageMenuHeadline))
					{
						pageMenuHeadline = childPage.name.Trim(' ');
						if (string.IsNullOrEmpty(pageMenuHeadline))
						{
							pageMenuHeadline = "Related Page";
						}
					}
					string pageEditLink = "";
					if (cpcore.doc.authContext.isEditing(ContentName))
					{
						pageEditLink = cpcore.html.main_GetRecordEditLink2(ContentName, childPage.id, true, childPage.name, true);
					}
					//
					string link = PageLink;
					if (ArchivePages)
					{
						link = genericController.modifyLinkQuery(archiveLink, rnPageId, Convert.ToString(childPage.id), true);
					}
					bool blockContentComposite = false;
					if (childPage.BlockContent | childPage.BlockPage)
					{
						blockContentComposite = !bypassContentBlock(childPage.ContentControlID, childPage.id);
					}
					string LinkedText = genericController.csv_GetLinkedText("<a href=\"" + genericController.encodeHTML(link) + "\">", pageMenuHeadline);
					if ((string.IsNullOrEmpty(UcaseRequestedListName)) && (childPage.ParentListName != "") & (!isAuthoring))
					{
						//
						// ----- Requested orphan list, and this record is in a named list, and not editing, do not display
						//
					}
					else if ((string.IsNullOrEmpty(UcaseRequestedListName)) && (childPage.ParentListName != ""))
					{
						//
						// ----- Requested orphan list, and this record is in a named list, but authoring, list it
						//
						if (isAuthoring)
						{
							inactiveList = inactiveList + "\r" + "<li name=\"page" + childPage.id + "\" name=\"page" + childPage.id + "\"  id=\"page" + childPage.id + "\" class=\"ccListItem\">";
							inactiveList = inactiveList + pageEditLink;
							inactiveList = inactiveList + "[from Child Page List '" + childPage.ParentListName + "': " + LinkedText + "]";
							inactiveList = inactiveList + "</li>";
						}
					}
					else if ((string.IsNullOrEmpty(UcaseRequestedListName)) && (!allowChildListDisplay) && (!isAuthoring))
					{
						//
						// ----- Requested orphan List, Not AllowChildListDisplay, not Authoring, do not display
						//
					}
					else if ((!string.IsNullOrEmpty(UcaseRequestedListName)) & (UcaseRequestedListName != genericController.vbUCase(childPage.ParentListName)))
					{
						//
						// ----- requested named list and wrong RequestedListName, do not display
						//
					}
					else if (!childPage.AllowInChildLists)
					{
						//
						// ----- Allow in Child Page Lists is false, display hint to authors
						//
						if (isAuthoring)
						{
							inactiveList = inactiveList + "\r" + "<li name=\"page" + childPage.id + "\"  id=\"page" + childPage.id + "\" class=\"ccListItem\">";
							inactiveList = inactiveList + pageEditLink;
							inactiveList = inactiveList + "[Hidden (Allow in Child Lists is not checked): " + LinkedText + "]";
							inactiveList = inactiveList + "</li>";
						}
					}
					else if (!childPage.Active)
					{
						//
						// ----- Not active record, display hint if authoring
						//
						if (isAuthoring)
						{
							inactiveList = inactiveList + "\r" + "<li name=\"page" + childPage.id + "\"  id=\"page" + childPage.id + "\" class=\"ccListItem\">";
							inactiveList = inactiveList + pageEditLink;
							inactiveList = inactiveList + "[Hidden (Inactive): " + LinkedText + "]";
							inactiveList = inactiveList + "</li>";
						}
					}
					else if ((childPage.PubDate != DateTime.MinValue) && (childPage.PubDate > cpcore.doc.profileStartTime))
					{
						//
						// ----- Child page has not been published
						//
						if (isAuthoring)
						{
							inactiveList = inactiveList + "\r" + "<li name=\"page" + childPage.id + "\"  id=\"page" + childPage.id + "\" class=\"ccListItem\">";
							inactiveList = inactiveList + pageEditLink;
							inactiveList = inactiveList + "[Hidden (To be published " + childPage.PubDate + "): " + LinkedText + "]";
							inactiveList = inactiveList + "</li>";
						}
					}
					else if ((childPage.DateExpires != DateTime.MinValue) && (childPage.DateExpires < cpcore.doc.profileStartTime))
					{
						//
						// ----- Child page has expired
						//
						if (isAuthoring)
						{
							inactiveList = inactiveList + "\r" + "<li name=\"page" + childPage.id + "\"  id=\"page" + childPage.id + "\" class=\"ccListItem\">";
							inactiveList = inactiveList + pageEditLink;
							inactiveList = inactiveList + "[Hidden (Expired " + childPage.DateExpires + "): " + LinkedText + "]";
							inactiveList = inactiveList + "</li>";
						}
					}
					else
					{
						//
						// ----- display list (and authoring links)
						//
						activeList = activeList + "\r" + "<li name=\"page" + childPage.id + "\"  id=\"page" + childPage.id + "\" class=\"ccListItem\">";
						if (!string.IsNullOrEmpty(pageEditLink))
						{
							activeList = activeList + pageEditLink + "&nbsp;";
						}
						activeList = activeList + LinkedText;
						//
						// include authoring mark for content block
						//
						if (isAuthoring)
						{
							if (childPage.BlockContent)
							{
								activeList = activeList + "&nbsp;[Content Blocked]";
							}
							if (childPage.BlockPage)
							{
								activeList = activeList + "&nbsp;[Page Blocked]";
							}
						}
						//
						// include overview
						// if AllowBrief is false, BriefFilename is not loaded
						//
						if ((childPage.BriefFilename != "") & (childPage.AllowBrief))
						{
							string Brief = Convert.ToString(cpcore.cdnFiles.readFile(childPage.BriefFilename)).Trim(' ');
							if (!string.IsNullOrEmpty(Brief))
							{
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
				if (!ArchivePages)
				{
					string AddLink = cpcore.html.main_GetRecordAddLink(ContentName, "parentid=" + parentPageID + ",ParentListName=" + UcaseRequestedListName, true);
					if (!string.IsNullOrEmpty(AddLink))
					{
						inactiveList = inactiveList + "\r" + "<li class=\"ccListItem\">" + AddLink + "</LI>";
					}
				}
				//
				// ----- If there is a list, add the list start and list end
				//
				result = "";
				if (!string.IsNullOrEmpty(activeList))
				{
					result = result + "\r" + "<ul id=\"childPageList_" + parentPageID + "_" + RequestedListName + "\" class=\"ccChildList\">" + genericController.htmlIndent(activeList) + "\r" + "</ul>";
				}
				if (!string.IsNullOrEmpty(inactiveList))
				{
					result = result + "\r" + "<ul id=\"childPageList_" + parentPageID + "_" + RequestedListName + "\" class=\"ccChildListInactive\">" + genericController.htmlIndent(inactiveList) + "\r" + "</ul>";
				}
				//
				// ----- if non-orphan list, authoring and none found, print none message
				//
				if ((!string.IsNullOrEmpty(UcaseRequestedListName)) & (ChildListCount == 0) & isAuthoring)
				{
					result = "[Child Page List with no pages]</p><p>" + result;
				}
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
			}
			return result;
		}
		//
		//=============================================================================
		//   BypassContentBlock
		//       Is This member allowed through the content block
		//=============================================================================
		//
		public bool bypassContentBlock(int ContentID, int RecordID)
		{
				bool tempbypassContentBlock = false;
			try
			{
				//
				int CS = 0;
				string SQL = null;
				//
				if (cpcore.doc.authContext.isAuthenticatedAdmin(cpcore))
				{
					tempbypassContentBlock = true;
				}
				else if (cpcore.doc.authContext.isAuthenticatedContentManager(cpcore, Models.Complex.cdefModel.getContentNameByID(cpcore, ContentID)))
				{
					tempbypassContentBlock = true;
				}
				else
				{
					SQL = "SELECT ccMemberRules.MemberID"
						+ " FROM (ccPageContentBlockRules LEFT JOIN ccgroups ON ccPageContentBlockRules.GroupID = ccgroups.ID) LEFT JOIN ccMemberRules ON ccgroups.ID = ccMemberRules.GroupID"
						+ " WHERE (((ccPageContentBlockRules.RecordID)=" + RecordID + ")"
						+ " AND ((ccPageContentBlockRules.Active)<>0)"
						+ " AND ((ccgroups.Active)<>0)"
						+ " AND ((ccMemberRules.Active)<>0)"
						+ " AND ((ccMemberRules.DateExpires) Is Null Or (ccMemberRules.DateExpires)>" + cpcore.db.encodeSQLDate(cpcore.doc.profileStartTime) + ")"
						+ " AND ((ccMemberRules.MemberID)=" + cpcore.doc.authContext.user.id + "));";
					CS = cpcore.db.csOpenSql(SQL);
					tempbypassContentBlock = cpcore.db.csOk(CS);
					cpcore.db.csClose(CS);
				}
				//
				return tempbypassContentBlock;
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError13("IsContentBlocked")
			return tempbypassContentBlock;
		}
		//'
		//'====================================================================================================
		//'   main_GetTemplateLink
		//'       Added to externals (aoDynamicMenu) can main_Get hard template links
		//'====================================================================================================
		//'
		//Public Function getTemplateLink(templateId As Integer) As String
		//    Dim template As pageTemplateModel = pageTemplateModel.create(cpcore, templateId, New List(Of String))
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
		public string deleteChildRecords(string ContentName, int RecordID, bool ReturnListWithoutDelete = false)
		{
			string result = string.Empty;
			try
			{
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
				CS = cpcore.db.csOpen(ContentName, "parentid=" + RecordID,,,,,, "ID");
				while (cpcore.db.csOk(CS))
				{
					result = result + "," + cpcore.db.csGetInteger(CS, "ID");
					cpcore.db.csGoNext(CS);
				}
				cpcore.db.csClose(CS);
				if (!string.IsNullOrEmpty(result))
				{
					result = result.Substring(1);
					//
					// main_Get a list of all pages, but do not delete anything yet
					//
					IDs = result.Split(',');
					IDCnt = IDs.GetUpperBound(0) + 1;
					SingleEntry = (IDCnt == 1);
					for (Ptr = 0; Ptr < IDCnt; Ptr++)
					{
						ChildList = deleteChildRecords(ContentName, genericController.EncodeInteger(IDs[Ptr]), true);
						if (!string.IsNullOrEmpty(ChildList))
						{
							result = result + "," + ChildList;
							SingleEntry = false;
						}
					}
					if (!ReturnListWithoutDelete)
					{
						//
						// Do the actual delete
						//
						IDs = result.Split(',');
						IDCnt = IDs.GetUpperBound(0) + 1;
						SingleEntry = (IDCnt == 1);
						QuickEditing = cpcore.doc.authContext.isQuickEditing(cpcore, "page content");
						for (Ptr = 0; Ptr < IDCnt; Ptr++)
						{
							cpcore.db.deleteContentRecord("page content", genericController.EncodeInteger(IDs[Ptr]));
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
			}
			return result;
		}
		//
		//========================================================================
		//
		//========================================================================
		//
		public void getAuthoringStatus(string ContentName, int RecordID, ref bool IsSubmitted, ref bool IsApproved, ref string SubmittedName, ref string ApprovedName, ref bool IsInserted, ref bool IsDeleted, ref bool IsModified, ref string ModifiedName, ref DateTime ModifiedDate, ref DateTime SubmittedDate, ref DateTime ApprovedDate)
		{
			try
			{
				//
				//If Not (true) Then Exit Sub
				//
				string MethodName;
				//
				MethodName = "main_GetAuthoringStatus";
				//
				cpcore.workflow.getAuthoringStatus(ContentName, RecordID, IsSubmitted, IsApproved, SubmittedName, ApprovedName, IsInserted, IsDeleted, IsModified, ModifiedName, ModifiedDate, SubmittedDate, ApprovedDate);
				//
				return;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
			//
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
		public void getAuthoringPermissions(string ContentName, int RecordID, ref bool AllowInsert, ref bool AllowCancel, ref bool allowSave, ref bool AllowDelete, ref bool ignore1, ref bool ignore2, ref bool ignore3, ref bool ignore4, ref bool readOnlyField)
		{
			try
			{
				//
				//If Not (true) Then Exit Sub
				//
				string MethodName = null;
				//
				bool IsEditing = false;
				bool IsSubmitted = false;
				bool IsApproved = false;
				bool IsInserted = false;
				bool IsDeleted = false;
				bool IsModified = false;
				string EditingName = null;
				DateTime EditingExpires = default(DateTime);
				string SubmittedName = string.Empty;
				string ApprovedName = string.Empty;
				string ModifiedName = string.Empty;
				Models.Complex.cdefModel CDef = null;
				DateTime ModifiedDate = default(DateTime);
				DateTime SubmittedDate = default(DateTime);
				DateTime ApprovedDate = default(DateTime);
				//
				MethodName = "main_GetAuthoringButtons";
				//
				// main_Get Authoring Workflow Status
				//
				if (RecordID != 0)
				{
					cpcore.workflow.getAuthoringStatus(ContentName, RecordID, IsSubmitted, IsApproved, SubmittedName, ApprovedName, IsInserted, IsDeleted, IsModified, ModifiedName, ModifiedDate, SubmittedDate, ApprovedDate);
				}
				//
				// main_Get Content Definition
				//
				CDef = Models.Complex.cdefModel.getCdef(cpcore, ContentName);
				//
				// Set Buttons based on Status
				//
				if (IsEditing)
				{
					//
					// Edit Locked
					//
					AllowCancel = true;
					readOnlyField = true;
				}
				else if ((!false) || (!false))
				{
					//
					// No Workflow Authoring
					//
					AllowCancel = true;
					allowSave = true;
					if ((CDef.AllowDelete) & (!IsDeleted) & (RecordID != 0))
					{
						AllowDelete = true;
					}
					if ((CDef.AllowAdd) && (!IsInserted))
					{
						AllowInsert = true;
					}
					//ElseIf cpCore.doc.authContext.isAuthenticatedAdmin(cpcore) Then
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
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
			//
		}
		//
		//
		//
		public void sendPublishSubmitNotice(string ContentName, int RecordID, string RecordName)
		{
			try
			{
				//
				//If Not (true) Then Exit Sub
				//
				string MethodName = null;
				Models.Complex.cdefModel CDef = null;
				string Copy = null;
				string Link = null;
				string FromAddress = null;
				//
				MethodName = "main_SendPublishSubmitNotice";
				//
				FromAddress = cpcore.siteProperties.getText("EmailPublishSubmitFrom", cpcore.siteProperties.emailAdmin);
				CDef = Models.Complex.cdefModel.getCdef(cpcore, ContentName);
				Link = "/" + cpcore.serverConfig.appConfig.adminRoute + "?af=" + AdminFormPublishing;
				Copy = Msg_AuthoringSubmittedNotification;
				Copy = genericController.vbReplace(Copy, "<DOMAINNAME>", "<a href=\"" + genericController.encodeHTML(Link) + "\">" + cpcore.webServer.requestDomain + "</a>");
				Copy = genericController.vbReplace(Copy, "<RECORDNAME>", RecordName);
				Copy = genericController.vbReplace(Copy, "<CONTENTNAME>", ContentName);
				Copy = genericController.vbReplace(Copy, "<RECORDID>", RecordID.ToString());
				Copy = genericController.vbReplace(Copy, "<SUBMITTEDDATE>", cpcore.doc.profileStartTime.ToString());
				Copy = genericController.vbReplace(Copy, "<SUBMITTEDNAME>", cpcore.doc.authContext.user.name);
				//
				cpcore.email.sendGroup(cpcore.siteProperties.getText("WorkflowEditorGroup", "Site Managers"), FromAddress, "Authoring Submitted Notification", Copy, false, true);
				//
				return;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
			//
		}
		//
		//=============================================================================
		//   main_Get the link for a Content Record by the ContentName and RecordID
		//=============================================================================
		//
		public string getContentWatchLinkByName(string ContentName, int RecordID, string DefaultLink = "", bool IncrementClicks = true)
		{
			try
			{
				//
				//If Not (true) Then Exit Function
				//
				string ContentRecordKey;
				//
				ContentRecordKey = Models.Complex.cdefModel.getContentId(cpcore, genericController.encodeText(ContentName)) + "." + genericController.EncodeInteger(RecordID);
				return getContentWatchLinkByKey(ContentRecordKey, DefaultLink, IncrementClicks);
				//
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetContentWatchLinkByName")
		}
		//
		//=============================================================================
		//   main_Get the link for a Content Record by its ContentRecordKey
		//=============================================================================
		//
		public string getContentWatchLinkByKey(string ContentRecordKey, string DefaultLink = "", bool IncrementClicks = false)
		{
				string tempgetContentWatchLinkByKey = null;
			try
			{
				//
				//If Not (true) Then Exit Function
				//
				int CSPointer;
				//
				// Lookup link in main_ContentWatch
				//
				CSPointer = cpcore.db.csOpen("Content Watch", "ContentRecordKey=" + cpcore.db.encodeSQLText(ContentRecordKey),,,,,, "Link,Clicks");
				if (cpcore.db.csOk(CSPointer))
				{
					tempgetContentWatchLinkByKey = cpcore.db.csGetText(CSPointer, "Link");
					if (genericController.EncodeBoolean(IncrementClicks))
					{
						cpcore.db.csSet(CSPointer, "Clicks", cpcore.db.csGetInteger(CSPointer, "clicks") + 1);
					}
				}
				else
				{
					tempgetContentWatchLinkByKey = genericController.encodeText(DefaultLink);
				}
				cpcore.db.csClose(CSPointer);
				//
				return genericController.EncodeAppRootPath(tempgetContentWatchLinkByKey, cpcore.webServer.requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain);
				//
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetContentWatchLinkByKey")
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
		internal int getPageSectionId(int PageID, ref List<int> UsedIDList, Dictionary<int, int> siteSectionRootPageIndex)
		{
			int sectionId = 0;
			try
			{
				Models.Entity.pageContentModel page = Models.Entity.pageContentModel.create(cpcore, PageID, new List<string>());
				if (page != null)
				{
					if ((page.ParentID == 0) && (!UsedIDList.Contains(page.ParentID)))
					{
						UsedIDList.Add(page.ParentID);
						if (siteSectionRootPageIndex.ContainsKey(page.ParentID))
						{
							sectionId = siteSectionRootPageIndex(page.ParentID);
						}
					}
					else
					{
						sectionId = getPageSectionId(page.ParentID, ref UsedIDList, siteSectionRootPageIndex);
					}
				}
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
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
		internal int main_GetPageDynamicLink_GetTemplateID(int PageID, string UsedIDList)
		{
				int tempmain_GetPageDynamicLink_GetTemplateID = 0;
			try
			{
				//
				int CS = 0;
				int ParentID = 0;
				int templateId = 0;
				//
				//
				CS = cpcore.db.csOpenRecord("Page Content", PageID,,, "TemplateID,ParentID");
				if (cpcore.db.csOk(CS))
				{
					templateId = cpcore.db.csGetInteger(CS, "TemplateID");
					ParentID = cpcore.db.csGetInteger(CS, "ParentID");
				}
				cpcore.db.csClose(CS);
				//
				// Chase page tree to main_Get templateid
				//
				if (templateId == 0 && ParentID != 0)
				{
					if (!genericController.IsInDelimitedString(UsedIDList, ParentID.ToString(), ","))
					{
						tempmain_GetPageDynamicLink_GetTemplateID = main_GetPageDynamicLink_GetTemplateID(ParentID, UsedIDList + "," + ParentID);
					}
				}
				//
				return tempmain_GetPageDynamicLink_GetTemplateID;
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError13("main_GetPageDynamicLink_GetTemplateID")
			return tempmain_GetPageDynamicLink_GetTemplateID;
		}
		//
		//====================================================================================================
		// main_Get a page link if you know nothing about the page
		//   If you already have all the info, lik the parents templateid, etc, call the ...WithArgs call
		//====================================================================================================
		//
		public string main_GetPageDynamicLink(int PageID, bool UseContentWatchLink)
		{
			//
			int CCID = 0;
			string DefaultLink = null;
			int SectionID = 0;
			bool IsRootPage = false;
			int templateId = 0;
			string MenuLinkOverRide = string.Empty;
			//
			//
			// Convert default page to default link
			//
			DefaultLink = cpcore.siteProperties.serverPageDefault;
			if (DefaultLink.Substring(0, 1) != "/")
			{
				DefaultLink = "/" + cpcore.siteProperties.serverPageDefault;
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
		internal string main_GetPageDynamicLinkWithArgs(int ContentControlID, int PageID, string DefaultLink, bool IsRootPage, int templateId, int SectionID, string MenuLinkOverRide, bool UseContentWatchLink)
		{
			string resultLink = "";
			try
			{
				if (!string.IsNullOrEmpty(MenuLinkOverRide))
				{
					//
					// -- redirect to this page record
					resultLink = "?rc=" + ContentControlID + "&ri=" + PageID;
				}
				else
				{
					if (UseContentWatchLink)
					{
						//
						// -- Legacy method - lookup link from a table set during the last page hit
						resultLink = getContentWatchLinkByID(ContentControlID, PageID, DefaultLink, false);
					}
					else
					{
						//
						// -- Current method - all pages are in the Template, Section, Page structure
						if (templateId != 0)
						{
							pageTemplateModel template = pageTemplateModel.create(cpcore, templateId, new List<string>());
							if (template != null)
							{
								resultLink = ""; // template.Link
							}
						}
						if (string.IsNullOrEmpty(resultLink))
						{
							//
							// -- not found, use default
							if (!string.IsNullOrEmpty(DefaultLink))
							{
								//
								// if default given, use that
								resultLink = DefaultLink;
							}
							else
							{
								//
								// -- fallback, use content watch
								resultLink = getContentWatchLinkByID(ContentControlID, PageID, "", false);
							}
						}
						if ((PageID == 0) || (IsRootPage))
						{
							//
							// -- Link to Root Page, no bid, and include sectionid if not 0
							if (IsRootPage && (SectionID != 0))
							{
								resultLink = genericController.modifyLinkQuery(resultLink, "sid", SectionID.ToString(), true);
							}
							resultLink = genericController.modifyLinkQuery(resultLink, rnPageId, "", false);
						}
						else
						{
							resultLink = genericController.modifyLinkQuery(resultLink, rnPageId, genericController.encodeText(PageID), true);
							if (PageID != 0)
							{
								resultLink = genericController.modifyLinkQuery(resultLink, "sid", "", false);
							}
						}
					}
				}
				resultLink = genericController.EncodeAppRootPath(resultLink, cpcore.webServer.requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain);
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
				throw;
			}
			return resultLink;
		}
		//
		//=============================================================================
		//   main_Get the link for a Content Record by the ContentID and RecordID
		//=============================================================================
		//
		public string getContentWatchLinkByID(int ContentID, int RecordID, string DefaultLink = "", bool IncrementClicks = true)
		{
			return getContentWatchLinkByKey(genericController.encodeText(ContentID) + "." + genericController.encodeText(RecordID), DefaultLink, IncrementClicks);
		}
		//
		//=============================================================================
		//
		public void verifyRegistrationFormPage(coreClass cpcore)
		{
			try
			{
				//
				int CS = 0;
				string GroupNameList = null;
				string Copy = null;
				//
				cpcore.db.deleteContentRecords("Form Pages", "name=" + cpcore.db.encodeSQLText("Registration Form"));
				CS = cpcore.db.csOpen("Form Pages", "name=" + cpcore.db.encodeSQLText("Registration Form"));
				if (!cpcore.db.csOk(CS))
				{
					//
					// create Version 1 template - just to main_Get it started
					//
					cpcore.db.csClose(CS);
					GroupNameList = "Registered";
					CS = cpcore.db.csInsertRecord("Form Pages");
					if (cpcore.db.csOk(CS))
					{
						cpcore.db.csSet(CS, "name", "Registration Form");
						Copy = ""
						+ Environment.NewLine + "<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" width=\"100%\">"
						+ Environment.NewLine + "{{REPEATSTART}}<tr><td align=right style=\"height:22px;\">{{CAPTION}}&nbsp;</td><td align=left>{{FIELD}}</td></tr>{{REPEATEND}}"
						+ Environment.NewLine + "<tr><td align=right><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=135 height=1></td><td width=\"100%\">&nbsp;</td></tr>"
						+ Environment.NewLine + "<tr><td colspan=2>&nbsp;<br>" + cpcore.html.main_GetPanelButtons(ButtonRegister, "Button") + "</td></tr>"
						+ Environment.NewLine + "</table>";
						cpcore.db.csSet(CS, "Body", Copy);
						Copy = ""
						+ "1"
						+ Environment.NewLine + GroupNameList + Environment.NewLine + "true"
						+ Environment.NewLine + "1,First Name,true,FirstName"
						+ Environment.NewLine + "1,Last Name,true,LastName"
						+ Environment.NewLine + "1,Email Address,true,Email"
						+ Environment.NewLine + "1,Phone,true,Phone"
						+ Environment.NewLine + "2,Please keep me informed of news and events,false,Subscribers"
						+ "";
						cpcore.db.csSet(CS, "Instructions", Copy);
					}
				}
				cpcore.db.csClose(CS);
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
			}
		}
		//
		//---------------------------------------------------------------------------
		//   Create the default landing page if it is missing
		//---------------------------------------------------------------------------
		//
		public int createPageGetID(string PageName, string ContentName, int CreatedBy, string pageGuid)
		{
			int Id = 0;
			//
			int CS = cpcore.db.csInsertRecord(ContentName, CreatedBy);
			if (cpcore.db.csOk(CS))
			{
				Id = cpcore.db.csGetInteger(CS, "ID");
				cpcore.db.csSet(CS, "name", PageName);
				cpcore.db.csSet(CS, "active", "1");
				if (true)
				{
					cpcore.db.csSet(CS, "ccGuid", pageGuid);
				}
				cpcore.db.csSave2(CS);
				//   Call cpcore.workflow.publishEdit("Page Content", Id)
			}
			cpcore.db.csClose(CS);
			//
			return Id;
		}
		//
		//====================================================================================================
		//   Returns the Alias link (SourceLink) from the actual link (DestinationLink)
		//
		//====================================================================================================
		//
		public static string getLinkAlias(coreClass cpcore, int PageID, string QueryStringSuffix, string DefaultLink)
		{
			string linkAlias = DefaultLink;
			List<Models.Entity.linkAliasModel> linkAliasList = Models.Entity.linkAliasModel.createList(cpcore, PageID, QueryStringSuffix);
			if (linkAliasList.Count > 0)
			{
				linkAlias = linkAliasList.First.name;
				if (linkAlias.Substring(0, 1) != "/")
				{
					linkAlias = "/" + linkAlias;
				}
			}
			return linkAlias;
		}
		//
		//=================================================================================================================================================
		//   csv_addLinkAlias
		//
		//   Link Alias
		//       A LinkAlias name is a unique string that identifies a page on the site.
		//       A page on the site is generated from the PageID, and the QueryStringSuffix
		//       PageID - obviously, this is the ID of the page
		//       QueryStringSuffix - other things needed on the Query to display the correct content.
		//           The Suffix is needed in cases like when an Add-on is embedded in a page. The URL to that content becomes the pages
		//           Link, plus the suffix needed to find the content.
		//
		//       When you make the menus, look up the most recent Link Alias entry with the pageID, and a blank QueryStringSuffix
		//
		//   The Link Alias table no longer needs the Link field.
		//'
		//'=================================================================================================================================================
		//'
		//' +++++ 9/8/2011 4.1.482, added csv_addLinkAlias to csv and changed main to call
		//'
		//Public Shared Sub app_addLinkAlias(cpcore As coreClass, linkAlias As String, PageID As Integer, QueryStringSuffix As String)
		//    Dim return_ignoreError As String = ""
		//    Call app_addLinkAlias2(cpcore, linkAlias, PageID, QueryStringSuffix, True, False, return_ignoreError)
		//End Sub
		//
		// +++++ 9/8/2011 4.1.482, added csv_addLinkAlias to csv and changed main to call
		//
		public static void addLinkAlias(coreClass cpcore, string linkAlias, int PageID, string QueryStringSuffix, bool OverRideDuplicate, bool DupCausesWarning)
		{
			string tempVar = "";
			addLinkAlias(cpcore, linkAlias, PageID, QueryStringSuffix, OverRideDuplicate, DupCausesWarning, ref tempVar);
		}

		public static void addLinkAlias(coreClass cpcore, string linkAlias, int PageID, string QueryStringSuffix, bool OverRideDuplicate)
		{
			string tempVar = "";
			addLinkAlias(cpcore, linkAlias, PageID, QueryStringSuffix, OverRideDuplicate, false, ref tempVar);
		}

		public static void addLinkAlias(coreClass cpcore, string linkAlias, int PageID, string QueryStringSuffix)
		{
			string tempVar = "";
			addLinkAlias(cpcore, linkAlias, PageID, QueryStringSuffix, false, false, ref tempVar);
		}

//INSTANT C# NOTE: Overloaded method(s) are created above to convert the following method having optional parameters:
//ORIGINAL LINE: Public Shared Sub addLinkAlias(cpcore As coreClass, linkAlias As String, PageID As Integer, QueryStringSuffix As String, Optional OverRideDuplicate As Boolean = False, Optional DupCausesWarning As Boolean = False, Optional ByRef return_WarningMessage As String = "")
		public static void addLinkAlias(coreClass cpcore, string linkAlias, int PageID, string QueryStringSuffix, bool OverRideDuplicate, bool DupCausesWarning, ref string return_WarningMessage)
		{
			const string SafeString = "0123456789abcdefghijklmnopqrstuvwxyz-_/";
			int Ptr = 0;
			string TestChr = null;
			string Src = null;
			string FieldList = null;
			int LinkAliasPageID = 0;
			int PageContentCID = 0;
			string WorkingLinkAlias = null;
			int CS = 0;
			bool AllowLinkAlias = false;
			//
			if (true)
			{
				AllowLinkAlias = cpcore.siteProperties.getBoolean("allowLinkAlias", false);
				WorkingLinkAlias = linkAlias;
				if (!string.IsNullOrEmpty(WorkingLinkAlias))
				{
					//
					// remove nonsafe URL characters
					//
					Src = WorkingLinkAlias;
					Src = genericController.vbReplace(Src, "’", "'");
					Src = genericController.vbReplace(Src, "\t", " ");
					WorkingLinkAlias = "";
//INSTANT C# NOTE: The ending condition of VB 'For' loops is tested only on entry to the loop. Instant C# has created a temporary variable in order to use the initial value of Len(Src) + 1 for every iteration:
					int tempVar2 = Src.Length + 1;
					for (Ptr = 1; Ptr <= tempVar2; Ptr++)
					{
						TestChr = Src.Substring(Ptr - 1, 1);
						if (genericController.vbInstr(1, SafeString, TestChr, Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
						{
						}
						else
						{
							TestChr = "\t";
						}
						WorkingLinkAlias = WorkingLinkAlias + TestChr;
					}
					Ptr = 0;
					while (genericController.vbInstr(1, WorkingLinkAlias, "\t" + "\t") != 0 && (Ptr < 100))
					{
						WorkingLinkAlias = genericController.vbReplace(WorkingLinkAlias, "\t" + "\t", "\t");
						Ptr = Ptr + 1;
					}
					if (WorkingLinkAlias.Substring(WorkingLinkAlias.Length - 1) == "\t")
					{
						WorkingLinkAlias = WorkingLinkAlias.Substring(0, WorkingLinkAlias.Length - 1);
					}
					if (WorkingLinkAlias.Substring(0, 1) == "\t")
					{
						WorkingLinkAlias = WorkingLinkAlias.Substring(1);
					}
					WorkingLinkAlias = genericController.vbReplace(WorkingLinkAlias, "\t", "-");
					if (!string.IsNullOrEmpty(WorkingLinkAlias))
					{
						//
						// Make sure there is not a folder or page in the wwwroot that matches this Alias
						//
						if (WorkingLinkAlias.Substring(0, 1) != "/")
						{
							WorkingLinkAlias = "/" + WorkingLinkAlias;
						}
						//
						if (genericController.vbLCase(WorkingLinkAlias) == genericController.vbLCase("/" + cpcore.serverConfig.appConfig.name))
						{
							//
							// This alias points to the cclib folder
							//
							if (AllowLinkAlias)
							{
								return_WarningMessage = ""
									+ "The Link Alias being created (" + WorkingLinkAlias + ") can not be used because there is a virtual directory in your website directory that already uses this name."
									+ " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page.";
							}
						}
						else if (genericController.vbLCase(WorkingLinkAlias) == "/cclib")
						{
							//
							// This alias points to the cclib folder
							//
							if (AllowLinkAlias)
							{
								return_WarningMessage = ""
									+ "The Link Alias being created (" + WorkingLinkAlias + ") can not be used because there is a virtual directory in your website directory that already uses this name."
									+ " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page.";
							}
						}
						else if (cpcore.appRootFiles.pathExists(cpcore.serverConfig.appConfig.appRootFilesPath + "\\" + WorkingLinkAlias.Substring(1)))
						{
							//ElseIf appRootFiles.pathExists(serverConfig.clusterPath & serverconfig.appConfig.appRootFilesPath & "\" & Mid(WorkingLinkAlias, 2)) Then
							//
							// This alias points to a different link, call it an error
							//
							if (AllowLinkAlias)
							{
								return_WarningMessage = ""
									+ "The Link Alias being created (" + WorkingLinkAlias + ") can not be used because there is a folder in your website directory that already uses this name."
									+ " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page.";
							}
						}
						else
						{
							//
							// Make sure there is one here for this
							//
							if (true)
							{
								FieldList = "Name,PageID,QueryStringSuffix";
							}
							else
							{
								//
								// must be > 33914 to run this routine
								//
								FieldList = "Name,PageID,'' as QueryStringSuffix";
							}
							CS = cpcore.db.csOpen("Link Aliases", "name=" + cpcore.db.encodeSQLText(WorkingLinkAlias),,,,,, FieldList);
							if (!cpcore.db.csOk(CS))
							{
								//
								// Alias not found, create a Link Aliases
								//
								cpcore.db.csClose(CS);
								CS = cpcore.db.csInsertRecord("Link Aliases", 0);
								if (cpcore.db.csOk(CS))
								{
									cpcore.db.csSet(CS, "Name", WorkingLinkAlias);
									//Call app.csv_SetCS(CS, "Link", Link)
									cpcore.db.csSet(CS, "Pageid", PageID);
									if (true)
									{
										cpcore.db.csSet(CS, "QueryStringSuffix", QueryStringSuffix);
									}
								}
							}
							else
							{
								//
								// Alias found, verify the pageid & QueryStringSuffix
								//
								int CurrentLinkAliasID = 0;
								bool resaveLinkAlias = false;
								int CS2 = 0;
								LinkAliasPageID = cpcore.db.csGetInteger(CS, "pageID");
								if ((cpcore.db.csGetText(CS, "QueryStringSuffix").ToLower == QueryStringSuffix.ToLower()) && (PageID == LinkAliasPageID))
								{
									//
									// it maches a current entry for this link alias, if the current entry is not the highest number id,
									//   remove it and add this one
									//
									CurrentLinkAliasID = cpcore.db.csGetInteger(CS, "id");
									CS2 = cpcore.db.csOpenSql_rev("default", "select top 1 id from ccLinkAliases where pageid=" + LinkAliasPageID + " order by id desc");
									if (cpcore.db.csOk(CS2))
									{
										resaveLinkAlias = (CurrentLinkAliasID != cpcore.db.csGetInteger(CS2, "id"));
									}
									cpcore.db.csClose(CS2);
									if (resaveLinkAlias)
									{
										cpcore.db.executeQuery("delete from ccLinkAliases where id=" + CurrentLinkAliasID);
										cpcore.db.csClose(CS);
										CS = cpcore.db.csInsertRecord("Link Aliases", 0);
										if (cpcore.db.csOk(CS))
										{
											cpcore.db.csSet(CS, "Name", WorkingLinkAlias);
											cpcore.db.csSet(CS, "Pageid", PageID);
											if (true)
											{
												cpcore.db.csSet(CS, "QueryStringSuffix", QueryStringSuffix);
											}
										}
									}
								}
								else
								{
									//
									// Does not match, this is either a change, or a duplicate that needs to be blocked
									//
									if (OverRideDuplicate)
									{
										//
										// change the Link Alias to the new link
										//
										//Call app.csv_SetCS(CS, "Link", Link)
										cpcore.db.csSet(CS, "Pageid", PageID);
										if (true)
										{
											cpcore.db.csSet(CS, "QueryStringSuffix", QueryStringSuffix);
										}
									}
									else if (AllowLinkAlias)
									{
										//
										// This alias points to a different link, and link aliasing is in use, call it an error (but save record anyway)
										//
										if (DupCausesWarning)
										{
											if (LinkAliasPageID == 0)
											{
												PageContentCID = Models.Complex.cdefModel.getContentId(cpcore, "Page Content");
												return_WarningMessage = ""
													+ "This page has been saved, but the Link Alias could not be created (" + WorkingLinkAlias + ") because it is already in use for another page."
													+ " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique on this site. To set or change the Link Alias, clicke the Link Alias tab and select a name not used by another page or a folder in your website.";
											}
											else
											{
												PageContentCID = Models.Complex.cdefModel.getContentId(cpcore, "Page Content");
												return_WarningMessage = ""
													+ "This page has been saved, but the Link Alias could not be created (" + WorkingLinkAlias + ") because it is already in use for another page (<a href=\"?af=4&cid=" + PageContentCID + "&id=" + LinkAliasPageID + "\">edit</a>)."
													+ " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique. To set or change the Link Alias, click the Link Alias tab and select a name not used by another page or a folder in your website.";
											}
										}
									}
								}
							}
							int linkAliasId = cpcore.db.csGetInteger(CS, "id");
							cpcore.db.csClose(CS);
							cpcore.cache.invalidateContent_Entity(cpcore, linkAliasModel.contentTableName, linkAliasId);
						}
					}
				}
			}
		}
		//'
		//'   Returns the next entry in the array, empty when there are no more
		//'
		//Public Function getNextStyleFilenames() As String
		//    Dim result As String = ""
		//    Dim Ptr As Integer
		//    If styleFilenames_Cnt >= 0 Then
		//        For Ptr = 0 To styleFilenames_Cnt - 1
		//            If styleFilenames(Ptr) <> "" Then
		//                result = styleFilenames(Ptr)
		//                styleFilenames(Ptr) = ""
		//                Exit For
		//            End If
		//        Next
		//    End If
		//    Return result
		//End Function
		//'
		//'   Returns the next entry in the array, empty when there are no more
		//'
		//Public Function getJavascriptOnLoad() As String
		//    Dim result As String = ""
		//    Dim Ptr As Integer
		//    If javascriptOnLoad.Count >= 0 Then
		//        For Ptr = 0 To javascriptOnLoad.Count - 1
		//            If javascriptOnLoad(Ptr) <> "" Then
		//                result = javascriptOnLoad(Ptr)
		//                javascriptOnLoad(Ptr) = ""
		//                Exit For
		//            End If
		//        Next
		//    End If
		//    Return result
		//End Function
		//'
		//'   Returns the next entry in the array, empty when there are no more
		//'
		//Public Function getNextJavascriptBodyEnd() As String
		//    Dim result As String = ""
		//    Dim Ptr As Integer
		//    If javascriptBodyEnd.Count >= 0 Then
		//        For Ptr = 0 To javascriptBodyEnd.Count - 1
		//            If javascriptBodyEnd(Ptr) <> "" Then
		//                result = javascriptBodyEnd(Ptr)
		//                javascriptBodyEnd(Ptr) = ""
		//                Exit For
		//            End If
		//        Next
		//    End If
		//    Return result
		//End Function
		//'
		//'   Returns the next entry in the array, empty when there are no more
		//'
		//Public Function getNextJSFilename() As String
		//    Dim result As String = ""
		//    Dim Ptr As Integer
		//    If javascriptReferenceFilename_Cnt >= 0 Then
		//        For Ptr = 0 To javascriptReferenceFilename_Cnt - 1
		//            If javascriptReferenceFilename(Ptr) <> "" Then
		//                result = javascriptReferenceFilename(Ptr)
		//                javascriptReferenceFilename(Ptr) = ""
		//                Exit For
		//            End If
		//        Next
		//    End If
		//    Return result
		//End Function
		//
		//
		//
		public void addRefreshQueryString(string Name, string Value = "")
		{
			try
			{
				string[] temp = null;
				//
				if (Name.IndexOf("=") + 1 > 0)
				{
					temp = Name.Split('=');
					refreshQueryString = genericController.ModifyQueryString(cpcore.doc.refreshQueryString, temp[0], temp[1], true);
				}
				else
				{
					refreshQueryString = genericController.ModifyQueryString(cpcore.doc.refreshQueryString, Name, Value, true);
				}
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
				throw;
			}

		}
		//
		//========================================================================
		//   Process manual changes needed for Page Content Special Cases
		//       If workflow, only call this routine on a publish - it changes live records
		//========================================================================
		//
		public void processAfterSave(bool IsDelete, string ContentName, int RecordID, string RecordName, int RecordParentID, bool UseContentWatchLink)
		{
			int addonId = 0;
			string Option_String = null;
			string Filename = null;
			string FilenameExt = null;
			string FilenameNoExt = null;
			string FilePath = string.Empty;
			int Pos = 0;
			string AltSizeList = null;
			imageEditController sf = null;
			bool RebuildSizes = false;
			int CS = 0;
			string TableName = null;
			int ContentID = 0;
			int ActivityLogOrganizationID = 0;
			//
			ContentID = Models.Complex.cdefModel.getContentId(cpcore, ContentName);
			TableName = Models.Complex.cdefModel.getContentTablename(cpcore, ContentName);
			markRecordReviewed(ContentName, RecordID);
			//
			// -- invalidate the specific cache for this record
			cpcore.cache.invalidateContent_Entity(cpcore, TableName, RecordID);
			//
			// -- invalidate the tablename -- meaning any cache consumer that cannot itemize its entity records, can depend on this, which will invalidate anytime any record clears
			cpcore.cache.invalidateContent(TableName);
			//
			switch (genericController.vbLCase(TableName))
			{
				case linkForwardModel.contentTableName:
					//
					Models.Complex.routeDictionaryModel.invalidateCache(cpcore);
					break;
				case linkAliasModel.contentTableName:
					//
					Models.Complex.routeDictionaryModel.invalidateCache(cpcore);
					break;
				case addonModel.contentTableName:
					//
					Models.Complex.routeDictionaryModel.invalidateCache(cpcore);
					break;
				case personModel.contentTableName:
					//
					// Log Activity for changes to people and organizattions
					//
					//hint = hint & ",110"
					CS = cpcore.db.cs_open2("people", RecordID,,, "Name,OrganizationID");
					if (cpcore.db.csOk(CS))
					{
						ActivityLogOrganizationID = cpcore.db.csGetInteger(CS, "OrganizationID");
					}
					cpcore.db.csClose(CS);
					if (IsDelete)
					{
						logController.logActivity2(cpcore, "deleting user #" + RecordID + " (" + RecordName + ")", RecordID, ActivityLogOrganizationID);
					}
					else
					{
						logController.logActivity2(cpcore, "saving changes to user #" + RecordID + " (" + RecordName + ")", RecordID, ActivityLogOrganizationID);
					}
					break;
				case "organizations":
					//
					// Log Activity for changes to people and organizattions
					//
					//hint = hint & ",120"
					if (IsDelete)
					{
						logController.logActivity2(cpcore, "deleting organization #" + RecordID + " (" + RecordName + ")", 0, RecordID);
					}
					else
					{
						logController.logActivity2(cpcore, "saving changes to organization #" + RecordID + " (" + RecordName + ")", 0, RecordID);
					}
					break;
				case "ccsetup":
					//
					// Site Properties
					//
					//hint = hint & ",130"
					switch (genericController.vbLCase(RecordName))
					{
						case "allowlinkalias":
							cpcore.cache.invalidateAllObjectsInContent("Page Content");
							break;
						case "sectionlandinglink":
							cpcore.cache.invalidateAllObjectsInContent("Page Content");
							break;
						case siteproperty_serverPageDefault_name:
							cpcore.cache.invalidateAllObjectsInContent("Page Content");
							break;
					}
					break;
				case "ccpagecontent":
					//
					// set ChildPagesFound true for parent page
					//
					//hint = hint & ",140"
					if (RecordParentID > 0)
					{
						if (!IsDelete)
						{
							cpcore.db.executeQuery("update ccpagecontent set ChildPagesfound=1 where ID=" + RecordParentID);
						}
					}
					//
					// Page Content special cases for delete
					//
					if (IsDelete)
					{
						//
						// Clear the Landing page and page not found site properties
						//
						if (RecordID == genericController.EncodeInteger(cpcore.siteProperties.getText("PageNotFoundPageID", "0")))
						{
							cpcore.siteProperties.setProperty("PageNotFoundPageID", "0");
						}
						if (RecordID == cpcore.siteProperties.landingPageID)
						{
							cpcore.siteProperties.setProperty("landingPageId", "0");
						}
						//
						// Delete Link Alias entries with this PageID
						//
						cpcore.db.executeQuery("delete from cclinkAliases where PageID=" + RecordID);
					}
					cpcore.cache.invalidateContent_Entity(cpcore, TableName, RecordID);
				//Case "cctemplates" ', "ccsharedstyles"
				//    '
				//    ' Attempt to update the PageContentCache (PCC) array stored in the PeristantVariants
				//    '
				//    'hint = hint & ",150"
				//    If Not IsNothing(cpCore.addonStyleRulesIndex) Then
				//        Call cpCore.addonStyleRulesIndex.clear()
				//    End If

					break;
				case "ccaggregatefunctions":
					//
					// -- add-ons, rebuild addonCache
					cpcore.cache.invalidateContent("addonCache");
					cpcore.cache.invalidateContent_Entity(cpcore, TableName, RecordID);
					break;
				case "cclibraryfiles":
					//
					// if a AltSizeList is blank, make large,medium,small and thumbnails
					//
					//hint = hint & ",180"
					if (cpcore.siteProperties.getBoolean("ImageAllowSFResize", true))
					{
						if (!IsDelete)
						{
							CS = cpcore.db.csOpenRecord("library files", RecordID);
							if (cpcore.db.csOk(CS))
							{
								Filename = cpcore.db.csGet(CS, "filename");
								Pos = Filename.LastIndexOf("/") + 1;
								if (Pos > 0)
								{
									FilePath = Filename.Substring(0, Pos);
									Filename = Filename.Substring(Pos);
								}
								cpcore.db.csSet(CS, "filesize", cpcore.appRootFiles.main_GetFileSize(FilePath + Filename));
								Pos = Filename.LastIndexOf(".") + 1;
								if (Pos > 0)
								{
									FilenameExt = Filename.Substring(Pos);
									FilenameNoExt = Filename.Substring(0, Pos - 1);
									if (genericController.vbInstr(1, "jpg,gif,png", FilenameExt, Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
									{
										sf = new imageEditController();
										if (sf.load(cpcore.appRootFiles.rootLocalPath + FilePath + Filename))
										{
											//
											//
											//
											cpcore.db.csSet(CS, "height", sf.height);
											cpcore.db.csSet(CS, "width", sf.width);
											AltSizeList = cpcore.db.csGetText(CS, "AltSizeList");
											RebuildSizes = (string.IsNullOrEmpty(AltSizeList));
											if (RebuildSizes)
											{
												AltSizeList = "";
												//
												// Attempt to make 640x
												//
												if (sf.width >= 640)
												{
													sf.height = Convert.ToInt32(sf.height * (640 / sf.width));
													sf.width = 640;
													sf.save(cpcore.appRootFiles.rootLocalPath + FilePath + FilenameNoExt + "-640x" + sf.height + "." + FilenameExt);
													AltSizeList = AltSizeList + Environment.NewLine + "640x" + sf.height;
												}
												//
												// Attempt to make 320x
												//
												if (sf.width >= 320)
												{
													sf.height = Convert.ToInt32(sf.height * (320 / sf.width));
													sf.width = 320;
													sf.save(cpcore.appRootFiles.rootLocalPath + FilePath + FilenameNoExt + "-320x" + sf.height + "." + FilenameExt);

													AltSizeList = AltSizeList + Environment.NewLine + "320x" + sf.height;
												}
												//
												// Attempt to make 160x
												//
												if (sf.width >= 160)
												{
													sf.height = Convert.ToInt32(sf.height * (160 / sf.width));
													sf.width = 160;
													sf.save(cpcore.appRootFiles.rootLocalPath + FilePath + FilenameNoExt + "-160x" + sf.height + "." + FilenameExt);
													AltSizeList = AltSizeList + Environment.NewLine + "160x" + sf.height;
												}
												//
												// Attempt to make 80x
												//
												if (sf.width >= 80)
												{
													sf.height = Convert.ToInt32(sf.height * (80 / sf.width));
													sf.width = 80;
													sf.save(cpcore.appRootFiles.rootLocalPath + FilePath + FilenameNoExt + "-180x" + sf.height + "." + FilenameExt);
													AltSizeList = AltSizeList + Environment.NewLine + "80x" + sf.height;
												}
												cpcore.db.csSet(CS, "AltSizeList", AltSizeList);
											}
											sf.Dispose();
											sf = null;
										}
										//                                sf.Algorithm = genericController.EncodeInteger(main_GetSiteProperty("ImageResizeSFAlgorithm", "5"))
										//                                On Error Resume Next
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
							cpcore.db.csClose(CS);
						}
					}
					break;
				default:
					//
					// -- edit and delete for records -- clear entity cache
					cpcore.cache.invalidateContent_Entity(cpcore, TableName, RecordID);
					break;
			}
			//
			// -- edit and delete for records -- clear entity cache
			cpcore.cache.invalidateContent_Entity(cpcore, TableName, RecordID);
			//
			// Process Addons marked to trigger a process call on content change
			//
			CS = cpcore.db.csOpen("Add-on Content Trigger Rules", "ContentID=" + ContentID,,,,,, "addonid");
			if (IsDelete)
			{
				Option_String = ""
					+ Environment.NewLine + "action=contentdelete"
					+ Environment.NewLine + "contentid=" + ContentID + Environment.NewLine + "recordid=" + RecordID + "";
			}
			else
			{
				Option_String = ""
					+ Environment.NewLine + "action=contentchange"
					+ Environment.NewLine + "contentid=" + ContentID + Environment.NewLine + "recordid=" + RecordID + "";
			}
			while (cpcore.db.csOk(CS))
			{
				addonId = cpcore.db.csGetInteger(CS, "Addonid");
				//hint = hint & ",210 addonid=[" & addonId & "]"
				cpcore.addon.executeAddonAsProcess(addonId.ToString(), Option_String);
				cpcore.db.csGoNext(CS);
			}
			cpcore.db.csClose(CS);
			//
			// -- ok, temp work-around for the damn cache not invalidating correctly -- the nuclear solution
			cpcore.cache.invalidateAll();
		}
		//
		public void markRecordReviewed(string ContentName, int RecordID)
		{
			try
			{
				if (Models.Complex.cdefModel.isContentFieldSupported(cpcore, ContentName, "DateReviewed"))
				{
					string DataSourceName = Models.Complex.cdefModel.getContentDataSource(cpcore, ContentName);
					string TableName = Models.Complex.cdefModel.getContentTablename(cpcore, ContentName);
					string SQL = "update " + TableName + " set DateReviewed=" + cpcore.db.encodeSQLDate(cpcore.doc.profileStartTime);
					if (Models.Complex.cdefModel.isContentFieldSupported(cpcore, ContentName, "ReviewedBy"))
					{
						SQL += ",ReviewedBy=" + cpcore.doc.authContext.user.id;
					}
					//
					// -- Mark the live record
					cpcore.db.executeQuery(SQL + " where id=" + RecordID, DataSourceName);
				}
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
			}
		}
		//
		//=============================================================================
		//   Sets the MetaContent subsystem so the next call to main_GetLastMeta... returns the correct value
		//       And neither takes much time
		//=============================================================================
		//
		public void setMetaContent(int ContentID, int RecordID)
		{
			string KeywordList = string.Empty;
			int CS = 0;
			string Criteria = null;
			string SQL = null;
			string FieldList = null;
			int iContentID = 0;
			int iRecordID = 0;
			int MetaContentID = 0;
			//
			iContentID = genericController.EncodeInteger(ContentID);
			iRecordID = genericController.EncodeInteger(RecordID);
			if ((iContentID != 0) & (iRecordID != 0))
			{
				//
				// main_Get ID, Description, Title
				//
				Criteria = "(ContentID=" + iContentID + ")and(RecordID=" + iRecordID + ")";
				if (false) //.3.550" Then
				{
					FieldList = "ID,Name,MetaDescription,'' as OtherHeadTags,'' as MetaKeywordList";
				}
				else if (false) //.3.930" Then
				{
					FieldList = "ID,Name,MetaDescription,OtherHeadTags,'' as MetaKeywordList";
				}
				else
				{
					FieldList = "ID,Name,MetaDescription,OtherHeadTags,MetaKeywordList";
				}
				CS = cpcore.db.csOpen("Meta Content", Criteria,,,,,, FieldList);
				if (cpcore.db.csOk(CS))
				{
					MetaContentID = cpcore.db.csGetInteger(CS, "ID");
					cpcore.html.addTitle(genericController.encodeHTML(cpcore.db.csGetText(CS, "Name")), "page content");
					cpcore.html.addMetaDescription(genericController.encodeHTML(cpcore.db.csGetText(CS, "MetaDescription")), "page content");
					cpcore.html.addHeadTag(cpcore.db.csGetText(CS, "OtherHeadTags"), "page content");
					if (true)
					{
						KeywordList = genericController.vbReplace(cpcore.db.csGetText(CS, "MetaKeywordList"), Environment.NewLine, ",");
					}
					//main_MetaContent_Title = encodeHTML(app.csv_cs_getText(CS, "Name"))
					//htmldoc.main_MetaContent_Description = encodeHTML(app.csv_cs_getText(CS, "MetaDescription"))
					//main_MetaContent_OtherHeadTags = app.csv_cs_getText(CS, "OtherHeadTags")
				}
				cpcore.db.csClose(CS);
				//
				// main_Get Keyword List
				//
				SQL = "select ccMetaKeywords.Name"
					+ " From ccMetaKeywords"
					+ " LEFT JOIN ccMetaKeywordRules on ccMetaKeywordRules.MetaKeywordID=ccMetaKeywords.ID"
					+ " Where ccMetaKeywordRules.MetaContentID=" + MetaContentID;
				CS = cpcore.db.csOpenSql(SQL);
				while (cpcore.db.csOk(CS))
				{
					KeywordList = KeywordList + "," + cpcore.db.csGetText(CS, "Name");
					cpcore.db.csGoNext(CS);
				}
				if (!string.IsNullOrEmpty(KeywordList))
				{
					if (KeywordList.Substring(0, 1) == ",")
					{
						KeywordList = KeywordList.Substring(1);
					}
					//KeyWordList = Mid(KeyWordList, 2)
					KeywordList = genericController.encodeHTML(KeywordList);
					cpcore.html.addMetaKeywordList(KeywordList, "page content");
				}
				cpcore.db.csClose(CS);
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// Clear all data from the metaData current instance. Next request will load from cache.
		/// </summary>
		public void clearMetaData()
		{
			try
			{
				if (cpcore.doc.cdefDictionary != null)
				{
					cdefDictionary.Clear();
				}
				if (tableSchemaDictionary != null)
				{
					tableSchemaDictionary.Clear();
				}
				contentNameIdDictionaryClear();
				contentIdDictClear();
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
				throw;
			}
		}

	}
}
