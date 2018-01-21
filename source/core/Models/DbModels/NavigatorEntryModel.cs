
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
    public class NavigatorEntryModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Navigator Entries";
        public const string contentTableName = "ccMenuEntries";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int ParentID { get; set; }
        public string NavIconTitle { get; set; }
        public int NavIconType { get; set; }
        public int AddonID { get; set; }
        public bool AdminOnly { get; set; }
        public int ContentID { get; set; }
        public bool DeveloperOnly { get; set; }



        public int HelpAddonID { get; set; }
        public int HelpCollectionID { get; set; }
        public int InstalledByCollectionID { get; set; }
        public string LinkPage { get; set; }
        public bool NewWindow { get; set; }
        //
        //====================================================================================================
        public static NavigatorEntryModel add(coreController cpCore) {
            return add<NavigatorEntryModel>(cpCore);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<NavigatorEntryModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel create(coreController cpCore, int recordId) {
            return create<NavigatorEntryModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<NavigatorEntryModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel create(coreController cpCore, string recordGuid) {
            return create<NavigatorEntryModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<NavigatorEntryModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel createByName(coreController cpCore, string recordName) {
            return createByName<NavigatorEntryModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<NavigatorEntryModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<NavigatorEntryModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<NavigatorEntryModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<NavigatorEntryModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<NavigatorEntryModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<NavigatorEntryModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<NavigatorEntryModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<NavigatorEntryModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<NavigatorEntryModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<NavigatorEntryModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<NavigatorEntryModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<NavigatorEntryModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<NavigatorEntryModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel createDefault(coreController cpcore) {
            return createDefault<NavigatorEntryModel>(cpcore);
        }
    }
}
