
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.Complex {
    //
    // ---------------------------------------------------------------------------------------------------
    // ----- CDefType
    //       class not structure because it has to marshall to vb6
    // ---------------------------------------------------------------------------------------------------
    //
    [Serializable]
    public class cdefModel {
        //
        private const string cacheNameInvalidateAll = "cdefInvalidateAll";
        //
        public int Id { get; set; } // index in content table
        public string Name { get; set; } // Name of Content
        public string ContentTableName { get; set; } // the name of the content table
        public string ContentDataSourceName { get; set; }
        public bool AllowAdd { get; set; } // Allow adding records
        public bool AllowDelete { get; set; } // Allow deleting records
        public string WhereClause { get; set; } // Used to filter records in the admin area
        public string DefaultSortMethod { get; set; } // FieldName Direction, ....
        public bool ActiveOnly { get; set; } // When true
        public bool AdminOnly { get; set; } // Only allow administrators to modify content
        public bool DeveloperOnly { get; set; } // Only allow developers to modify content
        public string DropDownFieldList { get; set; } // String used to populate select boxes
        public string EditorGroupName { get; set; } // Group of members who administer Workflow Authoring
        public int dataSourceId { get; set; }
        private string _dataSourceName { get; set; } = "";
        public bool IgnoreContentControl { get; set; } // if true, all records in the source are in this content
        public string AliasName { get; set; } // Field Name of the required "name" field
        public string AliasID { get; set; } // Field Name of the required "id" field
        public bool AllowTopicRules { get; set; } // For admin edit page
        public bool AllowContentTracking { get; set; } // For admin edit page
        public bool AllowCalendarEvents { get; set; } // For admin edit page
        public bool dataChanged { get; set; }
        public bool includesAFieldChange { get; set; } // if any fields().changed, this is set true to
        public bool Active { get; set; }
        public bool AllowContentChildTool { get; set; }
        public bool IsModifiedSinceInstalled { get; set; }
        public string IconLink { get; set; }
        public int IconWidth { get; set; }
        public int IconHeight { get; set; }
        public int IconSprites { get; set; }
        public string guid { get; set; }
        public bool IsBaseContent { get; set; }
        public string installedByCollectionGuid { get; set; }
        public int parentID { get; set; } // read from Db, if not IgnoreContentControl, the ID of the parent content
        public string parentName { get; set; } // read from xml, used to set parentId
        public string TimeStamp { get; set; } // string that changes if any record in Content Definition changes, in memory only
        public Dictionary<string, Models.Complex.cdefFieldModel> fields { get; set; } = new Dictionary<string, Models.Complex.cdefFieldModel>();
        public SortedList<string, CDefAdminColumnClass> adminColumns { get; set; } = new SortedList<string, CDefAdminColumnClass>();
        public string ContentControlCriteria { get; set; } // String created from ParentIDs used to select records
        public List<string> selectList { get; set; } = new List<string>();
        public string SelectCommaList { get; set; } // Field list used in OpenCSContent calls (all active field definitions)
                                                    //
                                                    //====================================================================================================
                                                    //
        public List<int> get_childIdList(coreClass cpCore) {
            if (_childIdList == null) {
                string Sql = "select id from cccontent where parentid=" + Id;
                DataTable dt = cpCore.db.executeQuery(Sql);
                if (dt.Rows.Count == 0) {
                    _childIdList = new List<int>();
                    foreach (DataRow parentrow in dt.Rows) {
                        _childIdList.Add(genericController.encodeInteger( parentrow[0]));
                    }
                }
                dt.Dispose();
            }
            return _childIdList;
        }
        public void set_childIdList(coreClass cpCore, List<int> value) {
            _childIdList = value;
        }
        private List<int> _childIdList = null;
        //
        //====================================================================================================
        // CDefAdminColumnType
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
        public static cdefModel create(coreClass cpcore, int contentId, bool loadInvalidFields = false, bool forceDbLoad = false) {
            cdefModel result = null;
            try {
                List<string> dependantCacheNameList = new List<string>();
                if (!forceDbLoad) {
                    result = getCache(cpcore, contentId);
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
                        + ", c.AllowTopicRules as AllowTopicRules"
                        + ", c.AllowContentTracking as AllowContentTracking"
                        + " from (((((ccContent c"
                        + " left join ccTables AS ContentTable ON c.ContentTableID = ContentTable.ID)"
                        + " left join ccTables AS AuthoringTable ON c.AuthoringTableID = AuthoringTable.ID)"
                        + " left join ccDataSources AS ContentDataSource ON ContentTable.DataSourceID = ContentDataSource.ID)"
                        + " left join ccDataSources AS AuthoringDataSource ON AuthoringTable.DataSourceID = AuthoringDataSource.ID)"
                        + " left join ccSortMethods ON c.DefaultSortMethodID = ccSortMethods.ID)"
                        + " left join ccGroups ON c.EditorGroupID = ccGroups.ID"
                        + " where (c.Active<>0)"
                        + " and(c.id=" + contentId.ToString() + ")";
                    DataTable dt = cpcore.db.executeQuery(sql);
                    if (dt.Rows.Count == 0) {
                        //
                        // cdef not found
                        //
                    } else {
                        result = new Models.Complex.cdefModel();
                        result.fields = new Dictionary<string, Models.Complex.cdefFieldModel>();
                        result.set_childIdList(cpcore, new List<int>());
                        result.selectList = new List<string>();
                        // -- !!!!! changed to string because dotnet json cannot serialize an integer key
                        result.adminColumns = new SortedList<string, Models.Complex.cdefModel.CDefAdminColumnClass>();
                        //
                        // ----- save values in definition
                        //
                        string contentName = null;
                        DataRow row = dt.Rows[0];
                        contentName = encodeText(genericController.encodeText(row[1])).Trim(' ');
                        string contentTablename = genericController.encodeText(row[10]);
                        result.Name = contentName;
                        result.Id = contentId;
                        result.AllowAdd = genericController.encodeBoolean(row[3]);
                        result.DeveloperOnly = genericController.encodeBoolean(row[4]);
                        result.AdminOnly = genericController.encodeBoolean(row[5]);
                        result.AllowDelete = genericController.encodeBoolean(row[6]);
                        result.parentID = genericController.encodeInteger(row[7]);
                        result.DropDownFieldList = genericController.vbUCase(genericController.encodeText(row[9]));
                        result.ContentTableName = genericController.encodeText(contentTablename);
                        result.ContentDataSourceName = "default";
                        result.AllowCalendarEvents = genericController.encodeBoolean(row[15]);
                        result.DefaultSortMethod = genericController.encodeText(row[17]);
                        if (string.IsNullOrEmpty(result.DefaultSortMethod)) {
                            result.DefaultSortMethod = "name";
                        }
                        result.EditorGroupName = genericController.encodeText(row[18]);
                        result.AllowContentTracking = genericController.encodeBoolean(row[19]);
                        result.AllowTopicRules = genericController.encodeBoolean(row[20]);
                        // .AllowMetaContent = genericController.EncodeBoolean(row[21])
                        //
                        result.ActiveOnly = true;
                        result.AliasID = "ID";
                        result.AliasName = "NAME";
                        result.IgnoreContentControl = false;
                        //
                        // load parent cdef fields first so we can overlay the current cdef field
                        //
                        if (result.parentID == 0) {
                            result.parentID = -1;
                        } else {
                            Models.Complex.cdefModel parentCdef = create(cpcore, result.parentID, loadInvalidFields, forceDbLoad);
                            foreach (var keyvaluepair in parentCdef.fields) {
                                Models.Complex.cdefFieldModel parentField = keyvaluepair.Value;
                                Models.Complex.cdefFieldModel childField = new Models.Complex.cdefFieldModel();
                                childField = (Models.Complex.cdefFieldModel)parentField.Clone();
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
                        dt = cpcore.db.executeQuery(sql);
                        if (dt.Rows.Count == 0) {
                            //
                        } else {
                            List<string> usedFields = new List<string>();
                            foreach (DataRow rowWithinLoop in dt.Rows) {
                                row = rowWithinLoop;
                                string fieldName = genericController.encodeText(rowWithinLoop[13]);
                                int fieldId = genericController.encodeInteger(rowWithinLoop[12]);
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
                                    Models.Complex.cdefFieldModel field = new Models.Complex.cdefFieldModel();
                                    int fieldIndexColumn = -1;
                                    int fieldTypeId = genericController.encodeInteger(rowWithinLoop[15]);
                                    if (genericController.encodeText(rowWithinLoop[4]) != "") {
                                        fieldIndexColumn = genericController.encodeInteger(rowWithinLoop[4]);
                                    }
                                    //
                                    // translate htmlContent to fieldtypehtml
                                    //   this is also converted in upgrade, daily housekeep, addon install
                                    //
                                    bool fieldHtmlContent = genericController.encodeBoolean(rowWithinLoop[25]);
                                    if (fieldHtmlContent) {
                                        if (fieldTypeId == FieldTypeIdLongText) {
                                            fieldTypeId = FieldTypeIdHTML;
                                        } else if (fieldTypeId == FieldTypeIdFileText) {
                                            fieldTypeId = FieldTypeIdFileHTML;
                                        }
                                    }
                                    field.active = genericController.encodeBoolean(rowWithinLoop[24]);
                                    field.adminOnly = genericController.encodeBoolean(rowWithinLoop[8]);
                                    field.authorable = genericController.encodeBoolean(rowWithinLoop[27]);
                                    field.blockAccess = genericController.encodeBoolean(rowWithinLoop[38]);
                                    field.caption = genericController.encodeText(rowWithinLoop[16]);
                                    field.dataChanged = false;
                                    //.Changed
                                    field.contentId = contentId;
                                    field.defaultValue = genericController.encodeText(rowWithinLoop[22]);
                                    field.developerOnly = genericController.encodeBoolean(rowWithinLoop[0]);
                                    field.editSortPriority = genericController.encodeInteger(rowWithinLoop[10]);
                                    field.editTabName = genericController.encodeText(rowWithinLoop[34]);
                                    field.fieldTypeId = fieldTypeId;
                                    field.htmlContent = fieldHtmlContent;
                                    field.id = fieldId;
                                    field.indexColumn = fieldIndexColumn;
                                    field.indexSortDirection = genericController.encodeInteger(rowWithinLoop[7]);
                                    field.indexSortOrder = genericController.encodeInteger(rowWithinLoop[6]);
                                    field.indexWidth = genericController.encodeText(genericController.encodeInteger(genericController.encodeText(rowWithinLoop[5]).Replace("%", "")));
                                    field.inherited = false;
                                    field.installedByCollectionGuid = genericController.encodeText(rowWithinLoop[39]);
                                    field.isBaseField = genericController.encodeBoolean(rowWithinLoop[38]);
                                    field.isModifiedSinceInstalled = false;
                                    field.lookupContentID = genericController.encodeInteger(rowWithinLoop[18]);
                                    //.lookupContentName = ""
                                    field.lookupList = genericController.encodeText(rowWithinLoop[37]);
                                    field.manyToManyContentID = genericController.encodeInteger(rowWithinLoop[28]);
                                    field.manyToManyRuleContentID = genericController.encodeInteger(rowWithinLoop[29]);
                                    field.ManyToManyRulePrimaryField = genericController.encodeText(rowWithinLoop[30]);
                                    field.ManyToManyRuleSecondaryField = genericController.encodeText(rowWithinLoop[31]);
                                    field.memberSelectGroupId_set( cpcore, genericController.encodeInteger(rowWithinLoop[36]));
                                    field.nameLc = fieldNameLower;
                                    field.notEditable = genericController.encodeBoolean(rowWithinLoop[26]);
                                    field.password = genericController.encodeBoolean(rowWithinLoop[3]);
                                    field.readOnly = genericController.encodeBoolean(rowWithinLoop[17]);
                                    field.redirectContentID = genericController.encodeInteger(rowWithinLoop[19]);
                                    //.RedirectContentName(cpCore) = ""
                                    field.redirectID = genericController.encodeText(rowWithinLoop[21]);
                                    field.redirectPath = genericController.encodeText(rowWithinLoop[20]);
                                    field.required = genericController.encodeBoolean(rowWithinLoop[14]);
                                    field.RSSTitleField = genericController.encodeBoolean(rowWithinLoop[32]);
                                    field.RSSDescriptionField = genericController.encodeBoolean(rowWithinLoop[33]);
                                    field.Scramble = genericController.encodeBoolean(rowWithinLoop[35]);
                                    field.textBuffered = genericController.encodeBoolean(rowWithinLoop[2]);
                                    field.uniqueName = genericController.encodeBoolean(rowWithinLoop[1]);
                                    //.ValueVariant
                                    //
                                    field.HelpCustom = genericController.encodeText(rowWithinLoop[41]);
                                    field.HelpDefault = genericController.encodeText(rowWithinLoop[40]);
                                    if (string.IsNullOrEmpty(field.HelpCustom)) {
                                        field.helpMessage = field.HelpDefault;
                                    } else {
                                        field.helpMessage = field.HelpCustom;
                                    }
                                    field.HelpChanged = false;
                                    dt.Dispose();
                                    result.fields.Add(fieldNameLower, field);
                                    //REFACTOR
                                    if ((field.fieldTypeId != FieldTypeIdManyToMany) & (field.fieldTypeId != FieldTypeIdRedirect) && (!result.selectList.Contains(fieldNameLower))) {
                                        //
                                        // add only fields that can be selected
                                        result.selectList.Add(fieldNameLower);
                                    }
                                }
                            }
                            result.SelectCommaList = string.Join(",", result.selectList);
                        }
                        //
                        // ----- Create the ContentControlCriteria
                        //
                        result.ContentControlCriteria = Models.Complex.cdefModel.getContentControlCriteria(cpcore, result.Id, result.ContentTableName, result.ContentDataSourceName, new List<int>());
                        //
                        getCdef_SetAdminColumns(cpcore, result);
                    }
                    setCache(cpcore, contentId, result);
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        private static void getCdef_SetAdminColumns(coreClass cpcore, Models.Complex.cdefModel cdef) {
            try {
                bool FieldActive = false;
                int FieldWidth = 0;
                int FieldWidthTotal = 0;
                Models.Complex.cdefModel.CDefAdminColumnClass adminColumn = null;
                //
                if (cdef.Id > 0) {
                    int cnt = 0;
                    foreach (KeyValuePair<string, Models.Complex.cdefFieldModel> keyValuePair in cdef.fields) {
                        Models.Complex.cdefFieldModel field = keyValuePair.Value;
                        FieldActive = field.active;
                        FieldWidth = genericController.encodeInteger(field.indexWidth);
                        if (FieldActive && (FieldWidth > 0)) {
                            FieldWidthTotal = FieldWidthTotal + FieldWidth;
                            adminColumn = new Models.Complex.cdefModel.CDefAdminColumnClass();
                            adminColumn.Name = field.nameLc;
                            adminColumn.SortDirection = field.indexSortDirection;
                            adminColumn.SortPriority = genericController.encodeInteger(field.indexSortOrder);
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
                                adminColumn = new Models.Complex.cdefModel.CDefAdminColumnClass();
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
                cpcore.handleException(ex);
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
        public static int getContentId(coreClass cpcore, string contentName) {
            int returnId = 0;
            try {
                if (cpcore.doc.contentNameIdDictionary.ContainsKey(contentName.ToLower())) {
                    returnId = cpcore.doc.contentNameIdDictionary[contentName.ToLower()];
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
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
        public static Models.Complex.cdefModel getCdef(coreClass cpcore, string contentName) {
            Models.Complex.cdefModel returnCdef = null;
            try {
                int ContentId = getContentId(cpcore, contentName);
                if (ContentId > 0) {
                    returnCdef = getCdef(cpcore, ContentId);
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
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
        public static Models.Complex.cdefModel getCdef(coreClass cpcore, int contentId, bool forceDbLoad = false, bool loadInvalidFields = false) {
            Models.Complex.cdefModel returnCdef = null;
            try {
                if (contentId <= 0) {
                    //
                    // -- invalid id                    
                } else if ((!forceDbLoad) && (cpcore.doc.cdefDictionary.ContainsKey(contentId.ToString()))) {
                    //
                    // -- already loaded and no force re-load, just return the current cdef                    
                    returnCdef = cpcore.doc.cdefDictionary[contentId.ToString()];
                } else {
                    if (cpcore.doc.cdefDictionary.ContainsKey(contentId.ToString())) {
                        //
                        // -- key is already there, remove it first                        
                        cpcore.doc.cdefDictionary.Remove(contentId.ToString());
                    }
                    returnCdef = Models.Complex.cdefModel.create(cpcore, contentId, loadInvalidFields, forceDbLoad);
                    cpcore.doc.cdefDictionary.Add(contentId.ToString(), returnCdef);
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
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
        internal static string getContentControlCriteria(coreClass cpcore, int contentId, string contentTableName, string contentDAtaSourceName, List<int> parentIdList) {
            string returnCriteria = "";
            try {
                //
                returnCriteria = "(1=0)";
                if (contentId >= 0) {
                    if (!parentIdList.Contains(contentId)) {
                        parentIdList.Add(contentId);
                        returnCriteria = "(" + contentTableName + ".contentcontrolId=" + contentId + ")";
                        foreach (KeyValuePair<int, contentModel> kvp in cpcore.doc.contentIdDict) {
                            if (kvp.Value.ParentID == contentId) {
                                returnCriteria += "OR" + getContentControlCriteria(cpcore, kvp.Value.id, contentTableName, contentDAtaSourceName, parentIdList);
                            }
                        }
                        parentIdList.Remove(contentId);
                        returnCriteria = "(" + returnCriteria + ")";
                    }
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
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
        public static bool isWithinContent(coreClass cpcore, int ChildContentID, int ParentContentID) {
            bool returnOK = false;
            try {
                Models.Complex.cdefModel cdef = null;
                if (ChildContentID == ParentContentID) {
                    returnOK = true;
                } else {
                    cdef = getCdef(cpcore, ParentContentID);
                    if (cdef != null) {
                        if (cdef.get_childIdList(cpcore).Count > 0) {
                            returnOK = cdef.get_childIdList(cpcore).Contains(ChildContentID);
                            if (!returnOK) {
                                foreach (int contentId in cdef.get_childIdList(cpcore)) {
                                    returnOK = isWithinContent(cpcore, contentId, ParentContentID);
                                    if (returnOK) {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
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
        public static List<int> getEditableCdefIdList(coreClass cpcore) {
            List<int> returnList = new List<int>();
            try {
                string SQL = null;
                DataTable cidDataTable = null;
                int CIDCount = 0;
                int CIDPointer = 0;
                Models.Complex.cdefModel CDef = null;
                int ContentID = 0;
                //
                SQL = "Select ccGroupRules.ContentID as ID"
                + " FROM ((ccmembersrules"
                + " Left Join ccGroupRules on ccMemberRules.GroupID=ccGroupRules.GroupID)"
                + " Left Join ccContent on ccGroupRules.ContentID=ccContent.ID)"
                + " WHERE"
                    + " (ccMemberRules.MemberID=" + cpcore.doc.sessionContext.user.id + ")"
                    + " AND(ccGroupRules.Active<>0)"
                    + " AND(ccContent.Active<>0)"
                    + " AND(ccMemberRules.Active<>0)";
                cidDataTable = cpcore.db.executeQuery(SQL);
                CIDCount = cidDataTable.Rows.Count;
                for (CIDPointer = 0; CIDPointer < CIDCount; CIDPointer++) {
                    ContentID = genericController.encodeInteger(cidDataTable.Rows[CIDPointer][0]);
                    returnList.Add(ContentID);
                    CDef = getCdef(cpcore, ContentID);
                    if (CDef != null) {
                        returnList.AddRange(CDef.get_childIdList(cpcore));
                    }
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
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
        public static void createContentChild(coreClass cpcore, string ChildContentName, string ParentContentName, int MemberID) {
            try {
                string DataSourceName = "";
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
                SQL = "select ID from ccContent where name=" + cpcore.db.encodeSQLText(ChildContentName) + ";";
                rs = cpcore.db.executeQuery(SQL);
                if (isDataTableOk(rs)) {
                    ChildContentID = genericController.encodeInteger(cpcore.db.getDataRowColumnName(rs.Rows[0], "ID"));
                    //
                    // mark the record touched so upgrade will not delete it
                    //
                    cpcore.db.executeQuery("update ccContent set CreateKey=0 where ID=" + ChildContentID);
                }
                closeDataTable(rs);
                if (ChildContentID == 0) {
                    //
                    // Get ContentID of parent
                    //
                    SQL = "select ID from ccContent where name=" + cpcore.db.encodeSQLText(ParentContentName) + ";";
                    rs = cpcore.db.executeQuery(SQL, DataSourceName);
                    if (isDataTableOk(rs)) {
                        ParentContentID = genericController.encodeInteger(cpcore.db.getDataRowColumnName(rs.Rows[0], "ID"));
                        //
                        // mark the record touched so upgrade will not delete it
                        //
                        cpcore.db.executeQuery("update ccContent set CreateKey=0 where ID=" + ParentContentID);
                    }
                    closeDataTable(rs);
                    //
                    if (ParentContentID == 0) {
                        throw (new ApplicationException("Can not create Child Content [" + ChildContentName + "] because the Parent Content [" + ParentContentName + "] was not found."));
                    } else {
                        //
                        // ----- create child content record, let the csv_ExecuteSQL reload CDef
                        //
                        DataSourceName = "Default";
                        CSContent = cpcore.db.cs_openContentRecord("Content", ParentContentID);
                        if (!cpcore.db.csOk(CSContent)) {
                            throw (new ApplicationException("Can not create Child Content [" + ChildContentName + "] because the Parent Content [" + ParentContentName + "] was not found."));
                        } else {
                            SelectFieldList = cpcore.db.cs_getSelectFieldList(CSContent);
                            if (string.IsNullOrEmpty(SelectFieldList)) {
                                throw (new ApplicationException("Can not create Child Content [" + ChildContentName + "] because the Parent Content [" + ParentContentName + "] record has not fields."));
                            } else {
                                CSNew = cpcore.db.csInsertRecord("Content", 0);
                                if (!cpcore.db.csOk(CSNew)) {
                                    throw (new ApplicationException("Can not create Child Content [" + ChildContentName + "] because there was an error creating a new record in ccContent."));
                                } else {
                                    Fields = SelectFieldList.Split(',');
                                    DateNow = DateTime.Now;
                                    for (var FieldPointer = 0; FieldPointer <= Fields.GetUpperBound(0); FieldPointer++) {
                                        FieldName = Fields[FieldPointer];
                                        switch (genericController.vbUCase(FieldName)) {
                                            case "ID":
                                                // do nothing
                                                break;
                                            case "NAME":
                                                cpcore.db.csSet(CSNew, FieldName, ChildContentName);
                                                break;
                                            case "PARENTID":
                                                cpcore.db.csSet(CSNew, FieldName, cpcore.db.csGetText(CSContent, "ID"));
                                                break;
                                            case "CREATEDBY":
                                            case "MODIFIEDBY":
                                                cpcore.db.csSet(CSNew, FieldName, MemberID);
                                                break;
                                            case "DATEADDED":
                                            case "MODIFIEDDATE":
                                                cpcore.db.csSet(CSNew, FieldName, DateNow);
                                                break;
                                            case "CCGUID":

                                                //
                                                // new, non-blank guid so if this cdef is exported, it will be updateable
                                                //
                                                cpcore.db.csSet(CSNew, FieldName, createGuid());
                                                break;
                                            default:
                                                cpcore.db.csSet(CSNew, FieldName, cpcore.db.csGetText(CSContent, FieldName));
                                                break;
                                        }
                                    }
                                }
                                cpcore.db.csClose(ref CSNew);
                            }
                        }
                        cpcore.db.csClose(ref CSContent);
                    }
                }
                //
                // ----- Load CDef
                //
                cpcore.cache.invalidateAll();
                cpcore.doc.clearMetaData();
            } catch (Exception ex) {
                cpcore.handleException(ex);
            }
        }
        //
        //========================================================================
        // Get a Contents Tablename from the ContentPointer
        //========================================================================
        //
        public static string getContentTablename(coreClass cpcore, string ContentName) {
            string returnTableName = "";
            try {
                Models.Complex.cdefModel CDef;
                //
                CDef = Models.Complex.cdefModel.getCdef(cpcore, ContentName);
                if (CDef != null) {
                    returnTableName = CDef.ContentTableName;
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return returnTableName;
        }
        //
        //========================================================================
        // ----- Get a DataSource Name from its ContentName
        //
        public static string getContentDataSource(coreClass cpcore, string ContentName) {
            string returnDataSource = "";
            try {
                Models.Complex.cdefModel CDef;
                //
                CDef = Models.Complex.cdefModel.getCdef(cpcore, ContentName);
                if (CDef == null) {
                    //
                } else {
                    returnDataSource = CDef.ContentDataSourceName;
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
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
        public static string getContentNameByID(coreClass cpcore, int ContentID) {
            string returnName = "";
            try {
                Models.Complex.cdefModel cdef;
                //
                cdef = Models.Complex.cdefModel.getCdef(cpcore, ContentID);
                if (cdef != null) {
                    returnName = cdef.Name;
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return returnName;
        }

        //========================================================================
        //   Create a content definition
        //       called from upgrade and DeveloperTools
        //========================================================================
        //
        public static int addContent(coreClass cpcore, bool Active, dataSourceModel datasource, string TableName, string contentName, bool AdminOnly = false, bool DeveloperOnly = false, bool AllowAdd = true, bool AllowDelete = true, string ParentName = "", string DefaultSortMethod = "", string DropDownFieldList = "", bool AllowWorkflowAuthoring = false, bool AllowCalendarEvents = false, bool AllowContentTracking = false, bool AllowTopicRules = false, bool AllowContentChildTool = false, bool ignore1 = false, string IconLink = "", int IconWidth = 0, int IconHeight = 0, int IconSprites = 0, string ccGuid = "", bool IsBaseContent = false, string installedByCollectionGuid = "", bool clearMetaCache = false) {
            int returnContentId = 0;
            try {
                //
                bool ContentIsBaseContent = false;
                string NewGuid = null;
                string LcContentGuid = null;
                string SQL = null;
                int parentId = 0;
                DataTable dt = null;
                int TableID = 0;
                string iDefaultSortMethod = null;
                int DefaultSortMethodID = 0;
                bool CDefFound = false;
                int InstalledByCollectionID = 0;
                sqlFieldListClass sqlList = null;
                Models.Complex.cdefFieldModel field = null;
                int ContentIDofContent = 0;
                //
                if (string.IsNullOrEmpty(contentName)) {
                    throw new ApplicationException("contentName can not be blank");
                } else {
                    //
                    if (string.IsNullOrEmpty(TableName)) {
                        throw new ApplicationException("Tablename can not be blank");
                    } else {
                        //
                        // Create the SQL table
                        //
                        cpcore.db.createSQLTable(datasource.Name, TableName);
                        //
                        // Check for a Content Definition
                        //
                        returnContentId = 0;
                        LcContentGuid = "";
                        ContentIsBaseContent = false;
                        NewGuid = encodeEmptyText(ccGuid, "");
                        //
                        // get contentId, guid, IsBaseContent
                        //
                        SQL = "select ID,ccguid,IsBaseContent from ccContent where (name=" + cpcore.db.encodeSQLText(contentName) + ") order by id;";
                        dt = cpcore.db.executeQuery(SQL);
                        if (dt.Rows.Count > 0) {
                            returnContentId = genericController.encodeInteger(dt.Rows[0]["ID"]);
                            LcContentGuid = genericController.vbLCase(genericController.encodeText(dt.Rows[0]["ccguid"]));
                            ContentIsBaseContent = genericController.encodeBoolean(dt.Rows[0]["IsBaseContent"]);
                        }
                        dt.Dispose();
                        //
                        // get contentid of content
                        //
                        ContentIDofContent = 0;
                        if (contentName.ToLower() == "content") {
                            ContentIDofContent = returnContentId;
                        } else {
                            SQL = "select ID from ccContent where (name='content') order by id;";
                            dt = cpcore.db.executeQuery(SQL);
                            if (dt.Rows.Count > 0) {
                                ContentIDofContent = genericController.encodeInteger(dt.Rows[0]["ID"]);
                            }
                            dt.Dispose();
                        }
                        //
                        // get parentId
                        //
                        if (!string.IsNullOrEmpty(ParentName)) {
                            SQL = "select id from ccContent where (name=" + cpcore.db.encodeSQLText(ParentName) + ") order by id;";
                            dt = cpcore.db.executeQuery(SQL);
                            if (dt.Rows.Count > 0) {
                                parentId = genericController.encodeInteger(dt.Rows[0][0]);
                            }
                            dt.Dispose();
                        }
                        //
                        // get InstalledByCollectionID
                        //
                        InstalledByCollectionID = 0;
                        if (!string.IsNullOrEmpty(installedByCollectionGuid)) {
                            SQL = "select id from ccAddonCollections where ccGuid=" + cpcore.db.encodeSQLText(installedByCollectionGuid);
                            dt = cpcore.db.executeQuery(SQL);
                            if (dt.Rows.Count > 0) {
                                InstalledByCollectionID = genericController.encodeInteger(dt.Rows[0]["ID"]);
                            }
                        }
                        //
                        // Block non-base update of a base field
                        //
                        if (ContentIsBaseContent && !IsBaseContent) {
                            throw new ApplicationException("Attempt to update a Base Content Definition [" + contentName + "] as non-base. This is not allowed.");
                        } else {
                            CDefFound = (returnContentId != 0);
                            if (!CDefFound) {
                                //
                                // ----- Create a new empty Content Record (to get ContentID)
                                //
                                returnContentId = cpcore.db.insertTableRecordGetId("Default", "ccContent", SystemMemberID);
                            }
                            //
                            // ----- Get the Table Definition ID, create one if missing
                            //
                            SQL = "SELECT ID from ccTables where (active<>0) and (name=" + cpcore.db.encodeSQLText(TableName) + ");";
                            dt = cpcore.db.executeQuery(SQL);
                            if (dt.Rows.Count <= 0) {
                                //
                                // ----- no table definition found, create one
                                //
                                //If genericController.vbUCase(DataSourceName) = "DEFAULT" Then
                                //    DataSourceID = -1
                                //ElseIf DataSourceName = "" Then
                                //    DataSourceID = -1
                                //Else
                                //    DataSourceID = cpCore.db.getDataSourceId(DataSourceName)
                                //    If DataSourceID = -1 Then
                                //        throw (New ApplicationException("Could not find DataSource [" & DataSourceName & "] for table [" & TableName & "]"))
                                //    End If
                                //End If
                                TableID = cpcore.db.insertTableRecordGetId("Default", "ccTables", SystemMemberID);
                                //
                                sqlList = new sqlFieldListClass();
                                sqlList.add("name", cpcore.db.encodeSQLText(TableName));
                                sqlList.add("active", SQLTrue);
                                sqlList.add("DATASOURCEID", cpcore.db.encodeSQLNumber(datasource.ID));
                                sqlList.add("CONTENTCONTROLID", cpcore.db.encodeSQLNumber(Models.Complex.cdefModel.getContentId(cpcore, "Tables")));
                                //
                                cpcore.db.updateTableRecord("Default", "ccTables", "ID=" + TableID, sqlList);
                            } else {
                                TableID = genericController.encodeInteger(dt.Rows[0]["ID"]);
                            }
                            //
                            // ----- Get Sort Method ID from SortMethod
                            iDefaultSortMethod = encodeEmptyText(DefaultSortMethod, "");
                            DefaultSortMethodID = 0;
                            //
                            // First - try lookup by name
                            //
                            if (string.IsNullOrEmpty(iDefaultSortMethod)) {
                                DefaultSortMethodID = 0;
                            } else {
                                dt = cpcore.db.openTable("Default", "ccSortMethods", "(name=" + cpcore.db.encodeSQLText(iDefaultSortMethod) + ")and(active<>0)", "ID", "ID", 1, 1);
                                if (dt.Rows.Count > 0) {
                                    DefaultSortMethodID = genericController.encodeInteger(dt.Rows[0]["ID"]);
                                }
                            }
                            if (DefaultSortMethodID == 0) {
                                //
                                // fallback - maybe they put the orderbyclause in (common mistake)
                                //
                                dt = cpcore.db.openTable("Default", "ccSortMethods", "(OrderByClause=" + cpcore.db.encodeSQLText(iDefaultSortMethod) + ")and(active<>0)", "ID", "ID", 1, 1);
                                if (dt.Rows.Count > 0) {
                                    DefaultSortMethodID = genericController.encodeInteger(dt.Rows[0]["ID"]);
                                }
                            }
                            //
                            // determine parentId from parentName
                            //

                            //
                            // ----- update record
                            //
                            sqlList = new sqlFieldListClass();
                            sqlList.add("name", cpcore.db.encodeSQLText(contentName));
                            sqlList.add("CREATEKEY", "0");
                            sqlList.add("active", cpcore.db.encodeSQLBoolean(Active));
                            sqlList.add("ContentControlID", cpcore.db.encodeSQLNumber(ContentIDofContent));
                            sqlList.add("AllowAdd", cpcore.db.encodeSQLBoolean(AllowAdd));
                            sqlList.add("AllowDelete", cpcore.db.encodeSQLBoolean(AllowDelete));
                            sqlList.add("AllowWorkflowAuthoring", cpcore.db.encodeSQLBoolean(AllowWorkflowAuthoring));
                            sqlList.add("DeveloperOnly", cpcore.db.encodeSQLBoolean(DeveloperOnly));
                            sqlList.add("AdminOnly", cpcore.db.encodeSQLBoolean(AdminOnly));
                            sqlList.add("ParentID", cpcore.db.encodeSQLNumber(parentId));
                            sqlList.add("DefaultSortMethodID", cpcore.db.encodeSQLNumber(DefaultSortMethodID));
                            sqlList.add("DropDownFieldList", cpcore.db.encodeSQLText(encodeEmptyText(DropDownFieldList, "Name")));
                            sqlList.add("ContentTableID", cpcore.db.encodeSQLNumber(TableID));
                            sqlList.add("AuthoringTableID", cpcore.db.encodeSQLNumber(TableID));
                            sqlList.add("ModifiedDate", cpcore.db.encodeSQLDate(DateTime.Now));
                            sqlList.add("CreatedBy", cpcore.db.encodeSQLNumber(SystemMemberID));
                            sqlList.add("ModifiedBy", cpcore.db.encodeSQLNumber(SystemMemberID));
                            sqlList.add("AllowCalendarEvents", cpcore.db.encodeSQLBoolean(AllowCalendarEvents));
                            sqlList.add("AllowContentTracking", cpcore.db.encodeSQLBoolean(AllowContentTracking));
                            sqlList.add("AllowTopicRules", cpcore.db.encodeSQLBoolean(AllowTopicRules));
                            sqlList.add("AllowContentChildTool", cpcore.db.encodeSQLBoolean(AllowContentChildTool));
                            //Call sqlList.add("AllowMetaContent", cpCore.db.encodeSQLBoolean(ignore1))
                            sqlList.add("IconLink", cpcore.db.encodeSQLText(encodeEmptyText(IconLink, "")));
                            sqlList.add("IconHeight", cpcore.db.encodeSQLNumber(IconHeight));
                            sqlList.add("IconWidth", cpcore.db.encodeSQLNumber(IconWidth));
                            sqlList.add("IconSprites", cpcore.db.encodeSQLNumber(IconSprites));
                            sqlList.add("installedByCollectionid", cpcore.db.encodeSQLNumber(InstalledByCollectionID));
                            if ((string.IsNullOrEmpty(LcContentGuid)) && (!string.IsNullOrEmpty(NewGuid))) {
                                //
                                // hard one - only update guid if the tables supports it, and it the new guid is not blank
                                // if the new guid does no match te old guid
                                //
                                sqlList.add("ccGuid", cpcore.db.encodeSQLText(NewGuid));
                            } else if ((!string.IsNullOrEmpty(NewGuid)) & (LcContentGuid != genericController.vbLCase(NewGuid))) {
                                //
                                // installing content definition with matching name, but different guid -- this is an error that needs to be fixed
                                //
                                cpcore.handleException(new ApplicationException("createContent call, content.name match found but content.ccGuid did not, name [" + contentName + "], newGuid [" + NewGuid + "], installedGuid [" + LcContentGuid + "] "));
                            }
                            cpcore.db.updateTableRecord("Default", "ccContent", "ID=" + returnContentId, sqlList);
                            //
                            //-----------------------------------------------------------------------------------------------
                            // Verify Core Content Definition Fields
                            //-----------------------------------------------------------------------------------------------
                            //
                            if (parentId < 1) {
                                //
                                // CDef does not inherit its fields, create what is needed for a non-inherited CDef
                                //
                                if (!cpcore.db.isCdefField(returnContentId, "ID")) {
                                    field = new Models.Complex.cdefFieldModel();
                                    field.nameLc = "id";
                                    field.active = true;
                                    field.fieldTypeId = FieldTypeIdAutoIdIncrement;
                                    field.editSortPriority = 100;
                                    field.authorable = false;
                                    field.caption = "ID";
                                    field.defaultValue = "";
                                    field.isBaseField = IsBaseContent;
                                    verifyCDefField_ReturnID(cpcore, contentName, field);
                                }
                                //
                                if (!cpcore.db.isCdefField(returnContentId, "name")) {
                                    field = new Models.Complex.cdefFieldModel();
                                    field.nameLc = "name";
                                    field.active = true;
                                    field.fieldTypeId = FieldTypeIdText;
                                    field.editSortPriority = 110;
                                    field.authorable = true;
                                    field.caption = "Name";
                                    field.defaultValue = "";
                                    field.isBaseField = IsBaseContent;
                                    verifyCDefField_ReturnID(cpcore, contentName, field);
                                }
                                //
                                if (!cpcore.db.isCdefField(returnContentId, "active")) {
                                    field = new Models.Complex.cdefFieldModel();
                                    field.nameLc = "active";
                                    field.active = true;
                                    field.fieldTypeId = FieldTypeIdBoolean;
                                    field.editSortPriority = 200;
                                    field.authorable = true;
                                    field.caption = "Active";
                                    field.defaultValue = "1";
                                    field.isBaseField = IsBaseContent;
                                    verifyCDefField_ReturnID(cpcore, contentName, field);
                                }
                                //
                                if (!cpcore.db.isCdefField(returnContentId, "sortorder")) {
                                    field = new Models.Complex.cdefFieldModel();
                                    field.nameLc = "sortorder";
                                    field.active = true;
                                    field.fieldTypeId = FieldTypeIdText;
                                    field.editSortPriority = 2000;
                                    field.authorable = false;
                                    field.caption = "Alpha Sort Order";
                                    field.defaultValue = "";
                                    field.isBaseField = IsBaseContent;
                                    verifyCDefField_ReturnID(cpcore, contentName, field);
                                }
                                //
                                if (!cpcore.db.isCdefField(returnContentId, "dateadded")) {
                                    field = new Models.Complex.cdefFieldModel();
                                    field.nameLc = "dateadded";
                                    field.active = true;
                                    field.fieldTypeId = FieldTypeIdDate;
                                    field.editSortPriority = 9999;
                                    field.authorable = false;
                                    field.caption = "Date Added";
                                    field.defaultValue = "";
                                    field.isBaseField = IsBaseContent;
                                    verifyCDefField_ReturnID(cpcore, contentName, field);
                                }
                                if (!cpcore.db.isCdefField(returnContentId, "createdby")) {
                                    field = new Models.Complex.cdefFieldModel();
                                    field.nameLc = "createdby";
                                    field.active = true;
                                    field.fieldTypeId = FieldTypeIdLookup;
                                    field.editSortPriority = 9999;
                                    field.authorable = false;
                                    field.caption = "Created By";
                                    field.set_lookupContentName(cpcore, "People");
                                    field.defaultValue = "";
                                    field.isBaseField = IsBaseContent;
                                    verifyCDefField_ReturnID(cpcore, contentName, field);
                                }
                                if (!cpcore.db.isCdefField(returnContentId, "modifieddate")) {
                                    field = new Models.Complex.cdefFieldModel();
                                    field.nameLc = "modifieddate";
                                    field.active = true;
                                    field.fieldTypeId = FieldTypeIdDate;
                                    field.editSortPriority = 9999;
                                    field.authorable = false;
                                    field.caption = "Date Modified";
                                    field.defaultValue = "";
                                    field.isBaseField = IsBaseContent;
                                    verifyCDefField_ReturnID(cpcore, contentName, field);
                                }
                                if (!cpcore.db.isCdefField(returnContentId, "modifiedby")) {
                                    field = new Models.Complex.cdefFieldModel();
                                    field.nameLc = "modifiedby";
                                    field.active = true;
                                    field.fieldTypeId = FieldTypeIdLookup;
                                    field.editSortPriority = 9999;
                                    field.authorable = false;
                                    field.caption = "Modified By";
                                    field.set_lookupContentName(cpcore, "People");
                                    field.defaultValue = "";
                                    field.isBaseField = IsBaseContent;
                                    verifyCDefField_ReturnID(cpcore, contentName, field);
                                }
                                if (!cpcore.db.isCdefField(returnContentId, "ContentControlId")) {
                                    field = new Models.Complex.cdefFieldModel();
                                    field.nameLc = "contentcontrolid";
                                    field.active = true;
                                    field.fieldTypeId = FieldTypeIdLookup;
                                    field.editSortPriority = 9999;
                                    field.authorable = false;
                                    field.caption = "Controlling Content";
                                    field.set_lookupContentName(cpcore, "Content");
                                    field.defaultValue = "";
                                    field.isBaseField = IsBaseContent;
                                    verifyCDefField_ReturnID(cpcore, contentName, field);
                                }
                                if (!cpcore.db.isCdefField(returnContentId, "CreateKey")) {
                                    field = new Models.Complex.cdefFieldModel();
                                    field.nameLc = "createkey";
                                    field.active = true;
                                    field.fieldTypeId = FieldTypeIdInteger;
                                    field.editSortPriority = 9999;
                                    field.authorable = false;
                                    field.caption = "Create Key";
                                    field.defaultValue = "";
                                    field.isBaseField = IsBaseContent;
                                    verifyCDefField_ReturnID(cpcore, contentName, field);
                                }
                                if (!cpcore.db.isCdefField(returnContentId, "ccGuid")) {
                                    field = new Models.Complex.cdefFieldModel();
                                    field.nameLc = "ccguid";
                                    field.active = true;
                                    field.fieldTypeId = FieldTypeIdText;
                                    field.editSortPriority = 9999;
                                    field.authorable = false;
                                    field.caption = "Guid";
                                    field.defaultValue = "";
                                    field.isBaseField = IsBaseContent;
                                    verifyCDefField_ReturnID(cpcore, contentName, field);
                                }
                                // -- 20171029 - had to un-deprecate because compatibility issues are too timeconsuming
                                if (!cpcore.db.isCdefField(returnContentId, "ContentCategoryId")) {
                                    field = new Models.Complex.cdefFieldModel();
                                    field.nameLc = "contentcategoryid";
                                    field.active = true;
                                    field.fieldTypeId = FieldTypeIdInteger;
                                    field.editSortPriority = 9999;
                                    field.authorable = false;
                                    field.caption = "Content Category";
                                    field.defaultValue = "";
                                    field.isBaseField = IsBaseContent;
                                    verifyCDefField_ReturnID(cpcore, contentName, field);
                                }
                            }
                            //
                            // ----- Load CDef
                            //
                            if (clearMetaCache) {
                                cpcore.cache.invalidateAllInContent(Models.Entity.contentModel.contentName.ToLower());
                                cpcore.cache.invalidateAllInContent(Models.Entity.contentFieldModel.contentName.ToLower());
                                cpcore.doc.clearMetaData();
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
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
        public static int verifyCDefField_ReturnID(coreClass cpcore, string ContentName, Models.Complex.cdefFieldModel field) // , ByVal FieldName As String, ByVal Args As String, ByVal Delimiter As String) As Integer
        {
            int returnId = 0;
            try {
                //
                bool RecordIsBaseField = false;
                bool IsBaseField = false;
                string SQL = null;
                int ContentID = 0;
                string[] SQLName = new string[101];
                string[] SQLValue = new string[101];
                string MethodName = null;
                int LookupContentID = 0;
                int RecordID = 0;
                int TableID = 0;
                string TableName = null;
                int DataSourceID = 0;
                string DataSourceName = null;
                bool FieldReadOnly = false;
                bool FieldActive = false;
                int fieldTypeId = 0;
                string FieldCaption = null;
                bool FieldAuthorable = false;
                string LookupContentName = null;
                string DefaultValue = null;
                bool NotEditable = false;
                string AdminIndexWidth = null;
                int AdminIndexSort = 0;
                string RedirectContentName = null;
                string RedirectIDField = null;
                string RedirectPath = null;
                bool HTMLContent = false;
                bool UniqueName = false;
                bool Password = false;
                int RedirectContentID = 0;
                bool FieldRequired = false;
                bool RSSTitle = false;
                bool RSSDescription = false;
                bool FieldDeveloperOnly = false;
                int MemberSelectGroupID = 0;
                string installedByCollectionGuid = null;
                int InstalledByCollectionID = 0;
                string EditTab = null;
                bool Scramble = false;
                string LookupList = null;
                DataTable rs = null;
                bool isNewFieldRecord = true;
                //
                MethodName = "csv_VerifyCDefField_ReturnID(" + ContentName + "," + field.nameLc + ")";
                //
                if ((ContentName.ToUpper() == "PAGE CONTENT") && (field.nameLc.ToUpper() == "ACTIVE")) {
                    field.nameLc = field.nameLc;
                }
                //
                // Prevent load during the changes
                //
                //StateOfAllowContentAutoLoad = AllowContentAutoLoad
                //AllowContentAutoLoad = False
                //
                // determine contentid and tableid
                //
                ContentID = -1;
                TableID = 0;
                SQL = "select ID,ContentTableID from ccContent where name=" + cpcore.db.encodeSQLText(ContentName) + ";";
                rs = cpcore.db.executeQuery(SQL);
                if (isDataTableOk(rs)) {
                    ContentID = genericController.encodeInteger(cpcore.db.getDataRowColumnName(rs.Rows[0], "ID"));
                    TableID = genericController.encodeInteger(cpcore.db.getDataRowColumnName(rs.Rows[0], "ContentTableID"));
                }
                //
                // test if field definition found or not
                //
                RecordID = 0;
                RecordIsBaseField = false;
                SQL = "select ID,IsBaseField from ccFields where (ContentID=" + cpcore.db.encodeSQLNumber(ContentID) + ")and(name=" + cpcore.db.encodeSQLText(field.nameLc) + ");";
                rs = cpcore.db.executeQuery(SQL);
                if (isDataTableOk(rs)) {
                    isNewFieldRecord = false;
                    RecordID = genericController.encodeInteger(cpcore.db.getDataRowColumnName(rs.Rows[0], "ID"));
                    RecordIsBaseField = genericController.encodeBoolean(cpcore.db.getDataRowColumnName(rs.Rows[0], "IsBaseField"));
                }
                //
                // check if this is a non-base field updating a base field
                //
                IsBaseField = field.isBaseField;
                if ((!IsBaseField) && (RecordIsBaseField)) {
                    //
                    // This update is not allowed
                    //
                    cpcore.handleException(new ApplicationException("Warning, updating non-base field with base field, content [" + ContentName + "], field [" + field.nameLc + "]"));
                }
                if (true) {
                    //FieldAdminOnly = field.adminOnly
                    FieldDeveloperOnly = field.developerOnly;
                    FieldActive = field.active;
                    FieldCaption = field.caption;
                    FieldReadOnly = field.readOnly;
                    fieldTypeId = field.fieldTypeId;
                    FieldAuthorable = field.authorable;
                    DefaultValue = genericController.encodeText(field.defaultValue);
                    NotEditable = field.notEditable;
                    LookupContentName = field.get_lookupContentName(cpcore);
                    AdminIndexWidth = field.indexWidth;
                    AdminIndexSort = field.indexSortOrder;
                    RedirectContentName = field.get_RedirectContentName(cpcore);
                    RedirectIDField = field.redirectID;
                    RedirectPath = field.redirectPath;
                    HTMLContent = field.htmlContent;
                    UniqueName = field.uniqueName;
                    Password = field.password;
                    FieldRequired = field.required;
                    RSSTitle = field.RSSTitleField;
                    RSSDescription = field.RSSDescriptionField;
                    MemberSelectGroupID = field.memberSelectGroupId_get( cpcore );
                    installedByCollectionGuid = field.installedByCollectionGuid;
                    EditTab = field.editTabName;
                    Scramble = field.Scramble;
                    LookupList = field.lookupList;
                    //
                    // ----- Check error conditions before starting
                    //
                    if (ContentID == -1) {
                        //
                        // Content Definition not found
                        //
                        throw (new ApplicationException("Could Not create Field [" + field.nameLc + "] because Content Definition [" + ContentName + "] was Not found In ccContent Table."));
                    } else if (TableID <= 0) {
                        //
                        // Content Definition not found
                        //
                        throw (new ApplicationException("Could Not create Field [" + field.nameLc + "] because Content Definition [" + ContentName + "] has no associated Content Table."));
                    } else if (fieldTypeId <= 0) {
                        //
                        // invalid field type
                        //
                        throw (new ApplicationException("Could Not create Field [" + field.nameLc + "] because the field type [" + fieldTypeId + "] Is Not valid."));
                    } else {
                        //
                        // Get the TableName and DataSourceID
                        //
                        TableName = "";
                        rs = cpcore.db.executeQuery("Select Name, DataSourceID from ccTables where ID=" + cpcore.db.encodeSQLNumber(TableID) + ";");
                        if (!isDataTableOk(rs)) {
                            throw (new ApplicationException("Could Not create Field [" + field.nameLc + "] because table For tableID [" + TableID + "] was Not found."));
                        } else {
                            DataSourceID = genericController.encodeInteger(cpcore.db.getDataRowColumnName(rs.Rows[0], "DataSourceID"));
                            TableName = genericController.encodeText(cpcore.db.getDataRowColumnName(rs.Rows[0], "Name"));
                        }
                        rs.Dispose();
                        if (!string.IsNullOrEmpty(TableName)) {
                            //
                            // Get the DataSourceName
                            //
                            if (DataSourceID < 1) {
                                DataSourceName = "Default";
                            } else {
                                rs = cpcore.db.executeQuery("Select Name from ccDataSources where ID=" + cpcore.db.encodeSQLNumber(DataSourceID) + ";");
                                if (!isDataTableOk(rs)) {

                                    DataSourceName = "Default";
                                    // change condition to successful -- the goal is 1) deliver pages 2) report problems
                                    // this problem, if translated to default, is really no longer a problem, unless the
                                    // resulting datasource does not have this data, then other errors will be generated anyway.
                                    //Call csv_HandleClassInternalError(MethodName, "Could Not create Field [" & field.name & "] because datasource For ID [" & DataSourceID & "] was Not found.")
                                } else {
                                    DataSourceName = genericController.encodeText(cpcore.db.getDataRowColumnName(rs.Rows[0], "Name"));
                                }
                                rs.Dispose();
                            }
                            //
                            // Get the installedByCollectionId
                            //
                            InstalledByCollectionID = 0;
                            if (!string.IsNullOrEmpty(installedByCollectionGuid)) {
                                rs = cpcore.db.executeQuery("Select id from ccAddonCollections where ccguid=" + cpcore.db.encodeSQLText(installedByCollectionGuid) + ";");
                                if (isDataTableOk(rs)) {
                                    InstalledByCollectionID = genericController.encodeInteger(cpcore.db.getDataRowColumnName(rs.Rows[0], "Id"));
                                }
                                rs.Dispose();
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
                                cpcore.db.createSQLTableField(DataSourceName, TableName, field.nameLc, fieldTypeId);
                            }
                            //
                            // create or update the field
                            //
                            sqlFieldListClass sqlList = new sqlFieldListClass();
                            sqlList.add("ACTIVE", cpcore.db.encodeSQLBoolean(field.active)); // Pointer)
                            sqlList.add("MODIFIEDBY", cpcore.db.encodeSQLNumber(SystemMemberID)); // Pointer)
                            sqlList.add("MODIFIEDDATE", cpcore.db.encodeSQLDate(DateTime.Now)); // Pointer)
                            sqlList.add("TYPE", cpcore.db.encodeSQLNumber(fieldTypeId)); // Pointer)
                            sqlList.add("CAPTION", cpcore.db.encodeSQLText(FieldCaption)); // Pointer)
                            sqlList.add("ReadOnly", cpcore.db.encodeSQLBoolean(FieldReadOnly)); // Pointer)
                            sqlList.add("REQUIRED", cpcore.db.encodeSQLBoolean(FieldRequired)); // Pointer)
                            sqlList.add("TEXTBUFFERED", SQLFalse); // Pointer)
                            sqlList.add("PASSWORD", cpcore.db.encodeSQLBoolean(Password)); // Pointer)
                            sqlList.add("EDITSORTPRIORITY", cpcore.db.encodeSQLNumber(field.editSortPriority)); // Pointer)
                            sqlList.add("ADMINONLY", cpcore.db.encodeSQLBoolean(field.adminOnly)); // Pointer)
                            sqlList.add("DEVELOPERONLY", cpcore.db.encodeSQLBoolean(FieldDeveloperOnly)); // Pointer)
                            sqlList.add("CONTENTCONTROLID", cpcore.db.encodeSQLNumber(Models.Complex.cdefModel.getContentId(cpcore, "Content Fields"))); // Pointer)
                            sqlList.add("DefaultValue", cpcore.db.encodeSQLText(DefaultValue)); // Pointer)
                            sqlList.add("HTMLCONTENT", cpcore.db.encodeSQLBoolean(HTMLContent)); // Pointer)
                            sqlList.add("NOTEDITABLE", cpcore.db.encodeSQLBoolean(NotEditable)); // Pointer)
                            sqlList.add("AUTHORABLE", cpcore.db.encodeSQLBoolean(FieldAuthorable)); // Pointer)
                            sqlList.add("INDEXCOLUMN", cpcore.db.encodeSQLNumber(field.indexColumn)); // Pointer)
                            sqlList.add("INDEXWIDTH", cpcore.db.encodeSQLText(AdminIndexWidth)); // Pointer)
                            sqlList.add("INDEXSORTPRIORITY", cpcore.db.encodeSQLNumber(AdminIndexSort)); // Pointer)
                            sqlList.add("REDIRECTID", cpcore.db.encodeSQLText(RedirectIDField)); // Pointer)
                            sqlList.add("REDIRECTPATH", cpcore.db.encodeSQLText(RedirectPath)); // Pointer)
                            sqlList.add("UNIQUENAME", cpcore.db.encodeSQLBoolean(UniqueName)); // Pointer)
                            sqlList.add("RSSTITLEFIELD", cpcore.db.encodeSQLBoolean(RSSTitle)); // Pointer)
                            sqlList.add("RSSDESCRIPTIONFIELD", cpcore.db.encodeSQLBoolean(RSSDescription)); // Pointer)
                            sqlList.add("MEMBERSELECTGROUPID", cpcore.db.encodeSQLNumber(MemberSelectGroupID)); // Pointer)
                            sqlList.add("installedByCollectionId", cpcore.db.encodeSQLNumber(InstalledByCollectionID)); // Pointer)
                            sqlList.add("EDITTAB", cpcore.db.encodeSQLText(EditTab)); // Pointer)
                            sqlList.add("SCRAMBLE", cpcore.db.encodeSQLBoolean(Scramble)); // Pointer)
                            sqlList.add("ISBASEFIELD", cpcore.db.encodeSQLBoolean(IsBaseField)); // Pointer)
                            sqlList.add("LOOKUPLIST", cpcore.db.encodeSQLText(LookupList));
                            //
                            // -- conditional fields
                            switch (fieldTypeId) {
                                case FieldTypeIdLookup:
                                    //
                                    // -- lookup field
                                    //
                                    if (!string.IsNullOrEmpty(LookupContentName)) {
                                        LookupContentID = Models.Complex.cdefModel.getContentId(cpcore, LookupContentName);
                                        if (LookupContentID <= 0) {
                                            logController.appendLog(cpcore, "Could not create lookup field [" + field.nameLc + "] for content definition [" + ContentName + "] because no content definition was found For lookup-content [" + LookupContentName + "].");
                                        }
                                    }
                                    sqlList.add("LOOKUPCONTENTID", cpcore.db.encodeSQLNumber(LookupContentID)); // Pointer)
                                    break;
                                case FieldTypeIdManyToMany:
                                    //
                                    // -- many-to-many field
                                    //
                                    string ManyToManyContent = field.get_ManyToManyContentName(cpcore);
                                    if (!string.IsNullOrEmpty(ManyToManyContent)) {
                                        int ManyToManyContentID = Models.Complex.cdefModel.getContentId(cpcore, ManyToManyContent);
                                        if (ManyToManyContentID <= 0) {
                                            logController.appendLog(cpcore, "Could not create many-to-many field [" + field.nameLc + "] for [" + ContentName + "] because no content definition was found For many-to-many-content [" + ManyToManyContent + "].");
                                        }
                                        sqlList.add("MANYTOMANYCONTENTID", cpcore.db.encodeSQLNumber(ManyToManyContentID));
                                    }
                                    //
                                    string ManyToManyRuleContent = field.get_ManyToManyRuleContentName(cpcore);
                                    if (!string.IsNullOrEmpty(ManyToManyRuleContent)) {
                                        int ManyToManyRuleContentID = Models.Complex.cdefModel.getContentId(cpcore, ManyToManyRuleContent);
                                        if (ManyToManyRuleContentID <= 0) {
                                            logController.appendLog(cpcore, "Could not create many-to-many field [" + field.nameLc + "] for [" + ContentName + "] because no content definition was found For many-to-many-rule-content [" + ManyToManyRuleContent + "].");
                                        }
                                        sqlList.add("MANYTOMANYRULECONTENTID", cpcore.db.encodeSQLNumber(ManyToManyRuleContentID));
                                    }
                                    sqlList.add("MANYTOMANYRULEPRIMARYFIELD", cpcore.db.encodeSQLText(field.ManyToManyRulePrimaryField));
                                    sqlList.add("MANYTOMANYRULESECONDARYFIELD", cpcore.db.encodeSQLText(field.ManyToManyRuleSecondaryField));
                                    break;
                                case FieldTypeIdRedirect:
                                    //
                                    // -- redirect field
                                    if (!string.IsNullOrEmpty(RedirectContentName)) {
                                        RedirectContentID = Models.Complex.cdefModel.getContentId(cpcore, RedirectContentName);
                                        if (RedirectContentID <= 0) {
                                            logController.appendLog(cpcore, "Could not create redirect field [" + field.nameLc + "] for Content Definition [" + ContentName + "] because no content definition was found For redirect-content [" + RedirectContentName + "].");
                                        }
                                    }
                                    sqlList.add("REDIRECTCONTENTID", cpcore.db.encodeSQLNumber(RedirectContentID)); // Pointer)
                                    break;
                            }
                            //
                            if (RecordID == 0) {
                                sqlList.add("NAME", cpcore.db.encodeSQLText(field.nameLc)); // Pointer)
                                sqlList.add("CONTENTID", cpcore.db.encodeSQLNumber(ContentID)); // Pointer)
                                sqlList.add("CREATEKEY", "0"); // Pointer)
                                sqlList.add("DATEADDED", cpcore.db.encodeSQLDate(DateTime.Now)); // Pointer)
                                sqlList.add("CREATEDBY", cpcore.db.encodeSQLNumber(SystemMemberID)); // Pointer)
                                                                                                     //
                                RecordID = cpcore.db.insertTableRecordGetId("Default", "ccFields");
                            }
                            if (RecordID == 0) {
                                throw (new ApplicationException("Could Not create Field [" + field.nameLc + "] because insert into ccfields failed."));
                            } else {
                                cpcore.db.updateTableRecord("Default", "ccFields", "ID=" + RecordID, sqlList);
                            }
                            //
                        }
                    }
                }
                //
                if (!isNewFieldRecord) {
                    cpcore.cache.invalidateAll();
                    cpcore.doc.clearMetaData();
                }
                //
                returnId = RecordID;
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return returnId;
        }
        //
        //=============================================================
        //
        //=============================================================
        //
        public static bool isContentFieldSupported(coreClass cpcore, string ContentName, string FieldName) {
            bool returnOk = false;
            try {
                Models.Complex.cdefModel cdef;
                //
                cdef = Models.Complex.cdefModel.getCdef(cpcore, ContentName);
                if (cdef != null) {
                    returnOk = cdef.fields.ContainsKey(FieldName.ToLower());
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return returnOk;
        }
        //
        //========================================================================
        // Get a tables first ContentID from Tablename
        //========================================================================
        //
        public static int getContentIDByTablename(coreClass cpcore, string TableName) {
            int tempgetContentIDByTablename = 0;
            //
            string SQL = null;
            int CS = 0;
            //
            tempgetContentIDByTablename = -1;
            if (!string.IsNullOrEmpty(TableName)) {
                SQL = "select ContentControlID from " + TableName + " where contentcontrolid is not null order by contentcontrolid;";
                CS = cpcore.db.csOpenSql_rev("Default", SQL, 1, 1);
                if (cpcore.db.csOk(CS)) {
                    tempgetContentIDByTablename = cpcore.db.csGetInteger(CS, "ContentControlID");
                }
                cpcore.db.csClose(ref CS);
            }
            return tempgetContentIDByTablename;
        }
        //
        //========================================================================
        //
        public static string getContentControlCriteria(coreClass cpcore, string ContentName) {
            return Models.Complex.cdefModel.getCdef(cpcore, ContentName).ContentControlCriteria;
        }
        //
        //============================================================================================================
        //   the content control Id for a record, all its edit and archive records, and all its child records
        //   returns records affected
        //   the contentname contains the record, but we do not know that this is the contentcontrol for the record,
        //   read it first to main_Get the correct contentid
        //============================================================================================================
        //
        public static void setContentControlId(coreClass cpcore, int ContentID, int RecordID, int NewContentControlID, string UsedIDString = "") {
            string SQL = null;
            int CS = 0;
            string RecordTableName = null;
            string ContentName = null;
            bool HasParentID = false;
            int RecordContentID = 0;
            string RecordContentName = "";
            string DataSourceName = null;
            //
            if (!genericController.IsInDelimitedString(UsedIDString, RecordID.ToString(), ",")) {
                ContentName = getContentNameByID(cpcore, ContentID);
                CS = cpcore.db.csOpenRecord(ContentName, RecordID, false, false);
                if (cpcore.db.csOk(CS)) {
                    HasParentID = cpcore.db.cs_isFieldSupported(CS, "ParentID");
                    RecordContentID = cpcore.db.csGetInteger(CS, "ContentControlID");
                    RecordContentName = getContentNameByID(cpcore, RecordContentID);
                }
                cpcore.db.csClose(ref CS);
                if (!string.IsNullOrEmpty(RecordContentName)) {
                    //
                    //
                    //
                    DataSourceName = getContentDataSource(cpcore, RecordContentName);
                    RecordTableName = Models.Complex.cdefModel.getContentTablename(cpcore, RecordContentName);
                    //
                    // either Workflow on non-workflow - it changes everything
                    //
                    SQL = "update " + RecordTableName + " set ContentControlID=" + NewContentControlID + " where ID=" + RecordID;
                    cpcore.db.executeQuery(SQL, DataSourceName);
                    if (HasParentID) {
                        SQL = "select contentcontrolid,ID from " + RecordTableName + " where ParentID=" + RecordID;
                        CS = cpcore.db.csOpenSql_rev(DataSourceName, SQL);
                        while (cpcore.db.csOk(CS)) {
                            setContentControlId(cpcore, cpcore.db.csGetInteger(CS, "contentcontrolid"), cpcore.db.csGetInteger(CS, "ID"), NewContentControlID, UsedIDString + "," + RecordID);
                            cpcore.db.csGoNext(CS);
                        }
                        cpcore.db.csClose(ref CS);
                    }
                    //
                    // fix content watch
                    //
                    SQL = "update ccContentWatch set ContentID=" + NewContentControlID + ", ContentRecordKey='" + NewContentControlID + "." + RecordID + "' where ContentID=" + ContentID + " and RecordID=" + RecordID;
                    cpcore.db.executeQuery(SQL);
                }
            }
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        public static string GetContentFieldProperty(coreClass cpcore, string ContentName, string FieldName, string PropertyName) {
            string result = "";
            try {
                cdefModel Contentdefinition = cdefModel.getCdef(cpcore, ContentName);
                if ((string.IsNullOrEmpty(FieldName)) || (Contentdefinition.fields.Count < 1)) {
                    throw (new ApplicationException("Content Name [" + genericController.encodeText(ContentName) + "] or FieldName [" + FieldName + "] was not valid")); 
                } else {
                    foreach (KeyValuePair<string, Models.Complex.cdefFieldModel> keyValuePair in Contentdefinition.fields) {
                        Models.Complex.cdefFieldModel field = keyValuePair.Value;
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
                                    result = genericController.encodeText(field.defaultValue);
                                    break;
                                case "MEMBERSELECTGROUPID":
                                    result = field.memberSelectGroupId_get( cpcore ).ToString() ;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        }
                    }
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
            }
            return result;


        }
        //
        //========================================================================
        //
        //========================================================================
        //
        public static string GetContentProperty(coreClass cpcore, string ContentName, string PropertyName) {
            string result = "";
            Models.Complex.cdefModel Contentdefinition;
            //
            Contentdefinition = Models.Complex.cdefModel.getCdef(cpcore, genericController.encodeText(ContentName));
            switch (genericController.vbUCase(genericController.encodeText(PropertyName))) {
                case "CONTENTCONTROLCRITERIA":
                    result = Contentdefinition.ContentControlCriteria;
                    break;
                case "ACTIVEONLY":
                    result = Contentdefinition.ActiveOnly.ToString();
                    break;
                case "ADMINONLY":
                    result = Contentdefinition.AdminOnly.ToString();
                    break;
                case "ALIASID":
                    result = Contentdefinition.AliasID;
                    break;
                case "ALIASNAME":
                    result = Contentdefinition.AliasName;
                    break;
                case "ALLOWADD":
                    result = Contentdefinition.AllowAdd.ToString();
                    break;
                case "ALLOWDELETE":
                    result = Contentdefinition.AllowDelete.ToString();
                    //Case "CHILDIDLIST"
                    //    main_result = Contentdefinition.ChildIDList
                    break;
                case "DATASOURCEID":
                    result = Contentdefinition.dataSourceId.ToString();
                    break;
                case "DEFAULTSORTMETHOD":
                    result = Contentdefinition.DefaultSortMethod;
                    break;
                case "DEVELOPERONLY":
                    result = Contentdefinition.DeveloperOnly.ToString();
                    break;
                case "FIELDCOUNT":
                    result = Contentdefinition.fields.Count.ToString();
                    //Case "FIELDPOINTER"
                    //    main_result = Contentdefinition.FieldPointer
                    break;
                case "ID":
                    result = Contentdefinition.Id.ToString();
                    break;
                case "IGNORECONTENTCONTROL":
                    result = Contentdefinition.IgnoreContentControl.ToString();
                    break;
                case "NAME":
                    result = Contentdefinition.Name;
                    break;
                case "PARENTID":
                    result = Contentdefinition.parentID.ToString();
                    //Case "SINGLERECORD"
                    //    main_result = Contentdefinition.SingleRecord
                    break;
                case "CONTENTTABLENAME":
                    result = Contentdefinition.ContentTableName;
                    break;
                case "CONTENTDATASOURCENAME":
                    result = Contentdefinition.ContentDataSourceName;
                    //Case "AUTHORINGTABLENAME"
                    //    result = Contentdefinition.AuthoringTableName
                    //Case "AUTHORINGDATASOURCENAME"
                    //    result = Contentdefinition.AuthoringDataSourceName
                    break;
                case "WHERECLAUSE":
                    result = Contentdefinition.WhereClause;
                    //Case "ALLOWWORKFLOWAUTHORING"
                    //    result = Contentdefinition.AllowWorkflowAuthoring.ToString
                    break;
                case "DROPDOWNFIELDLIST":
                    result = Contentdefinition.DropDownFieldList;
                    break;
                case "SELECTFIELDLIST":
                    result = Contentdefinition.SelectCommaList;
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
        public static cdefModel getCache(coreClass cpcore, int contentId) {
            cdefModel result = null;
            try {
                try {
                    string cacheName = Controllers.cacheController.getCacheKey_ComplexObject("cdef", contentId.ToString());
                    result = cpcore.cache.getObject<Models.Complex.cdefModel>(cacheName);
                } catch (Exception ex) {
                    cpcore.handleException(ex);
                }
            } catch (Exception) {}
            return result;
        }
        //
        //====================================================================================================
        //
        public static void setCache(coreClass cpcore, int contentId, cdefModel cdef) {
            string cacheName = Controllers.cacheController.getCacheKey_ComplexObject("cdef", contentId.ToString());
            //
            // -- make it dependant on cacheNameInvalidateAll. If invalidated, all cdef will invalidate
            List<string> dependantList = new List<string>();
            dependantList.Add(cacheNameInvalidateAll);
            cpcore.cache.setObject(cacheName, cdef, dependantList);
        }
        //
        //====================================================================================================
        //
        public static void invalidateCache(coreClass cpCore, int contentId) {
            string cacheName = Controllers.cacheController.getCacheKey_ComplexObject("cdef", contentId.ToString());
            cpCore.cache.invalidate(cacheName);
        }
        //
        //====================================================================================================
        //
        public static void invalidateCacheAll(coreClass cpCore) {
            cpCore.cache.invalidate(cacheNameInvalidateAll);
        }
    }
}
