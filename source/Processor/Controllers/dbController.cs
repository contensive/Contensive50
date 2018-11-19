
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Exceptions;
using Contensive.Addons.AdminSite.Controllers;
//
namespace Contensive.Processor.Controllers {
    //
    // todo - convert so each datasource has its own dbController - removing the datasource argument from every call
    // todo - this is only a type of database. Need support for noSql datasources also, and maybe read-only static file datasources (like a json file, or xml file)
    //==========================================================================================
    /// <summary>
    /// Manage an individual catalog within a server.
    /// </summary>
    public partial class DbController : IDisposable {
        //
        private CoreController core;
        //
        /// <summary>
        /// default page size. Page size is how many records are read in a single fetch.
        /// </summary>
        private const int pageSizeDefault = 9999;
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
        public int sqlCommandTimeout { get; set; } = 30;
        //
        private ContentSetClass[] contentSetStore = new ContentSetClass[] { };
        //
        /// <summary>
        /// number of elements being used
        /// </summary>
        private int contentSetStoreCount { get; set; }
        //
        /// <summary>
        /// number of elements available for use.
        /// </summary>
        private int contentSetStoreSize { get; set; }
        //
        /// <summary>
        /// How many are added at a time
        /// </summary>
        private const int contentSetStoreChunk = 50;
        //
        /// <summary>
        /// when true, all csOpen, etc, will be setup, but not return any data (csv_IsCSOK false), this is used to generate the csv_ContentSet.Source so we can run a csv_GetContentRows without first opening a recordset
        /// </summary>
        private bool contentSetOpenWithoutRecords = false;
        //
        // todo - this shouldbe moved into a contentSet model with most of what is not the csController
        //  ContentSet Type, Stores pointers to open recordsets of content being used by the page
        private class ContentSetClass {
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
            public Models.Domain.CDefModel CDef;
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
        }
        //
        //==========================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        public DbController(CoreController core) : base() {
            this.core = core;
        }
        //
        //====================================================================================================
        /// <summary>
        /// adonet connection string for this datasource. datasource represents the server, catalog is the application. 
        /// </summary>
        /// <returns>
        /// </returns>
        public string getConnectionStringADONET(string catalogName, string dataSourceName = "") {
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
                            + "server=" + tempVar.endPoint + ";"
                            + "User Id=" + tempVar.username + ";"
                            + "Password=" + tempVar.password + ";"
                            + "Database=" + catalogName + ";";
                        }
                    }
                    connectionStringDict.Add(key, returnConnString);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnConnString;
        }
        //
        //====================================================================================================
        /// <summary>
        /// legacy db model for ole. Leave in place to help remember that future technologies may have to fit into the db model.
        /// </summary>
        /// <returns>
        /// </returns>
        private string getConnectionStringOLEDB(string catalogName, string dataSourceName) {
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
                string normalizedDataSourceName = DataSourceModel.normalizeDataSourceName(dataSourceName);
                string defaultConnString = "";
                string serverUrl = core.serverConfig.defaultDataSourceAddress;
                if (serverUrl.IndexOf(":") > 0) {
                    serverUrl = serverUrl.Left(serverUrl.IndexOf(":"));
                }
                defaultConnString += ""
                    + "Provider=sqloledb;"
                    + "Data Source=" + serverUrl + ";"
                    + "Initial Catalog=" + catalogName + ";"
                    + "User Id=" + core.serverConfig.defaultDataSourceUsername + ";"
                    + "Password=" + core.serverConfig.defaultDataSourcePassword + ";"
                    + "";
                //
                // -- lookup dataSource
                if ((string.IsNullOrEmpty(normalizedDataSourceName)) || (normalizedDataSourceName == "default")) {
                    //
                    // -- default datasource
                    returnConnString = defaultConnString;
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
                        returnConnString += ""
                            + "Provider=sqloledb;"
                            + "Data Source=" + tempVar.endPoint + ";"
                            + "User Id=" + tempVar.username + ";"
                            + "Password=" + tempVar.password + ";"
                            + "";
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnConnString;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Execute a command (sql statemwent) and return a dataTable object
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="startRecord"></param>
        /// <param name="maxRecords"></param>
        /// <returns></returns>
        public DataTable executeQuery(string sql, string dataSourceName, int startRecord, int maxRecords) {
            int recordsReturned = 0;
            return executeQuery(sql, dataSourceName, startRecord, maxRecords, ref recordsReturned);
        }
        //
        //====================================================================================================
        //
        public DataTable executeQuery(string sql, string dataSourceName, int startRecord) {
            int tempVar = 0;
            return executeQuery(sql, dataSourceName, startRecord, 9999, ref tempVar);
        }
        //
        //====================================================================================================
        //
        public DataTable executeQuery(string sql, string dataSourceName) {
            int tempVar = 0;
            return executeQuery(sql, dataSourceName, 0, 9999, ref tempVar);
        }
        //
        //====================================================================================================
        //
        public DataTable executeQuery(string sql) {
            int tempVar = 0;
            return executeQuery(sql, "", 0, 9999, ref tempVar);
        }
        //
        //====================================================================================================
        //
        public DataTable executeQuery(string sql, string dataSourceName, int startRecord, int maxRecords, ref int recordsReturned) {
            DataTable returnData = new DataTable();
            try {
                if (!dbEnabled) {
                    //
                    // -- db not available
                } else if (core.serverConfig == null) {
                    //
                    // -- server config fail
                    LogController.handleError( core,new GenericException("Cannot execute Sql in dbController without an application"));
                } else if (core.appConfig == null) {
                    //
                    // -- server config fail
                    LogController.handleError( core,new GenericException("Cannot execute Sql in dbController without an application"));
                } else {
                    string connString = getConnectionStringADONET(core.appConfig.name, dataSourceName);
                    //returnData = executeSql_noErrorHandling(sql, getConnectionStringADONET(core.appConfig.name, dataSourceName), startRecord, maxRecords, recordsAffected)
                    //
                    // REFACTOR
                    // consider writing cs intrface to sql dataReader object -- one row at a time, vaster.
                    // https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqldatareader.aspx
                    //
                    Stopwatch sw = Stopwatch.StartNew();
                    using (SqlConnection connSQL = new SqlConnection(connString)) {
                        connSQL.Open();
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
                    saveTransactionLog(sql, sw.ElapsedMilliseconds, "query");
                }
            } catch (Exception ex) {
                ApplicationException newEx = new GenericException("Exception [" + ex.Message + "] executing sql [" + sql + "], datasource [" + dataSourceName + "], startRecord [" + startRecord + "], maxRecords [" + maxRecords + "]", ex);
                LogController.handleError( core,newEx);
            }
            return returnData;
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute sql on a specific datasource. No data is returned.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        public void executeNonQuery(string sql, string dataSourceName) {
            int tempVar = 0;
            executeNonQuery(sql, dataSourceName, ref tempVar);
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute sql on a the default datasource. 
        /// </summary>
        /// <param name="sql"></param>
        public void executeNonQuery(string sql) {
            int tempVar = 0;
            executeNonQuery(sql, "", ref tempVar);
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute sql and return records affected
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="recordsAffected"></param>
        public void executeNonQuery(string sql, string dataSourceName, ref int recordsAffected) {
            try {
                if (dbEnabled) {
                    Stopwatch sw = Stopwatch.StartNew();
                    string connString = getConnectionStringADONET(core.appConfig.name, dataSourceName);
                    using (SqlConnection connSQL = new SqlConnection(connString)) {
                        connSQL.Open();
                        using (SqlCommand cmdSQL = new SqlCommand()) {
                            cmdSQL.CommandType = CommandType.Text;
                            cmdSQL.CommandText = sql;
                            cmdSQL.Connection = connSQL;
                            recordsAffected = cmdSQL.ExecuteNonQuery();
                        }
                    }
                    dbVerified = true;
                    saveTransactionLog(sql, sw.ElapsedMilliseconds, "non-query");
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public void executeNonQueryAsync(string sql, string dataSourceName = "") {
            try {
                if (dbEnabled) {
                    Stopwatch sw = Stopwatch.StartNew();
                    string connString = getConnectionStringADONET(core.appConfig.name, dataSourceName);
                    using (SqlConnection connSQL = new SqlConnection(connString)) {
                        connSQL.Open();
                        using (SqlCommand cmdSQL = new SqlCommand()) {
                            cmdSQL.CommandType = CommandType.Text;
                            cmdSQL.CommandText = sql;
                            cmdSQL.Connection = connSQL;
                            cmdSQL.ExecuteNonQueryAsync();
                        }
                    }
                    dbVerified = true;
                    saveTransactionLog(sql, sw.ElapsedMilliseconds, "non-query-async");
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public void updateTableRecord(string dataSourceName, string tableName, string criteria, SqlFieldListClass sqlList, bool asyncSave = false ) {
            try {
                string SQL = "update " + tableName + " set " + sqlList.getNameValueList() + " where " + criteria + ";";
                if (!asyncSave) {
                    executeNonQuery(SQL, dataSourceName);
                } else {
                    executeNonQueryAsync(SQL, dataSourceName);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// iInserts a record into a table and returns the ID
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="tableName"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public int insertTableRecordGetId(string dataSourceName, string tableName, int memberId = 0) {
            int returnId = 0;
            try {
                using (DataTable dt = insertTableRecordGetDataTable(dataSourceName, tableName, memberId)) {
                    if (dt.Rows.Count > 0) {
                        returnId = GenericController.encodeInteger(dt.Rows[0]["id"]);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnId;
        }
        //
        //========================================================================
        /// <summary>
        /// Insert a record in a table, select it and return a datatable. You must dispose the datatable.
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <param name="tableName"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public DataTable insertTableRecordGetDataTable(string dataSourceName, string tableName, int memberId = 0) {
            DataTable returnDt = null;
            try {
                SqlFieldListClass sqlList = new SqlFieldListClass();
                //string CreateKeyString = null;
                //string sqlDateAdded = null;
                string sqlGuid = encodeSQLText( GenericController.getGUID());
                string sqlDateAdded = encodeSQLDate(DateTime.Now);
                //CreateKeyString = encodeSQLNumber(genericController.GetRandomInteger(core));
                sqlList.add("ccguid", sqlGuid);
                //sqlList.add("createkey", CreateKeyString);
                sqlList.add("dateadded", sqlDateAdded);
                sqlList.add("createdby", encodeSQLNumber(memberId));
                sqlList.add("ModifiedDate", sqlDateAdded);
                sqlList.add("ModifiedBy", encodeSQLNumber(memberId));
                sqlList.add("ContentControlID", encodeSQLNumber(0));
                sqlList.add("Name", encodeSQLText(""));
                sqlList.add("Active", encodeSQLNumber(1));
                //
                insertTableRecord(dataSourceName, tableName, sqlList);
                returnDt = openTable(dataSourceName, tableName, "(DateAdded=" + sqlDateAdded + ")and(ccguid=" + sqlGuid + ")", "ID DESC", "", 1);
                //returnDt = openTable(DataSourceName, TableName, "(DateAdded=" + DateAddedString + ")and(CreateKey=" + CreateKeyString + ")", "ID DESC", "", 1);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnDt;
        }
        //
        //========================================================================
        /// <summary>
        /// Insert a record in a table. There is no check for core fields
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="TableName"></param>
        /// <param name="sqlList"></param>
        public void insertTableRecord(string DataSourceName, string TableName, SqlFieldListClass sqlList, bool asyncSave = false ) {
            try {
                if (sqlList.count > 0) {
                    string sql = "INSERT INTO " + TableName + "(" + sqlList.getNameList() + ")values(" + sqlList.getValueList() + ")";
                    if ( !asyncSave ) {
                        executeNonQuery(sql, DataSourceName);
                    } else {
                        executeNonQueryAsync(sql, DataSourceName);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Opens the table specified and returns the data in a datatable. Returns all the active records in the table. Find the content record first, just for the dataSource.
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="TableName"></param>
        /// <param name="Criteria"></param>
        /// <param name="SortFieldList"></param>
        /// <param name="SelectFieldList"></param>
        /// <param name="PageSize"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        public DataTable openTable(string DataSourceName, string TableName, string Criteria = "", string SortFieldList = "", string SelectFieldList = "", int PageSize = 9999, int PageNumber = 1) {
            DataTable returnDataTable = null;
            try {
                string SQL = "SELECT";
                if (string.IsNullOrEmpty(SelectFieldList)) {
                    SQL += " *";
                } else {
                    SQL += " " + SelectFieldList;
                }
                SQL += " FROM " + TableName;
                if (!string.IsNullOrEmpty(Criteria)) {
                    SQL += " WHERE (" + Criteria + ")";
                }
                if (!string.IsNullOrEmpty(SortFieldList)) {
                    SQL += " ORDER BY " + SortFieldList;
                }
                //SQL &= ";"
                returnDataTable = executeQuery(SQL, DataSourceName, (PageNumber - 1) * PageSize, PageSize);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        private void saveTransactionLog(string sql, long ElapsedMilliseconds, string sqlMethodType) {
            string logMsg = "type [" + sqlMethodType + "], duration [" + ElapsedMilliseconds + "ms], sql [" + sql.Replace("\r", "").Replace("\n", "") + "]";
            if (ElapsedMilliseconds > sqlSlowThreshholdMsec) {
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
        /// <param name="TableName"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public bool isSQLTableField(string DataSourceName, string TableName, string FieldName) {
            bool returnOK = false;
            try {
                Models.Domain.TableSchemaModel tableSchema = Models.Domain.TableSchemaModel.getTableSchema(core, TableName, DataSourceName);
                if (tableSchema != null) {
                    returnOK = (null != tableSchema.columns.Find(x => x.COLUMN_NAME.ToLowerInvariant() == FieldName.ToLowerInvariant()));
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnOK;
        }
        //
        //========================================================================
        /// <summary>
        /// Returns true if the table exists
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public bool isSQLTable(string DataSourceName, string TableName) {
            bool ReturnOK = false;
            try {
                ReturnOK = (!(Models.Domain.TableSchemaModel.getTableSchema(core, TableName, DataSourceName) == null));
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        /// <param name="DataSourceName"></param>
        /// <param name="TableName"></param>
        /// <param name="AllowAutoIncrement"></param>
        public void createSQLTable(string DataSourceName, string TableName, bool AllowAutoIncrement = true) {
            try {
                //
                LogController.logTrace(core, "createSqlTable, DataSourceName [" + DataSourceName + "], TableName [" + TableName + "]");
                //
                if (string.IsNullOrEmpty(TableName)) {
                    //
                    // tablename required
                    //
                    throw new ArgumentException("Tablename can not be blank.");
                } else if (GenericController.vbInstr(1, TableName, ".") != 0) {
                    //
                    // Remote table -- remote system controls remote tables
                    //
                    throw new ArgumentException("Tablename can not contain a period(.)");
                } else {
                    //
                    // Local table -- create if not in schema
                    //
                    if (Models.Domain.TableSchemaModel.getTableSchema(core, TableName, DataSourceName) == null) {
                        if (!AllowAutoIncrement) {
                            string SQL = "Create Table " + TableName + "(ID " + getSQLAlterColumnType(DataSourceName, fieldTypeIdInteger) + ");";
                            executeQuery(SQL, DataSourceName).Dispose();
                        } else {
                            string SQL = "Create Table " + TableName + "(ID " + getSQLAlterColumnType(DataSourceName, fieldTypeIdAutoIdIncrement) + ");";
                            executeQuery(SQL, DataSourceName).Dispose();
                        }
                    }
                    //
                    // ----- Test the common fields required in all tables
                    //
                    createSQLTableField(DataSourceName, TableName, "id", fieldTypeIdAutoIdIncrement);
                    createSQLTableField(DataSourceName, TableName, "name", fieldTypeIdText);
                    createSQLTableField(DataSourceName, TableName, "dateAdded", fieldTypeIdDate);
                    createSQLTableField(DataSourceName, TableName, "createdby", fieldTypeIdInteger);
                    createSQLTableField(DataSourceName, TableName, "modifiedBy", fieldTypeIdInteger);
                    createSQLTableField(DataSourceName, TableName, "ModifiedDate", fieldTypeIdDate);
                    createSQLTableField(DataSourceName, TableName, "active", fieldTypeIdBoolean);
                    createSQLTableField(DataSourceName, TableName, "createKey", fieldTypeIdInteger);
                    createSQLTableField(DataSourceName, TableName, "sortOrder", fieldTypeIdText);
                    createSQLTableField(DataSourceName, TableName, "contentControlID", fieldTypeIdInteger);
                    createSQLTableField(DataSourceName, TableName, "ccGuid", fieldTypeIdText);
                    // -- 20171029 - deprecating fields makes migration difficult. add back and figure out future path
                    createSQLTableField(DataSourceName, TableName, "ContentCategoryID", fieldTypeIdInteger);
                    //
                    // ----- setup core indexes
                    //
                    // 20171029 primary key dow not need index -- Call createSQLIndex(DataSourceName, TableName, TableName & "Id", "ID")
                    createSQLIndex(DataSourceName, TableName, TableName + "Active", "ACTIVE");
                    createSQLIndex(DataSourceName, TableName, TableName + "Name", "NAME");
                    createSQLIndex(DataSourceName, TableName, TableName + "SortOrder", "SORTORDER");
                    createSQLIndex(DataSourceName, TableName, TableName + "DateAdded", "DATEADDED");
                    createSQLIndex(DataSourceName, TableName, TableName + "CreateKey", "CREATEKEY");
                    createSQLIndex(DataSourceName, TableName, TableName + "ContentControlID", "CONTENTCONTROLID");
                    createSQLIndex(DataSourceName, TableName, TableName + "ModifiedDate", "MODIFIEDDATE");
                    createSQLIndex(DataSourceName, TableName, TableName + "ccGuid", "CCGUID");
                }
                Models.Domain.TableSchemaModel.tableSchemaListClear(core);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Check for a field in a table in the database, if missing, create the field
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="TableName"></param>
        /// <param name="FieldName"></param>
        /// <param name="fieldType"></param>
        /// <param name="clearMetaCache"></param>
        public void createSQLTableField(string DataSourceName, string TableName, string FieldName, int fieldType, bool clearMetaCache = false) {
            try {
                if ((fieldType == fieldTypeIdRedirect) || (fieldType == fieldTypeIdManyToMany)) {
                    //
                    // OK -- contensive fields with no table field
                    //
                    //fieldType = fieldType;
                } else if (string.IsNullOrEmpty(TableName)) {
                    //
                    // Bad tablename
                    //
                    throw new ArgumentException("Table Name cannot be blank.");
                } else if (fieldType == 0) {
                    //
                    // Bad fieldtype
                    //
                    throw new ArgumentException("invalid fieldtype [" + fieldType + "]");
                } else if (GenericController.vbInstr(1, TableName, ".") != 0) {
                    //
                    // External table
                    //
                    throw new ArgumentException("Table name cannot include a period(.)");
                } else if (string.IsNullOrEmpty(FieldName)) {
                    //
                    // Bad fieldname
                    //
                    throw new ArgumentException("Field name cannot be blank");
                } else {
                    if (!isSQLTableField(DataSourceName, TableName, FieldName)) {
                        string SQL = "ALTER TABLE " + TableName + " ADD " + FieldName + " ";
                        if (!fieldType.IsNumeric()) {
                            //
                            // ----- support old calls
                            //
                            SQL += fieldType.ToString();
                        } else {
                            //
                            // ----- translater type into SQL string
                            //
                            SQL += getSQLAlterColumnType(DataSourceName, fieldType);
                        }
                        executeNonQuery(SQL, DataSourceName);
                        TableSchemaModel.tableSchemaListClear(core);
                        //
                        if (clearMetaCache) {
                            core.cache.invalidateAll();
                            core.clearMetaData();
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public void deleteTable(string DataSourceName, string TableName) {
            try {
                executeQuery("DROP TABLE " + TableName, DataSourceName).Dispose();
                core.cache.invalidateAll();
                core.clearMetaData();
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public void deleteTableField(string DataSourceName, string TableName, string FieldName) {
            try {
                if (isSQLTableField(DataSourceName, TableName, FieldName)) {
                    executeQuery("ALTER TABLE " + TableName + " DROP COLUMN " + FieldName + ";", DataSourceName);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public void createSQLIndex(string DataSourceName, string TableName, string IndexName, string FieldNames, bool clearMetaCache = false) {
            try {
                Models.Domain.TableSchemaModel ts = null;
                if (!(string.IsNullOrEmpty(TableName) && string.IsNullOrEmpty(IndexName) & string.IsNullOrEmpty(FieldNames))) {
                    ts = Models.Domain.TableSchemaModel.getTableSchema(core, TableName, DataSourceName);
                    if (ts != null) {
                        if (null == ts.indexes.Find(x => x.index_name.ToLowerInvariant() == IndexName.ToLowerInvariant())) {
                            executeQuery("CREATE INDEX [" + IndexName + "] ON [" + TableName + "]( " + FieldNames + " );", DataSourceName);
                            if (clearMetaCache) {
                                core.cache.invalidateAll();
                                core.clearMetaData();
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //=============================================================
        /// <summary>
        /// Return a record name given the record id. If not record is found, blank is returned.
        /// </summary>
        public string getRecordName(string ContentName, int RecordID) {
            string returnRecordName = "";
            try {
                int CS = csOpenContentRecord(ContentName, RecordID, 0, false, false, "Name");
                if (csOk(CS)) {
                    returnRecordName = csGet(CS, "Name");
                }
                csClose(ref CS);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnRecordName;
        }
        //
        //=============================================================
        /// <summary>
        /// Return a record name given the guid. If not record is found, blank is returned.
        /// </summary>
        public string getRecordName(string contentName, string recordGuid) {
            string returnRecordName = "";
            try {
                CsController cs = new CsController(core);
                if (cs.open(contentName, "(ccguid=" + encodeSQLText(recordGuid) + ")")) {
                    returnRecordName = cs.getText("Name");
                }
                cs.close();
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnRecordName;
        }
        //
        //=============================================================
        /// <summary>
        /// get the lowest recordId based on its name. If no record is found, 0 is returned
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordName"></param>
        /// <returns></returns>
        public int getRecordID(string ContentName, string RecordName) {
            int returnValue = 0;
            try {
                if ((!string.IsNullOrEmpty(ContentName.Trim())) && (!string.IsNullOrEmpty(RecordName.Trim()))) {
                    int cs = csOpen(ContentName, "name=" + encodeSQLText(RecordName), "ID", true, 0, false, false, "ID");
                    if (csOk(cs)) {
                        returnValue = csGetInteger(cs, "ID");
                    }
                    csClose(ref cs);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnValue;
        }
        //
        //========================================================================
        /// <summary>
        /// Get FieldDescritor from FieldType
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public string getSQLAlterColumnType(string DataSourceName, int fieldType) {
            string returnType = "";
            try {
                switch (fieldType) {
                    case Constants._fieldTypeIdBoolean:
                        returnType = "Int NULL";
                        break;
                    case Constants._fieldTypeIdCurrency:
                        returnType = "Float NULL";
                        break;
                    case Constants._fieldTypeIdDate:
                        // 20180416 - ms recommends using new, higher precision. Code requires 3 digits so 7 is more than enough
                        returnType = "DateTime2(7) NULL";
                        break;
                    case Constants._fieldTypeIdFloat:
                        returnType = "Float NULL";
                        break;
                    case Constants._fieldTypeIdInteger:
                        returnType = "Int NULL";
                        break;
                    case Constants._fieldTypeIdLookup:
                    case Constants._fieldTypeIdMemberSelect:
                        returnType = "Int NULL";
                        break;
                    case Constants._fieldTypeIdManyToMany:
                    case Constants._fieldTypeIdRedirect:
                    case Constants._fieldTypeIdFileImage:
                    case Constants._fieldTypeIdLink:
                    case Constants._fieldTypeIdResourceLink:
                    case Constants._fieldTypeIdText:
                    case Constants._fieldTypeIdFile:
                    case Constants._fieldTypeIdFileText:
                    case Constants._fieldTypeIdFileJavascript:
                    case Constants._fieldTypeIdFileXML:
                    case Constants._fieldTypeIdFileCSS:
                    case Constants._fieldTypeIdFileHTML:
                        returnType = "VarChar(255) NULL";
                        break;
                    case Constants._fieldTypeIdLongText:
                    case Constants._fieldTypeIdHTML:
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
                    case Constants._fieldTypeIdAutoIdIncrement:
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
                        throw new GenericException("Can Not proceed because the field being created has an invalid FieldType [" + fieldType + "]");
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public void deleteSqlIndex(string DataSourceName, string TableName, string IndexName) {
            try {
                Models.Domain.TableSchemaModel ts = null;
                int DataSourceType = 0;
                string sql = null;
                //
                ts = Models.Domain.TableSchemaModel.getTableSchema(core, TableName, DataSourceName);
                if (ts != null) {
                    if (null != ts.indexes.Find(x => x.index_name.ToLowerInvariant() == IndexName.ToLowerInvariant())) {
                        DataSourceType = getDataSourceType(DataSourceName);
                        switch (DataSourceType) {
                            case Constants.DataSourceTypeODBCAccess:
                                sql = "DROP INDEX " + IndexName + " On " + TableName + ";";
                                break;
                            case Constants.DataSourceTypeODBCMySQL:
                                throw new NotImplementedException("mysql");
                            default:
                                sql = "DROP INDEX [" + TableName + "].[" + IndexName + "];";
                                break;
                        }
                        executeQuery(sql, DataSourceName);
                        core.cache.invalidateAll();
                        core.clearMetaData();
                    }
                }

            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// returns true if the cdef field exists
        /// </summary>
        /// <param name="ContentID"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public bool isCdefField(int ContentID, string FieldName) {
            bool tempisCdefField = false;
            bool returnOk = false;
            try {
                DataTable dt = executeQuery("Select top 1 id from ccFields where name=" + encodeSQLText(FieldName) + " And contentid=" + ContentID);
                tempisCdefField = DbController.isDataTableOk(dt);
                dt.Dispose();
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnOk;
        }
        //
        //========================================================================
        /// <summary>
        /// Get a DataSource type (SQL Server, etc) from its Name
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <returns></returns>
        //
        public int getDataSourceType(string DataSourceName) {
            return Constants.DataSourceTypeODBCSQLServer;
        }
        //
        //========================================================================
        /// <summary>
        /// Get FieldType from ADO Field Type
        /// </summary>
        /// <param name="ADOFieldType"></param>
        /// <returns></returns>
        public int getFieldTypeIdByADOType(int ADOFieldType) {
            int returnType = 0;
            try {
                switch (ADOFieldType) {

                    case 2:
                        returnType = Constants.fieldTypeIdFloat;
                        break;
                    case 3:
                        returnType = Constants.fieldTypeIdInteger;
                        break;
                    case 4:
                        returnType = Constants.fieldTypeIdFloat;
                        break;
                    case 5:
                        returnType = Constants.fieldTypeIdFloat;
                        break;
                    case 6:
                        returnType = Constants.fieldTypeIdInteger;
                        break;
                    case 11:
                        returnType = Constants.fieldTypeIdBoolean;
                        break;
                    case 135:
                        returnType = Constants.fieldTypeIdDate;
                        break;
                    case 200:
                        returnType = Constants.fieldTypeIdText;
                        break;
                    case 201:
                        returnType = Constants.fieldTypeIdLongText;
                        break;
                    case 202:
                        returnType = Constants.fieldTypeIdText;
                        break;
                    default:
                        returnType = Constants.fieldTypeIdText;
                        break;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public int getFieldTypeIdFromFieldTypeName(string FieldTypeName) {
            int returnTypeId = 0;
            try {
                switch (GenericController.vbLCase(FieldTypeName)) {
                    case Constants.FieldTypeNameLcaseBoolean:
                        returnTypeId = Constants.fieldTypeIdBoolean;
                        break;
                    case Constants.FieldTypeNameLcaseCurrency:
                        returnTypeId = Constants.fieldTypeIdCurrency;
                        break;
                    case Constants.FieldTypeNameLcaseDate:
                        returnTypeId = Constants.fieldTypeIdDate;
                        break;
                    case Constants.FieldTypeNameLcaseFile:
                        returnTypeId = Constants.fieldTypeIdFile;
                        break;
                    case Constants.FieldTypeNameLcaseFloat:
                        returnTypeId = Constants.fieldTypeIdFloat;
                        break;
                    case Constants.FieldTypeNameLcaseImage:
                        returnTypeId = Constants.fieldTypeIdFileImage;
                        break;
                    case Constants.FieldTypeNameLcaseLink:
                        returnTypeId = Constants.fieldTypeIdLink;
                        break;
                    case Constants.FieldTypeNameLcaseResourceLink:
                    case "resource link":
                        returnTypeId = Constants.fieldTypeIdResourceLink;
                        break;
                    case Constants.FieldTypeNameLcaseInteger:
                        returnTypeId = Constants.fieldTypeIdInteger;
                        break;
                    case Constants.FieldTypeNameLcaseLongText:
                    case "Long text":
                        returnTypeId = Constants.fieldTypeIdLongText;
                        break;
                    case Constants.FieldTypeNameLcaseLookup:
                    case "lookuplist":
                    case "lookup list":
                        returnTypeId = Constants.fieldTypeIdLookup;
                        break;
                    case Constants.FieldTypeNameLcaseMemberSelect:
                        returnTypeId = Constants.fieldTypeIdMemberSelect;
                        break;
                    case Constants.FieldTypeNameLcaseRedirect:
                        returnTypeId = Constants.fieldTypeIdRedirect;
                        break;
                    case Constants.FieldTypeNameLcaseManyToMany:
                        returnTypeId = Constants.fieldTypeIdManyToMany;
                        break;
                    case Constants.FieldTypeNameLcaseTextFile:
                    case "text file":
                        returnTypeId = Constants.fieldTypeIdFileText;
                        break;
                    case Constants.FieldTypeNameLcaseCSSFile:
                    case "css file":
                        returnTypeId = Constants.fieldTypeIdFileCSS;
                        break;
                    case Constants.FieldTypeNameLcaseXMLFile:
                    case "xml file":
                        returnTypeId = Constants.fieldTypeIdFileXML;
                        break;
                    case Constants.FieldTypeNameLcaseJavascriptFile:
                    case "javascript file":
                    case "js file":
                    case "jsfile":
                        returnTypeId = Constants.fieldTypeIdFileJavascript;
                        break;
                    case Constants.FieldTypeNameLcaseText:
                        returnTypeId = Constants.fieldTypeIdText;
                        break;
                    case "autoincrement":
                        returnTypeId = Constants.fieldTypeIdAutoIdIncrement;
                        break;
                    case Constants.FieldTypeNameLcaseHTML:
                        returnTypeId = Constants.fieldTypeIdHTML;
                        break;
                    case Constants.FieldTypeNameLcaseHTMLFile:
                    case "html file":
                        returnTypeId = Constants.fieldTypeIdFileHTML;
                        break;
                    default:
                        //
                        // Bad field type is a text field
                        //
                        returnTypeId = Constants.fieldTypeIdText;
                        break;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnTypeId;
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
                    var CDef = CDefModel.create(core, ContentName);
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
                    var CDef = CDefModel.create(core, ContentName);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
                throw;
            }
            return returnCs;
        }
        //
        //========================================================================
        /// <summary>
        /// Close a csv_ContentSet
        /// Closes a currently open csv_ContentSet
        /// sets CSPointer to -1
        /// </summary>
        /// <param name="CSPointer"></param>
        /// <param name="asyncSave"></param>
        public void csClose(ref int CSPointer, bool asyncSave = false) {
            try {
                if ((CSPointer > 0) && (CSPointer <= contentSetStoreCount)) {
                    ContentSetClass tmp = contentSetStore[CSPointer];
                    if (tmp.IsOpen) {
                        csSave(CSPointer, asyncSave);
                        tmp.readCache = new string[,] { { }, { } };
                        tmp.writeCache = new Dictionary<string, string>();
                        tmp.resultColumnCount = 0;
                        tmp.readCacheRowCnt = 0;
                        tmp.readCacheRowPtr = -1;
                        tmp.resultEOF = true;
                        tmp.IsOpen = false;
                        if (tmp.dt != null) {
                            tmp.dt.Dispose();
                        }
                    }
                    CSPointer = -1;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //========================================================================
        //
        public void csGoNext(int CSPointer, bool AsyncSave = false) {
            try {
                //
                string ContentName = null;
                int RecordID = 0;
                //
                if (!csOk(CSPointer)) {
                    //
                    throw new GenericException("Dataset is not valid.");
                } else if(!contentSetStore[CSPointer].readable) {
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// ContentServer version of getDataRowColumnName
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public string getDataRowColumnName(DataRow dr, string FieldName) {
            string result = "";
            try {
                if (string.IsNullOrEmpty(FieldName)) {
                    throw new ArgumentException("fieldname cannot be blank");
                } else {
                    result = dr[FieldName].ToString();
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
        /// InsertContentRecordGetID
        /// Inserts a record based on a content definition.
        /// Returns the ID of the record, -1 if error
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        ///
        public int insertContentRecordGetID(string ContentName, int MemberID) {
            int result = -1;
            try {
                int CS = csInsertRecord(ContentName, MemberID);
                if (!csOk(CS)) {
                    csClose(ref CS);
                    throw new GenericException("could not insert record in content [" + ContentName + "]");
                } else {
                    result = csGetInteger(CS, "ID");
                }
                csClose(ref CS);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// Delete Content Record
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <param name="MemberID"></param>
        //
        public void deleteContentRecord(string ContentName, int RecordID, int MemberID = SystemMemberID) {
            try {
                if (string.IsNullOrEmpty(ContentName)) {
                    throw new ArgumentException("contentname cannot be blank");
                } else if (RecordID <= 0) {
                    throw new ArgumentException("recordId must be positive value");
                } else {
                    int CSPointer = csOpenContentRecord(ContentName, RecordID, MemberID, true, true);
                    if (csOk(CSPointer)) {
                        csDeleteRecord(CSPointer);
                    }
                    csClose(ref CSPointer);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// 'deleteContentRecords
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="Criteria"></param>
        /// <param name="MemberID"></param>
        //
        public void deleteContentRecords(string ContentName, string Criteria, int MemberID = 0) {
            try {
                //
                int CSPointer = 0;
                Models.Domain.CDefModel CDef = null;
                //
                if (string.IsNullOrEmpty(ContentName.Trim())) {
                    throw new ArgumentException("contentName cannot be blank");
                } else if (string.IsNullOrEmpty(Criteria.Trim())) {
                    throw new ArgumentException("criteria cannot be blank");
                } else {
                    CDef = CDefModel.create(core, ContentName);
                    if (CDef == null) {
                        throw new ArgumentException("ContentName [" + ContentName + "] was not found");
                    } else if (CDef.id == 0) {
                        throw new ArgumentException("ContentName [" + ContentName + "] was not found");
                    } else {
                        //
                        // -- treat all deletes one at a time to invalidate the primary cache
                        // another option is invalidate the entire table (tablename-invalidate), but this also has performance problems
                        //
                        List<string> invaldiateObjectList = new List<string>();
                        CSPointer = csOpen(ContentName, Criteria, "", false, MemberID, true, true);
                        while (csOk(CSPointer)) {
                            invaldiateObjectList.Add(CacheController.createCacheKey_forDbRecord(csGetInteger(CSPointer, "id"), CDef.tableName, CDef.dataSourceName));
                            csDeleteRecord(CSPointer);
                            csGoNext(CSPointer);
                        }
                        csClose(ref CSPointer);
                        core.cache.invalidate(invaldiateObjectList);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                Models.Domain.CDefModel CDef = null;
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
                    CDef = Models.Domain.CDefModel.create(core, ContentName);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                Models.Domain.CDefModel DestCDef = null;
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
                LogController.handleError( core,ex);
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
        public string csGetSource(int CSPointer) {
            string returnResult = "";
            try {
                if (!csOk(CSPointer)) {
                    throw new ArgumentException("the dataset is not valid");
                } else {
                    returnResult = contentSetStore[CSPointer].Source;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                                                LogController.handleError( core,new GenericException("Text length too long saving field [" + FieldName + "], length [" + FieldValue.Length + "], but max for Text field is 255. Save will be attempted"));
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
                                                LogController.handleError( core,new GenericException("Text length too long saving field [" + FieldName + "], length [" + FieldValue.Length + "], but max for LongText and Html is 65535. Save will be attempted"));
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                if ( csStore.dt != null ) {
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        /// <summary>
        /// Encode a value for a sql
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static string encodeSQL(object expression, int fieldType = _fieldTypeIdText) {
            string returnResult = "";
            try {
                switch (fieldType) {
                    case _fieldTypeIdBoolean:
                        returnResult = encodeSQLBoolean(GenericController.encodeBoolean(expression));
                        break;
                    case _fieldTypeIdCurrency:
                    case _fieldTypeIdFloat:
                        returnResult = encodeSQLNumber(GenericController.encodeNumber(expression));
                        break;
                    case _fieldTypeIdAutoIdIncrement:
                    case _fieldTypeIdInteger:
                    case _fieldTypeIdLookup:
                    case _fieldTypeIdMemberSelect:
                        returnResult = encodeSQLNumber(GenericController.encodeInteger(expression));
                        break;
                    case _fieldTypeIdDate:
                        returnResult = encodeSQLDate(GenericController.encodeDate(expression));
                        break;
                    case _fieldTypeIdLongText:
                    case _fieldTypeIdHTML:
                        returnResult = encodeSQLText(GenericController.encodeText(expression));
                        break;
                    case _fieldTypeIdFile:
                    case _fieldTypeIdFileImage:
                    case _fieldTypeIdLink:
                    case _fieldTypeIdResourceLink:
                    case _fieldTypeIdRedirect:
                    case _fieldTypeIdManyToMany:
                    case _fieldTypeIdText:
                    case _fieldTypeIdFileText:
                    case _fieldTypeIdFileJavascript:
                    case _fieldTypeIdFileXML:
                    case _fieldTypeIdFileCSS:
                    case _fieldTypeIdFileHTML:
                        returnResult = encodeSQLText(GenericController.encodeText(expression));
                        break;
                    default:
                        throw new GenericException("Unknown Field Type [" + fieldType + "");
                }
            } catch (Exception) {
                throw;
            }
            return returnResult;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// return a sql compatible string. 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string encodeSQLText(string expression) {
            string returnResult = "";
            if (expression == null) {
                returnResult = "null";
            } else {
                returnResult = GenericController.encodeText(expression);
                if (string.IsNullOrEmpty(returnResult)) {
                    returnResult = "null";
                } else {
                    returnResult = "'" + GenericController.vbReplace(returnResult, "'", "''") + "'";
                }
            }
            return returnResult;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// encode a string for a like operation
        /// </summary>
        /// <param name="core"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string encodeSqlTextLike( string source) {
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
            string returnResult = "";
            try {
                if (Convert.IsDBNull(expressionDate)) {
                    returnResult = "null";
                } else {
                    // 20180416 - not needed, type is already datetime
                    //DateTime expressionDate = genericController.encodeDate(expression);
                    if (expressionDate == DateTime.MinValue) {
                        returnResult = "null";
                    } else {
                        returnResult = "'" + expressionDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
                        //returnResult = "'" + expressionDate.Year + ("0" + expressionDate.Month).Substring(("0" + expressionDate.Month).Length - 2) + ("0" + expressionDate.Day).Substring(("0" + expressionDate.Day).Length - 2) + " " + ("0" + expressionDate.Hour).Substring(("0" + expressionDate.Hour).Length - 2) + ":" + ("0" + expressionDate.Minute).Substring(("0" + expressionDate.Minute).Length - 2) + ":" + ("0" + expressionDate.Second).Substring(("0" + expressionDate.Second).Length - 2) + ":" + ("00" + expressionDate.Millisecond).Substring(("00" + expressionDate.Millisecond).Length - 3) + "'";
                    }
                }
            } catch (Exception ) {
                throw;
            }
            return returnResult;
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
            string returnResult = SQLFalse;
            try {
                if (expression) {
                    returnResult = SQLTrue;
                }
            } catch (Exception) {
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        /// <summary>
        /// Create a filename for the Virtual Directory
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="FieldName"></param>
        /// <param name="RecordID"></param>
        /// <param name="OriginalFilename"></param>
        /// <returns></returns>
        //========================================================================
        //
        public string getVirtualFilename(string ContentName, string FieldName, int RecordID, string OriginalFilename = "") {
            string returnResult = "";
            try {
                int fieldTypeId = 0;
                string TableName = null;
                //Dim iOriginalFilename As String
                Models.Domain.CDefModel CDef = null;
                //
                if (string.IsNullOrEmpty(ContentName.Trim())) {
                    throw new ArgumentException("contentname cannot be blank");
                } else if (string.IsNullOrEmpty(FieldName.Trim())) {
                    throw new ArgumentException("fieldname cannot be blank");
                } else if (RecordID <= 0) {
                    throw new ArgumentException("recordid is not valid");
                } else {
                    CDef = Models.Domain.CDefModel.create(core, ContentName);
                    if (CDef.id == 0) {
                        throw new GenericException("contentname [" + ContentName + "] is not a valid content");
                    } else {
                        TableName = CDef.tableName;
                        if (string.IsNullOrEmpty(TableName)) {
                            TableName = ContentName;
                        }
                        //
                        //iOriginalFilename = genericController.encodeEmptyText(OriginalFilename, "")
                        //
                        fieldTypeId = CDef.fields[FieldName.ToLowerInvariant()].fieldTypeId;
                        //
                        if (string.IsNullOrEmpty(OriginalFilename)) {
                            returnResult = FileController.getVirtualRecordUnixPathFilename(TableName, FieldName, RecordID, fieldTypeId);
                        } else {
                            returnResult = FileController.getVirtualRecordUnixPathFilename(TableName, FieldName, RecordID, OriginalFilename);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                    returnResult = csOpenSql(SQL,"Default", PageSize, PageNumber);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnResult;
        }
        //========================================================================
        /// <summary>
        /// Get a Contents Tableid from the ContentPointer
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        //========================================================================
        //
        public int getContentTableID(string ContentName) {
            int returnResult = 0;
            try {
                DataTable dt = executeQuery("select ContentTableID from ccContent where name=" + encodeSQLText(ContentName));
                if (!DbController.isDataTableOk(dt)) {
                    throw new GenericException("Content [" + ContentName + "] was not found in ccContent table");
                } else {
                    returnResult = GenericController.encodeInteger(dt.Rows[0]["ContentTableID"]);
                }
                dt.Dispose();
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnResult;
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
        public void deleteTableRecord(int recordId, string tableName, string dataSourceName = "") {
            try {
                if (string.IsNullOrEmpty(tableName.Trim())) {
                    throw new GenericException("tablename cannot be blank");
                } else if (recordId <= 0) {
                    throw new GenericException("record id is not valid [" + recordId + "]");
                } else {
                    executeNonQuery("delete from " + tableName + " where id=" + recordId, dataSourceName);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        /// <summary>
        /// delete a record based on a guid
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="tableName"></param>
        /// <param name="dataSourceName"></param>
        public void deleteTableRecord(string guid, string tableName, string dataSourceName = "") {
            try {
                if (string.IsNullOrEmpty(tableName.Trim())) {
                    throw new GenericException("tablename cannot be blank");
                } else if (!isGuid( guid )) {
                    throw new GenericException("Guid is not valid [" + guid + "]");
                } else {
                    executeNonQuery("delete from " + tableName + " where ccguid=" + encodeSQLText(guid) , dataSourceName);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
            //
            //==================================================================================================
            /// <summary>
            /// Remove this record from all watch lists
            /// </summary>
            /// <param name="ContentID"></param>
            /// <param name="RecordID"></param>
            //
            public void deleteContentRules(int ContentID, int RecordID) {
            try {
                string ContentRecordKey = null;
                string Criteria = null;
                string ContentName = null;
                string TableName = null;
                //
                // ----- remove all ContentWatchListRules (uncheck the watch lists in admin)
                //
                if ((ContentID <= 0) || (RecordID <= 0)) {
                    //
                    throw new GenericException("ContentID [" + ContentID + "] or RecordID [" + RecordID + "] where blank");
                } else {
                    ContentRecordKey = ContentID.ToString() + "." + RecordID.ToString();
                    Criteria = "(ContentRecordKey=" + encodeSQLText(ContentRecordKey) + ")";
                    ContentName = CdefController.getContentNameByID(core, ContentID);
                    TableName = CdefController.getContentTablename(core, ContentName);
                    //
                    // ----- Table Specific rules
                    //
                    switch (GenericController.vbUCase(TableName)) {
                        case "CCCALENDARS":
                            //
                            deleteContentRecords("Calendar Event Rules", "CalendarID=" + RecordID);
                            break;
                        case "CCCALENDAREVENTS":
                            //
                            deleteContentRecords("Calendar Event Rules", "CalendarEventID=" + RecordID);
                            break;
                        case "CCCONTENT":
                            //
                            deleteContentRecords("Group Rules", "ContentID=" + RecordID);
                            break;
                        case "CCCONTENTWATCH":
                            //
                            deleteContentRecords("Content Watch List Rules", "Contentwatchid=" + RecordID);
                            break;
                        case "CCCONTENTWATCHLISTS":
                            //
                            deleteContentRecords("Content Watch List Rules", "Contentwatchlistid=" + RecordID);
                            break;
                        case "CCGROUPS":
                            //
                            deleteContentRecords("Group Rules", "GroupID=" + RecordID);
                            deleteContentRecords("Library Folder Rules", "GroupID=" + RecordID);
                            deleteContentRecords("Member Rules", "GroupID=" + RecordID);
                            deleteContentRecords("Page Content Block Rules", "GroupID=" + RecordID);
                            break;
                        case "CCLIBRARYFOLDERS":
                            //
                            deleteContentRecords("Library Folder Rules", "FolderID=" + RecordID);
                            break;
                        case "CCMEMBERS":
                            //
                            deleteContentRecords("Member Rules", "MemberID=" + RecordID);
                            deleteContentRecords("Topic Habits", "MemberID=" + RecordID);
                            deleteContentRecords("Member Topic Rules", "MemberID=" + RecordID);
                            break;
                        case "CCPAGECONTENT":
                            //
                            deleteContentRecords("Page Content Block Rules", "RecordID=" + RecordID);
                            deleteContentRecords("Page Content Topic Rules", "PageID=" + RecordID);
                            break;
                        case "CCSURVEYQUESTIONS":
                            //
                            deleteContentRecords("Survey Results", "QuestionID=" + RecordID);
                            break;
                        case "CCSURVEYS":
                            //
                            deleteContentRecords("Survey Questions", "SurveyID=" + RecordID);
                            break;
                        case "CCTOPICS":
                            //
                            deleteContentRecords("Topic Habits", "TopicID=" + RecordID);
                            deleteContentRecords("Page Content Topic Rules", "TopicID=" + RecordID);
                            deleteContentRecords("Member Topic Rules", "TopicID=" + RecordID);
                            break;
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
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
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        /// <summary>
        /// DeleteTableRecords
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="TableName"></param>
        /// <param name="Criteria"></param>
        //
        public void deleteTableRecords(string TableName, string Criteria, string DataSourceName) {
            try {
                if (string.IsNullOrEmpty(DataSourceName)) {
                    throw new ArgumentException("dataSourceName cannot be blank");
                } else if (string.IsNullOrEmpty(TableName)) {
                    throw new ArgumentException("TableName cannot be blank");
                } else if (string.IsNullOrEmpty(Criteria)) {
                    throw new ArgumentException("Criteria cannot be blank");
                } else {
                    string SQL = "DELETE FROM " + TableName + " WHERE " + Criteria;
                    executeQuery(SQL, DataSourceName);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
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
                LogController.handleError( core,ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        /// <summary>
        /// Get FieldDescritor from FieldType
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        //
        public string getFieldTypeNameFromFieldTypeId(int fieldType) {
            string returnFieldTypeName = "";
            try {
                switch (fieldType) {
                    case _fieldTypeIdBoolean:
                        returnFieldTypeName = FieldTypeNameBoolean;
                        break;
                    case _fieldTypeIdCurrency:
                        returnFieldTypeName = FieldTypeNameCurrency;
                        break;
                    case _fieldTypeIdDate:
                        returnFieldTypeName = FieldTypeNameDate;
                        break;
                    case _fieldTypeIdFile:
                        returnFieldTypeName = FieldTypeNameFile;
                        break;
                    case _fieldTypeIdFloat:
                        returnFieldTypeName = FieldTypeNameFloat;
                        break;
                    case _fieldTypeIdFileImage:
                        returnFieldTypeName = FieldTypeNameImage;
                        break;
                    case _fieldTypeIdLink:
                        returnFieldTypeName = FieldTypeNameLink;
                        break;
                    case _fieldTypeIdResourceLink:
                        returnFieldTypeName = FieldTypeNameResourceLink;
                        break;
                    case _fieldTypeIdInteger:
                        returnFieldTypeName = FieldTypeNameInteger;
                        break;
                    case _fieldTypeIdLongText:
                        returnFieldTypeName = FieldTypeNameLongText;
                        break;
                    case _fieldTypeIdLookup:
                        returnFieldTypeName = FieldTypeNameLookup;
                        break;
                    case _fieldTypeIdMemberSelect:
                        returnFieldTypeName = FieldTypeNameMemberSelect;
                        break;
                    case _fieldTypeIdRedirect:
                        returnFieldTypeName = FieldTypeNameRedirect;
                        break;
                    case _fieldTypeIdManyToMany:
                        returnFieldTypeName = FieldTypeNameManyToMany;
                        break;
                    case _fieldTypeIdFileText:
                        returnFieldTypeName = FieldTypeNameTextFile;
                        break;
                    case _fieldTypeIdFileCSS:
                        returnFieldTypeName = FieldTypeNameCSSFile;
                        break;
                    case _fieldTypeIdFileXML:
                        returnFieldTypeName = FieldTypeNameXMLFile;
                        break;
                    case _fieldTypeIdFileJavascript:
                        returnFieldTypeName = FieldTypeNameJavascriptFile;
                        break;
                    case _fieldTypeIdText:
                        returnFieldTypeName = FieldTypeNameText;
                        break;
                    case _fieldTypeIdHTML:
                        returnFieldTypeName = FieldTypeNameHTML;
                        break;
                    case _fieldTypeIdFileHTML:
                        returnFieldTypeName = FieldTypeNameHTMLFile;
                        break;
                    default:
                        if (fieldType == fieldTypeIdAutoIdIncrement) {
                            returnFieldTypeName = "AutoIncrement";
                        } else if (fieldType == fieldTypeIdMemberSelect) {
                            returnFieldTypeName = "MemberSelect";
                        } else {
                            //
                            // If field type is ignored, call it a text field
                            //
                            returnFieldTypeName = FieldTypeNameText;
                        }
                        break;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex); // "Unexpected exception")
                throw;
            }
            return returnFieldTypeName;
        }
        //
        //=============================================================
        /// <summary>
        /// get a record's id from its guid
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordGuid"></param>
        /// <returns></returns>
        //=============================================================
        //
        public int getRecordIDByGuid(string ContentName, string RecordGuid) {
            int returnResult = 0;
            try {
                if (string.IsNullOrEmpty(ContentName)) {
                    throw new ArgumentException("contentname cannot be blank");
                } else if (string.IsNullOrEmpty(RecordGuid)) {
                    throw new ArgumentException("RecordGuid cannot be blank");
                } else {
                    int CS = csOpen(ContentName, "ccguid=" + encodeSQLText(RecordGuid), "ID", true, 0, false, false, "ID");
                    if (csOk(CS)) {
                        returnResult = csGetInteger(CS, "ID");
                    }
                    csClose(ref CS);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        //
        public string[,] getContentRows(string ContentName, string Criteria = "", string SortFieldList = "", bool ActiveOnly = true, int MemberID = SystemMemberID, bool WorkflowRenderingMode = false, bool WorkflowEditingMode = false, string SelectFieldList = "", int PageSize = 9999, int PageNumber = 1) {
            string[,] returnRows = { { } };
            try {
                //
                int CS = csOpen(ContentName, Criteria, SortFieldList, ActiveOnly, MemberID, WorkflowRenderingMode, WorkflowEditingMode, SelectFieldList, PageSize, PageNumber);
                if (csOk(CS)) {
                    returnRows = contentSetStore[CS].readCache;
                }
                csClose(ref CS);
                //
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnRows;
        }
        //
        //========================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="From"></param>
        /// <param name="FieldList"></param>
        /// <param name="Where"></param>
        /// <param name="OrderBy"></param>
        /// <param name="GroupBy"></param>
        /// <param name="RecordLimit"></param>
        /// <returns></returns>
        public string getSQLSelect(string DataSourceName, string From, string FieldList = "", string Where = "", string OrderBy = "", string GroupBy = "", int RecordLimit = 0) {
            string SQL = "SELECT";
            if (RecordLimit != 0) {
                SQL += " TOP " + RecordLimit;
            }
            if (string.IsNullOrEmpty(FieldList)) {
                SQL += " *";
            } else {
                SQL += " " + FieldList;
            }
            SQL += " FROM " + From;
            if (!string.IsNullOrEmpty(Where)) { SQL += " WHERE " + Where; }
            if (!string.IsNullOrEmpty(OrderBy)) { SQL += " ORDER BY " + OrderBy; }
            if (!string.IsNullOrEmpty(GroupBy)) { SQL += " GROUP BY " + GroupBy; }
            return SQL;
        }
        //
        //========================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        //
        public string getSQLIndexList(string DataSourceName, string TableName) {
            string returnList = "";
            try {
                Models.Domain.TableSchemaModel ts = Models.Domain.TableSchemaModel.getTableSchema(core, TableName, DataSourceName);
                if (ts != null) {
                    foreach ( TableSchemaModel.IndexSchemaModel index in ts.indexes) {
                        returnList += "," + index.index_name;
                    }
                    if (returnList.Length > 0) {
                        returnList = returnList.Substring(2);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                string connString = getConnectionStringADONET(core.appConfig.name, "default");
                using (SqlConnection connSQL = new SqlConnection(connString)) {
                    connSQL.Open();
                    returnDt = connSQL.GetSchema("Tables", new[] { core.appConfig.name, null, tableName, null });
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                    string connString = getConnectionStringADONET(core.appConfig.name, "default");
                    using (SqlConnection connSQL = new SqlConnection(connString)) {
                        connSQL.Open();
                        returnDt = connSQL.GetSchema("Columns", new[] { core.appConfig.name, null, tableName, null });
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                    string connString = getConnectionStringADONET(core.appConfig.name, "default");
                    using (SqlConnection connSQL = new SqlConnection(connString)) {
                        connSQL.Open();
                        returnDt = connSQL.GetSchema("Indexes", new[] { core.appConfig.name, null, tableName, null });
                    }
                }
                //
                returnDt = executeQuery("sys.sp_helpindex @objname = N'" + tableName + "'");
                // EXEC sys.sp_helpindex @objname = N'cccontent' returns index_name, index_keys (comma delimited field list)
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
                throw;
            }
            return sqlCriteria;
        }
        //
        //=============================================================================
        /// <summary>
        /// Get a ContentID from the ContentName using just the tables
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        private int getDbContentID(string ContentName) {
            int returnContentId = 0;
            try {
                DataTable dt = executeQuery("Select ID from ccContent where name=" + encodeSQLText(ContentName));
                if (dt.Rows.Count > 0) {
                    returnContentId = GenericController.encodeInteger(dt.Rows[0]["id"]);
                }
                dt.Dispose();
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnContentId;
        }
        //
        //=============================================================================
        /// <summary>
        /// Imports the named table into the content system
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <param name="TableName"></param>
        /// <param name="ContentName"></param>
        //
        public void createContentFromSQLTable(DataSourceModel DataSource, string TableName, string ContentName) {
            try {
                //string DateAddedString = DbController.encodeSQLDate(DateTime.Now);
                //string sqlGuid = encodeSQLText( createGuid());
                //string CreateKeyString = DbController.encodeSQLNumber(genericController.GetRandomInteger(core));
                //
                // Read in a record from the table to get fields
                DataTable dt = core.db.openTable(DataSource.name, TableName, "", "", "", 1);
                if (dt.Rows.Count == 0) {
                    dt.Dispose();
                    //
                    // --- no records were found, add a blank if we can
                    //
                    dt = core.db.insertTableRecordGetDataTable(DataSource.name, TableName, core.session.user.id);
                    if (dt.Rows.Count > 0) {
                        int RecordID = GenericController.encodeInteger(dt.Rows[0]["ID"]);
                        core.db.executeQuery("Update " + TableName + " Set active=0 where id=" + RecordID + ";", DataSource.name);
                    }
                }
                string SQL = "";
                int ContentID = 0;
                if (dt.Rows.Count == 0) {
                    throw new GenericException("Could Not add a record To table [" + TableName + "].");
                } else {
                    //
                    //----------------------------------------------------------------
                    // --- Find/Create the Content Definition
                    //----------------------------------------------------------------
                    //
                    ContentID = CdefController.getContentId(core, ContentName);
                    if (ContentID <= 0) {
                        //
                        // ----- Content definition not found, create it
                        //
                        CdefController.verifyContent_returnId(core, new Models.Domain.CDefModel() {
                            dataSourceName = DataSource.name,
                            tableName = TableName,
                            name = ContentName,
                            active = true
                        });
                        SQL = "Select ID from ccContent where name=" + DbController.encodeSQLText(ContentName);
                        dt = core.db.executeQuery(SQL);
                        if (dt.Rows.Count == 0) {
                            throw new GenericException("Content Definition [" + ContentName + "] could Not be selected by name after it was inserted");
                        } else {
                            ContentID = GenericController.encodeInteger(dt.Rows[0]["ID"]);
                            core.db.executeQuery("update ccContent Set CreateKey=0 where id=" + ContentID);
                        }
                        dt.Dispose();
                        core.cache.invalidateAll();
                        core.clearMetaData();
                    }
                    //
                    //-----------------------------------------------------------
                    // --- Create the ccFields records for the new table
                    //-----------------------------------------------------------
                    //
                    // ----- locate the field in the content field table
                    //
                    SQL = "Select name from ccFields where ContentID=" + ContentID + ";";
                    DataTable dtFields = core.db.executeQuery(SQL);
                    //
                    // ----- verify all the table fields
                    foreach (DataColumn dcTableColumns in dt.Columns) {
                        //
                        // ----- see if the field is already in the content fields
                        string UcaseTableColumnName = GenericController.vbUCase(dcTableColumns.ColumnName);
                        bool ContentFieldFound = false;
                        foreach (DataRow drContentRecords in dtFields.Rows) {
                            if (GenericController.vbUCase(GenericController.encodeText(drContentRecords["name"])) == UcaseTableColumnName) {
                                ContentFieldFound = true;
                                break;
                            }
                        }
                        if (!ContentFieldFound) {
                            //
                            // create the content field
                            //
                            createContentFieldFromTableField(ContentName, dcTableColumns.ColumnName, GenericController.encodeInteger(dcTableColumns.DataType));
                        } else {
                            //
                            // touch field so upgrade does not delete it
                            //
                            core.db.executeQuery("update ccFields Set CreateKey=0 where (Contentid=" + ContentID + ") And (name = " + DbController.encodeSQLText(UcaseTableColumnName) + ")");
                        }
                    }
                }
                //
                // Fill ContentControlID fields with new ContentID
                //
                SQL = "Update " + TableName + " Set ContentControlID=" + ContentID + " where (ContentControlID Is null);";
                core.db.executeQuery(SQL, DataSource.name);
                //
                // ----- Load CDef
                //       Load only if the previous state of autoload was true
                //       Leave Autoload false during load so more do not trigger
                //
                core.cache.invalidateAll();
                core.clearMetaData();
                dt.Dispose();
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        // 
        //========================================================================
        /// <summary>
        /// Define a Content Definition Field based only on what is known from a SQL table
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="FieldName"></param>
        /// <param name="ADOFieldType"></param>
        public void createContentFieldFromTableField(string ContentName, string FieldName, int ADOFieldType) {
            try {
                //
                CDefFieldModel field = new CDefFieldModel {
                    fieldTypeId = core.db.getFieldTypeIdByADOType(ADOFieldType),
                    caption = FieldName,
                    editSortPriority = 1000,
                    readOnly = false,
                    authorable = true,
                    adminOnly = false,
                    developerOnly = false,
                    textBuffered = false,
                    htmlContent = false
                };
                //
                switch (GenericController.vbUCase(FieldName)) {
                    //
                    // --- Core fields
                    //
                    case "NAME":
                        field.caption = "Name";
                        field.editSortPriority = 100;
                        break;
                    case "ACTIVE":
                        field.caption = "Active";
                        field.editSortPriority = 200;
                        field.fieldTypeId = fieldTypeIdBoolean;
                        field.defaultValue = "1";
                        break;
                    case "DATEADDED":
                        field.caption = "Created";
                        field.readOnly = true;
                        field.editSortPriority = 5020;
                        break;
                    case "CREATEDBY":
                        field.caption = "Created By";
                        field.fieldTypeId = fieldTypeIdLookup;
                        field.set_lookupContentName(core, "Members");
                        field.readOnly = true;
                        field.editSortPriority = 5030;
                        break;
                    case "MODIFIEDDATE":
                        field.caption = "Modified";
                        field.readOnly = true;
                        field.editSortPriority = 5040;
                        break;
                    case "MODIFIEDBY":
                        field.caption = "Modified By";
                        field.fieldTypeId = fieldTypeIdLookup;
                        field.set_lookupContentName(core, "Members");
                        field.readOnly = true;
                        field.editSortPriority = 5050;
                        break;
                    case "ID":
                        field.caption = "Number";
                        field.readOnly = true;
                        field.editSortPriority = 5060;
                        field.authorable = true;
                        field.adminOnly = false;
                        field.developerOnly = true;
                        break;
                    case "CONTENTCONTROLID":
                        field.caption = "Content Definition";
                        field.fieldTypeId = fieldTypeIdLookup;
                        field.set_lookupContentName(core, "Content");
                        field.editSortPriority = 5070;
                        field.authorable = true;
                        field.readOnly = false;
                        field.adminOnly = true;
                        field.developerOnly = true;
                        break;
                    case "CREATEKEY":
                        field.caption = "CreateKey";
                        field.readOnly = true;
                        field.editSortPriority = 5080;
                        field.authorable = false;
                        //
                        // --- fields related to body content
                        //
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
                        field.fieldTypeId = fieldTypeIdLookup;
                        field.set_lookupContentName(core, "Organizations");
                        field.editSortPriority = 2005;
                        field.authorable = true;
                        field.readOnly = false;
                        break;
                    case "COPYFILENAME":
                        field.caption = "Copy";
                        field.fieldTypeId = fieldTypeIdFileHTML;
                        field.textBuffered = true;
                        field.editSortPriority = 2010;
                        break;
                    case "BRIEFFILENAME":
                        field.caption = "Overview";
                        field.fieldTypeId = fieldTypeIdFileHTML;
                        field.textBuffered = true;
                        field.editSortPriority = 2020;
                        field.htmlContent = false;
                        break;
                    case "IMAGEFILENAME":
                        field.caption = "Image";
                        field.fieldTypeId = fieldTypeIdFile;
                        field.editSortPriority = 2040;
                        break;
                    case "THUMBNAILFILENAME":
                        field.caption = "Thumbnail";
                        field.fieldTypeId = fieldTypeIdFile;
                        field.editSortPriority = 2050;
                        break;
                    case "CONTENTID":
                        field.caption = "Content";
                        field.fieldTypeId = fieldTypeIdLookup;
                        field.set_lookupContentName(core, "Content");
                        field.readOnly = false;
                        field.editSortPriority = 2060;
                        //
                        // --- Record Features
                        //
                        break;
                    case "PARENTID":
                        field.caption = "Parent";
                        field.fieldTypeId = fieldTypeIdLookup;
                        field.set_lookupContentName(core, ContentName);
                        field.readOnly = false;
                        field.editSortPriority = 3000;
                        break;
                    case "MEMBERID":
                        field.caption = "Member";
                        field.fieldTypeId = fieldTypeIdLookup;
                        field.set_lookupContentName(core, "Members");
                        field.readOnly = false;
                        field.editSortPriority = 3005;
                        break;
                    case "CONTACTMEMBERID":
                        field.caption = "Contact";
                        field.fieldTypeId = fieldTypeIdLookup;
                        field.set_lookupContentName(core, "Members");
                        field.readOnly = false;
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
                        // --- Display only information
                        //
                        break;
                    case "VIEWINGS":
                        field.caption = "Viewings";
                        field.readOnly = true;
                        field.editSortPriority = 5000;
                        field.defaultValue = "0";
                        break;
                    case "CLICKS":
                        field.caption = "Clicks";
                        field.readOnly = true;
                        field.editSortPriority = 5010;
                        field.defaultValue = "0";
                        break;
                }
                CdefController.verifyContentField_returnID(core, ContentName, field);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
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
                LogController.handleError( core,ex);
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
        public void deleteTableRecordChunks(string DataSourceName, string TableName, string Criteria, int ChunkSize = 1000, int MaxChunkCount = 1000) {
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
            DataSourceType = getDataSourceType(DataSourceName);
            if ((DataSourceType != DataSourceTypeODBCSQLServer) && (DataSourceType != DataSourceTypeODBCAccess)) {
                //
                // If not SQL server, just delete them
                //
                deleteTableRecords(TableName, Criteria, DataSourceName);
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
                    if (getDataSourceType(DataSourceName) == DataSourceTypeODBCMySQL) {
                        SQL = "delete from " + TableName + " where id in (select ID from " + TableName + " where " + Criteria + " limit " + iChunkSize + ")";
                    } else {
                        SQL = "delete from " + TableName + " where id in (select top " + iChunkSize + " ID from " + TableName + " where " + Criteria + ")";
                    }
                    executeQuery(SQL, DataSourceName);
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
                    LogController.handleError( core,new GenericException("Error deleting record chunks. No records were deleted and the process was not complete."));
                } else if (LoopCount >= iChunkCount) {
                    //
                    // records did not delete
                    //
                    LogController.handleError( core,new GenericException("Error deleting record chunks. The maximum chunk count was exceeded while deleting records."));
                }
            }
        }
        //
        //=============================================================================
        /// <summary>
        /// getLinkByContentRecordKey
        /// </summary>
        /// <param name="ContentRecordKey"></param>
        /// <param name="DefaultLink"></param>
        /// <returns></returns>
        public string getLinkByContentRecordKey(string ContentRecordKey, string DefaultLink = "") {
            string result = "";
            try {
                int CSPointer = 0;
                string[] KeySplit = null;
                int ContentID = 0;
                int RecordID = 0;
                string ContentName = null;
                int templateId = 0;
                int ParentID = 0;
                string DefaultTemplateLink = null;
                string TableName = null;
                string DataSource = null;
                int ParentContentID = 0;
                bool recordfound = false;
                //
                if (!string.IsNullOrEmpty(ContentRecordKey)) {
                    //
                    // First try main_ContentWatch table for a link
                    //
                    CSPointer = csOpen("Content Watch", "ContentRecordKey=" + encodeSQLText(ContentRecordKey), "", true, 0, false, false, "Link,Clicks");
                    if (csOk(CSPointer)) {
                        result = core.db.csGetText(CSPointer, "Link");
                    }
                    core.db.csClose(ref CSPointer);
                    //
                    if (string.IsNullOrEmpty(result)) {
                        //
                        // try template for this page
                        //
                        KeySplit = ContentRecordKey.Split('.');
                        if (KeySplit.GetUpperBound(0) == 1) {
                            ContentID = GenericController.encodeInteger(KeySplit[0]);
                            if (ContentID != 0) {
                                ContentName = CdefController.getContentNameByID(core, ContentID);
                                RecordID = GenericController.encodeInteger(KeySplit[1]);
                                if (!string.IsNullOrEmpty(ContentName) & RecordID != 0) {
                                    if (CdefController.getContentTablename(core, ContentName) == "ccPageContent") {
                                        CSPointer = core.db.csOpenRecord(ContentName, RecordID, false, false, "TemplateID,ParentID");
                                        if (csOk(CSPointer)) {
                                            recordfound = true;
                                            templateId = csGetInteger(CSPointer, "TemplateID");
                                            ParentID = csGetInteger(CSPointer, "ParentID");
                                        }
                                        csClose(ref CSPointer);
                                        if (!recordfound) {
                                            //
                                            // This content record does not exist - remove any records with this ContentRecordKey pointer
                                            //
                                            deleteContentRecords("Content Watch", "ContentRecordKey=" + encodeSQLText(ContentRecordKey));
                                            core.db.deleteContentRules(CdefController.getContentId(core, ContentName), RecordID);
                                        } else {

                                            if (templateId != 0) {
                                                CSPointer = core.db.csOpenRecord("Page Templates", templateId, false, false, "Link");
                                                if (csOk(CSPointer)) {
                                                    result = csGetText(CSPointer, "Link");
                                                }
                                                csClose(ref CSPointer);
                                            }
                                            if (string.IsNullOrEmpty(result) && ParentID != 0) {
                                                TableName = CdefController.getContentTablename(core, ContentName);
                                                DataSource = CdefController.getContentDataSource(core, ContentName);
                                                CSPointer = csOpenSql("Select ContentControlID from " + TableName + " where ID=" + RecordID, DataSource);
                                                if (csOk(CSPointer)) {
                                                    ParentContentID = GenericController.encodeInteger(csGetText(CSPointer, "ContentControlID"));
                                                }
                                                csClose(ref CSPointer);
                                                if (ParentContentID != 0) {
                                                    result = getLinkByContentRecordKey(encodeText(ParentContentID + "." + ParentID), "");
                                                }
                                            }
                                            if (string.IsNullOrEmpty(result)) {
                                                DefaultTemplateLink = core.siteProperties.getText("SectionLandingLink", "/" + core.siteProperties.serverPageDefault);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(result)) {
                            result = GenericController.modifyLinkQuery(result, rnPageId, RecordID.ToString(), true);
                        }
                    }
                }
                //
                if (string.IsNullOrEmpty(result)) {
                    result = DefaultLink;
                }
                //
                result = GenericController.encodeVirtualPath(result, core.appConfig.cdnFileUrl, appRootPath, core.webServer.requestDomain);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
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
        // ====================================================================================================
        //
        public int getTableID(string TableName) {
            int result = 0;
            int CS = 0;
            CS = core.db.csOpenSql("Select ID from ccTables where name=" + DbController.encodeSQLText(TableName), "", 1);
            if (core.db.csOk(CS)) {
                result = core.db.csGetInteger(CS, "ID");
            }
            core.db.csClose(ref CS);
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
        // ====================================================================================================
        //
        public void setContentCopy(string CopyName, string Content) {
            //
            int CS = 0;
            string iCopyName = null;
            string iContent = null;
            const string ContentName = "Copy Content";
            //
            //  BuildVersion = app.dataBuildVersion
            if (false) //.3.210" Then
            {
                throw (new Exception("Contensive database was created with version " + core.siteProperties.dataBuildVersion + ". main_SetContentCopy requires an builder."));
            } else {
                iCopyName = GenericController.encodeText(CopyName);
                iContent = GenericController.encodeText(Content);
                CS = csOpen(ContentName, "name=" + encodeSQLText(iCopyName));
                if (!csOk(CS)) {
                    csClose(ref CS);
                    CS = csInsertRecord(ContentName);
                }
                if (csOk(CS)) {
                    csSet(CS, "name", iCopyName);
                    csSet(CS, "Copy", iContent);
                }
                csClose(ref CS);
            }
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
                        result = AdminUIController.getRecordEditLink(core,ContentName, RecordID, GenericController.encodeBoolean(AllowCut), RecordName, core.session.isEditing(ContentName));
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
        //=================================================================================
        //
        public static bool isDataTableOk(DataTable dt) {
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
        public static  string csGetRecordAddLink( CoreController core,  int csPtr, string PresetNameValueList = "", bool AllowPaste = false) {
            string result = "";
            try {
                if (csPtr >= 0) {
                    string ContentName = core.db.csGetContentName(csPtr);
                    if (string.IsNullOrEmpty(ContentName)) {
                        LogController.logWarn(core, "getRecordAddLink was called with a ContentSet that was created with an SQL statement. The function requires a ContentSet opened with an OpenCSContent.");
                    } else {
                        result = AdminUIController.getRecordAddLink(core,ContentName, PresetNameValueList, AllowPaste);
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
        public DataTable executeRemoteQuery( string remoteQueryKey ) {
            DataTable result = null;
            try {
                var remoteQuery = Models.Db.RemoteQueryModel.create(core, remoteQueryKey);
                if ( remoteQuery == null ) {
                    throw new GenericException("remoteQuery was not found with key [" + remoteQueryKey + "]");
                } else {
                    result = executeQuery(remoteQuery.SQLQuery);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                result = null;
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
        /// <summary>
        /// Return just the tablename from a tablename reference (database.object.tablename->tablename)
        /// </summary>
        /// <param name="DbObject"></param>
        /// <returns></returns>
        public static string getDbObjectTableName(string DbObject) {
            string tempGetDbObjectTableName = null;
            int Position = 0;
            //
            tempGetDbObjectTableName = DbObject;
            Position = tempGetDbObjectTableName.LastIndexOf(".") + 1;
            if (Position > 0) {
                tempGetDbObjectTableName = tempGetDbObjectTableName.Substring(Position);
            }
            return tempGetDbObjectTableName;
        }
        //
        //========================================================================
        // main_DeleteChildRecords
        //========================================================================
        //
        public string deleteChildRecords(string ContentName, int RecordID, bool ReturnListWithoutDelete = false) {
            string result = "";
            try {
                bool QuickEditing = false;
                string[] IDs = null;
                int IDCnt = 0;
                int Ptr = 0;
                int CS = 0;
                string ChildList = null;
                bool SingleEntry = false;
                //
                // For now, the child delete only works in non-workflow
                //
                CS = core.db.csOpen(ContentName, "parentid=" + RecordID, "", false, 0, false, false, "ID");
                while (core.db.csOk(CS)) {
                    result += "," + core.db.csGetInteger(CS, "ID");
                    core.db.csGoNext(CS);
                }
                core.db.csClose(ref CS);
                if (!string.IsNullOrEmpty(result)) {
                    result = result.Substring(1);
                    //
                    // main_Get a list of all pages, but do not delete anything yet
                    //
                    IDs = result.Split(',');
                    IDCnt = IDs.GetUpperBound(0) + 1;
                    SingleEntry = (IDCnt == 1);
                    for (Ptr = 0; Ptr < IDCnt; Ptr++) {
                        ChildList = deleteChildRecords(ContentName, GenericController.encodeInteger(IDs[Ptr]), true);
                        if (!string.IsNullOrEmpty(ChildList)) {
                            result += "," + ChildList;
                            SingleEntry = false;
                        }
                    }
                    if (!ReturnListWithoutDelete) {
                        //
                        // Do the actual delete
                        //
                        IDs = result.Split(',');
                        IDCnt = IDs.GetUpperBound(0) + 1;
                        SingleEntry = (IDCnt == 1);
                        QuickEditing = core.session.isQuickEditing(core, "page content");
                        for (Ptr = 0; Ptr < IDCnt; Ptr++) {
                            int deleteRecordId = encodeInteger(IDs[Ptr]);
                            core.db.deleteTableRecord(deleteRecordId, PageContentModel.contentTableName);
                            PageContentModel.invalidateRecordCache(core, deleteRecordId);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }

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
                    //
                    // ----- Close all open csv_ContentSets, and make sure the RS is killed
                    //
                    if (contentSetStoreCount > 0) {
                        for (int CSPointer = 1; CSPointer <= contentSetStoreCount; CSPointer++) {
                            int tmpPtr = CSPointer;
                            if (contentSetStore[tmpPtr] != null) {
                                if (contentSetStore[tmpPtr].IsOpen) {
                                    csClose(ref tmpPtr);
                                }
                            }
                        }
                    }
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
    public class SqlFieldListClass {
        private List<NameValueClass> _sqlList = new List<NameValueClass>();
        public void add(string name, string value) {
            _sqlList.Add(new NameValueClass() {
                name = name,
                value = value
            });
        }
        public string getNameValueList() {
            string returnPairs = "";
            string delim = "";
            foreach (var nameValue in _sqlList) {
                returnPairs += delim + nameValue.name + "=" + nameValue.value;
                delim = ",";
            }
            return returnPairs;
        }
        public string getNameList() {
            string returnPairs = "";
            string delim = "";
            foreach (var nameValue in _sqlList) {
                returnPairs += delim + nameValue.name;
                delim = ",";
            }
            return returnPairs;
        }
        public string getValueList() {
            string returnPairs = "";
            string delim = "";
            foreach (var nameValue in _sqlList) {
                returnPairs += delim + nameValue.value;
                delim = ",";
            }
            return returnPairs;
        }
        public int count {
            get {
                return _sqlList.Count;
            }
        }

    }
}
