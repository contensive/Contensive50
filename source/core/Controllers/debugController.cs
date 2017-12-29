
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.Entity;
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
        public static void testPoint(coreClass cpcore, string Message) {
            //
            double ElapsedTime = 0;
            string iMessage = null;
            //
            if (cpcore.doc.visitPropertyAllowDebugging) {
                //
                // write to stream
                //
                ElapsedTime = Convert.ToSingle(cpcore.doc.appStopWatch.ElapsedMilliseconds) / 1000;
                iMessage = genericController.encodeText(Message);
                iMessage = (ElapsedTime).ToString("00.000") + " - " + iMessage;
                cpcore.doc.testPointMessage = cpcore.doc.testPointMessage + "<nobr>" + iMessage + "</nobr><br>";
                //writeAltBuffer ("<nobr>" & iMessage & "</nobr><br>")
            }
            if (cpcore.siteProperties.allowTestPointLogging) {
                //
                // write to debug log in virtual files - to read from a test verbose viewer
                //
                iMessage = genericController.encodeText(Message);
                iMessage = genericController.vbReplace(iMessage, "\r\n", " ");
                iMessage = genericController.vbReplace(iMessage, "\r", " ");
                iMessage = genericController.vbReplace(iMessage, "\n", " ");
                iMessage = DateTime.Now.ToString("") + "\t" + (ElapsedTime).ToString("00.000") + "\t" + cpcore.doc.authContext.visit.id + "\t" + iMessage;
                //
                logController.appendLog(cpcore, iMessage, "", "testPoints_" + cpcore.serverConfig.appConfig.name);
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
            //INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
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