
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
        private bool isOpen;
        /// <summary>
        /// The date/time this csv_ContentSet was last used
        /// </summary>
        private DateTime lastUsed;
        /// <summary>
        /// Can be used to write to the record. True if opened with a content definition.
        /// </summary>
        private bool createdByQuery;
        /// <summary>
        /// Can be read. True if created with open() or openSql(), false if created with openForUpdate()
        /// </summary>
        private bool readable;
        /// <summary>
        /// true if it was created here
        /// </summary>
        //private bool newRecord;
        ///// <summary>
        ///// ***** should be removed. This should be the same as metaData.name
        ///// </summary>
        private string contentName;
        /// <summary>
        /// If opened with a content name, this is that content's metadata
        /// </summary>
        private Models.Domain.MetaModel contentMeta;
        /// <summary>
        /// data that needs to be written to the database on the next save
        /// </summary>
        private Dictionary<string, string> writeCache;
        /// <summary>
        /// Set when CS is opened and if a save happens
        /// </summary>
        //private bool isModified;
        ///// <summary>
        ///// The read object, null when empty otherwise it needs to be disposed
        ///// </summary>
        private DataTable dt = null;
        /// <summary>
        /// Holds the SQL that created the result set
        /// </summary>
        private string sqlSource;
        /// <summary>
        /// The Datasource of the SQL that created the result set
        /// </summary>
        private string dataSourceName;
        /// <summary>
        /// Number of records in a cache page
        /// </summary>
        private int pageSize;
        /// <summary>
        /// The Page that this result starts with
        /// </summary>
        private int pageNumber;
        /// <summary>
        /// 
        /// </summary>
        private int fieldPointer;
        /// <summary>
        /// 1-D array of the result field names
        /// </summary>
        private string[] fieldNames;
        /// <summary>
        /// number of columns in the fieldNames and readCache
        /// </summary>
        private int resultColumnCount;
        ///// <summary>
        ///// readCache is at the last record
        ///// </summary>
        // private bool resultEOF;
        /// <summary>
        /// 2-D array of the result rows/columns
        /// </summary>
        private string[,] readCache;
        /// <summary>
        /// number of rows in the readCache
        /// </summary>
        private int readCacheRowCnt;
        /// <summary>
        /// Pointer to the current result row, first row is 0, BOF is -1
        /// </summary>
        private int readCacheRowPtr;
        /// <summary>
        /// comma delimited list of all fields selected, in the form table.field
        /// </summary>
        private string sqlSelectFieldList;
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
                    save(asyncSave);
                    readCache = new string[,] { { }, { } };
                    writeCache = new Dictionary<string, string>();
                    resultColumnCount = 0;
                    readCacheRowCnt = 0;
                    readCacheRowPtr = -1;
                    //resultEOF = true;
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
        //====================================================================================================
        /// <summary>
        /// close the read data set, save what needs to be written
        /// </summary>
        public void close() => close(false);
        //
        //====================================================================================================
        /// <summary>
        /// Copy the current dataset to a target set. Target must be already opened or inserted
        /// </summary>
        /// <param name="destination"></param>
        public void copyRecord(CsModel destination) {
            try {
                if (!isOpen) { throw new ArgumentException("source dataset is not valid"); }
                if (!destination.isOpen) { throw new ArgumentException("destination dataset is not valid"); }
                if (contentMeta == null) { throw new ArgumentException("copyRecord requires the source and destination datasets be created from an open or insert, not a sql."); }
                if (destination.contentMeta == null) { throw new ArgumentException("copyRecord requires the source and destination datasets be created from an open or insert, not a sql."); }
                //
                foreach (var kvp in contentMeta.fields) {
                    MetaFieldModel field = kvp.Value;
                    switch (field.nameLc) {
                        case "id":
                            break;
                        default:
                            //
                            // ----- fields to copy
                            //

                            switch (field.fieldTypeId) {
                                case Constants._fieldTypeIdRedirect:
                                case Constants._fieldTypeIdManyToMany:
                                    //
                                    // -- no data saved for these field types
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
                                            destination.set(field.nameLc, DestFilename);
                                            core.fileCdn.copyFile(SourceFilename, DestFilename);
                                        }
                                    }
                                    break;
                                case Constants._fieldTypeIdFileText:
                                case Constants._fieldTypeIdFileHTML:
                                    //
                                    // ----- private file
                                    {
                                        string SourceFilename = getFieldFilename(field.nameLc, "", contentMeta.name, field.fieldTypeId);
                                        if (!string.IsNullOrEmpty(SourceFilename)) {
                                            string DestFilename = destination.getFieldFilename(field.nameLc, "", destination.contentName, field.fieldTypeId);
                                            destination.set(field.nameLc, DestFilename);
                                            core.filePrivate.copyFile(SourceFilename, DestFilename);
                                        }
                                    }
                                    break;
                                default:
                                    //
                                    // ----- value
                                    destination.set(field.nameLc, getRawData(field.nameLc));
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
                if (!ok()) { throw new ArgumentException("Cannot delete because data set is empty or at End-Of-file"); }
                if (!createdByQuery) { throw new ArgumentException("Cannot delete because data set was created with a query."); }
                if (contentMeta == null) { throw new ArgumentException("Cannot delete because meta data is not valid."); }
                if (string.IsNullOrEmpty(contentMeta.name)) { throw new ArgumentException("Cannot delete because meta data name is blank."); }
                //
                // -- delete any files (if filename is part of select)
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
                                    // cdn file
                                    string Filename = getRawData(field.nameLc);
                                    if (!string.IsNullOrEmpty(Filename)) { core.fileCdn.deleteFile(Filename); }
                                    break;
                                }
                            case Constants._fieldTypeIdFileText:
                            case Constants._fieldTypeIdFileHTML: {
                                    //
                                    // private file
                                    string Filename = getRawData(field.nameLc);
                                    if (!string.IsNullOrEmpty(Filename)) { core.filePrivate.deleteFile(Filename); }
                                    break;
                                }
                        }
                    }
                }
                //
                int recordId = getInteger("ID");
                core.db.deleteTableRecord(recordId, contentMeta.tableName, contentMeta.dataSourceName);
                MetaController.deleteContentRules(core, contentMeta.id, recordId);
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
        /// <returns></returns>
        public bool insert(string contentName) {
            try {
                if (isOpen) { close(); }
                if (string.IsNullOrEmpty(contentName.Trim())) { throw new ArgumentException("Cannot insert new record because Content name is blank."); }
                var meta = MetaModel.createByUniqueName(core, contentName);
                if (meta == null) { throw new GenericException("Cannot insert new record because Content meta data cannot be found."); }
                if (meta.id <= 0) { throw new GenericException("Cannot insert new record because Content meta data is not valid."); }
                //
                // create default record in Live table
                SqlFieldListClass sqlList = new SqlFieldListClass();
                foreach (KeyValuePair<string, Models.Domain.MetaFieldModel> keyValuePair in meta.fields) {
                    MetaFieldModel field = keyValuePair.Value;
                    if ((!string.IsNullOrEmpty(field.nameLc)) && (!string.IsNullOrEmpty(field.defaultValue))) {
                        switch (field.nameLc) {
                            case "createkey":
                            case "dateadded":
                            case "createdby":
                            case "contentcontrolid":
                            case "id":
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
                string sqlGuid = DbController.encodeSQLText(getGUID());
                string sqlDateAdded = DbController.encodeSQLDate(DateTime.Now);
                sqlList.add("ccguid", sqlGuid);
                sqlList.add("DATEADDED", sqlDateAdded);
                sqlList.add("CONTENTCONTROLID", DbController.encodeSQLNumber(meta.id));
                sqlList.add("CREATEDBY", DbController.encodeSQLNumber(userId));
                core.db.insertTableRecord(meta.dataSourceName, meta.tableName, sqlList);
                //
                // ----- Get the record back so we can use the ID
                return open(contentName, "(ccguid=" + sqlGuid + ")And(DateAdded=" + sqlDateAdded + ")", "ID DESC", false, userId);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the field value as boolean type
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public bool getBoolean(string fieldName) {
            return encodeBoolean(getRawData(fieldName));
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the field value as an integer type
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public int getInteger(string fieldName) {
            return encodeInteger(getRawData(fieldName));
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the field data as a double
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public double getNumber(string fieldName) {
            return encodeNumber(getRawData(fieldName));
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the field data in DateTime format
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public DateTime getDate(string fieldName) {
            return encodeDate(getRawData(fieldName));
        }
        //
        //========================================================================
        /// <summary>
        /// Return a text field rendered for an Html page (executed embeded addons). For other rendering cases, use getText() and cp.utils.render methods.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public string getTextEncoded(string FieldName) {
            if (!isOpen) { throw new ArgumentException("source dataset is not valid"); }
            if (contentMeta == null) { throw new ArgumentException("Cannot return rendered html because dataset was created with a sql query."); }
            return ActiveContentController.renderHtmlForWeb(core, getText(FieldName), contentMeta.name, getInteger("id"), core.session.user.id, "", 0, CPUtilsBaseClass.addonContext.ContextPage);
        }
        //
        //========================================================================
        /// <summary>
        /// initialize a cs
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        private void init() {
            isOpen = true;
            //newRecord = true;
            contentName = "";
            contentMeta = null;
            dataSourceName = "";
            dt = null;
            fieldNames = null;
            fieldPointer = 0;
            //isModified = false;
            lastUsed = DateTime.Now;
            userId = core.session.user.id;
            pageNumber = 0;
            pageSize = 0;
            readCache = null;
            readCacheRowCnt = 0;
            readCacheRowPtr = 0;
            resultColumnCount = 0;
            //resultEOF = true;
            sqlSelectFieldList = "";
            sqlSource = "";
            createdByQuery = false;
            writeCache = null;
        }
        //
        //========================================================================
        /// <summary>
        /// Move to the next row in the dataset
        /// </summary>
        /// <param name="asyncSave"></param>
        public void goNext(bool asyncSave) {
            try {
                if (!ok()) { throw new GenericException("Dataset is not valid."); }
                if (!this.readable) { throw new GenericException("Cannot move to next row because dataset is not readable."); }
                save(asyncSave);
                this.writeCache = new Dictionary<string, string>();
                this.readCacheRowPtr = this.readCacheRowPtr + 1;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        public void goNext() => goNext(false);
        //
        //========================================================================
        /// <summary>
        /// Move to the first row in the dataset
        /// </summary>
        /// <param name="asyncSave"></param>
        public void goFirst(bool asyncSave) {
            try {
                save(asyncSave);
                this.writeCache = new Dictionary<string, string>();
                this.readCacheRowPtr = 0;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        public void goFirst() => goFirst(false);
        //
        //========================================================================
        /// <summary>
        /// The value read directly from the field in the Db, or from the write cache.
        /// For textFilenames, this is the filename of the content.
        /// For lookups, this is the id
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string getRawData(string fieldName) {
            string returnValue = "";
            try {
                string fieldNameTrim = fieldName.Trim();
                if (!ok()) {
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
                                if (this.createdByQuery) {
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
        //========================================================================
        /// <summary>
        /// get the first fieldname in the CS, Returns null if there are no more
        /// </summary>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public string getFirstFieldName() {
            try {
                if (!ok()) { throw new GenericException("data set is not valid"); }
                this.fieldPointer = 0;
                return getNextFieldName();
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// get the next fieldname in the CS, Returns null if there are no more
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        public string getNextFieldName() {
            try {
                if (!ok()) { throw new GenericException("data set is not valid"); }
                while (this.fieldPointer < this.resultColumnCount) {
                    if (!string.IsNullOrWhiteSpace(this.fieldNames[this.fieldPointer])) { return this.fieldNames[this.fieldPointer]; }
                    this.fieldPointer += 1;
                }
                return "";
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// get the type of a field within a csv_ContentSet
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public int getFieldTypeId(string fieldName) {
            try {
                if (ok() && this.createdByQuery && !string.IsNullOrEmpty(this.contentMeta.name)) { return this.contentMeta.fields[fieldName.ToLowerInvariant()].fieldTypeId; }
                return 0;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// get the caption of a field within a csv_ContentSet
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string getFieldCaption(string fieldName) {
            string returnResult = "";
            try {
                if (ok() && (this.createdByQuery) && (!string.IsNullOrEmpty(this.contentMeta.name))) {
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
        public string getSelectFieldList() {
            try {
                if (ok()) { return string.Join(",", this.fieldNames); }
                return string.Empty;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// get the caption of a field within a csv_ContentSet
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public bool isFieldSupported(string fieldName) {
            try {
                if (string.IsNullOrEmpty(fieldName)) { throw new ArgumentException("Field name cannot be blank"); }
                if (!ok()) { throw new ArgumentException("dataset is not valid"); }
                return GenericController.isInDelimitedString(getSelectFieldList(), fieldName, ",");
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// get the filename that backs the field specified. only valid for fields of TextFile and File type.
        /// Attempt to read the filename from the field
        /// if no filename, attempt to create it from the tablename-recordid
        /// if no recordid, create filename from a random
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="originalFilename"></param>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public string getFieldFilename(string fieldName, string originalFilename, string contentName, int fieldTypeId) {
            string returnFilename = "";
            try {
                string TableName = null;
                int RecordID = 0;
                string fieldNameUpper = null;
                int LenOriginalFilename = 0;
                int LenFilename = 0;
                int Pos = 0;
                //
                if (!ok()) {
                    throw new ArgumentException("the current data set is not valid.");
                } else if (string.IsNullOrEmpty(fieldName)) {
                    throw new ArgumentException("Fieldname Is blank");
                } else {
                    fieldNameUpper = GenericController.vbUCase(fieldName.Trim(' '));
                    returnFilename = getRawData(fieldNameUpper);
                    if (!string.IsNullOrEmpty(returnFilename)) {
                        //
                        // ----- A filename came from the record
                        //
                        if (!string.IsNullOrEmpty(originalFilename)) {
                            //
                            // ----- there was an original filename, make sure it matches the one in the record
                            //
                            LenOriginalFilename = originalFilename.Length;
                            LenFilename = returnFilename.Length;
                            Pos = (1 + LenFilename - LenOriginalFilename);
                            if (Pos <= 0) {
                                //
                                // Original Filename changed, create a new 
                                //
                                returnFilename = "";
                            } else if (returnFilename.Substring(Pos - 1) != originalFilename) {
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
                                    RecordID = getInteger("ID");
                                    break;
                                }
                            }
                        }
                        //
                        // ----- Get tablename
                        //
                        if (this.createdByQuery) {
                            //
                            // Get tablename from Content Definition
                            //
                            contentName = this.contentMeta.name;
                            TableName = this.contentMeta.tableName;
                        } else if (!string.IsNullOrEmpty(contentName)) {
                            //
                            // CS is SQL-based, use the contentname
                            //
                            TableName = MetaController.getContentTablename(core, contentName);
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
                            if (string.IsNullOrEmpty(contentName)) {
                                if (string.IsNullOrEmpty(originalFilename)) {
                                    fieldTypeId = Constants.fieldTypeIdText;
                                } else {
                                    fieldTypeId = Constants.fieldTypeIdFile;
                                }
                            } else if (this.createdByQuery) {
                                //
                                // -- get from metadata
                                fieldTypeId = this.contentMeta.fields[fieldName.ToLowerInvariant()].fieldTypeId;
                            } else {
                                //
                                // -- else assume text
                                if (string.IsNullOrEmpty(originalFilename)) {
                                    fieldTypeId = Constants.fieldTypeIdText;
                                } else {
                                    fieldTypeId = Constants.fieldTypeIdFile;
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(originalFilename)) {
                            returnFilename = FileController.getVirtualRecordUnixPathFilename(TableName, fieldName, RecordID, fieldTypeId);
                        } else {
                            returnFilename = FileController.getVirtualRecordUnixPathFilename(TableName, fieldName, RecordID, originalFilename);
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
        public string getFieldFilename(string fieldName, string originalFilename) => getFieldFilename(fieldName, originalFilename, "", 0);
        //
        //====================================================================================================
        /// <summary>
        /// if the field uses an underlying filename, use this method to set that filename. The content for the field will switch to that contained by the new file
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="fieldName"></param>
        /// <param name="filename"></param>
        public void setFilename(string fieldName, string filename) {
            try {
                if (!ok()) { throw new ArgumentException("dataset is not valid"); }
                if (string.IsNullOrEmpty(fieldName)) { throw new ArgumentException("fieldName cannot be blank"); }
                if (!this.createdByQuery) { throw new GenericException("Cannot set fields for a dataset based on a query."); }
                if (this.contentMeta == null) { throw new GenericException("Cannot set fields for a dataset based on a query."); }
                if (this.contentMeta.fields == null) { throw new GenericException("The dataset contains no fields."); }
                if (!this.contentMeta.fields.ContainsKey(fieldName.ToLowerInvariant())) { throw new GenericException("The dataset does not contain the field specified [" + fieldName.ToLowerInvariant() + "]."); }
                if (this.writeCache.ContainsKey(fieldName.ToLowerInvariant())) {
                    this.writeCache[fieldName.ToLowerInvariant()] = filename;
                    return;
                }
                this.writeCache.Add(fieldName.ToLowerInvariant(), filename);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================
        /// <summary>
        /// Save text to a field configured to use an external file. Use set() except when the data set is opened with a query.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="copy"></param>
        /// <param name="contentName"></param>
        public void setTextFile(string fieldName, string copy, string contentName) {
            try {
                if (!ok()) { throw new ArgumentException("dataset is not valid"); }
                if (string.IsNullOrEmpty(fieldName)) { throw new ArgumentException("field Name cannot be blank"); }
                if (string.IsNullOrEmpty(contentName)) { throw new ArgumentException("content Name cannot be blank"); }
                if (!createdByQuery) { throw new GenericException("Cannot save a data set created by a query."); }
                string OldFilename = getText(fieldName);
                string Filename = getFieldFilename(fieldName, "", contentName, Constants.fieldTypeIdFileText);
                if (OldFilename != Filename) {
                    //
                    // Filename changed, mark record changed
                    //
                    core.fileCdn.saveFile(Filename, copy);
                    set(fieldName, Filename);
                } else {
                    string OldCopy = core.fileCdn.readFileText(Filename);
                    if (OldCopy != copy) {
                        //
                        // copy changed, mark record changed
                        //
                        core.fileCdn.saveFile(Filename, copy);
                        set(fieldName, Filename);
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
        /// <returns></returns>
        public bool ok() {
            try {
                //
                // -- opened with openForUpdate. can be written but not read
                if (this.createdByQuery & !this.readable) { return this.isOpen; }
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
        /// <returns></returns>
        public string getSql() {
            try {
                if (!ok()) { throw new ArgumentException("the dataset is not valid"); }
                return this.sqlSource;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// returns the field value as a string type.
        /// Use for fields having Text types.
        /// For data sets created with queries, the raw data is returned
        /// For fileType fields the content of the file is returned. To get the filename, use GetFilename()
        /// For lookup type fields, the id is converted into the text equivalent, the name of the foreign key or the indexed string in a select list. To get the lookup key, use getInteger().
        /// For date types, a blank is returned for minDate, and the time is removed from the string if time=12:00:00am
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        //
        public string getText(string fieldName) {
            string result = "";
            try {
                if (!ok()) { throw new ArgumentException("Cannot read field because the dataset is not valid or end-of-file."); }
                if (string.IsNullOrEmpty(fieldName.Trim())) { throw new ArgumentException("Cannot read field because the fieldname cannot be blank."); }
                if (!this.createdByQuery) { return getRawData(fieldName); }
                if (!this.contentMeta.fields.ContainsKey(fieldName.ToLowerInvariant())) { return getRawData(fieldName); }
                var field = this.contentMeta.fields[fieldName.ToLowerInvariant()];
                //
                // -- many-to-many field, special case, return record id list
                if (field.fieldTypeId == Constants.fieldTypeIdManyToMany) {
                    int RecordID = 0;
                    string DbTable = null;
                    string ContentName = null;
                    if (this.contentMeta.fields.ContainsKey("id")) {
                        RecordID = GenericController.encodeInteger(getRawData("id"));
                        ContentName = MetaController.getContentNameByID(core, field.manyToManyRuleContentID);
                        DbTable = MetaController.getContentTablename(core, ContentName);
                        using (DataTable dt = core.db.executeQuery("Select " + field.ManyToManyRuleSecondaryField + " from " + DbTable + " where " + field.ManyToManyRulePrimaryField + "=" + RecordID)) {
                            if (DbController.isDataTableOk(dt)) {
                                foreach (DataRow dr in dt.Rows) {
                                    result += "," + dr[0].ToString();
                                }
                                result = result.Substring(1);
                            }
                        }
                    }
                    return result;
                }
                //
                // -- redirect field, special case, no data
                if (field.fieldTypeId == fieldTypeIdRedirect) { return string.Empty; }
                string rawData = getRawData(fieldName);
                if (IsNull(rawData)) { return string.Empty; }
                switch (field.fieldTypeId) {
                    case Constants._fieldTypeIdBoolean:
                        //
                        // -- boolean
                        if (GenericController.encodeBoolean(rawData)) { return "Yes"; }
                        return "No";
                    case Constants._fieldTypeIdDate:
                        //
                        // -- DateTime
                        DateTime dateValue = GenericController.encodeDate(rawData);
                        if (dateValue == DateTime.MinValue ) { return string.Empty; }
                        if (dateValue.Equals(dateValue.Date)) { return dateValue.ToString("d"); }
                        return dateValue.ToString();
                    case Constants._fieldTypeIdLookup:
                        //
                        // -- Lookup
                        if (!rawData.IsNumeric()) { return string.Empty; }
                        if (field.lookupContentID>0 ) {
                            string LookupContentName = MetaController.getContentNameByID(core, field.lookupContentID);
                            if (!string.IsNullOrEmpty(LookupContentName)) {
                                //
                                // -- First try Lookup Content
                                using (var cs = new CsModel(core)) {
                                    if (cs.open(LookupContentName, "ID=" + DbController.encodeSQLNumber(GenericController.encodeInteger(rawData)), "", true, 0, "name", 1)) {
                                        return cs.getText("name");
                                    }
                                }
                            }
                            return string.Empty;
                        }
                        if (!string.IsNullOrEmpty(field.lookupList)) {
                            //
                            // -- lookup list, index is 1-based (to be consistent with Db), but array is 0-based, so adjust
                            int FieldValueInteger = GenericController.encodeInteger(rawData) - 1;
                            if (FieldValueInteger >= 0) {
                                string[] lookups = field.lookupList.Split(',');
                                if (lookups.GetUpperBound(0) >= FieldValueInteger) {
                                    return lookups[FieldValueInteger];
                                }
                            }
                        }
                        return string.Empty;
                    case Constants._fieldTypeIdMemberSelect:
                        //
                        // -- member select
                        if (rawData.IsNumeric()) { return MetaController.getRecordName(core, "people", GenericController.encodeInteger(rawData)); }
                        return string.Empty;
                    case Constants._fieldTypeIdCurrency:
                        //
                        // -- currency
                        if (rawData.IsNumeric()) { return rawData.ToString(); }
                        return string.Empty;
                    case Constants._fieldTypeIdFileText:
                    case Constants._fieldTypeIdFileHTML:
                    case Constants._fieldTypeIdFileCSS:
                    case Constants._fieldTypeIdFileXML:
                    case Constants._fieldTypeIdFileJavascript:
                        //
                        // -- cdn file
                        return core.fileCdn.readFileText(GenericController.encodeText(rawData));
                    case Constants._fieldTypeIdText:
                    case Constants._fieldTypeIdLongText:
                    case Constants._fieldTypeIdHTML:
                        //
                        // -- text saved in database
                        return rawData;
                    case Constants._fieldTypeIdFile:
                    case Constants._fieldTypeIdFileImage:
                    case Constants._fieldTypeIdLink:
                    case Constants._fieldTypeIdResourceLink:
                    case Constants._fieldTypeIdAutoIdIncrement:
                    case Constants._fieldTypeIdFloat:
                    case Constants._fieldTypeIdInteger:
                        //
                        // -- other types returned in string format
                        return GenericController.encodeText(rawData);
                    case Constants._fieldTypeIdRedirect:
                    case Constants._fieldTypeIdManyToMany:
                        //
                        // This case is covered before the select - but leave this here as safety net
                        return string.Empty;
                    default:
                        //
                        // Unknown field type
                        //
                        throw new GenericException("Cannot use field [" + fieldName + "] because the fieldType [" + field.fieldTypeId + "] is not valid.");
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Saves the value for the field. If the field uses a file, the content is saved to the file using the fields filename. To set a file-based field's filename, use setFieldFilename
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="fieldName"></param>
        /// <param name="FieldValue"></param>
        //
        public void set(string fieldName, string fieldValue) {
            try {
                if (!ok()) { throw new ArgumentException("dataset is not valid or End-Of-file."); }
                if (string.IsNullOrEmpty(fieldName.Trim())) { throw new ArgumentException("fieldName cannnot be blank"); }
                if (!this.createdByQuery) { throw new GenericException("Cannot update a contentset created from a sql query."); }
                if (this.contentMeta == null) { throw new GenericException("Cannot update a contentset created with meta data."); }
                if (string.IsNullOrEmpty(this.contentMeta.name)) { throw new GenericException("Cannot update a contentset created with invalid meta data."); }
                string FieldNameLc = fieldName.Trim(' ').ToLowerInvariant();
                if (!this.contentMeta.fields.ContainsKey(FieldNameLc)) { throw new ArgumentException("The field [" + fieldName + "] could Not be found In content [" + this.contentMeta.name + "]"); }
                MetaFieldModel field = this.contentMeta.fields[FieldNameLc];
                string rawValueForDb = fieldValue ?? string.Empty;
                bool SetNeeded = false;
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
                        string pathFilenameOriginal = getRawData(field.nameLc);
                        PathFilename = pathFilenameOriginal;
                        string BlankTest = null;
                        BlankTest = rawValueForDb;
                        BlankTest = GenericController.vbReplace(BlankTest, " ", "");
                        BlankTest = GenericController.vbReplace(BlankTest, "\r", "");
                        BlankTest = GenericController.vbReplace(BlankTest, "\n", "");
                        BlankTest = GenericController.vbReplace(BlankTest, "\t", "");
                        if (string.IsNullOrEmpty(BlankTest)) {
                            if (!string.IsNullOrEmpty(PathFilename)) {
                                core.fileCdn.deleteFile(PathFilename);
                                PathFilename = "";
                            }
                        } else {
                            if (string.IsNullOrEmpty(PathFilename)) {
                                PathFilename = getFieldFilename(field.nameLc, "", this.contentName, field.fieldTypeId);
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
                                    string fileNameNoExt = PathFilename.Left(Pos - 1);
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
                                        string fileName = fileNameNoExt + ".r" + FilenameRev + "." + FileExt;
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
                                core.fileCdn.deleteFile(pathFilenameOriginal);
                            }
                            core.fileCdn.saveFile(PathFilename, rawValueForDb);
                        }
                        rawValueForDb = PathFilename;
                        SetNeeded = true;
                        break;
                    case Constants._fieldTypeIdBoolean:
                        //
                        // Boolean - sepcial case, block on typed GetAlways set
                        if (GenericController.encodeBoolean(rawValueForDb) != getBoolean(field.nameLc)) {
                            SetNeeded = true;
                        }
                        break;
                    case Constants._fieldTypeIdText:
                        //
                        // Set if text of value changes
                        //
                        if (GenericController.encodeText(rawValueForDb) != getText(field.nameLc)) {
                            SetNeeded = true;
                            if (rawValueForDb.Length > 255) {
                                LogController.handleError(core, new GenericException("Text length too long saving field [" + fieldName + "], length [" + rawValueForDb.Length + "], but max for Text field is 255. Save will be attempted"));
                            }
                        }
                        break;
                    case Constants._fieldTypeIdLongText:
                    case Constants._fieldTypeIdHTML:
                        //
                        // Set if text of value changes
                        //
                        if (GenericController.encodeText(rawValueForDb) != getText(field.nameLc)) {
                            SetNeeded = true;
                            if (rawValueForDb.Length > 65535) {
                                LogController.handleError(core, new GenericException("Text length too long saving field [" + fieldName + "], length [" + rawValueForDb.Length + "], but max for LongText and Html is 65535. Save will be attempted"));
                            }
                        }
                        break;
                    default:
                        //
                        // Set if text of value changes
                        //
                        if (GenericController.encodeText(rawValueForDb) != getText(field.nameLc)) {
                            SetNeeded = true;
                        }
                        break;
                }
                if (SetNeeded) {
                    //
                    // ----- set the new value into the row buffer
                    if (this.writeCache.ContainsKey(field.nameLc)) {
                        this.writeCache[field.nameLc] = rawValueForDb.ToString();
                    } else {
                        this.writeCache.Add(field.nameLc, rawValueForDb.ToString());
                    }
                    this.lastUsed = DateTime.Now;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        public void set(string fieldName, int fieldValue) => set(fieldName, fieldValue.ToString());
        //
        public void set(string fieldName, double fieldValue) => set(fieldName, fieldValue.ToString());
        //
        public void set(string fieldName, DateTime fieldValue) => set(fieldName, fieldValue.ToString());
        //
        public void set(string fieldName, bool fieldValue) => set(fieldName, fieldValue.ToString());
        //
        //========================================================================
        /// <summary>
        /// rollback, or undo the changes to the current row
        /// </summary>
        /// <param name="CSPointer"></param>
        public void rollBack() {
            try {
                if (!ok()) { throw new ArgumentException("dataset is not valid"); }
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
        public void save(bool asyncSave, bool blockClearCache) {
            try {
                if (!ok()) { return; }
                if (this.writeCache.Count == 0) { return; }
                if (!(this.createdByQuery)) { throw new ArgumentException("The dataset cannot be updated because it was created with a query and not a content table."); }
                {
                    var contentSet = this;
                    if (this.contentMeta == null) {
                        //
                        // -- dataset not updatable
                        throw new ArgumentException("The dataset cannot be updated because it was not created from a valid content table.");
                    } else {
                        //
                        // -- get id from read cache or write cache. if id=0 save is insert, else save is update
                        int id = getInteger("ID");
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
        public void save(bool asyncSave) => save(asyncSave, false);
        //
        public void save() => save(false, false);
        //
        //=====================================================================================================
        /// <summary>
        /// Initialize the csv_ContentSet Result Cache when it is first opened
        /// </summary>
        /// <param name="ignore"></param>
        private void initAfterOpen(int ignore) {
            try {
                this.resultColumnCount = 0;
                this.readCacheRowCnt = 0;
                this.readCacheRowPtr = -1;
                //this.resultEOF = true;
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
        private bool eof() {
            return (this.readCacheRowPtr >= this.readCacheRowCnt);
        }
        //
        //========================================================================
        /// <summary>
        /// Opens a csv_ContentSet with the Members of a group
        /// </summary>
        /// <param name="groupList"></param>
        /// <param name="sqlCriteria"></param>
        /// <param name="sortFieldList"></param>
        /// <param name="activeOnly"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public bool openGroupUsers(List<string> groupList, string sqlCriteria, string sortFieldList, bool activeOnly, int pageSize, int pageNumber) {
            try {
                DateTime rightNow = DateTime.Now;
                string sqlRightNow = DbController.encodeSQLDate(rightNow);
                //
                if (pageNumber == 0) {
                    pageNumber = 1;
                }
                if (pageSize == 0) {
                    pageSize = DbController.pageSizeDefault;
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
                    if (activeOnly) {
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
                    if (!string.IsNullOrEmpty(sortFieldList)) {
                        SQL += " Order by " + sortFieldList;
                    }
                    return openSql(SQL, "Default", pageSize, pageNumber);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return false;
        }
        //
        public bool openGroupUsers(List<string> groupList, string sqlCriteria, string sortFieldList, bool activeOnly, int pageSize)
            => openGroupUsers(groupList, sqlCriteria, sortFieldList, activeOnly, pageSize, 1);
        //
        public bool openGroupUsers(List<string> groupList, string sqlCriteria, string sortFieldList, bool activeOnly)
            => openGroupUsers(groupList, sqlCriteria, sortFieldList, activeOnly, 9999, 1);
        //
        public bool openGroupUsers(List<string> groupList, string sqlCriteria, string sortFieldList)
            => openGroupUsers(groupList, sqlCriteria, sortFieldList, true, 9999, 1);
        //
        public bool openGroupUsers(List<string> groupList, string sqlCriteria)
            => openGroupUsers(groupList, sqlCriteria, "", true, 9999, 1);
        //
        public bool openGroupUsers(List<string> groupList)
            => openGroupUsers(groupList, "", "", true, 9999, 1);
        //
        //========================================================================
        /// <summary>
        /// Return an array with all the data
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        public string[,] getRows() {
            return this.readCache;
        }
        //
        //========================================================================
        /// <summary>
        /// get the row count of the dataset
        /// </summary>
        /// <returns></returns>
        public int getRowCount() {
            if (ok()) { return this.readCacheRowCnt; }
            return 0;
        }
        //
        //========================================================================
        /// <summary>
        /// return the content name of a csv_ContentSet
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        //
        public string getContentName() {
            try {
                return this.contentName;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Return an edit link for the current record
        /// </summary>
        /// <param name="allowCut"></param>
        /// <returns></returns>
        public string getRecordEditLink(bool allowCut) {
            try {
                if (!ok()) { throw (new GenericException("Cannot create edit link because data set is not valid.")); }
                string ContentName = MetaController.getContentNameByID(core, getInteger("contentcontrolid"));
                if (!string.IsNullOrEmpty(ContentName)) { return Addons.AdminSite.Controllers.AdminUIController.getRecordEditLink(core, ContentName, getInteger("ID"), allowCut, getText("Name"), core.session.isEditing(ContentName)); }
                return string.Empty;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        public string getRecordEditLink() => getRecordEditLink(false);
        //
        // ====================================================================================================
        //
        public void setFormInput(CoreController core, string FieldName, string RequestName = "") {
            string LocalRequestName = null;
            string Filename = null;
            string Path = null;
            if (!ok()) {
                throw new GenericException("ContentSetPointer is invalid, empty, or end-of-file");
            } else if (string.IsNullOrEmpty(FieldName.Trim(' '))) {
                throw new GenericException("FieldName is invalid or blank");
            } else {
                LocalRequestName = RequestName;
                if (string.IsNullOrEmpty(LocalRequestName)) {
                    LocalRequestName = FieldName;
                }
                switch (getFieldTypeId(FieldName)) {
                    case Constants._fieldTypeIdBoolean:
                        //
                        // -- Boolean
                        set(FieldName, core.docProperties.getBoolean(LocalRequestName));
                        break;
                    case Constants._fieldTypeIdCurrency:
                    case Constants._fieldTypeIdFloat:
                    case Constants._fieldTypeIdInteger:
                    case Constants._fieldTypeIdLookup:
                    case Constants._fieldTypeIdManyToMany:
                        //
                        // -- Numbers
                        set(FieldName, core.docProperties.getNumber(LocalRequestName));
                        break;
                    case Constants._fieldTypeIdDate:
                        //
                        // -- Date
                        set(FieldName, core.docProperties.getDate(LocalRequestName));
                        break;
                    case Constants._fieldTypeIdFile:
                    case Constants._fieldTypeIdFileImage:
                        //
                        // -- upload file
                        Filename = core.docProperties.getText(LocalRequestName);
                        if (!string.IsNullOrEmpty(Filename)) {
                            Path = getFieldFilename(FieldName, Filename, "", getFieldTypeId(FieldName));
                            set(FieldName, Path);
                            Path = GenericController.vbReplace(Path, "\\", "/");
                            Path = GenericController.vbReplace(Path, "/" + Filename, "");
                            core.fileCdn.upload(LocalRequestName, Path, ref Filename);
                        }
                        break;
                    default:
                        //
                        // -- text files
                        set(FieldName, core.docProperties.getText(LocalRequestName));
                        break;
                }
            }
        }
        //
        public void setFormInput(CoreController core, string FieldName) => setFormInput(core, FieldName, "");
        //
        //====================================================================================================
        /// <summary>
        /// return a list of add links
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ignore"></param>
        /// <param name="PresetNameValueList"></param>
        /// <param name="AllowPaste"></param>
        /// <returns></returns>
        public string getRecordAddLink(CoreController core, string PresetNameValueList, bool AllowPaste) {
            string result = "";
            try {
                if (string.IsNullOrEmpty(this.contentName)) { throw new GenericException("getRecordAddLink was called with a ContentSet that was created with an SQL statement. The function requires a ContentSet opened with an OpenCSContent."); }
                foreach (var AddLink in Addons.AdminSite.Controllers.AdminUIController.getRecordAddLink(core, this.contentName, PresetNameValueList, AllowPaste)) { result += AddLink; }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        public string getRecordAddLink(CoreController core, string PresetNameValueList) => getRecordAddLink(core, PresetNameValueList, false);
        //
        public string getRecordAddLink(CoreController core) => getRecordAddLink(core, "", false);
        //
        //====================================================================================================
        /// <summary>
        /// Open a contentwatch data set for list 'whats new'
        /// </summary>
        /// <param name="core"></param>
        /// <param name="SortFieldList"></param>
        /// <param name="ActiveOnly"></param>
        /// <param name="PageSize"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        public bool openWhatsNew(CoreController core, string SortFieldList = "", bool ActiveOnly = true, int PageSize = 1000, int PageNumber = 1) {
            try {
                return openContentWatchList(core, "What's New", SortFieldList, ActiveOnly, PageSize, PageNumber);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Open a contentwatch data set
        /// </summary>
        /// <param name="core"></param>
        /// <param name="listName"></param>
        /// <param name="sortFieldList"></param>
        /// <param name="sctiveOnly"></param>
        /// <param name="PageSize"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public bool openContentWatchList(CoreController core, string listName, string sortFieldList, bool sctiveOnly, int PageSize, int pageNumber) {
            try {
                sortFieldList = encodeEmpty(sortFieldList, "dateadded").Trim(' ');
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
                    + " WHERE (((ccContentWatchLists.Name)=" + DbController.encodeSQLText(listName) + ")"
                    + "AND ((ccContentWatchLists.Active)<>0)"
                    + "AND ((ccContentWatchListRules.Active)<>0)"
                    + "AND ((ccContentWatch.Active)<>0)"
                    + "AND (ccContentWatch.Link is not null)"
                    + "AND (ccContentWatch.LinkLabel is not null)"
                    + "AND ((ccContentWatch.WhatsNewDateExpires is null)or(ccContentWatch.WhatsNewDateExpires>" + DbController.encodeSQLDate(core.doc.profileStartTime) + "))"
                    + ")"
                    + " ORDER BY " + sortFieldList + ";";
                if (!openSql(SQL, "", PageSize, pageNumber)) {
                    //
                    // Check if listname exists
                    //
                    if (!this.open("Content Watch Lists", "name=" + DbController.encodeSQLText(listName), "ID", false, 0, "ID")) {
                        this.close();
                        if (this.insert("Content Watch Lists")) {
                            this.set("name", listName);
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
        public bool openContentWatchList(CoreController core, string listName, string sortFieldList, bool activeOnly, int pageSize)
            => openContentWatchList(core, listName, sortFieldList, activeOnly, pageSize, 1);
        //
        public bool openContentWatchList(CoreController core, string listName, string sortFieldList, bool activeOnly)
            => openContentWatchList(core, listName, sortFieldList, activeOnly, 1000, 1);
        //
        public bool openContentWatchList(CoreController core, string listName, string sortFieldList)
            => openContentWatchList(core, listName, sortFieldList, true, 1000, 1);
        //
        public bool openContentWatchList(CoreController core, string listName)
            => openContentWatchList(core, listName, "", true, 1000, 1);
        //
        //========================================================================
        //
        public bool openRecord(string contentName, int recordId, string selectFieldList) {
            return open(contentName, "(ID=" + DbController.encodeSQLNumber(recordId) + ")", "", false, core.session.user.id, selectFieldList);
        }
        //
        public bool openRecord(string contentName, int recordId) {
            return open(contentName, "(ID=" + DbController.encodeSQLNumber(recordId) + ")", "", false, core.session.user.id);
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
        public bool open(string contentName, string sqlCriteria, string sqlOrderBy, bool activeOnly, int memberId, string sqlSelectFieldList, int PageSize, int PageNumber) {
            bool result = false;
            try {
                //
                // -- if a previous dataset is open, close it now
                if (isOpen) { close(); }
                //
                // -- verify arguments
                if (string.IsNullOrEmpty(contentName)) { throw new ArgumentException("ContentName cannot be blank"); }
                var contentMetaData = MetaModel.createByUniqueName(core, contentName);
                if (contentMetaData == null) { throw (new GenericException("No content found For [" + contentName + "]")); }
                if (contentMetaData.id <= 0) { throw (new GenericException("No content found For [" + contentName + "]")); }
                if (string.IsNullOrWhiteSpace(contentMetaData.tableName)) { throw (new GenericException("Content metadata [" + contentName + "] does not reference a valid table")); }
                sqlOrderBy = GenericController.encodeEmpty(sqlOrderBy, contentMetaData.defaultSortMethod);
                sqlSelectFieldList = GenericController.encodeEmpty(sqlSelectFieldList, contentMetaData.selectCommaList);
                //
                // -- verify the sortfields are in this table
                if (!string.IsNullOrEmpty(sqlOrderBy)) {
                    string[] SortFields = sqlOrderBy.Split(',');
                    for (int ptr = 0; ptr < SortFields.GetUpperBound(0) + 1; ptr++) {
                        string SortField = SortFields[ptr].ToLowerInvariant();
                        SortField = GenericController.vbReplace(SortField, "asc", "", 1, 99, 1);
                        SortField = GenericController.vbReplace(SortField, "desc", "", 1, 99, 1);
                        SortField = SortField.Trim(' ');
                        if (!contentMetaData.selectList.Contains(SortField)) { throw (new GenericException("The field [" + SortField + "] was used in sqlOrderBy for content [" + contentName + "], but the content does not include this field.")); }
                    }
                }
                //
                // -- fixup the criteria to include the ContentControlID(s) / EditSourceID
                string sqlContentCriteria = contentMetaData.legacyContentControlCriteria;
                if (string.IsNullOrEmpty(sqlContentCriteria)) {
                    sqlContentCriteria = "(1=1)";
                } else {
                    //
                    // -- remove tablename from contentcontrolcriteria - if in workflow mode, and authoringtable is different, this would be wrong, also makes sql smaller, and is not necessary
                    sqlContentCriteria = GenericController.vbReplace(sqlContentCriteria, contentMetaData.tableName + ".", "");
                }
                if (!string.IsNullOrEmpty(sqlCriteria)) { sqlContentCriteria += "and(" + sqlCriteria + ")"; }
                if (activeOnly) { sqlContentCriteria += "and(active<>0)"; }
                //
                // -- Process Select Fields, make sure ContentControlID,ID,Name,Active are included
                sqlSelectFieldList = GenericController.vbReplace(sqlSelectFieldList, "\t", " ");
                while (GenericController.vbInstr(1, sqlSelectFieldList, " ,") != 0) { sqlSelectFieldList = GenericController.vbReplace(sqlSelectFieldList, " ,", ","); }
                while (GenericController.vbInstr(1, sqlSelectFieldList, ", ") != 0) { sqlSelectFieldList = GenericController.vbReplace(sqlSelectFieldList, ", ", ","); }
                //
                // -- add required fields into select list
                if ((!string.IsNullOrEmpty(sqlSelectFieldList)) && (sqlSelectFieldList.IndexOf("*", System.StringComparison.OrdinalIgnoreCase) == -1)) {
                    string testList = ("," + sqlSelectFieldList + ",").ToLower();
                    if (!testList.Contains(",contentcontrolid,")) { sqlSelectFieldList += ",contentcontrolid"; }
                    if (!testList.Contains(",name,")) { sqlSelectFieldList += ",name"; }
                    if (!testList.Contains(",id,")) { sqlSelectFieldList += ",id"; }
                    if (!testList.Contains(",active,")) { sqlSelectFieldList += ",active"; }
                }
                //
                // ----- If no select list, use *
                if (string.IsNullOrEmpty(sqlSelectFieldList)) { sqlSelectFieldList = "*"; }
                string sql = "select " + sqlSelectFieldList + " from " + contentMetaData.tableName + " where (" + sqlContentCriteria + ")" + (string.IsNullOrWhiteSpace(sqlOrderBy) ? "" : " order by " + sqlOrderBy);
                //
                // -- now open the sql
                if (openSql(sql, contentMetaData.dataSourceName, PageSize, PageNumber)) {
                    //
                    // -- correct the status
                    this.readable = true;
                    this.createdByQuery = true;
                    this.contentName = contentName;
                    this.dataSourceName = contentMetaData.dataSourceName;
                    this.contentMeta = contentMetaData;
                    this.sqlSelectFieldList = sqlSelectFieldList;
                    this.sqlSource = sql;
                    return true;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return result;
        }
        //
        public bool open(string contentName, string sqlCriteria, string sqlOrderBy, bool activeOnly, int memberId)
            => open(contentName, sqlCriteria, sqlOrderBy, activeOnly, memberId, "", 9999, 1);
        //
        public bool open(string contentName, string sqlCriteria, string sqlOrderBy, bool activeOnly, int memberId, string sqlSelectFieldList, int PageSize)
            => open(contentName, sqlCriteria, sqlOrderBy, activeOnly, memberId, sqlSelectFieldList, PageSize, 1);
        //
        public bool open(string contentName, string sqlCriteria, string sqlOrderBy, bool activeOnly, int memberId, string sqlSelectFieldList)
            => open(contentName, sqlCriteria, sqlOrderBy, activeOnly, memberId, sqlSelectFieldList, 9999, 1);
        //
        public bool open(string contentName, string sqlCriteria, string sqlOrderBy, bool activeOnly)
            => open(contentName, sqlCriteria, sqlOrderBy, activeOnly, 0, "", 9999, 1);
        //
        public bool open(string contentName, string sqlCriteria, string sqlOrderBy)
            => open(contentName, sqlCriteria, sqlOrderBy, true, 0, "", 9999, 1);
        //
        public bool open(string contentName, string sqlCriteria)
            => open(contentName, sqlCriteria, "", true, 0, "", 9999, 1);
        //
        public bool open(string contentName)
            => open(contentName, "", "", true, 0, "", 9999, 1);
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
        public bool openSql(string sql, string dataSourceName, int pageSize, int pageNumber) {
            try {
                init();
                //this.newRecord = false;
                this.readable = true;
                this.createdByQuery = false;
                this.contentName = "";
                this.pageNumber = pageNumber;
                this.pageSize = (pageSize);
                this.dataSourceName = dataSourceName;
                this.sqlSelectFieldList = "";
                this.sqlSource = sql;
                this.dt = core.db.executeQuery(sql, dataSourceName, pageSize * (pageNumber - 1), pageSize);
                initAfterOpen(0);
                return ok();
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        public bool openSql(string sql, string dataSourceName, int pageSize) => openSql(sql, dataSourceName, pageSize, 1);
        //
        public bool openSql(string sql, string dataSourceName) => openSql(sql, dataSourceName, 9999, 1);
        //
        public bool openSql(string sql) => openSql(sql, "default", 9999, 1);
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
                    close();
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

