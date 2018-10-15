
Option Explicit On
Option Strict On

Imports System.Web.SessionState
Imports Contensive.Processor
Imports Contensive.Processor.Controllers
Imports Contensive.Processor.Controllers.GenericController
Imports System.Web.Routing
Imports System.IO
Imports Contensive.Processor.Models.Domain

Public Class ConfigurationClass
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
    Public Shared Function getServerConfig() As ServerConfigModel
        Dim serverConfig As New ServerConfigModel()
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
            serverConfig.enableLocalMemoryCache = GenericController.encodeBoolean(ConfigurationManager.AppSettings("ContensiveIsLocalCache"))
            serverConfig.isLocalFileSystem = GenericController.encodeBoolean(ConfigurationManager.AppSettings("ContensiveIsLocalFileSystem"))
            serverConfig.localDataDriveLetter = ConfigurationManager.AppSettings("ContensiveLocalDataDriveLetter")
            serverConfig.name = ConfigurationManager.AppSettings("ContensiveServerGroupName")
            serverConfig.password = ConfigurationManager.AppSettings("ContensiveServerGroupPassword")
            serverConfig.programFilesPath = ""
            serverConfig.username = ConfigurationManager.AppSettings("ContensiveServerGroupUsername")
            Dim appConfig As New AppConfigModel
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
    Public Shared Sub loadRouteMap(cp As CPClass)
        LogController.forceNLog("configurationClass, loadRouteMap, [" + cp.Site.Name + "]", LogController.logLevel.Trace)
        '
        ' test if route map needs to be loaded (routeMap.dateCreated <> application[routeMapDateCreated])
        If (Not cp.routeMap.dateCreated.Equals(HttpContext.Current.Application("RouteMapDateCreated"))) Then
            ' 20180307, added clear to resolve error 
            RouteTable.Routes.Clear()
            For Each kvp In cp.routeMap.routeDictionary
                Try
                    Dim newRouteName As String = kvp.Key
                    Dim newRoute As RouteMapModel.routeClass = kvp.Value
                    '
                    LogController.forceNLog("configurationClass, loadRouteMap, [" + cp.Site.Name + "] [" + newRoute.virtualRoute + "], [" + newRoute.physicalRoute + "]", LogController.logLevel.Trace)
                    '
                    RouteTable.Routes.Remove(RouteTable.Routes(newRouteName))
                    RouteTable.Routes.MapPageRoute(newRoute.virtualRoute, newRoute.virtualRoute, newRoute.physicalRoute)
                Catch ex As Exception
                    cp.Site.ErrorReport(ex, "Unexpected exception adding virtualRoute [" & kvp.Key & "]")
                End Try
            Next
            HttpContext.Current.Application("routeMapDateCreated") = cp.routeMap.dateCreated
        End If
    End Sub
End Class

