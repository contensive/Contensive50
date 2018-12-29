
using System;
using System.Data;
using Contensive.Processor.Controllers;
using Contensive.BaseClasses;

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
                LogController.handleError(cp.core,ex, "Unexpected Trap Error in CP.DB." + methodName);
            } catch (Exception) {}
        }
        //
        //====================================================================================================
        //
        public override void Delete(string DataSourcename, string TableName, int RecordId) {
            cp.core.db.deleteTableRecord(RecordId, TableName, DataSourcename);
        }
        //
        //====================================================================================================
        //
        public override string GetConnectionString(string DataSourcename) {
            return cp.core.db.getConnectionStringADONET(cp.core.appConfig.name, DataSourcename);
        }
        //
        //====================================================================================================
        //
        public override int GetTableID(string TableName) {
            return DbController.getTableID(cp.core, TableName);
        }
        //
        //====================================================================================================
        //
        public override bool IsTable(string DataSourcename, string TableName) {
            return cp.core.db.isSQLTable(DataSourcename, TableName);
        }
        //
        //====================================================================================================
        //
        public override bool IsTableField(string DataSourcename, string TableName, string FieldName) {
            return cp.core.db.isSQLTableField(DataSourcename, TableName, FieldName);
        }
        //
        //====================================================================================================
        //
        public override string EncodeSQLBoolean(bool SourceBoolean) {
            return DbController.encodeSQLBoolean(SourceBoolean);
        }
        //
        //====================================================================================================
        //
        public override string EncodeSQLDate(DateTime SourceDate) {
            return DbController.encodeSQLDate(SourceDate);
        }
        //
        //====================================================================================================
        //
        public override string EncodeSQLNumber(double SourceNumber) {
            return DbController.encodeSQLNumber(SourceNumber);
        }
        //
        //====================================================================================================
        //
        public override string EncodeSQLText(string SourceText) {
            return DbController.encodeSQLText(SourceText);
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
        public override string GetRemoteQueryKey(string sql, string DataSourceName, int pageSize) {
            string returnKey = "";
            try {
                using (var cs = new CPCSClass(cp)) {
                    //
                    if (pageSize == 0) {
                        pageSize = 9999;
                    }
                    if (cs.Insert("Remote Queries")) {
                        returnKey = GenericController.getGUIDString();
                        int dataSourceID = cp.Content.GetRecordID("Data Sources", DataSourceName);
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
                }
            } catch (Exception ex) {
                LogController.handleError(cp.core,ex);
            }
            return returnKey;
        }
        //
        public override string GetRemoteQueryKey(string sql, string DataSourceName) 
            => GetRemoteQueryKey(sql, DataSourceName, 100);
        //
        public override string GetRemoteQueryKey(string sql)
            => GetRemoteQueryKey(sql, "default", 100);
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
            cp.core.db.deleteTableRecord(RecordId, TableName);
        }
        public override string EncodeSQLNumber(int SourceNumber) {
            return DbController.encodeSQLNumber(SourceNumber);
        }
        public override void ExecuteNonQuery(string sql) {
            cp.core.db.executeNonQuery(sql);
        }
        public override void ExecuteNonQuery(string sql, string dataSourceName) {
            cp.core.db.executeNonQuery(sql,dataSourceName);
        }
        public override void ExecuteNonQuery(string sql, string dataSourceName, ref int recordsAffected) {
            cp.core.db.executeNonQuery(sql, dataSourceName,ref recordsAffected);
        }
        public override void ExecuteNonQueryAsync(string sql) {
            cp.core.db.executeNonQueryAsync(sql);
        }
        public override void ExecuteNonQueryAsync(string sql, string dataSourceName) {
            cp.core.db.executeNonQueryAsync(sql,dataSourceName);
        }
        public override DataTable ExecuteQuery(string sql) {
            return cp.core.db.executeQuery(sql);
        }
        public override DataTable ExecuteQuery(string sql, string dataSourceName) {
            return cp.core.db.executeQuery(sql, dataSourceName);
        }
        public override DataTable ExecuteQuery(string sql, string dataSourceName, int startRecord) {
            return cp.core.db.executeQuery(sql,dataSourceName,startRecord);
        }
        public override DataTable ExecuteQuery(string sql, string dataSourceName, int startRecord, int maxRecords) {
            return cp.core.db.executeQuery(sql, dataSourceName, startRecord, maxRecords);
        }
        public override bool IsTable(string TableName) {
            return cp.core.db.isSQLTable("", TableName);
        }
        public override bool IsTableField(string TableName, string FieldName) {
            return cp.core.db.isSQLTableField("", TableName, FieldName);
        }
        public override DataTable ExecuteRemoteQuery(string remoteQueryKey) {
            return cp.core.db.executeRemoteQuery(remoteQueryKey);
        }
        //
        //====================================================================================================
        // deprecated
        //
        [Obsolete("deprecated. Convert to datatables and use executeQuery(), executeNonQuery(), or executeNonQueryAsync()", true)]
        public override object ExecuteSQL(string sql, string dataSourcename, string ignoreRetries, string ignorePageSize, string ignorePageNumber) {
            cp.core.db.executeNonQuery(sql, dataSourcename);
            return null;
        }
        //
        [Obsolete("Convert to datatables or use models", true)]
        public override object ExecuteSQL(string sql, string dataSourcename, string Retries, string PageSize) {
            cp.core.db.executeNonQuery(sql, dataSourcename);
            return null;
        }
        //
        [Obsolete("Convert to datatables or use models", true)]
        public override object ExecuteSQL(string sql, string dataSourcename, string Retries) {
            cp.core.db.executeNonQuery(sql, dataSourcename);
            return null;
        }
        //
        [Obsolete("Convert to datatables or use models", true)]
        public override object ExecuteSQL(string sql, string dataSourcename) {
            cp.core.db.executeNonQuery(sql, dataSourcename);
            return null;
        }
        //
        [Obsolete("Convert to datatables or use models", true)]
        public override object ExecuteSQL(string sql) {
            cp.core.db.executeNonQuery(sql);
            return null;
        }
        //
        [Obsolete("Use GetConnectionString( dataSourceName )")]
        public override string DbGetConnectionString(string DataSourcename) {
            return GetConnectionString(DataSourcename);
        }
        //
        [Obsolete("Only Sql Server currently supported", true)]
        public override int GetDataSourceType(string DataSourcename) {
            return cp.core.db.getDataSourceType(DataSourcename);
        }
        //
        [Obsolete("Use GetDataSourceType( dataSourceName )")]
        public override int DbGetDataSourceType(string DataSourcename) {
            return GetDataSourceType(DataSourcename);
        }
        //
        [Obsolete("Use GetTableId instead.", true)]
        public override int DbGetTableID(string TableName) {
            return GetTableID(TableName);
        }
        //
        [Obsolete("Use isTable instead", true)]
        public override bool DbIsTable(string DataSourcename, string TableName) {
            return IsTable(DataSourcename, TableName);
        }
        //
        [Obsolete("Use isTableField instead", true)]
        public override bool DbIsTableField(string DataSourcename, string TableName, string FieldName) {
            return IsTableField(DataSourcename, TableName, FieldName);
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
        }
        #endregion
        //
    }
}