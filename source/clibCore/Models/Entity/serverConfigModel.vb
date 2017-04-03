
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
        ' -- old serverConfig
        'Public clusterPath As String
        Public allowTaskRunnerService As Boolean
        Public allowTaskSchedulerService As Boolean
        '
        ' -- old clusterConfig
        Public name As String = ""
        '
        ' -- AWS dotnet elaticcache client wraps enyim, and provides node autodiscovery through the configuration object. this is the srver:port to the config file it uses.
        Public isLocalCache As Boolean = False
        Public awsElastiCacheConfigurationEndpoint As String
        '
        ' -- datasource for the cluster (only sql support for now)
        Public defaultDataSourceType As dataSourceTypeEnum
        Public defaultDataSourceAddress As String = ""
        Public defaultDataSourceUsername As String = ""
        Public defaultDataSourcePassword As String = ""
        '
        ' -- endpoint for cluster files (not sure how it works, maybe this will be an object taht includes permissions, for now an fpo)
        Public isLocalFileSystem As Boolean = True
        Public cdnFilesRemoteEndpoint As String
        '
        ' -- configuration of async command listener on render machines (not sure if used still)
        Public serverListenerPort As Integer = Port_ContentServerControlDefault
        Public maxConcurrentTasksPerServer As Integer = 5
        Public username As String = ""
        Public password As String = ""
        '
        ' -- deprecated
        Public appPattern As String
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
            Public name As String
            Public appStatus As applicationStatusEnum
            Public enabled As Boolean
            Public privateKey As String                     ' rename hashKey
            Public defaultConnectionString As String
            Public appRootFilesPath As String               ' local file path to the appRoot (i.e. d:\inetpub\myApp\wwwRoot\)
            Public cdnFilesPath As String                   ' local file path to the content files (i.e. d:\inetpub\myApp\files\)
            Public privateFilesPath As String               ' local file path to the content files (i.e. d:\inetpub\myApp\private\)
            Public cdnFilesNetprefix As String              ' in some cases (like legacy), cdnFiles are iis virtual folder mapped to appRoot (/appName/files/). Some cases this is a URL (http:\\cdn.domain.com pointing to s3)
            Public allowSiteMonitor As Boolean
            Public domainList As New List(Of String)        ' primary domain is the first item in the list
            Public enableCache As Boolean
            Public adminRoute As String                                          ' The url pathpath that executes the addon site
        End Class    '
        '
        '====================================================================================================
        ''' <summary>
        ''' status of the app in the appConfigModel. Only applies to the app loaded in the serverstatus.appconfig
        ''' </summary>
        Public Enum applicationStatusEnum
            ApplicationStatusNotFound = 0
            ApplicationStatusNotEnabled = 1
            ApplicationStatusReady = 2
            ApplicationStatusLoading = 3
            ApplicationStatusUpgrading = 4
            ' ApplicationStatusConnectionBusy = 5    ' can not open connection because already open
            ApplicationStatusKernelFailure = 6     ' can not create Kernel
            ApplicationStatusNoHostService = 7     ' host service process ID not set
            ApplicationStatusLicenseFailure = 8    ' failed to start because of License failure
            ApplicationStatusDbNotFound = 9         ' failed to start because ccSetup table not found
            ApplicationStatusFailedToInitialize = 10   ' failed to start because of unknown error, see trace log
            ApplicationStatusDbBad = 11            ' ccContent,ccFields no records found
            ApplicationStatusConnectionObjectFailure = 12 ' Connection Object FAiled
            ApplicationStatusConnectionStringFailure = 13 ' Connection String FAiled to open the ODBC connection
            ApplicationStatusDataSourceFailure = 14 ' DataSource failed to open
            ApplicationStatusDuplicateDomains = 15 ' Can not locate application because there are 1+ apps that match the domain
            ApplicationStatusPaused = 16           ' Running, but all activity is blocked (for backup)
            ApplicationStatusAppConfigNotFound = 17
            ApplicationStatusAppConfigNotValid = 18
            ApplicationStatusDbFoundButContentMetaMissing = 19
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
                cpCore.handleExceptionAndContinue(ex, "exception in serverConfigModel.getObject")
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
                        returnModel.appConfig = New appConfigModel()
                        returnModel.appConfig.appStatus = applicationStatusEnum.ApplicationStatusAppConfigNotValid
                        Throw New Exception("application [" & appName & "] was not found in this server group.")
                    Else
                        returnModel.appConfig = returnModel.apps(appName.ToLower())
                    End If
                    'If vbInstr(1, returnModel.appConfig.domainList(0), ",") > 1 Then
                    '    '
                    '    ' if first entry in domain list is comma delimited, save only the first entry
                    '    '
                    '    returnModel.appConfig.domainList(0) = Mid(returnModel.appConfig.domainList(0), 1, vbInstr(1, returnModel.appConfig.domainList(0), ",") - 1)
                    'End If
                    returnModel.appConfig.appStatus = applicationStatusEnum.ApplicationStatusReady
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex, "exception in serverConfigModel.getObject")
            End Try
            Return returnModel
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' called only from getObject. Load the model from the Db without cache. If there are any properties or objects that cannot be used from cache, do not include them here either, load in getObject()
        ''' </summary>
        ''' <param name="recordId"></param>
        Private Shared Function getObjectNoCache(cp As CPBaseClass, recordId As Integer) As Models.Entity.blankCachedModel
            Dim returnNewModel As New blankCachedModel()
            Try
                Dim cs As CPCSBaseClass = cp.CSNew()
                returnNewModel.id = 0
                If recordId <> 0 Then
                    cs.Open(cnBlank, "(ID=" & recordId & ")")
                    If cs.OK() Then
                        returnNewModel.id = recordId
                        returnNewModel.name = cs.GetText("Name")
                    End If
                    Call cs.Close()
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return returnNewModel
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
                cpCore.handleExceptionAndContinue(ex)
            End Try
            Return 0
        End Function
    End Class
End Namespace

