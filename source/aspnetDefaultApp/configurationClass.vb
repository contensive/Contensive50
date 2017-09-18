
Option Explicit On
Option Strict On

Imports Contensive.Core
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports System.Web.Routing
Imports System.IO

Public Class configurationClass
    '
    ''' <summary>
    ''' determine the Contensive application name from the webconfig or iis sitename
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function getAppName() As String
        Dim appName As String = ConfigurationManager.AppSettings("ContensiveAppName")
        If (String.IsNullOrEmpty(appName)) Then
            appName = System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteName()
        End If
        Return appName
    End Function
    Public Shared Function getServerConfig() As Contensive.Core.Models.Entity.serverConfigModel
        Dim serverConfig As Contensive.Core.Models.Entity.serverConfigModel = Nothing
        Try
            serverConfig = New Contensive.Core.Models.Entity.serverConfigModel
            serverConfig.appConfig = New Contensive.Core.Models.Entity.serverConfigModel.appConfigModel
            '
            serverConfig.allowTaskRunnerService = False
            serverConfig.allowTaskSchedulerService = False
            serverConfig.appConfig.name = getAppName()
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
            Dim routesAdded As New List(Of String)
            '
            registerRoute(cp, adminRoute, routesAdded, routes, physicalFile)
            'If (Not String.IsNullOrEmpty(adminRoute)) Then
            '    '
            '    ' -- register admin route
            '    Try
            '        Dim route As String = genericController.convertToUnixSlash(adminRoute.Trim())
            '        If (route.Substring(0, 1).Equals("/")) Then
            '            route = adminRoute.Substring(1)
            '        End If
            '        If (Not String.IsNullOrEmpty(route)) Then

            '        End If
            '        If (Not routesAdded.Contains(route)) Then
            '            appendLog(cp, "RegisterRoutes, admin route, serverConfig.appConfig.adminRoute [" & route & "]")
            '            routes.MapPageRoute("Admin Route", route, physicalFile)
            '            routesAdded.Add(route)
            '        End If
            '    Catch ex As Exception
            '        cp.Site.ErrorReport(ex, "Exception while adding admin route")
            '    End Try
            'End If
            '
            Dim remoteMethods As List(Of Contensive.Core.Models.Entity.addonModel) = Contensive.Core.Models.Entity.addonModel.createList_RemoteMethods(cp.core, New List(Of String))
            For Each remoteMethod As Contensive.Core.Models.Entity.addonModel In remoteMethods
                registerRoute(cp, remoteMethod.name, routesAdded, routes, physicalFile)
                ''
                '' -- register each remote method
                'Try
                '    Dim routeName As String = remoteMethod.name.Trim()
                '    ' routes.
                '    If (Not routesAdded.Contains(routeName)) Then
                '        appendLog(cp, "RegisterRoutes, remoteMethods, routeName=[" & routeName & "], physicalFile=[" & physicalFile & "]")
                '        routes.MapPageRoute(routeName, routeName, physicalFile)
                '        routesAdded.Add(adminRoute)
                '    End If
                'Catch ex As Exception
                '    cp.Site.ErrorReport(ex, "Exception while adding remote method routes")
                'End Try
            Next
            '
            ' -- if page manager is used, register link aliases and link forwards
            Dim linkForwards As List(Of Models.Entity.linkForwardModel) = Models.Entity.linkForwardModel.createList(cp.core, "name Is Not null")
            For Each linkForward As Models.Entity.linkForwardModel In linkForwards
                registerRoute(cp, linkForward.name, routesAdded, routes, physicalFile)
            Next
            'If (cs.Open("link forwards", "name is not null")) Then
            '    Do
            '        Dim routeName As String = cs.GetText("name")
            '        appendLog(cp, "RegisterRoutes, link aliases, routeName=[" & routeName & "], physicalFile=[" & physicalFile & "]")
            '        routes.MapPageRoute(routeName, routeName, physicalFile)
            '        cs.GoNext()
            '    Loop While cs.OK()
            'End If
            'cs.Close()
            '
            ' -- if page manager is used, register link aliases and link forwards
            Dim linkAliasList As List(Of Models.Entity.linkAliasModel) = Models.Entity.linkAliasModel.createList(cp.core, "name Is Not null")
            For Each linkAlias As Models.Entity.linkAliasModel In linkAliasList
                registerRoute(cp, linkAlias.name, routesAdded, routes, physicalFile)
            Next

            'If (cs.Open("link aliases", "name is not null")) Then
            '    Do
            '        Dim routeName As String = genericController.convertToUnixSlash(cs.GetText("name").Trim())
            '        If (Not String.IsNullOrEmpty(routeName)) Then
            '            If (routeName.Substring(0, 1).Equals("/")) Then
            '                routeName = routeName.Substring(1)
            '            End If
            '            appendLog(cp, "RegisterRoutes, link aliases, routeName=[" & routeName & "], physicalFile=[" & physicalFile & "]")
            '            routes.MapPageRoute(routeName, routeName, physicalFile)
            '        End If
            '        cs.GoNext()
            '    Loop While cs.OK()
            'End If
            'cs.Close()
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
    '====================================================================================================
    ''' <summary>
    ''' register a route to the default page. Include with and without the trailing slash
    ''' </summary>
    ''' <param name="cp"></param>
    ''' <param name="route"></param>
    ''' <param name="routesAdded"></param>
    ''' <param name="routes"></param>
    ''' <param name="physicalFile"></param>
    Public Shared Sub registerRoute(cp As Contensive.BaseClasses.CPBaseClass, route As String, routesAdded As List(Of String), ByVal routes As RouteCollection, physicalFile As String)
        Try
            route = genericController.convertToUnixSlash(route.Trim())
            If (route.Substring(0, 1).Equals("/")) Then
                route = route.Substring(1)
            End If
            If (Not String.IsNullOrEmpty(route)) Then
                If (route.Substring(route.Length - 1, 1).Equals("/")) Then
                    route = route.Substring(0, route.Length - 1)
                End If
                If (Not String.IsNullOrEmpty(route)) Then
                    If (Not routesAdded.Contains(route)) Then
                        appendLog(cp, "RegisterRoute, [" & route & "]")
                        routes.MapPageRoute(route, route, physicalFile)
                        routesAdded.Add(route)
                    End If
                    route &= "/"
                    If (Not routesAdded.Contains(route)) Then
                        routes.MapPageRoute(route, route, physicalFile)
                        routesAdded.Add(route)
                    End If
                End If
            End If
        Catch ex As Exception
            Logger.appendProgramDataLog(ex.ToString())
        End Try
    End Sub
End Class

