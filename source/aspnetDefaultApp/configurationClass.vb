
Option Explicit On
Option Strict On

Imports Contensive.Core
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports System.Web.Routing
Imports System.IO

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
    ''' Site settings come from the contensive json configuration file in c:\ProgramData\Contensive, unless overridden by aspx app settings
    ''' </summary>
    ''' <returns></returns>
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
    '====================================================================================================
    ''' <summary>
    ''' Register Contensive routes with IIS
    ''' </summary>
    ''' <param name="cp"></param>
    ''' <param name="routes"></param>
    Public Shared Sub RegisterRoutes(cp As Contensive.Core.CPClass, ByVal routes As RouteCollection)
        Try
            Dim routesAdded As New List(Of String)
            For Each route As Contensive.BaseClasses.CPSiteBaseClass.routeClass In cp.Site.getRouteList()
                Dim virtualRoute As String = route.virtualRoute
                If (virtualRoute.Substring(0, 1).Equals("/")) Then
                    virtualRoute = virtualRoute.Substring(1)
                End If
                If (Not String.IsNullOrEmpty(virtualRoute)) Then
                    If (virtualRoute.Substring(virtualRoute.Length - 1, 1).Equals("/")) Then
                        virtualRoute = virtualRoute.Substring(0, virtualRoute.Length - 1)
                    End If
                    If (Not String.IsNullOrEmpty(virtualRoute)) Then
                        Try
                            If (Not routesAdded.Contains(virtualRoute)) Then
                                routes.MapPageRoute(virtualRoute, virtualRoute, route.physicalRoute)
                                routesAdded.Add(virtualRoute)
                            End If
                            virtualRoute &= "/"
                            If (Not routesAdded.Contains(virtualRoute)) Then
                                routes.MapPageRoute(virtualRoute, virtualRoute, route.physicalRoute)
                                routesAdded.Add(virtualRoute)
                            End If
                        Catch ex As Exception
                            cp.Site.ErrorReport(ex, "Unexpected exception adding virtualRoute [" & virtualRoute & "], physicalRoute [" & route.physicalRoute & "]")
                        End Try
                    End If
                End If
            Next
        Catch ex As Exception
            Logger.appendProgramDataLog(ex.ToString())
        End Try
    End Sub
End Class

