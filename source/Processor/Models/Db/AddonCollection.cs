﻿
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class AddonCollection : BaseModel {
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
        public static AddonCollection addEmpty(CoreController core) {
            return addEmpty<AddonCollection>(core);
        }
        //
        //====================================================================================================
        public static AddonCollection addDefault(CoreController core) {
            return addDefault<AddonCollection>(core);
        }
        //
        //====================================================================================================
        public static AddonCollection addDefault(CoreController core, ref List<string> callersCacheNameList) {
            return addDefault<AddonCollection>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonCollection create(CoreController core, int recordId) {
            return create<AddonCollection>(core, recordId);
        }
        //
        //====================================================================================================
        public static AddonCollection create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<AddonCollection>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonCollection create(CoreController core, string recordGuid) {
            return create<AddonCollection>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static AddonCollection create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<AddonCollection>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonCollection createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<AddonCollection>(core, recordName);
        }
        //
        //====================================================================================================
        public static AddonCollection createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<AddonCollection>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<AddonCollection>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<AddonCollection>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<AddonCollection> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<AddonCollection>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<AddonCollection> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<AddonCollection>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<AddonCollection> createList(CoreController core, string sqlCriteria) {
            return createList<AddonCollection>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<AddonCollection>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<AddonCollection>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<AddonCollection>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<AddonCollection>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static AddonCollection createDefault(CoreController core) {
            return createDefault<AddonCollection>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<AddonCollection>(core);
        }
    }
}
