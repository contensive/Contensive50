
using System;
//
namespace Contensive.Models.Db {
    [Serializable]
    public class AddonContentFieldTypeRulesModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "add-on Content Field Type Rules";
        public const string contentTableNameLowerCase = "ccaddoncontentfieldtyperules";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int addonID { get; set; }
        public int contentFieldTypeID { get; set; }
    }
}
