
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
    public class TemplateDomainRuleModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Template Domain Rules";
        public const string contentTableName = "ccDomainTemplateRules";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int domainId { get; set; }
        public int templateId { get; set; }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel add(coreController core) {
            return add<TemplateDomainRuleModel>(core);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<TemplateDomainRuleModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(coreController core, int recordId) {
            return create<TemplateDomainRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<TemplateDomainRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(coreController core, string recordGuid) {
            return create<TemplateDomainRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<TemplateDomainRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel createByName(coreController core, string recordName) {
            return createByName<TemplateDomainRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<TemplateDomainRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<TemplateDomainRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<TemplateDomainRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<TemplateDomainRuleModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<TemplateDomainRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<TemplateDomainRuleModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<TemplateDomainRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<TemplateDomainRuleModel> createList(coreController core, string sqlCriteria) {
            return createList<TemplateDomainRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<TemplateDomainRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<TemplateDomainRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<TemplateDomainRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<TemplateDomainRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel createDefault(coreController core) {
            return createDefault<TemplateDomainRuleModel>(core);
        }
    }
}
