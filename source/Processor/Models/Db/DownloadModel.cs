
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class DownloadModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "downloads";
        public const string contentTableName = "ccdownloads";
        public const string contentDataSource = "default"; 
        //
        //====================================================================================================
        // -- instance properties
        public FieldTypeTextFile filename { get; set; }
        public int requestedBy { get; set; }
        public DateTime dateRequested { get; set; }
        public DateTime dateCompleted { get; set; }
        public string resultMessage { get; set; }
        ////
        ////====================================================================================================
        //public static AddonModel addEmpty(CoreController core) {
        //    return addEmpty<AddonModel>(core);
        //}
        ////
        ////====================================================================================================
        //public static AddonModel addDefault(CoreController core, Domain.CDefModel cdef) {
        //    return addDefault<AddonModel>(core, metaData);
        //}
        ////
        ////====================================================================================================
        //public static AddonModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.CDefModel cdef) {
        //    return addDefault<AddonModel>(core, metaData, ref callersCacheNameList);
        //}
        ////
        ////====================================================================================================
        //public static DownloadModel create(CoreController core, int recordId) {
        //    return create<DownloadModel>(core, recordId);
        //}
        ////
        ////====================================================================================================
        //public static DownloadModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
        //    return create<DownloadModel>(core, recordId, ref callersCacheNameList);
        //}
        ////
        ////====================================================================================================
        //public static DownloadModel create(CoreController core, string recordGuid) {
        //    return create<DownloadModel>(core, recordGuid);
        //}
        ////
        ////====================================================================================================
        //public static DownloadModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
        //    return create<DownloadModel>(core, recordGuid, ref callersCacheNameList);
        //}
        ////
        ////====================================================================================================
        //public static DownloadModel createByUniqueName(CoreController core, string recordName) {
        //    return createByUniqueName<DownloadModel>(core, recordName);
        //}
        ////
        ////====================================================================================================
        //public static DownloadModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
        //    return createByUniqueName<DownloadModel>(core, recordName, ref callersCacheNameList);
        //}
        ////
        ////====================================================================================================
        //public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        ////
        ////====================================================================================================
        //public static void delete(CoreController core, int recordId) {
        //    delete<DownloadModel>(core, recordId);
        //}
        ////
        ////====================================================================================================
        //public static void delete(CoreController core, string ccGuid) {
        //    delete<DownloadModel>(core, ccGuid);
        //}
        ////
        ////====================================================================================================
        //public static List<DownloadModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
        //    return createList<DownloadModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        //}
        ////
        ////====================================================================================================
        //public static List<DownloadModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
        //    return createList<DownloadModel>(core, sqlCriteria, sqlOrderBy);
        //}
        ////
        ////====================================================================================================
        //public static List<DownloadModel> createList(CoreController core, string sqlCriteria) => createList<DownloadModel>(core, sqlCriteria);
        ////
        ////====================================================================================================
        //public static List<DownloadModel> createList(CoreController core) => createList<DownloadModel>(core);

        ////
        ////====================================================================================================
        //public static void invalidateRecordCache(CoreController core, int recordId) {
        //    invalidateRecordCache<DownloadModel>(core, recordId);
        //}
        ////
        ////====================================================================================================
        //public static void invalidateTableCache(CoreController core) {
        //    invalidateTableCache<DownloadModel>(core);
        //}
        ////
        ////====================================================================================================
        //public static string getRecordName(CoreController core, int recordId) {
        //    return BaseModel.getRecordName<DownloadModel>(core, recordId);
        //}
        ////
        ////====================================================================================================
        //public static string getRecordName(CoreController core, string ccGuid) {
        //    return BaseModel.getRecordName<DownloadModel>(core, ccGuid);
        //}
        ////
        ////====================================================================================================
        //public static int getRecordId(CoreController core, string ccGuid) {
        //    return BaseModel.getRecordId<DownloadModel>(core, ccGuid);
        //}
        //////
        //////====================================================================================================
        ////public static DownloadModel createDefault(CoreController core) {
        ////    return createDefault<DownloadModel>(core);
        ////}
        ////
        ////====================================================================================================
        ///// <summary>
        ///// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        ///// </summary>
        ///// <param name="core"></param>
        ///// <returns></returns>
        //public static string getTableInvalidationKey(CoreController core) {
        //    return getTableCacheKey<DownloadModel>(core);
        //}
    }
}
