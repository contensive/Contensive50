
using System;

namespace Contensive.Models.Db {
    [System.Serializable]
    public class EmailLogModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "email log";
        public const string contentTableNameLowerCase = "ccemaillog";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public DateTime? dateBlockExpires { get; set; }
        public int emailDropID { get; set; }
        public int emailID { get; set; }
        public string fromAddress { get; set; }
        public int logType { get; set; }
        public int memberID { get; set; }
        public string sendStatus { get; set; }
        public string subject { get; set; }
        public string toAddress { get; set; }
        public int visitID { get; set; }
        public string body { get; set; }
    }
}
