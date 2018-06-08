
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
    public class emailModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "email";
        public const string contentTableName = "ccemail";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public bool AddLinkEID { get; set; }
        public bool AllowSpamFooter { get; set; }
        public bool BlockSiteStyles { get; set; }
        public DateTime ConditionExpireDate { get; set; }
        public int ConditionID { get; set; }
        public int ConditionPeriod { get; set; }
        public baseModel.fieldTypeTextFile CopyFilename { get; set; }
        public int EmailTemplateID { get; set; }
        public int EmailWizardID { get; set; }
        public string FromAddress { get; set; }
        public string InlineStyles { get; set; }
        public DateTime LastSendTestDate { get; set; }
        public DateTime ScheduleDate { get; set; }
        public bool Sent { get; set; }
        public baseModel.fieldTypeCSSFile StylesFilename { get; set; }
        public string Subject { get; set; }
        public bool Submitted { get; set; }
        public int TestMemberID { get; set; }
        public bool ToAll { get; set; }

        //
        //====================================================================================================
        public static emailModel add(coreController core) {
            return add<emailModel>(core);
        }
        //
        //====================================================================================================
        public static emailModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<emailModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailModel create(coreController core, int recordId) {
            return create<emailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static emailModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<emailModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailModel create(coreController core, string recordGuid) {
            return create<emailModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static emailModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<emailModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailModel createByName(coreController core, string recordName) {
            return createByName<emailModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static emailModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<emailModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<emailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<emailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<emailModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<emailModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<emailModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<emailModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<emailModel> createList(coreController core, string sqlCriteria) {
            return createList<emailModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<emailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<emailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<emailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<emailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static emailModel createDefault(coreController core) {
            return createDefault<emailModel>(core);
        }
    }
}
