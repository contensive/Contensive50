
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Processor.Models.Db;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.BaseClasses;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Exceptions;
//
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// session context -- the identity, visit, visitor, view
    /// </summary>
    public class SessionController {
        //
        //====================================================================================================
        /// <summary>
        /// this class stores state, so it can hold a pointer to the core instance
        /// </summary>
        private CoreController core { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// the visit is the collection of pages, constructor creates default non-authenticated instance
        /// </summary>
        public VisitModel visit { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// visitor represents the browser, constructor creates default non-authenticated instance
        /// </summary>
        public VisitorModel visitor { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// user is the person at the keyboad, constructor creates default non-authenticated instance
        /// </summary>
        public PersonModel user { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// userLanguage will return a valid populated language object
        /// </summary>
        /// <returns></returns>
        public LanguageModel userLanguage {
            get {
                if ((_language == null) && (user != null)) {
                    if (user.languageID > 0) {
                        //
                        // -- get user language
                        _language = LanguageModel.create(core, user.languageID);
                    }
                    if (_language == null) {
                        //
                        // -- try browser language if available
                        string HTTP_Accept_Language = Controllers.WebServerController.getBrowserAcceptLanguage(core);
                        if (!string.IsNullOrEmpty(HTTP_Accept_Language)) {
                            List<LanguageModel> languageList = LanguageModel.createList(core, "(HTTP_Accept_Language='" + HTTP_Accept_Language + "')");
                            if (languageList.Count > 0) {
                                _language = languageList[0];
                            }
                        }
                    }
                    if (_language == null) {
                        //
                        // -- try default language
                        string defaultLanguageName = core.siteProperties.getText("Language", "English");
                        _language = LanguageModel.createByUniqueName(core, defaultLanguageName);
                    }
                    if (_language == null) {
                        //
                        // -- try english
                        _language = LanguageModel.createByUniqueName(core, "English");
                    }
                    if (_language == null) {
                        //
                        // -- add english to the table
                        _language = LanguageModel.addDefault(core, Models.Domain.MetaModel.createByUniqueName( core, LanguageModel.contentName));
                        _language.name = "English";
                        _language.http_Accept_Language = "en";
                        _language.save(core);
                        user.languageID = _language.id;
                        user.save(core);
                    }
                }
                return _language;
            }
        } private LanguageModel _language = null;
        //
        //====================================================================================================
        /// <summary>
        /// is this user authenticated in this visit
        /// </summary>
        public bool isAuthenticated {
            get {
                return visit.visitAuthenticated;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// browser is identified as a bot, and is not on the friendly-bot list
        /// </summary>
        public bool visitBadBot { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// The current request carries a cookie from the last request (use to detect back-button). if false, page is out of state (sequence)
        /// </summary>
        public bool visitStateOk { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// constructor, no arguments, created default authentication model for use without user, and before user is available
        /// </summary>
        public SessionController(CoreController core) {
            this.core = core;
            visit = new VisitModel();
            visitor = new VisitorModel();
            user = new PersonModel();
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
        public static SessionController create(CoreController core, bool trackVisits) {
            //
            LogController.logTrace(core, "SessionController.create(), enter");
            //
            SessionController resultSessionContext = null;
            var sw = Stopwatch.StartNew();
            try {
                if (core.serverConfig == null) {
                    //
                    // -- application error if no server config
                    LogController.handleError( core,new GenericException("authorization context cannot be created without a server configuration."));
                } else {
                    //
                    if (core.appConfig == null) {
                        //
                        // -- no application, this is a server-only call not related to a 
                        resultSessionContext = new SessionController(core);
                        LogController.logTrace(core, "SessionController.create(), app.config null, create server session");
                    } else {
                        //
                        resultSessionContext = new SessionController(core);
                        string appNameCookiePrefix = encodeCookieName(core.appConfig.name);
                        string visitCookie = core.webServer.getRequestCookie(appNameCookiePrefix + cookieNameVisit);
                        string memberLinkinEID = core.docProperties.getText("eid");
                        int memberLinkRecognizeID = 0;
                        //
                        LogController.logTrace(core, "SessionController.create(), visitCookie [" + visitCookie + "], MemberLinkinEID [" + memberLinkinEID + "]");
                        //
                        var linkToken = new SecurityController.TokenData();
                        if (!string.IsNullOrEmpty(memberLinkinEID)) {
                            //
                            // -- attempt link authentication
                            if (core.siteProperties.getBoolean("AllowLinkLogin", true)) {
                                //
                                // -- allow Link Login
                                linkToken = SecurityController.decodeToken(core, memberLinkinEID);
                            } else if (core.siteProperties.getBoolean("AllowLinkRecognize", true)) {
                                //
                                // -- allow Link Recognize
                                linkToken = SecurityController.decodeToken(core, memberLinkinEID);
                            } else {
                                //
                                // -- block link login
                                memberLinkinEID = "";
                            }
                        }
                        bool AllowOnNewVisitEvent = false;
                        if ((trackVisits) || (!string.IsNullOrEmpty(visitCookie)) || (linkToken.id != 0) || (memberLinkRecognizeID != 0)) {
                            //
                            // -- Visit Tracking
                            //
                            LogController.logTrace(core, "SessionController.create(), visittracking");
                            var visitToken = new SecurityController.TokenData();
                            if (!string.IsNullOrEmpty(visitCookie)) {
                                //
                                // -- visit cookie found
                                visitToken = SecurityController.decodeToken(core, visitCookie);
                                if (visitToken.id == 0) {
                                    //
                                    // -- Bad Cookie, clear it so a new one will be written
                                    visitCookie = "";
                                    LogController.logInfo(core, "SessionController.create(), BAD COOKIE");
                                }
                            }
                            if (visitToken.id != 0) {
                                //
                                // -- Visit is good, setup visit, then secondary visitor/user if possible
                                LogController.logTrace(core, "SessionController.create(), valid cookieVisit [" + visitToken.id + "]");
                                resultSessionContext.visit = VisitModel.create(core, visitToken.id);
                                if (resultSessionContext.visit == null) {
                                    //
                                    // -- visit record is missing, create a new visit
                                    LogController.logTrace(core, "SessionController.create(), visit record is missing, create a new visit");
                                    resultSessionContext.visit = VisitModel.addEmpty(core);
                                } else if (resultSessionContext.visit.lastVisitTime.AddHours(1) < core.doc.profileStartTime) {
                                    //
                                    // -- visit has expired, create new visit
                                    LogController.logTrace(core, "SessionController.create(), visit has expired, create new visit");
                                    resultSessionContext.visit = VisitModel.addEmpty(core);
                                } else {
                                    //
                                    // -- visit object is valid, share its data with other objects
                                    resultSessionContext.visit.timeToLastHit = 0;
                                    if (resultSessionContext.visit.startTime > DateTime.MinValue) {
                                        resultSessionContext.visit.timeToLastHit = encodeInteger((core.doc.profileStartTime - resultSessionContext.visit.startTime).TotalSeconds);
                                    }
                                    resultSessionContext.visit.cookieSupport = true;
                                    if (resultSessionContext.visit.visitorID > 0) {
                                        //
                                        // -- try visit's visitor object
                                        VisitorModel testVisitor = VisitorModel.create(core, resultSessionContext.visit.visitorID);
                                        if (testVisitor != null) {
                                            resultSessionContext.visitor = testVisitor;
                                        }
                                    }
                                    if (resultSessionContext.visit.memberID > 0) {
                                        //
                                        // -- try visit's person object
                                        PersonModel testUser = PersonModel.create(core, resultSessionContext.visit.memberID);
                                        if (testUser != null) {
                                            resultSessionContext.user = testUser;
                                        }
                                    }
                                    if (((visitToken.timeStamp - resultSessionContext.visit.lastVisitTime).TotalSeconds) > 2) {
                                        LogController.logTrace(core, "SessionController.create(), visit cookie timestamp [" + visitToken.timeStamp + "] does not match lastvisittime [" + resultSessionContext.visit.lastVisitTime + "]");
                                        resultSessionContext.visitStateOk = false;
                                    }
                                }
                            }
                            //
                            LogController.logTrace(core, "SessionController.create(), load session from cookied complete, visit.id [" + resultSessionContext.visit.id + "], visitor.id [" + resultSessionContext.visitor.id + "], user.id [" + resultSessionContext.user.id + "]");
                            //
                            bool visit_changes = false;
                            bool visitor_changes = false;
                            bool user_changes = false;
                            if (resultSessionContext.visit.id == 0) {
                                //
                                // -- create new visit record
                                LogController.logTrace(core, "SessionController.create(), visit id=0, create new visit");
                                resultSessionContext.visit = VisitModel.addEmpty(core);
                                if (string.IsNullOrEmpty(resultSessionContext.visit.name)) {
                                    resultSessionContext.visit.name = "User";
                                }
                                resultSessionContext.visit.pageVisits = 0;
                                resultSessionContext.visit.startTime = core.doc.profileStartTime;
                                resultSessionContext.visit.startDateValue = encodeInteger(core.doc.profileStartTime.ToOADate());
                                //
                                // -- setup referrer
                                if (!string.IsNullOrEmpty(core.webServer.requestReferrer)) {
                                    string WorkingReferer = core.webServer.requestReferrer;
                                    int SlashPosition = GenericController.vbInstr(1, WorkingReferer, "//");
                                    if ((SlashPosition != 0) && (WorkingReferer.Length > (SlashPosition + 2))) {
                                        WorkingReferer = WorkingReferer.Substring(SlashPosition + 1);
                                    }
                                    SlashPosition = GenericController.vbInstr(1, WorkingReferer, "/");
                                    if (SlashPosition == 0) {
                                        resultSessionContext.visit.refererPathPage = "";
                                        resultSessionContext.visit.http_referer = WorkingReferer;
                                    } else {
                                        resultSessionContext.visit.refererPathPage = WorkingReferer.Substring(SlashPosition - 1);
                                        resultSessionContext.visit.http_referer = WorkingReferer.Left(SlashPosition - 1);
                                    }
                                }
                                //
                                if (resultSessionContext.visitor.id == 0) {
                                    //
                                    // -- visit.visitor not valid, create visitor from cookie
                                    string CookieVisitor = GenericController.encodeText(core.webServer.getRequestCookie(appNameCookiePrefix + main_cookieNameVisitor));
                                    if (core.siteProperties.getBoolean("AllowAutoRecognize", true)) {
                                        //
                                        // -- auto recognize, setup user based on visitor
                                        var visitorToken = SecurityController.decodeToken(core, CookieVisitor);
                                        if (visitorToken.id != 0) {
                                            //
                                            // -- visitor cookie good
                                            VisitorModel testVisitor = VisitorModel.create(core, visitorToken.id);
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
                                    resultSessionContext.visitor = VisitorModel.addEmpty(core);
                                    visitor_changes = false;
                                    //
                                    resultSessionContext.visit.visitorNew = true;
                                    visit_changes = true;
                                }
                                //
                                // -- find  identity from the visitor
                                if (resultSessionContext.visitor.MemberID > 0) {
                                    //
                                    // -- recognize by the main_VisitorMemberID
                                    if (recognizeById(core, resultSessionContext.visitor.MemberID, ref resultSessionContext)) {
                                        //    //
                                        //    // -- id presented, but did not work. create dummy user
                                        //    resultSessionContext.user = new personModel();
                                        //} else {
                                        //
                                        // -- if successful, now test for autologin (authentication)
                                        if (core.siteProperties.allowAutoLogin & resultSessionContext.user.autoLogin & resultSessionContext.visit.cookieSupport) {
                                            //
                                            // -- they allow it, now Check if they were logged in on their last visit
                                            VisitModel lastVisit = VisitModel.getLastVisitByVisitor(core, resultSessionContext.visit.id, resultSessionContext.visitor.id);
                                            if (lastVisit != null) {
                                                if (lastVisit.visitAuthenticated && (lastVisit.memberID == resultSessionContext.visit.id)) {
                                                    if (authenticateById(core, resultSessionContext.user.id, resultSessionContext)) {
                                                        LogController.addSiteActivity(core, "autologin", resultSessionContext.user.id, resultSessionContext.user.organizationID);
                                                        visitor_changes = true;
                                                        user_changes = true;
                                                    }
                                                }
                                            }
                                        } else {
                                            //
                                            // -- Recognized, not auto login
                                            LogController.addSiteActivity(core, "recognized", resultSessionContext.user.id, resultSessionContext.user.organizationID);
                                        }
                                    }
                                }
                                if (core.webServer.requestBrowser == "") {
                                    //
                                    // blank browser, Blank-Browser-Bot
                                    //
                                    resultSessionContext.visit.name = "Blank-Browser-Bot";
                                    resultSessionContext.visit.bot = true;
                                    resultSessionContext.visitBadBot = false;
                                    resultSessionContext.visit.mobile = false;
                                } else {
                                    //
                                    // -- mobile detect
                                    switch (resultSessionContext.visitor.ForceBrowserMobile) {
                                        case 1:
                                            resultSessionContext.visit.mobile = true;
                                            break;
                                        case 2:
                                            resultSessionContext.visit.mobile = false;
                                            break;
                                        default:
                                            resultSessionContext.visit.mobile = isMobile(core.webServer.requestBrowser);
                                            break;
                                    }
                                    //
                                    // -- bot and badBot detect
                                    resultSessionContext.visit.bot = false;
                                    resultSessionContext.visitBadBot = false;
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
                                        core.cache.storeObject("DefaultBotNameList", botFileContent, DateTime.Now.AddHours(1), new List<string>());
                                    }
                                    //
                                    if (!string.IsNullOrEmpty(botFileContent)) {
                                        botFileContent = GenericController.vbReplace(botFileContent, "\r\n", "\n");
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
                                                    string[] Args = GenericController.stringSplit(line, "\t");
                                                    if (Args.GetUpperBound(0) > 0) {
                                                        // -- process argument 1
                                                        if (!string.IsNullOrEmpty(Args[1].Trim(' '))) {
                                                            if (GenericController.vbInstr(1, core.webServer.requestBrowser, Args[1], 1) != 0) {
                                                                resultSessionContext.visit.name = Args[0];
                                                                //visitNameFound = True
                                                                break;
                                                            }
                                                        }
                                                        if (Args.GetUpperBound(0) > 1) {
                                                            // -- process argument 2
                                                            if (!string.IsNullOrEmpty(Args[2].Trim(' '))) {
                                                                if (GenericController.vbInstr(1, core.webServer.requestRemoteIP, Args[2], 1) != 0) {
                                                                    resultSessionContext.visit.name = Args[0];
                                                                    //visitNameFound = True
                                                                    break;
                                                                }
                                                            }
                                                            if (Args.GetUpperBound(0) <= 2) {
                                                                resultSessionContext.visit.bot = true;
                                                                resultSessionContext.visitBadBot = false;
                                                            } else {
                                                                resultSessionContext.visitBadBot = (Args[3].ToLowerInvariant() == "b");
                                                                resultSessionContext.visit.bot = resultSessionContext.visitBadBot || (Args[3].ToLowerInvariant() == "r");
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
                                    core.webServer.addResponseCookie(appNameCookiePrefix + main_cookieNameVisitor, SecurityController.encodeToken(core, resultSessionContext.visitor.id, resultSessionContext.visit.startTime), resultSessionContext.visit.startTime.AddYears(1), "", appRootPath, false);
                                }
                                //
                                // -- OnNewVisit Add-on call
                                AllowOnNewVisitEvent = true;
                            }
                            resultSessionContext.visit.lastVisitTime = core.doc.profileStartTime;
                            //
                            // -- verify visitor
                            if (resultSessionContext.visitor.id == 0) {
                                //
                                // -- create new visitor
                                resultSessionContext.visitor = VisitorModel.addEmpty(core);
                                visitor_changes = true;
                                //
                                resultSessionContext.visit.visitorNew = true;
                                resultSessionContext.visit.visitorID = resultSessionContext.visitor.id;
                                visit_changes = true;
                            }
                            //
                            // -- Attempt Link-in recognize or login
                            if (linkToken.id != 0) {
                                //
                                // -- Link Login
                                LogController.logTrace(core, "SessionController.create(), attempt link Login, linkToken.id [" + linkToken.id + "]");
                                if (authenticateById(core, linkToken.id, resultSessionContext)) {
                                    LogController.addSiteActivity(core, "link login with eid " + memberLinkinEID, resultSessionContext.user.id, resultSessionContext.user.organizationID);
                                }
                            } else if (memberLinkRecognizeID != 0) {
                                //
                                // -- Link Recognize
                                LogController.logTrace(core, "SessionController.create(), attempt link Recognize, memberLinkRecognizeID [" + memberLinkRecognizeID + "]");
                                if (recognizeById(core, memberLinkRecognizeID, ref resultSessionContext)) {
                                    LogController.addSiteActivity(core, "Successful link recognize with eid " + memberLinkinEID, resultSessionContext.user.id, resultSessionContext.user.organizationID);
                                } else {
                                    LogController.addSiteActivity(core, "Unsuccessful link recognize with eid " + memberLinkinEID, resultSessionContext.user.id, resultSessionContext.user.organizationID);
                                }
                            }
                            //
                            // -- create guest identity if no identity
                            if (resultSessionContext.user.id < 1) {
                                //
                                // if a user record has not been created, do not automatically create it.
                                // lazy create a user if/when it is needed
                                string DefaultMemberName = resultSessionContext.visit.name;
                                if (DefaultMemberName.Left(5).ToLowerInvariant() == "visit") {
                                    DefaultMemberName = "Guest";
                                }
                                resultSessionContext.user = new PersonModel {
                                    name = DefaultMemberName
                                };
                                //user_changes = false;
                                if (!resultSessionContext.visitor.MemberID.Equals(0)) {
                                    resultSessionContext.visitor.MemberID = 0;
                                    visitor_changes = true;
                                }
                                if (!resultSessionContext.visit.memberID.Equals(0)) {
                                    resultSessionContext.visit.memberID = 0;
                                    resultSessionContext.visit.visitAuthenticated = false;
                                    visit_changes = true;
                                }
                                if (resultSessionContext.visit.visitAuthenticated) {
                                    resultSessionContext.visit.visitAuthenticated = false;
                                    visit_changes = true;
                                }
                            }
                            //
                            // -- check for changes in interrelationships
                            if (resultSessionContext.user.id>0) {
                                if (resultSessionContext.visitor.MemberID != resultSessionContext.user.id) {
                                    resultSessionContext.visitor.MemberID = resultSessionContext.user.id;
                                    visitor_changes = true;
                                }
                                if (resultSessionContext.visit.memberID != resultSessionContext.user.id) {
                                    resultSessionContext.visit.memberID = resultSessionContext.user.id;
                                    visit_changes = true;
                                }
                                if (resultSessionContext.visit.visitorID != resultSessionContext.visitor.id) {
                                    resultSessionContext.visit.visitorID = resultSessionContext.visitor.id;
                                    visit_changes = true;
                                }
                            }
                            //
                            // -- count the page hit
                            LogController.logTrace(core, "SessionController.create(), attempt visit count update");
                            resultSessionContext.visit.excludeFromAnalytics |= resultSessionContext.visit.bot || resultSessionContext.user.excludeFromAnalytics || resultSessionContext.user.admin || resultSessionContext.user.developer;
                            if (!core.webServer.pageExcludeFromAnalytics) {
                                resultSessionContext.visit.pageVisits += 1;
                                visit_changes = true;
                            }
                            //
                            // -- Save anything that changed
                            LogController.logTrace(core, "SessionController.create(), save visit,visitor,user if updated");
                            if (visit_changes) {
                                resultSessionContext.visit.save(core,true);
                            }
                            if (visitor_changes) {
                                resultSessionContext.visitor.save(core, true);
                            }
                            if (user_changes) {
                                resultSessionContext.user.save(core, true);
                            }
                            string visitCookieNew = SecurityController.encodeToken(core, resultSessionContext.visit.id, resultSessionContext.visit.lastVisitTime);
                            if (trackVisits && (visitCookie != visitCookieNew)) {
                                visitCookie = visitCookieNew;
                            }
                        }
                        if (AllowOnNewVisitEvent) {
                            LogController.logTrace(core, "SessionController.create(), execute onNewVisitEvent");
                            foreach ( var addon in core.addonCache.getOnNewVisitAddonList()) {
                                CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                                    addonType = CPUtilsBaseClass.addonContext.ContextOnNewVisit,
                                    errorContextMessage = "new visit event running addon  [" + addon.name + "]"
                                };
                                core.addon.execute(addon, executeContext);
                            }
                        }
                        //
                        // -- Write Visit Cookie
                        LogController.logTrace(core, "SessionController.create(), write visit cookie");
                        visitCookie = SecurityController.encodeToken( core,resultSessionContext.visit.id, core.doc.profileStartTime);
                        // -- very trial-error fix - W4S site does not send cookies from ajax calls right after changing from requestAppRootPath to appRootPath
                        core.webServer.addResponseCookie(appNameCookiePrefix + Constants.cookieNameVisit, visitCookie, default(DateTime), "", @"/", false);
                        //core.webServer.addResponseCookie(appNameCookiePrefix + constants.cookieNameVisit, visitCookie, default(DateTime), "", appRootPath, false);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            } finally {
                //
                LogController.logTrace(core, "SessionController.create(), finally");
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
        public bool isAuthenticatedAdmin(CoreController core) {
            bool result = false;
            try {
                result = visit.visitAuthenticated & (user.admin || user.developer);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public bool isAuthenticatedDeveloper(CoreController core) {
            bool result = false;
            try {
                result = visit.visitAuthenticated & (user.admin || user.developer);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// true if the user is authenticated and has content editing rights to the content provided.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        public bool isAuthenticatedContentManager(CoreController core, MetaModel cdef) {
            bool returnIsContentManager = false;
            try {
                if (core.session.isAuthenticatedAdmin(core)) { return true; }
                if (!isAuthenticated) { return false; }
                //
                // -- for specific Content
                returnIsContentManager = PermissionController.getUserContentPermissions(core, cdef).allowEdit;
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                return false;
            }
            return returnIsContentManager;
        }
        //
        //========================================================================
        /// <summary>
        /// true if the user is authenticated and has content editing rights to the content provided. If the content is blank, user must be admin or developer.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        public bool isAuthenticatedContentManager(CoreController core, string ContentName) {
            bool returnIsContentManager = false;
            try {
                if (core.session.isAuthenticatedAdmin(core)) { return true; }
                if (!isAuthenticated) { return false; }
                //
                if (string.IsNullOrEmpty(ContentName)) {
                    //
                    // -- for anything
                    return isAuthenticatedContentManager(core);
                } else {
                    //
                    // -- for specific Content
                    MetaModel cdef = MetaModel.createByUniqueName(core, ContentName);
                    returnIsContentManager = PermissionController.getUserContentPermissions(core, cdef).allowEdit;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                return false;
            }
            return returnIsContentManager;
        }
        //
        //========================================================================
        /// <summary>
        /// true if the user is authenticated and has content editing rights to the content provided. If the content is blank, user must be admin or developer.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        public bool isAuthenticatedContentManager(CoreController core) {
            bool returnIsContentManager = false;
            try {
                if (core.session.isAuthenticatedAdmin(core)) { return true; }
                if (!isAuthenticated) { return false; }
                //
                // Is a CM for any content def
                if ((!_isAuthenticatedContentManagerAnything_loaded) || (_isAuthenticatedContentManagerAnything_userId != user.id)) {
                    using (var csData = new CsModel(core)) {
                        string sql = "SELECT ccGroupRules.ContentID"
                            + " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID"
                            + " WHERE ("
                                + "(ccMemberRules.MemberID=" + DbController.encodeSQLNumber(user.id) + ")"
                                + " AND(ccMemberRules.active<>0)"
                                + " AND(ccGroupRules.active<>0)"
                                + " AND(ccGroupRules.ContentID Is not Null)"
                                + " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" + DbController.encodeSQLDate(core.doc.profileStartTime) + "))"
                                + ")";
                        _isAuthenticatedContentManagerAnything = csData.openSql(sql);
                    }
                    //
                    _isAuthenticatedContentManagerAnything_userId = user.id;
                    _isAuthenticatedContentManagerAnything_loaded = true;
                }
                returnIsContentManager = _isAuthenticatedContentManagerAnything;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
        public void logout(CoreController core) {
            try {
                LogController.addSiteActivity(core, "logout", user.id, user.organizationID);
                //
                // new guest
                user = PersonModel.addDefault(core, MetaModel.createByUniqueName( core, PersonModel.contentName));
                visit.memberID = user.id;
                visit.visitAuthenticated = false;
                visit.save(core);
                visitor.MemberID = user.id;
                visitor.save(core);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public int getUserIdForCredentials(CoreController core, string username, string password) {
            int returnUserId = 0;
            try {
                //
                const string badLoginUserError = "Your login was not successful. Please try again.";
                //
                string SQL = null;
                bool recordIsAdmin = false;
                bool recordIsDeveloper = false;
                string Criteria = null;
                string iPassword = null;
                bool allowEmailLogin = false;
                bool allowNoPasswordLogin = false;
                string iLoginFieldValue;
                //
                iLoginFieldValue = GenericController.encodeText(username);
                iPassword = GenericController.encodeText(password);
                //
                returnUserId = 0;
                allowEmailLogin = core.siteProperties.getBoolean("allowEmailLogin");
                allowNoPasswordLogin = core.siteProperties.getBoolean("allowNoPasswordLogin");
                if (string.IsNullOrEmpty(iLoginFieldValue)) {
                    //
                    // ----- loginFieldValue blank, stop here
                    //
                    if (allowEmailLogin) {
                        ErrorController.addUserError(core, "A valid login requires a non-blank username or email.");
                    } else {
                        ErrorController.addUserError(core, "A valid login requires a non-blank username.");
                    }
                } else if ((!allowNoPasswordLogin) && (string.IsNullOrEmpty(iPassword))) {
                    //
                    // ----- password blank, stop here
                    //
                    ErrorController.addUserError(core, "A valid login requires a non-blank password.");
                } else if (visit.loginAttempts >= core.siteProperties.maxVisitLoginAttempts) {
                    //
                    // ----- already tried 5 times
                    //
                    ErrorController.addUserError(core, badLoginUserError);
                } else {
                    if (allowEmailLogin) {
                        //
                        // login by username or email
                        //
                        Criteria = "((username=" + DbController.encodeSQLText(iLoginFieldValue) + ")or(email=" + DbController.encodeSQLText(iLoginFieldValue) + "))";
                    } else {
                        //
                        // login by username only
                        //
                        Criteria = "(username=" + DbController.encodeSQLText(iLoginFieldValue) + ")";
                    }
                    Criteria = Criteria + "and((dateExpires is null)or(dateExpires>" + DbController.encodeSQLDate(DateTime.Now) + "))";
                    using (var csData = new CsModel(core)) {
                        csData.open("People", Criteria, "id",true, user.id, "ID,password,admin,developer", PageSize: 2);
                        if (!csData.ok()) {
                            //
                            // ----- loginFieldValue not found, stop here
                            //
                            ErrorController.addUserError(core, badLoginUserError);
                        } else if ((!GenericController.encodeBoolean(core.siteProperties.getBoolean("AllowDuplicateUsernames", false))) && (csData.getRowCount() > 1)) {
                            //
                            // ----- AllowDuplicates is false, and there are more then one record
                            //
                            ErrorController.addUserError(core, "This user account can not be used because the username is not unique on this website. Please contact the site administrator.");
                        } else {
                            //
                            // ----- search all found records for the correct password
                            //
                            while (csData.ok()) {
                                returnUserId = 0;
                                //
                                // main_Get Id if password good
                                //
                                if (string.IsNullOrEmpty(iPassword)) {
                                    //
                                    // no-password-login -- allowNoPassword + no password given + account has no password + account not admin/dev/cm
                                    //
                                    recordIsAdmin = csData.getBoolean("admin");
                                    recordIsDeveloper = !csData.getBoolean("admin");
                                    if (allowNoPasswordLogin && (csData.getText("password") == "") && (!recordIsAdmin) && (recordIsDeveloper)) {
                                        returnUserId = csData.getInteger("ID");
                                        //
                                        // verify they are in no content manager groups
                                        //
                                        using (var csRules = new CsModel(core)) {
                                            SQL = "SELECT ccGroupRules.ContentID"
                                                + " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID"
                                                + " WHERE ("
                                                + "(ccMemberRules.MemberID=" + DbController.encodeSQLNumber(returnUserId) + ")"
                                                + " AND(ccMemberRules.active<>0)"
                                                + " AND(ccGroupRules.active<>0)"
                                                + " AND(ccGroupRules.ContentID Is not Null)"
                                                + " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" + DbController.encodeSQLDate(core.doc.profileStartTime) + "))"
                                                + ");";
                                            if (csRules.openSql(SQL)) { returnUserId = 0; }
                                        }
                                    }
                                } else {
                                    //
                                    // password login
                                    //
                                    if (GenericController.vbLCase(csData.getText("password")) == GenericController.vbLCase(iPassword)) {
                                        returnUserId = csData.getInteger("ID");
                                    }
                                }
                                if (returnUserId != 0) {
                                    break;
                                }
                                csData.goNext();
                            }
                            if (returnUserId == 0) {
                                ErrorController.addUserError(core, badLoginUserError);
                            }
                        }
                        csData.close();
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public bool isNewCredentialOK(CoreController core, string Username, string Password, ref string returnErrorMessage, ref int returnErrorCode) {
            bool returnOk = false;
            try {
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
                    using (var csData = new CsModel(core)) {
                        if (csData.open("People", "username=" + DbController.encodeSQLText(Username), "id", false, 2, "ID")) {
                            //
                            // ----- username was found, stop here
                            returnErrorCode = 3;
                            returnErrorMessage = "The username you supplied is currently in use.";
                        } else {
                            returnOk = true;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnOk;
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
        public bool authenticate(CoreController core, string username, string password, bool AllowAutoLogin = false) {
            bool result = false;
            try {
                int userId = getUserIdForCredentials(core, username, password);
                if (userId != 0) {
                    result = authenticateById(core, userId, this);
                    if (result) {
                        LogController.addSiteActivity(core, "successful password login, username [" + username + "]", user.id, user.organizationID);
                    } else {
                        LogController.addSiteActivity(core, "unsuccessful password login, username [" + username + "]", user.id, user.organizationID);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public static bool authenticateById(CoreController core, int userId, SessionController authContext) {
            bool result = false;
            try {
                result = recognizeById(core, userId, ref authContext);
                if (result) {
                    //
                    // Log them in
                    authContext.visit.visitAuthenticated = true;
                    if (authContext.visit.startTime == DateTime.MinValue) {
                        authContext.visit.startTime = core.doc.profileStartTime;
                    }
                    authContext.visit.save(core);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public static bool recognizeById(CoreController core, int userId, ref SessionController sessionContext) {
            bool result = false;
            try {
                if (sessionContext.visitor.id == 0) {
                    sessionContext.visitor = VisitorModel.addEmpty(core);
                }
                if (sessionContext.visit.id == 0) {
                    sessionContext.visit = VisitModel.addEmpty(core);
                }
                PersonModel contextUser = PersonModel.create(core, userId);
                if (contextUser != null) {
                    sessionContext.user = contextUser;
                    sessionContext.visitor.MemberID = sessionContext.user.id;
                    sessionContext.visit.memberID = sessionContext.user.id;
                    sessionContext.visit.visitAuthenticated = false;
                    sessionContext.visit.visitorID = sessionContext.visitor.id;
                    sessionContext.visit.loginAttempts = 0;
                    sessionContext.user.visits = sessionContext.user.visits + 1;
                    if (sessionContext.user.visits == 1) {
                        sessionContext.visit.memberNew = true;
                    } else {
                        sessionContext.visit.memberNew = false;
                    }
                    sessionContext.user.LastVisit = core.doc.profileStartTime;
                    sessionContext.visit.excludeFromAnalytics = sessionContext.visit.excludeFromAnalytics || sessionContext.visit.bot || sessionContext.user.excludeFromAnalytics || sessionContext.user.admin || sessionContext.user.developer;
                    sessionContext.visit.save(core);
                    sessionContext.visitor.save(core);
                    sessionContext.user.save(core);
                    result = true;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public bool isAuthenticatedMember(CoreController core) {
            return visit.visitAuthenticated & (MetaController.isWithinContent(core, user.contentControlID, MetaModel.getContentId(core, "members")));
        }
        //
        //========================================================================
        /// <summary>
        /// is Guest
        /// </summary>
        /// <returns></returns>
        public bool isGuest(CoreController core) {
            return !isRecognized(core);
        }
        //
        //========================================================================
        /// <summary>
        /// Is Recognized (not new and not authenticted)
        /// </summary>
        /// <returns></returns>
        public bool isRecognized(CoreController core) {
            return !visit.memberNew;
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
            bool result = false;
            try {
                if (isAuthenticated) {
                    string cacheTestName = contentNameOrId;
                    if (string.IsNullOrEmpty(cacheTestName)) {
                        cacheTestName = "iseditingall";
                    }
                    cacheTestName = cacheTestName.ToLowerInvariant();
                    if (core.doc.contentIsEditingList.Contains(cacheTestName)) {
                        //
                        // -- 
                        return true;
                    } else if (core.doc.contentNotEditingList.Contains(cacheTestName)) {
                        //
                        // -- 
                        return false;
                    } else {
                        if (core.visitProperty.getBoolean("AllowEditing") || core.visitProperty.getBoolean("AllowAdvancedEditor")) {
                            if (!string.IsNullOrEmpty(contentNameOrId)) {
                                if (contentNameOrId.IsNumeric()) {
                                    contentNameOrId = MetaController.getContentNameByID(core, encodeInteger(contentNameOrId));
                                }
                            }
                            result = isAuthenticatedContentManager(core, contentNameOrId);
                        }
                        if (result) {
                            core.doc.contentIsEditingList.Add(cacheTestName);
                        } else {
                            core.doc.contentNotEditingList.Add(cacheTestName);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// true if editing with the quick editor
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        public bool isQuickEditing(CoreController core, string ContentName) {
            bool returnResult = false;
            try {
                if (isAuthenticatedContentManager(core, ContentName)) {
                    returnResult = core.visitProperty.getBoolean("AllowQuickEditor");
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public bool isAdvancedEditing(CoreController core, string ContentName) {
            bool returnResult = false;
            try {
                if (isAuthenticatedContentManager(core, ContentName)) {
                    returnResult = core.visitProperty.getBoolean("AllowAdvancedEditor");
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public bool isLoginOK(CoreController core, string Username, string Password, string ErrorMessage = "", int ErrorCode = 0) {
            bool result = (getUserIdForCredentials(core, Username, Password) != 0);
            if (!result) {
                ErrorMessage = ErrorController.getUserError(core);
            }
            return result;
        }
        //
        // ================================================================================================
        //
        public string getAuthoringStatusMessage(CoreController core, bool IsContentWorkflowAuthoring, bool RecordEditLocked, string main_EditLockName, DateTime main_EditLockExpires, bool RecordApproved, string ApprovedBy, bool RecordSubmitted, string SubmittedBy, bool RecordDeleted, bool RecordInserted, bool RecordModified, string ModifiedBy) {
            string result = "";
            //
            string Copy = null;
            string Delimiter = "";
            int main_EditLockExpiresMinutes = encodeInteger((main_EditLockExpires - core.doc.profileStartTime).TotalMinutes);
            //
            // ----- site does not support workflow authoring
            //
            if (RecordEditLocked) {
                Copy = GenericController.vbReplace(Msg_EditLock, "<EDITNAME>", main_EditLockName);
                Copy = GenericController.vbReplace(Copy, "<EDITEXPIRES>", main_EditLockExpires.ToString());
                Copy = GenericController.vbReplace(Copy, "<EDITEXPIRESMINUTES>", GenericController.encodeText(main_EditLockExpiresMinutes));
                result += Delimiter + Copy;
                Delimiter = "<br>";
            }
            result += Delimiter + Msg_WorkflowDisabled;
            Delimiter = "<br>";
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// True if the current visitor is a content manager in workflow rendering mode
        /// </summary>
        /// <returns></returns>
        public bool isWorkflowRendering() {
            bool tempisWorkflowRendering = false;
            bool result = false;
            try {
                if (isAuthenticatedContentManager(core)) {
                    tempisWorkflowRendering = core.visitProperty.getBoolean("AllowWorkflowRendering");
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
    }
}