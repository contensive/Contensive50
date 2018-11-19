
using System;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using System.Collections.Generic;
using System.Linq;
//
namespace Contensive.Processor.Controllers {
    public class WorkflowController : IDisposable {
        /// <summary>
        /// record is being edted
        /// </summary>
        internal const int AuthoringControlsEditing = 1;
        /// <summary>
        /// record workflow deprecated
        /// </summary>
        internal const int AuthoringControlsSubmitted = 2;
        /// <summary>
        /// record workflow deprecated
        /// </summary>
        internal const int AuthoringControlsApproved = 3;
        /// <summary>
        /// record workflow deprecated
        /// </summary>
        internal const int AuthoringControlsModified = 4;
        //
        //==========================================================================================
        /// <summary>
        /// create the content record key
        /// </summary>
        /// <param name="tableId"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public static string getContentRecordKey(int tableId, int recordId) => DbController.encodeSQLText(tableId.ToString() + "/" + recordId.ToString());
        //
        //=================================================================================
        /// <summary>
        /// Get a record lock status. If session.user is the lock holder, returns unlocked
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="recordId"></param>
        /// <param name="ReturnMemberID"></param>
        /// <param name="ReturnDateExpires"></param>
        /// <returns></returns>
        public static editLockClass getEditLock(CoreController core, int tableId, int recordId) {
            try {
                var table = Models.Db.TableModel.create(core, tableId);
                if (table != null) {
                    string criteria = "(createdby<>" + core.session.user.id + ")and(contentRecordKey=" + getContentRecordKey(table.id, recordId) + ")and((DateExpires>" + DbController.encodeSQLDate(DateTime.Now) + ")or(DateExpires Is null))";
                    var authoringControlList = Models.Db.AuthoringControlModel.createList(core, criteria, "dateexpires desc");
                    if (authoringControlList.Count > 0) {
                        var person = Models.Db.PersonModel.create(core, authoringControlList.First().createdBy);
                        return new editLockClass() {
                            isEditLocked = true,
                            editLockExpiresDate = authoringControlList.First().DateExpires,
                            editLockByMemberId = (person == null) ? 0 : person.id,
                            editLockByMemberName = (person == null) ? "" : person.name
                        };                    
                    }
                };
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return new editLockClass() { isEditLocked = false };
        }
        //
        //========================================================================
        /// <summary>
        /// Returns true if the record is locked to the current member
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public static bool isRecordLocked(CoreController core, int tableId, int recordId) => getEditLock(core, tableId, recordId).isEditLocked;
        //
        //========================================================================
        /// <summary>
        /// Edit Lock member name
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <returns></returns>
        public static string getEditLockMemberName(CoreController core, int tableId, int recordId) => getEditLock(core, tableId, recordId).editLockByMemberName;
        //
        //========================================================================
        /// <summary>
        /// Edit Lock dat expires
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <returns></returns>
        public static DateTime getEditLockDateExpires(CoreController core, int tableId, int recordId) => getEditLock(core, tableId, recordId).editLockExpiresDate;
        //
        //========================================================================
        /// <summary>
        /// Clears the edit lock for this record
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        public static void clearEditLock(CoreController core, int tableId, int recordId) {
            string criteria = "(contentRecordKey=" + getContentRecordKey(tableId, recordId) + ")";
            Models.Db.AuthoringControlModel.deleteSelection(core, criteria);
        }
        //
        //========================================================================
        /// <summary>
        /// Sets the edit lock for this record
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        public static void setEditLock(CoreController core, int tableId, int recordId) => setEditLock(core, tableId, recordId, core.session.user.id);
        //
        //=================================================================================
        /// <summary>
        /// Set a record locked
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <param name="userId"></param>
        public static void setEditLock(CoreController core, int tableId, int recordId, int userId) {
            string contentRecordKey = getContentRecordKey(tableId, recordId);
            var editLockList = Models.Db.AuthoringControlModel.createList(core, "(contentRecordKey=" + contentRecordKey + ")");
            var editLock = (editLockList.Count > 0) ? editLockList.First() : Models.Db.AuthoringControlModel.addEmpty(core);
            editLock.contentRecordKey = contentRecordKey;
            editLock.ControlType = AuthoringControlsEditing;
            editLock.createdBy = userId;
            editLock.dateAdded = DateTime.Now;
            editLock.save(core);
        }
        //
        //=====================================================================================================
        /// <summary>
        /// Get the query to test the authoring control table if a record is locked
        /// </summary>
        /// <param name="cdef"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        private static string getAuthoringControlCriteria(CoreController core, Models.Domain.CDefModel cdef, int recordId) {
            string result = "";
            try {
                //
                // Authoring Control records are referenced by ContentID
                var table = Models.Db.TableModel.createByUniqueName(core, cdef.tableName);
                if (table == null) return "(1=0)";
                //
                var contentList = Models.Db.ContentModel.createList(core, "(contenttableid=" + table.id + ")");
                if (contentList.Count < 1) {
                    //
                    // No references to this table
                    result = "(1=0)";
                } else if (contentList.Count == 1) {
                    //
                    // One content record
                    result = "(ContentID=" + contentList.First().id + ")And(RecordID=" + recordId + ")And((DateExpires>" + DbController.encodeSQLDate(DateTime.Now) + ")Or(DateExpires Is null))";
                } else {
                    //
                    // Multiple content records
                    //
                    string contentIdList = "";
                    foreach (var content in contentList) contentIdList += "," + content.id.ToString();
                    result = "(contentid in (" + contentIdList.Substring(1) + "))and(recordid=" + recordId + ")and((dateexpires>" + DbController.encodeSQLDate(DateTime.Now) + ")or(dateexpires Is null))";
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //=====================================================================================================
        /// <summary>
        /// Get the query to test the authoring control table if a record is locked
        /// </summary>
        /// <param name="contentName"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        private static string getAuthoringControlCriteria(CoreController core, string contentName, int recordId) => getAuthoringControlCriteria(core, Models.Domain.CDefModel.create(core, contentName), recordId);
        //
        //=====================================================================================================
        /// <summary>
        /// Clear the Approved Authoring Control
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <param name="AuthoringControl"></param>
        /// <param name="MemberID"></param>
        public static void clearAuthoringControl(CoreController core, string ContentName, int RecordID, int AuthoringControl, int MemberID) {
            try {
                string Criteria = getAuthoringControlCriteria(core, ContentName, RecordID) + "And(ControlType=" + AuthoringControl + ")";
                switch (AuthoringControl) {
                    case AuthoringControlsEditing:
                        core.db.deleteContentRecords("Authoring Controls", Criteria + "And(CreatedBy=" + DbController.encodeSQLNumber(MemberID) + ")", MemberID);
                        break;
                    case AuthoringControlsSubmitted:
                    case AuthoringControlsApproved:
                    case AuthoringControlsModified:
                        core.db.deleteContentRecords("Authoring Controls", Criteria, MemberID);
                        break;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //=====================================================================================================
        /// <summary>
        /// the Approved Authoring Control
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <returns></returns>
        public static recordWorkflowStatusClass getWorkflowStatus(CoreController core, string ContentName, int RecordID) {
            recordWorkflowStatusClass result = new recordWorkflowStatusClass() {
                isEditLocked = false,
                editLockByMemberName = "",
                editLockExpiresDate = DateTime.MinValue,
                workflowSubmittedMemberName = "",
                isWorkflowModified = false,
                workflowModifiedByMemberName = "",
                workflowModifiedDate = DateTime.MinValue,
                isWorkflowSubmitted = false,
                workflowSubmittedDate = DateTime.MinValue,
                isWorkflowApproved = false,
                workflowApprovedMemberName = "",
                workflowApprovedDate = DateTime.MinValue,
                isWorkflowDeleted = false,
                isWorkflowInserted = false
            };
            try {
                if (RecordID > 0) {
                    //
                    // Get Workflow Locks
                    Models.Domain.CDefModel CDef = Models.Domain.CDefModel.create(core, ContentName);
                    if (CDef.id > 0) {
                        var nameDict = new Dictionary<int, string>();
                        foreach (var recordLock in Models.Db.AuthoringControlModel.createList(core, getAuthoringControlCriteria(core, ContentName, RecordID))) {
                            switch(recordLock.ControlType) {
                                case AuthoringControlsEditing:
                                    if (!result.isEditLocked) {
                                        result.isEditLocked = true;
                                        result.editLockExpiresDate = recordLock.dateAdded;
                                        if (nameDict.ContainsKey(recordLock.createdBy)) {
                                            result.editLockByMemberName = nameDict[recordLock.createdBy];
                                        } else {
                                            result.editLockByMemberName = Models.Db.PersonModel.getRecordName(core, recordLock.createdBy);
                                            nameDict.Add(recordLock.createdBy, result.workflowModifiedByMemberName);
                                        }
                                    }
                                    break;
                                case AuthoringControlsModified:
                                    if (!result.isWorkflowModified) {
                                        result.isWorkflowModified = true;
                                        result.workflowSubmittedDate = recordLock.dateAdded;
                                        if (nameDict.ContainsKey(recordLock.createdBy)) {
                                            result.workflowModifiedByMemberName = nameDict[recordLock.createdBy];
                                        } else {
                                            result.workflowModifiedByMemberName = Models.Db.PersonModel.getRecordName(core, recordLock.createdBy);
                                            nameDict.Add(recordLock.createdBy, result.workflowModifiedByMemberName);
                                        }
                                    }
                                    break;
                                case AuthoringControlsSubmitted:
                                    if (!result.isWorkflowSubmitted) {
                                        result.isWorkflowSubmitted = true;
                                        result.workflowModifiedDate = recordLock.dateAdded;
                                        if (nameDict.ContainsKey(recordLock.createdBy)) {
                                            result.workflowSubmittedMemberName = nameDict[recordLock.createdBy];
                                        } else {
                                            result.workflowSubmittedMemberName = Models.Db.PersonModel.getRecordName(core, recordLock.createdBy);
                                            nameDict.Add(recordLock.createdBy, result.workflowSubmittedMemberName);
                                        }
                                    }
                                    break;
                                case AuthoringControlsApproved:
                                    if (!result.isWorkflowApproved) {
                                        result.isWorkflowApproved = true;
                                        result.workflowApprovedDate = recordLock.dateAdded;
                                        if (nameDict.ContainsKey(recordLock.createdBy)) {
                                            result.workflowApprovedMemberName = nameDict[recordLock.createdBy];
                                        } else {
                                            result.workflowApprovedMemberName = Models.Db.PersonModel.getRecordName(core, recordLock.createdBy);
                                            nameDict.Add(recordLock.createdBy, result.workflowSubmittedMemberName);
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        ////
        ////=================================================================================
        ///// <summary>
        ///// Set a record locked
        ///// </summary>
        ///// <param name="ContentName"></param>
        ///// <param name="RecordID"></param>
        ///// <param name="MemberID"></param>
        //public void setEditLock(string ContentName, int RecordID, int MemberID) {
        //    try {
        //        setEditLock(ContentName, RecordID, MemberID, false);
        //    } catch (Exception ex) {
        //        LogController.handleError( core,ex);
        //        throw;
        //    }
        //}
        ////
        ////=================================================================================
        ///// <summary>
        ///// Set a record locked
        ///// </summary>
        ///// <param name="ContentName"></param>
        ///// <param name="RecordID"></param>
        ///// <param name="MemberID"></param>
        //public void clearEditLock(int tableId, int recordId, int userId) {
        //    string criteria = "(contentRecordKey=" + getContentRecordKey(tableId, recordId) + ")and((DateExpires>" + DbController.encodeSQLDate(DateTime.Now) + ")or(DateExpires Is null))";
        //    Models.Db.AuthoringControlModel.deleteSelection(core, criteria);
        //}
        //
        //=================================================================================
        /// <summary>
        /// The workflow and edit locking status of the record.
        /// </summary>
        public class recordWorkflowStatusClass {
            /// <summary>
            /// The record is locked because it is being edited
            /// </summary>
            public bool isEditLocked { get; set; }
            /// <summary>
            /// The record is being editing by this user
            /// </summary>
            public string editLockByMemberName { get; set; }
            /// <summary>
            /// The date and time when the record will be released from editing lock
            /// </summary>
            public DateTime editLockExpiresDate { get; set; }
            /// <summary>
            /// For deprected workflow editing. This user has submitted this record for publication
            /// </summary>
            public string workflowSubmittedMemberName { get; set; }
            /// <summary>
            /// For deprected workflow editing. This user has approved this record for publication
            /// </summary>
            public string workflowApprovedMemberName { get; set; }
            /// <summary>
            /// For deprected workflow editing. This user last saved changes to this record
            /// </summary>
            public string workflowModifiedByMemberName { get; set; }
            /// <summary>
            /// For deprected workflow editing. The record is locked because it has been submitted for publication
            /// </summary>
            public bool isWorkflowSubmitted { get; set; }
            /// <summary>
            /// For deprected workflow editing. The record is locked because it has been approved for publication
            /// </summary>
            public bool isWorkflowApproved { get; set; }
            /// <summary>
            /// For deprected workflow editing. The record has been modified
            /// </summary>
            public bool isWorkflowModified { get; set; }
            /// <summary>
            /// For deprected workflow editing. The date and time when the record was modified
            /// </summary>
            public DateTime workflowModifiedDate { get; set; }
            /// <summary>
            /// For deprected workflow editing. The date and time when the record was submitted for publishing
            /// </summary>
            public DateTime workflowSubmittedDate { get; set; }
            /// <summary>
            /// For deprected workflow editing. The date and time when the record was approved for publishing
            /// </summary>
            public DateTime workflowApprovedDate { get; set; }
            /// <summary>
            /// For deprected workflow editing.  The record has been inserted but not published
            /// </summary>
            public bool isWorkflowInserted { get; set; }
            /// <summary>
            /// For deprected workflow editing. The record has been deleted but not published
            /// </summary>
            public bool isWorkflowDeleted { get; set; }
        }

        //
        //=================================================================================
        /// <summary>
        /// The workflow and edit locking status of the record.
        /// </summary>
        public class editLockClass {
            /// <summary>
            /// The record is locked because it is being edited
            /// </summary>
            public bool isEditLocked { get; set; }
            /// <summary>
            /// The record is being editing by this user
            /// </summary>
            public int editLockByMemberId { get; set; }
            /// <summary>
            /// The record is being editing by this user
            /// </summary>
            public string editLockByMemberName { get; set; }
            /// <summary>
            /// The date and time when the record will be released from editing lock
            /// </summary>
            public DateTime editLockExpiresDate { get; set; }
        }
        //
        //==========================================================================================
        #region  IDisposable Support 
        protected bool disposed = false;
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // ----- call .dispose for managed objects
                    //
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~WorkflowController() {
            Dispose(false);
            
            
        }
        #endregion
    }
}

