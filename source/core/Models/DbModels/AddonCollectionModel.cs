
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
            return baseModel.getRecordName<AddonCollectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<AddonCollectionModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<AddonCollectionModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static AddonCollectionModel createDefault(CoreController core) {
            return createDefault<AddonCollectionModel>(core);
        }
    }
}
