
using System;

namespace Contensive.Models.Db {
    [System.Serializable]
    public class MemberRuleModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "member rules";
        public const string contentTableNameLowerCase = "ccmemberrules";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public DateTime? dateExpires { get; set; }
        public int groupId { get; set; }
        public int memberID { get; set; }
    }
}
