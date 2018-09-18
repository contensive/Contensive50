
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Models.Context
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Core
    Public Class coreToolsClass



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
            Dim TestTicks As Long
            Dim OpenTicks As Long
            Dim NextTicks As Long
            Dim ReadTicks As Long
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
                TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds
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
                    TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds
                    RS = cpCore.db.executeQuery(SQL)
                    OpenTicks = OpenTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks
                    RecordCount = 0
                    TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds
                    For Each dr As DataRow In RS.Rows
                        NextTicks = NextTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks
                        '
                        TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds
                        TestCopy = genericController.encodeText(dr("NAME"))
                        ReadTicks = ReadTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks
                        '
                        RecordCount = RecordCount + 1
                        TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds
                    Next
                    NextTicks = NextTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks
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
                    TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds
                    CS = cpCore.db.csOpen("Site Properties", , , , , , ,, PageSize, PageNumber)
                    OpenTicks = OpenTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks
                    '
                    RecordCount = 0
                    Do While cpCore.db.csOk(CS)
                        '
                        TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds
                        TestCopy = genericController.encodeText(cpCore.db.cs_getValue(CS, "Name"))
                        ReadTicks = ReadTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks
                        '
                        TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds
                        Call cpCore.db.csGoNext(CS)
                        NextTicks = NextTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks
                        '
                        RecordCount = RecordCount + 1
                    Loop
                    Call cpCore.db.csClose(CS)
                    ReadTicks = ReadTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks
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
                    TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds
                    CS = cpCore.db.csOpen("Site Properties", , , , , , , "name", PageSize, PageNumber)
                    OpenTicks = OpenTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks
                    '
                    RecordCount = 0
                    Do While cpCore.db.csOk(CS)
                        '
                        TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds
                        TestCopy = genericController.encodeText(cpCore.db.cs_getValue(CS, "Name"))
                        ReadTicks = ReadTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks
                        '
                        TestTicks = cpCore.doc.appStopWatch.ElapsedMilliseconds
                        Call cpCore.db.csGoNext(CS)
                        NextTicks = NextTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks
                        '
                        RecordCount = RecordCount + 1
                    Loop
                    Call cpCore.db.csClose(CS)
                    ReadTicks = ReadTicks + cpCore.doc.appStopWatch.ElapsedMilliseconds - TestTicks
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
            GetForm_Benchmark = htmlController.legacy_openFormTable(cpCore, ButtonList) & Stream.Text & htmlController.legacy_closeFormTable(cpCore, ButtonList)
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
            dt = cpCore.db.executeQuery("Select ID from ccContent where name=" & cpCore.db.encodeSQLText(ContentName))
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
            dt = cpCore.db.executeQuery("Select name from ccContent where id=" & ContentID)
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
            RS = cpCore.db.executeQuery("Select ccTables.Name as TableName from ccContent Left Join ccTables on ccContent.ContentTableID=ccTables.ID where ccContent.name=" & cpCore.db.encodeSQLText(ContentName))
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
            RS = cpCore.db.executeQuery(SQL)
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
            GetForm_DbSchema = htmlController.legacy_openFormTable(cpCore, ButtonList) & Stream.Text & htmlController.legacy_closeFormTable(cpCore, ButtonList)
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
                Dim DataSourceName As String = String.Empty
                Dim ToolButton As String
                Dim ContentID As Integer
                Dim RecordCount As Integer
                Dim RecordPointer As Integer
                Dim CSPointer As Integer
                Dim formFieldId As Integer
                Dim ContentName As String = String.Empty
                Dim CDef As Models.Complex.cdefModel = Nothing
                Dim formFieldName As String
                Dim formFieldTypeId As Integer
                Dim TableName As String = String.Empty
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
                Dim ParentCDef As Models.Complex.cdefModel = Nothing
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
                Dim parentField As Models.Complex.CDefFieldModel = Nothing
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
                        CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentID, True, True)
                    End If
                End If
                If (CDef IsNot Nothing) Then
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
                                    For Each cdefFieldKvp As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In CDef.fields
                                        If cdefFieldKvp.Value.id = formFieldId Then
                                            '
                                            ' Field was found in CDef
                                            '
                                            With cdefFieldKvp.Value
                                                If .inherited And (Not formFieldInherited) Then
                                                    '
                                                    ' Was inherited, but make a copy of the field
                                                    '
                                                    CSTarget = cpCore.db.csInsertRecord("Content Fields")
                                                    If (cpCore.db.csOk(CSTarget)) Then
                                                        CSSource = cpCore.db.cs_openContentRecord("Content Fields", formFieldId)
                                                        If (cpCore.db.csOk(CSSource)) Then
                                                            Call cpCore.db.csCopyRecord(CSSource, CSTarget)
                                                        End If
                                                        Call cpCore.db.csClose(CSSource)
                                                        formFieldId = cpCore.db.csGetInteger(CSTarget, "ID")
                                                        Call cpCore.db.csSet(CSTarget, "ContentID", ContentID)
                                                    End If
                                                    Call cpCore.db.csClose(CSTarget)
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
                                                            Call cpCore.db.executeQuery(SQL, DataSourceName)
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
                                                        If cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                                                            SQL &= ",adminonly=" & cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaAdminOnly." & RecordPointer))
                                                        End If
                                                        If cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore) Then
                                                            SQL &= ",DeveloperOnly=" & cpCore.db.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaDeveloperOnly." & RecordPointer))
                                                        End If
                                                        SQL &= " where ID=" & formFieldId
                                                        Call cpCore.db.executeQuery(SQL)
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
                            cpCore.doc.clearMetaData()
                        End If
                        If (ToolButton = ButtonAdd) Then
                            '
                            ' ----- Insert a blank Field
                            '
                            CSPointer = cpCore.db.csInsertRecord("Content Fields")
                            If cpCore.db.csOk(CSPointer) Then
                                Call cpCore.db.csSet(CSPointer, "name", "unnamedField" & cpCore.db.csGetInteger(CSPointer, "id").ToString())
                                Call cpCore.db.csSet(CSPointer, "ContentID", ContentID)
                                Call cpCore.db.csSet(CSPointer, "EditSortPriority", 0)
                                ReloadCDef = True
                            End If
                            Call cpCore.db.csClose(CSPointer)
                        End If
                        ''
                        '' ----- Button Reload CDef
                        ''
                        If (ToolButton = ButtonSaveandInvalidateCache) Then
                            cpCore.cache.invalidateAll()
                            cpCore.doc.clearMetaData()
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
                            Return cpCore.webServer.redirect(cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & cpCore.webServer.requestPath & cpCore.webServer.requestPage & "?af=" & AdminFormTools)
                        End If
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
                        & "<div style=""padding-left:20px;""><a href=""?af=4&cid=" & Models.Complex.cdefModel.getContentId(cpCore, "content") & "&id=" & ContentID & """>Edit '" & ContentName & "' Content Definition</a></div>" _
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
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentID, True, True)
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
                        ParentContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ParentContentID)
                        ParentCDef = Models.Complex.cdefModel.getCdef(cpCore, ParentContentID, True, True)
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
                                        For Each kvp As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In ParentCDef.fields
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
                                    If Not cpCore.db.csOk(CSPointer) Then
                                        Call streamRow.Add(SpanClassAdminSmall & "Unknown[" & .fieldTypeId & "]</SPAN>")
                                    Else
                                        Call streamRow.Add(SpanClassAdminSmall & cpCore.db.csGetText(CSPointer, "Name") & "</SPAN>")
                                    End If
                                    Call cpCore.db.csClose(CSPointer)
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
                                If cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                                    streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaAdminOnly." & RecordCount, genericController.encodeText(.adminOnly), .inherited))
                                End If
                                '
                                ' Developer Only
                                '
                                If cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore) Then
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
                result = htmlController.legacy_openFormTable(cpCore, ButtonList) & Stream.Text & htmlController.legacy_closeFormTable(cpCore, ButtonList)
            Catch ex As Exception
                cpCore.handleException(ex)
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
            If cpCore.db.csOk(CS) Then
                TableName = cpCore.db.csGetText(CS, "name")
                DataSource = cpCore.db.csGetLookup(CS, "DataSourceID")
            End If
            Call cpCore.db.csClose(CS)
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
            GetForm_DbIndex = htmlController.legacy_openFormTable(cpCore, ButtonList) & GetForm_DbIndex & htmlController.legacy_closeFormTable(cpCore, ButtonList)
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
            CS = cpCore.db.csOpenSql_rev("Default", SQL)
            TableName = ""
            Do While cpCore.db.csOk(CS)
                If TableName <> cpCore.db.csGetText(CS, "TableName") Then
                    TableName = cpCore.db.csGetText(CS, "TableName")
                    GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableRow("<B>" & TableName & "</b>", TableColSpan, TableEvenRow)
                End If
                GetForm_ContentDbSchema = GetForm_ContentDbSchema & StartTableRow()
                GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableCell("&nbsp;", , , TableEvenRow)
                GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableCell(cpCore.db.csGetText(CS, "FieldName"), , , TableEvenRow)
                GetForm_ContentDbSchema = GetForm_ContentDbSchema & GetTableCell(cpCore.db.csGetText(CS, "FieldType"), , , TableEvenRow)
                GetForm_ContentDbSchema = GetForm_ContentDbSchema & kmaEndTableRow
                TableEvenRow = Not TableEvenRow
                cpCore.db.csGoNext(CS)
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
            GetForm_ContentDbSchema = (htmlController.legacy_openFormTable(cpCore, ButtonList)) & GetForm_ContentDbSchema & (htmlController.legacy_closeFormTable(cpCore, ButtonList))
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
            GetForm_LogFiles = (htmlController.legacy_openFormTable(cpCore, ButtonList)) & GetForm_LogFiles & (htmlController.legacy_closeFormTable(cpCore, ButtonList))
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
            Dim result As String = ""
            Try

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
                result = ""
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
                    result = cpCore.appRootFiles.readFile(cpCore.docProperties.getText("SourceFile"))
                    cpCore.doc.continueProcessing = False
                Else
                    result = result & GetTableStart
                    '
                    ' Parent Folder Link
                    '
                    If CurrentPath <> ParentPath Then
                        FileSize = ""
                        FileDate = ""
                        result = result & GetForm_LogFiles_Details_GetRow("<A href=""" & cpCore.webServer.requestPage & "?SetPath=" & ParentPath & """>" & FolderOpenImage & "</A>", "<A href=""" & cpCore.webServer.requestPage & "?SetPath=" & ParentPath & """>" & ParentPath & "</A>", FileSize, FileDate, RowEven)
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
                                result = result & GetForm_LogFiles_Details_GetRow("<A href=""" & cpCore.webServer.requestPage & "?SetPath=" & CurrentPath & "\" & FolderName & """>" & FolderClosedImage & "</A>", "<A href=""" & cpCore.webServer.requestPage & "?SetPath=" & CurrentPath & "\" & FolderName & """>" & FolderName & "</A>", FileSize, FileDate, RowEven)
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
                        result = result & GetForm_LogFiles_Details_GetRow(SpacerImage, "no files were found in this folder", FileSize, FileDate, RowEven)
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
                                result = result & GetForm_LogFiles_Details_GetRow(SpacerImage, CellCopy, FileSize, FileDate, RowEven)
                            End If
                        Next
                    End If
                    '
                    result = result & GetTableEnd
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
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
                    cpCore.doc.clearMetaData()
                    Call Stream.Add("<br>Content Definitions loaded")
                End If
                '
                ' Display form
                '
                Call Stream.Add(SpanClassAdminNormal)
                Call Stream.Add("<br>")
                Call Stream.Add("</span>")
                '
                result = htmlController.legacy_openFormTable(cpCore, ButtonList) & Stream.Text & htmlController.legacy_closeFormTable(cpCore, ButtonList)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
            If Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
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
                logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "Restarting Contensive", "dll", "ToolsClass", "GetForm_Restart", 0, "dll", "Warning: member " & cpCore.doc.authContext.user.name & " (" & cpCore.doc.authContext.user.id & ") restarted using the Restart tool", False, True, cpCore.webServer.requestUrl, "", "")
                'runAtServer = New runAtServerClass(cpCore)
                Call cpCore.webServer.redirect("/ccLib/Popup/WaitForIISReset.htm",,, False)
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
                cmdDetail.docProperties = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, "")
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
            GetForm_Restart = htmlController.legacy_openFormTable(cpCore, ButtonList) & Stream.Text & htmlController.legacy_closeFormTable(cpCore, ButtonList)
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
        Private Function GetCDef(ByVal ContentName As String) As Models.Complex.cdefModel
            Return Models.Complex.cdefModel.getCdef(cpCore, ContentName)
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
            GetForm_LoadTemplates = htmlController.legacy_openFormTable(cpCore, ButtonList) & Stream.Text & htmlController.legacy_closeFormTable(cpCore, ButtonList)
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
        '                    ParentContentName = models.complex.cdefmodel.getContentNameByID(cpcore,ParentID)
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
            Dim result As String = ""
            Try
                Dim Adminui As New adminUIController(cpCore)
                Dim InstanceOptionString As String = "AdminLayout=1&filesystem=content files"
                Dim addon As addonModel = addonModel.create(cpCore, "{B966103C-DBF4-4655-856A-3D204DEF6B21}")
                Dim Content As String = cpCore.addon.execute(addon, New BaseClasses.CPUtilsBaseClass.addonExecuteContext() With {
                    .addonType = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                    .instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, InstanceOptionString),
                    .instanceGuid = "-2",
                    .errorCaption = "File Manager"
                })
                Dim Description As String = "Manage files and folders within the virtual content file area."
                Dim ButtonList As String = ButtonApply & "," & ButtonCancel
                result = Adminui.GetBody("Content File Manager", ButtonList, "", False, False, Description, "", 0, Content)
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
        Private Function GetForm_WebsiteFileManager() As String
            Dim result As String = ""
            Try
                Dim Adminui As New adminUIController(cpCore)
                Dim InstanceOptionString As String = "AdminLayout=1&filesystem=website files"
                Dim addon As addonModel = addonModel.create(cpCore, "{B966103C-DBF4-4655-856A-3D204DEF6B21}")
                Dim Content As String = cpCore.addon.execute(addon, New BaseClasses.CPUtilsBaseClass.addonExecuteContext() With {
                    .addonType = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                    .instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, InstanceOptionString),
                    .instanceGuid = "-2",
                    .errorCaption = "File Manager"
                })
                Dim Description As String = "Manage files and folders within the Website's file area."
                Dim ButtonList As String = ButtonApply & "," & ButtonCancel
                result = Adminui.GetBody("Website File Manager", ButtonList, "", False, False, Description, "", 0, Content)
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
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
            IsDeveloper = cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore)
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
                    cmdDetail.docProperties = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, QS)
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
            CS = cpCore.db.csOpen("Content")
            Do While cpCore.db.csOk(CS)
                RecordName = cpCore.db.csGetText(CS, "Name")
                lcName = genericController.vbLCase(RecordName)
                If IsDeveloper Or (lcName = "page content") Or (lcName = "copy content") Or (lcName = "page templates") Then
                    RecordID = cpCore.db.csGetInteger(CS, "ID")
                    If genericController.vbInstr(1, "," & CDefList & ",", "," & RecordName & ",") <> 0 Then
                        TopHalf = TopHalf & "<div>" & cpCore.html.html_GetFormInputCheckBox2("Cdef" & RowPtr, True) & cpCore.html.html_GetFormInputHidden("CDefName" & RowPtr, RecordName) & "&nbsp;" & cpCore.db.csGetText(CS, "Name") & "</div>"
                    Else
                        BottomHalf = BottomHalf & "<div>" & cpCore.html.html_GetFormInputCheckBox2("Cdef" & RowPtr, False) & cpCore.html.html_GetFormInputHidden("CDefName" & RowPtr, RecordName) & "&nbsp;" & cpCore.db.csGetText(CS, "Name") & "</div>"
                    End If
                End If
                Call cpCore.db.csGoNext(CS)
                RowPtr = RowPtr + 1
            Loop
            Call cpCore.db.csClose(CS)
            Stream.Add(TopHalf & BottomHalf & cpCore.html.html_GetFormInputHidden("CDefRowCnt", RowPtr))
            '
            GetForm_FindAndReplace = htmlController.legacy_openFormTable(cpCore, ButtonCancel & "," & ButtonFindAndReplace) & Stream.Text & htmlController.legacy_closeFormTable(cpCore, ButtonCancel & "," & ButtonFindAndReplace)
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
                logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "Resetting IIS", "dll", "ToolsClass", "GetForm_IISReset", 0, "dll", "Warning: member " & cpCore.doc.authContext.user.name & " (" & cpCore.doc.authContext.user.id & ") executed an IISReset using the IISReset tool", False, True, cpCore.webServer.requestUrl, "", "")
                'runAtServer = New runAtServerClass(cpCore)
                Call cpCore.webServer.redirect("/ccLib/Popup/WaitForIISReset.htm",,, False)
                Call Threading.Thread.Sleep(2000)



                Throw New NotImplementedException("GetForm_IISReset")
                Dim taskScheduler As New taskSchedulerController()
                Dim cmdDetail As New cmdDetailClass
                cmdDetail.addonId = 0
                cmdDetail.addonName = "GetForm_IISReset"
                cmdDetail.docProperties = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, "")
                Call taskScheduler.addTaskToQueue(cpCore, taskQueueCommandEnumModule.runAddon, cmdDetail, False)


                ' Call runAtServer.executeCmd("IISReset", "")
            End If
            '
            ' Display form
            '
            GetForm_IISReset = htmlController.legacy_openFormTable(cpCore, ButtonCancel & "," & ButtonIISReset) & s.Text & htmlController.legacy_closeFormTable(cpCore, ButtonCancel & "," & ButtonIISReset)
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
            GetForm_CreateGUID = htmlController.legacy_openFormTable(cpCore, ButtonCancel & "," & ButtonCreateGUID) & s.Text & htmlController.legacy_closeFormTable(cpCore, ButtonCancel & "," & ButtonCreateGUID)
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
        '    fixme-- cpCore.handleException(New ApplicationException("")) ' -----ignoreInteger, "App.EXEName", ErrDescription)
        'End Sub


    End Class
End Namespace
