
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Controllers {
    //
    /// <summary>
    /// Handle the nitty-gritty of reading and writing the xml file for collections and CDefMiniCollections
    /// </summary>
    public static class ApplicationCDefMiniCollection {
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

        private const int f_ID = 0;
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
        private const int f_RedirectID = 14;
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
        private const int f_LookupContentID = 34;
        private const int f_RedirectContentID = 35;
        private const int f_ManyToManyContentID = 36;
        private const int f_ManyToManyRuleContentID = 37;
        private const int f_helpdefault = 38;
        private const int f_helpcustom = 39;
        private const int f_IndexWidth = 40;
        //
        //====================================================================================================
        /// <summary>
        /// return an xml collection of the current application (all addon collections, etc)
        /// </summary>
        /// <param name="IncludeBaseFields"></param>
        /// <returns></returns>
        public static string get( CoreController core, bool IncludeBaseFields = false) {
            string tempGetXMLContentDefinition3 = null;
            try {
                //
                string ContentName = "";
                int FieldCnt = 0;
                string FieldName = null;
                int FieldContentID = 0;
                int LastFieldID = 0;
                int RecordID = 0;
                string RecordName = null;
                int AuthoringTableID = 0;
                string HelpDefault = null;
                int HelpCnt = 0;
                int fieldId = 0;
                string fieldType = null;
                int ContentTableID = 0;
                string TableName = null;
                string DataSourceName = null;
                int DefaultSortMethodID = 0;
                string DefaultSortMethod = null;
                int EditorGroupID = 0;
                string EditorGroupName = null;
                int ParentID = 0;
                string ParentName = null;
                int ContentID = 0;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                string iContentName = null;
                DataTable dt = null;
                string SQL = null;
                bool FoundMenuTable = false;
                int Ptr = 0;
                string[,] Tables = null;
                int TableCnt = 0;
                string[,] Sorts = null;
                int SortCnt = 0;
                string[,] Groups = null;
                int GroupCnt = 0;
                string[,] Contents = null;
                int ContentCnt = 0;
                string[,] CFields = null;
                int CFieldCnt = 0;
                int CFieldPtr = 0;
                string appName;
                //
                appName = core.appConfig.name;
                iContentName = ContentName;
                if (!string.IsNullOrEmpty(iContentName)) {
                    SQL = "select id from cccontent where name=" + core.db.encodeSQLText(iContentName);
                    dt = core.db.executeQuery(SQL);
                    if (dt.Rows.Count > 0) {
                        ContentID = GenericController.encodeInteger(dt.Rows[0]["id"]);
                    }
                }
                if (!string.IsNullOrEmpty(iContentName) && (ContentID == 0)) {
                    //
                    // export requested for content name that does not exist - return blank
                    //
                } else {
                    //
                    // Build table lookup
                    //
                    SQL = "select T.ID,T.Name as TableName,D.Name as DataSourceName from ccTables T Left Join ccDataSources D on D.ID=T.DataSourceID";
                    dt = core.db.executeQuery(SQL);
                    Tables = core.db.convertDataTabletoArray(dt);
                    if (Tables == null) {
                        TableCnt = 0;
                    } else {
                        TableCnt = Tables.GetUpperBound(1) + 1;
                    }
                    //
                    // Build SortMethod lookup
                    //
                    SQL = "select ID,Name from ccSortMethods";
                    dt = core.db.executeQuery(SQL);
                    Sorts = core.db.convertDataTabletoArray(dt);
                    if (Sorts == null) {
                        SortCnt = 0;
                    } else {
                        SortCnt = Sorts.GetUpperBound(1) + 1;
                    }
                    //
                    // Build SortMethod lookup
                    //
                    SQL = "select ID,Name from ccGroups";
                    dt = core.db.executeQuery(SQL);
                    Groups = core.db.convertDataTabletoArray(dt);
                    if (Groups == null) {
                        GroupCnt = 0;
                    } else {
                        GroupCnt = Groups.GetUpperBound(1) + 1;
                    }
                    //
                    // Build Content lookup
                    //
                    SQL = "select id,name from ccContent";
                    dt = core.db.executeQuery(SQL);
                    Contents = core.db.convertDataTabletoArray(dt);
                    if (Contents == null) {
                        ContentCnt = 0;
                    } else {
                        ContentCnt = Contents.GetUpperBound(1) + 1;
                    }
                    //
                    // select all the fields
                    //
                    if (ContentID != 0) {
                        SQL = "select " + FieldSelectList + ""
                            + " from ccfields f left join ccfieldhelp h on h.fieldid=f.id"
                            + " where (f.Type<>0)and(f.contentid=" + ContentID + ")"
                            + "";
                    } else {
                        SQL = "select " + FieldSelectList + ""
                            + " from ccfields f left join ccfieldhelp h on h.fieldid=f.id"
                            + " where (f.Type<>0)"
                            + "";
                    }
                    if (!IncludeBaseFields) {
                        SQL += " and ((f.IsBaseField is null)or(f.IsBaseField=0))";
                    }
                    SQL += " order by f.contentid,f.id,h.id desc";
                    dt = core.db.executeQuery(SQL);
                    CFields = core.db.convertDataTabletoArray(dt);
                    CFieldCnt = CFields.GetUpperBound(1) + 1;
                    //
                    // select the content
                    //
                    if (ContentID != 0) {
                        SQL = "select " + ContentSelectList + " from ccContent where (id=" + ContentID + ")and(contenttableid is not null)and(contentcontrolid is not null) order by id";
                    } else {
                        SQL = "select " + ContentSelectList + " from ccContent where (name<>'')and(name is not null)and(contenttableid is not null)and(contentcontrolid is not null) order by id";
                    }
                    dt = core.db.executeQuery(SQL);
                    //
                    // create output
                    //
                    CFieldPtr = 0;
                    foreach (DataRow dr in dt.Rows) {
                        //
                        // ----- <cdef>
                        //
                        iContentName = encodeXmlAttributeFieldValue(appName, dr, "Name");
                        ContentID = GenericController.encodeInteger(dr["ID"]);
                        sb.Append("\r\n\t<CDef");
                        sb.Append(" Name=\"" + iContentName + "\"");
                        if ((!encodeBoolean(dr["isBaseContent"])) || IncludeBaseFields) {
                            sb.Append(" Active=\"" + encodeXmlAttributeFieldValue(appName, dr, "Active") + "\"");
                            sb.Append(" AdminOnly=\"" + encodeXmlAttributeFieldValue(appName, dr, "AdminOnly") + "\"");
                            //sb.Append( " AliasID=""" & GetRSXMLAttribute( appname,RS, "AliasID") & """")
                            //sb.Append( " AliasName=""" & GetRSXMLAttribute( appname,RS, "AliasName") & """")
                            sb.Append(" AllowAdd=\"" + encodeXmlAttributeFieldValue(appName, dr, "AllowAdd") + "\"");
                            sb.Append(" AllowCalendarEvents=\"" + encodeXmlAttributeFieldValue(appName, dr, "AllowCalendarEvents") + "\"");
                            sb.Append(" AllowContentChildTool=\"" + encodeXmlAttributeFieldValue(appName, dr, "AllowContentChildTool") + "\"");
                            sb.Append(" AllowContentTracking=\"" + encodeXmlAttributeFieldValue(appName, dr, "AllowContentTracking") + "\"");
                            sb.Append(" AllowDelete=\"" + encodeXmlAttributeFieldValue(appName, dr, "AllowDelete") + "\"");
                            //sb.Append(" AllowMetaContent=""" & GetRSXMLAttribute(appName, dr, "AllowMetaContent") & """")
                            sb.Append(" AllowTopicRules=\"" + encodeXmlAttributeFieldValue(appName, dr, "AllowTopicRules") + "\"");
                            sb.Append(" AllowWorkflowAuthoring=\"" + encodeXmlAttributeFieldValue(appName, dr, "AllowWorkflowAuthoring") + "\"");
                            //
                            AuthoringTableID = GenericController.encodeInteger(dr["AuthoringTableID"]);
                            TableName = "";
                            DataSourceName = "";
                            if (AuthoringTableID != 0) {
                                for (Ptr = 0; Ptr < TableCnt; Ptr++) {
                                    if (GenericController.encodeInteger(Tables[0, Ptr]) == AuthoringTableID) {
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
                            ContentTableID = GenericController.encodeInteger(dr["ContentTableID"]);
                            if (ContentTableID != AuthoringTableID) {
                                if (ContentTableID != 0) {
                                    TableName = "";
                                    DataSourceName = "";
                                    for (Ptr = 0; Ptr < TableCnt; Ptr++) {
                                        if (GenericController.encodeInteger(Tables[0, Ptr]) == ContentTableID) {
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
                            DefaultSortMethodID = GenericController.encodeInteger(dr["DefaultSortMethodID"]);
                            DefaultSortMethod = cacheLookup(DefaultSortMethodID, Sorts);
                            sb.Append(" DefaultSortMethod=\"" + encodeXMLattribute(DefaultSortMethod) + "\"");
                            //
                            sb.Append(" DeveloperOnly=\"" + encodeXmlAttributeFieldValue(appName, dr, "DeveloperOnly") + "\"");
                            sb.Append(" DropDownFieldList=\"" + encodeXmlAttributeFieldValue(appName, dr, "DropDownFieldList") + "\"");
                            //
                            EditorGroupID = GenericController.encodeInteger(dr["EditorGroupID"]);
                            EditorGroupName = cacheLookup(EditorGroupID, Groups);
                            sb.Append(" EditorGroupName=\"" + encodeXMLattribute(EditorGroupName) + "\"");
                            //
                            ParentID = GenericController.encodeInteger(dr["ParentID"]);
                            ParentName = cacheLookup(ParentID, Contents);
                            sb.Append(" Parent=\"" + encodeXMLattribute(ParentName) + "\"");
                            //
                            sb.Append(" IconLink=\"" + encodeXmlAttributeFieldValue(appName, dr, "IconLink") + "\"");
                            sb.Append(" IconHeight=\"" + encodeXmlAttributeFieldValue(appName, dr, "IconHeight") + "\"");
                            sb.Append(" IconWidth=\"" + encodeXmlAttributeFieldValue(appName, dr, "IconWidth") + "\"");
                            sb.Append(" IconSprites=\"" + encodeXmlAttributeFieldValue(appName, dr, "IconSprites") + "\"");
                            sb.Append(" isbasecontent=\"" + encodeXmlAttributeFieldValue(appName, dr, "IsBaseContent") + "\"");
                        }
                        sb.Append(" guid=\"" + encodeXmlAttributeFieldValue(appName, dr, "ccGuid") + "\"");
                        sb.Append(" >");
                        //
                        // ----- <field>
                        //
                        FieldCnt = 0;
                        fieldId = 0;
                        while (CFieldPtr < CFieldCnt) {
                            LastFieldID = fieldId;
                            fieldId = GenericController.encodeInteger(CFields[f_ID, CFieldPtr]);
                            FieldName = GenericController.encodeText(CFields[f_Name, CFieldPtr]);
                            FieldContentID = GenericController.encodeInteger(CFields[f_contentid, CFieldPtr]);
                            if (FieldContentID > ContentID) {
                                break;
                            } else if ((FieldContentID == ContentID) && (fieldId != LastFieldID)) {
                                if (IncludeBaseFields || (",id,dateadded,createdby,modifiedby,ContentControlID,CreateKey,ModifiedDate,ccguid,".IndexOf("," + FieldName + ",", System.StringComparison.OrdinalIgnoreCase)  == -1)) {
                                    sb.Append("\r\n\t\t<Field");
                                    fieldType = core.db.getFieldTypeNameFromFieldTypeId(encodeInteger(CFields[f_Type, CFieldPtr]));
                                    sb.Append(" Name=\"" + xmlValueText(FieldName) + "\"");
                                    sb.Append(" active=\"" + xmlValueBoolean(CFields[f_Active, CFieldPtr]) + "\"");
                                    sb.Append(" AdminOnly=\"" + xmlValueBoolean(CFields[f_AdminOnly, CFieldPtr]) + "\"");
                                    sb.Append(" Authorable=\"" + xmlValueBoolean(CFields[f_Authorable, CFieldPtr]) + "\"");
                                    sb.Append(" Caption=\"" + xmlValueText(CFields[f_Caption, CFieldPtr]) + "\"");
                                    sb.Append(" DeveloperOnly=\"" + xmlValueBoolean(CFields[f_DeveloperOnly, CFieldPtr]) + "\"");
                                    sb.Append(" EditSortPriority=\"" + xmlValueText(CFields[f_EditSortPriority, CFieldPtr]) + "\"");
                                    sb.Append(" FieldType=\"" + fieldType + "\"");
                                    sb.Append(" HTMLContent=\"" + xmlValueBoolean(CFields[f_HTMLContent, CFieldPtr]) + "\"");
                                    sb.Append(" IndexColumn=\"" + xmlValueText(CFields[f_IndexColumn, CFieldPtr]) + "\"");
                                    sb.Append(" IndexSortDirection=\"" + xmlValueText(CFields[f_IndexSortDirection, CFieldPtr]) + "\"");
                                    sb.Append(" IndexSortOrder=\"" + xmlValueText(CFields[f_IndexSortPriority, CFieldPtr]) + "\"");
                                    sb.Append(" IndexWidth=\"" + xmlValueText(CFields[f_IndexWidth, CFieldPtr]) + "\"");
                                    sb.Append(" RedirectID=\"" + xmlValueText(CFields[f_RedirectID, CFieldPtr]) + "\"");
                                    sb.Append(" RedirectPath=\"" + xmlValueText(CFields[f_RedirectPath, CFieldPtr]) + "\"");
                                    sb.Append(" Required=\"" + xmlValueBoolean(CFields[f_Required, CFieldPtr]) + "\"");
                                    sb.Append(" TextBuffered=\"" + xmlValueBoolean(CFields[f_TextBuffered, CFieldPtr]) + "\"");
                                    sb.Append(" UniqueName=\"" + xmlValueBoolean(CFields[f_UniqueName, CFieldPtr]) + "\"");
                                    sb.Append(" DefaultValue=\"" + xmlValueText(CFields[f_DefaultValue, CFieldPtr]) + "\"");
                                    sb.Append(" RSSTitle=\"" + xmlValueBoolean(CFields[f_RSSTitleField, CFieldPtr]) + "\"");
                                    sb.Append(" RSSDescription=\"" + xmlValueBoolean(CFields[f_RSSDescriptionField, CFieldPtr]) + "\"");
                                    sb.Append(" EditTab=\"" + xmlValueText(CFields[f_EditTab, CFieldPtr]) + "\"");
                                    sb.Append(" Scramble=\"" + xmlValueBoolean(CFields[f_Scramble, CFieldPtr]) + "\"");
                                    sb.Append(" LookupList=\"" + xmlValueText(CFields[f_LookupList, CFieldPtr]) + "\"");
                                    sb.Append(" NotEditable=\"" + xmlValueBoolean(CFields[f_NotEditable, CFieldPtr]) + "\"");
                                    sb.Append(" Password=\"" + xmlValueBoolean(CFields[f_Password, CFieldPtr]) + "\"");
                                    sb.Append(" ReadOnly=\"" + xmlValueBoolean(CFields[f_ReadOnly, CFieldPtr]) + "\"");
                                    sb.Append(" ManyToManyRulePrimaryField=\"" + xmlValueText(CFields[f_ManyToManyRulePrimaryField, CFieldPtr]) + "\"");
                                    sb.Append(" ManyToManyRuleSecondaryField=\"" + xmlValueText(CFields[f_ManyToManyRuleSecondaryField, CFieldPtr]) + "\"");
                                    sb.Append(" IsModified=\"" + (encodeInteger(CFields[f_ModifiedBy, CFieldPtr]) != 0) + "\"");
                                    if (true) {
                                        sb.Append(" IsBaseField=\"" + xmlValueBoolean(CFields[f_IsBaseField, CFieldPtr]) + "\"");
                                    }
                                    //
                                    RecordID = GenericController.encodeInteger(CFields[f_LookupContentID, CFieldPtr]);
                                    RecordName = cacheLookup(RecordID, Contents);
                                    sb.Append(" LookupContent=\"" + HtmlController.encodeHtml(RecordName) + "\"");
                                    //
                                    RecordID = GenericController.encodeInteger(CFields[f_RedirectContentID, CFieldPtr]);
                                    RecordName = cacheLookup(RecordID, Contents);
                                    sb.Append(" RedirectContent=\"" + HtmlController.encodeHtml(RecordName) + "\"");
                                    //
                                    RecordID = GenericController.encodeInteger(CFields[f_ManyToManyContentID, CFieldPtr]);
                                    RecordName = cacheLookup(RecordID, Contents);
                                    sb.Append(" ManyToManyContent=\"" + HtmlController.encodeHtml(RecordName) + "\"");
                                    //
                                    RecordID = GenericController.encodeInteger(CFields[f_ManyToManyRuleContentID, CFieldPtr]);
                                    RecordName = cacheLookup(RecordID, Contents);
                                    sb.Append(" ManyToManyRuleContent=\"" + HtmlController.encodeHtml(RecordName) + "\"");
                                    //
                                    RecordID = GenericController.encodeInteger(CFields[f_MemberSelectGroupId, CFieldPtr]);
                                    RecordName = "";
                                    if (RecordID>0) {
                                        RecordName = core.db.getRecordName("groups", RecordID);
                                    }
                                    sb.Append(" MemberSelectGroup=\"" + xmlValueText(CFields[f_MemberSelectGroupId, CFieldPtr]) + "\"");


                                    sb.Append(" >");
                                    //
                                    HelpCnt = 0;
                                    HelpDefault = xmlValueText(CFields[f_helpcustom, CFieldPtr]);
                                    if (string.IsNullOrEmpty(HelpDefault)) {
                                        HelpDefault = xmlValueText(CFields[f_helpdefault, CFieldPtr]);
                                    }
                                    if (!string.IsNullOrEmpty(HelpDefault)) {
                                        sb.Append("\r\n\t\t\t<HelpDefault>" + HelpDefault + "</HelpDefault>");
                                        HelpCnt = HelpCnt + 1;
                                    }
                                    if (HelpCnt > 0) {
                                        sb.Append("\r\n\t\t");
                                    }
                                    sb.Append("</Field>");
                                }
                                FieldCnt = FieldCnt + 1;
                            }
                            CFieldPtr = CFieldPtr + 1;
                        }
                        //
                        if (FieldCnt > 0) {
                            sb.Append("\r\n\t");
                        }
                        sb.Append("</CDef>");
                    }
                    if (string.IsNullOrEmpty(ContentName)) {
                        //
                        // Add other areas of the CDef file
                        sb.Append(getSQLIndexes(core));
                        if (FoundMenuTable) {
                            sb.Append(getAdminMenus(core));
                        }
                    }
                    tempGetXMLContentDefinition3 = "<" + CollectionFileRootNode + " name=\"Application\" guid=\"" + ApplicationCollectionGuid + "\">" + sb.ToString() + "\r\n</" + CollectionFileRootNode + ">";
                }
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
            return tempGetXMLContentDefinition3;
        }
        //
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
                string SQL  = ""
                    + " select D.name as DataSourceName,T.name as TableName"
                    + " from cctables T left join ccDataSources d on D.ID=T.DataSourceID"
                    + " where t.active<>0";
                int CS = core.db.csOpenSql(SQL);
                while (core.db.csOk(CS)) {
                    string DataSourceName = core.db.csGetText(CS, "DataSourceName");
                    string TableName = core.db.csGetText(CS, "TableName");
                    string IndexList = core.db.getSQLIndexList(DataSourceName, TableName);
                    //
                    if (!string.IsNullOrEmpty(IndexList)) {
                        string[] ListRows = GenericController.stringSplit(IndexList, "\r\n");
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
                    core.db.csGoNext(CS);
                }
                core.db.csClose(ref CS);
                result = sb.ToString();
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
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
                s = s + getAdminMenus_MenuEntries( core );
                s = s + getAdminMenus_NavigatorEntries(core);
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
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
                int NavIconType = 0;
                string NavIconTitle = null;
                 System.Text.StringBuilder sb = new System.Text.StringBuilder();
                DataTable dt = null;
                string menuNameSpace = null;
                string RecordName = null;
                int ParentID = 0;
                int MenuContentID = 0;
                string[] SplitArray = null;
                int SplitIndex = 0;
                string appName;

                //
                // ****************************** if cdef not loaded, this fails
                //
                appName = core.appConfig.name;
                MenuContentID = core.db.getRecordID("Content", cnNavigatorEntries);
                dt = core.db.executeQuery("select * from ccMenuEntries where (contentcontrolid=" + MenuContentID + ")and(name<>'')");
                if (dt.Rows.Count > 0) {
                    NavIconType = 0;
                    NavIconTitle = "";
                    foreach (DataRow rsDr in dt.Rows) {
                        RecordName = GenericController.encodeText(rsDr["Name"]);
                        ParentID = GenericController.encodeInteger(rsDr["ParentID"]);
                        menuNameSpace = getMenuNameSpace(core, ParentID, "");
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
                        SplitArray = (NavIconTypeList + ",help").Split(',');
                        SplitIndex = NavIconType - 1;
                        if ((SplitIndex >= 0) && (SplitIndex <= SplitArray.GetUpperBound(0))) {
                            sb.Append(" NavIconType=\"" + SplitArray[SplitIndex] + "\"");
                        }
                        sb.Append(" guid=\"" + encodeXmlAttributeFieldValue(appName, rsDr, "ccGuid") + "\"");
                        sb.Append("></NavigatorEntry>\r\n");
                    }
                }
                result = sb.ToString();
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
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
                int MenuContentID = core.db.getRecordID("Content", cnNavigatorEntries);
                DataTable rs = core.db.executeQuery("select * from ccMenuEntries where (contentcontrolid=" + MenuContentID + ")and(name<>'')");
                if (DbController.isDataTableOk(rs)) {
                    if (true) {
                        foreach (DataRow dr in rs.Rows) {
                            string RecordName = GenericController.encodeText(dr["Name"]);
                            sb.Append("<MenuEntry Name=\"" + encodeXMLattribute(RecordName) + "\"");
                            sb.Append(" ParentName=\"" + encodeXmlAttributeFieldLookup(core, appName, dr, "ParentID", "ccMenuEntries") + "\"");
                            sb.Append(" LinkPage=\"" + encodeXmlAttributeFieldValue(appName, dr, "LinkPage") + "\"");
                            sb.Append(" ContentName=\"" + encodeXmlAttributeFieldLookup(core, appName, dr, "ContentID", "ccContent") + "\"");
                            sb.Append(" AdminOnly=\"" + encodeXmlAttributeFieldValue(appName, dr, "AdminOnly") + "\"");
                            sb.Append(" DeveloperOnly=\"" + encodeXmlAttributeFieldValue(appName, dr, "DeveloperOnly") + "\"");
                            sb.Append(" NewWindow=\"" + encodeXmlAttributeFieldValue(appName, dr, "NewWindow") + "\"");
                            sb.Append(" Active=\"" + encodeXmlAttributeFieldValue(appName, dr, "Active") + "\"");
                            if (true) {
                                sb.Append(" AddonName=\"" + encodeXmlAttributeFieldLookup(core, appName, dr, "AddonID", "ccAggregateFunctions") + "\"");
                            }
                            sb.Append("/>\r\n");
                        }
                    }
                }
                result = sb.ToString();
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
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
            tempEncodeXMLattribute = GenericController.vbReplace(tempEncodeXMLattribute, "\r\n", " ");
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
            string tempGetTableRecordName = null;
            try {
                //
                DataTable dt = null;
                string appName;
                //
                appName = core.appConfig.name;
                if (RecordID != 0 & !string.IsNullOrEmpty(TableName)) {
                    dt = core.db.executeQuery("select Name from " + TableName + " where ID=" + RecordID);
                    if (dt.Rows.Count > 0) {
                        tempGetTableRecordName = dt.Rows[0][0].ToString();
                    }
                }
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
            return tempGetTableRecordName;
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
            string tempgetMenuNameSpace = null;
            try {
                //
                DataTable rs = null;
                int ParentID = 0;
                string RecordName = "";
                string ParentSpace = "";
                string appName;
                //
                appName = core.appConfig.name;
                if (RecordID != 0) {
                    if (GenericController.vbInstr(1, "," + UsedIDString + ",", "," + RecordID + ",", 1) != 0) {
                        LogController.logError(core, "getMenuNameSpace, Circular reference found in UsedIDString [" + UsedIDString + "] getting ccMenuEntries namespace for recordid [" + RecordID + "]");
                        tempgetMenuNameSpace = "";
                    } else {
                        UsedIDString = UsedIDString + "," + RecordID;
                        ParentID = 0;
                        if (RecordID != 0) {
                            rs = core.db.executeQuery("select Name,ParentID from ccMenuEntries where ID=" + RecordID);
                            if (DbController.isDataTableOk(rs)) {
                                ParentID = GenericController.encodeInteger(rs.Rows[0]["ParentID"]);
                                RecordName = GenericController.encodeText(rs.Rows[0]["Name"]);
                            }
                            if (DbController.isDataTableOk(rs)) {
                                if (false) {
                                    //RS.Close()
                                }
                                //RS = Nothing
                            }
                        }
                        if (!string.IsNullOrEmpty(RecordName)) {
                            if (ParentID == RecordID) {
                                //
                                // circular reference
                                //
                                LogController.logError(core, "getMenuNameSpace, Circular reference found (ParentID=RecordID) getting ccMenuEntries namespace for recordid [" + RecordID + "]");
                                tempgetMenuNameSpace = "";
                            } else {
                                if (ParentID != 0) {
                                    //
                                    // get next parent
                                    //
                                    ParentSpace = getMenuNameSpace(core, ParentID, UsedIDString);
                                }
                                if (!string.IsNullOrEmpty(ParentSpace)) {
                                    tempgetMenuNameSpace = ParentSpace + "." + RecordName;
                                } else {
                                    tempgetMenuNameSpace = RecordName;
                                }
                            }
                        } else {
                            tempgetMenuNameSpace = "";
                        }
                    }
                }
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
            return tempgetMenuNameSpace;
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
            string tempCacheLookup = null;
            //
            int Ptr = 0;
            //
            tempCacheLookup = "";
            if (RecordID != 0) {
                for (Ptr = 0; Ptr <= Cache.GetUpperBound(1); Ptr++) {
                    if (GenericController.encodeInteger(Cache[0, Ptr]) == RecordID) {
                        tempCacheLookup = GenericController.encodeText(Cache[1, Ptr]);
                        break;
                    }
                }
            }
            return tempCacheLookup;
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