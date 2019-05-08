
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
    public class VisitorModel : DbModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "visitors";
        public const string contentTableName = "ccvisitors";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int MemberID { get; set; }
        public int ForceBrowserMobile { get; set; }
        // 
        //====================================================================================================
        public static VisitorModel addEmpty(CoreController core) {
            return addEmpty<VisitorModel>(core);
        }
        //
        //====================================================================================================
        public static VisitorModel addDefault(CoreController core, Domain.ContentMetadataModel metaData) {
            return addDefault<VisitorModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static VisitorModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.ContentMetadataModel metaData) {
            return addDefault<VisitorModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static VisitorModel create(CoreController core, int recordId) {
            return create<VisitorModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static VisitorModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<VisitorModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static VisitorModel create(CoreController core, string recordGuid) {
            return create<VisitorModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static VisitorModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<VisitorModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static VisitorModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<VisitorModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static VisitorModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<VisitorModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<VisitorModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<VisitorModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<VisitorModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<VisitorModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<VisitorModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<VisitorModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<VisitorModel> createList(CoreController core, string sqlCriteria) {
            return createList<VisitorModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateCacheOfRecord<VisitorModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return DbModel.getRecordName<VisitorModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return DbModel.getRecordName<VisitorModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return DbModel.getRecordId<VisitorModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<VisitorModel>(core);
        }
    }
}
