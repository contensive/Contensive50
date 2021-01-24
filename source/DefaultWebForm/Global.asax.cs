using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Contensive;
using Contensive.Processor.Controllers;
using System.Web;

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
        try {
            // 
            LogController.logLocalOnly("Global.asax, Application_Start [" + DefaultSite.ConfigurationClass.getAppName() + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Info);
            // 
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(DefaultSite.ConfigurationClass.getAppName())) {
                DefaultSite.ConfigurationClass.loadRouteMap(cp);
            }
        } catch (Exception ex) {
            LogController.logLocalOnly("Global.asax, Application_Start exception [" + DefaultSite.ConfigurationClass.getAppName() + "]" + getAppDescription("Application_Start ERROR exit") + ", ex [" + ex.ToString() + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Fatal);
        }
    }
    // 
    // ====================================================================================================
    /// <summary>
    ///     ''' Fires when the session is started
    ///     ''' </summary>
    ///     ''' <param name="sender"></param>
    ///     ''' <param name="e"></param>
    public void Session_Start(object sender, EventArgs e) {
        // 
        LogController.logLocalOnly("Global.asax, Session_Start [" + e.ToString() + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Info);
    }
    // 
    // ====================================================================================================
    /// <summary>
    ///     ''' Fires at the beginning of each request
    ///     ''' </summary>
    ///     ''' <param name="sender"></param>
    ///     ''' <param name="e"></param>
    public void Application_BeginRequest(object sender, EventArgs e) {
        // 
        LogController.logLocalOnly("Global.asax, Application_BeginRequest [" + e.ToString() + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Info);
    }
    // 
    // ====================================================================================================
    /// <summary>
    ///     ''' Fires when iis attempts to authenticate the use
    ///     ''' </summary>
    ///     ''' <param name="sender"></param>
    ///     ''' <param name="e"></param>
    public void Application_AuthenticateRequest(object sender, EventArgs e) {
        // 
        LogController.logLocalOnly("Global.asax, Application_AuthenticateRequest [" + e.ToString() + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Info);
    }
    // 
    // ====================================================================================================
    /// <summary>
    ///     ''' Fires when an error occurs
    ///     ''' </summary>
    ///     ''' <param name="sender"></param>
    ///     ''' <param name="e"></param>
    public void Application_Error(object sender, EventArgs e) {
        if ((sender != null)) {
            Exception exception = Server.GetLastError();
            if ((exception != null)) {
                // 
                // -- dont log [The file '...' does not exist.]
                string exMsg = exception.Message;
                if ((exMsg.Substring(0, 10).Equals("The file '") & exMsg.Substring(exMsg.Length - 17, 17).Equals("' does not exist.")))
                    // 
                    // -- File does not exist, thrown for every bot searching content
                    return;
                LogController.logLocalOnly("Global.asax, Application_Error, exception message [" + exception.Message + "], toString [" + exception.ToString() + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Error);
                Exception innerException = exception.InnerException;
                if ((innerException != null))
                    LogController.logLocalOnly("Global.asax, Application_Error, inner exception message [" + innerException.Message + "], toString [" + innerException.ToString() + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Error);
            }
        }
    }
    // 
    // ====================================================================================================
    /// <summary>
    ///     ''' Fires when the session ends
    ///     ''' </summary>
    ///     ''' <param name="sender"></param>
    ///     ''' <param name="e"></param>
    public void Session_End(object sender, EventArgs e) {
        // 
        LogController.logLocalOnly("Global.asax, Session_End [" + e.ToString() + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Info);
    }
    // 
    // ====================================================================================================
    /// <summary>
    ///     ''' Fires when the application ends
    ///     ''' </summary>
    ///     ''' <param name="sender"></param>
    ///     ''' <param name="e"></param>
    public void Application_End(object sender, EventArgs e) {
        // 
        LogController.logLocalOnly("Global.asax, Application_End [" + e.ToString() + "," + getShutdownDetail() + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Info);
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
    // 
    private string getShutdownDetail() {
        System.Web.ApplicationShutdownReason shutdownReason = System.Web.Hosting.HostingEnvironment.ShutdownReason;
        string shutdownDetail = "";

        switch (shutdownReason) {
            case ApplicationShutdownReason.BinDirChangeOrDirectoryRename: {
                    shutdownDetail = "A change was made to the bin directory or the directory was renamed";
                    break;
                }

            case ApplicationShutdownReason.BrowsersDirChangeOrDirectoryRename: {
                    shutdownDetail = "A change was made to the App_browsers folder or the files contained in it";
                    break;
                }

            case ApplicationShutdownReason.ChangeInGlobalAsax: {
                    shutdownDetail = "A change was made in the global.asax file";
                    break;
                }

            case ApplicationShutdownReason.ChangeInSecurityPolicyFile: {
                    shutdownDetail = "A change was made in the code access security policy file";
                    break;
                }

            case ApplicationShutdownReason.CodeDirChangeOrDirectoryRename: {
                    shutdownDetail = "A change was made in the App_Code folder or the files contained in it";
                    break;
                }

            case ApplicationShutdownReason.ConfigurationChange: {
                    shutdownDetail = "A change was made to the application level configuration";
                    break;
                }

            case ApplicationShutdownReason.HostingEnvironment: {
                    shutdownDetail = "The hosting environment shut down the application";
                    break;
                }

            case ApplicationShutdownReason.HttpRuntimeClose: {
                    shutdownDetail = "A call to Close() was requested";
                    break;
                }

            case ApplicationShutdownReason.IdleTimeout: {
                    shutdownDetail = "The idle time limit was reached";
                    break;
                }

            case ApplicationShutdownReason.InitializationError: {
                    shutdownDetail = "An error in the initialization of the AppDomain";
                    break;
                }

            case ApplicationShutdownReason.MaxRecompilationsReached: {
                    shutdownDetail = "The maximum number of dynamic recompiles of a resource limit was reached";
                    break;
                }

            case ApplicationShutdownReason.PhysicalApplicationPathChanged: {
                    shutdownDetail = "A change was made to the physical path to the application";
                    break;
                }

            case ApplicationShutdownReason.ResourcesDirChangeOrDirectoryRename: {
                    shutdownDetail = "A change was made to the App_GlobalResources foldr or the files contained within it";
                    break;
                }

            case ApplicationShutdownReason.UnloadAppDomainCalled: {
                    shutdownDetail = "A call to UnloadAppDomain() was completed";
                    break;
                }

            default: {
                    shutdownDetail = "Unknown shutdown reason";
                    break;
                }
        }
        return shutdownDetail;
    }
}
