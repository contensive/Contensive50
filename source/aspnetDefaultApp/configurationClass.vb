
Option Explicit On
Option Strict On

Imports Contensive.Core
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports System.Web.Routing
Imports System.IO

Public Class configurationClass
    Public Shared Function getServerConfig() As Contensive.Core.Models.Entity.serverConfigModel
        Logger.LogInfo("getServerConfig")
        Dim serverConfig As Contensive.Core.Models.Entity.serverConfigModel = Nothing
        Try
            serverConfig = New Contensive.Core.Models.Entity.serverConfigModel
            serverConfig.appConfig = New Contensive.Core.Models.Entity.serverConfigModel.appConfigModel
            '
            serverConfig.allowTaskRunnerService = False
            serverConfig.allowTaskSchedulerService = False
            serverConfig.appConfig.adminRoute = ConfigurationManager.AppSettings("ContensiveAdminRoute")
            serverConfig.appConfig.appRootFilesPath = ConfigurationManager.AppSettings("ContensiveAppRootFilesPath")
            serverConfig.appConfig.cdnFilesNetprefix = ConfigurationManager.AppSettings("ContensiveCdnFilesNetprefix")
            serverConfig.appConfig.cdnFilesPath = ConfigurationManager.AppSettings("ContensiveCdnFilesPath")
            serverConfig.appConfig.domainList.Add(ConfigurationManager.AppSettings("ContensivePrimaryDomain"))
            serverConfig.appConfig.enableCache = genericController.EncodeBoolean(ConfigurationManager.AppSettings("ContensiveEnableCache"))
            serverConfig.appConfig.enabled = True
            serverConfig.appConfig.name = ConfigurationManager.AppSettings("ContensiveAppName")
            serverConfig.appConfig.privateFilesPath = ConfigurationManager.AppSettings("ContensivePrivateFilesPath")
            serverConfig.appConfig.privateKey = ConfigurationManager.AppSettings("ContensivePrivateKey")
            serverConfig.apps.Add(serverConfig.appConfig.name, serverConfig.appConfig)
            serverConfig.cdnFilesRemoteEndpoint = ConfigurationManager.AppSettings("ContensiveCdnFilesRemoteEndpoint")
            serverConfig.defaultDataSourceAddress = ConfigurationManager.AppSettings("ContensiveDefaultDataSourceAddress")
            serverConfig.defaultDataSourcePassword = ConfigurationManager.AppSettings("ContensiveDefaultDataSourcePassword")
            'serverConfig.defaultDataSourceType = genericController.EncodeInteger(ConfigurationManager.AppSettings("ContensiveDefaultDataSourceType"))
            serverConfig.defaultDataSourceUsername = ConfigurationManager.AppSettings("ContensiveDefaultDataSourceUsername")
            serverConfig.isLocalCache = genericController.EncodeBoolean(ConfigurationManager.AppSettings("ContensiveIsLocalCache"))
            serverConfig.isLocalFileSystem = genericController.EncodeBoolean(ConfigurationManager.AppSettings("ContensiveIsLocalFileSystem"))
            serverConfig.localDataDriveLetter = ConfigurationManager.AppSettings("ContensiveLocalDataDriveLetter")
            serverConfig.name = ConfigurationManager.AppSettings("ContensiveServerGroupName")
            serverConfig.password = ConfigurationManager.AppSettings("ContensiveServerGroupPassword")
            serverConfig.programFilesPath = ""
            serverConfig.username = ConfigurationManager.AppSettings("ContensiveServerGroupUsername")
        Catch ex As Exception
            Logger.LogInfo(ex)
        End Try
        Return serverConfig
    End Function
    '
    Public Shared Sub RegisterRoutes(cp As Contensive.Core.CPClass, ByVal routes As RouteCollection)
        Logger.LogInfo("RegisterRoutes")
        Try
            Dim cs As Contensive.BaseClasses.CPCSBaseClass = cp.CSNew
            '
            ' -- drive all routes to the default page
            Dim physicalFile As String = "~/" & cp.Site.GetText("serverpagedefault")
            Dim adminRoute As String = cp.core.serverConfig.appConfig.adminRoute
            If (Not String.IsNullOrEmpty(adminRoute)) Then
                '
                ' -- register admin route
                Try
                    adminRoute.Replace("\", "/")
                    If (adminRoute.Substring(0, 1) = "/"c) Then
                        adminRoute = adminRoute.Substring(1)
                    End If
                    cp.Utils.AppendLog("RegisterRoutes, admin route, serverConfig.appConfig.adminRoute [" & adminRoute & "]")
                    routes.MapPageRoute("Admin Route", adminRoute, physicalFile)
                Catch ex As Exception
                    cp.Site.ErrorReport(ex, "Exception while adding admin route")
                End Try
            End If
            '
            Dim remoteMethods As List(Of Contensive.Core.Models.Entity.addonModel) = Contensive.Core.Models.Entity.addonModel.getRemoteMethods(cp.core)
            cp.Utils.AppendLog("RegisterRoutes, remoteMethods.Count=" & remoteMethods.Count.ToString())
            For Each remoteMethod As Contensive.Core.Models.Entity.addonModel In remoteMethods
                '
                ' -- register each remote method
                Try
                    Dim routeName As String = remoteMethod.name
                    Dim routeUrl As String = remoteMethod.name
                    cp.Utils.AppendLog("RegisterRoutes, remoteMethods, routeName=[" & routeName & "], routeUrl =[" & routeUrl & "], physicalFile=[" & physicalFile & "]")
                    routes.MapPageRoute(routeName, routeUrl, physicalFile)
                Catch ex As Exception
                    cp.Site.ErrorReport(ex, "Exception while adding remote method routes")
                End Try
            Next
            '
            ' -- if page manager is used, register link aliases and link forwards
            If (cs.Open("link forwards", "name is not null")) Then
                Do
                    Dim routeName As String = cs.GetText("name")
                    Dim routeUrl As String = routeName
                    cp.Utils.AppendLog("RegisterRoutes, link aliases, routeName=[" & routeName & "], routeUrl =[" & routeUrl & "], physicalFile=[" & physicalFile & "]")
                    routes.MapPageRoute(routeName, routeUrl, physicalFile)
                    cs.GoNext()
                Loop While cs.OK()
            End If
            cs.Close()
            '
            ' -- if page manager is used, register link aliases and link forwards
            If (cs.Open("link aliases", "name is not null")) Then
                Do
                    Dim routeName As String = cs.GetText("name")
                    Dim routeUrl As String = cs.GetText("SourceLink")
                    cp.Utils.AppendLog("RegisterRoutes, link aliases, routeName=[" & routeName & "], routeUrl =[" & routeUrl & "], physicalFile=[" & physicalFile & "]")
                    routes.MapPageRoute(routeName, routeUrl, physicalFile)
                    cs.GoNext()
                Loop While cs.OK()
            End If
            cs.Close()
        Catch ex As Exception
            Logger.LogInfo(ex)
        End Try
    End Sub
    '
    Private Shared Function EncodeBoolean(source As String) As Boolean
        Return Equals(source.ToLower(), "true") Or Equals(source.ToLower(), "yes") Or Equals(source.ToLower(), "on") Or (IsNumeric(source) And (source <> "0"))
    End Function
    '

    ' 

    ' This class is responsible for tracing application errors during runtime. It writes information to log file describing cause of error, file name & line number etc 
    '


    '
End Class
