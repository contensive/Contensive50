
<script runat="server">
    Sub Page_Load()
        Dim test As String = "test"
        Dim cp As Contensive.Core.CPClass
        Dim doc As String
        Dim serverConfig As Contensive.Core.Models.Entity.serverConfigModel = DefaultApp.configurationClass.getServerConfig()
        '
        cp = New Contensive.Core.CPClass(serverConfig, HttpContext.Current)
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
</script>