
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
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.DbModels {
    public class memberRuleModel : baseModel {
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
        public static memberRuleModel add(coreController core) {
            return add<memberRuleModel>(core);
        }
        //
        //====================================================================================================
        public static memberRuleModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<memberRuleModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static memberRuleModel create(coreController core, int recordId) {
            return create<memberRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static memberRuleModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<memberRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static memberRuleModel create(coreController core, string recordGuid) {
            return create<memberRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static memberRuleModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<memberRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static memberRuleModel createByName(coreController core, string recordName) {
            return createByName<memberRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static memberRuleModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<memberRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<memberRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<memberRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<memberRuleModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<memberRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<memberRuleModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<memberRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<memberRuleModel> createList(coreController core, string sqlCriteria) {
            return createList<memberRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<memberRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<memberRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<memberRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<memberRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static memberRuleModel createDefault(coreController core) {
            return createDefault<memberRuleModel>(core);
        }
    }
}
