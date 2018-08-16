
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
    public class addonPageRuleModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "add-on page rules";
        public const string contentTableName = "ccAddonPageRules";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int addonId { get; set; }
        public int pageId { get; set; }
        //
        //====================================================================================================
        public static addonPageRuleModel add(CoreController core) {
            return add<addonPageRuleModel>(core);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<addonPageRuleModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel create(CoreController core, int recordId) {
            return create<addonPageRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<addonPageRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel create(CoreController core, string recordGuid) {
            return create<addonPageRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<addonPageRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel createByName(CoreController core, string recordName) {
            return createByName<addonPageRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<addonPageRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<addonPageRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<addonPageRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<addonPageRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<addonPageRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<addonPageRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<addonPageRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<addonPageRuleModel> createList(CoreController core, string sqlCriteria) {
            return createList<addonPageRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<addonPageRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<addonPageRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<addonPageRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<addonPageRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel createDefault(CoreController core) {
            return createDefault<addonPageRuleModel>(core);
        }
    }
}
