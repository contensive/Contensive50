
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
    public class LanguageModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "languages";
        public const string contentTableName = "cclanguages";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public string http_Accept_Language { get; set; }
        //
        //====================================================================================================
        public static LanguageModel add(CoreController core) {
            return add<LanguageModel>(core);
        }
        //
        //====================================================================================================
        public static LanguageModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<LanguageModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LanguageModel create(CoreController core, int recordId) {
            return create<LanguageModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static LanguageModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<LanguageModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LanguageModel create(CoreController core, string recordGuid) {
            return create<LanguageModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static LanguageModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<LanguageModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LanguageModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<LanguageModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static LanguageModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<LanguageModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<LanguageModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<LanguageModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<LanguageModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<LanguageModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<LanguageModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<LanguageModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<LanguageModel> createList(CoreController core, string sqlCriteria) {
            return createList<LanguageModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<LanguageModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<LanguageModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<LanguageModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<LanguageModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static LanguageModel createDefault(CoreController core) {
            return createDefault<LanguageModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<LanguageModel>(core);
        }
    }
}
