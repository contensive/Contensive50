<%@ Page Title="Home Page" Language="C#" %>

<script runat="server">
    void Page_Load()
        {
            Contensive.Processor.Controllers.LogController.logLocalOnly("Page_Load", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Trace);
            try
            {
                if ((HttpContext.Current.Request.HttpMethod == "OPTIONS"))
                {
                    // 
                    // -- preflight options call, just return ok status
                    HttpContext.Current.Response.End();
                    return;
                }
                // 
                // -- initialize with contensive d:\contensive\serverConfig.json (use same settings as cli and services)
                string appName = DefaultSite.ConfigurationClass.getAppName();
                var context = DefaultSite.ConfigurationClass.buildContext(appName, HttpContext.Current);
                using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(appName, context))
                {
                    Response.Write(cp.executeRoute());
                    if ((DefaultSite.ConfigurationClass.routeMapDateInvalid() || (cp.routeMap.dateCreated != (DateTime)HttpContext.Current.Application["RouteMapDateCreated"])))
                        HttpRuntime.UnloadAppDomain();
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
            }
    }

</script>
