
using System;

namespace Contensive.Models.Db {
    [System.Serializable]
    public class GroupEmailModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("group email", "ccemail", "default", true);
        //
        //====================================================================================================
        public bool addLinkEId { get; set; }
        public bool allowSpamFooter { get; set; }
        public bool blockSiteStyles { get; set; }
        public string copyFilename { get; set; }
        public int emailTemplateId { get; set; }
        public string fromAddress { get; set; }
        public string inlineStyles { get; set; }
        public DateTime? lastSendTestDate { get; set; }
        public DateTime? scheduleDate { get; set; }
        public bool sent { get; set; }
        public string stylesFilename { get; set; }
        public string subject { get; set; }
        public bool submitted { get; set; }
        public int testMemberId { get; set; }
    }
}
