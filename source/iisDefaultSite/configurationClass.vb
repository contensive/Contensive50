
Option Explicit On
Option Strict On

Imports Contensive.Core
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports System.Web.Routing
Imports System.IO
Imports Contensive.Core.Models.Complex
Imports Contensive.Core.Models.Context

Public Class configurationClass
    '
    '====================================================================================================
    ''' <summary>
    ''' determine the Contensive application name from the webconfig or iis sitename
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function getAppName() As String
        '
        ' -- app name matches iis site name unless overridden by aspx app setting "ContensiveAppName"
        Dim appName As String = ConfigurationManager.AppSettings("ContensiveAppName")
        If (String.IsNullOrEmpty(appName)) Then
            appName = System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteName()
        End If
        Return appName
    End Function
    '
    '====================================================================================================
    ''' <summary>
    ''' Create serverConfig object from appConfig or webConfig. This is alternative to configuration with config.json file
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function getServerConfig() As serverConfigModel
        Dim serverConfig As New serverConfigModel()
        Try
            serverConfig.allowTaskRunnerService = False
            serverConfig.allowTaskSchedulerService = False
            serverConfig.awsBucketName = ConfigurationManager.AppSettings("ContensiveAwsBucketName")
            serverConfig.awsBucketRegionName = ConfigurationManager.AppSettings("ContensiveAwsBucketRegionName")
            serverConfig.awsAccessKey = ConfigurationManager.AppSettings("ContensiveAwsAccessKey")
            serverConfig.awsSecretAccessKey = ConfigurationManager.AppSettings("ContensiveAwsSecretAccessKey")
            serverConfig.defaultDataSourceAddress = ConfigurationManager.AppSettings("ContensiveDefaultDataSourceAddress")
            serverConfig.defaultDataSourcePassword = ConfigurationManager.AppSettings("ContensiveDefaultDataSourcePassword")
            serverConfig.defaultDataSourceUsername = ConfigurationManager.AppSettings("ContensiveDefaultDataSourceUsername")
            serverConfig.enableLocalMemoryCache = genericController.encodeBoolean(ConfigurationManager.AppSettings("ContensiveIsLocalCache"))
            serverConfig.isLocalFileSystem = genericController.encodeBoolean(ConfigurationManager.AppSettings("ContensiveIsLocalFileSystem"))
            serverConfig.localDataDriveLetter = ConfigurationManager.AppSettings("ContensiveLocalDataDriveLetter")
            serverConfig.name = ConfigurationManager.AppSettings("ContensiveServerGroupName")
            serverConfig.password = ConfigurationManager.AppSettings("ContensiveServerGroupPassword")
            serverConfig.programFilesPath = ""
            serverConfig.username = ConfigurationManager.AppSettings("ContensiveServerGroupUsername")
            Dim appConfig As New appConfigModel
            appConfig.name = getAppName()
            appConfig.adminRoute = ConfigurationManager.AppSettings("ContensiveAdminRoute")
            appConfig.localWwwPath = ConfigurationManager.AppSettings("ContensiveAppRootFilesPath")
            appConfig.remoteFilePath = ConfigurationManager.AppSettings("ContensiveCdnFilesNetprefix")
            appConfig.localFilesPath = ConfigurationManager.AppSettings("ContensiveCdnFilesPath")
            appConfig.domainList.Add(ConfigurationManager.AppSettings("ContensivePrimaryDomain"))
            appConfig.enabled = True
            appConfig.localPrivatePath = ConfigurationManager.AppSettings("ContensivePrivateFilesPath")
            appConfig.privateKey = ConfigurationManager.AppSettings("ContensivePrivateKey")
            serverConfig.apps.Add(appConfig.name.ToLower(), appConfig)
        Catch ex As Exception
            Logger.appendProgramDataLog(ex.ToString)
        End Try
        Return serverConfig
    End Function
    '
    Public Shared Sub loadRouteMap(cp As Contensive.BaseClasses.CPBaseClass)
        logController.forceNLog("configurationClass, loadRouteMap", logController.logLevel.Trace)
        ' 20180307, added clear to resolve error 
        RouteTable.Routes.Clear()
        For Each kvp In cp.Site.getRouteDictionary()
            Try
                Dim newRouteName As String = kvp.Key
                Dim newRoute As Contensive.BaseClasses.CPSiteBaseClass.routeClass = kvp.Value
                '
                logController.forceNLog("configurationClass, loadRouteMap [" + newRoute.virtualRoute + "], [" + newRoute.physicalRoute + "]", logController.logLevel.Trace)
                '
                RouteTable.Routes.Remove(RouteTable.Routes(newRouteName))
                RouteTable.Routes.MapPageRoute(newRoute.virtualRoute, newRoute.virtualRoute, newRoute.physicalRoute)
            Catch ex As Exception
                cp.Site.ErrorReport(ex, "Unexpected exception adding virtualRoute [" & kvp.Key & "]")
            End Try
        Next
    End Sub
End Class

