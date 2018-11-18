
using System;
using static Contensive.Processor.Constants;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class ErrorController : IDisposable {
        //
        //====================================================================================================
        //
        public static void addUserError(CoreController core, string Message) {
            if (!string.IsNullOrEmpty(Message)) {
                if (core.doc.debug_iUserError.IndexOf(Message, System.StringComparison.OrdinalIgnoreCase) == -1) {
                    core.doc.debug_iUserError = core.doc.debug_iUserError + "\r<li class=\"ccExceptionListRow\">" + GenericController.encodeText(Message) + "</LI>";
                }
            }
        }
        //
        //====================================================================================================
        //
        public static string getUserError(CoreController core) {
            string result = "";
            if (!string.IsNullOrEmpty(core.doc.debug_iUserError)) {
                result = UserErrorHeadline + "<ul class=\"ccExceptionList\">" + core.doc.debug_iUserError + "\r</ul>";
                core.doc.debug_iUserError = "";
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return an html ul list of each eception produced during this document.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string getDocExceptionHtmlList(CoreController core) {
            string returnHtmlList = "";
            try {
                if (core.doc.errList != null) {
                    if (core.doc.errList.Count > 0) {
                        foreach (string exMsg in core.doc.errList) {
                            returnHtmlList += cr2 + "<li class=\"ccExceptionListRow\">" + cr3 + core.html.convertTextToHtml(exMsg) + cr2 + "</li>";
                        }
                        returnHtmlList = "\r<ul class=\"ccExceptionList\">" + returnHtmlList + "\r</ul>";
                    }
                }
            } catch (Exception ex) {
                throw (ex);
            }
            return returnHtmlList;
        }
        //
        //====================================================================================================
        //
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
        ~ErrorController() {
            Dispose(false);
            
            
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