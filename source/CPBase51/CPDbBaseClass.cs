
using System;
using System.Data;

namespace Contensive.BaseClasses {
    /// <summary>
    /// CP.Db - This object references the database directly
    /// </summary>
    /// <remarks></remarks>
    public abstract class CPDbBaseClass : IDisposable {
        //
        //====================================================================================================
        /// <summary>
        /// Delete the record specified by tablename and recordId
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="recordId"></param>
        public abstract void Delete(string tableName, int recordId);
        //
        //====================================================================================================
        /// <summary>
        /// get the connection string for his 
        /// </summary>
        /// <returns></returns>
        public abstract string GetConnectionString();
        //
        //====================================================================================================
        /// <summary>
        /// Return true if this is a valid database table in the current application
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public abstract bool IsTable(string tableName);
        //
        //====================================================================================================
        /// <summary>
        /// Return true if this is a valid database field in a valid table in the current application
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public abstract bool IsTableField(string tableName, string fieldName);
        //
        //====================================================================================================
        /// <summary>
        /// Encode a boolean value to be used in an sql Query for this application. Boolean fields are stored as integers in Contensive. Example 'select id from ccmembers where active='+EncodeSqlBoolean(true)
        /// </summary>
        /// <param name="sourceBoolean"></param>
        /// <returns></returns>
        public abstract string EncodeSQLBoolean(bool sourceBoolean);
        //
        //====================================================================================================
        /// <summary>
        /// Encode a date value to be used in an sql Query for this application. Example 'select id from ccmembers where dateadded>'+EncodeSqlDate( myBirthday )
        /// </summary>
        /// <param name="sourceDate"></param>
        /// <returns></returns>
		public abstract string EncodeSQLDate(DateTime sourceDate);
        //
        //====================================================================================================
        /// <summary>
        /// Encode a numeric value (integer or double) to be used in an sql Query for this application. Example 'select id from orders where totalAmount>'+EncodeSqlNumber( 1000.00 )
        /// </summary>
        /// <param name="sourceNumber"></param>
        /// <returns></returns>
		public abstract string EncodeSQLNumber(double sourceNumber);
        //
        //====================================================================================================
        /// <summary>
        /// Encode a numeric value (integer or double) to be used in an sql Query for this application. Example 'select id where id>'+EncodeSqlNumber( 1000 )
        /// </summary>
        /// <param name="sourceNumber"></param>
        /// <returns></returns>
        public abstract string EncodeSQLNumber(int sourceNumber);
        //
        //====================================================================================================
        /// <summary>
        /// Encode a text string value to be used in an sql Query for this application. Example 'select id where name='+EncodeSqlText( 'bob' )
        /// </summary>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        public abstract string EncodeSQLText(string sourceText);
        //
        //====================================================================================================
        /// <summary>
        /// Execute a query and return a datatable.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="startRecord"></param>
        /// <param name="maxRecords"></param>
        /// <returns></returns>
        public abstract DataTable ExecuteQuery(string sql, int startRecord, int maxRecords);
        //
        //====================================================================================================
        /// <summary>
        /// Execute an sql query and return a datatable.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="startRecord"></param>
        /// <returns></returns>
        public abstract DataTable ExecuteQuery(string sql, int startRecord);
        //
        //====================================================================================================
        /// <summary>
        /// Execute an sql query and return a datatable.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public abstract DataTable ExecuteQuery(string sql);
        //
        //====================================================================================================
        /// <summary>
        /// Execute an sql command on a specific datasource. No data is returned.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="recordsAffected"></param>
        public abstract void ExecuteNonQuery(string sql, ref int recordsAffected);
        //
        //====================================================================================================
        /// <summary>
        /// Execute an sql command on a specific datasource. No data is returned.
        /// </summary>
        /// <param name="sql"></param>
        public abstract void ExecuteNonQuery(string sql);
        //
        //====================================================================================================
        /// <summary>
        /// Execute an sql command on a specific datasource, returning immediately.
        /// </summary>
        /// <param name="sql"></param>
        public abstract void ExecuteNonQueryAsync(string sql);
        //
        //====================================================================================================
        /// <summary>
        /// get or set the timeout for all Db methods in the current process
        /// </summary>
        public abstract int SQLTimeout { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// To give a remote method the ability to run an arbitrary query, store it as a remote query and executeRemoteQuery()
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="DataSourceName"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public abstract string GetRemoteQueryKey(string sql, int pageSize);
        //
        public abstract string GetRemoteQueryKey(string sql);
        //
        //====================================================================================================
        /// <summary>
        /// Execute a sql query stored with getRemoteQueryKey.
        /// </summary>
        /// <param name="remoteQueryKey"></param>
        /// <returns></returns>
        public abstract DataTable ExecuteRemoteQuery(string remoteQueryKey);
        //
        //====================================================================================================
        /// <summary>
        /// Support disposable for non-default datasources
        /// </summary>
        public abstract void Dispose();
        //
        //====================================================================================================
        // deprecated
        //
        [Obsolete("Convert to datatables and use executeQuery(), executeNonQuery(), or executeNonQueryAsync()", true)]
        public abstract object ExecuteSQL(string SQL, string DataSourcename, string Retries, string PageSize, string PageNumber);
        //
        [Obsolete("Convert to datatables and use executeQuery(), executeNonQuery(), or executeNonQueryAsync()", true)]
        public abstract object ExecuteSQL(string SQL, string DataSourcename, string Retries, string PageSize);
        //
        [Obsolete("Convert to datatables and use executeQuery(), executeNonQuery(), or executeNonQueryAsync()", true)]
        public abstract object ExecuteSQL(string SQL, string DataSourcename, string Retries);
        //
        [Obsolete("Convert to datatables and use executeQuery(), executeNonQuery(), or executeNonQueryAsync()", true)]
        public abstract object ExecuteSQL(string SQL, string DataSourcename);
        //
        [Obsolete("Convert to datatables and use executeQuery(), executeNonQuery(), or executeNonQueryAsync()", true)]
        public abstract object ExecuteSQL(string SQL);
        //
        [Obsolete("Use GetConnectionString( dataSourceName )")]
        public abstract string DbGetConnectionString(string DataSourcename);
        //
        [Obsolete("Use GetDataSourceType( dataSourceName )")]
        public abstract int DbGetDataSourceType(string DataSourcename);
        //
        [Obsolete("Use GetTableId instead.", true)]
        public abstract int DbGetTableID(string TableName);
        //
        [Obsolete("Use isTable instead", true)]
        public abstract bool DbIsTable(string DataSourcename, string TableName);
        //
        [Obsolete("Use isTableField instead", true)]
        public abstract bool DbIsTableField(string DataSourcename, string TableName, string FieldName);
        //
        [Obsolete("Deprecated. Use CP.Content.GetTableId().", true)]
        public abstract int GetTableID(string tableName);
        //
        [Obsolete("Only Sql Server currently supported", true)]
        public abstract int GetDataSourceType(string DataSourcename);
        //
		[Obsolete("Deprecated. Use methods without datasource reference. Instead create non-default datasource with CP.DbNew(datasourcename).", false)]
        public abstract void Delete(string dataSourcename, string tableName, int recordId);
        //
        [Obsolete("Deprecated. Use methods without datasource.", false)]
        public abstract string GetConnectionString(string dataSourcename);
        //
        [Obsolete("Deprecated. Use methods without datasource.", false)]
        public abstract bool IsTable(string dataSourcename, string tableName);
        //
        [Obsolete("Deprecated. Use methods without datasource.", false)]
        public abstract bool IsTableField(string dataSourcename, string tableName, string fieldName);
        //
        [Obsolete("Deprecated. Use methods without datasource.", false)]
        public abstract DataTable ExecuteQuery(string sql, string dataSourceName, int startRecord, int maxRecords);
        //
        [Obsolete("Deprecated. Use methods without datasource.", false)]
        public abstract DataTable ExecuteQuery(string sql, string dataSourceName, int startRecord);
        //
        [Obsolete("Deprecated. Use methods without datasource.", false)]
        public abstract DataTable ExecuteQuery(string sql, string dataSourceName);
        //
        [Obsolete("Deprecated. Use methods without datasource.", false)]
        public abstract void ExecuteNonQuery(string sql, string dataSourceName, ref int recordsAffected);
        //
        [Obsolete("Deprecated. Use methods without datasource.", false)]
        public abstract void ExecuteNonQuery(string sql, string dataSourceName);
        //
        [Obsolete("Deprecated. Use methods without datasource.", false)]
        public abstract void ExecuteNonQueryAsync(string sql, string dataSourceName);
        //
        [Obsolete("Deprecated. Use methods without datasource.", false)]
        public abstract string GetRemoteQueryKey(string sql, string DataSourceName, int pageSize);
        //
        [Obsolete("Deprecated. Use methods without datasource.", false)]
        public abstract string GetRemoteQueryKey(string sql, string DataSourceName);
    }

}

