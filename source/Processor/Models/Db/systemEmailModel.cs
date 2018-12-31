
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
    public class SystemEmailModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "system email";
        public const string contentTableName = "ccemail";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public bool addLinkEID { get; set; }
        public bool allowSpamFooter { get; set; }
        public string copyFilename { get; set; }
        public int emailTemplateID { get; set; }
        public string fromAddress { get; set; }
        public DateTime scheduleDate { get; set; }
        public bool sent { get; set; }
        public string subject { get; set; }
        public bool submitted { get; set; }
        public int testMemberID { get; set; }
        // 
        //====================================================================================================
        public static SystemEmailModel addEmpty(CoreController core) {
            return addEmpty<SystemEmailModel>(core);
        }
        //
        //====================================================================================================
        public static SystemEmailModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<SystemEmailModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static SystemEmailModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<SystemEmailModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static SystemEmailModel create(CoreController core, int recordId) {
            return create<SystemEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static SystemEmailModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<SystemEmailModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static SystemEmailModel create(CoreController core, string recordGuid) {
            return create<SystemEmailModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static SystemEmailModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<SystemEmailModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static SystemEmailModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<SystemEmailModel>(core, recordName );
        }
        //
        //====================================================================================================
        public static SystemEmailModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<SystemEmailModel>(core, recordName, ref callersCacheNameList );
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<SystemEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<SystemEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<SystemEmailModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<SystemEmailModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<SystemEmailModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<SystemEmailModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<SystemEmailModel> createList(CoreController core, string sqlCriteria) {
            return createList<SystemEmailModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<SystemEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<SystemEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<SystemEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<SystemEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        //public static SystemEmailModel createDefault(CoreController core) {
        //    return createDefault<SystemEmailModel>(core);
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<SystemEmailModel>(core);
        }
    }
}
