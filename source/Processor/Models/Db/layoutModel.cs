
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
    public class LayoutModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const (must be public const, not property)
        public const string contentName = "layouts";
        public const string contentTableName = "ccLayouts";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties (must be properties not fields)
        public FieldTypeHTMLFile layout { get; set; }
        public string stylesFilename { get; set; }
    }
}
