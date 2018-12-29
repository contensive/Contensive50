﻿
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
        private int userId;
        /// <summary>
        /// If true, it is in use
        /// </summary>
        public bool isOpen;
        /// <summary>
        /// The date/time this csv_ContentSet was last used
        /// </summary>
        public DateTime lastUsed;
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
        public string contentName;
        /// <summary>
        /// If opened with a content name, this is that content's metadata
        /// </summary>
        public Models.Domain.MetaModel contentMeta;
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
        public string sqlSource;
        /// <summary>
        /// The Datasource of the SQL that created the result set
        /// </summary>
        public string dataSource;
        /// <summary>
        /// Number of records in a cache page
        /// </summary>
        public int pageSize;
        /// <summary>
        /// The Page that this result starts with
        /// </summary>
        public int pageNumber;
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
        public string sqlSelectFieldList;
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
            userId = core.session.user.id;
        }
        //
        //====================================================================================================
        /// <summary>
        /// close the read data set, save what needs to be written
        /// </summary>
        /// <param name="asyncSave"></param>
        public void close(bool asyncSave) {
            try {
                if (isOpen) {
                    csSave(asyncSave);
                    readCache = new string[,] { { }, { } };
                    writeCache = new Dictionary<string, string>();
                    resultColumnCount = 0;
                    readCacheRowCnt = 0;
                    readCacheRowPtr = -1;
                    resultEOF = true;
                    isOpen = false;
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
        public void close() => close(false);
        //
        //====================================================================================================
        /// <summary>
        /// Copy the current dataset to a target set. Target must be already opened or inserted
        /// </summary>
        /// <param name="CSSource"></param>
        /// <param name="CSDestination"></param>
        public void copyRecord(CsModel destination) {
            try {
                if (!isOpen) { throw new ArgumentException("source dataset is not valid"); }
                if (!destination.isOpen) { throw new ArgumentException("destination dataset is not valid"); }
                if (contentMeta == null) { throw new ArgumentException("copyRecord requires the source and destination datasets be created from an open or insert, not a sql."); }
                if (destination.contentMeta == null) { throw new ArgumentException("copyRecord requires the source and destination datasets be created from an open or insert, not a sql."); }
                //
                foreach ( var kvp in contentMeta.fields) {
                    MetaFieldModel field = kvp.Value;
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
                                        string SourceFilename = getFieldFilename(field.nameLc, "", contentMeta.name, field.fieldTypeId);
                                        if (!string.IsNullOrEmpty(SourceFilename)) {
                                            string DestFilename = destination.getFieldFilename(field.nameLc, "", destination.contentName, field.fieldTypeId);
                                            destination.csSet(field.nameLc, DestFilename);
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
                                        string SourceFilename = getFieldFilename(field.nameLc, "", contentMeta.name, field.fieldTypeId);
                                        if (!string.IsNullOrEmpty(SourceFilename)) {
                                            string DestFilename = destination.getFieldFilename(field.nameLc, "", destination.contentName, field.fieldTypeId);
                                            destination.csSet(field.nameLc, DestFilename);
                                            core.privateFiles.copyFile(SourceFilename, DestFilename);
                                        }
                                    }
                                    break;
                                default:
                                    //
                                    // ----- value
                                    //
                                    destination.csSet(field.nameLc, csGetValue(0, field.nameLc));
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
        /// delete the current record in the dataset
        /// </summary>
        public void deleteRecord() {
            try {
                if (!csOk()) { throw new ArgumentException("csv_ContentSet Is empty Or at End-Of-file"); }
                if (!(writeable)) { throw new ArgumentException("Dataset is not writeable."); }
                if (string.IsNullOrEmpty(contentMeta.name)) { throw new ArgumentException("csv_ContentSet Is Not based On a Content Definition"); }
                //
                // delete any files (if filename is part of select)
                foreach (var selectedFieldName in contentMeta.selectList) {
                    if (contentMeta.fields.ContainsKey(selectedFieldName.ToLowerInvariant())) {
                        var field = contentMeta.fields[selectedFieldName.ToLowerInvariant()];
                        switch (field.fieldTypeId) {
                            case Constants._fieldTypeIdFile:
                            case Constants._fieldTypeIdFileImage:
                            case Constants._fieldTypeIdFileCSS:
                            case Constants._fieldTypeIdFileJavascript:
                            case Constants._fieldTypeIdFileXML: {
                                    //
                                    // public content files
                                    string Filename = csGetText(field.nameLc);
                                    if (!string.IsNullOrEmpty(Filename)) {
                                        core.cdnFiles.deleteFile(Filename);
                                        //Call core.cdnFiles.deleteFile(core.cdnFiles.joinPath(core.appConfig.cdnFilesNetprefix, Filename))
                                    }
                                    break;
                                }
                            case Constants._fieldTypeIdFileText:
                            case Constants._fieldTypeIdFileHTML: {
                                    //
                                    // private files
                                    string Filename = csGetText(field.nameLc);
                                    if (!string.IsNullOrEmpty(Filename)) {
                                        core.cdnFiles.deleteFile(Filename);
                                    }
                                    break;
                                }
                        }
                    }
                }
                //
                int LiveRecordID = csGetInteger("ID");
                core.db.deleteTableRecord(LiveRecordID, contentMeta.tableName, contentMeta.dataSourceName);
                MetaController.deleteContentRules(core, contentMeta.id, LiveRecordID);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Inserts a record in a table and return a dataset with just that record
        /// </summary>
        /// <param name="contentName"></param>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public bool insert(string contentName) {
            try {
                if (isOpen) { close(); }
                if (string.IsNullOrEmpty(contentName.Trim())) { throw new ArgumentException("ContentName cannot be blank"); }
                {
                    var CDef = MetaModel.createByUniqueName(core, contentName);
                    if (CDef == null) { throw new GenericException("content [" + contentName + "] could Not be found."); }
                    if (CDef.id <= 0) { throw new GenericException("content [" + contentName + "] could Not be found."); }
                    //
                    // create default record in Live table
                    SqlFieldListClass sqlList = new SqlFieldListClass();
                    foreach (KeyValuePair<string, Models.Domain.MetaFieldModel> keyValuePair in CDef.fields) {
                        MetaFieldModel field = keyValuePair.Value;
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
                                                    string LookupContentName = MetaController.getContentNameByID(core, field.lookupContentID);
                                                    if (!string.IsNullOrEmpty(LookupContentName)) {
                                                        DefaultValueText = MetaController.getRecordIdByUniqueName(core, LookupContentName, DefaultValueText).ToString();
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
                    sqlList.add("CREATEDBY", DbController.encodeSQLNumber(userId));
                    core.db.insertTableRecord(CDef.dataSourceName, CDef.tableName, sqlList);
                    //
                    // ----- Get the record back so we can use the ID
                    return csOpen(contentName, "(ccguid=" + sqlGuid + ")And(DateAdded=" + sqlDateAdded + ")", "ID DESC", false, userId);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        public bool getBoolean(string fieldName) {
            return csGetBoolean(fieldName);
        }
        //
        //====================================================================================================
        public DateTime getDate(string fieldName) {
            return csGetDate(fieldName);
        }
        //
        //====================================================================================================
        public int getInteger(string fieldName) {
            return csGetInteger(fieldName);
        }
        //
        //====================================================================================================
        public double getNumber(string fieldName) {
            return csGetNumber(fieldName);
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
            return csOk();
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, DateTime FieldValue) {
            csSet(FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, bool FieldValue) {
            csSet(FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, string FieldValue) {
            csSet(FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        public void setField(string FieldName, double FieldValue) {
            csSet(FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field. If the field is backed by a file, the value will be saved to the file
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, int FieldValue) {
            csSet(FieldName, FieldValue);
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
            if (csIsFieldSupported("id") & csIsFieldSupported("contentcontrolId")) {
                RecordID = csGetInteger("id");
                ContentName = MetaController.getContentNameByID(core, csGetInteger("contentcontrolId"));
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
            isOpen = true;
            newRecord = true;
            contentName = "";
            contentMeta = null;
            dataSource = "";
            dt = null;
            fieldNames = null;
            fieldPointer = 0;
            isModified = false;
            lastUsed = DateTime.Now;
            userId = core.session.user.id;
            pageNumber = 0;
            pageSize = 0;
            readCache = null;
            readCacheRowCnt = 0;
            readCacheRowPtr = 0;
            resultColumnCount = 0;
            resultEOF = true;
            sqlSelectFieldList = "";
            sqlSource = "";
            writeable = false;
            writeCache = null;
        }
        //
        private void csInit() => csInit(0);
        //
        //========================================================================
        //
        public void csGoNext(bool AsyncSave) {
            try {
                if (!csOk()) {
                    //
                    throw new GenericException("Dataset is not valid.");
                } else if (!this.readable) {
                    //
                    // -- if not readable, cannot move rows
                    throw new GenericException("Cannot move to next row because dataset is not readable.");
                } else {
                    csSave(AsyncSave);
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
        public void csGoNext(int ignore, bool AsyncSave) => csGoNext(AsyncSave);
        //
        public void csGoNext(int ignore) => csGoNext(false);
        //
        public void csGoNext() => csGoNext(false);
        //
        //========================================================================
        //
        public void csGoFirst(bool asyncSave) {
            try {
                csSave(asyncSave);
                this.writeCache = new Dictionary<string, string>();
                this.readCacheRowPtr = 0;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //public void csGoFirst(int CSPointer, bool AsyncSave) => csGoFirst(AsyncSave);
        //
        //public void csGoFirst(int CSPointer) => csGoFirst(false);
        //
        public void csGoFirst() => csGoFirst(false);
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
                                    throw new GenericException("Field [" + fieldNameTrim + "] was not found in [" + this.contentName + "] with selected fields [" + String.Join(",", dtFieldList.ToArray()) + "]");
                                } else {
                                    throw new GenericException("Field [" + fieldNameTrim + "] was not found in sql [" + this.sqlSource + "]");
                                }
                            } else {
                                returnValue = GenericController.encodeText(this.dt.Rows[this.readCacheRowPtr][fieldNameTrim.ToLowerInvariant()]);
                            }
                        }
                    }
                    this.lastUsed = DateTime.Now;
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
        /// <param name="ignore"></param>
        /// <returns></returns>
        public string csGetFirstFieldName() {
            try {
                if (!csOk()) { throw new GenericException("data set is not valid"); }
                this.fieldPointer = 0;
                return csGetNextFieldName();
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        public string csGetFirstFieldName(int ignore) => csGetFirstFieldName();
        //
        //========================================================================
        /// <summary>
        /// get the next fieldname in the CS, Returns null if there are no more
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        public string csGetNextFieldName() {
            try {
                if (!csOk()) { throw new GenericException("data set is not valid"); }
                while (this.fieldPointer < this.resultColumnCount) {
                    if(!string.IsNullOrWhiteSpace(this.fieldNames[this.fieldPointer])) { return this.fieldNames[this.fieldPointer]; }
                    this.fieldPointer += 1;
                }
                return "";
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //public string csGetNextFieldName(int CSPointer) => csGetNextFieldName();
        //
        //========================================================================
        /// <summary>
        /// get the type of a field within a csv_ContentSet
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public int csGetFieldTypeId(string FieldName) {
            try {
                if (csOk() && this.writeable && !string.IsNullOrEmpty(this.contentMeta.name)) { return this.contentMeta.fields[FieldName.ToLowerInvariant()].fieldTypeId; }
                return 0;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //public int csGetFieldTypeId(int CSPointer, string FieldName) => csGetFieldTypeId(FieldName);
        //
        //========================================================================
        /// <summary>
        /// get the caption of a field within a csv_ContentSet
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string csGetFieldCaption(string fieldName) {
            string returnResult = "";
            try {
                if (csOk() && (this.writeable) && (!string.IsNullOrEmpty(this.contentMeta.name))) {
                    returnResult = this.contentMeta.fields[fieldName.ToLowerInvariant()].caption;
                    if (string.IsNullOrEmpty(returnResult)) {
                        returnResult = fieldName;
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
        public string csGetSelectFieldList() {
            try {
                if (csOk()) { return string.Join(",", this.fieldNames); }
                return string.Empty;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //public string csGetSelectFieldList(int CSPointer) => csGetSelectFieldList();
        //
        //========================================================================
        /// <summary>
        /// get the caption of a field within a csv_ContentSet
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public bool csIsFieldSupported(string FieldName) {
            try {
                if (string.IsNullOrEmpty(FieldName)) { throw new ArgumentException("Field name cannot be blank"); }
                if (!csOk()) { throw new ArgumentException("dataset is not valid"); }
                return GenericController.isInDelimitedString(csGetSelectFieldList(), FieldName, ",");
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //public bool csIsFieldSupported(int CSPointer, string FieldName) => csIsFieldSupported(FieldName);
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
        public string getFieldFilename(string FieldName, string OriginalFilename, string ContentName, int fieldTypeId) {
            string returnFilename = "";
            try {
                string TableName = null;
                int RecordID = 0;
                string fieldNameUpper = null;
                int LenOriginalFilename = 0;
                int LenFilename = 0;
                int Pos = 0;
                //
                if (!csOk()) {
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
                                    RecordID = csGetInteger("ID");
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
                            ContentName = this.contentMeta.name;
                            TableName = this.contentMeta.tableName;
                        } else if (!string.IsNullOrEmpty(ContentName)) {
                            //
                            // CS is SQL-based, use the contentname
                            //
                            TableName = MetaController.getContentTablename(core, ContentName);
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
                                fieldTypeId = this.contentMeta.fields[FieldName.ToLowerInvariant()].fieldTypeId;
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
        public string getFieldFilename(string FieldName, string OriginalFilename) => getFieldFilename(FieldName, OriginalFilename, "", 0);
        //
        //public string getFieldFilename(int ignore, string FieldName, string OriginalFilename, string ContentName, int fieldTypeId) => getFieldFilename(FieldName, OriginalFilename, ContentName, fieldTypeId);
        //
        //public string getFieldFilename(int ignore, string FieldName, string OriginalFilename, string ContentName) => getFieldFilename(FieldName, OriginalFilename, ContentName, 0);
        //
        //public string getFieldFilename(int ignore, string FieldName, string OriginalFilename) => getFieldFilename(FieldName, OriginalFilename, "", 0);
        //
        //====================================================================================================
        //
        public string csGetText(string FieldName) {
            return csGetValue(FieldName);
        }
        //
        //public string csGetText(int CSPointer, string FieldName) => csGetText(FieldName);
        //
        //====================================================================================================
        //
        public int csGetInteger(string FieldName) {
            return encodeInteger(csGetValue(FieldName));
        }
        //public int csGetInteger(int CSPointer, string FieldName) => csGetInteger(FieldName);
        //
        //====================================================================================================
        //
        public double csGetNumber(string FieldName) {
            return encodeNumber(csGetValue(FieldName));
        }
        //
        //public double csGetNumber(int CSPointer, string FieldName) => csGetNumber(FieldName);
        //
        //====================================================================================================
        //
        public DateTime csGetDate(string FieldName) {
            return GenericController.encodeDate(csGetValue(FieldName));
        }
        //
        //public DateTime csGetDate(int CSPointer, string FieldName) => csGetDate(FieldName);
        //
        //====================================================================================================
        //
        public bool csGetBoolean(string FieldName) {
            return encodeBoolean(csGetValue(FieldName));
        }
        //
        //public bool csGetBoolean(int CSPointer, string FieldName) => csGetBoolean(FieldName);
        //
        //====================================================================================================
        //
        public string csGetLookup(string FieldName) {
            return csGet(0,FieldName);
        }
        //
        //public string csGetLookup(int CSPointer, string FieldName) => csGetLookup(FieldName);
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
                if (this.contentMeta == null) { throw new GenericException("Cannot set fields for a dataset based on a query."); }
                if (this.contentMeta.fields == null) { throw new GenericException("The dataset contains no fields."); }
                if (!this.contentMeta.fields.ContainsKey(FieldName.ToLowerInvariant())) { throw new GenericException("The dataset does not contain the field specified [" + FieldName.ToLowerInvariant() + "]."); }
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
                if (!csOk()) { throw new ArgumentException("dataset is not valid"); }
                if (string.IsNullOrEmpty(FieldName)) { throw new ArgumentException("fieldName cannot be blank"); }
                if (string.IsNullOrEmpty(ContentName)) { throw new ArgumentException("contentName cannot be blank"); }
                if (!this.writeable) { throw new GenericException("Cannot save this dataset because it is read-only."); }
                string OldFilename = csGetText( FieldName);
                string Filename = getFieldFilename( FieldName, "", ContentName, Constants.fieldTypeIdFileText);
                if (OldFilename != Filename) {
                    //
                    // Filename changed, mark record changed
                    //
                    core.cdnFiles.saveFile(Filename, Copy);
                    csSet(FieldName, Filename);
                } else {
                    string OldCopy = core.cdnFiles.readFileText(Filename);
                    if (OldCopy != Copy) {
                        //
                        // copy changed, mark record changed
                        //
                        core.cdnFiles.saveFile(Filename, Copy);
                        csSet(FieldName, Filename);
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
                if (this.writeable & !this.readable) { return this.isOpen; }
                //
                // -- normal open
                return this.isOpen & (this.readCacheRowPtr >= 0) && (this.readCacheRowPtr < this.readCacheRowCnt);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //       
        //public bool csOk(int ignore) => csOk();
        //       
        //========================================================================
        /// <summary>
        /// Returns the Source for the csv_ContentSet
        /// </summary>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public string csGetSql() {
            try {
                if (!csOk()) { throw new ArgumentException("the dataset is not valid"); }
                return this.sqlSource;
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
        public string csGet(string FieldName) {
            string fieldValue = "";
            try {
                if (!csOk()) { throw new ArgumentException("the dataset is not valid"); }
                if (string.IsNullOrEmpty(FieldName.Trim())) { throw new ArgumentException("fieldname cannot be blank"); }
                {
                    if (!this.writeable) {
                        //
                        // -- Not updateable, Just return what is there as a string
                        return csGetValue( FieldName);
                    } else {
                        //
                        // -- Updateable, interpret the value
                        if (!this.contentMeta.fields.ContainsKey(FieldName.ToLowerInvariant())) {
                            try {
                                fieldValue = GenericController.encodeText(csGetValue( FieldName));
                            } catch (Exception ex) {
                                throw new GenericException("Error [" + ex.Message + "] reading field [" + FieldName.ToLowerInvariant() + "] In content [" + this.contentMeta.name + "] With custom field list [" + this.sqlSelectFieldList + "");
                            }
                        } else {
                            var field = this.contentMeta.fields[FieldName.ToLowerInvariant()];
                            int fieldTypeId = field.fieldTypeId;
                            if (fieldTypeId == Constants.fieldTypeIdManyToMany) {
                                //
                                // special case - recordset contains no data - return record id list
                                //
                                int RecordID = 0;
                                string DbTable = null;
                                string ContentName = null;
                                if (this.contentMeta.fields.ContainsKey("id")) {
                                    RecordID = GenericController.encodeInteger(csGetValue( "id"));
                                    ContentName = MetaController.getContentNameByID(core, field.manyToManyRuleContentID);
                                    DbTable = MetaController.getContentTablename(core, ContentName);
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
                                object FieldValueVariant = csGetValue( FieldName);
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
                                                string LookupContentName = MetaController.getContentNameByID(core, fieldLookupId);
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
                                                fieldValue = MetaController.getRecordName(core, "people", GenericController.encodeInteger(FieldValueVariant));
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
        public string csGet(int ignore, string FieldName) => csGet(FieldName);
        //
        //========================================================================
        /// <summary>
        /// Saves the value for the field. If the field uses a file, the content is saved to the file using the fields filename. To set a file-based field's filename, use setFieldFilename
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        //
        public void csSet(string FieldName, string FieldValue) {
            try {
                string BlankTest = null;
                string FieldNameLc = null;
                bool SetNeeded = false;
                string fileNameNoExt = null;
                string ContentName = null;
                string fileName = null;
                string pathFilenameOriginal = null;
                //
                if (!csOk()) {
                    throw new ArgumentException("dataset is not valid or End-Of-file.");
                } else if (string.IsNullOrEmpty(FieldName.Trim())) {
                    throw new ArgumentException("fieldName cannnot be blank");
                } else {
                    if (!this.writeable) {
                        throw new GenericException("Cannot update a contentset created from a sql query.");
                    } else {
                        ContentName = this.contentName;
                        FieldNameLc = FieldName.Trim(' ').ToLowerInvariant();
                        if (FieldValue == null) {
                            FieldValue = "";
                        }
                        if (!string.IsNullOrEmpty(this.contentMeta.name)) {
                            Models.Domain.MetaFieldModel field = null;
                            if (!this.contentMeta.fields.ContainsKey(FieldNameLc)) {
                                throw new ArgumentException("The field [" + FieldName + "] could Not be found In content [" + this.contentMeta.name + "]");
                            } else {
                                field = this.contentMeta.fields[FieldNameLc];
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
                                        pathFilenameOriginal = csGetText(FieldNameLc);
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
                                                PathFilename = getFieldFilename(FieldNameLc, "", ContentName, field.fieldTypeId);
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
                                        if (GenericController.encodeBoolean(FieldValue) != csGetBoolean(FieldNameLc)) {
                                            SetNeeded = true;
                                        }
                                        break;
                                    case Constants._fieldTypeIdText:
                                        //
                                        // Set if text of value changes
                                        //
                                        if (GenericController.encodeText(FieldValue) != csGetText( FieldNameLc)) {
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
                                        if (GenericController.encodeText(FieldValue) != csGetText( FieldNameLc)) {
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
                                        if (GenericController.encodeText(FieldValue) != csGetText( FieldNameLc)) {
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
                            this.lastUsed = DateTime.Now;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        public void csSet(string FieldName, int FieldValue) => csSet(FieldName, FieldValue.ToString());
        //
        public void csSet(string FieldName, double FieldValue) => csSet(FieldName, FieldValue.ToString());
        //
        public void csSet(string FieldName, DateTime FieldValue) => csSet(FieldName, FieldValue.ToString());
        //
        public void csSet(string FieldName, bool FieldValue) => csSet(FieldName, FieldValue.ToString());
        //
        //public void csSet(int ignore, string FieldName, string FieldValue) => csSet(FieldName, FieldValue);
        //
        //public void csSet(int ignore, string FieldName, int FieldValue) => csSet(FieldName, FieldValue.ToString());
        //
        //public void csSet(int ignore, string FieldName, DateTime FieldValue) => csSet(FieldName, FieldValue.ToString());
        //
        //public void csSet(int ignore, string FieldName, bool FieldValue) => csSet(FieldName, FieldValue.ToString());
        //
        //public void csSet(int ignore, string FieldName, double FieldValue) => csSet(FieldName, FieldValue.ToString());
        //
        //========================================================================
        /// <summary>
        /// rollback, or undo the changes to the current row
        /// </summary>
        /// <param name="CSPointer"></param>
        public void csRollBack() {
            try {
                if (!csOk()) { throw new ArgumentException("dataset is not valid"); }
                this.writeCache.Clear();
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
        /// <param name="blockClearCache"></param>
        public void csSave(bool asyncSave, bool blockClearCache) {
            try {
                if (!csOk()) { return; }
                if (this.writeCache.Count == 0) { return;  }
                if (!(this.writeable)) { throw new ArgumentException("The dataset cannot be updated because it was created with a query and not a content table."); }
                {
                    var contentSet = this;
                    if (this.contentMeta == null) {
                        //
                        // -- dataset not updatable
                        throw new ArgumentException("The dataset cannot be updated because it was not created from a valid content table.");
                    } else {
                        //
                        // -- get id from read cache or write cache. if id=0 save is insert, else save is update
                        int id = csGetInteger("ID");
                        string sqlDelimiter = "";
                        string sqlUpdate = "";
                        DateTime sqlModifiedDate = DateTime.Now;
                        int sqlModifiedBy = this.userId;
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
                                Models.Domain.MetaFieldModel field = this.contentMeta.fields[fieldName.ToLowerInvariant()];
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
                                                SQLCriteriaUnique += "(" + field.nameLc + "=" + MetaController.encodeSQL(writeCacheValue, field.fieldTypeId) + ")";
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
                            string sqlUnique = "SELECT ID FROM " + this.contentMeta.tableName + " WHERE (ID<>" + id + ")AND(" + SQLCriteriaUnique + ")and(" + this.contentMeta.legacyContentControlCriteria + ");";
                            using (DataTable dt = core.db.executeQuery(sqlUnique, this.contentMeta.dataSourceName)) {
                                //
                                // -- unique violation
                                if (dt.Rows.Count > 0) {
                                    LogController.logWarn(core, "Can not save record to content [" + this.contentMeta.name + "] because it would create a non-unique record for one or more of the following field(s) [" + UniqueViolationFieldList + "]");
                                    return;
                                }
                            }
                        }
                        if (FieldFoundCount > 0) {
                            //
                            // ----- update live table (non-workflowauthoring and non-authorable fields)
                            //
                            if (!string.IsNullOrEmpty(sqlUpdate)) {
                                string SQLUpdate = "UPDATE " + this.contentMeta.tableName + " SET " + sqlUpdate + " WHERE ID=" + id + ";";
                                if (asyncSave) {
                                    core.db.executeNonQueryAsync(SQLUpdate, this.contentMeta.dataSourceName);
                                } else {
                                    core.db.executeNonQuery(SQLUpdate, this.contentMeta.dataSourceName);
                                }
                            }
                        }
                        this.lastUsed = DateTime.Now;
                        //
                        // -- invalidate the special cache name used to detect a change in any record
                        core.cache.invalidateDbRecord(id, this.contentMeta.tableName, this.contentMeta.dataSourceName);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        public void csSave(bool asyncSave) => csSave(asyncSave, false);
        //
        public void csSave() => csSave(false, false );
        //
        //public void csSave(int ignore, bool asyncSave) => csSave(asyncSave, false);
        //
        //public void csSave(int ignore) => csSave(false, false);
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
        private bool csEOF() {
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
        public string[,] csGetRows() {
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
        public string[,] csGetRows(int ignore) => csGetRows();
        //
        //========================================================================
        /// <summary>
        /// get the row count of the dataset
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        //
        public int csGetRowCount() {
            int returnResult = 0;
            try {
                if (csOk()) {
                    returnResult = this.readCacheRowCnt;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnResult;
        }
        //
        //public int csGetRowCount(int ignore) => csGetRowCount();
        //
        //========================================================================
        /// <summary>
        /// return the content name of a csv_ContentSet
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        //
        public string csGetContentName() {
            string returnResult = "";
            try {
                if (!csOk()) {
                    throw new ArgumentException("dataset is not valid");
                } else {
                    returnResult = this.contentName;
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
        public string csGetRecordEditLink(bool allowCut) {
            try {
                if (!csOk()) { throw (new GenericException("Cannot create edit link because data set is not valid.")); }
                string ContentName = MetaController.getContentNameByID(core, csGetInteger("contentcontrolid"));
                if (!string.IsNullOrEmpty(ContentName)) { return Addons.AdminSite.Controllers.AdminUIController.getRecordEditLink(core, ContentName, csGetInteger("ID"), GenericController.encodeBoolean(allowCut), csGetText("Name"), core.session.isEditing(ContentName)); }
                return string.Empty;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        public string csGetRecordEditLink(int ignore, bool allowCut) => csGetRecordEditLink(allowCut);
        //
        public string csGetRecordEditLink() => csGetRecordEditLink(false);
        //
        // ====================================================================================================
        //
        public void csSetFormInput(CoreController core, int ignore, string FieldName, string RequestName = "") {
            string LocalRequestName = null;
            string Filename = null;
            string Path = null;
            if (!csOk()) {
                throw new GenericException("ContentSetPointer is invalid, empty, or end-of-file");
            } else if (string.IsNullOrEmpty(FieldName.Trim(' '))) {
                throw new GenericException("FieldName is invalid or blank");
            } else {
                LocalRequestName = RequestName;
                if (string.IsNullOrEmpty(LocalRequestName)) {
                    LocalRequestName = FieldName;
                }
                switch (csGetFieldTypeId(FieldName)) {
                    case Constants._fieldTypeIdBoolean:
                        //
                        // -- Boolean
                        csSet(FieldName, core.docProperties.getBoolean(LocalRequestName));
                        break;
                    case Constants._fieldTypeIdCurrency:
                    case Constants._fieldTypeIdFloat:
                    case Constants._fieldTypeIdInteger:
                    case Constants._fieldTypeIdLookup:
                    case Constants._fieldTypeIdManyToMany:
                        //
                        // -- Numbers
                        csSet(FieldName, core.docProperties.getNumber(LocalRequestName));
                        break;
                    case Constants._fieldTypeIdDate:
                        //
                        // -- Date
                        csSet(FieldName, core.docProperties.getDate(LocalRequestName));
                        break;
                    case Constants._fieldTypeIdFile:
                    case Constants._fieldTypeIdFileImage:
                        //
                        // -- upload file
                        Filename = core.docProperties.getText(LocalRequestName);
                        if (!string.IsNullOrEmpty(Filename)) {
                            Path = getFieldFilename(FieldName, Filename, "", csGetFieldTypeId(FieldName));
                            csSet(FieldName, Path);
                            Path = GenericController.vbReplace(Path, "\\", "/");
                            Path = GenericController.vbReplace(Path, "/" + Filename, "");
                            core.cdnFiles.upload(LocalRequestName, Path, ref Filename);
                        }
                        break;
                    default:
                        //
                        // -- text files
                        csSet(FieldName, core.docProperties.getText(LocalRequestName));
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
                string ContentName = csGetContentName();
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
                        this.close();
                        if (this.insert("Content Watch Lists")) {
                            this.csSet("name", ListName);
                        }
                    }
                    this.close();
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
                    var CDef = Models.Domain.MetaModel.createByUniqueName(core, contentName);
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
                            this.contentName = contentName;
                            this.dataSource = CDef.dataSourceName;
                            this.contentMeta = CDef;
                            this.sqlSelectFieldList = sqlSelectFieldList;
                            this.sqlSource = sql;
                            return true;
                        }
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
        //            var CDef = Models.Domain.MetaModel.createByUniqueName(core, ContentName);
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
                this.contentName = "";
                this.pageNumber = pageNumber;
                this.pageSize = (pageSize);
                this.dataSource = dataSourceName;
                this.sqlSelectFieldList = "";
                this.sqlSource = sql;
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
            using (var csXfer = new CsModel(core)) {

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

