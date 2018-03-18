
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
using Contensive.BaseClasses;
using Contensive.Core.Models.Complex;
//
namespace Contensive.Core.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// session context -- the identity, visit, visitor, view
    /// </summary>
    public class sessionController {
        //
        //====================================================================================================
        // -- this class stores state, so it can hold a pointer to the core instance
        private coreController core { get; set; }
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
                        _language = languageModel.create(core, user.LanguageID);
                    }
                    if (_language == null) {
                        //
                        // -- try browser language if available
                        string HTTP_Accept_Language = Controllers.iisController.getBrowserAcceptLanguage(core);
                        if (!string.IsNullOrEmpty(HTTP_Accept_Language)) {
                            List<languageModel> languageList = languageModel.createList(core, "(HTTP_Accept_Language='" + HTTP_Accept_Language + "')");
                            if (languageList.Count > 0) {
                                _language = languageList[0];
                            }
                        }
                    }
                    if (_language == null) {
                        //
                        // -- try default language
                        string defaultLanguageName = core.siteProperties.getText("Language", "English");
                        _language = languageModel.createByName(core, defaultLanguageName);
                    }
                    if (_language == null) {
                        //
                        // -- try english
                        _language = languageModel.createByName(core, "English");
                    }
                    if (_language == null) {
                        //
                        // -- add english to the table
                        _language = languageModel.add(core);
                        _language.name = "English";
                        _language.HTTP_Accept_Language = "en";
                        _language.save(core);
                        user.LanguageID = _language.id;
                        user.save(core);
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
        public sessionController(coreController core) {
            this.core = core;
            visit = new visitModel();
            visitor = new visitorModel();
            user = new personModel();
        }
        //
        //========================================================================
        /// <summary>
        /// create a new session
        /// </summary>
        /// <param name="core"></param>
        /// <param name="trackVisits">When true, the session is initialized with a visit, visitor, user. Set false for background processing. 
        /// Set true for website processing when allowVisit true.
        /// When false, a visit can be configured on the fly by any application that attempts to access the cp.user.id
        /// </param>
        /// <returns></returns>
        public static sessionController create(coreController core, bool trackVisits) {
            sessionController resultSessionContext = null;
            var sw = Stopwatch.StartNew();
            try {
                if (core.serverConfig == null) {
                    //
                    // -- application error if no server config
                    logController.handleError( core,new ApplicationException("authorization context cannot be created without a server configuration."));
                } else {
                    //
                    if (core.appConfig == null) {
                        //
                        // -- no application, this is a server-only call not related to a 
                        resultSessionContext = new sessionController(core);
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
                        string main_appNameCookiePrefix = genericController.vbLCase(genericController.main_encodeCookieName(core.appConfig.name));
                        //
                        resultSessionContext = new sessionController(core);
                        visitCookieTimestamp = DateTime.MinValue;
                        visitCookie = core.webServer.getRequestCookie(main_appNameCookiePrefix + constants.main_cookieNameVisit);
                        MemberLinkinEID = core.docProperties.getText("eid");
                        MemberLinkLoginID = 0;
                        MemberLinkRecognizeID = 0;
                        if (!string.IsNullOrEmpty(MemberLinkinEID)) {
                            //
                            // -- attempt link authentication
                            if (core.siteProperties.getBoolean("AllowLinkLogin", true)) {
                                //
                                // -- allow Link Login
                                securityController.decodeToken(core, MemberLinkinEID, ref MemberLinkLoginID, ref tokenDate);
                            } else if (core.siteProperties.getBoolean("AllowLinkRecognize", true)) {
                                //
                                // -- allow Link Recognize
                                securityController.decodeToken(core,MemberLinkinEID, ref MemberLinkRecognizeID, ref tokenDate);
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
                                securityController.decodeToken(core, visitCookie, ref cookieVisitId, ref visitCookieTimestamp);
                                if (cookieVisitId == 0) {
                                    //
                                    // -- Bad Cookie, clear it so a new one will be written
                                    visitCookie = "";
                                }
                            }
                            if (cookieVisitId != 0) {
                                //
                                // -- Visit is good, setup visit, then secondary visitor/user if possible
                                resultSessionContext.visit = visitModel.create(core, cookieVisitId);
                                if (resultSessionContext.visit == null) {
                                    //
                                    // -- visit record is missing, create a new visit
                                    resultSessionContext.visit = visitModel.add(core);
                                } else if (resultSessionContext.visit.LastVisitTime.AddHours(1) < core.doc.profileStartTime) {
                                    //
                                    // -- visit has expired, create new visit
                                    resultSessionContext.visit = visitModel.add(core);
                                } else {
                                    //
                                    // -- visit object is valid, share its data with other objects
                                    resultSessionContext.visit.TimeToLastHit = 0;
                                    if (resultSessionContext.visit.StartTime > DateTime.MinValue) {
                                        resultSessionContext.visit.TimeToLastHit = encodeInteger((core.doc.profileStartTime - resultSessionContext.visit.StartTime).TotalSeconds);
                                    }
                                    resultSessionContext.visit.CookieSupport = true;
                                    if (resultSessionContext.visit.VisitorID > 0) {
                                        //
                                        // -- try visit's visitor object
                                        visitorModel testVisitor = visitorModel.create(core, resultSessionContext.visit.VisitorID);
                                        if (testVisitor != null) {
                                            resultSessionContext.visitor = testVisitor;
                                        }
                                    }
                                    if (resultSessionContext.visit.MemberID > 0) {
                                        //
                                        // -- try visit's person object
                                        personModel testUser = personModel.create(core, resultSessionContext.visit.MemberID);
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
                                resultSessionContext.visit = visitModel.add(core);
                                if (string.IsNullOrEmpty(resultSessionContext.visit.name)) {
                                    resultSessionContext.visit.name = "User";
                                }
                                resultSessionContext.visit.PageVisits = 0;
                                resultSessionContext.visit.StartTime = core.doc.profileStartTime;
                                resultSessionContext.visit.StartDateValue = encodeInteger(core.doc.profileStartTime.ToOADate());
                                //
                                // -- setup referrer
                                if (!string.IsNullOrEmpty(core.webServer.requestReferrer)) {
                                    WorkingReferer = core.webServer.requestReferrer;
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
                                if (resultSessionContext.visitor.id == 0) {
                                    //
                                    // -- visit.visitor not valid, create visitor from cookie
                                    CookieVisitor = genericController.encodeText(core.webServer.getRequestCookie(main_appNameCookiePrefix + main_cookieNameVisitor));
                                    if (core.siteProperties.getBoolean("AllowAutoRecognize", true)) {
                                        //
                                        // -- auto recognize, setup user based on visitor
                                        int cookieVisitorId = 0;
                                        securityController.decodeToken(core,CookieVisitor, ref cookieVisitorId, ref tokenDate);
                                        if (cookieVisitorId != 0) {
                                            //
                                            // -- visitor cookie good
                                            visitorModel testVisitor = visitorModel.create(core, cookieVisitorId);
                                            if (testVisitor != null) {
                                                resultSessionContext.visitor = testVisitor;
                                                visitor_changes = true;
                                            }
                                        }
                                    }
                                }
                                //
                                if (resultSessionContext.visitor.id == 0) {
                                    //
                                    // -- create new visitor
                                    resultSessionContext.visitor = visitorModel.add(core);
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
                                    if (recognizeById(core, resultSessionContext.visitor.MemberID, ref resultSessionContext)) {
                                        //
                                        // -- if successful, now test for autologin (authentication)
                                        if (core.siteProperties.AllowAutoLogin & resultSessionContext.user.AutoLogin & resultSessionContext.visit.CookieSupport) {
                                            //
                                            // -- they allow it, now Check if they were logged in on their last visit
                                            visitModel lastVisit = visitModel.getLastVisitByVisitor(core, resultSessionContext.visit.id, resultSessionContext.visitor.id);
                                            if (lastVisit != null) {
                                                if (lastVisit.VisitAuthenticated && (lastVisit.MemberID == resultSessionContext.visit.id)) {
                                                    if (authenticateById(core, resultSessionContext.user.id, resultSessionContext)) {
                                                        logController.addSiteActivity(core, "autologin", resultSessionContext.user.id, resultSessionContext.user.OrganizationID);
                                                        visitor_changes = true;
                                                        user_changes = true;
                                                    }
                                                }
                                            }
                                        } else {
                                            //
                                            // -- Recognized, not auto login
                                            logController.addSiteActivity(core, "recognized", resultSessionContext.user.id, resultSessionContext.user.OrganizationID);
                                        }
                                    }
                                }
                                if (core.webServer.requestBrowser == "") {
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
                                            resultSessionContext.visit.Mobile = isMobile(core.webServer.requestBrowser);
                                            break;
                                    }
                                    //
                                    // -- bot and badBot detect
                                    resultSessionContext.visit.Bot = false;
                                    resultSessionContext.visit_isBadBot = false;
                                    string botFileContent = core.cache.getObject<string>("DefaultBotNameList");
                                    if (string.IsNullOrEmpty(botFileContent)) {
                                        string Filename = "config\\VisitNameList.txt";
                                        botFileContent = core.privateFiles.readFileText(Filename);
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
                                            core.privateFiles.saveFile(Filename, botFileContent);
                                        }
                                        core.cache.setObject("DefaultBotNameList", botFileContent, DateTime.Now.AddHours(1), new List<string>());
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
                                                            if (genericController.vbInstr(1, core.webServer.requestBrowser, Args[1], 1) != 0) {
                                                                resultSessionContext.visit.name = Args[0];
                                                                //visitNameFound = True
                                                                break;
                                                            }
                                                        }
                                                        if (Args.GetUpperBound(0) > 1) {
                                                            // -- process argument 2
                                                            if (!string.IsNullOrEmpty(Args[2].Trim(' '))) {
                                                                if (genericController.vbInstr(1, core.webServer.requestRemoteIP, Args[2], 1) != 0) {
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
                                    core.webServer.addResponseCookie(main_appNameCookiePrefix + main_cookieNameVisitor, securityController.encodeToken( core,resultSessionContext.visitor.id, resultSessionContext.visit.StartTime), resultSessionContext.visit.StartTime.AddYears(1), "", requestAppRootPath, false);
                                }
                                //
                                // -- OnNewVisit Add-on call
                                AllowOnNewVisitEvent = true;
                            }
                            resultSessionContext.visit.LastVisitTime = core.doc.profileStartTime;
                            //
                            // -- verify visitor
                            if (resultSessionContext.visitor.id == 0) {
                                //
                                // -- create new visitor
                                resultSessionContext.visitor = visitorModel.add(core);
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
                                if (authenticateById(core, MemberLinkLoginID, resultSessionContext)) {
                                    logController.addSiteActivity(core, "link login with eid " + MemberLinkinEID, resultSessionContext.user.id, resultSessionContext.user.OrganizationID);
                                }
                            } else if (MemberLinkRecognizeID != 0) {
                                //
                                // -- Link Recognize
                                recognizeById(core, MemberLinkRecognizeID, ref resultSessionContext);
                                logController.addSiteActivity(core, "link recognize with eid " + MemberLinkinEID, resultSessionContext.user.id, resultSessionContext.user.OrganizationID);
                            }
                            //
                            // -- create guest identity if no identity
                            if (resultSessionContext.user.id < 1) {
                                //
                                // -- No user created
                                if (resultSessionContext.visit.name.Left(5).ToLower() != "visit") {
                                    DefaultMemberName = resultSessionContext.visit.name;
                                } else {
                                    DefaultMemberName = genericController.encodeText(Models.Complex.cdefModel.GetContentFieldProperty(core, "people", "name", "default"));
                                }
                                //resultAuthContext.isAuthenticatedAdmin_cache_isLoaded = False
                                //resultAuthContext.property_user_isMember_isLoaded = False
                                //resultAuthContext.isAuthenticatedDeveloper_cache_isLoaded = False
                                TrackGuests = core.siteProperties.getBoolean("track guests", false);
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
                                        resultSessionContext.user = personModel.add(core);
                                        user_changes = true;
                                        resultSessionContext.visitor.MemberID = resultSessionContext.user.id;
                                        visitor_changes = true;
                                        resultSessionContext.visit.VisitAuthenticated = false;
                                        visit_changes = true;
                                    } else {
                                        if (core.siteProperties.trackGuestsWithoutCookies) {
                                            //
                                            // -- create people for non-cookies too
                                            //
                                            resultSessionContext.user = personModel.add(core);
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
                            if (resultSessionContext.visit.VisitorID != resultSessionContext.visitor.id) {
                                resultSessionContext.visit.VisitorID = resultSessionContext.visitor.id;
                                visit_changes = true;
                            }
                            //
                            // -- Save anything that changed
                            resultSessionContext.visit.ExcludeFromAnalytics |= resultSessionContext.visit.Bot | resultSessionContext.user.ExcludeFromAnalytics | resultSessionContext.user.Admin | resultSessionContext.user.Developer;
                            if (!core.webServer.pageExcludeFromAnalytics) {
                                resultSessionContext.visit.PageVisits += 1;
                                visit_changes = true;
                            }
                            resultSessionContext.visit_initialized = true;
                            if (visit_changes) {
                                resultSessionContext.visit.save(core);
                            }
                            if (visitor_changes) {
                                resultSessionContext.visitor.save(core);
                            }
                            if (user_changes) {
                                resultSessionContext.user.save(core);
                            }
                            visitCookieNew = securityController.encodeToken( core,resultSessionContext.visit.id, resultSessionContext.visit.LastVisitTime);
                            if (trackVisits && (visitCookie != visitCookieNew)) {
                                visitCookie = visitCookieNew;
                            }
                        }
                        resultSessionContext.visit_initialized = true;
                        if ((AllowOnNewVisitEvent) && (true)) {
                            CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() { addonType = CPUtilsBaseClass.addonContext.ContextOnNewVisit };
                            foreach (addonModel addon in addonModel.createList_OnNewVisitEvent(core, new List<string>())) {
                                executeContext.errorCaption = addon.name;
                                core.addon.execute(addon, executeContext);
                            }
                        }
                        //
                        // -- Write Visit Cookie
                        visitCookie = securityController.encodeToken( core,resultSessionContext.visit.id, core.doc.profileStartTime);
                        core.webServer.addResponseCookie(main_appNameCookiePrefix + constants.main_cookieNameVisit, visitCookie, default(DateTime), "", requestAppRootPath, false);
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            } finally {
                //
            }
            return resultSessionContext;
        }
        //
        //========================================================================
        /// <summary>
        /// true if the user is authenticate and either an admin or a developer
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public bool isAuthenticatedAdmin(coreController core) {
            bool result = false;
            try {
                result = visit.VisitAuthenticated & (user.Admin | user.Developer);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// true if the user is authenticated and a developoer
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public bool isAuthenticatedDeveloper(coreController core) {
            bool result = false;
            try {
                result = visit.VisitAuthenticated & (user.Admin | user.Developer);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// true if the user is authenticated and has content editing rights to the content provided. If the content is blank, user must be admin or developer.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        public bool isAuthenticatedContentManager(coreController core, string ContentName = "") {
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
                        if (isAuthenticatedAdmin(core)) {
                            returnIsContentManager = true;
                        } else {
                            //
                            // Is a CM for any content def
                            //
                            if ((!_isAuthenticatedContentManagerAnything_loaded) || (_isAuthenticatedContentManagerAnything_userId != user.id)) {
                                SQL = "SELECT ccGroupRules.ContentID"
                                    + " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID"
                                    + " WHERE ("
                                        + "(ccMemberRules.MemberID=" + core.db.encodeSQLNumber(user.id) + ")"
                                        + " AND(ccMemberRules.active<>0)"
                                        + " AND(ccGroupRules.active<>0)"
                                        + " AND(ccGroupRules.ContentID Is not Null)"
                                        + " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" + core.db.encodeSQLDate(core.doc.profileStartTime) + "))"
                                        + ")";
                                CS = core.db.csOpenSql(SQL);
                                _isAuthenticatedContentManagerAnything = core.db.csOk(CS);
                                core.db.csClose(ref CS);
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
                    getContentAccessRights(core, ContentName, ref returnIsContentManager, ref notImplemented_allowAdd, ref notImplemented_allowDelete);
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return returnIsContentManager;
        }
        private bool _isAuthenticatedContentManagerAnything_loaded = false;
        private int _isAuthenticatedContentManagerAnything_userId;
        private bool _isAuthenticatedContentManagerAnything;
        //
        //========================================================================
        /// <summary>
        /// logout user
        /// </summary>
        /// <param name="core"></param>
        public void logout(coreController core) {
            try {
                logController.addSiteActivity(core, "logout", user.id, user.OrganizationID);
                //
                // new guest
                user = personModel.add(core);
                visit.MemberID = user.id;
                visit.VisitAuthenticated = false;
                visit.save(core);
                visitor.MemberID = user.id;
                visitor.save(core);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //===================================================================================================
        /// <summary>
        /// Returns the ID of a member given their Username and Password, If the Id can not be found, user errors are added with main_AddUserError and 0 is returned (false)
        /// </summary>
        /// <param name="core"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public int getUserIdForCredentials(coreController core, string username, string password) {
            int returnUserId = 0;
            try {
                //
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
                allowEmailLogin = core.siteProperties.getBoolean("allowEmailLogin");
                allowNoPasswordLogin = core.siteProperties.getBoolean("allowNoPasswordLogin");
                if (string.IsNullOrEmpty(iLoginFieldValue)) {
                    //
                    // ----- loginFieldValue blank, stop here
                    //
                    if (allowEmailLogin) {
                        errorController.addUserError(core, "A valid login requires a non-blank username or email.");
                    } else {
                        errorController.addUserError(core, "A valid login requires a non-blank username.");
                    }
                } else if ((!allowNoPasswordLogin) && (string.IsNullOrEmpty(iPassword))) {
                    //
                    // ----- password blank, stop here
                    //
                    errorController.addUserError(core, "A valid login requires a non-blank password.");
                } else if (visit.LoginAttempts >= core.siteProperties.maxVisitLoginAttempts) {
                    //
                    // ----- already tried 5 times
                    //
                    errorController.addUserError(core, badLoginUserError);
                } else {
                    if (allowEmailLogin) {
                        //
                        // login by username or email
                        //
                        Criteria = "((username=" + core.db.encodeSQLText(iLoginFieldValue) + ")or(email=" + core.db.encodeSQLText(iLoginFieldValue) + "))";
                    } else {
                        //
                        // login by username only
                        //
                        Criteria = "(username=" + core.db.encodeSQLText(iLoginFieldValue) + ")";
                    }
                    if (true) {
                        Criteria = Criteria + "and((dateExpires is null)or(dateExpires>" + core.db.encodeSQLDate(DateTime.Now) + "))";
                    }
                    CS = core.db.csOpen("People", Criteria, "id", SelectFieldList: "ID ,password,admin,developer", PageSize: 2);
                    if (!core.db.csOk(CS)) {
                        //
                        // ----- loginFieldValue not found, stop here
                        //
                        errorController.addUserError(core, badLoginUserError);
                    } else if ((!genericController.encodeBoolean(core.siteProperties.getBoolean("AllowDuplicateUsernames", false))) && (core.db.csGetRowCount(CS) > 1)) {
                        //
                        // ----- AllowDuplicates is false, and there are more then one record
                        //
                        errorController.addUserError(core, "This user account can not be used because the username is not unique on this website. Please contact the site administrator.");
                    } else {
                        //
                        // ----- search all found records for the correct password
                        //
                        while (core.db.csOk(CS)) {
                            returnUserId = 0;
                            //
                            // main_Get Id if password good
                            //
                            if (string.IsNullOrEmpty(iPassword)) {
                                //
                                // no-password-login -- allowNoPassword + no password given + account has no password + account not admin/dev/cm
                                //
                                recordIsAdmin = core.db.csGetBoolean(CS, "admin");
                                recordIsDeveloper = !core.db.csGetBoolean(CS, "admin");
                                if (allowNoPasswordLogin && (core.db.csGetText(CS, "password") == "") && (!recordIsAdmin) && (recordIsDeveloper)) {
                                    returnUserId = core.db.csGetInteger(CS, "ID");
                                    //
                                    // verify they are in no content manager groups
                                    //
                                    SQL = "SELECT ccGroupRules.ContentID"
                                    + " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID"
                                    + " WHERE ("
                                        + "(ccMemberRules.MemberID=" + core.db.encodeSQLNumber(returnUserId) + ")"
                                        + " AND(ccMemberRules.active<>0)"
                                        + " AND(ccGroupRules.active<>0)"
                                        + " AND(ccGroupRules.ContentID Is not Null)"
                                        + " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" + core.db.encodeSQLDate(core.doc.profileStartTime) + "))"
                                        + ");";
                                    CS = core.db.csOpenSql(SQL);
                                    if (core.db.csOk(CS)) {
                                        returnUserId = 0;
                                    }
                                    core.db.csClose(ref CS);
                                }
                            } else {
                                //
                                // password login
                                //
                                if (genericController.vbLCase(core.db.csGetText(CS, "password")) == genericController.vbLCase(iPassword)) {
                                    returnUserId = core.db.csGetInteger(CS, "ID");
                                }
                            }
                            if (returnUserId != 0) {
                                break;
                            }
                            core.db.csGoNext(CS);
                        }
                        if (returnUserId == 0) {
                            errorController.addUserError(core, badLoginUserError);
                        }
                    }
                    core.db.csClose(ref CS);
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return returnUserId;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Checks the username and password for a new login, returns true if this can be used, returns false, and a User Error response if it can not be used
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="returnErrorMessage"></param>
        /// <param name="returnErrorCode"></param>
        /// <returns></returns>
        public bool isNewCredentialOK(coreController core, string Username, string Password, ref string returnErrorMessage, ref int returnErrorCode) {
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

                    CSPointer = core.db.csOpen("People", "username=" + core.db.encodeSQLText(Username), "", false, SelectFieldList: "ID", PageSize: 2);
                    if (core.db.csOk(CSPointer)) {
                        //
                        // ----- username was found, stop here
                        //
                        returnErrorCode = 3;
                        returnErrorMessage = "The username you supplied is currently in use.";
                    } else {
                        returnOk = true;
                    }
                    core.db.csClose(ref CSPointer);
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return returnOk;
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns if the user can edit, add or delete records from a content
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ContentName"></param>
        /// <param name="returnAllowEdit"></param>
        /// <param name="returnAllowAdd"></param>
        /// <param name="returnAllowDelete"></param>
        public void getContentAccessRights(coreController core, string ContentName, ref bool returnAllowEdit, ref bool returnAllowAdd, ref bool returnAllowDelete) {
            try {
                int ContentID = 0;
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
                    } else if (isAuthenticatedDeveloper(core)) {
                        //
                        // developers are always content managers
                        //
                        returnAllowEdit = true;
                        returnAllowAdd = true;
                        returnAllowDelete = true;
                    } else if (isAuthenticatedAdmin(core)) {
                        //
                        // admin is content manager if the CDef is not developer only
                        //
                        cdefModel CDef = cdefModel.getCdef(core, ContentName);
                        if (CDef.id != 0) {
                            if (!CDef.developerOnly) {
                                returnAllowEdit = true;
                                returnAllowAdd = true;
                                returnAllowDelete = true;
                            }
                        }
                    } else {
                        //
                        // Authenticated and not admin or developer
                        //
                        ContentID = cdefModel.getContentId(core, ContentName);
                        getContentAccessRights_NonAdminByContentId(core, ContentID, ref returnAllowEdit, ref returnAllowAdd, ref returnAllowDelete, "");
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Checks if the member is a content manager for the specific content, Which includes transversing up the tree to find the next rule that applies. Member must be checked for authenticated and main_IsAdmin already
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ContentID"></param>
        /// <param name="returnAllowEdit"></param>
        /// <param name="returnAllowAdd"></param>
        /// <param name="returnAllowDelete"></param>
        /// <param name="usedContentIdList"></param>
        //========================================================================
        //
        private void getContentAccessRights_NonAdminByContentId(coreController core, int ContentID, ref bool returnAllowEdit, ref bool returnAllowAdd, ref bool returnAllowDelete, string usedContentIdList) {
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
                if (genericController.isInDelimitedString(usedContentIdList, ContentID.ToString(), ",")) {
                    //
                    // failed usedContentIdList test, this content id was in the child path
                    //
                    throw new ArgumentException("ContentID [" + ContentID + "] was found to be in it's own parentid path.");
                } else if (ContentID < 1) {
                    //
                    // ----- not a valid contentname
                    //
                } else if (genericController.isInDelimitedString(contentAccessRights_NotList, ContentID.ToString(), ",")) {
                    //
                    // ----- was previously found to not be a Content Manager
                    //
                } else if (genericController.isInDelimitedString(contentAccessRights_List, ContentID.ToString(), ",")) {
                    //
                    // ----- was previously found to be a Content Manager
                    //
                    returnAllowEdit = true;
                    returnAllowAdd = genericController.isInDelimitedString(contentAccessRights_AllowAddList, ContentID.ToString(), ",");
                    returnAllowDelete = genericController.isInDelimitedString(contentAccessRights_AllowDeleteList, ContentID.ToString(), ",");
                } else {
                    //
                    // ----- Must test it
                    //
                    SQL = "SELECT ccGroupRules.ContentID,allowAdd,allowDelete"
                    + " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID"
                    + " WHERE ("
                        + " (ccMemberRules.MemberID=" + core.db.encodeSQLNumber(user.id) + ")"
                        + " AND(ccMemberRules.active<>0)"
                        + " AND(ccGroupRules.active<>0)"
                        + " AND(ccGroupRules.ContentID=" + ContentID + ")"
                        + " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" + core.db.encodeSQLDate(core.doc.profileStartTime) + "))"
                        + ");";
                    CSPointer = core.db.csOpenSql(SQL);
                    if (core.db.csOk(CSPointer)) {
                        returnAllowEdit = true;
                        returnAllowAdd = core.db.csGetBoolean(CSPointer, "allowAdd");
                        returnAllowDelete = core.db.csGetBoolean(CSPointer, "allowDelete");
                    }
                    core.db.csClose(ref CSPointer);
                    //
                    if (!returnAllowEdit) {
                        //
                        // ----- Not a content manager for this one, check the parent
                        //
                        ContentName = cdefModel.getContentNameByID(core, ContentID);
                        if (!string.IsNullOrEmpty(ContentName)) {
                            CDef = cdefModel.getCdef(core, ContentName);
                            ParentID = CDef.parentID;
                            if (ParentID > 0) {
                                getContentAccessRights_NonAdminByContentId(core, ParentID, ref returnAllowEdit, ref returnAllowAdd, ref returnAllowDelete, usedContentIdList + "," + ContentID.ToString());
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
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// Login (by username and password)
        /// </summary>
        /// <param name="core"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="AllowAutoLogin"></param>
        /// <returns></returns>
        public bool authenticate(coreController core, string username, string password, bool AllowAutoLogin = false) {
            bool result = false;
            try {
                int userId = getUserIdForCredentials(core, username, password);
                if (userId != 0) {
                    result = authenticateById(core, userId, this);
                    if (result) {
                        logController.addSiteActivity(core, "successful password login, username [" + username + "]", user.id, user.OrganizationID);
                    } else {
                        logController.addSiteActivity(core, "unsuccessful password login, username [" + username + "]", user.id, user.OrganizationID);
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// Member Login By ID
        /// </summary>
        /// <param name="core"></param>
        /// <param name="userId"></param>
        /// <param name="authContext"></param>
        /// <returns></returns>
        public static bool authenticateById(coreController core, int userId, sessionController authContext) {
            bool result = false;
            try {
                result = recognizeById(core, userId, ref authContext);
                if (result) {
                    //
                    // Log them in
                    authContext.visit.VisitAuthenticated = true;
                    if (authContext.visit.StartTime == DateTime.MinValue) {
                        authContext.visit.StartTime = core.doc.profileStartTime;
                    }
                    authContext.visit.save(core);
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// RecognizeMember the current member to be non-authenticated, but recognized
        /// </summary>
        /// <param name="core"></param>
        /// <param name="userId"></param>
        /// <param name="sessionContext"></param>
        /// <returns></returns>
        //
        public static bool recognizeById(coreController core, int userId, ref sessionController sessionContext) {
            bool result = false;
            try {
                if (sessionContext.visitor.id == 0) {
                    sessionContext.visitor = visitorModel.add(core);
                }
                if (sessionContext.visit.id == 0) {
                    sessionContext.visit = visitModel.add(core);
                }
                sessionContext.user = personModel.create(core, userId);
                sessionContext.visitor.MemberID = sessionContext.user.id;
                sessionContext.visit.MemberID = sessionContext.user.id;
                sessionContext.visit.VisitAuthenticated = false;
                sessionContext.visit.VisitorID = sessionContext.visitor.id;
                sessionContext.visit.LoginAttempts = 0;
                sessionContext.user.Visits = sessionContext.user.Visits + 1;
                if (sessionContext.user.Visits == 1) {
                    sessionContext.visit.MemberNew = true;
                } else {
                    sessionContext.visit.MemberNew = false;
                }
                sessionContext.user.LastVisit = core.doc.profileStartTime;
                sessionContext.visit.ExcludeFromAnalytics = sessionContext.visit.ExcludeFromAnalytics | sessionContext.visit.Bot | sessionContext.user.ExcludeFromAnalytics | sessionContext.user.Admin | sessionContext.user.Developer;
                sessionContext.visit.save(core);
                sessionContext.visitor.save(core);
                sessionContext.user.save(core);
                result = true;
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// Returns true if the visitor is an admin, or authenticated and in the group named
        /// </summary>
        /// <param name="core"></param>
        /// <param name="GroupName"></param>
        /// <param name="checkMemberID"></param>
        /// <returns></returns>
        //
        public bool isMemberOfGroup(coreController core, string GroupName, int checkMemberID = 0) {
            bool result = false;
            try {
                int iMemberID = genericController.encodeInteger(checkMemberID);
                if (iMemberID == 0) {
                    iMemberID = user.id;
                }
                result = isMemberOfGroupList(core, "," + groupController.group_GetGroupID(core, genericController.encodeText(GroupName)), iMemberID, true);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        // ----- Returns true if the visitor is an admin, or authenticated and in the group list
        //========================================================================
        //
        public bool isMemberOfGroupList(coreController core, string GroupIDList, int checkMemberID = 0, bool adminReturnsTrue = false) {
            bool result = false;
            try {
                if (checkMemberID == 0) {
                    checkMemberID = user.id;
                }
                result = isMemberOfGroupIdList(core, checkMemberID, isAuthenticated, GroupIDList, adminReturnsTrue);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        //   IsMember
        //   true if the user is authenticated and is a trusted people (member content)
        //========================================================================
        //
        public bool isAuthenticatedMember(coreController core) {
            bool result = false;
            try {
                result = visit.VisitAuthenticated & (Models.Complex.cdefModel.isWithinContent(core, user.contentControlID, cdefModel.getContentId(core, "members")));
                //If (Not property_user_isMember_isLoaded) And (visit_initialized) Then
                //    property_user_isMember = isAuthenticated() And core.IsWithinContent(user.ContentControlID, core.main_GetContentID("members"))
                //    property_user_isMember_isLoaded = True
                //End If
                //result = property_user_isMember
            } catch (Exception ex) {
                logController.handleError( core,ex);
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
        public bool isMemberOfGroupIdList(coreController core, int MemberID, bool isAuthenticated, string GroupIDList) {
            return isMemberOfGroupIdList(core, MemberID, isAuthenticated, GroupIDList, true);
        }
        //
        //===============================================================================================================================
        //   Is Group Member of a GroupIDList
        //===============================================================================================================================
        //
        public bool isMemberOfGroupIdList(coreController core, int MemberID, bool isAuthenticated, string GroupIDList, bool adminReturnsTrue) {
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
                            CS = core.db.csOpenSql(SQL,"Default");
                            returnREsult = core.db.csOk(CS);
                            core.db.csClose(ref CS);
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
                            + " and((r.DateExpires is null)or(r.DateExpires>" + core.db.encodeSQLDate(DateTime.Now) + "))"
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
                        CS = core.db.csOpenSql(SQL,"Default");
                        returnREsult = core.db.csOk(CS);
                        core.db.csClose(ref CS);
                    }
                }

            } catch (Exception ex) {
                logController.handleError( core,ex);
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
        public bool isGuest(coreController core) {
            return !isRecognized(core);
        }
        //
        //========================================================================
        /// <summary>
        /// Is Recognized (not new and not authenticted)
        /// </summary>
        /// <returns></returns>
        public bool isRecognized(coreController core) {
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
        /// <param name="contentNameOrId"></param>
        /// <returns></returns>
        public bool isEditing(string contentNameOrId) {
            bool returnResult = false;
            try {
                string cacheTestName = contentNameOrId.ToLower();
                if (string.IsNullOrEmpty(cacheTestName)) {
                    cacheTestName = "iseditingall";
                }
                if (genericController.isInDelimitedString(main_IsEditingContentList, cacheTestName, ",")) {
                    //
                    // -- 
                    returnResult = true;
                } else if (genericController.isInDelimitedString(main_IsNotEditingContentList, cacheTestName, ",")) {
                    //
                    // -- 
                    //Call debugController.debug_testPoint(core, "...is in main_IsNotEditingContentList")
                } else {
                    if (isAuthenticated) {
                        if (true) {
                            if (core.visitProperty.getBoolean("AllowEditing") | core.visitProperty.getBoolean("AllowAdvancedEditor")) {
                                if (!string.IsNullOrEmpty(contentNameOrId)) {
                                    if (contentNameOrId.IsNumeric()) {
                                        contentNameOrId = cdefModel.getContentNameByID(core, encodeInteger(contentNameOrId));
                                    }
                                }
                                returnResult = isAuthenticatedContentManager(core, contentNameOrId);
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
                logController.handleError( core,ex);
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
        public bool isQuickEditing(coreController core, string ContentName) {
            bool returnResult = false;
            try {
                if (isAuthenticatedContentManager(core, ContentName)) {
                    returnResult = core.visitProperty.getBoolean("AllowQuickEditor");
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
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
        public bool isAdvancedEditing(coreController core, string ContentName) {
            bool returnResult = false;
            try {
                if (isAuthenticatedContentManager(core, ContentName)) {
                    returnResult = core.visitProperty.getBoolean("AllowAdvancedEditor");
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
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
        public bool isLoginOK(coreController core, string Username, string Password, string ErrorMessage = "", int ErrorCode = 0) {
            bool result = (getUserIdForCredentials(core, Username, Password) != 0);
            if (!result) {
                ErrorMessage = errorController.getUserError(core);
            }
            return result;
        }
        //
        // ================================================================================================
        //   conversion pass 2
        // ================================================================================================
        //
        public string getAuthoringStatusMessage(coreController core, bool IsContentWorkflowAuthoring, bool RecordEditLocked, string main_EditLockName, DateTime main_EditLockExpires, bool RecordApproved, string ApprovedBy, bool RecordSubmitted, string SubmittedBy, bool RecordDeleted, bool RecordInserted, bool RecordModified, string ModifiedBy) {
            string result = "";
            //
            string Copy = null;
            string Delimiter = "";
            int main_EditLockExpiresMinutes = encodeInteger((main_EditLockExpires - core.doc.profileStartTime).TotalMinutes);
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
                if (isAuthenticatedContentManager(core)) {
                    tempisWorkflowRendering = core.visitProperty.getBoolean("AllowWorkflowRendering");
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return result;
        }
    }
}