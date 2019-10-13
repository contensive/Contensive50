
namespace Contensive.Models.Db {
    [System.Serializable]
    public class AddonCollectionParentRuleModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Addon Collection Parent Rules", "ccAddonCollectionParentRules", "default", false);
        //
        //====================================================================================================
        //
        public int childID { get; set; }
        public int parentID { get; set; }
    }
}
