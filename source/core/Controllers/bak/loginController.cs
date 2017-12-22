

using Controllers;

using System.Xml;
using Contensive.Core;
using Models.Entity;
using Contensive.BaseClasses;
// 

namespace Controllers {
    
    // 
    public class loginController {
        
        // 
        // ========================================================================
        // '' <summary>
        // '' A complete html page with the login form in the middle
        // '' </summary>
        // '' <param name="forceDefaultLogin"></param>
        // '' <returns></returns>
        public static string getLoginPage(coreClass cpcore, bool forceDefaultLogin) {
            string returnREsult = "";
            try {
                string Body;
                // Dim head As String
                // Dim bodyTag As String
                // 
                //  ----- Default Login
                // 
                if (forceDefaultLogin) {
                    Body = loginController.getLoginForm_Default(cpcore);
                }
                else {
                    Body = loginController.getLoginForm(cpcore);
                }
                
                Body = ("" 
                            + (cr + ("<p class=\"ccAdminNormal\">You are attempting to enter an access controlled area. Continue only if you " +
                            "have authority to enter this area. Information about your visit will be recorded for security purpos" +
                            "es.</p>" 
                            + (Body + ""))));
                Body = ("" 
                            + (cpcore.html.main_GetPanel(Body, "ccPanel", "ccPanelHilite", "ccPanelShadow", "400", 15) 
                            + (cr + ("<p> </p>" 
                            + (cr + ("<p> </p>" 
                            + (cr + ("<p style=\"text-align:center\"><a href=\"http://www.Contensive.com\" target=\"_blank\"><img src=\"/ccLib/ima" +
                            "ges/ccLibLogin.GIF\" width=\"80\" height=\"33\" border=\"0\" alt=\"Contensive Content Control\" ></A></p>" 
                            + (cr + ("<p style=\"text-align:center\" class=\"ccAdminSmall\">The content on this web site is managed and deliver" +
                            "ed by the Contensive Site Management Server. If you do not have member access, please use your back " +
                            "button to return to the public area.</p>" + ""))))))))));
                Body = ("" 
                            + (cr + ("<div class=\"ccCon\" style=\"width:400px;margin:100px auto 0 auto;\">" 
                            + (htmlIndent(cpcore.html.main_GetPanelHeader("Login")) 
                            + (htmlIndent(Body) + "</div>")))));
                returnREsult = Body;
                // Call cpcore.doc.setMetaContent(0, 0)
                // Call cpcore.html.addTitle("Login", "loginPage")
                // bodyTag = TemplateDefaultBodyTag
                // returnREsult = cpcore.html.getHtmlDoc(Body, bodyTag, True, True, False)
            }
            catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            
            return returnREsult;
        }
        
