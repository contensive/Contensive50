

<script runat="server">

    Sub Page_Load()
        Contensive.Processor.Controllers.LogController.logLocalOnly("Page_Load", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Trace)
        Try
            If (HttpContext.Current.Request.HttpMethod = "OPTIONS") Then
                '
                ' -- preflight options call, just return ok status
                HttpContext.Current.Response.End()
                Exit Sub
            End If
            If (ConfigurationManager.AppSettings("ContensiveUseWebConfig").ToLower = "true") Then
                '
                ' -- initialize with web.config settings
                Dim serverConfig As Contensive.Processor.Models.Domain.ServerConfigModel = DefaultSite.ConfigurationClass.getServerConfig()
                Using cp As New Contensive.Processor.CPClass(serverConfig.apps(0).name, serverConfig, HttpContext.Current)
                    Response.Write(cp.executeRoute())
                    If (DefaultSite.ConfigurationClass.routeMapDateInvalid() OrElse (cp.routeMap.dateCreated <> CDate(HttpContext.Current.Application("RouteMapDateCreated")))) Then
                        HttpRuntime.UnloadAppDomain()
                    End If
                End Using
            Else
                '
                ' -- initialize with contensive c:\programdata\contensive\serverConfig.json (use same settings as cli and services)
                Using cp As New Contensive.Processor.CPClass(DefaultSite.ConfigurationClass.getAppName(), HttpContext.Current)
                    Response.Write(cp.executeRoute())
                    If (DefaultSite.ConfigurationClass.routeMapDateInvalid() OrElse (cp.routeMap.dateCreated <> CDate(HttpContext.Current.Application("RouteMapDateCreated")))) Then
                        HttpRuntime.UnloadAppDomain()
                    End If
                End Using
            End If
        Catch ex As Exception
        Finally
        End Try
    End Sub
</script>