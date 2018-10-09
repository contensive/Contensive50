
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
        private const string cacheNameInvalidateAll = "cdefInvalidateAll";
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
        /// if false, all records in the table are in this content
        /// </summary>
        public bool supportLegacyContentControl { get; set; }
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
            if (_childIdList == null) {
                string Sql = "select id from cccontent where parentid=" + id;
                DataTable dt = core.db.executeQuery(Sql);
                if (dt.Rows.Count == 0) {
                    _childIdList = new List<int>();
                    foreach (DataRow parentrow in dt.Rows) {
                        _childIdList.Add(GenericController.encodeInteger( parentrow[0]));
                    }
                }
                dt.Dispose();
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
        //
        public static CDefModel create(CoreController core, int contentId, bool loadInvalidFields = false, bool forceDbLoad = false) {
            CDefModel result = null;
            try {
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
                    sql += ", c.supportLegacyContentControl";
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
                    DataTable dt = core.db.executeQuery(sql);
                    if (dt.Rows.Count == 0) {
                        //
                        // cdef not found
                        //
                    } else {
                        result = new Models.Domain.CDefModel();
                        result.fields = new Dictionary<string, Models.Domain.CDefFieldModel>();
                        result.set_childIdList(core, new List<int>());
                        result.selectList = new List<string>();
                        // -- !!!!! changed to string because dotnet json cannot serialize an integer key
                        result.adminColumns = new SortedList<string, Models.Domain.CDefModel.CDefAdminColumnClass>();
                        //
                        // ----- save values in definition
                        //
                        string contentName = null;
                        DataRow row = dt.Rows[0];
                        contentName = encodeText(GenericController.encodeText(row[1])).Trim(' ');
                        string contentTablename = GenericController.encodeText(row[10]);
                        result.name = contentName;
                        result.id = contentId;
                        result.allowAdd = GenericController.encodeBoolean(row[3]);
                        result.developerOnly = GenericController.encodeBoolean(row[4]);
                        result.adminOnly = GenericController.encodeBoolean(row[5]);
                        result.allowDelete = GenericController.encodeBoolean(row[6]);
                        result.parentID = GenericController.encodeInteger(row[7]);
                        result.dropDownFieldList = GenericController.vbUCase(GenericController.encodeText(row[9]));
                        result.tableName = GenericController.encodeText(contentTablename);
                        result.dataSourceName = "default";
                        result.allowCalendarEvents = GenericController.encodeBoolean(row[15]);
                        result.defaultSortMethod = GenericController.encodeText(row[17]);
                        if (string.IsNullOrEmpty(result.defaultSortMethod)) {
                            result.defaultSortMethod = "name";
                        }
                        result.editorGroupName = GenericController.encodeText(row[18]);
                        result.allowContentTracking = GenericController.encodeBoolean(row[19]);
                        result.allowTopicRules = GenericController.encodeBoolean(row[20]);
                        // .AllowMetaContent = genericController.EncodeBoolean(row[21])
                        //
                        result.activeOnly = true;
                        result.aliasID = "ID";
                        result.aliasName = "NAME";
                        //
                        // -- figure out later how to deprecate it. Test adding Db field read, but make sure system loads without field, or -u correctly adds field
                        result.supportLegacyContentControl = GenericController.encodeBoolean(row[21]);
                        //result.supportLegacyContentControl = true;
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
                        dt = core.db.executeQuery(sql);
                        if (dt.Rows.Count == 0) {
                            //
                        } else {
                            List<string> usedFields = new List<string>();
                            foreach (DataRow rowWithinLoop in dt.Rows) {
                                row = rowWithinLoop;
                                string fieldName = GenericController.encodeText(rowWithinLoop[13]);
                                int fieldId = GenericController.encodeInteger(rowWithinLoop[12]);
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
                                    int fieldTypeId = GenericController.encodeInteger(rowWithinLoop[15]);
                                    if (GenericController.encodeText(rowWithinLoop[4]) != "") {
                                        fieldIndexColumn = GenericController.encodeInteger(rowWithinLoop[4]);
                                    }
                                    //
                                    // translate htmlContent to fieldtypehtml
                                    //   this is also converted in upgrade, daily housekeep, addon install
                                    //
                                    bool fieldHtmlContent = GenericController.encodeBoolean(rowWithinLoop[25]);
                                    if (fieldHtmlContent) {
                                        if (fieldTypeId == FieldTypeIdLongText) {
                                            fieldTypeId = FieldTypeIdHTML;
                                        } else if (fieldTypeId == FieldTypeIdFileText) {
                                            fieldTypeId = FieldTypeIdFileHTML;
                                        }
                                    }
                                    field.active = GenericController.encodeBoolean(rowWithinLoop[24]);
                                    field.adminOnly = GenericController.encodeBoolean(rowWithinLoop[8]);
                                    field.authorable = GenericController.encodeBoolean(rowWithinLoop[27]);
                                    field.blockAccess = GenericController.encodeBoolean(rowWithinLoop[38]);
                                    field.caption = GenericController.encodeText(rowWithinLoop[16]);
                                    field.dataChanged = false;
                                    //.Changed
                                    field.contentId = contentId;
                                    field.defaultValue = GenericController.encodeText(rowWithinLoop[22]);
                                    field.developerOnly = GenericController.encodeBoolean(rowWithinLoop[0]);
                                    field.editSortPriority = GenericController.encodeInteger(rowWithinLoop[10]);
                                    field.editTabName = GenericController.encodeText(rowWithinLoop[34]);
                                    field.fieldTypeId = fieldTypeId;
                                    field.htmlContent = fieldHtmlContent;
                                    field.id = fieldId;
                                    field.indexColumn = fieldIndexColumn;
                                    field.indexSortDirection = GenericController.encodeInteger(rowWithinLoop[7]);
                                    field.indexSortOrder = GenericController.encodeInteger(rowWithinLoop[6]);
                                    field.indexWidth = GenericController.encodeText(GenericController.encodeInteger(GenericController.encodeText(rowWithinLoop[5]).Replace("%", "")));
                                    field.inherited = false;
                                    field.installedByCollectionGuid = GenericController.encodeText(rowWithinLoop[39]);
                                    field.isBaseField = GenericController.encodeBoolean(rowWithinLoop[38]);
                                    field.isModifiedSinceInstalled = false;
                                    field.lookupContentID = GenericController.encodeInteger(rowWithinLoop[18]);
                                    //.lookupContentName = ""
                                    field.lookupList = GenericController.encodeText(rowWithinLoop[37]);
                                    field.manyToManyContentID = GenericController.encodeInteger(rowWithinLoop[28]);
                                    field.manyToManyRuleContentID = GenericController.encodeInteger(rowWithinLoop[29]);
                                    field.ManyToManyRulePrimaryField = GenericController.encodeText(rowWithinLoop[30]);
                                    field.ManyToManyRuleSecondaryField = GenericController.encodeText(rowWithinLoop[31]);
                                    field.memberSelectGroupId_set( core, GenericController.encodeInteger(rowWithinLoop[36]));
                                    field.nameLc = fieldNameLower;
                                    field.notEditable = GenericController.encodeBoolean(rowWithinLoop[26]);
                                    field.password = GenericController.encodeBoolean(rowWithinLoop[3]);
                                    field.readOnly = GenericController.encodeBoolean(rowWithinLoop[17]);
                                    field.redirectContentID = GenericController.encodeInteger(rowWithinLoop[19]);
                                    //.RedirectContentName(core) = ""
                                    field.redirectID = GenericController.encodeText(rowWithinLoop[21]);
                                    field.redirectPath = GenericController.encodeText(rowWithinLoop[20]);
                                    field.required = GenericController.encodeBoolean(rowWithinLoop[14]);
                                    field.RSSTitleField = GenericController.encodeBoolean(rowWithinLoop[32]);
                                    field.RSSDescriptionField = GenericController.encodeBoolean(rowWithinLoop[33]);
                                    field.Scramble = GenericController.encodeBoolean(rowWithinLoop[35]);
                                    field.textBuffered = GenericController.encodeBoolean(rowWithinLoop[2]);
                                    field.uniqueName = GenericController.encodeBoolean(rowWithinLoop[1]);
                                    //.ValueVariant
                                    //
                                    field.helpCustom = GenericController.encodeText(rowWithinLoop[41]);
                                    field.helpDefault = GenericController.encodeText(rowWithinLoop[40]);
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
                                    dt.Dispose();
                        //
                        // ----- Create the LegacyContentControlCriteria. For compatibility, if support=false, return (1=1)
                        result.legacyContentControlCriteria = getLegacyContentControlCriteria(core, result.supportLegacyContentControl, result.id, result.tableName, result.dataSourceName, new List<int>());
                        //
                        getCdef_SetAdminColumns(core, result);
                    }
                    setCache(core, contentId, result);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        private static void getCdef_SetAdminColumns(CoreController core, Models.Domain.CDefModel cdef) {
            try {
                bool FieldActive = false;
                int FieldWidth = 0;
                int FieldWidthTotal = 0;
                Models.Domain.CDefModel.CDefAdminColumnClass adminColumn = null;
                //
                if (cdef.id > 0) {
                    int cnt = 0;
                    foreach (KeyValuePair<string, Models.Domain.CDefFieldModel> keyValuePair in cdef.fields) {
                        Models.Domain.CDefFieldModel field = keyValuePair.Value;
                        FieldActive = field.active;
                        FieldWidth = GenericController.encodeInteger(field.indexWidth);
                        if (FieldActive && (FieldWidth > 0)) {
                            FieldWidthTotal = FieldWidthTotal + FieldWidth;
                            adminColumn = new Models.Domain.CDefModel.CDefAdminColumnClass();
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
                    //
                    if (cdef.fields.Count > 0) {
                        if (cdef.adminColumns.Count == 0) {
                            //
                            // Force the Name field as the only column
                            //
                            if (cdef.fields.ContainsKey("name")) {
                                adminColumn = new Models.Domain.CDefModel.CDefAdminColumnClass();
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
        /// get content id from content name
        /// </summary>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public static int getContentId(CoreController core, string contentName) {
            int returnId = 0;
            try {
                //
                // -- method 2 - if name/id dictionary doesnt have it, load the one record
                if (core.doc.contentNameIdDictionary.ContainsKey(contentName.ToLower())) {
                    returnId = core.doc.contentNameIdDictionary[contentName.ToLower()];
                } else { 
                    ContentModel content = ContentModel.createByUniqueName(core, contentName);
                    if ( content != null ) {
                        core.doc.contentNameIdDictionary.Add(contentName.ToLower(), content.id);
                        returnId = content.id;
                    }
                }
                //
                // -- method-1, on first request, load all content records
                //if (core.doc.contentNameIdDictionary.ContainsKey(contentName.ToLower())) {
                //    returnId = core.doc.contentNameIdDictionary[contentName.ToLower()];
                //}
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnId;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get Cdef from content name. If the cdef is not found, return nothing.
        /// </summary>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public static CDefModel getCdef(CoreController core, string contentName) {
            Models.Domain.CDefModel returnCdef = null;
            try {
                int ContentId = getContentId(core, contentName);
                if (ContentId > 0) {
                    returnCdef = getCdef(core, ContentId);
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
        /// return a cdef class from content id. Returns nothing if contentId is not valid
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public static CDefModel getCdef(CoreController core, int contentId, bool forceDbLoad = false, bool loadInvalidFields = false) {
            CDefModel returnCdef = null;
            try {
                if (contentId <= 0) {
                    //
                    // -- invalid id                    
                } else if ((!forceDbLoad) && (core.doc.cdefDictionary.ContainsKey(contentId.ToString()))) {
                    //
                    // -- already loaded and no force re-load, just return the current cdef                    
                    returnCdef = core.doc.cdefDictionary[contentId.ToString()];
                } else {
                    if (core.doc.cdefDictionary.ContainsKey(contentId.ToString())) {
                        //
                        // -- key is already there, remove it first                        
                        core.doc.cdefDictionary.Remove(contentId.ToString());
                    }
                    returnCdef = create(core, contentId, loadInvalidFields, forceDbLoad);
                    core.doc.cdefDictionary.Add(contentId.ToString(), returnCdef);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnCdef;
        }
        //
        //========================================================================
        //   Get Child Criteria
        //
        //   Dig into Content Definition Records and create an SQL Criteria statement
        //   for parent-child relationships.
        //
        //   for instance, for a ContentControlCriteria, call this with:
        //       CriteriaFieldName = "ContentID"
        //       ContentName = "Content"
        //
        //   Results in (ContentID=5)or(ContentID=6)or(ContentID=10)
        //
        // Get a string that can be used in the where criteria of a SQL statement
        // opening the content pointed to by the content pointer. This criteria
        // will include both the content, and its child contents.
        //========================================================================
        //
        internal static string getLegacyContentControlCriteria(CoreController core, bool supportLegacyContentControl, int contentId, string contentTableName, string contentDAtaSourceName, List<int> parentIdList) {
            string returnCriteria = "";
            try {
                //
                if (!supportLegacyContentControl) {
                    returnCriteria = "(1=1)";
                } else {
                    returnCriteria = "(1=0)";
                    if (contentId >= 0) {
                        if (!parentIdList.Contains(contentId)) {
                            parentIdList.Add(contentId);
                            returnCriteria = "(" + contentTableName + ".contentcontrolId=" + contentId + ")";
                            //
                            // TODO -- only works if contentIdDist is fully loaded, but this is the only requirement to preload Dict, so removing this removes negative-performance requirements
                            //
                            foreach (var childContent in ContentModel.createList(core, "(parentid=" + contentId + ")")) {
                                returnCriteria += "OR" + getLegacyContentControlCriteria(core, childContent.supportLegacyContentControl, childContent.id, contentTableName, contentDAtaSourceName, parentIdList);
                            }
                            //foreach (KeyValuePair<int, ContentModel> kvp in core.doc.contentIdDict) {
                            //    if (kvp.Value.parentID == contentId) {
                            //        returnCriteria += "OR" + getLegacyContentControlCriteria(core, kvp.Value.supportLegacyContentControl, kvp.Value.id, contentTableName, contentDAtaSourceName, parentIdList);
                            //    }
                            //}
                            parentIdList.Remove(contentId);
                            returnCriteria = "(" + returnCriteria + ")";
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnCriteria;
        }
        //
        //========================================================================
        //   IsWithinContent( ChildContentID, ParentContentID )
        //
        //       Returns true if ChildContentID is in ParentContentID
        //========================================================================
        //
        public static bool isWithinContent(CoreController core, int ChildContentID, int ParentContentID) {
            bool returnOK = false;
            try {
                Models.Domain.CDefModel cdef = null;
                if (ChildContentID == ParentContentID) {
                    returnOK = true;
                } else {
                    cdef = getCdef(core, ParentContentID);
                    if (cdef != null) {
                        if (cdef.get_childIdList(core).Count > 0) {
                            returnOK = cdef.get_childIdList(core).Contains(ChildContentID);
                            if (!returnOK) {
                                foreach (int contentId in cdef.get_childIdList(core)) {
                                    returnOK = isWithinContent(core, contentId, ParentContentID);
                                    if (returnOK) {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnOK;
        }
        //
        //===========================================================================
        //   main_Get Authoring List
        //       returns a comma delimited list of ContentIDs that the Member can author
        //===========================================================================
        //
        public static List<int> getEditableCdefIdList(CoreController core) {
            List<int> returnList = new List<int>();
            try {
                string SQL = null;
                DataTable cidDataTable = null;
                int CIDCount = 0;
                int CIDPointer = 0;
                Models.Domain.CDefModel CDef = null;
                int ContentID = 0;
                //
                SQL = "Select ccGroupRules.ContentID as ID"
                + " FROM ((ccmembersrules"
                + " Left Join ccGroupRules on ccMemberRules.GroupID=ccGroupRules.GroupID)"
                + " Left Join ccContent on ccGroupRules.ContentID=ccContent.ID)"
                + " WHERE"
                    + " (ccMemberRules.MemberID=" + core.session.user.id + ")"
                    + " AND(ccGroupRules.Active<>0)"
                    + " AND(ccContent.Active<>0)"
                    + " AND(ccMemberRules.Active<>0)";
                cidDataTable = core.db.executeQuery(SQL);
                CIDCount = cidDataTable.Rows.Count;
                for (CIDPointer = 0; CIDPointer < CIDCount; CIDPointer++) {
                    ContentID = GenericController.encodeInteger(cidDataTable.Rows[CIDPointer][0]);
                    returnList.Add(ContentID);
                    CDef = getCdef(core, ContentID);
                    if (CDef != null) {
                        returnList.AddRange(CDef.get_childIdList(core));
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnList;
        }
        //
        //=============================================================================
        // Create a child content from a parent content
        //
        //   If child does not exist, copy everything from the parent
        //   If child already exists, add any missing fields from parent
        //=============================================================================
        //
        public static void createContentChild(CoreController core, string ChildContentName, string ParentContentName, int MemberID) {
            try {
                string dataSourceName = "default";
                string SQL = null;
                DataTable rs = null;
                int ChildContentID = 0;
                int ParentContentID = 0;
                int CSContent = 0;
                int CSNew = 0;
                string SelectFieldList = null;
                string[] Fields = null;
                string FieldName = null;
                DateTime DateNow;
                //
                DateNow = DateTime.MinValue;
                SQL = "select ID from ccContent where name=" + core.db.encodeSQLText(ChildContentName) + ";";
                rs = core.db.executeQuery(SQL);
                if (DbController.isDataTableOk(rs)) {
                    ChildContentID = GenericController.encodeInteger(core.db.getDataRowColumnName(rs.Rows[0], "ID"));
                    //
                    // mark the record touched so upgrade will not delete it
                    //
                    core.db.executeQuery("update ccContent set CreateKey=0 where ID=" + ChildContentID);
                }
                DbController.closeDataTable(rs);
                if (ChildContentID == 0) {
                    //
                    // Get ContentID of parent
                    //
                    SQL = "select ID from ccContent where name=" + core.db.encodeSQLText(ParentContentName) + ";";
                    rs = core.db.executeQuery(SQL, dataSourceName);
                    if (DbController.isDataTableOk(rs)) {
                        ParentContentID = GenericController.encodeInteger(core.db.getDataRowColumnName(rs.Rows[0], "ID"));
                        //
                        // mark the record touched so upgrade will not delete it
                        //
                        core.db.executeQuery("update ccContent set CreateKey=0 where ID=" + ParentContentID);
                    }
                    DbController.closeDataTable(rs);
                    //
                    if (ParentContentID == 0) {
                        throw (new ApplicationException("Can not create Child Content [" + ChildContentName + "] because the Parent Content [" + ParentContentName + "] was not found."));
                    } else {
                        //
                        // ----- create child content record, let the csv_ExecuteSQL reload CDef
                        //
                        dataSourceName = "Default";
                        CSContent = core.db.csOpenContentRecord("Content", ParentContentID);
                        if (!core.db.csOk(CSContent)) {
                            throw (new ApplicationException("Can not create Child Content [" + ChildContentName + "] because the Parent Content [" + ParentContentName + "] was not found."));
                        } else {
                            SelectFieldList = core.db.csGetSelectFieldList(CSContent);
                            if (string.IsNullOrEmpty(SelectFieldList)) {
                                throw (new ApplicationException("Can not create Child Content [" + ChildContentName + "] because the Parent Content [" + ParentContentName + "] record has not fields."));
                            } else {
                                CSNew = core.db.csInsertRecord("Content", 0);
                                if (!core.db.csOk(CSNew)) {
                                    throw (new ApplicationException("Can not create Child Content [" + ChildContentName + "] because there was an error creating a new record in ccContent."));
                                } else {
                                    Fields = SelectFieldList.Split(',');
                                    DateNow = DateTime.Now;
                                    for (var FieldPointer = 0; FieldPointer <= Fields.GetUpperBound(0); FieldPointer++) {
                                        FieldName = Fields[FieldPointer];
                                        switch (GenericController.vbUCase(FieldName)) {
                                            case "ID":
                                                // do nothing
                                                break;
                                            case "NAME":
                                                core.db.csSet(CSNew, FieldName, ChildContentName);
                                                break;
                                            case "PARENTID":
                                                core.db.csSet(CSNew, FieldName, core.db.csGetText(CSContent, "ID"));
                                                break;
                                            case "CREATEDBY":
                                            case "MODIFIEDBY":
                                                core.db.csSet(CSNew, FieldName, MemberID);
                                                break;
                                            case "DATEADDED":
                                            case "MODIFIEDDATE":
                                                core.db.csSet(CSNew, FieldName, DateNow);
                                                break;
                                            case "CCGUID":

                                                //
                                                // new, non-blank guid so if this cdef is exported, it will be updateable
                                                //
                                                core.db.csSet(CSNew, FieldName, createGuid());
                                                break;
                                            default:
                                                core.db.csSet(CSNew, FieldName, core.db.csGetText(CSContent, FieldName));
                                                break;
                                        }
                                    }
                                }
                                core.db.csClose(ref CSNew);
                            }
                        }
                        core.db.csClose(ref CSContent);
                    }
                }
                //
                // ----- Load CDef
                //
                core.cache.invalidateAll();
                core.doc.clearMetaData();
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
        //
        //========================================================================
        // Get a Contents Tablename from the ContentPointer
        //========================================================================
        //
        public static string getContentTablename(CoreController core, string ContentName) {
            string returnTableName = "";
            try {
                Models.Domain.CDefModel CDef;
                //
                CDef = Models.Domain.CDefModel.getCdef(core, ContentName);
                if (CDef != null) {
                    returnTableName = CDef.tableName;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnTableName;
        }
        //
        //========================================================================
        // ----- Get a DataSource Name from its ContentName
        //
        public static string getContentDataSource(CoreController core, string ContentName) {
            string returnDataSource = "";
            try {
                Models.Domain.CDefModel CDef;
                //
                CDef = Models.Domain.CDefModel.getCdef(core, ContentName);
                if (CDef == null) {
                    //
                } else {
                    returnDataSource = CDef.dataSourceName;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnDataSource;
        }
        //
        //========================================================================
        // Get a Contents Name from the ContentID
        //   Bad ContentID returns blank
        //========================================================================
        //
        public static string getContentNameByID(CoreController core, int ContentID) {
            string returnName = "";
            try {
                Models.Domain.CDefModel cdef;
                //
                cdef = Models.Domain.CDefModel.getCdef(core, ContentID);
                if (cdef != null) {
                    returnName = cdef.name;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnName;
        }
        //
        //========================================================================
        /// <summary>
        /// Verify a content entry and return the id. If it does not exist, it is added with default values
        /// </summary>
        /// <param name="core"></param>
        /// <param name="cdef"></param>
        /// <returns></returns>
        public static int verifyContent_returnId(CoreController core, CDefModel cdef ) { // bool Active, DataSourceModel datasource, string TableName, string contentName, bool AdminOnly = false, bool DeveloperOnly = false, bool AllowAdd = true, bool AllowDelete = true, string ParentName = "", string DefaultSortMethod = "", string DropDownFieldList = "", bool AllowWorkflowAuthoring = false, bool AllowCalendarEvents = false, bool AllowContentTracking = false, bool AllowTopicRules = false, bool AllowContentChildTool = false, bool ignore1 = false, string IconLink = "", int IconWidth = 0, int IconHeight = 0, int IconSprites = 0, string ccGuid = "", bool IsBaseContent = false, string installedByCollectionGuid = "", bool clearMetaCache = false) {
            int returnContentId = 0;
            try {
                //
                LogController.logTrace(core, "addContent, contentName [" + cdef.name + "], tableName [" + cdef.tableName + "]");
                //
                if (string.IsNullOrEmpty(cdef.name)) {
                    throw new ApplicationException("contentName can not be blank");
                } else if (string.IsNullOrEmpty(cdef.tableName)) {
                    throw new ApplicationException("Tablename can not be blank");
                } else {
                    //
                    core.db.createSQLTable(cdef.dataSourceName, cdef.tableName);
                    //
                    string contentGuid = "";
                    bool ContentIsBaseContent = false;
                    //
                    // get contentId, guid, IsBaseContent
                    //
                    string SQL = "select ID,ccguid,IsBaseContent from ccContent where (name=" + core.db.encodeSQLText(cdef.name) + ") order by id;";
                    DataTable dt = core.db.executeQuery(SQL);
                    if (dt.Rows.Count > 0) {
                        returnContentId = GenericController.encodeInteger(dt.Rows[0]["ID"]);
                        contentGuid = GenericController.vbLCase(GenericController.encodeText(dt.Rows[0]["ccguid"]));
                        ContentIsBaseContent = GenericController.encodeBoolean(dt.Rows[0]["IsBaseContent"]);
                    }
                    dt.Dispose();
                    //
                    // get contentid of content
                    //
                    int ContentIDofContent = 0;
                    if (cdef.name.ToLower() == "content") {
                        ContentIDofContent = returnContentId;
                    } else {
                        SQL = "select ID from ccContent where (name='content') order by id;";
                        dt = core.db.executeQuery(SQL);
                        if (dt.Rows.Count > 0) {
                            ContentIDofContent = GenericController.encodeInteger(dt.Rows[0]["ID"]);
                        }
                        dt.Dispose();
                    }
                    int parentId = 0;
                    //
                    // get parentId
                    //
                    if (!string.IsNullOrEmpty(cdef.parentName)) {
                        SQL = "select id from ccContent where (name=" + core.db.encodeSQLText(cdef.parentName) + ") order by id;";
                        dt = core.db.executeQuery(SQL);
                        if (dt.Rows.Count > 0) {
                            parentId = GenericController.encodeInteger(dt.Rows[0][0]);
                        }
                        dt.Dispose();
                    }
                    //
                    // get InstalledByCollectionID
                    //
                    int InstalledByCollectionID = 0;
                    if (!string.IsNullOrEmpty(cdef.installedByCollectionGuid)) {
                        SQL = "select id from ccAddonCollections where ccGuid=" + core.db.encodeSQLText(cdef.installedByCollectionGuid);
                        dt = core.db.executeQuery(SQL);
                        if (dt.Rows.Count > 0) {
                            InstalledByCollectionID = GenericController.encodeInteger(dt.Rows[0]["ID"]);
                        }
                    }
                    //
                    // Block non-base update of a base field
                    //
                    {
                        bool CDefFound = (returnContentId != 0);
                        if (!CDefFound) {
                            //
                            // ----- Create a new empty Content Record (to get ContentID)
                            //
                            returnContentId = core.db.insertTableRecordGetId("Default", "ccContent", SystemMemberID);
                        }
                        //
                        // ----- Get the Table Definition ID, create one if missing
                        //
                        SQL = "SELECT ID from ccTables where (active<>0) and (name=" + core.db.encodeSQLText(cdef.tableName) + ");";
                        dt = core.db.executeQuery(SQL);
                        int TableID = 0;
                        SqlFieldListClass sqlList = null;
                        if (dt.Rows.Count <= 0) {
                            //
                            LogController.logTrace(core, "addContent, create ccTable record, tableName [" + cdef.tableName + "]");
                            //
                            //
                            // ----- no table definition found, create one
                            //
                            //If genericController.vbUCase(DataSourceName) = "DEFAULT" Then
                            //    DataSourceID = -1
                            //ElseIf DataSourceName = "" Then
                            //    DataSourceID = -1
                            //Else
                            //    DataSourceID = core.db.getDataSourceId(DataSourceName)
                            //    If DataSourceID = -1 Then
                            //        throw (New ApplicationException("Could not find DataSource [" & DataSourceName & "] for table [" & TableName & "]"))
                            //    End If
                            //End If
                            TableID = core.db.insertTableRecordGetId("Default", "ccTables", SystemMemberID);
                            //
                            sqlList = new SqlFieldListClass();
                            sqlList.add("name", core.db.encodeSQLText(cdef.tableName));
                            sqlList.add("active", SQLTrue);
                            sqlList.add("DATASOURCEID", core.db.encodeSQLNumber(cdef.dataSourceId));
                            sqlList.add("CONTENTCONTROLID", core.db.encodeSQLNumber(Models.Domain.CDefModel.getContentId(core, "Tables")));
                            //
                            core.db.updateTableRecord("Default", "ccTables", "ID=" + TableID, sqlList);
                            TableModel.invalidateRecordCache(core, TableID);
                        } else {
                            TableID = GenericController.encodeInteger(dt.Rows[0]["ID"]);
                        }
                        //
                        // ----- Get Sort Method ID from SortMethod
                        string iDefaultSortMethod = encodeEmpty(cdef.defaultSortMethod, "");
                        int DefaultSortMethodID = 0;
                        //
                        // First - try lookup by name
                        //
                        if (string.IsNullOrEmpty(iDefaultSortMethod)) {
                            DefaultSortMethodID = 0;
                        } else {
                            dt = core.db.openTable("Default", "ccSortMethods", "(name=" + core.db.encodeSQLText(iDefaultSortMethod) + ")and(active<>0)", "ID", "ID", 1, 1);
                            if (dt.Rows.Count > 0) {
                                DefaultSortMethodID = GenericController.encodeInteger(dt.Rows[0]["ID"]);
                            }
                        }
                        if (DefaultSortMethodID == 0) {
                            //
                            // fallback - maybe they put the orderbyclause in (common mistake)
                            //
                            dt = core.db.openTable("Default", "ccSortMethods", "(OrderByClause=" + core.db.encodeSQLText(iDefaultSortMethod) + ")and(active<>0)", "ID", "ID", 1, 1);
                            if (dt.Rows.Count > 0) {
                                DefaultSortMethodID = GenericController.encodeInteger(dt.Rows[0]["ID"]);
                            }
                        }
                        //
                        // determine parentId from parentName
                        //

                        //
                        // ----- update record
                        //
                        sqlList = new SqlFieldListClass();
                        sqlList.add("name", core.db.encodeSQLText(cdef.name));
                        sqlList.add("CREATEKEY", "0");
                        sqlList.add("active", core.db.encodeSQLBoolean(cdef.active));
                        sqlList.add("ContentControlID", core.db.encodeSQLNumber(ContentIDofContent));
                        sqlList.add("AllowAdd", core.db.encodeSQLBoolean(cdef.allowAdd));
                        sqlList.add("AllowDelete", core.db.encodeSQLBoolean(cdef.allowDelete));
                        sqlList.add("AllowWorkflowAuthoring", core.db.encodeSQLBoolean(false));
                        sqlList.add("DeveloperOnly", core.db.encodeSQLBoolean(cdef.developerOnly));
                        sqlList.add("AdminOnly", core.db.encodeSQLBoolean(cdef.adminOnly));
                        sqlList.add("ParentID", core.db.encodeSQLNumber(parentId));
                        sqlList.add("DefaultSortMethodID", core.db.encodeSQLNumber(DefaultSortMethodID));
                        sqlList.add("DropDownFieldList", core.db.encodeSQLText(encodeEmpty(cdef.dropDownFieldList, "Name")));
                        sqlList.add("ContentTableID", core.db.encodeSQLNumber(TableID));
                        sqlList.add("AuthoringTableID", core.db.encodeSQLNumber(TableID));
                        sqlList.add("ModifiedDate", core.db.encodeSQLDate(DateTime.Now));
                        sqlList.add("CreatedBy", core.db.encodeSQLNumber(SystemMemberID));
                        sqlList.add("ModifiedBy", core.db.encodeSQLNumber(SystemMemberID));
                        sqlList.add("AllowCalendarEvents", core.db.encodeSQLBoolean(cdef.allowCalendarEvents));
                        sqlList.add("AllowContentTracking", core.db.encodeSQLBoolean(cdef.allowContentTracking));
                        sqlList.add("AllowTopicRules", core.db.encodeSQLBoolean(cdef.allowTopicRules));
                        sqlList.add("AllowContentChildTool", core.db.encodeSQLBoolean(cdef.allowContentChildTool));
                        //Call sqlList.add("AllowMetaContent", core.db.encodeSQLBoolean(ignore1))
                        sqlList.add("IconLink", core.db.encodeSQLText(encodeEmpty(cdef.iconLink, "")));
                        sqlList.add("IconHeight", core.db.encodeSQLNumber(cdef.iconHeight));
                        sqlList.add("IconWidth", core.db.encodeSQLNumber(cdef.iconWidth));
                        sqlList.add("IconSprites", core.db.encodeSQLNumber(cdef.iconSprites));
                        sqlList.add("installedByCollectionid", core.db.encodeSQLNumber(InstalledByCollectionID));
                        sqlList.add("supportLegacyContentControl", core.db.encodeSQLBoolean(cdef.supportLegacyContentControl));
                        if ((string.IsNullOrEmpty(contentGuid)) && (!string.IsNullOrEmpty(cdef.guid))) {
                            //
                            // hard one - only update guid if the tables supports it, and it the new guid is not blank
                            // if the new guid does no match te old guid
                            //
                            sqlList.add("ccGuid", core.db.encodeSQLText(cdef.guid));
                        } else if ((!string.IsNullOrEmpty(cdef.guid)) & (contentGuid != GenericController.vbLCase(cdef.guid))) {
                            //
                            // installing content definition with matching name, but different guid -- this is an error that needs to be fixed
                            //
                            LogController.handleError( core,new ApplicationException("createContent call, content.name match found but content.ccGuid did not, name [" + cdef.name + "], newGuid [" + cdef.guid + "], installedGuid [" + contentGuid + "] "));
                        }
                        core.db.updateTableRecord("Default", "ccContent", "ID=" + returnContentId, sqlList);
                        ContentModel.invalidateRecordCache(core, returnContentId);
                        //
                        //-----------------------------------------------------------------------------------------------
                        // Verify Core Content Definition Fields
                        //-----------------------------------------------------------------------------------------------
                        //
                        if (parentId < 1) {
                            CDefFieldModel field = null;
                            //
                            // CDef does not inherit its fields, create what is needed for a non-inherited CDef
                            //
                            if (!core.db.isCdefField(returnContentId, "ID")) {
                                field = new Models.Domain.CDefFieldModel();
                                field.nameLc = "id";
                                field.active = true;
                                field.fieldTypeId = FieldTypeIdAutoIdIncrement;
                                field.editSortPriority = 100;
                                field.authorable = false;
                                field.caption = "ID";
                                field.defaultValue = "";
                                field.isBaseField = cdef.isBaseContent;
                                verifyContentField_returnID(core, cdef.name, field);
                            }
                            //
                            if (!core.db.isCdefField(returnContentId, "name")) {
                                field = new Models.Domain.CDefFieldModel();
                                field.nameLc = "name";
                                field.active = true;
                                field.fieldTypeId = FieldTypeIdText;
                                field.editSortPriority = 110;
                                field.authorable = true;
                                field.caption = "Name";
                                field.defaultValue = "";
                                field.isBaseField = cdef.isBaseContent;
                                verifyContentField_returnID(core, cdef.name, field);
                            }
                            //
                            if (!core.db.isCdefField(returnContentId, "active")) {
                                field = new Models.Domain.CDefFieldModel();
                                field.nameLc = "active";
                                field.active = true;
                                field.fieldTypeId = FieldTypeIdBoolean;
                                field.editSortPriority = 200;
                                field.authorable = true;
                                field.caption = "Active";
                                field.defaultValue = "1";
                                field.isBaseField = cdef.isBaseContent;
                                verifyContentField_returnID(core, cdef.name, field);
                            }
                            //
                            if (!core.db.isCdefField(returnContentId, "sortorder")) {
                                field = new Models.Domain.CDefFieldModel();
                                field.nameLc = "sortorder";
                                field.active = true;
                                field.fieldTypeId = FieldTypeIdText;
                                field.editSortPriority = 2000;
                                field.authorable = false;
                                field.caption = "Alpha Sort Order";
                                field.defaultValue = "";
                                field.isBaseField = cdef.isBaseContent;
                                verifyContentField_returnID(core, cdef.name, field);
                            }
                            //
                            if (!core.db.isCdefField(returnContentId, "dateadded")) {
                                field = new Models.Domain.CDefFieldModel();
                                field.nameLc = "dateadded";
                                field.active = true;
                                field.fieldTypeId = FieldTypeIdDate;
                                field.editSortPriority = 9999;
                                field.authorable = false;
                                field.caption = "Date Added";
                                field.defaultValue = "";
                                field.isBaseField = cdef.isBaseContent;
                                verifyContentField_returnID(core, cdef.name, field);
                            }
                            if (!core.db.isCdefField(returnContentId, "createdby")) {
                                field = new Models.Domain.CDefFieldModel();
                                field.nameLc = "createdby";
                                field.active = true;
                                field.fieldTypeId = FieldTypeIdLookup;
                                field.editSortPriority = 9999;
                                field.authorable = false;
                                field.caption = "Created By";
                                field.set_lookupContentName(core, "People");
                                field.defaultValue = "";
                                field.isBaseField = cdef.isBaseContent;
                                verifyContentField_returnID(core, cdef.name, field);
                            }
                            if (!core.db.isCdefField(returnContentId, "modifieddate")) {
                                field = new Models.Domain.CDefFieldModel();
                                field.nameLc = "modifieddate";
                                field.active = true;
                                field.fieldTypeId = FieldTypeIdDate;
                                field.editSortPriority = 9999;
                                field.authorable = false;
                                field.caption = "Date Modified";
                                field.defaultValue = "";
                                field.isBaseField = cdef.isBaseContent;
                                verifyContentField_returnID(core, cdef.name, field);
                            }
                            if (!core.db.isCdefField(returnContentId, "modifiedby")) {
                                field = new Models.Domain.CDefFieldModel();
                                field.nameLc = "modifiedby";
                                field.active = true;
                                field.fieldTypeId = FieldTypeIdLookup;
                                field.editSortPriority = 9999;
                                field.authorable = false;
                                field.caption = "Modified By";
                                field.set_lookupContentName(core, "People");
                                field.defaultValue = "";
                                field.isBaseField = cdef.isBaseContent;
                                verifyContentField_returnID(core, cdef.name, field);
                            }
                            if (!core.db.isCdefField(returnContentId, "ContentControlId")) {
                                field = new Models.Domain.CDefFieldModel();
                                field.nameLc = "contentcontrolid";
                                field.active = true;
                                field.fieldTypeId = FieldTypeIdLookup;
                                field.editSortPriority = 9999;
                                field.authorable = false;
                                field.caption = "Controlling Content";
                                field.set_lookupContentName(core, "Content");
                                field.defaultValue = "";
                                field.isBaseField = cdef.isBaseContent;
                                verifyContentField_returnID(core, cdef.name, field);
                            }
                            if (!core.db.isCdefField(returnContentId, "CreateKey")) {
                                field = new Models.Domain.CDefFieldModel();
                                field.nameLc = "createkey";
                                field.active = true;
                                field.fieldTypeId = FieldTypeIdInteger;
                                field.editSortPriority = 9999;
                                field.authorable = false;
                                field.caption = "Create Key";
                                field.defaultValue = "";
                                field.isBaseField = cdef.isBaseContent;
                                verifyContentField_returnID(core, cdef.name, field);
                            }
                            if (!core.db.isCdefField(returnContentId, "ccGuid")) {
                                field = new Models.Domain.CDefFieldModel();
                                field.nameLc = "ccguid";
                                field.active = true;
                                field.fieldTypeId = FieldTypeIdText;
                                field.editSortPriority = 9999;
                                field.authorable = false;
                                field.caption = "Guid";
                                field.defaultValue = "";
                                field.isBaseField = cdef.isBaseContent;
                                verifyContentField_returnID(core, cdef.name, field);
                            }
                            // -- 20171029 - had to un-deprecate because compatibility issues are too timeconsuming
                            if (!core.db.isCdefField(returnContentId, "ContentCategoryId")) {
                                field = new Models.Domain.CDefFieldModel();
                                field.nameLc = "contentcategoryid";
                                field.active = true;
                                field.fieldTypeId = FieldTypeIdInteger;
                                field.editSortPriority = 9999;
                                field.authorable = false;
                                field.caption = "Content Category";
                                field.defaultValue = "";
                                field.isBaseField = cdef.isBaseContent;
                                verifyContentField_returnID(core, cdef.name, field);
                            }
                        }
                        //
                        // ----- Load CDef
                        //
                        ContentModel.invalidateTableCache(core);
                        ContentFieldModel.invalidateTableCache(core);
                        core.doc.clearMetaData();
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnContentId;
        }
        //
        // ====================================================================================================================
        //   Verify a CDef field and return the recordid
        //       same a old csv_CreateContentField
        //      args is a delimited name=value pair sring: a=1,b=2,c=3 where delimiter = ","
        //
        // ***** add optional argument, doNotOverWrite -- called true from csv_CreateContent3 so if the cdef is there, it's fields will not be crushed.
        //
        // ====================================================================================================================
        //
        public static int verifyContentField_returnID(CoreController core, string ContentName, Models.Domain.CDefFieldModel field) // , ByVal FieldName As String, ByVal Args As String, ByVal Delimiter As String) As Integer
        {
            int returnId = 0;
            try {
                string[] SQLName = new string[101];
                string[] SQLValue = new string[101];
                //
                //
                // determine contentid and tableid
                int ContentID = -1;
                int TableID = 0;
                {
                    var content = ContentModel.createByUniqueName(core, ContentName);
                    if ( content != null ) {
                        ContentID = content.id;
                        TableID = content.contentTableID;
                    }
                }
                //SQL = "select ID,ContentTableID from ccContent where name=" + core.db.encodeSQLText(ContentName) + ";";
                //rs = core.db.executeQuery(SQL);
                //if (DbController.isDataTableOk(rs)) {
                //    ContentID = GenericController.encodeInteger(core.db.getDataRowColumnName(rs.Rows[0], "ID"));
                //    TableID = GenericController.encodeInteger(core.db.getDataRowColumnName(rs.Rows[0], "ContentTableID"));
                //}
                //
                // test if field definition found or not
                //
                int RecordID = 0;
                //
                bool RecordIsBaseField = false;
                bool isNewFieldRecord = true;
                {
                    var contentFieldList = ContentFieldModel.createList(core, "(ContentID=" + core.db.encodeSQLNumber(ContentID) + ")and(name=" + core.db.encodeSQLText(field.nameLc) + ")");
                    if (contentFieldList.Count > 0) {
                        isNewFieldRecord = false;
                        RecordID = contentFieldList.First().id;
                        RecordIsBaseField = contentFieldList.First().isBaseField;
                    }
                }
                //SQL = "select ID,IsBaseField from ccFields where (ContentID=" + core.db.encodeSQLNumber(ContentID) + ")and(name=" + core.db.encodeSQLText(field.nameLc) + ");";
                //rs = core.db.executeQuery(SQL);
                //if (DbController.isDataTableOk(rs)) {
                //    isNewFieldRecord = false;
                //    RecordID = GenericController.encodeInteger(core.db.getDataRowColumnName(rs.Rows[0], "ID"));
                //    RecordIsBaseField = GenericController.encodeBoolean(core.db.getDataRowColumnName(rs.Rows[0], "IsBaseField"));
                //}
                //
                // check if this is a non-base field updating a base field
                //
                bool IsBaseField = field.isBaseField;
                if ((!IsBaseField) && (RecordIsBaseField)) {
                    //
                    // This update is not allowed
                    //
                    LogController.handleWarn( core,new ApplicationException("Warning, updating non-base field with base field, content [" + ContentName + "], field [" + field.nameLc + "]"));
                }
                {
                    //FieldAdminOnly = field.adminOnly
                    bool FieldDeveloperOnly = field.developerOnly;
                    bool FieldActive = field.active;
                    string FieldCaption = field.caption;
                    bool FieldReadOnly = field.readOnly;
                    int fieldTypeId = field.fieldTypeId;
                    bool FieldAuthorable = field.authorable;
                    string DefaultValue = GenericController.encodeText(field.defaultValue);
                    bool NotEditable = field.notEditable;
                    string LookupContentName = field.get_lookupContentName(core);
                    string AdminIndexWidth = field.indexWidth;
                    int AdminIndexSort = field.indexSortOrder;
                    string RedirectContentName = field.get_redirectContentName(core);
                    string RedirectIDField = field.redirectID;
                    string RedirectPath = field.redirectPath;
                    bool HTMLContent = field.htmlContent;
                    bool UniqueName = field.uniqueName;
                    bool Password = field.password;
                    bool FieldRequired = field.required;
                    bool RSSTitle = field.RSSTitleField;
                    bool RSSDescription = field.RSSDescriptionField;
                    int MemberSelectGroupID = field.memberSelectGroupId_get(core);
                    string installedByCollectionGuid = field.installedByCollectionGuid;
                    string EditTab = field.editTabName;
                    string LookupList = field.lookupList;
                    //
                    // ----- Check error conditions before starting
                    //
                    if (ContentID == -1) {
                        //
                        // Content Definition not found
                        //
                        throw (new ApplicationException("Could not create field [" + field.nameLc + "] because Content Definition [" + ContentName + "] was not found In ccContent Table."));
                    } else if (TableID <= 0) {
                        //
                        // Content Definition not found
                        //
                        throw (new ApplicationException("Could not create field [" + field.nameLc + "] because Content Definition [" + ContentName + "] has no associated Content Table."));
                    } else if (fieldTypeId <= 0) {
                        //
                        // invalid field type
                        //
                        throw (new ApplicationException("Could Not create Field [" + field.nameLc + "] because the field type [" + fieldTypeId + "] Is Not valid."));
                    } else {
                        //
                        // Get the TableName and DataSourceID
                        //
                        string TableName = "";
                        int DataSourceID = 0;
                        {
                            var table = TableModel.create(core, TableID);
                            if (table != null) {
                                DataSourceID = table.dataSourceID;
                                TableName = table.name;
                            }
                        }
                        //rs = core.db.executeQuery("Select Name, DataSourceID from ccTables where ID=" + core.db.encodeSQLNumber(TableID) + ";");
                        //if (!DbController.isDataTableOk(rs)) {
                        //    throw (new ApplicationException("Could Not create Field [" + field.nameLc + "] because table For tableID [" + TableID + "] was not found."));
                        //} else {
                        //    DataSourceID = GenericController.encodeInteger(core.db.getDataRowColumnName(rs.Rows[0], "DataSourceID"));
                        //    TableName = GenericController.encodeText(core.db.getDataRowColumnName(rs.Rows[0], "Name"));
                        //}
                        //rs.Dispose();
                        if (!string.IsNullOrEmpty(TableName)) {
                            //
                            // Get the DataSourceName - special case model, returns default object if input not valid
                            var dataSource = DataSourceModel.create(core, DataSourceID);
                            string DataSourceName = dataSource.name;
                            //if (DataSourceID < 1) {
                            //    DataSourceName = "Default";
                            //} else {
                            //    rs = core.db.executeQuery("Select Name from ccDataSources where ID=" + core.db.encodeSQLNumber(DataSourceID) + ";");
                            //    if (!DbController.isDataTableOk(rs)) {

                            //        DataSourceName = "Default";
                            //        // change condition to successful -- the goal is 1) deliver pages 2) report problems
                            //        // this problem, if translated to default, is really no longer a problem, unless the
                            //        // resulting datasource does not have this data, then other errors will be generated anyway.
                            //        //Call csv_HandleClassInternalError(MethodName, "Could Not create Field [" & field.name & "] because datasource For ID [" & DataSourceID & "] was not found.")
                            //    } else {
                            //        DataSourceName = GenericController.encodeText(core.db.getDataRowColumnName(rs.Rows[0], "Name"));
                            //    }
                            //    rs.Dispose();
                            //}
                            //
                            // Get the installedByCollectionId
                            //
                            int InstalledByCollectionID = 0;
                            if (!string.IsNullOrEmpty(installedByCollectionGuid)) {
                                var addonCollection = AddonCollection.create(core, installedByCollectionGuid);
                                if ( addonCollection != null ) {
                                    InstalledByCollectionID = addonCollection.id;
                                }
                                //rs = core.db.executeQuery("Select id from ccAddonCollections where ccguid=" + core.db.encodeSQLText(installedByCollectionGuid) + ";");
                                //if (DbController.isDataTableOk(rs)) {
                                //    InstalledByCollectionID = GenericController.encodeInteger(core.db.getDataRowColumnName(rs.Rows[0], "Id"));
                                //}
                                //rs.Dispose();
                            }
                            //
                            // Create or update the Table Field
                            //
                            if (fieldTypeId == FieldTypeIdRedirect) {
                                //
                                // Redirect Field
                                //
                            } else if (fieldTypeId == FieldTypeIdManyToMany) {
                                //
                                // ManyToMany Field
                                //
                            } else {
                                //
                                // All other fields
                                //
                                core.db.createSQLTableField(DataSourceName, TableName, field.nameLc, fieldTypeId);
                            }
                            //
                            // create or update the field
                            //
                            SqlFieldListClass sqlList = new SqlFieldListClass();
                            sqlList.add("ACTIVE", core.db.encodeSQLBoolean(field.active));
                            sqlList.add("MODIFIEDBY", core.db.encodeSQLNumber(SystemMemberID));
                            sqlList.add("MODIFIEDDATE", core.db.encodeSQLDate(DateTime.Now));
                            sqlList.add("TYPE", core.db.encodeSQLNumber(fieldTypeId));
                            sqlList.add("CAPTION", core.db.encodeSQLText(FieldCaption));
                            sqlList.add("ReadOnly", core.db.encodeSQLBoolean(FieldReadOnly));
                            sqlList.add("REQUIRED", core.db.encodeSQLBoolean(FieldRequired));
                            sqlList.add("TEXTBUFFERED", SQLFalse);
                            sqlList.add("PASSWORD", core.db.encodeSQLBoolean(Password));
                            sqlList.add("EDITSORTPRIORITY", core.db.encodeSQLNumber(field.editSortPriority));
                            sqlList.add("ADMINONLY", core.db.encodeSQLBoolean(field.adminOnly));
                            sqlList.add("DEVELOPERONLY", core.db.encodeSQLBoolean(FieldDeveloperOnly));
                            sqlList.add("CONTENTCONTROLID", core.db.encodeSQLNumber(Models.Domain.CDefModel.getContentId(core, "Content Fields")));
                            sqlList.add("DefaultValue", core.db.encodeSQLText(DefaultValue));
                            sqlList.add("HTMLCONTENT", core.db.encodeSQLBoolean(HTMLContent));
                            sqlList.add("NOTEDITABLE", core.db.encodeSQLBoolean(NotEditable));
                            sqlList.add("AUTHORABLE", core.db.encodeSQLBoolean(FieldAuthorable));
                            sqlList.add("INDEXCOLUMN", core.db.encodeSQLNumber(field.indexColumn));
                            sqlList.add("INDEXWIDTH", core.db.encodeSQLText(AdminIndexWidth));
                            sqlList.add("INDEXSORTPRIORITY", core.db.encodeSQLNumber(AdminIndexSort));
                            sqlList.add("REDIRECTID", core.db.encodeSQLText(RedirectIDField));
                            sqlList.add("REDIRECTPATH", core.db.encodeSQLText(RedirectPath));
                            sqlList.add("UNIQUENAME", core.db.encodeSQLBoolean(UniqueName));
                            sqlList.add("RSSTITLEFIELD", core.db.encodeSQLBoolean(RSSTitle));
                            sqlList.add("RSSDESCRIPTIONFIELD", core.db.encodeSQLBoolean(RSSDescription));
                            sqlList.add("MEMBERSELECTGROUPID", core.db.encodeSQLNumber(MemberSelectGroupID));
                            sqlList.add("installedByCollectionId", core.db.encodeSQLNumber(InstalledByCollectionID));
                            sqlList.add("EDITTAB", core.db.encodeSQLText(EditTab));
                            sqlList.add("SCRAMBLE", core.db.encodeSQLBoolean(false));
                            sqlList.add("ISBASEFIELD", core.db.encodeSQLBoolean(IsBaseField));
                            sqlList.add("LOOKUPLIST", core.db.encodeSQLText(LookupList));
                            int RedirectContentID = 0;
                            int LookupContentID = 0;
                            //
                            // -- conditional fields
                            switch (fieldTypeId) {
                                case FieldTypeIdLookup:
                                    //
                                    // -- lookup field
                                    //
                                    if (!string.IsNullOrEmpty(LookupContentName)) {
                                        LookupContentID = Models.Domain.CDefModel.getContentId(core, LookupContentName);
                                        if (LookupContentID <= 0) {
                                            LogController.logError(core, "Could not create lookup field [" + field.nameLc + "] for content definition [" + ContentName + "] because no content definition was found For lookup-content [" + LookupContentName + "].");
                                        }
                                    }
                                    sqlList.add("LOOKUPCONTENTID", core.db.encodeSQLNumber(LookupContentID));
                                    break;
                                case FieldTypeIdManyToMany:
                                    //
                                    // -- many-to-many field
                                    //
                                    string ManyToManyContent = field.get_manyToManyContentName(core);
                                    if (!string.IsNullOrEmpty(ManyToManyContent)) {
                                        int ManyToManyContentID = Models.Domain.CDefModel.getContentId(core, ManyToManyContent);
                                        if (ManyToManyContentID <= 0) {
                                            LogController.logError(core, "Could not create many-to-many field [" + field.nameLc + "] for [" + ContentName + "] because no content definition was found For many-to-many-content [" + ManyToManyContent + "].");
                                        }
                                        sqlList.add("MANYTOMANYCONTENTID", core.db.encodeSQLNumber(ManyToManyContentID));
                                    }
                                    //
                                    string ManyToManyRuleContent = field.get_manyToManyRuleContentName(core);
                                    if (!string.IsNullOrEmpty(ManyToManyRuleContent)) {
                                        int ManyToManyRuleContentID = Models.Domain.CDefModel.getContentId(core, ManyToManyRuleContent);
                                        if (ManyToManyRuleContentID <= 0) {
                                            LogController.logError(core, "Could not create many-to-many field [" + field.nameLc + "] for [" + ContentName + "] because no content definition was found For many-to-many-rule-content [" + ManyToManyRuleContent + "].");
                                        }
                                        sqlList.add("MANYTOMANYRULECONTENTID", core.db.encodeSQLNumber(ManyToManyRuleContentID));
                                    }
                                    sqlList.add("MANYTOMANYRULEPRIMARYFIELD", core.db.encodeSQLText(field.ManyToManyRulePrimaryField));
                                    sqlList.add("MANYTOMANYRULESECONDARYFIELD", core.db.encodeSQLText(field.ManyToManyRuleSecondaryField));
                                    break;
                                case FieldTypeIdRedirect:
                                    //
                                    // -- redirect field
                                    if (!string.IsNullOrEmpty(RedirectContentName)) {
                                        RedirectContentID = Models.Domain.CDefModel.getContentId(core, RedirectContentName);
                                        if (RedirectContentID <= 0) {
                                            LogController.logError(core, "Could not create redirect field [" + field.nameLc + "] for Content Definition [" + ContentName + "] because no content definition was found For redirect-content [" + RedirectContentName + "].");
                                        }
                                    }
                                    sqlList.add("REDIRECTCONTENTID", core.db.encodeSQLNumber(RedirectContentID));
                                    break;
                            }
                            //
                            if (RecordID == 0) {
                                sqlList.add("NAME", core.db.encodeSQLText(field.nameLc));
                                sqlList.add("CONTENTID", core.db.encodeSQLNumber(ContentID));
                                sqlList.add("CREATEKEY", "0");
                                sqlList.add("DATEADDED", core.db.encodeSQLDate(DateTime.Now));
                                sqlList.add("CREATEDBY", core.db.encodeSQLNumber(SystemMemberID));
                                RecordID = core.db.insertTableRecordGetId("Default", "ccFields");
                            }
                            if (RecordID == 0) {
                                throw (new ApplicationException("Could Not create Field [" + field.nameLc + "] because insert into ccfields failed."));
                            } else {
                                core.db.updateTableRecord("Default", "ccFields", "ID=" + RecordID, sqlList);
                                ContentFieldModel.invalidateRecordCache(core, RecordID);
                            }
                            //
                        }
                    }
                }
                //
                // 20181007 - what if we didnt
                //if (!isNewFieldRecord) {
                //    core.cache.invalidateAll();
                //    core.doc.clearMetaData();
                //}
                //
                returnId = RecordID;
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnId;
        }
        //
        //=============================================================
        //
        //=============================================================
        //
        public static bool isContentFieldSupported(CoreController core, string ContentName, string FieldName) {
            bool returnOk = false;
            try {
                Models.Domain.CDefModel cdef;
                //
                cdef = Models.Domain.CDefModel.getCdef(core, ContentName);
                if (cdef != null) {
                    returnOk = cdef.fields.ContainsKey(FieldName.ToLower());
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnOk;
        }
        //
        //========================================================================
        // Get a tables first ContentID from Tablename
        //========================================================================
        //
        public static int getContentIDByTablename(CoreController core, string TableName) {
            int tempgetContentIDByTablename = 0;
            //
            string SQL = null;
            int CS = 0;
            //
            tempgetContentIDByTablename = -1;
            if (!string.IsNullOrEmpty(TableName)) {
                SQL = "select ContentControlID from " + TableName + " where contentcontrolid is not null order by contentcontrolid;";
                CS = core.db.csOpenSql(SQL,"Default", 1, 1);
                if (core.db.csOk(CS)) {
                    tempgetContentIDByTablename = core.db.csGetInteger(CS, "ContentControlID");
                }
                core.db.csClose(ref CS);
            }
            return tempgetContentIDByTablename;
        }
        //
        //========================================================================
        //
        public static string getContentControlCriteria(CoreController core, string ContentName) {
            return Models.Domain.CDefModel.getCdef(core, ContentName).legacyContentControlCriteria;
        }
        //
        //============================================================================================================
        //   the content control Id for a record, all its edit and archive records, and all its child records
        //   returns records affected
        //   the contentname contains the record, but we do not know that this is the contentcontrol for the record,
        //   read it first to main_Get the correct contentid
        //============================================================================================================
        //
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
                    RecordTableName = Models.Domain.CDefModel.getContentTablename(core, RecordContentName);
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
        //========================================================================
        //
        //========================================================================
        //
        public static string GetContentFieldProperty(CoreController core, string ContentName, string FieldName, string PropertyName) {
            string result = "";
            try {
                CDefModel Contentdefinition = CDefModel.getCdef(core, ContentName);
                if ((string.IsNullOrEmpty(FieldName)) || (Contentdefinition.fields.Count < 1)) {
                    throw (new ApplicationException("Content Name [" + GenericController.encodeText(ContentName) + "] or FieldName [" + FieldName + "] was not valid")); 
                } else {
                    foreach (KeyValuePair<string, Models.Domain.CDefFieldModel> keyValuePair in Contentdefinition.fields) {
                        Models.Domain.CDefFieldModel field = keyValuePair.Value;
                        if (FieldName.ToLower() == field.nameLc) {
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
                                    result = field.memberSelectGroupId_get( core ).ToString() ;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;


        }
        //
        //========================================================================
        //
        //========================================================================
        //
        public static string GetContentProperty(CoreController core, string ContentName, string PropertyName) {
            string result = "";
            Models.Domain.CDefModel Contentdefinition;
            //
            Contentdefinition = Models.Domain.CDefModel.getCdef(core, GenericController.encodeText(ContentName));
            switch (GenericController.vbUCase(GenericController.encodeText(PropertyName))) {
                case "CONTENTCONTROLCRITERIA":
                    result = Contentdefinition.legacyContentControlCriteria;
                    break;
                case "ACTIVEONLY":
                    result = Contentdefinition.activeOnly.ToString();
                    break;
                case "ADMINONLY":
                    result = Contentdefinition.adminOnly.ToString();
                    break;
                case "ALIASID":
                    result = Contentdefinition.aliasID;
                    break;
                case "ALIASNAME":
                    result = Contentdefinition.aliasName;
                    break;
                case "ALLOWADD":
                    result = Contentdefinition.allowAdd.ToString();
                    break;
                case "ALLOWDELETE":
                    result = Contentdefinition.allowDelete.ToString();
                    //Case "CHILDIDLIST"
                    //    main_result = Contentdefinition.ChildIDList
                    break;
                case "DATASOURCEID":
                    result = Contentdefinition.dataSourceId.ToString();
                    break;
                case "DEFAULTSORTMETHOD":
                    result = Contentdefinition.defaultSortMethod;
                    break;
                case "DEVELOPERONLY":
                    result = Contentdefinition.developerOnly.ToString();
                    break;
                case "FIELDCOUNT":
                    result = Contentdefinition.fields.Count.ToString();
                    //Case "FIELDPOINTER"
                    //    main_result = Contentdefinition.FieldPointer
                    break;
                case "ID":
                    result = Contentdefinition.id.ToString();
                    break;
                case "SUPPORTLEGACYCONTENTCONTROL":
                    result = Contentdefinition.supportLegacyContentControl.ToString();
                    break;
                case "NAME":
                    result = Contentdefinition.name;
                    break;
                case "PARENTID":
                    result = Contentdefinition.parentID.ToString();
                    //Case "SINGLERECORD"
                    //    main_result = Contentdefinition.SingleRecord
                    break;
                case "CONTENTTABLENAME":
                    result = Contentdefinition.tableName;
                    break;
                case "CONTENTDATASOURCENAME":
                    result = Contentdefinition.dataSourceName;
                    //Case "AUTHORINGTABLENAME"
                    //    result = Contentdefinition.AuthoringTableName
                    //Case "AUTHORINGDATASOURCENAME"
                    //    result = Contentdefinition.AuthoringDataSourceName
                    break;
                case "WHERECLAUSE":
                    result = Contentdefinition.whereClause;
                    //Case "ALLOWWORKFLOWAUTHORING"
                    //    result = Contentdefinition.AllowWorkflowAuthoring.ToString
                    break;
                case "DROPDOWNFIELDLIST":
                    result = Contentdefinition.dropDownFieldList;
                    break;
                case "SELECTFIELDLIST":
                    result = Contentdefinition.selectCommaList;
                    break;
                default:
                    //throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError14(MethodName, "Content Property [" & genericController.encodeText(PropertyName) & "] was not found in content [" & genericController.encodeText(ContentName) & "]")
                    break;
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public static CDefModel getCache(CoreController core, int contentId) {
            CDefModel result = null;
            try {
                try {
                    string cacheName = Controllers.CacheController.getCacheKey_forObject("cdef", contentId.ToString());
                    result = core.cache.getObject<Models.Domain.CDefModel>(cacheName);
                } catch (Exception ex) {
                    LogController.handleError( core,ex);
                }
            } catch (Exception) {}
            return result;
        }
        //
        //====================================================================================================
        //
        public static void setCache(CoreController core, int contentId, CDefModel cdef) {
            string cacheName = Controllers.CacheController.getCacheKey_forObject("cdef", contentId.ToString());
            //
            // -- make it dependant on cacheNameInvalidateAll. If invalidated, all cdef will invalidate
            List<string> dependantList = new List<string>();
            dependantList.Add(cacheNameInvalidateAll);
            core.cache.setObject(cacheName, cdef, dependantList);
        }
        //
        //====================================================================================================
        //
        public static void invalidateCache(CoreController core, int contentId) {
            string cacheName = Controllers.CacheController.getCacheKey_forObject("cdef", contentId.ToString());
            core.cache.invalidate(cacheName);
        }
        //
        //====================================================================================================
        //
        public static void invalidateCacheAll(CoreController core) {
            core.cache.invalidate(cacheNameInvalidateAll);
        }
    }
}
