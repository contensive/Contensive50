<script runat="server">
    Sub Page_Load()
        Dim cp As Contensive.Core.CPClass
        Dim doc As String
        '
        cp = New Contensive.Core.CPClass(getServerConfig(), HttpContext.Current)
        doc = cp.executeRoute()
        If cp.Response.isOpen() Then
            '
            ' page is open, modify it
            '
            doc = Replace(doc, "$myCustomTag$", "<div>cp.user.name = " & cp.User.Name & "</div>")
        End If
        cp.Dispose()
        Response.Write(doc)
    End Sub
    '
    Private Function getServerConfig() As Contensive.Core.Models.Entity.serverConfigModel
        Dim serverConfig As Contensive.Core.Models.Entity.serverConfigModel
        '
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
        serverConfig.appConfig.enableCache = ConfigurationManager.AppSettings("ContensiveEnableCache")
        serverConfig.appConfig.enabled = True
        serverConfig.appConfig.name = ConfigurationManager.AppSettings("ContensiveAppName")
        serverConfig.appConfig.privateFilesPath = ConfigurationManager.AppSettings("ContensivePrivateFilesPath")
        serverConfig.appConfig.privateKey = ConfigurationManager.AppSettings("ContensivePrivateKey")
        serverConfig.apps.Add(serverConfig.appConfig.name, serverConfig.appConfig)
        serverConfig.cdnFilesRemoteEndpoint = ConfigurationManager.AppSettings("ContensiveCdnFilesRemoteEndpoint")
        serverConfig.defaultDataSourceAddress = ConfigurationManager.AppSettings("ContensiveDefaultDataSourceAddress")
        serverConfig.defaultDataSourcePassword = ConfigurationManager.AppSettings("ContensiveDefaultDataSourcePassword")
        serverConfig.defaultDataSourceType = ConfigurationManager.AppSettings("ContensiveDefaultDataSourceType")
        serverConfig.defaultDataSourceUsername = ConfigurationManager.AppSettings("ContensiveDefaultDataSourceUsername")
        serverConfig.isLocalCache = ConfigurationManager.AppSettings("ContensiveIsLocalCache")
        serverConfig.isLocalFileSystem = ConfigurationManager.AppSettings("ContensiveIsLocalFileSystem")
        serverConfig.localDataDriveLetter = ConfigurationManager.AppSettings("ContensiveLocalDataDriveLetter")
        serverConfig.name = ConfigurationManager.AppSettings("ContensiveServerGroupName")
        serverConfig.password = ConfigurationManager.AppSettings("ContensiveServerGroupPassword")
        serverConfig.programFilesPath = ""
        serverConfig.username = ConfigurationManager.AppSettings("ContensiveServerGroupUsername")
        'Dim webConfig As System.Configuration.Configuration
        'webConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(Nothing)
        'If (webConfig.AppSettings.Settings.Count > 0) Then
        '    serverConfig.allowTaskRunnerService = False
        '    serverConfig.allowTaskSchedulerService = False
        '    serverConfig.appConfig.adminRoute = webConfig.AppSettings.Settings("ContensiveAdminRoute").Value
        '    serverConfig.appConfig.appRootFilesPath = webConfig.AppSettings.Settings("ContensiveAppRootFilesPath").Value
        '    serverConfig.appConfig.cdnFilesNetprefix = webConfig.AppSettings.Settings("ContensiveCdnFilesNetprefix").Value
        '    serverConfig.appConfig.cdnFilesPath = webConfig.AppSettings.Settings("ContensiveCdnFilesPath").Value
        '    serverConfig.appConfig.domainList.Add(webConfig.AppSettings.Settings("ContensivePrimaryDomain").Value)
        '    serverConfig.appConfig.enableCache = webConfig.AppSettings.Settings("ContensiveEnableCache").Value
        '    serverConfig.appConfig.enabled = True
        '    serverConfig.appConfig.name = webConfig.AppSettings.Settings("ContensiveAppName").Value
        '    serverConfig.appConfig.privateFilesPath = webConfig.AppSettings.Settings("ContensivePrivateFilesPath").Value
        '    serverConfig.appConfig.privateKey = webConfig.AppSettings.Settings("ContensivePrivateKey").Value
        '    serverConfig.apps.Add(serverConfig.appConfig.name, serverConfig.appConfig)
        '    serverConfig.cdnFilesRemoteEndpoint = webConfig.AppSettings.Settings("ContensiveCdnFilesRemoteEndpoint").Value
        '    serverConfig.defaultDataSourceAddress = webConfig.AppSettings.Settings("ContensiveDefaultDataSourceAddress").Value
        '    serverConfig.defaultDataSourcePassword = webConfig.AppSettings.Settings("ContensiveDefaultDataSourcePassword").Value
        '    serverConfig.defaultDataSourceType = webConfig.AppSettings.Settings("ContensiveDefaultDataSourceType").Value
        '    serverConfig.defaultDataSourceUsername = webConfig.AppSettings.Settings("ContensiveDefaultDataSourceUsername").Value
        '    serverConfig.isLocalCache = webConfig.AppSettings.Settings("ContensiveIsLocalCache").Value
        '    serverConfig.isLocalFileSystem = webConfig.AppSettings.Settings("ContensiveIsLocalFileSystem").Value
        '    serverConfig.localDataDriveLetter = webConfig.AppSettings.Settings("ContensiveLocalDataDriveLetter").Value
        '    serverConfig.name = webConfig.AppSettings.Settings("ContensiveServerGroupName").Value
        '    serverConfig.password = webConfig.AppSettings.Settings("ContensiveServerGroupPassword").Value
        '    serverConfig.programFilesPath = ""
        '    serverConfig.username = webConfig.AppSettings.Settings("ContensiveServerGroupUsername").Value
        'End If
        Return serverConfig
    End Function
</script>