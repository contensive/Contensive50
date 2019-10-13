
namespace Contensive.Models.Db {
    [System.Serializable]
    public class PageContentTopicRuleModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("page content topic rules", "ccpagecontenttopicrules", "default", false);
        //
        //====================================================================================================
        /// <summary>
        /// The page associated to this topic. Add to track users interested in this topic and to provide navigation to this topic.
        /// </summary>
        public int pageID { get; set; }
        /// <summary>
        /// The topic 
        /// </summary>
        public int topicID { get; set; }

    }
}
