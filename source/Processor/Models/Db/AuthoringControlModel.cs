
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class AuthoringControlModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        //
        public const string contentName = "Authoring Controls";
        public const string contentTableName = "ccAuthoringControls";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        //
        /// <summary>
        /// tableId/recordId
        /// </summary>
        public string contentRecordKey { get; set; }
        /// <summary>
        /// type of authoring control
        /// </summary>
        public int controlType { get; set; }
        /// <summary>
        /// date time when this lock expires
        /// </summary>
        public DateTime DateExpires { get; set; }
        //
        //====================================================================================================
        //
        public static AuthoringControlModel addEmpty(CoreController core) => addEmpty<AuthoringControlModel>(core);
        //
        //====================================================================================================
        //
        public static AuthoringControlModel addDefault(CoreController core, Domain.MetaModel metaData) => addDefault<AuthoringControlModel>(core, metaData);
        //
        //====================================================================================================
        //
        public static AuthoringControlModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) => addDefault<AuthoringControlModel>(core, metaData, ref callersCacheNameList);
        //
        //====================================================================================================
        //
        public static AuthoringControlModel create(CoreController core, int recordId) => create<AuthoringControlModel>(core, recordId);
        //
        //====================================================================================================
        //
        public static AuthoringControlModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) => create<AuthoringControlModel>(core, recordId, ref callersCacheNameList);
        //
        //====================================================================================================
        //
        public static AuthoringControlModel create(CoreController core, string recordGuid) => create<AuthoringControlModel>(core, recordGuid);
        //
        //====================================================================================================
        //
        public static AuthoringControlModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) => create<AuthoringControlModel>(core, recordGuid, ref callersCacheNameList);
        //
        //====================================================================================================
        //
        public static AuthoringControlModel createByUniqueName(CoreController core, string recordName) => createByUniqueName<AuthoringControlModel>(core, recordName);
        //
        //====================================================================================================
        public static AuthoringControlModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) => createByUniqueName<AuthoringControlModel>(core, recordName, ref callersCacheNameList);
        //
        //====================================================================================================
        //
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        //
        public static void delete(CoreController core, int recordId) => delete<AuthoringControlModel>(core, recordId);
        //
        //====================================================================================================
        //
        public static void delete(CoreController core, string ccGuid) => delete<AuthoringControlModel>(core, ccGuid);
        //
        //====================================================================================================
        //
        public static List<AuthoringControlModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) => createList<AuthoringControlModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        //
        //====================================================================================================
        //
        public static List<AuthoringControlModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) => createList<AuthoringControlModel>(core, sqlCriteria, sqlOrderBy);
        //
        //====================================================================================================
        //
        public static List<AuthoringControlModel> createList(CoreController core, string sqlCriteria) => createList<AuthoringControlModel>(core, sqlCriteria);
        //
        //====================================================================================================
        //
        public static void invalidateRecordCache(CoreController core, int recordId) => invalidateRecordCache<AuthoringControlModel>(core, recordId);
        //
        //====================================================================================================
        //
        public static void invalidateTableCache(CoreController core) => invalidateTableCache<AuthoringControlModel>(core);
        //
        //====================================================================================================
        //
        public static string getRecordName(CoreController core, int recordId) => BaseModel.getRecordName<AuthoringControlModel>(core, recordId);
        //
        //====================================================================================================
        //
        public static string getRecordName(CoreController core, string ccGuid) => BaseModel.getRecordName<AuthoringControlModel>(core, ccGuid);
        //
        //====================================================================================================
        //
        public static int getRecordId(CoreController core, string ccGuid) => BaseModel.getRecordId<AuthoringControlModel>(core, ccGuid);
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) => getTableCacheKey<AuthoringControlModel>(core);
        //
        //====================================================================================================
        //
        public static void deleteSelection(CoreController core, string sqlCriteria) => BaseModel.deleteSelection<AuthoringControlModel>(core, sqlCriteria);
    }
}
