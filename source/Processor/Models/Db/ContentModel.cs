﻿
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
        private const string contentDataSource = "default";
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
        public static ContentModel add(CoreController core) {
            return add<ContentModel>(core);
        }
        //
        //====================================================================================================
        public static ContentModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<ContentModel>(core, ref callersCacheNameList);
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
        public static ContentModel createByName(CoreController core, string recordName) {
            return createByName<ContentModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static ContentModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<ContentModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
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
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<ContentModel>(core, recordId);
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
        public static ContentModel createDefault(CoreController core) {
            return createDefault<ContentModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// pattern get a list of objects from this model
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="someCriteria"></param>
        /// <returns></returns>
        public static Dictionary<int, ContentModel> createDict(CoreController core, List<string> callersCacheNameList) {
            Dictionary<int, ContentModel> result = new Dictionary<int, ContentModel>();
            try {
                foreach (ContentModel content in createList(core, "")) {
                    result.Add(content.id, content);
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return result;
        }
    }
}