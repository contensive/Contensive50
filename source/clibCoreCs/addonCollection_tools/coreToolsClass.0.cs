using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.Core.Models.Entity;
using Contensive.Core.Models.Context;
using Contensive.Core.Controllers;
using Contensive.Core.Controllers.genericController;
//
namespace Contensive.Core
{
	public class coreToolsClass
	{
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
		private struct DiagActionType
		{
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
		public coreToolsClass(coreClass cpCore) : base()
		{
			this.cpCore = cpCore;
		}
		//
		//========================================================================
		// Print the Tools page
		//========================================================================
		//
		public string GetForm()
		{
				string tempGetForm = null;
			try
			{
				//
				int MenuEntryID = 0;
				int MenuHeaderID = 0;
				int MenuDirection = 0;
				stringBuilderLegacyController Stream = new stringBuilderLegacyController();
				adminUIController Adminui = new adminUIController(cpCore);
				//
				Button = cpCore.docProperties.getText("Button");
				if (Button == ButtonCancelAll)
				{
					//
					// Cancel to the admin site
					//
					return cpCore.webServer.redirect(cpCore.siteProperties.getText("AdminURL", "/admin/"));
				}
				//
				// ----- Check permissions
				//
				if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
				{
					//
					// You must be admin to use this feature
					//
					tempGetForm = Adminui.GetFormBodyAdminOnly();
					tempGetForm = Adminui.GetBody("Admin Tools", ButtonCancelAll, "", false, false, "<div>Administration Tools</div>", "", 0, tempGetForm);
				}
				else
				{
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
					if (Button == ButtonCancel)
					{
						//
						// -- Cancel
						return cpCore.webServer.redirect(cpCore.siteProperties.getText("AdminURL", "/admin/"));
					}
					else
					{
						//
						// -- Print out the page
						switch (AdminFormTool)
						{
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
								Stream.Add(GetForm_Benchmark);
								break;
							case AdminFormToolClearContentWatchLink:
								//
								Stream.Add(GetForm_ClearContentWatchLinks());
								break;
							case AdminFormToolSchema:
								//
								Stream.Add(GetForm_DbSchema);
								break;
							case AdminFormToolContentFileView:
								//
								Stream.Add(GetForm_ContentFileManager);
								break;
							case AdminFormToolWebsiteFileView:
								//
								Stream.Add(GetForm_WebsiteFileManager);
								break;
							case AdminFormToolDbIndex:
								//
								Stream.Add(GetForm_DbIndex);
								break;
							case AdminFormToolContentDbSchema:
								//
								Stream.Add(GetForm_ContentDbSchema);
								break;
							case AdminFormToolLogFileView:
								//
								Stream.Add(GetForm_LogFiles);
								break;
							case AdminFormToolLoadCDef:
								//
								Stream.Add(GetForm_LoadCDef);
								break;
							case AdminFormToolRestart:
								//
								Stream.Add(GetForm_Restart);
								break;
							case AdminFormToolLoadTemplates:
								//
								Stream.Add(GetForm_LoadTemplates);
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
				return tempGetForm;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // throw (New ApplicationException("Unexpected exception"))' Call handleLegacyClassErrors1("GetForm", "ErrorTrap")
			return tempGetForm;
		}
		//
		//=============================================================================
		//   Remove all Content Fields and rebuild them from the fields found in a table
		//=============================================================================
		//
		private string GetForm_DefineContentFieldsFromTable()
		{
			try
			{
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
				while (cpCore.db.csOk(CSPointer))
				{
					Stream.Add("<option value=\"" + cpCore.db.csGetText(CSPointer, "name") + "\">" + cpCore.db.csGetText(CSPointer, "name") + "</option>");
					ItemCount = ItemCount + 1;
					cpCore.db.csGoNext(CSPointer);
				}
				if (ItemCount == 0)
				{
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
				if (Button == ButtonCreateFields)
				{
					ContentName = cpCore.docProperties.getText("ContentName");
					if (string.IsNullOrEmpty(ContentName))
					{
						Stream.Add("Select a content before submitting. Fields were not changed.");
					}
					else
					{
						ContentID = Local_GetContentID(ContentName);
						if (ContentID == 0)
						{
							Stream.Add("GetContentID failed. Fields were not changed.");
						}
						else
						{
							cpCore.db.deleteContentRecords("Content Fields", "ContentID=" + cpCore.db.encodeSQLNumber(ContentID));
						}
					}
				}
				//
				return htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // throw (New ApplicationException("Unexpected exception"))' Call handleLegacyClassErrors1("GetForm_DefineContentFieldsFromTable", "ErrorTrap")
		}
		//
		private class fieldSortClass
		{
			public string sort;
			public Models.Complex.CDefFieldModel field;
		}
		//
		//=============================================================================
		//   prints a menu of Tools in a 100% table
		//=============================================================================
		//
		private string GetForm_Root()
		{
			try
			{
				//
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
				return htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // throw (New ApplicationException("Unexpected exception"))' Call handleLegacyClassErrors1("GetForm_Root", "ErrorTrap")
		}
		//
		//==================================================================================
		//
		//==================================================================================
		//
		private string GetForm_RootRow(int AdminFormId, int AdminFormToolId, string Caption, string Description)
		{
				string tempGetForm_RootRow = null;
			try
			{
				//
				tempGetForm_RootRow = "";
				tempGetForm_RootRow = tempGetForm_RootRow + "<tr><td colspan=\"2\">";
				tempGetForm_RootRow = tempGetForm_RootRow + SpanClassAdminNormal + "<P class=\"ccAdminNormal\"><A href=\"" + cpCore.webServer.requestPage + "?af=" + AdminFormToolId.ToString() + "\"><B>" + Caption + "</b></SPAN></A></p>";
				tempGetForm_RootRow = tempGetForm_RootRow + "</td></tr>";
				if (!string.IsNullOrEmpty(Description))
				{
					tempGetForm_RootRow = tempGetForm_RootRow + "<tr><td width=\"30\"><img src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"30\"></td>";
					tempGetForm_RootRow = tempGetForm_RootRow + "<td width=\"100%\"><P class=\"ccAdminsmall\">" + Description + "</p></td></tr>";
				}
				return tempGetForm_RootRow;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // throw (New ApplicationException("Unexpected exception"))' Call handleLegacyClassErrors1("GetForm_RootRow", "ErrorTrap")
			return tempGetForm_RootRow;
		}
		//
		//==================================================================================
		//
		//==================================================================================
		//
		private string GetTitle(string Title, string Description)
		{
			string result = "";
			try
			{
				result = ""
					+ "<p>" + SpanClassAdminNormal + "<a href=\"" + cpCore.webServer.requestPage + "?af=" + AdminFormToolRoot + "\"><b>Tools</b></a>&nbsp;&gt;&nbsp;" + Title + "</p>"
					+ "<p>" + SpanClassAdminNormal + Description + "</p>";
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		//=============================================================================
		//   Print the manual query form
		//=============================================================================
		//
		private string GetForm_ManualQuery()
		{
			string returnHtml = "";
			try
			{
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
				dataSourceModel datasource = dataSourceModel.create(cpCore, cpCore.docProperties.getInteger("dataSourceid"), new List<string>());
				//
				ButtonList = ButtonCancel + "," + ButtonRun;
				//
				Stream.Add(GetTitle("Run Manual Query", "This tool runs an SQL statement on a selected datasource. If there is a result set, the set is printed in a table."));
				//
				// Get the members SQL Queue
				//
				SQLFilename = cpCore.userProperty.getText("SQLArchive");
				if (string.IsNullOrEmpty(SQLFilename))
				{
					SQLFilename = "SQLArchive" + cpCore.doc.authContext.user.id.ToString("000000000") + ".txt";
					cpCore.userProperty.setProperty("SQLArchive", SQLFilename);
				}
				SQLArchive = cpCore.cdnFiles.readFile(SQLFilename);
				//
				// Read in arguments if available
				//
				Timeout = cpCore.docProperties.getInteger("Timeout");
				if (Timeout == 0)
				{
					Timeout = 30;
				}
				//
				PageSize = cpCore.docProperties.getInteger("PageSize");
				if (PageSize == 0)
				{
					PageSize = 10;
				}
				//
				PageNumber = cpCore.docProperties.getInteger("PageNumber");
				if (PageNumber == 0)
				{
					PageNumber = 1;
				}
				//
				SQL = cpCore.docProperties.getText("SQL");
				if (string.IsNullOrEmpty(SQL))
				{
					SQL = cpCore.docProperties.getText("SQLList");
				}
				//
				if ((cpCore.docProperties.getText("button")) == ButtonRun)
				{
					//
					// Add this SQL to the members SQL list
					//
					if (!string.IsNullOrEmpty(SQL))
					{
						SQLArchive = genericController.vbReplace(SQLArchive, SQL + Environment.NewLine, "");
						SQLArchiveOld = SQLArchive;
						SQLArchive = genericController.vbReplace(SQL, Environment.NewLine, " ") + Environment.NewLine;
						LineCounter = 0;
						while ((LineCounter < 10) && (!string.IsNullOrEmpty(SQLArchiveOld)))
						{
							SQLArchive = SQLArchive + getLine(SQLArchiveOld) + Environment.NewLine;
						}
						cpCore.appRootFiles.saveFile(SQLFilename, SQLArchive);
					}
					//
					// Run the SQL
					//
					Stream.Add("<P>" + SpanClassAdminSmall);
					Stream.Add(DateTime.Now + " Executing sql [" + SQL + "] on DataSource [" + datasource.Name + "]");
					try
					{
						dt = cpCore.db.executeQuery(SQL, datasource.Name, PageSize * (PageNumber - 1), PageSize);
					}
					catch (Exception ex)
					{
						//
						// ----- error
						//
						Stream.Add("<br>" + DateTime.Now + " SQL execution returned the following error");
						Stream.Add("<br>" + ex.Message);
					}
					Stream.Add("<br>" + DateTime.Now + " SQL executed successfully");
					if (dt == null)
					{
						Stream.Add("<br>" + DateTime.Now + " SQL returned invalid data.");
					}
					else if (dt.Rows == null)
					{
						Stream.Add("<br>" + DateTime.Now + " SQL returned invalid data rows.");
					}
					else if (dt.Rows.Count == 0)
					{
						Stream.Add("<br>" + DateTime.Now + " The SQL returned no data.");
					}
					else
					{
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
						foreach (DataColumn dc in dt.Columns)
						{
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
						for (RowPointer = 0; RowPointer <= RowMax; RowPointer++)
						{
							Stream.Add(RowStart);
							for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++)
							{
								CellData = resultArray[ColumnPointer, RowPointer];
								if (IsNull(CellData))
								{
									Stream.Add(ColumnStart + "[null]" + ColumnEnd);
									//ElseIf IsEmpty(CellData) Then
									//    Stream.Add(ColumnStart & "[empty]" & ColumnEnd)
									//ElseIf IsArray(CellData) Then
									//    Stream.Add(ColumnStart & "[array]")
									//    Cnt = UBound(CellData)
									//    For Ptr = 0 To Cnt - 1
									//        Stream.Add("<br>(" & Ptr & ")&nbsp;[" & CellData(Ptr) & "]")
									//    Next
									//    Stream.Add(ColumnEnd)
								}
								else if (string.IsNullOrEmpty(CellData))
								{
									Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
								}
								else
								{
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
				if (SQLRows == 0)
				{
					SQLRows = cpCore.userProperty.getInteger("ManualQueryInputRows", 5);
				}
				else
				{
					cpCore.userProperty.setProperty("ManualQueryInputRows", SQLRows.ToString());
				}
				Stream.Add("<TEXTAREA NAME=\"SQL\" ROWS=\"" + SQLRows + "\" ID=\"SQL\" STYLE=\"width: 800px;\">" + SQL + "</TEXTAREA>");
				Stream.Add("&nbsp;<INPUT TYPE=\"Text\" TabIndex=-1 NAME=\"SQLRows\" SIZE=\"3\" VALUE=\"" + SQLRows + "\" ID=\"\"  onchange=\"SQL.rows=SQLRows.value; return true\"> Rows");
				Stream.Add("<br><br>Data Source<br>" + cpCore.html.main_GetFormInputSelect("DataSourceID", datasource.ID, "Data Sources", "", "Default"));
				//
				SelectFieldWidthLimit = cpCore.siteProperties.getinteger("SelectFieldWidthLimit", 200);
				if (!string.IsNullOrEmpty(SQLArchive))
				{
					Stream.Add("<br><br>Previous Queries<br>");
					Stream.Add("<select size=\"1\" name=\"SQLList\" ID=\"SQLList\" onChange=\"SQL.value=SQLList.value\">");
					Stream.Add("<option value=\"\">Select One</option>");
					LineCounter = 0;
					while ((LineCounter < 10) && (!string.IsNullOrEmpty(SQLArchive)))
					{
						SQLLine = getLine(SQLArchive);
						if (SQLLine.Length > SelectFieldWidthLimit)
						{
							SQLName = SQLLine.Substring(0, SelectFieldWidthLimit) + "...";
						}
						else
						{
							SQLName = SQLLine;
						}
						Stream.Add("<option value=\"" + SQLLine + "\">" + SQLName + "</option>");
					}
					Stream.Add("</select><br>");
				}
				//
				if (IsNull(PageSize))
				{
					PageSize = 100;
				}
				Stream.Add("<br>Page Size:<br>" + cpCore.html.html_GetFormInputText("PageSize", PageSize.ToString()));
				//
				if (IsNull(PageNumber))
				{
					PageNumber = 1;
				}
				Stream.Add("<br>Page Number:<br>" + cpCore.html.html_GetFormInputText("PageNumber", PageNumber.ToString()));
				//
				if (IsNull(Timeout))
				{
					Timeout = 30;
				}
				Stream.Add("<br>Timeout (sec):<br>" + cpCore.html.html_GetFormInputText("Timeout", Timeout.ToString()));
				//
				//Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolManualQuery))
				//Stream.Add( cpCore.main_GetFormEnd & "</SPAN>")
				//
				returnHtml = htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
			}
			catch (Exception ex)
			{
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
		private string GetForm_CreateContentDefinition()
		{
			try
			{
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
				dataSourceModel datasource = dataSourceModel.create(cpCore, cpCore.docProperties.getInteger("DataSourceID"), new List<string>());
				//
				ButtonList = ButtonCancel + "," + ButtonRun;
				Caption = "Create Content Definition";
				Description = "This tool creates a Content Definition. If the SQL table exists, it is used. If it does not exist, it is created. If records exist in the table with a blank ContentControlID, the ContentControlID will be populated from this new definition. A Navigator Menu entry will be added under Manage Site Content - Advanced.";
				//
				//Stream.Add( GetTitle(Caption, Description)
				//
				//   print out the submit form
				//
				if (cpCore.docProperties.getText("Button") != "")
				{
					//
					// Process input
					//
					ContentName = cpCore.docProperties.getText("ContentName");
					TableName = cpCore.docProperties.getText("TableName");
					//
					Stream.Add(SpanClassAdminSmall);
					Stream.Add("<P>Creating content [" + ContentName + "] on table [" + TableName + "] on Datasource [" + datasource.Name + "].</P>");
					if ((!string.IsNullOrEmpty(ContentName)) & (!string.IsNullOrEmpty(TableName)) & (!string.IsNullOrEmpty(datasource.Name)))
					{
						cpCore.db.createSQLTable(datasource.Name, TableName);
						cpCore.db.createContentFromSQLTable(datasource, TableName, ContentName);
						cpCore.cache.invalidateAll();
						cpCore.doc.clearMetaData();
						ContentID = Models.Complex.cdefModel.getContentId(cpCore, ContentName);
						ParentNavID = cpCore.db.getRecordID(cnNavigatorEntries, "Manage Site Content");
						if (ParentNavID != 0)
						{
							CS = cpCore.db.csOpen(cnNavigatorEntries, "(name=" + cpCore.db.encodeSQLText("Advanced") + ")and(parentid=" + ParentNavID + ")");
							ParentNavID = 0;
							if (cpCore.db.csOk(CS))
							{
								ParentNavID = cpCore.db.csGetInteger(CS, "ID");
							}
							cpCore.db.csClose(CS);
							if (ParentNavID != 0)
							{
								CS = cpCore.db.csOpen(cnNavigatorEntries, "(name=" + cpCore.db.encodeSQLText(ContentName) + ")and(parentid=" + NavID + ")");
								if (!cpCore.db.csOk(CS))
								{
									cpCore.db.csClose(CS);
									CS = cpCore.db.csInsertRecord(cnNavigatorEntries);
								}
								if (cpCore.db.csOk(CS))
								{
									cpCore.db.csSet(CS, "name", ContentName);
									cpCore.db.csSet(CS, "parentid", ParentNavID);
									cpCore.db.csSet(CS, "contentid", ContentID);
								}
								cpCore.db.csClose(CS);
							}
						}
						ContentID = Models.Complex.cdefModel.getContentId(cpCore, ContentName);
						Stream.Add("<P>Content Definition was created. An admin menu entry for this definition has been added under 'Site Content', and will be visible on the next page view. Use the [<a href=\"?af=105&ContentID=" + ContentID + "\">Edit Content Definition Fields</a>] tool to review and edit this definition's fields.</P>");
					}
					else
					{
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
				//Call Stream.Add(cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolCreateContentDefinition))
				Stream.Add("</SPAN>");
				//
				return Adminui.GetBody(Caption, ButtonList, "", false, false, Description, "", 10, Stream.Text);
				//GetForm_CreateContentDefinition = genericLegacyView.OpenFormTable(cpcore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpcore,ButtonList)

				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // throw (New ApplicationException("Unexpected exception"))' Call handleLegacyClassErrors1("GetForm_CreateContentDefinition", "ErrorTrap")
		}
		//
		//=============================================================================
		//   Print the Configure Listing Page
		//=============================================================================
		//
		private string GetForm_ConfigureListing()
		{
			try
			{
				//
				string SQL = null;
				string MenuHeader = null;
				//Dim ColumnCount As Integer
				int ColumnWidth = 0;
				// converted array to dictionary - Dim FieldPointer As Integer
				string FieldName = null;
				int FieldToAdd = 0;
				string AStart = null;
				int CS = 0;
				bool SetSort = false;
				int MenuEntryID = 0;
				int MenuHeaderID = 0;
				int MenuDirection = 0;
				int SourceID = 0;
				int PreviousID = 0;
				int SetID = 0;
				int NextSetID = 0;
				bool SwapWithPrevious = false;
				int HitID = 0;
				string HitTable = null;
				int SortPriorityLowest = 0;
				string TempColumn = null;
				string Tempfield = null;
				string TempWidth = null;
				int TempSortPriority = 0;
				int TempSortDirection = 0;
				int CSPointer = 0;
				int RecordID = 0;
				int ContentID = 0;
				Models.Complex.cdefModel CDef = null;
				//Dim AdminColumn As appServices_metaDataClass.CDefAdminColumnClass
				int[] RowFieldID = null;
				int[] RowFieldWidth = null;
				string[] RowFieldCaption = null;
				//Dim RowFieldCount as integer
				int[] NonRowFieldID = null;
				string[] NonRowFieldCaption = null;
				int NonRowFieldCount = 0;
				string ContentName = null;
				//
				DataTable dt = null;
				int IndexWidth = 0;
				int CS1 = 0;
				int CS2 = 0;
				// Dim FieldPointer1 As Integer
				int FieldPointer2 = 0;
				int NewRowFieldWidth = 0;
				int TargetFieldID = 0;
				//
				int ColumnWidthTotal = 0;
				int ColumnNumberMax = 0;
				//
				//Dim ColumnPointer As Integer
				//Dim CDefFieldCount As Integer
				int fieldId = 0;
				int FieldWidth = 0;
				bool AllowContentAutoLoad = false;
				int TargetFieldPointer = 0;
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
				//Dim ContentNameValues() As NameValuePrivateType
				int ContentCount = 0;
				int ContentSize = 0;
				stringBuilderLegacyController Stream = new stringBuilderLegacyController();
				string ButtonList = null;
				string FormPanel = "";
				int ColumnWidthIncrease = 0;
				int ColumnWidthBalance = 0;
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
				if (ContentID != 0)
				{
					ButtonList = ButtonCancel + "," + ButtonSaveandInvalidateCache;
					ContentName = Local_GetContentNameByID(ContentID);
					CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentID, true, false);
					if (ToolsAction != 0)
					{
						//
						// Block contentautoload, then force a load at the end
						//
						AllowContentAutoLoad = (cpCore.siteProperties.getBoolean("AllowContentAutoLoad", true));
						cpCore.siteProperties.setProperty("AllowContentAutoLoad", false);
						//
						// Make sure the FieldNameToAdd is not-inherited, if not, create new field
						//
						if (FieldIDToAdd != 0)
						{
							foreach (var keyValuePair in CDef.fields)
							{
								Models.Complex.CDefFieldModel field = keyValuePair.Value;
								if (field.id == FieldIDToAdd)
								{
									//If field.Name = FieldNameToAdd Then
									if (field.inherited)
									{
										SourceContentID = field.contentId;
										SourceName = field.nameLc;
										CSSource = cpCore.db.csOpen("Content Fields", "(ContentID=" + SourceContentID + ")and(Name=" + cpCore.db.encodeSQLText(SourceName) + ")");
										if (cpCore.db.csOk(CSSource))
										{
											CSTarget = cpCore.db.csInsertRecord("Content Fields");
											if (cpCore.db.csOk(CSTarget))
											{
												cpCore.db.csCopyRecord(CSSource, CSTarget);
												cpCore.db.csSet(CSTarget, "ContentID", ContentID);
												ReloadCDef = true;
											}
											cpCore.db.csClose(CSTarget);
										}
										cpCore.db.csClose(CSSource);
									}
									break;
								}
							}
						}
						//
						// Make sure all fields are not-inherited, if not, create new fields
						//
						ColumnNumberMax = 0;
						foreach (var keyValuePair in CDef.adminColumns)
						{
							Models.Complex.cdefModel.CDefAdminColumnClass adminColumn = keyValuePair.Value;
							Models.Complex.CDefFieldModel field = CDef.fields(adminColumn.Name);
							if (field.inherited)
							{
								SourceContentID = field.contentId;
								SourceName = field.nameLc;
								CSSource = cpCore.db.csOpen("Content Fields", "(ContentID=" + SourceContentID + ")and(Name=" + cpCore.db.encodeSQLText(SourceName) + ")");
								if (cpCore.db.csOk(CSSource))
								{
									CSTarget = cpCore.db.csInsertRecord("Content Fields");
									if (cpCore.db.csOk(CSTarget))
									{
										cpCore.db.csCopyRecord(CSSource, CSTarget);
										cpCore.db.csSet(CSTarget, "ContentID", ContentID);
										ReloadCDef = true;
									}
									cpCore.db.csClose(CSTarget);
								}
								cpCore.db.csClose(CSSource);
							}
							if (ColumnNumberMax < field.indexColumn)
							{
								ColumnNumberMax = field.indexColumn;
							}
							ColumnWidthTotal = ColumnWidthTotal + adminColumn.Width;
						}
						//
						// ----- Perform any actions first
						//
						int columnPtr = 0;
						switch (ToolsAction)
						{
							case ToolsActionAddField:
							{
								//
								// Add a field to the Listing Page
								//
								if (FieldIDToAdd != 0)
								{
									//If FieldNameToAdd <> "" Then
									columnPtr = 0;
									if (CDef.adminColumns.Count > 1)
									{
										foreach (var keyValuePair in CDef.adminColumns)
										{
											Models.Complex.cdefModel.CDefAdminColumnClass adminColumn = keyValuePair.Value;
											Models.Complex.CDefFieldModel field = CDef.fields(adminColumn.Name);
											CSPointer = cpCore.db.csOpenRecord("Content Fields", field.id);
											cpCore.db.csSet(CSPointer, "IndexColumn", (columnPtr) * 10);
											cpCore.db.csSet(CSPointer, "IndexWidth", Math.Floor((adminColumn.Width * 80) / (double)ColumnWidthTotal));
											cpCore.db.csClose(CSPointer);
											columnPtr += 1;
										}
									}
									CSPointer = cpCore.db.csOpenRecord("Content Fields", FieldIDToAdd, false, false);
									if (cpCore.db.csOk(CSPointer))
									{
										cpCore.db.csSet(CSPointer, "IndexColumn", columnPtr * 10);
										cpCore.db.csSet(CSPointer, "IndexWidth", 20);
										cpCore.db.csSet(CSPointer, "IndexSortPriority", 99);
										cpCore.db.csSet(CSPointer, "IndexSortDirection", 1);
									}
									cpCore.db.csClose(CSPointer);
									ReloadCDef = true;
								}
								//
								break;
							}
							case ToolsActionRemoveField:
							{
								//
								// Remove a field to the Listing Page
								//
								if (CDef.adminColumns.Count > 1)
								{
									columnPtr = 0;
									foreach (var keyValuePair in CDef.adminColumns)
									{
										Models.Complex.cdefModel.CDefAdminColumnClass adminColumn = keyValuePair.Value;
										Models.Complex.CDefFieldModel field = CDef.fields(adminColumn.Name);
										CSPointer = cpCore.db.csOpenRecord("Content Fields", field.id);
										if (fieldId == TargetFieldID)
										{
											cpCore.db.csSet(CSPointer, "IndexColumn", 0);
											cpCore.db.csSet(CSPointer, "IndexWidth", 0);
											cpCore.db.csSet(CSPointer, "IndexSortPriority", 0);
											cpCore.db.csSet(CSPointer, "IndexSortDirection", 0);
										}
										else
										{
											cpCore.db.csSet(CSPointer, "IndexColumn", (columnPtr) * 10);
											cpCore.db.csSet(CSPointer, "IndexWidth", Math.Floor((adminColumn.Width * 100) / (double)ColumnWidthTotal));
										}
										cpCore.db.csClose(CSPointer);
										columnPtr += 1;
									}
									ReloadCDef = true;
								}
								break;
							}
							case ToolsActionMoveFieldRight:
							{
								//
								// Move column field right
								//
								if (CDef.adminColumns.Count > 1)
								{
									MoveNextColumn = false;
									columnPtr = 0;
									foreach (var keyValuePair in CDef.adminColumns)
									{
										Models.Complex.cdefModel.CDefAdminColumnClass adminColumn = keyValuePair.Value;
										Models.Complex.CDefFieldModel field = CDef.fields(adminColumn.Name);
										FieldName = adminColumn.Name;
										CS1 = cpCore.db.csOpenRecord("Content Fields", field.id);
										if ((CDef.fields(FieldName.ToLower()).id == TargetFieldID) && (columnPtr < CDef.adminColumns.Count))
										{
											cpCore.db.csSet(CS1, "IndexColumn", (columnPtr + 1) * 10);
											//
											MoveNextColumn = true;
										}
										else if (MoveNextColumn)
										{
											//
											// This is one past target
											//
											cpCore.db.csSet(CS1, "IndexColumn", (columnPtr - 1) * 10);
											MoveNextColumn = false;
										}
										else
										{
											//
											// not target or one past target
											//
											cpCore.db.csSet(CS1, "IndexColumn", (columnPtr) * 10);
											MoveNextColumn = false;
										}
										cpCore.db.csSet(CS1, "IndexWidth", Math.Floor((adminColumn.Width * 100) / (double)ColumnWidthTotal));
										cpCore.db.csClose(CS1);
										columnPtr += 1;
									}
									ReloadCDef = true;
								}
								// end case
								break;
							}
							case ToolsActionMoveFieldLeft:
							{
								//
								// Move Index column field left
								//
								if (CDef.adminColumns.Count > 1)
								{
									MoveNextColumn = false;
									columnPtr = 0;
									foreach (var keyValuePair in CDef.adminColumns.Reverse)
									{
										Models.Complex.cdefModel.CDefAdminColumnClass adminColumn = keyValuePair.Value;
										Models.Complex.CDefFieldModel field = CDef.fields(adminColumn.Name);
										FieldName = adminColumn.Name;
										CS1 = cpCore.db.csOpenRecord("Content Fields", field.id);
										if ((field.id == TargetFieldID) && (columnPtr < CDef.adminColumns.Count))
										{
											cpCore.db.csSet(CS1, "IndexColumn", (columnPtr - 1) * 10);
											//
											MoveNextColumn = true;
										}
										else if (MoveNextColumn)
										{
											//
											// This is one past target
											//
											cpCore.db.csSet(CS1, "IndexColumn", (columnPtr + 1) * 10);
											MoveNextColumn = false;
										}
										else
										{
											//
											// not target or one past target
											//
											cpCore.db.csSet(CS1, "IndexColumn", (columnPtr) * 10);
											MoveNextColumn = false;
										}
										cpCore.db.csSet(CS1, "IndexWidth", Math.Floor((adminColumn.Width * 100) / (double)ColumnWidthTotal));
										cpCore.db.csClose(CS1);
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
								//            fieldId = CDef.fields(FieldName.ToLower()).Id
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
								//            fieldId = CDef.fields(FieldName.ToLower()).Id
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
								//            fieldId = CDef.fields(FieldName.ToLower()).Id
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
								//                fieldId = CDef.fields(FieldName.ToLower()).Id
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
								//            fieldId = CDef.fields(FieldName.ToLower()).Id
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
								//                fieldId = CDef.fields(FieldName.ToLower()).Id
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
					if (Button == ButtonSaveandInvalidateCache)
					{
						cpCore.cache.invalidateAll();
						cpCore.doc.clearMetaData();
						return cpCore.webServer.redirect("?af=" + AdminFormToolConfigureListing + "&ContentID=" + ContentID);
					}
					//
					//--------------------------------------------------------------------------------
					//   Display the form
					//--------------------------------------------------------------------------------
					//
					if (!string.IsNullOrEmpty(ContentName))
					{
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
					if (CDef.adminColumns.Count > 0)
					{
						//
						// Calc total width
						//
						foreach (KeyValuePair<string, Models.Complex.cdefModel.CDefAdminColumnClass> kvp in CDef.adminColumns)
						{
							ColumnWidthTotal += kvp.Value.Width;
						}
						//For ColumnCount = 0 To CDef.adminColumns.Count - 1
						//    ColumnWidthTotal = ColumnWidthTotal + CDef.adminColumns(ColumnCount).Width
						//Next
						if (ColumnWidthTotal > 0)
						{
							Stream.Add("<table border=\"0\" cellpadding=\"5\" cellspacing=\"0\" width=\"90%\">");
							int ColumnCount = 0;
							foreach (KeyValuePair<string, Models.Complex.cdefModel.CDefAdminColumnClass> kvp in CDef.adminColumns)
							{
								//
								// print column headers - anchored so they sort columns
								//
								ColumnWidth = Convert.ToInt32(100 * (kvp.Value.Width / (double)ColumnWidthTotal));
								FieldName = kvp.Value.Name;
								var tempVar = CDef.fields(FieldName.ToLower());
								fieldId = tempVar.id;
								Caption = tempVar.caption;
								if (tempVar.inherited)
								{
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
					if (InheritedFieldCount > 0)
					{
						Stream.Add("<P class=\"ccNormal\">* This field was inherited from the Content Definition's Parent. Inherited fields will automatically change when the field in the parent is changed. If you alter these settings, this connection will be broken, and the field will no longer inherit it's properties.</P class=\"ccNormal\">");
					}
					//
					// ----- now output a list of fields to add
					//
					if (CDef.fields.Count == 0)
					{
						Stream.Add(SpanClassAdminNormal + "This Content Definition has no fields</SPAN><br>");
					}
					else
					{
						Stream.Add(SpanClassAdminNormal + "<br>");
						bool skipField = false;
						foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in CDef.fields)
						{
							Models.Complex.CDefFieldModel field = keyValuePair.Value;
							//
							// test if this column is in use
							//
							skipField = false;
							//ColumnPointer = CDef.adminColumns.Count
							if (CDef.adminColumns.Count > 0)
							{
								foreach (KeyValuePair<string, Models.Complex.cdefModel.CDefAdminColumnClass> kvp in CDef.adminColumns)
								{
									if (field.nameLc == kvp.Value.Name)
									{
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
							if (skipField)
							{
								if (false)
								{
									// this causes more problems then it fixes
									//If Not .Authorable Then
									//
									// not authorable
									//
									Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (not authorable field)<br>");
								}
								else if (field.fieldTypeId == FieldTypeIdFileText)
								{
									//
									// text filename can not be search
									//
									Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (text file field)<br>");
								}
								else if (field.fieldTypeId == FieldTypeIdFileCSS)
								{
									//
									// text filename can not be search
									//
									Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (css file field)<br>");
								}
								else if (field.fieldTypeId == FieldTypeIdFileXML)
								{
									//
									// text filename can not be search
									//
									Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (xml file field)<br>");
								}
								else if (field.fieldTypeId == FieldTypeIdFileJavascript)
								{
									//
									// text filename can not be search
									//
									Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (javascript file field)<br>");
								}
								else if (field.fieldTypeId == FieldTypeIdLongText)
								{
									//
									// long text can not be search
									//
									Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (long text field)<br>");
								}
								else if (field.fieldTypeId == FieldTypeIdFileImage)
								{
									//
									// long text can not be search
									//
									Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (image field)<br>");
								}
								else if (field.fieldTypeId == FieldTypeIdRedirect)
								{
									//
									// long text can not be search
									//
									Stream.Add("<IMG src=\"/ccLib/images/Spacer.gif\" width=\"50\" height=\"15\" border=\"0\"> " + field.caption + " (redirect field)<br>");
								}
								else
								{
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
				Stream.Add(cpCore.html.main_GetPanel(FormPanel));
				//
				cpCore.siteProperties.setProperty("AllowContentAutoLoad", AllowContentAutoLoad);
				Stream.Add(cpCore.html.html_GetFormInputHidden("ReloadCDef", ReloadCDef));

				return htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
				//
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("GetForm_ConfigureListing", "ErrorTrap")
		}
		//
		//=============================================================================
		// checks Content against Database tables
		//=============================================================================
		//
		private string GetForm_ContentDiagnostic()
		{
			try
			{
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
				if ((iDiagActionCount != 0) & ((Button == ButtonFix) || (Button == ButtonFixAndRun)))
				{
					//
					//-----------------------------------------------------------------------------------------------
					// ----- Perform actions from previous Diagnostic
					//-----------------------------------------------------------------------------------------------
					//
					Stream.Add("<br>");
					for (DiagActionPointer = 0; DiagActionPointer <= iDiagActionCount; DiagActionPointer++)
					{
						DiagAction = cpCore.docProperties.getText("DiagAction" + DiagActionPointer);
						Stream.Add("Perform Action " + DiagActionPointer + " - " + DiagAction + "<br>");
						switch (genericController.EncodeInteger(DiagArgument(DiagAction, 0)))
						{
							case DiagActionSetFieldType:
								//
								// ----- Set Field Type
								//
								ContentID = Local_GetContentID(DiagArgument(DiagAction, 1));
								CS = cpCore.db.csOpen("Content Fields", "(ContentID=" + ContentID + ")and(Name=" + cpCore.db.encodeSQLText(DiagArgument(DiagAction, 2)) + ")");
								if (cpCore.db.csOk(CS))
								{
									cpCore.db.csSet(CS, "Type", DiagArgument(DiagAction, 3));
								}
								cpCore.db.csClose(CS);
								//end case
								break;
							case DiagActionSetFieldInactive:
								//
								// ----- Set Field Inactive
								//
								ContentID = Local_GetContentID(DiagArgument(DiagAction, 1));
								CS = cpCore.db.csOpen("Content Fields", "(ContentID=" + ContentID + ")and(Name=" + cpCore.db.encodeSQLText(DiagArgument(DiagAction, 2)) + ")");
								if (cpCore.db.csOk(CS))
								{
									cpCore.db.csSet(CS, "active", 0);
								}
								cpCore.db.csClose(CS);
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
								if (cpCore.db.csOk(CS))
								{
									cpCore.db.csGoNext(CS);
									while (cpCore.db.csOk(CS))
									{
										cpCore.db.csSet(CS, "active", 0);
										cpCore.db.csGoNext(CS);
									}
								}
								cpCore.db.csClose(CS);
								//end case
								break;
							case DiagActionSetRecordInactive:
								//
								// ----- Set Field Inactive
								//
								ContentName = DiagArgument(DiagAction, 1);
								RecordID = genericController.EncodeInteger(DiagArgument(DiagAction, 2));
								CS = cpCore.db.csOpen(ContentName, "(ID=" + RecordID + ")");
								if (cpCore.db.csOk(CS))
								{
									cpCore.db.csSet(CS, "active", 0);
								}
								cpCore.db.csClose(CS);
								//end case
								break;
							case DiagActionSetFieldNotRequired:
								//
								// ----- Set Field not-required
								//
								ContentName = DiagArgument(DiagAction, 1);
								RecordID = genericController.EncodeInteger(DiagArgument(DiagAction, 2));
								CS = cpCore.db.csOpen(ContentName, "(ID=" + RecordID + ")");
								if (cpCore.db.csOk(CS))
								{
									cpCore.db.csSet(CS, "required", 0);
								}
								cpCore.db.csClose(CS);
								//end case
								break;
						}
					}
				}
				if ((Button == ButtonRun) || (Button == ButtonFixAndRun))
				{
					//
					// Process input
					//
					if (!cpCore.siteProperties.trapErrors)
					{
						//
						// TrapErrors must be true to run this tools
						//
						Stream.Add("Site Property 'TrapErrors' is currently set false. This property must be true to run Content Diagnostics successfully.<br>");
					}
					else
					{
						cpCore.html.enableOutputBuffer(false);
						//
						// ----- check Content Sources for duplicates
						//
						if (DiagActionCount < DiagActionCountMax)
						{
							Stream.Add(GetDiagHeader("Checking Content Definition Duplicates...<br>"));
							//
							SQL = "SELECT Count(ccContent.ID) AS RecordCount, ccContent.Name AS Name, ccContent.Active"
									+ " From ccContent"
									+ " GROUP BY ccContent.Name, ccContent.Active"
									+ " Having (((Count(ccContent.ID)) > 1) And ((ccContent.active) <> 0))"
									+ " ORDER BY Count(ccContent.ID) DESC;";
							CSPointer = cpCore.db.csOpenSql_rev("Default", SQL);
							if (cpCore.db.csOk(CSPointer))
							{
								while (cpCore.db.csOk(CSPointer))
								{
									DiagProblem = "PROBLEM: There are " + cpCore.db.csGetText(CSPointer, "RecordCount") + " records in the Content table with the name [" + cpCore.db.csGetText(CSPointer, "Name") + "]";
									DiagActions = new Contensive.Core.coreToolsClass.DiagActionType[3];
									DiagActions[0].Name = "Ignore, or handle this issue manually";
									DiagActions[0].Command = "";
									DiagActions[1].Name = "Mark all duplicate definitions inactive";
									DiagActions[1].Command = DiagActionContentDeDupe.ToString() + "," + cpCore.db.cs_getValue(CSPointer, "name");
									Stream.Add(GetDiagError(DiagProblem, DiagActions));
									cpCore.db.csGoNext(CSPointer);
								}
							}
							cpCore.db.csClose(CSPointer);
						}
						//
						// ----- Content Fields
						//
						if (DiagActionCount < DiagActionCountMax)
						{
							Stream.Add(GetDiagHeader("Checking Content Fields...<br>"));
							//
							SQL = "SELECT ccFields.required AS FieldRequired, ccFields.Authorable AS FieldAuthorable, ccFields.Type AS FieldType, ccFields.Name AS FieldName, ccContent.ID AS ContentID, ccContent.Name AS ContentName, ccTables.Name AS TableName, ccDataSources.Name AS DataSourceName"
									+ " FROM (ccFields LEFT JOIN ccContent ON ccFields.ContentID = ccContent.ID) LEFT JOIN (ccTables LEFT JOIN ccDataSources ON ccTables.DataSourceID = ccDataSources.ID) ON ccContent.ContentTableID = ccTables.ID"
									+ " WHERE (((ccFields.Active)<>0) AND ((ccContent.Active)<>0) AND ((ccTables.Active)<>0)) OR (((ccFields.Active)<>0) AND ((ccContent.Active)<>0) AND ((ccTables.Active)<>0));";
							CS = cpCore.db.csOpenSql_rev("Default", SQL);
							if (!cpCore.db.csOk(CS))
							{
								DiagProblem = "PROBLEM: No Content entries were found in the content table.";
								DiagActions = new Contensive.Core.coreToolsClass.DiagActionType[2];
								DiagActions[0].Name = "Ignore, or handle this issue manually";
								DiagActions[0].Command = "";
								Stream.Add(GetDiagError(DiagProblem, DiagActions));
							}
							else
							{
								while (cpCore.db.csOk(CS) && (DiagActionCount < DiagActionCountMax))
								{
									FieldName = cpCore.db.csGetText(CS, "FieldName");
									fieldType = cpCore.db.csGetInteger(CS, "FieldType");
									FieldRequired = cpCore.db.csGetBoolean(CS, "FieldRequired");
									FieldAuthorable = cpCore.db.csGetBoolean(CS, "FieldAuthorable");
									ContentName = cpCore.db.csGetText(CS, "ContentName");
									TableName = cpCore.db.csGetText(CS, "TableName");
									DataSourceName = cpCore.db.csGetText(CS, "DataSourceName");
									if (string.IsNullOrEmpty(DataSourceName))
									{
										DataSourceName = "Default";
									}
									if (FieldRequired && (!FieldAuthorable))
									{
										DiagProblem = "PROBLEM: Field [" + FieldName + "] in Content Definition [" + ContentName + "] is required, but is not referenced on the Admin Editing page. This will prevent content definition records from being saved.";
										DiagActions = new Contensive.Core.coreToolsClass.DiagActionType[2];
										DiagActions[0].Name = "Ignore, or handle this issue manually";
										DiagActions[0].Command = "";
										DiagActions[1].Name = "Set this Field inactive";
										DiagActions[1].Command = DiagActionSetFieldInactive.ToString() + "," + ContentName + "," + FieldName;
										DiagActions[1].Name = "Set this Field not required";
										DiagActions[1].Command = DiagActionSetFieldNotRequired.ToString() + "," + ContentName + "," + FieldName;
										Stream.Add(GetDiagError(DiagProblem, DiagActions));
									}
									if ((!string.IsNullOrEmpty(FieldName)) & (fieldType != FieldTypeIdRedirect) & (fieldType != FieldTypeIdManyToMany))
									{
										SQL = "SELECT " + FieldName + " FROM " + TableName + " WHERE ID=0;";
										CSTest = cpCore.db.csOpenSql_rev(DataSourceName, SQL);
										if (CSTest == -1)
										{
											DiagProblem = "PROBLEM: Field [" + FieldName + "] in Content Definition [" + ContentName + "] could not be read from database table [" + TableName + "] on datasource [" + DataSourceName + "].";
											DiagActions = new Contensive.Core.coreToolsClass.DiagActionType[2];
											DiagActions[0].Name = "Ignore, or handle this issue manually";
											DiagActions[0].Command = "";
											DiagActions[1].Name = "Set this Content Definition Field inactive";
											DiagActions[1].Command = DiagActionSetFieldInactive.ToString() + "," + ContentName + "," + FieldName;
											Stream.Add(GetDiagError(DiagProblem, DiagActions));
										}
									}
									cpCore.db.csClose(CSTest);
									cpCore.db.csGoNext(CS);
								}
							}
							cpCore.db.csClose(CS);
						}
						//
						// ----- Insert Content Testing
						//
						if (DiagActionCount < DiagActionCountMax)
						{
							Stream.Add(GetDiagHeader("Checking Content Insertion...<br>"));
							//
							CSContent = cpCore.db.csOpen("Content");
							if (!cpCore.db.csOk(CSContent))
							{
								DiagProblem = "PROBLEM: No Content entries were found in the content table.";
								DiagActions = new Contensive.Core.coreToolsClass.DiagActionType[2];
								DiagActions[0].Name = "Ignore, or handle this issue manually";
								DiagActions[0].Command = "";
								Stream.Add(GetDiagError(DiagProblem, DiagActions));
							}
							else
							{
								while (cpCore.db.csOk(CSContent) && (DiagActionCount < DiagActionCountMax))
								{
									ContentID = cpCore.db.csGetInteger(CSContent, "ID");
									ContentName = cpCore.db.csGetText(CSContent, "name");
									CSTestRecord = cpCore.db.csInsertRecord(ContentName);
									if (!cpCore.db.csOk(CSTestRecord))
									{
										DiagProblem = "PROBLEM: Could not insert a record using Content Definition [" + ContentName + "]";
										DiagActions = new Contensive.Core.coreToolsClass.DiagActionType[2];
										DiagActions[0].Name = "Ignore, or handle this issue manually";
										DiagActions[0].Command = "";
										Stream.Add(GetDiagError(DiagProblem, DiagActions));
									}
									else
									{
										TestRecordID = cpCore.db.csGetInteger(CSTestRecord, "id");
										if (TestRecordID == 0)
										{
											DiagProblem = "PROBLEM: Content Definition [" + ContentName + "] does not support the required field [ID]\"";
											DiagActions = new Contensive.Core.coreToolsClass.DiagActionType[2];
											DiagActions[0].Name = "Ignore, or handle this issue manually";
											DiagActions[0].Command = "";
											DiagActions[1].Name = "Set this Content Definition inactive";
											DiagActions[1].Command = DiagActionSetRecordInactive.ToString() + ",Content," + ContentID;
											Stream.Add(GetDiagError(DiagProblem, DiagActions));
										}
										else
										{
											CSFields = cpCore.db.csOpen("Content Fields", "ContentID=" + ContentID);
											while (cpCore.db.csOk(CSFields))
											{
												//
												// ----- read the value of the field to test its presents
												//
												FieldName = cpCore.db.csGetText(CSFields, "name");
												fieldType = cpCore.db.csGetInteger(CSFields, "Type");
												switch (fieldType)
												{
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
														if (IsNull(bitBucket) | (ErrorCount != cpCore.doc.errorCount))
														{
															DiagProblem = "PROBLEM: Content Field [" + ContentName + "].[" + FieldName + "] is a Redirection type, but the ContentID [" + RedirectContentID + "] is not valid.";
															if (string.IsNullOrEmpty(FieldName))
															{
																DiagProblem = DiagProblem + " Also, the field has no name attribute so these diagnostics can not automatically mark the field inactive.";
																DiagActions = new Contensive.Core.coreToolsClass.DiagActionType[2];
																DiagActions[0].Name = "Ignore, or handle this issue manually";
																DiagActions[0].Command = "";
																Stream.Add(GetDiagError(DiagProblem, DiagActions));
															}
															else
															{
																DiagActions = new Contensive.Core.coreToolsClass.DiagActionType[3];
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
														if (ErrorCount != cpCore.doc.errorCount)
														{
															DiagProblem = "PROBLEM: An error occurred reading the value of Content Field [" + ContentName + "].[" + FieldName + "]";
															DiagActions = new Contensive.Core.coreToolsClass.DiagActionType[2];
															DiagActions[0].Name = "Ignore, or handle this issue manually";
															DiagActions[0].Command = "";
															Stream.Add(GetDiagError(DiagProblem, DiagActions));
														}
														else
														{
															bitBucket = "";
															LookupList = cpCore.db.csGetText(CSFields, "Lookuplist");
															LookupContentID = cpCore.db.csGetInteger(CSFields, "LookupContentID");
															if (LookupContentID != 0)
															{
																ErrorCount = cpCore.doc.errorCount;
																bitBucket = Local_GetContentNameByID(LookupContentID);
															}
															if ((string.IsNullOrEmpty(LookupList)) && ((LookupContentID == 0) || (string.IsNullOrEmpty(bitBucket)) || (ErrorCount != cpCore.doc.errorCount)))
															{
																DiagProblem = "Content Field [" + ContentName + "].[" + FieldName + "] is a Lookup type, but LookupList is blank and LookupContentID [" + LookupContentID + "] is not valid.";
																DiagActions = new Contensive.Core.coreToolsClass.DiagActionType[3];
																DiagActions[0].Name = "Ignore, or handle this issue manually";
																DiagActions[0].Command = "";
																DiagActions[1].Name = "Convert the field to an Integer so no lookup is provided.";
																DiagActions[1].Command = DiagActionSetFieldType.ToString() + "," + ContentName + "," + FieldName + "," + Convert.ToString(FieldTypeIdInteger);
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
														if (ErrorCount != cpCore.doc.errorCount)
														{
															DiagProblem = "PROBLEM: An error occurred reading the value of Content Field [" + ContentName + "].[" + FieldName + "]";
															DiagActions = new Contensive.Core.coreToolsClass.DiagActionType[4];
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
										cpCore.db.csClose(CSFields);
										cpCore.db.csClose(CSTestRecord);
										cpCore.db.deleteContentRecord(ContentName, TestRecordID);
									}
									cpCore.db.csGoNext(CSContent);
								}
							}
							cpCore.db.csClose(CSContent);
						}
						//
						// ----- Check Navigator Entries
						//
						if (DiagActionCount < DiagActionCountMax)
						{
							Stream.Add(GetDiagHeader("Checking Navigator Entries...<br>"));
							CSPointer = cpCore.db.csOpen(cnNavigatorEntries);
							if (!cpCore.db.csOk(CSPointer))
							{
								DiagProblem = "PROBLEM: Could not open the [Navigator Entries] content.";
								DiagActions = new Contensive.Core.coreToolsClass.DiagActionType[4];
								DiagActions[0].Name = "Ignore, or handle this issue manually";
								DiagActions[0].Command = "";
								Stream.Add(GetDiagError(DiagProblem, DiagActions));
							}
							else
							{
								while (cpCore.db.csOk(CSPointer) && (DiagActionCount < DiagActionCountMax))
								{
									ContentID = cpCore.db.csGetInteger(CSPointer, "ContentID");
									if (ContentID != 0)
									{
										CSContent = cpCore.db.csOpen("Content", "ID=" + ContentID);
										if (!cpCore.db.csOk(CSContent))
										{
											DiagProblem = "PROBLEM: Menu Entry [" + cpCore.db.csGetText(CSPointer, "name") + "] points to an invalid Content Definition.";
											DiagActions = new Contensive.Core.coreToolsClass.DiagActionType[4];
											DiagActions[0].Name = "Ignore, or handle this issue manually";
											DiagActions[0].Command = "";
											DiagActions[1].Name = "Remove this menu entry";
											DiagActions[1].Command = DiagActionDeleteRecord.ToString() + ",Navigator Entries," + cpCore.db.csGetInteger(CSPointer, "ID");
											Stream.Add(GetDiagError(DiagProblem, DiagActions));
										}
										cpCore.db.csClose(CSContent);
									}
									cpCore.db.csGoNext(CSPointer);
								}
							}
							cpCore.db.csClose(CSPointer);
						}
						if (DiagActionCount >= DiagActionCountMax)
						{
							DiagProblem = "Diagnostic Problem Limit (" + DiagActionCountMax + ") has been reached. Resolve the above issues to see more.";
							DiagActions = new Contensive.Core.coreToolsClass.DiagActionType[2];
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
				if (DiagActionCount > 0)
				{
					ButtonList = ButtonList + "," + ButtonFix;
					ButtonList = ButtonList + "," + ButtonFixAndRun;
				}
				//
				// ----- end form
				//
				//Call Stream.Add(cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolContentDiagnostic))
				//
				return htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("GetForm_ContentDiagnostic", "ErrorTrap")
		}
		//
		//=============================================================================
		// Normalize the Index page Columns, setting proper values for IndexColumn, etc.
		//=============================================================================
		//
		private void NormalizeIndexColumns(int ContentID)
		{
			try
			{
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
				if (!cpCore.db.csOk(CSPointer))
				{
					throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors2("NormalizeIndexColumns", "Could not read Content Field Definitions")
				}
				else
				{
					//
					// Adjust IndexSortOrder to be 0 based, count by 1
					//
					ColumnCounter = 0;
					while (cpCore.db.csOk(CSPointer))
					{
						IndexColumn = cpCore.db.csGetInteger(CSPointer, "IndexColumn");
						ColumnWidth = cpCore.db.csGetInteger(CSPointer, "IndexWidth");
						if ((IndexColumn == 0) || (ColumnWidth == 0))
						{
							cpCore.db.csSet(CSPointer, "IndexColumn", 0);
							cpCore.db.csSet(CSPointer, "IndexWidth", 0);
							cpCore.db.csSet(CSPointer, "IndexSortPriority", 0);
						}
						else
						{
							//
							// Column appears in Index, clean it up
							//
							cpCore.db.csSet(CSPointer, "IndexColumn", ColumnCounter);
							ColumnCounter = ColumnCounter + 1;
							ColumnWidthTotal = ColumnWidthTotal + ColumnWidth;
						}
						cpCore.db.csGoNext(CSPointer);
					}
					if (ColumnCounter == 0)
					{
						//
						// No columns found, set name as Column 0, active as column 1
						//
						cpCore.db.cs_goFirst(CSPointer);
						while (cpCore.db.csOk(CSPointer))
						{
							switch (genericController.vbUCase(cpCore.db.csGetText(CSPointer, "name")))
							{
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
					if (ColumnWidthTotal > 0)
					{
						cpCore.db.cs_goFirst(CSPointer);
						while (cpCore.db.csOk(CSPointer))
						{
							ColumnWidth = cpCore.db.csGetInteger(CSPointer, "IndexWidth");
							ColumnWidth = Convert.ToInt32((ColumnWidth * 100) / (double)ColumnWidthTotal);
							cpCore.db.csSet(CSPointer, "IndexWidth", ColumnWidth);
							cpCore.db.csGoNext(CSPointer);
						}
					}
				}
				cpCore.db.csClose(CSPointer);
				//
				// ----- now fixup Sort Priority so only visible fields are sorted.
				//
				CSPointer = cpCore.db.csOpen("Content Fields", "(ContentID=" + ContentID + ")", "IndexSortPriority, IndexColumn");
				if (!cpCore.db.csOk(CSPointer))
				{
					throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors2("NormalizeIndexColumns", "Error reading Content Field Definitions")
				}
				else
				{
					//
					// Go through all fields, clear Sort Priority if it does not appear
					//
					int SortValue = 0;
					int SortDirection = 0;
					SortValue = 0;
					while (cpCore.db.csOk(CSPointer))
					{
						SortDirection = 0;
						if (cpCore.db.csGetInteger(CSPointer, "IndexColumn") == 0)
						{
							cpCore.db.csSet(CSPointer, "IndexSortPriority", 0);
						}
						else
						{
							cpCore.db.csSet(CSPointer, "IndexSortPriority", SortValue);
							SortDirection = cpCore.db.csGetInteger(CSPointer, "IndexSortDirection");
							if (SortDirection == 0)
							{
								SortDirection = 1;
							}
							else
							{
								if (SortDirection > 0)
								{
									SortDirection = 1;
								}
								else
								{
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
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("NormalizeIndexColumns", "ErrorTrap")
		}
		//
		//=============================================================================
		// Create a child content
		//=============================================================================
		//
		private string GetForm_CreateChildContent()
		{
			try
			{
				//
				int ParentContentID = 0;
				string ParentContentName = null;
				string ChildContentName = "";
				bool AddAdminMenuEntry = false;
				int CS = 0;
				string MenuName = "";
				bool AdminOnly = false;
				bool DeveloperOnly = false;
				stringBuilderLegacyController Stream = new stringBuilderLegacyController();
				string ButtonList;
				//
				ButtonList = ButtonCancel + "," + ButtonRun;
				//
				Stream.Add(GetTitle("Create a Child Content from a Content Definition", "This tool creates a Content Definition based on another Content Definition."));
				//
				//   print out the submit form
				//
				if (cpCore.docProperties.getText("Button") != "")
				{
					//
					// Process input
					//
					ParentContentID = cpCore.docProperties.getInteger("ParentContentID");
					ParentContentName = Local_GetContentNameByID(ParentContentID);
					ChildContentName = cpCore.docProperties.getText("ChildContentName");
					AddAdminMenuEntry = cpCore.docProperties.getBoolean("AddAdminMenuEntry");
					//
					Stream.Add(SpanClassAdminSmall);
					if ((string.IsNullOrEmpty(ParentContentName)) || (string.IsNullOrEmpty(ChildContentName)))
					{
						Stream.Add("<p>You must select a parent and provide a child name.</p>");
					}
					else
					{
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
				//'Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolCreateChildContent)
				Stream.Add("</SPAN>");
				//
				return htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);

				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("GetForm_CreateChildContent", "ErrorTrap")
		}
		//
		//=============================================================================
		// checks Content against Database tables
		//=============================================================================
		//
		private string GetForm_ClearContentWatchLinks()
		{
			try
			{
				//
				string SQL = null;
				stringBuilderLegacyController Stream = new stringBuilderLegacyController();
				string ButtonList;
				//
				ButtonList = ButtonCancel + ",Clear Content Watch Links";
				Stream.Add(SpanClassAdminNormal);
				Stream.Add(GetTitle("Clear ContentWatch Links", "This tools nulls the Links field of all Content Watch records. After running this tool, run the diagnostic spider to repopulate the links."));
				//
				if (cpCore.docProperties.getText("Button") != "")
				{
					//
					// Process input
					//
					Stream.Add("<br>");
					Stream.Add("<br>Clearing Content Watch Link field...");
					cpCore.db.executeQuery("update ccContentWatch set Link=null;");
					Stream.Add("<br>Content Watch Link field cleared.");
				}
				//
				// ----- end form
				//
				//'Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolClearContentWatchLink)
				Stream.Add("</span>");
				//
				return htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
				//
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("GetForm_ClearContentWatchLinks", "ErrorTrap")
		}
		//
		//=============================================================================
		/// <summary>
		/// Go through all Content Definitions and create appropriate tables and fields.
		/// </summary>
		/// <returns></returns>
		private string GetForm_SyncTables()
		{
			string returnValue = "";
			try
			{
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
				if (cpCore.docProperties.getText("Button") != "")
				{
					//
					//   Run Tools
					//
					Stream.Add("Synchronizing Tables to Content Definitions<br>");
					CSContent = cpCore.db.csOpen("Content",,,,,,, "id");
					if (cpCore.db.csOk(CSContent))
					{
						do
						{
							CD = Models.Complex.cdefModel.getCdef(cpCore, cpCore.db.csGetInteger(CSContent, "id"));
							TableName = CD.ContentTableName;
							Stream.Add("Synchronizing Content " + CD.Name + " to table " + TableName + "<br>");
							cpCore.db.createSQLTable(CD.ContentDataSourceName, TableName);
							if (CD.fields.Count > 0)
							{
								foreach (var keyValuePair in CD.fields)
								{
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
					cpCore.db.csClose(CSContent);
				}
				//
				returnValue = htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnValue;
		}
		//
		//
		//
		private string AddDiagError(string ProblemMsg, DiagActionType[] DiagActions)
		{
			return GetDiagError(ProblemMsg, DiagActions);
		}
		//
		//
		//
		private string GetDiagError(string ProblemMsg, DiagActionType[] DiagActions)
		{
			try
			{
				//
				string MethodName = null;
				int ActionCount = 0;
				int ActionPointer = 0;
				string Caption = null;
				string Panel = "";
				//
				MethodName = "GetDiagError";
				//
				Panel = Panel + "<table border=\"0\" cellpadding=\"2\" cellspacing=\"0\" width=\"100%\">";
				Panel = Panel + "<tr><td colspan=\"2\">" + SpanClassAdminNormal + "<b>" + ProblemMsg + "</b></SPAN></td></tr>";
				ActionCount = DiagActions.GetUpperBound(0);
				if (ActionCount > 0)
				{
					for (ActionPointer = 0; ActionPointer <= ActionCount; ActionPointer++)
					{
						Caption = DiagActions[ActionPointer].Name;
						if (!string.IsNullOrEmpty(Caption))
						{
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
				//
				return cpCore.html.main_GetPanel(Panel);
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("GetDiagError", "ErrorTrap")
		}
		//
		//
		//
		private string GetDiagHeader(string Copy)
		{
			return cpCore.html.main_GetPanel("<B>" + SpanClassAdminNormal + Copy + "</SPAN><B>");
		}
		//
		//
		//
		private string DiagArgument(string CommandList, int CommandPosition)
		{
			string tempDiagArgument = null;
			int CommandCount = 0;
			bool EndOfList = false;
			int CommandStartPosition = 0;
			int CommandEndPosition = 0;
			//
			tempDiagArgument = "";
			EndOfList = false;
			CommandStartPosition = 1;
			while ((CommandCount < CommandPosition) && (!EndOfList))
			{
				CommandStartPosition = genericController.vbInstr(CommandStartPosition, CommandList, ",");
				if (CommandStartPosition == 0)
				{
					EndOfList = true;
				}
				CommandStartPosition = CommandStartPosition + 1;
				CommandCount = CommandCount + 1;
			}
			if (!EndOfList)
			{
				CommandEndPosition = genericController.vbInstr(CommandStartPosition, CommandList, ",");
				if (CommandEndPosition == 0)
				{
					tempDiagArgument = CommandList.Substring(CommandStartPosition - 1);
				}
				else
				{
					tempDiagArgument = CommandList.Substring(CommandStartPosition - 1, CommandEndPosition - CommandStartPosition);
				}
			}
			return tempDiagArgument;
		}
	}
}
