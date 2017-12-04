
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
        'Dependencies:
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
                Return cpCore.webServer.redirect(cpCore.siteProperties.getText("AdminURL", "/admin/"))
            End If
            '
            ' ----- Check permissions
            '
            If Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
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
                '
                If (Button = ButtonCancel) Then
                    '
                    ' -- Cancel
                    Return cpCore.webServer.redirect(cpCore.siteProperties.getText("AdminURL", "/admin/"))
                Else
                    '
                    ' -- Print out the page
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
            CSPointer = cpCore.db.csOpen("Content", "", "name")
            Do While cpCore.db.csOk(CSPointer)
                Stream.Add("<option value=""" & cpCore.db.csGetText(CSPointer, "name") & """>" & cpCore.db.csGetText(CSPointer, "name") & "</option>")
                ItemCount = ItemCount + 1
                cpCore.db.csGoNext(CSPointer)
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
            GetForm_DefineContentFieldsFromTable = htmlController.legacy_openFormTable(cpCore, ButtonList) & Stream.Text & htmlController.legacy_closeFormTable(cpCore, ButtonList)
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
            Public field As Models.Complex.CDefFieldModel
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
            GetForm_Root = htmlController.legacy_openFormTable(cpCore, ButtonList) & Stream.Text & htmlController.legacy_closeFormTable(cpCore, ButtonList)
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
                cpCore.handleException(ex)
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
                    SQLFilename = "SQLArchive" & Format(cpCore.doc.authContext.user.id, "000000000") & ".txt"
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
                        dt = cpCore.db.executeQuery(SQL, datasource.Name, PageSize * (PageNumber - 1), PageSize)
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
                                    Stream.Add(ColumnStart & genericController.encodeHTML(genericController.encodeText(CellData)) & ColumnEnd)
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
                returnHtml = htmlController.legacy_openFormTable(cpCore, ButtonList) & Stream.Text & htmlController.legacy_closeFormTable(cpCore, ButtonList)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
                    cpCore.doc.clearMetaData()
                    ContentID = Models.Complex.cdefModel.getContentId(cpCore, ContentName)
                    ParentNavID = cpCore.db.getRecordID(cnNavigatorEntries, "Manage Site Content")
                    If ParentNavID <> 0 Then
                        CS = cpCore.db.csOpen(cnNavigatorEntries, "(name=" & cpCore.db.encodeSQLText("Advanced") & ")and(parentid=" & ParentNavID & ")")
                        ParentNavID = 0
                        If cpCore.db.csOk(CS) Then
                            ParentNavID = cpCore.db.csGetInteger(CS, "ID")
                        End If
                        Call cpCore.db.csClose(CS)
                        If ParentNavID <> 0 Then
                            CS = cpCore.db.csOpen(cnNavigatorEntries, "(name=" & cpCore.db.encodeSQLText(ContentName) & ")and(parentid=" & NavID & ")")
                            If Not cpCore.db.csOk(CS) Then
                                Call cpCore.db.csClose(CS)
                                CS = cpCore.db.csInsertRecord(cnNavigatorEntries)
                            End If
                            If cpCore.db.csOk(CS) Then
                                Call cpCore.db.csSet(CS, "name", ContentName)
                                Call cpCore.db.csSet(CS, "parentid", ParentNavID)
                                Call cpCore.db.csSet(CS, "contentid", ContentID)
                            End If
                            Call cpCore.db.csClose(CS)
                        End If
                    End If
                    ContentID = Models.Complex.cdefModel.getContentId(cpCore, ContentName)
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
            Dim CDef As Models.Complex.cdefModel
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
                CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentID, True, False)
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
                            Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                            If field.id = FieldIDToAdd Then
                                'If field.Name = FieldNameToAdd Then
                                If field.inherited Then
                                    SourceContentID = field.contentId
                                    SourceName = field.nameLc
                                    CSSource = cpCore.db.csOpen("Content Fields", "(ContentID=" & SourceContentID & ")and(Name=" & cpCore.db.encodeSQLText(SourceName) & ")")
                                    If cpCore.db.csOk(CSSource) Then
                                        CSTarget = cpCore.db.csInsertRecord("Content Fields")
                                        If cpCore.db.csOk(CSTarget) Then
                                            Call cpCore.db.csCopyRecord(CSSource, CSTarget)
                                            Call cpCore.db.csSet(CSTarget, "ContentID", ContentID)
                                            ReloadCDef = True
                                        End If
                                        Call cpCore.db.csClose(CSTarget)
                                    End If
                                    Call cpCore.db.csClose(CSSource)
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
                        Dim adminColumn As Models.Complex.cdefModel.CDefAdminColumnClass = keyValuePair.Value
                        Dim field As Models.Complex.CDefFieldModel = CDef.fields(adminColumn.Name)
                        If field.inherited Then
                            SourceContentID = field.contentId
                            SourceName = field.nameLc
                            CSSource = cpCore.db.csOpen("Content Fields", "(ContentID=" & SourceContentID & ")and(Name=" & cpCore.db.encodeSQLText(SourceName) & ")")
                            If cpCore.db.csOk(CSSource) Then
                                CSTarget = cpCore.db.csInsertRecord("Content Fields")
                                If cpCore.db.csOk(CSTarget) Then
                                    Call cpCore.db.csCopyRecord(CSSource, CSTarget)
                                    Call cpCore.db.csSet(CSTarget, "ContentID", ContentID)
                                    ReloadCDef = True
                                End If
                                Call cpCore.db.csClose(CSTarget)
                            End If
                            Call cpCore.db.csClose(CSSource)
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
                                        Dim adminColumn As Models.Complex.cdefModel.CDefAdminColumnClass = keyValuePair.Value
                                        Dim field As Models.Complex.CDefFieldModel = CDef.fields(adminColumn.Name)
                                        CSPointer = cpCore.db.csOpenRecord("Content Fields", field.id)
                                        Call cpCore.db.csSet(CSPointer, "IndexColumn", (columnPtr) * 10)
                                        Call cpCore.db.csSet(CSPointer, "IndexWidth", Int((adminColumn.Width * 80) / ColumnWidthTotal))
                                        Call cpCore.db.csClose(CSPointer)
                                        columnPtr += 1
                                    Next
                                End If
                                CSPointer = cpCore.db.csOpenRecord("Content Fields", FieldIDToAdd, False, False)
                                If cpCore.db.csOk(CSPointer) Then
                                    Call cpCore.db.csSet(CSPointer, "IndexColumn", columnPtr * 10)
                                    Call cpCore.db.csSet(CSPointer, "IndexWidth", 20)
                                    Call cpCore.db.csSet(CSPointer, "IndexSortPriority", 99)
                                    Call cpCore.db.csSet(CSPointer, "IndexSortDirection", 1)
                                End If
                                Call cpCore.db.csClose(CSPointer)
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
                                    Dim adminColumn As Models.Complex.cdefModel.CDefAdminColumnClass = keyValuePair.Value
                                    Dim field As Models.Complex.CDefFieldModel = CDef.fields(adminColumn.Name)
                                    CSPointer = cpCore.db.csOpenRecord("Content Fields", field.id)
                                    If fieldId = TargetFieldID Then
                                        Call cpCore.db.csSet(CSPointer, "IndexColumn", 0)
                                        Call cpCore.db.csSet(CSPointer, "IndexWidth", 0)
                                        Call cpCore.db.csSet(CSPointer, "IndexSortPriority", 0)
                                        Call cpCore.db.csSet(CSPointer, "IndexSortDirection", 0)
                                    Else
                                        Call cpCore.db.csSet(CSPointer, "IndexColumn", (columnPtr) * 10)
                                        Call cpCore.db.csSet(CSPointer, "IndexWidth", Int((adminColumn.Width * 100) / ColumnWidthTotal))
                                    End If
                                    Call cpCore.db.csClose(CSPointer)
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
                                    Dim adminColumn As Models.Complex.cdefModel.CDefAdminColumnClass = keyValuePair.Value
                                    Dim field As Models.Complex.CDefFieldModel = CDef.fields(adminColumn.Name)
                                    FieldName = adminColumn.Name
                                    CS1 = cpCore.db.csOpenRecord("Content Fields", field.id)
                                    If (CDef.fields(FieldName.ToLower()).id = TargetFieldID) And (columnPtr < CDef.adminColumns.Count) Then
                                        Call cpCore.db.csSet(CS1, "IndexColumn", (columnPtr + 1) * 10)
                                        '
                                        MoveNextColumn = True
                                    ElseIf MoveNextColumn Then
                                        '
                                        ' This is one past target
                                        '
                                        Call cpCore.db.csSet(CS1, "IndexColumn", (columnPtr - 1) * 10)
                                        MoveNextColumn = False
                                    Else
                                        '
                                        ' not target or one past target
                                        '
                                        Call cpCore.db.csSet(CS1, "IndexColumn", (columnPtr) * 10)
                                        MoveNextColumn = False
                                    End If
                                    Call cpCore.db.csSet(CS1, "IndexWidth", Int((adminColumn.Width * 100) / ColumnWidthTotal))
                                    Call cpCore.db.csClose(CS1)
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
                                    Dim adminColumn As Models.Complex.cdefModel.CDefAdminColumnClass = keyValuePair.Value
                                    Dim field As Models.Complex.CDefFieldModel = CDef.fields(adminColumn.Name)
                                    FieldName = adminColumn.Name
                                    CS1 = cpCore.db.csOpenRecord("Content Fields", field.id)
                                    If (field.id = TargetFieldID) And (columnPtr < CDef.adminColumns.Count) Then
                                        Call cpCore.db.csSet(CS1, "IndexColumn", (columnPtr - 1) * 10)
                                        '
                                        MoveNextColumn = True
                                    ElseIf MoveNextColumn Then
                                        '
                                        ' This is one past target
                                        '
                                        Call cpCore.db.csSet(CS1, "IndexColumn", (columnPtr + 1) * 10)
                                        MoveNextColumn = False
                                    Else
                                        '
                                        ' not target or one past target
                                        '
                                        Call cpCore.db.csSet(CS1, "IndexColumn", (columnPtr) * 10)
                                        MoveNextColumn = False
                                    End If
                                    Call cpCore.db.csSet(CS1, "IndexWidth", Int((adminColumn.Width * 100) / ColumnWidthTotal))
                                    Call cpCore.db.csClose(CS1)
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
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentID, True, False)
                End If
                If (Button = ButtonSaveandInvalidateCache) Then
                    cpCore.cache.invalidateAll()
                    cpCore.doc.clearMetaData()
                    Return cpCore.webServer.redirect("?af=" & AdminFormToolConfigureListing & "&ContentID=" & ContentID)
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
                    For Each kvp As KeyValuePair(Of String, Models.Complex.cdefModel.CDefAdminColumnClass) In CDef.adminColumns
                        ColumnWidthTotal += kvp.Value.Width
                    Next
                    'For ColumnCount = 0 To CDef.adminColumns.Count - 1
                    '    ColumnWidthTotal = ColumnWidthTotal + CDef.adminColumns(ColumnCount).Width
                    'Next
                    If ColumnWidthTotal > 0 Then
                        Stream.Add("<table border=""0"" cellpadding=""5"" cellspacing=""0"" width=""90%"">")
                        Dim ColumnCount As Integer = 0
                        For Each kvp As KeyValuePair(Of String, Models.Complex.cdefModel.CDefAdminColumnClass) In CDef.adminColumns
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
                    For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In CDef.fields
                        Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                        With field
                            '
                            ' test if this column is in use
                            '
                            skipField = False
                            'ColumnPointer = CDef.adminColumns.Count
                            If CDef.adminColumns.Count > 0 Then
                                For Each kvp As KeyValuePair(Of String, Models.Complex.cdefModel.CDefAdminColumnClass) In CDef.adminColumns
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
                                ElseIf (.fieldTypeId = FieldTypeIdFileText) Then
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

            GetForm_ConfigureListing = htmlController.legacy_openFormTable(cpCore, ButtonList) & Stream.Text & htmlController.legacy_closeFormTable(cpCore, ButtonList)
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
                            CS = cpCore.db.csOpen("Content Fields", "(ContentID=" & ContentID & ")and(Name=" & cpCore.db.encodeSQLText(DiagArgument(DiagAction, 2)) & ")")
                            If cpCore.db.csOk(CS) Then
                                Call cpCore.db.csSet(CS, "Type", DiagArgument(DiagAction, 3))
                            End If
                            Call cpCore.db.csClose(CS)
                            'end case
                        Case DiagActionSetFieldInactive
                            '
                            ' ----- Set Field Inactive
                            '
                            ContentID = Local_GetContentID(DiagArgument(DiagAction, 1))
                            CS = cpCore.db.csOpen("Content Fields", "(ContentID=" & ContentID & ")and(Name=" & cpCore.db.encodeSQLText(DiagArgument(DiagAction, 2)) & ")")
                            If cpCore.db.csOk(CS) Then
                                Call cpCore.db.csSet(CS, "active", 0)
                            End If
                            Call cpCore.db.csClose(CS)
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
                            CS = cpCore.db.csOpen("Content", "name=" & cpCore.db.encodeSQLText(ContentName), "ID")
                            If cpCore.db.csOk(CS) Then
                                Call cpCore.db.csGoNext(CS)
                                Do While cpCore.db.csOk(CS)
                                    Call cpCore.db.csSet(CS, "active", 0)
                                    Call cpCore.db.csGoNext(CS)
                                Loop
                            End If
                            Call cpCore.db.csClose(CS)
                            'end case
                        Case DiagActionSetRecordInactive
                            '
                            ' ----- Set Field Inactive
                            '
                            ContentName = DiagArgument(DiagAction, 1)
                            RecordID = genericController.EncodeInteger(DiagArgument(DiagAction, 2))
                            CS = cpCore.db.csOpen(ContentName, "(ID=" & RecordID & ")")
                            If cpCore.db.csOk(CS) Then
                                Call cpCore.db.csSet(CS, "active", 0)
                            End If
                            Call cpCore.db.csClose(CS)
                            'end case
                        Case DiagActionSetFieldNotRequired
                            '
                            ' ----- Set Field not-required
                            '
                            ContentName = DiagArgument(DiagAction, 1)
                            RecordID = genericController.EncodeInteger(DiagArgument(DiagAction, 2))
                            CS = cpCore.db.csOpen(ContentName, "(ID=" & RecordID & ")")
                            If cpCore.db.csOk(CS) Then
                                Call cpCore.db.csSet(CS, "required", 0)
                            End If
                            Call cpCore.db.csClose(CS)
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
                        CSPointer = cpCore.db.csOpenSql_rev("Default", SQL)
                        If cpCore.db.csOk(CSPointer) Then
                            Do While cpCore.db.csOk(CSPointer)
                                DiagProblem = "PROBLEM: There are " & cpCore.db.csGetText(CSPointer, "RecordCount") & " records in the Content table with the name [" & cpCore.db.csGetText(CSPointer, "Name") & "]"
                                ReDim DiagActions(2)
                                DiagActions(0).Name = "Ignore, or handle this issue manually"
                                DiagActions(0).Command = ""
                                DiagActions(1).Name = "Mark all duplicate definitions inactive"
                                DiagActions(1).Command = CStr(DiagActionContentDeDupe) & "," & cpCore.db.cs_getValue(CSPointer, "name")
                                Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                Call cpCore.db.csGoNext(CSPointer)
                            Loop
                        End If
                        Call cpCore.db.csClose(CSPointer)
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
                        CS = cpCore.db.csOpenSql_rev("Default", SQL)
                        If Not cpCore.db.csOk(CS) Then
                            DiagProblem = "PROBLEM: No Content entries were found in the content table."
                            ReDim DiagActions(1)
                            DiagActions(0).Name = "Ignore, or handle this issue manually"
                            DiagActions(0).Command = ""
                            Stream.Add(GetDiagError(DiagProblem, DiagActions))
                        Else
                            Do While cpCore.db.csOk(CS) And (DiagActionCount < DiagActionCountMax)
                                FieldName = cpCore.db.csGetText(CS, "FieldName")
                                fieldType = cpCore.db.csGetInteger(CS, "FieldType")
                                FieldRequired = cpCore.db.csGetBoolean(CS, "FieldRequired")
                                FieldAuthorable = cpCore.db.csGetBoolean(CS, "FieldAuthorable")
                                ContentName = cpCore.db.csGetText(CS, "ContentName")
                                TableName = cpCore.db.csGetText(CS, "TableName")
                                DataSourceName = cpCore.db.csGetText(CS, "DataSourceName")
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
                                    CSTest = cpCore.db.csOpenSql_rev(DataSourceName, SQL)
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
                                Call cpCore.db.csClose(CSTest)
                                cpCore.db.csGoNext(CS)
                            Loop
                        End If
                        Call cpCore.db.csClose(CS)
                    End If
                    '
                    ' ----- Insert Content Testing
                    '
                    If (DiagActionCount < DiagActionCountMax) Then
                        Stream.Add(GetDiagHeader("Checking Content Insertion...<br>"))
                        '
                        CSContent = cpCore.db.csOpen("Content")
                        If Not cpCore.db.csOk(CSContent) Then
                            DiagProblem = "PROBLEM: No Content entries were found in the content table."
                            ReDim DiagActions(1)
                            DiagActions(0).Name = "Ignore, or handle this issue manually"
                            DiagActions(0).Command = ""
                            Stream.Add(GetDiagError(DiagProblem, DiagActions))
                        Else
                            Do While cpCore.db.csOk(CSContent) And (DiagActionCount < DiagActionCountMax)
                                ContentID = cpCore.db.csGetInteger(CSContent, "ID")
                                ContentName = cpCore.db.csGetText(CSContent, "name")
                                CSTestRecord = cpCore.db.csInsertRecord(ContentName)
                                If Not cpCore.db.csOk(CSTestRecord) Then
                                    DiagProblem = "PROBLEM: Could not insert a record using Content Definition [" & ContentName & "]"
                                    ReDim DiagActions(1)
                                    DiagActions(0).Name = "Ignore, or handle this issue manually"
                                    DiagActions(0).Command = ""
                                    Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                Else
                                    TestRecordID = cpCore.db.csGetInteger(CSTestRecord, "id")
                                    If TestRecordID = 0 Then
                                        DiagProblem = "PROBLEM: Content Definition [" & ContentName & "] does not support the required field [ID]"""
                                        ReDim DiagActions(1)
                                        DiagActions(0).Name = "Ignore, or handle this issue manually"
                                        DiagActions(0).Command = ""
                                        DiagActions(1).Name = "Set this Content Definition inactive"
                                        DiagActions(1).Command = CStr(DiagActionSetRecordInactive) & ",Content," & ContentID
                                        Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                    Else
                                        CSFields = cpCore.db.csOpen("Content Fields", "ContentID=" & ContentID)
                                        Do While cpCore.db.csOk(CSFields)
                                            '
                                            ' ----- read the value of the field to test its presents
                                            '
                                            FieldName = cpCore.db.csGetText(CSFields, "name")
                                            fieldType = cpCore.db.csGetInteger(CSFields, "Type")
                                            Select Case fieldType
                                                Case FieldTypeIdManyToMany
                                                    '
                                                    '   skip it
                                                    '
                                                Case FieldTypeIdRedirect
                                                    '
                                                    ' ----- redirect type, check redirect contentid
                                                    '
                                                    RedirectContentID = cpCore.db.csGetInteger(CSFields, "RedirectContentID")
                                                    ErrorCount = cpCore.doc.errorCount
                                                    bitBucket = Local_GetContentNameByID(RedirectContentID)
                                                    If IsNull(bitBucket) Or (ErrorCount <> cpCore.doc.errorCount) Then
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
                                                    ErrorCount = cpCore.doc.errorCount
                                                    bitBucket = cpCore.db.cs_getValue(CSTestRecord, FieldName)
                                                    If ErrorCount <> cpCore.doc.errorCount Then
                                                        DiagProblem = "PROBLEM: An error occurred reading the value of Content Field [" & ContentName & "].[" & FieldName & "]"
                                                        ReDim DiagActions(1)
                                                        DiagActions(0).Name = "Ignore, or handle this issue manually"
                                                        DiagActions(0).Command = ""
                                                        Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                                    Else
                                                        bitBucket = ""
                                                        LookupList = cpCore.db.csGetText(CSFields, "Lookuplist")
                                                        LookupContentID = cpCore.db.csGetInteger(CSFields, "LookupContentID")
                                                        If LookupContentID <> 0 Then
                                                            ErrorCount = cpCore.doc.errorCount
                                                            bitBucket = Local_GetContentNameByID(LookupContentID)
                                                        End If
                                                        If (LookupList = "") And ((LookupContentID = 0) Or (bitBucket = "") Or (ErrorCount <> cpCore.doc.errorCount)) Then
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
                                                    ErrorCount = cpCore.doc.errorCount
                                                    bitBucket = cpCore.db.cs_getValue(CSTestRecord, FieldName)
                                                    If (ErrorCount <> cpCore.doc.errorCount) Then
                                                        DiagProblem = "PROBLEM: An error occurred reading the value of Content Field [" & ContentName & "].[" & FieldName & "]"
                                                        ReDim DiagActions(3)
                                                        DiagActions(0).Name = "Ignore, or handle this issue manually"
                                                        DiagActions(0).Command = ""
                                                        DiagActions(1).Name = "Add this field into the Content Definitions Content (and Authoring) Table."
                                                        DiagActions(1).Command = "x"
                                                        Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                                    End If
                                            End Select
                                            cpCore.db.csGoNext(CSFields)
                                        Loop
                                    End If
                                    Call cpCore.db.csClose(CSFields)
                                    Call cpCore.db.csClose(CSTestRecord)
                                    Call cpCore.db.deleteContentRecord(ContentName, TestRecordID)
                                End If
                                cpCore.db.csGoNext(CSContent)
                            Loop
                        End If
                        Call cpCore.db.csClose(CSContent)
                    End If
                    '
                    ' ----- Check Navigator Entries
                    '
                    If (DiagActionCount < DiagActionCountMax) Then
                        Stream.Add(GetDiagHeader("Checking Navigator Entries...<br>"))
                        CSPointer = cpCore.db.csOpen(cnNavigatorEntries)
                        If Not cpCore.db.csOk(CSPointer) Then
                            DiagProblem = "PROBLEM: Could not open the [Navigator Entries] content."
                            ReDim DiagActions(3)
                            DiagActions(0).Name = "Ignore, or handle this issue manually"
                            DiagActions(0).Command = ""
                            Stream.Add(GetDiagError(DiagProblem, DiagActions))
                        Else
                            Do While cpCore.db.csOk(CSPointer) And (DiagActionCount < DiagActionCountMax)
                                ContentID = cpCore.db.csGetInteger(CSPointer, "ContentID")
                                If ContentID <> 0 Then
                                    CSContent = cpCore.db.csOpen("Content", "ID=" & ContentID)
                                    If Not cpCore.db.csOk(CSContent) Then
                                        DiagProblem = "PROBLEM: Menu Entry [" & cpCore.db.csGetText(CSPointer, "name") & "] points to an invalid Content Definition."
                                        ReDim DiagActions(3)
                                        DiagActions(0).Name = "Ignore, or handle this issue manually"
                                        DiagActions(0).Command = ""
                                        DiagActions(1).Name = "Remove this menu entry"
                                        DiagActions(1).Command = CStr(DiagActionDeleteRecord) & ",Navigator Entries," & cpCore.db.csGetInteger(CSPointer, "ID")
                                        Stream.Add(GetDiagError(DiagProblem, DiagActions))
                                    End If
                                    Call cpCore.db.csClose(CSContent)
                                End If
                                cpCore.db.csGoNext(CSPointer)
                            Loop
                        End If
                        Call cpCore.db.csClose(CSPointer)
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
            GetForm_ContentDiagnostic = htmlController.legacy_openFormTable(cpCore, ButtonList) & Stream.Text & htmlController.legacy_closeFormTable(cpCore, ButtonList)
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
            CSPointer = cpCore.db.csOpen("Content Fields", "(ContentID=" & ContentID & ")", "IndexColumn")
            If Not cpCore.db.csOk(CSPointer) Then
                Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors2("NormalizeIndexColumns", "Could not read Content Field Definitions")
            Else
                '
                ' Adjust IndexSortOrder to be 0 based, count by 1
                '
                ColumnCounter = 0
                Do While cpCore.db.csOk(CSPointer)
                    IndexColumn = cpCore.db.csGetInteger(CSPointer, "IndexColumn")
                    ColumnWidth = cpCore.db.csGetInteger(CSPointer, "IndexWidth")
                    If (IndexColumn = 0) Or (ColumnWidth = 0) Then
                        Call cpCore.db.csSet(CSPointer, "IndexColumn", 0)
                        Call cpCore.db.csSet(CSPointer, "IndexWidth", 0)
                        Call cpCore.db.csSet(CSPointer, "IndexSortPriority", 0)
                    Else
                        '
                        ' Column appears in Index, clean it up
                        '
                        Call cpCore.db.csSet(CSPointer, "IndexColumn", ColumnCounter)
                        ColumnCounter = ColumnCounter + 1
                        ColumnWidthTotal = ColumnWidthTotal + ColumnWidth
                    End If
                    cpCore.db.csGoNext(CSPointer)
                Loop
                If ColumnCounter = 0 Then
                    '
                    ' No columns found, set name as Column 0, active as column 1
                    '
                    Call cpCore.db.cs_goFirst(CSPointer)
                    Do While cpCore.db.csOk(CSPointer)
                        Select Case genericController.vbUCase(cpCore.db.csGetText(CSPointer, "name"))
                            Case "ACTIVE"
                                Call cpCore.db.csSet(CSPointer, "IndexColumn", 0)
                                Call cpCore.db.csSet(CSPointer, "IndexWidth", 20)
                                ColumnWidthTotal = ColumnWidthTotal + 20
                            Case "NAME"
                                Call cpCore.db.csSet(CSPointer, "IndexColumn", 1)
                                Call cpCore.db.csSet(CSPointer, "IndexWidth", 80)
                                ColumnWidthTotal = ColumnWidthTotal + 80
                        End Select
                        Call cpCore.db.csGoNext(CSPointer)
                    Loop
                End If
                '
                ' ----- Now go back and set a normalized Width value
                '
                If ColumnWidthTotal > 0 Then
                    Call cpCore.db.cs_goFirst(CSPointer)
                    Do While cpCore.db.csOk(CSPointer)
                        ColumnWidth = cpCore.db.csGetInteger(CSPointer, "IndexWidth")
                        ColumnWidth = CInt((ColumnWidth * 100) / CDbl(ColumnWidthTotal))
                        Call cpCore.db.csSet(CSPointer, "IndexWidth", ColumnWidth)
                        cpCore.db.csGoNext(CSPointer)
                    Loop
                End If
            End If
            Call cpCore.db.csClose(CSPointer)
            '
            ' ----- now fixup Sort Priority so only visible fields are sorted.
            '
            CSPointer = cpCore.db.csOpen("Content Fields", "(ContentID=" & ContentID & ")", "IndexSortPriority, IndexColumn")
            If Not cpCore.db.csOk(CSPointer) Then
                Throw (New ApplicationException("Unexpected exception")) '  Call handleLegacyClassErrors2("NormalizeIndexColumns", "Error reading Content Field Definitions")
            Else
                '
                ' Go through all fields, clear Sort Priority if it does not appear
                '
                Dim SortValue As Integer
                Dim SortDirection As Integer
                SortValue = 0
                Do While cpCore.db.csOk(CSPointer)
                    SortDirection = 0
                    If (cpCore.db.csGetInteger(CSPointer, "IndexColumn") = 0) Then
                        Call cpCore.db.csSet(CSPointer, "IndexSortPriority", 0)
                    Else
                        Call cpCore.db.csSet(CSPointer, "IndexSortPriority", SortValue)
                        SortDirection = cpCore.db.csGetInteger(CSPointer, "IndexSortDirection")
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
                    Call cpCore.db.csSet(CSPointer, "IndexSortDirection", SortDirection)
                    Call cpCore.db.csGoNext(CSPointer)
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
                    Call Models.Complex.cdefModel.createContentChild(cpCore, ChildContentName, ParentContentName, cpCore.doc.authContext.user.id)
                    '
                    Stream.Add("<br>Reloading Content Definitions...")
                    cpCore.cache.invalidateAll()
                    cpCore.doc.clearMetaData()
                    '
                    ' Add Admin Menu Entry
                    '
                    'If AddAdminMenuEntry Then
                    '    Stream.Add("<br>Adding menu entry (will not display until the next page)...")
                    '    CS = cpCore.db.cs_open(cnNavigatorEntries, "ContentID=" & ParentContentID)
                    '    If cpCore.db.cs_ok(CS) Then
                    '        MenuName = cpCore.db.cs_getText(CS, "name")
                    '        AdminOnly = cpCore.db.cs_getBoolean(CS, "AdminOnly")
                    '        DeveloperOnly = cpCore.db.cs_getBoolean(CS, "DeveloperOnly")
                    '    End If
                    '    Call cpCore.db.cs_Close(CS)
                    '    If MenuName <> "" Then
                    '        Call Controllers.appBuilderController.admin_VerifyAdminMenu(cpCore, MenuName, ChildContentName, ChildContentName, "", ChildContentName, AdminOnly, DeveloperOnly, False)
                    '    Else
                    '        Call Controllers.appBuilderController.admin_VerifyAdminMenu(cpCore, "Site Content", ChildContentName, ChildContentName, "", "")
                    '    End If
                    'End If
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
            GetForm_CreateChildContent = htmlController.legacy_openFormTable(cpCore, ButtonList) & Stream.Text & htmlController.legacy_closeFormTable(cpCore, ButtonList)

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
                Call cpCore.db.executeQuery("update ccContentWatch set Link=null;")
                Call Stream.Add("<br>Content Watch Link field cleared.")
            End If
            '
            ' ----- end form
            '
            ''Stream.Add( cpCore.main_GetFormInputHidden(RequestNameAdminForm, AdminFormToolClearContentWatchLink)
            Call Stream.Add("</span>")
            '
            GetForm_ClearContentWatchLinks = htmlController.legacy_openFormTable(cpCore, ButtonList) & Stream.Text & htmlController.legacy_closeFormTable(cpCore, ButtonList)
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
                Dim CD As Models.Complex.cdefModel
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
                    CSContent = cpCore.db.csOpen("Content", , , , , ,, "id")
                    If cpCore.db.csOk(CSContent) Then
                        Do
                            CD = Models.Complex.cdefModel.getCdef(cpCore, cpCore.db.csGetInteger(CSContent, "id"))
                            TableName = CD.ContentTableName
                            Call Stream.Add("Synchronizing Content " & CD.Name & " to table " & TableName & "<br>")
                            Call cpCore.db.createSQLTable(CD.ContentDataSourceName, TableName)
                            If CD.fields.Count > 0 Then
                                For Each keyValuePair In CD.fields
                                    Dim field As Models.Complex.CDefFieldModel
                                    field = keyValuePair.Value
                                    Call Stream.Add("...Field " & field.nameLc & "<br>")
                                    Call cpCore.db.createSQLTableField(CD.ContentDataSourceName, TableName, field.nameLc, field.fieldTypeId)
                                Next
                            End If
                            cpCore.db.csGoNext(CSContent)
                        Loop While cpCore.db.csOk(CSContent)
                        ContentNameArray = cpCore.db.cs_getRows(CSContent)
                        ContentNameCount = UBound(ContentNameArray, 2) + 1
                    End If
                    Call cpCore.db.csClose(CSContent)
                End If
                '
                returnValue = htmlController.legacy_openFormTable(cpCore, ButtonList) & Stream.Text & htmlController.legacy_closeFormTable(cpCore, ButtonList)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
    End Class
End Namespace
