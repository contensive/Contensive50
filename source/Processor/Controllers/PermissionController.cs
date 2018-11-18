
using System;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class PermissionController : IDisposable {
        //
        //====================================================================================================
        /// <summary>
        /// Get athoring permissions to determine what buttons we display, and what authoring actions we can take
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <param name="AllowInsert"></param>
        /// <param name="AllowCancel"></param>
        /// <param name="allowSave"></param>
        /// <param name="AllowDelete"></param>
        /// <param name="ignore1"></param>
        /// <param name="ignore2"></param>
        /// <param name="ignore3"></param>
        /// <param name="ignore4"></param>
        /// <param name="readOnlyField"></param>
        public static AuthoringPermissions getAuthoringPermissions(CoreController core, string ContentName, int RecordID) {
            var result = new AuthoringPermissions() {
                AllowCancel = false,
                AllowDelete = false,
                AllowInsert = false,
                allowSave = false,
                ignore1 = false,
                ignore2 = false,
                ignore3 = false,
                ignore4 = false,
                readOnlyField = false
            };
            try {
                Models.Domain.CDefModel CDef = null;
                if (RecordID != 0) {
                    WorkflowController.AuthoringStatusClass authoringStatus = core.workflow.getAuthoringStatus(ContentName, RecordID);
                    //
                    // Set Buttons based on Status
                    result.AllowCancel = true;
                    if (authoringStatus.isEditing) {
                        //
                        // Edit Locked
                        result.readOnlyField = true;
                    } else {
                        //
                        // Not editing
                        CDef = Models.Domain.CDefModel.create(core, ContentName);
                        result.allowSave = true;
                        if ((CDef.allowDelete) && (RecordID != 0)) {
                            //
                            // -- allow delete if not new record and cdef allows
                            result.AllowDelete = true;
                        }
                        if (CDef.allowAdd) {
                            //
                            // -- allow delete if cdef allows
                            result.AllowInsert = true;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        public class AuthoringPermissions {
            public bool AllowInsert { get; set; }
            public bool AllowCancel { get; set; }
            public bool allowSave { get; set; }
            public bool AllowDelete { get; set; }
            public bool ignore1 { get; set; }
            public bool ignore2 { get; set; }
            public bool ignore3 { get; set; }
            public bool ignore4 { get; set; }
            public bool readOnlyField { get; set;  }
        }
        //
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
        ~ PermissionController() {
            // do not add code here. Use the Dispose(disposing) overload
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