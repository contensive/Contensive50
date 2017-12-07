
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
namespace Contensive.Core.Models.Entity {
    //
    //====================================================================================================
    /// <summary>
    /// classSummary
    /// </summary>
    public class domainLegacyModel {
        //
        private coreClass cpCore;
        //
        private List<string> domainList_local = new List<string>();
        private bool serverDomainList_localLoaded = false;
        // -- 20170528 no longer support this. host page includes app name so any domain works anyway
        //Public ServerMultiDomainMode As Boolean = False    ' When true, the site can run from any domain name.
        //
        //   values read from the domain record during init
        //
        public class domainDetailsClass {
            public string name;
            public int rootPageId;
            public bool noFollow;
            public int typeId;
            public bool visited;
            public int id;
            public string forwardUrl;
            public int defaultTemplateId;
            public int pageNotFoundPageId;
            //Public allowCrossLogin As Boolean
            public int forwardDomainId;
        }
        //
        // information about all the domains for this app
        //
        public Dictionary<string, domainDetailsClass> domainDetailsList;
        public domainDetailsClass domainDetails = new domainDetailsClass();
        //
        public domainLegacyModel(coreClass cpCore) : base() {
            this.cpCore = cpCore;
        }
        //
        //===========================================================================================
        //
        public List<string> getDomainDbList {
            get {
                List<string> returnDomainDbList = null;
                string SQL = null;
                DataTable dt = null;
                //
                const string cacheName = "domainDbList";
                //
                try {
                    if (!serverDomainList_localLoaded) {
                        domainList_local = (List<string>)cpCore.cache.getObject<List<string>>(cacheName);
                        if (domainList_local == null) {
                            //
                            // recreate (non-default) domain table list
                            //
                            domainList_local = new List<string>();
                            domainList_local.Add(cpCore.serverConfig.appConfig.domainList[0]);
                            //
                            // select all Normal domains (non-Forward)
                            //
                            SQL = "select name from ccDomains where typeId=1";
                            dt = cpCore.db.executeQuery(SQL);
                            foreach (DataRow dr in dt.Rows) {
                                domainList_local.Add(dr(0).ToString());
                            }
                            cpCore.cache.setContent(cacheName, domainList_local, "domains");
                            dt.Dispose();
                        }
                        serverDomainList_localLoaded = true;
                    }
                    returnDomainDbList = domainList_local;
                } catch (Exception ex) {
                    cpCore.handleException(ex);
                    throw;
                }
                return returnDomainDbList;
            }
        }
        //
        //===========================================================================================
        //
    }
}