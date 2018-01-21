
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class debugController : IDisposable {
        //
        //
        //========================================================================
        //   Test Point
        //       If main_PageTestPointPrinting print a string, value paior
        //========================================================================
        //
        public static void testPoint(coreController cpcore, string message) {
            //
            if ((cpcore != null) && (cpcore.serverConfig != null) && (cpcore.serverConfig.appConfig != null)) {
                bool debugging = cpcore.doc.visitPropertyAllowDebugging;
                bool logging = cpcore.siteProperties.allowTestPointLogging;
                double ElapsedTime = 0;
                if (logging || debugging) {
                    ElapsedTime = Convert.ToSingle(cpcore.doc.appStopWatch.ElapsedMilliseconds) / 1000;
                }
                if (debugging) {
                    message = (ElapsedTime).ToString("00.000") + " - " + message;
                    cpcore.doc.testPointMessage = cpcore.doc.testPointMessage + "<nobr>" + message + "</nobr><br>";
                }
                if (logging) {
                    message = message.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
                    message = DateTime.Now.ToString("") + "\t" + (ElapsedTime).ToString("00.000") + "\t" + cpcore.doc.sessionContext.visit.id + "\t" + message;
                    logController.appendLog(cpcore, message, "", "testPoints_" + cpcore.serverConfig.appConfig.name);
                }
            }
        }
        //
        //====================================================================================================
        #region  IDisposable Support 
        //
        // this class must implement System.IDisposable
        // never throw an exception in dispose
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        //====================================================================================================
        //
        protected bool disposed = false;
        //
        public void Dispose() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~debugController() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        //
        //====================================================================================================
        /// <summary>
        /// dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    //If (cacheClient IsNot Nothing) Then
                    //    cacheClient.Dispose()
                    //End If
                }
                //
                // cleanup non-managed objects
                //
            }
        }
        #endregion
    }
    //
}