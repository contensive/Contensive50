
Option Explicit On
Option Strict On

Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Imports Contensive
Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Models.Context

Namespace Contensive.Core.Controllers
    Public Class dbController
        Implements IDisposable
        '==================================================================================================
        ''' <summary>
        ''' Remove this record from all watch lists
        ''' </summary>
        ''' <param name="ContentID"></param>
        ''' <param name="RecordID"></param>
        '
        Public Sub deleteContentRules(ByVal ContentID As Integer, ByVal RecordID As Integer)
            Try
                Dim ContentRecordKey As String
                Dim Criteria As String
                Dim ContentName As String
                Dim TableName As String
                '
                ' ----- remove all ContentWatchListRules (uncheck the watch lists in admin)
                '
                If (ContentID <= 0) Or (RecordID <= 0) Then
                    '
                    Throw New ApplicationException("ContentID [" & ContentID & "] or RecordID [" & RecordID & "] where blank")
                Else
                    ContentRecordKey = CStr(ContentID) & "." & CStr(RecordID)
                    Criteria = "(ContentRecordKey=" & encodeSQLText(ContentRecordKey) & ")"
                    ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID)
                    TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName)
                    ''
                    '' ----- Delete CalendarEventRules and CalendarEvents
                    ''
                    'If models.complex.cdefmodel.isContentFieldSupported(cpcore,"calendar events", "ID") Then
                    '    Call deleteContentRecords("Calendar Events", Criteria)
                    'End If
                    ''
                    '' ----- Delete ContentWatch
                    ''
                    'CS = cs_open("Content Watch", Criteria)
                    'Do While cs_ok(CS)
                    '    Call cs_deleteRecord(CS)
                    '    cs_goNext(CS)
                    'Loop
                    'Call cs_Close(CS)
                    '
                    ' ----- Table Specific rules
                    '
                    Select Case genericController.vbUCase(TableName)
                        Case "CCCALENDARS"
                            '
                            Call deleteContentRecords("Calendar Event Rules", "CalendarID=" & RecordID)
                        Case "CCCALENDAREVENTS"
                            '
                            Call deleteContentRecords("Calendar Event Rules", "CalendarEventID=" & RecordID)
                        Case "CCCONTENT"
                            '
                            Call deleteContentRecords("Group Rules", "ContentID=" & RecordID)
                        Case "CCCONTENTWATCH"
                            '
                            Call deleteContentRecords("Content Watch List Rules", "Contentwatchid=" & RecordID)
                        Case "CCCONTENTWATCHLISTS"
                            '
                            Call deleteContentRecords("Content Watch List Rules", "Contentwatchlistid=" & RecordID)
                        Case "CCGROUPS"
                            '
                            Call deleteContentRecords("Group Rules", "GroupID=" & RecordID)
                            Call deleteContentRecords("Library Folder Rules", "GroupID=" & RecordID)
                            Call deleteContentRecords("Member Rules", "GroupID=" & RecordID)
                            Call deleteContentRecords("Page Content Block Rules", "GroupID=" & RecordID)
                            Call deleteContentRecords("Path Rules", "GroupID=" & RecordID)
                        Case "CCLIBRARYFOLDERS"
                            '
                            Call deleteContentRecords("Library Folder Rules", "FolderID=" & RecordID)
                        Case "CCMEMBERS"
                            '
                            Call deleteContentRecords("Member Rules", "MemberID=" & RecordID)
                            Call deleteContentRecords("Topic Habits", "MemberID=" & RecordID)
                            Call deleteContentRecords("Member Topic Rules", "MemberID=" & RecordID)
                        Case "CCPAGECONTENT"
                            '
                            Call deleteContentRecords("Page Content Block Rules", "RecordID=" & RecordID)
                            Call deleteContentRecords("Page Content Topic Rules", "PageID=" & RecordID)
                        Case "CCSURVEYQUESTIONS"
                            '
                            Call deleteContentRecords("Survey Results", "QuestionID=" & RecordID)
                        Case "CCSURVEYS"
                            '
                            Call deleteContentRecords("Survey Questions", "SurveyID=" & RecordID)
                        Case "CCTOPICS"
                            '
                            Call deleteContentRecords("Topic Habits", "TopicID=" & RecordID)
                            Call deleteContentRecords("Page Content Topic Rules", "TopicID=" & RecordID)
                            Call deleteContentRecords("Member Topic Rules", "TopicID=" & RecordID)
                    End Select
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Get the SQL value for the true state of a boolean
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <returns></returns>
        '
        Private Function GetSQLTrue(ByVal DataSourceName As String) As Integer
            Return 1
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        ' try declaring the return as object() - an array holder for variants
        ' try setting up each call to return a variant, not an array of variants
        '
        Public Function cs_getRows(ByVal CSPointer As Integer) As String(,)
            Dim returnResult As String(,) = {}
            Try
                returnResult = contentSetStore(CSPointer).readCache
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '        '
        '        ''' <summary>
        '        ''' Returns the current row from csv_ContentSet
        '        ''' </summary>
        '        ''' <param name="CSPointer"></param>
        '        ''' <returns></returns>
        '        '
        '        Public Function cs_getRow(ByVal CSPointer As Integer) As Object
        '            Dim returnResult As String
        '            Try

        '            Catch ex As Exception
        '                cpCore.handleExceptionAndContinue(ex) : Throw
        '            End Try
        '            Return returnResult

        '            On Error GoTo ErrorTrap 'Const Tn = "MethodName-119" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim Row() As String
        '            Dim ColumnPointer As Integer
        '            '
        '            If cs_Ok(CSPointer) Then
        '                With contentSetStore(CSPointer)
        '                    ReDim Row(.ResultColumnCount)
        '                    If useCSReadCacheMultiRow Then
        '                        For ColumnPointer = 0 To .ResultColumnCount - 1
        '                            Row(ColumnPointer) = genericController.encodeText(.readCache(ColumnPointer, .readCacheRowPtr))
        '                        Next
        '                    Else
        '                        For ColumnPointer = 0 To .ResultColumnCount - 1
        '                            Row(ColumnPointer) = genericController.encodeText(.readCache(ColumnPointer, 0))
        '                        Next
        '                    End If
        '                    cs_getRow = Row
        '                End With
        '            End If
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_cs_getRow", True)
        '        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get the row count of the dataset
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        '
        Public Function csGetRowCount(ByVal CSPointer As Integer) As Integer
            Dim returnResult As Integer
            Try
                If Not csOk(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    returnResult = contentSetStore(CSPointer).readCacheRowCnt
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Returns a 1-d array with the results from the csv_ContentSet
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        '
        Public Function cs_getRowFields(ByVal CSPointer As Integer) As String()
            Dim returnResult As String() = {}
            Try
                If Not csOk(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    returnResult = contentSetStore(CSPointer).fieldNames
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' csv_DeleteTableRecord
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <param name="Criteria"></param>
        '
        Public Sub DeleteTableRecords(ByVal TableName As String, ByVal Criteria As String, ByVal DataSourceName As String)
            Try
                If String.IsNullOrEmpty(DataSourceName) Then
                    Throw New ArgumentException("dataSourceName cannot be blank")
                ElseIf String.IsNullOrEmpty(TableName) Then
                    Throw New ArgumentException("TableName cannot be blank")
                ElseIf String.IsNullOrEmpty(Criteria) Then
                    Throw New ArgumentException("Criteria cannot be blank")
                Else
                    Dim SQL As String = "DELETE FROM " & TableName & " WHERE " & Criteria
                    Call executeQuery(SQL, DataSourceName)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' return the content name of a csv_ContentSet
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        '
        Public Function cs_getContentName(ByVal CSPointer As Integer) As String
            Dim returnResult As String = ""
            Try
                If Not csOk(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    returnResult = contentSetStore(CSPointer).ContentName
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Get FieldDescritor from FieldType
        ''' </summary>
        ''' <param name="fieldType"></param>
        ''' <returns></returns>
        '
        Public Function getFieldTypeNameFromFieldTypeId(ByVal fieldType As Integer) As String
            Dim returnFieldTypeName As String = ""
            Try
                Select Case fieldType
                    Case FieldTypeIdBoolean
                        returnFieldTypeName = FieldTypeNameBoolean
                    Case FieldTypeIdCurrency
                        returnFieldTypeName = FieldTypeNameCurrency
                    Case FieldTypeIdDate
                        returnFieldTypeName = FieldTypeNameDate
                    Case FieldTypeIdFile
                        returnFieldTypeName = FieldTypeNameFile
                    Case FieldTypeIdFloat
                        returnFieldTypeName = FieldTypeNameFloat
                    Case FieldTypeIdFileImage
                        returnFieldTypeName = FieldTypeNameImage
                    Case FieldTypeIdLink
                        returnFieldTypeName = FieldTypeNameLink
                    Case FieldTypeIdResourceLink
                        returnFieldTypeName = FieldTypeNameResourceLink
                    Case FieldTypeIdInteger
                        returnFieldTypeName = FieldTypeNameInteger
                    Case FieldTypeIdLongText
                        returnFieldTypeName = FieldTypeNameLongText
                    Case FieldTypeIdLookup
                        returnFieldTypeName = FieldTypeNameLookup
                    Case FieldTypeIdMemberSelect
                        returnFieldTypeName = FieldTypeNameMemberSelect
                    Case FieldTypeIdRedirect
                        returnFieldTypeName = FieldTypeNameRedirect
                    Case FieldTypeIdManyToMany
                        returnFieldTypeName = FieldTypeNameManyToMany
                    Case FieldTypeIdFileText
                        returnFieldTypeName = FieldTypeNameTextFile
                    Case FieldTypeIdFileCSS
                        returnFieldTypeName = FieldTypeNameCSSFile
                    Case FieldTypeIdFileXML
                        returnFieldTypeName = FieldTypeNameXMLFile
                    Case FieldTypeIdFileJavascript
                        returnFieldTypeName = FieldTypeNameJavascriptFile
                    Case FieldTypeIdText
                        returnFieldTypeName = FieldTypeNameText
                    Case FieldTypeIdHTML
                        returnFieldTypeName = FieldTypeNameHTML
                    Case FieldTypeIdFileHTML
                        returnFieldTypeName = FieldTypeNameHTMLFile
                    Case Else
                        If fieldType = FieldTypeIdAutoIdIncrement Then
                            returnFieldTypeName = "AutoIncrement"
                        ElseIf fieldType = FieldTypeIdMemberSelect Then
                            returnFieldTypeName = "MemberSelect"
                        Else
                            '
                            ' If field type is ignored, call it a text field
                            '
                            returnFieldTypeName = FieldTypeNameText
                        End If
                End Select
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected exception")
            End Try
            Return returnFieldTypeName
        End Function
        ''
        ''========================================================================
        '''' <summary>
        '''' Returns a csv_ContentSet with records from the Definition that joins the  current Definition at the field specified.
        '''' </summary>
        '''' <param name="CSPointer"></param>
        '''' <param name="FieldName"></param>
        '''' <returns></returns>
        ''
        'Public Function OpenCSJoin(ByVal CSPointer As Integer, ByVal FieldName As String) As Integer
        '    Dim returnResult As Integer = -1
        '    Try
        '        Dim FieldValueVariant As Object
        '        Dim LookupContentName As String
        '        Dim ContentName As String
        '        Dim RecordId As Integer
        '        Dim fieldTypeId As Integer
        '        Dim fieldLookupId As Integer
        '        Dim MethodName As String
        '        Dim CDef As coreMetaDataClass.CDefClass
        '        '
        '        If Not cs_Ok(CSPointer) Then
        '            Throw New ArgumentException("dataset is not valid")
        '        ElseIf String.IsNullOrEmpty(FieldName.Trim()) Then
        '            Throw New ArgumentException("fieldname cannot be blank")
        '        ElseIf Not contentSetStore(CSPointer).Updateable Then
        '            Throw New ArgumentException("This csv_ContentSet does not support csv_OpenCSJoin. It may have been created from a SQL statement, not a Content Definition.")
        '        Else
        '            '
        '            ' ----- csv_ContentSet is updateable
        '            '
        '            ContentName = contentSetStore(CSPointer).ContentName
        '            CDef = contentSetStore(CSPointer).CDef
        '            FieldValueVariant = cs_getField(CSPointer, FieldName)
        '            If IsNull(FieldValueVariant) Or (Not CDef.fields.ContainsKey(FieldName.ToLower)) Then
        '                '
        '                ' ----- fieldname is not valid
        '                '
        '                Call handleLegacyClassError3(MethodName, ("The fieldname [" & FieldName & "] was not found in the current csv_ContentSet created from [ " & ContentName & " ]."))
        '            Else
        '                '
        '                ' ----- Field is good
        '                '
        '                Dim field As coreMetaDataClass.CDefFieldClass = CDef.fields(FieldName.ToLower())

        '                RecordId = cs_getInteger(CSPointer, "ID")
        '                fieldTypeId = field.fieldTypeId
        '                fieldLookupId = field.lookupContentID
        '                If fieldTypeId <> FieldTypeIdLookup Then
        '                    '
        '                    ' ----- Wrong Field Type
        '                    '
        '                    Call handleLegacyClassError3(MethodName, ("csv_OpenCSJoin only supports Content Definition Fields set as 'Lookup' type. Field [ " & FieldName & " ] is not a 'Lookup'."))
        '                ElseIf genericController.vbIsNumeric(FieldValueVariant) Then
        '                    '
        '                    '
        '                    '
        '                    If (fieldLookupId = 0) Then
        '                        '
        '                        ' ----- Content Definition for this Lookup was not found
        '                        '
        '                        Call handleLegacyClassError3(MethodName, "The Lookup Content Definition [" & fieldLookupId & "] for this field [" & FieldName & "] is not valid.")
        '                    Else
        '                        LookupContentName = models.complex.cdefmodel.getContentNameByID(cpcore,fieldLookupId)
        '                        returnResult = cs_open(LookupContentName, "ID=" & encodeSQLNumber(FieldValueVariant), "name", , , , , , 1)
        '                        'CDefLookup = appEnvironment.GetCDefByID(FieldLookupID)
        '                        'csv_OpenCSJoin = csOpen(CDefLookup.Name, "ID=" & encodeSQLNumber(FieldValueVariant), "name", , , , , , 1)
        '                    End If
        '                End If
        '            End If
        '        End If
        '        'End If
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '    End Try
        '    Return returnResult
        'End Function
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        Public Property sqlCommandTimeout() As Integer
            Get
                sqlCommandTimeout = _sqlTimeoutSecond
            End Get
            Set(ByVal value As Integer)
                _sqlTimeoutSecond = value
            End Set
        End Property
        Private _sqlTimeoutSecond As Integer
        '
        '=============================================================
        ''' <summary>
        ''' get a record's id from its guid
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="RecordGuid"></param>
        ''' <returns></returns>
        '=============================================================
        '
        Public Function GetRecordIDByGuid(ByVal ContentName As String, ByVal RecordGuid As String) As Integer
            Dim returnResult As Integer = 0
            Try
                If String.IsNullOrEmpty(ContentName) Then
                    Throw New ArgumentException("contentname cannot be blank")
                ElseIf String.IsNullOrEmpty(RecordGuid) Then
                    Throw New ArgumentException("RecordGuid cannot be blank")
                Else
                    Dim CS As Integer = csOpen(ContentName, "ccguid=" & encodeSQLText(RecordGuid), "ID", , , , , "ID")
                    If csOk(CS) Then
                        returnResult = csGetInteger(CS, "ID")
                    End If
                    Call csClose(CS)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function GetContentRows(ByVal ContentName As String, Optional ByVal Criteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal MemberID As Integer = SystemMemberID, Optional ByVal WorkflowRenderingMode As Boolean = False, Optional ByVal WorkflowEditingMode As Boolean = False, Optional ByVal SelectFieldList As String = "", Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As String(,)
            Dim returnRows As String(,) = {}
            Try
                '
                Dim CS As Integer = csOpen(ContentName, Criteria, SortFieldList, ActiveOnly, MemberID, WorkflowRenderingMode, WorkflowEditingMode, SelectFieldList, PageSize, PageNumber)
                If csOk(CS) Then
                    returnRows = contentSetStore(CS).readCache
                End If
                Call csClose(CS)
                '
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnRows
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' set the defaults in a dataset row
        ''' </summary>
        ''' <param name="CS"></param>
        '
        Public Sub SetCSRecordDefaults(ByVal CS As Integer)
            Try
                Dim lookups() As String
                Dim UCaseDefaultValueText As String
                Dim LookupContentName As String
                Dim FieldName As String
                Dim DefaultValueText As String
                '
                If Not csOk(CS) Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In contentSetStore(CS).CDef.fields
                        Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                        With field
                            FieldName = .nameLc
                            If (FieldName <> "") And (Not String.IsNullOrEmpty(.defaultValue)) Then
                                Select Case genericController.vbUCase(FieldName)
                                    Case "ID", "CCGUID", "CREATEKEY", "DATEADDED", "CREATEDBY", "CONTENTCONTROLID"
                                        '
                                        ' Block control fields
                                        '
                                    Case Else
                                        '
                                        ' General case
                                        '
                                        Select Case .fieldTypeId
                                            Case FieldTypeIdLookup
                                                '
                                                ' *******************************
                                                ' This is a problem - the defaults should come in as the ID values, not the names
                                                '   so a select can be added to the default configuration page
                                                ' *******************************
                                                '
                                                DefaultValueText = genericController.encodeText(.defaultValue)
                                                Call csSet(CS, FieldName, "null")
                                                If DefaultValueText <> "" Then
                                                    If .lookupContentID <> 0 Then
                                                        LookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, .lookupContentID)
                                                        If LookupContentName <> "" Then
                                                            Call csSet(CS, FieldName, getRecordID(LookupContentName, DefaultValueText))
                                                        End If
                                                    ElseIf .lookupList <> "" Then
                                                        UCaseDefaultValueText = genericController.vbUCase(DefaultValueText)
                                                        lookups = Split(.lookupList, ",")
                                                        For Ptr = 0 To UBound(lookups)
                                                            If UCaseDefaultValueText = genericController.vbUCase(lookups(Ptr)) Then
                                                                Call csSet(CS, FieldName, Ptr + 1)
                                                                Exit For
                                                            End If
                                                        Next
                                                    End If
                                                End If
                                            Case Else
                                                '
                                                ' else text
                                                '
                                                Call csSet(CS, FieldName, .defaultValue)
                                        End Select
                                End Select
                            End If
                        End With
                    Next
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="From"></param>
        ''' <param name="FieldList"></param>
        ''' <param name="Where"></param>
        ''' <param name="OrderBy"></param>
        ''' <param name="GroupBy"></param>
        ''' <param name="RecordLimit"></param>
        ''' <returns></returns>
        Public Function GetSQLSelect(ByVal DataSourceName As String, ByVal From As String, Optional ByVal FieldList As String = "", Optional ByVal Where As String = "", Optional ByVal OrderBy As String = "", Optional ByVal GroupBy As String = "", Optional ByVal RecordLimit As Integer = 0) As String
            Dim SQL As String = ""
            Try
                Select Case getDataSourceType(DataSourceName)
                    Case DataSourceTypeODBCMySQL
                        SQL = "SELECT"
                        SQL &= " " & FieldList
                        SQL &= " FROM " & From
                        If Where <> "" Then
                            SQL &= " WHERE " & Where
                        End If
                        If OrderBy <> "" Then
                            SQL &= " ORDER BY " & OrderBy
                        End If
                        If GroupBy <> "" Then
                            SQL &= " GROUP BY " & GroupBy
                        End If
                        If RecordLimit <> 0 Then
                            SQL &= " LIMIT " & RecordLimit
                        End If
                    Case Else
                        SQL = "SELECT"
                        If RecordLimit <> 0 Then
                            SQL &= " TOP " & RecordLimit
                        End If
                        If FieldList = "" Then
                            SQL &= " *"
                        Else
                            SQL &= " " & FieldList
                        End If
                        SQL &= " FROM " & From
                        If Where <> "" Then
                            SQL &= " WHERE " & Where
                        End If
                        If OrderBy <> "" Then
                            SQL &= " ORDER BY " & OrderBy
                        End If
                        If GroupBy <> "" Then
                            SQL &= " GROUP BY " & GroupBy
                        End If
                End Select
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return SQL
        End Function
        '
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <returns></returns>
        '
        Public Function getSQLIndexList(ByVal DataSourceName As String, ByVal TableName As String) As String
            Dim returnList As String = ""
            Try
                Dim ts As Models.Complex.tableSchemaModel = Models.Complex.tableSchemaModel.getTableSchema(cpCore, TableName, DataSourceName)
                If (ts IsNot Nothing) Then
                    For Each entry As String In ts.indexes
                        returnList &= "," & entry
                    Next
                    If returnList.Length > 0 Then
                        returnList = returnList.Substring(2)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnList
        End Function
        '
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="tableName"></param>
        ''' <returns></returns>
        '
        Public Function getTableSchemaData(tableName As String) As DataTable
            Dim returnDt As New DataTable
            Try
                Dim connString As String = getConnectionStringADONET(cpCore.serverConfig.appConfig.name, "default")
                Using connSQL As New SqlConnection(connString)
                    connSQL.Open()
                    returnDt = connSQL.GetSchema("Tables", {cpCore.serverConfig.appConfig.name, Nothing, tableName, Nothing})
                End Using
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnDt
        End Function
        '
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="tableName"></param>
        ''' <returns></returns>
        '
        Public Function getColumnSchemaData(tableName As String) As DataTable
            Dim returnDt As New DataTable
            Try
                If String.IsNullOrEmpty(tableName.Trim()) Then
                    Throw New ArgumentException("tablename cannot be blank")
                Else
                    Dim connString As String = getConnectionStringADONET(cpCore.serverConfig.appConfig.name, "default")
                    Using connSQL As New SqlConnection(connString)
                        connSQL.Open()
                        returnDt = connSQL.GetSchema("Columns", {cpCore.serverConfig.appConfig.name, Nothing, tableName, Nothing})
                    End Using
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnDt
        End Function
        '
        ' 
        '
        Public Function getIndexSchemaData(tableName As String) As DataTable
            Dim returnDt As New DataTable
            Try
                If String.IsNullOrEmpty(tableName.Trim()) Then
                    Throw New ArgumentException("tablename cannot be blank")
                Else
                    Dim connString As String = getConnectionStringADONET(cpCore.serverConfig.appConfig.name, "default")
                    Using connSQL As New SqlConnection(connString)
                        connSQL.Open()
                        returnDt = connSQL.GetSchema("Indexes", {cpCore.serverConfig.appConfig.name, Nothing, tableName, Nothing})
                    End Using
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnDt
        End Function
        ''
        ''==========
        '''' <summary>
        '''' determine the application status code for the health of this application
        '''' </summary>
        '''' <returns></returns>
        'Public Function checkHealth() As applicationStatusEnum
        '    Dim returnStatus As applicationStatusEnum = Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusLoading
        '    Try
        '        '
        '        Try
        '            '
        '            '--------------------------------------------------------------------------
        '            '   Verify the ccContent table exists 
        '            '--------------------------------------------------------------------------
        '            '
        '            Dim testDt As DataTable
        '            testDt = executeSql("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='ccContent'")
        '            If testDt.Rows.Count <> 1 Then
        '                cpcore.serverConfig.appConfig.appStatus = Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusDbFoundButContentMetaMissing
        '            End If
        '            testDt.Dispose()
        '        Catch ex As Exception
        '            cpcore.serverConfig.appConfig.appStatus = Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusDbFoundButContentMetaMissing
        '        End Try
        '        '
        '        '--------------------------------------------------------------------------
        '        '   Perform DB Integregity checks
        '        '--------------------------------------------------------------------------
        '        '
        '        Dim ts As coreMetaDataClass.tableSchemaClass = cpCore.metaData.getTableSchema("ccContent", "Default")
        '        If (ts Is Nothing) Then
        '            '
        '            ' Bad Db and no upgrade - exit
        '            '
        '            cpcore.serverConfig.appConfig.appStatus = Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusDbBad
        '        Else
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '    End Try
        'End Function
        '
        '=============================================================================
        ''' <summary>
        ''' get Sql Criteria for string that could be id, guid or name
        ''' </summary>
        ''' <param name="nameIdOrGuid"></param>
        ''' <returns></returns>
        Public Function getNameIdOrGuidSqlCriteria(nameIdOrGuid As String) As String
            Dim sqlCriteria As String = ""
            Try
                If genericController.vbIsNumeric(nameIdOrGuid) Then
                    sqlCriteria = "id=" & encodeSQLNumber(CDbl(nameIdOrGuid))
                ElseIf genericController.common_isGuid(nameIdOrGuid) Then
                    sqlCriteria = "ccGuid=" & encodeSQLText(nameIdOrGuid)
                Else
                    sqlCriteria = "name=" & encodeSQLText(nameIdOrGuid)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return sqlCriteria
        End Function
        '
        '=============================================================================
        ''' <summary>
        ''' Get a ContentID from the ContentName using just the tables
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        Private Function getDbContentID(ByVal ContentName As String) As Integer
            Dim returnContentId As Integer = 0
            Try
                Dim dt As DataTable = executeQuery("Select ID from ccContent where name=" & encodeSQLText(ContentName))
                If dt.Rows.Count > 0 Then
                    returnContentId = genericController.EncodeInteger(dt.Rows(0).Item("id"))
                End If
                dt.Dispose()
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnContentId
        End Function
        ''
        ''========================================================================
        '''' <summary>
        '''' get dataSource Name from dataSource Id. Return "default" for Id=0. Name is as it appears in the name field of the dataSources table. To use a key in the dataSource cache, normaize with normalizeDataSourceKey()
        '''' </summary>
        '''' <param name="DataSourceID"></param>
        '''' <returns></returns>
        'Public Function getDataSourceNameByID(DataSourceID As Integer) As String
        '    Dim returndataSourceName As String = "default"
        '    Try
        '        If (DataSourceID > 0) Then
        '            For Each kvp As KeyValuePair(Of String, Models.Entity.dataSourceModel) In dataSources
        '                If (kvp.Value.id = DataSourceID) Then
        '                    returndataSourceName = kvp.Value.name
        '                End If
        '            Next
        '            If String.IsNullOrEmpty(returndataSourceName) Then
        '                Using dt As DataTable = executeSql("select name,connString from ccDataSources where id=" & DataSourceID)
        '                    If dt.Rows.Count > 0 Then
        '                        Dim dataSource As New Models.Entity.dataSourceModel
        '                        With dataSource
        '                            .id = genericController.EncodeInteger(dt(0).Item("id"))
        '                            .ConnString = genericController.encodeText(dt(0).Item("connString"))
        '                            .name = Models.Entity.dataSourceModel.normalizeDataSourceName(genericController.encodeText(dt(0).Item("name")))
        '                            returndataSourceName = .name
        '                        End With
        '                    End If
        '                End Using
        '            End If
        '            If String.IsNullOrEmpty(returndataSourceName) Then
        '                Throw New ApplicationException("Datasource ID [" & DataSourceID & "] was not found, the default datasource will be used")
        '            End If
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '    End Try
        '    Return returndataSourceName
        'End Function
        '
        '=============================================================================
        ''' <summary>
        ''' Imports the named table into the content system
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <param name="ContentName"></param>
        '
        Public Sub createContentFromSQLTable(ByVal DataSource As dataSourceModel, ByVal TableName As String, ByVal ContentName As String)
            Try
                Dim SQL As String
                Dim dtFields As DataTable
                Dim DateAddedString As String
                Dim CreateKeyString As String
                Dim ContentID As Integer
                'Dim DataSourceID As Integer
                Dim ContentFieldFound As Boolean
                Dim ContentIsNew As Boolean             ' true if the content definition is being created
                Dim RecordID As Integer
                '
                '----------------------------------------------------------------
                ' ----- lookup datasource ID, if default, ID is -1
                '----------------------------------------------------------------
                '
                'DataSourceID = cpCore.db.GetDataSourceID(DataSourceName)
                DateAddedString = cpCore.db.encodeSQLDate(Now())
                CreateKeyString = cpCore.db.encodeSQLNumber(genericController.GetRandomInteger)
                '
                '----------------------------------------------------------------
                ' ----- Read in a record from the table to get fields
                '----------------------------------------------------------------
                '
                Dim dt As DataTable = cpCore.db.openTable(DataSource.Name, TableName, "", "", , 1)
                If dt.Rows.Count = 0 Then
                    dt.Dispose()
                    '
                    ' --- no records were found, add a blank if we can
                    '
                    dt = cpCore.db.insertTableRecordGetDataTable(DataSource.Name, TableName, cpCore.doc.authContext.user.id)
                    If dt.Rows.Count > 0 Then
                        RecordID = genericController.EncodeInteger(dt.Rows(0).Item("ID"))
                        Call cpCore.db.executeQuery("Update " & TableName & " Set active=0 where id=" & RecordID & ";", DataSource.Name)
                    End If
                End If
                If dt.Rows.Count = 0 Then
                    Throw New ApplicationException("Could Not add a record To table [" & TableName & "].")
                Else
                    '
                    '----------------------------------------------------------------
                    ' --- Find/Create the Content Definition
                    '----------------------------------------------------------------
                    '
                    ContentID = Models.Complex.cdefModel.getContentId(cpCore, ContentName)
                    If (ContentID <= 0) Then
                        '
                        ' ----- Content definition not found, create it
                        '
                        ContentIsNew = True
                        Call Models.Complex.cdefModel.addContent(cpCore, True, DataSource, TableName, ContentName)
                        'ContentID = csv_GetContentID(ContentName)
                        SQL = "Select ID from ccContent where name=" & cpCore.db.encodeSQLText(ContentName)
                        dt = cpCore.db.executeQuery(SQL)
                        If dt.Rows.Count = 0 Then
                            Throw New ApplicationException("Content Definition [" & ContentName & "] could Not be selected by name after it was inserted")
                        Else
                            ContentID = genericController.EncodeInteger(dt(0).Item("ID"))
                            Call cpCore.db.executeQuery("update ccContent Set CreateKey=0 where id=" & ContentID)
                        End If
                        dt.Dispose()
                        cpCore.cache.invalidateAll()
                        cpCore.doc.clearMetaData()
                    End If
                    '
                    '-----------------------------------------------------------
                    ' --- Create the ccFields records for the new table
                    '-----------------------------------------------------------
                    '
                    ' ----- locate the field in the content field table
                    '
                    SQL = "Select name from ccFields where ContentID=" & ContentID & ";"
                    dtFields = cpCore.db.executeQuery(SQL)
                    '
                    ' ----- verify all the table fields
                    '
                    For Each dcTableColumns As DataColumn In dt.Columns
                        '
                        ' ----- see if the field is already in the content fields
                        '
                        Dim UcaseTableColumnName As String
                        UcaseTableColumnName = genericController.vbUCase(dcTableColumns.ColumnName)
                        ContentFieldFound = False
                        For Each drContentRecords As DataRow In dtFields.Rows
                            If genericController.vbUCase(genericController.encodeText(drContentRecords("name"))) = UcaseTableColumnName Then
                                ContentFieldFound = True
                                Exit For
                            End If
                        Next
                        If Not ContentFieldFound Then
                            '
                            ' create the content field
                            '
                            Call createContentFieldFromTableField(ContentName, dcTableColumns.ColumnName, genericController.EncodeInteger(dcTableColumns.DataType))
                        Else
                            '
                            ' touch field so upgrade does not delete it
                            '
                            Call cpCore.db.executeQuery("update ccFields Set CreateKey=0 where (Contentid=" & ContentID & ") And (name = " & cpCore.db.encodeSQLText(UcaseTableColumnName) & ")")
                        End If
                    Next
                End If
                '
                ' Fill ContentControlID fields with new ContentID
                '
                SQL = "Update " & TableName & " Set ContentControlID=" & ContentID & " where (ContentControlID Is null);"
                Call cpCore.db.executeQuery(SQL, DataSource.Name)
                '
                ' ----- Load CDef
                '       Load only if the previous state of autoload was true
                '       Leave Autoload false during load so more do not trigger
                '
                cpCore.cache.invalidateAll()
                cpCore.doc.clearMetaData()
                dt.Dispose()
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        ' 
        '========================================================================
        ''' <summary>
        ''' Define a Content Definition Field based only on what is known from a SQL table
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="FieldName"></param>
        ''' <param name="ADOFieldType"></param>
        Public Sub createContentFieldFromTableField(ByVal ContentName As String, ByVal FieldName As String, ByVal ADOFieldType As Integer)
            Try
                '
                Dim field As New Models.Complex.CDefFieldModel
                '
                field.fieldTypeId = cpCore.db.getFieldTypeIdByADOType(ADOFieldType)
                field.caption = FieldName
                field.editSortPriority = 1000
                field.ReadOnly = False
                field.authorable = True
                field.adminOnly = False
                field.developerOnly = False
                field.TextBuffered = False
                field.htmlContent = False
                '
                Select Case genericController.vbUCase(FieldName)
                '
                ' --- Core fields
                '
                    Case "NAME"
                        field.caption = "Name"
                        field.editSortPriority = 100
                    Case "ACTIVE"
                        field.caption = "Active"
                        field.editSortPriority = 200
                        field.fieldTypeId = FieldTypeIdBoolean
                        field.defaultValue = "1"
                    Case "DATEADDED"
                        field.caption = "Created"
                        field.ReadOnly = True
                        field.editSortPriority = 5020
                    Case "CREATEDBY"
                        field.caption = "Created By"
                        field.fieldTypeId = FieldTypeIdLookup
                        field.lookupContentName(cpCore) = "Members"
                        field.ReadOnly = True
                        field.editSortPriority = 5030
                    Case "MODIFIEDDATE"
                        field.caption = "Modified"
                        field.ReadOnly = True
                        field.editSortPriority = 5040
                    Case "MODIFIEDBY"
                        field.caption = "Modified By"
                        field.fieldTypeId = FieldTypeIdLookup
                        field.lookupContentName(cpCore) = "Members"
                        field.ReadOnly = True
                        field.editSortPriority = 5050
                    Case "ID"
                        field.caption = "Number"
                        field.ReadOnly = True
                        field.editSortPriority = 5060
                        field.authorable = True
                        field.adminOnly = False
                        field.developerOnly = True
                    Case "CONTENTCONTROLID"
                        field.caption = "Content Definition"
                        field.fieldTypeId = FieldTypeIdLookup
                        field.lookupContentName(cpCore) = "Content"
                        field.editSortPriority = 5070
                        field.authorable = True
                        field.ReadOnly = False
                        field.adminOnly = True
                        field.developerOnly = True
                    Case "CREATEKEY"
                        field.caption = "CreateKey"
                        field.ReadOnly = True
                        field.editSortPriority = 5080
                        field.authorable = False
                    '
                    ' --- fields related to body content
                    '
                    Case "HEADLINE"
                        field.caption = "Headline"
                        field.editSortPriority = 1000
                        field.htmlContent = False
                    Case "DATESTART"
                        field.caption = "Date Start"
                        field.editSortPriority = 1100
                    Case "DATEEND"
                        field.caption = "Date End"
                        field.editSortPriority = 1200
                    Case "PUBDATE"
                        field.caption = "Publish Date"
                        field.editSortPriority = 1300
                    Case "ORGANIZATIONID"
                        field.caption = "Organization"
                        field.fieldTypeId = FieldTypeIdLookup
                        field.lookupContentName(cpCore) = "Organizations"
                        field.editSortPriority = 2005
                        field.authorable = True
                        field.ReadOnly = False
                    Case "COPYFILENAME"
                        field.caption = "Copy"
                        field.fieldTypeId = FieldTypeIdFileHTML
                        field.TextBuffered = True
                        field.editSortPriority = 2010
                    Case "BRIEFFILENAME"
                        field.caption = "Overview"
                        field.fieldTypeId = FieldTypeIdFileHTML
                        field.TextBuffered = True
                        field.editSortPriority = 2020
                        field.htmlContent = False
                    Case "IMAGEFILENAME"
                        field.caption = "Image"
                        field.fieldTypeId = FieldTypeIdFile
                        field.editSortPriority = 2040
                    Case "THUMBNAILFILENAME"
                        field.caption = "Thumbnail"
                        field.fieldTypeId = FieldTypeIdFile
                        field.editSortPriority = 2050
                    Case "CONTENTID"
                        field.caption = "Content"
                        field.fieldTypeId = FieldTypeIdLookup
                        field.lookupContentName(cpCore) = "Content"
                        field.ReadOnly = False
                        field.editSortPriority = 2060
                    '
                    ' --- Record Features
                    '
                    Case "PARENTID"
                        field.caption = "Parent"
                        field.fieldTypeId = FieldTypeIdLookup
                        field.lookupContentName(cpCore) = ContentName
                        field.ReadOnly = False
                        field.editSortPriority = 3000
                    Case "MEMBERID"
                        field.caption = "Member"
                        field.fieldTypeId = FieldTypeIdLookup
                        field.lookupContentName(cpCore) = "Members"
                        field.ReadOnly = False
                        field.editSortPriority = 3005
                    Case "CONTACTMEMBERID"
                        field.caption = "Contact"
                        field.fieldTypeId = FieldTypeIdLookup
                        field.lookupContentName(cpCore) = "Members"
                        field.ReadOnly = False
                        field.editSortPriority = 3010
                    Case "ALLOWBULKEMAIL"
                        field.caption = "Allow Bulk Email"
                        field.editSortPriority = 3020
                    Case "ALLOWSEEALSO"
                        field.caption = "Allow See Also"
                        field.editSortPriority = 3030
                    Case "ALLOWFEEDBACK"
                        field.caption = "Allow Feedback"
                        field.editSortPriority = 3040
                        field.authorable = False
                    Case "SORTORDER"
                        field.caption = "Alpha Sort Order"
                        field.editSortPriority = 3050
                    '
                    ' --- Display only information
                    '
                    Case "VIEWINGS"
                        field.caption = "Viewings"
                        field.ReadOnly = True
                        field.editSortPriority = 5000
                        field.defaultValue = "0"
                    Case "CLICKS"
                        field.caption = "Clicks"
                        field.ReadOnly = True
                        field.editSortPriority = 5010
                        field.defaultValue = "0"
                End Select
                Call Models.Complex.cdefModel.verifyCDefField_ReturnID(cpCore, ContentName, field)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' convert a dtaTable to a simple array - quick way to adapt old code
        ''' </summary>
        ''' <param name="dt"></param>
        ''' <returns></returns>
        '====================================================================================================
        ' refqctor - do not convert datatable to array in initcs, just cache the datatable
        Public Function convertDataTabletoArray(dt As DataTable) As String(,)
            Dim rows As String(,) = {}
            Try
                Dim columnCnt As Integer
                Dim rowCnt As Integer
                Dim cPtr As Integer
                Dim rPtr As Integer
                '
                ' 20150717 check for no columns
                If ((dt.Rows.Count > 0) And (dt.Columns.Count > 0)) Then
                    columnCnt = dt.Columns.Count
                    rowCnt = dt.Rows.Count
                    ' 20150717 change from rows(columnCnt,rowCnt) because other routines appear to use this count
                    ReDim rows(columnCnt - 1, rowCnt - 1)
                    rPtr = 0
                    For Each dr As DataRow In dt.Rows
                        cPtr = 0
                        For Each cell As DataColumn In dt.Columns
                            rows(cPtr, rPtr) = genericController.encodeText(dr(cell))
                            cPtr += 1
                        Next
                        rPtr += 1
                    Next
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return rows
        End Function
        '
        '========================================================================
        ' app.csv_DeleteTableRecord
        '========================================================================
        '
        Public Sub DeleteTableRecordChunks(ByVal DataSourceName As String, ByVal TableName As String, ByVal Criteria As String, Optional ByVal ChunkSize As Integer = 1000, Optional ByVal MaxChunkCount As Integer = 1000)
            '
            Dim PreviousCount As Integer
            Dim CurrentCount As Integer
            Dim LoopCount As Integer
            Dim SQL As String
            Dim iChunkSize As Integer
            Dim iChunkCount As Integer
            'dim dt as datatable
            Dim DataSourceType As Integer
            '
            DataSourceType = getDataSourceType(DataSourceName)
            If (DataSourceType <> DataSourceTypeODBCSQLServer) And (DataSourceType <> DataSourceTypeODBCAccess) Then
                '
                ' If not SQL server, just delete them
                '
                Call DeleteTableRecords(TableName, Criteria, DataSourceName)
            Else
                '
                ' ----- Clear up to date for the properties
                '
                iChunkSize = ChunkSize
                If iChunkSize = 0 Then
                    iChunkSize = 1000
                End If
                iChunkCount = MaxChunkCount
                If iChunkCount = 0 Then
                    iChunkCount = 1000
                End If
                '
                ' Get an initial count and allow for timeout
                '
                PreviousCount = -1
                LoopCount = 0
                CurrentCount = 0
                SQL = "select count(*) as RecordCount from " & TableName & " where " & Criteria
                Dim dt As DataTable
                dt = executeQuery(SQL)
                If dt.Rows.Count > 0 Then
                    CurrentCount = genericController.EncodeInteger(dt.Rows(0).Item(0))
                End If
                Do While (CurrentCount <> 0) And (PreviousCount <> CurrentCount) And (LoopCount < iChunkCount)
                    If getDataSourceType(DataSourceName) = DataSourceTypeODBCMySQL Then
                        SQL = "delete from " & TableName & " where id in (select ID from " & TableName & " where " & Criteria & " limit " & iChunkSize & ")"
                    Else
                        SQL = "delete from " & TableName & " where id in (select top " & iChunkSize & " ID from " & TableName & " where " & Criteria & ")"
                    End If
                    Call executeQuery(SQL, DataSourceName)
                    PreviousCount = CurrentCount
                    SQL = "select count(*) as RecordCount from " & TableName & " where " & Criteria
                    dt = executeQuery(SQL)
                    If dt.Rows.Count > 0 Then
                        CurrentCount = genericController.EncodeInteger(dt.Rows(0).Item(0))
                    End If
                    LoopCount = LoopCount + 1
                Loop
                If (CurrentCount <> 0) And (PreviousCount = CurrentCount) Then
                    '
                    ' records did not delete
                    '
                    cpCore.handleException(New ApplicationException("Error deleting record chunks. No records were deleted and the process was not complete."))
                ElseIf (LoopCount >= iChunkCount) Then
                    '
                    ' records did not delete
                    '
                    cpCore.handleException(New ApplicationException("Error deleting record chunks. The maximum chunk count was exceeded while deleting records."))
                End If
            End If
        End Sub
        '
        '=============================================================================
        '   Returns the link to the page that contains the record designated by the ContentRecordKey
        '       Returns DefaultLink if it can not be determined
        '=============================================================================
        '
        Public Function main_GetLinkByContentRecordKey(ByVal ContentRecordKey As String, Optional ByVal DefaultLink As String = "") As String
            Dim result As String = String.Empty
            Try
                Dim CSPointer As Integer
                Dim KeySplit() As String
                Dim ContentID As Integer
                Dim RecordID As Integer
                Dim ContentName As String
                Dim templateId As Integer
                Dim ParentID As Integer
                Dim DefaultTemplateLink As String
                Dim TableName As String
                Dim DataSource As String
                Dim ParentContentID As Integer
                Dim recordfound As Boolean
                '
                If ContentRecordKey <> "" Then
                    '
                    ' First try main_ContentWatch table for a link
                    '
                    CSPointer = csOpen("Content Watch", "ContentRecordKey=" & encodeSQLText(ContentRecordKey), , , ,, , "Link,Clicks")
                    If csOk(CSPointer) Then
                        result = cpCore.db.csGetText(CSPointer, "Link")
                    End If
                    Call cpCore.db.csClose(CSPointer)
                    '
                    If result = "" Then
                        '
                        ' try template for this page
                        '
                        KeySplit = Split(ContentRecordKey, ".")
                        If UBound(KeySplit) = 1 Then
                            ContentID = genericController.EncodeInteger(KeySplit(0))
                            If ContentID <> 0 Then
                                ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID)
                                RecordID = genericController.EncodeInteger(KeySplit(1))
                                If ContentName <> "" And RecordID <> 0 Then
                                    If Models.Complex.cdefModel.getContentTablename(cpCore, ContentName) = "ccPageContent" Then
                                        CSPointer = cpCore.db.csOpenRecord(ContentName, RecordID, , , "TemplateID,ParentID")
                                        If csOk(CSPointer) Then
                                            recordfound = True
                                            templateId = csGetInteger(CSPointer, "TemplateID")
                                            ParentID = csGetInteger(CSPointer, "ParentID")
                                        End If
                                        Call csClose(CSPointer)
                                        If Not recordfound Then
                                            '
                                            ' This content record does not exist - remove any records with this ContentRecordKey pointer
                                            '
                                            Call deleteContentRecords("Content Watch", "ContentRecordKey=" & encodeSQLText(ContentRecordKey))
                                            Call cpCore.db.deleteContentRules(Models.Complex.cdefModel.getContentId(cpCore, ContentName), RecordID)
                                        Else

                                            If templateId <> 0 Then
                                                CSPointer = cpCore.db.csOpenRecord("Page Templates", templateId, , , "Link")
                                                If csOk(CSPointer) Then
                                                    result = csGetText(CSPointer, "Link")
                                                End If
                                                Call csClose(CSPointer)
                                            End If
                                            If result = "" And ParentID <> 0 Then
                                                TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName)
                                                DataSource = Models.Complex.cdefModel.getContentDataSource(cpCore, ContentName)
                                                CSPointer = csOpenSql_rev(DataSource, "Select ContentControlID from " & TableName & " where ID=" & RecordID)
                                                If csOk(CSPointer) Then
                                                    ParentContentID = genericController.EncodeInteger(csGetText(CSPointer, "ContentControlID"))
                                                End If
                                                Call csClose(CSPointer)
                                                If ParentContentID <> 0 Then
                                                    result = main_GetLinkByContentRecordKey(CStr(ParentContentID & "." & ParentID), "")
                                                End If
                                            End If
                                            If result = "" Then
                                                DefaultTemplateLink = cpCore.siteProperties.getText("SectionLandingLink", requestAppRootPath & cpCore.siteProperties.serverPageDefault)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                        If result <> "" Then
                            result = genericController.modifyLinkQuery(result, rnPageId, CStr(RecordID), True)
                        End If
                    End If
                End If
                '
                If result = "" Then
                    result = DefaultLink
                End If
                '
                result = genericController.EncodeAppRootPath(result, cpCore.webServer.requestVirtualFilePath, requestAppRootPath, cpCore.webServer.requestDomain)
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '============================================================================
        '
        Public Shared Function encodeSqlTableName(sourceName As String) As String
            Dim returnName As String = ""
            Const FirstCharSafeString As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
            Const SafeString As String = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_@#"
            Try
                Dim src As String
                Dim TestChr As String
                Dim Ptr As Integer = 0
                '
                ' remove nonsafe URL characters
                '
                src = sourceName
                returnName = ""
                ' first character
                Do While Ptr < src.Length
                    TestChr = src.Substring(Ptr, 1)
                    Ptr += 1
                    If FirstCharSafeString.IndexOf(TestChr) >= 0 Then
                        returnName &= TestChr
                        Exit Do
                    End If
                Loop
                ' non-first character
                Do While Ptr < src.Length
                    TestChr = src.Substring(Ptr, 1)
                    Ptr += 1
                    If SafeString.IndexOf(TestChr) >= 0 Then
                        returnName &= TestChr
                    End If
                Loop
            Catch ex As Exception
                ' shared method, rethrow error
                Throw New ApplicationException("Exception in encodeSqlTableName(" & sourceName & ")", ex)
            End Try
            Return returnName
        End Function
        '
        '
        '
        Public Function GetTableID(ByVal TableName As String) As Integer
            Dim result As Integer = 0
            Dim CS As Integer
            GetTableID = -1
            CS = cpCore.db.csOpenSql("Select ID from ccTables where name=" & cpCore.db.encodeSQLText(TableName), , 1)
            If cpCore.db.csOk(CS) Then
                result = cpCore.db.csGetInteger(CS, "ID")
            End If
            Call cpCore.db.csClose(CS)
            Return result
        End Function
        '
        '========================================================================
        ' Opens a Content Definition into a ContentSEt
        '   Returns and integer that points into the ContentSet array
        '   If there was a problem, it returns -1
        '
        '   If authoring mode, as group of records are returned.
        '       The first is the current edit record
        '       The rest are the archive records.
        '========================================================================
        '
        Public Function csOpenRecord(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal WorkflowAuthoringMode As Boolean = False, Optional ByVal WorkflowEditingMode As Boolean = False, Optional ByVal SelectFieldList As String = "") As Integer
            Return csOpen(genericController.encodeText(ContentName), "(ID=" & cpCore.db.encodeSQLNumber(RecordID) & ")", , False, cpCore.doc.authContext.user.id, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1)
        End Function
        '
        '========================================================================
        '
        Public Function cs_open2(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal WorkflowAuthoringMode As Boolean = False, Optional ByVal WorkflowEditingMode As Boolean = False, Optional ByVal SelectFieldList As String = "") As Integer
            Return csOpen(ContentName, "(ID=" & cpCore.db.encodeSQLNumber(RecordID) & ")", , False, cpCore.doc.authContext.user.id, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1)
        End Function
        '
        '========================================================================
        '
        Public Sub SetContentCopy(ByVal CopyName As String, ByVal Content As String)
            '
            Dim CS As Integer
            Dim iCopyName As String
            Dim iContent As String
            Const ContentName = "Copy Content"
            '
            '  BuildVersion = app.dataBuildVersion
            If False Then '.3.210" Then
                Throw (New Exception("Contensive database was created with version " & cpCore.siteProperties.dataBuildVersion & ". main_SetContentCopy requires an builder."))
            Else
                iCopyName = genericController.encodeText(CopyName)
                iContent = genericController.encodeText(Content)
                CS = csOpen(ContentName, "name=" & encodeSQLText(iCopyName))
                If Not csOk(CS) Then
                    Call csClose(CS)
                    CS = csInsertRecord(ContentName)
                End If
                If csOk(CS) Then
                    Call csSet(CS, "name", iCopyName)
                    Call csSet(CS, "Copy", iContent)
                End If
                Call csClose(CS)
            End If
        End Sub
        Public Function csGetRecordEditLink(ByVal CSPointer As Integer, Optional ByVal AllowCut As Object = False) As String
            Dim result As String = ""

            Dim RecordName As String
            Dim ContentName As String
            Dim RecordID As Integer
            Dim ContentControlID As Integer
            Dim iCSPointer As Integer
            '
            iCSPointer = genericController.EncodeInteger(CSPointer)
            If iCSPointer = -1 Then
                Throw (New ApplicationException("main_cs_getRecordEditLink called with invalid iCSPointer")) ' handleLegacyError14(MethodName, "")
            Else
                If Not cpCore.db.csOk(iCSPointer) Then
                    Throw (New ApplicationException("main_cs_getRecordEditLink called with Not main_CSOK")) ' handleLegacyError14(MethodName, "")
                Else
                    '
                    ' Print an edit link for the records Content (may not be iCSPointer content)
                    '
                    RecordID = (cpCore.db.csGetInteger(iCSPointer, "ID"))
                    RecordName = cpCore.db.csGetText(iCSPointer, "Name")
                    ContentControlID = (cpCore.db.csGetInteger(iCSPointer, "contentcontrolid"))
                    ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentControlID)
                    If ContentName <> "" Then
                        result = cpCore.html.main_GetRecordEditLink2(ContentName, RecordID, genericController.EncodeBoolean(AllowCut), RecordName, cpCore.doc.authContext.isEditing(ContentName))
                    End If
                End If
            End If
            Return result
        End Function


