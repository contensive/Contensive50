﻿
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
    public class NavigatorEntryModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Navigator Entries";
        public const string contentTableName = "ccMenuEntries";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int ParentID { get; set; }
        public string NavIconTitle { get; set; }
        public int NavIconType { get; set; }
        public int AddonID { get; set; }
        public bool AdminOnly { get; set; }
        public int ContentID { get; set; }
        public bool DeveloperOnly { get; set; }



        public int HelpAddonID { get; set; }
        public int HelpCollectionID { get; set; }
        public int InstalledByCollectionID { get; set; }
        public string LinkPage { get; set; }
        public bool NewWindow { get; set; }
        //
        //====================================================================================================
        public static NavigatorEntryModel add(CoreController core) {
            return add<NavigatorEntryModel>(core);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<NavigatorEntryModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel create(CoreController core, int recordId) {
            return create<NavigatorEntryModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<NavigatorEntryModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel create(CoreController core, string recordGuid) {
            return create<NavigatorEntryModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<NavigatorEntryModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel createByName(CoreController core, string recordName) {
            return createByName<NavigatorEntryModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<NavigatorEntryModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<NavigatorEntryModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<NavigatorEntryModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<NavigatorEntryModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<NavigatorEntryModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<NavigatorEntryModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<NavigatorEntryModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<NavigatorEntryModel> createList(CoreController core, string sqlCriteria) {
            return createList<NavigatorEntryModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<NavigatorEntryModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<NavigatorEntryModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<NavigatorEntryModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<NavigatorEntryModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static NavigatorEntryModel createDefault(CoreController core) {
            return createDefault<NavigatorEntryModel>(core);
        }
    }
}
