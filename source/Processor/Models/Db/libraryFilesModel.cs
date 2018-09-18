
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
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.Db {
    public class libraryFilesModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "library Files";
        public const string contentTableName = "cclibraryFiles";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public string altSizeList { get; set; }
        public string altText { get; set; }
        public int clicks { get; set; }
        public string description { get; set; }
        public string filename { get; set; }
        public int fileSize { get; set; }
        public int fileTypeId { get; set; }
        public int folderId { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        //
        //====================================================================================================
        public static libraryFilesModel add(CoreController core) {
            return add<libraryFilesModel>(core);
        }
        //
        //====================================================================================================
        public static libraryFilesModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<libraryFilesModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static libraryFilesModel create(CoreController core, int recordId) {
            return create<libraryFilesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static libraryFilesModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<libraryFilesModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static libraryFilesModel create(CoreController core, string recordGuid) {
            return create<libraryFilesModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static libraryFilesModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<libraryFilesModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static libraryFilesModel createByName(CoreController core, string recordName) {
            return createByName<libraryFilesModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static libraryFilesModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<libraryFilesModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<libraryFilesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<libraryFilesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<libraryFilesModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<libraryFilesModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<libraryFilesModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<libraryFilesModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<libraryFilesModel> createList(CoreController core, string sqlCriteria) {
            return createList<libraryFilesModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<libraryFilesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<libraryFilesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<libraryFilesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<libraryFilesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static libraryFilesModel createDefault(CoreController core) {
            return createDefault<libraryFilesModel>(core);
        }
    }
}
