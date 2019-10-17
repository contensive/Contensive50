
using System;
using System.Data;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.BaseClasses;
using System.Text;
using Contensive.Models.Db;
//
namespace Contensive.Processor.Controllers {
    //
    // todo 20190809, move most of this into the domain model to create and save a collection domain model (maybe all of it is a model)
    //
    /// <summary>
    /// Handle the nitty-gritty of reading and writing the xml file for collections and CDefMiniCollections
    /// </summary>
    public static class CollectionExportCDefController {
        //
        // ----- global scope variables
        //
        private const string ContentSelectList = ""
            + " id,name,active,adminonly,allowadd"
            + ",allowcalendarevents,allowcontentchildtool,allowcontenttracking,allowdelete,0 as allowmetacontent"
            + ",allowtopicrules,AllowWorkflowAuthoring,AuthoringTableID"
            + ",ContentTableID,DefaultSortMethodID,DeveloperOnly,DropDownFieldList"
            + ",EditorGroupID,ParentID,ccGuid,IsBaseContent"
            + ",IconLink,IconHeight,IconWidth,IconSprites";

        private const string FieldSelectList = ""
            + "f.ID,f.Name,f.contentid,f.Active,f.AdminOnly,f.Authorable,f.Caption,f.DeveloperOnly,f.EditSortPriority,f.Type,f.HTMLContent"
            + ",f.IndexColumn,f.IndexSortDirection,f.IndexSortPriority,f.RedirectID,f.RedirectPath,f.Required"
            + ",f.TextBuffered,f.UniqueName,f.DefaultValue,f.RSSTitleField,f.RSSDescriptionField,f.MemberSelectGroupID"
            + ",f.EditTab,f.Scramble,f.LookupList,f.NotEditable,f.Password,f.readonly,f.ManyToManyRulePrimaryField"
            + ",f.ManyToManyRuleSecondaryField,'' as HelpMessageDeprecated,f.ModifiedBy,f.IsBaseField,f.LookupContentID"
            + ",f.RedirectContentID,f.ManyToManyContentID,f.ManyToManyRuleContentID"
            + ",h.helpdefault,h.helpcustom,f.IndexWidth";

