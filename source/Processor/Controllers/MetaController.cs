
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
    public class MetaController {
        //
        //====================================================================================================
        /// <summary>
        /// When possible, create contentMetaData object and read property. When property is a variable, use this
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentName"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string getContentProperty(CoreController core, string contentName, string propertyName) {
            string result = "";
            MetaModel meta;
            //
            meta = MetaModel.createByUniqueName(core, encodeText(contentName));
            switch (GenericController.vbUCase(encodeText(propertyName))) {
                case "CONTENTCONTROLCRITERIA":
                    result = meta.legacyContentControlCriteria;
                    break;
                case "ACTIVEONLY":
                    result = meta.activeOnly.ToString();
                    break;
                case "ADMINONLY":
                    result = meta.adminOnly.ToString();
                    break;
                case "ALIASID":
                    result = meta.aliasID;
                    break;
                case "ALIASNAME":
                    result = meta.aliasName;
                    break;
                case "ALLOWADD":
                    result = meta.allowAdd.ToString();
                    break;
                case "ALLOWDELETE":
                    result = meta.allowDelete.ToString();
                    //Case "CHILDIDLIST"
                    //    main_result = Contentdefinition.ChildIDList
                    break;
                case "DATASOURCEID":
                    result = meta.dataSourceId.ToString();
                    break;
                case "DEFAULTSORTMETHOD":
                    result = meta.defaultSortMethod;
                    break;
                case "DEVELOPERONLY":
                    result = meta.developerOnly.ToString();
                    break;
                case "FIELDCOUNT":
                    result = meta.fields.Count.ToString();
                    //Case "FIELDPOINTER"
                    //    main_result = Contentdefinition.FieldPointer
                    break;
                case "ID":
                    result = meta.id.ToString();
                    break;
                case "NAME":
                    result = meta.name;
                    break;
                case "PARENTID":
                    result = meta.parentID.ToString();
                    //Case "SINGLERECORD"
                    //    main_result = Contentdefinition.SingleRecord
                    break;
                case "CONTENTTABLENAME":
                    result = meta.tableName;
                    break;
                case "CONTENTDATASOURCENAME":
                    result = meta.dataSourceName;
                    //Case "AUTHORINGTABLENAME"
                    //    result = Contentdefinition.AuthoringTableName
                    //Case "AUTHORINGDATASOURCENAME"
                    //    result = Contentdefinition.AuthoringDataSourceName
                    break;
                case "WHERECLAUSE":
                    result = meta.whereClause;
                    //Case "ALLOWWORKFLOWAUTHORING"
                    //    result = Contentdefinition.AllowWorkflowAuthoring.ToString
                    break;
                case "DROPDOWNFIELDLIST":
                    result = meta.dropDownFieldList;
                    break;
                case "SELECTFIELDLIST":
                    result = meta.selectCommaList;
                    break;
                default:
                    //throw new GenericException("Unexpected exception"); // todo - remove this - handleLegacyError14(MethodName, "Content Property [" & genericController.encodeText(PropertyName) & "] was not found in content [" & genericController.encodeText(ContentName) & "]")
                    break;
            }
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// Returns true if childContentID is in parentContentID
        /// </summary>
        /// <param name="core"></param>
        /// <param name="childContentId"></param>
        /// <param name="parentContentId"></param>
        /// <returns></returns>
        public static bool isWithinContent(CoreController core, int childContentId, int parentContentId) {
            try {
                if ((childContentId <= 0) || (parentContentId <= 0)) { return false; }
                if (childContentId == parentContentId) { return true; }
                var meta = MetaModel.create(core, parentContentId);
                if (meta == null) { return false; }
                if (meta.childIdList(core).Count == 0) { return false; }
                if (!meta.childIdList(core).Contains(childContentId)) { return false; }
                foreach (int contentId in meta.childIdList(core)) {
                    if (isWithinContent(core, contentId, parentContentId)) { return true; }
                }
                return false;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
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
                    var metaData = MetaModel.create(core, ContentID);
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
                var meta = MetaModel.createByUniqueName(core, contentName);
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
                var meta = MetaModel.createByUniqueName(core, contentName);
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
                var meta = MetaModel.create(core, contentID);
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
        /// Verify a content entry and return the id. If it does not exist, it is added with default values
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentMetadata"></param>
        /// <returns></returns>
        public static int verifyContent_returnId(CoreController core, MetaModel contentMetadata) {
            int returnContentId = 0;
            try {
                if (string.IsNullOrWhiteSpace(contentMetadata.name)) { throw new GenericException("Content name can not be blank"); }
                if (string.IsNullOrWhiteSpace(contentMetadata.tableName)) { throw new GenericException("Content table name can not be blank"); }
                using (var db = new DbController(core, contentMetadata.dataSourceName)) {
                    //
                    // -- verify table
                    db.createSQLTable(contentMetadata.tableName);
                    //
                    // get contentId, guid, IsBaseContent
                    var content = ContentModel.createByUniqueName(core, contentMetadata.name);
                    if (content == null) {
                        content = ContentModel.addDefault(core, contentMetadata);
                        content.name = contentMetadata.name;
                        content.save(core);
                    }
                    returnContentId = content.id;
                    string contentGuid = content.ccguid;
                    bool ContentIsBaseContent = content.isBaseContent;
                    int ContentIDofContent = MetaModel.getContentId(core, "content");
                    //
                    // get parentId
                    int parentId = 0;
                    if (!string.IsNullOrEmpty(contentMetadata.parentName)) {
                        var parentContent = ContentModel.createByUniqueName(core, contentMetadata.parentName);
                        if (parentContent != null) { parentId = parentContent.id; }
                    }
                    //
                    // get InstalledByCollectionID
                    int InstalledByCollectionID = 0;
                    var collection = AddonCollectionModel.create(core, contentMetadata.installedByCollectionGuid);
                    if (collection != null) { InstalledByCollectionID = collection.id; }
                    //
                    // Get the table object for this content metadata, create one if missing
                    var table = TableModel.createByUniqueName(core, contentMetadata.tableName);
                    if (table == null) {
                        //
                        // -- table model not found, create it - only name and datasource matter
                        var tableMetaData = MetaModel.createByUniqueName(core, "tables");
                        if (tableMetaData == null) {
                            //
                            // -- table metadata not fouond, create without defaults
                            table = TableModel.addEmpty(core);
                        } else {
                            //
                            // -- create model with table metadata defaults
                            table = TableModel.addDefault(core, tableMetaData);
                        }
                        table.name = contentMetadata.tableName;
                        if (!DataSourceModel.isDataSourceDefault(contentMetadata.dataSourceName)) {
                            //
                            // -- is not the default datasource, open a datasource model for it to get the id
                            var dataSource = DataSourceModel.createByUniqueName(core, contentMetadata.dataSourceName);
                            if (dataSource == null) {
                                //
                                // -- datasource record does not exist, create it now
                                dataSource = DataSourceModel.addEmpty(core);
                                dataSource.name = contentMetadata.dataSourceName;
                                dataSource.save(core);
                            }
                        }
                        table.save(core);
                        content.contentTableID = table.id;
                        content.authoringTableID = table.id;
                        content.save(core);
                    }
                    //
                    // sortmethod - First try lookup by name
                    int defaultSortMethodID = 0;
                    if (!string.IsNullOrEmpty(contentMetadata.defaultSortMethod)) {
                        var sortMethod = SortMethodModel.createByUniqueName(core, contentMetadata.defaultSortMethod);
                        if (sortMethod != null) { defaultSortMethodID = sortMethod.id; }
                    }
                    if (defaultSortMethodID == 0) {
                        //
                        // fallback - maybe they put the orderbyclause in (common mistake)
                        var sortMethodList = SortMethodModel.createList(core, "(OrderByClause=" + DbController.encodeSQLText(contentMetadata.defaultSortMethod) + ")and(active<>0)", "id");
                        if (sortMethodList.Count() > 0) { defaultSortMethodID = sortMethodList.First().id; }
                    }

                    //
                    // ----- update record
                    //
                    var sqlList = new SqlFieldListClass();
                    sqlList.add("name", DbController.encodeSQLText(contentMetadata.name));
                    sqlList.add("CREATEKEY", "0");
                    sqlList.add("active", DbController.encodeSQLBoolean(contentMetadata.active));
                    sqlList.add("contentControlId", DbController.encodeSQLNumber(ContentIDofContent));
                    sqlList.add("AllowAdd", DbController.encodeSQLBoolean(contentMetadata.allowAdd));
                    sqlList.add("AllowDelete", DbController.encodeSQLBoolean(contentMetadata.allowDelete));
                    sqlList.add("AllowWorkflowAuthoring", DbController.encodeSQLBoolean(false));
                    sqlList.add("DeveloperOnly", DbController.encodeSQLBoolean(contentMetadata.developerOnly));
                    sqlList.add("AdminOnly", DbController.encodeSQLBoolean(contentMetadata.adminOnly));
                    sqlList.add("ParentID", DbController.encodeSQLNumber(parentId));
                    sqlList.add("DefaultSortMethodID", DbController.encodeSQLNumber(defaultSortMethodID));
                    sqlList.add("DropDownFieldList", DbController.encodeSQLText(encodeEmpty(contentMetadata.dropDownFieldList, "Name")));
                    sqlList.add("ContentTableID", DbController.encodeSQLNumber(table.id));
                    sqlList.add("AuthoringTableID", DbController.encodeSQLNumber(table.id));
                    sqlList.add("ModifiedDate", DbController.encodeSQLDate(DateTime.Now));
                    sqlList.add("CreatedBy", DbController.encodeSQLNumber(SystemMemberID));
                    sqlList.add("ModifiedBy", DbController.encodeSQLNumber(SystemMemberID));
                    sqlList.add("AllowCalendarEvents", DbController.encodeSQLBoolean(contentMetadata.allowCalendarEvents));
                    sqlList.add("AllowContentTracking", DbController.encodeSQLBoolean(contentMetadata.allowContentTracking));
                    sqlList.add("AllowTopicRules", DbController.encodeSQLBoolean(contentMetadata.allowTopicRules));
                    sqlList.add("AllowContentChildTool", DbController.encodeSQLBoolean(contentMetadata.allowContentChildTool));
                    sqlList.add("IconLink", DbController.encodeSQLText(encodeEmpty(contentMetadata.iconLink, "")));
                    sqlList.add("IconHeight", DbController.encodeSQLNumber(contentMetadata.iconHeight));
                    sqlList.add("IconWidth", DbController.encodeSQLNumber(contentMetadata.iconWidth));
                    sqlList.add("IconSprites", DbController.encodeSQLNumber(contentMetadata.iconSprites));
                    sqlList.add("installedByCollectionid", DbController.encodeSQLNumber(InstalledByCollectionID));
                    db.updateTableRecord("ccContent", "ID=" + returnContentId, sqlList);
                    ContentModel.invalidateRecordCache(core, returnContentId);
                    //
                    // Verify Core Content Definition Fields
                    if (parentId < 1) {
                        MetaFieldModel field = null;
                        //
                        // metadata does not inherit its fields, create what is needed for a non-inherited metadata
                        //
                        if (!MetaController.isMetaDataField(core, returnContentId, "ID")) {
                            field = new Models.Domain.MetaFieldModel {
                                nameLc = "id",
                                active = true,
                                fieldTypeId = CPContentBaseClass.fileTypeIdEnum.AutoIdIncrement,
                                editSortPriority = 100,
                                authorable = false,
                                caption = "ID",
                                defaultValue = "",
                                isBaseField = contentMetadata.isBaseContent
                            };
                            verifyContentField_returnId(core, contentMetadata.name, field);
                        }
                        //
                        if (!MetaController.isMetaDataField(core, returnContentId, "name")) {
                            field = new Models.Domain.MetaFieldModel {
                                nameLc = "name",
                                active = true,
                                fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Text,
                                editSortPriority = 110,
                                authorable = true,
                                caption = "Name",
                                defaultValue = "",
                                isBaseField = contentMetadata.isBaseContent
                            };
                            verifyContentField_returnId(core, contentMetadata.name, field);
                        }
                        //
                        if (!MetaController.isMetaDataField(core, returnContentId, "active")) {
                            field = new Models.Domain.MetaFieldModel {
                                nameLc = "active",
                                active = true,
                                fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Boolean,
                                editSortPriority = 200,
                                authorable = true,
                                caption = "Active",
                                defaultValue = "1",
                                isBaseField = contentMetadata.isBaseContent
                            };
                            verifyContentField_returnId(core, contentMetadata.name, field);
                        }
                        //
                        if (!MetaController.isMetaDataField(core, returnContentId, "sortorder")) {
                            field = new Models.Domain.MetaFieldModel {
                                nameLc = "sortorder",
                                active = true,
                                fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Text,
                                editSortPriority = 2000,
                                authorable = false,
                                caption = "Alpha Sort Order",
                                defaultValue = "",
                                isBaseField = contentMetadata.isBaseContent
                            };
                            verifyContentField_returnId(core, contentMetadata.name, field);
                        }
                        //
                        if (!MetaController.isMetaDataField(core, returnContentId, "dateadded")) {
                            field = new Models.Domain.MetaFieldModel {
                                nameLc = "dateadded",
                                active = true,
                                fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Date,
                                editSortPriority = 9999,
                                authorable = false,
                                caption = "Date Added",
                                defaultValue = "",
                                isBaseField = contentMetadata.isBaseContent
                            };
                            verifyContentField_returnId(core, contentMetadata.name, field);
                        }
                        if (!MetaController.isMetaDataField(core, returnContentId, "createdby")) {
                            field = new Models.Domain.MetaFieldModel {
                                nameLc = "createdby",
                                active = true,
                                fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Lookup,
                                editSortPriority = 9999,
                                authorable = false,
                                caption = "Created By"
                            };
                            field.set_lookupContentName(core, "People");
                            field.defaultValue = "";
                            field.isBaseField = contentMetadata.isBaseContent;
                            verifyContentField_returnId(core, contentMetadata.name, field);
                        }
                        if (!MetaController.isMetaDataField(core, returnContentId, "modifieddate")) {
                            field = new Models.Domain.MetaFieldModel {
                                nameLc = "modifieddate",
                                active = true,
                                fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Date,
                                editSortPriority = 9999,
                                authorable = false,
                                caption = "Date Modified",
                                defaultValue = "",
                                isBaseField = contentMetadata.isBaseContent
                            };
                            verifyContentField_returnId(core, contentMetadata.name, field);
                        }
                        if (!MetaController.isMetaDataField(core, returnContentId, "modifiedby")) {
                            field = new Models.Domain.MetaFieldModel {
                                nameLc = "modifiedby",
                                active = true,
                                fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Lookup,
                                editSortPriority = 9999,
                                authorable = false,
                                caption = "Modified By"
                            };
                            field.set_lookupContentName(core, "People");
                            field.defaultValue = "";
                            field.isBaseField = contentMetadata.isBaseContent;
                            verifyContentField_returnId(core, contentMetadata.name, field);
                        }
                        if (!MetaController.isMetaDataField(core, returnContentId, "ContentControlId")) {
                            field = new Models.Domain.MetaFieldModel {
                                nameLc = "contentcontrolid",
                                active = true,
                                fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Lookup,
                                editSortPriority = 9999,
                                authorable = false,
                                caption = "Controlling Content"
                            };
                            field.set_lookupContentName(core, "Content");
                            field.defaultValue = "";
                            field.isBaseField = contentMetadata.isBaseContent;
                            verifyContentField_returnId(core, contentMetadata.name, field);
                        }
                        if (!MetaController.isMetaDataField(core, returnContentId, "CreateKey")) {
                            field = new Models.Domain.MetaFieldModel {
                                nameLc = "createkey",
                                active = true,
                                fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Integer,
                                editSortPriority = 9999,
                                authorable = false,
                                caption = "Create Key",
                                defaultValue = "",
                                isBaseField = contentMetadata.isBaseContent
                            };
                            verifyContentField_returnId(core, contentMetadata.name, field);
                        }
                        if (!MetaController.isMetaDataField(core, returnContentId, "ccGuid")) {
                            field = new Models.Domain.MetaFieldModel {
                                nameLc = "ccguid",
                                active = true,
                                fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Text,
                                editSortPriority = 9999,
                                authorable = false,
                                caption = "Guid",
                                defaultValue = "",
                                isBaseField = contentMetadata.isBaseContent
                            };
                            verifyContentField_returnId(core, contentMetadata.name, field);
                        }
                        // -- 20171029 - had to un-deprecate because compatibility issues are too timeconsuming
                        if (!MetaController.isMetaDataField(core, returnContentId, "ContentCategoryId")) {
                            field = new Models.Domain.MetaFieldModel {
                                nameLc = "contentcategoryid",
                                active = true,
                                fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Integer,
                                editSortPriority = 9999,
                                authorable = false,
                                caption = "Content Category",
                                defaultValue = "",
                                isBaseField = contentMetadata.isBaseContent
                            };
                            verifyContentField_returnId(core, contentMetadata.name, field);
                        }
                    }
                }
                //
                // ----- Load metadata
                //
                ContentModel.invalidateTableCache(core);
                ContentFieldModel.invalidateTableCache(core);
                core.clearMetaData();
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnContentId;
        }
        //
        // ====================================================================================================================
        /// <summary>
        /// Verify a metadata field and return the recordid
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ContentName"></param>
        /// <param name="field"></param>
        /// <param name="blockCacheClear"></param>
        /// <returns></returns>
        public static int verifyContentField_returnId(CoreController core, string ContentName, Models.Domain.MetaFieldModel field, bool blockCacheClear = false) {
            int returnId = 0;
            try {
                var content = ContentModel.createByUniqueName(core, ContentName);
                if (content == null) {
                    //
                    throw (new GenericException("Could not create field [" + field.nameLc + "] because Content Definition [" + ContentName + "] was not found In ccContent Table."));
                } else if (content.contentTableID <= 0) {
                    //
                    // Content Definition not found
                    throw (new GenericException("Could not create field [" + field.nameLc + "] because Content Definition [" + ContentName + "] has no associated Content Table."));
                } else if (field.fieldTypeId <= 0) {
                    //
                    // invalid field type
                    throw (new GenericException("Could Not create Field [" + field.nameLc + "] because the field type [" + field.fieldTypeId + "] Is Not valid."));
                } else {
                    var table = TableModel.create(core, content.contentTableID);
                    if (table == null) {
                        //
                        throw (new GenericException("Could not create field [" + field.nameLc + "] because Content Definition [" + ContentName + "] does not have a valid table."));
                    } else {
                        bool RecordIsBaseField = false;
                        var contentFieldList = ContentFieldModel.createList(core, "(ContentID=" + DbController.encodeSQLNumber(content.id) + ")and(name=" + DbController.encodeSQLText(field.nameLc) + ")");
                        if (contentFieldList.Count > 0) {
                            returnId = contentFieldList.First().id;
                            RecordIsBaseField = contentFieldList.First().isBaseField;
                        }
                        //
                        // check if this is a non-base field updating a base field
                        if ((!field.isBaseField) && (RecordIsBaseField)) {
                            //
                            // This update is not allowed
                            LogController.handleWarn(core, new GenericException("Warning, updating non-base field with base field, content [" + ContentName + "], field [" + field.nameLc + "]"));
                        }
                        //
                        // Get the DataSourceName - special case model, returns default object if input not valid
                        var dataSource = DataSourceModel.create(core, table.dataSourceID);
                        using (var db = new DbController(core, dataSource.name)) {
                            //
                            // Get the installedByCollectionId
                            int InstalledByCollectionID = 0;
                            if (!string.IsNullOrEmpty(field.installedByCollectionGuid)) {
                                var addonCollection = AddonCollectionModel.create(core, field.installedByCollectionGuid);
                                if (addonCollection != null) {
                                    InstalledByCollectionID = addonCollection.id;
                                }
                            }
                            //
                            // Create or update the Table Field
                            if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.Redirect) {
                                //
                                // Redirect Field
                            } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.ManyToMany) {
                                //
                                // ManyToMany Field
                            } else {
                                //
                                // All other fields
                                db.createSQLTableField(table.name, field.nameLc, field.fieldTypeId);
                            }
                            //
                            // create or update the field
                            SqlFieldListClass sqlList = new SqlFieldListClass();
                            sqlList.add("ACTIVE", DbController.encodeSQLBoolean(field.active));
                            sqlList.add("MODIFIEDBY", DbController.encodeSQLNumber(SystemMemberID));
                            sqlList.add("MODIFIEDDATE", DbController.encodeSQLDate(DateTime.Now));
                            sqlList.add("TYPE", DbController.encodeSQLNumber((int)field.fieldTypeId));
                            sqlList.add("CAPTION", DbController.encodeSQLText(field.caption));
                            sqlList.add("ReadOnly", DbController.encodeSQLBoolean(field.readOnly));
                            sqlList.add("REQUIRED", DbController.encodeSQLBoolean(field.required));
                            sqlList.add("TEXTBUFFERED", SQLFalse);
                            sqlList.add("PASSWORD", DbController.encodeSQLBoolean(field.password));
                            sqlList.add("EDITSORTPRIORITY", DbController.encodeSQLNumber(field.editSortPriority));
                            sqlList.add("ADMINONLY", DbController.encodeSQLBoolean(field.adminOnly));
                            sqlList.add("DEVELOPERONLY", DbController.encodeSQLBoolean(field.developerOnly));
                            sqlList.add("CONTENTCONTROLID", DbController.encodeSQLNumber(MetaModel.getContentId(core, "Content Fields")));
                            sqlList.add("DefaultValue", DbController.encodeSQLText(field.defaultValue));
                            sqlList.add("HTMLCONTENT", DbController.encodeSQLBoolean(field.htmlContent));
                            sqlList.add("NOTEDITABLE", DbController.encodeSQLBoolean(field.notEditable));
                            sqlList.add("AUTHORABLE", DbController.encodeSQLBoolean(field.authorable));
                            sqlList.add("INDEXCOLUMN", DbController.encodeSQLNumber(field.indexColumn));
                            sqlList.add("INDEXWIDTH", DbController.encodeSQLText(field.indexWidth));
                            sqlList.add("INDEXSORTPRIORITY", DbController.encodeSQLNumber(field.indexSortOrder));
                            sqlList.add("REDIRECTID", DbController.encodeSQLText(field.redirectID));
                            sqlList.add("REDIRECTPATH", DbController.encodeSQLText(field.redirectPath));
                            sqlList.add("UNIQUENAME", DbController.encodeSQLBoolean(field.uniqueName));
                            sqlList.add("RSSTITLEFIELD", DbController.encodeSQLBoolean(field.RSSTitleField));
                            sqlList.add("RSSDESCRIPTIONFIELD", DbController.encodeSQLBoolean(field.RSSDescriptionField));
                            sqlList.add("MEMBERSELECTGROUPID", DbController.encodeSQLNumber(field.memberSelectGroupId_get(core)));
                            sqlList.add("installedByCollectionId", DbController.encodeSQLNumber(InstalledByCollectionID));
                            sqlList.add("EDITTAB", DbController.encodeSQLText(field.editTabName));
                            sqlList.add("SCRAMBLE", DbController.encodeSQLBoolean(false));
                            sqlList.add("ISBASEFIELD", DbController.encodeSQLBoolean(field.isBaseField));
                            sqlList.add("LOOKUPLIST", DbController.encodeSQLText(field.lookupList));
                            int RedirectContentID = 0;
                            int LookupContentID = 0;
                            //
                            // -- conditional fields
                            switch (field.fieldTypeId) {
                                case  CPContentBaseClass.fileTypeIdEnum.Lookup:
                                    //
                                    // -- lookup field
                                    //
                                    string LookupContentName = field.get_lookupContentName(core);
                                    if (!string.IsNullOrEmpty(LookupContentName)) {
                                        LookupContentID = MetaModel.getContentId(core, LookupContentName);
                                        if (LookupContentID <= 0) {
                                            LogController.logError(core, "Could not create lookup field [" + field.nameLc + "] for content definition [" + ContentName + "] because no content definition was found For lookup-content [" + LookupContentName + "].");
                                        }
                                    }
                                    sqlList.add("LOOKUPCONTENTID", DbController.encodeSQLNumber(LookupContentID));
                                    break;
                                case  CPContentBaseClass.fileTypeIdEnum.ManyToMany:
                                    //
                                    // -- many-to-many field
                                    //
                                    string ManyToManyContent = field.get_manyToManyContentName(core);
                                    if (!string.IsNullOrEmpty(ManyToManyContent)) {
                                        int ManyToManyContentID = MetaModel.getContentId(core, ManyToManyContent);
                                        if (ManyToManyContentID <= 0) {
                                            LogController.logError(core, "Could not create many-to-many field [" + field.nameLc + "] for [" + ContentName + "] because no content definition was found For many-to-many-content [" + ManyToManyContent + "].");
                                        }
                                        sqlList.add("MANYTOMANYCONTENTID", DbController.encodeSQLNumber(ManyToManyContentID));
                                    }
                                    //
                                    string ManyToManyRuleContent = field.get_manyToManyRuleContentName(core);
                                    if (!string.IsNullOrEmpty(ManyToManyRuleContent)) {
                                        int ManyToManyRuleContentID = MetaModel.getContentId(core, ManyToManyRuleContent);
                                        if (ManyToManyRuleContentID <= 0) {
                                            LogController.logError(core, "Could not create many-to-many field [" + field.nameLc + "] for [" + ContentName + "] because no content definition was found For many-to-many-rule-content [" + ManyToManyRuleContent + "].");
                                        }
                                        sqlList.add("MANYTOMANYRULECONTENTID", DbController.encodeSQLNumber(ManyToManyRuleContentID));
                                    }
                                    sqlList.add("MANYTOMANYRULEPRIMARYFIELD", DbController.encodeSQLText(field.ManyToManyRulePrimaryField));
                                    sqlList.add("MANYTOMANYRULESECONDARYFIELD", DbController.encodeSQLText(field.ManyToManyRuleSecondaryField));
                                    break;
                                case  CPContentBaseClass.fileTypeIdEnum.Redirect:
                                    //
                                    // -- redirect field
                                    string RedirectContentName = field.get_redirectContentName(core);
                                    if (!string.IsNullOrEmpty(RedirectContentName)) {
                                        RedirectContentID = MetaModel.getContentId(core, RedirectContentName);
                                        if (RedirectContentID <= 0) {
                                            LogController.logError(core, "Could not create redirect field [" + field.nameLc + "] for Content Definition [" + ContentName + "] because no content definition was found For redirect-content [" + RedirectContentName + "].");
                                        }
                                    }
                                    sqlList.add("REDIRECTCONTENTID", DbController.encodeSQLNumber(RedirectContentID));
                                    break;
                            }
                            //
                            if (returnId == 0) {
                                sqlList.add("NAME", DbController.encodeSQLText(field.nameLc));
                                sqlList.add("CONTENTID", DbController.encodeSQLNumber(content.id));
                                sqlList.add("CREATEKEY", "0");
                                sqlList.add("DATEADDED", DbController.encodeSQLDate(DateTime.Now));
                                sqlList.add("CREATEDBY", DbController.encodeSQLNumber(SystemMemberID));
                                returnId = db.insertTableRecordGetId("ccFields");
                                //
                                if (!blockCacheClear) {
                                    core.cache.invalidateAll();
                                    core.clearMetaData();
                                }
                            }
                            if (returnId == 0) {
                                throw (new GenericException("Could Not create Field [" + field.nameLc + "] because insert into ccfields failed."));
                            } else {
                                db.updateTableRecord("ccFields", "ID=" + returnId, sqlList);
                                ContentFieldModel.invalidateRecordCache(core, returnId);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnId;
        }
        //
        //=============================================================
        /// <summary>
        /// isContentFieldSupported
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentName"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static bool isContentFieldSupported(CoreController core, string contentName, string fieldName) {
            try {
                var metaData = MetaModel.createByUniqueName(core, contentName);
                if (metaData != null) { return metaData.fields.ContainsKey(fieldName.ToLowerInvariant()); }
                return false;
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
            var meta = MetaModel.createByUniqueName(core, contentName);
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
        public static void setContentControlId(CoreController core, MetaModel contentMeta, int recordId, int newContentControlID, string UsedIDString = "") {
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
                var meta = MetaModel.createByUniqueName(core, ContentName);
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
                var meta = MetaModel.createByUniqueName(core, contentName);
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
            var meta = MetaModel.createByUniqueName(core, contentName);
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
                var meta = MetaModel.createByUniqueName(core, contentName);
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
        public static bool isMetaDataField(CoreController core, int ContentID, string FieldName) {
            var meta = MetaModel.create(core, ContentID);
            if (meta == null) { return false; }
            return meta.fields.ContainsKey(FieldName.Trim().ToLower());
        }
        //
        //========================================================================
        /// <summary>
        /// InsertContentRecordGetID
        /// Inserts a record based on a content definition.
        /// Returns the ID of the record, -1 if error
        /// </summary>
        /// <param name="contentName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        ///
        public static int insertContentRecordGetID(CoreController core, string contentName, int userId) {
            var meta = MetaModel.createByUniqueName(core, contentName);
            if (meta == null) { return 0; }
            using (var db = new DbController(core, meta.dataSourceName)) {
                return core.db.insertTableRecordGetId(meta.tableName, userId);
            }
        }
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
            var meta = MetaModel.createByUniqueName(core, contentName);
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
            var meta = MetaModel.createByUniqueName(core, contentName);
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
                var meta = MetaModel.createByUniqueName(core, contentName);
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
            var meta = MetaModel.createByUniqueName(core, contentName);
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
        public static void deleteContentRules(CoreController core, MetaModel meta, int RecordID) {
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
        //
        //========================================================================
        /// <summary>
        /// Get FieldDescritor from FieldType
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        //
        public static string getFieldTypeNameFromFieldTypeId(CoreController core, CPContentBaseClass.fileTypeIdEnum fieldType) {
            string returnFieldTypeName = "";
            try {
                switch (fieldType) {
                    case  CPContentBaseClass.fileTypeIdEnum.Boolean:
                        returnFieldTypeName = FieldTypeNameBoolean;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.Currency:
                        returnFieldTypeName = FieldTypeNameCurrency;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.Date:
                        returnFieldTypeName = FieldTypeNameDate;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.File:
                        returnFieldTypeName = FieldTypeNameFile;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.Float:
                        returnFieldTypeName = FieldTypeNameFloat;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.FileImage:
                        returnFieldTypeName = FieldTypeNameImage;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.Link:
                        returnFieldTypeName = FieldTypeNameLink;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.ResourceLink:
                        returnFieldTypeName = FieldTypeNameResourceLink;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.Integer:
                        returnFieldTypeName = FieldTypeNameInteger;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.LongText:
                        returnFieldTypeName = FieldTypeNameLongText;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.Lookup:
                        returnFieldTypeName = FieldTypeNameLookup;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.MemberSelect:
                        returnFieldTypeName = FieldTypeNameMemberSelect;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.Redirect:
                        returnFieldTypeName = FieldTypeNameRedirect;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.ManyToMany:
                        returnFieldTypeName = FieldTypeNameManyToMany;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.FileText:
                        returnFieldTypeName = FieldTypeNameTextFile;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.FileCSS:
                        returnFieldTypeName = FieldTypeNameCSSFile;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.FileXML:
                        returnFieldTypeName = FieldTypeNameXMLFile;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.FileJavascript:
                        returnFieldTypeName = FieldTypeNameJavascriptFile;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.Text:
                        returnFieldTypeName = FieldTypeNameText;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.HTML:
                        returnFieldTypeName = FieldTypeNameHTML;
                        break;
                    case  CPContentBaseClass.fileTypeIdEnum.FileHTML:
                        returnFieldTypeName = FieldTypeNameHTMLFile;
                        break;
                    default:
                        if (fieldType == CPContentBaseClass.fileTypeIdEnum.AutoIdIncrement) {
                            returnFieldTypeName = "AutoIncrement";
                        } else if (fieldType == CPContentBaseClass.fileTypeIdEnum.MemberSelect) {
                            returnFieldTypeName = "MemberSelect";
                        } else {
                            //
                            // If field type is ignored, call it a text field
                            //
                            returnFieldTypeName = FieldTypeNameText;
                        }
                        break;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex); // "Unexpected exception")
                throw;
            }
            return returnFieldTypeName;
        }
        //
        //=============================================================================
        /// <summary>
        /// Imports the named table into the content system
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="TableName"></param>
        /// <param name="ContentName"></param>
        //
        public static void createContentFromSQLTable(CoreController core, DataSourceModel DataSource, string TableName, string ContentName) {
            try {
                //
                // -- add a record if none found
                using( var targetDb = new DbController( core, DataSource.name )) {
                    using (DataTable dt = targetDb.executeQuery("select top 1 * from " + TableName)) {
                        if (dt.Rows.Count == 0) { core.db.executeNonQuery("insert into cccontent default values"); }
                    }
                    using (DataTable dt = targetDb.executeQuery("select top 1 * from " + TableName)) {
                        if (dt.Rows.Count == 0) { throw new GenericException("Could Not add a record To table [" + TableName + "]."); }
                        //
                        // -- Find/Create the Content Definition
                        int contentId = DbController.getContentId(core, ContentName);
                        if (contentId <= 0) {
                            //
                            // -- Content definition not found, create it
                            contentId = verifyContent_returnId(core, new MetaModel() {
                                tableName = TableName,
                                name = ContentName,
                                active = true
                            });
                            core.cache.invalidateAll();
                            core.clearMetaData();
                        }
                        //
                        // -- Create the ccFields records for the new table, locate the field in the content field table
                        using (DataTable dtFields = core.db.executeQuery("Select name from ccFields where ContentID=" + contentId + ";")) {
                            //
                            // ----- verify all the table fields
                            foreach (DataColumn dcTableColumns in dt.Columns) {
                                //
                                // ----- see if the field is already in the content fields
                                string UcaseTableColumnName = GenericController.vbUCase(dcTableColumns.ColumnName);
                                bool ContentFieldFound = false;
                                foreach (DataRow drContentRecords in dtFields.Rows) {
                                    if (GenericController.vbUCase(GenericController.encodeText(drContentRecords["name"])) == UcaseTableColumnName) {
                                        ContentFieldFound = true;
                                        break;
                                    }
                                }
                                if (!ContentFieldFound) {
                                    //
                                    // -- create the content field
                                    createContentFieldFromTableField(core, ContentName, dcTableColumns.ColumnName, encodeInteger(dcTableColumns.DataType));
                                } else {
                                    //
                                    // -- touch field so upgrade does not delete it
                                    core.db.executeQuery("update ccFields Set CreateKey=0 where (Contentid=" + contentId + ") And (name = " + DbController.encodeSQLText(UcaseTableColumnName) + ")");
                                }
                            }
                        }
                        //
                        // -- Fill ContentControlID fields with new ContentID
                        targetDb.executeQuery("Update " + TableName + " Set ContentControlID=" + contentId + " where (ContentControlID Is null);");
                        //
                        // ----- Load metadata, Load only if the previous state of autoload was true, Leave Autoload false during load so more do not trigger
                        core.cache.invalidateAll();
                        core.clearMetaData();
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        // 
        //========================================================================
        /// <summary>
        /// Define a Content Definition Field based only on what is known from a SQL table
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="FieldName"></param>
        /// <param name="ADOFieldType"></param>
        public static void createContentFieldFromTableField(CoreController core, string ContentName, string FieldName, int ADOFieldType) {
            try {
                //
                MetaFieldModel field = new MetaFieldModel {
                    fieldTypeId = core.db.getFieldTypeIdByADOType(ADOFieldType),
                    caption = FieldName,
                    editSortPriority = 1000,
                    readOnly = false,
                    authorable = true,
                    adminOnly = false,
                    developerOnly = false,
                    textBuffered = false,
                    htmlContent = false
                };
                //
                switch (GenericController.vbUCase(FieldName)) {
                    //
                    // --- Core fields
                    //
                    case "NAME":
                        field.caption = "Name";
                        field.editSortPriority = 100;
                        break;
                    case "ACTIVE":
                        field.caption = "Active";
                        field.editSortPriority = 200;
                        field.fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Boolean;
                        field.defaultValue = "1";
                        break;
                    case "DATEADDED":
                        field.caption = "Created";
                        field.readOnly = true;
                        field.editSortPriority = 5020;
                        break;
                    case "CREATEDBY":
                        field.caption = "Created By";
                        field.fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Lookup;
                        field.set_lookupContentName(core, "Members");
                        field.readOnly = true;
                        field.editSortPriority = 5030;
                        break;
                    case "MODIFIEDDATE":
                        field.caption = "Modified";
                        field.readOnly = true;
                        field.editSortPriority = 5040;
                        break;
                    case "MODIFIEDBY":
                        field.caption = "Modified By";
                        field.fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Lookup;
                        field.set_lookupContentName(core, "Members");
                        field.readOnly = true;
                        field.editSortPriority = 5050;
                        break;
                    case "ID":
                        field.caption = "Number";
                        field.readOnly = true;
                        field.editSortPriority = 5060;
                        field.authorable = true;
                        field.adminOnly = false;
                        field.developerOnly = true;
                        break;
                    case "CONTENTCONTROLID":
                        field.caption = "Content Definition";
                        field.fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Lookup;
                        field.set_lookupContentName(core, "Content");
                        field.editSortPriority = 5070;
                        field.authorable = true;
                        field.readOnly = false;
                        field.adminOnly = true;
                        field.developerOnly = true;
                        break;
                    case "CREATEKEY":
                        field.caption = "CreateKey";
                        field.readOnly = true;
                        field.editSortPriority = 5080;
                        field.authorable = false;
                        //
                        // --- fields related to body content
                        //
                        break;
                    case "HEADLINE":
                        field.caption = "Headline";
                        field.editSortPriority = 1000;
                        field.htmlContent = false;
                        break;
                    case "DATESTART":
                        field.caption = "Date Start";
                        field.editSortPriority = 1100;
                        break;
                    case "DATEEND":
                        field.caption = "Date End";
                        field.editSortPriority = 1200;
                        break;
                    case "PUBDATE":
                        field.caption = "Publish Date";
                        field.editSortPriority = 1300;
                        break;
                    case "ORGANIZATIONID":
                        field.caption = "Organization";
                        field.fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Lookup;
                        field.set_lookupContentName(core, "Organizations");
                        field.editSortPriority = 2005;
                        field.authorable = true;
                        field.readOnly = false;
                        break;
                    case "COPYFILENAME":
                        field.caption = "Copy";
                        field.fieldTypeId = CPContentBaseClass.fileTypeIdEnum.FileHTML;
                        field.textBuffered = true;
                        field.editSortPriority = 2010;
                        break;
                    case "BRIEFFILENAME":
                        field.caption = "Overview";
                        field.fieldTypeId = CPContentBaseClass.fileTypeIdEnum.FileHTML;
                        field.textBuffered = true;
                        field.editSortPriority = 2020;
                        field.htmlContent = false;
                        break;
                    case "IMAGEFILENAME":
                        field.caption = "Image";
                        field.fieldTypeId = CPContentBaseClass.fileTypeIdEnum.File;
                        field.editSortPriority = 2040;
                        break;
                    case "THUMBNAILFILENAME":
                        field.caption = "Thumbnail";
                        field.fieldTypeId = CPContentBaseClass.fileTypeIdEnum.File;
                        field.editSortPriority = 2050;
                        break;
                    case "CONTENTID":
                        field.caption = "Content";
                        field.fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Lookup;
                        field.set_lookupContentName(core, "Content");
                        field.readOnly = false;
                        field.editSortPriority = 2060;
                        //
                        // --- Record Features
                        //
                        break;
                    case "PARENTID":
                        field.caption = "Parent";
                        field.fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Lookup;
                        field.set_lookupContentName(core, ContentName);
                        field.readOnly = false;
                        field.editSortPriority = 3000;
                        break;
                    case "MEMBERID":
                        field.caption = "Member";
                        field.fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Lookup;
                        field.set_lookupContentName(core, "Members");
                        field.readOnly = false;
                        field.editSortPriority = 3005;
                        break;
                    case "CONTACTMEMBERID":
                        field.caption = "Contact";
                        field.fieldTypeId = CPContentBaseClass.fileTypeIdEnum.Lookup;
                        field.set_lookupContentName(core, "Members");
                        field.readOnly = false;
                        field.editSortPriority = 3010;
                        break;
                    case "ALLOWBULKEMAIL":
                        field.caption = "Allow Bulk Email";
                        field.editSortPriority = 3020;
                        break;
                    case "ALLOWSEEALSO":
                        field.caption = "Allow See Also";
                        field.editSortPriority = 3030;
                        break;
                    case "ALLOWFEEDBACK":
                        field.caption = "Allow Feedback";
                        field.editSortPriority = 3040;
                        field.authorable = false;
                        break;
                    case "SORTORDER":
                        field.caption = "Alpha Sort Order";
                        field.editSortPriority = 3050;
                        //
                        // --- Display only information
                        //
                        break;
                    case "VIEWINGS":
                        field.caption = "Viewings";
                        field.readOnly = true;
                        field.editSortPriority = 5000;
                        field.defaultValue = "0";
                        break;
                    case "CLICKS":
                        field.caption = "Clicks";
                        field.readOnly = true;
                        field.editSortPriority = 5010;
                        field.defaultValue = "0";
                        break;
                }
                verifyContentField_returnId(core, ContentName, field);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //=============================================================================
        /// <summary>
        /// getLinkByContentRecordKey
        /// </summary>
        /// <param name="ContentRecordKey"></param>
        /// <param name="DefaultLink"></param>
        /// <returns></returns>
        [Obsolete("deprecated", true)]
        public string getLinkByContentRecordKey(string ContentRecordKey, string DefaultLink = "") {
            return string.Empty;
            //string result = "";
            //try {
            //    int CSPointer = 0;
            //    string[] KeySplit = null;
            //    int ContentID = 0;
            //    int RecordID = 0;
            //    string ContentName = null;
            //    int templateId = 0;
            //    int ParentID = 0;
            //    string DefaultTemplateLink = null;
            //    string TableName = null;
            //    string DataSource = null;
            //    int ParentContentID = 0;
            //    bool recordfound = false;
            //    //
            //    if (!string.IsNullOrEmpty(ContentRecordKey)) {
            //        //
            //        // First try main_ContentWatch table for a link
            //        //
            //        CSPointer = csOpen("Content Watch", "ContentRecordKey=" + encodeSQLText(ContentRecordKey), "", true, 0, false, false, "Link,Clicks");
            //        if (csOk(CSPointer)) {
            //            result = csData.csGetText(CSPointer, "Link");
            //        }
            //        csData.csClose();
            //        //
            //        if (string.IsNullOrEmpty(result)) {
            //            //
            //            // try template for this page
            //            //
            //            KeySplit = ContentRecordKey.Split('.');
            //            if (KeySplit.GetUpperBound(0) == 1) {
            //                ContentID = GenericController.encodeInteger(KeySplit[0]);
            //                if (ContentID != 0) {
            //                    ContentName = ContentMetaController.getContentNameByID(core, ContentID);
            //                    RecordID = GenericController.encodeInteger(KeySplit[1]);
            //                    if (!string.IsNullOrEmpty(ContentName) & RecordID != 0) {
            //                        if (ContentMetaController.getContentTablename(core, ContentName) == "ccPageContent") {
            //                            CSPointer = csData.csOpenRecord(ContentName, RecordID, false, false, "TemplateID,ParentID");
            //                            if (csOk(CSPointer)) {
            //                                recordfound = true;
            //                                templateId = csGetInteger(CSPointer, "TemplateID");
            //                                ParentID = csGetInteger(CSPointer, "ParentID");
            //                            }
            //                            csClose();
            //                            if (!recordfound) {
            //                                //
            //                                // This content record does not exist - remove any records with this ContentRecordKey pointer
            //                                //
            //                                deleteContentRecords("Content Watch", "ContentRecordKey=" + encodeSQLText(ContentRecordKey));
            //                                core.db.deleteContentRules(MetaModel.getContentId(core, ContentName), RecordID);
            //                            } else {

            //                                if (templateId != 0) {
            //                                    CSPointer = csData.csOpenRecord("Page Templates", templateId, false, false, "Link");
            //                                    if (csOk(CSPointer)) {
            //                                        result = csGetText(CSPointer, "Link");
            //                                    }
            //                                    csClose();
            //                                }
            //                                if (string.IsNullOrEmpty(result) && ParentID != 0) {
            //                                    TableName = ContentMetaController.getContentTablename(core, ContentName);
            //                                    DataSource = ContentMetaController.getContentDataSource(core, ContentName);
            //                                    CSPointer = csOpenSql("Select ContentControlID from " + TableName + " where ID=" + RecordID, DataSource);
            //                                    if (csOk(CSPointer)) {
            //                                        ParentContentID = GenericController.encodeInteger(csGetText(CSPointer, "contentControlId"));
            //                                    }
            //                                    csClose();
            //                                    if (ParentContentID != 0) {
            //                                        result = getLinkByContentRecordKey(encodeText(ParentContentID + "." + ParentID), "");
            //                                    }
            //                                }
            //                                if (string.IsNullOrEmpty(result)) {
            //                                    DefaultTemplateLink = core.siteProperties.getText("SectionLandingLink", "/" + core.siteProperties.serverPageDefault);
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //            if (!string.IsNullOrEmpty(result)) {
            //                result = GenericController.modifyLinkQuery(result, rnPageId, RecordID.ToString(), true);
            //            }
            //        }
            //    }
            //    //
            //    if (string.IsNullOrEmpty(result)) {
            //        result = DefaultLink;
            //    }
            //    //
            //    result = GenericController.encodeVirtualPath(result, core.appConfig.cdnFileUrl, appRootPath, core.webServer.requestDomain);
            //} catch (Exception ex) {
            //    LogController.handleError(core, ex);
            //}
            //return result;
        }

    }
}