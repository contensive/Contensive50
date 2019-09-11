
//using System;
//using System.Collections.Generic;
//using System.Data;
//using Contensive.Models.Db;
//using Contensive.Processor.Controllers;
////
//namespace Contensive.Processor.Models.Db {
//    /// <summary>
//    /// Use this model when you just need the base fields or need to supply the tablename programmatically
//    /// </summary>
//    [System.Serializable]
//    public class DbGenericModel : DbBaseFieldsModel {
//        //
//        //=============================================================
//        /// <summary>
//        /// Get the lowest recordId based on its name. If no record is found, 0 is returned.
//        /// Use only when Models are not available. 
//        /// </summary>
//        /// <param name="tableName"></param>
//        /// <param name="recordName"></param>
//        /// <returns></returns>
//        public static int getRecordIdByUniqueName(CoreController core, string tableName, string recordName) {
//            try {
//                if (String.IsNullOrWhiteSpace(recordName)) { return 0; }
//                if (String.IsNullOrWhiteSpace(tableName)) { return 0; }
//                using (DataTable dt = core.db.executeQuery("select top 1 id from " + tableName + " where name=" + DbController.encodeSQLText(recordName) + " order by id")) {
//                    foreach (DataRow dr in dt.Rows) {
//                        return DbController.getDataRowFieldInteger(dr, "id");
//                    }
//                }
//                return 0;
//            } catch (Exception ex) {
//                LogController.logError(core, ex);
//                throw;
//            }
//        }
//        //
//        //=============================================================
//        /// <summary>
//        /// Get the lowest recordId based on its guid. If no record is found, 0 is returned.
//        /// Use only when Models are not available. 
//        /// </summary>
//        /// <param name="tableName"></param>
//        /// <param name="recordName"></param>
//        /// <returns></returns>
//        public static int getRecordId(CoreController core, string tableName, string recordGuid) {
//            try {
//                if (String.IsNullOrWhiteSpace(recordGuid)) { return 0; }
//                if (String.IsNullOrWhiteSpace(tableName)) { return 0; }
//                using (DataTable dt = core.db.executeQuery("select top 1 id from " + tableName + " where ccguid=" + DbController.encodeSQLText(recordGuid) + " order by id")) {
//                    foreach (DataRow dr in dt.Rows) {
//                        return DbController.getDataRowFieldInteger(dr, "id");
//                    }
//                }
//                return 0;
//            } catch (Exception ex) {
//                LogController.logError(core, ex);
//                throw;
//            }
//        }
//        //
//        //=============================================================
//        /// <summary>
//        /// Get a list of records that match a criteria
//        /// Use only when Models are not available. 
//        /// </summary>
//        /// <param name="tableName"></param>
//        /// <param name="recordName"></param>
//        /// <returns></returns>
//        public static   List<DbBaseFieldsModel> getRecordList(CoreController core, string tableName, string sqlCriteria, string orderBy) {
//            try {
//                if (String.IsNullOrWhiteSpace(tableName)) { return new List<DbBaseFieldsModel>(); }
//                using (DataTable dt = core.db.executeQuery("select id,name,active,ccguid,contentcontrolid,createdby,createkey,dateadded,modifiedby,modifiedDate,sortorder from " + tableName + ((string.IsNullOrWhiteSpace(sqlCriteria)) ? "" : " where " + sqlCriteria) + " order by " + ((string.IsNullOrWhiteSpace(orderBy)) ? "id" : orderBy))) {
//                    var result = new List<DbBaseFieldsModel>();
//                    foreach (DataRow dr in dt.Rows) {
//                        result.Add(new DbBaseFieldsModel() {
//                            id = DbController.getDataRowFieldInteger(dr, "id"),
//                            name = DbController.getDataRowFieldText(dr, "name"),
//                            active = DbController.getDataRowFieldBoolean(dr, "active"),
//                            ccguid = DbController.getDataRowFieldText(dr, "ccguid"),
//                            contentControlID = DbController.getDataRowFieldInteger(dr, "contentcontrolid"),
//                            createdBy = DbController.getDataRowFieldInteger(dr, "createdby"),
//                            createKey = DbController.getDataRowFieldInteger(dr, "createkey"),
//                            dateAdded = DbController.getDataRowFieldDate(dr, "dateadded"),
//                            modifiedBy = DbController.getDataRowFieldInteger(dr, "modifiedBy"),
//                            modifiedDate = DbController.getDataRowFieldDate(dr, "modifiedDate"),
//                            sortOrder = DbController.getDataRowFieldText(dr, "sortOrder")
//                        });
//                    }
//                    return result;
//                }
//            } catch (Exception ex) {
//                LogController.logError(core, ex);
//                throw;
//            }
//        }
//        //
//        //=============================================================
//        /// <summary>
//        /// Get a list of records that match a criteria
//        /// Use only when Models are not available. 
//        /// </summary>
//        /// <param name="tableName"></param>
//        /// <param name="recordName"></param>
//        /// <returns></returns>
//        public static List<DbBaseFieldsModel> getRecordList(CoreController core, string tableName, string sqlCriteria) => getRecordList(core, tableName, sqlCriteria, "");
//        //
//        //=============================================================
//        /// <summary>
//        /// Get a list of records that match a criteria
//        /// Use only when Models are not available. 
//        /// </summary>
//        /// <param name="tableName"></param>
//        /// <param name="recordName"></param>
//        /// <returns></returns>
//        public static List<DbBaseFieldsModel> getRecordList(CoreController core, string tableName) => getRecordList(core, tableName, "", "");
//    }
//}
