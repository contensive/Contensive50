
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
        public static conditionalEmailModel add(coreController cpCore) {
            return add<conditionalEmailModel>(cpCore);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<conditionalEmailModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel create(coreController cpCore, int recordId) {
            return create<conditionalEmailModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<conditionalEmailModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel create(coreController cpCore, string recordGuid) {
            return create<conditionalEmailModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<conditionalEmailModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel createByName(coreController cpCore, string recordName) {
            return createByName<conditionalEmailModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<conditionalEmailModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<conditionalEmailModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<conditionalEmailModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<conditionalEmailModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<conditionalEmailModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<conditionalEmailModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<conditionalEmailModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<conditionalEmailModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<conditionalEmailModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<conditionalEmailModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<conditionalEmailModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<conditionalEmailModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<conditionalEmailModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel createDefault(coreController cpcore) {
            return createDefault<conditionalEmailModel>(cpcore);
        }
    }
}
