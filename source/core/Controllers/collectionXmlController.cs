
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
namespace Contensive.Core.Controllers {
    public class collectionXmlController {
        //
        // ----- global scope variables
        //
        private const string ApplicationNameLocal = "unknown";
        private coreClass cpCore;
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
        /// constructor
        /// </summary>
        public collectionXmlController(coreClass cpCore) {
            this.cpCore = cpCore;
        }
        //
        //========================================================================
        // ----- Save all content to an XML Stream
        //   4/28/08 - changed so content is read from Db using RS/Conn, not cache version
        //   2/20/2010 - changed to include includebasefield
        //========================================================================
        //
        public string getApplicationCollectionXml(bool IncludeBaseFields = false) {
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
                appName = cpCore.serverConfig.appConfig.name;
                iContentName = ContentName;
                if (!string.IsNullOrEmpty(iContentName)) {
                    SQL = "select id from cccontent where name=" + cpCore.db.encodeSQLText(iContentName);
                    dt = cpCore.db.executeQuery(SQL);
                    if (dt.Rows.Count > 0) {
                        ContentID = genericController.encodeInteger(dt.Rows[0]["id"]);
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
                    dt = cpCore.db.executeQuery(SQL);
                    Tables = cpCore.db.convertDataTabletoArray(dt);
                    if (Tables == null) {
                        TableCnt = 0;
                    } else {
                        TableCnt = Tables.GetUpperBound(1) + 1;
                    }
                    //
                    // Build SortMethod lookup
                    //
                    SQL = "select ID,Name from ccSortMethods";
                    dt = cpCore.db.executeQuery(SQL);
                    Sorts = cpCore.db.convertDataTabletoArray(dt);
                    if (Sorts == null) {
                        SortCnt = 0;
                    } else {
                        SortCnt = Sorts.GetUpperBound(1) + 1;
                    }
                    //
                    // Build SortMethod lookup
                    //
                    SQL = "select ID,Name from ccGroups";
                    dt = cpCore.db.executeQuery(SQL);
                    Groups = cpCore.db.convertDataTabletoArray(dt);
                    if (Groups == null) {
                        GroupCnt = 0;
                    } else {
                        GroupCnt = Groups.GetUpperBound(1) + 1;
                    }
                    //
                    // Build Content lookup
                    //
                    SQL = "select id,name from ccContent";
                    dt = cpCore.db.executeQuery(SQL);
                    Contents = cpCore.db.convertDataTabletoArray(dt);
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
                    dt = cpCore.db.executeQuery(SQL);
                    CFields = cpCore.db.convertDataTabletoArray(dt);
                    CFieldCnt = CFields.GetUpperBound(1) + 1;
                    //
                    // select the content
                    //
                    if (ContentID != 0) {
                        SQL = "select " + ContentSelectList + " from ccContent where (id=" + ContentID + ")and(contenttableid is not null)and(contentcontrolid is not null) order by id";
                    } else {
                        SQL = "select " + ContentSelectList + " from ccContent where (name<>'')and(name is not null)and(contenttableid is not null)and(contentcontrolid is not null) order by id";
                    }
                    dt = cpCore.db.executeQuery(SQL);
                    //
                    // create output
                    //
                    CFieldPtr = 0;
                    foreach (DataRow dr in dt.Rows) {
                        //
                        // ----- <cdef>
                        //
                        iContentName = encodeXmlAttributeFieldValue(appName, dr, "Name");
                        ContentID = genericController.encodeInteger(dr["ID"]);
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
                            AuthoringTableID = genericController.encodeInteger(dr["AuthoringTableID"]);
                            TableName = "";
                            DataSourceName = "";
                            if (AuthoringTableID != 0) {
                                for (Ptr = 0; Ptr < TableCnt; Ptr++) {
                                    if (genericController.encodeInteger(Tables[0, Ptr]) == AuthoringTableID) {
                                        TableName = genericController.encodeText(Tables[1, Ptr]);
                                        DataSourceName = genericController.encodeText(Tables[2, Ptr]);
                                        break;
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(DataSourceName)) {
                                DataSourceName = "Default";
                            }
                            if (genericController.vbUCase(TableName) == "CCMENUENTRIES") {
                                FoundMenuTable = true;
                            }
                            sb.Append(" AuthoringDataSourceName=\"" + encodeXMLattribute(DataSourceName) + "\"");
                            sb.Append(" AuthoringTableName=\"" + encodeXMLattribute(TableName) + "\"");
                            //
                            ContentTableID = genericController.encodeInteger(dr["ContentTableID"]);
                            if (ContentTableID != AuthoringTableID) {
                                if (ContentTableID != 0) {
                                    TableName = "";
                                    DataSourceName = "";
                                    for (Ptr = 0; Ptr < TableCnt; Ptr++) {
                                        if (genericController.encodeInteger(Tables[0, Ptr]) == ContentTableID) {
                                            TableName = genericController.encodeText(Tables[1, Ptr]);
                                            DataSourceName = genericController.encodeText(Tables[2, Ptr]);
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
                            DefaultSortMethodID = genericController.encodeInteger(dr["DefaultSortMethodID"]);
                            DefaultSortMethod = CacheLookup(DefaultSortMethodID, Sorts);
                            sb.Append(" DefaultSortMethod=\"" + encodeXMLattribute(DefaultSortMethod) + "\"");
                            //
                            sb.Append(" DeveloperOnly=\"" + encodeXmlAttributeFieldValue(appName, dr, "DeveloperOnly") + "\"");
                            sb.Append(" DropDownFieldList=\"" + encodeXmlAttributeFieldValue(appName, dr, "DropDownFieldList") + "\"");
                            //
                            EditorGroupID = genericController.encodeInteger(dr["EditorGroupID"]);
                            EditorGroupName = CacheLookup(EditorGroupID, Groups);
                            sb.Append(" EditorGroupName=\"" + encodeXMLattribute(EditorGroupName) + "\"");
                            //
                            ParentID = genericController.encodeInteger(dr["ParentID"]);
                            ParentName = CacheLookup(ParentID, Contents);
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
                            fieldId = genericController.encodeInteger(CFields[f_ID, CFieldPtr]);
                            FieldName = genericController.encodeText(CFields[f_Name, CFieldPtr]);
                            FieldContentID = genericController.encodeInteger(CFields[f_contentid, CFieldPtr]);
                            if (FieldContentID > ContentID) {
                                break;
                            } else if ((FieldContentID == ContentID) && (fieldId != LastFieldID)) {
                                if (IncludeBaseFields || (",id,dateadded,createdby,modifiedby,ContentControlID,CreateKey,ModifiedDate,ccguid,".IndexOf("," + FieldName + ",", System.StringComparison.OrdinalIgnoreCase)  == -1)) {
                                    sb.Append("\r\n\t\t<Field");
                                    fieldType = cpCore.db.getFieldTypeNameFromFieldTypeId(encodeInteger(CFields[f_Type, CFieldPtr]));
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
                                    RecordID = genericController.encodeInteger(CFields[f_LookupContentID, CFieldPtr]);
                                    RecordName = CacheLookup(RecordID, Contents);
                                    sb.Append(" LookupContent=\"" + genericController.encodeHTML(RecordName) + "\"");
                                    //
                                    RecordID = genericController.encodeInteger(CFields[f_RedirectContentID, CFieldPtr]);
                                    RecordName = CacheLookup(RecordID, Contents);
                                    sb.Append(" RedirectContent=\"" + genericController.encodeHTML(RecordName) + "\"");
                                    //
                                    RecordID = genericController.encodeInteger(CFields[f_ManyToManyContentID, CFieldPtr]);
                                    RecordName = CacheLookup(RecordID, Contents);
                                    sb.Append(" ManyToManyContent=\"" + genericController.encodeHTML(RecordName) + "\"");
                                    //
                                    RecordID = genericController.encodeInteger(CFields[f_ManyToManyRuleContentID, CFieldPtr]);
                                    RecordName = CacheLookup(RecordID, Contents);
                                    sb.Append(" ManyToManyRuleContent=\"" + genericController.encodeHTML(RecordName) + "\"");
                                    //
                                    RecordID = genericController.encodeInteger(CFields[f_MemberSelectGroupId, CFieldPtr]);
                                    RecordName = "";
                                    if (RecordID>0) {
                                        RecordName = cpCore.db.getRecordName("groups", RecordID);
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
                        sb.Append(GetXMLContentDefinition_SQLIndexes());
                        if (FoundMenuTable) {
                            sb.Append(GetXMLContentDefinition_AdminMenus());
                        }
                    }
                    tempGetXMLContentDefinition3 = "<" + CollectionFileRootNode + " name=\"Application\" guid=\"" + ApplicationCollectionGuid + "\">" + sb.ToString() + "\r\n</" + CollectionFileRootNode + ">";
                }
            } catch( Exception ex ) {
                cpCore.handleException(ex);
            }
            return tempGetXMLContentDefinition3;
        }
        ////
        ////========================================================================
        //// ----- Save all content to an XML Stream
        ////   4/28/08 - changed so content is read from Db using RS/Conn, not cache version
        ////========================================================================
        ////
        //public string GetXMLContentDefinition(string ContentName = "") {
        //    return GetXMLContentDefinition3(ContentName, false);
        //}
        //
        //========================================================================
        // ----- Save all content to an XML Stream
        //========================================================================
        //
        //Private Function GetXMLContent(cmc as appServicesClass, ContentName As String) As String
        //    On Error GoTo ErrorTrap
        //    '
        //    Dim CS as integer
        //    Dim sb as new system.text.stringBuilder
        //    Dim CDefPointer as integer
        //    Dim CDefArrayCount as integer
        //    Dim CSRows as object
        //    Dim CSRowCaptions as object
        //    Dim RowCount as integer
        //    Dim RowPointer as integer
        //    Dim ColumnCount as integer
        //    Dim ColumnPointer as integer
        //    '
        //    sb.append( "<ContensiveContent>" & vbCrLf)
        //    If ContentName <> "" Then
        //        Call sb.append("<CDef Name=""" & ContentName & """>" & vbCrLf)
        //        CS = cpCore.csOpen(ContentName)
        //        CSRows = cpCore.Csv_cs_getRows(CS)
        //        RowCount = UBound(CSRows, 2)
        //        CSRowCaptions = cpCore.Csv_cs_getRowFields(CS)
        //        ColumnCount = UBound(CSRowCaptions)
        //        For RowPointer = 0 To RowCount - 1
        //            sb.append( "<CR>")
        //            For ColumnPointer = 0 To ColumnCount - 1
        //                sb.append( "<CC Name=""" & CSRowCaptions(ColumnPointer) & """>")
        //                sb.append( CSRows(RowPointer, ColumnPointer))
        //                sb.append( "</CC>")
        //                Next
        //            sb.append( "</CR>" & vbCrLf)
        //            Next
        //        sb.append( "</CDef>" & vbCrLf)
        //        End If
        //    sb.append( "</ContensiveContent>" & vbCrLf)
        //    GetXMLContent = sb.tostring
        //    '
        //    Exit Function
        //    '
        //    ' ----- Error Trap
        //    '
        ////ErrorTrap:
        //    Call HandleClassErrorAndBubble(appname,"GetXMLContent")
        //End Function
        //////
        //////========================================================================
        ////// ----- Get an XML nodes attribute based on its name
        //////========================================================================
        //////
        //private string GetXMLAttribute(bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
        //    string tempGetXMLAttribute = null;
        //    try {
        //        //
        //        //todo  NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
        //        //				XmlAttribute NodeAttribute = null;
        //        XmlNode ResultNode = null;
        //        string UcaseName = null;
        //        //
        //        tempGetXMLAttribute = "";
        //        Found = false;
        //        //Set REsultNode = Node.Attributes.getNamedItem(Name)
        //        //If Not (REsultNode Is Nothing) Then
        //        //    GetXMLAttribute = REsultNode.Value
        //        //    Found = True
        //        //End If
        //        //If Not Found Then
        //        //    GetXMLAttribute = DefaultIfNotFound
        //        //End If
        //        //Exit Function
        //        //If Not (Node.Attributes Is Nothing) Then
        //        //    REsultNode = Node.Attributes.getNamedItem(Name)
        //        //    If (REsultNode Is Nothing) Then
        //        UcaseName = genericController.vbUCase(Name);
        //        foreach (XmlAttribute NodeAttribute in Node.Attributes) {
        //            if (genericController.vbUCase(NodeAttribute.Name) == UcaseName) {
        //                tempGetXMLAttribute = NodeAttribute.Value;
        //                Found = true;
        //                break;
        //            }
        //        }
        //        if (!Found) {
        //            tempGetXMLAttribute = DefaultIfNotFound;
        //        }
        //        //    Else
        //        //        GetXMLAttribute = REsultNode.Value
        //        //        Found = True
        //        //    End If
        //        //End If
        //        return tempGetXMLAttribute;
        //        //
        //        // ----- Error Trap
        //        //
        //    } catch( Exception ex ) {
        //        cpCore.handleException(ex);
        //    }
        //    return tempGetXMLAttribute;
        //}
        ////
        ////========================================================================
        ////
        ////========================================================================
        ////
        //private double GetXMLAttributeNumber(bool Found, XmlNode Node, string Name, double DefaultIfNotFound) {
        //    return EncodeNumber(GetXMLAttribute(Found, Node, Name, DefaultIfNotFound.ToString()));
        //}
        ////
        ////========================================================================
        ////
        ////========================================================================
        ////
        //private bool GetXMLAttributeBoolean(bool Found, XmlNode Node, string Name, bool DefaultIfNotFound) {
        //    return genericController.encodeBoolean(GetXMLAttribute(Found, Node, Name, encodeText(DefaultIfNotFound)));
        //}
        ////
        ////========================================================================
        ////
        ////========================================================================
        ////
        //private int GetXMLAttributeInteger(bool Found, XmlNode Node, string Name, int DefaultIfNotFound) {
        //    return genericController.EncodeInteger(GetXMLAttribute(Found, Node, Name, DefaultIfNotFound.ToString()));
        //}
        //
        //========================================================================
        // ----- Get an XML nodes attribute based on its name
        //========================================================================
        //
        //Private Function GetXMLAttribute(NodeName As XmlNode, Name As String) As String
        //    On Error GoTo ErrorTrap
        //    '
        //    Dim NodeAttribute As xmlattribute
        //    Dim MethodName As String
        //    '
        //    MethodName = "XMLClass.GetXMLAttribute"
        //    '
        //    For Each NodeAttribute In NodeName.Attributes
        //        If genericController.vbUCase(NodeAttribute.Name) = genericController.vbUCase(Name) Then
        //            GetXMLAttribute = NodeAttribute.nodeValue
        //            End If
        //        Next
        //    '
        //    Exit Function
        //    '
        //    ' ----- Error Trap
        //    '
        ////ErrorTrap:
        //    Call HandleClassErrorAndBubble(appname,"GetXMLAttribute")
        //End Function
        //
        //
        //
        //Private Function GetContentNameByID(cmc As appServicesClass, ContentID as integer) As String
        //    On Error GoTo ErrorTrap
        //    '
        //    dim dt as datatable
        //    Dim appName As String
        //    '
        //    appName = cpCore.appEnvironment.name
        //    GetContentNameByID = ""
        //    RS = cpCore.app.executeSql("Default", "Select Name from ccContent where ID=" & encodeSQLNumber(ContentID))
        //    If isDataTableOk(RS) Then
        //        GetContentNameByID = cpCore.getDataRowColumnName(RS.rows(0), "Name")
        //        End If
        //    Call closeDataTable(RS)
        //    If (isDataTableOk(rs)) Then
        //        If false Then
        //            RS.Close
        //        End If
        //        'RS = Nothing
        //    End If
        //    '
        //    Exit Function
        //    '
        //    ' ----- Error Trap
        //    '
        ////ErrorTrap:
        //    Call HandleClassErrorAndBubble(appName, "GetContentNameByID")
        //End Function
        //
        //========================================================================
        // ----- Save the admin menus to CDef AdminMenu tags
        //========================================================================
        //
        private string GetXMLContentDefinition_SQLIndexes() {
            string result = "";
            try {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                string appName = cpCore.serverConfig.appConfig.name;
                string SQL  = ""
                    + " select D.name as DataSourceName,T.name as TableName"
                    + " from cctables T left join ccDataSources d on D.ID=T.DataSourceID"
                    + " where t.active<>0";
                int CS = cpCore.db.csOpenSql(SQL);
                while (cpCore.db.csOk(CS)) {
                    string DataSourceName = cpCore.db.csGetText(CS, "DataSourceName");
                    string TableName = cpCore.db.csGetText(CS, "TableName");
                    string IndexList = cpCore.db.getSQLIndexList(DataSourceName, TableName);
                    //
                    if (!string.IsNullOrEmpty(IndexList)) {
                        string[] ListRows = genericController.stringSplit(IndexList, "\r\n");
                        string IndexName = "";
                        //todo  NOTE: The ending condition of VB 'For' loops is tested only on entry to the loop. Instant C# has created a temporary variable in order to use the initial value of UBound(ListRows) + 1 for every iteration:
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
                    cpCore.db.csGoNext(CS);
                }
                cpCore.db.csClose(ref CS);
                result = sb.ToString();
            } catch( Exception ex ) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        // ----- Save the admin menus to CDef AdminMenu tags
        //========================================================================
        //
        private string GetXMLContentDefinition_AdminMenus() {
            string s = "";
            try {
                string appName = cpCore.serverConfig.appConfig.name;
                s = s + GetXMLContentDefinition_AdminMenus_MenuEntries();
                s = s + GetXMLContentDefinition_AdminMenus_NavigatorEntries();
            } catch( Exception ex ) {
                cpCore.handleException(ex);
            }
            return s;
        }
        //
        //========================================================================
        // ----- Save the admin menus to CDef AdminMenu tags
        //========================================================================
        //
        private string GetXMLContentDefinition_AdminMenus_NavigatorEntries() {
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
                appName = cpCore.serverConfig.appConfig.name;
                MenuContentID = cpCore.db.getRecordID("Content", cnNavigatorEntries);
                dt = cpCore.db.executeQuery("select * from ccMenuEntries where (contentcontrolid=" + MenuContentID + ")and(name<>'')");
                if (dt.Rows.Count > 0) {
                    NavIconType = 0;
                    NavIconTitle = "";
                    foreach (DataRow rsDr in dt.Rows) {
                        RecordName = genericController.encodeText(rsDr["Name"]);
                        ParentID = genericController.encodeInteger(rsDr["ParentID"]);
                        menuNameSpace = getMenuNameSpace(ParentID, "");
                        sb.Append("<NavigatorEntry Name=\"" + encodeXMLattribute(RecordName) + "\"");
                        sb.Append(" NameSpace=\"" + menuNameSpace + "\"");
                        sb.Append(" LinkPage=\"" + encodeXmlAttributeFieldValue(appName, rsDr, "LinkPage") + "\"");
                        sb.Append(" ContentName=\"" + encodeXmlAttributeFieldLookup(appName, rsDr, "ContentID", "ccContent") + "\"");
                        sb.Append(" AdminOnly=\"" + encodeXmlAttributeFieldValue(appName, rsDr, "AdminOnly") + "\"");
                        sb.Append(" DeveloperOnly=\"" + encodeXmlAttributeFieldValue(appName, rsDr, "DeveloperOnly") + "\"");
                        sb.Append(" NewWindow=\"" + encodeXmlAttributeFieldValue(appName, rsDr, "NewWindow") + "\"");
                        sb.Append(" Active=\"" + encodeXmlAttributeFieldValue(appName, rsDr, "Active") + "\"");
                        sb.Append(" AddonName=\"" + encodeXmlAttributeFieldLookup(appName, rsDr, "AddonID", "ccAggregateFunctions") + "\"");
                        sb.Append(" SortOrder=\"" + encodeXmlAttributeFieldValue(appName, rsDr, "SortOrder") + "\"");
                        NavIconType = genericController.encodeInteger(encodeXmlAttributeFieldValue(appName, rsDr, "NavIconType"));
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
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        // ----- Save the admin menus to CDef AdminMenu tags
        //========================================================================
        //
        private string GetXMLContentDefinition_AdminMenus_MenuEntries() {
            string result = "";
            try {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                string appName = cpCore.serverConfig.appConfig.name;
                int MenuContentID = cpCore.db.getRecordID("Content", cnNavigatorEntries);
                DataTable rs = cpCore.db.executeQuery("select * from ccMenuEntries where (contentcontrolid=" + MenuContentID + ")and(name<>'')");
                if (isDataTableOk(rs)) {
                    if (true) {
                        foreach (DataRow dr in rs.Rows) {
                            string RecordName = genericController.encodeText(dr["Name"]);
                            sb.Append("<MenuEntry Name=\"" + encodeXMLattribute(RecordName) + "\"");
                            sb.Append(" ParentName=\"" + encodeXmlAttributeFieldLookup(appName, dr, "ParentID", "ccMenuEntries") + "\"");
                            sb.Append(" LinkPage=\"" + encodeXmlAttributeFieldValue(appName, dr, "LinkPage") + "\"");
                            sb.Append(" ContentName=\"" + encodeXmlAttributeFieldLookup(appName, dr, "ContentID", "ccContent") + "\"");
                            sb.Append(" AdminOnly=\"" + encodeXmlAttributeFieldValue(appName, dr, "AdminOnly") + "\"");
                            sb.Append(" DeveloperOnly=\"" + encodeXmlAttributeFieldValue(appName, dr, "DeveloperOnly") + "\"");
                            sb.Append(" NewWindow=\"" + encodeXmlAttributeFieldValue(appName, dr, "NewWindow") + "\"");
                            sb.Append(" Active=\"" + encodeXmlAttributeFieldValue(appName, dr, "Active") + "\"");
                            if (true) {
                                sb.Append(" AddonName=\"" + encodeXmlAttributeFieldLookup(appName, dr, "AddonID", "ccAggregateFunctions") + "\"");
                            }
                            sb.Append("/>\r\n");
                        }
                    }
                }
                result = sb.ToString();
            } catch( Exception ex ) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        //========================================================================
        //
        //private string GetXMLContentDefinition_AggregateFunctions() {
        //    System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //    try {
        //        //
        //        DataTable rs = null;
        //        string appName;
        //        //
        //        appName = cpCore.serverConfig.appConfig.name;
        //        rs = cpCore.db.executeQuery("select * from ccAggregateFunctions");
        //        if (isDataTableOk(rs)) {
        //            if (true) {
        //                foreach (DataRow rsdr in rs.Rows) {
        //                    sb.Append("<Addon Name=\"" + encodeXmlAttributeFieldValue(appName, rsdr, "Name") + "\"");
        //                    sb.Append(" Link=\"" + encodeXmlAttributeFieldValue(appName, rsdr, "Link") + "\"");
        //                    sb.Append(" ObjectProgramID=\"" + encodeXmlAttributeFieldValue(appName, rsdr, "ObjectProgramID") + "\"");
        //                    sb.Append(" ArgumentList=\"" + encodeXmlAttributeFieldValue(appName, rsdr, "ArgumentList") + "\"");
        //                    sb.Append(" SortOrder=\"" + encodeXmlAttributeFieldValue(appName, rsdr, "SortOrder") + "\"");
        //                    sb.Append(" >");
        //                    sb.Append(encodeXmlAttributeFieldValue(appName, rsdr, "Copy"));
        //                    sb.Append("</Addon>\r\n");
        //                }
        //            }
        //        }
        //    } catch( Exception ex ) {
        //        cpCore.handleException(ex);
        //    }
        //    return sb.ToString();
        //}
        //
        //
        //
        private string encodeXMLattribute(string Source) {
            string tempEncodeXMLattribute = null;
            tempEncodeXMLattribute = genericController.encodeHTML(Source);
            tempEncodeXMLattribute = genericController.vbReplace(tempEncodeXMLattribute, "\r\n", " ");
            tempEncodeXMLattribute = genericController.vbReplace(tempEncodeXMLattribute, "\r", "");
            return genericController.vbReplace(tempEncodeXMLattribute, "\n", "");
        }
        //
        //
        //
        private string GetTableRecordName(string TableName, int RecordID) {
            string tempGetTableRecordName = null;
            try {
                //
                DataTable dt = null;
                string appName;
                //
                appName = cpCore.serverConfig.appConfig.name;
                if (RecordID != 0 & !string.IsNullOrEmpty(TableName)) {
                    dt = cpCore.db.executeQuery("select Name from " + TableName + " where ID=" + RecordID);
                    if (dt.Rows.Count > 0) {
                        tempGetTableRecordName = dt.Rows[0][0].ToString();
                    }
                }
            } catch( Exception ex ) {
                cpCore.handleException(ex);
            }
            return tempGetTableRecordName;
        }
        //
        //
        //
        private string encodeXmlAttributeFieldValue(string appName, DataRow dr, string FieldName) {
            return encodeXMLattribute(genericController.encodeText(dr[FieldName]));
        }
        //
        //
        //
        private string encodeXmlAttributeFieldLookup(string appName, DataRow dr, string FieldName, string TableName) {
            return encodeXMLattribute(GetTableRecordName(TableName, genericController.encodeInteger(dr[FieldName])));
        }
        //
        //
        //
        private string getMenuNameSpace(int RecordID, string UsedIDString) {
            string tempgetMenuNameSpace = null;
            try {
                //
                DataTable rs = null;
                int ParentID = 0;
                string RecordName = "";
                string ParentSpace = "";
                string appName;
                //
                appName = cpCore.serverConfig.appConfig.name;
                if (RecordID != 0) {
                    if (genericController.vbInstr(1, "," + UsedIDString + ",", "," + RecordID + ",", 1) != 0) {
                        logController.appendLog(cpCore, "getMenuNameSpace, Circular reference found in UsedIDString [" + UsedIDString + "] getting ccMenuEntries namespace for recordid [" + RecordID + "]");
                        tempgetMenuNameSpace = "";
                    } else {
                        UsedIDString = UsedIDString + "," + RecordID;
                        ParentID = 0;
                        if (RecordID != 0) {
                            rs = cpCore.db.executeQuery("select Name,ParentID from ccMenuEntries where ID=" + RecordID);
                            if (isDataTableOk(rs)) {
                                ParentID = genericController.encodeInteger(rs.Rows[0]["ParentID"]);
                                RecordName = genericController.encodeText(rs.Rows[0]["Name"]);
                            }
                            if (isDataTableOk(rs)) {
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
                                logController.appendLog(cpCore, "getMenuNameSpace, Circular reference found (ParentID=RecordID) getting ccMenuEntries namespace for recordid [" + RecordID + "]");
                                tempgetMenuNameSpace = "";
                            } else {
                                if (ParentID != 0) {
                                    //
                                    // get next parent
                                    //
                                    ParentSpace = getMenuNameSpace(ParentID, UsedIDString);
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
                cpCore.handleException(ex);
            }
            return tempgetMenuNameSpace;
        }
        //
        //
        //
        private string CacheLookup(int RecordID, object[,] Cache) {
            string tempCacheLookup = null;
            //
            int Ptr = 0;
            //
            tempCacheLookup = "";
            if (RecordID != 0) {
                for (Ptr = 0; Ptr <= Cache.GetUpperBound(1); Ptr++) {
                    if (genericController.encodeInteger(Cache[0, Ptr]) == RecordID) {
                        tempCacheLookup = genericController.encodeText(Cache[1, Ptr]);
                        break;
                    }
                }
            }
            return tempCacheLookup;
        }
        //
        //
        //
        private string xmlValueText(object Source) {
            return encodeXMLattribute(genericController.encodeText(Source));
        }
        //
        //
        //
        private string xmlValueBoolean(object Source) {
            return encodeText(genericController.encodeBoolean(genericController.encodeText(Source)));
        }
    }
}