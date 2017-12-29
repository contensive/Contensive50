
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
//
namespace Contensive.Core.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class errorController : IDisposable {
        //
        // ----- constants
        //
        //Private Const invalidationDaysDefault As Double = 365
        //
        //==========================================================================
        //   Add on to the common error message
        //==========================================================================
        //
        public static void addUserError(coreClass cpCore, string Message) {
            if (!string.IsNullOrEmpty(Message)) {
                if (cpCore.doc.debug_iUserError.IndexOf(Message, System.StringComparison.OrdinalIgnoreCase) == -1) {
                    cpCore.doc.debug_iUserError = cpCore.doc.debug_iUserError + "\r<li class=\"ccError\">" + genericController.encodeText(Message) + "</LI>";
                }
            }
        }
        //
        //==========================================================================
        //   main_Get The user error messages
        //       If there are none, return ""
        //==========================================================================
        //
        public static string getUserError(coreClass cpcore) {
            string temperror_GetUserError = null;
            temperror_GetUserError = genericController.encodeText(cpcore.doc.debug_iUserError);
            if (!string.IsNullOrEmpty(temperror_GetUserError)) {
                temperror_GetUserError = "<ul class=\"ccError\">" + genericController.htmlIndent(temperror_GetUserError) + "\r</ul>";
                temperror_GetUserError = UserErrorHeadline + "" + temperror_GetUserError;
                cpcore.doc.debug_iUserError = "";
            }
            return temperror_GetUserError;
        }

        //
        //==========================================================================================
        /// <summary>
        /// return an html ul list of each eception produced during this document.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string getDocExceptionHtmlList(coreClass cpcore) {
            string returnHtmlList = "";
            try {
                if (cpcore.doc.errList != null) {
                    if (cpcore.doc.errList.Count > 0) {
                        foreach (string exMsg in cpcore.doc.errList) {
                            returnHtmlList += cr2 + "<li class=\"ccExceptionListRow\">" + cr3 + cpcore.html.convertTextToHTML(exMsg) + cr2 + "</li>";
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
        ~errorController() {
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