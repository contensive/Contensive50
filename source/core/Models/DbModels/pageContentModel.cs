
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
        public bool AllowFeedback { get; set; }
        public bool AllowHitNotification { get; set; }
        public bool AllowInChildLists { get; set; }
        public bool AllowInMenus { get; set; }
        public bool AllowLastModifiedFooter { get; set; }
        public bool AllowMessageFooter { get; set; }
        public bool AllowMetaContentNoFollow { get; set; }
        public bool AllowMoreInfo { get; set; }
        //// deprecated 20180701
        //public bool AllowPrinterVersion { get; set; }
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
        public static pageContentModel add(CoreController core) {
            return add<pageContentModel>(core);
        }
        //
        //====================================================================================================
        public static pageContentModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<pageContentModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static pageContentModel create(CoreController core, int recordId) {
            return create<pageContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static pageContentModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<pageContentModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static pageContentModel create(CoreController core, string recordGuid) {
            return create<pageContentModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static pageContentModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<pageContentModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static pageContentModel createByName(CoreController core, string recordName) {
            return createByName<pageContentModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static pageContentModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<pageContentModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<pageContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<pageContentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<pageContentModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<pageContentModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<pageContentModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<pageContentModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<pageContentModel> createList(CoreController core, string sqlCriteria) {
            return createList<pageContentModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCacheSingleRecord<pageContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<pageContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<pageContentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<pageContentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static pageContentModel createDefault(CoreController core) {
            return createDefault<pageContentModel>(core);
        }
    }
}
