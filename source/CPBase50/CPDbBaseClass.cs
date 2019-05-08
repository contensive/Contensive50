
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using System.Data;
//
// documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
//
namespace Contensive.BaseClasses
{
	/// <summary>
	/// CP.Db - This object references the database directly
	/// </summary>
	/// <remarks></remarks>
	public abstract class CPDbBaseClass
	{
        /// <summary>
        /// Delete the record specified by tablename and recordId
        /// </summary>
        /// <param name="DataSourcename"></param>
        /// <param name="TableName"></param>
        /// <param name="RecordId"></param>
		public abstract void Delete(string DataSourcename, string TableName, int RecordId);
        /// <summary>
        /// Delete the record specified by tablename and recordId
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="RecordId"></param>
        public abstract void Delete(string TableName, int RecordId);
        /// <summary>
        /// get the connection string for his 
        /// </summary>
        /// <param name="DataSourcename"></param>
        /// <returns></returns>
        public abstract string GetConnectionString(string DataSourcename);
        /// <summary>
        /// Return the recordId in the ccTables table for this table
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
		public abstract int GetTableID(string TableName);
        /// <summary>
        /// Return true if this is a valid database table in the current application
        /// </summary>
        /// <param name="DataSourcename"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
		public abstract bool IsTable(string DataSourcename, string TableName);
        /// <summary>
        /// Return true if this is a valid database table in the current application
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public abstract bool IsTable(string TableName);
        /// <summary>
        /// Return true if this is a valid database field in a valid table in the current application
        /// </summary>
        /// <param name="DataSourcename"></param>
        /// <param name="TableName"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public abstract bool IsTableField(string DataSourcename, string TableName, string FieldName);
        /// <summary>
        /// Return true if this is a valid database field in a valid table in the current application
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public abstract bool IsTableField(string TableName, string FieldName);
        /// <summary>
        /// Encode a boolean value to be used in an sql Query for this application. Boolean fields are stored as integers in Contensive. Example 'select id from ccmembers where active='+EncodeSqlBoolean(true)
        /// </summary>
        /// <param name="SourceBoolean"></param>
        /// <returns></returns>
        public abstract string EncodeSQLBoolean(bool SourceBoolean);
        /// <summary>
        /// Encode a date value to be used in an sql Query for this application. Example 'select id from ccmembers where dateadded>'+EncodeSqlDate( myBirthday )
        /// </summary>
        /// <param name="SourceDate"></param>
        /// <returns></returns>
		public abstract string EncodeSQLDate(DateTime SourceDate);
        /// <summary>
        /// Encode a numeric value (integer or double) to be used in an sql Query for this application. Example 'select id from orders where totalAmount>'+EncodeSqlNumber( 1000.00 )
        /// </summary>
        /// <param name="SourceNumber"></param>
        /// <returns></returns>
		public abstract string EncodeSQLNumber(double SourceNumber);
        /// <summary>
        /// Encode a numeric value (integer or double) to be used in an sql Query for this application. Example 'select id where id>'+EncodeSqlNumber( 1000 )
        /// </summary>
        /// <param name="SourceNumber"></param>
        /// <returns></returns>
        public abstract string EncodeSQLNumber(int SourceNumber);
        /// <summary>
        /// Encode a text string value to be used in an sql Query for this application. Example 'select id where name='+EncodeSqlText( 'bob' )
        /// </summary>
        /// <param name="SourceText"></param>
        /// <returns></returns>
        public abstract string EncodeSQLText(string SourceText);
        /// <summary>
        /// Execute a query and return a datatable.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="startRecord"></param>
        /// <param name="maxRecords"></param>
        /// <returns></returns>
        public abstract DataTable ExecuteQuery(string sql, string dataSourceName, int startRecord, int maxRecords);
        /// <summary>
        /// Execute an sql query and return a datatable.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="startRecord"></param>
        /// <returns></returns>
        public abstract DataTable ExecuteQuery(string sql, string dataSourceName, int startRecord);
        /// <summary>
        /// Execute an sql query and return a datatable.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        public abstract DataTable ExecuteQuery(string sql, string dataSourceName);
        /// <summary>
        /// Execute an sql query and return a datatable.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public abstract DataTable ExecuteQuery(string sql);
        /// <summary>
        /// Execute an sql command on a specific datasource. No data is returned.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        /// <param name="recordsAffected"></param>
        public abstract void ExecuteNonQuery(string sql, string dataSourceName, ref int recordsAffected);
        /// <summary>
        /// Execute an sql command on a specific datasource. No data is returned.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        public abstract void ExecuteNonQuery(string sql, string dataSourceName);
        /// <summary>
        /// Execute an sql command on a specific datasource. No data is returned.
        /// </summary>
        /// <param name="sql"></param>
        public abstract void ExecuteNonQuery(string sql);
        /// <summary>
        /// Execute an sql command on a specific datasource, returning immediately.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dataSourceName"></param>
        public abstract void ExecuteNonQueryAsync(string sql, string dataSourceName);
        /// <summary>
        /// Execute an sql command on a specific datasource, returning immediately.
        /// </summary>
        /// <param name="sql"></param>
        public abstract void ExecuteNonQueryAsync(string sql);
        //
        // deprecated
        //
        [Obsolete("deprecated. Executes non-query calls, but no longer returns a recordset. Convert to datatables and use executeQuery(), executeNonQuery(), or executeNonQueryAsync()", false)]
        public abstract object ExecuteSQL(string SQL, string DataSourcename = "Default", string Retries = "0", string PageSize = "10", string PageNumber = "1");
        //
		public abstract int SQLTimeout {get; set;}
        //
		public abstract string GetRemoteQueryKey(string sql, string DataSourceName = "Default", int pageSize = 100);
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
        [Obsolete("Only Sql Server currently supported", true)]
        public abstract int GetDataSourceType(string DataSourcename);
        //
        // never implemented
        //
        //Public MustOverride Function ExecuteSQL_GetRecordSet(ByVal SQL As String) As ADODB.Recordset
        //Public MustOverride Function ExecuteSQL_GetRecordSet(ByVal SQL As String, ByVal MaxRows As Integer) As ADODB.Recordset
        //Public MustOverride Function ExecuteSQL_GetRecordSet(ByVal SQL As String, ByVal MaxRows As Integer, ByVal PageSize As Integer, ByVal PageNumber As Integer) As ADODB.Recordset
        //Public MustOverride Function ExecuteSQL_GetRecordSet(ByVal SQL As String, ByVal DataSourcename As String) As ADODB.Recordset
        //Public MustOverride Function ExecuteSQL_GetRecordSet(ByVal SQL As String, ByVal DataSourcename As String, ByVal MaxRows As Integer, ByVal PageSize As Integer, ByVal PageNumber As Integer) As ADODB.Recordset
        //Public MustOverride Function ExecuteSQL_GetDataTable(ByVal SQL As String) As DataTable
        //Public MustOverride Function ExecuteSQL_GetDataTable(ByVal SQL As String, ByVal MaxRows As Integer) As DataTable
        //Public MustOverride Function ExecuteSQL_GetDataTable(ByVal SQL As String, ByVal MaxRows As Integer, PageSize As Integer, ByVal PageNumber As Integer) As DataTable
        //Public MustOverride Function ExecuteSQL_GetDataTable(ByVal SQL As String, ByVal DataSourcename As String) As DataTable
        //Public MustOverride Function ExecuteSQL_GetDataTable(ByVal SQL As String, ByVal DataSourcename As String, ByVal MaxRows As Integer, ByVal PageSize As Integer, ByVal PageNumber As Integer) As DataTable
    }

}

