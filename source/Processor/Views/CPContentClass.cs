
using System;
using System.Collections.Generic;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using Contensive.BaseClasses;
using Contensive.Addons.AdminSite.Controllers;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Exceptions;
using System.Data;

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
        //
        //====================================================================================================
        //
        public CPContentClass(CPClass cp) : base() {
            this.cp = cp;
        }
        //
        //====================================================================================================
        //
        public override int GetTableID(string tableName) {
            throw new NotImplementedException();
        }
        //
        //====================================================================================================
        //
        public override string GetCopy(string copyName, string DefaultContent) {
            return cp.core.html.getContentCopy(copyName, DefaultContent, cp.core.session.user.id, true, cp.core.session.isAuthenticated);
        }
        //
        public override string GetCopy(string copyName) {
            return cp.core.html.getContentCopy(copyName, "", cp.core.session.user.id, true, cp.core.session.isAuthenticated);
        }
        //
        //====================================================================================================
        //
        public override string GetCopy(string copyName, string defaultContent, int personalizationPeopleId) {
            return cp.core.html.getContentCopy(copyName, defaultContent, personalizationPeopleId, true, cp.core.session.isAuthenticated);
        }
        //
        //====================================================================================================
        //
        public override void SetCopy(string copyName, string content) {
            cp.core.html.setContentCopy(copyName, content);
        }
        //
        //====================================================================================================
        //
        public override string GetAddLink(string contentName, string presetNameValueList, bool allowPaste, bool isEditing) {
            string result = "";
            foreach (var link in AdminUIController.getRecordAddLink(cp.core, contentName, presetNameValueList, allowPaste, isEditing)) {
                result += link;
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public override string GetContentControlCriteria(string contentName) {
            var meta = ContentMetadataModel.createByUniqueName(cp.core, contentName);
            if (meta != null) { return meta.legacyContentControlCriteria; }
            return string.Empty;
        }
        //
        //====================================================================================================
        //
        [Obsolete("deprecated, instead create contentMeta, lookup field and use property of field", false)]
        public override string GetFieldProperty(string contentName, string fieldName, string propertyName) {
            throw new GenericException("getContentFieldProperty deprecated, instead create contentMeta, lookup field and use property of field");
        }
        //
        //====================================================================================================
        //
        public override string GetDataSource(string contentName) {
            var meta = ContentMetadataModel.createByUniqueName(cp.core, contentName);
            if (meta != null) { return meta.dataSourceName; }
            return string.Empty;
        }
        //
        //====================================================================================================
        //
        public override string GetEditLink(string contentName, string recordID, bool allowCut, string recordName, bool isEditing) {
            return AdminUIController.getRecordEditLink(cp.core, contentName, GenericController.encodeInteger(recordID), allowCut, recordName, isEditing);
        }
        //
        //====================================================================================================
        //
        public override string GetLinkAliasByPageID(int pageID, string queryStringSuffix, string defaultLink) {
            return LinkAliasController.getLinkAlias(cp.core, pageID, queryStringSuffix, defaultLink);
        }
        //
        //====================================================================================================
        //
        public override string GetPageLink(int pageID, string queryStringSuffix, bool allowLinkAlias) {
            return PageContentController.getPageLink(cp.core, pageID, queryStringSuffix, allowLinkAlias, false);
        }
        //
        public override string GetPageLink(int pageID, string queryStringSuffix) {
            return PageContentController.getPageLink(cp.core, pageID, queryStringSuffix, true, false);
        }
        //
        public override string GetPageLink(int pageID) {
            return PageContentController.getPageLink(cp.core, pageID, "", true, false);
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a record id from its unique name. If a duplicate exists, the first ordered by id is returned
        /// </summary>
        /// <param name="contentName"></param>
        /// <param name="recordName"></param>
        /// <returns></returns>
        public override int GetRecordID(string contentName, string recordName) {
            return MetadataController.getRecordIdByUniqueName(cp.core, contentName, recordName);
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the matching record name if a match is found, otherwise blank. Does NOT validate the record.
        /// </summary>
        /// <param name="contentName"></param>
        /// <param name="recordID"></param>
        /// <returns></returns>
        public override string GetRecordName(string contentName, int recordID) {
            var meta = ContentMetadataModel.createByUniqueName(cp.core, contentName);
            if (meta == null) { return string.Empty; }
            return meta.getRecordName(cp.core, recordID);
        }
        //
        //====================================================================================================
        //
        public override string GetTable(string contentName) {
            var meta = ContentMetadataModel.createByUniqueName(cp.core, contentName);
            if (meta == null) { return string.Empty; }
            return meta.tableName;
        }
        //
        //====================================================================================================
        //
        public override bool IsField(string contentName, string fieldName) {
            var contentMetadata = Models.Domain.ContentMetadataModel.createByUniqueName(cp.core, contentName);
            return (contentMetadata == null) ? false : contentMetadata.containsField(cp.core, fieldName);
        }
        //
        //====================================================================================================
        //
        public override bool IsLocked(string contentName, string recordId) {
            var contentTable = TableModel.createByContentName(cp.core, contentName);
            if (contentTable != null) return WorkflowController.isRecordLocked(cp.core, contentTable.id, GenericController.encodeInteger(recordId));
            return false;
        }
        //
        //====================================================================================================
        //
        public override bool IsChildContent(string childContentID, string parentContentID) {
            var parentMetadata = ContentMetadataModel.create(cp.core, parentContentID);
            return (parentMetadata == null) ? false : parentMetadata.isParentOf(cp.core, GenericController.encodeInteger(childContentID));
        }
        //
        //====================================================================================================
        //
        public override string getLayout(string layoutName) {
            string result = "";
            try {
                using (var cs = new CsModel(cp.core)) {
                    cs.open("layouts", "name=" + DbController.encodeSQLText(layoutName), "id", false, cp.core.session.user.id, "layout");
                    if (cs.ok()) {
                        result = cs.getText("layout");
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public override int AddRecord(string contentName, string recordName) {
            int recordId = 0;
            try {
                CsModel cs = new CsModel(cp.core);
                if (cs.insert(contentName)) {
                    cs.set("name", recordName);
                    recordId = cs.getInteger("id");
                }
                cs.close();
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
                throw;
            }
            return recordId;
        }
        //
        //====================================================================================================
        //
        public override int AddRecord(string contentName) {
            int result = 0;
            try {
                CsModel cs = new CsModel(cp.core);
                if (cs.insert(contentName)) {
                    result = cs.getInteger("id");
                }
                cs.close();
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public override void Delete(string contentName, string sqlCriteria) {
            MetadataController.deleteContentRecords(cp.core, contentName, sqlCriteria);
        }
        //
        //====================================================================================================
        //
        public override void DeleteContent(string contentName) {
            ContentModel.delete(cp.core, Models.Domain.ContentMetadataModel.getContentId(cp.core, contentName));
        }
        //
        //====================================================================================================
        //
        public override int AddContentField(string contentName, string fieldName, CPContentBaseClass.fileTypeIdEnum fieldType) {
            var contentMetadata = ContentMetadataModel.createByUniqueName(cp.core, contentName);
            var fieldMeta = ContentFieldMetadataModel.createDefault(cp.core, fieldName, fieldType);
            contentMetadata.verifyContentField(cp.core, fieldMeta, false);
            return fieldMeta.id;
        }
        //
        public override int AddContentField(string contentName, string fieldName, int fieldTypeId)
            => AddContentField(contentName, fieldName, (CPContentBaseClass.fileTypeIdEnum)fieldTypeId);
        //
        //====================================================================================================
        //
        public override int AddContent(string contentName) {
            return AddContent(contentName, contentName.Replace(' '.ToString(), "").Replace(' '.ToString(), ""), "default");
        }
        //
        //====================================================================================================
        //
        public override int AddContent(string contentName, string sqlTableName) {
            return AddContent(contentName, sqlTableName, "default");
        }
        //
        //====================================================================================================
        //
        public override int AddContent(string contentName, string sqlTableName, string dataSourceName) {
            var tmpList = new List<string> { };
            DataSourceModel dataSource = DataSourceModel.createByUniqueName(cp.core, dataSourceName, ref tmpList);
            return ContentMetadataModel.verifyContent_returnId(cp.core, new Models.Domain.ContentMetadataModel() {
                dataSourceName = dataSource.name,
                tableName = sqlTableName,
                name = contentName
            });
        }
        //
        //====================================================================================================
        //
        public override string GetListLink(string contentName) {
            return AdminUIController.getIconEditAdminLink(cp.core, Models.Domain.ContentMetadataModel.createByUniqueName(cp.core, contentName));
        }
        //
        //====================================================================================================
        //
        public override int GetID(string ContentName) {
            var content = ContentModel.createByUniqueName(cp.core, ContentName);
            if (content != null) return content.id;
            return 0;

        }
        //
        //====================================================================================================
        // deprecated
        //
        [Obsolete("Use AddRecord( string contentName ) ", false)]
        public override int AddRecord(object ContentName) {
            return AddRecord(cp.Utils.EncodeText(ContentName));
        }
        //
        [Obsolete("workflow editing is deprecated", false)]
        public override bool IsWorkflow(string ContentName) {
            //
            // -- workflow no longer supported (but may come back)
            return false;
        }
        //
        [Obsolete("workflow editing is deprecated", false)]
        public override void PublishEdit(string ContentName, int RecordID) {
            // Call WorkflowController.publishEdit(ContentName, RecordID, 0)
        }
        //
        [Obsolete("workflow editing is deprecated", false)]
        public override void SubmitEdit(string ContentName, int RecordID) {
            //Call WorkflowController.submitEdit2(ContentName, RecordID, 0)
        }
        //
        [Obsolete("workflow editing is deprecated", false)]
        public override void AbortEdit(string ContentName, int RecordId) {
            // Call WorkflowController.abortEdit2(ContentName, RecordId, 0)
        }
        //
        [Obsolete("workflow editing is deprecated", false)]
        public override void ApproveEdit(string ContentName, int RecordId) {
            //Call WorkflowController.approveEdit(ContentName, RecordId, 0)
        }
        //
        [Obsolete("Deprecated, template link is not supported", false)]
        public override string GetTemplateLink(int TemplateID) {
            return "";
        }
        //
        [Obsolete("Deprecated, access model properties instead", false)]
        public override string GetProperty(string ContentName, string PropertyName) {
            var contentMetadata = Models.Domain.ContentMetadataModel.createByUniqueName(cp.core, ContentName);
            return contentMetadata.getContentProperty(cp.core, PropertyName);
        }
        //
        //
        #region  IDisposable Support 
        //
        //====================================================================================================
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
        protected bool disposed { get; set; } = false;
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPContentClass() {
            Dispose(false);
        }
        #endregion
    }
}
