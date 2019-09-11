
namespace Contensive.Models.Db {
    [System.Serializable]
    public class SortMethodModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "sort methods";
        public const string contentTableNameLowerCase = "ccSortMethods";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public string OrderByClause { get; set; }
    }
}
