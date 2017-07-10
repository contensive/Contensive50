
Option Explicit On
Option Strict On

Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Models.Context
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Core
    Public Class coreToolsClass
        '========================================================================
        ' This file and its contents are copyright by Kidwell McGowan Associates.
        '========================================================================
        '
        ' ----- global scope variables
        '
        '
        ' ----- data fields
        '
        Private AdminContentMax As Integer             ' after init, max possible value for AdminContentID
        Private FieldProblems As Boolean
        Private Findstring(50) As String                  ' Value to search for each index column
        '
        '=============================================================================
        ' Development Tools values
        '=============================================================================
        '
        Private cpCore As coreClass
        Private ToolsTable As String
        Private ToolsContentName As String
        Private DefaultReadOnly As Boolean
        Private DefaultActive As Boolean
        Private DefaultPassword As Boolean
        Private DefaulttextBuffered As Boolean
        Private DefaultRequired As Boolean
        Private DefaultAdminOnly As Boolean
        Private DefaultDeveloperOnly As Boolean
        Private DefaultCreateBlankRecord As Boolean
        Private defaultAddMenu As Boolean
        Private ToolsQuery As String
        Private ToolsDataSource As String
        '
        ' ----- Forms
        '
        Private AdminFormTool As Integer
        Private ToolsSourceForm As Integer
        '
        ' ----- Actions
        '
        Private ToolsAction As Integer
        '
        ' ----- Buttons
        '
        Private Button As String
        '
        Private ClassInitialized As Boolean        ' if true, the module has been
        '
        ' ----- Content Field descriptors (admin and tools)
        '
        Const fieldType = 0         ' The type of data the field holds
        Const FieldName = 1         ' The name of the field
        Const FieldCaption = 2          ' The caption for displaying the field
        Const FieldValue = 3        ' The value carried to and from the database
        Const FieldReadOnly = 4         ' If true, this field can not be written back to the database
        Const FieldLookupContentID = 5      ' If TYPELOOKUP, (for Db controled sites) this is the content ID of the source table
        Const FieldRequired = 7         ' if true, this field must be entered
        Const FieldDefaultVariant = 8          ' default value on a new record
        Const FieldHelpMessage = 9         ' explaination of this field
        Const FieldUniqueName = 10          ' not supported -- must change name anyway
        Const FieldTextBuffered = 11        ' if true, the input is run through RemoveControlCharacters()
        Const FieldPassword = 12            ' for text boxes, sets the password attribute
        Const FieldRedirectContentID = 13       ' If TYPEREDIRECT, this is new contentID
        Const FieldRedirectID = 14          ' If TYPEREDIRECT, this is the field that must match ID of this record
        Const FieldRedirectPath = 15        ' New Field, If TYPEREDIRECT, this is the path to the next page (if blank, current page is used)
        Const fieldId = 16              ' the ID in the ccContentFields Table that this came from
        Const FieldBlockAccess = 17         ' do not send field out to user because they do not have access rights
        Const FIELDATTRIBUTEMAX = 17        ' the maximum value of the attributes (second) FIELD index
        '
        ' ----- Diagnostic Action Types
        '
        Private DiagActionCount As Integer
        Const DiagActionCountMax = 10
        '
        Private Structure DiagActionType
            Dim Name As String
            Dim Command As String
        End Structure
        Const DiagActionNop = 0                 ' no arguments
        Const DiagActionSetFieldType = 1        ' Arg(1)=ContentName, Arg(2)=FieldName, Arg(3)=TypeID
        Const DiagActionDeleteRecord = 2        ' Arg(1)=ContentName, Arg(2)=RecordID
        Const DiagActionContentDeDupe = 3       ' Arg(1)=ContentName
        Const DiagActionSetFieldInactive = 4    ' Arg(1)=ContentName, Arg(2)=FieldName
        Const DiagActionSetRecordInactive = 5   ' Arg(1)=ContentName, Arg(2)=RecordID
        Const DiagActionAddStyle = 6            ' Arg(1)=Stylename from sample stylesheet
        Const DiagActionAddStyleSheet = 7       ' no arguments
        Const DiagActionSetFieldNotRequired = 8 ' Arg(1)=ContentName, Arg(2)=RecordID
        '
        Const ContentFileViewDefaultCopy = "<P>The Content File View displays the files which store content which may be too large to include in your database.</P>"
        '
        '========================================================================
        '   This header file controls the Admin Developer Tools arcitecture.
        '   It should be called from within the admin form structure
        '
        ' Features:
        '   03/15/00
        '       add a table Import function
        '
        ' Issues:
        '
        ' Contains:
        '   ToolsInit()
        '       is called within this code -- put this code first
        '
        '   GetForm()
        '       Prints the entire Admin Menu page structure, with the
        '       form in the content window. Uses GetFormTop() and
        '       AdminPageBottom() to complete the page.
        '
        ' Request Input:

        '
        ' Input:
        '   dta ToolsAction      Action to preform during init
        '   dtf AdminFormTool        next form to display in content window
        '   Button Button      Submit button selected in previous form
        '   dtt ToolsTable       Table used for Create Content from table button
        '   dtei    MenuEntryID
        '   dthi    MenuHeaderID
        '   dtmd    MenuDirection
        '   dtcn    ToolsColumnNumber    Used by Configure Index
        '   dtfe    ToolsFieldEdit   used together with field number and fieldname - dtfaActivex, where x=fieldID
        '   dtq ToolsQuery       used for manual query form
        '   dtdreadonly DefaultReadOnly
        '   dtdactive   DefaultActive
        '   dtdpassword DefaultPassword
        '   dtdtextbuffered DefaulttextBuffered
        '   dtdrequired DefaultRequired
        '   dtdadmin    DefaultAdminOnly
        '   dtddev      DefaultDeveloperOnly
        '   dtdam       DefaultAddMenu
        '   dtblank     DefaultCreateBlankRecord
        '
        'Dependancies:
        '   LibCommon2.asp
        '   LibAdmin3.asp
        '   site field definitions
        '
        '   This file and its contents are copyright by Kidwell McGowan Associates.
        '========================================================================
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
        '
        '========================================================================
        ' Print the Tools page
        '========================================================================
        '
        Public Function GetForm() As String
            On Error GoTo ErrorTrap
            '
            Dim MenuEntryID As Integer
            Dim MenuHeaderID As Integer
            Dim MenuDirection As Integer
            Dim Stream As New stringBuilderLegacyController
            Dim Adminui As New adminUIController(cpCore)
            '
            Button = cpCore.docProperties.getText("Button")
            If (Button = ButtonCancelAll) Then
                '
                ' Cancel to the admin site
                '
                Call cpCore.webServer.redirect(cpCore.siteProperties.getText("AdminURL", "/admin/"))
            End If
            '
            ' ----- Check permissions
            '
            If Not cpCore.authContext.isAuthenticatedAdmin(cpCore) Then
                '
                ' You must be admin to use this feature
                '
                GetForm = Adminui.GetFormBodyAdminOnly()
                GetForm = Adminui.GetBody("Admin Tools", ButtonCancelAll, "", False, False, "<div>Administration Tools</div>", "", 0, GetForm)
            Else
                ToolsAction = cpCore.docProperties.getInteger("dta")
                Button = cpCore.docProperties.getText("Button")
                AdminFormTool = cpCore.docProperties.getInteger(RequestNameAdminForm)
                ToolsTable = cpCore.docProperties.getText("dtt")
                ToolsContentName = cpCore.docProperties.getText("ContentName")
                ToolsQuery = cpCore.docProperties.getText("dtq")
                ToolsDataSource = cpCore.docProperties.getText("dtds")
                MenuEntryID = cpCore.docProperties.getInteger("dtei")
                MenuHeaderID = cpCore.docProperties.getInteger("dthi")
                MenuDirection = cpCore.docProperties.getInteger("dtmd")
                DefaultReadOnly = cpCore.docProperties.getBoolean("dtdreadonly")
                DefaultActive = cpCore.docProperties.getBoolean("dtdactive")
                DefaultPassword = cpCore.docProperties.getBoolean("dtdpassword")
                DefaulttextBuffered = cpCore.docProperties.getBoolean("dtdtextbuffered")
                DefaultRequired = cpCore.docProperties.getBoolean("dtdrequired")
                DefaultAdminOnly = cpCore.docProperties.getBoolean("dtdadmin")
                DefaultDeveloperOnly = cpCore.docProperties.getBoolean("dtddev")
                defaultAddMenu = cpCore.docProperties.getBoolean("dtdam")
                DefaultCreateBlankRecord = cpCore.docProperties.getBoolean("dtblank")
                '
                Call cpCore.doc.addRefreshQueryString("dta", ToolsAction.ToString())
                'Call cpCore.main_AddRefreshQueryString("Button=" & Button)
                'Call cpCore.main_AddRefreshQueryString("af=" & AdminFormTool)
                '
                debugController.debug_testPoint(cpCore, "Button = " & Button)
                debugController.debug_testPoint(cpCore, "ToolsQuery = " & ToolsQuery)
                debugController.debug_testPoint(cpCore, "ToolsDataSource = " & ToolsDataSource)
                debugController.debug_testPoint(cpCore, "AdminFormTool = " & AdminFormTool)
                '
                '
                '
                If (Button = ButtonCancel) Then
                    '
                    ' Cancel
                    '
                    Call cpCore.webServer.redirect(cpCore.siteProperties.getText("AdminURL", "/admin/"))
                    'If AdminFormTool = AdminFormToolRoot Then
                    '    '
                    '    ' Cancel to the admin site
                    '    '
                    '    Call cpCore.main_Redirect(cpCore.main_GetSiteProperty("AdminURL", "/admin/"))
                    'Else
                    '    '
                    '    ' cancel to the root form
                    '    '
                    '    Call Stream.Add(GetForm_Root)
                    'End If
                Else
                    '
                    '--------------------------------------------------------------------------------
                    ' Print out the page
                    '--------------------------------------------------------------------------------
                    '
                    Select Case AdminFormTool
                        Case AdminFormToolContentDiagnostic
                            '
                            Call Stream.Add(GetForm_ContentDiagnostic)
                        Case AdminFormToolCreateContentDefinition
                            '
                            Call Stream.Add(GetForm_CreateContentDefinition)
                        Case AdminFormToolDefineContentFieldsFromTable
                            '
                            Call Stream.Add(GetForm_DefineContentFieldsFromTable)
                        Case AdminFormToolConfigureListing
                            '
                            Call Stream.Add(GetForm_ConfigureListing)
                        Case AdminFormToolConfigureEdit
                            '
                            'Call Stream.Add(cpCore.addon.execute(guid_ToolConfigureEdit))
                            Call Stream.Add(GetForm_ConfigureEdit(cpCore.cp_forAddonExecutionOnly))
                        Case AdminFormToolManualQuery
                            '
                            Call Stream.Add(GetForm_ManualQuery)
                        Case AdminFormToolCreateChildContent
                            '
                            Call Stream.Add(GetForm_CreateChildContent)
                        Case AdminFormToolSyncTables
                            '
                            Call Stream.Add(GetForm_SyncTables)
                        Case AdminFormToolBenchmark
                            '
                            Call Stream.Add(GetForm_Benchmark)
                        Case AdminFormToolClearContentWatchLink
                            '
                            Call Stream.Add(GetForm_ClearContentWatchLinks)
                        Case AdminFormToolSchema
                            '
                            Call Stream.Add(GetForm_DbSchema)
                        Case AdminFormToolContentFileView
                            '
                            Call Stream.Add(GetForm_ContentFileManager)
                        Case AdminFormToolWebsiteFileView
                            '
                            Call Stream.Add(GetForm_WebsiteFileManager)
                        Case AdminFormToolDbIndex
                            '
                            Call Stream.Add(GetForm_DbIndex)
                        Case AdminFormToolContentDbSchema
                            '
                            Call Stream.Add(GetForm_ContentDbSchema)
                        Case AdminFormToolLogFileView
                            '
                            Call Stream.Add(GetForm_LogFiles)
                        Case AdminFormToolLoadCDef
                            '
                            Call Stream.Add(GetForm_LoadCDef)
                        Case AdminFormToolRestart
                            '
                            Call Stream.Add(GetForm_Restart)
                        Case AdminFormToolLoadTemplates
                            '
                            Call Stream.Add(GetForm_LoadTemplates)
                        Case AdminformToolFindAndReplace
                            '
                            Call Stream.Add(GetForm_FindAndReplace())
                        Case AdminformToolIISReset
                            '
                            Call Stream.Add(GetForm_IISReset())
                        Case AdminformToolCreateGUID
                            '
                            Call Stream.Add(GetForm_CreateGUID())
                        Case Else
                            '
                            debugController.debug_testPoint(cpCore, "Taking AdminFormToolRoot case")
                            Call Stream.Add(GetForm_Root)
                    End Select
                End If
                '        Call Stream.Add("</blockquote>")
                GetForm = Stream.Text
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            throw (New ApplicationException("Unexpected exception")) '  throw (New ApplicationException("Unexpected exception"))'  Call handleLegacyClassErrors1("GetForm", "ErrorTrap")
        End Function
        '
        '=============================================================================
        '   Remove all Content Fields and rebuild them from the fields found in a table
        '=============================================================================
        '
        Private Function GetForm_DefineContentFieldsFromTable() As String
            On Error GoTo ErrorTrap
            '
            Dim SQL As String
            Dim CS As Integer
            Dim DataSourceName As String
            Dim TableName As String
            Dim ContentName As String
            Dim ContentID As Integer
            Dim DataSourceCount As Integer
            Dim ItemCount As Integer
            Dim CSPointer As Integer
            Dim Stream As New stringBuilderLegacyController
            Dim ButtonList As String = ""
            '
            Stream.Add(SpanClassAdminNormal & "<strong><A href=""" & cpCore.webServer.requestPage & "?af=" & AdminFormToolRoot & """>Tools</A></strong></SPAN>")
            Stream.Add(SpanClassAdminNormal & ":Create Content Fields from Table</SPAN>")
            '
            '   print out the submit form
            '
            Stream.Add("<table border=""0"" cellpadding=""11"" cellspacing=""0"" width=""100%"">")
            '
            Stream.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal)
            Stream.Add("Delete the current content field definitions for this Content Definition, and recreate them from the table referenced by this content.")
            Stream.Add("</SPAN></td></tr>")
            '
            Stream.Add("<tr>")
            Stream.Add("<TD>" & SpanClassAdminNormal & "Content Name</SPAN></td>")
            Stream.Add("<TD><Select name=""ContentName"">")
            ItemCount = 0
            CSPointer = cpCore.db.cs_open("Content", "", "name")
            Do While cpCore.db.cs_ok(CSPointer)
                Stream.Add("<option value=""" & cpCore.db.cs_getText(CSPointer, "name") & """>" & cpCore.db.cs_getText(CSPointer, "name") & "</option>")
                ItemCount = ItemCount + 1
                cpCore.db.cs_goNext(CSPointer)
            Loop
            If ItemCount = 0 Then
                Stream.Add("<option value=""-1"">System</option>")
            End If
            Stream.Add("</select></td>")
            Stream.Add("</tr>")
            '
            Stream.Add("<tr>")
            Stream.Add("<TD>&nbsp;</td>")
            Stream.Add("<TD><INPUT type=""submit"" value=""" & ButtonCreateFields & """ name=""Button""></td>")
            Stream.Add("</tr>")
            '
            Stream.Add("<tr>")
            Stream.Add("<td width=""150""><IMG alt="""" src=""/ccLib/Images/spacer.gif"" width=""150"" height=""1""></td>")
            Stream.Add("<td width=""99%""><IMG alt="""" src=""/ccLib/Images/spacer.gif"" width=""100%"" height=""1""></td>")
            Stream.Add("</tr>")
            Stream.Add("</TABLE>")
            Stream.Add("</form>")
            '
            '   process the button if present
            '
            If Button = ButtonCreateFields Then
                ContentName = cpCore.docProperties.getText("ContentName")
                If (ContentName = "") Then
                    Stream.Add("Select a content before submitting. Fields were not changed.")
                Else
                    ContentID = Local_GetContentID(ContentName)
                    If (ContentID = 0) Then
                        Stream.Add("GetContentID failed. Fields were not changed.")
                    Else
                        Call cpCore.db.deleteContentRecords("Content Fields", "ContentID=" & cpCore.db.encodeSQLNumber(ContentID))
                    End If
                End If
            End If
            '
            GetForm_DefineContentFieldsFromTable = genericLegacyView.OpenFormTable(cpCore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpCore, ButtonList)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  throw (New ApplicationException("Unexpected exception"))'  Call handleLegacyClassErrors1("GetForm_DefineContentFieldsFromTable", "ErrorTrap")
        End Function
        '
        Private Class fieldSortClass
            Public sort As String
            Public field As CDefFieldModel
        End Class
        '
        '=============================================================================
        '   prints a menu of Tools in a 100% table
        '=============================================================================
        '
        Private Function GetForm_Root() As String
            On Error GoTo ErrorTrap
            '
            Dim Stream As New stringBuilderLegacyController
            Dim ButtonList As String
            '
            ButtonList = ButtonCancelAll
            '
            Stream.Add("<P class=""ccAdminNormal""><b>Tools</b></P>")
            Stream.Add("<table border=""0"" cellpadding=""4"" cellspacing=""0"" width=""100%"">")
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolContentDiagnostic, "Run Content Diagnostic", "General diagnostic routine that runs through the most common data structure errors."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolManualQuery, "Run Manual Query", "Run a query through Contensives database interface to change data directly, or simulate the activity of a errant query."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolCreateContentDefinition, "Create Content Definition", "Create a new content definition. If the required SQL table does not exist, it will also be created. If the SQL table exists, its fields will be included in the Content Definition. All required Content Definition fields will be added to the SQL table in either case."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolCreateChildContent, "Create Content Definition Child", "Create a new content definition based on a specific current definition, and make the new definition a hierarchical child of the original."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolConfigureEdit, "Edit Content Definition Fields", "Modify the administration interface editing page for a specific content definition."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolConfigureListing, "Edit Content Definition Admin Site Listing Columns", "Modify the administration interface listings page for a specific content definition. This is the first form you come to when selecting data to be modified."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolClearContentWatchLink, "Clear ContentWatch Links", "Clear all content watch links so they can be repopulated. Use this tool whenever there has been a domain name change, or errant links are found in the Whats New functionality."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolSyncTables, "Synchronize SQL Tables to their Content Definitions", "Synchronize the Database tables to match the current content definitions. Add any necessary table fields, but do not remove unused fields."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolBenchmark, "Benchmark Current System", "Run a series of data operations and compare the result to previous benchmarks."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolSchema, "Get Database Schema", "Get the database schema for any table."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolContentFileView, "Content File View", "View files and folders in the Content Files area."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolDbIndex, "Modify Database Indexes", "Add or remove Database indexes."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolContentDbSchema, "Get the Content Database Schema", "View the database schema required by the Content Definitions on this site."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolLogFileView, "Get the Content Log Files", "View the Contensive Log Files."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolLoadCDef, "Load the Content Definitions", "Reload the Contensive Content Definitions."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolLoadTemplates, "Import Templates from the wwwRoot folder", "Use this tool to create template records from the HTML files in the root folder of the site."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminformToolFindAndReplace, "Find and Replace", "Use this tool to replace text strings in content throughout the site."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminformToolIISReset, "IIS Reset", "IIS Reset the server. This will stop all sites on the server for a short period."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminFormToolRestart, "Contensive Application Restart", "Restart the Contensive Applicaiton. This will stop your site on the server for a short period."))
            Stream.Add(GetForm_RootRow(AdminFormTools, AdminformToolCreateGUID, "Create GUID", "Use this tool to create a new GUID. This is useful when creating a new cpcore.addon."))
            Stream.Add("</table>")
            GetForm_Root = genericLegacyView.OpenFormTable(cpCore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpCore, ButtonList)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  throw (New ApplicationException("Unexpected exception"))'  Call handleLegacyClassErrors1("GetForm_Root", "ErrorTrap")
        End Function
        '
        '==================================================================================
        '
        '==================================================================================
        '
        Private Function GetForm_RootRow(ByVal AdminFormId As Integer, ByVal AdminFormToolId As Integer, ByVal Caption As String, ByVal Description As String) As String
            On Error GoTo ErrorTrap
            '
            GetForm_RootRow = ""
            GetForm_RootRow = GetForm_RootRow & "<tr><td colspan=""2"">"
            GetForm_RootRow = GetForm_RootRow & SpanClassAdminNormal & "<P class=""ccAdminNormal""><A href=""" & cpCore.webServer.requestPage & "?af=" & AdminFormToolId.ToString() & """><B>" & Caption & "</b></SPAN></A></p>"
            GetForm_RootRow = GetForm_RootRow & "</td></tr>"
            If Description <> "" Then
                GetForm_RootRow = GetForm_RootRow & "<tr><td width=""30""><img src=""/ccLib/images/spacer.gif"" height=""1"" width=""30""></td>"
                GetForm_RootRow = GetForm_RootRow & "<td width=""100%""><P class=""ccAdminsmall"">" & Description & "</p></td></tr>"
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  throw (New ApplicationException("Unexpected exception"))'  Call handleLegacyClassErrors1("GetForm_RootRow", "ErrorTrap")
        End Function
        '
        '==================================================================================
        '
        '==================================================================================
        '
        Private Function GetTitle(ByVal Title As String, ByVal Description As String) As String
            Dim result As String = ""
            Try
                result = "" _
                    & "<p>" & SpanClassAdminNormal _
                    & "<a href=""" & cpCore.webServer.requestPage & "?af=" & AdminFormToolRoot & """><b>Tools</b></a>&nbsp;&gt;&nbsp;" & Title _
                    & "</p>" _
                    & "<p>" & SpanClassAdminNormal & Description & "</p>"
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '
        '=============================================================================
        '   Print the manual query form
        '=============================================================================
        '
        Private Function GetForm_ManualQuery() As String
            Dim returnHtml As String = ""
            Try
                'Dim DataSourceName As String
                'Dim DataSourceID As Integer
                Dim SQL As String = ""
                Dim dt As DataTable = Nothing
                Dim FieldCount As Integer
                Dim SQLFilename As String
                Dim SQLArchive As String
                Dim SQLArchiveOld As String
                Dim LineCounter As Integer
                Dim SQLLine As String
                Dim Timeout As Integer
                Dim PageSize As Integer
                Dim PageNumber As Integer
                Dim RowMax As Integer
                Dim RowPointer As Integer
                Dim ColumnMax As Integer
                Dim ColumnPointer As Integer
                Dim ColumnStart As String
                Dim ColumnEnd As String
                Dim RowStart As String
                Dim RowEnd As String
                Dim resultArray As String(,)
                Dim CellData As String
                Dim SelectFieldWidthLimit As Integer
                Dim SQLName As String
                Dim Stream As New stringBuilderLegacyController
                Dim ButtonList As String
                Dim datasource As dataSourceModel = dataSourceModel.create(cpCore, cpCore.docProperties.getInteger("dataSourceid"), New List(Of String))
                '
                ButtonList = ButtonCancel & "," & ButtonRun
                '
                Stream.Add(GetTitle("Run Manual Query", "This tool runs an SQL statement on a selected datasource. If there is a result set, the set is printed in a table."))
                '
                ' Get the members SQL Queue
                '
                SQLFilename = cpCore.userProperty.getText("SQLArchive")
                If SQLFilename = "" Then
                    SQLFilename = "SQLArchive" & Format(cpCore.authContext.user.id, "000000000") & ".txt"
                    Call cpCore.userProperty.setProperty("SQLArchive", SQLFilename)
                End If
                SQLArchive = cpCore.cdnFiles.readFile(SQLFilename)
                '
                ' Read in arguments if available
                '
                Timeout = cpCore.docProperties.getInteger("Timeout")
                If Timeout = 0 Then
                    Timeout = 30
                End If
                '
                PageSize = cpCore.docProperties.getInteger("PageSize")
                If PageSize = 0 Then
                    PageSize = 10
                End If
                '
                PageNumber = cpCore.docProperties.getInteger("PageNumber")
                If PageNumber = 0 Then
                    PageNumber = 1
                End If
                '
                SQL = cpCore.docProperties.getText("SQL")
                If SQL = "" Then
                    SQL = cpCore.docProperties.getText("SQLList")
                End If
                '
                If (cpCore.docProperties.getText("button")) = ButtonRun Then
                    '
                    ' Add this SQL to the members SQL list
                    '
                    If SQL <> "" Then
                        SQLArchive = genericController.vbReplace(SQLArchive, SQL & vbCrLf, "")
                        SQLArchiveOld = SQLArchive
                        SQLArchive = genericController.vbReplace(SQL, vbCrLf, " ") & vbCrLf
                        LineCounter = 0
                        Do While (LineCounter < 10) And (SQLArchiveOld <> "")
                            SQLArchive = SQLArchive & getLine(SQLArchiveOld) & vbCrLf
                        Loop
                        Call cpCore.appRootFiles.saveFile(SQLFilename, SQLArchive)
                    End If
                    '
                    ' Run the SQL
                    '
                    Call Stream.Add("<P>" & SpanClassAdminSmall)
                    Stream.Add(Now() & " Executing sql [" & SQL & "] on DataSource [" & datasource.Name & "]")
                    Try
                        dt = cpCore.db.executeSql(SQL, datasource.Name, PageSize * (PageNumber - 1), PageSize)
                    Catch ex As Exception
                        '
                        ' ----- error
                        '
                        Stream.Add("<br>" & Now() & " SQL execution returned the following error")
                        Stream.Add("<br>" & ex.Message)
                    End Try
                    Stream.Add("<br>" & Now() & " SQL executed successfully")
                    If (dt Is Nothing) Then
                        Stream.Add("<br>" & Now() & " SQL returned invalid data.")
                    ElseIf (dt.Rows Is Nothing) Then
                        Stream.Add("<br>" & Now() & " SQL returned invalid data rows.")
                    ElseIf (dt.Rows.Count = 0) Then
                        Stream.Add("<br>" & Now() & " The SQL returned no data.")
                    Else
                        '
                        ' ----- print results
                        '
                        Call Stream.Add("<br>" & Now() & " The following results were returned")
                        Call Stream.Add("<br>")
                        '
                        ' --- Create the Fields for the new table
                        '
                        FieldCount = dt.Columns.Count
                        Stream.Add("<table border=""1"" cellpadding=""1"" cellspacing=""1"" width=""100%"">")
                        Stream.Add("<tr>")
                        For Each dc As DataColumn In dt.Columns
                            Call Stream.Add("<TD><B>" & SpanClassAdminSmall & dc.ColumnName & "</b></SPAN></td>")
                        Next
                        Stream.Add("</tr>")
                        '
                        'Dim dtOK As Boolean
                        resultArray = cpCore.db.convertDataTabletoArray(dt)
                        '
                        RowMax = UBound(resultArray, 2)
                        ColumnMax = UBound(resultArray, 1)
                        RowStart = "<tr>"
                        RowEnd = "</tr>"
                        ColumnStart = "<td class=""ccadminsmall"">"
                        ColumnEnd = "</td>"
                        For RowPointer = 0 To RowMax
                            Stream.Add(RowStart)
                            For ColumnPointer = 0 To ColumnMax
                                CellData = resultArray(ColumnPointer, RowPointer)
                                If IsNull(CellData) Then
                                    Stream.Add(ColumnStart & "[null]" & ColumnEnd)
                                    'ElseIf IsEmpty(CellData) Then
                                    '    Stream.Add(ColumnStart & "[empty]" & ColumnEnd)
                                    'ElseIf IsArray(CellData) Then
                                    '    Stream.Add(ColumnStart & "[array]")
                                    '    Cnt = UBound(CellData)
                                    '    For Ptr = 0 To Cnt - 1
                                    '        Stream.Add("<br>(" & Ptr & ")&nbsp;[" & CellData(Ptr) & "]")
                                    '    Next
                                    '    Stream.Add(ColumnEnd)
                                ElseIf CellData = "" Then
                                    Stream.Add(ColumnStart & "[empty]" & ColumnEnd)
                                Else
                                    Stream.Add(ColumnStart & cpCore.html.html_EncodeHTML(genericController.encodeText(CellData)) & ColumnEnd)
                                End If
                            Next
                            Stream.Add(RowEnd)
                        Next
                        Stream.Add("</TABLE>")
                    End If
                    Call Stream.Add("<br>" & Now() & " Done")
                    Call Stream.Add("</P>")
                    '
                    ' End of Run SQL
                    '
                End If
                '
                ' Display form
                '
                'Call Stream.Add(SpanClassAdminNormal & cpCore.main_GetFormStart(""))
                '
                Dim SQLRows As Integer
                SQLRows = cpCore.docProperties.getInteger("SQLRows")
                If SQLRows = 0 Then
                    SQLRows = cpCore.userProperty.getInteger("ManualQueryInputRows", 5)
                Else
                    Call cpCore.userProperty.setProperty("ManualQueryInputRows", CStr(SQLRows))
                End If
                Call Stream.Add("<TEXTAREA NAME=""SQL"" ROWS=""" & SQLRows & """ ID=""SQL"" STYLE=""width: 800px;"">" & SQL & "</TEXTAREA>")
                Call Stream.Add("&nbsp;<INPUT TYPE=""Text"" TabIndex=-1 NAME=""SQLRows"" SIZE=""3"" VALUE=""" & SQLRows & """ ID=""""  onchange=""SQL.rows=SQLRows.value; return true""> Rows")
                Call Stream.Add("<br><br>Data Source<br>" & cpCore.html.main_GetFormInputSelect("DataSourceID", datasource.ID, "Data Sources", "", "Default"))
                '
                SelectFieldWidthLimit = cpCore.siteProperties.getinteger("SelectFieldWidthLimit", 200)
                If SQLArchive <> "" Then
                    Call Stream.Add("<br><br>Previous Queries<br>")
                    Call Stream.Add("<select size=""1"" name=""SQLList"" ID=""SQLList"" onChange=""SQL.value=SQLList.value"">")
                    Call Stream.Add("<option value="""">Select One</option>")
                    LineCounter = 0
                    Do While (LineCounter < 10) And (SQLArchive <> "")
                        SQLLine = getLine(SQLArchive)
                        If Len(SQLLine) > SelectFieldWidthLimit Then
                            SQLName = Mid(SQLLine, 1, SelectFieldWidthLimit) & "..."
                        Else
                            SQLName = SQLLine
                        End If
                        Call Stream.Add("<option value=""" & SQLLine & """>" & SQLName & "</option>")
                    Loop
                    Call Stream.Add("</select><br>")
                End If
                '
                If IsNull(PageSize) Then
                    PageSize = 100
                End If
                Call Stream.Add("<br>Page Size:<br>" & cpCore.html.html_GetFormInputText("PageSize", PageSize.ToString()))
                '
                If IsNull(PageNumber) Then
                    PageNumber = 1
                End If
                Call Stream.Add("<br>Page Number:<br>" & cpCore.html.html_GetFormInputText("PageNumber", PageNumber.ToString()))
                '
                If IsNull(Timeout) Then
                    Timeout = 30
                End If
                Call Stream.Add("<br>Timeout (sec):<br>" & cpCore.html.html_GetFormInputText("Timeout", Timeout.ToString()))
                '
                'Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolManualQuery))
                'Stream.Add( cpCore.main_GetFormEnd & "</SPAN>")
                '
                returnHtml = genericLegacyView.OpenFormTable(cpCore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpCore, ButtonList)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnHtml
        End Function
        '
        '=============================================================================
        ' Create a Content Definition from a table
        '=============================================================================
        '
        Private Function GetForm_CreateContentDefinition() As String
            On Error GoTo ErrorTrap
            '
            Dim CS As Integer
            Dim ContentID As Integer
            Dim TableName As String = ""
            Dim ContentName As String = ""
            Dim Stream As New stringBuilderLegacyController
            Dim ButtonList As String
            Dim Adminui As New adminUIController(cpCore)
            Dim Description As String
            Dim Caption As String
            Dim NavID As Integer
            Dim ParentNavID As Integer
            Dim datasource As dataSourceModel = dataSourceModel.create(cpCore, cpCore.docProperties.getInteger("DataSourceID"), New List(Of String))
            '
            ButtonList = ButtonCancel & "," & ButtonRun
            Caption = "Create Content Definition"
            Description = "This tool creates a Content Definition. If the SQL table exists, it is used. If it does not exist, it is created. If records exist in the table with a blank ContentControlID, the ContentControlID will be populated from this new definition. A Navigator Menu entry will be added under Manage Site Content - Advanced."
            '
            'Stream.Add( GetTitle(Caption, Description)
            '
            '   print out the submit form
            '
            If (cpCore.docProperties.getText("Button") <> "") Then
                '
                ' Process input
                '
                ContentName = cpCore.docProperties.getText("ContentName")
                TableName = cpCore.docProperties.getText("TableName")
                '
                Call Stream.Add(SpanClassAdminSmall)
                Stream.Add("<P>Creating content [" & ContentName & "] on table [" & TableName & "] on Datasource [" & datasource.Name & "].</P>")
                If (ContentName <> "") And (TableName <> "") And (datasource.Name <> "") Then
                    Call cpCore.db.createSQLTable(datasource.Name, TableName)
                    Call cpCore.db.createContentFromSQLTable(datasource, TableName, ContentName)
                    cpCore.cache.invalidateAll()
                    cpCore.metaData.clear()
                    ContentID = cpCore.metaData.getContentId(ContentName)
                    ParentNavID = cpCore.db.getRecordID("Navigator Entries", "Manage Site Content")
                    If ParentNavID <> 0 Then
                        CS = cpCore.db.cs_open("Navigator Entries", "(name=" & cpCore.db.encodeSQLText("Advanced") & ")and(parentid=" & ParentNavID & ")")
                        ParentNavID = 0
                        If cpCore.db.cs_ok(CS) Then
                            ParentNavID = cpCore.db.cs_getInteger(CS, "ID")
                        End If
                        Call cpCore.db.cs_Close(CS)
                        If ParentNavID <> 0 Then
                            CS = cpCore.db.cs_open("Navigator Entries", "(name=" & cpCore.db.encodeSQLText(ContentName) & ")and(parentid=" & NavID & ")")
                            If Not cpCore.db.cs_ok(CS) Then
                                Call cpCore.db.cs_Close(CS)
                                CS = cpCore.db.cs_insertRecord("Navigator Entries")
                            End If
                            If cpCore.db.cs_ok(CS) Then
                                Call cpCore.db.cs_set(CS, "name", ContentName)
                                Call cpCore.db.cs_set(CS, "parentid", ParentNavID)
                                Call cpCore.db.cs_set(CS, "contentid", ContentID)
                            End If
                            Call cpCore.db.cs_Close(CS)
                        End If
                    End If
                    Call Controllers.appBuilderController.admin_VerifyAdminMenu(cpCore, "Site Content", ContentName, ContentName, "", "")
                    ContentID = cpCore.metaData.getContentId(ContentName)
                    Call Stream.Add("<P>Content Definition was created. An admin menu entry for this definition has been added under 'Site Content', and will be visible on the next page view. Use the [<a href=""?af=105&ContentID=" & ContentID & """>Edit Content Definition Fields</a>] tool to review and edit this definition's fields.</P>")
                Else
                    Stream.Add("<P>Error, a required field is missing. Content not created.</P>")
                End If
                Call Stream.Add("</SPAN>")
            End If
            Call Stream.Add(SpanClassAdminNormal)
            Call Stream.Add("Data Source<br>")
            Call Stream.Add(cpCore.html.main_GetFormInputSelect("DataSourceID", datasource.ID, "Data Sources", "", "Default"))
            Call Stream.Add("<br><br>")
            Call Stream.Add("Content Name<br>")
            Call Stream.Add(cpCore.html.html_GetFormInputText2("ContentName", ContentName, 1, 40))
            Call Stream.Add("<br><br>")
            Call Stream.Add("Table Name<br>")
            Call Stream.Add(cpCore.html.html_GetFormInputText2("TableName", TableName, 1, 40))
            Call Stream.Add("<br><br>")
            'Call Stream.Add(cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolCreateContentDefinition))
            Call Stream.Add("</SPAN>")
            '
            GetForm_CreateContentDefinition = Adminui.GetBody(Caption, ButtonList, "", False, False, Description, "", 10, Stream.Text)
            'GetForm_CreateContentDefinition = genericLegacyView.OpenFormTable(cpcore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpcore,ButtonList)

            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  throw (New ApplicationException("Unexpected exception"))'  Call handleLegacyClassErrors1("GetForm_CreateContentDefinition", "ErrorTrap")
        End Function
        '
        '=============================================================================
        '   Print the Configure Listing Page
        '=============================================================================
        '
        Private Function GetForm_ConfigureListing() As String
            On Error GoTo ErrorTrap
            '
            Dim SQL As String
            Dim MenuHeader As String
            'Dim ColumnCount As Integer
            Dim ColumnWidth As Integer
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim FieldName As String
            Dim FieldToAdd As Integer
            Dim AStart As String
            Dim CS As Integer
            Dim SetSort As Boolean
            Dim MenuEntryID As Integer
            Dim MenuHeaderID As Integer
            Dim MenuDirection As Integer
            Dim SourceID As Integer
            Dim PreviousID As Integer
            Dim SetID As Integer
            Dim NextSetID As Integer
            Dim SwapWithPrevious As Boolean
            Dim HitID As Integer
            Dim HitTable As String
            Dim SortPriorityLowest As Integer
            Dim TempColumn As String
            Dim Tempfield As String
            Dim TempWidth As String
            Dim TempSortPriority As Integer
            Dim TempSortDirection As Integer
            Dim CSPointer As Integer
            Dim RecordID As Integer
            Dim ContentID As Integer
            Dim CDef As cdefModel
            'Dim AdminColumn As appServices_metaDataClass.CDefAdminColumnClass
            Dim RowFieldID() As Integer
            Dim RowFieldWidth() As Integer
            Dim RowFieldCaption() As String
            'Dim RowFieldCount as integer
            Dim NonRowFieldID() As Integer
            Dim NonRowFieldCaption() As String
            Dim NonRowFieldCount As Integer
            Dim ContentName As String
            '
            Dim dt As DataTable
            Dim IndexWidth As Integer
            Dim CS1 As Integer
            Dim CS2 As Integer
            ' Dim FieldPointer1 As Integer
            Dim FieldPointer2 As Integer
            Dim NewRowFieldWidth As Integer
            Dim TargetFieldID As Integer
            '
            Dim ColumnWidthTotal As Integer
            Dim ColumnNumberMax As Integer
            '
            'Dim ColumnPointer As Integer
            'Dim CDefFieldCount As Integer
            Dim fieldId As Integer
            Dim FieldWidth As Integer
            Dim AllowContentAutoLoad As Boolean
            Dim TargetFieldPointer As Integer
            Dim MoveNextColumn As Boolean
            Dim FieldNameToAdd As String
            Dim FieldIDToAdd As Integer
            Dim CSSource As Integer
            Dim CSTarget As Integer
            Dim SourceContentID As Integer
            Dim SourceName As String
            Dim ReloadCDef As Boolean
            Dim InheritedFieldCount As Integer
            Dim Caption As String
            'Dim ContentNameValues() As NameValuePrivateType
            Dim ContentCount As Integer
            Dim ContentSize As Integer
            Dim Stream As New stringBuilderLegacyController
            Dim ButtonList As String
            Dim FormPanel As String = ""
            Dim ColumnWidthIncrease As Integer
            Dim ColumnWidthBalance As Integer
            '
            Const RequestNameAddField = "addfield"
            Const RequestNameAddFieldID = "addfieldID"
            '
            Stream.Add(GetTitle("Configure Admin Listing", "Configure the Administration Content Listing Page."))
            '
            '--------------------------------------------------------------------------------
            '   Load Request
            '--------------------------------------------------------------------------------
            '
            ToolsAction = cpCore.docProperties.getInteger("dta")
            TargetFieldID = cpCore.docProperties.getInteger("fi")
            ContentID = cpCore.docProperties.getInteger(RequestNameToolContentID)
            'ColumnPointer = cpCore.main_GetStreamInteger("dtcn")
            FieldNameToAdd = genericController.vbUCase(cpCore.docProperties.getText(RequestNameAddField))
            FieldIDToAdd = cpCore.docProperties.getInteger(RequestNameAddFieldID)
            ButtonList = ButtonCancel & "," & ButtonSelect
            ReloadCDef = cpCore.docProperties.getBoolean("ReloadCDef")
            '
            '--------------------------------------------------------------------------------
            ' Process actions
            '--------------------------------------------------------------------------------
            '
            If ContentID <> 0 Then
                ButtonList = ButtonCancel & "," & ButtonSaveandInvalidateCache
                ContentName = Local_GetContentNameByID(ContentID)
                CDef = cpCore.metaData.getCdef(ContentID, True, False)
                If ToolsAction <> 0 Then
                    '
                    ' Block contentautoload, then force a load at the end
                    '
                    AllowContentAutoLoad = (cpCore.siteProperties.getBoolean("AllowContentAutoLoad", True))
                    Call cpCore.siteProperties.setProperty("AllowContentAutoLoad", False)
                    '
                    ' Make sure the FieldNameToAdd is not-inherited, if not, create new field
                    '
                    If (FieldIDToAdd <> 0) Then
                        For Each keyValuePair In CDef.fields
                            Dim field As CDefFieldModel = keyValuePair.Value
                            If field.id = FieldIDToAdd Then
                                'If field.Name = FieldNameToAdd Then
                                If field.inherited Then
                                    SourceContentID = field.contentId
                                    SourceName = field.nameLc
                                    CSSource = cpCore.db.cs_open("Content Fields", "(ContentID=" & SourceContentID & ")and(Name=" & cpCore.db.encodeSQLText(SourceName) & ")")
                                    If cpCore.db.cs_ok(CSSource) Then
                                        CSTarget = cpCore.db.cs_insertRecord("Content Fields")
                                        If cpCore.db.cs_ok(CSTarget) Then
                                            Call cpCore.db.cs_copyRecord(CSSource, CSTarget)
                                            Call cpCore.db.cs_set(CSTarget, "ContentID", ContentID)
                                            ReloadCDef = True
                                        End If
                                        Call cpCore.db.cs_Close(CSTarget)
                                    End If
                                    Call cpCore.db.cs_Close(CSSource)
                                End If
                                Exit For
                            End If
                        Next
                    End If
                    '
                    ' Make sure all fields are not-inherited, if not, create new fields
                    '
                    ColumnNumberMax = 0
                    For Each keyValuePair In CDef.adminColumns
                        Dim adminColumn As cdefModel.CDefAdminColumnClass = keyValuePair.Value
                        Dim field As CDefFieldModel = CDef.fields(adminColumn.Name)
                        If field.inherited Then
                            SourceContentID = field.contentId
                            SourceName = field.nameLc
                            CSSource = cpCore.db.cs_open("Content Fields", "(ContentID=" & SourceContentID & ")and(Name=" & cpCore.db.encodeSQLText(SourceName) & ")")
                            If cpCore.db.cs_ok(CSSource) Then
                                CSTarget = cpCore.db.cs_insertRecord("Content Fields")
                                If cpCore.db.cs_ok(CSTarget) Then
                                    Call cpCore.db.cs_copyRecord(CSSource, CSTarget)
                                    Call cpCore.db.cs_set(CSTarget, "ContentID", ContentID)
                                    ReloadCDef = True
                                End If
                                Call cpCore.db.cs_Close(CSTarget)
                            End If
                            Call cpCore.db.cs_Close(CSSource)
                        End If
                        If ColumnNumberMax < field.indexColumn Then
                            ColumnNumberMax = field.indexColumn
                        End If
                        ColumnWidthTotal = ColumnWidthTotal + adminColumn.Width
                    Next
                    '
                    ' ----- Perform any actions first
                    '
                    Dim columnPtr As Integer
                    Select Case ToolsAction
                        Case ToolsActionAddField
                            '
                            ' Add a field to the Listing Page
                            '
                            If FieldIDToAdd <> 0 Then
                                'If FieldNameToAdd <> "" Then
                                columnPtr = 0
                                If CDef.adminColumns.Count > 1 Then
                                    For Each keyValuePair In CDef.adminColumns
                                        Dim adminColumn As cdefModel.CDefAdminColumnClass = keyValuePair.Value
                                        Dim field As CDefFieldModel = CDef.fields(adminColumn.Name)
                                        CSPointer = cpCore.db.csOpenRecord("Content Fields", field.id)
                                        Call cpCore.db.cs_set(CSPointer, "IndexColumn", (columnPtr) * 10)
                                        Call cpCore.db.cs_set(CSPointer, "IndexWidth", Int((adminColumn.Width * 80) / ColumnWidthTotal))
                                        Call cpCore.db.cs_Close(CSPointer)
                                        columnPtr += 1
                                    Next
                                End If
                                CSPointer = cpCore.db.csOpenRecord("Content Fields", FieldIDToAdd, False, False)
                                If cpCore.db.cs_ok(CSPointer) Then
                                    Call cpCore.db.cs_set(CSPointer, "IndexColumn", columnPtr * 10)
                                    Call cpCore.db.cs_set(CSPointer, "IndexWidth", 20)
                                    Call cpCore.db.cs_set(CSPointer, "IndexSortPriority", 99)
                                    Call cpCore.db.cs_set(CSPointer, "IndexSortDirection", 1)
                                End If
                                Call cpCore.db.cs_Close(CSPointer)
                                ReloadCDef = True
                            End If
                            '
                        Case ToolsActionRemoveField
                            '
                            ' Remove a field to the Listing Page
                            '
                            If CDef.adminColumns.Count > 1 Then
                                columnPtr = 0
                                For Each keyValuePair In CDef.adminColumns
                                    Dim adminColumn As cdefModel.CDefAdminColumnClass = keyValuePair.Value
                                    Dim field As CDefFieldModel = CDef.fields(adminColumn.Name)
                                    CSPointer = cpCore.db.csOpenRecord("Content Fields", field.id)
                                    If fieldId = TargetFieldID Then
                                        Call cpCore.db.cs_set(CSPointer, "IndexColumn", 0)
                                        Call cpCore.db.cs_set(CSPointer, "IndexWidth", 0)
                                        Call cpCore.db.cs_set(CSPointer, "IndexSortPriority", 0)
                                        Call cpCore.db.cs_set(CSPointer, "IndexSortDirection", 0)
                                    Else
                                        Call cpCore.db.cs_set(CSPointer, "IndexColumn", (columnPtr) * 10)
                                        Call cpCore.db.cs_set(CSPointer, "IndexWidth", Int((adminColumn.Width * 100) / ColumnWidthTotal))
                                    End If
                                    Call cpCore.db.cs_Close(CSPointer)
                                    columnPtr += 1
                                Next
                                ReloadCDef = True
                            End If
                        Case ToolsActionMoveFieldRight
                            '
                            ' Move column field right
                            '
                            If CDef.adminColumns.Count > 1 Then
                                MoveNextColumn = False
                                columnPtr = 0
                                For Each keyValuePair In CDef.adminColumns
                                    Dim adminColumn As cdefModel.CDefAdminColumnClass = keyValuePair.Value
                                    Dim field As CDefFieldModel = CDef.fields(adminColumn.Name)
                                    FieldName = adminColumn.Name
                                    CS1 = cpCore.db.csOpenRecord("Content Fields", field.id)
                                    If (CDef.fields(FieldName.ToLower()).id = TargetFieldID) And (columnPtr < CDef.adminColumns.Count) Then
                                        Call cpCore.db.cs_set(CS1, "IndexColumn", (columnPtr + 1) * 10)
                                        '
                                        MoveNextColumn = True
                                    ElseIf MoveNextColumn Then
                                        '
                                        ' This is one past target
                                        '
                                        Call cpCore.db.cs_set(CS1, "IndexColumn", (columnPtr - 1) * 10)
                                        MoveNextColumn = False
                                    Else
                                        '
                                        ' not target or one past target
                                        '
                                        Call cpCore.db.cs_set(CS1, "IndexColumn", (columnPtr) * 10)
                                        MoveNextColumn = False
                                    End If
                                    Call cpCore.db.cs_set(CS1, "IndexWidth", Int((adminColumn.Width * 100) / ColumnWidthTotal))
                                    Call cpCore.db.cs_Close(CS1)
                                    columnPtr += 1
                                Next
                                ReloadCDef = True
                            End If
                            ' end case
                        Case ToolsActionMoveFieldLeft
                            '
                            ' Move Index column field left
                            '
                            If CDef.adminColumns.Count > 1 Then
                                MoveNextColumn = False
                                columnPtr = 0
                                For Each keyValuePair In CDef.adminColumns.Reverse
                                    Dim adminColumn As cdefModel.CDefAdminColumnClass = keyValuePair.Value
                                    Dim field As CDefFieldModel = CDef.fields(adminColumn.Name)
                                    FieldName = adminColumn.Name
                                    CS1 = cpCore.db.csOpenRecord("Content Fields", field.id)
                                    If (field.id = TargetFieldID) And (columnPtr < CDef.adminColumns.Count) Then
                                        Call cpCore.db.cs_set(CS1, "IndexColumn", (columnPtr - 1) * 10)
                                        '
                                        MoveNextColumn = True
                                    ElseIf MoveNextColumn Then
                                        '
                                        ' This is one past target
                                        '
                                        Call cpCore.db.cs_set(CS1, "IndexColumn", (columnPtr + 1) * 10)
                                        MoveNextColumn = False
                                    Else
                                        '
                                        ' not target or one past target
                                        '
                                        Call cpCore.db.cs_set(CS1, "IndexColumn", (columnPtr) * 10)
                                        MoveNextColumn = False
                                    End If
                                    Call cpCore.db.cs_set(CS1, "IndexWidth", Int((adminColumn.Width * 100) / ColumnWidthTotal))
                                    Call cpCore.db.cs_Close(CS1)
                                    columnPtr += 1
                                Next
                                ReloadCDef = True
                            End If
                            ' end case
                            'Case ToolsActionSetAZ
                            '    '
                            '    ' Set Column as a-z sort
                            '    '
                            '    If CDef.adminColumns.Count > 1 Then
                            '        For ColumnPointer = 0 To CDef.adminColumns.Count - 1
                            '            FieldName = CDef.adminColumns(ColumnPointer).Name
                            '            fieldId = CDef.fields(FieldName.ToLower()).Id
                            '            CSPointer = cpCore.main_OpenCSContentRecord("Content Fields", fieldId)
                            '            If fieldId = TargetFieldID Then
                            '                Call cpCore.app.SetCS(CSPointer, "IndexSortPriority", 0)
                            '                Call cpCore.app.SetCS(CSPointer, "IndexSortDirection", 1)
                            '            Else
                            '                Call cpCore.app.SetCS(CSPointer, "IndexSortPriority", 99)
                            '                Call cpCore.app.SetCS(CSPointer, "IndexSortDirection", 0)
                            '            End If
                            '            Call cpCore.app.SetCS(CSPointer, "IndexColumn", (ColumnPointer) * 10)
                            '            Call cpCore.app.SetCS(CSPointer, "IndexWidth", Int((CDef.adminColumns(ColumnPointer).Width * 100) / ColumnWidthTotal))
                            '            Call cpCore.app.closeCS(CSPointer)
                            '        Next
                            '        ReloadCDef = True
                            '    End If
                            '    ' end case
                            'Case ToolsActionSetZA
                            '    '
                            '    ' Set Column as a-z sort
                            '    '
                            '    If CDef.adminColumns.Count > 1 Then
                            '        For ColumnPointer = 0 To CDef.adminColumns.Count - 1
                            '            FieldName = CDef.adminColumns(ColumnPointer).Name
                            '            fieldId = CDef.fields(FieldName.ToLower()).Id
                            '            CSPointer = cpCore.main_OpenCSContentRecord("Content Fields", fieldId)
                            '            If fieldId = TargetFieldID Then
                            '                Call cpCore.app.SetCS(CSPointer, "IndexSortPriority", 0)
                            '                Call cpCore.app.SetCS(CSPointer, "IndexSortDirection", -1)
                            '            Else
                            '                Call cpCore.app.SetCS(CSPointer, "IndexSortPriority", 99)
                            '                Call cpCore.app.SetCS(CSPointer, "IndexSortDirection", 0)
                            '            End If
                            '            Call cpCore.app.SetCS(CSPointer, "IndexColumn", (ColumnPointer) * 10)
                            '            Call cpCore.app.SetCS(CSPointer, "IndexWidth", Int((CDef.adminColumns(ColumnPointer).Width * 100) / ColumnWidthTotal))
                            '            Call cpCore.app.closeCS(CSPointer)
                            '        Next
                            '        ReloadCDef = True
                            '    End If
                            '    ' end case
                            'Case ToolsActionExpand
                            '    '
                            '    ' Expand column
                            '    '
                            '    ColumnWidthBalance = 0
                            '    If CDef.adminColumns.Count > 0 Then
                            '        '
                            '        ' Calculate the total width of the non-target columns
                            '        '
                            '        ColumnWidthIncrease = ColumnWidthTotal * 0.1
                            '        For ColumnPointer = 0 To CDef.adminColumns.Count - 1
                            '            FieldName = CDef.adminColumns(ColumnPointer).Name
                            '            fieldId = CDef.fields(FieldName.ToLower()).Id
                            '            If fieldId <> TargetFieldID Then
                            '                ColumnWidthBalance = ColumnWidthBalance + CDef.adminColumns(ColumnPointer).Width
                            '            End If
                            '        Next
                            '        '
                            '        ' Adjust all columns
                            '        '
                            '        If ColumnWidthBalance > 0 Then
                            '            For ColumnPointer = 0 To CDef.adminColumns.Count - 1
                            '                FieldName = CDef.adminColumns(ColumnPointer).Name
                            '                fieldId = CDef.fields(FieldName.ToLower()).Id
                            '                CSPointer = cpCore.main_OpenCSContentRecord("Content Fields", fieldId)
                            '                If fieldId = TargetFieldID Then
                            '                    '
                            '                    ' Target gets 10% increase
                            '                    '
                            '                    Call cpCore.app.SetCS(CSPointer, "IndexWidth", Int(CDef.adminColumns(ColumnPointer).Width + ColumnWidthIncrease))
                            '                Else
                            '                    '
                            '                    ' non-targets get their share of the shrinkage
                            '                    '
                            '                    FieldWidth = CDef.adminColumns(ColumnPointer).Width
                            '                    FieldWidth = FieldWidth - ((ColumnWidthIncrease * FieldWidth) / ColumnWidthBalance)
                            '                    Call cpCore.app.SetCS(CSPointer, "IndexWidth", Int(FieldWidth))
                            '                End If
                            '                Call cpCore.app.SetCS(CSPointer, "IndexColumn", (ColumnPointer) * 10)
                            '                Call cpCore.app.closeCS(CSPointer)
                            '            Next
                            '            ReloadCDef = True
                            '        End If
                            '    End If

                            '    ' end case
                            'Case ToolsActionContract
                            '    '
                            '    ' Contract column
                            '    '
                            '    ColumnWidthBalance = 0
                            '    If CDef.adminColumns.Count > 0 Then
                            '        '
                            '        ' Calculate the total width of the non-target columns
                            '        '
                            '        ColumnWidthIncrease = -(ColumnWidthTotal * 0.1)
                            '        For ColumnPointer = 0 To CDef.adminColumns.Count - 1
                            '            FieldName = CDef.adminColumns(ColumnPointer).Name
                            '            fieldId = CDef.fields(FieldName.ToLower()).Id
                            '            If fieldId = TargetFieldID Then
                            '                FieldWidth = CDef.adminColumns(ColumnPointer).Width
                            '                If (FieldWidth + ColumnWidthIncrease) < 10 Then
                            '                    ColumnWidthIncrease = 10 - FieldWidth
                            '                End If
                            '            Else
                            '                ColumnWidthBalance = ColumnWidthBalance + CDef.adminColumns(ColumnPointer).Width
                            '            End If
                            '        Next
                            '        '
                            '        ' Adjust all columns
                            '        '
                            '        If (ColumnWidthBalance > 0) And (ColumnWidthIncrease <> 0) Then
                            '            For ColumnPointer = 0 To CDef.adminColumns.Count - 1
                            '                FieldName = CDef.adminColumns(ColumnPointer).Name
                            '                fieldId = CDef.fields(FieldName.ToLower()).Id
                            '                CSPointer = cpCore.main_OpenCSContentRecord("Content Fields", fieldId)
                            '                If fieldId = TargetFieldID Then
                            '                    '
                            '                    ' Target gets 10% increase
                            '                    '
                            '                    FieldWidth = Int(CDef.adminColumns(ColumnPointer).Width + ColumnWidthIncrease)
                            '                    Call cpCore.app.SetCS(CSPointer, "IndexWidth", FieldWidth)
                            '                Else
                            '                    '
                            '                    ' non-targets get their share of the shrinkage
                            '                    '
                            '                    FieldWidth = CDef.adminColumns(ColumnPointer).Width
                            '                    FieldWidth = FieldWidth - ((ColumnWidthIncrease * FieldWidth) / ColumnWidthBalance)
                            '                    Call cpCore.app.SetCS(CSPointer, "IndexWidth", Int(FieldWidth))
                            '                End If
                            '                Call cpCore.app.SetCS(CSPointer, "IndexColumn", (ColumnPointer) * 10)
                            '                Call cpCore.app.closeCS(CSPointer)
                            '            Next
                            '            ReloadCDef = True
                            '        End If
                            '    End If
                    End Select
                    '
                    ' Get a new copy of the content definition
                    '
                    CDef = cpCore.metaData.getCdef(ContentID, True, False)
                End If
                If (Button = ButtonSaveandInvalidateCache) Then
                    cpCore.cache.invalidateAll()
                    cpCore.metaData.clear()
                    Call cpCore.webServer.redirect("?af=" & AdminFormToolConfigureListing & "&ContentID=" & ContentID)
                End If
                '
                '--------------------------------------------------------------------------------
                '   Display the form
                '--------------------------------------------------------------------------------
                '
                If ContentName <> "" Then
                    Call Stream.Add("<br><br><B>" & ContentName & "</b><br>")
                End If
                Stream.Add("<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""99%""><tr>")
                Stream.Add("<td width=""5%"">&nbsp;</td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>10%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>20%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>30%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>40%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>50%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>60%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>70%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>80%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>90%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>100%</nobr></td>")
                Stream.Add("<td width=""4%"" align=""center"">&nbsp;</td>")
                Stream.Add("</tr></TABLE>")
                '
                Stream.Add("<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""99%""><tr>")
                Stream.Add("<td width=""9%""><nobr><IMG src=""/ccLib/images/black.gif"" width=""1"" height=""10""><IMG alt="""" src=""/ccLib/Images/spacer.gif"" width=""100%"" height=""10""></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><IMG src=""/ccLib/images/black.gif"" width=""1"" height=""10""><IMG alt="""" src=""/ccLib/Images/spacer.gif"" width=""100%"" height=""10""></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><IMG src=""/ccLib/images/black.gif"" width=""1"" height=""10""><IMG alt="""" src=""/ccLib/Images/spacer.gif"" width=""100%"" height=""10""></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><IMG src=""/ccLib/images/black.gif"" width=""1"" height=""10""><IMG alt="""" src=""/ccLib/Images/spacer.gif"" width=""100%"" height=""10""></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><IMG src=""/ccLib/images/black.gif"" width=""1"" height=""10""><IMG alt="""" src=""/ccLib/Images/spacer.gif"" width=""100%"" height=""10""></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><IMG src=""/ccLib/images/black.gif"" width=""1"" height=""10""><IMG alt="""" src=""/ccLib/Images/spacer.gif"" width=""100%"" height=""10""></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><IMG src=""/ccLib/images/black.gif"" width=""1"" height=""10""><IMG alt="""" src=""/ccLib/Images/spacer.gif"" width=""100%"" height=""10""></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><IMG src=""/ccLib/images/black.gif"" width=""1"" height=""10""><IMG alt="""" src=""/ccLib/Images/spacer.gif"" width=""100%"" height=""10""></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><IMG src=""/ccLib/images/black.gif"" width=""1"" height=""10""><IMG alt="""" src=""/ccLib/Images/spacer.gif"" width=""100%"" height=""10""></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><IMG src=""/ccLib/images/black.gif"" width=""1"" height=""10""><IMG alt="""" src=""/ccLib/Images/spacer.gif"" width=""100%"" height=""10""></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><IMG src=""/ccLib/images/black.gif"" width=""1"" height=""10""><IMG alt="""" src=""/ccLib/Images/spacer.gif"" width=""100%"" height=""10""></nobr></td>")
                Stream.Add("</tr></TABLE>")
                '
                ' print the column headers
                '
                ColumnWidthTotal = 0
                If CDef.adminColumns.Count > 0 Then
                    '
                    ' Calc total width
                    '
                    For Each kvp As KeyValuePair(Of String, cdefModel.CDefAdminColumnClass) In CDef.adminColumns
                        ColumnWidthTotal += kvp.Value.Width
                    Next
                    'For ColumnCount = 0 To CDef.adminColumns.Count - 1
                    '    ColumnWidthTotal = ColumnWidthTotal + CDef.adminColumns(ColumnCount).Width
                    'Next
                    If ColumnWidthTotal > 0 Then
                        Stream.Add("<table border=""0"" cellpadding=""5"" cellspacing=""0"" width=""90%"">")
                        Dim ColumnCount As Integer = 0
                        For Each kvp As KeyValuePair(Of String, cdefModel.CDefAdminColumnClass) In CDef.adminColumns
                            '
                            ' print column headers - anchored so they sort columns
                            '
                            ColumnWidth = CInt(100 * (kvp.Value.Width / ColumnWidthTotal))
                            FieldName = kvp.Value.Name
                            With CDef.fields(FieldName.ToLower())
                                fieldId = .id
                                Caption = .caption
                                If .inherited Then
                                    Caption = Caption & "*"
                                    InheritedFieldCount = InheritedFieldCount + 1
                                End If
                                AStart = "<A href=""" & cpCore.webServer.requestPage & "?" & RequestNameToolContentID & "=" & ContentID & "&af=" & AdminFormToolConfigureListing & "&fi=" & fieldId & "&dtcn=" & ColumnCount
                                Call Stream.Add("<td width=""" & ColumnWidth & "%"" valign=""top"" align=""left"">" & SpanClassAdminNormal & Caption & "<br>")
                                Call Stream.Add("<IMG src=""/ccLib/images/black.GIF"" width=""100%"" height=""1"">")
                                Call Stream.Add(AStart & "&dta=" & ToolsActionRemoveField & """><IMG src=""/ccLib/images/LibButtonDeleteUp.gif"" width=""50"" height=""15"" border=""0""></A><br>")
                                Call Stream.Add(AStart & "&dta=" & ToolsActionMoveFieldRight & """><IMG src=""/ccLib/images/LibButtonMoveRightUp.gif"" width=""50"" height=""15"" border=""0""></A><br>")
                                Call Stream.Add(AStart & "&dta=" & ToolsActionMoveFieldLeft & """><IMG src=""/ccLib/images/LibButtonMoveLeftUp.gif"" width=""50"" height=""15"" border=""0""></A><br>")
                                Call Stream.Add(AStart & "&dta=" & ToolsActionSetAZ & """><IMG src=""/ccLib/images/LibButtonSortazUp.gif"" width=""50"" height=""15"" border=""0""></A><br>")
                                Call Stream.Add(AStart & "&dta=" & ToolsActionSetZA & """><IMG src=""/ccLib/images/LibButtonSortzaUp.gif"" width=""50"" height=""15"" border=""0""></A><br>")
                                Call Stream.Add(AStart & "&dta=" & ToolsActionExpand & """><IMG src=""/ccLib/images/LibButtonOpenUp.gif"" width=""50"" height=""15"" border=""0""></A><br>")
                                Call Stream.Add(AStart & "&dta=" & ToolsActionContract & """><IMG src=""/ccLib/images/LibButtonCloseUp.gif"" width=""50"" height=""15"" border=""0""></A>")
                                Call Stream.Add("</SPAN></td>")
                            End With
                            ColumnCount += 1
                        Next
                        Stream.Add("</tr>")
                        Stream.Add("</TABLE>")
                    End If
                End If
                '
                ' ----- If anything was inherited, put up the message
                '
                If InheritedFieldCount > 0 Then
                    Call Stream.Add("<P class=""ccNormal"">* This field was inherited from the Content Definition's Parent. Inherited fields will automatically change when the field in the parent is changed. If you alter these settings, this connection will be broken, and the field will no longer inherit it's properties.</P class=""ccNormal"">")
                End If
                '
                ' ----- now output a list of fields to add
                '
                If CDef.fields.Count = 0 Then
                    Stream.Add(SpanClassAdminNormal & "This Content Definition has no fields</SPAN><br>")
                Else
                    Stream.Add(SpanClassAdminNormal & "<br>")
                    Dim skipField As Boolean
                    For Each keyValuePair As KeyValuePair(Of String, CDefFieldModel) In CDef.fields
                        Dim field As CDefFieldModel = keyValuePair.Value
                        With field
                            '
                            ' test if this column is in use
                            '
                            skipField = False
                            'ColumnPointer = CDef.adminColumns.Count
                            If CDef.adminColumns.Count > 0 Then
                                For Each kvp As KeyValuePair(Of String, cdefModel.CDefAdminColumnClass) In CDef.adminColumns
                                    If .nameLc = kvp.Value.Name Then
                                        skipField = True
                                        Exit For
                                    End If
                                Next
                                'For ColumnPointer = 0 To CDef.adminColumns.Count - 1
                                '    If .nameLc = CDef.adminColumns(ColumnPointer).Name Then
                                '        skipField = True
                                '        Exit For
                                '    End If
                                'Next
                            End If
                            '
                            ' display the column if it is not in use
                            '
                            If skipField Then
                                If False Then
                                    ' this causes more problems then it fixes
                                    'If Not .Authorable Then
                                    '
                                    ' not authorable
                                    '
                                    Stream.Add("<IMG src=""/ccLib/images/Spacer.gif"" width=""50"" height=""15"" border=""0""> " & .caption & " (not authorable field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdFileTextPrivate) Then
                                    '
                                    ' text filename can not be search
                                    '
                                    Stream.Add("<IMG src=""/ccLib/images/Spacer.gif"" width=""50"" height=""15"" border=""0""> " & .caption & " (text file field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdFileCSS) Then
                                    '
                                    ' text filename can not be search
                                    '
                                    Stream.Add("<IMG src=""/ccLib/images/Spacer.gif"" width=""50"" height=""15"" border=""0""> " & .caption & " (css file field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdFileXML) Then
                                    '
                                    ' text filename can not be search
                                    '
                                    Stream.Add("<IMG src=""/ccLib/images/Spacer.gif"" width=""50"" height=""15"" border=""0""> " & .caption & " (xml file field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdFileJavascript) Then
                                    '
                                    ' text filename can not be search
                                    '
                                    Stream.Add("<IMG src=""/ccLib/images/Spacer.gif"" width=""50"" height=""15"" border=""0""> " & .caption & " (javascript file field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdLongText) Then
                                    '
                                    ' long text can not be search
                                    '
                                    Stream.Add("<IMG src=""/ccLib/images/Spacer.gif"" width=""50"" height=""15"" border=""0""> " & .caption & " (long text field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdFileImage) Then
                                    '
                                    ' long text can not be search
                                    '
                                    Stream.Add("<IMG src=""/ccLib/images/Spacer.gif"" width=""50"" height=""15"" border=""0""> " & .caption & " (image field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdRedirect) Then
                                    '
                                    ' long text can not be search
                                    '
                                    Stream.Add("<IMG src=""/ccLib/images/Spacer.gif"" width=""50"" height=""15"" border=""0""> " & .caption & " (redirect field)<br>")
                                Else
                                    '
                                    ' can be used as column header
                                    '
                                    Stream.Add("<A href=""" & cpCore.webServer.requestPage & "?" & RequestNameToolContentID & "=" & ContentID & "&af=" & AdminFormToolConfigureListing & "&fi=" & .id & "&dta=" & ToolsActionAddField & "&" & RequestNameAddFieldID & "=" & .id & """><IMG src=""/ccLib/images/LibButtonAddUp.gif"" width=""50"" height=""15"" border=""0""></A> " & .caption & "<br>")
                                End If
                            End If
                        End With
                    Next
                End If
            End If
            '
            '--------------------------------------------------------------------------------
            ' print the content tables that have Listing Pages to Configure
            '--------------------------------------------------------------------------------
            '
            FormPanel = FormPanel & SpanClassAdminNormal & "Select a Content Definition to Configure its Listing Page<br>"
            'FormPanel = FormPanel & cpCore.main_GetFormInputHidden("af", AdminFormToolConfigureListing)
            FormPanel = FormPanel & cpCore.html.main_GetFormInputSelect("ContentID", ContentID, "Content")
            Call Stream.Add(cpCore.html.main_GetPanel(FormPanel))
            '
            Call cpCore.siteProperties.setProperty("AllowContentAutoLoad", AllowContentAutoLoad)
            Stream.Add(cpCore.html.html_GetFormInputHidden("ReloadCDef", ReloadCDef))

            GetForm_ConfigureListing = genericLegacyView.OpenFormTable(cpCore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpCore, ButtonList)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_ConfigureListing", "ErrorTrap")
        End Function
        '
        '=============================================================================
        ' checks Content against Database tables
        '=============================================================================
        '
        Private Function GetForm_ContentDiagnostic() As String
            On Error GoTo ErrorTrap
            '
            Dim SQL As String
            Dim DataSourceID As Integer
            Dim DataSourceName As String
            Dim TableName As String
            Dim ContentName As String
            Dim RecordID As Integer
            Dim CSContent As Integer
            Dim CSDataSources As Integer
            Dim StartButton As String
            Dim Name As String
            'Dim ConnectionObject As Connection
            Dim ConnectionString As String
            Dim CSPointer As Integer
            Dim ErrorCount As Integer
            Dim FieldName As String
            Dim bitBucket As String
            Dim ContentID As Integer
            Dim ParentID As Integer
            '
            Dim CSFields As Integer
            Dim CSTestRecord As Integer
            Dim TestRecordID As Integer
            Dim fieldType As Integer
            Dim RedirectContentID As Integer
            Dim LookupContentID As Integer
            Dim LinkPage As String
            Dim LookupList As String
            '
            Dim DiagProblem As String
            '
            Dim iDiagActionCount As Integer
            Dim DiagActionPointer As Integer
            Dim DiagAction As String
            Dim DiagActions() As DiagActionType
            Dim fieldId As Integer
            Dim CS As Integer
            Dim CSTest As Integer
            Dim Button As String
            Dim FieldRequired As Boolean
            Dim FieldAuthorable As Boolean
            '
            Dim PathFilename As String
            Dim SampleStyles As String
            Dim SampleLine As String
            Dim Copy As String
            Dim NextWhiteSpace As Integer
            Dim SampleName As String
            Dim LinePosition As Integer
            Dim Stream As New stringBuilderLegacyController
            Dim ButtonList As String
            '
            Const ButtonFix = "Make Corrections"
            Const ButtonFixAndRun = "Make Corrections and Run Diagnostic"
            Const ButtonRun = "Run Diagnostic"
            '
            ButtonList = ButtonRun
            '
            Call Stream.Add(GetTitle("Content Diagnostic", "This tool finds Content and Table problems. To run successfully, the Site Property 'TrapErrors' must be set to true."))
            Call Stream.Add(SpanClassAdminNormal & "<br>")
            '
            iDiagActionCount = cpCore.docProperties.getInteger("DiagActionCount")
            Button = cpCore.docProperties.getText("Button")
            If (iDiagActionCount <> 0) And ((Button = ButtonFix) Or (Button = ButtonFixAndRun)) Then
                '
                '-----------------------------------------------------------------------------------------------
                ' ----- Perform actions from previous Diagnostic
                '-----------------------------------------------------------------------------------------------
                '
                Stream.Add("<br>")
                For DiagActionPointer = 0 To iDiagActionCount
                    DiagAction = cpCore.docProperties.getText("DiagAction" & DiagActionPointer)
                    Stream.Add("Perform Action " & DiagActionPointer & " - " & DiagAction & "<br>")
                    Select Case genericController.EncodeInteger(DiagArgument(DiagAction, 0))
                        Case DiagActionSetFieldType
                            '
                            ' ----- Set Field Type
                            '
                            ContentID = Local_GetContentID(DiagArgument(DiagAction, 1))
                            CS = cpCore.db.cs_open("Content Fields", "(ContentID=" & ContentID & ")and(Name=" & cpCore.db.encodeSQLText(DiagArgument(DiagAction, 2)) & ")")
                            If cpCore.db.cs_ok(CS) Then
                                Call cpCore.db.cs_set(CS, "Type", DiagArgument(DiagAction, 3))
                            End If
                            Call cpCore.db.cs_Close(CS)
                            'end case
                        Case DiagActionSetFieldInactive
                            '
                            ' ----- Set Field Inactive
                            '
                            ContentID = Local_GetContentID(DiagArgument(DiagAction, 1))
                            CS = cpCore.db.cs_open("Content Fields", "(ContentID=" & ContentID & ")and(Name=" & cpCore.db.encodeSQLText(DiagArgument(DiagAction, 2)) & ")")
                            If cpCore.db.cs_ok(CS) Then
                                Call cpCore.db.cs_set(CS, "active", 0)
                            End If
                            Call cpCore.db.cs_Close(CS)
                            'end case
                        Case DiagActionDeleteRecord
                            '
                            ' ----- Delete Record
                            '
                            ContentName = DiagArgument(DiagAction, 1)
                            RecordID = genericController.EncodeInteger(DiagArgument(DiagAction, 2))
                            Call cpCore.db.deleteContentRecord(ContentName, RecordID)
                            'end case
                        Case DiagActionContentDeDupe
                            ContentName = DiagArgument(DiagAction, 1)
                            CS = cpCore.db.cs_open("Content", "name=" & cpCore.db.encodeSQLText(ContentName), "ID")
                            If cpCore.db.cs_ok(CS) Then
                                Call cpCore.db.cs_goNext(CS)
                                Do While cpCore.db.cs_ok(CS)
                                    Call cpCore.db.cs_set(CS, "active", 0)
                                    Call cpCore.db.cs_goNext(CS)
                                Loop
                            End If
                            Call cpCore.db.cs_Close(CS)
                            'end case
                        Case DiagActionSetRecordInactive
                            '
                            ' ----- Set Field Inactive
                            '
                            ContentName = DiagArgument(DiagAction, 1)
                            RecordID = genericController.EncodeInteger(DiagArgument(DiagAction, 2))
                            CS = cpCore.db.cs_open(ContentName, "(ID=" & RecordID & ")")
                            If cpCore.db.cs_ok(CS) Then
                                Call cpCore.db.cs_set(CS, "active", 0)
                            End If
                            Call cpCore.db.cs_Close(CS)
                            'end case
                        Case DiagActionSetFieldNotRequired
                            '
                            ' ----- Set Field not-required
                            '
                            ContentName = DiagArgument(DiagAction, 1)
                            RecordID = genericController.EncodeInteger(DiagArgument(DiagAction, 2))
                            CS = cpCore.db.cs_open(ContentName, "(ID=" & RecordID & ")")
                            If cpCore.db.cs_ok(CS) Then
                                Call cpCore.db.cs_set(CS, "required", 0)
                            End If
                            Call cpCore.db.cs_Close(CS)
                            'end case
                    End Select
                Next
            End If
            If ((Button = ButtonRun) Or (Button = ButtonFixAndRun)) Then
                '
                ' Process input
                '
                If Not cpCore.siteProperties.trapErrors Then
                    '
                    ' TrapErrors must be true to run this tools
                    '
                    Call Stream.Add("Site Property 'TrapErrors' is currently set false. This property must be true to run Content Diagnostics successfully.<br>")
                Else
                    Call cpCore.html.enableOutputBuffer(False)
                    '
                    ' ----- check Content Sources for duplicates
                    '
                    If (DiagActionCount < DiagActionCountMax) Then
                        Stream.Add(GetDiagHeader("Checking Content Definition Duplicates...<br>"))
                        '
                        SQL = "SELECT Count(ccContent.ID) AS RecordCount, ccContent.Name AS Name, ccContent.Active" _
                                & " From ccContent" _
                                & " GROUP BY ccContent.Name, ccContent.Active" _
                                & " Having (((Count(ccContent.ID)) > 1) And ((ccContent.active) <> 0))" _
                                & " ORDER BY Count(ccContent.ID) DESC;"
                        CSPointer = cpCore.db.cs_openCsSql_rev("Default", SQL)
                        If cpCore.db.cs_ok(CSPointer) Then
                            Do While cpCore.db.cs_ok(CSPointer)
                                DiagProblem = "PROBLEM: There are " & cpCore.db.cs_getText(CSPointer, "RecordCount") & " records in the Content table with the name [" & cpCore.db.cs_getText(CSPointer, "Name") & "]"
                                ReDim DiagActions(2)
                                DiagActions(0).Name = "Ignore, or handle this issue manually"
                                DiagActions(0).Command = ""
                                DiagActions(1).Name = "Mark all duplicate definitions inactive"
                                DiagActions(1).Command = CStr(DiagActionContentDeDupe) & "," & cpCore.db.cs_getField(CSPointer, "name")
                                Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                Call cpCore.db.cs_goNext(CSPointer)
                            Loop
                        End If
                        Call cpCore.db.cs_Close(CSPointer)
                    End If
                    '
                    ' ----- Content Fields
                    '
                    If (DiagActionCount < DiagActionCountMax) Then
                        Stream.Add(GetDiagHeader("Checking Content Fields...<br>"))
                        '
                        SQL = "SELECT ccFields.required AS FieldRequired, ccFields.Authorable AS FieldAuthorable, ccFields.Type AS FieldType, ccFields.Name AS FieldName, ccContent.ID AS ContentID, ccContent.Name AS ContentName, ccTables.Name AS TableName, ccDataSources.Name AS DataSourceName" _
                                & " FROM (ccFields LEFT JOIN ccContent ON ccFields.ContentID = ccContent.ID) LEFT JOIN (ccTables LEFT JOIN ccDataSources ON ccTables.DataSourceID = ccDataSources.ID) ON ccContent.ContentTableID = ccTables.ID" _
                                & " WHERE (((ccFields.Active)<>0) AND ((ccContent.Active)<>0) AND ((ccTables.Active)<>0)) OR (((ccFields.Active)<>0) AND ((ccContent.Active)<>0) AND ((ccTables.Active)<>0));"
                        CS = cpCore.db.cs_openCsSql_rev("Default", SQL)
                        If Not cpCore.db.cs_ok(CS) Then
                            DiagProblem = "PROBLEM: No Content entries were found in the content table."
                            ReDim DiagActions(1)
                            DiagActions(0).Name = "Ignore, or handle this issue manually"
                            DiagActions(0).Command = ""
                            Stream.Add(GetDiagError(DiagProblem, DiagActions))
                        Else
                            Do While cpCore.db.cs_ok(CS) And (DiagActionCount < DiagActionCountMax)
                                FieldName = cpCore.db.cs_getText(CS, "FieldName")
                                fieldType = cpCore.db.cs_getInteger(CS, "FieldType")
                                FieldRequired = cpCore.db.cs_getBoolean(CS, "FieldRequired")
                                FieldAuthorable = cpCore.db.cs_getBoolean(CS, "FieldAuthorable")
                                ContentName = cpCore.db.cs_getText(CS, "ContentName")
                                TableName = cpCore.db.cs_getText(CS, "TableName")
                                DataSourceName = cpCore.db.cs_getText(CS, "DataSourceName")
                                If DataSourceName = "" Then
                                    DataSourceName = "Default"
                                End If
                                If FieldRequired And (Not FieldAuthorable) Then
                                    DiagProblem = "PROBLEM: Field [" & FieldName & "] in Content Definition [" & ContentName & "] is required, but is not referenced on the Admin Editing page. This will prevent content definition records from being saved."
                                    ReDim DiagActions(1)
                                    DiagActions(0).Name = "Ignore, or handle this issue manually"
                                    DiagActions(0).Command = ""
                                    DiagActions(1).Name = "Set this Field inactive"
                                    DiagActions(1).Command = CStr(DiagActionSetFieldInactive) & "," & ContentName & "," & FieldName
                                    DiagActions(1).Name = "Set this Field not required"
                                    DiagActions(1).Command = CStr(DiagActionSetFieldNotRequired) & "," & ContentName & "," & FieldName
                                    Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                End If
                                If (FieldName <> "") And (fieldType <> FieldTypeIdRedirect) And (fieldType <> FieldTypeIdManyToMany) Then
                                    SQL = "SELECT " & FieldName & " FROM " & TableName & " WHERE ID=0;"
                                    CSTest = cpCore.db.cs_openCsSql_rev(DataSourceName, SQL)
                                    If CSTest = -1 Then
                                        DiagProblem = "PROBLEM: Field [" & FieldName & "] in Content Definition [" & ContentName & "] could not be read from database table [" & TableName & "] on datasource [" & DataSourceName & "]."
                                        ReDim DiagActions(1)
                                        DiagActions(0).Name = "Ignore, or handle this issue manually"
                                        DiagActions(0).Command = ""
                                        DiagActions(1).Name = "Set this Content Definition Field inactive"
                                        DiagActions(1).Command = CStr(DiagActionSetFieldInactive) & "," & ContentName & "," & FieldName
                                        Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                    End If
                                End If
                                Call cpCore.db.cs_Close(CSTest)
                                cpCore.db.cs_goNext(CS)
                            Loop
                        End If
                        Call cpCore.db.cs_Close(CS)
                    End If
                    '
                    ' ----- Insert Content Testing
                    '
                    If (DiagActionCount < DiagActionCountMax) Then
                        Stream.Add(GetDiagHeader("Checking Content Insertion...<br>"))
                        '
                        CSContent = cpCore.db.cs_open("Content")
                        If Not cpCore.db.cs_ok(CSContent) Then
                            DiagProblem = "PROBLEM: No Content entries were found in the content table."
                            ReDim DiagActions(1)
                            DiagActions(0).Name = "Ignore, or handle this issue manually"
                            DiagActions(0).Command = ""
                            Stream.Add(GetDiagError(DiagProblem, DiagActions))
                        Else
                            Do While cpCore.db.cs_ok(CSContent) And (DiagActionCount < DiagActionCountMax)
                                ContentID = cpCore.db.cs_getInteger(CSContent, "ID")
                                ContentName = cpCore.db.cs_getText(CSContent, "name")
                                CSTestRecord = cpCore.db.cs_insertRecord(ContentName)
                                If Not cpCore.db.cs_ok(CSTestRecord) Then
                                    DiagProblem = "PROBLEM: Could not insert a record using Content Definition [" & ContentName & "]"
                                    ReDim DiagActions(1)
                                    DiagActions(0).Name = "Ignore, or handle this issue manually"
                                    DiagActions(0).Command = ""
                                    Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                Else
                                    TestRecordID = cpCore.db.cs_getInteger(CSTestRecord, "id")
                                    If TestRecordID = 0 Then
                                        DiagProblem = "PROBLEM: Content Definition [" & ContentName & "] does not support the required field [ID]"""
                                        ReDim DiagActions(1)
                                        DiagActions(0).Name = "Ignore, or handle this issue manually"
                                        DiagActions(0).Command = ""
                                        DiagActions(1).Name = "Set this Content Definition inactive"
                                        DiagActions(1).Command = CStr(DiagActionSetRecordInactive) & ",Content," & ContentID
                                        Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                    Else
                                        CSFields = cpCore.db.cs_open("Content Fields", "ContentID=" & ContentID)
                                        Do While cpCore.db.cs_ok(CSFields)
                                            '
                                            ' ----- read the value of the field to test its presents
                                            '
                                            FieldName = cpCore.db.cs_getText(CSFields, "name")
                                            fieldType = cpCore.db.cs_getInteger(CSFields, "Type")
                                            Select Case fieldType
                                                Case FieldTypeIdManyToMany
                                                    '
                                                    '   skip it
                                                    '
                                                Case FieldTypeIdRedirect
                                                    '
                                                    ' ----- redirect type, check redirect contentid
                                                    '
                                                    RedirectContentID = cpCore.db.cs_getInteger(CSFields, "RedirectContentID")
                                                    ErrorCount = cpCore.app_errorCount
                                                    bitBucket = Local_GetContentNameByID(RedirectContentID)
                                                    If IsNull(bitBucket) Or (ErrorCount <> cpCore.app_errorCount) Then
                                                        DiagProblem = "PROBLEM: Content Field [" & ContentName & "].[" & FieldName & "] is a Redirection type, but the ContentID [" & RedirectContentID & "] is not valid."
                                                        If FieldName = "" Then
                                                            DiagProblem = DiagProblem & " Also, the field has no name attribute so these diagnostics can not automatically mark the field inactive."
                                                            ReDim DiagActions(1)
                                                            DiagActions(0).Name = "Ignore, or handle this issue manually"
                                                            DiagActions(0).Command = ""
                                                            Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                                        Else
                                                            ReDim DiagActions(2)
                                                            DiagActions(0).Name = "Ignore, or handle this issue manually"
                                                            DiagActions(0).Command = ""
                                                            DiagActions(1).Name = "Set this Content Definition Field inactive"
                                                            DiagActions(1).Command = CStr(DiagActionSetFieldInactive) & "," & ContentName & "," & FieldName
                                                            Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                                        End If
                                                    End If
                                                Case FieldTypeIdLookup
                                                    '
                                                    ' ----- lookup type, read value and check lookup contentid
                                                    '
                                                    ErrorCount = cpCore.app_errorCount
                                                    bitBucket = cpCore.db.cs_getField(CSTestRecord, FieldName)
                                                    If ErrorCount <> cpCore.app_errorCount Then
                                                        DiagProblem = "PROBLEM: An error occurred reading the value of Content Field [" & ContentName & "].[" & FieldName & "]"
                                                        ReDim DiagActions(1)
                                                        DiagActions(0).Name = "Ignore, or handle this issue manually"
                                                        DiagActions(0).Command = ""
                                                        Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                                    Else
                                                        bitBucket = ""
                                                        LookupList = cpCore.db.cs_getText(CSFields, "Lookuplist")
                                                        LookupContentID = cpCore.db.cs_getInteger(CSFields, "LookupContentID")
                                                        If LookupContentID <> 0 Then
                                                            ErrorCount = cpCore.app_errorCount
                                                            bitBucket = Local_GetContentNameByID(LookupContentID)
                                                        End If
                                                        If (LookupList = "") And ((LookupContentID = 0) Or (bitBucket = "") Or (ErrorCount <> cpCore.app_errorCount)) Then
                                                            DiagProblem = "Content Field [" & ContentName & "].[" & FieldName & "] is a Lookup type, but LookupList is blank and LookupContentID [" & LookupContentID & "] is not valid."
                                                            ReDim DiagActions(2)
                                                            DiagActions(0).Name = "Ignore, or handle this issue manually"
                                                            DiagActions(0).Command = ""
                                                            DiagActions(1).Name = "Convert the field to an Integer so no lookup is provided."
                                                            DiagActions(1).Command = CStr(DiagActionSetFieldType) & "," & ContentName & "," & FieldName & "," & CStr(FieldTypeIdInteger)
                                                            Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                                        End If
                                                    End If
                                                Case Else
                                                    '
                                                    ' ----- check for value in database
                                                    '
                                                    ErrorCount = cpCore.app_errorCount
                                                    bitBucket = cpCore.db.cs_getField(CSTestRecord, FieldName)
                                                    If (ErrorCount <> cpCore.app_errorCount) Then
                                                        DiagProblem = "PROBLEM: An error occurred reading the value of Content Field [" & ContentName & "].[" & FieldName & "]"
                                                        ReDim DiagActions(3)
                                                        DiagActions(0).Name = "Ignore, or handle this issue manually"
                                                        DiagActions(0).Command = ""
                                                        DiagActions(1).Name = "Add this field into the Content Definitions Content (and Authoring) Table."
                                                        DiagActions(1).Command = "x"
                                                        Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                                    End If
                                            End Select
                                            cpCore.db.cs_goNext(CSFields)
                                        Loop
                                    End If
                                    Call cpCore.db.cs_Close(CSFields)
                                    Call cpCore.db.cs_Close(CSTestRecord)
                                    Call cpCore.db.deleteContentRecord(ContentName, TestRecordID)
                                End If
                                cpCore.db.cs_goNext(CSContent)
                            Loop
                        End If
                        Call cpCore.db.cs_Close(CSContent)
                    End If
                    '
                    ' ----- Check menu entries
                    '
                    If (DiagActionCount < DiagActionCountMax) Then
                        Stream.Add(GetDiagHeader("Checking Menu Entries...<br>"))
                        CSPointer = cpCore.db.cs_open("Menu Entries")
                        If Not cpCore.db.cs_ok(CSPointer) Then
                            DiagProblem = "PROBLEM: Could not open the [Menu Entries] content."
                            ReDim DiagActions(3)
                            DiagActions(0).Name = "Ignore, or handle this issue manually"
                            DiagActions(0).Command = ""
                            Stream.Add(GetDiagError(DiagProblem, DiagActions))
                        Else
                            Do While cpCore.db.cs_ok(CSPointer) And (DiagActionCount < DiagActionCountMax)
                                ContentID = cpCore.db.cs_getInteger(CSPointer, "ContentID")
                                If ContentID <> 0 Then
                                    CSContent = cpCore.db.cs_open("Content", "ID=" & ContentID)
                                    If Not cpCore.db.cs_ok(CSContent) Then
                                        DiagProblem = "PROBLEM: Menu Entry [" & cpCore.db.cs_getText(CSPointer, "name") & "] points to an invalid Content Definition."
                                        ReDim DiagActions(3)
                                        DiagActions(0).Name = "Ignore, or handle this issue manually"
                                        DiagActions(0).Command = ""
                                        DiagActions(1).Name = "Remove this menu entry"
                                        DiagActions(1).Command = CStr(DiagActionDeleteRecord) & ",Menu Entries," & cpCore.db.cs_getInteger(CSPointer, "ID")
                                        Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                    End If
                                    Call cpCore.db.cs_Close(CSContent)
                                End If
                                cpCore.db.cs_goNext(CSPointer)
                            Loop
                        End If
                        Call cpCore.db.cs_Close(CSPointer)
                    End If
                    If (DiagActionCount >= DiagActionCountMax) Then
                        DiagProblem = "Diagnostic Problem Limit (" & DiagActionCountMax & ") has been reached. Resolve the above issues to see more."
                        ReDim DiagActions(1)
                        DiagActions(0).Name = ""
                        DiagActions(0).Command = ""
                        Stream.Add(GetDiagError(DiagProblem, DiagActions))
                    End If
                    '
                    ' ----- Done with diagnostics
                    '
                    Stream.Add(cpCore.html.html_GetFormInputHidden("DiagActionCount", DiagActionCount))
                End If
            End If
            '
            ' start diagnostic button
            '
            Call Stream.Add("</SPAN>")
            If DiagActionCount > 0 Then
                ButtonList = ButtonList & "," & ButtonFix
                ButtonList = ButtonList & "," & ButtonFixAndRun
            End If
            '
            ' ----- end form
            '
            'Call Stream.Add(cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolContentDiagnostic))
            '
            GetForm_ContentDiagnostic = genericLegacyView.OpenFormTable(cpCore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpCore, ButtonList)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_ContentDiagnostic", "ErrorTrap")
        End Function
        '
        '=============================================================================
        ' Normalize the Index page Columns, setting proper values for IndexColumn, etc.
        '=============================================================================
        '
        Private Sub NormalizeIndexColumns(ByVal ContentID As Integer)
            On Error GoTo ErrorTrap
            '
            Dim CSPointer As Integer
            Dim ColumnWidth As Integer
            Dim ColumnWidthTotal As Integer
            Dim ColumnCounter As Integer
            Dim IndexColumn As Integer
            '
            'cpCore.main_'TestPointEnter ("NormalizeIndexColumns()")
            '
            'Call LoadContentDefinitions
            '
            CSPointer = cpCore.db.cs_open("Content Fields", "(ContentID=" & ContentID & ")", "IndexColumn")
            If Not cpCore.db.cs_ok(CSPointer) Then
                Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors2("NormalizeIndexColumns", "Could not read Content Field Definitions")
            Else
                '
                ' Adjust IndexSortOrder to be 0 based, count by 1
                '
                ColumnCounter = 0
                Do While cpCore.db.cs_ok(CSPointer)
                    IndexColumn = cpCore.db.cs_getInteger(CSPointer, "IndexColumn")
                    ColumnWidth = cpCore.db.cs_getInteger(CSPointer, "IndexWidth")
                    If (IndexColumn = 0) Or (ColumnWidth = 0) Then
                        Call cpCore.db.cs_set(CSPointer, "IndexColumn", 0)
                        Call cpCore.db.cs_set(CSPointer, "IndexWidth", 0)
                        Call cpCore.db.cs_set(CSPointer, "IndexSortPriority", 0)
                    Else
                        '
                        ' Column appears in Index, clean it up
                        '
                        Call cpCore.db.cs_set(CSPointer, "IndexColumn", ColumnCounter)
                        ColumnCounter = ColumnCounter + 1
                        ColumnWidthTotal = ColumnWidthTotal + ColumnWidth
                    End If
                    cpCore.db.cs_goNext(CSPointer)
                Loop
                If ColumnCounter = 0 Then
                    '
                    ' No columns found, set name as Column 0, active as column 1
                    '
                    Call cpCore.db.cs_goFirst(CSPointer)
                    Do While cpCore.db.cs_ok(CSPointer)
                        Select Case genericController.vbUCase(cpCore.db.cs_getText(CSPointer, "name"))
                            Case "ACTIVE"
                                Call cpCore.db.cs_set(CSPointer, "IndexColumn", 0)
                                Call cpCore.db.cs_set(CSPointer, "IndexWidth", 20)
                                ColumnWidthTotal = ColumnWidthTotal + 20
                            Case "NAME"
                                Call cpCore.db.cs_set(CSPointer, "IndexColumn", 1)
                                Call cpCore.db.cs_set(CSPointer, "IndexWidth", 80)
                                ColumnWidthTotal = ColumnWidthTotal + 80
                        End Select
                        Call cpCore.db.cs_goNext(CSPointer)
                    Loop
                End If
                '
                ' ----- Now go back and set a normalized Width value
                '
                If ColumnWidthTotal > 0 Then
                    Call cpCore.db.cs_goFirst(CSPointer)
                    Do While cpCore.db.cs_ok(CSPointer)
                        ColumnWidth = cpCore.db.cs_getInteger(CSPointer, "IndexWidth")
                        ColumnWidth = CInt((ColumnWidth * 100) / CDbl(ColumnWidthTotal))
                        Call cpCore.db.cs_set(CSPointer, "IndexWidth", ColumnWidth)
                        cpCore.db.cs_goNext(CSPointer)
                    Loop
                End If
            End If
            Call cpCore.db.cs_Close(CSPointer)
            '
            ' ----- now fixup Sort Priority so only visible fields are sorted.
            '
            CSPointer = cpCore.db.cs_open("Content Fields", "(ContentID=" & ContentID & ")", "IndexSortPriority, IndexColumn")
            If Not cpCore.db.cs_ok(CSPointer) Then
                Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors2("NormalizeIndexColumns", "Error reading Content Field Definitions")
            Else
                '
                ' Go through all fields, clear Sort Priority if it does not appear
                '
                Dim SortValue As Integer
                Dim SortDirection As Integer
                SortValue = 0
                Do While cpCore.db.cs_ok(CSPointer)
                    SortDirection = 0
                    If (cpCore.db.cs_getInteger(CSPointer, "IndexColumn") = 0) Then
                        Call cpCore.db.cs_set(CSPointer, "IndexSortPriority", 0)
                    Else
                        Call cpCore.db.cs_set(CSPointer, "IndexSortPriority", SortValue)
                        SortDirection = cpCore.db.cs_getInteger(CSPointer, "IndexSortDirection")
                        If (SortDirection = 0) Then
                            SortDirection = 1
                        Else
                            If SortDirection > 0 Then
                                SortDirection = 1
                            Else
                                SortDirection = -1
                            End If
                        End If
                        SortValue = SortValue + 1
                    End If
                    Call cpCore.db.cs_set(CSPointer, "IndexSortDirection", SortDirection)
                    Call cpCore.db.cs_goNext(CSPointer)
                Loop
            End If
            '
            'cpCore.main_'TestPointExit
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("NormalizeIndexColumns", "ErrorTrap")
        End Sub
        '
        '=============================================================================
        ' Create a child content
        '=============================================================================
        '
        Private Function GetForm_CreateChildContent() As String
            On Error GoTo ErrorTrap
            '
            Dim ParentContentID As Integer
            Dim ParentContentName As String
            Dim ChildContentName As String = ""
            Dim AddAdminMenuEntry As Boolean
            Dim CS As Integer
            Dim MenuName As String = ""
            Dim AdminOnly As Boolean
            Dim DeveloperOnly As Boolean
            Dim Stream As New stringBuilderLegacyController
            Dim ButtonList As String
            '
            ButtonList = ButtonCancel & "," & ButtonRun
            '
            Stream.Add(GetTitle("Create a Child Content from a Content Definition", "This tool creates a Content Definition based on another Content Definition."))
            '
            '   print out the submit form
            '
            If (cpCore.docProperties.getText("Button") <> "") Then
                '
                ' Process input
                '
                ParentContentID = cpCore.docProperties.getInteger("ParentContentID")
                ParentContentName = Local_GetContentNameByID(ParentContentID)
                ChildContentName = cpCore.docProperties.getText("ChildContentName")
                AddAdminMenuEntry = cpCore.docProperties.getBoolean("AddAdminMenuEntry")
                '
                Call Stream.Add(SpanClassAdminSmall)
                If (ParentContentName = "") Or (ChildContentName = "") Then
                    Stream.Add("<p>You must select a parent and provide a child name.</p>")
                Else
                    '
                    ' Create Definition
                    '
                    Stream.Add("<P>Creating content [" & ChildContentName & "] from [" & ParentContentName & "]")
                    Call cpCore.metaData.createContentChild(ChildContentName, ParentContentName, cpCore.authContext.user.id)
                    '
                    Stream.Add("<br>Reloading Content Definitions...")
                    cpCore.cache.invalidateAll()
                    cpCore.metaData.clear()
                    '
                    ' Add Admin Menu Entry
                    '
                    If AddAdminMenuEntry Then
                        Stream.Add("<br>Adding menu entry (will not display until the next page)...")
                        CS = cpCore.db.cs_open("Menu Entries", "ContentID=" & ParentContentID)
                        If cpCore.db.cs_ok(CS) Then
                            MenuName = cpCore.db.cs_getText(CS, "name")
                            AdminOnly = cpCore.db.cs_getBoolean(CS, "AdminOnly")
                            DeveloperOnly = cpCore.db.cs_getBoolean(CS, "DeveloperOnly")
                        End If
                        Call cpCore.db.cs_Close(CS)
                        If MenuName <> "" Then
                            Call Controllers.appBuilderController.admin_VerifyAdminMenu(cpCore, MenuName, ChildContentName, ChildContentName, "", ChildContentName, AdminOnly, DeveloperOnly, False)
                        Else
                            Call Controllers.appBuilderController.admin_VerifyAdminMenu(cpCore, "Site Content", ChildContentName, ChildContentName, "", "")
                        End If
                    End If
                    Stream.Add("<br>Finished</P>")
                End If
                Call Stream.Add("</SPAN>")
            End If
            Call Stream.Add(SpanClassAdminNormal)
            '
            Call Stream.Add("Parent Content Name<br>")
            Stream.Add(cpCore.html.main_GetFormInputSelect("ParentContentID", ParentContentID, "Content", ""))
            Call Stream.Add("<br><br>")
            '
            Call Stream.Add("Child Content Name<br>")
            Stream.Add(cpCore.html.html_GetFormInputText2("ChildContentName", ChildContentName, 1, 40))
            Call Stream.Add("<br><br>")
            '
            Call Stream.Add("Add Admin Menu Entry under Parent's Menu Entry<br>")
            Stream.Add(cpCore.html.html_GetFormInputCheckBox2("AddAdminMenuEntry", AddAdminMenuEntry))
            Call Stream.Add("<br><br>")
            '
            ''Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolCreateChildContent)
            Call Stream.Add("</SPAN>")
            '
            GetForm_CreateChildContent = genericLegacyView.OpenFormTable(cpCore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpCore, ButtonList)

            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_CreateChildContent", "ErrorTrap")
        End Function
        '
        '=============================================================================
        ' checks Content against Database tables
        '=============================================================================
        '
        Private Function GetForm_ClearContentWatchLinks() As String
            On Error GoTo ErrorTrap
            '
            Dim SQL As String
            Dim Stream As New stringBuilderLegacyController
            Dim ButtonList As String
            '
            ButtonList = ButtonCancel & ",Clear Content Watch Links"
            Call Stream.Add(SpanClassAdminNormal)
            Stream.Add(GetTitle("Clear ContentWatch Links", "This tools nulls the Links field of all Content Watch records. After running this tool, run the diagnostic spider to repopulate the links."))
            '
            If (cpCore.docProperties.getText("Button") <> "") Then
                '
                ' Process input
                '
                Call Stream.Add("<br>")
                Call Stream.Add("<br>Clearing Content Watch Link field...")
                Call cpCore.db.executeSql("update ccContentWatch set Link=null;")
                Call Stream.Add("<br>Content Watch Link field cleared.")
            End If
            '
            ' ----- end form
            '
            ''Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolClearContentWatchLink)
            Call Stream.Add("</span>")
            '
            GetForm_ClearContentWatchLinks = genericLegacyView.OpenFormTable(cpCore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpCore, ButtonList)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_ClearContentWatchLinks", "ErrorTrap")
        End Function
        '
        '=============================================================================
        ''' <summary>
        ''' Go through all Content Definitions and create appropriate tables and fields.
        ''' </summary>
        ''' <returns></returns>
        Private Function GetForm_SyncTables() As String
            Dim returnValue As String = ""
            Try
                Dim CSContent As Integer
                Dim CD As cdefModel
                Dim Stream As New stringBuilderLegacyController
                Dim ContentNameArray As String(,)
                Dim ContentNameCount As Integer
                Dim TableName As String
                Dim ButtonList As String
                '
                ButtonList = ButtonCancel & "," & ButtonRun
                '
                Stream.Add(GetTitle("Synchronize Tables to Content Definitions", "This tools goes through all Content Definitions and creates any necessary Tables and Table Fields to support the Definition."))
                '
                If (cpCore.docProperties.getText("Button") <> "") Then
                    '
                    '   Run Tools
                    '
                    Call Stream.Add("Synchronizing Tables to Content Definitions<br>")
                    CSContent = cpCore.db.cs_open("Content", , , , , ,, "id")
                    If cpCore.db.cs_ok(CSContent) Then
                        Do
                            CD = cpCore.metaData.getCdef(cpCore.db.cs_getInteger(CSContent, "id"))
                            TableName = CD.ContentTableName
                            Call Stream.Add("Synchronizing Content " & CD.Name & " to table " & TableName & "<br>")
                            Call cpCore.db.createSQLTable(CD.ContentDataSourceName, TableName)
                            If CD.fields.Count > 0 Then
                                For Each keyValuePair In CD.fields
                                    Dim field As CDefFieldModel
                                    field = keyValuePair.Value
                                    Call Stream.Add("...Field " & field.nameLc & "<br>")
                                    Call cpCore.db.createSQLTableField(CD.ContentDataSourceName, TableName, field.nameLc, field.fieldTypeId)
                                Next
                            End If
                            cpCore.db.cs_goNext(CSContent)
                        Loop While cpCore.db.cs_ok(CSContent)
                        ContentNameArray = cpCore.db.cs_getRows(CSContent)
                        ContentNameCount = UBound(ContentNameArray, 2) + 1
                    End If
                    Call cpCore.db.cs_Close(CSContent)
                End If
                '
                returnValue = genericLegacyView.OpenFormTable(cpCore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpCore, ButtonList)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnValue
        End Function
        '
        '
        '
        Private Function AddDiagError(ByVal ProblemMsg As String, ByVal DiagActions() As DiagActionType) As String
            AddDiagError = GetDiagError(ProblemMsg, DiagActions)
        End Function
        '
        '
        '
        Private Function GetDiagError(ByVal ProblemMsg As String, ByVal DiagActions() As DiagActionType) As String
            On Error GoTo ErrorTrap
            '
            Dim MethodName As String
            Dim ActionCount As Integer
            Dim ActionPointer As Integer
            Dim Caption As String
            Dim Panel As String = ""
            '
            MethodName = "GetDiagError"
            '
            Panel = Panel & "<table border=""0"" cellpadding=""2"" cellspacing=""0"" width=""100%"">"
            Panel = Panel & "<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>" & ProblemMsg & "</b></SPAN></td></tr>"
            ActionCount = UBound(DiagActions)
            If ActionCount > 0 Then
                For ActionPointer = 0 To ActionCount
                    Caption = DiagActions(ActionPointer).Name
                    If Caption <> "" Then
                        Panel = Panel & "<tr>"
                        Panel = Panel & "<td width=""30"" align=""right"">"
                        Panel = Panel & cpCore.html.html_GetFormInputRadioBox("DiagAction" & DiagActionCount, DiagActions(ActionPointer).Command, "")
                        Panel = Panel & "</td>"
                        Panel = Panel & "<td width=""100%"">" & SpanClassAdminNormal & Caption & "</SPAN></td>"
                        Panel = Panel & "</tr>"
                    End If
                Next
            End If
            Panel = Panel & "</TABLE>"
            DiagActionCount = DiagActionCount + 1
            '
            GetDiagError = cpCore.html.main_GetPanel(Panel)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetDiagError", "ErrorTrap")
        End Function
        '
        '
        '
        Private Function GetDiagHeader(ByVal Copy As String) As String
            GetDiagHeader = cpCore.html.main_GetPanel("<B>" & SpanClassAdminNormal & Copy & "</SPAN><B>")
        End Function
        '
        '
        '
        Private Function DiagArgument(ByVal CommandList As String, ByVal CommandPosition As Integer) As String
            Dim CommandCount As Integer
            Dim EndOfList As Boolean
            Dim CommandStartPosition As Integer
            Dim CommandEndPosition As Integer
            '
            DiagArgument = ""
            EndOfList = False
            CommandStartPosition = 1
            Do While (CommandCount < CommandPosition) And (Not EndOfList)
                CommandStartPosition = genericController.vbInstr(CommandStartPosition, CommandList, ",")
                If CommandStartPosition = 0 Then
                    EndOfList = True
                End If
                CommandStartPosition = CommandStartPosition + 1
                CommandCount = CommandCount + 1
            Loop
            If (Not EndOfList) Then
                CommandEndPosition = genericController.vbInstr(CommandStartPosition, CommandList, ",")
                If CommandEndPosition = 0 Then
                    DiagArgument = Mid(CommandList, CommandStartPosition)
                Else
                    DiagArgument = Mid(CommandList, CommandStartPosition, CommandEndPosition - CommandStartPosition)
                End If
            End If
        End Function
        '
        '=============================================================================
        '=============================================================================
        '
        Private Function GetForm_Benchmark() As String
            On Error GoTo ErrorTrap
            '
            Dim MethodName As String
            Dim TestCount As Integer
            Dim TestPointer As Integer
            Dim TestTicks As Integer
            Dim OpenTicks As Integer
            Dim NextTicks As Integer
            Dim ReadTicks As Integer
            Dim RS As DataTable
            Dim TestCopy As String
            Dim Ticks As Integer
            Dim PageSize As Integer
            Dim PageNumber As Integer
            Dim RecordCount As Integer
            Dim SQL As String
            Dim CS As Integer
            Dim Stream As New stringBuilderLegacyController
            Dim ButtonList As String
            '
            ButtonList = ButtonCancel & "," & ButtonRun
            '
            Stream.Add(GetTitle("Benchmark", "Run a series of data operations and compare the results to previous known values."))
            '
            If (cpCore.docProperties.getText("Button") <> "") Then
                '
                '   Run Tools
                '
                Call Stream.Add("<br>")
                '
                Stream.Add(SpanClassAdminNormal)
                TestTicks = GetTickCount
                TestCount = 100
                PageSize = 50
                PageNumber = 1
                SQL = "SELECT * FROM ccSetup WHERE ACTIVE<>0;"
                '
                'Stream.Add("<br>")
                'Stream.Add("Measure ContentServer communication time by getting contentfieldcount 1000 times<br>")
                'Stream.Add(Now & " count=[" & TestCount & "]x, PageSize=[" & PageSize & "]<br>")
                'For TestPointer = 1 To 1000
                '    TestTicks = GetTickCount
                '    TestCopy = genericController.encodeText(cpCore.csGetFieldCount(1))
                '    OpenTicks = OpenTicks + GetTickCount - TestTicks
                'Next
                'Stream.Add(Now & " Finished<br>")
                Stream.Add("Time to make a ContentServer call = " & Format((OpenTicks / 1000), "00.000") & " msec<br>")
                '
                ' ExecuteSQL Test
                '
                Stream.Add("<br>")
                Stream.Add("Recordset ExecuteSQL Test<br>")
                Stream.Add(Now & " count=[" & TestCount & "]x, SQL=[" & SQL & "], PageSize=[" & PageSize & "]<br>")
                OpenTicks = 0
                ReadTicks = 0
                NextTicks = 0
                For TestPointer = 1 To TestCount
                    TestTicks = GetTickCount
                    RS = cpCore.db.executeSql(SQL)
                    OpenTicks = OpenTicks + GetTickCount - TestTicks
                    RecordCount = 0
                    TestTicks = GetTickCount
                    For Each dr As DataRow In RS.Rows
                        NextTicks = NextTicks + GetTickCount - TestTicks
                        '
                        TestTicks = GetTickCount
                        TestCopy = genericController.encodeText(dr("NAME"))
                        ReadTicks = ReadTicks + GetTickCount - TestTicks
                        '
                        RecordCount = RecordCount + 1
                        TestTicks = GetTickCount
                    Next
                    NextTicks = NextTicks + GetTickCount - TestTicks
                Next
                Stream.Add(Now & " Finished<br>")
                Stream.Add("Records = " & RecordCount & "<br>")
                Stream.Add("Time to open recordset = " & Format((OpenTicks) / (TestCount), "00.000") & " msec<br>")
                Stream.Add("Time to read field = " & Format((ReadTicks) / (TestCount * RecordCount), "00.000") & " msec<br>")
                Stream.Add("Time to next record = " & Format((NextTicks) / (TestCount * RecordCount), "00.000") & " msec<br>")
                '
                ' OpenCSContent Test
                '
                Stream.Add("<br>")
                Stream.Add("Contentset OpenCSContent Test<br>")
                Stream.Add(Now & " count=[" & TestCount & "]x, PageSize=[" & PageSize & "]<br>")
                OpenTicks = 0
                ReadTicks = 0
                NextTicks = 0
                For TestPointer = 1 To TestCount
                    '
                    TestTicks = GetTickCount
                    CS = cpCore.db.cs_open("Site Properties", , , , , , ,, PageSize, PageNumber)
                    OpenTicks = OpenTicks + GetTickCount - TestTicks
                    '
                    RecordCount = 0
                    Do While cpCore.db.cs_ok(CS)
                        '
                        TestTicks = GetTickCount
                        TestCopy = genericController.encodeText(cpCore.db.cs_getField(CS, "Name"))
                        ReadTicks = ReadTicks + GetTickCount - TestTicks
                        '
                        TestTicks = GetTickCount
                        Call cpCore.db.cs_goNext(CS)
                        NextTicks = NextTicks + GetTickCount - TestTicks
                        '
                        RecordCount = RecordCount + 1
                    Loop
                    Call cpCore.db.cs_Close(CS)
                    ReadTicks = ReadTicks + GetTickCount - TestTicks
                Next
                Stream.Add(Now & " Finished<br>")
                Stream.Add("Records = " & RecordCount & "<br>")
                Stream.Add("Time to open Contentset = " & Format((OpenTicks) / (TestCount), "00.000") & " msec<br>")
                Stream.Add("Time to read field = " & Format((ReadTicks) / (TestCount * RecordCount), "00.000") & " msec<br>")
                Stream.Add("Time to next record = " & Format((NextTicks) / (TestCount * RecordCount), "00.000") & " msec<br>")
                '
                ' OpenCSContent SelectFieldList Test
                '
                Stream.Add("<br>")
                Stream.Add("Contentset OpenCSContent Test with limited SelectFieldList<br>")
                Stream.Add(Now & " count=[" & TestCount & "]x, PageSize=[" & PageSize & "]<br>")
                OpenTicks = 0
                ReadTicks = 0
                NextTicks = 0
                For TestPointer = 1 To TestCount
                    '
                    TestTicks = GetTickCount
                    CS = cpCore.db.cs_open("Site Properties", , , , , , , "name", PageSize, PageNumber)
                    OpenTicks = OpenTicks + GetTickCount - TestTicks
                    '
                    RecordCount = 0
                    Do While cpCore.db.cs_ok(CS)
                        '
                        TestTicks = GetTickCount
                        TestCopy = genericController.encodeText(cpCore.db.cs_getField(CS, "Name"))
                        ReadTicks = ReadTicks + GetTickCount - TestTicks
                        '
                        TestTicks = GetTickCount
                        Call cpCore.db.cs_goNext(CS)
                        NextTicks = NextTicks + GetTickCount - TestTicks
                        '
                        RecordCount = RecordCount + 1
                    Loop
                    Call cpCore.db.cs_Close(CS)
                    ReadTicks = ReadTicks + GetTickCount - TestTicks
                Next
                Stream.Add(Now & " Finished<br>")
                Stream.Add("Records = " & RecordCount & "<br>")
                Stream.Add("Time to open Contentset = " & Format((OpenTicks) / (TestCount), "00.000") & " msec<br>")
                Stream.Add("Time to read field = " & Format((ReadTicks) / (TestCount * RecordCount), "00.000") & " msec<br>")
                Stream.Add("Time to next record = " & Format((NextTicks) / (TestCount * RecordCount), "00.000") & " msec<br>")
                '
                Stream.Add("</SPAN>")
            End If
            '
            ' Print Start Button
            '
            ''Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolBenchmark)
            '
            GetForm_Benchmark = genericLegacyView.OpenFormTable(cpCore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpCore, ButtonList)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            'RS = Nothing
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_Benchmark", "ErrorTrap")
        End Function
        '
        '=============================================================================
        '   Get a ContentID from the ContentName using just the tables
        '=============================================================================
        '
        Private Function Local_GetContentID(ByVal ContentName As String) As Integer
            On Error GoTo ErrorTrap
            '
            Dim dt As DataTable
            '
            Local_GetContentID = 0
            dt = cpCore.db.executeSql("Select ID from ccContent where name=" & cpCore.db.encodeSQLText(ContentName))
            If dt.Rows.Count > 0 Then
                Local_GetContentID = genericController.EncodeInteger(dt.Rows(0).Item(0))
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("Local_GetContentID", "ErrorTrap")
        End Function
        '
        '=============================================================================
        '   Get a ContentID from the ContentName using just the tables
        '=============================================================================
        '
        Private Function Local_GetContentNameByID(ByVal ContentID As Integer) As String
            On Error GoTo ErrorTrap
            '
            Dim dt As DataTable
            '
            Local_GetContentNameByID = ""
            dt = cpCore.db.executeSql("Select name from ccContent where id=" & ContentID)
            If dt.Rows.Count > 0 Then
                Local_GetContentNameByID = genericController.encodeText(dt.Rows(0).Item(0))
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("Local_GetContentNameByID", "ErrorTrap")
        End Function
        '
        '=============================================================================
        '   Get a ContentID from the ContentName using just the tables
        '=============================================================================
        '
        Private Function Local_GetContentTableName(ByVal ContentName As String) As String
            On Error GoTo ErrorTrap
            '
            Dim RS As DataTable
            '
            Local_GetContentTableName = ""
            RS = cpCore.db.executeSql("Select ccTables.Name as TableName from ccContent Left Join ccTables on ccContent.ContentTableID=ccTables.ID where ccContent.name=" & cpCore.db.encodeSQLText(ContentName))
            If RS.Rows.Count > 0 Then
                Local_GetContentTableName = genericController.encodeText(RS.Rows(0).Item(0))
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("Local_GetContentTableName", "ErrorTrap")
        End Function
        '
        '=============================================================================
        '   Get a ContentID from the ContentName using just the tables
        '=============================================================================
        '
        Private Function Local_GetContentDataSource(ByVal ContentName As String) As String
            On Error GoTo ErrorTrap
            '
            Dim RS As DataTable
            Dim SQL As String
            '
            Local_GetContentDataSource = ""
            SQL = "Select ccDataSources.Name" _
                    & " from ( ccContent Left Join ccTables on ccContent.ContentTableID=ccTables.ID )" _
                    & " Left Join ccDataSources on ccTables.DataSourceID=ccDataSources.ID" _
                    & " where ccContent.name=" & cpCore.db.encodeSQLText(ContentName)
            RS = cpCore.db.executeSql(SQL)
            If isDataTableOk(RS) Then
                Local_GetContentDataSource = genericController.encodeText(RS.Rows(0).Item("Name"))
            End If
            If Local_GetContentDataSource = "" Then
                Local_GetContentDataSource = "Default"
            End If
            Call closeDataTable(RS)
            'RS = Nothing
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("Local_GetContentDataSource", "ErrorTrap")
        End Function
        '
        '=============================================================================
        '   Print the manual query form
        '=============================================================================
        '
        Private Function GetForm_DbSchema() As String
            On Error GoTo ErrorTrap
            '
            Dim SQL As String
            Dim ErrorNumber As Integer
            Dim ErrorDescription As String = ""
            Dim StatusOK As Boolean
            Dim FieldCount As Integer
            Dim CellString As String
            Dim SQLFilename As String
            Dim SQLArchive As String
            Dim SQLArchiveOld As String
            Dim LineCounter As Integer
            Dim SQLLine As String
            Dim Retries As Object
            Dim PageSize As Integer
            Dim PageNumber As Integer
            Dim RowMax As Integer
            Dim RowPointer As Integer
            Dim ColumnMax As Integer
            Dim ColumnPointer As Integer
            Dim ColumnStart As String
            Dim ColumnEnd As String
            Dim RowStart As String
            Dim RowEnd As String
            Dim arrayOfSchema As String(,)
            Dim CellData As String
            Dim SelectFieldWidthLimit As Integer
            Dim SQLName As String
            Dim TableName As String = ""
            Dim Stream As New stringBuilderLegacyController
            Dim ButtonList As String
            Dim RSSchema As DataTable
            Dim datasource As dataSourceModel = dataSourceModel.create(cpCore, cpCore.docProperties.getInteger("DataSourceID"), New List(Of String))
            '
            ButtonList = ButtonCancel & "," & ButtonRun
            '
            Stream.Add(GetTitle("Query Database Schema", "This tool examines the database schema for all tables available."))
            '
            StatusOK = True
            If (cpCore.docProperties.getText("button")) <> ButtonRun Then
                '
                ' First pass, initialize
                '
                PageSize = 10
                PageNumber = 1
                Retries = 0
            Else
                '
                ' Read in arguments
                '
                TableName = cpCore.docProperties.getText("TableName")
                '
                ' Run the SQL
                '
                'ConnectionString = cpCore.db.getmain_GetConnectionString(DataSourceName)
                '
                Call Stream.Add(SpanClassAdminSmall & "<br><br>")
                Stream.Add(Now() & " Opening Table Schema on DataSource [" & datasource.Name & "]<br>")
                On Error Resume Next
                '
                RSSchema = cpCore.db.getTableSchemaData(TableName)
                If ErrorNumber = 0 Then
                    Stream.Add(Now() & " GetSchema executed successfully<br>")
                Else
                    '
                    ' ----- error
                    '
                    Stream.Add(Now() & " SQL execution returned the following error<br>")
                    Stream.Add("Error Number " & ErrorNumber & "<br>")
                    Stream.Add("Error Descrition " & ErrorDescription & "<br>")
                End If
                If (Not isDataTableOk(RSSchema)) Then
                    '
                    ' ----- no result
                    '
                    Stream.Add(Now() & " A schema was returned, but it contains no records.<br>")
                Else
                    '
                    ' ----- print results
                    '
                    PageSize = 9999
                    Call Stream.Add(Now() & " The following results were returned<br>")
                    '
                    ' --- Create the Fields for the new table
                    '
                    FieldCount = RSSchema.Columns.Count
                    Stream.Add("<table border=""1"" cellpadding=""1"" cellspacing=""1"" width=""100%"">")
                    Stream.Add("<tr>")
                    For Each RecordField As DataColumn In RSSchema.Columns
                        Call Stream.Add("<TD><B>" & SpanClassAdminSmall & RecordField.ColumnName & "</b></SPAN></td>")
                    Next
                    Stream.Add("</tr>")
                    '
                    'Dim dtok As Boolean = False
                    arrayOfSchema = cpCore.db.convertDataTabletoArray(RSSchema)
                    '
                    RowMax = UBound(arrayOfSchema, 2)
                    ColumnMax = UBound(arrayOfSchema, 1)
                    RowStart = "<tr>"
                    RowEnd = "</tr>"
                    ColumnStart = "<td class=""ccadminsmall"">"
                    ColumnEnd = "</td>"
                    'ColumnStart = "<td class=""ccadminsmall"">&nbsp;"
                    'ColumnEnd = "&nbsp;</td>"
                    'ColumnStart = "<TD>" & SpanClassAdminSmall
                    'ColumnEnd = "</SPAN></td>"
                    For RowPointer = 0 To RowMax
                        Stream.Add(RowStart)
                        For ColumnPointer = 0 To ColumnMax
                            CellData = arrayOfSchema(ColumnPointer, RowPointer)
                            If IsNull(CellData) Then
                                Stream.Add(ColumnStart & "[null]" & ColumnEnd)
                            ElseIf IsNothing(CellData) Then
                                Stream.Add(ColumnStart & "[empty]" & ColumnEnd)
                            ElseIf CellData = "" Then
                                Stream.Add(ColumnStart & "[empty]" & ColumnEnd)
                            Else
                                Stream.Add(ColumnStart & CellData & ColumnEnd)
                            End If
                        Next
                        Stream.Add(RowEnd)
                    Next
                    Stream.Add("</TABLE>")
                    RSSchema.Dispose()
                    RSSchema = Nothing
                End If
                '
                ' Index Schema
                '
                '    RSSchema = DataSourceConnectionObjs(DataSourcePointer).Conn.OpenSchema(SchemaEnum.adSchemaColumns, Array(Empty, Empty, TableName, Empty))
                Call Stream.Add(SpanClassAdminSmall & "<br><br>")
                Stream.Add(Now() & " Opening Index Schema<br>")
                On Error Resume Next
                '
                RSSchema = cpCore.db.getIndexSchemaData(TableName)
                If (Not isDataTableOk(RSSchema)) Then
                    '
                    ' ----- no result
                    '
                    Stream.Add(Now() & " A schema was returned, but it contains no records.<br>")
                Else
                    '
                    ' ----- print results
                    '
                    PageSize = 999
                    Call Stream.Add(Now() & " The following results were returned<br>")
                    '
                    ' --- Create the Fields for the new table
                    '
                    FieldCount = RSSchema.Columns.Count
                    Stream.Add("<table border=""1"" cellpadding=""1"" cellspacing=""1"" width=""100%"">")
                    Stream.Add("<tr>")
                    For Each RecordField As DataColumn In RSSchema.Columns
                        Call Stream.Add("<TD><B>" & SpanClassAdminSmall & RecordField.ColumnName & "</b></SPAN></td>")
                    Next
                    Stream.Add("</tr>")
                    '

                    arrayOfSchema = cpCore.db.convertDataTabletoArray(RSSchema)
                    '
                    RowMax = UBound(arrayOfSchema, 2)
                    ColumnMax = UBound(arrayOfSchema, 1)
                    RowStart = "<tr>"
                    RowEnd = "</tr>"
                    ColumnStart = "<td class=""ccadminsmall"">"
                    ColumnEnd = "</td>"
                    'ColumnStart = "<td class=""ccadminsmall"">&nbsp;"
                    'ColumnEnd = "&nbsp;</td>"
                    'ColumnStart = "<TD>" & SpanClassAdminSmall
                    'ColumnEnd = "</SPAN></td>"
                    For RowPointer = 0 To RowMax
                        Stream.Add(RowStart)
                        For ColumnPointer = 0 To ColumnMax
                            CellData = arrayOfSchema(ColumnPointer, RowPointer)
                            If IsNull(CellData) Then
                                Stream.Add(ColumnStart & "[null]" & ColumnEnd)
                            ElseIf IsNothing(CellData) Then
                                Stream.Add(ColumnStart & "[empty]" & ColumnEnd)
                            ElseIf CellData = "" Then
                                Stream.Add(ColumnStart & "[empty]" & ColumnEnd)
                            Else
                                Stream.Add(ColumnStart & CellData & ColumnEnd)
                            End If
                        Next
                        Stream.Add(RowEnd)
                    Next
                    Stream.Add("</TABLE>")
                    RSSchema.Dispose()
                    RSSchema = Nothing
                End If
                '
                ' Column Schema
                '
                Call Stream.Add(SpanClassAdminSmall & "<br><br>")
                Stream.Add(Now() & " Opening Column Schema<br>")
                On Error Resume Next
                '
                RSSchema = cpCore.db.getColumnSchemaData(TableName)
                If ErrorNumber = 0 Then
                    Stream.Add(Now() & " GetSchema executed successfully<br>")
                Else
                    '
                    ' ----- error
                    '
                    Stream.Add(Now() & " SQL execution returned the following error<br>")
                    Stream.Add("Error Number " & ErrorNumber & "<br>")
                    Stream.Add("Error Descrition " & ErrorDescription & "<br>")
                End If
                If (isDataTableOk(RSSchema)) Then
                    '
                    ' ----- no result
                    '
                    Stream.Add(Now() & " A schema was returned, but it contains no records.<br>")
                Else
                    '
                    ' ----- print results
                    '
                    PageSize = 9999
                    Call Stream.Add(Now() & " The following results were returned<br>")
                    '
                    ' --- Create the Fields for the new table
                    '
                    FieldCount = RSSchema.Columns.Count
                    Stream.Add("<table border=""1"" cellpadding=""1"" cellspacing=""1"" width=""100%"">")
                    Stream.Add("<tr>")
                    For Each RecordField As DataColumn In RSSchema.Columns
                        Call Stream.Add("<TD><B>" & SpanClassAdminSmall & RecordField.ColumnName & "</b></SPAN></td>")
                    Next
                    Stream.Add("</tr>")
                    '
                    arrayOfSchema = cpCore.db.convertDataTabletoArray(RSSchema)
                    '
                    RowMax = UBound(arrayOfSchema, 2)
                    ColumnMax = UBound(arrayOfSchema, 1)
                    RowStart = "<tr>"
                    RowEnd = "</tr>"
                    ColumnStart = "<td class=""ccadminsmall"">"
                    ColumnEnd = "</td>"
                    'ColumnStart = "<td class=""ccadminsmall"">&nbsp;"
                    'ColumnEnd = "&nbsp;</td>"
                    'ColumnStart = "<TD>" & SpanClassAdminSmall
                    'ColumnEnd = "</SPAN></td>"
                    For RowPointer = 0 To RowMax
                        Stream.Add(RowStart)
                        For ColumnPointer = 0 To ColumnMax
                            CellData = arrayOfSchema(ColumnPointer, RowPointer)
                            If IsNull(CellData) Then
                                Stream.Add(ColumnStart & "[null]" & ColumnEnd)
                            ElseIf IsNothing(CellData) Then
                                Stream.Add(ColumnStart & "[empty]" & ColumnEnd)
                            ElseIf CellData = "" Then
                                Stream.Add(ColumnStart & "[empty]" & ColumnEnd)
                            Else
                                Stream.Add(ColumnStart & CellData & ColumnEnd)
                            End If
                        Next
                        Stream.Add(RowEnd)
                    Next
                    Stream.Add("</TABLE>")
                    RSSchema.Dispose()
                    RSSchema = Nothing
                End If
                If Not StatusOK Then
                    Call Stream.Add("There was a problem executing this query that may have prevented the results from printing.")
                End If
                Call Stream.Add(Now() & " Done</SPAN>")
            End If
            '
            ' Display form
            '
            Call Stream.Add(SpanClassAdminNormal)
            '
            Call Stream.Add("<br>")
            Call Stream.Add("Table Name<br>")
            Stream.Add(cpCore.html.html_GetFormInputText("Tablename", TableName))
            '
            Call Stream.Add("<br><br>")
            Call Stream.Add("Data Source<br>")
            Stream.Add(cpCore.html.main_GetFormInputSelect("DataSourceID", datasource.ID, "Data Sources", "", "Default"))
            '
            ''Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolSchema)
            Call Stream.Add("</SPAN>")
            '
            GetForm_DbSchema = genericLegacyView.OpenFormTable(cpCore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpCore, ButtonList)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            'RSSchema = Nothing
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_DbSchema", "ErrorTrap")
            StatusOK = False
        End Function
        '
        '=============================================================================
        '   Get the Configure Edit
        '=============================================================================
        '
        Private Function GetForm_ConfigureEdit(cp As BaseClasses.CPBaseClass) As String
            Dim result As String = ""
            Try
                '
                Dim SQL As String
                Dim DataSourceName As String
                Dim ToolButton As String
                Dim ContentID As Integer
                Dim RecordCount As Integer
                Dim RecordPointer As Integer
                Dim CSPointer As Integer
                Dim formFieldId As Integer
                Dim ContentName As String
                Dim CDef As cdefModel
                Dim formFieldName As String
                Dim formFieldTypeId As Integer
                Dim TableName As String
                Dim ContentFieldsCID As Integer
                Dim StatusMessage As String = ""
                Dim ErrorMessage As String = ""
                Dim formFieldInherited As Boolean
                Dim AllowContentAutoLoad As Boolean
                Dim ReloadCDef As Boolean
                Dim CSTarget As Integer
                Dim CSSource As Integer
                Dim AllowCDefInherit As Boolean
                Dim ParentContentID As Integer
                Dim ParentContentName As String
                Dim ParentCDef As cdefModel = Nothing
                Dim NeedFootNote1 As Boolean
                Dim NeedFootNote2 As Boolean
                Dim FormPanel As String = ""
                Dim Index As New keyPtrController
                Dim ButtonList As String
                Dim Stream As New stringBuilderLegacyController
                Dim StreamValidRows As New stringBuilderLegacyController
                Dim TypeSelectTemplate As String
                Dim TypeSelect As String
                Dim FieldCount As Integer
                Dim parentField As CDefFieldModel = Nothing
                '
                ButtonList = ButtonCancel & "," & ButtonSelect
                '
                ToolButton = cp.Doc.GetText("Button")
                ReloadCDef = cp.Doc.GetBoolean("ReloadCDef")
                ContentID = cp.Doc.GetInteger("" & RequestNameToolContentID & "")
                If (ContentID > 0) Then
                    ContentName = cp.Content.GetRecordName("content", ContentID)
                    If (Not String.IsNullOrEmpty(ContentName)) Then
                        TableName = cp.Content.GetTable(ContentName)
                        DataSourceName = cp.Content.GetDataSource(ContentName)
                        CDef = cpCore.metaData.getCdef(ContentID, True, True)
                    End If
                End If
                '
                If (ToolButton <> "") Then
                    If (ToolButton <> ButtonCancel) Then
                        '
                        ' Save the form changes
                        '
                        AllowContentAutoLoad = cp.Site.GetBoolean("AllowContentAutoLoad", "true")
                        Call cp.Site.SetProperty("AllowContentAutoLoad", "false")
                        '
                        ' ----- Save the input
                        '
                        RecordCount = genericController.EncodeInteger(cp.Doc.GetInteger("dtfaRecordCount"))
                        If RecordCount > 0 Then
                            For RecordPointer = 0 To RecordCount - 1
                                '
                                formFieldName = cp.Doc.GetText("dtfaName." & RecordPointer)
                                formFieldTypeId = cp.Doc.GetInteger("dtfaType." & RecordPointer)
                                formFieldId = genericController.EncodeInteger(cp.Doc.GetInteger("dtfaID." & RecordPointer))
                                formFieldInherited = cp.Doc.GetBoolean("dtfaInherited." & RecordPointer)
                                '
                                ' problem - looking for the name in the Db using the form's name, but it could have changed.
                                ' have to look field up by id
                                '
                                For Each cdefFieldKvp As KeyValuePair(Of String, CDefFieldModel) In CDef.fields
                                    If cdefFieldKvp.Value.id = formFieldId Then
                                        '
                                        ' Field was found in CDef
                                        '
                                        With cdefFieldKvp.Value
                                            If .inherited And (Not formFieldInherited) Then
                                                '
                                                ' Was inherited, but make a copy of the field
                                                '
                                                CSTarget = cpCore.db.cs_insertRecord("Content Fields")
                                                If (cpCore.db.cs_ok(CSTarget)) Then
                                                    CSSource = cpCore.db.cs_openContentRecord("Content Fields", formFieldId)
                                                    If (cpCore.db.cs_ok(CSSource)) Then
                                                        Call cpCore.db.cs_copyRecord(CSSource, CSTarget)
                                                    End If
                                                    Call cpCore.db.cs_Close(CSSource)
                                                    formFieldId = cpCore.db.cs_getInteger(CSTarget, "ID")
                                                    Call cpCore.db.cs_set(CSTarget, "ContentID", ContentID)
                                                End If
                                                Call cpCore.db.cs_Close(CSTarget)
                                                ReloadCDef = True
                                            ElseIf (Not .inherited) And (formFieldInherited) Then
                                                '
                                                ' Was a field, make it inherit from it's parent
                                                '
                                                CSTarget = CSTarget
                                                Call cpCore.db.deleteContentRecord("Content Fields", formFieldId)
                                                ReloadCDef = True
                                            ElseIf (Not .inherited) And (Not formFieldInherited) Then
                                                '
                                                ' not inherited, save the field values and mark for a reload
                                                '
                                                If True Then
                                                    If (InStr(1, formFieldName, " ") <> 0) Then
                                                        '
                                                        ' remoave spaces from new name
                                                        '
                                                        StatusMessage = StatusMessage & "<LI>Field [" & formFieldName & "] was renamed [" & genericController.vbReplace(formFieldName, " ", "") & "] because the field name can not include spaces.</LI>"
                                                        formFieldName = genericController.vbReplace(formFieldName, " ", "")
                                                    End If
                                                    '
                                                    If (formFieldName <> "") And (formFieldTypeId <> 0) And ((.nameLc = "") Or (.fieldTypeId = 0)) Then
                                                        '
                                                        ' Create Db field, Field is good but was not before
                                                        '
                                                        Call cpCore.db.createSQLTableField(DataSourceName, TableName, formFieldName, formFieldTypeId)
                                                        StatusMessage = StatusMessage & "<LI>Field [" & formFieldName & "] was saved to this content definition and a database field was created in [" & CDef.ContentTableName & "].</LI>"
                                                    ElseIf (formFieldName = "") Or (formFieldTypeId = 0) Then
                                                        '
                                                        ' name blank or type=0 - do nothing but tell them
                                                        '
                                                        If formFieldName = "" And formFieldTypeId = 0 Then
                                                            ErrorMessage &= "<LI>Field number " & RecordPointer + 1 & " was saved to this content definition but no database field was created because a name and field type are required.</LI>"
                                                        ElseIf formFieldName = "unnamedfield" & coreToolsClass.fieldId.ToString Then
                                                            ErrorMessage &= "<LI>Field number " & RecordPointer + 1 & " was saved to this content definition but no database field was created because a field name is required.</LI>"
                                                        Else
                                                            ErrorMessage &= "<LI>Field [" & formFieldName & "] was saved to this content definition but no database field was created because a field type are required.</LI>"
                                                        End If
                                                    ElseIf (formFieldName = .nameLc) And (formFieldTypeId <> .fieldTypeId) Then
                                                        '
                                                        ' Field Type changed, must be done manually
                                                        '
                                                        ErrorMessage &= "<LI>Field [" & formFieldName & "] changed type from [" & cpCore.db.getRecordName("content Field Types", .fieldTypeId) & "] to [" & cpCore.db.getRecordName("content Field Types", formFieldTypeId) & "]. This may have caused a problem converting content.</LI>"
                                                        Dim DataSourceTypeID As Integer
                                                        DataSourceTypeID = cpCore.db.getDataSourceType(DataSourceName)
                                                        Select Case DataSourceTypeID
                                                            Case DataSourceTypeODBCMySQL
                                                                SQL = "alter table " & CDef.ContentTableName & " change " & .nameLc & " " & .nameLc & " " & cpCore.db.getSQLAlterColumnType(DataSourceName, fieldType) & ";"
                                                            Case Else
                                                                SQL = "alter table " & CDef.ContentTableName & " alter column " & .nameLc & " " & cpCore.db.getSQLAlterColumnType(DataSourceName, fieldType) & ";"
                                                        End Select
                                                        Call cpCore.db.executeSql(SQL, DataSourceName)
                                                    End If
                                                    SQL = "Update ccFields" _
                                                    & " Set name=" & cpCore.db.encodeSQLText(formFieldName) _
                                                    & ",type=" & formFieldTypeId _
                                                    & ",caption=" & cpCore.db.encodeSQLText(cp.Doc.GetText("dtfaCaption." & RecordPointer)) _
                                                    & ",DefaultValue=" & cpCore.db.encodeSQLText(cp.Doc.GetText("dtfaDefaultValue." & RecordPointer)) _
                                                    & ",EditSortPriority=" & cpCore.db.encodeSQLText(genericController.encodeText(cp.Doc.GetInteger("dtfaEditSortPriority." & RecordPointer))) _
                                                    & ",Active=" & cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaActive." & RecordPointer)) _
                                                    & ",ReadOnly=" & cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaReadOnly." & RecordPointer)) _
                                                    & ",Authorable=" & cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaAuthorable." & RecordPointer)) _
                                                    & ",Required=" & cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaRequired." & RecordPointer)) _
                                                    & ",UniqueName=" & cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaUniqueName." & RecordPointer)) _
                                                    & ",TextBuffered=" & cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaTextBuffered." & RecordPointer)) _
                                                    & ",Password=" & cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaPassword." & RecordPointer)) _
                                                    & ",HTMLContent=" & cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaHTMLContent." & RecordPointer)) _
                                                    & ",EditTab=" & cpCore.db.encodeSQLText(cp.Doc.GetText("dtfaEditTab." & RecordPointer)) _
                                                    & ",Scramble=" & cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaScramble." & RecordPointer)) _
                                                    & ""
                                                    If cpCore.authContext.isAuthenticatedAdmin(cpCore) Then
                                                        SQL &= ",adminonly=" & cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaAdminOnly." & RecordPointer))
                                                    End If
                                                    If cpCore.authContext.isAuthenticatedDeveloper(cpCore) Then
                                                        SQL &= ",DeveloperOnly=" & cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaDeveloperOnly." & RecordPointer))
                                                    End If
                                                    SQL &= " where ID=" & formFieldId
                                                    Call cpCore.db.executeSql(SQL)
                                                    ReloadCDef = True
                                                End If
                                            End If
                                        End With
                                        Exit For
                                    End If
                                Next
                            Next
                        End If
                        cpCore.cache.invalidateAll()
                        cpCore.metaData.clear()
                    End If
                    If (ToolButton = ButtonAdd) Then
                        debugController.debug_testPoint(cpCore, "ConfigureEdit, Process Add Button")
                        '
                        ' ----- Insert a blank Field
                        '
                        CSPointer = cpCore.db.cs_insertRecord("Content Fields")
                        If cpCore.db.cs_ok(CSPointer) Then
                            Call cpCore.db.cs_set(CSPointer, "name", "unnamedField" & cpCore.db.cs_getInteger(CSPointer, "id").ToString())
                            Call cpCore.db.cs_set(CSPointer, "ContentID", ContentID)
                            Call cpCore.db.cs_set(CSPointer, "EditSortPriority", 0)
                            ReloadCDef = True
                        End If
                        Call cpCore.db.cs_Close(CSPointer)
                    End If
                    ''
                    '' ----- Button Reload CDef
                    ''
                    If (ToolButton = ButtonSaveandInvalidateCache) Then
                        cpCore.cache.invalidateAll()
                        cpCore.metaData.clear()
                    End If
                    '
                    ' ----- Restore Content Autoload site property
                    '
                    If AllowContentAutoLoad Then
                        Call cp.Site.SetProperty("AllowContentAutoLoad", AllowContentAutoLoad.ToString())
                    End If
                    '
                    ' ----- Cancel or Save, reload CDef and go
                    '
                    If (ToolButton = ButtonCancel) Or (ToolButton = ButtonOK) Then
                        '
                        ' ----- Exit back to menu
                        '
                        Call cpCore.webServer.redirect(cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.webServerIO_requestDomain & cpCore.webServer.requestPath & cpCore.webServer.requestPage & "?af=" & AdminFormTools)
                    End If
                End If
                '
                '--------------------------------------------------------------------------------
                '   Print Output
                '--------------------------------------------------------------------------------
                '
                Stream.Add(SpanClassAdminNormal & "<strong><a href=""" & cpCore.webServer.requestPage & "?af=" & AdminFormToolRoot & """>Tools</a></strong>&nbsp;»&nbsp;Manage Admin Edit Fields</span>")
                Stream.Add("<div>")
                Stream.Add("<div style=""width:45%;float:left;padding:10px;"">" _
                    & "Use this tool to add or modify content definition fields. Contensive uses a caching system for content definitions that is not automatically reloaded. Change you make will not take effect until the next time the system is reloaded. When you create a new field, the database field is created automatically when you have saved both a name and a field type. If you change the field type, you may have to manually change the database field." _
                    & "</div>")
                If ContentID = 0 Then
                    Stream.Add("<div style=""width:45%;float:left;padding:10px;"">Related Tools:" _
                        & "<div style=""padding-left:20px;""><a href=""?af=104"">Set Default Admin Listing page columns</a></div>" _
                        & "</div>")
                Else
                    Stream.Add("<div style=""width:45%;float:left;padding:10px;"">Related Tools:" _
                        & "<div style=""padding-left:20px;""><a href=""?af=104&ContentID=" & ContentID & """>Set Default Admin Listing page columns for '" & ContentName & "'</a></div>" _
                        & "<div style=""padding-left:20px;""><a href=""?af=4&cid=" & cpCore.metaData.getContentId("content") & "&id=" & ContentID & """>Edit '" & ContentName & "' Content Definition</a></div>" _
                        & "<div style=""padding-left:20px;""><a href=""?cid=" & ContentID & """>View records in '" & ContentName & "'</a></div>" _
                        & "</div>")
                End If
                Stream.Add("</div>")
                Stream.Add("<div style=""clear:both"">&nbsp;</div>")
                If StatusMessage <> "" Then
                    Call Stream.Add("<UL>" & StatusMessage & "</UL>")
                    Stream.Add("<div style=""clear:both"">&nbsp;</div>")
                End If
                If ErrorMessage <> "" Then
                    Call Stream.Add("<br><span class=""ccError"">There was a problem saving these changes</span><UL>" & ErrorMessage & "</UL></SPAN>")
                    Stream.Add("<div style=""clear:both"">&nbsp;</div>")
                End If
                If ReloadCDef Then
                    CDef = cpCore.metaData.getCdef(ContentID, True, True)
                End If
                If (ContentID <> 0) Then
                    '
                    '--------------------------------------------------------------------------------
                    ' print the Configure edit form
                    '--------------------------------------------------------------------------------
                    '
                    Call Stream.Add(cpCore.html.main_GetPanelTop())
                    ContentName = Local_GetContentNameByID(ContentID)
                    ButtonList = ButtonCancel & "," & ButtonSave & "," & ButtonOK & "," & ButtonAdd '  & "," & ButtonReloadCDef
                    '
                    ' Get a new copy of the content definition
                    '
                    Call Stream.Add(SpanClassAdminNormal & "<P><B>" & ContentName & "</b></P>")
                    Stream.Add("<table border=""0"" cellpadding=""1"" cellspacing=""1"" width=""100%"">")
                    '
                    ParentContentID = CDef.parentID
                    If ParentContentID = -1 Then
                        AllowCDefInherit = False
                    Else
                        AllowCDefInherit = True
                        ParentContentName = cpCore.metaData.getContentNameByID(ParentContentID)
                        ParentCDef = cpCore.metaData.getCdef(ParentContentID, True, True)
                    End If
                    If CDef.fields.Count > 0 Then
                        Stream.Add("<tr>")
                        Stream.Add("<td valign=""bottom"" width=""50"" class=""ccPanelInput"" align=""center""></td>")
                        If Not AllowCDefInherit Then
                            Stream.Add("<td valign=""bottom"" width=""100"" class=""ccPanelInput"" align=""center"">" & SpanClassAdminSmall & "<b><br>Inherited*</b></span></td>")
                            NeedFootNote1 = True
                        Else
                            Stream.Add("<td valign=""bottom"" width=""100"" class=""ccPanelInput"" align=""center"">" & SpanClassAdminSmall & "<b><br>Inherited</b></span></td>")
                        End If
                        Stream.Add("<td valign=""bottom"" width=""100"" class=""ccPanelInput"" align=""left"">" & SpanClassAdminSmall & "<b><br>Field</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""100"" class=""ccPanelInput"" align=""left"">" & SpanClassAdminSmall & "<b><br>Caption</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""100"" class=""ccPanelInput"" align=""left"">" & SpanClassAdminSmall & "<b><br>Edit Tab</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""100"" class=""ccPanelInput"" align=""left"">" & SpanClassAdminSmall & "<b><br>Default</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""50"" class=""ccPanelInput"" align=""left"">" & SpanClassAdminSmall & "<b><br>Type</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""50"" class=""ccPanelInput"" align=""left"">" & SpanClassAdminSmall & "<b>Edit<br>Order</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""50"" class=""ccPanelInput"" align=""center"">" & SpanClassAdminSmall & "<b><br>Active</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""50"" class=""ccPanelInput"" align=""center"">" & SpanClassAdminSmall & "<b>Read<br>Only</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""50"" class=""ccPanelInput"" align=""center"">" & SpanClassAdminSmall & "<b><br>Auth</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""50"" class=""ccPanelInput"" align=""center"">" & SpanClassAdminSmall & "<b><br>Req</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""50"" class=""ccPanelInput"" align=""center"">" & SpanClassAdminSmall & "<b><br>Unique</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""50"" class=""ccPanelInput"" align=""center"">" & SpanClassAdminSmall & "<b>Text<br>Buffer</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""50"" class=""ccPanelInput"" align=""center"">" & SpanClassAdminSmall & "<b><br>Pass</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""50"" class=""ccPanelInput"" align=""center"">" & SpanClassAdminSmall & "<b>Text<br>Scrm</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""50"" class=""ccPanelInput"" align=""left"">" & SpanClassAdminSmall & "<b><br>HTML</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""50"" class=""ccPanelInput"" align=""left"">" & SpanClassAdminSmall & "<b>Admin<br>Only</b></span></td>")
                        Stream.Add("<td valign=""bottom"" width=""50"" class=""ccPanelInput"" align=""left"">" & SpanClassAdminSmall & "<b>Dev<br>Only</b></span></td>")
                        Stream.Add("</tr>")
                        RecordCount = 0
                        '
                        ' Build a select template for Type
                        '
                        TypeSelectTemplate = cpCore.html.main_GetFormInputSelect("menuname", -1, "Content Field Types", "", "unknown")
                        '
                        ' Index the sort order
                        '
                        Dim fieldList As New List(Of fieldSortClass)
                        FieldCount = CDef.fields.Count
                        For Each keyValuePair In CDef.fields
                            Dim fieldSort As New fieldSortClass
                            'Dim field As New appServices_metaDataClass.CDefFieldClass
                            Dim sortOrder As String = ""
                            fieldSort.field = keyValuePair.Value
                            sortOrder = ""
                            If fieldSort.field.active Then
                                sortOrder &= "0"
                            Else
                                sortOrder &= "1"
                            End If
                            If fieldSort.field.authorable Then
                                sortOrder &= "0"
                            Else
                                sortOrder &= "1"
                            End If
                            sortOrder &= fieldSort.field.editTabName & GetIntegerString(fieldSort.field.editSortPriority, 10) & GetIntegerString(fieldSort.field.id, 10)
                            fieldSort.sort = sortOrder
                            fieldList.Add(fieldSort)
                        Next
                        fieldList.Sort(Function(p1, p2) p1.sort.CompareTo(p2.sort))
                        For Each fieldsort As fieldSortClass In fieldList
                            Dim streamRow As New stringBuilderLegacyController
                            Dim rowValid As Boolean = True
                            With fieldsort.field
                                '
                                ' If Field has name and type, it is locked and can not be changed
                                '
                                Dim FieldLocked As Boolean = (.nameLc <> "") And (.fieldTypeId <> 0)
                                '
                                ' put the menu into the current menu format
                                '
                                formFieldId = .id
                                Call streamRow.Add(cpCore.html.html_GetFormInputHidden("dtfaID." & RecordCount, formFieldId))
                                streamRow.Add("<tr>")
                                '
                                ' edit button
                                '
                                streamRow.Add("<td class=""ccPanelInput"" align=""left""><img src=""/ccLib/images/spacer.gif"" width=""1"" height=""10"">")
                                ContentFieldsCID = Local_GetContentID("Content Fields")
                                If (ContentFieldsCID <> 0) Then
                                    streamRow.Add("<nobr>" & SpanClassAdminSmall & "[<a href=""?aa=" & AdminActionNop & "&af=" & AdminFormEdit & "&id=" & formFieldId & "&cid=" & ContentFieldsCID & "&mm=0"">EDIT</a>]</span></nobr>")
                                End If
                                streamRow.Add("</td>")
                                '
                                ' Inherited
                                '
                                If Not AllowCDefInherit Then
                                    '
                                    ' no parent
                                    '
                                    streamRow.Add("<td class=""ccPanelInput"" align=""center"">" & SpanClassAdminSmall & "False</span></td>")
                                ElseIf .inherited Then
                                    '
                                    ' inherited property
                                    '
                                    streamRow.Add("<td class=""ccPanelInput"" align=""center"">" & cpCore.html.html_GetFormInputCheckBox2("dtfaInherited." & RecordCount, .inherited) & "</td>")
                                Else
                                    '
                                    ' CDef has a parent, but the field is non-inherited, test for a matching Parent Field
                                    '
                                    If (ParentCDef Is Nothing) Then
                                        For Each kvp As KeyValuePair(Of String, CDefFieldModel) In ParentCDef.fields
                                            If kvp.Value.nameLc = .nameLc Then
                                                parentField = kvp.Value
                                                Exit For
                                            End If
                                        Next
                                    End If
                                    If (parentField Is Nothing) Then
                                        streamRow.Add("<td class=""ccPanelInput"" align=""center"">" & SpanClassAdminSmall & "False**</span></td>")
                                        NeedFootNote2 = True
                                    Else
                                        streamRow.Add("<td class=""ccPanelInput"" align=""center"">" & cpCore.html.html_GetFormInputCheckBox2("dtfaInherited." & RecordCount, .inherited) & "</td>")
                                    End If
                                End If
                                '
                                ' name
                                '
                                Dim tmpValue As Boolean = String.IsNullOrEmpty(.nameLc)
                                rowValid = rowValid And Not tmpValue
                                streamRow.Add("<td class=""ccPanelInput"" align=""left""><nobr>")
                                If .inherited Then
                                    Call streamRow.Add(SpanClassAdminSmall & .nameLc & "&nbsp;</SPAN>")
                                ElseIf FieldLocked Then
                                    Call streamRow.Add(SpanClassAdminSmall & .nameLc & "&nbsp;</SPAN><input type=hidden name=dtfaName." & RecordCount & " value=""" & .nameLc & """>")
                                Else
                                    Call streamRow.Add(cpCore.html.html_GetFormInputText2("dtfaName." & RecordCount, .nameLc, 1, 10))
                                End If
                                streamRow.Add("</nobr></td>")
                                '
                                ' caption
                                '
                                streamRow.Add("<td class=""ccPanelInput"" align=""left""><nobr>")
                                If .inherited Then
                                    Call streamRow.Add(SpanClassAdminSmall & .caption & "</SPAN>")
                                Else
                                    Call streamRow.Add(cpCore.html.html_GetFormInputText2("dtfaCaption." & RecordCount, .caption, 1, 10))
                                End If
                                streamRow.Add("</nobr></td>")
                                '
                                ' Edit Tab
                                '
                                streamRow.Add("<td class=""ccPanelInput"" align=""left""><nobr>")
                                If .inherited Then
                                    Call streamRow.Add(SpanClassAdminSmall & .editTabName & "</SPAN>")
                                Else
                                    Call streamRow.Add(cpCore.html.html_GetFormInputText2("dtfaEditTab." & RecordCount, .editTabName, 1, 10))
                                End If
                                streamRow.Add("</nobr></td>")
                                '
                                ' default
                                '
                                streamRow.Add("<td class=""ccPanelInput"" align=""left""><nobr>")
                                If .inherited Then
                                    Call streamRow.Add(SpanClassAdminSmall & genericController.encodeText(.defaultValue) & "</SPAN>")
                                Else
                                    Call streamRow.Add(cpCore.html.html_GetFormInputText2("dtfaDefaultValue." & RecordCount, genericController.encodeText(.defaultValue), 1, 10))
                                End If
                                streamRow.Add("</nobr></td>")
                                '
                                ' type
                                '
                                rowValid = rowValid And (.fieldTypeId > 0)
                                streamRow.Add("<td class=""ccPanelInput"" align=""left""><nobr>")
                                If .inherited Then
                                    CSPointer = cpCore.db.csOpenRecord("Content Field Types", .fieldTypeId)
                                    If Not cpCore.db.cs_ok(CSPointer) Then
                                        Call streamRow.Add(SpanClassAdminSmall & "Unknown[" & .fieldTypeId & "]</SPAN>")
                                    Else
                                        Call streamRow.Add(SpanClassAdminSmall & cpCore.db.cs_getText(CSPointer, "Name") & "</SPAN>")
                                    End If
                                    Call cpCore.db.cs_Close(CSPointer)
                                ElseIf FieldLocked Then
                                    Call streamRow.Add(cpCore.db.getRecordName("content field types", .fieldTypeId) & cpCore.html.html_GetFormInputHidden("dtfaType." & RecordCount, .fieldTypeId))
                                Else
                                    TypeSelect = TypeSelectTemplate
                                    TypeSelect = genericController.vbReplace(TypeSelect, "menuname", "dtfaType." & RecordCount, 1, 99, vbTextCompare)
                                    TypeSelect = genericController.vbReplace(TypeSelect, "=""" & .fieldTypeId & """", "=""" & .fieldTypeId & """ selected", 1, 99, vbTextCompare)
                                    Call streamRow.Add(TypeSelect)
                                End If
                                streamRow.Add("</nobr></td>")
                                '
                                ' sort priority
                                '
                                streamRow.Add("<td class=""ccPanelInput"" align=""left""><nobr>")
                                If .inherited Then
                                    Call streamRow.Add(SpanClassAdminSmall & .editSortPriority & "</SPAN>")
                                Else
                                    Call streamRow.Add(cpCore.html.html_GetFormInputText2("dtfaEditSortPriority." & RecordCount, .editSortPriority.ToString(), 1, 10))
                                End If
                                streamRow.Add("</nobr></td>")
                                '
                                ' active
                                '
                                streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaActive." & RecordCount, genericController.encodeText(.active), .inherited))
                                '
                                ' read only
                                '
                                streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaReadOnly." & RecordCount, genericController.encodeText(.ReadOnly), .inherited))
                                '
                                ' authorable
                                '
                                streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaAuthorable." & RecordCount, genericController.encodeText(.authorable), .inherited))
                                '
                                ' required
                                '
                                streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaRequired." & RecordCount, genericController.encodeText(.Required), .inherited))
                                '
                                ' UniqueName
                                '
                                streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaUniqueName." & RecordCount, genericController.encodeText(.UniqueName), .inherited))
                                '
                                ' text buffered
                                '
                                streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaTextBuffered." & RecordCount, genericController.encodeText(.TextBuffered), .inherited))
                                '
                                ' password
                                '
                                streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaPassword." & RecordCount, genericController.encodeText(.Password), .inherited))
                                '
                                ' scramble
                                '
                                streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaScramble." & RecordCount, genericController.encodeText(.Scramble), .inherited))
                                '
                                ' HTML Content
                                '
                                streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaHTMLContent." & RecordCount, genericController.encodeText(.htmlContent), .inherited))
                                '
                                ' Admin Only
                                '
                                If cpCore.authContext.isAuthenticatedAdmin(cpCore) Then
                                    streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaAdminOnly." & RecordCount, genericController.encodeText(.adminOnly), .inherited))
                                End If
                                '
                                ' Developer Only
                                '
                                If cpCore.authContext.isAuthenticatedDeveloper(cpCore) Then
                                    streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaDeveloperOnly." & RecordCount, genericController.encodeText(.developerOnly), .inherited))
                                End If
                                '
                                streamRow.Add("</tr>")
                                RecordCount = RecordCount + 1
                            End With
                            '
                            ' rows are built - put the blank rows at the top
                            '
                            If Not rowValid Then
                                Call Stream.Add(streamRow.Text())
                            Else
                                Call StreamValidRows.Add(streamRow.Text())
                            End If
                        Next
                        Call Stream.Add(StreamValidRows.Text())
                        Call Stream.Add(cpCore.html.html_GetFormInputHidden("dtfaRecordCount", RecordCount))
                    End If
                    Stream.Add("</table>")
                    'Stream.Add( cpcore.htmldoc.main_GetPanelButtons(ButtonList, "Button"))
                    '
                    Call Stream.Add(cpCore.html.main_GetPanelBottom())
                    'Call Stream.Add(cpCore.main_GetFormEnd())
                    If NeedFootNote1 Then
                        Call Stream.Add("<br>*Field Inheritance is not allowed because this Content Definition has no parent.")
                    End If
                    If NeedFootNote2 Then
                        Call Stream.Add("<br>**This field can not be inherited because the Parent Content Definition does not have a field with the same name.")
                    End If
                End If
                debugController.debug_testPoint(cpCore, "ConfigureEdit, Form Done")
                If ContentID <> 0 Then
                    '
                    ' Save the content selection
                    '
                    Call Stream.Add(cpCore.html.html_GetFormInputHidden(RequestNameToolContentID, ContentID))
                Else
                    '
                    ' content tables that have edit forms to Configure
                    '
                    FormPanel = FormPanel & SpanClassAdminNormal & "Select a Content Definition to Configure its edit form<br>"
                    FormPanel = FormPanel & "<br>"
                    FormPanel = FormPanel & cpCore.html.main_GetFormInputSelect(RequestNameToolContentID, ContentID, "Content")
                    FormPanel = FormPanel & "</SPAN>"
                    Call Stream.Add(cpCore.html.main_GetPanel(FormPanel))
                End If
                '
                Call Stream.Add(cpCore.html.html_GetFormInputHidden("ReloadCDef", ReloadCDef))
                result = genericLegacyView.OpenFormTable(cpCore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpCore, ButtonList)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '
        '
        '
        Private Function GetForm_ConfigureEdit_CheckBox(ByVal Label As String, ByVal Value As String, ByVal Inherited As Boolean) As String
            GetForm_ConfigureEdit_CheckBox = "<td class=""ccPanelInput"" align=""center""><nobr>"
            If Inherited Then
                GetForm_ConfigureEdit_CheckBox = GetForm_ConfigureEdit_CheckBox & SpanClassAdminSmall & Value & "</SPAN>"
            Else
                GetForm_ConfigureEdit_CheckBox = GetForm_ConfigureEdit_CheckBox & cpCore.html.html_GetFormInputCheckBox(Label, Value)
            End If
            GetForm_ConfigureEdit_CheckBox = GetForm_ConfigureEdit_CheckBox & "</nobr></td>"
        End Function
        '
        '=============================================================================
        '   Print the manual query form
        '=============================================================================
        '
        Private Function GetForm_DbIndex() As String
            On Error GoTo ErrorTrap
            '
            Dim Count As Integer
            Dim Pointer As Integer
            Dim SQL As String
            Dim TableID As Integer
            Dim TableName As String = ""
            Dim FieldName As String
            Dim IndexName As String
            Dim DataSource As String = ""
            Dim RSSchema As DataTable
            Dim Button As String
            Dim CS As Integer
            Dim ErrorNumber As Integer
            Dim ErrorDescription As String
            Dim Rows As String(,)
            Dim RowMax As Integer
            Dim RowPointer As Integer
            Dim Copy As String = ""
            Dim TableRowEven As Boolean
            Dim TableColSpan As Integer
            Dim ButtonList As String
            '
            ButtonList = ButtonCancel & "," & ButtonSelect
            GetForm_DbIndex = GetTitle("Modify Database Indexes", "This tool adds and removes database indexes.")
            '
            ' Process Input
            '
            Button = cpCore.docProperties.getText("Button")
            TableID = cpCore.docProperties.getInteger("TableID")
            '
            ' Get Tablename and DataSource
            '
            CS = cpCore.db.csOpenRecord("Tables", TableID, , , "Name,DataSourceID")
            If cpCore.db.cs_ok(CS) Then
                TableName = cpCore.db.cs_getText(CS, "name")
                DataSource = cpCore.db.cs_getLookup(CS, "DataSourceID")
            End If
            Call cpCore.db.cs_Close(CS)
            '
            If (TableID <> 0) And (TableID = cpCore.docProperties.getInteger("previoustableid")) And (Button <> "") Then
                '
                ' Drop Indexes
                '
                Count = cpCore.docProperties.getInteger("DropCount")
                If Count > 0 Then
                    For Pointer = 0 To Count - 1
                        If cpCore.docProperties.getBoolean("DropIndex." & Pointer) Then
                            IndexName = cpCore.docProperties.getText("DropIndexName." & Pointer)
                            GetForm_DbIndex = GetForm_DbIndex & "<br>Dropping index [" & IndexName & "] from table [" & TableName & "]"
                            Call cpCore.db.deleteSqlIndex("Default", TableName, IndexName)
                        End If
                    Next
                End If
                '
                ' Add Indexes
                '
                Count = cpCore.docProperties.getInteger("AddCount")
                If Count > 0 Then
                    For Pointer = 0 To Count - 1
                        If cpCore.docProperties.getBoolean("AddIndex." & Pointer) Then
                            'IndexName = cpCore.main_GetStreamText2("AddIndexFieldName." & Pointer)
                            FieldName = cpCore.docProperties.getText("AddIndexFieldName." & Pointer)
                            IndexName = TableName & FieldName
                            GetForm_DbIndex = GetForm_DbIndex & "<br>Adding index [" & IndexName & "] to table [" & TableName & "] for field [" & FieldName & "]"
                            Call cpCore.db.createSQLIndex(DataSource, TableName, IndexName, FieldName)
                        End If
                    Next
                End If
            End If
            '
            GetForm_DbIndex = GetForm_DbIndex & cpCore.html.html_GetFormStart
            TableColSpan = 3
            GetForm_DbIndex = GetForm_DbIndex & StartTable(2, 0, 0)
            '
            ' Select Table Form
            '
            GetForm_DbIndex = GetForm_DbIndex & GetTableRow("<br><br><B>Select table to index</b>", TableColSpan, False)
            GetForm_DbIndex = GetForm_DbIndex & GetTableRow(cpCore.html.main_GetFormInputSelect("TableID", TableID, "Tables", , "Select a SQL table to start"), TableColSpan, False)
            If TableID <> 0 Then
                '
                ' Add/Drop Indexes form
                '
                GetForm_DbIndex = GetForm_DbIndex & cpCore.html.html_GetFormInputHidden("PreviousTableID", TableID)
                '
                ' Drop Indexes
                '
                GetForm_DbIndex = GetForm_DbIndex & GetTableRow("<br><br><B>Select indexes to remove</b>", TableColSpan, TableRowEven)
                RSSchema = cpCore.db.getIndexSchemaData(TableName)


                If (RSSchema.Rows.Count = 0) Then
                    '
                    ' ----- no result
                    '
                    Copy = Copy & Now() & " A schema was returned, but it contains no indexs."
                    GetForm_DbIndex = GetForm_DbIndex & GetTableRow(Copy, TableColSpan, TableRowEven)
                Else

                    Rows = cpCore.db.convertDataTabletoArray(RSSchema)
                    RowMax = UBound(Rows, 2)
                    For RowPointer = 0 To RowMax
                        IndexName = genericController.encodeText(Rows(5, RowPointer))
                        If IndexName <> "" Then
                            GetForm_DbIndex = GetForm_DbIndex & StartTableRow()
                            Copy = cpCore.html.html_GetFormInputCheckBox2("DropIndex." & RowPointer, False) _
                                    & cpCore.html.html_GetFormInputHidden("DropIndexName." & RowPointer, IndexName) _
                                    & genericController.encodeText(IndexName)
                            GetForm_DbIndex = GetForm_DbIndex & GetTableCell(Copy, , , TableRowEven)
                            GetForm_DbIndex = GetForm_DbIndex & GetTableCell(genericController.encodeText(Rows(17, RowPointer)), , , TableRowEven)
                            GetForm_DbIndex = GetForm_DbIndex & GetTableCell("&nbsp;", , , TableRowEven)
                            GetForm_DbIndex = GetForm_DbIndex & kmaEndTableRow
                            TableRowEven = Not TableRowEven
                        End If
                    Next
                    GetForm_DbIndex = GetForm_DbIndex & cpCore.html.html_GetFormInputHidden("DropCount", RowMax + 1)
                End If
                '
                ' Add Indexes
                '
                TableRowEven = False
                GetForm_DbIndex = GetForm_DbIndex & GetTableRow("<br><br><B>Select database fields to index</b>", TableColSpan, TableRowEven)
                RSSchema = cpCore.db.getColumnSchemaData(TableName)
                If (RSSchema.Rows.Count = 0) Then
                    '
                    ' ----- no result
                    '
                    Copy = Copy & Now() & " A schema was returned, but it contains no indexs."
                    GetForm_DbIndex = GetForm_DbIndex & GetTableRow(Copy, TableColSpan, TableRowEven)
                Else

                    Rows = cpCore.db.convertDataTabletoArray(RSSchema)
                    '
                    RowMax = UBound(Rows, 2)
                    For RowPointer = 0 To RowMax
                        GetForm_DbIndex = GetForm_DbIndex & StartTableRow()
                        Copy = cpCore.html.html_GetFormInputCheckBox2("AddIndex." & RowPointer, False) _
                                & cpCore.html.html_GetFormInputHidden("AddIndexFieldName." & RowPointer, Rows(3, RowPointer)) _
                                & genericController.encodeText(Rows(3, RowPointer))
                        GetForm_DbIndex = GetForm_DbIndex & GetTableCell(Copy, , , TableRowEven)
                        GetForm_DbIndex = GetForm_DbIndex & GetTableCell("&nbsp;", , , TableRowEven)
                        GetForm_DbIndex = GetForm_DbIndex & GetTableCell("&nbsp;", , , TableRowEven)
                        GetForm_DbIndex = GetForm_DbIndex & kmaEndTableRow
                        TableRowEven = Not TableRowEven
                    Next
                    GetForm_DbIndex = GetForm_DbIndex & cpCore.html.html_GetFormInputHidden("AddCount", RowMax + 1)
                End If
                '
                ' Spacers
                '
                GetForm_DbIndex = GetForm_DbIndex & StartTableRow()
                GetForm_DbIndex = GetForm_DbIndex & GetTableCell(getSpacer(300, 1), "200")
                GetForm_DbIndex = GetForm_DbIndex & GetTableCell(getSpacer(200, 1), "200")
                GetForm_DbIndex = GetForm_DbIndex & GetTableCell("&nbsp;", "100%")
                GetForm_DbIndex = GetForm_DbIndex & kmaEndTableRow
            End If
            GetForm_DbIndex = GetForm_DbIndex & kmaEndTable
            '
            ' Buttons
            '
            'GetForm_DbIndex = GetForm_DbIndex & cpCore.main_GetFormInputHidden("af", AdminFormToolDbIndex)
            GetForm_DbIndex = genericLegacyView.OpenFormTable(cpCore, ButtonList) & GetForm_DbIndex & genericLegacyView.CloseFormTable(cpCore, ButtonList)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_DbIndex", "ErrorTrap")
        End Function
        '
        '=============================================================================
        '   Print the manual query form
        '=============================================================================
        '
        Private Function GetForm_ContentDbSchema() As String
            On Error GoTo ErrorTrap
            '
            Dim CS As Integer
            Dim TableColSpan As Integer
            Dim TableEvenRow As Boolean
            Dim SQL As String
            Dim TableName As String
            Dim ButtonList As String
            '
            ButtonList = ButtonCancel
            GetForm_ContentDbSchema = GetTitle("Get Content Database Schema", "This tool displays all tables and fields required for the current Content Defintions.")
            '
            TableColSpan = 3
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & StartTable(2, 0, 0)
            SQL = "SELECT DISTINCT ccTables.Name as TableName, ccFields.Name as FieldName, ccFieldTypes.Name as FieldType" _
                    & " FROM ((ccContent LEFT JOIN ccTables ON ccContent.ContentTableID = ccTables.ID) LEFT JOIN ccFields ON ccContent.ID = ccFields.ContentID) LEFT JOIN ccFieldTypes ON ccFields.Type = ccFieldTypes.ID" _
                    & " ORDER BY ccTables.Name, ccFields.Name;"
            CS = cpCore.db.cs_openCsSql_rev("Default", SQL)
            TableName = ""
            Do While cpCore.db.cs_ok(CS)
                If TableName <> cpCore.db.cs_getText(CS, "TableName") Then
                    TableName = cpCore.db.cs_getText(CS, "TableName")
                    GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableRow("<B>" & TableName & "</b>", TableColSpan, TableEvenRow)
                End If
                GetForm_ContentDbSchema = GetForm_ContentDbSchema & StartTableRow()
                GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableCell("&nbsp;", , , TableEvenRow)
                GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableCell(cpCore.db.cs_getText(CS, "FieldName"), , , TableEvenRow)
                GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableCell(cpCore.db.cs_getText(CS, "FieldType"), , , TableEvenRow)
                GetForm_ContentDbSchema = GetForm_ContentDbSchema & kmaEndTableRow
                TableEvenRow = Not TableEvenRow
                cpCore.db.cs_goNext(CS)
            Loop
            '
            ' Field Type Definitions
            '
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableRow("<br><br><B>Field Type Definitions</b>", TableColSpan, TableEvenRow)
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableRow("Boolean - Boolean values 0 and 1 are stored in a database long integer field type", TableColSpan, TableEvenRow)
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableRow("Lookup - References to related records stored as database long integer field type", TableColSpan, TableEvenRow)
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableRow("Integer - database long integer field type", TableColSpan, TableEvenRow)
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableRow("Float - database floating point value", TableColSpan, TableEvenRow)
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableRow("Date - database DateTime field type.", TableColSpan, TableEvenRow)
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableRow("AutoIncrement - database long integer field type. Field automatically increments when a record is added.", TableColSpan, TableEvenRow)
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableRow("Text - database character field up to 255 characters.", TableColSpan, TableEvenRow)
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableRow("LongText - database character field up to 64K characters.", TableColSpan, TableEvenRow)
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableRow("TextFile - references a filename in the Content Files folder. Database character field up to 255 characters. ", TableColSpan, TableEvenRow)
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableRow("File - references a filename in the Content Files folder. Database character field up to 255 characters. ", TableColSpan, TableEvenRow)
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableRow("Redirect - This field has no database equivelent. No Database field is required.", TableColSpan, TableEvenRow)
            '
            ' Spacers
            '
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & StartTableRow()
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableCell(getSpacer(20, 1), "20")
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableCell(getSpacer(300, 1), "300")
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableCell("&nbsp;", "100%")
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & kmaEndTableRow
            GetForm_ContentDbSchema = GetForm_ContentDbSchema & kmaEndTable
            '
            'GetForm_ContentDbSchema = GetForm_ContentDbSchema & cpCore.main_GetFormInputHidden("af", AdminFormToolContentDbSchema)
            GetForm_ContentDbSchema = (genericLegacyView.OpenFormTable(cpCore, ButtonList)) & GetForm_ContentDbSchema & (genericLegacyView.CloseFormTable(cpCore, ButtonList))
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_ContentDbSchema", "ErrorTrap")
        End Function
        '
        '=============================================================================
        '   Print the manual query form
        '=============================================================================
        '
        Private Function GetForm_LogFiles() As String
            On Error GoTo ErrorTrap
            '
            Dim QueryOld As String
            Dim QueryNew As String
            Dim ButtonList As String
            '
            ButtonList = ButtonCancel
            GetForm_LogFiles = GetTitle("Log File View", "This tool displays the Contensive Log Files.")
            GetForm_LogFiles = GetForm_LogFiles & "<P></P>"
            '
            QueryOld = ".asp?"
            QueryNew = genericController.ModifyQueryString(QueryOld, RequestNameAdminForm, AdminFormToolLogFileView, True)
            GetForm_LogFiles = GetForm_LogFiles & genericController.vbReplace(GetForm_LogFiles_Details(), QueryOld, QueryNew & "&", 1, 99, vbTextCompare)
            '
            'GetForm_LogFiles = GetForm_LogFiles & cpCore.main_GetFormInputHidden("af", AdminFormToolLogFileView)
            GetForm_LogFiles = (genericLegacyView.OpenFormTable(cpCore, ButtonList)) & GetForm_LogFiles & (genericLegacyView.CloseFormTable(cpCore, ButtonList))
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_LogFiles", "ErrorTrap")
        End Function
        '
        '==============================================================================================
        '   Display a path in the Content Files with links to download and change folders
        '==============================================================================================
        '
        Private Function GetForm_LogFiles_Details() As String
            Dim StartPath As String
            Dim CurrentPath As String
            Dim SourceFolders As String
            Dim FolderSplit() As String
            Dim FolderCount As Integer
            Dim FolderPointer As Integer
            Dim LineSplit() As String
            Dim FolderLine As String
            Dim FolderName As String
            Dim ParentPath As String
            Dim Position As Integer
            Dim Filename As String
            Dim RowEven As Boolean
            Dim FileSize As String
            Dim FileDate As String
            Dim FileURL As String
            Dim CellCopy As String
            Dim QueryString As String
            '
            Const GetTableStart = "<table border=""1"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><TD><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr>" _
                    & "<td width=""23""><img src=""/ccLib/images/spacer.gif"" height=""1"" width=""23""></td>" _
                    & "<td width=""60%""><img src=""/ccLib/images/spacer.gif"" height=""1"" width=""1""></td>" _
                    & "<td width=""20%""><img src=""/ccLib/images/spacer.gif"" height=""1"" width=""1""></td>" _
                    & "<td width=""20%""><img src=""/ccLib/images/spacer.gif"" height=""1"" width=""1""></td>" _
                    & "</tr>"
            Const GetTableEnd = "</table></td></tr></table>"
            '
            Const SpacerImage = "<img src=""/ccLib/Images/spacer.gif"" width=""23"" height=""22"" border=""0"">"
            Const FolderOpenImage = "<img src=""/ccLib/Images/iconfolderopen.gif"" width=""23"" height=""22"" border=""0"">"
            Const FolderClosedImage = "<img src=""/ccLib/Images/iconfolderclosed.gif"" width=""23"" height=""22"" border=""0"">"
            '
            ' StartPath is the root - the top of the directory, it ends in the folder name (no slash)
            '
            GetForm_LogFiles_Details = ""
            StartPath = cpCore.programDataFiles.rootLocalPath & "Logs\"
            '
            ' CurrentPath is what is concatinated on to StartPath to get the current folder, it must start with a slash
            '
            CurrentPath = cpCore.docProperties.getText("SetPath")
            If CurrentPath = "" Then
                CurrentPath = "\"
            ElseIf Left(CurrentPath, 1) <> "\" Then
                CurrentPath = "\" & CurrentPath
            End If
            '
            ' Parent Folder is the path to the parent of current folder, and must start with a slash
            '
            Position = InStrRev(CurrentPath, "\")
            If Position = 1 Then
                ParentPath = "\"
            Else
                ParentPath = Mid(CurrentPath, 1, Position - 1)
            End If
            '
            '
            If cpCore.docProperties.getText("SourceFile") <> "" Then
                '
                ' Return the content of the file
                '
                Call cpCore.webServer.setResponseContentType("text/text")
                Call cpCore.html.writeAltBuffer(cpCore.appRootFiles.readFile(cpCore.docProperties.getText("SourceFile")))
                cpCore.continueProcessing = False
            Else
                GetForm_LogFiles_Details = GetForm_LogFiles_Details & GetTableStart
                '
                ' Parent Folder Link
                '
                If CurrentPath <> ParentPath Then
                    FileSize = ""
                    FileDate = ""
                    GetForm_LogFiles_Details = GetForm_LogFiles_Details & GetForm_LogFiles_Details_GetRow("<A href=""" & cpCore.webServer.requestPage & "?SetPath=" & ParentPath & """>" & FolderOpenImage & "</A>", "<A href=""" & cpCore.webServer.requestPage & "?SetPath=" & ParentPath & """>" & ParentPath & "</A>", FileSize, FileDate, RowEven)
                End If
                '
                ' Sub-Folders
                '

                SourceFolders = cpCore.appRootFiles.getFolderNameList(StartPath & CurrentPath)
                If SourceFolders <> "" Then
                    FolderSplit = Split(SourceFolders, vbCrLf)
                    FolderCount = UBound(FolderSplit) + 1
                    For FolderPointer = 0 To FolderCount - 1
                        FolderLine = FolderSplit(FolderPointer)
                        If FolderLine <> "" Then
                            LineSplit = Split(FolderLine, ",")
                            FolderName = LineSplit(0)
                            FileSize = LineSplit(1)
                            FileDate = LineSplit(2)
                            GetForm_LogFiles_Details = GetForm_LogFiles_Details & GetForm_LogFiles_Details_GetRow("<A href=""" & cpCore.webServer.requestPage & "?SetPath=" & CurrentPath & "\" & FolderName & """>" & FolderClosedImage & "</A>", "<A href=""" & cpCore.webServer.requestPage & "?SetPath=" & CurrentPath & "\" & FolderName & """>" & FolderName & "</A>", FileSize, FileDate, RowEven)
                        End If
                    Next
                End If
                '
                ' Files
                '
                SourceFolders = cpCore.appRootFiles.convertFileINfoArrayToParseString(cpCore.appRootFiles.getFileList(StartPath & CurrentPath))
                If SourceFolders = "" Then
                    FileSize = ""
                    FileDate = ""
                    GetForm_LogFiles_Details = GetForm_LogFiles_Details & GetForm_LogFiles_Details_GetRow(SpacerImage, "no files were found in this folder", FileSize, FileDate, RowEven)
                Else
                    FolderSplit = Split(SourceFolders, vbCrLf)
                    FolderCount = UBound(FolderSplit) + 1
                    For FolderPointer = 0 To FolderCount - 1
                        FolderLine = FolderSplit(FolderPointer)
                        If FolderLine <> "" Then
                            LineSplit = Split(FolderLine, ",")
                            Filename = LineSplit(0)
                            FileSize = LineSplit(5)
                            FileDate = LineSplit(3)
                            FileURL = StartPath & CurrentPath & "\" & Filename
                            QueryString = cpCore.doc.refreshQueryString
                            QueryString = genericController.ModifyQueryString(QueryString, RequestNameAdminForm, CStr(AdminFormTool), True)
                            QueryString = genericController.ModifyQueryString(QueryString, "at", AdminFormToolLogFileView, True)
                            QueryString = genericController.ModifyQueryString(QueryString, "SourceFile", FileURL, True)
                            CellCopy = "<A href=""" & cpCore.webServer.requestPath & "?" & QueryString & """ target=""_blank"">" & Filename & "</A>"
                            GetForm_LogFiles_Details = GetForm_LogFiles_Details & GetForm_LogFiles_Details_GetRow(SpacerImage, CellCopy, FileSize, FileDate, RowEven)
                        End If
                    Next
                End If
                '
                GetForm_LogFiles_Details = GetForm_LogFiles_Details & GetTableEnd
            End If
            '
        End Function
        '
        '=============================================================================
        '   Table Rows
        '=============================================================================
        '
        Function GetForm_LogFiles_Details_GetRow(ByVal Cell0 As String, ByVal Cell1 As String, ByVal Cell2 As String, ByVal Cell3 As String, ByVal RowEven As Boolean) As String
            '
            Dim ClassString As String
            '
            If genericController.EncodeBoolean(RowEven) Then
                RowEven = False
                ClassString = " class=""ccPanelRowEven"" "
            Else
                RowEven = True
                ClassString = " class=""ccPanelRowOdd"" "
            End If
            '
            Cell0 = genericController.encodeText(Cell0)
            If Cell0 = "" Then
                Cell0 = "&nbsp;"
            End If
            '
            Cell1 = genericController.encodeText(Cell1)
            '
            If Cell1 = "" Then
                GetForm_LogFiles_Details_GetRow = "<tr><TD" & ClassString & " Colspan=""4"">" & Cell0 & "</td></tr>"
            Else
                GetForm_LogFiles_Details_GetRow = "<tr><TD" & ClassString & ">" & Cell0 & "</td><TD" & ClassString & ">" & Cell1 & "</td><td align=right " & ClassString & ">" & Cell2 & "</td><td align=right " & ClassString & ">" & Cell3 & "</td></tr>"
            End If
            '
        End Function
        '
        '=============================================================================
        '   Print the manual query form
        '=============================================================================
        '
        Private Function GetForm_LoadCDef() As String
            Dim result As String = ""
            Try
                '
                Dim Stream As New stringBuilderLegacyController
                Dim ButtonList As String
                '
                ButtonList = ButtonCancel & "," & ButtonSaveandInvalidateCache
                Stream.Add(GetTitle("Load Content Definitions", "This tool reloads the content definitions. This is necessary when changes are made to the ccContent or ccFields tables outside Contensive. The site will be blocked during the load."))
                '
                If (cpCore.docProperties.getText("button")) <> ButtonSaveandInvalidateCache Then
                    '
                    ' First pass, initialize
                    '
                Else
                    '
                    ' Restart
                    '
                    Call Stream.Add("<br>Loading Content Definitions...")
                    cpCore.cache.invalidateAll()
                    cpCore.metaData.clear()
                    Call Stream.Add("<br>Content Definitions loaded")
                End If
                '
                ' Display form
                '
                Call Stream.Add(SpanClassAdminNormal)
                Call Stream.Add("<br>")
                Call Stream.Add("</span>")
                '
                result = genericLegacyView.OpenFormTable(cpCore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpCore, ButtonList)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return result
        End Function
        '
        '=============================================================================
        '   Print the manual query form
        '=============================================================================
        '
        Private Function GetForm_Restart() As String
            On Error GoTo ErrorTrap
            '
            Dim Stream As New stringBuilderLegacyController
            Dim ButtonList As String
            ' Dim runAtServer As runAtServerClass
            '
            ButtonList = ButtonCancel & "," & ButtonRestartContensiveApplication
            Stream.Add(GetTitle("Load Content Definitions", "This tool stops and restarts the Contensive Application controlling this website. If you restart, the site will be unavailable for up to a minute while the site is stopped and restarted. If the site does not restart, you will need to contact a site administrator to have the Contensive Server restarted."))
            '
            If Not cpCore.authContext.isAuthenticatedAdmin(cpCore) Then
                '
                GetForm_Restart = "<P>You must be an administrator to use this tool.</P>"
                '
            ElseIf (cpCore.docProperties.getText("button")) <> ButtonRestartContensiveApplication Then
                '
                ' First pass, initialize
                '
            Else
                '
                ' Restart
                '
                logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "Restarting Contensive", "dll", "ToolsClass", "GetForm_Restart", 0, "dll", "Warning: member " & cpCore.authContext.user.Name & " (" & cpCore.authContext.user.id & ") restarted using the Restart tool", False, True, cpCore.webServer.requestUrl, "", "")
                'runAtServer = New runAtServerClass(cpCore)
                Call cpCore.webServer.redirect("/ccLib/Popup/WaitForIISReset.htm")
                Call Threading.Thread.Sleep(2000)
                '
                '
                '
                Throw New NotImplementedException("GetForm_Restart")
                'hint = hint & ",035"
                Dim taskScheduler As New taskSchedulerController()
                Dim cmdDetail As New cmdDetailClass
                cmdDetail.addonId = 0
                cmdDetail.addonName = "commandRestart"
                cmdDetail.docProperties = taskScheduler.convertAddonArgumentstoDocPropertiesList(cpCore, "")
                Call taskScheduler.addTaskToQueue(cpCore, taskQueueCommandEnumModule.runAddon, cmdDetail, False)
                '
                'Call runAtServer.executeCmd("Restart", "appname=" & cpCore.app.config.name)
            End If
            '
            ' Display form
            '
            Call Stream.Add(SpanClassAdminNormal)
            Call Stream.Add("<br>")
            ''Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolRestart)
            Call Stream.Add("</SPAN>")
            '
            GetForm_Restart = genericLegacyView.OpenFormTable(cpCore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpCore, ButtonList)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_Restart", "ErrorTrap")
        End Function
        '
        '
        '
        Private Function GetCDef(ByVal ContentName As String) As cdefModel
            Return cpCore.metaData.getCdef(ContentName)
        End Function



        'Function CloseFormTable(ByVal ButtonList As String) As String
        '    If ButtonList <> "" Then
        '        CloseFormTable = "</td></tr></TABLE>" & cpcore.htmldoc.main_GetPanelButtons(ButtonList, "Button") & "</form>"
        '    Else
        '        CloseFormTable = "</td></tr></TABLE></form>"
        '    End If
        'End Function
        ''
        ''
        ''
        'Function genericLegacyView.OpenFormTable(cpcore,ByVal ButtonList As String) As String
        '    Dim result As String = ""
        '    Try
        '        OpenFormTable = cpCore.htmldoc.html_GetFormStart()
        '        If ButtonList <> "" Then
        '            OpenFormTable = OpenFormTable & cpcore.htmldoc.main_GetPanelButtons(ButtonList, "Button")
        '        End If
        '        OpenFormTable = OpenFormTable & "<table border=""0"" cellpadding=""10"" cellspacing=""0"" width=""100%""><tr><TD>"
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex)
        '    End Try
        '    Return result
        'End Function

        '
        '=============================================================================
        '   Import the htm and html files in the FileRootPath and below into Page Templates
        '       FileRootPath is the path to the root of the site
        '       AppPath is the path to the folder currently
        '=============================================================================
        '
        Private Function GetForm_LoadTemplates() As String
            On Error GoTo ErrorTrap
            '
            Dim Stream As New stringBuilderLegacyController
            Dim ButtonList As String
            '
            Dim Folders() As String
            Dim FolderList As String
            Dim FolderDetailString As String
            Dim FolderDetails() As String
            Dim FolderName As String
            '
            Dim FileList As String
            Dim Files() As String
            Dim Ptr As Integer
            Dim FilePath As String
            Dim FileDetailString As String
            Dim FileDetails() As String
            Dim Filename As String
            Dim PageSource As String
            Dim CS As Integer
            Dim Link As String
            Dim TemplateName As String
            '
            'dim buildversion As String
            Dim AppPath As String
            Dim FileRootPath As String
            '
            Dim AllowBodyHTML As Boolean
            Dim AllowScriptLink As Boolean
            Dim AllowImageImport As Boolean
            Dim AllowStyleImport As Boolean
            '
            AllowBodyHTML = cpCore.docProperties.getBoolean("AllowBodyHTML")
            AllowScriptLink = cpCore.docProperties.getBoolean("AllowScriptLink")
            AllowImageImport = cpCore.docProperties.getBoolean("AllowImageImport")
            AllowStyleImport = cpCore.docProperties.getBoolean("AllowStyleImport")
            '
            Stream.Add(GetTitle("Load Templates", "This tool creates template records from the HTML files in the root folder of the site."))
            '
            If (cpCore.docProperties.getText("button")) <> ButtonImportTemplates Then
                '
                ' First pass, initialize
                '
            Else
                '
                ' Restart
                '
                Call Stream.Add("<br>Loading Templates...")
                'Call Stream.Add(ImportTemplates(cpCore.appRootFiles.rootLocalPath, "", AllowBodyHTML, AllowScriptLink, AllowImageImport, AllowStyleImport))
                Call Stream.Add("<br>Templates loaded")
            End If
            '
            ' Display form
            '
            Call Stream.Add(SpanClassAdminNormal)
            Call Stream.Add("<br>")
            'Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolLoadTemplates)
            Stream.Add("<br>" & cpCore.html.html_GetFormInputCheckBox2("AllowBodyHTML", AllowBodyHTML) & " Update/Import Soft Templates from the Body of .HTM and .HTML files")
            Stream.Add("<br>" & cpCore.html.html_GetFormInputCheckBox2("AllowScriptLink", AllowScriptLink) & " Update/Import Hard Templates with links to .ASP and .ASPX scripts")
            Stream.Add("<br>" & cpCore.html.html_GetFormInputCheckBox2("AllowImageImport", AllowImageImport) & " Update/Import image links (.GIF,.JPG,.PDF ) into the resource library")
            Stream.Add("<br>" & cpCore.html.html_GetFormInputCheckBox2("AllowStyleImport", AllowStyleImport) & " Import style sheets (.CSS) to Dynamic Styles")
            Call Stream.Add("</SPAN>")
            '
            ButtonList = ButtonCancel & "," & ButtonImportTemplates
            GetForm_LoadTemplates = genericLegacyView.OpenFormTable(cpCore, ButtonList) & Stream.Text & genericLegacyView.CloseFormTable(cpCore, ButtonList)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_LoadTemplates", "ErrorTrap")
        End Function
        ''
        ''=============================================================================
        ''   Import the htm and html files in the FileRootPath and below into Page Templates
        ''       FileRootPath is the path to the root of the site
        ''       AppPath is the path to the folder currently
        ''=============================================================================
        ''
        'Private Function ImportTemplates(ByVal FileRootPath As String, ByVal AppPath As String, ByVal AllowBodyHTML As Boolean, ByVal AllowScriptLink As Boolean, ByVal AllowImageImport As Boolean, ByVal AllowStyleImport As Boolean) As String
        '    Dim result As String = ""
        '    Try
        '        '
        '        Dim Stream As New stringBuilderLegacyController
        '        Dim Folders() As String
        '        Dim FolderList As String
        '        Dim FolderDetailString As String
        '        Dim FolderDetails() As String
        '        Dim FolderName As String
        '        Dim FileList As String
        '        Dim Files() As String
        '        Dim Ptr As Integer
        '        Dim FileDetailString As String
        '        Dim FileDetails() As String
        '        Dim Filename As String
        '        Dim PageSource As String
        '        Dim CS As Integer
        '        Dim Link As String
        '        Dim TemplateName As String
        '        Dim Copy As String
        '        '
        '        FileList = cpCore.appRootFiles.convertFileINfoArrayToParseString(cpCore.appRootFiles.getFileList(FileRootPath & AppPath))
        '        Files = Split(FileList, vbCrLf)
        '        For Ptr = 0 To UBound(Files)
        '            FileDetailString = Files(Ptr)
        '            FileDetails = Split(FileDetailString, ",")
        '            Filename = FileDetails(0)
        '            Link = genericController.vbReplace(AppPath & Filename, "\", "/")
        '            If AllowScriptLink And (InStr(1, Filename, ".asp", vbTextCompare) <> 0) And (Mid(Filename, 1, 1) <> "_") Then
        '                '
        '                result = result & "<br>Create Hard Template for script page [" & Link & "]"
        '                '
        '                Link = genericController.vbReplace(AppPath & Filename, "\", "/")
        '                TemplateName = genericController.vbReplace(Link, "/", "-")
        '                If genericController.vbInstr(1, TemplateName, ".") <> 0 Then
        '                    TemplateName = Mid(TemplateName, 1, genericController.vbInstr(1, TemplateName, ".") - 1)

        '                End If
        '                '
        '                CS = cpCore.db.cs_open("Page Templates", "Link=" & cpCore.db.encodeSQLText(Link))
        '                If Not cpCore.db.cs_ok(CS) Then
        '                    Call cpCore.db.cs_Close(CS)
        '                    CS = cpCore.db.cs_insertRecord("Page Templates")
        '                    Call cpCore.db.cs_set(CS, "Link", Link)
        '                End If
        '                If cpCore.db.cs_ok(CS) Then
        '                    Call cpCore.db.cs_set(CS, "name", TemplateName)
        '                End If
        '                Call cpCore.db.cs_Close(CS)
        '            ElseIf AllowBodyHTML And (InStr(1, Filename, ".htm", vbTextCompare) <> 0) And (Mid(Filename, 1, 1) <> "_") Then
        '                '
        '                ' HTML, import body
        '                '
        '                PageSource = cpCore.appRootFiles.readFile(Filename)
        '                Call cpCore.main_EncodePage_SplitBody(PageSource, PageSource, "", "")
        '                Link = genericController.vbReplace(AppPath & Filename, "\", "/")
        '                TemplateName = genericController.vbReplace(Link, "/", "-")
        '                If genericController.vbInstr(1, TemplateName, ".") <> 0 Then
        '                    TemplateName = Mid(TemplateName, 1, genericController.vbInstr(1, TemplateName, ".") - 1)

        '                End If
        '                '
        '                result = result & "<br>Create Soft Template from source [" & Link & "]"
        '                '
        '                CS = cpCore.db.cs_open("Page Templates", "Source=" & cpCore.db.encodeSQLText(Link))
        '                If Not cpCore.db.cs_ok(CS) Then
        '                    Call cpCore.db.cs_Close(CS)
        '                    CS = cpCore.db.cs_insertRecord("Page Templates")
        '                    Call cpCore.db.cs_set(CS, "Source", Link)
        '                    Call cpCore.db.cs_set(CS, "name", TemplateName)
        '                End If
        '                If cpCore.db.cs_ok(CS) Then
        '                    Call cpCore.db.cs_set(CS, "Link", "")
        '                    Call cpCore.db.cs_set(CS, "bodyhtml", PageSource)
        '                End If
        '                Call cpCore.db.cs_Close(CS)
        '                '
        '            ElseIf AllowImageImport And (InStr(1, Filename, ".gif", vbTextCompare) <> 0) And (Mid(Filename, 1, 1) <> "_") Then
        '                '
        '                ' Import GIF images
        '                '
        '                result = result & "<br>Import Image Link to Resource Library [" & Link & "]"
        '                '
        '            ElseIf AllowStyleImport And (InStr(1, Filename, ".css", vbTextCompare) <> 0) And (Mid(Filename, 1, 1) <> "_") Then
        '                '
        '                ' Import CSS to Dynamic styles
        '                '
        '                result = result & "<br>Import style sheet to Dynamic Styles [" & Link & "]"
        '                '
        '                Dim DynamicFilename As String
        '                DynamicFilename = "templates\styles.css"
        '                Copy = cpCore.appRootFiles.readFile(DynamicFilename)
        '                Copy = RemoveStyleTags(Copy)
        '                Copy = Copy _
        '                    & vbCrLf _
        '                    & vbCrLf & "/* Import of " & FileRootPath & AppPath & Filename & "*/" _
        '                    & vbCrLf _
        '                    & vbCrLf
        '                Copy = Copy & RemoveStyleTags(cpCore.appRootFiles.readFile(Filename))
        '                Call cpCore.appRootFiles.saveFile(DynamicFilename, Copy)
        '            End If
        '        Next
        '        '
        '        ' Now process all subfolders
        '        '
        '        FolderList = cpCore.getFolderNameList(FileRootPath & AppPath)
        '        If FolderList <> "" Then
        '            Folders = Split(FolderList, vbCrLf)
        '            For Ptr = 0 To UBound(Folders)
        '                FolderDetailString = Folders(Ptr)
        '                If FolderDetailString <> "" Then
        '                    FolderDetails = Split(FolderDetailString, ",")
        '                    FolderName = FolderDetails(0)
        '                    If Mid(FolderName, 1, 1) <> "_" Then
        '                        result = result & ImportTemplates(FileRootPath, AppPath & FolderName & "\", AllowBodyHTML, AllowScriptLink, AllowImageImport, AllowStyleImport)
        '                    End If
        '                End If
        '            Next
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '    End Try
        '    Return result
        'End Function
        '        '
        '        '
        '        '
        '        Private Function LoadCDef(ByVal ContentName As String) As coreMetaDataClass.CDefClass
        '            On Error GoTo ErrorTrap
        '            '
        '            Dim SQL As String
        '            Dim CS As Integer
        '            Dim ContentID As Integer
        '            Dim CSContent As Integer
        '            Dim ParentContentName As String
        '            Dim ParentID As Integer
        '            '
        '            CSContent = cpCore.db.cs_open("Content", "name=" & cpCore.db.encodeSQLText(ContentName))
        '            If cpCore.db.cs_ok(CSContent) Then
        '                '
        '                ' Start with parent CDef
        '                '
        '                ParentID = cpCore.db.cs_getInteger(CSContent, "parentID")
        '                If ParentID <> 0 Then
        '                    ParentContentName = cpCore.metaData.getContentNameByID(ParentID)
        '                    If ParentContentName <> "" Then
        '                        LoadCDef = LoadCDef(ParentContentName)
        '                    End If
        '                End If
        '                '
        '                ' Add this definition on it
        '                '
        '                With LoadCDef
        '                    CS = cpCore.db.cs_open("Content Fields", "contentid=" & ContentID)
        '                    Do While cpCore.db.cs_ok(CS)
        '                        Select Case genericController.vbUCase(cpCore.db.cs_getText(CS, "name"))
        '                            Case "NAME"
        '                                .Name = ""
        '                        End Select
        '                        Call cpCore.db.cs_goNext(CS)
        '                    Loop
        '                    Call cpCore.db.cs_Close(CS)
        '                End With
        '            End If
        '            Call cpCore.db.cs_Close(CSContent)
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw (New ApplicationException("Unexpected exception"))'  Call handleLegacyClassErrors1("ImportTemplates", "ErrorTrap")
        '        End Function
        '        '
        '        '=================================================================================
        '        '
        '        ' From Admin code in ccWeb42
        '        '   Put it here so the same data will be used for both the admin site and the tool page
        '        '   change this so it reads from the CDef, not the database
        '        '
        '        '
        '        '=================================================================================
        '        '
        '        Public Sub GetDbCDef_SetAdminColumns(ByRef CDef As appServices_metaDataClass.CDefClass)
        '            On Error GoTo ErrorTrap
        '            '
        '            Dim DestPtr As Integer
        '            Dim UcaseFieldName As String
        '            Dim DefaultFieldPointer As Integer
        '            ' converted array to dictionary - Dim FieldPointer As Integer
        '            Dim FieldActive As Boolean
        '            Dim FieldAuthorable As Boolean
        '            Dim FieldWidth As Integer
        '            Dim FieldWidthTotal As Integer
        '            'Dim IndexColumn() As Integer
        '            Dim adminColumn As appServices_metaDataClass.CDefAdminColumnClass
        '            '
        '            With CDef
        '                If .Id > 0 Then
        '                    For Each keyValuePair As KeyValuePair(Of String, appServices_metaDataClass.CDefFieldClass) In .fields
        '                        Dim field As appServices_metaDataClass.CDefFieldClass = keyValuePair.Value
        '                        FieldActive = field.active
        '                        FieldWidth = genericController.EncodeInteger(field.IndexWidth)
        '                        If FieldActive And (FieldWidth > 0) Then
        '                            adminColumn = New appServices_metaDataClass.CDefAdminColumnClass
        '                            FieldWidthTotal = FieldWidthTotal + FieldWidth
        '                            'ReDim Preserve IndexColumn(.adminColumns.Count)
        '                            DestPtr = -1
        '                            If .adminColumns.Count > 0 Then
        '                                '
        '                                ' Sort the columns to make room
        '                                '
        '                                For DestPtr = .adminColumns.Count - 1 To 0 Step -1
        '                                    If field.IndexColumn >= IndexColumn(DestPtr) Then
        '                                        '
        '                                        ' Put the new entry into Destination+1
        '                                        '
        '                                        Exit For
        '                                    Else
        '                                        '
        '                                        ' move entry destination->destination+1
        '                                        '
        '                                        IndexColumn(DestPtr + 1) = IndexColumn(DestPtr)
        '                                        adminColumn.Name = .adminColumns(DestPtr).Name
        '                                        adminColumn.SortDirection = .adminColumns(DestPtr).SortDirection
        '                                        adminColumn.SortPriority = .adminColumns(DestPtr).SortPriority
        '                                        adminColumn.Width = .adminColumns(DestPtr).Width
        '                                    End If
        '                                Next
        '                            End If
        '                            IndexColumn(DestPtr + 1) = field.IndexColumn
        '                            adminColumn.Name = field.Name
        '                            adminColumn.SortDirection = field.IndexSortDirection
        '                            adminColumn.SortPriority = genericController.EncodeInteger(field.IndexSortOrder)
        '                            adminColumn.Width = FieldWidth
        '                            .adminColumns.Add(adminColumn)
        '                        End If
        '                    Next
        '                    If .adminColumns.Count = 0 Then
        '                        '
        '                        ' Force the Name field as the only column
        '                        '
        '                        adminColumn = New appServices_metaDataClass.CDefAdminColumnClass
        '                        With adminColumn
        '                            .Name = "Name"
        '                            .SortDirection = 1
        '                            .SortPriority = 1
        '                            .Width = 100
        '                            FieldWidthTotal = FieldWidthTotal + .Width
        '                        End With
        '                        .adminColumns.Add(adminColumn)
        '                    End If
        '                    '
        '                    ' Normalize the column widths
        '                    '
        '                    For FieldPointer = 0 To .adminColumns.Count - 1
        '                        With .adminColumns(FieldPointer)
        '                            .Width = 100 * (.Width / FieldWidthTotal)
        '                        End With
        '                    Next
        '                End If
        '            End With
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw (New ApplicationException("Unexpected exception"))'  Call handleLegacyClassErrors1("GetDbCDef_SetAdminColumns", "ErrorTrap")
        '        End Sub
        '
        '=============================================================================
        '   Print the manual query form
        '=============================================================================
        '
        Private Function GetForm_ContentFileManager() As String
            On Error GoTo ErrorTrap
            '
            'Dim QueryOld As String
            'Dim QueryNew As String
            Dim ButtonList As String
            'Dim FileView As New FileViewClass
            Dim IsContentManager As Boolean
            Dim Adminui As New adminUIController(cpCore)
            Dim Content As String
            Dim Description As String
            Dim InstanceOptionString As String
            '
            ButtonList = ButtonApply & "," & ButtonCancel
            '
            InstanceOptionString = "AdminLayout=1&filesystem=content files"
            'InstanceOptionString = cpCore.main_GetMemberProperty("Addon [File Manager] Options", "")
            Content = cpCore.addon.execute_legacy1(0, "{B966103C-DBF4-4655-856A-3D204DEF6B21}", InstanceOptionString, Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, "", 0, "", "-2", -1)
            'If Content = "" Then
            '    IsContentManager = cpcore.authContext.user.main_IsContentManager()
            '    Content = FileView.GetContentFileView2( "", IsContentManager, IsContentManager, True, False, True, False)
            'End If
            Description = "Manage files and folders within the virtual content file area."
            GetForm_ContentFileManager = Adminui.GetBody("Content File Manager", ButtonList, "", False, False, Description, "", 0, Content)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_ContentFileManager", "ErrorTrap")
        End Function
        '
        '=============================================================================
        '   Print the manual query form
        '=============================================================================
        '
        Private Function GetForm_WebsiteFileManager() As String
            On Error GoTo ErrorTrap
            '
            'Dim QueryOld As String
            'Dim QueryNew As String
            Dim ButtonList As String
            'Dim FileView As New FileViewClass
            Dim IsContentManager As Boolean
            Dim Adminui As New adminUIController(cpCore)
            Dim Content As String
            Dim Description As String
            Dim InstanceOptionString As String
            '
            ButtonList = ButtonApply & "," & ButtonCancel
            '
            InstanceOptionString = "AdminLayout=1&filesystem=website files"
            'InstanceOptionString = cpCore.main_GetMemberProperty("Addon [File Manager] Options", "")
            Content = cpCore.addon.execute_legacy1(0, "{B966103C-DBF4-4655-856A-3D204DEF6B21}", InstanceOptionString, Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, "", 0, "", "-2", -1)
            'If Content = "" Then
            '    IsContentManager = cpcore.authContext.user.main_IsContentManager()
            '    Content = FileView.GetContentFileView2( "", IsContentManager, IsContentManager, True, False, True, False)
            'End If
            Description = "Manage files and folders within the Website's file area."
            GetForm_WebsiteFileManager = Adminui.GetBody("Website File Manager", ButtonList, "", False, False, Description, "", 0, Content)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_WebsiteFileManager", "ErrorTrap")
        End Function
        '
        '=============================================================================
        ' Find and Replace launch tool
        '=============================================================================
        '
        Private Function GetForm_FindAndReplace() As String
            On Error GoTo ErrorTrap
            '
            Dim IsDeveloper As Boolean
            Dim QS As String
            Dim RecordName As String
            Dim RowCnt As Integer
            Dim TopHalf As String = ""
            Dim BottomHalf As String = ""
            Dim RowPtr As Integer
            Dim RecordID As Integer
            Dim CS As Integer
            Dim Stream As New stringBuilderLegacyController
            ' Dim runAtServer As New runAtServerClass(cpCore)
            Dim CDefList As String = ""
            Dim FindText As String = ""
            Dim ReplaceText As String = ""
            Dim Button As String
            Dim ReplaceRows As Integer
            Dim FindRows As Integer
            Dim lcName As String
            '
            Stream.Add(GetTitle("Find and Replace", "This tool runs a find and replace operation on content throughout the site."))
            '
            ' Process the form
            '
            Button = cpCore.docProperties.getText("button")
            '
            IsDeveloper = cpCore.authContext.isAuthenticatedDeveloper(cpCore)
            If Button = ButtonFindAndReplace Then
                RowCnt = cpCore.docProperties.getInteger("CDefRowCnt")
                If RowCnt > 0 Then
                    For RowPtr = 0 To RowCnt - 1
                        If cpCore.docProperties.getBoolean("Cdef" & RowPtr) Then
                            lcName = genericController.vbLCase(cpCore.docProperties.getText("CDefName" & RowPtr))
                            If IsDeveloper Or (lcName = "page content") Or (lcName = "copy content") Or (lcName = "page templates") Then
                                CDefList = CDefList & "," & lcName
                            End If
                        End If
                    Next
                    If CDefList <> "" Then
                        CDefList = Mid(CDefList, 2)
                    End If
                    'CDefList = cpCore.main_GetStreamText2("CDefList")
                    FindText = cpCore.docProperties.getText("FindText")
                    ReplaceText = cpCore.docProperties.getText("ReplaceText")
                    'runAtServer.ipAddress = "127.0.0.1"
                    'runAtServer.port = "4531"
                    QS = "app=" & encodeNvaArgument(cpCore.serverConfig.appConfig.name) & "&FindText=" & encodeNvaArgument(FindText) & "&ReplaceText=" & encodeNvaArgument(ReplaceText) & "&CDefNameList=" & encodeNvaArgument(CDefList)

                    Throw New NotImplementedException("GetForm_FindAndReplace")
                    Dim taskScheduler As New taskSchedulerController()
                    Dim cmdDetail As New cmdDetailClass
                    cmdDetail.addonId = 0
                    cmdDetail.addonName = "GetForm_FindAndReplace"
                    cmdDetail.docProperties = taskScheduler.convertAddonArgumentstoDocPropertiesList(cpCore, QS)
                    Call taskScheduler.addTaskToQueue(cpCore, taskQueueCommandEnumModule.runAddon, cmdDetail, False)


                    '                    Call runAtServer.executeCmd("FindAndReplace", QS)
                    Call Stream.Add("Find and Replace has been requested for content definitions [" & CDefList & "], finding [" & FindText & "] and replacing with [" & ReplaceText & "]")
                End If
            Else
                CDefList = "Page Content,Copy Content,Page Templates"
                FindText = ""
                ReplaceText = ""
            End If
            '
            ' Display form
            '
            FindRows = cpCore.docProperties.getInteger("SQLRows")
            If FindRows = 0 Then
                FindRows = cpCore.userProperty.getInteger("FindAndReplaceFindRows", 1)
            Else
                Call cpCore.userProperty.setProperty("FindAndReplaceFindRows", CStr(FindRows))
            End If
            ReplaceRows = cpCore.docProperties.getInteger("ReplaceRows")
            If ReplaceRows = 0 Then
                ReplaceRows = cpCore.userProperty.getInteger("FindAndReplaceReplaceRows", 1)
            Else
                Call cpCore.userProperty.setProperty("FindAndReplaceReplaceRows", CStr(ReplaceRows))
            End If
            '
            Call Stream.Add("<div>Find</div>")
            Call Stream.Add("<TEXTAREA NAME=""FindText"" ROWS=""" & FindRows & """ ID=""FindText"" STYLE=""width: 800px;"">" & FindText & "</TEXTAREA>")
            Call Stream.Add("&nbsp;<INPUT TYPE=""Text"" TabIndex=-1 NAME=""FindTextRows"" SIZE=""3"" VALUE=""" & FindRows & """ ID=""""  onchange=""FindText.rows=FindTextRows.value; return true""> Rows")
            Call Stream.Add("<br><br>")
            '
            Call Stream.Add("<div>Replace it with</div>")
            Call Stream.Add("<TEXTAREA NAME=""ReplaceText"" ROWS=""" & ReplaceRows & """ ID=""ReplaceText"" STYLE=""width: 800px;"">" & ReplaceText & "</TEXTAREA>")
            Call Stream.Add("&nbsp;<INPUT TYPE=""Text"" TabIndex=-1 NAME=""ReplaceTextRows"" SIZE=""3"" VALUE=""" & ReplaceRows & """ ID=""""  onchange=""ReplaceText.rows=ReplaceTextRows.value; return true""> Rows")
            Call Stream.Add("<br><br>")
            '
            CS = cpCore.db.cs_open("Content")
            Do While cpCore.db.cs_ok(CS)
                RecordName = cpCore.db.cs_getText(CS, "Name")
                lcName = genericController.vbLCase(RecordName)
                If IsDeveloper Or (lcName = "page content") Or (lcName = "copy content") Or (lcName = "page templates") Then
                    RecordID = cpCore.db.cs_getInteger(CS, "ID")
                    If genericController.vbInstr(1, "," & CDefList & ",", "," & RecordName & ",") <> 0 Then
                        TopHalf = TopHalf & "<div>" & cpCore.html.html_GetFormInputCheckBox2("Cdef" & RowPtr, True) & cpCore.html.html_GetFormInputHidden("CDefName" & RowPtr, RecordName) & "&nbsp;" & cpCore.db.cs_getText(CS, "Name") & "</div>"
                    Else
                        BottomHalf = BottomHalf & "<div>" & cpCore.html.html_GetFormInputCheckBox2("Cdef" & RowPtr, False) & cpCore.html.html_GetFormInputHidden("CDefName" & RowPtr, RecordName) & "&nbsp;" & cpCore.db.cs_getText(CS, "Name") & "</div>"
                    End If
                End If
                Call cpCore.db.cs_goNext(CS)
                RowPtr = RowPtr + 1
            Loop
            Call cpCore.db.cs_Close(CS)
            Stream.Add(TopHalf & BottomHalf & cpCore.html.html_GetFormInputHidden("CDefRowCnt", RowPtr))
            '
            GetForm_FindAndReplace = genericLegacyView.OpenFormTable(cpCore, ButtonCancel & "," & ButtonFindAndReplace) & Stream.Text & genericLegacyView.CloseFormTable(cpCore, ButtonCancel & "," & ButtonFindAndReplace)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_FindAndReplace", "ErrorTrap")
        End Function
        '
        '=============================================================================
        ' GUID Tools
        '=============================================================================
        '
        Private Function GetForm_IISReset() As String
            On Error GoTo ErrorTrap
            '
            Dim Button As String
            'Dim GUIDGenerator As guidClass
            Dim s As stringBuilderLegacyController
            'Dim runAtServer As runAtServerClass
            '
            s = New stringBuilderLegacyController
            s.Add(GetTitle("IIS Reset", "Reset the webserver."))
            '
            ' Process the form
            '
            Button = cpCore.docProperties.getText("button")
            '
            If Button = ButtonIISReset Then
                '
                '
                '
                logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "Resetting IIS", "dll", "ToolsClass", "GetForm_IISReset", 0, "dll", "Warning: member " & cpCore.authContext.user.Name & " (" & cpCore.authContext.user.id & ") executed an IISReset using the IISReset tool", False, True, cpCore.webServer.requestUrl, "", "")
                'runAtServer = New runAtServerClass(cpCore)
                Call cpCore.webServer.redirect("/ccLib/Popup/WaitForIISReset.htm")
                Call Threading.Thread.Sleep(2000)



                Throw New NotImplementedException("GetForm_IISReset")
                Dim taskScheduler As New taskSchedulerController()
                Dim cmdDetail As New cmdDetailClass
                cmdDetail.addonId = 0
                cmdDetail.addonName = "GetForm_IISReset"
                cmdDetail.docProperties = taskScheduler.convertAddonArgumentstoDocPropertiesList(cpCore, "")
                Call taskScheduler.addTaskToQueue(cpCore, taskQueueCommandEnumModule.runAddon, cmdDetail, False)


                ' Call runAtServer.executeCmd("IISReset", "")
            End If
            '
            ' Display form
            '
            GetForm_IISReset = genericLegacyView.OpenFormTable(cpCore, ButtonCancel & "," & ButtonIISReset) & s.Text & genericLegacyView.CloseFormTable(cpCore, ButtonCancel & "," & ButtonIISReset)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_IISReset", "ErrorTrap")
        End Function
        '
        '=============================================================================
        ' GUID Tools
        '=============================================================================
        '
        Private Function GetForm_CreateGUID() As String
            On Error GoTo ErrorTrap
            '
            Dim Button As String
            ''Dim GUIDGenerator As guidClass
            Dim s As stringBuilderLegacyController
            '
            s = New stringBuilderLegacyController
            s.Add(GetTitle("Create GUID", "Use this tool to create a GUID. This is useful when creating new Addons."))
            '
            ' Process the form
            '
            Button = cpCore.docProperties.getText("button")
            '
            'If Button = ButtonCreateGUID Then
            'GUIDGenerator = New guidClass
            Call s.Add(cpCore.html.html_GetFormInputText2("GUID", Guid.NewGuid().ToString, 1, 80))
            'Call s.Add("<span style=""border:inset; padding:4px; background-color:white;"">" & Guid.NewGuid.ToString() & "</span>")
            'End If
            '
            ' Display form
            '
            GetForm_CreateGUID = genericLegacyView.OpenFormTable(cpCore, ButtonCancel & "," & ButtonCreateGUID) & s.Text & genericLegacyView.CloseFormTable(cpCore, ButtonCancel & "," & ButtonCreateGUID)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors1("GetForm_CreateGUID", "ErrorTrap")
        End Function
        ''
        ''====================================================================================================
        '''' <summary>
        '''' 'handle legacy errors in this class, v1
        '''' </summary>
        '''' <param name="MethodName"></param>
        '''' <param name="ignore0"></param>
        '''' <remarks></remarks>
        'Private Sub handleLegacyClassErrors1(ByVal MethodName As String, Optional ByVal ignore0 As String = "")
        '   throw (New ApplicationException("Unexpected exception"))'cpCore.handleLegacyError("Tools", MethodName, Err.Number, Err.Source, Err.Description, True, False)
        'End Sub
        ''
        ''====================================================================================================
        '''' <summary>
        '''' handle legacy errors in this class, v2
        '''' </summary>
        '''' <param name="MethodName"></param>
        '''' <param name="ErrDescription"></param>
        '''' <remarks></remarks>
        'Private Sub handleLegacyClassErrors2(ByVal MethodName As String, ByVal ErrDescription As String)
        '    Call Err.Raise(ignoreInteger, "App.EXEName", ErrDescription)
        'End Sub
    End Class
End Namespace
