
using System;
using System.Collections.Generic;

namespace Contensive.Addons.AdminSite {
    public static class Constants {
        //
        public  const int ToolsActionMenuMove = 1;
        public  const int ToolsActionAddField = 2; // Add a field to the Index page
        public  const int ToolsActionRemoveField = 3;
        public  const int ToolsActionMoveFieldRight = 4;
        public  const int ToolsActionMoveFieldLeft = 5;
        public  const int ToolsActionSetAZ = 6;
        public  const int ToolsActionSetZA = 7;
        public  const int ToolsActionExpand = 8;
        public  const int ToolsActionContract = 9;
        public  const int ToolsActionEditMove = 10;
        public  const int ToolsActionRunQuery = 11;
        public  const int ToolsActionDuplicateDataSource = 12;
        public  const int ToolsActionDefineContentFieldFromTableFieldsFromTable = 13;
        public  const int ToolsActionFindAndReplace = 14;
        //
        public  const string AddonGuidPreferences = "{D9C2D64E-9004-4DBE-806F-60635B9F52C8}";

        public const int AdminActionNop = 0; // do nothing
        public const int AdminActionDelete = 4; // delete record
        public const int AdminActionFind = 5;
        public const int AdminActionDeleteFilex = 6;
        public const int AdminActionUpload = 7;
        public const int AdminActionSaveNormal = 3; // save fields to database
        public const int AdminActionSaveEmail = 8; // save email record (and update EmailGroups) to database
        public const int AdminActionSaveMember = 11;
        public const int AdminActionSaveSystem = 12;
        public const int AdminActionSavePaths = 13; // Save a record that is in the BathBlocking Format
        public const int AdminActionSendEmail = 9;
        public const int AdminActionSendEmailTest = 10;
        public const int AdminActionNext = 14;
        public const int AdminActionPrevious = 15;
        public const int AdminActionFirst = 16;
        public const int AdminActionSaveContent = 17;
        public const int AdminActionSaveField = 18; // Save a single field, fieldname = fn input
        public const int AdminActionPublish = 19; // Publish record live
        public const int AdminActionAbortEdit = 20; // Publish record live
        public const int AdminActionPublishSubmit = 21; // Submit for Workflow Publishing
        public const int AdminActionPublishApprove = 22; // Approve for Workflow Publishing
                                                         //Public Const AdminActionWorkflowPublishApproved = 23    ' Publish what was approved
        public const int AdminActionSetHTMLEdit = 24; // Set Member Property for this field to HTML Edit
        public const int AdminActionSetTextEdit = 25; // Set Member Property for this field to Text Edit
        public const int AdminActionSave = 26; // Save Record
        public const int AdminActionActivateEmail = 27; // Activate a Conditional Email
        public const int AdminActionDeactivateEmail = 28; // Deactivate a conditional email
        public const int AdminActionDuplicate = 29; // Duplicate the (sent email) record
        public const int AdminActionDeleteRows = 30; // Delete from rows of records, row0 is boolean, rowid0 is ID, rowcnt is count
        public const int AdminActionSaveAddNew = 31; // Save Record and add a new record
        public const int AdminActionReloadCDef = 32; // Load Content Definitions
                                                     // Public Const AdminActionWorkflowPublishSelected = 33 ' Publish what was selected
        public const int AdminActionMarkReviewed = 34; // Mark the record reviewed without making any changes
        public const int AdminActionEditRefresh = 35; // reload the page just like a save, but do not save
        /// <summary>
        /// 
        /// </summary>
        public const bool allowSaveBeforeDuplicate = false;
        /// <summary>
        /// 
        /// </summary>
        public const int OrderByFieldPointerDefault = -1;
        /// <summary>
        /// 
        /// </summary>
        public const int RecordsPerPageDefault = 50;


    }
}
