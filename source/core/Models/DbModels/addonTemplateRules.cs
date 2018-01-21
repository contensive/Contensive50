
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
    public class addonTemplateRuleModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "add-on template rules";
        public const string contentTableName = "ccAddonTemplateRules";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int addonId { get; set; }
        public int templateId { get; set; }
        //
        //====================================================================================================
        public static addonTemplateRuleModel add(coreController cpCore) {
            return add<addonTemplateRuleModel>(cpCore);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<addonTemplateRuleModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel create(coreController cpCore, int recordId) {
            return create<addonTemplateRuleModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<addonTemplateRuleModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel create(coreController cpCore, string recordGuid) {
            return create<addonTemplateRuleModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<addonTemplateRuleModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel createByName(coreController cpCore, string recordName) {
            return createByName<addonTemplateRuleModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<addonTemplateRuleModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<addonTemplateRuleModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<addonTemplateRuleModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<addonTemplateRuleModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<addonTemplateRuleModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<addonTemplateRuleModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<addonTemplateRuleModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<addonTemplateRuleModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<addonTemplateRuleModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<addonTemplateRuleModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<addonTemplateRuleModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<addonTemplateRuleModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<addonTemplateRuleModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel createDefault(coreController cpcore) {
            return createDefault<addonTemplateRuleModel>(cpcore);
        }
    }
}
