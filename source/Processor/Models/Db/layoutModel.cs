
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
namespace Contensive.Processor.Models.Db {
    public class LayoutModel : DbModel {
        //
        //====================================================================================================
        //-- const. MUST be exposed const. NOT wrapped in property
        public const string contentName = "layouts";
        public const string contentTableName = "ccLayouts";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public FieldTypeHTMLFile layout { get; set; }
        public string stylesFilename { get; set; }
    }
}
