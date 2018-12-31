
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
    public class LibraryFileLogModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "library File log";
        public const string contentTableName = "cclibraryFileLog";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int fileID { get; set; }
        public int memberID { get; set; }
        public int visitID { get; set; }
        // 
        //====================================================================================================
        public static LibraryFileLogModel addEmpty(CoreController core) {
            return addEmpty<LibraryFileLogModel>(core);
        }
        //
        //====================================================================================================
        public static LibraryFileLogModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<LibraryFileLogModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static LibraryFileLogModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<LibraryFileLogModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LibraryFileLogModel create(CoreController core, int recordId) {
            return create<LibraryFileLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static LibraryFileLogModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<LibraryFileLogModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LibraryFileLogModel create(CoreController core, string recordGuid) {
            return create<LibraryFileLogModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static LibraryFileLogModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<LibraryFileLogModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LibraryFileLogModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<LibraryFileLogModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static LibraryFileLogModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<LibraryFileLogModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<LibraryFileLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<LibraryFileLogModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<LibraryFileLogModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<LibraryFileLogModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<LibraryFileLogModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<LibraryFileLogModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<LibraryFileLogModel> createList(CoreController core, string sqlCriteria) {
            return createList<LibraryFileLogModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<LibraryFileLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<LibraryFileLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<LibraryFileLogModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<LibraryFileLogModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        //public static LibraryFileLogModel createDefault(CoreController core) {
        //    return createDefault<LibraryFileLogModel>(core);
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<LibraryFileLogModel>(core);
        }
    }
}
