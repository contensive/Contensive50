

<script runat="server">

    Sub Page_Load()
        Contensive.Core.Controllers.logController.forceNLog("Page_Load", Contensive.Core.Controllers.logController.logLevel.Trace)
        '
        ' -- two possible initialization methods 
        Dim sw As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()
        Dim useWebConfig As Boolean = (ConfigurationManager.AppSettings("ContensiveUseWebConfig").ToLower = "true")
        If useWebConfig Then
            '
            ' -- initialize with web.config
            Dim serverConfig As Contensive.Core.Models.Context.serverConfigModel = DefaultSite.configurationClass.getServerConfig()
            Using cp As New Contensive.Core.CPClass(serverConfig.apps(0).name, serverConfig, HttpContext.Current)
                Response.Write(cp.executeRoute())
                ' use application_start -- DefaultSite.configurationClass.loadRouteMap(cp)
            End Using
        Else
            '
            ' -- initialize with contensive c:\programdata\contensive\serverConfig.json setup during installation
            Using cp As New Contensive.Core.CPClass(DefaultSite.configurationClass.getAppName(), HttpContext.Current)
                Response.Write(cp.executeRoute())
                ' use application_start -- DefaultSite.configurationClass.loadRouteMap(cp)
            End Using
        End If
    End Sub
</script>