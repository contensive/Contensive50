
namespace Contensive.Models.Db {
    [System.Serializable]
    public class TopicHabitModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Topic Habits", "Topic Habits", "default", false);
        //
        //====================================================================================================
        /// <summary>
        /// field properties
        /// </summary>
        public string contentRecordKey { get; set; }
        public int memberID { get; set; }
        public int score { get; set; }
        public int topicID { get; set; }
        public int visitID { get; set; }
    }
}