        // 
        // ========================================================================
        //    default login form
        // ========================================================================
        // 
        public static string getLoginForm_Default(coreClass cpcore) {
            string returnHtml = "";
            try {
                string Panel;
                string usernameMsg;
                string QueryString;
                string loginForm;
                string Caption;
                string formType;
                bool needLoginForm;
                // 
                //  ----- process the previous form, if login OK, return blank (signal for page refresh)
                // 
                needLoginForm = true;
                formType = cpcore.docProperties.getText("type");
                if ((formType == FormTypeLogin)) {
                    if (loginController.processFormLoginDefault(cpcore)) {
                        returnHtml = "";
                        needLoginForm = false;
                    }
                    
                }
                
                if (needLoginForm) {
                    // 
                    //  ----- When page loads, set focus on login username
                    // 
                    cpcore.doc.addRefreshQueryString("method", "");
                    loginForm = "";
                    cpcore.html.addScriptCode_onLoad("document.getElementById(\'LoginUsernameInput\').focus()", "login");
                    // 
                    //  ----- Error Messages
                    // 
                    if (genericController.EncodeBoolean(cpcore.siteProperties.getBoolean("allowEmailLogin", false))) {
                        usernameMsg = "<b>To login, enter your username or email address with your password.</b></p>";
                    }
                    else {
                        usernameMsg = "<b>To login, enter your username and password.</b></p>";
                    }
                    
                    // 
                    QueryString = cpcore.webServer.requestQueryString;
                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", false);
                    QueryString = genericController.ModifyQueryString(QueryString, "requestbinary", "", false);
                    // 
                    //  ----- Username
                    // 
                    if (genericController.EncodeBoolean(cpcore.siteProperties.getBoolean("allowEmailLogin", false))) {
                        Caption = "Username or Email";
                    }
                    else {
                        Caption = "Username";
                    }
                    
                    // 
                    loginForm = (loginForm 
                                + (cr + ("<tr>" 
                                + (cr2 + ("<td style=\"text-align:right;vertical-align:middle;width:30%;padding:4px\" align=\"right\" width=\"30%\">" 
                                + (SpanClassAdminNormal 
                                + (Caption + (" </span></td>" 
                                + (cr2 + ("<td style=\"text-align:left;vertical-align:middle;width:70%;padding:4px\" align=\"left\"  width=\"70%\"><in" +
                                "put ID=\"LoginUsernameInput\" NAME=\"" + ("username\" VALUE=\"\" SIZE=\"20\" MAXLENGTH=\"50\" ></td>" 
                                + (cr + "</tr>"))))))))))));
                    if (genericController.EncodeBoolean(cpcore.siteProperties.getBoolean("allowNoPasswordLogin", false))) {
                        Caption = "Password (optional)";
                    }
                    else {
                        Caption = "Password";
                    }
                    
                    loginForm = (loginForm 
                                + (cr + ("<tr>" 
                                + (cr2 + ("<td style=\"text-align:right;vertical-align:middle;width:30%;padding:4px\" align=\"right\">" 
                                + (SpanClassAdminNormal 
                                + (Caption + (" </span></td>" 
                                + (cr2 + ("<td style=\"text-align:left;vertical-align:middle;width:70%;padding:4px\" align=\"left\" ><input NAME=\"" + ("password\" VALUE=\"\" SIZE=\"20\" MAXLENGTH=\"50\" type=\"password\"></td>" 
                                + (cr + ("</tr>" + "")))))))))))));
                    if (genericController.EncodeBoolean(cpcore.siteProperties.getBoolean("AllowAutoLogin", false))) {
                        loginForm = (loginForm 
                                    + (cr + ("<tr>" 
                                    + (cr2 + ("<td align=\"right\"> </td>" 
                                    + (cr2 + ("<td align=\"left\" >" 
                                    + (cr3 + ("<table border=\"0\" cellpadding=\"5\" cellspacing=\"0\" width=\"100%\">" 
                                    + (cr4 + ("<tr>" 
                                    + (cr5 + ("<td valign=\"top\" width=\"20\"><input type=\"checkbox\" name=\"" + ("autologin\" value=\"ON\" checked></td>" 
                                    + (cr5 + ("<td valign=\"top\" width=\"100%\">" 
                                    + (SpanClassAdminNormal + ("Login automatically from this computer</span></td>" 
                                    + (cr4 + ("</tr>" 
                                    + (cr3 + ("</table>" 
                                    + (cr2 + ("</td>" 
                                    + (cr + "</tr>")))))))))))))))))))))))));
                    }
                    
                    loginForm = (loginForm 
                                + (cr + ("<tr>" 
                                + (cr2 + ("<td colspan=\"2\"> </td>" 
                                + (cr + ("</tr>" + "")))))));
                    loginForm = ("" 
                                + (cr + ("<table border=\"0\" cellpadding=\"5\" cellspacing=\"0\" width=\"100%\">" 
                                + (htmlIndent(loginForm) 
                                + (cr + ("</table>" + ""))))));
                    loginForm = (loginForm 
                                + (cpcore.html.html_GetFormInputHidden("Type", FormTypeLogin) 
                                + (cpcore.html.html_GetFormInputHidden("email", cpCore.doc.authContext.user.Email) 
                                + (cpcore.html.main_GetPanelButtons(ButtonLogin, "Button") + ""))));
                    loginForm = ("" 
                                + (cpcore.html.html_GetFormStart(QueryString) 
                                + (htmlIndent(loginForm) 
                                + (cr + ("</form>" + "")))));
                    Panel = ("" 
                                + (errorController.error_GetUserError(cpcore) 
                                + (cr + ("<p class=\"ccAdminNormal\">" 
                                + (usernameMsg 
                                + (loginForm + ""))))));
                    if (genericController.EncodeBoolean(cpcore.siteProperties.getBoolean("allowPasswordEmail", true))) {
                        Panel = ("" 
                                    + (Panel 
                                    + (cr + ("<p class=\"ccAdminNormal\"><b>Forget your password?</b></p>" 
                                    + (cr + ("<p class=\"ccAdminNormal\">If you are a member of the system and can not remember your password, enter " +
                                    "your email address below and we will email your matching username and password.</p>" 
                                    + (this.getSendPasswordForm(cpcore) + "")))))));
                    }
                    
                    // 
                    returnHtml = ("" 
                                + (cr + ("<div class=\"ccLoginFormCon\">" 
                                + (htmlIndent(Panel) 
                                + (cr + ("</div>" + ""))))));
                }
                
            }
            catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            
            return returnHtml;
        }
        
