
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
using Contensive.Processor.Models.Domain;
using Contensive.Addons.Tools;
using static Contensive.Processor.AdminUIController;
//
namespace Contensive.Processor.Controllers {
    /// <summary>
    /// object that contains the context for the admin site, like recordsPerPage, etc. Should eventually include the loadContext and be its own document
    /// </summary>
    public class AdminMenuController {
        //
        /// <summary>
        /// tab systems used for admin site
        /// </summary>
        public MenuComboTabController menuComboTab {
            get {
                if (_menuComboTab == null) _menuComboTab = new MenuComboTabController();
                return _menuComboTab;
            }
        }
        private MenuComboTabController _menuComboTab;
        /// <summary>
        /// tab systems used for admin site
        /// </summary>
        public MenuLiveTabController menuLiveTab {
            get {
                if (_menuLiveTab == null) _menuLiveTab = new MenuLiveTabController();
                return _menuLiveTab;
            }
        }
        private MenuLiveTabController _menuLiveTab;
        //
    }
}
