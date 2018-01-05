
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core {
    public class CPDbClass : BaseClasses.CPDbBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "13B707F9-190A-4ECD-8A53-49FE1D8CAE54";
        public const string InterfaceId = "A20CF53D-1FB7-41C4-8493-3D5AC3671A5E";
        public const string EventsId = "F68D64F0-595F-4CA2-9EC6-6DBA9A7E458B";
        #endregion
        //
        private CPClass cp;
        //
        public enum remoteQueryType {
            sql = 1,
            openContent = 2,
            updateContent = 3,
            insertContent = 4
        }
        //
        //====================================================================================================
        /// <summary>
        /// Construct
        /// </summary>
        /// <param name="cpParent"></param>
        public CPDbClass(CPClass cpParent) : base() {
            cp = cpParent;
        }
        //
        //====================================================================================================
        //
        private void reportClassError(Exception ex, string methodName) {
            try {
                cp.core.handleException(ex, "Unexpected Trap Error in CP.DB." + methodName);
            } catch (Exception exInner) {
                //
            }
        }
        //
        //====================================================================================================
        //
        public override void Delete(string DataSourcename, string TableName, int RecordId) {
            cp.core.db.deleteTableRecord(TableName, RecordId, DataSourcename);
        }
        //
        //====================================================================================================
        //
        public override string GetConnectionString(string DataSourcename) {
            return cp.core.db.getConnectionStringADONET(cp.core.serverConfig.appConfig.name, DataSourcename);
        }
        //
        //====================================================================================================
        // deprecated
        //
        [Obsolete("Use GetConnectionString( dataSourceName )")]
        public override string DbGetConnectionString(string DataSourcename) {
            return GetConnectionString(DataSourcename);
        }
        //
        //====================================================================================================
        //
        public override int GetDataSourceType(string DataSourcename) {
            return cp.core.db.getDataSourceType(DataSourcename);
        }
        //
        //====================================================================================================
        // deprecated
        //
        [Obsolete("Use GetDataSourceType( dataSourceName )")]
        public override int DbGetDataSourceType(string DataSourcename) {
            return GetDataSourceType(DataSourcename);
        }
        //
        //====================================================================================================
        //
        public override int GetTableID(string TableName) {
            return cp.core.db.GetTableID(TableName);
        }
        //
        //====================================================================================================
        // deprecated 
        //
        [Obsolete("Use GetTableId instead.", true)]
        public override int DbGetTableID(string TableName) {
            return GetTableID(TableName);
        }
        //
        //====================================================================================================
        //
        public override bool IsTable(string DataSourcename, string TableName) {
            return cp.core.db.isSQLTable(DataSourcename, TableName);
        }
        //
        //====================================================================================================
        // deprecated
        //
        [Obsolete("Use isTable instead", true)]
        public override bool DbIsTable(string DataSourcename, string TableName) {
            return IsTable(DataSourcename, TableName);
        }
        //
        //====================================================================================================
        //
        public override bool IsTableField(string DataSourcename, string TableName, string FieldName) {
            return cp.core.db.isSQLTableField(DataSourcename, TableName, FieldName);
        }
        //
        //====================================================================================================
        // deprecated 
        //
        [Obsolete("Use isTableField instead", true)]
        public override bool DbIsTableField(string DataSourcename, string TableName, string FieldName) {
            return IsTableField(DataSourcename, TableName, FieldName);
        }
        //
        //====================================================================================================
        //
        public override string EncodeSQLBoolean(bool SourceBoolean) {
            return cp.core.db.encodeSQLBoolean(SourceBoolean);
        }
        //
        //====================================================================================================
        //
        public override string EncodeSQLDate(DateTime SourceDate) {
            return cp.core.db.encodeSQLDate(SourceDate);
        }
        //
        //====================================================================================================
        //
        public override string EncodeSQLNumber(double SourceNumber) {
            return cp.core.db.encodeSQLNumber(SourceNumber);
        }
        //
        //====================================================================================================
        //
        public override string EncodeSQLText(string SourceText) {
            return cp.core.db.encodeSQLText(SourceText);
        }
        //
        //====================================================================================================
        /// <summary>
        /// process a sql query to be executed remotely. Returns a key that is used in the future.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="DataSourceName"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public override string GetRemoteQueryKey(string sql, string DataSourceName = "Default", int pageSize = 100) {
            string returnKey = "";
            try {
                Contensive.BaseClasses.CPCSBaseClass cs = cp.CSNew();
                int dataSourceID = 0;
                //
                if (pageSize == 0) {
                    pageSize = 9999;
                }
                if (cs.Insert("Remote Queries")) {
                    returnKey = Guid.NewGuid().ToString();
                    dataSourceID = cp.Content.GetRecordID("Data Sources", DataSourceName);
                    cs.SetField("remotekey", returnKey);
                    cs.SetField("datasourceid", dataSourceID.ToString());
                    cs.SetField("sqlquery", sql);
                    cs.SetField("maxRows", pageSize.ToString());
                    cs.SetField("dateexpires", DateTime.Now.AddDays(1).ToString());
                    cs.SetField("QueryTypeID", remoteQueryType.sql.ToString());
                    cs.SetField("VisitID", cp.Visit.Id.ToString());
                }
                cs.Close();
                //
            } catch (Exception ex) {
                cp.core.handleException(ex);
            }
            return returnKey;
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute a sql query and return a recordset
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="DataSourcename"></param>
        /// <param name="Retries"></param>
        /// <param name="PageSize"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        [Obsolete("deprecated. Convert to datatables and use executeQuery(), executeNonQuery(), or executeNonQueryAsync()", false)]
        public override object ExecuteSQL(string SQL, string DataSourcename = "Default", string Retries = "0", string PageSize = "10", string PageNumber = "1") {
            return cp.core.db.executeSql_getRecordSet(SQL, DataSourcename, Controllers.genericController.encodeInteger(PageNumber) * Controllers.genericController.encodeInteger(PageSize), Controllers.genericController.encodeInteger(PageSize));
        }
        //
        //====================================================================================================
        /// <summary>
        /// Execute sql or command on the default datasource and return all rows in a datatable.
        /// </summary>
        /// <param name="SQL"></param>
        /// <returns></returns>
        //Public Overrides Function ExecuteSQL_GetDataTable(SQL As String) As DataTable
        //    Return cp.core.db.executeQuery(SQL)
        //End Function
        //
        //====================================================================================================
        // <summary>
        // Execute sql or command on the default datasource and return MaxRows in a datatable.
        // </summary>
        // <param name="SQL"></param>
        // <param name="MaxRows"></param>
        // <returns></returns>
        //Public Overrides Function ExecuteSQL_GetDataTable(SQL As String, MaxRows As Integer) As DataTable
        //    Return cp.core.db.executeQuery(SQL, "", 0, MaxRows)
        //End Function
        //
        //====================================================================================================
        // <summary>
        // Execute sql or command on a specific datasource and return all rows in a datatable. The default datasource is either "", or "default"
        // </summary>
        // <param name="SQL"></param>
        // <param name="DataSourcename"></param>
        // <returns></returns>
        //Public Overrides Function ExecuteSQL_GetDataTable(SQL As String, DataSourcename As String) As DataTable
        //    Return cp.core.db.executeQuery(SQL, DataSourcename)
        //End Function
        //
        //====================================================================================================
        // <summary>
        // Execute sql or command on a specific datasource and return MaxRows starting on page PageNumber (0 based) as a datatable.
        // </summary>
        // <param name="SQL"></param>
        // <param name="DataSourcename"></param>
        // <param name="PageSize"></param>
        // <param name="PageNumber"></param>
        // <returns></returns>
        //Public Overrides Function ExecuteSQL_GetDataTable(SQL As String, DataSourcename As String, MaxRows As Integer, PageSize As Integer, PageNumber As Integer) As DataTable
        //    Return cp.core.db.executeQuery(SQL, "", (PageSize * PageNumber), MaxRows)
        //End Function
        //
        //====================================================================================================
        // <summary>
        // Execute sql or command on the default datasource and return all rows started on page pageNumber (0 based) as a datatable. Limit the number of rows within the query. The default datasource is either "", or "default"
        // </summary>
        // <param name="SQL"></param>
        // <param name="PageSize"></param>
        // <param name="PageNumber"></param>
        // <returns></returns>
        //Public Overrides Function ExecuteSQL_GetDataTable(SQL As String, MaxRows As Integer, PageSize As Integer, PageNumber As Integer) As DataTable
        //    Throw New NotImplementedException()
        //End Function
        //
        //====================================================================================================
        // <summary>
        // Execute sql or command on the default datasource and return all rows a recordset (ADODB).
        // </summary>
        // <param name="SQL"></param>
        // <returns></returns>
        //Public Overrides Function ExecuteSQL_GetRecordSet(SQL As String) As Recordset
        //    Throw New NotImplementedException()
        //End Function
        //
        //====================================================================================================
        // <summary>
        // Execute sql or command on the default datasource and return MaxRows in a recordset (ADODB)
        // </summary>
        // <param name="SQL"></param>
        // <param name="MaxRows"></param>
        // <returns></returns>
        //Public Overrides Function ExecuteSQL_GetRecordSet(SQL As String, MaxRows As Integer) As Recordset
        //    Throw New NotImplementedException()
        //End Function
        //
        //====================================================================================================
        // <summary>
        // Execute sql or command on a sepcific datasource and a recordset (ADODB). The default datasource is either "", or "default"
        // </summary>
        // <param name="SQL"></param>
        // <param name="DataSourcename"></param>
        // <returns></returns>
        //Public Overrides Function ExecuteSQL_GetRecordSet(SQL As String, DataSourcename As String) As Recordset
        //    Return cp.core.db.executeSql_getRecordSet(SQL, DataSourcename, 0, -1)
        //End Function
        //
        //====================================================================================================
        // <summary>
        // Execute sql or command on the default datasource and return all rows started on page pageNumber (0 based) as a datatable. Limit the number of rows within the query. The default datasource is either "", or "default"
        // </summary>
        // <param name="SQL"></param>
        // <param name="MaxRows"></param>
        // <param name="PageSize"></param>
        // <param name="PageNumber"></param>
        // <returns></returns>
        //Public Overrides Function ExecuteSQL_GetRecordSet(SQL As String, MaxRows As Integer, PageSize As Integer, PageNumber As Integer) As Recordset
        //    Return cp.core.db.executeSql_getRecordSet(SQL, "", (PageSize * PageNumber), MaxRows)
        //End Function
        //
        //====================================================================================================
        // <summary>
        // Execute sql or command on a specific datasource and return all rows started on page pageNumber (0 based) as a datatable. Limit the number of rows within the query. The default datasource is either "", or "default"
        // </summary>
        // <param name="SQL"></param>
        // <param name="DataSourcename"></param>
        // <param name="PageSize"></param>
        // <param name="PageNumber"></param>
        // <returns></returns>
        //Public Overrides Function ExecuteSQL_GetRecordSet(SQL As String, DataSourcename As String, MaxRows As Integer, PageSize As Integer, PageNumber As Integer) As Recordset
        //    Return cp.core.db.executeSql_getRecordSet(SQL, DataSourcename, (PageSize * PageNumber), MaxRows)
        //End Function
        //
        //====================================================================================================
        //
        public override int SQLTimeout {
            get {
                return cp.core.db.sqlCommandTimeout;
            }
            set {
                cp.core.db.sqlCommandTimeout = value;
            }
        }
        //
        //====================================================================================================
        //
        private void appendDebugLog(string copy) {
            //My.Computer.FileSystem.WriteAllText("c:\clibCpDbDebug.log", Now & " - cp.db, " & copy & vbCrLf, True)
        }
        //
        //
        //
        #region  IDisposable Support 
        //
        protected bool disposed = false;
        //====================================================================================================
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                appendDebugLog(".dispose, dereference main, csv");
                if (disposing) {
                    //
                    // call .dispose for managed objects
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
        ~CPDbClass() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
        //
        //====================================================================================================
        // -- version 4.1
        //
        //Public MustOverride Sub Delete(ByVal DataSourcename As String, ByVal TableName As String, ByVal RecordId As Integer)
        //Public MustOverride Function GetConnectionString(ByVal DataSourcename As String) As String
        //Public MustOverride Function GetDataSourceType(ByVal DataSourcename As String) As Integer
        //Public MustOverride Function GetTableID(ByVal TableName As String) As Integer
        //Public MustOverride Function IsTable(ByVal DataSourcename As String, ByVal TableName As String) As Boolean
        //Public MustOverride Function IsTableField(ByVal DataSourcename As String, ByVal TableName As String, ByVal FieldName As String) As Boolean
        //Public MustOverride Function EncodeSQLBoolean(ByVal SourceBoolean As Boolean) As String
        //Public MustOverride Function EncodeSQLDate(ByVal SourceDate As Date) As String
        //Public MustOverride Function EncodeSQLNumber(ByVal SourceNumber As Double) As String
        //Public MustOverride Function EncodeSQLText(ByVal SourceText As String) As String
        //Public MustOverride Function ExecuteSQL(ByVal SQL As String, Optional ByVal DataSourcename As String = "Default", Optional ByVal Retries As String = "0", Optional ByVal PageSize As String = "10", Optional ByVal PageNumber As String = "1") As Object
        //Public MustOverride Property SQLTimeout() As Integer
        //Public MustOverride Function GetRemoteQueryKey(ByVal sql As String, Optional ByVal DataSourceName As String = "Default", Optional ByVal pageSize As Integer = 100) As String
        //
        // deprecated
        //
        //Public MustOverride Function DbGetConnectionString(ByVal DataSourcename As String) As String
        //Public MustOverride Function DbGetDataSourceType(ByVal DataSourcename As String) As Integer
        //Public MustOverride Function DbGetTableID(ByVal TableName As String) As Integer
        //Public MustOverride Function DbIsTable(ByVal DataSourcename As String, ByVal TableName As String) As Boolean
        //Public MustOverride Function DbIsTableField(ByVal DataSourcename As String, ByVal TableName As String, ByVal FieldName As String) As Boolean





    }
}