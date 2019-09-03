<script runat="server">
    Public void Page_Load(Object sender, System.EventArgs e) {
        //
            Contensive.Processor.Controllers.LogController.logRaw("Page_Load", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Trace);
            try {
                if ((ConfigurationManager.AppSettings["ContensiveUseWebConfig"].ToLower == "true")) {
                    // 
                    // -- initialize with web.config settings (everything in VS project, no contensive server.config needed)
                    Contensive.Processor.Models.Domain.serverConfigModel serverConfig = DefaultSite.configurationClass.getServerConfig();
                    using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(serverConfig.apps(0).name, serverConfig, HttpContext.Current)) {
                        Response.Write(cp.executeRoute());
                        DefaultSite.ConfigurationClass.loadRouteMap(cp);
                    }
                } else
                    // 
                    // -- initialize with contensive c:\programdata\contensive\serverConfig.json (use same settings as cli and services)
                    using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(DefaultSite.configurationClass.getAppName(), HttpContext.Current)) {
                        Response.Write(cp.executeRoute());
                        DefaultSite.ConfigurationClass.loadRouteMap(cp);
                    }
            } catch (Exception ex) {
            } finally {
            }
        }
</script>

