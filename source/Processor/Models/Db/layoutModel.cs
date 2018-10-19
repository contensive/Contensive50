
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
    public class LayoutModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "layouts";
        public const string contentTableName = "ccLayouts";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public BaseModel.FieldTypeHTMLFile layout { get; set; }
        public string stylesFilename { get; set; }
        // 
        //====================================================================================================
        public static LayoutModel addEmpty(CoreController core) {
            return addEmpty<LayoutModel>(core);
        }
        //
        //====================================================================================================
        public static LayoutModel addDefault(CoreController core, Domain.CDefModel cdef) {
            return addDefault<LayoutModel>(core, cdef);
        }
        //
        //====================================================================================================
        public static LayoutModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.CDefModel cdef) {
            return addDefault<LayoutModel>(core, cdef, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LayoutModel create(CoreController core, int recordId) {
            return create<LayoutModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static LayoutModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<LayoutModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LayoutModel create(CoreController core, string recordGuid) {
            return create<LayoutModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static LayoutModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<LayoutModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LayoutModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<LayoutModel>(core, recordName );
        }
        //
        //====================================================================================================
        public static LayoutModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<LayoutModel>(core, recordName, ref callersCacheNameList );
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<LayoutModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<LayoutModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<LayoutModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<LayoutModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<LayoutModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<LayoutModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<LayoutModel> createList(CoreController core, string sqlCriteria) {
            return createList<LayoutModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<LayoutModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<LayoutModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<LayoutModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<LayoutModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        //public static LayoutModel createDefault(CoreController core) {
        //    return createDefault<LayoutModel>(core);
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<LayoutModel>(core);
        }
    }
}
