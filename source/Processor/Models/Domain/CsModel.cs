
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
using Contensive.BaseClasses;
using System.Data;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Exceptions;
using static Contensive.Processor.Constants;
using static Contensive.Processor.Controllers.GenericController;
//
namespace Contensive.Processor {
    //
    //==========================================================================================
    /// <summary>
    /// A layer on top of DbController that accesses data using the 'content' metadata
    /// </summary>
    public class CsModel : IDisposable {
        /// <summary>
        /// dependencies
        /// </summary>
        private CoreController core;
        /// <summary>
        /// The user that opened this set
        /// </summary>
        private int openingMemberID;
        /// <summary>
        /// ? same as openingMemberId (ID of the member who opened the csv_ContentSet)
        /// </summary>
        public int OwnerMemberID;
        /// <summary>
        /// If true, it is in use
        /// </summary>
        public bool IsOpen;
        /// <summary>
        /// The date/time this csv_ContentSet was last used
        /// </summary>
        public DateTime LastUsed;
        /// <summary>
        /// Can be used to write to the record. True if opened with a content definition.
        /// </summary>
        public bool writeable;
        /// <summary>
        /// Can be read. True if created with open() or openSql(), false if created with openForUpdate()
        /// </summary>
        public bool readable;
        /// <summary>
        /// true if it was created here
        /// </summary>
        public bool newRecord;
        /// <summary>
        /// ***** should be removed. This should be the same as metaData.name
        /// </summary>
        public string ContentName;
        /// <summary>
        /// If opened with a content name, this is that content's metadata
        /// </summary>
        public Models.Domain.ContentMetaDomainModel CDef;
        /// <summary>
        /// data that needs to be written to the database on the next save
        /// </summary>
        public Dictionary<string, string> writeCache;
        /// <summary>
        /// Set when CS is opened and if a save happens
        /// </summary>
        public bool isModified;
        /// <summary>
        /// The read object, null when empty otherwise it needs to be disposed
        /// </summary>
        public DataTable dt = null;
        /// <summary>
        /// Holds the SQL that created the result set
        /// </summary>
        public string Source;
        /// <summary>
        /// The Datasource of the SQL that created the result set
        /// </summary>
        public string DataSource;
        /// <summary>
        /// Number of records in a cache page
        /// </summary>
        public int PageSize;
        /// <summary>
        /// The Page that this result starts with
        /// </summary>
        public int PageNumber;
        /// <summary>
        /// 
        /// </summary>
        public int fieldPointer;
        /// <summary>
        /// 1-D array of the result field names
        /// </summary>
        public string[] fieldNames;
        /// <summary>
        /// number of columns in the fieldNames and readCache
        /// </summary>
        public int resultColumnCount;
        /// <summary>
        /// readCache is at the last record
        /// </summary>
        public bool resultEOF;
        /// <summary>
        /// 2-D array of the result rows/columns
        /// </summary>
        public string[,] readCache;
        /// <summary>
        /// number of rows in the readCache
        /// </summary>
        public int readCacheRowCnt;
        /// <summary>
        /// Pointer to the current result row, first row is 0, BOF is -1
        /// </summary>
        public int readCacheRowPtr;
        /// <summary>
        /// comma delimited list of all fields selected, in the form table.field
        /// </summary>
        public string SelectTableFieldList;
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="core"></param>
        public CsModel(CoreController core) {
            this.core = core;
            //
            // -- capture userId at the time data opened
            openingMemberID = core.session.user.id;
        }
        //
        //====================================================================================================
        /// <summary>
        /// close the read data set, save what needs to be written
        /// </summary>
        /// <param name="asyncSave"></param>
        public void csClose(bool asyncSave) {
            try {
                if (IsOpen) {
                    csSave(asyncSave);
                    readCache = new string[,] { { }, { } };
                    writeCache = new Dictionary<string, string>();
                    resultColumnCount = 0;
                    readCacheRowCnt = 0;
                    readCacheRowPtr = -1;
                    resultEOF = true;
                    IsOpen = false;
                    if (dt != null) {
                        dt.Dispose();
                        dt = null;
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        public void csClose() => csClose(false);
        //
        public void csClose(int ignore, bool asyncSave) => csClose(asyncSave);
        //
        public void csClose(int ignore) => csClose(false);
        //
        //====================================================================================================
        /// <summary>
        /// Copy the current dataset to a target set. Target must be already opened or inserted
        /// </summary>
        /// <param name="CSSource"></param>
        /// <param name="CSDestination"></param>
        public void csCopyRecord(CsModel csDestination) {
            try {
                if (!IsOpen) { throw new ArgumentException("source dataset is not valid"); }
                if (!csDestination.IsOpen) { throw new ArgumentException("destination dataset is not valid"); }
                if (CDef == null) { throw new ArgumentException("copyRecord requires the source and destination datasets be created from an open or insert, not a sql."); }
                if (csDestination.CDef == null) { throw new ArgumentException("copyRecord requires the source and destination datasets be created from an open or insert, not a sql."); }
                //
                foreach ( var kvp in CDef.fields) {
                    CDefFieldModel field = kvp.Value;
                    switch (GenericController.vbUCase(field.nameLc)) {
                        case "ID":
                            break;
                        default:
                            //
                            // ----- fields to copy
                            //
                            
                            switch (field.fieldTypeId) {
                                case Constants._fieldTypeIdRedirect:
                                case Constants._fieldTypeIdManyToMany:
                                    break;
                                case Constants._fieldTypeIdFile:
                                case Constants._fieldTypeIdFileImage:
                                case Constants._fieldTypeIdFileCSS:
                                case Constants._fieldTypeIdFileXML:
                                case Constants._fieldTypeIdFileJavascript:
                                    //
                                    // ----- cdn file
                                    {
                                        string SourceFilename = getFieldFilename(0, field.nameLc, "", CDef.name, field.fieldTypeId);
                                        if (!string.IsNullOrEmpty(SourceFilename)) {
                                            string DestFilename = csDestination.getFieldFilename(0, field.nameLc, "", csDestination.ContentName, field.fieldTypeId);
                                            csDestination.csSet(0, field.nameLc, DestFilename);
                                            core.cdnFiles.copyFile(SourceFilename, DestFilename);
                                        }
                                    }
                                    break;
                                case Constants._fieldTypeIdFileText:
                                case Constants._fieldTypeIdFileHTML:
                                    //
                                    // ----- private file
                                    //
                                    {
                                        string SourceFilename = getFieldFilename(0, field.nameLc, "", CDef.name, field.fieldTypeId);
                                        if (!string.IsNullOrEmpty(SourceFilename)) {
                                            string DestFilename = csDestination.getFieldFilename(0, field.nameLc, "", csDestination.ContentName, field.fieldTypeId);
                                            csDestination.csSet(0, field.nameLc, DestFilename);
                                            core.privateFiles.copyFile(SourceFilename, DestFilename);
                                        }
                                    }
                                    break;
                                default:
                                    //
                                    // ----- value
                                    //
                                    csDestination.csSet(0, field.nameLc, csGetValue(0, field.nameLc));
                                    break;
                            }
                            break;
                    }
                }

            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// csv_DeleteCSRecord
        /// </summary>
        /// <param name="ignore"></param>
        public void csDeleteRecord(int ignore) {
            try {
                if (!csOk(0)) { throw new ArgumentException("csv_ContentSet Is empty Or at End-Of-file"); }

                if (!(writeable)) { throw new ArgumentException("Dataset is not writeable."); }

                {
                    int ContentID = CDef.id;
                    string ContentName = CDef.name;
                    string ContentTableName = CDef.tableName;
                    string ContentDataSourceName = CDef.dataSourceName;
                    if (string.IsNullOrEmpty(ContentName)) { throw new ArgumentException("csv_ContentSet Is Not based On a Content Definition"); }
                    {
                        int LiveRecordID = csGetInteger(0, "ID");
                        //
                        // delete any files (if filename is part of select)
                        string fieldName = null;
                        Models.Domain.CDefFieldModel field = null;
                        foreach (var selectedFieldName in CDef.selectList) {
                            if (CDef.fields.ContainsKey(selectedFieldName.ToLowerInvariant())) {
                                field = CDef.fields[selectedFieldName.ToLowerInvariant()];
                                fieldName = field.nameLc;
                                string Filename = null;
                                switch (field.fieldTypeId) {
                                    case Constants._fieldTypeIdFile:
                                    case Constants._fieldTypeIdFileImage:
                                    case Constants._fieldTypeIdFileCSS:
                                    case Constants._fieldTypeIdFileJavascript:
                                    case Constants._fieldTypeIdFileXML:
                                        //
                                        // public content files
                                        //
                                        Filename = csGetText(0, fieldName);
                                        if (!string.IsNullOrEmpty(Filename)) {
                                            core.cdnFiles.deleteFile(Filename);
                                            //Call core.cdnFiles.deleteFile(core.cdnFiles.joinPath(core.appConfig.cdnFilesNetprefix, Filename))
                                        }
                                        break;
                                    case Constants._fieldTypeIdFileText:
                                    case Constants._fieldTypeIdFileHTML:
                                        //
                                        // private files
                                        //
                                        Filename = csGetText(0, fieldName);
                                        if (!string.IsNullOrEmpty(Filename)) {
                                            core.cdnFiles.deleteFile(Filename);
                                        }
                                        break;
                                }
                            }
                        }
                        //
                        core.db.deleteTableRecord(LiveRecordID, ContentTableName, ContentDataSourceName);
                        //
                        ContentMetaController.deleteContentRules(core, ContentID, LiveRecordID);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Inserts a record in a content definition and returns a csv_ContentSet with just that record
        /// If there was a problem, it returns -1
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public bool csInsert(string ContentName) {
            try {
                if (IsOpen) { csClose(); }
                if (string.IsNullOrEmpty(ContentName.Trim())) { throw new ArgumentException("ContentName cannot be blank"); }
                {
                    var CDef = ContentMetaDomainModel.createByUniqueName(core, ContentName);
                    if (CDef == null) { throw new GenericException("content [" + ContentName + "] could Not be found."); }
                    if (CDef.id <= 0) { throw new GenericException("content [" + ContentName + "] could Not be found."); }
                    //
                    // create default record in Live table
                    SqlFieldListClass sqlList = new SqlFieldListClass();
                    foreach (KeyValuePair<string, Models.Domain.CDefFieldModel> keyValuePair in CDef.fields) {
                        CDefFieldModel field = keyValuePair.Value;
                        if ((!string.IsNullOrEmpty(field.nameLc)) && (!string.IsNullOrEmpty(field.defaultValue))) {
                            switch (GenericController.vbUCase(field.nameLc)) {
                                case "CREATEKEY":
                                case "DATEADDED":
                                case "CREATEDBY":
                                case "CONTENTCONTROLID":
                                case "ID":
                                    //
                                    // Block control fields
                                    break;
                                default:
                                    switch (field.fieldTypeId) {
                                        case Constants._fieldTypeIdAutoIdIncrement:
                                            //
                                            // cannot insert an autoincremnt
                                            break;
                                        case Constants._fieldTypeIdRedirect:
                                        case Constants._fieldTypeIdManyToMany:
                                            //
                                            // ignore these fields, they have no associated DB field
                                            break;
                                        case Constants._fieldTypeIdBoolean:
                                            sqlList.add(field.nameLc, DbController.encodeSQLBoolean(GenericController.encodeBoolean(field.defaultValue)));
                                            break;
                                        case Constants._fieldTypeIdCurrency:
                                        case Constants._fieldTypeIdFloat:
                                            sqlList.add(field.nameLc, DbController.encodeSQLNumber(GenericController.encodeNumber(field.defaultValue)));
                                            break;
                                        case Constants._fieldTypeIdInteger:
                                        case Constants._fieldTypeIdMemberSelect:
                                            sqlList.add(field.nameLc, DbController.encodeSQLNumber(GenericController.encodeInteger(field.defaultValue)));
                                            break;
                                        case Constants._fieldTypeIdDate:
                                            sqlList.add(field.nameLc, DbController.encodeSQLDate(GenericController.encodeDate(field.defaultValue)));
                                            break;
                                        case Constants._fieldTypeIdLookup:
                                            //
                                            // refactor --
                                            // This is a problem - the defaults should come in as the ID values, not the names
                                            //   so a select can be added to the default configuration page
                                            //
                                            string DefaultValueText = GenericController.encodeText(field.defaultValue);
                                            if (string.IsNullOrEmpty(DefaultValueText)) {
                                                DefaultValueText = "null";
                                            } else {
                                                if (field.lookupContentID != 0) {
                                                    string LookupContentName = ContentMetaController.getContentNameByID(core, field.lookupContentID);
                                                    if (!string.IsNullOrEmpty(LookupContentName)) {
                                                        DefaultValueText = ContentMetaController.getRecordIdByUniqueName(core, LookupContentName, DefaultValueText).ToString();
                                                    }
                                                } else if (field.lookupList != "") {
                                                    string UCaseDefaultValueText = GenericController.vbUCase(DefaultValueText);
                                                    string[] lookups = field.lookupList.Split(',');

                                                    int Ptr = 0;
                                                    for (Ptr = 0; Ptr <= lookups.GetUpperBound(0); Ptr++) {
                                                        if (UCaseDefaultValueText == GenericController.vbUCase(lookups[Ptr])) {
                                                            DefaultValueText = (Ptr + 1).ToString();
                                                        }
                                                    }
                                                }
                                            }
                                            sqlList.add(field.nameLc, DefaultValueText);
                                            break;
                                        default:
                                            //
                                            // else text
                                            //
                                            sqlList.add(field.nameLc, DbController.encodeSQLText(field.defaultValue));
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                    //
                    string sqlGuid = DbController.encodeSQLText(GenericController.getGUID());
                    string sqlDateAdded = DbController.encodeSQLDate(DateTime.Now);
                    sqlList.add("ccguid", sqlGuid);
                    sqlList.add("DATEADDED", sqlDateAdded);
                    sqlList.add("CONTENTCONTROLID", DbController.encodeSQLNumber(CDef.id));
                    sqlList.add("CREATEDBY", DbController.encodeSQLNumber(openingMemberID));
                    core.db.insertTableRecord(CDef.dataSourceName, CDef.tableName, sqlList);
                    //
                    // ----- Get the record back so we can use the ID
                    return csOpen(ContentName, "(ccguid=" + sqlGuid + ")And(DateAdded=" + sqlDateAdded + ")", "ID DESC", false, openingMemberID);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        public bool getBoolean(string fieldName) {
            return csGetBoolean(0, fieldName);
        }
        //
        //====================================================================================================
        public DateTime getDate(string fieldName) {
            return csGetDate(0, fieldName);
        }
        //
        //====================================================================================================
        public int getInteger(string fieldName) {
            return csGetInteger(0, fieldName);
        }
        //
        //====================================================================================================
        public double getNumber(string fieldName) {
            return csGetNumber(0, fieldName);
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the text value stored in the field. For Lookup fields, this method returns the name of the foreign key record.
        /// For textFile fields, this method returns the filename.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string getText(string fieldName) {
            string result = "";
            try {
                result = csGet(0, fieldName);
                if (result == null) {
                    result = "";
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        public void goNext() {
            csGoNext(0);
        }
        //
        //====================================================================================================
        public bool ok() {
            return csOk(0);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, DateTime FieldValue) {
            csSet(0, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, bool FieldValue) {
            csSet(0, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, string FieldValue) {
            csSet(0, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        public void setField(string FieldName, double FieldValue) {
            csSet(0, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field. If the field is backed by a file, the value will be saved to the file
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, int FieldValue) {
            csSet(0, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// if the field uses an underlying filename, use this method to set that filename. The content for the field will switch to that contained by the new file
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="filename"></param>
        public void setFieldFilename( string fieldName, string filename ) {
            csSetFieldFilename(0, fieldName, filename);
        }
        //
        //========================================================================
        //
        public string getTextEncoded(string FieldName) {
            string ContentName = "";
            int RecordID = 0;
            if (csIsFieldSupported(0, "id") & csIsFieldSupported(0, "contentcontrolId")) {
                RecordID = csGetInteger(0, "id");
                ContentName = ContentMetaController.getContentNameByID(core, csGetInteger(0, "contentcontrolId"));
            }
            string source = csGet(0, FieldName);
            return ActiveContentController.renderHtmlForWeb(core, source, ContentName, RecordID, core.session.user.id, "", 0, CPUtilsBaseClass.addonContext.ContextPage);
        }
        //
        //========================================================================
        //
        public string getValue(string FieldName) {
            string result = "";
            try {
                result = csGetValue(0, FieldName);
                if (result == null) {
                    result = "";
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// initialize a cs
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        private void csInit(int ignore) {
            IsOpen = true;
            newRecord = true;
            ContentName = "";
            CDef = null;
            DataSource = "";
            dt = null;
            fieldNames = null;
            fieldPointer = 0;
            isModified = false;
            LastUsed = DateTime.Now;
            OwnerMemberID = core.session.user.id;
            PageNumber = 0;
            PageSize = 0;
            readCache = null;
            readCacheRowCnt = 0;
            readCacheRowPtr = 0;
            resultColumnCount = 0;
            resultEOF = true;
            SelectTableFieldList = "";
            Source = "";
            writeable = false;
            writeCache = null;
        }
        //
        private void csInit() => csInit(0);
        //
        //========================================================================
        //
        public void csGoNext(int ignore, bool AsyncSave = false) {
            try {
                if (!csOk(0)) {
                    //
                    throw new GenericException("Dataset is not valid.");
                } else if (!this.readable) {
                    //
                    // -- if not readable, cannot move rows
                    throw new GenericException("Cannot move to next row because dataset is not readable.");
                } else {
                    csSave(0, AsyncSave);
                    this.writeCache = new Dictionary<string, string>();
                    //
                    // Move to next row
                    this.readCacheRowPtr = this.readCacheRowPtr + 1;
                    //if (!csEOF(CSPointer)) {
                    //    //
                    //    // Not EOF, Set Workflow Edit Mode from Request and EditLock state
                    //    if (this.writeable) {
                    //        WorkflowController.setEditLock(core, tableid, RecordID, this.OwnerMemberID);


                    //        ContentName = this.ContentName;
                    //        RecordID = csGetInteger(CSPointer, "ID");
                    //        if (!WorkflowController.isRecordLocked(ContentName, RecordID, this.OwnerMemberID)) {
                    //        }
                    //    }
                    //}
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        //
        public void csGoFirst(int CSPointer, bool AsyncSave = false) {
            try {
                csSave(CSPointer, AsyncSave);
                this.writeCache = new Dictionary<string, string>();
                this.readCacheRowPtr = 0;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// The value read directly from the field in the Db, or from the write cache.
        /// For textFilenames, this is the filename of the content.
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string csGetValue(string fieldName) {
            string returnValue = "";
            try {
                string fieldNameTrim = fieldName.Trim();
                if (!csOk()) {
                    throw new GenericException("Attempt To GetValue fieldname[" + fieldNameTrim + "], but the dataset Is empty Or does not point To a valid row");
                } else {
                    
                    bool fieldFound = false;
                    if (this.writeCache.Count > 0) {
                        //
                        // ----- something has been set in buffer, check it first
                        if (this.writeCache.ContainsKey(fieldNameTrim.ToLowerInvariant())) {
                            returnValue = this.writeCache[fieldNameTrim.ToLowerInvariant()];
                            fieldFound = true;
                        }
                    }
                    if (!fieldFound) {
                        //
                        // ----- attempt read from readCache
                        if (!this.readable) {
                            //
                            // -- reading from write-only returns default value, because save there is legacy code that detects change bycomparing value to read cache
                            returnValue = "";
                            //throw new GenericException("Cannot read from a dataset opened write-only.");
                        } else if (this.dt == null) {
                            throw new GenericException("Cannot read from a dataset because the data is not valid.");
                        } else {
                            if (!this.dt.Columns.Contains(fieldNameTrim.ToLowerInvariant())) {
                                if (this.writeable) {
                                    var dtFieldList = new List<string>();
                                    foreach (DataColumn column in this.dt.Columns) dtFieldList.Add(column.ColumnName);
                                    throw new GenericException("Field [" + fieldNameTrim + "] was not found in [" + this.ContentName + "] with selected fields [" + String.Join(",", dtFieldList.ToArray()) + "]");
                                } else {
                                    throw new GenericException("Field [" + fieldNameTrim + "] was not found in sql [" + this.Source + "]");
                                }
                            } else {
                                returnValue = GenericController.encodeText(this.dt.Rows[this.readCacheRowPtr][fieldNameTrim.ToLowerInvariant()]);
                            }
                        }
                    }
                    this.LastUsed = DateTime.Now;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnValue;
        }
        //
        public string csGetValue(int ignore, string fieldName) => csGetValue(fieldName);
        //
        //========================================================================
        /// <summary>
        /// get the first fieldname in the CS, Returns null if there are no more
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        public string csGetFirstFieldName(int CSPointer) {
            string returnFieldName = "";
            try {
                if (!csOk(CSPointer)) {
                    throw new GenericException("data set is not valid");
                } else {
                    this.fieldPointer = 0;
                    returnFieldName = csGetNextFieldName(CSPointer);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnFieldName;
        }
        //
        //========================================================================
        /// <summary>
        /// get the next fieldname in the CS, Returns null if there are no more
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        public string csGetNextFieldName(int CSPointer) {
            string returnFieldName = "";
            try {
                if (!csOk(CSPointer)) {
                    throw new GenericException("data set is not valid");
                } else {
                    while ((string.IsNullOrEmpty(returnFieldName)) && (this.fieldPointer < this.resultColumnCount)) {
                        returnFieldName = this.fieldNames[this.fieldPointer];
                        this.fieldPointer = this.fieldPointer + 1;
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnFieldName;
        }
        //
        //========================================================================
        /// <summary>
        /// get the type of a field within a csv_ContentSet
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public int csGetFieldTypeId(int CSPointer, string FieldName) {
            int returnFieldTypeid = 0;
            try {
                if (csOk(CSPointer)) {
                    if (this.writeable) {
                        if (!string.IsNullOrEmpty(this.CDef.name)) {
                            returnFieldTypeid = this.CDef.fields[FieldName.ToLowerInvariant()].fieldTypeId;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnFieldTypeid;
        }
        //
        //========================================================================
        /// <summary>
        /// get the caption of a field within a csv_ContentSet
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public string csGetFieldCaption(int CSPointer, string FieldName) {
            string returnResult = "";
            try {
                if (csOk(CSPointer)) {
                    if (this.writeable) {
                        if (!string.IsNullOrEmpty(this.CDef.name)) {
                            returnResult = this.CDef.fields[FieldName.ToLowerInvariant()].caption;
                            if (string.IsNullOrEmpty(returnResult)) {
                                returnResult = FieldName;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        /// <summary>
        /// get a list of captions of fields within a data set
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        public string csGetSelectFieldList(int CSPointer) {
            string returnResult = "";
            try {
                if (csOk(CSPointer)) {
                    returnResult = string.Join(",", this.fieldNames);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        /// <summary>
        /// get the caption of a field within a csv_ContentSet
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public bool csIsFieldSupported(int CSPointer, string FieldName) {
            bool returnResult = false;
            try {
                if (string.IsNullOrEmpty(FieldName)) {
                    throw new ArgumentException("Field name cannot be blank");
                } else if (!csOk(CSPointer)) {
                    throw new ArgumentException("dataset is not valid");
                } else {
                    string CSSelectFieldList = csGetSelectFieldList(CSPointer);
                    returnResult = GenericController.isInDelimitedString(CSSelectFieldList, FieldName, ",");
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        /// <summary>
        /// get the filename that backs the field specified. only valid for fields of TextFile and File type.
        /// Attempt to read the filename from the field
        /// if no filename, attempt to create it from the tablename-recordid
        /// if no recordid, create filename from a random
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <param name="FieldName"></param>
        /// <param name="OriginalFilename"></param>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        public string getFieldFilename(int ignore, string FieldName, string OriginalFilename, string ContentName = "", int fieldTypeId = 0) {
            string returnFilename = "";
            try {
                string TableName = null;
                int RecordID = 0;
                string fieldNameUpper = null;
                int LenOriginalFilename = 0;
                int LenFilename = 0;
                int Pos = 0;
                //
                if (!csOk(0)) {
                    throw new ArgumentException("the current data set is not valid.");
                } else if (string.IsNullOrEmpty(FieldName)) {
                    throw new ArgumentException("Fieldname Is blank");
                } else {
                    fieldNameUpper = GenericController.vbUCase(FieldName.Trim(' '));
                    returnFilename = csGetValue(0, fieldNameUpper);
                    if (!string.IsNullOrEmpty(returnFilename)) {
                        //
                        // ----- A filename came from the record
                        //
                        if (!string.IsNullOrEmpty(OriginalFilename)) {
                            //
                            // ----- there was an original filename, make sure it matches the one in the record
                            //
                            LenOriginalFilename = OriginalFilename.Length;
                            LenFilename = returnFilename.Length;
                            Pos = (1 + LenFilename - LenOriginalFilename);
                            if (Pos <= 0) {
                                //
                                // Original Filename changed, create a new 
                                //
                                returnFilename = "";
                            } else if (returnFilename.Substring(Pos - 1) != OriginalFilename) {
                                //
                                // Original Filename changed, create a new 
                                //
                                returnFilename = "";
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(returnFilename)) {
                        //
                        // ----- no filename present, get id field
                        if (this.resultColumnCount > 0) {
                            for (var FieldPointer = 0; FieldPointer < this.resultColumnCount; FieldPointer++) {
                                if (GenericController.vbUCase(this.fieldNames[FieldPointer]) == "ID") {
                                    RecordID = csGetInteger(0, "ID");
                                    break;
                                }
                            }
                        }
                        //
                        // ----- Get tablename
                        //
                        if (this.writeable) {
                            //
                            // Get tablename from Content Definition
                            //
                            ContentName = this.CDef.name;
                            TableName = this.CDef.tableName;
                        } else if (!string.IsNullOrEmpty(ContentName)) {
                            //
                            // CS is SQL-based, use the contentname
                            //
                            TableName = ContentMetaController.getContentTablename(core, ContentName);
                        } else {
                            //
                            // no Contentname given
                            //
                            throw new GenericException("Can Not create a filename because no ContentName was given, And the csv_ContentSet Is SQL-based.");
                        }
                        //
                        // ----- Create filename
                        //
                        if (fieldTypeId == 0) {
                            if (string.IsNullOrEmpty(ContentName)) {
                                if (string.IsNullOrEmpty(OriginalFilename)) {
                                    fieldTypeId = Constants.fieldTypeIdText;
                                } else {
                                    fieldTypeId = Constants.fieldTypeIdFile;
                                }
                            } else if (this.writeable) {
                                //
                                // -- get from cdef
                                fieldTypeId = this.CDef.fields[FieldName.ToLowerInvariant()].fieldTypeId;
                            } else {
                                //
                                // -- else assume text
                                if (string.IsNullOrEmpty(OriginalFilename)) {
                                    fieldTypeId = Constants.fieldTypeIdText;
                                } else {
                                    fieldTypeId = Constants.fieldTypeIdFile;
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(OriginalFilename)) {
                            returnFilename = FileController.getVirtualRecordUnixPathFilename(TableName, FieldName, RecordID, fieldTypeId);
                        } else {
                            returnFilename = FileController.getVirtualRecordUnixPathFilename(TableName, FieldName, RecordID, OriginalFilename);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnFilename;
        }
        //
        //====================================================================================================
        //
        public string csGetText(string FieldName) {
            return csGetValue(FieldName);
        }
        //
        public string csGetText(int CSPointer, string FieldName) => csGetText(FieldName);
        //
        //====================================================================================================
        //
        public int csGetInteger(string FieldName) {
            return encodeInteger(csGetValue(FieldName));
        }
        public int csGetInteger(int CSPointer, string FieldName) => csGetInteger(FieldName);
        //
        //====================================================================================================
        //
        public double csGetNumber(string FieldName) {
            return encodeNumber(csGetValue(FieldName));
        }
        //
        public double csGetNumber(int CSPointer, string FieldName) => csGetNumber(FieldName);
        //
        //====================================================================================================
        //
        public DateTime csGetDate(string FieldName) {
            return GenericController.encodeDate(csGetValue(FieldName));
        }
        //
        public DateTime csGetDate(int CSPointer, string FieldName) => csGetDate(FieldName);
        //
        //====================================================================================================
        //
        public bool csGetBoolean(string FieldName) {
            return encodeBoolean(csGetValue(FieldName));
        }
        //
        public bool csGetBoolean(int CSPointer, string FieldName) => csGetBoolean(FieldName);
        //
        //====================================================================================================
        //
        public string csGetLookup(string FieldName) {
            return csGet(0,FieldName);
        }
        //
        public string csGetLookup(int CSPointer, string FieldName) => csGetLookup(FieldName);
        //
        //====================================================================================================
        /// <summary>
        /// if the field uses an underlying filename, use this method to set that filename. The content for the field will switch to that contained by the new file
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="FieldName"></param>
        /// <param name="filename"></param>
        public void csSetFieldFilename(string FieldName, string filename) {
            try {
                if (!csOk()) { throw new ArgumentException("dataset is not valid"); }
                if (string.IsNullOrEmpty(FieldName)) { throw new ArgumentException("fieldName cannot be blank"); }
                if (!this.writeable) { throw new GenericException("Cannot set fields for a dataset based on a query."); }
                if (this.CDef == null) { throw new GenericException("Cannot set fields for a dataset based on a query."); }
                if (this.CDef.fields == null) { throw new GenericException("The dataset contains no fields."); }
                if (!this.CDef.fields.ContainsKey(FieldName.ToLowerInvariant())) { throw new GenericException("The dataset does not contain the field specified [" + FieldName.ToLowerInvariant() + "]."); }
                if (this.writeCache.ContainsKey(FieldName.ToLowerInvariant())) {
                    this.writeCache[FieldName.ToLowerInvariant()] = filename;
                    return;
                }
                this.writeCache.Add(FieldName.ToLowerInvariant(), filename);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        public void csSetFieldFilename(int ignore, string FieldName, string filename) => csSetFieldFilename(FieldName, filename);
        //
        //====================================================================================
        // Set a csv_ContentSet Field value for a TextFile fieldtype
        //   Saves the value in a file and saves the filename in the field
        //
        //   CSPointer   The current Content Set Pointer
        //   FieldName   The name of the field to be saved
        //   Copy        Literal string to be saved in the field
        //   ContentName Contentname for the field to be saved
        //====================================================================================
        //
        public void csSetTextFile(int ignore, string FieldName, string Copy, string ContentName) {
            try {
                if (!csOk(ignore)) { throw new ArgumentException("dataset is not valid"); }
                if (string.IsNullOrEmpty(FieldName)) { throw new ArgumentException("fieldName cannot be blank"); }
                if (string.IsNullOrEmpty(ContentName)) { throw new ArgumentException("contentName cannot be blank"); }
                if (!this.writeable) { throw new GenericException("Cannot save this dataset because it is read-only."); }
                string OldFilename = csGetText(ignore, FieldName);
                string Filename = getFieldFilename(ignore, FieldName, "", ContentName, Constants.fieldTypeIdFileText);
                if (OldFilename != Filename) {
                    //
                    // Filename changed, mark record changed
                    //
                    core.cdnFiles.saveFile(Filename, Copy);
                    csSet(ignore, FieldName, Filename);
                } else {
                    string OldCopy = core.cdnFiles.readFileText(Filename);
                    if (OldCopy != Copy) {
                        //
                        // copy changed, mark record changed
                        //
                        core.cdnFiles.saveFile(Filename, Copy);
                        csSet(ignore, FieldName, Filename);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// true if csPointer is a valid dataset, and currently points to a valid row
        /// </summary>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public bool csOk() {
            try {
                //
                // -- opened with openForUpdate. can be written but not read
                if (this.writeable & !this.readable) { return this.IsOpen; }
                //
                // -- normal open
                return this.IsOpen & (this.readCacheRowPtr >= 0) && (this.readCacheRowPtr < this.readCacheRowCnt);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //       
        public bool csOk(int ignore) => csOk();
        //       
        //========================================================================
        /// <summary>
        /// Returns the Source for the csv_ContentSet
        /// </summary>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public string csGetSql(int ignore) {
            try {
                if (!csOk(0)) { throw new ArgumentException("the dataset is not valid"); }
                return this.Source;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Returns the value of a field, decoded into a text string result, if there is a problem, null is returned, this may be because the lookup record is inactive, so its not an error
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        //
        public string csGet(int ignore, string FieldName) {
            string fieldValue = "";
            try {
                if (!csOk(ignore)) { throw new ArgumentException("the dataset is not valid"); }
                if (string.IsNullOrEmpty(FieldName.Trim())) { throw new ArgumentException("fieldname cannot be blank"); }
                {
                    if (!this.writeable) {
                        //
                        // -- Not updateable, Just return what is there as a string
                        return csGetValue(ignore, FieldName);
                    } else {
                        //
                        // -- Updateable, interpret the value
                        if (!this.CDef.fields.ContainsKey(FieldName.ToLowerInvariant())) {
                            try {
                                fieldValue = GenericController.encodeText(csGetValue(ignore, FieldName));
                            } catch (Exception ex) {
                                throw new GenericException("Error [" + ex.Message + "] reading field [" + FieldName.ToLowerInvariant() + "] In content [" + this.CDef.name + "] With custom field list [" + this.SelectTableFieldList + "");
                            }
                        } else {
                            var field = this.CDef.fields[FieldName.ToLowerInvariant()];
                            int fieldTypeId = field.fieldTypeId;
                            if (fieldTypeId == Constants.fieldTypeIdManyToMany) {
                                //
                                // special case - recordset contains no data - return record id list
                                //
                                int RecordID = 0;
                                string DbTable = null;
                                string ContentName = null;
                                string SQL = null;
                                if (this.CDef.fields.ContainsKey("id")) {
                                    RecordID = GenericController.encodeInteger(csGetValue(ignore, "id"));
                                    ContentName = ContentMetaController.getContentNameByID(core, field.manyToManyRuleContentID);
                                    DbTable = ContentMetaController.getContentTablename(core, ContentName);
                                    using (DataTable dt = core.db.executeQuery("Select " + field.ManyToManyRuleSecondaryField + " from " + DbTable + " where " + field.ManyToManyRulePrimaryField + "=" + RecordID)) {
                                        if (DbController.isDataTableOk(dt)) {
                                            foreach (DataRow dr in dt.Rows) {
                                                fieldValue += "," + dr[0].ToString();
                                            }
                                            fieldValue = fieldValue.Substring(1);
                                        }
                                    }
                                }
                            } else if (fieldTypeId == fieldTypeIdRedirect) {
                                //
                                // special case - recordset contains no data - return blank
                                //
                                //fieldTypeId = Constants.fieldTypeId;
                            } else {
                                object FieldValueVariant = csGetValue(ignore, FieldName);
                                if (!GenericController.IsNull(FieldValueVariant)) {
                                    //
                                    // Field is good
                                    //
                                    switch (fieldTypeId) {
                                        case Constants._fieldTypeIdBoolean:
                                            //
                                            //
                                            //
                                            if (GenericController.encodeBoolean(FieldValueVariant)) {
                                                fieldValue = "Yes";
                                            } else {
                                                fieldValue = "No";
                                            }
                                            //NeedsHTMLEncode = False
                                            break;
                                        case Constants._fieldTypeIdDate:
                                            //
                                            //
                                            //
                                            if (GenericController.IsDate(FieldValueVariant)) {
                                                //
                                                // formatdatetime returns 'wednesday june 5, 1990', which fails IsDate()!!
                                                //
                                                fieldValue = GenericController.encodeDate(FieldValueVariant).ToString();
                                            }
                                            break;
                                        case Constants._fieldTypeIdLookup:
                                            //
                                            //
                                            //
                                            if (FieldValueVariant.IsNumeric()) {
                                                int fieldLookupId = field.lookupContentID;
                                                string LookupContentName = ContentMetaController.getContentNameByID(core, fieldLookupId);
                                                string LookupList = field.lookupList;
                                                if (!string.IsNullOrEmpty(LookupContentName)) {
                                                    //
                                                    // -- First try Lookup Content
                                                    using (var cs = new CsModel(core)) {
                                                        if (cs.csOpen(LookupContentName, "ID=" + DbController.encodeSQLNumber(GenericController.encodeInteger(FieldValueVariant)), "", true, 0, "name", 1)) {
                                                            fieldValue = cs.csGetText("name");
                                                        }
                                                    }
                                                } else if (!string.IsNullOrEmpty(LookupList)) {
                                                    //
                                                    // -- Next try lookup list
                                                    int FieldValueInteger = GenericController.encodeInteger(FieldValueVariant) - 1;
                                                    if (FieldValueInteger >= 0) {
                                                        string[] lookups = LookupList.Split(',');
                                                        if (lookups.GetUpperBound(0) >= FieldValueInteger) {
                                                            fieldValue = lookups[FieldValueInteger];
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        case Constants._fieldTypeIdMemberSelect:
                                            //
                                            //
                                            //
                                            if (FieldValueVariant.IsNumeric()) {
                                                fieldValue = ContentMetaController.getRecordName(core, "people", GenericController.encodeInteger(FieldValueVariant));
                                            }
                                            break;
                                        case Constants._fieldTypeIdCurrency:
                                            //
                                            //
                                            //
                                            if (FieldValueVariant.IsNumeric()) {
                                                fieldValue = FieldValueVariant.ToString();
                                            }
                                            break;
                                        case Constants._fieldTypeIdFileText:
                                        case Constants._fieldTypeIdFileHTML:
                                            //
                                            //
                                            //
                                            fieldValue = core.cdnFiles.readFileText(GenericController.encodeText(FieldValueVariant));
                                            break;
                                        case Constants._fieldTypeIdFileCSS:
                                        case Constants._fieldTypeIdFileXML:
                                        case Constants._fieldTypeIdFileJavascript:
                                            //
                                            //
                                            //
                                            fieldValue = core.cdnFiles.readFileText(GenericController.encodeText(FieldValueVariant));
                                            //NeedsHTMLEncode = False
                                            break;
                                        case Constants._fieldTypeIdText:
                                        case Constants._fieldTypeIdLongText:
                                        case Constants._fieldTypeIdHTML:
                                            //
                                            //
                                            //
                                            fieldValue = GenericController.encodeText(FieldValueVariant);
                                            break;
                                        case Constants._fieldTypeIdFile:
                                        case Constants._fieldTypeIdFileImage:
                                        case Constants._fieldTypeIdLink:
                                        case Constants._fieldTypeIdResourceLink:
                                        case Constants._fieldTypeIdAutoIdIncrement:
                                        case Constants._fieldTypeIdFloat:
                                        case Constants._fieldTypeIdInteger:
                                            //
                                            //
                                            //
                                            fieldValue = GenericController.encodeText(FieldValueVariant);
                                            //NeedsHTMLEncode = False
                                            break;
                                        case Constants._fieldTypeIdRedirect:
                                        case Constants._fieldTypeIdManyToMany:
                                            //
                                            // This case is covered before the select - but leave this here as safety net
                                            //
                                            //NeedsHTMLEncode = False
                                            break;
                                        default:
                                            //
                                            // Unknown field type
                                            //
                                            throw new GenericException("Can Not use field [" + FieldName + "] because the FieldType [" + fieldTypeId + "] Is invalid.");
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return fieldValue;
        }
        //
        //========================================================================
        /// <summary>
        /// Saves the value for the field. If the field uses a file, the content is saved to the file using the fields filename. To set a file-based field's filename, use setFieldFilename
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        //
        public void csSet(int ignore, string FieldName, string FieldValue) {
            try {
                string BlankTest = null;
                string FieldNameLc = null;
                bool SetNeeded = false;
                string fileNameNoExt = null;
                string ContentName = null;
                string fileName = null;
                string pathFilenameOriginal = null;
                //
                if (!csOk(ignore)) {
                    throw new ArgumentException("dataset is not valid or End-Of-file.");
                } else if (string.IsNullOrEmpty(FieldName.Trim())) {
                    throw new ArgumentException("fieldName cannnot be blank");
                } else {
                    if (!this.writeable) {
                        throw new GenericException("Cannot update a contentset created from a sql query.");
                    } else {
                        ContentName = this.ContentName;
                        FieldNameLc = FieldName.Trim(' ').ToLowerInvariant();
                        if (FieldValue == null) {
                            FieldValue = "";
                        }
                        if (!string.IsNullOrEmpty(this.CDef.name)) {
                            Models.Domain.CDefFieldModel field = null;
                            if (!this.CDef.fields.ContainsKey(FieldNameLc)) {
                                throw new ArgumentException("The field [" + FieldName + "] could Not be found In content [" + this.CDef.name + "]");
                            } else {
                                field = this.CDef.fields[FieldNameLc];
                                switch (field.fieldTypeId) {
                                    case Constants._fieldTypeIdAutoIdIncrement:
                                    case Constants._fieldTypeIdRedirect:
                                    case Constants._fieldTypeIdManyToMany:
                                        //
                                        // Never set
                                        //
                                        break;
                                    case Constants._fieldTypeIdFile:
                                    case Constants._fieldTypeIdFileImage:
                                        //
                                        // Always set
                                        // Saved in the field is the filename to the file
                                        SetNeeded = true;
                                        break;
                                    case Constants._fieldTypeIdFileText:
                                    case Constants._fieldTypeIdFileHTML:
                                    //
                                    //fileNameNoExt = csGetText(CSPointer, FieldNameLc);
                                    ////FieldValue = genericController.encodeText(FieldValueVariantLocal)
                                    //if (string.IsNullOrEmpty(FieldValue)) {
                                    //    if (!string.IsNullOrEmpty(fileNameNoExt)) {
                                    //        core.cdnFiles.deleteFile(fileNameNoExt);
                                    //        //Call publicFiles.DeleteFile(fileNameNoExt)
                                    //        fileNameNoExt = "";
                                    //    }
                                    //} else {
                                    //    if (string.IsNullOrEmpty(fileNameNoExt)) {
                                    //        fileNameNoExt = getFieldFilename(CSPointer, FieldName, "", ContentName, field.fieldTypeId);
                                    //    }
                                    //    core.cdnFiles.saveFile(fileNameNoExt, FieldValue);
                                    //    //Call publicFiles.SaveFile(fileNameNoExt, FieldValue)
                                    //}
                                    //FieldValue = fileNameNoExt;
                                    //SetNeeded = true;
                                    //break;
                                    case Constants._fieldTypeIdFileCSS:
                                    case Constants._fieldTypeIdFileXML:
                                    case Constants._fieldTypeIdFileJavascript:
                                        //
                                        // public files - save as FieldTypeTextFile except if only white space, consider it blank
                                        //
                                        string PathFilename = null;
                                        string FileExt = null;
                                        int FilenameRev = 0;
                                        string path = null;
                                        int Pos = 0;
                                        pathFilenameOriginal = csGetText(ignore, FieldNameLc);
                                        PathFilename = pathFilenameOriginal;
                                        BlankTest = FieldValue;
                                        BlankTest = GenericController.vbReplace(BlankTest, " ", "");
                                        BlankTest = GenericController.vbReplace(BlankTest, "\r", "");
                                        BlankTest = GenericController.vbReplace(BlankTest, "\n", "");
                                        BlankTest = GenericController.vbReplace(BlankTest, "\t", "");
                                        if (string.IsNullOrEmpty(BlankTest)) {
                                            if (!string.IsNullOrEmpty(PathFilename)) {
                                                core.cdnFiles.deleteFile(PathFilename);
                                                PathFilename = "";
                                            }
                                        } else {
                                            if (string.IsNullOrEmpty(PathFilename)) {
                                                PathFilename = getFieldFilename(ignore, FieldNameLc, "", ContentName, field.fieldTypeId);
                                            }
                                            if (PathFilename.Left(1) == "/") {
                                                //
                                                // root file, do not include revision
                                                //
                                            } else {
                                                //
                                                // content file, add a revision to the filename
                                                //
                                                Pos = PathFilename.LastIndexOf(".") + 1;
                                                if (Pos > 0) {
                                                    FileExt = PathFilename.Substring(Pos);
                                                    fileNameNoExt = PathFilename.Left(Pos - 1);
                                                    Pos = fileNameNoExt.LastIndexOf("/") + 1;
                                                    if (Pos > 0) {
                                                        //path = PathFilename
                                                        fileNameNoExt = fileNameNoExt.Substring(Pos);
                                                        path = PathFilename.Left(Pos);
                                                        FilenameRev = 1;
                                                        if (!fileNameNoExt.IsNumeric()) {
                                                            Pos = GenericController.vbInstr(1, fileNameNoExt, ".r", 1);
                                                            if (Pos > 0) {
                                                                FilenameRev = GenericController.encodeInteger(fileNameNoExt.Substring(Pos + 1));
                                                                FilenameRev = FilenameRev + 1;
                                                                fileNameNoExt = fileNameNoExt.Left(Pos - 1);
                                                            }
                                                        }
                                                        fileName = fileNameNoExt + ".r" + FilenameRev + "." + FileExt;
                                                        //PathFilename = PathFilename & dstFilename
                                                        path = GenericController.convertCdnUrlToCdnPathFilename(path);
                                                        //srcSysFile = config.physicalFilePath & genericController.vbReplace(srcPathFilename, "/", "\")
                                                        //dstSysFile = config.physicalFilePath & genericController.vbReplace(PathFilename, "/", "\")
                                                        PathFilename = path + fileName;
                                                        //Call publicFiles.renameFile(pathFilenameOriginal, fileName)
                                                    }
                                                }
                                            }
                                            if ((!string.IsNullOrEmpty(pathFilenameOriginal)) && (pathFilenameOriginal != PathFilename)) {
                                                pathFilenameOriginal = GenericController.convertCdnUrlToCdnPathFilename(pathFilenameOriginal);
                                                core.cdnFiles.deleteFile(pathFilenameOriginal);
                                            }
                                            core.cdnFiles.saveFile(PathFilename, FieldValue);
                                        }
                                        FieldValue = PathFilename;
                                        SetNeeded = true;
                                        break;
                                    case Constants._fieldTypeIdBoolean:
                                        //
                                        // Boolean - sepcial case, block on typed GetAlways set
                                        if (GenericController.encodeBoolean(FieldValue) != csGetBoolean(ignore, FieldNameLc)) {
                                            SetNeeded = true;
                                        }
                                        break;
                                    case Constants._fieldTypeIdText:
                                        //
                                        // Set if text of value changes
                                        //
                                        if (GenericController.encodeText(FieldValue) != csGetText(ignore, FieldNameLc)) {
                                            SetNeeded = true;
                                            if (FieldValue.Length > 255) {
                                                LogController.handleError(core, new GenericException("Text length too long saving field [" + FieldName + "], length [" + FieldValue.Length + "], but max for Text field is 255. Save will be attempted"));
                                            }
                                        }
                                        break;
                                    case Constants._fieldTypeIdLongText:
                                    case Constants._fieldTypeIdHTML:
                                        //
                                        // Set if text of value changes
                                        //
                                        if (GenericController.encodeText(FieldValue) != csGetText(ignore, FieldNameLc)) {
                                            SetNeeded = true;
                                            if (FieldValue.Length > 65535) {
                                                LogController.handleError(core, new GenericException("Text length too long saving field [" + FieldName + "], length [" + FieldValue.Length + "], but max for LongText and Html is 65535. Save will be attempted"));
                                            }
                                        }
                                        break;
                                    default:
                                        //
                                        // Set if text of value changes
                                        //
                                        if (GenericController.encodeText(FieldValue) != csGetText(ignore, FieldNameLc)) {
                                            SetNeeded = true;
                                        }
                                        break;
                                }
                            }
                        }
                        if (!SetNeeded) {
                            //SetNeeded = SetNeeded;
                        } else {
                            //
                            // ----- set the new value into the row buffer
                            //
                            if (this.writeCache.ContainsKey(FieldNameLc)) {
                                this.writeCache[FieldNameLc] = FieldValue.ToString();
                            } else {
                                this.writeCache.Add(FieldNameLc, FieldValue.ToString());
                            }
                            this.LastUsed = DateTime.Now;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        //
        public void csSet(int CSPointer, string FieldName, DateTime FieldValue) {
            csSet(CSPointer, FieldName, FieldValue.ToString());
        }
        //
        //========================================================================
        //
        public void csSet(int CSPointer, string FieldName, bool FieldValue) {
            csSet(CSPointer, FieldName, FieldValue.ToString());
        }
        //
        //========================================================================
        //
        public void csSet(int CSPointer, string FieldName, int FieldValue) {
            csSet(CSPointer, FieldName, FieldValue.ToString());
        }
        //
        //========================================================================
        //
        public void csSet(int CSPointer, string FieldName, double FieldValue) {
            csSet(CSPointer, FieldName, FieldValue.ToString());
        }
        //
        //========================================================================
        /// <summary>
        /// rollback, or undo the changes to the current row
        /// </summary>
        /// <param name="CSPointer"></param>
        public void csRollBack(int CSPointer) {
            try {
                if (!csOk(CSPointer)) {
                    throw new ArgumentException("dataset is not valid");
                } else {
                    this.writeCache.Clear();
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Save the current CS Cache back to the database
        /// </summary>
        /// <param name="csPtr"></param>
        /// <param name="asyncSave"></param>
        /// <param name="Blockcsv_ClearBake"></param>
        public void csSave(int csPtr, bool asyncSave, bool Blockcsv_ClearBake) {
            try {
                if (!csOk(csPtr)) {
                    //
                    // -- already closed or not opened or not on a current row. No error so you can always call save(), it skips if nothing to save
                } else if (this.writeCache.Count == 0) {
                    //
                    // -- nothing to write, just exit
                } else if (!(this.writeable)) {
                    //
                    // -- dataset not updatable
                    throw new ArgumentException("The dataset cannot be updated because it was created with a query and not a content table.");
                } else {
                    var contentSet = this;
                    if (this.CDef == null) {
                        //
                        // -- dataset not updatable
                        throw new ArgumentException("The dataset cannot be updated because it was not created from a valid content table.");
                    } else {
                        //
                        // -- get id from read cache or write cache. if id=0 save is insert, else save is update
                        int id = csGetInteger(csPtr, "ID");
                        //bool recordInactive = !csGetBoolean(csPtr, "ACTIVE");
                        string sqlDelimiter = "";
                        string sqlUpdate = "";
                        DateTime sqlModifiedDate = DateTime.Now;
                        int sqlModifiedBy = this.OwnerMemberID;
                        bool AuthorableFieldUpdate = false;
                        int FieldFoundCount = 0;
                        string SQLCriteriaUnique = "";
                        string UniqueViolationFieldList = "";
                        foreach (var keyValuePair in this.writeCache) {
                            string fieldName = keyValuePair.Key;
                            string ucaseFieldName = GenericController.vbUCase(fieldName);
                            object writeCacheValue = keyValuePair.Value;
                            if (ucaseFieldName == "ID") {
                                //
                                // do not add to update, it is hardcoded to update where clause
                            } else if (ucaseFieldName == "MODIFIEDBY") {
                                //
                                // capture and block it - it is hardcoded in sql
                                //
                                AuthorableFieldUpdate = true;
                                sqlModifiedBy = GenericController.encodeInteger(writeCacheValue);
                            } else if (ucaseFieldName == "MODIFIEDDATE") {
                                //
                                // capture and block it - it is hardcoded in sql
                                //
                                AuthorableFieldUpdate = true;
                                sqlModifiedDate = GenericController.encodeDate(writeCacheValue);
                            } else {
                                //
                                // let these field be added to the sql
                                //
                                //recordInactive = (ucaseFieldName == "ACTIVE" && (!GenericController.encodeBoolean(writeCacheValue)));
                                FieldFoundCount += 1;
                                Models.Domain.CDefFieldModel field = this.CDef.fields[fieldName.ToLowerInvariant()];
                                string SQLSetPair = "";
                                bool FieldReadOnly = field.readOnly;
                                bool FieldAdminAuthorable = ((!field.readOnly) && (!field.notEditable) && (field.authorable));
                                //
                                // ----- Set SQLSetPair to the name=value pair for the SQL statement
                                //
                                switch (field.fieldTypeId) {
                                    case Constants._fieldTypeIdRedirect:
                                    case Constants._fieldTypeIdManyToMany:
                                        break;
                                    case Constants._fieldTypeIdInteger:
                                    case Constants._fieldTypeIdLookup:
                                    case Constants._fieldTypeIdAutoIdIncrement:
                                    case Constants._fieldTypeIdMemberSelect:
                                        SQLSetPair = fieldName + "=" + DbController.encodeSQLNumber(encodeInteger(writeCacheValue));
                                        break;
                                    case Constants._fieldTypeIdCurrency:
                                    case Constants._fieldTypeIdFloat:
                                        SQLSetPair = fieldName + "=" + DbController.encodeSQLNumber(encodeNumber(writeCacheValue));
                                        break;
                                    case Constants._fieldTypeIdBoolean:
                                        SQLSetPair = fieldName + "=" + DbController.encodeSQLBoolean(encodeBoolean(writeCacheValue));
                                        break;
                                    case Constants._fieldTypeIdDate:
                                        SQLSetPair = fieldName + "=" + DbController.encodeSQLDate(encodeDate(writeCacheValue));
                                        break;
                                    case Constants._fieldTypeIdText:
                                        string Copy = encodeText(writeCacheValue);
                                        if (Copy.Length > 255) {
                                            Copy = Copy.Left(255);
                                        }
                                        if (field.Scramble) {
                                            Copy = TextScramble(core, Copy);
                                        }
                                        SQLSetPair = fieldName + "=" + DbController.encodeSQLText(Copy);
                                        break;
                                    case Constants._fieldTypeIdLink:
                                    case Constants._fieldTypeIdResourceLink:
                                    case Constants._fieldTypeIdFile:
                                    case Constants._fieldTypeIdFileImage:
                                    case Constants._fieldTypeIdFileText:
                                    case Constants._fieldTypeIdFileCSS:
                                    case Constants._fieldTypeIdFileXML:
                                    case Constants._fieldTypeIdFileJavascript:
                                    case Constants._fieldTypeIdFileHTML:
                                        string filename = encodeText(writeCacheValue);
                                        if (filename.Length > 255) {
                                            filename = filename.Left(255);
                                        }
                                        SQLSetPair = fieldName + "=" + DbController.encodeSQLText(filename);
                                        break;
                                    case Constants._fieldTypeIdLongText:
                                    case Constants._fieldTypeIdHTML:
                                        SQLSetPair = fieldName + "=" + DbController.encodeSQLText(GenericController.encodeText(writeCacheValue));
                                        break;
                                    default:
                                        //
                                        // Invalid fieldtype
                                        //
                                        throw new GenericException("Can Not save this record because the field [" + field.nameLc + "] has an invalid field type Id [" + field.fieldTypeId + "]");
                                }
                                if (!string.IsNullOrEmpty(SQLSetPair)) {
                                    //
                                    // ----- Set the new value in the 
                                    //
                                    if (this.resultColumnCount > 0) {
                                        for (int ColumnPtr = 0; ColumnPtr < this.resultColumnCount; ColumnPtr++) {
                                            if (this.fieldNames[ColumnPtr] == ucaseFieldName) {
                                                this.readCache[ColumnPtr, this.readCacheRowPtr] = writeCacheValue.ToString();
                                                break;
                                            }
                                        }
                                    }
                                    if (field.uniqueName & (GenericController.encodeText(writeCacheValue) != "")) {
                                        //
                                        // ----- set up for unique name check
                                        //
                                        if (!string.IsNullOrEmpty(SQLCriteriaUnique)) {
                                            SQLCriteriaUnique += "Or";
                                            UniqueViolationFieldList += ",";
                                        }
                                        string writeCacheValueText = GenericController.encodeText(writeCacheValue);
                                        if (writeCacheValueText.Length < 255) {
                                            UniqueViolationFieldList += field.nameLc + "=\"" + writeCacheValueText + "\"";
                                        } else {
                                            UniqueViolationFieldList += field.nameLc + "=\"" + writeCacheValueText.Left(255) + "...\"";
                                        }
                                        switch (field.fieldTypeId) {
                                            case Constants._fieldTypeIdRedirect:
                                            case Constants._fieldTypeIdManyToMany:
                                                break;
                                            default:
                                                SQLCriteriaUnique += "(" + field.nameLc + "=" + ContentMetaController.encodeSQL(writeCacheValue, field.fieldTypeId) + ")";
                                                break;
                                        }
                                    }
                                    //
                                    // ----- update live record
                                    //
                                    sqlUpdate = sqlUpdate + sqlDelimiter + SQLSetPair;
                                    sqlDelimiter = ",";
                                    if (FieldAdminAuthorable) {
                                        AuthorableFieldUpdate = true;
                                    }
                                }
                            }
                        }
                        //
                        // -- clear write cache
                        // 20180314 - no, dont cleare the write cache for now because a subsequent read will replace the original read's value, which may be updated by the save
                        //this.writeCache = new Dictionary<string, string>();
                        //
                        // ----- Set ModifiedBy,ModifiedDate Fields if an admin visible field has changed
                        //
                        if (AuthorableFieldUpdate) {
                            if (!string.IsNullOrEmpty(sqlUpdate)) {
                                //
                                // ----- Authorable Fields Updated in non-Authoring Mode, set Live Record Modified
                                //
                                sqlUpdate = sqlUpdate + ",MODIFIEDDATE=" + DbController.encodeSQLDate(sqlModifiedDate) + ",MODIFIEDBY=" + DbController.encodeSQLNumber(sqlModifiedBy);
                            }
                        }
                        //
                        // ----- Do the unique check on the content table, if necessary
                        //
                        if (!string.IsNullOrEmpty(SQLCriteriaUnique)) {
                            string sqlUnique = "SELECT ID FROM " + this.CDef.tableName + " WHERE (ID<>" + id + ")AND(" + SQLCriteriaUnique + ")and(" + this.CDef.legacyContentControlCriteria + ");";
                            using (DataTable dt = core.db.executeQuery(sqlUnique, this.CDef.dataSourceName)) {
                                //
                                // -- unique violation
                                if (dt.Rows.Count > 0) {
                                    LogController.logWarn(core, "Can not save record to content [" + this.CDef.name + "] because it would create a non-unique record for one or more of the following field(s) [" + UniqueViolationFieldList + "]");
                                    return;
                                }
                            }
                        }
                        if (FieldFoundCount > 0) {
                            //
                            // ----- update live table (non-workflowauthoring and non-authorable fields)
                            //
                            if (!string.IsNullOrEmpty(sqlUpdate)) {
                                string SQLUpdate = "UPDATE " + this.CDef.tableName + " SET " + sqlUpdate + " WHERE ID=" + id + ";";
                                if (asyncSave) {
                                    core.db.executeNonQueryAsync(SQLUpdate, this.CDef.dataSourceName);
                                } else {
                                    core.db.executeNonQuery(SQLUpdate, this.CDef.dataSourceName);
                                }
                            }
                        }
                        this.LastUsed = DateTime.Now;
                        //
                        // -- invalidate the special cache name used to detect a change in any record
                        core.cache.invalidateDbRecord(id, this.CDef.tableName, this.CDef.dataSourceName);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        public void csSave(int ignore, bool asyncSave) => csSave(0, asyncSave, false);
        //
        public void csSave(int ignore) => csSave(0, false, false);
        //
        public void csSave(bool asyncSave) => csSave(0, asyncSave, false);
        //
        public void csSave(bool asyncSave, bool Blockcsv_ClearBake) => csSave(0, asyncSave, Blockcsv_ClearBake);
        //
        public void csSave() => csSave(0);
        //
        //=====================================================================================================
        /// <summary>
        /// Initialize the csv_ContentSet Result Cache when it is first opened
        /// </summary>
        /// <param name="ignore"></param>
        //
        private void csInitAfterOpen(int ignore) {
            try {
                this.resultColumnCount = 0;
                this.readCacheRowCnt = 0;
                this.readCacheRowPtr = -1;
                this.resultEOF = true;
                this.writeCache = new Dictionary<string, string>();
                this.fieldNames = new String[] { };
                if (this.dt != null) {
                    if (this.dt.Rows.Count > 0) {
                        this.resultColumnCount = this.dt.Columns.Count;
                        this.fieldNames = new String[this.resultColumnCount];
                        int ColumnPtr = 0;
                        foreach (DataColumn dc in this.dt.Columns) {
                            this.fieldNames[ColumnPtr] = GenericController.vbUCase(dc.ColumnName);
                            ColumnPtr += 1;
                        }
                        // refactor -- convert interal storage to dt and assign -- will speedup open
                        this.readCache = core.db.convertDataTabletoArray(this.dt);
                        this.readCacheRowCnt = this.readCache.GetUpperBound(1) + 1;
                        this.readCacheRowPtr = 0;
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //=====================================================================================================
        /// <summary>
        /// returns tru if the dataset is pointing past the last row
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        //
        private bool csEOF(int CSPointer) {
            return (this.readCacheRowPtr >= this.readCacheRowCnt);
        }
        //
        //========================================================================
        /// <summary>
        /// Opens a csv_ContentSet with the Members of a group
        /// </summary>
        /// <param name="groupList"></param>
        /// <param name="sqlCriteria"></param>
        /// <param name="SortFieldList"></param>
        /// <param name="ActiveOnly"></param>
        /// <param name="PageSize"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        public bool csOpenGroupUsers(List<string> groupList, string sqlCriteria = "", string SortFieldList = "", bool ActiveOnly = true, int PageSize = 9999, int PageNumber = 1) {
            try {
                DateTime rightNow = DateTime.Now;
                string sqlRightNow = DbController.encodeSQLDate(rightNow);
                //
                if (PageNumber == 0) {
                    PageNumber = 1;
                }
                if (PageSize == 0) {
                    PageSize = DbController.pageSizeDefault;
                }
                if (groupList.Count > 0) {
                    //
                    // Build Inner Query to select distinct id needed
                    //
                    string SQL = "SELECT DISTINCT ccMembers.id"
                        + " FROM (ccMembers"
                        + " LEFT JOIN ccMemberRules ON ccMembers.ID = ccMemberRules.MemberID)"
                        + " LEFT JOIN ccGroups ON ccMemberRules.GroupID = ccGroups.ID"
                        + " WHERE (ccMemberRules.Active<>0)AND(ccGroups.Active<>0)";
                    //
                    if (ActiveOnly) {
                        SQL += "AND(ccMembers.Active<>0)";
                    }
                    //
                    string subQuery = "";
                    foreach (string groupName in groupList) {
                        if (!string.IsNullOrEmpty(groupName.Trim())) {
                            subQuery += "or(ccGroups.Name=" + DbController.encodeSQLText(groupName.Trim()) + ")";
                        }
                    }
                    if (!string.IsNullOrEmpty(subQuery)) {
                        SQL += "and(" + subQuery.Substring(2) + ")";
                    }
                    //
                    // -- group expiration
                    SQL += "and((ccMemberRules.DateExpires Is Null)or(ccMemberRules.DateExpires>" + sqlRightNow + "))";
                    //
                    // Build outer query to get all ccmember fields
                    // Must do this inner/outer because if the table has a text field, it can not be in the distinct
                    //
                    SQL = "SELECT * from ccMembers where id in (" + SQL + ")";
                    if (!string.IsNullOrEmpty(sqlCriteria)) {
                        SQL += "and(" + sqlCriteria + ")";
                    }
                    if (!string.IsNullOrEmpty(SortFieldList)) {
                        SQL += " Order by " + SortFieldList;
                    }
                    return csOpenSql(SQL, "Default", PageSize, PageNumber);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return false;
        }
        //
        //========================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        // try declaring the return as object() - an array holder for variants
        // try setting up each call to return a variant, not an array of variants
        //
        public string[,] csGetRows(int CSPointer) {
            string[,] returnResult = { { } };
            try {
                returnResult = this.readCache;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        /// <summary>
        /// get the row count of the dataset
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        //
        public int csGetRowCount(int CSPointer) {
            int returnResult = 0;
            try {
                if (csOk(CSPointer)) {
                    returnResult = this.readCacheRowCnt;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        /// <summary>
        /// return the content name of a csv_ContentSet
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        //
        public string csGetContentName(int CSPointer) {
            string returnResult = "";
            try {
                if (!csOk(CSPointer)) {
                    throw new ArgumentException("dataset is not valid");
                } else {
                    returnResult = this.ContentName;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnResult;
        }
        //
        // ====================================================================================================
        //
        public string csGetRecordEditLink(int ignore, bool AllowCut = false) {
            try {
                if (!csOk(0)) { throw (new GenericException("Cannot create edit link because data set is not valid.")); }
                string ContentName = ContentMetaController.getContentNameByID(core, csGetInteger(0, "contentcontrolid"));
                if (!string.IsNullOrEmpty(ContentName)) { return Addons.AdminSite.Controllers.AdminUIController.getRecordEditLink(core, ContentName, csGetInteger(0, "ID"), GenericController.encodeBoolean(AllowCut), csGetText(0, "Name"), core.session.isEditing(ContentName)); }
                return string.Empty;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        // ====================================================================================================
        //
        public void csSetFormInput(CoreController core, int ignore, string FieldName, string RequestName = "") {
            string LocalRequestName = null;
            string Filename = null;
            string Path = null;
            if (!csOk(0)) {
                throw new GenericException("ContentSetPointer is invalid, empty, or end-of-file");
            } else if (string.IsNullOrEmpty(FieldName.Trim(' '))) {
                throw new GenericException("FieldName is invalid or blank");
            } else {
                LocalRequestName = RequestName;
                if (string.IsNullOrEmpty(LocalRequestName)) {
                    LocalRequestName = FieldName;
                }
                switch (csGetFieldTypeId(0, FieldName)) {
                    case Constants._fieldTypeIdBoolean:
                        //
                        // -- Boolean
                        csSet(0, FieldName, core.docProperties.getBoolean(LocalRequestName));
                        break;
                    case Constants._fieldTypeIdCurrency:
                    case Constants._fieldTypeIdFloat:
                    case Constants._fieldTypeIdInteger:
                    case Constants._fieldTypeIdLookup:
                    case Constants._fieldTypeIdManyToMany:
                        //
                        // -- Numbers
                        csSet(0, FieldName, core.docProperties.getNumber(LocalRequestName));
                        break;
                    case Constants._fieldTypeIdDate:
                        //
                        // -- Date
                        csSet(0, FieldName, core.docProperties.getDate(LocalRequestName));
                        break;
                    case Constants._fieldTypeIdFile:
                    case Constants._fieldTypeIdFileImage:
                        //
                        // -- upload file
                        Filename = core.docProperties.getText(LocalRequestName);
                        if (!string.IsNullOrEmpty(Filename)) {
                            Path = getFieldFilename(0, FieldName, Filename, "", csGetFieldTypeId(0, FieldName));
                            csSet(0, FieldName, Path);
                            Path = GenericController.vbReplace(Path, "\\", "/");
                            Path = GenericController.vbReplace(Path, "/" + Filename, "");
                            core.cdnFiles.upload(LocalRequestName, Path, ref Filename);
                        }
                        break;
                    default:
                        //
                        // -- text files
                        csSet(0, FieldName, core.docProperties.getText(LocalRequestName));
                        break;
                }
            }
        }
        //
        //====================================================================================================
        //
        public string csGetRecordAddLink(CoreController core, int ignore, string PresetNameValueList = "", bool AllowPaste = false) {
            string result = "";
            try {
                string ContentName = csGetContentName(0);
                if (string.IsNullOrEmpty(ContentName)) { throw new GenericException("getRecordAddLink was called with a ContentSet that was created with an SQL statement. The function requires a ContentSet opened with an OpenCSContent."); }
                foreach (var AddLink in Addons.AdminSite.Controllers.AdminUIController.getRecordAddLink(core, ContentName, PresetNameValueList, AllowPaste)) { result += AddLink; }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public bool csOpenWhatsNew(CoreController core, string SortFieldList = "", bool ActiveOnly = true, int PageSize = 1000, int PageNumber = 1) {
            try {
                return csOpenContentWatchList(core, "What's New", SortFieldList, ActiveOnly, PageSize, PageNumber);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public bool csOpenContentWatchList(CoreController core, string ListName, string SortFieldList = "", bool ActiveOnly = true, int PageSize = 1000, int PageNumber = 1) {
            try {
                string sortFieldList = encodeText(encodeEmpty(SortFieldList, "")).Trim(' ');
                if (string.IsNullOrEmpty(sortFieldList)) {
                    sortFieldList = "DateAdded";
                }
                //
                // ----- Add tablename to the front of SortFieldList fieldnames
                sortFieldList = " " + GenericController.vbReplace(sortFieldList, ",", " , ") + " ";
                sortFieldList = GenericController.vbReplace(sortFieldList, " ID ", " ccContentWatch.ID ", 1, 99, 1);
                sortFieldList = GenericController.vbReplace(sortFieldList, " Link ", " ccContentWatch.Link ", 1, 99, 1);
                sortFieldList = GenericController.vbReplace(sortFieldList, " LinkLabel ", " ccContentWatch.LinkLabel ", 1, 99, 1);
                sortFieldList = GenericController.vbReplace(sortFieldList, " SortOrder ", " ccContentWatch.SortOrder ", 1, 99, 1);
                sortFieldList = GenericController.vbReplace(sortFieldList, " DateAdded ", " ccContentWatch.DateAdded ", 1, 99, 1);
                sortFieldList = GenericController.vbReplace(sortFieldList, " ContentID ", " ccContentWatch.ContentID ", 1, 99, 1);
                sortFieldList = GenericController.vbReplace(sortFieldList, " RecordID ", " ccContentWatch.RecordID ", 1, 99, 1);
                sortFieldList = GenericController.vbReplace(sortFieldList, " ModifiedDate ", " ccContentWatch.ModifiedDate ", 1, 99, 1);
                //
                // ----- Special case
                sortFieldList = GenericController.vbReplace(sortFieldList, " name ", " ccContentWatch.LinkLabel ", 1, 99, 1);
                //
                string SQL = "SELECT"
                    + " ccContentWatch.ID AS ID"
                    + ",ccContentWatch.Link as Link"
                    + ",ccContentWatch.LinkLabel as LinkLabel"
                    + ",ccContentWatch.SortOrder as SortOrder"
                    + ",ccContentWatch.DateAdded as DateAdded"
                    + ",ccContentWatch.ContentID as ContentID"
                    + ",ccContentWatch.RecordID as RecordID"
                    + ",ccContentWatch.ModifiedDate as ModifiedDate"
                    + " FROM (ccContentWatchLists"
                    + " LEFT JOIN ccContentWatchListRules ON ccContentWatchLists.ID = ccContentWatchListRules.ContentWatchListID)"
                    + " LEFT JOIN ccContentWatch ON ccContentWatchListRules.ContentWatchID = ccContentWatch.ID"
                    + " WHERE (((ccContentWatchLists.Name)=" + DbController.encodeSQLText(ListName) + ")"
                    + "AND ((ccContentWatchLists.Active)<>0)"
                    + "AND ((ccContentWatchListRules.Active)<>0)"
                    + "AND ((ccContentWatch.Active)<>0)"
                    + "AND (ccContentWatch.Link is not null)"
                    + "AND (ccContentWatch.LinkLabel is not null)"
                    + "AND ((ccContentWatch.WhatsNewDateExpires is null)or(ccContentWatch.WhatsNewDateExpires>" + DbController.encodeSQLDate(core.doc.profileStartTime) + "))"
                    + ")"
                    + " ORDER BY " + sortFieldList + ";";
                if (!csOpenSql(SQL, "", PageSize, PageNumber)) {
                    //
                    // Check if listname exists
                    //
                    if (!this.csOpen("Content Watch Lists", "name=" + DbController.encodeSQLText(ListName), "ID", false, 0, "ID")) {
                        this.csClose();
                        if (this.csInsert("Content Watch Lists")) {
                            this.csSet(0, "name", ListName);
                        }
                    }
                    this.csClose(0);
                    return false;
                }
                return true;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        //
        public bool csOpenRecord(string ContentName, int RecordID, string SelectFieldList) {
            return csOpen(ContentName, "(ID=" + DbController.encodeSQLNumber(RecordID) + ")", "", false, core.session.user.id, SelectFieldList);
        }
        //
        public bool csOpenRecord(string ContentName, int RecordID) {
            return csOpen(ContentName, "(ID=" + DbController.encodeSQLNumber(RecordID) + ")", "", false, core.session.user.id);
        }
        //
        [Obsolete("workflow is deprecated", true)]
        public bool csOpenRecord(string ContentName, int RecordID, bool WorkflowAuthoringMode = false, bool WorkflowEditingMode = false, string SelectFieldList = "") {
            return csOpen(ContentName, "(ID=" + DbController.encodeSQLNumber(RecordID) + ")", "", false, core.session.user.id, SelectFieldList);
        }
        //
        [Obsolete("workflow is deprecated", true)]
        public bool csOpen2(string ContentName, int RecordID, bool WorkflowAuthoringMode = false, bool WorkflowEditingMode = false, string SelectFieldList = "") {
            return csOpen(ContentName, "(ID=" + DbController.encodeSQLNumber(RecordID) + ")", "", false, core.session.user.id, SelectFieldList, 1);
        }
        //
        //========================================================================
        /// <summary>
        /// Opens a dataTable for the table/row definied by the contentname and criteria
        /// </summary>
        /// <param name="contentName"></param>
        /// <param name="sqlCriteria"></param>
        /// <param name="sqlOrderBy"></param>
        /// <param name="activeOnly"></param>
        /// <param name="memberId"></param>
        /// <param name="ignorefalse2"></param>
        /// <param name="ignorefalse"></param>
        /// <param name="sqlSelectFieldList"></param>
        /// <param name="PageSize"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        public bool csOpen(string contentName, string sqlCriteria, string sqlOrderBy, bool activeOnly, int memberId, string sqlSelectFieldList, int PageSize, int PageNumber) {
            bool returnCs = false; 
            try {
                if (string.IsNullOrEmpty(contentName)) { throw new ArgumentException("ContentName cannot be blank"); }
                {
                    var CDef = Models.Domain.ContentMetaDomainModel.createByUniqueName(core, contentName);
                    if (CDef == null) { throw (new GenericException("No content found For [" + contentName + "]")); }
                    if (CDef.id <= 0) { throw (new GenericException("No content found For [" + contentName + "]")); }
                    {
                        sqlOrderBy = GenericController.encodeEmpty(sqlOrderBy, CDef.defaultSortMethod);
                        sqlSelectFieldList = GenericController.encodeEmpty(sqlSelectFieldList, CDef.selectCommaList);
                        //
                        // verify the sortfields are in this table
                        if (!string.IsNullOrEmpty(sqlOrderBy)) {
                            string[] SortFields = sqlOrderBy.Split(',');
                            for (int ptr = 0; ptr < SortFields.GetUpperBound(0) + 1; ptr++) {
                                string SortField = SortFields[ptr].ToLowerInvariant();
                                SortField = GenericController.vbReplace(SortField, "asc", "", 1, 99, 1);
                                SortField = GenericController.vbReplace(SortField, "desc", "", 1, 99, 1);
                                SortField = SortField.Trim(' ');
                                if (!CDef.selectList.Contains(SortField)) { throw (new GenericException("The field [" + SortField + "] was used in sqlOrderBy for content [" + contentName + "], but the content does not include this field.")); }
                            }
                        }
                        //
                        // ----- fixup the criteria to include the ContentControlID(s) / EditSourceID
                        string sqlContentCriteria = CDef.legacyContentControlCriteria;
                        if (string.IsNullOrEmpty(sqlContentCriteria)) {
                            sqlContentCriteria = "(1=1)";
                        } else {
                            //
                            // remove tablename from contentcontrolcriteria - if in workflow mode, and authoringtable is different, this would be wrong, also makes sql smaller, and is not necessary
                            sqlContentCriteria = GenericController.vbReplace(sqlContentCriteria, CDef.tableName + ".", "");
                        }
                        if (!string.IsNullOrEmpty(sqlCriteria)) {
                            sqlContentCriteria = sqlContentCriteria + "and(" + sqlCriteria + ")";
                        }
                        //
                        // ----- Active Only records
                        if (activeOnly) {
                            sqlContentCriteria = sqlContentCriteria + "and(active<>0)";
                        }
                        //
                        // ----- Process Select Fields, make sure ContentControlID,ID,Name,Active are included
                        sqlSelectFieldList = GenericController.vbReplace(sqlSelectFieldList, "\t", " ");
                        while (GenericController.vbInstr(1, sqlSelectFieldList, " ,") != 0) {
                            sqlSelectFieldList = GenericController.vbReplace(sqlSelectFieldList, " ,", ",");
                        }
                        while (GenericController.vbInstr(1, sqlSelectFieldList, ", ") != 0) {
                            sqlSelectFieldList = GenericController.vbReplace(sqlSelectFieldList, ", ", ",");
                        }
                        if ((!string.IsNullOrEmpty(sqlSelectFieldList)) && (sqlSelectFieldList.IndexOf("*", System.StringComparison.OrdinalIgnoreCase) == -1)) {
                            string TestUcaseFieldList = GenericController.vbUCase("," + sqlSelectFieldList + ",");
                            if (GenericController.vbInstr(1, TestUcaseFieldList, ",CONTENTCONTROLID,", 1) == 0) {
                                sqlSelectFieldList = sqlSelectFieldList + ",ContentControlID";
                            }
                            if (GenericController.vbInstr(1, TestUcaseFieldList, ",NAME,", 1) == 0) {
                                sqlSelectFieldList = sqlSelectFieldList + ",Name";
                            }
                            if (GenericController.vbInstr(1, TestUcaseFieldList, ",ID,", 1) == 0) {
                                sqlSelectFieldList = sqlSelectFieldList + ",ID";
                            }
                            if (GenericController.vbInstr(1, TestUcaseFieldList, ",ACTIVE,", 1) == 0) {
                                sqlSelectFieldList = sqlSelectFieldList + ",ACTIVE";
                            }
                        }
                        //
                        // ----- Check for blank Tablename or DataSource
                        if (string.IsNullOrEmpty(CDef.tableName)) {
                            throw (new Exception("Content metadata [" + contentName + "] does not reference a valid table"));
                        } else if (string.IsNullOrEmpty(CDef.dataSourceName)) {
                            throw (new Exception("Table metadata [" + CDef.tableName + "] does not reference a valid datasource"));
                        }
                        //
                        // ----- If no select list, use *
                        if (string.IsNullOrEmpty(sqlSelectFieldList)) {
                            sqlSelectFieldList = "*";
                        }
                        string sql = "select " + sqlSelectFieldList + " from " + CDef.tableName + " where (" + sqlContentCriteria + ")" + (string.IsNullOrWhiteSpace(sqlOrderBy) ? "" : " order by " + sqlOrderBy);
                        //
                        // -- now open the sql
                        if ( csOpenSql( sql, CDef.dataSourceName, PageSize, PageNumber )) {
                            //
                            // -- correct the status
                            this.readable = true;
                            this.writeable = true;
                            this.ContentName = contentName;
                            this.DataSource = CDef.dataSourceName;
                            this.CDef = CDef;
                            this.SelectTableFieldList = sqlSelectFieldList;
                            this.Source = sql;
                            return true;
                        }
                        //csInit(memberId);
                        //this.readable = true;
                        //this.writeable = true;
                        //this.ContentName = contentName;
                        //this.DataSource = CDef.dataSourceName;
                        //this.CDef = CDef;
                        //this.SelectTableFieldList = sqlSelectFieldList;
                        //this.PageNumber = PageNumber;
                        //this.PageSize = PageSize;
                        //if (this.PageNumber <= 0) {
                        //    this.PageNumber = 1;
                        //}
                        //if (this.PageSize < 0) {
                        //    this.PageSize = Constants.maxLongValue;
                        //} else if (this.PageSize == 0) {
                        //    this.PageSize = DbController.pageSizeDefault;
                        //}
                        //this.Source = SQL;
                        //this.dt = core.db.executeQuery(SQL, CDef.dataSourceName, this.PageSize * (this.PageNumber - 1), this.PageSize);
                        //csInitAfterOpen(0);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnCs;
        }
        //
        [Obsolete("deprecated, remove ignore fields",true)]
        public bool csOpen(string contentName, string sqlCriteria, string sqlOrderBy, bool activeOnly, int memberId, bool ignorefalse2, bool ignorefalse, string sqlSelectFieldList, int PageSize, int PageNumber)
            => csOpen(contentName, sqlCriteria, sqlOrderBy, activeOnly, memberId, sqlSelectFieldList, PageSize, PageNumber);
        //
        [Obsolete("deprecated, remove ignore fields", true)]
        public bool csOpen(string contentName, string sqlCriteria, string sqlOrderBy, bool activeOnly, int memberId, bool ignorefalse2, bool ignorefalse, string sqlSelectFieldList, int PageSize)
            => csOpen(contentName, sqlCriteria, sqlOrderBy, activeOnly, memberId, sqlSelectFieldList, PageSize, 1);
        //
        [Obsolete("deprecated, remove ignore fields", true)]
        public bool csOpen(string contentName, string sqlCriteria, string sqlOrderBy, bool activeOnly, int memberId, bool ignorefalse2, bool ignorefalse, string sqlSelectFieldList)
            => csOpen(contentName, sqlCriteria, sqlOrderBy, activeOnly, memberId, sqlSelectFieldList, 9999, 1);
        //
        public bool csOpen(string contentName, string sqlCriteria, string sqlOrderBy, bool activeOnly, int memberId)
            => csOpen(contentName, sqlCriteria, sqlOrderBy, activeOnly, memberId, "", 9999, 1);
        //
        public bool csOpen(string contentName, string sqlCriteria, string sqlOrderBy, bool activeOnly, int memberId, string sqlSelectFieldList, int PageSize)
            => csOpen(contentName, sqlCriteria, sqlOrderBy, activeOnly, memberId, sqlSelectFieldList, PageSize, 1);
        //
        public bool csOpen(string contentName, string sqlCriteria, string sqlOrderBy, bool activeOnly, int memberId, string sqlSelectFieldList)
            => csOpen(contentName, sqlCriteria, sqlOrderBy, activeOnly, memberId, sqlSelectFieldList, 9999, 1);
        //
        public bool csOpen(string contentName, string sqlCriteria, string sqlOrderBy, bool activeOnly)
            => csOpen(contentName, sqlCriteria, sqlOrderBy, activeOnly, 0, "", 9999, 1);
        //
        public bool csOpen(string contentName, string sqlCriteria, string sqlOrderBy)
            => csOpen(contentName, sqlCriteria, sqlOrderBy, true, 0, "", 9999, 1);
        //
        public bool csOpen(string contentName, string sqlCriteria)
            => csOpen(contentName, sqlCriteria, "", true, 0, "", 9999, 1);
        //
        public bool csOpen(string contentName)
            => csOpen(contentName, "", "", true, 0, "", 9999, 1);
        //
        ////        
        ////========================================================================
        ///// <summary>
        ///// open a contentset without a record, to be used to update a record. you can set to write cache, and read from write cache, but cannot read fields not written
        ///// </summary>
        ///// <param name="ContentName"></param>
        ///// <param name="recordId">The record that will be updated if saved</param>
        ///// <param name="memberId">The user logged as modified by if saved</param>
        ///// <returns></returns>
        ////
        //public bool csOpenForUpdate(string ContentName, int recordId, int memberId = 0) {
        //    try {
        //        if (string.IsNullOrEmpty(ContentName)) {
        //            throw new ArgumentException("ContentName cannot be blank");
        //        } else {
        //            var CDef = Models.Domain.ContentMetaDomainModel.createByUniqueName(core, ContentName);
        //            if (CDef == null) {
        //                throw (new GenericException("No content found For [" + ContentName + "]"));
        //            } else if (CDef.id <= 0) {
        //                throw (new GenericException("No content found For [" + ContentName + "]"));
        //            } else {
        //                //
        //                // ----- Open the csv_ContentSet
        //                returnCs = csInit(memberId);
        //                DbController.ContentSetClass contentSet = contentSetStore[returnCs];
        //                this.readable = false;
        //                this.writeable = true;
        //                this.ContentName = ContentName;
        //                this.DataSource = CDef.dataSourceName;
        //                this.CDef = CDef;
        //                this.SelectTableFieldList = CDef.selectCommaList;
        //                this.PageNumber = 1;
        //                this.PageSize = 1;
        //                this.Source = "";
        //                csInitAfterOpen(returnCs);
        //                //
        //                // -- initialize the id because the legacy system uses getinteger("id") to map to db
        //                if (this.writeCache.ContainsKey("id")) {
        //                    this.writeCache["id"] = recordId.ToString();
        //                } else {
        //                    this.writeCache.Add("id", recordId.ToString());
        //                }
        //            }
        //        }
        //    } catch (Exception ex) {
        //        LogController.handleError(core, ex);
        //        throw;
        //    }
        //    return returnCs;
        //}
        ////
        ////====================================================================================================
        ////
        //// need a new cs method here... openForUpdate( optional id )
        ////  creates a cs with no read data and an empty write buffer
        ////  read buffer get() is blocked, but you can setField()
        ////  cs.save() writes values, if id=0 it does insert, else just update
        ////
        //public bool openForUpdate(string ContentName, int recordId) {
        //    return csOpenForUpdate(ContentName, recordId);
        //}
        //
        //========================================================================
        /// <summary>
        /// openSql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public bool csOpenSql(string sql, string dataSourceName = "", int pageSize = 9999, int pageNumber = 1) {
            try {
                csInit(core.session.user.id);
                this.readable = true;
                this.writeable = false;
                this.ContentName = "";
                this.PageNumber = pageNumber;
                this.PageSize = (pageSize);
                this.DataSource = dataSourceName;
                this.SelectTableFieldList = "";
                this.Source = sql;
                this.dt = core.db.executeQuery(sql, dataSourceName, pageSize * (pageNumber - 1), pageSize);
                csInitAfterOpen(0);
                return csOk();
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public bool openSQL(string sql) {
            return csOpenSql(sql, "Default");
        }
        //========================================================================
        // Opens a Content Record
        //   If there was a problem, it returns -1 (not csv_IsCSOK)
        //   Can open either the ContentRecord or the AuthoringRecord (WorkflowAuthoringMode)
        //   Isolated in API so later we can save record in an Index buffer for fast access
        //========================================================================
        //
        public bool csOpenContentRecord(string ContentName, int RecordID, int MemberID = SystemMemberID, bool WorkflowAuthoringMode = false, bool WorkflowEditingMode = false, string SelectFieldList = "") {
            return csOpen(ContentName, "(ID=" + DbController.encodeSQLNumber(RecordID) + ")", "", false, MemberID, SelectFieldList, 1);
        }
        //
        private void sample() {
            using (var cs = new CsModel(core)) {
                if (cs.openSQL("")) {

                }
            }
        }
        //
        //========================================================================
        // Dispose
        //
        #region  IDisposable Support 
        //
        //====================================================================================================
        /// <summary>
        /// dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // -- call .dispose for managed objects
                    if (dt != null) {
                        dt.Dispose();
                        dt = null;
                    }
                }
                //
                // -- Add code here to release the unmanaged resource.
            }
            this.disposed = true;
        }
        protected bool disposed = false;
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CsModel() {
            Dispose(false);
        }
        #endregion
    }

}

