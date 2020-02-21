﻿
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Contensive.BaseClasses;
using Contensive.BaseModels;
using Contensive.Processor.Controllers;

namespace Contensive.Processor {
    public class CPHttpClass : BaseClasses.CPHttpBaseClass {
        /// <summary>
        /// dependencies
        /// </summary>
        private readonly CPClass cp;
        //
        // ====================================================================================================
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cpParent"></param>
        //
        public CPHttpClass(CPClass cpParent) {
            cp = cpParent;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Simple http get of a url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public override string Get(string url) {
            HttpRequestController kmaHTTP = new HttpRequestController();
            return kmaHTTP.getURL(url);
        }
        //
        // ====================================================================================================
        public override string Post(string url, List<KeyValuePair<string, string>> requestArguments) {
            throw new NotImplementedException();
        }
        //
        // ====================================================================================================
        public override string Post(string url, string entity) {
            throw new NotImplementedException();
        }
    }
}