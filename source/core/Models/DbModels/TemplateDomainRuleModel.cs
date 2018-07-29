
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
        public static TemplateDomainRuleModel add(CoreController core) {
            return add<TemplateDomainRuleModel>(core);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<TemplateDomainRuleModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(CoreController core, int recordId) {
            return create<TemplateDomainRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<TemplateDomainRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(CoreController core, string recordGuid) {
            return create<TemplateDomainRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<TemplateDomainRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel createByName(CoreController core, string recordName) {
            return createByName<TemplateDomainRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<TemplateDomainRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<TemplateDomainRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<TemplateDomainRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<TemplateDomainRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<TemplateDomainRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<TemplateDomainRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<TemplateDomainRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<TemplateDomainRuleModel> createList(CoreController core, string sqlCriteria) {
            return createList<TemplateDomainRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCacheSingleRecord<TemplateDomainRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<TemplateDomainRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<TemplateDomainRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<TemplateDomainRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel createDefault(CoreController core) {
            return createDefault<TemplateDomainRuleModel>(core);
        }
    }
}
