
using System;

namespace Contensive.Models.Db {
    [System.Serializable]
    public class SystemEmailModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("system email", "ccemail", "default", true);
        //
        //====================================================================================================
        public bool addLinkEID { get; set; }
        public bool allowSpamFooter { get; set; }
        public FieldTypeHTMLFile copyFilename { get; set; }
        public int emailTemplateID { get; set; }
        public string fromAddress { get; set; }
        public DateTime? scheduleDate { get; set; }
        public bool sent { get; set; }
        public string subject { get; set; }
        public bool submitted { get; set; }
        public int testMemberID { get; set; }
    }
}
