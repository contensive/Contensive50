
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
namespace Contensive.Processor.Controllers {
    public class MenuLiveTabController {
        private MenuComboTabController comboTab = new MenuComboTabController();
        //
        public void AddEntry(string Caption, string LiveBody, string StylePrefix = "") {
            comboTab.AddEntry(Caption, "", "", LiveBody, false, "ccAdminTab");
        }
        //
        public string GetTabs(CoreController core) {
            return comboTab.GetTabs(core);
        }
        //
        public string GetTabBlank() {
            return comboTab.GetTabBlank();
        }
    }
}
