
Option Explicit On
Option Strict On

Imports System.Web.SessionState
Imports Contensive.Processor
Imports Contensive.Processor.Controllers
Imports Contensive.Processor.Controllers.GenericController
Imports System.Web.Routing
Imports System.IO
Imports Contensive
Imports Contensive.Processor.Models.Domain

Public Class ConfigurationClass
    '
    '====================================================================================================
    ''' <summary>
    ''' if true, the route map is not loaded or invalid and needs to be loaded
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function routeMapDateInvalid() As Boolean
        If (HttpContext.Current.Application("RouteMapDateCreated") Is Nothing) Then Return True
        Dim dateResult As Date
        Return (Not Date.TryParse(HttpContext.Current.Application("RouteMapDateCreated").ToString(), dateResult))
    End Function
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
            serverConfig.awsRegionName = ConfigurationManager.AppSettings("ContensiveAwsRegionName")
            serverConfig.awsAccessKey = ConfigurationManager.AppSettings("ContensiveAwsAccessKey")
            serverConfig.awsSecretAccessKey = ConfigurationManager.AppSettings("ContensiveAwsSecretAccessKey")
            serverConfig.defaultDataSourceAddress = ConfigurationManager.AppSettings("ContensiveDefaultDataSourceAddress")
            serverConfig.defaultDataSourcePassword = ConfigurationManager.AppSettings("ContensiveDefaultDataSourcePassword")
            serverConfig.defaultDataSourceUsername = ConfigurationManager.AppSettings("ContensiveDefaultDataSourceUsername")
            serverConfig.enableLocalMemoryCache = GenericController.encodeBoolean(ConfigurationManager.AppSettings("ContensiveIsLocalCache"))
            serverConfig.isLocalFileSystem = GenericController.encodeBoolean(ConfigurationManager.AppSettings("ContensiveIsLocalFileSystem"))
            serverConfig.localDataDriveLetter = ConfigurationManager.AppSettings("ContensiveLocalDataDriveLetter")
            serverConfig.name = ConfigurationManager.AppSettings("ContensiveServerGroupName")
            'serverConfig.password = ConfigurationManager.AppSettings("ContensiveServerGroupPassword")
            serverConfig.programFilesPath = ""
            'serverConfig.username = ConfigurationManager.AppSettings("ContensiveServerGroupUsername")
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
            serverConfig.apps.Add(appConfig.name.ToLowerInvariant(), appConfig)
        Catch ex As Exception
            'Logger.appendProgramDataLog(ex.ToString)
        End Try
        Return serverConfig
    End Function
    '
    Public Shared Sub loadRouteMap(cp As CPClass)
        '
        ' -- if application var does not equal routemap.datecreated rebuild
        If (routeMapDateInvalid() OrElse (cp.routeMap.dateCreated <> CDate(HttpContext.Current.Application("RouteMapDateCreated")))) Then
            '
            If routeMapDateInvalid() Then
                LogController.logLocalOnly("configurationClass, loadRouteMap, [" + cp.Site.Name + "], rebuild because HttpContext.Current.Application(RouteMapDateCreated) is not valid", BaseClasses.CPLogBaseClass.LogLevel.Info)
            Else
                LogController.logLocalOnly("configurationClass, loadRouteMap, [" + cp.Site.Name + "], rebuild because not equal, cp.routeMap.dateCreated [" + cp.routeMap.dateCreated.ToString() + "], HttpContext.Current.Application(RouteMapDateCreated) [" + HttpContext.Current.Application("RouteMapDateCreated").ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Info)
            End If
            '
            HttpContext.Current.Application("routeMapDateCreated") = cp.routeMap.dateCreated
            SyncLock RouteTable.Routes
                ' 20180307, added clear to resolve error 
                RouteTable.Routes.Clear()
                For Each newRouteKeyValuePair In cp.routeMap.routeDictionary
                    Try
                        '
                        LogController.logLocalOnly("configurationClass, loadRouteMap, [" + cp.Site.Name + "] [" + newRouteKeyValuePair.Value.virtualRoute + "], [" + newRouteKeyValuePair.Value.physicalRoute + "]", BaseClasses.CPLogBaseClass.LogLevel.Trace)
                        '
                        RouteTable.Routes.Remove(RouteTable.Routes(newRouteKeyValuePair.Key))
                        RouteTable.Routes.MapPageRoute(newRouteKeyValuePair.Value.virtualRoute, newRouteKeyValuePair.Value.virtualRoute, newRouteKeyValuePair.Value.physicalRoute)
                    Catch ex As Exception
                        cp.Site.ErrorReport(ex, "Unexpected exception adding virtualRoute [" & newRouteKeyValuePair.Key & "]")
                    End Try
                Next
            End SyncLock
            LogController.logLocalOnly("configurationClass, loadRouteMap, [" + cp.Site.Name + "] done", BaseClasses.CPLogBaseClass.LogLevel.Info)
        End If
    End Sub
End Class

