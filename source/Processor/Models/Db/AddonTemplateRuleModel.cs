﻿
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class AddonTemplateRuleModel : BaseModel {
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
        public static AddonTemplateRuleModel add(CoreController core) {
            return add<AddonTemplateRuleModel>(core);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<AddonTemplateRuleModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel create(CoreController core, int recordId) {
            return create<AddonTemplateRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<AddonTemplateRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel create(CoreController core, string recordGuid) {
            return create<AddonTemplateRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<AddonTemplateRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel createByName(CoreController core, string recordName) {
            return createByName<AddonTemplateRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<AddonTemplateRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<AddonTemplateRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<AddonTemplateRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<AddonTemplateRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<AddonTemplateRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<AddonTemplateRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<AddonTemplateRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<AddonTemplateRuleModel> createList(CoreController core, string sqlCriteria) {
            return createList<AddonTemplateRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<AddonTemplateRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<AddonTemplateRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<AddonTemplateRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<AddonTemplateRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel createDefault(CoreController core) {
            return createDefault<AddonTemplateRuleModel>(core);
        }
    }
}