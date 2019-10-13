
namespace Contensive.Models.Db {
    [System.Serializable]
    public class FormSetModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Form Sets", "ccFormSets", "default", false);
        //
        //====================================================================================================
        //
        public int joinGroupID { get; set; }
        public int notificationEmailID { get; set; }
        public int responseEmailID { get; set; }
        public string thankYouCopy { get; set; }
    }
}
