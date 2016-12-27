
Option Explicit On
Option Strict On

Imports System.Data.SqlClient
Imports System.Text.RegularExpressions

Namespace Contensive.Core
    Public Class coreAppClass
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
        Private cpCore As cpCoreClass
        '
        '===================================================================================================
        ''' <summary>
        ''' returns the cache object.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property cache() As coreCacheClass
            Get
                If (_cache Is Nothing) Then
                    _cache = New coreCacheClass(cpCore)
                End If
                Return _cache
            End Get
        End Property
        Private _cache As coreCacheClass = Nothing
        '
        '------------------------------------------------------------------------------------------------------------------------
        ' objects created within class to dispose in dispose
        '   typically used as app.meaData.method()
        '------------------------------------------------------------------------------------------------------------------------
        '
        Friend ReadOnly Property metaData As coreMetaDataClass
            Get
                If _metaData Is Nothing Then
                    _metaData = New coreMetaDataClass(cpCore)
                End If
                Return _metaData
            End Get
        End Property
        Private _metaData As coreMetaDataClass = Nothing
        '
        '------------------------------------------------------------------------------------------------------------------------
        ' internal storage
        '   these objects is deserialized during constructor
        '   appConfig has static setup values like file system endpoints and Db connection string
        '   appProperties has dynamic values and is serialized and saved when changed
        '   include properties saved in appConfig file, settings not editable online.
        '------------------------------------------------------------------------------------------------------------------------
        '
        Private constructed As Boolean = False                                  ' set true when contructor is finished 
        'Private cacheEnabled As Boolean = True                                 ' set true when configured and tested - if false all cache calls disabled
        '
        ' on Db success, verified set true. If error and not verified, a simple test is run. on failure, Db disabled 
        '
        Private dbVerified As Boolean = False                                  ' set true when configured and tested - else db calls are skipped
        Private dbEnabled As Boolean = True                                    ' set true when configured and tested - else db calls are skipped
        '
        Public Const csv_DefaultPageSize = 9999
        Private Const csv_AllowAutocsv_ClearContentTimeStamp = True
        '
        ' Private structures that can stay until this class is converted
        '
        Public Const useCSReadCacheMultiRow = True
        '
        Const csv_AllowWorkflowErrors = False
        '
        '-----------------------------------------------------------------------------------------------
        '
        Public Property config As appConfigClass
        Public Property status As applicationStatusEnum
        '
        ' file systems
        '
        Public Property serverFiles As coreFileSystemClass           ' files written directly to the local server
        Public Property appRootFiles As coreFileSystemClass         ' wwwRoot path for the app server, both local and scale modes
        Public Property privateFiles As coreFileSystemClass         ' path not available to web interface, local: physcial storage location, scale mode mirror location
        Public Property cdnFiles As coreFileSystemClass             ' file uploads etc. Local mode this should point to appRoot folder (or a virtual folder in appRoot). Scale mode it goes to an s3 mirror
        '
        '------------------------------------------------------------------------------------------------------------------------
        ' simple lazy cached values
        '------------------------------------------------------------------------------------------------------------------------
        '
        Private dataBuildVersion_Local As String
        Friend dataBuildVersion_LocalLoaded As Boolean = False
        '
        Private siteProperty_allowLinkAlias_Local As Boolean
        Private siteProperty_allowLinkAlias_LocalLoaded As Boolean = False
        '
        ' Site Property Buffer
        '
        Private siteProperty_ChildListAddonID_Local As Integer
        Private siteProperty_ChildListAddonID_LocalLoaded As Boolean = False
        '
        Private siteProperty_DocTypeAdmin_Local As String
        Private siteProperty_DocTypeAdmin_LocalLoaded As Boolean = False
        '
        Private siteProperty_DocType_Local As String
        Private siteProperty_DocType_LocalLoaded As Boolean = False
        '
        Private siteProperty_DefaultWrapperID_local As Integer
        Private siteProperty_DefaultWrapperID_LocalLoaded As Boolean = False
        '
        Private siteProperty_UseContentWatchLink_local As Boolean
        Private siteProperty_UseContentWatchLink_LocalLoaded As Boolean = False
        '
        Private siteProperty_AllowTemplateLinkVerification_Local As Boolean
        Private siteProperty_AllowTemplateLinkVerification_LocalLoaded As Boolean = False
        '
        Private siteProperty_AllowTestPointLogging_Local As Boolean
        Private siteProperty_AllowTestPointLogging_LocalLoaded As Boolean = False
        '
        Private siteProperty_DefaultFormInputWidth_Local As Integer
        Private siteProperty_DefaultFormInputWidth_LocalLoaded As Boolean = False
        '
        Private siteProperty_SelectFieldWidthLimit_Local As Integer
        Private siteProperty_SelectFieldWidthLimit_LocalLoaded As Boolean = False
        '
        Private siteProperty_SelectFieldLimit_Local As Integer
        Private siteProperty_SelectFieldLimit_LocalLoaded As Boolean
        '
        Private siteProperty_DefaultFormInputTextHeight_Local As Integer
        Private siteProperty_DefaultFormInputTextHeight_LocalLoaded As Boolean
        '
        Private siteProperty_EmailAdmin_Local As String
        Private siteProperty_EmailAdmin_LocalLoaded As Boolean = False
        '
        Private siteProperty_Language_Local As String
        Private siteProperty_Language_LocalLoaded As Boolean = False
        '
        Private siteProperty_AdminURL_Local As String
        Private siteProperty_AdminURL_LocalLoaded As Boolean = False
        '
        Private siteProperty_CalendarYearLimit_Local As Integer
        Private siteProperty_CalendarYearLimit_LocalLoaded As Boolean = False
        ''
        '
        Private siteProperty_DefaultFormInputHTMLHeight_Local As Integer
        Private siteProperty_DefaultFormInputHTMLHeight_LocalLoaded As Boolean = False
        '
        Private siteProperty_AllowWorkflowAuthoring_Local As Boolean
        Private siteProperty_AllowWorkflowAuthoring_LocalLoaded As Boolean = False
        '
        Private siteProperty_AllowPathBlocking_Local As Boolean
        Private siteProperty_AllowPathBlocking_LocalLoaded As Boolean = False
        ''
        Private csv_ContentSet() As ContentSetType2
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
        'Public LoadContentEngineInProcess As Boolean    ' delays CDef calls while loading CDef cache.
        ' Set true in ccCsvrv LoadContentDefinition
        ' Checked here, waits for up to 5 seconds
        '
        ' Tokens - TakeTokens returns true, if already taken, reutrns false. When done, ReturnTokens
        '
        Private Tokens() As String
        Private TokenSize As Integer
        Private TokenCount As Integer
        '
        ' keepers
        '
        Public HitCounter As Integer
        Public StatusPauseExpiration As Date            ' if paused, the application will resume automatically after this time
        Public Progress As String                       ' Use for long commands to show movement (like upgrade)
        Public PhysicalFilePath As String
        Public PhysicalWWWPath As String
        Public DefaultConnectionString As String
        Public LicenseKey As String
        Public DomainName As String
        Public RootWebPath As String
        Public ConnectionsActive As Integer
        'Public ConnectionHandleCount As Integer            ' The connection handles created
        Public ErrorCount As Integer                       ' Errors since last start
        Public QueryStringExcludeList As String       ' Strings that will be excluded from spider and contentwatch
        Public AbortActivity As Boolean                 ' Signal to stop all activity for a shutdown
        '
        '   SQL Timeouts
        '
        Public csv_SQLTimeout As Integer
        Public csv_SlowSQLThreshholdMSec As Integer        '
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
        ''
        'Private cdefServices.CDefName() As String
        'Private cdefServices.CDefID() As Integer
        '
        Private Structure EditLockType
            Public Key As String
            Public MemberID As Integer
            Public DateExpires As Date
        End Structure
        '
        Private EditLockArray() As EditLockType
        Private EditLockCount As Integer
        Private AbuseCheckBuffer As String
        Public AbuseCheckLimit As Integer
        Public AbuseCheckPeriod As Integer
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
        ''' <param name="cluster"></param>
        ''' <param name="appName"></param>
        ''' <remarks></remarks>
        Friend Sub New(cpCore As cpCoreClass, ByVal appName As String)
            MyBase.New()
            Try
                '
                ' called during core constructor - so cpcore is not valid
                '   read from cache and deserialize
                '   if not in cache, build it from scratch
                '   eventually, setup public properties as indivisual lazyCache
                '
                Me.cpCore = cpCore
                status = applicationStatusEnum.ApplicationStatusLoading
                ReDim dataSources(0)
                dataSources(0) = New dataSourceClass()
                '
                If (cpCore.cluster Is Nothing) Then
                    '
                    ' cannot continue with the cluster created
                    '
                    Throw New ApplicationException("appServices constructor failed because clusterServices are not valid.")
                Else
                    Dim propertyValue As String = ""
                    Dim needToLoadCdefCache As Boolean = True
                    '
                    csv_SQLTimeout = 30
                    csv_SlowSQLThreshholdMSec = 1000
                    DomainName = "www.DomainName.com"
                    RootWebPath = "/"
                    AllowMonitoring = False
                    LicenseKey = GetSiteLicenseKey()
                    If (Not cpCore.cluster.config.apps.ContainsKey(appName.ToLower())) Then
                        '
                        ' application now configured
                        '
                        config = New appConfigClass()
                        status = applicationStatusEnum.ApplicationStatusAppConfigNotValid
                        Throw New Exception("application [" & appName & "] was not found in this cluster.")
                    Else
                        config = cpCore.cluster.config.apps(appName.ToLower())
                    End If
                    '
                    If InStr(1, config.domainList(0), ",") > 1 Then
                        '
                        ' if first entry in domain list is comma delimited, save only the first entry
                        '
                        config.domainList(0) = Mid(config.domainList(0), 1, InStr(1, config.domainList(0), ",") - 1)
                    End If
                    '
                    ' initialie filesystem, public and rivate now, setup virtual when site property is available
                    '
                    If cpCore.cluster.config.isLocal Then
                        '
                        ' local server -- everything is ephemeral
                        '
                        serverFiles = New coreFileSystemClass(cpCore, cpCore.cluster.config, coreFileSystemClass.fileSyncModeEnum.noSync, "")
                        appRootFiles = New coreFileSystemClass(cpCore, cpCore.cluster.config, coreFileSystemClass.fileSyncModeEnum.noSync, cpCore.cluster.config.clusterPhysicalPath & config.appRootPath)
                        privateFiles = New coreFileSystemClass(cpCore, cpCore.cluster.config, coreFileSystemClass.fileSyncModeEnum.noSync, cpCore.cluster.config.clusterPhysicalPath & config.privateFilesPath)
                        cdnFiles = New coreFileSystemClass(cpCore, cpCore.cluster.config, coreFileSystemClass.fileSyncModeEnum.noSync, cpCore.cluster.config.clusterPhysicalPath & config.cdnFilesPath)
                    Else
                        '
                        ' cluster mode - each filesystem is configured accordingly
                        '
                        serverFiles = New coreFileSystemClass(cpCore, cpCore.cluster.config, coreFileSystemClass.fileSyncModeEnum.noSync, "")
                        appRootFiles = New coreFileSystemClass(cpCore, cpCore.cluster.config, coreFileSystemClass.fileSyncModeEnum.activeSync, cpCore.cluster.config.clusterPhysicalPath & config.appRootPath)
                        privateFiles = New coreFileSystemClass(cpCore, cpCore.cluster.config, coreFileSystemClass.fileSyncModeEnum.passiveSync, cpCore.cluster.config.clusterPhysicalPath & config.privateFilesPath)
                        cdnFiles = New coreFileSystemClass(cpCore, cpCore.cluster.config, coreFileSystemClass.fileSyncModeEnum.passiveSync, cpCore.cluster.config.clusterPhysicalPath & config.cdnFilesPath)
                    End If
                    '
                    ' initialize datasource
                    '
                    AddDataSource("Default", -1, DefaultConnectionString)
                    '
                    ' REFACTOR - this was removed because during debug is costs 300msec, and only helps case with small edge case of Db loss -- test that case for risks
                    '
                    status = applicationStatusEnum.ApplicationStatusReady
                    '
                    'If Not cpCore.cluster.checkDatabaseExists(appName) Then
                    '    '
                    '    ' database does not exist
                    '    '
                    '    status = applicationStatusEnum.ApplicationStatusDbNotFound
                    'Else
                    '    'Call metaData.load()
                    '    status = applicationStatusEnum.ApplicationStatusReady
                    'End If
                End If
                constructed = True
            Catch ex As Exception
                cpCore.handleException(ex)
                Throw (ex)
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
                    If Not (_metaData Is Nothing) Then
                        _metaData.Dispose()
                    End If
                    '
                    If Not (_cache Is Nothing) Then
                        Call _cache.Dispose()
                    End If
                    '
                    '
                    ' ----- Close all open csv_ContentSets, and make sure the RS is killed
                    '
                    If csv_ContentSetCount > 0 Then
                        Dim CSPointer As Integer
                        For CSPointer = 1 To csv_ContentSetCount
                            If csv_ContentSet(CSPointer).IsOpen Then
                                Call db_csClose(CSPointer)
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
        '
        '====================================================================================================
        ''' <summary>
        ''' returns the addon install object. It only has access to the current app, so there is no access protection.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property addonInstall() As coreAddonInstallClass
            Get
                If _addonInstall Is Nothing Then
                    _addonInstall = New coreAddonInstallClass(cpCore)
                End If
                Return _addonInstall
            End Get
        End Property
        '
        '========================================================================
        ' Add a DataSource to the DataSourceLocal
        '   Returns the AddDataSource
        '   If found, it updates
        '========================================================================
        '
        Public Function AddDataSource(ByVal DataSourceName As String, ByVal Id As Integer, ByVal ConnectionString As String) As Integer
            Dim returnPtr As Integer = 0
            Try
                Dim dataSourceNameLower As String
                '
                If dataSources.Length > 0 Then
                    '
                    ' Find it
                    '
                    dataSourceNameLower = DataSourceName.ToLower
                    For returnPtr = 0 To dataSources.Length - 1
                        If UCase(dataSources(returnPtr).NameLower) = dataSourceNameLower Then
                            Exit For
                        End If
                    Next
                End If
                '
                If returnPtr >= dataSources.Length Then
                    '
                    ' Add it if not found
                    '
                    returnPtr = dataSources.Length
                    ReDim Preserve dataSources(returnPtr)
                    dataSources(returnPtr) = New dataSourceClass
                End If
                With dataSources(returnPtr)
                    .NameLower = DataSourceName.ToLower
                    .Id = Id
                    .odbcConnectionString = ConnectionString
                    .password = ""
                    .dataSourceType = dataSourceTypeEnum.mySqlNative
                    .username = ""
                    .endPoint = ""
                End With
                '
            Catch ex As Exception
                handleLegacyClassError5(ex, "addDataSource", "trap")
            End Try
            Return returnPtr
        End Function
        '
        '========================================================================
        ' Get Data Source By Pointer
        '   If failure, return an empty DataSourceType (.ID=0)
        '========================================================================
        '
        Public Function GetDataSourceByPointer(ByVal DataSourcePointer As Integer) As dataSourceClass
            On Error GoTo ErrorTrap

            'Const Tn = "AppServicesClass.GetDataSourceByPointer" : Call logMethodEntry(Tn) : ''Dim th as integer : th = TimerTraceStart(Tn)

            '
            If DataSourcePointer >= 0 Then
                GetDataSourceByPointer = dataSources(DataSourcePointer)
            End If
            '

            'Call logMethodExit(Tn) : Call TimerTraceStop(th)

            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError1(config.name, "GetDataSourceByPointer")
        End Function
        '
        '========================================================================
        ' Get Data Source By ID
        '   If failure, return an empty DataSourceType (.ID=0)
        '========================================================================
        '
        Public Function GetDataSourceByID(ByVal DataSourceID As Integer) As dataSourceClass
            On Error GoTo ErrorTrap

            'Const Tn = "AppServicesClass.GetDataSourceByID" : Call logMethodEntry(Tn) : ''Dim th as integer : th = TimerTraceStart(Tn)

            '
            Dim DataSourcePointer As Integer
            '
            If dataSources.Length > 0 Then
                For DataSourcePointer = 0 To dataSources.Length - 1
                    If dataSources(DataSourcePointer).Id = DataSourceID Then
                        GetDataSourceByID = dataSources(dataSources.Length)
                        Exit For
                    End If
                    ''''DoEvents
                Next
            End If
            '

            'Call logMethodExit(Tn) : Call TimerTraceStop(th)

            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError1(config.name, "GetDataSourceByID")
        End Function
        '
        '========================================================================
        ' Get Data Source By Name
        '   If failure, return an empty DataSourceType (.ID=0)
        '========================================================================
        '
        Public Function GetDataSource(ByVal DataSourceName As String) As dataSourceClass
            Dim returnDataSource As New dataSourceClass
            Try

                Dim DataSourcePointer As Integer
                Dim lowerDataSourceName As String
                '
                If dataSources.Length > 0 Then
                    lowerDataSourceName = DataSourceName.ToLower
                    For DataSourcePointer = 0 To dataSources.Length - 1
                        If UCase(dataSources(DataSourcePointer).NameLower) = lowerDataSourceName Then
                            returnDataSource = dataSources(dataSources.Length)
                            Exit For
                        End If
                    Next
                End If
            Catch ex As Exception
                handleLegacyClassError5(ex, "getDataSource", "trap")
            End Try
            Return returnDataSource
        End Function
        '
        '========================================================================
        '   Increment HitCount
        '========================================================================
        '
        Public Sub IncrementHitCounter()
            On Error GoTo ErrorTrap

            'Const Tn = "AppServicesClass.IncrementHitCounter" : Call logMethodEntry(Tn) : ''Dim th as integer : th = TimerTraceStart(Tn)

            '
            HitCounter = HitCounter + 1
            '

            'Call logMethodExit(Tn) : Call TimerTraceStop(th)

            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError1(config.name, "IncrementHitCounter")
        End Sub
        '
        '========================================================================
        '   Increment HitCount
        '========================================================================
        '
        Public Sub IncrementErrorCount()
            ErrorCount += 1
        End Sub
        '        '
        '        '========================================================================
        '        '   Open a Connection
        '        '   Increments Active Connections
        '        '   Returns a connection handle
        '        '========================================================================
        '        '
        '        Public Function OpenConnection() As AppConnectionType
        '            On Error GoTo ErrorTrap
        '            Dim returnString As String

        '            'Const Tn = "AppServicesClass.OpenConnection" : Call logMethodEntry(Tn) : 'Dim th as integer
        '            '
        '            '-------------------------------------------------------------------------------
        '            ' Open TimerTrace if needed
        '            '-------------------------------------------------------------------------------
        '            '
        '            If InStr(1, "," & DomainName & ",", ",TRACE,", vbTextCompare) <> 0 Then
        '                Call TimerTraceOpen("TimerTrace_AppServicesClass_" & Name & "_" & ConnectionHandleCount & ".txt")
        '                th = TimerTraceStart(Tn)
        '            End If

        '            '
        '            'ConnectionsActive = ConnectionsActive + 1
        '            OpenConnection.metaCache.metaCache.cdefCount = CDefCount
        '            If CDefCount > 0 Then
        '                OpenConnection.CDefID = CDefID
        '                OpenConnection.CDefName = CDefName
        '            End If
        '            OpenConnection.ConnectionHandle = ConnectionHandleCount
        '            OpenConnection.DomainName = DomainName
        '            OpenConnection.PhysicalFilePath = PhysicalFilePath
        '            OpenConnection.QueryStringExcludeList = QueryStringExcludeList
        '            OpenConnection.RootPath = RootPath
        '            OpenConnection.status = status
        '            OpenConnection.URLEncoder = URLEncoder
        '            '
        '            Call GetSiteProperty("AllowTransactionLog", False, SystemMemberID, returnString)
        '            OpenConnection.AllowTransactionLog = EncodeBoolean(returnString)
        '            '
        '            '
        '            Call GetSiteProperty("AllowWorkflowAuthoring", False, SystemMemberID, returnString)
        '            OpenConnection.AllowWorkflowAuthoring = EncodeBoolean(returnString)
        '            '
        '            Call GetSiteProperty(siteproperty_serverPageDefault_name, siteproperty_serverPageDefault_defaultValue, SystemMemberID, returnString)
        '            OpenConnection.ServerPageDefault = EncodeText(returnString)
        '            '
        '            ConnectionHandleCount = ConnectionHandleCount + 1
        '            '

        '            'Call logMethodExit(Tn) : Call TimerTraceStop(th)

        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call HandleClassTrapError(config.Name, "OpenConnection")
        '        End Function
        '        '
        '        '========================================================================
        '        '   Close a Connection
        '        '   Decrements Active Connections
        '        '   Returns the number of open connections
        '        '========================================================================
        '        '
        '        Public Function CloseConnection() As Integer
        '            On Error GoTo ErrorTrap

        '            'Const Tn = "AppServicesClass.CloseConnection" : Call logMethodEntry(Tn) : ''Dim th as integer : th = TimerTraceStart(Tn)

        '            '
        '            ConnectionsActive = ConnectionsActive - 1
        '            CloseConnection = ConnectionsActive
        '            '

        '            'Call logMethodExit(Tn) : Call TimerTraceStop(th) : Call TimerTraceClose()

        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call HandleClassTrapError(config.Name, "CloseConnection")
        '        End Function
        '
        ''========================================================================
        '''' <summary>
        '''' return the last modified date for the records in this table 
        '''' </summary>
        '''' <param name="tag">A tag that represents the source of the data in a cache entry. When that data source is modifed, the tag can be used to invalidate the cache entries based on it.</param>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Private Function cache_getTagInvalidationDate(ByVal tag As String) As Date
        '    Dim returnTagInvalidationDate As Date = Date.MinValue
        '    Try
        '        'Dim cacheData As Object
        '        Dim cacheNameTagInvalidateDate As String = "tagInvalidationDate-" & tag
        '        Dim tableName As String = ""
        '        Dim sql As String
        '        '
        '        If config.enableCache Then
        '            If Not String.IsNullOrEmpty(tag) Then
        '                '
        '                ' get it from raw cache
        '                '
        '                returnTagInvalidationDate = EncodeDate(cache_readRaw(cacheNameTagInvalidateDate))
        '                'If (TypeOf cacheData Is Date) Then
        '                '    returnTagInvalidationDate = EncodeDate(cacheData)
        '                'End If
        '                If returnTagInvalidationDate = Date.MinValue Then
        '                    '
        '                    ' if tag is a content name, use the last modified date from Db
        '                    '
        '                    Try
        '                        sql = "select top 1 t.modifiedDate" _
        '                            & " From cccontent c" _
        '                            & " left join cctables t on t.id=c.contenttableId" _
        '                            & " where c.name=" & db_EncodeSQLText(tag) _
        '                            & " order by t.modifiedDate desc"
        '                        Using dt As DataTable = executeSql(sql)
        '                            If dt.Rows.Count > 0 Then
        '                                returnTagInvalidationDate = EncodeDate(dt.Rows(0).Item(0))
        '                            End If
        '                        End Using
        '                    Catch ex As Exception
        '                        '
        '                    End Try
        '                    If returnTagInvalidationDate = Date.MinValue Then
        '                        returnTagInvalidationDate = New Date(1990, 8, 7)
        '                    End If
        '                    cache_saveRaw(cacheNameTagInvalidateDate, returnTagInvalidationDate)
        '                End If
        '            End If
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        '    Return returnTagInvalidationDate
        'End Function
        ''
        ''========================================================================
        ''   a Content definitions TimeStamp
        ''========================================================================
        ''
        'Public Sub SetContentTimeStamp(ByVal ContentName As String)
        '    Try
        '        Dim cacheName As String = "tagInvalidationDate-" & ContentName
        '        '
        '        If Not String.IsNullOrEmpty(ContentName) Then
        '            '
        '            ' set the current time
        '            '
        '            cache_saveRaw(cacheName, Now.ToString)
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        'End Sub
        '
        '=================================================================================
        '
        '=================================================================================
        '
        Public Sub SetEditLock(ByVal ContentName As String, ByVal RecordID As Integer, ByVal MemberID As Integer, Optional ByVal ClearLock As Boolean = False)
            On Error GoTo ErrorTrap

            'Const Tn = "AppServicesClass.SetEditLock" : Call logMethodEntry(Tn) : ''Dim th as integer : th = TimerTraceStart(Tn)

            '
            Dim SourcePointer As Integer
            Dim EditLockDetailArray As Object
            Dim LockDateExpires As Date
            Dim LockContentName As String
            Dim LockRecordID As Integer
            Dim LockMemberID As Integer
            Dim EditLockKey As Integer
            Dim EditLockTimeoutMinutes As Double
            'Dim EditLockTimeoutDays As Double
            Dim DestinationBuffer As String
            Dim LockFound As Boolean
            Dim EditLockDateExpires As Date
            Dim SourceKey As String
            Dim SourceDateExpires As Date
            Dim DestinationPointer As Integer
            Dim StringBuffer As String
            Dim EditLockKey2 As String
            '
            If (ContentName <> "") And (RecordID <> 0) Then
                EditLockKey2 = UCase(ContentName & "," & CStr(RecordID))
                StringBuffer = siteProperty_getText("EditLockTimeout", "5")
                EditLockTimeoutMinutes = EncodeNumber(StringBuffer)
                'EditLockTimeoutDays = (EditLockTimeoutMinutes / 60 / 24)
                EditLockDateExpires = Now.AddMinutes(EditLockTimeoutMinutes)
                If EditLockCount > 0 Then
                    For SourcePointer = 0 To EditLockCount - 1
                        If AbortActivity Then
                            Exit For
                        End If
                        SourceKey = EditLockArray(SourcePointer).Key
                        SourceDateExpires = EditLockArray(SourcePointer).DateExpires
                        If SourceKey = EditLockKey2 Then
                            '
                            ' This edit lock was found
                            '
                            LockFound = True
                            If EditLockArray(SourcePointer).MemberID <> MemberID Then
                                '
                                ' This member did not create the lock, he can not change it either
                                '
                                If (SourcePointer <> DestinationPointer) Then
                                    EditLockArray(DestinationPointer).Key = SourceKey
                                    EditLockArray(DestinationPointer).MemberID = MemberID
                                    EditLockArray(DestinationPointer).DateExpires = SourceDateExpires
                                End If
                                DestinationPointer = DestinationPointer + 1
                            ElseIf Not ClearLock Then
                                '
                                ' This lock created by this member, he can change it
                                '
                                EditLockArray(DestinationPointer).Key = SourceKey
                                EditLockArray(DestinationPointer).MemberID = MemberID
                                EditLockArray(DestinationPointer).DateExpires = EditLockDateExpires
                                DestinationPointer = DestinationPointer + 1
                            End If
                        ElseIf (SourceDateExpires >= EditLockDateExpires) Then
                            '
                            ' Lock not expired, move it if needed
                            '
                            If (SourcePointer <> DestinationPointer) Then
                                EditLockArray(DestinationPointer).Key = SourceKey
                                EditLockArray(DestinationPointer).MemberID = MemberID
                                EditLockArray(DestinationPointer).DateExpires = SourceDateExpires
                            End If
                            DestinationPointer = DestinationPointer + 1
                        End If
                        '''DoEvents()
                    Next
                End If
                If (Not LockFound) And (Not ClearLock) Then
                    '
                    ' Lock not found, add it
                    '
                    If EditLockCount > 0 Then
                        If (DestinationPointer > UBound(EditLockArray)) Then
                            ReDim Preserve EditLockArray(DestinationPointer + 10)
                        End If
                    Else
                        ReDim Preserve EditLockArray(10)
                    End If
                    EditLockArray(DestinationPointer).Key = EditLockKey2
                    EditLockArray(DestinationPointer).MemberID = MemberID
                    EditLockArray(DestinationPointer).DateExpires = EditLockDateExpires
                    DestinationPointer = DestinationPointer + 1
                End If
                EditLockCount = DestinationPointer
            End If

            'Call logMethodExit(Tn) : Call TimerTraceStop(th)

            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError1(config.name, "SetEditLock")
        End Sub
        '
        '=================================================================================
        '   Returns true if this content/record is locked
        '       if true, the ReturnMemberID is the member that locked it
        '           and ReturnDteExpires is the date when it will be released
        '=================================================================================
        '
        Public Function GetEditLock(ByVal ContentName As String, ByVal RecordID As Integer, ByRef ReturnMemberID As Integer, ByRef ReturnDateExpires As Date) As Boolean
            On Error GoTo ErrorTrap

            'Const Tn = "AppServicesClass.GetEditLock" : Call logMethodEntry(Tn) : ''Dim th as integer : th = TimerTraceStart(Tn)

            '
            Dim SourcePointer As Integer
            Dim EditLockKey As Integer
            Dim EditLockKey2 As String
            Dim DateNow As Date
            '
            If (ContentName <> "") And (RecordID <> 0) And (EditLockCount > 0) Then
                EditLockKey2 = UCase(ContentName & "," & CStr(RecordID))
                DateNow = Now
                For SourcePointer = 0 To EditLockCount - 1
                    If AbortActivity Then
                        Exit For
                    End If
                    If (EditLockArray(SourcePointer).Key = EditLockKey2) Then
                        ReturnMemberID = EditLockArray(SourcePointer).MemberID
                        ReturnDateExpires = EditLockArray(SourcePointer).DateExpires
                        If ReturnDateExpires > Now() Then
                            GetEditLock = True
                        End If
                        Exit For
                    End If
                    '''DoEvents()
                Next
            End If

            'Call logMethodExit(Tn) : Call TimerTraceStop(th)

            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError1(config.name, "GetEditLock")
        End Function
        '        '
        '        '=================================================================================
        '        '   Returns true if the server is OK
        '        '=================================================================================
        '        '
        '        Public Function IsStoreOK() As Boolean
        '            On Error GoTo ErrorTrap

        '            'Const Tn = "AppServicesClass.IsStoreOK" : Call logMethodEntry(Tn) : ''Dim th as integer : th = TimerTraceStart(Tn)

        '            '
        '            Dim CDefPointer As Integer
        '            Dim Foundcount As Integer
        '            '
        '            ' Make sure we have Content, Content Fields, Tables, DataSources
        '            '
        '            If metaData.metaCache.cdefCount > 0 Then
        '                For CDefPointer = 0 To metaData.metaCache.cdefCount - 1
        '                    Select Case UCase(metaData.metaCache.cdef(CDefPointer).Name)
        '                        Case "CONTENT"
        '                            Foundcount = Foundcount + 1
        '                        Case "CONTENT FIELDS"
        '                            Foundcount = Foundcount + 1
        '                        Case "TABLES"
        '                            Foundcount = Foundcount + 1
        '                        Case "DATA SOURCES"
        '                            Foundcount = Foundcount + 1
        '                    End Select
        '                    '''DoEvents()
        '                Next
        '            End If
        '            IsStoreOK = (Foundcount = 4)
        '            '

        '            'Call logMethodExit(Tn) : Call TimerTraceStop(th)

        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call handleLegacyClassError1(config.name, "IsStoreOK")
        '        End Function
        '
        '=================================================================================
        '   Returns true if the token has not been taken, else it returns false
        '=================================================================================
        '
        Public Function TakeToken(ByVal Token As String) As Boolean
            On Error GoTo ErrorTrap

            'Const Tn = "AppServicesClass.TakeToken" : Call logMethodEntry(Tn) : ''Dim th as integer : th = TimerTraceStart(Tn)

            '
            Dim TokenPointer As Integer
            Dim UcaseToken As String
            '
            UcaseToken = UCase(Token)
            If TokenCount > 0 Then
                For TokenPointer = 0 To TokenCount - 1
                    If UcaseToken = Tokens(TokenPointer) Then
                        Exit For
                    End If
                Next
            End If
            If TokenPointer >= TokenCount Then
                If TokenCount >= TokenSize Then
                    TokenSize = TokenSize + 10
                    ReDim Preserve Tokens(TokenSize)
                End If
                Tokens(TokenCount) = UcaseToken
                TokenCount = TokenCount + 1
                TakeToken = True
            End If
            '

            'Call logMethodExit(Tn) : Call TimerTraceStop(th)

            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError1(config.name, "TakeToken")
        End Function
        '
        '=================================================================================
        '   Returns true if the token has not been taken, else it returns false
        '=================================================================================
        '
        Public Sub ReturnToken(ByVal Token As String)
            On Error GoTo ErrorTrap

            'Const Tn = "AppServicesClass.ReturnToken" : Call logMethodEntry(Tn) : ''Dim th as integer : th = TimerTraceStart(Tn)

            '
            Dim TokenPointer As Integer
            Dim UcaseToken As String
            '
            If TokenCount > 0 Then
                UcaseToken = UCase(Token)
                For TokenPointer = 0 To TokenCount - 1
                    If UcaseToken = Tokens(TokenPointer) Then
                        Tokens(TokenPointer) = ""
                        Exit For
                    End If
                    '''DoEvents()
                Next
            End If
            '

            'Call logMethodExit(Tn) : Call TimerTraceStop(th)

            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError1(config.name, "TakeToken")
        End Sub

        Public Property ConnectionString() As String
            Get
                ConnectionString = DefaultConnectionString
            End Get
            Set(ByVal value As String)
                DefaultConnectionString = value
            End Set
        End Property
        '
        ' execute sql offline
        '
        Public Sub executeSqlAsync(ByVal sql As String, Optional ByVal dataSourceName As String = "", Optional ByVal startRecord As Integer = 0, Optional ByVal maxRecords As Integer = 9999)
            Throw New NotImplementedException("executeSqlAsync not implemented yet")
        End Sub
        '
        ' execute sql on default connection and return datatable
        '
        Public Function executeSql(ByVal sql As String, Optional ByVal dataSourceName As String = "", Optional ByVal startRecord As Integer = 0, Optional ByVal maxRecords As Integer = 9999) As DataTable
            Dim returnData As New DataTable
            Dim connString As String = cpCore.cluster.getConnectionString(config.name)
            Try
                If dbEnabled Then
                    'Dim dataSourceUrl As String
                    'dataSourceUrl = cpCore.cluster.config.defaultDataSourceAddress
                    'If (dataSourceUrl.IndexOf(":") > 0) Then
                    '    dataSourceUrl = dataSourceUrl.Substring(0, dataSourceUrl.IndexOf(":"))
                    'End If
                    'connString = "" _
                    '    & "data source=" & dataSourceUrl & ";" _
                    '    & "initial catalog=" & config.name & ";" _
                    '    & "UID=" & cpCore.cluster.config.defaultDataSourceUsername & ";" _
                    '    & "PWD=" & cpCore.cluster.config.defaultDataSourcePassword & ";" _
                    '    & ""
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
                Dim newEx As New ApplicationException("Exception [" & ex.Message & "] executing sql [" & sql & "], datasource [" & dataSourceName & "], startRecord [" & startRecord & "], maxRecords [" & maxRecords & "]", ex)
                cpCore.handleException(newEx)
                Throw newEx
            End Try
            Return returnData
        End Function
        '
        '========================================================================
        ' ----- Set a value in the Setup array
        '       If its found, set it. If it changes, update the datasource
        '       If not found, create it and insert a datasource record
        '========================================================================
        '
        Public Sub siteProperty_set(ByVal propertyName As String, ByVal Value As String)
            Try
                Dim cacheName As String = "siteProperty-" & propertyName
                Dim RecordID As Integer
                Dim dt As DataTable
                Dim SQL As String
                Dim ContentID As Integer
                Dim SitePropertyResult As Integer
                Dim SQLNow As String = db_EncodeSQLDate(Now)

                RecordID = 0
                SQL = "SELECT ID FROM CCSETUP WHERE NAME=" & db_EncodeSQLText(propertyName) & " order by id"
                dt = executeSql(SQL)
                If dt.Rows.Count > 0 Then
                    RecordID = EncodeInteger(dt.Rows(0).Item("ID"))
                End If
                If RecordID <> 0 Then
                    SQL = "UPDATE ccSetup Set FieldValue=" & db_EncodeSQLText(Value) & ",ModifiedDate=" & SQLNow & " WHERE ID=" & RecordID
                    Call executeSql(SQL)
                Else
                    ' get contentId manually, getContentId call checks cache, which gets site property, which may set
                    ContentID = 0
                    SQL = "SELECT ID FROM cccontent WHERE NAME='site properties' order by id"
                    dt = executeSql(SQL)
                    If dt.Rows.Count > 0 Then
                        ContentID = EncodeInteger(dt.Rows(0).Item("ID"))
                    End If
                    'ContentID = csv_GetContentID("Site Properties")
                    SQL = "INSERT INTO ccSetup (ACTIVE,CONTENTCONTROLID,NAME,FIELDVALUE,ModifiedDate,DateAdded)VALUES(" & SQLTrue & "," & db_EncodeSQLNumber(ContentID) & "," & db_EncodeSQLText(UCase(propertyName)) & "," & db_EncodeSQLText(Value) & "," & SQLNow & "," & SQLNow & ");"
                    Call executeSql(SQL)
                End If
                Call cache.SetKey(cacheName, Value, "site properties")

            Catch ex As Exception
                Call cpCore.handleException(ex)
            End Try
        End Sub
        '
        Public Sub siteProperty_set(ByVal propertyName As String, ByVal Value As Boolean)
            siteProperty_set(propertyName, Value.ToString)
        End Sub
        '
        Public Sub siteProperty_set(ByVal propertyName As String, ByVal Value As Integer)
            siteProperty_set(propertyName, Value.ToString)
        End Sub
        '========================================================================
        ''' <summary>
        ''' get site property without a cache check, return as text. If not found, set and return default value
        ''' </summary>
        ''' <param name="PropertyName"></param>
        ''' <param name="DefaultValue"></param>
        ''' <param name="memberId"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function siteProperty_getText_noCache(ByVal PropertyName As String, ByVal DefaultValue As String, ByRef return_propertyFound As Boolean) As String
            Dim returnString As String
            Try
                Dim SQL As String
                Dim dt As DataTable

                SQL = "select FieldValue from ccSetup where name=" & db_EncodeSQLText(PropertyName) & " order by id"
                dt = executeSql(SQL)
                If dt.Rows.Count > 0 Then
                    returnString = EncodeText(dt.Rows(0).Item("FieldValue"))
                    return_propertyFound = True
                ElseIf (DefaultValue <> "") Then
                    ' do not set - set may have to save, and save needs contentId, which now loads ondemand, which checks cache, which does a getSiteProperty.
                    Call siteProperty_set(PropertyName, DefaultValue)
                    returnString = DefaultValue
                    return_propertyFound = True
                Else
                    returnString = ""
                    return_propertyFound = False
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnString
        End Function
        '========================================================================
        ''' <summary>
        ''' get site property, return as text. If not found, set and return default value
        ''' </summary>
        ''' <param name="PropertyName"></param>
        ''' <param name="DefaultValue"></param>
        ''' <param name="memberId"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function siteProperty_getText(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As String
            Dim returnString As String = ""
            Try
                Dim cacheName As String = "siteProperty-" & PropertyName
                Dim propertyFound As Boolean = False
                returnString = EncodeText(cache.GetObject(Of String)(cacheName))
                If returnString = "" Then
                    returnString = siteProperty_getText_noCache(PropertyName, DefaultValue, propertyFound)
                    If (propertyFound) And (returnString <> "") Then
                        Call cache.SetKey(cacheName, returnString, "Site Properties")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnString
        End Function
        '
        Public Function siteProperty_getText(ByVal PropertyName As String) As String
            Return siteProperty_getText(PropertyName, "")
        End Function
        '
        Public Function siteProperty_getinteger(ByVal PropertyName As String, Optional ByVal DefaultValue As Integer = 0) As Integer
            Return EncodeInteger(siteProperty_getText(PropertyName, DefaultValue.ToString))
        End Function
        '
        Public Function siteProperty_getBoolean(ByVal PropertyName As String, Optional ByVal DefaultValue As Boolean = False) As Boolean
            Return EncodeBoolean(siteProperty_getText(PropertyName, DefaultValue.ToString))
        End Function
        '
        '
        '
        Public ReadOnly Property dataBuildVersion As String
            Get
                Dim returnString = ""
                Try

                    If Not dataBuildVersion_LocalLoaded Then
                        dataBuildVersion_Local = siteProperty_getText("BuildVersion", "")
                        If dataBuildVersion_Local = "" Then
                            dataBuildVersion_Local = "0.0.000"
                        End If
                        dataBuildVersion_LocalLoaded = True
                    End If
                    returnString = dataBuildVersion_Local
                Catch ex As Exception
                    cpCore.handleException(ex)
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
        Public Function csv_GetDataSourcePointer(ByVal DataSourceName As String) As Integer
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
            csv_GetDataSourcePointer = 0
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
                csv_GetDataSourcePointer = DataSourcePointer
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError1(MethodName, "trap")
        End Function

        '
        '========================================================================
        ' Opens the datasource from the connection string
        '   Assumes all datasource info is loaded in the appServices Datasource object
        '
        '   If it opens OK, sets .open true
        '   if it can not open, .open is false and he error is reported
        '========================================================================
        '
        Friend Sub csv_OpenDataSource(ByVal DataSourceName As String, ByVal TimeoutSeconds As Integer)
            Throw New NotImplementedException("Datasources will not be cached")

            '            On Error GoTo ErrorTrap
            '            '
            '            Dim MethodName As String
            '            Dim RSSchema as datatable
            '            Dim Pointer As Integer
            '            Dim TablePointer As Integer
            '            Dim TableName As String
            '            Dim Conn As Connection
            '            Dim DataSourcePointer As Integer
            '            Dim hint As String
            '            '
            '            MethodName = "csv_OpenDataSource"
            '            '
            '            ' ----- Get DataSourcePointer
            '            '
            '            'cpCore.AppendLog("csv_OpenDataSource, enter, name=[" & DataSourceName & "]")
            '            'hint = "openDataSource, entry(" & GetTickCount() & ")"
            '            DataSourcePointer = csv_GetDataSourcePointer(DataSourceName)
            '            'cpCore.AppendLog("csv_OpenDataSource, DataSourcePointer=[" & DataSourcePointer & "]")
            '            ''hint = hint & ", a(" & GetTickCount() & ")"
            '            If DataSourcePointer = -1 Then
            '                '
            '                ' This is an error
            '                '
            '                Call csv_HandleClassErrorAndResume(MethodName, "Datasource was not foundError opening Data Source [" & DataSourceName & "], this datasource was not found.")
            '            ElseIf Not DataSourceConnectionObjs(DataSourcePointer).IsOpen Then
            '                '
            '                '   Create Connection Object
            '                '
            '                With DataSourceConnectionObjs(DataSourcePointer)
            '                    '
            '                    ' 2011-3-15 - added .close looking for memory leak
            '                    '
            '                    'cpCore.AppendLog("csv_OpenDataSource, create Connection object and open")
            '                    If Not (.Conn Is Nothing) Then
            '                        If .Conn.State = ObjectStateEnum.adStateOpen Then
            '                            .Conn.Close()
            '                        End If
            '                        .Conn = Nothing
            '                    End If
            '                    ''hint = hint & ", b(" & GetTickCount() & ")"
            '                    On Error Resume Next
            '                    .Conn = CreateObject("ADODB.Connection")
            '                    If Err.Number <> 0 Then
            '                        Call csv_HandleClassErrorAndResume(MethodName, "Error creating ADODB connection object for Datasource [" & DataSourceName & "]")
            '                    Else
            '                        ''hint = hint & ", c(" & GetTickCount() & ")"
            '                        On Error GoTo ErrorTrap
            '                        If (.Conn Is Nothing) Then
            '                            Call csv_HandleClassErrorAndResume(MethodName, "Datasource was created without error, but object is nothing for datasource [" & DataSourceName & "]")
            '                        Else
            '                            ''
            '                            '' Added because performance tests showed big improvement on NT - moved to within database type detect
            '                            '' so mysql could be client cursor
            '                            ''
            '                            '.Conn.CursorLocation = CursorLocationEnum.adUseServer
            '                            '
            '                            ' timeout
            '                            '
            '                            .Conn.CommandTimeout = TimeoutSeconds
            '                            .Conn.ConnectionTimeout = TimeoutSeconds
            '                            '
            '                            ''hint = hint & ", d(" & GetTickCount() & ")"
            '                            On Error Resume Next
            '                            .Conn.Open(dataSources(DataSourcePointer).ConnectionString)
            '                            If Err.Number <> 0 Then
            '                                Call csv_HandleClassErrorAndResume(MethodName, "Connection Open failed for Data Source [" & DataSourceName & "]")
            '                            Else
            '                                '
            '                                ''hint = hint & ", e(" & GetTickCount() & ")"
            '                                On Error GoTo ErrorTrap
            '                                If .Conn.State <> 1 Then
            '                                    Call csv_HandleClassErrorAndResume(MethodName, "Connection for " & DataSourceName & " opened in an incorrect state")
            '                                Else
            '                                    Select Case .Conn.Properties("DBMS Name")
            '                                        Case "Microsoft SQL Server"
            '                                            .Type = DataSourceTypeODBCSQLServer
            '                                            .DefaultCursorLocation = CursorLocationEnum.adUseServer
            '                                        Case "ACCESS"
            '                                            .Type = DataSourceTypeODBCAccess
            '                                            .DefaultCursorLocation = CursorLocationEnum.adUseServer
            '                                        Case "MySQL"
            '                                            .Type = DataSourceTypeODBCMySQL
            '                                            .DefaultCursorLocation = CursorLocationEnum.adUseClient
            '                                        Case Else
            '                                            .Type = DataSourceTypeODBCSQL99
            '                                            .DefaultCursorLocation = CursorLocationEnum.adUseServer
            '                                    End Select
            '                                    .IsOpen = True
            '                                End If
            '                            End If
            '                        End If
            '                    End If
            '                End With
            '            End If
            '            '
            '            Exit Sub
            '            '
            '            ' ----- Error Trap
            '            '
            'ErrorTrap:
            '            Call csv_HandleClassErrorAndResume(MethodName, "trap")
        End Sub
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
                cdef = metaData.getCdef(ContentName)
                If cdef Is Nothing Then
                    returnId = -1
                Else
                    returnId = cdef.Id
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnId
        End Function
        '
        '========================================================================
        '   converts a virtual file into a filename
        '       - in local mode, the cdnFiles can be mapped to a virtual folder in appRoot
        '           -- see appConfig.cdnFilesVirtualFolder
        '       convert all / to \
        '       if it includes "://", it is a root file
        '       if it starts with "/", it is already root relative
        '       else (if it start with a file or a path), add the publicFileContentPathPrefix
        '========================================================================
        '
        Public Function convertCdnUrlToCdnPathFilename(ByVal cdnUrl As String) As String
            '
            ' this routine was originally written to handle modes that were not adopted (content file absolute and relative URLs)
            ' leave it here as a simple slash converter in case other conversions are needed later
            '
            Return Replace(cdnUrl, "/", "\")

            'Dim returnFilename As String = cdnUrl
            'Dim Pos As Integer
            ''
            '' make sure Url is formatted correctly
            ''
            'returnFilename = Replace(returnFilename, "\", "/")
            ''
            '' convert
            ''
            'Pos = InStr(1, returnFilename, "://")
            'If (Pos <> 0) Then
            '    '
            '    ' Absolute URL - skip the domain and return root filename
            '    '
            '    Pos = InStr(Pos + 3, returnFilename, "/")
            '    If (Pos > 0) Then
            '        returnFilename = returnFilename.Substring(Pos)
            '    Else
            '        returnFilename = ""
            '    End If
            'ElseIf returnFilename.Substring(0, 1) = "/" Then
            '    '
            '    ' root path - get root filename
            '    '
            '    returnFilename = returnFilename.Substring(1)
            'Else
            '    '
            '    ' ok as it
            '    '
            '    returnFilename = returnFilename
            'End If
            'returnFilename = Replace(returnFilename, "/", "\")
            ''
            '' ????? This is a problem- addons being installed are using this before saving the jsfilename content, and the filename is starting with a "/" -- but this is wrong
            '' 20160929 - removed this. I think this was added when the cden and appRoot were going to be merged. Not the case now
            '' convertCdnUrlToCdnPathFilename = appRoot.joinPath(config.cdnFilesNetprefix, returnFilename)
            'Return returnFilename
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
                Call executeSql(SQL, DataSourceName)
            Catch ex As Exception
                cpCore.handleException(ex)
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
                cpCore.handleException(ex)
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
                CreateKeyString = db_EncodeSQLNumber(getRandomLong)
                DateAddedString = db_EncodeSQLDate(Now)
                '
                sqlList.add("createkey", CreateKeyString)
                sqlList.add("dateadded", DateAddedString)
                sqlList.add("createdby", db_EncodeSQLNumber(MemberID))
                sqlList.add("ModifiedDate", DateAddedString)
                sqlList.add("ModifiedBy", db_EncodeSQLNumber(MemberID))
                sqlList.add("EditSourceID", db_EncodeSQLNumber(0))
                sqlList.add("EditArchive", db_EncodeSQLNumber(0))
                sqlList.add("EditBlank", db_EncodeSQLNumber(0))
                sqlList.add("ContentControlID", db_EncodeSQLNumber(0))
                sqlList.add("Name", db_EncodeSQLText(""))
                sqlList.add("Active", db_EncodeSQLNumber(1))
                '
                Call db_InsertTableRecord(DataSourceName, TableName, sqlList)
                returnDt = db_openTable(DataSourceName, TableName, "(DateAdded=" & DateAddedString & ")and(CreateKey=" & CreateKeyString & ")", "ID DESC",  , 1)
            Catch ex As Exception
                cpCore.handleException(ex)
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
                    Call executeSql(sql, DataSourceName)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
            db_openTable = executeSql(SQL, DataSourceName, (PageNumber - 1) * PageSize, PageSize)
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
        Private Sub csv_SaveSlowQueryLog(ByVal TransactionTickCount As Integer, ByVal appname As String, ByVal SQL As String)
            cpCore.appendLogWithLegacyRow(appname, "query time  " & GetIntegerString(TransactionTickCount, 7) & "ms: " & SQL, "dll", "cpCoreClass", "csv_ExecuteSQL", 0, "", SQL, False, True, "", "Performance", "SlowSQL")
        End Sub

        '
        '
        '
        Private Sub csv_SaveTransactionLog(ByVal LogEntry As String)
            On Error GoTo ErrorTrap
            '
            Dim Message As String
            '
            If LogEntry <> "" Then
                Message = Replace(LogEntry, vbCr, "")
                Message = Replace(Message, vbLf, "")
                cpCore.appendLog(Message, "DbTransactions")
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
        Private Function csv_GetSQLWherePosition(ByVal SQL As String) As Integer
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
            csv_GetSQLWherePosition = 0
            If isInStr(1, SQL, "WHERE", vbTextCompare) Then
                '
                ' ----- contains the word "WHERE", now weed out if not a where clause
                '
                csv_GetSQLWherePosition = InStrRev(SQL, " WHERE ", , vbTextCompare)
                If csv_GetSQLWherePosition = 0 Then
                    csv_GetSQLWherePosition = InStrRev(SQL, ")WHERE ", , vbTextCompare)
                    If csv_GetSQLWherePosition = 0 Then
                        csv_GetSQLWherePosition = InStrRev(SQL, " WHERE(", , vbTextCompare)
                        If csv_GetSQLWherePosition = 0 Then
                            csv_GetSQLWherePosition = InStrRev(SQL, ")WHERE(", , vbTextCompare)
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
        'Friend Function csv_IsSQLTableField(ByVal DataSourceName As String, ByVal TableName As String, ByVal FieldName As String)
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
                tableSchema = metaData.getTableSchema(TableName, DataSourceName)
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
                ReturnOK = (Not metaData.getTableSchema(TableName, DataSourceName) Is Nothing)
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
            'If csv_AbortActivity Then Exit Sub
            '
            Const MissingTableErrorMySQL = -2147467259
            Const MissingTableErrorAccess = -2147217900
            Const MissingTableErrorMsSQL = -2147217865
            '
            If TableName = "" Then
                '
                ' tablename required
                '
                Call Err.Raise(KmaErrorInternal, "dll", "Tablename can not be blank.")
            ElseIf InStr(1, TableName, ".") <> 0 Then
                '
                ' Remote table -- remote system controls remote tables
                '
            Else
                '
                ' Local table -- create if not in schema
                '
                iAllowAutoIncrement = AllowAutoIncrement
                TableFound = (Not metaData.getTableSchema(TableName, DataSourceName) Is Nothing)
                '
                If Not TableFound Then
                    If Not iAllowAutoIncrement Then
                        SQL = "Create Table " & TableName & "(ID " & db_GetSQLAlterColumnType(DataSourceName, FieldTypeIdInteger) & ");"
                        Call executeSql(SQL, DataSourceName)
                    Else
                        SQL = "Create Table " & TableName & "(ID " & db_GetSQLAlterColumnType(DataSourceName, FieldTypeIdAutoIdIncrement) & ");"
                        Call executeSql(SQL, DataSourceName)
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
            metaData.tableSchemaListClear()
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
            'If csv_AbortActivity Then Exit Sub
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
            ElseIf InStr(1, TableName, ".") <> 0 Then
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
                UcaseFieldName = UCase(FieldName)
                If Not db_IsSQLTableField(DataSourceName, TableName, FieldName) Then
                    SQL = "ALTER TABLE " & TableName & " ADD " & FieldName & " "
                    If Not IsNumeric(fieldType) Then
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
                    Call executeSql(SQL, DataSourceName)
                    '
                    '
                    '
                    If clearMetaCache Then
                        Call cache.invalidateAll()
                        Call metaData.clear()
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
                Call executeSql("DROP TABLE " & TableName, DataSourceName)
                cache.invalidateAll()
                metaData.clear()
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
                    ts = metaData.getTableSchema(TableName, DataSourceName)
                    If (Not ts Is Nothing) Then
                        If Not ts.indexes.Contains(IndexName.ToLower) Then
                            Call executeSql("CREATE INDEX " & IndexName & " ON " & TableName & "( " & FieldNames & " );", DataSourceName)
                            If clearMetaCache Then
                                cache.invalidateAll()
                                metaData.clear()
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                Call handleLegacyClassError1("csv_CreateSQLIndex", "trap")
            End Try
        End Sub
        '
        '========================================================================
        ' Get a Contents Tablename from the ContentPointer
        '========================================================================
        '
        Public Function metaData_GetContentTablename(ByVal ContentName As String) As String
            On Error GoTo ErrorTrap
            '
            Dim CDef As coreMetaDataClass.CDefClass
            '
            CDef = metaData.getCdef(ContentName)
            metaData_GetContentTablename = CDef.ContentTableName
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError1("csv_GetContentTablename", "trap")
        End Function
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
            If db_csOk(CS) Then
                db_GetRecordName = db_GetCS(CS, "Name")
            End If
            Call db_csClose(CS)

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
                CS = db_csOpen(ContentName, "name=" & db_EncodeSQLText(RecordName), "ID", , , , , "ID")
                If db_csOk(CS) Then
                    returnValue = EncodeInteger(db_GetCS(CS, "ID"))
                    If returnValue = 0 Then
                        cpCore.handleException(New ApplicationException("getRecordId([" & ContentName & "],[" & RecordName & "]), a record was found but id returned 0"))
                    End If
                End If
                Call db_csClose(CS)
            Catch ex As Exception
                cpCore.handleException(ex)
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
                cdef = metaData.getCdef(ContentName)
                If Not cdef Is Nothing Then
                    returnOk = cdef.fields.ContainsKey(FieldName.ToLower)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnOk
        End Function
        '
        '========================================================================
        ' ----- Get FieldDescritor from FieldType
        '========================================================================
        '
        Public Function db_GetSQLAlterColumnType(ByVal DataSourceName As String, ByVal fieldType As Integer) As String
            On Error GoTo ErrorTrap
            '
            Dim MethodName As String
            Dim Pointer As Integer
            '
            MethodName = "csv_GetSQLAlterColumnType(" & DataSourceName & "," & fieldType & ")"
            '
            db_GetSQLAlterColumnType = ""
            '
            ' ----- It may depend on the datasource
            '
            Pointer = csv_GetDataSourcePointer(DataSourceName)
            If Pointer < 0 Then
                Call handleLegacyClassError4(KmaErrorInternal, "dll", "Data Source [" & DataSourceName & "] was not found", MethodName, True)
            Else
                'If Not DataSourceConnectionObjs(Pointer).IsOpen Then
                '    Call csv_OpenDataSource(DataSourceName, 30)
                'End If
                If True Then
                    '    Call csv_HandleClassTrapError(KmaErrorInternal, "dll", "Data Source [" & DataSourceName & "] could not be opened", MethodName, True)
                    'Else
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
                            '
                            ' Invalid field type
                            Call Err.Raise(KmaErrorInternal, "dll", "Can not proceed because the field being created has an invalid FieldType [" & fieldType & "]")
                            'csv_GetSQLAlterColumnType = "VarChar(255) NULL"
                            'Call HandleClassTrapError(KmaErrorUser, "dll", "Unknown FieldType [" & FieldType & "], used FieldTypeText", MethodName, False, True)
                    End Select
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
                ts = metaData.getTableSchema(TableName, DataSourceName)
                If (Not ts Is Nothing) Then
                    If ts.indexes.Contains(IndexName.ToLower) Then
                        DataSourceType = csv_GetDataSourceType(DataSourceName)
                        Select Case DataSourceType
                            Case DataSourceTypeODBCAccess
                                sql = "DROP INDEX " & IndexName & " ON " & TableName & ";"
                            Case DataSourceTypeODBCMySQL
                                Throw New NotImplementedException("mysql")
                            Case Else
                                sql = "DROP INDEX " & TableName & "." & IndexName & ";"
                        End Select
                        Call executeSql(sql, DataSourceName)
                        cache.invalidateAll()
                        metaData.clear()
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
            '            UcaseIndexName = UCase(IndexName)
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
        Public Function csv_GetDataSourceID(ByVal DataSourceName As String) As Integer
            On Error GoTo ErrorTrap
            '
            Dim DataSourcePointer As Integer
            Dim MethodName As String
            '
            MethodName = "csv_GetDataSourceID"
            '
            DataSourcePointer = csv_GetDataSourcePointer(DataSourceName)
            If dataSources.Length > 0 Then
                csv_GetDataSourceID = dataSources(DataSourcePointer).Id
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
        Friend Function db_isCdefField(ByVal ContentID As Integer, ByVal FieldName As String) As Boolean
            Dim RS As DataTable
            '
            RS = executeSql("select top 1 id from ccFields where name=" & db_EncodeSQLText(FieldName) & " and contentid=" & ContentID)
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
        Public Function csv_GetDataSourceType(ByVal DataSourceName As String) As Integer
            csv_GetDataSourceType = DataSourceTypeODBCSQLServer
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
        Friend Function db_GetFieldTypeIdByADOType(ByVal ADOFieldType As Integer) As Integer
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
                    'Call csv_HandleClassTrapError(KmaErrorUser, "dll", "Unknown ADO field type [" & ADOFieldType & "], [Text] used", MethodName, False, True)
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
            Select Case LCase(FieldTypeName)
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
                    'Call Err.Raise(KmaErrorInternal, "dll", "Unknown FieldTypeName [" & FieldTypeName & "], valid values are [" & Mid(Copy, 2) & "]")
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
        Public Function db_csOpen(ByVal ContentName As String, Optional ByVal Criteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal MemberID As Integer = 0, Optional ByVal WorkflowRenderingMode As Boolean = False, Optional ByVal WorkflowEditingMode As Boolean = False, Optional ByVal SelectFieldList As String = "", Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
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
                db_csOpen = -1
                '
                If (ContentName.ToLower() = "system email") Then
                    ContentName = ContentName
                End If
                If ContentName = "" Then
                    Throw (New ApplicationException("db_csOpen called With a blank ContentName"))
                Else
                    CDef = metaData.getCdef(ContentName)
                    If (CDef Is Nothing) Then
                        Throw (New ApplicationException("No content definition found For [" & ContentName & "]"))
                    ElseIf (CDef.Id <= 0) Then
                        Throw (New ApplicationException("No content definition found For [" & ContentName & "]"))
                    Else
                        '
                        'hint = hint & ", 100"
                        iActiveOnly = ((ActiveOnly))
                        iSortFieldList = encodeEmptyText(SortFieldList, CDef.DefaultSortMethod)
                        iWorkflowRenderingMode = siteProperty_AllowWorkflowAuthoring And CDef.AllowWorkflowAuthoring And (WorkflowRenderingMode)
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
                                SortField = Replace(SortField, "asc", "", , , vbTextCompare)
                                SortField = Replace(SortField, "desc", "", , , vbTextCompare)
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
                            ContentCriteria = Replace(ContentCriteria, CDef.ContentTableName & ".", "")
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
                        iSelectFieldList = Replace(iSelectFieldList, vbTab, " ")
                        Do While InStr(1, iSelectFieldList, " ,") <> 0
                            iSelectFieldList = Replace(iSelectFieldList, " ,", ",")
                        Loop
                        Do While InStr(1, iSelectFieldList, ", ") <> 0
                            iSelectFieldList = Replace(iSelectFieldList, ", ", ",")
                        Loop
                        If (iSelectFieldList <> "") And (InStr(1, iSelectFieldList, "*", vbTextCompare) = 0) Then
                            TestUcaseFieldList = UCase("," & iSelectFieldList & ",")
                            If InStr(1, TestUcaseFieldList, ",CONTENTCONTROLID,", vbTextCompare) = 0 Then
                                iSelectFieldList = iSelectFieldList & ",ContentControlID"
                            End If
                            If InStr(1, TestUcaseFieldList, ",NAME,", vbTextCompare) = 0 Then
                                iSelectFieldList = iSelectFieldList & ",Name"
                            End If
                            If InStr(1, TestUcaseFieldList, ",ID,", vbTextCompare) = 0 Then
                                iSelectFieldList = iSelectFieldList & ",ID"
                            End If
                            If InStr(1, TestUcaseFieldList, ",ACTIVE,", vbTextCompare) = 0 Then
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
                            'UcaseTablename = UCase(TableName)
                            DataSourceName = CDef.AuthoringDataSourceName
                            '
                            ' Substitute ID for EditSourceID in criteria
                            '
                            ContentCriteria = Replace(ContentCriteria, " ID=", " " & TableName & ".EditSourceID=")
                            ContentCriteria = Replace(ContentCriteria, " ID>", " " & TableName & ".EditSourceID>")
                            ContentCriteria = Replace(ContentCriteria, " ID<", " " & TableName & ".EditSourceID<")
                            ContentCriteria = Replace(ContentCriteria, " ID ", " " & TableName & ".EditSourceID ")
                            ContentCriteria = Replace(ContentCriteria, "(ID=", "(" & TableName & ".EditSourceID=")
                            ContentCriteria = Replace(ContentCriteria, "(ID>", "(" & TableName & ".EditSourceID>")
                            ContentCriteria = Replace(ContentCriteria, "(ID<", "(" & TableName & ".EditSourceID<")
                            ContentCriteria = Replace(ContentCriteria, "(ID ", "(" & TableName & ".EditSourceID ")
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
                                iSortFieldList = Replace(iSortFieldList, ",ID,", "," & TableName & ".EditSourceID,")
                                iSortFieldList = Replace(iSortFieldList, ",ID ", "," & TableName & ".EditSourceID ")
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
                                If InStr(1, iSelectFieldList, ",ID,", vbTextCompare) = 0 Then
                                    '
                                    ' Add ID select if not there
                                    '
                                    iSelectFieldList = iSelectFieldList & TableName & ".EditSourceID As ID,"
                                Else
                                    '
                                    ' remove ID, and add ID alias to return EditSourceID (the live records id)
                                    '
                                    iSelectFieldList = Replace(iSelectFieldList, ",ID,", "," & TableName & ".EditSourceID As ID,")
                                End If
                                '
                                ' Add the edit fields to the select
                                '
                                Copy2 = TableName & ".EDITSOURCEID,"
                                If (InStr(1, iSelectFieldList, ",EDITSOURCEID,", vbTextCompare) <> 0) Then
                                    iSelectFieldList = Replace(iSelectFieldList, ",EDITSOURCEID,", "," & Copy2, 1, 99, vbTextCompare)
                                Else
                                    iSelectFieldList = iSelectFieldList & Copy2
                                End If
                                Copy2 = TableName & ".EDITARCHIVE,"
                                If (InStr(1, iSelectFieldList, ",EDITARCHIVE,", vbTextCompare) <> 0) Then
                                    iSelectFieldList = Replace(iSelectFieldList, ",EDITARCHIVE,", "," & Copy2, 1, 99, vbTextCompare)
                                Else
                                    iSelectFieldList = iSelectFieldList & Copy2
                                End If
                                Copy2 = TableName & ".EDITBLANK,"
                                If (InStr(1, iSelectFieldList, ",EDITBLANK,", vbTextCompare) <> 0) Then
                                    iSelectFieldList = Replace(iSelectFieldList, ",EDITBLANK,", "," & Copy2, 1, 99, vbTextCompare)
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
                        db_csOpen = db_initCS(iMemberID)
                        With csv_ContentSet(db_csOpen)
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
                                csv_ContentSet(db_csOpen).Source = SQL
                            Else
                                '
                                ' Run the query
                                '
                                csv_ContentSet(db_csOpen).dt = executeSql(SQL, DataSourceName, .PageSize * (.PageNumber - 1), .PageSize)
                            End If
                        End With
                        'hint = hint & ",600"
                        Call db_initCSData(db_csOpen)
                        Call db_LoadContentSetCurrentRow(db_csOpen)
                        '
                        'hint = hint & ",700"
                        If iWorkflowRenderingMode Then
                            '
                            ' Authoring mode
                            '
                            Call db_VerifyWorkflowAuthoringRecord(db_csOpen)
                            If AllowWorkflowSave Then
                                '
                                ' Workflow Editing Mode - lock the first record
                                '
                                If Not db_IsCSEOF(db_csOpen) Then
                                    If csv_ContentSet(db_csOpen).WorkflowEditingRequested Then
                                        RecordID = db_GetCSInteger(db_csOpen, "ID")
                                        If Not csv_IsRecordLocked(ContentName, RecordID, MemberID) Then
                                            csv_ContentSet(db_csOpen).WorkflowEditingMode = True
                                            Call csv_SetEditLock(ContentName, RecordID, MemberID)
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
                cpCore.handleException(ex)
                'If False Then
                If Not siteProperty_trapErrors Then
                    Throw New ApplicationException("rethrow", ex)
                End If
                'End If
            End Try
        End Function
        '
        ' ----- Load the Word Search Exclude List
        '
        Public Function csv_GetWordSearchExcludeList() As String
            'Dim aoTextSearch As Object
            '
            '    aoTextSearch = CreateObject("aoTextSearch.TextSearchClass")
            '    Call aoTextSearch.Init(Me)
            '    csv_GetWordSearchExcludeList = aoTextSearch.GetWordSearchExcludeList()
            '    aoTextSearch = Nothing
            '
        End Function
        '
        '========================================================================
        ' csv_DeleteCSRecord
        '========================================================================
        '
        Public Sub csv_DeleteCSRecord(ByVal CSPointer As Integer)
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
            If Not db_csOk(CSPointer) Then
                '
                Call handleLegacyClassError3(MethodName, ("csv_ContentSet Is empty Or at End-Of-file"))
            ElseIf Not csv_ContentSet(CSPointer).Updateable Then
                '
                Call handleLegacyClassError3(MethodName, ("csv_ContentSet Is Not Updateable"))
            Else
                With csv_ContentSet(CSPointer).CDef
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
                    LiveRecordID = db_GetCSInteger(CSPointer, "ID")
                    If Not csv_ContentSet(CSPointer).WorkflowAuthoringMode Then
                        '
                        ' delete any files
                        '
                        Dim fieldName As String
                        Dim field As coreMetaDataClass.CDefFieldClass
                        With csv_ContentSet(CSPointer).CDef
                            For Each keyValue In .fields
                                field = keyValue.Value
                                With field
                                    fieldName = .nameLc
                                    Select Case .fieldTypeId
                                        Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdFileCSS, FieldTypeIdFileJavascript, FieldTypeIdFileXML
                                            '
                                            ' public content files
                                            '
                                            Filename = db_GetCSText(CSPointer, fieldName)
                                            If Filename <> "" Then
                                                Call cdnFiles.DeleteFile(cdnFiles.joinPath(config.cdnFilesNetprefix, Filename))
                                            End If
                                        Case FieldTypeIdFileTextPrivate, FieldTypeIdFileHTMLPrivate
                                            '
                                            ' private files
                                            '
                                            Filename = db_GetCSText(CSPointer, fieldName)
                                            If Filename <> "" Then
                                                Call privateFiles.DeleteFile(Filename)
                                            End If
                                    End Select
                                End With
                            Next
                        End With
                        '
                        ' non-workflow mode, delete the live record
                        '
                        Call db_DeleteTableRecord(ContentDataSourceName, ContentTableName, LiveRecordID)
                        If csv_AllowAutocsv_ClearContentTimeStamp Then
                            Call cache.invalidateTag(ContentName)
                        End If
                        Call csv_DeleteContentRules(ContentID, LiveRecordID)
                        '                Select Case UCase(ContentTableName)
                        '                    Case "CCCONTENTWATCH", "CCCONTENTWATCHLISTS", "CCCONTENTWATCHLISTRULES"
                        '                    Case Else
                        '                        Call csv_DeleteContentTracking(ContentName, LiveRecordID, True)
                        '                    End Select
                    Else
                        '
                        ' workflow mode, mark the editrecord "Blanked"
                        '
                        EditRecordID = db_GetCSInteger(CSPointer, "EditID")
                        sqlList = New sqlFieldListClass
                        Call sqlList.add("EDITBLANK", SQLTrue) ' Pointer)
                        Call sqlList.add("MODIFIEDBY", db_EncodeSQLNumber(csv_ContentSet(CSPointer).OwnerMemberID)) ' Pointer)
                        Call sqlList.add("MODIFIEDDATE", db_EncodeSQLDate(Now)) ' Pointer)
                        Call db_UpdateTableRecord(AuthoringDataSourceName, AuthoringTableName, "ID=" & EditRecordID, sqlList)
                        Call csv_SetAuthoringControl(ContentName, LiveRecordID, AuthoringControlsModified, csv_ContentSet(CSPointer).OwnerMemberID)
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
            Return db_csOpenSql(SQL, DataSourceName, PageSize, PageNumber)
        End Function
        '
        Public Function db_csOpenSql(ByVal SQL As String, Optional ByVal DataSourceName As String = "", Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
            Dim returnCs As Integer = -1
            Try
                'Dim CSPointer As Integer
                'Dim RS As DataTable
                '
                ' ----- Open the csv_ContentSet
                '       (Save all buffered csv_ContentSet rows incase of overlap)
                '       No - Csets may be from other PageProcesses in unknown states
                '
                returnCs = db_initCS(cpCore.userId)
                With csv_ContentSet(returnCs)
                    .Updateable = False
                    .ContentName = ""
                    .PageNumber = PageNumber
                    .PageSize = (PageSize)
                    .DataSource = DataSourceName
                    .SelectTableFieldList = ""
                End With
                '
                If useCSReadCacheMultiRow Then
                    csv_ContentSet(returnCs).dt = executeSql(SQL, DataSourceName, PageSize * (PageNumber - 1), PageSize)
                    Call db_initCSData(returnCs)
                    Call db_LoadContentSetCurrentRow(returnCs)
                Else
                    csv_ContentSet(returnCs).dt = executeSql(SQL, DataSourceName, PageSize * (PageNumber - 1), PageSize)
                    Call db_initCSData(returnCs)
                    Call db_LoadContentSetCurrentRow(returnCs)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
                        If Not csv_ContentSet(ContentSetPointer).IsOpen Then
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
                        ReDim Preserve csv_ContentSet(csv_ContentSetSize + 1)
                    End If
                    csv_ContentSetCount = csv_ContentSetCount + 1
                    returnCs = csv_ContentSetCount
                End If
                '
                With csv_ContentSet(returnCs)
                    .IsOpen = True
                    .WorkflowAuthoringMode = False
                    .ContentName = ""
                    .NewRecord = True
                    '.writeCacheSize = 0
                    '.writeCacheCount = 0
                    .Updateable = False
                    .IsModified = False
                    .OwnerMemberID = MemberID
                    .LastUsed = Now
                End With
            Catch ex As Exception
                cpCore.handleException(ex)
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
        Public Sub db_csClose(ByRef CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False)
            Try
                If (CSPointer > 0) And (CSPointer <= csv_ContentSetCount) Then
                    With csv_ContentSet(CSPointer)
                        If .IsOpen Then
                            Call csv_SaveCSRecord(CSPointer, AsyncSave)
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
                cpCore.handleException(ex)
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
            If Not db_csOk(CSPointer) Then
                '
                Call handleLegacyClassError3(MethodName, ("CSPointer Not csv_IsCSOK."))
            Else
                Call csv_SaveCSRecord(CSPointer, AsyncSave)
                ' ##### moved into save so First does it also
                'csv_ContentSet(CSPointer).writeCacheCount = 0
                csv_ContentSet(CSPointer).WorkflowEditingMode = False
                '
                ' Move to next row
                '
                csv_ContentSet(CSPointer).readCacheRowPtr = csv_ContentSet(CSPointer).readCacheRowPtr + 1
                If Not db_IsCSEOF(CSPointer) Then
                    '
                    ' Not EOF
                    '
                    Call db_LoadContentSetCurrentRow(CSPointer)
                    '
                    ' Set Workflow Edit Mode from Request and EditLock state
                    '
                    If (Not db_IsCSEOF(CSPointer)) And csv_ContentSet(CSPointer).WorkflowEditingRequested Then
                        ContentName = csv_ContentSet(CSPointer).ContentName
                        RecordID = db_GetCSInteger(CSPointer, "ID")
                        If Not csv_IsRecordLocked(ContentName, RecordID, csv_ContentSet(CSPointer).OwnerMemberID) Then
                            csv_ContentSet(CSPointer).WorkflowEditingMode = True
                            Call csv_SetEditLock(ContentName, RecordID, csv_ContentSet(CSPointer).OwnerMemberID)
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
                    csv_ContentSet(CSPointer).readCacheRowPtr = -1
                Else
                    csv_ContentSet(CSPointer).ResultEOF = True
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
                Call csv_SaveCSRecord(CSPointer, AsyncSave)
                If useCSReadCacheMultiRow Then
                    csv_ContentSet(CSPointer).readCacheRowPtr = 0
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
            FieldNameLocalUcase = UCase(FieldNameLocal)
            If Not db_csOk(CSPointer) Then
                '
                cpCore.handleException(New Exception("csv_ContentSet Is empty Or EOF"))
            Else
                With csv_ContentSet(CSPointer)
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
                        '        If FieldNameLocalUcase = UCase(.Name) Then
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
                                Call handleLegacyClassError4(KmaErrorUser, "dll", "Field [" & FieldNameLocal & "] was Not found In csv_ContentSet from source [" & .Source & "]", "unknownMethodNameLegacyCall", False, False)
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
                                                db_GetCSField = csv_TextDeScramble(EncodeText(db_GetCSField))
                                            End If
                                        End If
                                        Exit For
                                    End If
                                Next
                                If ColumnPointer = .ResultColumnCount Then
                                    Call handleLegacyClassError4(KmaErrorUser, "dll", "Field [" & FieldNameLocal & "] was Not found In csv_ContentSet from source [" & .Source & "]", "unknownMethodNameLegacyCall", False, False)
                                End If
                            End If
                            On Error GoTo ErrorTrap
                        End If
                    End If
                    .LastUsed = Now
                End With
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            cpCore.handleException(New Exception("Unexpected exception"))
ErrorTrapField:
            Dim ErrDescription As String
            ErrDescription = GetErrString(Err)
            If csv_ContentSet(CSPointer).Updateable Then
                Call handleLegacyClassError4(KmaErrorUser, "dll", "Error " & ErrDescription & " reading Field [" & FieldNameLocal & "] from Content Definition [" & csv_ContentSet(CSPointer).ContentName & "],  recordset sql [" & csv_ContentSet(CSPointer).Source & "]", "csv_GetCSField", False)
            Else
                Call handleLegacyClassError4(KmaErrorUser, "dll", "Error " & ErrDescription & " reading Field [" & FieldNameLocal & "] from Source [" & csv_ContentSet(CSPointer).Source & "]", "csv_GetCSField", False)
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
            If Not db_csOk(CSPointer) Then
                Call handleLegacyClassError3(MethodName, ("csv_ContentSet invalid Or End-Of-file"))
            Else
                csv_ContentSet(CSPointer).fieldPointer = 0
                db_GetCSFirstFieldName = csv_GetCSNextFieldName(CSPointer)
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
        Function csv_GetCSNextFieldName(ByVal CSPointer As Integer) As String
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-061" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim ContentPointer As Integer
            '
            csv_GetCSNextFieldName = ""
            '
            If Not db_csOk(CSPointer) Then
                Call handleLegacyClassError3("csv_GetCSNextFieldName", "csv_ContentSet invalid Or End-Of-file")
            Else
                With csv_ContentSet(CSPointer)
                    If useCSReadCacheMultiRow Then
                        Do While (csv_GetCSNextFieldName = "") And (.fieldPointer < .ResultColumnCount)
                            csv_GetCSNextFieldName = .fieldNames(.fieldPointer)
                            .fieldPointer = .fieldPointer + 1
                        Loop
                    Else
                        Do While (csv_GetCSNextFieldName = "") And (.fieldPointer < .dt.Columns.Count)
                            csv_GetCSNextFieldName = .dt.Columns(.fieldPointer).ColumnName
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
            If db_csOk(CSPointer) Then
                If csv_ContentSet(CSPointer).Updateable Then
                    With csv_ContentSet(CSPointer).CDef
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
            If db_csOk(CSPointer) Then
                If csv_ContentSet(CSPointer).Updateable Then
                    With csv_ContentSet(CSPointer).CDef
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
            If db_csOk(CSPointer) Then
                If useCSReadCacheMultiRow Then
                    db_GetCSSelectFieldList = Join(csv_ContentSet(CSPointer).fieldNames, ",")
                Else
                    db_GetCSSelectFieldList = csv_ContentSet(CSPointer).SelectTableFieldList
                    If db_GetCSSelectFieldList = "" Then
                        With csv_ContentSet(CSPointer)
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
            'csv_IsCSFieldSupported = InStr(1, CSSelectFieldList & ",", "." & FieldName & ",", vbTextCompare)
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
        Public Function db_GetCSFilename(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal OriginalFilename As String, Optional ByVal ContentName As String = "") As String
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
            If Not db_csOk(CSPointer) Then
                Call Err.Raise(KmaErrorInternal, "dll", "CSPointer Not OK")
            ElseIf FieldName = "" Then
                Call Err.Raise(KmaErrorInternal, "dll", "Fieldname Is blank")
            Else
                UcaseFieldName = UCase(Trim(FieldName))
                db_GetCSFilename = EncodeText(db_GetCSField(CSPointer, UcaseFieldName))
                If db_GetCSFilename <> "" Then
                    '
                    ' ----- A filename came from the record
                    '
                    If OriginalFilename <> "" Then
                        '
                        ' ----- there was an original filename, make sure it matches the one in the record
                        '
                        LenOriginalFilename = Len(OriginalFilename)
                        LenFilename = Len(db_GetCSFilename)
                        Pos = (1 + LenFilename - LenOriginalFilename)
                        If Pos <= 0 Then
                            '
                            ' Original Filename changed, create a new csv_GetCSFilename
                            '
                            db_GetCSFilename = ""
                        ElseIf Mid(db_GetCSFilename, Pos) <> OriginalFilename Then
                            '
                            ' Original Filename changed, create a new csv_GetCSFilename
                            '
                            db_GetCSFilename = ""
                        End If
                    End If
                End If
                If db_GetCSFilename = "" Then
                    With csv_ContentSet(CSPointer)
                        '
                        ' ----- no filename present, get id field
                        '
                        If .ResultColumnCount > 0 Then
                            For FieldPointer = 0 To .ResultColumnCount - 1
                                If UCase(.fieldNames(FieldPointer)) = "ID" Then
                                    RecordID = db_GetCSInteger(CSPointer, "ID")
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
                            If csv_ContentSet(CSPointer).WorkflowAuthoringMode Then
                                TableName = .CDef.AuthoringTableName
                            Else
                                TableName = .CDef.ContentTableName
                            End If
                        ElseIf ContentName <> "" Then
                            '
                            ' CS is SQL-based, use the contentname
                            '
                            TableName = metaData_GetContentTablename(ContentName)
                        Else
                            '
                            ' no Contentname given
                            '
                            Call Err.Raise(KmaErrorInternal, "dll", "Can Not create a filename because no ContentName was given, And the csv_ContentSet Is SQL-based.")
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
                        db_GetCSFilename = csv_GetVirtualFilenameByTable(TableName, FieldName, RecordID, OriginalFilename, fieldTypeId)
                        Call db_SetCSField(CSPointer, UcaseFieldName, db_GetCSFilename)
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
        Public Function db_GetCSText(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            db_GetCSText = EncodeText(db_GetCSField(CSPointer, FieldName))
        End Function
        '
        '   encodeInteger( csv_GetCSField )
        '
        Public Function db_GetCSInteger(ByVal CSPointer As Integer, ByVal FieldName As String) As Integer
            db_GetCSInteger = EncodeInteger(db_GetCSField(CSPointer, FieldName))
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
        Public Function db_GetCSBoolean(ByVal CSPointer As Integer, ByVal FieldName As String) As Boolean
            db_GetCSBoolean = EncodeBoolean(db_GetCSField(CSPointer, FieldName))
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
            If Not db_csOk(CSPointer) Then
                '
                Call handleLegacyClassError3("csv_SetCSTextFile", ("csv_ContentSet invalid Or End-Of-file"))
            Else
                With csv_ContentSet(CSPointer)
                    If Not .Updateable Then
                        '
                        Call handleLegacyClassError3("csv_SetCSTextFile", ("Attempting To update an unupdateable Content Set"))
                    Else
                        OldFilename = db_GetCSText(CSPointer, FieldName)
                        Filename = db_GetCSFilename(CSPointer, FieldName, "", ContentName)
                        If OldFilename <> Filename Then
                            '
                            ' Filename changed, mark record changed
                            '
                            Call privateFiles.SaveFile(Filename, Copy)
                            Call db_SetCSField(CSPointer, FieldName, Filename)
                        Else
                            OldCopy = cdnFiles.ReadFile(Filename)
                            If OldCopy <> Copy Then
                                '
                                ' copy changed, mark record changed
                                '
                                Call privateFiles.SaveFile(Filename, Copy)
                                Call db_SetCSField(CSPointer, FieldName, Filename)
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
            Filename = db_GetCSText(CSPointer, FieldName)
            If Filename <> "" Then
                db_csGetTextFile = cdnFiles.ReadFile(Filename)
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
        Public Sub db_SetCSField(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Object)
            Try
                Dim FieldNameLocal As String
                Dim FieldNameLocalUcase As String
                Dim SetNeeded As Boolean
                Dim field As coreMetaDataClass.CDefFieldClass
                '
                If Not db_csOk(CSPointer) Then
                    cpCore.handleException(New ApplicationException("csv_ContentSet invalid Or End-Of-file"))
                Else
                    With csv_ContentSet(CSPointer)
                        If Not .Updateable Then
                            cpCore.handleException(New ApplicationException("Attempting To update an unupdateable Content Set"))
                        Else
                            FieldNameLocal = Trim(FieldName)
                            FieldNameLocalUcase = UCase(FieldNameLocal)
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
                                            If EncodeBoolean(FieldValue) <> db_GetCSBoolean(CSPointer, FieldName) Then
                                                SetNeeded = True
                                            End If
                                        Case Else
                                            '
                                            ' Set if text of value changes
                                            '
                                            If EncodeText(FieldValue) <> db_GetCSText(CSPointer, FieldName) Then
                                                SetNeeded = True
                                            End If
                                    End Select
                                End If
                            End With
                            If Not SetNeeded Then
                                SetNeeded = SetNeeded
                            Else
                                If csv_ContentSet(CSPointer).WorkflowAuthoringMode Then
                                    '
                                    ' Do phantom ID replacement
                                    '
                                    If FieldNameLocalUcase = "ID" Then
                                        FieldNameLocal = "EditSourceID"
                                        FieldNameLocalUcase = UCase(FieldNameLocal)
                                    End If
                                End If
                                If .writeCache.ContainsKey(FieldName.ToLower) Then
                                    .writeCache.Item(FieldName.ToLower) = EncodeText(FieldValue)
                                Else
                                    .writeCache.Add(FieldName.ToLower, EncodeText(FieldValue))
                                End If
                                .LastUsed = Now
                            End If
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-071" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CS As Integer
            Dim hint As String
            '
            'hint = "csv_InsertContentRecordGetID(" & ContentName & ")"
            metaData_InsertContentRecordGetID = -1
            CS = db_csInsertRecord(ContentName, MemberID)
            If Not db_csOk(CS) Then
                'hint = hint & ", notok"
            Else
                metaData_InsertContentRecordGetID = EncodeInteger(db_GetCSField(CS, "ID"))
                'hint = hint & ", ok Return=" & db_InsertContentRecordGetID
            End If
            Call db_csClose(CS)
            ''Call main_testPoint(hint)
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
            If db_csOk(CSPointer) Then
                Call csv_DeleteCSRecord(CSPointer)
            End If
            Call db_csClose(CSPointer)
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
        Public Sub db_DeleteContentRecords(ByVal ContentName As String, ByVal Criteria As String, Optional ByVal MemberID As Integer = 0)
            Try
                '
                Dim MethodName As String
                Dim CSPointer As Integer
                Dim CDef As coreMetaDataClass.CDefClass
                Dim ContentCriteria As String
                '
                MethodName = "csv_DeleteContentRecords"
                '
                CDef = metaData.getCdef(ContentName)
                If CDef Is Nothing Then
                    cpCore.handleException(New ApplicationException("ContentName [" & ContentName & "] was Not found"))
                Else
                    If CDef.Id = 0 Then
                        cpCore.handleException(New ApplicationException("ContentName [" & ContentName & "] was Not found"))
                    Else
                        '
                        ' Delete records from Content
                        '
                        If siteProperty_AllowWorkflowAuthoring And (CDef.AllowWorkflowAuthoring) Then
                            '
                            ' Supports Workflow Authoring, handle it record at a time
                            '
                            CSPointer = db_csOpen(ContentName, Criteria, , False, MemberID, True, True, "ID")
                            Do While db_csOk(CSPointer)
                                Call csv_DeleteCSRecord(CSPointer)
                                Call db_csGoNext(CSPointer)
                            Loop
                            Call db_csClose(CSPointer)
                        Else
                            '
                            ' No Workflow Authoring, just delete records
                            '
                            ContentCriteria = "(" & Criteria & ")And(" & CDef.ContentControlCriteria & ")"
                            Call csv_DeleteTableRecords(CDef.ContentDataSourceName, CDef.ContentTableName, ContentCriteria)
                            If csv_AllowAutocsv_ClearContentTimeStamp Then
                                Call cache.invalidateTag(ContentName)
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
        Public Function db_csInsertRecord(ByVal ContentName As String, Optional ByVal MemberID As Integer = -1) As Integer
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
                    MemberID = cpCore.userId
                End If
                returnCs = -1
                CDef = metaData.getCdef(ContentName)
                If (CDef Is Nothing) Then
                    '
                    Throw (New ApplicationException("content [" & ContentName & "] could Not be found."))
                Else
                    With CDef
                        WorkflowAuthoringMode = .AllowWorkflowAuthoring And siteProperty_AllowWorkflowAuthoring
                        If WorkflowAuthoringMode Then
                            '
                            ' authoring, Create Blank in Live Table
                            '
                            LiveRecordID = db_InsertTableRecordGetID(.ContentDataSourceName, .ContentTableName, MemberID)
                            sqlList = New sqlFieldListClass
                            Call sqlList.add("EDITBLANK", SQLTrue) ' Pointer)
                            Call sqlList.add("EDITSOURCEID", db_EncodeSQLNumber(Nothing)) ' Pointer)
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
                                        Select Case UCase(FieldName)
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
                                                        sqlList.add(FieldName, db_EncodeSQLBoolean(.defaultValue))
                                                    Case FieldTypeIdCurrency, FieldTypeIdFloat, FieldTypeIdInteger, FieldTypeIdMemberSelect
                                                        sqlList.add(FieldName, db_EncodeSQLNumber(.defaultValue))
                                                    Case FieldTypeIdDate
                                                        sqlList.add(FieldName, db_EncodeSQLDate(.defaultValue))
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
                                                                LookupContentName = csv_GetContentNameByID(.lookupContentID)
                                                                If LookupContentName <> "" Then
                                                                    DefaultValueText = getRecordID(LookupContentName, DefaultValueText).ToString()
                                                                End If
                                                            ElseIf .lookupList <> "" Then
                                                                UCaseDefaultValueText = UCase(DefaultValueText)
                                                                lookups = Split(.lookupList, ",")
                                                                For Ptr = 0 To UBound(lookups)
                                                                    If UCaseDefaultValueText = UCase(lookups(Ptr)) Then
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
                                                        sqlList.add(FieldName, db_EncodeSQLText(.defaultValue))
                                                End Select
                                        End Select
                                    End If
                                End With
                            Next
                        End If
                        '
                        If WorkflowAuthoringMode Then
                            Call sqlList.add("EDITSOURCEID", db_EncodeSQLNumber(LiveRecordID)) ' ArrayPointer)
                            Call sqlList.add("EDITARCHIVE", SQLFalse) ' ArrayPointer)
                            Call sqlList.add("EDITBLANK", SQLFalse) ' ArrayPointer)
                        Else
                            Call sqlList.add("EDITSOURCEID", db_EncodeSQLNumber(Nothing)) ' ArrayPointer)
                            Call sqlList.add("EDITARCHIVE", SQLFalse) ' ArrayPointer)
                            Call sqlList.add("EDITBLANK", SQLFalse) ' ArrayPointer)
                        End If
                        '
                        CreateKeyString = db_EncodeSQLNumber(getRandomLong)
                        DateAddedString = db_EncodeSQLDate(Now)
                        '
                        Call sqlList.add("CREATEKEY", CreateKeyString) ' ArrayPointer)
                        Call sqlList.add("DATEADDED", DateAddedString) ' ArrayPointer)
                        Call sqlList.add("CONTENTCONTROLID", db_EncodeSQLNumber(CDef.Id)) ' ArrayPointer)
                        Call sqlList.add("CREATEDBY", db_EncodeSQLNumber(MemberID)) ' ArrayPointer)
                        '
                        Call db_InsertTableRecord(DataSourceName, TableName, sqlList)
                        '
                        ' ----- Get the record back so we can use the ID
                        '
                        Criteria = "((createkey=" & CreateKeyString & ")And(DateAdded=" & DateAddedString & "))"
                        returnCs = db_csOpen(ContentName, Criteria, "ID DESC", False, MemberID, WorkflowAuthoringMode, True)
                        '
                        ' ----- Clear Time Stamp because a record changed
                        '
                        If csv_AllowAutocsv_ClearContentTimeStamp Then
                            Call cache.invalidateTag(ContentName)
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
            db_OpenCSContentRecord = db_csOpen(ContentName, "(ID=" & db_EncodeSQLNumber(RecordID) & ")", , False, MemberID, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1)
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
        Function db_csOk(ByVal CSPointer As Integer) As Boolean
            On Error GoTo ErrorTrap 'Const Tn = "IsCSOK" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            If CSPointer > 0 Then
                If useCSReadCacheMultiRow Then
                    With csv_ContentSet(CSPointer)
                        db_csOk = .IsOpen And (.readCacheRowPtr >= 0) And (.readCacheRowPtr < .readCacheRowCnt)
                    End With
                Else
                    db_csOk = csv_ContentSet(CSPointer).IsOpen And (Not csv_ContentSet(CSPointer).ResultEOF)
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
            cpCore.handleException(New Exception("Unexpected exception"))
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
            If db_csOk(CSSource) And db_csOk(CSDestination) Then
                '
                DestCDef = csv_ContentSet(CSDestination).CDef
                DestContentName = DestCDef.Name
                DestRecordID = db_GetCSInteger(CSDestination, "ID")
                FieldName = db_GetCSFirstFieldName(CSSource)
                Do While (FieldName <> "")
                    Select Case UCase(FieldName)
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
                                    SourceFilename = db_GetCSFilename(CSSource, FieldName, "")
                                    'SourceFilename = (csv_GetCSText(CSSource, FieldName))
                                    If (SourceFilename <> "") Then
                                        DestFilename = db_GetCSFilename(CSDestination, FieldName, "")
                                        'DestFilename = csv_GetVirtualFilename(DestContentName, FieldName, DestRecordID)
                                        Call db_SetCSField(CSDestination, FieldName, DestFilename)
                                        Call cdnFiles.copyFile(SourceFilename, DestFilename)
                                    End If
                                Case FieldTypeIdFileTextPrivate, FieldTypeIdFileHTMLPrivate
                                    '
                                    ' ----- private file
                                    '
                                    SourceFilename = db_GetCSFilename(CSSource, FieldName, "")
                                    'SourceFilename = (csv_GetCSText(CSSource, FieldName))
                                    If (SourceFilename <> "") Then
                                        DestFilename = db_GetCSFilename(CSDestination, FieldName, "")
                                        'DestFilename = csv_GetVirtualFilename(DestContentName, FieldName, DestRecordID)
                                        Call db_SetCSField(CSDestination, FieldName, DestFilename)
                                        Call privateFiles.copyFile(SourceFilename, DestFilename)
                                    End If
                                Case Else
                                    '
                                    ' ----- value
                                    '
                                    Call db_SetCSField(CSDestination, FieldName, db_GetCSField(CSSource, FieldName))
                            End Select
                    End Select
                    FieldName = csv_GetCSNextFieldName(CSSource)
                Loop
                Call csv_SaveCSRecord(CSDestination)
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
            If db_csOk(CSPointer) Then
                db_GetCSSource = csv_ContentSet(CSPointer).Source
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
                If Not db_csOk(CSPointer) Then
                    '
                    cpCore.handleException(New Exception("csv_ContentSet Is empty Or End Of file"))
                Else
                    '
                    ' csv_ContentSet good
                    '
                    With csv_ContentSet(CSPointer)
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
                                            ContentName = csv_GetContentNameByID(.manyToManyRuleContentID)
                                            DbTable = metaData_GetContentTablename(ContentName)
                                            SQL = "Select " & .ManyToManyRuleSecondaryField & " from " & DbTable & " where " & .ManyToManyRulePrimaryField & "=" & RecordID
                                            rs = executeSql(SQL)
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
                                                If IsNumeric(FieldValueVariant) Then
                                                    fieldLookupId = field.lookupContentID
                                                    LookupContentName = csv_GetContentNameByID(fieldLookupId)
                                                    LookupList = field.lookupList
                                                    If (LookupContentName <> "") Then
                                                        '
                                                        ' First try Lookup Content
                                                        '
                                                        CSLookup = db_csOpen(LookupContentName, "ID=" & db_EncodeSQLNumber(FieldValueVariant), , , , , , "name", 1)
                                                        If db_csOk(CSLookup) Then
                                                            db_GetCS = db_GetCSText(CSLookup, "name")
                                                        End If
                                                        Call db_csClose(CSLookup)
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
                                                If IsNumeric(FieldValueVariant) Then
                                                    db_GetCS = db_GetRecordName("people", EncodeInteger(FieldValueVariant))
                                                End If
                                            Case FieldTypeIdCurrency
                                                '
                                                '
                                                '
                                                If IsNumeric(FieldValueVariant) Then
                                                    db_GetCS = FormatCurrency(FieldValueVariant, 2, vbFalse, vbFalse, vbFalse)
                                                End If
                                            'NeedsHTMLEncode = False
                                            Case FieldTypeIdFileTextPrivate, FieldTypeIdFileHTMLPrivate
                                                '
                                                '
                                                '
                                                db_GetCS = privateFiles.ReadFile(EncodeText(FieldValueVariant))
                                            Case FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript
                                                '
                                                '
                                                '
                                                db_GetCS = cdnFiles.ReadFile(EncodeText(FieldValueVariant))
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
                cpCore.handleException(ex)
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
        Public Sub db_setCS(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As Object)
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
                If Not db_csOk(CSPointer) Then
                    Throw New ApplicationException("contentset is invalid Or End-Of-file.")
                Else
                    With csv_ContentSet(CSPointer)
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
                                                fileNameNoExt = db_GetCSText(CSPointer, FieldNameLc)
                                                FieldValueText = EncodeText(FieldValueVariantLocal)
                                                If FieldValueText = "" Then
                                                    If fileNameNoExt <> "" Then
                                                        Call privateFiles.DeleteFile(fileNameNoExt)
                                                        'Call publicFiles.DeleteFile(fileNameNoExt)
                                                        fileNameNoExt = ""
                                                    End If
                                                Else
                                                    If fileNameNoExt = "" Then
                                                        fileNameNoExt = db_GetCSFilename(CSPointer, FieldName, "", ContentName)
                                                    End If
                                                    Call privateFiles.SaveFile(fileNameNoExt, FieldValueText)
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
                                                pathFilenameOriginal = db_GetCSText(CSPointer, FieldNameLc)
                                                PathFilename = pathFilenameOriginal
                                                FieldValueText = EncodeText(FieldValueVariantLocal)
                                                BlankTest = FieldValueText
                                                BlankTest = Replace(BlankTest, " ", "")
                                                BlankTest = Replace(BlankTest, vbCr, "")
                                                BlankTest = Replace(BlankTest, vbLf, "")
                                                BlankTest = Replace(BlankTest, vbTab, "")
                                                If BlankTest = "" Then
                                                    If PathFilename <> "" Then
                                                        Call cdnFiles.DeleteFile(PathFilename)
                                                        PathFilename = ""
                                                    End If
                                                Else
                                                    If PathFilename = "" Then
                                                        PathFilename = db_GetCSFilename(CSPointer, FieldNameLc, "", ContentName)
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
                                                                If Not IsNumeric(fileNameNoExt) Then
                                                                    Pos = InStr(1, fileNameNoExt, ".r", vbTextCompare)
                                                                    If Pos > 0 Then
                                                                        FilenameRev = EncodeInteger(Mid(fileNameNoExt, Pos + 2))
                                                                        FilenameRev = FilenameRev + 1
                                                                        fileNameNoExt = Mid(fileNameNoExt, 1, Pos - 1)
                                                                    End If
                                                                End If
                                                                fileName = fileNameNoExt & ".r" & FilenameRev & "." & FileExt
                                                                'PathFilename = PathFilename & dstFilename
                                                                path = convertCdnUrlToCdnPathFilename(path)
                                                                'srcSysFile = config.physicalFilePath & Replace(srcPathFilename, "/", "\")
                                                                'dstSysFile = config.physicalFilePath & Replace(PathFilename, "/", "\")
                                                                PathFilename = path & fileName
                                                                'Call publicFiles.renameFile(pathFilenameOriginal, fileName)
                                                            End If
                                                        End If
                                                    End If
                                                    If (pathFilenameOriginal <> "") And (pathFilenameOriginal <> PathFilename) Then
                                                        pathFilenameOriginal = convertCdnUrlToCdnPathFilename(pathFilenameOriginal)
                                                        Call cdnFiles.DeleteFile(pathFilenameOriginal)
                                                    End If
                                                    Call cdnFiles.SaveFile(PathFilename, FieldValueText)
                                                End If
                                                FieldValueVariantLocal = PathFilename
                                                SetNeeded = True
                                            Case FieldTypeIdBoolean
                                                '
                                                ' Boolean - sepcial case, block on typed GetAlways set
                                                FieldValueVariantLocal = EncodeBoolean(FieldValueVariantLocal)
                                                If EncodeBoolean(FieldValueVariantLocal) <> db_GetCSBoolean(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                End If
                                            Case FieldTypeIdText
                                                '
                                                ' Set if text of value changes
                                                '
                                                FieldValueVariantLocal = Left(EncodeText(FieldValueVariantLocal), 255)
                                                If EncodeText(FieldValueVariantLocal) <> db_GetCSText(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                End If
                                            Case FieldTypeIdLongText, FieldTypeIdHTML
                                                '
                                                ' Set if text of value changes
                                                '
                                                FieldValueVariantLocal = Left(EncodeText(FieldValueVariantLocal), 65535)
                                                If EncodeText(FieldValueVariantLocal) <> db_GetCSText(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                End If
                                            Case Else
                                                '
                                                ' Set if text of value changes
                                                '
                                                If EncodeText(FieldValueVariantLocal) <> db_GetCSText(CSPointer, FieldNameLc) Then
                                                    SetNeeded = True
                                                End If
                                        End Select
                                    End If
                                End If
                            End With
                            If Not SetNeeded Then
                                SetNeeded = SetNeeded
                            Else
                                If csv_ContentSet(CSPointer).WorkflowAuthoringMode Then
                                    '
                                    ' Do phantom ID replacement
                                    '
                                    If FieldNameLc = "id" Then
                                        FieldNameLc = "editsourceid"
                                        'FieldNameLocalUcase = UCase(FieldNameLocal)
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
                                '            If FieldNameLocalUcase = UCase(.Name) Then
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
                                .LastUsed = Now
                            End If
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
                Throw ex
            End Try
        End Sub
        '
        '
        '
        Public Sub db_RollBackCS(ByVal CSPointer As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-190" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            If db_csOk(CSPointer) Then
                csv_ContentSet(CSPointer).writeCache.Clear()
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
            If db_csOk(CSPointer) Then
                With csv_ContentSet(CSPointer)
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
                            WorkflowMode = .AllowWorkflowAuthoring And siteProperty_AllowWorkflowAuthoring
                        End With
                        '
                        LiveRecordID = db_GetCSInteger(CSPointer, "ID")
                        LiveRecordContentControlID = db_GetCSInteger(CSPointer, "CONTENTCONTROLID")
                        LiveRecordContentName = csv_GetContentNameByID(LiveRecordContentControlID)
                        LiveRecordInactive = Not db_GetCSBoolean(CSPointer, "ACTIVE")
                        '
                        ' Get Edit Record ID
                        '
                        If Not WorkflowMode Then
                            '
                            ' Live Mode
                            '
                            EditRecordID = db_GetCSInteger(CSPointer, "ID")
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
                            rs = executeSql(SQL, EditDataSourceName)
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
                            EditRecordID = db_GetCSInteger(CSPointer, "EDITID")
                        End If
                        '
                        SQLLiveDelimiter = ""
                        SQLLiveUpdate = ""
                        SQLLiveDelimiter = ""
                        SQLEditUpdate = ""
                        SQLEditDelimiter = ""
                        sqlModifiedDate = Now
                        sqlModifiedBy = .OwnerMemberID
                        '
                        AuthorableFieldUpdate = False
                        FieldFoundCount = 0
                        For Each keyValuePair In .writeCache
                            FieldName = keyValuePair.Key
                            UcaseFieldName = UCase(FieldName)
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
                                '                                Call csv_HandleClassErrorAndResume(MethodName, "Record was saved, but field [" & LCase(.CDef.Name) & "].[" & LCase(FieldName) & "] should Not be used In a csv_ContentSet because it Is one Of the Protected record control fields [" & protectedcsv_ContentSetControlFieldList & "]")
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
                                                SQLSetPair = FieldName & "=" & db_EncodeSQLNumber(writeCacheValueVariant)
                                            Case FieldTypeIdBoolean
                                                SQLSetPair = FieldName & "=" & db_EncodeSQLBoolean(writeCacheValueVariant)
                                            Case FieldTypeIdDate
                                                SQLSetPair = FieldName & "=" & db_EncodeSQLDate(writeCacheValueVariant)
                                            Case FieldTypeIdText
                                                Copy = Left(EncodeText(writeCacheValueVariant), 255)
                                                If .Scramble Then
                                                    Copy = csv_TextScramble(Copy)
                                                End If
                                                SQLSetPair = FieldName & "=" & db_EncodeSQLText(Copy)
                                            Case FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdFileTextPrivate, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript, FieldTypeIdFileHTMLPrivate
                                                Copy = Left(EncodeText(writeCacheValueVariant), 255)
                                                SQLSetPair = FieldName & "=" & db_EncodeSQLText(Copy)
                                            Case FieldTypeIdLongText, FieldTypeIdHTML
                                                SQLSetPair = FieldName & "=" & db_EncodeSQLLongText(writeCacheValueVariant)
                                            Case Else
                                                '
                                                ' Invalid fieldtype
                                                '
                                                Call Err.Raise(KmaErrorInternal, "dll", "Can Not save this record because the field [" & .nameLc & "] has an invalid field type Id [" & .fieldTypeId & "]")
                                        End Select
                                        If SQLSetPair <> "" Then
                                            '
                                            ' ----- Set the new value in the 
                                            '
                                            With csv_ContentSet(CSPointer)
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
                                    SQLEditUpdate = SQLEditUpdate & ",MODIFIEDDATE=" & db_EncodeSQLDate(sqlModifiedDate) & ",MODIFIEDBY=" & db_EncodeSQLNumber(sqlModifiedBy)
                                End If
                            Else
                                If (SQLLiveUpdate <> "") Then
                                    '
                                    ' ----- Authorable Fields Updated in non-Authoring Mode, set Live Record Modified
                                    '
                                    SQLLiveUpdate = SQLLiveUpdate & ",MODIFIEDDATE=" & db_EncodeSQLDate(sqlModifiedDate) & ",MODIFIEDBY=" & db_EncodeSQLNumber(sqlModifiedBy)
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
                            Call csv_SetAuthoringControl(ContentName, LiveRecordID, AuthoringControlsModified, .OwnerMemberID)
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
                            Call handleLegacyClassError3(MethodName, ("Can not save record to content [" & LiveRecordContentName & "] because it would create a non-unique record for one or more of the following field(s) [" & UniqueViolationFieldList & "]"))
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
                                If csv_AllowAutocsv_ClearContentTimeStamp And (Not Blockcsv_ClearBake) Then
                                    Call cache.invalidateTag(LiveRecordContentName)
                                End If
                                '
                                ' ----- mark the record NOT UpToDate for SpiderDocs
                                '
                                If (LCase(EditTableName) = "ccpagecontent") And (LiveRecordID <> 0) Then
                                    If db_IsSQLTableField("default", "ccSpiderDocs", "PageID") Then
                                        SQL = "UPDATE ccspiderdocs SET UpToDate = 0 WHERE PageID=" & LiveRecordID
                                        Call executeSql(SQL)
                                    End If
                                End If
                            End If
                        End If
                        .LastUsed = Now
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
            With csv_ContentSet(CSPointer)
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
                                .fieldNames(ColumnPtr) = UCase(dc.ColumnName)
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
            With csv_ContentSet(CSPointer)
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
                csv_ContentSet(CSPointer).ResultEOF = True
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
                Call handleLegacyClassError4(KmaErrorUser, "dll", "csv_IsCSEOF called with an invalid CSPointer", "csv_IsCSEOF", False)
            Else
                If useCSReadCacheMultiRow Then
                    With csv_ContentSet(CSPointer)
                        db_IsCSEOF = (.readCacheRowPtr >= .readCacheRowCnt)
                    End With
                Else
                    db_IsCSEOF = csv_ContentSet(CSPointer).ResultEOF
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            cpCore.handleException(New Exception("Unexpected exception"))
        End Function
        '
        '
        '
        Private Function db_IsCSBOF(ByVal CSPointer As Integer) As Boolean
            On Error GoTo ErrorTrap 'Const Tn = "csv_IsCSBOF" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            If CSPointer <= 0 Then
                Call handleLegacyClassError4(KmaErrorUser, "dll", "csv_IsCSBOF called with an invalid CSPointer", "csv_IsCSBOF", False)
            Else
                db_IsCSBOF = (csv_ContentSet(CSPointer).readCacheRowPtr < 0)
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
        '        '
        '        '========================================================================
        '        '   Read in a file from a given PathFilename, return content
        '        '
        '        '   something out if there was a problem.
        '        '========================================================================
        '        '
        '        Public Function csv_ReadFile(ByVal PathFilename As String) As String
        '            On Error GoTo ErrorTrap : 'Const Tn = "ReadFile" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim MethodName As String
        '            'Dim kmafs As New fileSystemClass
        '            '
        '            MethodName = "csv_ReadFile"
        '            '
        '            csv_ReadFile = ""
        '            If (PathFilename <> "") Then
        '                csv_ReadFile = publicFiles.ReadFile(PathFilename)
        '            End If
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            'kmafs = Nothing
        '            Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, MethodName, True)
        '        End Function
        '        '
        '        '========================================================================
        '        '   Save data to a file
        '        '========================================================================
        '        '
        '        Public Sub csv_SaveFile(ByVal Filename As String, ByVal FileContent As String)
        '            On Error GoTo ErrorTrap : 'Const Tn = "SaveFile" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim MethodName As String
        '            'Dim kmafs As New fileSystemClass
        '            '
        '            MethodName = "csv_SaveFile"
        '            '
        '            If (Filename = "") Then
        '                Call csv_HandleClassTrapError(KmaErrorUser, "dll", "Invalid Argument, Filename is empty.", MethodName, False)
        '            Else
        '                Call publicFiles.SaveFile(Filename, FileContent)
        '            End If
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            'kmafs = Nothing
        '            Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, MethodName, True)
        '        End Sub
        '        '
        '        '========================================================================
        '        '   Save data to a file
        '        '========================================================================
        '        '
        '        Public Sub csv_AppendFile(ByVal Filename As String, ByVal FileContent As String)
        '            On Error GoTo ErrorTrap : 'Const Tn = "AppendFile" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim MethodName As String
        '            'Dim kmafs As New fileSystemClass
        '            '
        '            MethodName = "csv_AppendFile"
        '            '
        '            If (Filename = "") Then
        '                Call csv_HandleClassTrapError(KmaErrorUser, "dll", "Invalid Argument, Filename is empty.", MethodName, False)
        '            Else
        '                Call publicFiles.appendFile(Filename, FileContent)
        '            End If
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            'kmafs = Nothing
        '            Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, MethodName, True)
        '        End Sub
        '        '        '
        '        '========================================================================
        '        ' ----- Creates a file folder if it does not exist
        '        '========================================================================
        '        '
        '        Public Sub csv_CreateFileFolder(ByVal FolderPath As String)
        '            On Error GoTo ErrorTrap : 'Const Tn = "CreateFileFolder" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim MethodName As String
        '            'Dim kmafs As New fileSystemClass
        '            '
        '            MethodName = "csv_CreateFileFolder"
        '            '
        '            If (FolderPath = "") Then
        '                Call csv_HandleClassTrapError(KmaErrorUser, "dll", "Invalid Argument, FolderPath is empty.", MethodName, False)
        '            Else
        '                Call publicFiles.createPath(FolderPath)
        '            End If
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            'kmafs = Nothing
        '            Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, MethodName, True)
        '        End Sub
        '        '
        '        '========================================================================
        '        '   Deletes a file if it exists
        '        '========================================================================
        '        '
        '        Public Sub publicFiles_DeleteFile(ByVal PathFilename As String)
        '            On Error GoTo ErrorTrap 'Const Tn = "DeleteFile" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim MethodName As String
        '            'Dim kmafs As New fileSystemClass
        '            '
        '            MethodName = "csv_DeleteFile"
        '            '
        '            If (PathFilename = "") Then
        '                Call handleLegacyClassError4(KmaErrorUser, "dll", "Invalid Argument, PathFilename is empty.", MethodName, False)
        '            Else
        '                Call publicFiles.DeleteFile(PathFilename)
        '            End If
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            'kmafs = Nothing
        '            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        '        End Sub
        '        '
        '        '========================================================================
        '        '
        '        '========================================================================
        '        '
        '        Public Sub csv_CopxyFile(ByVal SourceFilename As String, ByVal DestinationFilename As String)
        '            On Error GoTo ErrorTrap 'Const Tn = "CopyFile" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim MethodName As String
        '            'Dim kmafs As New fileSystemClass
        '            Dim iSource As String
        '            Dim iDestination As String
        '            '
        '            MethodName = "csv_CopyFile"
        '            '
        '            If (SourceFilename = "") Or (DestinationFilename = "") Then
        '                Call csv_HandleClassTrapError(KmaErrorUser, "dll", "Invalid Argument, Source [" & SourceFilename & "] or Destination [" & DestinationFilename & "] Filename is empty.", MethodName, False)
        '            Else
        '                iSource = Replace(SourceFilename, "/", "\")
        '                iDestination = Replace(DestinationFilename, "/", "\")
        '                Call publicFiles.CopyFile(SourceFilename, DestinationFilename)
        '            End If
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            'kmafs = Nothing
        '            Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, MethodName, True)
        '        End Sub
        '        '
        '        '========================================================================
        '        '
        '        '========================================================================
        '        '
        '        Public Sub csv_rexnameFile(ByVal SourcePathFilename As String, ByVal DestinationFilename As String)
        '            On Error GoTo ErrorTrap 'Const Tn = "renameFile" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim MethodName As String
        '            'Dim kmafs As New fileSystemClass
        '            Dim iSource As String
        '            Dim iDestination As String
        '            '
        '            MethodName = "csv_renameFile"
        '            '
        '            If (SourcePathFilename = "") Or (DestinationFilename = "") Then
        '                Call csv_HandleClassTrapError(KmaErrorUser, "dll", "Invalid Argument, Source [" & SourcePathFilename & "] or Destination [" & DestinationFilename & "] Filename is empty.", MethodName, False)
        '            Else
        '                iSource = Replace(SourcePathFilename, "/", "\")
        '                iDestination = Replace(DestinationFilename, "/", "\")
        '                Call publicFiles.renameFile(SourcePathFilename, DestinationFilename)
        '            End If
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            'kmafs = Nothing
        '            Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, MethodName, True)
        '        End Sub
        ''
        ''========================================================================
        ''   Returns a list of file in a path
        ''========================================================================
        ''
        'Public Function csv_GetFileList(ByVal FolderPath As String, Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As IO.FileInfo()
        '    'Dim kmafs As New fileSystemClass
        '    Return publicFiles.GetFolderFiles(FolderPath)
        'End Function
        ''
        ''========================================================================
        ''   Returns a list of file in a path
        ''========================================================================
        ''
        'Public Function publicFiles.checkPath(ByVal FolderPath As String) As Boolean
        '    'Dim kmafs As New fileSystemClass
        '    publicFiles.checkPath = publicFiles.checkPath(FolderPath)
        'End Function
        ''
        ''========================================================================
        ''
        ''========================================================================
        ''
        'Public Function publicFiles.checkFile(ByVal PathFilename As String) As Boolean
        '    'Dim kmafs As New fileSystemClass
        '    publicFiles.CheckFile = publicFiles.CheckFile(PathFilename)
        'End Function
        '        '
        '        '========================================================================
        '        '   Returns a list of file in a path
        '        '========================================================================
        '        '
        '        Public Function getPublicFileCount(ByVal FolderPath As String) As Integer
        '            On Error GoTo ErrorTrap 'Const Tn = "GetFileCount" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            'Dim kmafs As New fileSystemClass
        '            '
        '            If (FolderPath <> "") Then
        '                If publicFiles.checkPath(FolderPath) Then
        '                    getPublicFileCount = publicFiles.GetFolderFiles(FolderPath).Count
        '                End If
        '            End If
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            'kmafs = Nothing
        '            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_GetFileCount", True)
        '        End Function
        ''
        ''========================================================================
        ''   Read in a file from the sites virtual file directory given filename
        ''========================================================================
        ''
        'Public Function contentFiles.ReadFile(ByVal Filename As String) As String
        '    'Dim publicFilename As String = publicFiles.joinPath(config.contentFilePathPrefix, Filename)
        '    Return contentFiles.ReadFile(Filename)
        'End Function
        '        '
        '        '========================================================================
        '        '
        '        '========================================================================
        '        '
        '        Public Function csv_GetVirtualFileCount(ByVal FolderPath As String) As Integer
        '            On Error GoTo ErrorTrap 'Const Tn = "GetVirtualFileCount" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim physicalFolderPath As String
        '            '
        '            physicalFolderPath = convertUrlToContentFilesPathFilename(FolderPath)
        '            csv_GetVirtualFileCount = getPublicFileCount(physicalFolderPath)
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_GetVirtualFileCount", True)
        '        End Function
        '        '
        '        '========================================================================
        '        '
        '        '========================================================================
        '        '
        '        Public Function contentFiles.getFolderList(ByVal FolderPath As String) As String
        '            On Error GoTo ErrorTrap 'Const Tn = "GetVirtualFolderList" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim physicalFolderPath As String
        '            '
        '            physicalFolderPath = convertUrlToContentFilesPathFilename(FolderPath)
        '            csv_GetVirtualFolderList = contentFiles.getFolderList(physicalFolderPath)
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_GetVirtualFolderList", True)
        '        End Function
        '        '
        '        '========================================================================
        '        ' ----- Create a filename for the Virtual Directory
        '        '   Do not allow spaces.
        '        '   If the content supports authoring, the filename returned will be for the
        '        '   current authoring record.
        '        '========================================================================
        '        '
        '        Public Function csv_GetVirtualFilenameByTable(ByVal TableName As String, ByVal FieldName As String, ByVal RecordID As Integer, ByVal OriginalFilename As String, ByVal fieldType As Integer) As String
        '            On Error GoTo ErrorTrap : 'Const Tn = "GetVirtualFilenameByTable" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim RecordIDString As String
        '            Dim iTableName As String
        '            Dim iFieldName As String
        '            Dim iRecordID As Integer
        '            Dim MethodName As String
        '            Dim iOriginalFilename As String
        '            '
        '            MethodName = "csv_GetVirtualFilenameByTable"
        '            '
        '            iTableName = TableName
        '            iTableName = Replace(iTableName, " ", "_")
        '            iTableName = Replace(iTableName, ".", "_")
        '            '
        '            iFieldName = FieldName
        '            iFieldName = Replace(FieldName, " ", "_")
        '            iFieldName = Replace(iFieldName, ".", "_")
        '            '
        '            iOriginalFilename = OriginalFilename
        '            iOriginalFilename = Replace(iOriginalFilename, " ", "_")
        '            iOriginalFilename = Replace(iOriginalFilename, ".", "_")
        '            '
        '            RecordIDString = CStr(RecordID)
        '            If RecordID = 0 Then
        '                RecordIDString = CStr(getRandomLong)
        '                RecordIDString = New String("0", 12 - Len(RecordIDString)) & RecordIDString
        '            Else
        '                RecordIDString = New String("0", 12 - Len(RecordIDString), "0") & RecordIDString
        '            End If
        '            '
        '            If OriginalFilename <> "" Then
        '                csv_GetVirtualFilenameByTable = iTableName & "/" & iFieldName & "/" & RecordIDString & "/" & OriginalFilename
        '            Else
        '                Select Case fieldType
        '                    Case FieldTypeCSSFile
        '                        csv_GetVirtualFilenameByTable = iTableName & "/" & iFieldName & "/" & RecordIDString & ".css"
        '                    Case FieldTypeXMLFile
        '                        csv_GetVirtualFilenameByTable = iTableName & "/" & iFieldName & "/" & RecordIDString & ".xml"
        '                    Case FieldTypeJavascriptFile
        '                        csv_GetVirtualFilenameByTable = iTableName & "/" & iFieldName & "/" & RecordIDString & ".js"
        '                    Case FieldTypeHTMLFile
        '                        csv_GetVirtualFilenameByTable = iTableName & "/" & iFieldName & "/" & RecordIDString & ".html"
        '                    Case Else
        '                        csv_GetVirtualFilenameByTable = iTableName & "/" & iFieldName & "/" & RecordIDString & ".txt"
        '                End Select
        '            End If
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, MethodName, True)
        '        End Function
        '    '
        '    '========================================================================
        '    '   EncodeSQL
        '    '       encode a variable to go in an sql expression
        '    '       NOT supported
        '    '========================================================================
        '    '
        Public Function EncodeSQL(ByVal expression As Object, Optional ByVal fieldType As Integer = FieldTypeIdText) As String
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
                    EncodeSQL = db_EncodeSQLBoolean(expression)
                Case FieldTypeIdCurrency, FieldTypeIdAutoIdIncrement, FieldTypeIdFloat, FieldTypeIdInteger, FieldTypeIdLookup, FieldTypeIdMemberSelect
                    EncodeSQL = db_EncodeSQLNumber(expression)
                Case FieldTypeIdDate
                    EncodeSQL = db_EncodeSQLDate(expression)
                Case FieldTypeIdLongText, FieldTypeIdHTML
                    EncodeSQL = db_EncodeSQLLongText(expression)
                Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdRedirect, FieldTypeIdManyToMany, FieldTypeIdText, FieldTypeIdFileTextPrivate, FieldTypeIdFileJavascript, FieldTypeIdFileXML, FieldTypeIdFileCSS, FieldTypeIdFileHTMLPrivate
                    EncodeSQL = db_EncodeSQLText(expression)
                Case Else
                    EncodeSQL = db_EncodeSQLText(expression)
                    On Error GoTo 0
                    Call Err.Raise(KmaErrorBase, "dll", "Unknown Field Type [" & fieldType & "] used FieldTypeText.")
            End Select
            '
        End Function

        '
        '========================================================================
        '   encodeSQLText
        '========================================================================
        '
        Public Function db_EncodeSQLText(ByVal expression As Object) As String
            Dim returnString As String = ""
            If expression Is Nothing Then
                returnString = "null"
            Else
                returnString = EncodeText(expression)
                If returnString = "" Then
                    returnString = "null"
                Else
                    returnString = "'" & Replace(returnString, "'", "''") & "'"
                End If
            End If
            Return returnString
        End Function
        '
        '========================================================================
        '   encodeSQLLongText
        '========================================================================
        '
        Public Function db_EncodeSQLLongText(ByVal expression As Object) As String
            Dim returnString As String = ""
            If expression Is Nothing Then
                returnString = "null"
            Else
                returnString = EncodeText(expression)
                If returnString = "" Then
                    returnString = "null"
                Else
                    returnString = "'" & Replace(returnString, "'", "''") & "'"
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
        Public Function db_EncodeSQLDate(ByVal expression As Object) As String
            Dim returnString As String = ""
            Dim expressionDate As Date = Date.MinValue
            If expression Is Nothing Then
                returnString = "null"
            ElseIf Not IsDate(expression) Then
                returnString = "null"
            Else
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
            End If
            Return returnString
        End Function
        '
        '========================================================================
        '   encodeSQLNumber
        '       encode a number variable to go in an sql expression
        '========================================================================
        '
        Public Function db_EncodeSQLNumber(ByVal expression As Object) As String
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
            ElseIf Not IsNumeric(expression) Then
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
        Public Function db_EncodeSQLBoolean(ByVal ExpressionVariant As Object) As String
            '
            'Dim src As String
            '
            db_EncodeSQLBoolean = SQLFalse
            If EncodeBoolean(ExpressionVariant) Then
                db_EncodeSQLBoolean = SQLTrue
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
        Public Function csv_GetVirtualFilename(ByVal ContentName As String, ByVal FieldName As String, ByVal RecordID As Integer, Optional ByVal OriginalFilename As String = "") As String
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
            csv_GetVirtualFilename = ""
            CDef = metaData.getCdef(ContentName)
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
                csv_GetVirtualFilename = csv_GetVirtualFilenameByTable(TableName, FieldName, RecordID, iOriginalFilename, fieldTypeId)
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
        Public Function csv_OpenCSGroupUsers(ByVal GroupName As String, ByVal IsList As Boolean, Optional ByVal sqlCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As Integer
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
            rightNow = Now
            sqlRightNow = db_EncodeSQLDate(rightNow)
            If PageNumber = 0 Then
                PageNumber = 1
            End If
            If PageSize = 0 Then
                PageSize = csv_DefaultPageSize
            End If
            '
            csv_OpenCSGroupUsers = -1
            If GroupName <> "" Then
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
                If Not IsList Then
                    '
                    ' single group name
                    '
                    GroupName = Trim(GroupName)
                    SQL &= "" _
                        & "AND(ccGroups.Name=" & db_EncodeSQLText(GroupName) & ")" _
                        & ""
                Else
                    '
                    ' list of names
                    '
                    SQL &= "" _
                        & "AND(ccGroups.Name In (" & Replace(db_EncodeSQLText(GroupName), ",", "','") & "))" _
                        & ""
                End If
                '
                ' group expiration
                '
                SQL &= "" _
                    & "AND((ccMemberRules.DateExpires Is Null) Or (ccMemberRules.DateExpires>" & sqlRightNow & "))" _
                    & ""
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
                csv_OpenCSGroupUsers = db_openCsSql_rev("default", SQL, PageSize, PageNumber)
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
        Public Function csv_GetContentTableID(ByVal ContentName As String) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "GetContentTableID" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CDef As coreMetaDataClass.CDefClass
            Dim rs As DataTable
            Dim SQL As String
            '
            SQL = "select ContentTableID from ccContent where name=" & db_EncodeSQLText(ContentName) & ";"
            rs = executeSql(SQL)
            If Not isDataTableOk(rs) Then
                cpCore.handleLegacyError2("cpCoreClass", "csv_GetContentTableID", config.name & ", Content [" & ContentName & "] was not found in ccContent table")
            Else
                csv_GetContentTableID = EncodeInteger(rs.Rows(0).Item("ContentTableID"))
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            cpCore.handleException(New Exception("Unexpected exception"))
        End Function
        '
        '=====================================================================================================
        '   returns true if another user has the record locked
        '=====================================================================================================
        '
        Public Function csv_IsRecordLocked(ByVal ContentName As String, ByVal RecordID As Integer, ByVal MemberID As Integer) As Boolean
            On Error GoTo ErrorTrap 'Const Tn = "csv_IsRecordLocked" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim TableID As Integer
            Dim Criteria As String
            Dim CS As Integer
            '
            Criteria = csv_GetAuthoringControlCriteria(ContentName, RecordID) & "and(CreatedBy<>" & db_EncodeSQLNumber(MemberID) & ")"
            CS = db_csOpen("Authoring Controls", Criteria, , , MemberID)
            csv_IsRecordLocked = db_csOk(CS)
            Call db_csClose(CS)
            '
            Exit Function
            '
ErrorTrap:
            cpCore.handleException(New Exception("Unexpected exception"))
        End Function
        '
        '=====================================================================================================
        '   returns true if another user has the record locked
        '=====================================================================================================
        '
        Private Function csv_GetAuthoringControlCriteria(ByVal ContentName As String, ByVal RecordID As Integer) As String
            On Error GoTo ErrorTrap 'Const Tn = "GetAuthoringControlCriteria" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            Dim CSPointer As Integer
            Dim ContentCnt As Integer
            Dim CS As Integer
            Dim ContentID As Integer
            Dim Criteria As String
            Dim TableID As Integer
            '
            MethodName = "csv_GetAuthoringControlCriteria"
            '
            TableID = csv_GetContentTableID(ContentName)
            '
            ' Authoring Control records are referenced by ContentID
            '
            ContentCnt = 0
            CS = db_csOpen("Content", "(contenttableid=" & TableID & ")")
            Do While db_csOk(CS)
                Criteria = Criteria & "," & db_GetCSInteger(CS, "ID")
                ContentCnt = ContentCnt + 1
                Call db_csGoNext(CS)
            Loop
            Call db_csClose(CS)
            If ContentCnt < 1 Then
                '
                ' No references to this table
                '
                Call handleLegacyClassError4(KmaErrorInternal, "dll", "TableID [" & TableID & "] could not be found in any ccContent.ContentTableID", "csv_GetAuthoringControlCriteria", False, True)
                csv_GetAuthoringControlCriteria = "(1=0)"
            ElseIf ContentCnt = 1 Then
                '
                ' One content record
                '
                csv_GetAuthoringControlCriteria = "(ContentID=" & db_EncodeSQLNumber(Mid(Criteria, 2)) & ")and(RecordID=" & db_EncodeSQLNumber(RecordID) & ")and((DateExpires>" & db_EncodeSQLDate(Now) & ")or(DateExpires is null))"
            Else
                '
                ' Multiple content records
                '
                csv_GetAuthoringControlCriteria = "(ContentID in (" & Mid(Criteria, 2) & "))and(RecordID=" & db_EncodeSQLNumber(RecordID) & ")and((DateExpires>" & db_EncodeSQLDate(Now) & ")or(DateExpires is null))"
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function
        '
        '=====================================================================================================
        '   Clear the Approved Authoring Control
        '=====================================================================================================
        '
        Public Sub csv_ClearAuthoringControl(ByVal ContentName As String, ByVal RecordID As Integer, ByVal AuthoringControl As Integer, ByVal MemberID As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-136" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            'Dim ContentID as integer
            Dim TableID As Integer
            Dim Criteria As String
            '
            MethodName = "csv_ClearAuthoringControl"
            '
            'ContentID = csv_GetContentID(ContentName)
            Criteria = csv_GetAuthoringControlCriteria(ContentName, RecordID) & "and(ControlType=" & AuthoringControl & ")"

            'If ContentID > -1 Then
            Select Case AuthoringControl
                Case AuthoringControlsEditing
                    Call db_DeleteContentRecords("Authoring Controls", Criteria & "and(CreatedBy=" & db_EncodeSQLNumber(MemberID) & ")", MemberID)
                    'Call csv_DeleteContentRecords("Authoring Controls", "(ControlType=" & AuthoringControl & ")and(ContentID=" & encodeSQLNumber(ContentID) & ")and(RecordID=" & encodeSQLNumber(RecordID) & ")and(CreatedBy=" & encodeSQLNumber(MemberID) & ")", MemberID)
                Case AuthoringControlsSubmitted, AuthoringControlsApproved, AuthoringControlsModified
                    Call db_DeleteContentRecords("Authoring Controls", Criteria, MemberID)
                    'Call csv_DeleteContentRecords("Authoring Controls", "(ControlType=" & AuthoringControl & ")and(ContentID=" & encodeSQLNumber(ContentID) & ")and(RecordID=" & encodeSQLNumber(RecordID) & ")", MemberID)
            End Select
            'End If
            '
            Exit Sub
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Sub
        '
        '=====================================================================================================
        '   the Approved Authoring Control
        '=====================================================================================================
        '
        Public Sub csv_SetAuthoringControl(ByVal ContentName As String, ByVal RecordID As Integer, ByVal AuthoringControl As Integer, ByVal MemberID As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-137" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            Dim ContentID As Integer
            'Dim TableID as integer
            Dim AuthoringCriteria As String
            Dim CSNewLock As Integer
            Dim CSCurrentLock As Integer
            Dim sqlCriteria As String
            Dim EditLockTimeoutDays As Double
            Dim EditLockTimeoutMinutes As Double
            Dim CDef As coreMetaDataClass.CDefClass
            '
            MethodName = "csv_SetAuthoringControl"
            '
            CDef = metaData.getCdef(ContentName)
            ContentID = CDef.Id
            If ContentID <> 0 Then
                'TableID = csv_GetContentTableID(ContentName)
                AuthoringCriteria = csv_GetAuthoringControlCriteria(ContentName, RecordID)
                Select Case AuthoringControl
                    Case AuthoringControlsEditing
                        EditLockTimeoutMinutes = EncodeNumber(siteProperty_getText("EditLockTimeout", "5"))
                        If (EditLockTimeoutMinutes <> 0) Then
                            sqlCriteria = AuthoringCriteria & "and(ControlType=" & AuthoringControlsEditing & ")"
                            EditLockTimeoutDays = (EditLockTimeoutMinutes / 60 / 24)
                            '
                            ' Delete expired locks
                            '
                            Call db_DeleteContentRecords("Authoring Controls", sqlCriteria & "AND(DATEEXPIRES<" & db_EncodeSQLDate(Now) & ")")
                            '
                            ' Select any lock left, only the newest counts
                            '
                            CSCurrentLock = db_csOpen("Authoring Controls", sqlCriteria, "ID DESC", , MemberID, False, False)
                            If Not db_csOk(CSCurrentLock) Then
                                '
                                ' No lock, create one
                                '
                                CSNewLock = db_csInsertRecord("Authoring Controls", MemberID)
                                If db_csOk(CSNewLock) Then
                                    Call db_SetCSField(CSNewLock, "RecordID", RecordID)
                                    Call db_SetCSField(CSNewLock, "DateExpires", (Now.AddDays(EditLockTimeoutDays)))
                                    Call db_SetCSField(CSNewLock, "ControlType", AuthoringControlsEditing)
                                    Call db_SetCSField(CSNewLock, "ContentRecordKey", EncodeText(ContentID & "." & RecordID))
                                    Call db_SetCSField(CSNewLock, "ContentID", ContentID)
                                End If
                                Call db_csClose(CSNewLock)
                            Else
                                If (db_GetCSInteger(CSCurrentLock, "CreatedBy") = MemberID) Then
                                    '
                                    ' Record Locked by Member, update DateExpire
                                    '
                                    Call db_SetCSField(CSCurrentLock, "DateExpires", (Now.AddDays(EditLockTimeoutDays)))
                                End If
                            End If
                            Call db_csClose(CSCurrentLock)
                        End If
                    Case AuthoringControlsSubmitted, AuthoringControlsApproved, AuthoringControlsModified
                        If CDef.AllowWorkflowAuthoring And siteProperty_AllowWorkflowAuthoring Then
                            sqlCriteria = AuthoringCriteria & "and(ControlType=" & AuthoringControl & ")"
                            CSCurrentLock = db_csOpen("Authoring Controls", sqlCriteria, "ID DESC", , MemberID, False, False)
                            If Not db_csOk(CSCurrentLock) Then
                                '
                                ' Create new lock
                                '
                                CSNewLock = db_csInsertRecord("Authoring Controls", MemberID)
                                If db_csOk(CSNewLock) Then
                                    Call db_SetCSField(CSNewLock, "RecordID", RecordID)
                                    Call db_SetCSField(CSNewLock, "ControlType", AuthoringControl)
                                    Call db_SetCSField(CSNewLock, "ContentRecordKey", EncodeText(ContentID & "." & RecordID))
                                    Call db_SetCSField(CSNewLock, "ContentID", ContentID)
                                End If
                                Call db_csClose(CSNewLock)
                            Else
                                '
                                ' Update current lock
                                '
                                'Call csv_SetCSField(CSCurrentLock, "ContentID", ContentID)
                                'Call csv_SetCSField(CSCurrentLock, "RecordID", RecordID)
                                'Call csv_SetCSField(CSCurrentLock, "ControlType", AuthoringControl)
                                'Call csv_SetCSField(CSCurrentLock, "ContentRecordKey", encodeText(ContentID & "." & RecordID))
                            End If
                            Call db_csClose(CSCurrentLock)
                        End If
                End Select
            End If
            '
            Exit Sub
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Sub
        '
        '=====================================================================================================
        '   the Approved Authoring Control
        '=====================================================================================================
        '
        Public Sub csv_GetAuthoringStatus(ByVal ContentName As String, ByVal RecordID As Integer, ByRef IsSubmitted As Boolean, ByRef IsApproved As Boolean, ByRef SubmittedName As String, ByRef ApprovedName As String, ByRef IsInserted As Boolean, ByRef IsDeleted As Boolean, ByRef IsModified As Boolean, ByRef ModifiedName As String, ByRef ModifiedDate As Date, ByRef SubmittedDate As Date, ByRef ApprovedDate As Date)
            On Error GoTo ErrorTrap 'Const Tn = "csv_GetAuthoringStatus" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            '
            MethodName = "csv_GetAuthoringStatus"
            '
            Dim SQL As String
            Dim CSLocks As Integer
            Dim ContentID As Integer
            Dim Criteria As String
            Dim ControlType As Integer
            Dim EditMemberID As Integer
            Dim rs As DataTable
            Dim ContentTableName As String
            Dim AuthoringTableName As String
            Dim DataSourceName As String
            Dim CDef As coreMetaDataClass.CDefClass
            Dim CS As Integer
            Dim EditingMemberID As Integer
            'Dim TableID as integer
            '
            IsModified = False
            ModifiedName = ""
            ModifiedDate = Date.MinValue
            IsSubmitted = False
            SubmittedName = ""
            SubmittedDate = Date.MinValue
            IsApproved = False
            ApprovedName = ""
            ApprovedDate = Date.MinValue
            IsInserted = False
            IsDeleted = False
            If RecordID > 0 Then
                '
                ' Get Workflow Locks
                '
                CDef = metaData.getCdef(ContentName)
                ContentID = CDef.Id
                If ContentID > 0 Then
                    If CDef.AllowWorkflowAuthoring And siteProperty_AllowWorkflowAuthoring Then
                        '
                        ' Check Authoring Controls record for Locks
                        '
                        'TableID = csv_GetContentTableID(ContentName)
                        Criteria = csv_GetAuthoringControlCriteria(ContentName, RecordID)
                        CSLocks = db_csOpen("Authoring Controls", Criteria, "DateAdded Desc", , , , , "DateAdded,ControlType,CreatedBy,ID,DateExpires")
                        Do While db_csOk(CSLocks)
                            ControlType = db_GetCSInteger(CSLocks, "ControlType")
                            Select Case ControlType
                                Case AuthoringControlsModified
                                    If Not IsModified Then
                                        ModifiedDate = db_GetCSDate(CSLocks, "DateAdded")
                                        ModifiedName = db_GetCS(CSLocks, "CreatedBy")
                                        IsModified = True
                                    End If
                                Case AuthoringControlsSubmitted
                                    If Not IsSubmitted Then
                                        SubmittedDate = db_GetCSDate(CSLocks, "DateAdded")
                                        SubmittedName = db_GetCS(CSLocks, "CreatedBy")
                                        IsSubmitted = True
                                    End If
                                Case AuthoringControlsApproved
                                    If Not IsApproved Then
                                        ApprovedDate = db_GetCSDate(CSLocks, "DateAdded")
                                        ApprovedName = db_GetCS(CSLocks, "CreatedBy")
                                        IsApproved = True
                                    End If
                            End Select
                            db_csGoNext(CSLocks)
                        Loop
                        Call db_csClose(CSLocks)
                        '
                        ContentTableName = CDef.ContentTableName
                        AuthoringTableName = CDef.AuthoringTableName
                        DataSourceName = CDef.ContentDataSourceName
                        SQL = "SELECT ContentTableName.ID AS LiveRecordID, AuthoringTableName.ID AS EditRecordID, AuthoringTableName.EditBlank AS IsDeleted, ContentTableName.EditBlank AS IsInserted, AuthoringTableName.ModifiedDate AS EditRecordModifiedDate, ContentTableName.ModifiedDate AS LiveRecordModifiedDate" _
                            & " FROM " & AuthoringTableName & " AS AuthoringTableName RIGHT JOIN " & ContentTableName & " AS ContentTableName ON AuthoringTableName.EditSourceID = ContentTableName.ID" _
                            & " Where (((ContentTableName.ID) = " & RecordID & "))" _
                            & " ORDER BY AuthoringTableName.ID DESC;"
                        rs = executeSql(SQL, DataSourceName)
                        If isDataTableOk(rs) Then
                            IsInserted = EncodeBoolean(db_getDataRowColumnName(rs.Rows(0), "IsInserted"))
                            IsDeleted = EncodeBoolean(db_getDataRowColumnName(rs.Rows(0), "IsDeleted"))
                            'IsModified = (getDataRowColumnName(RS.rows(0), "LiveRecordModifiedDate") <> getDataRowColumnName(RS.rows(0), "EditRecordModifiedDate"))
                        End If
                        Call closeDataTable(rs)
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
        '=================================================================================
        '   Depricate this
        '   Instead, use csv_IsRecordLocked2, which uses the Db
        '=================================================================================
        '
        Public Sub csv_SetEditLock(ByVal ContentName As String, ByVal RecordID As Integer, ByVal MemberID As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-160" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Call SetEditLock(ContentName, RecordID, MemberID, False)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_SetEditLock", True)
        End Sub
        '
        '=================================================================================
        '   Depricate this
        '   Instead, use csv_IsRecordLocked, which uses the Db
        '=================================================================================
        '
        Public Sub csv_ClearEditLock(ByVal ContentName As String, ByVal RecordID As Integer, ByVal MemberID As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-161" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Call SetEditLock(ContentName, RecordID, MemberID, True)
            '
            'Call csv_ClearAuthoringControl(ContentName, RecordID, AuthoringControlsEditing, MemberID)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_ClearEditLock", True)
        End Sub
        '
        '=================================================================================
        '   Depricate this
        '   Instead, use csv_IsRecordLocked, which uses the Db
        '=================================================================================
        '
        Public Function csv_GetEditLock(ByVal ContentName As String, ByVal RecordID As Integer, ByRef ReturnMemberID As Integer, ByRef ReturnDateExpires As Date) As Boolean
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-162" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            csv_GetEditLock = GetEditLock(ContentName, RecordID, ReturnMemberID, ReturnDateExpires)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_ClearEditLock", True)
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
            Call csv_DeleteTableRecords(DataSourceName, TableName, "ID=" & RecordID)
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
        Public Sub csv_DeleteContentRules(ByVal ContentID As Integer, ByVal RecordID As Integer)
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
                Call Err.Raise(KmaErrorInternal, "dll", "ContentID [" & ContentID & "] or RecordID [" & RecordID & "] where blank")
            Else
                ContentRecordKey = CStr(ContentID) & "." & CStr(RecordID)
                Criteria = "(ContentRecordKey=" & db_EncodeSQLText(ContentRecordKey) & ")"
                ContentName = csv_GetContentNameByID(ContentID)
                TableName = metaData_GetContentTablename(ContentName)
                '
                ' ----- Delete CalendarEventRules and CalendarEvents
                '
                If metaData_IsContentFieldSupported("calendar events", "ID") Then
                    Call db_DeleteContentRecords("Calendar Events", Criteria)
                End If
                '
                ' ----- Delete ContentWatch
                '
                CS = db_csOpen("Content Watch", Criteria)
                Do While db_csOk(CS)
                    Call csv_DeleteCSRecord(CS)
                    db_csGoNext(CS)
                Loop
                Call db_csClose(CS)
                '
                ' ----- Table Specific rules
                '
                Select Case UCase(TableName)
                    Case "CCCALENDARS"
                        '
                        Call db_DeleteContentRecords("Calendar Event Rules", "CalendarID=" & RecordID)
                    Case "CCCALENDAREVENTS"
                        '
                        Call db_DeleteContentRecords("Calendar Event Rules", "CalendarEventID=" & RecordID)
                    Case "CCCONTENT"
                        '
                        Call db_DeleteContentRecords("Group Rules", "ContentID=" & RecordID)
                    Case "CCCONTENTWATCH"
                        '
                        Call db_DeleteContentRecords("Content Watch List Rules", "Contentwatchid=" & RecordID)
                    Case "CCCONTENTWATCHLISTS"
                        '
                        Call db_DeleteContentRecords("Content Watch List Rules", "Contentwatchlistid=" & RecordID)
                    Case "CCGROUPS"
                        '
                        Call db_DeleteContentRecords("Group Rules", "GroupID=" & RecordID)
                        Call db_DeleteContentRecords("Library Folder Rules", "GroupID=" & RecordID)
                        Call db_DeleteContentRecords("Member Rules", "GroupID=" & RecordID)
                        Call db_DeleteContentRecords("Page Content Block Rules", "GroupID=" & RecordID)
                        Call db_DeleteContentRecords("Path Rules", "GroupID=" & RecordID)
                    Case "CCLIBRARYFOLDERS"
                        '
                        Call db_DeleteContentRecords("Library Folder Rules", "FolderID=" & RecordID)
                    Case "CCMEMBERS"
                        '
                        Call db_DeleteContentRecords("Member Rules", "MemberID=" & RecordID)
                        Call db_DeleteContentRecords("Topic Habits", "MemberID=" & RecordID)
                        Call db_DeleteContentRecords("Member Topic Rules", "MemberID=" & RecordID)
                    Case "CCPAGECONTENT"
                        '
                        Call db_DeleteContentRecords("Page Content Block Rules", "RecordID=" & RecordID)
                        Call db_DeleteContentRecords("Page Content Topic Rules", "PageID=" & RecordID)
                    Case "CCPATHS"
                        '
                        Call db_DeleteContentRecords("Path Rules", "PathID=" & RecordID)
                    Case "CCSURVEYQUESTIONS"
                        '
                        Call db_DeleteContentRecords("Survey Results", "QuestionID=" & RecordID)
                    Case "CCSURVEYS"
                        '
                        Call db_DeleteContentRecords("Survey Questions", "SurveyID=" & RecordID)
                    Case "CCTOPICS"
                        '
                        Call db_DeleteContentRecords("Topic Habits", "TopicID=" & RecordID)
                        Call db_DeleteContentRecords("Page Content Topic Rules", "TopicID=" & RecordID)
                        Call db_DeleteContentRecords("Member Topic Rules", "TopicID=" & RecordID)
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
        Public Sub csv_SaveCSRecord(ByVal CSPointer As Integer, Optional ByVal AsyncSave As Boolean = False, Optional ByVal Blockcsv_ClearBake As Boolean = False)
            Call db_SaveCS(CSPointer, AsyncSave, Blockcsv_ClearBake)
        End Sub

        '
        '========================================================================
        ' ----- Get the SQL value for the true state of a boolean
        '========================================================================
        '
        Private Function csv_GetSQLTrue(ByVal DataSourceName As String) As Integer
            csv_GetSQLTrue = 1
            '            On Error GoTo ErrorTrap : 'Const Tn = "MethodName-045" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '            '
            '            Dim MethodName As String
            '            Dim Pointer As Integer
            '            '
            '            MethodName = "csv_GetSQLTrue"
            '            '
            '            Pointer = csv_GetDataSourcePointer(DataSourceName)
            '            If Pointer < 0 Then
            '                Call csv_HandleClassTrapError(KmaErrorInternal, "dll", "DataSource [" & DataSourceName & "] was not found", MethodName, True)
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
        '                cdefPtr = metaData.metaCache.cdefContentNameIndex.getPtr(ContentName)
        '                If cdefPtr = -1 Then
        '                    Call metaData.getCdef(ContentName)
        '                    cdefPtr = metaData.metaCache.cdefContentNameIndex.getPtr(ContentName)
        '                End If
        '                FieldPointer = metaData.metaCache.cdefContentFieldIndex(cdefPtr).getPtr(iFieldName)
        '                If FieldPointer = -1 Then
        '                    If Not metaData.metaCache.cdefContentFieldIndexPopulated(cdefPtr) Then
        '                        With metaData.metaCache.cdef(cdefPtr)
        '                            For Ptr = 0 To (.fields.Count - 1)
        '                                Dim arrayOfFields As appServices_metaDataClass.CDefFieldClass()
        '                                arrayOfFields = .fields
        '                                Call metaData.metaCache.cdefContentFieldIndex(cdefPtr).setPtr(arrayOfFields(Ptr).Name, Ptr)
        '                                'Call properties.contentFieldPtrIndex(cdfptr).SetPointer(.Fields(Ptr).Name, Ptr)
        '                            Next
        '                        End With
        '                        metaData.metaCache.cdefContentFieldIndexPopulated(cdefPtr) = True
        '                        FieldPointer = metaData.metaCache.cdefContentFieldIndex(cdefPtr).getPtr(iFieldName)
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
        Public Function csv_GetCSRows(ByVal CSPointer As Integer) As String(,)
            csv_GetCSRows = csv_GetCSRows2(CSPointer)
        End Function
        '
        ' try declaring the return as object() - an array holder for variants
        ' try setting up each call to return a variant, not an array of variants
        '
        Friend Function csv_GetCSRows2(ByVal CSPointer As Integer) As String(,)
            On Error GoTo ErrorTrap 'Const Tn = "csv_GetCSRows2" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            Dim rs As DataTable
            Dim hint As String
            Dim returnOK As Boolean
            '
            If useCSReadCacheMultiRow Then
                csv_GetCSRows2 = csv_ContentSet(CSPointer).readCache
            Else
                '
                'hint = "csv_GetCSRows2"
                returnOK = False
                If db_csOk(CSPointer) Then
                    'hint = hint & ",10"
                    If isDataTableOk(csv_ContentSet(CSPointer).dt) Then
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
                            rs = executeSql(csv_ContentSet(CSPointer).Source, csv_ContentSet(CSPointer).DataSource)
                            If isDataTableOk(rs) Then
                                csv_GetCSRows2 = convertDataTabletoArray(rs)
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
        Public Function csv_GetCSRow(ByVal CSPointer As Integer) As Object
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-119" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim Row() As String
            Dim ColumnPointer As Integer
            '
            If db_csOk(CSPointer) Then
                With csv_ContentSet(CSPointer)
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
                    csv_GetCSRow = Row
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
        Public Function csv_GetCSRowCount(ByVal CSPointer As Integer) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "csv_GetCSRowCount" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            '
            MethodName = "csv_GetCSRowCount"
            '
            If CSPointer > 0 Then
                If useCSReadCacheMultiRow Then
                    csv_GetCSRowCount = csv_ContentSet(CSPointer).readCacheRowCnt
                Else
                    If csv_ContentSet(CSPointer).IsOpen Then

                        csv_GetCSRowCount = csv_ContentSet(CSPointer).dt.Rows.Count
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
        Public Function csv_GetCSRowFields(ByVal CSPointer As Integer) As Object
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-121" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            '
            MethodName = "csv_GetCSRowFields"
            '
            If Not db_csOk(CSPointer) Then
                Call handleLegacyClassError4(KmaErrorUser, "dll", "csv_ContentSet is not valid or End-of-File", MethodName, False, False)
            Else
                csv_GetCSRowFields = csv_ContentSet(CSPointer).fieldNames
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
        '=============================================================================
        '   Return just the copy from a content page
        '=============================================================================
        '
        Public Function csv_TextDeScramble(ByVal Copy As String) As String
            On Error GoTo ErrorTrap 'Const Tn = "TextDeScramble" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CS As Integer
            Dim CPtr As Integer
            Dim C As String
            Dim CValue As Integer
            Dim crc As Integer
            Dim ModAnswer As String
            Dim Source As String
            Dim Base As Integer
            Const CMin = 32
            Const CMax = 126
            '
            ' assume this one is not converted
            '
            Source = Copy
            Base = 50
            '
            ' First characger must be _
            ' Second character is the scramble version 'a' is the starting system
            '
            If Mid(Source, 1, 2) <> "_a" Then
                csv_TextDeScramble = Copy
            Else
                Source = Mid(Source, 3)
                '
                ' cycle through all characters
                '
                For CPtr = Len(Source) - 1 To 1 Step -1
                    C = Mid(Source, CPtr, 1)
                    CValue = Asc(C)
                    crc = crc + CValue
                    If (CValue < CMin) Or (CValue > CMax) Then
                        '
                        ' if out of ascii bounds, just leave it in place
                        '
                    Else
                        CValue = CValue - Base
                        If CValue < CMin Then
                            CValue = CValue + CMax - CMin + 1
                        End If
                    End If
                    csv_TextDeScramble = csv_TextDeScramble & Chr(CValue)
                Next
                '
                ' Test mod
                '
                If CStr(crc Mod 9) <> Mid(Source, Len(Source), 1) Then
                    '
                    ' Nope - set it back to the input
                    '
                    csv_TextDeScramble = Copy
                End If
            End If
            '
            'csv_TextDeScramble = Mid(Source, 2)
            '
            Exit Function
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_TextDeScramble", True)
        End Function

        '
        '=============================================================================
        '   Return just the copy from a content page
        '=============================================================================
        '
        Public Function csv_TextScramble(ByVal Copy As String) As String
            On Error GoTo ErrorTrap 'Const Tn = "TextScramble" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CS As Integer
            Dim CPtr As Integer
            Dim C As String
            Dim CValue As Integer
            Dim crc As Integer
            Dim Base As Integer
            Const CMin = 32
            Const CMax = 126
            '
            ' scrambled starts with _
            '
            Base = 50
            For CPtr = 1 To Len(Copy)
                C = Mid(Copy, CPtr, 1)
                CValue = Asc(C)
                If (CValue < CMin) Or (CValue > CMax) Then
                    '
                    ' if out of ascii bounds, just leave it in place
                    '
                Else
                    CValue = CValue + Base
                    If CValue > CMax Then
                        CValue = CValue - CMax + CMin - 1
                    End If
                End If
                '
                ' CRC is addition of all scrambled characters
                '
                crc = crc + CValue
                '
                ' put together backwards
                '
                csv_TextScramble = Chr(CValue) & csv_TextScramble
            Next
            '
            ' Ends with the mod of the CRC and 13
            '
            csv_TextScramble = "_a" & csv_TextScramble & CStr(crc Mod 9)
            '
            '
            Exit Function
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_TextScramble", True)
        End Function
        '
        '========================================================================
        ' csv_DeleteTableRecord
        '========================================================================
        '
        Public Sub csv_DeleteTableRecords(ByVal DataSourceName As String, ByVal TableName As String, ByVal Criteria As String)
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
            Call executeSql(SQL, DataSourceName)
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
        ' Get a Contents Name from the ContentID
        '   Bad ContentID returns blank
        '========================================================================
        '
        Public Function csv_GetContentNameByID(ByVal ContentID As Integer) As String
            Dim returnName As String = ""
            Try
                Dim cdef As coreMetaDataClass.CDefClass
                '
                cdef = metaData.getCdef(ContentID)
                If Not cdef Is Nothing Then
                    returnName = cdef.Name
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnName
        End Function
        '        '
        '        '======================================================================================
        '        ' csv_ExecuteSQL
        '        '
        '        '   All executions from the outside should go through here, not the trapless entry
        '        '======================================================================================
        '        '
        '        Public Function csv_ExecuteSQL(ByVal DataSourceName As String, ByVal SQL As String, Optional ByVal Retries As Integer = 0, Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) as datatable
        '            On Error GoTo ErrorTrap : 'Const Tn = "ExecuteSQL" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim MethodName As String
        '            Dim ErrorTrapMessage As String
        '            Dim ErrorPointer As Integer
        '            Dim ErrorNumber As Integer
        '            Dim ErrorDescription As String
        '            Dim ErrorSource As String
        '            Dim PrimaryErrorNumber As Integer
        '            Dim PrimaryErrorDescription As String
        '            Dim PrimaryErrorSource As String
        '            Dim ErrorSQL As String
        '            Dim ExecuteSuccess As Boolean
        '            Dim Attempts As Integer
        '            Dim DataSourcePointer As Integer
        '            Dim UseServerCursor As Boolean
        '            '
        '            MethodName = "csv_ExecuteSQL"
        '            '
        '            UseServerCursor = (csv_GetDataSourceDefaultCursorLocation(DataSourceName) = CursorLocationEnum.adUseServer)
        '            csv_ExecuteSQL = executeSql(SQL,DataSourceName,  csv_SQLTimeout, PageSize, PageNumber, True, UseServerCursor)
        '            '
        '            Exit Function
        '            '
        'ErrorTrap_Retry:
        '            If PrimaryErrorNumber = 0 Then
        '                PrimaryErrorNumber = Err.Number
        '                PrimaryErrorDescription = Err.Description
        '                PrimaryErrorSource = Err.Source
        '            End If
        '            Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, "csv_ExecuteSQL " & ErrorTrapMessage & ", [" & Attempts - 1 & "] Attempts remaining", True, True)
        '            ExecuteSuccess = False
        '            Resume Next
        '            Exit Function
        '            '
        'ErrorTrap:
        '            Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, "csv_ExecuteSQL", True, False)
        '        End Function
        '        '
        '        '========================================================================
        '        ' Execute a sql on the DataSource provided, return the recordset
        '        '   DataSources must already be loaded.
        '        '   To load datasources, run the SQL directly on the Datasource object
        '        '
        '        '   Note: This routine has no Error Handler. Errors will raise to the caller.
        '        '       So it can not be visible at the class level
        '        '========================================================================
        '        '
        '        Public Function csv_ExecuteSQLCommand(ByVal DataSourceName As String, ByVal SQL As String, Optional ByVal CommandTimeout As Integer = 0, Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) as datatable
        '            On Error GoTo ErrorTrap : 'Const Tn = "ExecuteSQLCommand" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            Dim ErrNumber As Integer
        '            Dim ErrSource As String
        '            Dim ErrDescription As String

        '            csv_ExecuteSQLCommand = executeSql(SQL,DataSourceName,  CommandTimeout, PageSize, PageNumber, True, True)
        '            Exit Function
        '            '
        'ErrorTrap:
        '            ErrNumber = Err.Number
        '            ErrSource = Err.Source
        '            ErrDescription = Err.Description
        '            ' sql and datasource were already added to description in csv_OpenRSSQL_Internal
        '            'ErrDescription = Err.Description & ", executing SQL [" & SQL & "] on Datasource [" & DataSourceName & "]"
        '            Call Err.Raise(ErrNumber, ErrSource, ErrDescription)
        '        End Function
        '
        '=============================================================================
        ' Create a child content from a parent content
        '
        '   If child does not exist, copy everything from the parent
        '   If child already exists, add any missing fields from parent
        '=============================================================================
        '
        Public Sub metaData_CreateContentChild(ByVal ChildContentName As String, ByVal ParentContentName As String, ByVal MemberID As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-037" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            'Dim GUIDGenerator As guidClass
            Dim DataSourceName As String = ""
            Dim MethodName As String
            Dim SQL As String
            Dim rs As DataTable
            Dim ChildContentID As Integer
            Dim ParentContentID As Integer
            'Dim StateOfAllowContentAutoLoad As Boolean
            Dim CSContent As Integer
            Dim CSNew As Integer
            Dim SelectFieldList As String
            Dim Fields() As String
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim FieldName As String
            Dim DateNow As Date
            '
            DateNow = Date.MinValue
            '
            MethodName = "csv_CreateContentChild"
            '
            ' ----- Prevent StateOfAllowContentAutoLoad
            '
            'StateOfAllowContentAutoLoad = AllowContentAutoLoad
            'AllowContentAutoLoad = False
            '
            ' ----- check if child already exists
            '
            SQL = "select ID from ccContent where name=" & db_EncodeSQLText(ChildContentName) & ";"
            rs = executeSql(SQL)
            If isDataTableOk(rs) Then
                ChildContentID = EncodeInteger(db_getDataRowColumnName(rs.Rows(0), "ID"))
                '
                ' mark the record touched so upgrade will not delete it
                '
                Call executeSql("update ccContent set CreateKey=0 where ID=" & ChildContentID)
            End If
            Call closeDataTable(rs)
            If (isDataTableOk(rs)) Then
                If False Then
                    'RS.Dispose)
                End If
                'RS = Nothing
            End If
            If ChildContentID = 0 Then
                '
                ' Get ContentID of parent
                '
                SQL = "select ID from ccContent where name=" & db_EncodeSQLText(ParentContentName) & ";"
                rs = executeSql(SQL, DataSourceName)
                If isDataTableOk(rs) Then
                    ParentContentID = EncodeInteger(db_getDataRowColumnName(rs.Rows(0), "ID"))
                    '
                    ' mark the record touched so upgrade will not delete it
                    '
                    Call executeSql("update ccContent set CreateKey=0 where ID=" & ParentContentID)
                End If
                Call closeDataTable(rs)
                If (isDataTableOk(rs)) Then
                    If False Then
                        'RS.Close()
                    End If
                    'RS = Nothing
                End If
                '
                If ParentContentID = 0 Then
                    Call handleLegacyClassError3("csv_CreateContentChild", "Can not create Child Content [" & ChildContentName & "] because the Parent Content [" & ParentContentName & "] was not found.")
                Else
                    '
                    ' ----- create child content record, let the csv_ExecuteSQL reload CDef
                    '
                    DataSourceName = "Default"
                    CSContent = db_OpenCSContentRecord("Content", ParentContentID)
                    If Not db_csOk(CSContent) Then
                        Call handleLegacyClassError3("csv_CreateContentChild", "Can not create Child Content [" & ChildContentName & "] because the Parent Content [" & ParentContentName & "] was not found.")
                    Else
                        SelectFieldList = db_GetCSSelectFieldList(CSContent)
                        If SelectFieldList = "" Then
                            Call handleLegacyClassError3("csv_CreateContentChild", "Can not create Child Content [" & ChildContentName & "] because the Parent Content [" & ParentContentName & "] record has not fields.")
                        Else
                            CSNew = db_csInsertRecord("Content", 0)
                            If Not db_csOk(CSNew) Then
                                Call handleLegacyClassError3("csv_CreateContentChild", "Can not create Child Content [" & ChildContentName & "] because there was an error creating a new record in ccContent.")
                            Else
                                Fields = Split(SelectFieldList, ",")
                                DateNow = Now()
                                For FieldPointer = 0 To UBound(Fields)
                                    FieldName = Fields(FieldPointer)
                                    Select Case UCase(FieldName)
                                        Case "ID"
                                            ' do nothing
                                        Case "NAME"
                                            Call db_SetCSField(CSNew, FieldName, ChildContentName)
                                        Case "PARENTID"
                                            Call db_SetCSField(CSNew, FieldName, db_GetCSText(CSContent, "ID"))
                                        Case "CREATEDBY", "MODIFIEDBY"
                                            Call db_SetCSField(CSNew, FieldName, MemberID)
                                        Case "DATEADDED", "MODIFIEDDATE"
                                            Call db_SetCSField(CSNew, FieldName, DateNow)
                                        Case "CCGUID"

                                            '
                                            ' new, non-blank guid so if this cdef is exported, it will be updateable
                                            '
                                            Call db_SetCSField(CSNew, FieldName, Guid.NewGuid.ToString())
                                        Case Else
                                            Call db_SetCSField(CSNew, FieldName, db_GetCSText(CSContent, FieldName))
                                    End Select
                                Next
                            End If
                            Call db_csClose(CSNew)
                        End If
                    End If
                    Call db_csClose(CSContent)
                    'SQL = "INSERT INTO ccContent ( Name, Active, DateAdded, CreatedBy, ModifiedBy, ModifiedDate, AllowAdd, DeveloperOnly, AdminOnly, CreateKey, SortOrder, ContentControlID, AllowDelete, ParentID, EditSourceID, EditArchive, EditBlank, ContentTableID, AuthoringTableID, AllowWorkflowAuthoring, DefaultSortMethodID, DropDownFieldList, EditorGroupID )" _
                    '    & " SELECT " & encodeSQLText(ChildContentName) & " AS Name, ccContent.Active, ccContent.DateAdded, ccContent.CreatedBy, ccContent.ModifiedBy, ccContent.ModifiedDate, ccContent.AllowAdd, ccContent.DeveloperOnly, ccContent.AdminOnly, ccContent.CreateKey, ccContent.SortOrder, ccContent.ContentControlID, ccContent.AllowDelete, ccContent.ID, ccContent.EditSourceID, ccContent.EditArchive, ccContent.EditBlank, ccContent.ContentTableID, ccContent.AuthoringTableID, ccContent.AllowWorkflowAuthoring, ccContent.DefaultSortMethodID, ccContent.DropDownFieldList, ccContent.EditorGroupID" _
                    '    & " From ccContent" _
                    '    & " WHERE (((ccContent.ID)=" & encodeSQLNumber(ParentContentID) & "));"
                    'Call csv_ExecuteSQL(sql,DataSourceName)
                End If
            End If
            '
            ' ----- Load CDef
            '
            cache.invalidateAll()
            metaData.clear()
            '
            Exit Sub
            '
            ' ----- Error Trap

            '
ErrorTrap:
            If (isDataTableOk(rs)) Then
                If False Then
                    'RS.Close()
                End If
                'RS = Nothing
            End If
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Sub
        '        '
        '        '========================================================================
        '        '   Track Content by csv_ContentSet
        '        '
        '        '   Increment the viewings record
        '        '   Update the Public URL field
        '        '========================================================================
        '        '
        '        Public Sub csv_TrackContentSet(ByVal CSPointer As Integer, ByVal pathPage As String, ByVal MemberID As Integer)
        '            On Error GoTo ErrorTrap 'Const Tn = "MethodName-082" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            ''dim buildversion As String
        '            Dim topicIds() As String
        '            Dim Ptr As Integer
        '            Dim CS As Integer
        '            Dim score As Integer
        '            Dim SQL As String
        '            Dim TopicIDList As String
        '            Dim rs As DataTable
        '            Dim topicHabitId As Integer
        '            Dim QueryString As String
        '            Dim CSTopicRules As Integer
        '            Dim CSTopicHabits As Integer
        '            Dim TopicScore As Integer
        '            Dim TopicID As Integer
        '            Dim StatePosition As Integer
        '            Dim StateEnd As Integer
        '            Dim CSContentWatch As Integer
        '            Dim ContentID As Integer
        '            Dim RecordID As Integer
        '            Dim MyPathPage As String
        '            Dim MethodName As String
        '            Dim SelectFieldList As String
        '            Dim TopicRulesSelectFieldList As String
        '            Dim TopicHabitsSelectFieldList As String
        '            Dim ContentName As String
        '            Dim ContentTableName As String
        '            '
        '            MethodName = "csv_TrackContentSet"
        '            '
        '            If Not db_IsCSOK(CSPointer) Then
        '                '
        '                Call handleLegacyClassError3(MethodName, ("csv_ContentSet is invalid or End-of-file"))
        '            Else
        '                RecordID = db_GetCSInteger(CSPointer, "ID")
        '                ContentID = db_GetCSInteger(CSPointer, "ContentControlID")
        '                ContentName = csv_GetContentNameByID(ContentID)
        '                ContentTableName = csv_GetContentTablename(ContentName)
        '                ' BuildVersion = dataBuildVersion
        '                '
        '                ' ----- Filter the PathPage to exclude Member Actions and StateStrings
        '                '
        '                MyPathPage = pathPage
        '                'MyPathPage = csv_FilterQueryString(MyPathPage)
        '                '
        '                ' ----- Update current content watch record for the current link address
        '                '
        '                SelectFieldList = "ContentID,RecordID,ContentRecordKey,Clicks,Link"
        '                CSContentWatch = db_OpenCSContent("Content Watch", "(ContentID=" & ContentID & ")AND(recordid=" & EncodeSQLNumber(RecordID) & ")", , , , , , SelectFieldList, 1)
        '                If Not db_IsCSOK(CSContentWatch) Then
        '                    '
        '                    ' ----- Create a new record
        '                    '
        '                    Call db_closeCS(CSContentWatch)
        '                    CSContentWatch = db_InsertCSRecord("Content Watch", MemberID)
        '                    Call db_SetCSField(CSContentWatch, "ContentID", ContentID)
        '                    Call db_SetCSField(CSContentWatch, "RecordID", RecordID)
        '                    Call db_SetCSField(CSContentWatch, "ContentRecordKey", ContentID & "." & RecordID)
        '                End If
        '                If Not db_IsCSOK(CSContentWatch) Then
        '                    '
        '                    Call handleLegacyClassError3(MethodName, ("Could not create Content Watch record"))
        '                Else
        '                    '
        '                    ' ----- Update content watch
        '                    '
        '                    If EncodeBoolean(siteProperty_getText("AllowContentWatchLinkUpdate", 1)) Then
        '                        If EncodeText(db_GetCSField(CSContentWatch, "Link")) <> MyPathPage Then
        '                            Call db_SetCSField(CSContentWatch, "Link", MyPathPage)
        '                        End If
        '                    End If
        '                End If
        '                '
        '                ' ----- update topic habits
        '                '
        '                If (LCase(ContentTableName) = "ccpagecontent") And (EncodeBoolean(siteProperty_getText("AllowTopicHabitUpdate", 1))) Then
        '                    '
        '                    ' ----- increment or create Topic habits for each Topic Rule for this content
        '                    '
        '                    TopicRulesSelectFieldList = "TopicID"
        '                    TopicHabitsSelectFieldList = "Score,TopicID,MemberID,Active"
        '                    '
        '                    SQL = "select topicid from ccpagecontenttopicrules where pageid=" & RecordID
        '                    rs = executeSql(SQL)
        '                    If (isDataTableOk(rs)) Then
        '                        If True Then
        '                            TopicIDList = convertDataTableColumntoItemList(rs)
        '                        End If
        '                        If False Then
        '                            'RS.Close()
        '                        End If
        '                        'RS = Nothing
        '                    End If
        '                    '
        '                    If TopicIDList <> "" Then
        '                        topicIds = Split(TopicIDList, ",")
        '                        For Ptr = 0 To UBound(topicIds)
        '                            TopicID = EncodeInteger(topicIds(Ptr))
        '                            If TopicID <> 0 Then
        '                                score = 0
        '                                topicHabitId = 0
        '                                SQL = "select id,score from cctopichabits where (MemberID=" & EncodeSQLNumber(MemberID) & ")and(TopicID=" & EncodeSQLNumber(TopicID) & ")"
        '                                rs = executeSql(SQL)
        '                                If (isDataTableOk(rs)) Then
        '                                    If Not rs.Rows.Count <= 0 Then
        '                                        topicHabitId = EncodeInteger(rs.Rows(0).Item("id"))
        '                                        score = EncodeInteger(rs.Rows(0).Item("score"))
        '                                    End If
        '                                    If False Then
        '                                        'RS.Close()
        '                                    End If
        '                                    'RS = Nothing
        '                                End If
        '                                '
        '                                If topicHabitId = 0 Then
        '                                    CS = db_InsertCSRecord("topic habits", 0)
        '                                    If db_IsCSOK(CS) Then
        '                                        topicHabitId = db_GetCSInteger(CS, "id")
        '                                        Call db_SetCS(CS, "memberid", MemberID)
        '                                        Call db_SetCS(CS, "TopicID", TopicID)
        '                                    End If
        '                                    Call db_closeCS(CS)
        '                                End If
        '                                '
        '                                If topicHabitId <> 0 Then
        '                                    SQL = "update cctopichabits set score=" & (score + 1) & " where (id=" & topicHabitId & ")"
        '                                    Call executeSql(SQL)
        '                                End If
        '                            End If
        '                        Next
        '                    End If
        '                    '
        '                End If
        '                Call db_closeCS(CSContentWatch, True)
        '            End If
        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        '        End Sub
        ''
        ''========================================================================
        '' Delete an Index for a table
        ''========================================================================
        ''
        'Public Sub csv_DeleteTableIndex(ByVal DataSourceName As String, ByVal TableName As String, ByVal IndexName As String)
        '    Call csv_DeleteTableIndex(DataSourceName, TableName, IndexName)
        'End Sub
        '
        '========================================================================
        '   csv_IsWithinContent( ChildContentID, ParentContentID )
        '
        '       Returns true if ChildContentID is in ParentContentID
        '========================================================================
        '
        Function isChildContent(ByVal ChildContentID As Integer, ByVal ParentContentID As Integer) As Boolean
            Dim returnOk As Boolean = False
            Try
                isChildContent = metaData.isWithinContent(ChildContentID, ParentContentID)
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Function
        '
        '========================================================================
        '   return the content name of a csv_ContentSet
        '========================================================================
        '
        Public Function csv_GetCSContentName(ByVal CSPointer As Integer) As String
            On Error GoTo ErrorTrap 'Const Tn = "GetCSContentName" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            '
            MethodName = "csv_GetCSContentName"
            '
            If CSPointer <> -1 Then
                'If csv_IsCSOK(CSPointer) Then
                csv_GetCSContentName = csv_ContentSet(CSPointer).ContentName
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function
        ''
        ''   Buffered Site Property
        ''
        'Public ReadOnly Property dataBuildVersion() As String
        '    Get

        '    End Get
        'End Property
        '
        '   Buffered Site Property
        '
        Friend ReadOnly Property siteProperty_allowLinkAlias() As Boolean
            Get
                siteProperty_allowLinkAlias = False
                If True Then
                    If Not siteProperty_allowLinkAlias_LocalLoaded Then
                        siteProperty_allowLinkAlias_Local = EncodeBoolean(siteProperty_getText("allowLinkAlias", "1"))
                        siteProperty_allowLinkAlias_LocalLoaded = True
                    End If
                    siteProperty_allowLinkAlias = siteProperty_allowLinkAlias_Local
                End If

            End Get
        End Property
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
                            'Call Err.Raise(KmaErrorUser, "dll", "Unknown FieldType [" & fieldType & "]")
                        End If
                End Select
            Catch ex As Exception
                cpCore.handleException(ex, "Unexpected exception")
            End Try
            Return returnFieldTypeName
        End Function
        '
        '========================================================================
        '   Returns a csv_ContentSet with records from the Definition that joins the
        '       current Definition at the field specified.
        '========================================================================
        '
        Public Function csv_OpenCSJoin(ByVal CSPointer As Integer, ByVal FieldName As String) As Integer
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
            csv_OpenCSJoin = -1
            If Not db_csOk(CSPointer) Then
                '
                Call handleLegacyClassError3(MethodName, ("csv_ContentSet is empty or end of file"))
            Else
                '
                ' csv_ContentSet good
                '
                If Not csv_ContentSet(CSPointer).Updateable Then
                    '
                    ' ----- csv_ContentSet is not updateable (created with an SQL statement)
                    '
                    Call handleLegacyClassError3(MethodName, ("This csv_ContentSet does not support csv_OpenCSJoin. It may have been created from a SQL statement, not a Content Definition."))
                Else
                    '
                    ' ----- csv_ContentSet is updateable
                    '
                    ContentName = csv_ContentSet(CSPointer).ContentName
                    CDef = csv_ContentSet(CSPointer).CDef
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

                        RecordId = db_GetCSInteger(CSPointer, "ID")
                        fieldTypeId = field.fieldTypeId
                        fieldLookupId = field.lookupContentID
                        If fieldTypeId <> FieldTypeIdLookup Then
                            '
                            ' ----- Wrong Field Type
                            '
                            Call handleLegacyClassError3(MethodName, ("csv_OpenCSJoin only supports Content Definition Fields set as 'Lookup' type. Field [ " & FieldName & " ] is not a 'Lookup'."))
                        ElseIf IsNumeric(FieldValueVariant) Then
                            '
                            '
                            '
                            If (fieldLookupId = 0) Then
                                '
                                ' ----- Content Definition for this Lookup was not found
                                '
                                Call handleLegacyClassError3(MethodName, "The Lookup Content Definition [" & fieldLookupId & "] for this field [" & FieldName & "] is not valid.")
                            Else
                                LookupContentName = csv_GetContentNameByID(fieldLookupId)
                                csv_OpenCSJoin = db_csOpen(LookupContentName, "ID=" & db_EncodeSQLNumber(FieldValueVariant), "name", , , , , , 1)
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
        '
        '=================================================================================
        '
        '=================================================================================
        '
        Public Sub csv_PublishEdit(ByVal ContentName As String, ByVal RecordID As Integer, ByVal MemberID As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-140" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim RSLive As DataTable
            Dim LiveRecordID As Integer
            Dim LiveDataSourceName As String
            Dim LiveTableName As String
            Dim LiveSQLValue As String
            Dim LiveRecordBlank As Boolean
            '
            Dim BlankSQLValue As String
            '
            Dim RSEdit As DataTable
            Dim EditDataSourceName As String
            Dim EditTableName As String
            Dim EditRecordID As Integer
            Dim EditSQLValue As String
            Dim EditFilename As String
            Dim EditRecordCID As Integer
            Dim EditRecordBlank As Boolean
            '
            Dim NewEditRecordID As Integer
            Dim NewEditFilename As String
            '
            Dim ContentID As Integer
            '
            Dim FieldNameArray() As String
            Dim ArchiveSqlFieldList As New sqlFieldListClass
            Dim NewEditSqlFieldList As New sqlFieldListClass
            Dim PublishFieldNameArray() As String
            Dim PublishSqlFieldList As New sqlFieldListClass
            Dim BlankSqlFieldList As New sqlFieldListClass
            '
            Dim FieldPointer As Integer
            Dim FieldCount As Integer
            Dim FieldName As String
            Dim fieldTypeId As Integer
            Dim SQL As String
            Dim MethodName As String
            Dim FieldArraySize As Integer
            Dim PublishingDelete As Boolean
            Dim PublishingInactive As Boolean
            Dim CDef As coreMetaDataClass.CDefClass
            Dim FieldList As String
            '
            Dim archiveSqlList As sqlFieldListClass
            '
            MethodName = "csv_PublishEdit"
            '
            CDef = metaData.getCdef(ContentName)
            If CDef.Id > 0 Then
                If CDef.AllowWorkflowAuthoring And siteProperty_AllowWorkflowAuthoring Then
                    With CDef
                        FieldList = .SelectCommaList
                        LiveDataSourceName = .ContentDataSourceName
                        LiveTableName = .ContentTableName
                        EditDataSourceName = .AuthoringDataSourceName
                        EditTableName = .AuthoringTableName
                        FieldCount = .fields.Count
                        ContentID = .Id
                        ContentName = .Name
                    End With
                    FieldArraySize = FieldCount + 6
                    ReDim FieldNameArray(FieldArraySize)
                    'ReDim ArchiveSqlFieldList(FieldArraySize)
                    'ReDim NewEditSqlFieldList(FieldArraySize)
                    'ReDim BlankSqlFieldList(FieldArraySize)
                    'ReDim PublishSqlFieldList(FieldArraySize)
                    ReDim PublishFieldNameArray(FieldArraySize)
                    LiveRecordID = RecordID
                    '
                    ' ----- Open the live record
                    '
                    RSLive = executeSql("SELECT " & FieldList & " FROM " & LiveTableName & " WHERE ID=" & db_EncodeSQLNumber(LiveRecordID) & ";", LiveDataSourceName)
                    'RSLive = appservices.executeSql(LiveDataSourceName, "SELECT " & FieldList & " FROM " & LiveTableName & " WHERE ID=" & encodeSQLNumber(LiveRecordID) & ";")
                    If RSLive.Rows.Count <= 0 Then
                        '
                        Call handleLegacyClassError3(MethodName, ("During record publishing, there was an error opening the live record, [ID=" & LiveRecordID & "] in table [" & LiveTableName & "] on datasource [" & LiveDataSourceName & " ]"))
                    Else
                        If True Then
                            LiveRecordID = EncodeInteger(db_getDataRowColumnName(RSLive.Rows(0), "ID"))
                            LiveRecordBlank = EncodeBoolean(db_getDataRowColumnName(RSLive.Rows(0), "EditBlank"))
                            '
                            ' ----- Open the edit record
                            '
                            RSEdit = executeSql("SELECT " & FieldList & " FROM " & EditTableName & " WHERE (EditSourceID=" & db_EncodeSQLNumber(LiveRecordID) & ")and(EditArchive=0) Order By ID DESC;", EditDataSourceName)
                            'RSEdit = appservices.executeSql(EditDataSourceName, "SELECT " & FieldList & " FROM " & EditTableName & " WHERE (EditSourceID=" & encodeSQLNumber(LiveRecordID) & ")and(EditArchive=0) Order By ID DESC;")
                            If RSEdit.Rows.Count <= 0 Then
                                '
                                Call handleLegacyClassError3(MethodName, ("During record publishing, there was an error opening the edit record [EditSourceID=" & LiveRecordID & "] in table [" & EditTableName & "] on datasource [" & EditDataSourceName & " ]"))
                            Else
                                If True Then
                                    EditRecordID = EncodeInteger(db_getDataRowColumnName(RSEdit.Rows(0), "ID"))
                                    EditRecordCID = EncodeInteger(db_getDataRowColumnName(RSEdit.Rows(0), "ContentControlID"))
                                    EditRecordBlank = EncodeBoolean(db_getDataRowColumnName(RSEdit.Rows(0), "EditBlank"))
                                    PublishingDelete = EditRecordBlank
                                    PublishingInactive = Not EncodeBoolean(db_getDataRowColumnName(RSEdit.Rows(0), "active"))
                                    '
                                    ' ----- Create new Edit record
                                    '
                                    If Not PublishingDelete Then
                                        NewEditRecordID = db_InsertTableRecordGetID(EditDataSourceName, EditTableName, SystemMemberID)
                                        If NewEditRecordID < 1 Then
                                            '
                                            Call handleLegacyClassError3(MethodName, ("During record publishing, a new edit record could not be create, table [" & EditTableName & "] on datasource [" & EditDataSourceName & " ]"))
                                        End If
                                    End If
                                    If True Then
                                        '
                                        ' ----- create update arrays
                                        '
                                        FieldPointer = 0
                                        For Each keyValuePair As KeyValuePair(Of String, coreMetaDataClass.CDefFieldClass) In CDef.fields
                                            Dim field As coreMetaDataClass.CDefFieldClass = keyValuePair.Value
                                            With field
                                                FieldName = .nameLc
                                                fieldTypeId = .fieldTypeId
                                                Select Case fieldTypeId
                                                    Case FieldTypeIdManyToMany, FieldTypeIdRedirect
                                                        '
                                                        ' These content fields have no Db Field
                                                        '
                                                    Case Else
                                                        '
                                                        ' Process These field types
                                                        '
                                                        Select Case UCase(FieldName)
                                                            Case "EDITARCHIVE", "ID", "EDITSOURCEID", "EDITBLANK", "CONTENTCONTROLID"
                                                                '
                                                                ' ----- control fields that should not be in any dataset
                                                                '
                                                            Case "MODIFIEDDATE", "MODIFIEDBY"
                                                                '
                                                                ' ----- non-content fields that need to be in all datasets
                                                                '       add manually later, in case they are not in ContentDefinition
                                                                '
                                                            Case Else
                                                                '
                                                                ' ----- Content related field
                                                                '
                                                                LiveSQLValue = EncodeSQL(db_getDataRowColumnName(RSLive.Rows(0), FieldName), fieldTypeId)
                                                                EditSQLValue = EncodeSQL(db_getDataRowColumnName(RSEdit.Rows(0), FieldName), fieldTypeId)
                                                                BlankSQLValue = EncodeSQL(Nothing, fieldTypeId)
                                                                FieldNameArray(FieldPointer) = FieldName
                                                                '
                                                                ' ----- New Edit Record value
                                                                '
                                                                If Not PublishingDelete Then
                                                                    Select Case fieldTypeId
                                                                        Case FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript
                                                                            '
                                                                            ' ----- cdn files - create copy of File for neweditrecord
                                                                            '
                                                                            EditFilename = EncodeText(db_getDataRowColumnName(RSEdit.Rows(0), FieldName))
                                                                            If EditFilename = "" Then
                                                                                NewEditSqlFieldList.add(FieldName, db_EncodeSQLText(""))
                                                                            Else
                                                                                NewEditFilename = csv_GetVirtualFilenameByTable(EditTableName, FieldName, NewEditRecordID, "", fieldTypeId)
                                                                                'NewEditFilename = csv_GetVirtualFilename(ContentName, FieldName, NewEditRecordID)
                                                                                Call cdnFiles.copyFile(EditFilename, NewEditFilename)
                                                                                NewEditSqlFieldList.add(FieldName, db_EncodeSQLText(NewEditFilename))
                                                                            End If
                                                                        Case FieldTypeIdFileTextPrivate, FieldTypeIdFileHTMLPrivate
                                                                            '
                                                                            ' ----- private files - create copy of File for neweditrecord
                                                                            '
                                                                            EditFilename = EncodeText(db_getDataRowColumnName(RSEdit.Rows(0), FieldName))
                                                                            If EditFilename = "" Then
                                                                                NewEditSqlFieldList.add(FieldName, db_EncodeSQLText(""))
                                                                            Else
                                                                                NewEditFilename = csv_GetVirtualFilenameByTable(EditTableName, FieldName, NewEditRecordID, "", fieldTypeId)
                                                                                'NewEditFilename = csv_GetVirtualFilename(ContentName, FieldName, NewEditRecordID)
                                                                                Call privateFiles.copyFile(EditFilename, NewEditFilename)
                                                                                NewEditSqlFieldList.add(FieldName, db_EncodeSQLText(NewEditFilename))
                                                                            End If
                                                                        Case Else
                                                                            '
                                                                            ' ----- put edit value in new edit record
                                                                            '
                                                                            NewEditSqlFieldList.add(FieldName, EditSQLValue)
                                                                    End Select
                                                                End If
                                                                '
                                                                ' ----- set archive value
                                                                '
                                                                ArchiveSqlFieldList.add(FieldName, LiveSQLValue)
                                                                '
                                                                ' ----- set live record value (and name)
                                                                '
                                                                If PublishingDelete Then
                                                                    '
                                                                    ' ----- Record delete - fill the live record with null
                                                                    '
                                                                    PublishFieldNameArray(FieldPointer) = FieldName
                                                                    PublishSqlFieldList.add(FieldName, BlankSQLValue)
                                                                Else
                                                                    PublishFieldNameArray(FieldPointer) = FieldName
                                                                    PublishSqlFieldList.add(FieldName, EditSQLValue)
                                                                End If
                                                        End Select
                                                End Select
                                            End With
                                            FieldPointer += 1
                                        Next
                                        '
                                        ' ----- create non-content control field entries
                                        '
                                        FieldName = "MODIFIEDDATE"
                                        fieldTypeId = FieldTypeIdDate
                                        FieldNameArray(FieldPointer) = FieldName
                                        LiveSQLValue = EncodeSQL(db_getDataRowColumnName(RSLive.Rows(0), FieldName), fieldTypeId)
                                        EditSQLValue = EncodeSQL(db_getDataRowColumnName(RSEdit.Rows(0), FieldName), fieldTypeId)
                                        ArchiveSqlFieldList.add(FieldName, LiveSQLValue)
                                        NewEditSqlFieldList.add(FieldName, EditSQLValue)
                                        PublishFieldNameArray(FieldPointer) = FieldName
                                        PublishSqlFieldList.add(FieldName, EditSQLValue)
                                        FieldPointer = FieldPointer + 1
                                        '
                                        FieldName = "MODIFIEDBY"
                                        fieldTypeId = FieldTypeIdLookup
                                        FieldNameArray(FieldPointer) = FieldName
                                        LiveSQLValue = EncodeSQL(db_getDataRowColumnName(RSLive.Rows(0), FieldName), fieldTypeId)
                                        EditSQLValue = EncodeSQL(db_getDataRowColumnName(RSEdit.Rows(0), FieldName), fieldTypeId)
                                        ArchiveSqlFieldList.add(FieldName, LiveSQLValue)
                                        NewEditSqlFieldList.add(FieldName, EditSQLValue)
                                        PublishFieldNameArray(FieldPointer) = FieldName
                                        PublishSqlFieldList.add(FieldName, EditSQLValue)
                                        FieldPointer = FieldPointer + 1
                                        '
                                        FieldName = "CONTENTCONTROLID"
                                        fieldTypeId = FieldTypeIdLookup
                                        FieldNameArray(FieldPointer) = FieldName
                                        LiveSQLValue = EncodeSQL(db_getDataRowColumnName(RSLive.Rows(0), FieldName), fieldTypeId)
                                        EditSQLValue = EncodeSQL(db_getDataRowColumnName(RSEdit.Rows(0), FieldName), fieldTypeId)
                                        ArchiveSqlFieldList.add(FieldName, LiveSQLValue)
                                        NewEditSqlFieldList.add(FieldName, EditSQLValue)
                                        PublishFieldNameArray(FieldPointer) = FieldName
                                        If PublishingDelete Then
                                            PublishSqlFieldList.add(FieldName, db_EncodeSQLNumber(0))
                                        Else
                                            PublishSqlFieldList.add(FieldName, EditSQLValue)
                                        End If
                                        FieldPointer = FieldPointer + 1
                                        '
                                        FieldName = "EDITBLANK"
                                        fieldTypeId = FieldTypeIdBoolean
                                        FieldNameArray(FieldPointer) = FieldName
                                        LiveSQLValue = EncodeSQL(db_getDataRowColumnName(RSLive.Rows(0), FieldName), fieldTypeId)
                                        EditSQLValue = EncodeSQL(db_getDataRowColumnName(RSEdit.Rows(0), FieldName), fieldTypeId)
                                        ArchiveSqlFieldList.add(FieldName, LiveSQLValue)
                                        NewEditSqlFieldList.add(FieldName, EditSQLValue)
                                        PublishFieldNameArray(FieldPointer) = FieldName
                                        If PublishingDelete Then
                                            PublishSqlFieldList.add(FieldName, SQLFalse)
                                        Else
                                            PublishSqlFieldList.add(FieldName, EditSQLValue)
                                        End If
                                        FieldPointer = FieldPointer + 1
                                        '
                                        FieldNameArray(FieldPointer) = "EDITSOURCEID"
                                        NewEditSqlFieldList.add(FieldName, db_EncodeSQLNumber(LiveRecordID))
                                        ArchiveSqlFieldList.add(FieldName, db_EncodeSQLNumber(LiveRecordID))
                                        FieldPointer = FieldPointer + 1
                                        '
                                        FieldNameArray(FieldPointer) = "EDITARCHIVE"
                                        NewEditSqlFieldList.add(FieldName, db_EncodeSQLBoolean(False))
                                        ArchiveSqlFieldList.add(FieldName, db_EncodeSQLBoolean(True))
                                        '
                                        ' ----- copy edit record to live record
                                        '
                                        Call db_UpdateTableRecord(LiveDataSourceName, LiveTableName, "ID=" & LiveRecordID, PublishSqlFieldList)
                                        '
                                        ' ----- copy live record to archive record and the edit to the new edit
                                        '
                                        Call db_UpdateTableRecord(EditDataSourceName, EditTableName, "ID=" & EditRecordID, ArchiveSqlFieldList)
                                        If Not PublishingDelete Then
                                            Call db_UpdateTableRecord(EditDataSourceName, EditTableName, "ID=" & NewEditRecordID, NewEditSqlFieldList)
                                        End If
                                        '
                                        ' ----- Content Watch effects
                                        '
                                        If PublishingDelete Then
                                            '
                                            ' Record was deleted, delete contentwatch records also
                                            '
                                            Call csv_DeleteContentRules(ContentID, RecordID)
                                        End If
                                        '
                                        ' ----- mark the SpiderDocs record not up-to-date
                                        '
                                        If (LCase(EditTableName) = "ccpagecontent") And (LiveRecordID <> 0) Then
                                            If db_IsSQLTableField("default", "ccSpiderDocs", "PageID") Then
                                                SQL = "UPDATE ccspiderdocs SET UpToDate = 0 WHERE PageID=" & LiveRecordID
                                                Call executeSql(SQL)
                                            End If
                                        End If
                                        '
                                        ' ----- Clear Time Stamp because a record changed
                                        '
                                        If csv_AllowAutocsv_ClearContentTimeStamp Then
                                            Call cache.invalidateTag(ContentName)
                                        End If
                                    End If
                                End If
                                'RSEdit.Close()
                            End If
                            'RSEdit = Nothing
                        End If
                        'RSLive.Close()
                    End If
                    RSLive.Dispose()
                    '
                    ' ----- Clear all Authoring Controls
                    '
                    Call csv_ClearAuthoringControl(ContentName, LiveRecordID, AuthoringControlsEditing, MemberID)
                    Call csv_ClearAuthoringControl(ContentName, LiveRecordID, AuthoringControlsModified, MemberID)
                    Call csv_ClearAuthoringControl(ContentName, LiveRecordID, AuthoringControlsApproved, MemberID)
                    Call csv_ClearAuthoringControl(ContentName, LiveRecordID, AuthoringControlsSubmitted, MemberID)
                    '
                    '
                    '
                    If PublishingDelete Or PublishingInactive Then
                        Call csv_DeleteContentRules(ContentID, LiveRecordID)
                    End If
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_PublishEdit", True)
        End Sub
        '
        '=================================================================================
        '
        '=================================================================================
        '
        Public Sub csv_AbortEdit(ByVal ContentName As String, ByVal RecordID As Integer, ByVal MemberID As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-141" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CSPointer As Integer

            Dim RSLive As DataTable
            Dim LiveRecordID As Integer
            Dim LiveDataSourceName As String
            Dim LiveTableName As String
            Dim LiveSQLValue As String
            Dim LiveFilename As String
            '
            Dim RSEdit As DataTable
            Dim EditDataSourceName As String
            Dim EditTableName As String
            Dim EditRecordID As Integer
            Dim EditSQLValue As String
            Dim EditFilename As String
            '
            Dim NewEditRecordID As Integer
            Dim NewEditFilename As String
            '
            Dim ContentID As Integer
            '
            'Dim PublishFieldNameArray() As String
            Dim FieldPointer As Integer
            Dim FieldCount As Integer
            Dim FieldName As String
            Dim fieldTypeId As Integer
            Dim SQL As String
            Dim CDef As coreMetaDataClass.CDefClass
            Dim sqlFieldList As New sqlFieldListClass
            '
            CDef = metaData.getCdef(ContentName)
            If CDef.Id > 0 Then
                If CDef.AllowWorkflowAuthoring And siteProperty_AllowWorkflowAuthoring Then
                    With CDef
                        LiveDataSourceName = .ContentDataSourceName
                        LiveTableName = .ContentTableName
                        EditDataSourceName = .AuthoringDataSourceName
                        EditTableName = .AuthoringTableName
                        FieldCount = .fields.Count
                        ContentID = .Id
                        ContentName = .Name
                    End With
                    'ReDim sqlFieldList(FieldCount + 2)
                    'ReDim PublishFieldNameArray(FieldCount + 2)
                    LiveRecordID = RecordID
                    ' LiveRecordID = appservices.csv_GetCSField(CSPointer, "ID")
                    '
                    ' Open the live record
                    '
                    RSLive = executeSql("SELECT * FROM " & LiveTableName & " WHERE ID=" & db_EncodeSQLNumber(LiveRecordID) & ";", LiveDataSourceName)
                    If (RSLive Is Nothing) Then
                        '
                        Call handleLegacyClassError3("csv_AbortEdit", ("During record publishing, there was an error opening the live record, [ID=" & LiveRecordID & "] in table [" & LiveTableName & "] on datasource [" & LiveDataSourceName & " ]"))
                    Else
                        If RSLive.Rows.Count <= 0 Then
                            '
                            Call handleLegacyClassError3("csv_AbortEdit", ("During record publishing, the live record could not be found, [ID=" & LiveRecordID & "] in table [" & LiveTableName & "] on datasource [" & LiveDataSourceName & " ]"))
                        Else
                            LiveRecordID = EncodeInteger(db_getDataRowColumnName(RSLive.Rows(0), "ID"))
                            '
                            ' Open the edit record
                            '
                            RSEdit = executeSql("SELECT * FROM " & EditTableName & " WHERE (EditSourceID=" & db_EncodeSQLNumber(LiveRecordID) & ")and(EditArchive=0) Order By ID DESC;", EditDataSourceName)
                            If (RSEdit Is Nothing) Then
                                '
                                Call handleLegacyClassError3("csv_AbortEdit", ("During record publishing, there was an error opening the edit record [EditSourceID=" & LiveRecordID & "] in table [" & EditTableName & "] on datasource [" & EditDataSourceName & " ]"))
                            Else
                                If RSEdit.Rows.Count <= 0 Then
                                    '
                                    Call handleLegacyClassError3("csv_AbortEdit", ("During record publishing, the edit record could not be found, [EditSourceID=" & LiveRecordID & "] in table [" & EditTableName & "] on datasource [" & EditDataSourceName & " ]"))
                                Else
                                    EditRecordID = EncodeInteger(db_getDataRowColumnName(RSEdit.Rows(0), "ID"))
                                    '
                                    ' create update arrays
                                    '
                                    FieldPointer = 0
                                    For Each keyValuePair As KeyValuePair(Of String, coreMetaDataClass.CDefFieldClass) In CDef.fields
                                        Dim field As coreMetaDataClass.CDefFieldClass = keyValuePair.Value
                                        With field
                                            FieldName = .nameLc
                                            If db_IsSQLTableField(EditDataSourceName, EditTableName, FieldName) Then
                                                fieldTypeId = .fieldTypeId
                                                LiveSQLValue = EncodeSQL(db_getDataRowColumnName(RSLive.Rows(0), FieldName), fieldTypeId)
                                                Select Case UCase(FieldName)
                                                    Case "EDITARCHIVE", "ID", "EDITSOURCEID"
                                                        '
                                                        '   block from dataset
                                                        '
                                                    Case Else
                                                        '
                                                        ' allow only authorable fields
                                                        '
                                                        If (fieldTypeId = FieldTypeIdFileCSS) Or (fieldTypeId = FieldTypeIdFileJavascript) Or (fieldTypeId = FieldTypeIdFileXML) Then
                                                            '
                                                            '   cdnfiles - create copy of Live TextFile for Edit record
                                                            '
                                                            LiveFilename = EncodeText(db_getDataRowColumnName(RSLive.Rows(0), FieldName))
                                                            If LiveFilename <> "" Then
                                                                EditFilename = csv_GetVirtualFilenameByTable(EditTableName, FieldName, EditRecordID, "", fieldTypeId)
                                                                Call cdnFiles.copyFile(LiveFilename, EditFilename)
                                                                LiveSQLValue = db_EncodeSQLText(EditFilename)
                                                            End If
                                                        End If
                                                        If (fieldTypeId = FieldTypeIdFileTextPrivate) Or (fieldTypeId = FieldTypeIdFileHTMLPrivate) Then
                                                            '
                                                            '   pivatefiles - create copy of Live TextFile for Edit record
                                                            '
                                                            LiveFilename = EncodeText(db_getDataRowColumnName(RSLive.Rows(0), FieldName))
                                                            If LiveFilename <> "" Then
                                                                EditFilename = csv_GetVirtualFilenameByTable(EditTableName, FieldName, EditRecordID, "", fieldTypeId)
                                                                Call privateFiles.copyFile(LiveFilename, EditFilename)
                                                                LiveSQLValue = db_EncodeSQLText(EditFilename)
                                                            End If
                                                        End If
                                                        '
                                                        sqlFieldList.add(FieldName, LiveSQLValue)
                                                End Select
                                            End If
                                        End With
                                        FieldPointer += 1
                                    Next
                                End If
                                Call RSEdit.Dispose()
                            End If
                            RSEdit = Nothing
                        End If
                        RSLive.Dispose()
                    End If
                    RSLive = Nothing
                    '
                    ' ----- copy live record to editrecord
                    '
                    Call db_UpdateTableRecord(EditDataSourceName, EditTableName, "ID=" & EditRecordID, sqlFieldList)
                    '
                    ' ----- Clear all authoring controls
                    '
                    Call csv_ClearAuthoringControl(ContentName, RecordID, AuthoringControlsModified, MemberID)
                    Call csv_ClearAuthoringControl(ContentName, RecordID, AuthoringControlsSubmitted, MemberID)
                    Call csv_ClearAuthoringControl(ContentName, RecordID, AuthoringControlsApproved, MemberID)
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_AbortEdit", True)
        End Sub
        '
        '=================================================================================
        '
        '=================================================================================
        '
        Public Sub csv_ApproveEdit(ByVal ContentName As String, ByVal RecordID As Integer, ByVal MemberID As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-142" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CDef As coreMetaDataClass.CDefClass
            '
            CDef = metaData.getCdef(ContentName)
            If CDef.Id > 0 Then
                If CDef.AllowWorkflowAuthoring And siteProperty_AllowWorkflowAuthoring Then
                    Call csv_SetAuthoringControl(ContentName, RecordID, AuthoringControlsApproved, MemberID)
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_ApproveEdit", True)
        End Sub
        '
        '=================================================================================
        '
        '=================================================================================
        '
        Public Sub csv_SubmitEdit(ByVal ContentName As String, ByVal RecordID As Integer, ByVal MemberID As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-143" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CDef As coreMetaDataClass.CDefClass
            '
            CDef = metaData.getCdef(ContentName)
            If CDef.Id > 0 Then
                If CDef.AllowWorkflowAuthoring And siteProperty_AllowWorkflowAuthoring Then
                    Call csv_SetAuthoringControl(ContentName, RecordID, AuthoringControlsSubmitted, MemberID)
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_SubmitEdit", True)
        End Sub

        Public Property csv_SQLCommandTimeout() As Integer
            Get
                On Error GoTo ErrorTrap 'Const Tn = "MethodName-192" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
                '
                csv_SQLCommandTimeout = csv_SQLTimeout
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
                csv_SQLTimeout = value
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
        Public Function csv_GetRecordIDByGuid(ByVal ContentName As String, ByVal RecordGuid As String) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "GetRecordIDByGuid" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CS As Integer
            '
            CS = db_csOpen(ContentName, "ccguid=" & db_EncodeSQLText(RecordGuid), "ID", , , , , "ID")
            If db_csOk(CS) Then
                csv_GetRecordIDByGuid = EncodeInteger(db_GetCS(CS, "ID"))
            End If
            Call db_csClose(CS)

            Exit Function
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_GetRecordID", True)
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function csv_GetContentRows(ByVal ContentName As String, Optional ByVal Criteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal MemberID As Integer = SystemMemberID, Optional ByVal WorkflowRenderingMode As Boolean = False, Optional ByVal WorkflowEditingMode As Boolean = False, Optional ByVal SelectFieldList As String = "", Optional ByVal PageSize As Integer = 9999, Optional ByVal PageNumber As Integer = 1) As String(,)
            Dim returnRows As String(,) = {}
            Try
                '
                Dim OldState As Boolean
                Dim CS As Integer
                Dim rs As DataTable
                Dim Rows() As Object
                '
                If useCSReadCacheMultiRow Then
                    CS = db_csOpen(ContentName, Criteria, SortFieldList, ActiveOnly, MemberID, WorkflowRenderingMode, WorkflowEditingMode, SelectFieldList, PageSize, PageNumber)
                    If db_csOk(CS) Then
                        returnRows = csv_ContentSet(CS).readCache
                    End If
                    Call db_csClose(CS)
                Else
                    '
                    ' Open the CS, but do not run the query yet
                    '
                    OldState = csv_OpenCSWithoutRecords
                    csv_OpenCSWithoutRecords = True
                    CS = db_csOpen(ContentName, Criteria, SortFieldList, ActiveOnly, MemberID, WorkflowRenderingMode, WorkflowEditingMode, SelectFieldList, PageSize, PageNumber)
                    csv_OpenCSWithoutRecords = OldState
                    '
                    If csv_ContentSet(CS).Source <> "" Then
                        rs = executeSql(csv_ContentSet(CS).Source, csv_ContentSet(CS).DataSource)
                        'RS = executeSql(csv_ContentSet(CS).DataSource, csv_ContentSet(CS).Source)
                        If isDataTableOk(rs) Then
                            returnRows = convertDataTabletoArray(rs)
                        End If
                        Call closeDataTable(rs)
                    End If
                    Call db_csClose(CS)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnRows
        End Function
        '
        '
        '
        Public Sub csv_SetCSRecordDefaults(ByVal CS As Integer)
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
            For Each keyValuePair As KeyValuePair(Of String, coreMetaDataClass.CDefFieldClass) In csv_ContentSet(CS).CDef.fields
                Dim field As coreMetaDataClass.CDefFieldClass = keyValuePair.Value
                With field
                    FieldName = .nameLc
                    If (FieldName <> "") And (Not String.IsNullOrEmpty(.defaultValue)) Then
                        Select Case UCase(FieldName)
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
                                        Call db_setCS(CS, FieldName, "null")
                                        If DefaultValueText <> "" Then
                                            If .lookupContentID <> 0 Then
                                                LookupContentName = csv_GetContentNameByID(.lookupContentID)
                                                If LookupContentName <> "" Then
                                                    Call db_setCS(CS, FieldName, getRecordID(LookupContentName, DefaultValueText))
                                                End If
                                            ElseIf .lookupList <> "" Then
                                                UCaseDefaultValueText = UCase(DefaultValueText)
                                                lookups = Split(.lookupList, ",")
                                                For Ptr = 0 To UBound(lookups)
                                                    If UCaseDefaultValueText = UCase(lookups(Ptr)) Then
                                                        Call db_setCS(CS, FieldName, Ptr + 1)
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                        End If
                                    Case Else
                                        '
                                        ' else text
                                        '
                                        Call db_setCS(CS, FieldName, .defaultValue)
                                End Select
                        End Select
                    End If
                End With
            Next
            '
            Exit Sub
ErrorTrap:
            cpCore.handleException(New Exception("Unexpected exception"))
        End Sub

        '
        ' delete when done
        '
        Private Sub profileLogMethodExit(ByVal ignore As String)
            '
        End Sub
        '
        Private Function profileLogMethodEnter(ByVal ignore As String) As String
            '
        End Function
        '
        '===========================================================================
        '   Log
        '===========================================================================
        '
        Private Sub csv_HandleClassAppendLog(ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String, ByVal MethodName As String, ByVal ErrorTrap As Boolean, Optional ByVal ResumeNext As Boolean = False, Optional ByVal Context As String = "")
            '
            On Error GoTo 0
            cpCore.appendLogWithLegacyRow(config.name, Context, "dll", "cpCoreClass", MethodName, ErrNumber, ErrSource, ErrDescription, False, True, "", "", "")
            '
        End Sub
        '
        ' This is a copy of the routine in cpCoreClass -- duplicated so I do not have to make a public until the interface is worked-out
        '
        Public Function csv_GetSQLSelect(ByVal DataSourceName As String, ByVal From As String, Optional ByVal FieldList As String = "", Optional ByVal Where As String = "", Optional ByVal OrderBy As String = "", Optional ByVal GroupBy As String = "", Optional ByVal RecordLimit As Integer = 0) As String
            On Error GoTo ErrorTrap 'Const Tn = "csv_GetSQLSelect" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim SQL As String
            '
            Select Case csv_GetDataSourceType(DataSourceName)
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
            csv_GetSQLSelect = SQL
            '
            Exit Function
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, "csv_GetSQLSelect", True, False)
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Sub exportApplicationCDefXml(ByVal privateFilesPathFilename As String, ByVal IncludeBaseFields As Boolean)
            On Error GoTo ErrorTrap 'Const Tn = "ExportXML2" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            Dim XML As coreXmlToolsClass
            Dim Content As String
            '
            MethodName = "csv_ExportXML2"
            '
            XML = New coreXmlToolsClass(cpCore)
            'Call XML.Init(Me)
            Content = XML.GetXMLContentDefinition3("", IncludeBaseFields)
            Call privateFiles.SaveFile(privateFilesPathFilename, Content)
            XML = Nothing
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Sub
        '
        '
        '
        Public Function csv_GetSQLIndexList(ByVal DataSourceName As String, ByVal TableName As String) As String
            Dim returnList As String = ""
            Try
                Dim ts As coreMetaDataClass.tableSchemaClass
                ts = metaData.getTableSchema(TableName, DataSourceName)
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
                Dim connString As String = cpCore.cluster.getConnectionString(config.name)
                'Dim dataSourceUrl As String
                'dataSourceUrl = cpCore.cluster.config.defaultDataSourceAddress
                'If (dataSourceUrl.IndexOf(":") > 0) Then
                '    dataSourceUrl = dataSourceUrl.Substring(0, dataSourceUrl.IndexOf(":"))
                'End If
                ''
                'connString = "" _
                '    & "data source=" & dataSourceUrl & ";" _
                '    & "initial catalog=" & config.name & ";" _
                '    & "UID=" & cpCore.cluster.config.defaultDataSourceUsername & ";" _
                '    & "PWD=" & cpCore.cluster.config.defaultDataSourcePassword & ";" _
                '    & ""
                Using connSQL As New SqlConnection(connString)
                    connSQL.Open()
                    returnDt = connSQL.GetSchema("Tables", {config.name, Nothing, tableName, Nothing})
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
                Dim connString As String = cpCore.cluster.getConnectionString(config.name)
                'Dim dataSourceUrl As String
                'dataSourceUrl = cpCore.cluster.config.defaultDataSourceAddress
                'If (dataSourceUrl.IndexOf(":") > 0) Then
                '    dataSourceUrl = dataSourceUrl.Substring(0, dataSourceUrl.IndexOf(":"))
                'End If
                ''
                'connString = "" _
                '    & "data source=" & dataSourceUrl & ";" _
                '    & "initial catalog=" & config.name & ";" _
                '    & "UID=" & cpCore.cluster.config.defaultDataSourceUsername & ";" _
                '    & "PWD=" & cpCore.cluster.config.defaultDataSourcePassword & ";" _
                '    & ""
                Using connSQL As New SqlConnection(connString)
                    connSQL.Open()
                    returnDt = connSQL.GetSchema("Columns", {config.name, Nothing, tableName, Nothing})
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
                Dim connString As String = cpCore.cluster.getConnectionString(config.name)
                'Dim dataSourceUrl As String
                'dataSourceUrl = cpCore.cluster.config.defaultDataSourceAddress
                'If (dataSourceUrl.IndexOf(":") > 0) Then
                '    dataSourceUrl = dataSourceUrl.Substring(0, dataSourceUrl.IndexOf(":"))
                'End If
                ''
                'connString = "" _
                '    & "data source=" & dataSourceUrl & ";" _
                '    & "initial catalog=" & cpCore.app.config.name & ";" _
                '    & "UID=" & cpCore.cluster.config.defaultDataSourceUsername & ";" _
                '    & "PWD=" & cpCore.cluster.config.defaultDataSourcePassword & ";" _
                '    & ""
                Using connSQL As New SqlConnection(connString)
                    connSQL.Open()
                    returnDt = connSQL.GetSchema("Indexes", {config.name, Nothing, tableName, Nothing})
                End Using
            Catch ex As Exception
                Call handleLegacyClassError5(ex, System.Reflection.MethodBase.GetCurrentMethod.Name, "exception")
            End Try
            Return returnDt
        End Function
        '
        '
        '
        Public Function getAddonPath() As String
            Return "addons\"
        End Function
        '
        '   Buffered Site Property
        '
        Public ReadOnly Property siteProperty_DefaultFormInputWidth() As Integer
            Get
                If Not siteProperty_DefaultFormInputWidth_LocalLoaded Then
                    siteProperty_DefaultFormInputWidth_Local = EncodeInteger(siteProperty_getText("DefaultFormInputWidth", "60"))
                    siteProperty_DefaultFormInputWidth_LocalLoaded = True
                End If
                siteProperty_DefaultFormInputWidth = siteProperty_DefaultFormInputWidth_Local
            End Get
        End Property
        '
        '   Buffered Site Property
        '
        Public ReadOnly Property siteProperty_SelectFieldWidthLimit() As Integer
            Get
                If Not siteProperty_SelectFieldWidthLimit_LocalLoaded Then
                    siteProperty_SelectFieldWidthLimit_Local = EncodeInteger(siteProperty_getText("SelectFieldWidthLimit", "200"))
                    siteProperty_SelectFieldWidthLimit_LocalLoaded = True
                End If
                siteProperty_SelectFieldWidthLimit = siteProperty_SelectFieldWidthLimit_Local
            End Get
        End Property
        '
        '   Buffered Site Property
        '
        Public ReadOnly Property siteProperty_SelectFieldLimit() As Integer
            Get
                If Not siteProperty_SelectFieldLimit_LocalLoaded Then
                    siteProperty_SelectFieldLimit_Local = EncodeInteger(siteProperty_getText("SelectFieldLimit", "1000"))
                    If siteProperty_SelectFieldLimit_Local = 0 Then
                        siteProperty_SelectFieldLimit_Local = 1000
                        Call siteProperty_set("SelectFieldLimit", CStr(siteProperty_SelectFieldLimit_Local))
                    End If
                    siteProperty_SelectFieldLimit_LocalLoaded = True
                End If
                siteProperty_SelectFieldLimit = siteProperty_SelectFieldLimit_Local
            End Get
        End Property
        '
        '   Buffered Site Property
        '
        Public ReadOnly Property siteProperty_DefaultFormInputTextHeight() As Integer
            Get
                If Not siteProperty_DefaultFormInputTextHeight_LocalLoaded Then
                    siteProperty_DefaultFormInputTextHeight_Local = EncodeInteger(siteProperty_getText("DefaultFormInputTextHeight", "1"))
                    siteProperty_DefaultFormInputTextHeight_LocalLoaded = True
                End If
                siteProperty_DefaultFormInputTextHeight = siteProperty_DefaultFormInputTextHeight_Local
            End Get
        End Property
        '
        '
        ' Buffered Site Property
        '
        Public ReadOnly Property siteProperty_AllowTransactionLog() As Boolean
            Get
                If Not siteProperty_AllowTransactionLog_localLoaded Then
                    siteProperty_AllowTransactionLog_local = EncodeBoolean(siteProperty_getText("UseContentWatchLink", "false"))
                    siteProperty_AllowTransactionLog_localLoaded = True
                End If
                siteProperty_AllowTransactionLog = siteProperty_AllowTransactionLog_local
            End Get
        End Property
        Private siteProperty_AllowTransactionLog_localLoaded As Boolean = False
        Private siteProperty_AllowTransactionLog_local As Boolean
        '
        ' Buffered Site Property
        '
        Public ReadOnly Property siteProperty_UseContentWatchLink() As Boolean
            Get
                If Not siteProperty_UseContentWatchLink_LocalLoaded Then
                    siteProperty_UseContentWatchLink_local = EncodeBoolean(siteProperty_getText("UseContentWatchLink", "false"))
                    siteProperty_UseContentWatchLink_LocalLoaded = True
                End If
                siteProperty_UseContentWatchLink = siteProperty_UseContentWatchLink_local

            End Get
        End Property
        '
        '   Buffered Site Property
        '
        Public ReadOnly Property siteProperty_AllowTestPointLogging() As Boolean
            Get
                If Not siteProperty_AllowTestPointLogging_LocalLoaded Then
                    siteProperty_AllowTestPointLogging_Local = EncodeBoolean(siteProperty_getText("AllowTestPointLogging", "false"))
                    siteProperty_AllowTestPointLogging_LocalLoaded = True
                End If
                siteProperty_AllowTestPointLogging = siteProperty_AllowTestPointLogging_Local

            End Get
        End Property
        '====================================================================================================
        ''' <summary>
        ''' trap errors (hide errors) - when true, errors will be logged and code resumes next. When false, errors are re-thrown
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property siteProperty_trapErrors() As Boolean
            Get
                If Not _trapErrorsLoaded Then
                    _trapErrors = EncodeBoolean(siteProperty_getText("TrapErrors", "true"))
                    _trapErrorsLoaded = True
                End If
                siteProperty_trapErrors = _trapErrors
            End Get
        End Property
        Private _trapErrors As Boolean
        Private _trapErrorsLoaded As Boolean = False
        '
        '   Buffered Site Property
        '
        Public ReadOnly Property siteProperty_EmailAdmin() As String
            Get
                If Not siteProperty_EmailAdmin_LocalLoaded Then
                    siteProperty_EmailAdmin_Local = siteProperty_getText("main_EmailAdmin", "webmaster@" & cpCore.main_ServerDomain)
                    siteProperty_EmailAdmin_LocalLoaded = True
                End If
                siteProperty_EmailAdmin = siteProperty_EmailAdmin_Local

            End Get
        End Property
        '
        '   Buffered Site Property
        '
        Public ReadOnly Property siteProperty_ServerPageDefault() As String
            Get
                If Not siteProperty_ServerPageDefault_localLoaded Then
                    siteProperty_ServerPageDefault_local = siteProperty_getText(siteproperty_serverPageDefault_name, siteproperty_serverPageDefault_defaultValue)
                    siteProperty_ServerPageDefault_localLoaded = True
                End If
                siteProperty_ServerPageDefault = siteProperty_ServerPageDefault_local

            End Get
        End Property
        Private siteProperty_ServerPageDefault_local As String
        Private siteProperty_ServerPageDefault_localLoaded As Boolean = False
        ''
        ''   Buffered Site Property
        ''
        'Public ReadOnly Property config.contentFilePathPrefix() As String
        '    Get
        '        If Not config.contentFilePathPrefix_LocalLoaded Then
        '            config.contentFilePathPrefix_Local = siteProperty_getText("contentPathPrefix", "\contentFiles\")
        '            config.contentFilePathPrefix_LocalLoaded = True
        '        End If
        '        config.contentFilePathPrefix = config.contentFilePathPrefix_Local

        '    End Get
        'End Property
        'Private config.contentFilePathPrefix_Local As String
        'Private config.contentFilePathPrefix_LocalLoaded As Boolean = False
        '
        '   Buffered Site Property
        '
        Public ReadOnly Property siteProperty_Language() As String
            Get
                If Not siteProperty_Language_LocalLoaded Then
                    siteProperty_Language_Local = siteProperty_getText("Language", "English")
                    siteProperty_Language_LocalLoaded = True
                End If
                siteProperty_Language = siteProperty_Language_Local

            End Get
        End Property
        '
        '   Buffered Site Property
        '
        Public ReadOnly Property siteProperty_AdminURL() As String
            Get
                Dim Position As Integer
                If Not siteProperty_AdminURL_LocalLoaded Then
                    siteProperty_AdminURL_Local = siteProperty_getText("AdminURL", config.adminRoute)
                    'siteProperty_AdminURL_Local = cpCore.main_EncodeAppRootPath(siteProperty_AdminURL_Local)
                    Position = InStr(1, siteProperty_AdminURL_Local, "?")
                    If Position <> 0 Then
                        siteProperty_AdminURL_Local = Mid(siteProperty_AdminURL_Local, 1, Position - 1)
                    End If
                    siteProperty_AdminURL_LocalLoaded = True
                End If
                siteProperty_AdminURL = siteProperty_AdminURL_Local

            End Get
        End Property

        '
        '   Buffered Site Property
        '
        Public ReadOnly Property siteProperty_CalendarYearLimit() As Integer
            Get
                If Not siteProperty_CalendarYearLimit_LocalLoaded Then
                    siteProperty_CalendarYearLimit_Local = EncodeInteger(siteProperty_getText("CalendarYearLimit", "1"))
                    siteProperty_CalendarYearLimit_LocalLoaded = True
                End If
                siteProperty_CalendarYearLimit = siteProperty_CalendarYearLimit_Local

            End Get
        End Property

        '
        '   Buffered Site Property
        '
        Public ReadOnly Property siteProperty_AllowChildMenuHeadline() As Boolean
            Get
                Return False
            End Get
        End Property

        '
        '   Buffered Site Property
        '
        Public ReadOnly Property siteProperty_DefaultFormInputHTMLHeight() As Integer
            Get
                If Not siteProperty_DefaultFormInputHTMLHeight_LocalLoaded Then
                    siteProperty_DefaultFormInputHTMLHeight_Local = siteProperty_getinteger("DefaultFormInputHTMLHeight", 500)
                    siteProperty_DefaultFormInputHTMLHeight_LocalLoaded = True
                End If
                siteProperty_DefaultFormInputHTMLHeight = siteProperty_DefaultFormInputHTMLHeight_Local

            End Get
        End Property
        '
        '   Buffered Site Property
        '
        Public ReadOnly Property siteProperty_AllowWorkflowAuthoring() As Boolean
            Get
                If Not siteProperty_AllowWorkflowAuthoring_LocalLoaded Then
                    siteProperty_AllowWorkflowAuthoring_Local = EncodeBoolean(siteProperty_getText("AllowWorkflowAuthoring", "false"))
                    siteProperty_AllowWorkflowAuthoring_LocalLoaded = True
                End If
                siteProperty_AllowWorkflowAuthoring = siteProperty_AllowWorkflowAuthoring_Local

            End Get
        End Property
        '
        '   Buffered Site Property
        '
        Public ReadOnly Property siteProperty_AllowPathBlocking() As Boolean
            Get
                If Not siteProperty_AllowPathBlocking_LocalLoaded Then
                    siteProperty_AllowPathBlocking_Local = EncodeBoolean(siteProperty_getText("AllowPathBlocking", "false"))
                    siteProperty_AllowPathBlocking_LocalLoaded = True
                End If
                siteProperty_AllowPathBlocking = siteProperty_AllowPathBlocking_Local

            End Get
        End Property
        '
        '
        '===================================================================================================
        '   Special Property Load
        '       If the property did not exist, add it
        '           true for existing sites
        '               if there are any pathrule records or any template link is not blank
        '           false for new sites
        '===================================================================================================
        '
        Public ReadOnly Property siteProperty_AllowTemplateLinkVerification() As Boolean
            Get
                Dim TestString As String
                Dim CS As Integer
                Dim SetTrue As Boolean
                Dim SQL As String
                '
                '    If true Then
                '        siteProperty_AllowTemplateLinkVerification = True
                '    Else
                If Not siteProperty_AllowTemplateLinkVerification_LocalLoaded Then
                    TestString = siteProperty_getText("AllowTemplateLinkVerification", "")
                    If TestString <> "" Then
                        '
                        ' read value from property
                        '
                        siteProperty_AllowTemplateLinkVerification_Local = EncodeBoolean(TestString)
                    Else
                        '
                        ' Update - template link verification is needed for Template.IsSecure, so turn it on for new sites
                        '
                        siteProperty_AllowTemplateLinkVerification_LocalLoaded = True
                        Call siteProperty_set("AllowTemplateLinkVerification", "true")
                    End If
                    siteProperty_AllowTemplateLinkVerification_LocalLoaded = True
                End If
                siteProperty_AllowTemplateLinkVerification = siteProperty_AllowTemplateLinkVerification_Local

            End Get
        End Property
        '
        '
        '
        Friend ReadOnly Property siteProperty_DefaultWrapperID() As Integer
            Get
                If Not siteProperty_DefaultWrapperID_LocalLoaded Then
                    siteProperty_DefaultWrapperID_LocalLoaded = True
                    siteProperty_DefaultWrapperID_local = EncodeInteger(siteProperty_getText("DefaultWrapperID", "0"))
                End If
                siteProperty_DefaultWrapperID = siteProperty_DefaultWrapperID_local

            End Get
        End Property
        '
        '
        '
        Friend ReadOnly Property siteProperty_ChildListAddonID() As Integer
            Get
                Dim CS As Integer
                Dim BuildSupportsGuid As Boolean
                '
                If Not siteProperty_ChildListAddonID_LocalLoaded Then
                    siteProperty_ChildListAddonID_LocalLoaded = True
                    siteProperty_ChildListAddonID_Local = EncodeInteger(siteProperty_getText("ChildListAddonID", ""))
                    If siteProperty_ChildListAddonID_Local = 0 Then
                        BuildSupportsGuid = True
                        If BuildSupportsGuid Then
                            CS = db_csOpen("Add-ons", "ccguid='" & ChildListGuid & "'", , , ,,  , "ID")
                        Else
                            CS = db_csOpen("Add-ons", "name='Child Page List'", , , , ,, "ID")
                        End If
                        If db_csOk(CS) Then
                            siteProperty_ChildListAddonID_Local = db_GetCSInteger(CS, "ID")
                        End If
                        Call db_csClose(CS)
                        If siteProperty_ChildListAddonID_Local = 0 Then
                            CS = db_csInsertRecord("Add-ons")
                            If db_csOk(CS) Then
                                siteProperty_ChildListAddonID_Local = db_GetCSInteger(CS, "ID")
                                Call db_setCS(CS, "name", "Child Page List")
                                Call db_setCS(CS, "ArgumentList", "Name")
                                Call db_setCS(CS, "CopyText", "<ac type=""childlist"" name=""$name$"">")
                                Call db_setCS(CS, "Content", "1")
                                Call db_setCS(CS, "StylesFilename", "")
                                If BuildSupportsGuid Then
                                    Call db_setCS(CS, "ccguid", ChildListGuid)
                                End If
                            End If
                            Call db_csClose(CS)
                        End If
                        Call siteProperty_set("ChildListAddonID", CStr(siteProperty_ChildListAddonID_Local))
                    End If
                End If
                siteProperty_ChildListAddonID = siteProperty_ChildListAddonID_Local

            End Get

        End Property
        '
        '
        '
        Public ReadOnly Property siteProperty_docTypeDeclaration() As String
            Get
                If Not siteProperty_DocType_LocalLoaded Then
                    siteProperty_DocType_LocalLoaded = True
                    siteProperty_DocType_Local = siteProperty_getText("DocTypeDeclaration", DTDDefault)
                    If siteProperty_DocType_Local = "" Then
                        siteProperty_DocType_Local = DTDDefault
                        Call siteProperty_set("DocTypeDeclaration", DTDDefault)
                    End If
                End If
                siteProperty_docTypeDeclaration = siteProperty_DocType_Local

            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property siteProperty_docTypeDeclarationAdmin() As String
            Get
                If Not siteProperty_DocTypeAdmin_LocalLoaded Then
                    siteProperty_DocTypeAdmin_LocalLoaded = True
                    siteProperty_DocTypeAdmin_Local = siteProperty_getText("DocTypeDeclarationAdmin", DTDDefaultAdmin)
                End If
                siteProperty_docTypeDeclarationAdmin = siteProperty_DocTypeAdmin_Local

            End Get
        End Property
        '
        '==========
        ''' <summary>
        ''' determine the application status code for the health of this application
        ''' </summary>
        ''' <returns></returns>
        Public Function checkHealth() As applicationStatusEnum
            Dim returnStatus As applicationStatusEnum = applicationStatusEnum.ApplicationStatusLoading
            Try
                '
                Try
                    '
                    '--------------------------------------------------------------------------
                    '   Verify the ccContent table exists 
                    '--------------------------------------------------------------------------
                    '
                    Dim testDt As DataTable
                    testDt = executeSql("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='ccContent'")
                    If testDt.Rows.Count <> 1 Then
                        cpCore.app.status = applicationStatusEnum.ApplicationStatusDbFoundButContentMetaMissing
                    End If
                    testDt.Dispose()
                Catch ex As Exception
                    cpCore.app.status = applicationStatusEnum.ApplicationStatusDbFoundButContentMetaMissing
                End Try
                '
                '--------------------------------------------------------------------------
                '   Perform DB Integregity checks
                '--------------------------------------------------------------------------
                '
                Dim ts As coreMetaDataClass.tableSchemaClass = cpCore.app.metaData.getTableSchema("ccContent", "Default")
                If (ts Is Nothing) Then
                    '
                    ' Bad Db and no upgrade - exit
                    '
                    cpCore.app.status = applicationStatusEnum.ApplicationStatusDbBad
                Else
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Function
        '
        '==========
        '
        Friend Function db_getNameIdOrGuidSqlCriteria(nameIdOrGuid As String) As String
            Dim sqlCriteria As String = ""
            Try
                If IsNumeric(nameIdOrGuid) Then
                    sqlCriteria = "id=" & db_EncodeSQLNumber(CDbl(nameIdOrGuid))
                ElseIf cpCore.common_isGuid(nameIdOrGuid) Then
                    sqlCriteria = "ccGuid=" & db_EncodeSQLText(nameIdOrGuid)
                Else
                    sqlCriteria = "name=" & db_EncodeSQLText(nameIdOrGuid)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return sqlCriteria
        End Function



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

