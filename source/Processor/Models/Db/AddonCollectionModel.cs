
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class AddonCollectionModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Add-on Collections";
        public const string contentTableName = "ccAddonCollections";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public bool blockNavigatorNode { get; set; }
        public string contentFileList { get; set; }
        public string dataRecordList { get; set; }
        public string execFileList { get; set; }
        public string help { get; set; }
        public string helpLink { get; set; }
        public DateTime lastChangeDate { get; set; }
        public string otherXML { get; set; }
        public bool system { get; set; }
        public bool updatable { get; set; }
        public string wwwFileList { get; set; }
        //
        //====================================================================================================
        public static AddonCollectionModel add(CoreController core) {
            return add<AddonCollectionModel>(core);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<AddonCollectionModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel create(CoreController core, int recordId) {
            return create<AddonCollectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<AddonCollectionModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel create(CoreController core, string recordGuid) {
            return create<AddonCollectionModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<AddonCollectionModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel createByName(CoreController core, string recordName) {
            return createByName<AddonCollectionModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<AddonCollectionModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<AddonCollectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<AddonCollectionModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<AddonCollectionModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<AddonCollectionModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<AddonCollectionModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<AddonCollectionModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<AddonCollectionModel> createList(CoreController core, string sqlCriteria) {
            return createList<AddonCollectionModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<AddonCollectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<AddonCollectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<AddonCollectionModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<AddonCollectionModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel createDefault(CoreController core) {
            return createDefault<AddonCollectionModel>(core);
        }
    }
}
