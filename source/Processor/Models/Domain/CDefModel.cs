
using System;
using System.Collections.Generic;
using System.Data;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
using System.Linq;
//
namespace Contensive.Processor.Models.Domain {
    //
    //====================================================================================================
    /// <summary>
    /// content definitions - meta data
    /// </summary>
    [Serializable]
    public class CDefModel {
        //
        // -- constructor to setup defaults for fields required
        public CDefModel() {
            // set defaults, create methods require name, table
            active = true;
        }
        //
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
        /// deprecate one day - domain model cdef calculates hasChild from this. If hasChild is false, contentcontrolid is ignored and queries are from the whole table
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
        public Dictionary<string, Models.Domain.CDefFieldModel> fields { get; set; } = new Dictionary<string, Models.Domain.CDefFieldModel>();
        //
        /// <summary>
        /// metadata for admin site editing columns
        /// </summary>
        public SortedList<string, CDefAdminColumnClass> adminColumns { get; set; } = new SortedList<string, CDefAdminColumnClass>();
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
        public List<int> get_childIdList(CoreController core) {
            _childIdList = new List<int>();
            if (( parentID>0)&(_childIdList == null)) {
                string Sql = "select id from cccontent where parentid=" + id;
                using (DataTable dt = core.db.executeQuery(Sql)) {
                    if (dt.Rows.Count == 0) {
                        foreach (DataRow row in dt.Rows) {
                            _childIdList.Add(GenericController.encodeInteger(row[0]));
                        }
                    }
                }
            }
            return _childIdList;
        }
        public void set_childIdList(CoreController core, List<int> value) {
            _childIdList = value;
        }
        private List<int> _childIdList = null;
        //
        //====================================================================================================
        /// <summary>
        /// metadata for column definition
        /// </summary>
        //
        [Serializable]
        public class CDefAdminColumnClass {
            public string Name;
            //Public FieldPointer As Integer
            public int Width;
            public int SortPriority;
            public int SortDirection;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Create cdef object from cache or database for provided contentId
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentId"></param>
        /// <param name="loadInvalidFields"></param>
        /// <param name="forceDbLoad"></param>
        /// <returns></returns>
        public static CDefModel create(CoreController core, int contentId, bool loadInvalidFields = false, bool forceDbLoad = false) {
            CDefModel result = null;
            try {
                if (contentId <= 0) {
                    //
                    // -- invalid id, return null
                } else if ((!forceDbLoad) && (core.doc.cdefDictionary.ContainsKey(contentId.ToString()))) {
                    //
                    // -- already loaded and no force re-load, just return the current cdef                    
                    result = core.doc.cdefDictionary[contentId.ToString()];
                } else {
                    if (core.doc.cdefDictionary.ContainsKey(contentId.ToString())) {
                        //
                        // -- key is already there, remove it first                        
                        core.doc.cdefDictionary.Remove(contentId.ToString());
                    }

                    List<string> dependantCacheNameList = new List<string>();
                    if (!forceDbLoad) {
                        result = getCache(core, contentId);
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
                            + " and(c.id=" + contentId.ToString() + ")";
                        using (DataTable dtContent = core.db.executeQuery(sql)) {
                            if (dtContent.Rows.Count == 0) {
                                //
                                // cdef not found
                                //
                            } else {
                                result = new Models.Domain.CDefModel();
                                result.fields = new Dictionary<string, Models.Domain.CDefFieldModel>();
                                result.set_childIdList(core, new List<int>());
                                result.selectList = new List<string>();
                                // -- !!!!! changed to string because dotnet json cannot serialize an integer key
                                result.adminColumns = new SortedList<string, CDefAdminColumnClass>();
                                //
                                // ----- save values in definition
                                //
                                string contentName = null;
                                DataRow contentRow = dtContent.Rows[0];
                                contentName = encodeText(GenericController.encodeText(contentRow[1])).Trim(' ');
                                string contentTablename = GenericController.encodeText(contentRow[10]);
                                result.name = contentName;
                                result.id = contentId;
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
                                // load parent cdef fields first so we can overlay the current cdef field
                                //
                                if (result.parentID == 0) {
                                    result.parentID = -1;
                                } else {
                                    Models.Domain.CDefModel parentCdef = create(core, result.parentID, loadInvalidFields, forceDbLoad);
                                    foreach (var keyvaluepair in parentCdef.fields) {
                                        Models.Domain.CDefFieldModel parentField = keyvaluepair.Value;
                                        Models.Domain.CDefFieldModel childField = new Models.Domain.CDefFieldModel();
                                        childField = (Models.Domain.CDefFieldModel)parentField.Clone();
                                        childField.inherited = true;
                                        result.fields.Add(childField.nameLc.ToLower(), childField);
                                        if (!((parentField.fieldTypeId == FieldTypeIdManyToMany) || (parentField.fieldTypeId == FieldTypeIdRedirect))) {
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
                                    + " and(c.ID=" + contentId + ")"
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
                                            string fieldNameLower = fieldName.ToLower();
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
                                                Models.Domain.CDefFieldModel field = new Models.Domain.CDefFieldModel();
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
                                                    if (fieldTypeId == FieldTypeIdLongText) {
                                                        fieldTypeId = FieldTypeIdHTML;
                                                    } else if (fieldTypeId == FieldTypeIdFileText) {
                                                        fieldTypeId = FieldTypeIdFileHTML;
                                                    }
                                                }
                                                field.active = GenericController.encodeBoolean(fieldRow[24]);
                                                field.adminOnly = GenericController.encodeBoolean(fieldRow[8]);
                                                field.authorable = GenericController.encodeBoolean(fieldRow[27]);
                                                field.blockAccess = GenericController.encodeBoolean(fieldRow[38]);
                                                field.caption = GenericController.encodeText(fieldRow[16]);
                                                field.dataChanged = false;
                                                //.Changed
                                                field.contentId = contentId;
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
                                                if ((field.fieldTypeId != FieldTypeIdManyToMany) & (field.fieldTypeId != FieldTypeIdRedirect) && (!result.selectList.Contains(fieldNameLower))) {
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
                        setCache(core, contentId, result);
                    }
                    core.doc.cdefDictionary.Add(contentId.ToString(), result);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
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
            string returnCriteria = "";
            try {
                //
                {
                    returnCriteria = "(1=0)";
                    if (contentId >= 0) {
                        if (!parentIdList.Contains(contentId)) {
                            parentIdList.Add(contentId);
                            returnCriteria = "(" + contentTableName + ".contentcontrolId=" + contentId + ")";
                            foreach (var childContent in ContentModel.createList(core, "(parentid=" + contentId + ")")) {
                                returnCriteria += "OR" + getLegacyContentControlCriteria(core, childContent.id, contentTableName, contentDAtaSourceName, parentIdList);
                            }
                            parentIdList.Remove(contentId);
                            returnCriteria = "(" + returnCriteria + ")";
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnCriteria;
        }
        //
        //====================================================================================================
        //
        private static void create_setAdminColumns(CoreController core, Models.Domain.CDefModel cdef) {
            try {
                bool FieldActive = false;
                int FieldWidth = 0;
                int FieldWidthTotal = 0;
                CDefAdminColumnClass adminColumn = null;
                //
                if (cdef.id > 0) {
                    int cnt = 0;
                    foreach (KeyValuePair<string, Models.Domain.CDefFieldModel> keyValuePair in cdef.fields) {
                        CDefFieldModel field = keyValuePair.Value;
                        FieldActive = field.active;
                        FieldWidth = GenericController.encodeInteger(field.indexWidth);
                        if (FieldActive && (FieldWidth > 0)) {
                            FieldWidthTotal = FieldWidthTotal + FieldWidth;
                            adminColumn = new CDefAdminColumnClass();
                            adminColumn.Name = field.nameLc;
                            adminColumn.SortDirection = field.indexSortDirection;
                            adminColumn.SortPriority = GenericController.encodeInteger(field.indexSortOrder);
                            adminColumn.Width = FieldWidth;
                            FieldWidthTotal = FieldWidthTotal + adminColumn.Width;
                            string key = (cnt + (adminColumn.SortPriority * 1000)).ToString().PadLeft(6, '0');
                            cdef.adminColumns.Add(key, adminColumn);
                        }
                        cnt += 1;
                    }
                    //
                    // Force the Name field as the only column
                    if (cdef.fields.Count > 0) {
                        if (cdef.adminColumns.Count == 0) {
                            //
                            // Force the Name field as the only column
                            //
                            if (cdef.fields.ContainsKey("name")) {
                                adminColumn = new CDefAdminColumnClass();
                                adminColumn.Name = "Name";
                                adminColumn.SortDirection = 1;
                                adminColumn.SortPriority = 1;
                                adminColumn.Width = 100;
                                FieldWidthTotal = FieldWidthTotal + adminColumn.Width;
                                string key = ((1000)).ToString().PadLeft(6, '0');
                                cdef.adminColumns.Add(key, adminColumn);
                            }
                        }
                        //
                        // Normalize the column widths
                        //
                        foreach (var keyvaluepair in cdef.adminColumns) {
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
        /// get Cdef from content name. If the cdef is not found, return nothing.
        /// </summary>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public static CDefModel create(CoreController core, string contentName) {
            CDefModel returnCdef = null;
            try {
                int ContentId = CdefController.getContentId(core, contentName);
                if (ContentId > 0) {
                    returnCdef = create(core, ContentId);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnCdef;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Get cache key for a cdef object in cache
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public static string getCacheKey(int contentId) {
            return CacheController.getCacheKey_forObject("cdef", contentId.ToString());
        }
        //
        //====================================================================================================
        /// <summary>
        /// invalidate the cdef record for a given contentid
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentId"></param>
        public static void invalidateCache(CoreController core, int contentId) {
            core.cache.invalidate(getCacheKey(contentId));
        }
        //
        //====================================================================================================
        /// <summary>
        /// update cache for a cdef model
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentId"></param>
        /// <param name="cdef"></param>
        public static void setCache(CoreController core, int contentId, CDefModel cdef) {
            List<string> dependantList = new List<string>();
            core.cache.setObject(getCacheKey(contentId), cdef, dependantList);
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a cache version of a cdef model
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public static CDefModel getCache(CoreController core, int contentId) {
            CDefModel result = null;
            try {
                try {
                    result = core.cache.getObject<Models.Domain.CDefModel>(getCacheKey(contentId));
                } catch (Exception ex) {
                    LogController.handleError(core, ex);
                }
            } catch (Exception) { }
            return result;
        }
    }
}
