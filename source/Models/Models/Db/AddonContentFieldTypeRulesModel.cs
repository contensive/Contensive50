
using System;
//
namespace Contensive.Models.Db {
    [Serializable]
    public class AddonContentFieldTypeRulesModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("add-on Content Field Type Rules", "ccaddoncontentfieldtyperules", "default", false);
        //
        //====================================================================================================
        /// <summary>
        /// field properties
        /// </summary>
        public int addonID { get; set; }
        public int contentFieldTypeID { get; set; }
    }
}
