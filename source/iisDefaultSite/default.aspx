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
                '
                For Each header As Contensive.Processor.Models.Domain.HttpContextResponseHeader In context.Response.headers
                    Response.Headers.Add(header.name, header.value)
                Next
                '
                For Each cookie As KeyValuePair(Of String, Contensive.Processor.Models.Domain.HttpContextResponseCookie) In context.Response.cookies
                    Dim ck As New HttpCookie(cookie.Key, cookie.Value.value)
                    ck.Domain = cookie.Value.domain
                    ck.Expires = cookie.Value.expires
                    ck.HttpOnly = cookie.Value.httpOnly
                    ck.Name = cookie.Key
                    ck.Path = cookie.Value.path
                    ck.SameSite = cookie.Value.sameSite
                    ck.Secure = cookie.Value.secure
                    Response.AppendCookie(ck)
                Next
                '
                Response.ContentType = context.Response.contentType
                Response.CacheControl = context.Response.cacheControl
                Response.Status = context.Response.status
                Response.Expires = context.Response.expires
                Response.Buffer = context.Response.buffer
                '
                Response.Write(content)
                If (ConfigurationClass.routeMapDateInvalid() OrElse (cp.routeMap.dateCreated <> CDate(HttpContext.Current.Application("RouteMapDateCreated")))) Then
                    HttpRuntime.UnloadAppDomain()
                End If
            End Using
        Catch ex As Exception
        Finally
        End Try
    End Sub
</script>
