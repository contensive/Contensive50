
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
using System.Threading;
//
namespace Contensive.Core.Addons.Tools {
    public class legacyToolsClass {
        //========================================================================
        // This file and its contents are copyright by Kidwell McGowan Associates.
        //========================================================================
        //
        // ----- global scope variables
        //
        //
        // ----- data fields
        //
        private int AdminContentMax; // after init, max possible value for AdminContentID
        private bool FieldProblems;
        private string[] Findstring = new string[51]; // Value to search for each index column
                                                      //
                                                      //=============================================================================
                                                      // Development Tools values
                                                      //=============================================================================
                                                      //
        private coreClass cpCore;
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
        private int ToolsSourceForm;
        //
        // ----- Actions
        //
        private int ToolsAction;
        //
        // ----- Buttons
        //
        private string Button;
        //
        private bool ClassInitialized; // if true, the module has been
                                       //
                                       // ----- Content Field descriptors (admin and tools)
                                       //
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
        private int DiagActionCount;
        private const int DiagActionCountMax = 10;
        //
        private struct DiagActionType {
            public string Name;
            public string Command;
        }
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
        //Dependencies:
        //   LibCommon2.asp
        //   LibAdmin3.asp
        //   site field definitions
        //
        //   This file and its contents are copyright by Kidwell McGowan Associates.
        //========================================================================
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public legacyToolsClass(coreClass cpCore) : base() {
            this.cpCore = cpCore;
        }
        //
        //========================================================================
        // Print the Tools page
        //========================================================================
        //
        public string GetForm() {
            string tempGetForm = null;
            try {
                //
                int MenuEntryID = 0;
                int MenuHeaderID = 0;
                int MenuDirection = 0;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                adminUIController Adminui = new adminUIController(cpCore);
                //
                Button = cpCore.docProperties.getText("Button");
                if (Button == ButtonCancelAll) {
                    //
                    // Cancel to the admin site
                    //
                    return cpCore.webServer.redirect(cpCore.siteProperties.getText("AdminURL", "/admin/"));
                }
                //
                // ----- Check permissions
                //
                if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                    //
                    // You must be admin to use this feature
                    //
                    tempGetForm = Adminui.GetFormBodyAdminOnly();
                    tempGetForm = Adminui.GetBody("Admin Tools", ButtonCancelAll, "", false, false, "<div>Administration Tools</div>", "", 0, tempGetForm);
                } else {
                    ToolsAction = cpCore.docProperties.getInteger("dta");
                    Button = cpCore.docProperties.getText("Button");
                    AdminFormTool = cpCore.docProperties.getInteger(RequestNameAdminForm);
                    ToolsTable = cpCore.docProperties.getText("dtt");
                    ToolsContentName = cpCore.docProperties.getText("ContentName");
                    ToolsQuery = cpCore.docProperties.getText("dtq");
                    ToolsDataSource = cpCore.docProperties.getText("dtds");
                    MenuEntryID = cpCore.docProperties.getInteger("dtei");
                    MenuHeaderID = cpCore.docProperties.getInteger("dthi");
                    MenuDirection = cpCore.docProperties.getInteger("dtmd");
                    DefaultReadOnly = cpCore.docProperties.getBoolean("dtdreadonly");
                    DefaultActive = cpCore.docProperties.getBoolean("dtdactive");
                    DefaultPassword = cpCore.docProperties.getBoolean("dtdpassword");
                    DefaulttextBuffered = cpCore.docProperties.getBoolean("dtdtextbuffered");
                    DefaultRequired = cpCore.docProperties.getBoolean("dtdrequired");
                    DefaultAdminOnly = cpCore.docProperties.getBoolean("dtdadmin");
                    DefaultDeveloperOnly = cpCore.docProperties.getBoolean("dtddev");
                    defaultAddMenu = cpCore.docProperties.getBoolean("dtdam");
                    DefaultCreateBlankRecord = cpCore.docProperties.getBoolean("dtblank");
                    //
                    cpCore.doc.addRefreshQueryString("dta", ToolsAction.ToString());
                    //
                    if (Button == ButtonCancel) {
                        //
                        // -- Cancel
                        return cpCore.webServer.redirect(cpCore.siteProperties.getText("AdminURL", "/admin/"));
                    } else {
                        //
                        // -- Print out the page
                        switch (AdminFormTool) {
                            case AdminFormToolContentDiagnostic:
                                //
                                Stream.Add(GetForm_ContentDiagnostic());
                                break;
                            case AdminFormToolCreateContentDefinition:
                                //
                                Stream.Add(GetForm_CreateContentDefinition());
                                break;
                            case AdminFormToolDefineContentFieldsFromTable:
                                //
                                Stream.Add(GetForm_DefineContentFieldsFromTable());
                                break;
                            case AdminFormToolConfigureListing:
                                //
                                Stream.Add(GetForm_ConfigureListing());
                                break;
                            case AdminFormToolConfigureEdit:
                                //
                                //Call Stream.Add(cpCore.addon.execute(guid_ToolConfigureEdit))
                                Stream.Add(GetForm_ConfigureEdit(cpCore.cp_forAddonExecutionOnly));
                                break;
                            case AdminFormToolManualQuery:
                                //
                                Stream.Add(GetForm_ManualQuery());
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
                            case AdminFormToolRestart:
                                //
                                Stream.Add(GetForm_Restart());
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
                cpCore.handleException(ex);
            }
            return tempGetForm;
        }
        //
        //=============================================================================
        //   Remove all Content Fields and rebuild them from the fields found in a table
        //=============================================================================
        //
        private string GetForm_DefineContentFieldsFromTable() {
            string result = "";
            try {
                //
                string SQL = null;
                int CS = 0;
                string DataSourceName = null;
                string TableName = null;
                string ContentName = null;
                int ContentID = 0;
                int DataSourceCount = 0;
                int ItemCount = 0;
                int CSPointer = 0;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string ButtonList = "";
                //
                Stream.Add(SpanClassAdminNormal + "<strong><A href=\"" + cpCore.webServer.requestPage + "?af=" + AdminFormToolRoot + "\">Tools</A></strong></SPAN>");
                Stream.Add(SpanClassAdminNormal + ":Create Content Fields from Table</SPAN>");
                //
                //   print out the submit form
                //
                Stream.Add("<table border=\"0\" cellpadding=\"11\" cellspacing=\"0\" width=\"100%\">");
                //
                Stream.Add("<tr><td colspan=\"2\">" + SpanClassAdminNormal);
                Stream.Add("Delete the current content field definitions for this Content Definition, and recreate them from the table referenced by this content.");
                Stream.Add("</SPAN></td></tr>");
                //
                Stream.Add("<tr>");
                Stream.Add("<TD>" + SpanClassAdminNormal + "Content Name</SPAN></td>");
                Stream.Add("<TD><Select name=\"ContentName\">");
                ItemCount = 0;
                CSPointer = cpCore.db.csOpen("Content", "", "name");
                while (cpCore.db.csOk(CSPointer)) {
                    Stream.Add("<option value=\"" + cpCore.db.csGetText(CSPointer, "name") + "\">" + cpCore.db.csGetText(CSPointer, "name") + "</option>");
                    ItemCount = ItemCount + 1;
                    cpCore.db.csGoNext(CSPointer);
                }
                if (ItemCount == 0) {
                    Stream.Add("<option value=\"-1\">System</option>");
                }
                Stream.Add("</select></td>");
                Stream.Add("</tr>");
                //
                Stream.Add("<tr>");
                Stream.Add("<TD>&nbsp;</td>");
                Stream.Add("<TD><INPUT type=\"submit\" value=\"" + ButtonCreateFields + "\" name=\"Button\"></td>");
                Stream.Add("</tr>");
                //
                Stream.Add("<tr>");
                Stream.Add("<td width=\"150\"><IMG alt=\"\" src=\"/ccLib/Images/spacer.gif\" width=\"150\" height=\"1\"></td>");
                Stream.Add("<td width=\"99%\"><IMG alt=\"\" src=\"/ccLib/Images/spacer.gif\" width=\"100%\" height=\"1\"></td>");
                Stream.Add("</tr>");
                Stream.Add("</TABLE>");
                Stream.Add("</form>");
                //
                //   process the button if present
                //
                if (Button == ButtonCreateFields) {
                    ContentName = cpCore.docProperties.getText("ContentName");
                    if (string.IsNullOrEmpty(ContentName)) {
                        Stream.Add("Select a content before submitting. Fields were not changed.");
                    } else {
                        ContentID = Local_GetContentID(ContentName);
                        if (ContentID == 0) {
                            Stream.Add("GetContentID failed. Fields were not changed.");
                        } else {
                            cpCore.db.deleteContentRecords("Content Fields", "ContentID=" + cpCore.db.encodeSQLNumber(ContentID));
                        }
                    }
                }
                result = htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        private class fieldSortClass {
            public string sort;
            public Models.Complex.CDefFieldModel field;
        }
        //
        //=============================================================================
        //   prints a menu of Tools in a 100% table
        //=============================================================================
        //
        private string GetForm_Root() {
            string result = "";
            try {
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
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
                Stream.Add(GetForm_RootRow(AdminFormTools, AdminformToolCreateGUID, "Create GUID", "Use this tool to create a new GUID. This is useful when creating a new cpcore.addon."));
                Stream.Add("</table>");
                result = htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
                tempGetForm_RootRow = tempGetForm_RootRow + SpanClassAdminNormal + "<P class=\"ccAdminNormal\"><A href=\"" + cpCore.webServer.requestPage + "?af=" + AdminFormToolId.ToString() + "\"><B>" + Caption + "</b></SPAN></A></p>";
                tempGetForm_RootRow = tempGetForm_RootRow + "</td></tr>";
                if (!string.IsNullOrEmpty(Description)) {
                    tempGetForm_RootRow = tempGetForm_RootRow + "<tr><td width=\"30\"><img src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"30\"></td>";
                    tempGetForm_RootRow = tempGetForm_RootRow + "<td width=\"100%\"><P class=\"ccAdminsmall\">" + Description + "</p></td></tr>";
                }
                return tempGetForm_RootRow;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return tempGetForm_RootRow;
        }
        //
        //==================================================================================
        //
        //==================================================================================
        //
        private string GetTitle(string Title, string Description) {
            string result = "";
            try {
                result = ""
                    + "<p>" + SpanClassAdminNormal + "<a href=\"" + cpCore.webServer.requestPage + "?af=" + AdminFormToolRoot + "\"><b>Tools</b></a>&nbsp;&gt;&nbsp;" + Title + "</p>"
                    + "<p>" + SpanClassAdminNormal + Description + "</p>";
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   Print the manual query form
        //=============================================================================
        //
        private string GetForm_ManualQuery() {
            string returnHtml = "";
            try {
                //Dim DataSourceName As String
                //Dim DataSourceID As Integer
                string SQL = "";
                DataTable dt = null;
                int FieldCount = 0;
                string SQLFilename = null;
                string SQLArchive = null;
                string SQLArchiveOld = null;
                int LineCounter = 0;
                string SQLLine = null;
                int Timeout = 0;
                int PageSize = 0;
                int PageNumber = 0;
                int RowMax = 0;
                int RowPointer = 0;
                int ColumnMax = 0;
                int ColumnPointer = 0;
                string ColumnStart = null;
                string ColumnEnd = null;
                string RowStart = null;
                string RowEnd = null;
                string[,] resultArray = null;
                string CellData = null;
                int SelectFieldWidthLimit = 0;
                string SQLName = null;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string ButtonList = null;
                dataSourceModel datasource = dataSourceModel.create(cpCore, cpCore.docProperties.getInteger("dataSourceid"));
                //
                ButtonList = ButtonCancel + "," + ButtonRun;
                //
                Stream.Add(GetTitle("Run Manual Query", "This tool runs an SQL statement on a selected datasource. If there is a result set, the set is printed in a table."));
                //
                // Get the members SQL Queue
                //
                SQLFilename = cpCore.userProperty.getText("SQLArchive");
                if (string.IsNullOrEmpty(SQLFilename)) {
                    SQLFilename = "SQLArchive" + cpCore.doc.authContext.user.id.ToString("000000000") + ".txt";
                    cpCore.userProperty.setProperty("SQLArchive", SQLFilename);
                }
                SQLArchive = cpCore.cdnFiles.readFile(SQLFilename);
                //
                // Read in arguments if available
                //
                Timeout = cpCore.docProperties.getInteger("Timeout");
                if (Timeout == 0) {
                    Timeout = 30;
                }
                //
                PageSize = cpCore.docProperties.getInteger("PageSize");
                if (PageSize == 0) {
                    PageSize = 10;
                }
                //
                PageNumber = cpCore.docProperties.getInteger("PageNumber");
                if (PageNumber == 0) {
                    PageNumber = 1;
                }
                //
                SQL = cpCore.docProperties.getText("SQL");
                if (string.IsNullOrEmpty(SQL)) {
                    SQL = cpCore.docProperties.getText("SQLList");
                }
                //
                if ((cpCore.docProperties.getText("button")) == ButtonRun) {
                    //
                    // Add this SQL to the members SQL list
                    //
                    if (!string.IsNullOrEmpty(SQL)) {
                        SQLArchive = genericController.vbReplace(SQLArchive, SQL + "\r\n", "");
                        SQLArchiveOld = SQLArchive;
                        SQLArchive = genericController.vbReplace(SQL, "\r\n", " ") + "\r\n";
                        LineCounter = 0;
                        while ((LineCounter < 10) && (!string.IsNullOrEmpty(SQLArchiveOld))) {
                            SQLArchive = SQLArchive + getLine( ref SQLArchiveOld) + "\r\n";
                        }
                        cpCore.appRootFiles.saveFile(SQLFilename, SQLArchive);
                    }
                    //
                    // Run the SQL
                    //
                    Stream.Add("<P>" + SpanClassAdminSmall);
                    Stream.Add(DateTime.Now + " Executing sql [" + SQL + "] on DataSource [" + datasource.Name + "]");
                    try {
                        dt = cpCore.db.executeQuery(SQL, datasource.Name, PageSize * (PageNumber - 1), PageSize);
                    } catch (Exception ex) {
                        //
                        // ----- error
                        //
                        Stream.Add("<br>" + DateTime.Now + " SQL execution returned the following error");
                        Stream.Add("<br>" + ex.Message);
                    }
                    Stream.Add("<br>" + DateTime.Now + " SQL executed successfully");
                    if (dt == null) {
                        Stream.Add("<br>" + DateTime.Now + " SQL returned invalid data.");
                    } else if (dt.Rows == null) {
                        Stream.Add("<br>" + DateTime.Now + " SQL returned invalid data rows.");
                    } else if (dt.Rows.Count == 0) {
                        Stream.Add("<br>" + DateTime.Now + " The SQL returned no data.");
                    } else {
                        //
                        // ----- print results
                        //
                        Stream.Add("<br>" + DateTime.Now + " The following results were returned");
                        Stream.Add("<br>");
                        //
                        // --- Create the Fields for the new table
                        //
                        FieldCount = dt.Columns.Count;
                        Stream.Add("<table border=\"1\" cellpadding=\"1\" cellspacing=\"1\" width=\"100%\">");
                        Stream.Add("<tr>");
                        foreach (DataColumn dc in dt.Columns) {
                            Stream.Add("<TD><B>" + SpanClassAdminSmall + dc.ColumnName + "</b></SPAN></td>");
                        }
                        Stream.Add("</tr>");
                        //
                        //Dim dtOK As Boolean
                        resultArray = cpCore.db.convertDataTabletoArray(dt);
                        //
                        RowMax = resultArray.GetUpperBound(1);
                        ColumnMax = resultArray.GetUpperBound(0);
                        RowStart = "<tr>";
                        RowEnd = "</tr>";
                        ColumnStart = "<td class=\"ccadminsmall\">";
                        ColumnEnd = "</td>";
                        for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                            Stream.Add(RowStart);
                            for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                CellData = resultArray[ColumnPointer, RowPointer];
                                if (IsNull(CellData)) {
                                    Stream.Add(ColumnStart + "[null]" + ColumnEnd);
                                    //ElseIf IsEmpty(CellData) Then
                                    //    Stream.Add(ColumnStart & "[empty]" & ColumnEnd)
                                    //ElseIf IsArray(CellData) Then
                                    //    Stream.Add(ColumnStart & "[array]")
                                    //    Cnt = UBound(CellData)
                                    //    For Ptr = 0 To Cnt - 1
                                    //        Stream.Add("<br>(" & Ptr & ")&nbsp;[" & CellData[Ptr] & "]")
                                    //    Next
                                    //    Stream.Add(ColumnEnd)
                                } else if (string.IsNullOrEmpty(CellData)) {
                                    Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
                                } else {
                                    Stream.Add(ColumnStart + genericController.encodeHTML(genericController.encodeText(CellData)) + ColumnEnd);
                                }
                            }
                            Stream.Add(RowEnd);
                        }
                        Stream.Add("</TABLE>");
                    }
                    Stream.Add("<br>" + DateTime.Now + " Done");
                    Stream.Add("</P>");
                    //
                    // End of Run SQL
                    //
                }
                //
                // Display form
                //
                //Call Stream.Add(SpanClassAdminNormal & cpCore.main_GetFormStart(""))
                //
                int SQLRows = cpCore.docProperties.getInteger("SQLRows");
                if (SQLRows == 0) {
                    SQLRows = cpCore.userProperty.getInteger("ManualQueryInputRows", 5);
                } else {
                    cpCore.userProperty.setProperty("ManualQueryInputRows", SQLRows.ToString());
                }
                Stream.Add("<TEXTAREA NAME=\"SQL\" ROWS=\"" + SQLRows + "\" ID=\"SQL\" STYLE=\"width: 800px;\">" + SQL + "</TEXTAREA>");
                Stream.Add("&nbsp;<INPUT TYPE=\"Text\" TabIndex=-1 NAME=\"SQLRows\" SIZE=\"3\" VALUE=\"" + SQLRows + "\" ID=\"\"  onchange=\"SQL.rows=SQLRows.value; return true\"> Rows");
                Stream.Add("<br><br>Data Source<br>" + cpCore.html.main_GetFormInputSelect("DataSourceID", datasource.ID, "Data Sources", "", "Default"));
                //
                SelectFieldWidthLimit = cpCore.siteProperties.getInteger("SelectFieldWidthLimit", 200);
                if (!string.IsNullOrEmpty(SQLArchive)) {
                    Stream.Add("<br><br>Previous Queries<br>");
                    Stream.Add("<select size=\"1\" name=\"SQLList\" ID=\"SQLList\" onChange=\"SQL.value=SQLList.value\">");
                    Stream.Add("<option value=\"\">Select One</option>");
                    LineCounter = 0;
                    while ((LineCounter < 10) && (!string.IsNullOrEmpty(SQLArchive))) {
                        SQLLine = getLine( ref SQLArchive);
                        if (SQLLine.Length > SelectFieldWidthLimit) {
                            SQLName = SQLLine.Left( SelectFieldWidthLimit) + "...";
                        } else {
                            SQLName = SQLLine;
                        }
                        Stream.Add("<option value=\"" + SQLLine + "\">" + SQLName + "</option>");
                    }
                    Stream.Add("</select><br>");
                }
                //
                if (IsNull(PageSize)) {
                    PageSize = 100;
                }
                Stream.Add("<br>Page Size:<br>" + cpCore.html.html_GetFormInputText("PageSize", PageSize.ToString()));
                //
                if (IsNull(PageNumber)) {
                    PageNumber = 1;
                }
                Stream.Add("<br>Page Number:<br>" + cpCore.html.html_GetFormInputText("PageNumber", PageNumber.ToString()));
                //
                if (IsNull(Timeout)) {
                    Timeout = 30;
                }
                Stream.Add("<br>Timeout (sec):<br>" + cpCore.html.html_GetFormInputText("Timeout", Timeout.ToString()));
                //
                //Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolManualQuery))
                //Stream.Add( cpCore.main_GetFormEnd & "</SPAN>")
                //
                returnHtml = htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnHtml;
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
                int CS = 0;
                int ContentID = 0;
                string TableName = "";
                string ContentName = "";
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string ButtonList = null;
                adminUIController Adminui = new adminUIController(cpCore);
                string Description = null;
                string Caption = null;
                int NavID = 0;
                int ParentNavID = 0;
                dataSourceModel datasource = dataSourceModel.create(cpCore, cpCore.docProperties.getInteger("DataSourceID"));
                //
                ButtonList = ButtonCancel + "," + ButtonRun;
                Caption = "Create Content Definition";
                Description = "This tool creates a Content Definition. If the SQL table exists, it is used. If it does not exist, it is created. If records exist in the table with a blank ContentControlID, the ContentControlID will be populated from this new definition. A Navigator Menu entry will be added under Manage Site Content - Advanced.";
                //
                //Stream.Add( GetTitle(Caption, Description)
                //
                //   print out the submit form
                //
                if (cpCore.docProperties.getText("Button") != "") {
                    //
                    // Process input
                    //
                    ContentName = cpCore.docProperties.getText("ContentName");
                    TableName = cpCore.docProperties.getText("TableName");
                    //
                    Stream.Add(SpanClassAdminSmall);
                    Stream.Add("<P>Creating content [" + ContentName + "] on table [" + TableName + "] on Datasource [" + datasource.Name + "].</P>");
                    if ((!string.IsNullOrEmpty(ContentName)) & (!string.IsNullOrEmpty(TableName)) & (!string.IsNullOrEmpty(datasource.Name))) {
                        cpCore.db.createSQLTable(datasource.Name, TableName);
                        cpCore.db.createContentFromSQLTable(datasource, TableName, ContentName);
                        cpCore.cache.invalidateAll();
                        cpCore.doc.clearMetaData();
                        ContentID = Models.Complex.cdefModel.getContentId(cpCore, ContentName);
                        ParentNavID = cpCore.db.getRecordID(cnNavigatorEntries, "Manage Site Content");
                        if (ParentNavID != 0) {
                            CS = cpCore.db.csOpen(cnNavigatorEntries, "(name=" + cpCore.db.encodeSQLText("Advanced") + ")and(parentid=" + ParentNavID + ")");
                            ParentNavID = 0;
                            if (cpCore.db.csOk(CS)) {
                                ParentNavID = cpCore.db.csGetInteger(CS, "ID");
                            }
                            cpCore.db.csClose(ref CS);
                            if (ParentNavID != 0) {
                                CS = cpCore.db.csOpen(cnNavigatorEntries, "(name=" + cpCore.db.encodeSQLText(ContentName) + ")and(parentid=" + NavID + ")");
                                if (!cpCore.db.csOk(CS)) {
                                    cpCore.db.csClose(ref CS);
                                    CS = cpCore.db.csInsertRecord(cnNavigatorEntries);
                                }
                                if (cpCore.db.csOk(CS)) {
                                    cpCore.db.csSet(CS, "name", ContentName);
                                    cpCore.db.csSet(CS, "parentid", ParentNavID);
                                    cpCore.db.csSet(CS, "contentid", ContentID);
                                }
                                cpCore.db.csClose(ref CS);
                            }
                        }
                        ContentID = Models.Complex.cdefModel.getContentId(cpCore, ContentName);
                        Stream.Add("<P>Content Definition was created. An admin menu entry for this definition has been added under 'Site Content', and will be visible on the next page view. Use the [<a href=\"?af=105&ContentID=" + ContentID + "\">Edit Content Definition Fields</a>] tool to review and edit this definition's fields.</P>");
                    } else {
                        Stream.Add("<P>Error, a required field is missing. Content not created.</P>");
                    }
                    Stream.Add("</SPAN>");
                }
                Stream.Add(SpanClassAdminNormal);
                Stream.Add("Data Source<br>");
                Stream.Add(cpCore.html.main_GetFormInputSelect("DataSourceID", datasource.ID, "Data Sources", "", "Default"));
                Stream.Add("<br><br>");
                Stream.Add("Content Name<br>");
                Stream.Add(cpCore.html.html_GetFormInputText2("ContentName", ContentName, 1, 40));
                Stream.Add("<br><br>");
                Stream.Add("Table Name<br>");
                Stream.Add(cpCore.html.html_GetFormInputText2("TableName", TableName, 1, 40));
                Stream.Add("<br><br>");
                Stream.Add("</SPAN>");
                result = Adminui.GetBody(Caption, ButtonList, "", false, false, Description, "", 10, Stream.Text);
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
                int ColumnWidth = 0;
                string FieldName = null;
                string AStart = null;
                int CSPointer = 0;
                int ContentID = 0;
                Models.Complex.cdefModel CDef = null;
                string ContentName = null;
                int CS1 = 0;
                int TargetFieldID = 0;
                int ColumnWidthTotal = 0;
                int ColumnNumberMax = 0;
                int fieldId = 0;
                bool AllowContentAutoLoad = false;
                bool MoveNextColumn = false;
                string FieldNameToAdd = null;
                int FieldIDToAdd = 0;
                int CSSource = 0;
                int CSTarget = 0;
                int SourceContentID = 0;
                string SourceName = null;
                bool ReloadCDef = false;
                int InheritedFieldCount = 0;
                string Caption = null;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string ButtonList = null;
                string FormPanel = "";
                //
                const string RequestNameAddField = "addfield";
                const string RequestNameAddFieldID = "addfieldID";
                //
                Stream.Add(GetTitle("Configure Admin Listing", "Configure the Administration Content Listing Page."));
                //
                //--------------------------------------------------------------------------------
                //   Load Request
                //--------------------------------------------------------------------------------
                //
                ToolsAction = cpCore.docProperties.getInteger("dta");
                TargetFieldID = cpCore.docProperties.getInteger("fi");
                ContentID = cpCore.docProperties.getInteger(RequestNameToolContentID);
                //ColumnPointer = cpCore.main_GetStreamInteger("dtcn")
                FieldNameToAdd = genericController.vbUCase(cpCore.docProperties.getText(RequestNameAddField));
                FieldIDToAdd = cpCore.docProperties.getInteger(RequestNameAddFieldID);
                ButtonList = ButtonCancel + "," + ButtonSelect;
                ReloadCDef = cpCore.docProperties.getBoolean("ReloadCDef");
                //
                //--------------------------------------------------------------------------------
                // Process actions
                //--------------------------------------------------------------------------------
                //
                if (ContentID != 0) {
                    ButtonList = ButtonCancel + "," + ButtonSaveandInvalidateCache;
                    ContentName = Local_GetContentNameByID(ContentID);
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentID, true, false);
                    if (ToolsAction != 0) {
                        //
                        // Block contentautoload, then force a load at the end
                        //
                        AllowContentAutoLoad = (cpCore.siteProperties.getBoolean("AllowContentAutoLoad", true));
                        cpCore.siteProperties.setProperty("AllowContentAutoLoad", false);
                        //
                        // Make sure the FieldNameToAdd is not-inherited, if not, create new field
                        //
                        if (FieldIDToAdd != 0) {
                            foreach (var keyValuePair in CDef.fields) {
                                Models.Complex.CDefFieldModel field = keyValuePair.Value;
                                if (field.id == FieldIDToAdd) {
                                    //If field.Name = FieldNameToAdd Then
                                    if (field.inherited) {
                                        SourceContentID = field.contentId;
                                        SourceName = field.nameLc;
                                        CSSource = cpCore.db.csOpen("Content Fields", "(ContentID=" + SourceContentID + ")and(Name=" + cpCore.db.encodeSQLText(SourceName) + ")");
                                        if (cpCore.db.csOk(CSSource)) {
                                            CSTarget = cpCore.db.csInsertRecord("Content Fields");
                                            if (cpCore.db.csOk(CSTarget)) {
                                                cpCore.db.csCopyRecord(CSSource, CSTarget);
                                                cpCore.db.csSet(CSTarget, "ContentID", ContentID);
                                                ReloadCDef = true;
                                            }
                                            cpCore.db.csClose(ref CSTarget);
                                        }
                                        cpCore.db.csClose(ref CSSource);
                                    }
                                    break;
                                }
                            }
                        }
                        //
                        // Make sure all fields are not-inherited, if not, create new fields
                        //
                        ColumnNumberMax = 0;
                        foreach (var keyValuePair in CDef.adminColumns) {
                            Models.Complex.cdefModel.CDefAdminColumnClass adminColumn = keyValuePair.Value;
                            Models.Complex.CDefFieldModel field = CDef.fields[adminColumn.Name];
                            if (field.inherited) {
                                SourceContentID = field.contentId;
                                SourceName = field.nameLc;
                                CSSource = cpCore.db.csOpen("Content Fields", "(ContentID=" + SourceContentID + ")and(Name=" + cpCore.db.encodeSQLText(SourceName) + ")");
                                if (cpCore.db.csOk(CSSource)) {
                                    CSTarget = cpCore.db.csInsertRecord("Content Fields");
                                    if (cpCore.db.csOk(CSTarget)) {
                                        cpCore.db.csCopyRecord(CSSource, CSTarget);
                                        cpCore.db.csSet(CSTarget, "ContentID", ContentID);
                                        ReloadCDef = true;
                                    }
                                    cpCore.db.csClose(ref CSTarget);
                                }
                                cpCore.db.csClose(ref CSSource);
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
                                                Models.Complex.cdefModel.CDefAdminColumnClass adminColumn = keyValuePair.Value;
                                                Models.Complex.CDefFieldModel field = CDef.fields[adminColumn.Name];
                                                CSPointer = cpCore.db.csOpenRecord("Content Fields", field.id);
                                                cpCore.db.csSet(CSPointer, "IndexColumn", (columnPtr) * 10);
                                                cpCore.db.csSet(CSPointer, "IndexWidth", Math.Floor((adminColumn.Width * 80) / (double)ColumnWidthTotal));
                                                cpCore.db.csClose(ref CSPointer);
                                                columnPtr += 1;
                                            }
                                        }
                                        CSPointer = cpCore.db.csOpenRecord("Content Fields", FieldIDToAdd, false, false);
                                        if (cpCore.db.csOk(CSPointer)) {
                                            cpCore.db.csSet(CSPointer, "IndexColumn", columnPtr * 10);
                                            cpCore.db.csSet(CSPointer, "IndexWidth", 20);
                                            cpCore.db.csSet(CSPointer, "IndexSortPriority", 99);
                                            cpCore.db.csSet(CSPointer, "IndexSortDirection", 1);
                                        }
                                        cpCore.db.csClose(ref CSPointer);
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
                                            Models.Complex.cdefModel.CDefAdminColumnClass adminColumn = keyValuePair.Value;
                                            Models.Complex.CDefFieldModel field = CDef.fields[adminColumn.Name];
                                            CSPointer = cpCore.db.csOpenRecord("Content Fields", field.id);
                                            if (fieldId == TargetFieldID) {
                                                cpCore.db.csSet(CSPointer, "IndexColumn", 0);
                                                cpCore.db.csSet(CSPointer, "IndexWidth", 0);
                                                cpCore.db.csSet(CSPointer, "IndexSortPriority", 0);
                                                cpCore.db.csSet(CSPointer, "IndexSortDirection", 0);
                                            } else {
                                                cpCore.db.csSet(CSPointer, "IndexColumn", (columnPtr) * 10);
                                                cpCore.db.csSet(CSPointer, "IndexWidth", Math.Floor((adminColumn.Width * 100) / (double)ColumnWidthTotal));
                                            }
                                            cpCore.db.csClose(ref CSPointer);
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
                                            Models.Complex.cdefModel.CDefAdminColumnClass adminColumn = keyValuePair.Value;
                                            Models.Complex.CDefFieldModel field = CDef.fields[adminColumn.Name];
                                            FieldName = adminColumn.Name;
                                            CS1 = cpCore.db.csOpenRecord("Content Fields", field.id);
                                            if ((CDef.fields[FieldName.ToLower()].id == TargetFieldID) && (columnPtr < CDef.adminColumns.Count)) {
                                                cpCore.db.csSet(CS1, "IndexColumn", (columnPtr + 1) * 10);
                                                //
                                                MoveNextColumn = true;
                                            } else if (MoveNextColumn) {
                                                //
                                                // This is one past target
                                                //
                                                cpCore.db.csSet(CS1, "IndexColumn", (columnPtr - 1) * 10);
                                                MoveNextColumn = false;
                                            } else {
                                                //
                                                // not target or one past target
                                                //
                                                cpCore.db.csSet(CS1, "IndexColumn", (columnPtr) * 10);
                                                MoveNextColumn = false;
                                            }
                                            cpCore.db.csSet(CS1, "IndexWidth", Math.Floor((adminColumn.Width * 100) / (double)ColumnWidthTotal));
                                            cpCore.db.csClose(ref CS1);
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
                                            Models.Complex.cdefModel.CDefAdminColumnClass adminColumn = keyValuePair.Value;
                                            Models.Complex.CDefFieldModel field = CDef.fields[adminColumn.Name];
                                            FieldName = adminColumn.Name;
                                            CS1 = cpCore.db.csOpenRecord("Content Fields", field.id);
                                            if ((field.id == TargetFieldID) && (columnPtr < CDef.adminColumns.Count)) {
                                                cpCore.db.csSet(CS1, "IndexColumn", (columnPtr - 1) * 10);
                                                //
                                                MoveNextColumn = true;
                                            } else if (MoveNextColumn) {
                                                //
                                                // This is one past target
                                                //
                                                cpCore.db.csSet(CS1, "IndexColumn", (columnPtr + 1) * 10);
                                                MoveNextColumn = false;
                                            } else {
                                                //
                                                // not target or one past target
                                                //
                                                cpCore.db.csSet(CS1, "IndexColumn", (columnPtr) * 10);
                                                MoveNextColumn = false;
                                            }
                                            cpCore.db.csSet(CS1, "IndexWidth", Math.Floor((adminColumn.Width * 100) / (double)ColumnWidthTotal));
                                            cpCore.db.csClose(ref CS1);
                                            columnPtr += 1;
                                        }
                                        ReloadCDef = true;
                                    }
                                    // end case
                                    //Case ToolsActionSetAZ
                                    //    '
                                    //    ' Set Column as a-z sort
                                    //    '
                                    //    If CDef.adminColumns.Count > 1 Then
                                    //        For ColumnPointer = 0 To CDef.adminColumns.Count - 1
                                    //            FieldName = CDef.adminColumns(ColumnPointer).Name
                                    //            fieldId = CDef.fields[FieldName.ToLower()].Id
                                    //            CSPointer = cpCore.main_OpenCSContentRecord("Content Fields", fieldId)
                                    //            If fieldId = TargetFieldID Then
                                    //                Call cpCore.app.SetCS(CSPointer, "IndexSortPriority", 0)
                                    //                Call cpCore.app.SetCS(CSPointer, "IndexSortDirection", 1)
                                    //            Else
                                    //                Call cpCore.app.SetCS(CSPointer, "IndexSortPriority", 99)
                                    //                Call cpCore.app.SetCS(CSPointer, "IndexSortDirection", 0)
                                    //            End If
                                    //            Call cpCore.app.SetCS(CSPointer, "IndexColumn", (ColumnPointer) * 10)
                                    //            Call cpCore.app.SetCS(CSPointer, "IndexWidth", Int((CDef.adminColumns(ColumnPointer).Width * 100) / ColumnWidthTotal))
                                    //            Call cpCore.app.closeCS(CSPointer)
                                    //        Next
                                    //        ReloadCDef = True
                                    //    End If
                                    //    ' end case
                                    //Case ToolsActionSetZA
                                    //    '
                                    //    ' Set Column as a-z sort
                                    //    '
                                    //    If CDef.adminColumns.Count > 1 Then
                                    //        For ColumnPointer = 0 To CDef.adminColumns.Count - 1
                                    //            FieldName = CDef.adminColumns(ColumnPointer).Name
                                    //            fieldId = CDef.fields[FieldName.ToLower()].Id
                                    //            CSPointer = cpCore.main_OpenCSContentRecord("Content Fields", fieldId)
                                    //            If fieldId = TargetFieldID Then
                                    //                Call cpCore.app.SetCS(CSPointer, "IndexSortPriority", 0)
                                    //                Call cpCore.app.SetCS(CSPointer, "IndexSortDirection", -1)
                                    //            Else
                                    //                Call cpCore.app.SetCS(CSPointer, "IndexSortPriority", 99)
                                    //                Call cpCore.app.SetCS(CSPointer, "IndexSortDirection", 0)
                                    //            End If
                                    //            Call cpCore.app.SetCS(CSPointer, "IndexColumn", (ColumnPointer) * 10)
                                    //            Call cpCore.app.SetCS(CSPointer, "IndexWidth", Int((CDef.adminColumns(ColumnPointer).Width * 100) / ColumnWidthTotal))
                                    //            Call cpCore.app.closeCS(CSPointer)
                                    //        Next
                                    //        ReloadCDef = True
                                    //    End If
                                    //    ' end case
                                    //Case ToolsActionExpand
                                    //    '
                                    //    ' Expand column
                                    //    '
                                    //    ColumnWidthBalance = 0
                                    //    If CDef.adminColumns.Count > 0 Then
                                    //        '
                                    //        ' Calculate the total width of the non-target columns
                                    //        '
                                    //        ColumnWidthIncrease = ColumnWidthTotal * 0.1
                                    //        For ColumnPointer = 0 To CDef.adminColumns.Count - 1
                                    //            FieldName = CDef.adminColumns(ColumnPointer).Name
                                    //            fieldId = CDef.fields[FieldName.ToLower()].Id
                                    //            If fieldId <> TargetFieldID Then
                                    //                ColumnWidthBalance = ColumnWidthBalance + CDef.adminColumns(ColumnPointer).Width
                                    //            End If
                                    //        Next
                                    //        '
                                    //        ' Adjust all columns
                                    //        '
                                    //        If ColumnWidthBalance > 0 Then
                                    //            For ColumnPointer = 0 To CDef.adminColumns.Count - 1
                                    //                FieldName = CDef.adminColumns(ColumnPointer).Name
                                    //                fieldId = CDef.fields[FieldName.ToLower()].Id
                                    //                CSPointer = cpCore.main_OpenCSContentRecord("Content Fields", fieldId)
                                    //                If fieldId = TargetFieldID Then
                                    //                    '
                                    //                    ' Target gets 10% increase
                                    //                    '
                                    //                    Call cpCore.app.SetCS(CSPointer, "IndexWidth", Int(CDef.adminColumns(ColumnPointer).Width + ColumnWidthIncrease))
                                    //                Else
                                    //                    '
                                    //                    ' non-targets get their share of the shrinkage
                                    //                    '
                                    //                    FieldWidth = CDef.adminColumns(ColumnPointer).Width
                                    //                    FieldWidth = FieldWidth - ((ColumnWidthIncrease * FieldWidth) / ColumnWidthBalance)
                                    //                    Call cpCore.app.SetCS(CSPointer, "IndexWidth", Int(FieldWidth))
                                    //                End If
                                    //                Call cpCore.app.SetCS(CSPointer, "IndexColumn", (ColumnPointer) * 10)
                                    //                Call cpCore.app.closeCS(CSPointer)
                                    //            Next
                                    //            ReloadCDef = True
                                    //        End If
                                    //    End If

                                    //    ' end case
                                    //Case ToolsActionContract
                                    //    '
                                    //    ' Contract column
                                    //    '
                                    //    ColumnWidthBalance = 0
                                    //    If CDef.adminColumns.Count > 0 Then
                                    //        '
                                    //        ' Calculate the total width of the non-target columns
                                    //        '
                                    //        ColumnWidthIncrease = -(ColumnWidthTotal * 0.1)
                                    //        For ColumnPointer = 0 To CDef.adminColumns.Count - 1
                                    //            FieldName = CDef.adminColumns(ColumnPointer).Name
                                    //            fieldId = CDef.fields[FieldName.ToLower()].Id
                                    //            If fieldId = TargetFieldID Then
                                    //                FieldWidth = CDef.adminColumns(ColumnPointer).Width
                                    //                If (FieldWidth + ColumnWidthIncrease) < 10 Then
                                    //                    ColumnWidthIncrease = 10 - FieldWidth
                                    //                End If
                                    //            Else
                                    //                ColumnWidthBalance = ColumnWidthBalance + CDef.adminColumns(ColumnPointer).Width
                                    //            End If
                                    //        Next
                                    //        '
                                    //        ' Adjust all columns
                                    //        '
                                    //        If (ColumnWidthBalance > 0) And (ColumnWidthIncrease <> 0) Then
                                    //            For ColumnPointer = 0 To CDef.adminColumns.Count - 1
                                    //                FieldName = CDef.adminColumns(ColumnPointer).Name
                                    //                fieldId = CDef.fields[FieldName.ToLower()].Id
                                    //                CSPointer = cpCore.main_OpenCSContentRecord("Content Fields", fieldId)
                                    //                If fieldId = TargetFieldID Then
                                    //                    '
                                    //                    ' Target gets 10% increase
                                    //                    '
                                    //                    FieldWidth = Int(CDef.adminColumns(ColumnPointer).Width + ColumnWidthIncrease)
                                    //                    Call cpCore.app.SetCS(CSPointer, "IndexWidth", FieldWidth)
                                    //                Else
                                    //                    '
                                    //                    ' non-targets get their share of the shrinkage
                                    //                    '
                                    //                    FieldWidth = CDef.adminColumns(ColumnPointer).Width
                                    //                    FieldWidth = FieldWidth - ((ColumnWidthIncrease * FieldWidth) / ColumnWidthBalance)
                                    //                    Call cpCore.app.SetCS(CSPointer, "IndexWidth", Int(FieldWidth))
                                    //                End If
                                    //                Call cpCore.app.SetCS(CSPointer, "IndexColumn", (ColumnPointer) * 10)
                                    //                Call cpCore.app.closeCS(CSPointer)
                                    //            Next
                                    //            ReloadCDef = True
                                    //        End If
                                    //    End If
                                    break;
                                }
                        }
                        //
                        // Get a new copy of the content definition
                        //
                        CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentID, true, false);
                    }
                    if (Button == ButtonSaveandInvalidateCache) {
                        cpCore.cache.invalidateAll();
                        cpCore.doc.clearMetaData();
                        return cpCore.webServer.redirect("?af=" + AdminFormToolConfigureListing + "&ContentID=" + ContentID);
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
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"/ccLib/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"/ccLib/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"/ccLib/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"/ccLib/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"/ccLib/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"/ccLib/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"/ccLib/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"/ccLib/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"/ccLib/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"/ccLib/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><IMG src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\"><IMG alt=\"\" src=\"/ccLib/Images/spacer.gif\" width=\"100%\" height=\"10\"></nobr></td>");
                    Stream.Add("</tr></TABLE>");
                    //
                    // print the column headers
                    //
                    ColumnWidthTotal = 0;
                    if (CDef.adminColumns.Count > 0) {
                        //
                        // Calc total width
                        //
                        foreach (KeyValuePair<string, Models.Complex.cdefModel.CDefAdminColumnClass> kvp in CDef.adminColumns) {
                            ColumnWidthTotal += kvp.Value.Width;
                        }
                        //For ColumnCount = 0 To CDef.adminColumns.Count - 1
                        //    ColumnWidthTotal = ColumnWidthTotal + CDef.adminColumns(ColumnCount).Width
                        //Next
                        if (ColumnWidthTotal > 0) {
                            Stream.Add("<table border=\"0\" cellpadding=\"5\" cellspacing=\"0\" width=\"90%\">");
                            int ColumnCount = 0;
                            foreach (KeyValuePair<string, Models.Complex.cdefModel.CDefAdminColumnClass> kvp in CDef.adminColumns) {
                                //
                                // print column headers - anchored so they sort columns
                                //
                                ColumnWidth = EncodeInteger(100 * (kvp.Value.Width / (double)ColumnWidthTotal));
                                FieldName = kvp.Value.Name;
                                var tempVar = CDef.fields[FieldName.ToLower()];
                                fieldId = tempVar.id;
                                Caption = tempVar.caption;
                                if (tempVar.inherited) {
                                    Caption = Caption + "*";
                                    InheritedFieldCount = InheritedFieldCount + 1;
                                }
                                AStart = "<A href=\"" + cpCore.webServer.requestPage + "?" + RequestNameToolContentID + "=" + ContentID + "&af=" + AdminFormToolConfigureListing + "&fi=" + fieldId + "&dtcn=" + ColumnCount;
                                Stream.Add("<td width=\"" + ColumnWidth + "%\" valign=\"top\" align=\"left\">" + SpanClassAdminNormal + Caption + "<br>");
                                Stream.Add("<IMG src=\"/ccLib/images/black.GIF\" width=\"100%\" height=\"1\">");
                                Stream.Add(AStart + "&dta=" + ToolsActionRemoveField + "\"><IMG src=\"/ccLib/images/LibButtonDeleteUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionMoveFieldRight + "\"><IMG src=\"/ccLib/images/LibButtonMoveRightUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionMoveFieldLeft + "\"><IMG src=\"/ccLib/images/LibButtonMoveLeftUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionSetAZ + "\"><IMG src=\"/ccLib/images/LibButtonSortazUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionSetZA + "\"><IMG src=\"/ccLib/images/LibButtonSortzaUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionExpand + "\"><IMG src=\"/ccLib/images/LibButtonOpenUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionContract + "\"><IMG src=\"/ccLib/images/LibButtonCloseUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A>");
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
                        foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in CDef.fields) {
                            Models.Complex.CDefFieldModel field = keyValuePair.Value;
                            //
                            // test if this column is in use
                            //
                            skipField = false;
                            //ColumnPointer = CDef.adminColumns.Count
                            if (CDef.adminColumns.Count > 0) {
                                foreach (KeyValuePair<string, Models.Complex.cdefModel.CDefAdminColumnClass> kvp in CDef.adminColumns) {
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
                                if (false) {
                                    // this causes more problems then it fixes
                                    //If Not .Authorable Then
                                    //
                                    // not authorable
                                    //
                                    Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (not authorable field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileText) {
                                    //
                                    // text filename can not be search
                                    //
                                    Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (text file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileCSS) {
                                    //
                                    // text filename can not be search
                                    //
                                    Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (css file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileXML) {
                                    //
                                    // text filename can not be search
                                    //
                                    Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (xml file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileJavascript) {
                                    //
                                    // text filename can not be search
                                    //
                                    Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (javascript file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdLongText) {
                                    //
                                    // long text can not be search
                                    //
                                    Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (long text field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileImage) {
                                    //
                                    // long text can not be search
                                    //
                                    Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (image field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdRedirect) {
                                    //
                                    // long text can not be search
                                    //
                                    Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (redirect field)<br>");
                                } else {
                                    //
                                    // can be used as column header
                                    //
                                    Stream.Add("<A href=\"" + cpCore.webServer.requestPage + "?" + RequestNameToolContentID + "=" + ContentID + "&af=" + AdminFormToolConfigureListing + "&fi=" + field.id + "&dta=" + ToolsActionAddField + "&" + RequestNameAddFieldID + "=" + field.id + "\"><IMG src=\"/ccLib/images/LibButtonAddUp.gif\" width=\"50\" height=\"15\" border=\"0\"></A> " + field.caption + "<br>");
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
                FormPanel = FormPanel + SpanClassAdminNormal + "Select a Content Definition to Configure its Listing Page<br>";
                //FormPanel = FormPanel & cpCore.main_GetFormInputHidden("af", AdminFormToolConfigureListing)
                FormPanel = FormPanel + cpCore.html.main_GetFormInputSelect("ContentID", ContentID, "Content");
                Stream.Add(cpCore.html.getPanel(FormPanel));
                //
                cpCore.siteProperties.setProperty("AllowContentAutoLoad", AllowContentAutoLoad);
                Stream.Add(cpCore.html.html_GetFormInputHidden("ReloadCDef", ReloadCDef));
                result =  htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //=============================================================================
        // checks Content against Database tables
        //=============================================================================
        //
        private string GetForm_ContentDiagnostic() {
            string result = "";
            try {
                //
                string SQL = null;
                int DataSourceID = 0;
                string DataSourceName = null;
                string TableName = null;
                string ContentName = null;
                int RecordID = 0;
                int CSContent = 0;
                int CSDataSources = 0;
                string StartButton = null;
                string Name = null;
                //Dim ConnectionObject As Connection
                string ConnectionString = null;
                int CSPointer = 0;
                int ErrorCount = 0;
                string FieldName = null;
                string bitBucket = null;
                int ContentID = 0;
                int ParentID = 0;
                //
                int CSFields = 0;
                int CSTestRecord = 0;
                int TestRecordID = 0;
                int fieldType = 0;
                int RedirectContentID = 0;
                int LookupContentID = 0;
                string LinkPage = null;
                string LookupList = null;
                //
                string DiagProblem = null;
                //
                int iDiagActionCount = 0;
                int DiagActionPointer = 0;
                string DiagAction = null;
                DiagActionType[] DiagActions = null;
                int fieldId = 0;
                int CS = 0;
                int CSTest = 0;
                string Button = null;
                bool FieldRequired = false;
                bool FieldAuthorable = false;
                //
                string PathFilename = null;
                string SampleStyles = null;
                string SampleLine = null;
                string Copy = null;
                int NextWhiteSpace = 0;
                string SampleName = null;
                int LinePosition = 0;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string ButtonList = null;
                //
                const string ButtonFix = "Make Corrections";
                const string ButtonFixAndRun = "Make Corrections and Run Diagnostic";
                const string ButtonRun = "Run Diagnostic";
                //
                ButtonList = ButtonRun;
                //
                Stream.Add(GetTitle("Content Diagnostic", "This tool finds Content and Table problems. To run successfully, the Site Property 'TrapErrors' must be set to true."));
                Stream.Add(SpanClassAdminNormal + "<br>");
                //
                iDiagActionCount = cpCore.docProperties.getInteger("DiagActionCount");
                Button = cpCore.docProperties.getText("Button");
                if ((iDiagActionCount != 0) & ((Button == ButtonFix) || (Button == ButtonFixAndRun))) {
                    //
                    //-----------------------------------------------------------------------------------------------
                    // ----- Perform actions from previous Diagnostic
                    //-----------------------------------------------------------------------------------------------
                    //
                    Stream.Add("<br>");
                    for (DiagActionPointer = 0; DiagActionPointer <= iDiagActionCount; DiagActionPointer++) {
                        DiagAction = cpCore.docProperties.getText("DiagAction" + DiagActionPointer);
                        Stream.Add("Perform Action " + DiagActionPointer + " - " + DiagAction + "<br>");
                        switch (genericController.EncodeInteger(DiagArgument(DiagAction, 0))) {
                            case DiagActionSetFieldType:
                                //
                                // ----- Set Field Type
                                //
                                ContentID = Local_GetContentID(DiagArgument(DiagAction, 1));
                                CS = cpCore.db.csOpen("Content Fields", "(ContentID=" + ContentID + ")and(Name=" + cpCore.db.encodeSQLText(DiagArgument(DiagAction, 2)) + ")");
                                if (cpCore.db.csOk(CS)) {
                                    cpCore.db.csSet(CS, "Type", DiagArgument(DiagAction, 3));
                                }
                                cpCore.db.csClose(ref CS);
                                //end case
                                break;
                            case DiagActionSetFieldInactive:
                                //
                                // ----- Set Field Inactive
                                //
                                ContentID = Local_GetContentID(DiagArgument(DiagAction, 1));
                                CS = cpCore.db.csOpen("Content Fields", "(ContentID=" + ContentID + ")and(Name=" + cpCore.db.encodeSQLText(DiagArgument(DiagAction, 2)) + ")");
                                if (cpCore.db.csOk(CS)) {
                                    cpCore.db.csSet(CS, "active", 0);
                                }
                                cpCore.db.csClose(ref CS);
                                //end case
                                break;
                            case DiagActionDeleteRecord:
                                //
                                // ----- Delete Record
                                //
                                ContentName = DiagArgument(DiagAction, 1);
                                RecordID = genericController.EncodeInteger(DiagArgument(DiagAction, 2));
                                cpCore.db.deleteContentRecord(ContentName, RecordID);
                                //end case
                                break;
                            case DiagActionContentDeDupe:
                                ContentName = DiagArgument(DiagAction, 1);
                                CS = cpCore.db.csOpen("Content", "name=" + cpCore.db.encodeSQLText(ContentName), "ID");
                                if (cpCore.db.csOk(CS)) {
                                    cpCore.db.csGoNext(CS);
                                    while (cpCore.db.csOk(CS)) {
                                        cpCore.db.csSet(CS, "active", 0);
                                        cpCore.db.csGoNext(CS);
                                    }
                                }
                                cpCore.db.csClose(ref CS);
                                //end case
                                break;
                            case DiagActionSetRecordInactive:
                                //
                                // ----- Set Field Inactive
                                //
                                ContentName = DiagArgument(DiagAction, 1);
                                RecordID = genericController.EncodeInteger(DiagArgument(DiagAction, 2));
                                CS = cpCore.db.csOpen(ContentName, "(ID=" + RecordID + ")");
                                if (cpCore.db.csOk(CS)) {
                                    cpCore.db.csSet(CS, "active", 0);
                                }
                                cpCore.db.csClose(ref CS);
                                //end case
                                break;
                            case DiagActionSetFieldNotRequired:
                                //
                                // ----- Set Field not-required
                                //
                                ContentName = DiagArgument(DiagAction, 1);
                                RecordID = genericController.EncodeInteger(DiagArgument(DiagAction, 2));
                                CS = cpCore.db.csOpen(ContentName, "(ID=" + RecordID + ")");
                                if (cpCore.db.csOk(CS)) {
                                    cpCore.db.csSet(CS, "required", 0);
                                }
                                cpCore.db.csClose(ref CS);
                                //end case
                                break;
                        }
                    }
                }
                if ((Button == ButtonRun) || (Button == ButtonFixAndRun)) {
                    //
                    // Process input
                    //
                    if (!cpCore.siteProperties.trapErrors) {
                        //
                        // TrapErrors must be true to run this tools
                        //
                        Stream.Add("Site Property 'TrapErrors' is currently set false. This property must be true to run Content Diagnostics successfully.<br>");
                    } else {
                        cpCore.html.enableOutputBuffer(false);
                        //
                        // ----- check Content Sources for duplicates
                        //
                        if (DiagActionCount < DiagActionCountMax) {
                            Stream.Add(GetDiagHeader("Checking Content Definition Duplicates...<br>"));
                            //
                            SQL = "SELECT Count(ccContent.ID) AS RecordCount, ccContent.Name AS Name, ccContent.Active"
                                    + " From ccContent"
                                    + " GROUP BY ccContent.Name, ccContent.Active"
                                    + " Having (((Count(ccContent.ID)) > 1) And ((ccContent.active) <> 0))"
                                    + " ORDER BY Count(ccContent.ID) DESC;";
                            CSPointer = cpCore.db.csOpenSql_rev("Default", SQL);
                            if (cpCore.db.csOk(CSPointer)) {
                                while (cpCore.db.csOk(CSPointer)) {
                                    DiagProblem = "PROBLEM: There are " + cpCore.db.csGetText(CSPointer, "RecordCount") + " records in the Content table with the name [" + cpCore.db.csGetText(CSPointer, "Name") + "]";
                                    DiagActions = new  DiagActionType[3];
                                    DiagActions[0].Name = "Ignore, or handle this issue manually";
                                    DiagActions[0].Command = "";
                                    DiagActions[1].Name = "Mark all duplicate definitions inactive";
                                    DiagActions[1].Command = DiagActionContentDeDupe.ToString() + "," + cpCore.db.cs_getValue(CSPointer, "name");
                                    Stream.Add(GetDiagError(DiagProblem, DiagActions));
                                    cpCore.db.csGoNext(CSPointer);
                                }
                            }
                            cpCore.db.csClose(ref CSPointer);
                        }
                        //
                        // ----- Content Fields
                        //
                        if (DiagActionCount < DiagActionCountMax) {
                            Stream.Add(GetDiagHeader("Checking Content Fields...<br>"));
                            //
                            SQL = "SELECT ccFields.required AS FieldRequired, ccFields.Authorable AS FieldAuthorable, ccFields.Type AS FieldType, ccFields.Name AS FieldName, ccContent.ID AS ContentID, ccContent.Name AS ContentName, ccTables.Name AS TableName, ccDataSources.Name AS DataSourceName"
                                    + " FROM (ccFields LEFT JOIN ccContent ON ccFields.ContentID = ccContent.ID) LEFT JOIN (ccTables LEFT JOIN ccDataSources ON ccTables.DataSourceID = ccDataSources.ID) ON ccContent.ContentTableID = ccTables.ID"
                                    + " WHERE (((ccFields.Active)<>0) AND ((ccContent.Active)<>0) AND ((ccTables.Active)<>0)) OR (((ccFields.Active)<>0) AND ((ccContent.Active)<>0) AND ((ccTables.Active)<>0));";
                            CS = cpCore.db.csOpenSql_rev("Default", SQL);
                            if (!cpCore.db.csOk(CS)) {
                                DiagProblem = "PROBLEM: No Content entries were found in the content table.";
                                DiagActions = new DiagActionType[2];
                                DiagActions[0].Name = "Ignore, or handle this issue manually";
                                DiagActions[0].Command = "";
                                Stream.Add(GetDiagError(DiagProblem, DiagActions));
                            } else {
                                while (cpCore.db.csOk(CS) && (DiagActionCount < DiagActionCountMax)) {
                                    FieldName = cpCore.db.csGetText(CS, "FieldName");
                                    fieldType = cpCore.db.csGetInteger(CS, "FieldType");
                                    FieldRequired = cpCore.db.csGetBoolean(CS, "FieldRequired");
                                    FieldAuthorable = cpCore.db.csGetBoolean(CS, "FieldAuthorable");
                                    ContentName = cpCore.db.csGetText(CS, "ContentName");
                                    TableName = cpCore.db.csGetText(CS, "TableName");
                                    DataSourceName = cpCore.db.csGetText(CS, "DataSourceName");
                                    if (string.IsNullOrEmpty(DataSourceName)) {
                                        DataSourceName = "Default";
                                    }
                                    if (FieldRequired && (!FieldAuthorable)) {
                                        DiagProblem = "PROBLEM: Field [" + FieldName + "] in Content Definition [" + ContentName + "] is required, but is not referenced on the Admin Editing page. This will prevent content definition records from being saved.";
                                        DiagActions = new DiagActionType[2];
                                        DiagActions[0].Name = "Ignore, or handle this issue manually";
                                        DiagActions[0].Command = "";
                                        DiagActions[1].Name = "Set this Field inactive";
                                        DiagActions[1].Command = DiagActionSetFieldInactive.ToString() + "," + ContentName + "," + FieldName;
                                        DiagActions[1].Name = "Set this Field not required";
                                        DiagActions[1].Command = DiagActionSetFieldNotRequired.ToString() + "," + ContentName + "," + FieldName;
                                        Stream.Add(GetDiagError(DiagProblem, DiagActions));
                                    }
                                    if ((!string.IsNullOrEmpty(FieldName)) & (fieldType != FieldTypeIdRedirect) & (fieldType != FieldTypeIdManyToMany)) {
                                        SQL = "SELECT " + FieldName + " FROM " + TableName + " WHERE ID=0;";
                                        CSTest = cpCore.db.csOpenSql_rev(DataSourceName, SQL);
                                        if (CSTest == -1) {
                                            DiagProblem = "PROBLEM: Field [" + FieldName + "] in Content Definition [" + ContentName + "] could not be read from database table [" + TableName + "] on datasource [" + DataSourceName + "].";
                                            DiagActions = new DiagActionType[2];
                                            DiagActions[0].Name = "Ignore, or handle this issue manually";
                                            DiagActions[0].Command = "";
                                            DiagActions[1].Name = "Set this Content Definition Field inactive";
                                            DiagActions[1].Command = DiagActionSetFieldInactive.ToString() + "," + ContentName + "," + FieldName;
                                            Stream.Add(GetDiagError(DiagProblem, DiagActions));
                                        }
                                    }
                                    cpCore.db.csClose(ref CSTest);
                                    cpCore.db.csGoNext(CS);
                                }
                            }
                            cpCore.db.csClose(ref CS);
                        }
                        //
                        // ----- Insert Content Testing
                        //
                        if (DiagActionCount < DiagActionCountMax) {
                            Stream.Add(GetDiagHeader("Checking Content Insertion...<br>"));
                            //
                            CSContent = cpCore.db.csOpen("Content");
                            if (!cpCore.db.csOk(CSContent)) {
                                DiagProblem = "PROBLEM: No Content entries were found in the content table.";
                                DiagActions = new DiagActionType[2];
                                DiagActions[0].Name = "Ignore, or handle this issue manually";
                                DiagActions[0].Command = "";
                                Stream.Add(GetDiagError(DiagProblem, DiagActions));
                            } else {
                                while (cpCore.db.csOk(CSContent) && (DiagActionCount < DiagActionCountMax)) {
                                    ContentID = cpCore.db.csGetInteger(CSContent, "ID");
                                    ContentName = cpCore.db.csGetText(CSContent, "name");
                                    CSTestRecord = cpCore.db.csInsertRecord(ContentName);
                                    if (!cpCore.db.csOk(CSTestRecord)) {
                                        DiagProblem = "PROBLEM: Could not insert a record using Content Definition [" + ContentName + "]";
                                        DiagActions = new DiagActionType[2];
                                        DiagActions[0].Name = "Ignore, or handle this issue manually";
                                        DiagActions[0].Command = "";
                                        Stream.Add(GetDiagError(DiagProblem, DiagActions));
                                    } else {
                                        TestRecordID = cpCore.db.csGetInteger(CSTestRecord, "id");
                                        if (TestRecordID == 0) {
                                            DiagProblem = "PROBLEM: Content Definition [" + ContentName + "] does not support the required field [ID]\"";
                                            DiagActions = new DiagActionType[2];
                                            DiagActions[0].Name = "Ignore, or handle this issue manually";
                                            DiagActions[0].Command = "";
                                            DiagActions[1].Name = "Set this Content Definition inactive";
                                            DiagActions[1].Command = DiagActionSetRecordInactive.ToString() + ",Content," + ContentID;
                                            Stream.Add(GetDiagError(DiagProblem, DiagActions));
                                        } else {
                                            CSFields = cpCore.db.csOpen("Content Fields", "ContentID=" + ContentID);
                                            while (cpCore.db.csOk(CSFields)) {
                                                //
                                                // ----- read the value of the field to test its presents
                                                //
                                                FieldName = cpCore.db.csGetText(CSFields, "name");
                                                fieldType = cpCore.db.csGetInteger(CSFields, "Type");
                                                switch (fieldType) {
                                                    case FieldTypeIdManyToMany:
                                                        //
                                                        //   skip it
                                                        //
                                                        break;
                                                    case FieldTypeIdRedirect:
                                                        //
                                                        // ----- redirect type, check redirect contentid
                                                        //
                                                        RedirectContentID = cpCore.db.csGetInteger(CSFields, "RedirectContentID");
                                                        ErrorCount = cpCore.doc.errorCount;
                                                        bitBucket = Local_GetContentNameByID(RedirectContentID);
                                                        if (IsNull(bitBucket) | (ErrorCount != cpCore.doc.errorCount)) {
                                                            DiagProblem = "PROBLEM: Content Field [" + ContentName + "].[" + FieldName + "] is a Redirection type, but the ContentID [" + RedirectContentID + "] is not valid.";
                                                            if (string.IsNullOrEmpty(FieldName)) {
                                                                DiagProblem = DiagProblem + " Also, the field has no name attribute so these diagnostics can not automatically mark the field inactive.";
                                                                DiagActions = new DiagActionType[2];
                                                                DiagActions[0].Name = "Ignore, or handle this issue manually";
                                                                DiagActions[0].Command = "";
                                                                Stream.Add(GetDiagError(DiagProblem, DiagActions));
                                                            } else {
                                                                DiagActions = new DiagActionType[3];
                                                                DiagActions[0].Name = "Ignore, or handle this issue manually";
                                                                DiagActions[0].Command = "";
                                                                DiagActions[1].Name = "Set this Content Definition Field inactive";
                                                                DiagActions[1].Command = DiagActionSetFieldInactive.ToString() + "," + ContentName + "," + FieldName;
                                                                Stream.Add(GetDiagError(DiagProblem, DiagActions));
                                                            }
                                                        }
                                                        break;
                                                    case FieldTypeIdLookup:
                                                        //
                                                        // ----- lookup type, read value and check lookup contentid
                                                        //
                                                        ErrorCount = cpCore.doc.errorCount;
                                                        bitBucket = cpCore.db.cs_getValue(CSTestRecord, FieldName);
                                                        if (ErrorCount != cpCore.doc.errorCount) {
                                                            DiagProblem = "PROBLEM: An error occurred reading the value of Content Field [" + ContentName + "].[" + FieldName + "]";
                                                            DiagActions = new DiagActionType[2];
                                                            DiagActions[0].Name = "Ignore, or handle this issue manually";
                                                            DiagActions[0].Command = "";
                                                            Stream.Add(GetDiagError(DiagProblem, DiagActions));
                                                        } else {
                                                            bitBucket = "";
                                                            LookupList = cpCore.db.csGetText(CSFields, "Lookuplist");
                                                            LookupContentID = cpCore.db.csGetInteger(CSFields, "LookupContentID");
                                                            if (LookupContentID != 0) {
                                                                ErrorCount = cpCore.doc.errorCount;
                                                                bitBucket = Local_GetContentNameByID(LookupContentID);
                                                            }
                                                            if ((string.IsNullOrEmpty(LookupList)) && ((LookupContentID == 0) || (string.IsNullOrEmpty(bitBucket)) || (ErrorCount != cpCore.doc.errorCount))) {
                                                                DiagProblem = "Content Field [" + ContentName + "].[" + FieldName + "] is a Lookup type, but LookupList is blank and LookupContentID [" + LookupContentID + "] is not valid.";
                                                                DiagActions = new DiagActionType[3];
                                                                DiagActions[0].Name = "Ignore, or handle this issue manually";
                                                                DiagActions[0].Command = "";
                                                                DiagActions[1].Name = "Convert the field to an Integer so no lookup is provided.";
                                                                DiagActions[1].Command = DiagActionSetFieldType.ToString() + "," + ContentName + "," + FieldName + "," + encodeText(FieldTypeIdInteger);
                                                                Stream.Add(GetDiagError(DiagProblem, DiagActions));
                                                            }
                                                        }
                                                        break;
                                                    default:
                                                        //
                                                        // ----- check for value in database
                                                        //
                                                        ErrorCount = cpCore.doc.errorCount;
                                                        bitBucket = cpCore.db.cs_getValue(CSTestRecord, FieldName);
                                                        if (ErrorCount != cpCore.doc.errorCount) {
                                                            DiagProblem = "PROBLEM: An error occurred reading the value of Content Field [" + ContentName + "].[" + FieldName + "]";
                                                            DiagActions = new DiagActionType[4];
                                                            DiagActions[0].Name = "Ignore, or handle this issue manually";
                                                            DiagActions[0].Command = "";
                                                            DiagActions[1].Name = "Add this field into the Content Definitions Content (and Authoring) Table.";
                                                            DiagActions[1].Command = "x";
                                                            Stream.Add(GetDiagError(DiagProblem, DiagActions));
                                                        }
                                                        break;
                                                }
                                                cpCore.db.csGoNext(CSFields);
                                            }
                                        }
                                        cpCore.db.csClose(ref CSFields);
                                        cpCore.db.csClose(ref CSTestRecord);
                                        cpCore.db.deleteContentRecord(ContentName, TestRecordID);
                                    }
                                    cpCore.db.csGoNext(CSContent);
                                }
                            }
                            cpCore.db.csClose(ref CSContent);
                        }
                        //
                        // ----- Check Navigator Entries
                        //
                        if (DiagActionCount < DiagActionCountMax) {
                            Stream.Add(GetDiagHeader("Checking Navigator Entries...<br>"));
                            CSPointer = cpCore.db.csOpen(cnNavigatorEntries);
                            if (!cpCore.db.csOk(CSPointer)) {
                                DiagProblem = "PROBLEM: Could not open the [Navigator Entries] content.";
                                DiagActions = new DiagActionType[4];
                                DiagActions[0].Name = "Ignore, or handle this issue manually";
                                DiagActions[0].Command = "";
                                Stream.Add(GetDiagError(DiagProblem, DiagActions));
                            } else {
                                while (cpCore.db.csOk(CSPointer) && (DiagActionCount < DiagActionCountMax)) {
                                    ContentID = cpCore.db.csGetInteger(CSPointer, "ContentID");
                                    if (ContentID != 0) {
                                        CSContent = cpCore.db.csOpen("Content", "ID=" + ContentID);
                                        if (!cpCore.db.csOk(CSContent)) {
                                            DiagProblem = "PROBLEM: Menu Entry [" + cpCore.db.csGetText(CSPointer, "name") + "] points to an invalid Content Definition.";
                                            DiagActions = new DiagActionType[4];
                                            DiagActions[0].Name = "Ignore, or handle this issue manually";
                                            DiagActions[0].Command = "";
                                            DiagActions[1].Name = "Remove this menu entry";
                                            DiagActions[1].Command = DiagActionDeleteRecord.ToString() + ",Navigator Entries," + cpCore.db.csGetInteger(CSPointer, "ID");
                                            Stream.Add(GetDiagError(DiagProblem, DiagActions));
                                        }
                                        cpCore.db.csClose(ref CSContent);
                                    }
                                    cpCore.db.csGoNext(CSPointer);
                                }
                            }
                            cpCore.db.csClose(ref CSPointer);
                        }
                        if (DiagActionCount >= DiagActionCountMax) {
                            DiagProblem = "Diagnostic Problem Limit (" + DiagActionCountMax + ") has been reached. Resolve the above issues to see more.";
                            DiagActions = new DiagActionType[2];
                            DiagActions[0].Name = "";
                            DiagActions[0].Command = "";
                            Stream.Add(GetDiagError(DiagProblem, DiagActions));
                        }
                        //
                        // ----- Done with diagnostics
                        //
                        Stream.Add(cpCore.html.html_GetFormInputHidden("DiagActionCount", DiagActionCount));
                    }
                }
                //
                // start diagnostic button
                //
                Stream.Add("</SPAN>");
                if (DiagActionCount > 0) {
                    ButtonList = ButtonList + "," + ButtonFix;
                    ButtonList = ButtonList + "," + ButtonFixAndRun;
                }
                result = htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
                int CSPointer = 0;
                int ColumnWidth = 0;
                int ColumnWidthTotal = 0;
                int ColumnCounter = 0;
                int IndexColumn = 0;
                //
                //cpCore.main_'TestPointEnter ("NormalizeIndexColumns()")
                //
                //Call LoadContentDefinitions
                //
                CSPointer = cpCore.db.csOpen("Content Fields", "(ContentID=" + ContentID + ")", "IndexColumn");
                if (!cpCore.db.csOk(CSPointer)) {
                    throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors2("NormalizeIndexColumns", "Could not read Content Field Definitions")
                } else {
                    //
                    // Adjust IndexSortOrder to be 0 based, count by 1
                    //
                    ColumnCounter = 0;
                    while (cpCore.db.csOk(CSPointer)) {
                        IndexColumn = cpCore.db.csGetInteger(CSPointer, "IndexColumn");
                        ColumnWidth = cpCore.db.csGetInteger(CSPointer, "IndexWidth");
                        if ((IndexColumn == 0) || (ColumnWidth == 0)) {
                            cpCore.db.csSet(CSPointer, "IndexColumn", 0);
                            cpCore.db.csSet(CSPointer, "IndexWidth", 0);
                            cpCore.db.csSet(CSPointer, "IndexSortPriority", 0);
                        } else {
                            //
                            // Column appears in Index, clean it up
                            //
                            cpCore.db.csSet(CSPointer, "IndexColumn", ColumnCounter);
                            ColumnCounter = ColumnCounter + 1;
                            ColumnWidthTotal = ColumnWidthTotal + ColumnWidth;
                        }
                        cpCore.db.csGoNext(CSPointer);
                    }
                    if (ColumnCounter == 0) {
                        //
                        // No columns found, set name as Column 0, active as column 1
                        //
                        cpCore.db.cs_goFirst(CSPointer);
                        while (cpCore.db.csOk(CSPointer)) {
                            switch (genericController.vbUCase(cpCore.db.csGetText(CSPointer, "name"))) {
                                case "ACTIVE":
                                    cpCore.db.csSet(CSPointer, "IndexColumn", 0);
                                    cpCore.db.csSet(CSPointer, "IndexWidth", 20);
                                    ColumnWidthTotal = ColumnWidthTotal + 20;
                                    break;
                                case "NAME":
                                    cpCore.db.csSet(CSPointer, "IndexColumn", 1);
                                    cpCore.db.csSet(CSPointer, "IndexWidth", 80);
                                    ColumnWidthTotal = ColumnWidthTotal + 80;
                                    break;
                            }
                            cpCore.db.csGoNext(CSPointer);
                        }
                    }
                    //
                    // ----- Now go back and set a normalized Width value
                    //
                    if (ColumnWidthTotal > 0) {
                        cpCore.db.cs_goFirst(CSPointer);
                        while (cpCore.db.csOk(CSPointer)) {
                            ColumnWidth = cpCore.db.csGetInteger(CSPointer, "IndexWidth");
                            ColumnWidth = EncodeInteger((ColumnWidth * 100) / (double)ColumnWidthTotal);
                            cpCore.db.csSet(CSPointer, "IndexWidth", ColumnWidth);
                            cpCore.db.csGoNext(CSPointer);
                        }
                    }
                }
                cpCore.db.csClose(ref CSPointer);
                //
                // ----- now fixup Sort Priority so only visible fields are sorted.
                //
                CSPointer = cpCore.db.csOpen("Content Fields", "(ContentID=" + ContentID + ")", "IndexSortPriority, IndexColumn");
                if (!cpCore.db.csOk(CSPointer)) {
                    throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors2("NormalizeIndexColumns", "Error reading Content Field Definitions")
                } else {
                    //
                    // Go through all fields, clear Sort Priority if it does not appear
                    //
                    int SortValue = 0;
                    int SortDirection = 0;
                    SortValue = 0;
                    while (cpCore.db.csOk(CSPointer)) {
                        SortDirection = 0;
                        if (cpCore.db.csGetInteger(CSPointer, "IndexColumn") == 0) {
                            cpCore.db.csSet(CSPointer, "IndexSortPriority", 0);
                        } else {
                            cpCore.db.csSet(CSPointer, "IndexSortPriority", SortValue);
                            SortDirection = cpCore.db.csGetInteger(CSPointer, "IndexSortDirection");
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
                        cpCore.db.csSet(CSPointer, "IndexSortDirection", SortDirection);
                        cpCore.db.csGoNext(CSPointer);
                    }
                }
                //
                //cpCore.main_'TestPointExit
                //
                return;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
                string ParentContentName = null;
                string ChildContentName = "";
                bool AddAdminMenuEntry = false;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string ButtonList = ButtonCancel + "," + ButtonRun;
                //
                Stream.Add(GetTitle("Create a Child Content from a Content Definition", "This tool creates a Content Definition based on another Content Definition."));
                //
                //   print out the submit form
                if (cpCore.docProperties.getText("Button") != "") {
                    //
                    // Process input
                    //
                    ParentContentID = cpCore.docProperties.getInteger("ParentContentID");
                    ParentContentName = Local_GetContentNameByID(ParentContentID);
                    ChildContentName = cpCore.docProperties.getText("ChildContentName");
                    AddAdminMenuEntry = cpCore.docProperties.getBoolean("AddAdminMenuEntry");
                    //
                    Stream.Add(SpanClassAdminSmall);
                    if ((string.IsNullOrEmpty(ParentContentName)) || (string.IsNullOrEmpty(ChildContentName))) {
                        Stream.Add("<p>You must select a parent and provide a child name.</p>");
                    } else {
                        //
                        // Create Definition
                        //
                        Stream.Add("<P>Creating content [" + ChildContentName + "] from [" + ParentContentName + "]");
                        Models.Complex.cdefModel.createContentChild(cpCore, ChildContentName, ParentContentName, cpCore.doc.authContext.user.id);
                        //
                        Stream.Add("<br>Reloading Content Definitions...");
                        cpCore.cache.invalidateAll();
                        cpCore.doc.clearMetaData();
                        //
                        // Add Admin Menu Entry
                        //
                        //If AddAdminMenuEntry Then
                        //    Stream.Add("<br>Adding menu entry (will not display until the next page)...")
                        //    CS = cpCore.db.cs_open(cnNavigatorEntries, "ContentID=" & ParentContentID)
                        //    If cpCore.db.cs_ok(CS) Then
                        //        MenuName = cpCore.db.cs_getText(CS, "name")
                        //        AdminOnly = cpCore.db.cs_getBoolean(CS, "AdminOnly")
                        //        DeveloperOnly = cpCore.db.cs_getBoolean(CS, "DeveloperOnly")
                        //    End If
                        //    Call cpCore.db.cs_Close(CS)
                        //    If MenuName <> "" Then
                        //        Call Controllers.appBuilderController.admin_VerifyAdminMenu(cpCore, MenuName, ChildContentName, ChildContentName, "", ChildContentName, AdminOnly, DeveloperOnly, False)
                        //    Else
                        //        Call Controllers.appBuilderController.admin_VerifyAdminMenu(cpCore, "Site Content", ChildContentName, ChildContentName, "", "")
                        //    End If
                        //End If
                        Stream.Add("<br>Finished</P>");
                    }
                    Stream.Add("</SPAN>");
                }
                Stream.Add(SpanClassAdminNormal);
                //
                Stream.Add("Parent Content Name<br>");
                Stream.Add(cpCore.html.main_GetFormInputSelect("ParentContentID", ParentContentID, "Content", ""));
                Stream.Add("<br><br>");
                //
                Stream.Add("Child Content Name<br>");
                Stream.Add(cpCore.html.html_GetFormInputText2("ChildContentName", ChildContentName, 1, 40));
                Stream.Add("<br><br>");
                //
                Stream.Add("Add Admin Menu Entry under Parent's Menu Entry<br>");
                Stream.Add(cpCore.html.html_GetFormInputCheckBox2("AddAdminMenuEntry", AddAdminMenuEntry));
                Stream.Add("<br><br>");
                //
                //Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolCreateChildContent)
                Stream.Add("</SPAN>");
                //
                result = htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string ButtonList;
                //
                ButtonList = ButtonCancel + ",Clear Content Watch Links";
                Stream.Add(SpanClassAdminNormal);
                Stream.Add(GetTitle("Clear ContentWatch Links", "This tools nulls the Links field of all Content Watch records. After running this tool, run the diagnostic spider to repopulate the links."));
                //
                if (cpCore.docProperties.getText("Button") != "") {
                    //
                    // Process input
                    //
                    Stream.Add("<br>");
                    Stream.Add("<br>Clearing Content Watch Link field...");
                    cpCore.db.executeQuery("update ccContentWatch set Link=null;");
                    Stream.Add("<br>Content Watch Link field cleared.");
                }
                Stream.Add("</span>");
                result = htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
                int CSContent = 0;
                Models.Complex.cdefModel CD = null;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string[,] ContentNameArray = null;
                int ContentNameCount = 0;
                string TableName = null;
                string ButtonList;
                //
                ButtonList = ButtonCancel + "," + ButtonRun;
                //
                Stream.Add(GetTitle("Synchronize Tables to Content Definitions", "This tools goes through all Content Definitions and creates any necessary Tables and Table Fields to support the Definition."));
                //
                if (cpCore.docProperties.getText("Button") != "") {
                    //
                    //   Run Tools
                    //
                    Stream.Add("Synchronizing Tables to Content Definitions<br>");
                    CSContent = cpCore.db.csOpen("Content", "", "", false, 0, false, false, "id");
                    if (cpCore.db.csOk(CSContent)) {
                        do {
                            CD = Models.Complex.cdefModel.getCdef(cpCore, cpCore.db.csGetInteger(CSContent, "id"));
                            TableName = CD.ContentTableName;
                            Stream.Add("Synchronizing Content " + CD.Name + " to table " + TableName + "<br>");
                            cpCore.db.createSQLTable(CD.ContentDataSourceName, TableName);
                            if (CD.fields.Count > 0) {
                                foreach (var keyValuePair in CD.fields) {
                                    Models.Complex.CDefFieldModel field = keyValuePair.Value;
                                    Stream.Add("...Field " + field.nameLc + "<br>");
                                    cpCore.db.createSQLTableField(CD.ContentDataSourceName, TableName, field.nameLc, field.fieldTypeId);
                                }
                            }
                            cpCore.db.csGoNext(CSContent);
                        } while (cpCore.db.csOk(CSContent));
                        ContentNameArray = cpCore.db.cs_getRows(CSContent);
                        ContentNameCount = ContentNameArray.GetUpperBound(1) + 1;
                    }
                    cpCore.db.csClose(ref CSContent);
                }
                //
                returnValue = htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnValue;
        }
        //
        //
        //
        private string AddDiagError(string ProblemMsg, DiagActionType[] DiagActions) {
            return GetDiagError(ProblemMsg, DiagActions);
        }
        //
        //
        //
        private string GetDiagError(string ProblemMsg, DiagActionType[] DiagActions) {
            string result = "";
            try {
                int ActionCount = 0;
                int ActionPointer = 0;
                string Caption = null;
                string Panel = "";
                //
                Panel = Panel + "<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" width=\"100%\">";
                Panel = Panel + "<tr><td colspan=\"2\">" + SpanClassAdminNormal + "<b>" + ProblemMsg + "</b></SPAN></td></tr>";
                ActionCount = DiagActions.GetUpperBound(0);
                if (ActionCount > 0) {
                    for (ActionPointer = 0; ActionPointer <= ActionCount; ActionPointer++) {
                        Caption = DiagActions[ActionPointer].Name;
                        if (!string.IsNullOrEmpty(Caption)) {
                            Panel = Panel + "<tr>";
                            Panel = Panel + "<td width=\"30\" align=\"right\">";
                            Panel = Panel + cpCore.html.html_GetFormInputRadioBox("DiagAction" + DiagActionCount, DiagActions[ActionPointer].Command, "");
                            Panel = Panel + "</td>";
                            Panel = Panel + "<td width=\"100%\">" + SpanClassAdminNormal + Caption + "</SPAN></td>";
                            Panel = Panel + "</tr>";
                        }
                    }
                }
                Panel = Panel + "</TABLE>";
                DiagActionCount = DiagActionCount + 1;
                result =  cpCore.html.getPanel(Panel);
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //
        //
        private string GetDiagHeader(string Copy) {
            return cpCore.html.getPanel("<B>" + SpanClassAdminNormal + Copy + "</SPAN><B>");
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
                CommandStartPosition = genericController.vbInstr(CommandStartPosition, CommandList, ",");
                if (CommandStartPosition == 0) {
                    EndOfList = true;
                }
                CommandStartPosition = CommandStartPosition + 1;
                CommandCount = CommandCount + 1;
            }
            if (!EndOfList) {
                CommandEndPosition = genericController.vbInstr(CommandStartPosition, CommandList, ",");
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
                int Ticks = 0;
                int PageSize = 0;
                int PageNumber = 0;
                int RecordCount = 0;
                string SQL = null;
                int CS = 0;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string ButtonList;
                //
                ButtonList = ButtonCancel + "," + ButtonRun;
                //
                Stream.Add(GetTitle("Benchmark", "Run a series of data operations and compare the results to previous known values."));
                //
                if (cpCore.docProperties.getText("Button") != "") {
                    //
                    //   Run Tools
                    //
                    Stream.Add("<br>");
                    //
                    Stream.Add(SpanClassAdminNormal);
                    TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
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
                    //    TestCopy = genericController.encodeText(cpCore.csGetFieldCount(1))
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
                        TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
                        RS = cpCore.db.executeQuery(SQL);
                        OpenTicks = OpenTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                        RecordCount = 0;
                        TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
                        foreach (DataRow dr in RS.Rows) {
                            NextTicks = NextTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                            //
                            TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
                            TestCopy = genericController.encodeText(dr["NAME"]);
                            ReadTicks = ReadTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                            //
                            RecordCount = RecordCount + 1;
                            TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
                        }
                        NextTicks = NextTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
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
                        TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
                        CS = cpCore.db.csOpen("Site Properties", "","",true,0,true,false,"", PageSize, PageNumber);
                        OpenTicks = OpenTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                        //
                        RecordCount = 0;
                        while (cpCore.db.csOk(CS)) {
                            //
                            TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
                            TestCopy = genericController.encodeText(cpCore.db.cs_getValue(CS, "Name"));
                            ReadTicks = ReadTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                            //
                            TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
                            cpCore.db.csGoNext(CS);
                            NextTicks = NextTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                            //
                            RecordCount = RecordCount + 1;
                        }
                        cpCore.db.csClose(ref CS);
                        ReadTicks = ReadTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
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
                        TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
                        CS = cpCore.db.csOpen("Site Properties", "", "", true, 0, false, false, "name", PageSize, PageNumber);
                        OpenTicks = OpenTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                        //
                        RecordCount = 0;
                        while (cpCore.db.csOk(CS)) {
                            //
                            TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
                            TestCopy = genericController.encodeText(cpCore.db.cs_getValue(CS, "Name"));
                            ReadTicks = ReadTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                            //
                            TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
                            cpCore.db.csGoNext(CS);
                            NextTicks = NextTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
                            //
                            RecordCount = RecordCount + 1;
                        }
                        cpCore.db.csClose(ref CS);
                        ReadTicks = ReadTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
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
                result =  htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
                DataTable dt = cpCore.db.executeQuery("Select ID from ccContent where name=" + cpCore.db.encodeSQLText(ContentName));
                if (dt.Rows.Count > 0) {
                    tempLocal_GetContentID = genericController.EncodeInteger(dt.Rows[0][0]);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
                dt = cpCore.db.executeQuery("Select name from ccContent where id=" + ContentID);
                if (dt.Rows.Count > 0) {
                    tempLocal_GetContentNameByID = genericController.encodeText(dt.Rows[0][0]);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
                RS = cpCore.db.executeQuery("Select ccTables.Name as TableName from ccContent Left Join ccTables on ccContent.ContentTableID=ccTables.ID where ccContent.name=" + cpCore.db.encodeSQLText(ContentName));
                if (RS.Rows.Count > 0) {
                    tempLocal_GetContentTableName = genericController.encodeText(RS.Rows[0][0]);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return tempLocal_GetContentTableName;
        }
        //
        //=============================================================================
        //   Get a ContentID from the ContentName using just the tables
        //=============================================================================
        //
        private string Local_GetContentDataSource(string ContentName) {
            string tempLocal_GetContentDataSource = null;
            try {
                tempLocal_GetContentDataSource = "";
                string SQL = ""
                    +"Select ccDataSources.Name"
                    + " from ( ccContent Left Join ccTables on ccContent.ContentTableID=ccTables.ID )"
                    + " Left Join ccDataSources on ccTables.DataSourceID=ccDataSources.ID"
                    + " where ccContent.name=" + cpCore.db.encodeSQLText(ContentName);
                DataTable RS = cpCore.db.executeQuery(SQL);
                if (isDataTableOk(RS)) {
                    tempLocal_GetContentDataSource = genericController.encodeText(RS.Rows[0]["Name"]);
                }
                if (string.IsNullOrEmpty(tempLocal_GetContentDataSource)) {
                    tempLocal_GetContentDataSource = "Default";
                }
                closeDataTable(RS);
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return tempLocal_GetContentDataSource;
        }
        //
        //=============================================================================
        //   Print the manual query form
        //=============================================================================
        //
        private string GetForm_DbSchema() {
            string result = "";
            try {
                //
                string SQL = null;
                int ErrorNumber = 0;
                string ErrorDescription = "";
                bool StatusOK = false;
                int FieldCount = 0;
                string CellString = null;
                string SQLFilename = null;
                string SQLArchive = null;
                string SQLArchiveOld = null;
                int LineCounter = 0;
                string SQLLine = null;
                object Retries = null;
                int PageSize = 0;
                int PageNumber = 0;
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
                int SelectFieldWidthLimit = 0;
                string SQLName = null;
                string TableName = "";
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string ButtonList = null;
                DataTable RSSchema = null;
                var tmpList = new List<string> { };
                dataSourceModel datasource = dataSourceModel.create(cpCore, cpCore.docProperties.getInteger("DataSourceID"),ref tmpList);
                //
                ButtonList = ButtonCancel + "," + ButtonRun;
                //
                Stream.Add(GetTitle("Query Database Schema", "This tool examines the database schema for all tables available."));
                //
                StatusOK = true;
                if ((cpCore.docProperties.getText("button")) != ButtonRun) {
                    //
                    // First pass, initialize
                    //
                    PageSize = 10;
                    PageNumber = 1;
                    Retries = 0;
                } else {
                    //
                    // Read in arguments
                    //
                    TableName = cpCore.docProperties.getText("TableName");
                    //
                    // Run the SQL
                    //
                    //ConnectionString = cpCore.db.getmain_GetConnectionString(DataSourceName)
                    //
                    Stream.Add(SpanClassAdminSmall + "<br><br>");
                    Stream.Add(DateTime.Now + " Opening Table Schema on DataSource [" + datasource.Name + "]<br>");
                    //
                    RSSchema = cpCore.db.getTableSchemaData(TableName);
                    if (true) {
                        Stream.Add(DateTime.Now + " GetSchema executed successfully<br>");
                    } else {
                        //
                        // ----- error
                        //
                        Stream.Add(DateTime.Now + " SQL execution returned the following error<br>");
                        Stream.Add("Error Number " + ErrorNumber + "<br>");
                        Stream.Add("Error Descrition " + ErrorDescription + "<br>");
                    }
                    if (!isDataTableOk(RSSchema)) {
                        //
                        // ----- no result
                        //
                        Stream.Add(DateTime.Now + " A schema was returned, but it contains no records.<br>");
                    } else {
                        //
                        // ----- print results
                        //
                        PageSize = 9999;
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
                        arrayOfSchema = cpCore.db.convertDataTabletoArray(RSSchema);
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
                    RSSchema = cpCore.db.getIndexSchemaData(TableName);
                    if (!isDataTableOk(RSSchema)) {
                        //
                        // ----- no result
                        //
                        Stream.Add(DateTime.Now + " A schema was returned, but it contains no records.<br>");
                    } else {
                        //
                        // ----- print results
                        //
                        PageSize = 999;
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

                        arrayOfSchema = cpCore.db.convertDataTabletoArray(RSSchema);
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
                    RSSchema = cpCore.db.getColumnSchemaData(TableName);
                    Stream.Add(DateTime.Now + " GetSchema executed successfully<br>");
                    if (isDataTableOk(RSSchema)) {
                        //
                        // ----- no result
                        //
                        Stream.Add(DateTime.Now + " A schema was returned, but it contains no records.<br>");
                    } else {
                        //
                        // ----- print results
                        //
                        PageSize = 9999;
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
                        arrayOfSchema = cpCore.db.convertDataTabletoArray(RSSchema);
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
                Stream.Add(cpCore.html.html_GetFormInputText("Tablename", TableName));
                //
                Stream.Add("<br><br>");
                Stream.Add("Data Source<br>");
                Stream.Add(cpCore.html.main_GetFormInputSelect("DataSourceID", datasource.ID, "Data Sources", "", "Default"));
                //
                //Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolSchema)
                Stream.Add("</SPAN>");
                //
                result = htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   Get the Configure Edit
        //=============================================================================
        //
        private string GetForm_ConfigureEdit(BaseClasses.CPBaseClass cp) {
            string result = "";
            try {
                //
                string SQL = null;
                string DataSourceName = "";
                string ToolButton = null;
                int ContentID = 0;
                int RecordCount = 0;
                int RecordPointer = 0;
                int CSPointer = 0;
                int formFieldId = 0;
                string ContentName = "";
                Models.Complex.cdefModel CDef = null;
                string formFieldName = null;
                int formFieldTypeId = 0;
                string TableName = "";
                int ContentFieldsCID = 0;
                string StatusMessage = "";
                string ErrorMessage = "";
                bool formFieldInherited = false;
                bool AllowContentAutoLoad = false;
                bool ReloadCDef = false;
                int CSTarget = 0;
                int CSSource = 0;
                bool AllowCDefInherit = false;
                int ParentContentID = 0;
                string ParentContentName = null;
                Models.Complex.cdefModel ParentCDef = null;
                bool NeedFootNote1 = false;
                bool NeedFootNote2 = false;
                string FormPanel = "";
                keyPtrController Index = new keyPtrController();
                string ButtonList = null;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                stringBuilderLegacyController StreamValidRows = new stringBuilderLegacyController();
                string TypeSelectTemplate = null;
                string TypeSelect = null;
                int FieldCount = 0;
                Models.Complex.CDefFieldModel parentField = null;
                //
                ButtonList = ButtonCancel + "," + ButtonSelect;
                //
                ToolButton = cp.Doc.GetText("Button");
                ReloadCDef = cp.Doc.GetBoolean("ReloadCDef");
                ContentID = cp.Doc.GetInteger("" + RequestNameToolContentID + "");
                if (ContentID > 0) {
                    ContentName = cp.Content.GetRecordName("content", ContentID);
                    if (!string.IsNullOrEmpty(ContentName)) {
                        TableName = cp.Content.GetTable(ContentName);
                        DataSourceName = cp.Content.GetDataSource(ContentName);
                        CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentID, true, true);
                    }
                }
                if (CDef != null) {
                    //
                    if (!string.IsNullOrEmpty(ToolButton)) {
                        if (ToolButton != ButtonCancel) {
                            //
                            // Save the form changes
                            //
                            AllowContentAutoLoad = cp.Site.GetBoolean("AllowContentAutoLoad", "true");
                            cp.Site.SetProperty("AllowContentAutoLoad", "false");
                            //
                            // ----- Save the input
                            //
                            RecordCount = genericController.EncodeInteger(cp.Doc.GetInteger("dtfaRecordCount"));
                            if (RecordCount > 0) {
                                for (RecordPointer = 0; RecordPointer < RecordCount; RecordPointer++) {
                                    //
                                    formFieldName = cp.Doc.GetText("dtfaName." + RecordPointer);
                                    formFieldTypeId = cp.Doc.GetInteger("dtfaType." + RecordPointer);
                                    formFieldId = genericController.EncodeInteger(cp.Doc.GetInteger("dtfaID." + RecordPointer));
                                    formFieldInherited = cp.Doc.GetBoolean("dtfaInherited." + RecordPointer);
                                    //
                                    // problem - looking for the name in the Db using the form's name, but it could have changed.
                                    // have to look field up by id
                                    //
                                    foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> cdefFieldKvp in CDef.fields) {
                                        if (cdefFieldKvp.Value.id == formFieldId) {
                                            //
                                            // Field was found in CDef
                                            //
                                            if (cdefFieldKvp.Value.inherited && (!formFieldInherited)) {
                                                //
                                                // Was inherited, but make a copy of the field
                                                //
                                                CSTarget = cpCore.db.csInsertRecord("Content Fields");
                                                if (cpCore.db.csOk(CSTarget)) {
                                                    CSSource = cpCore.db.cs_openContentRecord("Content Fields", formFieldId);
                                                    if (cpCore.db.csOk(CSSource)) {
                                                        cpCore.db.csCopyRecord(CSSource, CSTarget);
                                                    }
                                                    cpCore.db.csClose(ref CSSource);
                                                    formFieldId = cpCore.db.csGetInteger(CSTarget, "ID");
                                                    cpCore.db.csSet(CSTarget, "ContentID", ContentID);
                                                }
                                                cpCore.db.csClose(ref CSTarget);
                                                ReloadCDef = true;
                                            } else if ((!cdefFieldKvp.Value.inherited) && (formFieldInherited)) {
                                                //
                                                // Was a field, make it inherit from it's parent
                                                //
                                                //CSTarget = CSTarget;
                                                cpCore.db.deleteContentRecord("Content Fields", formFieldId);
                                                ReloadCDef = true;
                                            } else if ((!cdefFieldKvp.Value.inherited) && (!formFieldInherited)) {
                                                //
                                                // not inherited, save the field values and mark for a reload
                                                //
                                                if (true) {
                                                    if (formFieldName.IndexOf(" ")  != -1) {
                                                        //
                                                        // remoave spaces from new name
                                                        //
                                                        StatusMessage = StatusMessage + "<LI>Field [" + formFieldName + "] was renamed [" + genericController.vbReplace(formFieldName, " ", "") + "] because the field name can not include spaces.</LI>";
                                                        formFieldName = genericController.vbReplace(formFieldName, " ", "");
                                                    }
                                                    //
                                                    if ((!string.IsNullOrEmpty(formFieldName)) & (formFieldTypeId != 0) & ((cdefFieldKvp.Value.nameLc == "") || (cdefFieldKvp.Value.fieldTypeId == 0))) {
                                                        //
                                                        // Create Db field, Field is good but was not before
                                                        //
                                                        cpCore.db.createSQLTableField(DataSourceName, TableName, formFieldName, formFieldTypeId);
                                                        StatusMessage = StatusMessage + "<LI>Field [" + formFieldName + "] was saved to this content definition and a database field was created in [" + CDef.ContentTableName + "].</LI>";
                                                    } else if ((string.IsNullOrEmpty(formFieldName)) || (formFieldTypeId == 0)) {
                                                        //
                                                        // name blank or type=0 - do nothing but tell them
                                                        //
                                                        if (string.IsNullOrEmpty(formFieldName) && formFieldTypeId == 0) {
                                                            ErrorMessage += "<LI>Field number " + (RecordPointer + 1) + " was saved to this content definition but no database field was created because a name and field type are required.</LI>";
                                                        } else if (formFieldName == "unnamedfield" + legacyToolsClass.fieldId.ToString()) {
                                                            ErrorMessage += "<LI>Field number " + (RecordPointer + 1) + " was saved to this content definition but no database field was created because a field name is required.</LI>";
                                                        } else {
                                                            ErrorMessage += "<LI>Field [" + formFieldName + "] was saved to this content definition but no database field was created because a field type are required.</LI>";
                                                        }
                                                    } else if ((formFieldName == cdefFieldKvp.Value.nameLc) && (formFieldTypeId != cdefFieldKvp.Value.fieldTypeId)) {
                                                        //
                                                        // Field Type changed, must be done manually
                                                        //
                                                        ErrorMessage += "<LI>Field [" + formFieldName + "] changed type from [" + cpCore.db.getRecordName("content Field Types", cdefFieldKvp.Value.fieldTypeId) + "] to [" + cpCore.db.getRecordName("content Field Types", formFieldTypeId) + "]. This may have caused a problem converting content.</LI>";
                                                        int DataSourceTypeID = cpCore.db.getDataSourceType(DataSourceName);
                                                        switch (DataSourceTypeID) {
                                                            case DataSourceTypeODBCMySQL:
                                                                SQL = "alter table " + CDef.ContentTableName + " change " + cdefFieldKvp.Value.nameLc + " " + cdefFieldKvp.Value.nameLc + " " + cpCore.db.getSQLAlterColumnType(DataSourceName, fieldType) + ";";
                                                                break;
                                                            default:
                                                                SQL = "alter table " + CDef.ContentTableName + " alter column " + cdefFieldKvp.Value.nameLc + " " + cpCore.db.getSQLAlterColumnType(DataSourceName, fieldType) + ";";
                                                                break;
                                                        }
                                                        cpCore.db.executeQuery(SQL, DataSourceName);
                                                    }
                                                    SQL = "Update ccFields"
                                                + " Set name=" + cpCore.db.encodeSQLText(formFieldName) + ",type=" + formFieldTypeId + ",caption=" + cpCore.db.encodeSQLText(cp.Doc.GetText("dtfaCaption." + RecordPointer)) + ",DefaultValue=" + cpCore.db.encodeSQLText(cp.Doc.GetText("dtfaDefaultValue." + RecordPointer)) + ",EditSortPriority=" + cpCore.db.encodeSQLText(genericController.encodeText(cp.Doc.GetInteger("dtfaEditSortPriority." + RecordPointer))) + ",Active=" + cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaActive." + RecordPointer)) + ",ReadOnly=" + cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaReadOnly." + RecordPointer)) + ",Authorable=" + cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaAuthorable." + RecordPointer)) + ",Required=" + cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaRequired." + RecordPointer)) + ",UniqueName=" + cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaUniqueName." + RecordPointer)) + ",TextBuffered=" + cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaTextBuffered." + RecordPointer)) + ",Password=" + cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaPassword." + RecordPointer)) + ",HTMLContent=" + cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaHTMLContent." + RecordPointer)) + ",EditTab=" + cpCore.db.encodeSQLText(cp.Doc.GetText("dtfaEditTab." + RecordPointer)) + ",Scramble=" + cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaScramble." + RecordPointer)) + "";
                                                    if (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                                                        SQL += ",adminonly=" + cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaAdminOnly." + RecordPointer));
                                                    }
                                                    if (cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore)) {
                                                        SQL += ",DeveloperOnly=" + cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaDeveloperOnly." + RecordPointer));
                                                    }
                                                    SQL += " where ID=" + formFieldId;
                                                    cpCore.db.executeQuery(SQL);
                                                    ReloadCDef = true;
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            cpCore.cache.invalidateAll();
                            cpCore.doc.clearMetaData();
                        }
                        if (ToolButton == ButtonAdd) {
                            //
                            // ----- Insert a blank Field
                            //
                            CSPointer = cpCore.db.csInsertRecord("Content Fields");
                            if (cpCore.db.csOk(CSPointer)) {
                                cpCore.db.csSet(CSPointer, "name", "unnamedField" + cpCore.db.csGetInteger(CSPointer, "id").ToString());
                                cpCore.db.csSet(CSPointer, "ContentID", ContentID);
                                cpCore.db.csSet(CSPointer, "EditSortPriority", 0);
                                ReloadCDef = true;
                            }
                            cpCore.db.csClose(ref CSPointer);
                        }
                        //
                        // ----- Button Reload CDef
                        //
                        if (ToolButton == ButtonSaveandInvalidateCache) {
                            cpCore.cache.invalidateAll();
                            cpCore.doc.clearMetaData();
                        }
                        //
                        // ----- Restore Content Autoload site property
                        //
                        if (AllowContentAutoLoad) {
                            cp.Site.SetProperty("AllowContentAutoLoad", AllowContentAutoLoad.ToString());
                        }
                        //
                        // ----- Cancel or Save, reload CDef and go
                        //
                        if ((ToolButton == ButtonCancel) || (ToolButton == ButtonOK)) {
                            //
                            // ----- Exit back to menu
                            //
                            return cpCore.webServer.redirect(cpCore.webServer.requestProtocol + cpCore.webServer.requestDomain + cpCore.webServer.requestPath + cpCore.webServer.requestPage + "?af=" + AdminFormTools);
                        }
                    }
                }
                //
                //--------------------------------------------------------------------------------
                //   Print Output
                //--------------------------------------------------------------------------------
                //
                Stream.Add(SpanClassAdminNormal + "<strong><a href=\"" + cpCore.webServer.requestPage + "?af=" + AdminFormToolRoot + "\">Tools</a></strong>&nbsp;»&nbsp;Manage Admin Edit Fields</span>");
                Stream.Add("<div>");
                Stream.Add("<div style=\"width:45%;float:left;padding:10px;\">Use this tool to add or modify content definition fields. Contensive uses a caching system for content definitions that is not automatically reloaded. Change you make will not take effect until the next time the system is reloaded. When you create a new field, the database field is created automatically when you have saved both a name and a field type. If you change the field type, you may have to manually change the database field.</div>");
                if (ContentID == 0) {
                    Stream.Add("<div style=\"width:45%;float:left;padding:10px;\">Related Tools:<div style=\"padding-left:20px;\"><a href=\"?af=104\">Set Default Admin Listing page columns</a></div></div>");
                } else {
                    Stream.Add("<div style=\"width:45%;float:left;padding:10px;\">Related Tools:<div style=\"padding-left:20px;\"><a href=\"?af=104&ContentID=" + ContentID + "\">Set Default Admin Listing page columns for '" + ContentName + "'</a></div><div style=\"padding-left:20px;\"><a href=\"?af=4&cid=" + Models.Complex.cdefModel.getContentId(cpCore, "content") + "&id=" + ContentID + "\">Edit '" + ContentName + "' Content Definition</a></div><div style=\"padding-left:20px;\"><a href=\"?cid=" + ContentID + "\">View records in '" + ContentName + "'</a></div></div>");
                }
                Stream.Add("</div>");
                Stream.Add("<div style=\"clear:both\">&nbsp;</div>");
                if (!string.IsNullOrEmpty(StatusMessage)) {
                    Stream.Add("<UL>" + StatusMessage + "</UL>");
                    Stream.Add("<div style=\"clear:both\">&nbsp;</div>");
                }
                if (!string.IsNullOrEmpty(ErrorMessage)) {
                    Stream.Add("<br><span class=\"ccError\">There was a problem saving these changes</span><UL>" + ErrorMessage + "</UL></SPAN>");
                    Stream.Add("<div style=\"clear:both\">&nbsp;</div>");
                }
                if (ReloadCDef) {
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentID, true, true);
                }
                if (ContentID != 0) {
                    //
                    //--------------------------------------------------------------------------------
                    // print the Configure edit form
                    //--------------------------------------------------------------------------------
                    //
                    Stream.Add(cpCore.html.getPanelTop());
                    ContentName = Local_GetContentNameByID(ContentID);
                    ButtonList = ButtonCancel + "," + ButtonSave + "," + ButtonOK + "," + ButtonAdd; // & "," & ButtonReloadCDef
                                                                                                     //
                                                                                                     // Get a new copy of the content definition
                                                                                                     //
                    Stream.Add(SpanClassAdminNormal + "<P><B>" + ContentName + "</b></P>");
                    Stream.Add("<table border=\"0\" cellpadding=\"1\" cellspacing=\"1\" width=\"100%\">");
                    //
                    ParentContentID = CDef.parentID;
                    if (ParentContentID == -1) {
                        AllowCDefInherit = false;
                    } else {
                        AllowCDefInherit = true;
                        ParentContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ParentContentID);
                        ParentCDef = Models.Complex.cdefModel.getCdef(cpCore, ParentContentID, true, true);
                    }
                    if (CDef.fields.Count > 0) {
                        Stream.Add("<tr>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\"></td>");
                        if (!AllowCDefInherit) {
                            Stream.Add("<td valign=\"bottom\" width=\"100\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b><br>Inherited*</b></span></td>");
                            NeedFootNote1 = true;
                        } else {
                            Stream.Add("<td valign=\"bottom\" width=\"100\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b><br>Inherited</b></span></td>");
                        }
                        Stream.Add("<td valign=\"bottom\" width=\"100\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b><br>Field</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"100\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b><br>Caption</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"100\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b><br>Edit Tab</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"100\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b><br>Default</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b><br>Type</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b>Edit<br>Order</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b><br>Active</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b>Read<br>Only</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b><br>Auth</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b><br>Req</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b><br>Unique</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b>Text<br>Buffer</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b><br>Pass</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b>Text<br>Scrm</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b><br>HTML</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b>Admin<br>Only</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b>Dev<br>Only</b></span></td>");
                        Stream.Add("</tr>");
                        RecordCount = 0;
                        //
                        // Build a select template for Type
                        //
                        TypeSelectTemplate = cpCore.html.main_GetFormInputSelect("menuname", -1, "Content Field Types", "", "unknown");
                        //
                        // Index the sort order
                        //
                        List<fieldSortClass> fieldList = new List<fieldSortClass>();
                        FieldCount = CDef.fields.Count;
                        foreach (var keyValuePair in CDef.fields) {
                            fieldSortClass fieldSort = new fieldSortClass();
                            //Dim field As New appServices_metaDataClass.CDefFieldClass
                            string sortOrder = "";
                            fieldSort.field = keyValuePair.Value;
                            sortOrder = "";
                            if (fieldSort.field.active) {
                                sortOrder += "0";
                            } else {
                                sortOrder += "1";
                            }
                            if (fieldSort.field.authorable) {
                                sortOrder += "0";
                            } else {
                                sortOrder += "1";
                            }
                            sortOrder += fieldSort.field.editTabName + GetIntegerString(fieldSort.field.editSortPriority, 10) + GetIntegerString(fieldSort.field.id, 10);
                            fieldSort.sort = sortOrder;
                            fieldList.Add(fieldSort);
                        }
                        fieldList.Sort((p1, p2) => p1.sort.CompareTo(p2.sort));
                        foreach (fieldSortClass fieldsort in fieldList) {
                            stringBuilderLegacyController streamRow = new stringBuilderLegacyController();
                            bool rowValid = true;
                            //
                            // If Field has name and type, it is locked and can not be changed
                            //
                            bool FieldLocked = (fieldsort.field.nameLc != "") & (fieldsort.field.fieldTypeId != 0);
                            //
                            // put the menu into the current menu format
                            //
                            formFieldId = fieldsort.field.id;
                            streamRow.Add(cpCore.html.html_GetFormInputHidden("dtfaID." + RecordCount, formFieldId));
                            streamRow.Add("<tr>");
                            //
                            // edit button
                            //
                            streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><img src=\"/ccLib/images/spacer.gif\" width=\"1\" height=\"10\">");
                            ContentFieldsCID = Local_GetContentID("Content Fields");
                            if (ContentFieldsCID != 0) {
                                streamRow.Add("<nobr>" + SpanClassAdminSmall + "[<a href=\"?aa=" + AdminActionNop + "&af=" + AdminFormEdit + "&id=" + formFieldId + "&cid=" + ContentFieldsCID + "&mm=0\">EDIT</a>]</span></nobr>");
                            }
                            streamRow.Add("</td>");
                            //
                            // Inherited
                            //
                            if (!AllowCDefInherit) {
                                //
                                // no parent
                                //
                                streamRow.Add("<td class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "False</span></td>");
                            } else if (fieldsort.field.inherited) {
                                //
                                // inherited property
                                //
                                streamRow.Add("<td class=\"ccPanelInput\" align=\"center\">" + cpCore.html.html_GetFormInputCheckBox2("dtfaInherited." + RecordCount, fieldsort.field.inherited) + "</td>");
                            } else {
                                //
                                // CDef has a parent, but the field is non-inherited, test for a matching Parent Field
                                //
                                if (ParentCDef == null) {
                                    foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> kvp in ParentCDef.fields) {
                                        if (kvp.Value.nameLc == fieldsort.field.nameLc) {
                                            parentField = kvp.Value;
                                            break;
                                        }
                                    }
                                }
                                if (parentField == null) {
                                    streamRow.Add("<td class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "False**</span></td>");
                                    NeedFootNote2 = true;
                                } else {
                                    streamRow.Add("<td class=\"ccPanelInput\" align=\"center\">" + cpCore.html.html_GetFormInputCheckBox2("dtfaInherited." + RecordCount, fieldsort.field.inherited) + "</td>");
                                }
                            }
                            //
                            // name
                            //
                            bool tmpValue = string.IsNullOrEmpty(fieldsort.field.nameLc);
                            rowValid = rowValid && !tmpValue;
                            streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
                            if (fieldsort.field.inherited) {
                                streamRow.Add(SpanClassAdminSmall + fieldsort.field.nameLc + "&nbsp;</SPAN>");
                            } else if (FieldLocked) {
                                streamRow.Add(SpanClassAdminSmall + fieldsort.field.nameLc + "&nbsp;</SPAN><input type=hidden name=dtfaName." + RecordCount + " value=\"" + fieldsort.field.nameLc + "\">");
                            } else {
                                streamRow.Add(cpCore.html.html_GetFormInputText2("dtfaName." + RecordCount, fieldsort.field.nameLc, 1, 10));
                            }
                            streamRow.Add("</nobr></td>");
                            //
                            // caption
                            //
                            streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
                            if (fieldsort.field.inherited) {
                                streamRow.Add(SpanClassAdminSmall + fieldsort.field.caption + "</SPAN>");
                            } else {
                                streamRow.Add(cpCore.html.html_GetFormInputText2("dtfaCaption." + RecordCount, fieldsort.field.caption, 1, 10));
                            }
                            streamRow.Add("</nobr></td>");
                            //
                            // Edit Tab
                            //
                            streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
                            if (fieldsort.field.inherited) {
                                streamRow.Add(SpanClassAdminSmall + fieldsort.field.editTabName + "</SPAN>");
                            } else {
                                streamRow.Add(cpCore.html.html_GetFormInputText2("dtfaEditTab." + RecordCount, fieldsort.field.editTabName, 1, 10));
                            }
                            streamRow.Add("</nobr></td>");
                            //
                            // default
                            //
                            streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
                            if (fieldsort.field.inherited) {
                                streamRow.Add(SpanClassAdminSmall + genericController.encodeText(fieldsort.field.defaultValue) + "</SPAN>");
                            } else {
                                streamRow.Add(cpCore.html.html_GetFormInputText2("dtfaDefaultValue." + RecordCount, genericController.encodeText(fieldsort.field.defaultValue), 1, 10));
                            }
                            streamRow.Add("</nobr></td>");
                            //
                            // type
                            //
                            rowValid = rowValid && (fieldsort.field.fieldTypeId > 0);
                            streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
                            if (fieldsort.field.inherited) {
                                CSPointer = cpCore.db.csOpenRecord("Content Field Types", fieldsort.field.fieldTypeId);
                                if (!cpCore.db.csOk(CSPointer)) {
                                    streamRow.Add(SpanClassAdminSmall + "Unknown[" + fieldsort.field.fieldTypeId + "]</SPAN>");
                                } else {
                                    streamRow.Add(SpanClassAdminSmall + cpCore.db.csGetText(CSPointer, "Name") + "</SPAN>");
                                }
                                cpCore.db.csClose(ref CSPointer);
                            } else if (FieldLocked) {
                                streamRow.Add(cpCore.db.getRecordName("content field types", fieldsort.field.fieldTypeId) + cpCore.html.html_GetFormInputHidden("dtfaType." + RecordCount, fieldsort.field.fieldTypeId));
                            } else {
                                TypeSelect = TypeSelectTemplate;
                                TypeSelect = genericController.vbReplace(TypeSelect, "menuname", "dtfaType." + RecordCount, 1, 99, 1);
                                TypeSelect = genericController.vbReplace(TypeSelect, "=\"" + fieldsort.field.fieldTypeId + "\"", "=\"" + fieldsort.field.fieldTypeId + "\" selected", 1, 99, 1);
                                streamRow.Add(TypeSelect);
                            }
                            streamRow.Add("</nobr></td>");
                            //
                            // sort priority
                            //
                            streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
                            if (fieldsort.field.inherited) {
                                streamRow.Add(SpanClassAdminSmall + fieldsort.field.editSortPriority + "</SPAN>");
                            } else {
                                streamRow.Add(cpCore.html.html_GetFormInputText2("dtfaEditSortPriority." + RecordCount, fieldsort.field.editSortPriority.ToString(), 1, 10));
                            }
                            streamRow.Add("</nobr></td>");
                            //
                            // active
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaActive." + RecordCount, genericController.encodeText(fieldsort.field.active), fieldsort.field.inherited));
                            //
                            // read only
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaReadOnly." + RecordCount, genericController.encodeText(fieldsort.field.ReadOnly), fieldsort.field.inherited));
                            //
                            // authorable
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaAuthorable." + RecordCount, genericController.encodeText(fieldsort.field.authorable), fieldsort.field.inherited));
                            //
                            // required
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaRequired." + RecordCount, genericController.encodeText(fieldsort.field.Required), fieldsort.field.inherited));
                            //
                            // UniqueName
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaUniqueName." + RecordCount, genericController.encodeText(fieldsort.field.UniqueName), fieldsort.field.inherited));
                            //
                            // text buffered
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaTextBuffered." + RecordCount, genericController.encodeText(fieldsort.field.TextBuffered), fieldsort.field.inherited));
                            //
                            // password
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaPassword." + RecordCount, genericController.encodeText(fieldsort.field.Password), fieldsort.field.inherited));
                            //
                            // scramble
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaScramble." + RecordCount, genericController.encodeText(fieldsort.field.Scramble), fieldsort.field.inherited));
                            //
                            // HTML Content
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaHTMLContent." + RecordCount, genericController.encodeText(fieldsort.field.htmlContent), fieldsort.field.inherited));
                            //
                            // Admin Only
                            //
                            if (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                                streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaAdminOnly." + RecordCount, genericController.encodeText(fieldsort.field.adminOnly), fieldsort.field.inherited));
                            }
                            //
                            // Developer Only
                            //
                            if (cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore)) {
                                streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaDeveloperOnly." + RecordCount, genericController.encodeText(fieldsort.field.developerOnly), fieldsort.field.inherited));
                            }
                            //
                            streamRow.Add("</tr>");
                            RecordCount = RecordCount + 1;
                            //
                            // rows are built - put the blank rows at the top
                            //
                            if (!rowValid) {
                                Stream.Add(streamRow.Text);
                            } else {
                                StreamValidRows.Add(streamRow.Text);
                            }
                        }
                        Stream.Add(StreamValidRows.Text);
                        Stream.Add(cpCore.html.html_GetFormInputHidden("dtfaRecordCount", RecordCount));
                    }
                    Stream.Add("</table>");
                    //Stream.Add( cpcore.htmldoc.main_GetPanelButtons(ButtonList, "Button"))
                    //
                    Stream.Add(cpCore.html.getPanelBottom());
                    //Call Stream.Add(cpCore.main_GetFormEnd())
                    if (NeedFootNote1) {
                        Stream.Add("<br>*Field Inheritance is not allowed because this Content Definition has no parent.");
                    }
                    if (NeedFootNote2) {
                        Stream.Add("<br>**This field can not be inherited because the Parent Content Definition does not have a field with the same name.");
                    }
                }
                if (ContentID != 0) {
                    //
                    // Save the content selection
                    //
                    Stream.Add(cpCore.html.html_GetFormInputHidden(RequestNameToolContentID, ContentID));
                } else {
                    //
                    // content tables that have edit forms to Configure
                    //
                    FormPanel = FormPanel + SpanClassAdminNormal + "Select a Content Definition to Configure its edit form<br>";
                    FormPanel = FormPanel + "<br>";
                    FormPanel = FormPanel + cpCore.html.main_GetFormInputSelect(RequestNameToolContentID, ContentID, "Content");
                    FormPanel = FormPanel + "</SPAN>";
                    Stream.Add(cpCore.html.getPanel(FormPanel));
                }
                //
                Stream.Add(cpCore.html.html_GetFormInputHidden("ReloadCDef", ReloadCDef));
                result = htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //
        //
        private string GetForm_ConfigureEdit_CheckBox(string Label, string Value, bool Inherited) {
            string tempGetForm_ConfigureEdit_CheckBox = null;
            tempGetForm_ConfigureEdit_CheckBox = "<td class=\"ccPanelInput\" align=\"center\"><nobr>";
            if (Inherited) {
                tempGetForm_ConfigureEdit_CheckBox = tempGetForm_ConfigureEdit_CheckBox + SpanClassAdminSmall + Value + "</SPAN>";
            } else {
                tempGetForm_ConfigureEdit_CheckBox = tempGetForm_ConfigureEdit_CheckBox + cpCore.html.html_GetFormInputCheckBox(Label, Value);
            }
            return tempGetForm_ConfigureEdit_CheckBox + "</nobr></td>";
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
                string SQL = null;
                int TableID = 0;
                string TableName = "";
                string FieldName = null;
                string IndexName = null;
                string DataSource = "";
                DataTable RSSchema = null;
                string Button = null;
                int CS = 0;
                string[,] Rows = null;
                int RowMax = 0;
                int RowPointer = 0;
                string Copy = "";
                bool TableRowEven = false;
                int TableColSpan = 0;
                string ButtonList;
                //
                ButtonList = ButtonCancel + "," + ButtonSelect;
                result = GetTitle("Modify Database Indexes", "This tool adds and removes database indexes.");
                //
                // Process Input
                //
                Button = cpCore.docProperties.getText("Button");
                TableID = cpCore.docProperties.getInteger("TableID");
                //
                // Get Tablename and DataSource
                //
                CS = cpCore.db.csOpenRecord("Tables", TableID,false,false, "Name,DataSourceID");
                if (cpCore.db.csOk(CS)) {
                    TableName = cpCore.db.csGetText(CS, "name");
                    DataSource = cpCore.db.csGetLookup(CS, "DataSourceID");
                }
                cpCore.db.csClose(ref CS);
                //
                if ((TableID != 0) & (TableID == cpCore.docProperties.getInteger("previoustableid")) & (!string.IsNullOrEmpty(Button))) {
                    //
                    // Drop Indexes
                    //
                    Count = cpCore.docProperties.getInteger("DropCount");
                    if (Count > 0) {
                        for (Pointer = 0; Pointer < Count; Pointer++) {
                            if (cpCore.docProperties.getBoolean("DropIndex." + Pointer)) {
                                IndexName = cpCore.docProperties.getText("DropIndexName." + Pointer);
                                result = result + "<br>Dropping index [" + IndexName + "] from table [" + TableName + "]";
                                cpCore.db.deleteSqlIndex("Default", TableName, IndexName);
                            }
                        }
                    }
                    //
                    // Add Indexes
                    //
                    Count = cpCore.docProperties.getInteger("AddCount");
                    if (Count > 0) {
                        for (Pointer = 0; Pointer < Count; Pointer++) {
                            if (cpCore.docProperties.getBoolean("AddIndex." + Pointer)) {
                                //IndexName = cpCore.main_GetStreamText2("AddIndexFieldName." & Pointer)
                                FieldName = cpCore.docProperties.getText("AddIndexFieldName." + Pointer);
                                IndexName = TableName + FieldName;
                                result = result + "<br>Adding index [" + IndexName + "] to table [" + TableName + "] for field [" + FieldName + "]";
                                cpCore.db.createSQLIndex(DataSource, TableName, IndexName, FieldName);
                            }
                        }
                    }
                }
                //
                result = result + cpCore.html.html_GetFormStart();
                TableColSpan = 3;
                result = result + StartTable(2, 0, 0);
                //
                // Select Table Form
                //
                result = result + GetTableRow("<br><br><B>Select table to index</b>", TableColSpan, false);
                result = result + GetTableRow(cpCore.html.main_GetFormInputSelect("TableID", TableID, "Tables","", "Select a SQL table to start"), TableColSpan, false);
                if (TableID != 0) {
                    //
                    // Add/Drop Indexes form
                    //
                    result = result + cpCore.html.html_GetFormInputHidden("PreviousTableID", TableID);
                    //
                    // Drop Indexes
                    //
                    result = result + GetTableRow("<br><br><B>Select indexes to remove</b>", TableColSpan, TableRowEven);
                    RSSchema = cpCore.db.getIndexSchemaData(TableName);


                    if (RSSchema.Rows.Count == 0) {
                        //
                        // ----- no result
                        //
                        Copy += DateTime.Now + " A schema was returned, but it contains no indexs.";
                        result = result + GetTableRow(Copy, TableColSpan, TableRowEven);
                    } else {

                        Rows = cpCore.db.convertDataTabletoArray(RSSchema);
                        RowMax = Rows.GetUpperBound(1);
                        for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                            IndexName = genericController.encodeText(Rows[5, RowPointer]);
                            if (!string.IsNullOrEmpty(IndexName)) {
                                result = result + StartTableRow();
                                Copy = cpCore.html.html_GetFormInputCheckBox2("DropIndex." + RowPointer, false) + cpCore.html.html_GetFormInputHidden("DropIndexName." + RowPointer, IndexName) + genericController.encodeText(IndexName);
                                result = result + GetTableCell(Copy,"",0, TableRowEven);
                                result = result + GetTableCell(genericController.encodeText(Rows[17, RowPointer]),"",0, TableRowEven);
                                result = result + GetTableCell("&nbsp;","",0, TableRowEven);
                                result = result + kmaEndTableRow;
                                TableRowEven = !TableRowEven;
                            }
                        }
                        result = result + cpCore.html.html_GetFormInputHidden("DropCount", RowMax + 1);
                    }
                    //
                    // Add Indexes
                    //
                    TableRowEven = false;
                    result = result + GetTableRow("<br><br><B>Select database fields to index</b>", TableColSpan, TableRowEven);
                    RSSchema = cpCore.db.getColumnSchemaData(TableName);
                    if (RSSchema.Rows.Count == 0) {
                        //
                        // ----- no result
                        //
                        Copy += DateTime.Now + " A schema was returned, but it contains no indexs.";
                        result = result + GetTableRow(Copy, TableColSpan, TableRowEven);
                    } else {

                        Rows = cpCore.db.convertDataTabletoArray(RSSchema);
                        //
                        RowMax = Rows.GetUpperBound(1);
                        for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                            result = result + StartTableRow();
                            Copy = cpCore.html.html_GetFormInputCheckBox2("AddIndex." + RowPointer, false) + cpCore.html.html_GetFormInputHidden("AddIndexFieldName." + RowPointer, Rows[3, RowPointer]) + genericController.encodeText(Rows[3, RowPointer]);
                            result = result + GetTableCell(Copy,"",0, TableRowEven);
                            result = result + GetTableCell("&nbsp;","",0, TableRowEven);
                            result = result + GetTableCell("&nbsp;","",0, TableRowEven);
                            result = result + kmaEndTableRow;
                            TableRowEven = !TableRowEven;
                        }
                        result = result + cpCore.html.html_GetFormInputHidden("AddCount", RowMax + 1);
                    }
                    //
                    // Spacers
                    //
                    result = result + StartTableRow();
                    result = result + GetTableCell(getSpacer(300, 1), "200");
                    result = result + GetTableCell(getSpacer(200, 1), "200");
                    result = result + GetTableCell("&nbsp;", "100%");
                    result = result + kmaEndTableRow;
                }
                result = result + kmaEndTable;
                //
                // Buttons
                //
                result = htmlController.legacy_openFormTable(cpCore, ButtonList) + result + htmlController.legacy_closeFormTable(cpCore, ButtonList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
                int CS = 0;
                int TableColSpan = 0;
                bool TableEvenRow = false;
                string SQL = null;
                string TableName = null;
                string ButtonList;
                //
                ButtonList = ButtonCancel;
                result = GetTitle("Get Content Database Schema", "This tool displays all tables and fields required for the current Content Defintions.");
                //
                TableColSpan = 3;
                result = result + StartTable(2, 0, 0);
                SQL = "SELECT DISTINCT ccTables.Name as TableName, ccFields.Name as FieldName, ccFieldTypes.Name as FieldType"
                        + " FROM ((ccContent LEFT JOIN ccTables ON ccContent.ContentTableID = ccTables.ID) LEFT JOIN ccFields ON ccContent.ID = ccFields.ContentID) LEFT JOIN ccFieldTypes ON ccFields.Type = ccFieldTypes.ID"
                        + " ORDER BY ccTables.Name, ccFields.Name;";
                CS = cpCore.db.csOpenSql_rev("Default", SQL);
                TableName = "";
                while (cpCore.db.csOk(CS)) {
                    if (TableName != cpCore.db.csGetText(CS, "TableName")) {
                        TableName = cpCore.db.csGetText(CS, "TableName");
                        result = result + GetTableRow("<B>" + TableName + "</b>", TableColSpan, TableEvenRow);
                    }
                    result = result + StartTableRow();
                    result = result + GetTableCell("&nbsp;","",0, TableEvenRow);
                    result = result + GetTableCell(cpCore.db.csGetText(CS, "FieldName"), "", 0, TableEvenRow);
                    result = result + GetTableCell(cpCore.db.csGetText(CS, "FieldType"), "", 0, TableEvenRow);
                    result = result + kmaEndTableRow;
                    TableEvenRow = !TableEvenRow;
                    cpCore.db.csGoNext(CS);
                }
                //
                // Field Type Definitions
                //
                result = result + GetTableRow("<br><br><B>Field Type Definitions</b>", TableColSpan, TableEvenRow);
                result = result + GetTableRow("Boolean - Boolean values 0 and 1 are stored in a database long integer field type", TableColSpan, TableEvenRow);
                result = result + GetTableRow("Lookup - References to related records stored as database long integer field type", TableColSpan, TableEvenRow);
                result = result + GetTableRow("Integer - database long integer field type", TableColSpan, TableEvenRow);
                result = result + GetTableRow("Float - database floating point value", TableColSpan, TableEvenRow);
                result = result + GetTableRow("Date - database DateTime field type.", TableColSpan, TableEvenRow);
                result = result + GetTableRow("AutoIncrement - database long integer field type. Field automatically increments when a record is added.", TableColSpan, TableEvenRow);
                result = result + GetTableRow("Text - database character field up to 255 characters.", TableColSpan, TableEvenRow);
                result = result + GetTableRow("LongText - database character field up to 64K characters.", TableColSpan, TableEvenRow);
                result = result + GetTableRow("TextFile - references a filename in the Content Files folder. Database character field up to 255 characters. ", TableColSpan, TableEvenRow);
                result = result + GetTableRow("File - references a filename in the Content Files folder. Database character field up to 255 characters. ", TableColSpan, TableEvenRow);
                result = result + GetTableRow("Redirect - This field has no database equivelent. No Database field is required.", TableColSpan, TableEvenRow);
                //
                // Spacers
                //
                result = result + StartTableRow();
                result = result + GetTableCell(getSpacer(20, 1), "20");
                result = result + GetTableCell(getSpacer(300, 1), "300");
                result = result + GetTableCell("&nbsp;", "100%");
                result = result + kmaEndTableRow;
                result = result + kmaEndTable;
                //
                //GetForm_ContentDbSchema = GetForm_ContentDbSchema & cpCore.main_GetFormInputHidden("af", AdminFormToolContentDbSchema)
                result =  (htmlController.legacy_openFormTable(cpCore, ButtonList)) + result + (htmlController.legacy_closeFormTable(cpCore, ButtonList));
                //
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
                tempGetForm_LogFiles = GetTitle("Log File View", "This tool displays the Contensive Log Files.");
                tempGetForm_LogFiles = tempGetForm_LogFiles + "<P></P>";
                //
                string QueryOld = ".asp?";
                string QueryNew = genericController.ModifyQueryString(QueryOld, RequestNameAdminForm, AdminFormToolLogFileView, true);
                tempGetForm_LogFiles = tempGetForm_LogFiles + genericController.vbReplace(GetForm_LogFiles_Details(), QueryOld, QueryNew + "&", 1, 99, 1);
                //
                tempGetForm_LogFiles = htmlController.legacy_openFormTable(cpCore, ButtonList) + tempGetForm_LogFiles + (htmlController.legacy_closeFormTable(cpCore, ButtonList));
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
                    + "<td width=\"23\"><img src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"23\"></td>"
                    + "<td width=\"60%\"><img src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"1\"></td>"
                    + "<td width=\"20%\"><img src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"1\"></td>"
                    + "<td width=\"20%\"><img src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"1\"></td>"
                    + "</tr>";
                const string GetTableEnd = "</table></td></tr></table>";
                //
                const string SpacerImage = "<img src=\"/ccLib/Images/spacer.gif\" width=\"23\" height=\"22\" border=\"0\">";
                const string FolderOpenImage = "<img src=\"/ccLib/Images/iconfolderopen.gif\" width=\"23\" height=\"22\" border=\"0\">";
                const string FolderClosedImage = "<img src=\"/ccLib/Images/iconfolderclosed.gif\" width=\"23\" height=\"22\" border=\"0\">";
                //
                // StartPath is the root - the top of the directory, it ends in the folder name (no slash)
                //
                result = "";
                StartPath = cpCore.programDataFiles.rootLocalPath + "Logs\\";
                //
                // CurrentPath is what is concatinated on to StartPath to get the current folder, it must start with a slash
                //
                CurrentPath = cpCore.docProperties.getText("SetPath");
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
                if (cpCore.docProperties.getText("SourceFile") != "") {
                    //
                    // Return the content of the file
                    //
                    cpCore.webServer.setResponseContentType("text/text");
                    result = cpCore.appRootFiles.readFile(cpCore.docProperties.getText("SourceFile"));
                    cpCore.doc.continueProcessing = false;
                } else {
                    result = result + GetTableStart;
                    //
                    // Parent Folder Link
                    //
                    if (CurrentPath != ParentPath) {
                        FileSize = "";
                        FileDate = "";
                        result = result + GetForm_LogFiles_Details_GetRow("<A href=\"" + cpCore.webServer.requestPage + "?SetPath=" + ParentPath + "\">" + FolderOpenImage + "</A>", "<A href=\"" + cpCore.webServer.requestPage + "?SetPath=" + ParentPath + "\">" + ParentPath + "</A>", FileSize, FileDate, RowEven);
                    }
                    //
                    // Sub-Folders
                    //

                    SourceFolders = cpCore.appRootFiles.getFolderNameList(StartPath + CurrentPath);
                    if (!string.IsNullOrEmpty(SourceFolders)) {
                        FolderSplit = SourceFolders.Split(new[] { "\r\n" }, StringSplitOptions.None);
                        FolderCount = FolderSplit.GetUpperBound(0) + 1;
                        for (FolderPointer = 0; FolderPointer < FolderCount; FolderPointer++) {
                            FolderLine = FolderSplit[FolderPointer];
                            if (!string.IsNullOrEmpty(FolderLine)) {
                                LineSplit = FolderLine.Split(',');
                                FolderName = LineSplit[0];
                                FileSize = LineSplit[1];
                                FileDate = LineSplit[2];
                                result = result + GetForm_LogFiles_Details_GetRow("<A href=\"" + cpCore.webServer.requestPage + "?SetPath=" + CurrentPath + "\\" + FolderName + "\">" + FolderClosedImage + "</A>", "<A href=\"" + cpCore.webServer.requestPage + "?SetPath=" + CurrentPath + "\\" + FolderName + "\">" + FolderName + "</A>", FileSize, FileDate, RowEven);
                            }
                        }
                    }
                    //
                    // Files
                    //
                    SourceFolders = cpCore.appRootFiles.convertFileINfoArrayToParseString(cpCore.appRootFiles.getFileList(StartPath + CurrentPath));
                    if (string.IsNullOrEmpty(SourceFolders)) {
                        FileSize = "";
                        FileDate = "";
                        result = result + GetForm_LogFiles_Details_GetRow(SpacerImage, "no files were found in this folder", FileSize, FileDate, RowEven);
                    } else {
                        FolderSplit = SourceFolders.Split(new[] { "\r\n" }, StringSplitOptions.None);
                        FolderCount = FolderSplit.GetUpperBound(0) + 1;
                        for (FolderPointer = 0; FolderPointer < FolderCount; FolderPointer++) {
                            FolderLine = FolderSplit[FolderPointer];
                            if (!string.IsNullOrEmpty(FolderLine)) {
                                LineSplit = FolderLine.Split(',');
                                Filename = LineSplit[0];
                                FileSize = LineSplit[5];
                                FileDate = LineSplit[3];
                                FileURL = StartPath + CurrentPath + "\\" + Filename;
                                QueryString = cpCore.doc.refreshQueryString;
                                QueryString = genericController.ModifyQueryString(QueryString, RequestNameAdminForm, encodeText(AdminFormTool), true);
                                QueryString = genericController.ModifyQueryString(QueryString, "at", AdminFormToolLogFileView, true);
                                QueryString = genericController.ModifyQueryString(QueryString, "SourceFile", FileURL, true);
                                CellCopy = "<A href=\"" + cpCore.webServer.requestPath + "?" + QueryString + "\" target=\"_blank\">" + Filename + "</A>";
                                result = result + GetForm_LogFiles_Details_GetRow(SpacerImage, CellCopy, FileSize, FileDate, RowEven);
                            }
                        }
                    }
                    //
                    result = result + GetTableEnd;
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   Table Rows
        //=============================================================================
        //
        public string GetForm_LogFiles_Details_GetRow(string Cell0, string Cell1, string Cell2, string Cell3, bool RowEven) {
            string tempGetForm_LogFiles_Details_GetRow = null;
            //
            string ClassString = null;
            //
            if (genericController.encodeBoolean(RowEven)) {
                RowEven = false;
                ClassString = " class=\"ccPanelRowEven\" ";
            } else {
                RowEven = true;
                ClassString = " class=\"ccPanelRowOdd\" ";
            }
            //
            Cell0 = genericController.encodeText(Cell0);
            if (string.IsNullOrEmpty(Cell0)) {
                Cell0 = "&nbsp;";
            }
            //
            Cell1 = genericController.encodeText(Cell1);
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
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string ButtonList;
                //
                ButtonList = ButtonCancel + "," + ButtonSaveandInvalidateCache;
                Stream.Add(GetTitle("Load Content Definitions", "This tool reloads the content definitions. This is necessary when changes are made to the ccContent or ccFields tables outside Contensive. The site will be blocked during the load."));
                //
                if ((cpCore.docProperties.getText("button")) != ButtonSaveandInvalidateCache) {
                    //
                    // First pass, initialize
                    //
                } else {
                    //
                    // Restart
                    //
                    Stream.Add("<br>Loading Content Definitions...");
                    cpCore.cache.invalidateAll();
                    cpCore.doc.clearMetaData();
                    Stream.Add("<br>Content Definitions loaded");
                }
                //
                // Display form
                //
                Stream.Add(SpanClassAdminNormal);
                Stream.Add("<br>");
                Stream.Add("</span>");
                //
                result = htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //=============================================================================
        //   Print the manual query form
        //=============================================================================
        //
        private string GetForm_Restart() {
            string tempGetForm_Restart = null;
            try {
                //
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string ButtonList;
                // Dim runAtServer As runAtServerClass
                //
                ButtonList = ButtonCancel + "," + ButtonRestartContensiveApplication;
                Stream.Add(GetTitle("Load Content Definitions", "This tool stops and restarts the Contensive Application controlling this website. If you restart, the site will be unavailable for up to a minute while the site is stopped and restarted. If the site does not restart, you will need to contact a site administrator to have the Contensive Server restarted."));
                //
                if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                    //
                    tempGetForm_Restart = "<P>You must be an administrator to use this tool.</P>";
                    //
                } else if ((cpCore.docProperties.getText("button")) != ButtonRestartContensiveApplication) {
                    //
                    // First pass, initialize
                    //
                } else {
                    //
                    // Restart
                    logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "Restarting Contensive", "dll", "ToolsClass", "GetForm_Restart", 0, "dll", "Warning: member " + cpCore.doc.authContext.user.name + " (" + cpCore.doc.authContext.user.id + ") restarted using the Restart tool", false, true, cpCore.webServer.requestUrl, "", "");
                    cpCore.webServer.redirect("/ccLib/Popup/WaitForIISReset.htm");
                    Thread.Sleep(2000);
                    //
                    taskSchedulerController.addTaskToQueue(cpCore, taskQueueCommandEnumModule.runAddon, new cmdDetailClass() {
                        addonId = 0,
                        addonName = "commandRestart",
                        docProperties = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, "")
                    }, false);
                }
                //
                // Display form
                //
                Stream.Add(SpanClassAdminNormal);
                Stream.Add("<br>");
                Stream.Add("</SPAN>");
                //
                tempGetForm_Restart =  htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return tempGetForm_Restart;
        }
        //
        //
        //
        private Models.Complex.cdefModel GetCDef(string ContentName) {
            return Models.Complex.cdefModel.getCdef(cpCore, ContentName);
        }



        //Function CloseFormTable(ByVal ButtonList As String) As String
        //    If ButtonList <> "" Then
        //        CloseFormTable = "</td></tr></TABLE>" & cpcore.htmldoc.main_GetPanelButtons(ButtonList, "Button") & "</form>"
        //    Else
        //        CloseFormTable = "</td></tr></TABLE></form>"
        //    End If
        //End Function
        //
        //
        //
        //Function genericLegacyView.OpenFormTable(cpcore,ByVal ButtonList As String) As String
        //    Dim result As String = ""
        //    Try
        //        OpenFormTable = cpCore.htmldoc.html_GetFormStart()
        //        If ButtonList <> "" Then
        //            OpenFormTable = OpenFormTable & cpcore.htmldoc.main_GetPanelButtons(ButtonList, "Button")
        //        End If
        //        OpenFormTable = OpenFormTable & "<table border=""0"" cellpadding=""10"" cellspacing=""0"" width=""100%""><tr><TD>"
        //    Catch ex As Exception
        //        cpCore.handleExceptionAndContinue(ex)
        //    End Try
        //    Return result
        //End Function

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
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string ButtonList = null;
                bool AllowImageImport = false;
                bool AllowStyleImport = false;
                //
                bool AllowBodyHTML = cpCore.docProperties.getBoolean("AllowBodyHTML");
                bool AllowScriptLink = cpCore.docProperties.getBoolean("AllowScriptLink");
                AllowImageImport = cpCore.docProperties.getBoolean("AllowImageImport");
                AllowStyleImport = cpCore.docProperties.getBoolean("AllowStyleImport");
                //
                Stream.Add(GetTitle("Load Templates", "This tool creates template records from the HTML files in the root folder of the site."));
                //
                if ((cpCore.docProperties.getText("button")) != ButtonImportTemplates) {
                    //
                    // First pass, initialize
                    //
                } else {
                    //
                    // Restart
                    //
                    Stream.Add("<br>Loading Templates...");
                    //Call Stream.Add(ImportTemplates(cpCore.appRootFiles.rootLocalPath, "", AllowBodyHTML, AllowScriptLink, AllowImageImport, AllowStyleImport))
                    Stream.Add("<br>Templates loaded");
                }
                //
                // Display form
                //
                Stream.Add(SpanClassAdminNormal);
                Stream.Add("<br>");
                //Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolLoadTemplates)
                Stream.Add("<br>" + cpCore.html.html_GetFormInputCheckBox2("AllowBodyHTML", AllowBodyHTML) + " Update/Import Soft Templates from the Body of .HTM and .HTML files");
                Stream.Add("<br>" + cpCore.html.html_GetFormInputCheckBox2("AllowScriptLink", AllowScriptLink) + " Update/Import Hard Templates with links to .ASP and .ASPX scripts");
                Stream.Add("<br>" + cpCore.html.html_GetFormInputCheckBox2("AllowImageImport", AllowImageImport) + " Update/Import image links (.GIF,.JPG,.PDF ) into the resource library");
                Stream.Add("<br>" + cpCore.html.html_GetFormInputCheckBox2("AllowStyleImport", AllowStyleImport) + " Import style sheets (.CSS) to Dynamic Styles");
                Stream.Add("</SPAN>");
                //
                ButtonList = ButtonCancel + "," + ButtonImportTemplates;
                result = htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
        //        FileList = cpCore.appRootFiles.convertFileINfoArrayToParseString(cpCore.appRootFiles.getFileList(FileRootPath & AppPath))
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
        //                CS = cpCore.db.cs_open("Page Templates", "Link=" & cpCore.db.encodeSQLText(Link))
        //                If Not cpCore.db.cs_ok(CS) Then
        //                    Call cpCore.db.cs_Close(CS)
        //                    CS = cpCore.db.cs_insertRecord("Page Templates")
        //                    Call cpCore.db.cs_set(CS, "Link", Link)
        //                End If
        //                If cpCore.db.cs_ok(CS) Then
        //                    Call cpCore.db.cs_set(CS, "name", TemplateName)
        //                End If
        //                Call cpCore.db.cs_Close(CS)
        //            ElseIf AllowBodyHTML And (InStr(1, Filename, ".htm", vbTextCompare) <> 0) And (Mid(Filename, 1, 1) <> "_") Then
        //                '
        //                ' HTML, import body
        //                '
        //                PageSource = cpCore.appRootFiles.readFile(Filename)
        //                Call cpCore.main_EncodePage_SplitBody(PageSource, PageSource, "", "")
        //                Link = genericController.vbReplace(AppPath & Filename, "\", "/")
        //                TemplateName = genericController.vbReplace(Link, "/", "-")
        //                If genericController.vbInstr(1, TemplateName, ".") <> 0 Then
        //                    TemplateName = Mid(TemplateName, 1, genericController.vbInstr(1, TemplateName, ".") - 1)

        //                End If
        //                '
        //                result = result & "<br>Create Soft Template from source [" & Link & "]"
        //                '
        //                CS = cpCore.db.cs_open("Page Templates", "Source=" & cpCore.db.encodeSQLText(Link))
        //                If Not cpCore.db.cs_ok(CS) Then
        //                    Call cpCore.db.cs_Close(CS)
        //                    CS = cpCore.db.cs_insertRecord("Page Templates")
        //                    Call cpCore.db.cs_set(CS, "Source", Link)
        //                    Call cpCore.db.cs_set(CS, "name", TemplateName)
        //                End If
        //                If cpCore.db.cs_ok(CS) Then
        //                    Call cpCore.db.cs_set(CS, "Link", "")
        //                    Call cpCore.db.cs_set(CS, "bodyhtml", PageSource)
        //                End If
        //                Call cpCore.db.cs_Close(CS)
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
        //                Copy = cpCore.appRootFiles.readFile(DynamicFilename)
        //                Copy = RemoveStyleTags(Copy)
        //                Copy = Copy _
        //                    & vbCrLf _
        //                    & vbCrLf & "/* Import of " & FileRootPath & AppPath & Filename & "*/" _
        //                    & vbCrLf _
        //                    & vbCrLf
        //                Copy = Copy & RemoveStyleTags(cpCore.appRootFiles.readFile(Filename))
        //                Call cpCore.appRootFiles.saveFile(DynamicFilename, Copy)
        //            End If
        //        Next
        //        '
        //        ' Now process all subfolders
        //        '
        //        FolderList = cpCore.getFolderNameList(FileRootPath & AppPath)
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
        //        cpCore.handleExceptionAndContinue(ex) : Throw
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
        //            CSContent = cpCore.db.cs_open("Content", "name=" & cpCore.db.encodeSQLText(ContentName))
        //            If cpCore.db.cs_ok(CSContent) Then
        //                '
        //                ' Start with parent CDef
        //                '
        //                ParentID = cpCore.db.cs_getInteger(CSContent, "parentID")
        //                If ParentID <> 0 Then
        //                    ParentContentName = Models.Complex.cdefModel.getContentNameByID(cpcore,ParentID)
        //                    If ParentContentName <> "" Then
        //                        LoadCDef = LoadCDef(ParentContentName)
        //                    End If
        //                End If
        //                '
        //                ' Add this definition on it
        //                '
        //                With LoadCDef
        //                    CS = cpCore.db.cs_open("Content Fields", "contentid=" & ContentID)
        //                    Do While cpCore.db.cs_ok(CS)
        //                        Select Case genericController.vbUCase(cpCore.db.cs_getText(CS, "name"))
        //                            Case "NAME"
        //                                .Name = ""
        //                        End Select
        //                        Call cpCore.db.cs_goNext(CS)
        //                    Loop
        //                    Call cpCore.db.cs_Close(CS)
        //                End With
        //            End If
        //            Call cpCore.db.cs_Close(CSContent)
        //            throw (New ApplicationException("Unexpected exception"))'  Call handleLegacyClassErrors1("ImportTemplates", "ErrorTrap")
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
        //            throw (New ApplicationException("Unexpected exception"))'  Call handleLegacyClassErrors1("GetDbCDef_SetAdminColumns", "ErrorTrap")
        //        End Sub
        //
        //=============================================================================
        //   Print the manual query form
        //=============================================================================
        //
        private string GetForm_ContentFileManager() {
            string result = "";
            try {
                adminUIController Adminui = new adminUIController(cpCore);
                string InstanceOptionString = "AdminLayout=1&filesystem=content files";
                addonModel addon = addonModel.create(cpCore, "{B966103C-DBF4-4655-856A-3D204DEF6B21}");
                string Content = cpCore.addon.execute(addon, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                    addonType = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                    instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, InstanceOptionString),
                    instanceGuid = "-2",
                    errorCaption = "File Manager"
                });
                string Description = "Manage files and folders within the virtual content file area.";
                string ButtonList = ButtonApply + "," + ButtonCancel;
                result = Adminui.GetBody("Content File Manager", ButtonList, "", false, false, Description, "", 0, Content);
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
                adminUIController Adminui = new adminUIController(cpCore);
                string InstanceOptionString = "AdminLayout=1&filesystem=website files";
                addonModel addon = addonModel.create(cpCore, "{B966103C-DBF4-4655-856A-3D204DEF6B21}");
                string Content = cpCore.addon.execute(addon, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                    addonType = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                    instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, InstanceOptionString),
                    instanceGuid = "-2",
                    errorCaption = "File Manager"
                });
                string Description = "Manage files and folders within the Website's file area.";
                string ButtonList = ButtonApply + "," + ButtonCancel;
                result = Adminui.GetBody("Website File Manager", ButtonList, "", false, false, Description, "", 0, Content);
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
                int CS = 0;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                // Dim runAtServer As New runAtServerClass(cpCore)
                string CDefList = "";
                string FindText = "";
                string ReplaceText = "";
                string Button = null;
                int ReplaceRows = 0;
                int FindRows = 0;
                string lcName = null;
                //
                Stream.Add(GetTitle("Find and Replace", "This tool runs a find and replace operation on content throughout the site."));
                //
                // Process the form
                //
                Button = cpCore.docProperties.getText("button");
                //
                IsDeveloper = cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore);
                if (Button == ButtonFindAndReplace) {
                    RowCnt = cpCore.docProperties.getInteger("CDefRowCnt");
                    if (RowCnt > 0) {
                        for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                            if (cpCore.docProperties.getBoolean("Cdef" + RowPtr)) {
                                lcName = genericController.vbLCase(cpCore.docProperties.getText("CDefName" + RowPtr));
                                if (IsDeveloper || (lcName == "page content") || (lcName == "copy content") || (lcName == "page templates")) {
                                    CDefList = CDefList + "," + lcName;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(CDefList)) {
                            CDefList = CDefList.Substring(1);
                        }
                        FindText = cpCore.docProperties.getText("FindText");
                        ReplaceText = cpCore.docProperties.getText("ReplaceText");
                        QS = "app=" + encodeNvaArgument(cpCore.serverConfig.appConfig.name) + "&FindText=" + encodeNvaArgument(FindText) + "&ReplaceText=" + encodeNvaArgument(ReplaceText) + "&CDefNameList=" + encodeNvaArgument(CDefList);
                        cmdDetailClass cmdDetail = new cmdDetailClass();
                        cmdDetail.addonId = 0;
                        cmdDetail.addonName = "GetForm_FindAndReplace";
                        cmdDetail.docProperties = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, QS);
                        taskSchedulerController.addTaskToQueue(cpCore, taskQueueCommandEnumModule.runAddon, cmdDetail, false);
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
                FindRows = cpCore.docProperties.getInteger("SQLRows");
                if (FindRows == 0) {
                    FindRows = cpCore.userProperty.getInteger("FindAndReplaceFindRows", 1);
                } else {
                    cpCore.userProperty.setProperty("FindAndReplaceFindRows", FindRows.ToString());
                }
                ReplaceRows = cpCore.docProperties.getInteger("ReplaceRows");
                if (ReplaceRows == 0) {
                    ReplaceRows = cpCore.userProperty.getInteger("FindAndReplaceReplaceRows", 1);
                } else {
                    cpCore.userProperty.setProperty("FindAndReplaceReplaceRows", ReplaceRows.ToString());
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
                CS = cpCore.db.csOpen("Content");
                while (cpCore.db.csOk(CS)) {
                    RecordName = cpCore.db.csGetText(CS, "Name");
                    lcName = genericController.vbLCase(RecordName);
                    if (IsDeveloper || (lcName == "page content") || (lcName == "copy content") || (lcName == "page templates")) {
                        RecordID = cpCore.db.csGetInteger(CS, "ID");
                        if (genericController.vbInstr(1, "," + CDefList + ",", "," + RecordName + ",") != 0) {
                            TopHalf = TopHalf + "<div>" + cpCore.html.html_GetFormInputCheckBox2("Cdef" + RowPtr, true) + cpCore.html.html_GetFormInputHidden("CDefName" + RowPtr, RecordName) + "&nbsp;" + cpCore.db.csGetText(CS, "Name") + "</div>";
                        } else {
                            BottomHalf = BottomHalf + "<div>" + cpCore.html.html_GetFormInputCheckBox2("Cdef" + RowPtr, false) + cpCore.html.html_GetFormInputHidden("CDefName" + RowPtr, RecordName) + "&nbsp;" + cpCore.db.csGetText(CS, "Name") + "</div>";
                        }
                    }
                    cpCore.db.csGoNext(CS);
                    RowPtr = RowPtr + 1;
                }
                cpCore.db.csClose(ref CS);
                Stream.Add(TopHalf + BottomHalf + cpCore.html.html_GetFormInputHidden("CDefRowCnt", RowPtr));
                //
                result = htmlController.legacy_openFormTable(cpCore, ButtonCancel + "," + ButtonFindAndReplace) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonCancel + "," + ButtonFindAndReplace);
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //=============================================================================
        // GUID Tools
        //=============================================================================
        //
        private string GetForm_IISReset() {
            string result = "";
            try {
                string Button = null;
                stringBuilderLegacyController s = new stringBuilderLegacyController();
                //
                s.Add(GetTitle("IIS Reset", "Reset the webserver."));
                //
                // Process the form
                //
                Button = cpCore.docProperties.getText("button");
                //
                if (Button == ButtonIISReset) {
                    //
                    //
                    //
                    logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "Resetting IIS", "dll", "ToolsClass", "GetForm_IISReset", 0, "dll", "Warning: member " + cpCore.doc.authContext.user.name + " (" + cpCore.doc.authContext.user.id + ") executed an IISReset using the IISReset tool", false, true, cpCore.webServer.requestUrl, "", "");
                    //runAtServer = New runAtServerClass(cpCore)
                    cpCore.webServer.redirect("/ccLib/Popup/WaitForIISReset.htm");
                    Thread.Sleep(2000);
                    cmdDetailClass cmdDetail = new cmdDetailClass();
                    cmdDetail.addonId = 0;
                    cmdDetail.addonName = "GetForm_IISReset";
                    cmdDetail.docProperties = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, "");
                    taskSchedulerController.addTaskToQueue(cpCore, taskQueueCommandEnumModule.runAddon, cmdDetail, false);
                }
                //
                // Display form
                //
                result = htmlController.legacy_openFormTable(cpCore, ButtonCancel + "," + ButtonIISReset) + s.Text + htmlController.legacy_closeFormTable(cpCore, ButtonCancel + "," + ButtonIISReset);
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
                stringBuilderLegacyController s;
                s = new stringBuilderLegacyController();
                s.Add(GetTitle("Create GUID", "Use this tool to create a GUID. This is useful when creating new Addons."));
                //
                // Process the form
                Button = cpCore.docProperties.getText("button");
                //
                s.Add(cpCore.html.html_GetFormInputText2("GUID", Guid.NewGuid().ToString(), 1, 80));
                //
                // Display form
                result= htmlController.legacy_openFormTable(cpCore, ButtonCancel + "," + ButtonCreateGUID) + s.Text + htmlController.legacy_closeFormTable(cpCore, ButtonCancel + "," + ButtonCreateGUID);
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
        //   throw (New ApplicationException("Unexpected exception"))'cpCore.handleLegacyError("Tools", MethodName, Err.Number, Err.Source, Err.Description, True, False)
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
        //    fixme-- cpCore.handleException(New ApplicationException("")) ' -----ignoreInteger, "App.EXEName", ErrDescription)
        //End Sub




    }
}
