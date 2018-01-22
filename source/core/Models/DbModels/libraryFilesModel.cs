
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
    public class libraryFilesModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "library Files";
        public const string contentTableName = "cclibraryFiles";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public string AltSizeList { get; set; }
        public string AltText { get; set; }
        public int Clicks { get; set; }
        public string Description { get; set; }
        public string Filename { get; set; }
        public int FileSize { get; set; }
        public int FileTypeID { get; set; }
        public int FolderID { get; set; }
        public int pxHeight { get; set; }
        public int pxWidth { get; set; }
        //
        //====================================================================================================
        public static libraryFilesModel add(coreController core) {
            return add<libraryFilesModel>(core);
        }
        //
        //====================================================================================================
        public static libraryFilesModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<libraryFilesModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static libraryFilesModel create(coreController core, int recordId) {
            return create<libraryFilesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static libraryFilesModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<libraryFilesModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static libraryFilesModel create(coreController core, string recordGuid) {
            return create<libraryFilesModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static libraryFilesModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<libraryFilesModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static libraryFilesModel createByName(coreController core, string recordName) {
            return createByName<libraryFilesModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static libraryFilesModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<libraryFilesModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<libraryFilesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<libraryFilesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<libraryFilesModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<libraryFilesModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<libraryFilesModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<libraryFilesModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<libraryFilesModel> createList(coreController core, string sqlCriteria) {
            return createList<libraryFilesModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<libraryFilesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<libraryFilesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<libraryFilesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<libraryFilesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static libraryFilesModel createDefault(coreController core) {
            return createDefault<libraryFilesModel>(core);
        }
    }
}
