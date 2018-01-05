
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
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
namespace Contensive.Core {
    //
    // comVisible to be activeScript compatible
    //
    //[ComVisible(true), Microsoft.VisualBasic.ComClass(CPResponseClass.ClassId, CPResponseClass.InterfaceId, CPResponseClass.EventsId)]
    public class CPResponseClass : BaseClasses.CPResponseBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "054CD625-A12A-4D21-A581-84EC0D604E65";
        public const string InterfaceId = "130395BA-EF1A-4B1D-B43C-01356127660A";
        public const string EventsId = "C7FCA224-8542-46F2-9019-52A7B5BAE4DB";
        #endregion
        //
        private Contensive.Core.coreClass cpCore;
        protected bool disposed = false;
        //
        // Constructor
        //
        public CPResponseClass(Contensive.Core.coreClass cpCoreObj) : base() {
            cpCore = cpCoreObj;
        }
        //
        // dispose
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                appendDebugLog(".dispose, dereference main, csv");
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    cpCore = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        //
        //
        public override string ContentType {
            get {
                if (true) {
                    return cpCore.webServer.bufferContentType;
                } else {
                    return "";
                }
            }
            set {
                if (true) {
                    cpCore.webServer.setResponseContentType(value);
                }
            }
        }

        public override string Cookies {
            get {
                if (true) {
                    return cpCore.webServer.bufferCookies;
                } else {
                    return "";
                }
            }
        }

        public override string Header //Inherits BaseClasses.CPResponseBaseClass.Header
        {
            get {
                if (true) {
                    return cpCore.webServer.bufferResponseHeader;
                } else {
                    return "";
                }
            }
        }
        //
        //
        //
        public override void Clear() //Inherits BaseClasses.CPResponseBaseClass.Clear
        {
            if (true) {
                cpCore.webServer.clearResponseBuffer();
            }
        }
        //
        //
        //
        public override void Close() //Inherits BaseClasses.CPResponseBaseClass.Close
        {
            cpCore.doc.continueProcessing = false;
        }

        public override void AddHeader(string HeaderName, string HeaderValue) //Inherits BaseClasses.CPResponseBaseClass.AddHeader
        {
            if (true) {
                cpCore.webServer.addResponseHeader(HeaderName, HeaderValue);
            }
        }

        public override void Flush() {
            if (true) {
                cpCore.webServer.flushStream();
            }
        }
        public override void Redirect(string Link) {
            if (true) {
                cpCore.webServer.redirect(Link, "", false, false);
            }
        }

        public override void SetBuffer(bool BufferOn) {
            if (true) {
                cpCore.html.enableOutputBuffer(BufferOn);
            }
        }

        public override void SetStatus(string status) //Inherits BaseClasses.CPResponseBaseClass.SetStatus
        {
            if (true) {
                cpCore.webServer.setResponseStatus(status);
            }
        }

        public override void SetTimeout(string TimeoutSeconds) {
            if (true) {
                //Call cmc.main_SetStreamTimeout(TimeoutSeconds)
            }
        }

        public override void SetType(string ContentType) //Inherits BaseClasses.CPResponseBaseClass.SetType
        {
            cpCore.webServer.setResponseContentType(ContentType);
        }

        public override void SetCookie(string CookieName, string CookieValue) {
            cpCore.webServer.addResponseCookie(CookieName, CookieValue, DateTime.MinValue, "", "", false);
        }
        public override void SetCookie(string CookieName, string CookieValue, DateTime DateExpires) {
            cpCore.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, "", "", false);
        }
        public override void SetCookie(string CookieName, string CookieValue, DateTime DateExpires, string Domain) {
            cpCore.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, Domain, "", false);
        }
        public override void SetCookie(string CookieName, string CookieValue, DateTime DateExpires, string Domain, string Path) {
            cpCore.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, Domain, Path, false);
        }
        public override void SetCookie(string CookieName, string CookieValue, DateTime DateExpires, string Domain, string Path, bool Secure) {
            cpCore.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, Domain, Path, Secure);
        }

        [Obsolete("The write buffer is deprecated")]
        public override void Write(string message) {
            //
        }
        //
        //
        //
        public override bool isOpen {
            get {
                if (true) {
                    return cpCore.doc.continueProcessing;
                } else {
                    return false;
                }
            }
        }
        //
        //
        //
        private void appendDebugLog(string copy) {
            //My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.response, " & copy & vbCrLf, True)
            // 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
        }
        //
        // testpoint
        //
        private void tp(string msg) {
            //Call appendDebugLog(msg)
        }
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPResponseClass() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}