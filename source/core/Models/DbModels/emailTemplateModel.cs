
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
    public class emailTemplateModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "email templates";
        public const string contentTableName = "ccemailtemplates";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public string BodyHTML { get; set; }
        public string Source { get; set; }
        //
        //====================================================================================================
        public static emailTemplateModel add(CoreController core) {
            return add<emailTemplateModel>(core);
        }
        //
        //====================================================================================================
        public static emailTemplateModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<emailTemplateModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailTemplateModel create(CoreController core, int recordId) {
            return create<emailTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static emailTemplateModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<emailTemplateModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailTemplateModel create(CoreController core, string recordGuid) {
            return create<emailTemplateModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static emailTemplateModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<emailTemplateModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailTemplateModel createByName(CoreController core, string recordName) {
            return createByName<emailTemplateModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static emailTemplateModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<emailTemplateModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<emailTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<emailTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<emailTemplateModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<emailTemplateModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<emailTemplateModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<emailTemplateModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<emailTemplateModel> createList(CoreController core, string sqlCriteria) {
            return createList<emailTemplateModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<emailTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<emailTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<emailTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<emailTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static emailTemplateModel createDefault(CoreController core) {
            return createDefault<emailTemplateModel>(core);
        }
    }
}
