
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
        //-- const
        public static string contentName { get { return "layouts"; } }
        public static string contentTableName { get { return "ccLayouts"; } }
        public static string contentDataSource { get { return "default"; } }
        public static bool nameFieldIsUnique { get { return true; } }
        //
        //====================================================================================================
        // -- instance properties
        public FieldTypeHTMLFile layout { get; set; }
        public string stylesFilename { get; set; }
    }
}
