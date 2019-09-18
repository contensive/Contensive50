
using System;
//
namespace Contensive.Models.Db {
    [System.Serializable]
    public class RemoteQueryModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "remote queries";
        public const string contentTableNameLowerCase = "ccremotequeries";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        //
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
