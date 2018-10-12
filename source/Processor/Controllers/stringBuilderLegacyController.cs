
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
using static Contensive.Processor.constants;
using System.Text;
//
namespace Contensive.Processor.Controllers {
    public class StringBuilderLegacyController {
        private StringBuilder builder = new StringBuilder();
        public void Add(string NewString) {
            builder.Append(NewString);
        }
        public string Text {
            get {
                return builder.ToString();
            }
        }
    }
}