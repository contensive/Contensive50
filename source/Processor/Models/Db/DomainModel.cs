
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class DomainModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "domains";
        public const string contentTableName = "ccdomains";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        /// <summary>
        /// the template used for this domain. Can be overridden by page
        /// </summary>
        public int defaultTemplateId { get; set; }
        /// <summary>
        /// forward traffic to this domain to another domain
        /// </summary>
        public int forwardDomainId { get; set; }
        /// <summary>
        /// forward traffic to this url
        /// </summary>
        public string forwardUrl { get; set; }
        /// <summary>
        /// set response header to noFollow for this domain
        /// </summary>
        public bool noFollow { get; set; }
        /// <summary>
        /// for this domain, display this page not found
        /// </summary>
        public int pageNotFoundPageId { get; set; }
        /// <summary>
        /// for this domain, the home/landing page
        /// </summary>
        public int rootPageId { get; set; }
        /// <summary>
        /// determines the type of response
        /// </summary>
        public int typeId { get; set; }
        /// <summary>
        /// true if this domain has received traffic
        /// </summary>
        public bool visited { get; set; }
        /// <summary>
        /// the default code to execute for this domain
        /// </summary>
        public int defaultRouteId { get; set; }
        /// <summary>
        /// if true, add the default CORS headers
        /// </summary>
        public bool allowCORS { get; set; }
        //
        //====================================================================================================
        public static DomainModel addEmpty(CoreController core) {
            return addEmpty<DomainModel>(core);
        }
        //
        //====================================================================================================
        public static DomainModel addDefault(CoreController core, Domain.MetaModel cdef) {
            return addDefault<DomainModel>(core, cdef);
        }
        //
        //====================================================================================================
        public static DomainModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel cdef) {
            return addDefault<DomainModel>(core, cdef, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static DomainModel create(CoreController core, int recordId) {
            return create<DomainModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static DomainModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<DomainModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static DomainModel create(CoreController core, string recordGuid) {
            return create<DomainModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static DomainModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<DomainModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static DomainModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<DomainModel>(core, recordName );
        }
        //
        //====================================================================================================
        public static DomainModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<DomainModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<DomainModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<DomainModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<DomainModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<DomainModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<DomainModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<DomainModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<DomainModel> createList(CoreController core, string sqlCriteria) {
            return createList<DomainModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<DomainModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<DomainModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<DomainModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<DomainModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        //public static DomainModel createDefault(CoreController core) {
        //    return createDefault<DomainModel>(core);
        //}
        //
        //====================================================================================================
        public static Dictionary<string,DomainModel> createDictionary(CoreController core, string sqlCriteria) {
            var result = new Dictionary<string, DomainModel> { };
            foreach (var domain in createList(core, sqlCriteria)) {
                if (!result.ContainsKey(domain.name.ToLowerInvariant())) {
                    result.Add(domain.name.ToLowerInvariant(), domain);
                }
            }
            return result;
        }
        //
        public enum DomainTypeEnum {
            Normal = 1,
            ForwardToUrl = 2,
            ForwardToReplacementDomain = 3
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<DomainModel>(core);
        }
    }
}
