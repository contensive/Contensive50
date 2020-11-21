
namespace Contensive.Models.Db {
    public class CountryModelModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("content", "table", "default", false);
        //
        //====================================================================================================
        //
        public string abbreviation { get; set; }
        public bool domesticShipping  { get; set; }

    }
}
