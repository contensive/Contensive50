
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
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
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
        public static AddonCollectionModel addEmpty(CoreController core) {
            return addEmpty<AddonCollectionModel>(core);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<AddonCollectionModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel addDefault(CoreController core, Domain.MetaModel metaData, ref List<string> callersCacheNameList) {
            return addDefault<AddonCollectionModel>(core, metaData, ref callersCacheNameList);
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
        public static AddonCollectionModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<AddonCollectionModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<AddonCollectionModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
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
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<AddonCollectionModel>(core, recordId);
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
        //public static AddonCollection createDefault(CoreController core) {
        //    return createDefault<AddonCollection>(core);
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<AddonCollectionModel>(core);
        }
    }
}
