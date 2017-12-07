
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
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.Context {
    //
    //====================================================================================================
    public class _blankModel {
        //
        private coreClass cpCore;
        //
        //====================================================================================================
        /// <summary>
        /// new
        /// </summary>
        /// <param name="cpCore"></param>
        public _blankModel(coreClass cpCore) : base() {
            this.cpCore = cpCore;
        }
        //
        //
        //
    }
}