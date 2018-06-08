
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
        public static addonTemplateRuleModel add(coreController core) {
            return add<addonTemplateRuleModel>(core);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<addonTemplateRuleModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel create(coreController core, int recordId) {
            return create<addonTemplateRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<addonTemplateRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel create(coreController core, string recordGuid) {
            return create<addonTemplateRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<addonTemplateRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel createByName(coreController core, string recordName) {
            return createByName<addonTemplateRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<addonTemplateRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<addonTemplateRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<addonTemplateRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<addonTemplateRuleModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<addonTemplateRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<addonTemplateRuleModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<addonTemplateRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<addonTemplateRuleModel> createList(coreController core, string sqlCriteria) {
            return createList<addonTemplateRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<addonTemplateRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<addonTemplateRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<addonTemplateRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<addonTemplateRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static addonTemplateRuleModel createDefault(coreController core) {
            return createDefault<addonTemplateRuleModel>(core);
        }
    }
}
