
using System;
//
namespace Contensive.Models.Db {
    [System.Serializable]
    public class RemoteQueryModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("remote queries", "ccremotequeries", "default", false);
        //
        //====================================================================================================
        public bool AllowInactiveRecords { get; set; }
        public int ContentID { get; set; }
        public string Criteria { get; set; }
        public int DataSourceID { get; set; }
        public DateTime? DateExpires { get; set; }
        public int MaxRows { get; set; }
        public int QueryTypeID { get; set; }
        public string RemoteKey { get; set; }
        public string SelectFieldList { get; set; }
        public string SortFieldList { get; set; }
        public string SQLQuery { get; set; }
        public int VisitID { get; set; }
    }
}
