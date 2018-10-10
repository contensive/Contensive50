
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
    public class LibraryFilesModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "library Files";
        public const string contentTableName = "cclibraryFiles";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
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
        public static LibraryFilesModel addEmpty(CoreController core) {
            return addEmpty<LibraryFilesModel>(core);
        }
        //
        //====================================================================================================
        public static LibraryFilesModel addDefault(CoreController core) {
            return addDefault<LibraryFilesModel>(core);
        }
        //
        //====================================================================================================
        public static LibraryFilesModel addDefault(CoreController core, ref List<string> callersCacheNameList) {
            return addDefault<LibraryFilesModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LibraryFilesModel create(CoreController core, int recordId) {
            return create<LibraryFilesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static LibraryFilesModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<LibraryFilesModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LibraryFilesModel create(CoreController core, string recordGuid) {
            return create<LibraryFilesModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static LibraryFilesModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<LibraryFilesModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LibraryFilesModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<LibraryFilesModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static LibraryFilesModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<LibraryFilesModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<LibraryFilesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<LibraryFilesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<LibraryFilesModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<LibraryFilesModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<LibraryFilesModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<LibraryFilesModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<LibraryFilesModel> createList(CoreController core, string sqlCriteria) {
            return createList<LibraryFilesModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<LibraryFilesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<LibraryFilesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<LibraryFilesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<LibraryFilesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static LibraryFilesModel createDefault(CoreController core) {
            return createDefault<LibraryFilesModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<LibraryFilesModel>(core);
        }
    }
}
