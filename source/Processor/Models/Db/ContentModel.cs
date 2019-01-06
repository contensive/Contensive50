
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class ContentModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "content";
        public const string contentTableName = "cccontent";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public bool adminOnly { get; set; }
        public bool allowAdd { get; set; }
        public bool allowContentChildTool { get; set; }
        public bool allowContentTracking { get; set; }
        public bool allowDelete { get; set; }
        public bool allowTopicRules { get; set; }
        public bool allowWorkflowAuthoring { get; set; }
        public int authoringTableID { get; set; }
        public int contentTableID { get; set; }
        public int defaultSortMethodID { get; set; }
        public bool developerOnly { get; set; }
        public string dropDownFieldList { get; set; }
        public int editorGroupID { get; set; }
        public int iconHeight { get; set; }
        public string iconLink { get; set; }
        public int iconSprites { get; set; }
        public int iconWidth { get; set; }
        public int installedByCollectionID { get; set; }
        public bool isBaseContent { get; set; }
        public int parentID { get; set; }
        // 
        //====================================================================================================
        public static ContentModel addEmpty(CoreController core) {
            return addEmpty<ContentModel>(core);
        }
        //
        //====================================================================================================
        public static ContentModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<ContentModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static ContentModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<ContentModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ContentModel create(CoreController core, int recordId) {
            return create<ContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static ContentModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<ContentModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ContentModel create(CoreController core, string recordGuid) {
            return create<ContentModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static ContentModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<ContentModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        /// <summary>
        /// name is a unique field, so this is treated similar to guid
        /// </summary>
        /// <param name="core"></param>
        /// <param name="recordName"></param>
        /// <returns></returns>
        public static ContentModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<ContentModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static ContentModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<ContentModel>(core, recordName, ref callersCacheNameList );
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<ContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<ContentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<ContentModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<ContentModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<ContentModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<ContentModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<ContentModel> createList(CoreController core, string sqlCriteria) {
            return createList<ContentModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<ContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void invalidateTableCache(CoreController core) {
            invalidateTableCache<ContentModel>(core);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<ContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<ContentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<ContentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<ContentModel>(core);
        }
    }
}
