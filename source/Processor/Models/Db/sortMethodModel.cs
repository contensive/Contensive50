
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
using static Contensive.Processor.Constants;
//
namespace Contensive.Processor.Models.Db {
    public class SortMethodModel : DbModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "sort methods";
        public const string contentTableName = "ccSortMethods";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public string OrderByClause { get; set; }
        // 
        //====================================================================================================
        public static SortMethodModel addEmpty(CoreController core) {
            return AddEmpty<SortMethodModel>(core);
        }
        //
        //====================================================================================================
        public static SortMethodModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<SortMethodModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static SortMethodModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<SortMethodModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static SortMethodModel create(CoreController core, int recordId) {
            return create<SortMethodModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static SortMethodModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<SortMethodModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static SortMethodModel create(CoreController core, string recordGuid) {
            return create<SortMethodModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static SortMethodModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<SortMethodModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static SortMethodModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<SortMethodModel>(core, recordName );
        }
        //
        //====================================================================================================
        public static SortMethodModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<SortMethodModel>(core, recordName, ref callersCacheNameList );
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<SortMethodModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<SortMethodModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<SortMethodModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<SortMethodModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<SortMethodModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<SortMethodModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<SortMethodModel> createList(CoreController core, string sqlCriteria) {
            return createList<SortMethodModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<SortMethodModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return DbModel.getRecordName<SortMethodModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return DbModel.getRecordName<SortMethodModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return DbModel.getRecordId<SortMethodModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<SortMethodModel>(core);
        }
    }
}
