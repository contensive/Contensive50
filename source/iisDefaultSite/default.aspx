

<script runat="server">

    Sub Page_Load()
        '
        ' -- two possible initialization methods 
        Dim sw As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()
        Dim useWebConfig As Boolean = (ConfigurationManager.AppSettings("ContensiveUseWebConfig").ToLower = "true")
        If useWebConfig Then
            '
            ' -- initialize with appSettings in web.config
            Using cp As New Contensive.Core.CPClass(DefaultSite.configurationClass.getServerConfig(), HttpContext.Current)
                Response.Write(cp.executeRoute())
                DefaultSite.configurationClass.loadRouteMap(cp)
            End Using
        Else
            '
            ' -- initialize with contensive c:\programdata\contensive\serverConfig.json setup during installation
            Using cp As New Contensive.Core.CPClass(DefaultSite.configurationClass.getAppName(), HttpContext.Current)
                Response.Write(cp.executeRoute())
                DefaultSite.configurationClass.loadRouteMap(cp)
            End Using
        End If
    End Sub
</script>