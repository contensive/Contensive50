
using System;

namespace Contensive.Models.Db {
    [System.Serializable]
    public class EmailModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("email", "ccemail", "default", true);
        //
        //====================================================================================================
        public bool addLinkEID { get; set; }
        public bool allowSpamFooter { get; set; }
        public bool blockSiteStyles { get; set; }
        public DateTime? conditionExpireDate { get; set; }
        public int conditionID { get; set; }
        public int conditionPeriod { get; set; }
        public DbBaseModel.FieldTypeTextFile copyFilename { get; set; }
        public int emailTemplateID { get; set; }
        public int emailWizardID { get; set; }
        public string fromAddress { get; set; }
        public string inlineStyles { get; set; }
        public DateTime? lastSendTestDate { get; set; }
        public DateTime? scheduleDate { get; set; }
        public bool sent { get; set; }
        public DbBaseModel.FieldTypeCSSFile stylesFilename { get; set; }
        public string subject { get; set; }
        public bool submitted { get; set; }
        public int testMemberID { get; set; }
        public bool toAll { get; set; }
        public string addonList { get; set; }
    }
}
