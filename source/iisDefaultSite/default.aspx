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
            '
            ' -- initialize with contensive d:\contensive\serverConfig.json (use same settings as cli and services)
            Dim appName As String = DefaultSite.ConfigurationClass.getAppName()
            Dim context = DefaultSite.ConfigurationClass.buildContext(appName, HttpContext.Current)
            Using cp As New Contensive.Processor.CPClass(appName, context)
                Response.Write(cp.executeRoute())
                If (DefaultSite.ConfigurationClass.routeMapDateInvalid() OrElse (cp.routeMap.dateCreated <> CDate(HttpContext.Current.Application("RouteMapDateCreated")))) Then
                    HttpRuntime.UnloadAppDomain()
                End If
            End Using
        Catch ex As Exception
        Finally
        End Try
    End Sub
</script>
