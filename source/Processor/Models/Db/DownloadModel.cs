
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    [System.Serializable]
    public class DownloadModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "downloads";
        public const string contentTableNameLowerCase = "ccdownloads";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public FieldTypeTextFile filename { get; set; }
        public int requestedBy { get; set; }
        public DateTime dateRequested { get; set; }
        public DateTime dateCompleted { get; set; }
        public string resultMessage { get; set; }
    }
}
