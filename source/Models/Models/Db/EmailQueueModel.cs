
using System.Collections.Generic;

namespace Contensive.Models.Db {
    [System.Serializable]
    public class EmailQueueModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "email queue";
        public const string contentTableNameLowerCase = "ccemailqueue";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        //
        public string toAddress { get; set; }
        public string subject { get; set; }
        public string content { get; set; }
        public bool immediate { get; set;  }
        public int attempts { get; set; }
    }
}
