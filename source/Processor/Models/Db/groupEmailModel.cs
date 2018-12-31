
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class GroupEmailModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "group email";
        public const string contentTableName = "ccemail";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public bool addLinkEID { get; set; }
        public bool allowSpamFooter { get; set; }
        public bool blockSiteStyles { get; set; }
        public string copyFilename { get; set; }
        public int emailTemplateID { get; set; }
        public string fromAddress { get; set; }
        public string inlineStyles { get; set; }
        public DateTime lastSendTestDate { get; set; }
        public DateTime scheduleDate { get; set; }
        public bool sent { get; set; }
        public string stylesFilename { get; set; }
        public string subject { get; set; }
        public bool submitted { get; set; }
        public int testMemberID { get; set; }
        // 
        //====================================================================================================
        public static GroupEmailModel addEmpty(CoreController core) {
            return addEmpty<GroupEmailModel>(core);
        }
        //
        //====================================================================================================
        public static GroupEmailModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<GroupEmailModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static GroupEmailModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<GroupEmailModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static GroupEmailModel create(CoreController core, int recordId) {
            return create<GroupEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static GroupEmailModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<GroupEmailModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static GroupEmailModel create(CoreController core, string recordGuid) {
            return create<GroupEmailModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static GroupEmailModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<GroupEmailModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static GroupEmailModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<GroupEmailModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static GroupEmailModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<GroupEmailModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<GroupEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<GroupEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<GroupEmailModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<GroupEmailModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<GroupEmailModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<GroupEmailModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<GroupEmailModel> createList(CoreController core, string sqlCriteria) {
            return createList<GroupEmailModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<GroupEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<GroupEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<GroupEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<GroupEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        //public static GroupEmailModel createDefault(CoreController core) {
        //    return createDefault<GroupEmailModel>(core);
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<GroupEmailModel>(core);
        }
    }
}
