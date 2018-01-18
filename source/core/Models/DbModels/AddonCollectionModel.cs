
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
        public static AddonCollectionModel add(coreClass cpCore) {
            return add<AddonCollectionModel>(cpCore);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel add(coreClass cpCore, ref List<string> callersCacheNameList) {
            return add<AddonCollectionModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel create(coreClass cpCore, int recordId) {
            return create<AddonCollectionModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<AddonCollectionModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel create(coreClass cpCore, string recordGuid) {
            return create<AddonCollectionModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<AddonCollectionModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel createByName(coreClass cpCore, string recordName) {
            return createByName<AddonCollectionModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<AddonCollectionModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreClass cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, int recordId) {
            delete<AddonCollectionModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, string ccGuid) {
            delete<AddonCollectionModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<AddonCollectionModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<AddonCollectionModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<AddonCollectionModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<AddonCollectionModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<AddonCollectionModel> createList(coreClass cpCore, string sqlCriteria) {
            return createList<AddonCollectionModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreClass cpCore, int recordId) {
            invalidateCacheSingleRecord<AddonCollectionModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, int recordId) {
            return baseModel.getRecordName<AddonCollectionModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordName<AddonCollectionModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordId<AddonCollectionModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel createDefault(coreClass cpcore) {
            return createDefault<AddonCollectionModel>(cpcore);
        }
    }
}
