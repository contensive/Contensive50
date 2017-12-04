

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
        // 
        private coreClass cpCore;
        
        // 
        //  on Db success, verified set true. If error and not verified, a simple test is run. on failure, Db disabled 
        private bool dbVerified {
        }
        
        private struct ContentSetType2 {
            
            private bool IsOpen;
            
            //  If true, it is in use
            private DateTime LastUsed;
            
            //  The date/time this csv_ContentSet was last used
            private bool Updateable;
            
            //  Can not update an csv_OpenCSSQL because Fields are not accessable
            private bool NewRecord;
            
            //  true if it was created here
            private string ContentName;
            
            private Models.Complex.cdefModel CDef;
            
            private int OwnerMemberID;
            
            //  ID of the member who opened the csv_ContentSet
            // 
            //  Write Cache
            private Dictionary<string, string> writeCache;
            
            private bool IsModified;
            
            //  Set when CS is opened and if a save happens
            // 
            //  Recordset used to retrieve the results
            private DataTable dt;
            
            //  The Recordset
            // 
            private string Source;
            
            //  Holds the SQL that created the result set
            private string DataSource;
            
            //  The Datasource of the SQL that created the result set
            private int PageSize;
            
            //  Number of records in a cache page
            private int PageNumber;
            
            //  The Page that this result starts with
            // 
            //  Read Cache
            private int fieldPointer;
            
            //  ptr into fieldNames used for getFirstField, getnext, etc.
            private string[] fieldNames;
            
            //  1-D array of the result field names
            private int ResultColumnCount;
            
            //  number of columns in the fieldNames and readCache
            private bool ResultEOF;
            
            //  readCache is at the last record
            private string[,] readCache;
            
            private int readCacheRowCnt;
            
            //  number of rows in the readCache
            private int readCacheRowPtr;
            
            //  Pointer to the current result row, first row is 0, BOF is -1
            // 
            //  converted array to dictionary - Dim FieldPointer As Integer                ' Used for GetFirstField, GetNextField, etc
            private string SelectTableFieldList;
        }
        
        // 
        // ==========================================================================================
        // '' <summary>
        // '' app services constructor
        // '' </summary>
        // '' <param name="cp"></param>
        // '' <remarks></remarks>
        public dbController(coreClass cpCore) {
            try {
                this.cpCore = cpCore;
                _sqlTimeoutSecond = 30;
                sqlSlowThreshholdMsec = 1000;
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' return the correctly formated connection string for this datasource. Called only from within this class
        // '' </summary>
        // '' <returns>
        // '' </returns>
        public string getConnectionStringADONET(string catalogName, string dataSourceName, void =, void ) {
            // 
            // Warning!!! Optional parameters not supported
            //  (OLEDB) OLE DB Provider for SQL Server > "Provider=sqloledb;Data Source=MyServerName;Initial Catalog=MyDatabaseName;User Id=MyUsername;Password=MyPassword;"
            //      https://www.codeproject.com/Articles/2304/ADO-Connection-Strings#OLE%20DB%20SqlServer
            // 
            //  (OLEDB) Microsoft OLE DB Provider for SQL Server connection strings > "Provider=sqloledb;Data Source=myServerAddress;Initial Catalog=myDataBase;User Id = myUsername;Password=myPassword;"
            //      https://www.connectionstrings.com/microsoft-ole-db-provider-for-sql-server-sqloledb/
            // 
            //  (ADONET) .NET Framework Data Provider for SQL Server > Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password = myPassword;
            //      https://www.connectionstrings.com/sql-server/
            // 
            string returnConnString = "";
            try {
                // 
                //  -- simple local cache so it does not have to be recreated each time
                string cacheName = ("catalog:" 
                            + (catalogName + ("/datasource:" + dataSourceName)));
                if (connectionStringDict.ContainsKey(cacheName)) {
                    returnConnString = connectionStringDict(cacheName);
                }
                else {
                    // 
                    //  -- lookup dataSource
                    string normalizedDataSourceName = Models.Entity.dataSourceModel.normalizeDataSourceName(dataSourceName);
                    if ((string.IsNullOrEmpty(normalizedDataSourceName) 
                                || (normalizedDataSourceName == "default"))) {
                        // 
                        //  -- default datasource
                        returnConnString = ("" 
                                    + (cpCore.dbServer.getConnectionStringADONET() + ("Database=" 
                                    + (catalogName + ";"))));
                    }
                    else {
                        // 
                        //  -- custom datasource from Db in primary datasource
                        if (!cpCore.dataSourceDictionary.ContainsKey(normalizedDataSourceName)) {
                            // 
                            //  -- not found, this is a hard error
                            throw new ApplicationException(("Datasource [" 
                                            + (normalizedDataSourceName + "] was not found.")));
                        }
                        else {
                            // 
                            //  -- found in local cache
                            // With...
                            returnConnString = ("" + ("server=" 
                                        + (cpCore.dataSourceDictionary(normalizedDataSourceName).endPoint + (";" + ("User Id=" 
                                        + (cpCore.dataSourceDictionary(normalizedDataSourceName).username + (";" + ("Password=" 
                                        + (cpCore.dataSourceDictionary(normalizedDataSourceName).password + (";" + ("Database=" 
                                        + (catalogName + ";"))))))))))));
                        }
                        
                    }
                    
                    connectionStringDict.Add(cacheName, returnConnString);
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnConnString;
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' return the correctly formated connection string for this datasource. Called only from within this class
        // '' </summary>
        // '' <returns>
        // '' </returns>
        private string getConnectionStringOLEDB(string catalogName, string dataSourceName) {
            // 
            //  (OLEDB) OLE DB Provider for SQL Server > "Provider=sqloledb;Data Source=MyServerName;Initial Catalog=MyDatabaseName;User Id=MyUsername;Password=MyPassword;"
            //      https://www.codeproject.com/Articles/2304/ADO-Connection-Strings#OLE%20DB%20SqlServer
            // 
            //  (OLEDB) Microsoft OLE DB Provider for SQL Server connection strings > "Provider=sqloledb;Data Source=myServerAddress;Initial Catalog=myDataBase;User Id = myUsername;Password=myPassword;"
            //      https://www.connectionstrings.com/microsoft-ole-db-provider-for-sql-server-sqloledb/
            // 
            //  (ADONET) .NET Framework Data Provider for SQL Server > Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password = myPassword;
            //      https://www.connectionstrings.com/sql-server/
            // 
            string returnConnString = "";
            try {
                string normalizedDataSourceName = Models.Entity.dataSourceModel.normalizeDataSourceName(dataSourceName);
                string defaultConnString = "";
                string serverUrl = cpCore.serverConfig.defaultDataSourceAddress;
                if ((serverUrl.IndexOf(":") > 0)) {
                    serverUrl = serverUrl.Substring(0, serverUrl.IndexOf(":"));
                }
                
                ("" + ("Provider=sqloledb;" + ("Data Source=" 
                            + (serverUrl + (";" + ("Initial Catalog=" 
                            + (catalogName + (";" + ("User Id=" 
                            + (cpCore.serverConfig.defaultDataSourceUsername + (";" + ("Password=" 
                            + (cpCore.serverConfig.defaultDataSourcePassword + (";" + ""))))))))))))));
                if ((string.IsNullOrEmpty(normalizedDataSourceName) 
                            || (normalizedDataSourceName == "default"))) {
                    // 
                    //  -- default datasource
                    returnConnString = defaultConnString;
                }
                else {
                    // 
                    //  -- custom datasource from Db in primary datasource
                    if (!cpCore.dataSourceDictionary.ContainsKey(normalizedDataSourceName)) {
                        // 
                        //  -- not found, this is a hard error
                        throw new ApplicationException(("Datasource [" 
                                        + (normalizedDataSourceName + "] was not found.")));
                    }
                    else {
                        // 
                        //  -- found in local cache
                        // With...
                        ("" + ("Provider=sqloledb;" + ("Data Source=" 
                                    + (cpCore.dataSourceDictionary(normalizedDataSourceName).endPoint + (";" + ("User Id=" 
                                    + (cpCore.dataSourceDictionary(normalizedDataSourceName).username + (";" + ("Password=" 
                                    + (cpCore.dataSourceDictionary(normalizedDataSourceName).password + (";" + "")))))))))));
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnConnString;
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' Execute a command (sql statemwent) and return a dataTable object
        // '' </summary>
        // '' <param name="sql"></param>
        // '' <param name="dataSourceName"></param>
        // '' <param name="startRecord"></param>
        // '' <param name="maxRecords"></param>
        // '' <returns></returns>
        public DataTable executeQuery(string sql, string dataSourceName, void =, void , int startRecord, void =, void 0, int maxRecords, void =, void 9999, ref int recordsReturned, void =, void 0) {
            DataTable returnData = new DataTable();
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            try {
                if (!dbEnabled) {
                    // 
                    //  -- db not available
                }
                else if ((cpCore.serverConfig == null)) {
                    // 
                    //  -- server config fail
                    cpCore.handleException(new ApplicationException("Cannot execute Sql in dbController without an application"));
                }
                else if ((cpCore.serverConfig.appConfig == null)) {
                    // 
                    //  -- server config fail
                    cpCore.handleException(new ApplicationException("Cannot execute Sql in dbController without an application"));
                }
                else {
                    string connString = this.getConnectionStringADONET(cpCore.serverConfig.appConfig.name, dataSourceName);
                    // returnData = executeSql_noErrorHandling(sql, getConnectionStringADONET(cpCore.serverConfig.appConfig.name, dataSourceName), startRecord, maxRecords, recordsAffected)
                    // 
                    //  REFACTOR
                    //  consider writing cs intrface to sql dataReader object -- one row at a time, vaster.
                    //  https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqldatareader.aspx
                    // 
                    Stopwatch sw = Stopwatch.StartNew();
                    Using;
                    ((void)(connSQL));
                    new SqlConnection(connString);
                    connSQL.Open();
                    Using;
                    ((void)(cmdSQL));
                    new SqlCommand();
                    cmdSQL.CommandType = Data.CommandType.Text;
                    cmdSQL.CommandText = sql;
                    cmdSQL.Connection = connSQL;
                    Using;
                    adptSQL = new SqlClient.SqlDataAdapter(cmdSQL);
                    recordsReturned = adptSQL.Fill(startRecord, maxRecords, returnData);
                }
                
            }
            
        }
    }
}
return returnData;
EndFunctionCatchcpCore.handleException(ex);
throw;
Endtry {
}

CatchcpCore.handleException(ex);
throw;
Endtry {
}

cs_initData(returnCs);
EndIfIfcpCore.handleException(ex);
throw;
Endtry {
    return returnCs;
}

Endclass  {
    
    public class sqlFieldListClass {
        
        private List<NameValuePairType> _sqlList = new List<NameValuePairType>();
        
        public void add(string name, string value) {
            NameValuePairType nameValue;
            nameValue.Name = name;
            nameValue.Value = value;
            _sqlList.Add(nameValue);
        }
        
        public string getNameValueList() {
            string returnPairs = "";
            string delim = "";
            foreach (nameValue in _sqlList) {
                (delim 
                            + (nameValue.Name + ("=" + nameValue.Value)));
                delim = ",";
            }
            
            return returnPairs;
        }
        
        public string getNameList() {
            string returnPairs = "";
            string delim = "";
            foreach (nameValue in _sqlList) {
                (delim + nameValue.Name);
                delim = ",";
            }
            
            return returnPairs;
        }
        
        public string getValueList() {
            string returnPairs = "";
            string delim = "";
            foreach (nameValue in _sqlList) {
                (delim + nameValue.Value);
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

    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' Execute a command via ADO and return a recordset. Note the recordset must be disposed by the caller.
    // '' </summary>
    // '' <param name="sql"></param>
    // '' <param name="dataSourceName"></param>
    // '' <param name="startRecord"></param>
    // '' <param name="maxRecords"></param>
    // '' <returns>You must close the recordset after use.</returns>
    public ADODB.Recordset executeSql_getRecordSet(string sql, string dataSourceName, void =, void , int startRecord, void =, void 0, int maxRecords, void =, void 9999) {
        // 
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        //  from - https://support.microsoft.com/en-us/kb/308611
        // 
        //  REFACTOR 
        //  - add start recrod And max record in
        //  - add dataSourceName into the getConnectionString call - if no dataSourceName, return catalog in cluster Db, else return connstring
        // 
        // Dim cn As ADODB.Connection = New ADODB.Connection()
        ADODB.Recordset rs = new ADODB.Recordset();
        string connString = this.getConnectionStringOLEDB(cpCore.serverConfig.appConfig.name, dataSourceName);
        try {
            if (dbEnabled) {
                if ((maxRecords > 0)) {
                    rs.MaxRecords = maxRecords;
                }
                
                //  Open Recordset without connection object.
                rs.Open(sql, connString, ADODB.CursorTypeEnum.adOpenKeyset, ADODB.LockTypeEnum.adLockOptimistic, -1);
                //  REFACTOR -- dbVerified Is only for the datasource. Need one for each...
                dbVerified = true;
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
            ApplicationException newEx = new ApplicationException(("Exception [" 
                            + (ex.Message + ("] executing sql [" 
                            + (sql + ("], datasource [" 
                            + (dataSourceName + ("], startRecord [" 
                            + (startRecord + ("], maxRecords [" 
                            + (maxRecords + "]")))))))))), ex);
            cpCore.handleException(newEx);
            throw newEx;
        }
        
        return rs;
    }
    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' execute sql on a specific datasource. No data is returned.
    // '' </summary>
    // '' <param name="sql"></param>
    // '' <param name="dataSourceName"></param>
    public void executeNonQuery(string sql, string dataSourceName, void =, void , ref int recordsAffected, void =, void 0) {
        try {
            if (dbEnabled) {
                string connString = this.getConnectionStringADONET(cpCore.serverConfig.appConfig.name, dataSourceName);
                // Warning!!! Optional parameters not supported
                // Warning!!! Optional parameters not supported
                Using;
                ((void)(connSQL));
                new SqlConnection(connString);
                connSQL.Open();
                Using;
                ((void)(cmdSQL));
                new SqlCommand();
                cmdSQL.CommandType = Data.CommandType.Text;
                cmdSQL.CommandText = sql;
                cmdSQL.Connection = connSQL;
                recordsAffected = cmdSQL.ExecuteNonQuery();
            }
            
        }
        
        dbVerified = true;
    }
    
    private Exception ex;
    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' execute sql on a specific datasource asynchonously. No data is returned.
    // '' </summary>
    // '' <param name="sql"></param>
    // '' <param name="dataSourceName"></param>
    public void executeNonQueryAsync(string sql, string dataSourceName, void =, void ) {
        try {
            if (dbEnabled) {
                string connString = this.getConnectionStringADONET(cpCore.serverConfig.appConfig.name, dataSourceName);
                // Warning!!! Optional parameters not supported
                Using;
                ((void)(connSQL));
                new SqlConnection(connString);
                connSQL.Open();
                Using;
                ((void)(cmdSQL));
                new SqlCommand();
                cmdSQL.CommandType = Data.CommandType.Text;
                cmdSQL.CommandText = sql;
                cmdSQL.Connection = connSQL;
                cmdSQL.ExecuteNonQueryAsync();
            }
            
        }
        
        dbVerified = true;
    }
    
    private Exception ex;
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Update a record in a table
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <param name="Criteria"></param>
    // '' <param name="sqlList"></param>
    public void updateTableRecord(string DataSourceName, string TableName, string Criteria, sqlFieldListClass sqlList) {
        try {
            string SQL = ("update " 
                        + (TableName + (" set " 
                        + (sqlList.getNameValueList + (" where " 
                        + (Criteria + ";"))))));
            executeNonQuery(SQL, DataSourceName);
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' iInserts a record into a table and returns the ID
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <param name="MemberID"></param>
    // '' <returns></returns>
    public int insertTableRecordGetId(string DataSourceName, string TableName, int MemberID, void =, void 0) {
        int returnId = 0;
        // Warning!!! Optional parameters not supported
        try {
            Using;
            ((DataTable)(dt)) = insertTableRecordGetDataTable(DataSourceName, TableName, MemberID);
            if ((dt.Rows.Count > 0)) {
                returnId = genericController.EncodeInteger(dt.Rows[0].Item["id"]);
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnId;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Insert a record in a table, select it and return a datatable. You must dispose the datatable.
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <param name="MemberID"></param>
    // '' <returns></returns>
    public DataTable insertTableRecordGetDataTable(string DataSourceName, string TableName, int MemberID, void =, void 0) {
        DataTable returnDt = null;
        // Warning!!! Optional parameters not supported
        try {
            sqlFieldListClass sqlList = new sqlFieldListClass();
            string CreateKeyString;
            string DateAddedString;
            // 
            CreateKeyString = encodeSQLNumber(genericController.GetRandomInteger);
            DateAddedString = encodeSQLDate(Now);
            // 
            sqlList.add("createkey", CreateKeyString);
            sqlList.add("dateadded", DateAddedString);
            sqlList.add("createdby", encodeSQLNumber(MemberID));
            sqlList.add("ModifiedDate", DateAddedString);
            sqlList.add("ModifiedBy", encodeSQLNumber(MemberID));
            sqlList.add("ContentControlID", encodeSQLNumber(0));
            sqlList.add("Name", encodeSQLText(""));
            sqlList.add("Active", encodeSQLNumber(1));
            // 
            insertTableRecord(DataSourceName, TableName, sqlList);
            returnDt = openTable(DataSourceName, TableName, ("(DateAdded=" 
                            + (DateAddedString + (")and(CreateKey=" 
                            + (CreateKeyString + ")")))), "ID DESC", ,, 1);
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnDt;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Insert a record in a table. There is no check for core fields
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <param name="sqlList"></param>
    public void insertTableRecord(string DataSourceName, string TableName, sqlFieldListClass sqlList) {
        try {
            if ((sqlList.count > 0)) {
                string sql = ("INSERT INTO " 
                            + (TableName + ("(" 
                            + (sqlList.getNameList + (")values(" 
                            + (sqlList.getValueList + ")"))))));
                DataTable dt = this.executeQuery(sql, DataSourceName);
                dt.Dispose();
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
    // '' Opens the table specified and returns the data in a datatable. Returns all the active records in the table. Find the content record first, just for the dataSource.
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <param name="Criteria"></param>
    // '' <param name="SortFieldList"></param>
    // '' <param name="SelectFieldList"></param>
    // '' <param name="PageSize"></param>
    // '' <param name="PageNumber"></param>
    // '' <returns></returns>
    public DataTable openTable(
                string DataSourceName, 
                string TableName, 
                string Criteria, 
                void =, 
                void , 
                string SortFieldList, 
                void =, 
                void , 
                string SelectFieldList, 
                void =, 
                void , 
                int PageSize, 
                void =, 
                void 9999, 
                int PageNumber, 
                void =, 
                void 1) {
        DataTable returnDataTable = null;
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        try {
            string SQL = "SELECT";
            if (string.IsNullOrEmpty(SelectFieldList)) {
                " *";
            }
            else {
                (" " + SelectFieldList);
            }
            
            (" FROM " + TableName);
            if (!string.IsNullOrEmpty(Criteria)) {
                (" WHERE (" 
                            + (Criteria + ")"));
            }
            
            if (!string.IsNullOrEmpty(SortFieldList)) {
                (" ORDER BY " + SortFieldList);
            }
            
            // SQL &= ";"
            returnDataTable = this.executeQuery(SQL, DataSourceName, ((PageNumber - 1) 
                            * PageSize), PageSize);
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnDataTable;
    }
    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' update the transaction log
    // '' </summary>
    // '' <param name="LogEntry"></param>
    private void saveTransactionLog(string sql, long ElapsedMilliseconds) {
        // 
        //  -- do not allow reentry
        //  -- if during save, site properties need to be loaded, this stack-overflows
        if (!saveTransactionLog_InProcess) {
            saveTransactionLog_InProcess = true;
            if ((cpCore.serverConfig.enableLogging 
                        && (cpCore.serverConfig.appConfig.appStatus == Models.Entity.serverConfigModel.appStatusEnum.OK))) {
                if (cpCore.siteProperties.allowTransactionLog) {
                    string LogEntry = (("duration [" 
                                + (ElapsedMilliseconds + ("], sql [" 
                                + (sql + "]"))))).Replace("\r", "").Replace("\n", "");
                    logController.appendLog(cpCore, LogEntry, "DbTransactions");
                }
                
                if ((ElapsedMilliseconds > sqlSlowThreshholdMsec)) {
                    logController.appendLog(cpCore, ("query time  " 
                                    + (ElapsedMilliseconds + ("ms, sql: " + sql))), "SlowSQL");
                }
                
            }
            
            saveTransactionLog_InProcess = false;
        }
        
    }
    
    // '
    // '====================================================================================================
    // ''' <summary>
    // ''' Finds the where clause (first WHERE not in single quotes). returns 0 if not found, otherwise returns locaion of word where
    // ''' </summary>
    // ''' <param name="SQL"></param>
    // ''' <returns></returns>
    // Private Function getSQLWherePosition(ByVal SQL As String) As Integer
    //     Dim returnPos As Integer = 0
    //     Try
    //         If genericController.isInStr(1, SQL, "WHERE", vbTextCompare) Then
    //             '
    //             ' ----- contains the word "WHERE", now weed out if not a where clause
    //             '
    //             returnPos = InStrRev(SQL, " WHERE ", , vbTextCompare)
    //             If returnPos = 0 Then
    //                 returnPos = InStrRev(SQL, ")WHERE ", , vbTextCompare)
    //                 If returnPos = 0 Then
    //                     returnPos = InStrRev(SQL, " WHERE(", , vbTextCompare)
    //                     If returnPos = 0 Then
    //                         returnPos = InStrRev(SQL, ")WHERE(", , vbTextCompare)
    //                     End If
    //                 End If
    //             End If
    //         End If
    //     Catch ex As Exception
    //         cpCore.handleException(ex) : Throw
    //     End Try
    //     Return returnPos
    // End Function
    // 
    // ========================================================================
    // '' <summary>
    // '' Returns true if the field exists in the table
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <param name="FieldName"></param>
    // '' <returns></returns>
    public bool isSQLTableField(string DataSourceName, string TableName, string FieldName) {
        bool returnOK = false;
        try {
            Models.Complex.tableSchemaModel tableSchema = Models.Complex.tableSchemaModel.getTableSchema(cpCore, TableName, DataSourceName);
            if (tableSchema) {
                IsNot;
                null;
                returnOK = tableSchema.columns.Contains(FieldName.ToLower);
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnOK;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Returns true if the table exists
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <returns></returns>
    public bool isSQLTable(string DataSourceName, string TableName) {
        bool ReturnOK = false;
        try {
            ReturnOK = !(Models.Complex.tableSchemaModel.getTableSchema(cpCore, TableName, DataSourceName) == null);
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return ReturnOK;
    }
    
    //    
    // ========================================================================
    // '' <summary>
    // '' Check for a table in a datasource
    // '' if the table is missing, create the table and the core fields
    // '' if NoAutoIncrement is false or missing, the ID field is created as an auto incremenet
    // '' if NoAutoIncrement is true, ID is created an an long
    // '' if the table is present, check all core fields
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <param name="AllowAutoIncrement"></param>
    public void createSQLTable(string DataSourceName, string TableName, bool AllowAutoIncrement, void =, void True) {
        try {
            if (string.IsNullOrEmpty(TableName)) {
                // 
                // Warning!!! Optional parameters not supported
                //  tablename required
                // 
                throw new ArgumentException("Tablename can not be blank.");
            }
            else if ((genericController.vbInstr(1, TableName, ".") != 0)) {
                // 
                //  Remote table -- remote system controls remote tables
                // 
                throw new ArgumentException("Tablename can not contain a period(.)");
            }
            else {
                // 
                //  Local table -- create if not in schema
                // 
                if ((Models.Complex.tableSchemaModel.getTableSchema(cpCore, TableName, DataSourceName) == null)) {
                    if (!AllowAutoIncrement) {
                        string SQL = ("Create Table " 
                                    + (TableName + ("(ID " 
                                    + (getSQLAlterColumnType(DataSourceName, FieldTypeIdInteger) + ");"))));
                        this.executeQuery(SQL, DataSourceName).Dispose();
                    }
                    else {
                        string SQL = ("Create Table " 
                                    + (TableName + ("(ID " 
                                    + (getSQLAlterColumnType(DataSourceName, FieldTypeIdAutoIdIncrement) + ");"))));
                        this.executeQuery(SQL, DataSourceName).Dispose();
                    }
                    
                }
                
                // 
                //  ----- Test the common fields required in all tables
                // 
                createSQLTableField(DataSourceName, TableName, "id", FieldTypeIdAutoIdIncrement);
                createSQLTableField(DataSourceName, TableName, "name", FieldTypeIdText);
                createSQLTableField(DataSourceName, TableName, "dateAdded", FieldTypeIdDate);
                createSQLTableField(DataSourceName, TableName, "createdby", FieldTypeIdInteger);
                createSQLTableField(DataSourceName, TableName, "modifiedBy", FieldTypeIdInteger);
                createSQLTableField(DataSourceName, TableName, "ModifiedDate", FieldTypeIdDate);
                createSQLTableField(DataSourceName, TableName, "active", FieldTypeIdBoolean);
                createSQLTableField(DataSourceName, TableName, "createKey", FieldTypeIdInteger);
                createSQLTableField(DataSourceName, TableName, "sortOrder", FieldTypeIdText);
                createSQLTableField(DataSourceName, TableName, "contentControlID", FieldTypeIdInteger);
                createSQLTableField(DataSourceName, TableName, "ccGuid", FieldTypeIdText);
                //  -- 20171029 - deprecating fields makes migration difficult. add back and figure out future path
                createSQLTableField(DataSourceName, TableName, "ContentCategoryID", FieldTypeIdInteger);
                // 
                //  ----- setup core indexes
                // 
                //  20171029 primary key dow not need index -- Call createSQLIndex(DataSourceName, TableName, TableName & "Id", "ID")
                createSQLIndex(DataSourceName, TableName, (TableName + "Active"), "ACTIVE");
                createSQLIndex(DataSourceName, TableName, (TableName + "Name"), "NAME");
                createSQLIndex(DataSourceName, TableName, (TableName + "SortOrder"), "SORTORDER");
                createSQLIndex(DataSourceName, TableName, (TableName + "DateAdded"), "DATEADDED");
                createSQLIndex(DataSourceName, TableName, (TableName + "CreateKey"), "CREATEKEY");
                createSQLIndex(DataSourceName, TableName, (TableName + "ContentControlID"), "CONTENTCONTROLID");
                createSQLIndex(DataSourceName, TableName, (TableName + "ModifiedDate"), "MODIFIEDDATE");
                createSQLIndex(DataSourceName, TableName, (TableName + "ccGuid"), "CCGUID");
            }
            
            Models.Complex.tableSchemaModel.tableSchemaListClear(cpCore);
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Check for a field in a table in the database, if missing, create the field
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <param name="FieldName"></param>
    // '' <param name="fieldType"></param>
    // '' <param name="clearMetaCache"></param>
    public void createSQLTableField(string DataSourceName, string TableName, string FieldName, int fieldType, bool clearMetaCache, void =, void False) {
        try {
            if (((fieldType == FieldTypeIdRedirect) 
                        || (fieldType == FieldTypeIdManyToMany))) {
                // 
                // Warning!!! Optional parameters not supported
                //  OK -- contensive fields with no table field
                // 
                fieldType = fieldType;
            }
            else if (string.IsNullOrEmpty(TableName)) {
                // 
                //  Bad tablename
                // 
                throw new ArgumentException("Table Name cannot be blank.");
            }
            else if ((fieldType == 0)) {
                // 
                //  Bad fieldtype
                // 
                throw new ArgumentException(("invalid fieldtype [" 
                                + (fieldType + "]")));
            }
            else if ((genericController.vbInstr(1, TableName, ".") != 0)) {
                // 
                //  External table
                // 
                throw new ArgumentException("Table name cannot include a period(.)");
            }
            else if ((FieldName == "")) {
                // 
                //  Bad fieldname
                // 
                throw new ArgumentException("Field name cannot be blank");
            }
            else if (!isSQLTableField(DataSourceName, TableName, FieldName)) {
                string SQL = ("ALTER TABLE " 
                            + (TableName + (" ADD " 
                            + (FieldName + " "))));
                if (!genericController.vbIsNumeric(fieldType)) {
                    // 
                    //  ----- support old calls
                    // 
                    fieldType;
                }
                else {
                    // 
                    //  ----- translater type into SQL string
                    // 
                    getSQLAlterColumnType(DataSourceName, fieldType);
                }
                
                this.executeQuery(SQL, DataSourceName).Dispose();
                // 
                if (clearMetaCache) {
                    cpCore.cache.invalidateAll();
                    cpCore.doc.clearMetaData();
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
    // '' Delete (drop) a table
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    public void deleteTable(string DataSourceName, string TableName) {
        try {
            this.executeQuery(("DROP TABLE " + TableName), DataSourceName).Dispose();
            cpCore.cache.invalidateAll();
            cpCore.doc.clearMetaData();
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Delete a table field from a table
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <param name="FieldName"></param>
    public void deleteTableField(string DataSourceName, string TableName, string FieldName) {
        // Throw New NotImplementedException("deletetablefield")
        try {
            if (isSQLTableField(DataSourceName, TableName, FieldName)) {
                // 
                //    Delete any indexes that use this column
                //  refactor -- need to finish this
                DataTable tableScheme = getIndexSchemaData(TableName);
                // With cdefCache.tableSchema(SchemaPointer)
                //     If .IndexCount > 0 Then
                //         For IndexPointer = 0 To .IndexCount - 1
                //             If FieldName = .IndexFieldName(IndexPointer) Then
                //                 Call csv_DeleteTableIndex(DataSourceName, TableName, .IndexName(IndexPointer))
                //             End If
                //         Next
                //     End If
                // End With
                // 
                //    Delete the field
                // 
                this.executeQuery(("ALTER TABLE " 
                                + (TableName + (" DROP COLUMN " 
                                + (FieldName + ";")))), DataSourceName);
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
    // '' Create an index on a table, Fieldnames is  a comma delimited list of fields
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <param name="IndexName"></param>
    // '' <param name="FieldNames"></param>
    // '' <param name="clearMetaCache"></param>
    public void createSQLIndex(string DataSourceName, string TableName, string IndexName, string FieldNames, bool clearMetaCache, void =, void False) {
        try {
            Models.Complex.tableSchemaModel ts;
            // Warning!!! Optional parameters not supported
            if (!(string.IsNullOrEmpty(TableName) 
                        && (string.IsNullOrEmpty(IndexName) && string.IsNullOrEmpty(FieldNames)))) {
                ts = Models.Complex.tableSchemaModel.getTableSchema(cpCore, TableName, DataSourceName);
                if (ts) {
                    IsNot;
                    null;
                    if (!ts.indexes.Contains(IndexName.ToLower)) {
                        this.executeQuery(("CREATE INDEX " 
                                        + (IndexName + (" ON " 
                                        + (TableName + ("( " 
                                        + (FieldNames + " );")))))), DataSourceName);
                        if (clearMetaCache) {
                            cpCore.cache.invalidateAll();
                            cpCore.doc.clearMetaData();
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
    // =============================================================
    // '' <summary>
    // '' Return a record name given the record id. If not record is found, blank is returned.
    // '' </summary>
    // '' <param name="ContentName"></param>
    // '' <param name="RecordID"></param>
    // '' <returns></returns>
    public string getRecordName(string ContentName, int RecordID) {
        string returnRecordName = "";
        try {
            int CS = cs_openContentRecord(ContentName, RecordID, ,, ,, "Name");
            if (csOk(CS)) {
                returnRecordName = csGet(CS, "Name");
            }
            
            csClose(CS);
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnRecordName;
    }
    
    // 
    // =============================================================
    // '' <summary>
    // '' get the lowest recordId based on its name. If no record is found, 0 is returned
    // '' </summary>
    // '' <param name="ContentName"></param>
    // '' <param name="RecordName"></param>
    // '' <returns></returns>
    public int getRecordID(string ContentName, string RecordName) {
        int returnValue = 0;
        try {
            if ((!string.IsNullOrEmpty(ContentName.Trim()) 
                        && !string.IsNullOrEmpty(RecordName.Trim()))) {
                int cs = csOpen(ContentName, ("name=" + encodeSQLText(RecordName)), "ID", ,, ,, "ID");
                if (csOk(cs)) {
                    returnValue = csGetInteger(cs, "ID");
                }
                
                csClose(cs);
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnValue;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Get FieldDescritor from FieldType
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="fieldType"></param>
    // '' <returns></returns>
    public string getSQLAlterColumnType(string DataSourceName, int fieldType) {
        string returnType = "";
        try {
            switch (fieldType) {
                case FieldTypeIdBoolean:
                    returnType = "Int NULL";
                    break;
                case FieldTypeIdCurrency:
                    returnType = "Float NULL";
                    break;
                case FieldTypeIdDate:
                    returnType = "DateTime NULL";
                    break;
                case FieldTypeIdFloat:
                    returnType = "Float NULL";
                    break;
                case FieldTypeIdCurrency:
                    returnType = "Float NULL";
                    break;
                case FieldTypeIdInteger:
                    returnType = "Int NULL";
                    break;
                case FieldTypeIdLookup:
                case FieldTypeIdMemberSelect:
                    returnType = "Int NULL";
                    break;
                case FieldTypeIdManyToMany:
                case FieldTypeIdRedirect:
                case FieldTypeIdFileImage:
                case FieldTypeIdLink:
                case FieldTypeIdResourceLink:
                case FieldTypeIdText:
                case FieldTypeIdFile:
                case FieldTypeIdFileText:
                case FieldTypeIdFileJavascript:
                case FieldTypeIdFileXML:
                case FieldTypeIdFileCSS:
                case FieldTypeIdFileHTML:
                    returnType = "VarChar(255) NULL";
                    break;
                case FieldTypeIdLongText:
                case FieldTypeIdHTML:
                    // 
                    //  ----- Longtext, depends on datasource
                    // 
                    returnType = "Text Null";
                    break;
                case FieldTypeIdAutoIdIncrement:
                    // 
                    //  ----- autoincrement type, depends on datasource
                    // 
                    returnType = "INT IDENTITY PRIMARY KEY";
                    break;
                default:
                    throw new ApplicationException(("Can Not proceed because the field being created has an invalid FieldType [" 
                                    + (fieldType + "]")));
                    break;
            }
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnType;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Delete an Index for a table
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="TableName"></param>
    // '' <param name="IndexName"></param>
    public void deleteSqlIndex(string DataSourceName, string TableName, string IndexName) {
        try {
            Models.Complex.tableSchemaModel ts;
            int DataSourceType;
            string sql;
            // 
            ts = Models.Complex.tableSchemaModel.getTableSchema(cpCore, TableName, DataSourceName);
            if (!(ts == null)) {
                if (ts.indexes.Contains(IndexName.ToLower)) {
                    DataSourceType = getDataSourceType(DataSourceName);
                    switch (DataSourceType) {
                        case DataSourceTypeODBCAccess:
                            sql = ("DROP INDEX " 
                                        + (IndexName + (" On " 
                                        + (TableName + ";"))));
                            break;
                        case DataSourceTypeODBCMySQL:
                            throw new NotImplementedException("mysql");
                            break;
                        default:
                            sql = ("DROP INDEX " 
                                        + (TableName + ("." 
                                        + (IndexName + ";"))));
                            break;
                    }
                    this.executeQuery(sql, DataSourceName);
                    cpCore.cache.invalidateAll();
                    cpCore.doc.clearMetaData();
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
    // '' returns true if the cdef field exists
    // '' </summary>
    // '' <param name="ContentID"></param>
    // '' <param name="FieldName"></param>
    // '' <returns></returns>
    public bool isCdefField(int ContentID, string FieldName) {
        bool returnOk = false;
        try {
            DataTable dt = this.executeQuery(("Select top 1 id from ccFields where name=" 
                            + (encodeSQLText(FieldName) + (" And contentid=" + ContentID))));
            isCdefField = genericController.isDataTableOk(dt);
            dt.Dispose();
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnOk;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Get a DataSource type (SQL Server, etc) from its Name
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <returns></returns>
    // 
    public int getDataSourceType(string DataSourceName) {
        return DataSourceTypeODBCSQLServer;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Get FieldType from ADO Field Type
    // '' </summary>
    // '' <param name="ADOFieldType"></param>
    // '' <returns></returns>
    public int getFieldTypeIdByADOType(int ADOFieldType) {
        int returnType;
        try {
            switch (ADOFieldType) {
                case 2:
                    returnType = FieldTypeIdFloat;
                    break;
                case 3:
                    returnType = FieldTypeIdInteger;
                    break;
                case 4:
                    returnType = FieldTypeIdFloat;
                    break;
                case 5:
                    returnType = FieldTypeIdFloat;
                    break;
                case 6:
                    returnType = FieldTypeIdInteger;
                    break;
                case 11:
                    returnType = FieldTypeIdBoolean;
                    break;
                case 135:
                    returnType = FieldTypeIdDate;
                    break;
                case 200:
                    returnType = FieldTypeIdText;
                    break;
                case 201:
                    returnType = FieldTypeIdLongText;
                    break;
                case 202:
                    returnType = FieldTypeIdText;
                    break;
                default:
                    returnType = FieldTypeIdText;
                    break;
            }
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnType;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Get FieldType from FieldTypeName
    // '' </summary>
    // '' <param name="FieldTypeName"></param>
    // '' <returns></returns>
    public int getFieldTypeIdFromFieldTypeName(string FieldTypeName) {
        int returnTypeId = 0;
        try {
            switch (genericController.vbLCase(FieldTypeName)) {
                case FieldTypeNameLcaseBoolean:
                    returnTypeId = FieldTypeIdBoolean;
                    break;
                case FieldTypeNameLcaseCurrency:
                    returnTypeId = FieldTypeIdCurrency;
                    break;
                case FieldTypeNameLcaseDate:
                    returnTypeId = FieldTypeIdDate;
                    break;
                case FieldTypeNameLcaseFile:
                    returnTypeId = FieldTypeIdFile;
                    break;
                case FieldTypeNameLcaseFloat:
                    returnTypeId = FieldTypeIdFloat;
                    break;
                case FieldTypeNameLcaseImage:
                    returnTypeId = FieldTypeIdFileImage;
                    break;
                case FieldTypeNameLcaseLink:
                    returnTypeId = FieldTypeIdLink;
                    break;
                case FieldTypeNameLcaseResourceLink:
                case "resource link":
                case "resourcelink":
                    returnTypeId = FieldTypeIdResourceLink;
                    break;
                case FieldTypeNameLcaseInteger:
                    returnTypeId = FieldTypeIdInteger;
                    break;
                case FieldTypeNameLcaseLongText:
                case "longtext":
                case "Long text":
                    returnTypeId = FieldTypeIdLongText;
                    break;
                case FieldTypeNameLcaseLookup:
                case "lookuplist":
                case "lookup list":
                    returnTypeId = FieldTypeIdLookup;
                    break;
                case FieldTypeNameLcaseMemberSelect:
                    returnTypeId = FieldTypeIdMemberSelect;
                    break;
                case FieldTypeNameLcaseRedirect:
                    returnTypeId = FieldTypeIdRedirect;
                    break;
                case FieldTypeNameLcaseManyToMany:
                    returnTypeId = FieldTypeIdManyToMany;
                    break;
                case FieldTypeNameLcaseTextFile:
                case "text file":
                case "textfile":
                    returnTypeId = FieldTypeIdFileText;
                    break;
                case FieldTypeNameLcaseCSSFile:
                case "cssfile":
                case "css file":
                    returnTypeId = FieldTypeIdFileCSS;
                    break;
                case FieldTypeNameLcaseXMLFile:
                case "xmlfile":
                case "xml file":
                    returnTypeId = FieldTypeIdFileXML;
                    break;
                case FieldTypeNameLcaseJavascriptFile:
                case "javascript file":
                case "javascriptfile":
                case "js file":
                case "jsfile":
                    returnTypeId = FieldTypeIdFileJavascript;
                    break;
                case FieldTypeNameLcaseText:
                    returnTypeId = FieldTypeIdText;
                    break;
                case "autoincrement":
                    returnTypeId = FieldTypeIdAutoIdIncrement;
                    break;
                case "memberselect":
                    returnTypeId = FieldTypeIdMemberSelect;
                    break;
                case FieldTypeNameLcaseHTML:
                    returnTypeId = FieldTypeIdHTML;
                    break;
                case FieldTypeNameLcaseHTMLFile:
                case "html file":
                    returnTypeId = FieldTypeIdFileHTML;
                    break;
                default:
                    returnTypeId = FieldTypeIdText;
                    break;
            }
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnTypeId;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Opens a dataTable for the table/row definied by the contentname and criteria
    // '' </summary>
    // '' <param name="ContentName"></param>
    // '' <param name="Criteria"></param>
    // '' <param name="SortFieldList"></param>
    // '' <param name="ActiveOnly"></param>
    // '' <param name="MemberID"></param>
    // '' <param name="ignorefalse2"></param>
    // '' <param name="ignorefalse"></param>
    // '' <param name="SelectFieldList"></param>
    // '' <param name="PageSize"></param>
    // '' <param name="PageNumber"></param>
    // '' <returns></returns>
    // ========================================================================
    // 
    public int csOpen(
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
                void 0, 
                bool ignorefalse2, 
                void =, 
                void False, 
                bool ignorefalse, 
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
        int returnCs = -1;
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
            string[] SortFields;
            string SortField;
            int Ptr;
            int Cnt;
            string ContentCriteria;
            string TableName;
            string DataSourceName;
            bool iActiveOnly;
            string iSortFieldList;
            // Dim iWorkflowRenderingMode As Boolean
            // Dim AllowWorkflowSave As Boolean
            int iMemberID;
            string iCriteria;
            string iSelectFieldList;
            Models.Complex.cdefModel CDef;
            string TestUcaseFieldList;
            string SQL;
            // 
            if (string.IsNullOrEmpty(ContentName)) {
                throw new ApplicationException("ContentName cannot be blank");
            }
            else {
                CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
                if ((CDef == null)) {
                    throw new ApplicationException(("No content found For [" 
                                    + (ContentName + "]")));
                }
                else if ((CDef.Id <= 0)) {
                    throw new ApplicationException(("No content found For [" 
                                    + (ContentName + "]")));
                }
                else {
                    // 
                    // hint = hint & ", 100"
                    iActiveOnly = ActiveOnly;
                    iSortFieldList = genericController.encodeEmptyText(SortFieldList, CDef.DefaultSortMethod);
                    iMemberID = MemberID;
                    iCriteria = genericController.encodeEmptyText(Criteria, "");
                    iSelectFieldList = genericController.encodeEmptyText(SelectFieldList, CDef.SelectCommaList);
                    // 
                    //  verify the sortfields are in this table
                    // 
                    if ((iSortFieldList != "")) {
                        SortFields = iSortFieldList.Split(",");
                        Cnt = (UBound(SortFields) + 1);
                        for (Ptr = 0; (Ptr 
                                    <= (Cnt - 1)); Ptr++) {
                            SortField = SortFields[Ptr].ToLower;
                            SortField = genericController.vbReplace(SortField, "asc", "", 1, 99, vbTextCompare);
                            SortField = genericController.vbReplace(SortField, "desc", "", 1, 99, vbTextCompare);
                            SortField = SortField.Trim();
                            if (!CDef.selectList.Contains(SortField)) {
                                // throw (New ApplicationException("Unexpected exception"))
                                throw new ApplicationException(("The field [" 
                                                + (SortField + ("] was used In a sort method For content [" 
                                                + (ContentName + "], but the content does Not include this field.")))));
                            }
                            
                        }
                        
                    }
                    
                    // 
                    //  ----- fixup the criteria to include the ContentControlID(s) / EditSourceID
                    // 
                    ContentCriteria = CDef.ContentControlCriteria;
                    if ((ContentCriteria == "")) {
                        ContentCriteria = "(1=1)";
                    }
                    else {
                        // 
                        //  remove tablename from contentcontrolcriteria - if in workflow mode, and authoringtable is different, this would be wrong
                        //  also makes sql smaller, and is not necessary
                        // 
                        ContentCriteria = genericController.vbReplace(ContentCriteria, (CDef.ContentTableName + "."), "");
                    }
                    
                    if ((iCriteria != "")) {
                        ContentCriteria = (ContentCriteria + ("And(" 
                                    + (iCriteria + ")")));
                    }
                    
                    // 
                    //  ----- Active Only records
                    // 
                    if (iActiveOnly) {
                        ContentCriteria = (ContentCriteria + "And(Active<>0)");
                    }
                    
                    // 
                    //  ----- Process Select Fields, make sure ContentControlID,ID,Name,Active are included
                    // 
                    iSelectFieldList = genericController.vbReplace(iSelectFieldList, '\t', " ");
                    while ((genericController.vbInstr(1, iSelectFieldList, " ,") != 0)) {
                        iSelectFieldList = genericController.vbReplace(iSelectFieldList, " ,", ",");
                    }
                    
                    while ((genericController.vbInstr(1, iSelectFieldList, ", ") != 0)) {
                        iSelectFieldList = genericController.vbReplace(iSelectFieldList, ", ", ",");
                    }
                    
                    if (((iSelectFieldList != "") 
                                && ((iSelectFieldList.IndexOf("*", 0, System.StringComparison.OrdinalIgnoreCase) + 1) 
                                == 0))) {
                        TestUcaseFieldList = genericController.vbUCase(("," 
                                        + (iSelectFieldList + ",")));
                        if ((genericController.vbInstr(1, TestUcaseFieldList, ",CONTENTCONTROLID,", vbTextCompare) == 0)) {
                            iSelectFieldList = (iSelectFieldList + ",ContentControlID");
                        }
                        
                        if ((genericController.vbInstr(1, TestUcaseFieldList, ",NAME,", vbTextCompare) == 0)) {
                            iSelectFieldList = (iSelectFieldList + ",Name");
                        }
                        
                        if ((genericController.vbInstr(1, TestUcaseFieldList, ",ID,", vbTextCompare) == 0)) {
                            iSelectFieldList = (iSelectFieldList + ",ID");
                        }
                        
                        if ((genericController.vbInstr(1, TestUcaseFieldList, ",ACTIVE,", vbTextCompare) == 0)) {
                            iSelectFieldList = (iSelectFieldList + ",ACTIVE");
                        }
                        
                    }
                    
                    // 
                    TableName = CDef.ContentTableName;
                    DataSourceName = CDef.ContentDataSourceName;
                    // 
                    //  ----- Check for blank Tablename or DataSource
                    // 
                    if ((TableName == "")) {
                        throw new Exception(("Error opening csv_ContentSet because Content Definition [" 
                                        + (ContentName + "] does Not reference a valid table")));
                    }
                    else if ((DataSourceName == "")) {
                        throw new Exception(("Error opening csv_ContentSet because Table Definition [" 
                                        + (TableName + "] does Not reference a valid datasource")));
                    }
                    
                    // 
                    //  ----- If no select list, use *
                    // 
                    if ((iSelectFieldList == "")) {
                        iSelectFieldList = "*";
                    }
                    
                    // 
                    //  ----- Open the csv_ContentSet
                    // 
                    returnCs = cs_init(iMemberID);
                    // With...
                    (PageNumber <= 0);
                    true.ContentName = PageNumber;
                    contentSetStore(returnCs).Updateable = PageNumber;
                    PageNumber = 1;
                    PageSize = PageSize;
                    (PageSize < 0);
                    PageSize = maxLongValue;
                }
                
            }
            
            PageSize = 0;
            PageSize = pageSizeDefault;
        }
        
        CDef.SelectTableFieldList = iSelectFieldList;
        DataSourceName.CDef = iSelectFieldList;
        DataSource = iSelectFieldList;
        // 
        if ((iSortFieldList != "")) {
            SQL = ("Select " 
                        + (iSelectFieldList + (" FROM " 
                        + (TableName + (" WHERE (" 
                        + (ContentCriteria + (") ORDER BY " + iSortFieldList)))))));
        }
        else {
            SQL = ("Select " 
                        + (iSelectFieldList + (" FROM " 
                        + (TableName + (" WHERE (" 
                        + (ContentCriteria + ")"))))));
        }
        
        // 
        if (contentSetOpenWithoutRecords) {
            // 
            //  Save the source, but do not open the recordset
            // 
            contentSetStore(returnCs).Source = SQL;
        }
        else {
            // 
            //  Run the query
            // 
            contentSetStore(returnCs).dt = this.executeQuery(SQL, DataSourceName, ., PageSizeStar(, ., (PageNumber - 1));
            PageSize;
        }
        
    }
    
    private Exception ex;
    
    // 
    // ========================================================================
    // '' <summary>
    // '' csv_DeleteCSRecord
    // '' </summary>
    // '' <param name="CSPointer"></param>
    public void csDeleteRecord(int CSPointer) {
        try {
            // 
            int LiveRecordID;
            int ContentID;
            string ContentName;
            string ContentDataSourceName;
            string ContentTableName;
            string[,] SQLName;
            string[,] SQLValue;
            string Filename;
            // 
            if (!csOk(CSPointer)) {
                // 
                throw new ArgumentException("csv_ContentSet Is empty Or at End-Of-file");
            }
            else if (!contentSetStore(CSPointer).Updateable) {
                // 
                throw new ArgumentException("csv_ContentSet Is Not Updateable");
            }
            else {
                // With...
                ContentID = contentSetStore(CSPointer).CDef.Id;
                ContentName = contentSetStore(CSPointer).CDef.Name;
                ContentTableName = contentSetStore(CSPointer).CDef.ContentTableName;
                ContentDataSourceName = contentSetStore(CSPointer).CDef.ContentDataSourceName;
                if ((ContentName == "")) {
                    throw new ArgumentException("csv_ContentSet Is Not based On a Content Definition");
                }
                else {
                    LiveRecordID = csGetInteger(CSPointer, "ID");
                    // 
                    //  delete any files (only if filename is part of select)
                    // 
                    string fieldName;
                    Models.Complex.CDefFieldModel field;
                    foreach (selectedFieldName in contentSetStore(CSPointer).CDef.selectList) {
                        if (contentSetStore(CSPointer).CDef.fields.ContainsKey[selectedFieldName.ToLower()]) {
                            field = contentSetStore(CSPointer).CDef.fields;
                            selectedFieldName.ToLower();
                            // With...
                            fieldName = field.nameLc;
                            switch (field.fieldTypeId) {
                                case FieldTypeIdFile:
                                case FieldTypeIdFileImage:
                                case FieldTypeIdFileCSS:
                                case FieldTypeIdFileJavascript:
                                case FieldTypeIdFileXML:
                                    // 
                                    //  public content files
                                    // 
                                    Filename = csGetText(CSPointer, fieldName);
                                    if ((Filename != "")) {
                                        cpCore.cdnFiles.deleteFile(Filename);
                                        // Call cpCore.cdnFiles.deleteFile(cpCore.cdnFiles.joinPath(cpCore.serverConfig.appConfig.cdnFilesNetprefix, Filename))
                                    }
                                    
                                    break;
                                case FieldTypeIdFileText:
                                case FieldTypeIdFileHTML:
                                    // 
                                    //  private files
                                    // 
                                    Filename = csGetText(CSPointer, fieldName);
                                    if ((Filename != "")) {
                                        cpCore.cdnFiles.deleteFile(Filename);
                                    }
                                    
                                    break;
                            }
                        }
                        
                    }
                    
                    // 
                    //  non-workflow mode, delete the live record
                    // 
                    deleteTableRecord(ContentTableName, LiveRecordID, ContentDataSourceName);
                    if (workflowController.csv_AllowAutocsv_ClearContentTimeStamp) {
                        cpCore.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(ContentTableName, "id", LiveRecordID.ToString()));
                        // Call cpCore.cache.invalidateObject(ContentName)
                    }
                    
                    deleteContentRules(ContentID, LiveRecordID);
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
    // '' Opens a Select SQL into a csv_ContentSet
    // ''   Returns and long that points into the csv_ContentSet array
    // ''   If there was a problem, it returns -1
    // '' </summary>
    // '' <param name="DataSourceName"></param>
    // '' <param name="SQL"></param>
    // '' <param name="PageSize"></param>
    // '' <param name="PageNumber"></param>
    // '' <returns></returns>
    public int csOpenSql_rev(string DataSourceName, string SQL, int PageSize, void =, void 9999, int PageNumber, void =, void 1) {
        return csOpenSql(SQL, DataSourceName, PageSize, PageNumber);
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' openSql
    // '' </summary>
    // '' <param name="SQL"></param>
    // '' <param name="DataSourceName"></param>
    // '' <param name="PageSize"></param>
    // '' <param name="PageNumber"></param>
    // '' <returns></returns>
    public int csOpenSql(string SQL, string DataSourceName, void =, void , int PageSize, void =, void 9999, int PageNumber, void =, void 1) {
        int returnCs = -1;
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        try {
            returnCs = cs_init(cpCore.doc.authContext.user.id);
            // With...
            PageSize.DataSource = "";
            PageNumber.PageSize = "";
            "".PageNumber = "";
            false.ContentName = "";
            contentSetStore(returnCs).Updateable = "";
            // 
            if (useCSReadCacheMultiRow) {
                contentSetStore(returnCs).dt = this.executeQuery(SQL, DataSourceName, (PageSize 
                                * (PageNumber - 1)), PageSize);
                cs_initData(returnCs);
                // Call cs_loadCurrentRow(returnCs)
            }
            else {
                contentSetStore(returnCs).dt = this.executeQuery(SQL, DataSourceName, (PageSize 
                                * (PageNumber - 1)), PageSize);
                cs_initData(returnCs);
                // Call cs_loadCurrentRow(returnCs)
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
    // '' <summary>
    // '' initialize a cs
    // '' </summary>
    // '' <param name="MemberID"></param>
    // '' <returns></returns>
    private int cs_init(int MemberID) {
        int returnCs = -1;
        try {
            int ptr;
            // 
            if ((contentSetStoreCount > 0)) {
                for (ptr = 1; (ptr <= contentSetStoreCount); ptr++) {
                    if (!contentSetStore(ptr).IsOpen) {
                        // 
                        //  Open CS found
                        // 
                        returnCs = ptr;
                        break;
                    }
                    
                }
                
            }
            
            // 
            if ((returnCs == -1)) {
                if ((contentSetStoreCount >= contentSetStoreSize)) {
                    contentSetStoreSize = (contentSetStoreSize + contentSetStoreChunk);
                    object Preserve;
                    contentSetStore((contentSetStoreSize + 1));
                }
                
                contentSetStoreCount = (contentSetStoreCount + 1);
                returnCs = contentSetStoreCount;
            }
            
            // 
            // With...
            false.OwnerMemberID = DateTime.Now;
            false.IsModified = DateTime.Now;
            true.Updateable = DateTime.Now;
            "".NewRecord = DateTime.Now;
            true.ContentName = DateTime.Now;
            contentSetStore(returnCs).IsOpen = DateTime.Now;
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnCs;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Close a csv_ContentSet
    // '' Closes a currently open csv_ContentSet
    // '' sets CSPointer to -1
    // '' </summary>
    // '' <param name="CSPointer"></param>
    // '' <param name="AsyncSave"></param>
    public void csClose(ref int CSPointer, bool AsyncSave, void =, void False) {
        try {
            if (((CSPointer > 0) 
                        && (CSPointer <= contentSetStoreCount))) {
                // With...
                if (contentSetStore(CSPointer).IsOpen) {
                    csSave2(CSPointer, AsyncSave);
                    // Warning!!! Optional parameters not supported
                    object .;
                    readCache[0, 0];
                    object .;
                    true.IsOpen = false;
                    (1.ResultEOF * -1) = false;
                    0.readCacheRowPtr = false;
                    0.readCacheRowCnt = false;
                    fieldNames[0].ResultColumnCount = false;
                    if (!(contentSetStore(CSPointer).dt == null)) {
                        contentSetStore(CSPointer).dt.Dispose;
                        // With...
                        ((Exception)(ex));
                        cpCore.handleException(ex);
                        throw;
                    }
                    
                }
                
                // 
                // ========================================================================
                //  Move the csv_ContentSet to the next row
                // ========================================================================
                // 
                csGoNext(((int)(CSPointer)), Optional, AsyncSaveAsBoolean=False);
                try {
                    // 
                    string ContentName;
                    int RecordID;
                    // 
                    if (!csOk(CSPointer)) {
                        // 
                        throw new ApplicationException("CSPointer Not csv_IsCSOK.");
                    }
                    else {
                        csSave2(CSPointer, AsyncSave);
                        // contentSetStore(CSPointer).WorkflowEditingMode = False
                        // 
                        //  Move to next row
                        // 
                        contentSetStore(CSPointer).readCacheRowPtr = (contentSetStore(CSPointer).readCacheRowPtr + 1);
                        if (!cs_IsEOF(CSPointer)) {
                            // 
                            //  Not EOF
                            // 
                            //  Call cs_loadCurrentRow(CSPointer)
                            // 
                            //  Set Workflow Edit Mode from Request and EditLock state
                            // 
                            if (!cs_IsEOF(CSPointer)) {
                                ContentName = contentSetStore(CSPointer).ContentName;
                                RecordID = csGetInteger(CSPointer, "ID");
                                if (!cpCore.workflow.isRecordLocked(ContentName, RecordID, contentSetStore(CSPointer).OwnerMemberID)) {
                                    cpCore.workflow.setEditLock(ContentName, RecordID, contentSetStore(CSPointer).OwnerMemberID);
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                }
                catch (Exception ex) {
                    cpCore.handleException(ex);
                    throw;
                }
                
                // 
                // ========================================================================
                //  Move the csv_ContentSet to the first row
                // ========================================================================
                // 
                cs_goFirst(((int)(CSPointer)), Optional, AsyncSaveAsBoolean=False);
                try {
                    if (!csOk(CSPointer)) {
                        throw new ApplicationException("data set is not valid");
                    }
                    else {
                        csSave2(CSPointer, AsyncSave);
                        contentSetStore(CSPointer).readCacheRowPtr = 0;
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
            // '' getField returns a value from a nameValue dataset specified by the cs pointer. get the value of a field within a csv_ContentSet,  if CS in authoring mode, it gets the edit record value, except ID field. otherwise, it gets the live record value.
            // '' </summary>
            // '' <param name="CSPointer"></param>
            // '' <param name="FieldName"></param>
            // '' <returns></returns>
            ((string)(cs_getValue(((int)(CSPointer)), ((string)(FieldName)))));
            string returnValue = "";
            try {
                bool fieldFound;
                int ColumnPointer;
                string fieldNameTrimUpper;
                string fieldNameTrim;
                // 
                fieldNameTrim = FieldName.Trim();
                fieldNameTrimUpper = genericController.vbUCase(fieldNameTrim);
                if (!csOk(CSPointer)) {
                    throw new ApplicationException(("Attempt To GetValue fieldname[" 
                                    + (FieldName + "], but the dataset Is empty Or does Not point To a valid row")));
                }
                else {
                    // With...
                    // 
                    // 
                    fieldFound = false;
                    if ((contentSetStore(CSPointer).writeCache.Count > 0)) {
                        // 
                        //  ----- something has been set in buffer, check it first
                        // 
                        if (contentSetStore(CSPointer).writeCache.ContainsKey) {
                            FieldName.ToLower;
                            returnValue = contentSetStore(CSPointer).writeCache.Item;
                            FieldName.ToLower;
                            fieldFound = true;
                        }
                        
                    }
                    
                    if (!fieldFound) {
                        // 
                        //  ----- attempt read from readCache
                        // 
                        if (useCSReadCacheMultiRow) {
                            if (!contentSetStore(CSPointer).dt.Columns.Contains) {
                                FieldName.ToLower;
                                if (contentSetStore(CSPointer).Updateable) {
                                    throw new ApplicationException(("Field [" 
                                                    + (fieldNameTrim + ("] was Not found in [" 
                                                    + (contentSetStore(CSPointer).ContentName + ("] with selected fields [" 
                                                    + (contentSetStore(CSPointer).SelectTableFieldList + "]")))))));
                                }
                                else {
                                    throw new ApplicationException(("Field [" 
                                                    + (fieldNameTrim + ("] was Not found in sql [" 
                                                    + (contentSetStore(CSPointer).Source + "]")))));
                                }
                                
                            }
                            else {
                                returnValue = genericController.encodeText(contentSetStore(CSPointer).dt.Rows, contentSetStore(CSPointer).readCacheRowPtr.Item[FieldName.ToLower]);
                            }
                            
                        }
                        else {
                            // 
                            //  ----- read the value from the Recordset Result
                            // 
                            if ((contentSetStore(CSPointer).ResultColumnCount > 0)) {
                                for (ColumnPointer = 0; (ColumnPointer 
                                            <= (contentSetStore(CSPointer).ResultColumnCount - 1)); ColumnPointer++) {
                                    if (contentSetStore(CSPointer).fieldNames) {
                                        ColumnPointer = fieldNameTrimUpper;
                                        returnValue = contentSetStore(CSPointer).readCache;
                                        ColumnPointer;
                                        0;
                                        if ((contentSetStore(CSPointer).Updateable 
                                                    && ((contentSetStore(CSPointer).ContentName != "") 
                                                    && (FieldName != "")))) {
                                            if (contentSetStore(CSPointer).CDef.fields) {
                                                FieldName.ToLower().Scramble;
                                                returnValue = genericController.TextDeScramble(cpCore, genericController.encodeText(returnValue));
                                            }
                                            
                                        }
                                        
                                        break;
                                    }
                                    
                                }
                                
                                if ((ColumnPointer == contentSetStore(CSPointer).ResultColumnCount)) {
                                    throw new ApplicationException(("Field [" 
                                                    + (fieldNameTrim + ("] was Not found In csv_ContentSet from source [" 
                                                    + (contentSetStore(CSPointer).Source + "]")))));
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                    contentSetStore(CSPointer).LastUsed = DateTime.Now;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnValue;
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' get the first fieldname in the CS, Returns null if there are no more
        // '' </summary>
        // '' <param name="CSPointer"></param>
        // '' <returns></returns>
    }
    
    string cs_getFirstFieldName(int CSPointer) {
        string returnFieldName = "";
        try {
            if (!csOk(CSPointer)) {
                throw new ApplicationException("data set is not valid");
            }
            else {
                contentSetStore(CSPointer).fieldPointer = 0;
                returnFieldName = cs_getNextFieldName(CSPointer);
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnFieldName;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' get the next fieldname in the CS, Returns null if there are no more
    // '' </summary>
    // '' <param name="CSPointer"></param>
    // '' <returns></returns>
    string cs_getNextFieldName(int CSPointer) {
        string returnFieldName = "";
        try {
            if (!csOk(CSPointer)) {
                throw new ApplicationException("data set is not valid");
            }
            else {
                // With...
                if (useCSReadCacheMultiRow) {
                    while (((returnFieldName == "") 
                                && (contentSetStore(CSPointer).fieldPointer < contentSetStore(CSPointer).ResultColumnCount))) {
                        returnFieldName = contentSetStore(CSPointer).fieldNames;
                        contentSetStore(CSPointer).fieldPointer.fieldPointer = (contentSetStore(CSPointer).fieldPointer + 1);
                    }
                    
                }
                else {
                    while (((returnFieldName == "") 
                                && (contentSetStore(CSPointer).fieldPointer < contentSetStore(CSPointer).dt.Columns.Count))) {
                        returnFieldName = contentSetStore(CSPointer).dt.Columns;
                        contentSetStore(CSPointer).fieldPointer.ColumnName.fieldPointer = (contentSetStore(CSPointer).fieldPointer + 1);
                    }
                    
                }
                
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnFieldName;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' get the type of a field within a csv_ContentSet
    // '' </summary>
    // '' <param name="CSPointer"></param>
    // '' <param name="FieldName"></param>
    // '' <returns></returns>
    public int cs_getFieldTypeId(int CSPointer, string FieldName) {
        int returnFieldTypeid = 0;
        try {
            if (csOk(CSPointer)) {
                if (contentSetStore(CSPointer).Updateable) {
                    // With...
                    if ((contentSetStore(CSPointer).CDef.Name != "")) {
                        returnFieldTypeid = contentSetStore(CSPointer).CDef.fields;
                        FieldName.ToLower().fieldTypeId;
                    }
                    
                }
                
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnFieldTypeid;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' get the caption of a field within a csv_ContentSet
    // '' </summary>
    // '' <param name="CSPointer"></param>
    // '' <param name="FieldName"></param>
    // '' <returns></returns>
    public string cs_getFieldCaption(int CSPointer, string FieldName) {
        string returnResult = "";
        try {
            if (csOk(CSPointer)) {
                if (contentSetStore(CSPointer).Updateable) {
                    // With...
                    if ((contentSetStore(CSPointer).CDef.Name != "")) {
                        returnResult = contentSetStore(CSPointer).CDef.fields;
                        FieldName.ToLower().caption;
                        if (string.IsNullOrEmpty(returnResult)) {
                            returnResult = FieldName;
                        }
                        
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
    // '' get a list of captions of fields within a data set
    // '' </summary>
    // '' <param name="CSPointer"></param>
    // '' <returns></returns>
    public string cs_getSelectFieldList(int CSPointer) {
        string returnResult = "";
        try {
            if (csOk(CSPointer)) {
                if (useCSReadCacheMultiRow) {
                    returnResult = Join(contentSetStore(CSPointer).fieldNames, ",");
                }
                else {
                    returnResult = contentSetStore(CSPointer).SelectTableFieldList;
                    if (string.IsNullOrEmpty(returnResult)) {
                        // With...
                        if (!(contentSetStore(CSPointer).dt == null)) {
                            if ((contentSetStore(CSPointer).dt.Columns.Count > 0)) {
                                for (FieldPointer = 0; (FieldPointer 
                                            <= (contentSetStore(CSPointer).dt.Columns.Count - 1)); FieldPointer++) {
                                    returnResult = (returnResult + ("," + contentSetStore(CSPointer).dt.Columns));
                                    FieldPointer.ColumnName;
                                }
                                
                                returnResult = returnResult.Substring(1);
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
        
        return returnResult;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' get the caption of a field within a csv_ContentSet
    // '' </summary>
    // '' <param name="CSPointer"></param>
    // '' <param name="FieldName"></param>
    // '' <returns></returns>
    public bool cs_isFieldSupported(int CSPointer, string FieldName) {
        bool returnResult = false;
        try {
            if (string.IsNullOrEmpty(FieldName)) {
                throw new ArgumentException("Field name cannot be blank");
            }
            else if (!csOk(CSPointer)) {
                throw new ArgumentException("dataset is not valid");
            }
            else {
                string CSSelectFieldList = cs_getSelectFieldList(CSPointer);
                returnResult = genericController.IsInDelimitedString(CSSelectFieldList, FieldName, ",");
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
    // '' get the filename that backs the field specified. only valid for fields of TextFile and File type.
    // '' Attempt to read the filename from the field
    // '' if no filename, attempt to create it from the tablename-recordid
    // '' if no recordid, create filename from a random
    // '' </summary>
    // '' <param name="CSPointer"></param>
    // '' <param name="FieldName"></param>
    // '' <param name="OriginalFilename"></param>
    // '' <param name="ContentName"></param>
    // '' <returns></returns>
    public string csGetFilename(int CSPointer, string FieldName, string OriginalFilename, string ContentName, void =, void , int fieldTypeId, void =, void 0) {
        string returnFilename = "";
        // Warning!!! Optional parameters not supported
        // Warning!!! Optional parameters not supported
        try {
            string TableName;
            int RecordID;
            string fieldNameUpper;
            int LenOriginalFilename;
            int LenFilename;
            int Pos;
            // 
            if (!csOk(CSPointer)) {
                throw new ArgumentException("CSPointer does Not point To a valid dataset, it Is empty, Or it Is Not pointing To a valid row.");
            }
            else if ((FieldName == "")) {
                throw new ArgumentException("Fieldname Is blank");
            }
            else {
                fieldNameUpper = genericController.vbUCase(FieldName.Trim());
                returnFilename = cs_getValue(CSPointer, fieldNameUpper);
                if ((returnFilename != "")) {
                    // 
                    //  ----- A filename came from the record
                    // 
                    if ((OriginalFilename != "")) {
                        // 
                        //  ----- there was an original filename, make sure it matches the one in the record
                        // 
                        LenOriginalFilename = OriginalFilename.Length();
                        LenFilename = returnFilename.Length();
                        Pos = (1 
                                    + (LenFilename - LenOriginalFilename));
                        if ((Pos <= 0)) {
                            // 
                            //  Original Filename changed, create a new csv_cs_getFilename
                            // 
                            returnFilename = "";
                        }
                        else if ((returnFilename.Substring((Pos - 1)) != OriginalFilename)) {
                            // 
                            //  Original Filename changed, create a new csv_cs_getFilename
                            // 
                            returnFilename = "";
                        }
                        
                    }
                    
                }
                
                if ((returnFilename == "")) {
                    // With...
                    // 
                    //  ----- no filename present, get id field
                    // 
                    if ((contentSetStore(CSPointer).ResultColumnCount > 0)) {
                        for (FieldPointer = 0; (FieldPointer 
                                    <= (contentSetStore(CSPointer).ResultColumnCount - 1)); FieldPointer++) {
                            if ((genericController.vbUCase(contentSetStore(CSPointer).fieldNames, FieldPointer) == "ID")) {
                                RecordID = csGetInteger(CSPointer, "ID");
                                break;
                            }
                            
                        }
                        
                    }
                    
                    // 
                    //  ----- Get tablename
                    // 
                    if (contentSetStore(CSPointer).Updateable) {
                        // 
                        //  Get tablename from Content Definition
                        // 
                        ContentName = contentSetStore(CSPointer).CDef.Name;
                        TableName = contentSetStore(CSPointer).CDef.ContentTableName;
                    }
                    else if ((ContentName != "")) {
                        // 
                        //  CS is SQL-based, use the contentname
                        // 
                        TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName);
                    }
                    else {
                        // 
                        //  no Contentname given
                        // 
                        throw new ApplicationException("Can Not create a filename because no ContentName was given, And the csv_ContentSet Is SQL-based.");
                    }
                    
                    // 
                    //  ----- Create filename
                    // 
                    if ((fieldTypeId == 0)) {
                        if ((ContentName == "")) {
                            if ((OriginalFilename == "")) {
                                fieldTypeId = FieldTypeIdText;
                            }
                            else {
                                fieldTypeId = FieldTypeIdFile;
                            }
                            
                        }
                        else if (contentSetStore(CSPointer).Updateable) {
                            // 
                            //  -- get from cdef
                            fieldTypeId = contentSetStore(CSPointer).CDef.fields;
                            FieldName.ToLower().fieldTypeId;
                        }
                        else {
                            // 
                            //  -- else assume text
                            if ((OriginalFilename == "")) {
                                fieldTypeId = FieldTypeIdText;
                            }
                            else {
                                fieldTypeId = FieldTypeIdFile;
                            }
                            
                        }
                        
                    }
                    
                    if ((OriginalFilename == "")) {
                        returnFilename = fileController.getVirtualRecordPathFilename(TableName, FieldName, RecordID, fieldTypeId);
                    }
                    else {
                        returnFilename = fileController.getVirtualRecordPathFilename(TableName, FieldName, RecordID, OriginalFilename);
                    }
                    
                    //  20160607 - no, if you call the cs_set, it stack-overflows. this is a get, so do not save it here.
                    // Call cs_set(CSPointer, fieldNameUpper, returnFilename)
                }
                
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return returnFilename;
    }
    
    // 
    //    csv_cs_getText
    // 
    public string csGetText(int CSPointer, string FieldName) {
        return genericController.encodeText(cs_getValue(CSPointer, FieldName));
    }
    
    // 
    //    genericController.EncodeInteger( csv_cs_getField )
    // 
    public int csGetInteger(int CSPointer, string FieldName) {
        return genericController.EncodeInteger(cs_getValue(CSPointer, FieldName));
    }
    
    // 
    //    encodeNumber( csv_cs_getField )
    // 
    public double csGetNumber(int CSPointer, string FieldName) {
        return genericController.EncodeNumber(cs_getValue(CSPointer, FieldName));
    }
    
    // 
    //     genericController.EncodeDate( csv_cs_getField )
    // 
    public DateTime csGetDate(int CSPointer, string FieldName) {
        return genericController.EncodeDate(cs_getValue(CSPointer, FieldName));
    }
    
    // 
    //    genericController.EncodeBoolean( csv_cs_getField )
    // 
    public bool csGetBoolean(int CSPointer, string FieldName) {
        return genericController.EncodeBoolean(cs_getValue(CSPointer, FieldName));
    }
    
    // 
    //    genericController.EncodeBoolean( csv_cs_getField )
    // 
    public string csGetLookup(int CSPointer, string FieldName) {
        return csGet(CSPointer, FieldName);
    }
    
    // 
    // ====================================================================================
    //  Set a csv_ContentSet Field value for a TextFile fieldtype
    //    Saves the value in a file and saves the filename in the field
    // 
    //    CSPointer   The current Content Set Pointer
    //    FieldName   The name of the field to be saved
    //    Copy        Literal string to be saved in the field
    //    ContentName Contentname for the field to be saved
    // ====================================================================================
    // 
    public void csSetTextFile(int CSPointer, string FieldName, string Copy, string ContentName) {
        try {
            if (!csOk(CSPointer)) {
                throw new ArgumentException("dataset is not valid");
            }
            else if (string.IsNullOrEmpty(FieldName)) {
                throw new ArgumentException("fieldName cannot be blank");
            }
            else if (string.IsNullOrEmpty(ContentName)) {
                throw new ArgumentException("contentName cannot be blank");
            }
            else {
                // With...
                if (!contentSetStore(CSPointer).Updateable) {
                    throw new ApplicationException("Attempting To update an unupdateable data set");
                }
                else {
                    string OldFilename = csGetText(CSPointer, FieldName);
                    string Filename = csGetFilename(CSPointer, FieldName, "", ContentName, FieldTypeIdFileText);
                    if ((OldFilename != Filename)) {
                        // 
                        //  Filename changed, mark record changed
                        // 
                        cpCore.cdnFiles.saveFile(Filename, Copy);
                        csSet(CSPointer, FieldName, Filename);
                    }
                    else {
                        string OldCopy = cpCore.cdnFiles.readFile(Filename);
                        if ((OldCopy != Copy)) {
                            // 
                            //  copy changed, mark record changed
                            // 
                            cpCore.cdnFiles.saveFile(Filename, Copy);
                            csSet(CSPointer, FieldName, Filename);
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
    // ========================================================================
    // '' <summary>
    // '' ContentServer version of getDataRowColumnName
    // '' </summary>
    // '' <param name="dr"></param>
    // '' <param name="FieldName"></param>
    // '' <returns></returns>
    public string getDataRowColumnName(DataRow dr, string FieldName) {
        string result = "";
        try {
            if (string.IsNullOrEmpty(FieldName)) {
                throw new ArgumentException("fieldname cannot be blank");
            }
            else {
                result = dr.Item[FieldName].ToString;
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return result;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' InsertContentRecordGetID
    // '' Inserts a record based on a content definition.
    // '' Returns the ID of the record, -1 if error
    // '' </summary>
    // '' <param name="ContentName"></param>
    // '' <param name="MemberID"></param>
    // '' <returns></returns>
    // ''
    public int insertContentRecordGetID(string ContentName, int MemberID) {
        int result = -1;
        try {
            int CS = csInsertRecord(ContentName, MemberID);
            if (!csOk(CS)) {
                csClose(CS);
                throw new ApplicationException(("could not insert record in content [" 
                                + (ContentName + "]")));
            }
            else {
                result = csGetInteger(CS, "ID");
            }
            
            csClose(CS);
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
        return result;
    }
    
    // 
    // ========================================================================
    // '' <summary>
    // '' Delete Content Record
    // '' </summary>
    // '' <param name="ContentName"></param>
    // '' <param name="RecordID"></param>
    // '' <param name="MemberID"></param>
    // 
    public void deleteContentRecord(string ContentName, int RecordID, int MemberID, void =, void SystemMemberID) {
        try {
            if (string.IsNullOrEmpty(ContentName)) {
                throw new ArgumentException("contentname cannot be blank");
                // Warning!!! Optional parameters not supported
            }
            else if ((RecordID <= 0)) {
                throw new ArgumentException("recordId must be positive value");
            }
            else {
                int CSPointer = cs_openContentRecord(ContentName, RecordID, MemberID, true, true);
                if (csOk(CSPointer)) {
                    csDeleteRecord(CSPointer);
                }
                
                csClose(CSPointer);
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
    // '' 'deleteContentRecords
    // '' </summary>
    // '' <param name="ContentName"></param>
    // '' <param name="Criteria"></param>
    // '' <param name="MemberID"></param>
    // 
    public void deleteContentRecords(string ContentName, string Criteria, int MemberID, void =, void 0) {
        try {
            // 
            // Warning!!! Optional parameters not supported
            int CSPointer;
            Models.Complex.cdefModel CDef;
            // 
            if (string.IsNullOrEmpty(ContentName.Trim())) {
                throw new ArgumentException("contentName cannot be blank");
            }
            else if (string.IsNullOrEmpty(Criteria.Trim())) {
                throw new ArgumentException("criteria cannot be blank");
            }
            else {
                CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
                if ((CDef == null)) {
                    throw new ArgumentException(("ContentName [" 
                                    + (ContentName + "] was Not found")));
                }
                else if ((CDef.Id == 0)) {
                    throw new ArgumentException(("ContentName [" 
                                    + (ContentName + "] was Not found")));
                }
                else {
                    // 
                    //  -- treat all deletes one at a time to invalidate the primary cache
                    //  another option is invalidate the entire table (tablename-invalidate), but this also has performance problems
                    // 
                    List<string> invaldiateObjectList = new List<string>();
                    CSPointer = csOpen(ContentName, Criteria, ,, false, MemberID, true, true);
                    while (csOk(CSPointer)) {
                        invaldiateObjectList.Add(Controllers.cacheController.getCacheKey_Entity(CDef.ContentTableName, "id", csGetInteger(CSPointer, "id").ToString()));
                        csDeleteRecord(CSPointer);
                        csGoNext(CSPointer);
                    }
                    
                    csClose(CSPointer);
                    cpCore.cache.invalidateContent(invaldiateObjectList);
                    //     ElseIf cpCore.siteProperties.allowWorkflowAuthoring And (false) Then
                    //     '
                    //     ' Supports Workflow Authoring, handle it record at a time
                    //     '
                    //     CSPointer = cs_open(ContentName, Criteria, , False, MemberID, True, True, "ID")
                    //     Do While cs_ok(CSPointer)
                    //         Call cs_deleteRecord(CSPointer)
                    //         Call cs_goNext(CSPointer)
                    //     Loop
                    //     Call cs_Close(CSPointer)
                    // Else
                    //     '
                    //     ' No Workflow Authoring, just delete records
                    //     '
                    //     Call DeleteTableRecords(CDef.ContentTableName, "(" & Criteria & ") And (" & CDef.ContentControlCriteria & ")", CDef.ContentDataSourceName)
                    //     If coreWorkflowClass.csv_AllowAutocsv_ClearContentTimeStamp Then
                    //         Call cpCore.cache.invalidateObject(CDef.ContentTableName & "-invalidate")
                    //     End If
                }
                
            }
            
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
    }
    
    protected bool disposed = false;
    
    protected virtual void Dispose(bool disposing) {
        if (!this.disposed) {
            if (disposing) {
                // 
                //  ----- call .dispose for managed objects
                // 
                // 
                //  ----- Close all open csv_ContentSets, and make sure the RS is killed
                // 
                if ((contentSetStoreCount > 0)) {
                    int CSPointer;
                    for (CSPointer = 1; (CSPointer <= contentSetStoreCount); CSPointer++) {
                        if (contentSetStore(CSPointer).IsOpen) {
                            csClose(CSPointer);
                        }
                        
                    }
                    
                }
                
            }
            
            // 
            //  Add code here to release the unmanaged resource.
            // 
        }
        
        this.disposed = true;
    }
    
    //  Do not change or add Overridable to these methods.
    //  Put cleanup code in Dispose(ByVal disposing As Boolean).
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected override void Finalize() {
        Dispose(false);
        base.Finalize();
    }