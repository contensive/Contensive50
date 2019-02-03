
using System;
using System.Collections.Generic;
using Contensive.Processor.Models.Db;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using System.Data;
using System.Linq;
using Contensive.Processor.Exceptions;
using Contensive.BaseClasses;
//
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// Static methods that support the metadata domain model.
    /// </summary>
    public class MetadataController {

        //       
        //===========================================================================
        /// <summary>
        /// returns a comma delimited list of ContentIDs that the Member can author
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static List<int> getEditableMetaDataIdList(CoreController core) {
            try {
                string SQL = "Select ccGroupRules.ContentID as ID"
                + " FROM ((ccmembersrules"
                + " Left Join ccGroupRules on ccMemberRules.GroupID=ccGroupRules.GroupID)"
                + " Left Join ccContent on ccGroupRules.ContentID=ccContent.ID)"
                + " WHERE"
                    + " (ccMemberRules.MemberID=" + core.session.user.id + ")"
                    + " AND(ccGroupRules.Active<>0)"
                    + " AND(ccContent.Active<>0)"
                    + " AND(ccMemberRules.Active<>0)";
                DataTable cidDataTable = core.db.executeQuery(SQL);
                int CIDCount = cidDataTable.Rows.Count;
                List<int> returnList = new List<int>();
                for (int CIDPointer = 0; CIDPointer < CIDCount; CIDPointer++) {
                    int ContentID = encodeInteger(cidDataTable.Rows[CIDPointer][0]);
                    returnList.Add(ContentID);
                    var metaData = ContentMetadataModel.create(core, ContentID);
                    if (metaData != null) {
                        returnList.AddRange(metaData.childIdList(core));
                    }
                }
                return returnList;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //=============================================================================
        /// <summary>
        /// Create a child content from a parent content
        /// </summary>
        /// <param name="core"></param>
        /// <param name="childContentName"></param>
        /// <param name="parentContentName"></param>
        /// <param name="memberID"></param>
        public static void createContentChild(CoreController core, string childContentName, string parentContentName, int memberID) {
            try {
                //
                // Get ContentID of parent
                var content = ContentModel.createByUniqueName(core, parentContentName);
                if (content == null) { throw (new GenericException("Can not create Child Content [" + childContentName + "] because the Parent Content [" + parentContentName + "] was not found.")); }
                //
                // -- test if the child already exists
                var childContent = ContentModel.createByUniqueName(core, childContentName);
                if (childContent != null) {
                    if (childContent.parentID != content.id) { throw (new GenericException("Can not create Child Content [" + childContentName + "] because this content name is already in use.")); }
                    //
                    // -- this child already exists, mark createkey so upgrade will not delete it
                    childContent.createKey = 0;
                    childContent.save(core);
                    return;
                }
                //
                // -- convert this object to a child of the record it was opened with, and save
                content.parentID = content.id;
                content.name = childContentName;
                content.createdBy = memberID;
                content.modifiedBy = memberID;
                content.dateAdded = content.modifiedDate = DateTime.Now;
                content.ccguid = createGuid();
                content.id = 0;
                content.save(core);
                //
                // ----- Load metadata
                //
                core.cache.invalidateAll();
                core.clearMetaData();
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Get a Contents Tablename from the ContentPointer
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public static string getContentTablename(CoreController core, string contentName) {
            try {
                var meta = ContentMetadataModel.createByUniqueName(core, contentName);
                if (meta != null) { return meta.tableName; }
                return string.Empty;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Get a DataSource Name from its ContentName
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public static string getContentDataSource(CoreController core, string contentName) {
            try {
                var meta = ContentMetadataModel.createByUniqueName(core, contentName);
                if (meta != null) { return meta.dataSourceName; }
                return string.Empty;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Get a Contents Name from the ContentID
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentID"></param>
        /// <returns></returns>
        public static string getContentNameByID(CoreController core, int contentID) {
            try {
                var meta = ContentMetadataModel.create(core, contentID);
                if (meta != null) { return meta.name; }
                return string.Empty;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Get a tables first ContentID from Tablename
        /// </summary>
        /// <param name="core"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static int getContentIdByTablename(CoreController core, string tableName) {
            if (string.IsNullOrWhiteSpace(tableName)) { return 0; }
            using (var dt = core.db.executeQuery("select top 1 ContentControlID from " + tableName + " where (contentcontrolid is not null) order by contentcontrolid;")) {
                if (dt != null) { return DbController.getDataRowFieldInteger(dt.Rows[0], "contentcontrolid"); }
            }

            return 0;
        }
        //
        //========================================================================
        /// <summary>
        /// return the sql criteria required to return only records included in the content specified
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public static string getContentControlCriteria(CoreController core, string contentName) {
            var meta = ContentMetadataModel.createByUniqueName(core, contentName);
            if (meta == null) { return ""; }
            return meta.legacyContentControlCriteria;
        }
        //   
        //============================================================================================================
        /// <summary>
        /// set the content control Id for a record, all potentially all its child records (if parentid field exists)
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentId"></param>
        /// <param name="recordId"></param>
        /// <param name="newContentControlID"></param>
        /// <param name="UsedIDString"></param>
        public static void setContentControlId(CoreController core, ContentMetadataModel contentMeta, int recordId, int newContentControlID, string UsedIDString = "") {
            if (contentMeta == null) { return; }
            //
            // -- update the record
            core.db.executeNonQuery("update " + contentMeta.tableName + " set contentcontrolid=" + newContentControlID + " where id=" + recordId);
            //
            // -- fix content watch
            core.db.executeQuery("update ccContentWatch set ContentID=" + newContentControlID + ", ContentRecordKey='" + newContentControlID + "." + recordId + "' where ContentID=" + contentMeta.id + " and RecordID=" + recordId);
            //
            // -- if content includes a parentId field (like page content), update all child records to this meta.id
            if (contentMeta.fields.ContainsKey("parentid")) {
                using (var dt = core.db.executeQuery("select id from " + contentMeta.tableName + " where parentid=" + recordId)) {
                    foreach (DataRow dr in dt.Rows) {
                        setContentControlId(core, contentMeta, DbController.getDataRowFieldInteger(dr, "id"), newContentControlID, UsedIDString);
                    }
                }
            }
        }
        //
        [Obsolete("deprecated, instead create contentMeta, lookup field and use property of field", true)]
        public static string getContentFieldProperty(CoreController core, string ContentName, string FieldName, string PropertyName) {
            throw new GenericException("getContentFieldProperty deprecated, instead create contentMeta, lookup field and use property of field");
            //string result = "";
            //try {
            //    ContentMetaDomainModel Contentdefinition = MetaModel.createByUniqueName(core, ContentName);
            //    if ((string.IsNullOrEmpty(FieldName)) || (Contentdefinition.fields.Count < 1)) {
            //        throw (new GenericException("Content Name [" + GenericController.encodeText(ContentName) + "] or FieldName [" + FieldName + "] was not valid"));
            //    } else {
            //        foreach (KeyValuePair<string, Models.Domain.metadataFieldModel> keyValuePair in Contentdefinition.fields) {
            //            Models.Domain.metadataFieldModel field = keyValuePair.Value;
            //            if (FieldName.ToLowerInvariant() == field.nameLc) {
            //                switch (PropertyName.ToUpper()) {
            //                    case "FIELDTYPE":
            //                    case "TYPE":
            //                        result = field.fieldTypeId.ToString();
            //                        break;
            //                    case "HTMLCONTENT":
            //                        result = field.htmlContent.ToString();
            //                        break;
            //                    case "ADMINONLY":
            //                        result = field.adminOnly.ToString();
            //                        break;
            //                    case "AUTHORABLE":
            //                        result = field.authorable.ToString();
            //                        break;
            //                    case "CAPTION":
            //                        result = field.caption;
            //                        break;
            //                    case "REQUIRED":
            //                        result = field.required.ToString();
            //                        break;
            //                    case "UNIQUENAME":
            //                        result = field.uniqueName.ToString();
            //                        break;
            //                    case "UNIQUE":
            //                        //
            //                        // fix for the uniquename screwup - it is not unique name, it is unique value
            //                        //
            //                        result = field.uniqueName.ToString();
            //                        break;
            //                    case "DEFAULT":
            //                        result = GenericController.encodeText(field.defaultValue);
            //                        break;
            //                    case "MEMBERSELECTGROUPID":
            //                        result = field.memberSelectGroupId_get(core).ToString();
            //                        break;
            //                    default:
            //                        break;
            //                }
            //                break;
            //            }
            //        }
            //    }
            //} catch (Exception ex) {
            //    LogController.handleError(core, ex);
            //}
            //return result;
        }
        //
        // todo change contentname to contentMeta object
        //=============================================================
        /// <summary>
        /// Return a record name given the record id. If not record is found, blank is returned.
        /// </summary>
        public static string getRecordName(CoreController core, string ContentName, int recordID) {
            try {
                var meta = ContentMetadataModel.createByUniqueName(core, ContentName);
                if (meta == null) { return string.Empty; }
                using (DataTable dt = core.db.executeQuery("select name from " + meta.tableName + " where id=" + recordID)) {
                    foreach (DataRow dr in dt.Rows) {
                        return DbController.getDataRowFieldText(dr, "name");
                    }
                }
                return string.Empty;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //=============================================================
        /// <summary>
        /// Return a record name given the guid. If not record is found, blank is returned.
        /// </summary>
        public static string getRecordName(CoreController core, string contentName, string recordGuid) {
            try {
                var meta = ContentMetadataModel.createByUniqueName(core, contentName);
                if (meta == null) { return string.Empty; }
                using (DataTable dt = core.db.executeQuery("select top 1 name from " + meta.tableName + " where ccguid=" + DbController.encodeSQLText(recordGuid) + " order by id")) {
                    foreach (DataRow dr in dt.Rows) {
                        return DbController.getDataRowFieldText(dr, "name");
                    }
                }
                return string.Empty;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //=============================================================
        /// <summary>
        /// Legacy method to get a records id from either the guid or name
        /// </summary>
        [Obsolete("Use the methods specific to each field type", false)]
        public static int getRecordId_Legacy(CoreController core, string contentName, string recordGuidOrName) {
            if (isGuid(recordGuidOrName)) { return getRecordId(core, contentName, recordGuidOrName); }
            return getRecordIdByUniqueName(core, contentName, recordGuidOrName);
        }
        //
        //=============================================================
        /// <summary>
        /// get a record's id from its guid
        /// </summary>
        /// <param name="contentName"></param>
        /// <param name="recordGuid"></param>
        /// <returns></returns>
        //=============================================================
        //
        public static int getRecordId(CoreController core, string contentName, string recordGuid) {
            if (string.IsNullOrWhiteSpace(recordGuid)) { return 0; }
            var meta = ContentMetadataModel.createByUniqueName(core, contentName);
            if ((meta == null) || (string.IsNullOrWhiteSpace(meta.tableName))) { return 0; }
            using (DataTable dt = core.db.executeQuery("select top 1 id from " + meta.tableName + " where ccguid=" + DbController.encodeSQLText(recordGuid) + " order by id")) {
                foreach (DataRow dr in dt.Rows) {
                    return DbController.getDataRowFieldInteger(dr, "id");
                }
            }
            return 0;
        }
        //
        //=============================================================
        /// <summary>
        /// get the lowest recordId based on its name. If no record is found, 0 is returned
        /// </summary>
        /// <param name="contentName"></param>
        /// <param name="recordName"></param>
        /// <returns></returns>
        public static int getRecordIdByUniqueName(CoreController core, string contentName, string recordName) {
            try {
                if (String.IsNullOrWhiteSpace(recordName)) { return 0; }
                var meta = ContentMetadataModel.createByUniqueName(core, contentName);
                if ((meta == null) || (String.IsNullOrWhiteSpace(meta.tableName))) { return 0; }
                using (DataTable dt = core.db.executeQuery("select top 1 id from " + meta.tableName + " where name=" + DbController.encodeSQLText(recordName) + " order by id")) {
                    foreach (DataRow dr in dt.Rows) {
                        return DbController.getDataRowFieldInteger(dr, "id");
                    }
                }
                return 0;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        // todo rename metadata as meta
        //========================================================================
        /// <summary>
        /// returns true if the metadata field exists
        /// </summary>
        /// <param name="ContentID"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public static bool isMetadataField(CoreController core, int ContentID, string FieldName) {
            var meta = ContentMetadataModel.create(core, ContentID);
            if (meta == null) { return false; }
            return meta.fields.ContainsKey(FieldName.Trim().ToLower());
        }
        ////
        ////========================================================================
        ///// <summary>
        ///// InsertContentRecordGetID
        ///// Inserts a record based on a content definition.
        ///// Returns the ID of the record, -1 if error
        ///// </summary>
        ///// <param name="contentName"></param>
        ///// <param name="userId"></param>
        ///// <returns></returns>
        /////
        //public static int insertContentRecordGetID(CoreController core, string contentName, int userId) {
        //    var meta = ContentMetadataModel.createByUniqueName(core, contentName);
        //    if (meta == null) { return 0; }
        //    using (var db = new DbController(core, meta.dataSourceName)) {
        //        return core.db.insertTableRecordGetId(meta.tableName, userId);
        //    }
        //}
        //
        //========================================================================
        /// <summary>
        /// Delete Content Record
        /// </summary>
        /// <param name="contentName"></param>
        /// <param name="recordId"></param>
        /// <param name="userId"></param>
        //
        public static void deleteContentRecord(CoreController core, string contentName, int recordId, int userId = SystemMemberID) {
            var meta = ContentMetadataModel.createByUniqueName(core, contentName);
            if (meta == null) { return; }
            using (var db = new DbController(core, meta.dataSourceName)) {
                core.db.deleteTableRecord(recordId, meta.tableName);
            }
        }
        //
        //========================================================================
        /// <summary>
        /// 'deleteContentRecords
        /// </summary>
        /// <param name="contentName"></param>
        /// <param name="sqlCriteria"></param>
        /// <param name="userId"></param>
        //
        public static void deleteContentRecords(CoreController core, string contentName, string sqlCriteria, int userId = 0) {
            var meta = ContentMetadataModel.createByUniqueName(core, contentName);
            if (meta == null) { return; }
            using (var db = new DbController(core, meta.dataSourceName)) {
                core.db.deleteTableRecords(meta.tableName, sqlCriteria);
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Encode a value for a sql
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static string encodeSQL(object expression, CPContentBaseClass.fileTypeIdEnum fieldType = CPContentBaseClass.fileTypeIdEnum.Text) {
            try {
                switch (fieldType) {
                    case CPContentBaseClass.fileTypeIdEnum.Boolean:
                        return DbController.encodeSQLBoolean(GenericController.encodeBoolean(expression));
                    case CPContentBaseClass.fileTypeIdEnum.Currency:
                    case CPContentBaseClass.fileTypeIdEnum.Float:
                        return DbController.encodeSQLNumber(GenericController.encodeNumber(expression));
                    case CPContentBaseClass.fileTypeIdEnum.AutoIdIncrement:
                    case CPContentBaseClass.fileTypeIdEnum.Integer:
                    case CPContentBaseClass.fileTypeIdEnum.Lookup:
                    case CPContentBaseClass.fileTypeIdEnum.MemberSelect:
                        return DbController.encodeSQLNumber(GenericController.encodeInteger(expression));
                    case CPContentBaseClass.fileTypeIdEnum.Date:
                        return DbController.encodeSQLDate(GenericController.encodeDate(expression));
                    case CPContentBaseClass.fileTypeIdEnum.LongText:
                    case CPContentBaseClass.fileTypeIdEnum.HTML:
                        return DbController.encodeSQLText(GenericController.encodeText(expression));
                    case CPContentBaseClass.fileTypeIdEnum.File:
                    case CPContentBaseClass.fileTypeIdEnum.FileImage:
                    case CPContentBaseClass.fileTypeIdEnum.Link:
                    case CPContentBaseClass.fileTypeIdEnum.ResourceLink:
                    case CPContentBaseClass.fileTypeIdEnum.Redirect:
                    case CPContentBaseClass.fileTypeIdEnum.ManyToMany:
                    case CPContentBaseClass.fileTypeIdEnum.Text:
                    case CPContentBaseClass.fileTypeIdEnum.FileText:
                    case CPContentBaseClass.fileTypeIdEnum.FileJavascript:
                    case CPContentBaseClass.fileTypeIdEnum.FileXML:
                    case CPContentBaseClass.fileTypeIdEnum.FileCSS:
                    case CPContentBaseClass.fileTypeIdEnum.FileHTML:
                        return DbController.encodeSQLText(GenericController.encodeText(expression));
                    default:
                        throw new GenericException("Unknown Field Type [" + fieldType + "");
                }
            } catch (Exception) {
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Create a filename for the Virtual Directory
        /// </summary>
        /// <param name="contentName"></param>
        /// <param name="fieldName"></param>
        /// <param name="recordId"></param>
        /// <param name="originalFilename"></param>
        /// <returns></returns>
        public static string getVirtualFilename(CoreController core, string contentName, string fieldName, int recordId, string originalFilename = "") {
            try {
                if (string.IsNullOrEmpty(contentName.Trim())) { throw new ArgumentException("contentname cannot be blank"); }
                if (string.IsNullOrEmpty(fieldName.Trim())) { throw new ArgumentException("fieldname cannot be blank"); }
                if (recordId <= 0) { throw new ArgumentException("recordid is not valid"); }
                var meta = ContentMetadataModel.createByUniqueName(core, contentName);
                if (meta == null) { throw new ArgumentException("contentName is not valid"); }
                string workingFieldName = fieldName.Trim().ToLowerInvariant();
                if (!meta.fields.ContainsKey(workingFieldName)) { throw new ArgumentException("content metadata does not include field [" + fieldName + "]"); }
                if (string.IsNullOrEmpty(originalFilename)) { return FileController.getVirtualRecordUnixPathFilename(meta.tableName, fieldName, recordId, meta.fields[fieldName.ToLowerInvariant()].fieldTypeId); }
                return FileController.getVirtualRecordUnixPathFilename(meta.tableName, fieldName, recordId, originalFilename);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// get the id of the table record for a content
        /// </summary>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public static int getContentTableID(CoreController core, string contentName) {
            var meta = ContentMetadataModel.createByUniqueName(core, contentName);
            if (meta == null) { return 0; }
            var table = TableModel.createByUniqueName(core, meta.tableName);
            if (table != null) { return table.id; }
            return 0;
        }
        //
        //==================================================================================================
        /// <summary>
        /// Remove this record from all watch lists
        /// </summary>
        /// <param name="ContentID"></param>
        /// <param name="RecordID"></param>
        //
        public static void deleteContentRules(CoreController core, ContentMetadataModel meta, int RecordID) {
            try {
                if (meta == null) { return; }
                if (RecordID <= 0) { throw new GenericException("RecordID [" + RecordID + "] where blank"); }
                string ContentRecordKey = meta.id.ToString() + "." + RecordID.ToString();
                //
                // ----- Table Specific rules
                switch (meta.tableName.ToUpperInvariant()) {
                    case "CCCONTENT":
                        //
                        deleteContentRecords(core, "Group Rules", "ContentID=" + RecordID);
                        break;
                    case "CCCONTENTWATCH":
                        //
                        deleteContentRecords(core, "Content Watch List Rules", "Contentwatchid=" + RecordID);
                        break;
                    case "CCCONTENTWATCHLISTS":
                        //
                        deleteContentRecords(core, "Content Watch List Rules", "Contentwatchlistid=" + RecordID);
                        break;
                    case "CCGROUPS":
                        //
                        deleteContentRecords(core, "Group Rules", "GroupID=" + RecordID);
                        deleteContentRecords(core, "Library Folder Rules", "GroupID=" + RecordID);
                        deleteContentRecords(core, "Member Rules", "GroupID=" + RecordID);
                        deleteContentRecords(core, "Page Content Block Rules", "GroupID=" + RecordID);
                        break;
                    case "CCLIBRARYFOLDERS":
                        //
                        deleteContentRecords(core, "Library Folder Rules", "FolderID=" + RecordID);
                        break;
                    case "CCMEMBERS":
                        //
                        deleteContentRecords(core, "Member Rules", "MemberID=" + RecordID);
                        deleteContentRecords(core, "Topic Habits", "MemberID=" + RecordID);
                        deleteContentRecords(core, "Member Topic Rules", "MemberID=" + RecordID);
                        break;
                    case "CCPAGECONTENT":
                        //
                        deleteContentRecords(core, "Page Content Block Rules", "RecordID=" + RecordID);
                        deleteContentRecords(core, "Page Content Topic Rules", "PageID=" + RecordID);
                        break;
                    case "CCSURVEYQUESTIONS":
                        //
                        deleteContentRecords(core, "Survey Results", "QuestionID=" + RecordID);
                        break;
                    case "CCSURVEYS":
                        //
                        deleteContentRecords(core, "Survey Questions", "SurveyID=" + RecordID);
                        break;
                    case "CCTOPICS":
                        //
                        deleteContentRecords(core, "Topic Habits", "TopicID=" + RecordID);
                        deleteContentRecords(core, "Page Content Topic Rules", "TopicID=" + RecordID);
                        deleteContentRecords(core, "Member Topic Rules", "TopicID=" + RecordID);
                        break;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
    }
}