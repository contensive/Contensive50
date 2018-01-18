
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
using Contensive.Core.Models.DbModels;
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
                return cpCore.webServer.bufferContentType;
            }
            set {
                cpCore.webServer.setResponseContentType(value);
            }
        }

        public override string Cookies {
            get {
                return cpCore.webServer.bufferCookies;
            }
        }

        public override string Header {
            get {
                return cpCore.webServer.bufferResponseHeader;
            }
        }
        //
        //
        //
        public override void Clear() {
            cpCore.webServer.clearResponseBuffer();
        }
        //
        //
        //
        public override void Close()  {
            cpCore.doc.continueProcessing = false;
        }
        //
        public override void AddHeader(string HeaderName, string HeaderValue) {
            cpCore.webServer.addResponseHeader(HeaderName, HeaderValue);
        }
        //
        public override void Flush() {
            cpCore.webServer.flushStream();
        }
        //
        public override void Redirect(string Link) {
            cpCore.webServer.redirect(Link, "", false, false);
        }

        public override void SetBuffer(bool BufferOn) {
            cpCore.html.enableOutputBuffer(BufferOn);
        }
        /// <summary>
        /// /
        /// </summary>
        /// <param name="status"></param>
        public override void SetStatus(string status) {
            cpCore.webServer.setResponseStatus(status);
        }
        //
        public override void SetTimeout(string TimeoutSeconds) {
        }
        //
        public override void SetType(string ContentType) {
            cpCore.webServer.setResponseContentType(ContentType);
        }
        //
        public override void SetCookie(string CookieName, string CookieValue) {
            cpCore.webServer.addResponseCookie(CookieName, CookieValue, DateTime.MinValue, "", "", false);
        }
        //
        public override void SetCookie(string CookieName, string CookieValue, DateTime DateExpires) {
            cpCore.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, "", "", false);
        }
        public override void SetCookie(string CookieName, string CookieValue, DateTime DateExpires, string Domain) {
            cpCore.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, Domain, "", false);
        }
        //
        public override void SetCookie(string CookieName, string CookieValue, DateTime DateExpires, string Domain, string Path) {
            cpCore.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, Domain, Path, false);
        }
        //
        public override void SetCookie(string CookieName, string CookieValue, DateTime DateExpires, string Domain, string Path, bool Secure) {
            cpCore.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, Domain, Path, Secure);
        }
        //
        [Obsolete("The write buffer is deprecated")]
        //
        //
        //
        public override void Write(string message) {}
        //
        //
        //
        public override bool isOpen {
            get {
                return cpCore.doc.continueProcessing;
            }
        }
        //
        //
        //
        private void appendDebugLog(string copy) { }
        //
        // testpoint
        //
        private void tp(string msg) { }
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