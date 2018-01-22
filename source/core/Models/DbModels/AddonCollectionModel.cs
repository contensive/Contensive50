
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
    public class AddonCollectionModel : baseModel {
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
        public string ContentFileList { get; set; }
        public string DataRecordList { get; set; }
        public string ExecFileList { get; set; }
        public string Help { get; set; }
        public string helpLink { get; set; }
        public DateTime LastChangeDate { get; set; }
        public string OtherXML { get; set; }
        public bool System { get; set; }
        public bool Updatable { get; set; }
        public string wwwFileList { get; set; }
        //
        //====================================================================================================
        public static AddonCollectionModel add(coreController core) {
            return add<AddonCollectionModel>(core);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<AddonCollectionModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel create(coreController core, int recordId) {
            return create<AddonCollectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<AddonCollectionModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel create(coreController core, string recordGuid) {
            return create<AddonCollectionModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<AddonCollectionModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel createByName(coreController core, string recordName) {
            return createByName<AddonCollectionModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<AddonCollectionModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<AddonCollectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<AddonCollectionModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<AddonCollectionModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<AddonCollectionModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<AddonCollectionModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<AddonCollectionModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<AddonCollectionModel> createList(coreController core, string sqlCriteria) {
            return createList<AddonCollectionModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<AddonCollectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<AddonCollectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<AddonCollectionModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<AddonCollectionModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel createDefault(coreController core) {
            return createDefault<AddonCollectionModel>(core);
        }
    }
}
