
Option Explicit On
Option Strict On

Imports System.Data.SqlClient
Imports System.Text.RegularExpressions

Namespace Contensive.Core
    Public Class coreDbClass
        Implements IDisposable
        '
        '------------------------------------------------------------------------------------------------------------------------
        ' objects created locally and must be disposed on exit
        '------------------------------------------------------------------------------------------------------------------------
        '
        Private _addonInstall As coreAddonInstallClass
        '
        '------------------------------------------------------------------------------------------------------------------------
        ' objects passed in that are not disposed
        '------------------------------------------------------------------------------------------------------------------------
        '
        Private cpCore As coreClass
        '
        '------------------------------------------------------------------------------------------------------------------------
        ' objects created within class to dispose in dispose
        '   typically used as app.meaData.method()
        '------------------------------------------------------------------------------------------------------------------------
        '
        '------------------------------------------------------------------------------------------------------------------------
        ' internal storage
        '------------------------------------------------------------------------------------------------------------------------
        '
        Private constructed As Boolean = False                                  ' set true when contructor is finished 
        '
        ' on Db success, verified set true. If error and not verified, a simple test is run. on failure, Db disabled 
        '
        Private dbVerified As Boolean = False                                  ' set true when configured and tested - else db calls are skipped
        Private dbEnabled As Boolean = True                                    ' set true when configured and tested - else db calls are skipped
        '
        Public Const csv_DefaultPageSize = 9999
        '
        ' Private structures that can stay until this class is converted
        '
        Public Const useCSReadCacheMultiRow = True
        '
        Const csv_AllowWorkflowErrors = False
        '
        '------------------------------------------------------------------------------------------------------------------------
        ' simple lazy cached values
        '------------------------------------------------------------------------------------------------------------------------
        '
        Private dataBuildVersion_Local As String
        Friend dataBuildVersion_LocalLoaded As Boolean = False
        ''
        Private db_ContentSet() As ContentSetType2
        Public csv_ContentSetCount As Integer       ' The number of elements being used
        Public csv_ContentSetSize As Integer        ' The number of current elements in the array
        Const csv_ContentSetChunk = 50              ' How many are added at a time
        '
        ' when true, all db_csOpen, etc, will be setup, but not return any data (csv_IsCSOK false)
        ' this is used to generate the csv_ContentSet.Source so we can run a csv_GetContentRows without first opening a recordset
        '
        Private csv_OpenCSWithoutRecords As Boolean
        '
        '------------------------------------------------------------------------------------------------------------------------
        ' Application specific public values
        '------------------------------------------------------------------------------------------------------------------------
        '
        'Public Name As String
        'Public status As Integer
        'Public URLEncoder As String ' rename hashKey
        '
        Public tabcnt As Integer                              '
        '
        ' properties maintained from outside this module
        '
        Public CDefConfigSaveNeeded As Boolean          ' When true, a CDef Export is needed
        'Public AutoStart As Boolean                     ' When true, service can autostart the app if found not running
        Public AllowMonitoring As Boolean               ' When true the site monitor alarms if the site is not running
        'Public SitePropertiesOutOfDate As Boolean       ' when true, need LoadContentEngine_SiteProperties
        'Public SitePropertiesLoading As Boolean         ' when true, site properties are being loaded, other processes should wait
        'Public ContentDefinitionsOutOfDate As Boolean   ' true if a change has been made that requires a reload
        'Public DataSourcesOutOfDate As Boolean          ' true when changes made to ccDataSources, requires reload of datasources and CDefs
        'Public AllowContentAutoLoad As Boolean          ' mirrors SiteProperty. See Get/Set/Load SiteProperty for details
        '                                               '   requires mirror so it can be checked in ExecuteSQL
        Public UpgradeInProgress As Boolean             ' Block content autoload when upgrading
        'Public LoadContentEngineInProcess As Boolean    ' delays CDef calls while loading CDef cpCore.cache.
        ' Set true in ccCsvrv LoadContentDefinition
        ' Checked here, waits for up to 5 seconds
        '
        '   SQL Timeouts
        '
        Public db_SQLTimeout As Integer
        Public csv_SlowSQLThreshholdMSec As Integer        '
        Public DefaultConnectionString As String
        '
        'Public DataBuildVersion_DontUseThis As String               ' the build version of the database, valid only after start
        '
        Public dataSources() As dataSourceClass         ' array from the appServices object
        '
        '
        ' ----- ContentField Type
        '       Stores information about fields in a content set
        '
        Private Structure ContentSetWriteCacheType
            Dim Name As String
            Dim Caption As String
            Dim ValueVariant As Object
            Dim fieldType As Integer
            Dim Changed As Boolean                  ' If true, the next csv_SaveCSRecord will save this field
        End Structure

        '
        ' ----- csv_ContentSet Type
        '       Stores pointers to open recordsets of content being used by the page
        '
        Private Structure ContentSetType2
            Dim IsOpen As Boolean                   ' If true, it is in use
            Dim LastUsed As Date                    ' The date/time this csv_ContentSet was last used
            Dim Updateable As Boolean               ' Can not update an csv_OpenCSSQL because Fields are not accessable
            Dim NewRecord As Boolean                ' true if it was created here
            'ContentPointer as integer              ' Pointer to the content for this Set
            Dim ContentName As String
            Dim CDef As coreMetaDataClass.CDefClass
            Dim OwnerMemberID As Integer               ' ID of the member who opened the csv_ContentSet
            '
            ' Workflow editing modes
            '
            Dim WorkflowAuthoringMode As Boolean    ' if true, these records came from the AuthoringTable, else ContentTable
            Dim WorkflowEditingRequested As Boolean ' if true, the CS was opened requesting WorkflowEditingMode
            Dim WorkflowEditingMode As Boolean      ' if true, the current record can be edited, else just rendered (effects EditBlank and csv_SaveCSRecord)
            '
            ' ----- Write Cache
            '
            Dim writeCache As Dictionary(Of String, String)
            'Dim writeCacheChanged As Boolean          ' if true, writeCache contains changes
            'Dim writeCache() As ContentSetWriteCacheType ' array of fields buffered for this set
            'Dim writeCacheSize As Integer                ' the total number of fields in the row
            'Dim writeCacheCount As Integer               ' the number of field() values to write
            Dim IsModified As Boolean               ' Set when CS is opened and if a save happens
            '
            ' ----- Recordset used to retrieve the results
            '
            Dim dt As DataTable                        ' The Recordset
            'RSOpen As Boolean                   ' true if the recordset is open
            'EOF As Boolean                      ' if true, Row is empty and at end of records
            ' ##### new way 4/19/2004
            '   readCache stores only the current row
            '   RS holds all other rows
            '   csv_GetCSRow returns the readCache
            '   csv_NextCSRecord saves the difference between the readCache and the writeCache, and movesnext, inc ResultachePointer
            '   csv_LoadreadCache stores the current RS row to the readCache
            '
            '
            ' ##### old way
            ' Storage for the RecordSet results (future)
            '       Result - refers to the entire set of rows the the SQL (Source) returns
            '       readCache - the block of records currently stored in member (readCacheTop to readCacheTop+PageSize-1)
            '       readCache is initially loaded with PageSize records, starting on page PageNumber
            '       csv_NextCSRecord increments readCacheRowPtr
            '           If readCacheRowPtr > readCacheRowCnt-1 then csv_LoadreadCache
            '       EOF true if ( readCacheRowPtr > readCacheRowCnt-1 ) and ( readCacheRowCnt < PageSize )
            '
            Dim Source As String                    ' Holds the SQL that created the result set
            Dim DataSource As String                ' The Datasource of the SQL that created the result set
            Dim PageSize As Integer                    ' Number of records in a cache page
            Dim PageNumber As Integer                  ' The Page that this result starts with
            '
            ' ----- Read Cache
            '
            Dim fieldPointer As Integer             ' ptr into fieldNames used for getFirstField, getnext, etc.
            ' deprecate these and use the dt.columns ecollection instead
            Dim fieldNames() As String              ' 1-D array of the result field names
            Dim ResultColumnCount As Integer        ' number of columns in the fieldNames and readCache
            'deprecated, but leave here for the test - useMultiRowCache
            Dim ResultEOF As Boolean                ' readCache is at the last record
            '
            ' ----- Read Cache
            '
            Dim readCache As String(,)            ' 2-D array of the result rows/columns
            Dim readCacheRowCnt As Integer         ' number of rows in the readCache
            Dim readCacheRowPtr As Integer         ' Pointer to the current result row, first row is 0, BOF is -1
            '
            ' converted array to dictionary - Dim FieldPointer As Integer                ' Used for GetFirstField, GetNextField, etc
            '
            Dim SelectTableFieldList As String      ' comma delimited list of all fields selected, in the form table.field
            'Rows as object                     ' getRows read during csv_InitContentSetResult
        End Structure
        '
        ' Tracing - Debugging
        '
        Private TimerTraceOn As Boolean
        Private TimerTraceFilename As String
        Private TimerTraceCnt As Integer
        Private TimerTraceSize As Integer
        Private Const TimerTraceChunk = 100
        Private TimerTrace() As String
        '
        Public csv_ConnectionHandleLocal As Integer = 0              ' Local storage for connection handle established when appServices opened

        Private csv_TransactionSerialNumber As Integer
        '
        ' Persistent Variant Storage Collection
        '
        Public docCache As New Collection
        Public docCacheUsedKeyList As String
        '
        ' Persistent Object Storage Collection
        '
        Public PersistentObjects As New Collection
        Public PersistentObjectUsedKeyList As String
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
                '
                ' called during core constructor - so cpcore is not valid
                '   read from cache and deserialize
                '   if not in cache, build it from scratch
                '   eventually, setup public properties as indivisual lazyCache
                '
                Me.cpCore = cpCore
                constructed = True
                ReDim dataSources(0)
                dataSources(0) = New dataSourceClass()
                db_SQLTimeout = 30
                csv_SlowSQLThreshholdMSec = 1000
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw (ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' return the correctly formated connection string for a connection to the cluster's database (default connection no catalog) -- used to create new catalogs (appication databases) in the database
        ''' </summary>
        ''' <returns></returns>
        Public Function getMasterADONETConnectionString() As String
            Return getConnectionStringADONET("", "")
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return the correctly formated connection string for this datasource. Called only from within this class
        ''' </summary>
        ''' <returns>
        ''' </returns>
        Private Function getConnectionStringADONET(catalogName As String, dataSourceName As String) As String
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
                Dim normalizedDataSourceName As String = dataSourceName.Trim().ToLower()
                Dim masterConnString As String = ""
                Dim defaultDataSourceConnString As String = ""
                Dim serverUrl As String
                serverUrl = cpCore.clusterConfig.defaultDataSourceAddress
                If (serverUrl.IndexOf(":") > 0) Then
                    serverUrl = serverUrl.Substring(0, serverUrl.IndexOf(":"))
                End If
                'If Not String.IsNullOrEmpty(provider) Then
                '    '
                '    ' add provider if required by connection
                '    '
                '    masterConnString &= "Provider=" & provider & ";"
                '    'masterConnString &= "Provider=SQLOLEDB;"
                'End If
                ''
                masterConnString &= "" _
                    & "server=" & serverUrl & ";" _
                    & "User Id=" & cpCore.clusterConfig.defaultDataSourceUsername & ";" _
                    & "Password=" & cpCore.clusterConfig.defaultDataSourcePassword & ";" _
                    & ""
                ''
                'masterConnString &= "" _
                '    & "data source=" & dataSourceUrl & ";" _
                '    & "UID=" & cpCore.clusterConfig.defaultDataSourceUsername & ";" _
                '    & "PWD=" & cpCore.clusterConfig.defaultDataSourcePassword & ";" _
                '    & ""
                If String.IsNullOrEmpty(catalogName) Then
                    '
                    ' if no catalog, uses masterConnectionString
                    '
                    returnConnString = masterConnString
                Else
                    defaultDataSourceConnString = masterConnString & "Database=" & catalogName & ";"
                    'defaultDataSourceConnString = masterConnString & "initial catalog=" & catalogName & ";"
                    If (String.IsNullOrEmpty(normalizedDataSourceName)) Or (normalizedDataSourceName = "default") Then
                        '
                        ' use default datasource
                        '
                        returnConnString = defaultDataSourceConnString
                    ElseIf (_dataSourceDictionary.ContainsKey(normalizedDataSourceName)) Then
                        returnConnString = _dataSourceDictionary(normalizedDataSourceName)
                    Else
                        Dim sql As String = "select connString from ccDataSources where name=" & cpCore.db.encodeSQLText(normalizedDataSourceName) & " order by id"
                        Using dt As DataTable = executeSql_getDataTable_internal(sql, defaultDataSourceConnString, 0, 1)
                            If (dt Is Nothing) Then
                                Throw New ApplicationException("dataSourceName [" & dataSourceName & "] is not valid.")
                            ElseIf (dt.Rows.Count = 0) Then
                                Throw New ApplicationException("dataSourceName [" & dataSourceName & "] is not valid.")
                            Else
                                returnConnString = dt.Rows(0).Field(Of String)("connString")
                                _dataSourceDictionary.Add(normalizedDataSourceName, returnConnString)
                            End If
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnConnString
        End Function
        Private _dataSourceDictionary As New Dictionary(Of String, String)
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
                Dim normalizedDataSourceName As String = dataSourceName.Trim().ToLower()
                Dim masterConnString As String = ""
                Dim defaultDataSourceConnString As String = ""
                Dim serverUrl As String
                serverUrl = cpCore.clusterConfig.defaultDataSourceAddress
                If (serverUrl.IndexOf(":") > 0) Then
                    serverUrl = serverUrl.Substring(0, serverUrl.IndexOf(":"))
                End If
                'If Not String.IsNullOrEmpty(provider) Then
                '    '
                '    ' add provider if required by connection
                '    '
                '    masterConnString &= "Provider=" & provider & ";"
                '    'masterConnString &= "Provider=SQLOLEDB;"
                'End If
                ''
                masterConnString &= "" _
                    & "Provider=sqloledb;" _
                    & "Data Source=" & serverUrl & ";" _
                    & "User Id=" & cpCore.clusterConfig.defaultDataSourceUsername & ";" _
                    & "Password=" & cpCore.clusterConfig.defaultDataSourcePassword & ";" _
                    & ""
                ''
                'masterConnString &= "" _
                '    & "data source=" & dataSourceUrl & ";" _
                '    & "UID=" & cpCore.clusterConfig.defaultDataSourceUsername & ";" _
                '    & "PWD=" & cpCore.clusterConfig.defaultDataSourcePassword & ";" _
                '    & ""
                If String.IsNullOrEmpty(catalogName) Then
                    '
                    ' if no catalog, uses masterConnectionString
                    '
                    returnConnString = masterConnString
                Else
                    defaultDataSourceConnString = masterConnString & "Initial Catalog=" & catalogName & ";"
                    'defaultDataSourceConnString = masterConnString & "initial catalog=" & catalogName & ";"
                    If (String.IsNullOrEmpty(normalizedDataSourceName)) Or (normalizedDataSourceName = "default") Then
                        '
                        ' use default datasource
                        '
                        returnConnString = defaultDataSourceConnString
                    ElseIf (_dataSourceDictionary.ContainsKey(normalizedDataSourceName)) Then
                        returnConnString = _dataSourceDictionary(normalizedDataSourceName)
                    Else
                        Dim sql As String = "select connString from ccDataSources where name=" & cpCore.db.encodeSQLText(normalizedDataSourceName) & " order by id"
                        Using dt As DataTable = executeSql_getDataTable_internal(sql, defaultDataSourceConnString, 0, 1)
                            If (dt Is Nothing) Then
                                Throw New ApplicationException("dataSourceName [" & dataSourceName & "] is not valid.")
                            ElseIf (dt.Rows.Count = 0) Then
                                Throw New ApplicationException("dataSourceName [" & dataSourceName & "] is not valid.")
                            Else
                                returnConnString = dt.Rows(0).Field(Of String)("connString")
                                _dataSourceDictionary.Add(normalizedDataSourceName, returnConnString)
                            End If
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnConnString
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Create a new catalog in the database
        ''' </summary>
        ''' <param name="catalogName"></param>
        Public Sub createCatalog(catalogName As String)
            Try
                executeMasterSql_ReturnDataTable("create database " + catalogName)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Check if the database exists
        ''' </summary>
        ''' <param name="catalog"></param>
        ''' <returns></returns>
        Public Function checkCatalogExists(catalog As String) As Boolean
            Dim returnOk As Boolean = False
            Try
                Dim sql As String
                Dim databaseId As Integer = 0
                Dim dt As DataTable
                '
                sql = String.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", catalog)
                dt = executeMasterSql_ReturnDataTable(sql)
                returnOk = (dt.Rows.Count > 0)
                dt.Dispose()
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnOk
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
        Public Function executeMasterSql_ReturnDataTable(ByVal sql As String) As DataTable
            Dim returnData As New DataTable
            Try
                Dim connString As String = getMasterADONETConnectionString()
                If dbEnabled Then
                    Using connSQL As New SqlConnection(connString)
                        connSQL.Open()
                        Using cmdSQL As New SqlCommand()
                            cmdSQL.CommandType = Data.CommandType.Text
                            cmdSQL.CommandText = sql
                            cmdSQL.Connection = connSQL
                            Using adptSQL = New SqlClient.SqlDataAdapter(cmdSQL)
                                adptSQL.Fill(returnData)
                            End Using
                        End Using
                    End Using
                    dbVerified = True
                End If
            Catch ex As Exception
                Dim newEx As New ApplicationException("Exception [" & ex.Message & "] executing master sql [" & sql & "]", ex)
                cpCore.handleExceptionAndRethrow(newEx)
            End Try
            Return returnData
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
        Public Function executeSql_getDataTable(ByVal sql As String, Optional ByVal dataSourceName As String = "", Optional ByVal startRecord As Integer = 0, Optional ByVal maxRecords As Integer = 9999) As DataTable
            Dim returnData As New DataTable
            Try
                returnData = executeSql_getDataTable_internal(sql, getConnectionStringADONET(cpCore.appConfig.name, dataSourceName), startRecord, maxRecords)
            Catch ex As Exception
                Dim newEx As New ApplicationException("Exception [" & ex.Message & "] executing sql [" & sql & "], datasource [" & dataSourceName & "], startRecord [" & startRecord & "], maxRecords [" & maxRecords & "]", ex)
                cpCore.handleExceptionAndRethrow(newEx)
            End Try
            Return returnData
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' execute a command given the connection string. This method is provided to allow getConnectionString() to query the dataSources table. startRecord is 0 based. maxRecords=0 returns all rows.
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <param name="connString"></param>
        ''' <param name="startRecord"></param>
        ''' <param name="maxRecords"></param>
        ''' <returns></returns>
        Private Function executeSql_getDataTable_internal(ByVal sql As String, ByVal connString As String, ByVal startRecord As Integer, ByVal maxRecords As Integer) As DataTable
            '
            ' REFACTOR
            ' consider writing cs intrface to sql dataReader object -- one row at a time, vaster.
            ' https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqldatareader.aspx
            '
            Dim returnData As New DataTable
            Try
                If dbEnabled Then
                    Using connSQL As New SqlConnection(connString)
                        connSQL.Open()
                        Using cmdSQL As New SqlCommand()
                            cmdSQL.CommandType = Data.CommandType.Text
                            cmdSQL.CommandText = sql
                            cmdSQL.Connection = connSQL
                            Using adptSQL = New SqlClient.SqlDataAdapter(cmdSQL)
                                adptSQL.Fill(startRecord, maxRecords, returnData)
                            End Using
                        End Using
                    End Using
                    dbVerified = True
                End If
            Catch ex As Exception
                Dim newEx As New ApplicationException("Exception [" & ex.Message & "] executing sql [" & sql & "], connString [" & connString & "], startRecord [" & startRecord & "], maxRecords [" & maxRecords & "]", ex)
                cpCore.handleExceptionAndRethrow(newEx)
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
            ' from - https://support.microsoft.com/en-us/kb/308611
            '
            ' REFACTOR 
            ' - add start recrod And max record in
            ' - add dataSourceName into the getConnectionString call - if no dataSourceName, return catalog in cluster Db, else return connstring
            '
            'Dim cn As ADODB.Connection = New ADODB.Connection()
            Dim rs As ADODB.Recordset = New ADODB.Recordset()
            Dim connString As String = getConnectionStringOLEDB(cpCore.appConfig.name, dataSourceName)
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
                cpCore.handleExceptionAndRethrow(newEx)
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
            Try
                If dbEnabled Then
                    Dim connString As String = getConnectionStringADONET(cpCore.appConfig.name, dataSourceName)
                    Using connSQL As New SqlConnection(connString)
                        connSQL.Open()
                        Using cmdSQL As New SqlCommand()
                            cmdSQL.CommandType = Data.CommandType.Text
                            cmdSQL.CommandText = sql
                            cmdSQL.Connection = connSQL
                            cmdSQL.BeginExecuteNonQuery()
                        End Using
                    End Using
                    dbVerified = True
                End If
            Catch ex As Exception
                Dim newEx As New ApplicationException("Exception [" & ex.Message & "] executing sql async [" & sql & "], datasource [" & dataSourceName & "]", ex)
                cpCore.handleExceptionAndRethrow(newEx)
                Throw newEx
            End Try
        End Sub
        '
        '
        '
        Public ReadOnly Property siteproperty_dataBuildVersion As String
            Get
                Dim returnString = ""
                Try

                    If Not dataBuildVersion_LocalLoaded Then
                        dataBuildVersion_Local = cpCore.siteProperties.getText("BuildVersion", "")
                        If dataBuildVersion_Local = "" Then
                            dataBuildVersion_Local = "0.0.000"
                        End If
                        dataBuildVersion_LocalLoaded = True
                    End If
                    returnString = dataBuildVersion_Local
                Catch ex As Exception
                    cpCore.handleExceptionAndRethrow(ex)
                End Try
                Return returnString
            End Get
        End Property
        '
        '========================================================================================
        ' ----- Get a DataSource Pointer from its name
        '       DataSources must already be loaded
        '       If not found, pointer 0 is returned for default
        '       Returns -1 if there are no datasources
        '========================================================================================
        '
        Public Function db_GetDataSourcePointer(ByVal DataSourceName As String) As Integer
            If (DataSourceName <> "") And (DataSourceName <> "-1") And (DataSourceName.ToLower() <> "default") Then
                Throw New NotImplementedException("only supports default datasource")
            End If
            On Error GoTo ErrorTrap
            '
            Dim DataSourcePointer As Integer
            Dim lcaseDataSourceName As String
            Dim MethodName As String
            '
            MethodName = "csv_GetDataSourcePointer"
            '
            db_GetDataSourcePointer = 0
            lcaseDataSourceName = "default"
            If dataSources.Length <= 0 Then
                '
                ' no datasources loaded
                '
                Call handleLegacyClassError1(MethodName, "Datasource [" & DataSourceName & "] was not found because there are no datasources loaded, the default was used")
            ElseIf (DataSourceName = "") Or (DataSourceName = "-1") Then
                '
                ' Blank or -1 should be default datasource (compatibility)
                '
                lcaseDataSourceName = "default"
            Else
                '
                ' Set ucas
                '
                lcaseDataSourceName = DataSourceName.ToLower
            End If
            '
            ' search for datasource
            '
            For DataSourcePointer = 0 To dataSources.Length - 1
                If dataSources(DataSourcePointer).NameLower = lcaseDataSourceName Then
                    Exit For
                End If
            Next
            '
            '
            '
            If (DataSourcePointer >= dataSources.Length) Then
                '
                ' Not found
                '
                Call handleLegacyClassError1(MethodName, "Datasource [" & DataSourceName & "] was not found, the default datasource was used")
            Else
                db_GetDataSourcePointer = DataSourcePointer
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError1(MethodName, "trap")
        End Function

        ''
        '
        '========================================================================
        ' Get a Contents ID from the ContentName
        '   Returns -1 if not found
        '========================================================================
        '
        Public Function db_GetContentID(ByVal ContentName As String) As Integer
            Dim returnId As Integer
            Try
                Dim cdef As coreMetaDataClass.CDefClass
                '
                cdef = cpCore.metaData.getCdef(ContentName)
                If cdef Is Nothing Then
                    returnId = -1
                Else
                    returnId = cdef.Id
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnId
        End Function
        '
        '========================================================================
        '   Update a record in a table
        '========================================================================
        '
        Public Sub db_UpdateTableRecord(ByVal DataSourceName As String, ByVal TableName As String, ByVal Criteria As String, sqlList As sqlFieldListClass)
            Try
                Dim SQL As String
                Dim SQLDelimiter As String
                '
                SQL = "UPDATE " & TableName & " SET " & sqlList.getNameValueList
                SQL &= " WHERE " & Criteria & ";"
                Call executeSql_getDataTable(SQL, DataSourceName)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '========================================================================
        ' csv_InsertTableRecord and return the ID
        '   Inserts a record into a table and returns the ID
        '========================================================================
        '
        Public Function db_InsertTableRecordGetID(ByVal DataSourceName As String, ByVal TableName As String, Optional ByVal MemberID As Integer = 0) As Integer
            Dim returnId As Integer = 0
            Try
                Using dt As DataTable = db_InsertTableRecordGetDataTable(DataSourceName, TableName, MemberID)
                    If dt.Rows.Count > 0 Then
                        returnId = EncodeInteger(dt.Rows(0).Item("id"))
                    End If
                End Using
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnId
        End Function
        '
        '========================================================================
        '   Insert a record in a table, select it and return a recordset
        '========================================================================
        '
        Public Function db_InsertTableRecordGetDataTable(ByVal DataSourceName As String, ByVal TableName As String, Optional ByVal MemberID As Integer = 0) As DataTable
            Dim returnDt As DataTable = Nothing
            Try
                Dim sqlList As New sqlFieldListClass
                Dim CreateKeyString As String
                Dim DateAddedString As String
                '
                CreateKeyString = encodeSQLNumber(getRandomLong)
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
                Call db_InsertTableRecord(DataSourceName, TableName, sqlList)
                returnDt = db_openTable(DataSourceName, TableName, "(DateAdded=" & DateAddedString & ")and(CreateKey=" & CreateKeyString & ")", "ID DESC",, 1)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnDt
        End Function
        '
        '========================================================================
        '   Insert a record in a table
        '========================================================================
        '
        Public Sub db_InsertTableRecord(ByVal DataSourceName As String, ByVal TableName As String, sqlList As sqlFieldListClass)
            Try
                Dim sql As String
                '
                If sqlList.count > 0 Then
                    sql = "INSERT INTO " & TableName & "(" & sqlList.getNameList & ")values(" & sqlList.getValueList & ")"
                    Call executeSql_getDataTable(sql, DataSourceName)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try

            '            '
            '            Dim MethodName As String
            '            Dim SQL As String
            '            Dim SQLNames As String
            '            Dim SQLValues As String
            '            Dim SQLEnd As String
            '            Dim SQLDelimiter As String
            '            Dim ArrayPointer As Integer
            '            Dim ArrayCount As Integer
            '            '
            '            MethodName = "csv_InsertTableRecord"
            '            '
            '            SQLEnd = ";"
            '            ArrayCount = UBound(SQLNameArray)
            '            If ArrayCount > 0 Then
            '                SQLNames = "("
            '                SQLValues = ")VALUES("
            '                SQLEnd = ");"
            '                SQLDelimiter = ""
            '                For ArrayPointer = 0 To ArrayCount
            '                    If SQLNameArray(ArrayPointer) <> "" Then
            '                        SQLNames = SQLNames & SQLDelimiter & SQLNameArray(ArrayPointer)
            '                        SQLValues = SQLValues & SQLDelimiter & SQLValueArray(ArrayPointer)
            '                        SQLDelimiter = ","
            '                    End If
            '                Next
            '            End If
            '            SQL = "INSERT INTO " & TableName & SQLNames & SQLValues & SQLEnd
            '            Call executeSql(SQL, DataSourceName)
            '            Exit Sub
            '            '
            '            ' ----- Error Trap
            '            '
            'ErrorTrap:
            '            handleLegacyClassError1(MethodName, "unknown")
        End Sub
        '
        '========================================================================
        ' Opens the table specified and returns the data in a recordset
        '
        '   Returns all the active records in the table
        '   Find the content record first, just for the dataSource.
        '========================================================================
        '
        Public Function db_openTable(ByVal DataSourceName As String, ByVal TableName As String, Optional ByVal Criteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal SelectFieldList As String = "", Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As DataTable
            On Error GoTo ErrorTrap
            '
            Dim SQL As String
            Dim MethodName As String
            Dim iSelectFieldList As String
            '
            MethodName = "csv_OpenRSTable"
            '
            SQL = "SELECT"
            If SelectFieldList = "" Then
                SQL &= " *"
            Else
                SQL &= " " & SelectFieldList
            End If
            SQL &= " FROM " & TableName
            If Criteria <> "" Then
                SQL &= " WHERE (" & Criteria & ")"
            End If
            If SortFieldList <> "" Then
                SQL &= " ORDER BY " & SortFieldList
            End If
            SQL &= ";"
            db_openTable = executeSql_getDataTable(SQL, DataSourceName, (PageNumber - 1) * PageSize, PageSize)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError1(MethodName, "csv_OpenRSTable")
        End Function
        '
        '
        '
        Private Sub db_SaveSlowQueryLog(ByVal TransactionTickCount As Integer, ByVal appname As String, ByVal SQL As String)
            cpCore.appendLogWithLegacyRow(appname, "query time  " & GetIntegerString(TransactionTickCount, 7) & "ms: " & SQL, "dll", "cpCoreClass", "csv_ExecuteSQL", 0, "", SQL, False, True, "", "Performance", "SlowSQL")
        End Sub

        '
        '
        '
        Private Sub db_SaveTransactionLog(ByVal LogEntry As String)
            On Error GoTo ErrorTrap
            '
            Dim Message As String
            '
            If LogEntry <> "" Then
                Message = vbReplace(LogEntry, vbCr, "")
                Message = vbReplace(Message, vbLf, "")
                cpCore.log_appendLog(Message, "DbTransactions")
                'Call csv_AppendFile(getDataPath() & "\logs\Trans" & CStr(CLng(Int(Now()))) & ".log", Message & vbCrLf)
            End If
            '
            Exit Sub
            '
ErrorTrap:
            Call handleLegacyClassError1("csv_SaveTransactionLog", "trap")
        End Sub
        '
        '   Finds the where clause (first WHERE not in single quotes)
        '   returns 0 if not found, otherwise returns locaion of word where
        '
        Private Function db_GetSQLWherePosition(ByVal SQL As String) As Integer
            On Error GoTo ErrorTrap
            '
            Dim MethodName As String
            Dim QuoteCount As Integer
            Dim QuotePosition As Integer
            Dim WherePosition As Integer
            Dim SearchDone As Boolean
            '
            MethodName = "csv_GetSQLWherePosition"
            '
            db_GetSQLWherePosition = 0
            If isInStr(1, SQL, "WHERE", vbTextCompare) Then
                '
                ' ----- contains the word "WHERE", now weed out if not a where clause
                '
                db_GetSQLWherePosition = InStrRev(SQL, " WHERE ", , vbTextCompare)
                If db_GetSQLWherePosition = 0 Then
                    db_GetSQLWherePosition = InStrRev(SQL, ")WHERE ", , vbTextCompare)
                    If db_GetSQLWherePosition = 0 Then
                        db_GetSQLWherePosition = InStrRev(SQL, " WHERE(", , vbTextCompare)
                        If db_GetSQLWherePosition = 0 Then
                            db_GetSQLWherePosition = InStrRev(SQL, ")WHERE(", , vbTextCompare)
                        End If
                    End If
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError1(MethodName, "trap")
        End Function

        ''
        ''========================================================================
        ''   Returns true if the field exists in the table
        ''========================================================================
        ''
        'public Function csv_IsSQLTableField(ByVal DataSourceName As String, ByVal TableName As String, ByVal FieldName As String)
        '    csv_IsSQLTableField = csv_IsSQLTableField(DataSourceName, TableName, FieldName)
        'End Function
        '
        '========================================================================
        '   Returns true if the field exists in the table
        '========================================================================
        '
        Public Function db_IsSQLTableField(ByVal DataSourceName As String, ByVal TableName As String, ByVal FieldName As String) As Boolean
            Dim returnOK As Boolean = False
            Try
                Dim tableSchema As coreMetaDataClass.tableSchemaClass
                '
                tableSchema = cpCore.metaData.getTableSchema(TableName, DataSourceName)
                If (Not tableSchema Is Nothing) Then
                    returnOK = tableSchema.columns.Contains(FieldName.ToLower)
                End If
            Catch ex As Exception
                Call handleLegacyClassError1("csv_IsSQLTableField", "trap")
            End Try
            Return returnOK
        End Function
        '
        '========================================================================
        '   Returns true if the table exists
        '========================================================================
        '
        Public Function db_IsSQLTable(ByVal DataSourceName As String, ByVal TableName As String) As Boolean
            Dim ReturnOK As Boolean = False
            Try
                ReturnOK = (Not cpCore.metaData.getTableSchema(TableName, DataSourceName) Is Nothing)
            Catch ex As Exception
                Call handleLegacyClassError1("csv_IsSQLTable", "trap")
            End Try
            Return ReturnOK
        End Function
        '
        '========================================================================
        ' Check for a table in a datasource
        '   if the table is missing, create the table and the core fields
        '       if NoAutoIncrement is false or missing, the ID field is created as an auto incremenet
        '       if NoAutoIncrement is true, ID is created an an long
        '   if the table is present, check all core fields
        '========================================================================
        '
        Public Sub db_CreateSQLTable(ByVal DataSourceName As String, ByVal TableName As String, Optional ByVal AllowAutoIncrement As Boolean = True)
            On Error GoTo ErrorTrap
            '
            Dim SQL As String
            Dim dt As DataTable
            Dim ContentID As Integer
            Dim SelectError As Integer
            Dim SelectErrorDescription As String
            Dim iAllowAutoIncrement As Boolean
            Dim RSSchema As DataTable
            Dim TableFound As Boolean
            '
            'If csv_false Then Exit Sub
            '
            Const MissingTableErrorMySQL = -2147467259
            Const MissingTableErrorAccess = -2147217900
            Const MissingTableErrorMsSQL = -2147217865
            '
            If TableName = "" Then
                '
                ' tablename required
                '
                Call Err.Raise(ignoreInteger, "dll", "Tablename can not be blank.")
            ElseIf vbInstr(1, TableName, ".") <> 0 Then
                '
                ' Remote table -- remote system controls remote tables
                '
            Else
                '
                ' Local table -- create if not in schema
                '
                iAllowAutoIncrement = AllowAutoIncrement
                TableFound = (Not cpCore.metaData.getTableSchema(TableName, DataSourceName) Is Nothing)
                '
                If Not TableFound Then
                    If Not iAllowAutoIncrement Then
                        SQL = "Create Table " & TableName & "(ID " & db_GetSQLAlterColumnType(DataSourceName, FieldTypeIdInteger) & ");"
                        Call executeSql_getDataTable(SQL, DataSourceName)
                    Else
                        SQL = "Create Table " & TableName & "(ID " & db_GetSQLAlterColumnType(DataSourceName, FieldTypeIdAutoIdIncrement) & ");"
                        Call executeSql_getDataTable(SQL, DataSourceName)
                    End If
                End If
                '
                ' ----- Test the common fields required in all tables
                '
                Call db_CreateSQLTableField(DataSourceName, TableName, "ID", FieldTypeIdAutoIdIncrement)
                Call db_CreateSQLTableField(DataSourceName, TableName, "Name", FieldTypeIdText)
                Call db_CreateSQLTableField(DataSourceName, TableName, "DateAdded", FieldTypeIdDate)
                Call db_CreateSQLTableField(DataSourceName, TableName, "CreatedBy", FieldTypeIdInteger)
                Call db_CreateSQLTableField(DataSourceName, TableName, "ModifiedBy", FieldTypeIdInteger)
                Call db_CreateSQLTableField(DataSourceName, TableName, "ModifiedDate", FieldTypeIdDate)
                Call db_CreateSQLTableField(DataSourceName, TableName, "Active", FieldTypeIdBoolean)
                Call db_CreateSQLTableField(DataSourceName, TableName, "CreateKey", FieldTypeIdInteger)
                Call db_CreateSQLTableField(DataSourceName, TableName, "SortOrder", FieldTypeIdText)
                Call db_CreateSQLTableField(DataSourceName, TableName, "ContentControlID", FieldTypeIdInteger)
                Call db_CreateSQLTableField(DataSourceName, TableName, "EditSourceID", FieldTypeIdInteger)
                Call db_CreateSQLTableField(DataSourceName, TableName, "EditArchive", FieldTypeIdBoolean)
                Call db_CreateSQLTableField(DataSourceName, TableName, "EditBlank", FieldTypeIdBoolean)
                Call db_CreateSQLTableField(DataSourceName, TableName, "ContentCategoryID", FieldTypeIdInteger)
                Call db_CreateSQLTableField(DataSourceName, TableName, "ccGuid", FieldTypeIdText)
                '
                ' ----- setup core indexes
                '
                Call db_CreateSQLIndex(DataSourceName, TableName, TableName & "ID", "ID")
                Call db_CreateSQLIndex(DataSourceName, TableName, TableName & "Active", "ACTIVE")
                Call db_CreateSQLIndex(DataSourceName, TableName, TableName & "Name", "NAME")
                Call db_CreateSQLIndex(DataSourceName, TableName, TableName & "SortOrder", "SORTORDER")
                Call db_CreateSQLIndex(DataSourceName, TableName, TableName & "DateAdded", "DATEADDED")
                Call db_CreateSQLIndex(DataSourceName, TableName, TableName & "CreateKey", "CREATEKEY")
                Call db_CreateSQLIndex(DataSourceName, TableName, TableName & "EditSourceID", "EDITSOURCEID")
                Call db_CreateSQLIndex(DataSourceName, TableName, TableName & "ContentControlID", "CONTENTCONTROLID")
                Call db_CreateSQLIndex(DataSourceName, TableName, TableName & "ModifiedDate", "MODIFIEDDATE")
                Call db_CreateSQLIndex(DataSourceName, TableName, TableName & "ContentCategoryID", "CONTENTCATEGORYID")
                Call db_CreateSQLIndex(DataSourceName, TableName, TableName & "ccGuid", "CCGUID")
            End If
            cpCore.metaData.tableSchemaListClear()
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError1("csv_CreateSQLTable", "trap")
        End Sub
        '
        '========================================================================
        ' Check for a field in a table in the database
        '   if missing, create the field
        '========================================================================
        '
        Public Sub db_CreateSQLTableField(ByVal DataSourceName As String, ByVal TableName As String, ByVal FieldName As String, ByVal fieldType As Integer, Optional clearMetaCache As Boolean = False)
            On Error GoTo ErrorTrap
            '
            Dim SQL As String
            Dim dt As DataTable
            Dim NewField As Boolean
            Dim SelectError As Integer
            Dim DataSourceID As Integer
            Dim CSPointer As Integer
            Dim MethodName As String
            Dim UcaseFieldName As String
            Dim Pointer As Integer
            Dim RSSchema As DataTable
            Dim fieldFound As Boolean
            Dim cacheName As String
            '
            MethodName = "csv_CreateSQLTableField(" & DataSourceName & "," & TableName & "," & FieldName & "," & fieldType & ",)"
            '
            'If csv_false Then Exit Sub
            '
            If TableName = "" Then
                '
                ' Bad tablename
                '
                Call handleLegacyClassError1(MethodName, "csv_CreateSQLTableField called with blank tablename")
            ElseIf fieldType = 0 Then
                '
                ' Bad fieldtype
                '
                Call handleLegacyClassError1(MethodName, "csv_CreateSQLTableField called with invalid fieldtype [" & fieldType & "]")
            ElseIf (fieldType = FieldTypeIdRedirect) Or (fieldType = FieldTypeIdManyToMany) Then
                '
                ' contensive fields with no table field
                '
                fieldType = fieldType
            ElseIf vbInstr(1, TableName, ".") <> 0 Then
                '
                ' External table
                '
                TableName = TableName
            ElseIf FieldName = "" Then
                '
                ' Bad fieldname
                '
                Call handleLegacyClassError1(MethodName, "csv_CreateSQLTableField called with blank fieldname")
            Else
                UcaseFieldName = vbUCase(FieldName)
                If Not db_IsSQLTableField(DataSourceName, TableName, FieldName) Then
                    SQL = "ALTER TABLE " & TableName & " ADD " & FieldName & " "
                    If Not vbIsNumeric(fieldType) Then
                        '
                        ' ----- support old calls
                        '
                        SQL &= fieldType
                    Else
                        '
                        ' ----- translater type into SQL string
                        '
                        SQL &= db_GetSQLAlterColumnType(DataSourceName, fieldType)
                    End If
                    On Error GoTo ErrorTrap_SQL
                    Call executeSql_getDataTable(SQL, DataSourceName)
                    '
                    '
                    '
                    If clearMetaCache Then
                        Call cpCore.cache.invalidateAll()
                        Call cpCore.metaData.clear()
                    End If
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap_SQL:
            Call handleLegacyClassError1(MethodName & " Running SQL [" & SQL & "]", "trap")
            Exit Sub
ErrorTrap:
            Call handleLegacyClassError1(MethodName, "trap")
        End Sub
        '
        '========================================================================
        '   Delete a table field from a table
        '========================================================================
        '
        Public Sub db_DeleteTable(ByVal DataSourceName As String, ByVal TableName As String)
            Try
                Call executeSql_getDataTable("DROP TABLE " & TableName, DataSourceName)
                cpCore.cache.invalidateAll()
                cpCore.metaData.clear()
            Catch ex As Exception
                Call handleLegacyClassError1(System.Reflection.MethodBase.GetCurrentMethod.Name, "trap")
            End Try
        End Sub
        '
        '========================================================================
        '   Delete a table field from a table
        '========================================================================
        '
        Public Sub db_DeleteTableField(ByVal DataSourceName As String, ByVal TableName As String, ByVal FieldName As String)
            Throw New NotImplementedException("deletetablefield")

            '            On Error GoTo ErrorTrap
            '            '
            '            Dim MethodName As String
            '            Dim Pointer As Integer
            '            '
            '            MethodName = "csv_DeleteTableField"
            '            '
            '            ' See if it is in the table to begin with
            '            '
            '            If csv_IsSQLTableField(DataSourceName, TableName, FieldName) Then
            '                '
            '                '   Delete any indexes that use this column
            '                '
            '                Dim SchemaPointer As Integer
            '                Dim IndexPointer As Integer
            '                SchemaPointer = csv_getTableSchema(TableName, DataSourceName)
            '                With cdefCache.tableSchema(SchemaPointer)
            '                    If .IndexCount > 0 Then
            '                        For IndexPointer = 0 To .IndexCount - 1
            '                            If FieldName = .IndexFieldName(IndexPointer) Then
            '                                Call csv_DeleteTableIndex(DataSourceName, TableName, .IndexName(IndexPointer))
            '                            End If
            '                        Next
            '                    End If
            '                End With
            '                '
            '                '   Delete the field
            '                '
            '                Pointer = csv_GetDataSourcePointer(DataSourceName)
            '                Select Case DataSourceConnectionObjs(Pointer).Type
            '                    Case DataSourceTypeODBCAccess
            '                        '
            '                        '   MS Access field
            '                        '
            '                        Call executeSql("ALTER TABLE " & TableName & " DROP [" & FieldName & "];", DataSourceName)
            '                    Case Else
            '                        '
            '                        '   other
            '                        '
            '                        Call executeSql("ALTER TABLE " & TableName & " DROP COLUMN " & FieldName & ";", DataSourceName)
            '                End Select
            '                Pointer = csv_getTableSchema(TableName, DataSourceName)
            '                If Pointer <> -1 Then
            '                    cdefCache.tableSchema(Pointer).Dirty = True
            '                End If
            '            End If
            '            '
            '            Exit Sub
            '            '
            '            ' ----- Error Trap
            '            '
            'ErrorTrap:
            '            Call csv_HandleClassErrorAndResume(MethodName, "trap")
        End Sub
        '
        '========================================================================
        ' Create an index on a table
        '
        '   Fieldnames is  a comma delimited list of fields
        '========================================================================
        '
        Public Sub db_CreateSQLIndex(ByVal DataSourceName As String, ByVal TableName As String, ByVal IndexName As String, ByVal FieldNames As String, Optional clearMetaCache As Boolean = False)
            Try
                Dim ts As coreMetaDataClass.tableSchemaClass
                If TableName <> "" And IndexName <> "" And FieldNames <> "" Then
                    ts = cpCore.metaData.getTableSchema(TableName, DataSourceName)
                    If (Not ts Is Nothing) Then
                        If Not ts.indexes.Contains(IndexName.ToLower) Then
                            Call executeSql_getDataTable("CREATE INDEX " & IndexName & " ON " & TableName & "( " & FieldNames & " );", DataSourceName)
                            If clearMetaCache Then
                                cpCore.cache.invalidateAll()
                                cpCore.metaData.clear()
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                Call handleLegacyClassError1("csv_CreateSQLIndex", "trap")
            End Try
        End Sub
        '
        '=============================================================
        '
        '=============================================================
        '
        Public Function db_GetRecordName(ByVal ContentName As String, ByVal RecordID As Integer) As String
            On Error GoTo ErrorTrap
            '
            Dim CS As Integer
            '
            CS = db_OpenCSContentRecord(ContentName, RecordID, , , , "Name")
            If cs_Ok(CS) Then
                db_GetRecordName = db_GetCS(CS, "Name")
            End If
            Call cs_Close(CS)

            Exit Function
ErrorTrap:
            Call handleLegacyClassError1("csv_GetRecordName", "csv_GetRecordName")
        End Function
        '
        '=============================================================
        '
        '=============================================================
        '
        Public Function getRecordID(ByVal ContentName As String, ByVal RecordName As String) As Integer
            Dim returnValue As Integer = 0
            Try
                Dim CS As Integer
                '
                CS = csOpen(ContentName, "name=" & encodeSQLText(RecordName), "ID", , , , , "ID")
                If cs_Ok(CS) Then
                    returnValue = EncodeInteger(db_GetCS(CS, "ID"))
                    If returnValue = 0 Then
                        cpCore.handleExceptionAndRethrow(New ApplicationException("getRecordId([" & ContentName & "],[" & RecordName & "]), a record was found but id returned 0"))
                    End If
                End If
                Call cs_Close(CS)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnValue
        End Function
        '
        '=============================================================
        '
        '=============================================================
        '
        Public Function metaData_IsContentFieldSupported(ByVal ContentName As String, ByVal FieldName As String) As Boolean
            Dim returnOk As Boolean = False
            Try
                Dim cdef As coreMetaDataClass.CDefClass
                '
                cdef = cpCore.metaData.getCdef(ContentName)
                If Not cdef Is Nothing Then
                    returnOk = cdef.fields.ContainsKey(FieldName.ToLower)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnOk
        End Function
        '
        '========================================================================
        ' ----- Get FieldDescritor from FieldType
        '========================================================================
        '
        Public Function db_GetSQLAlterColumnType(ByVal DataSourceName As String, ByVal fieldType As Integer) As String
            Try
                Select Case fieldType
                    Case FieldTypeIdBoolean
                        db_GetSQLAlterColumnType = "Int NULL"
                    Case FieldTypeIdCurrency
                        db_GetSQLAlterColumnType = "Float NULL"
                    Case FieldTypeIdDate
                        db_GetSQLAlterColumnType = "DateTime NULL"
                    Case FieldTypeIdFloat
                        db_GetSQLAlterColumnType = "Float NULL"
                    Case FieldTypeIdCurrency
                        db_GetSQLAlterColumnType = "Float NULL"
                    Case FieldTypeIdInteger
                        db_GetSQLAlterColumnType = "Int NULL"
                    Case FieldTypeIdLookup, FieldTypeIdMemberSelect
                        db_GetSQLAlterColumnType = "Int NULL"
                    Case FieldTypeIdManyToMany, FieldTypeIdRedirect, FieldTypeIdFileImage, FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdText, FieldTypeIdFile, FieldTypeIdFileTextPrivate, FieldTypeIdFileJavascript, FieldTypeIdFileXML, FieldTypeIdFileCSS, FieldTypeIdFileHTMLPrivate
                        db_GetSQLAlterColumnType = "VarChar(255) NULL"
                    Case FieldTypeIdLongText, FieldTypeIdHTML
                        '
                        ' ----- Longtext, depends on datasource
                        '
                        db_GetSQLAlterColumnType = "Text Null"
                            'Select Case DataSourceConnectionObjs(Pointer).Type
                            '    Case DataSourceTypeODBCSQLServer
                            '        csv_GetSQLAlterColumnType = "Text Null"
                            '    Case DataSourceTypeODBCAccess
                            '        csv_GetSQLAlterColumnType = "Memo Null"
                            '    Case DataSourceTypeODBCMySQL
                            '        csv_GetSQLAlterColumnType = "Text Null"
                            '    Case Else
                            '        csv_GetSQLAlterColumnType = "VarChar(65535)"
                            'End Select
                    Case FieldTypeIdAutoIdIncrement
                        '
                        ' ----- autoincrement type, depends on datasource
                        '
                        db_GetSQLAlterColumnType = "INT IDENTITY PRIMARY KEY"
                        'Select Case DataSourceConnectionObjs(Pointer).Type
                        '    Case DataSourceTypeODBCSQLServer
                        '        csv_GetSQLAlterColumnType = "INT IDENTITY PRIMARY KEY"
                        '    Case DataSourceTypeODBCAccess
                        '        csv_GetSQLAlterColumnType = "COUNTER CONSTRAINT PrimaryKey PRIMARY KEY"
                        '    Case DataSourceTypeODBCMySQL
                        '        csv_GetSQLAlterColumnType = "INT AUTO_INCREMENT PRIMARY KEY"
                        '    Case Else
                        '        csv_GetSQLAlterColumnType = "INT AUTO_INCREMENT PRIMARY KEY"
                        'End Select
                    Case Else
                        '
                        ' Invalid field type
                        '
                        Throw New ApplicationException("Can not proceed because the field being created has an invalid FieldType [" & fieldType & "]")
                End Select
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Function
        '
        '========================================================================
        ' Delete an Index for a table
        '========================================================================
        '
        Public Sub db_deleteSqlIndex(ByVal DataSourceName As String, ByVal TableName As String, ByVal IndexName As String)
            Try
                Dim ts As coreMetaDataClass.tableSchemaClass
                Dim DataSourceType As Integer
                Dim sql As String
                '
                ts = cpCore.metaData.getTableSchema(TableName, DataSourceName)
                If (Not ts Is Nothing) Then
                    If ts.indexes.Contains(IndexName.ToLower) Then
                        DataSourceType = db_GetDataSourceType(DataSourceName)
                        Select Case DataSourceType
                            Case DataSourceTypeODBCAccess
                                sql = "DROP INDEX " & IndexName & " ON " & TableName & ";"
                            Case DataSourceTypeODBCMySQL
                                Throw New NotImplementedException("mysql")
                            Case Else
                                sql = "DROP INDEX " & TableName & "." & IndexName & ";"
                        End Select
                        Call executeSql_getDataTable(sql, DataSourceName)
                        cpCore.cache.invalidateAll()
                        cpCore.metaData.clear()
                    End If
                End If

            Catch ex As Exception
                Call handleLegacyClassError1("csv_DeleteTableIndex", "")
            End Try
            '            On Error GoTo ErrorTrap
            '            '
            '            Dim DataSourceType As Integer
            '            Dim SQL As String
            '            Dim IndexPointer As Integer
            '            Dim SchemaPointer As Integer
            '            Dim UcaseIndexName As String
            '            '
            '            UcaseIndexName = vbUCase(IndexName)
            '            SchemaPointer = getTableSchema(TableName, DataSourceName)
            '            If SchemaPointer <> -1 Then
            '                With metaCache.tableSchema(SchemaPointer)
            '                    If .IndexCount > 0 Then
            '                        For IndexPointer = 0 To .IndexCount - 1
            '                            If .IndexName(IndexPointer) = UcaseIndexName Then
            '                                metaCache.tableSchema(SchemaPointer).Dirty = True
            '                                DataSourceType = csv_GetDataSourceType(DataSourceName)
            '                                If DataSourceType = DataSourceTypeODBCAccess Then
            '                                    SQL = "DROP INDEX " & IndexName & " ON " & TableName & ";"
            '                                Else
            '                                    SQL = "DROP INDEX " & TableName & "." & IndexName & ";"
            '                                End If
            '                                Call executeSql(SQL, DataSourceName)
            '                                Exit For
            '                            End If
            '                        Next
            '                    End If
            '                End With
            '            End If
            '            '
            '            Exit Sub
            '            '
            '            ' ----- Error Trap
            '            '
            'ErrorTrap:
        End Sub
        '
        ' ----- Get a DataSource ID from its Name
        '       If it is not found, -1 is returned (for system datasource)
        '
        Public Function db_GetDataSourceID(ByVal DataSourceName As String) As Integer
            On Error GoTo ErrorTrap
            '
            Dim DataSourcePointer As Integer
            Dim MethodName As String
            '
            MethodName = "csv_GetDataSourceID"
            '
            DataSourcePointer = db_GetDataSourcePointer(DataSourceName)
            If dataSources.Length > 0 Then
                db_GetDataSourceID = dataSources(DataSourcePointer).Id
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError1(MethodName, "")
        End Function
        '
        '   Patch -- returns true if the cdef field exists
        '
        Public Function db_isCdefField(ByVal ContentID As Integer, ByVal FieldName As String) As Boolean
            Dim RS As DataTable
            '
            RS = executeSql_getDataTable("select top 1 id from ccFields where name=" & encodeSQLText(FieldName) & " and contentid=" & ContentID)
            db_isCdefField = isDataTableOk(RS)
            If (isDataTableOk(RS)) Then
                RS.Dispose()
            End If
        End Function '
        '        '========================================================================
        '        '   Create a content field definition if it is missing
        '        '
        '        '       Add or modify the field in ContentName
        '        '       Add the field to all child definitions of ContentName
        '        '========================================================================
        '        '
        '        Public Sub metaData_VerifyCDefField_ReturnID(ByVal Active As Boolean, ByVal ContentName As String, ByVal FieldName As String, ByVal fieldType As Integer, Optional ByVal FieldSortOrder As String = "", Optional ByVal FieldAuthorable As Boolean = True, Optional ByVal FieldCaption As String = "", Optional ByVal LookupContentName As String = "", Optional ByVal DefaultValue As String = "", Optional ByVal NotEditable As Boolean = False, Optional ByVal AdminIndexColumn As Integer = 0, Optional ByVal AdminIndexWidth As Integer = 0, Optional ByVal AdminIndexSort As Integer = 0, Optional ByVal RedirectContentName As String = "", Optional ByVal RedirectIDField As String = "", Optional ByVal RedirectPath As String = "", Optional ByVal HTMLContent As String = "", Optional ByVal UniqueName As Boolean = False, Optional ByVal Password As String = "", Optional ByVal AdminOnly As Boolean = False, Optional ByVal DeveloperOnly As Boolean = False, Optional ByVal readOnlyField As Boolean = False, Optional ByVal FieldRequired As Boolean = False, Optional ByVal IsBaseField As Boolean = False)
        '            On Error GoTo ErrorTrap
        '            '
        '            Dim Args As String
        '            '
        '            '
        '            Args = "" _
        '                & ",Active=" & Active _
        '                & ",Type=" & fieldType _
        '                & ",SortOrder=" & FieldSortOrder.Replace(",", "") _
        '                & ",Authorable=" & FieldAuthorable _
        '                & ",Caption=" & FieldCaption.Replace(",", "") _
        '                & ",LookupContentName=" & LookupContentName.Replace(",", "") _
        '                & ",DefaultValue=" & DefaultValue.Replace(",", "") _
        '                & ""
        '            Args = Args _
        '                & ",NotEditable=" & NotEditable _
        '                & ",AdminIndexColumn=" & AdminIndexColumn _
        '                & ",AdminIndexWidth=" & AdminIndexWidth _
        '                & ",AdminIndexSort=" & AdminIndexSort _
        '                & ",RedirectContentName=" & RedirectContentName.Replace(",", "") _
        '                & ",RedirectIDField=" & RedirectIDField.Replace(",", "") _
        '                & ",RedirectPath=" & RedirectPath.Replace(",", "") _
        '                & ",HTMLContent=" & HTMLContent _
        '                & ",UniqueName=" & UniqueName _
        '                & ",Password=" & Password.Replace(",", "") _
        '                & ",AdminOnly=" & AdminOnly _
        '                & ",DeveloperOnly=" & DeveloperOnly _
        '                & ",ReadOnly=" & readOnlyField _
        '                & ",Required=" & FieldRequired _
        '                & ",IsBaseField=" & IsBaseField _
        '                & ""
        '            Call metaData_VerifyCDefField_ReturnID(ContentName, FieldName, Args, ",")

        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "metaData_VerifyCDefField_ReturnID", True)
        '        End Sub
        '
        ' ----- Get a DataSource type (SQL Server, etc) from its Name
        '
        Public Function db_GetDataSourceType(ByVal DataSourceName As String) As Integer
            db_GetDataSourceType = DataSourceTypeODBCSQLServer
            '            On Error GoTo ErrorTrap
            '            '
            '            Dim DataSourcePointer As Integer
            '            '
            '            DataSourcePointer = csv_GetDataSourcePointer(DataSourceName)
            '            If dataSources.length > 0 Then
            '                If Not DataSourceConnectionObjs(DataSourcePointer).IsOpen Then
            '                    Call csv_OpenDataSource(DataSourceName, 30)
            '                End If
            '                csv_GetDataSourceType = DataSourceConnectionObjs(DataSourcePointer).Type
            '            End If
            '            '
            '            Exit Function
            '            '
            '            ' ----- Error Trap
            '            '
            'ErrorTrap:
            '            Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, "csv_GetDataSourceType", True)
        End Function
        '
        '========================================================================
        ' ----- Get FieldType from ADO Field Type
        '========================================================================
        '
        Public Function db_GetFieldTypeIdByADOType(ByVal ADOFieldType As Integer) As Integer
            On Error GoTo ErrorTrap
            '
            Dim MethodName As String
            '
            MethodName = "csv_GetFieldTypeByADOType"
            '
            Select Case ADOFieldType

                Case 2
                    db_GetFieldTypeIdByADOType = FieldTypeIdFloat
                Case 3
                    db_GetFieldTypeIdByADOType = FieldTypeIdInteger
                Case 4
                    db_GetFieldTypeIdByADOType = FieldTypeIdFloat
                Case 5
                    db_GetFieldTypeIdByADOType = FieldTypeIdFloat
                Case 6
                    db_GetFieldTypeIdByADOType = FieldTypeIdInteger
                Case 11
                    db_GetFieldTypeIdByADOType = FieldTypeIdBoolean
                Case 135
                    db_GetFieldTypeIdByADOType = FieldTypeIdDate
                Case 200
                    db_GetFieldTypeIdByADOType = FieldTypeIdText
                Case 201
                    db_GetFieldTypeIdByADOType = FieldTypeIdLongText
                Case 202
                    db_GetFieldTypeIdByADOType = FieldTypeIdText
                Case Else
                    db_GetFieldTypeIdByADOType = FieldTypeIdText
                    'Call csv_HandleClassTrapError(ignoreString, "dll", "Unknown ADO field type [" & ADOFieldType & "], [Text] used", MethodName, False, True)
            End Select
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function
        '
        '========================================================================
        ' ----- Get FieldType from FieldTypeName
        '========================================================================
        '
        Public Function getFieldTypeIdFromFieldTypeName(ByVal FieldTypeName As String) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "GetFieldTypeByDescriptor" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            Dim Ptr As Integer
            Dim Copy As String
            '
            MethodName = "csv_GetFieldTypeByDescriptor"
            '
            Select Case vbLCase(FieldTypeName)
                Case FieldTypeNameLcaseBoolean
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdBoolean
                Case FieldTypeNameLcaseCurrency
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdCurrency
                Case FieldTypeNameLcaseDate
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdDate
                Case FieldTypeNameLcaseFile
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdFile
                Case FieldTypeNameLcaseFloat
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdFloat
                Case FieldTypeNameLcaseImage
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdFileImage
                Case FieldTypeNameLcaseLink
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdLink
                Case FieldTypeNameLcaseResourceLink, "resource link", "resourcelink"
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdResourceLink
                Case FieldTypeNameLcaseInteger
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdInteger
                Case FieldTypeNameLcaseLongText, "longtext", "Long text"
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdLongText
                Case FieldTypeNameLcaseLookup, "lookuplist", "lookup list"
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdLookup
                Case FieldTypeNameLcaseMemberSelect
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdMemberSelect
                Case FieldTypeNameLcaseRedirect
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdRedirect
                Case FieldTypeNameLcaseManyToMany
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdManyToMany
                Case FieldTypeNameLcaseTextFile, "text file", "textfile"
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdFileTextPrivate
                Case FieldTypeNameLcaseCSSFile, "cssfile", "css file"
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdFileCSS
                Case FieldTypeNameLcaseXMLFile, "xmlfile", "xml file"
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdFileXML
                Case FieldTypeNameLcaseJavascriptFile, "javascript file", "javascriptfile", "js file", "jsfile"
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdFileJavascript
                Case FieldTypeNameLcaseText
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdText
                Case "autoincrement"
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdAutoIdIncrement
                Case "memberselect"
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdMemberSelect
                Case FieldTypeNameLcaseHTML
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdHTML
                Case FieldTypeNameLcaseHTMLFile, "html file"
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdFileHTMLPrivate
                Case Else
                    '
                    ' Bad field type is a text field
                    '
                    getFieldTypeIdFromFieldTypeName = FieldTypeIdText
                    'For Ptr = 1 To FieldTypeMax
                    '    Copy = Copy & ", " & csv_GetFieldTypeNameByType(Ptr)
                    'Next
                    'Call Err.Raise(ignoreInteger, "dll", "Unknown FieldTypeName [" & FieldTypeName & "], valid values are [" & Mid(Copy, 2) & "]")
            End Select
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
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
        Public Function csOpen(ByVal ContentName As String, Optional ByVal Criteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal MemberID As Integer = 0, Optional ByVal WorkflowRenderingMode As Boolean = False, Optional ByVal WorkflowEditingMode As Boolean = False, Optional ByVal SelectFieldList As String = "", Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
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
                Dim CDef As coreMetaDataClass.CDefClass
                Dim TestUcaseFieldList As String
                Dim SQL As String
                '
                csOpen = -1
                '
                If (ContentName.ToLower() = "system email") Then
                    ContentName = ContentName
                End If
                If ContentName = "" Then
                    Throw (New ApplicationException("db_csOpen called With a blank ContentName"))
                Else
                    CDef = cpCore.metaData.getCdef(ContentName)
                    If (CDef Is Nothing) Then
                        Throw (New ApplicationException("No content definition found For [" & ContentName & "]"))
                    ElseIf (CDef.Id <= 0) Then
                        Throw (New ApplicationException("No content definition found For [" & ContentName & "]"))
                    Else
                        '
                        'hint = hint & ", 100"
                        iActiveOnly = ((ActiveOnly))
                        iSortFieldList = encodeEmptyText(SortFieldList, CDef.DefaultSortMethod)
                        iWorkflowRenderingMode = cpCore.siteProperties.allowWorkflowAuthoring And CDef.AllowWorkflowAuthoring And (WorkflowRenderingMode)
                        AllowWorkflowSave = iWorkflowRenderingMode And (WorkflowEditingMode)
                        iMemberID = MemberID
                        iCriteria = encodeEmptyText(Criteria, "")
                        iSelectFieldList = encodeEmptyText(SelectFieldList, CDef.SelectCommaList)
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
                                SortField = vbReplace(SortField, "asc", "", 1, 99, vbTextCompare)
                                SortField = vbReplace(SortField, "desc", "", 1, 99, vbTextCompare)
                                SortField = Trim(SortField)
                                If Not CDef.selectList.Contains(SortField) Then
                                    'throw (New ApplicationException(""))
                                    Throw (New ApplicationException("The field [" & SortField & "] was used in a sort method For content [" & ContentName & "], but the content does not include this field."))
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
                            ContentCriteria = vbReplace(ContentCriteria, CDef.ContentTableName & ".", "")
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
                        iSelectFieldList = vbReplace(iSelectFieldList, vbTab, " ")
                        Do While vbInstr(1, iSelectFieldList, " ,") <> 0
                            iSelectFieldList = vbReplace(iSelectFieldList, " ,", ",")
                        Loop
                        Do While vbInstr(1, iSelectFieldList, ", ") <> 0
                            iSelectFieldList = vbReplace(iSelectFieldList, ", ", ",")
                        Loop
                        If (iSelectFieldList <> "") And (InStr(1, iSelectFieldList, "*", vbTextCompare) = 0) Then
                            TestUcaseFieldList = vbUCase("," & iSelectFieldList & ",")
                            If vbInstr(1, TestUcaseFieldList, ",CONTENTCONTROLID,", vbTextCompare) = 0 Then
                                iSelectFieldList = iSelectFieldList & ",ContentControlID"
                            End If
                            If vbInstr(1, TestUcaseFieldList, ",NAME,", vbTextCompare) = 0 Then
                                iSelectFieldList = iSelectFieldList & ",Name"
                            End If
                            If vbInstr(1, TestUcaseFieldList, ",ID,", vbTextCompare) = 0 Then
                                iSelectFieldList = iSelectFieldList & ",ID"
                            End If
                            If vbInstr(1, TestUcaseFieldList, ",ACTIVE,", vbTextCompare) = 0 Then
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
                            'UcaseTablename = vbUCase(TableName)
                            DataSourceName = CDef.AuthoringDataSourceName
                            '
                            ' Substitute ID for EditSourceID in criteria
                            '
                            ContentCriteria = vbReplace(ContentCriteria, " ID=", " " & TableName & ".EditSourceID=")
                            ContentCriteria = vbReplace(ContentCriteria, " ID>", " " & TableName & ".EditSourceID>")
                            ContentCriteria = vbReplace(ContentCriteria, " ID<", " " & TableName & ".EditSourceID<")
                            ContentCriteria = vbReplace(ContentCriteria, " ID ", " " & TableName & ".EditSourceID ")
                            ContentCriteria = vbReplace(ContentCriteria, "(ID=", "(" & TableName & ".EditSourceID=")
                            ContentCriteria = vbReplace(ContentCriteria, "(ID>", "(" & TableName & ".EditSourceID>")
                            ContentCriteria = vbReplace(ContentCriteria, "(ID<", "(" & TableName & ".EditSourceID<")
                            ContentCriteria = vbReplace(ContentCriteria, "(ID ", "(" & TableName & ".EditSourceID ")
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
                                iSortFieldList = vbReplace(iSortFieldList, ",ID,", "," & TableName & ".EditSourceID,")
                                iSortFieldList = vbReplace(iSortFieldList, ",ID ", "," & TableName & ".EditSourceID ")
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
                                ' this was done in the csv_GetCSField, but then a csv_GetCSrows call would return incorrect
                                ' values because there is no translation
                                '
                                If vbInstr(1, iSelectFieldList, ",ID,", vbTextCompare) = 0 Then
                                    '
                                    ' Add ID select if not there
                                    '
                                    iSelectFieldList = iSelectFieldList & TableName & ".EditSourceID As ID,"
                                Else
                                    '
                                    ' remove ID, and add ID alias to return EditSourceID (the live records id)
                                    '
                                    iSelectFieldList = vbReplace(iSelectFieldList, ",ID,", "," & TableName & ".EditSourceID As ID,")
                                End If
                                '
                                ' Add the edit fields to the select
                                '
                                Copy2 = TableName & ".EDITSOURCEID,"
                                If (InStr(1, iSelectFieldList, ",EDITSOURCEID,", vbTextCompare) <> 0) Then
                                    iSelectFieldList = vbReplace(iSelectFieldList, ",EDITSOURCEID,", "," & Copy2, 1, 99, vbTextCompare)
                                Else
                                    iSelectFieldList = iSelectFieldList & Copy2
                                End If
                                Copy2 = TableName & ".EDITARCHIVE,"
                                If (InStr(1, iSelectFieldList, ",EDITARCHIVE,", vbTextCompare) <> 0) Then
                                    iSelectFieldList = vbReplace(iSelectFieldList, ",EDITARCHIVE,", "," & Copy2, 1, 99, vbTextCompare)
                                Else
                                    iSelectFieldList = iSelectFieldList & Copy2
                                End If
                                Copy2 = TableName & ".EDITBLANK,"
                                If (InStr(1, iSelectFieldList, ",EDITBLANK,", vbTextCompare) <> 0) Then
                                    iSelectFieldList = vbReplace(iSelectFieldList, ",EDITBLANK,", "," & Copy2, 1, 99, vbTextCompare)
                                Else
                                    iSelectFieldList = iSelectFieldList & Copy2
                                End If
                                '
                                ' WorkflowR2 - this also came from the csv_GetCSField - if EditID is requested, return the edit records id
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
                        csOpen = db_initCS(iMemberID)
                        With db_ContentSet(csOpen)
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
                                .PageSize = csv_DefaultPageSize
                            End If

                            'hint = hint & ",500"
                            'cpCore.AppendLog("db_csOpen, ContentName=[" & ContentName & "], PageSize=[" & .PageSize & "], PageNumber=[" & .PageNumber & "]")
                            'hint = hint & ",510"


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
                            If csv_OpenCSWithoutRecords Then
                                '
                                ' Save the source, but do not open the recordset
                                '
                                db_ContentSet(csOpen).Source = SQL
                            Else
                                '
                                ' Run the query
                                '
                                db_ContentSet(csOpen).dt = executeSql_getDataTable(SQL, DataSourceName, .PageSize * (.PageNumber - 1), .PageSize)
                            End If
                        End With
                        'hint = hint & ",600"
                        Call db_initCSData(csOpen)
                        Call db_LoadContentSetCurrentRow(csOpen)
                        '
                        'hint = hint & ",700"
                        If iWorkflowRenderingMode Then
                            '
                            ' Authoring mode
                            '
                            Call db_VerifyWorkflowAuthoringRecord(csOpen)
                            If AllowWorkflowSave Then
                                '
                                ' Workflow Editing Mode - lock the first record
                                '
                                If Not db_IsCSEOF(csOpen) Then
                                    If db_ContentSet(csOpen).WorkflowEditingRequested Then
                                        RecordID = cs_getInteger(csOpen, "ID")
                                        If Not cpCore.workflow.isRecordLocked(ContentName, RecordID, MemberID) Then
                                            db_ContentSet(csOpen).WorkflowEditingMode = True
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
                '
                'hint = hint & ",800"
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                'If False Then
                If Not cpCore.siteProperties.trapErrors Then
                    Throw New ApplicationException("rethrow", ex)
                End If
                'End If
            End Try
        End Function
        '
        '========================================================================
        ' csv_DeleteCSRecord
        '========================================================================
        '
        Public Sub db_DeleteCSRecord(ByVal CSPointer As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-053" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim LiveRecordID As Integer
            Dim EditRecordID As Integer
            Dim MethodName As String
            Dim ContentID As Integer
            Dim ContentName As String
            Dim ContentDataSourceName As String
            Dim ContentTableName As String
            Dim AuthoringDataSourceName As String
            Dim AuthoringTableName As String
            Dim SQL As String
            Dim SQLName(5) As String
            Dim SQLValue(5) As String
            Dim Pointer As Integer
            Dim Ptr As Integer
            Dim Filename As String
            Dim fieldArray As coreMetaDataClass.CDefFieldClass()
            Dim sqlList As sqlFieldListClass
            '
            MethodName = "csv_DeleteCSRecord"
            '
            If Not cs_Ok(CSPointer) Then
                '
                Call handleLegacyClassError3(MethodName, ("csv_ContentSet Is empty Or at End-Of-file"))
            ElseIf Not db_ContentSet(CSPointer).Updateable Then
                '
                Call handleLegacyClassError3(MethodName, ("csv_ContentSet Is Not Updateable"))
            Else
                With db_ContentSet(CSPointer).CDef
                    ContentID = .Id
                    ContentName = .Name
                    ContentTableName = .ContentTableName
                    ContentDataSourceName = .ContentDataSourceName
                    AuthoringTableName = .AuthoringTableName
                    AuthoringDataSourceName = .AuthoringDataSourceName
                End With
                If (ContentName = "") Then
                    '
                    Call handleLegacyClassError3(MethodName, ("csv_ContentSet Is Not based On a Content Definition"))
                Else
                    LiveRecordID = cs_getInteger(CSPointer, "ID")
                    If Not db_ContentSet(CSPointer).WorkflowAuthoringMode Then
                        '
                        ' delete any files
                        '
                        Dim fieldName As String
                        Dim field As coreMetaDataClass.CDefFieldClass
                        With db_ContentSet(CSPointer).CDef
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
                                                Call cpCore.cdnFiles.deleteFile(cpCore.cdnFiles.joinPath(cpCore.appConfig.cdnFilesNetprefix, Filename))
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
                        Call db_DeleteTableRecord(ContentDataSourceName, ContentTableName, LiveRecordID)
                        If cpCore.workflow.csv_AllowAutocsv_ClearContentTimeStamp Then
                            Call cpCore.cache.invalidateTag(ContentName)
                        End If
                        Call db_DeleteContentRules(ContentID, LiveRecordID)
                        '                Select Case vbUCase(ContentTableName)
                        '                    Case "CCCONTENTWATCH", "CCCONTENTWATCHLISTS", "CCCONTENTWATCHLISTRULES"
                        '                    Case Else
                        '                        Call csv_DeleteContentTracking(ContentName, LiveRecordID, True)
                        '                    End Select
                    Else
                        '
                        ' workflow mode, mark the editrecord "Blanked"
                        '
                        EditRecordID = cs_getInteger(CSPointer, "EditID")
                        sqlList = New sqlFieldListClass
                        Call sqlList.add("EDITBLANK", SQLTrue) ' Pointer)
                        Call sqlList.add("MODIFIEDBY", encodeSQLNumber(db_ContentSet(CSPointer).OwnerMemberID)) ' Pointer)
                        Call sqlList.add("MODIFIEDDATE", encodeSQLDate(Now)) ' Pointer)
                        Call db_UpdateTableRecord(AuthoringDataSourceName, AuthoringTableName, "ID=" & EditRecordID, sqlList)
                        Call cpCore.workflow.setAuthoringControl(ContentName, LiveRecordID, AuthoringControlsModified, db_ContentSet(CSPointer).OwnerMemberID)
                    End If
                End If
            End If
            '
            Exit Sub
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Sub
        '
        '========================================================================
        ' Opens a Select SQL into a csv_ContentSet
        '   Returns and long that points into the csv_ContentSet array
        '   If there was a problem, it returns -1
        '========================================================================
        '
        Public Function db_openCsSql_rev(ByVal DataSourceName As String, ByVal SQL As String, Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
            Return cs_openSql(SQL, DataSourceName, PageSize, PageNumber)
        End Function
        '
        Public Function cs_openSql(ByVal SQL As String, Optional ByVal DataSourceName As String = "", Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
            Dim returnCs As Integer = -1
            Try
                'Dim CSPointer As Integer
                'Dim RS As DataTable
                '
                ' ----- Open the csv_ContentSet
                '       (Save all buffered csv_ContentSet rows incase of overlap)
                '       No - Csets may be from other PageProcesses in unknown states
                '
                returnCs = db_initCS(cpCore.user.id)
                With db_ContentSet(returnCs)
                    .Updateable = False
                    .ContentName = ""
                    .PageNumber = PageNumber
                    .PageSize = (PageSize)
                    .DataSource = DataSourceName
                    .SelectTableFieldList = ""
                End With
                '
                If useCSReadCacheMultiRow Then
                    db_ContentSet(returnCs).dt = executeSql_getDataTable(SQL, DataSourceName, PageSize * (PageNumber - 1), PageSize)
                    Call db_initCSData(returnCs)
                    Call db_LoadContentSetCurrentRow(returnCs)
                Else
                    db_ContentSet(returnCs).dt = executeSql_getDataTable(SQL, DataSourceName, PageSize * (PageNumber - 1), PageSize)
                    Call db_initCSData(returnCs)
                    Call db_LoadContentSetCurrentRow(returnCs)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw ex
            End Try
            Return returnCs
        End Function
        '
        '========================================================================
        '   Open a csv_ContentSet (without any data)
        '========================================================================
        '
        Private Function db_initCS(ByVal MemberID As Integer) As Integer
            Dim returnCs As Integer = -1
            Try
                Dim TestTrap As Boolean
                Dim ContentSetPointer As Integer
                Dim MethodName As String
                '
                MethodName = "csv_OpenCS"
                '
                If csv_ContentSetCount > 0 Then
                    For ContentSetPointer = 1 To csv_ContentSetCount
                        If Not db_ContentSet(ContentSetPointer).IsOpen Then
                            '
                            ' Open CS found
                            '
                            returnCs = ContentSetPointer
                            Exit For
                        End If
                    Next
                End If
                '
                If returnCs = -1 Then
                    If csv_ContentSetCount >= csv_ContentSetSize Then
                        csv_ContentSetSize = csv_ContentSetSize + csv_ContentSetChunk
                        ReDim Preserve db_ContentSet(csv_ContentSetSize + 1)
                    End If
                    csv_ContentSetCount = csv_ContentSetCount + 1
                    returnCs = csv_ContentSetCount
                End If
                '
                With db_ContentSet(returnCs)
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
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnCs
        End Function
        '
        '========================================================================
        ' Close a csv_ContentSet
        '   Closes a currently open csv_ContentSet
        '   sets CSPointer to -1
        '========================================================================
        '
        Public Sub cs_Close(ByRef CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False)
            Try
                If (CSPointer > 0) And (CSPointer <= csv_ContentSetCount) Then
                    With db_ContentSet(CSPointer)
                        If .IsOpen Then
                            Call db_SaveCSRecord(CSPointer, AsyncSave)
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
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '========================================================================
        ' Move the csv_ContentSet to the next row
        '========================================================================
        '
        Public Sub db_csGoNext(ByVal CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False)
            On Error GoTo ErrorTrap 'Const Tn = "NextCSRecord" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            Dim ContentName As String
            Dim RecordID As Integer
            Dim ErrNumber As Integer
            Dim ErrSource As String
            Dim ErrDescription As String
            '
            MethodName = "csv_NextCSRecord"
            '
            If Not cs_Ok(CSPointer) Then
                '
                Call handleLegacyClassError3(MethodName, ("CSPointer Not csv_IsCSOK."))
            Else
                Call db_SaveCSRecord(CSPointer, AsyncSave)
                ' ##### moved into save so First does it also
                'csv_ContentSet(CSPointer).writeCacheCount = 0
                db_ContentSet(CSPointer).WorkflowEditingMode = False
                '
                ' Move to next row
                '
                db_ContentSet(CSPointer).readCacheRowPtr = db_ContentSet(CSPointer).readCacheRowPtr + 1
                If Not db_IsCSEOF(CSPointer) Then
                    '
                    ' Not EOF
                    '
                    Call db_LoadContentSetCurrentRow(CSPointer)
                    '
                    ' Set Workflow Edit Mode from Request and EditLock state
                    '
                    If (Not db_IsCSEOF(CSPointer)) And db_ContentSet(CSPointer).WorkflowEditingRequested Then
                        ContentName = db_ContentSet(CSPointer).ContentName
                        RecordID = cs_getInteger(CSPointer, "ID")
                        If Not cpCore.workflow.isRecordLocked(ContentName, RecordID, db_ContentSet(CSPointer).OwnerMemberID) Then
                            db_ContentSet(CSPointer).WorkflowEditingMode = True
                            Call cpCore.workflow.setEditLock(ContentName, RecordID, db_ContentSet(CSPointer).OwnerMemberID)
                        End If
                    End If
                End If
            End If
            '
            Exit Sub
            '
ErrorTrap:
            '
            ' set EOF, to fix problem where csv_NextCSRecord throws an error, but csv_IsCSOK returns true which causes an endless loop
            '
            ErrNumber = Err.Number
            ErrSource = Err.Source
            ErrDescription = Err.Description
            If (CSPointer >= 0) And (CSPointer < csv_ContentSetCount) Then
                ' more multirow readCache
                If useCSReadCacheMultiRow Then
                    db_ContentSet(CSPointer).readCacheRowPtr = -1
                Else
                    db_ContentSet(CSPointer).ResultEOF = True
                End If
            End If
            Call handleLegacyClassError4(ErrNumber, ErrSource, ErrDescription, MethodName, True)
        End Sub
        '
        '========================================================================
        ' Move the csv_ContentSet to the first row
        '========================================================================
        '
        Public Sub db_firstCSRecord(ByVal CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-058" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            '
            MethodName = "csv_FirstCSRecord"
            '
            ' ----- manually check the CSPointer because it may be at EOF so csv_IsCSOK will be false
            '
            If CSPointer < 0 Then
                '
                Call handleLegacyClassError3(MethodName, ("csv_ContentSet Pointer Is invalid"))
            Else
                Call db_SaveCSRecord(CSPointer, AsyncSave)
                If useCSReadCacheMultiRow Then
                    db_ContentSet(CSPointer).readCacheRowPtr = 0
                Else
                    'If Not csv_IsCSBOF(CSPointer) Then
                    '    '
                    '    ' ----- only move if not BOF
                    '    '
                    '    csv_ContentSet(CSPointer).readCacheRowPtr = 0
                    '    csv_ContentSet(CSPointer).rs.MoveFirst()
                    '    Call csv_LoadContentSetCurrentRow(CSPointer)
                    'End If
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Sub
        '
        '========================================================================
        ' get the value of a field within a csv_ContentSet
        '   if CS in authoring mode, it gets the edit record value, except ID field
        '   otherwise, it gets the live record value
        '========================================================================
        '
        Public Function db_GetCSField(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            On Error GoTo ErrorTrap 'Const Tn = "GetCSField" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim fieldFound As Boolean
            Dim writeCachePointer As Integer
            Dim ColumnPointer As Integer
            Dim FieldNameLocalUcase As String
            Dim FieldNameLocal As String
            Dim FieldPtr As Integer
            'Dim arrayOfFields As appServices_metaDataClass.CDefFieldClass()
            '
            FieldNameLocal = Trim(FieldName)
            FieldNameLocalUcase = vbUCase(FieldNameLocal)
            If Not cs_Ok(CSPointer) Then
                '
                cpCore.handleExceptionAndRethrow(New Exception("csv_ContentSet Is empty Or EOF"))
            Else
                With db_ContentSet(CSPointer)
                    '
                    ' WorkflowR2
                    ' this code ws replaced in csv_InitContentSetResult by replacing the column names. That was to fix the
                    ' problem of csv_GetCSRows and csv_GetCSRowNames. In those cases, the FieldNameLocal swap could not be done.Even
                    ' with this EditID can not be swapped in csv_GetCSRow
                    '
                    If .WorkflowAuthoringMode Then
                        '    If (FieldNameLocalUcase = "EDITSOURCEID") Then
                        '        FieldNameLocalUcase = "ID"
                        '    End If
                    Else
                        If (FieldNameLocalUcase = "EDITID") Then
                            FieldNameLocalUcase = "ID"
                        End If
                    End If
                    If .writeCache.Count > 0 Then
                        '
                        ' ----- something has been set in buffer, check it first
                        '
                        If .writeCache.ContainsKey(FieldName.ToLower) Then
                            db_GetCSField = .writeCache.Item(FieldName.ToLower)
                            fieldFound = True
                        End If
                        'For writeCachePointer = 0 To .writeCacheCount - 1
                        '    With .writeCache(writeCachePointer)
                        '        If FieldNameLocalUcase = vbUCase(.Name) Then
                        '            '
                        '            ' ----- field found, use the buffer value
                        '            '
                        '            db_GetCSField = .ValueVariant
                        '            fieldFound = True
                        '            Exit For
                        '        End If
                        '    End With
                        'Next
                    End If
                    If Not fieldFound Then
                        '
                        ' ----- attempt read from readCache
                        '
                        If useCSReadCacheMultiRow Then
                            If Not .dt.Columns.Contains(FieldName.ToLower) Then
                                Call handleLegacyClassError4(ignoreInteger, "dll", "Field [" & FieldNameLocal & "] was Not found In csv_ContentSet from source [" & .Source & "]", "unknownMethodNameLegacyCall", False, False)
                            Else
                                db_GetCSField = EncodeText(.dt.Rows(.readCacheRowPtr).Item(FieldName.ToLower))
                            End If
                        Else
                            '
                            ' ----- read the value from the Recordset Result
                            '
                            On Error GoTo ErrorTrapField
                            If .ResultColumnCount > 0 Then
                                For ColumnPointer = 0 To .ResultColumnCount - 1
                                    If .fieldNames(ColumnPointer) = FieldNameLocalUcase Then
                                        db_GetCSField = .readCache(ColumnPointer, 0)
                                        If .Updateable And (.ContentName <> "") And (FieldName <> "") Then
                                            If .CDef.fields(FieldName.ToLower()).Scramble Then
                                                db_GetCSField = cpCore.metaData.TextDeScramble(EncodeText(db_GetCSField))
                                            End If
                                        End If
                                        Exit For
                                    End If
                                Next
                                If ColumnPointer = .ResultColumnCount Then
                                    Call handleLegacyClassError4(ignoreInteger, "dll", "Field [" & FieldNameLocal & "] was Not found In csv_ContentSet from source [" & .Source & "]", "unknownMethodNameLegacyCall", False, False)
                                End If
                            End If
                            On Error GoTo ErrorTrap
                        End If
                    End If
                    .LastUsed = DateTime.Now
                End With
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("Unexpected exception"))
ErrorTrapField:
            Dim ErrDescription As String
            ErrDescription = GetErrString(Err)
            If db_ContentSet(CSPointer).Updateable Then
                Call handleLegacyClassError4(ignoreInteger, "dll", "Error " & ErrDescription & " reading Field [" & FieldNameLocal & "] from Content Definition [" & db_ContentSet(CSPointer).ContentName & "],  recordset sql [" & db_ContentSet(CSPointer).Source & "]", "csv_GetCSField", False)
            Else
                Call handleLegacyClassError4(ignoreInteger, "dll", "Error " & ErrDescription & " reading Field [" & FieldNameLocal & "] from Source [" & db_ContentSet(CSPointer).Source & "]", "csv_GetCSField", False)
            End If
        End Function
        '
        '========================================================================
        ' get the first fieldname in the CS
        '   Returns null if there are no more
        '========================================================================
        '
        Function db_GetCSFirstFieldName(ByVal CSPointer As Integer) As String
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-060" : ''Dim th as integer : th = profileLogMethodEnter(Tn)

            Dim MethodName As String
            Dim ContentName As String
            '
            MethodName = "csv_GetCSFirstFieldName"
            db_GetCSFirstFieldName = ""
            '
            If Not cs_Ok(CSPointer) Then
                Call handleLegacyClassError3(MethodName, ("csv_ContentSet invalid Or End-Of-file"))
            Else
                db_ContentSet(CSPointer).fieldPointer = 0
                db_GetCSFirstFieldName = db_GetCSNextFieldName(CSPointer)
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function
        '
        '========================================================================
        ' get the next fieldname in the CS
        '   Returns null if there are no more
        '========================================================================
        '
        Function db_GetCSNextFieldName(ByVal CSPointer As Integer) As String
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-061" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim ContentPointer As Integer
            '
            db_GetCSNextFieldName = ""
            '
            If Not cs_Ok(CSPointer) Then
                Call handleLegacyClassError3("csv_GetCSNextFieldName", "csv_ContentSet invalid Or End-Of-file")
            Else
                With db_ContentSet(CSPointer)
                    If useCSReadCacheMultiRow Then
                        Do While (db_GetCSNextFieldName = "") And (.fieldPointer < .ResultColumnCount)
                            db_GetCSNextFieldName = .fieldNames(.fieldPointer)
                            .fieldPointer = .fieldPointer + 1
                        Loop
                    Else
                        Do While (db_GetCSNextFieldName = "") And (.fieldPointer < .dt.Columns.Count)
                            db_GetCSNextFieldName = .dt.Columns(.fieldPointer).ColumnName
                            .fieldPointer = .fieldPointer + 1
                        Loop
                    End If
                End With
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_GetCSNextFieldName", True)
        End Function
        '
        '========================================================================
        ' get the type of a field within a csv_ContentSet
        '========================================================================
        '
        Public Function db_GetCSFieldTypeId(ByVal CSPointer As Integer, ByVal FieldName As String) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-062" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim MethodName As String
            Dim ContentName As String
            'Dim CDef As CDefType
            Dim arrayOfFields As coreMetaDataClass.CDefFieldClass()
            '
            MethodName = "csv_GetCSFieldType"
            '
            db_GetCSFieldTypeId = 0
            If cs_Ok(CSPointer) Then
                If db_ContentSet(CSPointer).Updateable Then
                    With db_ContentSet(CSPointer).CDef
                        If .Name <> "" Then
                            db_GetCSFieldTypeId = .fields(FieldName.ToLower()).fieldTypeId
                        End If
                    End With
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function
        '
        '========================================================================
        ' get the caption of a field within a csv_ContentSet
        '========================================================================
        '
        Public Function db_getCSFieldCaption(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-063" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim MethodName As String
            Dim CDef As coreMetaDataClass.CDefClass
            Dim arrayOfFields As coreMetaDataClass.CDefFieldClass()
            '
            MethodName = "csv_GetCSFieldCaption"
            '
            db_getCSFieldCaption = ""
            If cs_Ok(CSPointer) Then
                If db_ContentSet(CSPointer).Updateable Then
                    With db_ContentSet(CSPointer).CDef
                        If .Name <> "" Then
                            db_getCSFieldCaption = .fields(FieldName.ToLower()).caption
                        End If
                    End With
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function
        '
        '========================================================================
        ' get the caption of a field within a csv_ContentSet
        '========================================================================
        '
        Public Function db_GetCSSelectFieldList(ByVal CSPointer As Integer) As String
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-064" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim MethodName As String
            Dim CDef As coreMetaDataClass.CDefClass
            '
            MethodName = "csv_GetCSSelectFieldList"
            '
            db_GetCSSelectFieldList = ""
            If cs_Ok(CSPointer) Then
                If useCSReadCacheMultiRow Then
                    db_GetCSSelectFieldList = Join(db_ContentSet(CSPointer).fieldNames, ",")
                Else
                    db_GetCSSelectFieldList = db_ContentSet(CSPointer).SelectTableFieldList
                    If db_GetCSSelectFieldList = "" Then
                        With db_ContentSet(CSPointer)
                            If Not (.dt Is Nothing) Then
                                If .dt.Columns.Count > 0 Then
                                    For FieldPointer = 0 To .dt.Columns.Count - 1
                                        db_GetCSSelectFieldList = db_GetCSSelectFieldList & "," & .dt.Columns(FieldPointer).ColumnName
                                    Next
                                    db_GetCSSelectFieldList = Mid(db_GetCSSelectFieldList, 2)
                                End If
                            End If
                        End With
                    End If
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function
        '
        '========================================================================
        ' get the caption of a field within a csv_ContentSet
        '========================================================================
        '
        Public Function db_IsCSFieldSupported(ByVal CSPointer As Integer, ByVal FieldName As String) As Boolean
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-065" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CSSelectFieldList As String
            Dim MethodName As String
            '
            MethodName = "csv_IsCSFieldSupported"
            '
            CSSelectFieldList = db_GetCSSelectFieldList(CSPointer)
            db_IsCSFieldSupported = IsInDelimitedString(CSSelectFieldList, FieldName, ",")
            'csv_IsCSFieldSupported = vbInstr(1, CSSelectFieldList & ",", "." & FieldName & ",", vbTextCompare)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function
        '
        '=====================================================================================
        '   csv_GetCSFilename (only valid for fields of TextFile and File type
        '
        '   Attempt to read the filename from the field
        '   if no filename, attempt to create it from the tablename-recordid
        '   if no recordid, create filename from a random
        '=====================================================================================
        '
        Public Function cs_getFilename(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal OriginalFilename As String, Optional ByVal ContentName As String = "") As String
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-066" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim fieldTypeId As Integer
            Dim MethodName As String
            Dim TableName As String
            Dim RecordID As Integer
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim UcaseFieldName As String
            Dim LenOriginalFilename As Integer
            Dim LenFilename As Integer
            Dim Pos As Integer
            '
            MethodName = "csv_GetCSFilename"
            '
            If Not cs_Ok(CSPointer) Then
                Call Err.Raise(ignoreInteger, "dll", "CSPointer Not OK")
            ElseIf FieldName = "" Then
                Call Err.Raise(ignoreInteger, "dll", "Fieldname Is blank")
            Else
                UcaseFieldName = vbUCase(Trim(FieldName))
                cs_getFilename = EncodeText(db_GetCSField(CSPointer, UcaseFieldName))
                If cs_getFilename <> "" Then
                    '
                    ' ----- A filename came from the record
                    '
                    If OriginalFilename <> "" Then
                        '
                        ' ----- there was an original filename, make sure it matches the one in the record
                        '
                        LenOriginalFilename = Len(OriginalFilename)
                        LenFilename = Len(cs_getFilename)
                        Pos = (1 + LenFilename - LenOriginalFilename)
                        If Pos <= 0 Then
                            '
                            ' Original Filename changed, create a new csv_GetCSFilename
                            '
                            cs_getFilename = ""
                        ElseIf Mid(cs_getFilename, Pos) <> OriginalFilename Then
                            '
                            ' Original Filename changed, create a new csv_GetCSFilename
                            '
                            cs_getFilename = ""
                        End If
                    End If
                End If
                If cs_getFilename = "" Then
                    With db_ContentSet(CSPointer)
                        '
                        ' ----- no filename present, get id field
                        '
                        If .ResultColumnCount > 0 Then
                            For FieldPointer = 0 To .ResultColumnCount - 1
                                If vbUCase(.fieldNames(FieldPointer)) = "ID" Then
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
                            If db_ContentSet(CSPointer).WorkflowAuthoringMode Then
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
                            Call Err.Raise(ignoreInteger, "dll", "Can Not create a filename because no ContentName was given, And the csv_ContentSet Is SQL-based.")
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
                        cs_getFilename = csv_GetVirtualFilenameByTable(TableName, FieldName, RecordID, OriginalFilename, fieldTypeId)
                        Call cs_setField(CSPointer, UcaseFieldName, cs_getFilename)
                    End With
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function
        '
        '   csv_GetCSText
        '
        Public Function cs_getText(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            cs_getText = EncodeText(db_GetCSField(CSPointer, FieldName))
        End Function
        '
        '   encodeInteger( csv_GetCSField )
        '
        Public Function cs_getInteger(ByVal CSPointer As Integer, ByVal FieldName As String) As Integer
            cs_getInteger = EncodeInteger(db_GetCSField(CSPointer, FieldName))
        End Function
        '
        '   encodeNumber( csv_GetCSField )
        '
        Public Function db_GetCSNumber(ByVal CSPointer As Integer, ByVal FieldName As String) As Double
            db_GetCSNumber = EncodeNumber(db_GetCSField(CSPointer, FieldName))
        End Function
        '
        '   encodeDate( csv_GetCSField )
        '
        Public Function db_GetCSDate(ByVal CSPointer As Integer, ByVal FieldName As String) As Date
            db_GetCSDate = EncodeDate(db_GetCSField(CSPointer, FieldName))
        End Function
        '
        '   encodeBoolean( csv_GetCSField )
        '
        Public Function cs_getBoolean(ByVal CSPointer As Integer, ByVal FieldName As String) As Boolean
            cs_getBoolean = EncodeBoolean(db_GetCSField(CSPointer, FieldName))
        End Function
        '
        '   encodeBoolean( csv_GetCSField )
        '
        Public Function db_GetCSLookup(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            db_GetCSLookup = db_GetCS(CSPointer, FieldName)
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
        Public Sub db_SetCSTextFile(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal Copy As String, ByVal ContentName As String)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-067" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim Filename As String
            Dim OldFilename As String
            Dim OldCopy As String
            '
            If Not cs_Ok(CSPointer) Then
                '
                Call handleLegacyClassError3("csv_SetCSTextFile", ("csv_ContentSet invalid Or End-Of-file"))
            Else
                With db_ContentSet(CSPointer)
                    If Not .Updateable Then
                        '
                        Call handleLegacyClassError3("csv_SetCSTextFile", ("Attempting To update an unupdateable Content Set"))
                    Else
                        OldFilename = cs_getText(CSPointer, FieldName)
                        Filename = cs_getFilename(CSPointer, FieldName, "", ContentName)
                        If OldFilename <> Filename Then
                            '
                            ' Filename changed, mark record changed
                            '
                            Call cpCore.privateFiles.saveFile(Filename, Copy)
                            Call cs_setField(CSPointer, FieldName, Filename)
                        Else
                            OldCopy = cpCore.cdnFiles.readFile(Filename)
                            If OldCopy <> Copy Then
                                '
                                ' copy changed, mark record changed
                                '
                                Call cpCore.privateFiles.saveFile(Filename, Copy)
                                Call cs_setField(CSPointer, FieldName, Filename)
                            End If
                        End If
                    End If
                End With
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_SetCSTextFile", True)
        End Sub
        '
        '====================================================================================
        ' Get the value of a a csv_ContentSet Field for a TextFile fieldtype
        '   (returns the content of the filename stored in the field)
        '
        '   CSPointer   The current Content Set Pointer
        '   FieldName   The name of the field to be saved
        '====================================================================================
        '
        Public Function db_csGetTextFile(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-068" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim Filename As String
            '
            Filename = cs_getText(CSPointer, FieldName)
            If Filename <> "" Then
                db_csGetTextFile = cpCore.cdnFiles.readFile(Filename)
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_GetCSTextFile", True)
        End Function

        '
        '========================================================================
        ' set the value of a field within a csv_ContentSet
        '========================================================================
        '
        Public Sub cs_setField(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Object)
            Try
                Dim FieldNameLocal As String
                Dim FieldNameLocalUcase As String
                Dim SetNeeded As Boolean
                Dim field As coreMetaDataClass.CDefFieldClass
                '
                If Not cs_Ok(CSPointer) Then
                    cpCore.handleExceptionAndRethrow(New ApplicationException("csv_ContentSet invalid Or End-Of-file"))
                Else
                    With db_ContentSet(CSPointer)
                        If Not .Updateable Then
                            cpCore.handleExceptionAndRethrow(New ApplicationException("Attempting To update an unupdateable Content Set"))
                        Else
                            FieldNameLocal = Trim(FieldName)
                            FieldNameLocalUcase = vbUCase(FieldNameLocal)
                            With .CDef
                                If .Name <> "" Then
                                    Try
                                        field = .fields(FieldName.ToLower)
                                    Catch ex As Exception
                                        Throw New ApplicationException("setField failed because field [" & FieldName.ToLower & "] was not found in content [" & .Name & "]", ex)
                                    End Try
                                    Select Case field.fieldTypeId
                                        Case FieldTypeIdAutoIdIncrement, FieldTypeIdRedirect, FieldTypeIdManyToMany
                                            '
                                            ' Never set
                                            '
                                        Case FieldTypeIdFileTextPrivate, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript, FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdFileHTMLPrivate
                                            '
                                            ' Always set
                                            ' TextFile, assume this call is only made if a change was made to the copy.
                                            ' Use the csv_SetCSTextFile to manage the modified name and date correctly.
                                            ' csv_SetCSTextFile uses this method to set the row changed, so leave this here.
                                            '
                                            SetNeeded = True
                                        Case FieldTypeIdBoolean
                                            '
                                            ' Boolean - sepcial case, block on typed GetAlways set
                                            If EncodeBoolean(FieldValue) <> cs_getBoolean(CSPointer, FieldName) Then
                                                SetNeeded = True
                                            End If
                                        Case Else
                                            '
                                            ' Set if text of value changes
                                            '
                                            If EncodeText(FieldValue) <> cs_getText(CSPointer, FieldName) Then
                                                SetNeeded = True
                                            End If
                                    End Select
                                End If
                            End With
                            If Not SetNeeded Then
                                SetNeeded = SetNeeded
                            Else
                                If db_ContentSet(CSPointer).WorkflowAuthoringMode Then
                                    '
                                    ' Do phantom ID replacement
                                    '
                                    If FieldNameLocalUcase = "ID" Then
                                        FieldNameLocal = "EditSourceID"
                                        FieldNameLocalUcase = vbUCase(FieldNameLocal)
                                    End If
                                End If
                                If .writeCache.ContainsKey(FieldName.ToLower) Then
                                    .writeCache.Item(FieldName.ToLower) = EncodeText(FieldValue)
                                Else
                                    .writeCache.Add(FieldName.ToLower, EncodeText(FieldValue))
                                End If
                                .LastUsed = DateTime.Now
                            End If
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        ' ContentServer version of getDataRowColumnName
        '
        Public Function db_getDataRowColumnName(ByVal dr As DataRow, ByVal FieldName As String) As Object
            Return dr.Item(FieldName).ToString
        End Function
        '
        '========================================================================
        ' csv_InsertContentRecordGetID
        '   Inserts a record based on a content definition.
        '   Returns the ID of the record, -1 if error
        '========================================================================
        '
        Public Function metaData_InsertContentRecordGetID(ByVal ContentName As String, ByVal MemberID As Integer) As Integer
            On Error GoTo ErrorTrap
            '
            Dim CS As Integer
            '
            metaData_InsertContentRecordGetID = -1
            CS = cs_insertRecord(ContentName, MemberID)
            If cs_Ok(CS) Then
                metaData_InsertContentRecordGetID = cs_getInteger(CS, "ID")
            End If
            Call cs_Close(CS)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_InsertContentRecordGetID", True)
        End Function
        '
        '========================================================================
        ' Delete Content Record
        '
        '   If not Workflowauthoringmode is provided, the default is the ContentDefinition
        '========================================================================
        '
        Public Sub db_DeleteContentRecord(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal MemberID As Integer = SystemMemberID)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-072" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            Dim CSPointer As Integer
            Dim WorkflowMode As Boolean
            Dim ContentPointer As Integer
            '
            MethodName = "csv_DeleteContentRecord"
            '
            CSPointer = db_OpenCSContentRecord(ContentName, RecordID, MemberID, True, True)
            If cs_Ok(CSPointer) Then
                Call db_DeleteCSRecord(CSPointer)
            End If
            Call cs_Close(CSPointer)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Sub
        '
        '========================================================================
        ' csv_DeleteContentRecords
        '========================================================================
        '
        Public Sub deleteContentRecords(ByVal ContentName As String, ByVal Criteria As String, Optional ByVal MemberID As Integer = 0)
            Try
                '
                Dim MethodName As String
                Dim CSPointer As Integer
                Dim CDef As coreMetaDataClass.CDefClass
                Dim ContentCriteria As String
                '
                MethodName = "csv_DeleteContentRecords"
                '
                CDef = cpCore.metaData.getCdef(ContentName)
                If CDef Is Nothing Then
                    cpCore.handleExceptionAndRethrow(New ApplicationException("ContentName [" & ContentName & "] was Not found"))
                Else
                    If CDef.Id = 0 Then
                        cpCore.handleExceptionAndRethrow(New ApplicationException("ContentName [" & ContentName & "] was Not found"))
                    Else
                        '
                        ' Delete records from Content
                        '
                        If cpCore.siteProperties.allowWorkflowAuthoring And (CDef.AllowWorkflowAuthoring) Then
                            '
                            ' Supports Workflow Authoring, handle it record at a time
                            '
                            CSPointer = csOpen(ContentName, Criteria, , False, MemberID, True, True, "ID")
                            Do While cs_Ok(CSPointer)
                                Call db_DeleteCSRecord(CSPointer)
                                Call db_csGoNext(CSPointer)
                            Loop
                            Call cs_Close(CSPointer)
                        Else
                            '
                            ' No Workflow Authoring, just delete records
                            '
                            ContentCriteria = "(" & Criteria & ")And(" & CDef.ContentControlCriteria & ")"
                            Call db_DeleteTableRecords(CDef.ContentDataSourceName, CDef.ContentTableName, ContentCriteria)
                            If cpCore.workflow.csv_AllowAutocsv_ClearContentTimeStamp Then
                                Call cpCore.cache.invalidateTag(ContentName)
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '========================================================================
        ' Inserts a record in a content definition and returns a csv_ContentSet with just that record
        '   Returns and long that points into the csv_ContentSet array
        '   If there was a problem, it returns -1
        '
        '   If the content definition supports Workflow Authoring...
        '       an empty database record is created in the live table
        '       an editable record is created in the edit table
        '========================================================================
        '
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
                Dim CDef As coreMetaDataClass.CDefClass
                Dim DefaultValueText As String
                Dim LookupContentName As String
                Dim Ptr As Integer
                Dim lookups() As String
                Dim UCaseDefaultValueText As String
                Dim sqlList As New sqlFieldListClass
                '
                If MemberID = -1 Then
                    MemberID = cpCore.user.id
                End If
                returnCs = -1
                CDef = cpCore.metaData.getCdef(ContentName)
                If (CDef Is Nothing) Then
                    '
                    Throw (New ApplicationException("content [" & ContentName & "] could not be found."))
                Else
                    With CDef
                        WorkflowAuthoringMode = .AllowWorkflowAuthoring And cpCore.siteProperties.allowWorkflowAuthoring
                        If WorkflowAuthoringMode Then
                            '
                            ' authoring, Create Blank in Live Table
                            '
                            LiveRecordID = db_InsertTableRecordGetID(.ContentDataSourceName, .ContentTableName, MemberID)
                            sqlList = New sqlFieldListClass
                            Call sqlList.add("EDITBLANK", SQLTrue) ' Pointer)
                            Call sqlList.add("EDITSOURCEID", encodeSQLNumber(Nothing)) ' Pointer)
                            Call sqlList.add("EDITARCHIVE", SQLFalse) ' Pointer)
                            Call db_UpdateTableRecord(.ContentDataSourceName, .ContentTableName, "ID=" & LiveRecordID, sqlList)
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
                            For Each keyValuePair As KeyValuePair(Of String, coreMetaDataClass.CDefFieldClass) In .fields
                                Dim field As coreMetaDataClass.CDefFieldClass = keyValuePair.Value
                                With field
                                    FieldName = .nameLc
                                    If (FieldName <> "") And (Not String.IsNullOrEmpty(.defaultValue)) Then
                                        Select Case vbUCase(FieldName)
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
                                                        sqlList.add(FieldName, encodeSQLBoolean(EncodeBoolean(.defaultValue)))
                                                    Case FieldTypeIdCurrency, FieldTypeIdFloat, FieldTypeIdInteger, FieldTypeIdMemberSelect
                                                        sqlList.add(FieldName, encodeSQLNumber(.defaultValue))
                                                    Case FieldTypeIdDate
                                                        sqlList.add(FieldName, encodeSQLDate(EncodeDate(.defaultValue)))
                                                    Case FieldTypeIdLookup
                                                        '
                                                        ' *******************************
                                                        ' This is a problem - the defaults should come in as the ID values, not the names
                                                        '   so a select can be added to the default configuration page
                                                        ' *******************************
                                                        '
                                                        DefaultValueText = EncodeText(.defaultValue)
                                                        If DefaultValueText = "" Then
                                                            DefaultValueText = "null"
                                                        Else
                                                            If .lookupContentID <> 0 Then
                                                                LookupContentName = cpCore.metaData.getContentNameByID(.lookupContentID)
                                                                If LookupContentName <> "" Then
                                                                    DefaultValueText = getRecordID(LookupContentName, DefaultValueText).ToString()
                                                                End If
                                                            ElseIf .lookupList <> "" Then
                                                                UCaseDefaultValueText = vbUCase(DefaultValueText)
                                                                lookups = Split(.lookupList, ",")
                                                                For Ptr = 0 To UBound(lookups)
                                                                    If UCaseDefaultValueText = vbUCase(lookups(Ptr)) Then
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
                        CreateKeyString = encodeSQLNumber(getRandomLong)
                        DateAddedString = encodeSQLDate(Now)
                        '
                        Call sqlList.add("CREATEKEY", CreateKeyString) ' ArrayPointer)
                        Call sqlList.add("DATEADDED", DateAddedString) ' ArrayPointer)
                        Call sqlList.add("CONTENTCONTROLID", encodeSQLNumber(CDef.Id)) ' ArrayPointer)
                        Call sqlList.add("CREATEDBY", encodeSQLNumber(MemberID)) ' ArrayPointer)
                        '
                        Call db_InsertTableRecord(DataSourceName, TableName, sqlList)
                        '
                        ' ----- Get the record back so we can use the ID
                        '
                        Criteria = "((createkey=" & CreateKeyString & ")And(DateAdded=" & DateAddedString & "))"
                        returnCs = csOpen(ContentName, Criteria, "ID DESC", False, MemberID, WorkflowAuthoringMode, True)
                        '
                        ' ----- Clear Time Stamp because a record changed
                        '
                        If cpCore.workflow.csv_AllowAutocsv_ClearContentTimeStamp Then
                            Call cpCore.cache.invalidateTag(ContentName)
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw ex
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
        Public Function db_OpenCSContentRecord(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal MemberID As Integer = SystemMemberID, Optional ByVal WorkflowAuthoringMode As Boolean = False, Optional ByVal WorkflowEditingMode As Boolean = False, Optional ByVal SelectFieldList As String = "") As Integer
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-109" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            '
            MethodName = "db_csOpen"
            '
            db_OpenCSContentRecord = csOpen(ContentName, "(ID=" & encodeSQLNumber(RecordID) & ")", , False, MemberID, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1)
            '
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function
        '        '
        '        '========================================================================
        '        '   2.1 Compatibility
        '        '========================================================================
        '        '
        '        Public Function db_InsertContentRecordByPointer(ByVal ContentPointer As Integer, ByVal MemberID As Integer) As Integer
        '            On Error GoTo ErrorTrap 'Const Tn = "MethodName-110" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim MethodName As String
        '            Dim CDef As appServices_metaDataClass.CDefClass
        '            '
        '            MethodName = "csv_InsertContentRecordByPointer"
        '            '
        '            CDef = cdefServices.GetCDefByPointer(ContentPointer)
        '            '
        '            db_InsertContentRecordByPointer = db_InsertContentRecordGetID(CDef.Name, MemberID)
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        '        End Function
        '
        '========================================================================
        ' csv_IsCSOK( CSPointer )
        '   returns true if the csv_ContentSet is not empty and not EOF
        '========================================================================
        '
        Function cs_Ok(ByVal CSPointer As Integer) As Boolean
            On Error GoTo ErrorTrap 'Const Tn = "IsCSOK" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            If CSPointer > 0 Then
                If useCSReadCacheMultiRow Then
                    With db_ContentSet(CSPointer)
                        cs_Ok = .IsOpen And (.readCacheRowPtr >= 0) And (.readCacheRowPtr < .readCacheRowCnt)
                    End With
                Else
                    cs_Ok = db_ContentSet(CSPointer).IsOpen And (Not db_ContentSet(CSPointer).ResultEOF)
                End If
                'With csv_ContentSet(CSPointer)
                '    '
                '    ' ----- change this so the last row returns with EOF status
                '    '
                '    csv_IsCSOK = (csv_ContentSet(CSPointer).IsOpen) And (Not csv_IsCSEOF(CSPointer))
                'End With
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("Unexpected exception"))
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Sub db_CopyCSRecord(ByVal CSSource As Integer, ByVal CSDestination As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-112" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            Dim FieldName As String
            Dim ContentControlID As Integer
            Dim DestContentName As String
            Dim DestRecordID As Integer
            Dim DestFilename As String
            Dim SourceFilename As String
            Dim DestCDef As coreMetaDataClass.CDefClass
            '
            MethodName = "csv_CopyCSRecord"
            '
            If cs_Ok(CSSource) And cs_Ok(CSDestination) Then
                '
                DestCDef = db_ContentSet(CSDestination).CDef
                DestContentName = DestCDef.Name
                DestRecordID = cs_getInteger(CSDestination, "ID")
                FieldName = db_GetCSFirstFieldName(CSSource)
                Do While (FieldName <> "")
                    Select Case vbUCase(FieldName)
                        Case "ID", "EDITSOURCEID", "EDITARCHIVE"
                        Case Else
                            '
                            ' ----- fields to copy
                            '
                            Select Case db_GetCSFieldTypeId(CSSource, FieldName)
                                Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript
                                    '
                                    ' ----- cdn file
                                    '
                                    SourceFilename = cs_getFilename(CSSource, FieldName, "")
                                    'SourceFilename = (csv_GetCSText(CSSource, FieldName))
                                    If (SourceFilename <> "") Then
                                        DestFilename = cs_getFilename(CSDestination, FieldName, "")
                                        'DestFilename = csv_GetVirtualFilename(DestContentName, FieldName, DestRecordID)
                                        Call cs_setField(CSDestination, FieldName, DestFilename)
                                        Call cpCore.cdnFiles.copyFile(SourceFilename, DestFilename)
                                    End If
                                Case FieldTypeIdFileTextPrivate, FieldTypeIdFileHTMLPrivate
                                    '
                                    ' ----- private file
                                    '
                                    SourceFilename = cs_getFilename(CSSource, FieldName, "")
                                    'SourceFilename = (csv_GetCSText(CSSource, FieldName))
                                    If (SourceFilename <> "") Then
                                        DestFilename = cs_getFilename(CSDestination, FieldName, "")
                                        'DestFilename = csv_GetVirtualFilename(DestContentName, FieldName, DestRecordID)
                                        Call cs_setField(CSDestination, FieldName, DestFilename)
                                        Call cpCore.privateFiles.copyFile(SourceFilename, DestFilename)
                                    End If
                                Case Else
                                    '
                                    ' ----- value
                                    '
                                    Call cs_setField(CSDestination, FieldName, db_GetCSField(CSSource, FieldName))
                            End Select
                    End Select
                    FieldName = db_GetCSNextFieldName(CSSource)
                Loop
                Call db_SaveCSRecord(CSDestination)
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Sub   '
        '========================================================================
        '   csv_GetCSSource
        '       Returns the Source for the csv_ContentSet
        '========================================================================
        '
        Public Function db_GetCSSource(ByVal CSPointer As Integer) As String
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-187" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            If cs_Ok(CSPointer) Then
                db_GetCSSource = db_ContentSet(CSPointer).Source
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_GetCSSource", True)
        End Function
        '
        '========================================================================
        '   csv_GetCS
        '
        '   Returns the value of a field, decoded into a text string result
        '       if there is a problem, null is returned
        '       this may be because the lookup record is inactive, so its not an error
        '========================================================================
        '
        Public Function db_GetCS(ByVal CSPointer As Integer, ByVal FieldName As String) As String
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
                db_GetCS = ""
                If Not cs_Ok(CSPointer) Then
                    '
                    cpCore.handleExceptionAndRethrow(New Exception("csv_ContentSet Is empty Or End Of file"))
                Else
                    '
                    ' csv_ContentSet good
                    '
                    With db_ContentSet(CSPointer)
                        If Not .Updateable Then
                            '
                            ' Not updateable -- Just return what is there as a string
                            '
                            Try
                                db_GetCS = EncodeText(db_GetCSField(CSPointer, FieldName))
                            Catch ex As Exception
                                Throw New ApplicationException("error [" & ex.Message & "] reading field [" & FieldName.ToLower & "] in source [" & .Source & "")
                            End Try
                        Else
                            '
                            ' Updateable -- enterprete the value
                            '
                            'ContentName = .ContentName
                            Dim field As coreMetaDataClass.CDefFieldClass
                            If Not .CDef.fields.ContainsKey(FieldName.ToLower()) Then
                                Try
                                    db_GetCS = EncodeText(db_GetCSField(CSPointer, FieldName))
                                Catch ex As Exception
                                    Throw New ApplicationException("error [" & ex.Message & "] reading field [" & FieldName.ToLower & "] in content [" & .CDef.Name & "] with custom field list [" & .SelectTableFieldList & "")
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
                                        RecordID = EncodeInteger(db_GetCSField(CSPointer, "id"))
                                        With field
                                            ContentName = cpCore.metaData.getContentNameByID(.manyToManyRuleContentID)
                                            DbTable = cpCore.metaData.getContentTablename(ContentName)
                                            SQL = "Select " & .ManyToManyRuleSecondaryField & " from " & DbTable & " where " & .ManyToManyRulePrimaryField & "=" & RecordID
                                            rs = executeSql_getDataTable(SQL)
                                            If (isDataTableOk(rs)) Then
                                                For Each dr As DataRow In rs.Rows
                                                    db_GetCS &= "," & dr.Item(0).ToString
                                                Next
                                                db_GetCS = db_GetCS.Substring(1)
                                            End If
                                        End With
                                    End If
                                ElseIf fieldTypeId = FieldTypeIdRedirect Then
                                    '
                                    ' special case - recordset contains no data - return blank
                                    '
                                    fieldTypeId = fieldTypeId
                                Else
                                    FieldValueVariant = db_GetCSField(CSPointer, FieldName)
                                    If Not IsNull(FieldValueVariant) Then
                                        '
                                        ' Field is good
                                        '
                                        Select Case fieldTypeId
                                            Case FieldTypeIdBoolean
                                                '
                                                '
                                                '
                                                If EncodeBoolean(FieldValueVariant) Then
                                                    db_GetCS = "Yes"
                                                Else
                                                    db_GetCS = "No"
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
                                                    db_GetCS = EncodeDate(FieldValueVariant).ToString()
                                                End If
                                            Case FieldTypeIdLookup
                                                '
                                                '
                                                '
                                                If vbIsNumeric(FieldValueVariant) Then
                                                    fieldLookupId = field.lookupContentID
                                                    LookupContentName = cpCore.metaData.getContentNameByID(fieldLookupId)
                                                    LookupList = field.lookupList
                                                    If (LookupContentName <> "") Then
                                                        '
                                                        ' First try Lookup Content
                                                        '
                                                        CSLookup = csOpen(LookupContentName, "ID=" & encodeSQLNumber(FieldValueVariant), , , , , , "name", 1)
                                                        If cs_Ok(CSLookup) Then
                                                            db_GetCS = cs_getText(CSLookup, "name")
                                                        End If
                                                        Call cs_Close(CSLookup)
                                                    ElseIf LookupList <> "" Then
                                                        '
                                                        ' Next try lookup list
                                                        '
                                                        FieldValueInteger = EncodeInteger(FieldValueVariant) - 1
                                                        lookups = Split(LookupList, ",")
                                                        If UBound(lookups) >= FieldValueInteger Then
                                                            db_GetCS = lookups(FieldValueInteger)
                                                        End If
                                                    End If
                                                End If
                                            Case FieldTypeIdMemberSelect
                                                '
                                                '
                                                '
                                                If vbIsNumeric(FieldValueVariant) Then
                                                    db_GetCS = db_GetRecordName("people", EncodeInteger(FieldValueVariant))
                                                End If
                                            Case FieldTypeIdCurrency
                                                '
                                                '
                                                '
                                                If vbIsNumeric(FieldValueVariant) Then
                                                    db_GetCS = FormatCurrency(FieldValueVariant, 2, vbFalse, vbFalse, vbFalse)
                                                End If
                                            'NeedsHTMLEncode = False
                                            Case FieldTypeIdFileTextPrivate, FieldTypeIdFileHTMLPrivate
                                                '
                                                '
                                                '
                                                db_GetCS = cpCore.privateFiles.readFile(EncodeText(FieldValueVariant))
                                            Case FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript
                                                '
                                                '
                                                '
                                                db_GetCS = cpCore.cdnFiles.readFile(EncodeText(FieldValueVariant))
                                            'NeedsHTMLEncode = False
                                            Case FieldTypeIdText, FieldTypeIdLongText, FieldTypeIdHTML
                                                '
                                                '
                                                '
                                                db_GetCS = EncodeText(FieldValueVariant)
                                            Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdAutoIdIncrement, FieldTypeIdFloat, FieldTypeIdInteger
                                                '
                                                '
                                                '
                                                db_GetCS = EncodeText(FieldValueVariant)
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
                                                Throw New ApplicationException("Can not use field [" & FieldName & "] because the FieldType [" & fieldTypeId & "] Is invalid.")
                                        End Select
                                    End If
                                End If
                            End If
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Function
        '
        '========================================================================
        '   csv_SetCS
        '       Saves the value to the field, independant of field type
        '       this routine accounts for the destination type, and saves the field as required (file, etc)
        '           csv_SetCS( CS, "copyfilename", "This Is the text" ) - saves this in the file
        '========================================================================
        '
        Public Sub cs_set(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Object)
            Try
                Dim BlankTest As String
                Dim FieldValueText As String
                Dim FieldNameLc As String
                Dim FieldValueVariantLocal As Object
                Dim SetNeeded As Boolean
                Dim fileNameNoExt As String
                Dim ContentName As String
                Dim fileName As String
                Dim pathFilenameOriginal As String
                '
                If Not cs_Ok(CSPointer) Then
                    Throw New ApplicationException("contentset is invalid Or End-Of-file.")
                Else
                    With db_ContentSet(CSPointer)
                        If Not .Updateable Then
                            Throw New ApplicationException("Can not update a contentset created from a sql query.")
                        Else
                            ContentName = .ContentName
                            FieldNameLc = Trim(FieldName).ToLower
                            FieldValueVariantLocal = FieldValue
                            With .CDef
                                If .Name <> "" Then
                                    Dim field As coreMetaDataClass.CDefFieldClass
                                    If Not .fields.ContainsKey(FieldNameLc) Then
                                        Throw New ArgumentException("The field [" & FieldName & "] could not be found in content [" & .Name & "]")
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
                                                ' csv_GetCS returns the filename
                                                ' csv_SetCS saves the filename
                                                '
                                                FieldValueVariantLocal = FieldValueVariantLocal
                                                SetNeeded = True
                                            Case FieldTypeIdFileTextPrivate, FieldTypeIdFileHTMLPrivate
                                                '
                                                ' Always set
                                                ' A virtual file is created to hold the content, 'tablename/FieldNameLocal/0000.ext
                                                ' the extension is different for each fieldtype
                                                ' csv_SetCS and csv_GetCS return the content, not the filename
                                                '
                                                ' Saved in the field is the filename of the virtual file
                                                ' TextFile, assume this call is only made if a change was made to the copy.
                                                ' Use the csv_SetCSTextFile to manage the modified name and date correctly.
                                                ' csv_SetCSTextFile uses this method to set the row changed, so leave this here.
                                                '
                                                fileNameNoExt = cs_getText(CSPointer, FieldNameLc)
                                                FieldValueText = EncodeText(FieldValueVariantLocal)
                                                If FieldValueText = "" Then
                                                    If fileNameNoExt <> "" Then
                                                        Call cpCore.privateFiles.deleteFile(fileNameNoExt)
                                                        'Call publicFiles.DeleteFile(fileNameNoExt)
                                                        fileNameNoExt = ""
                                                    End If
                                                Else
                                                    If fileNameNoExt = "" Then
                                                        fileNameNoExt = cs_getFilename(CSPointer, FieldName, "", ContentName)
                                                    End If
                                                    Call cpCore.privateFiles.saveFile(fileNameNoExt, FieldValueText)
                                                    'Call publicFiles.SaveFile(fileNameNoExt, FieldValueText)
                                                End If
                                                FieldValueVariantLocal = fileNameNoExt
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
                                                FieldValueText = EncodeText(FieldValueVariantLocal)
                                                BlankTest = FieldValueText
                                                BlankTest = vbReplace(BlankTest, " ", "")
                                                BlankTest = vbReplace(BlankTest, vbCr, "")
                                                BlankTest = vbReplace(BlankTest, vbLf, "")
                                                BlankTest = vbReplace(BlankTest, vbTab, "")
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
                                                                If Not vbIsNumeric(fileNameNoExt) Then
                                                                    Pos = vbInstr(1, fileNameNoExt, ".r", vbTextCompare)
                                                                    If Pos > 0 Then
                                                                        FilenameRev = EncodeInteger(Mid(fileNameNoExt, Pos + 2))
                                                                        FilenameRev = FilenameRev + 1
                                                                        fileNameNoExt = Mid(fileNameNoExt, 1, Pos - 1)
                                                                    End If
                                                                End If
                                                                fileName = fileNameNoExt & ".r" & FilenameRev & "." & FileExt
                                                                'PathFilename = PathFilename & dstFilename
                                                                path = convertCdnUrlToCdnPathFilename(path)
                                                                'srcSysFile = config.physicalFilePath & vbReplace(srcPathFilename, "/", "\")
                                                                'dstSysFile = config.physicalFilePath & vbReplace(PathFilename, "/", "\")
                                                                PathFilename = path & fileName
                                                                'Call publicFiles.renameFile(pathFilenameOriginal, fileName)
                                                            End If
                                                        End If
                                                    End If
                                                    If (pathFilenameOriginal <> "") And (pathFilenameOriginal <> PathFilename) Then
                                                        pathFilenameOriginal = convertCdnUrlToCdnPathFilename(pathFilenameOriginal)
                                                        Call cpCore.cdnFiles.deleteFile(pathFilenameOriginal)
                                                    End If
                                                    Call cpCore.cdnFiles.saveFile(PathFilename, FieldValueText)
                                                End If
                                                FieldValueVariantLocal = PathFilename
                                                SetNeeded = True
                                            Case FieldTypeIdBoolean
                                                '
                                                ' Boolean - sepcial case, block on typed GetAlways set
                                                FieldValueVariantLocal = EncodeBoolean(FieldValueVariantLocal)
                                                If EncodeBoolean(FieldValueVariantLocal) <> cs_getBoolean(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                End If
                                            Case FieldTypeIdText
                                                '
                                                ' Set if text of value changes
                                                '
                                                FieldValueVariantLocal = Left(EncodeText(FieldValueVariantLocal), 255)
                                                If EncodeText(FieldValueVariantLocal) <> cs_getText(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                End If
                                            Case FieldTypeIdLongText, FieldTypeIdHTML
                                                '
                                                ' Set if text of value changes
                                                '
                                                FieldValueVariantLocal = Left(EncodeText(FieldValueVariantLocal), 65535)
                                                If EncodeText(FieldValueVariantLocal) <> cs_getText(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                End If
                                            Case Else
                                                '
                                                ' Set if text of value changes
                                                '
                                                If EncodeText(FieldValueVariantLocal) <> cs_getText(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                End If
                                        End Select
                                    End If
                                End If
                            End With
                            If Not SetNeeded Then
                                SetNeeded = SetNeeded
                            Else
                                If db_ContentSet(CSPointer).WorkflowAuthoringMode Then
                                    '
                                    ' Do phantom ID replacement
                                    '
                                    If FieldNameLc = "id" Then
                                        FieldNameLc = "editsourceid"
                                        'FieldNameLocalUcase = vbUCase(FieldNameLocal)
                                    End If
                                End If
                                'If .writeCacheSize = 0 Then
                                '    '
                                '    ' ----- initialize field array for this csv_ContentSet
                                '    '
                                '    .writeCacheSize = .writeCacheSize + .ResultColumnCount
                                '    ReDim .writeCache(.writeCacheSize - 1)
                                'End If
                                '
                                ' ----- set the new value into the row buffer
                                '
                                If .writeCache.ContainsKey(FieldNameLc) Then
                                    .writeCache.Item(FieldNameLc) = FieldValueVariantLocal.ToString()
                                Else
                                    .writeCache.Add(FieldNameLc, FieldValueVariantLocal.ToString())
                                End If
                                'If .writeCacheCount > 0 Then
                                '    '
                                '    ' ----- search buffer for field
                                '    '
                                '    For writeCachePointer = 0 To .writeCacheCount - 1
                                '        With .writeCache(writeCachePointer)
                                '            If FieldNameLocalUcase = vbUCase(.Name) Then
                                '                '
                                '                ' ----- field found, update it
                                '                '
                                '                .ValueVariant = FieldValueVariantLocal
                                '                .Changed = True
                                '                fieldFound = True
                                '                Exit For
                                '            End If
                                '        End With
                                '    Next
                                'End If
                                'If Not fieldFound Then
                                '    '
                                '    ' ----- field not found, create it
                                '    '
                                '    If .writeCacheCount >= .writeCacheSize Then
                                '        .writeCacheSize = .writeCacheSize + .ResultColumnCount
                                '        ReDim Preserve .writeCache(.writeCacheSize)
                                '    End If
                                '    .writeCache(.writeCacheCount).Name = FieldNameLocal
                                '    .writeCache(.writeCacheCount).ValueVariant = FieldValueVariantLocal
                                '    .writeCache(.writeCacheCount).Changed = True
                                '    .writeCacheCount = .writeCacheCount + 1
                                'End If
                                '.writeCacheChanged = True
                                .LastUsed = DateTime.Now
                            End If
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw ex
            End Try
        End Sub
        '
        '
        '
        Public Sub db_RollBackCS(ByVal CSPointer As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-190" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            If cs_Ok(CSPointer) Then
                db_ContentSet(CSPointer).writeCache.Clear()
                'csv_ContentSet(CSPointer).writeCacheCount = 0
                'csv_ContentSet(CSPointer).writeCacheChanged = False
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_RollBackCS", True)
        End Sub
        '
        '========================================================================
        ' Save the current CS Cache back to the database
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
        Public Sub db_SaveCS(ByVal CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False, Optional ByVal Blockcsv_ClearBake As Boolean = False)
            On Error GoTo ErrorTrap 'Const Tn = "SaveCS" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim sqlModifiedDate As Date
            Dim sqlModifiedBy As Integer
            Dim RSDocs As DataTable
            Dim writeCachePointer As Integer
            Dim writeCacheValueVariant As Object
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim FieldCount As Integer
            Dim UcaseFieldName As String
            Dim FieldName As String
            Dim FieldFoundCount As Integer
            Dim FieldAdminAuthorable As Boolean
            Dim FieldReadOnly As Boolean
            Dim rs As DataTable
            Dim SQL As String
            Dim SQLSetPair As String
            Dim SQLUpdate As String
            Dim SQLEditUpdate As String
            Dim SQLEditDelimiter As String
            Dim SQLLiveUpdate As String
            Dim SQLLiveDelimiter As String
            Dim Filename As String
            Dim ContentPointer As Integer
            Dim SQLUnique As String
            Dim UniqueViolationFieldList As String = ""
            Dim UniqueViolation As Boolean
            Dim RSUnique As DataTable
            Dim csv_ContentSetNewRecord As Boolean
            Dim LiveTableName As String
            Dim LiveDataSourceName As String
            Dim LiveRecordID As Integer
            Dim EditRecordID As Integer
            Dim LiveRecordContentControlID As Integer
            Dim LiveRecordContentName As String
            Dim EditTableName As String
            Dim EditDataSourceName As String
            Dim AuthorableFieldUpdate As Boolean            ' true if an Edit field is being updated
            Dim EditLockBlock As Boolean                ' if true, EditLock blocks Authoring Field saves
            Dim WorkflowRenderingMode As Boolean
            Dim AllowWorkflowSave As Boolean
            Dim Copy As String
            Dim ContentID As Integer
            Dim ContentName As String
            Dim MethodName As String
            Dim WorkflowMode As Boolean
            Dim LiveRecordInactive As Boolean
            Dim ColumnPtr As Integer
            Dim writeCacheValueText As String
            '
            MethodName = "csv_SaveCS"
            '
            If cs_Ok(CSPointer) Then
                With db_ContentSet(CSPointer)
                    If (.Updateable) And (.writeCache.Count > 0) Then
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
                            rs = executeSql_getDataTable(SQL, EditDataSourceName)
                            If isDataTableOk(rs) Then
                                EditRecordID = EncodeInteger(db_getDataRowColumnName(rs.Rows(0), "ID"))
                            End If
                            If (isDataTableOk(rs)) Then
                                If False Then
                                    'RS.Dispose)
                                End If
                                'RS = Nothing
                            End If
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
                            UcaseFieldName = vbUCase(FieldName)
                            writeCacheValueVariant = keyValuePair.Value
                            '
                            ' field has changed
                            '
                            If UcaseFieldName = "MODIFIEDBY" Then
                                '
                                ' capture and block it - it is hardcoded in sql
                                '
                                AuthorableFieldUpdate = True
                                sqlModifiedBy = EncodeInteger(writeCacheValueVariant)
                            ElseIf UcaseFieldName = "MODIFIEDDATE" Then
                                '
                                ' capture and block it - it is hardcoded in sql
                                '
                                AuthorableFieldUpdate = True
                                sqlModifiedDate = EncodeDate(writeCacheValueVariant)
                            Else
                                '
                                ' let these field be added to the sql
                                '
                                If UcaseFieldName = "ACTIVE" And (Not EncodeBoolean(writeCacheValueVariant)) Then
                                    '
                                    ' Record being saved inactive
                                    '
                                    LiveRecordInactive = True
                                End If
                                '                            If (InStr(1, "," & protectedcsv_ContentSetControlFieldList & ",", "," & UcaseFieldName & ",") <> 0) Then
                                '                                '
                                '                                ' ----- Control field, log and continue
                                '                                '
                                '                                Call csv_HandleClassErrorAndResume(MethodName, "Record was saved, but field [" & vbLCase(.CDef.Name) & "].[" & vbLCase(FieldName) & "] should Not be used In a csv_ContentSet because it Is one Of the Protected record control fields [" & protectedcsv_ContentSetControlFieldList & "]")
                                '                            End If
                                '
                                Dim field As coreMetaDataClass.CDefFieldClass = .CDef.fields(FieldName.ToLower())
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
                                            Case FieldTypeIdInteger, FieldTypeIdLookup, FieldTypeIdCurrency, FieldTypeIdFloat, FieldTypeIdAutoIdIncrement, FieldTypeIdMemberSelect
                                                SQLSetPair = FieldName & "=" & encodeSQLNumber(writeCacheValueVariant)
                                            Case FieldTypeIdBoolean
                                                SQLSetPair = FieldName & "=" & encodeSQLBoolean(EncodeBoolean(writeCacheValueVariant))
                                            Case FieldTypeIdDate
                                                SQLSetPair = FieldName & "=" & encodeSQLDate(EncodeDate(writeCacheValueVariant))
                                            Case FieldTypeIdText
                                                Copy = Left(EncodeText(writeCacheValueVariant), 255)
                                                If .Scramble Then
                                                    Copy = cpCore.metaData.TextScramble(Copy)
                                                End If
                                                SQLSetPair = FieldName & "=" & encodeSQLText(Copy)
                                            Case FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdFileTextPrivate, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript, FieldTypeIdFileHTMLPrivate
                                                Copy = Left(EncodeText(writeCacheValueVariant), 255)
                                                SQLSetPair = FieldName & "=" & encodeSQLText(Copy)
                                            Case FieldTypeIdLongText, FieldTypeIdHTML
                                                SQLSetPair = FieldName & "=" & encodeSQLText(EncodeText(writeCacheValueVariant))
                                            Case Else
                                                '
                                                ' Invalid fieldtype
                                                '
                                                Call Err.Raise(ignoreInteger, "dll", "Can Not save this record because the field [" & .nameLc & "] has an invalid field type Id [" & .fieldTypeId & "]")
                                        End Select
                                        If SQLSetPair <> "" Then
                                            '
                                            ' ----- Set the new value in the 
                                            '
                                            With db_ContentSet(CSPointer)
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
                                            If .UniqueName And (EncodeText(writeCacheValueVariant) <> "") Then
                                                '
                                                ' ----- set up for unique name check
                                                '
                                                If (Not String.IsNullOrEmpty(SQLUnique)) Then
                                                    SQLUnique &= "Or"
                                                    UniqueViolationFieldList &= ","
                                                End If
                                                writeCacheValueText = EncodeText(writeCacheValueVariant)
                                                If Len(writeCacheValueText) < 255 Then
                                                    UniqueViolationFieldList &= .nameLc & "=""" & writeCacheValueText & """"
                                                Else
                                                    UniqueViolationFieldList &= .nameLc & "=""" & Left(writeCacheValueText, 255) & "..."""
                                                End If
                                                Select Case .fieldTypeId
                                                    Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                                    Case Else
                                                        SQLUnique &= "(" & .nameLc & "=" & db_EncodeSQL(writeCacheValueVariant, .fieldTypeId) & ")"
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
                                                If csv_AllowWorkflowErrors And FieldAdminAuthorable Then
                                                    Call handleLegacyClassError3(MethodName, ("Workflow Edit Error In content[" & ContentName & "], field[" & .nameLc & "]. A csv_ContentSet opened In non-WorkflowRenderingMode, based On a Content Definition which supports Workflow Edit, can Not update fields marked 'Authorable', non-'NotEditable', non-'ReadOnly' or 'Active'. These fields can only be updated through Edit protocols."))
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
                                                If csv_AllowWorkflowErrors And FieldAdminAuthorable Then
                                                    Call handleLegacyClassError3(MethodName, ("Workflow Edit error in content[" & ContentName & "], field[" & .nameLc & "]. You can not update an Authorable field in Workflow Authoring mode without Workflow Editing enabled."))
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
                            RSUnique = executeSql_getDataTable(SQLUnique, LiveDataSourceName)
                            If (RSUnique.Rows.Count > 0) Then
                                UniqueViolation = True
                            End If
                            Call RSUnique.Dispose()
                        End If
                        If UniqueViolation Then
                            '
                            ' ----- trap unique violations here
                            '
                            Call handleLegacyClassError3(MethodName, ("Can not save record to content [" & LiveRecordContentName & "] because it would create a non-unique record for one or more of the following field(s) [" & UniqueViolationFieldList & "]"))
                        ElseIf (FieldFoundCount > 0) Then
                            '
                            ' ----- update live table (non-workflowauthoring and non-authorable fields)
                            '
                            If (SQLLiveUpdate <> "") Then
                                SQLUpdate = "UPDATE " & LiveTableName & " SET " & SQLLiveUpdate & " WHERE ID=" & LiveRecordID & ";"
                                Call executeSql_getDataTable(SQLUpdate, EditDataSourceName)
                            End If
                            '
                            ' ----- update edit table (authoring and non-authoring fields)
                            '
                            If (SQLEditUpdate <> "") Then
                                SQLUpdate = "UPDATE " & EditTableName & " SET " & SQLEditUpdate & " WHERE ID=" & EditRecordID & ";"
                                Call executeSql_getDataTable(SQLUpdate, EditDataSourceName)
                            End If
                            '
                            ' ----- Live record has changed
                            '
                            If AuthorableFieldUpdate And (Not WorkflowRenderingMode) Then
                                '
                                ' ----- reset the ContentTimeStamp to csv_ClearBake
                                '
                                If cpCore.workflow.csv_AllowAutocsv_ClearContentTimeStamp And (Not Blockcsv_ClearBake) Then
                                    Call cpCore.cache.invalidateTag(LiveRecordContentName)
                                End If
                                '
                                ' ----- mark the record NOT UpToDate for SpiderDocs
                                '
                                If (LCase(EditTableName) = "ccpagecontent") And (LiveRecordID <> 0) Then
                                    If db_IsSQLTableField("default", "ccSpiderDocs", "PageID") Then
                                        SQL = "UPDATE ccspiderdocs SET UpToDate = 0 WHERE PageID=" & LiveRecordID
                                        Call executeSql_getDataTable(SQL)
                                    End If
                                End If
                            End If
                        End If
                        .LastUsed = DateTime.Now
                    End If
                End With
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            RSUnique = Nothing
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Sub
        '
        ' ----- Get a DataSource default cursor location
        '
        Private Function db_GetDataSourceDefaultCursorLocation(ByVal DataSourceName As String) As Integer
            Throw New NotImplementedException("cursor location not implemented")
            '            On Error GoTo ErrorTrap : 'Const Tn = "GetDataSourceDefaultCursorLocation" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '            '
            '            Dim DataSourcePointer As Integer
            '            '
            '            DataSourcePointer = csv_GetDataSourcePointer(DataSourceName)
            '            If dataSources.length > 0 Then
            '                If Not DataSourceConnectionObjs(DataSourcePointer).IsOpen Then
            '                    Call csv_OpenDataSource(DataSourceName, 30)
            '                End If
            '                csv_GetDataSourceDefaultCursorLocation = DataSourceConnectionObjs(DataSourcePointer).DefaultCursorLocation
            '            End If
            '            '
            '            Exit Function
            '            '
            '            ' ----- Error Trap
            '            '
            'ErrorTrap:
            '            Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, "csv_GetDataSourceDefaultCursorLocation", True)
        End Function
        '
        '=====================================================================================================
        ' Initialize the csv_ContentSet Result Cache when it is first opened
        '=====================================================================================================
        '
        Private Sub db_initCSData(ByVal CSPointer As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "csv_InitContentSetResult" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            'Dim RecordField2 As Field
            Dim tickStart As Integer
            Dim tickCount As Integer
            Dim PageSize As Integer
            Dim pageStart As Integer
            Dim ColumnPtr As Integer
            Dim hint As String
            '
            MethodName = "csv_InitContentSetResult"
            'hint = MethodName
            '
            With db_ContentSet(CSPointer)
                '
                ' clear results
                '
                .ResultColumnCount = 0
                .readCacheRowCnt = 0
                .readCacheRowPtr = -1
                .writeCache = New Dictionary(Of String, String)
                '.writeCacheCount = 0
                '.writeCacheSize = 0
                .ResultEOF = True
                'hint = hint & ",100"
                If True Then
                    If (True) Then
                        'hint = hint & ",200"
                        '.Source = .rs.Source '
                        If .dt.Rows.Count <= 0 Then
                            'hint = hint & ",210"
                        Else
                            '
                            ' store result
                            '
                            'hint = hint & ",300"
                            .ResultColumnCount = .dt.Columns.Count
                            ColumnPtr = 0
                            ReDim .fieldNames(.ResultColumnCount)
                            'hint = hint & ",310"
                            For Each dc As DataColumn In .dt.Columns
                                'hint = hint & "," & dc.ColumnName
                                .fieldNames(ColumnPtr) = vbUCase(dc.ColumnName)
                                ColumnPtr = ColumnPtr + 1
                            Next
                            '
                            ' ????? test if this will slow things down too much
                            '
                            tickStart = GetTickCount()
                            PageSize = .PageSize
                            pageStart = (.PageNumber - 1) * PageSize
                            'hint = hint & ",400 " & PageSize & "/" & pageStart
                            .readCache = convertDataTabletoArray(.dt)
                            'hint = hint & ",410"
                            '.rs.MoveFirst()
                            'hint = hint & ",420"
                            .readCacheRowCnt = UBound(.readCache, 2) + 1
                            .readCacheRowPtr = 0
                        End If
                        .writeCache = New Dictionary(Of String, String)
                        '.writeCacheSize = .ResultColumnCount
                        'ReDim .writeCache(.writeCacheSize)
                    End If
                End If
                '
                'cpCore.AppendLog("csv_InitContentSetResult, CSPointer=[" & CSPointer & "], .readCacheRowCnt=[" & .readCacheRowCnt & "]")
                '
            End With
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName & ", hint=[" & hint & "]", True)
        End Sub
        '
        '=====================================================================================================
        '   Called to store the Result cache into the csv_ContentSet when CS is first opened
        '=====================================================================================================
        '
        Private Sub db_LoadContentSetCurrentRow(ByVal CSPointer As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "Loadcsv_ContentSetResult" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            'Dim RecordField2 As Field
            Dim ColumnPointer As Integer
            Dim ErrNumber As Integer
            Dim ErrSource As String
            Dim ErrDescription As String
            Dim hint As String
            '
            MethodName = "csv_LoadContentSetCurrentRow"
            '
            'hint = MethodName
            With db_ContentSet(CSPointer)
                If useCSReadCacheMultiRow Then
                    '
                    ' multi row readCache
                    '
                    'hint = hint & ", multi row readCache"
                Else
                    '
                    ' single row readCache
                    '
                    'hint = hint & ", single row readCache"
                    .readCacheRowPtr = 0
                    .readCacheRowCnt = 0
                    .ResultEOF = True
                    If Not (.dt Is Nothing) Then
                        'hint = hint & ", not RS is nothing"
                        If (True) Then
                            '
                            ' Seek to the correct position
                            '
                            .ResultEOF = (.dt.Rows.Count = 0)
                            If Not .ResultEOF Then
                                'hint = hint & ", not .ResultEOF"
                                '
                                ' store result
                                '
                                .readCache = convertDataTabletoArray(.dt)
                                .readCacheRowCnt = 1
                            End If
                        Else
                            MethodName = MethodName
                        End If
                    End If
                End If
            End With
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            '
            ' set EOF, to fix problem where csv_NextCSRecord throws an error, but csv_IsCSOK returns true which causes an endless loop
            '
            ErrNumber = Err.Number
            ErrSource = Err.Source
            ErrDescription = Err.Description
            If (CSPointer >= 0) And (CSPointer < csv_ContentSetCount) Then
                db_ContentSet(CSPointer).ResultEOF = True
            End If
            Call handleLegacyClassError4(ErrNumber, ErrSource, ErrDescription & hint, MethodName, True)
        End Sub
        '
        '=====================================================================================================
        '   Verify the integrety of a workflow authoring record
        '
        '       if EditRecord has no LiveRecord, attempt a restore
        '=====================================================================================================
        '
        Private Sub db_VerifyWorkflowAuthoringRecord(ByVal CSPointer As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-126" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            Dim ContentPointer As Integer
            '
            MethodName = "csv_VerifyWorkflowAuthoringRecord"
            '
            Exit Sub
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Sub
        '
        '
        '
        Private Function db_IsCSEOF(ByVal CSPointer As Integer) As Boolean
            On Error GoTo ErrorTrap 'Const Tn = "csv_IsCSEOF" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            If CSPointer <= 0 Then
                Call handleLegacyClassError4(ignoreInteger, "dll", "csv_IsCSEOF called with an invalid CSPointer", "csv_IsCSEOF", False)
            Else
                If useCSReadCacheMultiRow Then
                    With db_ContentSet(CSPointer)
                        db_IsCSEOF = (.readCacheRowPtr >= .readCacheRowCnt)
                    End With
                Else
                    db_IsCSEOF = db_ContentSet(CSPointer).ResultEOF
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("Unexpected exception"))
        End Function
        '
        '
        '
        Private Function db_IsCSBOF(ByVal CSPointer As Integer) As Boolean
            On Error GoTo ErrorTrap 'Const Tn = "csv_IsCSBOF" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            If CSPointer <= 0 Then
                Call handleLegacyClassError4(ignoreInteger, "dll", "csv_IsCSBOF called with an invalid CSPointer", "csv_IsCSBOF", False)
            Else
                db_IsCSBOF = (db_ContentSet(CSPointer).readCacheRowPtr < 0)
                ' ##### readCacheRowPtr no longer supported csv_IsCSBOF = (csv_ContentSet(CSPointer).readCacheRowPtr = 0) And (csv_ContentSet(CSPointer).PageNumber = 1)
            End If
            'csv_IsCSBOF = csv_ContentSet(CSPointer).RS.BOF
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_IsCSBOF", True)
        End Function
        '    '
        '    '========================================================================
        '    '   EncodeSQL
        '    '       encode a variable to go in an sql expression
        '    '       NOT supported
        '    '========================================================================
        '    '
        Public Function db_EncodeSQL(ByVal expression As Object, Optional ByVal fieldType As Integer = FieldTypeIdText) As String
            ' ##### removed to catch err<>0 problem on error resume next
            '
            Dim iFieldType As Integer
            Dim MethodName As String
            '
            MethodName = "EncodeSQL"
            '
            iFieldType = fieldType
            Select Case iFieldType
                Case FieldTypeIdBoolean
                    db_EncodeSQL = encodeSQLBoolean(EncodeBoolean(expression))
                Case FieldTypeIdCurrency, FieldTypeIdAutoIdIncrement, FieldTypeIdFloat, FieldTypeIdInteger, FieldTypeIdLookup, FieldTypeIdMemberSelect
                    db_EncodeSQL = encodeSQLNumber(expression)
                Case FieldTypeIdDate
                    db_EncodeSQL = encodeSQLDate(EncodeDate(expression))
                Case FieldTypeIdLongText, FieldTypeIdHTML
                    db_EncodeSQL = encodeSQLText(EncodeText(expression))
                Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdRedirect, FieldTypeIdManyToMany, FieldTypeIdText, FieldTypeIdFileTextPrivate, FieldTypeIdFileJavascript, FieldTypeIdFileXML, FieldTypeIdFileCSS, FieldTypeIdFileHTMLPrivate
                    db_EncodeSQL = encodeSQLText(EncodeText(expression))
                Case Else
                    db_EncodeSQL = encodeSQLText(EncodeText(expression))
                    On Error GoTo 0
                    Call Err.Raise(ignoreInteger, "dll", "Unknown Field Type [" & fieldType & "] used FieldTypeText.")
            End Select
            '
        End Function

        '
        '========================================================================
        '   encodeSQLText
        '========================================================================
        '
        Public Function encodeSQLText(ByVal expression As String) As String
            Dim returnString As String = ""
            If expression Is Nothing Then
                returnString = "null"
            Else
                returnString = EncodeText(expression)
                If returnString = "" Then
                    returnString = "null"
                Else
                    returnString = "'" & vbReplace(returnString, "'", "''") & "'"
                End If
            End If
            Return returnString
        End Function
        '
        '========================================================================
        '   encodeSQLDate
        '       encode a date variable to go in an sql expression
        '========================================================================
        '
        Public Function encodeSQLDate(ByVal expression As Date) As String
            Dim returnString As String = ""
            Dim expressionDate As Date = Date.MinValue
            'If expression Is Nothing Then
            '    returnString = "null"
            'ElseIf Not IsDate(expression) Then
            '    returnString = "null"
            'Else
            If IsDBNull(expression) Then
                returnString = "null"
            Else
                expressionDate = EncodeDate(expression)
                If (expressionDate = Date.MinValue) Then
                    returnString = "null"
                Else
                    returnString = "'" & Year(expressionDate) & Right("0" & Month(expressionDate), 2) & Right("0" & Day(expressionDate), 2) & " " & Right("0" & expressionDate.Hour, 2) & ":" & Right("0" & expressionDate.Minute, 2) & ":" & Right("0" & expressionDate.Second, 2) & ":" & Right("00" & expressionDate.Millisecond, 3) & "'"
                End If
            End If
            'End If
            Return returnString
        End Function
        '
        '========================================================================
        '   encodeSQLNumber
        '       encode a number variable to go in an sql expression
        '========================================================================
        '
        Public Function encodeSQLNumber(ByVal expression As Object) As String
            Dim returnString As String = ""
            Dim expressionNumber As Double = 0
            If expression Is Nothing Then
                returnString = "null"
            ElseIf VarType(expression) = vbBoolean Then
                If EncodeBoolean(expression) Then
                    returnString = SQLTrue
                Else
                    returnString = SQLFalse
                End If
            ElseIf Not vbIsNumeric(expression) Then
                returnString = "null"
            Else
                returnString = expression.ToString
            End If
            Return returnString
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
        End Function
        '
        '========================================================================
        '   encodeSQLBoolean
        '       encode a boolean variable to go in an sql expression
        '========================================================================
        '
        Public Function encodeSQLBoolean(ByVal ExpressionVariant As Boolean) As String
            '
            'Dim src As String
            '
            encodeSQLBoolean = SQLFalse
            If EncodeBoolean(ExpressionVariant) Then
                encodeSQLBoolean = SQLTrue
            End If
        End Function
        '
        '========================================================================
        ' ----- Create a filename for the Virtual Directory
        '   Do not allow spaces.
        '   no, see below (If the content supports authoring, the filename returned will be for the
        '   current authoring record.)
        '   1/9/2004:
        '       When you need a virtual filename, first take it from the record.
        '       If the record has no filename, or there is no record, call this.
        '       This routine creates a filename from the input directly (so if the record is an edit record,
        '       you have to supply the EditRecordID.) If no ID is present, it makes a random ID
        '========================================================================
        '
        Public Function db_GetVirtualFilename(ByVal ContentName As String, ByVal FieldName As String, ByVal RecordID As Integer, Optional ByVal OriginalFilename As String = "") As String
            On Error GoTo ErrorTrap 'Const Tn = "GetVirtualFilename" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim FieldPtr As Integer
            Dim fieldTypeId As Integer
            Dim TableName As String
            Dim DataSourceName As String
            'Dim MethodName As String
            Dim iOriginalFilename As String
            Dim ContentPointer As Integer
            Dim CDef As coreMetaDataClass.CDefClass
            'Dim arrayOfFields() As appServices_metaDataClass.CDefFieldClass
            '
            db_GetVirtualFilename = ""
            CDef = cpCore.metaData.getCdef(ContentName)
            '
            If CDef.Id = 0 Then
                '
                Call handleLegacyClassError3("csv_GetVirtualFilename", ("ContentName [" & ContentName & "] was not found"))
            Else
                TableName = CDef.ContentTableName
                '
                If TableName = "" Then
                    TableName = ContentName
                End If
                '
                iOriginalFilename = encodeEmptyText(OriginalFilename, "")
                '
                fieldTypeId = CDef.fields(FieldName.ToLower()).fieldTypeId
                '
                db_GetVirtualFilename = csv_GetVirtualFilenameByTable(TableName, FieldName, RecordID, iOriginalFilename, fieldTypeId)
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_GetVirtualFilename", True)
        End Function
        '
        '========================================================================
        ' Opens a csv_ContentSet with the Members of a group
        '   Returns and integer that points into the csv_ContentSet array
        '   If there was a problem, it returns -1
        '========================================================================
        '
        Public Function db_OpenCSGroupUsers(ByVal groupList As List(Of String), Optional ByVal sqlCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "csv_OpenCSGroupUsers" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim rightNow As Date
            Dim sqlRightNow As String
            Dim MemberCriteria As String
            Dim GroupCriteria As String
            Dim CS As Integer
            Dim GroupIDList As String
            Dim SortFieldArray() As String
            Dim OrderByClause As String
            Dim SortFieldPointer As Integer
            Dim FieldName As String
            Dim SQL As String
            Dim iActiveOnly As Boolean
            '
            iActiveOnly = ActiveOnly
            rightNow = DateTime.Now
            sqlRightNow = encodeSQLDate(rightNow)
            If PageNumber = 0 Then
                PageNumber = 1
            End If
            If PageSize = 0 Then
                PageSize = csv_DefaultPageSize
            End If
            '
            db_OpenCSGroupUsers = -1
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
                If iActiveOnly Then
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
                db_OpenCSGroupUsers = db_openCsSql_rev("default", SQL, PageSize, PageNumber)
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_OpenCSGroupUsers", True)
        End Function '
        '========================================================================
        ' Get a Contents Tableid from the ContentPointer
        '========================================================================
        '
        Public Function db_GetContentTableID(ByVal ContentName As String) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "GetContentTableID" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CDef As coreMetaDataClass.CDefClass
            Dim rs As DataTable
            Dim SQL As String
            '
            SQL = "select ContentTableID from ccContent where name=" & encodeSQLText(ContentName) & ";"
            rs = executeSql_getDataTable(SQL)
            If Not isDataTableOk(rs) Then
                cpCore.handleLegacyError2("cpCoreClass", "csv_GetContentTableID", cpCore.appConfig.name & ", Content [" & ContentName & "] was not found in ccContent table")
            Else
                db_GetContentTableID = EncodeInteger(rs.Rows(0).Item("ContentTableID"))
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("Unexpected exception"))
        End Function
        '
        '========================================================================
        ' csv_DeleteTableRecord
        '========================================================================
        '
        Public Sub db_DeleteTableRecord(ByVal DataSourceName As String, ByVal TableName As String, ByVal RecordID As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-031" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            '
            MethodName = "csv_DeleteTableRecord"
            '
            Call db_DeleteTableRecords(DataSourceName, TableName, "ID=" & RecordID)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Sub     '
        '==================================================================================================
        ' ----- Remove this record from all watch lists
        '       Mark MarkInactive if the content is being deleted. non-MarkInactive otherwise
        '==================================================================================================
        '
        Public Sub db_DeleteContentRules(ByVal ContentID As Integer, ByVal RecordID As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-099" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            'Dim IsSpiderDocsSupported As Boolean
            Dim SQL As String
            Dim CS As Integer
            Dim MethodName As String
            Dim ContentRecordKey As String
            Dim Criteria As String
            Dim ContentName As String
            Dim TableName As String
            Dim Link As String
            Dim CalendarEventID As Integer
            Dim SurveyQuestionID As Integer
            '
            MethodName = "csv_DeleteContentRules"
            '
            ' ----- remove all ContentWatchListRules (uncheck the watch lists in admin)
            '
            If (ContentID <= 0) Or (RecordID <= 0) Then
                '
                Call Err.Raise(ignoreInteger, "dll", "ContentID [" & ContentID & "] or RecordID [" & RecordID & "] where blank")
            Else
                ContentRecordKey = CStr(ContentID) & "." & CStr(RecordID)
                Criteria = "(ContentRecordKey=" & encodeSQLText(ContentRecordKey) & ")"
                ContentName = cpCore.metaData.getContentNameByID(ContentID)
                TableName = cpCore.metaData.getContentTablename(ContentName)
                '
                ' ----- Delete CalendarEventRules and CalendarEvents
                '
                If metaData_IsContentFieldSupported("calendar events", "ID") Then
                    Call deleteContentRecords("Calendar Events", Criteria)
                End If
                '
                ' ----- Delete ContentWatch
                '
                CS = csOpen("Content Watch", Criteria)
                Do While cs_Ok(CS)
                    Call db_DeleteCSRecord(CS)
                    db_csGoNext(CS)
                Loop
                Call cs_Close(CS)
                '
                ' ----- Table Specific rules
                '
                Select Case vbUCase(TableName)
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
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Sub
        '
        '========================================================================
        '   See csv_SaveCS - rename
        '========================================================================
        '
        Public Sub db_SaveCSRecord(ByVal CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False, Optional ByVal Blockcsv_ClearBake As Boolean = False)
            Call db_SaveCS(CSPointer, AsyncSave, Blockcsv_ClearBake)
        End Sub

        '
        '========================================================================
        ' ----- Get the SQL value for the true state of a boolean
        '========================================================================
        '
        Private Function db_GetSQLTrue(ByVal DataSourceName As String) As Integer
            db_GetSQLTrue = 1
            '            On Error GoTo ErrorTrap : 'Const Tn = "MethodName-045" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '            '
            '            Dim MethodName As String
            '            Dim Pointer As Integer
            '            '
            '            MethodName = "csv_GetSQLTrue"
            '            '
            '            Pointer = csv_GetDataSourcePointer(DataSourceName)
            '            If Pointer < 0 Then
            '                Call csv_HandleClassTrapError(ignoreInteger, "dll", "DataSource [" & DataSourceName & "] was not found", MethodName, True)
            '            Else
            '                Select Case DataSourceConnectionObjs(Pointer).Type
            '                    Case DataSourceTypeODBCAccess
            '                        csv_GetSQLTrue = -1
            '                    Case Else
            '                        csv_GetSQLTrue = 1
            '                End Select
            '            End If
            '            '
            '            Exit Function
            '            '
            '            ' ----- Error Trap
            '            '
            'ErrorTrap:
            '            Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function
        '        '
        '        '
        '        '
        '        Private Function csv_GetContentFieldPointer(ByVal ContentName As String, ByVal FieldName As String) As Integer
        '            On Error GoTo ErrorTrap 'Const Tn = "GetContentFieldPointer" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim MethodName As String
        '            Dim FieldCount As Integer
        '            ' converted array to dictionary - Dim FieldPointer As Integer
        '            Dim UcaseFieldName As String
        '            Dim CDef As appServices_metaDataClass.CDefClass
        '            Dim cdefPtr As Integer
        '            Dim Ptr As Integer
        '            Dim test As Integer
        '            Dim iFieldName As String
        '            '
        '            MethodName = "csv_GetContentFieldPointer"
        '            '
        '            csv_GetContentFieldPointer = -1
        '            iFieldName = Trim(FieldName)
        '            If (ContentName <> "") And (iFieldName <> "") Then
        '                cdefPtr = cpCore.metaData.metaCache.cdefContentNameIndex.getPtr(ContentName)
        '                If cdefPtr = -1 Then
        '                    Call cpCore.metaData.getCdef(ContentName)
        '                    cdefPtr = cpCore.metaData.metaCache.cdefContentNameIndex.getPtr(ContentName)
        '                End If
        '                FieldPointer = cpCore.metaData.metaCache.cdefContentFieldIndex(cdefPtr).getPtr(iFieldName)
        '                If FieldPointer = -1 Then
        '                    If Not cpCore.metaData.metaCache.cdefContentFieldIndexPopulated(cdefPtr) Then
        '                        With cpCore.metaData.metaCache.cdef(cdefPtr)
        '                            For Ptr = 0 To (.fields.Count - 1)
        '                                Dim arrayOfFields As appServices_metaDataClass.CDefFieldClass()
        '                                arrayOfFields = .fields
        '                                Call cpCore.metaData.metaCache.cdefContentFieldIndex(cdefPtr).setPtr(arrayOfFields(Ptr).Name, Ptr)
        '                                'Call properties.contentFieldPtrIndex(cdfptr).SetPointer(.Fields(Ptr).Name, Ptr)
        '                            Next
        '                        End With
        '                        cpCore.metaData.metaCache.cdefContentFieldIndexPopulated(cdefPtr) = True
        '                        FieldPointer = cpCore.metaData.metaCache.cdefContentFieldIndex(cdefPtr).getPtr(iFieldName)
        '                    Else
        '                        test = test
        '                    End If
        '                End If
        '            End If
        '            csv_GetContentFieldPointer = FieldPointer
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        '        End Function
        '
        '   Returns a 2-d array with the results from the csv_ContentSet
        '
        Public Function db_GetCSRows(ByVal CSPointer As Integer) As String(,)
            db_GetCSRows = db_GetCSRows2(CSPointer)
        End Function
        '
        ' try declaring the return as object() - an array holder for variants
        ' try setting up each call to return a variant, not an array of variants
        '
        Public Function db_GetCSRows2(ByVal CSPointer As Integer) As String(,)
            On Error GoTo ErrorTrap 'Const Tn = "csv_GetCSRows2" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            Dim rs As DataTable
            Dim hint As String
            Dim returnOK As Boolean
            '
            If useCSReadCacheMultiRow Then
                db_GetCSRows2 = db_ContentSet(CSPointer).readCache
            Else
                '
                'hint = "csv_GetCSRows2"
                returnOK = False
                If cs_Ok(CSPointer) Then
                    'hint = hint & ",10"
                    If isDataTableOk(db_ContentSet(CSPointer).dt) Then
                        'hint = hint & ",20"
                        On Error Resume Next
                        'Call csv_ContentSet(CSPointer).RS.MoveFirst()
                        '                If False Then
                        '                'If (Err.Number = 0) Then
                        '                    'hint = hint & ",30"
                        '                    csv_GetCSRows2 = csv_ContentSet(CSPointer).RS.GetRows(-1, 0)
                        '                    If Err.Number = 0 Then
                        '                        'hint = hint & ",40"
                        '                        returnOK = True
                        '                        'Call main_testPoint("csv_GetCSRows2, rows returned OK")
                        '                    Else
                        '                        'hint = hint & ",50 err=" & GetErrString()
                        '                        'Call main_testPoint("csv_GetCSRows2, Error returning rows from ContentSet, reopen RS, errString=[" & GetErrString() & "]")
                        '                    End If
                        '                End If
                        If Not returnOK Then
                            '
                            ' fallback, open the RS again
                            '
                            'hint = hint & ",60"
                            Err.Clear()
                            'Call main_testPoint("csv_GetCSRows2, problem with RS, try requerying")
                            On Error GoTo ErrorTrap
                            'hint = hint & ",70"
                            rs = executeSql_getDataTable(db_ContentSet(CSPointer).Source, db_ContentSet(CSPointer).DataSource)
                            If isDataTableOk(rs) Then
                                db_GetCSRows2 = convertDataTabletoArray(rs)
                            End If
                            Call closeDataTable(rs)
                            'RS = Nothing
                            'hint = hint & ",80"
                        End If
                    End If
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "unknownMethodNameLegacyCall" & " hint=" & hint, True)
        End Function
        '
        '   Returns the current row from csv_ContentSet
        '
        Public Function db_GetCSRow(ByVal CSPointer As Integer) As Object
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-119" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim Row() As String
            Dim ColumnPointer As Integer
            '
            If cs_Ok(CSPointer) Then
                With db_ContentSet(CSPointer)
                    ReDim Row(.ResultColumnCount)
                    If useCSReadCacheMultiRow Then
                        For ColumnPointer = 0 To .ResultColumnCount - 1
                            Row(ColumnPointer) = EncodeText(.readCache(ColumnPointer, .readCacheRowPtr))
                        Next
                    Else
                        For ColumnPointer = 0 To .ResultColumnCount - 1
                            Row(ColumnPointer) = EncodeText(.readCache(ColumnPointer, 0))
                        Next
                    End If
                    db_GetCSRow = Row
                End With
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_GetCSRow", True)
        End Function
        '
        '
        '
        Public Function db_GetCSRowCount(ByVal CSPointer As Integer) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "csv_GetCSRowCount" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            '
            MethodName = "csv_GetCSRowCount"
            '
            If CSPointer > 0 Then
                If useCSReadCacheMultiRow Then
                    db_GetCSRowCount = db_ContentSet(CSPointer).readCacheRowCnt
                Else
                    If db_ContentSet(CSPointer).IsOpen Then

                        db_GetCSRowCount = db_ContentSet(CSPointer).dt.Rows.Count
                    End If
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function
        '
        '   Returns a 1-d array with the results from the csv_ContentSet
        '
        Public Function db_GetCSRowFields(ByVal CSPointer As Integer) As Object
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-121" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            '
            MethodName = "csv_GetCSRowFields"
            '
            If Not cs_Ok(CSPointer) Then
                Call handleLegacyClassError4(ignoreInteger, "dll", "csv_ContentSet is not valid or End-of-File", MethodName, False, False)
            Else
                db_GetCSRowFields = db_ContentSet(CSPointer).fieldNames
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function
        '
        '========================================================================
        ' csv_DeleteTableRecord
        '========================================================================
        '
        Public Sub db_DeleteTableRecords(ByVal DataSourceName As String, ByVal TableName As String, ByVal Criteria As String)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-030" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            Dim SQL As String
            '
            MethodName = "csv_DeleteTableRecords"
            '
            ' ----- delete the content record
            '
            If Criteria = "" Then
                SQL = "DELETE FROM " & TableName & ";"
            Else
                SQL = "DELETE FROM " & TableName & " WHERE " & Criteria & ";"
            End If
            Call executeSql_getDataTable(SQL, DataSourceName)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Sub
        '
        '========================================================================
        '   return the content name of a csv_ContentSet
        '========================================================================
        '
        Public Function db_GetCSContentName(ByVal CSPointer As Integer) As String
            Try
                If CSPointer <> -1 Then
                    db_GetCSContentName = db_ContentSet(CSPointer).ContentName
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Function
        '
        '========================================================================
        ' ----- Get FieldDescritor from FieldType
        '========================================================================
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
                            'Call Err.Raise(ignoreString, "dll", "Unknown FieldType [" & fieldType & "]")
                        End If
                End Select
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex, "Unexpected exception")
            End Try
            Return returnFieldTypeName
        End Function
        '
        '========================================================================
        '   Returns a csv_ContentSet with records from the Definition that joins the
        '       current Definition at the field specified.
        '========================================================================
        '
        Public Function db_OpenCSJoin(ByVal CSPointer As Integer, ByVal FieldName As String) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-090" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim ContentPointer As Integer
            Dim FieldValueVariant As Object
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim FieldCount As Integer
            Dim LookupContentID As Integer
            Dim LookupContentName As String
            Dim CSLookup As Integer
            'Dim UcaseFieldName As String
            Dim fieldFound As Boolean
            Dim ContentName As String
            Dim RecordId As Integer
            Dim fieldTypeId As Integer
            Dim fieldLookupId As Integer
            Dim MethodName As String
            Dim CDef As coreMetaDataClass.CDefClass
            Dim CDefLookup As coreMetaDataClass.CDefClass
            '
            MethodName = "csv_OpenCSJoin"
            '
            ' ----- needs work. Go to fields table and get field definition
            '       then print accordingly
            '
            db_OpenCSJoin = -1
            If Not cs_Ok(CSPointer) Then
                '
                Call handleLegacyClassError3(MethodName, ("csv_ContentSet is empty or end of file"))
            Else
                '
                ' csv_ContentSet good
                '
                If Not db_ContentSet(CSPointer).Updateable Then
                    '
                    ' ----- csv_ContentSet is not updateable (created with an SQL statement)
                    '
                    Call handleLegacyClassError3(MethodName, ("This csv_ContentSet does not support csv_OpenCSJoin. It may have been created from a SQL statement, not a Content Definition."))
                Else
                    '
                    ' ----- csv_ContentSet is updateable
                    '
                    ContentName = db_ContentSet(CSPointer).ContentName
                    CDef = db_ContentSet(CSPointer).CDef
                    FieldValueVariant = db_GetCSField(CSPointer, FieldName)
                    If IsNull(FieldValueVariant) Or (Not CDef.fields.ContainsKey(FieldName.ToLower)) Then
                        '
                        ' ----- fieldname is not valid
                        '
                        Call handleLegacyClassError3(MethodName, ("The fieldname [" & FieldName & "] was not found in the current csv_ContentSet created from [ " & ContentName & " ]."))
                    Else
                        '
                        ' ----- Field is good
                        '
                        Dim field As coreMetaDataClass.CDefFieldClass = CDef.fields(FieldName.ToLower())

                        RecordId = cs_getInteger(CSPointer, "ID")
                        fieldTypeId = field.fieldTypeId
                        fieldLookupId = field.lookupContentID
                        If fieldTypeId <> FieldTypeIdLookup Then
                            '
                            ' ----- Wrong Field Type
                            '
                            Call handleLegacyClassError3(MethodName, ("csv_OpenCSJoin only supports Content Definition Fields set as 'Lookup' type. Field [ " & FieldName & " ] is not a 'Lookup'."))
                        ElseIf vbIsNumeric(FieldValueVariant) Then
                            '
                            '
                            '
                            If (fieldLookupId = 0) Then
                                '
                                ' ----- Content Definition for this Lookup was not found
                                '
                                Call handleLegacyClassError3(MethodName, "The Lookup Content Definition [" & fieldLookupId & "] for this field [" & FieldName & "] is not valid.")
                            Else
                                LookupContentName = cpCore.metaData.getContentNameByID(fieldLookupId)
                                db_OpenCSJoin = csOpen(LookupContentName, "ID=" & encodeSQLNumber(FieldValueVariant), "name", , , , , , 1)
                                'CDefLookup = appEnvironment.GetCDefByID(FieldLookupID)
                                'csv_OpenCSJoin = db_csOpen(CDefLookup.Name, "ID=" & encodeSQLNumber(FieldValueVariant), "name", , , , , , 1)
                            End If
                        End If
                    End If
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function

        Public Property sqlCommandTimeout() As Integer
            Get
                On Error GoTo ErrorTrap 'Const Tn = "MethodName-192" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
                '
                sqlCommandTimeout = db_SQLTimeout
                '
                Exit Property
                '
                ' ----- Error Trap
                '
ErrorTrap:
                Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "Get csv_SQLCommandTimeout", True)

            End Get
            Set(ByVal value As Integer)
                On Error GoTo ErrorTrap 'Const Tn = "MethodName-193" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
                '
                db_SQLTimeout = value
                '
                Exit Property
                '
                ' ----- Error Trap
                '
ErrorTrap:
                Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "Let csv_SQLCommandTimeout", True)

            End Set
        End Property
        '
        '=============================================================
        '
        '=============================================================
        '
        Public Function db_GetRecordIDByGuid(ByVal ContentName As String, ByVal RecordGuid As String) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "GetRecordIDByGuid" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CS As Integer
            '
            CS = csOpen(ContentName, "ccguid=" & encodeSQLText(RecordGuid), "ID", , , , , "ID")
            If cs_Ok(CS) Then
                db_GetRecordIDByGuid = EncodeInteger(db_GetCS(CS, "ID"))
            End If
            Call cs_Close(CS)

            Exit Function
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_GetRecordID", True)
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function db_GetContentRows(ByVal ContentName As String, Optional ByVal Criteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal MemberID As Integer = SystemMemberID, Optional ByVal WorkflowRenderingMode As Boolean = False, Optional ByVal WorkflowEditingMode As Boolean = False, Optional ByVal SelectFieldList As String = "", Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As String(,)
            Dim returnRows As String(,) = {}
            Try
                '
                Dim OldState As Boolean
                Dim CS As Integer
                Dim rs As DataTable
                Dim Rows() As Object
                '
                If useCSReadCacheMultiRow Then
                    CS = csOpen(ContentName, Criteria, SortFieldList, ActiveOnly, MemberID, WorkflowRenderingMode, WorkflowEditingMode, SelectFieldList, PageSize, PageNumber)
                    If cs_Ok(CS) Then
                        returnRows = db_ContentSet(CS).readCache
                    End If
                    Call cs_Close(CS)
                Else
                    '
                    ' Open the CS, but do not run the query yet
                    '
                    OldState = csv_OpenCSWithoutRecords
                    csv_OpenCSWithoutRecords = True
                    CS = csOpen(ContentName, Criteria, SortFieldList, ActiveOnly, MemberID, WorkflowRenderingMode, WorkflowEditingMode, SelectFieldList, PageSize, PageNumber)
                    csv_OpenCSWithoutRecords = OldState
                    '
                    If db_ContentSet(CS).Source <> "" Then
                        rs = executeSql_getDataTable(db_ContentSet(CS).Source, db_ContentSet(CS).DataSource)
                        'RS = executeSql(csv_ContentSet(CS).DataSource, csv_ContentSet(CS).Source)
                        If isDataTableOk(rs) Then
                            returnRows = convertDataTabletoArray(rs)
                        End If
                        Call closeDataTable(rs)
                    End If
                    Call cs_Close(CS)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnRows
        End Function
        '
        '
        '
        Public Sub db_SetCSRecordDefaults(ByVal CS As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "csv_SetCSRecordDefaults" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            'Dim Ptr As Integer
            Dim lookups() As String
            Dim UCaseDefaultValueText As String
            Dim LookupContentName As String
            'Dim CDef As appServices_metaDataClass.CDefClass
            Dim FieldPtr As Integer
            Dim FieldName As String
            Dim DefaultValueText As String
            '
            For Each keyValuePair As KeyValuePair(Of String, coreMetaDataClass.CDefFieldClass) In db_ContentSet(CS).CDef.fields
                Dim field As coreMetaDataClass.CDefFieldClass = keyValuePair.Value
                With field
                    FieldName = .nameLc
                    If (FieldName <> "") And (Not String.IsNullOrEmpty(.defaultValue)) Then
                        Select Case vbUCase(FieldName)
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
                                        DefaultValueText = EncodeText(.defaultValue)
                                        Call cs_set(CS, FieldName, "null")
                                        If DefaultValueText <> "" Then
                                            If .lookupContentID <> 0 Then
                                                LookupContentName = cpCore.metaData.getContentNameByID(.lookupContentID)
                                                If LookupContentName <> "" Then
                                                    Call cs_set(CS, FieldName, getRecordID(LookupContentName, DefaultValueText))
                                                End If
                                            ElseIf .lookupList <> "" Then
                                                UCaseDefaultValueText = vbUCase(DefaultValueText)
                                                lookups = Split(.lookupList, ",")
                                                For Ptr = 0 To UBound(lookups)
                                                    If UCaseDefaultValueText = vbUCase(lookups(Ptr)) Then
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
            '
            Exit Sub
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("Unexpected exception"))
        End Sub
        '
        ' This is a copy of the routine in cpCoreClass -- duplicated so I do not have to make a public until the interface is worked-out
        '
        Public Function db_GetSQLSelect(ByVal DataSourceName As String, ByVal From As String, Optional ByVal FieldList As String = "", Optional ByVal Where As String = "", Optional ByVal OrderBy As String = "", Optional ByVal GroupBy As String = "", Optional ByVal RecordLimit As Integer = 0) As String
            On Error GoTo ErrorTrap 'Const Tn = "csv_GetSQLSelect" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim SQL As String
            '
            Select Case db_GetDataSourceType(DataSourceName)
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
            db_GetSQLSelect = SQL
            '
            Exit Function
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_GetSQLSelect", True, False)
        End Function
        '
        '
        '
        Public Function db_GetSQLIndexList(ByVal DataSourceName As String, ByVal TableName As String) As String
            Dim returnList As String = ""
            Try
                Dim ts As coreMetaDataClass.tableSchemaClass
                ts = cpCore.metaData.getTableSchema(TableName, DataSourceName)
                If (Not ts Is Nothing) Then
                    For Each entry As String In ts.indexes
                        returnList &= "," & entry
                    Next
                    If returnList.Length > 0 Then
                        returnList = returnList.Substring(2)
                    End If
                End If
            Catch ex As Exception
                Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_GetSQLIndexList", True, False)
            End Try
            Return returnList
        End Function
        '
        '                                        dt = connSQL.GetSchema("Columns", {config.name, Nothing, TableName, Nothing})
        '
        Public Function getTableSchemaData(tableName As String) As DataTable
            Dim returnDt As New DataTable
            Try
                Dim connString As String = getConnectionStringADONET(cpCore.appConfig.name, "default")
                Using connSQL As New SqlConnection(connString)
                    connSQL.Open()
                    returnDt = connSQL.GetSchema("Tables", {cpCore.appConfig.name, Nothing, tableName, Nothing})
                End Using
            Catch ex As Exception
                Call handleLegacyClassError5(ex, System.Reflection.MethodBase.GetCurrentMethod.Name, "exception")
            End Try
            Return returnDt
        End Function
        '
        ' connSQL.GetSchema("Indexes", {config.name, Nothing, TableName, Nothing})
        '
        Public Function getColumnSchemaData(tableName As String) As DataTable
            Dim returnDt As New DataTable
            Try
                Dim connString As String = getConnectionStringADONET(cpCore.appConfig.name, "default")
                Using connSQL As New SqlConnection(connString)
                    connSQL.Open()
                    returnDt = connSQL.GetSchema("Columns", {cpCore.appConfig.name, Nothing, tableName, Nothing})
                End Using
            Catch ex As Exception
                Call handleLegacyClassError5(ex, System.Reflection.MethodBase.GetCurrentMethod.Name, "exception")
            End Try
            Return returnDt
        End Function
        '
        ' 
        '
        Public Function getIndexSchemaData(tableName As String) As DataTable
            Dim returnDt As New DataTable
            Try
                Dim connString As String = getConnectionStringADONET(cpCore.appConfig.name, "default")
                Using connSQL As New SqlConnection(connString)
                    connSQL.Open()
                    returnDt = connSQL.GetSchema("Indexes", {cpCore.appConfig.name, Nothing, tableName, Nothing})
                End Using
            Catch ex As Exception
                Call handleLegacyClassError5(ex, System.Reflection.MethodBase.GetCurrentMethod.Name, "exception")
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
        '    Dim returnStatus As applicationStatusEnum = applicationStatusEnum.ApplicationStatusLoading
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
        '                cpCore.appStatus = applicationStatusEnum.ApplicationStatusDbFoundButContentMetaMissing
        '            End If
        '            testDt.Dispose()
        '        Catch ex As Exception
        '            cpCore.appStatus = applicationStatusEnum.ApplicationStatusDbFoundButContentMetaMissing
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
        '            cpCore.appStatus = applicationStatusEnum.ApplicationStatusDbBad
        '        Else
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndRethrow(ex)
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
                If vbIsNumeric(nameIdOrGuid) Then
                    sqlCriteria = "id=" & encodeSQLNumber(CDbl(nameIdOrGuid))
                ElseIf cpCore.common_isGuid(nameIdOrGuid) Then
                    sqlCriteria = "ccGuid=" & encodeSQLText(nameIdOrGuid)
                Else
                    sqlCriteria = "name=" & encodeSQLText(nameIdOrGuid)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
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
                '
                Dim dt As DataTable
                '
                getDbContentID = 0
                dt = executeSql_getDataTable("Select ID from ccContent where name=" & encodeSQLText(ContentName))
                If dt.Rows.Count > 0 Then
                    getDbContentID = EncodeInteger(dt.Rows(0).Item("id"))
                End If
                dt.Dispose()
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnContentId
        End Function
        '
        '========================================================================
        ' Get a DataSource Name from its ID
        '   If failure, return an empty DataSourceType (.ID=0)
        '========================================================================
        '
        Public Function getDataSourceNameByID(DataSourceID As Integer) As String
            Dim returnDataSource As String = ""
            Try
                '
                Dim ptr As Integer
                '
                If dataSources.Length <= 0 Then
                    '
                    ' if none available, use default
                    '
                    returnDataSource = "Default"
                ElseIf DataSourceID <= 0 Then
                    '
                    ' compatibility, if datasourceid is not give, or default, make sourcedefault
                    '
                    returnDataSource = "Default"
                Else
                    For ptr = 0 To dataSources.Length - 1
                        If dataSources(ptr).Id = DataSourceID Then
                            Exit For
                        End If
                    Next
                    If (ptr >= dataSources.Length) Then
                        returnDataSource = "Default"
                        Throw New ApplicationException("Datasource ID [" & DataSourceID & "] was not found, the default datasource will be used")
                    Else
                        returnDataSource = dataSources(ptr).NameLower
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnDataSource
        End Function
        '
        '=============================================================================
        ' Imports the named table into the content system
        '   Note: ContentNames are unique, so you can not have the same name on different
        '   datasources, so the datasource here is ...
        '
        '   What if...
        '       - content is found on different datasource
        '       - content does not exist
        '=============================================================================
        '
        Public Sub db_CreateContentFromSQLTable(ByVal DataSourceName As String, ByVal TableName As String, ByVal ContentName As String)
            Try
                '
                Dim SQL As String
                Dim dtFields As DataTable
                Dim DateAddedString As String
                Dim CreateKeyString As String
                Dim ContentID As Integer
                Dim DataSourceID As Integer
                Dim ContentFieldFound As Boolean
                Dim ContentIsNew As Boolean             ' true if the content definition is being created
                Dim RecordID As Integer
                '
                '----------------------------------------------------------------
                ' ----- lookup datasource ID, if default, ID is -1
                '----------------------------------------------------------------
                '
                DataSourceID = cpCore.db.db_GetDataSourceID(DataSourceName)
                DateAddedString = cpCore.db.encodeSQLDate(Now())
                CreateKeyString = cpCore.db.encodeSQLNumber(getRandomLong)
                '
                '----------------------------------------------------------------
                ' ----- Read in a record from the table to get fields
                '----------------------------------------------------------------
                '
                Dim rsTable As DataTable
                rsTable = cpCore.db.db_openTable(DataSourceName, TableName, "", "", , 1)
                If True Then
                    If rsTable.Rows.Count = 0 Then
                        '
                        ' --- no records were found, add a blank if we can
                        '
                        rsTable = cpCore.db.db_InsertTableRecordGetDataTable(DataSourceName, TableName, cpCore.user.id)
                        If rsTable.Rows.Count > 0 Then
                            RecordID = EncodeInteger(rsTable.Rows(0).Item("ID"))
                            Call cpCore.db.executeSql_getDataTable("Update " & TableName & " Set active=0 where id=" & RecordID & ";", DataSourceName)
                        End If
                    End If
                    If rsTable.Rows.Count = 0 Then
                        '
                        Call cpCore.handleExceptionAndRethrow(New ApplicationException("Could Not add a record To table [" & TableName & "]."))
                    Else
                        '
                        '----------------------------------------------------------------
                        ' --- Find/Create the Content Definition
                        '----------------------------------------------------------------
                        '
                        ContentID = cpCore.db.db_GetContentID(ContentName)
                        If (ContentID < 0) Then
                            '
                            ' ----- Content definition not found, create it
                            '
                            ContentIsNew = True
                            Call cpCore.metaData.metaData_CreateContent4(True, DataSourceName, TableName, ContentName)
                            'ContentID = csv_GetContentID(ContentName)
                            SQL = "Select ID from ccContent where name=" & cpCore.db.encodeSQLText(ContentName)
                            Dim rsContent As DataTable
                            rsContent = cpCore.db.executeSql_getDataTable(SQL)
                            If rsContent.Rows.Count = 0 Then
                                Call cpCore.handleExceptionAndRethrow(New ApplicationException("Content Definition [" & ContentName & "] could Not be selected by name after it was inserted"))
                            Else
                                ContentID = EncodeInteger(rsContent(0).Item("ID"))
                                Call cpCore.db.executeSql_getDataTable("update ccContent Set CreateKey=0 where id=" & ContentID)
                            End If
                            rsContent = Nothing
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
                        dtFields = cpCore.db.executeSql_getDataTable(SQL)
                        '
                        ' ----- verify all the table fields
                        '
                        For Each dcTableColumns As DataColumn In rsTable.Columns
                            '
                            ' ----- see if the field is already in the content fields
                            '
                            Dim UcaseTableColumnName As String
                            UcaseTableColumnName = vbUCase(dcTableColumns.ColumnName)
                            ContentFieldFound = False
                            For Each drContentRecords As DataRow In dtFields.Rows
                                If vbUCase(EncodeText(drContentRecords("name"))) = UcaseTableColumnName Then
                                    ContentFieldFound = True
                                    Exit For
                                End If
                            Next
                            If Not ContentFieldFound Then
                                '
                                ' create the content field
                                '
                                Call db_CreateContentFieldFromTableField(ContentName, dcTableColumns.ColumnName, EncodeInteger(dcTableColumns.DataType))
                            Else
                                '
                                ' touch field so upgrade does not delete it
                                '
                                Call cpCore.db.executeSql_getDataTable("update ccFields Set CreateKey=0 where (Contentid=" & ContentID & ") And (name = " & cpCore.db.encodeSQLText(UcaseTableColumnName) & ")")
                            End If
                        Next
                    End If
                End If
                '
                ' Fill ContentControlID fields with new ContentID
                '
                SQL = "Update " & TableName & " Set ContentControlID=" & ContentID & " where (ContentControlID Is null);"
                Call cpCore.db.executeSql_getDataTable(SQL, DataSourceName)
                '
                ' ----- Load CDef
                '       Load only if the previous state of autoload was true
                '       Leave Autoload false during load so more do not trigger
                '
                cpCore.cache.invalidateAll()
                cpCore.metaData.clear()
                rsTable = Nothing
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '========================================================================
        ' Define a Content Definition Field based only on what is known from a SQL table
        '========================================================================
        '
        Public Sub db_CreateContentFieldFromTableField(ByVal ContentName As String, ByVal FieldName As String, ByVal ADOFieldType As Integer)
            Try
                '
                Dim field As New coreMetaDataClass.CDefFieldClass
                '
                field.fieldTypeId = cpCore.db.db_GetFieldTypeIdByADOType(ADOFieldType)
                field.caption = FieldName
                field.editSortPriority = 1000
                field.ReadOnly = False
                field.authorable = True
                field.adminOnly = False
                field.developerOnly = False
                field.TextBuffered = False
                field.htmlContent = False
                '
                Select Case vbUCase(FieldName)
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
                        field.lookupContentName = "Members"
                        field.ReadOnly = True
                        field.editSortPriority = 5030
                    Case "MODIFIEDDATE"
                        field.caption = "Modified"
                        field.ReadOnly = True
                        field.editSortPriority = 5040
                    Case "MODIFIEDBY"
                        field.caption = "Modified By"
                        field.fieldTypeId = FieldTypeIdLookup
                        field.lookupContentName = "Members"
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
                        field.lookupContentName = "Content"
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
                        field.lookupContentName = "Organizations"
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
                    Case "DOCFILENAME"
                        field.caption = "Download Document"
                        field.fieldTypeId = FieldTypeIdFile
                        field.editSortPriority = 2030
                    Case "DOCLABEL"
                        field.caption = "Download Label"
                        field.editSortPriority = 2035
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
                        field.lookupContentName = "Content"
                        field.ReadOnly = False
                        field.editSortPriority = 2060
                    '
                    ' --- Record Features
                    '
                    Case "PARENTID"
                        field.caption = "Parent"
                        field.fieldTypeId = FieldTypeIdLookup
                        field.lookupContentName = ContentName
                        field.ReadOnly = False
                        field.editSortPriority = 3000
                    Case "MEMBERID"
                        field.caption = "Member"
                        field.fieldTypeId = FieldTypeIdLookup
                        field.lookupContentName = "Members"
                        field.ReadOnly = False
                        field.editSortPriority = 3005
                    Case "CONTACTMEMBERID"
                        field.caption = "Contact"
                        field.fieldTypeId = FieldTypeIdLookup
                        field.lookupContentName = "Members"
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
                Call cpCore.metaData.metaData_VerifyCDefField_ReturnID(ContentName, field)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
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
                    If csv_ContentSetCount > 0 Then
                        Dim CSPointer As Integer
                        For CSPointer = 1 To csv_ContentSetCount
                            If db_ContentSet(CSPointer).IsOpen Then
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

#Region "handleLegacyErrors"
        '
        '==========================================================================================
        ''' <summary>
        ''' handle legacy error for this class, v1
        ''' </summary>
        ''' <param name="MethodName"></param>
        ''' <param name="Cause"></param>
        ''' <remarks></remarks>
        Private Sub handleLegacyClassError1(ByVal MethodName As String, ByVal Cause As String)
            cpCore.handleLegacyError("appServicesClass", MethodName & ", error cause [" & Cause & "]", Err.Number, Err.Source, Err.Description, True, False, "")
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' handle legacy error for this class, v2
        ''' </summary>
        ''' <param name="MethodName"></param>
        ''' <param name="Cause"></param>
        ''' <remarks></remarks>
        Private Sub handleLegacyClassError2(ByVal MethodName As String, ByVal Cause As String)
            cpCore.handleLegacyError("appServicesClass", MethodName & ", error cause [" & Cause & "]", Err.Number, Err.Source, Err.Description, True, True, "")
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' handle legacy error for this class, v3
        ''' </summary>
        ''' <param name="MethodName"></param>
        ''' <param name="Cause"></param>
        ''' <remarks></remarks>
        Private Sub handleLegacyClassError3(ByVal MethodName As String, ByVal Cause As String)
            cpCore.handleLegacyError("appServicesClass", MethodName & ", error cause [" & Cause & "]", Err.Number, Err.Source, Err.Description, True, False, "")
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' handle legacy error for this class, v
        ''' </summary>
        ''' <param name="ignore1"></param>
        ''' <param name="ignore2"></param>
        ''' <param name="cause"></param>
        ''' <param name="MethodName"></param>
        ''' <param name="ignore"></param>
        ''' <param name="ignore3"></param>
        ''' <remarks></remarks>
        Private Sub handleLegacyClassError4(ByVal ignore1 As Integer, ByVal ignore2 As String, ByVal cause As String, ByVal MethodName As String, ByVal ignore As Boolean, Optional ByVal ignore3 As Boolean = False)
            Call handleLegacyClassError1(MethodName, cause)
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' handle legacy error for this class, v5
        ''' </summary>
        ''' <param name="ex"></param>
        ''' <param name="methodName"></param>
        ''' <param name="cause"></param>
        ''' <remarks></remarks>
        Private Sub handleLegacyClassError5(ByVal ex As Exception, ByVal methodName As String, ByVal cause As String)
            Call cpCore.handleExceptionLegacyRow2(ex, "appServicesClass", methodName, cause)
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

