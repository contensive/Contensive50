
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


		End Class
End Namespace

