
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
    public class pageContentModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "page content";
        public const string contentTableName = "ccpagecontent";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public bool AllowBrief { get; set; }
        public bool AllowChildListDisplay { get; set; }
        public bool AllowEmailPage { get; set; }
        public bool AllowFeedback { get; set; }
        public bool AllowHitNotification { get; set; }
        public bool AllowInChildLists { get; set; }
        public bool AllowInMenus { get; set; }
        public bool AllowLastModifiedFooter { get; set; }
        public bool AllowMessageFooter { get; set; }
        public bool AllowMetaContentNoFollow { get; set; }
        public bool AllowMoreInfo { get; set; }
        public bool AllowPrinterVersion { get; set; }
        public bool AllowReturnLinkDisplay { get; set; }
        public bool AllowReviewedFooter { get; set; }
        public bool AllowSeeAlso { get; set; }
        public int ArchiveParentID { get; set; }
        public bool BlockContent { get; set; }
        public bool BlockPage { get; set; }
        public int BlockSourceID { get; set; }
        public string BriefFilename { get; set; }
        public string ChildListInstanceOptions { get; set; }
        public int ChildListSortMethodID { get; set; }
        public bool ChildPagesFound { get; set; }
        public int Clicks { get; set; }
        public int ContactMemberID { get; set; }
        public int ContentPadding { get; set; }
        public fieldTypeHTMLFile Copyfilename { get; set; } = new fieldTypeHTMLFile();
        public string CustomBlockMessage { get; set; }
        public DateTime DateArchive { get; set; }
        public DateTime DateExpires { get; set; }
        public DateTime DateReviewed { get; set; }



        public string Headline { get; set; }
        public string imageFilename { get; set; }
        public bool IsSecure { get; set; }
        public string JSEndBody { get; set; }
        public string JSFilename { get; set; }
        public string JSHead { get; set; }
        public string JSOnLoad { get; set; }
        public string LinkAlias { get; set; }
        public string MenuHeadline { get; set; }
        public string metaDescription { get; set; }
        public string MetaKeywordList { get; set; }
        public string OtherHeadTags { get; set; }
        public string PageLink { get; set; }
        public string pageTitle { get; set; }
        public int ParentID { get; set; }
        public string ParentListName { get; set; }
        public DateTime PubDate { get; set; }
        public int RegistrationGroupID { get; set; }
        public int ReviewedBy { get; set; }
        public int TemplateID { get; set; }
        public int TriggerAddGroupID { get; set; }
        public int TriggerConditionGroupID { get; set; }
        public int TriggerConditionID { get; set; }
        public int TriggerRemoveGroupID { get; set; }
        public int TriggerSendSystemEmailID { get; set; }
        public int Viewings { get; set; }
        //
        //====================================================================================================
        public static pageContentModel add(coreController cpCore) {
            return add<pageContentModel>(cpCore);
        }
        //
        //====================================================================================================
        public static pageContentModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<pageContentModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static pageContentModel create(coreController cpCore, int recordId) {
            return create<pageContentModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static pageContentModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<pageContentModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static pageContentModel create(coreController cpCore, string recordGuid) {
            return create<pageContentModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static pageContentModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<pageContentModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static pageContentModel createByName(coreController cpCore, string recordName) {
            return createByName<pageContentModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static pageContentModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<pageContentModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<pageContentModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<pageContentModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<pageContentModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<pageContentModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<pageContentModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<pageContentModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<pageContentModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<pageContentModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<pageContentModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<pageContentModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<pageContentModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<pageContentModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static pageContentModel createDefault(coreController cpcore) {
            return createDefault<pageContentModel>(cpcore);
        }
    }
}
