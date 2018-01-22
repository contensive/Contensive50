
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
        public static emailTemplateModel add(coreController core) {
            return add<emailTemplateModel>(core);
        }
        //
        //====================================================================================================
        public static emailTemplateModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<emailTemplateModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailTemplateModel create(coreController core, int recordId) {
            return create<emailTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static emailTemplateModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<emailTemplateModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailTemplateModel create(coreController core, string recordGuid) {
            return create<emailTemplateModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static emailTemplateModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<emailTemplateModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailTemplateModel createByName(coreController core, string recordName) {
            return createByName<emailTemplateModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static emailTemplateModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<emailTemplateModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<emailTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<emailTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<emailTemplateModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<emailTemplateModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<emailTemplateModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<emailTemplateModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<emailTemplateModel> createList(coreController core, string sqlCriteria) {
            return createList<emailTemplateModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<emailTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<emailTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<emailTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<emailTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static emailTemplateModel createDefault(coreController core) {
            return createDefault<emailTemplateModel>(core);
        }
    }
}
