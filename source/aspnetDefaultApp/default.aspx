

<script runat="server">
    Sub Page_Load()
        '
        ' -- two possible initialization methods 
        Dim useWebConfig As Boolean = (ConfigurationManager.AppSettings("ContensiveUseWebConfig").ToLower = "true")
        If useWebConfig Then
            '
            ' -- initialize with appSettings in web.config
            Using cp As New Contensive.Core.CPClass(DefaultSite.configurationClass.getServerConfig(), HttpContext.Current)
                Response.Write(cp.executeRoute())
            End Using
        Else
            '
            ' -- initialize with contensive c:\programdata\contensive\serverConfig.json setup during installation
            Using cp As New Contensive.Core.CPClass(ConfigurationManager.AppSettings("ContensiveAppName"), HttpContext.Current)
                Response.Write(cp.executeRoute())
            End Using
        End If
    End Sub
</script>