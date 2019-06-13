
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
//
// todo -- should not be here
using Contensive.BaseClasses;
using System.Xml.Linq;
//
namespace Contensive.Processor.Controllers {
    //
    public class LoginController {
        //
        //========================================================================
        /// <summary>
        /// A complete html page with the login form in the middle
        /// </summary>
        /// <param name="forceDefaultLogin"></param>
        /// <returns></returns>
        public static string getLoginPage(CoreController core, bool forceDefaultLogin) {
            string result = "";
            try {
                if (forceDefaultLogin) {
                    result = getLoginForm_Default(core);
                } else {
                    result = getLoginForm(core);
                }
                //result = htmlController.div(result, "ccCon bg-light");
                result = ""
                    + "<div class=\"ccCon bg-light pt-2 pb-4\" style=\"width:400px;margin:100px auto 0 auto;border:1px solid #bbb;border-radius:5px;\">"
                    + result
                    + "</div>";
                //result = ""
                //    + "\r<div class=\"ccCon bg-light\" style=\"width:400px;margin:100px auto 0 auto;\">"
                //    + nop(core.html.getPanelHeader("Login"))
                //    + nop(result)
                //    + "</div>";
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// process and return the default login form. If processing is successful, a blank response is returned
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getLoginForm_Default(CoreController core) {
            string result = "";
            try {
                //
                bool needLoginForm = true;
                string formType = core.docProperties.getText("type");
                if (formType == FormTypeLogin) {
                    //
                    // -- process a previous login for instance, and return blank if it is successful (legacy workflow)
                    if (processLoginFormDefault(core)) {
                        result = "";
                        needLoginForm = false;
                    }
                }
                if (needLoginForm) {
                    string loginForm;
                    //
                    // -- select the correct html from resources
                    bool allowAutoLogin = core.siteProperties.getBoolean("AllowAutoLogin", false);
                    if (core.siteProperties.getBoolean("allowEmailLogin", false)) {
                        if (allowAutoLogin) {
                            loginForm = Properties.Resources.defaultLogin_email_autoLogin_html;
                        } else {
                            loginForm = Properties.Resources.defaultLogin_email_html;
                        }
                    } else {
                        if (allowAutoLogin) {
                            loginForm = Properties.Resources.defaultLogin_autoLogin_html;
                        } else {
                            loginForm = Properties.Resources.defaultLogin_html;
                        }
                    }
                    //
                    // -- add user errors
                    loginForm = loginForm.Replace("{message}", ErrorController.getUserError(core));
                    if (!core.doc.errorList.Count.Equals(0)) {
                    }
                    //
                    // -- create the action query
                    string QueryString = GenericController.modifyQueryString(core.webServer.requestQueryString, RequestNameHardCodedPage, "", false);
                    QueryString = GenericController.modifyQueryString(QueryString, "requestbinary", "", false);
                    loginForm += HtmlController.inputHidden("Type", FormTypeLogin);
                    loginForm += HtmlController.inputHidden("email", core.session.user.email);
                    result = HtmlController.form(core, loginForm, QueryString);
                    //string Panel = errorController.getUserError(core) + "\r<p class=\"ccAdminNormal\">" + usernameMsg + loginForm + "";
                    //
                    // ----- Password Form
                    if (core.siteProperties.getBoolean("allowPasswordEmail", true)) {
                        result += getSendPasswordForm(core);
                    }
                    //
                    result = HtmlController.div(result, "ccLoginFormCon"); 
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return result;
        }
        //
        //=============================================================================
        /// <summary>
        /// A login form that can be added to any page. This is just form with no surrounding border, etc. 
        /// </summary>
        /// <returns></returns>
        public static string getLoginForm(CoreController core, bool forceDefaultLoginForm = false) {
            string returnHtml = "";
            try {
                int loginAddonID = 0;
                if (!forceDefaultLoginForm) {
                    loginAddonID = core.siteProperties.getInteger("Login Page AddonID");
                    if (loginAddonID != 0) {
                        //
                        // -- Custom Login
                        AddonModel addon = AddonModel.create(core, loginAddonID);
                        CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                            addonType = CPUtilsBaseClass.addonContext.ContextPage,
                            errorContextMessage = "calling login form addon [" + loginAddonID + "] from internal method"
                        };
                        returnHtml = core.addon.execute(addon, executeContext);
                        //returnHtml = core.addon.execute_legacy2(loginAddonID, "", "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, 0, "", False, Nothing)
                        if (string.IsNullOrEmpty(returnHtml)) {
                            //
                            // -- login successful, redirect back to this page (without a method)
                            string QS = core.doc.refreshQueryString;
                            QS = GenericController.modifyQueryString(QS, "method", "");
                            QS = GenericController.modifyQueryString(QS, "RequestBinary", "");
                            //
                            return core.webServer.redirect("?" + QS, "Login form success");
                        }
                    }
                }
                if (loginAddonID == 0) {
                    //
                    // ----- When page loads, set focus on login username
                    //
                    returnHtml = getLoginForm_Default(core);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
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
        public static string getSendPasswordForm(CoreController core) {
            string returnResult = "";
            try {
                string QueryString = null;
                //
                if (core.siteProperties.getBoolean("allowPasswordEmail", true)) {
                    returnResult = Properties.Resources.defaultForgetPassword_html;
                    //
                    // write out all of the form input (except state) to hidden fields so they can be read after login
                    returnResult = ""
                    + returnResult + HtmlController.inputHidden("Type", FormTypeSendPassword) + "";
                    foreach (string key in core.docProperties.getKeyList()) {
                        var tempVar = core.docProperties.getProperty(key);
                        if (tempVar.IsForm) {
                            switch (GenericController.vbUCase(tempVar.Name)) {
                                case "S":
                                case "MA":
                                case "MB":
                                case "USERNAME":
                                case "PASSWORD":
                                case "EMAIL":
                                    break;
                                default:
                                    returnResult = returnResult + HtmlController.inputHidden(tempVar.Name, tempVar.Value);
                                    break;
                            }
                        }
                    }
                    QueryString = core.doc.refreshQueryString;
                    QueryString = GenericController.modifyQueryString(QueryString, "S", "");
                    QueryString = GenericController.modifyQueryString(QueryString, "ccIPage", "");
                    returnResult = HtmlController.form( core,returnResult,QueryString);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        /// <summary>
        /// Process the login form username and password
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static bool processLoginFormDefault(CoreController core) {
            bool returnResult = false;
            try {
                //
                if ((core.session.visit.loginAttempts < core.siteProperties.maxVisitLoginAttempts) && core.session.visit.cookieSupport) {
                    int LocalMemberID = core.session.getUserIdForCredentials(
                        core.docProperties.getText("username"), 
                        core.docProperties.getText("password")
                    );
                    if (LocalMemberID == 0) {
                        if((core.session.isAuthenticated) ||(core.session.isRecognized())) { core.session.logout(); }
                        core.session.visit.loginAttempts = core.session.visit.loginAttempts + 1;
                        core.session.visit.save(core);
                    } else {
                        returnResult = core.session.authenticateById(LocalMemberID, core.session);
                        if (returnResult) {
                            LogController.addSiteActivity(core, "successful username/password login", core.session.user.id, core.session.user.organizationID);
                        } else {
                            LogController.addSiteActivity(core, "bad username/password login", core.session.user.id, core.session.user.organizationID);
                        }
                    }
                }

            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return returnResult;
        }
        //
        //========================================================================
        // ----- Process the send password form
        //
        public static void processSendPasswordForm(CoreController core) {
            try {
                string returnUserMessage = "";
                sendPassword(core, core.docProperties.getText("email"), ref returnUserMessage);
            } catch (Exception ex) {
                LogController.logError( core,ex);
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
        public static bool sendPassword(CoreController core, string Email, ref string returnUserMessage) {
            bool result = false;
            returnUserMessage = "";
            try {
                string sqlCriteria = null;
                string Message = "";
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
                workingEmail = GenericController.encodeText(Email);
                //
                result = false;
                if (string.IsNullOrEmpty(workingEmail)) {
                    //hint = "110"
                    ErrorController.addUserError(core, "Please enter your email address before requesting your username and password.");
                } else {
                    //hint = "120"
                    atPtr = GenericController.vbInstr(1, workingEmail, "@");
                    if (atPtr < 2) {
                        //
                        // email not valid
                        //
                        //hint = "130"
                        ErrorController.addUserError(core, "Please enter a valid email address before requesting your username and password.");
                    } else {
                        EMailName = vbMid(workingEmail, 1, atPtr - 1);
                        //
                        LogController.addSiteActivity(core, "password request for email " + workingEmail, core.session.user.id, core.session.user.organizationID);
                        //
                        allowEmailLogin = core.siteProperties.getBoolean("allowEmailLogin", false);
                        recordCnt = 0;
                        using (var csData = new CsModel(core)) {
                            sqlCriteria = "(email=" + DbController.encodeSQLText(workingEmail) + ")";
                            sqlCriteria = sqlCriteria + "and((dateExpires is null)or(dateExpires>" + DbController.encodeSQLDate(DateTime.Now) + "))";
                            csData.open("People", sqlCriteria, "ID",true,core.session.user.id,"username,password", 1);
                            if (!csData.ok()) {
                                //
                                // valid login account for this email not found
                                //
                                if (encodeText(vbMid(workingEmail, atPtr + 1)).ToLowerInvariant() == "contensive.com") {
                                    //
                                    // look for expired account to renew
                                    //
                                    csData.close();
                                    csData.open("People", "((email=" + DbController.encodeSQLText(workingEmail) + "))", "ID");
                                    if (csData.ok()) {
                                        //
                                        // renew this old record
                                        //
                                        //hint = "150"
                                        csData.set("developer", "1");
                                        csData.set("admin", "1");
                                        if ( csData.getDate( "dateExpires") > DateTime.MinValue ) { csData.set("dateExpires", DateTime.Now.AddDays(7).Date.ToString());  }
                                    } else {
                                        //
                                        // inject support record
                                        //
                                        csData.close();
                                        csData.insert("people");
                                        csData.set("name", "Contensive Support");
                                        csData.set("email", workingEmail);
                                        csData.set("developer", "1");
                                        csData.set("admin", "1");
                                        csData.set("dateExpires", DateTime.Now.AddDays(7).Date.ToString());
                                    }
                                } else {
                                    ErrorController.addUserError(core, "No current user was found matching this email address. Please try again. ");
                                }
                            }
                            if (csData.ok()) {
                                FromAddress = core.siteProperties.getText("EmailFromAddress", "info@" + core.webServer.requestDomain);
                                subject = "Password Request at " + core.webServer.requestDomain;
                                Message = "";
                                while (csData.ok()) {
                                    //hint = "170"
                                    updateUser = false;
                                    if (string.IsNullOrEmpty(Message)) {
                                        //hint = "180"
                                        Message = "This email was sent in reply to a request at " + core.webServer.requestDomain + " for the username and password associated with this email address. ";
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
                                    Username = csData.getText("Username");
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
                                                Username = EMailName + encodeInteger(Math.Floor(encodeNumber(Microsoft.VisualBasic.VBMath.Rnd() * 9999)));
                                                usernameOK = !core.session.isLoginOK(Username, "test");
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
                                        Password = csData.getText("Password");
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
                                                Index = encodeInteger(Microsoft.VisualBasic.VBMath.Rnd() * passwordChrsLength);
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
                                            csData.set("username", Username);
                                            csData.set("password", Password);
                                        }
                                        recordCnt = recordCnt + 1;
                                    }
                                    csData.goNext();
                                }
                            }
                        }
                    }
                }
                if (result) {
                    string sendStatus = "";
                    EmailController.queueAdHocEmail(core,"Password Email", core.session.user.id, workingEmail, FromAddress, subject, Message, "", "", "", true, false, 0, ref sendStatus);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        //
        public static void processJoinForm(CoreController core) {
            try {
                string ErrorMessage = "";
                string FirstName = null;
                string LastName = null;
                string FullName = null;
                string Email = null;
                int errorCode = 0;
                //
                string loginForm_Username = ""; 
                string loginForm_Password = ""; 
                loginForm_Username = core.docProperties.getText("username");
                loginForm_Password = core.docProperties.getText("password");
                //
                if (!GenericController.encodeBoolean(core.siteProperties.getBoolean("AllowMemberJoin", false))) {
                    ErrorController.addUserError(core, "This site does not accept public main_MemberShip.");
                } else {
                    if (!core.session.isNewCredentialOK( loginForm_Username, loginForm_Password, ref ErrorMessage, ref errorCode)) {
                        ErrorController.addUserError(core, ErrorMessage);
                    } else {
                        if (!(!core.doc.userErrorList.Count.Equals(0))) {
                            using (var csData = new CsModel(core)) {
                                csData.open("people", "ID=" + DbController.encodeSQLNumber(core.session.user.id));
                                if (!csData.ok()) {
                                    LogController.logError(core, new Exception("Could not open the current members account to set the username and password."));
                                } else {
                                    if ((csData.getText("username") != "") || (csData.getText("password") != "") || (csData.getBoolean("admin")) || (csData.getBoolean("developer"))) {
                                        //
                                        // if the current account can be logged into, you can not join 'into' it
                                        //
                                        core.session.logout();
                                    }
                                    FirstName = core.docProperties.getText("firstname");
                                    LastName = core.docProperties.getText("lastname");
                                    FullName = FirstName + " " + LastName;
                                    Email = core.docProperties.getText("email");
                                    csData.set("FirstName", FirstName);
                                    csData.set("LastName", LastName);
                                    csData.set("Name", FullName);
                                    csData.set("username", loginForm_Username);
                                    csData.set("password", loginForm_Password);
                                    core.session.authenticateById(core.session.user.id, core.session);
                                }
                                csData.close();
                            }
                        }
                    }
                }
                PersonModel.invalidateRecordCache(core, core.session.user.id);
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
    }
}
