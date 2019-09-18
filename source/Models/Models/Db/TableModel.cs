
using Contensive.BaseClasses;

namespace Contensive.Models.Db {
    [System.Serializable]
    public class TableModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "tables";
        public const string contentTableNameLowerCase = "cctables";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public int dataSourceID { get; set; }
        //
        //====================================================================================================
        //
        public static TableModel createByContentName(CPBaseClass cp, string contentName) {
            var content = createByUniqueName<ContentModel>(cp, contentName);
            if (content != null) { return create<TableModel>(cp, content.contentTableID); }
            return null;
        }
    }
}
