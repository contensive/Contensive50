
using System;

namespace Contensive.Models.Db {
    [System.Serializable]
    public class AuthoringControlModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        //
        public const string contentName = "Authoring Controls";
        public const string contentTableNameLowerCase = "ccauthoringcontrols";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        //
        /// <summary>
        /// tableId/recordId
        /// </summary>
        public string contentRecordKey { get; set; }
        /// <summary>
        /// type of authoring control
        /// </summary>
        public int controlType { get; set; }
        /// <summary>
        /// date time when this lock expires
        /// </summary>
        public DateTime? DateExpires { get; set; }
    }
}
