
using System;
using System.Collections.Generic;
using System.Data;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using System.Linq;
//
namespace Contensive.Processor.Models.Domain {
    //
    //====================================================================================================
    /// <summary>
    /// content definitions - meta data
    /// </summary>
    [Serializable]
    public class MetaModel : ICloneable {
        //
        //====================================================================================================
        /// <summary>
        /// constructor to setup defaults for fields required
        /// </summary>
        public MetaModel() {
            // set defaults, create methods require name, table
            active = true;
        }
        //
        //====================================================================================================
        /// <summary>
        /// index in content table
        /// </summary>
        public int id { get; set; }
        //
        /// <summary>
        /// Name of Content
        /// </summary>
        public string name { get; set; }
        //
        /// <summary>
        /// the name of the content table
        /// </summary>
        public string tableName { get; set; }
        //
        /// <summary>
        /// The name of the datasource that stores this content (the name of the database connection)
        /// </summary>
        public string dataSourceName { get; set; }
        //
        /// <summary>
        /// Allow adding records
        /// </summary>
        public bool allowAdd { get; set; }
        //
        /// <summary>
        /// Allow deleting records
        /// </summary>
        public bool allowDelete { get; set; }
        //
        /// <summary>
        /// deprecate - filter records from the datasource for this content
        /// </summary>
        public string whereClause { get; set; }
        //
        /// <summary>
        /// name of sort method to use as default for queries against this content
        /// </summary>
        public string defaultSortMethod { get; set; } 
        //
        /// <summary>
        /// 
        /// </summary>
        public bool activeOnly { get; set; }
        //
        /// <summary>
        /// Only allow administrators to modify content
        /// </summary>
        public bool adminOnly { get; set; }
        //
        /// <summary>
        /// Only allow developers to modify content
        /// </summary>
        public bool developerOnly { get; set; }
        //
        /// <summary>
        /// String used to populate select boxes
        /// </summary>
        public string dropDownFieldList { get; set; }
        //
        /// <summary>
        /// Group of members who administer Workflow Authoring
        /// </summary>
        public string editorGroupName { get; set; } 
        //
        /// <summary>
        /// 
        /// </summary>
        public int dataSourceId { get; set; }
        //
        /// <summary>
        /// 
        /// </summary>
        private string _dataSourceName { get; set; } = "";
        //
        /// <summary>
        /// deprecate - Field Name of the required "name" field
        /// </summary>
        public string aliasName { get; set; }
        //
        /// <summary>
        /// deprecate - Field Name of the required "id" field
        /// </summary>
        public string aliasID { get; set; }
        //
        /// <summary>
        /// deprecate - For admin edit page
        /// </summary>
        public bool allowTopicRules { get; set; }
        //
        /// <summary>
        /// deprecate - For admin edit page
        /// </summary>
        public bool allowContentTracking { get; set; }
        //
        /// <summary>
        /// deprecate - For admin edit page
        /// </summary>
        public bool allowCalendarEvents { get; set; } 
        //
        /// <summary>
        /// 
        /// </summary>
        public bool dataChanged { get; set; }
        //
        /// <summary>
        /// deprecate - if any fields().changed, this is set true to
        /// </summary>
        public bool includesAFieldChange { get; set; }
        //
        /// <summary>
        /// when false, the content is not included in queries
        /// </summary>
        public bool active { get; set; }
        //
        /// <summary>
        /// deprecate
        /// </summary>
        public bool allowContentChildTool { get; set; }
        //
        /// <summary>
        /// deprecate
        /// </summary>
        public bool isModifiedSinceInstalled { get; set; }
        //
        /// <summary>
        /// icon for content
        /// </summary>
        public string iconLink { get; set; }
        //
        /// <summary>
        /// icon for content
        /// </summary>
        public int iconWidth { get; set; }
        //
        /// <summary>
        /// icon for content
        /// </summary>
        public int iconHeight { get; set; }
        //
        /// <summary>
        /// icon for content
        /// </summary>
        public int iconSprites { get; set; }
        //
        /// <summary>
        /// 
        /// </summary>
        public string guid { get; set; }
        //
        /// <summary>
        /// deprecate, true if this was installed as part of the base collection. replaced with isntalledByCollectionGuid
        /// </summary>
        public bool isBaseContent { get; set; }
        //
        /// <summary>
        /// the guid of the collection that installed this content
        /// </summary>
        public string installedByCollectionGuid { get; set; }
        //
        /// <summary>
        /// deprecate one day - domain model metadata calculates hasChild from this. If hasChild is false, contentcontrolid is ignored and queries are from the whole table
        /// </summary>
        public int parentID { get; set; }
        //
        /// <summary>
        /// consider deprecation - read from xml, used to set parentId
        /// </summary>
        public string parentName { get; set; }
        //
        /// <summary>
        /// string that changes if any record in Content Definition changes, in memory only
        /// </summary>
        public string timeStamp { get; set; } 
        //
        /// <summary>
        /// field for this content
        /// </summary>
        public Dictionary<string, Models.Domain.MetaFieldModel> fields { get; set; } = new Dictionary<string, Models.Domain.MetaFieldModel>();
        //
        /// <summary>
        /// metadata for admin site editing columns
        /// !!!!! changed to string because dotnet json cannot serialize an integer key
        /// </summary>
        public SortedList<string, MetaAdminColumnClass> adminColumns { get; set; } = new SortedList<string, MetaAdminColumnClass>();
        //
        /// <summary>
        /// consider deprection - string created from ParentIDs used to select records. If we eliminate parentId, then the whole table belongs to the content. This will speed queries and simplify concepts
        /// </summary>
        public string legacyContentControlCriteria { get; set; }
        //
        /// <summary>
        /// 
        /// </summary>
        public List<string> selectList { get; set; } = new List<string>();
        //
        /// <summary>
        /// Field list used in OpenCSContent calls (all active field definitions)
        /// </summary>
        public string selectCommaList { get; set; } 
        //
        //====================================================================================================
        /// <summary>
        /// consider deprecating - list of child content definitions. Not needed if we deprecate parentid
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public List<int> childIdList(CoreController core) {
            if ( _childIdList == null ) {
                _childIdList = new List<int>();
                using (DataTable dt = core.db.executeQuery("select id from cccontent where parentid=" + id)) {
                    foreach (DataRow row in dt.Rows) { _childIdList.Add(encodeInteger(row[0])); }
                }
            }
            return _childIdList;
        }
        private List<int> _childIdList = null;
        //
        //====================================================================================================
        /// <summary>
        /// metadata for column definition
        /// </summary>
        //
        [Serializable]
        public class MetaAdminColumnClass {
            public string Name;
            //Public FieldPointer As Integer
            public int Width;
            public int SortPriority;
            public int SortDirection;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Create metadata object from cache or database for provided contentId
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentId"></param>
        /// <param name="loadInvalidFields"></param>
        /// <param name="forceDbLoad"></param>
        /// <returns></returns>
        public static MetaModel create(CoreController core, ContentModel content, bool loadInvalidFields, bool forceDbLoad) {
            MetaModel result = null;
            try {
                if ( content == null ) { return null; }
                if ((!forceDbLoad) && (core.metaDataDictionary.ContainsKey(content.id.ToString()))) { return core.metaDataDictionary[content.id.ToString()]; }
                if (core.metaDataDictionary.ContainsKey(content.id.ToString())) {
                    //
                    // -- key is already there, remove it first                        
                    core.metaDataDictionary.Remove(content.id.ToString());
                }
                List<string> dependentCacheNameList = new List<string>();
                if (!forceDbLoad) {
                    result = getCache(core, content.id);
                }
                if (result == null) {
                    //
                    // load Db version
                    //
                    string sql = "SELECT "
                        + "c.ID"
                        + ", c.Name"
                        + ", c.name"
                        + ", c.AllowAdd"
                        + ", c.DeveloperOnly"
                        + ", c.AdminOnly"
                        + ", c.AllowDelete"
                        + ", c.ParentID"
                        + ", c.DefaultSortMethodID"
                        + ", c.DropDownFieldList"
                        + ", ContentTable.Name AS ContentTableName"
                        + ", ContentDataSource.Name AS ContentDataSourceName"
                        + ", '' AS AuthoringTableName"
                        + ", '' AS AuthoringDataSourceName"
                        + ", 0 AS AllowWorkflowAuthoring"
                        + ", c.AllowCalendarEvents as AllowCalendarEvents"
                        + ", ContentTable.DataSourceID"
                        + ", ccSortMethods.OrderByClause as DefaultSortMethod"
                        + ", ccGroups.Name as EditorGroupName"
                        + ", c.AllowContentTracking as AllowContentTracking"
                        + ", c.AllowTopicRules as AllowTopicRules";
                    //
                    sql += ""
                        + " from (((((ccContent c"
                        + " left join ccTables AS ContentTable ON c.ContentTableID = ContentTable.ID)"
                        + " left join ccTables AS AuthoringTable ON c.AuthoringTableID = AuthoringTable.ID)"
                        + " left join ccDataSources AS ContentDataSource ON ContentTable.DataSourceID = ContentDataSource.ID)"
                        + " left join ccDataSources AS AuthoringDataSource ON AuthoringTable.DataSourceID = AuthoringDataSource.ID)"
                        + " left join ccSortMethods ON c.DefaultSortMethodID = ccSortMethods.ID)"
                        + " left join ccGroups ON c.EditorGroupID = ccGroups.ID"
                        + " where (c.Active<>0)"
                        + " and(c.id=" + content.id.ToString() + ")";
                    using (DataTable dtContent = core.db.executeQuery(sql)) {
                        if (dtContent.Rows.Count == 0) {
                            //
                            // metadata not found
                            //
                        } else {
                            result = new Models.Domain.MetaModel();
                            result.fields = new Dictionary<string, Models.Domain.MetaFieldModel>();
                            result.selectList = new List<string>();
                            result.adminColumns = new SortedList<string, MetaAdminColumnClass>();
                            //
                            // ----- save values in definition
                            //
                            string contentName = null;
                            DataRow contentRow = dtContent.Rows[0];
                            contentName = encodeText(GenericController.encodeText(contentRow[1])).Trim(' ');
                            string contentTablename = GenericController.encodeText(contentRow[10]);
                            result.name = contentName;
                            result.id = content.id;
                            result.allowAdd = GenericController.encodeBoolean(contentRow[3]);
                            result.developerOnly = GenericController.encodeBoolean(contentRow[4]);
                            result.adminOnly = GenericController.encodeBoolean(contentRow[5]);
                            result.allowDelete = GenericController.encodeBoolean(contentRow[6]);
                            result.parentID = GenericController.encodeInteger(contentRow[7]);
                            result.dropDownFieldList = GenericController.vbUCase(GenericController.encodeText(contentRow[9]));
                            result.tableName = GenericController.encodeText(contentTablename);
                            result.dataSourceName = "default";
                            result.allowCalendarEvents = GenericController.encodeBoolean(contentRow[15]);
                            result.defaultSortMethod = GenericController.encodeText(contentRow[17]);
                            if (string.IsNullOrEmpty(result.defaultSortMethod)) {
                                result.defaultSortMethod = "name";
                            }
                            result.editorGroupName = GenericController.encodeText(contentRow[18]);
                            result.allowContentTracking = GenericController.encodeBoolean(contentRow[19]);
                            result.allowTopicRules = GenericController.encodeBoolean(contentRow[20]);
                            // .AllowMetaContent = genericController.EncodeBoolean(row[21])
                            //
                            result.activeOnly = true;
                            result.aliasID = "ID";
                            result.aliasName = "NAME";
                            //
                            // load parent metadata fields first so we can overlay the current metadata field
                            //
                            if (result.parentID == 0) {
                                result.parentID = -1;
                            } else {
                                Models.Domain.MetaModel parentMetaData = create(core, result.parentID, loadInvalidFields, forceDbLoad);
                                foreach (var keyvaluepair in parentMetaData.fields) {
                                    Models.Domain.MetaFieldModel parentField = keyvaluepair.Value;
                                    Models.Domain.MetaFieldModel childField = new Models.Domain.MetaFieldModel();
                                    childField = (Models.Domain.MetaFieldModel)parentField.Clone();
                                    childField.inherited = true;
                                    result.fields.Add(childField.nameLc.ToLowerInvariant(), childField);
                                    if (!((parentField.fieldTypeId == fieldTypeIdManyToMany) || (parentField.fieldTypeId == fieldTypeIdRedirect))) {
                                        if (!result.selectList.Contains(parentField.nameLc)) {
                                            result.selectList.Add(parentField.nameLc);
                                        }
                                    }
                                }
                            }
                            //
                            // ----- now load all the Content Definition Fields
                            //
                            sql = "SELECT"
                                + " f.DeveloperOnly"
                                + ",f.UniqueName"
                                + ",f.TextBuffered"
                                + ",f.Password"
                                + ",f.IndexColumn"
                                + ",f.IndexWidth"
                                + ",f.IndexSortPriority"
                                + ",f.IndexSortDirection"
                                + ",f.AdminOnly"
                                + ",f.SortOrder"
                                + ",f.EditSortPriority"
                                + ",f.ContentID"
                                + ",f.ID"
                                + ",f.Name"
                                + ",f.Required"
                                + ",f.Type"
                                + ",f.Caption"
                                + ",f.readonly"
                                + ",f.LookupContentID"
                                + ",f.RedirectContentID"
                                + ",f.RedirectPath"
                                + ",f.RedirectID"
                                + ",f.DefaultValue"
                                + ",'' as HelpMessageDeprecated"
                                + ",f.Active"
                                + ",f.HTMLContent"
                                + ",f.NotEditable"
                                + ",f.authorable"
                                + ",f.ManyToManyContentID"
                                + ",f.ManyToManyRuleContentID"
                                + ",f.ManyToManyRulePrimaryField"
                                + ",f.ManyToManyRuleSecondaryField"
                                + ",f.RSSTitleField"
                                + ",f.RSSDescriptionField"
                                + ",f.EditTab"
                                + ",f.Scramble"
                                + ",f.MemberSelectGroupID"
                                + ",f.LookupList"
                                + ",f.IsBaseField"
                                + ",f.InstalledByCollectionID"
                                + ",h.helpDefault"
                                + ",h.helpCustom"
                                + ""
                                + " from ((ccFields f"
                                + " left join ccContent c ON f.ContentID = c.ID)"
                                + " left join ccfieldHelp h on h.fieldid=f.id)"
                                + ""
                                + " where"
                                + " (c.ID Is not Null)"
                                + " and(c.Active<>0)"
                                + " and(c.ID=" + content.id + ")"
                                + ""
                                + "";
                            //
                            if (!loadInvalidFields) {
                                sql += ""
                                        + " and(f.active<>0)"
                                        + " and(f.Type<>0)"
                                        + " and(f.name <>'')"
                                        + "";
                            }
                            sql += ""
                                    + " order by"
                                    + " f.ContentID,f.EditTab,f.EditSortPriority"
                                    + "";
                            using (var dtFields = core.db.executeQuery(sql)) {
                                if (dtFields.Rows.Count == 0) {
                                    //
                                } else {
                                    List<string> usedFields = new List<string>();
                                    foreach (DataRow fieldRow in dtFields.Rows) {
                                        string fieldName = GenericController.encodeText(fieldRow[13]);
                                        int fieldId = GenericController.encodeInteger(fieldRow[12]);
                                        string fieldNameLower = fieldName.ToLowerInvariant();
                                        bool skipDuplicateField = false;
                                        if (usedFields.Contains(fieldNameLower)) {
                                            //
                                            // this is a dup field for this content (not accounting for possibleinherited field) - keep the one with the lowest id
                                            //
                                            if (result.fields[fieldNameLower].id < fieldId) {
                                                //
                                                // this new field has a higher id, skip it
                                                //
                                                skipDuplicateField = true;
                                            } else {
                                                //
                                                // this new field has a lower id, remove the other one
                                                //
                                                result.fields.Remove(fieldNameLower);
                                            }
                                        }
                                        if (!skipDuplicateField) {
                                            //
                                            // only add the first field found, ordered by id
                                            //
                                            if (result.fields.ContainsKey(fieldNameLower)) {
                                                //
                                                // remove inherited field and replace it with field from this table
                                                //
                                                result.fields.Remove(fieldNameLower);
                                            }
                                            Models.Domain.MetaFieldModel field = new Models.Domain.MetaFieldModel();
                                            int fieldIndexColumn = -1;
                                            int fieldTypeId = GenericController.encodeInteger(fieldRow[15]);
                                            if (GenericController.encodeText(fieldRow[4]) != "") {
                                                fieldIndexColumn = GenericController.encodeInteger(fieldRow[4]);
                                            }
                                            //
                                            // translate htmlContent to fieldtypehtml
                                            //   this is also converted in upgrade, daily housekeep, addon install
                                            //
                                            bool fieldHtmlContent = GenericController.encodeBoolean(fieldRow[25]);
                                            if (fieldHtmlContent) {
                                                if (fieldTypeId == fieldTypeIdLongText) {
                                                    fieldTypeId = fieldTypeIdHTML;
                                                } else if (fieldTypeId == fieldTypeIdFileText) {
                                                    fieldTypeId = fieldTypeIdFileHTML;
                                                }
                                            }
                                            field.active = GenericController.encodeBoolean(fieldRow[24]);
                                            field.adminOnly = GenericController.encodeBoolean(fieldRow[8]);
                                            field.authorable = GenericController.encodeBoolean(fieldRow[27]);
                                            field.blockAccess = GenericController.encodeBoolean(fieldRow[38]);
                                            field.caption = GenericController.encodeText(fieldRow[16]);
                                            field.dataChanged = false;
                                            //.Changed
                                            field.contentId = content.id;
                                            field.defaultValue = GenericController.encodeText(fieldRow[22]);
                                            field.developerOnly = GenericController.encodeBoolean(fieldRow[0]);
                                            field.editSortPriority = GenericController.encodeInteger(fieldRow[10]);
                                            field.editTabName = GenericController.encodeText(fieldRow[34]);
                                            field.fieldTypeId = fieldTypeId;
                                            field.htmlContent = fieldHtmlContent;
                                            field.id = fieldId;
                                            field.indexColumn = fieldIndexColumn;
                                            field.indexSortDirection = GenericController.encodeInteger(fieldRow[7]);
                                            field.indexSortOrder = GenericController.encodeInteger(fieldRow[6]);
                                            field.indexWidth = GenericController.encodeText(GenericController.encodeInteger(GenericController.encodeText(fieldRow[5]).Replace("%", "")));
                                            field.inherited = false;
                                            field.installedByCollectionGuid = GenericController.encodeText(fieldRow[39]);
                                            field.isBaseField = GenericController.encodeBoolean(fieldRow[38]);
                                            field.isModifiedSinceInstalled = false;
                                            field.lookupContentID = GenericController.encodeInteger(fieldRow[18]);
                                            //.lookupContentName = ""
                                            field.lookupList = GenericController.encodeText(fieldRow[37]);
                                            field.manyToManyContentID = GenericController.encodeInteger(fieldRow[28]);
                                            field.manyToManyRuleContentID = GenericController.encodeInteger(fieldRow[29]);
                                            field.ManyToManyRulePrimaryField = GenericController.encodeText(fieldRow[30]);
                                            field.ManyToManyRuleSecondaryField = GenericController.encodeText(fieldRow[31]);
                                            field.memberSelectGroupId_set(core, GenericController.encodeInteger(fieldRow[36]));
                                            field.nameLc = fieldNameLower;
                                            field.notEditable = GenericController.encodeBoolean(fieldRow[26]);
                                            field.password = GenericController.encodeBoolean(fieldRow[3]);
                                            field.readOnly = GenericController.encodeBoolean(fieldRow[17]);
                                            field.redirectContentID = GenericController.encodeInteger(fieldRow[19]);
                                            //.RedirectContentName(core) = ""
                                            field.redirectID = GenericController.encodeText(fieldRow[21]);
                                            field.redirectPath = GenericController.encodeText(fieldRow[20]);
                                            field.required = GenericController.encodeBoolean(fieldRow[14]);
                                            field.RSSTitleField = GenericController.encodeBoolean(fieldRow[32]);
                                            field.RSSDescriptionField = GenericController.encodeBoolean(fieldRow[33]);
                                            field.Scramble = GenericController.encodeBoolean(fieldRow[35]);
                                            field.textBuffered = GenericController.encodeBoolean(fieldRow[2]);
                                            field.uniqueName = GenericController.encodeBoolean(fieldRow[1]);
                                            //.ValueVariant
                                            //
                                            field.helpCustom = GenericController.encodeText(fieldRow[41]);
                                            field.helpDefault = GenericController.encodeText(fieldRow[40]);
                                            if (string.IsNullOrEmpty(field.helpCustom)) {
                                                field.helpMessage = field.helpDefault;
                                            } else {
                                                field.helpMessage = field.helpCustom;
                                            }
                                            field.HelpChanged = false;
                                            result.fields.Add(fieldNameLower, field);
                                            //REFACTOR
                                            if ((field.fieldTypeId != fieldTypeIdManyToMany) && (field.fieldTypeId != fieldTypeIdRedirect) && (!result.selectList.Contains(fieldNameLower))) {
                                                //
                                                // add only fields that can be selected
                                                result.selectList.Add(fieldNameLower);
                                            }
                                        }
                                    }
                                    result.selectCommaList = string.Join(",", result.selectList);
                                }
                            }
                            //
                            // ----- Create the LegacyContentControlCriteria. For compatibility, if support=false, return (1=1)
                            result.legacyContentControlCriteria = (result.parentID <= 0) ? "(1=1)" : getLegacyContentControlCriteria(core, result.id, result.tableName, result.dataSourceName, new List<int>());
                            //
                            create_setAdminColumns(core, result);
                        }
                    }
                    setCache(core, content.id, result);
                }
                core.metaDataDictionary.Add(content.id.ToString(), result);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        public static MetaModel create(CoreController core, ContentModel content, bool loadInvalidFields) => create(core, content, loadInvalidFields, false);
        //
        public static MetaModel create(CoreController core, ContentModel content) => create(core, content, false, false);
        //
        //====================================================================================================
        /// <summary>
        /// Create metadata object from cache or database for provided contentId
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentGuid"></param>
        /// <param name="loadInvalidFields"></param>
        /// <param name="forceDbLoad"></param>
        /// <returns></returns>
        public static MetaModel create(CoreController core, string contentGuid, bool loadInvalidFields, bool forceDbLoad) {
            var content = ContentModel.create(core, contentGuid);
            if (content == null) { return null; }
            return create(core, content, loadInvalidFields, forceDbLoad);
        }
        //
        public static MetaModel create(CoreController core, string contentGuid, bool loadInvalidFields) => create(core, contentGuid, loadInvalidFields, false );
        //
        public static MetaModel create(CoreController core, string contentGuid) => create(core, contentGuid, false, false );
        //
        //====================================================================================================
        /// <summary>
        /// Create metadata object from cache or database for provided contentId
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentId"></param>
        /// <param name="loadInvalidFields"></param>
        /// <param name="forceDbLoad"></param>
        /// <returns></returns>
        public static MetaModel create(CoreController core, int contentId, bool loadInvalidFields, bool forceDbLoad) {
            var content = ContentModel.create(core, contentId);
            if (content == null) { return null; }
            return create(core, content, loadInvalidFields, forceDbLoad);
        }
        //
        public static MetaModel create(CoreController core, int contentId, bool loadInvalidFields) => create(core, contentId, loadInvalidFields, false);
        //
        public static MetaModel create(CoreController core, int contentId) => create(core, contentId, false, false);
        //   
        //====================================================================================================
        /// <summary>
        /// get metadata from content name. If the metadata is not found, return nothing.
        /// </summary>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public static MetaModel createByUniqueName(CoreController core, string contentName, bool loadInvalidFields, bool forceDbLoad) {
            var content = ContentModel.createByUniqueName(core, contentName);
            if (content == null) { return null; }
            return create(core, content, loadInvalidFields, forceDbLoad);
        }
        //
        public static MetaModel createByUniqueName(CoreController core, string contentName, bool loadInvalidFields) => createByUniqueName(core, contentName, loadInvalidFields, false);
        //
        public static MetaModel createByUniqueName(CoreController core, string contentName) => createByUniqueName(core, contentName, false, false);
        //
        //========================================================================
        /// <summary>
        /// Calculates the query criteria for a content with parentId set non-zero. DO NOT CALL if parentId is not 0.
        /// Dig into Content Definition Records and create an SQL Criteria statement for parent-child relationships.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentId"></param>
        /// <param name="contentTableName"></param>
        /// <param name="contentDAtaSourceName"></param>
        /// <param name="parentIdList"></param>
        /// <returns></returns>
        private static string getLegacyContentControlCriteria(CoreController core, int contentId, string contentTableName, string contentDAtaSourceName, List<int> parentIdList) {
            try {
                string returnCriteria = "(1=0)";
                if (contentId >= 0) {
                    if (!parentIdList.Contains(contentId)) {
                        returnCriteria = "";
                        //
                        // -- first contentid in list, include contentid 0
                        if (parentIdList.Count == 0) returnCriteria += "(" + contentTableName + ".contentcontrolId=0)or";
                        parentIdList.Add(contentId);
                        //
                        // -- add this content id to the list
                        returnCriteria += "(" + contentTableName + ".contentcontrolId=" + contentId + ")";
                        foreach (var childContent in ContentModel.createList(core, "(parentid=" + contentId + ")")) {
                            returnCriteria += "or" + getLegacyContentControlCriteria(core, childContent.id, contentTableName, contentDAtaSourceName, parentIdList);
                        }
                        parentIdList.Remove(contentId);
                        returnCriteria = "(" + returnCriteria + ")";
                    }
                }
                return returnCriteria;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// setup the admin column model for the create methods
        /// </summary>
        /// <param name="core"></param>
        /// <param name="metaData"></param>
        private static void create_setAdminColumns(CoreController core, MetaModel metaData) {
            try {
                if (metaData.id > 0) {
                    int cnt = 0;
                    int FieldWidthTotal = 0;
                    MetaAdminColumnClass adminColumn = null;
                    foreach (KeyValuePair<string, Models.Domain.MetaFieldModel> keyValuePair in metaData.fields) {
                        MetaFieldModel field = keyValuePair.Value;
                        bool FieldActive = field.active;
                        int FieldWidth = GenericController.encodeInteger(field.indexWidth);
                        if (FieldActive && (FieldWidth > 0)) {
                            FieldWidthTotal = FieldWidthTotal + FieldWidth;
                            adminColumn = new MetaAdminColumnClass();
                            adminColumn.Name = field.nameLc;
                            adminColumn.SortDirection = field.indexSortDirection;
                            adminColumn.SortPriority = GenericController.encodeInteger(field.indexSortOrder);
                            adminColumn.Width = FieldWidth;
                            FieldWidthTotal = FieldWidthTotal + adminColumn.Width;
                            string key = (cnt + (adminColumn.SortPriority * 1000)).ToString().PadLeft(6, '0');
                            metaData.adminColumns.Add(key, adminColumn);
                        }
                        cnt += 1;
                    }
                    //
                    // Force the Name field as the only column
                    if (metaData.fields.Count > 0) {
                        if (metaData.adminColumns.Count == 0) {
                            //
                            // Force the Name field as the only column
                            //
                            if (metaData.fields.ContainsKey("name")) {
                                adminColumn = new MetaAdminColumnClass();
                                adminColumn.Name = "Name";
                                adminColumn.SortDirection = 1;
                                adminColumn.SortPriority = 1;
                                adminColumn.Width = 100;
                                FieldWidthTotal = FieldWidthTotal + adminColumn.Width;
                                string key = ((1000)).ToString().PadLeft(6, '0');
                                metaData.adminColumns.Add(key, adminColumn);
                            }
                        }
                        //
                        // Normalize the column widths
                        //
                        foreach (var keyvaluepair in metaData.adminColumns) {
                            adminColumn = keyvaluepair.Value;
                            adminColumn.Width = encodeInteger(100 * ((double)adminColumn.Width / (double)FieldWidthTotal));
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Get cache key for a metadata object in cache
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public static string getCacheKey(int contentId) {
            return CacheController.createCacheKey_forObject("metadata", contentId.ToString());
        }
        //
        //====================================================================================================
        /// <summary>
        /// invalidate the metadata record for a given contentid
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentId"></param>
        public static void invalidateCache(CoreController core, int contentId) {
            core.cache.invalidate(getCacheKey(contentId));
        }
        //
        //====================================================================================================
        /// <summary>
        /// update cache for a metadata model
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentId"></param>
        /// <param name="metaData"></param>
        public static void setCache(CoreController core, int contentId, MetaModel metaData) {
            List<string> dependantList = new List<string>();
            core.cache.storeObject(getCacheKey(contentId), metaData, dependantList);
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a cache version of a metadata model
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public static MetaModel getCache(CoreController core, int contentId) {
            MetaModel result = null;
            try {
                try {
                    result = core.cache.getObject<Models.Domain.MetaModel>(getCacheKey(contentId));
                } catch (Exception ex) {
                    LogController.handleError(core, ex);
                }
            } catch (Exception) { }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get content id from content name. In model not controller because controller calls model, not the other wau (see constants)
        /// </summary>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public static int getContentId(CoreController core, string contentName) {
            try {
                var nameLower = contentName.Trim().ToLowerInvariant();
                if (core.contentNameIdDictionary.ContainsKey(nameLower)) { return core.contentNameIdDictionary[nameLower]; }
                ContentModel content = ContentModel.createByUniqueName(core, contentName);
                if (content != null) {
                    core.contentNameIdDictionary.Add(nameLower, content.id);
                    return content.id;
                }
                return 0;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the meta field object specified in the fieldname. If it does not exist, return null
        /// </summary>
        /// <param name="core"></param>
        /// <param name="meta"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static MetaFieldModel getField( CoreController core, MetaModel meta, string fieldName  ) {
            if (meta == null) return null;
            if (!meta.fields.ContainsKey(fieldName.ToLower())) return null;
            return meta.fields[fieldName.ToLower()];
        }
        //
        //====================================================================================================
        /// <summary>
        /// create a clone of this object. Used for cases like cs copy
        /// </summary>
        /// <returns></returns>
        public Object Clone() {
            return MemberwiseClone();
        }
    }
}
