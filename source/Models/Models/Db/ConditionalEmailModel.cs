
using System;

namespace Contensive.Models.Db {
    [System.Serializable]
    public class ConditionalEmailModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("conditional email", "ccemail", "default", false);
        //
        //====================================================================================================
        //
        public DateTime? conditionExpireDate { get; set; }
        public int conditionID { get; set; }
        public int conditionPeriod { get; set; }
        public bool sent { get; set; }
        public bool submitted { get; set; }

    }
}
