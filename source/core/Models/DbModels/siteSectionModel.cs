
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
    public class siteSectionModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "site sections";
        public const string contentTableName = "ccSections";
        private const string contentDataSource = "default"; 
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
        public static siteSectionModel add(CoreController core) {
            return add<siteSectionModel>(core);
        }
        //
        //====================================================================================================
        public static siteSectionModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<siteSectionModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static siteSectionModel create(CoreController core, int recordId) {
            return create<siteSectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static siteSectionModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<siteSectionModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static siteSectionModel create(CoreController core, string recordGuid) {
            return create<siteSectionModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static siteSectionModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<siteSectionModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static siteSectionModel createByName(CoreController core, string recordName) {
            return createByName<siteSectionModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static siteSectionModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<siteSectionModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<siteSectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<siteSectionModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<siteSectionModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<siteSectionModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<siteSectionModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<siteSectionModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<siteSectionModel> createList(CoreController core, string sqlCriteria) {
            return createList<siteSectionModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<siteSectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<siteSectionModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<siteSectionModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<siteSectionModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static siteSectionModel createDefault(CoreController core) {
            return createDefault<siteSectionModel>(core);
        }
    }
}
