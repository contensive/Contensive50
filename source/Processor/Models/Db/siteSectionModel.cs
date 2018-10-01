
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
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.Db {
    public class SiteSectionModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "site sections";
        public const string contentTableName = "ccSections";
        public const string contentDataSource = "default"; 
        //
        //====================================================================================================
        // -- instance properties
        public bool BlockSection { get; set; }
        public string Caption { get; set; }
        public int ContentID { get; set; }
        public bool HideMenu { get; set; }
        public string JSEndBody { get; set; }
        public string JSFilename { get; set; }
        public string JSHead { get; set; }
        public string JSOnLoad { get; set; }
        public string MenuImageDownFilename { get; set; }
        public string menuImageDownOverFilename { get; set; }
        public string MenuImageFilename { get; set; }
        public string MenuImageOverFilename { get; set; }
        public int RootPageID { get; set; }
        public int TemplateID { get; set; }
        //
        //====================================================================================================
        public static SiteSectionModel add(CoreController core) {
            return add<SiteSectionModel>(core);
        }
        //
        //====================================================================================================
        public static SiteSectionModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<SiteSectionModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static SiteSectionModel create(CoreController core, int recordId) {
            return create<SiteSectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static SiteSectionModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<SiteSectionModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static SiteSectionModel create(CoreController core, string recordGuid) {
            return create<SiteSectionModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static SiteSectionModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<SiteSectionModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static SiteSectionModel createByName(CoreController core, string recordName) {
            return createByName<SiteSectionModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static SiteSectionModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<SiteSectionModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<SiteSectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<SiteSectionModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<SiteSectionModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<SiteSectionModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<SiteSectionModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<SiteSectionModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<SiteSectionModel> createList(CoreController core, string sqlCriteria) {
            return createList<SiteSectionModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<SiteSectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<SiteSectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<SiteSectionModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<SiteSectionModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static SiteSectionModel createDefault(CoreController core) {
            return createDefault<SiteSectionModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<SiteSectionModel>(core);
        }
    }
}
