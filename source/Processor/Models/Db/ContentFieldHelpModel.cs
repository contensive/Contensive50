
using System.Linq;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class ContentFieldHelpModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "content field help";
        public const string contentTableName = "ccFieldHelp";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int fieldID { get; set; }
        public string helpCustom { get; set; }
        public string helpDefault { get; set; }
        // 
        //====================================================================================================
        public static ContentFieldHelpModel addEmpty(CoreController core) {
            return addEmpty<ContentFieldHelpModel>(core);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel addDefault(CoreController core) {
            return addDefault<ContentFieldHelpModel>(core);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel addDefault(CoreController core, ref List<string> callersCacheNameList) {
            return addDefault<ContentFieldHelpModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel create(CoreController core, int recordId) {
            return create<ContentFieldHelpModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<ContentFieldHelpModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel create(CoreController core, string recordGuid) {
            return create<ContentFieldHelpModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<ContentFieldHelpModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<ContentFieldHelpModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<ContentFieldHelpModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<ContentFieldHelpModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<ContentFieldHelpModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<ContentFieldHelpModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<ContentFieldHelpModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<ContentFieldHelpModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<ContentFieldHelpModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<ContentFieldHelpModel> createList(CoreController core, string sqlCriteria) {
            return createList<ContentFieldHelpModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<ContentFieldHelpModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<ContentFieldHelpModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<ContentFieldHelpModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<ContentFieldHelpModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        //public static ContentFieldHelpModel createDefault(CoreController core) {
        //    return createDefault<ContentFieldHelpModel>(core);
        //}
        //
        //====================================================================================================
        /// <summary>
        /// get the first field help for a field, digard the rest
        /// </summary>
        /// <param name="core"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public static ContentFieldHelpModel createByFieldId(CoreController core, int fieldId) {
            List<ContentFieldHelpModel> helpList = createList(core, "(fieldId=" + fieldId + ")", "id");
            if (helpList.Count == 0) {
                return null;
            } else {
                return helpList.First();
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<ContentFieldHelpModel>(core);
        }
    }
}
