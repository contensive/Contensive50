
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
// todo -- should not be here
using Contensive.BaseClasses;
//
namespace Contensive.Core.Controllers {
    //
    public class loginController {
        //
        //========================================================================
        /// <summary>
        /// A complete html page with the login form in the middle
        /// </summary>
        /// <param name="forceDefaultLogin"></param>
        /// <returns></returns>
        public static string getLoginPage(coreClass cpcore, bool forceDefaultLogin) {
            string returnREsult = "";
            try {
                string Body = null;
                //Dim head As String
                //Dim bodyTag As String
                //
                // ----- Default Login
                //
                if (forceDefaultLogin) {
                    Body = getLoginForm_Default(cpcore);
                } else {
                    Body = getLoginForm(cpcore);
                }
                Body = ""
                    + "\r<p class=\"ccAdminNormal\">You are attempting to enter an access controlled area. Continue only if you have authority to enter this area. Information about your visit will be recorded for security purposes.</p>"
                    + Body + "";
                //
                Body = ""
                    + cpcore.html.getPanel(Body, "ccPanel", "ccPanelHilite", "ccPanelShadow", "400", 15) + "\r<p>&nbsp;</p>"
                    + "\r<p>&nbsp;</p>"
                    + "\r<p style=\"text-align:center\"><a href=\"http://www.Contensive.com\" target=\"_blank\"><img src=\"/ccLib/images/ccLibLogin.GIF\" width=\"80\" height=\"33\" border=\"0\" alt=\"Contensive Content Control\" ></A></p>"
                    + "\r<p style=\"text-align:center\" class=\"ccAdminSmall\">The content on this web site is managed and delivered by the Contensive Site Management Server. If you do not have member access, please use your back button to return to the public area.</p>"
                    + "";
                //
                // --- create an outer table to hold the form
                //
                Body = ""
                    + "\r<div class=\"ccCon\" style=\"width:400px;margin:100px auto 0 auto;\">"
                    + htmlIndent(cpcore.html.getPanelHeader("Login")) + htmlIndent(Body) + "</div>";
                //
                returnREsult = Body;
                //Call cpcore.doc.setMetaContent(0, 0)
                //Call cpcore.html.addTitle("Login", "loginPage")
                //bodyTag = TemplateDefaultBodyTag
                //returnREsult = cpcore.html.getHtmlDoc(Body, bodyTag, True, True, False)
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return returnREsult;
        }
        //
        //========================================================================
        //   default login form
        //========================================================================
        //
        public static string getLoginForm_Default(coreClass cpcore) {
            string returnHtml = "";
            try {
                string Panel = null;
                string usernameMsg = null;
                string QueryString = null;
                string loginForm = null;
                string Caption = null;
                string formType = null;
                bool needLoginForm;
                //
                // ----- process the previous form, if login OK, return blank (signal for page refresh)
                //
                needLoginForm = true;
                formType = cpcore.docProperties.getText("type");
                if (formType == FormTypeLogin) {
                    if (processFormLoginDefault(cpcore)) {
                        returnHtml = "";
                        needLoginForm = false;
                    }
                }
                if (needLoginForm) {
                    //
                    // ----- When page loads, set focus on login username
                    //
                    cpcore.doc.addRefreshQueryString("method", "");
                    loginForm = "";
                    cpcore.html.addScriptCode_onLoad("document.getElementById('LoginUsernameInput').focus()", "login");
                    //
                    // ----- Error Messages
                    //
                    if (genericController.encodeBoolean(cpcore.siteProperties.getBoolean("allowEmailLogin", false))) {
                        usernameMsg = "<b>To login, enter your username or email address with your password.</b></p>";
                    } else {
                        usernameMsg = "<b>To login, enter your username and password.</b></p>";
                    }
                    //
                    QueryString = cpcore.webServer.requestQueryString;
                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", false);
                    QueryString = genericController.ModifyQueryString(QueryString, "requestbinary", "", false);
                    //
                    // ----- Username
                    //
                    if (genericController.encodeBoolean(cpcore.siteProperties.getBoolean("allowEmailLogin", false))) {
                        Caption = "Username&nbsp;or&nbsp;Email";
                    } else {
                        Caption = "Username";
                    }
                    //
                    loginForm = loginForm + "\r<tr>"
                    + cr2 + "<td style=\"text-align:right;vertical-align:middle;width:30%;padding:4px\" align=\"right\" width=\"30%\">" + SpanClassAdminNormal + Caption + "&nbsp;</span></td>"
                    + cr2 + "<td style=\"text-align:left;vertical-align:middle;width:70%;padding:4px\" align=\"left\"  width=\"70%\"><input ID=\"LoginUsernameInput\" NAME=\"username\" VALUE=\"\" SIZE=\"20\" MAXLENGTH=\"50\" ></td>"
                    + "\r</tr>";
                    //
                    // ----- Password
                    //
                    if (genericController.encodeBoolean(cpcore.siteProperties.getBoolean("allowNoPasswordLogin", false))) {
                        Caption = "Password&nbsp;(optional)";
                    } else {
                        Caption = "Password";
                    }
                    loginForm = loginForm + "\r<tr>"
                    + cr2 + "<td style=\"text-align:right;vertical-align:middle;width:30%;padding:4px\" align=\"right\">" + SpanClassAdminNormal + Caption + "&nbsp;</span></td>"
                    + cr2 + "<td style=\"text-align:left;vertical-align:middle;width:70%;padding:4px\" align=\"left\" ><input NAME=\"password\" VALUE=\"\" SIZE=\"20\" MAXLENGTH=\"50\" type=\"password\"></td>"
                    + "\r</tr>"
                    + "";
                    //
                    // ----- autologin support
                    //
                    if (genericController.encodeBoolean(cpcore.siteProperties.getBoolean("AllowAutoLogin", false))) {
                        loginForm = loginForm + "\r<tr>"
                        + cr2 + "<td align=\"right\">&nbsp;</td>"
                        + cr2 + "<td align=\"left\" >"
                        + cr3 + "<table border=\"0\" cellpadding=\"5\" cellspacing=\"0\" width=\"100%\">"
                        + cr4 + "<tr>"
                        + cr5 + "<td valign=\"top\" width=\"20\"><input type=\"checkbox\" name=\"autologin\" value=\"ON\" checked></td>"
                        + cr5 + "<td valign=\"top\" width=\"100%\">" + SpanClassAdminNormal + "Login automatically from this computer</span></td>"
                        + cr4 + "</tr>"
                        + cr3 + "</table>"
                        + cr2 + "</td>"
                        + "\r</tr>";
                    }
                    loginForm = loginForm + "\r<tr>"
                        + cr2 + "<td colspan=\"2\">&nbsp;</td>"
                        + "\r</tr>"
                        + "";
                    loginForm = ""
                        + "\r<table border=\"0\" cellpadding=\"5\" cellspacing=\"0\" width=\"100%\">"
                        + htmlIndent(loginForm) + "\r</table>"
                        + "";
                    loginForm = loginForm + cpcore.html.inputHidden("Type", FormTypeLogin) + cpcore.html.inputHidden("email", cpcore.doc.authContext.user.Email) + cpcore.html.getPanelButtons(ButtonLogin, "Button") + "";
                    loginForm = ""
                        + cpcore.html.formStart(QueryString) + htmlIndent(loginForm) + "\r</form>"
                        + "";

                    //-------

                    Panel = ""
                        + errorController.getUserError(cpcore) + "\r<p class=\"ccAdminNormal\">" + usernameMsg + loginForm + "";
                    //
                    // ----- Password Form
                    //
                    if (genericController.encodeBoolean(cpcore.siteProperties.getBoolean("allowPasswordEmail", true))) {
                        Panel = ""
                            + Panel + "\r<p class=\"ccAdminNormal\"><b>Forget your password?</b></p>"
                            + "\r<p class=\"ccAdminNormal\">If you are a member of the system and can not remember your password, enter your email address below and we will email your matching username and password.</p>"
                            + getSendPasswordForm(cpcore) + "";
                    }
                    //
                    returnHtml = ""
                        + "\r<div class=\"ccLoginFormCon\">"
                        + htmlIndent(Panel) + "\r</div>"
                        + "";
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return returnHtml;
        }
        //
        //=============================================================================
        /// <summary>
        /// A login form that can be added to any page. This is just form with no surrounding border, etc. 
        /// </summary>
        /// <returns></returns>
        public static string getLoginForm(coreClass cpcore, bool forceDefaultLoginForm = false) {
            string returnHtml = "";
            try {
                int loginAddonID = 0;
                if (!forceDefaultLoginForm) {
                    loginAddonID = cpcore.siteProperties.getInteger("Login Page AddonID");
                    if (loginAddonID != 0) {
                        //
                        // -- Custom Login
                        addonModel addon = addonModel.create(cpcore, loginAddonID);
                        CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() { addonType = CPUtilsBaseClass.addonContext.ContextPage };
                        returnHtml = cpcore.addon.execute(addon, executeContext);
                        //returnHtml = cpcore.addon.execute_legacy2(loginAddonID, "", "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, 0, "", False, Nothing)
                        if (string.IsNullOrEmpty(returnHtml)) {
                            //
                            // -- login successful, redirect back to this page (without a method)
                            string QS = cpcore.doc.refreshQueryString;
                            QS = genericController.ModifyQueryString(QS, "method", "");
                            QS = genericController.ModifyQueryString(QS, "RequestBinary", "");
                            //
                            return cpcore.webServer.redirect("?" + QS, "Login form success");
                        }
                    }
                }
                if (loginAddonID == 0) {
                    //
                    // ----- When page loads, set focus on login username
                    //
                    returnHtml = getLoginForm_Default(cpcore);
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return returnHtml;
        }
        //
        //=============================================================================
        /// <summary>
        /// a simple email password form
        /// </summary>
        /// <returns></returns>
        public static string getSendPasswordForm(coreClass cpcore) {
            string returnResult = "";
            try {
                string QueryString = null;
                //
                if (cpcore.siteProperties.getBoolean("allowPasswordEmail", true)) {
                    returnResult = ""
                    + "\r<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">"
                    + cr2 + "<tr>"
                    + cr3 + "<td style=\"text-align:right;vertical-align:middle;width:30%;padding:4px\" align=\"right\" width=\"30%\">" + SpanClassAdminNormal + "Email</span></td>"
                    + cr3 + "<td style=\"text-align:left;vertical-align:middle;width:70%;padding:4px\" align=\"left\"  width=\"70%\"><input NAME=\"email\" VALUE=\"" + genericController.encodeHTML(cpcore.doc.authContext.user.Email) + "\" SIZE=\"20\" MAXLENGTH=\"50\"></td>"
                    + cr2 + "</tr>"
                    + cr2 + "<tr>"
                    + cr3 + "<td colspan=\"2\">&nbsp;</td>"
                    + cr2 + "</tr>"
                    + cr2 + "<tr>"
                    + cr3 + "<td colspan=\"2\">"
                    + htmlIndent(htmlIndent(cpcore.html.getPanelButtons(ButtonSendPassword, "Button"))) + cr3 + "</td>"
                    + cr2 + "</tr>"
                    + "\r</table>"
                    + "";
                    //
                    // write out all of the form input (except state) to hidden fields so they can be read after login
                    //
                    //
                    returnResult = ""
                    + returnResult + cpcore.html.inputHidden("Type", FormTypeSendPassword) + "";
                    foreach (string key in cpcore.docProperties.getKeyList()) {
                        var tempVar = cpcore.docProperties.getProperty(key);
                        if (tempVar.IsForm) {
                            switch (genericController.vbUCase(tempVar.Name)) {
                                case "S":
                                case "MA":
                                case "MB":
                                case "USERNAME":
                                case "PASSWORD":
                                case "EMAIL":
                                    break;
                                default:
                                    returnResult = returnResult + cpcore.html.inputHidden(tempVar.Name, tempVar.Value);
                                    break;
                            }
                        }
                    }
                    //
                    QueryString = cpcore.doc.refreshQueryString;
                    QueryString = genericController.ModifyQueryString(QueryString, "S", "");
                    QueryString = genericController.ModifyQueryString(QueryString, "ccIPage", "");
                    returnResult = ""
                    + cpcore.html.formStart(QueryString) + htmlIndent(returnResult) + "\r</form>"
                    + "";
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        // ----- Process the login form
        //========================================================================
        //
        public static bool processFormLoginDefault(coreClass cpcore) {
            bool returnREsult = false;
            try {
                int LocalMemberID = 0;
                string loginForm_Username = ""; // Values entered with the login form
                string loginForm_Password = ""; // =
                string loginForm_Email = ""; // =
                bool loginForm_AutoLogin = false; // =
                returnREsult = false;
                //
                if (true) {
                    //
                    // Processing can happen
                    //   1) early in init() -- legacy
                    //   2) as well as at the front of main_GetLoginForm - to support addon Login forms
                    // This flag prevents the default form from processing twice
                    //
                    loginForm_Username = cpcore.docProperties.getText("username");
                    loginForm_Password = cpcore.docProperties.getText("password");
                    loginForm_AutoLogin = cpcore.docProperties.getBoolean("autologin");
                    //
                    if ((cpcore.doc.authContext.visit.LoginAttempts < cpcore.siteProperties.maxVisitLoginAttempts) && cpcore.doc.authContext.visit.CookieSupport) {
                        LocalMemberID = cpcore.doc.authContext.authenticateGetId(cpcore, loginForm_Username, loginForm_Password);
                        if (LocalMemberID == 0) {
                            cpcore.doc.authContext.visit.LoginAttempts = cpcore.doc.authContext.visit.LoginAttempts + 1;
                            cpcore.doc.authContext.visit.saveObject(cpcore);
                        } else {
                            returnREsult = cpcore.doc.authContext.authenticateById(cpcore, LocalMemberID, cpcore.doc.authContext);
                            if (returnREsult) {
                                logController.logActivity2(cpcore, "successful username/password login", cpcore.doc.authContext.user.id, cpcore.doc.authContext.user.OrganizationID);
                            } else {
                                logController.logActivity2(cpcore, "bad username/password login", cpcore.doc.authContext.user.id, cpcore.doc.authContext.user.OrganizationID);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return returnREsult;
        }
        //
        //========================================================================
        // ----- Process the send password form
        //
        public static void processFormSendPassword(coreClass cpCore) {
            try {
                string returnUserMessage = "";
                sendPassword(cpCore, cpCore.docProperties.getText("email"), ref returnUserMessage);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Send the Member his username and password
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
        public static bool sendPassword(coreClass cpCore, string Email, ref string returnUserMessage) {
            bool result = false;
            returnUserMessage = "";
            try {
                string sqlCriteria = null;
                string Message = "";
                int CS = 0;
                string workingEmail = null;
                string FromAddress = "";
                string subject = "";
                bool allowEmailLogin = false;
                string Password = null;
                string Username = null;
                bool updateUser = false;
                int atPtr = 0;
                int Index = 0;
                string EMailName = null;
                bool usernameOK = false;
                int recordCnt = 0;
                int Ptr = 0;
                //
                const string passwordChrs = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ012345678999999";
                const int passwordChrsLength = 62;
                //
                workingEmail = genericController.encodeText(Email);
                //
                result = false;
                if (string.IsNullOrEmpty(workingEmail)) {
                    //hint = "110"
                    errorController.addUserError(cpCore, "Please enter your email address before requesting your username and password.");
                } else {
                    //hint = "120"
                    atPtr = genericController.vbInstr(1, workingEmail, "@");
                    if (atPtr < 2) {
                        //
                        // email not valid
                        //
                        //hint = "130"
                        errorController.addUserError(cpCore, "Please enter a valid email address before requesting your username and password.");
                    } else {
                        //hint = "140"
                        EMailName = vbMid(workingEmail, 1, atPtr - 1);
                        //
                        logController.logActivity2(cpCore, "password request for email " + workingEmail, cpCore.doc.authContext.user.id, cpCore.doc.authContext.user.OrganizationID);
                        //
                        allowEmailLogin = cpCore.siteProperties.getBoolean("allowEmailLogin", false);
                        recordCnt = 0;
                        sqlCriteria = "(email=" + cpCore.db.encodeSQLText(workingEmail) + ")";
                        if (true) {
                            sqlCriteria = sqlCriteria + "and((dateExpires is null)or(dateExpires>" + cpCore.db.encodeSQLDate(DateTime.Now) + "))";
                        }
                        CS = cpCore.db.csOpen("People", sqlCriteria, "ID", SelectFieldList: "username,password", PageSize: 1);
                        if (!cpCore.db.csOk(CS)) {
                            //
                            // valid login account for this email not found
                            //
                            if (encodeText(vbMid(workingEmail, atPtr + 1)).ToLower() == "contensive.com") {
                                //
                                // look for expired account to renew
                                //
                                cpCore.db.csClose(ref CS);
                                CS = cpCore.db.csOpen("People", "((email=" + cpCore.db.encodeSQLText(workingEmail) + "))", "ID", PageSize: 1);
                                if (cpCore.db.csOk(CS)) {
                                    //
                                    // renew this old record
                                    //
                                    //hint = "150"
                                    cpCore.db.csSet(CS, "developer", "1");
                                    cpCore.db.csSet(CS, "admin", "1");
                                    cpCore.db.csSet(CS, "dateExpires", DateTime.Now.AddDays(7).Date.ToString());
                                } else {
                                    //
                                    // inject support record
                                    //
                                    //hint = "150"
                                    cpCore.db.csClose(ref CS);
                                    CS = cpCore.db.csInsertRecord("people");
                                    cpCore.db.csSet(CS, "name", "Contensive Support");
                                    cpCore.db.csSet(CS, "email", workingEmail);
                                    cpCore.db.csSet(CS, "developer", "1");
                                    cpCore.db.csSet(CS, "admin", "1");
                                    cpCore.db.csSet(CS, "dateExpires", DateTime.Now.AddDays(7).Date.ToString());
                                }
                                cpCore.db.csSave2(CS);
                            } else {
                                //hint = "155"
                                errorController.addUserError(cpCore, "No current user was found matching this email address. Please try again. ");
                            }
                        }
                        if (cpCore.db.csOk(CS)) {
                            //hint = "160"
                            FromAddress = cpCore.siteProperties.getText("EmailFromAddress", "info@" + cpCore.webServer.requestDomain);
                            subject = "Password Request at " + cpCore.webServer.requestDomain;
                            Message = "";
                            while (cpCore.db.csOk(CS)) {
                                //hint = "170"
                                updateUser = false;
                                if (string.IsNullOrEmpty(Message)) {
                                    //hint = "180"
                                    Message = "This email was sent in reply to a request at " + cpCore.webServer.requestDomain + " for the username and password associated with this email address. ";
                                    Message = Message + "If this request was made by you, please return to the login screen and use the following:\r\n";
                                    Message = Message + "\r\n";
                                } else {
                                    //hint = "190"
                                    Message = Message + "\r\n";
                                    Message = Message + "Additional user accounts with the same email address: \r\n";
                                }
                                //
                                // username
                                //
                                //hint = "200"
                                Username = cpCore.db.csGetText(CS, "Username");
                                usernameOK = true;
                                if (!allowEmailLogin) {
                                    //hint = "210"
                                    if (Username != Username.Trim()) {
                                        //hint = "220"
                                        Username = Username.Trim();
                                        updateUser = true;
                                    }
                                    if (string.IsNullOrEmpty(Username)) {
                                        //hint = "230"
                                        //username = emailName & Int(Rnd() * 9999)
                                        usernameOK = false;
                                        Ptr = 0;
                                        while (!usernameOK && (Ptr < 100)) {
                                            //hint = "240"
                                            Username = EMailName + EncodeInteger(Math.Floor(EncodeNumber(Microsoft.VisualBasic.VBMath.Rnd() * 9999)));
                                            usernameOK = !cpCore.doc.authContext.isLoginOK(cpCore, Username, "test");
                                            Ptr = Ptr + 1;
                                        }
                                        //hint = "250"
                                        if (usernameOK) {
                                            updateUser = true;
                                        }
                                    }
                                    //hint = "260"
                                    Message = Message + " username: " + Username + "\r\n";
                                }
                                //hint = "270"
                                if (usernameOK) {
                                    //
                                    // password
                                    //
                                    //hint = "280"
                                    Password = cpCore.db.csGetText(CS, "Password");
                                    if (Password.Trim() != Password) {
                                        //hint = "290"
                                        Password = Password.Trim();
                                        updateUser = true;
                                    }
                                    //hint = "300"
                                    if (string.IsNullOrEmpty(Password)) {
                                        //hint = "310"
                                        for (Ptr = 0; Ptr <= 8; Ptr++) {
                                            //hint = "320"
                                            Index = EncodeInteger(Microsoft.VisualBasic.VBMath.Rnd() * passwordChrsLength);
                                            Password = Password + vbMid(passwordChrs, Index, 1);
                                        }
                                        //hint = "330"
                                        updateUser = true;
                                    }
                                    //hint = "340"
                                    Message = Message + " password: " + Password + "\r\n";
                                    result = true;
                                    if (updateUser) {
                                        //hint = "350"
                                        cpCore.db.csSet(CS, "username", Username);
                                        cpCore.db.csSet(CS, "password", Password);
                                    }
                                    recordCnt = recordCnt + 1;
                                }
                                cpCore.db.csGoNext(CS);
                            }
                        }
                    }
                }
                if (result) {
                    string sendStatus = "";
                    emailController.sendAdHoc(cpCore, workingEmail, FromAddress, subject, Message, "", "", "", true, false, 0, ref sendStatus);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        // ----- Process the send password form
        //========================================================================
        //
        public static void processFormJoin(coreClass cpcore) {
            try {
                string ErrorMessage = "";
                int CS = 0;
                string FirstName = null;
                string LastName = null;
                string FullName = null;
                string Email = null;
                int errorCode = 0;
                //
                string loginForm_Username = ""; // Values entered with the login form
                string loginForm_Password = ""; // =
                string loginForm_Email = ""; // =
                bool loginForm_AutoLogin = false; // =
                                                  //
                loginForm_Username = cpcore.docProperties.getText("username");
                loginForm_Password = cpcore.docProperties.getText("password");
                //
                if (!genericController.encodeBoolean(cpcore.siteProperties.getBoolean("AllowMemberJoin", false))) {
                    errorController.addUserError(cpcore, "This site does not accept public main_MemberShip.");
                } else {
                    if (!cpcore.doc.authContext.isNewLoginOK(cpcore, loginForm_Username, loginForm_Password, ref ErrorMessage, ref errorCode)) {
                        errorController.addUserError(cpcore, ErrorMessage);
                    } else {
                        if (!(cpcore.doc.debug_iUserError != "")) {
                            CS = cpcore.db.csOpen("people", "ID=" + cpcore.db.encodeSQLNumber(cpcore.doc.authContext.user.id));
                            if (!cpcore.db.csOk(CS)) {
                                cpcore.handleException(new Exception("Could not open the current members account to set the username and password."));
                            } else {
                                if ((cpcore.db.csGetText(CS, "username") != "") | (cpcore.db.csGetText(CS, "password") != "") | (cpcore.db.csGetBoolean(CS, "admin")) | (cpcore.db.csGetBoolean(CS, "developer"))) {
                                    //
                                    // if the current account can be logged into, you can not join 'into' it
                                    //
                                    cpcore.doc.authContext.logout(cpcore);
                                }
                                FirstName = cpcore.docProperties.getText("firstname");
                                LastName = cpcore.docProperties.getText("firstname");
                                FullName = FirstName + " " + LastName;
                                Email = cpcore.docProperties.getText("email");
                                cpcore.db.csSet(CS, "FirstName", FirstName);
                                cpcore.db.csSet(CS, "LastName", LastName);
                                cpcore.db.csSet(CS, "Name", FullName);
                                cpcore.db.csSet(CS, "username", loginForm_Username);
                                cpcore.db.csSet(CS, "password", loginForm_Password);
                                cpcore.doc.authContext.authenticateById(cpcore, cpcore.doc.authContext.user.id, cpcore.doc.authContext);
                            }
                            cpcore.db.csClose(ref CS);
                        }
                    }
                }
                cpcore.cache.invalidateAllObjectsInContent("People");
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
        }
    }
}
