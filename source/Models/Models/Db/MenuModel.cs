
namespace Contensive.Models.Db {
    [System.Serializable]
    public class MenuModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Menus", "ccmenus", "default", false);
        //
        //====================================================================================================
    }
}