        private const int f_Id = 0;
        private const int f_Name = 1;
        private const int f_contentid = 2;
        private const int f_Active = 3;
        private const int f_AdminOnly = 4;
        private const int f_Authorable = 5;
        private const int f_Caption = 6;
        private const int f_DeveloperOnly = 7;
        private const int f_EditSortPriority = 8;
        private const int f_Type = 9;
        private const int f_HTMLContent = 10;
        private const int f_IndexColumn = 11;
        private const int f_IndexSortDirection = 12;
        private const int f_IndexSortPriority = 13;
        private const int f_RedirectId = 14;
        private const int f_RedirectPath = 15;
        private const int f_Required = 16;
        private const int f_TextBuffered = 17;
        private const int f_UniqueName = 18;
        private const int f_DefaultValue = 19;
        private const int f_RSSTitleField = 20;
        private const int f_RSSDescriptionField = 21;
        private const int f_MemberSelectGroupId = 22;
        private const int f_EditTab = 23;
        private const int f_Scramble = 24;
        private const int f_LookupList = 25;
        private const int f_NotEditable = 26;
        private const int f_Password = 27;
        private const int f_ReadOnly = 28;
        private const int f_ManyToManyRulePrimaryField = 29;
        private const int f_ManyToManyRuleSecondaryField = 30;
        private const int f_HelpMessageDeprecated = 31;
        private const int f_ModifiedBy = 32;
        private const int f_IsBaseField = 33;
        private const int f_LookupContentId = 34;
        private const int f_RedirectContentId = 35;
        private const int f_ManyToManyContentId = 36;
        private const int f_ManyToManyRuleContentId = 37;
        private const int f_helpdefault = 38;
        private const int f_helpcustom = 39;
        private const int f_IndexWidth = 40;
        //
        //====================================================================================================
        /// <summary>
        /// Return collection CDEF node
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentId"></param>
        /// <param name="IncludeBaseFields">legacy, replace with installedByCollectionId</param>
        /// <param name="FoundMenuTable">hack, remove this</param>
        /// <returns></returns>
        public static string getCollectionCdef(CoreController core, int contentId, bool IncludeBaseFields, ref bool FoundMenuTable) {
            try {
                FoundMenuTable = false;
                //
                // Build table lookup
                string[,] Tables = { };
                int TableCnt = 0;
                using (var dt = core.db.executeQuery("select T.ID,T.Name as TableName,D.Name as DataSourceName from ccTables T Left Join ccDataSources D on D.ID=T.DataSourceID")) {
                    Tables = core.db.convertDataTabletoArray(dt);
                    if (Tables != null) { TableCnt = Tables.GetUpperBound(1) + 1; }
                }
                //
                // Build SortMethod lookup
                string[,] Sorts = { };
                int SortCnt = 0;
                using (var dt = core.db.executeQuery("select ID,Name from ccSortMethods")) {
                    Sorts = core.db.convertDataTabletoArray(dt);
                    if (Sorts != null) {
                        SortCnt = Sorts.GetUpperBound(1) + 1;
                    }
                }
                //
                // Build SortMethod lookup
                string[,] Groups = { };
                int GroupCnt = 0;
                using (var dt = core.db.executeQuery("select ID,Name from ccGroups")) {
                    Groups = core.db.convertDataTabletoArray(dt);
                    if (Groups != null) {
                        GroupCnt = Groups.GetUpperBound(1) + 1;
                    }
                }
                //
                // Build Content lookup
                //
                string[,] Contents = { };
                int ContentCnt = 0;
                using (var dt = core.db.executeQuery("select id,name from ccContent")) {
                    Contents = core.db.convertDataTabletoArray(dt);
                    if (Contents != null) {
                        ContentCnt = Contents.GetUpperBound(1) + 1;
                    }
                }
                //
                // select all the fields
                //
                string sqlFieldMeta = "";
                if (contentId != 0) {
                    sqlFieldMeta = "select " + FieldSelectList + ""
                        + " from ccfields f left join ccfieldhelp h on h.fieldid=f.id"
                        + " where (f.Type<>0)and(f.contentid=" + contentId + ")"
                        + "";
                } else {
                    sqlFieldMeta = "select " + FieldSelectList + ""
                        + " from ccfields f left join ccfieldhelp h on h.fieldid=f.id"
                        + " where (f.Type<>0)"
                        + "";
                }
                if (!IncludeBaseFields) { sqlFieldMeta += " and ((f.IsBaseField is null)or(f.IsBaseField=0))"; }
                string[,] fieldMeta = { };
                int CFieldCnt = 0;
                sqlFieldMeta += " order by f.contentid,f.id,h.id desc";
                using (var dt = core.db.executeQuery(sqlFieldMeta)) {
                    fieldMeta = core.db.convertDataTabletoArray(dt);
                    CFieldCnt = fieldMeta.GetUpperBound(1) + 1;
                }
                //
                // select the table Metadata
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                string sqlTableMeta;
                if (contentId != 0) {
                    sqlTableMeta = "select " + ContentSelectList + " from ccContent where (id=" + contentId + ")and(contenttableid is not null)and(contentcontrolid is not null) order by id";
                } else {
                    sqlTableMeta = "select " + ContentSelectList + " from ccContent where (name<>'')and(name is not null)and(contenttableid is not null)and(contentcontrolid is not null) order by id";
                }
                using (var dataTableMeta = core.db.executeQuery(sqlTableMeta)) {
                    int CFieldPtr = 0;
                    foreach (DataRow rowTableMeta in dataTableMeta.Rows) {
                        //
                        // ----- <cdef>
                        //
                        string appName = core.appConfig.name;
                        string iContentName = encodeXmlAttributeFieldValue(appName, rowTableMeta, "Name");
                        contentId = GenericController.encodeInteger(rowTableMeta["ID"]);
                        sb.Append(Environment.NewLine + "\t<CDef");
                        sb.Append(" Name=\"" + iContentName + "\"");
                        if ((!encodeBoolean(rowTableMeta["isBaseContent"])) || IncludeBaseFields) {
                            sb.Append(" Active=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "Active") + "\"");
                            sb.Append(" AdminOnly=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "AdminOnly") + "\"");
                            sb.Append(" AllowAdd=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "AllowAdd") + "\"");
                            sb.Append(" AllowCalendarEvents=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "AllowCalendarEvents") + "\"");
                            sb.Append(" AllowContentChildTool=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "AllowContentChildTool") + "\"");
                            sb.Append(" AllowContentTracking=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "AllowContentTracking") + "\"");
                            sb.Append(" AllowDelete=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "AllowDelete") + "\"");
                            sb.Append(" AllowTopicRules=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "AllowTopicRules") + "\"");
                            sb.Append(" AllowWorkflowAuthoring=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "AllowWorkflowAuthoring") + "\"");
                            //
                            int AuthoringTableId = GenericController.encodeInteger(rowTableMeta["AuthoringTableID"]);
                            string TableName = "";
                            string DataSourceName = "";
                            int Ptr = 0;
                            if (AuthoringTableId != 0) {
                                for (Ptr = 0; Ptr < TableCnt; Ptr++) {
                                    if (GenericController.encodeInteger(Tables[0, Ptr]) == AuthoringTableId) {
                                        TableName = GenericController.encodeText(Tables[1, Ptr]);
                                        DataSourceName = GenericController.encodeText(Tables[2, Ptr]);
                                        break;
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(DataSourceName)) {
                                DataSourceName = "Default";
                            }
                            if (GenericController.vbUCase(TableName) == "CCMENUENTRIES") {
                                FoundMenuTable = true;
                            }
                            sb.Append(" AuthoringDataSourceName=\"" + encodeXMLattribute(DataSourceName) + "\"");
                            sb.Append(" AuthoringTableName=\"" + encodeXMLattribute(TableName) + "\"");
                            //
                            int ContentTableId = GenericController.encodeInteger(rowTableMeta["ContentTableID"]);
                            if (ContentTableId != AuthoringTableId) {
                                if (ContentTableId != 0) {
                                    TableName = "";
                                    DataSourceName = "";
                                    for (Ptr = 0; Ptr < TableCnt; Ptr++) {
                                        if (GenericController.encodeInteger(Tables[0, Ptr]) == ContentTableId) {
                                            TableName = GenericController.encodeText(Tables[1, Ptr]);
                                            DataSourceName = GenericController.encodeText(Tables[2, Ptr]);
                                            break;
                                        }
                                    }
                                    if (string.IsNullOrEmpty(DataSourceName)) {
                                        DataSourceName = "Default";
                                    }
                                }
                            }
                            sb.Append(" ContentDataSourceName=\"" + encodeXMLattribute(DataSourceName) + "\"");
                            sb.Append(" ContentTableName=\"" + encodeXMLattribute(TableName) + "\"");
                            //
                            int DefaultSortMethodId = GenericController.encodeInteger(rowTableMeta["DefaultSortMethodID"]);
                            string DefaultSortMethod = cacheLookup(DefaultSortMethodId, Sorts);
                            sb.Append(" DefaultSortMethod=\"" + encodeXMLattribute(DefaultSortMethod) + "\"");
                            //
                            sb.Append(" DeveloperOnly=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "DeveloperOnly") + "\"");
                            sb.Append(" DropDownFieldList=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "DropDownFieldList") + "\"");
                            //
                            int EditorGroupId = GenericController.encodeInteger(rowTableMeta["EditorGroupID"]);
                            string EditorGroupName = cacheLookup(EditorGroupId, Groups);
                            sb.Append(" EditorGroupName=\"" + encodeXMLattribute(EditorGroupName) + "\"");
                            //
                            int ParentId = GenericController.encodeInteger(rowTableMeta["ParentID"]);
                            string ParentName = cacheLookup(ParentId, Contents);
                            sb.Append(" Parent=\"" + encodeXMLattribute(ParentName) + "\"");
                            //
                            sb.Append(" IconLink=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "IconLink") + "\"");
                            sb.Append(" IconHeight=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "IconHeight") + "\"");
                            sb.Append(" IconWidth=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "IconWidth") + "\"");
                            sb.Append(" IconSprites=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "IconSprites") + "\"");
                            sb.Append(" isbasecontent=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "IsBaseContent") + "\"");
                        }
                        sb.Append(" guid=\"" + encodeXmlAttributeFieldValue(appName, rowTableMeta, "ccGuid") + "\"");
                        sb.Append(" >");
                        //
                        // ----- <field>
                        //
                        int FieldCnt = 0;
                        int fieldId = 0;
                        while (CFieldPtr < CFieldCnt) {
                            int LastFieldId = fieldId;
                            fieldId = GenericController.encodeInteger(fieldMeta[f_Id, CFieldPtr]);
                            string FieldName = GenericController.encodeText(fieldMeta[f_Name, CFieldPtr]);
                            int FieldContentId = GenericController.encodeInteger(fieldMeta[f_contentid, CFieldPtr]);
                            if (FieldContentId > contentId) {
                                break;
                            } else if ((FieldContentId == contentId) && (fieldId != LastFieldId)) {
                                if (IncludeBaseFields || (",id,dateadded,createdby,modifiedby,ContentControlID,CreateKey,ModifiedDate,ccguid,".IndexOf("," + FieldName + ",", System.StringComparison.OrdinalIgnoreCase) == -1)) {
                                    sb.Append(Environment.NewLine + "\t\t<Field");
                                    string fieldType = Models.Domain.ContentFieldMetadataModel.getFieldTypeNameFromFieldTypeId(core, (CPContentBaseClass.FieldTypeIdEnum)encodeInteger(fieldMeta[f_Type, CFieldPtr]));
                                    sb.Append(" Name=\"" + xmlValueText(FieldName) + "\"");
                                    sb.Append(" active=\"" + xmlValueBoolean(fieldMeta[f_Active, CFieldPtr]) + "\"");
                                    sb.Append(" AdminOnly=\"" + xmlValueBoolean(fieldMeta[f_AdminOnly, CFieldPtr]) + "\"");
                                    sb.Append(" Authorable=\"" + xmlValueBoolean(fieldMeta[f_Authorable, CFieldPtr]) + "\"");
                                    sb.Append(" Caption=\"" + xmlValueText(fieldMeta[f_Caption, CFieldPtr]) + "\"");
                                    sb.Append(" DeveloperOnly=\"" + xmlValueBoolean(fieldMeta[f_DeveloperOnly, CFieldPtr]) + "\"");
                                    sb.Append(" EditSortPriority=\"" + xmlValueText(fieldMeta[f_EditSortPriority, CFieldPtr]) + "\"");
                                    sb.Append(" FieldType=\"" + fieldType + "\"");
                                    sb.Append(" HTMLContent=\"" + xmlValueBoolean(fieldMeta[f_HTMLContent, CFieldPtr]) + "\"");
                                    sb.Append(" IndexColumn=\"" + xmlValueText(fieldMeta[f_IndexColumn, CFieldPtr]) + "\"");
                                    sb.Append(" IndexSortDirection=\"" + xmlValueText(fieldMeta[f_IndexSortDirection, CFieldPtr]) + "\"");
                                    sb.Append(" IndexSortOrder=\"" + xmlValueText(fieldMeta[f_IndexSortPriority, CFieldPtr]) + "\"");
                                    sb.Append(" IndexWidth=\"" + xmlValueText(fieldMeta[f_IndexWidth, CFieldPtr]) + "\"");
                                    sb.Append(" RedirectID=\"" + xmlValueText(fieldMeta[f_RedirectId, CFieldPtr]) + "\"");
                                    sb.Append(" RedirectPath=\"" + xmlValueText(fieldMeta[f_RedirectPath, CFieldPtr]) + "\"");
                                    sb.Append(" Required=\"" + xmlValueBoolean(fieldMeta[f_Required, CFieldPtr]) + "\"");
                                    sb.Append(" TextBuffered=\"" + xmlValueBoolean(fieldMeta[f_TextBuffered, CFieldPtr]) + "\"");
                                    sb.Append(" UniqueName=\"" + xmlValueBoolean(fieldMeta[f_UniqueName, CFieldPtr]) + "\"");
                                    sb.Append(" DefaultValue=\"" + xmlValueText(fieldMeta[f_DefaultValue, CFieldPtr]) + "\"");
                                    sb.Append(" RSSTitle=\"" + xmlValueBoolean(fieldMeta[f_RSSTitleField, CFieldPtr]) + "\"");
                                    sb.Append(" RSSDescription=\"" + xmlValueBoolean(fieldMeta[f_RSSDescriptionField, CFieldPtr]) + "\"");
                                    sb.Append(" EditTab=\"" + xmlValueText(fieldMeta[f_EditTab, CFieldPtr]) + "\"");
                                    sb.Append(" Scramble=\"" + xmlValueBoolean(fieldMeta[f_Scramble, CFieldPtr]) + "\"");
                                    sb.Append(" LookupList=\"" + xmlValueText(fieldMeta[f_LookupList, CFieldPtr]) + "\"");
                                    sb.Append(" NotEditable=\"" + xmlValueBoolean(fieldMeta[f_NotEditable, CFieldPtr]) + "\"");
                                    sb.Append(" Password=\"" + xmlValueBoolean(fieldMeta[f_Password, CFieldPtr]) + "\"");
                                    sb.Append(" ReadOnly=\"" + xmlValueBoolean(fieldMeta[f_ReadOnly, CFieldPtr]) + "\"");
                                    sb.Append(" ManyToManyRulePrimaryField=\"" + xmlValueText(fieldMeta[f_ManyToManyRulePrimaryField, CFieldPtr]) + "\"");
                                    sb.Append(" ManyToManyRuleSecondaryField=\"" + xmlValueText(fieldMeta[f_ManyToManyRuleSecondaryField, CFieldPtr]) + "\"");
                                    sb.Append(" IsModified=\"" + (encodeInteger(fieldMeta[f_ModifiedBy, CFieldPtr]) != 0) + "\"");
                                    {
                                        sb.Append(" IsBaseField=\"" + xmlValueBoolean(fieldMeta[f_IsBaseField, CFieldPtr]) + "\"");
                                    }
                                    //
                                    int recordId = GenericController.encodeInteger(fieldMeta[f_LookupContentId, CFieldPtr]);
                                    string RecordName = cacheLookup(recordId, Contents);
                                    sb.Append(" LookupContent=\"" + HtmlController.encodeHtml(RecordName) + "\"");
                                    //
                                    recordId = GenericController.encodeInteger(fieldMeta[f_RedirectContentId, CFieldPtr]);
                                    RecordName = cacheLookup(recordId, Contents);
                                    sb.Append(" RedirectContent=\"" + HtmlController.encodeHtml(RecordName) + "\"");
                                    //
                                    recordId = GenericController.encodeInteger(fieldMeta[f_ManyToManyContentId, CFieldPtr]);
                                    RecordName = cacheLookup(recordId, Contents);
                                    sb.Append(" ManyToManyContent=\"" + HtmlController.encodeHtml(RecordName) + "\"");
                                    //
                                    recordId = GenericController.encodeInteger(fieldMeta[f_ManyToManyRuleContentId, CFieldPtr]);
                                    RecordName = cacheLookup(recordId, Contents);
                                    sb.Append(" ManyToManyRuleContent=\"" + HtmlController.encodeHtml(RecordName) + "\"");
                                    //
                                    recordId = GenericController.encodeInteger(fieldMeta[f_MemberSelectGroupId, CFieldPtr]);
                                    RecordName = "";
                                    if (recordId > 0) {
                                        RecordName = DbBaseModel.getRecordName<GroupModel>(core.cpParent, recordId);
                                    }
                                    sb.Append(" MemberSelectGroup=\"" + xmlValueText(fieldMeta[f_MemberSelectGroupId, CFieldPtr]) + "\"");


                                    sb.Append(" >");
                                    //
                                    int HelpCnt = 0;
                                    string HelpDefault = xmlValueText(fieldMeta[f_helpcustom, CFieldPtr]);
                                    if (string.IsNullOrEmpty(HelpDefault)) {
                                        HelpDefault = xmlValueText(fieldMeta[f_helpdefault, CFieldPtr]);
                                    }
                                    if (!string.IsNullOrEmpty(HelpDefault)) {
                                        sb.Append(Environment.NewLine + "\t\t\t<HelpDefault>" + HelpDefault + "</HelpDefault>");
                                        HelpCnt = HelpCnt + 1;
                                    }
                                    if (HelpCnt > 0) {
                                        sb.Append(Environment.NewLine + "\t\t");
                                    }
                                    sb.Append("</Field>");
                                }
                                FieldCnt = FieldCnt + 1;
                            }
                            CFieldPtr = CFieldPtr + 1;
                        }
                        //
                        if (FieldCnt > 0) {
                            sb.Append(Environment.NewLine + "\t");
                        }
                        sb.Append("</CDef>");
                    }
                }
                return sb.ToString();
            } catch (Exception) {
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a CDef node of a collection for this content Name
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentName"></param>
        /// <param name="IncludeBaseFields"></param>
        /// <returns></returns>
        public static string getCollectionCdef(CoreController core, string contentName, bool IncludeBaseFields = false) {
            int contentId = 0;
            if (!string.IsNullOrEmpty(contentName)) {
                using (var dt = core.db.executeQuery("select id from cccontent where name=" + DbController.encodeSQLText(contentName))) {
                    if (dt.Rows.Count > 0) {
                        contentId = GenericController.encodeInteger(dt.Rows[0]["id"]);
                        bool FoundMenuTable = false;
                        return getCollectionCdef(core, contentId, IncludeBaseFields, ref FoundMenuTable);
                    }
                }
            }
            return string.Empty;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return an xml collection of the current application (all addon collections, etc)
        /// </summary>
        /// <param name="IncludeBaseFields"></param>
        /// <returns></returns>
        public static string get(CoreController core, bool IncludeBaseFields = false) {
            try {
                //
                StringBuilder sb = new StringBuilder();
                //
                //
                bool FoundMenuTable = false;
                sb.Append(getCollectionCdef(core, 0, IncludeBaseFields, ref FoundMenuTable));
                //
                // Add other areas of the CDef file
                sb.Append(getSQLIndexes(core));
                if (FoundMenuTable) {
                    sb.Append(getAdminMenus(core));
                }
                return "<" + CollectionFileRootNode + " name=\"Application\" guid=\"" + ApplicationCollectionGuid + "\">" + sb.ToString() + Environment.NewLine + "</" + CollectionFileRootNode + ">";
            } catch (Exception ex) {
                LogController.logError(core, ex);
                return string.Empty;
            }
        }
        //====================================================================================================
        /// <summary>
        /// get an xml string representing sql indexes section of appliction collection
        /// </summary>
        /// <returns></returns>
        private static string getSQLIndexes(CoreController core) {
            string result = "";
            try {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                string appName = core.appConfig.name;
                string SQL = ""
                    + " select D.name as DataSourceName,T.name as TableName"
                    + " from cctables T left join ccDataSources d on D.ID=T.DataSourceID"
                    + " where t.active<>0";
                using (var csData = new CsModel(core)) {
                    csData.openSql(SQL);
                    while (csData.ok()) {
                        string DataSourceName = csData.getText("DataSourceName");
                        string TableName = csData.getText("TableName");
                        string IndexList = core.db.getSQLIndexList(TableName);
                        //
                        if (!string.IsNullOrEmpty(IndexList)) {
                            string[] ListRows = GenericController.stringSplit(IndexList, Environment.NewLine);
                            string IndexName = "";
                            int tempVar = ListRows.GetUpperBound(0) + 1;
                            string IndexFields = "";
                            for (int Ptr = 0; Ptr <= tempVar; Ptr++) {
                                string[] ListRowSplit = null;
                                if (Ptr <= ListRows.GetUpperBound(0)) {
                                    //
                                    // ListRowSplit has the indexname and field for this index
                                    //
                                    ListRowSplit = ListRows[Ptr].Split(',');
                                } else {
                                    //
                                    // one past the last row, ListRowSplit gets a dummy entry to force the output of the last line
                                    //
                                    ListRowSplit = ("-,-").Split(',');
                                }
                                if (ListRowSplit.GetUpperBound(0) > 0) {
                                    if (!string.IsNullOrEmpty(ListRowSplit[0])) {
                                        if (string.IsNullOrEmpty(IndexName)) {
                                            //
                                            // first line of the first index description
                                            //
                                            IndexName = ListRowSplit[0];
                                            IndexFields = ListRowSplit[1];
                                        } else if (IndexName == ListRowSplit[0]) {
                                            //
                                            // next line of the index description
                                            //
                                            IndexFields = IndexFields + "," + ListRowSplit[1];
                                        } else {
                                            //
                                            // first line of a new index description
                                            // save previous line
                                            //
                                            if (!string.IsNullOrEmpty(IndexName) & !string.IsNullOrEmpty(IndexFields)) {
                                                sb.Append("<SQLIndex");
                                                sb.Append(" Indexname=\"" + encodeXMLattribute(IndexName) + "\"");
                                                sb.Append(" DataSourceName=\"" + encodeXMLattribute(DataSourceName) + "\"");
                                                sb.Append(" TableName=\"" + encodeXMLattribute(TableName) + "\"");
                                                sb.Append(" FieldNameList=\"" + encodeXMLattribute(IndexFields) + "\"");
                                                sb.Append("></SQLIndex>\r\n");
                                            }
                                            //
                                            IndexName = ListRowSplit[0];
                                            IndexFields = ListRowSplit[1];
                                        }
                                    }
                                }
                            }
                        }
                        csData.goNext();
                    }
                }
                result = sb.ToString();
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Save the admin menus to CDef AdminMenu tags
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        private static string getAdminMenus(CoreController core) {
            string s = "";
            try {
                string appName = core.appConfig.name;
                s = s + getAdminMenus_MenuEntries(core);
                s = s + getAdminMenus_NavigatorEntries(core);
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return s;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Save the admin menus to CDef AdminMenu tags
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        private static string getAdminMenus_NavigatorEntries(CoreController core) {
            string result = "";
            try {
                string appName = core.appConfig.name;
                int MenuContentId = MetadataController.getRecordIdByUniqueName(core, "Content", NavigatorEntryModel.tableMetadata.contentName);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                using (DataTable dt = core.db.executeQuery("select * from ccMenuEntries where (contentcontrolid=" + MenuContentId + ")and(name<>'')")) {
                    if (dt.Rows.Count > 0) {
                        int NavIconType = 0;
                        string NavIconTitle = "";
                        foreach (DataRow rsDr in dt.Rows) {
                            string RecordName = GenericController.encodeText(rsDr["Name"]);
                            int ParentId = GenericController.encodeInteger(rsDr["ParentID"]);
                            string menuNameSpace = getMenuNameSpace(core, ParentId, "");
                            sb.Append("<NavigatorEntry Name=\"" + encodeXMLattribute(RecordName) + "\"");
                            sb.Append(" NameSpace=\"" + menuNameSpace + "\"");
                            sb.Append(" LinkPage=\"" + encodeXmlAttributeFieldValue(appName, rsDr, "LinkPage") + "\"");
                            sb.Append(" ContentName=\"" + encodeXmlAttributeFieldLookup(core, appName, rsDr, "ContentID", "ccContent") + "\"");
                            sb.Append(" AdminOnly=\"" + encodeXmlAttributeFieldValue(appName, rsDr, "AdminOnly") + "\"");
                            sb.Append(" DeveloperOnly=\"" + encodeXmlAttributeFieldValue(appName, rsDr, "DeveloperOnly") + "\"");
                            sb.Append(" NewWindow=\"" + encodeXmlAttributeFieldValue(appName, rsDr, "NewWindow") + "\"");
                            sb.Append(" Active=\"" + encodeXmlAttributeFieldValue(appName, rsDr, "Active") + "\"");
                            sb.Append(" AddonName=\"" + encodeXmlAttributeFieldLookup(core, appName, rsDr, "AddonID", "ccAggregateFunctions") + "\"");
                            sb.Append(" SortOrder=\"" + encodeXmlAttributeFieldValue(appName, rsDr, "SortOrder") + "\"");
                            NavIconType = GenericController.encodeInteger(encodeXmlAttributeFieldValue(appName, rsDr, "NavIconType"));
                            NavIconTitle = encodeXmlAttributeFieldValue(appName, rsDr, "NavIconTitle");
                            sb.Append(" NavIconTitle=\"" + NavIconTitle + "\"");
                            string[] SplitArray = (NavIconTypeList + ",help").Split(',');
                            int SplitIndex = NavIconType - 1;
                            if ((SplitIndex >= 0) && (SplitIndex <= SplitArray.GetUpperBound(0))) {
                                sb.Append(" NavIconType=\"" + SplitArray[SplitIndex] + "\"");
                            }
                            sb.Append(" guid=\"" + encodeXmlAttributeFieldValue(appName, rsDr, "ccGuid") + "\"");
                            sb.Append("></NavigatorEntry>\r\n");
                        }
                    }
                }
                result = sb.ToString();
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Save the admin menus to CDef AdminMenu tags
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        private static string getAdminMenus_MenuEntries(CoreController core) {
            string result = "";
            try {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                string appName = core.appConfig.name;
                int MenuContentId = MetadataController.getRecordIdByUniqueName(core, "Content", NavigatorEntryModel.tableMetadata.contentName);
                using (DataTable dt = core.db.executeQuery("select * from ccMenuEntries where (contentcontrolid=" + MenuContentId + ")and(name<>'')")) {
                    if (DbController.isDataTableOk(dt)) {
                        foreach (DataRow dr in dt.Rows) {
                            string RecordName = GenericController.encodeText(dr["Name"]);
                            sb.Append("<MenuEntry Name=\"" + encodeXMLattribute(RecordName) + "\"");
                            sb.Append(" ParentName=\"" + encodeXmlAttributeFieldLookup(core, appName, dr, "ParentID", "ccMenuEntries") + "\"");
                            sb.Append(" LinkPage=\"" + encodeXmlAttributeFieldValue(appName, dr, "LinkPage") + "\"");
                            sb.Append(" ContentName=\"" + encodeXmlAttributeFieldLookup(core, appName, dr, "ContentID", "ccContent") + "\"");
                            sb.Append(" AdminOnly=\"" + encodeXmlAttributeFieldValue(appName, dr, "AdminOnly") + "\"");
                            sb.Append(" DeveloperOnly=\"" + encodeXmlAttributeFieldValue(appName, dr, "DeveloperOnly") + "\"");
                            sb.Append(" NewWindow=\"" + encodeXmlAttributeFieldValue(appName, dr, "NewWindow") + "\"");
                            sb.Append(" Active=\"" + encodeXmlAttributeFieldValue(appName, dr, "Active") + "\"");
                            sb.Append(" AddonName=\"" + encodeXmlAttributeFieldLookup(core, appName, dr, "AddonID", "ccAggregateFunctions") + "\"");
                            sb.Append("/>\r\n");
                        }
                    }
                }
                result = sb.ToString();
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        private static string encodeXMLattribute(string Source) {
            string tempEncodeXMLattribute = HtmlController.encodeHtml(Source);
            tempEncodeXMLattribute = GenericController.vbReplace(tempEncodeXMLattribute, Environment.NewLine, " ");
            tempEncodeXMLattribute = GenericController.vbReplace(tempEncodeXMLattribute, "\r", "");
            return GenericController.vbReplace(tempEncodeXMLattribute, "\n", "");
        }
        //
        //====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="core"></param>
        /// <param name="TableName"></param>
        /// <param name="RecordID"></param>
        /// <returns></returns>
        private static string getTableRecordName(CoreController core, string TableName, int RecordID) {
            try {
                if ((RecordID != 0) && (!string.IsNullOrEmpty(TableName))) {
                    using (DataTable dt = core.db.executeQuery("select Name from " + TableName + " where ID=" + RecordID)) {
                        if (dt.Rows.Count > 0) { return dt.Rows[0][0].ToString(); }
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return "";
        }
        //
        //====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="dr"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        private static string encodeXmlAttributeFieldValue(string appName, DataRow dr, string FieldName) {
            return encodeXMLattribute(GenericController.encodeText(dr[FieldName]));
        }
        //
        //====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="core"></param>
        /// <param name="appName"></param>
        /// <param name="dr"></param>
        /// <param name="FieldName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        private static string encodeXmlAttributeFieldLookup(CoreController core, string appName, DataRow dr, string FieldName, string TableName) {
            return encodeXMLattribute(getTableRecordName(core, TableName, GenericController.encodeInteger(dr[FieldName])));
        }
        //
        //====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="core"></param>
        /// <param name="RecordID"></param>
        /// <param name="UsedIDString"></param>
        /// <returns></returns>
        private static string getMenuNameSpace(CoreController core, int RecordID, string UsedIDString) {
            string result = "";
            try {
                if (RecordID != 0) {
                    if (GenericController.vbInstr(1, "," + UsedIDString + ",", "," + RecordID + ",", 1) != 0) {
                        LogController.logError(core, "getMenuNameSpace, Circular reference found in UsedIDString [" + UsedIDString + "] getting ccMenuEntries namespace for recordid [" + RecordID + "]");
                    } else {
                        UsedIDString = UsedIDString + "," + RecordID;
                        int ParentId = 0;
                        string RecordName = "";
                        if (RecordID != 0) {
                            using (DataTable dt = core.db.executeQuery("select Name,ParentID from ccMenuEntries where ID=" + RecordID)) {
                                if (DbController.isDataTableOk(dt)) {
                                    ParentId = GenericController.encodeInteger(dt.Rows[0]["ParentID"]);
                                    RecordName = GenericController.encodeText(dt.Rows[0]["Name"]);
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(RecordName)) {
                            if (ParentId == RecordID) {
                                //
                                // circular reference
                                LogController.logError(core, "getMenuNameSpace, Circular reference found (ParentID=RecordID) getting ccMenuEntries namespace for recordid [" + RecordID + "]");
                            } else {
                                string ParentSpace = "";
                                if (ParentId != 0) {
                                    //
                                    // get next parent
                                    ParentSpace = getMenuNameSpace(core, ParentId, UsedIDString);
                                }
                                if (!string.IsNullOrEmpty(ParentSpace)) {
                                    result = ParentSpace + "." + RecordName;
                                } else {
                                    result = RecordName;
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="RecordID"></param>
        /// <param name="Cache"></param>
        /// <returns></returns>
        private static string cacheLookup(int RecordID, object[,] Cache) {
            if (RecordID != 0) {
                for (int Ptr = 0; Ptr <= Cache.GetUpperBound(1); Ptr++) {
                    if (GenericController.encodeInteger(Cache[0, Ptr]) == RecordID) {
                        return GenericController.encodeText(Cache[1, Ptr]);
                    }
                }
            }
            return "";
        }
        //
        //====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        private static string xmlValueText(object Source) {
            return encodeXMLattribute(GenericController.encodeText(Source));
        }
        //
        //====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        private static string xmlValueBoolean(object Source) {
            return encodeText(GenericController.encodeBoolean(GenericController.encodeText(Source)));
        }
    }
}