
using System;

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
        //====================================================================================================
        /// <summary>
        /// dependencies
        /// </summary>
        private CPClass cp;
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cp"></param>
        public CPResponseClass(CPClass cp) {
            this.cp = cp;
        }
        //
        //====================================================================================================
        //
        public override string ContentType {
            get {
                return cp.core.webServer.bufferContentType;
            }
            set {
                cp.core.webServer.setResponseContentType(value);
            }
        }
        //
        //====================================================================================================
        //
        public override string Cookies {
            get {
                return cp.core.webServer.bufferCookies;
            }
        }
        //
        //====================================================================================================
        //
        public override string Header {
            get {
                return cp.core.webServer.bufferResponseHeader;
            }
        }
        //
        //====================================================================================================
        //
        public override void Clear() {
            cp.core.webServer.clearResponseBuffer();
        }
        //
        //====================================================================================================
        //
        public override void Close()  {
            cp.core.doc.continueProcessing = false;
        }
        //
        //====================================================================================================
        //
        public override void AddHeader(string name, string value) {
            cp.core.webServer.addResponseHeader(name, value);
        }
        //
        //====================================================================================================
        //
        public override void Flush() {
            cp.core.webServer.flushStream();
        }
        //
        //====================================================================================================
        //
        public override void Redirect(string link) {
            cp.core.webServer.redirect(link, "", false, false);
        }
        //
        //====================================================================================================
        //
        public override void SetBuffer(bool bufferOn) {
            cp.core.html.enableOutputBuffer(bufferOn);
        }
        //
        //====================================================================================================
        //
        /// <summary>
        ///
        /// </summary>
        /// <param name="status"></param>
        public override void SetStatus(string status) {
            cp.core.webServer.setResponseStatus(status);
        }
        //
        //====================================================================================================
        //
        public override void SetTimeout(string timeoutSeconds) {
        }
        //
        //====================================================================================================
        //
        public override void SetType(string contentType) {
            cp.core.webServer.setResponseContentType(contentType);
        }
        //
        //====================================================================================================
        //
        public override void SetCookie(string name, string value) {
            cp.core.webServer.addResponseCookie(name, value, DateTime.MinValue, "", "", false);
        }
        //
        //====================================================================================================
        //
        public override void SetCookie(string name, string value, DateTime dateExpires) {
            cp.core.webServer.addResponseCookie(name, value, dateExpires, "", "", false);
        }
        //
        //====================================================================================================
        //
        public override void SetCookie(string CookieName, string CookieValue, DateTime DateExpires, string Domain) {
            cp.core.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, Domain, "", false);
        }
        //
        //====================================================================================================
        //
        public override void SetCookie(string CookieName, string CookieValue, DateTime DateExpires, string Domain, string Path) {
            cp.core.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, Domain, Path, false);
        }
        //
        //====================================================================================================
        //
        public override bool isOpen {
            get {
                return cp.core.doc.continueProcessing;
            }
        }
        //
        //====================================================================================================
        //
        public override void SetCookie(string CookieName, string CookieValue, DateTime DateExpires, string Domain, string Path, bool Secure) {
            cp.core.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, Domain, Path, Secure);
        }
        //
        //====================================================================================================
        // deprecated
        //
        [Obsolete("The write buffer is deprecated")]
        public override void Write(string message) {}

        #region  IDisposable Support 
        //
        // dispose
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        protected bool disposed = false;
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPResponseClass() {
            Dispose(false);
            
            
        }
        #endregion
    }
}