
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.DbModels {
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
        public static addonPageRuleModel add(coreController core) {
            return add<addonPageRuleModel>(core);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<addonPageRuleModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel create(coreController core, int recordId) {
            return create<addonPageRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<addonPageRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel create(coreController core, string recordGuid) {
            return create<addonPageRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<addonPageRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel createByName(coreController core, string recordName) {
            return createByName<addonPageRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<addonPageRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<addonPageRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<addonPageRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<addonPageRuleModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<addonPageRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<addonPageRuleModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<addonPageRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<addonPageRuleModel> createList(coreController core, string sqlCriteria) {
            return createList<addonPageRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<addonPageRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<addonPageRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<addonPageRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<addonPageRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel createDefault(coreController core) {
            return createDefault<addonPageRuleModel>(core);
        }
    }
}
