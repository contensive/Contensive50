
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
//
namespace Contensive.Processor.Controllers {
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
        public static void addUserError(coreController core, string Message) {
            if (!string.IsNullOrEmpty(Message)) {
                if (core.doc.debug_iUserError.IndexOf(Message, System.StringComparison.OrdinalIgnoreCase) == -1) {
                    core.doc.debug_iUserError = core.doc.debug_iUserError + "\r<li class=\"ccError\">" + genericController.encodeText(Message) + "</LI>";
                }
            }
        }
        //
        //==========================================================================
        //   main_Get The user error messages
        //       If there are none, return ""
        //==========================================================================
        //
        public static string getUserError(coreController core) {
            string temperror_GetUserError = null;
            temperror_GetUserError = genericController.encodeText(core.doc.debug_iUserError);
            if (!string.IsNullOrEmpty(temperror_GetUserError)) {
                temperror_GetUserError = "<ul class=\"ccError\">" + genericController.nop(temperror_GetUserError) + "\r</ul>";
                temperror_GetUserError = UserErrorHeadline + "" + temperror_GetUserError;
                core.doc.debug_iUserError = "";
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
        public static string getDocExceptionHtmlList(coreController core) {
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