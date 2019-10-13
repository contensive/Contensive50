
namespace Contensive.Models.Db {
    [System.Serializable]
    public class MemberTopicRuleModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Member Topic Rules", "ccMemberTopicRules", "default", false);
        //
        //====================================================================================================
        /// <summary>
        /// field properties
        /// </summary>
        public int MemberID { get; set; }
        public int TopicID { get; set; }
    }
}
