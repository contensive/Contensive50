
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
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
namespace Contensive.Processor.Models.Db {
    public class NavigatorEntryModel : DbModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Navigator Entries";
        public const string contentTableName = "ccMenuEntries";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
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
        public static NavigatorEntryModel addEmpty(CoreController core) {
            return addEmpty<NavigatorEntryModel>(core);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<NavigatorEntryModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<NavigatorEntryModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel create(CoreController core, int recordId) {
            return create<NavigatorEntryModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<NavigatorEntryModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel create(CoreController core, string recordGuid) {
            return create<NavigatorEntryModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<NavigatorEntryModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<NavigatorEntryModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<NavigatorEntryModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<NavigatorEntryModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<NavigatorEntryModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<NavigatorEntryModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<NavigatorEntryModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<NavigatorEntryModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<NavigatorEntryModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<NavigatorEntryModel> createList(CoreController core, string sqlCriteria) {
            return createList<NavigatorEntryModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<NavigatorEntryModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return DbModel.getRecordName<NavigatorEntryModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return DbModel.getRecordName<NavigatorEntryModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return DbModel.getRecordId<NavigatorEntryModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<NavigatorEntryModel>(core);
        }
    }
}
