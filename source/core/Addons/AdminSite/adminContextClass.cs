
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
using Contensive.Processor.Models.Complex;
using Contensive.Addons.Tools;
using static Contensive.Processor.AdminUIController;
//
namespace Contensive.Addons.AdminSite {
    /// <summary>
    /// object that contains the context for the admin site, like recordsPerPage, etc. Should eventually include the loadContext and be its own document
    /// </summary>
    public class adminContextClass {

        public const int AdminActionNop = 0; // do nothing
        public const int AdminActionDelete = 4; // delete record
        public const int AdminActionFind = 5;
        public const int AdminActionDeleteFilex = 6;
        public const int AdminActionUpload = 7;
        public const int AdminActionSaveNormal = 3; // save fields to database
        public const int AdminActionSaveEmail = 8; // save email record (and update EmailGroups) to database
        public const int AdminActionSaveMember = 11;
        public const int AdminActionSaveSystem = 12;
        public const int AdminActionSavePaths = 13; // Save a record that is in the BathBlocking Format
        public const int AdminActionSendEmail = 9;
        public const int AdminActionSendEmailTest = 10;
        public const int AdminActionNext = 14;
        public const int AdminActionPrevious = 15;
        public const int AdminActionFirst = 16;
        public const int AdminActionSaveContent = 17;
        public const int AdminActionSaveField = 18; // Save a single field, fieldname = fn input
        public const int AdminActionPublish = 19; // Publish record live
        public const int AdminActionAbortEdit = 20; // Publish record live
        public const int AdminActionPublishSubmit = 21; // Submit for Workflow Publishing
        public const int AdminActionPublishApprove = 22; // Approve for Workflow Publishing
                                                         //Public Const AdminActionWorkflowPublishApproved = 23    ' Publish what was approved
        public const int AdminActionSetHTMLEdit = 24; // Set Member Property for this field to HTML Edit
        public const int AdminActionSetTextEdit = 25; // Set Member Property for this field to Text Edit
        public const int AdminActionSave = 26; // Save Record
        public const int AdminActionActivateEmail = 27; // Activate a Conditional Email
        public const int AdminActionDeactivateEmail = 28; // Deactivate a conditional email
        public const int AdminActionDuplicate = 29; // Duplicate the (sent email) record
        public const int AdminActionDeleteRows = 30; // Delete from rows of records, row0 is boolean, rowid0 is ID, rowcnt is count
        public const int AdminActionSaveAddNew = 31; // Save Record and add a new record
        public const int AdminActionReloadCDef = 32; // Load Content Definitions
                                                     // Public Const AdminActionWorkflowPublishSelected = 33 ' Publish what was selected
        public const int AdminActionMarkReviewed = 34; // Mark the record reviewed without making any changes
        public const int AdminActionEditRefresh = 35; // reload the page just like a save, but do not save

        /// <summary>
        /// the content being edited
        /// </summary>
        public cdefModel adminContent = null;
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


