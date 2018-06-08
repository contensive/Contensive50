
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
namespace Contensive.Processor.Models.Context {
    //
    //====================================================================================================
    public class _blankModel {
        //
        private coreController core;
        //
        //====================================================================================================
        /// <summary>
        /// new
        /// </summary>
        /// <param name="core"></param>
        public _blankModel(coreController core) : base() {
            this.core = core;
        }
        //
        //
        //
    }
}