
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
namespace Contensive.Processor.Models.Domain {
    //
    //====================================================================================================
    [System.Serializable]
    public class StylesheetContextModel {
        public int templateId { get; set; }
        public int emailId { get; set; }
        public string styleSheet { get; set; }
    }
}