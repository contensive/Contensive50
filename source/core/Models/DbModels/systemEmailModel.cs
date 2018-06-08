
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
        public static systemEmailModel add(coreController core) {
            return add<systemEmailModel>(core);
        }
        //
        //====================================================================================================
        public static systemEmailModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<systemEmailModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static systemEmailModel create(coreController core, int recordId) {
            return create<systemEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static systemEmailModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<systemEmailModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static systemEmailModel create(coreController core, string recordGuid) {
            return create<systemEmailModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static systemEmailModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<systemEmailModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static systemEmailModel createByName(coreController core, string recordName) {
            return createByName<systemEmailModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static systemEmailModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<systemEmailModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<systemEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<systemEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<systemEmailModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<systemEmailModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<systemEmailModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<systemEmailModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<systemEmailModel> createList(coreController core, string sqlCriteria) {
            return createList<systemEmailModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<systemEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<systemEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<systemEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<systemEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static systemEmailModel createDefault(coreController core) {
            return createDefault<systemEmailModel>(core);
        }
    }
}
