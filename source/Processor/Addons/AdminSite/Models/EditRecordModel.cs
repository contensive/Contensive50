
using Contensive.Models.Db;
using Contensive.Processor.Controllers;
using System;
using System.Collections.Generic;

namespace Contensive.Processor.Addons.AdminSite.Models {
    /// <summary>
    /// 
    /// </summary>
    public class EditRecordModel {
        public Dictionary<string, EditRecordFieldModel> fieldsLc = new Dictionary<string, EditRecordFieldModel>();
        /// <summary>
        /// ID field of edit record (Record to be edited)
        /// </summary>
        public int id;
        /// <summary>
        /// ParentID field of edit record (Record to be edited)
        /// </summary>
        public int parentID;
        /// <summary>
        /// name field of edit record
        /// </summary>
        public string nameLc;
        /// <summary>
        /// active field of the edit record
        /// </summary>
        public bool active;
        /// <summary>
        /// ContentControlID of the edit record
        /// </summary>
        public int contentControlId;
        /// <summary>
        /// denormalized name from contentControlId property
        /// </summary>
        public string contentControlId_Name;
        /// <summary>
        /// Used for Content Watch Link Label if default
        /// </summary>
        public string menuHeadline;
        /// <summary>
        /// Used for control section display
        /// </summary>
        public DateTime modifiedDate;
        public PersonModel modifiedBy;
        public DateTime dateAdded;
        public PersonModel createdBy;
        public bool Loaded; // true/false - set true when the field array values are loaded
        public bool Saved; // true if edit record was saved during this page
        public bool userReadOnly; // set if this record can not be edited, for various reasons
        public bool IsDeleted; // true means the edit record has been deleted
        public bool IsInserted; // set if Workflow authoring insert
        public bool IsModified; // record has been modified since last published
        public string LockModifiedName; // member who first edited the record
        public DateTime LockModifiedDate; // Date when member modified record
        public bool SubmitLock; // set if a submit Lock, even if the current user is admin
        public string SubmittedName; // member who submitted the record
        public DateTime SubmittedDate; // Date when record was submitted
        public bool ApproveLock; // set if an approve Lock
        public string ApprovedName; // member who approved the record
        public DateTime ApprovedDate; // Date when record was approved
                                      /// <summary>
                                      /// This user can add records to this content
                                      /// </summary>
        public bool AllowUserAdd;
        /// <summary>
        /// This user can save the current record
        /// </summary>
        public bool AllowUserSave;
        /// <summary>
        /// This user can delete the current record
        /// </summary>
        public bool AllowUserDelete;
        /// <summary>
        /// set if an edit Lock by anyone else besides the current user
        /// </summary>
        public WorkflowController.editLockClass EditLock;
    }
}
