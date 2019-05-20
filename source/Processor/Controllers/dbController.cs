
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Models.Domain;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Exceptions;
using Contensive.BaseClasses;

namespace Contensive.Processor.Controllers {
    //
    // todo - convert so each datasource has its own dbController - removing the datasource argument from every call
    // todo - this is only a type of database. Need support for noSql datasources also, and maybe read-only static file datasources (like a json file, or xml file)
    //==========================================================================================
    /// <summary>
    /// Data Access Layer for individual catalogs. Properties and Methods related to interfacing with the Database
    /// </summary>
    public partial class DbController : IDisposable {
        //
        /// <summary>
        /// dependencies
        /// </summary>
        private CoreController core;
        /// <summary>
        /// The datasouorce used for this instance of the object
        /// </summary>
        internal string dataSourceName;
        //
        /// <summary>
        /// default page size. Page size is how many records are read in a single fetch.
        /// </summary>
        public const int pageSizeDefault = 9999;
        //
        /// <summary>
        /// set true when configured and tested - else db calls are skipped
        /// </summary>
        private bool dbVerified { get; set; } = false;
        //
        /// <summary>
        /// set true when configured and tested - else db calls are skipped, simple lazy cached values
        /// </summary>
        private bool dbEnabled { get; set; } = true;
        //
        /// <summary>
        /// connection string, lazy cache
        /// </summary>
        private Dictionary<string, string> connectionStringDict { get; set; } = new Dictionary<string, string>();
        //
        /// <summary>
        /// above this threshold, queries are logged as slow
        /// </summary>
        public int sqlSlowThreshholdMsec { get; set; } = 1000;
        //
        /// <summary>
        /// timeout in seconds
        /// </summary>
        public int sqlCommandTimeout { get; set; } = 30;
        //
        //==========================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="core">dependencies</param>
        /// <param name="dataSourceName">The datasource. The default datasource is setup in the config file. Others are in the Datasources table</param>
        public DbController(CoreController core, string dataSourceName) : base() {
            this.core = core;
            this.dataSourceName = dataSourceName;
        }
        //
        //====================================================================================================
        /// <summary>
        /// adonet connection string for this datasource. datasource represents the server, catalog is the application. 
        /// </summary>
        /// <returns>
        /// </returns>
        public string getConnectionStringADONET(string catalogName) {
            //
            // (OLEDB) OLE DB Provider for SQL Server > "Provider=sqloledb;Data Source=MyServerName;Initial Catalog=MyDatabaseName;User Id=MyUsername;Password=MyPassword;"
            //     https://www.codeproject.com/Articles/2304/ADO-Connection-Strings#OLE%20DB%20SqlServer
            //
            // (OLEDB) Microsoft OLE DB Provider for SQL Server connection strings > "Provider=sqloledb;Data Source=myServerAddress;Initial Catalog=myDataBase;User Id = myUsername;Password=myPassword;"
            //     https://www.connectionstrings.com/microsoft-ole-db-provider-for-sql-server-sqloledb/
            //
            // (ADONET) .NET Framework Data Provider for SQL Server > Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password = myPassword;
            //     https://www.connectionstrings.com/sql-server/
            //
            string returnConnString = "";
            try {
                //
                // -- simple local cache so it does not have to be recreated each time
                string key = "catalog:" + catalogName + "/datasource:" + dataSourceName;
                if (connectionStringDict.ContainsKey(key)) {
                    returnConnString = connectionStringDict[key];
                } else {
                    //
                    // -- lookup dataSource
                    string normalizedDataSourceName = DataSourceModel.normalizeDataSourceName(dataSourceName);
                    if ((string.IsNullOrEmpty(normalizedDataSourceName)) || (normalizedDataSourceName == "default")) {
                        //
                        // -- default datasource
                        returnConnString = ""
                        + core.dbServer.getConnectionStringADONET() + "Database=" + catalogName + ";";
                    } else {
                        //
                        // -- custom datasource from Db in primary datasource
                        if (!core.dataSourceDictionary.ContainsKey(normalizedDataSourceName)) {
                            //
                            // -- not found, this is a hard error
                            throw new GenericException("Datasource [" + normalizedDataSourceName + "] was not found.");
                        } else {
                            //
                            // -- found in local cache
                            var tempVar = core.dataSourceDictionary[normalizedDataSourceName];
                            returnConnString = ""
                            + "server=" + tempVar.endpoint + ";"
                            + "User Id=" + tempVar.username + ";"
                            + "Password=" + tempVar.password + ";"
                            + "Database=" + catalogName + ";";
                        }
                    }
                    connectionStringDict.Add(key, returnConnString);
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnConnString;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Execute a query (returns data)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="startRecord"></param>
        /// <param name="maxRecords"></param>
        /// <returns></returns>
        public DataTable executeQuery(string sql, int startRecord, int maxRecords) {
            int recordsReturned = 0;
            return executeQuery(sql, startRecord, maxRecords, ref recordsReturned);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Execute a query (returns data)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="startRecord"></param>
        /// <returns></returns>
        public DataTable executeQuery(string sql, int startRecord) {
            int tempVar = 0;
            return executeQuery(sql, startRecord, Constants.sqlPageSizeDefault, ref tempVar);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Execute a query (returns data) on the default datasource
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable executeQuery(string sql) {
            int tempVar = 0;
            return executeQuery(sql, 0, Constants.sqlPageSizeDefault, ref tempVar);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Execute a query (returns data)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="startRecord"></param>
        /// <param name="maxRecords"></param>
        /// <param name="recordsReturned"></param>
        /// <returns></returns>
        public DataTable executeQuery(string sql, int startRecord, int maxRecords, ref int recordsReturned) {
            DataTable returnData = new DataTable();
            try {
                if (!dbEnabled) { return new DataTable(); }
                if (core.serverConfig == null) { LogController.logError(core, new GenericException("Cannot execute Sql in dbController without an application")); }
                if (core.appConfig == null) { LogController.logError(core, new GenericException("Cannot execute Sql in dbController without an application")); }
                //
                // REFACTOR
                // consider writing cs intrface to sql dataReader object -- one row at a time, vaster.
                // https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqldatareader.aspx
                //
                Stopwatch sw = Stopwatch.StartNew();
                using (SqlConnection connSQL = new SqlConnection(getConnectionStringADONET(core.appConfig.name))) {
                    int retryCnt = 1;
                    bool success = false;
                    do {
                        try {
                            connSQL.Open();
                            success = true;
                        } catch (System.Data.SqlClient.SqlException exSql) {
                            // network related error, retry once
                            LogController.logError(core, "executeQuery SqlException, retries left [" + retryCnt.ToString() + "], ex [" + exSql.ToString() + "]");
                            if (retryCnt <= 0) { throw; }
                            retryCnt--;
                        } catch (Exception) {
                            throw;
                        }
                    } while (!success && (retryCnt >= 0));
                    using (SqlCommand cmdSQL = new SqlCommand()) {
                        cmdSQL.CommandType = CommandType.Text;
                        cmdSQL.CommandText = sql;
                        cmdSQL.Connection = connSQL;
                        using (dynamic adptSQL = new System.Data.SqlClient.SqlDataAdapter(cmdSQL)) {
                            recordsReturned = adptSQL.Fill(startRecord, maxRecords, returnData);
                        }
                    }
                }
                dbVerified = true;
                saveTransactionLog(sql, sw.ElapsedMilliseconds, "query", recordsReturned);
            } catch (Exception ex) {
                LogController.logError(core, new GenericException("Exception [" + ex.Message + "] executing sql [" + sql + "], datasource [" + dataSourceName + "], startRecord [" + startRecord + "], maxRecords [" + maxRecords + "], recordsReturned [" + recordsReturned + "]", ex));
                throw;
            }
            return returnData;
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute sql on a the default datasource. 
        /// </summary>
        /// <param name="sql"></param>
        public void executeNonQuery(string sql) {
            int tempVar = 0;
            executeNonQuery(sql, ref tempVar);
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute a nonQuery command (non-record returning) and return records affected
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="recordsAffected"></param>
        public void executeNonQuery(string sql, ref int recordsAffected) {
            try {
                if (!dbEnabled) { return; }
                Stopwatch sw = Stopwatch.StartNew();
                using (SqlConnection connSQL = new SqlConnection(getConnectionStringADONET(core.appConfig.name))) {
                    connSQL.Open();
                    using (SqlCommand cmdSQL = new SqlCommand()) {
                        cmdSQL.CommandType = CommandType.Text;
                        cmdSQL.CommandText = sql;
                        cmdSQL.Connection = connSQL;
                        recordsAffected = cmdSQL.ExecuteNonQuery();
                    }
                }
                dbVerified = true;
                saveTransactionLog(sql, sw.ElapsedMilliseconds, "non-query", recordsAffected);
            } catch (Exception ex) {
                LogController.logError(core, new GenericException("Exception [" + ex.Message + "] executing sql [" + sql + "], datasource [" + dataSourceName + "], recordsAffected [" + recordsAffected + "]", ex));
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute sql on a specific datasource asynchonously. No data is returned.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        public void executeNonQueryAsync(string sql) {
            try {
                if (!dbEnabled) { return; }
                Stopwatch sw = Stopwatch.StartNew();
                using (SqlConnection connSQL = new SqlConnection(getConnectionStringADONET(core.appConfig.name))) {
                    connSQL.Open();
                    using (SqlCommand cmdSQL = new SqlCommand()) {
                        cmdSQL.CommandType = CommandType.Text;
                        cmdSQL.CommandText = sql;
                        cmdSQL.Connection = connSQL;
                        cmdSQL.ExecuteNonQueryAsync();
                    }
                }
                dbVerified = true;
                saveTransactionLog(sql, sw.ElapsedMilliseconds, "non-query-async", -1);
            } catch (Exception ex) {
                LogController.logError(core, new GenericException("Exception [" + ex.Message + "] executing sql [" + sql + "], datasource [" + dataSourceName + "]", ex));
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Update a record in a table
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="tableName"></param>
        /// <param name="criteria"></param>
        /// <param name="sqlList"></param>
        public void updateTableRecord(string tableName, string criteria, SqlFieldListClass sqlList, bool asyncSave = false) {
            try {
                string SQL = "update " + tableName + " set " + sqlList.getNameValueList() + " where " + criteria + ";";
                if (!asyncSave) {
                    executeNonQuery(SQL);
                } else {
                    executeNonQueryAsync(SQL);
                }
            } catch (Exception ex) {
                LogController.logError(core, new GenericException("Exception [" + ex.Message + "] updating table [" + tableName + "], criteria [" + criteria + "], dataSourceName [" + dataSourceName + "]", ex));
                throw;
            }
        }
        //
        public void updateTableRecord(string tableName, string criteria, SqlFieldListClass sqlList)
            => updateTableRecord(tableName, criteria, sqlList, false);
        //
        //========================================================================
        /// <summary>
        /// iInserts a record into a table and returns the ID
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="tableName"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public int insertTableRecordGetId(string tableName, int memberId) {
            try {
                using (DataTable dt = insertTableRecordGetDataTable(tableName, memberId)) {
                    if (dt.Rows.Count > 0) { return encodeInteger(dt.Rows[0]["id"]); }
                }
                return 0;
            } catch (Exception ex) {
                LogController.logError(core, new GenericException("Exception [" + ex.Message + "] inserting table [" + tableName + "], dataSourceName [" + dataSourceName + "]", ex));
                throw;
            }
        }
        //
        public int insertTableRecordGetId(string tableName)
            => insertTableRecordGetId(tableName, 0);
        //
        //========================================================================
        /// <summary>
        /// Insert a record in a table, select it and return a datatable. You must dispose the datatable.
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="tableName"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public DataTable insertTableRecordGetDataTable(string tableName, int memberId) {
            try {
                string sqlGuid = encodeSQLText(GenericController.getGUID());
                string sqlDateAdded = encodeSQLDate(DateTime.Now);
                SqlFieldListClass sqlList = new SqlFieldListClass();
                sqlList.add("ccGuid", sqlGuid);
                sqlList.add("dateadded", sqlDateAdded);
                sqlList.add("createdby", encodeSQLNumber(memberId));
                sqlList.add("ModifiedDate", sqlDateAdded);
                sqlList.add("ModifiedBy", encodeSQLNumber(memberId));
                sqlList.add("contentControlId", encodeSQLNumber(0));
                sqlList.add("Name", encodeSQLText(""));
                sqlList.add("Active", encodeSQLNumber(1));
                //
                insertTableRecord(tableName, sqlList);
                return openTable(tableName, "(DateAdded=" + sqlDateAdded + ")and(ccguid=" + sqlGuid + ")", "ID DESC", "", 1, 1);
            } catch (Exception ex) {
                LogController.logError(core, new GenericException("Exception [" + ex.Message + "] inserting table [" + tableName + "], dataSourceName [" + dataSourceName + "]", ex));
                throw;
            }
        }
        //
        public DataTable insertTableRecordGetDataTable(string tableName)
            => insertTableRecordGetDataTable(tableName, 0);
        //
        //========================================================================
        /// <summary>
        /// Insert a record in a table. There is no check for core fields
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="tableName"></param>
        /// <param name="sqlList"></param>
        public void insertTableRecord(string tableName, SqlFieldListClass sqlList, bool asyncSave) {
            try {
                if (sqlList.count == 0) { return; }
                string sql = "INSERT INTO " + tableName + "(" + sqlList.getNameList() + ")values(" + sqlList.getValueList() + ")";
                if (!asyncSave) {
                    executeNonQuery(sql);
                    return;
                }
                executeNonQueryAsync(sql);
            } catch (Exception ex) {
                LogController.logError(core, new GenericException("Exception [" + ex.Message + "], inserting table [" + tableName + "], dataSourceName [" + dataSourceName + "]", ex));
                throw;
            }
        }
        //
        public void insertTableRecord(string tableName, SqlFieldListClass sqlList)
            => insertTableRecord(tableName, sqlList, false);
        //
        //========================================================================
        /// <summary>
        /// Opens the table specified and returns the data in a datatable. Returns all the active records in the table. Find the content record first, just for the dataSource.
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="tableName"></param>
        /// <param name="criteria"></param>
        /// <param name="sortFieldList"></param>
        /// <param name="selectFieldList"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public DataTable openTable(string tableName, string criteria, string sortFieldList, string selectFieldList, int pageSize, int pageNumber) {
            DataTable returnDataTable = null;
            try {
                string SQL = "SELECT";
                if (string.IsNullOrEmpty(selectFieldList)) {
                    SQL += " *";
                } else {
                    SQL += " " + selectFieldList;
                }
                SQL += " FROM " + tableName;
                if (!string.IsNullOrEmpty(criteria)) {
                    SQL += " WHERE (" + criteria + ")";
                }
                if (!string.IsNullOrEmpty(sortFieldList)) {
                    SQL += " ORDER BY " + sortFieldList;
                }
                //SQL &= ";"
                returnDataTable = executeQuery(SQL, (pageNumber - 1) * pageSize, pageSize);
            } catch (Exception ex) {
                LogController.logError(core, new GenericException("Exception [" + ex.Message + "], opening table [" + tableName + "], dataSourceName [" + dataSourceName + "]", ex));
                throw;
            }
            return returnDataTable;
        }
        //
        //====================================================================================================
        /// <summary>
        /// update the transaction log
        /// </summary>
        /// <param name="LogEntry"></param>
        private void saveTransactionLog(string sql, long elapsedMilliseconds, string sqlMethodType, int recordsAffected) {
            string logMsg = "type [" + sqlMethodType + "], duration [" + elapsedMilliseconds + "ms], recordsAffected [" + recordsAffected + "], sql [" + sql.Replace("\r", "").Replace("\n", "") + "]";
            if (elapsedMilliseconds > sqlSlowThreshholdMsec) {
                LogController.logWarn(core, "Slow Query " + logMsg);
            } else {
                LogController.logDebug(core, logMsg);
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Returns true if the field exists in the table
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public bool isSQLTableField(string tableName, string fieldName) {
            bool returnOK = false;
            try {
                Models.Domain.TableSchemaModel tableSchema = TableSchemaModel.getTableSchema(core, tableName, dataSourceName);
                if (tableSchema != null) {
                    returnOK = (null != tableSchema.columns.Find(x => x.COLUMN_NAME.ToLowerInvariant() == fieldName.ToLowerInvariant()));
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnOK;
        }
        //
        //========================================================================
        /// <summary>
        /// Returns true if the table exists
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool isSQLTable(string tableName) {
            bool ReturnOK = false;
            try {
                ReturnOK = (!(Models.Domain.TableSchemaModel.getTableSchema(core, tableName, dataSourceName) == null));
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return ReturnOK;
        }
        //   
        //========================================================================
        /// <summary>
        /// Check for a table in a datasource
        /// if the table is missing, create the table and the core fields
        /// if NoAutoIncrement is false or missing, the ID field is created as an auto incremenet
        /// if NoAutoIncrement is true, ID is created an an long
        /// if the table is present, check all core fields
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="allowAutoIncrement"></param>
        public void createSQLTable(string tableName, bool allowAutoIncrement = true) {
            try {
                //
                LogController.logTrace(core, "createSqlTable, DataSourceName [" + dataSourceName + "], TableName [" + tableName + "]");
                //
                if (string.IsNullOrEmpty(tableName)) {
                    //
                    // tablename required
                    //
                    throw new ArgumentException("Tablename can not be blank.");
                } else if (GenericController.vbInstr(1, tableName, ".") != 0) {
                    //
                    // Remote table -- remote system controls remote tables
                    //
                    throw new ArgumentException("Tablename can not contain a period(.)");
                } else {
                    //
                    // Local table -- create if not in schema
                    //
                    if (Models.Domain.TableSchemaModel.getTableSchema(core, tableName, dataSourceName) == null) {
                        if (!allowAutoIncrement) {
                            string SQL = "Create Table " + tableName + "(ID " + getSQLAlterColumnType(CPContentBaseClass.fileTypeIdEnum.Integer) + ");";
                            executeQuery(SQL).Dispose();
                        } else {
                            string SQL = "Create Table " + tableName + "(ID " + getSQLAlterColumnType(CPContentBaseClass.fileTypeIdEnum.AutoIdIncrement) + ");";
                            executeQuery(SQL).Dispose();
                        }
                    }
                    //
                    // ----- Test the common fields required in all tables
                    //
                    createSQLTableField(tableName, "id", CPContentBaseClass.fileTypeIdEnum.AutoIdIncrement);
                    createSQLTableField(tableName, "name", CPContentBaseClass.fileTypeIdEnum.Text);
                    createSQLTableField(tableName, "dateAdded", CPContentBaseClass.fileTypeIdEnum.Date);
                    createSQLTableField(tableName, "createdby", CPContentBaseClass.fileTypeIdEnum.Integer);
                    createSQLTableField(tableName, "modifiedBy", CPContentBaseClass.fileTypeIdEnum.Integer);
                    createSQLTableField(tableName, "modifiedDate", CPContentBaseClass.fileTypeIdEnum.Date);
                    createSQLTableField(tableName, "active", CPContentBaseClass.fileTypeIdEnum.Boolean);
                    createSQLTableField(tableName, "sortOrder", CPContentBaseClass.fileTypeIdEnum.Text);
                    createSQLTableField(tableName, "contentControlId", CPContentBaseClass.fileTypeIdEnum.Integer);
                    createSQLTableField(tableName, "ccGuid", CPContentBaseClass.fileTypeIdEnum.Text);
                    // -- 20171029 - deprecating fields makes migration difficult. add back and figure out future path
                    createSQLTableField(tableName, "createKey", CPContentBaseClass.fileTypeIdEnum.Integer);
                    createSQLTableField(tableName, "contentCategoryId", CPContentBaseClass.fileTypeIdEnum.Integer);
                    //
                    // ----- setup core indexes
                    //
                    // 20171029 primary key dow not need index -- Call createSQLIndex( TableName, TableName & "Id", "ID")
                    createSQLIndex(tableName, tableName + "Active", "active");
                    createSQLIndex(tableName, tableName + "Name", "name");
                    createSQLIndex(tableName, tableName + "SortOrder", "sortOrder");
                    createSQLIndex(tableName, tableName + "DateAdded", "dateAdded");
                    createSQLIndex(tableName, tableName + "ContentControlId", "contentControlId");
                    createSQLIndex(tableName, tableName + "ModifiedDate", "modifiedDate");
                    createSQLIndex(tableName, tableName + "CcGuid", "ccGuid");
                    //createSQLIndex(TableName, TableName + "CreateKey", "createKey");
                }
                Models.Domain.TableSchemaModel.tableSchemaListClear(core);
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Check for a field in a table in the database, if missing, create the field
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldType"></param>
        /// <param name="clearMetadataCache">If true, the metadata cache is cleared on success.</param>
        public void createSQLTableField(string tableName, string fieldName, CPContentBaseClass.fileTypeIdEnum fieldType, bool clearMetadataCache = false) {
            try {
                if ((fieldType == CPContentBaseClass.fileTypeIdEnum.Redirect) || (fieldType == CPContentBaseClass.fileTypeIdEnum.ManyToMany)) { return; }
                if (string.IsNullOrEmpty(tableName)) { throw new ArgumentException("Table Name cannot be blank."); }
                if (fieldType == 0) { throw new ArgumentException("invalid fieldtype [" + fieldType + "]"); }
                if (GenericController.vbInstr(1, tableName, ".") != 0) { throw new ArgumentException("Table name cannot include a period(.)"); }
                if (string.IsNullOrEmpty(fieldName)) { throw new ArgumentException("Field name cannot be blank"); }
                if (!isSQLTableField(tableName, fieldName)) {
                    executeNonQuery("ALTER TABLE " + tableName + " ADD " + fieldName + " " + getSQLAlterColumnType(fieldType));
                    TableSchemaModel.tableSchemaListClear(core);
                    //
                    if (clearMetadataCache) {
                        core.cache.invalidateAll();
                        core.clearMetaData();
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Delete (drop) a table
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="TableName"></param>
        public void deleteTable(string TableName) {
            try {
                executeQuery("DROP TABLE " + TableName).Dispose();
                core.cache.invalidateAll();
                core.clearMetaData();
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Delete a table field from a table
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="TableName"></param>
        /// <param name="FieldName"></param>
        public void deleteTableField(string TableName, string FieldName) {
            try {
                if (isSQLTableField(TableName, FieldName)) {
                    executeQuery("ALTER TABLE " + TableName + " DROP COLUMN " + FieldName + ";");
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Create an index on a table, Fieldnames is  a comma delimited list of fields
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="TableName"></param>
        /// <param name="IndexName"></param>
        /// <param name="FieldNames"></param>
        /// <param name="clearMetaCache"></param>
        public void createSQLIndex(string TableName, string IndexName, string FieldNames, bool clearMetaCache = false) {
            try {
                if (!(string.IsNullOrEmpty(TableName) && string.IsNullOrEmpty(IndexName) & string.IsNullOrEmpty(FieldNames))) {
                    TableSchemaModel ts = TableSchemaModel.getTableSchema(core, TableName, dataSourceName);
                    if (ts != null) {
                        if (null == ts.indexes.Find(x => x.index_name.ToLowerInvariant() == IndexName.ToLowerInvariant())) {
                            executeQuery("CREATE INDEX [" + IndexName + "] ON [" + TableName + "]( " + FieldNames + " );");
                            if (clearMetaCache) {
                                core.cache.invalidateAll();
                                core.clearMetaData();
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Get FieldDescritor from FieldType
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public string getSQLAlterColumnType(CPContentBaseClass.fileTypeIdEnum fieldType) {
            string returnType = "";
            try {
                switch (fieldType) {
                    case CPContentBaseClass.fileTypeIdEnum.Boolean:
                        returnType = "Int NULL";
                        break;
                    case CPContentBaseClass.fileTypeIdEnum.Currency:
                        returnType = "Float NULL";
                        break;
                    case CPContentBaseClass.fileTypeIdEnum.Date:
                        // 20180416 - ms recommends using new, higher precision. Code requires 3 digits so 7 is more than enough
                        returnType = "DateTime2(7) NULL";
                        break;
                    case CPContentBaseClass.fileTypeIdEnum.Float:
                        returnType = "Float NULL";
                        break;
                    case CPContentBaseClass.fileTypeIdEnum.Integer:
                        returnType = "Int NULL";
                        break;
                    case CPContentBaseClass.fileTypeIdEnum.Lookup:
                    case CPContentBaseClass.fileTypeIdEnum.MemberSelect:
                        returnType = "Int NULL";
                        break;
                    case CPContentBaseClass.fileTypeIdEnum.ManyToMany:
                    case CPContentBaseClass.fileTypeIdEnum.Redirect:
                    case CPContentBaseClass.fileTypeIdEnum.FileImage:
                    case CPContentBaseClass.fileTypeIdEnum.Link:
                    case CPContentBaseClass.fileTypeIdEnum.ResourceLink:
                    case CPContentBaseClass.fileTypeIdEnum.Text:
                    case CPContentBaseClass.fileTypeIdEnum.File:
                    case CPContentBaseClass.fileTypeIdEnum.FileText:
                    case CPContentBaseClass.fileTypeIdEnum.FileJavascript:
                    case CPContentBaseClass.fileTypeIdEnum.FileXML:
                    case CPContentBaseClass.fileTypeIdEnum.FileCSS:
                    case CPContentBaseClass.fileTypeIdEnum.FileHTML:
                        returnType = "VarChar(255) NULL";
                        break;
                    case CPContentBaseClass.fileTypeIdEnum.LongText:
                    case CPContentBaseClass.fileTypeIdEnum.HTML:
                        //
                        // ----- Longtext, depends on datasource
                        //
                        returnType = "Text Null";
                        //Select Case DataSourceConnectionObjs(Pointer).Type
                        //    Case DataSourceTypeODBCSQLServer
                        //        csv_returnType = "Text Null"
                        //    Case DataSourceTypeODBCAccess
                        //        csv_returnType = "Memo Null"
                        //    Case DataSourceTypeODBCMySQL
                        //        csv_returnType = "Text Null"
                        //    Case Else
                        //        csv_returnType = "VarChar(65535)"
                        //End Select
                        break;
                    case CPContentBaseClass.fileTypeIdEnum.AutoIdIncrement:
                        //
                        // ----- autoincrement type, depends on datasource
                        //
                        returnType = "INT IDENTITY PRIMARY KEY";
                        //Select Case DataSourceConnectionObjs(Pointer).Type
                        //    Case DataSourceTypeODBCSQLServer
                        //        csv_returnType = "INT IDENTITY PRIMARY KEY"
                        //    Case DataSourceTypeODBCAccess
                        //        csv_returnType = "COUNTER CONSTRAINT PrimaryKey PRIMARY KEY"
                        //    Case DataSourceTypeODBCMySQL
                        //        csv_returnType = "INT AUTO_INCREMENT PRIMARY KEY"
                        //    Case Else
                        //        csv_returnType = "INT AUTO_INCREMENT PRIMARY KEY"
                        //End Select
                        break;
                    default:
                        //
                        // Invalid field type
                        //
                        throw new GenericException("Can not proceed because the field being created has an invalid FieldType [" + fieldType + "]");
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnType;
        }
        //
        //========================================================================
        /// <summary>
        /// Delete an Index for a table
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="TableName"></param>
        /// <param name="IndexName"></param>
        public void deleteSqlIndex(string TableName, string IndexName) {
            try {
                TableSchemaModel ts = TableSchemaModel.getTableSchema(core, TableName, dataSourceName);
                if (ts != null) {
                    if (null != ts.indexes.Find(x => x.index_name.ToLowerInvariant() == IndexName.ToLowerInvariant())) {
                        string sql = "";
                        switch (getDataSourceType()) {
                            case Constants.DataSourceTypeODBCAccess:
                                sql = "DROP INDEX " + IndexName + " On " + TableName + ";";
                                break;
                            default:
                                sql = "DROP INDEX [" + TableName + "].[" + IndexName + "];";
                                break;
                        }
                        executeQuery(sql);
                        core.cache.invalidateAll();
                        core.clearMetaData();
                    }
                }

            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Get a DataSource type (SQL Server, etc) from its Name
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <returns></returns>
        //
        public int getDataSourceType() {
            return Constants.DataSourceTypeODBCSQLServer;
        }
        //
        //========================================================================
        /// <summary>
        /// Get FieldType from ADO Field Type
        /// </summary>
        /// <param name="ADOFieldType"></param>
        /// <returns></returns>
        public CPContentBaseClass.fileTypeIdEnum getFieldTypeIdByADOType(int ADOFieldType) {
            CPContentBaseClass.fileTypeIdEnum returnType = 0;
            try {
                switch (ADOFieldType) {
                    case 2:
                        returnType = CPContentBaseClass.fileTypeIdEnum.Float;
                        break;
                    case 3:
                        returnType = CPContentBaseClass.fileTypeIdEnum.Integer;
                        break;
                    case 4:
                        returnType = CPContentBaseClass.fileTypeIdEnum.Float;
                        break;
                    case 5:
                        returnType = CPContentBaseClass.fileTypeIdEnum.Float;
                        break;
                    case 6:
                        returnType = CPContentBaseClass.fileTypeIdEnum.Integer;
                        break;
                    case 11:
                        returnType = CPContentBaseClass.fileTypeIdEnum.Boolean;
                        break;
                    case 135:
                        returnType = CPContentBaseClass.fileTypeIdEnum.Date;
                        break;
                    case 200:
                        returnType = CPContentBaseClass.fileTypeIdEnum.Text;
                        break;
                    case 201:
                        returnType = CPContentBaseClass.fileTypeIdEnum.LongText;
                        break;
                    case 202:
                        returnType = CPContentBaseClass.fileTypeIdEnum.Text;
                        break;
                    default:
                        returnType = CPContentBaseClass.fileTypeIdEnum.Text;
                        break;
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnType;
        }
        //
        //========================================================================
        /// <summary>
        /// Get FieldType from FieldTypeName
        /// </summary>
        /// <param name="FieldTypeName"></param>
        /// <returns></returns>
        public CPContentBaseClass.fileTypeIdEnum getFieldTypeIdFromFieldTypeName(string FieldTypeName) {
            CPContentBaseClass.fileTypeIdEnum returnTypeId = 0;
            try {
                switch (GenericController.vbLCase(FieldTypeName)) {
                    case Constants.FieldTypeNameLcaseBoolean:
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.Boolean;
                        break;
                    case Constants.FieldTypeNameLcaseCurrency:
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.Currency;
                        break;
                    case Constants.FieldTypeNameLcaseDate:
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.Date;
                        break;
                    case Constants.FieldTypeNameLcaseFile:
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.File;
                        break;
                    case Constants.FieldTypeNameLcaseFloat:
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.Float;
                        break;
                    case Constants.FieldTypeNameLcaseImage:
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.FileImage;
                        break;
                    case Constants.FieldTypeNameLcaseLink:
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.Link;
                        break;
                    case Constants.FieldTypeNameLcaseResourceLink:
                    case "resource link":
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.ResourceLink;
                        break;
                    case Constants.FieldTypeNameLcaseInteger:
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.Integer;
                        break;
                    case Constants.FieldTypeNameLcaseLongText:
                    case "Long text":
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.LongText;
                        break;
                    case Constants.FieldTypeNameLcaseLookup:
                    case "lookuplist":
                    case "lookup list":
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.Lookup;
                        break;
                    case Constants.FieldTypeNameLcaseMemberSelect:
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.MemberSelect;
                        break;
                    case Constants.FieldTypeNameLcaseRedirect:
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.Redirect;
                        break;
                    case Constants.FieldTypeNameLcaseManyToMany:
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.ManyToMany;
                        break;
                    case Constants.FieldTypeNameLcaseTextFile:
                    case "text file":
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.FileText;
                        break;
                    case Constants.FieldTypeNameLcaseCSSFile:
                    case "css file":
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.FileCSS;
                        break;
                    case Constants.FieldTypeNameLcaseXMLFile:
                    case "xml file":
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.FileXML;
                        break;
                    case Constants.FieldTypeNameLcaseJavascriptFile:
                    case "javascript file":
                    case "js file":
                    case "jsfile":
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.FileJavascript;
                        break;
                    case Constants.FieldTypeNameLcaseText:
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.Text;
                        break;
                    case "autoincrement":
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.AutoIdIncrement;
                        break;
                    case Constants.FieldTypeNameLcaseHTML:
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.HTML;
                        break;
                    case Constants.FieldTypeNameLcaseHTMLFile:
                    case "html file":
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.FileHTML;
                        break;
                    default:
                        //
                        // Bad field type is a text field
                        //
                        returnTypeId = CPContentBaseClass.fileTypeIdEnum.Text;
                        break;
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnTypeId;
        }
        //
        //========================================================================
        /// <summary>
        /// get a field value from a dataTable row 
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string getDataRowFieldText(DataRow dr, string fieldName) {
            if (string.IsNullOrWhiteSpace(fieldName)) { throw new ArgumentException("field name cannot be blank"); }
            return dr[fieldName].ToString();
        }
        //
        //========================================================================
        /// <summary>
        /// get a field value from a dataTable row, interpreted as an integer
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static int getDataRowFieldInteger(DataRow dr, string fieldName) => encodeInteger(getDataRowFieldText(dr, fieldName));
        //
        //========================================================================
        /// <summary>
        /// get a field value from a dataTable row, interpreted as a double
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static double getDataRowFieldNumber(DataRow dr, string fieldName) => encodeNumber(getDataRowFieldText(dr, fieldName));
        //
        //========================================================================
        /// <summary>
        /// get a field value from a dataTable row, interpreted as a boolean 
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static bool getDataRowFieldBoolean(DataRow dr, string fieldName) => encodeBoolean(getDataRowFieldText(dr, fieldName));
        //
        //========================================================================
        /// <summary>
        /// get a field value from a dataTable row, interpreted as a date
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static DateTime getDataRowFieldDate(DataRow dr, string fieldName) => encodeDate(getDataRowFieldText(dr, fieldName));
        //
        // ====================================================================================================
        /// <summary>
        /// return a sql compatible string. 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string encodeSQLText(string expression) {
            if (expression == null) { return "null"; }
            string returnResult = GenericController.encodeText(expression);
            if (string.IsNullOrEmpty(returnResult)) {
                return "null";
            } else {
                return "'" + GenericController.vbReplace(returnResult, "'", "''") + "'";
            }
        }
        //
        // ====================================================================================================
        /// <summary>
        /// encode a string for a like operation
        /// </summary>
        /// <param name="core"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string encodeSqlTextLike(string source) {
            return encodeSQLText("%" + source + "%");
        }
        //
        // ====================================================================================================
        /// <summary>
        ///    encodeSQLDate
        /// </summary>
        /// <param name="expressionDate"></param>
        /// <returns></returns>
        //
        public static string encodeSQLDate(DateTime expressionDate) {
            try {
                if (Convert.IsDBNull(expressionDate)) { return "null"; }
                if (expressionDate == DateTime.MinValue) { return "null"; }
                return "'" + expressionDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            } catch (Exception) {
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// encodeSQLNumber
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        //
        public static string encodeSQLNumber(double expression) {
            return expression.ToString();
        }
        //
        //========================================================================
        //
        public static string encodeSQLNumber(int expression) {
            return expression.ToString();
        }
        //
        //========================================================================
        /// <summary>
        /// encodeSQLBoolean
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        //
        public static string encodeSQLBoolean(bool expression) {
            if (expression) { return SQLTrue; }
            return SQLFalse;
        }
        //
        //========================================================================
        /// <summary>
        /// delete a record
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="tableName"></param>
        /// <param name="recordId"></param>
        //
        public void deleteTableRecord(int recordId, string tableName) {
            try {
                if (string.IsNullOrEmpty(tableName.Trim())) { throw new GenericException("tablename cannot be blank"); }
                if (recordId <= 0) { throw new GenericException("record id is not valid [" + recordId + "]"); }
                executeNonQuery("delete from " + tableName + " where id=" + recordId);
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        /// <summary>
        /// delete a record based on a guid
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="tableName"></param>
        /// <param name="dataSourceName"></param>
        public void deleteTableRecord(string guid, string tableName) {
            try {
                if (string.IsNullOrWhiteSpace(tableName)) { throw new GenericException("tablename cannot be blank"); }
                if (!isGuid(guid)) { throw new GenericException("Guid is not valid [" + guid + "]"); }
                executeNonQuery("delete from " + tableName + " where ccguid=" + encodeSQLText(guid));
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// DeleteTableRecords
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="tableName"></param>
        /// <param name="criteria"></param>
        //
        public void deleteTableRecords(string tableName, string criteria) {
            try {
                if (string.IsNullOrEmpty(tableName)) { throw new ArgumentException("TableName cannot be blank"); }
                if (string.IsNullOrEmpty(criteria)) { throw new ArgumentException("Criteria cannot be blank"); }
                executeQuery("delete from " + tableName + " where " + criteria);
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="from"></param>
        /// <param name="fieldList"></param>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="groupBy"></param>
        /// <param name="recordLimit"></param>
        /// <returns></returns>
        public string getSQLSelect(string from, string fieldList, string where, string orderBy, string groupBy, int recordLimit) {
            string sql = "select";
            if (recordLimit != 0) { sql += " top " + recordLimit; }
            sql += (string.IsNullOrWhiteSpace(fieldList)) ? " *" : " " + fieldList;
            sql += " from " + from;
            if (!string.IsNullOrWhiteSpace(where)) { sql += " where " + where; }
            if (!string.IsNullOrWhiteSpace(orderBy)) { sql += " order by " + orderBy; }
            if (!string.IsNullOrWhiteSpace(groupBy)) { sql += " group by " + groupBy; }
            return sql;
        }
        //
        public string getSQLSelect(string from, string fieldList, string where, string orderBy, string groupBy) => getSQLSelect(from, fieldList, where, orderBy, groupBy, 0);
        //
        public string getSQLSelect(string from, string fieldList, string where, string orderBy) => getSQLSelect(from, fieldList, where, orderBy, "", 0);
        //
        public string getSQLSelect(string from, string fieldList, string where) => getSQLSelect(from, fieldList, where, "", "", 0);
        //
        public string getSQLSelect(string from, string fieldList) => getSQLSelect(from, fieldList, "", "", "", 0);
        //
        public string getSQLSelect(string from) => getSQLSelect(from, "", "", "", "", 0);
        //
        //========================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        //
        public string getSQLIndexList(string TableName) {
            string returnList = "";
            try {
                Models.Domain.TableSchemaModel ts = Models.Domain.TableSchemaModel.getTableSchema(core, TableName, dataSourceName);
                if (ts != null) {
                    foreach (TableSchemaModel.IndexSchemaModel index in ts.indexes) {
                        returnList += "," + index.index_name;
                    }
                    if (returnList.Length > 0) {
                        returnList = returnList.Substring(2);
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnList;
        }
        //
        //========================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        //
        public DataTable getTableSchemaData(string tableName) {
            DataTable returnDt = new DataTable();
            try {
                string connString = getConnectionStringADONET(core.appConfig.name);
                using (SqlConnection connSQL = new SqlConnection(connString)) {
                    connSQL.Open();
                    returnDt = connSQL.GetSchema("Tables", new[] { core.appConfig.name, null, tableName, null });
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnDt;
        }
        //
        //========================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        //
        public DataTable getColumnSchemaData(string tableName) {
            DataTable returnDt = new DataTable();
            try {
                if (string.IsNullOrEmpty(tableName.Trim())) {
                    throw new ArgumentException("tablename cannot be blank");
                } else {
                    string connString = getConnectionStringADONET(core.appConfig.name);
                    using (SqlConnection connSQL = new SqlConnection(connString)) {
                        connSQL.Open();
                        returnDt = connSQL.GetSchema("Columns", new[] { core.appConfig.name, null, tableName, null });
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnDt;
        }
        //
        //========================================================================
        //
        public DataTable getIndexSchemaData(string tableName) {
            DataTable returnDt = new DataTable();
            try {
                if (string.IsNullOrEmpty(tableName.Trim())) {
                    throw new ArgumentException("tablename cannot be blank");
                } else {
                    string connString = getConnectionStringADONET(core.appConfig.name);
                    using (SqlConnection connSQL = new SqlConnection(connString)) {
                        connSQL.Open();
                        returnDt = connSQL.GetSchema("Indexes", new[] { core.appConfig.name, null, tableName, null });
                    }
                }
                //
                returnDt = executeQuery("sys.sp_helpindex @objname = N'" + tableName + "'");
                // EXEC sys.sp_helpindex @objname = N'cccontent' returns index_name, index_keys (comma delimited field list)
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnDt;
        }
        //
        //=============================================================================
        /// <summary>
        /// get Sql Criteria for string that could be id, guid or name
        /// </summary>
        /// <param name="nameIdOrGuid"></param>
        /// <returns></returns>
        public string getNameIdOrGuidSqlCriteria(string nameIdOrGuid) {
            string sqlCriteria = "";
            try {
                if (nameIdOrGuid.IsNumeric()) {
                    sqlCriteria = "id=" + encodeSQLNumber(double.Parse(nameIdOrGuid));
                } else if (GenericController.common_isGuid(nameIdOrGuid)) {
                    sqlCriteria = "ccGuid=" + encodeSQLText(nameIdOrGuid);
                } else {
                    sqlCriteria = "name=" + encodeSQLText(nameIdOrGuid);
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return sqlCriteria;
        }
        //
        //====================================================================================================
        /// <summary>
        /// convert a dtaTable to a simple array - quick way to adapt old code
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        ///
        public string[,] convertDataTabletoArray(DataTable dt) {
            string[,] rows = { { } };
            try {
                int columnCnt = 0;
                int rowCnt = 0;
                int cPtr = 0;
                int rPtr = 0;
                //
                // 20150717 check for no columns
                if ((dt.Rows.Count > 0) && (dt.Columns.Count > 0)) {
                    columnCnt = dt.Columns.Count;
                    rowCnt = dt.Rows.Count;
                    // 20150717 change from rows(columnCnt,rowCnt) because other routines appear to use this count
                    rows = new string[columnCnt, rowCnt];
                    rPtr = 0;
                    foreach (DataRow dr in dt.Rows) {
                        cPtr = 0;
                        foreach (DataColumn cell in dt.Columns) {
                            rows[cPtr, rPtr] = GenericController.encodeText(dr[cell]);
                            cPtr += 1;
                        }
                        rPtr += 1;
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return rows;
        }
        //
        //========================================================================
        /// <summary>
        /// DeleteTableRecordChunks
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="TableName"></param>
        /// <param name="Criteria"></param>
        /// <param name="ChunkSize"></param>
        /// <param name="MaxChunkCount"></param>
        public void deleteTableRecordChunks(string TableName, string Criteria, int ChunkSize = 1000, int MaxChunkCount = 1000) {
            //
            int PreviousCount = 0;
            int CurrentCount = 0;
            int LoopCount = 0;
            string SQL = null;
            int iChunkSize = 0;
            int iChunkCount = 0;
            //dim dt as datatable
            int DataSourceType;
            //
            DataSourceType = getDataSourceType();
            if ((DataSourceType != DataSourceTypeODBCSQLServer) && (DataSourceType != DataSourceTypeODBCAccess)) {
                //
                // If not SQL server, just delete them
                //
                deleteTableRecords(TableName, Criteria);
            } else {
                //
                // ----- Clear up to date for the properties
                //
                iChunkSize = ChunkSize;
                if (iChunkSize == 0) {
                    iChunkSize = 1000;
                }
                iChunkCount = MaxChunkCount;
                if (iChunkCount == 0) {
                    iChunkCount = 1000;
                }
                //
                // Get an initial count and allow for timeout
                //
                PreviousCount = -1;
                LoopCount = 0;
                CurrentCount = 0;
                SQL = "select count(*) as RecordCount from " + TableName + " where " + Criteria;
                DataTable dt = executeQuery(SQL);
                if (dt.Rows.Count > 0) {
                    CurrentCount = GenericController.encodeInteger(dt.Rows[0][0]);
                }
                while ((CurrentCount != 0) && (PreviousCount != CurrentCount) && (LoopCount < iChunkCount)) {
                    if (getDataSourceType() == DataSourceTypeODBCMySQL) {
                        SQL = "delete from " + TableName + " where id in (select ID from " + TableName + " where " + Criteria + " limit " + iChunkSize + ")";
                    } else {
                        SQL = "delete from " + TableName + " where id in (select top " + iChunkSize + " ID from " + TableName + " where " + Criteria + ")";
                    }
                    executeQuery(SQL);
                    PreviousCount = CurrentCount;
                    SQL = "select count(*) as RecordCount from " + TableName + " where " + Criteria;
                    dt = executeQuery(SQL);
                    if (dt.Rows.Count > 0) {
                        CurrentCount = GenericController.encodeInteger(dt.Rows[0][0]);
                    }
                    LoopCount = LoopCount + 1;
                }
                if ((CurrentCount != 0) && (PreviousCount == CurrentCount)) {
                    //
                    // records did not delete
                    //
                    LogController.logError(core, new GenericException("Error deleting record chunks. No records were deleted and the process was not complete."));
                } else if (LoopCount >= iChunkCount) {
                    //
                    // records did not delete
                    //
                    LogController.logError(core, new GenericException("Error deleting record chunks. The maximum chunk count was exceeded while deleting records."));
                }
            }
        }
        //
        // ====================================================================================================
        //
        public static string encodeSqlTableName(string sourceName) {
            string returnName = "";
            const string FirstCharSafeString = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string SafeString = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_@#";
            try {
                string src = null;
                string TestChr = null;
                int Ptr = 0;
                //
                // remove nonsafe URL characters
                //
                src = sourceName;
                returnName = "";
                // first character
                while (Ptr < src.Length) {
                    TestChr = src.Substring(Ptr, 1);
                    Ptr += 1;
                    if (FirstCharSafeString.IndexOf(TestChr) >= 0) {
                        returnName += TestChr;
                        break;
                    }
                }
                // non-first character
                while (Ptr < src.Length) {
                    TestChr = src.Substring(Ptr, 1);
                    Ptr += 1;
                    if (SafeString.IndexOf(TestChr) >= 0) {
                        returnName += TestChr;
                    }
                }
            } catch (Exception ex) {
                // shared method, rethrow error
                throw new GenericException("Exception in encodeSqlTableName(" + sourceName + ")", ex);
            }
            return returnName;
        }
        //
        //=================================================================================
        //
        public static bool isDataTableOk(DataTable dt) {
            if (dt == null) { return false; }
            if (dt.Rows == null) { return false; }
            return (dt.Rows.Count > 0);
        }
        //
        //=================================================================================
        //
        public static void closeDataTable(DataTable dt) {
            dt.Dispose();
        }
        //
        //====================================================================================================
        //
        public DataTable executeRemoteQuery(string remoteQueryKey) {
            DataTable result = null;
            try {
                var remoteQuery = Models.Db.RemoteQueryModel.create(core, remoteQueryKey);
                if (remoteQuery == null) {
                    throw new GenericException("remoteQuery was not found with key [" + remoteQueryKey + "]");
                } else {
                    result = executeQuery(remoteQuery.SQLQuery);
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                result = null;
            }
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Return just the tablename from a tablename reference (database.object.tablename->tablename)
        /// </summary>
        /// <param name="DbObject"></param>
        /// <returns></returns>
        public static string getDbObjectTableName(string DbObject) {
            int Position = DbObject.LastIndexOf(".") + 1;
            if (Position > 0) {
                return DbObject.Substring(Position);
            }
            return "";
        }
        //
        //=============================================================================
        /// <summary>
        /// Get a ContentID from the ContentName using just the tables
        /// </summary>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public static int getContentId(CoreController core, string contentName) {
            using (DataTable dt = core.db.executeQuery("select top 1 id from cccontent where name=" + encodeSQLText(contentName) + " order by id")) {
                if (dt.Rows.Count == 0) { return 0; }
                return getDataRowFieldInteger(dt.Rows[0], "id");
            }
        }
        //
        //=============================================================================
        /// <summary>
        /// Get a Content Field Id (id of the ccFields table record) using just the database
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static int getContentFieldId(CoreController core, int contentId, string fieldName) {
            if ((contentId <= 0) || (string.IsNullOrWhiteSpace(fieldName))) { return 0; }
            using (DataTable dt = core.db.executeQuery("select top 1 id from ccfields where (contentid=" + contentId + ")and(name=" + encodeSQLText(fieldName) + ") order by id")) {
                if (dt.Rows.Count == 0) { return 0; }
                return getDataRowFieldInteger(dt.Rows[0], "id");
            }
        }
        //
        //=============================================================================
        /// <summary>
        /// Get a Content Field Id (id of the ccFields table record) using just the database
        /// </summary>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public static int getContentFieldId(CoreController core, string contentName, string fieldName) {
            if ((string.IsNullOrWhiteSpace(contentName)) || (string.IsNullOrWhiteSpace(fieldName))) { return 0; }
            return getContentFieldId(core, getContentId(core, contentName), fieldName);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// get the id of the table in the cctable table
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static int getTableID(CoreController core, string TableName) {
            if ((string.IsNullOrWhiteSpace(TableName))) { return 0; }
            using (DataTable dt = core.db.executeQuery("select top 1 id from cctables where name=" + encodeSQLText(TableName) + " order by id")) {
                if (dt.Rows.Count == 0) { return 0; }
                return getDataRowFieldInteger(dt.Rows[0], "id");
            }
        }
        /// <summary>
        /// model for simplest generic recorsd
        /// </summary>
        #region  IDisposable Support 
        protected bool disposed = false;
        //
        //==========================================================================================
        /// <summary>
        /// dispose
        /// </summary>
        /// <param name="disposing"></param>
        /// <remarks></remarks>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // ----- call .dispose for managed objects
                    //
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~DbController() {
            Dispose(false);
        }
        #endregion
    }
    //
    // ====================================================================================================
    /// <summary>
    /// Model to create name=value lists
    /// </summary>
    public class SqlFieldListClass {
        /// <summary>
        /// store
        /// </summary>
        private List<NameValueClass> _sqlList = new List<NameValueClass>();
        /// <summary>
        /// add a name=value pair to the list. values MUST be punctuated correctly for their type in sql (quoted text, etc)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void add(string name, string value) {
            _sqlList.Add(new NameValueClass() {
                name = name,
                value = value
            });
        }
        /// <summary>
        /// get name=value list
        /// </summary>
        /// <returns></returns>
        public string getNameValueList() {
            string returnPairs = "";
            string delim = "";
            foreach (var nameValue in _sqlList) {
                returnPairs += delim + nameValue.name + "=" + nameValue.value;
                delim = ",";
            }
            return returnPairs;
        }
        /// <summary>
        /// get list of names
        /// </summary>
        /// <returns></returns>
        public string getNameList() {
            string returnPairs = "";
            string delim = "";
            foreach (var nameValue in _sqlList) {
                returnPairs += delim + nameValue.name;
                delim = ",";
            }
            return returnPairs;
        }
        /// <summary>
        /// get list of values
        /// </summary>
        /// <returns></returns>
        public string getValueList() {
            string returnPairs = "";
            string delim = "";
            foreach (var nameValue in _sqlList) {
                returnPairs += delim + nameValue.value;
                delim = ",";
            }
            return returnPairs;
        }
        /// <summary>
        /// get count of name=value pairs
        /// </summary>
        public int count {
            get {
                return _sqlList.Count;
            }
        }

    }
}
