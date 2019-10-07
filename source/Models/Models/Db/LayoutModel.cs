
namespace Contensive.Models.Db {
    [System.Serializable]
    public class LayoutModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("layouts", "cclayouts", "default", true);
        //
        //====================================================================================================
        public DbBaseModel.FieldTypeTextFile layout { get; set; }
        public string stylesFilename { get; set; }
    }
}