        //
        //====================================================================================================
        // properties
        //====================================================================================================
        //
        // ----- ccGroupRules storage for list of Content that a group can author
        //
        public struct ContentGroupRuleType {
            public int ContentID;
            //public int GroupID;
            public bool AllowAdd;
            public bool AllowDelete;
        }
        ////
        //// ----- generic id/name dictionary
        ////
        //public struct StorageType {
        //    public int Id;
        //    public string Name;
        //}
        //
        // ----- Group Rules
        //
        public struct GroupRuleType {
            public int GroupID;
            public bool AllowAdd;
            public bool AllowDelete;
        }
        //
        // ----- Used within Admin site to create fancyBox popups
        //
        public bool includeFancyBox;
        public int fancyBoxPtr;
        public string fancyBoxHeadJS;
        public const bool allowSaveBeforeDuplicate = false;
        //
        // ----- To interigate Add-on Collections to check for re-use
        //
        //public struct DeleteType {
        //    public string Name;
        //    public int ParentID;
        //}
        //public struct NavigatorType {
        //    public string Name;
        //    public string menuNameSpace;
        //}
        //public struct Collection2Type {
        //    public int AddOnCnt;
        //    public string[] AddonGuid;
        //    public string[] AddonName;
        //    public int MenuCnt;
        //    public string[] Menus;
        //    public int NavigatorCnt;
        //    public NavigatorType[] Navigators;
        //}
        //public int CollectionCnt;
        //public Collection2Type[] Collections;
        //
        // ----- Target Data Storage
        //
        public int requestedContentId;
        public int requestedRecordId;
        //public false As Boolean    ' set if content and site support workflow authoring
        public bool BlockEditForm; // true if there was an error loading the edit record - use to block the edit form
                                    //
                                    //=============================================================================
                                    // ----- Control Response
                                    //=============================================================================
                                    //
        public int Admin_Action; // The action to be performed before the next form
        public int AdminSourceForm; // The form that submitted that the button to process
        public string[,] WherePair = new string[3, 11]; // for passing where clause values from page to page
        public int WherePairCount; // the current number of WherePairCount in use
                                    //public OrderByFieldPointer as integer
        public const int OrderByFieldPointerDefault = -1;
        //public Direction as integer
        public int RecordTop;
        public int RecordsPerPage;
        public const int RecordsPerPageDefault = 50;
        //public InputFieldName As String   ' Input FieldName used for DHTMLEdit

        public int ignore_legacyMenuDepth; // The number of windows open (below this one)
        public string TitleExtension; // String that adds on to the end of the title
        //
        // SpellCheck Features
        //
        //public bool SpellCheckSupported; // if true, spell checking is supported
        //public bool SpellCheckRequest; // If true, send the spell check form to the browser
        //
        //=============================================================================
        // preferences
        //=============================================================================
        //
        public int AdminMenuModeID; // Controls the menu mode, set from core.main_MemberAdminMenuModeID
        public bool allowAdminTabs; // true uses tab system
        public string fieldEditorPreference; // this is a hidden on the edit form. The popup editor preferences sets this hidden and submits
        //
        //=============================================================================
        //   Content Tracking Editing
        //
        //   These values are read from Edit form response, and are used to populate then
        //   ContentWatch and ContentWatchListRules records.
        //
        //   They are read in before the current record is processed, then processed and
        //   Saved back to ContentWatch and ContentWatchRules after the current record is
        //   processed, so changes to the record can be reflected in the ContentWatch records.
        //   For instance, if the record is marked inactive, the ContentWatchLink is cleared
        //   and all ContentWatchListRules are deleted.
        //
        //=============================================================================
        //
        public bool ContentWatchLoaded; // flag set that shows the rest are valid
        //
        public int ContentWatchRecordID;
        public string ContentWatchLink;
        public int ContentWatchClicks;
        public string ContentWatchLinkLabel;
        public DateTime ContentWatchExpires;
        public int[] ContentWatchListID; // list of all ContentWatchLists for this Content, read from response, then later saved to Rules
        public int ContentWatchListIDSize; // size of ContentWatchListID() array
        public int ContentWatchListIDCount; // number of valid entries in ContentWatchListID()
        //
        //=============================================================================
        // Other
        //=============================================================================
        // Count of Buttons in use
        public int ButtonObjectCount;
        public string[,] ImagePreloads = new string[3, 101];
        public string JavaScriptString; // Collected string of Javascript functions to print at end
        public string adminFooter; // the HTML needed to complete the Admin Form after contents
        public bool UserAllowContentEdit;
        public int FormInputCount; // used to generate labels for form input
        public int EditSectionPanelCount;

