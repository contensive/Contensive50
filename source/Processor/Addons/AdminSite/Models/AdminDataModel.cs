
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
    /// <summary>
    /// object that contains the context for the admin site, like recordsPerPage, etc. Should eventually include the loadContext and be its own document
    /// </summary>
    public class AdminDataModel {
        //
        /// <summary>
        /// the content being edited
        /// </summary>
        public CDefModel adminContent = null;
        /// <summary>
        /// the record being edited
        /// </summary>
        public EditRecordClass editRecord = null;
        /// <summary>
        /// Value returned from a submit button, process into action/form
        /// </summary>
        public string requestButton;
        /// <summary>
        /// the next form requested (the get)
        /// </summary>
        public int AdminForm;
        /// <summary>
        /// ccGroupRules storage for list of Content that a group can author
        /// </summary>
        public struct ContentGroupRuleType {
            public int ContentID;
            public bool AllowAdd;
            public bool AllowDelete;
        }
        /// <summary>
        /// Group Rules
        /// </summary>
        public struct GroupRuleType {
            public int GroupID;
            public bool AllowAdd;
            public bool AllowDelete;
        }
        /// <summary>
        /// Used within Admin site to create fancyBox popups
        /// </summary>
        public bool includeFancyBox;
        /// <summary>
        /// 
        /// </summary>
        public int fancyBoxPtr;
        /// <summary>
        /// 
        /// </summary>
        public string fancyBoxHeadJS;
        /// <summary>
        /// 
        /// </summary>
        public int requestedContentId;
        /// <summary>
        /// 
        /// </summary>
        public int requestedRecordId;
        /// <summary>
        /// true if there was an error loading the edit record - use to block the edit form
        /// </summary>
        public bool BlockEditForm;
        /// <summary>
        /// The action to be performed before the next form
        /// </summary>
        public int Admin_Action;
        /// <summary>
        /// The form that submitted that the button to process
        /// </summary>
        public int AdminSourceForm;
        /// <summary>
        /// for passing where clause values from page to page
        /// </summary>
        public string[,] WherePair = new string[3, 11];
        /// <summary>
        /// the current number of WherePairCount in use
        /// </summary>
        public int WherePairCount;
        /// <summary>
        /// 
        /// </summary>
        public int RecordTop;
        /// <summary>
        /// 
        /// </summary>
        public int RecordsPerPage;
        /// <summary>
        /// The number of windows open (below this one)
        /// </summary>
        public int ignore_legacyMenuDepth;
        /// <summary>
        /// String that adds on to the end of the title
        /// </summary>
        public string TitleExtension;
        /// <summary>
        /// Controls the menu mode, set from core.main_MemberAdminMenuModeID
        /// </summary>
        public int AdminMenuModeID;
        /// <summary>
        /// true uses tab system
        /// </summary>
        public bool allowAdminTabs;
        /// <summary>
        /// this is a hidden on the edit form. The popup editor preferences sets this hidden and submits
        /// </summary>
        public string fieldEditorPreference;
        /// <summary>
        /// flag set that shows the rest are valid
        /// </summary>
        public bool ContentWatchLoaded;
        /// <summary>
        /// 
        /// </summary>
        public int ContentWatchRecordID;
        /// <summary>
        /// 
        /// </summary>
        public string ContentWatchLink;
        /// <summary>
        /// 
        /// </summary>
        public int ContentWatchClicks;
        /// <summary>
        /// 
        /// </summary>
        public string ContentWatchLinkLabel;
        /// <summary>
        /// 
        /// </summary>
        public DateTime ContentWatchExpires;
        /// <summary>
        /// list of all ContentWatchLists for this Content, read from response, then later saved to Rules
        /// </summary>
        public int[] ContentWatchListID;
        /// <summary>
        /// size of ContentWatchListID() array
        /// </summary>
        public int ContentWatchListIDSize;
        /// <summary>
        /// number of valid entries in ContentWatchListID()
        /// </summary>
        public int ContentWatchListIDCount;
        /// <summary>
        /// Count of Buttons in use
        /// </summary>
        public int ButtonObjectCount;
        /// <summary>
        /// 
        /// </summary>
        public string[,] ImagePreloads = new string[3, 101];
        /// <summary>
        /// Collected string of Javascript functions to print at end
        /// </summary>
        public string JavaScriptString;
        /// <summary>
        /// the HTML needed to complete the Admin Form after contents
        /// </summary>
        public string adminFooter;
        /// <summary>
        /// 
        /// </summary>
        public bool UserAllowContentEdit;
        /// <summary>
        /// used to generate labels for form input
        /// </summary>
        public int FormInputCount;
        /// <summary>
        /// 
        /// </summary>
        public int EditSectionPanelCount;
        /// <summary>
        /// 
        /// </summary>
        public const string OpenLiveWindowTable = "<div ID=\"LiveWindowTable\">";
        /// <summary>
        /// 
        /// </summary>
        public const string CloseLiveWindowTable = "</div>";
        /// <summary>
        /// 
        /// </summary>
        public const string AdminFormErrorOpen = "<table border=\"0\" cellpadding=\"20\" cellspacing=\"0\" width=\"100%\"><tr><td align=\"left\">";
        /// <summary>
        /// 
        /// </summary>
        public const string AdminFormErrorClose = "</td></tr></table>";
        /// <summary>
        /// these were defined different in csv
        /// </summary>
        public enum NodeTypeEnum {
            NodeTypeEntry = 0,
            NodeTypeCollection = 1,
            NodeTypeAddon = 2,
            NodeTypeContent = 3
        }
        /// <summary>
        /// 
        /// </summary>
        public const string IndexConfigPrefix = "IndexConfig:";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public bool AllowAdminFieldCheck(CoreController core) {
            if (AllowAdminFieldCheck_Local == null) {
                AllowAdminFieldCheck_Local = core.siteProperties.getBoolean("AllowAdminFieldCheck", true);
            }
            return (bool)AllowAdminFieldCheck_Local;
        } private bool? AllowAdminFieldCheck_Local = null;
        //
        //========================================================================
        /// <summary>
        /// Read in Whats New values if present, Field values must be loaded
        /// </summary>
        /// <param name="core"></param>
        /// <param name="adminInfo"></param>
        public void LoadContentTrackingDataBase(CoreController core) {
            try {
                // todo
                //AdminUIController.EditRecordClass editRecord = adminInfo.editRecord;
                //
                int ContentID = 0;
                int CSPointer = 0;
                // converted array to dictionary - Dim FieldPointer As Integer
                //
                // ----- check if admin record is present
                //
                if ((editRecord.id != 0) & (adminContent.allowContentTracking)) {
                    //
                    // ----- Open the content watch record for this content record
                    //
                    ContentID = ((editRecord.contentControlId.Equals(0)) ? adminContent.id : editRecord.contentControlId);
                    CSPointer = core.db.csOpen("Content Watch", "(ContentID=" + core.db.encodeSQLNumber(ContentID) + ")AND(RecordID=" + core.db.encodeSQLNumber(editRecord.id) + ")");
                    if (core.db.csOk(CSPointer)) {
                        ContentWatchLoaded = true;
                        ContentWatchRecordID = (core.db.csGetInteger(CSPointer, "ID"));
                        ContentWatchLink = (core.db.csGet(CSPointer, "Link"));
                        ContentWatchClicks = (core.db.csGetInteger(CSPointer, "Clicks"));
                        ContentWatchLinkLabel = (core.db.csGet(CSPointer, "LinkLabel"));
                        ContentWatchExpires = (core.db.csGetDate(CSPointer, "WhatsNewDateExpires"));
                        core.db.csClose(ref CSPointer);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //========================================================================
        /// <summary>
        /// Read in Whats New values if present
        /// </summary>
        /// <param name="core"></param>
        /// <param name="adminContext"></param>
        public void LoadContentTrackingResponse(CoreController core) {
            try {
                //
                int CSContentWatchList = 0;
                int RecordID = 0;
                //
                ContentWatchListIDCount = 0;
                if ((core.docProperties.getText("WhatsNewResponse") != "") & (adminContent.allowContentTracking)) {
                    //
                    // ----- set single fields
                    //
                    ContentWatchLinkLabel = core.docProperties.getText("ContentWatchLinkLabel");
                    ContentWatchExpires = core.docProperties.getDate("ContentWatchExpires");
                    //
                    // ----- Update ContentWatchListRules for all checked boxes
                    //
                    CSContentWatchList = core.db.csOpen("Content Watch Lists");
                    while (core.db.csOk(CSContentWatchList)) {
                        RecordID = (core.db.csGetInteger(CSContentWatchList, "ID"));
                        if (core.docProperties.getBoolean("ContentWatchList." + RecordID)) {
                            if (ContentWatchListIDCount >= ContentWatchListIDSize) {
                                ContentWatchListIDSize = ContentWatchListIDSize + 50;
                                Array.Resize(ref ContentWatchListID, ContentWatchListIDSize);
                            }
                            ContentWatchListID[ContentWatchListIDCount] = RecordID;
                            ContentWatchListIDCount = ContentWatchListIDCount + 1;
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
        // ====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="core"></param>
        /// <param name="AdminOnly"></param>
        /// <param name="DeveloperOnly"></param>
        /// <param name="Active"></param>
        /// <param name="Authorable"></param>
        /// <param name="Name"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static bool IsVisibleUserField(CoreController core, bool AdminOnly, bool DeveloperOnly, bool Active, bool Authorable, string Name, string TableName) {
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
        // ====================================================================================================
        /// <summary>
        /// Test Content Access -- return based on Admin/Developer/MemberRules, if developer, let all through, if admin, block if table is developeronly 
        /// if member, run blocking query (which also traps adminonly and developer only), if blockin query has a null RecordID, this member gets everything
        /// if not null recordid in blocking query, use RecordIDs in result for Where clause on this lookup
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ContentID"></param>
        /// <returns></returns>
        public static  bool userHasContentAccess(CoreController core, int ContentID) {
            bool result = false;
            try {
                string ContentName = CdefController.getContentNameByID(core, ContentID);
                if (!string.IsNullOrEmpty(ContentName)) {
                    result = core.session.isAuthenticatedContentManager(core, ContentName);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// Get the Wherepair value for a fieldname, If there is a match with the left side, return the right,If no match, return ""
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public string getWherePairValue(string FieldName) {
            try {
                FieldName = FieldName.ToLower();
                if (WherePairCount > 0) {
                    for (int WhereCount = 0; WhereCount < WherePairCount; WhereCount++) {
                        if (FieldName == WherePair[0, WhereCount].ToLower()) {
                            return WherePair[1, WhereCount];
                        }
                    }
                }
            } catch (Exception) {
                throw;
            }
            return "";
        }
        //
        // ====================================================================================================
        /// <summary>
        /// constructor - load the context for the admin site, controlled by request inputs like rnContent (cid) and rnRecordId (id)
        /// </summary>
        /// <param name="core"></param>
        public AdminDataModel(CoreController core) {
            try {
                //
                // Tab Control
                allowAdminTabs = GenericController.encodeBoolean(core.userProperty.getText("AllowAdminTabs", "1"));
                if (core.docProperties.getText("tabs") != "") {
                    if (core.docProperties.getBoolean("tabs") != allowAdminTabs) {
                        allowAdminTabs = !allowAdminTabs;
                        core.userProperty.setProperty("AllowAdminTabs", allowAdminTabs.ToString());
                    }
                }
                //
                // adminContext.content init
                requestedContentId = core.docProperties.getInteger("cid");
                if (requestedContentId != 0) {
                    adminContent = CDefModel.create(core, requestedContentId);
                    if (adminContent == null) {
                        adminContent = new CDefModel();
                        adminContent.id = 0;
                        Processor.Controllers.ErrorController.addUserError(core, "There is no content with the requested id [" + requestedContentId + "]");
                        requestedContentId = 0;
                    }
                }
                if (adminContent == null) {
                    adminContent = new CDefModel();
                }
                //
                // determine user rights to this content
                UserAllowContentEdit = true;
                if (!core.session.isAuthenticatedAdmin(core)) {
                    if (adminContent.id > 0) {
                        UserAllowContentEdit = userHasContentAccess(core, adminContent.id);
                    }
                }
                //
                // editRecord init
                //
                editRecord = new EditRecordClass {
                    Loaded = false
                };
                requestedRecordId = core.docProperties.getInteger("id");
                if ((UserAllowContentEdit) & (requestedRecordId != 0) && (adminContent.id > 0)) {
                    //
                    // set adminContext.content to the content definition of the requested record
                    //
                    int CS = core.db.csOpenRecord(adminContent.name, requestedRecordId, false, false, "ContentControlID");
                    if (core.db.csOk(CS)) {
                        editRecord.id = requestedRecordId;
                        int recordContentId = core.db.csGetInteger(CS, "ContentControlID");
                        //adminContent.id = core.db.csGetInteger(CS, "ContentControlID");
                        if ((recordContentId > 0) & (recordContentId != adminContent.id)) {
                            adminContent = CDefModel.create(core, recordContentId);
                        }
                    }
                    core.db.csClose(ref CS);
                }
                //
                // Other page control fields
                //
                TitleExtension = core.docProperties.getText(RequestNameTitleExtension);
                RecordTop = core.docProperties.getInteger("RT");
                RecordsPerPage = core.docProperties.getInteger("RS");
                if (RecordsPerPage == 0) {
                    RecordsPerPage = Constants.RecordsPerPageDefault;
                }
                //
                // Read WherePairCount
                //
                WherePairCount = 99;
                int WCount = 0;
                for (WCount = 0; WCount <= 99; WCount++) {
                    WherePair[0, WCount] = GenericController.encodeText(core.docProperties.getText("WL" + WCount));
                    if (WherePair[0, WCount] == "") {
                        WherePairCount = WCount;
                        break;
                    } else {
                        WherePair[1, WCount] = GenericController.encodeText(core.docProperties.getText("WR" + WCount));
                        core.doc.addRefreshQueryString("wl" + WCount, GenericController.encodeRequestVariable(WherePair[0, WCount]));
                        core.doc.addRefreshQueryString("wr" + WCount, GenericController.encodeRequestVariable(WherePair[1, WCount]));
                    }
                }
                //
                // Read WhereClauseContent to WherePairCount
                //
                string WhereClauseContent = GenericController.encodeText(core.docProperties.getText("wc"));
                if (!string.IsNullOrEmpty(WhereClauseContent)) {
                    //
                    // ***** really needs a server.URLDecode() function
                    //
                    core.doc.addRefreshQueryString("wc", WhereClauseContent);
                    //WhereClauseContent = genericController.vbReplace(WhereClauseContent, "%3D", "=")
                    //WhereClauseContent = genericController.vbReplace(WhereClauseContent, "%26", "&")
                    if (!string.IsNullOrEmpty(WhereClauseContent)) {
                        string[] QSSplit = WhereClauseContent.Split(',');
                        int QSPointer = 0;
                        for (QSPointer = 0; QSPointer <= QSSplit.GetUpperBound(0); QSPointer++) {
                            string NameValue = QSSplit[QSPointer];
                            if (!string.IsNullOrEmpty(NameValue)) {
                                if ((NameValue.Left(1) == "(") && (NameValue.Substring(NameValue.Length - 1) == ")") && (NameValue.Length > 2)) {
                                    NameValue = NameValue.Substring(1, NameValue.Length - 2);
                                }
                                string[] NVSplit = NameValue.Split('=');
                                WherePair[0, WherePairCount] = NVSplit[0];
                                if (NVSplit.GetUpperBound(0) > 0) {
                                    WherePair[1, WherePairCount] = NVSplit[1];
                                }
                                WherePairCount = WherePairCount + 1;
                            }
                        }
                    }
                }
                //
                // --- If AdminMenuMode is not given locally, use the Members Preferences
                //
                AdminMenuModeID = core.docProperties.getInteger("mm");
                if (AdminMenuModeID == 0) {
                    AdminMenuModeID = core.session.user.AdminMenuModeID;
                }
                if (AdminMenuModeID == 0) {
                    AdminMenuModeID = AdminMenuModeLeft;
                }
                if (core.session.user.AdminMenuModeID != AdminMenuModeID) {
                    core.session.user.AdminMenuModeID = AdminMenuModeID;
                    core.session.user.save(core);
                }
                //    '
                //    ' ----- FieldName
                //    '
                //    InputFieldName = core.main_GetStreamText2(RequestNameFieldName)
                //
                // ----- Other
                //
                Admin_Action = core.docProperties.getInteger(rnAdminAction);
                AdminSourceForm = core.docProperties.getInteger(rnAdminSourceForm);
                AdminForm = core.docProperties.getInteger(rnAdminForm);
                requestButton = core.docProperties.getText(RequestNameButton);
                if (AdminForm == AdminFormEdit) {
                    ignore_legacyMenuDepth = 0;
                } else {
                    ignore_legacyMenuDepth = core.docProperties.getInteger(RequestNameAdminDepth);
                }
                //
                // ----- convert fieldEditorPreference change to a refresh action
                //
                if (adminContent.id != 0) {
                    fieldEditorPreference = core.docProperties.getText("fieldEditorPreference");
                    if (fieldEditorPreference != "") {
                        //
                        // Editor Preference change attempt. Set new preference and set this as a refresh
                        //
                        requestButton = "";
                        Admin_Action = Constants.AdminActionEditRefresh;
                        AdminForm = AdminFormEdit;
                        int Pos = GenericController.vbInstr(1, fieldEditorPreference, ":");
                        if (Pos > 0) {
                            int fieldEditorFieldId = GenericController.encodeInteger(fieldEditorPreference.Left(Pos - 1));
                            int fieldEditorAddonId = GenericController.encodeInteger(fieldEditorPreference.Substring(Pos));
                            if (fieldEditorFieldId != 0) {
                                bool editorOk = true;
                                string SQL = "select id from ccfields where (active<>0) and id=" + fieldEditorFieldId;
                                DataTable dtTest = core.db.executeQuery(SQL);
                                if (dtTest.Rows.Count == 0) {
                                    editorOk = false;
                                }
                                //RS = core.app.executeSql(SQL)
                                //If (not isdatatableok(rs)) Then
                                //    editorOk = False
                                //ElseIf rs.rows.count=0 Then
                                //    editorOk = False
                                //End If
                                //If (isDataTableOk(rs)) Then
                                //    If false Then
                                //        'RS.Close()
                                //    End If
                                //    'RS = Nothing
                                //End If
                                if (editorOk && (fieldEditorAddonId != 0)) {
                                    SQL = "select id from ccaggregatefunctions where (active<>0) and id=" + fieldEditorAddonId;
                                    dtTest = core.db.executeQuery(SQL);
                                    if (dtTest.Rows.Count == 0) {
                                        editorOk = false;
                                    }
                                    //RS = core.app.executeSql(SQL)
                                    //If (not isdatatableok(rs)) Then
                                    //    editorOk = False
                                    //ElseIf rs.rows.count=0 Then
                                    //    editorOk = False
                                    //End If
                                    //If (isDataTableOk(rs)) Then
                                    //    If false Then
                                    //        'RS.Close()
                                    //    End If
                                    //    'RS = Nothing
                                    //End If
                                }
                                if (editorOk) {
                                    string Key = "editorPreferencesForContent:" + adminContent.id;
                                    //
                                    string editorpreferences = core.userProperty.getText(Key, "");
                                    if (!string.IsNullOrEmpty(editorpreferences)) {
                                        //
                                        // remove current preferences for this field
                                        //
                                        string[] Parts = ("," + editorpreferences).Split(new[] { "," + fieldEditorFieldId.ToString() + ":" }, StringSplitOptions.None);
                                        int Cnt = Parts.GetUpperBound(0) + 1;
                                        if (Cnt > 0) {
                                            int Ptr = 0;
                                            for (Ptr = 1; Ptr < Cnt; Ptr++) {
                                                Pos = GenericController.vbInstr(1, Parts[Ptr], ",");
                                                if (Pos == 0) {
                                                    Parts[Ptr] = "";
                                                } else if (Pos > 0) {
                                                    Parts[Ptr] = Parts[Ptr].Substring(Pos);
                                                }
                                            }
                                        }
                                        editorpreferences = string.Join("", Parts);
                                    }
                                    editorpreferences = editorpreferences + "," + fieldEditorFieldId + ":" + fieldEditorAddonId;
                                    core.userProperty.setProperty(Key, editorpreferences);
                                }
                            }
                        }
                    }
                }
                //
                // --- Spell Check
                //SpellCheckSupported = false;
                //SpellCheckRequest = false;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return;
        }
        // ====================================================================================================
        // Load Array
        //   Get defaults if no record ID
        //   Then load in any response elements
        //
        public void LoadEditRecord(CoreController core, bool CheckUserErrors = false) {
            try {
                // todo refactor out
                if (string.IsNullOrEmpty(adminContent.name)) {
                    //
                    // Can not load edit record because bad content definition
                    //
                    if (adminContent.id == 0) {
                        throw (new Exception("The record can Not be edited because no content definition was specified."));
                    } else {
                        throw (new Exception("The record can Not be edited because a content definition For ID [" + adminContent.id + "] was not found."));
                    }
                } else {
                    //
                    if (editRecord.id == 0) {
                        //
                        // ----- New record, just load defaults
                        //
                        LoadEditRecord_Default(core);
                        LoadEditRecord_WherePairs(core);
                    } else {
                        //
                        // ----- Load the Live Record specified
                        //
                        LoadEditRecord_Dbase(core, CheckUserErrors);
                        LoadEditRecord_WherePairs(core);
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
                    editRecord.menuHeadline = "";
                    if (editRecord.fieldsLc.ContainsKey("menuheadline")) {
                        editRecord.menuHeadline = GenericController.encodeText(editRecord.fieldsLc["menuheadline"].value);
                    }
                    //
                    //editRecord.menuHeadline = "";
                    if (editRecord.fieldsLc.ContainsKey("name")) {
                        //Dim editRecordField As editRecordFieldClass = editRecord.fieldsLc["name")
                        //editRecord.nameLc = editRecordField.value.ToString()
                        editRecord.nameLc = GenericController.encodeText(editRecord.fieldsLc["name"].value);
                    }
                    //
                    //editRecord.menuHeadline = "";
                    if (editRecord.fieldsLc.ContainsKey("active")) {
                        editRecord.active = GenericController.encodeBoolean(editRecord.fieldsLc["active"].value);
                    }
                    //
                    //editRecord.menuHeadline = "";
                    if (editRecord.fieldsLc.ContainsKey("contentcontrolid")) {
                        editRecord.contentControlId = GenericController.encodeInteger(editRecord.fieldsLc["contentcontrolid"].value);
                    }
                    //
                    //editRecord.menuHeadline = "";
                    if (editRecord.fieldsLc.ContainsKey("parentid")) {
                        editRecord.parentID = GenericController.encodeInteger(editRecord.fieldsLc["parentid"].value);
                    }
                    //
                    //editRecord.menuHeadline = "";
                    //if (editRecord.fieldsLc.ContainsKey("rootpageid")) {
                    //    editRecord.RootPageID = genericController.encodeInteger(editRecord.fieldsLc["rootpageid"].value);
                    //}
                    //
                    // ----- Set the local global copy of Edit Record Locks
                    //
                    core.doc.getAuthoringStatus(adminContent.name, editRecord.id, ref editRecord.SubmitLock, ref editRecord.ApproveLock, ref editRecord.SubmittedName, ref editRecord.ApprovedName, ref editRecord.IsInserted, ref editRecord.IsDeleted, ref editRecord.IsModified, ref editRecord.LockModifiedName, ref editRecord.LockModifiedDate, ref editRecord.SubmittedDate, ref editRecord.ApprovedDate);
                    //
                    // ----- Set flags used to determine the Authoring State
                    //
                    core.doc.getAuthoringPermissions(adminContent.name, editRecord.id, ref editRecord.AllowInsert, ref editRecord.AllowCancel, ref editRecord.AllowSave, ref editRecord.AllowDelete, ref editRecord.AllowPublish, ref editRecord.AllowAbort, ref editRecord.AllowSubmit, ref editRecord.AllowApprove, ref editRecord.Read_Only);
                    //
                    // ----- Set Edit Lock
                    //
                    if (editRecord.id != 0) {
                        editRecord.EditLock = core.workflow.GetEditLockStatus(adminContent.name, editRecord.id);
                        if (editRecord.EditLock) {
                            editRecord.EditLockMemberName = core.workflow.GetEditLockMemberName(adminContent.name, editRecord.id);
                            editRecord.EditLockExpires = core.workflow.GetEditLockDateExpires(adminContent.name, editRecord.id);
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
                    if (GenericController.vbUCase(adminContent.tableName) == GenericController.vbUCase("ccMembers")) {
                        if (!core.session.isAuthenticatedDeveloper(core)) {
                            if (editRecord.fieldsLc.ContainsKey("developer")) {
                                if (GenericController.encodeBoolean(editRecord.fieldsLc["developer"].value)) {
                                    editRecord.Read_Only = true;
                                    Processor.Controllers.ErrorController.addUserError(core, "You Do Not have access rights To edit this record.");
                                    BlockEditForm = true;
                                }
                            }
                        }
                    }
                    //
                    // ----- Now make sure this record is locked from anyone else
                    //
                    if (!(editRecord.Read_Only)) {
                        core.workflow.SetEditLock(adminContent.name, editRecord.id);
                    }
                    editRecord.Loaded = true;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        // ====================================================================================================
        //   Load both Live and Edit Record values from definition defaults
        //
        public void LoadEditRecord_Default(CoreController core) {
            try {
                //
                string DefaultValueText = null;
                string LookupContentName = null;
                string UCaseDefaultValueText = null;
                string[] lookups = null;
                int Ptr = 0;
                string defaultValue = null;
                EditRecordFieldClass editRecordField = null;
                CDefFieldModel field = null;
                editRecord.active = true;
                editRecord.contentControlId = adminContent.id;
                editRecord.contentControlId_Name = adminContent.name;
                editRecord.EditLock = false;
                editRecord.Loaded = false;
                editRecord.Saved = false;
                foreach (var keyValuePair in adminContent.fields) {
                    field = keyValuePair.Value;
                    if (!(editRecord.fieldsLc.ContainsKey(field.nameLc))) {
                        editRecordField = new EditRecordFieldClass();
                        editRecord.fieldsLc.Add(field.nameLc, editRecordField);
                    }
                    defaultValue = field.defaultValue;
                    //    End If
                    if (field.active & !GenericController.IsNull(defaultValue)) {
                        switch (field.fieldTypeId) {
                            case FieldTypeIdInteger:
                            case FieldTypeIdAutoIdIncrement:
                            case FieldTypeIdMemberSelect:
                                //
                                editRecord.fieldsLc[field.nameLc].value = GenericController.encodeInteger(defaultValue);
                                break;
                            case FieldTypeIdCurrency:
                            case FieldTypeIdFloat:
                                //
                                editRecord.fieldsLc[field.nameLc].value = GenericController.encodeNumber(defaultValue);
                                break;
                            case FieldTypeIdBoolean:
                                //
                                editRecord.fieldsLc[field.nameLc].value = GenericController.encodeBoolean(defaultValue);
                                break;
                            case FieldTypeIdDate:
                                //
                                editRecord.fieldsLc[field.nameLc].value = GenericController.encodeDate(defaultValue);
                                break;
                            case FieldTypeIdLookup:

                                DefaultValueText = GenericController.encodeText(field.defaultValue);
                                if (!string.IsNullOrEmpty(DefaultValueText)) {
                                    if (DefaultValueText.IsNumeric()) {
                                        editRecord.fieldsLc[field.nameLc].value = DefaultValueText;
                                    } else {
                                        if (field.lookupContentID != 0) {
                                            LookupContentName = CdefController.getContentNameByID(core, field.lookupContentID);
                                            if (!string.IsNullOrEmpty(LookupContentName)) {
                                                editRecord.fieldsLc[field.nameLc].value = core.db.getRecordID(LookupContentName, DefaultValueText);
                                            }
                                        } else if (field.lookupList != "") {
                                            UCaseDefaultValueText = GenericController.vbUCase(DefaultValueText);
                                            lookups = field.lookupList.Split(',');
                                            for (Ptr = 0; Ptr <= lookups.GetUpperBound(0); Ptr++) {
                                                if (UCaseDefaultValueText == GenericController.vbUCase(lookups[Ptr])) {
                                                    editRecord.fieldsLc[field.nameLc].value = Ptr + 1;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                break;
                            default:
                                //
                                editRecord.fieldsLc[field.nameLc].value = GenericController.encodeText(defaultValue);
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
                            editRecord.fieldsLc[field.nameLc].value = core.session.user.id;
                            //    .readonlyfield = True
                            //    .Required = False
                            break;
                        case "CREATEDBY":
                            editRecord.fieldsLc[field.nameLc].value = core.session.user.id;
                            //    .readonlyfield = True
                            //    .Required = False
                            //Case "DATEADDED"
                            //    .readonlyfield = True
                            //    .Required = False
                            break;
                        case "CONTENTCONTROLID":
                            editRecord.fieldsLc[field.nameLc].value = adminContent.id;
                            //Case "SORTORDER"
                            // default to ID * 100, but must be done later
                            break;
                    }
                    editRecord.fieldsLc[field.nameLc].dbValue = editRecord.fieldsLc[field.nameLc].value;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        // ====================================================================================================
        //   Load both Live and Edit Record values from definition defaults
        //
        public void LoadEditRecord_WherePairs(CoreController core) {
            try {
                // todo refactor out
                string DefaultValueText = null;
                foreach (var keyValuePair in adminContent.fields) {
                    CDefFieldModel field = keyValuePair.Value;
                    DefaultValueText = getWherePairValue(field.nameLc);
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
        // ====================================================================================================
        //   Load Records from the database
        //
        public void LoadEditRecord_Dbase(CoreController core, bool CheckUserErrors = false) {
            try {
                //
                //
                object DBValueVariant = null;
                int CSEditRecord = 0;
                object NullVariant = null;
                int CSPointer = 0;
                //
                // ----- test for content problem
                //
                if (editRecord.id == 0) {
                    //
                    // ----- Skip load, this is a new record
                    //
                } else if (adminContent.id == 0) {
                    //
                    // ----- Error: no content ID
                    //
                    BlockEditForm = true;
                    Processor.Controllers.ErrorController.addUserError(core, "No content definition was found For Content ID [" + editRecord.id + "]. Please contact your application developer For more assistance.");
                    LogController.handleError(core, new ApplicationException("AdminClass.LoadEditRecord_Dbase, No content definition was found For Content ID [" + editRecord.id + "]."));
                } else if (string.IsNullOrEmpty(adminContent.name)) {
                    //
                    // ----- Error: no content name
                    //
                    BlockEditForm = true;
                    Processor.Controllers.ErrorController.addUserError(core, "No content definition could be found For ContentID [" + adminContent.id + "]. This could be a menu Error. Please contact your application developer For more assistance.");
                    LogController.handleError(core, new ApplicationException("AdminClass.LoadEditRecord_Dbase, No content definition For ContentID [" + adminContent.id + "] could be found."));
                } else if (adminContent.tableName == "") {
                    //
                    // ----- Error: no content table
                    //
                    BlockEditForm = true;
                    Processor.Controllers.ErrorController.addUserError(core, "The content definition [" + adminContent.name + "] Is Not associated With a valid database table. Please contact your application developer For more assistance.");
                    LogController.handleError(core, new ApplicationException("AdminClass.LoadEditRecord_Dbase, No content definition For ContentID [" + adminContent.id + "] could be found."));
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
                    BlockEditForm = true;
                    Processor.Controllers.ErrorController.addUserError(core, "The content definition [" + adminContent.name + "] has no field records defined. Please contact your application developer For more assistance.");
                    LogController.handleError(core, new ApplicationException("AdminClass.LoadEditRecord_Dbase, Content [" + adminContent.name + "] has no fields defined."));
                } else {
                    //
                    //   Open Content Sets with the data
                    //
                    CSEditRecord = core.db.csOpen2(adminContent.name, editRecord.id, true, true);
                    //
                    //
                    // store fieldvalues in RecordValuesVariant
                    //
                    if (!(core.db.csOk(CSEditRecord))) {
                        //
                        //   Live or Edit records were not found
                        //
                        BlockEditForm = true;
                        Processor.Controllers.ErrorController.addUserError(core, "The information you have requested could not be found. The record could have been deleted, Or there may be a system Error.");
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
                            if (!editRecord.fieldsLc.ContainsKey(fieldNameLc)) {
                                editRecordField = new EditRecordFieldClass();
                                editRecord.fieldsLc.Add(fieldNameLc, editRecordField);
                            } else {
                                editRecordField = editRecord.fieldsLc[fieldNameLc];
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
                                        Processor.Controllers.ErrorController.addUserError(core, "The value for [" + adminContentcontent.caption + "] was empty but is required. This must be set before you can save this record.");
                                    } else {
                                        Processor.Controllers.ErrorController.addUserError(core, "The value for [" + adminContentcontent.caption + "] in tab [" + adminContentcontent.editTabName + "] was empty but is required. This must be set before you can save this record.");
                                    }
                                } else {
                                    //
                                    // if required and null, set value to the default
                                    //
                                    DBValueVariant = adminContentcontent.defaultValue;
                                    if (adminContentcontent.editTabName == "") {
                                        Processor.Controllers.ErrorController.addUserError(core, "The value for [" + adminContentcontent.caption + "] was null but is required. The default value Is shown, And will be saved if you save this record.");
                                    } else {
                                        Processor.Controllers.ErrorController.addUserError(core, "The value for [" + adminContentcontent.caption + "] in tab [" + adminContentcontent.editTabName + "] was null but is required. The default value Is shown, And will be saved if you save this record.");
                                    }
                                }
                            }
                            //
                            // Save EditRecord values
                            //
                            switch (GenericController.vbUCase(adminContentcontent.nameLc)) {
                                case "DATEADDED":
                                    editRecord.dateAdded = core.db.csGetDate(CSEditRecord, adminContentcontent.nameLc);
                                    break;
                                case "MODIFIEDDATE":
                                    editRecord.modifiedDate = core.db.csGetDate(CSEditRecord, adminContentcontent.nameLc);
                                    break;
                                case "CREATEDBY":
                                    int createdByPersonId = core.db.csGetInteger(CSEditRecord, adminContentcontent.nameLc);
                                    if (createdByPersonId == 0) {
                                        editRecord.createdBy = new PersonModel() { name = "system" };
                                    } else {
                                        editRecord.createdBy = PersonModel.create(core, createdByPersonId);
                                        if (editRecord.createdBy == null) {
                                            editRecord.createdBy = new PersonModel() { name = "deleted #" + createdByPersonId.ToString() };
                                        }
                                    }
                                    break;
                                case "MODIFIEDBY":
                                    int modifiedByPersonId = core.db.csGetInteger(CSEditRecord, adminContentcontent.nameLc);
                                    if (modifiedByPersonId == 0) {
                                        editRecord.modifiedBy = new PersonModel() { name = "system" };
                                    } else {
                                        editRecord.modifiedBy = PersonModel.create(core, modifiedByPersonId);
                                        if (editRecord.modifiedBy == null) {
                                            editRecord.modifiedBy = new PersonModel() { name = "deleted #" + modifiedByPersonId.ToString() };
                                        }
                                    }
                                    break;
                                case "ACTIVE":
                                    editRecord.active = core.db.csGetBoolean(CSEditRecord, adminContentcontent.nameLc);
                                    break;
                                case "CONTENTCONTROLID":
                                    editRecord.contentControlId = core.db.csGetInteger(CSEditRecord, adminContentcontent.nameLc);
                                    if (editRecord.contentControlId.Equals(0)) {
                                        editRecord.contentControlId = adminContent.id;
                                    }
                                    editRecord.contentControlId_Name = CdefController.getContentNameByID(core, editRecord.contentControlId);
                                    break;
                                case "ID":
                                    editRecord.id = core.db.csGetInteger(CSEditRecord, adminContentcontent.nameLc);
                                    break;
                                case "MENUHEADLINE":
                                    editRecord.menuHeadline = core.db.csGetText(CSEditRecord, adminContentcontent.nameLc);
                                    break;
                                case "NAME":
                                    editRecord.nameLc = core.db.csGetText(CSEditRecord, adminContentcontent.nameLc);
                                    break;
                                case "PARENTID":
                                    editRecord.parentID = core.db.csGetInteger(CSEditRecord, adminContentcontent.nameLc);
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
        // ====================================================================================================
        //   Read the Form into the fields array
        //
        public void LoadEditRecord_Request(CoreController core) {
            try {
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
                if (AllowAdminFieldCheck(core) && (FormFieldLcListToBeLoaded.Count == 0)) {
                    //
                    // The field list was not returned
                    Processor.Controllers.ErrorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [no field list].");
                } else if (AllowAdminFieldCheck(core) && (FormEmptyFieldLcList.Count == 0)) {
                    //
                    // The field list was not returned
                    Processor.Controllers.ErrorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [no empty field list].");
                } else {
                    //
                    // fixup the string so it can be reduced by each field found, leaving and empty string if all correct
                    //
                    var tmpList = new List<string>();
                    DataSourceModel datasource = DataSourceModel.create(core, adminContent.dataSourceId, ref tmpList);
                    //DataSourceName = core.db.getDataSourceNameByID(adminContext.content.dataSourceId)
                    foreach (var keyValuePair in adminContent.fields) {
                        CDefFieldModel field = keyValuePair.Value;
                        LoadEditRecord_RequestField(core, field, datasource.name, FormFieldLcListToBeLoaded, FormEmptyFieldLcList);
                    }
                    //
                    // If there are any form fields that were no loaded, flag the error now
                    //
                    if (AllowAdminFieldCheck(core) & (FormFieldLcListToBeLoaded.Count > 0)) {
                        Processor.Controllers.ErrorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The following fields where Not found [" + string.Join(",", FormFieldLcListToBeLoaded) + "].");
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
        // ====================================================================================================
        //   Read the Form into the fields array
        //
        public void LoadEditRecord_RequestField(CoreController core, CDefFieldModel field, string ignore, List<string> FormFieldLcListToBeLoaded, List<string> FormEmptyFieldLcList) {
            try {
                // todo
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
                            if (AllowAdminFieldCheck(core)) {
                                if (!core.docProperties.containsKey(field.nameLc.ToUpper())) {
                                    if (!(core.doc.debug_iUserError != "")) {
                                        //
                                        // Add user error only for the first missing field
                                        //
                                        Processor.Controllers.ErrorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try again, taking care Not To submit the page until your browser has finished loading. If this Error occurs again, please report this problem To your site administrator. The first Error was [" + field.nameLc + " Not found]. There may have been others.");
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
                            if (AllowAdminFieldCheck(core) && (!InResponse) && (!InEmptyFieldList)) {
                                Processor.Controllers.ErrorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [" + field.nameLc + " Not found].");
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
                            if (AllowAdminFieldCheck(core)) {
                                if ((!InResponse) && (!InEmptyFieldList)) {
                                    Processor.Controllers.ErrorController.addUserError(core, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [" + field.nameLc + " Not found].");
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
                            } else if (AllowAdminFieldCheck(core) && (!InResponse) && (!InEmptyFieldList)) {
                                //
                                // Was sent out non-blank, and no response back, flag error and leave the current value to a retry
                                string errorMessage = "There has been an error reading the response from your browser. The field[" + field.caption + "]" + TabCopy + " was missing. Please Try your change again. If this error happens repeatedly, please report this problem to your site administrator.";
                                Processor.Controllers.ErrorController.addUserError(core, errorMessage);
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
                                                Processor.Controllers.ErrorController.addUserError(core, "The record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be a numeric value.");
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
                                                Processor.Controllers.ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be a numeric value.");
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
                                                Processor.Controllers.ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " had an invalid selection.");
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
                                                Processor.Controllers.ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be a date And/Or time in the form mm/dd/yy 0000 AM(PM).");
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
                                            core.userProperty.setProperty(adminContent.name + "." + field.nameLc + ".RowHeight", EditorRowHeight);
                                        }
                                        EditorPixelHeight = core.docProperties.getInteger(field.nameLc + "PixelHeight");
                                        if (EditorPixelHeight != 0) {
                                            core.userProperty.setProperty(adminContent.name + "." + field.nameLc + ".PixelHeight", EditorPixelHeight);
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
                                            core.userProperty.setProperty(adminContent.name + "." + field.nameLc + ".RowHeight", EditorRowHeight);
                                        }
                                        EditorPixelHeight = core.docProperties.getInteger(field.nameLc + "PixelHeight");
                                        if (EditorPixelHeight != 0) {
                                            core.userProperty.setProperty(adminContent.name + "." + field.nameLc + ".PixelHeight", EditorPixelHeight);
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
                                        CS = core.db.csOpen(adminContent.name, "ID=" + ParentID, "", true, 0, false, false, "ParentID");
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
                                        Processor.Controllers.ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " creates a relationship between records that Is too large. Please limit the depth of this relationship to " + LoopPtrMax + " records.");
                                        ResponseFieldValueIsOKToSave = false;
                                    } else if ((editRecord.id != 0) && (editRecord.id == ParentID)) {
                                        //
                                        // Reference to iteslf
                                        //
                                        Processor.Controllers.ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " contains a circular reference. This record points back to itself. This Is Not allowed.");
                                        ResponseFieldValueIsOKToSave = false;
                                    } else if (ParentID != 0) {
                                        //
                                        // Circular reference
                                        //
                                        Processor.Controllers.ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " contains a circular reference. This field either points to other records which then point back to this record. This Is Not allowed.");
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
                                    Processor.Controllers.ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " Is required but has no value.");
                                    ResponseFieldValueIsOKToSave = false;
                                }
                                //
                                // special case - people records without Allowduplicateusername require username to be unique
                                //
                                if (GenericController.vbLCase(adminContent.tableName) == "ccmembers") {
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
                                    string SQLUnique = "select id from " + adminContent.tableName + " where (" + field.nameLc + "=" + core.db.encodeSQL(ResponseFieldValueText, field.fieldTypeId) + ")and(" + CdefController.getContentControlCriteria(core, adminContent.name) + ")";
                                    if (editRecord.id > 0) {
                                        //
                                        // --editing record
                                        SQLUnique = SQLUnique + "and(id<>" + editRecord.id + ")";
                                    }
                                    CSPointer = core.db.csOpenSql(SQLUnique, adminContent.dataSourceName);
                                    if (core.db.csOk(CSPointer)) {
                                        //
                                        // field is not unique, skip it and flag error
                                        //
                                        if (blockDuplicateUsername) {
                                            //
                                            //
                                            //
                                            Processor.Controllers.ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be unique and there Is another record with [" + ResponseFieldValueText + "]. This must be unique because the preference 'Allow Duplicate Usernames' is Not checked.");
                                        } else if (blockDuplicateEmail) {
                                            //
                                            //
                                            //
                                            Processor.Controllers.ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be unique and there is another record with [" + ResponseFieldValueText + "]. This must be unique because the preference 'Allow Email Login' is checked.");
                                        } else if (false) {
                                        } else {
                                            //
                                            // non-workflow
                                            //
                                            Processor.Controllers.ErrorController.addUserError(core, "This record cannot be saved because the field [" + field.caption + "]" + TabCopy + " must be unique and there is another record with [" + ResponseFieldValueText + "].");
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
    }
}
