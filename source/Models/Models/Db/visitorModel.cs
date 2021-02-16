
using System;

namespace Contensive.Models.Db {
    /// <summary>
    /// record to track persistent cookie. Small footprint because we want to save an archive
    /// </summary>
    public class VisitorModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("visitors", "ccvisitors", "default", false);
        //
        //====================================================================================================
        /// <summary>
        /// The last people record associated with this visitor (not necessarily authenticated)
        /// </summary>
        public int memberId { get; set; }
        /// <summary>
        /// deprecate
        /// </summary>
        [Obsolete("Deprecated.",false)]
        public int forceBrowserMobile { get; set; }
    }
}
