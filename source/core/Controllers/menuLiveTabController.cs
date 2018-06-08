
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
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Controllers {
    public class menuLiveTabController {
        private menuComboTabController comboTab = new menuComboTabController();
        //
        public void AddEntry(string Caption, string LiveBody, string StylePrefix = "") {
            comboTab.AddEntry(Caption, "", "", LiveBody, false, "ccAdminTab");
        }
        //
        public string GetTabs(coreController core) {
            return comboTab.GetTabs(core);
        }
        //
        public string GetTabBlank() {
            return comboTab.GetTabBlank();
        }
    }
}
