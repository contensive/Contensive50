
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
    public class conditionalEmailModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "conditional email";
        public const string contentTableName = "ccemail";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        //
        public DateTime ConditionExpireDate { get; set; }
        public int ConditionID { get; set; }
        public int ConditionPeriod { get; set; }
        public bool Sent { get; set; }
        public bool Submitted { get; set; }
        //
        //====================================================================================================
        public static conditionalEmailModel add(coreController core) {
            return add<conditionalEmailModel>(core);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<conditionalEmailModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel create(coreController core, int recordId) {
            return create<conditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<conditionalEmailModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel create(coreController core, string recordGuid) {
            return create<conditionalEmailModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<conditionalEmailModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel createByName(coreController core, string recordName) {
            return createByName<conditionalEmailModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<conditionalEmailModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<conditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<conditionalEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<conditionalEmailModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<conditionalEmailModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<conditionalEmailModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<conditionalEmailModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<conditionalEmailModel> createList(coreController core, string sqlCriteria) {
            return createList<conditionalEmailModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<conditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<conditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<conditionalEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<conditionalEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel createDefault(coreController core) {
            return createDefault<conditionalEmailModel>(core);
        }
    }
}
