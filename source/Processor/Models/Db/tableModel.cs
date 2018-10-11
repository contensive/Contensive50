
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
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.Db {
    public class TableModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "tables";
        public const string contentTableName = "ccTables";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public int dataSourceID { get; set; }
        // 
        //====================================================================================================
        public static TableModel addEmpty(CoreController core) {
            return addEmpty<TableModel>(core);
        }
        //
        //====================================================================================================
        public static TableModel addDefault(CoreController core) {
            return addDefault<TableModel>(core);
        }
        //
        //====================================================================================================
        public static TableModel addDefault(CoreController core, ref List<string> callersCacheNameList) {
            return addDefault<TableModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TableModel create(CoreController core, int recordId) {
            return create<TableModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static TableModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<TableModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TableModel create(CoreController core, string recordGuid) {
            return create<TableModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static TableModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<TableModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TableModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<TableModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static TableModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<TableModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<TableModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<TableModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<TableModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<TableModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<TableModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<TableModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<TableModel> createList(CoreController core, string sqlCriteria) {
            return createList<TableModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<TableModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<TableModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<TableModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<TableModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        //public static TableModel createDefault(CoreController core) {
        //    return createDefault<TableModel>(core);
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<TableModel>(core);
        }
    }
}
