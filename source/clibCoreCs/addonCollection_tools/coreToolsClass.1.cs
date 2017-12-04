
using System;
using Contensive.Core.Models.Entity;
using Contensive.Core.Models.Context;
using Contensive.Core.Controllers;
using static Contensive.Core.constants;
using static Contensive.Core.Controllers.genericController;
//
namespace Contensive.Core
{
	public class coreToolsClass
	{



		//
		//=============================================================================
		//=============================================================================
		//
		private string GetForm_Benchmark()
		{
			try
			{
				//
				string MethodName = null;
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
				if (cpCore.docProperties.getText("Button") != "")
				{
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
					for (TestPointer = 1; TestPointer <= TestCount; TestPointer++)
					{
						TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
						RS = cpCore.db.executeQuery(SQL);
						OpenTicks = OpenTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
						RecordCount = 0;
						TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
						foreach (DataRow dr in RS.Rows)
						{
							NextTicks = NextTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
							//
							TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
							TestCopy = genericController.encodeText(dr("NAME"));
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
					for (TestPointer = 1; TestPointer <= TestCount; TestPointer++)
					{
						//
						TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
						CS = cpCore.db.csOpen("Site Properties",,,,,,,, PageSize, PageNumber);
						OpenTicks = OpenTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
						//
						RecordCount = 0;
						while (cpCore.db.csOk(CS))
						{
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
						cpCore.db.csClose(CS);
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
					for (TestPointer = 1; TestPointer <= TestCount; TestPointer++)
					{
						//
						TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds;
						CS = cpCore.db.csOpen("Site Properties",,,,,,, "name", PageSize, PageNumber);
						OpenTicks = OpenTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks;
						//
						RecordCount = 0;
						while (cpCore.db.csOk(CS))
						{
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
						cpCore.db.csClose(CS);
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
				//
				//'Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolBenchmark)
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
			//RS = Nothing
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("GetForm_Benchmark", "ErrorTrap")
		}
		//
		//=============================================================================
		//   Get a ContentID from the ContentName using just the tables
		//=============================================================================
		//
		private int Local_GetContentID(string ContentName)
		{
				int tempLocal_GetContentID = 0;
			try
			{
				//
				DataTable dt = null;
				//
				tempLocal_GetContentID = 0;
				dt = cpCore.db.executeQuery("Select ID from ccContent where name=" + cpCore.db.encodeSQLText(ContentName));
				if (dt.Rows.Count > 0)
				{
					tempLocal_GetContentID = genericController.EncodeInteger(dt.Rows(0).Item(0));
				}
				//
				return tempLocal_GetContentID;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("Local_GetContentID", "ErrorTrap")
			return tempLocal_GetContentID;
		}
		//
		//=============================================================================
		//   Get a ContentID from the ContentName using just the tables
		//=============================================================================
		//
		private string Local_GetContentNameByID(int ContentID)
		{
				string tempLocal_GetContentNameByID = null;
			try
			{
				//
				DataTable dt = null;
				//
				tempLocal_GetContentNameByID = "";
				dt = cpCore.db.executeQuery("Select name from ccContent where id=" + ContentID);
				if (dt.Rows.Count > 0)
				{
					tempLocal_GetContentNameByID = genericController.encodeText(dt.Rows(0).Item(0));
				}
				//
				return tempLocal_GetContentNameByID;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("Local_GetContentNameByID", "ErrorTrap")
			return tempLocal_GetContentNameByID;
		}
		//
		//=============================================================================
		//   Get a ContentID from the ContentName using just the tables
		//=============================================================================
		//
		private string Local_GetContentTableName(string ContentName)
		{
				string tempLocal_GetContentTableName = null;
			try
			{
				//
				DataTable RS = null;
				//
				tempLocal_GetContentTableName = "";
				RS = cpCore.db.executeQuery("Select ccTables.Name as TableName from ccContent Left Join ccTables on ccContent.ContentTableID=ccTables.ID where ccContent.name=" + cpCore.db.encodeSQLText(ContentName));
				if (RS.Rows.Count > 0)
				{
					tempLocal_GetContentTableName = genericController.encodeText(RS.Rows(0).Item(0));
				}
				//
				return tempLocal_GetContentTableName;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("Local_GetContentTableName", "ErrorTrap")
			return tempLocal_GetContentTableName;
		}
		//
		//=============================================================================
		//   Get a ContentID from the ContentName using just the tables
		//=============================================================================
		//
		private string Local_GetContentDataSource(string ContentName)
		{
				string tempLocal_GetContentDataSource = null;
			try
			{
				//
				DataTable RS = null;
				string SQL = null;
				//
				tempLocal_GetContentDataSource = "";
				SQL = "Select ccDataSources.Name"
						+ " from ( ccContent Left Join ccTables on ccContent.ContentTableID=ccTables.ID )"
						+ " Left Join ccDataSources on ccTables.DataSourceID=ccDataSources.ID"
						+ " where ccContent.name=" + cpCore.db.encodeSQLText(ContentName);
				RS = cpCore.db.executeQuery(SQL);
				if (isDataTableOk(RS))
				{
					tempLocal_GetContentDataSource = genericController.encodeText(RS.Rows(0).Item("Name"));
				}
				if (string.IsNullOrEmpty(tempLocal_GetContentDataSource))
				{
					tempLocal_GetContentDataSource = "Default";
				}
				closeDataTable(RS);
				//RS = Nothing
				//
				return tempLocal_GetContentDataSource;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("Local_GetContentDataSource", "ErrorTrap")
			return tempLocal_GetContentDataSource;
		}
		//
		//=============================================================================
		//   Print the manual query form
		//=============================================================================
		//
		private string GetForm_DbSchema()
		{
			try
			{
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
				dataSourceModel datasource = dataSourceModel.create(cpCore, cpCore.docProperties.getInteger("DataSourceID"), new List<string>());
				//
				ButtonList = ButtonCancel + "," + ButtonRun;
				//
				Stream.Add(GetTitle("Query Database Schema", "This tool examines the database schema for all tables available."));
				//
				StatusOK = true;
				if ((cpCore.docProperties.getText("button")) != ButtonRun)
				{
					//
					// First pass, initialize
					//
					PageSize = 10;
					PageNumber = 1;
					Retries = 0;
				}
				else
				{
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
					if( true )
					{
						Stream.Add(DateTime.Now + " GetSchema executed successfully<br>");
					}
					else
					{
						//
						// ----- error
						//
						Stream.Add(DateTime.Now + " SQL execution returned the following error<br>");
						Stream.Add("Error Number " + ErrorNumber + "<br>");
						Stream.Add("Error Descrition " + ErrorDescription + "<br>");
					}
					if (!isDataTableOk(RSSchema))
					{
						//
						// ----- no result
						//
						Stream.Add(DateTime.Now + " A schema was returned, but it contains no records.<br>");
					}
					else
					{
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
						foreach (DataColumn RecordField in RSSchema.Columns)
						{
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
						for (RowPointer = 0; RowPointer <= RowMax; RowPointer++)
						{
							Stream.Add(RowStart);
							for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++)
							{
								CellData = arrayOfSchema[ColumnPointer, RowPointer];
								if (IsNull(CellData))
								{
									Stream.Add(ColumnStart + "[null]" + ColumnEnd);
								}
								else if ((CellData == null))
								{
									Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
								}
								else if (string.IsNullOrEmpty(CellData))
								{
									Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
								}
								else
								{
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
//INSTANT C# TODO TASK: The 'On Error Resume Next' statement is not converted by Instant C#:
					On Error Resume Next
					//
					RSSchema = cpCore.db.getIndexSchemaData(TableName);
					if (!isDataTableOk(RSSchema))
					{
						//
						// ----- no result
						//
						Stream.Add(DateTime.Now + " A schema was returned, but it contains no records.<br>");
					}
					else
					{
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
						foreach (DataColumn RecordField in RSSchema.Columns)
						{
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
						for (RowPointer = 0; RowPointer <= RowMax; RowPointer++)
						{
							Stream.Add(RowStart);
							for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++)
							{
								CellData = arrayOfSchema[ColumnPointer, RowPointer];
								if (IsNull(CellData))
								{
									Stream.Add(ColumnStart + "[null]" + ColumnEnd);
								}
								else if ((CellData == null))
								{
									Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
								}
								else if (string.IsNullOrEmpty(CellData))
								{
									Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
								}
								else
								{
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
					if( true )
					{
						Stream.Add(DateTime.Now + " GetSchema executed successfully<br>");
					}
					else
					{
						//
						// ----- error
						//
						Stream.Add(DateTime.Now + " SQL execution returned the following error<br>");
						Stream.Add("Error Number " + ErrorNumber + "<br>");
						Stream.Add("Error Descrition " + ErrorDescription + "<br>");
					}
					if (isDataTableOk(RSSchema))
					{
						//
						// ----- no result
						//
						Stream.Add(DateTime.Now + " A schema was returned, but it contains no records.<br>");
					}
					else
					{
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
						foreach (DataColumn RecordField in RSSchema.Columns)
						{
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
						for (RowPointer = 0; RowPointer <= RowMax; RowPointer++)
						{
							Stream.Add(RowStart);
							for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++)
							{
								CellData = arrayOfSchema[ColumnPointer, RowPointer];
								if (IsNull(CellData))
								{
									Stream.Add(ColumnStart + "[null]" + ColumnEnd);
								}
								else if ((CellData == null))
								{
									Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
								}
								else if (string.IsNullOrEmpty(CellData))
								{
									Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
								}
								else
								{
									Stream.Add(ColumnStart + CellData + ColumnEnd);
								}
							}
							Stream.Add(RowEnd);
						}
						Stream.Add("</TABLE>");
						RSSchema.Dispose();
						RSSchema = null;
					}
					if (!StatusOK)
					{
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
				//'Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolSchema)
				Stream.Add("</SPAN>");
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
			//RSSchema = Nothing
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("GetForm_DbSchema", "ErrorTrap")
			StatusOK = false;
		}
		//
		//=============================================================================
		//   Get the Configure Edit
		//=============================================================================
		//
		private string GetForm_ConfigureEdit(BaseClasses.CPBaseClass cp)
		{
			string result = "";
			try
			{
				//
				string SQL = null;
				string DataSourceName = string.Empty;
				string ToolButton = null;
				int ContentID = 0;
				int RecordCount = 0;
				int RecordPointer = 0;
				int CSPointer = 0;
				int formFieldId = 0;
				string ContentName = string.Empty;
				Models.Complex.cdefModel CDef = null;
				string formFieldName = null;
				int formFieldTypeId = 0;
				string TableName = string.Empty;
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
				if (ContentID > 0)
				{
					ContentName = cp.Content.GetRecordName("content", ContentID);
					if (!string.IsNullOrEmpty(ContentName))
					{
						TableName = cp.Content.GetTable(ContentName);
						DataSourceName = cp.Content.GetDataSource(ContentName);
						CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentID, true, true);
					}
				}
				if (CDef != null)
				{
					//
					if (!string.IsNullOrEmpty(ToolButton))
					{
						if (ToolButton != ButtonCancel)
						{
							//
							// Save the form changes
							//
							AllowContentAutoLoad = cp.Site.GetBoolean("AllowContentAutoLoad", "true");
							cp.Site.SetProperty("AllowContentAutoLoad", "false");
							//
							// ----- Save the input
							//
							RecordCount = genericController.EncodeInteger(cp.Doc.GetInteger("dtfaRecordCount"));
							if (RecordCount > 0)
							{
								for (RecordPointer = 0; RecordPointer < RecordCount; RecordPointer++)
								{
									//
									formFieldName = cp.Doc.GetText("dtfaName." + RecordPointer);
									formFieldTypeId = cp.Doc.GetInteger("dtfaType." + RecordPointer);
									formFieldId = genericController.EncodeInteger(cp.Doc.GetInteger("dtfaID." + RecordPointer));
									formFieldInherited = cp.Doc.GetBoolean("dtfaInherited." + RecordPointer);
									//
									// problem - looking for the name in the Db using the form's name, but it could have changed.
									// have to look field up by id
									//
									foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> cdefFieldKvp in CDef.fields)
									{
										if (cdefFieldKvp.Value.id == formFieldId)
										{
											//
											// Field was found in CDef
											//
											if (cdefFieldKvp.Value.inherited && (!formFieldInherited))
											{
												//
												// Was inherited, but make a copy of the field
												//
												CSTarget = cpCore.db.csInsertRecord("Content Fields");
												if (cpCore.db.csOk(CSTarget))
												{
													CSSource = cpCore.db.cs_openContentRecord("Content Fields", formFieldId);
													if (cpCore.db.csOk(CSSource))
													{
														cpCore.db.csCopyRecord(CSSource, CSTarget);
													}
													cpCore.db.csClose(CSSource);
													formFieldId = cpCore.db.csGetInteger(CSTarget, "ID");
													cpCore.db.csSet(CSTarget, "ContentID", ContentID);
												}
												cpCore.db.csClose(CSTarget);
												ReloadCDef = true;
											}
											else if ((!cdefFieldKvp.Value.inherited) && (formFieldInherited))
											{
												//
												// Was a field, make it inherit from it's parent
												//
												CSTarget = CSTarget;
												cpCore.db.deleteContentRecord("Content Fields", formFieldId);
												ReloadCDef = true;
											}
											else if ((!cdefFieldKvp.Value.inherited) && (!formFieldInherited))
											{
												//
												// not inherited, save the field values and mark for a reload
												//
												if (true)
												{
													if (formFieldName.IndexOf(" ") + 1 != 0)
													{
														//
														// remoave spaces from new name
														//
														StatusMessage = StatusMessage + "<LI>Field [" + formFieldName + "] was renamed [" + genericController.vbReplace(formFieldName, " ", "") + "] because the field name can not include spaces.</LI>";
														formFieldName = genericController.vbReplace(formFieldName, " ", "");
													}
													//
													if ((!string.IsNullOrEmpty(formFieldName)) & (formFieldTypeId != 0) & ((cdefFieldKvp.Value.nameLc == "") || (cdefFieldKvp.Value.fieldTypeId == 0)))
													{
														//
														// Create Db field, Field is good but was not before
														//
														cpCore.db.createSQLTableField(DataSourceName, TableName, formFieldName, formFieldTypeId);
														StatusMessage = StatusMessage + "<LI>Field [" + formFieldName + "] was saved to this content definition and a database field was created in [" + CDef.ContentTableName + "].</LI>";
													}
													else if ((string.IsNullOrEmpty(formFieldName)) || (formFieldTypeId == 0))
													{
														//
														// name blank or type=0 - do nothing but tell them
														//
														if (string.IsNullOrEmpty(formFieldName) && formFieldTypeId == 0)
														{
															ErrorMessage += "<LI>Field number " + (RecordPointer + 1) + " was saved to this content definition but no database field was created because a name and field type are required.</LI>";
														}
														else if (formFieldName == "unnamedfield" + coreToolsClass.fieldId.ToString())
														{
															ErrorMessage += "<LI>Field number " + (RecordPointer + 1) + " was saved to this content definition but no database field was created because a field name is required.</LI>";
														}
														else
														{
															ErrorMessage += "<LI>Field [" + formFieldName + "] was saved to this content definition but no database field was created because a field type are required.</LI>";
														}
													}
													else if ((formFieldName == cdefFieldKvp.Value.nameLc) && (formFieldTypeId != cdefFieldKvp.Value.fieldTypeId))
													{
														//
														// Field Type changed, must be done manually
														//
														ErrorMessage += "<LI>Field [" + formFieldName + "] changed type from [" + cpCore.db.getRecordName("content Field Types", cdefFieldKvp.Value.fieldTypeId) + "] to [" + cpCore.db.getRecordName("content Field Types", formFieldTypeId) + "]. This may have caused a problem converting content.</LI>";
														int DataSourceTypeID = cpCore.db.getDataSourceType(DataSourceName);
														switch (DataSourceTypeID)
														{
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
													if (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
													{
														SQL += ",adminonly=" + cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaAdminOnly." + RecordPointer));
													}
													if (cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore))
													{
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
						if (ToolButton == ButtonAdd)
						{
							//
							// ----- Insert a blank Field
							//
							CSPointer = cpCore.db.csInsertRecord("Content Fields");
							if (cpCore.db.csOk(CSPointer))
							{
								cpCore.db.csSet(CSPointer, "name", "unnamedField" + cpCore.db.csGetInteger(CSPointer, "id").ToString());
								cpCore.db.csSet(CSPointer, "ContentID", ContentID);
								cpCore.db.csSet(CSPointer, "EditSortPriority", 0);
								ReloadCDef = true;
							}
							cpCore.db.csClose(CSPointer);
						}
						//'
						//' ----- Button Reload CDef
						//'
						if (ToolButton == ButtonSaveandInvalidateCache)
						{
							cpCore.cache.invalidateAll();
							cpCore.doc.clearMetaData();
						}
						//
						// ----- Restore Content Autoload site property
						//
						if (AllowContentAutoLoad)
						{
							cp.Site.SetProperty("AllowContentAutoLoad", AllowContentAutoLoad.ToString());
						}
						//
						// ----- Cancel or Save, reload CDef and go
						//
						if ((ToolButton == ButtonCancel) || (ToolButton == ButtonOK))
						{
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
				Stream.Add("<div style=\"width:45%;float:left;padding:10px;\">" + "Use this tool to add or modify content definition fields. Contensive uses a caching system for content definitions that is not automatically reloaded. Change you make will not take effect until the next time the system is reloaded. When you create a new field, the database field is created automatically when you have saved both a name and a field type. If you change the field type, you may have to manually change the database field." + "</div>");
				if (ContentID == 0)
				{
					Stream.Add("<div style=\"width:45%;float:left;padding:10px;\">Related Tools:" + "<div style=\"padding-left:20px;\"><a href=\"?af=104\">Set Default Admin Listing page columns</a></div>" + "</div>");
				}
				else
				{
					Stream.Add("<div style=\"width:45%;float:left;padding:10px;\">Related Tools:" + "<div style=\"padding-left:20px;\"><a href=\"?af=104&ContentID=" + ContentID + "\">Set Default Admin Listing page columns for '" + ContentName + "'</a></div>" + "<div style=\"padding-left:20px;\"><a href=\"?af=4&cid=" + Models.Complex.cdefModel.getContentId(cpCore, "content") + "&id=" + ContentID + "\">Edit '" + ContentName + "' Content Definition</a></div>" + "<div style=\"padding-left:20px;\"><a href=\"?cid=" + ContentID + "\">View records in '" + ContentName + "'</a></div>" + "</div>");
				}
				Stream.Add("</div>");
				Stream.Add("<div style=\"clear:both\">&nbsp;</div>");
				if (!string.IsNullOrEmpty(StatusMessage))
				{
					Stream.Add("<UL>" + StatusMessage + "</UL>");
					Stream.Add("<div style=\"clear:both\">&nbsp;</div>");
				}
				if (!string.IsNullOrEmpty(ErrorMessage))
				{
					Stream.Add("<br><span class=\"ccError\">There was a problem saving these changes</span><UL>" + ErrorMessage + "</UL></SPAN>");
					Stream.Add("<div style=\"clear:both\">&nbsp;</div>");
				}
				if (ReloadCDef)
				{
					CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentID, true, true);
				}
				if (ContentID != 0)
				{
					//
					//--------------------------------------------------------------------------------
					// print the Configure edit form
					//--------------------------------------------------------------------------------
					//
					Stream.Add(cpCore.html.main_GetPanelTop());
					ContentName = Local_GetContentNameByID(ContentID);
					ButtonList = ButtonCancel + "," + ButtonSave + "," + ButtonOK + "," + ButtonAdd; // & "," & ButtonReloadCDef
					//
					// Get a new copy of the content definition
					//
					Stream.Add(SpanClassAdminNormal + "<P><B>" + ContentName + "</b></P>");
					Stream.Add("<table border=\"0\" cellpadding=\"1\" cellspacing=\"1\" width=\"100%\">");
					//
					ParentContentID = CDef.parentID;
					if (ParentContentID == -1)
					{
						AllowCDefInherit = false;
					}
					else
					{
						AllowCDefInherit = true;
						ParentContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ParentContentID);
						ParentCDef = Models.Complex.cdefModel.getCdef(cpCore, ParentContentID, true, true);
					}
					if (CDef.fields.Count > 0)
					{
						Stream.Add("<tr>");
						Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\"></td>");
						if (!AllowCDefInherit)
						{
							Stream.Add("<td valign=\"bottom\" width=\"100\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b><br>Inherited*</b></span></td>");
							NeedFootNote1 = true;
						}
						else
						{
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
						foreach (var keyValuePair in CDef.fields)
						{
							fieldSortClass fieldSort = new fieldSortClass();
							//Dim field As New appServices_metaDataClass.CDefFieldClass
							string sortOrder = "";
							fieldSort.field = keyValuePair.Value;
							sortOrder = "";
							if (fieldSort.field.active)
							{
								sortOrder += "0";
							}
							else
							{
								sortOrder += "1";
							}
							if (fieldSort.field.authorable)
							{
								sortOrder += "0";
							}
							else
							{
								sortOrder += "1";
							}
							sortOrder += fieldSort.field.editTabName + GetIntegerString(fieldSort.field.editSortPriority, 10) + GetIntegerString(fieldSort.field.id, 10);
							fieldSort.sort = sortOrder;
							fieldList.Add(fieldSort);
						}
						fieldList.Sort((p1, p2) => p1.sort.CompareTo(p2.sort));
						foreach (fieldSortClass fieldsort in fieldList)
						{
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
							if (ContentFieldsCID != 0)
							{
								streamRow.Add("<nobr>" + SpanClassAdminSmall + "[<a href=\"?aa=" + AdminActionNop + "&af=" + AdminFormEdit + "&id=" + formFieldId + "&cid=" + ContentFieldsCID + "&mm=0\">EDIT</a>]</span></nobr>");
							}
							streamRow.Add("</td>");
							//
							// Inherited
							//
							if (!AllowCDefInherit)
							{
								//
								// no parent
								//
								streamRow.Add("<td class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "False</span></td>");
							}
							else if (fieldsort.field.inherited)
							{
								//
								// inherited property
								//
								streamRow.Add("<td class=\"ccPanelInput\" align=\"center\">" + cpCore.html.html_GetFormInputCheckBox2("dtfaInherited." + RecordCount, fieldsort.field.inherited) + "</td>");
							}
							else
							{
								//
								// CDef has a parent, but the field is non-inherited, test for a matching Parent Field
								//
								if (ParentCDef == null)
								{
									foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> kvp in ParentCDef.fields)
									{
										if (kvp.Value.nameLc == fieldsort.field.nameLc)
										{
											parentField = kvp.Value;
											break;
										}
									}
								}
								if (parentField == null)
								{
									streamRow.Add("<td class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "False**</span></td>");
									NeedFootNote2 = true;
								}
								else
								{
									streamRow.Add("<td class=\"ccPanelInput\" align=\"center\">" + cpCore.html.html_GetFormInputCheckBox2("dtfaInherited." + RecordCount, fieldsort.field.inherited) + "</td>");
								}
							}
							//
							// name
							//
							bool tmpValue = string.IsNullOrEmpty(fieldsort.field.nameLc);
							rowValid = rowValid && !tmpValue;
							streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
							if (fieldsort.field.inherited)
							{
								streamRow.Add(SpanClassAdminSmall + fieldsort.field.nameLc + "&nbsp;</SPAN>");
							}
							else if (FieldLocked)
							{
								streamRow.Add(SpanClassAdminSmall + fieldsort.field.nameLc + "&nbsp;</SPAN><input type=hidden name=dtfaName." + RecordCount + " value=\"" + fieldsort.field.nameLc + "\">");
							}
							else
							{
								streamRow.Add(cpCore.html.html_GetFormInputText2("dtfaName." + RecordCount, fieldsort.field.nameLc, 1, 10));
							}
							streamRow.Add("</nobr></td>");
							//
							// caption
							//
							streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
							if (fieldsort.field.inherited)
							{
								streamRow.Add(SpanClassAdminSmall + fieldsort.field.caption + "</SPAN>");
							}
							else
							{
								streamRow.Add(cpCore.html.html_GetFormInputText2("dtfaCaption." + RecordCount, fieldsort.field.caption, 1, 10));
							}
							streamRow.Add("</nobr></td>");
							//
							// Edit Tab
							//
							streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
							if (fieldsort.field.inherited)
							{
								streamRow.Add(SpanClassAdminSmall + fieldsort.field.editTabName + "</SPAN>");
							}
							else
							{
								streamRow.Add(cpCore.html.html_GetFormInputText2("dtfaEditTab." + RecordCount, fieldsort.field.editTabName, 1, 10));
							}
							streamRow.Add("</nobr></td>");
							//
							// default
							//
							streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
							if (fieldsort.field.inherited)
							{
								streamRow.Add(SpanClassAdminSmall + genericController.encodeText(fieldsort.field.defaultValue) + "</SPAN>");
							}
							else
							{
								streamRow.Add(cpCore.html.html_GetFormInputText2("dtfaDefaultValue." + RecordCount, genericController.encodeText(fieldsort.field.defaultValue), 1, 10));
							}
							streamRow.Add("</nobr></td>");
							//
							// type
							//
							rowValid = rowValid && (fieldsort.field.fieldTypeId > 0);
							streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
							if (fieldsort.field.inherited)
							{
								CSPointer = cpCore.db.csOpenRecord("Content Field Types", fieldsort.field.fieldTypeId);
								if (!cpCore.db.csOk(CSPointer))
								{
									streamRow.Add(SpanClassAdminSmall + "Unknown[" + fieldsort.field.fieldTypeId + "]</SPAN>");
								}
								else
								{
									streamRow.Add(SpanClassAdminSmall + cpCore.db.csGetText(CSPointer, "Name") + "</SPAN>");
								}
								cpCore.db.csClose(CSPointer);
							}
							else if (FieldLocked)
							{
								streamRow.Add(cpCore.db.getRecordName("content field types", fieldsort.field.fieldTypeId) + cpCore.html.html_GetFormInputHidden("dtfaType." + RecordCount, fieldsort.field.fieldTypeId));
							}
							else
							{
								TypeSelect = TypeSelectTemplate;
								TypeSelect = genericController.vbReplace(TypeSelect, "menuname", "dtfaType." + RecordCount, 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
								TypeSelect = genericController.vbReplace(TypeSelect, "=\"" + fieldsort.field.fieldTypeId + "\"", "=\"" + fieldsort.field.fieldTypeId + "\" selected", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
								streamRow.Add(TypeSelect);
							}
							streamRow.Add("</nobr></td>");
							//
							// sort priority
							//
							streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
							if (fieldsort.field.inherited)
							{
								streamRow.Add(SpanClassAdminSmall + fieldsort.field.editSortPriority + "</SPAN>");
							}
							else
							{
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
							if (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
							{
								streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaAdminOnly." + RecordCount, genericController.encodeText(fieldsort.field.adminOnly), fieldsort.field.inherited));
							}
							//
							// Developer Only
							//
							if (cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore))
							{
								streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaDeveloperOnly." + RecordCount, genericController.encodeText(fieldsort.field.developerOnly), fieldsort.field.inherited));
							}
							//
							streamRow.Add("</tr>");
							RecordCount = RecordCount + 1;
							//
							// rows are built - put the blank rows at the top
							//
							if (!rowValid)
							{
								Stream.Add(streamRow.Text());
							}
							else
							{
								StreamValidRows.Add(streamRow.Text());
							}
						}
						Stream.Add(StreamValidRows.Text());
						Stream.Add(cpCore.html.html_GetFormInputHidden("dtfaRecordCount", RecordCount));
					}
					Stream.Add("</table>");
					//Stream.Add( cpcore.htmldoc.main_GetPanelButtons(ButtonList, "Button"))
					//
					Stream.Add(cpCore.html.main_GetPanelBottom());
					//Call Stream.Add(cpCore.main_GetFormEnd())
					if (NeedFootNote1)
					{
						Stream.Add("<br>*Field Inheritance is not allowed because this Content Definition has no parent.");
					}
					if (NeedFootNote2)
					{
						Stream.Add("<br>**This field can not be inherited because the Parent Content Definition does not have a field with the same name.");
					}
				}
				if (ContentID != 0)
				{
					//
					// Save the content selection
					//
					Stream.Add(cpCore.html.html_GetFormInputHidden(RequestNameToolContentID, ContentID));
				}
				else
				{
					//
					// content tables that have edit forms to Configure
					//
					FormPanel = FormPanel + SpanClassAdminNormal + "Select a Content Definition to Configure its edit form<br>";
					FormPanel = FormPanel + "<br>";
					FormPanel = FormPanel + cpCore.html.main_GetFormInputSelect(RequestNameToolContentID, ContentID, "Content");
					FormPanel = FormPanel + "</SPAN>";
					Stream.Add(cpCore.html.main_GetPanel(FormPanel));
				}
				//
				Stream.Add(cpCore.html.html_GetFormInputHidden("ReloadCDef", ReloadCDef));
				result = htmlController.legacy_openFormTable(cpCore, ButtonList) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonList);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		//
		//
		private string GetForm_ConfigureEdit_CheckBox(string Label, string Value, bool Inherited)
		{
			string tempGetForm_ConfigureEdit_CheckBox = null;
			tempGetForm_ConfigureEdit_CheckBox = "<td class=\"ccPanelInput\" align=\"center\"><nobr>";
			if (Inherited)
			{
				tempGetForm_ConfigureEdit_CheckBox = tempGetForm_ConfigureEdit_CheckBox + SpanClassAdminSmall + Value + "</SPAN>";
			}
			else
			{
				tempGetForm_ConfigureEdit_CheckBox = tempGetForm_ConfigureEdit_CheckBox + cpCore.html.html_GetFormInputCheckBox(Label, Value);
			}
			return tempGetForm_ConfigureEdit_CheckBox + "</nobr></td>";
		}
		//
		//=============================================================================
		//   Print the manual query form
		//=============================================================================
		//
		private string GetForm_DbIndex()
		{
				string tempGetForm_DbIndex = null;
			try
			{
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
				int ErrorNumber = 0;
				string ErrorDescription = null;
				string[,] Rows = null;
				int RowMax = 0;
				int RowPointer = 0;
				string Copy = "";
				bool TableRowEven = false;
				int TableColSpan = 0;
				string ButtonList;
				//
				ButtonList = ButtonCancel + "," + ButtonSelect;
				tempGetForm_DbIndex = GetTitle("Modify Database Indexes", "This tool adds and removes database indexes.");
				//
				// Process Input
				//
				Button = cpCore.docProperties.getText("Button");
				TableID = cpCore.docProperties.getInteger("TableID");
				//
				// Get Tablename and DataSource
				//
				CS = cpCore.db.csOpenRecord("Tables", TableID,,, "Name,DataSourceID");
				if (cpCore.db.csOk(CS))
				{
					TableName = cpCore.db.csGetText(CS, "name");
					DataSource = cpCore.db.csGetLookup(CS, "DataSourceID");
				}
				cpCore.db.csClose(CS);
				//
				if ((TableID != 0) & (TableID == cpCore.docProperties.getInteger("previoustableid")) & (!string.IsNullOrEmpty(Button)))
				{
					//
					// Drop Indexes
					//
					Count = cpCore.docProperties.getInteger("DropCount");
					if (Count > 0)
					{
						for (Pointer = 0; Pointer < Count; Pointer++)
						{
							if (cpCore.docProperties.getBoolean("DropIndex." + Pointer))
							{
								IndexName = cpCore.docProperties.getText("DropIndexName." + Pointer);
								tempGetForm_DbIndex = tempGetForm_DbIndex + "<br>Dropping index [" + IndexName + "] from table [" + TableName + "]";
								cpCore.db.deleteSqlIndex("Default", TableName, IndexName);
							}
						}
					}
					//
					// Add Indexes
					//
					Count = cpCore.docProperties.getInteger("AddCount");
					if (Count > 0)
					{
						for (Pointer = 0; Pointer < Count; Pointer++)
						{
							if (cpCore.docProperties.getBoolean("AddIndex." + Pointer))
							{
								//IndexName = cpCore.main_GetStreamText2("AddIndexFieldName." & Pointer)
								FieldName = cpCore.docProperties.getText("AddIndexFieldName." + Pointer);
								IndexName = TableName + FieldName;
								tempGetForm_DbIndex = tempGetForm_DbIndex + "<br>Adding index [" + IndexName + "] to table [" + TableName + "] for field [" + FieldName + "]";
								cpCore.db.createSQLIndex(DataSource, TableName, IndexName, FieldName);
							}
						}
					}
				}
				//
				tempGetForm_DbIndex = tempGetForm_DbIndex + cpCore.html.html_GetFormStart;
				TableColSpan = 3;
				tempGetForm_DbIndex = tempGetForm_DbIndex + StartTable(2, 0, 0);
				//
				// Select Table Form
				//
				tempGetForm_DbIndex = tempGetForm_DbIndex + GetTableRow("<br><br><B>Select table to index</b>", TableColSpan, false);
				tempGetForm_DbIndex = tempGetForm_DbIndex + GetTableRow(cpCore.html.main_GetFormInputSelect("TableID", TableID, "Tables",, "Select a SQL table to start"), TableColSpan, false);
				if (TableID != 0)
				{
					//
					// Add/Drop Indexes form
					//
					tempGetForm_DbIndex = tempGetForm_DbIndex + cpCore.html.html_GetFormInputHidden("PreviousTableID", TableID);
					//
					// Drop Indexes
					//
					tempGetForm_DbIndex = tempGetForm_DbIndex + GetTableRow("<br><br><B>Select indexes to remove</b>", TableColSpan, TableRowEven);
					RSSchema = cpCore.db.getIndexSchemaData(TableName);


					if (RSSchema.Rows.Count == 0)
					{
						//
						// ----- no result
						//
						Copy = Copy + DateTime.Now + " A schema was returned, but it contains no indexs.";
						tempGetForm_DbIndex = tempGetForm_DbIndex + GetTableRow(Copy, TableColSpan, TableRowEven);
					}
					else
					{

						Rows = cpCore.db.convertDataTabletoArray(RSSchema);
						RowMax = Rows.GetUpperBound(1);
						for (RowPointer = 0; RowPointer <= RowMax; RowPointer++)
						{
							IndexName = genericController.encodeText(Rows[5, RowPointer]);
							if (!string.IsNullOrEmpty(IndexName))
							{
								tempGetForm_DbIndex = tempGetForm_DbIndex + StartTableRow();
								Copy = cpCore.html.html_GetFormInputCheckBox2("DropIndex." + RowPointer, false) + cpCore.html.html_GetFormInputHidden("DropIndexName." + RowPointer, IndexName) + genericController.encodeText(IndexName);
								tempGetForm_DbIndex = tempGetForm_DbIndex + GetTableCell(Copy,,, TableRowEven);
								tempGetForm_DbIndex = tempGetForm_DbIndex + GetTableCell(genericController.encodeText(Rows[17, RowPointer]),,, TableRowEven);
								tempGetForm_DbIndex = tempGetForm_DbIndex + GetTableCell("&nbsp;",,, TableRowEven);
								tempGetForm_DbIndex = tempGetForm_DbIndex + kmaEndTableRow;
								TableRowEven = !TableRowEven;
							}
						}
						tempGetForm_DbIndex = tempGetForm_DbIndex + cpCore.html.html_GetFormInputHidden("DropCount", RowMax + 1);
					}
					//
					// Add Indexes
					//
					TableRowEven = false;
					tempGetForm_DbIndex = tempGetForm_DbIndex + GetTableRow("<br><br><B>Select database fields to index</b>", TableColSpan, TableRowEven);
					RSSchema = cpCore.db.getColumnSchemaData(TableName);
					if (RSSchema.Rows.Count == 0)
					{
						//
						// ----- no result
						//
						Copy = Copy + DateTime.Now + " A schema was returned, but it contains no indexs.";
						tempGetForm_DbIndex = tempGetForm_DbIndex + GetTableRow(Copy, TableColSpan, TableRowEven);
					}
					else
					{

						Rows = cpCore.db.convertDataTabletoArray(RSSchema);
						//
						RowMax = Rows.GetUpperBound(1);
						for (RowPointer = 0; RowPointer <= RowMax; RowPointer++)
						{
							tempGetForm_DbIndex = tempGetForm_DbIndex + StartTableRow();
							Copy = cpCore.html.html_GetFormInputCheckBox2("AddIndex." + RowPointer, false) + cpCore.html.html_GetFormInputHidden("AddIndexFieldName." + RowPointer, Rows[3, RowPointer]) + genericController.encodeText(Rows[3, RowPointer]);
							tempGetForm_DbIndex = tempGetForm_DbIndex + GetTableCell(Copy,,, TableRowEven);
							tempGetForm_DbIndex = tempGetForm_DbIndex + GetTableCell("&nbsp;",,, TableRowEven);
							tempGetForm_DbIndex = tempGetForm_DbIndex + GetTableCell("&nbsp;",,, TableRowEven);
							tempGetForm_DbIndex = tempGetForm_DbIndex + kmaEndTableRow;
							TableRowEven = !TableRowEven;
						}
						tempGetForm_DbIndex = tempGetForm_DbIndex + cpCore.html.html_GetFormInputHidden("AddCount", RowMax + 1);
					}
					//
					// Spacers
					//
					tempGetForm_DbIndex = tempGetForm_DbIndex + StartTableRow();
					tempGetForm_DbIndex = tempGetForm_DbIndex + GetTableCell(getSpacer(300, 1), "200");
					tempGetForm_DbIndex = tempGetForm_DbIndex + GetTableCell(getSpacer(200, 1), "200");
					tempGetForm_DbIndex = tempGetForm_DbIndex + GetTableCell("&nbsp;", "100%");
					tempGetForm_DbIndex = tempGetForm_DbIndex + kmaEndTableRow;
				}
				tempGetForm_DbIndex = tempGetForm_DbIndex + kmaEndTable;
				//
				// Buttons
				//
				//GetForm_DbIndex = GetForm_DbIndex & cpCore.main_GetFormInputHidden("af", AdminFormToolDbIndex)
				return htmlController.legacy_openFormTable(cpCore, ButtonList) + tempGetForm_DbIndex + htmlController.legacy_closeFormTable(cpCore, ButtonList);
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
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("GetForm_DbIndex", "ErrorTrap")
			return tempGetForm_DbIndex;
		}
		//
		//=============================================================================
		//   Print the manual query form
		//=============================================================================
		//
		private string GetForm_ContentDbSchema()
		{
				string tempGetForm_ContentDbSchema = null;
			try
			{
				//
				int CS = 0;
				int TableColSpan = 0;
				bool TableEvenRow = false;
				string SQL = null;
				string TableName = null;
				string ButtonList;
				//
				ButtonList = ButtonCancel;
				tempGetForm_ContentDbSchema = GetTitle("Get Content Database Schema", "This tool displays all tables and fields required for the current Content Defintions.");
				//
				TableColSpan = 3;
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + StartTable(2, 0, 0);
				SQL = "SELECT DISTINCT ccTables.Name as TableName, ccFields.Name as FieldName, ccFieldTypes.Name as FieldType"
						+ " FROM ((ccContent LEFT JOIN ccTables ON ccContent.ContentTableID = ccTables.ID) LEFT JOIN ccFields ON ccContent.ID = ccFields.ContentID) LEFT JOIN ccFieldTypes ON ccFields.Type = ccFieldTypes.ID"
						+ " ORDER BY ccTables.Name, ccFields.Name;";
				CS = cpCore.db.csOpenSql_rev("Default", SQL);
				TableName = "";
				while (cpCore.db.csOk(CS))
				{
					if (TableName != cpCore.db.csGetText(CS, "TableName"))
					{
						TableName = cpCore.db.csGetText(CS, "TableName");
						tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableRow("<B>" + TableName + "</b>", TableColSpan, TableEvenRow);
					}
					tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + StartTableRow();
					tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableCell("&nbsp;",,, TableEvenRow);
					tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableCell(cpCore.db.csGetText(CS, "FieldName"),,, TableEvenRow);
					tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableCell(cpCore.db.csGetText(CS, "FieldType"),,, TableEvenRow);
					tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + kmaEndTableRow;
					TableEvenRow = !TableEvenRow;
					cpCore.db.csGoNext(CS);
				}
				//
				// Field Type Definitions
				//
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableRow("<br><br><B>Field Type Definitions</b>", TableColSpan, TableEvenRow);
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableRow("Boolean - Boolean values 0 and 1 are stored in a database long integer field type", TableColSpan, TableEvenRow);
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableRow("Lookup - References to related records stored as database long integer field type", TableColSpan, TableEvenRow);
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableRow("Integer - database long integer field type", TableColSpan, TableEvenRow);
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableRow("Float - database floating point value", TableColSpan, TableEvenRow);
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableRow("Date - database DateTime field type.", TableColSpan, TableEvenRow);
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableRow("AutoIncrement - database long integer field type. Field automatically increments when a record is added.", TableColSpan, TableEvenRow);
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableRow("Text - database character field up to 255 characters.", TableColSpan, TableEvenRow);
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableRow("LongText - database character field up to 64K characters.", TableColSpan, TableEvenRow);
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableRow("TextFile - references a filename in the Content Files folder. Database character field up to 255 characters. ", TableColSpan, TableEvenRow);
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableRow("File - references a filename in the Content Files folder. Database character field up to 255 characters. ", TableColSpan, TableEvenRow);
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableRow("Redirect - This field has no database equivelent. No Database field is required.", TableColSpan, TableEvenRow);
				//
				// Spacers
				//
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + StartTableRow();
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableCell(getSpacer(20, 1), "20");
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableCell(getSpacer(300, 1), "300");
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + GetTableCell("&nbsp;", "100%");
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + kmaEndTableRow;
				tempGetForm_ContentDbSchema = tempGetForm_ContentDbSchema + kmaEndTable;
				//
				//GetForm_ContentDbSchema = GetForm_ContentDbSchema & cpCore.main_GetFormInputHidden("af", AdminFormToolContentDbSchema)
				return (htmlController.legacy_openFormTable(cpCore, ButtonList)) + tempGetForm_ContentDbSchema + (htmlController.legacy_closeFormTable(cpCore, ButtonList));
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
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("GetForm_ContentDbSchema", "ErrorTrap")
			return tempGetForm_ContentDbSchema;
		}
		//
		//=============================================================================
		//   Print the manual query form
		//=============================================================================
		//
		private string GetForm_LogFiles()
		{
				string tempGetForm_LogFiles = null;
			try
			{
				//
				string QueryOld = null;
				string QueryNew = null;
				string ButtonList;
				//
				ButtonList = ButtonCancel;
				tempGetForm_LogFiles = GetTitle("Log File View", "This tool displays the Contensive Log Files.");
				tempGetForm_LogFiles = tempGetForm_LogFiles + "<P></P>";
				//
				QueryOld = ".asp?";
				QueryNew = genericController.ModifyQueryString(QueryOld, RequestNameAdminForm, AdminFormToolLogFileView, true);
				tempGetForm_LogFiles = tempGetForm_LogFiles + genericController.vbReplace(GetForm_LogFiles_Details(), QueryOld, QueryNew + "&", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
				//
				//GetForm_LogFiles = GetForm_LogFiles & cpCore.main_GetFormInputHidden("af", AdminFormToolLogFileView)
				return (htmlController.legacy_openFormTable(cpCore, ButtonList)) + tempGetForm_LogFiles + (htmlController.legacy_closeFormTable(cpCore, ButtonList));
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
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("GetForm_LogFiles", "ErrorTrap")
			return tempGetForm_LogFiles;
		}
		//
		//==============================================================================================
		//   Display a path in the Content Files with links to download and change folders
		//==============================================================================================
		//
		private string GetForm_LogFiles_Details()
		{
			string result = "";
			try
			{

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
				if (string.IsNullOrEmpty(CurrentPath))
				{
					CurrentPath = "\\";
				}
				else if (CurrentPath.Substring(0, 1) != "\\")
				{
					CurrentPath = "\\" + CurrentPath;
				}
				//
				// Parent Folder is the path to the parent of current folder, and must start with a slash
				//
				Position = CurrentPath.LastIndexOf("\\") + 1;
				if (Position == 1)
				{
					ParentPath = "\\";
				}
				else
				{
					ParentPath = CurrentPath.Substring(0, Position - 1);
				}
				//
				//
				if (cpCore.docProperties.getText("SourceFile") != "")
				{
					//
					// Return the content of the file
					//
					cpCore.webServer.setResponseContentType("text/text");
					result = cpCore.appRootFiles.readFile(cpCore.docProperties.getText("SourceFile"));
					cpCore.doc.continueProcessing = false;
				}
				else
				{
					result = result + GetTableStart;
					//
					// Parent Folder Link
					//
					if (CurrentPath != ParentPath)
					{
						FileSize = "";
						FileDate = "";
						result = result + GetForm_LogFiles_Details_GetRow("<A href=\"" + cpCore.webServer.requestPage + "?SetPath=" + ParentPath + "\">" + FolderOpenImage + "</A>", "<A href=\"" + cpCore.webServer.requestPage + "?SetPath=" + ParentPath + "\">" + ParentPath + "</A>", FileSize, FileDate, RowEven);
					}
					//
					// Sub-Folders
					//

					SourceFolders = cpCore.appRootFiles.getFolderNameList(StartPath + CurrentPath);
					if (!string.IsNullOrEmpty(SourceFolders))
					{
						FolderSplit = Microsoft.VisualBasic.Strings.Split(SourceFolders, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
						FolderCount = FolderSplit.GetUpperBound(0) + 1;
						for (FolderPointer = 0; FolderPointer < FolderCount; FolderPointer++)
						{
							FolderLine = FolderSplit[FolderPointer];
							if (!string.IsNullOrEmpty(FolderLine))
							{
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
					if (string.IsNullOrEmpty(SourceFolders))
					{
						FileSize = "";
						FileDate = "";
						result = result + GetForm_LogFiles_Details_GetRow(SpacerImage, "no files were found in this folder", FileSize, FileDate, RowEven);
					}
					else
					{
						FolderSplit = Microsoft.VisualBasic.Strings.Split(SourceFolders, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
						FolderCount = FolderSplit.GetUpperBound(0) + 1;
						for (FolderPointer = 0; FolderPointer < FolderCount; FolderPointer++)
						{
							FolderLine = FolderSplit[FolderPointer];
							if (!string.IsNullOrEmpty(FolderLine))
							{
								LineSplit = FolderLine.Split(',');
								Filename = LineSplit[0];
								FileSize = LineSplit[5];
								FileDate = LineSplit[3];
								FileURL = StartPath + CurrentPath + "\\" + Filename;
								QueryString = cpCore.doc.refreshQueryString;
								QueryString = genericController.ModifyQueryString(QueryString, RequestNameAdminForm, Convert.ToString(AdminFormTool), true);
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
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		//=============================================================================
		//   Table Rows
		//=============================================================================
		//
		public string GetForm_LogFiles_Details_GetRow(string Cell0, string Cell1, string Cell2, string Cell3, bool RowEven)
		{
			string tempGetForm_LogFiles_Details_GetRow = null;
			//
			string ClassString = null;
			//
			if (genericController.EncodeBoolean(RowEven))
			{
				RowEven = false;
				ClassString = " class=\"ccPanelRowEven\" ";
			}
			else
			{
				RowEven = true;
				ClassString = " class=\"ccPanelRowOdd\" ";
			}
			//
			Cell0 = genericController.encodeText(Cell0);
			if (string.IsNullOrEmpty(Cell0))
			{
				Cell0 = "&nbsp;";
			}
			//
			Cell1 = genericController.encodeText(Cell1);
			//
			if (string.IsNullOrEmpty(Cell1))
			{
				tempGetForm_LogFiles_Details_GetRow = "<tr><TD" + ClassString + " Colspan=\"4\">" + Cell0 + "</td></tr>";
			}
			else
			{
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
		private string GetForm_LoadCDef()
		{
			string result = "";
			try
			{
				//
				stringBuilderLegacyController Stream = new stringBuilderLegacyController();
				string ButtonList;
				//
				ButtonList = ButtonCancel + "," + ButtonSaveandInvalidateCache;
				Stream.Add(GetTitle("Load Content Definitions", "This tool reloads the content definitions. This is necessary when changes are made to the ccContent or ccFields tables outside Contensive. The site will be blocked during the load."));
				//
				if ((cpCore.docProperties.getText("button")) != ButtonSaveandInvalidateCache)
				{
					//
					// First pass, initialize
					//
				}
				else
				{
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
			}
			catch (Exception ex)
			{
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
		private string GetForm_Restart()
		{
				string tempGetForm_Restart = null;
			try
			{
				//
				stringBuilderLegacyController Stream = new stringBuilderLegacyController();
				string ButtonList;
				// Dim runAtServer As runAtServerClass
				//
				ButtonList = ButtonCancel + "," + ButtonRestartContensiveApplication;
				Stream.Add(GetTitle("Load Content Definitions", "This tool stops and restarts the Contensive Application controlling this website. If you restart, the site will be unavailable for up to a minute while the site is stopped and restarted. If the site does not restart, you will need to contact a site administrator to have the Contensive Server restarted."));
				//
				if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
				{
					//
					tempGetForm_Restart = "<P>You must be an administrator to use this tool.</P>";
					//
				}
				else if ((cpCore.docProperties.getText("button")) != ButtonRestartContensiveApplication)
				{
					//
					// First pass, initialize
					//
				}
				else
				{
					//
					// Restart
					//
					logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "Restarting Contensive", "dll", "ToolsClass", "GetForm_Restart", 0, "dll", "Warning: member " + cpCore.doc.authContext.user.name + " (" + cpCore.doc.authContext.user.id + ") restarted using the Restart tool", false, true, cpCore.webServer.requestUrl, "", "");
					//runAtServer = New runAtServerClass(cpCore)
					cpCore.webServer.redirect("/ccLib/Popup/WaitForIISReset.htm",,, false);
					Threading.Thread.Sleep(2000);
					//
					//
					//
					throw new NotImplementedException("GetForm_Restart");
					//hint = hint & ",035"
					taskSchedulerController taskScheduler = new taskSchedulerController();
					cmdDetailClass cmdDetail = new cmdDetailClass();
					cmdDetail.addonId = 0;
					cmdDetail.addonName = "commandRestart";
					cmdDetail.docProperties = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, "");
					taskScheduler.addTaskToQueue(cpCore, taskQueueCommandEnumModule.runAddon, cmdDetail, false);
					//
					//Call runAtServer.executeCmd("Restart", "appname=" & cpCore.app.config.name)
				}
				//
				// Display form
				//
				Stream.Add(SpanClassAdminNormal);
				Stream.Add("<br>");
				//'Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolRestart)
				Stream.Add("</SPAN>");
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
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("GetForm_Restart", "ErrorTrap")
			return tempGetForm_Restart;
		}
		//
		//
		//
		private Models.Complex.cdefModel GetCDef(string ContentName)
		{
			return Models.Complex.cdefModel.getCdef(cpCore, ContentName);
		}



		//Function CloseFormTable(ByVal ButtonList As String) As String
		//    If ButtonList <> "" Then
		//        CloseFormTable = "</td></tr></TABLE>" & cpcore.htmldoc.main_GetPanelButtons(ButtonList, "Button") & "</form>"
		//    Else
		//        CloseFormTable = "</td></tr></TABLE></form>"
		//    End If
		//End Function
		//'
		//'
		//'
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
		private string GetForm_LoadTemplates()
		{
			try
			{
				//
				stringBuilderLegacyController Stream = new stringBuilderLegacyController();
				string ButtonList = null;
				//
				string[] Folders = null;
				string FolderList = null;
				string FolderDetailString = null;
				string[] FolderDetails = null;
				string FolderName = null;
				//
				string FileList = null;
				string[] Files = null;
				int Ptr = 0;
				string FilePath = null;
				string FileDetailString = null;
				string[] FileDetails = null;
				string Filename = null;
				string PageSource = null;
				int CS = 0;
				string Link = null;
				string TemplateName = null;
				//
				//dim buildversion As String
				string AppPath = null;
				string FileRootPath = null;
				//
				bool AllowBodyHTML = false;
				bool AllowScriptLink = false;
				bool AllowImageImport = false;
				bool AllowStyleImport = false;
				//
				AllowBodyHTML = cpCore.docProperties.getBoolean("AllowBodyHTML");
				AllowScriptLink = cpCore.docProperties.getBoolean("AllowScriptLink");
				AllowImageImport = cpCore.docProperties.getBoolean("AllowImageImport");
				AllowStyleImport = cpCore.docProperties.getBoolean("AllowStyleImport");
				//
				Stream.Add(GetTitle("Load Templates", "This tool creates template records from the HTML files in the root folder of the site."));
				//
				if ((cpCore.docProperties.getText("button")) != ButtonImportTemplates)
				{
					//
					// First pass, initialize
					//
				}
				else
				{
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
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("GetForm_LoadTemplates", "ErrorTrap")
		}
		//'
		//'=============================================================================
		//'   Import the htm and html files in the FileRootPath and below into Page Templates
		//'       FileRootPath is the path to the root of the site
		//'       AppPath is the path to the folder currently
		//'=============================================================================
		//'
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
		//            FileDetailString = Files(Ptr)
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
		//                FolderDetailString = Folders(Ptr)
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
		//                    ParentContentName = models.complex.cdefmodel.getContentNameByID(cpcore,ParentID)
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
		//            '
		//            Exit Function
		//            '
		//            ' ----- Error Trap
		//            '
		//ErrorTrap:
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
		//ErrorTrap:
		//            throw (New ApplicationException("Unexpected exception"))'  Call handleLegacyClassErrors1("GetDbCDef_SetAdminColumns", "ErrorTrap")
		//        End Sub
		//
		//=============================================================================
		//   Print the manual query form
		//=============================================================================
		//
		private string GetForm_ContentFileManager()
		{
			string result = "";
			try
			{
				adminUIController Adminui = new adminUIController(cpCore);
				string InstanceOptionString = "AdminLayout=1&filesystem=content files";
				addonModel addon = addonModel.create(cpCore, "{B966103C-DBF4-4655-856A-3D204DEF6B21}");
				string Content = cpCore.addon.execute(addon, new BaseClasses.CPUtilsBaseClass.addonExecuteContext()
				{
					addonType = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
					instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, InstanceOptionString),
					instanceGuid = "-2",
					errorCaption = "File Manager"
				});
				string Description = "Manage files and folders within the virtual content file area.";
				string ButtonList = ButtonApply + "," + ButtonCancel;
				result = Adminui.GetBody("Content File Manager", ButtonList, "", false, false, Description, "", 0, Content);
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
		private string GetForm_WebsiteFileManager()
		{
			string result = "";
			try
			{
				adminUIController Adminui = new adminUIController(cpCore);
				string InstanceOptionString = "AdminLayout=1&filesystem=website files";
				addonModel addon = addonModel.create(cpCore, "{B966103C-DBF4-4655-856A-3D204DEF6B21}");
				string Content = cpCore.addon.execute(addon, new BaseClasses.CPUtilsBaseClass.addonExecuteContext()
				{
					addonType = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
					instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, InstanceOptionString),
					instanceGuid = "-2",
					errorCaption = "File Manager"
				});
				string Description = "Manage files and folders within the Website's file area.";
				string ButtonList = ButtonApply + "," + ButtonCancel;
				result = Adminui.GetBody("Website File Manager", ButtonList, "", false, false, Description, "", 0, Content);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		//=============================================================================
		// Find and Replace launch tool
		//=============================================================================
		//
		private string GetForm_FindAndReplace()
		{
			try
			{
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
				if (Button == ButtonFindAndReplace)
				{
					RowCnt = cpCore.docProperties.getInteger("CDefRowCnt");
					if (RowCnt > 0)
					{
						for (RowPtr = 0; RowPtr < RowCnt; RowPtr++)
						{
							if (cpCore.docProperties.getBoolean("Cdef" + RowPtr))
							{
								lcName = genericController.vbLCase(cpCore.docProperties.getText("CDefName" + RowPtr));
								if (IsDeveloper || (lcName == "page content") || (lcName == "copy content") || (lcName == "page templates"))
								{
									CDefList = CDefList + "," + lcName;
								}
							}
						}
						if (!string.IsNullOrEmpty(CDefList))
						{
							CDefList = CDefList.Substring(1);
						}
						//CDefList = cpCore.main_GetStreamText2("CDefList")
						FindText = cpCore.docProperties.getText("FindText");
						ReplaceText = cpCore.docProperties.getText("ReplaceText");
						//runAtServer.ipAddress = "127.0.0.1"
						//runAtServer.port = "4531"
						QS = "app=" + encodeNvaArgument(cpCore.serverConfig.appConfig.name) + "&FindText=" + encodeNvaArgument(FindText) + "&ReplaceText=" + encodeNvaArgument(ReplaceText) + "&CDefNameList=" + encodeNvaArgument(CDefList);

						throw new NotImplementedException("GetForm_FindAndReplace");
						taskSchedulerController taskScheduler = new taskSchedulerController();
						cmdDetailClass cmdDetail = new cmdDetailClass();
						cmdDetail.addonId = 0;
						cmdDetail.addonName = "GetForm_FindAndReplace";
						cmdDetail.docProperties = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, QS);
						taskScheduler.addTaskToQueue(cpCore, taskQueueCommandEnumModule.runAddon, cmdDetail, false);


						//                    Call runAtServer.executeCmd("FindAndReplace", QS)
						Stream.Add("Find and Replace has been requested for content definitions [" + CDefList + "], finding [" + FindText + "] and replacing with [" + ReplaceText + "]");
					}
				}
				else
				{
					CDefList = "Page Content,Copy Content,Page Templates";
					FindText = "";
					ReplaceText = "";
				}
				//
				// Display form
				//
				FindRows = cpCore.docProperties.getInteger("SQLRows");
				if (FindRows == 0)
				{
					FindRows = cpCore.userProperty.getInteger("FindAndReplaceFindRows", 1);
				}
				else
				{
					cpCore.userProperty.setProperty("FindAndReplaceFindRows", FindRows.ToString());
				}
				ReplaceRows = cpCore.docProperties.getInteger("ReplaceRows");
				if (ReplaceRows == 0)
				{
					ReplaceRows = cpCore.userProperty.getInteger("FindAndReplaceReplaceRows", 1);
				}
				else
				{
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
				while (cpCore.db.csOk(CS))
				{
					RecordName = cpCore.db.csGetText(CS, "Name");
					lcName = genericController.vbLCase(RecordName);
					if (IsDeveloper || (lcName == "page content") || (lcName == "copy content") || (lcName == "page templates"))
					{
						RecordID = cpCore.db.csGetInteger(CS, "ID");
						if (genericController.vbInstr(1, "," + CDefList + ",", "," + RecordName + ",") != 0)
						{
							TopHalf = TopHalf + "<div>" + cpCore.html.html_GetFormInputCheckBox2("Cdef" + RowPtr, true) + cpCore.html.html_GetFormInputHidden("CDefName" + RowPtr, RecordName) + "&nbsp;" + cpCore.db.csGetText(CS, "Name") + "</div>";
						}
						else
						{
							BottomHalf = BottomHalf + "<div>" + cpCore.html.html_GetFormInputCheckBox2("Cdef" + RowPtr, false) + cpCore.html.html_GetFormInputHidden("CDefName" + RowPtr, RecordName) + "&nbsp;" + cpCore.db.csGetText(CS, "Name") + "</div>";
						}
					}
					cpCore.db.csGoNext(CS);
					RowPtr = RowPtr + 1;
				}
				cpCore.db.csClose(CS);
				Stream.Add(TopHalf + BottomHalf + cpCore.html.html_GetFormInputHidden("CDefRowCnt", RowPtr));
				//
				return htmlController.legacy_openFormTable(cpCore, ButtonCancel + "," + ButtonFindAndReplace) + Stream.Text + htmlController.legacy_closeFormTable(cpCore, ButtonCancel + "," + ButtonFindAndReplace);
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("GetForm_FindAndReplace", "ErrorTrap")
		}
		//
		//=============================================================================
		// GUID Tools
		//=============================================================================
		//
		private string GetForm_IISReset()
		{
			try
			{
				//
				string Button = null;
				//Dim GUIDGenerator As guidClass
				stringBuilderLegacyController s;
				//Dim runAtServer As runAtServerClass
				//
				s = new stringBuilderLegacyController();
				s.Add(GetTitle("IIS Reset", "Reset the webserver."));
				//
				// Process the form
				//
				Button = cpCore.docProperties.getText("button");
				//
				if (Button == ButtonIISReset)
				{
					//
					//
					//
					logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "Resetting IIS", "dll", "ToolsClass", "GetForm_IISReset", 0, "dll", "Warning: member " + cpCore.doc.authContext.user.name + " (" + cpCore.doc.authContext.user.id + ") executed an IISReset using the IISReset tool", false, true, cpCore.webServer.requestUrl, "", "");
					//runAtServer = New runAtServerClass(cpCore)
					cpCore.webServer.redirect("/ccLib/Popup/WaitForIISReset.htm",,, false);
					Threading.Thread.Sleep(2000);



					throw new NotImplementedException("GetForm_IISReset");
					taskSchedulerController taskScheduler = new taskSchedulerController();
					cmdDetailClass cmdDetail = new cmdDetailClass();
					cmdDetail.addonId = 0;
					cmdDetail.addonName = "GetForm_IISReset";
					cmdDetail.docProperties = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, "");
					taskScheduler.addTaskToQueue(cpCore, taskQueueCommandEnumModule.runAddon, cmdDetail, false);


					// Call runAtServer.executeCmd("IISReset", "")
				}
				//
				// Display form
				//
				return htmlController.legacy_openFormTable(cpCore, ButtonCancel + "," + ButtonIISReset) + s.Text + htmlController.legacy_closeFormTable(cpCore, ButtonCancel + "," + ButtonIISReset);
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("GetForm_IISReset", "ErrorTrap")
		}
		//
		//=============================================================================
		// GUID Tools
		//=============================================================================
		//
		private string GetForm_CreateGUID()
		{
			try
			{
				//
				string Button = null;
				//'Dim GUIDGenerator As guidClass
				stringBuilderLegacyController s;
				//
				s = new stringBuilderLegacyController();
				s.Add(GetTitle("Create GUID", "Use this tool to create a GUID. This is useful when creating new Addons."));
				//
				// Process the form
				//
				Button = cpCore.docProperties.getText("button");
				//
				//If Button = ButtonCreateGUID Then
				//GUIDGenerator = New guidClass
				s.Add(cpCore.html.html_GetFormInputText2("GUID", Guid.NewGuid().ToString(), 1, 80));
				//Call s.Add("<span style=""border:inset; padding:4px; background-color:white;"">" & Guid.NewGuid.ToString() & "</span>")
				//End If
				//
				// Display form
				//
				return htmlController.legacy_openFormTable(cpCore, ButtonCancel + "," + ButtonCreateGUID) + s.Text + htmlController.legacy_closeFormTable(cpCore, ButtonCancel + "," + ButtonCreateGUID);
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw (new ApplicationException("Unexpected exception")); // Call handleLegacyClassErrors1("GetForm_CreateGUID", "ErrorTrap")
		}
		//'
		//'====================================================================================================
		//''' <summary>
		//''' 'handle legacy errors in this class, v1
		//''' </summary>
		//''' <param name="MethodName"></param>
		//''' <param name="ignore0"></param>
		//''' <remarks></remarks>
		//Private Sub handleLegacyClassErrors1(ByVal MethodName As String, Optional ByVal ignore0 As String = "")
		//   throw (New ApplicationException("Unexpected exception"))'cpCore.handleLegacyError("Tools", MethodName, Err.Number, Err.Source, Err.Description, True, False)
		//End Sub
		//'
		//'====================================================================================================
		//''' <summary>
		//''' handle legacy errors in this class, v2
		//''' </summary>
		//''' <param name="MethodName"></param>
		//''' <param name="ErrDescription"></param>
		//''' <remarks></remarks>
		//Private Sub handleLegacyClassErrors2(ByVal MethodName As String, ByVal ErrDescription As String)
		//    fixme-- cpCore.handleException(New ApplicationException("")) ' -----ignoreInteger, "App.EXEName", ErrDescription)
		//End Sub


	}
}
