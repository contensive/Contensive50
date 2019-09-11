
using System;

namespace Contensive.Models.Db {
    [System.Serializable]
    public class GroupEmailModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "group email";
        public const string contentTableNameLowerCase = "ccemail";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public bool addLinkEID { get; set; }
        public bool allowSpamFooter { get; set; }
        public bool blockSiteStyles { get; set; }
        public string copyFilename { get; set; }
        public int emailTemplateID { get; set; }
        public string fromAddress { get; set; }
        public string inlineStyles { get; set; }
        public DateTime lastSendTestDate { get; set; }
        public DateTime scheduleDate { get; set; }
        public bool sent { get; set; }
        public string stylesFilename { get; set; }
        public string subject { get; set; }
        public bool submitted { get; set; }
        public int testMemberID { get; set; }
    }
}
