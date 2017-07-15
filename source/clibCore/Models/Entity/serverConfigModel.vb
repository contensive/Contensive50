
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Contensive.Core.Models.Entity
    '
    '====================================================================================================
    ' cached entity model pattern
    '   factory pattern creator, constructor is a shared method that returns a loaded object
    '   new() - to allow deserialization (so all methods must pass in cp)
    '   shared getObject( cp, id ) - returns loaded model
    '   saveObject( cp ) - saves instance properties, returns the record id
    '
    <Serializable()> Public Class serverConfigModel
        '
        ' -- public properties
        '
        ' -- set during installation
        Public programFilesPath As String
        '
        ' -- old serverConfig
        'Public clusterPath As String
        Public allowTaskRunnerService As Boolean
        Public allowTaskSchedulerService As Boolean
        '
        ' -- old clusterConfig
        Public name As String = ""
        '
        ' -- If true, use local dotnet memory cache backed by filesystem
        Public enableLocalMemoryCache As Boolean = True
        Public enableLocalFileCache As Boolean = True
        '
        ' -- AWS dotnet elaticcache client wraps enyim, and provides node autodiscovery through the configuration object. this is the srver:port to the config file it uses.
        Public enableRemoteCache As Boolean = False
        Public awsElastiCacheConfigurationEndpoint As String = ""
        '
        ' -- datasource for the cluster (only sql support for now)
        Public defaultDataSourceType As Models.Entity.dataSourceModel.dataSourceTypeEnum
        Public defaultDataSourceAddress As String = ""
        Public defaultDataSourceUsername As String = ""
        Public defaultDataSourcePassword As String = ""
        '
        ' -- endpoint for cluster files (not sure how it works, maybe this will be an object taht includes permissions, for now an fpo)
        Public isLocalFileSystem As Boolean = True
        Public localDataDriveLetter As String = "D"
        Public cdnFilesRemoteEndpoint As String
        '
        ' -- configuration of async command listener on render machines (not sure if used still)
        Public serverListenerPort As Integer = Port_ContentServerControlDefault
        Public maxConcurrentTasksPerServer As Integer = 5
        Public username As String = ""
        Public password As String = ""
        '
        Public enableLogging As Boolean = False
        '
        ' -- deprecated
        'Public appPattern As String
        '
        ' -- List of all apps on this server
        Public apps As New Dictionary(Of String, appConfigModel)
        '
        ' -- the specific application in use for this instance (may be empty if this instance is not initialized
        <NonSerialized()> Public appConfig As appConfigModel
        '
        '====================================================================================================
        ''' <summary>
        ''' application configuration class
        ''' </summary>
        Public Class appConfigModel
            Public name As String = ""
            Public appStatus As appStatusEnum = appStatusEnum.errorAppConfigNotFound
            Public appMode As appModeEnum = appModeEnum.maintainence                    ' must be set to normal after setup
            Public enabled As Boolean = False
            Public privateKey As String = ""                                            ' rename hashKey
            Public appRootFilesPath As String = ""                                      ' local file path to the appRoot (i.e. d:\inetpub\myApp\wwwRoot\)
            Public cdnFilesPath As String = ""                                          ' local file path to the content files (i.e. d:\inetpub\myApp\files\)
            Public privateFilesPath As String = ""                                      ' local file path to the content files (i.e. d:\inetpub\myApp\private\)
            Public cdnFilesNetprefix As String = ""                                     ' in some cases (like legacy), cdnFiles are iis virtual folder mapped to appRoot (/appName/files/). Some cases this is a URL (http:\\cdn.domain.com pointing to s3)
            Public allowSiteMonitor As Boolean = False
            Public domainList As New List(Of String)                                    ' primary domain is the first item in the list
            Public adminRoute As String = ""                                            ' The url pathpath that executes the addon site
            Public defaultPage As String = "default.aspx"                               ' when exeecuting iis 
        End Class    '
        '
        '====================================================================================================
        ''' <summary>
        ''' status of the app in the appConfigModel. Only applies to the app loaded in the serverstatus.appconfig
        ''' </summary>
        Public Enum appModeEnum
            normal = 0
            maintainence = 1
        End Enum
        '
        '====================================================================================================
        ''' <summary>
        ''' status of the app in the appConfigModel. Only applies to the app loaded in the serverstatus.appconfig
        ''' </summary>
        Public Enum appStatusEnum
            notFound = 0
            notEnabled = 1
            ready = 2
            loading = 3
            errorKernelFailure = 6     ' can not create Kernel
            errorNoHostService = 7     ' host service process ID not set
            errorLicenseFailure = 8    ' failed to start because of License failure
            errorDbNotFound = 9         ' failed to start because ccSetup table not found
            errorFailedToInitialize = 10   ' failed to start because of unknown error, see trace log
            errorDbBad = 11            ' ccContent,ccFields no records found
            errorConnectionObjectFailure = 12 ' Connection Object FAiled
            errorConnectionStringFailure = 13 ' Connection String FAiled to open the ODBC connection
            errorDataSourceFailure = 14 ' DataSource failed to open
            errorDuplicateDomains = 15 ' Can not locate application because there are 1+ apps that match the domain
            paused = 16           ' Running, but all activity is blocked (for backup)
            errorAppConfigNotFound = 17
            errorAppConfigNotValid = 18
            errorDbFoundButContentMetaMissing = 19
        End Enum
        '
        '====================================================================================================
        ''' <summary>
        ''' Create an empty object. needed for deserialization. Use newModel() method as constructor, includes cache
        ''' </summary>
        Public Sub New()
            '
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' get ServerConfig, returning only the server data section without specific serverConfig.app
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Function getObject(cpCore As coreClass) As Models.Entity.serverConfigModel
            Dim returnModel As Models.Entity.serverConfigModel = Nothing
            Try
                Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer
                Dim JSONTemp As String
                '
                ' ----- read/create serverConfig
                '
                JSONTemp = cpCore.programDataFiles.readFile("config.json")
                If String.IsNullOrEmpty(JSONTemp) Then
                    '
                    ' for now it fails, maybe later let it autobuild a local cluster
                    '
                    returnModel = New Models.Entity.serverConfigModel
                    returnModel.allowTaskRunnerService = False
                    returnModel.allowTaskSchedulerService = False
                    cpCore.programDataFiles.saveFile("config.json", json_serializer.Serialize(returnModel))
                Else
                    returnModel = json_serializer.Deserialize(Of serverConfigModel)(JSONTemp)
                End If
                '
                ' -- block the configured app that last saved the server model
                returnModel.appConfig = Nothing
            Catch ex As Exception
                cpCore.handleException(ex, "exception in serverConfigModel.getObject")
            End Try
            Return returnModel
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the server configuration and assign an application to the appConf
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Function getObject(cpCore As coreClass, appName As String) As Models.Entity.serverConfigModel
            Dim returnModel As Models.Entity.serverConfigModel = Nothing
            Try
                returnModel = getObject(cpCore)
                If (Not String.IsNullOrEmpty(appName)) Then
                    If (Not returnModel.apps.ContainsKey(appName.ToLower())) Then
                        '
                        ' -- application now configured
                        returnModel.appConfig = Nothing
                        Throw New Exception("application [" & appName & "] was not found in this server group.")
                    Else
                        returnModel.appConfig = returnModel.apps(appName.ToLower())
                        returnModel.appConfig.appStatus = appStatusEnum.ready
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex, "exception in serverConfigModel.getObject")
            End Try
            Return returnModel
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Save the object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Function saveObject(cpCore As coreClass) As Integer
            Try
                Dim jsonTemp As String = cpCore.json.Serialize(Me)
                cpCore.programDataFiles.saveFile("config.json", jsonTemp)
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return 0
        End Function
    End Class
End Namespace

