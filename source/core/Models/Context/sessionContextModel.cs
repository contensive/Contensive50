
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
using Contensive.BaseClasses;
using Contensive.Core.Models.Complex;
//
namespace Contensive.Core.Models.Context {
    //
    //====================================================================================================
    /// <summary>
    /// session context -- the identity, visit, visitor, view
    /// </summary>
    public class sessionContextModel {
        //
        //====================================================================================================
        // -- this class stores state, so it can hold a pointer to the cpCore instance
        private coreClass cpCore { get; set; }
        //
        //====================================================================================================
        // -- the visit is the collection of pages, constructor creates default non-authenticated instance
        public visitModel visit { get; set; }
        //
        //====================================================================================================
        // -- visitor represents the browser, constructor creates default non-authenticated instance
        public visitorModel visitor { get; set; }
        //
        //====================================================================================================
        // -- user is the person at the keyboad, constructor creates default non-authenticated instance
        public personModel user { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// userLanguage will return a valid populated language object
        /// </summary>
        /// <returns></returns>
        public languageModel userLanguage {
            get {
                if ((_language == null) && (user != null)) {
                    if (user.LanguageID > 0) {
                        //
                        // -- get user language
                        _language = languageModel.create(cpCore, user.LanguageID);
                    }
                    if (_language == null) {
                        //
                        // -- try browser language if available
                        string HTTP_Accept_Language = Controllers.iisController.getBrowserAcceptLanguage(cpCore);
                        if (!string.IsNullOrEmpty(HTTP_Accept_Language)) {
                            List<languageModel> languageList = languageModel.createList(cpCore, "(HTTP_Accept_Language='" + HTTP_Accept_Language + "')");
                            if (languageList.Count > 0) {
                                _language = languageList[0];
                            }
                        }
                    }
                    if (_language == null) {
                        //
                        // -- try default language
                        string defaultLanguageName = cpCore.siteProperties.getText("Language", "English");
                        _language = languageModel.createByName(cpCore, defaultLanguageName);
                    }
                    if (_language == null) {
                        //
                        // -- try english
                        _language = languageModel.createByName(cpCore, "English");
                    }
                    if (_language == null) {
                        //
                        // -- add english to the table
                        _language = languageModel.add(cpCore);
                        _language.name = "English";
                        _language.HTTP_Accept_Language = "en";
                        _language.save(cpCore);
                        user.LanguageID = _language.id;
                        user.save(cpCore);
                    }
                }
                return _language;
            }
        } private languageModel _language = null;
        //
        //====================================================================================================
        // -- is this user authenticated in this visit
        public bool isAuthenticated {
            get {
                return visit.VisitAuthenticated;
            }
        }
        //
        //====================================================================================================
        // -- legacy
        public bool visit_initialized = false; // true when visit has been initialized
        public bool visit_browserIsIE = false; // if detail includes msie
        public bool visit_browserIsNS = false; // if detail or detailtail is netscape
        public string visit_browserVersion = "";
        public bool visit_browserIsWindows = false; // if any browser detail includes "windows"
        public bool visit_browserIsMac = false; // if any browser deail includes "mac"
        public bool visit_browserIsLinux = false; // not sure
        public bool visit_browserIsMobile = false; // if a WAP Mobile device
        public bool visit_isBadBot = false;
        public bool visit_stateOK = false; // if false, page is out of state (sequence)
        private string contentAccessRights_NotList = ""; // If ContentId in this list, they are not a content manager
        private string contentAccessRights_List = ""; // If ContentId in this list, they are a content manager
        private string contentAccessRights_AllowAddList = ""; // If in _List, test this for allowAdd
        private string contentAccessRights_AllowDeleteList = ""; // If in _List, test this for allowDelete
        public string main_IsEditingContentList = "";
        public string main_IsNotEditingContentList = "";
        //
        //====================================================================================================
        /// <summary>
        /// constructor, no arguments, created default authentication model for use without user, and before user is available
        /// </summary>
        public sessionContextModel(coreClass cpCore) {
            this.cpCore = cpCore;
            visit = new visitModel();
            visitor = new visitorModel();
            user = new personModel();
        }
        //
        //========================================================================
        /// <summary>
        /// create a new session
        /// </summary>
        /// <param name="cpCore"></param>
        /// <param name="trackVisits">When true, the session is initialized with a visit, visitor, user. Set false for background processing. 
        /// Set true for website processing when allowVisit true.
        /// When false, a visit can be configured on the fly by any application that attempts to access the cp.user.id
        /// </param>
        /// <returns></returns>
        public static sessionContextModel create(coreClass cpCore, bool trackVisits) {
            sessionContextModel resultSessionContext = null;
            var sw = Stopwatch.StartNew();
            try {
                if (cpCore.serverConfig == null) {
                    //
                    // -- application error if no server config
                    cpCore.handleException(new ApplicationException("authorization context cannot be created without a server configuration."));
                } else {
                    //
                    // -- test point message
                    string msg = "SessionContext.create, enter";
                    debugController.testPoint(cpCore, msg);
                    logController.appendLogDebug(cpCore, msg);
                    if (cpCore.serverConfig.appConfig == null) {
                        //
                        // -- no application, this is a server-only call not related to a 
                        resultSessionContext = new sessionContextModel(cpCore);
                    } else {
                        bool TrackGuests = false;
                        string DefaultMemberName = null;
                        bool AllowOnNewVisitEvent = false;
                        DateTime visitCookieTimestamp = default(DateTime);
                        int SlashPosition = 0;
                        string MemberLinkinEID = null;
                        int MemberLinkLoginID = 0;
                        int MemberLinkRecognizeID = 0;
                        string visitCookieNew = null;
                        string visitCookie = null;
                        string CookieVisitor = null;
                        string WorkingReferer = null;
                        DateTime tokenDate = default(DateTime);
                        bool visit_changes = false;
                        bool visitor_changes = false;
                        bool user_changes = false;
                        string main_appNameCookiePrefix = genericController.vbLCase(genericController.main_encodeCookieName(cpCore.serverConfig.appConfig.name));
                        //
                        resultSessionContext = new sessionContextModel(cpCore);
                        visitCookieTimestamp = DateTime.MinValue;
                        visitCookie = cpCore.webServer.getRequestCookie(main_appNameCookiePrefix + constants.main_cookieNameVisit);
                        MemberLinkinEID = cpCore.docProperties.getText("eid");
                        MemberLinkLoginID = 0;
                        MemberLinkRecognizeID = 0;
                        if (!string.IsNullOrEmpty(MemberLinkinEID)) {
                            //
                            // -- attempt link authentication
                            if (cpCore.siteProperties.getBoolean("AllowLinkLogin", true)) {
                                //
                                // -- allow Link Login
                                cpCore.security.decodeToken(MemberLinkinEID, ref MemberLinkLoginID, ref tokenDate);
                            } else if (cpCore.siteProperties.getBoolean("AllowLinkRecognize", true)) {
                                //
                                // -- allow Link Recognize
                                cpCore.security.decodeToken(MemberLinkinEID, ref MemberLinkRecognizeID, ref tokenDate);
                            } else {
                                //
                                // -- block link login
                                MemberLinkinEID = "";
                            }
                        }
                        if ((trackVisits) || (!string.IsNullOrEmpty(visitCookie)) | (MemberLinkLoginID != 0) | (MemberLinkRecognizeID != 0)) {
                            //
                            // -- Visit Tracking
                            int cookieVisitId = 0;
                            if (!string.IsNullOrEmpty(visitCookie)) {
                                //
                                // -- visit cookie found
                                cpCore.security.decodeToken(visitCookie, ref cookieVisitId, ref visitCookieTimestamp);
                                if (cookieVisitId == 0) {
                                    //
                                    // -- Bad Cookie, clear it so a new one will be written
                                    visitCookie = "";
                                }
                            }
                            if (cookieVisitId != 0) {
                                //
                                // -- Visit is good, setup visit, then secondary visitor/user if possible
                                resultSessionContext.visit = visitModel.create(cpCore, cookieVisitId);
                                if (resultSessionContext.visit == null) {
                                    //
                                    // -- visit record is missing, create a new visit
                                    resultSessionContext.visit = visitModel.add(cpCore);
                                } else if (resultSessionContext.visit.LastVisitTime.AddHours(1) < cpCore.doc.profileStartTime) {
                                    //
                                    // -- visit has expired, create new visit
                                    resultSessionContext.visit = visitModel.add(cpCore);
                                } else {
                                    //
                                    // -- visit object is valid, share its data with other objects
                                    resultSessionContext.visit.TimeToLastHit = 0;
                                    if (resultSessionContext.visit.StartTime > DateTime.MinValue) {
                                        resultSessionContext.visit.TimeToLastHit = encodeInteger((cpCore.doc.profileStartTime - resultSessionContext.visit.StartTime).TotalSeconds);
                                    }
                                    resultSessionContext.visit.CookieSupport = true;
                                    if (resultSessionContext.visit.VisitorID > 0) {
                                        //
                                        // -- try visit's visitor object
                                        visitorModel testVisitor = visitorModel.create(cpCore, resultSessionContext.visit.VisitorID);
                                        if (testVisitor != null) {
                                            resultSessionContext.visitor = testVisitor;
                                        }
                                    }
                                    if (resultSessionContext.visit.MemberID > 0) {
                                        //
                                        // -- try visit's person object
                                        personModel testUser = personModel.create(cpCore, resultSessionContext.visit.MemberID);
                                        if (testUser != null) {
                                            resultSessionContext.user = testUser;
                                        }
                                    }
                                    if (((visitCookieTimestamp - resultSessionContext.visit.LastVisitTime).TotalSeconds) > 2) {
                                        resultSessionContext.visit_stateOK = false;
                                    }
                                }
                            }
                            if (resultSessionContext.visit.id == 0) {
                                //
                                // -- create new visit record
                                resultSessionContext.visit = visitModel.add(cpCore);
                                if (string.IsNullOrEmpty(resultSessionContext.visit.name)) {
                                    resultSessionContext.visit.name = "User";
                                }
                                resultSessionContext.visit.PageVisits = 0;
                                resultSessionContext.visit.StartTime = cpCore.doc.profileStartTime;
                                resultSessionContext.visit.StartDateValue = encodeInteger(cpCore.doc.profileStartTime.ToOADate());
                                //
                                // -- setup referrer
                                if (!string.IsNullOrEmpty(cpCore.webServer.requestReferrer)) {
                                    WorkingReferer = cpCore.webServer.requestReferrer;
                                    SlashPosition = genericController.vbInstr(1, WorkingReferer, "//");
                                    if ((SlashPosition != 0) && (WorkingReferer.Length > (SlashPosition + 2))) {
                                        WorkingReferer = WorkingReferer.Substring(SlashPosition + 1);
                                    }
                                    SlashPosition = genericController.vbInstr(1, WorkingReferer, "/");
                                    if (SlashPosition == 0) {
                                        resultSessionContext.visit.RefererPathPage = "";
                                        resultSessionContext.visit.HTTP_REFERER = WorkingReferer;
                                    } else {
                                        resultSessionContext.visit.RefererPathPage = WorkingReferer.Substring(SlashPosition - 1);
                                        resultSessionContext.visit.HTTP_REFERER = WorkingReferer.Left(SlashPosition - 1);
                                    }
                                }
                                //
                                if (resultSessionContext.visitor.ID == 0) {
                                    //
                                    // -- visit.visitor not valid, create visitor from cookie
                                    CookieVisitor = genericController.encodeText(cpCore.webServer.getRequestCookie(main_appNameCookiePrefix + main_cookieNameVisitor));
                                    if (cpCore.siteProperties.getBoolean("AllowAutoRecognize", true)) {
                                        //
                                        // -- auto recognize, setup user based on visitor
                                        int cookieVisitorId = 0;
                                        cpCore.security.decodeToken(CookieVisitor, ref cookieVisitorId, ref tokenDate);
                                        if (cookieVisitorId != 0) {
                                            //
                                            // -- visitor cookie good
                                            visitorModel testVisitor = visitorModel.create(cpCore, cookieVisitorId);
                                            if (testVisitor != null) {
                                                resultSessionContext.visitor = testVisitor;
                                                visitor_changes = true;
                                            }
                                        }
                                    }
                                }
                                //
                                if (resultSessionContext.visitor.ID == 0) {
                                    //
                                    // -- create new visitor
                                    resultSessionContext.visitor = visitorModel.add(cpCore);
                                    visitor_changes = false;
                                    //
                                    resultSessionContext.visit.VisitorNew = true;
                                    visit_changes = true;
                                }
                                //
                                // -- find  identity from the visitor
                                if (resultSessionContext.visitor.MemberID > 0) {
                                    //
                                    // -- recognize by the main_VisitorMemberID
                                    if (resultSessionContext.recognizeById(cpCore, resultSessionContext.visitor.MemberID, ref resultSessionContext)) {
                                        //
                                        // -- if successful, now test for autologin (authentication)
                                        if (cpCore.siteProperties.AllowAutoLogin & resultSessionContext.user.AutoLogin & resultSessionContext.visit.CookieSupport) {
                                            //
                                            // -- they allow it, now Check if they were logged in on their last visit
                                            visitModel lastVisit = visitModel.getLastVisitByVisitor(cpCore, resultSessionContext.visit.id, resultSessionContext.visitor.ID);
                                            if (lastVisit != null) {
                                                if (lastVisit.VisitAuthenticated && (lastVisit.MemberID == resultSessionContext.visit.id)) {
                                                    if (resultSessionContext.authenticateById(cpCore, resultSessionContext.user.id, resultSessionContext)) {
                                                        logController.logActivity2(cpCore, "autologin", resultSessionContext.user.id, resultSessionContext.user.OrganizationID);
                                                        visitor_changes = true;
                                                        user_changes = true;
                                                    }
                                                }
                                            }
                                        } else {
                                            //
                                            // -- Recognized, not auto login
                                            logController.logActivity2(cpCore, "recognized", resultSessionContext.user.id, resultSessionContext.user.OrganizationID);
                                        }
                                    }
                                }
                                if (cpCore.webServer.requestBrowser == "") {
                                    //
                                    // blank browser, Blank-Browser-Bot
                                    //
                                    resultSessionContext.visit.name = "Blank-Browser-Bot";
                                    resultSessionContext.visit.Bot = true;
                                    resultSessionContext.visit_isBadBot = false;
                                    resultSessionContext.visit.Mobile = false;
                                } else {
                                    //
                                    // -- mobile detect
                                    switch (resultSessionContext.visitor.ForceBrowserMobile) {
                                        case 1:
                                            resultSessionContext.visit.Mobile = true;
                                            break;
                                        case 2:
                                            resultSessionContext.visit.Mobile = false;
                                            break;
                                        default:
                                            resultSessionContext.visit.Mobile = isMobile(cpCore.webServer.requestBrowser);
                                            break;
                                    }
                                    //
                                    // -- bot and badBot detect
                                    resultSessionContext.visit.Bot = false;
                                    resultSessionContext.visit_isBadBot = false;
                                    string botFileContent = cpCore.cache.getObject<string>("DefaultBotNameList");
                                    if (string.IsNullOrEmpty(botFileContent)) {
                                        string Filename = "config\\VisitNameList.txt";
                                        botFileContent = cpCore.privateFiles.readFile(Filename);
                                        if (string.IsNullOrEmpty(botFileContent)) {
                                            botFileContent = ""
                                                + "\r\n//"
                                                + "\r\n// Default Bot Name list"
                                                + "\r\n// This file is maintained by the server. On the first hit of a visit,"
                                                + "\r\n// the default member name is overridden with this name if there is a match"
                                                + "\r\n// in either the user agent or the ipaddress."
                                                + "\r\n// format:  name -tab- browser-user-agent-substring -tab- ip-address-substring -tab- type "
                                                + "\r\n// This text is cached by the server for 1 hour, so changes take"
                                                + "\r\n// effect when the cache expires. It is updated daily from the"
                                                + "\r\n// support site feed. Manual changes may be over written."
                                                + "\r\n// type - r=robot (default), b=bad robot, u=user"
                                                + "\r\n//"
                                                + "\r\nContensive MonitorContensive Monitor\t\tr"
                                                + "\r\nGoogle-Bot\tgooglebot\t\tr"
                                                + "\r\nMSN-Bot\tmsnbot\t\tr"
                                                + "\r\nYahoo-Bot\tslurp\t\tr"
                                                + "\r\nSearchMe-Bot\tsearchme.com\t\tr"
                                                + "\r\nTwiceler-Bot\twww.cuil.com\t\tr"
                                                + "\r\nUnknown Bot\trobot\t\tr"
                                                + "\r\nUnknown Bot\tcrawl\t\tr"
                                                + "";
                                            cpCore.privateFiles.saveFile(Filename, botFileContent);
                                        }
                                        cpCore.cache.setObject("DefaultBotNameList", botFileContent, DateTime.Now.AddHours(1), new List<string>());
                                    }
                                    //
                                    if (!string.IsNullOrEmpty(botFileContent)) {
                                        botFileContent = genericController.vbReplace(botFileContent, "\r\n", "\n");
                                        List<string> botList = new List<string>();
                                        botList.AddRange(botFileContent.Split(Convert.ToChar("\n")));
                                        foreach (string srcLine in botList) {
                                            string line = srcLine.Trim();
                                            if (!string.IsNullOrWhiteSpace(line)) {
                                                // -- remove comment
                                                int posComment = line.IndexOf("//");
                                                if (posComment >= 0) {
                                                    line = line.Left(posComment);
                                                }
                                                if (!string.IsNullOrWhiteSpace(line)) {
                                                    // -- parse line on tab characters
                                                    string[] Args = genericController.stringSplit(line, "\t");
                                                    if (Args.GetUpperBound(0) > 0) {
                                                        // -- process argument 1
                                                        if (!string.IsNullOrEmpty(Args[1].Trim(' '))) {
                                                            if (genericController.vbInstr(1, cpCore.webServer.requestBrowser, Args[1], 1) != 0) {
                                                                resultSessionContext.visit.name = Args[0];
                                                                //visitNameFound = True
                                                                break;
                                                            }
                                                        }
                                                        if (Args.GetUpperBound(0) > 1) {
                                                            // -- process argument 2
                                                            if (!string.IsNullOrEmpty(Args[2].Trim(' '))) {
                                                                if (genericController.vbInstr(1, cpCore.webServer.requestRemoteIP, Args[2], 1) != 0) {
                                                                    resultSessionContext.visit.name = Args[0];
                                                                    //visitNameFound = True
                                                                    break;
                                                                }
                                                            }
                                                            if (Args.GetUpperBound(0) <= 2) {
                                                                resultSessionContext.visit.Bot = true;
                                                                resultSessionContext.visit_isBadBot = false;
                                                            } else {
                                                                resultSessionContext.visit_isBadBot = (Args[3].ToLower() == "b");
                                                                resultSessionContext.visit.Bot = resultSessionContext.visit_isBadBot || (Args[3].ToLower() == "r");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                //
                                // -- new visit, update the persistant visitor cookie
                                if (trackVisits) {
                                    cpCore.webServer.addResponseCookie(main_appNameCookiePrefix + main_cookieNameVisitor, cpCore.security.encodeToken(resultSessionContext.visitor.ID, resultSessionContext.visit.StartTime), resultSessionContext.visit.StartTime.AddYears(1), "", requestAppRootPath, false);
                                }
                                //
                                // -- OnNewVisit Add-on call
                                AllowOnNewVisitEvent = true;
                            }
                            resultSessionContext.visit.LastVisitTime = cpCore.doc.profileStartTime;
                            //
                            // -- verify visitor
                            if (resultSessionContext.visitor.ID == 0) {
                                //
                                // -- create new visitor
                                resultSessionContext.visitor = visitorModel.add(cpCore);
                                visitor_changes = false;
                                //
                                resultSessionContext.visit.VisitorNew = true;
                                visit_changes = true;
                            }
                            //
                            // -- Attempt Link-in recognize or login
                            if (MemberLinkLoginID != 0) {
                                //
                                // -- Link Login
                                if (resultSessionContext.authenticateById(cpCore, MemberLinkLoginID, resultSessionContext)) {
                                    logController.logActivity2(cpCore, "link login with eid " + MemberLinkinEID, resultSessionContext.user.id, resultSessionContext.user.OrganizationID);
                                }
                            } else if (MemberLinkRecognizeID != 0) {
                                //
                                // -- Link Recognize
                                resultSessionContext.recognizeById(cpCore, MemberLinkRecognizeID, ref resultSessionContext);
                                logController.logActivity2(cpCore, "link recognize with eid " + MemberLinkinEID, resultSessionContext.user.id, resultSessionContext.user.OrganizationID);
                            }
                            //
                            // -- create guest identity if no identity
                            if (resultSessionContext.user.id < 1) {
                                //
                                // -- No user created
                                if (resultSessionContext.visit.name.Left(5).ToLower() != "visit") {
                                    DefaultMemberName = resultSessionContext.visit.name;
                                } else {
                                    DefaultMemberName = genericController.encodeText(Models.Complex.cdefModel.GetContentFieldProperty(cpCore, "people", "name", "default"));
                                }
                                //resultAuthContext.isAuthenticatedAdmin_cache_isLoaded = False
                                //resultAuthContext.property_user_isMember_isLoaded = False
                                //resultAuthContext.isAuthenticatedDeveloper_cache_isLoaded = False
                                TrackGuests = cpCore.siteProperties.getBoolean("track guests", false);
                                if (!TrackGuests) {
                                    //
                                    // -- do not track guests at all
                                    resultSessionContext.user = new personModel();
                                    resultSessionContext.user.name = DefaultMemberName;
                                    user_changes = false;
                                    resultSessionContext.visitor.MemberID = 0;
                                    visitor_changes = true;
                                    resultSessionContext.visit.VisitAuthenticated = false;
                                    resultSessionContext.visit.MemberID = 0;
                                    visit_changes = true;
                                    //
                                } else {
                                    if (resultSessionContext.visit.CookieSupport) {
                                        //
                                        // -- cookies supported, not first hit and not spider
                                        resultSessionContext.user = personModel.add(cpCore);
                                        user_changes = true;
                                        resultSessionContext.visitor.MemberID = resultSessionContext.user.id;
                                        visitor_changes = true;
                                        resultSessionContext.visit.VisitAuthenticated = false;
                                        visit_changes = true;
                                    } else {
                                        if (cpCore.siteProperties.trackGuestsWithoutCookies) {
                                            //
                                            // -- create people for non-cookies too
                                            //
                                            resultSessionContext.user = personModel.add(cpCore);
                                            resultSessionContext.user.name = resultSessionContext.visit.name;
                                            user_changes = true;
                                        } else {
                                            //
                                            // set defaults for people object, but no record
                                            //
                                            resultSessionContext.user = new personModel();
                                            resultSessionContext.user.name = DefaultMemberName;
                                            user_changes = true;
                                        }
                                    }
                                }
                            }
                            //
                            // -- check for changes in interrelationships
                            if (resultSessionContext.visitor.MemberID != resultSessionContext.user.id) {
                                resultSessionContext.visitor.MemberID = resultSessionContext.user.id;
                                visitor_changes = true;
                            }
                            if (resultSessionContext.visit.MemberID != resultSessionContext.user.id) {
                                resultSessionContext.visit.MemberID = resultSessionContext.user.id;
                                visit_changes = true;
                            }
                            if (resultSessionContext.visit.VisitorID != resultSessionContext.visitor.ID) {
                                resultSessionContext.visit.VisitorID = resultSessionContext.visitor.ID;
                                visit_changes = true;
                            }
                            //
                            // -- Save anything that changed
                            resultSessionContext.visit.ExcludeFromAnalytics |= resultSessionContext.visit.Bot | resultSessionContext.user.ExcludeFromAnalytics | resultSessionContext.user.Admin | resultSessionContext.user.Developer;
                            if (!cpCore.webServer.pageExcludeFromAnalytics) {
                                resultSessionContext.visit.PageVisits += 1;
                                visit_changes = true;
                            }
                            resultSessionContext.visit_initialized = true;
                            if (visit_changes) {
                                resultSessionContext.visit.save(cpCore);
                            }
                            if (visitor_changes) {
                                resultSessionContext.visitor.saveObject(cpCore);
                            }
                            if (user_changes) {
                                resultSessionContext.user.save(cpCore);
                            }
                            visitCookieNew = cpCore.security.encodeToken(resultSessionContext.visit.id, resultSessionContext.visit.LastVisitTime);
                            if (trackVisits && (visitCookie != visitCookieNew)) {
                                visitCookie = visitCookieNew;
                            }
                        }
                        resultSessionContext.visit_initialized = true;
                        if ((AllowOnNewVisitEvent) && (true)) {
                            CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() { addonType = CPUtilsBaseClass.addonContext.ContextOnNewVisit };
                            foreach (addonModel addon in addonModel.createList_OnNewVisitEvent(cpCore, new List<string>())) {
                                executeContext.errorCaption = addon.name;
                                cpCore.addon.execute(addon, executeContext);
                            }
                        }
                        //
                        // -- Write Visit Cookie
                        visitCookie = cpCore.security.encodeToken(resultSessionContext.visit.id, cpCore.doc.profileStartTime);
                        cpCore.webServer.addResponseCookie(main_appNameCookiePrefix + constants.main_cookieNameVisit, visitCookie, default(DateTime), "", requestAppRootPath, false);
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            } finally {
                //
                // -- test point message
                string msg = "SessionContext.create, exit (" + sw.ElapsedMilliseconds.ToString() + "ms)";
                debugController.testPoint(cpCore, msg);
                logController.appendLogDebug(cpCore, msg);
            }
            return resultSessionContext;
        }
        //
        //========================================================================
        //   main_IsAdmin
        //   true if:
        //       Is Authenticated
        //       Is Member
        //       Member has admin or developer status
        //========================================================================
        //
        public bool isAuthenticatedAdmin(coreClass cpCore) {
            bool result = false;
            try {
                result = visit.VisitAuthenticated & (user.Admin | user.Developer);
                //If (Not isAuthenticatedAdmin_cache_isLoaded) And visit_initialized Then
                //    isAuthenticatedAdmin_cache = isAuthenticated() And (user.Admin Or user.Developer)
                //    isAuthenticatedAdmin_cache_isLoaded = True
                //End If
                //result = isAuthenticatedAdmin_cache
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //Private isAuthenticatedAdmin_cache As Boolean = False               ' true if member is administrator
        //Private isAuthenticatedAdmin_cache_isLoaded As Boolean = False              ' true if main_IsAdminCache is initialized
        //
        //========================================================================
        //   main_IsDeveloper
        //========================================================================
        //
        public bool isAuthenticatedDeveloper(coreClass cpCore) {
            bool result = false;
            try {
                result = visit.VisitAuthenticated & (user.Admin | user.Developer);
                //If (Not isAuthenticatedDeveloper_cache_isLoaded) And visit_initialized Then
                //    isAuthenticatedDeveloper_cache = (isAuthenticated() And user.Developer)
                //    isAuthenticatedDeveloper_cache_isLoaded = True
                //End If
                //result = isAuthenticatedDeveloper_cache
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //Private isAuthenticatedDeveloper_cache As Boolean = False
        //Private isAuthenticatedDeveloper_cache_isLoaded As Boolean = False
        //
        //========================================================================
        // main_IsContentManager2
        //   If ContentName is missing, returns true if this is an authenticated member with
        //       content management over anything
        //   If ContentName is given, it only tests this content
        //========================================================================
        //
        public bool isAuthenticatedContentManager(coreClass cpCore, string ContentName = "") {
            bool returnIsContentManager = false;
            try {
                string SQL = null;
                int CS = 0;
                bool notImplemented_allowAdd = false;
                bool notImplemented_allowDelete = false;
                //
                // REFACTOR -- add a private dictionary with contentname=>result, plus a authenticationChange flag that makes properties like this invalid
                //
                returnIsContentManager = false;
                if (string.IsNullOrEmpty(ContentName)) {
                    if (isAuthenticated) {
                        if (isAuthenticatedAdmin(cpCore)) {
                            returnIsContentManager = true;
                        } else {
                            //
                            // Is a CM for any content def
                            //
                            if ((!_isAuthenticatedContentManagerAnything_loaded) || (_isAuthenticatedContentManagerAnything_userId != user.id)) {
                                SQL = "SELECT ccGroupRules.ContentID"
                                    + " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID"
                                    + " WHERE ("
                                        + "(ccMemberRules.MemberID=" + cpCore.db.encodeSQLNumber(user.id) + ")"
                                        + " AND(ccMemberRules.active<>0)"
                                        + " AND(ccGroupRules.active<>0)"
                                        + " AND(ccGroupRules.ContentID Is not Null)"
                                        + " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" + cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) + "))"
                                        + ")";
                                CS = cpCore.db.csOpenSql(SQL);
                                _isAuthenticatedContentManagerAnything = cpCore.db.csOk(CS);
                                cpCore.db.csClose(ref CS);
                                //
                                _isAuthenticatedContentManagerAnything_userId = user.id;
                                _isAuthenticatedContentManagerAnything_loaded = true;
                            }
                            returnIsContentManager = _isAuthenticatedContentManagerAnything;
                        }
                    }
                } else {
                    //
                    // Specific Content called out
                    //
                    getContentAccessRights(cpCore, ContentName, ref returnIsContentManager, ref notImplemented_allowAdd, ref notImplemented_allowDelete);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnIsContentManager;
        }
        private bool _isAuthenticatedContentManagerAnything_loaded = false;
        private int _isAuthenticatedContentManagerAnything_userId;
        private bool _isAuthenticatedContentManagerAnything;
        //
        //========================================================================
        // Member Logout
        //   Create and assign a guest Member identity
        //========================================================================
        //
        public void logout(coreClass cpCore) {
            try {
                logController.logActivity2(cpCore, "logout", user.id, user.OrganizationID);
                //
                // Clear MemberID for this page
                //
                user = personModel.add(cpCore);
                //
                visit.VisitAuthenticated = false;
                visit.save(cpCore);
                //
                visitor.MemberID = user.id;
                visitor.saveObject(cpCore);
                //
                //isAuthenticatedAdmin_cache_isLoaded = False
                //property_user_isMember_isLoaded = False
                //isAuthenticatedDeveloper_cache_isLoaded = False
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //===================================================================================================
        //   Returns the ID of a member given their Username and Password
        //
        //   If the Id can not be found, user errors are added with main_AddUserError and 0 is returned (false)
        //===================================================================================================
        //
        public int authenticateGetId(coreClass cpCore, string username, string password) {
            int returnUserId = 0;
            try {
                const string badLoginUserError = "Your login was not successful. Please try again.";
                //
                string SQL = null;
                bool recordIsAdmin = false;
                bool recordIsDeveloper = false;
                string Criteria = null;
                int CS = 0;
                string iPassword = null;
                bool allowEmailLogin = false;
                bool allowNoPasswordLogin = false;
                string iLoginFieldValue;
                //
                iLoginFieldValue = genericController.encodeText(username);
                iPassword = genericController.encodeText(password);
                //
                returnUserId = 0;
                allowEmailLogin = cpCore.siteProperties.getBoolean("allowEmailLogin");
                allowNoPasswordLogin = cpCore.siteProperties.getBoolean("allowNoPasswordLogin");
                if (string.IsNullOrEmpty(iLoginFieldValue)) {
                    //
                    // ----- loginFieldValue blank, stop here
                    //
                    if (allowEmailLogin) {
                        errorController.addUserError(cpCore, "A valid login requires a non-blank username or email.");
                    } else {
                        errorController.addUserError(cpCore, "A valid login requires a non-blank username.");
                    }
                } else if ((!allowNoPasswordLogin) && (string.IsNullOrEmpty(iPassword))) {
                    //
                    // ----- password blank, stop here
                    //
                    errorController.addUserError(cpCore, "A valid login requires a non-blank password.");
                } else if (visit.LoginAttempts >= cpCore.siteProperties.maxVisitLoginAttempts) {
                    //
                    // ----- already tried 5 times
                    //
                    errorController.addUserError(cpCore, badLoginUserError);
                } else {
                    if (allowEmailLogin) {
                        //
                        // login by username or email
                        //
                        Criteria = "((username=" + cpCore.db.encodeSQLText(iLoginFieldValue) + ")or(email=" + cpCore.db.encodeSQLText(iLoginFieldValue) + "))";
                    } else {
                        //
                        // login by username only
                        //
                        Criteria = "(username=" + cpCore.db.encodeSQLText(iLoginFieldValue) + ")";
                    }
                    if (true) {
                        Criteria = Criteria + "and((dateExpires is null)or(dateExpires>" + cpCore.db.encodeSQLDate(DateTime.Now) + "))";
                    }
                    CS = cpCore.db.csOpen("People", Criteria, "id", SelectFieldList: "ID ,password,admin,developer", PageSize: 2);
                    if (!cpCore.db.csOk(CS)) {
                        //
                        // ----- loginFieldValue not found, stop here
                        //
                        errorController.addUserError(cpCore, badLoginUserError);
                    } else if ((!genericController.encodeBoolean(cpCore.siteProperties.getBoolean("AllowDuplicateUsernames", false))) && (cpCore.db.csGetRowCount(CS) > 1)) {
                        //
                        // ----- AllowDuplicates is false, and there are more then one record
                        //
                        errorController.addUserError(cpCore, "This user account can not be used because the username is not unique on this website. Please contact the site administrator.");
                    } else {
                        //
                        // ----- search all found records for the correct password
                        //
                        while (cpCore.db.csOk(CS)) {
                            returnUserId = 0;
                            //
                            // main_Get Id if password good
                            //
                            if (string.IsNullOrEmpty(iPassword)) {
                                //
                                // no-password-login -- allowNoPassword + no password given + account has no password + account not admin/dev/cm
                                //
                                recordIsAdmin = cpCore.db.csGetBoolean(CS, "admin");
                                recordIsDeveloper = !cpCore.db.csGetBoolean(CS, "admin");
                                if (allowNoPasswordLogin && (cpCore.db.csGetText(CS, "password") == "") && (!recordIsAdmin) && (recordIsDeveloper)) {
                                    returnUserId = cpCore.db.csGetInteger(CS, "ID");
                                    //
                                    // verify they are in no content manager groups
                                    //
                                    SQL = "SELECT ccGroupRules.ContentID"
                                    + " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID"
                                    + " WHERE ("
                                        + "(ccMemberRules.MemberID=" + cpCore.db.encodeSQLNumber(returnUserId) + ")"
                                        + " AND(ccMemberRules.active<>0)"
                                        + " AND(ccGroupRules.active<>0)"
                                        + " AND(ccGroupRules.ContentID Is not Null)"
                                        + " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" + cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) + "))"
                                        + ");";
                                    CS = cpCore.db.csOpenSql(SQL);
                                    if (cpCore.db.csOk(CS)) {
                                        returnUserId = 0;
                                    }
                                    cpCore.db.csClose(ref CS);
                                }
                            } else {
                                //
                                // password login
                                //
                                if (genericController.vbLCase(cpCore.db.csGetText(CS, "password")) == genericController.vbLCase(iPassword)) {
                                    returnUserId = cpCore.db.csGetInteger(CS, "ID");
                                }
                            }
                            if (returnUserId != 0) {
                                break;
                            }
                            cpCore.db.csGoNext(CS);
                        }
                        if (returnUserId == 0) {
                            errorController.addUserError(cpCore, badLoginUserError);
                        }
                    }
                    cpCore.db.csClose(ref CS);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnUserId;
        }
        //
        //====================================================================================================
        //   Checks the username and password for a new login
        //       returns true if this can be used
        //       returns false, and a User Error response if it can not be used
        //
        public bool isNewLoginOK(coreClass cpCore, string Username, string Password, ref string returnErrorMessage, ref int returnErrorCode) {
            bool returnOk = false;
            try {
                int CSPointer = 0;
                //
                returnOk = false;
                if (string.IsNullOrEmpty(Username)) {
                    //
                    // ----- username blank, stop here
                    //
                    returnErrorCode = 1;
                    returnErrorMessage = "A valid login requires a non-blank username.";
                } else if (string.IsNullOrEmpty(Password)) {
                    //
                    // ----- password blank, stop here
                    //
                    returnErrorCode = 4;
                    returnErrorMessage = "A valid login requires a non-blank password.";
                    //    ElseIf Not main_VisitCookieSupport Then
                    //        '
                    //        ' No Cookie Support, can not log in
                    //        '
                    //        errorCode = 2
                    //        errorMessage = "You currently have cookie support disabled in your browser. Without cookies, your browser can not support the level of security required to login."
                } else {

                    CSPointer = cpCore.db.csOpen("People", "username=" + cpCore.db.encodeSQLText(Username), "", false, SelectFieldList: "ID", PageSize: 2);
                    if (cpCore.db.csOk(CSPointer)) {
                        //
                        // ----- username was found, stop here
                        //
                        returnErrorCode = 3;
                        returnErrorMessage = "The username you supplied is currently in use.";
                    } else {
                        returnOk = true;
                    }
                    cpCore.db.csClose(ref CSPointer);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnOk;
        }
        //
        //====================================================================================================
        // main_GetContentAccessRights( ContentIdOrName, returnAllowEdit, returnAllowAdd, returnAllowDelete )
        //
        public void getContentAccessRights(coreClass cpCore, string ContentName, ref bool returnAllowEdit, ref bool returnAllowAdd, ref bool returnAllowDelete) {
            try {
                int ContentID = 0;
                Models.Complex.cdefModel CDef = null;
                //
                returnAllowEdit = false;
                returnAllowAdd = false;
                returnAllowDelete = false;
                if (true) {
                    if (!isAuthenticated) {
                        //
                        // no authenticated, you are not a conent manager
                        //
                    } else if (string.IsNullOrEmpty(ContentName)) {
                        //
                        // no content given, do not handle the general case -- use authcontext.user.main_IsContentManager2()
                        //
                    } else if (isAuthenticatedDeveloper(cpCore)) {
                        //
                        // developers are always content managers
                        //
                        returnAllowEdit = true;
                        returnAllowAdd = true;
                        returnAllowDelete = true;
                    } else if (isAuthenticatedAdmin(cpCore)) {
                        //
                        // admin is content manager if the CDef is not developer only
                        //
                        CDef = cdefModel.getCdef(cpCore, ContentName);
                        if (CDef.Id != 0) {
                            if (!CDef.DeveloperOnly) {
                                returnAllowEdit = true;
                                returnAllowAdd = true;
                                returnAllowDelete = true;
                            }
                        }
                    } else {
                        //
                        // Authenticated and not admin or developer
                        //
                        ContentID = cdefModel.getContentId(cpCore, ContentName);
                        getContentAccessRights_NonAdminByContentId(cpCore, ContentID, ref returnAllowEdit, ref returnAllowAdd, ref returnAllowDelete, "");
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        // main_GetContentAccessRights_NonAdminByContentId
        //   Checks if the member is a content manager for the specific content,
        //   Which includes transversing up the tree to find the next rule that applies'
        //   Member must be checked for authenticated and main_IsAdmin already
        //========================================================================
        //
        private void getContentAccessRights_NonAdminByContentId(coreClass cpCore, int ContentID, ref bool returnAllowEdit, ref bool returnAllowAdd, ref bool returnAllowDelete, string usedContentIdList) {
            try {
                string SQL = null;
                int CSPointer = 0;
                int ParentID = 0;
                string ContentName = null;
                Models.Complex.cdefModel CDef = null;
                //
                returnAllowEdit = false;
                returnAllowAdd = false;
                returnAllowDelete = false;
                if (genericController.IsInDelimitedString(usedContentIdList, ContentID.ToString(), ",")) {
                    //
                    // failed usedContentIdList test, this content id was in the child path
                    //
                    throw new ArgumentException("ContentID [" + ContentID + "] was found to be in it's own parentid path.");
                } else if (ContentID < 1) {
                    //
                    // ----- not a valid contentname
                    //
                } else if (genericController.IsInDelimitedString(contentAccessRights_NotList, ContentID.ToString(), ",")) {
                    //
                    // ----- was previously found to not be a Content Manager
                    //
                } else if (genericController.IsInDelimitedString(contentAccessRights_List, ContentID.ToString(), ",")) {
                    //
                    // ----- was previously found to be a Content Manager
                    //
                    returnAllowEdit = true;
                    returnAllowAdd = genericController.IsInDelimitedString(contentAccessRights_AllowAddList, ContentID.ToString(), ",");
                    returnAllowDelete = genericController.IsInDelimitedString(contentAccessRights_AllowDeleteList, ContentID.ToString(), ",");
                } else {
                    //
                    // ----- Must test it
                    //
                    SQL = "SELECT ccGroupRules.ContentID,allowAdd,allowDelete"
                    + " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID"
                    + " WHERE ("
                        + " (ccMemberRules.MemberID=" + cpCore.db.encodeSQLNumber(user.id) + ")"
                        + " AND(ccMemberRules.active<>0)"
                        + " AND(ccGroupRules.active<>0)"
                        + " AND(ccGroupRules.ContentID=" + ContentID + ")"
                        + " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" + cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) + "))"
                        + ");";
                    CSPointer = cpCore.db.csOpenSql(SQL);
                    if (cpCore.db.csOk(CSPointer)) {
                        returnAllowEdit = true;
                        returnAllowAdd = cpCore.db.csGetBoolean(CSPointer, "allowAdd");
                        returnAllowDelete = cpCore.db.csGetBoolean(CSPointer, "allowDelete");
                    }
                    cpCore.db.csClose(ref CSPointer);
                    //
                    if (!returnAllowEdit) {
                        //
                        // ----- Not a content manager for this one, check the parent
                        //
                        ContentName = cdefModel.getContentNameByID(cpCore, ContentID);
                        if (!string.IsNullOrEmpty(ContentName)) {
                            CDef = cdefModel.getCdef(cpCore, ContentName);
                            ParentID = CDef.parentID;
                            if (ParentID > 0) {
                                getContentAccessRights_NonAdminByContentId(cpCore, ParentID, ref returnAllowEdit, ref returnAllowAdd, ref returnAllowDelete, usedContentIdList + "," + ContentID.ToString());
                            }
                        }
                    }
                    if (returnAllowEdit) {
                        //
                        // ----- Was found to be true
                        //
                        contentAccessRights_List += "," + ContentID.ToString();
                        if (returnAllowAdd) {
                            contentAccessRights_AllowAddList += "," + ContentID.ToString();
                        }
                        if (returnAllowDelete) {
                            contentAccessRights_AllowDeleteList += "," + ContentID.ToString();
                        }
                    } else {
                        //
                        // ----- Was found to be false
                        //
                        contentAccessRights_NotList += "," + ContentID.ToString();
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //========================================================================
        // Member Login (by username and password)
        //
        //   See main_GetLoginMemberID and main_LoginMemberByID
        //========================================================================
        //
        public bool authenticate(coreClass cpCore, string loginFieldValue, string password, bool AllowAutoLogin = false) {
            bool returnREsult = false;
            try {
                int LocalMemberID = 0;
                //
                returnREsult = false;
                LocalMemberID = authenticateGetId(cpCore, loginFieldValue, password);
                if (LocalMemberID != 0) {
                    returnREsult = authenticateById(cpCore, LocalMemberID, this);
                    if (returnREsult) {
                        logController.logActivity2(cpCore, "successful password login", user.id, user.OrganizationID);
                        //isAuthenticatedAdmin_cache_isLoaded = False
                        //property_user_isMember_isLoaded = False
                    } else {
                        logController.logActivity2(cpCore, "unsuccessful login (loginField:" + loginFieldValue + "/password:" + password + ")", user.id, user.OrganizationID);
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnREsult;
        }
        //
        //========================================================================
        //   Member Login By ID
        //
        //========================================================================
        //
        public bool authenticateById(coreClass cpCore, int userId, sessionContextModel authContext) {
            bool returnResult = false;
            try {
                returnResult = recognizeById(cpCore, userId, ref authContext);
                if (returnResult) {
                    //
                    // Log them in
                    //
                    authContext.visit.VisitAuthenticated = true;
                    if (authContext.visit.StartTime == DateTime.MinValue) {
                        authContext.visit.StartTime = cpCore.doc.profileStartTime;
                    }
                    authContext.visit.save(cpCore);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        //   RecognizeMember
        //
        //   the current member to be non-authenticated, but recognized
        //========================================================================
        //
        public bool recognizeById(coreClass cpCore, int userId, ref sessionContextModel sessionContext) {
            bool returnResult = false;
            try {
                if (sessionContext.visitor.ID == 0) {
                    sessionContext.visitor = visitorModel.add(cpCore);
                }
                if (sessionContext.visit.id == 0) {
                    sessionContext.visit = visitModel.add(cpCore);
                }
                sessionContext.user = personModel.create(cpCore, userId);
                sessionContext.visitor.MemberID = sessionContext.user.id;
                sessionContext.visit.MemberID = sessionContext.user.id;
                sessionContext.visit.VisitAuthenticated = false;
                sessionContext.visit.VisitorID = sessionContext.visitor.ID;
                sessionContext.visit.LoginAttempts = 0;
                sessionContext.user.Visits = sessionContext.user.Visits + 1;
                if (sessionContext.user.Visits == 1) {
                    sessionContext.visit.MemberNew = true;
                } else {
                    sessionContext.visit.MemberNew = false;
                }
                sessionContext.user.LastVisit = cpCore.doc.profileStartTime;
                sessionContext.visit.ExcludeFromAnalytics = visit.ExcludeFromAnalytics | sessionContext.visit.Bot | sessionContext.user.ExcludeFromAnalytics | sessionContext.user.Admin | sessionContext.user.Developer;
                sessionContext.visit.save(cpCore);
                sessionContext.visitor.saveObject(cpCore);
                sessionContext.user.save(cpCore);
                returnResult = true;
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        // ----- Returns true if the visitor is an admin, or authenticated and in the group named
        //========================================================================
        //
        public bool IsMemberOfGroup2(coreClass cpCore, string GroupName, int checkMemberID = 0) {
            bool returnREsult = false;
            try {
                int iMemberID = genericController.encodeInteger(checkMemberID);
                if (iMemberID == 0) {
                    iMemberID = user.id;
                }
                returnREsult = isMemberOfGroupList(cpCore, "," + groupController.group_GetGroupID(cpCore, genericController.encodeText(GroupName)), iMemberID, true);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnREsult;
        }
        //
        //========================================================================
        // ----- Returns true if the visitor is a member, and in the group named
        //========================================================================
        //
        public bool isMemberOfGroup(coreClass cpCore, string GroupName, int checkMemberID = 0, bool adminReturnsTrue = false) {
            bool returnREsult = false;
            try {
                int iMemberID = checkMemberID;
                if (iMemberID == 0) {
                    iMemberID = user.id;
                }
                returnREsult = isMemberOfGroupList(cpCore, "," + groupController.group_GetGroupID(cpCore, genericController.encodeText(GroupName)), iMemberID, adminReturnsTrue);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnREsult;
        }

        //
        //========================================================================
        // ----- Returns true if the visitor is an admin, or authenticated and in the group list
        //========================================================================
        //
        public bool isMemberOfGroupList(coreClass cpCore, string GroupIDList, int checkMemberID = 0, bool adminReturnsTrue = false) {
            bool returnREsult = false;
            try {
                if (checkMemberID == 0) {
                    checkMemberID = user.id;
                }
                returnREsult = isMemberOfGroupIdList(cpCore, checkMemberID, isAuthenticated, GroupIDList, adminReturnsTrue);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnREsult;
        }
        //
        //========================================================================
        //   IsMember
        //   true if the user is authenticated and is a trusted people (member content)
        //========================================================================
        //
        public bool isAuthenticatedMember(coreClass cpCore) {
            bool result = false;
            try {
                result = visit.VisitAuthenticated & (Models.Complex.cdefModel.isWithinContent(cpCore, user.contentControlID, cdefModel.getContentId(cpCore, "members")));
                //If (Not property_user_isMember_isLoaded) And (visit_initialized) Then
                //    property_user_isMember = isAuthenticated() And cpCore.IsWithinContent(user.ContentControlID, cpCore.main_GetContentID("members"))
                //    property_user_isMember_isLoaded = True
                //End If
                //result = property_user_isMember
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //Private property_user_isMember As Boolean = False
        //Private property_user_isMember_isLoaded As Boolean = False
        //
        //===============================================================================================================================
        //   Is Group Member of a GroupIDList
        //   admins are always returned true
        //===============================================================================================================================
        //
        public bool isMemberOfGroupIdList(coreClass cpCore, int MemberID, bool isAuthenticated, string GroupIDList) {
            return isMemberOfGroupIdList(cpCore, MemberID, isAuthenticated, GroupIDList, true);
        }
        //
        //===============================================================================================================================
        //   Is Group Member of a GroupIDList
        //===============================================================================================================================
        //
        public bool isMemberOfGroupIdList(coreClass cpCore, int MemberID, bool isAuthenticated, string GroupIDList, bool adminReturnsTrue) {
            bool returnREsult = false;
            try {
                //
                int CS = 0;
                string SQL = null;
                string Criteria = null;
                string WorkingIDList = null;
                //
                returnREsult = false;
                if (isAuthenticated) {
                    WorkingIDList = GroupIDList;
                    WorkingIDList = genericController.vbReplace(WorkingIDList, " ", "");
                    while (genericController.vbInstr(1, WorkingIDList, ",,") != 0) {
                        WorkingIDList = genericController.vbReplace(WorkingIDList, ",,", ",");
                    }
                    if (!string.IsNullOrEmpty(WorkingIDList)) {
                        if (vbMid(WorkingIDList, 1) == ",") {
                            if (vbLen(WorkingIDList) <= 1) {
                                WorkingIDList = "";
                            } else {
                                WorkingIDList = vbMid(WorkingIDList, 2);
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(WorkingIDList)) {
                        if (WorkingIDList.Right(1) == ",") {
                            if (vbLen(WorkingIDList) <= 1) {
                                WorkingIDList = "";
                            } else {
                                WorkingIDList = genericController.vbMid(WorkingIDList, 1, vbLen(WorkingIDList) - 1);
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(WorkingIDList)) {
                        if (adminReturnsTrue) {
                            //
                            // check if memberid is admin
                            //
                            SQL = "select top 1 m.id"
                                + " from ccmembers m"
                                + " where"
                                + " (m.id=" + MemberID + ")"
                                + " and(m.active<>0)"
                                + " and("
                                + " (m.admin<>0)"
                                + " or(m.developer<>0)"
                                + " )"
                                + " ";
                            CS = cpCore.db.csOpenSql_rev("default", SQL);
                            returnREsult = cpCore.db.csOk(CS);
                            cpCore.db.csClose(ref CS);
                        }
                    } else {
                        //
                        // check if they are admin or in the group list
                        //
                        if (genericController.vbInstr(1, WorkingIDList, ",") != 0) {
                            Criteria = "r.GroupID in (" + WorkingIDList + ")";
                        } else {
                            Criteria = "r.GroupID=" + WorkingIDList;
                        }
                        Criteria = ""
                            + "(" + Criteria + ")"
                            + " and(r.id is not null)"
                            + " and((r.DateExpires is null)or(r.DateExpires>" + cpCore.db.encodeSQLDate(DateTime.Now) + "))"
                            + " ";
                        if (adminReturnsTrue) {
                            Criteria = "(" + Criteria + ")or(m.admin<>0)or(m.developer<>0)";
                        }
                        Criteria = ""
                            + "(" + Criteria + ")"
                            + " and(m.active<>0)"
                            + " and(m.id=" + MemberID + ")";
                        //
                        SQL = "select top 1 m.id"
                            + " from ccmembers m"
                            + " left join ccMemberRules r on r.Memberid=m.id"
                            + " where" + Criteria;
                        CS = cpCore.db.csOpenSql_rev("default", SQL);
                        returnREsult = cpCore.db.csOk(CS);
                        cpCore.db.csClose(ref CS);
                    }
                }

            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnREsult;
        }
        //
        //========================================================================
        /// <summary>
        /// is Guest
        /// </summary>
        /// <returns></returns>
        public bool isGuest(coreClass cpCore) {
            return !isAuthenticatedMember(cpCore);
        }
        //
        //========================================================================
        /// <summary>
        /// Is Recognized (not new and not authenticted)
        /// </summary>
        /// <returns></returns>
        public bool isRecognized(coreClass cpCore) {
            return !visit.MemberNew;
        }
        //
        //========================================================================
        /// <summary>
        /// true if editing any content
        /// </summary>
        /// <returns></returns>
        public bool isEditingAnything() {
            return isEditing("");
        }
        //
        //========================================================================
        /// <summary>
        /// True if editing a specific content
        /// </summary>
        /// <param name="ContentNameOrId"></param>
        /// <returns></returns>
        public bool isEditing(string ContentNameOrId) {
            bool returnResult = false;
            try {
                string localContentNameOrId = null;
                string cacheTestName = null;
                localContentNameOrId = genericController.encodeText(ContentNameOrId);
                cacheTestName = localContentNameOrId;
                if (string.IsNullOrEmpty(cacheTestName)) {
                    cacheTestName = "iseditingall";
                }
                cacheTestName = genericController.vbLCase(cacheTestName);
                if (genericController.IsInDelimitedString(main_IsEditingContentList, cacheTestName, ",")) {
                    //
                    // -- 
                    returnResult = true;
                } else if (genericController.IsInDelimitedString(main_IsNotEditingContentList, cacheTestName, ",")) {
                    //
                    // -- 
                    //Call debugController.debug_testPoint(cpCore, "...is in main_IsNotEditingContentList")
                } else {
                    if (isAuthenticated) {
                        if (true) {
                            if (cpCore.visitProperty.getBoolean("AllowEditing") | cpCore.visitProperty.getBoolean("AllowAdvancedEditor")) {
                                if (!string.IsNullOrEmpty(localContentNameOrId)) {
                                    if (localContentNameOrId.IsNumeric()) {
                                        localContentNameOrId = cdefModel.getContentNameByID(cpCore, encodeInteger(localContentNameOrId));
                                    }
                                }
                                returnResult = isAuthenticatedContentManager(cpCore, localContentNameOrId);
                            }
                        }
                    }
                    if (returnResult) {
                        main_IsEditingContentList = main_IsEditingContentList + "," + cacheTestName;
                    } else {
                        main_IsNotEditingContentList = main_IsNotEditingContentList + "," + cacheTestName;
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        /// <summary>
        /// true if editing with the quick editor
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        public bool isQuickEditing(coreClass cpCore, string ContentName) {
            bool returnResult = false;
            try {
                if (true) {
                    if (isAuthenticatedContentManager(cpCore, ContentName)) {
                        returnResult = cpCore.visitProperty.getBoolean("AllowQuickEditor");
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        // main_IsAdvancedEditing( ContentName )
        /// <summary>
        /// true if advanded editing
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        public bool isAdvancedEditing(coreClass cpCore, string ContentName) {
            bool returnResult = false;
            try {
                if (true) {
                    if (isAuthenticatedContentManager(cpCore, ContentName)) {
                        returnResult = cpCore.visitProperty.getBoolean("AllowAdvancedEditor");
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnResult;
        }
        //
        // ================================================================================================
        //
        public static bool isMobile( string browserUserAgent ) {
            Regex b = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Regex v = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return (b.IsMatch(browserUserAgent) || v.IsMatch(browserUserAgent.Substring(0, 4)));
        }
        //
        //   Checks the username and password
        //
        public bool isLoginOK(coreClass cpcore, string Username, string Password, string ErrorMessage = "", int ErrorCode = 0) {
            bool result = (authenticateGetId(cpcore, Username, Password) != 0);
            if (!result) {
                ErrorMessage = errorController.getUserError(cpcore);
            }
            return result;
        }
        //
        // ================================================================================================
        //   conversion pass 2
        // ================================================================================================
        //
        public string main_GetAuthoringStatusMessage(coreClass cpcore, bool IsContentWorkflowAuthoring, bool RecordEditLocked, string main_EditLockName, DateTime main_EditLockExpires, bool RecordApproved, string ApprovedBy, bool RecordSubmitted, string SubmittedBy, bool RecordDeleted, bool RecordInserted, bool RecordModified, string ModifiedBy) {
            string result = "";
            result = "";
            //
            string MethodName = null;
            string Copy = null;
            string Delimiter = "";
            int main_EditLockExpiresMinutes = 0;
            //
            MethodName = "result";
            //
            main_EditLockExpiresMinutes = encodeInteger((main_EditLockExpires - cpcore.doc.profileStartTime).TotalMinutes);
            //
            // ----- site does not support workflow authoring
            //
            if (RecordEditLocked) {
                Copy = genericController.vbReplace(Msg_EditLock, "<EDITNAME>", main_EditLockName);
                Copy = genericController.vbReplace(Copy, "<EDITEXPIRES>", main_EditLockExpires.ToString());
                Copy = genericController.vbReplace(Copy, "<EDITEXPIRESMINUTES>", genericController.encodeText(main_EditLockExpiresMinutes));
                result += Delimiter + Copy;
                Delimiter = "<br>";
            }
            result += Delimiter + Msg_WorkflowDisabled;
            Delimiter = "<br>";
            return result;
        }
        //
        //========================================================================
        // main_IsWorkflowRendering()
        //   True if the current visitor is a content manager in workflow rendering mode
        //========================================================================
        //
        public bool isWorkflowRendering() {
            bool tempisWorkflowRendering = false;
            bool result = false;
            try {
                if (isAuthenticatedContentManager(cpCore)) {
                    tempisWorkflowRendering = cpCore.visitProperty.getBoolean("AllowWorkflowRendering");
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
    }
}