
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
    public class MemberRuleModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "member rules";
        public const string contentTableName = "ccmemberrules";
        public const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public DateTime DateExpires { get; set; }
        public int GroupID { get; set; }
        public int MemberID { get; set; }
        //
        //====================================================================================================
        public static MemberRuleModel add(CoreController core) {
            return add<MemberRuleModel>(core);
        }
        //
        //====================================================================================================
        public static MemberRuleModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<MemberRuleModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static MemberRuleModel create(CoreController core, int recordId) {
            return create<MemberRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static MemberRuleModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<MemberRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static MemberRuleModel create(CoreController core, string recordGuid) {
            return create<MemberRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static MemberRuleModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<MemberRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static MemberRuleModel createByName(CoreController core, string recordName) {
            return createByName<MemberRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static MemberRuleModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<MemberRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<MemberRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<MemberRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<MemberRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<MemberRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<MemberRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<MemberRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<MemberRuleModel> createList(CoreController core, string sqlCriteria) {
            return createList<MemberRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<MemberRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<MemberRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<MemberRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<MemberRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static MemberRuleModel createDefault(CoreController core) {
            return createDefault<MemberRuleModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<MemberRuleModel>(core);
        }
    }
}
