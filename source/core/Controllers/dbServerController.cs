
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
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

namespace Contensive.Core.Controllers {
    //
    //==========================================================================================
    /// <summary>
    /// Manage the sql server (adding catalogs, etc.)
    /// </summary>
    public class dbServerController : IDisposable {
        //
        // objects passed in that are not disposed
        //
        private coreClass cpCore;
        //
        // internal storage
        //
        private bool dbEnabled = true; // set true when configured and tested - else db calls are skipped
        //
        //==========================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public dbServerController(coreClass cpCore) : base() {
            try {
                this.cpCore = cpCore;
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the correctly formated connection string for this datasource. Called only from within this class
        /// </summary>
        /// <returns>
        /// </returns>
        public string getConnectionStringADONET() {
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
                string serverUrl = cpCore.serverConfig.defaultDataSourceAddress;
                if (serverUrl.IndexOf(":") > 0) {
                    serverUrl = serverUrl.Left( serverUrl.IndexOf(":"));
                }
                returnConnString += ""
                    + "server=" + serverUrl + ";"
                    + "User Id=" + cpCore.serverConfig.defaultDataSourceUsername + ";"
                    + "Password=" + cpCore.serverConfig.defaultDataSourcePassword + ";"
                    + "";
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnConnString;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Create a new catalog in the database
        /// </summary>
        /// <param name="catalogName"></param>
        public void createCatalog(string catalogName) {
            try {
                executeSql("create database " + catalogName);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Check if the database exists
        /// </summary>
        /// <param name="catalog"></param>
        /// <returns></returns>
        public bool checkCatalogExists(string catalog) {
            bool returnOk = false;
            try {
                string sql = null;
                DataTable dt = null;
                //
                sql = string.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", catalog);
                dt = executeSql(sql);
                returnOk = (dt.Rows.Count > 0);
                dt.Dispose();
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnOk;
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
        private DataTable executeSql(string sql) {
            DataTable returnData = new DataTable();
            try {
                string connString = getConnectionStringADONET();
                if (dbEnabled) {
                    using (SqlConnection connSQL = new SqlConnection(connString)) {
                        connSQL.Open();
                        using (SqlCommand cmdSQL = new SqlCommand()) {
                            cmdSQL.CommandType = CommandType.Text;
                            cmdSQL.CommandText = sql;
                            cmdSQL.Connection = connSQL;
                            using (dynamic adptSQL = new System.Data.SqlClient.SqlDataAdapter(cmdSQL)) {
                                adptSQL.Fill(returnData);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                ApplicationException newEx = new ApplicationException("Exception [" + ex.Message + "] executing master sql [" + sql + "]", ex);
                cpCore.handleException(newEx);
            }
            return returnData;
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
        ~dbServerController() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}

