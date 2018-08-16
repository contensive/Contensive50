
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
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.DbModels {
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
        public static contentModel add(CoreController core) {
            return add<contentModel>(core);
        }
        //
        //====================================================================================================
        public static contentModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<contentModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static contentModel create(CoreController core, int recordId) {
            return create<contentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static contentModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<contentModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static contentModel create(CoreController core, string recordGuid) {
            return create<contentModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static contentModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<contentModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static contentModel createByName(CoreController core, string recordName) {
            return createByName<contentModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static contentModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<contentModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<contentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<contentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<contentModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<contentModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<contentModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<contentModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<contentModel> createList(CoreController core, string sqlCriteria) {
            return createList<contentModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<contentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<contentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<contentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<contentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static contentModel createDefault(CoreController core) {
            return createDefault<contentModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// pattern get a list of objects from this model
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="someCriteria"></param>
        /// <returns></returns>
        public static Dictionary<int, contentModel> createDict(CoreController core, List<string> callersCacheNameList) {
            Dictionary<int, contentModel> result = new Dictionary<int, contentModel>();
            try {
                foreach (contentModel content in createList(core, "")) {
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