        public const string OpenLiveWindowTable = "<div ID=\"LiveWindowTable\">";
        public const string CloseLiveWindowTable = "</div>";
        //Const OpenLiveWindowTable = "<table ID=""LiveWindowTable"" border=0 cellpadding=0 cellspacing=0 width=""100%""><tr><td>"
        //Const CloseLiveWindowTable = "</td></tr></table>"
        //
        //Const adminUIController.EditTableClose = "<tr>" _
        //        & "<td width=20%><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1"" ></td>" _
        //        & "<td width=""70%""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1"" ></td>" _
        //        & "<td width=""10%""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1"" ></td>" _
        //        & "</tr>" _
        //        & "</table>"
        public const string AdminFormErrorOpen = "<table border=\"0\" cellpadding=\"20\" cellspacing=\"0\" width=\"100%\"><tr><td align=\"left\">";
        public const string AdminFormErrorClose = "</td></tr></table>";
        //
        // these were defined different in csv
        //
        public enum NodeTypeEnum {
            NodeTypeEntry = 0,
            NodeTypeCollection = 1,
            NodeTypeAddon = 2,
            NodeTypeContent = 3
        }
        //
        public const string IndexConfigPrefix = "IndexConfig:";
        /// <summary>
        /// loads the context for the admin site, controlled by request inputs like rnContent (cid) and rnRecordId (id)
        /// </summary>
        /// <param name="core"></param>
        public adminContextClass( CoreController core) {
            try {
                //
                // Tab Control
                allowAdminTabs = genericController.encodeBoolean(core.userProperty.getText("AllowAdminTabs", "1"));
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
                    adminContent = cdefModel.getCdef(core, requestedContentId);
                    if (adminContent == null) {
                        adminContent = new cdefModel();
                        adminContent.id = 0;
                        errorController.addUserError(core, "There is no content with the requested id [" + requestedContentId + "]");
                        requestedContentId = 0;
                    }
                }
                if (adminContent == null) {
                    adminContent = new cdefModel();
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
                        adminContent.id = core.db.csGetInteger(CS, "ContentControlID");
                        if (adminContent.id <= 0) {
                            adminContent.id = requestedContentId;
                        } else if (adminContent.id != requestedContentId) {
                            adminContent = cdefModel.getCdef(core, adminContent.id);
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
                    RecordsPerPage = RecordsPerPageDefault;
                }
                //
                // Read WherePairCount
                //
                WherePairCount = 99;
                int WCount = 0;
                for (WCount = 0; WCount <= 99; WCount++) {
                    WherePair[0, WCount] = genericController.encodeText(core.docProperties.getText("WL" + WCount));
                    if (WherePair[0, WCount] == "") {
                        WherePairCount = WCount;
                        break;
                    } else {
                        WherePair[1, WCount] = genericController.encodeText(core.docProperties.getText("WR" + WCount));
                        core.doc.addRefreshQueryString("wl" + WCount, genericController.encodeRequestVariable(WherePair[0, WCount]));
                        core.doc.addRefreshQueryString("wr" + WCount, genericController.encodeRequestVariable(WherePair[1, WCount]));
                    }
                }
                //
                // Read WhereClauseContent to WherePairCount
                //
                string WhereClauseContent = genericController.encodeText(core.docProperties.getText("wc"));
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
                        Admin_Action = AdminActionEditRefresh;
                        AdminForm = AdminFormEdit;
                        int Pos = genericController.vbInstr(1, fieldEditorPreference, ":");
                        if (Pos > 0) {
                            int fieldEditorFieldId = genericController.encodeInteger(fieldEditorPreference.Left(Pos - 1));
                            int fieldEditorAddonId = genericController.encodeInteger(fieldEditorPreference.Substring(Pos));
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
                                                Pos = genericController.vbInstr(1, Parts[Ptr], ",");
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
                logController.handleError(core, ex);
            }
            return;
        }

        //
        //========================================================================
        // Test Content Access -- return based on Admin/Developer/MemberRules
        //   if developer, let all through
        //   if admin, block if table is developeronly
        //   if member, run blocking query (which also traps adminonly and developer only)
        //       if blockin query has a null RecordID, this member gets everything
        //       if not null recordid in blocking query, use RecordIDs in result for Where clause on this lookup
        //========================================================================
        //
        public static  bool userHasContentAccess(CoreController core, int ContentID) {
            bool result = false;
            try {
                string ContentName = cdefModel.getContentNameByID(core, ContentID);
                if (!string.IsNullOrEmpty(ContentName)) {
                    result = core.session.isAuthenticatedContentManager(core, ContentName);
                }
            } catch (Exception ex) {
                logController.handleError(core, ex);
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
        public string GetWherePairValue(string FieldName) {
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
    }
}