#Region " IDisposable Support "
        Protected disposed As Boolean = False
        '
        '==========================================================================================
        ''' <summary>
        ''' dispose
        ''' </summary>
        ''' <param name="disposing"></param>
        ''' <remarks></remarks>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    '
                    ' ----- call .dispose for managed objects
                    '
                    '
                    ' ----- Close all open csv_ContentSets, and make sure the RS is killed
                    '
                    If contentSetStoreCount > 0 Then
                        Dim CSPointer As Integer
                        For CSPointer = 1 To contentSetStoreCount
                            If contentSetStore(CSPointer).IsOpen Then
                                Call csClose(CSPointer)
                            End If
                        Next
                    End If
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region
    End Class
    '
    Public Class sqlFieldListClass
        Private _sqlList As New List(Of NameValuePairType)
        Public Sub add(name As String, value As String)
            Dim nameValue As NameValuePairType
            nameValue.Name = name
            nameValue.Value = value
            _sqlList.Add(nameValue)
        End Sub
        Public Function getNameValueList() As String
            Dim returnPairs As String = ""
            Dim delim As String = ""
            For Each nameValue In _sqlList
                returnPairs &= delim & nameValue.Name & "=" & nameValue.Value
                delim = ","
            Next
            Return returnPairs
        End Function
        Public Function getNameList() As String
            Dim returnPairs As String = ""
            Dim delim As String = ""
            For Each nameValue In _sqlList
                returnPairs &= delim & nameValue.Name
                delim = ","
            Next
            Return returnPairs
        End Function
        Public Function getValueList() As String
            Dim returnPairs As String = ""
            Dim delim As String = ""
            For Each nameValue In _sqlList
                returnPairs &= delim & nameValue.Value
                delim = ","
            Next
            Return returnPairs
        End Function
        Public ReadOnly Property count As Integer
            Get
                Return _sqlList.Count
            End Get
        End Property
    End Class
End Namespace

