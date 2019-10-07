
namespace Contensive.Models.Db {
    [System.Serializable]
    public class AddonIncludeRuleModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Add-on Include Rules", "ccaddonincluderules", "default", false);
        //
        //====================================================================================================
        /// <summary>
        /// field properties
        /// </summary>
        public int addonID { get; set; }
        public int includedAddonID { get; set; }
    }
}