        // 
        // =============================================================================
        // '' <summary>
        // '' A login form that can be added to any page. This is just form with no surrounding border, etc. 
        // '' </summary>
        // '' <returns></returns>
        public static string getLoginForm(coreClass cpcore, bool forceDefaultLoginForm, void =, void False) {
            string returnHtml = "";
            // Warning!!! Optional parameters not supported
            try {
                int loginAddonID = 0;
                if (!forceDefaultLoginForm) {
                    loginAddonID = cpcore.siteProperties.getinteger("Login Page AddonID");
                    if ((loginAddonID != 0)) {
                        // 
                        //  -- Custom Login
                        Models.Entity.addonModel addon = Models.Entity.addonModel.create(cpcore, loginAddonID);
                        CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext();
                        // With...
                        addonType = CPUtilsBaseClass.addonContext.ContextPage;
                        returnHtml = cpcore.addon.execute(addon, executeContext);
                        // returnHtml = cpcore.addon.execute_legacy2(loginAddonID, "", "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, 0, "", False, Nothing)
                        if (string.IsNullOrEmpty(returnHtml)) {
                            // 
                            //  -- login successful, redirect back to this page (without a method)
                            string QS = cpcore.doc.refreshQueryString;
                            QS = genericController.ModifyQueryString(QS, "method", "");
                            QS = genericController.ModifyQueryString(QS, "RequestBinary", "");
                            // 
                            return cpcore.webServer.redirect(("?" + QS), "Login form success");
                        }
                        
                    }
                    
                    if ((loginAddonID == 0)) {
                        // 
                        //  ----- When page loads, set focus on login username
                        // 
                        returnHtml = loginController.getLoginForm_Default(cpcore);
                    }
                    
                    ((Exception)(ex));
                    cpcore.handleException(ex);
                    throw;
                }
                
                return returnHtml;
            }
            
            // 
            // =============================================================================
            // '' <summary>
            // '' a simple email password form
            // '' </summary>
            // '' <returns></returns>
        }
        
        string getSendPasswordForm(coreClass cpcore) {
            string returnResult = "";
            try {
                string QueryString;
                // 
                if (cpcore.siteProperties.getBoolean("allowPasswordEmail", true)) {
                    returnResult = ("" 
                                + (cr + ("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">" 
                                + (cr2 + ("<tr>" 
                                + (cr3 + ("<td style=\"text-align:right;vertical-align:middle;width:30%;padding:4px\" align=\"right\" width=\"30%\">" 
                                + (SpanClassAdminNormal + ("Email</span></td>" 
                                + (cr3 + ("<td style=\"text-align:left;vertical-align:middle;width:70%;padding:4px\" align=\"left\"  width=\"70%\"><in" +
                                "put NAME=\"" + ("email\" VALUE=\"" 
                                + (genericController.encodeHTML(cpCore.doc.authContext.user.Email) + ("\" SIZE=\"20\" MAXLENGTH=\"50\"></td>" 
                                + (cr2 + ("</tr>" 
                                + (cr2 + ("<tr>" 
                                + (cr3 + ("<td colspan=\"2\"> </td>" 
                                + (cr2 + ("</tr>" 
                                + (cr2 + ("<tr>" 
                                + (cr3 + ("<td colspan=\"2\">" 
                                + (htmlIndent(htmlIndent(cpcore.html.main_GetPanelButtons(ButtonSendPassword, "Button"))) 
                                + (cr3 + ("</td>" 
                                + (cr2 + ("</tr>" 
                                + (cr + ("</table>" + "")))))))))))))))))))))))))))))))));
                    returnResult = ("" 
                                + (returnResult 
                                + (cpcore.html.html_GetFormInputHidden("Type", FormTypeSendPassword) + "")));
                    foreach (string key in cpcore.docProperties.getKeyList) {
                        // With...
                        if (cpcore.docProperties.getProperty(key).IsForm) {
                            switch (genericController.vbUCase(cpcore.docProperties.getProperty(key).Name)) {
                                case "S":
                                case "MA":
                                case "MB":
                                case "USERNAME":
                                case "PASSWORD":
                                case "EMAIL":
                                    break;
                                default:
                                    returnResult = (returnResult + cpcore.html.html_GetFormInputHidden(cpcore.docProperties.getProperty(key).Name, cpcore.docProperties.getProperty(key).Value));
                                    break;
                            }
                        }
                        
                    }
                    
                    // 
                    QueryString = cpcore.doc.refreshQueryString;
                    QueryString = genericController.ModifyQueryString(QueryString, "S", "");
                    QueryString = genericController.ModifyQueryString(QueryString, "ccIPage", "");
                    returnResult = ("" 
                                + (cpcore.html.html_GetFormStart(QueryString) 
                                + (htmlIndent(returnResult) 
                                + (cr + ("</form>" + "")))));
                }
                
            }
            catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            
            return returnResult;
        }
        
