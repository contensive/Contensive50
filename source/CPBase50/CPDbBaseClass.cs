
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
		public abstract void Delete(string DataSourcename, string TableName, int RecordId);
		public abstract string GetConnectionString(string DataSourcename);
		public abstract int GetDataSourceType(string DataSourcename);
		public abstract int GetTableID(string TableName);
		public abstract bool IsTable(string DataSourcename, string TableName);
		public abstract bool IsTableField(string DataSourcename, string TableName, string FieldName);
		public abstract string EncodeSQLBoolean(bool SourceBoolean);
		public abstract string EncodeSQLDate(DateTime SourceDate);
		public abstract string EncodeSQLNumber(double SourceNumber);
		public abstract string EncodeSQLText(string SourceText);
        [Obsolete("deprecated. Convert to datatables and use executeQuery(), executeNonQuery(), or executeNonQueryAsync()", false)]
        public abstract object ExecuteSQL(string SQL, string DataSourcename = "Default", string Retries = "0", string PageSize = "10", string PageNumber = "1");
		//
		// need: executeQuery() returns dataTable
		// need: executeNonQuery() returns recordsAffected
		// need: executeNonQueryAsync() returns nothing
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
		public abstract int SQLTimeout {get; set;}
		public abstract string GetRemoteQueryKey(string sql, string DataSourceName = "Default", int pageSize = 100);
        //
        // deprecated
        //
        [Obsolete("Use GetConnectionString( dataSourceName )")]
        public abstract string DbGetConnectionString(string DataSourcename);
        [Obsolete("Use GetDataSourceType( dataSourceName )")]
        public abstract int DbGetDataSourceType(string DataSourcename);
        [Obsolete("Use GetTableId instead.", true)]
        public abstract int DbGetTableID(string TableName);
        [Obsolete("Use isTable instead", true)]
        public abstract bool DbIsTable(string DataSourcename, string TableName);
        [Obsolete("Use isTableField instead", true)]
        public abstract bool DbIsTableField(string DataSourcename, string TableName, string FieldName);
	}

}

