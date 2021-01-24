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
            Dim appName As String = ConfigurationClass.getAppName()
            Dim context = ConfigurationClass.buildContext(appName, HttpContext.Current)
            Using cp As New Contensive.Processor.CPClass(appName, context)
                Dim content As String = cp.executeRoute()

                If (Not String.IsNullOrEmpty(context.Response.redirectUrl)) Then
                    Response.Redirect(context.Response.redirectUrl)
                    Exit Sub
                End If
                For Each header As Contensive.Processor.Models.Domain.HttpContextResponseHeader In context.Response.headers
                    Dim responseHeader As New NameValueCollection( ,)
                    responseHeader

                    Response.Headers.Add(New NameValueCollection() {{""}, {}})

                Next
                Response.Write(cp.executeRoute())
                If (ConfigurationClass.routeMapDateInvalid() OrElse (cp.routeMap.dateCreated <> CDate(HttpContext.Current.Application("RouteMapDateCreated")))) Then
                    HttpRuntime.UnloadAppDomain()
                End If
            End Using
        Catch ex As Exception
        Finally
        End Try
    End Sub
</script>
