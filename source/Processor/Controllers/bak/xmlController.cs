

using Models.Entity;
using Controllers;

using System.Xml;

namespace Controllers {
    
    public class xmlController {
        
        // 
        // ========================================================================
        //  This page and its contents are copyright by Kidwell McGowan Associates.
        // ========================================================================
        // 
        //  ----- global scope variables
        // 
        private bool iAbort;
        
        private int iBusy;
        
        private int iTaskCount;
        
        private coreClass cpCore;
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' constructor
        // '' </summary>
        // '' <param name="cp"></param>
        // '' <remarks></remarks>
        public xmlController(coreClass cpCore) {
            this.cpCore = cpCore;
        }
        
        // 
        // ========================================================================
        //  ----- Save all content to an XML Stream
        //    4/28/08 - changed so content is read from Db using RS/Conn, not cache version
        // ========================================================================
        // 
        public string GetXMLContentDefinition2(string ContentName, void =, void ) {
            GetXMLContentDefinition2 = this.GetXMLContentDefinition3(ContentName, false);
            // Warning!!! Optional parameters not supported
        }
        
        // 
        // ========================================================================
        //  ----- Save all content to an XML Stream
        //    4/28/08 - changed so content is read from Db using RS/Conn, not cache version
        //    2/20/2010 - changed to include includebasefield
        // ========================================================================
        // 
        public string GetXMLContentDefinition3(string ContentName, void =, void , bool IncludeBaseFields, void =, void False) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // 
            const object ContentSelectList = ("" + (" id,name,active,adminonly,allowadd" + (",allowcalendarevents,allowcontentchildtool,allowcontenttracking,allowdelete,0 as allowmetacontent" + (",allowtopicrules,AllowWorkflowAuthoring,AuthoringTableID" + (",ContentTableID,DefaultSortMethodID,DeveloperOnly,DropDownFieldList" + (",EditorGroupID,ParentID,ccGuid,IsBaseContent" + ",IconLink,IconHeight,IconWidth,IconSprites"))))));
            const object FieldSelectList = ("" + ("f.ID,f.Name,f.contentid,f.Active,f.AdminOnly,f.Authorable,f.Caption,f.DeveloperOnly,f.EditSortPriorit" +
            "y,f.Type,f.HTMLContent" + (",f.IndexColumn,f.IndexSortDirection,f.IndexSortPriority,f.RedirectID,f.RedirectPath,f.Required" + (",f.TextBuffered,f.UniqueName,f.DefaultValue,f.RSSTitleField,f.RSSDescriptionField,f.MemberSelectGroup" +
            "ID" + (",f.EditTab,f.Scramble,f.LookupList,f.NotEditable,f.Password,f.readonly,f.ManyToManyRulePrimaryField" + (",f.ManyToManyRuleSecondaryField,\'\' as HelpMessageDeprecated,f.ModifiedBy,f.IsBaseField,f.LookupConten" +
            "tID" + (",f.RedirectContentID,f.ManyToManyContentID,f.ManyToManyRuleContentID" + ",h.helpdefault,h.helpcustom,f.IndexWidth")))))));
            const object f_ID = 0;
            const object f_Name = 1;
            const object f_contentid = 2;
            const object f_Active = 3;
            const object f_AdminOnly = 4;
            const object f_Authorable = 5;
            const object f_Caption = 6;
            const object f_DeveloperOnly = 7;
            const object f_EditSortPriority = 8;
            const object f_Type = 9;
            const object f_HTMLContent = 10;
            const object f_IndexColumn = 11;
            const object f_IndexSortDirection = 12;
            const object f_IndexSortPriority = 13;
            const object f_RedirectID = 14;
            const object f_RedirectPath = 15;
            const object f_Required = 16;
            const object f_TextBuffered = 17;
            const object f_UniqueName = 18;
            const object f_DefaultValue = 19;
            const object f_RSSTitleField = 20;
            const object f_RSSDescriptionField = 21;
            const object f_MemberSelectGroupID = 22;
            const object f_EditTab = 23;
            const object f_Scramble = 24;
            const object f_LookupList = 25;
            const object f_NotEditable = 26;
            const object f_Password = 27;
            const object f_ReadOnly = 28;
            const object f_ManyToManyRulePrimaryField = 29;
            const object f_ManyToManyRuleSecondaryField = 30;
            const object f_HelpMessageDeprecated = 31;
            const object f_ModifiedBy = 32;
            const object f_IsBaseField = 33;
            const object f_LookupContentID = 34;
            const object f_RedirectContentID = 35;
            const object f_ManyToManyContentID = 36;
            const object f_ManyToManyRuleContentID = 37;
            const object f_helpdefault = 38;
            const object f_helpcustom = 39;
            const object f_IndexWidth = 40;
            // 
            bool IsBaseContent;
            int FieldCnt;
            string FieldName;
            int FieldContentID;
            int LastFieldID;
            int RecordID;
            string RecordName;
            int AuthoringTableID;
            string HelpDefault;
            string HelpCustom;
            int HelpCnt;
            int fieldId;
            string fieldType;
            int TableID;
            int ContentTableID;
            string TableName;
            int DataSourceID;
            string DataSourceName;
            // Dim RSTable as datatable
            int DefaultSortMethodID;
            string DefaultSortMethod;
            int EditorGroupID;
            string EditorGroupName;
            int ParentID;
            string ParentName;
            int CSContent;
            int ContentID;
            int CSDataSources;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            Models.Complex.cdefModel CDef;
            int CDefPointer;
            //  converted array to dictionary - Dim FieldPointer As Integer
            string iContentName;
            int CDefPointerMin;
            int CDefPointerMax;
            Models.Complex.cdefModel[] CDefArray;
            int CDefArrayCount;
            bool AllowContentChildTool;
            int CSField;
            DataTable RS;
            DataTable RSF;
            DataTable RSH;
            string SQL;
            bool FoundMenuTable;
            // Dim FoundAFTable As Boolean
            int Ptr;
            string[,] Tables;
            int TableCnt;
            string[,] Sorts;
            int SortCnt;
            string[,] Groups;
            int GroupCnt;
            string[,] Contents;
            int ContentCnt;
            // Dim ContentSrc as object
            // Dim ContentSrcCnt as integer
            // Dim ContentSrcPtr as integer
            string[,] CFields;
            int CFieldCnt;
            int CFieldPtr;
            string appName;
            // 
            appName = cpCore.serverConfig.appConfig.name;
            iContentName = ContentName;
            if ((iContentName != "")) {
                SQL = ("select id from cccontent where name=" + cpCore.db.encodeSQLText(iContentName));
                RS = cpCore.db.executeQuery(SQL);
                if ((RS.Rows.Count > 0)) {
                    ContentID = genericController.EncodeInteger(RS.Rows[0].Item["id"]);
                }
                
            }
            
