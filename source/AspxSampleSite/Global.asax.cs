using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Text;
using Contensive.Processor.Controllers;

namespace AspxSampleSite {
    public class Global_asax : System.Web.HttpApplication {
        // 
        public Guid AppId = Guid.NewGuid();
        // 
        // ====================================================================================================
        /// <summary>
        ///     ''' application load -- build routing
        ///     ''' </summary>
        ///     ''' <param name="sender"></param>
        ///     ''' <param name="e"></param>
        public void Application_Start(object sender, EventArgs e) {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///     ''' Fires when the session is started
        ///     ''' </summary>
        ///     ''' <param name="sender"></param>
        ///     ''' <param name="e"></param>
        public void Session_Start(object sender, EventArgs e) {
            LogController.logRaw("Global.asax, Session_Start [" + e.ToString() + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Trace);
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///     ''' Fires at the beginning of each request
        ///     ''' </summary>
        ///     ''' <param name="sender"></param>
        ///     ''' <param name="e"></param>
        public void Application_BeginRequest(object sender, EventArgs e) {
            LogController.logRaw("Global.asax, Application_BeginRequest [" + e.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Trace);
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///     ''' Fires when iis attempts to authenticate the use
        ///     ''' </summary>
        ///     ''' <param name="sender"></param>
        ///     ''' <param name="e"></param>
        public void Application_AuthenticateRequest(object sender, EventArgs e) {
            LogController.logRaw("Global.asax, Application_AuthenticateRequest [" + e.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Trace);
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///     ''' Fires when an error occurs
        ///     ''' </summary>
        ///     ''' <param name="sender"></param>
        ///     ''' <param name="e"></param>
        public void Application_Error(object sender, EventArgs e) {
            LogController.logRaw("Global.asax, Application_Error, Server.GetLastError().InnerException [" + System.Web.HttpApplication.Server.GetLastError().InnerException.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Error);
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///     ''' Fires when the session ends
        ///     ''' </summary>
        ///     ''' <param name="sender"></param>
        ///     ''' <param name="e"></param>
        public void Session_End(object sender, EventArgs e) {
            LogController.logRaw("Global.asax, Session_End [" + e.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Trace);
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///     ''' Fires when the application ends
        ///     ''' </summary>
        ///     ''' <param name="sender"></param>
        ///     ''' <param name="e"></param>
        public void Application_End(object sender, EventArgs e) {
            LogController.logRaw("Global.asax, Application_End [" + e.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Trace);
        }
        // 
        // ====================================================================================================
        private string getAppDescription(string eventName) {
            StringBuilder builder = new StringBuilder();
            // 
            builder.AppendFormat("Event: {0}", eventName);
            builder.AppendFormat(", Guid: {0}", AppId);
            builder.AppendFormat(", Thread Id: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
            builder.AppendFormat(", Appdomain: {0}", AppDomain.CurrentDomain.FriendlyName);
            builder.Append(Interaction.IIf(System.Threading.Thread.CurrentThread.IsThreadPoolThread, ", Pool Thread", ", No Thread").ToString());
            return builder.ToString();
        }
    }

}



