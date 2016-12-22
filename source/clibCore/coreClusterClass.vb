
Option Explicit On
Option Strict On

Imports System.Data.SqlClient

Namespace Contensive.Core
    '
    '====================================================================================================
    ''' <summary>
    ''' application configuration class
    ''' </summary>
    Public Class appConfigClass
        Public name As String
        Public enabled As Boolean
        Public privateKey As String                     ' rename hashKey
        Public defaultConnectionString As String
        Public appRootPath As String                    ' path relative to clusterPhysicalPath
        Public cdnFilesPath As String                   ' path relative to clusterPhysicalPath
        Public privateFilesPath As String               ' path relative to clusterPhysicalPath
        Public cdnFilesNetprefix As String              ' in some cases (like legacy), cdnFiles are iis virtual folder mapped to appRoot (/files/). Some cases this is a URL (http:\\cdn.domain.com pointing to s3)
        Public allowSiteMonitor As Boolean
        Public domainList As New List(Of String)        ' primary domain is the first item in the list
        Public enableCache As Boolean
        Public adminRoute As String                                          ' The url pathpath that executes the addon site
    End Class    '
    '
    '====================================================================================================
    ''' <summary>
    ''' Holds location on the server of the clusterConfig file. Physically stored at programDataFolder/clib/serverConfig.json
    ''' </summary>
    Public Class serverConfigClass
        Public clusterPath As String
        Public allowTaskRunnerService As Boolean
        Public allowTaskSchedulerService As Boolean
    End Class

    '
    '====================================================================================================
    ''' <summary>
    ''' cluster configuration class - deserialized configration file
    ''' </summary>
    ''' <remarks></remarks>
    Public Class clusterConfigClass
        Public isLocal As Boolean = True
        Public name As String = ""
        '
        ' local caching using dotnet framework, flushes on appPool
        '
        Public isLocalCache As Boolean = False
        '
        ' AWS dotnet elaticcache client wraps enyim, and provides node autodiscovery through the configuration object.
        ' this is the srver:port to the config file it uses.
        '
        Public awsElastiCacheConfigurationEndpoint As String
        '
        ' datasource for the cluster
        '
        Public defaultDataSourceType As dataSourceTypeEnum
        '
        ' odbc
        '
        Public defaultDataSourceODBCConnectionString As String
        '
        ' native
        '
        Public defaultDataSourceAddress As String = ""
        '
        ' user for creating new databases, and creating the new user for the database during site create, and saved to appconfig
        '
        Public defaultDataSourceUsername As String = ""
        Public defaultDataSourcePassword As String = ""
        '
        ' endpoint for cluster files (not sure how it works, maybe this will be an object taht includes permissions, for now an fpo)
        '
        Public clusterFilesEndpoint As String
        '
        ' configuration of async command listener on render machines (not sure if used still)
        '
        Public serverListenerPort As Integer = Port_ContentServerControlDefault
        Public maxConcurrentTasksPerServer As Integer = 5
        ' ayncCmd server authentication -- change this to a key later
        Public username As String = ""
        Public password As String = ""
        '
        ' This is the root path to the localCluster files, typically getLocalDataFolder (d:\inetpub)
        '   if isLocal, the cluster runs from these files
        '   if not, this is the local mirror of the cluster files
        '
        Public clusterPhysicalPath As String
        '
        'Public domainRoutes As Dictionary(Of String, String)
        '
        Public appPattern As String
        '
        '
        '
        Public apps As New Dictionary(Of String, appConfigClass)
    End Class
    '
    '====================================================================================================
    ''' <summary>
    ''' cluster srervices - properties and methods to maintain the cluster. Applications do not have access to this. 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class coreClusterClass
        Implements IDisposable
        '
        ' the cp parent for this object
        '
        Private cpCore As cpCoreClass
        '
        Public config As clusterConfigClass
        Public clusterFiles As coreFileSystemClass
        '
        '========================================================================
        ''' <summary>
        ''' Constructor builds data. read from cache and deserialize, if not in cache, build it from scratch, eventually, setup public properties as indivisual lazyCache
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Friend Sub New(cpCore As cpCoreClass)
            '
            ' called during core constructor - so cp.core is not valid
            '
            MyBase.New()
            Me.cpCore = cpCore
            Try
                Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer
                Dim JSONTemp As String
                Dim serverPortSplit As String()
                Dim port As Integer = 11211
                Dim serverConfig As serverConfigClass
                Dim programDataFiles As coreFileSystemClass
                Dim tmpPath As String = ""
                '
                ' setup programData\clib to bootstrap clusterConfig file
                '
                config = New clusterConfigClass()
                config.isLocal = True
                config.clusterPhysicalPath = getProgramDataFolder() & "\"
                _ok = False
                '
                ' load server config
                '
                programDataFiles = New coreFileSystemClass(cpCore, config, coreFileSystemClass.fileSyncModeEnum.noSync, getProgramDataFolder)
                JSONTemp = programDataFiles.ReadFile("serverConfig.json")
                If String.IsNullOrEmpty(JSONTemp) Then
                    serverConfig = New serverConfigClass
                    serverConfig.clusterPath = "d:\"
                    If (Not System.IO.Directory.Exists(serverConfig.clusterPath)) Then
                        serverConfig.clusterPath = "c:\"
                    End If
                    serverConfig.clusterPath &= "inetPub"
                    If Not (System.IO.Directory.Exists(serverConfig.clusterPath)) Then
                        System.IO.Directory.CreateDirectory(serverConfig.clusterPath)
                    End If
                    serverConfig.allowTaskRunnerService = False
                    serverConfig.allowTaskSchedulerService = False
                    programDataFiles.SaveFile("serverConfig.json", json_serializer.Serialize(serverConfig))
                Else
                    serverConfig = json_serializer.Deserialize(Of serverConfigClass)(JSONTemp)
                End If
                clusterFiles = New coreFileSystemClass(cpCore, config, coreFileSystemClass.fileSyncModeEnum.activeSync, serverConfig.clusterPath)
                JSONTemp = clusterFiles.ReadFile("clusterConfig.json")
                If String.IsNullOrEmpty(JSONTemp) Then
                    '
                    ' for now it fails, maybe later let it autobuild a local cluster
                    '
                Else
                    config = json_serializer.Deserialize(Of clusterConfigClass)(JSONTemp)
                    _ok = True
                End If
                '
                ' backfill with default in case it was set blank
                '
                If String.IsNullOrEmpty(config.clusterPhysicalPath) Then
                    config.clusterPhysicalPath = serverConfig.clusterPath
                End If
                '
                ' init file system
                '
                If _ok Then
                    If Not config.isLocal Then
                        clusterFiles = New coreFileSystemClass(cpCore, config, coreFileSystemClass.fileSyncModeEnum.noSync, localDataPath, "")
                    Else
                        clusterFiles = New coreFileSystemClass(cpCore, config, coreFileSystemClass.fileSyncModeEnum.activeSync, localDataPath, config.clusterFilesEndpoint)
                    End If
                End If
                '
                ' setup cache
                '
                If config.isLocalCache Then
                    Throw New NotImplementedException("local cache not implemented yet")
                Else
                    '
                    ' converted to lazy constructor, remove this after test
                    '
                    'If Not String.IsNullOrEmpty(config.awsElastiCacheConfigurationEndpoint) Then
                    '    serverPortSplit = config.awsElastiCacheConfigurationEndpoint.Split(":"c)
                    '    If serverPortSplit.Count > 1 Then
                    '        port = EncodeInteger(serverPortSplit(1))
                    '    End If
                    '    Dim cacheConfig As Amazon.ElastiCacheCluster.ElastiCacheClusterConfig
                    '    cacheConfig = New Amazon.ElastiCacheCluster.ElastiCacheClusterConfig(serverPortSplit(0), port)
                    '    cacheConfig.Protocol = Enyim.Caching.Memcached.MemcachedProtocol.Binary
                    '    cacheClient = New Enyim.Caching.MemcachedClient(cacheConfig)
                    '    '
                    '    ' REFACTOR - removed because during debug it costs 20 msec. test cache fail case to measure benefit
                    '    '
                    '    'mc.Store(Enyim.Caching.Memcached.StoreMode.Set, "testing", "123", Now.AddMinutes(10))
                    'End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' ok - means the class has initialized and methods can be used to maintain the cluser
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ok As Boolean
            Get
                Return _ok
            End Get
        End Property
        Private _ok As Boolean = False
        '
        '====================================================================================================
        ''' <summary>
        ''' physical path to the head of the local data storage
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property localDataPath As String
            Get
                If (config Is Nothing) Then
                    Return ""
                Else
                    Return config.clusterPhysicalPath
                End If
            End Get
        End Property
        '
        Public ReadOnly Property localAppsPath As String
            Get
                If (config Is Nothing) Then
                    Return ""
                Else
                    Return config.clusterPhysicalPath & "apps\"
                End If
            End Get
        End Property
        '
        '====================================================================================================
        ''' <summary>
        ''' return the correctly formated connection string for this datasource
        ''' </summary>
        ''' <returns></returns>
        Friend Function getConnectionString(catalogName As String) As String
            Dim returnString As String = ""
            Try
                Dim dataSourceUrl As String
                dataSourceUrl = config.defaultDataSourceAddress
                If (dataSourceUrl.IndexOf(":") > 0) Then
                    dataSourceUrl = dataSourceUrl.Substring(0, dataSourceUrl.IndexOf(":"))
                End If
                '
                returnString = "" _
                    & "data source=" & dataSourceUrl & ";" _
                    & "UID=" & config.defaultDataSourceUsername & ";" _
                    & "PWD=" & config.defaultDataSourcePassword & ";" _
                    & ""
                If Not String.IsNullOrEmpty(catalogName) Then
                    returnString &= "initial catalog=" & catalogName & ";"
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnString
        End Function
        '
        ' execute sql on default connection and return datatable
        '
        Public Function executeSql(ByVal sql As String, Optional ByVal startRecord As Integer = 0, Optional ByVal maxRecords As Integer = 9999) As DataTable
            Dim returnData As New DataTable
            Try
                Dim connString As String = getConnectionString("")
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
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnData
        End Function
        '
        ' verify database exists
        '
        Public Function checkDatabaseExists(databaseName As String) As Boolean
            Dim returnOk As Boolean = False
            Try
                Dim sql As String
                Dim databaseId As Integer = 0
                Dim dt As DataTable
                '
                sql = String.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", databaseName)
                dt = executeSql(sql)
                returnOk = (dt.Rows.Count > 0)
                dt.Dispose()
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnOk
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' save config changes to the clusterConfig.json file
        ''' </summary>
        Public Sub saveConfig()
            Dim json As New System.Web.Script.Serialization.JavaScriptSerializer
            Dim jsonTemp As String = json.Serialize(config)
            clusterFiles.SaveFile("clusterConfig.json", jsonTemp)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' handle exceptions in this class
        ''' </summary>
        ''' <param name="ex"></param>
        ''' <param name="methodName"></param>
        ''' <param name="Cause"></param>
        ''' <remarks></remarks>
        Private Sub handleClassException(ByVal ex As Exception, ByVal methodName As String, ByVal Cause As String)
            cpCore.handleException(ex, "Unexpected exception in clusterServicesClass." & methodName & ", cause=[" & Cause & "]")
        End Sub
        '
        '====================================================================================================
        ' dispose
        '====================================================================================================
        '
#Region " IDisposable Support "
        Protected disposed As Boolean = False
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    'CP = Nothing

                    '
                    ' ----- Close all open csv_ContentSets, and make sure the RS is killed
                    '
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
End Namespace