            if (((iContentName != "") 
                        && (ContentID == 0))) {
                // 
                //  export requested for content name that does not exist - return blank
                // 
            }
            else {
                // 
                //  Build table lookup
                // 
                SQL = "select T.ID,T.Name as TableName,D.Name as DataSourceName from ccTables T Left Join ccDataSources D on" +
                " D.ID=T.DataSourceID";
                RS = cpCore.db.executeQuery(SQL);
                Tables = cpCore.db.convertDataTabletoArray(RS);
                if ((Tables == null)) {
                    TableCnt = 0;
                }
                else {
                    TableCnt = (UBound(Tables, 2) + 1);
                }
                
                // 
                //  Build SortMethod lookup
                // 
                SQL = "select ID,Name from ccSortMethods";
                RS = cpCore.db.executeQuery(SQL);
                Sorts = cpCore.db.convertDataTabletoArray(RS);
                if ((Sorts == null)) {
                    SortCnt = 0;
                }
                else {
                    SortCnt = (UBound(Sorts, 2) + 1);
                }
                
                // 
                //  Build SortMethod lookup
                // 
                SQL = "select ID,Name from ccGroups";
                RS = cpCore.db.executeQuery(SQL);
                Groups = cpCore.db.convertDataTabletoArray(RS);
                if ((Groups == null)) {
                    GroupCnt = 0;
                }
                else {
                    GroupCnt = (UBound(Groups, 2) + 1);
                }
                
                // 
                //  Build Content lookup
                // 
                SQL = "select id,name from ccContent";
                RS = cpCore.db.executeQuery(SQL);
                Contents = cpCore.db.convertDataTabletoArray(RS);
                if ((Contents == null)) {
                    ContentCnt = 0;
                }
                else {
                    ContentCnt = (UBound(Contents, 2) + 1);
                }
                
                // 
                //  select all the fields
                // 
                if ((ContentID != 0)) {
                    SQL = ("select " 
                                + (FieldSelectList + ("" + (" from ccfields f left join ccfieldhelp h on h.fieldid=f.id" + (" where (f.Type<>0)and(f.contentid=" 
                                + (ContentID + (")" + "")))))));
                }
                else {
                    SQL = ("select " 
                                + (FieldSelectList + ("" + (" from ccfields f left join ccfieldhelp h on h.fieldid=f.id" + (" where (f.Type<>0)" + "")))));
                }
                
                if (!IncludeBaseFields) {
                    " and ((f.IsBaseField is null)or(f.IsBaseField=0))";
                }
                
                " order by f.contentid,f.id,h.id desc";
                RS = cpCore.db.executeQuery(SQL);
                CFields = cpCore.db.convertDataTabletoArray(RS);
                CFieldCnt = (UBound(CFields, 2) + 1);
                // 
                //  select the content
                // 
                if ((ContentID != 0)) {
                    SQL = ("select " 
                                + (ContentSelectList + (" from ccContent where (id=" 
                                + (ContentID + ")and(contenttableid is not null)and(contentcontrolid is not null) order by id"))));
                }
                else {
                    SQL = ("select " 
                                + (ContentSelectList + " from ccContent where (name<>\'\')and(name is not null)and(contenttableid is not null)and(contentcontro" +
                                "lid is not null) order by id"));
                }
                
                RS = cpCore.db.executeQuery(SQL);
                // 
                //  create output
                // 
                CFieldPtr = 0;
                foreach (DataRow dr in RS.Rows) {
                    // 
                    //  ----- <cdef>
                    // 
                    IsBaseContent = genericController.EncodeBoolean(dr["isBaseContent"]);
                    iContentName = this.GetRSXMLAttribute(appName, dr, "Name");
                    if ((genericController.vbInstr(1, iContentName, "data sources", vbTextCompare) == 1)) {
                        iContentName = iContentName;
                    }
                    
                    ContentID = genericController.EncodeInteger(dr["ID"]);
                    sb.Append(("\r\n" + ('\t' + "<CDef")));
                    sb.Append((" Name=\"" 
                                    + (iContentName + "\"")));
                    if ((!IsBaseContent 
                                || IncludeBaseFields)) {
                        sb.Append((" Active=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "Active") + "\"")));
                        sb.Append((" AdminOnly=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "AdminOnly") + "\"")));
                        // sb.Append( " AliasID=""" & GetRSXMLAttribute( appname,RS, "AliasID") & """")
                        // sb.Append( " AliasName=""" & GetRSXMLAttribute( appname,RS, "AliasName") & """")
                        sb.Append((" AllowAdd=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "AllowAdd") + "\"")));
                        sb.Append((" AllowCalendarEvents=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "AllowCalendarEvents") + "\"")));
                        sb.Append((" AllowContentChildTool=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "AllowContentChildTool") + "\"")));
                        sb.Append((" AllowContentTracking=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "AllowContentTracking") + "\"")));
                        sb.Append((" AllowDelete=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "AllowDelete") + "\"")));
                        // sb.Append(" AllowMetaContent=""" & GetRSXMLAttribute(appName, dr, "AllowMetaContent") & """")
                        sb.Append((" AllowTopicRules=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "AllowTopicRules") + "\"")));
                        sb.Append((" AllowWorkflowAuthoring=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "AllowWorkflowAuthoring") + "\"")));
                        // 
                        AuthoringTableID = genericController.EncodeInteger(dr["AuthoringTableID"]);
                        TableName = "";
                        DataSourceName = "";
                        if ((AuthoringTableID != 0)) {
                            for (Ptr = 0; (Ptr 
                                        <= (TableCnt - 1)); Ptr++) {
                                if ((genericController.EncodeInteger(Tables[0, Ptr]) == AuthoringTableID)) {
                                    TableName = genericController.encodeText(Tables[1, Ptr]);
                                    DataSourceName = genericController.encodeText(Tables[2, Ptr]);
                                    break;
                                }
                                
                            }
                            
                        }
                        
                        if ((DataSourceName == "")) {
                            DataSourceName = "Default";
                        }
                        
                        if ((genericController.vbUCase(TableName) == "CCMENUENTRIES")) {
                            FoundMenuTable = true;
                        }
                        
                        sb.Append((" AuthoringDataSourceName=\"" 
                                        + (this.EncodeXMLattribute(DataSourceName) + "\"")));
                        sb.Append((" AuthoringTableName=\"" 
                                        + (this.EncodeXMLattribute(TableName) + "\"")));
                        // 
                        ContentTableID = genericController.EncodeInteger(dr["ContentTableID"]);
                        if ((ContentTableID != AuthoringTableID)) {
                            if ((ContentTableID != 0)) {
                                TableName = "";
                                DataSourceName = "";
                                for (Ptr = 0; (Ptr 
                                            <= (TableCnt - 1)); Ptr++) {
                                    if ((genericController.EncodeInteger(Tables[0, Ptr]) == ContentTableID)) {
                                        TableName = genericController.encodeText(Tables[1, Ptr]);
                                        DataSourceName = genericController.encodeText(Tables[2, Ptr]);
                                        break;
                                    }
                                    
                                }
                                
                                if ((DataSourceName == "")) {
                                    DataSourceName = "Default";
                                }
                                
                            }
                            
                        }
                        
                        sb.Append((" ContentDataSourceName=\"" 
                                        + (this.EncodeXMLattribute(DataSourceName) + "\"")));
                        sb.Append((" ContentTableName=\"" 
                                        + (this.EncodeXMLattribute(TableName) + "\"")));
                        // 
                        DefaultSortMethodID = genericController.EncodeInteger(dr["DefaultSortMethodID"]);
                        DefaultSortMethod = this.CacheLookup(DefaultSortMethodID, Sorts);
                        sb.Append((" DefaultSortMethod=\"" 
                                        + (this.EncodeXMLattribute(DefaultSortMethod) + "\"")));
                        // 
                        sb.Append((" DeveloperOnly=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "DeveloperOnly") + "\"")));
                        sb.Append((" DropDownFieldList=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "DropDownFieldList") + "\"")));
                        // 
                        EditorGroupID = genericController.EncodeInteger(dr["EditorGroupID"]);
                        EditorGroupName = this.CacheLookup(EditorGroupID, Groups);
                        sb.Append((" EditorGroupName=\"" 
                                        + (this.EncodeXMLattribute(EditorGroupName) + "\"")));
                        // 
                        ParentID = genericController.EncodeInteger(dr["ParentID"]);
                        ParentName = this.CacheLookup(ParentID, Contents);
                        sb.Append((" Parent=\"" 
                                        + (this.EncodeXMLattribute(ParentName) + "\"")));
                        // 
                        sb.Append((" IconLink=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "IconLink") + "\"")));
                        sb.Append((" IconHeight=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "IconHeight") + "\"")));
                        sb.Append((" IconWidth=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "IconWidth") + "\"")));
                        sb.Append((" IconSprites=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "IconSprites") + "\"")));
                        sb.Append((" isbasecontent=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "IsBaseContent") + "\"")));
                    }
                    
                    sb.Append((" guid=\"" 
                                    + (this.GetRSXMLAttribute(appName, dr, "ccGuid") + "\"")));
                    sb.Append(" >");
                    // 
                    //  ----- <field>
                    // 
                    FieldCnt = 0;
                    fieldId = 0;
                    while ((CFieldPtr < CFieldCnt)) {
                        LastFieldID = fieldId;
                        fieldId = genericController.EncodeInteger(CFields[f_ID, CFieldPtr]);
                        FieldName = genericController.encodeText(CFields[f_Name, CFieldPtr]);
                        FieldContentID = genericController.EncodeInteger(CFields[f_contentid, CFieldPtr]);
                        if ((FieldContentID > ContentID)) {
                            break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                        }
                        else if (((FieldContentID == ContentID) 
                                    && (fieldId != LastFieldID))) {
                            if ((IncludeBaseFields 
                                        || ((",id,dateadded,createdby,modifiedby,ContentControlID,CreateKey,ModifiedDate,ccguid,".IndexOf(("," 
                                            + (FieldName + ",")), 0, System.StringComparison.OrdinalIgnoreCase) + 1) 
                                        == 0))) {
                                sb.Append(("\r\n" + ('\t' + ('\t' + "<Field"))));
                                fieldType = cpCore.db.getFieldTypeNameFromFieldTypeId(EncodeInteger(CFields[f_Type, CFieldPtr]));
                                sb.Append((" Name=\"" 
                                                + (this.xaT(FieldName) + "\"")));
                                sb.Append((" active=\"" 
                                                + (this.xaB(CFields[f_Active, CFieldPtr]) + "\"")));
                                sb.Append((" AdminOnly=\"" 
                                                + (this.xaB(CFields[f_AdminOnly, CFieldPtr]) + "\"")));
                                sb.Append((" Authorable=\"" 
                                                + (this.xaB(CFields[f_Authorable, CFieldPtr]) + "\"")));
                                sb.Append((" Caption=\"" 
                                                + (this.xaT(CFields[f_Caption, CFieldPtr]) + "\"")));
                                sb.Append((" DeveloperOnly=\"" 
                                                + (this.xaB(CFields[f_DeveloperOnly, CFieldPtr]) + "\"")));
                                sb.Append((" EditSortPriority=\"" 
                                                + (this.xaT(CFields[f_EditSortPriority, CFieldPtr]) + "\"")));
                                sb.Append((" FieldType=\"" 
                                                + (fieldType + "\"")));
                                sb.Append((" HTMLContent=\"" 
                                                + (this.xaB(CFields[f_HTMLContent, CFieldPtr]) + "\"")));
                                sb.Append((" IndexColumn=\"" 
                                                + (this.xaT(CFields[f_IndexColumn, CFieldPtr]) + "\"")));
                                sb.Append((" IndexSortDirection=\"" 
                                                + (this.xaT(CFields[f_IndexSortDirection, CFieldPtr]) + "\"")));
                                sb.Append((" IndexSortOrder=\"" 
                                                + (this.xaT(CFields[f_IndexSortPriority, CFieldPtr]) + "\"")));
                                sb.Append((" IndexWidth=\"" 
                                                + (this.xaT(CFields[f_IndexWidth, CFieldPtr]) + "\"")));
                                sb.Append((" RedirectID=\"" 
                                                + (this.xaT(CFields[f_RedirectID, CFieldPtr]) + "\"")));
                                sb.Append((" RedirectPath=\"" 
                                                + (this.xaT(CFields[f_RedirectPath, CFieldPtr]) + "\"")));
                                sb.Append((" Required=\"" 
                                                + (this.xaB(CFields[f_Required, CFieldPtr]) + "\"")));
                                sb.Append((" TextBuffered=\"" 
                                                + (this.xaB(CFields[f_TextBuffered, CFieldPtr]) + "\"")));
                                sb.Append((" UniqueName=\"" 
                                                + (this.xaB(CFields[f_UniqueName, CFieldPtr]) + "\"")));
                                sb.Append((" DefaultValue=\"" 
                                                + (this.xaT(CFields[f_DefaultValue, CFieldPtr]) + "\"")));
                                sb.Append((" RSSTitle=\"" 
                                                + (this.xaB(CFields[f_RSSTitleField, CFieldPtr]) + "\"")));
                                sb.Append((" RSSDescription=\"" 
                                                + (this.xaB(CFields[f_RSSDescriptionField, CFieldPtr]) + "\"")));
                                sb.Append((" MemberSelectGroupID=\"" 
                                                + (this.xaT(CFields[f_MemberSelectGroupID, CFieldPtr]) + "\"")));
                                sb.Append((" EditTab=\"" 
                                                + (this.xaT(CFields[f_EditTab, CFieldPtr]) + "\"")));
                                sb.Append((" Scramble=\"" 
                                                + (this.xaB(CFields[f_Scramble, CFieldPtr]) + "\"")));
                                sb.Append((" LookupList=\"" 
                                                + (this.xaT(CFields[f_LookupList, CFieldPtr]) + "\"")));
                                sb.Append((" NotEditable=\"" 
                                                + (this.xaB(CFields[f_NotEditable, CFieldPtr]) + "\"")));
                                sb.Append((" Password=\"" 
                                                + (this.xaB(CFields[f_Password, CFieldPtr]) + "\"")));
                                sb.Append((" ReadOnly=\"" 
                                                + (this.xaB(CFields[f_ReadOnly, CFieldPtr]) + "\"")));
                                sb.Append((" ManyToManyRulePrimaryField=\"" 
                                                + (this.xaT(CFields[f_ManyToManyRulePrimaryField, CFieldPtr]) + "\"")));
                                sb.Append((" ManyToManyRuleSecondaryField=\"" 
                                                + (this.xaT(CFields[f_ManyToManyRuleSecondaryField, CFieldPtr]) + "\"")));
                                sb.Append((" IsModified=\"" 
                                                + ((EncodeInteger(CFields[f_ModifiedBy, CFieldPtr]) != 0) 
                                                + "\"")));
                                if (true) {
                                    sb.Append((" IsBaseField=\"" 
                                                    + (this.xaB(CFields[f_IsBaseField, CFieldPtr]) + "\"")));
                                }
                                
                                // 
                                RecordID = genericController.EncodeInteger(CFields[f_LookupContentID, CFieldPtr]);
                                RecordName = this.CacheLookup(RecordID, Contents);
                                sb.Append((" LookupContent=\"" 
                                                + (genericController.encodeHTML(RecordName) + "\"")));
                                // 
                                RecordID = genericController.EncodeInteger(CFields[f_RedirectContentID, CFieldPtr]);
                                RecordName = this.CacheLookup(RecordID, Contents);
                                sb.Append((" RedirectContent=\"" 
                                                + (genericController.encodeHTML(RecordName) + "\"")));
                                // 
                                RecordID = genericController.EncodeInteger(CFields[f_ManyToManyContentID, CFieldPtr]);
                                RecordName = this.CacheLookup(RecordID, Contents);
                                sb.Append((" ManyToManyContent=\"" 
                                                + (genericController.encodeHTML(RecordName) + "\"")));
                                // 
                                RecordID = genericController.EncodeInteger(CFields[f_ManyToManyRuleContentID, CFieldPtr]);
                                RecordName = this.CacheLookup(RecordID, Contents);
                                sb.Append((" ManyToManyRuleContent=\"" 
                                                + (genericController.encodeHTML(RecordName) + "\"")));
                                // 
                                sb.Append(" >");
                                // 
                                HelpCnt = 0;
                                //                     HelpDefault = xaT(CFields(f_helpdefault, CFieldPtr))
                                //                     If HelpDefault <> "" Then
                                //                         sb.Append( vbCrLf & vbTab & vbTab & vbTab & "<HelpDefault>" & HelpDefault & "</HelpDefault>")
                                //                         HelpCnt = HelpCnt + 1
                                //                     End If
                                HelpDefault = this.xaT(CFields[f_helpcustom, CFieldPtr]);
                                if ((HelpDefault == "")) {
                                    HelpDefault = this.xaT(CFields[f_helpdefault, CFieldPtr]);
                                }
                                
                                if ((HelpDefault != "")) {
                                    sb.Append(("\r\n" + ('\t' + ('\t' + ('\t' + ("<HelpDefault>" 
                                                    + (HelpDefault + "</HelpDefault>")))))));
                                    HelpCnt = (HelpCnt + 1);
                                }
                                
                                //                             HelpCustom = xaT(CFields(f_helpcustom, CFieldPtr))
                                //                             If HelpCustom <> "" Then
                                //                                 sb.Append( vbCrLf & vbTab & vbTab & vbTab & "<HelpCustom>" & HelpCustom & "</HelpCustom>")
                                //                                 HelpCnt = HelpCnt + 1
                                //                             End If
                                if ((HelpCnt > 0)) {
                                    sb.Append(("\r\n" + ('\t' + '\t')));
                                }
                                
                                sb.Append("</Field>");
                            }
                            
                            FieldCnt = (FieldCnt + 1);
                        }
                        
                        CFieldPtr = (CFieldPtr + 1);
                    }
                    
                    // 
                    if ((FieldCnt > 0)) {
                        sb.Append(("\r\n" + '\t'));
                    }
                    
                    sb.Append("</CDef>");
                }
                
                if ((ContentName == "")) {
                    // 
                    //  Add other areas of the CDef file
                    // 
                    sb.Append(this.GetXMLContentDefinition_SQLIndexes());
                    if (FoundMenuTable) {
                        sb.Append(this.GetXMLContentDefinition_AdminMenus());
                    }
                    
                    // 
                    //  These are not needed anymore - later add "ImportCollection" entries for all collections installed
                    // 
                    //         If FoundAFTable Then
                    //             sb.Append( GetXMLContentDefinition_AggregateFunctions()
                    //         End If
                }
                
                GetXMLContentDefinition3 = ("<" 
                            + (CollectionFileRootNode + (" name=\"Application\" guid=\"" 
                            + (ApplicationCollectionGuid + ("\">" 
                            + (sb.ToString + ("\r\n" + ("</" 
                            + (CollectionFileRootNode + ">")))))))));
            }
            
            // 
            // cpCore.AppendLog("getXmlContentDefinition, exit")
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            this.HandleClassErrorAndBubble(appName, "GetXMLContentDefinition3");
        }
        
        // 
        // ========================================================================
        //  ----- Save all content to an XML Stream
        //    4/28/08 - changed so content is read from Db using RS/Conn, not cache version
        // ========================================================================
        // 
        public string GetXMLContentDefinition(string ContentName, void =, void ) {
            // 
            // Warning!!! Optional parameters not supported
            string appName;
            // 
            appName = cpCore.serverConfig.appConfig.name;
            return this.GetXMLContentDefinition3(ContentName, false);
            
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            this.HandleClassErrorAndBubble(appName, "GetXMLContentDefinition");
        }
        
        // '
        // '========================================================================
        // ' ----- Save all content to an XML Stream
        // '========================================================================
        // '
        // Private Function GetXMLContent(cmc as appServicesClass, ContentName As String) As String
        //     On Error GoTo ErrorTrap
        //     '
        //     Dim CS as integer
        //     Dim sb as new system.text.stringBuilder
        //     Dim CDefPointer as integer
        //     Dim CDefArrayCount as integer
        //     Dim CSRows as object
        //     Dim CSRowCaptions as object
        //     Dim RowCount as integer
        //     Dim RowPointer as integer
        //     Dim ColumnCount as integer
        //     Dim ColumnPointer as integer
        //     '
        //     sb.append( "<ContensiveContent>" & vbCrLf)
        //     If ContentName <> "" Then
        //         Call sb.append("<CDef Name=""" & ContentName & """>" & vbCrLf)
        //         CS = cpCore.csOpen(ContentName)
        //         CSRows = cpCore.Csv_cs_getRows(CS)
        //         RowCount = UBound(CSRows, 2)
        //         CSRowCaptions = cpCore.Csv_cs_getRowFields(CS)
        //         ColumnCount = UBound(CSRowCaptions)
        //         For RowPointer = 0 To RowCount - 1
        //             sb.append( "<CR>")
        //             For ColumnPointer = 0 To ColumnCount - 1
        //                 sb.append( "<CC Name=""" & CSRowCaptions(ColumnPointer) & """>")
        //                 sb.append( CSRows(RowPointer, ColumnPointer))
        //                 sb.append( "</CC>")
        //                 Next
        //             sb.append( "</CR>" & vbCrLf)
        //             Next
        //         sb.append( "</CDef>" & vbCrLf)
        //         End If
        //     sb.append( "</ContensiveContent>" & vbCrLf)
        //     GetXMLContent = sb.tostring
        //     '
        //     Exit Function
        //     '
        //     ' ----- Error Trap
        //     '
        // ErrorTrap:
        //     Call HandleClassErrorAndBubble(appname,"GetXMLContent")
        // End Function
        // 
        // ========================================================================
        //  ----- Get an XML nodes attribute based on its name
        // ========================================================================
        // 
        private string GetXMLAttribute(bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            XmlAttribute NodeAttribute;
            XmlNode ResultNode;
            string UcaseName;
            // 
            GetXMLAttribute = "";
            Found = false;
            UcaseName = genericController.vbUCase(Name);
            foreach (NodeAttribute in Node.Attributes) {
                if ((genericController.vbUCase(NodeAttribute.Name) == UcaseName)) {
                    GetXMLAttribute = NodeAttribute.Value;
                    Found = true;
                    break;
                }
                
            }
            
            if (!Found) {
                GetXMLAttribute = DefaultIfNotFound;
            }
            
            //     Else
            //         GetXMLAttribute = REsultNode.Value
            //         Found = True
            //     End If
            // End If
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            this.HandleClassErrorAndBubble("unknown", "GetXMLAttribute");
        }
        
        // 
        // ========================================================================
        // 
        // ========================================================================
        // 
        private double GetXMLAttributeNumber(bool Found, XmlNode Node, string Name, double DefaultIfNotFound) {
            return EncodeNumber(this.GetXMLAttribute(Found, Node, Name, DefaultIfNotFound.ToString()));
        }
        
        // 
        // ========================================================================
        // 
        // ========================================================================
        // 
        private bool GetXMLAttributeBoolean(bool Found, XmlNode Node, string Name, bool DefaultIfNotFound) {
            return genericController.EncodeBoolean(this.GetXMLAttribute(Found, Node, Name, DefaultIfNotFound.ToString()));
        }
        
        // 
        // ========================================================================
        // 
        // ========================================================================
        // 
        private int GetXMLAttributeInteger(bool Found, XmlNode Node, string Name, int DefaultIfNotFound) {
            return genericController.EncodeInteger(this.GetXMLAttribute(Found, Node, Name, DefaultIfNotFound.ToString()));
        }
        
        // '
        // '========================================================================
        // ' ----- Get an XML nodes attribute based on its name
        // '========================================================================
        // '
        // Private Function GetXMLAttribute(NodeName As XmlNode, Name As String) As String
        //     On Error GoTo ErrorTrap
        //     '
        //     Dim NodeAttribute As xmlattribute
        //     Dim MethodName As String
        //     '
        //     MethodName = "XMLClass.GetXMLAttribute"
        //     '
        //     For Each NodeAttribute In NodeName.Attributes
        //         If genericController.vbUCase(NodeAttribute.Name) = genericController.vbUCase(Name) Then
        //             GetXMLAttribute = NodeAttribute.nodeValue
        //             End If
        //         Next
        //     '
        //     Exit Function
        //     '
        //     ' ----- Error Trap
        //     '
        // ErrorTrap:
        //     Call HandleClassErrorAndBubble(appname,"GetXMLAttribute")
        // End Function
        // '
        // '
        // '
        // Private Function GetContentNameByID(cmc As appServicesClass, ContentID as integer) As String
        //     On Error GoTo ErrorTrap
        //     '
        //     dim dt as datatable
        //     Dim appName As String
        //     '
        //     appName = cpCore.appEnvironment.name
        //     GetContentNameByID = ""
        //     RS = cpCore.app.executeSql("Default", "Select Name from ccContent where ID=" & encodeSQLNumber(ContentID))
        //     If isDataTableOk(RS) Then
        //         GetContentNameByID = cpCore.getDataRowColumnName(RS.rows(0), "Name")
        //         End If
        //     Call closeDataTable(RS)
        //     If (isDataTableOk(rs)) Then
        //         If false Then
        //             RS.Close
        //         End If
        //         'RS = Nothing
        //     End If
        //     '
        //     Exit Function
        //     '
        //     ' ----- Error Trap
        //     '
        // ErrorTrap:
        //     Call HandleClassErrorAndBubble(appName, "GetContentNameByID")
        // End Function
        // 
        // ========================================================================
        //  ----- Save the admin menus to CDef AdminMenu tags
        // ========================================================================
        // 
        private string GetXMLContentDefinition_SQLIndexes() {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            string DataSourceName;
            string TableName;
            // 
            string IndexFields = "";
            string IndexList;
            string IndexName;
            string[] ListRows;
            string ListRow;
            string[] ListRowSplit;
            string SQL;
            int CS;
            int Ptr;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string appName;
            // 
            appName = cpCore.serverConfig.appConfig.name;
            SQL = ("select D.name as DataSourceName,T.name as TableName" + (" from cctables T left join ccDataSources d on D.ID=T.DataSourceID" + " where t.active<>0"));
            CS = cpCore.db.csOpenSql_rev("default", SQL);
            while (cpCore.db.csOk(CS)) {
                DataSourceName = cpCore.db.csGetText(CS, "DataSourceName");
                TableName = cpCore.db.csGetText(CS, "TableName");
                IndexList = cpCore.db.getSQLIndexList(DataSourceName, TableName);
                // 
                //  name1,index1
                //  name1,index2
                //  name2,field3
                //  name3,field4
                // 
                // 
                if ((IndexList != "")) {
                    ListRows = IndexList.Split("\r\n");
                    IndexName = "";
                    for (Ptr = 0; (Ptr 
                                <= (UBound(ListRows) + 1)); Ptr++) {
                        if ((Ptr <= UBound(ListRows))) {
                            // 
                            //  ListRowSplit has the indexname and field for this index
                            // 
                            ListRowSplit = ListRows[Ptr].Split(",");
                        }
                        else {
                            // 
                            //  one past the last row, ListRowSplit gets a dummy entry to force the output of the last line
                            // 
                            ListRowSplit = "-,-".Split(",");
                        }
                        
                        if ((UBound(ListRowSplit) > 0)) {
                            if ((ListRowSplit[0] != "")) {
                                if ((IndexName == "")) {
                                    // 
                                    //  first line of the first index description
                                    // 
                                    IndexName = ListRowSplit[0];
                                    IndexFields = ListRowSplit[1];
                                }
                                else if ((IndexName == ListRowSplit[0])) {
                                    // 
                                    //  next line of the index description
                                    // 
                                    IndexFields = (IndexFields + ("," + ListRowSplit[1]));
                                }
                                else {
                                    // 
                                    //  first line of a new index description
                                    //  save previous line
                                    // 
                                    if (((IndexName != "") 
                                                && (IndexFields != ""))) {
                                        sb.Append("<SQLIndex");
                                        sb.Append((" Indexname=\"" 
                                                        + (this.EncodeXMLattribute(IndexName) + "\"")));
                                        sb.Append((" DataSourceName=\"" 
                                                        + (this.EncodeXMLattribute(DataSourceName) + "\"")));
                                        sb.Append((" TableName=\"" 
                                                        + (this.EncodeXMLattribute(TableName) + "\"")));
                                        sb.Append((" FieldNameList=\"" 
                                                        + (this.EncodeXMLattribute(IndexFields) + "\"")));
                                        sb.Append(("></SQLIndex>" + "\r\n"));
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
            
            cpCore.db.csClose(CS);
            GetXMLContentDefinition_SQLIndexes = sb.ToString;
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            this.HandleClassErrorAndBubble(appName, "GetXMLContentDefinition_SQLIndexes");
        }
        
        // 
        // ========================================================================
        //  ----- Save the admin menus to CDef AdminMenu tags
        // ========================================================================
        // 
        private string GetXMLContentDefinition_AdminMenus() {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            string s = "";
            int ContentID;
            string appName;
            // 
            appName = cpCore.serverConfig.appConfig.name;
            s = (s + this.GetXMLContentDefinition_AdminMenus_MenuEntries());
            s = (s + this.GetXMLContentDefinition_AdminMenus_NavigatorEntries());
            // 
            GetXMLContentDefinition_AdminMenus = s;
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            this.HandleClassErrorAndBubble(appName, "GetXMLContentDefinition_AdminMenus");
        }
        
        // 
        // ========================================================================
        //  ----- Save the admin menus to CDef AdminMenu tags
        // ========================================================================
        // 
        private string GetXMLContentDefinition_AdminMenus_NavigatorEntries() {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            int NavIconType;
            string NavIconTitle;
            int CSPointer;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            DataTable dt;
            int ContentID;
            string menuNameSpace;
            string RecordName;
            int ParentID;
            int MenuContentID;
            string[] SplitArray;
            int SplitIndex;
            string appName;
            // 
            //  ****************************** if cdef not loaded, this fails
            // 
            appName = cpCore.serverConfig.appConfig.name;
            MenuContentID = cpCore.db.getRecordID("Content", cnNavigatorEntries);
            dt = cpCore.db.executeQuery(("select * from ccMenuEntries where (contentcontrolid=" 
                            + (MenuContentID + ")and(name<>\'\')")));
            if ((dt.Rows.Count > 0)) {
                NavIconType = 0;
                NavIconTitle = "";
                foreach (DataRow rsDr in dt.Rows) {
                    RecordName = genericController.encodeText(rsDr["Name"]);
                    if ((RecordName == "Advanced")) {
                        RecordName = RecordName;
                    }
                    
                    ParentID = genericController.EncodeInteger(rsDr["ParentID"]);
                    menuNameSpace = this.getMenuNameSpace(ParentID, "");
                    sb.Append(("<NavigatorEntry Name=\"" 
                                    + (this.EncodeXMLattribute(RecordName) + "\"")));
                    sb.Append((" NameSpace=\"" 
                                    + (menuNameSpace + "\"")));
                    sb.Append((" LinkPage=\"" 
                                    + (this.GetRSXMLAttribute(appName, rsDr, "LinkPage") + "\"")));
                    sb.Append((" ContentName=\"" 
                                    + (this.GetRSXMLLookupAttribute(appName, rsDr, "ContentID", "ccContent") + "\"")));
                    sb.Append((" AdminOnly=\"" 
                                    + (this.GetRSXMLAttribute(appName, rsDr, "AdminOnly") + "\"")));
                    sb.Append((" DeveloperOnly=\"" 
                                    + (this.GetRSXMLAttribute(appName, rsDr, "DeveloperOnly") + "\"")));
                    sb.Append((" NewWindow=\"" 
                                    + (this.GetRSXMLAttribute(appName, rsDr, "NewWindow") + "\"")));
                    sb.Append((" Active=\"" 
                                    + (this.GetRSXMLAttribute(appName, rsDr, "Active") + "\"")));
                    sb.Append((" AddonName=\"" 
                                    + (this.GetRSXMLLookupAttribute(appName, rsDr, "AddonID", "ccAggregateFunctions") + "\"")));
                    sb.Append((" SortOrder=\"" 
                                    + (this.GetRSXMLAttribute(appName, rsDr, "SortOrder") + "\"")));
                    NavIconType = genericController.EncodeInteger(this.GetRSXMLAttribute(appName, rsDr, "NavIconType"));
                    NavIconTitle = this.GetRSXMLAttribute(appName, rsDr, "NavIconTitle");
                    sb.Append((" NavIconTitle=\"" 
                                    + (NavIconTitle + "\"")));
                    SplitArray = (NavIconTypeList + ",help").Split(",");
                    SplitIndex = (NavIconType - 1);
                    if (((SplitIndex >= 0) 
                                && (SplitIndex <= UBound(SplitArray)))) {
                        sb.Append((" NavIconType=\"" 
                                        + (SplitArray[SplitIndex] + "\"")));
                    }
                    else {
                        SplitIndex = SplitIndex;
                    }
                    
                    // 
                    if (true) {
                        sb.Append((" guid=\"" 
                                        + (this.GetRSXMLAttribute(appName, rsDr, "ccGuid") + "\"")));
                    }
                    else if (true) {
                        sb.Append((" guid=\"" 
                                        + (this.GetRSXMLAttribute(appName, rsDr, "NavGuid") + "\"")));
                    }
                    
                    // 
                    sb.Append(("></NavigatorEntry>" + "\r\n"));
                }
                
            }
            
            GetXMLContentDefinition_AdminMenus_NavigatorEntries = sb.ToString;
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            this.HandleClassErrorAndBubble(appName, "GetXMLContentDefinition_NavigatorEntries");
        }
        
        // 
        // ========================================================================
        //  ----- Save the admin menus to CDef AdminMenu tags
        // ========================================================================
        // 
        private string GetXMLContentDefinition_AdminMenus_MenuEntries() {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            int CSPointer;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            DataTable rs;
            int ContentID;
            // dim buildversion As String
            string menuNameSpace;
            string RecordName;
            int ParentID;
            int MenuContentID;
            string appName;
            // 
            appName = cpCore.serverConfig.appConfig.name;
            //  BuildVersion = cpCore.app.getSiteProperty("BuildVersion", "0.0.000", SystemMemberID)
            // 
            //  ****************************** if cdef not loaded, this fails
            // 
            MenuContentID = cpCore.db.getRecordID("Content", cnNavigatorEntries);
            rs = cpCore.db.executeQuery(("select * from ccMenuEntries where (contentcontrolid=" 
                            + (MenuContentID + ")and(name<>\'\')")));
            if (isDataTableOk(rs)) {
                if (true) {
                    foreach (DataRow dr in rs.Rows) {
                        RecordName = genericController.encodeText(dr["Name"]);
                        sb.Append(("<MenuEntry Name=\"" 
                                        + (this.EncodeXMLattribute(RecordName) + "\"")));
                        sb.Append((" ParentName=\"" 
                                        + (this.GetRSXMLLookupAttribute(appName, dr, "ParentID", "ccMenuEntries") + "\"")));
                        sb.Append((" LinkPage=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "LinkPage") + "\"")));
                        sb.Append((" ContentName=\"" 
                                        + (this.GetRSXMLLookupAttribute(appName, dr, "ContentID", "ccContent") + "\"")));
                        sb.Append((" AdminOnly=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "AdminOnly") + "\"")));
                        sb.Append((" DeveloperOnly=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "DeveloperOnly") + "\"")));
                        sb.Append((" NewWindow=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "NewWindow") + "\"")));
                        sb.Append((" Active=\"" 
                                        + (this.GetRSXMLAttribute(appName, dr, "Active") + "\"")));
                        if (true) {
                            sb.Append((" AddonName=\"" 
                                            + (this.GetRSXMLLookupAttribute(appName, dr, "AddonID", "ccAggregateFunctions") + "\"")));
                        }
                        
                        sb.Append(("/>" + "\r\n"));
                    }
                    
                }
                
            }
            
            GetXMLContentDefinition_AdminMenus_MenuEntries = sb.ToString;
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            this.HandleClassErrorAndBubble(appName, "GetXMLContentDefinition_NavigatorEntries");
        }
        
        // 
        // ========================================================================
        // 
        // ========================================================================
        // 
        private string GetXMLContentDefinition_AggregateFunctions() {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            DataTable rs;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string appName;
            // 
            appName = cpCore.serverConfig.appConfig.name;
            rs = cpCore.db.executeQuery("select * from ccAggregateFunctions");
            if (isDataTableOk(rs)) {
                if (true) {
                    foreach (DataRow rsdr in rs.Rows) {
                        sb.Append(("<Addon Name=\"" 
                                        + (this.GetRSXMLAttribute(appName, rsdr, "Name") + "\"")));
                        sb.Append((" Link=\"" 
                                        + (this.GetRSXMLAttribute(appName, rsdr, "Link") + "\"")));
                        sb.Append((" ObjectProgramID=\"" 
                                        + (this.GetRSXMLAttribute(appName, rsdr, "ObjectProgramID") + "\"")));
                        sb.Append((" ArgumentList=\"" 
                                        + (this.GetRSXMLAttribute(appName, rsdr, "ArgumentList") + "\"")));
                        sb.Append((" SortOrder=\"" 
                                        + (this.GetRSXMLAttribute(appName, rsdr, "SortOrder") + "\"")));
                        sb.Append(" >");
                        sb.Append(this.GetRSXMLAttribute(appName, rsdr, "Copy"));
                        sb.Append(("</Addon>" + "\r\n"));
                    }
                    
                }
                
            }
            
            GetXMLContentDefinition_AggregateFunctions = sb.ToString;
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            this.HandleClassErrorAndBubble(appName, "GetXMLContentDefinition_AggregateFunctions");
        }
        
        // 
        // 
        // 
        private string EncodeXMLattribute(string Source) {
            EncodeXMLattribute = genericController.encodeHTML(Source);
            EncodeXMLattribute = genericController.vbReplace(EncodeXMLattribute, "\r\n", " ");
            EncodeXMLattribute = genericController.vbReplace(EncodeXMLattribute, "\r", "");
            return genericController.vbReplace(EncodeXMLattribute, "\n", "");
        }
        
        // 
        // 
        // 
        private string GetTableRecordName(string TableName, int RecordID) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            DataTable dt;
            string appName;
            // 
            appName = cpCore.serverConfig.appConfig.name;
            if (((RecordID != 0) 
                        && (TableName != ""))) {
                dt = cpCore.db.executeQuery(("select Name from " 
                                + (TableName + (" where ID=" + RecordID))));
                if ((dt.Rows.Count > 0)) {
                    GetTableRecordName = dt.Rows[0].Item[0].ToString;
                }
                
            }
            
            // 
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            this.HandleClassErrorAndBubble(appName, "GetTableRecordName");
        }
        
        // 
        // 
        // 
        private string GetRSXMLAttribute(string appName, DataRow dr, string FieldName) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            GetRSXMLAttribute = this.EncodeXMLattribute(genericController.encodeText(dr(FieldName)));
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            this.HandleClassErrorAndBubble(appName, "GetRSXML");
        }
        
        // 
        // 
        // 
        private string GetRSXMLLookupAttribute(string appName, DataRow dr, string FieldName, string TableName) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            GetRSXMLLookupAttribute = this.EncodeXMLattribute(this.GetTableRecordName(TableName, genericController.EncodeInteger(dr(FieldName))));
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            this.HandleClassErrorAndBubble(appName, "GetRSXMLLookupAttribute");
        }
        
        // 
        // 
        // 
        private string getMenuNameSpace(int RecordID, string UsedIDString) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            DataTable rs;
            int ParentID;
            string RecordName = "";
            string ParentSpace = "";
            string appName;
            // 
            appName = cpCore.serverConfig.appConfig.name;
            if ((RecordID != 0)) {
                if ((genericController.vbInstr(1, ("," 
                                + (UsedIDString + ",")), ("," 
                                + (RecordID + ",")), vbTextCompare) != 0)) {
                    this.HandleClassErrorAndResume(appName, "getMenuNameSpace", ("Circular reference found in UsedIDString [" 
                                    + (UsedIDString + ("] getting ccMenuEntries namespace for recordid [" 
                                    + (RecordID + "]")))));
                    getMenuNameSpace = "";
                }
                else {
                    UsedIDString = (UsedIDString + ("," + RecordID));
                    ParentID = 0;
                    if ((RecordID != 0)) {
                        rs = cpCore.db.executeQuery(("select Name,ParentID from ccMenuEntries where ID=" + RecordID));
                        if (isDataTableOk(rs)) {
                            ParentID = genericController.EncodeInteger(rs.Rows[0].Item["ParentID"]);
                            RecordName = genericController.encodeText(rs.Rows[0].Item["Name"]);
                        }
                        
                        if (isDataTableOk(rs)) {
                            if (false) {
                                // RS.Close()
                            }
                            
                            // RS = Nothing
                        }
                        
                    }
                    
                    if ((RecordName != "")) {
                        if ((ParentID == RecordID)) {
                            // 
                            //  circular reference
                            // 
                            this.HandleClassErrorAndResume(appName, "getMenuNameSpace", ("Circular reference found (ParentID=RecordID) getting ccMenuEntries namespace for recordid [" 
                                            + (RecordID + "]")));
                            getMenuNameSpace = "";
                        }
                        else {
                            if ((ParentID != 0)) {
                                // 
                                //  get next parent
                                // 
                                ParentSpace = this.getMenuNameSpace(ParentID, UsedIDString);
                            }
                            
                            if ((ParentSpace != "")) {
                                getMenuNameSpace = (ParentSpace + ("." + RecordName));
                            }
                            else {
                                getMenuNameSpace = RecordName;
                            }
                            
                        }
                        
                    }
                    else {
                        getMenuNameSpace = "";
                    }
                    
                }
                
            }
            
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
        ErrorTrap:
            this.HandleClassErrorAndBubble(appName, "getMenuNameSpace");
        }
        
        // 
        // 
        // 
        private string CacheLookup(int RecordID, object[,] Cache) {
            // 
            int Ptr;
            // 
            CacheLookup = "";
            if ((RecordID != 0)) {
                for (Ptr = 0; (Ptr <= UBound(Cache, 2)); Ptr++) {
                    if ((genericController.EncodeInteger(Cache(0, Ptr)) == RecordID)) {
                        CacheLookup = genericController.encodeText(Cache(1, Ptr));
                        break;
                    }
                    
                }
                
            }
            
        }
        
        // 
        // 
        // 
        private string xaT(object Source) {
            return this.EncodeXMLattribute(genericController.encodeText(Source));
        }
        
        // 
        // 
        // 
        private string xaB(object Source) {
            return genericController.EncodeBoolean(genericController.encodeText(Source)).ToString();
        }
        
        // 
        // ===========================================================================
        //    Error handler
        // ===========================================================================
        // 
        private void HandleClassErrorAndBubble(string appName, string MethodName, string Cause, void =, void unknown) {
            // 
            // Warning!!! Optional parameters not supported
            throw new ApplicationException("Unexpected exception");
        }
        
        // 
        // ===========================================================================
        //    Error handler
        // ===========================================================================
        // 
        private void HandleClassErrorAndResume(string appName, string MethodName, string Cause) {
            // 
            throw new ApplicationException("Unexpected exception");
        }
    }
}