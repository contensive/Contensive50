
using Contensive.BaseClasses;
using System.Linq;

namespace Contensive.Models.Db {
    [System.Serializable]
    public class ContentFieldHelpModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "content field help";
        public const string contentTableNameLowerCase = "ccfieldhelp";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int fieldID { get; set; }
        public string helpCustom { get; set; }
        public string helpDefault { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// get the first field help for a field, digard the rest
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public static ContentFieldHelpModel createByFieldId(CPBaseClass cp, int fieldId) {
            var helpList = createList<ContentFieldHelpModel>(cp, "(fieldId=" + fieldId + ")", "id");
            if (helpList.Count == 0) {
                return null;
            } else {
                return helpList.First();
            }
        }
    }
}
