
namespace Contensive.Models.Db {
    [System.Serializable]
    public class AddonContentTriggerRuleModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Addon Content Trigger Rules", "ccAddonContentTriggerRules", "default", false);
        //
        //====================================================================================================
        public int addonID { get; set; }
        public int contentID { get; set; }
    }
}
