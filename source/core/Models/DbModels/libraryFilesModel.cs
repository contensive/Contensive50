
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
        public static libraryFilesModel add(coreController cpCore) {
            return add<libraryFilesModel>(cpCore);
        }
        //
        //====================================================================================================
        public static libraryFilesModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<libraryFilesModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static libraryFilesModel create(coreController cpCore, int recordId) {
            return create<libraryFilesModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static libraryFilesModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<libraryFilesModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static libraryFilesModel create(coreController cpCore, string recordGuid) {
            return create<libraryFilesModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static libraryFilesModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<libraryFilesModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static libraryFilesModel createByName(coreController cpCore, string recordName) {
            return createByName<libraryFilesModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static libraryFilesModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<libraryFilesModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<libraryFilesModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<libraryFilesModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<libraryFilesModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<libraryFilesModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<libraryFilesModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<libraryFilesModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<libraryFilesModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<libraryFilesModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<libraryFilesModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<libraryFilesModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<libraryFilesModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<libraryFilesModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static libraryFilesModel createDefault(coreController cpcore) {
            return createDefault<libraryFilesModel>(cpcore);
        }
    }
}
