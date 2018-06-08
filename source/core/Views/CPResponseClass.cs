
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor {
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
        private Contensive.Processor.Controllers.coreController core;
        protected bool disposed = false;
        //
        // Constructor
        //
        public CPResponseClass(Contensive.Processor.Controllers.coreController coreObj) : base() {
            core = coreObj;
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
                    core = null;
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
                return core.webServer.bufferContentType;
            }
            set {
                core.webServer.setResponseContentType(value);
            }
        }

        public override string Cookies {
            get {
                return core.webServer.bufferCookies;
            }
        }

        public override string Header {
            get {
                return core.webServer.bufferResponseHeader;
            }
        }
        //
        //
        //
        public override void Clear() {
            core.webServer.clearResponseBuffer();
        }
        //
        //
        //
        public override void Close()  {
            core.doc.continueProcessing = false;
        }
        //
        public override void AddHeader(string HeaderName, string HeaderValue) {
            core.webServer.addResponseHeader(HeaderName, HeaderValue);
        }
        //
        public override void Flush() {
            core.webServer.flushStream();
        }
        //
        public override void Redirect(string Link) {
            core.webServer.redirect(Link, "", false, false);
        }

        public override void SetBuffer(bool BufferOn) {
            core.html.enableOutputBuffer(BufferOn);
        }
        /// <summary>
        /// /
        /// </summary>
        /// <param name="status"></param>
        public override void SetStatus(string status) {
            core.webServer.setResponseStatus(status);
        }
        //
        public override void SetTimeout(string TimeoutSeconds) {
        }
        //
        public override void SetType(string ContentType) {
            core.webServer.setResponseContentType(ContentType);
        }
        //
        public override void SetCookie(string CookieName, string CookieValue) {
            core.webServer.addResponseCookie(CookieName, CookieValue, DateTime.MinValue, "", "", false);
        }
        //
        public override void SetCookie(string CookieName, string CookieValue, DateTime DateExpires) {
            core.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, "", "", false);
        }
        public override void SetCookie(string CookieName, string CookieValue, DateTime DateExpires, string Domain) {
            core.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, Domain, "", false);
        }
        //
        public override void SetCookie(string CookieName, string CookieValue, DateTime DateExpires, string Domain, string Path) {
            core.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, Domain, Path, false);
        }
        //
        public override void SetCookie(string CookieName, string CookieValue, DateTime DateExpires, string Domain, string Path, bool Secure) {
            core.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, Domain, Path, Secure);
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
                return core.doc.continueProcessing;
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