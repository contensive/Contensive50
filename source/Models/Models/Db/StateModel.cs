
namespace Contensive.Models.Db {
    [System.Serializable]
    public class StateModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "states";           
        public const string contentTableNameLowerCase = "ccstates";     
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public string abbreviation { get; set; }
        public int countryID { get; set; }
        public double salesTax { get; set; }
    }
}