        // 
        // ========================================================================
        //  ----- Process the login form
        // ========================================================================
        // 
        public static bool processFormLoginDefault(coreClass cpcore) {
            bool returnREsult = false;
            try {
                int LocalMemberID;
                string loginForm_Username = "";
                string loginForm_Password = "";
                string loginForm_Email = "";
                bool loginForm_AutoLogin = false;
                returnREsult = false;
                if (true) {
                    // 
                    //  Processing can happen
                    //    1) early in init() -- legacy
                    //    2) as well as at the front of main_GetLoginForm - to support addon Login forms
                    //  This flag prevents the default form from processing twice
                    // 
                    loginForm_Username = cpcore.docProperties.getText("username");
                    loginForm_Password = cpcore.docProperties.getText("password");
                    loginForm_AutoLogin = cpcore.docProperties.getBoolean("autologin");
                    // 
                    if (((cpCore.doc.authContext.visit.LoginAttempts < cpcore.siteProperties.maxVisitLoginAttempts) 
                                && cpCore.doc.authContext.visit.CookieSupport)) {
                        LocalMemberID = cpCore.doc.authContext.authenticateGetId(cpcore, loginForm_Username, loginForm_Password);
                        if ((LocalMemberID == 0)) {
                            cpCore.doc.authContext.visit.LoginAttempts = (cpCore.doc.authContext.visit.LoginAttempts + 1);
                            cpCore.doc.authContext.visit.saveObject(cpcore);
                        }
                        else {
                            returnREsult = cpCore.doc.authContext.authenticateById(cpcore, LocalMemberID, cpCore.doc.authContext);
                            if (returnREsult) {
                                logController.logActivity2(cpcore, "successful username/password login", cpCore.doc.authContext.user.id, cpCore.doc.authContext.user.OrganizationID);
                            }
                            else {
                                logController.logActivity2(cpcore, "bad username/password login", cpCore.doc.authContext.user.id, cpCore.doc.authContext.user.OrganizationID);
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            
            return returnREsult;
        }
        
        // 
        // ========================================================================
        //  ----- Process the send password form
        // ========================================================================
        // 
        public static void processFormSendPassword(coreClass cpcore) {
            try {
                cpcore.email.sendPassword(cpcore.docProperties.getText("email"));
            }
            catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // ========================================================================
        //  ----- Process the send password form
        // ========================================================================
        // 
        public static void processFormJoin(coreClass cpcore) {
            try {
                string ErrorMessage = "";
                int CS;
                string FirstName;
                string LastName;
                string FullName;
                string Email;
                int errorCode = 0;
                // 
                string loginForm_Username = "";
                string loginForm_Password = "";
                string loginForm_Email = "";
                bool loginForm_AutoLogin = false;
                loginForm_Username = cpcore.docProperties.getText("username");
                loginForm_Password = cpcore.docProperties.getText("password");
                // 
                if (!genericController.EncodeBoolean(cpcore.siteProperties.getBoolean("AllowMemberJoin", false))) {
                    errorController.error_AddUserError(cpcore, "This site does not accept public main_MemberShip.");
                }
                else if (!cpCore.doc.authContext.isNewLoginOK(cpcore, loginForm_Username, loginForm_Password, ErrorMessage, errorCode)) {
                    errorController.error_AddUserError(cpcore, ErrorMessage);
                }
                else if (!(cpCore.doc.debug_iUserError != "")) {
                    CS = cpcore.db.csOpen("people", ("ID=" + cpcore.db.encodeSQLNumber(cpCore.doc.authContext.user.id)));
                    if (!cpcore.db.csOk(CS)) {
                        cpcore.handleException(new Exception("Could not open the current members account to set the username and password."));
                    }
                    else {
                        if (((cpcore.db.csGetText(CS, "username") != "") 
                                    || ((cpcore.db.csGetText(CS, "password") != "") 
                                    || (cpcore.db.csGetBoolean(CS, "admin") || cpcore.db.csGetBoolean(CS, "developer"))))) {
                            // 
                            //  if the current account can be logged into, you can not join 'into' it
                            // 
                            cpCore.doc.authContext.logout(cpcore);
                        }
                        
                        FirstName = cpcore.docProperties.getText("firstname");
                        LastName = cpcore.docProperties.getText("firstname");
                        FullName = (FirstName + (" " + LastName));
                        Email = cpcore.docProperties.getText("email");
                        cpcore.db.csSet(CS, "FirstName", FirstName);
                        cpcore.db.csSet(CS, "LastName", LastName);
                        cpcore.db.csSet(CS, "Name", FullName);
                        cpcore.db.csSet(CS, "username", loginForm_Username);
                        cpcore.db.csSet(CS, "password", loginForm_Password);
                        cpCore.doc.authContext.authenticateById(cpcore, cpCore.doc.authContext.user.id, cpCore.doc.authContext);
                    }
                    
                    cpcore.db.csClose(CS);
                }
                
                cpcore.cache.invalidateAllObjectsInContent("People");
            }
            catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            
        }
    }
}