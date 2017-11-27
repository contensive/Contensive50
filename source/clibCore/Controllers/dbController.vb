
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
        '
        '========================================================================
        '
        Private cpCore As coreClass
        '
        '========================================================================
        '
        Private Const pageSizeDefault = 9999
        '
        Private Const useCSReadCacheMultiRow = True
        '
        Private Const allowWorkflowErrors = False
        '
        ' on Db success, verified set true. If error and not verified, a simple test is run. on failure, Db disabled 
        Private Property dbVerified As Boolean = False                                  ' set true when configured and tested - else db calls are skipped
        Private Property dbEnabled As Boolean = True                                    ' set true when configured and tested - else db calls are skipped
        ' simple lazy cached values
        Private Property connectionStringDict As New Dictionary(Of String, String)          ' simple lazy cache so it only calculates conn string once
        Private Property contentSetStore As ContentSetType2()
        Private Property contentSetStoreCount As Integer       ' The number of elements being used
        Private Property contentSetStoreSize As Integer        ' The number of current elements in the array
        '
        Private Const contentSetStoreChunk = 50              ' How many are added at a time
        '
        ' when true, all csOpen, etc, will be setup, but not return any data (csv_IsCSOK false)
        ' this is used to generate the csv_ContentSet.Source so we can run a csv_GetContentRows without first opening a recordset
        Private contentSetOpenWithoutRecords As Boolean
        '
        '   SQL Timeouts
        Public sqlSlowThreshholdMsec As Integer        '
        '
        Private saveTransactionLog_InProcess As Boolean = False
        '
        ' ContentField Type, Stores information about fields in a content set
        Private Structure ContentSetWriteCacheType
            Dim fieldName As String
            Dim caption As String
            Dim valueObject As Object
            Dim fieldType As Integer
            Dim changed As Boolean                  ' If true, the next csv_SaveCSRecord will save this field
        End Structure
        '
        '  csv_ContentSet Type, Stores pointers to open recordsets of content being used by the page
        Private Structure ContentSetType2
            Dim IsOpen As Boolean                   ' If true, it is in use
            Dim LastUsed As Date                    ' The date/time this csv_ContentSet was last used
            Dim Updateable As Boolean               ' Can not update an csv_OpenCSSQL because Fields are not accessable
            Dim NewRecord As Boolean                ' true if it was created here
            Dim ContentName As String
            Dim CDef As Models.Complex.cdefModel
            Dim OwnerMemberID As Integer               ' ID of the member who opened the csv_ContentSet
            '
            ' Write Cache
            Dim writeCache As Dictionary(Of String, String)
            Dim IsModified As Boolean               ' Set when CS is opened and if a save happens
            '
            ' Recordset used to retrieve the results
            Dim dt As DataTable                        ' The Recordset
            '
            Dim Source As String                    ' Holds the SQL that created the result set
            Dim DataSource As String                ' The Datasource of the SQL that created the result set
            Dim PageSize As Integer                    ' Number of records in a cache page
            Dim PageNumber As Integer                  ' The Page that this result starts with
            '
            ' Read Cache
            Dim fieldPointer As Integer             ' ptr into fieldNames used for getFirstField, getnext, etc.
            Dim fieldNames() As String              ' 1-D array of the result field names
            Dim ResultColumnCount As Integer        ' number of columns in the fieldNames and readCache
            Dim ResultEOF As Boolean                ' readCache is at the last record
            Dim readCache As String(,)            ' 2-D array of the result rows/columns
            Dim readCacheRowCnt As Integer         ' number of rows in the readCache
            Dim readCacheRowPtr As Integer         ' Pointer to the current result row, first row is 0, BOF is -1
            '
            ' converted array to dictionary - Dim FieldPointer As Integer                ' Used for GetFirstField, GetNextField, etc
            Dim SelectTableFieldList As String      ' comma delimited list of all fields selected, in the form table.field
        End Structure
        '
        '==========================================================================================
        ''' <summary>
        ''' app services constructor
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Try
                Me.cpCore = cpCore
                _sqlTimeoutSecond = 30
                sqlSlowThreshholdMsec = 1000
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' return the correctly formated connection string for this datasource. Called only from within this class
        ''' </summary>
        ''' <returns>
        ''' </returns>
        Public Function getConnectionStringADONET(catalogName As String, Optional dataSourceName As String = "") As String
            '
            ' (OLEDB) OLE DB Provider for SQL Server > "Provider=sqloledb;Data Source=MyServerName;Initial Catalog=MyDatabaseName;User Id=MyUsername;Password=MyPassword;"
            '     https://www.codeproject.com/Articles/2304/ADO-Connection-Strings#OLE%20DB%20SqlServer
            '
            ' (OLEDB) Microsoft OLE DB Provider for SQL Server connection strings > "Provider=sqloledb;Data Source=myServerAddress;Initial Catalog=myDataBase;User Id = myUsername;Password=myPassword;"
            '     https://www.connectionstrings.com/microsoft-ole-db-provider-for-sql-server-sqloledb/
            '
            ' (ADONET) .NET Framework Data Provider for SQL Server > Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password = myPassword;
            '     https://www.connectionstrings.com/sql-server/
            '
            Dim returnConnString As String = ""
            Try
                '
                ' -- simple local cache so it does not have to be recreated each time
                Dim cacheName As String = "catalog:" & catalogName & "/datasource:" & dataSourceName
                If (connectionStringDict.ContainsKey(cacheName)) Then
                    returnConnString = connectionStringDict(cacheName)
                Else
                    '
                    ' -- lookup dataSource
                    Dim normalizedDataSourceName As String = Models.Entity.dataSourceModel.normalizeDataSourceName(dataSourceName)
                    If (String.IsNullOrEmpty(normalizedDataSourceName)) Or (normalizedDataSourceName = "default") Then
                        '
                        ' -- default datasource
                        returnConnString = "" _
                        & cpCore.dbServer.getConnectionStringADONET() _
                        & "Database=" & catalogName & ";"
                    Else
                        '
                        ' -- custom datasource from Db in primary datasource
                        If (Not cpCore.dataSourceDictionary.ContainsKey(normalizedDataSourceName)) Then
                            '
                            ' -- not found, this is a hard error
                            Throw New ApplicationException("Datasource [" & normalizedDataSourceName & "] was not found.")
                        Else
                            '
                            ' -- found in local cache
                            With cpCore.dataSourceDictionary(normalizedDataSourceName)
                                returnConnString = "" _
                                & "server=" & .endPoint & ";" _
                                & "User Id=" & .username & ";" _
                                & "Password=" & .password & ";" _
                                & "Database=" & catalogName & ";"
                            End With
                        End If
                    End If
                    connectionStringDict.Add(cacheName, returnConnString)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnConnString
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return the correctly formated connection string for this datasource. Called only from within this class
        ''' </summary>
        ''' <returns>
        ''' </returns>
        Private Function getConnectionStringOLEDB(catalogName As String, dataSourceName As String) As String
            '
            ' (OLEDB) OLE DB Provider for SQL Server > "Provider=sqloledb;Data Source=MyServerName;Initial Catalog=MyDatabaseName;User Id=MyUsername;Password=MyPassword;"
            '     https://www.codeproject.com/Articles/2304/ADO-Connection-Strings#OLE%20DB%20SqlServer
            '
            ' (OLEDB) Microsoft OLE DB Provider for SQL Server connection strings > "Provider=sqloledb;Data Source=myServerAddress;Initial Catalog=myDataBase;User Id = myUsername;Password=myPassword;"
            '     https://www.connectionstrings.com/microsoft-ole-db-provider-for-sql-server-sqloledb/
            '
            ' (ADONET) .NET Framework Data Provider for SQL Server > Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password = myPassword;
            '     https://www.connectionstrings.com/sql-server/
            '
            Dim returnConnString As String = ""
            Try
                Dim normalizedDataSourceName As String = Models.Entity.dataSourceModel.normalizeDataSourceName(dataSourceName)
                Dim defaultConnString As String = ""
                Dim serverUrl As String = cpCore.serverConfig.defaultDataSourceAddress
                If (serverUrl.IndexOf(":") > 0) Then
                    serverUrl = serverUrl.Substring(0, serverUrl.IndexOf(":"))
                End If
                defaultConnString &= "" _
                    & "Provider=sqloledb;" _
                    & "Data Source=" & serverUrl & ";" _
                    & "Initial Catalog=" & catalogName & ";" _
                    & "User Id=" & cpCore.serverConfig.defaultDataSourceUsername & ";" _
                    & "Password=" & cpCore.serverConfig.defaultDataSourcePassword & ";" _
                    & ""
                '
                ' -- lookup dataSource
                If (String.IsNullOrEmpty(normalizedDataSourceName)) Or (normalizedDataSourceName = "default") Then
                    '
                    ' -- default datasource
                    returnConnString = defaultConnString
                Else
                    '
                    ' -- custom datasource from Db in primary datasource
                    If (Not cpCore.dataSourceDictionary.ContainsKey(normalizedDataSourceName)) Then
                        '
                        ' -- not found, this is a hard error
                        Throw New ApplicationException("Datasource [" & normalizedDataSourceName & "] was not found.")
                    Else
                        '
                        ' -- found in local cache
                        With cpCore.dataSourceDictionary(normalizedDataSourceName)
                            returnConnString &= "" _
                                & "Provider=sqloledb;" _
                                & "Data Source=" & .endPoint & ";" _
                                & "User Id=" & .username & ";" _
                                & "Password=" & .password & ";" _
                                & ""
                        End With
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnConnString
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Execute a command (sql statemwent) and return a dataTable object
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <param name="dataSourceName"></param>
        ''' <param name="startRecord"></param>
        ''' <param name="maxRecords"></param>
        ''' <returns></returns>
        Public Function executeQuery(ByVal sql As String, Optional ByVal dataSourceName As String = "", Optional ByVal startRecord As Integer = 0, Optional ByVal maxRecords As Integer = 9999, Optional ByRef recordsReturned As Integer = 0) As DataTable
            Dim returnData As New DataTable
            Try
                If Not dbEnabled Then
                    '
                    ' -- db not available
                ElseIf (cpCore.serverConfig Is Nothing) Then
                    '
                    ' -- server config fail
                    cpCore.handleException(New ApplicationException("Cannot execute Sql in dbController without an application"))
                ElseIf (cpCore.serverConfig.appConfig Is Nothing) Then
                    '
                    ' -- server config fail
                    cpCore.handleException(New ApplicationException("Cannot execute Sql in dbController without an application"))
                Else
                    Dim connString As String = getConnectionStringADONET(cpCore.serverConfig.appConfig.name, dataSourceName)
                    'returnData = executeSql_noErrorHandling(sql, getConnectionStringADONET(cpCore.serverConfig.appConfig.name, dataSourceName), startRecord, maxRecords, recordsAffected)
                    '
                    ' REFACTOR
                    ' consider writing cs intrface to sql dataReader object -- one row at a time, vaster.
                    ' https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqldatareader.aspx
                    '
                    Dim sw As Stopwatch = Stopwatch.StartNew()
                    Using connSQL As New SqlConnection(connString)
                        connSQL.Open()
                        Using cmdSQL As New SqlCommand()
                            cmdSQL.CommandType = Data.CommandType.Text
                            cmdSQL.CommandText = sql
                            cmdSQL.Connection = connSQL
                            Using adptSQL = New SqlClient.SqlDataAdapter(cmdSQL)
                                recordsReturned = adptSQL.Fill(startRecord, maxRecords, returnData)
                            End Using
                        End Using
                    End Using
                    dbVerified = True
                    saveTransactionLog(sql, sw.ElapsedMilliseconds)
                End If
            Catch ex As Exception
                Dim newEx As New ApplicationException("Exception [" & ex.Message & "] executing sql [" & sql & "], datasource [" & dataSourceName & "], startRecord [" & startRecord & "], maxRecords [" & maxRecords & "]", ex)
                cpCore.handleException(newEx)
            End Try
            Return returnData
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Execute a command via ADO and return a recordset. Note the recordset must be disposed by the caller.
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <param name="dataSourceName"></param>
        ''' <param name="startRecord"></param>
        ''' <param name="maxRecords"></param>
        ''' <returns>You must close the recordset after use.</returns>
        Public Function executeSql_getRecordSet(ByVal sql As String, Optional ByVal dataSourceName As String = "", Optional ByVal startRecord As Integer = 0, Optional ByVal maxRecords As Integer = 9999) As ADODB.Recordset
            '
            ' from - https://support.microsoft.com/en-us/kb/308611
            '
            ' REFACTOR 
            ' - add start recrod And max record in
            ' - add dataSourceName into the getConnectionString call - if no dataSourceName, return catalog in cluster Db, else return connstring
            '
            'Dim cn As ADODB.Connection = New ADODB.Connection()
            Dim rs As ADODB.Recordset = New ADODB.Recordset()
            Dim connString As String = getConnectionStringOLEDB(cpCore.serverConfig.appConfig.name, dataSourceName)
            Try
                If dbEnabled Then
                    If (maxRecords > 0) Then
                        rs.MaxRecords = maxRecords
                    End If
                    ' Open Recordset without connection object.
                    rs.Open(sql, connString, ADODB.CursorTypeEnum.adOpenKeyset, ADODB.LockTypeEnum.adLockOptimistic, -1)
                    ' REFACTOR -- dbVerified Is only for the datasource. Need one for each...
                    dbVerified = True
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Dim newEx As New ApplicationException("Exception [" & ex.Message & "] executing sql [" & sql & "], datasource [" & dataSourceName & "], startRecord [" & startRecord & "], maxRecords [" & maxRecords & "]", ex)
                cpCore.handleException(newEx)
                Throw newEx
            End Try
            Return rs
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' execute sql on a specific datasource. No data is returned.
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <param name="dataSourceName"></param>
        Public Sub executeNonQuery(ByVal sql As String, Optional ByVal dataSourceName As String = "", Optional ByRef recordsAffected As Integer = 0)
            Try
                If dbEnabled Then
                    Dim connString As String = getConnectionStringADONET(cpCore.serverConfig.appConfig.name, dataSourceName)
                    Using connSQL As New SqlConnection(connString)
                        connSQL.Open()
                        Using cmdSQL As New SqlCommand()
                            cmdSQL.CommandType = Data.CommandType.Text
                            cmdSQL.CommandText = sql
                            cmdSQL.Connection = connSQL
                            recordsAffected = cmdSQL.ExecuteNonQuery()
                        End Using
                    End Using
                    dbVerified = True
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' execute sql on a specific datasource asynchonously. No data is returned.
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <param name="dataSourceName"></param>
        Public Sub executeNonQueryAsync(ByVal sql As String, Optional ByVal dataSourceName As String = "")
            Try
                If dbEnabled Then
                    Dim connString As String = getConnectionStringADONET(cpCore.serverConfig.appConfig.name, dataSourceName)
                    Using connSQL As New SqlConnection(connString)
                        connSQL.Open()
                        Using cmdSQL As New SqlCommand()
                            cmdSQL.CommandType = Data.CommandType.Text
                            cmdSQL.CommandText = sql
                            cmdSQL.Connection = connSQL
                            cmdSQL.ExecuteNonQueryAsync()
                        End Using
                    End Using
                    dbVerified = True
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Update a record in a table
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <param name="Criteria"></param>
        ''' <param name="sqlList"></param>
        Public Sub updateTableRecord(ByVal DataSourceName As String, ByVal TableName As String, ByVal Criteria As String, sqlList As sqlFieldListClass)
            Try
                Dim SQL As String = "update " & TableName & " set " & sqlList.getNameValueList & " where " & Criteria & ";"
                executeNonQuery(SQL, DataSourceName)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' iInserts a record into a table and returns the ID
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <param name="MemberID"></param>
        ''' <returns></returns>
        Public Function insertTableRecordGetId(ByVal DataSourceName As String, ByVal TableName As String, Optional ByVal MemberID As Integer = 0) As Integer
            Dim returnId As Integer = 0
            Try
                Using dt As DataTable = insertTableRecordGetDataTable(DataSourceName, TableName, MemberID)
                    If dt.Rows.Count > 0 Then
                        returnId = genericController.EncodeInteger(dt.Rows(0).Item("id"))
                    End If
                End Using
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnId
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Insert a record in a table, select it and return a datatable. You must dispose the datatable.
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <param name="MemberID"></param>
        ''' <returns></returns>
        Public Function insertTableRecordGetDataTable(ByVal DataSourceName As String, ByVal TableName As String, Optional ByVal MemberID As Integer = 0) As DataTable
            Dim returnDt As DataTable = Nothing
            Try
                Dim sqlList As New sqlFieldListClass
                Dim CreateKeyString As String
                Dim DateAddedString As String
                '
                CreateKeyString = encodeSQLNumber(genericController.GetRandomInteger)
                DateAddedString = encodeSQLDate(Now)
                '
                sqlList.add("createkey", CreateKeyString)
                sqlList.add("dateadded", DateAddedString)
                sqlList.add("createdby", encodeSQLNumber(MemberID))
                sqlList.add("ModifiedDate", DateAddedString)
                sqlList.add("ModifiedBy", encodeSQLNumber(MemberID))
                sqlList.add("ContentControlID", encodeSQLNumber(0))
                sqlList.add("Name", encodeSQLText(""))
                sqlList.add("Active", encodeSQLNumber(1))
                '
                Call insertTableRecord(DataSourceName, TableName, sqlList)
                returnDt = openTable(DataSourceName, TableName, "(DateAdded=" & DateAddedString & ")and(CreateKey=" & CreateKeyString & ")", "ID DESC",, 1)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnDt
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Insert a record in a table. There is no check for core fields
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <param name="sqlList"></param>
        Public Sub insertTableRecord(ByVal DataSourceName As String, ByVal TableName As String, sqlList As sqlFieldListClass)
            Try
                If sqlList.count > 0 Then
                    Dim sql As String = "INSERT INTO " & TableName & "(" & sqlList.getNameList & ")values(" & sqlList.getValueList & ")"
                    Dim dt As DataTable = executeQuery(sql, DataSourceName)
                    dt.Dispose()
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Opens the table specified and returns the data in a datatable. Returns all the active records in the table. Find the content record first, just for the dataSource.
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <param name="Criteria"></param>
        ''' <param name="SortFieldList"></param>
        ''' <param name="SelectFieldList"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        Public Function openTable(ByVal DataSourceName As String, ByVal TableName As String, Optional ByVal Criteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal SelectFieldList As String = "", Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As DataTable
            Dim returnDataTable As DataTable = Nothing
            Try
                Dim SQL As String = "SELECT"
                If String.IsNullOrEmpty(SelectFieldList) Then
                    SQL &= " *"
                Else
                    SQL &= " " & SelectFieldList
                End If
                SQL &= " FROM " & TableName
                If Not String.IsNullOrEmpty(Criteria) Then
                    SQL &= " WHERE (" & Criteria & ")"
                End If
                If Not String.IsNullOrEmpty(SortFieldList) Then
                    SQL &= " ORDER BY " & SortFieldList
                End If
                'SQL &= ";"
                returnDataTable = executeQuery(SQL, DataSourceName, (PageNumber - 1) * PageSize, PageSize)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnDataTable
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' update the transaction log
        ''' </summary>
        ''' <param name="LogEntry"></param>
        Private Sub saveTransactionLog(sql As String, ElapsedMilliseconds As Long)
            '
            ' -- do not allow reentry
            ' -- if during save, site properties need to be loaded, this stack-overflows
            If (Not saveTransactionLog_InProcess) Then
                saveTransactionLog_InProcess = True
                '
                ' -- block before appStatus OK because need site properties
                If (cpCore.serverConfig.enableLogging) And (cpCore.serverConfig.appConfig.appStatus = Models.Entity.serverConfigModel.appStatusEnum.OK) Then
                    If (cpCore.siteProperties.allowTransactionLog) Then
                        Dim LogEntry As String = ("duration [" & ElapsedMilliseconds & "], sql [" & sql & "]").Replace(vbCr, "").Replace(vbLf, "")
                        logController.appendLog(cpCore, LogEntry, "DbTransactions")
                    End If
                    If (ElapsedMilliseconds > sqlSlowThreshholdMsec) Then
                        logController.appendLog(cpCore, "query time  " & ElapsedMilliseconds & "ms, sql: " & sql, "SlowSQL")
                    End If
                End If
                saveTransactionLog_InProcess = False
            End If
        End Sub
        ''
        ''====================================================================================================
        '''' <summary>
        '''' Finds the where clause (first WHERE not in single quotes). returns 0 if not found, otherwise returns locaion of word where
        '''' </summary>
        '''' <param name="SQL"></param>
        '''' <returns></returns>
        'Private Function getSQLWherePosition(ByVal SQL As String) As Integer
        '    Dim returnPos As Integer = 0
        '    Try
        '        If genericController.isInStr(1, SQL, "WHERE", vbTextCompare) Then
        '            '
        '            ' ----- contains the word "WHERE", now weed out if not a where clause
        '            '
        '            returnPos = InStrRev(SQL, " WHERE ", , vbTextCompare)
        '            If returnPos = 0 Then
        '                returnPos = InStrRev(SQL, ")WHERE ", , vbTextCompare)
        '                If returnPos = 0 Then
        '                    returnPos = InStrRev(SQL, " WHERE(", , vbTextCompare)
        '                    If returnPos = 0 Then
        '                        returnPos = InStrRev(SQL, ")WHERE(", , vbTextCompare)
        '                    End If
        '                End If
        '            End If
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleException(ex) : Throw
        '    End Try
        '    Return returnPos
        'End Function
        '
        '========================================================================
        ''' <summary>
        ''' Returns true if the field exists in the table
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public Function isSQLTableField(ByVal DataSourceName As String, ByVal TableName As String, ByVal FieldName As String) As Boolean
            Dim returnOK As Boolean = False
            Try
                Dim tableSchema As Models.Complex.tableSchemaModel = Models.Complex.tableSchemaModel.getTableSchema(cpCore, TableName, DataSourceName)
                If (tableSchema IsNot Nothing) Then
                    returnOK = tableSchema.columns.Contains(FieldName.ToLower)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnOK
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Returns true if the table exists
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <returns></returns>
        Public Function isSQLTable(ByVal DataSourceName As String, ByVal TableName As String) As Boolean
            Dim ReturnOK As Boolean = False
            Try
                ReturnOK = (Not Models.Complex.tableSchemaModel.getTableSchema(cpCore, TableName, DataSourceName) Is Nothing)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return ReturnOK
        End Function
        '   
        '========================================================================
        ''' <summary>
        ''' Check for a table in a datasource
        ''' if the table is missing, create the table and the core fields
        ''' if NoAutoIncrement is false or missing, the ID field is created as an auto incremenet
        ''' if NoAutoIncrement is true, ID is created an an long
        ''' if the table is present, check all core fields
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <param name="AllowAutoIncrement"></param>
        Public Sub createSQLTable(ByVal DataSourceName As String, ByVal TableName As String, Optional ByVal AllowAutoIncrement As Boolean = True)
            Try
                If String.IsNullOrEmpty(TableName) Then
                    '
                    ' tablename required
                    '
                    Throw New ArgumentException("Tablename can not be blank.")
                ElseIf genericController.vbInstr(1, TableName, ".") <> 0 Then
                    '
                    ' Remote table -- remote system controls remote tables
                    '
                    Throw New ArgumentException("Tablename can not contain a period(.)")
                Else
                    '
                    ' Local table -- create if not in schema
                    '
                    If (Models.Complex.tableSchemaModel.getTableSchema(cpCore, TableName, DataSourceName) Is Nothing) Then
                        If Not AllowAutoIncrement Then
                            Dim SQL As String = "Create Table " & TableName & "(ID " & getSQLAlterColumnType(DataSourceName, FieldTypeIdInteger) & ");"
                            executeQuery(SQL, DataSourceName).Dispose()
                        Else
                            Dim SQL As String = "Create Table " & TableName & "(ID " & getSQLAlterColumnType(DataSourceName, FieldTypeIdAutoIdIncrement) & ");"
                            executeQuery(SQL, DataSourceName).Dispose()
                        End If
                    End If
                    '
                    ' ----- Test the common fields required in all tables
                    '
                    Call createSQLTableField(DataSourceName, TableName, "id", FieldTypeIdAutoIdIncrement)
                    Call createSQLTableField(DataSourceName, TableName, "name", FieldTypeIdText)
                    Call createSQLTableField(DataSourceName, TableName, "dateAdded", FieldTypeIdDate)
                    Call createSQLTableField(DataSourceName, TableName, "createdby", FieldTypeIdInteger)
                    Call createSQLTableField(DataSourceName, TableName, "modifiedBy", FieldTypeIdInteger)
                    Call createSQLTableField(DataSourceName, TableName, "ModifiedDate", FieldTypeIdDate)
                    Call createSQLTableField(DataSourceName, TableName, "active", FieldTypeIdBoolean)
                    Call createSQLTableField(DataSourceName, TableName, "createKey", FieldTypeIdInteger)
                    Call createSQLTableField(DataSourceName, TableName, "sortOrder", FieldTypeIdText)
                    Call createSQLTableField(DataSourceName, TableName, "contentControlID", FieldTypeIdInteger)
                    Call createSQLTableField(DataSourceName, TableName, "ccGuid", FieldTypeIdText)
                    ' -- 20171029 - deprecating fields makes migration difficult. add back and figure out future path
                    Call createSQLTableField(DataSourceName, TableName, "ContentCategoryID", FieldTypeIdInteger)
                    '
                    ' ----- setup core indexes
                    '
                    ' 20171029 primary key dow not need index -- Call createSQLIndex(DataSourceName, TableName, TableName & "Id", "ID")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "Active", "ACTIVE")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "Name", "NAME")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "SortOrder", "SORTORDER")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "DateAdded", "DATEADDED")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "CreateKey", "CREATEKEY")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "ContentControlID", "CONTENTCONTROLID")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "ModifiedDate", "MODIFIEDDATE")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "ccGuid", "CCGUID")
                End If
                Models.Complex.tableSchemaModel.tableSchemaListClear(cpCore)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Check for a field in a table in the database, if missing, create the field
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <param name="FieldName"></param>
        ''' <param name="fieldType"></param>
        ''' <param name="clearMetaCache"></param>
        Public Sub createSQLTableField(ByVal DataSourceName As String, ByVal TableName As String, ByVal FieldName As String, ByVal fieldType As Integer, Optional clearMetaCache As Boolean = False)
            Try
                If (fieldType = FieldTypeIdRedirect) Or (fieldType = FieldTypeIdManyToMany) Then
                    '
                    ' OK -- contensive fields with no table field
                    '
                    fieldType = fieldType
                ElseIf String.IsNullOrEmpty(TableName) Then
                    '
                    ' Bad tablename
                    '
                    Throw New ArgumentException("Table Name cannot be blank.")
                ElseIf fieldType = 0 Then
                    '
                    ' Bad fieldtype
                    '
                    Throw New ArgumentException("invalid fieldtype [" & fieldType & "]")
                ElseIf genericController.vbInstr(1, TableName, ".") <> 0 Then
                    '
                    ' External table
                    '
                    Throw New ArgumentException("Table name cannot include a period(.)")
                ElseIf FieldName = "" Then
                    '
                    ' Bad fieldname
                    '
                    Throw New ArgumentException("Field name cannot be blank")
                Else
                    If Not isSQLTableField(DataSourceName, TableName, FieldName) Then
                        Dim SQL As String = "ALTER TABLE " & TableName & " ADD " & FieldName & " "
                        If Not genericController.vbIsNumeric(fieldType) Then
                            '
                            ' ----- support old calls
                            '
                            SQL &= fieldType
                        Else
                            '
                            ' ----- translater type into SQL string
                            '
                            SQL &= getSQLAlterColumnType(DataSourceName, fieldType)
                        End If
                        Call executeQuery(SQL, DataSourceName).Dispose()
                        '
                        If clearMetaCache Then
                            Call cpCore.cache.invalidateAll()
                            Call cpCore.doc.clearMetaData()
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Delete (drop) a table
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        Public Sub deleteTable(ByVal DataSourceName As String, ByVal TableName As String)
            Try
                Call executeQuery("DROP TABLE " & TableName, DataSourceName).Dispose()
                cpCore.cache.invalidateAll()
                cpCore.doc.clearMetaData()
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Delete a table field from a table
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <param name="FieldName"></param>
        Public Sub deleteTableField(ByVal DataSourceName As String, ByVal TableName As String, ByVal FieldName As String)
            'Throw New NotImplementedException("deletetablefield")
            Try
                If isSQLTableField(DataSourceName, TableName, FieldName) Then
                    '
                    '   Delete any indexes that use this column
                    ' refactor -- need to finish this
                    Dim tableScheme As DataTable = getIndexSchemaData(TableName)
                    'With cdefCache.tableSchema(SchemaPointer)
                    '    If .IndexCount > 0 Then
                    '        For IndexPointer = 0 To .IndexCount - 1
                    '            If FieldName = .IndexFieldName(IndexPointer) Then
                    '                Call csv_DeleteTableIndex(DataSourceName, TableName, .IndexName(IndexPointer))
                    '            End If
                    '        Next
                    '    End If
                    'End With
                    '
                    '   Delete the field
                    '
                    Call executeQuery("ALTER TABLE " & TableName & " DROP COLUMN " & FieldName & ";", DataSourceName)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Create an index on a table, Fieldnames is  a comma delimited list of fields
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <param name="IndexName"></param>
        ''' <param name="FieldNames"></param>
        ''' <param name="clearMetaCache"></param>
        Public Sub createSQLIndex(ByVal DataSourceName As String, ByVal TableName As String, ByVal IndexName As String, ByVal FieldNames As String, Optional clearMetaCache As Boolean = False)
            Try
                Dim ts As Models.Complex.tableSchemaModel
                If Not (String.IsNullOrEmpty(TableName) And String.IsNullOrEmpty(IndexName) And String.IsNullOrEmpty(FieldNames)) Then
                    ts = Models.Complex.tableSchemaModel.getTableSchema(cpCore, TableName, DataSourceName)
                    If (ts IsNot Nothing) Then
                        If Not ts.indexes.Contains(IndexName.ToLower) Then
                            Call executeQuery("CREATE INDEX " & IndexName & " ON " & TableName & "( " & FieldNames & " );", DataSourceName)
                            If clearMetaCache Then
                                cpCore.cache.invalidateAll()
                                cpCore.doc.clearMetaData()
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=============================================================
        ''' <summary>
        ''' Return a record name given the record id. If not record is found, blank is returned.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="RecordID"></param>
        ''' <returns></returns>
        Public Function getRecordName(ByVal ContentName As String, ByVal RecordID As Integer) As String
            Dim returnRecordName As String = ""
            Try
                Dim CS As Integer = cs_openContentRecord(ContentName, RecordID, , , , "Name")
                If csOk(CS) Then
                    returnRecordName = csGet(CS, "Name")
                End If
                Call csClose(CS)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnRecordName
        End Function
        '
        '=============================================================
        ''' <summary>
        ''' get the lowest recordId based on its name. If no record is found, 0 is returned
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="RecordName"></param>
        ''' <returns></returns>
        Public Function getRecordID(ByVal ContentName As String, ByVal RecordName As String) As Integer
            Dim returnValue As Integer = 0
            Try
                If (Not String.IsNullOrEmpty(ContentName.Trim())) And (Not String.IsNullOrEmpty(RecordName.Trim())) Then
                    Dim cs As Integer = csOpen(ContentName, "name=" & encodeSQLText(RecordName), "ID", , , , , "ID")
                    If csOk(cs) Then
                        returnValue = csGetInteger(cs, "ID")
                    End If
                    Call csClose(cs)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnValue
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Get FieldDescritor from FieldType
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="fieldType"></param>
        ''' <returns></returns>
        Public Function getSQLAlterColumnType(ByVal DataSourceName As String, ByVal fieldType As Integer) As String
            Dim returnType As String = ""
            Try
                Select Case fieldType
                    Case FieldTypeIdBoolean
                        returnType = "Int NULL"
                    Case FieldTypeIdCurrency
                        returnType = "Float NULL"
                    Case FieldTypeIdDate
                        returnType = "DateTime NULL"
                    Case FieldTypeIdFloat
                        returnType = "Float NULL"
                    Case FieldTypeIdCurrency
                        returnType = "Float NULL"
                    Case FieldTypeIdInteger
                        returnType = "Int NULL"
                    Case FieldTypeIdLookup, FieldTypeIdMemberSelect
                        returnType = "Int NULL"
                    Case FieldTypeIdManyToMany, FieldTypeIdRedirect, FieldTypeIdFileImage, FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdText, FieldTypeIdFile, FieldTypeIdFileText, FieldTypeIdFileJavascript, FieldTypeIdFileXML, FieldTypeIdFileCSS, FieldTypeIdFileHTML
                        returnType = "VarChar(255) NULL"
                    Case FieldTypeIdLongText, FieldTypeIdHTML
                        '
                        ' ----- Longtext, depends on datasource
                        '
                        returnType = "Text Null"
                            'Select Case DataSourceConnectionObjs(Pointer).Type
                            '    Case DataSourceTypeODBCSQLServer
                            '        csv_returnType = "Text Null"
                            '    Case DataSourceTypeODBCAccess
                            '        csv_returnType = "Memo Null"
                            '    Case DataSourceTypeODBCMySQL
                            '        csv_returnType = "Text Null"
                            '    Case Else
                            '        csv_returnType = "VarChar(65535)"
                            'End Select
                    Case FieldTypeIdAutoIdIncrement
                        '
                        ' ----- autoincrement type, depends on datasource
                        '
                        returnType = "INT IDENTITY PRIMARY KEY"
                        'Select Case DataSourceConnectionObjs(Pointer).Type
                        '    Case DataSourceTypeODBCSQLServer
                        '        csv_returnType = "INT IDENTITY PRIMARY KEY"
                        '    Case DataSourceTypeODBCAccess
                        '        csv_returnType = "COUNTER CONSTRAINT PrimaryKey PRIMARY KEY"
                        '    Case DataSourceTypeODBCMySQL
                        '        csv_returnType = "INT AUTO_INCREMENT PRIMARY KEY"
                        '    Case Else
                        '        csv_returnType = "INT AUTO_INCREMENT PRIMARY KEY"
                        'End Select
                    Case Else
                        '
                        ' Invalid field type
                        '
                        Throw New ApplicationException("Can Not proceed because the field being created has an invalid FieldType [" & fieldType & "]")
                End Select
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnType
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Delete an Index for a table
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="TableName"></param>
        ''' <param name="IndexName"></param>
        Public Sub deleteSqlIndex(ByVal DataSourceName As String, ByVal TableName As String, ByVal IndexName As String)
            Try
                Dim ts As Models.Complex.tableSchemaModel
                Dim DataSourceType As Integer
                Dim sql As String
                '
                ts = Models.Complex.tableSchemaModel.getTableSchema(cpCore, TableName, DataSourceName)
                If (Not ts Is Nothing) Then
                    If ts.indexes.Contains(IndexName.ToLower) Then
                        DataSourceType = getDataSourceType(DataSourceName)
                        Select Case DataSourceType
                            Case DataSourceTypeODBCAccess
                                sql = "DROP INDEX " & IndexName & " On " & TableName & ";"
                            Case DataSourceTypeODBCMySQL
                                Throw New NotImplementedException("mysql")
                            Case Else
                                sql = "DROP INDEX " & TableName & "." & IndexName & ";"
                        End Select
                        Call executeQuery(sql, DataSourceName)
                        cpCore.cache.invalidateAll()
                        cpCore.doc.clearMetaData()
                    End If
                End If

            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' returns true if the cdef field exists
        ''' </summary>
        ''' <param name="ContentID"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public Function isCdefField(ByVal ContentID As Integer, ByVal FieldName As String) As Boolean
            Dim returnOk As Boolean = False
            Try
                Dim dt As DataTable = executeQuery("Select top 1 id from ccFields where name=" & encodeSQLText(FieldName) & " And contentid=" & ContentID)
                isCdefField = genericController.isDataTableOk(dt)
                dt.Dispose()
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnOk
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Get a DataSource type (SQL Server, etc) from its Name
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <returns></returns>
        '
        Public Function getDataSourceType(ByVal DataSourceName As String) As Integer
            Return DataSourceTypeODBCSQLServer
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Get FieldType from ADO Field Type
        ''' </summary>
        ''' <param name="ADOFieldType"></param>
        ''' <returns></returns>
        Public Function getFieldTypeIdByADOType(ByVal ADOFieldType As Integer) As Integer
            Dim returnType As Integer
            Try
                Select Case ADOFieldType

                    Case 2
                        returnType = FieldTypeIdFloat
                    Case 3
                        returnType = FieldTypeIdInteger
                    Case 4
                        returnType = FieldTypeIdFloat
                    Case 5
                        returnType = FieldTypeIdFloat
                    Case 6
                        returnType = FieldTypeIdInteger
                    Case 11
                        returnType = FieldTypeIdBoolean
                    Case 135
                        returnType = FieldTypeIdDate
                    Case 200
                        returnType = FieldTypeIdText
                    Case 201
                        returnType = FieldTypeIdLongText
                    Case 202
                        returnType = FieldTypeIdText
                    Case Else
                        returnType = FieldTypeIdText
                End Select
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnType
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Get FieldType from FieldTypeName
        ''' </summary>
        ''' <param name="FieldTypeName"></param>
        ''' <returns></returns>
        Public Function getFieldTypeIdFromFieldTypeName(ByVal FieldTypeName As String) As Integer
            Dim returnTypeId As Integer = 0
            Try
                Select Case genericController.vbLCase(FieldTypeName)
                    Case FieldTypeNameLcaseBoolean
                        returnTypeId = FieldTypeIdBoolean
                    Case FieldTypeNameLcaseCurrency
                        returnTypeId = FieldTypeIdCurrency
                    Case FieldTypeNameLcaseDate
                        returnTypeId = FieldTypeIdDate
                    Case FieldTypeNameLcaseFile
                        returnTypeId = FieldTypeIdFile
                    Case FieldTypeNameLcaseFloat
                        returnTypeId = FieldTypeIdFloat
                    Case FieldTypeNameLcaseImage
                        returnTypeId = FieldTypeIdFileImage
                    Case FieldTypeNameLcaseLink
                        returnTypeId = FieldTypeIdLink
                    Case FieldTypeNameLcaseResourceLink, "resource link", "resourcelink"
                        returnTypeId = FieldTypeIdResourceLink
                    Case FieldTypeNameLcaseInteger
                        returnTypeId = FieldTypeIdInteger
                    Case FieldTypeNameLcaseLongText, "longtext", "Long text"
                        returnTypeId = FieldTypeIdLongText
                    Case FieldTypeNameLcaseLookup, "lookuplist", "lookup list"
                        returnTypeId = FieldTypeIdLookup
                    Case FieldTypeNameLcaseMemberSelect
                        returnTypeId = FieldTypeIdMemberSelect
                    Case FieldTypeNameLcaseRedirect
                        returnTypeId = FieldTypeIdRedirect
                    Case FieldTypeNameLcaseManyToMany
                        returnTypeId = FieldTypeIdManyToMany
                    Case FieldTypeNameLcaseTextFile, "text file", "textfile"
                        returnTypeId = FieldTypeIdFileText
                    Case FieldTypeNameLcaseCSSFile, "cssfile", "css file"
                        returnTypeId = FieldTypeIdFileCSS
                    Case FieldTypeNameLcaseXMLFile, "xmlfile", "xml file"
                        returnTypeId = FieldTypeIdFileXML
                    Case FieldTypeNameLcaseJavascriptFile, "javascript file", "javascriptfile", "js file", "jsfile"
                        returnTypeId = FieldTypeIdFileJavascript
                    Case FieldTypeNameLcaseText
                        returnTypeId = FieldTypeIdText
                    Case "autoincrement"
                        returnTypeId = FieldTypeIdAutoIdIncrement
                    Case "memberselect"
                        returnTypeId = FieldTypeIdMemberSelect
                    Case FieldTypeNameLcaseHTML
                        returnTypeId = FieldTypeIdHTML
                    Case FieldTypeNameLcaseHTMLFile, "html file"
                        returnTypeId = FieldTypeIdFileHTML
                    Case Else
                        '
                        ' Bad field type is a text field
                        '
                        returnTypeId = FieldTypeIdText
                End Select
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnTypeId
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Opens a dataTable for the table/row definied by the contentname and criteria
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="Criteria"></param>
        ''' <param name="SortFieldList"></param>
        ''' <param name="ActiveOnly"></param>
        ''' <param name="MemberID"></param>
        ''' <param name="ignorefalse2"></param>
        ''' <param name="ignorefalse"></param>
        ''' <param name="SelectFieldList"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        '========================================================================
        '
        Public Function csOpen(ByVal ContentName As String, Optional ByVal Criteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal MemberID As Integer = 0, Optional ByVal ignorefalse2 As Boolean = False, Optional ByVal ignorefalse As Boolean = False, Optional ByVal SelectFieldList As String = "", Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
            Dim returnCs As Integer = -1
            Try
                Dim SortFields() As String
                Dim SortField As String
                Dim Ptr As Integer
                Dim Cnt As Integer
                Dim ContentCriteria As String
                Dim TableName As String
                Dim DataSourceName As String
                Dim iActiveOnly As Boolean
                Dim iSortFieldList As String
                'Dim iWorkflowRenderingMode As Boolean
                'Dim AllowWorkflowSave As Boolean
                Dim iMemberID As Integer
                Dim iCriteria As String
                Dim iSelectFieldList As String
                Dim CDef As Models.Complex.cdefModel
                Dim TestUcaseFieldList As String
                Dim SQL As String
                '
                If String.IsNullOrEmpty(ContentName) Then
                    Throw New ApplicationException("ContentName cannot be blank")
                Else
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName)
                    If (CDef Is Nothing) Then
                        Throw (New ApplicationException("No content found For [" & ContentName & "]"))
                    ElseIf (CDef.Id <= 0) Then
                        Throw (New ApplicationException("No content found For [" & ContentName & "]"))
                    Else
                        '
                        'hint = hint & ", 100"
                        iActiveOnly = ((ActiveOnly))
                        iSortFieldList = genericController.encodeEmptyText(SortFieldList, CDef.DefaultSortMethod)
                        iMemberID = MemberID
                        iCriteria = genericController.encodeEmptyText(Criteria, "")
                        iSelectFieldList = genericController.encodeEmptyText(SelectFieldList, CDef.SelectCommaList)
                        '
                        ' verify the sortfields are in this table
                        '
                        If iSortFieldList <> "" Then
                            SortFields = Split(iSortFieldList, ",")
                            Cnt = UBound(SortFields) + 1
                            For Ptr = 0 To Cnt - 1
                                SortField = SortFields(Ptr).ToLower
                                SortField = genericController.vbReplace(SortField, "asc", "", 1, 99, vbTextCompare)
                                SortField = genericController.vbReplace(SortField, "desc", "", 1, 99, vbTextCompare)
                                SortField = Trim(SortField)
                                If Not CDef.selectList.Contains(SortField) Then
                                    'throw (New ApplicationException("Unexpected exception"))
                                    Throw (New ApplicationException("The field [" & SortField & "] was used In a sort method For content [" & ContentName & "], but the content does Not include this field."))
                                End If
                            Next
                        End If
                        '
                        ' ----- fixup the criteria to include the ContentControlID(s) / EditSourceID
                        '
                        ContentCriteria = CDef.ContentControlCriteria
                        If ContentCriteria = "" Then
                            ContentCriteria = "(1=1)"
                        Else
                            '
                            ' remove tablename from contentcontrolcriteria - if in workflow mode, and authoringtable is different, this would be wrong
                            ' also makes sql smaller, and is not necessary
                            '
                            ContentCriteria = genericController.vbReplace(ContentCriteria, CDef.ContentTableName & ".", "")
                        End If
                        If iCriteria <> "" Then
                            ContentCriteria = ContentCriteria & "And(" & iCriteria & ")"
                        End If
                        '
                        ' ----- Active Only records
                        '
                        If iActiveOnly Then
                            ContentCriteria = ContentCriteria & "And(Active<>0)"
                            'ContentCriteria = ContentCriteria & "And(" & TableName & ".Active<>0)"
                        End If
                        '
                        ' ----- Process Select Fields, make sure ContentControlID,ID,Name,Active are included
                        '
                        iSelectFieldList = genericController.vbReplace(iSelectFieldList, vbTab, " ")
                        Do While genericController.vbInstr(1, iSelectFieldList, " ,") <> 0
                            iSelectFieldList = genericController.vbReplace(iSelectFieldList, " ,", ",")
                        Loop
                        Do While genericController.vbInstr(1, iSelectFieldList, ", ") <> 0
                            iSelectFieldList = genericController.vbReplace(iSelectFieldList, ", ", ",")
                        Loop
                        If (iSelectFieldList <> "") And (InStr(1, iSelectFieldList, "*", vbTextCompare) = 0) Then
                            TestUcaseFieldList = genericController.vbUCase("," & iSelectFieldList & ",")
                            If genericController.vbInstr(1, TestUcaseFieldList, ",CONTENTCONTROLID,", vbTextCompare) = 0 Then
                                iSelectFieldList = iSelectFieldList & ",ContentControlID"
                            End If
                            If genericController.vbInstr(1, TestUcaseFieldList, ",NAME,", vbTextCompare) = 0 Then
                                iSelectFieldList = iSelectFieldList & ",Name"
                            End If
                            If genericController.vbInstr(1, TestUcaseFieldList, ",ID,", vbTextCompare) = 0 Then
                                iSelectFieldList = iSelectFieldList & ",ID"
                            End If
                            If genericController.vbInstr(1, TestUcaseFieldList, ",ACTIVE,", vbTextCompare) = 0 Then
                                iSelectFieldList = iSelectFieldList & ",ACTIVE"
                            End If
                        End If
                        '
                        TableName = CDef.ContentTableName
                        DataSourceName = CDef.ContentDataSourceName
                        '
                        ' ----- Check for blank Tablename or DataSource
                        '
                        If TableName = "" Then
                            Throw (New Exception("Error opening csv_ContentSet because Content Definition [" & ContentName & "] does Not reference a valid table"))
                        ElseIf DataSourceName = "" Then
                            Throw (New Exception("Error opening csv_ContentSet because Table Definition [" & TableName & "] does Not reference a valid datasource"))
                        End If
                        '
                        ' ----- If no select list, use *
                        '
                        If iSelectFieldList = "" Then
                            iSelectFieldList = "*"
                        End If
                        '
                        ' ----- Open the csv_ContentSet
                        '
                        returnCs = cs_init(iMemberID)
                        With contentSetStore(returnCs)
                            .Updateable = True
                            .ContentName = ContentName
                            .PageNumber = PageNumber
                            If .PageNumber <= 0 Then
                                .PageNumber = 1
                            End If
                            .PageSize = PageSize
                            If .PageSize < 0 Then
                                .PageSize = maxLongValue
                            ElseIf .PageSize = 0 Then
                                .PageSize = pageSizeDefault
                            End If

                            .DataSource = DataSourceName
                            .CDef = CDef
                            .SelectTableFieldList = iSelectFieldList
                            '
                            If iSortFieldList <> "" Then
                                SQL = "Select " & iSelectFieldList & " FROM " & TableName & " WHERE (" & ContentCriteria & ") ORDER BY " & iSortFieldList
                            Else
                                SQL = "Select " & iSelectFieldList & " FROM " & TableName & " WHERE (" & ContentCriteria & ")"
                            End If
                            '
                            If contentSetOpenWithoutRecords Then
                                '
                                ' Save the source, but do not open the recordset
                                '
                                contentSetStore(returnCs).Source = SQL
                            Else
                                '
                                ' Run the query
                                '
                                contentSetStore(returnCs).dt = executeQuery(SQL, DataSourceName, .PageSize * (.PageNumber - 1), .PageSize)
                            End If
                        End With
                        Call cs_initData(returnCs)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnCs
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' csv_DeleteCSRecord
        ''' </summary>
        ''' <param name="CSPointer"></param>
        Public Sub csDeleteRecord(ByVal CSPointer As Integer)
            Try
                '
                Dim LiveRecordID As Integer
                Dim ContentID As Integer
                Dim ContentName As String
                Dim ContentDataSourceName As String
                Dim ContentTableName As String
                Dim SQLName(5) As String
                Dim SQLValue(5) As String
                Dim Filename As String
                '
                If Not csOk(CSPointer) Then
                    '
                    Throw New ArgumentException("csv_ContentSet Is empty Or at End-Of-file")
                ElseIf Not contentSetStore(CSPointer).Updateable Then
                    '
                    Throw New ArgumentException("csv_ContentSet Is Not Updateable")
                Else
                    With contentSetStore(CSPointer).CDef
                        ContentID = .Id
                        ContentName = .Name
                        ContentTableName = .ContentTableName
                        ContentDataSourceName = .ContentDataSourceName
                        If (ContentName = "") Then
                            Throw New ArgumentException("csv_ContentSet Is Not based On a Content Definition")
                        Else
                            LiveRecordID = csGetInteger(CSPointer, "ID")
                            '
                            ' delete any files (only if filename is part of select)
                            '
                            Dim fieldName As String
                            Dim field As Models.Complex.CDefFieldModel
                            For Each selectedFieldName In .selectList
                                If (.fields.ContainsKey(selectedFieldName.ToLower())) Then
                                    field = .fields(selectedFieldName.ToLower())
                                    With field
                                        fieldName = .nameLc
                                        Select Case .fieldTypeId
                                            Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdFileCSS, FieldTypeIdFileJavascript, FieldTypeIdFileXML
                                                '
                                                ' public content files
                                                '
                                                Filename = csGetText(CSPointer, fieldName)
                                                If Filename <> "" Then
                                                    Call cpCore.cdnFiles.deleteFile(Filename)
                                                    'Call cpCore.cdnFiles.deleteFile(cpCore.cdnFiles.joinPath(cpCore.serverConfig.appConfig.cdnFilesNetprefix, Filename))
                                                End If
                                            Case FieldTypeIdFileText, FieldTypeIdFileHTML
                                                '
                                                ' private files
                                                '
                                                Filename = csGetText(CSPointer, fieldName)
                                                If Filename <> "" Then
                                                    Call cpCore.cdnFiles.deleteFile(Filename)
                                                End If
                                        End Select
                                    End With
                                End If
                            Next
                            '
                            ' non-workflow mode, delete the live record
                            '
                            Call deleteTableRecord(ContentTableName, LiveRecordID, ContentDataSourceName)
                            If workflowController.csv_AllowAutocsv_ClearContentTimeStamp Then
                                Call cpCore.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(ContentTableName, "id", LiveRecordID.ToString()))
                                'Call cpCore.cache.invalidateObject(ContentName)
                            End If
                            Call deleteContentRules(ContentID, LiveRecordID)
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Opens a Select SQL into a csv_ContentSet
        '''   Returns and long that points into the csv_ContentSet array
        '''   If there was a problem, it returns -1
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <param name="SQL"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        Public Function csOpenSql_rev(ByVal DataSourceName As String, ByVal SQL As String, Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
            Return csOpenSql(SQL, DataSourceName, PageSize, PageNumber)
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' openSql
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <param name="DataSourceName"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        Public Function csOpenSql(ByVal SQL As String, Optional ByVal DataSourceName As String = "", Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
            Dim returnCs As Integer = -1
            Try
                returnCs = cs_init(cpCore.doc.authContext.user.id)
                With contentSetStore(returnCs)
                    .Updateable = False
                    .ContentName = ""
                    .PageNumber = PageNumber
                    .PageSize = (PageSize)
                    .DataSource = DataSourceName
                    .SelectTableFieldList = ""
                End With
                '
                If useCSReadCacheMultiRow Then
                    contentSetStore(returnCs).dt = executeQuery(SQL, DataSourceName, PageSize * (PageNumber - 1), PageSize)
                    Call cs_initData(returnCs)
                    'Call cs_loadCurrentRow(returnCs)
                Else
                    contentSetStore(returnCs).dt = executeQuery(SQL, DataSourceName, PageSize * (PageNumber - 1), PageSize)
                    Call cs_initData(returnCs)
                    'Call cs_loadCurrentRow(returnCs)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnCs
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' initialize a cs
        ''' </summary>
        ''' <param name="MemberID"></param>
        ''' <returns></returns>
        Private Function cs_init(ByVal MemberID As Integer) As Integer
            Dim returnCs As Integer = -1
            Try
                Dim ptr As Integer
                '
                If contentSetStoreCount > 0 Then
                    For ptr = 1 To contentSetStoreCount
                        If Not contentSetStore(ptr).IsOpen Then
                            '
                            ' Open CS found
                            '
                            returnCs = ptr
                            Exit For
                        End If
                    Next
                End If
                '
                If returnCs = -1 Then
                    If contentSetStoreCount >= contentSetStoreSize Then
                        contentSetStoreSize = contentSetStoreSize + contentSetStoreChunk
                        ReDim Preserve contentSetStore(contentSetStoreSize + 1)
                    End If
                    contentSetStoreCount = contentSetStoreCount + 1
                    returnCs = contentSetStoreCount
                End If
                '
                With contentSetStore(returnCs)
                    .IsOpen = True
                    '.WorkflowAuthoringMode = False
                    .ContentName = ""
                    .NewRecord = True
                    '.writeCacheSize = 0
                    '.writeCacheCount = 0
                    .Updateable = False
                    .IsModified = False
                    .OwnerMemberID = MemberID
                    .LastUsed = DateTime.Now
                End With
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnCs
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Close a csv_ContentSet
        ''' Closes a currently open csv_ContentSet
        ''' sets CSPointer to -1
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="AsyncSave"></param>
        Public Sub csClose(ByRef CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False)
            Try
                If (CSPointer > 0) And (CSPointer <= contentSetStoreCount) Then
                    With contentSetStore(CSPointer)
                        If .IsOpen Then
                            Call csSave2(CSPointer, AsyncSave)
                            ReDim .readCache(0, 0)
                            ReDim .fieldNames(0)
                            .ResultColumnCount = 0
                            .readCacheRowCnt = 0
                            .readCacheRowPtr = -1
                            .ResultEOF = True
                            .IsOpen = False
                            If (Not .dt Is Nothing) Then
                                .dt.Dispose()
                            End If
                        End If
                    End With
                    CSPointer = -1
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ' Move the csv_ContentSet to the next row
        '========================================================================
        '
        Public Sub csGoNext(ByVal CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False)
            Try
                '
                Dim ContentName As String
                Dim RecordID As Integer
                '
                If Not csOk(CSPointer) Then
                    '
                    Throw New ApplicationException("CSPointer Not csv_IsCSOK.")
                Else
                    Call csSave2(CSPointer, AsyncSave)
                    'contentSetStore(CSPointer).WorkflowEditingMode = False
                    '
                    ' Move to next row
                    '
                    contentSetStore(CSPointer).readCacheRowPtr = contentSetStore(CSPointer).readCacheRowPtr + 1
                    If Not cs_IsEOF(CSPointer) Then
                        '
                        ' Not EOF
                        '
                        ' Call cs_loadCurrentRow(CSPointer)
                        '
                        ' Set Workflow Edit Mode from Request and EditLock state
                        '
                        If (Not cs_IsEOF(CSPointer)) Then
                            ContentName = contentSetStore(CSPointer).ContentName
                            RecordID = csGetInteger(CSPointer, "ID")
                            If Not cpCore.workflow.isRecordLocked(ContentName, RecordID, contentSetStore(CSPointer).OwnerMemberID) Then
                                Call cpCore.workflow.setEditLock(ContentName, RecordID, contentSetStore(CSPointer).OwnerMemberID)
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ' Move the csv_ContentSet to the first row
        '========================================================================
        '
        Public Sub cs_goFirst(ByVal CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False)
            Try
                If Not csOk(CSPointer) Then
                    Throw New ApplicationException("data set is not valid")
                Else
                    Call csSave2(CSPointer, AsyncSave)
                    contentSetStore(CSPointer).readCacheRowPtr = 0
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' getField returns a value from a nameValue dataset specified by the cs pointer. get the value of a field within a csv_ContentSet,  if CS in authoring mode, it gets the edit record value, except ID field. otherwise, it gets the live record value.
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public Function cs_getValue(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            Dim returnValue As String = ""
            Try
                Dim fieldFound As Boolean
                Dim ColumnPointer As Integer
                Dim fieldNameTrimUpper As String
                Dim fieldNameTrim As String
                '
                fieldNameTrim = FieldName.Trim()
                fieldNameTrimUpper = genericController.vbUCase(fieldNameTrim)
                If Not csOk(CSPointer) Then
                    Throw New ApplicationException("Attempt To GetValue fieldname[" & FieldName & "], but the dataset Is empty Or does Not point To a valid row")
                Else
                    With contentSetStore(CSPointer)
                        '
                        '
                        fieldFound = False
                        If .writeCache.Count > 0 Then
                            '
                            ' ----- something has been set in buffer, check it first
                            '
                            If .writeCache.ContainsKey(FieldName.ToLower) Then
                                returnValue = .writeCache.Item(FieldName.ToLower)
                                fieldFound = True
                            End If
                        End If
                        If Not fieldFound Then
                            '
                            ' ----- attempt read from readCache
                            '
                            If useCSReadCacheMultiRow Then
                                If Not .dt.Columns.Contains(FieldName.ToLower) Then
                                    If (.Updateable) Then
                                        Throw New ApplicationException("Field [" & fieldNameTrim & "] was Not found in [" & .ContentName & "] with selected fields [" & .SelectTableFieldList & "]")
                                    Else
                                        Throw New ApplicationException("Field [" & fieldNameTrim & "] was Not found in sql [" & .Source & "]")
                                    End If
                                Else
                                    returnValue = genericController.encodeText(.dt.Rows(.readCacheRowPtr).Item(FieldName.ToLower))
                                End If
                            Else
                                '
                                ' ----- read the value from the Recordset Result
                                '
                                If .ResultColumnCount > 0 Then
                                    For ColumnPointer = 0 To .ResultColumnCount - 1
                                        If .fieldNames(ColumnPointer) = fieldNameTrimUpper Then
                                            returnValue = .readCache(ColumnPointer, 0)
                                            If .Updateable And (.ContentName <> "") And (FieldName <> "") Then
                                                If .CDef.fields(FieldName.ToLower()).Scramble Then
                                                    returnValue = genericController.TextDeScramble(cpCore, genericController.encodeText(returnValue))
                                                End If
                                            End If
                                            Exit For
                                        End If
                                    Next
                                    If ColumnPointer = .ResultColumnCount Then
                                        Throw New ApplicationException("Field [" & fieldNameTrim & "] was Not found In csv_ContentSet from source [" & .Source & "]")
                                    End If
                                End If
                            End If
                        End If
                        .LastUsed = DateTime.Now
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnValue
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get the first fieldname in the CS, Returns null if there are no more
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        Function cs_getFirstFieldName(ByVal CSPointer As Integer) As String
            Dim returnFieldName As String = ""
            Try
                If Not csOk(CSPointer) Then
                    Throw New ApplicationException("data set is not valid")
                Else
                    contentSetStore(CSPointer).fieldPointer = 0
                    returnFieldName = cs_getNextFieldName(CSPointer)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnFieldName
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get the next fieldname in the CS, Returns null if there are no more
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        Function cs_getNextFieldName(ByVal CSPointer As Integer) As String
            Dim returnFieldName As String = ""
            Try
                If Not csOk(CSPointer) Then
                    Throw New ApplicationException("data set is not valid")
                Else
                    With contentSetStore(CSPointer)
                        If useCSReadCacheMultiRow Then
                            Do While (returnFieldName = "") And (.fieldPointer < .ResultColumnCount)
                                returnFieldName = .fieldNames(.fieldPointer)
                                .fieldPointer = .fieldPointer + 1
                            Loop
                        Else
                            Do While (returnFieldName = "") And (.fieldPointer < .dt.Columns.Count)
                                returnFieldName = .dt.Columns(.fieldPointer).ColumnName
                                .fieldPointer = .fieldPointer + 1
                            Loop
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnFieldName
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get the type of a field within a csv_ContentSet
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public Function cs_getFieldTypeId(ByVal CSPointer As Integer, ByVal FieldName As String) As Integer
            Dim returnFieldTypeid As Integer = 0
            Try
                If csOk(CSPointer) Then
                    If contentSetStore(CSPointer).Updateable Then
                        With contentSetStore(CSPointer).CDef
                            If .Name <> "" Then
                                returnFieldTypeid = .fields(FieldName.ToLower()).fieldTypeId
                            End If
                        End With
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnFieldTypeid
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get the caption of a field within a csv_ContentSet
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public Function cs_getFieldCaption(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            Dim returnResult As String = ""
            Try
                If csOk(CSPointer) Then
                    If contentSetStore(CSPointer).Updateable Then
                        With contentSetStore(CSPointer).CDef
                            If .Name <> "" Then
                                returnResult = .fields(FieldName.ToLower()).caption
                                If String.IsNullOrEmpty(returnResult) Then
                                    returnResult = FieldName
                                End If
                            End If
                        End With
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get a list of captions of fields within a data set
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        Public Function cs_getSelectFieldList(ByVal CSPointer As Integer) As String
            Dim returnResult As String = ""
            Try
                If csOk(CSPointer) Then
                    If useCSReadCacheMultiRow Then
                        returnResult = Join(contentSetStore(CSPointer).fieldNames, ",")
                    Else
                        returnResult = contentSetStore(CSPointer).SelectTableFieldList
                        If String.IsNullOrEmpty(returnResult) Then
                            With contentSetStore(CSPointer)
                                If Not (.dt Is Nothing) Then
                                    If .dt.Columns.Count > 0 Then
                                        For FieldPointer = 0 To .dt.Columns.Count - 1
                                            returnResult = returnResult & "," & .dt.Columns(FieldPointer).ColumnName
                                        Next
                                        returnResult = Mid(returnResult, 2)
                                    End If
                                End If
                            End With
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get the caption of a field within a csv_ContentSet
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public Function cs_isFieldSupported(ByVal CSPointer As Integer, ByVal FieldName As String) As Boolean
            Dim returnResult As Boolean = False
            Try
                If String.IsNullOrEmpty(FieldName) Then
                    Throw New ArgumentException("Field name cannot be blank")
                ElseIf Not csOk(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    Dim CSSelectFieldList As String = cs_getSelectFieldList(CSPointer)
                    returnResult = genericController.IsInDelimitedString(CSSelectFieldList, FieldName, ",")
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get the filename that backs the field specified. only valid for fields of TextFile and File type.
        ''' Attempt to read the filename from the field
        ''' if no filename, attempt to create it from the tablename-recordid
        ''' if no recordid, create filename from a random
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="FieldName"></param>
        ''' <param name="OriginalFilename"></param>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        Public Function csGetFilename(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal OriginalFilename As String, Optional ByVal ContentName As String = "", Optional fieldTypeId As Integer = 0) As String
            Dim returnFilename As String = ""
            Try
                Dim TableName As String
                Dim RecordID As Integer
                Dim fieldNameUpper As String
                Dim LenOriginalFilename As Integer
                Dim LenFilename As Integer
                Dim Pos As Integer
                '
                If Not csOk(CSPointer) Then
                    Throw New ArgumentException("CSPointer does Not point To a valid dataset, it Is empty, Or it Is Not pointing To a valid row.")
                ElseIf FieldName = "" Then
                    Throw New ArgumentException("Fieldname Is blank")
                Else
                    fieldNameUpper = genericController.vbUCase(Trim(FieldName))
                    returnFilename = cs_getValue(CSPointer, fieldNameUpper)
                    If returnFilename <> "" Then
                        '
                        ' ----- A filename came from the record
                        '
                        If OriginalFilename <> "" Then
                            '
                            ' ----- there was an original filename, make sure it matches the one in the record
                            '
                            LenOriginalFilename = OriginalFilename.Length()
                            LenFilename = returnFilename.Length()
                            Pos = (1 + LenFilename - LenOriginalFilename)
                            If Pos <= 0 Then
                                '
                                ' Original Filename changed, create a new csv_cs_getFilename
                                '
                                returnFilename = ""
                            ElseIf Mid(returnFilename, Pos) <> OriginalFilename Then
                                '
                                ' Original Filename changed, create a new csv_cs_getFilename
                                '
                                returnFilename = ""
                            End If
                        End If
                    End If
                    If returnFilename = "" Then
                        With contentSetStore(CSPointer)
                            '
                            ' ----- no filename present, get id field
                            '
                            If .ResultColumnCount > 0 Then
                                For FieldPointer = 0 To .ResultColumnCount - 1
                                    If genericController.vbUCase(.fieldNames(FieldPointer)) = "ID" Then
                                        RecordID = csGetInteger(CSPointer, "ID")
                                        Exit For
                                    End If
                                Next
                            End If
                            '
                            ' ----- Get tablename
                            '
                            If .Updateable Then
                                '
                                ' Get tablename from Content Definition
                                '
                                ContentName = .CDef.Name
                                TableName = .CDef.ContentTableName
                            ElseIf ContentName <> "" Then
                                '
                                ' CS is SQL-based, use the contentname
                                '
                                TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName)
                            Else
                                '
                                ' no Contentname given
                                '
                                Throw New ApplicationException("Can Not create a filename because no ContentName was given, And the csv_ContentSet Is SQL-based.")
                            End If
                            '
                            ' ----- Create filename
                            '
                            If fieldTypeId = 0 Then
                                If ContentName = "" Then
                                    If OriginalFilename = "" Then
                                        fieldTypeId = FieldTypeIdText
                                    Else
                                        fieldTypeId = FieldTypeIdFile
                                    End If
                                ElseIf (.Updateable) Then
                                    '
                                    ' -- get from cdef
                                    fieldTypeId = .CDef.fields(FieldName.ToLower()).fieldTypeId
                                Else
                                    '
                                    ' -- else assume text
                                    If OriginalFilename = "" Then
                                        fieldTypeId = FieldTypeIdText
                                    Else
                                        fieldTypeId = FieldTypeIdFile
                                    End If
                                End If
                            End If
                            If (OriginalFilename = "") Then
                                returnFilename = fileController.getVirtualRecordPathFilename(TableName, FieldName, RecordID, fieldTypeId)
                            Else
                                returnFilename = fileController.getVirtualRecordPathFilename(TableName, FieldName, RecordID, OriginalFilename)
                            End If
                            ' 20160607 - no, if you call the cs_set, it stack-overflows. this is a get, so do not save it here.
                            'Call cs_set(CSPointer, fieldNameUpper, returnFilename)
                        End With
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnFilename
        End Function
        '
        '   csv_cs_getText
        '
        Public Function csGetText(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            csGetText = genericController.encodeText(cs_getValue(CSPointer, FieldName))
        End Function
        '
        '   genericController.EncodeInteger( csv_cs_getField )
        '
        Public Function csGetInteger(ByVal CSPointer As Integer, ByVal FieldName As String) As Integer
            csGetInteger = genericController.EncodeInteger(cs_getValue(CSPointer, FieldName))
        End Function
        '
        '   encodeNumber( csv_cs_getField )
        '
        Public Function csGetNumber(ByVal CSPointer As Integer, ByVal FieldName As String) As Double
            csGetNumber = genericController.EncodeNumber(cs_getValue(CSPointer, FieldName))
        End Function
        '
        '    genericController.EncodeDate( csv_cs_getField )
        '
        Public Function csGetDate(ByVal CSPointer As Integer, ByVal FieldName As String) As Date
            csGetDate = genericController.EncodeDate(cs_getValue(CSPointer, FieldName))
        End Function
        '
        '   genericController.EncodeBoolean( csv_cs_getField )
        '
        Public Function csGetBoolean(ByVal CSPointer As Integer, ByVal FieldName As String) As Boolean
            csGetBoolean = genericController.EncodeBoolean(cs_getValue(CSPointer, FieldName))
        End Function
        '
        '   genericController.EncodeBoolean( csv_cs_getField )
        '
        Public Function csGetLookup(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            csGetLookup = csGet(CSPointer, FieldName)
        End Function
        '
        '====================================================================================
        ' Set a csv_ContentSet Field value for a TextFile fieldtype
        '   Saves the value in a file and saves the filename in the field
        '
        '   CSPointer   The current Content Set Pointer
        '   FieldName   The name of the field to be saved
        '   Copy        Literal string to be saved in the field
        '   ContentName Contentname for the field to be saved
        '====================================================================================
        '
        Public Sub csSetTextFile(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal Copy As String, ByVal ContentName As String)
            Try
                If Not csOk(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid")
                ElseIf String.IsNullOrEmpty(FieldName) Then
                    Throw New ArgumentException("fieldName cannot be blank")
                ElseIf String.IsNullOrEmpty(ContentName) Then
                    Throw New ArgumentException("contentName cannot be blank")
                Else
                    With contentSetStore(CSPointer)
                        If Not .Updateable Then
                            Throw New ApplicationException("Attempting To update an unupdateable data set")
                        Else
                            Dim OldFilename As String = csGetText(CSPointer, FieldName)
                            Dim Filename As String = csGetFilename(CSPointer, FieldName, "", ContentName, FieldTypeIdFileText)
                            If OldFilename <> Filename Then
                                '
                                ' Filename changed, mark record changed
                                '
                                Call cpCore.cdnFiles.saveFile(Filename, Copy)
                                Call csSet(CSPointer, FieldName, Filename)
                            Else
                                Dim OldCopy As String = cpCore.cdnFiles.readFile(Filename)
                                If OldCopy <> Copy Then
                                    '
                                    ' copy changed, mark record changed
                                    '
                                    Call cpCore.cdnFiles.saveFile(Filename, Copy)
                                    Call csSet(CSPointer, FieldName, Filename)
                                End If
                            End If
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' ContentServer version of getDataRowColumnName
        ''' </summary>
        ''' <param name="dr"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public Function getDataRowColumnName(ByVal dr As DataRow, ByVal FieldName As String) As String
            Dim result As String = ""
            Try
                If String.IsNullOrEmpty(FieldName) Then
                    Throw New ArgumentException("fieldname cannot be blank")
                Else
                    result = dr.Item(FieldName).ToString
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' InsertContentRecordGetID
        ''' Inserts a record based on a content definition.
        ''' Returns the ID of the record, -1 if error
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="MemberID"></param>
        ''' <returns></returns>
        '''
        Public Function insertContentRecordGetID(ByVal ContentName As String, ByVal MemberID As Integer) As Integer
            Dim result As Integer = -1
            Try
                Dim CS As Integer = csInsertRecord(ContentName, MemberID)
                If Not csOk(CS) Then
                    Call csClose(CS)
                    Throw New ApplicationException("could not insert record in content [" & ContentName & "]")
                Else
                    result = csGetInteger(CS, "ID")
                End If
                Call csClose(CS)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Delete Content Record
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="RecordID"></param>
        ''' <param name="MemberID"></param>
        '
        Public Sub deleteContentRecord(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal MemberID As Integer = SystemMemberID)
            Try
                If String.IsNullOrEmpty(ContentName) Then
                    Throw New ArgumentException("contentname cannot be blank")
                ElseIf (RecordID <= 0) Then
                    Throw New ArgumentException("recordId must be positive value")
                Else
                    Dim CSPointer As Integer = cs_openContentRecord(ContentName, RecordID, MemberID, True, True)
                    If csOk(CSPointer) Then
                        Call csDeleteRecord(CSPointer)
                    End If
                    Call csClose(CSPointer)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' 'deleteContentRecords
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="Criteria"></param>
        ''' <param name="MemberID"></param>
        '
        Public Sub deleteContentRecords(ByVal ContentName As String, ByVal Criteria As String, Optional ByVal MemberID As Integer = 0)
            Try
                '
                Dim CSPointer As Integer
                Dim CDef As Models.Complex.cdefModel
                '
                If String.IsNullOrEmpty(ContentName.Trim()) Then
                    Throw New ArgumentException("contentName cannot be blank")
                ElseIf String.IsNullOrEmpty(Criteria.Trim()) Then
                    Throw New ArgumentException("criteria cannot be blank")
                Else
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName)
                    If CDef Is Nothing Then
                        Throw New ArgumentException("ContentName [" & ContentName & "] was Not found")
                    ElseIf CDef.Id = 0 Then
                        Throw New ArgumentException("ContentName [" & ContentName & "] was Not found")
                    Else
                        '
                        ' -- treat all deletes one at a time to invalidate the primary cache
                        ' another option is invalidate the entire table (tablename-invalidate), but this also has performance problems
                        '
                        Dim invaldiateObjectList As New List(Of String)
                        CSPointer = csOpen(ContentName, Criteria, , False, MemberID, True, True)
                        Do While csOk(CSPointer)
                            invaldiateObjectList.Add(Controllers.cacheController.getCacheKey_Entity(CDef.ContentTableName, "id", csGetInteger(CSPointer, "id").ToString()))
                            Call csDeleteRecord(CSPointer)
                            Call csGoNext(CSPointer)
                        Loop
                        Call csClose(CSPointer)
                        Call cpCore.cache.invalidateContent(invaldiateObjectList)

                        '    ElseIf cpCore.siteProperties.allowWorkflowAuthoring And (false) Then
                        '    '
                        '    ' Supports Workflow Authoring, handle it record at a time
                        '    '
                        '    CSPointer = cs_open(ContentName, Criteria, , False, MemberID, True, True, "ID")
                        '    Do While cs_ok(CSPointer)
                        '        Call cs_deleteRecord(CSPointer)
                        '        Call cs_goNext(CSPointer)
                        '    Loop
                        '    Call cs_Close(CSPointer)
                        'Else
                        '    '
                        '    ' No Workflow Authoring, just delete records
                        '    '
                        '    Call DeleteTableRecords(CDef.ContentTableName, "(" & Criteria & ") And (" & CDef.ContentControlCriteria & ")", CDef.ContentDataSourceName)
                        '    If coreWorkflowClass.csv_AllowAutocsv_ClearContentTimeStamp Then
                        '        Call cpCore.cache.invalidateObject(CDef.ContentTableName & "-invalidate")
                        '    End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Inserts a record in a content definition and returns a csv_ContentSet with just that record
        ''' If there was a problem, it returns -1
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="MemberID"></param>
        ''' <returns></returns>
        Public Function csInsertRecord(ByVal ContentName As String, Optional ByVal MemberID As Integer = -1) As Integer
            Dim returnCs As Integer = -1
            Try
                Dim DateAddedString As String
                Dim CreateKeyString As String
                Dim Criteria As String
                Dim DataSourceName As String
                Dim FieldName As String
                Dim TableName As String
                Dim CDef As Models.Complex.cdefModel
                Dim DefaultValueText As String
                Dim LookupContentName As String
                Dim Ptr As Integer
                Dim lookups() As String
                Dim UCaseDefaultValueText As String
                Dim sqlList As New sqlFieldListClass
                '
                If String.IsNullOrEmpty(ContentName.Trim()) Then
                    Throw New ArgumentException("ContentName cannot be blank")
                Else
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName)
                    If (CDef Is Nothing) Then
                        Throw New ApplicationException("content [" & ContentName & "] could Not be found.")
                    ElseIf (CDef.Id <= 0) Then
                        Throw New ApplicationException("content [" & ContentName & "] could Not be found.")
                    Else
                        If MemberID = -1 Then
                            MemberID = cpCore.doc.authContext.user.id
                        End If
                        With CDef
                            '
                            ' no authoring, create default record in Live table
                            '
                            DataSourceName = .ContentDataSourceName
                            TableName = .ContentTableName
                            If .fields.Count > 0 Then
                                For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In .fields
                                    Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                                    With field
                                        FieldName = .nameLc
                                        If (FieldName <> "") And (Not String.IsNullOrEmpty(.defaultValue)) Then
                                            Select Case genericController.vbUCase(FieldName)
                                                Case "CREATEKEY", "DATEADDED", "CREATEDBY", "CONTENTCONTROLID", "ID"
                                                    '
                                                    ' Block control fields
                                                    '
                                                Case Else
                                                    '
                                                    ' General case
                                                    '
                                                    Select Case .fieldTypeId
                                                        Case FieldTypeIdAutoIdIncrement
                                                            '
                                                            ' cannot insert an autoincremnt
                                                            '
                                                        Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                                            '
                                                            ' ignore these fields, they have no associated DB field
                                                            '
                                                        Case FieldTypeIdBoolean
                                                            sqlList.add(FieldName, encodeSQLBoolean(genericController.EncodeBoolean(.defaultValue)))
                                                        Case FieldTypeIdCurrency, FieldTypeIdFloat
                                                            sqlList.add(FieldName, encodeSQLNumber(genericController.EncodeNumber(.defaultValue)))
                                                        Case FieldTypeIdInteger, FieldTypeIdMemberSelect
                                                            sqlList.add(FieldName, encodeSQLNumber(genericController.EncodeInteger(.defaultValue)))
                                                        Case FieldTypeIdDate
                                                            sqlList.add(FieldName, encodeSQLDate(genericController.EncodeDate(.defaultValue)))
                                                        Case FieldTypeIdLookup
                                                            '
                                                            ' refactor --
                                                            ' This is a problem - the defaults should come in as the ID values, not the names
                                                            '   so a select can be added to the default configuration page
                                                            '
                                                            DefaultValueText = genericController.encodeText(.defaultValue)
                                                            If DefaultValueText = "" Then
                                                                DefaultValueText = "null"
                                                            Else
                                                                If .lookupContentID <> 0 Then
                                                                    LookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, .lookupContentID)
                                                                    If LookupContentName <> "" Then
                                                                        DefaultValueText = getRecordID(LookupContentName, DefaultValueText).ToString()
                                                                    End If
                                                                ElseIf .lookupList <> "" Then
                                                                    UCaseDefaultValueText = genericController.vbUCase(DefaultValueText)
                                                                    lookups = Split(.lookupList, ",")
                                                                    For Ptr = 0 To UBound(lookups)
                                                                        If UCaseDefaultValueText = genericController.vbUCase(lookups(Ptr)) Then
                                                                            DefaultValueText = (Ptr + 1).ToString()
                                                                        End If
                                                                    Next
                                                                End If
                                                            End If
                                                            sqlList.add(FieldName, DefaultValueText)
                                                        Case Else
                                                            '
                                                            ' else text
                                                            '
                                                            sqlList.add(FieldName, encodeSQLText(.defaultValue))
                                                    End Select
                                            End Select
                                        End If
                                    End With
                                Next
                            End If
                            '
                            CreateKeyString = encodeSQLNumber(genericController.GetRandomInteger)
                            DateAddedString = encodeSQLDate(Now)
                            '
                            Call sqlList.add("CREATEKEY", CreateKeyString) ' ArrayPointer)
                            Call sqlList.add("DATEADDED", DateAddedString) ' ArrayPointer)
                            Call sqlList.add("CONTENTCONTROLID", encodeSQLNumber(CDef.Id)) ' ArrayPointer)
                            Call sqlList.add("CREATEDBY", encodeSQLNumber(MemberID)) ' ArrayPointer)
                            '
                            Call insertTableRecord(DataSourceName, TableName, sqlList)
                            '
                            ' ----- Get the record back so we can use the ID
                            '
                            Criteria = "((createkey=" & CreateKeyString & ")And(DateAdded=" & DateAddedString & "))"
                            returnCs = csOpen(ContentName, Criteria, "ID DESC", False, MemberID, False, True)
                            ''
                            '' ----- Clear Time Stamp because a record changed
                            ''
                            'If coreWorkflowClass.csv_AllowAutocsv_ClearContentTimeStamp Then
                            '    Call cpCore.cache.invalidateObject(ContentName)
                            'End If
                        End With
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnCs
        End Function        '
        '========================================================================
        ' Opens a Content Record
        '   If there was a problem, it returns -1 (not csv_IsCSOK)
        '   Can open either the ContentRecord or the AuthoringRecord (WorkflowAuthoringMode)
        '   Isolated in API so later we can save record in an Index buffer for fast access
        '========================================================================
        '
        Public Function cs_openContentRecord(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal MemberID As Integer = SystemMemberID, Optional ByVal WorkflowAuthoringMode As Boolean = False, Optional ByVal WorkflowEditingMode As Boolean = False, Optional ByVal SelectFieldList As String = "") As Integer
            Dim returnResult As Integer = -1
            Try
                If (RecordID <= 0) Then
                    ' no error, return -1 - Throw New ArgumentException("recordId is not valid [" & RecordID & "]")
                Else
                    returnResult = csOpen(ContentName, "(ID=" & encodeSQLNumber(RecordID) & ")", , False, MemberID, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' true if csPointer is a valid dataset, and currently points to a valid row
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        Function csOk(ByVal CSPointer As Integer) As Boolean
            Dim returnResult As Boolean = False
            Try
                If CSPointer < 0 Then
                    returnResult = False
                ElseIf (CSPointer >= contentSetStore.Count) Then
                    Throw New ArgumentException("dateset is not valid")
                Else
                    With contentSetStore(CSPointer)
                        returnResult = .IsOpen And (.readCacheRowPtr >= 0) And (.readCacheRowPtr < .readCacheRowCnt)
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' copy the current row of the source dataset to the destination dataset. The destination dataset must have been created with cs open or insert, and must contain all the fields in the source dataset.
        ''' </summary>
        ''' <param name="CSSource"></param>
        ''' <param name="CSDestination"></param>
        '========================================================================
        '
        Public Sub csCopyRecord(ByVal CSSource As Integer, ByVal CSDestination As Integer)
            Try
                Dim FieldName As String
                Dim DestContentName As String
                Dim DestRecordID As Integer
                Dim DestFilename As String
                Dim SourceFilename As String
                Dim DestCDef As Models.Complex.cdefModel
                '
                If Not csOk(CSSource) Then
                    Throw New ArgumentException("source dataset is not valid")
                ElseIf Not csOk(CSDestination) Then
                    Throw New ArgumentException("destination dataset is not valid")
                ElseIf (contentSetStore(CSDestination).CDef Is Nothing) Then
                    Throw New ArgumentException("copyRecord requires the destination dataset to be created from a cs Open or Insert, not a query.")
                Else
                    DestCDef = contentSetStore(CSDestination).CDef
                    DestContentName = DestCDef.Name
                    DestRecordID = csGetInteger(CSDestination, "ID")
                    FieldName = cs_getFirstFieldName(CSSource)
                    Do While (Not String.IsNullOrEmpty(FieldName))
                        Select Case genericController.vbUCase(FieldName)
                            Case "ID"
                            Case Else
                                '
                                ' ----- fields to copy
                                '
                                Dim sourceFieldTypeId As Integer = cs_getFieldTypeId(CSSource, FieldName)
                                Select Case sourceFieldTypeId
                                    Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                    Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript
                                        '
                                        ' ----- cdn file
                                        '
                                        SourceFilename = csGetFilename(CSSource, FieldName, "", contentSetStore(CSDestination).CDef.Name, sourceFieldTypeId)
                                        'SourceFilename = (csv_cs_getText(CSSource, FieldName))
                                        If (SourceFilename <> "") Then
                                            DestFilename = csGetFilename(CSDestination, FieldName, "", DestContentName, sourceFieldTypeId)
                                            'DestFilename = csv_GetVirtualFilename(DestContentName, FieldName, DestRecordID)
                                            Call csSet(CSDestination, FieldName, DestFilename)
                                            Call cpCore.cdnFiles.copyFile(SourceFilename, DestFilename)
                                        End If
                                    Case FieldTypeIdFileText, FieldTypeIdFileHTML
                                        '
                                        ' ----- private file
                                        '
                                        SourceFilename = csGetFilename(CSSource, FieldName, "", DestContentName, sourceFieldTypeId)
                                        'SourceFilename = (csv_cs_getText(CSSource, FieldName))
                                        If (SourceFilename <> "") Then
                                            DestFilename = csGetFilename(CSDestination, FieldName, "", DestContentName, sourceFieldTypeId)
                                            'DestFilename = csv_GetVirtualFilename(DestContentName, FieldName, DestRecordID)
                                            Call csSet(CSDestination, FieldName, DestFilename)
                                            Call cpCore.cdnFiles.copyFile(SourceFilename, DestFilename)
                                        End If
                                    Case Else
                                        '
                                        ' ----- value
                                        '
                                        Call csSet(CSDestination, FieldName, cs_getValue(CSSource, FieldName))
                                End Select
                        End Select
                        FieldName = cs_getNextFieldName(CSSource)
                    Loop
                    Call csSave2(CSDestination)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub   '
        '       
        '========================================================================
        ''' <summary>
        ''' Returns the Source for the csv_ContentSet
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        Public Function csGetSource(ByVal CSPointer As Integer) As String
            Dim returnResult As String = ""
            Try
                If Not csOk(CSPointer) Then
                    Throw New ArgumentException("the dataset is not valid")
                Else
                    returnResult = contentSetStore(CSPointer).Source
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Returns the value of a field, decoded into a text string result, if there is a problem, null is returned, this may be because the lookup record is inactive, so its not an error
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        '
        Public Function csGet(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            Dim fieldValue As String = ""
            Try
                Dim FieldValueInteger As Integer
                Dim LookupContentName As String
                Dim LookupList As String
                Dim lookups() As String
                Dim FieldValueVariant As Object
                Dim CSLookup As Integer
                Dim fieldTypeId As Integer
                Dim fieldLookupId As Integer
                '
                ' ----- needs work. Go to fields table and get field definition
                '       then print accordingly
                '
                If Not csOk(CSPointer) Then
                    Throw New ArgumentException("the dataset is not valid")
                ElseIf String.IsNullOrEmpty(FieldName.Trim()) Then
                    Throw New ArgumentException("fieldname cannot be blank")
                Else
                    '
                    ' csv_ContentSet good
                    '
                    With contentSetStore(CSPointer)
                        If Not .Updateable Then
                            '
                            ' Not updateable -- Just return what is there as a string
                            '
                            Try
                                fieldValue = genericController.encodeText(cs_getValue(CSPointer, FieldName))
                            Catch ex As Exception
                                Throw New ApplicationException("Error [" & ex.Message & "] reading field [" & FieldName.ToLower & "] In source [" & .Source & "")
                            End Try
                        Else
                            '
                            ' Updateable -- enterprete the value
                            '
                            'ContentName = .ContentName
                            Dim field As Models.Complex.CDefFieldModel
                            If Not .CDef.fields.ContainsKey(FieldName.ToLower()) Then
                                Try
                                    fieldValue = genericController.encodeText(cs_getValue(CSPointer, FieldName))
                                Catch ex As Exception
                                    Throw New ApplicationException("Error [" & ex.Message & "] reading field [" & FieldName.ToLower & "] In content [" & .CDef.Name & "] With custom field list [" & .SelectTableFieldList & "")
                                End Try
                            Else
                                field = .CDef.fields(FieldName.ToLower)
                                fieldTypeId = field.fieldTypeId
                                If fieldTypeId = FieldTypeIdManyToMany Then
                                    '
                                    ' special case - recordset contains no data - return record id list
                                    '
                                    Dim RecordID As Integer
                                    Dim DbTable As String
                                    Dim ContentName As String
                                    Dim SQL As String
                                    Dim rs As DataTable
                                    If .CDef.fields.ContainsKey("id") Then
                                        RecordID = genericController.EncodeInteger(cs_getValue(CSPointer, "id"))
                                        With field
                                            ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, .manyToManyRuleContentID)
                                            DbTable = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName)
                                            SQL = "Select " & .ManyToManyRuleSecondaryField & " from " & DbTable & " where " & .ManyToManyRulePrimaryField & "=" & RecordID
                                            rs = executeQuery(SQL)
                                            If (genericController.isDataTableOk(rs)) Then
                                                For Each dr As DataRow In rs.Rows
                                                    fieldValue &= "," & dr.Item(0).ToString
                                                Next
                                                fieldValue = fieldValue.Substring(1)
                                            End If
                                        End With
                                    End If
                                ElseIf fieldTypeId = FieldTypeIdRedirect Then
                                    '
                                    ' special case - recordset contains no data - return blank
                                    '
                                    fieldTypeId = fieldTypeId
                                Else
                                    FieldValueVariant = cs_getValue(CSPointer, FieldName)
                                    If Not genericController.IsNull(FieldValueVariant) Then
                                        '
                                        ' Field is good
                                        '
                                        Select Case fieldTypeId
                                            Case FieldTypeIdBoolean
                                                '
                                                '
                                                '
                                                If genericController.EncodeBoolean(FieldValueVariant) Then
                                                    fieldValue = "Yes"
                                                Else
                                                    fieldValue = "No"
                                                End If
                                            'NeedsHTMLEncode = False
                                            Case FieldTypeIdDate
                                                '
                                                '
                                                '
                                                If IsDate(FieldValueVariant) Then
                                                    '
                                                    ' formatdatetime returns 'wednesday june 5, 1990', which fails IsDate()!!
                                                    '
                                                    fieldValue = genericController.EncodeDate(FieldValueVariant).ToString()
                                                End If
                                            Case FieldTypeIdLookup
                                                '
                                                '
                                                '
                                                If genericController.vbIsNumeric(FieldValueVariant) Then
                                                    fieldLookupId = field.lookupContentID
                                                    LookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, fieldLookupId)
                                                    LookupList = field.lookupList
                                                    If (LookupContentName <> "") Then
                                                        '
                                                        ' -- First try Lookup Content
                                                        CSLookup = csOpen(LookupContentName, "ID=" & encodeSQLNumber(genericController.EncodeInteger(FieldValueVariant)), , , , , , "name", 1)
                                                        If csOk(CSLookup) Then
                                                            fieldValue = csGetText(CSLookup, "name")
                                                        End If
                                                        Call csClose(CSLookup)
                                                    ElseIf LookupList <> "" Then
                                                        '
                                                        ' -- Next try lookup list
                                                        FieldValueInteger = genericController.EncodeInteger(FieldValueVariant) - 1
                                                        If (FieldValueInteger >= 0) Then
                                                            lookups = Split(LookupList, ",")
                                                            If UBound(lookups) >= FieldValueInteger Then
                                                                fieldValue = lookups(FieldValueInteger)
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            Case FieldTypeIdMemberSelect
                                                '
                                                '
                                                '
                                                If genericController.vbIsNumeric(FieldValueVariant) Then
                                                    fieldValue = getRecordName("people", genericController.EncodeInteger(FieldValueVariant))
                                                End If
                                            Case FieldTypeIdCurrency
                                                '
                                                '
                                                '
                                                If genericController.vbIsNumeric(FieldValueVariant) Then
                                                    fieldValue = FormatCurrency(FieldValueVariant, 2, vbFalse, vbFalse, vbFalse)
                                                End If
                                            'NeedsHTMLEncode = False
                                            Case FieldTypeIdFileText, FieldTypeIdFileHTML
                                                '
                                                '
                                                '
                                                fieldValue = cpCore.cdnFiles.readFile(genericController.encodeText(FieldValueVariant))
                                            Case FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript
                                                '
                                                '
                                                '
                                                fieldValue = cpCore.cdnFiles.readFile(genericController.encodeText(FieldValueVariant))
                                            'NeedsHTMLEncode = False
                                            Case FieldTypeIdText, FieldTypeIdLongText, FieldTypeIdHTML
                                                '
                                                '
                                                '
                                                fieldValue = genericController.encodeText(FieldValueVariant)
                                            Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdAutoIdIncrement, FieldTypeIdFloat, FieldTypeIdInteger
                                                '
                                                '
                                                '
                                                fieldValue = genericController.encodeText(FieldValueVariant)
                                            'NeedsHTMLEncode = False
                                            Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                                '
                                                ' This case is covered before the select - but leave this here as safety net
                                                '
                                                'NeedsHTMLEncode = False
                                            Case Else
                                                '
                                                ' Unknown field type
                                                '
                                                Throw New ApplicationException("Can Not use field [" & FieldName & "] because the FieldType [" & fieldTypeId & "] Is invalid.")
                                        End Select
                                    End If
                                End If
                            End If
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return fieldValue
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Saves the value to the field, independant of field type, this routine accounts for the destination type, and saves the field as required (file, etc)
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="FieldName"></param>
        ''' <param name="FieldValue"></param>
        '
        Public Sub csSet(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As String)
            Try
                Dim BlankTest As String
                Dim FieldNameLc As String
                Dim SetNeeded As Boolean
                Dim fileNameNoExt As String
                Dim ContentName As String
                Dim fileName As String
                Dim pathFilenameOriginal As String
                '
                If Not csOk(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid or End-Of-file.")
                ElseIf String.IsNullOrEmpty(FieldName.Trim()) Then
                    Throw New ArgumentException("fieldName cannnot be blank")
                Else
                    With contentSetStore(CSPointer)
                        If Not .Updateable Then
                            Throw New ApplicationException("Cannot update a contentset created from a sql query.")
                        Else
                            ContentName = .ContentName
                            FieldNameLc = Trim(FieldName).ToLower
                            If (FieldValue Is Nothing) Then
                                FieldValue = String.Empty
                            End If
                            With .CDef
                                If .Name <> "" Then
                                    Dim field As Models.Complex.CDefFieldModel
                                    If Not .fields.ContainsKey(FieldNameLc) Then
                                        Throw New ArgumentException("The field [" & FieldName & "] could Not be found In content [" & .Name & "]")
                                    Else
                                        field = .fields.Item(FieldNameLc)
                                        Select Case field.fieldTypeId
                                            Case FieldTypeIdAutoIdIncrement, FieldTypeIdRedirect, FieldTypeIdManyToMany
                                            '
                                            ' Never set
                                            '
                                            Case FieldTypeIdFile, FieldTypeIdFileImage
                                                '
                                                ' Always set
                                                ' Saved in the field is the filename to the file
                                                ' csv_cs_get returns the filename
                                                ' csv_SetCS saves the filename
                                                '
                                                'FieldValueVariantLocal = FieldValueVariantLocal
                                                SetNeeded = True
                                            Case FieldTypeIdFileText, FieldTypeIdFileHTML
                                                '
                                                ' Always set
                                                ' A virtual file is created to hold the content, 'tablename/FieldNameLocal/0000.ext
                                                ' the extension is different for each fieldtype
                                                ' csv_SetCS and csv_cs_get return the content, not the filename
                                                '
                                                ' Saved in the field is the filename of the virtual file
                                                ' TextFile, assume this call is only made if a change was made to the copy.
                                                ' Use the csv_SetCSTextFile to manage the modified name and date correctly.
                                                ' csv_SetCSTextFile uses this method to set the row changed, so leave this here.
                                                '
                                                fileNameNoExt = csGetText(CSPointer, FieldNameLc)
                                                'FieldValue = genericController.encodeText(FieldValueVariantLocal)
                                                If FieldValue = "" Then
                                                    If fileNameNoExt <> "" Then
                                                        Call cpCore.cdnFiles.deleteFile(fileNameNoExt)
                                                        'Call publicFiles.DeleteFile(fileNameNoExt)
                                                        fileNameNoExt = ""
                                                    End If
                                                Else
                                                    If fileNameNoExt = "" Then
                                                        fileNameNoExt = csGetFilename(CSPointer, FieldName, "", ContentName, field.fieldTypeId)
                                                    End If
                                                    Call cpCore.cdnFiles.saveFile(fileNameNoExt, FieldValue)
                                                    'Call publicFiles.SaveFile(fileNameNoExt, FieldValue)
                                                End If
                                                FieldValue = fileNameNoExt
                                                SetNeeded = True
                                            Case FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript
                                                '
                                                ' public files - save as FieldTypeTextFile except if only white space, consider it blank
                                                '
                                                Dim PathFilename As String
                                                Dim FileExt As String
                                                Dim FilenameRev As Integer
                                                Dim path As String
                                                Dim Pos As Integer
                                                pathFilenameOriginal = csGetText(CSPointer, FieldNameLc)
                                                PathFilename = pathFilenameOriginal
                                                BlankTest = FieldValue
                                                BlankTest = genericController.vbReplace(BlankTest, " ", "")
                                                BlankTest = genericController.vbReplace(BlankTest, vbCr, "")
                                                BlankTest = genericController.vbReplace(BlankTest, vbLf, "")
                                                BlankTest = genericController.vbReplace(BlankTest, vbTab, "")
                                                If BlankTest = "" Then
                                                    If PathFilename <> "" Then
                                                        Call cpCore.cdnFiles.deleteFile(PathFilename)
                                                        PathFilename = ""
                                                    End If
                                                Else
                                                    If PathFilename = "" Then
                                                        PathFilename = csGetFilename(CSPointer, FieldNameLc, "", ContentName, field.fieldTypeId)
                                                    End If
                                                    If Left(PathFilename, 1) = "/" Then
                                                        '
                                                        ' root file, do not include revision
                                                        '
                                                    Else
                                                        '
                                                        ' content file, add a revision to the filename
                                                        '
                                                        Pos = InStrRev(PathFilename, ".")
                                                        If Pos > 0 Then
                                                            FileExt = Mid(PathFilename, Pos + 1)
                                                            fileNameNoExt = Mid(PathFilename, 1, Pos - 1)
                                                            Pos = InStrRev(fileNameNoExt, "/")
                                                            If Pos > 0 Then
                                                                'path = PathFilename
                                                                fileNameNoExt = Mid(fileNameNoExt, Pos + 1)
                                                                path = Mid(PathFilename, 1, Pos)
                                                                FilenameRev = 1
                                                                If Not genericController.vbIsNumeric(fileNameNoExt) Then
                                                                    Pos = genericController.vbInstr(1, fileNameNoExt, ".r", vbTextCompare)
                                                                    If Pos > 0 Then
                                                                        FilenameRev = genericController.EncodeInteger(Mid(fileNameNoExt, Pos + 2))
                                                                        FilenameRev = FilenameRev + 1
                                                                        fileNameNoExt = Mid(fileNameNoExt, 1, Pos - 1)
                                                                    End If
                                                                End If
                                                                fileName = fileNameNoExt & ".r" & FilenameRev & "." & FileExt
                                                                'PathFilename = PathFilename & dstFilename
                                                                path = genericController.convertCdnUrlToCdnPathFilename(path)
                                                                'srcSysFile = config.physicalFilePath & genericController.vbReplace(srcPathFilename, "/", "\")
                                                                'dstSysFile = config.physicalFilePath & genericController.vbReplace(PathFilename, "/", "\")
                                                                PathFilename = path & fileName
                                                                'Call publicFiles.renameFile(pathFilenameOriginal, fileName)
                                                            End If
                                                        End If
                                                    End If
                                                    If (pathFilenameOriginal <> "") And (pathFilenameOriginal <> PathFilename) Then
                                                        pathFilenameOriginal = genericController.convertCdnUrlToCdnPathFilename(pathFilenameOriginal)
                                                        Call cpCore.cdnFiles.deleteFile(pathFilenameOriginal)
                                                    End If
                                                    Call cpCore.cdnFiles.saveFile(PathFilename, FieldValue)
                                                End If
                                                FieldValue = PathFilename
                                                SetNeeded = True
                                            Case FieldTypeIdBoolean
                                                '
                                                ' Boolean - sepcial case, block on typed GetAlways set
                                                If genericController.EncodeBoolean(FieldValue) <> csGetBoolean(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                End If
                                            Case FieldTypeIdText
                                                '
                                                ' Set if text of value changes
                                                '
                                                If genericController.encodeText(FieldValue) <> csGetText(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                    If (FieldValue.Length > 255) Then
                                                        cpCore.handleException(New ApplicationException("Text length too long saving field [" & FieldName & "], length [" & FieldValue.Length & "], but max for Text field is 255. Save will be attempted"))
                                                    End If
                                                End If
                                            Case FieldTypeIdLongText, FieldTypeIdHTML
                                                '
                                                ' Set if text of value changes
                                                '
                                                If genericController.encodeText(FieldValue) <> csGetText(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                    If (FieldValue.Length > 65535) Then
                                                        cpCore.handleException(New ApplicationException("Text length too long saving field [" & FieldName & "], length [" & FieldValue.Length & "], but max for LongText and Html is 65535. Save will be attempted"))
                                                    End If
                                                End If
                                            Case Else
                                                '
                                                ' Set if text of value changes
                                                '
                                                If genericController.encodeText(FieldValue) <> csGetText(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                End If
                                        End Select
                                    End If
                                End If
                            End With
                            If Not SetNeeded Then
                                SetNeeded = SetNeeded
                            Else
                                '
                                ' ----- set the new value into the row buffer
                                '
                                If .writeCache.ContainsKey(FieldNameLc) Then
                                    .writeCache.Item(FieldNameLc) = FieldValue.ToString()
                                Else
                                    .writeCache.Add(FieldNameLc, FieldValue.ToString())
                                End If
                                .LastUsed = DateTime.Now
                            End If
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        Public Sub csSet(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Date)
            csSet(CSPointer, FieldName, FieldValue.ToString())
        End Sub
        Public Sub csSet(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Boolean)
            csSet(CSPointer, FieldName, FieldValue.ToString())
        End Sub
        Public Sub csSet(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Integer)
            csSet(CSPointer, FieldName, FieldValue.ToString())
        End Sub
        Public Sub csSet(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Double)
            csSet(CSPointer, FieldName, FieldValue.ToString())
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' rollback, or undo the changes to the current row
        ''' </summary>
        ''' <param name="CSPointer"></param>
        Public Sub csRollBack(ByVal CSPointer As Integer)
            Try
                If Not csOk(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    contentSetStore(CSPointer).writeCache.Clear()
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Save the current CS Cache back to the database
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="AsyncSave"></param>
        ''' <param name="Blockcsv_ClearBake"></param>
        '   If in Workflow Edit, save authorable fields to EditRecord, non-authorable to (both EditRecord and LiveRecord)
        '   non-authorable fields are inactive, non-authorable, read-only, and not-editable
        '
        ' Comment moved from in-line -- it was too hard to read around
        ' No -- IsModified is now set from an authoring control.
        '   Update all non-authorable fields in the edit record so they can be read in admin.
        '   Update all non-authorable fields in live record, because non-authorable is not a publish-able field
        '   edit record ModifiedDate in record only if non-authorable field is changed
        '
        ' ???
        '   I believe Non-FieldAdminAuthorable Fields should only save to the LiveRecord.
        '   They should also be read from the LiveRecord.
        '   Saving to the EditRecord sets the record Modified, which fields like "Viewings" should not change
        '
        '========================================================================
        '
        Public Sub csSave2(ByVal CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False, Optional ByVal Blockcsv_ClearBake As Boolean = False)
            Try
                Dim sqlModifiedDate As Date
                Dim sqlModifiedBy As Integer
                Dim writeCacheValue As Object
                Dim UcaseFieldName As String
                Dim FieldName As String
                Dim FieldFoundCount As Integer
                Dim FieldAdminAuthorable As Boolean
                Dim FieldReadOnly As Boolean
                Dim SQL As String
                Dim SQLSetPair As String
                Dim SQLUpdate As String
                'Dim SQLEditUpdate As String
                'Dim SQLEditDelimiter As String
                Dim SQLLiveUpdate As String
                Dim SQLLiveDelimiter As String
                Dim SQLCriteriaUnique As String = String.Empty
                Dim UniqueViolationFieldList As String = String.Empty

                Dim LiveTableName As String
                Dim LiveDataSourceName As String
                Dim LiveRecordID As Integer
                'Dim EditRecordID As Integer
                Dim LiveRecordContentControlID As Integer
                Dim LiveRecordContentName As String
                'Dim EditTableName As String
                'Dim EditDataSourceName As String = ""
                Dim AuthorableFieldUpdate As Boolean            ' true if an Edit field is being updated
                'Dim WorkflowRenderingMode As Boolean
                ' Dim AllowWorkflowSave As Boolean
                Dim Copy As String
                Dim ContentID As Integer
                Dim ContentName As String
                ' Dim WorkflowMode As Boolean
                Dim LiveRecordInactive As Boolean
                Dim ColumnPtr As Integer

                '
                If Not csOk(CSPointer) Then
                    '
                    ' already closed or not opened or not on a current row. No error so you can always call save(), it skips if nothing to save
                    '
                    'Throw New ArgumentException("dataset is not valid")
                ElseIf (contentSetStore(CSPointer).writeCache.Count = 0) Then
                    '
                    ' nothing to write, just exit
                    '
                ElseIf (Not contentSetStore(CSPointer).Updateable) Then
                    Throw New ArgumentException("The dataset cannot be updated because it was created with a query and not a content table.")
                Else
                    With contentSetStore(CSPointer)
                        '
                        With .CDef
                            LiveTableName = .ContentTableName
                            LiveDataSourceName = .ContentDataSourceName
                            ContentName = .Name
                            ContentID = .Id
                        End With
                        '
                        LiveRecordID = csGetInteger(CSPointer, "ID")
                        LiveRecordContentControlID = csGetInteger(CSPointer, "CONTENTCONTROLID")
                        LiveRecordContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, LiveRecordContentControlID)
                        LiveRecordInactive = Not csGetBoolean(CSPointer, "ACTIVE")
                        '
                        '
                        SQLLiveDelimiter = ""
                        SQLLiveUpdate = ""
                        SQLLiveDelimiter = ""
                        'SQLEditUpdate = ""
                        'SQLEditDelimiter = ""
                        sqlModifiedDate = DateTime.Now
                        sqlModifiedBy = .OwnerMemberID
                        '
                        AuthorableFieldUpdate = False
                        FieldFoundCount = 0
                        For Each keyValuePair In .writeCache
                            FieldName = keyValuePair.Key
                            UcaseFieldName = genericController.vbUCase(FieldName)
                            writeCacheValue = keyValuePair.Value
                            '
                            ' field has changed
                            '
                            If UcaseFieldName = "MODIFIEDBY" Then
                                '
                                ' capture and block it - it is hardcoded in sql
                                '
                                AuthorableFieldUpdate = True
                                sqlModifiedBy = genericController.EncodeInteger(writeCacheValue)
                            ElseIf UcaseFieldName = "MODIFIEDDATE" Then
                                '
                                ' capture and block it - it is hardcoded in sql
                                '
                                AuthorableFieldUpdate = True
                                sqlModifiedDate = genericController.EncodeDate(writeCacheValue)
                            Else
                                '
                                ' let these field be added to the sql
                                '
                                LiveRecordInactive = (UcaseFieldName = "ACTIVE" And (Not genericController.EncodeBoolean(writeCacheValue)))
                                FieldFoundCount += 1
                                Dim field As Models.Complex.CDefFieldModel = .CDef.fields(FieldName.ToLower())
                                With field
                                    SQLSetPair = ""
                                    FieldReadOnly = (.ReadOnly)
                                    FieldAdminAuthorable = ((Not .ReadOnly) And (Not .NotEditable) And (.authorable))
                                    '
                                    ' ----- Set SQLSetPair to the name=value pair for the SQL statement
                                    '
                                    Select Case .fieldTypeId
                                        Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                        Case FieldTypeIdInteger, FieldTypeIdLookup, FieldTypeIdAutoIdIncrement, FieldTypeIdMemberSelect
                                            SQLSetPair = FieldName & "=" & encodeSQLNumber(genericController.EncodeInteger(writeCacheValue))
                                        Case FieldTypeIdCurrency, FieldTypeIdFloat
                                            SQLSetPair = FieldName & "=" & encodeSQLNumber(genericController.EncodeNumber(writeCacheValue))
                                        Case FieldTypeIdBoolean
                                            SQLSetPair = FieldName & "=" & encodeSQLBoolean(genericController.EncodeBoolean(writeCacheValue))
                                        Case FieldTypeIdDate
                                            SQLSetPair = FieldName & "=" & encodeSQLDate(genericController.EncodeDate(writeCacheValue))
                                        Case FieldTypeIdText
                                            Copy = Left(genericController.encodeText(writeCacheValue), 255)
                                            If .Scramble Then
                                                Copy = genericController.TextScramble(cpCore, Copy)
                                            End If
                                            SQLSetPair = FieldName & "=" & encodeSQLText(Copy)
                                        Case FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdFileText, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript, FieldTypeIdFileHTML
                                            Copy = Left(genericController.encodeText(writeCacheValue), 255)
                                            SQLSetPair = FieldName & "=" & encodeSQLText(Copy)
                                        Case FieldTypeIdLongText, FieldTypeIdHTML
                                            SQLSetPair = FieldName & "=" & encodeSQLText(genericController.encodeText(writeCacheValue))
                                        Case Else
                                            '
                                            ' Invalid fieldtype
                                            '
                                            Throw New ApplicationException("Can Not save this record because the field [" & .nameLc & "] has an invalid field type Id [" & .fieldTypeId & "]")
                                    End Select
                                    If SQLSetPair <> "" Then
                                        '
                                        ' ----- Set the new value in the 
                                        '
                                        With contentSetStore(CSPointer)
                                            If .ResultColumnCount > 0 Then
                                                For ColumnPtr = 0 To .ResultColumnCount - 1
                                                    If .fieldNames(ColumnPtr) = UcaseFieldName Then
                                                        .readCache(ColumnPtr, .readCacheRowPtr) = writeCacheValue.ToString()
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                        End With
                                        If .UniqueName And (genericController.encodeText(writeCacheValue) <> "") Then
                                            '
                                            ' ----- set up for unique name check
                                            '
                                            If (Not String.IsNullOrEmpty(SQLCriteriaUnique)) Then
                                                SQLCriteriaUnique &= "Or"
                                                UniqueViolationFieldList &= ","
                                            End If
                                            Dim writeCacheValueText As String = genericController.encodeText(writeCacheValue)
                                            If Len(writeCacheValueText) < 255 Then
                                                UniqueViolationFieldList &= .nameLc & "=""" & writeCacheValueText & """"
                                            Else
                                                UniqueViolationFieldList &= .nameLc & "=""" & Left(writeCacheValueText, 255) & "..."""
                                            End If
                                            Select Case .fieldTypeId
                                                Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                                Case Else
                                                    SQLCriteriaUnique &= "(" & .nameLc & "=" & EncodeSQL(writeCacheValue, .fieldTypeId) & ")"
                                            End Select
                                        End If
                                        '
                                        ' ----- Live mode: update live record
                                        '
                                        SQLLiveUpdate = SQLLiveUpdate & SQLLiveDelimiter & SQLSetPair
                                        SQLLiveDelimiter = ","
                                        If FieldAdminAuthorable Then
                                            AuthorableFieldUpdate = True
                                        End If
                                    End If
                                End With
                            End If
                        Next
                        '
                        ' ----- Set ModifiedBy,ModifiedDate Fields if an admin visible field has changed
                        '
                        If AuthorableFieldUpdate Then
                            If (SQLLiveUpdate <> "") Then
                                '
                                ' ----- Authorable Fields Updated in non-Authoring Mode, set Live Record Modified
                                '
                                SQLLiveUpdate = SQLLiveUpdate & ",MODIFIEDDATE=" & encodeSQLDate(sqlModifiedDate) & ",MODIFIEDBY=" & encodeSQLNumber(sqlModifiedBy)
                            End If
                        End If
                        ''
                        '' not sure why, but this section was commented out.
                        '' Modified was not being set, so I un-commented it
                        ''
                        'If (SQLEditUpdate <> "") And (AuthorableFieldUpdate) Then
                        '    '
                        '    ' ----- set the csv_ContentSet Modified
                        '    '
                        '    Call cpCore.workflow.setRecordLocking(ContentName, LiveRecordID, AuthoringControlsModified, .OwnerMemberID)
                        'End If
                        '
                        ' ----- Do the unique check on the content table, if necessary
                        '
                        If SQLCriteriaUnique <> "" Then
                            Dim sqlUnique As String = "SELECT ID FROM " & LiveTableName & " WHERE (ID<>" & LiveRecordID & ")AND(" & SQLCriteriaUnique & ")and(" & .CDef.ContentControlCriteria & ");"
                            Using dt As DataTable = executeQuery(sqlUnique, LiveDataSourceName)
                                '
                                ' -- unique violation
                                If (dt.Rows.Count > 0) Then
                                    Throw New ApplicationException(("Can not save record to content [" & LiveRecordContentName & "] because it would create a non-unique record for one or more of the following field(s) [" & UniqueViolationFieldList & "]"))
                                End If
                            End Using
                        End If
                        If (FieldFoundCount > 0) Then
                            '
                            ' ----- update live table (non-workflowauthoring and non-authorable fields)
                            '
                            If (SQLLiveUpdate <> "") Then
                                SQLUpdate = "UPDATE " & LiveTableName & " SET " & SQLLiveUpdate & " WHERE ID=" & LiveRecordID & ";"
                                Call executeQuery(SQLUpdate, LiveDataSourceName)
                            End If
                            '
                            ' ----- Live record has changed
                            '
                            If AuthorableFieldUpdate Then
                                '
                                ' ----- reset the ContentTimeStamp to csv_ClearBake
                                '
                                Call cpCore.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(LiveTableName, "id", LiveRecordID.ToString()))
                                '
                                ' ----- mark the record NOT UpToDate for SpiderDocs
                                '
                                If (LCase(LiveTableName) = "ccpagecontent") And (LiveRecordID <> 0) Then
                                    If isSQLTableField("default", "ccSpiderDocs", "PageID") Then
                                        SQL = "UPDATE ccspiderdocs SET UpToDate = 0 WHERE PageID=" & LiveRecordID
                                        Call executeQuery(SQL)
                                    End If
                                End If
                            End If
                        End If
                        .LastUsed = DateTime.Now
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=====================================================================================================
        ''' <summary>
        ''' Initialize the csv_ContentSet Result Cache when it is first opened
        ''' </summary>
        ''' <param name="CSPointer"></param>
        '
        Private Sub cs_initData(ByVal CSPointer As Integer)
            Try
                Dim ColumnPtr As Integer
                '
                With contentSetStore(CSPointer)
                    .ResultColumnCount = 0
                    .readCacheRowCnt = 0
                    .readCacheRowPtr = -1
                    .writeCache = New Dictionary(Of String, String)
                    .ResultEOF = True
                    If .dt.Rows.Count > 0 Then
                        .ResultColumnCount = .dt.Columns.Count
                        ColumnPtr = 0
                        ReDim .fieldNames(.ResultColumnCount)
                        For Each dc As DataColumn In .dt.Columns
                            .fieldNames(ColumnPtr) = genericController.vbUCase(dc.ColumnName)
                            ColumnPtr = ColumnPtr + 1
                        Next
                        ' refactor -- convert interal storage to dt and assign -- will speedup open
                        .readCache = convertDataTabletoArray(.dt)
                        .readCacheRowCnt = UBound(.readCache, 2) + 1
                        .readCacheRowPtr = 0
                    End If
                    .writeCache = New Dictionary(Of String, String)
                End With
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=====================================================================================================
        ''' <summary>
        ''' returns tru if the dataset is pointing past the last row
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        '
        Private Function cs_IsEOF(ByVal CSPointer As Integer) As Boolean
            Dim returnResult As Boolean = True
            Try
                If CSPointer <= 0 Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    With contentSetStore(CSPointer)
                        cs_IsEOF = (.readCacheRowPtr >= .readCacheRowCnt)
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Encode a value for a sql
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <param name="fieldType"></param>
        ''' <returns></returns>
        Public Function EncodeSQL(ByVal expression As Object, Optional ByVal fieldType As Integer = FieldTypeIdText) As String
            Dim returnResult As String = ""
            Try
                Select Case fieldType
                    Case FieldTypeIdBoolean
                        returnResult = encodeSQLBoolean(genericController.EncodeBoolean(expression))
                    Case FieldTypeIdCurrency, FieldTypeIdFloat
                        returnResult = encodeSQLNumber(genericController.EncodeNumber(expression))
                    Case FieldTypeIdAutoIdIncrement, FieldTypeIdInteger, FieldTypeIdLookup, FieldTypeIdMemberSelect
                        returnResult = encodeSQLNumber(genericController.EncodeInteger(expression))
                    Case FieldTypeIdDate
                        returnResult = encodeSQLDate(genericController.EncodeDate(expression))
                    Case FieldTypeIdLongText, FieldTypeIdHTML
                        returnResult = encodeSQLText(genericController.encodeText(expression))
                    Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdRedirect, FieldTypeIdManyToMany, FieldTypeIdText, FieldTypeIdFileText, FieldTypeIdFileJavascript, FieldTypeIdFileXML, FieldTypeIdFileCSS, FieldTypeIdFileHTML
                        returnResult = encodeSQLText(genericController.encodeText(expression))
                    Case Else
                        cpCore.handleException(New ApplicationException("Unknown Field Type [" & fieldType & ""))
                        returnResult = encodeSQLText(genericController.encodeText(expression))
                End Select
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' return a sql compatible string. 
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        Public Function encodeSQLText(ByVal expression As String) As String
            Dim returnResult As String = ""
            If expression Is Nothing Then
                returnResult = "null"
            Else
                returnResult = genericController.encodeText(expression)
                If returnResult = "" Then
                    returnResult = "null"
                Else
                    returnResult = "'" & genericController.vbReplace(returnResult, "'", "''") & "'"
                End If
            End If
            Return returnResult
        End Function
        Public Function encodeSqlTextLike(cpcore As coreClass, source As String) As String
            Return encodeSQLText("%" & source & "%")
        End Function
        '
        '========================================================================
        ''' <summary>
        '''    encodeSQLDate
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        '
        Public Function encodeSQLDate(ByVal expression As Date) As String
            Dim returnResult As String = ""
            Try
                If IsDBNull(expression) Then
                    returnResult = "null"
                Else
                    Dim expressionDate As Date = genericController.EncodeDate(expression)
                    If (expressionDate = Date.MinValue) Then
                        returnResult = "null"
                    Else
                        returnResult = "'" & Year(expressionDate) & Right("0" & Month(expressionDate), 2) & Right("0" & Day(expressionDate), 2) & " " & Right("0" & expressionDate.Hour, 2) & ":" & Right("0" & expressionDate.Minute, 2) & ":" & Right("0" & expressionDate.Second, 2) & ":" & Right("00" & expressionDate.Millisecond, 3) & "'"
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' encodeSQLNumber
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        '
        Public Function encodeSQLNumber(ByVal expression As Double) As String
            Return expression.ToString
            'Dim returnResult As String = ""
            'Try
            '    If False Then
            '        'If expression Is Nothing Then
            '        'returnResult = "null"
            '        'ElseIf VarType(expression) = vbBoolean Then
            '        '    If genericController.EncodeBoolean(expression) Then
            '        '        returnResult = SQLTrue
            '        '    Else
            '        '        returnResult = SQLFalse
            '        '    End If
            '    ElseIf Not genericController.vbIsNumeric(expression) Then
            '        returnResult = "null"
            '    Else
            '        returnResult = expression.ToString
            '    End If
            'Catch ex As Exception
            '    cpCore.handleExceptionAndContinue(ex) : Throw
            'End Try
            'Return returnResult
        End Function
        '
        Public Function encodeSQLNumber(ByVal expression As Integer) As String
            Return expression.ToString
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' encodeSQLBoolean
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        '
        Public Function encodeSQLBoolean(ByVal expression As Boolean) As String
            Dim returnResult As String = SQLFalse
            Try
                If expression Then
                    returnResult = SQLTrue
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Create a filename for the Virtual Directory
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="FieldName"></param>
        ''' <param name="RecordID"></param>
        ''' <param name="OriginalFilename"></param>
        ''' <returns></returns>
        '========================================================================
        '
        Public Function GetVirtualFilename(ByVal ContentName As String, ByVal FieldName As String, ByVal RecordID As Integer, Optional ByVal OriginalFilename As String = "") As String
            Dim returnResult As String = ""
            Try
                Dim fieldTypeId As Integer
                Dim TableName As String
                'Dim iOriginalFilename As String
                Dim CDef As Models.Complex.cdefModel
                '
                If String.IsNullOrEmpty(ContentName.Trim()) Then
                    Throw New ArgumentException("contentname cannot be blank")
                ElseIf String.IsNullOrEmpty(FieldName.Trim()) Then
                    Throw New ArgumentException("fieldname cannot be blank")
                ElseIf (RecordID <= 0) Then
                    Throw New ArgumentException("recordid is not valid")
                Else
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName)
                    If CDef.Id = 0 Then
                        Throw New ApplicationException("contentname [" & ContentName & "] is not a valid content")
                    Else
                        TableName = CDef.ContentTableName
                        If TableName = "" Then
                            TableName = ContentName
                        End If
                        '
                        'iOriginalFilename = genericController.encodeEmptyText(OriginalFilename, "")
                        '
                        fieldTypeId = CDef.fields(FieldName.ToLower()).fieldTypeId
                        '
                        If OriginalFilename = "" Then
                            returnResult = fileController.getVirtualRecordPathFilename(TableName, FieldName, RecordID, fieldTypeId)
                        Else
                            returnResult = fileController.getVirtualRecordPathFilename(TableName, FieldName, RecordID, OriginalFilename)
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Opens a csv_ContentSet with the Members of a group
        ''' </summary>
        ''' <param name="groupList"></param>
        ''' <param name="sqlCriteria"></param>
        ''' <param name="SortFieldList"></param>
        ''' <param name="ActiveOnly"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        Public Function csOpenGroupUsers(ByVal groupList As List(Of String), Optional ByVal sqlCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
            Dim returnResult As Integer = -1
            Try
                Dim rightNow As Date = DateTime.Now
                Dim sqlRightNow As String = encodeSQLDate(rightNow)
                '
                If PageNumber = 0 Then
                    PageNumber = 1
                End If
                If PageSize = 0 Then
                    PageSize = pageSizeDefault
                End If
                If groupList.Count > 0 Then
                    '
                    ' Build Inner Query to select distinct id needed
                    '
                    Dim SQL As String = "SELECT DISTINCT ccMembers.id" _
                        & " FROM (ccMembers" _
                        & " LEFT JOIN ccMemberRules ON ccMembers.ID = ccMemberRules.MemberID)" _
                        & " LEFT JOIN ccGroups ON ccMemberRules.GroupID = ccGroups.ID" _
                        & " WHERE (ccMemberRules.Active<>0)AND(ccGroups.Active<>0)"
                    '
                    If ActiveOnly Then
                        SQL &= "AND(ccMembers.Active<>0)"
                    End If
                    '
                    Dim subQuery As String = ""
                    For Each groupName As String In groupList
                        If Not String.IsNullOrEmpty(groupName.Trim) Then
                            subQuery &= "or(ccGroups.Name=" & encodeSQLText(groupName.Trim) & ")"
                        End If
                    Next
                    If Not String.IsNullOrEmpty(subQuery) Then
                        SQL &= "and(" & subQuery.Substring(2) & ")"
                    End If
                    '
                    ' -- group expiration
                    SQL &= "and((ccMemberRules.DateExpires Is Null)or(ccMemberRules.DateExpires>" & sqlRightNow & "))"
                    '
                    ' Build outer query to get all ccmember fields
                    ' Must do this inner/outer because if the table has a text field, it can not be in the distinct
                    '
                    SQL = "SELECT * from ccMembers where id in (" & SQL & ")"
                    If sqlCriteria <> "" Then
                        SQL &= "and(" & sqlCriteria & ")"
                    End If
                    If SortFieldList <> "" Then
                        SQL &= " Order by " & SortFieldList
                    End If
                    returnResult = csOpenSql_rev("default", SQL, PageSize, PageNumber)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function '
        '========================================================================
        ''' <summary>
        ''' Get a Contents Tableid from the ContentPointer
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        '========================================================================
        '
        Public Function GetContentTableID(ByVal ContentName As String) As Integer
            Dim returnResult As Integer
            Try
                Dim dt As DataTable = executeQuery("select ContentTableID from ccContent where name=" & encodeSQLText(ContentName))
                If Not genericController.isDataTableOk(dt) Then
                    Throw New ApplicationException("Content [" & ContentName & "] was not found in ccContent table")
                Else
                    returnResult = genericController.EncodeInteger(dt.Rows(0).Item("ContentTableID"))
                End If
                dt.Dispose()
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
        ''' <param name="RecordID"></param>
        '
        Public Sub deleteTableRecord(ByVal TableName As String, ByVal RecordID As Integer, ByVal DataSourceName As String)
            Try
                If String.IsNullOrEmpty(TableName.Trim()) Then
                    Throw New ApplicationException("tablename cannot be blank")
                ElseIf (RecordID <= 0) Then
                    Throw New ApplicationException("record id is not valid [" & RecordID & "]")
                Else
                    Call DeleteTableRecords(TableName, "ID=" & RecordID, DataSourceName)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub     '
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

