
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
    public class xmlController {
        // todo - remove copyrights
        //========================================================================
        // This page and its contents are copyright by Kidwell McGowan Associates.
        //========================================================================
        //
        // ----- global scope variables
        //
        private bool iAbort;
        private int iBusy;
        private int iTaskCount;
        private const string ApplicationNameLocal = "unknown";
        private coreClass cpCore;
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public xmlController(coreClass cpCore) {
            this.cpCore = cpCore;
        }
        //
        //========================================================================
        // ----- Save all content to an XML Stream
        //   4/28/08 - changed so content is read from Db using RS/Conn, not cache version
        //========================================================================
        //
        public string GetXMLContentDefinition2(string ContentName = "") {
            return GetXMLContentDefinition3(ContentName, false);
        }
        //
        //========================================================================
        // ----- Save all content to an XML Stream
        //   4/28/08 - changed so content is read from Db using RS/Conn, not cache version
        //   2/20/2010 - changed to include includebasefield
        //========================================================================
        //
        public string GetXMLContentDefinition3(string ContentName = "", bool IncludeBaseFields = false) {
            string tempGetXMLContentDefinition3 = null;
            try {
                //
                const string ContentSelectList = ""
                    + " id,name,active,adminonly,allowadd"
                    + ",allowcalendarevents,allowcontentchildtool,allowcontenttracking,allowdelete,0 as allowmetacontent"
                    + ",allowtopicrules,AllowWorkflowAuthoring,AuthoringTableID"
                    + ",ContentTableID,DefaultSortMethodID,DeveloperOnly,DropDownFieldList"
                    + ",EditorGroupID,ParentID,ccGuid,IsBaseContent"
                    + ",IconLink,IconHeight,IconWidth,IconSprites";

                const string FieldSelectList = ""
                    + "f.ID,f.Name,f.contentid,f.Active,f.AdminOnly,f.Authorable,f.Caption,f.DeveloperOnly,f.EditSortPriority,f.Type,f.HTMLContent"
                    + ",f.IndexColumn,f.IndexSortDirection,f.IndexSortPriority,f.RedirectID,f.RedirectPath,f.Required"
                    + ",f.TextBuffered,f.UniqueName,f.DefaultValue,f.RSSTitleField,f.RSSDescriptionField,f.MemberSelectGroupID"
                    + ",f.EditTab,f.Scramble,f.LookupList,f.NotEditable,f.Password,f.readonly,f.ManyToManyRulePrimaryField"
                    + ",f.ManyToManyRuleSecondaryField,'' as HelpMessageDeprecated,f.ModifiedBy,f.IsBaseField,f.LookupContentID"
                    + ",f.RedirectContentID,f.ManyToManyContentID,f.ManyToManyRuleContentID"
                    + ",h.helpdefault,h.helpcustom,f.IndexWidth";

                const int f_ID = 0;
                const int f_Name = 1;
                const int f_contentid = 2;
                const int f_Active = 3;
                const int f_AdminOnly = 4;
                const int f_Authorable = 5;
                const int f_Caption = 6;
                const int f_DeveloperOnly = 7;
                const int f_EditSortPriority = 8;
                const int f_Type = 9;
                const int f_HTMLContent = 10;
                const int f_IndexColumn = 11;
                const int f_IndexSortDirection = 12;
                const int f_IndexSortPriority = 13;
                const int f_RedirectID = 14;
                const int f_RedirectPath = 15;
                const int f_Required = 16;
                const int f_TextBuffered = 17;
                const int f_UniqueName = 18;
                const int f_DefaultValue = 19;
                const int f_RSSTitleField = 20;
                const int f_RSSDescriptionField = 21;
                const int f_MemberSelectGroupID = 22;
                const int f_EditTab = 23;
                const int f_Scramble = 24;
                const int f_LookupList = 25;
                const int f_NotEditable = 26;
                const int f_Password = 27;
                const int f_ReadOnly = 28;
                const int f_ManyToManyRulePrimaryField = 29;
                const int f_ManyToManyRuleSecondaryField = 30;
                const int f_HelpMessageDeprecated = 31;
                const int f_ModifiedBy = 32;
                const int f_IsBaseField = 33;
                const int f_LookupContentID = 34;
                const int f_RedirectContentID = 35;
                const int f_ManyToManyContentID = 36;
                const int f_ManyToManyRuleContentID = 37;
                const int f_helpdefault = 38;
                const int f_helpcustom = 39;
                const int f_IndexWidth = 40;
                //
                bool IsBaseContent = false;
                int FieldCnt = 0;
                string FieldName = null;
                int FieldContentID = 0;
                int LastFieldID = 0;
                int RecordID = 0;
                string RecordName = null;
                int AuthoringTableID = 0;
                string HelpDefault = null;
                string HelpCustom = null;
                int HelpCnt = 0;
                int fieldId = 0;
                string fieldType = null;
                int TableID = 0;
                int ContentTableID = 0;
                string TableName = null;
                int DataSourceID = 0;
                string DataSourceName = null;
                //Dim RSTable as datatable
                int DefaultSortMethodID = 0;
                string DefaultSortMethod = null;
                int EditorGroupID = 0;
                string EditorGroupName = null;
                int ParentID = 0;
                string ParentName = null;
                int CSContent = 0;
                int ContentID = 0;
                int CSDataSources = 0;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                Models.Complex.cdefModel CDef = null;
                int CDefPointer = 0;
                // converted array to dictionary - Dim FieldPointer As Integer
                string iContentName = null;
                int CDefPointerMin = 0;
                int CDefPointerMax = 0;
                Models.Complex.cdefModel[] CDefArray = null;
                int CDefArrayCount = 0;
                bool AllowContentChildTool = false;
                int CSField = 0;
                DataTable RS = null;
                DataTable RSF = null;
                DataTable RSH = null;
                string SQL = null;
                bool FoundMenuTable = false;
                //Dim FoundAFTable As Boolean
                int Ptr = 0;
                string[,] Tables = null;
                int TableCnt = 0;
                string[,] Sorts = null;
                int SortCnt = 0;
                string[,] Groups = null;
                int GroupCnt = 0;
                string[,] Contents = null;
                int ContentCnt = 0;
                //Dim ContentSrc as object
                //Dim ContentSrcCnt as integer
                //Dim ContentSrcPtr as integer
                string[,] CFields = null;
                int CFieldCnt = 0;
                int CFieldPtr = 0;
                string appName;
                //
                appName = cpCore.serverConfig.appConfig.name;
                iContentName = ContentName;
                if (!string.IsNullOrEmpty(iContentName)) {
                    SQL = "select id from cccontent where name=" + cpCore.db.encodeSQLText(iContentName);
                    RS = cpCore.db.executeQuery(SQL);
                    if (RS.Rows.Count > 0) {
                        ContentID = genericController.EncodeInteger(RS.Rows[0]["id"]);
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
                    RS = cpCore.db.executeQuery(SQL);
                    Tables = cpCore.db.convertDataTabletoArray(RS);
                    if (Tables == null) {
                        TableCnt = 0;
                    } else {
                        TableCnt = Tables.GetUpperBound(1) + 1;
                    }
                    //
                    // Build SortMethod lookup
                    //
                    SQL = "select ID,Name from ccSortMethods";
                    RS = cpCore.db.executeQuery(SQL);
                    Sorts = cpCore.db.convertDataTabletoArray(RS);
                    if (Sorts == null) {
                        SortCnt = 0;
                    } else {
                        SortCnt = Sorts.GetUpperBound(1) + 1;
                    }
                    //
                    // Build SortMethod lookup
                    //
                    SQL = "select ID,Name from ccGroups";
                    RS = cpCore.db.executeQuery(SQL);
                    Groups = cpCore.db.convertDataTabletoArray(RS);
                    if (Groups == null) {
                        GroupCnt = 0;
                    } else {
                        GroupCnt = Groups.GetUpperBound(1) + 1;
                    }
                    //
                    // Build Content lookup
                    //
                    SQL = "select id,name from ccContent";
                    RS = cpCore.db.executeQuery(SQL);
                    Contents = cpCore.db.convertDataTabletoArray(RS);
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
                    RS = cpCore.db.executeQuery(SQL);
                    CFields = cpCore.db.convertDataTabletoArray(RS);
                    CFieldCnt = CFields.GetUpperBound(1) + 1;
                    //
                    // select the content
                    //
                    if (ContentID != 0) {
                        SQL = "select " + ContentSelectList + " from ccContent where (id=" + ContentID + ")and(contenttableid is not null)and(contentcontrolid is not null) order by id";
                    } else {
                        SQL = "select " + ContentSelectList + " from ccContent where (name<>'')and(name is not null)and(contenttableid is not null)and(contentcontrolid is not null) order by id";
                    }
                    RS = cpCore.db.executeQuery(SQL);
                    //
                    // create output
                    //
                    CFieldPtr = 0;
                    foreach (DataRow dr in RS.Rows) {
                        //
                        // ----- <cdef>
                        //
                        IsBaseContent = genericController.encodeBoolean(dr["isBaseContent"]);
                        iContentName = GetRSXMLAttribute(appName, dr, "Name");
                        ContentID = genericController.EncodeInteger(dr["ID"]);
                        sb.Append("\r\n\t<CDef");
                        sb.Append(" Name=\"" + iContentName + "\"");
                        if ((!IsBaseContent) || IncludeBaseFields) {
                            sb.Append(" Active=\"" + GetRSXMLAttribute(appName, dr, "Active") + "\"");
                            sb.Append(" AdminOnly=\"" + GetRSXMLAttribute(appName, dr, "AdminOnly") + "\"");
                            //sb.Append( " AliasID=""" & GetRSXMLAttribute( appname,RS, "AliasID") & """")
                            //sb.Append( " AliasName=""" & GetRSXMLAttribute( appname,RS, "AliasName") & """")
                            sb.Append(" AllowAdd=\"" + GetRSXMLAttribute(appName, dr, "AllowAdd") + "\"");
                            sb.Append(" AllowCalendarEvents=\"" + GetRSXMLAttribute(appName, dr, "AllowCalendarEvents") + "\"");
                            sb.Append(" AllowContentChildTool=\"" + GetRSXMLAttribute(appName, dr, "AllowContentChildTool") + "\"");
                            sb.Append(" AllowContentTracking=\"" + GetRSXMLAttribute(appName, dr, "AllowContentTracking") + "\"");
                            sb.Append(" AllowDelete=\"" + GetRSXMLAttribute(appName, dr, "AllowDelete") + "\"");
                            //sb.Append(" AllowMetaContent=""" & GetRSXMLAttribute(appName, dr, "AllowMetaContent") & """")
                            sb.Append(" AllowTopicRules=\"" + GetRSXMLAttribute(appName, dr, "AllowTopicRules") + "\"");
                            sb.Append(" AllowWorkflowAuthoring=\"" + GetRSXMLAttribute(appName, dr, "AllowWorkflowAuthoring") + "\"");
                            //
                            AuthoringTableID = genericController.EncodeInteger(dr["AuthoringTableID"]);
                            TableName = "";
                            DataSourceName = "";
                            if (AuthoringTableID != 0) {
                                for (Ptr = 0; Ptr < TableCnt; Ptr++) {
                                    if (genericController.EncodeInteger(Tables[0, Ptr]) == AuthoringTableID) {
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
                            sb.Append(" AuthoringDataSourceName=\"" + EncodeXMLattribute(DataSourceName) + "\"");
                            sb.Append(" AuthoringTableName=\"" + EncodeXMLattribute(TableName) + "\"");
                            //
                            ContentTableID = genericController.EncodeInteger(dr["ContentTableID"]);
                            if (ContentTableID != AuthoringTableID) {
                                if (ContentTableID != 0) {
                                    TableName = "";
                                    DataSourceName = "";
                                    for (Ptr = 0; Ptr < TableCnt; Ptr++) {
                                        if (genericController.EncodeInteger(Tables[0, Ptr]) == ContentTableID) {
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
                            sb.Append(" ContentDataSourceName=\"" + EncodeXMLattribute(DataSourceName) + "\"");
                            sb.Append(" ContentTableName=\"" + EncodeXMLattribute(TableName) + "\"");
                            //
                            DefaultSortMethodID = genericController.EncodeInteger(dr["DefaultSortMethodID"]);
                            DefaultSortMethod = CacheLookup(DefaultSortMethodID, Sorts);
                            sb.Append(" DefaultSortMethod=\"" + EncodeXMLattribute(DefaultSortMethod) + "\"");
                            //
                            sb.Append(" DeveloperOnly=\"" + GetRSXMLAttribute(appName, dr, "DeveloperOnly") + "\"");
                            sb.Append(" DropDownFieldList=\"" + GetRSXMLAttribute(appName, dr, "DropDownFieldList") + "\"");
                            //
                            EditorGroupID = genericController.EncodeInteger(dr["EditorGroupID"]);
                            EditorGroupName = CacheLookup(EditorGroupID, Groups);
                            sb.Append(" EditorGroupName=\"" + EncodeXMLattribute(EditorGroupName) + "\"");
                            //
                            ParentID = genericController.EncodeInteger(dr["ParentID"]);
                            ParentName = CacheLookup(ParentID, Contents);
                            sb.Append(" Parent=\"" + EncodeXMLattribute(ParentName) + "\"");
                            //
                            sb.Append(" IconLink=\"" + GetRSXMLAttribute(appName, dr, "IconLink") + "\"");
                            sb.Append(" IconHeight=\"" + GetRSXMLAttribute(appName, dr, "IconHeight") + "\"");
                            sb.Append(" IconWidth=\"" + GetRSXMLAttribute(appName, dr, "IconWidth") + "\"");
                            sb.Append(" IconSprites=\"" + GetRSXMLAttribute(appName, dr, "IconSprites") + "\"");
                            sb.Append(" isbasecontent=\"" + GetRSXMLAttribute(appName, dr, "IsBaseContent") + "\"");
                        }
                        sb.Append(" guid=\"" + GetRSXMLAttribute(appName, dr, "ccGuid") + "\"");
                        sb.Append(" >");
                        //
                        // ----- <field>
                        //
                        FieldCnt = 0;
                        fieldId = 0;
                        while (CFieldPtr < CFieldCnt) {
                            LastFieldID = fieldId;
                            fieldId = genericController.EncodeInteger(CFields[f_ID, CFieldPtr]);
                            FieldName = genericController.encodeText(CFields[f_Name, CFieldPtr]);
                            FieldContentID = genericController.EncodeInteger(CFields[f_contentid, CFieldPtr]);
                            if (FieldContentID > ContentID) {
                                break;
                            } else if ((FieldContentID == ContentID) && (fieldId != LastFieldID)) {
                                if (IncludeBaseFields || (",id,dateadded,createdby,modifiedby,ContentControlID,CreateKey,ModifiedDate,ccguid,".IndexOf("," + FieldName + ",", System.StringComparison.OrdinalIgnoreCase)  == -1)) {
                                    sb.Append("\r\n\t\t<Field");
                                    fieldType = cpCore.db.getFieldTypeNameFromFieldTypeId(EncodeInteger(CFields[f_Type, CFieldPtr]));
                                    sb.Append(" Name=\"" + xaT(FieldName) + "\"");
                                    sb.Append(" active=\"" + xaB(CFields[f_Active, CFieldPtr]) + "\"");
                                    sb.Append(" AdminOnly=\"" + xaB(CFields[f_AdminOnly, CFieldPtr]) + "\"");
                                    sb.Append(" Authorable=\"" + xaB(CFields[f_Authorable, CFieldPtr]) + "\"");
                                    sb.Append(" Caption=\"" + xaT(CFields[f_Caption, CFieldPtr]) + "\"");
                                    sb.Append(" DeveloperOnly=\"" + xaB(CFields[f_DeveloperOnly, CFieldPtr]) + "\"");
                                    sb.Append(" EditSortPriority=\"" + xaT(CFields[f_EditSortPriority, CFieldPtr]) + "\"");
                                    sb.Append(" FieldType=\"" + fieldType + "\"");
                                    sb.Append(" HTMLContent=\"" + xaB(CFields[f_HTMLContent, CFieldPtr]) + "\"");
                                    sb.Append(" IndexColumn=\"" + xaT(CFields[f_IndexColumn, CFieldPtr]) + "\"");
                                    sb.Append(" IndexSortDirection=\"" + xaT(CFields[f_IndexSortDirection, CFieldPtr]) + "\"");
                                    sb.Append(" IndexSortOrder=\"" + xaT(CFields[f_IndexSortPriority, CFieldPtr]) + "\"");
                                    sb.Append(" IndexWidth=\"" + xaT(CFields[f_IndexWidth, CFieldPtr]) + "\"");
                                    sb.Append(" RedirectID=\"" + xaT(CFields[f_RedirectID, CFieldPtr]) + "\"");
                                    sb.Append(" RedirectPath=\"" + xaT(CFields[f_RedirectPath, CFieldPtr]) + "\"");
                                    sb.Append(" Required=\"" + xaB(CFields[f_Required, CFieldPtr]) + "\"");
                                    sb.Append(" TextBuffered=\"" + xaB(CFields[f_TextBuffered, CFieldPtr]) + "\"");
                                    sb.Append(" UniqueName=\"" + xaB(CFields[f_UniqueName, CFieldPtr]) + "\"");
                                    sb.Append(" DefaultValue=\"" + xaT(CFields[f_DefaultValue, CFieldPtr]) + "\"");
                                    sb.Append(" RSSTitle=\"" + xaB(CFields[f_RSSTitleField, CFieldPtr]) + "\"");
                                    sb.Append(" RSSDescription=\"" + xaB(CFields[f_RSSDescriptionField, CFieldPtr]) + "\"");
                                    sb.Append(" MemberSelectGroupID=\"" + xaT(CFields[f_MemberSelectGroupID, CFieldPtr]) + "\"");
                                    sb.Append(" EditTab=\"" + xaT(CFields[f_EditTab, CFieldPtr]) + "\"");
                                    sb.Append(" Scramble=\"" + xaB(CFields[f_Scramble, CFieldPtr]) + "\"");
                                    sb.Append(" LookupList=\"" + xaT(CFields[f_LookupList, CFieldPtr]) + "\"");
                                    sb.Append(" NotEditable=\"" + xaB(CFields[f_NotEditable, CFieldPtr]) + "\"");
                                    sb.Append(" Password=\"" + xaB(CFields[f_Password, CFieldPtr]) + "\"");
                                    sb.Append(" ReadOnly=\"" + xaB(CFields[f_ReadOnly, CFieldPtr]) + "\"");
                                    sb.Append(" ManyToManyRulePrimaryField=\"" + xaT(CFields[f_ManyToManyRulePrimaryField, CFieldPtr]) + "\"");
                                    sb.Append(" ManyToManyRuleSecondaryField=\"" + xaT(CFields[f_ManyToManyRuleSecondaryField, CFieldPtr]) + "\"");
                                    sb.Append(" IsModified=\"" + (EncodeInteger(CFields[f_ModifiedBy, CFieldPtr]) != 0) + "\"");
                                    if (true) {
                                        sb.Append(" IsBaseField=\"" + xaB(CFields[f_IsBaseField, CFieldPtr]) + "\"");
                                    }
                                    //
                                    RecordID = genericController.EncodeInteger(CFields[f_LookupContentID, CFieldPtr]);
                                    RecordName = CacheLookup(RecordID, Contents);
                                    sb.Append(" LookupContent=\"" + genericController.encodeHTML(RecordName) + "\"");
                                    //
                                    RecordID = genericController.EncodeInteger(CFields[f_RedirectContentID, CFieldPtr]);
                                    RecordName = CacheLookup(RecordID, Contents);
                                    sb.Append(" RedirectContent=\"" + genericController.encodeHTML(RecordName) + "\"");
                                    //
                                    RecordID = genericController.EncodeInteger(CFields[f_ManyToManyContentID, CFieldPtr]);
                                    RecordName = CacheLookup(RecordID, Contents);
                                    sb.Append(" ManyToManyContent=\"" + genericController.encodeHTML(RecordName) + "\"");
                                    //
                                    RecordID = genericController.EncodeInteger(CFields[f_ManyToManyRuleContentID, CFieldPtr]);
                                    RecordName = CacheLookup(RecordID, Contents);
                                    sb.Append(" ManyToManyRuleContent=\"" + genericController.encodeHTML(RecordName) + "\"");
                                    sb.Append(" >");
                                    //
                                    HelpCnt = 0;
                                    HelpDefault = xaT(CFields[f_helpcustom, CFieldPtr]);
                                    if (string.IsNullOrEmpty(HelpDefault)) {
                                        HelpDefault = xaT(CFields[f_helpdefault, CFieldPtr]);
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
        //
        //========================================================================
        // ----- Save all content to an XML Stream
        //   4/28/08 - changed so content is read from Db using RS/Conn, not cache version
        //========================================================================
        //
        public string GetXMLContentDefinition(string ContentName = "") {
            return GetXMLContentDefinition3(ContentName, false);
        }
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
        //
        //========================================================================
        // ----- Get an XML nodes attribute based on its name
        //========================================================================
        //
        private string GetXMLAttribute(bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
            string tempGetXMLAttribute = null;
            try {
                //
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlAttribute NodeAttribute = null;
                XmlNode ResultNode = null;
                string UcaseName = null;
                //
                tempGetXMLAttribute = "";
                Found = false;
                //Set REsultNode = Node.Attributes.getNamedItem(Name)
                //If Not (REsultNode Is Nothing) Then
                //    GetXMLAttribute = REsultNode.Value
                //    Found = True
                //End If
                //If Not Found Then
                //    GetXMLAttribute = DefaultIfNotFound
                //End If
                //Exit Function
                //If Not (Node.Attributes Is Nothing) Then
                //    REsultNode = Node.Attributes.getNamedItem(Name)
                //    If (REsultNode Is Nothing) Then
                UcaseName = genericController.vbUCase(Name);
                foreach (XmlAttribute NodeAttribute in Node.Attributes) {
                    if (genericController.vbUCase(NodeAttribute.Name) == UcaseName) {
                        tempGetXMLAttribute = NodeAttribute.Value;
                        Found = true;
                        break;
                    }
                }
                if (!Found) {
                    tempGetXMLAttribute = DefaultIfNotFound;
                }
                //    Else
                //        GetXMLAttribute = REsultNode.Value
                //        Found = True
                //    End If
                //End If
                return tempGetXMLAttribute;
                //
                // ----- Error Trap
                //
            } catch( Exception ex ) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            HandleClassErrorAndBubble("unknown", "GetXMLAttribute");
            //INSTANT C# TODO TASK: The '//Resume Next' statement is not converted by Instant C#:
            //Resume Next
            return tempGetXMLAttribute;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private double GetXMLAttributeNumber(bool Found, XmlNode Node, string Name, double DefaultIfNotFound) {
            return EncodeNumber(GetXMLAttribute(Found, Node, Name, DefaultIfNotFound.ToString()));
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private bool GetXMLAttributeBoolean(bool Found, XmlNode Node, string Name, bool DefaultIfNotFound) {
            return genericController.encodeBoolean(GetXMLAttribute(Found, Node, Name, encodeText(DefaultIfNotFound)));
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private int GetXMLAttributeInteger(bool Found, XmlNode Node, string Name, int DefaultIfNotFound) {
            return genericController.EncodeInteger(GetXMLAttribute(Found, Node, Name, DefaultIfNotFound.ToString()));
        }
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
            string result = string.Empty;
            try {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                string appName = cpCore.serverConfig.appConfig.name;
                string SQL  = ""
                    + " select D.name as DataSourceName,T.name as TableName"
                    + " from cctables T left join ccDataSources d on D.ID=T.DataSourceID"
                    + " where t.active<>0";
                int CS = cpCore.db.csOpenSql_rev("default", SQL);
                while (cpCore.db.csOk(CS)) {
                    string DataSourceName = cpCore.db.csGetText(CS, "DataSourceName");
                    string TableName = cpCore.db.csGetText(CS, "TableName");
                    string IndexList = cpCore.db.getSQLIndexList(DataSourceName, TableName);
                    //
                    if (!string.IsNullOrEmpty(IndexList)) {
                        string[] ListRows = genericController.stringSplit(IndexList, "\r\n");
                        string IndexName = "";
                        //INSTANT C# NOTE: The ending condition of VB 'For' loops is tested only on entry to the loop. Instant C# has created a temporary variable in order to use the initial value of UBound(ListRows) + 1 for every iteration:
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
                                            sb.Append(" Indexname=\"" + EncodeXMLattribute(IndexName) + "\"");
                                            sb.Append(" DataSourceName=\"" + EncodeXMLattribute(DataSourceName) + "\"");
                                            sb.Append(" TableName=\"" + EncodeXMLattribute(TableName) + "\"");
                                            sb.Append(" FieldNameList=\"" + EncodeXMLattribute(IndexFields) + "\"");
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
            string result = string.Empty;
            try {
                int NavIconType = 0;
                string NavIconTitle = null;
                int CSPointer = 0;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                DataTable dt = null;
                int ContentID = 0;
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
                        ParentID = genericController.EncodeInteger(rsDr["ParentID"]);
                        menuNameSpace = getMenuNameSpace(ParentID, "");
                        sb.Append("<NavigatorEntry Name=\"" + EncodeXMLattribute(RecordName) + "\"");
                        sb.Append(" NameSpace=\"" + menuNameSpace + "\"");
                        sb.Append(" LinkPage=\"" + GetRSXMLAttribute(appName, rsDr, "LinkPage") + "\"");
                        sb.Append(" ContentName=\"" + GetRSXMLLookupAttribute(appName, rsDr, "ContentID", "ccContent") + "\"");
                        sb.Append(" AdminOnly=\"" + GetRSXMLAttribute(appName, rsDr, "AdminOnly") + "\"");
                        sb.Append(" DeveloperOnly=\"" + GetRSXMLAttribute(appName, rsDr, "DeveloperOnly") + "\"");
                        sb.Append(" NewWindow=\"" + GetRSXMLAttribute(appName, rsDr, "NewWindow") + "\"");
                        sb.Append(" Active=\"" + GetRSXMLAttribute(appName, rsDr, "Active") + "\"");
                        sb.Append(" AddonName=\"" + GetRSXMLLookupAttribute(appName, rsDr, "AddonID", "ccAggregateFunctions") + "\"");
                        sb.Append(" SortOrder=\"" + GetRSXMLAttribute(appName, rsDr, "SortOrder") + "\"");
                        NavIconType = genericController.EncodeInteger(GetRSXMLAttribute(appName, rsDr, "NavIconType"));
                        NavIconTitle = GetRSXMLAttribute(appName, rsDr, "NavIconTitle");
                        sb.Append(" NavIconTitle=\"" + NavIconTitle + "\"");
                        SplitArray = (NavIconTypeList + ",help").Split(',');
                        SplitIndex = NavIconType - 1;
                        if ((SplitIndex >= 0) && (SplitIndex <= SplitArray.GetUpperBound(0))) {
                            sb.Append(" NavIconType=\"" + SplitArray[SplitIndex] + "\"");
                        }
                        sb.Append(" guid=\"" + GetRSXMLAttribute(appName, rsDr, "ccGuid") + "\"");
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
            string result = string.Empty;
            try {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                string appName = cpCore.serverConfig.appConfig.name;
                int MenuContentID = cpCore.db.getRecordID("Content", cnNavigatorEntries);
                DataTable rs = cpCore.db.executeQuery("select * from ccMenuEntries where (contentcontrolid=" + MenuContentID + ")and(name<>'')");
                if (isDataTableOk(rs)) {
                    if (true) {
                        foreach (DataRow dr in rs.Rows) {
                            string RecordName = genericController.encodeText(dr["Name"]);
                            sb.Append("<MenuEntry Name=\"" + EncodeXMLattribute(RecordName) + "\"");
                            sb.Append(" ParentName=\"" + GetRSXMLLookupAttribute(appName, dr, "ParentID", "ccMenuEntries") + "\"");
                            sb.Append(" LinkPage=\"" + GetRSXMLAttribute(appName, dr, "LinkPage") + "\"");
                            sb.Append(" ContentName=\"" + GetRSXMLLookupAttribute(appName, dr, "ContentID", "ccContent") + "\"");
                            sb.Append(" AdminOnly=\"" + GetRSXMLAttribute(appName, dr, "AdminOnly") + "\"");
                            sb.Append(" DeveloperOnly=\"" + GetRSXMLAttribute(appName, dr, "DeveloperOnly") + "\"");
                            sb.Append(" NewWindow=\"" + GetRSXMLAttribute(appName, dr, "NewWindow") + "\"");
                            sb.Append(" Active=\"" + GetRSXMLAttribute(appName, dr, "Active") + "\"");
                            if (true) {
                                sb.Append(" AddonName=\"" + GetRSXMLLookupAttribute(appName, dr, "AddonID", "ccAggregateFunctions") + "\"");
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
        //========================================================================
        //
        //========================================================================
        //
        private string GetXMLContentDefinition_AggregateFunctions() {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try {
                //
                DataTable rs = null;
                string appName;
                //
                appName = cpCore.serverConfig.appConfig.name;
                rs = cpCore.db.executeQuery("select * from ccAggregateFunctions");
                if (isDataTableOk(rs)) {
                    if (true) {
                        foreach (DataRow rsdr in rs.Rows) {
                            sb.Append("<Addon Name=\"" + GetRSXMLAttribute(appName, rsdr, "Name") + "\"");
                            sb.Append(" Link=\"" + GetRSXMLAttribute(appName, rsdr, "Link") + "\"");
                            sb.Append(" ObjectProgramID=\"" + GetRSXMLAttribute(appName, rsdr, "ObjectProgramID") + "\"");
                            sb.Append(" ArgumentList=\"" + GetRSXMLAttribute(appName, rsdr, "ArgumentList") + "\"");
                            sb.Append(" SortOrder=\"" + GetRSXMLAttribute(appName, rsdr, "SortOrder") + "\"");
                            sb.Append(" >");
                            sb.Append(GetRSXMLAttribute(appName, rsdr, "Copy"));
                            sb.Append("</Addon>\r\n");
                        }
                    }
                }
            } catch( Exception ex ) {
                cpCore.handleException(ex);
            }
            return sb.ToString();
        }
        //
        //
        //
        private string EncodeXMLattribute(string Source) {
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
        private string GetRSXMLAttribute(string appName, DataRow dr, string FieldName) {
            return EncodeXMLattribute(genericController.encodeText(dr[FieldName]));
        }
        //
        //
        //
        private string GetRSXMLLookupAttribute(string appName, DataRow dr, string FieldName, string TableName) {
            return EncodeXMLattribute(GetTableRecordName(TableName, genericController.EncodeInteger(dr[FieldName])));
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
                        HandleClassErrorAndResume(appName, "getMenuNameSpace", "Circular reference found in UsedIDString [" + UsedIDString + "] getting ccMenuEntries namespace for recordid [" + RecordID + "]");
                        tempgetMenuNameSpace = "";
                    } else {
                        UsedIDString = UsedIDString + "," + RecordID;
                        ParentID = 0;
                        if (RecordID != 0) {
                            rs = cpCore.db.executeQuery("select Name,ParentID from ccMenuEntries where ID=" + RecordID);
                            if (isDataTableOk(rs)) {
                                ParentID = genericController.EncodeInteger(rs.Rows[0]["ParentID"]);
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
                                HandleClassErrorAndResume(appName, "getMenuNameSpace", "Circular reference found (ParentID=RecordID) getting ccMenuEntries namespace for recordid [" + RecordID + "]");
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
                    if (genericController.EncodeInteger(Cache[0, Ptr]) == RecordID) {
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
        private string xaT(object Source) {
            return EncodeXMLattribute(genericController.encodeText(Source));
        }
        //
        //
        //
        private string xaB(object Source) {
            return encodeText(genericController.encodeBoolean(genericController.encodeText(Source)));
        }
        //
        //===========================================================================
        //   Error handler
        //===========================================================================
        //
        private void HandleClassErrorAndBubble(string appName, string MethodName, string Cause = "unknown") {
            //
            throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(appName, Cause, "dll", "XMLToolsClass", MethodName, Err.Number, Err.Source, Err.Description, False, False, "")
                                                                      //
        }
        //
        //===========================================================================
        //   Error handler
        //===========================================================================
        //
        private void HandleClassErrorAndResume(string appName, string MethodName, string Cause) {
            //
            throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(appName, Cause, "dll", "XMLToolsClass", MethodName, Err.Number, Err.Source, Err.Description, False, True, "")
                                                                      //
        }



    }
}