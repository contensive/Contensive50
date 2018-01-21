
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
    public class systemEmailModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "system email";
        public const string contentTableName = "ccemail";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public bool AddLinkEID { get; set; }
        public bool AllowSpamFooter { get; set; }
        public string CopyFilename { get; set; }
        public int EmailTemplateID { get; set; }
        public string FromAddress { get; set; }
        public DateTime ScheduleDate { get; set; }
        public bool Sent { get; set; }
        public string Subject { get; set; }
        public bool Submitted { get; set; }
        public int TestMemberID { get; set; }
        //
        //====================================================================================================
        public static systemEmailModel add(coreController cpCore) {
            return add<systemEmailModel>(cpCore);
        }
        //
        //====================================================================================================
        public static systemEmailModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<systemEmailModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static systemEmailModel create(coreController cpCore, int recordId) {
            return create<systemEmailModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static systemEmailModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<systemEmailModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static systemEmailModel create(coreController cpCore, string recordGuid) {
            return create<systemEmailModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static systemEmailModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<systemEmailModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static systemEmailModel createByName(coreController cpCore, string recordName) {
            return createByName<systemEmailModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static systemEmailModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<systemEmailModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<systemEmailModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<systemEmailModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<systemEmailModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<systemEmailModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<systemEmailModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<systemEmailModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<systemEmailModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<systemEmailModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<systemEmailModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<systemEmailModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<systemEmailModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<systemEmailModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static systemEmailModel createDefault(coreController cpcore) {
            return createDefault<systemEmailModel>(cpcore);
        }
    }
}
