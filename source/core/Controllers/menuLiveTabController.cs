
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
//
namespace Contensive.Core.Controllers {
    public class menuLiveTabController {
        private menuComboTabController comboTab = new menuComboTabController();
        //
        public void AddEntry(string Caption, string LiveBody, string StylePrefix = "") {
            comboTab.AddEntry(Caption, "", "", LiveBody, false, "ccAdminTab");
        }
        //
        public string GetTabs(coreController cpCore) {
            return comboTab.GetTabs(cpCore);
        }
        //
        public string GetTabBlank() {
            return comboTab.GetTabBlank();
        }
    }
}
