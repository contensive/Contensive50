
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
using Contensive.Addons.AdminSite.Controllers;

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
        private CoreController core;
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
        public int sqlCommandTimeout { get; set; } = 30;
        //
        //
        // +++++ start by removing the store - each instance of csController should 
        //
        //private ContentSetClass[] contentSetStore = new ContentSetClass[] { };
        //
        /// <summary>
        /// number of elements being used
        /// </summary>
        //private int contentSetStoreCount { get; set; }
        //
        /// <summary>
        /// number of elements available for use.
        /// </summary>
        //private int contentSetStoreSize { get; set; }
        //
        /// <summary>
        /// How many are added at a time
        /// </summary>
        //private const int contentSetStoreChunk = 50;
        //
        /// <summary>
        /// when true, all csOpen, etc, will be setup, but not return any data (csv_IsCSOK false), this is used to generate the csv_ContentSet.Source so we can run a csv_GetContentRows without first opening a recordset
        /// </summary>
        //public bool contentSetOpenWithoutRecords = false;
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
                                throw new NotImplementedException("MySql not implemented");
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
                    if (expressionDate == DateTime.MinValue) {
                        returnResult = "null";
                    } else {
                        returnResult = "'" + expressionDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
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
            if ( dt == null ) { return false; }
            if ( dt.Rows == null) { return false; }
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
            using (DataTable dt = core.db.executeQuery("select top 1 id from cccontent where name=" + encodeSQLText(contentName) + " order by id" )) {
                if (dt.Rows.Count == 0) { return 0; }
                return getDataRowFieldInteger(dt.Rows[0], "id");
            }
        }
        //
        // ====================================================================================================
        /// <summary>
        /// get the id of the table in the cctable table
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static int getTableID(CoreController core, string TableName) {
            using (DataTable dt = core.db.executeQuery("select top 1 id from cctables where name=" + encodeSQLText(TableName) + " order by id")) {
                if (dt.Rows.Count == 0) { return 0; }
                return getDataRowFieldInteger(dt.Rows[0], "id");
            }
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
