
Option Explicit On
Option Strict On

Imports Contensive.Core
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports System.Web.Routing
Imports System.IO

Public Class configurationClass
    Public Shared Function getServerConfig() As Contensive.Core.Models.Entity.serverConfigModel
        Dim serverConfig As Contensive.Core.Models.Entity.serverConfigModel = Nothing
        Try
            serverConfig = New Contensive.Core.Models.Entity.serverConfigModel
            serverConfig.appConfig = New Contensive.Core.Models.Entity.serverConfigModel.appConfigModel
            '
            serverConfig.allowTaskRunnerService = False
            serverConfig.allowTaskSchedulerService = False
            serverConfig.appConfig.name = ConfigurationManager.AppSettings("ContensiveAppName")
            'If (String.IsNullOrEmpty(serverConfig.appConfig.name)) Then
            '    serverConfig.appConfig.name = System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteName()
            'End If
            serverConfig.appConfig.adminRoute = ConfigurationManager.AppSettings("ContensiveAdminRoute")
            serverConfig.appConfig.appRootFilesPath = ConfigurationManager.AppSettings("ContensiveAppRootFilesPath")
            serverConfig.appConfig.cdnFilesNetprefix = ConfigurationManager.AppSettings("ContensiveCdnFilesNetprefix")
            serverConfig.appConfig.cdnFilesPath = ConfigurationManager.AppSettings("ContensiveCdnFilesPath")
            serverConfig.appConfig.domainList.Add(ConfigurationManager.AppSettings("ContensivePrimaryDomain"))
            serverConfig.appConfig.enabled = True
            serverConfig.appConfig.privateFilesPath = ConfigurationManager.AppSettings("ContensivePrivateFilesPath")
            serverConfig.appConfig.privateKey = ConfigurationManager.AppSettings("ContensivePrivateKey")
            serverConfig.apps.Add(serverConfig.appConfig.name, serverConfig.appConfig)
            serverConfig.cdnFilesRemoteEndpoint = ConfigurationManager.AppSettings("ContensiveCdnFilesRemoteEndpoint")
            serverConfig.defaultDataSourceAddress = ConfigurationManager.AppSettings("ContensiveDefaultDataSourceAddress")
            serverConfig.defaultDataSourcePassword = ConfigurationManager.AppSettings("ContensiveDefaultDataSourcePassword")
            'serverConfig.defaultDataSourceType = genericController.EncodeInteger(ConfigurationManager.AppSettings("ContensiveDefaultDataSourceType"))
            serverConfig.defaultDataSourceUsername = ConfigurationManager.AppSettings("ContensiveDefaultDataSourceUsername")
            serverConfig.enableLocalMemoryCache = genericController.EncodeBoolean(ConfigurationManager.AppSettings("ContensiveIsLocalCache"))
            serverConfig.isLocalFileSystem = genericController.EncodeBoolean(ConfigurationManager.AppSettings("ContensiveIsLocalFileSystem"))
            serverConfig.localDataDriveLetter = ConfigurationManager.AppSettings("ContensiveLocalDataDriveLetter")
            serverConfig.name = ConfigurationManager.AppSettings("ContensiveServerGroupName")
            serverConfig.password = ConfigurationManager.AppSettings("ContensiveServerGroupPassword")
            serverConfig.programFilesPath = ""
            serverConfig.username = ConfigurationManager.AppSettings("ContensiveServerGroupUsername")
        Catch ex As Exception
            Logger.appendProgramDataLog(ex.ToString)
        End Try
        Return serverConfig
    End Function
    '
    Public Shared Sub RegisterRoutes(cp As Contensive.Core.CPClass, ByVal routes As RouteCollection)
        Try
            Dim cs As Contensive.BaseClasses.CPCSBaseClass = cp.CSNew
            '
            ' -- drive all routes to the default page
            Dim defaultPage As String = cp.Site.GetText("serverpagedefault", "default.aspx")
            Dim physicalFile As String = "~/" & defaultPage
            Dim adminRoute As String = cp.core.serverConfig.appConfig.adminRoute
            If (Not String.IsNullOrEmpty(adminRoute)) Then
                '
                ' -- register admin route
                Try
                    adminRoute.Replace("\", "/")
                    If (adminRoute.Substring(0, 1) = "/"c) Then
                        adminRoute = adminRoute.Substring(1)
                    End If
                    appendLog(cp, "RegisterRoutes, admin route, serverConfig.appConfig.adminRoute [" & adminRoute & "]")
                    routes.MapPageRoute("Admin Route", adminRoute, physicalFile)
                Catch ex As Exception
                    cp.Site.ErrorReport(ex, "Exception while adding admin route")
                End Try
            End If
            '
            Dim remoteMethods As List(Of Contensive.Core.Models.Entity.addonModel) = Contensive.Core.Models.Entity.addonModel.createList_RemoteMethods(cp.core, New List(Of String))
            appendLog(cp, "RegisterRoutes, remoteMethods.Count=" & remoteMethods.Count.ToString())
            For Each remoteMethod As Contensive.Core.Models.Entity.addonModel In remoteMethods
                '
                ' -- register each remote method
                Try
                    Dim routeName As String = remoteMethod.Name
                    Dim routeUrl As String = remoteMethod.Name
                    appendLog(cp, "RegisterRoutes, remoteMethods, routeName=[" & routeName & "], routeUrl =[" & routeUrl & "], physicalFile=[" & physicalFile & "]")
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
                    appendLog(cp, "RegisterRoutes, link aliases, routeName=[" & routeName & "], routeUrl =[" & routeUrl & "], physicalFile=[" & physicalFile & "]")
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
                    appendLog(cp, "RegisterRoutes, link aliases, routeName=[" & routeName & "], routeUrl =[" & routeUrl & "], physicalFile=[" & physicalFile & "]")
                    routes.MapPageRoute(routeName, routeUrl, physicalFile)
                    cs.GoNext()
                Loop While cs.OK()
            End If
            cs.Close()
        Catch ex As Exception
            Logger.appendProgramDataLog(ex.ToString())
        End Try
    End Sub
    '
    Private Shared Function EncodeBoolean(source As String) As Boolean
        Return Equals(source.ToLower(), "true") Or Equals(source.ToLower(), "yes") Or Equals(source.ToLower(), "on") Or (IsNumeric(source) And (source <> "0"))
    End Function
    '
    Public Shared Sub appendLog(cp As Contensive.BaseClasses.CPBaseClass, message As String)
        cp.Utils.AppendLog("aspnet\", message)
        'cp.Utils.AppendLog("aspnet\" & Now.Year.ToString & Now.Month.ToString().PadLeft(2, "0"c) & Now.Day.ToString().PadLeft(2, "0"c) & ".log", message)
        Trace.WriteLine(message)
    End Sub
    '
    ' 
    ' This class is responsible for tracing application errors during runtime. It writes information to log file describing cause of error, file name & line number etc 
    '


    '
End Class
