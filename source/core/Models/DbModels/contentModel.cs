
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
    public class contentModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "content";
        public const string contentTableName = "cccontent";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public bool AdminOnly { get; set; }
        public bool AllowAdd { get; set; }
        public bool AllowContentChildTool { get; set; }
        public bool AllowContentTracking { get; set; }
        public bool AllowDelete { get; set; }
        public bool AllowTopicRules { get; set; }
        public bool AllowWorkflowAuthoring { get; set; }
        public int AuthoringTableID { get; set; }
        public int ContentTableID { get; set; }
        public int DefaultSortMethodID { get; set; }
        public bool DeveloperOnly { get; set; }
        public string DropDownFieldList { get; set; }


        public int EditorGroupID { get; set; }

        public int IconHeight { get; set; }
        public string IconLink { get; set; }
        public int IconSprites { get; set; }
        public int IconWidth { get; set; }
        public int InstalledByCollectionID { get; set; }
        public bool IsBaseContent { get; set; }
        public int ParentID { get; set; }
        //
        //====================================================================================================
        public static contentModel add(coreController cpCore) {
            return add<contentModel>(cpCore);
        }
        //
        //====================================================================================================
        public static contentModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<contentModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static contentModel create(coreController cpCore, int recordId) {
            return create<contentModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static contentModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<contentModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static contentModel create(coreController cpCore, string recordGuid) {
            return create<contentModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static contentModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<contentModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static contentModel createByName(coreController cpCore, string recordName) {
            return createByName<contentModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static contentModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<contentModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<contentModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<contentModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<contentModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<contentModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<contentModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<contentModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<contentModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<contentModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<contentModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<contentModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<contentModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<contentModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static contentModel createDefault(coreController cpcore) {
            return createDefault<contentModel>(cpcore);
        }
        //
        //====================================================================================================
        /// <summary>
        /// pattern get a list of objects from this model
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="someCriteria"></param>
        /// <returns></returns>
        public static Dictionary<int, contentModel> createDict(coreController cpCore, List<string> callersCacheNameList) {
            Dictionary<int, contentModel> result = new Dictionary<int, contentModel>();
            try {
                foreach (contentModel content in createList(cpCore, "")) {
                    result.Add(content.id, content);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
    }
}
