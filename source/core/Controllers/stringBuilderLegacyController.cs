
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
using System.Text;
//
namespace Contensive.Core.Controllers {
    public class stringBuilderLegacyController {
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