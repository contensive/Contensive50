
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class ContentFieldModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "content fields";
        public const string contentTableName = "ccfields";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public bool adminOnly { get; set; }
        public bool authorable { get; set; }
        public string caption { get; set; }
        public int contentID { get; set; }
        public string defaultValue { get; set; }
        public bool developerOnly { get; set; }
        public int editorAddonID { get; set; }
        public int editSortPriority { get; set; }
        public string editTab { get; set; }
        public bool htmlContent { get; set; }
        public int indexColumn { get; set; }
        public int indexSortDirection { get; set; }
        public int indexSortPriority { get; set; }
        public string indexWidth { get; set; }
        public int installedByCollectionID { get; set; }
        public int lookupContentID { get; set; }
        public string lookupList { get; set; }
        public int manyToManyContentID { get; set; }
        public int manyToManyRuleContentID { get; set; }
        public string manyToManyRulePrimaryField { get; set; }
        public string manyToManyRuleSecondaryField { get; set; }
        public int memberSelectGroupID { get; set; }
        public bool notEditable { get; set; }
        public bool password { get; set; }
        public bool readOnly { get; set; }
        public int redirectContentID { get; set; }
        public string redirectID { get; set; }
        public string redirectPath { get; set; }
        public bool required { get; set; }
        public bool rssDescriptionField { get; set; }
        public bool rssTitleField { get; set; }
        public bool scramble { get; set; }
        public bool textBuffered { get; set; }
        public int type { get; set; }
        public bool uniqueName { get; set; }
        public bool isBaseField { get; set; }
        // 
        //====================================================================================================
        public static ContentFieldModel addEmpty(CoreController core) {
            return addEmpty<ContentFieldModel>(core);
        }
        //
        //====================================================================================================
        public static ContentFieldModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<ContentFieldModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static ContentFieldModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<ContentFieldModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ContentFieldModel create(CoreController core, int recordId) {
            return create<ContentFieldModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static ContentFieldModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<ContentFieldModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ContentFieldModel create(CoreController core, string recordGuid) {
            return create<ContentFieldModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static ContentFieldModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<ContentFieldModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ContentFieldModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<ContentFieldModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static ContentFieldModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<ContentFieldModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<ContentFieldModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<ContentFieldModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<ContentFieldModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<ContentFieldModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<ContentFieldModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<ContentFieldModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<ContentFieldModel> createList(CoreController core, string sqlCriteria) {
            return createList<ContentFieldModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<ContentFieldModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void invalidateTableCache(CoreController core) {
            invalidateTableCache<ContentFieldModel>(core);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<ContentFieldModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<ContentFieldModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<ContentFieldModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<ContentFieldModel>(core);
        }
    }
}
