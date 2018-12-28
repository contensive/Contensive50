
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
using Contensive.BaseClasses;
using System.Data;
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
        public bool NewRecord;
        /// <summary>
        /// 
        /// </summary>
        public string ContentName;
        /// <summary>
        /// 
        /// </summary>
        public Models.Domain.CDefDomainModel CDef;
        /// <summary>
        /// ID of the member who opened the csv_ContentSet
        /// </summary>
        public int OwnerMemberID;
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> writeCache;
        /// <summary>
        /// Set when CS is opened and if a save happens
        /// </summary>
        public bool IsModified;
        /// <summary>
        /// 
        /// </summary>
        public DataTable dt;
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
        /// insert
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        public bool insert(string ContentName) {
            bool success = false;
            try {
                if (csKey != -1) {
                    core.db.csClose(ref csKey);
                }
                csKey = core.db.csInsertRecord(ContentName, openingMemberID);
                success = core.db.csOk(csKey);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        public bool open(string ContentName, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, string SelectFieldList = "", int pageSize = 0, int PageNumber = 1) {
            bool success = false;
            try {
                if (csKey != -1) {
                    core.db.csClose(ref csKey);
                }
                csKey = core.db.csOpen(ContentName, SQLCriteria, SortFieldList, ActiveOnly, 0, false, false, SelectFieldList, pageSize, PageNumber);
                success = core.db.csOk(csKey);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        //
        // need a new cs method here... openForUpdate( optional id )
        //  creates a cs with no read data and an empty write buffer
        //  read buffer get() is blocked, but you can setField()
        //  cs.save() writes values, if id=0 it does insert, else just update
        //
        public bool openForUpdate(string ContentName, int recordId ) {
            bool success = false;
            try {
                if (csKey != -1) {
                    core.db.csClose(ref csKey);
                }
                csKey = core.db.csOpenForUpdate(ContentName, recordId);
                success = core.db.csOk(csKey);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        public bool openSQL(string sql) {
            bool success = false;
            try {
                if (csKey != -1) {
                    core.db.csClose(ref csKey);
                }
                csKey = core.db.csOpenSql(sql,"Default");
                success = core.db.csOk(csKey);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        public void close(bool asyncSave = false) {
            try {
                if (csKey != -1) {
                    core.db.csClose(ref csKey, asyncSave);
                    csKey = -1;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        public bool getBoolean(string fieldName) {
            return core.db.csGetBoolean(csKey, fieldName);
        }
        //
        //====================================================================================================
        public DateTime getDate(string fieldName) {
            return core.db.csGetDate(csKey, fieldName);
        }
        //
        //====================================================================================================
        public int getInteger(string fieldName) {
            return core.db.csGetInteger(csKey, fieldName);
        }
        //
        //====================================================================================================
        public double getNumber(string fieldName) {
            return core.db.csGetNumber(csKey, fieldName);
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
                result = core.db.csGet(csKey, fieldName);
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
            core.db.csGoNext(csKey);
        }
        //
        //====================================================================================================
        public bool ok() {
            return core.db.csOk(csKey);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, DateTime FieldValue) {
            core.db.csSet(csKey, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, bool FieldValue) {
            core.db.csSet(csKey, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, string FieldValue) {
            core.db.csSet(csKey, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        public void setField(string FieldName, double FieldValue) {
            core.db.csSet(csKey, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field. If the field is backed by a file, the value will be saved to the file
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, int FieldValue) {
            core.db.csSet(csKey, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// if the field uses an underlying filename, use this method to set that filename. The content for the field will switch to that contained by the new file
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="filename"></param>
        public void setFieldFilename( string fieldName, string filename ) {
            core.db.csSetFieldFilename(csKey, fieldName, filename);
        }
        //
        //========================================================================
        //
        public static string getTextEncoded(CoreController core, int CSPointer, string FieldName) {
            string ContentName = "";
            int RecordID = 0;
            if (core.db.csIsFieldSupported(CSPointer, "id") & core.db.csIsFieldSupported(CSPointer, "contentcontrolId")) {
                RecordID = core.db.csGetInteger(CSPointer, "id");
                ContentName = CdefController.getContentNameByID(core, core.db.csGetInteger(CSPointer, "contentcontrolId"));
            }
            string source = core.db.csGet(CSPointer, FieldName);
            return ActiveContentController.renderHtmlForWeb(core, source, ContentName, RecordID, core.session.user.id, "", 0, CPUtilsBaseClass.addonContext.ContextPage);
        }
        //
        //========================================================================
        //
        public string getValue(string FieldName) {
            string result = "";
            try {
                result = core.db.csGetValue(csKey, FieldName);
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
        /// Close a csv_ContentSet
        /// Closes a currently open csv_ContentSet
        /// sets CSPointer to -1
        /// </summary>
        /// <param name="csKey_ignore"></param>
        /// <param name="asyncSave"></param>
        public void csClose(ref int csKey_ignore, bool asyncSave) {
            try {
                if (IsOpen) {
                    csSave(csKey_ignore, asyncSave);
                    readCache = new string[,] { { }, { } };
                    writeCache = new Dictionary<string, string>();
                    resultColumnCount = 0;
                    readCacheRowCnt = 0;
                    readCacheRowPtr = -1;
                    resultEOF = true;
                    IsOpen = false;
                    if (dt != null) {
                        dt.Dispose();
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        public void csClose(ref int csKey_ignore) => csClose(ref csKey_ignore, false);
        //
        public void csClose() {
            int csKey_ignore = 0;
            csClose(ref csKey_ignore, false);
        }
        //
        //========================================================================
        /// <summary>
        /// Opens a dataTable for the table/row definied by the contentname and criteria
        /// </summary>
        /// <param name="ContentName"></param>
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
        //========================================================================
        //
        public int csOpen(string ContentName, string sqlCriteria = "", string sqlOrderBy = "", bool activeOnly = true, int memberId = 0, bool ignorefalse2 = false, bool ignorefalse = false, string sqlSelectFieldList = "", int PageSize = 9999, int PageNumber = 1) {
            int returnCs = -1;
            try {
                if (string.IsNullOrEmpty(ContentName)) {
                    throw new ArgumentException("ContentName cannot be blank");
                } else {
                    var CDef = CDefDomainModel.create(core, ContentName);
                    if (CDef == null) {
                        throw (new GenericException("No content found For [" + ContentName + "]"));
                    } else if (CDef.id <= 0) {
                        throw (new GenericException("No content found For [" + ContentName + "]"));
                    } else {
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
                                if (!CDef.selectList.Contains(SortField)) {
                                    throw (new GenericException("The field [" + SortField + "] was used in sqlOrderBy for content [" + ContentName + "], but the content does not include this field."));
                                }
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
                            throw (new Exception("Content metadata [" + ContentName + "] does not reference a valid table"));
                        } else if (string.IsNullOrEmpty(CDef.dataSourceName)) {
                            throw (new Exception("Table metadata [" + CDef.tableName + "] does not reference a valid datasource"));
                        }
                        //
                        // ----- If no select list, use *
                        if (string.IsNullOrEmpty(sqlSelectFieldList)) {
                            sqlSelectFieldList = "*";
                        }
                        //
                        // ----- Open the csv_ContentSet
                        returnCs = csInit(memberId);
                        {
                            DbController.ContentSetClass contentSet = contentSetStore[returnCs];
                            contentSet.readable = true;
                            contentSet.writeable = true;
                            contentSet.ContentName = ContentName;
                            contentSet.DataSource = CDef.dataSourceName;
                            contentSet.CDef = CDef;
                            contentSet.SelectTableFieldList = sqlSelectFieldList;
                            contentSet.PageNumber = PageNumber;
                            contentSet.PageSize = PageSize;
                            if (contentSet.PageNumber <= 0) {
                                contentSet.PageNumber = 1;
                            }
                            if (contentSet.PageSize < 0) {
                                contentSet.PageSize = Constants.maxLongValue;
                            } else if (contentSet.PageSize == 0) {
                                contentSet.PageSize = pageSizeDefault;
                            }
                            string SQL = null;
                            //
                            if (!string.IsNullOrWhiteSpace(sqlOrderBy)) {
                                SQL = "Select " + sqlSelectFieldList + " FROM " + CDef.tableName + " WHERE (" + sqlContentCriteria + ") ORDER BY " + sqlOrderBy;
                            } else {
                                SQL = "Select " + sqlSelectFieldList + " FROM " + CDef.tableName + " WHERE (" + sqlContentCriteria + ")";
                            }
                            contentSet.Source = SQL;
                            if (!contentSetOpenWithoutRecords) {
                                contentSet.dt = executeQuery(SQL, CDef.dataSourceName, contentSet.PageSize * (contentSet.PageNumber - 1), contentSet.PageSize);
                            }
                        }
                        csInitData(returnCs);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnCs;
        }
        //        
        //========================================================================
        /// <summary>
        /// open a contentset without a record, to be used to update a record. you can set to write cache, and read from write cache, but cannot read fields not written
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="recordId">The record that will be updated if saved</param>
        /// <param name="memberId">The user logged as modified by if saved</param>
        /// <returns></returns>
        //
        public int csOpenForUpdate(string ContentName, int recordId, int memberId = 0) {
            int returnCs = -1;
            try {
                if (string.IsNullOrEmpty(ContentName)) {
                    throw new ArgumentException("ContentName cannot be blank");
                } else {
                    var CDef = CDefDomainModel.create(core, ContentName);
                    if (CDef == null) {
                        throw (new GenericException("No content found For [" + ContentName + "]"));
                    } else if (CDef.id <= 0) {
                        throw (new GenericException("No content found For [" + ContentName + "]"));
                    } else {
                        //
                        // ----- Open the csv_ContentSet
                        returnCs = csInit(memberId);
                        DbController.ContentSetClass contentSet = contentSetStore[returnCs];
                        contentSet.readable = false;
                        contentSet.writeable = true;
                        contentSet.ContentName = ContentName;
                        contentSet.DataSource = CDef.dataSourceName;
                        contentSet.CDef = CDef;
                        contentSet.SelectTableFieldList = CDef.selectCommaList;
                        contentSet.PageNumber = 1;
                        contentSet.PageSize = 1;
                        contentSet.Source = "";
                        csInitData(returnCs);
                        //
                        // -- initialize the id because the legacy system uses getinteger("id") to map to db
                        if (contentSet.writeCache.ContainsKey("id")) {
                            contentSet.writeCache["id"] = recordId.ToString();
                        } else {
                            contentSet.writeCache.Add("id", recordId.ToString());
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
        //========================================================================
        /// <summary>
        /// csv_DeleteCSRecord
        /// </summary>
        /// <param name="CSPointer"></param>
        public void csDeleteRecord(int CSPointer) {
            try {
                //string[] SQLName = new string[6];
                //string[] SQLValue = new string[6];
                //
                if (!csOk(CSPointer)) {
                    //
                    throw new ArgumentException("csv_ContentSet Is empty Or at End-Of-file");
                } else if (!(contentSetStore[CSPointer].writeable)) {
                    //
                    throw new ArgumentException("Dataset is not writeable.");
                } else {
                    int ContentID = contentSetStore[CSPointer].CDef.id;
                    string ContentName = contentSetStore[CSPointer].CDef.name;
                    string ContentTableName = contentSetStore[CSPointer].CDef.tableName;
                    string ContentDataSourceName = contentSetStore[CSPointer].CDef.dataSourceName;
                    if (string.IsNullOrEmpty(ContentName)) {
                        throw new ArgumentException("csv_ContentSet Is Not based On a Content Definition");
                    } else {
                        //
                        int LiveRecordID = csGetInteger(CSPointer, "ID");
                        //
                        // delete any files (if filename is part of select)
                        //
                        string fieldName = null;
                        Models.Domain.CDefFieldModel field = null;
                        foreach (var selectedFieldName in contentSetStore[CSPointer].CDef.selectList) {
                            if (contentSetStore[CSPointer].CDef.fields.ContainsKey(selectedFieldName.ToLowerInvariant())) {
                                field = contentSetStore[CSPointer].CDef.fields[selectedFieldName.ToLowerInvariant()];
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
                                        Filename = csGetText(CSPointer, fieldName);
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
                                        Filename = csGetText(CSPointer, fieldName);
                                        if (!string.IsNullOrEmpty(Filename)) {
                                            core.cdnFiles.deleteFile(Filename);
                                        }
                                        break;
                                }
                            }
                        }
                        //
                        // non-workflow mode, delete the live record
                        //
                        deleteTableRecord(LiveRecordID, ContentTableName, ContentDataSourceName);
                        //
                        // -- invalidate the special cache name used to detect a change in any record
                        // todo remove all these. do not invalidate the table for a record delete. it is up to the object to set invaliation dependencies
                        //core.cache.invalidateAllInTable(ContentTableName);
                        //if (workflowController.csv_AllowAutocsv_ClearContentTimeStamp) {
                        //    core.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(ContentTableName, "id", LiveRecordID.ToString()));
                        //    //Call core.cache.invalidateObject(ContentName)
                        //}
                        deleteContentRules(ContentID, LiveRecordID);
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
        /// openSql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public int csOpenSql(string sql, string dataSourceName = "", int pageSize = 9999, int pageNumber = 1) {
            int returnCs = -1;
            try {
                returnCs = csInit(core.session.user.id);
                {
                    ContentSetClass contentSet = contentSetStore[returnCs];
                    contentSet.readable = true;
                    contentSet.writeable = false;
                    contentSet.ContentName = "";
                    contentSet.PageNumber = pageNumber;
                    contentSet.PageSize = (pageSize);
                    contentSet.DataSource = dataSourceName;
                    contentSet.SelectTableFieldList = "";
                    contentSet.Source = sql;
                    contentSet.dt = executeQuery(sql, dataSourceName, pageSize * (pageNumber - 1), pageSize);
                }
                csInitData(returnCs);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnCs;
        }
        //
        //========================================================================
        /// <summary>
        /// initialize a cs
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        private int csInit(int MemberID) {
            int returnCs = -1;
            try {
                //
                // -- attempt to reuse space
                if (contentSetStoreCount > 0) {
                    for (int ptr = 1; ptr <= contentSetStoreCount; ptr++) {
                        if (!(contentSetStore[ptr].IsOpen)) {
                            returnCs = ptr;
                            break;
                        }
                    }
                }
                if (returnCs == -1) {
                    if (contentSetStoreCount >= contentSetStoreSize) {
                        contentSetStoreSize = contentSetStoreSize + contentSetStoreChunk;
                        Array.Resize(ref contentSetStore, contentSetStoreSize + 2);
                    }
                    contentSetStoreCount += 1;
                    returnCs = contentSetStoreCount;
                }
                //
                contentSetStore[returnCs] = new ContentSetClass() {
                    IsOpen = true,
                    NewRecord = true,
                    ContentName = "",
                    CDef = null,
                    DataSource = "",
                    dt = null,
                    fieldNames = null,
                    fieldPointer = 0,
                    IsModified = false,
                    LastUsed = DateTime.Now,
                    OwnerMemberID = MemberID,
                    PageNumber = 0,
                    PageSize = 0,
                    readCache = null,
                    readCacheRowCnt = 0,
                    readCacheRowPtr = 0,
                    resultColumnCount = 0,
                    resultEOF = true,
                    SelectTableFieldList = "",
                    Source = "",
                    writeable = false,
                    writeCache = null
                };
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnCs;
        }
        //
        //========================================================================
        //
        public void csGoNext(int CSPointer, bool AsyncSave = false) {
            try {
                if (!csOk(CSPointer)) {
                    //
                    throw new GenericException("Dataset is not valid.");
                } else if (!contentSetStore[CSPointer].readable) {
                    //
                    // -- if not readable, cannot move rows
                    throw new GenericException("Cannot move to next row because dataset is not readable.");
                } else {
                    csSave(CSPointer, AsyncSave);
                    contentSetStore[CSPointer].writeCache = new Dictionary<string, string>();
                    //
                    // Move to next row
                    contentSetStore[CSPointer].readCacheRowPtr = contentSetStore[CSPointer].readCacheRowPtr + 1;
                    //if (!csEOF(CSPointer)) {
                    //    //
                    //    // Not EOF, Set Workflow Edit Mode from Request and EditLock state
                    //    if (contentSetStore[CSPointer].writeable) {
                    //        WorkflowController.setEditLock(core, tableid, RecordID, contentSetStore[CSPointer].OwnerMemberID);


                    //        ContentName = contentSetStore[CSPointer].ContentName;
                    //        RecordID = csGetInteger(CSPointer, "ID");
                    //        if (!WorkflowController.isRecordLocked(ContentName, RecordID, contentSetStore[CSPointer].OwnerMemberID)) {
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
                contentSetStore[CSPointer].writeCache = new Dictionary<string, string>();
                contentSetStore[CSPointer].readCacheRowPtr = 0;
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
        /// <param name="CSPointer"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public string csGetValue(int CSPointer, string FieldName) {
            string returnValue = "";
            try {
                string fieldNameTrim = FieldName.Trim();
                if (!csOk(CSPointer)) {
                    throw new GenericException("Attempt To GetValue fieldname[" + fieldNameTrim + "], but the dataset Is empty Or does not point To a valid row");
                } else {
                    var contentSet = contentSetStore[CSPointer];
                    bool fieldFound = false;
                    if (contentSet.writeCache.Count > 0) {
                        //
                        // ----- something has been set in buffer, check it first
                        if (contentSet.writeCache.ContainsKey(fieldNameTrim.ToLowerInvariant())) {
                            returnValue = contentSet.writeCache[fieldNameTrim.ToLowerInvariant()];
                            fieldFound = true;
                        }
                    }
                    if (!fieldFound) {
                        //
                        // ----- attempt read from readCache
                        if (!contentSet.readable) {
                            //
                            // -- reading from write-only returns default value, because save there is legacy code that detects change bycomparing value to read cache
                            returnValue = "";
                            //throw new GenericException("Cannot read from a dataset opened write-only.");
                        } else if (contentSet.dt == null) {
                            throw new GenericException("Cannot read from a dataset because the data is not valid.");
                        } else {
                            if (!contentSet.dt.Columns.Contains(fieldNameTrim.ToLowerInvariant())) {
                                if (contentSet.writeable) {
                                    var dtFieldList = new List<string>();
                                    foreach (DataColumn column in contentSet.dt.Columns) dtFieldList.Add(column.ColumnName);
                                    throw new GenericException("Field [" + fieldNameTrim + "] was not found in [" + contentSet.ContentName + "] with selected fields [" + String.Join(",", dtFieldList.ToArray()) + "]");
                                } else {
                                    throw new GenericException("Field [" + fieldNameTrim + "] was not found in sql [" + contentSet.Source + "]");
                                }
                            } else {
                                returnValue = GenericController.encodeText(contentSet.dt.Rows[contentSet.readCacheRowPtr][fieldNameTrim.ToLowerInvariant()]);
                            }
                        }
                    }
                    contentSet.LastUsed = DateTime.Now;
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
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        public string csGetFirstFieldName(int CSPointer) {
            string returnFieldName = "";
            try {
                if (!csOk(CSPointer)) {
                    throw new GenericException("data set is not valid");
                } else {
                    contentSetStore[CSPointer].fieldPointer = 0;
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
                    var tempVar = contentSetStore[CSPointer];
                    while ((string.IsNullOrEmpty(returnFieldName)) && (tempVar.fieldPointer < tempVar.resultColumnCount)) {
                        returnFieldName = tempVar.fieldNames[tempVar.fieldPointer];
                        tempVar.fieldPointer = tempVar.fieldPointer + 1;
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
                    if (contentSetStore[CSPointer].writeable) {
                        if (!string.IsNullOrEmpty(contentSetStore[CSPointer].CDef.name)) {
                            returnFieldTypeid = contentSetStore[CSPointer].CDef.fields[FieldName.ToLowerInvariant()].fieldTypeId;
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
                    if (contentSetStore[CSPointer].writeable) {
                        if (!string.IsNullOrEmpty(contentSetStore[CSPointer].CDef.name)) {
                            returnResult = contentSetStore[CSPointer].CDef.fields[FieldName.ToLowerInvariant()].caption;
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
                    returnResult = string.Join(",", contentSetStore[CSPointer].fieldNames);
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
        public string csGetFieldFilename(int CSPointer, string FieldName, string OriginalFilename, string ContentName = "", int fieldTypeId = 0) {
            string returnFilename = "";
            try {
                string TableName = null;
                int RecordID = 0;
                string fieldNameUpper = null;
                int LenOriginalFilename = 0;
                int LenFilename = 0;
                int Pos = 0;
                //
                if (!csOk(CSPointer)) {
                    throw new ArgumentException("CSPointer does not point To a valid dataset, it Is empty, Or it Is Not pointing To a valid row.");
                } else if (string.IsNullOrEmpty(FieldName)) {
                    throw new ArgumentException("Fieldname Is blank");
                } else {
                    fieldNameUpper = GenericController.vbUCase(FieldName.Trim(' '));
                    returnFilename = csGetValue(CSPointer, fieldNameUpper);
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
                        var tempVar = contentSetStore[CSPointer];
                        //
                        // ----- no filename present, get id field
                        //
                        if (tempVar.resultColumnCount > 0) {
                            for (var FieldPointer = 0; FieldPointer < tempVar.resultColumnCount; FieldPointer++) {
                                if (GenericController.vbUCase(tempVar.fieldNames[FieldPointer]) == "ID") {
                                    RecordID = csGetInteger(CSPointer, "ID");
                                    break;
                                }
                            }
                        }
                        //
                        // ----- Get tablename
                        //
                        if (tempVar.writeable) {
                            //
                            // Get tablename from Content Definition
                            //
                            ContentName = tempVar.CDef.name;
                            TableName = tempVar.CDef.tableName;
                        } else if (!string.IsNullOrEmpty(ContentName)) {
                            //
                            // CS is SQL-based, use the contentname
                            //
                            TableName = CdefController.getContentTablename(core, ContentName);
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
                            } else if (tempVar.writeable) {
                                //
                                // -- get from cdef
                                fieldTypeId = tempVar.CDef.fields[FieldName.ToLowerInvariant()].fieldTypeId;
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
        public string csGetText(int CSPointer, string FieldName) {
            return GenericController.encodeText(csGetValue(CSPointer, FieldName));
        }
        //
        //====================================================================================================
        //
        public int csGetInteger(int CSPointer, string FieldName) {
            return GenericController.encodeInteger(csGetValue(CSPointer, FieldName));
        }
        //
        //====================================================================================================
        //
        public double csGetNumber(int CSPointer, string FieldName) {
            return GenericController.encodeNumber(csGetValue(CSPointer, FieldName));
        }
        //
        //====================================================================================================
        //
        public DateTime csGetDate(int CSPointer, string FieldName) {
            return GenericController.encodeDate(csGetValue(CSPointer, FieldName));
        }
        //
        //====================================================================================================
        //
        public bool csGetBoolean(int CSPointer, string FieldName) {
            return GenericController.encodeBoolean(csGetValue(CSPointer, FieldName));
        }
        //
        //====================================================================================================
        //
        public string csGetLookup(int CSPointer, string FieldName) {
            return csGet(CSPointer, FieldName);
        }
        //
        //====================================================================================================
        /// <summary>
        /// if the field uses an underlying filename, use this method to set that filename. The content for the field will switch to that contained by the new file
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <param name="FieldName"></param>
        /// <param name="filename"></param>
        public void csSetFieldFilename(int CSPointer, string FieldName, string filename) {
            try {
                if (!csOk(CSPointer)) {
                    throw new ArgumentException("dataset is not valid");
                } else if (string.IsNullOrEmpty(FieldName)) {
                    throw new ArgumentException("fieldName cannot be blank");
                } else {
                    var contentSet = contentSetStore[CSPointer];
                    if (!contentSet.writeable) {
                        throw new GenericException("Cannot set fields for a dataset based on a query.");
                    } else if (contentSet.CDef == null) {
                        throw new GenericException("Cannot set fields for a dataset based on a query.");
                    } else if (contentSet.CDef.fields == null) {
                        throw new GenericException("The dataset contains no fields.");
                    } else if (!contentSet.CDef.fields.ContainsKey(FieldName.ToLowerInvariant())) {
                        throw new GenericException("The dataset does not contain the field specified [" + FieldName.ToLowerInvariant() + "].");
                    } else {
                        if (contentSet.writeCache.ContainsKey(FieldName.ToLowerInvariant())) {
                            contentSet.writeCache[FieldName.ToLowerInvariant()] = filename;
                        } else {
                            contentSet.writeCache.Add(FieldName.ToLowerInvariant(), filename);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
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
        public void csSetTextFile(int CSPointer, string FieldName, string Copy, string ContentName) {
            try {
                if (!csOk(CSPointer)) {
                    throw new ArgumentException("dataset is not valid");
                } else if (string.IsNullOrEmpty(FieldName)) {
                    throw new ArgumentException("fieldName cannot be blank");
                } else if (string.IsNullOrEmpty(ContentName)) {
                    throw new ArgumentException("contentName cannot be blank");
                } else {
                    var dataSet = contentSetStore[CSPointer];
                    if (!dataSet.writeable) {
                        throw new GenericException("Cannot save this dataset because it is read-only.");
                    } else {
                        string OldFilename = csGetText(CSPointer, FieldName);
                        string Filename = csGetFieldFilename(CSPointer, FieldName, "", ContentName, Constants.fieldTypeIdFileText);
                        if (OldFilename != Filename) {
                            //
                            // Filename changed, mark record changed
                            //
                            core.cdnFiles.saveFile(Filename, Copy);
                            csSet(CSPointer, FieldName, Filename);
                        } else {
                            string OldCopy = core.cdnFiles.readFileText(Filename);
                            if (OldCopy != Copy) {
                                //
                                // copy changed, mark record changed
                                //
                                core.cdnFiles.saveFile(Filename, Copy);
                                csSet(CSPointer, FieldName, Filename);
                            }
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
        /// <summary>
        /// Inserts a record in a content definition and returns a csv_ContentSet with just that record
        /// If there was a problem, it returns -1
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public int csInsertRecord(string ContentName, int MemberID = -1) {
            int returnCs = -1;
            try {
                string Criteria = null;
                string DataSourceName = null;
                string FieldName = null;
                string TableName = null;
                Models.Domain.CDefDomainModel CDef = null;
                string DefaultValueText = null;
                string LookupContentName = null;
                int Ptr = 0;
                string[] lookups = null;
                string UCaseDefaultValueText = null;
                SqlFieldListClass sqlList = new SqlFieldListClass();
                //
                if (string.IsNullOrEmpty(ContentName.Trim())) {
                    throw new ArgumentException("ContentName cannot be blank");
                } else {
                    CDef = Models.Domain.CDefDomainModel.createByUniqueName(core, ContentName);
                    if (CDef == null) {
                        throw new GenericException("content [" + ContentName + "] could Not be found.");
                    } else if (CDef.id <= 0) {
                        throw new GenericException("content [" + ContentName + "] could Not be found.");
                    } else {
                        if (MemberID == -1) {
                            MemberID = core.session.user.id;
                        }
                        //
                        // no authoring, create default record in Live table
                        //
                        DataSourceName = CDef.dataSourceName;
                        TableName = CDef.tableName;
                        if (CDef.fields.Count > 0) {
                            foreach (KeyValuePair<string, Models.Domain.CDefFieldModel> keyValuePair in CDef.fields) {
                                Models.Domain.CDefFieldModel field = keyValuePair.Value;
                                FieldName = field.nameLc;
                                if ((!string.IsNullOrEmpty(FieldName)) && (!string.IsNullOrEmpty(field.defaultValue))) {
                                    switch (GenericController.vbUCase(FieldName)) {
                                        case "CREATEKEY":
                                        case "DATEADDED":
                                        case "CREATEDBY":
                                        case "CONTENTCONTROLID":
                                        case "ID":
                                            //
                                            // Block control fields
                                            //
                                            break;
                                        default:
                                            //
                                            // General case
                                            //
                                            switch (field.fieldTypeId) {
                                                case Constants._fieldTypeIdAutoIdIncrement:
                                                    //
                                                    // cannot insert an autoincremnt
                                                    //
                                                    break;
                                                case Constants._fieldTypeIdRedirect:
                                                case Constants._fieldTypeIdManyToMany:
                                                    //
                                                    // ignore these fields, they have no associated DB field
                                                    //
                                                    break;
                                                case Constants._fieldTypeIdBoolean:
                                                    sqlList.add(FieldName, encodeSQLBoolean(GenericController.encodeBoolean(field.defaultValue)));
                                                    break;
                                                case Constants._fieldTypeIdCurrency:
                                                case Constants._fieldTypeIdFloat:
                                                    sqlList.add(FieldName, encodeSQLNumber(GenericController.encodeNumber(field.defaultValue)));
                                                    break;
                                                case Constants._fieldTypeIdInteger:
                                                case Constants._fieldTypeIdMemberSelect:
                                                    sqlList.add(FieldName, encodeSQLNumber(GenericController.encodeInteger(field.defaultValue)));
                                                    break;
                                                case Constants._fieldTypeIdDate:
                                                    sqlList.add(FieldName, encodeSQLDate(GenericController.encodeDate(field.defaultValue)));
                                                    break;
                                                case Constants._fieldTypeIdLookup:
                                                    //
                                                    // refactor --
                                                    // This is a problem - the defaults should come in as the ID values, not the names
                                                    //   so a select can be added to the default configuration page
                                                    //
                                                    DefaultValueText = GenericController.encodeText(field.defaultValue);
                                                    if (string.IsNullOrEmpty(DefaultValueText)) {
                                                        DefaultValueText = "null";
                                                    } else {
                                                        if (field.lookupContentID != 0) {
                                                            LookupContentName = CdefController.getContentNameByID(core, field.lookupContentID);
                                                            if (!string.IsNullOrEmpty(LookupContentName)) {
                                                                DefaultValueText = getRecordID(LookupContentName, DefaultValueText).ToString();
                                                            }
                                                        } else if (field.lookupList != "") {
                                                            UCaseDefaultValueText = GenericController.vbUCase(DefaultValueText);
                                                            lookups = field.lookupList.Split(',');
                                                            for (Ptr = 0; Ptr <= lookups.GetUpperBound(0); Ptr++) {
                                                                if (UCaseDefaultValueText == GenericController.vbUCase(lookups[Ptr])) {
                                                                    DefaultValueText = (Ptr + 1).ToString();
                                                                }
                                                            }
                                                        }
                                                    }
                                                    sqlList.add(FieldName, DefaultValueText);
                                                    break;
                                                default:
                                                    //
                                                    // else text
                                                    //
                                                    sqlList.add(FieldName, encodeSQLText(field.defaultValue));
                                                    break;
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                        //
                        string sqlGuid = encodeSQLText(GenericController.getGUID());
                        //string CreateKeyString = encodeSQLNumber(genericController.GetRandomInteger(core));
                        string sqlDateAdded = encodeSQLDate(DateTime.Now);
                        //
                        sqlList.add("ccguid", sqlGuid);
                        //sqlList.add("CREATEKEY", CreateKeyString); 
                        sqlList.add("DATEADDED", sqlDateAdded);
                        sqlList.add("CONTENTCONTROLID", encodeSQLNumber(CDef.id));
                        sqlList.add("CREATEDBY", encodeSQLNumber(MemberID));
                        insertTableRecord(DataSourceName, TableName, sqlList);
                        //
                        // ----- Get the record back so we can use the ID
                        //
                        Criteria = "(ccguid=" + sqlGuid + ")And(DateAdded=" + sqlDateAdded + ")";
                        //Criteria = "((createkey=" + CreateKeyString + ")And(DateAdded=" + DateAddedString + "))";
                        returnCs = csOpen(ContentName, Criteria, "ID DESC", false, MemberID, false, true);
                        //
                        // ----- Clear Time Stamp because a record changed
                        // 20171213 added back for integration test (had not noted why it was commented out
                        //core.cache.invalidateAllInContent(ContentName);
                        //If coreWorkflowClass.csv_AllowAutocsv_ClearContentTimeStamp Then
                        //    Call core.cache.invalidateObject(ContentName)
                        //End If
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnCs;
        }
        //========================================================================
        // Opens a Content Record
        //   If there was a problem, it returns -1 (not csv_IsCSOK)
        //   Can open either the ContentRecord or the AuthoringRecord (WorkflowAuthoringMode)
        //   Isolated in API so later we can save record in an Index buffer for fast access
        //========================================================================
        //
        public int csOpenContentRecord(string ContentName, int RecordID, int MemberID = SystemMemberID, bool WorkflowAuthoringMode = false, bool WorkflowEditingMode = false, string SelectFieldList = "") {
            int returnResult = -1;
            try {
                if (RecordID <= 0) {
                    // no error, return -1 - Throw New ArgumentException("recordId is not valid [" & RecordID & "]")
                } else {
                    returnResult = csOpen(ContentName, "(ID=" + encodeSQLNumber(RecordID) + ")", "", false, MemberID, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1);
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
        /// true if csPointer is a valid dataset, and currently points to a valid row
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        public bool csOk(int CSPointer) {
            bool returnResult = false;
            try {
                if (CSPointer < 0) {
                    returnResult = false;
                } else if (CSPointer > contentSetStoreCount) {
                    // todo
                    // 20171209 - appears csPtr starts at 1, not 0, so it can equal the count -- creates upper limit issue with array (refactor after conversion)
                    throw new ArgumentException("dateset is not valid");
                } else if (contentSetStore[CSPointer].writeable & !contentSetStore[CSPointer].readable) {
                    //
                    // -- opened with openForUpdate. can be written but not read
                    returnResult = contentSetStore[CSPointer].IsOpen;
                } else {
                    returnResult = contentSetStore[CSPointer].IsOpen & (contentSetStore[CSPointer].readCacheRowPtr >= 0) && (contentSetStore[CSPointer].readCacheRowPtr < contentSetStore[CSPointer].readCacheRowCnt);
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
        /// copy the current row of the source dataset to the destination dataset. The destination dataset must have been created with cs open or insert, and must contain all the fields in the source dataset.
        /// </summary>
        /// <param name="CSSource"></param>
        /// <param name="CSDestination"></param>
        //========================================================================
        //
        public void csCopyRecord(int CSSource, int CSDestination) {
            try {
                string FieldName = null;
                string DestContentName = null;
                int DestRecordID = 0;
                string DestFilename = null;
                string SourceFilename = null;
                Models.Domain.CDefDomainModel DestCDef = null;
                //
                if (!csOk(CSSource)) {
                    throw new ArgumentException("source dataset is not valid");
                } else if (!csOk(CSDestination)) {
                    throw new ArgumentException("destination dataset is not valid");
                } else if (contentSetStore[CSDestination].CDef == null) {
                    throw new ArgumentException("copyRecord requires the destination dataset to be created from a cs Open or Insert, not a query.");
                } else {
                    DestCDef = contentSetStore[CSDestination].CDef;
                    DestContentName = DestCDef.name;
                    DestRecordID = csGetInteger(CSDestination, "ID");
                    FieldName = csGetFirstFieldName(CSSource);
                    while (!string.IsNullOrEmpty(FieldName)) {
                        switch (GenericController.vbUCase(FieldName)) {
                            case "ID":
                                break;
                            default:
                                //
                                // ----- fields to copy
                                //
                                int sourceFieldTypeId = csGetFieldTypeId(CSSource, FieldName);
                                switch (sourceFieldTypeId) {
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
                                        //
                                        SourceFilename = csGetFieldFilename(CSSource, FieldName, "", contentSetStore[CSDestination].CDef.name, sourceFieldTypeId);
                                        if (!string.IsNullOrEmpty(SourceFilename)) {
                                            DestFilename = csGetFieldFilename(CSDestination, FieldName, "", DestContentName, sourceFieldTypeId);
                                            csSet(CSDestination, FieldName, DestFilename);
                                            core.cdnFiles.copyFile(SourceFilename, DestFilename);
                                        }
                                        break;
                                    case Constants._fieldTypeIdFileText:
                                    case Constants._fieldTypeIdFileHTML:
                                        //
                                        // ----- private file
                                        //
                                        SourceFilename = csGetFieldFilename(CSSource, FieldName, "", DestContentName, sourceFieldTypeId);
                                        if (!string.IsNullOrEmpty(SourceFilename)) {
                                            DestFilename = csGetFieldFilename(CSDestination, FieldName, "", DestContentName, sourceFieldTypeId);
                                            csSet(CSDestination, FieldName, DestFilename);
                                            core.cdnFiles.copyFile(SourceFilename, DestFilename);
                                        }
                                        break;
                                    default:
                                        //
                                        // ----- value
                                        //
                                        csSet(CSDestination, FieldName, csGetValue(CSSource, FieldName));
                                        break;
                                }
                                break;
                        }
                        FieldName = csGetNextFieldName(CSSource);
                    }
                    csSave(CSDestination);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //       
        //========================================================================
        /// <summary>
        /// Returns the Source for the csv_ContentSet
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <returns></returns>
        public string csGetSql(int CSPointer) {
            string returnResult = "";
            try {
                if (!csOk(CSPointer)) {
                    throw new ArgumentException("the dataset is not valid");
                } else {
                    returnResult = contentSetStore[CSPointer].Source;
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
        /// Returns the value of a field, decoded into a text string result, if there is a problem, null is returned, this may be because the lookup record is inactive, so its not an error
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        //
        public string csGet(int CSPointer, string FieldName) {
            string fieldValue = "";
            try {
                int FieldValueInteger = 0;
                string LookupContentName = null;
                string LookupList = null;
                string[] lookups = null;
                object FieldValueVariant = null;
                int CSLookup = 0;
                int fieldTypeId = 0;
                int fieldLookupId = 0;
                //
                // ----- needs work. Go to fields table and get field definition
                //       then print accordingly
                //
                if (!csOk(CSPointer)) {
                    throw new ArgumentException("the dataset is not valid");
                } else if (string.IsNullOrEmpty(FieldName.Trim())) {
                    throw new ArgumentException("fieldname cannot be blank");
                } else {
                    //
                    // csv_ContentSet good
                    //
                    var tempVar = contentSetStore[CSPointer];
                    if (!tempVar.writeable) {
                        //
                        // Not updateable -- Just return what is there as a string
                        //
                        try {
                            fieldValue = GenericController.encodeText(csGetValue(CSPointer, FieldName));
                        } catch (Exception ex) {
                            throw new GenericException("Error [" + ex.Message + "] reading field [" + FieldName.ToLowerInvariant() + "] In source [" + tempVar.Source + "");
                        }
                    } else {
                        //
                        // Updateable -- enterprete the value
                        //
                        //ContentName = .ContentName
                        Models.Domain.CDefFieldModel field = null;
                        if (!tempVar.CDef.fields.ContainsKey(FieldName.ToLowerInvariant())) {
                            try {
                                fieldValue = GenericController.encodeText(csGetValue(CSPointer, FieldName));
                            } catch (Exception ex) {
                                throw new GenericException("Error [" + ex.Message + "] reading field [" + FieldName.ToLowerInvariant() + "] In content [" + tempVar.CDef.name + "] With custom field list [" + tempVar.SelectTableFieldList + "");
                            }
                        } else {
                            field = tempVar.CDef.fields[FieldName.ToLowerInvariant()];
                            fieldTypeId = field.fieldTypeId;
                            if (fieldTypeId == fieldTypeIdManyToMany) {
                                //
                                // special case - recordset contains no data - return record id list
                                //
                                int RecordID = 0;
                                string DbTable = null;
                                string ContentName = null;
                                string SQL = null;
                                DataTable rs = null;
                                if (tempVar.CDef.fields.ContainsKey("id")) {
                                    RecordID = GenericController.encodeInteger(csGetValue(CSPointer, "id"));
                                    ContentName = CdefController.getContentNameByID(core, field.manyToManyRuleContentID);
                                    DbTable = CdefController.getContentTablename(core, ContentName);
                                    SQL = "Select " + field.ManyToManyRuleSecondaryField + " from " + DbTable + " where " + field.ManyToManyRulePrimaryField + "=" + RecordID;
                                    rs = executeQuery(SQL);
                                    if (DbController.isDataTableOk(rs)) {
                                        foreach (DataRow dr in rs.Rows) {
                                            fieldValue += "," + dr[0].ToString();
                                        }
                                        fieldValue = fieldValue.Substring(1);
                                    }
                                }
                            } else if (fieldTypeId == fieldTypeIdRedirect) {
                                //
                                // special case - recordset contains no data - return blank
                                //
                                //fieldTypeId = fieldTypeId;
                            } else {
                                FieldValueVariant = csGetValue(CSPointer, FieldName);
                                if (!GenericController.IsNull(FieldValueVariant)) {
                                    //
                                    // Field is good
                                    //
                                    switch (fieldTypeId) {
                                        case _fieldTypeIdBoolean:
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
                                        case _fieldTypeIdDate:
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
                                        case _fieldTypeIdLookup:
                                            //
                                            //
                                            //
                                            if (FieldValueVariant.IsNumeric()) {
                                                fieldLookupId = field.lookupContentID;
                                                LookupContentName = CdefController.getContentNameByID(core, fieldLookupId);
                                                LookupList = field.lookupList;
                                                if (!string.IsNullOrEmpty(LookupContentName)) {
                                                    //
                                                    // -- First try Lookup Content
                                                    CSLookup = csOpen(LookupContentName, "ID=" + encodeSQLNumber(GenericController.encodeInteger(FieldValueVariant)), "", true, 0, false, false, "name", 1);
                                                    if (csOk(CSLookup)) {
                                                        fieldValue = csGetText(CSLookup, "name");
                                                    }
                                                    csClose(ref CSLookup);
                                                } else if (!string.IsNullOrEmpty(LookupList)) {
                                                    //
                                                    // -- Next try lookup list
                                                    FieldValueInteger = GenericController.encodeInteger(FieldValueVariant) - 1;
                                                    if (FieldValueInteger >= 0) {
                                                        lookups = LookupList.Split(',');
                                                        if (lookups.GetUpperBound(0) >= FieldValueInteger) {
                                                            fieldValue = lookups[FieldValueInteger];
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        case _fieldTypeIdMemberSelect:
                                            //
                                            //
                                            //
                                            if (FieldValueVariant.IsNumeric()) {
                                                fieldValue = getRecordName("people", GenericController.encodeInteger(FieldValueVariant));
                                            }
                                            break;
                                        case _fieldTypeIdCurrency:
                                            //
                                            //
                                            //
                                            if (FieldValueVariant.IsNumeric()) {
                                                fieldValue = FieldValueVariant.ToString();
                                            }
                                            break;
                                        case _fieldTypeIdFileText:
                                        case _fieldTypeIdFileHTML:
                                            //
                                            //
                                            //
                                            fieldValue = core.cdnFiles.readFileText(GenericController.encodeText(FieldValueVariant));
                                            break;
                                        case _fieldTypeIdFileCSS:
                                        case _fieldTypeIdFileXML:
                                        case _fieldTypeIdFileJavascript:
                                            //
                                            //
                                            //
                                            fieldValue = core.cdnFiles.readFileText(GenericController.encodeText(FieldValueVariant));
                                            //NeedsHTMLEncode = False
                                            break;
                                        case _fieldTypeIdText:
                                        case _fieldTypeIdLongText:
                                        case _fieldTypeIdHTML:
                                            //
                                            //
                                            //
                                            fieldValue = GenericController.encodeText(FieldValueVariant);
                                            break;
                                        case _fieldTypeIdFile:
                                        case _fieldTypeIdFileImage:
                                        case _fieldTypeIdLink:
                                        case _fieldTypeIdResourceLink:
                                        case _fieldTypeIdAutoIdIncrement:
                                        case _fieldTypeIdFloat:
                                        case _fieldTypeIdInteger:
                                            //
                                            //
                                            //
                                            fieldValue = GenericController.encodeText(FieldValueVariant);
                                            //NeedsHTMLEncode = False
                                            break;
                                        case _fieldTypeIdRedirect:
                                        case _fieldTypeIdManyToMany:
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
        /// <param name="CSPointer"></param>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        //
        public void csSet(int CSPointer, string FieldName, string FieldValue) {
            try {
                string BlankTest = null;
                string FieldNameLc = null;
                bool SetNeeded = false;
                string fileNameNoExt = null;
                string ContentName = null;
                string fileName = null;
                string pathFilenameOriginal = null;
                //
                if (!csOk(CSPointer)) {
                    throw new ArgumentException("dataset is not valid or End-Of-file.");
                } else if (string.IsNullOrEmpty(FieldName.Trim())) {
                    throw new ArgumentException("fieldName cannnot be blank");
                } else {
                    var dataSet = contentSetStore[CSPointer];
                    if (!dataSet.writeable) {
                        throw new GenericException("Cannot update a contentset created from a sql query.");
                    } else {
                        ContentName = dataSet.ContentName;
                        FieldNameLc = FieldName.Trim(' ').ToLowerInvariant();
                        if (FieldValue == null) {
                            FieldValue = "";
                        }
                        if (!string.IsNullOrEmpty(dataSet.CDef.name)) {
                            Models.Domain.CDefFieldModel field = null;
                            if (!dataSet.CDef.fields.ContainsKey(FieldNameLc)) {
                                throw new ArgumentException("The field [" + FieldName + "] could Not be found In content [" + dataSet.CDef.name + "]");
                            } else {
                                field = dataSet.CDef.fields[FieldNameLc];
                                switch (field.fieldTypeId) {
                                    case _fieldTypeIdAutoIdIncrement:
                                    case _fieldTypeIdRedirect:
                                    case _fieldTypeIdManyToMany:
                                        //
                                        // Never set
                                        //
                                        break;
                                    case _fieldTypeIdFile:
                                    case _fieldTypeIdFileImage:
                                        //
                                        // Always set
                                        // Saved in the field is the filename to the file
                                        SetNeeded = true;
                                        break;
                                    case _fieldTypeIdFileText:
                                    case _fieldTypeIdFileHTML:
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
                                    //        fileNameNoExt = csGetFieldFilename(CSPointer, FieldName, "", ContentName, field.fieldTypeId);
                                    //    }
                                    //    core.cdnFiles.saveFile(fileNameNoExt, FieldValue);
                                    //    //Call publicFiles.SaveFile(fileNameNoExt, FieldValue)
                                    //}
                                    //FieldValue = fileNameNoExt;
                                    //SetNeeded = true;
                                    //break;
                                    case _fieldTypeIdFileCSS:
                                    case _fieldTypeIdFileXML:
                                    case _fieldTypeIdFileJavascript:
                                        //
                                        // public files - save as FieldTypeTextFile except if only white space, consider it blank
                                        //
                                        string PathFilename = null;
                                        string FileExt = null;
                                        int FilenameRev = 0;
                                        string path = null;
                                        int Pos = 0;
                                        pathFilenameOriginal = csGetText(CSPointer, FieldNameLc);
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
                                                PathFilename = csGetFieldFilename(CSPointer, FieldNameLc, "", ContentName, field.fieldTypeId);
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
                                    case _fieldTypeIdBoolean:
                                        //
                                        // Boolean - sepcial case, block on typed GetAlways set
                                        if (GenericController.encodeBoolean(FieldValue) != csGetBoolean(CSPointer, FieldNameLc)) {
                                            SetNeeded = true;
                                        }
                                        break;
                                    case _fieldTypeIdText:
                                        //
                                        // Set if text of value changes
                                        //
                                        if (GenericController.encodeText(FieldValue) != csGetText(CSPointer, FieldNameLc)) {
                                            SetNeeded = true;
                                            if (FieldValue.Length > 255) {
                                                LogController.handleError(core, new GenericException("Text length too long saving field [" + FieldName + "], length [" + FieldValue.Length + "], but max for Text field is 255. Save will be attempted"));
                                            }
                                        }
                                        break;
                                    case _fieldTypeIdLongText:
                                    case _fieldTypeIdHTML:
                                        //
                                        // Set if text of value changes
                                        //
                                        if (GenericController.encodeText(FieldValue) != csGetText(CSPointer, FieldNameLc)) {
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
                                        if (GenericController.encodeText(FieldValue) != csGetText(CSPointer, FieldNameLc)) {
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
                            if (dataSet.writeCache.ContainsKey(FieldNameLc)) {
                                dataSet.writeCache[FieldNameLc] = FieldValue.ToString();
                            } else {
                                dataSet.writeCache.Add(FieldNameLc, FieldValue.ToString());
                            }
                            dataSet.LastUsed = DateTime.Now;
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
                    contentSetStore[CSPointer].writeCache.Clear();
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
        public void csSave(int csPtr, bool asyncSave = false, bool Blockcsv_ClearBake = false) {
            try {
                if (!csOk(csPtr)) {
                    //
                    // -- already closed or not opened or not on a current row. No error so you can always call save(), it skips if nothing to save
                } else if (contentSetStore[csPtr].writeCache.Count == 0) {
                    //
                    // -- nothing to write, just exit
                } else if (!(contentSetStore[csPtr].writeable)) {
                    //
                    // -- dataset not updatable
                    throw new ArgumentException("The dataset cannot be updated because it was created with a query and not a content table.");
                } else {
                    var contentSet = contentSetStore[csPtr];
                    if (contentSet.CDef == null) {
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
                        int sqlModifiedBy = contentSet.OwnerMemberID;
                        bool AuthorableFieldUpdate = false;
                        int FieldFoundCount = 0;
                        string SQLCriteriaUnique = "";
                        string UniqueViolationFieldList = "";
                        foreach (var keyValuePair in contentSet.writeCache) {
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
                                Models.Domain.CDefFieldModel field = contentSet.CDef.fields[fieldName.ToLowerInvariant()];
                                string SQLSetPair = "";
                                bool FieldReadOnly = field.readOnly;
                                bool FieldAdminAuthorable = ((!field.readOnly) && (!field.notEditable) && (field.authorable));
                                //
                                // ----- Set SQLSetPair to the name=value pair for the SQL statement
                                //
                                switch (field.fieldTypeId) {
                                    case _fieldTypeIdRedirect:
                                    case _fieldTypeIdManyToMany:
                                        break;
                                    case _fieldTypeIdInteger:
                                    case _fieldTypeIdLookup:
                                    case _fieldTypeIdAutoIdIncrement:
                                    case _fieldTypeIdMemberSelect:
                                        SQLSetPair = fieldName + "=" + encodeSQLNumber(encodeInteger(writeCacheValue));
                                        break;
                                    case _fieldTypeIdCurrency:
                                    case _fieldTypeIdFloat:
                                        SQLSetPair = fieldName + "=" + encodeSQLNumber(encodeNumber(writeCacheValue));
                                        break;
                                    case _fieldTypeIdBoolean:
                                        SQLSetPair = fieldName + "=" + encodeSQLBoolean(encodeBoolean(writeCacheValue));
                                        break;
                                    case _fieldTypeIdDate:
                                        SQLSetPair = fieldName + "=" + encodeSQLDate(encodeDate(writeCacheValue));
                                        break;
                                    case _fieldTypeIdText:
                                        string Copy = encodeText(writeCacheValue);
                                        if (Copy.Length > 255) {
                                            Copy = Copy.Left(255);
                                        }
                                        if (field.Scramble) {
                                            Copy = TextScramble(core, Copy);
                                        }
                                        SQLSetPair = fieldName + "=" + encodeSQLText(Copy);
                                        break;
                                    case _fieldTypeIdLink:
                                    case _fieldTypeIdResourceLink:
                                    case _fieldTypeIdFile:
                                    case _fieldTypeIdFileImage:
                                    case _fieldTypeIdFileText:
                                    case _fieldTypeIdFileCSS:
                                    case _fieldTypeIdFileXML:
                                    case _fieldTypeIdFileJavascript:
                                    case _fieldTypeIdFileHTML:
                                        string filename = encodeText(writeCacheValue);
                                        if (filename.Length > 255) {
                                            filename = filename.Left(255);
                                        }
                                        SQLSetPair = fieldName + "=" + encodeSQLText(filename);
                                        break;
                                    case _fieldTypeIdLongText:
                                    case _fieldTypeIdHTML:
                                        SQLSetPair = fieldName + "=" + encodeSQLText(GenericController.encodeText(writeCacheValue));
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
                                    var dataSet = contentSetStore[csPtr];
                                    if (dataSet.resultColumnCount > 0) {
                                        for (int ColumnPtr = 0; ColumnPtr < dataSet.resultColumnCount; ColumnPtr++) {
                                            if (dataSet.fieldNames[ColumnPtr] == ucaseFieldName) {
                                                dataSet.readCache[ColumnPtr, dataSet.readCacheRowPtr] = writeCacheValue.ToString();
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
                                            case _fieldTypeIdRedirect:
                                            case _fieldTypeIdManyToMany:
                                                break;
                                            default:
                                                SQLCriteriaUnique += "(" + field.nameLc + "=" + encodeSQL(writeCacheValue, field.fieldTypeId) + ")";
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
                        //contentSet.writeCache = new Dictionary<string, string>();
                        //
                        // ----- Set ModifiedBy,ModifiedDate Fields if an admin visible field has changed
                        //
                        if (AuthorableFieldUpdate) {
                            if (!string.IsNullOrEmpty(sqlUpdate)) {
                                //
                                // ----- Authorable Fields Updated in non-Authoring Mode, set Live Record Modified
                                //
                                sqlUpdate = sqlUpdate + ",MODIFIEDDATE=" + encodeSQLDate(sqlModifiedDate) + ",MODIFIEDBY=" + encodeSQLNumber(sqlModifiedBy);
                            }
                        }
                        //
                        // ----- Do the unique check on the content table, if necessary
                        //
                        if (!string.IsNullOrEmpty(SQLCriteriaUnique)) {
                            string sqlUnique = "SELECT ID FROM " + contentSet.CDef.tableName + " WHERE (ID<>" + id + ")AND(" + SQLCriteriaUnique + ")and(" + contentSet.CDef.legacyContentControlCriteria + ");";
                            using (DataTable dt = executeQuery(sqlUnique, contentSet.CDef.dataSourceName)) {
                                //
                                // -- unique violation
                                if (dt.Rows.Count > 0) {
                                    LogController.logWarn(core, "Can not save record to content [" + contentSet.CDef.name + "] because it would create a non-unique record for one or more of the following field(s) [" + UniqueViolationFieldList + "]");
                                    return;
                                    //throw new GenericException(("Can not save record to content [" + contentSet.CDef.name + "] because it would create a non-unique record for one or more of the following field(s) [" + UniqueViolationFieldList + "]"));
                                }
                            }
                        }
                        if (FieldFoundCount > 0) {
                            //
                            // ----- update live table (non-workflowauthoring and non-authorable fields)
                            //
                            if (!string.IsNullOrEmpty(sqlUpdate)) {
                                string SQLUpdate = "UPDATE " + contentSet.CDef.tableName + " SET " + sqlUpdate + " WHERE ID=" + id + ";";
                                if (asyncSave) {
                                    executeNonQueryAsync(SQLUpdate, contentSet.CDef.dataSourceName);
                                } else {
                                    executeNonQuery(SQLUpdate, contentSet.CDef.dataSourceName);
                                }
                            }
                        }
                        contentSet.LastUsed = DateTime.Now;
                        //
                        // -- invalidate the special cache name used to detect a change in any record
                        core.cache.invalidateDbRecord(id, contentSet.CDef.tableName, contentSet.CDef.dataSourceName);
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
        /// Initialize the csv_ContentSet Result Cache when it is first opened
        /// </summary>
        /// <param name="CSPointer"></param>
        //
        private void csInitData(int CSPointer) {
            try {
                ContentSetClass csStore = contentSetStore[CSPointer];
                csStore.resultColumnCount = 0;
                csStore.readCacheRowCnt = 0;
                csStore.readCacheRowPtr = -1;
                csStore.resultEOF = true;
                csStore.writeCache = new Dictionary<string, string>();
                csStore.fieldNames = new String[] { };
                if (csStore.dt != null) {
                    if (csStore.dt.Rows.Count > 0) {
                        csStore.resultColumnCount = csStore.dt.Columns.Count;
                        csStore.fieldNames = new String[csStore.resultColumnCount];
                        int ColumnPtr = 0;
                        foreach (DataColumn dc in csStore.dt.Columns) {
                            csStore.fieldNames[ColumnPtr] = GenericController.vbUCase(dc.ColumnName);
                            ColumnPtr += 1;
                        }
                        // refactor -- convert interal storage to dt and assign -- will speedup open
                        csStore.readCache = convertDataTabletoArray(csStore.dt);
                        csStore.readCacheRowCnt = csStore.readCache.GetUpperBound(1) + 1;
                        csStore.readCacheRowPtr = 0;
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
            bool returnResult = true;
            try {
                if (CSPointer <= 0) {
                    throw new ArgumentException("dataset is not valid");
                } else {
                    returnResult = (contentSetStore[CSPointer].readCacheRowPtr >= contentSetStore[CSPointer].readCacheRowCnt);
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
        /// Opens a csv_ContentSet with the Members of a group
        /// </summary>
        /// <param name="groupList"></param>
        /// <param name="sqlCriteria"></param>
        /// <param name="SortFieldList"></param>
        /// <param name="ActiveOnly"></param>
        /// <param name="PageSize"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        public int csOpenGroupUsers(List<string> groupList, string sqlCriteria = "", string SortFieldList = "", bool ActiveOnly = true, int PageSize = 9999, int PageNumber = 1) {
            int returnResult = -1;
            try {
                DateTime rightNow = DateTime.Now;
                string sqlRightNow = encodeSQLDate(rightNow);
                //
                if (PageNumber == 0) {
                    PageNumber = 1;
                }
                if (PageSize == 0) {
                    PageSize = pageSizeDefault;
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
                            subQuery += "or(ccGroups.Name=" + encodeSQLText(groupName.Trim()) + ")";
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
                    returnResult = csOpenSql(SQL, "Default", PageSize, PageNumber);
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
                returnResult = contentSetStore[CSPointer].readCache;
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
                    returnResult = contentSetStore[CSPointer].readCacheRowCnt;
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
                    returnResult = contentSetStore[CSPointer].ContentName;
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
        public string csGetRecordEditLink(int CSPointer, bool AllowCut = false) {
            string result = "";

            string RecordName = null;
            string ContentName = null;
            int RecordID = 0;
            int ContentControlID = 0;
            int iCSPointer;
            //
            iCSPointer = GenericController.encodeInteger(CSPointer);
            if (iCSPointer == -1) {
                throw (new GenericException("csGetRecordEditLink called with invalid iCSPointer")); // handleLegacyError14(MethodName, "")
            } else {
                if (!core.db.csOk(iCSPointer)) {
                    throw (new GenericException("csGetRecordEditLink called with Not main_CSOK")); // handleLegacyError14(MethodName, "")
                } else {
                    //
                    // Print an edit link for the records Content (may not be iCSPointer content)
                    //
                    RecordID = (core.db.csGetInteger(iCSPointer, "ID"));
                    RecordName = core.db.csGetText(iCSPointer, "Name");
                    ContentControlID = (core.db.csGetInteger(iCSPointer, "contentcontrolid"));
                    ContentName = CdefController.getContentNameByID(core, ContentControlID);
                    if (!string.IsNullOrEmpty(ContentName)) {
                        result = AdminUIController.getRecordEditLink(core, ContentName, RecordID, GenericController.encodeBoolean(AllowCut), RecordName, core.session.isEditing(ContentName));
                    }
                }
            }
            return result;
        }
        //
        // ====================================================================================================
        //
        public static void csSetFormInput(CoreController core, int CSPointer, string FieldName, string RequestName = "") {
            string LocalRequestName = null;
            string Filename = null;
            string Path = null;
            if (!core.db.csOk(CSPointer)) {
                throw new GenericException("ContentSetPointer is invalid, empty, or end-of-file");
            } else if (string.IsNullOrEmpty(FieldName.Trim(' '))) {
                throw new GenericException("FieldName is invalid or blank");
            } else {
                LocalRequestName = RequestName;
                if (string.IsNullOrEmpty(LocalRequestName)) {
                    LocalRequestName = FieldName;
                }
                switch (core.db.csGetFieldTypeId(CSPointer, FieldName)) {
                    case _fieldTypeIdBoolean:
                        //
                        // -- Boolean
                        core.db.csSet(CSPointer, FieldName, core.docProperties.getBoolean(LocalRequestName));
                        break;
                    case _fieldTypeIdCurrency:
                    case _fieldTypeIdFloat:
                    case _fieldTypeIdInteger:
                    case _fieldTypeIdLookup:
                    case _fieldTypeIdManyToMany:
                        //
                        // -- Numbers
                        core.db.csSet(CSPointer, FieldName, core.docProperties.getNumber(LocalRequestName));
                        break;
                    case _fieldTypeIdDate:
                        //
                        // -- Date
                        core.db.csSet(CSPointer, FieldName, core.docProperties.getDate(LocalRequestName));
                        break;
                    case _fieldTypeIdFile:
                    case _fieldTypeIdFileImage:
                        //
                        // -- upload file
                        Filename = core.docProperties.getText(LocalRequestName);
                        if (!string.IsNullOrEmpty(Filename)) {
                            Path = core.db.csGetFieldFilename(CSPointer, FieldName, Filename, "", core.db.csGetFieldTypeId(CSPointer, FieldName));
                            core.db.csSet(CSPointer, FieldName, Path);
                            Path = GenericController.vbReplace(Path, "\\", "/");
                            Path = GenericController.vbReplace(Path, "/" + Filename, "");
                            core.cdnFiles.upload(LocalRequestName, Path, ref Filename);
                        }
                        break;
                    default:
                        //
                        // -- text files
                        core.db.csSet(CSPointer, FieldName, core.docProperties.getText(LocalRequestName));
                        break;
                }
            }
        }
        //
        //====================================================================================================
        //
        public static string csGetRecordAddLink(CoreController core, int csPtr, string PresetNameValueList = "", bool AllowPaste = false) {
            string result = "";
            try {
                if (csPtr >= 0) {
                    string ContentName = core.db.csGetContentName(csPtr);
                    if (string.IsNullOrEmpty(ContentName)) {
                        LogController.logWarn(core, "getRecordAddLink was called with a ContentSet that was created with an SQL statement. The function requires a ContentSet opened with an OpenCSContent.");
                    } else {
                        foreach (var AddLink in AdminUIController.getRecordAddLink(core, ContentName, PresetNameValueList, AllowPaste)) { result += AddLink; }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public int csOpenWhatsNew(CoreController core, string SortFieldList = "", bool ActiveOnly = true, int PageSize = 1000, int PageNumber = 1) {
            int result = -1;
            try {
                result = csOpenContentWatchList(core, "What's New", SortFieldList, ActiveOnly, PageSize, PageNumber);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public int csOpenContentWatchList(CoreController core, string ListName, string SortFieldList = "", bool ActiveOnly = true, int PageSize = 1000, int PageNumber = 1) {
            int result = -1;
            try {
                string SQL = null;
                string iSortFieldList = null;
                string MethodName = null;
                int CS = 0;
                //
                iSortFieldList = encodeText(encodeEmpty(SortFieldList, "")).Trim(' ');
                //iSortFieldList = encodeMissingText(SortFieldList, "DateAdded")
                if (string.IsNullOrEmpty(iSortFieldList)) {
                    iSortFieldList = "DateAdded";
                }
                //
                MethodName = "main_OpenCSContentWatchList( " + ListName + ", " + iSortFieldList + ", " + ActiveOnly + " )";
                //
                // ----- Add tablename to the front of SortFieldList fieldnames
                //
                iSortFieldList = " " + GenericController.vbReplace(iSortFieldList, ",", " , ") + " ";
                iSortFieldList = GenericController.vbReplace(iSortFieldList, " ID ", " ccContentWatch.ID ", 1, 99, 1);
                iSortFieldList = GenericController.vbReplace(iSortFieldList, " Link ", " ccContentWatch.Link ", 1, 99, 1);
                iSortFieldList = GenericController.vbReplace(iSortFieldList, " LinkLabel ", " ccContentWatch.LinkLabel ", 1, 99, 1);
                iSortFieldList = GenericController.vbReplace(iSortFieldList, " SortOrder ", " ccContentWatch.SortOrder ", 1, 99, 1);
                iSortFieldList = GenericController.vbReplace(iSortFieldList, " DateAdded ", " ccContentWatch.DateAdded ", 1, 99, 1);
                iSortFieldList = GenericController.vbReplace(iSortFieldList, " ContentID ", " ccContentWatch.ContentID ", 1, 99, 1);
                iSortFieldList = GenericController.vbReplace(iSortFieldList, " RecordID ", " ccContentWatch.RecordID ", 1, 99, 1);
                iSortFieldList = GenericController.vbReplace(iSortFieldList, " ModifiedDate ", " ccContentWatch.ModifiedDate ", 1, 99, 1);
                //
                // ----- Special case
                //
                iSortFieldList = GenericController.vbReplace(iSortFieldList, " name ", " ccContentWatch.LinkLabel ", 1, 99, 1);
                //
                SQL = "SELECT"
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
                + " ORDER BY " + iSortFieldList + ";";
                result = this.core.db.csOpenSql(SQL, "", PageSize, PageNumber);
                if (!this.core.db.csOk(result)) {
                    //
                    // Check if listname exists
                    //
                    CS = this.core.db.csOpen("Content Watch Lists", "name=" + DbController.encodeSQLText(ListName), "ID", false, 0, false, false, "ID");
                    if (!this.core.db.csOk(CS)) {
                        this.core.db.csClose(ref CS);
                        CS = this.core.db.csInsertRecord("Content Watch Lists");
                        if (this.core.db.csOk(CS)) {
                            this.core.db.csSet(CS, "name", ListName);
                        }
                    }
                    this.core.db.csClose(ref CS);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        // ====================================================================================================
        // Opens a Content Definition into a ContentSEt
        //   Returns and integer that points into the ContentSet array
        //   If there was a problem, it returns -1
        //
        //   If authoring mode, as group of records are returned.
        //       The first is the current edit record
        //       The rest are the archive records.
        //
        public int csOpenRecord(string ContentName, int RecordID, bool WorkflowAuthoringMode = false, bool WorkflowEditingMode = false, string SelectFieldList = "") {
            return csOpen(GenericController.encodeText(ContentName), "(ID=" + DbController.encodeSQLNumber(RecordID) + ")", "", false, core.session.user.id, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1);
        }
        //
        // ====================================================================================================
        //
        public int csOpen2(string ContentName, int RecordID, bool WorkflowAuthoringMode = false, bool WorkflowEditingMode = false, string SelectFieldList = "") {
            return csOpen(ContentName, "(ID=" + DbController.encodeSQLNumber(RecordID) + ")", "", false, core.session.user.id, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1);
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
                    csClose(0, false);
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

