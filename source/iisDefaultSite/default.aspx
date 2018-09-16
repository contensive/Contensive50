

<script runat="server">

    Sub Page_Load()
        Contensive.Processor.Controllers.logController.forceNLog("Page_Load", Contensive.Processor.Controllers.logController.logLevel.Trace)
        Try
            If (ConfigurationManager.AppSettings("ContensiveUseWebConfig").ToLower = "true") Then
                '
                ' -- initialize with web.config settings (everything in VS project, no contensive server.config needed)
                Dim serverConfig As Contensive.Processor.Models.Domain.serverConfigModel = DefaultSite.configurationClass.getServerConfig()
                Using cp As New Contensive.Processor.CPClass(serverConfig.apps(0).name, serverConfig, HttpContext.Current)
                    Response.Write(cp.executeRoute())
                    If (cp.routeDictionaryChanges) Then DefaultSite.configurationClass.loadRouteMap(cp)
                End Using
            Else
                '
                ' -- initialize with contensive c:\programdata\contensive\serverConfig.json (use same settings as cli and services)
                Using cp As New Contensive.Processor.CPClass(DefaultSite.configurationClass.getAppName(), HttpContext.Current)
                    Response.Write(cp.executeRoute())
                    If (cp.routeDictionaryChanges) Then DefaultSite.configurationClass.loadRouteMap(cp)
                End Using
            End If
        Catch ex As Exception
        Finally
        End Try
    End Sub
</script>