
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
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.Db {
    public class memberRuleModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "member rules";
        public const string contentTableName = "ccmemberrules";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public DateTime DateExpires { get; set; }
        public int GroupID { get; set; }
        public int MemberID { get; set; }
        //
        //====================================================================================================
        public static memberRuleModel add(CoreController core) {
            return add<memberRuleModel>(core);
        }
        //
        //====================================================================================================
        public static memberRuleModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<memberRuleModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static memberRuleModel create(CoreController core, int recordId) {
            return create<memberRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static memberRuleModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<memberRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static memberRuleModel create(CoreController core, string recordGuid) {
            return create<memberRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static memberRuleModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<memberRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static memberRuleModel createByName(CoreController core, string recordName) {
            return createByName<memberRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static memberRuleModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<memberRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<memberRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<memberRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<memberRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<memberRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<memberRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<memberRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<memberRuleModel> createList(CoreController core, string sqlCriteria) {
            return createList<memberRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<memberRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<memberRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<memberRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<memberRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static memberRuleModel createDefault(CoreController core) {
            return createDefault<memberRuleModel>(core);
        }
    }
}
