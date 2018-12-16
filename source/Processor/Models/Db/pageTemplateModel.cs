
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
using static Contensive.Processor.Constants;
//
namespace Contensive.Processor.Models.Db {
    public class PageTemplateModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "page templates";
        public const string contentTableName = "cctemplates";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public string bodyHTML { get; set; }
        // Public Property BodyTag As String
        public bool isSecure { get; set; }
        // Public Property JSEndBody As String
        // Public Property JSFilename As String
        // Public Property JSHead As String
        // Public Property JSOnLoad As String
        // Public Property Link As String
        // Public Property MobileBodyHTML As String
        // Public Property OtherHeadTags As String
        //
        //====================================================================================================
        public static PageTemplateModel addEmpty(CoreController core) {
            return addEmpty<PageTemplateModel>(core);
        }
        //
        //====================================================================================================
        public static PageTemplateModel addDefault(CoreController core, Domain.CDefDomainModel cdef) {
            return addDefault<PageTemplateModel>(core, cdef);
        }
        //
        //====================================================================================================
        public static PageTemplateModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.CDefDomainModel cdef) {
            return addDefault<PageTemplateModel>(core, cdef, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static PageTemplateModel create(CoreController core, int recordId) {
            return create<PageTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static PageTemplateModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<PageTemplateModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static PageTemplateModel create(CoreController core, string recordGuid) {
            return create<PageTemplateModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static PageTemplateModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<PageTemplateModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static PageTemplateModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<PageTemplateModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static PageTemplateModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<PageTemplateModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<PageTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<PageTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<PageTemplateModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<PageTemplateModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<PageTemplateModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<PageTemplateModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<PageTemplateModel> createList(CoreController core, string sqlCriteria) {
            return createList<PageTemplateModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<PageTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<PageTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<PageTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<PageTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        //public static PageTemplateModel createDefault(CoreController core) {
        //    return createDefault<PageTemplateModel>(core);
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<PageTemplateModel>(core);
        }
    }
}
