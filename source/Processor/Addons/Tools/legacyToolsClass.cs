
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
using static Contensive.Processor.Constants;
//
using System.Threading;
using Contensive.Processor.Exceptions;
using Contensive.Addons.AdminSite.Controllers;
using Contensive.Processor.Models.Domain;
using Contensive.BaseClasses;
//
namespace Contensive.Addons.Tools {
    public class LegacyToolsClass {
        //========================================================================
        // This file and its contents are copyright by Kidwell McGowan Associates.
        //========================================================================
        //
        // ----- global scope variables
        //
        //
        // ----- data fields
        //
        private string[] Findstring = new string[51]; 
        private CoreController core;
        private string ToolsTable;
        private string ToolsContentName;
        private bool DefaultReadOnly;
        private bool DefaultActive;
        private bool DefaultPassword;
        private bool DefaulttextBuffered;
        private bool DefaultRequired;
        private bool DefaultAdminOnly;
        private bool DefaultDeveloperOnly;
        private bool DefaultCreateBlankRecord;
        private bool defaultAddMenu;
        private string ToolsQuery;
        private string ToolsDataSource;
        //
        // ----- Forms
        //
        private int AdminFormTool;
        //
        // ----- Actions
        //
        private int ToolsAction;
        //
        // ----- Buttons
        //
        private string Button;
        private const int fieldType = 0; // The type of data the field holds
        private const int FieldName = 1; // The name of the field
        private const int FieldCaption = 2; // The caption for displaying the field
        private const int FieldValue = 3; // The value carried to and from the database
        private const int FieldReadOnly = 4; // If true, this field can not be written back to the database
        private const int FieldLookupContentID = 5; // If TYPELOOKUP, (for Db controled sites) this is the content ID of the source table
        private const int FieldRequired = 7; // if true, this field must be entered
        private const int FieldDefaultVariant = 8; // default value on a new record
        private const int FieldHelpMessage = 9; // explaination of this field
        private const int FieldUniqueName = 10; // not supported -- must change name anyway
        private const int FieldTextBuffered = 11; // if true, the input is run through RemoveControlCharacters()
        private const int FieldPassword = 12; // for text boxes, sets the password attribute
        private const int FieldRedirectContentID = 13; // If TYPEREDIRECT, this is new contentID
        private const int FieldRedirectID = 14; // If TYPEREDIRECT, this is the field that must match ID of this record
        private const int FieldRedirectPath = 15; // New Field, If TYPEREDIRECT, this is the path to the next page (if blank, current page is used)
        private const int fieldId = 16; // the ID in the ccContentFields Table that this came from
        private const int FieldBlockAccess = 17; // do not send field out to user because they do not have access rights
        private const int FIELDATTRIBUTEMAX = 17; // the maximum value of the attributes (second) FIELD index
                                                  //
                                                  // ----- Diagnostic Action Types
                                                  //
        //private int DiagActionCount;
        private const int DiagActionCountMax = 10;
        ////
        //private class DiagActionType {
        //    public string Name;
        //    public string Command;
        //}
        private const int DiagActionNop = 0; // no arguments
        private const int DiagActionSetFieldType = 1; // Arg(1)=ContentName, Arg(2)=FieldName, Arg(3)=TypeID
        private const int DiagActionDeleteRecord = 2; // Arg(1)=ContentName, Arg(2)=RecordID
        private const int DiagActionContentDeDupe = 3; // Arg(1)=ContentName
        private const int DiagActionSetFieldInactive = 4; // Arg(1)=ContentName, Arg(2)=FieldName
        private const int DiagActionSetRecordInactive = 5; // Arg(1)=ContentName, Arg(2)=RecordID
        private const int DiagActionAddStyle = 6; // Arg(1)=Stylename from sample stylesheet
        private const int DiagActionAddStyleSheet = 7; // no arguments
        private const int DiagActionSetFieldNotRequired = 8; // Arg(1)=ContentName, Arg(2)=RecordID
                                                             //
        private const string ContentFileViewDefaultCopy = "<P>The Content File View displays the files which store content which may be too large to include in your database.</P>";
        //
        //========================================================================
        //   This header file controls the Admin Developer Tools arcitecture.
        //   It should be called from within the admin form structure
        //
        // Features:
        //   03/15/00
        //       add a table Import function
        //
        // Issues:
        //
        // Contains:
        //   ToolsInit()
        //       is called within this code -- put this code first
        //
        //   GetForm()
        //       Prints the entire Admin Menu page structure, with the
        //       form in the content window. Uses GetFormTop() and
        //       AdminPageBottom() to complete the page.
        //
        // Request Input:

