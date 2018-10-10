
using System;
using System.Collections.Generic;
using System.Data;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class SitePropertyModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Site Property";
        public const string contentTableName = "ccsetup";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public string fieldValue { get; set; }
        // 
        //====================================================================================================
        public static SitePropertyModel addEmpty(CoreController core) {
            return addEmpty<SitePropertyModel>(core);
        }
        //
        //====================================================================================================
        public static SitePropertyModel addDefault(CoreController core) {
            return addDefault<SitePropertyModel>(core);
        }
        //
        //====================================================================================================
        public static SitePropertyModel addDefault(CoreController core, ref List<string> callersCacheNameList) {
            return addDefault<SitePropertyModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static SitePropertyModel create(CoreController core, int recordId) {
            return create<SitePropertyModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static SitePropertyModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<SitePropertyModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static SitePropertyModel create(CoreController core, string recordGuid) {
            return create<SitePropertyModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static SitePropertyModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<SitePropertyModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static SitePropertyModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<SitePropertyModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static SitePropertyModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<SitePropertyModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<SitePropertyModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<SitePropertyModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<SitePropertyModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<SitePropertyModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<SitePropertyModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<SitePropertyModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<SitePropertyModel> createList(CoreController core, string sqlCriteria) {
            return createList<SitePropertyModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<SitePropertyModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void invalidateTableCache(CoreController core) {
            invalidateTableCache<SitePropertyModel>(core);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<SitePropertyModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<SitePropertyModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<SitePropertyModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static SitePropertyModel createDefault(CoreController core) {
            return createDefault<SitePropertyModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<SitePropertyModel>(core);
        }
        //
        //
        //========================================================================
        /// <summary>
        /// get site property without a cache check, return as text. If not found, set and return default value
        /// </summary>
        /// <param name="PropertyName"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string getValue(CoreController core, string PropertyName, ref bool return_propertyFound) {
            string returnString = "";
            try {
                using (DataTable dt = core.db.executeQuery("select FieldValue from ccSetup where (active>0)and(name=" + core.db.encodeSQLText(PropertyName) + ") order by id")) {
                    if (dt.Rows.Count > 0) {
                        returnString = GenericController.encodeText(dt.Rows[0]["FieldValue"]);
                        return_propertyFound = true;
                    } else {
                        returnString = "";
                        return_propertyFound = false;
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnString;
        }
        //
        //====================================================================================================
        //
        public static Dictionary<string, string> getNameValueDict( CoreController core ) {
            var result = new Dictionary<string, string>();
            CsController cs = new CsController(core);
            if (cs.openSQL("select name,FieldValue from ccsetup where (active>0) order by id")) {
                do {
                    string name = cs.getText("name").Trim().ToLower();
                    if (!string.IsNullOrEmpty(name)) {
                        if (!result.ContainsKey(name)) {
                            result.Add(name, cs.getText("FieldValue"));
                        }
                    }
                    cs.goNext();
                } while (cs.ok());
            }
            cs.close();
            return result;
        }
    }
}
