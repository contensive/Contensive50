
using System;

namespace Contensive.Models.Db {
    [System.Serializable]
    public class ConditionalEmailModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "conditional email";
        public const string contentTableNameLowerCase = "ccemail";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        //
        public DateTime conditionExpireDate { get; set; }
        public int conditionID { get; set; }
        public int conditionPeriod { get; set; }
        public bool sent { get; set; }
        public bool submitted { get; set; }

    }
}
