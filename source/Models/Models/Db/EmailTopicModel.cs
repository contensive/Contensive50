﻿
namespace Contensive.Models.Db {
    [System.Serializable]
    public class EmailTopicModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Email Topics", "ccEmailTopics", "default", false);
        //
        //====================================================================================================
        /// <summary>
        /// field properties
        /// </summary>
        public int emailID { get; set; }
        public int topicID { get; set; }

    }
}
