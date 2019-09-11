
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
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
        public static DataSourceModel addEmpty(CoreController core) {
            return addEmpty<DataSourceModel>(core);
        }
        //
        //====================================================================================================
        public static DataSourceModel addDefault(CoreController core, Domain.ContentMetadataModel metaData) {
            return addDefault<DataSourceModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static DataSourceModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.ContentMetadataModel metaData) {
            return addDefault<DataSourceModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static DataSourceModel create(CoreController core, string recordGuid) {
            return create<DataSourceModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static DataSourceModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<DataSourceModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<DataSourceModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<DataSourceModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<DataSourceModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<DataSourceModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<DataSourceModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<DataSourceModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<DataSourceModel> createList(CoreController core, string sqlCriteria) {
            return createList<DataSourceModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateCacheOfRecord<DataSourceModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void invalidateTableCache(CoreController core) {
            invalidateCacheOfTable<DataSourceModel>(core);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return DbBaseModel.getRecordName<DataSourceModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return DbBaseModel.getRecordName<DataSourceModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return DbBaseModel.getRecordId<DataSourceModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Special case for create. If id less than or equal to 0  return default datasource
        /// </summary>
        /// <param name="core"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public static DataSourceModel create(CoreController core, int recordId) {
            return (recordId > 0) ? create<DataSourceModel>(core, recordId) : getDefaultDatasource(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Special case for create. If id less than or equal to 0  return default datasource
        /// </summary>
        /// <param name="core"></param>
        /// <param name="recordId"></param>
        /// <param name="callersCacheNameList"></param>
        /// <returns></returns>
        public static DataSourceModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return (recordId > 0) ? create<DataSourceModel>(core, recordId, ref callersCacheNameList) : getDefaultDatasource(core);
        }
        //
        //====================================================================================================
        public static DataSourceModel createByUniqueName(CoreController core, string recordName) {
            return (string.IsNullOrWhiteSpace(recordName)|(recordName.ToLowerInvariant()=="default")) ? getDefaultDatasource(core) : createByUniqueName<DataSourceModel>(core, recordName );
        }
        //
        //====================================================================================================
        public static DataSourceModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return (string.IsNullOrWhiteSpace(recordName) || (recordName.ToLowerInvariant() == "default")) ? getDefaultDatasource(core) : createByUniqueName<DataSourceModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<DataSourceModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// return true if the datasource name is either blank or the word default (in this case, use the config model's datasource
        /// </summary>
        /// <returns></returns>
        public static bool isDataSourceDefault( string datasourceName ) {
            return (string.IsNullOrWhiteSpace(datasourceName) || (datasourceName.ToLower() == "default"));
        }
        //
        //====================================================================================================
        public static Dictionary<string, DataSourceModel> getNameDict(CoreController core) {
            Dictionary<string, DataSourceModel> result = new Dictionary<string, DataSourceModel>();
            try {
                CsModel cs = new CsModel(core);
                List<string> ignoreCacheNames = new List<string>();
                if (cs.openSql("select id from ccdatasources where active>0")) {
                    do {
                        DataSourceModel instance = create(core, cs.getInteger("id"));
                        if (instance != null) {
                            result.Add(instance.name.ToLowerInvariant(), instance);
                        }
                    } while (true);
                }
                if (!result.ContainsKey("default")) {
                    result.Add("default", getDefaultDatasource(core));
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
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
        public static DataSourceModel getDefaultDatasource(CoreController core) {
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
                    endpoint = core.serverConfig.defaultDataSourceAddress,
                    name = "default"
                    //password = core.serverConfig.password,
                    //username = core.serverConfig.username
                };
            } catch (Exception ex) {
                LogController.logError(core, ex);
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
