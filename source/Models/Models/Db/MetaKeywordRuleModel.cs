
namespace Contensive.Models.Db {
    [System.Serializable]
    public class MetaKeywordRuleModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Meta Keyword Rules", "ccMetaKeywordRules", "default", false);
        //
        //====================================================================================================
        //
        public int metaContentID { get; set; }
        public int metaKeywordID { get; set; }
    }
}
