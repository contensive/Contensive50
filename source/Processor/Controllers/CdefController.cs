﻿
using System;
using System.Collections.Generic;
using Contensive.Processor.Models.Db;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using System.Data;
using System.Linq;
using Contensive.Processor.Exceptions;
//
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// Static methods that support the cdef domain model.
    /// </summary>
    public class CdefController {
        //
        //====================================================================================================
        /// <summary>
        /// When possible, create cdef object and read property. When property is a variable, use this
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentName"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string getContentProperty(CoreController core, string contentName, string propertyName) {
            string result = "";
            CDefDomainModel meta;
            //
            meta = CDefDomainModel.createByUniqueName(core, encodeText(contentName));
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
                if ((childContentId<=0) || (parentContentId <= 0)) { return false; }
                if (childContentId == parentContentId) { return true; }
                var cdef = CDefDomainModel.create(core, parentContentId);
                if (cdef == null) { return false; }
                if (cdef.childIdList(core).Count == 0) { return false; }
                if (!cdef.childIdList(core).Contains(childContentId)) { return false; }
                foreach (int contentId in cdef.childIdList(core)) {
                   if ( isWithinContent(core, contentId, parentContentId)) { return true; }
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
        public static List<int> getEditableCdefIdList(CoreController core) {
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
                    var CDef =  CDefDomainModel.create(core, ContentID);
                    if (CDef != null) {
                        returnList.AddRange(CDef.childIdList(core));
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
                if ( childContent!=null) {
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
                // ----- Load CDef
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
                var meta = CDefDomainModel.createByUniqueName(core, contentName);
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
                var meta = CDefDomainModel.createByUniqueName(core, contentName);
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
                var meta = CDefDomainModel.create(core, contentID);
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
        /// <param name="cdef"></param>
        /// <returns></returns>
        public static int verifyContent_returnId(CoreController core, CDefDomainModel cdef) {
            int returnContentId = 0;
            try {
                if (string.IsNullOrWhiteSpace(cdef.name)) { throw new GenericException("Content name can not be blank"); }
                if (string.IsNullOrWhiteSpace(cdef.tableName)) { throw new GenericException("Content table name can not be blank"); }
                //
                // -- verify table
                core.db.createSQLTable(cdef.dataSourceName, cdef.tableName);
                //
                // get contentId, guid, IsBaseContent
                var content = ContentModel.create(core, cdef.name);
                if ( content == null ) {
                    content = ContentModel.addDefault(core, cdef);
                }
                returnContentId = content.id;
                string contentGuid = content.ccguid;
                bool ContentIsBaseContent = content.isBaseContent;
                int ContentIDofContent = CDefDomainModel.getContentId(core, "content");
                //
                // get parentId
                int parentId = 0;
                if (!string.IsNullOrEmpty(cdef.parentName)) {
                    var parentContent = ContentModel.createByUniqueName(core, cdef.parentName);
                    if ( parentContent != null ) { parentId = parentContent.id; }
                }
                //
                // get InstalledByCollectionID
                int InstalledByCollectionID = 0;
                var collection = AddonCollectionModel.create(core, cdef.installedByCollectionGuid);
                if (collection != null) { InstalledByCollectionID = collection.id; }
                //
                // Get the Table Definition ID, create one if missing
                var table = TableModel.createByUniqueName(core, cdef.tableName);
                if ( table==null ) {
                    var tableCdef = CDefDomainModel.createByUniqueName(core, "tables");
                    table = TableModel.addDefault(core, tableCdef);
                }
                //
                // sortmethod - First try lookup by name
                int defaultSortMethodID = 0;
                if (!string.IsNullOrEmpty(cdef.defaultSortMethod)) {
                    var sortMethod = SortMethodModel.createByUniqueName(core, cdef.defaultSortMethod);
                    if (sortMethod != null ) { defaultSortMethodID = sortMethod.id; }
                }
                if (defaultSortMethodID == 0) {
                    //
                    // fallback - maybe they put the orderbyclause in (common mistake)
                    var sortMethodList = SortMethodModel.createList(core, "(OrderByClause=" + DbController.encodeSQLText(cdef.defaultSortMethod) + ")and(active<>0)","id");
                    if (sortMethodList.Count() > 0) { defaultSortMethodID = sortMethodList.First().id; }
                }

                //
                // ----- update record
                //
                var sqlList = new SqlFieldListClass();
                sqlList.add("name", DbController.encodeSQLText(cdef.name));
                sqlList.add("CREATEKEY", "0");
                sqlList.add("active", DbController.encodeSQLBoolean(cdef.active));
                sqlList.add("ContentControlID", DbController.encodeSQLNumber(ContentIDofContent));
                sqlList.add("AllowAdd", DbController.encodeSQLBoolean(cdef.allowAdd));
                sqlList.add("AllowDelete", DbController.encodeSQLBoolean(cdef.allowDelete));
                sqlList.add("AllowWorkflowAuthoring", DbController.encodeSQLBoolean(false));
                sqlList.add("DeveloperOnly", DbController.encodeSQLBoolean(cdef.developerOnly));
                sqlList.add("AdminOnly", DbController.encodeSQLBoolean(cdef.adminOnly));
                sqlList.add("ParentID", DbController.encodeSQLNumber(parentId));
                sqlList.add("DefaultSortMethodID", DbController.encodeSQLNumber(defaultSortMethodID));
                sqlList.add("DropDownFieldList", DbController.encodeSQLText(encodeEmpty(cdef.dropDownFieldList, "Name")));
                sqlList.add("ContentTableID", DbController.encodeSQLNumber(table.id));
                sqlList.add("AuthoringTableID", DbController.encodeSQLNumber(table.id));
                sqlList.add("ModifiedDate", DbController.encodeSQLDate(DateTime.Now));
                sqlList.add("CreatedBy", DbController.encodeSQLNumber(SystemMemberID));
                sqlList.add("ModifiedBy", DbController.encodeSQLNumber(SystemMemberID));
                sqlList.add("AllowCalendarEvents", DbController.encodeSQLBoolean(cdef.allowCalendarEvents));
                sqlList.add("AllowContentTracking", DbController.encodeSQLBoolean(cdef.allowContentTracking));
                sqlList.add("AllowTopicRules", DbController.encodeSQLBoolean(cdef.allowTopicRules));
                sqlList.add("AllowContentChildTool", DbController.encodeSQLBoolean(cdef.allowContentChildTool));
                sqlList.add("IconLink", DbController.encodeSQLText(encodeEmpty(cdef.iconLink, "")));
                sqlList.add("IconHeight", DbController.encodeSQLNumber(cdef.iconHeight));
                sqlList.add("IconWidth", DbController.encodeSQLNumber(cdef.iconWidth));
                sqlList.add("IconSprites", DbController.encodeSQLNumber(cdef.iconSprites));
                sqlList.add("installedByCollectionid", DbController.encodeSQLNumber(InstalledByCollectionID));
                core.db.updateTableRecord("Default", "ccContent", "ID=" + returnContentId, sqlList);
                ContentModel.invalidateRecordCache(core, returnContentId);
                //
                // Verify Core Content Definition Fields
                if (parentId < 1) {
                    CDefFieldModel field = null;
                    //
                    // CDef does not inherit its fields, create what is needed for a non-inherited CDef
                    //
                    if (!CdefController.isCdefField(core, returnContentId, "ID")) {
                        field = new Models.Domain.CDefFieldModel {
                            nameLc = "id",
                            active = true,
                            fieldTypeId = fieldTypeIdAutoIdIncrement,
                            editSortPriority = 100,
                            authorable = false,
                            caption = "ID",
                            defaultValue = "",
                            isBaseField = cdef.isBaseContent
                        };
                        verifyContentField_returnID(core, cdef.name, field);
                    }
                    //
                    if (!CdefController.isCdefField(core,returnContentId, "name")) {
                        field = new Models.Domain.CDefFieldModel {
                            nameLc = "name",
                            active = true,
                            fieldTypeId = fieldTypeIdText,
                            editSortPriority = 110,
                            authorable = true,
                            caption = "Name",
                            defaultValue = "",
                            isBaseField = cdef.isBaseContent
                        };
                        verifyContentField_returnID(core, cdef.name, field);
                    }
                    //
                    if (!CdefController.isCdefField(core,returnContentId, "active")) {
                        field = new Models.Domain.CDefFieldModel {
                            nameLc = "active",
                            active = true,
                            fieldTypeId = fieldTypeIdBoolean,
                            editSortPriority = 200,
                            authorable = true,
                            caption = "Active",
                            defaultValue = "1",
                            isBaseField = cdef.isBaseContent
                        };
                        verifyContentField_returnID(core, cdef.name, field);
                    }
                    //
                    if (!CdefController.isCdefField(core,returnContentId, "sortorder")) {
                        field = new Models.Domain.CDefFieldModel {
                            nameLc = "sortorder",
                            active = true,
                            fieldTypeId = fieldTypeIdText,
                            editSortPriority = 2000,
                            authorable = false,
                            caption = "Alpha Sort Order",
                            defaultValue = "",
                            isBaseField = cdef.isBaseContent
                        };
                        verifyContentField_returnID(core, cdef.name, field);
                    }
                    //
                    if (!CdefController.isCdefField(core,returnContentId, "dateadded")) {
                        field = new Models.Domain.CDefFieldModel {
                            nameLc = "dateadded",
                            active = true,
                            fieldTypeId = fieldTypeIdDate,
                            editSortPriority = 9999,
                            authorable = false,
                            caption = "Date Added",
                            defaultValue = "",
                            isBaseField = cdef.isBaseContent
                        };
                        verifyContentField_returnID(core, cdef.name, field);
                    }
                    if (!CdefController.isCdefField(core,returnContentId, "createdby")) {
                        field = new Models.Domain.CDefFieldModel {
                            nameLc = "createdby",
                            active = true,
                            fieldTypeId = fieldTypeIdLookup,
                            editSortPriority = 9999,
                            authorable = false,
                            caption = "Created By"
                        };
                        field.set_lookupContentName(core, "People");
                        field.defaultValue = "";
                        field.isBaseField = cdef.isBaseContent;
                        verifyContentField_returnID(core, cdef.name, field);
                    }
                    if (!CdefController.isCdefField(core,returnContentId, "modifieddate")) {
                        field = new Models.Domain.CDefFieldModel {
                            nameLc = "modifieddate",
                            active = true,
                            fieldTypeId = fieldTypeIdDate,
                            editSortPriority = 9999,
                            authorable = false,
                            caption = "Date Modified",
                            defaultValue = "",
                            isBaseField = cdef.isBaseContent
                        };
                        verifyContentField_returnID(core, cdef.name, field);
                    }
                    if (!CdefController.isCdefField(core,returnContentId, "modifiedby")) {
                        field = new Models.Domain.CDefFieldModel {
                            nameLc = "modifiedby",
                            active = true,
                            fieldTypeId = fieldTypeIdLookup,
                            editSortPriority = 9999,
                            authorable = false,
                            caption = "Modified By"
                        };
                        field.set_lookupContentName(core, "People");
                        field.defaultValue = "";
                        field.isBaseField = cdef.isBaseContent;
                        verifyContentField_returnID(core, cdef.name, field);
                    }
                    if (!CdefController.isCdefField(core,returnContentId, "ContentControlId")) {
                        field = new Models.Domain.CDefFieldModel {
                            nameLc = "contentcontrolid",
                            active = true,
                            fieldTypeId = fieldTypeIdLookup,
                            editSortPriority = 9999,
                            authorable = false,
                            caption = "Controlling Content"
                        };
                        field.set_lookupContentName(core, "Content");
                        field.defaultValue = "";
                        field.isBaseField = cdef.isBaseContent;
                        verifyContentField_returnID(core, cdef.name, field);
                    }
                    if (!CdefController.isCdefField(core,returnContentId, "CreateKey")) {
                        field = new Models.Domain.CDefFieldModel {
                            nameLc = "createkey",
                            active = true,
                            fieldTypeId = fieldTypeIdInteger,
                            editSortPriority = 9999,
                            authorable = false,
                            caption = "Create Key",
                            defaultValue = "",
                            isBaseField = cdef.isBaseContent
                        };
                        verifyContentField_returnID(core, cdef.name, field);
                    }
                    if (!CdefController.isCdefField(core,returnContentId, "ccGuid")) {
                        field = new Models.Domain.CDefFieldModel {
                            nameLc = "ccguid",
                            active = true,
                            fieldTypeId = fieldTypeIdText,
                            editSortPriority = 9999,
                            authorable = false,
                            caption = "Guid",
                            defaultValue = "",
                            isBaseField = cdef.isBaseContent
                        };
                        verifyContentField_returnID(core, cdef.name, field);
                    }
                    // -- 20171029 - had to un-deprecate because compatibility issues are too timeconsuming
                    if (!CdefController.isCdefField(core,returnContentId, "ContentCategoryId")) {
                        field = new Models.Domain.CDefFieldModel {
                            nameLc = "contentcategoryid",
                            active = true,
                            fieldTypeId = fieldTypeIdInteger,
                            editSortPriority = 9999,
                            authorable = false,
                            caption = "Content Category",
                            defaultValue = "",
                            isBaseField = cdef.isBaseContent
                        };
                        verifyContentField_returnID(core, cdef.name, field);
                    }
                }
                //
                // ----- Load CDef
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
        /// Verify a CDef field and return the recordid
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ContentName"></param>
        /// <param name="field"></param>
        /// <param name="blockCacheClear"></param>
        /// <returns></returns>
        public static int verifyContentField_returnID(CoreController core, string ContentName, Models.Domain.CDefFieldModel field, bool blockCacheClear = false)
        {
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
                        // Get the TableName and DataSourceID
                        string LookupContentName = field.get_lookupContentName(core);
                        string RedirectContentName = field.get_redirectContentName(core);
                        int MemberSelectGroupID = field.memberSelectGroupId_get(core);
                        string LookupList = field.lookupList;
                        //
                        // Get the DataSourceName - special case model, returns default object if input not valid
                        var dataSource = DataSourceModel.create(core, table.dataSourceID);
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
                        if (field.fieldTypeId == fieldTypeIdRedirect) {
                            //
                            // Redirect Field
                        } else if (field.fieldTypeId == fieldTypeIdManyToMany) {
                            //
                            // ManyToMany Field
                        } else {
                            //
                            // All other fields
                            core.db.createSQLTableField(dataSource.name, table.name, field.nameLc, field.fieldTypeId);
                        }
                        //
                        // create or update the field
                        SqlFieldListClass sqlList = new SqlFieldListClass();
                        sqlList.add("ACTIVE", DbController.encodeSQLBoolean(field.active));
                        sqlList.add("MODIFIEDBY", DbController.encodeSQLNumber(SystemMemberID));
                        sqlList.add("MODIFIEDDATE", DbController.encodeSQLDate(DateTime.Now));
                        sqlList.add("TYPE", DbController.encodeSQLNumber(field.fieldTypeId));
                        sqlList.add("CAPTION", DbController.encodeSQLText(field.caption));
                        sqlList.add("ReadOnly", DbController.encodeSQLBoolean(field.readOnly));
                        sqlList.add("REQUIRED", DbController.encodeSQLBoolean(field.required));
                        sqlList.add("TEXTBUFFERED", SQLFalse);
                        sqlList.add("PASSWORD", DbController.encodeSQLBoolean(field.password));
                        sqlList.add("EDITSORTPRIORITY", DbController.encodeSQLNumber(field.editSortPriority));
                        sqlList.add("ADMINONLY", DbController.encodeSQLBoolean(field.adminOnly));
                        sqlList.add("DEVELOPERONLY", DbController.encodeSQLBoolean(field.developerOnly));
                        sqlList.add("CONTENTCONTROLID", DbController.encodeSQLNumber(CDefDomainModel.getContentId(core, "Content Fields")));
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
                        sqlList.add("MEMBERSELECTGROUPID", DbController.encodeSQLNumber(MemberSelectGroupID));
                        sqlList.add("installedByCollectionId", DbController.encodeSQLNumber(InstalledByCollectionID));
                        sqlList.add("EDITTAB", DbController.encodeSQLText(field.editTabName));
                        sqlList.add("SCRAMBLE", DbController.encodeSQLBoolean(false));
                        sqlList.add("ISBASEFIELD", DbController.encodeSQLBoolean(field.isBaseField));
                        sqlList.add("LOOKUPLIST", DbController.encodeSQLText(LookupList));
                        int RedirectContentID = 0;
                        int LookupContentID = 0;
                        //
                        // -- conditional fields
                        switch (field.fieldTypeId) {
                            case _fieldTypeIdLookup:
                                //
                                // -- lookup field
                                //
                                if (!string.IsNullOrEmpty(LookupContentName)) {
                                    LookupContentID = CDefDomainModel.getContentId(core, LookupContentName);
                                    if (LookupContentID <= 0) {
                                        LogController.logError(core, "Could not create lookup field [" + field.nameLc + "] for content definition [" + ContentName + "] because no content definition was found For lookup-content [" + LookupContentName + "].");
                                    }
                                }
                                sqlList.add("LOOKUPCONTENTID", DbController.encodeSQLNumber(LookupContentID));
                                break;
                            case _fieldTypeIdManyToMany:
                                //
                                // -- many-to-many field
                                //
                                string ManyToManyContent = field.get_manyToManyContentName(core);
                                if (!string.IsNullOrEmpty(ManyToManyContent)) {
                                    int ManyToManyContentID = CDefDomainModel.getContentId(core, ManyToManyContent);
                                    if (ManyToManyContentID <= 0) {
                                        LogController.logError(core, "Could not create many-to-many field [" + field.nameLc + "] for [" + ContentName + "] because no content definition was found For many-to-many-content [" + ManyToManyContent + "].");
                                    }
                                    sqlList.add("MANYTOMANYCONTENTID", DbController.encodeSQLNumber(ManyToManyContentID));
                                }
                                //
                                string ManyToManyRuleContent = field.get_manyToManyRuleContentName(core);
                                if (!string.IsNullOrEmpty(ManyToManyRuleContent)) {
                                    int ManyToManyRuleContentID = CDefDomainModel.getContentId(core, ManyToManyRuleContent);
                                    if (ManyToManyRuleContentID <= 0) {
                                        LogController.logError(core, "Could not create many-to-many field [" + field.nameLc + "] for [" + ContentName + "] because no content definition was found For many-to-many-rule-content [" + ManyToManyRuleContent + "].");
                                    }
                                    sqlList.add("MANYTOMANYRULECONTENTID", DbController.encodeSQLNumber(ManyToManyRuleContentID));
                                }
                                sqlList.add("MANYTOMANYRULEPRIMARYFIELD", DbController.encodeSQLText(field.ManyToManyRulePrimaryField));
                                sqlList.add("MANYTOMANYRULESECONDARYFIELD", DbController.encodeSQLText(field.ManyToManyRuleSecondaryField));
                                break;
                            case _fieldTypeIdRedirect:
                                //
                                // -- redirect field
                                if (!string.IsNullOrEmpty(RedirectContentName)) {
                                    RedirectContentID = CDefDomainModel.getContentId(core, RedirectContentName);
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
                            returnId = core.db.insertTableRecordGetId("Default", "ccFields");
                            //
                            if (!blockCacheClear) {
                                core.cache.invalidateAll();
                                core.clearMetaData();
                            }
                        }
                        if (returnId == 0) {
                            throw (new GenericException("Could Not create Field [" + field.nameLc + "] because insert into ccfields failed."));
                        } else {
                            core.db.updateTableRecord("Default", "ccFields", "ID=" + returnId, sqlList);
                            ContentFieldModel.invalidateRecordCache(core, returnId);
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
                var cdef = CDefDomainModel.createByUniqueName(core, contentName);
                if (cdef != null) { return cdef.fields.ContainsKey(fieldName.ToLowerInvariant()); }
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
        public static int getContentIDByTablename(CoreController core, string tableName) {
            if (string.IsNullOrWhiteSpace(tableName)) { return 0; }
            var dt = core.db.executeQuery("select top 1 ContentControlID from " + tableName + " where (contentcontrolid is not null) order by contentcontrolid;");
            if ( dt != null ) { return DbController.getDataRowFieldInteger(dt.Rows[0], "contentcontrolid");  }
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
            var meta = CDefDomainModel.createByUniqueName(core, contentName);
            if ( meta == null ) { return ""; }
            return meta.legacyContentControlCriteria;
        }
        //   
        //============================================================================================================
        /// <summary>
        /// the content control Id for a record, all its edit and archive records, and all its child records returns records affected the contentname contains the record, but we do not know that this is the contentcontrol for the record, read it first to main_Get the correct contentid
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ContentID"></param>
        /// <param name="RecordID"></param>
        /// <param name="NewContentControlID"></param>
        /// <param name="UsedIDString"></param>
        public static void setContentControlId(CoreController core, int ContentID, int RecordID, int NewContentControlID, string UsedIDString = "") {
            string SQL = null;
            int CS = 0;
            string RecordTableName = null;
            string ContentName = null;
            bool HasParentID = false;
            int RecordContentID = 0;
            string RecordContentName = "";
            string DataSourceName = null;
            //
            if (!GenericController.isInDelimitedString(UsedIDString, RecordID.ToString(), ",")) {
                ContentName = getContentNameByID(core, ContentID);
                CS = core.db.csOpenRecord(ContentName, RecordID, false, false);
                if (core.db.csOk(CS)) {
                    HasParentID = core.db.csIsFieldSupported(CS, "ParentID");
                    RecordContentID = core.db.csGetInteger(CS, "ContentControlID");
                    RecordContentName = getContentNameByID(core, RecordContentID);
                }
                core.db.csClose(ref CS);
                if (!string.IsNullOrEmpty(RecordContentName)) {
                    //
                    //
                    //
                    DataSourceName = getContentDataSource(core, RecordContentName);
                    RecordTableName = getContentTablename(core, RecordContentName);
                    //
                    // either Workflow on non-workflow - it changes everything
                    //
                    SQL = "update " + RecordTableName + " set ContentControlID=" + NewContentControlID + " where ID=" + RecordID;
                    core.db.executeQuery(SQL, DataSourceName);
                    if (HasParentID) {
                        SQL = "select contentcontrolid,ID from " + RecordTableName + " where ParentID=" + RecordID;
                        CS = core.db.csOpenSql(SQL, DataSourceName);
                        while (core.db.csOk(CS)) {
                            setContentControlId(core, core.db.csGetInteger(CS, "contentcontrolid"), core.db.csGetInteger(CS, "ID"), NewContentControlID, UsedIDString + "," + RecordID);
                            core.db.csGoNext(CS);
                        }
                        core.db.csClose(ref CS);
                    }
                    //
                    // fix content watch
                    //
                    SQL = "update ccContentWatch set ContentID=" + NewContentControlID + ", ContentRecordKey='" + NewContentControlID + "." + RecordID + "' where ContentID=" + ContentID + " and RecordID=" + RecordID;
                    core.db.executeQuery(SQL);
                }
            }
        }
        //
        // ====================================================================================================
        //
        public static string getContentFieldProperty(CoreController core, string ContentName, string FieldName, string PropertyName) {
            string result = "";
            try {
                CDefDomainModel Contentdefinition = CDefDomainModel.createByUniqueName(core, ContentName);
                if ((string.IsNullOrEmpty(FieldName)) || (Contentdefinition.fields.Count < 1)) {
                    throw (new GenericException("Content Name [" + GenericController.encodeText(ContentName) + "] or FieldName [" + FieldName + "] was not valid"));
                } else {
                    foreach (KeyValuePair<string, Models.Domain.CDefFieldModel> keyValuePair in Contentdefinition.fields) {
                        Models.Domain.CDefFieldModel field = keyValuePair.Value;
                        if (FieldName.ToLowerInvariant() == field.nameLc) {
                            switch (PropertyName.ToUpper()) {
                                case "FIELDTYPE":
                                case "TYPE":
                                    result = field.fieldTypeId.ToString();
                                    break;
                                case "HTMLCONTENT":
                                    result = field.htmlContent.ToString();
                                    break;
                                case "ADMINONLY":
                                    result = field.adminOnly.ToString();
                                    break;
                                case "AUTHORABLE":
                                    result = field.authorable.ToString();
                                    break;
                                case "CAPTION":
                                    result = field.caption;
                                    break;
                                case "REQUIRED":
                                    result = field.required.ToString();
                                    break;
                                case "UNIQUENAME":
                                    result = field.uniqueName.ToString();
                                    break;
                                case "UNIQUE":
                                    //
                                    // fix for the uniquename screwup - it is not unique name, it is unique value
                                    //
                                    result = field.uniqueName.ToString();
                                    break;
                                case "DEFAULT":
                                    result = GenericController.encodeText(field.defaultValue);
                                    break;
                                case "MEMBERSELECTGROUPID":
                                    result = field.memberSelectGroupId_get(core).ToString();
                                    break;
                                default:
                                    break;
                            }
                            break;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //=============================================================
        /// <summary>
        /// Return a record name given the record id. If not record is found, blank is returned.
        /// </summary>
        public string getRecordName(string ContentName, int RecordID) {
            string returnRecordName = "";
            try {

                int CS = csOpenContentRecord(ContentName, RecordID, 0, false, false, "Name");
                if (csOk(CS)) {
                    returnRecordName = csGet(CS, "Name");
                }
                csClose(ref CS);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnRecordName;
        }
        //
        //=============================================================
        /// <summary>
        /// Return a record name given the guid. If not record is found, blank is returned.
        /// </summary>
        public string getRecordName(string contentName, string recordGuid) {
            string returnRecordName = "";
            try {
                CsModel cs = new CsModel(core);
                if (cs.open(contentName, "(ccguid=" + encodeSQLText(recordGuid) + ")")) {
                    returnRecordName = cs.getText("Name");
                }
                cs.close();
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnRecordName;
        }
        //
        //=============================================================
        /// <summary>
        /// get the lowest recordId based on its name. If no record is found, 0 is returned
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordName"></param>
        /// <returns></returns>
        public int getRecordID(string ContentName, string RecordName) {
            int returnValue = 0;
            try {
                if ((!string.IsNullOrEmpty(ContentName.Trim())) && (!string.IsNullOrEmpty(RecordName.Trim()))) {
                    int cs = csOpen(ContentName, "name=" + encodeSQLText(RecordName), "ID", true, 0, false, false, "ID");
                    if (csOk(cs)) {
                        returnValue = csGetInteger(cs, "ID");
                    }
                    csClose(ref cs);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnValue;
        }
        //
        //========================================================================
        /// <summary>
        /// returns true if the cdef field exists
        /// </summary>
        /// <param name="ContentID"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public static bool isCdefField(CoreController core, int ContentID, string FieldName) {
            bool tempisCdefField = false;
            bool returnOk = false;
            try {
                DataTable dt = core.db.executeQuery("Select top 1 id from ccFields where name=" + DbController.encodeSQLText(FieldName) + " And contentid=" + ContentID);
                tempisCdefField = DbController.isDataTableOk(dt);
                dt.Dispose();
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnOk;
        }
        //
        //========================================================================
        /// <summary>
        /// InsertContentRecordGetID
        /// Inserts a record based on a content definition.
        /// Returns the ID of the record, -1 if error
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        ///
        public int insertContentRecordGetID(string ContentName, int MemberID) {
            int result = -1;
            try {
                int CS = csInsertRecord(ContentName, MemberID);
                if (!csOk(CS)) {
                    csClose(ref CS);
                    throw new GenericException("could not insert record in content [" + ContentName + "]");
                } else {
                    result = csGetInteger(CS, "ID");
                }
                csClose(ref CS);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// Delete Content Record
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <param name="MemberID"></param>
        //
        public void deleteContentRecord(string ContentName, int RecordID, int MemberID = SystemMemberID) {
            try {
                if (string.IsNullOrEmpty(ContentName)) {
                    throw new ArgumentException("contentname cannot be blank");
                } else if (RecordID <= 0) {
                    throw new ArgumentException("recordId must be positive value");
                } else {
                    int CSPointer = csOpenContentRecord(ContentName, RecordID, MemberID, true, true);
                    if (csOk(CSPointer)) {
                        csDeleteRecord(CSPointer);
                    }
                    csClose(ref CSPointer);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// 'deleteContentRecords
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="Criteria"></param>
        /// <param name="MemberID"></param>
        //
        public void deleteContentRecords(string ContentName, string Criteria, int MemberID = 0) {
            try {
                //
                int CSPointer = 0;
                Models.Domain.CDefDomainModel CDef = null;
                //
                if (string.IsNullOrEmpty(ContentName.Trim())) {
                    throw new ArgumentException("contentName cannot be blank");
                } else if (string.IsNullOrEmpty(Criteria.Trim())) {
                    throw new ArgumentException("criteria cannot be blank");
                } else {
                    CDef = CDefDomainModel.create(core, ContentName);
                    if (CDef == null) {
                        throw new ArgumentException("ContentName [" + ContentName + "] was not found");
                    } else if (CDef.id == 0) {
                        throw new ArgumentException("ContentName [" + ContentName + "] was not found");
                    } else {
                        //
                        // -- treat all deletes one at a time to invalidate the primary cache
                        // another option is invalidate the entire table (tablename-invalidate), but this also has performance problems
                        //
                        List<string> invaldiateObjectList = new List<string>();
                        CSPointer = csOpen(ContentName, Criteria, "", false, MemberID, true, true);
                        while (csOk(CSPointer)) {
                            invaldiateObjectList.Add(CacheController.createCacheKey_forDbRecord(csGetInteger(CSPointer, "id"), CDef.tableName, CDef.dataSourceName));
                            csDeleteRecord(CSPointer);
                            csGoNext(CSPointer);
                        }
                        csClose(ref CSPointer);
                        core.cache.invalidate(invaldiateObjectList);
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
        /// Encode a value for a sql
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static string encodeSQL(object expression, int fieldType = _fieldTypeIdText) {
            string returnResult = "";
            try {
                switch (fieldType) {
                    case _fieldTypeIdBoolean:
                        returnResult = DbController.encodeSQLBoolean(GenericController.encodeBoolean(expression));
                        break;
                    case _fieldTypeIdCurrency:
                    case _fieldTypeIdFloat:
                        returnResult = DbController.encodeSQLNumber(GenericController.encodeNumber(expression));
                        break;
                    case _fieldTypeIdAutoIdIncrement:
                    case _fieldTypeIdInteger:
                    case _fieldTypeIdLookup:
                    case _fieldTypeIdMemberSelect:
                        returnResult = DbController.encodeSQLNumber(GenericController.encodeInteger(expression));
                        break;
                    case _fieldTypeIdDate:
                        returnResult = DbController.encodeSQLDate(GenericController.encodeDate(expression));
                        break;
                    case _fieldTypeIdLongText:
                    case _fieldTypeIdHTML:
                        returnResult = DbController.encodeSQLText(GenericController.encodeText(expression));
                        break;
                    case _fieldTypeIdFile:
                    case _fieldTypeIdFileImage:
                    case _fieldTypeIdLink:
                    case _fieldTypeIdResourceLink:
                    case _fieldTypeIdRedirect:
                    case _fieldTypeIdManyToMany:
                    case _fieldTypeIdText:
                    case _fieldTypeIdFileText:
                    case _fieldTypeIdFileJavascript:
                    case _fieldTypeIdFileXML:
                    case _fieldTypeIdFileCSS:
                    case _fieldTypeIdFileHTML:
                        returnResult = DbController.encodeSQLText(GenericController.encodeText(expression));
                        break;
                    default:
                        throw new GenericException("Unknown Field Type [" + fieldType + "");
                }
            } catch (Exception) {
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        /// <summary>
        /// Create a filename for the Virtual Directory
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="FieldName"></param>
        /// <param name="RecordID"></param>
        /// <param name="OriginalFilename"></param>
        /// <returns></returns>
        //========================================================================
        //
        public string getVirtualFilename(string ContentName, string FieldName, int RecordID, string OriginalFilename = "") {
            string returnResult = "";
            try {
                int fieldTypeId = 0;
                string TableName = null;
                //Dim iOriginalFilename As String
                Models.Domain.CDefDomainModel CDef = null;
                //
                if (string.IsNullOrEmpty(ContentName.Trim())) {
                    throw new ArgumentException("contentname cannot be blank");
                } else if (string.IsNullOrEmpty(FieldName.Trim())) {
                    throw new ArgumentException("fieldname cannot be blank");
                } else if (RecordID <= 0) {
                    throw new ArgumentException("recordid is not valid");
                } else {
                    CDef = Models.Domain.CDefDomainModel.create(core, ContentName);
                    if (CDef.id == 0) {
                        throw new GenericException("contentname [" + ContentName + "] is not a valid content");
                    } else {
                        TableName = CDef.tableName;
                        if (string.IsNullOrEmpty(TableName)) {
                            TableName = ContentName;
                        }
                        fieldTypeId = CDef.fields[FieldName.ToLowerInvariant()].fieldTypeId;
                        //
                        if (string.IsNullOrEmpty(OriginalFilename)) {
                            returnResult = FileController.getVirtualRecordUnixPathFilename(TableName, FieldName, RecordID, fieldTypeId);
                        } else {
                            returnResult = FileController.getVirtualRecordUnixPathFilename(TableName, FieldName, RecordID, OriginalFilename);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnResult;
        }
        //========================================================================
        /// <summary>
        /// Get a Contents Tableid from the ContentPointer
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        //========================================================================
        //
        public int getContentTableID(string ContentName) {
            int returnResult = 0;
            try {
                DataTable dt = executeQuery("select ContentTableID from ccContent where name=" + encodeSQLText(ContentName));
                if (!DbController.isDataTableOk(dt)) {
                    throw new GenericException("Content [" + ContentName + "] was not found in ccContent table");
                } else {
                    returnResult = GenericController.encodeInteger(dt.Rows[0]["ContentTableID"]);
                }
                dt.Dispose();
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnResult;
        }
        //
        //==================================================================================================
        /// <summary>
        /// Remove this record from all watch lists
        /// </summary>
        /// <param name="ContentID"></param>
        /// <param name="RecordID"></param>
        //
        public void deleteContentRules(int ContentID, int RecordID) {
            try {
                string ContentRecordKey = null;
                string Criteria = null;
                string ContentName = null;
                string TableName = null;
                //
                // ----- remove all ContentWatchListRules (uncheck the watch lists in admin)
                //
                if ((ContentID <= 0) || (RecordID <= 0)) {
                    //
                    throw new GenericException("ContentID [" + ContentID + "] or RecordID [" + RecordID + "] where blank");
                } else {
                    ContentRecordKey = ContentID.ToString() + "." + RecordID.ToString();
                    Criteria = "(ContentRecordKey=" + encodeSQLText(ContentRecordKey) + ")";
                    ContentName = CdefController.getContentNameByID(core, ContentID);
                    TableName = CdefController.getContentTablename(core, ContentName);
                    //
                    // ----- Table Specific rules
                    //
                    switch (GenericController.vbUCase(TableName)) {
                        case "CCCALENDARS":
                            //
                            deleteContentRecords("Calendar Event Rules", "CalendarID=" + RecordID);
                            break;
                        case "CCCALENDAREVENTS":
                            //
                            deleteContentRecords("Calendar Event Rules", "CalendarEventID=" + RecordID);
                            break;
                        case "CCCONTENT":
                            //
                            deleteContentRecords("Group Rules", "ContentID=" + RecordID);
                            break;
                        case "CCCONTENTWATCH":
                            //
                            deleteContentRecords("Content Watch List Rules", "Contentwatchid=" + RecordID);
                            break;
                        case "CCCONTENTWATCHLISTS":
                            //
                            deleteContentRecords("Content Watch List Rules", "Contentwatchlistid=" + RecordID);
                            break;
                        case "CCGROUPS":
                            //
                            deleteContentRecords("Group Rules", "GroupID=" + RecordID);
                            deleteContentRecords("Library Folder Rules", "GroupID=" + RecordID);
                            deleteContentRecords("Member Rules", "GroupID=" + RecordID);
                            deleteContentRecords("Page Content Block Rules", "GroupID=" + RecordID);
                            break;
                        case "CCLIBRARYFOLDERS":
                            //
                            deleteContentRecords("Library Folder Rules", "FolderID=" + RecordID);
                            break;
                        case "CCMEMBERS":
                            //
                            deleteContentRecords("Member Rules", "MemberID=" + RecordID);
                            deleteContentRecords("Topic Habits", "MemberID=" + RecordID);
                            deleteContentRecords("Member Topic Rules", "MemberID=" + RecordID);
                            break;
                        case "CCPAGECONTENT":
                            //
                            deleteContentRecords("Page Content Block Rules", "RecordID=" + RecordID);
                            deleteContentRecords("Page Content Topic Rules", "PageID=" + RecordID);
                            break;
                        case "CCSURVEYQUESTIONS":
                            //
                            deleteContentRecords("Survey Results", "QuestionID=" + RecordID);
                            break;
                        case "CCSURVEYS":
                            //
                            deleteContentRecords("Survey Questions", "SurveyID=" + RecordID);
                            break;
                        case "CCTOPICS":
                            //
                            deleteContentRecords("Topic Habits", "TopicID=" + RecordID);
                            deleteContentRecords("Page Content Topic Rules", "TopicID=" + RecordID);
                            deleteContentRecords("Member Topic Rules", "TopicID=" + RecordID);
                            break;
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
        /// Get FieldDescritor from FieldType
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        //
        public string getFieldTypeNameFromFieldTypeId(int fieldType) {
            string returnFieldTypeName = "";
            try {
                switch (fieldType) {
                    case _fieldTypeIdBoolean:
                        returnFieldTypeName = FieldTypeNameBoolean;
                        break;
                    case _fieldTypeIdCurrency:
                        returnFieldTypeName = FieldTypeNameCurrency;
                        break;
                    case _fieldTypeIdDate:
                        returnFieldTypeName = FieldTypeNameDate;
                        break;
                    case _fieldTypeIdFile:
                        returnFieldTypeName = FieldTypeNameFile;
                        break;
                    case _fieldTypeIdFloat:
                        returnFieldTypeName = FieldTypeNameFloat;
                        break;
                    case _fieldTypeIdFileImage:
                        returnFieldTypeName = FieldTypeNameImage;
                        break;
                    case _fieldTypeIdLink:
                        returnFieldTypeName = FieldTypeNameLink;
                        break;
                    case _fieldTypeIdResourceLink:
                        returnFieldTypeName = FieldTypeNameResourceLink;
                        break;
                    case _fieldTypeIdInteger:
                        returnFieldTypeName = FieldTypeNameInteger;
                        break;
                    case _fieldTypeIdLongText:
                        returnFieldTypeName = FieldTypeNameLongText;
                        break;
                    case _fieldTypeIdLookup:
                        returnFieldTypeName = FieldTypeNameLookup;
                        break;
                    case _fieldTypeIdMemberSelect:
                        returnFieldTypeName = FieldTypeNameMemberSelect;
                        break;
                    case _fieldTypeIdRedirect:
                        returnFieldTypeName = FieldTypeNameRedirect;
                        break;
                    case _fieldTypeIdManyToMany:
                        returnFieldTypeName = FieldTypeNameManyToMany;
                        break;
                    case _fieldTypeIdFileText:
                        returnFieldTypeName = FieldTypeNameTextFile;
                        break;
                    case _fieldTypeIdFileCSS:
                        returnFieldTypeName = FieldTypeNameCSSFile;
                        break;
                    case _fieldTypeIdFileXML:
                        returnFieldTypeName = FieldTypeNameXMLFile;
                        break;
                    case _fieldTypeIdFileJavascript:
                        returnFieldTypeName = FieldTypeNameJavascriptFile;
                        break;
                    case _fieldTypeIdText:
                        returnFieldTypeName = FieldTypeNameText;
                        break;
                    case _fieldTypeIdHTML:
                        returnFieldTypeName = FieldTypeNameHTML;
                        break;
                    case _fieldTypeIdFileHTML:
                        returnFieldTypeName = FieldTypeNameHTMLFile;
                        break;
                    default:
                        if (fieldType == fieldTypeIdAutoIdIncrement) {
                            returnFieldTypeName = "AutoIncrement";
                        } else if (fieldType == fieldTypeIdMemberSelect) {
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
        //=============================================================
        /// <summary>
        /// get a record's id from its guid
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordGuid"></param>
        /// <returns></returns>
        //=============================================================
        //
        public int getRecordIDByGuid(string ContentName, string RecordGuid) {
            int returnResult = 0;
            try {
                if (string.IsNullOrEmpty(ContentName)) {
                    throw new ArgumentException("contentname cannot be blank");
                } else if (string.IsNullOrEmpty(RecordGuid)) {
                    throw new ArgumentException("RecordGuid cannot be blank");
                } else {
                    int CS = csOpen(ContentName, "ccguid=" + encodeSQLText(RecordGuid), "ID", true, 0, false, false, "ID");
                    if (csOk(CS)) {
                        returnResult = csGetInteger(CS, "ID");
                    }
                    csClose(ref CS);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        //
        public string[,] getContentRows(string ContentName, string Criteria = "", string SortFieldList = "", bool ActiveOnly = true, int MemberID = SystemMemberID, bool WorkflowRenderingMode = false, bool WorkflowEditingMode = false, string SelectFieldList = "", int PageSize = 9999, int PageNumber = 1) {
            string[,] returnRows = { { } };
            try {
                //
                int CS = csOpen(ContentName, Criteria, SortFieldList, ActiveOnly, MemberID, WorkflowRenderingMode, WorkflowEditingMode, SelectFieldList, PageSize, PageNumber);
                if (csOk(CS)) {
                    returnRows = contentSetStore[CS].readCache;
                }
                csClose(ref CS);
                //
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnRows;
        }
        //
        //=============================================================================
        /// <summary>
        /// Get a ContentID from the ContentName using just the tables
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        private int getDbContentID(string ContentName) {
            int returnContentId = 0;
            try {
                DataTable dt = executeQuery("Select ID from ccContent where name=" + encodeSQLText(ContentName));
                if (dt.Rows.Count > 0) {
                    returnContentId = GenericController.encodeInteger(dt.Rows[0]["id"]);
                }
                dt.Dispose();
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnContentId;
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
        public void createContentFromSQLTable(DataSourceModel DataSource, string TableName, string ContentName) {
            try {
                //string DateAddedString = DbController.encodeSQLDate(DateTime.Now);
                //string sqlGuid = encodeSQLText( createGuid());
                //string CreateKeyString = DbController.encodeSQLNumber(genericController.GetRandomInteger(core));
                //
                // Read in a record from the table to get fields
                DataTable dt = core.db.openTable(DataSource.name, TableName, "", "", "", 1);
                if (dt.Rows.Count == 0) {
                    dt.Dispose();
                    //
                    // --- no records were found, add a blank if we can
                    //
                    dt = core.db.insertTableRecordGetDataTable(DataSource.name, TableName, core.session.user.id);
                    if (dt.Rows.Count > 0) {
                        int RecordID = GenericController.encodeInteger(dt.Rows[0]["ID"]);
                        core.db.executeQuery("Update " + TableName + " Set active=0 where id=" + RecordID + ";", DataSource.name);
                    }
                }
                string SQL = "";
                int ContentID = 0;
                if (dt.Rows.Count == 0) {
                    throw new GenericException("Could Not add a record To table [" + TableName + "].");
                } else {
                    //
                    //----------------------------------------------------------------
                    // --- Find/Create the Content Definition
                    //----------------------------------------------------------------
                    //
                    ContentID = CDefDomainModel.getContentId(core, ContentName);
                    if (ContentID <= 0) {
                        //
                        // ----- Content definition not found, create it
                        //
                        CdefController.verifyContent_returnId(core, new Models.Domain.CDefDomainModel() {
                            dataSourceName = DataSource.name,
                            tableName = TableName,
                            name = ContentName,
                            active = true
                        });
                        SQL = "Select ID from ccContent where name=" + DbController.encodeSQLText(ContentName);
                        dt = core.db.executeQuery(SQL);
                        if (dt.Rows.Count == 0) {
                            throw new GenericException("Content Definition [" + ContentName + "] could Not be selected by name after it was inserted");
                        } else {
                            ContentID = GenericController.encodeInteger(dt.Rows[0]["ID"]);
                            core.db.executeQuery("update ccContent Set CreateKey=0 where id=" + ContentID);
                        }
                        dt.Dispose();
                        core.cache.invalidateAll();
                        core.clearMetaData();
                    }
                    //
                    //-----------------------------------------------------------
                    // --- Create the ccFields records for the new table
                    //-----------------------------------------------------------
                    //
                    // ----- locate the field in the content field table
                    //
                    SQL = "Select name from ccFields where ContentID=" + ContentID + ";";
                    DataTable dtFields = core.db.executeQuery(SQL);
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
                            // create the content field
                            //
                            createContentFieldFromTableField(ContentName, dcTableColumns.ColumnName, GenericController.encodeInteger(dcTableColumns.DataType));
                        } else {
                            //
                            // touch field so upgrade does not delete it
                            //
                            core.db.executeQuery("update ccFields Set CreateKey=0 where (Contentid=" + ContentID + ") And (name = " + DbController.encodeSQLText(UcaseTableColumnName) + ")");
                        }
                    }
                }
                //
                // Fill ContentControlID fields with new ContentID
                //
                SQL = "Update " + TableName + " Set ContentControlID=" + ContentID + " where (ContentControlID Is null);";
                core.db.executeQuery(SQL, DataSource.name);
                //
                // ----- Load CDef
                //       Load only if the previous state of autoload was true
                //       Leave Autoload false during load so more do not trigger
                //
                core.cache.invalidateAll();
                core.clearMetaData();
                dt.Dispose();
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
        public void createContentFieldFromTableField(string ContentName, string FieldName, int ADOFieldType) {
            try {
                //
                CDefFieldModel field = new CDefFieldModel {
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
                        field.fieldTypeId = fieldTypeIdBoolean;
                        field.defaultValue = "1";
                        break;
                    case "DATEADDED":
                        field.caption = "Created";
                        field.readOnly = true;
                        field.editSortPriority = 5020;
                        break;
                    case "CREATEDBY":
                        field.caption = "Created By";
                        field.fieldTypeId = fieldTypeIdLookup;
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
                        field.fieldTypeId = fieldTypeIdLookup;
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
                        field.fieldTypeId = fieldTypeIdLookup;
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
                        field.fieldTypeId = fieldTypeIdLookup;
                        field.set_lookupContentName(core, "Organizations");
                        field.editSortPriority = 2005;
                        field.authorable = true;
                        field.readOnly = false;
                        break;
                    case "COPYFILENAME":
                        field.caption = "Copy";
                        field.fieldTypeId = fieldTypeIdFileHTML;
                        field.textBuffered = true;
                        field.editSortPriority = 2010;
                        break;
                    case "BRIEFFILENAME":
                        field.caption = "Overview";
                        field.fieldTypeId = fieldTypeIdFileHTML;
                        field.textBuffered = true;
                        field.editSortPriority = 2020;
                        field.htmlContent = false;
                        break;
                    case "IMAGEFILENAME":
                        field.caption = "Image";
                        field.fieldTypeId = fieldTypeIdFile;
                        field.editSortPriority = 2040;
                        break;
                    case "THUMBNAILFILENAME":
                        field.caption = "Thumbnail";
                        field.fieldTypeId = fieldTypeIdFile;
                        field.editSortPriority = 2050;
                        break;
                    case "CONTENTID":
                        field.caption = "Content";
                        field.fieldTypeId = fieldTypeIdLookup;
                        field.set_lookupContentName(core, "Content");
                        field.readOnly = false;
                        field.editSortPriority = 2060;
                        //
                        // --- Record Features
                        //
                        break;
                    case "PARENTID":
                        field.caption = "Parent";
                        field.fieldTypeId = fieldTypeIdLookup;
                        field.set_lookupContentName(core, ContentName);
                        field.readOnly = false;
                        field.editSortPriority = 3000;
                        break;
                    case "MEMBERID":
                        field.caption = "Member";
                        field.fieldTypeId = fieldTypeIdLookup;
                        field.set_lookupContentName(core, "Members");
                        field.readOnly = false;
                        field.editSortPriority = 3005;
                        break;
                    case "CONTACTMEMBERID":
                        field.caption = "Contact";
                        field.fieldTypeId = fieldTypeIdLookup;
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
                CdefController.verifyContentField_returnID(core, ContentName, field);
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
        public string getLinkByContentRecordKey(string ContentRecordKey, string DefaultLink = "") {
            string result = "";
            try {
                int CSPointer = 0;
                string[] KeySplit = null;
                int ContentID = 0;
                int RecordID = 0;
                string ContentName = null;
                int templateId = 0;
                int ParentID = 0;
                string DefaultTemplateLink = null;
                string TableName = null;
                string DataSource = null;
                int ParentContentID = 0;
                bool recordfound = false;
                //
                if (!string.IsNullOrEmpty(ContentRecordKey)) {
                    //
                    // First try main_ContentWatch table for a link
                    //
                    CSPointer = csOpen("Content Watch", "ContentRecordKey=" + encodeSQLText(ContentRecordKey), "", true, 0, false, false, "Link,Clicks");
                    if (csOk(CSPointer)) {
                        result = core.db.csGetText(CSPointer, "Link");
                    }
                    core.db.csClose(ref CSPointer);
                    //
                    if (string.IsNullOrEmpty(result)) {
                        //
                        // try template for this page
                        //
                        KeySplit = ContentRecordKey.Split('.');
                        if (KeySplit.GetUpperBound(0) == 1) {
                            ContentID = GenericController.encodeInteger(KeySplit[0]);
                            if (ContentID != 0) {
                                ContentName = CdefController.getContentNameByID(core, ContentID);
                                RecordID = GenericController.encodeInteger(KeySplit[1]);
                                if (!string.IsNullOrEmpty(ContentName) & RecordID != 0) {
                                    if (CdefController.getContentTablename(core, ContentName) == "ccPageContent") {
                                        CSPointer = core.db.csOpenRecord(ContentName, RecordID, false, false, "TemplateID,ParentID");
                                        if (csOk(CSPointer)) {
                                            recordfound = true;
                                            templateId = csGetInteger(CSPointer, "TemplateID");
                                            ParentID = csGetInteger(CSPointer, "ParentID");
                                        }
                                        csClose(ref CSPointer);
                                        if (!recordfound) {
                                            //
                                            // This content record does not exist - remove any records with this ContentRecordKey pointer
                                            //
                                            deleteContentRecords("Content Watch", "ContentRecordKey=" + encodeSQLText(ContentRecordKey));
                                            core.db.deleteContentRules(CDefDomainModel.getContentId(core, ContentName), RecordID);
                                        } else {

                                            if (templateId != 0) {
                                                CSPointer = core.db.csOpenRecord("Page Templates", templateId, false, false, "Link");
                                                if (csOk(CSPointer)) {
                                                    result = csGetText(CSPointer, "Link");
                                                }
                                                csClose(ref CSPointer);
                                            }
                                            if (string.IsNullOrEmpty(result) && ParentID != 0) {
                                                TableName = CdefController.getContentTablename(core, ContentName);
                                                DataSource = CdefController.getContentDataSource(core, ContentName);
                                                CSPointer = csOpenSql("Select ContentControlID from " + TableName + " where ID=" + RecordID, DataSource);
                                                if (csOk(CSPointer)) {
                                                    ParentContentID = GenericController.encodeInteger(csGetText(CSPointer, "ContentControlID"));
                                                }
                                                csClose(ref CSPointer);
                                                if (ParentContentID != 0) {
                                                    result = getLinkByContentRecordKey(encodeText(ParentContentID + "." + ParentID), "");
                                                }
                                            }
                                            if (string.IsNullOrEmpty(result)) {
                                                DefaultTemplateLink = core.siteProperties.getText("SectionLandingLink", "/" + core.siteProperties.serverPageDefault);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(result)) {
                            result = GenericController.modifyLinkQuery(result, rnPageId, RecordID.ToString(), true);
                        }
                    }
                }
                //
                if (string.IsNullOrEmpty(result)) {
                    result = DefaultLink;
                }
                //
                result = GenericController.encodeVirtualPath(result, core.appConfig.cdnFileUrl, appRootPath, core.webServer.requestDomain);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        // ====================================================================================================
        //
        public int getTableID(string TableName) {
            int result = 0;
            int CS = 0;
            CS = core.db.csOpenSql("Select ID from ccTables where name=" + DbController.encodeSQLText(TableName), "", 1);
            if (core.db.csOk(CS)) {
                result = core.db.csGetInteger(CS, "ID");
            }
            core.db.csClose(ref CS);
            return result;
        }
        //
        //========================================================================
        // main_DeleteChildRecords
        //========================================================================
        //
        public string deleteChildRecords(string ContentName, int RecordID, bool ReturnListWithoutDelete = false) {
            string result = "";
            try {
                bool QuickEditing = false;
                string[] IDs = null;
                int IDCnt = 0;
                int Ptr = 0;
                int CS = 0;
                string ChildList = null;
                bool SingleEntry = false;
                //
                // For now, the child delete only works in non-workflow
                //
                CS = core.db.csOpen(ContentName, "parentid=" + RecordID, "", false, 0, false, false, "ID");
                while (core.db.csOk(CS)) {
                    result += "," + core.db.csGetInteger(CS, "ID");
                    core.db.csGoNext(CS);
                }
                core.db.csClose(ref CS);
                if (!string.IsNullOrEmpty(result)) {
                    result = result.Substring(1);
                    //
                    // main_Get a list of all pages, but do not delete anything yet
                    //
                    IDs = result.Split(',');
                    IDCnt = IDs.GetUpperBound(0) + 1;
                    SingleEntry = (IDCnt == 1);
                    for (Ptr = 0; Ptr < IDCnt; Ptr++) {
                        ChildList = deleteChildRecords(ContentName, GenericController.encodeInteger(IDs[Ptr]), true);
                        if (!string.IsNullOrEmpty(ChildList)) {
                            result += "," + ChildList;
                            SingleEntry = false;
                        }
                    }
                    if (!ReturnListWithoutDelete) {
                        //
                        // Do the actual delete
                        //
                        IDs = result.Split(',');
                        IDCnt = IDs.GetUpperBound(0) + 1;
                        SingleEntry = (IDCnt == 1);
                        QuickEditing = core.session.isQuickEditing(core, "page content");
                        for (Ptr = 0; Ptr < IDCnt; Ptr++) {
                            int deleteRecordId = encodeInteger(IDs[Ptr]);
                            core.db.deleteTableRecord(deleteRecordId, PageContentModel.contentTableName);
                            PageContentModel.invalidateRecordCache(core, deleteRecordId);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
    }
}