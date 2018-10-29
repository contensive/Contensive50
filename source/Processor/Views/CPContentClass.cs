
using System;
using System.Collections.Generic;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using Contensive.BaseClasses;

namespace Contensive.Processor {
    public class CPContentClass : CPContentBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "D8D3D8F9-8459-46F7-B8AC-01B4DFAA4DB2";
        public const string InterfaceId = "9B321DE5-D154-4EB1-B533-DBA2E5F2B5D2";
        public const string EventsId = "6E068297-E09E-42C8-97B6-02DE591009DD";
        #endregion
        //
        private CPClass cp { get; set; }
        private CoreController core { get; set; }
        protected bool disposed { get; set; } = false;
        //
        //====================================================================================================
        //
        public CPContentClass(CPClass cpParent) : base() {
            cp = cpParent;
            core = cp.core;
        }
        //
        //====================================================================================================
        //
        public override string GetCopy(string CopyName, string DefaultContent = "") {
            return core.html.getContentCopy(CopyName, DefaultContent, core.session.user.id, true, core.session.isAuthenticated);
        }
        //
        //====================================================================================================
        //
        public override string GetCopy(string CopyName, string DefaultContent, int personalizationPeopleId) {
            return core.html.getContentCopy(CopyName, DefaultContent, personalizationPeopleId, true, core.session.isAuthenticated);
        }
        //
        //====================================================================================================
        //
        public override void SetCopy(string CopyName, string Content) {
            core.db.setContentCopy(CopyName, Content);
        }
        //
        //====================================================================================================
        //
        public override string GetAddLink(string ContentName, string PresetNameValueList, bool AllowPaste, bool IsEditing) {
            return AdminUIController.getRecordAddLink2(core, ContentName, PresetNameValueList, AllowPaste, IsEditing);
        }
        //
        //====================================================================================================
        //
        public override string GetContentControlCriteria(string ContentName) {
            return CdefController.getContentControlCriteria(core, ContentName);
        }
        //
        //====================================================================================================
        //
        public override string GetFieldProperty(string ContentName, string FieldName, string PropertyName) {
            return CdefController.getContentFieldProperty(core, ContentName, FieldName, PropertyName);
        }
        //
        //====================================================================================================
        //
        public override int GetID(string ContentName) {
            return CdefController.getContentId(core, ContentName);
        }
        //
        //====================================================================================================
        /// <summary>
        /// will be deprecated. Instead create cdef domain model and reference property
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="PropertyName"></param>
        /// <returns></returns>
        public override string GetProperty(string ContentName, string PropertyName) {
            return CdefController.getContentProperty(cp.core, ContentName, PropertyName);
        }
        //
        //====================================================================================================
        //
        public override string GetDataSource(string ContentName) {
            return CdefController.getContentDataSource(core, ContentName);
        }
        //
        //====================================================================================================
        //
        public override string GetEditLink(string ContentName, string RecordID, bool AllowCut, string RecordName, bool IsEditing) {
            return AdminUIController.getRecordEditLink(core,ContentName, GenericController.encodeInteger(RecordID), AllowCut, RecordName, IsEditing);
        }
        //
        //====================================================================================================
        //
        public override string GetLinkAliasByPageID(int PageID, string QueryStringSuffix, string DefaultLink) {
            return LinkAliasController.getLinkAlias(core, PageID, QueryStringSuffix, DefaultLink);
        }
        //
        //====================================================================================================
        //
        public override string GetPageLink(int PageID, string QueryStringSuffix = "", bool AllowLinkAlias = true) {
            return PageContentController.getPageLink(core, PageID, QueryStringSuffix, AllowLinkAlias, false);
        }
        //
        //====================================================================================================
        //
        public override int GetRecordID(string ContentName, string RecordName) {
            return core.db.getRecordID(ContentName, RecordName);
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the matching record name if a match is found, otherwise blank. Does NOT validate the record.
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <returns></returns>
        public override string GetRecordName(string ContentName, int RecordID) {
            return core.db.getRecordName(ContentName, RecordID);
        }
        //
        //====================================================================================================
        //
        public override string GetTable(string ContentName) {
            return CdefController.getContentTablename(core, ContentName);
        }
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated, template link is not supported", true)]
        public override string GetTemplateLink(int TemplateID) {
            return "";
        }
        //
        //====================================================================================================
        //
        public override bool IsField(string ContentName, string FieldName) {
            return CdefController.isContentFieldSupported(core, ContentName, FieldName);
        }
        //
        //====================================================================================================
        //
        public override bool IsLocked(string ContentName, string RecordID) {
            return core.workflow.isRecordLocked(ContentName, GenericController.encodeInteger(RecordID), 0);
        }
        //
        //====================================================================================================
        //
        public override bool IsChildContent(string ChildContentID, string ParentContentID) {
            return CdefController.isWithinContent(cp.core, GenericController.encodeInteger(ChildContentID), GenericController.encodeInteger(ParentContentID));
        }
        //
        //====================================================================================================
        //
        [Obsolete("workflow editing is deprecated", false)]
        public override bool IsWorkflow(string ContentName) {
            //
            // -- workflow no longer supported (but may come back)
            return false;
        }
        //
        //====================================================================================================
        //
        [Obsolete("workflow editing is deprecated", false)]
        public override void PublishEdit(string ContentName, int RecordID) {
            // Call core.workflow.publishEdit(ContentName, RecordID, 0)
        }
        //
        //====================================================================================================
        //
        [Obsolete("workflow editing is deprecated", false)]
        public override void SubmitEdit(string ContentName, int RecordID) {
            //Call core.workflow.submitEdit2(ContentName, RecordID, 0)
        }
        //
        //====================================================================================================
        //
        [Obsolete("workflow editing is deprecated", false)]
        public override void AbortEdit(string ContentName, int RecordId) {
            // Call core.workflow.abortEdit2(ContentName, RecordId, 0)
        }
        //
        //====================================================================================================
        //
        [Obsolete("workflow editing is deprecated", false)]
        public override void ApproveEdit(string ContentName, int RecordId) {
            //Call core.workflow.approveEdit(ContentName, RecordId, 0)
        }
        //
        //====================================================================================================
        //
        public override string getLayout(string layoutName) {
            string result = "";
            try {
                CsController cs = new CsController(core);
                cs.open("layouts", "name=" + cp.Db.EncodeSQLText(layoutName), "id", false, "layout");
                if (cs.ok()) {
                    result = cs.getText("layout");
                }
                cs.close();
            } catch (Exception ex) {
                LogController.handleError( core,ex); // "Unexpected error in getLayout")
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public override int AddRecord(string ContentName, string recordName) {
            int recordId = 0;
            try {
                CsController cs = new CsController(core);
                if (cs.insert(ContentName)) {
                    cs.setField("name", recordName);
                    recordId = cs.getInteger("id");
                }
                cs.close();
            } catch (Exception ex) {
                LogController.handleError( core,ex); // "Unexpected error in AddRecord")
                throw;
            }
            return recordId;
        }
        //
        //====================================================================================================
        //
        [Obsolete("Please use AddRecord( ContentName as string) ", true)]
        public override int AddRecord(object ContentName) {
            return AddRecord(cp.Utils.EncodeText(ContentName));
        }
        public override int AddRecord(string ContentName) {
            int recordId = 0;
            try {
                CsController cs = new CsController(core);
                if (cs.insert(ContentName)) {
                    recordId = cs.getInteger("id");
                }
                cs.close();
            } catch (Exception ex) {
                LogController.handleError( core,ex); // "Unexpected error in AddRecord")
                throw;
            }
            return recordId;
        }
        //
        //====================================================================================================
        //
        public override void Delete(string ContentName, string SQLCriteria) {
            core.db.deleteContentRecords(ContentName, SQLCriteria);
        }
        //
        //====================================================================================================
        //
        public override void DeleteContent(string ContentName) {
            ContentModel.delete(core, CdefController.getContentId(core, ContentName));
        }
        //
        //====================================================================================================
        //
        public override int AddContentField(string ContentName, string FieldName, int FieldType) {
            Models.Domain.CDefFieldModel field = new Models.Domain.CDefFieldModel();
            field.active = true;
            field.adminOnly = false;
            field.authorable = true;
            field.blockAccess = false;
            field.caption = FieldName;
            field.contentId = CdefController.getContentId(core, ContentName);
            field.developerOnly = false;
            field.editSortPriority = 9999;
            field.editTabName = "";
            field.fieldTypeId = FieldType;
            field.htmlContent = false;
            field.indexColumn = 0;
            field.indexSortDirection = 0;
            field.indexSortOrder = 0;
            field.indexWidth = "";
            field.installedByCollectionGuid = "";
            field.isBaseField = false;
            field.lookupContentID = 0;
            //field.lookupContentName = ""
            field.lookupList = "";
            field.manyToManyContentID = 0;
            field.set_manyToManyContentName(core, "");
            field.manyToManyRuleContentID = 0;
            field.set_manyToManyRuleContentName(core, "");
            field.ManyToManyRulePrimaryField = "";
            field.ManyToManyRuleSecondaryField = "";
            field.memberSelectGroupId_set( core, 0 );
            field.nameLc = FieldName.ToLowerInvariant();
            field.password = false;
            field.readOnly = false;
            field.redirectContentID = 0;
            field.set_redirectContentName(core, "");
            field.redirectID = "";
            field.redirectPath = "";
            field.required = false;
            field.Scramble = false;
            field.textBuffered = false;
            field.uniqueName = false;
            return CdefController.verifyContentField_returnID(core, ContentName, field);
        }
        //
        //====================================================================================================
        //
        public override int AddContent(string ContentName) {
            return AddContent(ContentName, ContentName.Replace(' '.ToString(), "").Replace(' '.ToString(), ""), "default");
        }
        //
        //====================================================================================================
        //
        public override int AddContent(string ContentName, string sqlTableName) {
            return AddContent(ContentName, sqlTableName, "default");
        }
        //
        //====================================================================================================
        //
        public override int AddContent(string ContentName, string sqlTableName, string dataSourceName) {
            var tmpList = new List<string> { };
            DataSourceModel dataSource = DataSourceModel.createByUniqueName(core, dataSourceName, ref tmpList);
            return CdefController.verifyContent_returnId(core, new Models.Domain.CDefModel() {
                dataSourceName = dataSource.name,
                tableName = sqlTableName,
                name = ContentName
            });
        }
        //
        //====================================================================================================
        //
        public override string GetListLink(string ContentName) {
            return AdminUIController.getIconEditAdminLink(core, Models.Domain.CDefModel.create(core, ContentName));
        }
        //
        //====================================================================================================
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    core = null;
                    cp = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        //====================================================================================================
        //
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPContentClass() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}
