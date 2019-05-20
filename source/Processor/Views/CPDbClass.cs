
using System;
using System.Data;
using Contensive.Processor.Controllers;
using Contensive.BaseClasses;

namespace Contensive.Processor {
    //
    //====================================================================================================
    /// <summary>
    /// Implements CPDbBaseClass. The default datasource is automatically setup as CP.Db and is disposed on cp dispose.
    /// Other datasources are implemented with CP.DbNew(datasourceName)
    /// </summary>
    public class CPDbClass : CPDbBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "13B707F9-190A-4ECD-8A53-49FE1D8CAE54";
        public const string InterfaceId = "A20CF53D-1FB7-41C4-8493-3D5AC3671A5E";
        public const string EventsId = "F68D64F0-595F-4CA2-9EC6-6DBA9A7E458B";
        #endregion
        /// <summary>
        /// dependencies
        /// </summary>
        private CPClass cp;
        /// <summary>
        /// All db controller calls go to this object. 
        /// If this instance was created with the default datasource, db is set the core.db
        /// </summary>
        internal DbController db;
        /// <summary>
        /// 
        /// </summary>
        public enum RemoteQueryType {
            sql = 1,
            openContent = 2,
            updateContent = 3,
            insertContent = 4
        }
        //
        //====================================================================================================
        /// <summary>
        /// Construct. This object is just a wrapper to create a consistent API passed to addon execution. The
        /// actual code is all within coreController. During startup, core loads and uses the default datasource
        /// at core.db. That is the only Db implemention used internally.
        /// If an Addon uses CP.Db, a new Db controller is used (so there can be 2 DbControllers on the default datasource)
        /// If an addon creates a new DbController instance using CP.Db New(datasource) a new instance of this class 
        /// creates a new instance of dbcontroller. There may be mulitple instances of dbcontroller pointing to the same Db,
        /// but this construct makes disposing more straight forward.
        /// </summary>
        /// <param name="cpParent"></param>
        public CPDbClass(CPClass cpParent, string dataSourceName) : base() {
            cp = cpParent;
            db = new DbController(cp.core, dataSourceName);
        }
        //
        //====================================================================================================
        //
        public override string GetConnectionString() {
            return db.getConnectionStringADONET(cp.core.appConfig.name);
        }
        //
        //====================================================================================================
        //
        public override DataTable ExecuteQuery(string sql, int startRecord, int maxRecords) {
            return db.executeQuery(sql, startRecord, maxRecords);
        }
        //
        //====================================================================================================
        //
        public override DataTable ExecuteQuery(string sql, int startRecord) {
            return db.executeQuery(sql, startRecord);
        }
        //
        //====================================================================================================
        //
        public override void ExecuteNonQuery(string sql, ref int recordsAffected) {
            db.executeNonQuery(sql, ref recordsAffected);
        }
        //
        //====================================================================================================
        //
        public override string GetRemoteQueryKey(string sql, int pageSize) {
            string returnKey = "";
            try {
                using (var cs = new CPCSClass(cp)) {
                    //
                    if (pageSize == 0) {
                        pageSize = 9999;
                    }
                    if (cs.Insert("Remote Queries")) {
                        returnKey = GenericController.getGUIDNaked();
                        int dataSourceID = cp.Content.GetRecordID("Data Sources", "");
                        cs.SetField("remotekey", returnKey);
                        cs.SetField("datasourceid", dataSourceID.ToString());
                        cs.SetField("sqlquery", sql);
                        cs.SetField("maxRows", pageSize.ToString());
                        cs.SetField("dateexpires", DateTime.Now.AddDays(1).ToString());
                        cs.SetField("QueryTypeID", RemoteQueryType.sql.ToString());
                        cs.SetField("VisitID", cp.Visit.Id.ToString());
                    }
                    cs.Close();
                    //
                }
            } catch (Exception ex) {
                LogController.logError(cp.core, ex);
            }
            return returnKey;
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
        public override string GetRemoteQueryKey(string sql)
            => GetRemoteQueryKey(sql, Constants.sqlPageSizeDefault);
        //
        //====================================================================================================
        //
        public override int SQLTimeout {
            get {
                return db.sqlCommandTimeout;
            }
            set {
                db.sqlCommandTimeout = value;
            }
        }
        public override void Delete(string TableName, int RecordId) {
            db.deleteTableRecord(RecordId, TableName);
        }
        public override string EncodeSQLNumber(int SourceNumber) {
            return DbController.encodeSQLNumber(SourceNumber);
        }
        //
        public override void ExecuteNonQuery(string sql) {
            db.executeNonQuery(sql);
        }
        //
        public override void ExecuteNonQueryAsync(string sql) {
            db.executeNonQueryAsync(sql);
        }
        public override DataTable ExecuteQuery(string sql) {
            return db.executeQuery(sql);
        }
        public override bool IsTable(string TableName) {
            return db.isSQLTable(TableName);
        }
        public override bool IsTableField(string TableName, string FieldName) {
            return db.isSQLTableField(TableName, FieldName);
        }
        public override DataTable ExecuteRemoteQuery(string remoteQueryKey) {
            return db.executeRemoteQuery(remoteQueryKey);
        }
        //
        //====================================================================================================
        // deprecated
        //
        [Obsolete("deprecated. Convert to datatables and use executeQuery(), executeNonQuery(), or executeNonQueryAsync()", false)]
        public override object ExecuteSQL(string sql, string ignoreDataSourceName, string ignoreRetries, string ignorePageSize, string ignorePageNumber) {
            db.executeNonQuery(sql);
            return null;
        }
        //
        [Obsolete("Convert to datatables or use models", false)]
        public override object ExecuteSQL(string sql, string ignoreDataSourceName, string Retries, string PageSize) {
            db.executeNonQuery(sql);
            return null;
        }
        //
        [Obsolete("Convert to datatables or use models", false)]
        public override object ExecuteSQL(string sql, string ignoreDataSourceName, string Retries) {
            db.executeNonQuery(sql);
            return null;
        }
        //
        [Obsolete("Convert to datatables or use models", false)]
        public override object ExecuteSQL(string sql, string ignoreDataSourceName) {
            db.executeNonQuery(sql);
            return null;
        }
        //
        [Obsolete("Convert to datatables or use models", false)]
        public override object ExecuteSQL(string sql) {
            db.executeNonQuery(sql);
            return null;
        }
        //
        [Obsolete("Use GetConnectionString( dataSourceName )")]
        public override string DbGetConnectionString(string ignoreDataSourceName) {
            return GetConnectionString();
        }
        //
        [Obsolete("Only Sql Server currently supported", false)]
        public override int GetDataSourceType(string ignoreDataSourceName) {
            return db.getDataSourceType();
        }
        //
        [Obsolete("Use GetDataSourceType( dataSourceName )")]
        public override int DbGetDataSourceType(string ignoreDataSourceName) {
            return db.getDataSourceType();
        }
        //
        [Obsolete("Use Db.Content.GetTableId instead.", false)]
        public override int DbGetTableID(string TableName) {
            return GetTableID(TableName);
        }
        //
        [Obsolete("Use isTable instead", false)]
        public override bool DbIsTable(string ignoreDataSourceName, string TableName) {
            return IsTable(TableName);
        }
        //
        [Obsolete("Use isTableField instead", false)]
        public override bool DbIsTableField(string ignoreDataSourceName, string TableName, string FieldName) {
            return IsTableField(TableName, FieldName);
        }
        //
        [Obsolete("Deprecated. Use methods without explicit datasource.", false)]
        public override DataTable ExecuteQuery(string sql, string ignoreDataSourceName) {
            return db.executeQuery(sql);
        }
        //
        [Obsolete("Deprecated. Use methods without explicit datasource.", false)]
        public override DataTable ExecuteQuery(string sql, string ignoreDataSourceName, int startRecord) {
            return db.executeQuery(sql, startRecord);
        }
        //
        [Obsolete("Deprecated. Use methods without explicit datasource.", false)]
        public override DataTable ExecuteQuery(string sql, string ignoreDataSourceName, int startRecord, int maxRecords) {
            return db.executeQuery(sql, startRecord, maxRecords);
        }
        //
        [Obsolete("Deprecated. Use methods without explicit datasource.", false)]
        public override void Delete(string ignoreDataSourceName, string TableName, int RecordId) {
            db.deleteTableRecord(RecordId, TableName);
        }
        //
        [Obsolete("Deprecated. Use methods without explicit datasource.", false)]
        public override string GetConnectionString(string ignoreDataSourceName) {
            return db.getConnectionStringADONET(cp.core.appConfig.name);
        }
        //
        [Obsolete("Deprecated. Use CP.Content.GetTableId().", false)]
        public override int GetTableID(string TableName) {
            return DbController.getTableID(cp.core, TableName);
        }
        //
        [Obsolete("Deprecated. Use methods without explicit datasource.", false)]
        public override bool IsTable(string ignoreDataSourceName, string TableName) {
            return db.isSQLTable(TableName);
        }
        //
        [Obsolete("Deprecated. Use methods without explicit datasource.", false)]
        public override bool IsTableField(string ignoreDataSourceName, string TableName, string FieldName) {
            return db.isSQLTableField(TableName, FieldName);
        }
        //
        [Obsolete("Deprecated. Use methods without explicit datasource.", false)]
        public override string GetRemoteQueryKey(string sql, string ignoreDataSourceName, int pageSize) {
            string returnKey = "";
            try {
                using (var cs = new CPCSClass(cp)) {
                    //
                    if (pageSize == 0) {
                        pageSize = 9999;
                    }
                    if (cs.Insert("Remote Queries")) {
                        returnKey = GenericController.getGUIDNaked();
                        int dataSourceID = cp.Content.GetRecordID("Data Sources", db.dataSourceName);
                        cs.SetField("remotekey", returnKey);
                        cs.SetField("datasourceid", dataSourceID.ToString());
                        cs.SetField("sqlquery", sql);
                        cs.SetField("maxRows", pageSize.ToString());
                        cs.SetField("dateexpires", DateTime.Now.AddDays(1).ToString());
                        cs.SetField("QueryTypeID", RemoteQueryType.sql.ToString());
                        cs.SetField("VisitID", cp.Visit.Id.ToString());
                    }
                    cs.Close();
                    //
                }
            } catch (Exception ex) {
                LogController.logError(cp.core, ex);
            }
            return returnKey;
        }
        //
        [Obsolete("Deprecated. Use methods without explicit datasource.", false)]
        public override string GetRemoteQueryKey(string sql, string ignoreDataSourceName)
            => GetRemoteQueryKey(sql, Constants.sqlPageSizeDefault);
        //
        [Obsolete("Deprecated. Use methods without explicit datasource.", false)]
        public override void ExecuteNonQuery(string sql, string ignoreDataSourceName) {
            db.executeNonQuery(sql);
        }
        //
        [Obsolete("Deprecated. Use methods without explicit datasource.", false)]
        public override void ExecuteNonQuery(string sql, string ignoreDataSourceName, ref int recordsAffected) {
            db.executeNonQuery(sql, ref recordsAffected);
        }
        //
        [Obsolete("Deprecated. Use methods without explicit datasource.", false)]
        public override void ExecuteNonQueryAsync(string sql, string ignoreDataSourceName) {
            db.executeNonQueryAsync(sql);
        }
        //
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
        public override void Dispose() {
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