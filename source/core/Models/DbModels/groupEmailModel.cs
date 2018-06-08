
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
    public class groupEmailModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "group email";
        public const string contentTableName = "ccemail";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public bool AddLinkEID { get; set; }
        public bool AllowSpamFooter { get; set; }
        public bool BlockSiteStyles { get; set; }
        public string CopyFilename { get; set; }
        public int EmailTemplateID { get; set; }
        public string FromAddress { get; set; }
        public string InlineStyles { get; set; }
        public DateTime LastSendTestDate { get; set; }
        public DateTime ScheduleDate { get; set; }
        public bool Sent { get; set; }
        public string StylesFilename { get; set; }
        public string Subject { get; set; }
        public bool Submitted { get; set; }
        public int TestMemberID { get; set; }
        //
        //====================================================================================================
        public static groupEmailModel add(coreController core) {
            return add<groupEmailModel>(core);
        }
        //
        //====================================================================================================
        public static groupEmailModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<groupEmailModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static groupEmailModel create(coreController core, int recordId) {
            return create<groupEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static groupEmailModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<groupEmailModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static groupEmailModel create(coreController core, string recordGuid) {
            return create<groupEmailModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static groupEmailModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<groupEmailModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static groupEmailModel createByName(coreController core, string recordName) {
            return createByName<groupEmailModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static groupEmailModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<groupEmailModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<groupEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<groupEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<groupEmailModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<groupEmailModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<groupEmailModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<groupEmailModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<groupEmailModel> createList(coreController core, string sqlCriteria) {
            return createList<groupEmailModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<groupEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<groupEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<groupEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<groupEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static groupEmailModel createDefault(coreController core) {
            return createDefault<groupEmailModel>(core);
        }
    }
}
