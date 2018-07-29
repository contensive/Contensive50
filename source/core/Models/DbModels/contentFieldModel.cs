
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
    public class contentFieldModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "content fields";
        public const string contentTableName = "ccfields";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public bool AdminOnly { get; set; }
        public bool Authorable { get; set; }
        public string Caption { get; set; }
        public int ContentID { get; set; }
        public bool createResourceFilesOnRoot { get; set; }
        public string DefaultValue { get; set; }
        public bool DeveloperOnly { get; set; }
        public int editorAddonID { get; set; }
        public int EditSortPriority { get; set; }
        public string EditTab { get; set; }
        public bool HTMLContent { get; set; }
        public int IndexColumn { get; set; }
        public int IndexSortDirection { get; set; }
        public int IndexSortPriority { get; set; }
        public string IndexWidth { get; set; }
        public int InstalledByCollectionID { get; set; }
        //Public Property IsBaseField As Boolean
        public int LookupContentID { get; set; }
        public string LookupList { get; set; }
        public int ManyToManyContentID { get; set; }
        public int ManyToManyRuleContentID { get; set; }
        public string ManyToManyRulePrimaryField { get; set; }
        public string ManyToManyRuleSecondaryField { get; set; }
        public int MemberSelectGroupID { get; set; }
        public bool NotEditable { get; set; }
        public bool Password { get; set; }
        public string prefixForRootResourceFiles { get; set; }
        public bool ReadOnly { get; set; }
        public int RedirectContentID { get; set; }
        public string RedirectID { get; set; }
        public string RedirectPath { get; set; }
        public bool Required { get; set; }
        public bool RSSDescriptionField { get; set; }
        public bool RSSTitleField { get; set; }
        public bool Scramble { get; set; }
        public bool TextBuffered { get; set; }
        public int Type { get; set; }
        public bool UniqueName { get; set; }
        //
        //====================================================================================================
        public static contentFieldModel add(CoreController core) {
            return add<contentFieldModel>(core);
        }
        //
        //====================================================================================================
        public static contentFieldModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<contentFieldModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static contentFieldModel create(CoreController core, int recordId) {
            return create<contentFieldModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static contentFieldModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<contentFieldModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static contentFieldModel create(CoreController core, string recordGuid) {
            return create<contentFieldModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static contentFieldModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<contentFieldModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static contentFieldModel createByName(CoreController core, string recordName) {
            return createByName<contentFieldModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static contentFieldModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<contentFieldModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<contentFieldModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<contentFieldModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<contentFieldModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<contentFieldModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<contentFieldModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<contentFieldModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<contentFieldModel> createList(CoreController core, string sqlCriteria) {
            return createList<contentFieldModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCacheSingleRecord<contentFieldModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<contentFieldModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<contentFieldModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<contentFieldModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static contentFieldModel createDefault(CoreController core) {
            return createDefault<contentFieldModel>(core);
        }
    }
}