        //
        // Input:
        //   dta ToolsAction      Action to preform during init
        //   dtf AdminFormTool        next form to display in content window
        //   Button Button      Submit button selected in previous form
        //   dtt ToolsTable       Table used for Create Content from table button
        //   dtei    MenuEntryID
        //   dthi    MenuHeaderID
        //   dtmd    MenuDirection
        //   dtcn    ToolsColumnNumber    Used by Configure Index
        //   dtfe    ToolsFieldEdit   used together with field number and fieldname - dtfaActivex, where x=fieldID
        //   dtq ToolsQuery       used for manual query form
        //   dtdreadonly DefaultReadOnly
        //   dtdactive   DefaultActive
        //   dtdpassword DefaultPassword
        //   dtdtextbuffered DefaulttextBuffered
        //   dtdrequired DefaultRequired
        //   dtdadmin    DefaultAdminOnly
        //   dtddev      DefaultDeveloperOnly
        //   dtdam       DefaultAddMenu
        //   dtblank     DefaultCreateBlankRecord
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public LegacyToolsClass(CoreController core) : base() {
            this.core = core;
        }
        //
        //========================================================================
        /// <summary>
        /// Print the Tools page from the admin site
        /// </summary>
        /// <returns></returns>
        public string getToolsList() {
            string tempGetForm = null;
            try {
                //
                int MenuEntryID = 0;
                int MenuHeaderID = 0;
                int MenuDirection = 0;
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                //
                Button = core.docProperties.getText("Button");
                if (Button == ButtonCancelAll) {
                    //
                    // Cancel to the admin site
                    //
                    return core.webServer.redirect( core.appConfig.adminRoute, "Tools-List, cancel button");
                }
                //
                // ----- Check permissions
                //
                if (!core.session.isAuthenticatedAdmin()) {
                    //
                    // You must be admin to use this feature
                    //
                    tempGetForm = AdminUIController.getFormBodyAdminOnly();
                    tempGetForm = AdminUIController.getBody(core,"Admin Tools", ButtonCancelAll, "", false, false, "<div>Administration Tools</div>", "", 0, tempGetForm);
                } else {
                    ToolsAction = core.docProperties.getInteger("dta");
                    Button = core.docProperties.getText("Button");
                    AdminFormTool = core.docProperties.getInteger(RequestNameAdminForm);
                    ToolsTable = core.docProperties.getText("dtt");
                    ToolsContentName = core.docProperties.getText("ContentName");
                    ToolsQuery = core.docProperties.getText("dtq");
                    ToolsDataSource = core.docProperties.getText("dtds");
                    MenuEntryID = core.docProperties.getInteger("dtei");
                    MenuHeaderID = core.docProperties.getInteger("dthi");
                    MenuDirection = core.docProperties.getInteger("dtmd");
                    DefaultReadOnly = core.docProperties.getBoolean("dtdreadonly");
                    DefaultActive = core.docProperties.getBoolean("dtdactive");
                    DefaultPassword = core.docProperties.getBoolean("dtdpassword");
                    DefaulttextBuffered = core.docProperties.getBoolean("dtdtextbuffered");
                    DefaultRequired = core.docProperties.getBoolean("dtdrequired");
                    DefaultAdminOnly = core.docProperties.getBoolean("dtdadmin");
                    DefaultDeveloperOnly = core.docProperties.getBoolean("dtddev");
                    defaultAddMenu = core.docProperties.getBoolean("dtdam");
                    DefaultCreateBlankRecord = core.docProperties.getBoolean("dtblank");
                    //
                    core.doc.addRefreshQueryString("dta", ToolsAction.ToString());
                    //
                    if (Button == ButtonCancel) {
                        //
                        // -- Cancel
                        return core.webServer.redirect(core.appConfig.adminRoute, "Tools-List, cancel button");
                    } else {
                        //
                        // -- Print out the page
                        switch (AdminFormTool) {
                            //case AdminFormToolContentDiagnostic:
                            //    //
                            //    Stream.Add(GetForm_ContentDiagnostic());
                            //    break;
                            case AdminFormToolCreateContentDefinition:
                                //
                                Stream.Add(GetForm_CreateContentDefinition());
                                break;
                            case AdminFormToolDefineContentFieldsFromTable:
                                //
                                Stream.Add((new DefineContentFieldsFromTableClass()).Execute(core.cp_forAddonExecutionOnly).ToString());
                                break;
                            case AdminFormToolConfigureListing:
                                //
                                Stream.Add(GetForm_ConfigureListing());
                                break;
                            case AdminFormToolConfigureEdit:
                                //
                                //Call Stream.Add(core.addon.execute(guid_ToolConfigureEdit))
                                Stream.Add( ConfigureContentEditClass.configureContentEdit(core.cp_forAddonExecutionOnly));
                                break;
                            case AdminFormToolManualQuery:
                                //
                                Stream.Add( ManualQueryClass.getForm_ManualQuery(core.cp_forAddonExecutionOnly));
                                break;
                            case AdminFormToolCreateChildContent:
                                //
                                Stream.Add(GetForm_CreateChildContent());
                                break;
                            case AdminFormToolSyncTables:
                                //
                                Stream.Add(GetForm_SyncTables());
                                break;
                            case AdminFormToolBenchmark:
                                //
                                Stream.Add(GetForm_Benchmark());
                                break;
                            case AdminFormToolClearContentWatchLink:
                                //
                                Stream.Add(GetForm_ClearContentWatchLinks());
                                break;
                            case AdminFormToolSchema:
                                //
                                Stream.Add(GetForm_DbSchema());
                                break;
                            case AdminFormToolContentFileView:
                                //
                                Stream.Add(GetForm_ContentFileManager());
                                break;
                            case AdminFormToolWebsiteFileView:
                                //
                                Stream.Add(GetForm_WebsiteFileManager());
                                break;
                            case AdminFormToolDbIndex:
                                //
                                Stream.Add(GetForm_DbIndex());
                                break;
                            case AdminFormToolContentDbSchema:
                                //
                                Stream.Add(GetForm_ContentDbSchema());
                                break;
                            case AdminFormToolLogFileView:
                                //
                                Stream.Add(GetForm_LogFiles());
                                break;
                            case AdminFormToolLoadCDef:
                                //
                                Stream.Add(GetForm_LoadCDef());
                                break;
                            case AdminFormToolLoadTemplates:
                                //
                                Stream.Add(GetForm_LoadTemplates());
                                break;
                            case AdminformToolFindAndReplace:
                                //
                                Stream.Add(GetForm_FindAndReplace());
                                break;
                            case AdminformToolIISReset:
                                //
                                Stream.Add(GetForm_IISReset());
                                break;
                            case AdminformToolCreateGUID:
                                //
                                Stream.Add(GetForm_CreateGUID());
                                break;
                            default:
                                //
                                Stream.Add(GetForm_Root());
                                break;
                        }
                    }
                    //        Call Stream.Add("</blockquote>")
                    tempGetForm = Stream.Text;
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return tempGetForm;
        }
        //
        //=============================================================================
        //   prints a menu of Tools in a 100% table
        //=============================================================================
        //
        private string GetForm_Root() {
            string result = "";
            try {
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                string ButtonList;
                //
                ButtonList = ButtonCancelAll;
                //
                Stream.Add("<P class=\"ccAdminNormal\"><b>Tools</b></P>");
                Stream.Add("<table border=\"0\" cellpadding=\"4\" cellspacing=\"0\" width=\"100%\">");
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolContentDiagnostic, "Run Content Diagnostic", "General diagnostic routine that runs through the most common data structure errors."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolManualQuery, "Run Manual Query", "Run a query through Contensives database interface to change data directly, or simulate the activity of a errant query."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolCreateContentDefinition, "Create Content Definition", "Create a new content definition. If the required SQL table does not exist, it will also be created. If the SQL table exists, its fields will be included in the Content Definition. All required Content Definition fields will be added to the SQL table in either case."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolCreateChildContent, "Create Content Definition Child", "Create a new content definition based on a specific current definition, and make the new definition a hierarchical child of the original."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolConfigureEdit, "Edit Content Definition Fields", "Modify the administration interface editing page for a specific content definition."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolConfigureListing, "Edit Content Definition Admin Site Listing Columns", "Modify the administration interface listings page for a specific content definition. This is the first form you come to when selecting data to be modified."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolClearContentWatchLink, "Clear ContentWatch Links", "Clear all content watch links so they can be repopulated. Use this tool whenever there has been a domain name change, or errant links are found in the Whats New functionality."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolSyncTables, "Synchronize SQL Tables to their Content Definitions", "Synchronize the Database tables to match the current content definitions. Add any necessary table fields, but do not remove unused fields."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolBenchmark, "Benchmark Current System", "Run a series of data operations and compare the result to previous benchmarks."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolSchema, "Get Database Schema", "Get the database schema for any table."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolContentFileView, "Content File View", "View files and folders in the Content Files area."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolDbIndex, "Modify Database Indexes", "Add or remove Database indexes."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolContentDbSchema, "Get the Content Database Schema", "View the database schema required by the Content Definitions on this site."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolLogFileView, "Get the Content Log Files", "View the Contensive Log Files."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolLoadCDef, "Load the Content Definitions", "Reload the Contensive Content Definitions."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolLoadTemplates, "Import Templates from the wwwRoot folder", "Use this tool to create template records from the HTML files in the root folder of the site."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminformToolFindAndReplace, "Find and Replace", "Use this tool to replace text strings in content throughout the site."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminformToolIISReset, "IIS Reset", "IIS Reset the server. This will stop all sites on the server for a short period."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolRestart, "Contensive Application Restart", "Restart the Contensive Applicaiton. This will stop your site on the server for a short period."));
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminformToolCreateGUID, "Create GUID", "Use this tool to create a new GUID. This is useful when creating a new core.addon."));
                Stream.Add("</table>");
                result = AdminUIController.getToolForm(core, Stream.Text, ButtonList);
                //result = adminUIController.getToolFormOpen(core, ButtonList) + Stream.Text + adminUIController.getToolFormClose(core, ButtonList);
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result; 
        }
        //
        //==================================================================================
        //
        //==================================================================================
        //
        private string GetForm_RootRow(int AdminFormId, int AdminFormToolId, string Caption, string Description) {
            string tempGetForm_RootRow = null;
            try {
                //
                tempGetForm_RootRow = "";
                tempGetForm_RootRow = tempGetForm_RootRow + "<tr><td colspan=\"2\">";
                tempGetForm_RootRow = tempGetForm_RootRow + SpanClassAdminNormal + "<P class=\"ccAdminNormal\"><A href=\"" + core.webServer.requestPage + "?af=" + AdminFormToolId.ToString() + "\"><B>" + Caption + "</b></SPAN></A></p>";
                tempGetForm_RootRow = tempGetForm_RootRow + "</td></tr>";
                if (!string.IsNullOrEmpty(Description)) {
                    tempGetForm_RootRow = tempGetForm_RootRow + "<tr><td width=\"30\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/spacer.gif\" height=\"1\" width=\"30\"></td>";
                    tempGetForm_RootRow = tempGetForm_RootRow + "<td width=\"100%\"><P class=\"ccAdminsmall\">" + Description + "</p></td></tr>";
                }
                return tempGetForm_RootRow;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return tempGetForm_RootRow;
        }
        //
        //=============================================================================
        // Create a Content Definition from a table
        //=============================================================================
        //
        private string GetForm_CreateContentDefinition() {
            string result = "";
            try {
                //
                int ContentID = 0;
                string TableName = "";
                string ContentName = "";
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                string ButtonList = null;
                string Description = null;
                string Caption = null;
                int NavID = 0;
                int ParentNavID = 0;
                DataSourceModel datasource = DataSourceModel.create(core, core.docProperties.getInteger("DataSourceID"));
                //
                ButtonList = ButtonCancel + "," + ButtonRun;
                Caption = "Create Content Definition";
                Description = "This tool creates a Content Definition. If the SQL table exists, it is used. If it does not exist, it is created. If records exist in the table with a blank ContentControlID, the ContentControlID will be populated from this new definition. A Navigator Menu entry will be added under Manage Site Content - Advanced.";
                //
                //Stream.Add( GetTitle(Caption, Description)
                //
                //   print out the submit form
                //
                if (core.docProperties.getText("Button") != "") {
                    //
                    // Process input
                    //
                    ContentName = core.docProperties.getText("ContentName");
                    TableName = core.docProperties.getText("TableName");
                    //
                    Stream.Add(SpanClassAdminSmall);
                    Stream.Add("<P>Creating content [" + ContentName + "] on table [" + TableName + "] on Datasource [" + datasource.name + "].</P>");
                    if ((!string.IsNullOrEmpty(ContentName)) && (!string.IsNullOrEmpty(TableName)) && (!string.IsNullOrEmpty(datasource.name))) {
                        using (var db = new DbController(core, datasource.name)) {
                            db.createSQLTable(TableName);
                        }
                        ContentMetadataModel.createFromSQLTable(core,datasource, TableName, ContentName);
                        core.cache.invalidateAll();
                        core.clearMetaData();
                        ContentID = Processor.Models.Domain.ContentMetadataModel.getContentId(core, ContentName);
                        ParentNavID = MetadataController.getRecordIdByUniqueName( core,Processor.Models.Db.NavigatorEntryModel.contentName, "Manage Site Content");
                        if (ParentNavID != 0) {
                            ParentNavID = 0;
                            using (var csSrc = new CsModel(core)) {
                                if (csSrc.open(NavigatorEntryModel.contentName, "(name=" + DbController.encodeSQLText("Advanced") + ")and(parentid=" + ParentNavID + ")")) {
                                    ParentNavID = csSrc.getInteger("ID");
                                }
                            }
                            if (ParentNavID != 0) {
                                using (var csDest = new CsModel(core)) {
                                    csDest.open(NavigatorEntryModel.contentName, "(name=" + DbController.encodeSQLText(ContentName) + ")and(parentid=" + NavID + ")");
                                    if (!csDest.ok()) {
                                        csDest.close();
                                        csDest.insert(NavigatorEntryModel.contentName);
                                    }
                                    if (csDest.ok()) {
                                        csDest.set("name", ContentName);
                                        csDest.set("parentid", ParentNavID);
                                        csDest.set("contentid", ContentID);
                                    }
                                }
                            }
                        }
                        ContentID = ContentMetadataModel.getContentId(core, ContentName);
                        Stream.Add("<P>Content Definition was created. An admin menu entry for this definition has been added under 'Site Content', and will be visible on the next page view. Use the [<a href=\"?af=105&ContentID=" + ContentID + "\">Edit Content Definition Fields</a>] tool to review and edit this definition's fields.</P>");
                    } else {
                        Stream.Add("<P>Error, a required field is missing. Content not created.</P>");
                    }
                    Stream.Add("</SPAN>");
                }
                Stream.Add(SpanClassAdminNormal);
                Stream.Add("Data Source<br>");
                Stream.Add(core.html.selectFromContent("DataSourceID", datasource.id, "Data Sources", "", "Default"));
                Stream.Add("<br><br>");
                Stream.Add("Content Name<br>");
                Stream.Add(HtmlController.inputText( core,"ContentName", ContentName, 1, 40));
                Stream.Add("<br><br>");
                Stream.Add("Table Name<br>");
                Stream.Add(HtmlController.inputText( core,"TableName", TableName, 1, 40));
                Stream.Add("<br><br>");
                Stream.Add("</SPAN>");
                result = AdminUIController.getBody(core,Caption, ButtonList, "", false, false, Description, "", 10, Stream.Text);
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   Print the Configure Listing Page
        //=============================================================================
        //
        private string GetForm_ConfigureListing() {
            string result = "";
            try {
                //
                const string RequestNameAddField = "addfield";
                const string RequestNameAddFieldID = "addfieldID";
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                Stream.Add(AdminUIController.getToolFormTitle("Configure Admin Listing", "Configure the Administration Content Listing Page."));
                //
                //   Load Request
                ToolsAction = core.docProperties.getInteger("dta");
                int TargetFieldID = core.docProperties.getInteger("fi");
                int ContentID = core.docProperties.getInteger(RequestNameToolContentID);
                string FieldNameToAdd = GenericController.vbUCase(core.docProperties.getText(RequestNameAddField));
                int FieldIDToAdd = core.docProperties.getInteger(RequestNameAddFieldID);
                string ButtonList = ButtonCancel + "," + ButtonSelect;
                bool ReloadCDef = core.docProperties.getBoolean("ReloadCDef");
                bool AllowContentAutoLoad = false;
                //
                //--------------------------------------------------------------------------------
                // Process actions
                //--------------------------------------------------------------------------------
                //
                if (ContentID != 0) {
                    ButtonList = ButtonCancel + "," + ButtonSaveandInvalidateCache;
                    string ContentName = Local_GetContentNameByID(ContentID);
                    Processor.Models.Domain.ContentMetadataModel CDef = Processor.Models.Domain.ContentMetadataModel.create(core, ContentID, false, true);
                    string FieldName = null;
                    int ColumnWidthTotal = 0;
                    int fieldId = 0;
                    if (ToolsAction != 0) {
                        //
                        // Block contentautoload, then force a load at the end
                        //
                        AllowContentAutoLoad = (core.siteProperties.getBoolean("AllowContentAutoLoad", true));
                        core.siteProperties.setProperty("AllowContentAutoLoad", false);
                        int SourceContentID = 0;
                        string SourceName = null;
                        //
                        // Make sure the FieldNameToAdd is not-inherited, if not, create new field
                        //
                        if (FieldIDToAdd != 0) {
                            foreach (var keyValuePair in CDef.fields) {
                                Processor.Models.Domain.ContentFieldMetadataModel field = keyValuePair.Value;
                                if (field.id == FieldIDToAdd) {
                                    //If field.Name = FieldNameToAdd Then
                                    if (field.inherited) {
                                        SourceContentID = field.contentId;
                                        SourceName = field.nameLc;
                                        using (var CSSource = new CsModel(core)) {
                                            CSSource.open("Content Fields", "(ContentID=" + SourceContentID + ")and(Name=" + DbController.encodeSQLText(SourceName) + ")");
                                            if (CSSource.ok()) {
                                                using (var CSTarget = new CsModel(core)) {
                                                    CSTarget.insert("Content Fields");
                                                    if (CSTarget.ok()) {
                                                        CSSource.copyRecord(CSTarget);
                                                        CSTarget.set("ContentID", ContentID);
                                                        ReloadCDef = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        //
                        // Make sure all fields are not-inherited, if not, create new fields
                        //
                        int ColumnNumberMax = 0;
                        foreach (var keyValuePair in CDef.adminColumns) {
                            Processor.Models.Domain.ContentMetadataModel.MetaAdminColumnClass adminColumn = keyValuePair.Value;
                            Processor.Models.Domain.ContentFieldMetadataModel field = CDef.fields[adminColumn.Name];
                            if (field.inherited) {
                                SourceContentID = field.contentId;
                                SourceName = field.nameLc;
                                using (var CSSource = new CsModel(core)) {
                                    if (CSSource.open("Content Fields", "(ContentID=" + SourceContentID + ")and(Name=" + DbController.encodeSQLText(SourceName) + ")")) {
                                        using (var CSTarget = new CsModel(core)) {
                                            if (CSTarget.insert("Content Fields")) {
                                                CSSource.copyRecord(CSTarget);
                                                CSTarget.set("ContentID", ContentID);
                                                ReloadCDef = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (ColumnNumberMax < field.indexColumn) {
                                ColumnNumberMax = field.indexColumn;
                            }
                            ColumnWidthTotal = ColumnWidthTotal + adminColumn.Width;
                        }
                        //
                        // ----- Perform any actions first
                        //
                        int columnPtr = 0;
                        bool MoveNextColumn = false;
                        switch (ToolsAction) {
                            case ToolsActionAddField: {
                                    //
                                    // Add a field to the Listing Page
                                    //
                                    if (FieldIDToAdd != 0) {
                                        //If FieldNameToAdd <> "" Then
                                        columnPtr = 0;
                                        if (CDef.adminColumns.Count > 1) {
                                            foreach (var keyValuePair in CDef.adminColumns) {
                                                Processor.Models.Domain.ContentMetadataModel.MetaAdminColumnClass adminColumn = keyValuePair.Value;
                                                Processor.Models.Domain.ContentFieldMetadataModel field = CDef.fields[adminColumn.Name];
                                                using (var csData = new CsModel(core)) {
                                                    csData.openRecord("Content Fields", field.id);
                                                    csData.set("IndexColumn", (columnPtr) * 10);
                                                    csData.set("IndexWidth", Math.Floor((adminColumn.Width * 80) / (double)ColumnWidthTotal));
                                                }
                                                columnPtr += 1;
                                            }
                                        }
                                        using (var csData = new CsModel(core)) {
                                            if (csData.openRecord("Content Fields", FieldIDToAdd)) {
                                                csData.set("IndexColumn", columnPtr * 10);
                                                csData.set("IndexWidth", 20);
                                                csData.set("IndexSortPriority", 99);
                                                csData.set("IndexSortDirection", 1);
                                            }
                                        }
                                        ReloadCDef = true;
                                    }
                                    //
                                    break;
                                }
                            case ToolsActionRemoveField: {
                                    //
                                    // Remove a field to the Listing Page
                                    //
                                    if (CDef.adminColumns.Count > 1) {
                                        columnPtr = 0;
                                        foreach (var keyValuePair in CDef.adminColumns) {
                                            Processor.Models.Domain.ContentMetadataModel.MetaAdminColumnClass adminColumn = keyValuePair.Value;
                                            Processor.Models.Domain.ContentFieldMetadataModel field = CDef.fields[adminColumn.Name];
                                            using (var csData = new CsModel(core)) {
                                                csData.openRecord("Content Fields", field.id);
                                                if (fieldId == TargetFieldID) {
                                                    csData.set("IndexColumn", 0);
                                                    csData.set("IndexWidth", 0);
                                                    csData.set("IndexSortPriority", 0);
                                                    csData.set("IndexSortDirection", 0);
                                                } else {
                                                    csData.set("IndexColumn", (columnPtr) * 10);
                                                    csData.set("IndexWidth", Math.Floor((adminColumn.Width * 100) / (double)ColumnWidthTotal));
                                                }
                                            }
                                            columnPtr += 1;
                                        }
                                        ReloadCDef = true;
                                    }
                                    break;
                                }
                            case ToolsActionMoveFieldRight: {
                                    //
                                    // Move column field right
                                    //
                                    if (CDef.adminColumns.Count > 1) {
                                        MoveNextColumn = false;
                                        columnPtr = 0;
                                        foreach (var keyValuePair in CDef.adminColumns) {
                                            Processor.Models.Domain.ContentMetadataModel.MetaAdminColumnClass adminColumn = keyValuePair.Value;
                                            Processor.Models.Domain.ContentFieldMetadataModel field = CDef.fields[adminColumn.Name];
                                            FieldName = adminColumn.Name;
                                            using (var csData = new CsModel(core)) {
                                                csData.openRecord("Content Fields", field.id);
                                                if ((CDef.fields[FieldName.ToLowerInvariant()].id == TargetFieldID) && (columnPtr < CDef.adminColumns.Count)) {
                                                    csData.set("IndexColumn", (columnPtr + 1) * 10);
                                                    //
                                                    MoveNextColumn = true;
                                                } else if (MoveNextColumn) {
                                                    //
                                                    // This is one past target
                                                    //
                                                    csData.set("IndexColumn", (columnPtr - 1) * 10);
                                                    MoveNextColumn = false;
                                                } else {
                                                    //
                                                    // not target or one past target
                                                    //
                                                    csData.set("IndexColumn", (columnPtr) * 10);
                                                    MoveNextColumn = false;
                                                }
                                                csData.set("IndexWidth", Math.Floor((adminColumn.Width * 100) / (double)ColumnWidthTotal));
                                            }
                                            columnPtr += 1;
                                        }
                                        ReloadCDef = true;
                                    }
                                    // end case
                                    break;
                                }
                            case ToolsActionMoveFieldLeft: {
                                    //
                                    // Move Index column field left
                                    //
                                    if (CDef.adminColumns.Count > 1) {
                                        MoveNextColumn = false;
                                        columnPtr = 0;
                                        foreach (var keyValuePair in CDef.adminColumns.Reverse()) {
                                            Processor.Models.Domain.ContentMetadataModel.MetaAdminColumnClass adminColumn = keyValuePair.Value;
                                            Processor.Models.Domain.ContentFieldMetadataModel field = CDef.fields[adminColumn.Name];
                                            FieldName = adminColumn.Name;
                                            using (var csData = new CsModel(core)) {
                                                csData.openRecord("Content Fields", field.id);
                                                if ((field.id == TargetFieldID) && (columnPtr < CDef.adminColumns.Count)) {
                                                    csData.set("IndexColumn", (columnPtr - 1) * 10);
                                                    //
                                                    MoveNextColumn = true;
                                                } else if (MoveNextColumn) {
                                                    //
                                                    // This is one past target
                                                    //
                                                    csData.set("IndexColumn", (columnPtr + 1) * 10);
                                                    MoveNextColumn = false;
                                                } else {
                                                    //
                                                    // not target or one past target
                                                    //
                                                    csData.set("IndexColumn", (columnPtr) * 10);
                                                    MoveNextColumn = false;
                                                }
                                                csData.set("IndexWidth", Math.Floor((adminColumn.Width * 100) / (double)ColumnWidthTotal));
                                            }
                                            columnPtr += 1;
                                        }
                                        ReloadCDef = true;
                                    }
                                    break;
                                }
                        }
                        //
                        // Get a new copy of the content definition
                        //
                        CDef = Processor.Models.Domain.ContentMetadataModel.create(core, ContentID, false, true);
                    }
                    if (Button == ButtonSaveandInvalidateCache) {
                        core.cache.invalidateAll();
                        core.clearMetaData();
                        return core.webServer.redirect("?af=" + AdminFormToolConfigureListing + "&ContentID=" + ContentID, "Tools-ConfigureListing, Save and Invalidate Cache, Go to back ConfigureListing tools");
                    }
                    //
                    //--------------------------------------------------------------------------------
                    //   Display the form
                    //--------------------------------------------------------------------------------
                    //
                    if (!string.IsNullOrEmpty(ContentName)) {
                        Stream.Add("<br><br><B>" + ContentName + "</b><br>");
                    }
                    Stream.Add("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"99%\"><tr>");
                    Stream.Add("<td width=\"5%\">&nbsp;</td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>10%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>20%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>30%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>40%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>50%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>60%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>70%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>80%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>90%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>100%</nobr></td>");
                    Stream.Add("<td width=\"4%\" align=\"center\">&nbsp;</td>");
                    Stream.Add("</tr></TABLE>");
                    //
                    Stream.Add("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"99%\"><tr>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("</tr></TABLE>");
                    //
                    // print the column headers
                    //
                    ColumnWidthTotal = 0;
                    int InheritedFieldCount = 0;
                    if (CDef.adminColumns.Count > 0) {
                        //
                        // Calc total width
                        //
                        foreach (KeyValuePair<string, Processor.Models.Domain.ContentMetadataModel.MetaAdminColumnClass> kvp in CDef.adminColumns) {
                            ColumnWidthTotal += kvp.Value.Width;
                        }
                        //For ColumnCount = 0 To CDef.adminColumns.Count - 1
                        //    ColumnWidthTotal = ColumnWidthTotal + CDef.adminColumns(ColumnCount).Width
                        //Next
                        if (ColumnWidthTotal > 0) {
                            Stream.Add("<table border=\"0\" cellpadding=\"5\" cellspacing=\"0\" width=\"90%\">");
                            int ColumnCount = 0;
                            foreach (KeyValuePair<string, Processor.Models.Domain.ContentMetadataModel.MetaAdminColumnClass> kvp in CDef.adminColumns) {
                                //
                                // print column headers - anchored so they sort columns
                                //
                                int ColumnWidth = encodeInteger(100 * (kvp.Value.Width / (double)ColumnWidthTotal));
                                FieldName = kvp.Value.Name;
                                var tempVar = CDef.fields[FieldName.ToLowerInvariant()];
                                fieldId = tempVar.id;
                                string Caption = tempVar.caption;
                                if (tempVar.inherited) {
                                    Caption = Caption + "*";
                                    InheritedFieldCount = InheritedFieldCount + 1;
                                }
                                string AStart = "<A href=\"" + core.webServer.requestPage + "?" + RequestNameToolContentID + "=" + ContentID + "&af=" + AdminFormToolConfigureListing + "&fi=" + fieldId + "&dtcn=" + ColumnCount;
                                Stream.Add("<td width=\"" + ColumnWidth + "%\" valign=\"top\" align=\"left\">" + SpanClassAdminNormal + Caption + "<br>");
                                Stream.Add("<IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/black.GIF\" width=\"100%\" height=\"1\">");
                                Stream.Add(AStart + "&dta=" + ToolsActionRemoveField + "\"><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/LibButtonDeleteUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionMoveFieldRight + "\"><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/LibButtonMoveRightUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionMoveFieldLeft + "\"><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/LibButtonMoveLeftUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionSetAZ + "\"><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/LibButtonSortazUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionSetZA + "\"><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/LibButtonSortzaUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionExpand + "\"><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/LibButtonOpenUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionContract + "\"><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/LibButtonCloseUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A>");
                                Stream.Add("</SPAN></td>");
                                ColumnCount += 1;
                            }
                            Stream.Add("</tr>");
                            Stream.Add("</TABLE>");
                        }
                    }
                    //
                    // ----- If anything was inherited, put up the message
                    //
                    if (InheritedFieldCount > 0) {
                        Stream.Add("<P class=\"ccNormal\">* This field was inherited from the Content Definition's Parent. Inherited fields will automatically change when the field in the parent is changed. If you alter these settings, this connection will be broken, and the field will no longer inherit it's properties.</P class=\"ccNormal\">");
                    }
                    //
                    // ----- now output a list of fields to add
                    //
                    if (CDef.fields.Count == 0) {
                        Stream.Add(SpanClassAdminNormal + "This Content Definition has no fields</SPAN><br>");
                    } else {
                        Stream.Add(SpanClassAdminNormal + "<br>");
                        bool skipField = false;
                        foreach (KeyValuePair<string, Processor.Models.Domain.ContentFieldMetadataModel> keyValuePair in CDef.fields) {
                            Processor.Models.Domain.ContentFieldMetadataModel field = keyValuePair.Value;
                            //
                            // test if this column is in use
                            //
                            skipField = false;
                            //ColumnPointer = CDef.adminColumns.Count
                            if (CDef.adminColumns.Count > 0) {
                                foreach (KeyValuePair<string, Processor.Models.Domain.ContentMetadataModel.MetaAdminColumnClass> kvp in CDef.adminColumns) {
                                    if (field.nameLc == kvp.Value.Name) {
                                        skipField = true;
                                        break;
                                    }
                                }
                                //For ColumnPointer = 0 To CDef.adminColumns.Count - 1
                                //    If .nameLc = CDef.adminColumns(ColumnPointer).Name Then
                                //        skipField = True
                                //        Exit For
                                //    End If
                                //Next
                            }
                            //
                            // display the column if it is not in use
                            //
                            if (skipField) {
                                if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.FileText) {
                                    //
                                    // text filename can not be search
                                    //
                                    Stream.Add("<IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (text file field)<br>");
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.FileCSS) {
                                    //
                                    // text filename can not be search
                                    //
                                    Stream.Add("<IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (css file field)<br>");
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.FileXML) {
                                    //
                                    // text filename can not be search
                                    //
                                    Stream.Add("<IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (xml file field)<br>");
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.FileJavascript) {
                                    //
                                    // text filename can not be search
                                    //
                                    Stream.Add("<IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (javascript file field)<br>");
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.LongText) {
                                    //
                                    // long text can not be search
                                    //
                                    Stream.Add("<IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (long text field)<br>");
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.FileImage) {
                                    //
                                    // long text can not be search
                                    //
                                    Stream.Add("<IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (image field)<br>");
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.Redirect) {
                                    //
                                    // long text can not be search
                                    //
                                    Stream.Add("<IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (redirect field)<br>");
                                } else {
                                    //
                                    // can be used as column header
                                    //
                                    Stream.Add("<A href=\"" + core.webServer.requestPage + "?" + RequestNameToolContentID + "=" + ContentID + "&af=" + AdminFormToolConfigureListing + "&fi=" + field.id + "&dta=" + ToolsActionAddField + "&" + RequestNameAddFieldID + "=" + field.id + "\"><IMG src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/LibButtonAddUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A> " + field.caption + "<br>");
                                }
                            }
                        }
                    }
                }
                //
                //--------------------------------------------------------------------------------
                // print the content tables that have Listing Pages to Configure
                //--------------------------------------------------------------------------------
                //
                string FormPanel = SpanClassAdminNormal + "Select a Content Definition to Configure its Listing Page<br>";
                FormPanel = FormPanel + core.html.selectFromContent("ContentID", ContentID, "Content");
                Stream.Add(core.html.getPanel(FormPanel));
                core.siteProperties.setProperty("AllowContentAutoLoad", AllowContentAutoLoad);
                Stream.Add(HtmlController.inputHidden("ReloadCDef", ReloadCDef));
                result = AdminUIController.getToolForm(core, Stream.Text, ButtonList);
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }

        //
        //=============================================================================
        // Normalize the Index page Columns, setting proper values for IndexColumn, etc.
        //=============================================================================
        //
        private void NormalizeIndexColumns(int ContentID) {
            try {
                //
                int ColumnWidth = 0;
                int ColumnWidthTotal = 0;
                int ColumnCounter = 0;
                int IndexColumn = 0;
                //
                //Call LoadContentDefinitions
                //
                using (var csData = new CsModel(core)) {
                    csData.open("Content Fields", "(ContentID=" + ContentID + ")", "IndexColumn");
                    if (!csData.ok()) {
                        throw (new GenericException("Unexpected exception")); // Call handleLegacyClassErrors2("NormalizeIndexColumns", "Could not read Content Field Definitions")
                    } else {
                        //
                        // Adjust IndexSortOrder to be 0 based, count by 1
                        //
                        ColumnCounter = 0;
                        while (csData.ok()) {
                            IndexColumn = csData.getInteger("IndexColumn");
                            ColumnWidth = csData.getInteger("IndexWidth");
                            if ((IndexColumn == 0) || (ColumnWidth == 0)) {
                                csData.set( "IndexColumn", 0);
                                csData.set( "IndexWidth", 0);
                                csData.set( "IndexSortPriority", 0);
                            } else {
                                //
                                // Column appears in Index, clean it up
                                //
                                csData.set( "IndexColumn", ColumnCounter);
                                ColumnCounter = ColumnCounter + 1;
                                ColumnWidthTotal = ColumnWidthTotal + ColumnWidth;
                            }
                            csData.goNext();
                        }
                        if (ColumnCounter == 0) {
                            //
                            // No columns found, set name as Column 0, active as column 1
                            //
                            csData.goFirst();
                            while (csData.ok()) {
                                switch (GenericController.vbUCase(csData.getText( "name"))) {
                                    case "ACTIVE":
                                        csData.set( "IndexColumn", 0);
                                        csData.set( "IndexWidth", 20);
                                        ColumnWidthTotal = ColumnWidthTotal + 20;
                                        break;
                                    case "NAME":
                                        csData.set( "IndexColumn", 1);
                                        csData.set( "IndexWidth", 80);
                                        ColumnWidthTotal = ColumnWidthTotal + 80;
                                        break;
                                }
                                csData.goNext();
                            }
                        }
                        //
                        // ----- Now go back and set a normalized Width value
                        //
                        if (ColumnWidthTotal > 0) {
                            csData.goFirst();
                            while (csData.ok()) {
                                ColumnWidth = csData.getInteger("IndexWidth");
                                ColumnWidth = encodeInteger((ColumnWidth * 100) / (double)ColumnWidthTotal);
                                csData.set( "IndexWidth", ColumnWidth);
                                csData.goNext();
                            }
                        }
                    }
                }
                //
                // ----- now fixup Sort Priority so only visible fields are sorted.
                //
                using (var csData = new CsModel(core)) {
                    csData.open("Content Fields", "(ContentID=" + ContentID + ")", "IndexSortPriority, IndexColumn");
                    if (!csData.ok()) {
                        throw (new GenericException("Unexpected exception")); // Call handleLegacyClassErrors2("NormalizeIndexColumns", "Error reading Content Field Definitions")
                    } else {
                        //
                        // Go through all fields, clear Sort Priority if it does not appear
                        //
                        int SortValue = 0;
                        int SortDirection = 0;
                        SortValue = 0;
                        while (csData.ok()) {
                            SortDirection = 0;
                            if (csData.getInteger("IndexColumn") == 0) {
                                csData.set("IndexSortPriority", 0);
                            } else {
                                csData.set("IndexSortPriority", SortValue);
                                SortDirection = csData.getInteger("IndexSortDirection");
                                if (SortDirection == 0) {
                                    SortDirection = 1;
                                } else {
                                    if (SortDirection > 0) {
                                        SortDirection = 1;
                                    } else {
                                        SortDirection = -1;
                                    }
                                }
                                SortValue = SortValue + 1;
                            }
                            csData.set("IndexSortDirection", SortDirection);
                            csData.goNext();
                        }
                    }
                }
                return;
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
        }
        //
        //=============================================================================
        // Create a child content
        //=============================================================================
        //
        private string GetForm_CreateChildContent() {
            string result = "";
            try {
                int ParentContentID = 0;
                //string ParentContentName = null;
                string ChildContentName = "";
                bool AddAdminMenuEntry = false;
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                string ButtonList = ButtonCancel + "," + ButtonRun;
                //
                Stream.Add(AdminUIController.getToolFormTitle("Create a Child Content from a Content Definition", "This tool creates a Content Definition based on another Content Definition."));
                //
                //   print out the submit form
                if (core.docProperties.getText("Button") != "") {
                    //
                    // Process input
                    //
                    ParentContentID = core.docProperties.getInteger("ParentContentID");
                    var parentContentMetadata = ContentMetadataModel.create(core, ParentContentID);
                    //ParentContentName = Local_GetContentNameByID(ParentContentID);
                    ChildContentName = core.docProperties.getText("ChildContentName");
                    AddAdminMenuEntry = core.docProperties.getBoolean("AddAdminMenuEntry");
                    //
                    Stream.Add(SpanClassAdminSmall);
                    if ((parentContentMetadata == null) || (string.IsNullOrEmpty(ChildContentName))) {
                        Stream.Add("<p>You must select a parent and provide a child name.</p>");
                    } else {
                        //
                        // Create Definition
                        //
                        Stream.Add("<P>Creating content [" + ChildContentName + "] from [" + parentContentMetadata + "]");
                        var childContentMetadata = parentContentMetadata.createContentChild(core, ChildContentName, core.session.user.id);
                        //
                        Stream.Add("<br>Reloading Content Definitions...");
                        core.cache.invalidateAll();
                        core.clearMetaData();
                        //
                        // Add Admin Menu Entry
                        //
                        //If AddAdminMenuEntry Then
                        //    Stream.Add("<br>Adding menu entry (will not display until the next page)...")
                        //    csData.cs_open(Processor.Models.Db.NavigatorEntryModel.contentName, "ContentID=" & ParentContentID)
                        //    If csData.cs_ok(CS) Then
                        //        MenuName = csData.cs_getText(CS, "name")
                        //        AdminOnly = csData.cs_getBoolean(CS, "AdminOnly")
                        //        DeveloperOnly = csData.cs_getBoolean(CS, "DeveloperOnly")
                        //    End If
                        //    Call csData.cs_Close(CS)
                        //    If MenuName <> "" Then
                        //        Call Controllers.appBuilderController.admin_VerifyAdminMenu(core, MenuName, ChildContentName, ChildContentName, "", ChildContentName, AdminOnly, DeveloperOnly, False)
                        //    Else
                        //        Call Controllers.appBuilderController.admin_VerifyAdminMenu(core, "Site Content", ChildContentName, ChildContentName, "", "")
                        //    End If
                        //End If
                        Stream.Add("<br>Finished</P>");
                    }
                    Stream.Add("</SPAN>");
                }
                Stream.Add(SpanClassAdminNormal);
                //
                Stream.Add("Parent Content Name<br>");
                Stream.Add(core.html.selectFromContent("ParentContentID", ParentContentID, "Content", ""));
                Stream.Add("<br><br>");
                //
                Stream.Add("Child Content Name<br>");
                Stream.Add(HtmlController.inputText( core,"ChildContentName", ChildContentName, 1, 40));
                Stream.Add("<br><br>");
                //
                Stream.Add("Add Admin Menu Entry under Parent's Menu Entry<br>");
                Stream.Add(HtmlController.checkbox("AddAdminMenuEntry", AddAdminMenuEntry));
                Stream.Add("<br><br>");
                //
                //Stream.Add( core.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolCreateChildContent)
                Stream.Add("</SPAN>");
                //
                result = AdminUIController.getToolForm(core, Stream.Text, ButtonList);
                //result = adminUIController.getToolFormOpen(core, ButtonList) + Stream.Text + adminUIController.getToolFormClose(core, ButtonList);
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        // checks Content against Database tables
        //=============================================================================
        //
        private string GetForm_ClearContentWatchLinks() {
            string result = "";
            try {
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                string ButtonList;
                //
                ButtonList = ButtonCancel + ",Clear Content Watch Links";
                Stream.Add(SpanClassAdminNormal);
                Stream.Add(AdminUIController.getToolFormTitle("Clear ContentWatch Links", "This tools nulls the Links field of all Content Watch records. After running this tool, run the diagnostic spider to repopulate the links."));
                //
                if (core.docProperties.getText("Button") != "") {
                    //
                    // Process input
                    //
                    Stream.Add("<br>");
                    Stream.Add("<br>Clearing Content Watch Link field...");
                    core.db.executeQuery("update ccContentWatch set Link=null;");
                    Stream.Add("<br>Content Watch Link field cleared.");
                }
                Stream.Add("</span>");
                result = AdminUIController.getToolForm(core, Stream.Text, ButtonList);
                //result = adminUIController.getToolFormOpen(core, ButtonList) + Stream.Text + adminUIController.getToolFormClose(core, ButtonList);
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        /// <summary>
        /// Go through all Content Definitions and create appropriate tables and fields.
        /// </summary>
        /// <returns></returns>
        private string GetForm_SyncTables() {
            string returnValue = "";
            try {
                Processor.Models.Domain.ContentMetadataModel metadata = null;
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                string[,] ContentNameArray = null;
                int ContentNameCount = 0;
                string TableName = null;
                string ButtonList;
                //
                ButtonList = ButtonCancel + "," + ButtonRun;
                //
                Stream.Add(AdminUIController.getToolFormTitle("Synchronize Tables to Content Definitions", "This tools goes through all Content Definitions and creates any necessary Tables and Table Fields to support the Definition."));
                //
                if (core.docProperties.getText("Button") != "") {
                    //
                    //   Run Tools
                    //
                    Stream.Add("Synchronizing Tables to Content Definitions<br>");
                    using (var csData = new CsModel(core)) {
                        csData.open("Content", "", "", false, 0, "id");
                        if (csData.ok()) {
                            do {
                                metadata = Processor.Models.Domain.ContentMetadataModel.create(core, csData.getInteger("id"));
                                TableName = metadata.tableName;
                                Stream.Add("Synchronizing Content " + metadata.name + " to table " + TableName + "<br>");
                                using( var db = new DbController( core, metadata.dataSourceName )) {
                                    db.createSQLTable(TableName);
                                    if (metadata.fields.Count > 0) {
                                        foreach (var keyValuePair in metadata.fields) {
                                            ContentFieldMetadataModel field = keyValuePair.Value;
                                            Stream.Add("...Field " + field.nameLc + "<br>");
                                            db.createSQLTableField(TableName, field.nameLc, field.fieldTypeId);
                                        }
                                    }
                                }
                                csData.goNext();
                            } while (csData.ok());
                            ContentNameArray = csData.getRows();
                            ContentNameCount = ContentNameArray.GetUpperBound(1) + 1;
                        }
                    }
                }
                returnValue = AdminUIController.getToolForm(core, Stream.Text, ButtonList);
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return returnValue;
        }
        ////
        ////
        ////
        //private string AddDiagError(string ProblemMsg, DiagActionType[] DiagActions) {
        //    return GetDiagError(ProblemMsg, DiagActions);
        //}
        //
        //
        //
        //private string GetDiagError(string ProblemMsg, DiagActionType[] DiagActions) {
        //    string result = "";
        //    try {
        //        int ActionCount = 0;
        //        int ActionPointer = 0;
        //        string Caption = null;
        //        string Panel = "";
        //        //
        //        Panel = Panel + "<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" width=\"100%\">";
        //        Panel = Panel + "<tr><td colspan=\"2\">" + SpanClassAdminNormal + "<b>" + ProblemMsg + "</b></SPAN></td></tr>";
        //        ActionCount = DiagActions.GetUpperBound(0);
        //        if (ActionCount > 0) {
        //            for (ActionPointer = 0; ActionPointer <= ActionCount; ActionPointer++) {
        //                Caption = DiagActions[ActionPointer].Name;
        //                if (!string.IsNullOrEmpty(Caption)) {
        //                    Panel = Panel + "<tr>";
        //                    Panel = Panel + "<td width=\"30\" align=\"right\">";
        //                    Panel = Panel + core.html.inputRadio("DiagAction" + DiagActionCount, DiagActions[ActionPointer].Command, "");
        //                    Panel = Panel + "</td>";
        //                    Panel = Panel + "<td width=\"100%\">" + SpanClassAdminNormal + Caption + "</SPAN></td>";
        //                    Panel = Panel + "</tr>";
        //                }
        //            }
        //        }
        //        Panel = Panel + "</TABLE>";
        //        DiagActionCount = DiagActionCount + 1;
        //        result =  core.html.getPanel(Panel);
        //    } catch (Exception ex) {
        //        LogController.handleError( core,ex);
        //    }
        //    return result;
        //}
        //
        //
        //
        private string GetDiagHeader(string Copy) {
            return core.html.getPanel("<B>" + SpanClassAdminNormal + Copy + "</SPAN><B>");
        }
        //
        //
        //
        private string DiagArgument(string CommandList, int CommandPosition) {
            string tempDiagArgument = null;
            int CommandCount = 0;
            bool EndOfList = false;
            int CommandStartPosition = 0;
            int CommandEndPosition = 0;
            //
            tempDiagArgument = "";
            EndOfList = false;
            CommandStartPosition = 1;
            while ((CommandCount < CommandPosition) && (!EndOfList)) {
                CommandStartPosition = GenericController.vbInstr(CommandStartPosition, CommandList, ",");
                if (CommandStartPosition == 0) {
                    EndOfList = true;
                }
                CommandStartPosition = CommandStartPosition + 1;
                CommandCount = CommandCount + 1;
            }
            if (!EndOfList) {
                CommandEndPosition = GenericController.vbInstr(CommandStartPosition, CommandList, ",");
                if (CommandEndPosition == 0) {
                    tempDiagArgument = CommandList.Substring(CommandStartPosition - 1);
                } else {
                    tempDiagArgument = CommandList.Substring(CommandStartPosition - 1, CommandEndPosition - CommandStartPosition);
                }
            }
            return tempDiagArgument;
        }



        //
        //=============================================================================
        //=============================================================================
        //
        private string GetForm_Benchmark() {
            string result = "";
            try {
                int TestCount = 0;
                int TestPointer = 0;
                long TestTicks = 0;
                long OpenTicks = 0;
                long NextTicks = 0;
                long ReadTicks = 0;
                DataTable RS = null;
                string TestCopy = null;
                int PageSize = 0;
                int PageNumber = 0;
                int RecordCount = 0;
                string SQL = null;
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                string ButtonList;
                //
                ButtonList = ButtonCancel + "," + ButtonRun;
                //
                Stream.Add(AdminUIController.getToolFormTitle("Benchmark", "Run a series of data operations and compare the results to previous known values."));
                //
                if (core.docProperties.getText("Button") != "") {
                    //
                    //   Run Tools
                    //
                    Stream.Add("<br>");
                    //
                    Stream.Add(SpanClassAdminNormal);
                    TestTicks = core.doc.appStopWatch.ElapsedMilliseconds;
                    TestCount = 100;
                    PageSize = 50;
                    PageNumber = 1;
                    SQL = "SELECT * FROM ccSetup WHERE ACTIVE<>0;";
                    //
                    //Stream.Add("<br>")
                    //Stream.Add("Measure ContentServer communication time by getting contentfieldcount 1000 times<br>")
                    //Stream.Add(Now & " count=[" & TestCount & "]x, PageSize=[" & PageSize & "]<br>")
                    //For TestPointer = 1 To 1000
                    //    TestTicks = GetTickCount
                    //    TestCopy = genericController.encodeText(core.csGetFieldCount(1))
                    //    OpenTicks = OpenTicks + GetTickCount - TestTicks
                    //Next
                    //Stream.Add(Now & " Finished<br>")
                    Stream.Add("Time to make a ContentServer call = " + ((OpenTicks / 1000.0)).ToString("00.000") + " msec<br>");
                    //
                    // ExecuteSQL Test
                    //
                    Stream.Add("<br>");
                    Stream.Add("Recordset ExecuteSQL Test<br>");
                    Stream.Add(DateTime.Now + " count=[" + TestCount + "]x, SQL=[" + SQL + "], PageSize=[" + PageSize + "]<br>");
                    OpenTicks = 0;
                    ReadTicks = 0;
                    NextTicks = 0;
                    for (TestPointer = 1; TestPointer <= TestCount; TestPointer++) {
                        TestTicks = core.doc.appStopWatch.ElapsedMilliseconds;
                        RS = core.db.executeQuery(SQL);
                        OpenTicks = OpenTicks + core.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                        RecordCount = 0;
                        TestTicks = core.doc.appStopWatch.ElapsedMilliseconds;
                        foreach (DataRow dr in RS.Rows) {
                            NextTicks = NextTicks + core.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                            //
                            TestTicks = core.doc.appStopWatch.ElapsedMilliseconds;
                            TestCopy = GenericController.encodeText(dr["NAME"]);
                            ReadTicks = ReadTicks + core.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                            //
                            RecordCount = RecordCount + 1;
                            TestTicks = core.doc.appStopWatch.ElapsedMilliseconds;
                        }
                        NextTicks = NextTicks + core.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                    }
                    Stream.Add(DateTime.Now + " Finished<br>");
                    Stream.Add("Records = " + RecordCount + "<br>");
                    Stream.Add("Time to open recordset = " + ((OpenTicks) / (double)(TestCount)).ToString("00.000") + " msec<br>");
                    Stream.Add("Time to read field = " + ((ReadTicks) / (double)(TestCount * RecordCount)).ToString("00.000") + " msec<br>");
                    Stream.Add("Time to next record = " + ((NextTicks) / (double)(TestCount * RecordCount)).ToString("00.000") + " msec<br>");
                    //
                    // OpenCSContent Test
                    //
                    Stream.Add("<br>");
                    Stream.Add("Contentset OpenCSContent Test<br>");
                    Stream.Add(DateTime.Now + " count=[" + TestCount + "]x, PageSize=[" + PageSize + "]<br>");
                    OpenTicks = 0;
                    ReadTicks = 0;
                    NextTicks = 0;
                    for (TestPointer = 1; TestPointer <= TestCount; TestPointer++) {
                        //
                        TestTicks = core.doc.appStopWatch.ElapsedMilliseconds;
                        using (var csData = new CsModel(core)) {
                            csData.open("Site Properties", "", "", true, 0, "", PageSize, PageNumber);
                            OpenTicks = OpenTicks + core.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                            //
                            RecordCount = 0;
                            while (csData.ok()) {
                                //
                                TestTicks = core.doc.appStopWatch.ElapsedMilliseconds;
                                TestCopy = GenericController.encodeText(csData.getRawData("Name"));
                                ReadTicks = ReadTicks + core.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                                //
                                TestTicks = core.doc.appStopWatch.ElapsedMilliseconds;
                                csData.goNext();
                                NextTicks = NextTicks + core.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                                //
                                RecordCount = RecordCount + 1;
                            }
                        }
                        ReadTicks = ReadTicks + core.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                    }
                    Stream.Add(DateTime.Now + " Finished<br>");
                    Stream.Add("Records = " + RecordCount + "<br>");
                    Stream.Add("Time to open Contentset = " + ((OpenTicks) / (double)(TestCount)).ToString("00.000") + " msec<br>");
                    Stream.Add("Time to read field = " + ((ReadTicks) / (double)(TestCount * RecordCount)).ToString("00.000") + " msec<br>");
                    Stream.Add("Time to next record = " + ((NextTicks) / (double)(TestCount * RecordCount)).ToString("00.000") + " msec<br>");
                    //
                    // OpenCSContent SelectFieldList Test
                    //
                    Stream.Add("<br>");
                    Stream.Add("Contentset OpenCSContent Test with limited SelectFieldList<br>");
                    Stream.Add(DateTime.Now + " count=[" + TestCount + "]x, PageSize=[" + PageSize + "]<br>");
                    OpenTicks = 0;
                    ReadTicks = 0;
                    NextTicks = 0;
                    for (TestPointer = 1; TestPointer <= TestCount; TestPointer++) {
                        //
                        TestTicks = core.doc.appStopWatch.ElapsedMilliseconds;
                        using (var csData = new CsModel(core)) {
                            csData.open("Site Properties", "", "", true, 0, "name", PageSize, PageNumber);
                            OpenTicks = OpenTicks + core.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                            //
                            RecordCount = 0;
                            while (csData.ok()) {
                                //
                                TestTicks = core.doc.appStopWatch.ElapsedMilliseconds;
                                TestCopy = GenericController.encodeText(csData.getRawData("Name"));
                                ReadTicks = ReadTicks + core.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                                //
                                TestTicks = core.doc.appStopWatch.ElapsedMilliseconds;
                                csData.goNext();
                                NextTicks = NextTicks + core.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                                //
                                RecordCount = RecordCount + 1;
                            }
                        }
                        ReadTicks = ReadTicks + core.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                    }
                    Stream.Add(DateTime.Now + " Finished<br>");
                    Stream.Add("Records = " + RecordCount + "<br>");
                    Stream.Add("Time to open Contentset = " + ((OpenTicks) / (double)(TestCount)).ToString("00.000") + " msec<br>");
                    Stream.Add("Time to read field = " + ((ReadTicks) / (double)(TestCount * RecordCount)).ToString("00.000") + " msec<br>");
                    Stream.Add("Time to next record = " + ((NextTicks) / (double)(TestCount * RecordCount)).ToString("00.000") + " msec<br>");
                    //
                    Stream.Add("</SPAN>");
                }
                //
                // Print Start Button
                result = AdminUIController.getToolForm(core, Stream.Text, ButtonList);
                //result =  adminUIController.getToolFormOpen(core, ButtonList) + Stream.Text + adminUIController.getToolFormClose(core, ButtonList);
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   Get a ContentID from the ContentName using just the tables
        //=============================================================================
        //
        private int Local_GetContentID(string ContentName) {
            int tempLocal_GetContentID = 0;
            try {
                tempLocal_GetContentID = 0;
                DataTable dt = core.db.executeQuery("Select ID from ccContent where name=" + DbController.encodeSQLText(ContentName));
                if (dt.Rows.Count > 0) {
                    tempLocal_GetContentID = GenericController.encodeInteger(dt.Rows[0][0]);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return tempLocal_GetContentID;
        }
        //
        //=============================================================================
        //   Get a ContentID from the ContentName using just the tables
        //=============================================================================
        //
        private string Local_GetContentNameByID(int ContentID) {
            string tempLocal_GetContentNameByID = null;
            try {
                //
                DataTable dt = null;
                //
                tempLocal_GetContentNameByID = "";
                dt = core.db.executeQuery("Select name from ccContent where id=" + ContentID);
                if (dt.Rows.Count > 0) {
                    tempLocal_GetContentNameByID = GenericController.encodeText(dt.Rows[0][0]);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return tempLocal_GetContentNameByID;
        }
        //
        //=============================================================================
        //   Get a ContentID from the ContentName using just the tables
        //=============================================================================
        //
        private string Local_GetContentTableName(string ContentName) {
            string tempLocal_GetContentTableName = null;
            try {
                //
                DataTable RS = null;
                //
                tempLocal_GetContentTableName = "";
                RS = core.db.executeQuery("Select ccTables.Name as TableName from ccContent Left Join ccTables on ccContent.ContentTableID=ccTables.ID where ccContent.name=" + DbController.encodeSQLText(ContentName));
                if (RS.Rows.Count > 0) {
                    tempLocal_GetContentTableName = GenericController.encodeText(RS.Rows[0][0]);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return tempLocal_GetContentTableName;
        }
        //
        //=============================================================================
        //   Print the manual query form
        //=============================================================================
        //
        private string GetForm_DbSchema() {
            string result = "";
            try {
                bool StatusOK = false;
                int FieldCount = 0;
                object Retries = null;
                int RowMax = 0;
                int RowPointer = 0;
                int ColumnMax = 0;
                int ColumnPointer = 0;
                string ColumnStart = null;
                string ColumnEnd = null;
                string RowStart = null;
                string RowEnd = null;
                string[,] arrayOfSchema = null;
                string CellData = null;
                string TableName = "";
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                string ButtonList = null;
                DataTable RSSchema = null;
                var tmpList = new List<string> { };
                DataSourceModel datasource = DataSourceModel.create(core, core.docProperties.getInteger("DataSourceID"),ref tmpList);
                //
                ButtonList = ButtonCancel + "," + ButtonRun;
                //
                Stream.Add(AdminUIController.getToolFormTitle("Query Database Schema", "This tool examines the database schema for all tables available."));
                //
                StatusOK = true;
                if ((core.docProperties.getText("button")) != ButtonRun) {
                    //
                    // First pass, initialize
                    //
                    Retries = 0;
                } else {
                    //
                    // Read in arguments
                    //
                    TableName = core.docProperties.getText("TableName");
                    //
                    // Run the SQL
                    //
                    //ConnectionString = core.db.getmain_GetConnectionString(DataSourceName)
                    //
                    Stream.Add(SpanClassAdminSmall + "<br><br>");
                    Stream.Add(DateTime.Now + " Opening Table Schema on DataSource [" + datasource.name + "]<br>");
                    //
                    RSSchema = core.db.getTableSchemaData(TableName);
                    Stream.Add(DateTime.Now + " GetSchema executed successfully<br>");
                    if (!DbController.isDataTableOk(RSSchema)) {
                        //
                        // ----- no result
                        //
                        Stream.Add(DateTime.Now + " A schema was returned, but it contains no records.<br>");
                    } else {
                        //
                        // ----- print results
                        //
                        Stream.Add(DateTime.Now + " The following results were returned<br>");
                        //
                        // --- Create the Fields for the new table
                        //
                        FieldCount = RSSchema.Columns.Count;
                        Stream.Add("<table border=\"1\" cellpadding=\"1\" cellspacing=\"1\" width=\"100%\">");
                        Stream.Add("<tr>");
                        foreach (DataColumn RecordField in RSSchema.Columns) {
                            Stream.Add("<TD><B>" + SpanClassAdminSmall + RecordField.ColumnName + "</b></SPAN></td>");
                        }
                        Stream.Add("</tr>");
                        //
                        //Dim dtok As Boolean = False
                        arrayOfSchema = core.db.convertDataTabletoArray(RSSchema);
                        //
                        RowMax = arrayOfSchema.GetUpperBound(1);
                        ColumnMax = arrayOfSchema.GetUpperBound(0);
                        RowStart = "<tr>";
                        RowEnd = "</tr>";
                        ColumnStart = "<td class=\"ccadminsmall\">";
                        ColumnEnd = "</td>";
                        //ColumnStart = "<td class=""ccadminsmall"">&nbsp;"
                        //ColumnEnd = "&nbsp;</td>"
                        //ColumnStart = "<TD>" & SpanClassAdminSmall
                        //ColumnEnd = "</SPAN></td>"
                        for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                            Stream.Add(RowStart);
                            for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                CellData = arrayOfSchema[ColumnPointer, RowPointer];
                                if (IsNull(CellData)) {
                                    Stream.Add(ColumnStart + "[null]" + ColumnEnd);
                                } else if ((CellData == null)) {
                                    Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
                                } else if (string.IsNullOrEmpty(CellData)) {
                                    Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
                                } else {
                                    Stream.Add(ColumnStart + CellData + ColumnEnd);
                                }
                            }
                            Stream.Add(RowEnd);
                        }
                        Stream.Add("</TABLE>");
                        RSSchema.Dispose();
                        RSSchema = null;
                    }
                    //
                    // Index Schema
                    //
                    //    RSSchema = DataSourceConnectionObjs(DataSourcePointer).Conn.OpenSchema(SchemaEnum.adSchemaColumns, Array(Empty, Empty, TableName, Empty))
                    Stream.Add(SpanClassAdminSmall + "<br><br>");
                    Stream.Add(DateTime.Now + " Opening Index Schema<br>");
                    //
                    RSSchema = core.db.getIndexSchemaData(TableName);
                    if (!DbController.isDataTableOk(RSSchema)) {
                        //
                        // ----- no result
                        //
                        Stream.Add(DateTime.Now + " A schema was returned, but it contains no records.<br>");
                    } else {
                        //
                        // ----- print results
                        //
                        Stream.Add(DateTime.Now + " The following results were returned<br>");
                        //
                        // --- Create the Fields for the new table
                        //
                        FieldCount = RSSchema.Columns.Count;
                        Stream.Add("<table border=\"1\" cellpadding=\"1\" cellspacing=\"1\" width=\"100%\">");
                        Stream.Add("<tr>");
                        foreach (DataColumn RecordField in RSSchema.Columns) {
                            Stream.Add("<TD><B>" + SpanClassAdminSmall + RecordField.ColumnName + "</b></SPAN></td>");
                        }
                        Stream.Add("</tr>");
                        //

                        arrayOfSchema = core.db.convertDataTabletoArray(RSSchema);
                        //
                        RowMax = arrayOfSchema.GetUpperBound(1);
                        ColumnMax = arrayOfSchema.GetUpperBound(0);
                        RowStart = "<tr>";
                        RowEnd = "</tr>";
                        ColumnStart = "<td class=\"ccadminsmall\">";
                        ColumnEnd = "</td>";
                        //ColumnStart = "<td class=""ccadminsmall"">&nbsp;"
                        //ColumnEnd = "&nbsp;</td>"
                        //ColumnStart = "<TD>" & SpanClassAdminSmall
                        //ColumnEnd = "</SPAN></td>"
                        for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                            Stream.Add(RowStart);
                            for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                CellData = arrayOfSchema[ColumnPointer, RowPointer];
                                if (IsNull(CellData)) {
                                    Stream.Add(ColumnStart + "[null]" + ColumnEnd);
                                } else if ((CellData == null)) {
                                    Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
                                } else if (string.IsNullOrEmpty(CellData)) {
                                    Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
                                } else {
                                    Stream.Add(ColumnStart + CellData + ColumnEnd);
                                }
                            }
                            Stream.Add(RowEnd);
                        }
                        Stream.Add("</TABLE>");
                        RSSchema.Dispose();
                        RSSchema = null;
                    }
                    //
                    // Column Schema
                    //
                    Stream.Add(SpanClassAdminSmall + "<br><br>");
                    Stream.Add(DateTime.Now + " Opening Column Schema<br>");
                    //
                    RSSchema = core.db.getColumnSchemaData(TableName);
                    Stream.Add(DateTime.Now + " GetSchema executed successfully<br>");
                    if (DbController.isDataTableOk(RSSchema)) {
                        //
                        // ----- no result
                        //
                        Stream.Add(DateTime.Now + " A schema was returned, but it contains no records.<br>");
                    } else {
                        //
                        // ----- print results
                        //
                        Stream.Add(DateTime.Now + " The following results were returned<br>");
                        //
                        // --- Create the Fields for the new table
                        //
                        FieldCount = RSSchema.Columns.Count;
                        Stream.Add("<table border=\"1\" cellpadding=\"1\" cellspacing=\"1\" width=\"100%\">");
                        Stream.Add("<tr>");
                        foreach (DataColumn RecordField in RSSchema.Columns) {
                            Stream.Add("<TD><B>" + SpanClassAdminSmall + RecordField.ColumnName + "</b></SPAN></td>");
                        }
                        Stream.Add("</tr>");
                        //
                        arrayOfSchema = core.db.convertDataTabletoArray(RSSchema);
                        //
                        RowMax = arrayOfSchema.GetUpperBound(1);
                        ColumnMax = arrayOfSchema.GetUpperBound(0);
                        RowStart = "<tr>";
                        RowEnd = "</tr>";
                        ColumnStart = "<td class=\"ccadminsmall\">";
                        ColumnEnd = "</td>";
                        //ColumnStart = "<td class=""ccadminsmall"">&nbsp;"
                        //ColumnEnd = "&nbsp;</td>"
                        //ColumnStart = "<TD>" & SpanClassAdminSmall
                        //ColumnEnd = "</SPAN></td>"
                        for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                            Stream.Add(RowStart);
                            for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                CellData = arrayOfSchema[ColumnPointer, RowPointer];
                                if (IsNull(CellData)) {
                                    Stream.Add(ColumnStart + "[null]" + ColumnEnd);
                                } else if ((CellData == null)) {
                                    Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
                                } else if (string.IsNullOrEmpty(CellData)) {
                                    Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
                                } else {
                                    Stream.Add(ColumnStart + CellData + ColumnEnd);
                                }
                            }
                            Stream.Add(RowEnd);
                        }
                        Stream.Add("</TABLE>");
                        RSSchema.Dispose();
                        RSSchema = null;
                    }
                    if (!StatusOK) {
                        Stream.Add("There was a problem executing this query that may have prevented the results from printing.");
                    }
                    Stream.Add(DateTime.Now + " Done</SPAN>");
                }
                //
                // Display form
                //
                Stream.Add(SpanClassAdminNormal);
                //
                Stream.Add("<br>");
                Stream.Add("Table Name<br>");
                Stream.Add(HtmlController.inputText( core,"Tablename", TableName));
                //
                Stream.Add("<br><br>");
                Stream.Add("Data Source<br>");
                Stream.Add(core.html.selectFromContent("DataSourceID", datasource.id, "Data Sources", "", "Default"));
                //
                //Stream.Add( core.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolSchema)
                Stream.Add("</SPAN>");
                //
                result = AdminUIController.getToolForm(core, Stream.Text, ButtonList);
                //result = adminUIController.getToolFormOpen(core, ButtonList) + Stream.Text + adminUIController.getToolFormClose(core, ButtonList);
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   Print the manual query form
        //=============================================================================
        //
        private string GetForm_DbIndex() {
            string result = null;
            try {
                //
                int Count = 0;
                int Pointer = 0;
                int TableID = 0;
                string TableName = "";
                string FieldName = null;
                string IndexName = null;
                string DataSource = "";
                DataTable RSSchema = null;
                string Button = null;
                string[,] Rows = null;
                int RowMax = 0;
                int RowPointer = 0;
                string Copy = "";
                bool TableRowEven = false;
                int TableColSpan = 0;
                string ButtonList;
                //
                ButtonList = ButtonCancel + "," + ButtonSelect;
                result = AdminUIController.getToolFormTitle("Modify Database Indexes", "This tool adds and removes database indexes.");
                //
                // Process Input
                //
                Button = core.docProperties.getText("Button");
                TableID = core.docProperties.getInteger("TableID");
                //
                // Get Tablename and DataSource
                //
                using (var csData = new CsModel(core)) {
                    csData.openRecord("Tables", TableID, "Name,DataSourceID");
                    if (csData.ok()) {
                        TableName = csData.getText("name");
                        DataSource = csData.getText("DataSourceID");
                    }
                }
                using( var db = new DbController( core, DataSource )) {
                    //
                    if ((TableID != 0) && (TableID == core.docProperties.getInteger("previoustableid")) && (!string.IsNullOrEmpty(Button))) {
                        //
                        // Drop Indexes
                        //
                        Count = core.docProperties.getInteger("DropCount");
                        if (Count > 0) {
                            for (Pointer = 0; Pointer < Count; Pointer++) {
                                if (core.docProperties.getBoolean("DropIndex." + Pointer)) {
                                    IndexName = core.docProperties.getText("DropIndexName." + Pointer);
                                    result += "<br>Dropping index [" + IndexName + "] from table [" + TableName + "]";
                                    db.deleteSqlIndex(TableName, IndexName);
                                }
                            }
                        }
                        //
                        // Add Indexes
                        //
                        Count = core.docProperties.getInteger("AddCount");
                        if (Count > 0) {
                            for (Pointer = 0; Pointer < Count; Pointer++) {
                                if (core.docProperties.getBoolean("AddIndex." + Pointer)) {
                                    //IndexName = core.main_GetStreamText2("AddIndexFieldName." & Pointer)
                                    FieldName = core.docProperties.getText("AddIndexFieldName." + Pointer);
                                    IndexName = TableName + FieldName;
                                    result += "<br>Adding index [" + IndexName + "] to table [" + TableName + "] for field [" + FieldName + "]";
                                    db.createSQLIndex(TableName, IndexName, FieldName);
                                }
                            }
                        }
                    }
                    //
                    //result += htmlController.form_start(core);
                    TableColSpan = 3;
                    result += HtmlController.tableStart(2, 0, 0);
                    //
                    // Select Table Form
                    //
                    result += HtmlController.tableRow("<br><br><B>Select table to index</b>", TableColSpan, false);
                    result += HtmlController.tableRow(core.html.selectFromContent("TableID", TableID, "Tables", "", "Select a SQL table to start"), TableColSpan, false);
                    if (TableID != 0) {
                        //
                        // Add/Drop Indexes form
                        //
                        result += HtmlController.inputHidden("PreviousTableID", TableID);
                        //
                        // Drop Indexes
                        //
                        result += HtmlController.tableRow("<br><br><B>Select indexes to remove</b>", TableColSpan, TableRowEven);
                        RSSchema = db.getIndexSchemaData(TableName);


                        if (RSSchema.Rows.Count == 0) {
                            //
                            // ----- no result
                            //
                            Copy += DateTime.Now + " A schema was returned, but it contains no indexs.";
                            result += HtmlController.tableRow(Copy, TableColSpan, TableRowEven);
                        } else {

                            Rows = db.convertDataTabletoArray(RSSchema);
                            RowMax = Rows.GetUpperBound(1);
                            for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                                IndexName = GenericController.encodeText(Rows[5, RowPointer]);
                                if (!string.IsNullOrEmpty(IndexName)) {
                                    result += HtmlController.tableRowStart();
                                    Copy = HtmlController.checkbox("DropIndex." + RowPointer, false) + HtmlController.inputHidden("DropIndexName." + RowPointer, IndexName) + GenericController.encodeText(IndexName);
                                    result += HtmlController.td(Copy, "", 0, TableRowEven);
                                    result += HtmlController.td(GenericController.encodeText(Rows[17, RowPointer]), "", 0, TableRowEven);
                                    result += HtmlController.td("&nbsp;", "", 0, TableRowEven);
                                    result += kmaEndTableRow;
                                    TableRowEven = !TableRowEven;
                                }
                            }
                            result += HtmlController.inputHidden("DropCount", RowMax + 1);
                        }
                        //
                        // Add Indexes
                        //
                        TableRowEven = false;
                        result += HtmlController.tableRow("<br><br><B>Select database fields to index</b>", TableColSpan, TableRowEven);
                        RSSchema = db.getColumnSchemaData(TableName);
                        if (RSSchema.Rows.Count == 0) {
                            //
                            // ----- no result
                            //
                            Copy += DateTime.Now + " A schema was returned, but it contains no indexs.";
                            result += HtmlController.tableRow(Copy, TableColSpan, TableRowEven);
                        } else {

                            Rows = db.convertDataTabletoArray(RSSchema);
                            //
                            RowMax = Rows.GetUpperBound(1);
                            for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                                result += HtmlController.tableRowStart();
                                Copy = HtmlController.checkbox("AddIndex." + RowPointer, false) + HtmlController.inputHidden("AddIndexFieldName." + RowPointer, Rows[3, RowPointer]) + GenericController.encodeText(Rows[3, RowPointer]);
                                result += HtmlController.td(Copy, "", 0, TableRowEven);
                                result += HtmlController.td("&nbsp;", "", 0, TableRowEven);
                                result += HtmlController.td("&nbsp;", "", 0, TableRowEven);
                                result += kmaEndTableRow;
                                TableRowEven = !TableRowEven;
                            }
                            result += HtmlController.inputHidden("AddCount", RowMax + 1);
                        }
                        //
                        // Spacers
                        //
                        result += HtmlController.tableRowStart();
                        result += HtmlController.td(nop2(300, 1), "200");
                        result += HtmlController.td(nop2(200, 1), "200");
                        result += HtmlController.td("&nbsp;", "100%");
                        result += kmaEndTableRow;
                    }
                    result += kmaEndTable;
                    //
                    // Buttons
                    //
                    result = AdminUIController.getToolForm(core, result, ButtonList);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   Print the manual query form
        //=============================================================================
        //
        private string GetForm_ContentDbSchema() {
            string result = null;
            try {
                //
                int TableColSpan = 0;
                bool TableEvenRow = false;
                string SQL = null;
                string TableName = null;
                string ButtonList;
                //
                ButtonList = ButtonCancel;
                result = AdminUIController.getToolFormTitle("Get Content Database Schema", "This tool displays all tables and fields required for the current Content Defintions.");
                //
                TableColSpan = 3;
                result += HtmlController.tableStart(2, 0, 0);
                SQL = "SELECT DISTINCT ccTables.Name as TableName, ccFields.Name as FieldName, ccFieldTypes.Name as FieldType"
                        + " FROM ((ccContent LEFT JOIN ccTables ON ccContent.ContentTableID = ccTables.ID) LEFT JOIN ccFields ON ccContent.ID = ccFields.ContentID) LEFT JOIN ccFieldTypes ON ccFields.Type = ccFieldTypes.ID"
                        + " ORDER BY ccTables.Name, ccFields.Name;";
                using (var csData = new CsModel(core)) {
                    csData.openSql(SQL, "Default");
                    TableName = "";
                    while (csData.ok()) {
                        if (TableName != csData.getText( "TableName")) {
                            TableName = csData.getText( "TableName");
                            result += HtmlController.tableRow("<B>" + TableName + "</b>", TableColSpan, TableEvenRow);
                        }
                        result += HtmlController.tableRowStart();
                        result += HtmlController.td("&nbsp;", "", 0, TableEvenRow);
                        result += HtmlController.td(csData.getText( "FieldName"), "", 0, TableEvenRow);
                        result += HtmlController.td(csData.getText( "FieldType"), "", 0, TableEvenRow);
                        result += kmaEndTableRow;
                        TableEvenRow = !TableEvenRow;
                        csData.goNext();
                    }
                }
                //
                // Field Type Definitions
                //
                result += HtmlController.tableRow("<br><br><B>Field Type Definitions</b>", TableColSpan, TableEvenRow);
                result += HtmlController.tableRow("Boolean - Boolean values 0 and 1 are stored in a database long integer field type", TableColSpan, TableEvenRow);
                result += HtmlController.tableRow("Lookup - References to related records stored as database long integer field type", TableColSpan, TableEvenRow);
                result += HtmlController.tableRow("Integer - database long integer field type", TableColSpan, TableEvenRow);
                result += HtmlController.tableRow("Float - database floating point value", TableColSpan, TableEvenRow);
                result += HtmlController.tableRow("Date - database DateTime field type.", TableColSpan, TableEvenRow);
                result += HtmlController.tableRow("AutoIncrement - database long integer field type. Field automatically increments when a record is added.", TableColSpan, TableEvenRow);
                result += HtmlController.tableRow("Text - database character field up to 255 characters.", TableColSpan, TableEvenRow);
                result += HtmlController.tableRow("LongText - database character field up to 64K characters.", TableColSpan, TableEvenRow);
                result += HtmlController.tableRow("TextFile - references a filename in the Content Files folder. Database character field up to 255 characters. ", TableColSpan, TableEvenRow);
                result += HtmlController.tableRow("File - references a filename in the Content Files folder. Database character field up to 255 characters. ", TableColSpan, TableEvenRow);
                result += HtmlController.tableRow("Redirect - This field has no database equivelent. No Database field is required.", TableColSpan, TableEvenRow);
                //
                // Spacers
                //
                result += HtmlController.tableRowStart();
                result += HtmlController.td(nop2(20, 1), "20");
                result += HtmlController.td(nop2(300, 1), "300");
                result += HtmlController.td("&nbsp;", "100%");
                result += kmaEndTableRow;
                result += kmaEndTable;
                //
                //GetForm_ContentDbSchema = GetForm_ContentDbSchema & core.main_GetFormInputHidden("af", AdminFormToolContentDbSchema)
                result = AdminUIController.getToolForm(core, result, ButtonList);
                //result =  (adminUIController.getToolFormOpen(core, ButtonList)) + result + (adminUIController.getToolFormClose(core, ButtonList));
                //
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   Print the manual query form
        //=============================================================================
        //
        private string GetForm_LogFiles() {
            string tempGetForm_LogFiles = null;
            try {
                string ButtonList = ButtonCancel;
                tempGetForm_LogFiles = AdminUIController.getToolFormTitle("Log File View", "This tool displays the Contensive Log Files.");
                tempGetForm_LogFiles = tempGetForm_LogFiles + "<P></P>";
                //
                string QueryOld = ".asp?";
                string QueryNew = GenericController.modifyQueryString(QueryOld, RequestNameAdminForm, AdminFormToolLogFileView, true);
                tempGetForm_LogFiles = tempGetForm_LogFiles + GenericController.vbReplace(GetForm_LogFiles_Details(), QueryOld, QueryNew + "&", 1, 99, 1);
                //
                tempGetForm_LogFiles = AdminUIController.getToolForm(core, tempGetForm_LogFiles, ButtonList);
                //tempGetForm_LogFiles = adminUIController.getToolFormOpen(core, ButtonList) + tempGetForm_LogFiles + (adminUIController.getToolFormClose(core, ButtonList));
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return tempGetForm_LogFiles;
        }
        //
        //==============================================================================================
        //   Display a path in the Content Files with links to download and change folders
        //==============================================================================================
        //
        private string GetForm_LogFiles_Details() {
            string result = "";
            try {

                string StartPath = null;
                string CurrentPath = null;
                string SourceFolders = null;
                string[] FolderSplit = null;
                int FolderCount = 0;
                int FolderPointer = 0;
                string[] LineSplit = null;
                string FolderLine = null;
                string FolderName = null;
                string ParentPath = null;
                int Position = 0;
                string Filename = null;
                bool RowEven = false;
                string FileSize = null;
                string FileDate = null;
                string FileURL = null;
                string CellCopy = null;
                string QueryString = null;
                //
                const string GetTableStart = "<table border=\"1\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\"><tr><TD><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\"><tr>"
                    + "<td width=\"23\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/spacer.gif\" height=\"1\" width=\"23\"></td>"
                    + "<td width=\"60%\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/spacer.gif\" height=\"1\" width=\"1\"></td>"
                    + "<td width=\"20%\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/spacer.gif\" height=\"1\" width=\"1\"></td>"
                    + "<td width=\"20%\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/images/spacer.gif\" height=\"1\" width=\"1\"></td>"
                    + "</tr>";
                const string GetTableEnd = "</table></td></tr></table>";
                //
                const string SpacerImage = "<img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/Images/spacer.gif\" width=\"23\" height=\"22\" border=\"0\">";
                const string FolderOpenImage = "<img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/Images/iconfolderopen.gif\" width=\"23\" height=\"22\" border=\"0\">";
                const string FolderClosedImage = "<img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/Images/iconfolderclosed.gif\" width=\"23\" height=\"22\" border=\"0\">";
                //
                // StartPath is the root - the top of the directory, it ends in the folder name (no slash)
                //
                result = "";
                StartPath = core.programDataFiles.localAbsRootPath + "Logs\\";
                //
                // CurrentPath is what is concatinated on to StartPath to get the current folder, it must start with a slash
                //
                CurrentPath = core.docProperties.getText("SetPath");
                if (string.IsNullOrEmpty(CurrentPath)) {
                    CurrentPath = "\\";
                } else if (CurrentPath.Left( 1) != "\\") {
                    CurrentPath = "\\" + CurrentPath;
                }
                //
                // Parent Folder is the path to the parent of current folder, and must start with a slash
                //
                Position = CurrentPath.LastIndexOf("\\") + 1;
                if (Position == 1) {
                    ParentPath = "\\";
                } else {
                    ParentPath = CurrentPath.Left( Position - 1);
                }
                //
                //
                if (core.docProperties.getText("SourceFile") != "") {
                    //
                    // Return the content of the file
                    //
                    core.webServer.setResponseContentType("text/text");
                    result = core.wwwFiles.readFileText(core.docProperties.getText("SourceFile"));
                    core.doc.continueProcessing = false;
                } else {
                    result += GetTableStart;
                    //
                    // Parent Folder Link
                    //
                    if (CurrentPath != ParentPath) {
                        FileSize = "";
                        FileDate = "";
                        result += getForm_LogFiles_Details_GetRow("<A href=\"" + core.webServer.requestPage + "?SetPath=" + ParentPath + "\">" + FolderOpenImage + "</A>", "<A href=\"" + core.webServer.requestPage + "?SetPath=" + ParentPath + "\">" + ParentPath + "</A>", FileSize, FileDate, RowEven);
                    }
                    //
                    // Sub-Folders
                    //

                    SourceFolders = core.wwwFiles.getFolderNameList(StartPath + CurrentPath);
                    if (!string.IsNullOrEmpty(SourceFolders)) {
                        FolderSplit = SourceFolders.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        FolderCount = FolderSplit.GetUpperBound(0) + 1;
                        for (FolderPointer = 0; FolderPointer < FolderCount; FolderPointer++) {
                            FolderLine = FolderSplit[FolderPointer];
                            if (!string.IsNullOrEmpty(FolderLine)) {
                                LineSplit = FolderLine.Split(',');
                                FolderName = LineSplit[0];
                                FileSize = LineSplit[1];
                                FileDate = LineSplit[2];
                                result += getForm_LogFiles_Details_GetRow("<A href=\"" + core.webServer.requestPage + "?SetPath=" + CurrentPath + "\\" + FolderName + "\">" + FolderClosedImage + "</A>", "<A href=\"" + core.webServer.requestPage + "?SetPath=" + CurrentPath + "\\" + FolderName + "\">" + FolderName + "</A>", FileSize, FileDate, RowEven);
                            }
                        }
                    }
                    //
                    // Files
                    //
                    SourceFolders = UpgradeController.Upgrade51ConvertFileInfoArrayToParseString(core.wwwFiles.getFileList(StartPath + CurrentPath));
                    if (string.IsNullOrEmpty(SourceFolders)) {
                        FileSize = "";
                        FileDate = "";
                        result += getForm_LogFiles_Details_GetRow(SpacerImage, "no files were found in this folder", FileSize, FileDate, RowEven);
                    } else {
                        FolderSplit = SourceFolders.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        FolderCount = FolderSplit.GetUpperBound(0) + 1;
                        for (FolderPointer = 0; FolderPointer < FolderCount; FolderPointer++) {
                            FolderLine = FolderSplit[FolderPointer];
                            if (!string.IsNullOrEmpty(FolderLine)) {
                                LineSplit = FolderLine.Split(',');
                                Filename = LineSplit[0];
                                FileSize = LineSplit[5];
                                FileDate = LineSplit[3];
                                FileURL = StartPath + CurrentPath + "\\" + Filename;
                                QueryString = core.doc.refreshQueryString;
                                QueryString = GenericController.modifyQueryString(QueryString, RequestNameAdminForm, encodeText(AdminFormTool), true);
                                QueryString = GenericController.modifyQueryString(QueryString, "at", AdminFormToolLogFileView, true);
                                QueryString = GenericController.modifyQueryString(QueryString, "SourceFile", FileURL, true);
                                CellCopy = "<A href=\"" + core.webServer.requestPath + "?" + QueryString + "\" target=\"_blank\">" + Filename + "</A>";
                                result += getForm_LogFiles_Details_GetRow(SpacerImage, CellCopy, FileSize, FileDate, RowEven);
                            }
                        }
                    }
                    //
                    result += GetTableEnd;
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   Table Rows
        //=============================================================================
        //
        public string getForm_LogFiles_Details_GetRow(string Cell0, string Cell1, string Cell2, string Cell3, bool RowEven) {
            string tempGetForm_LogFiles_Details_GetRow = null;
            //
            string ClassString = null;
            //
            if (GenericController.encodeBoolean(RowEven)) {
                RowEven = false;
                ClassString = " class=\"ccPanelRowEven\" ";
            } else {
                RowEven = true;
                ClassString = " class=\"ccPanelRowOdd\" ";
            }
            //
            Cell0 = GenericController.encodeText(Cell0);
            if (string.IsNullOrEmpty(Cell0)) {
                Cell0 = "&nbsp;";
            }
            //
            Cell1 = GenericController.encodeText(Cell1);
            //
            if (string.IsNullOrEmpty(Cell1)) {
                tempGetForm_LogFiles_Details_GetRow = "<tr><TD" + ClassString + " Colspan=\"4\">" + Cell0 + "</td></tr>";
            } else {
                tempGetForm_LogFiles_Details_GetRow = "<tr><TD" + ClassString + ">" + Cell0 + "</td><TD" + ClassString + ">" + Cell1 + "</td><td align=right " + ClassString + ">" + Cell2 + "</td><td align=right " + ClassString + ">" + Cell3 + "</td></tr>";
            }
            //
            return tempGetForm_LogFiles_Details_GetRow;
        }
        //
        //=============================================================================
        //   Print the manual query form
        //=============================================================================
        //
        private string GetForm_LoadCDef() {
            string result = "";
            try {
                //
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                string ButtonList;
                //
                ButtonList = ButtonCancel + "," + ButtonSaveandInvalidateCache;
                Stream.Add(AdminUIController.getToolFormTitle("Load Content Definitions", "This tool reloads the content definitions. This is necessary when changes are made to the ccContent or ccFields tables outside Contensive. The site will be blocked during the load."));
                //
                if ((core.docProperties.getText("button")) != ButtonSaveandInvalidateCache) {
                    //
                    // First pass, initialize
                    //
                } else {
                    //
                    // Restart
                    //
                    Stream.Add("<br>Loading Content Definitions...");
                    core.cache.invalidateAll();
                    core.clearMetaData();
                    Stream.Add("<br>Content Definitions loaded");
                }
                //
                // Display form
                //
                Stream.Add(SpanClassAdminNormal);
                Stream.Add("<br>");
                Stream.Add("</span>");
                //
                result = AdminUIController.getToolForm(core, Stream.Text, ButtonList);
                //result = adminUIController.getToolFormOpen(core, ButtonList) + Stream.Text + adminUIController.getToolFormClose(core, ButtonList);
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return result;
        }
        //
        //
        //
        private Processor.Models.Domain.ContentMetadataModel GetCDef(string ContentName) {
            return Processor.Models.Domain.ContentMetadataModel.createByUniqueName(core, ContentName);
        }
        //
        //=============================================================================
        //   Import the htm and html files in the FileRootPath and below into Page Templates
        //       FileRootPath is the path to the root of the site
        //       AppPath is the path to the folder currently
        //=============================================================================
        //
        private string GetForm_LoadTemplates() {
            string result = "";
            try {
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                string ButtonList = null;
                bool AllowImageImport = false;
                bool AllowStyleImport = false;
                //
                bool AllowBodyHTML = core.docProperties.getBoolean("AllowBodyHTML");
                bool AllowScriptLink = core.docProperties.getBoolean("AllowScriptLink");
                AllowImageImport = core.docProperties.getBoolean("AllowImageImport");
                AllowStyleImport = core.docProperties.getBoolean("AllowStyleImport");
                //
                Stream.Add(AdminUIController.getToolFormTitle("Load Templates", "This tool creates template records from the HTML files in the root folder of the site."));
                //
                if ((core.docProperties.getText("button")) != ButtonImportTemplates) {
                    //
                    // First pass, initialize
                    //
                } else {
                    //
                    // Restart
                    //
                    Stream.Add("<br>Loading Templates...");
                    //Call Stream.Add(ImportTemplates(core.appRootFiles.rootLocalPath, "", AllowBodyHTML, AllowScriptLink, AllowImageImport, AllowStyleImport))
                    Stream.Add("<br>Templates loaded");
                }
                //
                // Display form
                //
                Stream.Add(SpanClassAdminNormal);
                Stream.Add("<br>");
                //Stream.Add( core.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolLoadTemplates)
                Stream.Add("<br>" + HtmlController.checkbox("AllowBodyHTML", AllowBodyHTML) + " Update/Import Soft Templates from the Body of .HTM and .HTML files");
                Stream.Add("<br>" + HtmlController.checkbox("AllowScriptLink", AllowScriptLink) + " Update/Import Hard Templates with links to .ASP and .ASPX scripts");
                Stream.Add("<br>" + HtmlController.checkbox("AllowImageImport", AllowImageImport) + " Update/Import image links (.GIF,.JPG,.PDF ) into the resource library");
                Stream.Add("<br>" + HtmlController.checkbox("AllowStyleImport", AllowStyleImport) + " Import style sheets (.CSS) to Dynamic Styles");
                Stream.Add("</SPAN>");
                //
                ButtonList = ButtonCancel + "," + ButtonImportTemplates;
                result = AdminUIController.getToolForm(core, Stream.Text, ButtonList);
                //result = adminUIController.getToolFormOpen(core, ButtonList) + Stream.Text + adminUIController.getToolFormClose(core, ButtonList);
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   Import the htm and html files in the FileRootPath and below into Page Templates
        //       FileRootPath is the path to the root of the site
        //       AppPath is the path to the folder currently
        //=============================================================================
        //
        //Private Function ImportTemplates(ByVal FileRootPath As String, ByVal AppPath As String, ByVal AllowBodyHTML As Boolean, ByVal AllowScriptLink As Boolean, ByVal AllowImageImport As Boolean, ByVal AllowStyleImport As Boolean) As String
        //    Dim result As String = ""
        //    Try
        //        '
        //        Dim Stream As New stringBuilderLegacyController
        //        Dim Folders() As String
        //        Dim FolderList As String
        //        Dim FolderDetailString As String
        //        Dim FolderDetails() As String
        //        Dim FolderName As String
        //        Dim FileList As String
        //        Dim Files() As String
        //        Dim Ptr As Integer
        //        Dim FileDetailString As String
        //        Dim FileDetails() As String
        //        Dim Filename As String
        //        Dim PageSource As String
        //        Dim CS As Integer
        //        Dim Link As String
        //        Dim TemplateName As String
        //        Dim Copy As String
        //        '
        //        FileList = core.appRootFiles.convertFileINfoArrayToParseString(core.appRootFiles.getFileList(FileRootPath & AppPath))
        //        Files = Split(FileList, vbCrLf)
        //        For Ptr = 0 To UBound(Files)
        //            FileDetailString = Files[Ptr]
        //            FileDetails = Split(FileDetailString, ",")
        //            Filename = FileDetails(0)
        //            Link = genericController.vbReplace(AppPath & Filename, "\", "/")
        //            If AllowScriptLink And (InStr(1, Filename, ".asp", vbTextCompare) <> 0) And (Mid(Filename, 1, 1) <> "_") Then
        //                '
        //                result = result & "<br>Create Hard Template for script page [" & Link & "]"
        //                '
        //                Link = genericController.vbReplace(AppPath & Filename, "\", "/")
        //                TemplateName = genericController.vbReplace(Link, "/", "-")
        //                If genericController.vbInstr(1, TemplateName, ".") <> 0 Then
        //                    TemplateName = Mid(TemplateName, 1, genericController.vbInstr(1, TemplateName, ".") - 1)

        //                End If
        //                '
        //                csData.cs_open("Page Templates", "Link=" & DbController.encodeSQLText(Link))
        //                If Not csData.cs_ok(CS) Then
        //                    Call csData.cs_Close(CS)
        //                    csData.cs_insertRecord("Page Templates")
        //                    Call csData.cs_set(CS, "Link", Link)
        //                End If
        //                If csData.cs_ok(CS) Then
        //                    Call csData.cs_set(CS, "name", TemplateName)
        //                End If
        //                Call csData.cs_Close(CS)
        //            ElseIf AllowBodyHTML And (InStr(1, Filename, ".htm", vbTextCompare) <> 0) And (Mid(Filename, 1, 1) <> "_") Then
        //                '
        //                ' HTML, import body
        //                '
        //                PageSource = core.appRootFiles.readFile(Filename)
        //                Call core.main_EncodePage_SplitBody(PageSource, PageSource, "", "")
        //                Link = genericController.vbReplace(AppPath & Filename, "\", "/")
        //                TemplateName = genericController.vbReplace(Link, "/", "-")
        //                If genericController.vbInstr(1, TemplateName, ".") <> 0 Then
        //                    TemplateName = Mid(TemplateName, 1, genericController.vbInstr(1, TemplateName, ".") - 1)

        //                End If
        //                '
        //                result = result & "<br>Create Soft Template from source [" & Link & "]"
        //                '
        //                csData.cs_open("Page Templates", "Source=" & DbController.encodeSQLText(Link))
        //                If Not csData.cs_ok(CS) Then
        //                    Call csData.cs_Close(CS)
        //                    csData.cs_insertRecord("Page Templates")
        //                    Call csData.cs_set(CS, "Source", Link)
        //                    Call csData.cs_set(CS, "name", TemplateName)
        //                End If
        //                If csData.cs_ok(CS) Then
        //                    Call csData.cs_set(CS, "Link", "")
        //                    Call csData.cs_set(CS, "bodyhtml", PageSource)
        //                End If
        //                Call csData.cs_Close(CS)
        //                '
        //            ElseIf AllowImageImport And (InStr(1, Filename, ".gif", vbTextCompare) <> 0) And (Mid(Filename, 1, 1) <> "_") Then
        //                '
        //                ' Import GIF images
        //                '
        //                result = result & "<br>Import Image Link to Resource Library [" & Link & "]"
        //                '
        //            ElseIf AllowStyleImport And (InStr(1, Filename, ".css", vbTextCompare) <> 0) And (Mid(Filename, 1, 1) <> "_") Then
        //                '
        //                ' Import CSS to Dynamic styles
        //                '
        //                result = result & "<br>Import style sheet to Dynamic Styles [" & Link & "]"
        //                '
        //                Dim DynamicFilename As String
        //                DynamicFilename = "templates\styles.css"
        //                Copy = core.appRootFiles.readFile(DynamicFilename)
        //                Copy = RemoveStyleTags(Copy)
        //                Copy = Copy _
        //                    & vbCrLf _
        //                    & vbCrLf & "/* Import of " & FileRootPath & AppPath & Filename & "*/" _
        //                    & vbCrLf _
        //                    & vbCrLf
        //                Copy = Copy & RemoveStyleTags(core.appRootFiles.readFile(Filename))
        //                Call core.appRootFiles.saveFile(DynamicFilename, Copy)
        //            End If
        //        Next
        //        '
        //        ' Now process all subfolders
        //        '
        //        FolderList = core.getFolderNameList(FileRootPath & AppPath)
        //        If FolderList <> "" Then
        //            Folders = Split(FolderList, vbCrLf)
        //            For Ptr = 0 To UBound(Folders)
        //                FolderDetailString = Folders[Ptr]
        //                If FolderDetailString <> "" Then
        //                    FolderDetails = Split(FolderDetailString, ",")
        //                    FolderName = FolderDetails(0)
        //                    If Mid(FolderName, 1, 1) <> "_" Then
        //                        result = result & ImportTemplates(FileRootPath, AppPath & FolderName & "\", AllowBodyHTML, AllowScriptLink, AllowImageImport, AllowStyleImport)
        //                    End If
        //                End If
        //            Next
        //        End If
        //    Catch ex As Exception
        //        core.handleExceptionAndContinue(ex) : Throw
        //    End Try
        //    Return result
        //End Function
        //        '
        //        '
        //        '
        //        Private Function LoadCDef(ByVal ContentName As String) As coreMetaDataClass.CDefClass
        //            On Error GoTo ErrorTrap
        //            '
        //            Dim SQL As String
        //            Dim CS As Integer
        //            Dim ContentID As Integer
        //            Dim CSContent As Integer
        //            Dim ParentContentName As String
        //            Dim ParentID As Integer
        //            '
        //            CSContent = csData.cs_open("Content", "name=" & DbController.encodeSQLText(ContentName))
        //            If csData.cs_ok(CSContent) Then
        //                '
        //                ' Start with parent CDef
        //                '
        //                ParentID = csData.cs_getInteger(CSContent, "parentID")
        //                If ParentID <> 0 Then
        //                    ParentContentName = Models.Complex.CdefController.getContentNameByID(core,ParentID)
        //                    If ParentContentName <> "" Then
        //                        LoadCDef = LoadCDef(ParentContentName)
        //                    End If
        //                End If
        //                '
        //                ' Add this definition on it
        //                '
        //                With LoadCDef
        //                    csData.cs_open("Content Fields", "contentid=" & ContentID)
        //                    Do While csData.cs_ok(CS)
        //                        Select Case genericController.vbUCase(csData.cs_getText(CS, "name"))
        //                            Case "NAME"
        //                                .Name = ""
        //                        End Select
        //                        Call csData.cs_goNext(CS)
        //                    Loop
        //                    Call csData.cs_Close(CS)
        //                End With
        //            End If
        //            Call csData.cs_Close(CSContent)
        //            throw (new GenericException("Unexpected exception"))'  Call handleLegacyClassErrors1("ImportTemplates", "ErrorTrap")
        //        End Function
        //        '
        //        '=================================================================================
        //        '
        //        ' From Admin code in ccWeb42
        //        '   Put it here so the same data will be used for both the admin site and the tool page
        //        '   change this so it reads from the CDef, not the database
        //        '
        //        '
        //        '=================================================================================
        //        '
        //        Public Sub GetDbCDef_SetAdminColumns(ByRef CDef As appServices_metaDataClass.CDefClass)
        //            On Error GoTo ErrorTrap
        //            '
        //            Dim DestPtr As Integer
        //            Dim UcaseFieldName As String
        //            Dim DefaultFieldPointer As Integer
        //            ' converted array to dictionary - Dim FieldPointer As Integer
        //            Dim FieldActive As Boolean
        //            Dim FieldAuthorable As Boolean
        //            Dim FieldWidth As Integer
        //            Dim FieldWidthTotal As Integer
        //            'Dim IndexColumn() As Integer
        //            Dim adminColumn As appServices_metaDataClass.CDefAdminColumnClass
        //            '
        //            With CDef
        //                If .Id > 0 Then
        //                    For Each keyValuePair As KeyValuePair(Of String, appServices_metaDataClass.CDefFieldClass) In .fields
        //                        Dim field As appServices_metaDataClass.CDefFieldClass = keyValuePair.Value
        //                        FieldActive = field.active
        //                        FieldWidth = genericController.EncodeInteger(field.IndexWidth)
        //                        If FieldActive And (FieldWidth > 0) Then
        //                            adminColumn = New appServices_metaDataClass.CDefAdminColumnClass
        //                            FieldWidthTotal = FieldWidthTotal + FieldWidth
        //                            'ReDim Preserve IndexColumn(.adminColumns.Count)
        //                            DestPtr = -1
        //                            If .adminColumns.Count > 0 Then
        //                                '
        //                                ' Sort the columns to make room
        //                                '
        //                                For DestPtr = .adminColumns.Count - 1 To 0 Step -1
        //                                    If field.IndexColumn >= IndexColumn(DestPtr) Then
        //                                        '
        //                                        ' Put the new entry into Destination+1
        //                                        '
        //                                        Exit For
        //                                    Else
        //                                        '
        //                                        ' move entry destination->destination+1
        //                                        '
        //                                        IndexColumn(DestPtr + 1) = IndexColumn(DestPtr)
        //                                        adminColumn.Name = .adminColumns(DestPtr).Name
        //                                        adminColumn.SortDirection = .adminColumns(DestPtr).SortDirection
        //                                        adminColumn.SortPriority = .adminColumns(DestPtr).SortPriority
        //                                        adminColumn.Width = .adminColumns(DestPtr).Width
        //                                    End If
        //                                Next
        //                            End If
        //                            IndexColumn(DestPtr + 1) = field.IndexColumn
        //                            adminColumn.Name = field.Name
        //                            adminColumn.SortDirection = field.IndexSortDirection
        //                            adminColumn.SortPriority = genericController.EncodeInteger(field.IndexSortOrder)
        //                            adminColumn.Width = FieldWidth
        //                            .adminColumns.Add(adminColumn)
        //                        End If
        //                    Next
        //                    If .adminColumns.Count = 0 Then
        //                        '
        //                        ' Force the Name field as the only column
        //                        '
        //                        adminColumn = New appServices_metaDataClass.CDefAdminColumnClass
        //                        With adminColumn
        //                            .Name = "Name"
        //                            .SortDirection = 1
        //                            .SortPriority = 1
        //                            .Width = 100
        //                            FieldWidthTotal = FieldWidthTotal + .Width
        //                        End With
        //                        .adminColumns.Add(adminColumn)
        //                    End If
        //                    '
        //                    ' Normalize the column widths
        //                    '
        //                    For FieldPointer = 0 To .adminColumns.Count - 1
        //                        With .adminColumns(FieldPointer)
        //                            .Width = 100 * (.Width / FieldWidthTotal)
        //                        End With
        //                    Next
        //                End If
        //            End With
        //            Exit Sub
        //            '
        //            ' ----- Error Trap
        //            '
        //            throw (new GenericException("Unexpected exception"))'  Call handleLegacyClassErrors1("GetDbCDef_SetAdminColumns", "ErrorTrap")
        //        End Sub
        //
        //=============================================================================
        //   Print the manual query form
        //=============================================================================
        //
        private string GetForm_ContentFileManager() {
            string result = "";
            try {
                string InstanceOptionString = "AdminLayout=1&filesystem=content files";
                AddonModel addon = AddonModel.create(core, "{B966103C-DBF4-4655-856A-3D204DEF6B21}");
                string Content = core.addon.execute(addon, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                    addonType = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                    argumentKeyValuePairs = GenericController.convertQSNVAArgumentstoDocPropertiesList(core, InstanceOptionString),
                    instanceGuid = "-2",
                    errorContextMessage = "executing File Manager addon within Content File Manager"
                });
                string Description = "Manage files and folders within the virtual content file area.";
                string ButtonList = ButtonApply + "," + ButtonCancel;
                result = AdminUIController.getBody(core,"Content File Manager", ButtonList, "", false, false, Description, "", 0, Content);
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   Print the manual query form
        //=============================================================================
        //
        private string GetForm_WebsiteFileManager() {
            string result = "";
            try {
                string InstanceOptionString = "AdminLayout=1&filesystem=website files";
                AddonModel addon = AddonModel.create(core, "{B966103C-DBF4-4655-856A-3D204DEF6B21}");
                string Content = core.addon.execute(addon, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                    addonType = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                    argumentKeyValuePairs = GenericController.convertQSNVAArgumentstoDocPropertiesList(core, InstanceOptionString),
                    instanceGuid = "-2",
                    errorContextMessage = "executing File Manager within website file manager"
                });
                string Description = "Manage files and folders within the Website's file area.";
                string ButtonList = ButtonApply + "," + ButtonCancel;
                result = AdminUIController.getBody(core,"Website File Manager", ButtonList, "", false, false, Description, "", 0, Content);
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        // Find and Replace launch tool
        //=============================================================================
        //
        private string GetForm_FindAndReplace() {
            string result = "";
            try {
                //
                bool IsDeveloper = false;
                string QS = null;
                string RecordName = null;
                int RowCnt = 0;
                string TopHalf = "";
                string BottomHalf = "";
                int RowPtr = 0;
                int RecordID = 0;
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                // Dim runAtServer As New runAtServerClass(core)
                string CDefList = "";
                string FindText = "";
                string ReplaceText = "";
                string Button = null;
                int ReplaceRows = 0;
                int FindRows = 0;
                string lcName = null;
                //
                Stream.Add(AdminUIController.getToolFormTitle("Find and Replace", "This tool runs a find and replace operation on content throughout the site."));
                //
                // Process the form
                //
                Button = core.docProperties.getText("button");
                //
                IsDeveloper = core.session.isAuthenticatedDeveloper();
                if (Button == ButtonFindAndReplace) {
                    RowCnt = core.docProperties.getInteger("CDefRowCnt");
                    if (RowCnt > 0) {
                        for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                            if (core.docProperties.getBoolean("Cdef" + RowPtr)) {
                                lcName = GenericController.vbLCase(core.docProperties.getText("CDefName" + RowPtr));
                                if (IsDeveloper || (lcName == "page content") || (lcName == "copy content") || (lcName == "page templates")) {
                                    CDefList = CDefList + "," + lcName;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(CDefList)) {
                            CDefList = CDefList.Substring(1);
                        }
                        FindText = core.docProperties.getText("FindText");
                        ReplaceText = core.docProperties.getText("ReplaceText");
                        QS = "app=" + encodeNvaArgument(core.appConfig.name) + "&FindText=" + encodeNvaArgument(FindText) + "&ReplaceText=" + encodeNvaArgument(ReplaceText) + "&CDefNameList=" + encodeNvaArgument(CDefList);
                        var cmdDetail = new TaskModel.CmdDetailClass {
                            addonId = 0,
                            addonName = "GetForm_FindAndReplace",
                            args = GenericController.convertQSNVAArgumentstoDocPropertiesList(core, QS)
                        };
                        TaskSchedulerController.addTaskToQueue(core, cmdDetail, false);
                        Stream.Add("Find and Replace has been requested for content definitions [" + CDefList + "], finding [" + FindText + "] and replacing with [" + ReplaceText + "]");
                    }
                } else {
                    CDefList = "Page Content,Copy Content,Page Templates";
                    FindText = "";
                    ReplaceText = "";
                }
                //
                // Display form
                //
                FindRows = core.docProperties.getInteger("SQLRows");
                if (FindRows == 0) {
                    FindRows = core.userProperty.getInteger("FindAndReplaceFindRows", 1);
                } else {
                    core.userProperty.setProperty("FindAndReplaceFindRows", FindRows.ToString());
                }
                ReplaceRows = core.docProperties.getInteger("ReplaceRows");
                if (ReplaceRows == 0) {
                    ReplaceRows = core.userProperty.getInteger("FindAndReplaceReplaceRows", 1);
                } else {
                    core.userProperty.setProperty("FindAndReplaceReplaceRows", ReplaceRows.ToString());
                }
                //
                Stream.Add("<div>Find</div>");
                Stream.Add("<TEXTAREA NAME=\"FindText\" ROWS=\"" + FindRows + "\" ID=\"FindText\" STYLE=\"width: 800px;\">" + FindText + "</TEXTAREA>");
                Stream.Add("&nbsp;<INPUT TYPE=\"Text\" TabIndex=-1 NAME=\"FindTextRows\" SIZE=\"3\" VALUE=\"" + FindRows + "\" ID=\"\"  onchange=\"FindText.rows=FindTextRows.value; return true\"> Rows");
                Stream.Add("<br><br>");
                //
                Stream.Add("<div>Replace it with</div>");
                Stream.Add("<TEXTAREA NAME=\"ReplaceText\" ROWS=\"" + ReplaceRows + "\" ID=\"ReplaceText\" STYLE=\"width: 800px;\">" + ReplaceText + "</TEXTAREA>");
                Stream.Add("&nbsp;<INPUT TYPE=\"Text\" TabIndex=-1 NAME=\"ReplaceTextRows\" SIZE=\"3\" VALUE=\"" + ReplaceRows + "\" ID=\"\"  onchange=\"ReplaceText.rows=ReplaceTextRows.value; return true\"> Rows");
                Stream.Add("<br><br>");
                //
                using (var csData = new CsModel(core)) {
                    csData.open("Content");
                    while (csData.ok()) {
                        RecordName = csData.getText("Name");
                        lcName = GenericController.vbLCase(RecordName);
                        if (IsDeveloper || (lcName == "page content") || (lcName == "copy content") || (lcName == "page templates")) {
                            RecordID = csData.getInteger("ID");
                            if (GenericController.vbInstr(1, "," + CDefList + ",", "," + RecordName + ",") != 0) {
                                TopHalf = TopHalf + "<div>" + HtmlController.checkbox("Cdef" + RowPtr, true) + HtmlController.inputHidden("CDefName" + RowPtr, RecordName) + "&nbsp;" + csData.getText( "Name") + "</div>";
                            } else {
                                BottomHalf = BottomHalf + "<div>" + HtmlController.checkbox("Cdef" + RowPtr, false) + HtmlController.inputHidden("CDefName" + RowPtr, RecordName) + "&nbsp;" + csData.getText( "Name") + "</div>";
                            }
                        }
                        csData.goNext();
                        RowPtr = RowPtr + 1;
                    }
                }
                Stream.Add(TopHalf + BottomHalf + HtmlController.inputHidden("CDefRowCnt", RowPtr));
                //
                result = AdminUIController.getToolForm(core, Stream.Text, ButtonCancel + "," + ButtonFindAndReplace);
                //result = adminUIController.getToolFormOpen(core, ) + Stream.Text + adminUIController.getToolFormClose(core, ButtonCancel + "," + ButtonFindAndReplace);
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        // todo change iisreset to an addon
        private string GetForm_IISReset() {
            string result = "";
            try {
                string Button = null;
                StringBuilderLegacyController s = new StringBuilderLegacyController();
                //
                s.Add(AdminUIController.getToolFormTitle("IIS Reset", "Reset the webserver."));
                //
                // Process the form
                //
                Button = core.docProperties.getText("button");
                //
                if (Button == ButtonIISReset) {
                    //
                    //
                    //
                    LogController.logDebug(core, "Restarting IIS");
                    core.webServer.redirect("https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/Popup/WaitForIISReset.htm", "Redirect to iis reset");
                    Thread.Sleep(2000);
                    var cmdDetail = new TaskModel.CmdDetailClass {
                        addonId = 0,
                        addonName = "GetForm_IISReset",
                        args = new Dictionary<string, string>()
                    };
                    TaskSchedulerController.addTaskToQueue(core, cmdDetail, false );
                }
                //
                // Display form
                //
                result = AdminUIController.getToolForm(core, s.Text, ButtonCancel + "," + ButtonIISReset);
                //result = adminUIController.getToolFormOpen(core, ButtonCancel + "," + ButtonIISReset) + s.Text + adminUIController.getToolFormClose(core, ButtonCancel + "," + ButtonIISReset);
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //=============================================================================
        // GUID Tools
        //=============================================================================
        //
        private string GetForm_CreateGUID() {
            string result = "";
            try {
                string Button = null;
                StringBuilderLegacyController s;
                s = new StringBuilderLegacyController();
                s.Add(AdminUIController.getToolFormTitle("Create GUID", "Use this tool to create a GUID. This is useful when creating new Addons."));
                //
                // Process the form
                Button = core.docProperties.getText("button");
                //
                s.Add(HtmlController.inputText( core,"GUID", GenericController.getGUID(), 1, 80));
                //
                // Display form
                result = AdminUIController.getToolForm(core, s.Text, ButtonCancel + "," + ButtonCreateGUID);
                //result = adminUIController.getToolFormOpen(core, ButtonCancel + "," + ButtonCreateGUID) + s.Text + adminUIController.getToolFormClose(core, ButtonCancel + "," + ButtonCreateGUID);
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        // <summary>
        // 'handle legacy errors in this class, v1
        // </summary>
        // <param name="MethodName"></param>
        // <param name="ignore0"></param>
        // <remarks></remarks>
        //Private Sub handleLegacyClassErrors1(ByVal MethodName As String, Optional ByVal ignore0 As String = "")
        //   throw (new GenericException("Unexpected exception"))'core.handleLegacyError("Tools", MethodName, Err.Number, Err.Source, Err.Description, True, False)
        //End Sub
        //
        //====================================================================================================
        // <summary>
        // handle legacy errors in this class, v2
        // </summary>
        // <param name="MethodName"></param>
        // <param name="ErrDescription"></param>
        // <remarks></remarks>
        //Private Sub handleLegacyClassErrors2(ByVal MethodName As String, ByVal ErrDescription As String)
        //    fixme-- logController.handleException( core,new GenericException("")) ' -----ignoreInteger, "App.EXEName", ErrDescription)
        //End Sub




    }
}
