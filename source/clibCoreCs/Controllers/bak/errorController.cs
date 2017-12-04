

using System.Text.RegularExpressions;
// 

namespace Controllers {
    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' static class controller
    // '' </summary>
    public class errorController : IDisposable {
        
        // 
        //  ----- constants
        // 
        // Private Const invalidationDaysDefault As Double = 365
        // 
        // ==========================================================================
        //    Add on to the common error message
        // ==========================================================================
        // 
        public static void error_AddUserError(coreClass cpCore, string Message) {
            if (((cpCore.doc.debug_iUserError.IndexOf(Message, 0, System.StringComparison.OrdinalIgnoreCase) + 1) 
                        == 0)) {
                cpCore.doc.debug_iUserError = (cpCore.doc.debug_iUserError 
                            + (cr + ("<li class=\"ccError\">" 
                            + (genericController.encodeText(Message) + "</LI>"))));
            }
            
        }
        
        // 
        // ==========================================================================
        //    main_Get The user error messages
        //        If there are none, return ""
        // ==========================================================================
        // 
        public static string error_GetUserError(coreClass cpcore) {
            error_GetUserError = genericController.encodeText(cpCore.doc.debug_iUserError);
            if ((error_GetUserError != "")) {
                error_GetUserError = ("<ul class=\"ccError\">" 
                            + (genericController.htmlIndent(error_GetUserError) 
                            + (cr + "</ul>")));
                error_GetUserError = (UserErrorHeadline + ("" + error_GetUserError));
                cpCore.doc.debug_iUserError = "";
            }
            
        }
        
        // 
        // ==========================================================================================
        // '' <summary>
        // '' return an html ul list of each eception produced during this document.
        // '' </summary>
        // '' <returns></returns>
        // '' <remarks></remarks>
        public static string getDocExceptionHtmlList(coreClass cpcore) {
            string returnHtmlList = "";
            try {
                if (!(cpcore.doc.errList == null)) {
                    if ((cpcore.doc.errList.Count > 0)) {
                        foreach (string exMsg in cpcore.doc.errList) {
                            (cr2 + ("<li class=\"ccExceptionListRow\">" 
                                        + (cr3 
                                        + (cpcore.html.convertTextToHTML(exMsg) 
                                        + (cr2 + "</li>")))));
                        }
                        
                        returnHtmlList = (cr + ("<ul class=\"ccExceptionList\">" 
                                    + (returnHtmlList 
                                    + (cr + "</ul>"))));
                    }
                    
                }
                
            }
            catch (Exception ex) {
                throw ex;
            }
            
            return returnHtmlList;
        }
        
        // 
        //  this class must implement System.IDisposable
        //  never throw an exception in dispose
        //  Do not change or add Overridable to these methods.
        //  Put cleanup code in Dispose(ByVal disposing As Boolean).
        // ====================================================================================================
        // 
        protected bool disposed = false;
        
        public void Dispose() {
            //  do not add code here. Use the Dispose(disposing) overload
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        // 
        protected override void Finalize() {
            //  do not add code here. Use the Dispose(disposing) overload
            this.Dispose(false);
            base.Finalize();
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' dispose.
        // '' </summary>
        // '' <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    // If (cacheClient IsNot Nothing) Then
                    //     cacheClient.Dispose()
                    // End If
                }
                
                // 
                //  cleanup non-managed objects
                // 
            }
            
        }
    }
}