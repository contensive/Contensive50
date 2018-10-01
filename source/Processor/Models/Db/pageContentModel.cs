
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
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.Db {
    public class PageContentModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "page content";
        public const string contentTableName = "ccpagecontent";
        public const string contentDataSource = "default";
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
        public FieldTypeHTMLFile Copyfilename { get; set; } = new FieldTypeHTMLFile();
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
        public static PageContentModel add(CoreController core) {
            return add<PageContentModel>(core);
        }
        //
        //====================================================================================================
        public static PageContentModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<PageContentModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static PageContentModel create(CoreController core, int recordId) {
            return create<PageContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static PageContentModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<PageContentModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static PageContentModel create(CoreController core, string recordGuid) {
            return create<PageContentModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static PageContentModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<PageContentModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static PageContentModel createByName(CoreController core, string recordName) {
            return createByName<PageContentModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static PageContentModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<PageContentModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<PageContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<PageContentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<PageContentModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<PageContentModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<PageContentModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<PageContentModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<PageContentModel> createList(CoreController core, string sqlCriteria) {
            return createList<PageContentModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<PageContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void invalidateTableCache(CoreController core) {
            invalidateTableCache<PageContentModel>(core);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<PageContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<PageContentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<PageContentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static PageContentModel createDefault(CoreController core) {
            return createDefault<PageContentModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<PageContentModel>(core);
        }
    }
}
