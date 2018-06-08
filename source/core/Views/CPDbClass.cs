
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor {
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
                logController.handleError(cp.core,ex, "Unexpected Trap Error in CP.DB." + methodName);
            } catch (Exception) {}
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
            return cp.core.db.getConnectionStringADONET(cp.core.appConfig.name, DataSourcename);
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
            return cp.core.db.getTableID(TableName);
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
                    returnKey = genericController.getGUIDString();
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
                logController.handleError(cp.core,ex);
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
            throw new NotImplementedException("ADODB is not supported. Convert to executeQuery, executeNonQuery and executeNonQueryAsync");
            //return cp.core.db.executeSql_getRecordSet(SQL, DataSourcename, Controllers.genericController.encodeInteger(PageNumber) * Controllers.genericController.encodeInteger(PageSize), Controllers.genericController.encodeInteger(PageSize));
        }
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
        public override void Delete(string TableName, int RecordId) {
            throw new NotImplementedException();
        }
        public override string EncodeSQLNumber(int SourceNumber) {
            throw new NotImplementedException();
        }
        public override void ExecuteNonQuery(string sql) {
            throw new NotImplementedException();
        }
        public override void ExecuteNonQuery(string sql, string dataSourceName) {
            throw new NotImplementedException();
        }
        public override void ExecuteNonQuery(string sql, string dataSourceName, ref int recordsAffected) {
            throw new NotImplementedException();
        }
        public override void ExecuteNonQueryAsync(string sql) {
            throw new NotImplementedException();
        }
        public override void ExecuteNonQueryAsync(string sql, string dataSourceName) {
            throw new NotImplementedException();
        }
        public override DataTable ExecuteQuery(string sql) {
            throw new NotImplementedException();
        }
        public override DataTable ExecuteQuery(string sql, string dataSourceName) {
            throw new NotImplementedException();
        }
        public override DataTable ExecuteQuery(string sql, string dataSourceName, int startRecord) {
            throw new NotImplementedException();
        }
        public override DataTable ExecuteQuery(string sql, string dataSourceName, int startRecord, int maxRecords) {
            throw new NotImplementedException();
        }
        public override bool IsTable(string TableName) {
            throw new NotImplementedException();
        }
        public override bool IsTableField(string TableName, string FieldName) {
            throw new NotImplementedException();
        }
        //
        //====================================================================================================
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
    }
}