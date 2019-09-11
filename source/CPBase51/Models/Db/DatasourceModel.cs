
using Contensive.BaseClasses;
using System;
using System.Collections.Generic;
using System.Data;

namespace Contensive.Models.Db {
    [System.Serializable]
    public class DataSourceModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "data sources";
        public const string contentTableNameLowerCase = "ccdatasources";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        public enum DataSourceTypeEnum {
            sqlServerOdbc = 1,
            sqlServerNative = 2,
            mySqlNative = 3
        }
        //
        //====================================================================================================
        // -- instance properties
        public string connString { get; set; }
        public string endpoint { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public int dbTypeId { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// return true if the datasource name is either blank or the word default (in this case, use the config model's datasource
        /// </summary>
        /// <returns></returns>
        public static bool isDataSourceDefault(string datasourceName) {
            return (string.IsNullOrWhiteSpace(datasourceName) || (datasourceName.ToLower() == "default"));
        }
        //
        //====================================================================================================
        //
        public static Dictionary<string, DataSourceModel> getNameDict(CPBaseClass cp) {
            Dictionary<string, DataSourceModel> result = new Dictionary<string, DataSourceModel>();
            try {
                List<string> ignoreCacheNames = new List<string>();
                using ( DataTable dt = cp.Db.ExecuteQuery("select id from ccdatasources where active>0")) {
                    foreach ( DataRow row in dt.Rows) {
                        DataSourceModel instance = create<DataSourceModel>(cp, cp.Utils.EncodeInteger( row["id"] ));
                        if (instance != null) {
                            result.Add(instance.name.ToLowerInvariant(), instance);
                        }
                    }
                }
                if (!result.ContainsKey("default")) {
                    result.Add("default", getDefaultDatasource(cp));
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the default datasource. The default datasource is defined in the application configuration and is NOT a Db record in the ccdatasources table
        /// </summary>
        /// <param name="cp"></param>
        public static DataSourceModel getDefaultDatasource(CPBaseClass cp) {
            DataSourceModel result = null;
            try {
                result = new DataSourceModel {
                    active = true,
                    ccguid = "",
                    connString = "",
                    contentControlID = 0,
                    createdBy = 0,
                    createKey = 0,
                    dateAdded = DateTime.MinValue,
                    dbTypeId = (int)DataSourceTypeEnum.sqlServerNative,
                    endpoint = cp.serverConfig.defaultDataSourceAddress,
                    name = "default"
                    //password = core.serverConfig.password,
                    //username = core.serverConfig.username
                };
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// convert a datasource name into the key value used by the datasourcedictionary cache
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <returns></returns>
        public static string normalizeDataSourceName(string DataSourceName) {
            if (!string.IsNullOrEmpty(DataSourceName)) {
                return DataSourceName.Trim().ToLowerInvariant();
            }
            return string.Empty;
        }
    }
}
