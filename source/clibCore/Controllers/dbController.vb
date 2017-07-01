
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
        ' refactor -- replace all err.raise and handleLegacy... errors with throw new exception

        '
        '------------------------------------------------------------------------------------------------------------------------
        ' objects passed in that are not disposed
        '------------------------------------------------------------------------------------------------------------------------
        '
        Private cpCore As coreClass
        '
        '------------------------------------------------------------------------------------------------------------------------
        ' internal storage
        '------------------------------------------------------------------------------------------------------------------------
        '
        ' on Db success, verified set true. If error and not verified, a simple test is run. on failure, Db disabled 
        Private dbVerified As Boolean = False                                  ' set true when configured and tested - else db calls are skipped
        Private dbEnabled As Boolean = True                                    ' set true when configured and tested - else db calls are skipped
        '
        Private Const pageSizeDefault = 9999
        '
        Private Const useCSReadCacheMultiRow = True
        '
        Private Const allowWorkflowErrors = False
        '
        ' simple lazy cached values
        Private contentSetStore() As ContentSetType2
        Private contentSetStoreCount As Integer       ' The number of elements being used
        Private contentSetStoreSize As Integer        ' The number of current elements in the array
        Private Const contentSetStoreChunk = 50              ' How many are added at a time
        '
        ' when true, all csOpen, etc, will be setup, but not return any data (csv_IsCSOK false)
        ' this is used to generate the csv_ContentSet.Source so we can run a csv_GetContentRows without first opening a recordset
        Private contentSetOpenWithoutRecords As Boolean
        '
        '   SQL Timeouts
        Public sqlSlowThreshholdMsec As Integer        '
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
            Dim CDef As cdefModel
            Dim OwnerMemberID As Integer               ' ID of the member who opened the csv_ContentSet
            '
            ' Workflow editing modes
            Dim WorkflowAuthoringMode As Boolean    ' if true, these records came from the AuthoringTable, else ContentTable
            Dim WorkflowEditingRequested As Boolean ' if true, the CS was opened requesting WorkflowEditingMode
            Dim WorkflowEditingMode As Boolean      ' if true, the current record can be edited, else just rendered (effects EditBlank and csv_SaveCSRecord)
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
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        ''
        ''====================================================================================================
        '''' <summary>
        '''' get the database id for a given datasource name. If not found, -1 is returned
        '''' </summary>
        '''' <param name="DataSourceName"></param>
        '''' <returns></returns>
        'Public Function getDataSourceId(ByVal DataSourceName As String) As Integer
        '    Dim returnDataSourceId As Integer = -1
        '    Try
        '        Dim normalizedDataSourceName As String = Models.Entity.dataSourceModel.normalizeDataSourceName(DataSourceName)
        '        If (dataSources.ContainsKey(normalizedDataSourceName)) Then
        '            returnDataSourceId = dataSources(normalizedDataSourceName).id
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '    End Try
        '    Return returnDataSourceId
        'End Function
        ''
        ''====================================================================================================
        '''' <summary>
        '''' return the correctly formated connection string for a connection to the cluster's database (default connection no catalog) -- used to create new catalogs (appication databases) in the database
        '''' </summary>
        '''' <returns></returns>
        'Public Function dbEngine_getMasterADONETConnectionString() As String
        '    Return getConnectionStringADONET("", "")
        'End Function
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
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnConnString
        End Function
        ''
        ''====================================================================================================
        '''' <summary>
        '''' Create a new catalog in the database
        '''' </summary>
        '''' <param name="catalogName"></param>
        'Public Sub dbEngine_createCatalog(catalogName As String)
        '    Try
        '        dbEngine_executeMasterSql("create database " + catalogName)
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '    End Try
        'End Sub
        ''
        ''====================================================================================================
        '''' <summary>
        '''' Check if the database exists
        '''' </summary>
        '''' <param name="catalog"></param>
        '''' <returns></returns>
        'Public Function dbEngine_checkCatalogExists(catalog As String) As Boolean
        '    Dim returnOk As Boolean = False
        '    Try
        '        Dim sql As String
        '        Dim databaseId As Integer = 0
        '        Dim dt As DataTable
        '        '
        '        sql = String.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", catalog)
        '        dt = dbEngine_executeMasterSql(sql)
        '        returnOk = (dt.Rows.Count > 0)
        '        dt.Dispose()
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '    End Try
        '    Return returnOk
        'End Function
        ''
        ''====================================================================================================
        '''' <summary>
        '''' Execute a command (sql statemwent) and return a dataTable object
        '''' </summary>
        '''' <param name="sql"></param>
        '''' <param name="dataSourceName"></param>
        '''' <param name="startRecord"></param>
        '''' <param name="maxRecords"></param>
        '''' <returns></returns>
        'Public Function dbEngine_executeMasterSql(ByVal sql As String) As DataTable
        '    Dim returnData As New DataTable
        '    Try
        '        Dim connString As String = dbEngine_getMasterADONETConnectionString()
        '        If dbEnabled Then
        '            Using connSQL As New SqlConnection(connString)
        '                connSQL.Open()
        '                Using cmdSQL As New SqlCommand()
        '                    cmdSQL.CommandType = Data.CommandType.Text
        '                    cmdSQL.CommandText = sql
        '                    cmdSQL.Connection = connSQL
        '                    Using adptSQL = New SqlClient.SqlDataAdapter(cmdSQL)
        '                        adptSQL.Fill(returnData)
        '                    End Using
        '                End Using
        '            End Using
        '            dbVerified = True
        '        End If
        '    Catch ex As Exception
        '        Dim newEx As New ApplicationException("Exception [" & ex.Message & "] executing master sql [" & sql & "]", ex)
        '        cpCore.handleExceptionAndContinue(newEx)
        '    End Try
        '    Return returnData
        'End Function
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
        Public Function executeSql(ByVal sql As String, Optional ByVal dataSourceName As String = "", Optional ByVal startRecord As Integer = 0, Optional ByVal maxRecords As Integer = 9999, Optional ByRef recordsAffected As Integer = 0) As DataTable
            Dim returnData As New DataTable
            Try
                If (cpCore.serverConfig Is Nothing) Then
                    '
                    ' -- server config fail
                    cpCore.handleExceptionAndContinue(New ApplicationException("Cannot execute Sql in dbController without an application"))
                ElseIf (cpCore.serverConfig.appConfig Is Nothing) Then
                    '
                    ' -- server config fail
                    cpCore.handleExceptionAndContinue(New ApplicationException("Cannot execute Sql in dbController without an application"))
                Else
                    returnData = executeSql_noErrorHandling(sql, getConnectionStringADONET(cpCore.serverConfig.appConfig.name, dataSourceName), startRecord, maxRecords, recordsAffected)
                End If
            Catch ex As Exception
                Dim newEx As New ApplicationException("Exception [" & ex.Message & "] executing sql [" & sql & "], datasource [" & dataSourceName & "], startRecord [" & startRecord & "], maxRecords [" & maxRecords & "]", ex)
                cpCore.handleExceptionAndContinue(newEx)
            End Try
            Return returnData
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' executeSql without handling. Used from executeSql(), and as an initial connection test.
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <param name="connString"></param>
        ''' <param name="startRecord"></param>
        ''' <param name="maxRecords"></param>
        ''' <returns></returns>
        Private Function executeSql_noErrorHandling(ByVal sql As String, ByVal connString As String, ByVal startRecord As Integer, ByVal maxRecords As Integer, ByRef recordsAffected As Integer) As DataTable
            '
            ' REFACTOR
            ' consider writing cs intrface to sql dataReader object -- one row at a time, vaster.
            ' https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqldatareader.aspx
            '
            Dim sw As Stopwatch = Stopwatch.StartNew()
            Dim returnData As New DataTable
            Dim tickCountStart As Integer = GetTickCount
            If dbEnabled Then
                Using connSQL As New SqlConnection(connString)
                    connSQL.Open()
                    Using cmdSQL As New SqlCommand()
                        cmdSQL.CommandType = Data.CommandType.Text
                        cmdSQL.CommandText = sql
                        cmdSQL.Connection = connSQL
                        Using adptSQL = New SqlClient.SqlDataAdapter(cmdSQL)
                            recordsAffected = adptSQL.Fill(startRecord, maxRecords, returnData)
                        End Using
                    End Using
                End Using
                dbVerified = True
                sw.Stop()
                saveTransactionLog("duration [" & sw.ElapsedMilliseconds & "], sql [" & sql & "]")
                If (sw.ElapsedMilliseconds > sqlSlowThreshholdMsec) Then
                    saveSlowQueryLog(CInt(sw.ElapsedMilliseconds), sql)
                End If
            End If
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
            ' from - https://support.microsoft.com/en-us/kb/308611
            '
            ' REFACTOR 
            ' - add start recrod And max record in
            ' - add dataSourceName into the getConnectionString call - if no dataSourceName, return catalog in cluster Db, else return connstring
            '
            'Dim cn As ADODB.Connection = New ADODB.Connection()
            Dim rs As ADODB.Recordset = New ADODB.Recordset()
            Dim connString As String = getConnectionStringOLEDB(cpCore.serverConfig.appConfig.name, dataSourceName)
            '
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
                Dim newEx As New ApplicationException("Exception [" & ex.Message & "] executing sql [" & sql & "], datasource [" & dataSourceName & "], startRecord [" & startRecord & "], maxRecords [" & maxRecords & "]", ex)
                cpCore.handleExceptionAndContinue(newEx)
                Throw newEx
            End Try
            Return rs
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' execute sql on a specific datasource asynchonously. No data is returned.
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <param name="dataSourceName"></param>
        '
        Public Sub executeSqlAsync(ByVal sql As String, Optional ByVal dataSourceName As String = "")
            Exit Sub
            Try
                If dbEnabled Then
                    Dim connString As String = getConnectionStringADONET(cpCore.serverConfig.appConfig.name, dataSourceName)
                    Using connSQL As New SqlConnection(connString)
                        connSQL.Open()
                        Using cmdSQL As New SqlCommand()
                            cmdSQL.CommandType = Data.CommandType.Text
                            cmdSQL.CommandText = sql
                            cmdSQL.Connection = connSQL
                            cmdSQL.ExecuteNonQuery()
                            'cmdSQL.BeginExecuteNonQuery()
                        End Using
                    End Using
                    dbVerified = True
                End If
            Catch ex As Exception
                Dim newEx As New ApplicationException("Exception [" & ex.Message & "] executing sql async [" & sql & "], datasource [" & dataSourceName & "]", ex)
                cpCore.handleExceptionAndContinue(newEx)
                Throw newEx
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Get a Contents ID from the ContentName, Returns -1 if not found
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        Public Function getContentId(ByVal ContentName As String) As Integer
            Dim returnId As Integer = -1
            Try
                Dim cdef As cdefModel
                '
                cdef = cpCore.metaData.getCdef(ContentName)
                If (cdef IsNot Nothing) Then
                    returnId = cdef.Id
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnId
        End Function
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
                Dim dt As DataTable = executeSql(SQL, DataSourceName)
                dt.Dispose()
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                CreateKeyString = encodeSQLNumber(genericController.getRandomInteger)
                DateAddedString = encodeSQLDate(Now)
                '
                sqlList.add("createkey", CreateKeyString)
                sqlList.add("dateadded", DateAddedString)
                sqlList.add("createdby", encodeSQLNumber(MemberID))
                sqlList.add("ModifiedDate", DateAddedString)
                sqlList.add("ModifiedBy", encodeSQLNumber(MemberID))
                sqlList.add("EditSourceID", encodeSQLNumber(0))
                sqlList.add("EditArchive", encodeSQLNumber(0))
                sqlList.add("EditBlank", encodeSQLNumber(0))
                sqlList.add("ContentControlID", encodeSQLNumber(0))
                sqlList.add("Name", encodeSQLText(""))
                sqlList.add("Active", encodeSQLNumber(1))
                '
                Call insertTableRecord(DataSourceName, TableName, sqlList)
                returnDt = openTable(DataSourceName, TableName, "(DateAdded=" & DateAddedString & ")and(CreateKey=" & CreateKeyString & ")", "ID DESC",, 1)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                    Dim dt As DataTable = executeSql(sql, DataSourceName)
                    dt.Dispose()
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                returnDataTable = executeSql(SQL, DataSourceName, (PageNumber - 1) * PageSize, PageSize)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnDataTable
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' log a slow query
        ''' </summary>
        ''' <param name="TransactionTickCount"></param>
        ''' <param name="SQL"></param>
        Private Sub saveSlowQueryLog(ByVal TransactionTickCount As Integer, ByVal SQL As String)
            logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "query time  " & genericController.GetIntegerString(TransactionTickCount, 7) & "ms: " & SQL, "dll", "cpCoreClass", "csv_ExecuteSQL", 0, "", SQL, False, True, "", "Performance", "SlowSQL")
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' update the transaction log
        ''' </summary>
        ''' <param name="LogEntry"></param>
        Private Sub saveTransactionLog(ByVal LogEntry As String)
            If Not String.IsNullOrEmpty(LogEntry) Then
                Dim Message As String = LogEntry.Replace(vbCr, "")
                Message = Message.Replace(vbLf, "")
                logController.appendLog(cpCore, Message, "DbTransactions")
            End If
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Finds the where clause (first WHERE not in single quotes). returns 0 if not found, otherwise returns locaion of word where
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <returns></returns>
        Private Function getSQLWherePosition(ByVal SQL As String) As Integer
            Dim returnPos As Integer = 0
            Try
                If genericController.isInStr(1, SQL, "WHERE", vbTextCompare) Then
                    '
                    ' ----- contains the word "WHERE", now weed out if not a where clause
                    '
                    returnPos = InStrRev(SQL, " WHERE ", , vbTextCompare)
                    If returnPos = 0 Then
                        returnPos = InStrRev(SQL, ")WHERE ", , vbTextCompare)
                        If returnPos = 0 Then
                            returnPos = InStrRev(SQL, " WHERE(", , vbTextCompare)
                            If returnPos = 0 Then
                                returnPos = InStrRev(SQL, ")WHERE(", , vbTextCompare)
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnPos
        End Function
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
                Dim tableSchema As tableSchemaModel = cpCore.metaData.getTableSchema(TableName, DataSourceName)
                If (tableSchema IsNot Nothing) Then
                    returnOK = tableSchema.columns.Contains(FieldName.ToLower)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                ReturnOK = (Not cpCore.metaData.getTableSchema(TableName, DataSourceName) Is Nothing)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                '
                Dim SQL As String
                Dim iAllowAutoIncrement As Boolean
                '
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
                    iAllowAutoIncrement = AllowAutoIncrement
                    '
                    If (cpCore.metaData.getTableSchema(TableName, DataSourceName) Is Nothing) Then
                        If Not iAllowAutoIncrement Then
                            SQL = "Create Table " & TableName & "(ID " & getSQLAlterColumnType(DataSourceName, FieldTypeIdInteger) & ");"
                            executeSql(SQL, DataSourceName).Dispose()
                        Else
                            SQL = "Create Table " & TableName & "(ID " & getSQLAlterColumnType(DataSourceName, FieldTypeIdAutoIdIncrement) & ");"
                            executeSql(SQL, DataSourceName).Dispose()
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
                    Call createSQLTableField(DataSourceName, TableName, "editSourceID", FieldTypeIdInteger)
                    Call createSQLTableField(DataSourceName, TableName, "editArchive", FieldTypeIdBoolean)
                    Call createSQLTableField(DataSourceName, TableName, "editBlank", FieldTypeIdBoolean)
                    Call createSQLTableField(DataSourceName, TableName, "contentCategoryID", FieldTypeIdInteger)
                    Call createSQLTableField(DataSourceName, TableName, "ccGuid", FieldTypeIdText)
                    '
                    ' ----- setup core indexes
                    '
                    Call createSQLIndex(DataSourceName, TableName, TableName & "Id", "ID")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "Active", "ACTIVE")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "Name", "NAME")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "SortOrder", "SORTORDER")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "DateAdded", "DATEADDED")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "CreateKey", "CREATEKEY")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "EditSourceID", "EDITSOURCEID")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "ContentControlID", "CONTENTCONTROLID")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "ModifiedDate", "MODIFIEDDATE")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "ContentCategoryID", "CONTENTCATEGORYID")
                    Call createSQLIndex(DataSourceName, TableName, TableName & "ccGuid", "CCGUID")
                End If
                cpCore.metaData.tableSchemaListClear()
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                        Call executeSql(SQL, DataSourceName).Dispose()
                        '
                        If clearMetaCache Then
                            Call cpCore.cache.invalidateAll()
                            Call cpCore.metaData.clear()
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Call executeSql("DROP TABLE " & TableName, DataSourceName).Dispose()
                cpCore.cache.invalidateAll()
                cpCore.metaData.clear()
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                    Call executeSql("ALTER TABLE " & TableName & " DROP COLUMN " & FieldName & ";", DataSourceName)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Dim ts As tableSchemaModel
                If Not (String.IsNullOrEmpty(TableName) And String.IsNullOrEmpty(IndexName) And String.IsNullOrEmpty(FieldNames)) Then
                    ts = cpCore.metaData.getTableSchema(TableName, DataSourceName)
                    If (ts IsNot Nothing) Then
                        If Not ts.indexes.Contains(IndexName.ToLower) Then
                            Call executeSql("CREATE INDEX " & IndexName & " ON " & TableName & "( " & FieldNames & " );", DataSourceName)
                            If clearMetaCache Then
                                cpCore.cache.invalidateAll()
                                cpCore.metaData.clear()
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                If cs_ok(CS) Then
                    returnRecordName = cs_get(CS, "Name")
                End If
                Call cs_Close(CS)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnRecordName
        End Function
        '
        '=============================================================
        ''' <summary>
        ''' get the lowest recordId based on its name. If not record is found, 0 is returned
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="RecordName"></param>
        ''' <returns></returns>
        Public Function getRecordID(ByVal ContentName As String, ByVal RecordName As String) As Integer
            Dim returnValue As Integer = 0
            Try
                Dim CS As Integer = cs_open(ContentName, "name=" & encodeSQLText(RecordName), "ID", , , , , "ID")
                If cs_ok(CS) Then
                    returnValue = genericController.EncodeInteger(cs_get(CS, "ID"))
                    If returnValue = 0 Then
                        Throw New ApplicationException("getRecordId, contentname [" & ContentName & "], recordName [" & RecordName & "]), a record was found but id returned 0")
                    End If
                End If
                Call cs_Close(CS)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                    Case FieldTypeIdManyToMany, FieldTypeIdRedirect, FieldTypeIdFileImage, FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdText, FieldTypeIdFile, FieldTypeIdFileTextPrivate, FieldTypeIdFileJavascript, FieldTypeIdFileXML, FieldTypeIdFileCSS, FieldTypeIdFileHTMLPrivate
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Dim ts As tableSchemaModel
                Dim DataSourceType As Integer
                Dim sql As String
                '
                ts = cpCore.metaData.getTableSchema(TableName, DataSourceName)
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
                        Call executeSql(sql, DataSourceName)
                        cpCore.cache.invalidateAll()
                        cpCore.metaData.clear()
                    End If
                End If

            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Dim dt As DataTable = executeSql("Select top 1 id from ccFields where name=" & encodeSQLText(FieldName) & " And contentid=" & ContentID)
                isCdefField = genericController.isDataTableOk(dt)
                dt.Dispose()
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                        returnTypeId = FieldTypeIdFileTextPrivate
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
                        returnTypeId = FieldTypeIdFileHTMLPrivate
                    Case Else
                        '
                        ' Bad field type is a text field
                        '
                        returnTypeId = FieldTypeIdText
                End Select
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        ''' <param name="WorkflowRenderingMode"></param>
        ''' <param name="WorkflowEditingMode"></param>
        ''' <param name="SelectFieldList"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        '========================================================================
        '
        Public Function cs_open(ByVal ContentName As String, Optional ByVal Criteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal MemberID As Integer = 0, Optional ByVal WorkflowRenderingMode As Boolean = False, Optional ByVal WorkflowEditingMode As Boolean = False, Optional ByVal SelectFieldList As String = "", Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
            Dim returnCs As Integer = -1
            Try
                '
                Dim Copy2 As String
                Dim SortFields() As String
                Dim SortField As String
                Dim Ptr As Integer
                Dim Cnt As Integer
                Dim ContentCriteria As String
                Dim TableName As String
                Dim DataSourceName As String
                Dim iActiveOnly As Boolean
                Dim iSortFieldList As String
                Dim iWorkflowRenderingMode As Boolean
                Dim AllowWorkflowSave As Boolean
                Dim iMemberID As Integer
                Dim iCriteria As String
                Dim RecordID As Integer
                Dim iSelectFieldList As String
                Dim CDef As cdefModel
                Dim TestUcaseFieldList As String
                Dim SQL As String
                '
                If String.IsNullOrEmpty(ContentName) Then
                    Throw New ApplicationException("ContentName cannot be blank")
                Else
                    CDef = cpCore.metaData.getCdef(ContentName)
                    If (CDef Is Nothing) Then
                        Throw (New ApplicationException("No content found For [" & ContentName & "]"))
                    ElseIf (CDef.Id <= 0) Then
                        Throw (New ApplicationException("No content found For [" & ContentName & "]"))
                    Else
                        '
                        'hint = hint & ", 100"
                        iActiveOnly = ((ActiveOnly))
                        iSortFieldList = genericController.encodeEmptyText(SortFieldList, CDef.DefaultSortMethod)
                        iWorkflowRenderingMode = cpCore.siteProperties.allowWorkflowAuthoring And CDef.AllowWorkflowAuthoring And (WorkflowRenderingMode)
                        AllowWorkflowSave = iWorkflowRenderingMode And (WorkflowEditingMode)
                        iMemberID = MemberID
                        iCriteria = genericController.encodeEmptyText(Criteria, "")
                        iSelectFieldList = genericController.encodeEmptyText(SelectFieldList, CDef.SelectCommaList)
                        If AllowWorkflowSave Then
                            AllowWorkflowSave = AllowWorkflowSave
                        End If
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
                        'hint = hint & ",200"
                        If Not iWorkflowRenderingMode Then
                            '
                            ' Return Live Records
                            '
                            '%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                            '
                            ' I really can not be sure if this is for workflow/non-rendering, or for non-workflow mode
                            '   if non-rendering, it has to stay (workflow mode that shows the live records
                            '
                            '%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
                            '
                            TableName = CDef.ContentTableName
                            DataSourceName = CDef.ContentDataSourceName
                            If CDef.AllowWorkflowAuthoring Then
                                '
                                ' Workflow Authoring table - block edit records
                                '
                                ContentCriteria = ContentCriteria & "And((EditSourceID=0)Or(EditSourceID Is null))"
                                ContentCriteria = ContentCriteria & "And((EditBlank=0)Or(EditBlank Is null))"
                            Else
                                '
                                ' Live Authoring table - just let sql go as-is
                                '
                                ContentCriteria = ContentCriteria
                            End If
                        Else
                            '
                            '------------------------------------------------------------------------------
                            ' Workflow authoring mode - PhantomID substitution
                            ' REturn the edit records instead of the live records
                            '------------------------------------------------------------------------------
                            '
                            'hint = hint & ",300"
                            TableName = CDef.AuthoringTableName
                            'UcaseTablename = genericController.vbUCase(TableName)
                            DataSourceName = CDef.AuthoringDataSourceName
                            '
                            ' Substitute ID for EditSourceID in criteria
                            '
                            ContentCriteria = genericController.vbReplace(ContentCriteria, " ID=", " " & TableName & ".EditSourceID=")
                            ContentCriteria = genericController.vbReplace(ContentCriteria, " ID>", " " & TableName & ".EditSourceID>")
                            ContentCriteria = genericController.vbReplace(ContentCriteria, " ID<", " " & TableName & ".EditSourceID<")
                            ContentCriteria = genericController.vbReplace(ContentCriteria, " ID ", " " & TableName & ".EditSourceID ")
                            ContentCriteria = genericController.vbReplace(ContentCriteria, "(ID=", "(" & TableName & ".EditSourceID=")
                            ContentCriteria = genericController.vbReplace(ContentCriteria, "(ID>", "(" & TableName & ".EditSourceID>")
                            ContentCriteria = genericController.vbReplace(ContentCriteria, "(ID<", "(" & TableName & ".EditSourceID<")
                            ContentCriteria = genericController.vbReplace(ContentCriteria, "(ID ", "(" & TableName & ".EditSourceID ")
                            '
                            ' Require non-null editsourceid and editarchive false
                            ' include tablename so fields are used, not aliases created for phantomID
                            '
                            ContentCriteria = ContentCriteria & "And(" & TableName & ".EditSourceID Is Not null)"
                            ContentCriteria = ContentCriteria & "And(" & TableName & ".EditArchive=0)"
                            '
                            ' Workflow Rendering (WorkflowEditing false) - block deleted records, allow inserted records.
                            '
                            If Not AllowWorkflowSave Then
                                ContentCriteria = ContentCriteria & "And(" & TableName & ".EditBlank=0)"
                            End If
                            '
                            ' Order by clause is included, translate ID to EditSourceID
                            '
                            If iSortFieldList <> "" Then
                                iSortFieldList = "," & iSortFieldList & ","
                                iSortFieldList = genericController.vbReplace(iSortFieldList, ",ID,", "," & TableName & ".EditSourceID,")
                                iSortFieldList = genericController.vbReplace(iSortFieldList, ",ID ", "," & TableName & ".EditSourceID ")
                                iSortFieldList = Mid(iSortFieldList, 2, Len(iSortFieldList) - 2)
                            End If
                            '
                            ' Change select field list from ID to EditSourceID
                            '
                            If iSelectFieldList <> "" Then
                                iSelectFieldList = "," & iSelectFieldList & ","
                                '
                                ' WorkflowR2
                                ' conversion the ID field to EditSourceID
                                ' this was done in the csv_cs_getField, but then a csv_cs_getrows call would return incorrect
                                ' values because there is no translation
                                '
                                If genericController.vbInstr(1, iSelectFieldList, ",ID,", vbTextCompare) = 0 Then
                                    '
                                    ' Add ID select if not there
                                    '
                                    iSelectFieldList = iSelectFieldList & TableName & ".EditSourceID As ID,"
                                Else
                                    '
                                    ' remove ID, and add ID alias to return EditSourceID (the live records id)
                                    '
                                    iSelectFieldList = genericController.vbReplace(iSelectFieldList, ",ID,", "," & TableName & ".EditSourceID As ID,")
                                End If
                                '
                                ' Add the edit fields to the select
                                '
                                Copy2 = TableName & ".EDITSOURCEID,"
                                If (InStr(1, iSelectFieldList, ",EDITSOURCEID,", vbTextCompare) <> 0) Then
                                    iSelectFieldList = genericController.vbReplace(iSelectFieldList, ",EDITSOURCEID,", "," & Copy2, 1, 99, vbTextCompare)
                                Else
                                    iSelectFieldList = iSelectFieldList & Copy2
                                End If
                                Copy2 = TableName & ".EDITARCHIVE,"
                                If (InStr(1, iSelectFieldList, ",EDITARCHIVE,", vbTextCompare) <> 0) Then
                                    iSelectFieldList = genericController.vbReplace(iSelectFieldList, ",EDITARCHIVE,", "," & Copy2, 1, 99, vbTextCompare)
                                Else
                                    iSelectFieldList = iSelectFieldList & Copy2
                                End If
                                Copy2 = TableName & ".EDITBLANK,"
                                If (InStr(1, iSelectFieldList, ",EDITBLANK,", vbTextCompare) <> 0) Then
                                    iSelectFieldList = genericController.vbReplace(iSelectFieldList, ",EDITBLANK,", "," & Copy2, 1, 99, vbTextCompare)
                                Else
                                    iSelectFieldList = iSelectFieldList & Copy2
                                End If
                                '
                                ' WorkflowR2 - this also came from the csv_cs_getField - if EditID is requested, return the edit records id
                                ' must include the tablename or the ID would be the alias ID, which has the editsourceid in it.
                                '
                                iSelectFieldList = iSelectFieldList & TableName & ".ID As EditID,"
                                '
                                iSelectFieldList = Mid(iSelectFieldList, 2, Len(iSelectFieldList) - 2)
                            End If
                        End If
                        '
                        ' ----- Check for blank Tablename or DataSource
                        '
                        'hint = hint & ",400"
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
                            .WorkflowAuthoringMode = iWorkflowRenderingMode
                            .WorkflowEditingRequested = AllowWorkflowSave
                            .WorkflowEditingMode = False ' set only if lock is OK
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
                                contentSetStore(returnCs).dt = executeSql(SQL, DataSourceName, .PageSize * (.PageNumber - 1), .PageSize)
                            End If
                        End With
                        'hint = hint & ",600"
                        Call cs_initData(returnCs)
                        'Call cs_loadCurrentRow(returnCs)
                        '
                        'hint = hint & ",700"
                        If iWorkflowRenderingMode Then
                            '
                            ' Authoring mode
                            '
                            'Call verifyWorkflowAuthoringRecord(returnCs)
                            If AllowWorkflowSave Then
                                '
                                ' Workflow Editing Mode - lock the first record
                                '
                                If Not cs_IsEOF(returnCs) Then
                                    If contentSetStore(returnCs).WorkflowEditingRequested Then
                                        RecordID = cs_getInteger(returnCs, "ID")
                                        If Not cpCore.workflow.isRecordLocked(ContentName, RecordID, MemberID) Then
                                            contentSetStore(returnCs).WorkflowEditingMode = True
                                            Call cpCore.workflow.setEditLock(ContentName, RecordID, MemberID)
                                        End If
                                    End If
                                End If
                            Else
                                '
                                ' Workflow Rendering Mode
                                '
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnCs
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' csv_DeleteCSRecord
        ''' </summary>
        ''' <param name="CSPointer"></param>
        Public Sub cs_deleteRecord(ByVal CSPointer As Integer)
            Try
                '
                Dim LiveRecordID As Integer
                Dim EditRecordID As Integer
                Dim ContentID As Integer
                Dim ContentName As String
                Dim ContentDataSourceName As String
                Dim ContentTableName As String
                Dim AuthoringDataSourceName As String
                Dim AuthoringTableName As String
                Dim SQLName(5) As String
                Dim SQLValue(5) As String
                Dim Filename As String
                Dim sqlList As sqlFieldListClass
                '
                If Not cs_ok(CSPointer) Then
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
                        AuthoringTableName = .AuthoringTableName
                        AuthoringDataSourceName = .AuthoringDataSourceName
                    End With
                    If (ContentName = "") Then
                        Throw New ArgumentException("csv_ContentSet Is Not based On a Content Definition")
                    Else
                        LiveRecordID = cs_getInteger(CSPointer, "ID")
                        If Not contentSetStore(CSPointer).WorkflowAuthoringMode Then
                            '
                            ' delete any files
                            '
                            Dim fieldName As String
                            Dim field As CDefFieldModel
                            With contentSetStore(CSPointer).CDef
                                For Each keyValue In .fields
                                    field = keyValue.Value
                                    With field
                                        fieldName = .nameLc
                                        Select Case .fieldTypeId
                                            Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdFileCSS, FieldTypeIdFileJavascript, FieldTypeIdFileXML
                                                '
                                                ' public content files
                                                '
                                                Filename = cs_getText(CSPointer, fieldName)
                                                If Filename <> "" Then
                                                    Call cpCore.cdnFiles.deleteFile(cpCore.cdnFiles.joinPath(cpCore.serverConfig.appConfig.cdnFilesNetprefix, Filename))
                                                End If
                                            Case FieldTypeIdFileTextPrivate, FieldTypeIdFileHTMLPrivate
                                                '
                                                ' private files
                                                '
                                                Filename = cs_getText(CSPointer, fieldName)
                                                If Filename <> "" Then
                                                    Call cpCore.privateFiles.deleteFile(Filename)
                                                End If
                                        End Select
                                    End With
                                Next
                            End With
                            '
                            ' non-workflow mode, delete the live record
                            '
                            Call deleteTableRecord(ContentTableName, LiveRecordID, ContentDataSourceName)
                            If workflowController.csv_AllowAutocsv_ClearContentTimeStamp Then
                                Call cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(ContentTableName, "id", LiveRecordID.ToString()))
                                'Call cpCore.cache.invalidateObject(ContentName)
                            End If
                            Call deleteContentRules(ContentID, LiveRecordID)
                        Else
                            '
                            ' workflow mode, mark the editrecord "Blanked"
                            '
                            EditRecordID = cs_getInteger(CSPointer, "EditID")
                            sqlList = New sqlFieldListClass
                            Call sqlList.add("EDITBLANK", SQLTrue) ' Pointer)
                            Call sqlList.add("MODIFIEDBY", encodeSQLNumber(contentSetStore(CSPointer).OwnerMemberID)) ' Pointer)
                            Call sqlList.add("MODIFIEDDATE", encodeSQLDate(Now)) ' Pointer)
                            Call updateTableRecord(AuthoringDataSourceName, AuthoringTableName, "ID=" & EditRecordID, sqlList)
                            Call cpCore.workflow.setAuthoringControl(ContentName, LiveRecordID, AuthoringControlsModified, contentSetStore(CSPointer).OwnerMemberID)
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Function cs_openCsSql_rev(ByVal DataSourceName As String, ByVal SQL As String, Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
            Return cs_openSql(SQL, DataSourceName, PageSize, PageNumber)
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
        Public Function cs_openSql(ByVal SQL As String, Optional ByVal DataSourceName As String = "", Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
            Dim returnCs As Integer = -1
            Try
                returnCs = cs_init(cpCore.authContext.user.id)
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
                    contentSetStore(returnCs).dt = executeSql(SQL, DataSourceName, PageSize * (PageNumber - 1), PageSize)
                    Call cs_initData(returnCs)
                    'Call cs_loadCurrentRow(returnCs)
                Else
                    contentSetStore(returnCs).dt = executeSql(SQL, DataSourceName, PageSize * (PageNumber - 1), PageSize)
                    Call cs_initData(returnCs)
                    'Call cs_loadCurrentRow(returnCs)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                    .WorkflowAuthoringMode = False
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Sub cs_Close(ByRef CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False)
            Try
                If (CSPointer > 0) And (CSPointer <= contentSetStoreCount) Then
                    With contentSetStore(CSPointer)
                        If .IsOpen Then
                            Call cs_save2(CSPointer, AsyncSave)
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
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ' Move the csv_ContentSet to the next row
        '========================================================================
        '
        Public Sub cs_goNext(ByVal CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False)
            Try
                '
                Dim ContentName As String
                Dim RecordID As Integer
                '
                If Not cs_ok(CSPointer) Then
                    '
                    Throw New ApplicationException("CSPointer Not csv_IsCSOK.")
                Else
                    Call cs_save2(CSPointer, AsyncSave)
                    contentSetStore(CSPointer).WorkflowEditingMode = False
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
                        If (Not cs_IsEOF(CSPointer)) And contentSetStore(CSPointer).WorkflowEditingRequested Then
                            ContentName = contentSetStore(CSPointer).ContentName
                            RecordID = cs_getInteger(CSPointer, "ID")
                            If Not cpCore.workflow.isRecordLocked(ContentName, RecordID, contentSetStore(CSPointer).OwnerMemberID) Then
                                contentSetStore(CSPointer).WorkflowEditingMode = True
                                Call cpCore.workflow.setEditLock(ContentName, RecordID, contentSetStore(CSPointer).OwnerMemberID)
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ' Move the csv_ContentSet to the first row
        '========================================================================
        '
        Public Sub cs_goFirst(ByVal CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False)
            Try
                If Not cs_ok(CSPointer) Then
                    Throw New ApplicationException("data set is not valid")
                Else
                    Call cs_save2(CSPointer, AsyncSave)
                    contentSetStore(CSPointer).readCacheRowPtr = 0
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Function cs_getField(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            Dim returnValue As String = ""
            Try
                Dim fieldFound As Boolean
                Dim ColumnPointer As Integer
                Dim fieldNameTrimUpper As String
                Dim fieldNameTrim As String
                '
                fieldNameTrim = FieldName.Trim()
                fieldNameTrimUpper = genericController.vbUCase(fieldNameTrim)
                If (fieldNameTrimUpper = "STYLESFILENAME") Then
                    fieldNameTrimUpper = fieldNameTrimUpper
                End If
                If Not cs_ok(CSPointer) Then
                    Throw New ApplicationException("Attempt To getField fieldname[" & FieldName & "], but the dataset Is empty Or does Not point To a valid row")
                Else
                    With contentSetStore(CSPointer)
                        '
                        ' WorkflowR2
                        ' this code ws replaced in csv_InitContentSetResult by replacing the column names. That was to fix the
                        ' problem of csv_cs_getRows and csv_cs_getRowNames. In those cases, the FieldNameLocal swap could not be done.Even
                        ' with this EditID can not be swapped in csv_cs_getRow
                        '
                        If .WorkflowAuthoringMode Then
                            '    If (FieldNameLocalUcase = "EDITSOURCEID") Then
                            '        FieldNameLocalUcase = "ID"
                            '    End If
                        Else
                            If (fieldNameTrimUpper = "EDITID") Then
                                fieldNameTrimUpper = "ID"
                            End If
                        End If
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
                                    Throw New ApplicationException("Field [" & fieldNameTrim & "] was Not found In csv_ContentSet from source [" & .Source & "]")
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
                                                    returnValue = cpCore.metaData.TextDeScramble(genericController.encodeText(returnValue))
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                If Not cs_ok(CSPointer) Then
                    Throw New ApplicationException("data set is not valid")
                Else
                    contentSetStore(CSPointer).fieldPointer = 0
                    returnFieldName = cs_getNextFieldName(CSPointer)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                If Not cs_ok(CSPointer) Then
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                If cs_ok(CSPointer) Then
                    If contentSetStore(CSPointer).Updateable Then
                        With contentSetStore(CSPointer).CDef
                            If .Name <> "" Then
                                returnFieldTypeid = .fields(FieldName.ToLower()).fieldTypeId
                            End If
                        End With
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                If cs_ok(CSPointer) Then
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                If cs_ok(CSPointer) Then
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                ElseIf Not cs_ok(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    Dim CSSelectFieldList As String = cs_getSelectFieldList(CSPointer)
                    returnResult = genericController.IsInDelimitedString(CSSelectFieldList, FieldName, ",")
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Function cs_getFilename(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal OriginalFilename As String, Optional ByVal ContentName As String = "") As String
            Dim returnFilename As String = ""
            Try
                Dim fieldTypeId As Integer
                Dim TableName As String
                Dim RecordID As Integer
                Dim fieldNameUpper As String
                Dim LenOriginalFilename As Integer
                Dim LenFilename As Integer
                Dim Pos As Integer
                '
                If Not cs_ok(CSPointer) Then
                    Throw New ArgumentException("CSPointer does Not point To a valid dataset, it Is empty, Or it Is Not pointing To a valid row.")
                ElseIf FieldName = "" Then
                    Throw New ArgumentException("Fieldname Is blank")
                Else
                    fieldNameUpper = genericController.vbUCase(Trim(FieldName))
                    returnFilename = cs_getField(CSPointer, fieldNameUpper)
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
                                        RecordID = cs_getInteger(CSPointer, "ID")
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
                                If contentSetStore(CSPointer).WorkflowAuthoringMode Then
                                    TableName = .CDef.AuthoringTableName
                                Else
                                    TableName = .CDef.ContentTableName
                                End If
                            ElseIf ContentName <> "" Then
                                '
                                ' CS is SQL-based, use the contentname
                                '
                                TableName = cpCore.metaData.getContentTablename(ContentName)
                            Else
                                '
                                ' no Contentname given
                                '
                                Throw New ApplicationException("Can Not create a filename because no ContentName was given, And the csv_ContentSet Is SQL-based.")
                            End If
                            '
                            ' ----- Create filename
                            '
                            If ContentName = "" Then
                                If OriginalFilename = "" Then
                                    fieldTypeId = FieldTypeIdText
                                Else
                                    fieldTypeId = FieldTypeIdFile
                                End If
                            Else
                                fieldTypeId = .CDef.fields(FieldName.ToLower()).fieldTypeId
                            End If
                            returnFilename = genericController.csv_GetVirtualFilenameByTable(TableName, FieldName, RecordID, OriginalFilename, fieldTypeId)
                            ' 20160607 - no, if you call the cs_set, it stack-overflows. this is a get, so do not save it here.
                            'Call cs_set(CSPointer, fieldNameUpper, returnFilename)
                        End With
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnFilename
        End Function
        '
        '   csv_cs_getText
        '
        Public Function cs_getText(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            cs_getText = genericController.encodeText(cs_getField(CSPointer, FieldName))
        End Function
        '
        '   genericController.EncodeInteger( csv_cs_getField )
        '
        Public Function cs_getInteger(ByVal CSPointer As Integer, ByVal FieldName As String) As Integer
            cs_getInteger = genericController.EncodeInteger(cs_getField(CSPointer, FieldName))
        End Function
        '
        '   encodeNumber( csv_cs_getField )
        '
        Public Function cs_getNumber(ByVal CSPointer As Integer, ByVal FieldName As String) As Double
            cs_getNumber = genericController.EncodeNumber(cs_getField(CSPointer, FieldName))
        End Function
        '
        '    genericController.EncodeDate( csv_cs_getField )
        '
        Public Function cs_getDate(ByVal CSPointer As Integer, ByVal FieldName As String) As Date
            cs_getDate = genericController.EncodeDate(cs_getField(CSPointer, FieldName))
        End Function
        '
        '   genericController.EncodeBoolean( csv_cs_getField )
        '
        Public Function cs_getBoolean(ByVal CSPointer As Integer, ByVal FieldName As String) As Boolean
            cs_getBoolean = genericController.EncodeBoolean(cs_getField(CSPointer, FieldName))
        End Function
        '
        '   genericController.EncodeBoolean( csv_cs_getField )
        '
        Public Function cs_getLookup(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            cs_getLookup = cs_get(CSPointer, FieldName)
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
        Public Sub SetCSTextFile(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal Copy As String, ByVal ContentName As String)
            Try
                Dim Filename As String
                Dim OldFilename As String
                Dim OldCopy As String
                '
                If Not cs_ok(CSPointer) Then
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
                            OldFilename = cs_getText(CSPointer, FieldName)
                            Filename = cs_getFilename(CSPointer, FieldName, "", ContentName)
                            If OldFilename <> Filename Then
                                '
                                ' Filename changed, mark record changed
                                '
                                Call cpCore.privateFiles.saveFile(Filename, Copy)
                                Call cs_set(CSPointer, FieldName, Filename)
                            Else
                                OldCopy = cpCore.cdnFiles.readFile(Filename)
                                If OldCopy <> Copy Then
                                    '
                                    ' copy changed, mark record changed
                                    '
                                    Call cpCore.privateFiles.saveFile(Filename, Copy)
                                    Call cs_set(CSPointer, FieldName, Filename)
                                End If
                            End If
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================
        ''' <summary>
        ''' Get the value of a a csv_ContentSet Field for a TextFile fieldtype
        ''' (returns the content of the filename stored in the field)
        ''' 
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public Function cs_getTextFile(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            Dim returnResult As String = ""
            Try
                If Not cs_ok(CSPointer) Then
                    Throw New ArgumentException("dataset must be valid")
                ElseIf String.IsNullOrEmpty(FieldName) Then
                    Throw New ArgumentException("fieldname cannot be blank")
                Else
                    Dim Filename As String = cs_getText(CSPointer, FieldName)
                    If Not String.IsNullOrEmpty(Filename) Then
                        cs_getTextFile = cpCore.cdnFiles.readFile(Filename)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnResult
        End Function
        ''
        ''========================================================================
        '''' <summary>
        '''' set the value of a field within a csv_ContentSet
        '''' </summary>
        '''' <param name="CSPointer"></param>
        '''' <param name="FieldName"></param>
        '''' <param name="FieldValue"></param>
        'Public Sub cs_setFieldx(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Object)
        '    Try
        '        cs_set(CSPointer, FieldName, FieldValue)
        '        'Dim FieldNameLocal As String
        '        'Dim FieldNameLocalUcase As String
        '        'Dim SetNeeded As Boolean
        '        'Dim field As coreMetaDataClass.CDefFieldClass
        '        ''
        '        'If Not cs_ok(CSPointer) Then
        '        '    Throw New ArgumentException("dataset must be valid")
        '        'Else
        '        '    With contentSetStore(CSPointer)
        '        '        If Not .Updateable Then
        '        '            Throw New ApplicationException("Attempting To update an unupdateable Content Set")
        '        '        Else
        '        '            FieldNameLocal = Trim(FieldName)
        '        '            FieldNameLocalUcase = genericController.vbUCase(FieldNameLocal)
        '        '            With .CDef
        '        '                If String.IsNullOrEmpty(.Name) Then
        '        '                    Throw New ApplicationException("Dataset must specify the content to update and cannot be created from a query.")
        '        '                ElseIf Not .fields.ContainsKey(FieldName.ToLower()) Then
        '        '                    Throw New ApplicationException("setField failed because field [" & FieldName.ToLower & "] was Not found In content [" & .Name & "]")
        '        '                Else
        '        '                    field = .fields(FieldName.ToLower)
        '        '                    Select Case field.fieldTypeId
        '        '                        Case FieldTypeIdAutoIdIncrement, FieldTypeIdRedirect, FieldTypeIdManyToMany
        '        '                        '
        '        '                        ' Never set
        '        '                        '
        '        '                        Case FieldTypeIdFileTextPrivate, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript, FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdFileHTMLPrivate
        '        '                            '
        '        '                            ' Always set
        '        '                            ' TextFile, assume this call is only made if a change was made to the copy.
        '        '                            ' Use the csv_SetCSTextFile to manage the modified name and date correctly.
        '        '                            ' csv_SetCSTextFile uses this method to set the row changed, so leave this here.
        '        '                            '
        '        '                            SetNeeded = True
        '        '                        Case FieldTypeIdBoolean
        '        '                            '
        '        '                            ' Boolean - sepcial case, block on typed GetAlways set
        '        '                            If genericController.EncodeBoolean(FieldValue) <> cs_getBoolean(CSPointer, FieldName) Then
        '        '                                SetNeeded = True
        '        '                            End If
        '        '                        Case Else
        '        '                            '
        '        '                            ' Set if text of value changes
        '        '                            '
        '        '                            If genericController.encodeText(FieldValue) <> cs_getText(CSPointer, FieldName) Then
        '        '                                SetNeeded = True
        '        '                            End If
        '        '                    End Select
        '        '                End If
        '        '            End With
        '        '            If Not SetNeeded Then
        '        '                SetNeeded = SetNeeded
        '        '            Else
        '        '                If contentSetStore(CSPointer).WorkflowAuthoringMode Then
        '        '                    '
        '        '                    ' Do phantom ID replacement
        '        '                    '
        '        '                    If FieldNameLocalUcase = "ID" Then
        '        '                        FieldNameLocal = "EditSourceID"
        '        '                        FieldNameLocalUcase = genericController.vbUCase(FieldNameLocal)
        '        '                    End If
        '        '                End If
        '        '                If .writeCache.ContainsKey(FieldName.ToLower) Then
        '        '                    .writeCache.Item(FieldName.ToLower) = genericController.encodeText(FieldValue)
        '        '                Else
        '        '                    .writeCache.Add(FieldName.ToLower, genericController.encodeText(FieldValue))
        '        '                End If
        '        '                .LastUsed = DateTime.Now
        '        '            End If
        '        '        End If
        '        '    End With
        '        'End If
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '    End Try
        'End Sub
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        '
        ' refactor -- this should not be metaData
        Public Function metaData_InsertContentRecordGetID(ByVal ContentName As String, ByVal MemberID As Integer) As Integer
            Dim result As Integer = -1
            Try
                Dim CS As Integer = cs_insertRecord(ContentName, MemberID)
                If Not cs_ok(CS) Then
                    Call cs_Close(CS)
                    Throw New ApplicationException("could not insert record in content [" & ContentName & "]")
                Else
                    result = cs_getInteger(CS, "ID")
                End If
                Call cs_Close(CS)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                    If Not cs_ok(CSPointer) Then
                        Throw New ApplicationException("Could not open record [" & RecordID.ToString() & "] in content [" & ContentName & "]")
                    Else
                        Call cs_deleteRecord(CSPointer)
                    End If
                    Call cs_Close(CSPointer)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Dim CDef As cdefModel
                '
                If String.IsNullOrEmpty(ContentName.Trim()) Then
                    Throw New ArgumentException("contentName cannot be blank")
                ElseIf String.IsNullOrEmpty(Criteria.Trim()) Then
                    Throw New ArgumentException("criteria cannot be blank")
                Else
                    CDef = cpCore.metaData.getCdef(ContentName)
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
                        CSPointer = cs_open(ContentName, Criteria, , False, MemberID, True, True, "ID")
                        Do While cs_ok(CSPointer)
                            invaldiateObjectList.Add(Controllers.cacheController.getDbRecordCacheName(CDef.ContentTableName, "id", cs_getInteger(CSPointer, "id").ToString()))
                            Call cs_deleteRecord(CSPointer)
                            Call cs_goNext(CSPointer)
                        Loop
                        Call cs_Close(CSPointer)
                        Call cpCore.cache.invalidateObjectList(invaldiateObjectList)

                        '    ElseIf cpCore.siteProperties.allowWorkflowAuthoring And (CDef.AllowWorkflowAuthoring) Then
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Function cs_insertRecord(ByVal ContentName As String, Optional ByVal MemberID As Integer = -1) As Integer
            Dim returnCs As Integer = -1
            Try
                Dim DateAddedString As String
                Dim CreateKeyString As String
                Dim Criteria As String
                Dim DataSourceName As String
                Dim FieldName As String
                Dim TableName As String
                Dim WorkflowAuthoringMode As Boolean
                Dim LiveRecordID As Integer
                Dim CDef As cdefModel
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
                    CDef = cpCore.metaData.getCdef(ContentName)
                    If (CDef Is Nothing) Then
                        Throw New ApplicationException("content [" & ContentName & "] could Not be found.")
                    ElseIf (CDef.Id <= 0) Then
                        Throw New ApplicationException("content [" & ContentName & "] could Not be found.")
                    Else
                        If MemberID = -1 Then
                            MemberID = cpCore.authContext.user.id
                        End If
                        With CDef
                            WorkflowAuthoringMode = .AllowWorkflowAuthoring And cpCore.siteProperties.allowWorkflowAuthoring
                            If WorkflowAuthoringMode Then
                                '
                                ' authoring, Create Blank in Live Table
                                '
                                LiveRecordID = insertTableRecordGetId(.ContentDataSourceName, .ContentTableName, MemberID)
                                sqlList = New sqlFieldListClass
                                ' refactor -- only put edit- fields in pagecontent
                                Call sqlList.add("EDITBLANK", SQLTrue) ' Pointer)
                                Call sqlList.add("EDITSOURCEID", encodeSQLNumber(Nothing)) ' Pointer)
                                Call sqlList.add("EDITARCHIVE", SQLFalse) ' Pointer)
                                Call updateTableRecord(.ContentDataSourceName, .ContentTableName, "ID=" & LiveRecordID, sqlList)
                                '
                                ' Create default record in Edit Table
                                '
                                DataSourceName = .AuthoringDataSourceName
                                TableName = .AuthoringTableName
                            Else
                                '
                                ' no authoring, create default record in Live table
                                '
                                DataSourceName = .ContentDataSourceName
                                TableName = .ContentTableName
                            End If
                            If .fields.Count > 0 Then
                                For Each keyValuePair As KeyValuePair(Of String, CDefFieldModel) In .fields
                                    Dim field As CDefFieldModel = keyValuePair.Value
                                    With field
                                        FieldName = .nameLc
                                        If (FieldName <> "") And (Not String.IsNullOrEmpty(.defaultValue)) Then
                                            Select Case genericController.vbUCase(FieldName)
                                                Case "CREATEKEY", "DATEADDED", "CREATEDBY", "CONTENTCONTROLID", "EDITSOURCEID", "EDITARCHIVE", "EDITBLANK", "ID"
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
                                                                    LookupContentName = cpCore.metaData.getContentNameByID(.lookupContentID)
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
                            ' -- refactor -- only for pagecontent
                            If WorkflowAuthoringMode Then
                                Call sqlList.add("EDITSOURCEID", encodeSQLNumber(LiveRecordID)) ' ArrayPointer)
                                Call sqlList.add("EDITARCHIVE", SQLFalse) ' ArrayPointer)
                                Call sqlList.add("EDITBLANK", SQLFalse) ' ArrayPointer)
                            Else
                                Call sqlList.add("EDITSOURCEID", encodeSQLNumber(Nothing)) ' ArrayPointer)
                                Call sqlList.add("EDITARCHIVE", SQLFalse) ' ArrayPointer)
                                Call sqlList.add("EDITBLANK", SQLFalse) ' ArrayPointer)
                            End If
                            '
                            CreateKeyString = encodeSQLNumber(genericController.getRandomInteger)
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
                            returnCs = cs_open(ContentName, Criteria, "ID DESC", False, MemberID, WorkflowAuthoringMode, True)
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                    Throw New ArgumentException("recordId is not valid [" & RecordID & "]")
                Else
                    returnResult = cs_open(ContentName, "(ID=" & encodeSQLNumber(RecordID) & ")", , False, MemberID, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Function cs_ok(ByVal CSPointer As Integer) As Boolean
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Sub cs_copyRecord(ByVal CSSource As Integer, ByVal CSDestination As Integer)
            Try
                Dim FieldName As String
                Dim DestContentName As String
                Dim DestRecordID As Integer
                Dim DestFilename As String
                Dim SourceFilename As String
                Dim DestCDef As cdefModel
                '
                If Not cs_ok(CSSource) Then
                    Throw New ArgumentException("source dataset is not valid")
                ElseIf Not cs_ok(CSDestination) Then
                    Throw New ArgumentException("destination dataset is not valid")
                ElseIf (contentSetStore(CSDestination).CDef Is Nothing) Then
                    Throw New ArgumentException("copyRecord requires the destination dataset to be created from a cs Open or Insert, not a query.")
                Else
                    DestCDef = contentSetStore(CSDestination).CDef
                    DestContentName = DestCDef.Name
                    DestRecordID = cs_getInteger(CSDestination, "ID")
                    FieldName = cs_getFirstFieldName(CSSource)
                    Do While (Not String.IsNullOrEmpty(FieldName))
                        Select Case genericController.vbUCase(FieldName)
                            Case "ID", "EDITSOURCEID", "EDITARCHIVE"
                            Case Else
                                '
                                ' ----- fields to copy
                                '
                                Select Case cs_getFieldTypeId(CSSource, FieldName)
                                    Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                    Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript
                                        '
                                        ' ----- cdn file
                                        '
                                        SourceFilename = cs_getFilename(CSSource, FieldName, "")
                                        'SourceFilename = (csv_cs_getText(CSSource, FieldName))
                                        If (SourceFilename <> "") Then
                                            DestFilename = cs_getFilename(CSDestination, FieldName, "")
                                            'DestFilename = csv_GetVirtualFilename(DestContentName, FieldName, DestRecordID)
                                            Call cs_set(CSDestination, FieldName, DestFilename)
                                            Call cpCore.cdnFiles.copyFile(SourceFilename, DestFilename)
                                        End If
                                    Case FieldTypeIdFileTextPrivate, FieldTypeIdFileHTMLPrivate
                                        '
                                        ' ----- private file
                                        '
                                        SourceFilename = cs_getFilename(CSSource, FieldName, "")
                                        'SourceFilename = (csv_cs_getText(CSSource, FieldName))
                                        If (SourceFilename <> "") Then
                                            DestFilename = cs_getFilename(CSDestination, FieldName, "")
                                            'DestFilename = csv_GetVirtualFilename(DestContentName, FieldName, DestRecordID)
                                            Call cs_set(CSDestination, FieldName, DestFilename)
                                            Call cpCore.privateFiles.copyFile(SourceFilename, DestFilename)
                                        End If
                                    Case Else
                                        '
                                        ' ----- value
                                        '
                                        Call cs_set(CSDestination, FieldName, cs_getField(CSSource, FieldName))
                                End Select
                        End Select
                        FieldName = cs_getNextFieldName(CSSource)
                    Loop
                    Call cs_save2(CSDestination)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub   '
        '       
        '========================================================================
        ''' <summary>
        ''' Returns the Source for the csv_ContentSet
        ''' </summary>
        ''' <param name="CSPointer"></param>
        ''' <returns></returns>
        Public Function cs_getSource(ByVal CSPointer As Integer) As String
            Dim returnResult As String = ""
            Try
                If Not cs_ok(CSPointer) Then
                    Throw New ArgumentException("the dataset is not valid")
                Else
                    returnResult = contentSetStore(CSPointer).Source
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Function cs_get(ByVal CSPointer As Integer, ByVal FieldName As String) As String
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
                If Not cs_ok(CSPointer) Then
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
                                fieldValue = genericController.encodeText(cs_getField(CSPointer, FieldName))
                            Catch ex As Exception
                                Throw New ApplicationException("Error [" & ex.Message & "] reading field [" & FieldName.ToLower & "] In source [" & .Source & "")
                            End Try
                        Else
                            '
                            ' Updateable -- enterprete the value
                            '
                            'ContentName = .ContentName
                            Dim field As CDefFieldModel
                            If Not .CDef.fields.ContainsKey(FieldName.ToLower()) Then
                                Try
                                    fieldValue = genericController.encodeText(cs_getField(CSPointer, FieldName))
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
                                        RecordID = genericController.EncodeInteger(cs_getField(CSPointer, "id"))
                                        With field
                                            ContentName = cpCore.metaData.getContentNameByID(.manyToManyRuleContentID)
                                            DbTable = cpCore.metaData.getContentTablename(ContentName)
                                            SQL = "Select " & .ManyToManyRuleSecondaryField & " from " & DbTable & " where " & .ManyToManyRulePrimaryField & "=" & RecordID
                                            rs = executeSql(SQL)
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
                                    FieldValueVariant = cs_getField(CSPointer, FieldName)
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
                                                    LookupContentName = cpCore.metaData.getContentNameByID(fieldLookupId)
                                                    LookupList = field.lookupList
                                                    If (LookupContentName <> "") Then
                                                        '
                                                        ' First try Lookup Content
                                                        '
                                                        CSLookup = cs_open(LookupContentName, "ID=" & encodeSQLNumber(genericController.EncodeInteger(FieldValueVariant)), , , , , , "name", 1)
                                                        If cs_ok(CSLookup) Then
                                                            fieldValue = cs_getText(CSLookup, "name")
                                                        End If
                                                        Call cs_Close(CSLookup)
                                                    ElseIf LookupList <> "" Then
                                                        '
                                                        ' Next try lookup list
                                                        '
                                                        FieldValueInteger = genericController.EncodeInteger(FieldValueVariant) - 1
                                                        lookups = Split(LookupList, ",")
                                                        If UBound(lookups) >= FieldValueInteger Then
                                                            fieldValue = lookups(FieldValueInteger)
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
                                            Case FieldTypeIdFileTextPrivate, FieldTypeIdFileHTMLPrivate
                                                '
                                                '
                                                '
                                                fieldValue = cpCore.privateFiles.readFile(genericController.encodeText(FieldValueVariant))
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Sub cs_set(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As String)
            Try
                Dim BlankTest As String
                Dim FieldNameLc As String
                Dim SetNeeded As Boolean
                Dim fileNameNoExt As String
                Dim ContentName As String
                Dim fileName As String
                Dim pathFilenameOriginal As String
                '
                If Not cs_ok(CSPointer) Then
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
                                    Dim field As CDefFieldModel
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
                                            Case FieldTypeIdFileTextPrivate, FieldTypeIdFileHTMLPrivate
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
                                                fileNameNoExt = cs_getText(CSPointer, FieldNameLc)
                                                'FieldValue = genericController.encodeText(FieldValueVariantLocal)
                                                If FieldValue = "" Then
                                                    If fileNameNoExt <> "" Then
                                                        Call cpCore.privateFiles.deleteFile(fileNameNoExt)
                                                        'Call publicFiles.DeleteFile(fileNameNoExt)
                                                        fileNameNoExt = ""
                                                    End If
                                                Else
                                                    If fileNameNoExt = "" Then
                                                        fileNameNoExt = cs_getFilename(CSPointer, FieldName, "", ContentName)
                                                    End If
                                                    Call cpCore.privateFiles.saveFile(fileNameNoExt, FieldValue)
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
                                                pathFilenameOriginal = cs_getText(CSPointer, FieldNameLc)
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
                                                        PathFilename = cs_getFilename(CSPointer, FieldNameLc, "", ContentName)
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
                                                If genericController.EncodeBoolean(FieldValue) <> cs_getBoolean(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                End If
                                            Case FieldTypeIdText
                                                '
                                                ' Set if text of value changes
                                                '
                                                If (FieldValue.Length > 255) Then
                                                    cpCore.handleExceptionAndContinue(New ApplicationException("Text length too long saving field [" & FieldName & "], length [" & FieldValue.Length & "], but max for Text field is 255. Save will be attempted"))
                                                Else
                                                    If genericController.encodeText(FieldValue) <> cs_getText(CSPointer, FieldNameLc) Then
                                                        SetNeeded = True
                                                    End If
                                                End If
                                            Case FieldTypeIdLongText, FieldTypeIdHTML
                                                '
                                                ' Set if text of value changes
                                                '
                                                If (FieldValue.Length > 65535) Then
                                                    cpCore.handleExceptionAndContinue(New ApplicationException("Text length too long saving field [" & FieldName & "], length [" & FieldValue.Length & "], but max for LongText and Html is 65535. Save will be attempted"))
                                                Else
                                                    If genericController.encodeText(FieldValue) <> cs_getText(CSPointer, FieldNameLc) Then
                                                        SetNeeded = True
                                                    End If
                                                End If
                                            Case Else
                                                '
                                                ' Set if text of value changes
                                                '
                                                If genericController.encodeText(FieldValue) <> cs_getText(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                End If
                                        End Select
                                    End If
                                End If
                            End With
                            If Not SetNeeded Then
                                SetNeeded = SetNeeded
                            Else
                                If contentSetStore(CSPointer).WorkflowAuthoringMode Then
                                    '
                                    ' Do phantom ID replacement
                                    '
                                    If FieldNameLc = "id" Then
                                        FieldNameLc = "editsourceid"
                                    End If
                                End If
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
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        Public Sub cs_set(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Date)
            cs_set(CSPointer, FieldName, FieldValue.ToString())
        End Sub
        Public Sub cs_set(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Boolean)
            cs_set(CSPointer, FieldName, FieldValue.ToString())
        End Sub
        Public Sub cs_set(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Integer)
            cs_set(CSPointer, FieldName, FieldValue.ToString())
        End Sub
        Public Sub cs_set(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Double)
            cs_set(CSPointer, FieldName, FieldValue.ToString())
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' rollback, or undo the changes to the current row
        ''' </summary>
        ''' <param name="CSPointer"></param>
        Public Sub cs_rollBack(ByVal CSPointer As Integer)
            Try
                If Not cs_ok(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    contentSetStore(CSPointer).writeCache.Clear()
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Sub cs_save2(ByVal CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False, Optional ByVal Blockcsv_ClearBake As Boolean = False)
            Try
                Dim sqlModifiedDate As Date
                Dim sqlModifiedBy As Integer
                Dim writeCacheValueVariant As Object
                Dim UcaseFieldName As String
                Dim FieldName As String
                Dim FieldFoundCount As Integer
                Dim FieldAdminAuthorable As Boolean
                Dim FieldReadOnly As Boolean
                Dim dt As DataTable
                Dim SQL As String
                Dim SQLSetPair As String
                Dim SQLUpdate As String
                Dim SQLEditUpdate As String
                Dim SQLEditDelimiter As String
                Dim SQLLiveUpdate As String
                Dim SQLLiveDelimiter As String
                Dim SQLUnique As String = String.Empty
                Dim UniqueViolationFieldList As String = String.Empty
                Dim UniqueViolation As Boolean
                Dim RSUnique As DataTable
                Dim LiveTableName As String
                Dim LiveDataSourceName As String
                Dim LiveRecordID As Integer
                Dim EditRecordID As Integer
                Dim LiveRecordContentControlID As Integer
                Dim LiveRecordContentName As String
                Dim EditTableName As String
                Dim EditDataSourceName As String
                Dim AuthorableFieldUpdate As Boolean            ' true if an Edit field is being updated
                Dim WorkflowRenderingMode As Boolean
                Dim AllowWorkflowSave As Boolean
                Dim Copy As String
                Dim ContentID As Integer
                Dim ContentName As String
                Dim WorkflowMode As Boolean
                Dim LiveRecordInactive As Boolean
                Dim ColumnPtr As Integer
                Dim writeCacheValueText As String
                '
                If Not cs_ok(CSPointer) Then
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
                        ' ----- input is good, build sql statement
                        '
                        WorkflowRenderingMode = .WorkflowAuthoringMode
                        AllowWorkflowSave = .WorkflowEditingMode
                        '
                        With .CDef
                            LiveTableName = .ContentTableName
                            LiveDataSourceName = .ContentDataSourceName
                            EditTableName = .AuthoringTableName
                            EditDataSourceName = .AuthoringDataSourceName
                            ContentName = .Name
                            ContentID = .Id
                            WorkflowMode = .AllowWorkflowAuthoring And cpCore.siteProperties.allowWorkflowAuthoring
                        End With
                        '
                        LiveRecordID = cs_getInteger(CSPointer, "ID")
                        LiveRecordContentControlID = cs_getInteger(CSPointer, "CONTENTCONTROLID")
                        LiveRecordContentName = cpCore.metaData.getContentNameByID(LiveRecordContentControlID)
                        LiveRecordInactive = Not cs_getBoolean(CSPointer, "ACTIVE")
                        '
                        ' Get Edit Record ID
                        '
                        If Not WorkflowMode Then
                            '
                            ' Live Mode
                            '
                            EditRecordID = cs_getInteger(CSPointer, "ID")
                        ElseIf Not (WorkflowRenderingMode) Then
                            '
                            ' Workflow Live Mode, (Workflow system, but opened the live record)
                            ' need to get the Record ID manually
                            '
                            SQL = "Select ID" _
                                & " from " & EditTableName _
                                & " where editsourceid=" & LiveRecordID _
                                & " And (EditArchive=0) And (editblank=0)" _
                                & " order by id desc;"
                            dt = executeSql(SQL, EditDataSourceName)
                            If genericController.isDataTableOk(dt) Then
                                EditRecordID = genericController.EncodeInteger(getDataRowColumnName(dt.Rows(0), "ID"))
                            End If
                            dt.Dispose()
                        Else
                            '
                            ' Workflow Render or Workflow Edit mode, get the Edit Record ID from the original recordset
                            '
                            EditRecordID = cs_getInteger(CSPointer, "EDITID")
                        End If
                        '
                        SQLLiveDelimiter = ""
                        SQLLiveUpdate = ""
                        SQLLiveDelimiter = ""
                        SQLEditUpdate = ""
                        SQLEditDelimiter = ""
                        sqlModifiedDate = DateTime.Now
                        sqlModifiedBy = .OwnerMemberID
                        '
                        AuthorableFieldUpdate = False
                        FieldFoundCount = 0
                        For Each keyValuePair In .writeCache
                            FieldName = keyValuePair.Key
                            UcaseFieldName = genericController.vbUCase(FieldName)
                            writeCacheValueVariant = keyValuePair.Value
                            '
                            ' field has changed
                            '
                            If UcaseFieldName = "MODIFIEDBY" Then
                                '
                                ' capture and block it - it is hardcoded in sql
                                '
                                AuthorableFieldUpdate = True
                                sqlModifiedBy = genericController.EncodeInteger(writeCacheValueVariant)
                            ElseIf UcaseFieldName = "MODIFIEDDATE" Then
                                '
                                ' capture and block it - it is hardcoded in sql
                                '
                                AuthorableFieldUpdate = True
                                sqlModifiedDate = genericController.EncodeDate(writeCacheValueVariant)
                            Else
                                '
                                ' let these field be added to the sql
                                '
                                If UcaseFieldName = "ACTIVE" And (Not genericController.EncodeBoolean(writeCacheValueVariant)) Then
                                    '
                                    ' Record being saved inactive
                                    '
                                    LiveRecordInactive = True
                                End If
                                '
                                Dim field As CDefFieldModel = .CDef.fields(FieldName.ToLower())
                                If True Then
                                    FieldFoundCount = FieldFoundCount + 1
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
                                                SQLSetPair = FieldName & "=" & encodeSQLNumber(genericController.EncodeInteger(writeCacheValueVariant))
                                            Case FieldTypeIdCurrency, FieldTypeIdFloat
                                                SQLSetPair = FieldName & "=" & encodeSQLNumber(genericController.EncodeNumber(writeCacheValueVariant))
                                            Case FieldTypeIdBoolean
                                                SQLSetPair = FieldName & "=" & encodeSQLBoolean(genericController.EncodeBoolean(writeCacheValueVariant))
                                            Case FieldTypeIdDate
                                                SQLSetPair = FieldName & "=" & encodeSQLDate(genericController.EncodeDate(writeCacheValueVariant))
                                            Case FieldTypeIdText
                                                Copy = Left(genericController.encodeText(writeCacheValueVariant), 255)
                                                If .Scramble Then
                                                    Copy = cpCore.metaData.TextScramble(Copy)
                                                End If
                                                SQLSetPair = FieldName & "=" & encodeSQLText(Copy)
                                            Case FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdFileTextPrivate, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript, FieldTypeIdFileHTMLPrivate
                                                Copy = Left(genericController.encodeText(writeCacheValueVariant), 255)
                                                SQLSetPair = FieldName & "=" & encodeSQLText(Copy)
                                            Case FieldTypeIdLongText, FieldTypeIdHTML
                                                SQLSetPair = FieldName & "=" & encodeSQLText(genericController.encodeText(writeCacheValueVariant))
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
                                                            If useCSReadCacheMultiRow Then
                                                                .readCache(ColumnPtr, .readCacheRowPtr) = writeCacheValueVariant.ToString()
                                                            Else
                                                                .readCache(ColumnPtr, 0) = writeCacheValueVariant.ToString()
                                                            End If
                                                            Exit For
                                                        End If
                                                    Next
                                                End If
                                            End With
                                            If .UniqueName And (genericController.encodeText(writeCacheValueVariant) <> "") Then
                                                '
                                                ' ----- set up for unique name check
                                                '
                                                If (Not String.IsNullOrEmpty(SQLUnique)) Then
                                                    SQLUnique &= "Or"
                                                    UniqueViolationFieldList &= ","
                                                End If
                                                writeCacheValueText = genericController.encodeText(writeCacheValueVariant)
                                                If Len(writeCacheValueText) < 255 Then
                                                    UniqueViolationFieldList &= .nameLc & "=""" & writeCacheValueText & """"
                                                Else
                                                    UniqueViolationFieldList &= .nameLc & "=""" & Left(writeCacheValueText, 255) & "..."""
                                                End If
                                                Select Case .fieldTypeId
                                                    Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                                    Case Else
                                                        SQLUnique &= "(" & .nameLc & "=" & EncodeSQL(writeCacheValueVariant, .fieldTypeId) & ")"
                                                End Select
                                            End If
                                            If Not WorkflowMode Then
                                                '
                                                ' ----- Live mode: update live record
                                                '
                                                SQLLiveUpdate = SQLLiveUpdate & SQLLiveDelimiter & SQLSetPair
                                                SQLLiveDelimiter = ","
                                                If FieldAdminAuthorable Then
                                                    AuthorableFieldUpdate = True
                                                End If
                                            ElseIf Not WorkflowRenderingMode Then
                                                '
                                                ' ----- Workflow Live Mode
                                                '
                                                If allowWorkflowErrors And FieldAdminAuthorable Then
                                                    Throw New ApplicationException("Workflow Edit Error In content[" & ContentName & "], field[" & .nameLc & "]. A csv_ContentSet opened In non-WorkflowRenderingMode, based On a Content Definition which supports Workflow Edit, can Not update fields marked 'Authorable', non-'NotEditable', non-'ReadOnly' or 'Active'. These fields can only be updated through Edit protocols.")
                                                Else
                                                    '
                                                    ' update non-FieldAdminAuthorable in Both Records
                                                    '
                                                    SQLLiveUpdate = SQLLiveUpdate & SQLLiveDelimiter & SQLSetPair
                                                    SQLLiveDelimiter = ","
                                                    SQLEditUpdate = SQLEditUpdate & SQLEditDelimiter & SQLSetPair
                                                    SQLEditDelimiter = ","
                                                End If
                                            ElseIf Not AllowWorkflowSave Then
                                                '
                                                ' ----- Workflow Rendering mode: only allow non-authorable saves
                                                '       save non-authorables to both live and edit record
                                                '
                                                If allowWorkflowErrors And FieldAdminAuthorable Then
                                                    Throw New ApplicationException("Workflow Edit error in content[" & ContentName & "], field[" & .nameLc & "]. You can not update an Authorable field in Workflow Authoring mode without Workflow Editing enabled.")
                                                Else
                                                    '
                                                    ' update non-FieldAdminAuthorable in Both Records
                                                    '
                                                    SQLLiveUpdate = SQLLiveUpdate & SQLLiveDelimiter & SQLSetPair
                                                    SQLLiveDelimiter = ","
                                                    SQLEditUpdate = SQLEditUpdate & SQLEditDelimiter & SQLSetPair
                                                    SQLEditDelimiter = ","
                                                End If
                                            Else
                                                '
                                                ' ----- Workflow Editing mode, allow saves
                                                '
                                                If FieldAdminAuthorable Then
                                                    '
                                                    ' update authorable field in authoring record
                                                    '
                                                    AuthorableFieldUpdate = True
                                                    SQLEditUpdate = SQLEditUpdate & SQLEditDelimiter & SQLSetPair
                                                    SQLEditDelimiter = ","
                                                Else
                                                    '
                                                    ' update non-authorable field in Both Records
                                                    '
                                                    SQLLiveUpdate = SQLLiveUpdate & SQLLiveDelimiter & SQLSetPair
                                                    SQLLiveDelimiter = ","
                                                    SQLEditUpdate = SQLEditUpdate & SQLEditDelimiter & SQLSetPair
                                                    SQLEditDelimiter = ","
                                                End If
                                            End If
                                        End If
                                    End With
                                End If
                            End If
                        Next
                        '
                        ' ----- Set ModifiedBy,ModifiedDate Fields if an admin visible field has changed
                        '
                        If AuthorableFieldUpdate Then
                            If WorkflowRenderingMode Then
                                If (SQLEditUpdate <> "") Then
                                    '
                                    ' ----- Authorable Fields Updated in Authoring Mode, set Edit Record Modified
                                    '
                                    SQLEditUpdate = SQLEditUpdate & ",MODIFIEDDATE=" & encodeSQLDate(sqlModifiedDate) & ",MODIFIEDBY=" & encodeSQLNumber(sqlModifiedBy)
                                End If
                            Else
                                If (SQLLiveUpdate <> "") Then
                                    '
                                    ' ----- Authorable Fields Updated in non-Authoring Mode, set Live Record Modified
                                    '
                                    SQLLiveUpdate = SQLLiveUpdate & ",MODIFIEDDATE=" & encodeSQLDate(sqlModifiedDate) & ",MODIFIEDBY=" & encodeSQLNumber(sqlModifiedBy)
                                End If
                            End If
                        End If
                        '
                        ' not sure why, but this section was commented out.
                        ' Modified was not being set, so I un-commented it
                        '
                        If (SQLEditUpdate <> "") And (AuthorableFieldUpdate) Then
                            '
                            ' ----- set the csv_ContentSet Modified
                            '
                            Call cpCore.workflow.setAuthoringControl(ContentName, LiveRecordID, AuthoringControlsModified, .OwnerMemberID)
                        End If
                        '
                        ' ----- Do the unique check on the content table, if necessary
                        '
                        UniqueViolation = False
                        If SQLUnique <> "" Then
                            SQLUnique = "SELECT ID FROM " & LiveTableName & " WHERE (ID<>" & LiveRecordID & ")AND(" & SQLUnique & ")and(editsourceid is null)and(" & .CDef.ContentControlCriteria & ");"
                            RSUnique = executeSql(SQLUnique, LiveDataSourceName)
                            If (RSUnique.Rows.Count > 0) Then
                                UniqueViolation = True
                            End If
                            Call RSUnique.Dispose()
                        End If
                        If UniqueViolation Then
                            '
                            ' ----- trap unique violations here
                            '
                            Throw New ApplicationException(("Can not save record to content [" & LiveRecordContentName & "] because it would create a non-unique record for one or more of the following field(s) [" & UniqueViolationFieldList & "]"))
                        ElseIf (FieldFoundCount > 0) Then
                            '
                            ' ----- update live table (non-workflowauthoring and non-authorable fields)
                            '
                            If (SQLLiveUpdate <> "") Then
                                SQLUpdate = "UPDATE " & LiveTableName & " SET " & SQLLiveUpdate & " WHERE ID=" & LiveRecordID & ";"
                                Call executeSql(SQLUpdate, EditDataSourceName)
                            End If
                            '
                            ' ----- update edit table (authoring and non-authoring fields)
                            '
                            If (SQLEditUpdate <> "") Then
                                SQLUpdate = "UPDATE " & EditTableName & " SET " & SQLEditUpdate & " WHERE ID=" & EditRecordID & ";"
                                Call executeSql(SQLUpdate, EditDataSourceName)
                            End If
                            '
                            ' ----- Live record has changed
                            '
                            If AuthorableFieldUpdate And (Not WorkflowRenderingMode) Then
                                '
                                ' ----- reset the ContentTimeStamp to csv_ClearBake
                                '
                                Call cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(LiveTableName, "id", LiveRecordID.ToString()))
                                '
                                ' ----- mark the record NOT UpToDate for SpiderDocs
                                '
                                If (LCase(EditTableName) = "ccpagecontent") And (LiveRecordID <> 0) Then
                                    If isSQLTableField("default", "ccSpiderDocs", "PageID") Then
                                        SQL = "UPDATE ccspiderdocs SET UpToDate = 0 WHERE PageID=" & LiveRecordID
                                        Call executeSql(SQL)
                                    End If
                                End If
                            End If
                        End If
                        .LastUsed = DateTime.Now
                    End With
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                    Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdRedirect, FieldTypeIdManyToMany, FieldTypeIdText, FieldTypeIdFileTextPrivate, FieldTypeIdFileJavascript, FieldTypeIdFileXML, FieldTypeIdFileCSS, FieldTypeIdFileHTMLPrivate
                        returnResult = encodeSQLText(genericController.encodeText(expression))
                    Case Else
                        cpCore.handleExceptionAndContinue(New ApplicationException("Unknown Field Type [" & fieldType & ""))
                        returnResult = encodeSQLText(genericController.encodeText(expression))
                End Select
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Dim iOriginalFilename As String
                Dim CDef As cdefModel
                '
                If String.IsNullOrEmpty(ContentName.Trim()) Then
                    Throw New ArgumentException("contentname cannot be blank")
                ElseIf String.IsNullOrEmpty(FieldName.Trim()) Then
                    Throw New ArgumentException("fieldname cannot be blank")
                ElseIf (RecordID <= 0) Then
                    Throw New ArgumentException("recordid is not valid")
                Else
                    CDef = cpCore.metaData.getCdef(ContentName)
                    If CDef.Id = 0 Then
                        Throw New ApplicationException("contentname [" & ContentName & "] is not a valid content")
                    Else
                        TableName = CDef.ContentTableName
                        If TableName = "" Then
                            TableName = ContentName
                        End If
                        '
                        iOriginalFilename = genericController.encodeEmptyText(OriginalFilename, "")
                        '
                        fieldTypeId = CDef.fields(FieldName.ToLower()).fieldTypeId
                        '
                        returnResult = genericController.csv_GetVirtualFilenameByTable(TableName, FieldName, RecordID, iOriginalFilename, fieldTypeId)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Function cs_openGroupUsers(ByVal groupList As List(Of String), Optional ByVal sqlCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
            Dim returnResult As Integer
            Try
                '
                Dim rightNow As Date
                Dim sqlRightNow As String
                Dim SQL As String
                '
                rightNow = DateTime.Now
                sqlRightNow = encodeSQLDate(rightNow)
                If PageNumber = 0 Then
                    PageNumber = 1
                End If
                If PageSize = 0 Then
                    PageSize = pageSizeDefault
                End If
                '
                returnResult = -1
                If groupList.Count > 0 Then
                    '
                    ' Build Inner Query to select distinct id needed
                    '
                    SQL = "SELECT DISTINCT ccMembers.id" _
                    & " FROM (ccMembers" _
                    & " LEFT JOIN ccMemberRules ON ccMembers.ID = ccMemberRules.MemberID)" _
                    & " LEFT JOIN ccGroups ON ccMemberRules.GroupID = ccGroups.ID" _
                    & " WHERE (ccMemberRules.Active<>0)AND(ccGroups.Active<>0)"
                    '
                    ' active members
                    '
                    If ActiveOnly Then
                        SQL &= "AND(ccMembers.Active<>0)"
                    End If
                    '
                    ' list of groups
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
                    ' group expiration
                    '
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
                    returnResult = cs_openCsSql_rev("default", SQL, PageSize, PageNumber)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Dim dt As DataTable = executeSql("select ContentTableID from ccContent where name=" & encodeSQLText(ContentName))
                If Not genericController.isDataTableOk(dt) Then
                    Throw New ApplicationException("Content [" & ContentName & "] was not found in ccContent table")
                Else
                    returnResult = genericController.EncodeInteger(dt.Rows(0).Item("ContentTableID"))
                End If
                dt.Dispose()
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Dim CS As Integer
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
                    ContentName = cpCore.metaData.getContentNameByID(ContentID)
                    TableName = cpCore.metaData.getContentTablename(ContentName)
                    '
                    ' ----- Delete CalendarEventRules and CalendarEvents
                    '
                    If cpCore.metaData.isContentFieldSupported("calendar events", "ID") Then
                        Call deleteContentRecords("Calendar Events", Criteria)
                    End If
                    '
                    ' ----- Delete ContentWatch
                    '
                    CS = cs_open("Content Watch", Criteria)
                    Do While cs_ok(CS)
                        Call cs_deleteRecord(CS)
                        cs_goNext(CS)
                    Loop
                    Call cs_Close(CS)
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
                        Case "CCPATHS"
                            '
                            Call deleteContentRecords("Path Rules", "PathID=" & RecordID)
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Function cs_getRowCount(ByVal CSPointer As Integer) As Integer
            Dim returnResult As Integer
            Try
                If Not cs_ok(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    returnResult = contentSetStore(CSPointer).readCacheRowCnt
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                If Not cs_ok(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    returnResult = contentSetStore(CSPointer).fieldNames
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                    Call executeSql(SQL, DataSourceName)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                If Not cs_ok(CSPointer) Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    returnResult = contentSetStore(CSPointer).ContentName
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                    Case FieldTypeIdFileTextPrivate
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
                    Case FieldTypeIdFileHTMLPrivate
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
                cpCore.handleExceptionAndContinue(ex, "Unexpected exception")
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
        '                        LookupContentName = cpCore.metaData.getContentNameByID(fieldLookupId)
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
                    Dim CS As Integer = cs_open(ContentName, "ccguid=" & encodeSQLText(RecordGuid), "ID", , , , , "ID")
                    If cs_ok(CS) Then
                        returnResult = cs_getInteger(CS, "ID")
                    End If
                    Call cs_Close(CS)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Dim CS As Integer = cs_open(ContentName, Criteria, SortFieldList, ActiveOnly, MemberID, WorkflowRenderingMode, WorkflowEditingMode, SelectFieldList, PageSize, PageNumber)
                If cs_ok(CS) Then
                    returnRows = contentSetStore(CS).readCache
                End If
                Call cs_Close(CS)
                '
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                If Not cs_ok(CS) Then
                    Throw New ArgumentException("dataset is not valid")
                Else
                    For Each keyValuePair As KeyValuePair(Of String, CDefFieldModel) In contentSetStore(CS).CDef.fields
                        Dim field As CDefFieldModel = keyValuePair.Value
                        With field
                            FieldName = .nameLc
                            If (FieldName <> "") And (Not String.IsNullOrEmpty(.defaultValue)) Then
                                Select Case genericController.vbUCase(FieldName)
                                    Case "ID", "CCGUID", "CREATEKEY", "DATEADDED", "CREATEDBY", "CONTENTCONTROLID", "EDITSOURCEID", "EDITARCHIVE", "EDITBLANK"
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
                                                Call cs_set(CS, FieldName, "null")
                                                If DefaultValueText <> "" Then
                                                    If .lookupContentID <> 0 Then
                                                        LookupContentName = cpCore.metaData.getContentNameByID(.lookupContentID)
                                                        If LookupContentName <> "" Then
                                                            Call cs_set(CS, FieldName, getRecordID(LookupContentName, DefaultValueText))
                                                        End If
                                                    ElseIf .lookupList <> "" Then
                                                        UCaseDefaultValueText = genericController.vbUCase(DefaultValueText)
                                                        lookups = Split(.lookupList, ",")
                                                        For Ptr = 0 To UBound(lookups)
                                                            If UCaseDefaultValueText = genericController.vbUCase(lookups(Ptr)) Then
                                                                Call cs_set(CS, FieldName, Ptr + 1)
                                                                Exit For
                                                            End If
                                                        Next
                                                    End If
                                                End If
                                            Case Else
                                                '
                                                ' else text
                                                '
                                                Call cs_set(CS, FieldName, .defaultValue)
                                        End Select
                                End Select
                            End If
                        End With
                    Next
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Dim ts As tableSchemaModel = cpCore.metaData.getTableSchema(TableName, DataSourceName)
                If (ts IsNot Nothing) Then
                    For Each entry As String In ts.indexes
                        returnList &= "," & entry
                    Next
                    If returnList.Length > 0 Then
                        returnList = returnList.Substring(2)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Dim dt As DataTable = executeSql("Select ID from ccContent where name=" & encodeSQLText(ContentName))
                If dt.Rows.Count > 0 Then
                    returnContentId = genericController.EncodeInteger(dt.Rows(0).Item("id"))
                End If
                dt.Dispose()
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                CreateKeyString = cpCore.db.encodeSQLNumber(genericController.getRandomInteger)
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
                    dt = cpCore.db.insertTableRecordGetDataTable(DataSource.Name, TableName, cpCore.authContext.user.id)
                    If dt.Rows.Count > 0 Then
                        RecordID = genericController.EncodeInteger(dt.Rows(0).Item("ID"))
                        Call cpCore.db.executeSql("Update " & TableName & " Set active=0 where id=" & RecordID & ";", DataSource.Name)
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
                    ContentID = cpCore.db.getContentId(ContentName)
                    If (ContentID < 0) Then
                        '
                        ' ----- Content definition not found, create it
                        '
                        ContentIsNew = True
                        Call cpCore.metaData.createContent(True, DataSource, TableName, ContentName)
                        'ContentID = csv_GetContentID(ContentName)
                        SQL = "Select ID from ccContent where name=" & cpCore.db.encodeSQLText(ContentName)
                        dt = cpCore.db.executeSql(SQL)
                        If dt.Rows.Count = 0 Then
                            Throw New ApplicationException("Content Definition [" & ContentName & "] could Not be selected by name after it was inserted")
                        Else
                            ContentID = genericController.EncodeInteger(dt(0).Item("ID"))
                            Call cpCore.db.executeSql("update ccContent Set CreateKey=0 where id=" & ContentID)
                        End If
                        dt.Dispose()
                        cpCore.cache.invalidateAll()
                        cpCore.metaData.clear()
                    End If
                    '
                    '-----------------------------------------------------------
                    ' --- Create the ccFields records for the new table
                    '-----------------------------------------------------------
                    '
                    ' ----- locate the field in the content field table
                    '
                    SQL = "Select name from ccFields where ContentID=" & ContentID & ";"
                    dtFields = cpCore.db.executeSql(SQL)
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
                            Call cpCore.db.executeSql("update ccFields Set CreateKey=0 where (Contentid=" & ContentID & ") And (name = " & cpCore.db.encodeSQLText(UcaseTableColumnName) & ")")
                        End If
                    Next
                End If
                '
                ' Fill ContentControlID fields with new ContentID
                '
                SQL = "Update " & TableName & " Set ContentControlID=" & ContentID & " where (ContentControlID Is null);"
                Call cpCore.db.executeSql(SQL, DataSource.Name)
                '
                ' ----- Load CDef
                '       Load only if the previous state of autoload was true
                '       Leave Autoload false during load so more do not trigger
                '
                cpCore.cache.invalidateAll()
                cpCore.metaData.clear()
                dt.Dispose()
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Dim field As New CDefFieldModel
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
                    Case "EDITSOURCEID"
                        field.caption = "Edit Source"
                        field.ReadOnly = True
                        field.editSortPriority = 5090
                        field.authorable = False
                        field.defaultValue = "null"
                    Case "EDITARCHIVE"
                        field.caption = "Edit Archive"
                        field.fieldTypeId = FieldTypeIdBoolean
                        field.ReadOnly = True
                        field.editSortPriority = 5100
                        field.authorable = False
                        field.defaultValue = "0"
                    Case "EDITBLANK"
                        field.caption = "Edit Blank"
                        field.fieldTypeId = FieldTypeIdBoolean
                        field.ReadOnly = True
                        field.editSortPriority = 5110
                        field.authorable = False
                        field.defaultValue = "0"
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
                        field.fieldTypeId = FieldTypeIdFileHTMLPrivate
                        field.TextBuffered = True
                        field.editSortPriority = 2010
                    Case "BRIEFFILENAME"
                        field.caption = "Overview"
                        field.fieldTypeId = FieldTypeIdFileHTMLPrivate
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
                Call cpCore.metaData.verifyCDefField_ReturnID(ContentName, field)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return rows
        End Function
        '
        Public Sub markRecordReviewed(ContentName As String, RecordID As Integer)
            Try
                If cpCore.metaData.isContentFieldSupported(ContentName, "DateReviewed") Then
                    Dim DataSourceName As String = cpCore.metaData.getContentDataSource(ContentName)
                    Dim TableName As String = cpCore.metaData.getContentTablename(ContentName)
                    Dim SQL As String = "update " & TableName & " set DateReviewed=" & cpCore.db.encodeSQLDate(cpCore.app_startTime)
                    If cpCore.metaData.isContentFieldSupported(ContentName, "ReviewedBy") Then
                        SQL &= ",ReviewedBy=" & cpCore.authContext.user.id
                    End If
                    '
                    ' -- Mark the live record
                    Call cpCore.db.executeSql(SQL & " where id=" & RecordID, DataSourceName)
                    '
                    ' -- Mark the edit record if in workflow
                    If cpCore.metaData.isContentFieldSupported(ContentName, "editsourceid") Then
                        Call cpCore.db.executeSql(SQL & " where (editsourceid=" & RecordID & ")and(editarchive=0)", DataSourceName)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
            End Try
        End Sub
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
                dt = executeSql(SQL)
                If dt.Rows.Count > 0 Then
                    CurrentCount = genericController.EncodeInteger(dt.Rows(0).Item(0))
                End If
                Do While (CurrentCount <> 0) And (PreviousCount <> CurrentCount) And (LoopCount < iChunkCount)
                    If getDataSourceType(DataSourceName) = DataSourceTypeODBCMySQL Then
                        SQL = "delete from " & TableName & " where id in (select ID from " & TableName & " where " & Criteria & " limit " & iChunkSize & ")"
                    Else
                        SQL = "delete from " & TableName & " where id in (select top " & iChunkSize & " ID from " & TableName & " where " & Criteria & ")"
                    End If
                    Call executeSql(SQL, DataSourceName)
                    PreviousCount = CurrentCount
                    SQL = "select count(*) as RecordCount from " & TableName & " where " & Criteria
                    dt = executeSql(SQL)
                    If dt.Rows.Count > 0 Then
                        CurrentCount = genericController.EncodeInteger(dt.Rows(0).Item(0))
                    End If
                    LoopCount = LoopCount + 1
                Loop
                If (CurrentCount <> 0) And (PreviousCount = CurrentCount) Then
                    '
                    ' records did not delete
                    '
                    Call Err.Raise(ignoreInteger, "dll", "Error deleting record chunks. No records were deleted and the process was not complete.")
                ElseIf (LoopCount >= iChunkCount) Then
                    '
                    ' records did not delete
                    '
                    Call Err.Raise(ignoreInteger, "dll", "Error deleting record chunks. The maximum chunk count was exceeded while deleting records.")
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
                CSPointer = cs_open("Content Watch", "ContentRecordKey=" & encodeSQLText(ContentRecordKey), , , ,, , "Link,Clicks")
                If cs_ok(CSPointer) Then
                    main_GetLinkByContentRecordKey = cpCore.db.cs_getText(CSPointer, "Link")
                End If
                Call cpCore.db.cs_Close(CSPointer)
                '
                If main_GetLinkByContentRecordKey = "" Then
                    '
                    ' try template for this page
                    '
                    KeySplit = Split(ContentRecordKey, ".")
                    If UBound(KeySplit) = 1 Then
                        ContentID = genericController.EncodeInteger(KeySplit(0))
                        If ContentID <> 0 Then
                            ContentName = cpCore.metaData.getContentNameByID(ContentID)
                            RecordID = genericController.EncodeInteger(KeySplit(1))
                            If ContentName <> "" And RecordID <> 0 Then
                                If cpCore.metaData.getContentTablename(ContentName) = "ccPageContent" Then
                                    CSPointer = cpCore.db.csOpen2(ContentName, RecordID, , , "TemplateID,ParentID")
                                    If cs_ok(CSPointer) Then
                                        recordfound = True
                                        templateId = cs_getInteger(CSPointer, "TemplateID")
                                        ParentID = cs_getInteger(CSPointer, "ParentID")
                                    End If
                                    Call cs_Close(CSPointer)
                                    If Not recordfound Then
                                        '
                                        ' This content record does not exist - remove any records with this ContentRecordKey pointer
                                        '
                                        Call deleteContentRecords("Content Watch", "ContentRecordKey=" & encodeSQLText(ContentRecordKey))
                                        Call cpCore.db.deleteContentRules(cpCore.metaData.getContentId(ContentName), RecordID)
                                    Else

                                        If templateId <> 0 Then
                                            CSPointer = cpCore.db.csOpen2("Page Templates", templateId, , , "Link")
                                            If cs_ok(CSPointer) Then
                                                main_GetLinkByContentRecordKey = cs_getText(CSPointer, "Link")
                                            End If
                                            Call cs_Close(CSPointer)
                                        End If
                                        If main_GetLinkByContentRecordKey = "" And ParentID <> 0 Then
                                            TableName = cpCore.metaData.getContentTablename(ContentName)
                                            DataSource = cpCore.metaData.getContentDataSource(ContentName)
                                            CSPointer = cs_openCsSql_rev(DataSource, "Select ContentControlID from " & TableName & " where ID=" & RecordID)
                                            If cs_ok(CSPointer) Then
                                                ParentContentID = genericController.EncodeInteger(cs_getText(CSPointer, "ContentControlID"))
                                            End If
                                            Call cs_Close(CSPointer)
                                            If ParentContentID <> 0 Then
                                                main_GetLinkByContentRecordKey = main_GetLinkByContentRecordKey(CStr(ParentContentID & "." & ParentID), "")
                                            End If
                                        End If
                                        If main_GetLinkByContentRecordKey = "" Then
                                            DefaultTemplateLink = cpCore.siteProperties.getText("SectionLandingLink", requestAppRootPath & cpCore.siteProperties.serverPageDefault)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                    If main_GetLinkByContentRecordKey <> "" Then
                        main_GetLinkByContentRecordKey = genericController.modifyLinkQuery(main_GetLinkByContentRecordKey, "bid", CStr(RecordID), True)
                    End If
                End If
            End If
            '
            If main_GetLinkByContentRecordKey = "" Then
                main_GetLinkByContentRecordKey = DefaultLink
            End If
            '
            main_GetLinkByContentRecordKey = genericController.EncodeAppRootPath(main_GetLinkByContentRecordKey, cpCore.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, cpCore.webServer.requestDomain)
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
        '========================================================================
        '   Process manual changes needed for Page Content Special Cases
        '       If workflow, only call this routine on a publish - it changes live records
        '========================================================================
        '
        Public Sub main_ProcessSpecialCaseAfterSave(IsDelete As Boolean, ContentName As String, RecordID As Integer, RecordName As String, RecordParentID As Integer, UseContentWatchLink As Boolean)
            Dim addonId As Integer
            Dim Option_String As String
            Dim Filename As String
            Dim FilenameExt As String
            Dim FilenameNoExt As String
            Dim FilePath As String
            Dim Pos As Integer
            Dim AltSizeList As String
            'Dim innovaEditor As innovaEditorAddonClassFPO
            Dim sf As imageEditController
            Dim RebuildSizes As Boolean
            Dim AddonStatusOK As Boolean
            Dim pageContentName As String
            Dim PageContentID As Integer
            Dim rootPageId As Integer
            Dim Cmd As String
            Dim CS As Integer
            Dim TableName As String
            Dim PageName As String
            Dim ContentID As Integer
            Dim ActivityLogOrganizationID As Integer
            Dim ActivityLogName As String
            Dim hint As String
            '
            'hint = hint & ",000"
            ContentID = cpCore.metaData.getContentId(ContentName)
            TableName = cpCore.metaData.getContentTablename(ContentName)
            Call markRecordReviewed(ContentName, RecordID)
            '
            ' Test for parentid=id loop
            '
            ' needs to be finished
            '
            '    If (RecordParentID <> 0) And metaData.IsContentFieldSupported(ContentName, "parentid") Then
            '
            '    End If
            'hint = hint & ",100"
            Select Case genericController.vbLCase(TableName)
                Case "linkaliases"
                    'Call cache_linkAlias_clear
                Case "ccmembers"
                    '
                    ' Log Activity for changes to people and organizattions
                    '
                    'hint = hint & ",110"
                    CS = cpCore.db.cs_open2("people", RecordID, , , "Name,OrganizationID")
                    If cs_ok(CS) Then
                        ActivityLogOrganizationID = cs_getInteger(CS, "OrganizationID")
                    End If
                    Call cs_Close(CS)
                    If IsDelete Then
                        Call logController.logActivity2(cpCore, "deleting user #" & RecordID & " (" & RecordName & ")", RecordID, ActivityLogOrganizationID)
                    Else
                        Call logController.logActivity2(cpCore, "saving changes to user #" & RecordID & " (" & RecordName & ")", RecordID, ActivityLogOrganizationID)
                    End If
                Case "organizations"
                    '
                    ' Log Activity for changes to people and organizattions
                    '
                    'hint = hint & ",120"
                    If IsDelete Then
                        Call logController.logActivity2(cpCore, "deleting organization #" & RecordID & " (" & RecordName & ")", 0, RecordID)
                    Else
                        Call logController.logActivity2(cpCore, "saving changes to organization #" & RecordID & " (" & RecordName & ")", 0, RecordID)
                    End If
                Case "ccsetup"
                    '
                    ' Site Properties
                    '
                    'hint = hint & ",130"
                    Select Case genericController.vbLCase(RecordName)
                        Case "allowlinkalias"
                            Call cpCore.cache.invalidateContent("Page Content")
                        Case "sectionlandinglink"
                            Call cpCore.cache.invalidateContent("Page Content")
                        Case siteproperty_serverPageDefault_name
                            Call cpCore.cache.invalidateContent("Page Content")
                    End Select
                Case "ccpagecontent"
                    '
                    ' set ChildPagesFound true for parent page
                    '
                    'hint = hint & ",140"
                    If RecordParentID > 0 Then
                        Call cpCore.pages.cache_pageContent_updateRow(RecordParentID, False, False)
                        If Not IsDelete Then
                            Call executeSql("update ccpagecontent set ChildPagesfound=1 where ID=" & RecordParentID)
                        End If
                    End If
                    '
                    ' Page Content special cases for delete
                    '
                    If IsDelete Then
                        '
                        ' If this was a section's root page, clear the rootpageid so a new page will be created
                        '
                        Call executeSql("update ccsections set RootPageID=0 where RootPageID=" & RecordID)
                        Call cpCore.pages.pageManager_cache_siteSection_clear()
                        '
                        ' Clear the Landing page and page not found site properties
                        '

                        If genericController.vbLCase(TableName) = "ccpagecontent" Then
                            Call cpCore.pages.cache_pageContent_removeRow(RecordID, cpCore.pages.pagemanager_IsWorkflowRendering, False)
                            If RecordID = genericController.EncodeInteger(cpCore.siteProperties.getText("PageNotFoundPageID", "0")) Then
                                Call cpCore.siteProperties.setProperty("PageNotFoundPageID", "0")
                            End If
                            If RecordID = cpCore.siteProperties.landingPageID Then
                                cpCore.siteProperties.setProperty("landingPageId", "0")
                            End If
                        End If
                        '
                        ' Delete Link Alias entries with this PageID
                        '
                        Call executeSql("delete from cclinkAliases where PageID=" & RecordID)
                    Else
                        '
                        ' Attempt to update the PageContentCache (PCC) array stored in the PeristantVariants
                        '
                        Call cpCore.pages.cache_pageContent_updateRow(RecordID, False, False)
                    End If
                Case "cctemplates", "ccsharedstyles"
                    '
                    ' Attempt to update the PageContentCache (PCC) array stored in the PeristantVariants
                    '
                    'hint = hint & ",150"
                    Call cpCore.pages.pageManager_cache_pageTemplate_clear()
                    If Not IsNothing(cpCore.addonStyleRulesIndex) Then
                        Call cpCore.addonStyleRulesIndex.clear()
                    End If

                Case "ccaggregatefunctions"
                    '
                    ' Update wysiwyg addon menus
                    '
                    'hint = hint & ",170"
                    Call cpCore.addonLegacyCache.clear()
                    If Not IsNothing(cpCore.addonStyleRulesIndex) Then
                        Call cpCore.addonStyleRulesIndex.clear()
                    End If
                Case "ccsharedstylesaddonrules"
                    '
                    ' Update wysiwyg addon menus
                    '
                    'hint = hint & ",175"
                    If Not IsNothing(cpCore.addonStyleRulesIndex) Then
                        Call cpCore.addonStyleRulesIndex.clear()
                    End If

                    Call cpCore.addonLegacyCache.clear()
                Case "cclibraryfiles"
                    '
                    ' if a AltSizeList is blank, make large,medium,small and thumbnails
                    '
                    'hint = hint & ",180"
                    If (cpCore.siteProperties.getBoolean("ImageAllowSFResize", True)) Then
                        If Not IsDelete Then
                            CS = cpCore.db.csOpen2("library files", RecordID)
                            If cs_ok(CS) Then
                                Filename = cs_get(CS, "filename")
                                Pos = InStrRev(Filename, "/")
                                If Pos > 0 Then
                                    FilePath = Mid(Filename, 1, Pos)
                                    Filename = Mid(Filename, Pos + 1)
                                End If
                                Call cs_set(CS, "filesize", cpCore.appRootFiles.main_GetFileSize(FilePath & Filename))
                                Pos = InStrRev(Filename, ".")
                                If Pos > 0 Then
                                    FilenameExt = Mid(Filename, Pos + 1)
                                    FilenameNoExt = Mid(Filename, 1, Pos - 1)
                                    If genericController.vbInstr(1, "jpg,gif,png", FilenameExt, vbTextCompare) <> 0 Then
                                        sf = New imageEditController
                                        If sf.load(cpCore.appRootFiles.rootLocalPath & FilePath & Filename) Then
                                            '
                                            '
                                            '
                                            Call cs_set(CS, "height", sf.height)
                                            Call cs_set(CS, "width", sf.width)
                                            AltSizeList = cs_getText(CS, "AltSizeList")
                                            RebuildSizes = (AltSizeList = "")
                                            If RebuildSizes Then
                                                AltSizeList = ""
                                                '
                                                ' Attempt to make 640x
                                                '
                                                If sf.width >= 640 Then
                                                    sf.height = CInt(sf.height * (640 / sf.width))
                                                    sf.width = 640
                                                    Call sf.save(cpCore.appRootFiles.rootLocalPath & FilePath & FilenameNoExt & "-640x" & sf.height & "." & FilenameExt)
                                                    AltSizeList = AltSizeList & vbCrLf & "640x" & sf.height
                                                End If
                                                '
                                                ' Attempt to make 320x
                                                '
                                                If sf.width >= 320 Then
                                                    sf.height = CInt(sf.height * (320 / sf.width))
                                                    sf.width = 320
                                                    Call sf.save(cpCore.appRootFiles.rootLocalPath & FilePath & FilenameNoExt & "-320x" & sf.height & "." & FilenameExt)

                                                    AltSizeList = AltSizeList & vbCrLf & "320x" & sf.height
                                                End If
                                                '
                                                ' Attempt to make 160x
                                                '
                                                If sf.width >= 160 Then
                                                    sf.height = CInt(sf.height * (160 / sf.width))
                                                    sf.width = 160
                                                    Call sf.save(cpCore.appRootFiles.rootLocalPath & FilePath & FilenameNoExt & "-160x" & sf.height & "." & FilenameExt)
                                                    AltSizeList = AltSizeList & vbCrLf & "160x" & sf.height
                                                End If
                                                '
                                                ' Attempt to make 80x
                                                '
                                                If sf.width >= 80 Then
                                                    sf.height = CInt(sf.height * (80 / sf.width))
                                                    sf.width = 80
                                                    Call sf.save(cpCore.appRootFiles.rootLocalPath & FilePath & FilenameNoExt & "-180x" & sf.height & "." & FilenameExt)
                                                    AltSizeList = AltSizeList & vbCrLf & "80x" & sf.height
                                                End If
                                                Call cs_set(CS, "AltSizeList", AltSizeList)
                                            End If
                                            Call sf.Dispose()
                                            sf = Nothing
                                        End If
                                        '                                sf.Algorithm = genericController.EncodeInteger(main_GetSiteProperty("ImageResizeSFAlgorithm", "5"))
                                        '                                On Error Resume Next
                                        '                                sf.LoadFromFile (app.publicFiles.rootFullPath & FilePath & Filename)
                                        '                                If Err.Number = 0 Then
                                        '                                    Call app.SetCS(CS, "height", sf.Height)
                                        '                                    Call app.SetCS(CS, "width", sf.Width)
                                        '                                Else
                                        '                                    Err.Clear
                                        '                                End If
                                        '                                AltSizeList = cs_getText(CS, "AltSizeList")
                                        '                                RebuildSizes = (AltSizeList = "")
                                        '                                If RebuildSizes Then
                                        '                                    AltSizeList = ""
                                        '                                    '
                                        '                                    ' Attempt to make 640x
                                        '                                    '
                                        '                                    If sf.Width >= 640 Then
                                        '                                        sf.Width = 640
                                        '                                        Call sf.DoResize
                                        '                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-640x" & sf.Height & "." & FilenameExt)
                                        '                                        AltSizeList = AltSizeList & vbCrLf & "640x" & sf.Height
                                        '                                    End If
                                        '                                    '
                                        '                                    ' Attempt to make 320x
                                        '                                    '
                                        '                                    If sf.Width >= 320 Then
                                        '                                        sf.Width = 320
                                        '                                        Call sf.DoResize
                                        '                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-320x" & sf.Height & "." & FilenameExt)
                                        '                                        AltSizeList = AltSizeList & vbCrLf & "320x" & sf.Height
                                        '                                    End If
                                        '                                    '
                                        '                                    ' Attempt to make 160x
                                        '                                    '
                                        '                                    If sf.Width >= 160 Then
                                        '                                        sf.Width = 160
                                        '                                        Call sf.DoResize
                                        '                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-160x" & sf.Height & "." & FilenameExt)
                                        '                                        AltSizeList = AltSizeList & vbCrLf & "160x" & sf.Height
                                        '                                    End If
                                        '                                    '
                                        '                                    ' Attempt to make 80x
                                        '                                    '
                                        '                                    If sf.Width >= 80 Then
                                        '                                        sf.Width = 80
                                        '                                        Call sf.DoResize
                                        '                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-80x" & sf.Height & "." & FilenameExt)
                                        '                                        AltSizeList = AltSizeList & vbCrLf & "80x" & sf.Height
                                        '                                    End If
                                        '                                    Call app.SetCS(CS, "AltSizeList", AltSizeList)
                                        '                                End If
                                        '                                sf = Nothing
                                    End If
                                End If
                            End If
                            Call cs_Close(CS)
                        End If
                    End If
                Case Else
                    '
                    '
                    '
            End Select
            '
            ' Process Addons marked to trigger a process call on content change
            '
            'hint = hint & ",190"
            If True Then
                'hint = hint & ",200 content=[" & ContentID & "]"
                CS = cs_open("Add-on Content Trigger Rules", "ContentID=" & ContentID, , , , , , "addonid")
                Option_String = "" _
                    & vbCrLf & "action=contentchange" _
                    & vbCrLf & "contentid=" & ContentID _
                    & vbCrLf & "recordid=" & RecordID _
                    & ""
                Do While cs_ok(CS)
                    addonId = cs_getInteger(CS, "Addonid")
                    'hint = hint & ",210 addonid=[" & addonId & "]"
                    Call cpCore.addon.executeAddonAsProcess(CStr(addonId), Option_String)
                    Call cs_goNext(CS)
                Loop
                Call cs_Close(CS)
            End If
        End Sub
        '
        '
        '
        Public Function GetTableID(ByVal TableName As String) As Integer
            Dim result As Integer = 0
            Dim CS As Integer
            GetTableID = -1
            CS = cpCore.db.cs_openSql("Select ID from ccTables where name=" & cpCore.db.encodeSQLText(TableName), , 1)
            If cpCore.db.cs_ok(CS) Then
                result = cpCore.db.cs_getInteger(CS, "ID")
            End If
            Call cpCore.db.cs_Close(CS)
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
        Public Function csOpen2(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal WorkflowAuthoringMode As Boolean = False, Optional ByVal WorkflowEditingMode As Boolean = False, Optional ByVal SelectFieldList As String = "") As Integer
            Return cs_open(genericController.encodeText(ContentName), "(ID=" & cpCore.db.encodeSQLNumber(RecordID) & ")", , False, cpCore.authContext.user.id, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1)
        End Function
        '
        '========================================================================
        '
        Public Function cs_open2(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal WorkflowAuthoringMode As Boolean = False, Optional ByVal WorkflowEditingMode As Boolean = False, Optional ByVal SelectFieldList As String = "") As Integer
            Return cs_open(ContentName, "(ID=" & cpCore.db.encodeSQLNumber(RecordID) & ")", , False, cpCore.authContext.user.id, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1)
        End Function
        '========================================================================
        '   Determine the current persons Language
        '
        '   Return the ID in the Languages content
        '========================================================================
        '
        Public Function web_GetBrowserLanguageID() As Integer
            Dim LanguageID As Integer = 0
            Dim LanguageName As String = ""
            Call web_GetBrowserLanguage(LanguageID, LanguageName)
            web_GetBrowserLanguageID = LanguageID
        End Function
        '
        '========================================================================
        '   Determine the current persons Language
        '
        '   Return the ID in the Languages content
        '========================================================================
        '
        Public Sub web_GetBrowserLanguage(ByRef LanguageID As Integer, ByRef LanguageName As String)
            '
            Dim MethodName As String
            Dim CS As Integer
            Dim CommaPosition As Integer
            Dim DashPosition As Integer
            Dim AcceptLanguageString As String
            Dim AcceptLanguage As String
            '
            MethodName = "main_GetBrowserLanguage"
            LanguageID = 0
            LanguageName = ""
            '
            ' ----- Determine Language by browser
            '
            AcceptLanguageString = genericController.encodeText(cpCore.webServer.RequestLanguage) & ","
            CommaPosition = genericController.vbInstr(1, AcceptLanguageString, ",")
            Do While CommaPosition <> 0 And LanguageID = 0
                AcceptLanguage = Trim(Mid(AcceptLanguageString, 1, CommaPosition - 1))
                AcceptLanguageString = Mid(AcceptLanguageString, CommaPosition + 1)
                If Len(AcceptLanguage) > 0 Then
                    DashPosition = genericController.vbInstr(1, AcceptLanguage, "-")
                    If DashPosition > 1 Then
                        AcceptLanguage = Mid(AcceptLanguage, 1, DashPosition - 1)
                    End If
                    DashPosition = genericController.vbInstr(1, AcceptLanguage, ";")
                    If DashPosition > 1 Then
                        AcceptLanguage = Mid(AcceptLanguage, 1, DashPosition - 1)
                    End If
                    If Len(AcceptLanguage) > 0 Then
                        CS = cs_open("languages", "HTTP_Accept_LANGUAGE=" & encodeSQLText(AcceptLanguage), , , , , , "ID", 1)
                        If cs_ok(CS) Then
                            LanguageID = cs_getInteger(CS, "ID")
                            LanguageName = cs_getText(CS, "Name")
                        End If
                        Call cs_Close(CS)
                    End If
                End If
                CommaPosition = genericController.vbInstr(1, AcceptLanguageString, ",")
            Loop
            '
            If LanguageID = 0 Then
                '
                ' ----- no matching browser language, use site default
                '
                CS = cs_open("languages", "name=" & encodeSQLText(cpCore.siteProperties.language), , , , , , "ID", 1)
                If cs_ok(CS) Then
                    LanguageID = cs_getInteger(CS, "ID")
                    LanguageName = cs_getText(CS, "Name")
                End If
                Call cs_Close(CS)
            End If
        End Sub
        Public Sub content_SetContentCopy(ByVal CopyName As String, ByVal Content As String)
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
                CS = cs_open(ContentName, "name=" & encodeSQLText(iCopyName))
                If Not cs_ok(CS) Then
                    Call cs_Close(CS)
                    CS = cs_insertRecord(ContentName)
                End If
                If cs_ok(CS) Then
                    Call cs_set(CS, "name", iCopyName)
                    Call cs_set(CS, "Copy", iContent)
                End If
                Call cs_Close(CS)
            End If
        End Sub


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
                                Call cs_Close(CSPointer)
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

