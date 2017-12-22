

using Models.Context;
using Models.Entity;
using Models;
using Contensive.Core;
using Contensive.BaseClasses;
using Contensive;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace Controllers {
    
    public class dbController : IDisposable {
        
        // 
        // ========================================================================
        // '' <summary>
        // '' Inserts a record in a content definition and returns a csv_ContentSet with just that record
        // '' If there was a problem, it returns -1
        // '' </summary>
        // '' <param name="ContentName"></param>
        // '' <param name="MemberID"></param>
        // '' <returns></returns>
        public int csInsertRecord(string ContentName, int MemberID, void =, void -, void 1) {
            int returnCs = -1;
            // Warning!!! Optional parameters not supported
            try {
                string DateAddedString;
                string CreateKeyString;
                string Criteria;
                string DataSourceName;
                string FieldName;
                string TableName;
                Models.Complex.cdefModel CDef;
                string DefaultValueText;
                string LookupContentName;
                int Ptr;
                string[] lookups;
                string UCaseDefaultValueText;
                sqlFieldListClass sqlList = new sqlFieldListClass();
                // 
                if (string.IsNullOrEmpty(ContentName.Trim())) {
                    throw new ArgumentException("ContentName cannot be blank");
                }
                else {
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
                    if ((CDef == null)) {
                        throw new ApplicationException(("content [" 
                                        + (ContentName + "] could Not be found.")));
                    }
                    else if ((CDef.Id <= 0)) {
                        throw new ApplicationException(("content [" 
                                        + (ContentName + "] could Not be found.")));
                    }
                    else {
                        if ((MemberID == -1)) {
                            MemberID = cpCore.doc.authContext.user.id;
                        }
                        
                        // With...
                        // 
                        //  no authoring, create default record in Live table
                        // 
                        DataSourceName = CDef.ContentDataSourceName;
                        TableName = CDef.ContentTableName;
                        if ((CDef.fields.Count > 0)) {
                            foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in CDef.fields) {
                                Models.Complex.CDefFieldModel field = keyValuePair.Value;
                                // With...
                                FieldName = field.nameLc;
                                if (((FieldName != "") 
                                            && !string.IsNullOrEmpty(field.defaultValue))) {
                                    switch (genericController.vbUCase(FieldName)) {
                                        case "CREATEKEY":
                                        case "DATEADDED":
                                        case "CREATEDBY":
                                        case "CONTENTCONTROLID":
                                        case "ID":
                                            break;
                                        case FieldTypeIdAutoIdIncrement:
                                            // 
                                            //  cannot insert an autoincremnt
                                            // 
                                            break;
                                        case FieldTypeIdRedirect:
                                        case FieldTypeIdManyToMany:
                                            // 
                                            //  ignore these fields, they have no associated DB field
                                            // 
                                            break;
                                        case FieldTypeIdBoolean:
                                            sqlList.add(FieldName, encodeSQLBoolean(genericController.EncodeBoolean(field.defaultValue)));
                                            break;
                                        case FieldTypeIdCurrency:
                                        case FieldTypeIdFloat:
                                            sqlList.add(FieldName, encodeSQLNumber(genericController.EncodeNumber(field.defaultValue)));
                                            break;
                                        case FieldTypeIdInteger:
                                        case FieldTypeIdMemberSelect:
                                            sqlList.add(FieldName, encodeSQLNumber(genericController.EncodeInteger(field.defaultValue)));
                                            break;
                                        case FieldTypeIdDate:
                                            sqlList.add(FieldName, this.encodeSQLDate(genericController.EncodeDate(field.defaultValue)));
                                            break;
                                        case FieldTypeIdLookup:
                                            // 
                                            //  refactor --
                                            //  This is a problem - the defaults should come in as the ID values, not the names
                                            //    so a select can be added to the default configuration page
                                            // 
                                            DefaultValueText = genericController.encodeText(field.defaultValue);
                                            if ((DefaultValueText == "")) {
                                                DefaultValueText = "null";
                                            }
                                            else if ((field.lookupContentID != 0)) {
                                                LookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, field.lookupContentID);
                                                if ((LookupContentName != "")) {
                                                    DefaultValueText = getRecordID(LookupContentName, DefaultValueText).ToString();
                                                }
                                                
                                            }
                                            else if ((field.lookupList != "")) {
                                                UCaseDefaultValueText = genericController.vbUCase(DefaultValueText);
                                                lookups = field.lookupList.Split(",");
                                                for (Ptr = 0; (Ptr <= UBound(lookups)); Ptr++) {
                                                    if ((UCaseDefaultValueText == genericController.vbUCase(lookups[Ptr]))) {
                                                        DefaultValueText = ((Ptr + 1)).ToString();
                                                    }
                                                    
                                                }
                                                
                                            }
                                            
                                            sqlList.add(FieldName, DefaultValueText);
                                            break;
                                        default:
                                            sqlList.add(FieldName, this.encodeSQLText(field.defaultValue));
                                            break;
                                    }
                                }
                                
                            }
                            
                        }
                        
                        // 
                        CreateKeyString = encodeSQLNumber(genericController.GetRandomInteger);
                        DateAddedString = this.encodeSQLDate(Now);
                        // 
                        sqlList.add("CREATEKEY", CreateKeyString);
                        //  ArrayPointer)
                        sqlList.add("DATEADDED", DateAddedString);
                        //  ArrayPointer)
                        sqlList.add("CONTENTCONTROLID", encodeSQLNumber(CDef.Id));
                        //  ArrayPointer)
                        sqlList.add("CREATEDBY", encodeSQLNumber(MemberID));
                        //  ArrayPointer)
                        // 
                        insertTableRecord(DataSourceName, TableName, sqlList);
                        // 
                        //  ----- Get the record back so we can use the ID
                        // 
                        Criteria = ("((createkey=" 
                                    + (CreateKeyString + (")And(DateAdded=" 
                                    + (DateAddedString + "))"))));
                        returnCs = csOpen(ContentName, Criteria, "ID DESC", false, MemberID, false, true);
                        // '
                        // ' ----- Clear Time Stamp because a record changed
                        // '
                        // If coreWorkflowClass.csv_AllowAutocsv_ClearContentTimeStamp Then
                        //     Call cpCore.cache.invalidateObject(ContentName)
                        // End If
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnCs;
        }
        
        // 
        // ========================================================================
        //  Opens a Content Record
        //    If there was a problem, it returns -1 (not csv_IsCSOK)
        //    Can open either the ContentRecord or the AuthoringRecord (WorkflowAuthoringMode)
        //    Isolated in API so later we can save record in an Index buffer for fast access
        // ========================================================================
        // 
        public int cs_openContentRecord(string ContentName, int RecordID, int MemberID, void =, void SystemMemberID, bool WorkflowAuthoringMode, void =, void False, bool WorkflowEditingMode, void =, void False, string SelectFieldList, void =, void ) {
            int returnResult = -1;
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            try {
                if ((RecordID <= 0)) {
                    //  no error, return -1 - Throw New ArgumentException("recordId is not valid [" & RecordID & "]")
                }
                else {
                    returnResult = csOpen(ContentName, ("(ID=" 
                                    + (encodeSQLNumber(RecordID) + ")")), ,, false, MemberID, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1);
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnResult;
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' true if csPointer is a valid dataset, and currently points to a valid row
        // '' </summary>
        // '' <param name="CSPointer"></param>
        // '' <returns></returns>
        bool csOk(int CSPointer) {
            bool returnResult = false;
            try {
                if ((CSPointer < 0)) {
                    returnResult = false;
                }
                else if ((CSPointer >= contentSetStore.Count)) {
                    throw new ArgumentException("dateset is not valid");
                }
                else {
                    // With...
                    returnResult = (contentSetStore(CSPointer).IsOpen 
                                & ((contentSetStore(CSPointer).readCacheRowPtr >= 0) 
                                & (contentSetStore(CSPointer).readCacheRowPtr < contentSetStore(CSPointer).readCacheRowCnt)));
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnResult;
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' copy the current row of the source dataset to the destination dataset. The destination dataset must have been created with cs open or insert, and must contain all the fields in the source dataset.
        // '' </summary>
        // '' <param name="CSSource"></param>
        // '' <param name="CSDestination"></param>
        // ========================================================================
        // 
        public void csCopyRecord(int CSSource, int CSDestination) {
            try {
                string FieldName;
                string DestContentName;
                int DestRecordID;
                string DestFilename;
                string SourceFilename;
                Models.Complex.cdefModel DestCDef;
                // 
                if (!this.csOk(CSSource)) {
                    throw new ArgumentException("source dataset is not valid");
                }
                else if (!this.csOk(CSDestination)) {
                    throw new ArgumentException("destination dataset is not valid");
                }
                else if ((contentSetStore(CSDestination).CDef == null)) {
                    throw new ArgumentException("copyRecord requires the destination dataset to be created from a cs Open or Insert, not a query.");
                }
                else {
                    DestCDef = contentSetStore(CSDestination).CDef;
                    DestContentName = DestCDef.Name;
                    DestRecordID = csGetInteger(CSDestination, "ID");
                    FieldName = cs_getFirstFieldName(CSSource);
                    while (!string.IsNullOrEmpty(FieldName)) {
                        switch (genericController.vbUCase(FieldName)) {
                            case "ID":
                                break;
                            default:
                                int sourceFieldTypeId = cs_getFieldTypeId(CSSource, FieldName);
                                switch (sourceFieldTypeId) {
                                    case FieldTypeIdRedirect:
                                    case FieldTypeIdManyToMany:
                                        break;
                                    case FieldTypeIdFile:
                                    case FieldTypeIdFileImage:
                                    case FieldTypeIdFileCSS:
                                    case FieldTypeIdFileXML:
                                    case FieldTypeIdFileJavascript:
                                        // 
                                        //  ----- cdn file
                                        // 
                                        SourceFilename = csGetFilename(CSSource, FieldName, "", contentSetStore(CSDestination).CDef.Name, sourceFieldTypeId);
                                        // SourceFilename = (csv_cs_getText(CSSource, FieldName))
                                        if ((SourceFilename != "")) {
                                            DestFilename = csGetFilename(CSDestination, FieldName, "", DestContentName, sourceFieldTypeId);
                                            // DestFilename = csv_GetVirtualFilename(DestContentName, FieldName, DestRecordID)
                                            this.csSet(CSDestination, FieldName, DestFilename);
                                            cpCore.cdnFiles.copyFile(SourceFilename, DestFilename);
                                        }
                                        
                                        break;
                                    case FieldTypeIdFileText:
                                    case FieldTypeIdFileHTML:
                                        // 
                                        //  ----- private file
                                        // 
                                        SourceFilename = csGetFilename(CSSource, FieldName, "", DestContentName, sourceFieldTypeId);
                                        // SourceFilename = (csv_cs_getText(CSSource, FieldName))
                                        if ((SourceFilename != "")) {
                                            DestFilename = csGetFilename(CSDestination, FieldName, "", DestContentName, sourceFieldTypeId);
                                            // DestFilename = csv_GetVirtualFilename(DestContentName, FieldName, DestRecordID)
                                            this.csSet(CSDestination, FieldName, DestFilename);
                                            cpCore.cdnFiles.copyFile(SourceFilename, DestFilename);
                                        }
                                        
                                        break;
                                    default:
                                        this.csSet(CSDestination, FieldName, cs_getValue(CSSource, FieldName));
                                        break;
                                }
                                break;
                        }
                        FieldName = cs_getNextFieldName(CSSource);
                    }
                    
                    this.csSave2(CSDestination);
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        //        
        // ========================================================================
        // '' <summary>
        // '' Returns the Source for the csv_ContentSet
        // '' </summary>
        // '' <param name="CSPointer"></param>
        // '' <returns></returns>
        public string csGetSource(int CSPointer) {
            string returnResult = "";
            try {
                if (!this.csOk(CSPointer)) {
                    throw new ArgumentException("the dataset is not valid");
                }
                else {
                    returnResult = contentSetStore(CSPointer).Source;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnResult;
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' Returns the value of a field, decoded into a text string result, if there is a problem, null is returned, this may be because the lookup record is inactive, so its not an error
        // '' </summary>
        // '' <param name="CSPointer"></param>
        // '' <param name="FieldName"></param>
        // '' <returns></returns>
        // 
        public string csGet(int CSPointer, string FieldName) {
            string fieldValue = "";
            try {
                int FieldValueInteger;
                string LookupContentName;
                string LookupList;
                string[] lookups;
                object FieldValueVariant;
                int CSLookup;
                int fieldTypeId;
                int fieldLookupId;
                // 
                //  ----- needs work. Go to fields table and get field definition
                //        then print accordingly
                // 
                if (!this.csOk(CSPointer)) {
                    throw new ArgumentException("the dataset is not valid");
                }
                else if (string.IsNullOrEmpty(FieldName.Trim())) {
                    throw new ArgumentException("fieldname cannot be blank");
                }
                else {
                    // 
                    //  csv_ContentSet good
                    // 
                    // With...
                    if (!contentSetStore(CSPointer).Updateable) {
                        // 
                        //  Not updateable -- Just return what is there as a string
                        // 
                        try {
                            fieldValue = genericController.encodeText(cs_getValue(CSPointer, FieldName));
                        }
                        catch (Exception ex) {
                            throw new ApplicationException(("Error [" 
                                            + (ex.Message + ("] reading field [" 
                                            + (FieldName.ToLower + ("] In source [" 
                                            + (contentSetStore(CSPointer).Source + "")))))));
                        }
                        
                    }
                    else {
                        // 
                        //  Updateable -- enterprete the value
                        // 
                        // ContentName = .ContentName
                        Models.Complex.CDefFieldModel field;
                        if (!contentSetStore(CSPointer).CDef.fields.ContainsKey) {
                            FieldName.ToLower();
                            try {
                                fieldValue = genericController.encodeText(cs_getValue(CSPointer, FieldName));
                            }
                            catch (Exception ex) {
                                throw new ApplicationException(("Error [" 
                                                + (ex.Message + ("] reading field [" 
                                                + (FieldName.ToLower + ("] In content [" 
                                                + (contentSetStore(CSPointer).CDef.Name + ("] With custom field list [" 
                                                + (contentSetStore(CSPointer).SelectTableFieldList + "")))))))));
                            }
                            
                        }
                        else {
                            field = contentSetStore(CSPointer).CDef.fields;
                            FieldName.ToLower;
                            fieldTypeId = field.fieldTypeId;
                            if ((fieldTypeId == FieldTypeIdManyToMany)) {
                                // 
                                //  special case - recordset contains no data - return record id list
                                // 
                                int RecordID;
                                string DbTable;
                                string ContentName;
                                string SQL;
                                DataTable rs;
                                if (contentSetStore(CSPointer).CDef.fields.ContainsKey) {
                                    "id";
                                    RecordID = genericController.EncodeInteger(cs_getValue(CSPointer, "id"));
                                    // With...
                                    ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, field.manyToManyRuleContentID);
                                    DbTable = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName);
                                    SQL = ("Select " 
                                                + (field.ManyToManyRuleSecondaryField + (" from " 
                                                + (DbTable + (" where " 
                                                + (field.ManyToManyRulePrimaryField + ("=" + RecordID)))))));
                                    rs = executeQuery(SQL);
                                    if (genericController.isDataTableOk(rs)) {
                                        foreach (DataRow dr in rs.Rows) {
                                            ("," + dr.Item[0].ToString);
                                        }
                                        
                                        fieldValue = fieldValue.Substring(1);
                                    }
                                    
                                }
                                
                            }
                            else if ((fieldTypeId == FieldTypeIdRedirect)) {
                                // 
                                //  special case - recordset contains no data - return blank
                                // 
                                fieldTypeId = fieldTypeId;
                            }
                            else {
                                FieldValueVariant = cs_getValue(CSPointer, FieldName);
                                if (!genericController.IsNull(FieldValueVariant)) {
                                    // 
                                    //  Field is good
                                    // 
                                    switch (fieldTypeId) {
                                        case FieldTypeIdBoolean:
                                            // 
                                            // 
                                            // 
                                            if (genericController.EncodeBoolean(FieldValueVariant)) {
                                                fieldValue = "Yes";
                                            }
                                            else {
                                                fieldValue = "No";
                                            }
                                            
                                            // NeedsHTMLEncode = False
                                            break;
                                        case FieldTypeIdDate:
                                            // 
                                            // 
                                            // 
                                            if (IsDate(FieldValueVariant)) {
                                                // 
                                                //  formatdatetime returns 'wednesday june 5, 1990', which fails IsDate()!!
                                                // 
                                                fieldValue = genericController.EncodeDate(FieldValueVariant).ToString();
                                            }
                                            
                                            break;
                                        case FieldTypeIdLookup:
                                            // 
                                            // 
                                            // 
                                            if (genericController.vbIsNumeric(FieldValueVariant)) {
                                                fieldLookupId = field.lookupContentID;
                                                LookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, fieldLookupId);
                                                LookupList = field.lookupList;
                                                if ((LookupContentName != "")) {
                                                    // 
                                                    //  -- First try Lookup Content
                                                    CSLookup = csOpen(LookupContentName, ("ID=" + encodeSQLNumber(genericController.EncodeInteger(FieldValueVariant))), ,, ,, ,, "name", 1);
                                                    if (this.csOk(CSLookup)) {
                                                        fieldValue = csGetText(CSLookup, "name");
                                                    }
                                                    
                                                    csClose(CSLookup);
                                                }
                                                else if ((LookupList != "")) {
                                                    // 
                                                    //  -- Next try lookup list
                                                    FieldValueInteger = (genericController.EncodeInteger(FieldValueVariant) - 1);
                                                    if ((FieldValueInteger >= 0)) {
                                                        lookups = LookupList.Split(",");
                                                        if ((UBound(lookups) >= FieldValueInteger)) {
                                                            fieldValue = lookups[FieldValueInteger];
                                                        }
                                                        
                                                    }
                                                    
                                                }
                                                
                                            }
                                            
                                            break;
                                        case FieldTypeIdMemberSelect:
                                            // 
                                            // 
                                            // 
                                            if (genericController.vbIsNumeric(FieldValueVariant)) {
                                                fieldValue = getRecordName("people", genericController.EncodeInteger(FieldValueVariant));
                                            }
                                            
                                            break;
                                        case FieldTypeIdCurrency:
                                            // 
                                            // 
                                            // 
                                            if (genericController.vbIsNumeric(FieldValueVariant)) {
                                                fieldValue = FormatCurrency(FieldValueVariant, 2, vbFalse, vbFalse, vbFalse);
                                            }
                                            
                                            // NeedsHTMLEncode = False
                                            break;
                                        case FieldTypeIdFileText:
                                        case FieldTypeIdFileHTML:
                                            // 
                                            // 
                                            // 
                                            fieldValue = cpCore.cdnFiles.readFile(genericController.encodeText(FieldValueVariant));
                                            break;
                                        case FieldTypeIdFileCSS:
                                        case FieldTypeIdFileXML:
                                        case FieldTypeIdFileJavascript:
                                            // 
                                            // 
                                            // 
                                            fieldValue = cpCore.cdnFiles.readFile(genericController.encodeText(FieldValueVariant));
                                            // NeedsHTMLEncode = False
                                            break;
                                        case FieldTypeIdText:
                                        case FieldTypeIdLongText:
                                        case FieldTypeIdHTML:
                                            // 
                                            // 
                                            // 
                                            fieldValue = genericController.encodeText(FieldValueVariant);
                                            break;
                                        case FieldTypeIdFile:
                                        case FieldTypeIdFileImage:
                                        case FieldTypeIdLink:
                                        case FieldTypeIdResourceLink:
                                        case FieldTypeIdAutoIdIncrement:
                                        case FieldTypeIdFloat:
                                        case FieldTypeIdInteger:
                                            // 
                                            // 
                                            // 
                                            fieldValue = genericController.encodeText(FieldValueVariant);
                                            // NeedsHTMLEncode = False
                                            break;
                                        case FieldTypeIdRedirect:
                                        case FieldTypeIdManyToMany:
                                            // 
                                            //  This case is covered before the select - but leave this here as safety net
                                            // 
                                            // NeedsHTMLEncode = False
                                            break;
                                        default:
                                            throw new ApplicationException(("Can Not use field [" 
                                                            + (FieldName + ("] because the FieldType [" 
                                                            + (fieldTypeId + "] Is invalid.")))));
                                            break;
                                    }
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return fieldValue;
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' Saves the value to the field, independant of field type, this routine accounts for the destination type, and saves the field as required (file, etc)
        // '' </summary>
        // '' <param name="CSPointer"></param>
        // '' <param name="FieldName"></param>
        // '' <param name="FieldValue"></param>
        // 
        public void csSet(int CSPointer, string FieldName, string FieldValue) {
            try {
                string BlankTest;
                string FieldNameLc;
                bool SetNeeded;
                string fileNameNoExt;
                string ContentName;
                string fileName;
                string pathFilenameOriginal;
                // 
                if (!this.csOk(CSPointer)) {
                    throw new ArgumentException("dataset is not valid or End-Of-file.");
                }
                else if (string.IsNullOrEmpty(FieldName.Trim())) {
                    throw new ArgumentException("fieldName cannnot be blank");
                }
                else {
                    // With...
                    if (!contentSetStore(CSPointer).Updateable) {
                        throw new ApplicationException("Cannot update a contentset created from a sql query.");
                    }
                    else {
                        ContentName = contentSetStore(CSPointer).ContentName;
                        FieldNameLc = FieldName.Trim().ToLower;
                        if ((FieldValue == null)) {
                            FieldValue = String.Empty;
                        }
                        
                        // With...
                        if ((contentSetStore(CSPointer).CDef.Name != "")) {
                            Models.Complex.CDefFieldModel field;
                            if (!contentSetStore(CSPointer).CDef.fields.ContainsKey) {
                                FieldNameLc;
                                throw new ArgumentException(("The field [" 
                                                + (FieldName + ("] could Not be found In content [" 
                                                + (contentSetStore(CSPointer).CDef.Name + "]")))));
                            }
                            else {
                                field = contentSetStore(CSPointer).CDef.fields.Item;
                                FieldNameLc;
                                switch (field.fieldTypeId) {
                                    case FieldTypeIdAutoIdIncrement:
                                    case FieldTypeIdRedirect:
                                    case FieldTypeIdManyToMany:
                                        // 
                                        //  Never set
                                        // 
                                        break;
                                    case FieldTypeIdFile:
                                    case FieldTypeIdFileImage:
                                        // 
                                        //  Always set
                                        //  Saved in the field is the filename to the file
                                        //  csv_cs_get returns the filename
                                        //  csv_SetCS saves the filename
                                        // 
                                        // FieldValueVariantLocal = FieldValueVariantLocal
                                        SetNeeded = true;
                                        break;
                                    case FieldTypeIdFileText:
                                    case FieldTypeIdFileHTML:
                                        // 
                                        //  Always set
                                        //  A virtual file is created to hold the content, 'tablename/FieldNameLocal/0000.ext
                                        //  the extension is different for each fieldtype
                                        //  csv_SetCS and csv_cs_get return the content, not the filename
                                        // 
                                        //  Saved in the field is the filename of the virtual file
                                        //  TextFile, assume this call is only made if a change was made to the copy.
                                        //  Use the csv_SetCSTextFile to manage the modified name and date correctly.
                                        //  csv_SetCSTextFile uses this method to set the row changed, so leave this here.
                                        // 
                                        fileNameNoExt = csGetText(CSPointer, FieldNameLc);
                                        // FieldValue = genericController.encodeText(FieldValueVariantLocal)
                                        if ((FieldValue == "")) {
                                            if ((fileNameNoExt != "")) {
                                                cpCore.cdnFiles.deleteFile(fileNameNoExt);
                                                // Call publicFiles.DeleteFile(fileNameNoExt)
                                                fileNameNoExt = "";
                                            }
                                            
                                        }
                                        else {
                                            if ((fileNameNoExt == "")) {
                                                fileNameNoExt = csGetFilename(CSPointer, FieldName, "", ContentName, field.fieldTypeId);
                                            }
                                            
                                            cpCore.cdnFiles.saveFile(fileNameNoExt, FieldValue);
                                            // Call publicFiles.SaveFile(fileNameNoExt, FieldValue)
                                        }
                                        
                                        FieldValue = fileNameNoExt;
                                        SetNeeded = true;
                                        break;
                                    case FieldTypeIdFileCSS:
                                    case FieldTypeIdFileXML:
                                    case FieldTypeIdFileJavascript:
                                        // 
                                        //  public files - save as FieldTypeTextFile except if only white space, consider it blank
                                        // 
                                        string PathFilename;
                                        string FileExt;
                                        int FilenameRev;
                                        string path;
                                        int Pos;
                                        pathFilenameOriginal = csGetText(CSPointer, FieldNameLc);
                                        PathFilename = pathFilenameOriginal;
                                        BlankTest = FieldValue;
                                        BlankTest = genericController.vbReplace(BlankTest, " ", "");
                                        BlankTest = genericController.vbReplace(BlankTest, "\r", "");
                                        BlankTest = genericController.vbReplace(BlankTest, "\n", "");
                                        BlankTest = genericController.vbReplace(BlankTest, '\t', "");
                                        if ((BlankTest == "")) {
                                            if ((PathFilename != "")) {
                                                cpCore.cdnFiles.deleteFile(PathFilename);
                                                PathFilename = "";
                                            }
                                            
                                        }
                                        else {
                                            if ((PathFilename == "")) {
                                                PathFilename = csGetFilename(CSPointer, FieldNameLc, "", ContentName, field.fieldTypeId);
                                            }
                                            
                                            if ((PathFilename.Substring(0, 1) == "/")) {
                                                // 
                                                //  root file, do not include revision
                                                // 
                                            }
                                            else {
                                                // 
                                                //  content file, add a revision to the filename
                                                // 
                                                Pos = InStrRev(PathFilename, ".");
                                                if ((Pos > 0)) {
                                                    FileExt = PathFilename.Substring(Pos);
                                                    fileNameNoExt = PathFilename.Substring(0, (Pos - 1));
                                                    Pos = InStrRev(fileNameNoExt, "/");
                                                    if ((Pos > 0)) {
                                                        // path = PathFilename
                                                        fileNameNoExt = fileNameNoExt.Substring(Pos);
                                                        path = PathFilename.Substring(0, Pos);
                                                        FilenameRev = 1;
                                                        if (!genericController.vbIsNumeric(fileNameNoExt)) {
                                                            Pos = genericController.vbInstr(1, fileNameNoExt, ".r", vbTextCompare);
                                                            if ((Pos > 0)) {
                                                                FilenameRev = genericController.EncodeInteger(fileNameNoExt.Substring((Pos + 1)));
                                                                FilenameRev = (FilenameRev + 1);
                                                                fileNameNoExt = fileNameNoExt.Substring(0, (Pos - 1));
                                                            }
                                                            
                                                        }
                                                        
                                                        fileName = (fileNameNoExt + (".r" 
                                                                    + (FilenameRev + ("." + FileExt))));
                                                        // PathFilename = PathFilename & dstFilename
                                                        path = genericController.convertCdnUrlToCdnPathFilename(path);
                                                        // srcSysFile = config.physicalFilePath & genericController.vbReplace(srcPathFilename, "/", "\")
                                                        // dstSysFile = config.physicalFilePath & genericController.vbReplace(PathFilename, "/", "\")
                                                        PathFilename = (path + fileName);
                                                        // Call publicFiles.renameFile(pathFilenameOriginal, fileName)
                                                    }
                                                    
                                                }
                                                
                                            }
                                            
                                            if (((pathFilenameOriginal != "") 
                                                        && (pathFilenameOriginal != PathFilename))) {
                                                pathFilenameOriginal = genericController.convertCdnUrlToCdnPathFilename(pathFilenameOriginal);
                                                cpCore.cdnFiles.deleteFile(pathFilenameOriginal);
                                            }
                                            
                                            cpCore.cdnFiles.saveFile(PathFilename, FieldValue);
                                        }
                                        
                                        FieldValue = PathFilename;
                                        SetNeeded = true;
                                        break;
                                    case FieldTypeIdBoolean:
                                        // 
                                        //  Boolean - sepcial case, block on typed GetAlways set
                                        if ((genericController.EncodeBoolean(FieldValue) != csGetBoolean(CSPointer, FieldNameLc))) {
                                            SetNeeded = true;
                                        }
                                        
                                        break;
                                    case FieldTypeIdText:
                                        // 
                                        //  Set if text of value changes
                                        // 
                                        if ((genericController.encodeText(FieldValue) != csGetText(CSPointer, FieldNameLc))) {
                                            SetNeeded = true;
                                            if ((FieldValue.Length > 255)) {
                                                cpCore.handleException(new ApplicationException(("Text length too long saving field [" 
                                                                    + (FieldName + ("], length [" 
                                                                    + (FieldValue.Length + "], but max for Text field is 255. Save will be attempted"))))));
                                            }
                                            
                                        }
                                        
                                        break;
                                    case FieldTypeIdLongText:
                                    case FieldTypeIdHTML:
                                        // 
                                        //  Set if text of value changes
                                        // 
                                        if ((genericController.encodeText(FieldValue) != csGetText(CSPointer, FieldNameLc))) {
                                            SetNeeded = true;
                                            if ((FieldValue.Length > 65535)) {
                                                cpCore.handleException(new ApplicationException(("Text length too long saving field [" 
                                                                    + (FieldName + ("], length [" 
                                                                    + (FieldValue.Length + "], but max for LongText and Html is 65535. Save will be attempted"))))));
                                            }
                                            
                                        }
                                        
                                        break;
                                        SetNeeded = true;
                                        break;
                                }
                            }
                            
                        }
                        
                        if (!SetNeeded) {
                            SetNeeded = SetNeeded;
                        }
                        else {
                            // 
                            //  ----- set the new value into the row buffer
                            // 
                            if (contentSetStore(CSPointer).writeCache.ContainsKey) {
                                FieldNameLc;
                                contentSetStore(CSPointer).writeCache.Item;
                                FieldNameLc = FieldValue.ToString();
                            }
                            else {
                                contentSetStore(CSPointer).writeCache.Add;
                                FieldNameLc;
                                FieldValue.ToString();
                            }
                            
                            contentSetStore(CSPointer).LastUsed = DateTime.Now;
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        public void csSet(int CSPointer, string FieldName, DateTime FieldValue) {
            this.csSet(CSPointer, FieldName, FieldValue.ToString());
        }
        
        public void csSet(int CSPointer, string FieldName, bool FieldValue) {
            this.csSet(CSPointer, FieldName, FieldValue.ToString());
        }
        
        public void csSet(int CSPointer, string FieldName, int FieldValue) {
            this.csSet(CSPointer, FieldName, FieldValue.ToString());
        }
        
        public void csSet(int CSPointer, string FieldName, double FieldValue) {
            this.csSet(CSPointer, FieldName, FieldValue.ToString());
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' rollback, or undo the changes to the current row
        // '' </summary>
        // '' <param name="CSPointer"></param>
        public void csRollBack(int CSPointer) {
            try {
                if (!this.csOk(CSPointer)) {
                    throw new ArgumentException("dataset is not valid");
                }
                else {
                    contentSetStore(CSPointer).writeCache.Clear();
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' Save the current CS Cache back to the database
        // '' </summary>
        // '' <param name="CSPointer"></param>
        // '' <param name="AsyncSave"></param>
        // '' <param name="Blockcsv_ClearBake"></param>
        //    If in Workflow Edit, save authorable fields to EditRecord, non-authorable to (both EditRecord and LiveRecord)
        //    non-authorable fields are inactive, non-authorable, read-only, and not-editable
        // 
        //  Comment moved from in-line -- it was too hard to read around
        //  No -- IsModified is now set from an authoring control.
        //    Update all non-authorable fields in the edit record so they can be read in admin.
        //    Update all non-authorable fields in live record, because non-authorable is not a publish-able field
        //    edit record ModifiedDate in record only if non-authorable field is changed
        // 
        //  ???
        //    I believe Non-FieldAdminAuthorable Fields should only save to the LiveRecord.
        //    They should also be read from the LiveRecord.
        //    Saving to the EditRecord sets the record Modified, which fields like "Viewings" should not change
        // 
        // ========================================================================
        // 
        public void csSave2(int CSPointer, bool AsyncSave, void =, void False, bool Blockcsv_ClearBake, void =, void False) {
            try {
                DateTime sqlModifiedDate;
                // Warning!!! Optional parameters not supported
                // Warning!!! Optional parameters not supported
                int sqlModifiedBy;
                object writeCacheValue;
                string UcaseFieldName;
                string FieldName;
                int FieldFoundCount;
                bool FieldAdminAuthorable;
                bool FieldReadOnly;
                string SQL;
                string SQLSetPair;
                string SQLUpdate;
                // Dim SQLEditUpdate As String
                // Dim SQLEditDelimiter As String
                string SQLLiveUpdate;
                string SQLLiveDelimiter;
                string SQLCriteriaUnique = String.Empty;
                string UniqueViolationFieldList = String.Empty;
                string LiveTableName;
                string LiveDataSourceName;
                int LiveRecordID;
                // Dim EditRecordID As Integer
                int LiveRecordContentControlID;
                string LiveRecordContentName;
                // Dim EditTableName As String
                // Dim EditDataSourceName As String = ""
                bool AuthorableFieldUpdate;
                //  true if an Edit field is being updated
                // Dim WorkflowRenderingMode As Boolean
                //  Dim AllowWorkflowSave As Boolean
                string Copy;
                int ContentID;
                string ContentName;
                //  Dim WorkflowMode As Boolean
                bool LiveRecordInactive;
                int ColumnPtr;
                // 
                if (!this.csOk(CSPointer)) {
                    // 
                    //  already closed or not opened or not on a current row. No error so you can always call save(), it skips if nothing to save
                    // 
                    // Throw New ArgumentException("dataset is not valid")
                }
                else if ((contentSetStore(CSPointer).writeCache.Count == 0)) {
                    // 
                    //  nothing to write, just exit
                    // 
                }
                else if (!contentSetStore(CSPointer).Updateable) {
                    throw new ArgumentException("The dataset cannot be updated because it was created with a query and not a content table.");
                }
                else {
                    // With...
                    // 
                    // With...
                    LiveTableName = contentSetStore(CSPointer).CDef.ContentTableName;
                    LiveDataSourceName = contentSetStore(CSPointer).CDef.ContentDataSourceName;
                    ContentName = contentSetStore(CSPointer).CDef.Name;
                    ContentID = contentSetStore(CSPointer).CDef.Id;
                    // 
                    LiveRecordID = csGetInteger(CSPointer, "ID");
                    LiveRecordContentControlID = csGetInteger(CSPointer, "CONTENTCONTROLID");
                    LiveRecordContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, LiveRecordContentControlID);
                    LiveRecordInactive = !csGetBoolean(CSPointer, "ACTIVE");
                    // 
                    // 
                    SQLLiveDelimiter = "";
                    SQLLiveUpdate = "";
                    SQLLiveDelimiter = "";
                    sqlModifiedDate = DateTime.Now;
                    sqlModifiedBy = contentSetStore(CSPointer).OwnerMemberID;
                    // 
                    AuthorableFieldUpdate = false;
                    FieldFoundCount = 0;
                    foreach (keyValuePair in contentSetStore(CSPointer).writeCache) {
                        FieldName = keyValuePair.Key;
                        UcaseFieldName = genericController.vbUCase(FieldName);
                        writeCacheValue = keyValuePair.Value;
                        // 
                        //  field has changed
                        // 
                        if ((UcaseFieldName == "MODIFIEDBY")) {
                            // 
                            //  capture and block it - it is hardcoded in sql
                            // 
                            AuthorableFieldUpdate = true;
                            sqlModifiedBy = genericController.EncodeInteger(writeCacheValue);
                        }
                        else if ((UcaseFieldName == "MODIFIEDDATE")) {
                            // 
                            //  capture and block it - it is hardcoded in sql
                            // 
                            AuthorableFieldUpdate = true;
                            sqlModifiedDate = genericController.EncodeDate(writeCacheValue);
                        }
                        else {
                            // 
                            //  let these field be added to the sql
                            // 
                            LiveRecordInactive = ((UcaseFieldName == "ACTIVE") 
                                        && !genericController.EncodeBoolean(writeCacheValue));
                            FieldFoundCount++;
                            Models.Complex.CDefFieldModel field = contentSetStore(CSPointer).CDef.fields;
                            FieldName.ToLower();
                            // With...
                            SQLSetPair = "";
                            FieldReadOnly = field.ReadOnly;
                            FieldAdminAuthorable = (!field.ReadOnly 
                                        && (!field.NotEditable 
                                        && field.authorable));
                            switch (field.fieldTypeId) {
                                case FieldTypeIdRedirect:
                                case FieldTypeIdManyToMany:
                                    break;
                                case FieldTypeIdInteger:
                                case FieldTypeIdLookup:
                                case FieldTypeIdAutoIdIncrement:
                                case FieldTypeIdMemberSelect:
                                    SQLSetPair = (FieldName + ("=" + encodeSQLNumber(genericController.EncodeInteger(writeCacheValue))));
                                    break;
                                case FieldTypeIdCurrency:
                                case FieldTypeIdFloat:
                                    SQLSetPair = (FieldName + ("=" + encodeSQLNumber(genericController.EncodeNumber(writeCacheValue))));
                                    break;
                                case FieldTypeIdBoolean:
                                    SQLSetPair = (FieldName + ("=" + encodeSQLBoolean(genericController.EncodeBoolean(writeCacheValue))));
                                    break;
                                case FieldTypeIdDate:
                                    SQLSetPair = (FieldName + ("=" + this.encodeSQLDate(genericController.EncodeDate(writeCacheValue))));
                                    break;
                                case FieldTypeIdText:
                                    Copy = genericController.encodeText(writeCacheValue).Substring(0, 255);
                                    if (field.Scramble) {
                                        Copy = genericController.TextScramble(cpCore, Copy);
                                    }
                                    
                                    SQLSetPair = (FieldName + ("=" + this.encodeSQLText(Copy)));
                                    break;
                                case FieldTypeIdLink:
                                case FieldTypeIdResourceLink:
                                case FieldTypeIdFile:
                                case FieldTypeIdFileImage:
                                case FieldTypeIdFileText:
                                case FieldTypeIdFileCSS:
                                case FieldTypeIdFileXML:
                                case FieldTypeIdFileJavascript:
                                case FieldTypeIdFileHTML:
                                    Copy = genericController.encodeText(writeCacheValue).Substring(0, 255);
                                    SQLSetPair = (FieldName + ("=" + this.encodeSQLText(Copy)));
                                    break;
                                case FieldTypeIdLongText:
                                case FieldTypeIdHTML:
                                    SQLSetPair = (FieldName + ("=" + this.encodeSQLText(genericController.encodeText(writeCacheValue))));
                                    break;
                                default:
                                    throw new ApplicationException(("Can Not save this record because the field [" 
                                                    + (field.nameLc + ("] has an invalid field type Id [" 
                                                    + (field.fieldTypeId + "]")))));
                                    break;
                            }
                            if ((SQLSetPair != "")) {
                                // 
                                //  ----- Set the new value in the 
                                // 
                                // With...
                                if ((contentSetStore(CSPointer).ResultColumnCount > 0)) {
                                    for (ColumnPtr = 0; (ColumnPtr 
                                                <= (contentSetStore(CSPointer).ResultColumnCount - 1)); ColumnPtr++) {
                                        if (contentSetStore(CSPointer).fieldNames) {
                                            ColumnPtr = UcaseFieldName;
                                            contentSetStore(CSPointer).readCache;
                                            ColumnPtr;
                                            contentSetStore(CSPointer).readCacheRowPtr;
                                            writeCacheValue.ToString();
                                            break;
                                        }
                                        
                                    }
                                    
                                }
                                
                                if ((field.UniqueName 
                                            && (genericController.encodeText(writeCacheValue) != ""))) {
                                    // 
                                    //  ----- set up for unique name check
                                    // 
                                    if (!string.IsNullOrEmpty(SQLCriteriaUnique)) {
                                        "Or";
                                        ",";
                                    }
                                    
                                    string writeCacheValueText = genericController.encodeText(writeCacheValue);
                                    if ((writeCacheValueText.Length < 255)) {
                                        (field.nameLc + ("=\"" 
                                                    + (writeCacheValueText + "\"")));
                                    }
                                    else {
                                        (field.nameLc + ("=\"" 
                                                    + (writeCacheValueText.Substring(0, 255) + "...\"")));
                                    }
                                    
                                    switch (field.fieldTypeId) {
                                        case FieldTypeIdRedirect:
                                        case FieldTypeIdManyToMany:
                                            break;
                                        default:
                                            ("(" 
                                                        + (field.nameLc + ("=" 
                                                        + (this.EncodeSQL(writeCacheValue, field.fieldTypeId) + ")"))));
                                            break;
                                    }
                                }
                                
                                // 
                                //  ----- Live mode: update live record
                                // 
                                SQLLiveUpdate = (SQLLiveUpdate 
                                            + (SQLLiveDelimiter + SQLSetPair));
                                SQLLiveDelimiter = ",";
                                if (FieldAdminAuthorable) {
                                    AuthorableFieldUpdate = true;
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                    // 
                    //  ----- Set ModifiedBy,ModifiedDate Fields if an admin visible field has changed
                    // 
                    if (AuthorableFieldUpdate) {
                        if ((SQLLiveUpdate != "")) {
                            // 
                            //  ----- Authorable Fields Updated in non-Authoring Mode, set Live Record Modified
                            // 
                            SQLLiveUpdate = (SQLLiveUpdate + (",MODIFIEDDATE=" 
                                        + (this.encodeSQLDate(sqlModifiedDate) + (",MODIFIEDBY=" + encodeSQLNumber(sqlModifiedBy)))));
                        }
                        
                    }
                    
                    // '
                    // ' not sure why, but this section was commented out.
                    // ' Modified was not being set, so I un-commented it
                    // '
                    // If (SQLEditUpdate <> "") And (AuthorableFieldUpdate) Then
                    //     '
                    //     ' ----- set the csv_ContentSet Modified
                    //     '
                    //     Call cpCore.workflow.setRecordLocking(ContentName, LiveRecordID, AuthoringControlsModified, .OwnerMemberID)
                    // End If
                    // 
                    //  ----- Do the unique check on the content table, if necessary
                    // 
                    if ((SQLCriteriaUnique != "")) {
                        string sqlUnique = ("SELECT ID FROM " 
                                    + (LiveTableName + (" WHERE (ID<>" 
                                    + (LiveRecordID + (")AND(" 
                                    + (SQLCriteriaUnique + (")and(" 
                                    + (contentSetStore(CSPointer).CDef.ContentControlCriteria + ");"))))))));
                        Using;
                        ((DataTable)(dt)) = executeQuery(sqlUnique, LiveDataSourceName);
                        // 
                        //  -- unique violation
                        if ((dt.Rows.Count > 0)) {
                            throw new ApplicationException(("Can not save record to content [" 
                                            + (LiveRecordContentName + ("] because it would create a non-unique record for one or more of the following field(s) [" 
                                            + (UniqueViolationFieldList + "]")))));
                        }
                        
                    }
                    
                    if ((FieldFoundCount > 0)) {
                        // 
                        //  ----- update live table (non-workflowauthoring and non-authorable fields)
                        // 
                        if ((SQLLiveUpdate != "")) {
                            SQLUpdate = ("UPDATE " 
                                        + (LiveTableName + (" SET " 
                                        + (SQLLiveUpdate + (" WHERE ID=" 
                                        + (LiveRecordID + ";"))))));
                            executeQuery(SQLUpdate, LiveDataSourceName);
                        }
                        
                        // 
                        //  ----- Live record has changed
                        // 
                        if (AuthorableFieldUpdate) {
                            // 
                            //  ----- reset the ContentTimeStamp to csv_ClearBake
                            // 
                            cpCore.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(LiveTableName, "id", LiveRecordID.ToString()));
                            // 
                            //  ----- mark the record NOT UpToDate for SpiderDocs
                            // 
                            if (((LiveTableName.ToLower() == "ccpagecontent") 
                                        && (LiveRecordID != 0))) {
                                if (isSQLTableField("default", "ccSpiderDocs", "PageID")) {
                                    SQL = ("UPDATE ccspiderdocs SET UpToDate = 0 WHERE PageID=" + LiveRecordID);
                                    executeQuery(SQL);
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                    LastUsed = DateTime.Now;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // =====================================================================================================
        // '' <summary>
        // '' Initialize the csv_ContentSet Result Cache when it is first opened
        // '' </summary>
        // '' <param name="CSPointer"></param>
        // 
        private void cs_initData(int CSPointer) {
            try {
                int ColumnPtr;
                // 
                // With...
                ResultEOF = true;
                0.readCacheRowPtr = new Dictionary<string, string>();
                0.readCacheRowCnt = new Dictionary<string, string>();
                contentSetStore(CSPointer).ResultColumnCount = new Dictionary<string, string>();
                (dt.Rows.Count > 0);
                dt.Columns.Count;
                ColumnPtr = 0;
                object .;
                fieldNames(., ResultColumnCount);
                foreach (DataColumn dc in // TODO: Warning!!!! NULL EXPRESSION DETECTED...
                ) {
                    dt.Columns.fieldNames(ColumnPtr) = genericController.vbUCase(dc.ColumnName);
                    ColumnPtr = (ColumnPtr + 1);
                }
                
                //  refactor -- convert interal storage to dt and assign -- will speedup open
                (UBound(., readCache, 2) + 1.readCacheRowPtr) = 0;
                convertDataTabletoArray(., dt).readCacheRowCnt = 0;
                readCache = 0;
                writeCache = new Dictionary<string, string>();
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // =====================================================================================================
        // '' <summary>
        // '' returns tru if the dataset is pointing past the last row
        // '' </summary>
        // '' <param name="CSPointer"></param>
        // '' <returns></returns>
        // 
        private bool cs_IsEOF(int CSPointer) {
            bool returnResult = true;
            try {
                if ((CSPointer <= 0)) {
                    throw new ArgumentException("dataset is not valid");
                }
                else {
                    // With...
                    cs_IsEOF = (contentSetStore(CSPointer).readCacheRowPtr >= contentSetStore(CSPointer).readCacheRowCnt);
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnResult;
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' Encode a value for a sql
        // '' </summary>
        // '' <param name="expression"></param>
        // '' <param name="fieldType"></param>
        // '' <returns></returns>
        public string EncodeSQL(object expression, int fieldType, void =, void FieldTypeIdText) {
            string returnResult = "";
            // Warning!!! Optional parameters not supported
            try {
                switch (fieldType) {
                    case FieldTypeIdBoolean:
                        returnResult = encodeSQLBoolean(genericController.EncodeBoolean(expression));
                        break;
                    case FieldTypeIdCurrency:
                    case FieldTypeIdFloat:
                        returnResult = encodeSQLNumber(genericController.EncodeNumber(expression));
                        break;
                    case FieldTypeIdAutoIdIncrement:
                    case FieldTypeIdInteger:
                    case FieldTypeIdLookup:
                    case FieldTypeIdMemberSelect:
                        returnResult = encodeSQLNumber(genericController.EncodeInteger(expression));
                        break;
                    case FieldTypeIdDate:
                        returnResult = this.encodeSQLDate(genericController.EncodeDate(expression));
                        break;
                    case FieldTypeIdLongText:
                    case FieldTypeIdHTML:
                        returnResult = this.encodeSQLText(genericController.encodeText(expression));
                        break;
                    case FieldTypeIdFile:
                    case FieldTypeIdFileImage:
                    case FieldTypeIdLink:
                    case FieldTypeIdResourceLink:
                    case FieldTypeIdRedirect:
                    case FieldTypeIdManyToMany:
                    case FieldTypeIdText:
                    case FieldTypeIdFileText:
                    case FieldTypeIdFileJavascript:
                    case FieldTypeIdFileXML:
                    case FieldTypeIdFileCSS:
                    case FieldTypeIdFileHTML:
                        returnResult = this.encodeSQLText(genericController.encodeText(expression));
                        break;
                    default:
                        cpCore.handleException(new ApplicationException(("Unknown Field Type [" 
                                            + (fieldType + ""))));
                        returnResult = this.encodeSQLText(genericController.encodeText(expression));
                        break;
                }
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnResult;
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' return a sql compatible string. 
        // '' </summary>
        // '' <param name="expression"></param>
        // '' <returns></returns>
        public string encodeSQLText(string expression) {
            string returnResult = "";
            if ((expression == null)) {
                returnResult = "null";
            }
            else {
                returnResult = genericController.encodeText(expression);
                if ((returnResult == "")) {
                    returnResult = "null";
                }
                else {
                    returnResult = ("\'" 
                                + (genericController.vbReplace(returnResult, "\'", "\'\'") + "\'"));
                }
                
            }
            
            return returnResult;
        }
        
        public string encodeSqlTextLike(coreClass cpcore, string source) {
            return this.encodeSQLText(("%" 
                            + (source + "%")));
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // ''    encodeSQLDate
        // '' </summary>
        // '' <param name="expression"></param>
        // '' <returns></returns>
        // 
        public string encodeSQLDate(DateTime expression) {
            string returnResult = "";
            try {
                if (IsDBNull(expression)) {
                    returnResult = "null";
                }
                else {
                    DateTime expressionDate = genericController.EncodeDate(expression);
                    MinValue;
                    returnResult = "null";
                }
                
                returnResult = ("\'" 
                            + (expressionDate.Year 
                            + (("0" + expressionDate.Month).Substring((("0" + expressionDate.Month).Length - 2)) 
                            + (("0" + expressionDate.Day).Substring((("0" + expressionDate.Day).Length - 2)) + (" " 
                            + (("0" + expressionDate.Hour).Substring((("0" + expressionDate.Hour).Length - 2)) + (":" 
                            + (("0" + expressionDate.Minute).Substring((("0" + expressionDate.Minute).Length - 2)) + (":" 
                            + (("0" + expressionDate.Second).Substring((("0" + expressionDate.Second).Length - 2)) + (":" 
                            + (("00" + expressionDate.Millisecond).Substring((("00" + expressionDate.Millisecond).Length - 3)) + "\'"))))))))))));
            }
            
        }
        
        private Exception ex;
    }
}
Endclass End {
}

    
    // 
    // ========================================================================
    // '' <summary>
    // '' encodeSQLNumber
    // '' </summary>
    // '' <param name="expression"></param>
    // '' <returns></returns>
    // 
    public string encodeSQLNumber(double expression) {
        return expression.ToString;
        // Dim returnResult As String = ""
        // Try
        //     If False Then
        //         'If expression Is Nothing Then
        //         'returnResult = "null"
        //         'ElseIf VarType(expression) = vbBoolean Then
        //         '    If genericController.EncodeBoolean(expression) Then
        //         '        returnResult = SQLTrue
        //         '    Else
        //         '        returnResult = SQLFalse
        //         '    End If
        //     ElseIf Not genericController.vbIsNumeric(expression) Then
        //         returnResult = "null"
        //     Else
        //         returnResult = expression.ToString
        //     End If
        // Catch ex As Exception
        //     cpCore.handleExceptionAndContinue(ex) : Throw
        // End Try
        // Return returnResult
    }
    
    // 
    public string encodeSQLNumber(int expression) {
        return expression.ToString;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' encodeSQLBoolean
    // '' </summary>
    // '' <param name="expression"></param>
    // '' <returns></returns>
    // 
    public string encodeSQLBoolean(bool expression) {
        string returnResult = SQLFalse;
        try {
            if (expression) {
                returnResult = SQLTrue;
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnResult;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Create a filename for the Virtual Directory
    // '' </summary>
    // '' <param name="ContentName"></param>
    // '' <param name="FieldName"></param>
    // '' <param name="RecordID"></param>
    // '' <param name="OriginalFilename"></param>
    // '' <returns></returns>
    // ========================================================================
    // 
    public string GetVirtualFilename(string ContentName, string FieldName, int RecordID, string OriginalFilename, void =, void ) {
        string returnResult = "";
        // Warning!!! Optional parameters not supported
        try {
            int fieldTypeId;
            string TableName;
            // Dim iOriginalFilename As String
            Models.Complex.cdefModel CDef;
            // 
            if (string.IsNullOrEmpty(ContentName.Trim())) {
                throw new ArgumentException("contentname cannot be blank");
            }
            else if (string.IsNullOrEmpty(FieldName.Trim())) {
                throw new ArgumentException("fieldname cannot be blank");
            }
            else if ((RecordID <= 0)) {
                throw new ArgumentException("recordid is not valid");
            }
            else {
                CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
                if ((CDef.Id == 0)) {
                    throw new ApplicationException(("contentname [" 
                                    + (ContentName + "] is not a valid content")));
                }
                else {
                    TableName = CDef.ContentTableName;
                    if ((TableName == "")) {
                        TableName = ContentName;
                    }
                    
                    // 
                    // iOriginalFilename = genericController.encodeEmptyText(OriginalFilename, "")
                    // 
                    fieldTypeId = CDef.fields(FieldName.ToLower()).fieldTypeId;
                    // 
                    if ((OriginalFilename == "")) {
                        returnResult = fileController.getVirtualRecordPathFilename(TableName, FieldName, RecordID, fieldTypeId);
                    }
                    else {
                        returnResult = fileController.getVirtualRecordPathFilename(TableName, FieldName, RecordID, OriginalFilename);
                    }
                    
                }
                
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnResult;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Opens a csv_ContentSet with the Members of a group
    // '' </summary>
    // '' <param name="groupList"></param>
    // '' <param name="sqlCriteria"></param>
    // '' <param name="SortFieldList"></param>
    // '' <param name="ActiveOnly"></param>
    // '' <param name="PageSize"></param>
    // '' <param name="PageNumber"></param>
    // '' <returns></returns>
    public int csOpenGroupUsers(
                List<string> groupList, 
                string sqlCriteria, 
                void =, 
                void , 
                string SortFieldList, 
                void =, 
                void , 
                bool ActiveOnly, 
                void =, 
                void True, 
                int PageSize, 
                void =, 
                void 9999, 
                int PageNumber, 
                void =, 
                void 1) {
        int returnResult = -1;
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        try {
            DateTime rightNow = DateTime.Now;
            string sqlRightNow = this.encodeSQLDate(rightNow);
            // 
            if ((PageNumber == 0)) {
                PageNumber = 1;
            }
            
            if ((PageSize == 0)) {
                PageSize = pageSizeDefault;
            }
            
            if ((groupList.Count > 0)) {
                // 
                //  Build Inner Query to select distinct id needed
                // 
                string SQL = ("SELECT DISTINCT ccMembers.id" + (" FROM (ccMembers" + (" LEFT JOIN ccMemberRules ON ccMembers.ID = ccMemberRules.MemberID)" + (" LEFT JOIN ccGroups ON ccMemberRules.GroupID = ccGroups.ID" + " WHERE (ccMemberRules.Active<>0)AND(ccGroups.Active<>0)"))));
                if (ActiveOnly) {
                    "AND(ccMembers.Active<>0)";
                }
                
                // 
                string subQuery = "";
                foreach (string groupName in groupList) {
                    if (!string.IsNullOrEmpty(groupName.Trim)) {
                        ("or(ccGroups.Name=" 
                                    + (this.encodeSQLText(groupName.Trim) + ")"));
                    }
                    
                }
                
                if (!string.IsNullOrEmpty(subQuery)) {
                    ("and(" 
                                + (subQuery.Substring(2) + ")"));
                }
                
                // 
                //  -- group expiration
                ("and((ccMemberRules.DateExpires Is Null)or(ccMemberRules.DateExpires>" 
                            + (sqlRightNow + "))"));
                SQL = ("SELECT * from ccMembers where id in (" 
                            + (SQL + ")"));
                if ((sqlCriteria != "")) {
                    ("and(" 
                                + (sqlCriteria + ")"));
                }
                
                if ((SortFieldList != "")) {
                    (" Order by " + SortFieldList);
                }
                
                returnResult = csOpenSql_rev("default", SQL, PageSize, PageNumber);
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnResult;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Get a Contents Tableid from the ContentPointer
    // '' </summary>
    // '' <param name="ContentName"></param>
    // '' <returns></returns>
    // ========================================================================
    // 
    public int GetContentTableID(string ContentName) {
        int returnResult;
        try {
            DataTable dt = executeQuery(("select ContentTableID from ccContent where name=" + this.encodeSQLText(ContentName)));
            if (!genericController.isDataTableOk(dt)) {
                throw new ApplicationException(("Content [" 
                                + (ContentName + "] was not found in ccContent table")));
            }
            else {
                returnResult = genericController.EncodeInteger(dt.Rows[0].Item["ContentTableID"]);
            }
            
            dt.Dispose();
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnResult;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' csv_DeleteTableRecord
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <param name="RecordID"></param>
    // 
    public void deleteTableRecord(string TableName, int RecordID, string DataSourceName) {
        try {
            if (string.IsNullOrEmpty(TableName.Trim())) {
                throw new ApplicationException("tablename cannot be blank");
            }
            else if ((RecordID <= 0)) {
                throw new ApplicationException(("record id is not valid [" 
                                + (RecordID + "]")));
            }
            else {
                DeleteTableRecords(TableName, ("ID=" + RecordID), DataSourceName);
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
    }
    
    // 
    // ==================================================================================================
    // '' <summary>
    // '' Remove this record from all watch lists
    // '' </summary>
    // '' <param name="ContentID"></param>
    // '' <param name="RecordID"></param>
    // 
    public void deleteContentRules(int ContentID, int RecordID) {
        try {
            string ContentRecordKey;
            string Criteria;
            string ContentName;
            string TableName;
            // 
            //  ----- remove all ContentWatchListRules (uncheck the watch lists in admin)
            // 
            if (((ContentID <= 0) 
                        || (RecordID <= 0))) {
                // 
                throw new ApplicationException(("ContentID [" 
                                + (ContentID + ("] or RecordID [" 
                                + (RecordID + "] where blank")))));
            }
            else {
                ContentRecordKey = (ContentID.ToString() + ("." + RecordID.ToString()));
                Criteria = ("(ContentRecordKey=" 
                            + (this.encodeSQLText(ContentRecordKey) + ")"));
                ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID);
                TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName);
                // '
                // ' ----- Delete CalendarEventRules and CalendarEvents
                // '
                // If models.complex.cdefmodel.isContentFieldSupported(cpcore,"calendar events", "ID") Then
                //     Call deleteContentRecords("Calendar Events", Criteria)
                // End If
                // '
                // ' ----- Delete ContentWatch
                // '
                // CS = cs_open("Content Watch", Criteria)
                // Do While cs_ok(CS)
                //     Call cs_deleteRecord(CS)
                //     cs_goNext(CS)
                // Loop
                // Call cs_Close(CS)
                // 
                //  ----- Table Specific rules
                // 
                switch (genericController.vbUCase(TableName)) {
                    case "CCCALENDARS":
                        deleteContentRecords("Calendar Event Rules", ("CalendarID=" + RecordID));
                        break;
                    case "CCCALENDAREVENTS":
                        deleteContentRecords("Calendar Event Rules", ("CalendarEventID=" + RecordID));
                        break;
                    case "CCCONTENT":
                        deleteContentRecords("Group Rules", ("ContentID=" + RecordID));
                        break;
                    case "CCCONTENTWATCH":
                        deleteContentRecords("Content Watch List Rules", ("Contentwatchid=" + RecordID));
                        break;
                    case "CCCONTENTWATCHLISTS":
                        deleteContentRecords("Content Watch List Rules", ("Contentwatchlistid=" + RecordID));
                        break;
                    case "CCGROUPS":
                        deleteContentRecords("Group Rules", ("GroupID=" + RecordID));
                        deleteContentRecords("Library Folder Rules", ("GroupID=" + RecordID));
                        deleteContentRecords("Member Rules", ("GroupID=" + RecordID));
                        deleteContentRecords("Page Content Block Rules", ("GroupID=" + RecordID));
                        deleteContentRecords("Path Rules", ("GroupID=" + RecordID));
                        break;
                    case "CCLIBRARYFOLDERS":
                        deleteContentRecords("Library Folder Rules", ("FolderID=" + RecordID));
                        break;
                    case "CCMEMBERS":
                        deleteContentRecords("Member Rules", ("MemberID=" + RecordID));
                        deleteContentRecords("Topic Habits", ("MemberID=" + RecordID));
                        deleteContentRecords("Member Topic Rules", ("MemberID=" + RecordID));
                        break;
                    case "CCPAGECONTENT":
                        deleteContentRecords("Page Content Block Rules", ("RecordID=" + RecordID));
                        deleteContentRecords("Page Content Topic Rules", ("PageID=" + RecordID));
                        break;
                    case "CCSURVEYQUESTIONS":
                        deleteContentRecords("Survey Results", ("QuestionID=" + RecordID));
                        break;
                    case "CCSURVEYS":
                        deleteContentRecords("Survey Questions", ("SurveyID=" + RecordID));
                        break;
                    case "CCTOPICS":
                        deleteContentRecords("Topic Habits", ("TopicID=" + RecordID));
                        deleteContentRecords("Page Content Topic Rules", ("TopicID=" + RecordID));
                        deleteContentRecords("Member Topic Rules", ("TopicID=" + RecordID));
                        break;
                }
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Get the SQL value for the true state of a boolean
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <returns></returns>
    // 
    private int GetSQLTrue(string DataSourceName) {
        return 1;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' 
    // '' </summary>
    // '' <param name="CSPointer"></param>
    // '' <returns></returns>
    //  try declaring the return as object() - an array holder for variants
    //  try setting up each call to return a variant, not an array of variants
    // 
    public string[,] cs_getRows(int CSPointer) {
        string[,] returnResult;
        try {
            returnResult = contentSetStore(CSPointer).readCache;
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnResult;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' get the row count of the dataset
    // '' </summary>
    // '' <param name="CSPointer"></param>
    // '' <returns></returns>
    // 
    public int csGetRowCount(int CSPointer) {
        int returnResult;
        try {
            if (!this.csOk(CSPointer)) {
                throw new ArgumentException("dataset is not valid");
            }
            else {
                returnResult = contentSetStore(CSPointer).readCacheRowCnt;
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnResult;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Returns a 1-d array with the results from the csv_ContentSet
    // '' </summary>
    // '' <param name="CSPointer"></param>
    // '' <returns></returns>
    // 
    public string[] cs_getRowFields(int CSPointer) {
        string[] returnResult;
        try {
            if (!this.csOk(CSPointer)) {
                throw new ArgumentException("dataset is not valid");
            }
            else {
                returnResult = contentSetStore(CSPointer).fieldNames;
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnResult;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' csv_DeleteTableRecord
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <param name="Criteria"></param>
    // 
    public void DeleteTableRecords(string TableName, string Criteria, string DataSourceName) {
        try {
            if (string.IsNullOrEmpty(DataSourceName)) {
                throw new ArgumentException("dataSourceName cannot be blank");
            }
            else if (string.IsNullOrEmpty(TableName)) {
                throw new ArgumentException("TableName cannot be blank");
            }
            else if (string.IsNullOrEmpty(Criteria)) {
                throw new ArgumentException("Criteria cannot be blank");
            }
            else {
                string SQL = ("DELETE FROM " 
                            + (TableName + (" WHERE " + Criteria)));
                executeQuery(SQL, DataSourceName);
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' return the content name of a csv_ContentSet
    // '' </summary>
    // '' <param name="CSPointer"></param>
    // '' <returns></returns>
    // 
    public string cs_getContentName(int CSPointer) {
        string returnResult = "";
        try {
            if (!this.csOk(CSPointer)) {
                throw new ArgumentException("dataset is not valid");
            }
            else {
                returnResult = contentSetStore(CSPointer).ContentName;
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnResult;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Get FieldDescritor from FieldType
    // '' </summary>
    // '' <param name="fieldType"></param>
    // '' <returns></returns>
    // 
    public string getFieldTypeNameFromFieldTypeId(int fieldType) {
        string returnFieldTypeName = "";
        try {
            switch (fieldType) {
                case FieldTypeIdBoolean:
                    returnFieldTypeName = FieldTypeNameBoolean;
                    break;
                case FieldTypeIdCurrency:
                    returnFieldTypeName = FieldTypeNameCurrency;
                    break;
                case FieldTypeIdDate:
                    returnFieldTypeName = FieldTypeNameDate;
                    break;
                case FieldTypeIdFile:
                    returnFieldTypeName = FieldTypeNameFile;
                    break;
                case FieldTypeIdFloat:
                    returnFieldTypeName = FieldTypeNameFloat;
                    break;
                case FieldTypeIdFileImage:
                    returnFieldTypeName = FieldTypeNameImage;
                    break;
                case FieldTypeIdLink:
                    returnFieldTypeName = FieldTypeNameLink;
                    break;
                case FieldTypeIdResourceLink:
                    returnFieldTypeName = FieldTypeNameResourceLink;
                    break;
                case FieldTypeIdInteger:
                    returnFieldTypeName = FieldTypeNameInteger;
                    break;
                case FieldTypeIdLongText:
                    returnFieldTypeName = FieldTypeNameLongText;
                    break;
                case FieldTypeIdLookup:
                    returnFieldTypeName = FieldTypeNameLookup;
                    break;
                case FieldTypeIdMemberSelect:
                    returnFieldTypeName = FieldTypeNameMemberSelect;
                    break;
                case FieldTypeIdRedirect:
                    returnFieldTypeName = FieldTypeNameRedirect;
                    break;
                case FieldTypeIdManyToMany:
                    returnFieldTypeName = FieldTypeNameManyToMany;
                    break;
                case FieldTypeIdFileText:
                    returnFieldTypeName = FieldTypeNameTextFile;
                    break;
                case FieldTypeIdFileCSS:
                    returnFieldTypeName = FieldTypeNameCSSFile;
                    break;
                case FieldTypeIdFileXML:
                    returnFieldTypeName = FieldTypeNameXMLFile;
                    break;
                case FieldTypeIdFileJavascript:
                    returnFieldTypeName = FieldTypeNameJavascriptFile;
                    break;
                case FieldTypeIdText:
                    returnFieldTypeName = FieldTypeNameText;
                    break;
                case FieldTypeIdHTML:
                    returnFieldTypeName = FieldTypeNameHTML;
                    break;
                case FieldTypeIdFileHTML:
                    returnFieldTypeName = FieldTypeNameHTMLFile;
                    break;
                case FieldTypeIdAutoIdIncrement:
                    returnFieldTypeName = "AutoIncrement";
                    break;
                case FieldTypeIdMemberSelect:
                    returnFieldTypeName = "MemberSelect";
                    break;
                default:
                    // 
                    //  If field type is ignored, call it a text field
                    // 
                    returnFieldTypeName = FieldTypeNameText;
                    break;
            }
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnFieldTypeName;
    }
    
    // '' <summary>
    // '' 
    // '' </summary>
    // '' <returns></returns>
    public int sqlCommandTimeout {
        get {
            return _sqlTimeoutSecond;
        }
        set {
            _sqlTimeoutSecond = value;
        }
    }
    
    private int _sqlTimeoutSecond;
    
    // 
    // =============================================================
    // '' <summary>
    // '' get a record's id from its guid
    // '' </summary>
    // '' <param name="ContentName"></param>
    // '' <param name="RecordGuid"></param>
    // '' <returns></returns>
    // =============================================================
    // 
    public int GetRecordIDByGuid(string ContentName, string RecordGuid) {
        int returnResult = 0;
        try {
            if (string.IsNullOrEmpty(ContentName)) {
                throw new ArgumentException("contentname cannot be blank");
            }
            else if (string.IsNullOrEmpty(RecordGuid)) {
                throw new ArgumentException("RecordGuid cannot be blank");
            }
            else {
                int CS = csOpen(ContentName, ("ccguid=" + this.encodeSQLText(RecordGuid)), "ID", ,, ,, "ID");
                if (this.csOk(CS)) {
                    returnResult = csGetInteger(CS, "ID");
                }
                
                csClose(CS);
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnResult;
    }
    
    // 
    // ========================================================================
    // 
    // ========================================================================
    // 
    public string[,] GetContentRows(
                string ContentName, 
                string Criteria, 
                void =, 
                void , 
                string SortFieldList, 
                void =, 
                void , 
                bool ActiveOnly, 
                void =, 
                void True, 
                int MemberID, 
                void =, 
                void SystemMemberID, 
                bool WorkflowRenderingMode, 
                void =, 
                void False, 
                bool WorkflowEditingMode, 
                void =, 
                void False, 
                string SelectFieldList, 
                void =, 
                void , 
                int PageSize, 
                void =, 
                void 9999, 
                int PageNumber, 
                void =, 
                void 1) {
        string[,] returnRows;
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        try {
            // 
            int CS = csOpen(ContentName, Criteria, SortFieldList, ActiveOnly, MemberID, WorkflowRenderingMode, WorkflowEditingMode, SelectFieldList, PageSize, PageNumber);
            if (this.csOk(CS)) {
                returnRows = contentSetStore(CS).readCache;
            }
            
            csClose(CS);
            // 
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnRows;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' set the defaults in a dataset row
    // '' </summary>
    // '' <param name="CS"></param>
    // 
    public void SetCSRecordDefaults(int CS) {
        try {
            string[] lookups;
            string UCaseDefaultValueText;
            string LookupContentName;
            string FieldName;
            string DefaultValueText;
            // 
            if (!this.csOk(CS)) {
                throw new ArgumentException("dataset is not valid");
            }
            else {
                foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in contentSetStore(CS).CDef.fields) {
                    Models.Complex.CDefFieldModel field = keyValuePair.Value;
                    // With...
                    FieldName = field.nameLc;
                    if (((FieldName != "") 
                                && !string.IsNullOrEmpty(field.defaultValue))) {
                        switch (genericController.vbUCase(FieldName)) {
                            case "ID":
                            case "CCGUID":
                            case "CREATEKEY":
                            case "DATEADDED":
                            case "CREATEDBY":
                            case "CONTENTCONTROLID":
                                break;
                            case FieldTypeIdLookup:
                                // 
                                //  *******************************
                                //  This is a problem - the defaults should come in as the ID values, not the names
                                //    so a select can be added to the default configuration page
                                //  *******************************
                                // 
                                DefaultValueText = genericController.encodeText(field.defaultValue);
                                this.csSet(CS, FieldName, "null");
                                if ((DefaultValueText != "")) {
                                    if ((field.lookupContentID != 0)) {
                                        LookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, field.lookupContentID);
                                        if ((LookupContentName != "")) {
                                            this.csSet(CS, FieldName, getRecordID(LookupContentName, DefaultValueText));
                                        }
                                        
                                    }
                                    else if ((field.lookupList != "")) {
                                        UCaseDefaultValueText = genericController.vbUCase(DefaultValueText);
                                        lookups = field.lookupList.Split(",");
                                        for (Ptr = 0; (Ptr <= UBound(lookups)); Ptr++) {
                                            if ((UCaseDefaultValueText == genericController.vbUCase(lookups[Ptr]))) {
                                                this.csSet(CS, FieldName, (Ptr + 1));
                                                break;
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                }
                                
                                break;
                            default:
                                this.csSet(CS, FieldName, field.defaultValue);
                                break;
                        }
                    }
                    
                }
                
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
    }
    
    // 
    // '' <summary>
    // '' 
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="From"></param>
    // '' <param name="FieldList"></param>
    // '' <param name="Where"></param>
    // '' <param name="OrderBy"></param>
    // '' <param name="GroupBy"></param>
    // '' <param name="RecordLimit"></param>
    // '' <returns></returns>
    public string GetSQLSelect(
                string DataSourceName, 
                string From, 
                string FieldList, 
                void =, 
                void , 
                string Where, 
                void =, 
                void , 
                string OrderBy, 
                void =, 
                void , 
                string GroupBy, 
                void =, 
                void , 
                int RecordLimit, 
                void =, 
                void 0) {
        string SQL = "";
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        try {
            switch (getDataSourceType(DataSourceName)) {
                case DataSourceTypeODBCMySQL:
                    SQL = "SELECT";
                    (" " + FieldList);
                    (" FROM " + From);
                    if ((Where != "")) {
                        (" WHERE " + Where);
                    }
                    
                    if ((OrderBy != "")) {
                        (" ORDER BY " + OrderBy);
                    }
                    
                    if ((GroupBy != "")) {
                        (" GROUP BY " + GroupBy);
                    }
                    
                    if ((RecordLimit != 0)) {
                        (" LIMIT " + RecordLimit);
                    }
                    
                    break;
                default:
                    SQL = "SELECT";
                    if ((RecordLimit != 0)) {
                        (" TOP " + RecordLimit);
                    }
                    
                    if ((FieldList == "")) {
                        " *";
                    }
                    else {
                        (" " + FieldList);
                    }
                    
                    (" FROM " + From);
                    if ((Where != "")) {
                        (" WHERE " + Where);
                    }
                    
                    if ((OrderBy != "")) {
                        (" ORDER BY " + OrderBy);
                    }
                    
                    if ((GroupBy != "")) {
                        (" GROUP BY " + GroupBy);
                    }
                    
                    break;
            }
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return SQL;
    }
    
    // 
    // '' <summary>
    // '' 
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <returns></returns>
    // 
    public string getSQLIndexList(string DataSourceName, string TableName) {
        string returnList = "";
        try {
            Models.Complex.tableSchemaModel ts = Models.Complex.tableSchemaModel.getTableSchema(cpCore, TableName, DataSourceName);
            if (ts) {
                IsNot;
                null;
                foreach (string entry in ts.indexes) {
                    ("," + entry);
                }
                
                if ((returnList.Length > 0)) {
                    returnList = returnList.Substring(2);
                }
                
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnList;
    }
    
    // 
    // '' <summary>
    // '' 
    // '' </summary>
    // '' <param name="tableName"></param>
    // '' <returns></returns>
    // 
    public DataTable getTableSchemaData(string tableName) {
        DataTable returnDt = new DataTable();
        try {
            string connString = getConnectionStringADONET(cpCore.serverConfig.appConfig.name, "default");
            Using;
            ((void)(connSQL));
            new SqlConnection(connString);
            connSQL.Open();
            returnDt = connSQL.GetSchema("Tables", {, cpCore.serverConfig.appConfig.name, null, tableName, null);
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnDt;
    }
    
    // 
    // '' <summary>
    // '' 
    // '' </summary>
    // '' <param name="tableName"></param>
    // '' <returns></returns>
    // 
    public DataTable getColumnSchemaData(string tableName) {
        DataTable returnDt = new DataTable();
        try {
            if (string.IsNullOrEmpty(tableName.Trim())) {
                throw new ArgumentException("tablename cannot be blank");
            }
            else {
                string connString = getConnectionStringADONET(cpCore.serverConfig.appConfig.name, "default");
                Using;
                ((void)(connSQL));
                new SqlConnection(connString);
                connSQL.Open();
                returnDt = connSQL.GetSchema("Columns", {, cpCore.serverConfig.appConfig.name, null, tableName, null);
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnDt;
    }
    
    // 
    //  
    // 
    public DataTable getIndexSchemaData(string tableName) {
        DataTable returnDt = new DataTable();
        try {
            if (string.IsNullOrEmpty(tableName.Trim())) {
                throw new ArgumentException("tablename cannot be blank");
            }
            else {
                string connString = getConnectionStringADONET(cpCore.serverConfig.appConfig.name, "default");
                Using;
                ((void)(connSQL));
                new SqlConnection(connString);
                connSQL.Open();
                returnDt = connSQL.GetSchema("Indexes", {, cpCore.serverConfig.appConfig.name, null, tableName, null);
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnDt;
    }
    
    // 
    // =============================================================================
    // '' <summary>
    // '' get Sql Criteria for string that could be id, guid or name
    // '' </summary>
    // '' <param name="nameIdOrGuid"></param>
    // '' <returns></returns>
    public string getNameIdOrGuidSqlCriteria(string nameIdOrGuid) {
        string sqlCriteria = "";
        try {
            if (genericController.vbIsNumeric(nameIdOrGuid)) {
                sqlCriteria = ("id=" + encodeSQLNumber(double.Parse(nameIdOrGuid)));
            }
            else if (genericController.common_isGuid(nameIdOrGuid)) {
                sqlCriteria = ("ccGuid=" + this.encodeSQLText(nameIdOrGuid));
            }
            else {
                sqlCriteria = ("name=" + this.encodeSQLText(nameIdOrGuid));
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return sqlCriteria;
    }
    
    // 
    // =============================================================================
    // '' <summary>
    // '' Get a ContentID from the ContentName using just the tables
    // '' </summary>
    // '' <param name="ContentName"></param>
    // '' <returns></returns>
    private int getDbContentID(string ContentName) {
        int returnContentId = 0;
        try {
            DataTable dt = executeQuery(("Select ID from ccContent where name=" + this.encodeSQLText(ContentName)));
            if ((dt.Rows.Count > 0)) {
                returnContentId = genericController.EncodeInteger(dt.Rows[0].Item["id"]);
            }
            
            dt.Dispose();
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnContentId;
    }
    
    // 
    // =============================================================================
    // '' <summary>
    // '' Imports the named table into the content system
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <param name="ContentName"></param>
    // 
    public void createContentFromSQLTable(dataSourceModel DataSource, string TableName, string ContentName) {
        try {
            string SQL;
            DataTable dtFields;
            string DateAddedString;
            string CreateKeyString;
            int ContentID;
            // Dim DataSourceID As Integer
            bool ContentFieldFound;
            bool ContentIsNew;
            //  true if the content definition is being created
            int RecordID;
            // 
            // ----------------------------------------------------------------
            //  ----- lookup datasource ID, if default, ID is -1
            // ----------------------------------------------------------------
            // 
            // DataSourceID = cpCore.db.GetDataSourceID(DataSourceName)
            DateAddedString = cpCore.db.encodeSQLDate(Now());
            CreateKeyString = cpCore.db.encodeSQLNumber(genericController.GetRandomInteger);
            // 
            // ----------------------------------------------------------------
            //  ----- Read in a record from the table to get fields
            // ----------------------------------------------------------------
            // 
            DataTable dt = cpCore.db.openTable(DataSource.Name, TableName, "", "", ,, 1);
            if ((dt.Rows.Count == 0)) {
                dt.Dispose();
                // 
                //  --- no records were found, add a blank if we can
                // 
                dt = cpCore.db.insertTableRecordGetDataTable(DataSource.Name, TableName, cpCore.doc.authContext.user.id);
                if ((dt.Rows.Count > 0)) {
                    RecordID = genericController.EncodeInteger(dt.Rows[0].Item["ID"]);
                    cpCore.db.executeQuery(("Update " 
                                    + (TableName + (" Set active=0 where id=" 
                                    + (RecordID + ";")))), DataSource.Name);
                }
                
            }
            
            if ((dt.Rows.Count == 0)) {
                throw new ApplicationException(("Could Not add a record To table [" 
                                + (TableName + "].")));
            }
            else {
                // 
                // ----------------------------------------------------------------
                //  --- Find/Create the Content Definition
                // ----------------------------------------------------------------
                // 
                ContentID = Models.Complex.cdefModel.getContentId(cpCore, ContentName);
                if ((ContentID <= 0)) {
                    // 
                    //  ----- Content definition not found, create it
                    // 
                    ContentIsNew = true;
                    Models.Complex.cdefModel.addContent(cpCore, true, DataSource, TableName, ContentName);
                    // ContentID = csv_GetContentID(ContentName)
                    SQL = ("Select ID from ccContent where name=" + cpCore.db.encodeSQLText(ContentName));
                    dt = cpCore.db.executeQuery(SQL);
                    if ((dt.Rows.Count == 0)) {
                        throw new ApplicationException(("Content Definition [" 
                                        + (ContentName + "] could Not be selected by name after it was inserted")));
                    }
                    else {
                        ContentID = genericController.EncodeInteger(dt[0].Item["ID"]);
                        cpCore.db.executeQuery(("update ccContent Set CreateKey=0 where id=" + ContentID));
                    }
                    
                    dt.Dispose();
                    cpCore.cache.invalidateAll();
                    cpCore.doc.clearMetaData();
                }
                
                // 
                // -----------------------------------------------------------
                //  --- Create the ccFields records for the new table
                // -----------------------------------------------------------
                // 
                //  ----- locate the field in the content field table
                // 
                SQL = ("Select name from ccFields where ContentID=" 
                            + (ContentID + ";"));
                dtFields = cpCore.db.executeQuery(SQL);
                // 
                //  ----- verify all the table fields
                // 
                foreach (DataColumn dcTableColumns in dt.Columns) {
                    // 
                    //  ----- see if the field is already in the content fields
                    // 
                    string UcaseTableColumnName;
                    UcaseTableColumnName = genericController.vbUCase(dcTableColumns.ColumnName);
                    ContentFieldFound = false;
                    foreach (DataRow drContentRecords in dtFields.Rows) {
                        if ((genericController.vbUCase(genericController.encodeText(drContentRecords["name"])) == UcaseTableColumnName)) {
                            ContentFieldFound = true;
                            break;
                        }
                        
                    }
                    
                    if (!ContentFieldFound) {
                        // 
                        //  create the content field
                        // 
                        createContentFieldFromTableField(ContentName, dcTableColumns.ColumnName, genericController.EncodeInteger(dcTableColumns.DataType));
                    }
                    else {
                        // 
                        //  touch field so upgrade does not delete it
                        // 
                        cpCore.db.executeQuery(("update ccFields Set CreateKey=0 where (Contentid=" 
                                        + (ContentID + (") And (name = " 
                                        + (cpCore.db.encodeSQLText(UcaseTableColumnName) + ")")))));
                    }
                    
                }
                
            }
            
            // 
            //  Fill ContentControlID fields with new ContentID
            // 
            SQL = ("Update " 
                        + (TableName + (" Set ContentControlID=" 
                        + (ContentID + " where (ContentControlID Is null);"))));
            cpCore.db.executeQuery(SQL, DataSource.Name);
            // 
            //  ----- Load CDef
            //        Load only if the previous state of autoload was true
            //        Leave Autoload false during load so more do not trigger
            // 
            cpCore.cache.invalidateAll();
            cpCore.doc.clearMetaData();
            dt.Dispose();
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
    }
    
    //  
    // ========================================================================
    // '' <summary>
    // '' Define a Content Definition Field based only on what is known from a SQL table
    // '' </summary>
    // '' <param name="ContentName"></param>
    // '' <param name="FieldName"></param>
    // '' <param name="ADOFieldType"></param>
    public void createContentFieldFromTableField(string ContentName, string FieldName, int ADOFieldType) {
        try {
            // 
            Models.Complex.CDefFieldModel field = new Models.Complex.CDefFieldModel();
            // 
            field.fieldTypeId = cpCore.db.getFieldTypeIdByADOType(ADOFieldType);
            field.caption = FieldName;
            field.editSortPriority = 1000;
            field.ReadOnly = false;
            field.authorable = true;
            field.adminOnly = false;
            field.developerOnly = false;
            field.TextBuffered = false;
            field.htmlContent = false;
            switch (genericController.vbUCase(FieldName)) {
                case "NAME":
                    field.caption = "Name";
                    field.editSortPriority = 100;
                    break;
                case "ACTIVE":
                    field.caption = "Active";
                    field.editSortPriority = 200;
                    field.fieldTypeId = FieldTypeIdBoolean;
                    field.defaultValue = "1";
                    break;
                case "DATEADDED":
                    field.caption = "Created";
                    field.ReadOnly = true;
                    field.editSortPriority = 5020;
                    break;
                case "CREATEDBY":
                    field.caption = "Created By";
                    field.fieldTypeId = FieldTypeIdLookup;
                    field.lookupContentName(cpCore) = "Members";
                    field.ReadOnly = true;
                    field.editSortPriority = 5030;
                    break;
                case "MODIFIEDDATE":
                    field.caption = "Modified";
                    field.ReadOnly = true;
                    field.editSortPriority = 5040;
                    break;
                case "MODIFIEDBY":
                    field.caption = "Modified By";
                    field.fieldTypeId = FieldTypeIdLookup;
                    field.lookupContentName(cpCore) = "Members";
                    field.ReadOnly = true;
                    field.editSortPriority = 5050;
                    break;
                case "ID":
                    field.caption = "Number";
                    field.ReadOnly = true;
                    field.editSortPriority = 5060;
                    field.authorable = true;
                    field.adminOnly = false;
                    field.developerOnly = true;
                    break;
                case "CONTENTCONTROLID":
                    field.caption = "Content Definition";
                    field.fieldTypeId = FieldTypeIdLookup;
                    field.lookupContentName(cpCore) = "Content";
                    field.editSortPriority = 5070;
                    field.authorable = true;
                    field.ReadOnly = false;
                    field.adminOnly = true;
                    field.developerOnly = true;
                    break;
                case "CREATEKEY":
                    field.caption = "CreateKey";
                    field.ReadOnly = true;
                    field.editSortPriority = 5080;
                    field.authorable = false;
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
                    field.fieldTypeId = FieldTypeIdLookup;
                    field.lookupContentName(cpCore) = "Organizations";
                    field.editSortPriority = 2005;
                    field.authorable = true;
                    field.ReadOnly = false;
                    break;
                case "COPYFILENAME":
                    field.caption = "Copy";
                    field.fieldTypeId = FieldTypeIdFileHTML;
                    field.TextBuffered = true;
                    field.editSortPriority = 2010;
                    break;
                case "BRIEFFILENAME":
                    field.caption = "Overview";
                    field.fieldTypeId = FieldTypeIdFileHTML;
                    field.TextBuffered = true;
                    field.editSortPriority = 2020;
                    field.htmlContent = false;
                    break;
                case "IMAGEFILENAME":
                    field.caption = "Image";
                    field.fieldTypeId = FieldTypeIdFile;
                    field.editSortPriority = 2040;
                    break;
                case "THUMBNAILFILENAME":
                    field.caption = "Thumbnail";
                    field.fieldTypeId = FieldTypeIdFile;
                    field.editSortPriority = 2050;
                    break;
                case "CONTENTID":
                    field.caption = "Content";
                    field.fieldTypeId = FieldTypeIdLookup;
                    field.lookupContentName(cpCore) = "Content";
                    field.ReadOnly = false;
                    field.editSortPriority = 2060;
                    // 
                    //  --- Record Features
                    // 
                    break;
                case "PARENTID":
                    field.caption = "Parent";
                    field.fieldTypeId = FieldTypeIdLookup;
                    field.lookupContentName(cpCore) = ContentName;
                    field.ReadOnly = false;
                    field.editSortPriority = 3000;
                    break;
                case "MEMBERID":
                    field.caption = "Member";
                    field.fieldTypeId = FieldTypeIdLookup;
                    field.lookupContentName(cpCore) = "Members";
                    field.ReadOnly = false;
                    field.editSortPriority = 3005;
                    break;
                case "CONTACTMEMBERID":
                    field.caption = "Contact";
                    field.fieldTypeId = FieldTypeIdLookup;
                    field.lookupContentName(cpCore) = "Members";
                    field.ReadOnly = false;
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
                    //  --- Display only information
                    // 
                    break;
                case "VIEWINGS":
                    field.caption = "Viewings";
                    field.ReadOnly = true;
                    field.editSortPriority = 5000;
                    field.defaultValue = "0";
                    break;
                case "CLICKS":
                    field.caption = "Clicks";
                    field.ReadOnly = true;
                    field.editSortPriority = 5010;
                    field.defaultValue = "0";
                    break;
            }
            Models.Complex.cdefModel.verifyCDefField_ReturnID(cpCore, ContentName, field);
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
    }
    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' convert a dtaTable to a simple array - quick way to adapt old code
    // '' </summary>
    // '' <param name="dt"></param>
    // '' <returns></returns>
    // ====================================================================================================
    //  refqctor - do not convert datatable to array in initcs, just cache the datatable
    public string[,] convertDataTabletoArray(DataTable dt) {
        string[,] rows;
        try {
            int columnCnt;
            int rowCnt;
            int cPtr;
            int rPtr;
            // 
            //  20150717 check for no columns
            if (((dt.Rows.Count > 0) 
                        && (dt.Columns.Count > 0))) {
                columnCnt = dt.Columns.Count;
                rowCnt = dt.Rows.Count;
                //  20150717 change from rows(columnCnt,rowCnt) because other routines appear to use this count
                object rows;
                rPtr = 0;
                foreach (DataRow dr in dt.Rows) {
                    cPtr = 0;
                    foreach (DataColumn cell in dt.Columns) {
                        rows[cPtr, rPtr] = genericController.encodeText(dr[cell]);
                        cPtr++;
                    }
                    
                    rPtr++;
                }
                
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return rows;
    }
    
    // 
    // ========================================================================
    //  app.csv_DeleteTableRecord
    // ========================================================================
    // 
    public void DeleteTableRecordChunks(string DataSourceName, string TableName, string Criteria, int ChunkSize, void =, void 1000, int MaxChunkCount, void =, void 1000) {
        // 
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        int PreviousCount;
        int CurrentCount;
        int LoopCount;
        string SQL;
        int iChunkSize;
        int iChunkCount;
        // dim dt as datatable
        int DataSourceType;
        // 
        DataSourceType = getDataSourceType(DataSourceName);
        if (((DataSourceType != DataSourceTypeODBCSQLServer) 
                    && (DataSourceType != DataSourceTypeODBCAccess))) {
            // 
            //  If not SQL server, just delete them
            // 
            DeleteTableRecords(TableName, Criteria, DataSourceName);
        }
        else {
            // 
            //  ----- Clear up to date for the properties
            // 
            iChunkSize = ChunkSize;
            if ((iChunkSize == 0)) {
                iChunkSize = 1000;
            }
            
            iChunkCount = MaxChunkCount;
            if ((iChunkCount == 0)) {
                iChunkCount = 1000;
            }
            
            // 
            //  Get an initial count and allow for timeout
            // 
            PreviousCount = -1;
            LoopCount = 0;
            CurrentCount = 0;
            SQL = ("select count(*) as RecordCount from " 
                        + (TableName + (" where " + Criteria)));
            DataTable dt;
            dt = executeQuery(SQL);
            if ((dt.Rows.Count > 0)) {
                CurrentCount = genericController.EncodeInteger(dt.Rows[0].Item[0]);
            }
            
            while (((CurrentCount != 0) 
                        && ((PreviousCount != CurrentCount) 
                        && (LoopCount < iChunkCount)))) {
                if ((getDataSourceType(DataSourceName) == DataSourceTypeODBCMySQL)) {
                    SQL = ("delete from " 
                                + (TableName + (" where id in (select ID from " 
                                + (TableName + (" where " 
                                + (Criteria + (" limit " 
                                + (iChunkSize + ")"))))))));
                }
                else {
                    SQL = ("delete from " 
                                + (TableName + (" where id in (select top " 
                                + (iChunkSize + (" ID from " 
                                + (TableName + (" where " 
                                + (Criteria + ")"))))))));
                }
                
                executeQuery(SQL, DataSourceName);
                PreviousCount = CurrentCount;
                SQL = ("select count(*) as RecordCount from " 
                            + (TableName + (" where " + Criteria)));
                dt = executeQuery(SQL);
                if ((dt.Rows.Count > 0)) {
                    CurrentCount = genericController.EncodeInteger(dt.Rows[0].Item[0]);
                }
                
                LoopCount = (LoopCount + 1);
            }
            
            if (((CurrentCount != 0) 
                        && (PreviousCount == CurrentCount))) {
                // 
                //  records did not delete
                // 
                cpCore.handleException(new ApplicationException("Error deleting record chunks. No records were deleted and the process was not complete."));
            }
            else if ((LoopCount >= iChunkCount)) {
                // 
                //  records did not delete
                // 
                cpCore.handleException(new ApplicationException("Error deleting record chunks. The maximum chunk count was exceeded while deleting records."));
            }
            
        }
        
    }
    
    // 
    // =============================================================================
    //    Returns the link to the page that contains the record designated by the ContentRecordKey
    //        Returns DefaultLink if it can not be determined
    // =============================================================================
    // 
    public string main_GetLinkByContentRecordKey(string ContentRecordKey, string DefaultLink, void =, void ) {
        string result = String.Empty;
        // Warning!!! Optional parameters not supported
        try {
            int CSPointer;
            string[] KeySplit;
            int ContentID;
            int RecordID;
            string ContentName;
            int templateId;
            int ParentID;
            string DefaultTemplateLink;
            string TableName;
            string DataSource;
            int ParentContentID;
            bool recordfound;
            // 
            if ((ContentRecordKey != "")) {
                // 
                //  First try main_ContentWatch table for a link
                // 
                CSPointer = csOpen("Content Watch", ("ContentRecordKey=" + this.encodeSQLText(ContentRecordKey)), ,, ,, ,, "Link,Clicks");
                if (this.csOk(CSPointer)) {
                    result = cpCore.db.csGetText(CSPointer, "Link");
                }
                
                cpCore.db.csClose(CSPointer);
                // 
                if ((result == "")) {
                    // 
                    //  try template for this page
                    // 
                    KeySplit = ContentRecordKey.Split(".");
                    if ((UBound(KeySplit) == 1)) {
                        ContentID = genericController.EncodeInteger(KeySplit[0]);
                        if ((ContentID != 0)) {
                            ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID);
                            RecordID = genericController.EncodeInteger(KeySplit[1]);
                            if (((ContentName != "") 
                                        && (RecordID != 0))) {
                                if ((Models.Complex.cdefModel.getContentTablename(cpCore, ContentName) == "ccPageContent")) {
                                    CSPointer = cpCore.db.csOpenRecord(ContentName, RecordID, ,, "TemplateID,ParentID");
                                    if (this.csOk(CSPointer)) {
                                        recordfound = true;
                                        templateId = csGetInteger(CSPointer, "TemplateID");
                                        ParentID = csGetInteger(CSPointer, "ParentID");
                                    }
                                    
                                    csClose(CSPointer);
                                    if (!recordfound) {
                                        // 
                                        //  This content record does not exist - remove any records with this ContentRecordKey pointer
                                        // 
                                        deleteContentRecords("Content Watch", ("ContentRecordKey=" + this.encodeSQLText(ContentRecordKey)));
                                        cpCore.db.deleteContentRules(Models.Complex.cdefModel.getContentId(cpCore, ContentName), RecordID);
                                    }
                                    else {
                                        if ((templateId != 0)) {
                                            CSPointer = cpCore.db.csOpenRecord("Page Templates", templateId, ,, "Link");
                                            if (this.csOk(CSPointer)) {
                                                result = csGetText(CSPointer, "Link");
                                            }
                                            
                                            csClose(CSPointer);
                                        }
                                        
                                        if (((result == "") 
                                                    && (ParentID != 0))) {
                                            TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName);
                                            DataSource = Models.Complex.cdefModel.getContentDataSource(cpCore, ContentName);
                                            CSPointer = csOpenSql_rev(DataSource, ("Select ContentControlID from " 
                                                            + (TableName + (" where ID=" + RecordID))));
                                            if (this.csOk(CSPointer)) {
                                                ParentContentID = genericController.EncodeInteger(csGetText(CSPointer, "ContentControlID"));
                                            }
                                            
                                            csClose(CSPointer);
                                            if ((ParentContentID != 0)) {
                                                result = main_GetLinkByContentRecordKey(((ParentContentID + ("." + ParentID))).ToString(), "");
                                            }
                                            
                                        }
                                        
                                        if ((result == "")) {
                                            DefaultTemplateLink = cpCore.siteProperties.getText("SectionLandingLink", (requestAppRootPath + cpCore.siteProperties.serverPageDefault));
                                        }
                                        
                                    }
                                    
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                    if ((result != "")) {
                        result = genericController.modifyLinkQuery(result, rnPageId, RecordID.ToString(), true);
                    }
                    
                }
                
            }
            
            // 
            if ((result == "")) {
                result = DefaultLink;
            }
            
            // 
            result = genericController.EncodeAppRootPath(result, cpCore.webServer.requestVirtualFilePath, requestAppRootPath, cpCore.webServer.requestDomain);
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
        }
        
        return result;
    }
    
    // 
    // ============================================================================
    // 
    public static string encodeSqlTableName(string sourceName) {
        string returnName = "";
        const string FirstCharSafeString = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string SafeString = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_@#";
        try {
            string src;
            string TestChr;
            int Ptr = 0;
            // 
            //  remove nonsafe URL characters
            // 
            src = sourceName;
            returnName = "";
            while ((Ptr < src.Length)) {
                TestChr = src.Substring(Ptr, 1);
                Ptr++;
                if ((FirstCharSafeString.IndexOf(TestChr) >= 0)) {
                    TestChr;
                    break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                }
                
            }
            
            //  non-first character
            while ((Ptr < src.Length)) {
                TestChr = src.Substring(Ptr, 1);
                Ptr++;
                if ((SafeString.IndexOf(TestChr) >= 0)) {
                    TestChr;
                }
                
            }
            
        }
        catch (Exception ex) {
            //  shared method, rethrow error
            throw new ApplicationException(("Exception in encodeSqlTableName(" 
                            + (sourceName + ")")), ex);
        }
        
        return returnName;
    }
    
    // 
    // 
    // 
    public int GetTableID(string TableName) {
        int result = 0;
        int CS;
        GetTableID = -1;
        CS = cpCore.db.csOpenSql(("Select ID from ccTables where name=" + cpCore.db.encodeSQLText(TableName)), ,, 1);
        if (cpCore.db.csOk(CS)) {
            result = cpCore.db.csGetInteger(CS, "ID");
        }
        
        cpCore.db.csClose(CS);
        return result;
    }
    
    // 
    // ========================================================================
    //  Opens a Content Definition into a ContentSEt
    //    Returns and integer that points into the ContentSet array
    //    If there was a problem, it returns -1
    // 
    //    If authoring mode, as group of records are returned.
    //        The first is the current edit record
    //        The rest are the archive records.
    // ========================================================================
    // 
    public int csOpenRecord(string ContentName, int RecordID, bool WorkflowAuthoringMode, void =, void False, bool WorkflowEditingMode, void =, void False, string SelectFieldList, void =, void ) {
        return csOpen(genericController.encodeText(ContentName), ("(ID=" 
                        + (cpCore.db.encodeSQLNumber(RecordID) + ")")), ,, false, cpCore.doc.authContext.user.id, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1);
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
    }
    
    // 
    // ========================================================================
    // 
    public int cs_open2(string ContentName, int RecordID, bool WorkflowAuthoringMode, void =, void False, bool WorkflowEditingMode, void =, void False, string SelectFieldList, void =, void ) {
        return csOpen(ContentName, ("(ID=" 
                        + (cpCore.db.encodeSQLNumber(RecordID) + ")")), ,, false, cpCore.doc.authContext.user.id, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1);
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
    }
    
    // 
    // ========================================================================
    // 
    public void SetContentCopy(string CopyName, string Content) {
        // 
        int CS;
        string iCopyName;
        string iContent;
        const object ContentName = "Copy Content";
        if (false) {
            // .3.210" Then
            throw new Exception(("Contensive database was created with version " 
                            + (cpCore.siteProperties.dataBuildVersion + ". main_SetContentCopy requires an builder.")));
        }
        else {
            iCopyName = genericController.encodeText(CopyName);
            iContent = genericController.encodeText(Content);
            CS = csOpen(ContentName, ("name=" + this.encodeSQLText(iCopyName)));
            if (!this.csOk(CS)) {
                csClose(CS);
                CS = this.csInsertRecord(ContentName);
            }
            
            if (this.csOk(CS)) {
                this.csSet(CS, "name", iCopyName);
                this.csSet(CS, "Copy", iContent);
            }
            
            csClose(CS);
        }
        
    }
    
    public string csGetRecordEditLink(int CSPointer, object AllowCut, void =, void False) {
        string result = "";
        // Warning!!! Optional parameters not supported
        string RecordName;
        string ContentName;
        int RecordID;
        int ContentControlID;
        int iCSPointer;
        // 
        iCSPointer = genericController.EncodeInteger(CSPointer);
        if ((iCSPointer == -1)) {
            throw new ApplicationException("main_cs_getRecordEditLink called with invalid iCSPointer");
        }
        else if (!cpCore.db.csOk(iCSPointer)) {
            throw new ApplicationException("main_cs_getRecordEditLink called with Not main_CSOK");
        }
        else {
            // 
            //  Print an edit link for the records Content (may not be iCSPointer content)
            // 
            RecordID = cpCore.db.csGetInteger(iCSPointer, "ID");
            RecordName = cpCore.db.csGetText(iCSPointer, "Name");
            ContentControlID = cpCore.db.csGetInteger(iCSPointer, "contentcontrolid");
            ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentControlID);
            if ((ContentName != "")) {
                result = cpCore.html.main_GetRecordEditLink2(ContentName, RecordID, genericController.EncodeBoolean(AllowCut), RecordName, cpCore.doc.authContext.isEditing(ContentName));
            }
            
        }
        
        return result;
    }