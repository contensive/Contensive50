
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
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public bool allowBrief { get; set; }
        public bool allowChildListDisplay { get; set; }
        public bool allowFeedback { get; set; }
        public bool allowHitNotification { get; set; }
        public bool allowInChildLists { get; set; }
        public bool allowInMenus { get; set; }
        public bool allowLastModifiedFooter { get; set; }
        public bool allowMessageFooter { get; set; }
        public bool allowMetaContentNoFollow { get; set; }
        public bool allowMoreInfo { get; set; }
        //// deprecated 20180701
        //public bool AllowPrinterVersion { get; set; }
        public bool allowReturnLinkDisplay { get; set; }
        public bool allowReviewedFooter { get; set; }
        public bool allowSeeAlso { get; set; }
        public int archiveParentID { get; set; }
        public bool blockContent { get; set; }
        public bool blockPage { get; set; }
        public int blockSourceID { get; set; }
        public string briefFilename { get; set; }
        public string childListInstanceOptions { get; set; }
        public int childListSortMethodID { get; set; }
        public bool childPagesFound { get; set; }
        public int clicks { get; set; }
        public int contactMemberID { get; set; }
        public int contentPadding { get; set; }
        public FieldTypeHTMLFile copyfilename { get; set; } = new FieldTypeHTMLFile();
        public string customBlockMessage { get; set; }
        public DateTime dateArchive { get; set; }
        public DateTime dateExpires { get; set; }
        public DateTime dateReviewed { get; set; }



        public string headline { get; set; }
        public string imageFilename { get; set; }
        public bool isSecure { get; set; }
        public string jSEndBody { get; set; }
        public string jSFilename { get; set; }
        public string jSHead { get; set; }
        public string jSOnLoad { get; set; }
        public string linkAlias { get; set; }
        public string menuHeadline { get; set; }
        public string metaDescription { get; set; }
        public string metaKeywordList { get; set; }
        public string otherHeadTags { get; set; }
        public string pageLink { get; set; }
        public string pageTitle { get; set; }
        public int parentID { get; set; }
        public string parentListName { get; set; }
        public DateTime pubDate { get; set; }
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
        public static PageContentModel addEmpty(CoreController core) {
            return addEmpty<PageContentModel>(core);
        }
        //
        //====================================================================================================
        public static PageContentModel addDefault(CoreController core) {
            return addDefault<PageContentModel>(core);
        }
        //
        //====================================================================================================
        public static PageContentModel addDefault(CoreController core, ref List<string> callersCacheNameList) {
            return addDefault<PageContentModel>(core, ref callersCacheNameList);
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
        public static PageContentModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<PageContentModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static PageContentModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<PageContentModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
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
