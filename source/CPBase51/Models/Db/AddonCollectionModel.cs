
using System;
//
namespace Contensive.Models.Db {
    [Serializable]
    public class AddonCollectionModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Add-on Collections";
        public const string contentTableNameLowerCase = "ccaddoncollections";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public bool blockNavigatorNode { get; set; }
        public string contentFileList { get; set; }
        public string dataRecordList { get; set; }
        public string execFileList { get; set; }
        public string help { get; set; }
        public string helpLink { get; set; }
        public DateTime? lastChangeDate { get; set; }
        public string otherXML { get; set; }
        public bool system { get; set; }
        public bool updatable { get; set; }
        public string wwwFileList { get; set; }
    }
}
