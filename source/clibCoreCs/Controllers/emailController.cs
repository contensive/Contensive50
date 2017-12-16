
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
using Contensive.BaseClasses;
//
namespace Contensive.Core.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class emailController : IDisposable {
        //
        // ----- constants
        //
        //Private Const invalidationDaysDefault As Double = 365
        //
        // ----- objects constructed that must be disposed
        //
        //Private cacheClient As Enyim.Caching.MemcachedClient
        //
        // ----- private instance storage
        //
        private coreClass cpcore;
        //
        // Email Block List - these are people who have asked to not have email sent to them from this site
        //   Loaded ondemand by csv_GetEmailBlockList
        //
        private string email_BlockList_Local { get; set; } = "";
        private bool email_BlockList_LocalLoaded { get; set; }
        //
        private string getBlockList() {
            //
            string Filename = null;
            //
            if (!email_BlockList_LocalLoaded) {
                Filename = "Config\\SMTPBlockList.txt";
                email_BlockList_Local = cpcore.privateFiles.readFile(Filename);
                email_BlockList_LocalLoaded = true;
            }
            return email_BlockList_Local;
            //
        }

        //
        //
        //
        public void addToBlockList(string EmailAddress) {
            var blockList = getBlockList();
            if (string.IsNullOrEmpty(EmailAddress)) {
                //
                // bad email address
                //
            } else if ((EmailAddress.IndexOf("@")  == -1) || (EmailAddress.IndexOf(".")  == -1)) {
                //
                // bad email address
                //
            } else if (blockList.IndexOf("\r\n" + EmailAddress + "\t") >= 0) {
                //
                // They are already in the list
                //
            } else {
                //
                // add them to the list
                //
                email_BlockList_Local = getBlockList() + "\r\n" + EmailAddress + "\t" + DateTime.Now;
                cpcore.privateFiles.saveFile("Config\\SMTPBlockList.txt", email_BlockList_Local);
                email_BlockList_LocalLoaded = false;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Send Email
        /// </summary>
        /// <param name="ToAddress"></param>
        /// <param name="FromAddress"></param>
        /// <param name="SubjectMessage"></param>
        /// <param name="BodyMessage"></param>
        /// <param name="BounceAddress"></param>
        /// <param name="ReplyToAddress"></param>
        /// <param name="ResultLogFilename"></param>
        /// <param name="isImmediate"></param>
        /// <param name="isHTML"></param>
        /// <param name="emailIdOrZeroForLog"></param>
        /// <returns>OK if send is successful, otherwise returns the principle issue as a user error.</returns>
        public string send(string ToAddress, string FromAddress, string SubjectMessage, string BodyMessage, string BounceAddress, string ReplyToAddress, string ResultLogFilename, bool isImmediate, bool isHTML, int emailIdOrZeroForLog) {
            string returnStatus = "";
            try {
                //
                string htmlBody = null;
                string rootUrl = null;
                smtpController EmailHandler = new smtpController(cpcore);
                string iResultLogPathPage = null;
                string WarningMsg = "";
                int CSLog = 0;
                //
                if (string.IsNullOrEmpty(ToAddress)) {
                    // block
                } else if ((ToAddress.IndexOf("@")  == -1) || (ToAddress.IndexOf(".")  == -1)) {
                    // block
                } else if (string.IsNullOrEmpty(FromAddress)) {
                    // block
                } else if ((FromAddress.IndexOf("@")  == -1) || (FromAddress.IndexOf(".")  == -1)) {
                    // block
                } else if (0 != genericController.vbInstr(1, getBlockList(), "\r\n" + ToAddress + "\r\n", 1)) {
                    //
                    // They are in the block list
                    //
                    returnStatus = "Recipient has blocked this email";
                } else {
                    //
                    iResultLogPathPage = ResultLogFilename;
                    //
                    // Test for from-address / to-address matches
                    //
                    if (genericController.vbLCase(FromAddress) == genericController.vbLCase(ToAddress)) {
                        FromAddress = cpcore.siteProperties.getText("EmailFromAddress", "");
                        if (string.IsNullOrEmpty(FromAddress)) {
                            //
                            //
                            //
                            FromAddress = ToAddress;
                            WarningMsg = "The from-address matches the to-address. This email was sent, but may be blocked by spam filtering.";
                        } else if (genericController.vbLCase(FromAddress) == genericController.vbLCase(ToAddress)) {
                            //
                            //
                            //
                            WarningMsg = "The from-address matches the to-address. This email was sent, but may be blocked by spam filtering.";
                        } else {
                            //
                            //
                            //
                            WarningMsg = "The from-address matches the to-address. The from-address was changed to " + FromAddress + " to prevent it from being blocked by spam filtering.";
                        }
                    }
                    //
                    if (isHTML) {
                        //
                        // Fix links for HTML send
                        //
                        rootUrl = "http://" + cpcore.serverConfig.appConfig.domainList[0] + "/";
                        BodyMessage = genericController.ConvertLinksToAbsolute(BodyMessage, rootUrl);
                        //
                        // compose body
                        //
                        htmlBody = ""
                            + "<html>"
                            + "<head>"
                            + "<Title>" + SubjectMessage + "</Title>"
                            + "<Base href=\"" + rootUrl + "\" >"
                            + "</head>"
                            + "<body class=\"ccBodyEmail\">"
                            + "<Base href=\"" + rootUrl + "\" >"
                            + BodyMessage + "</body>"
                            + "</html>";
                        returnStatus = EmailHandler.sendEmail5(ToAddress, FromAddress, SubjectMessage, BodyMessage, BounceAddress, ReplyToAddress, iResultLogPathPage, cpcore.siteProperties.getText("SMTPServer", "SMTP.YourServer.Com"), isImmediate, isHTML, "");
                    } else {
                        returnStatus = EmailHandler.sendEmail5(ToAddress, FromAddress, SubjectMessage, BodyMessage, BounceAddress, ReplyToAddress, iResultLogPathPage, cpcore.siteProperties.getText("SMTPServer", "SMTP.YourServer.Com"), isImmediate, isHTML, "");
                    }
                    if (string.IsNullOrEmpty(returnStatus)) {
                        returnStatus = WarningMsg;
                    }
                    //
                    // ----- Log the send
                    //
                    if (true) {
                        CSLog = cpcore.db.csInsertRecord("Email Log", 0);
                        if (cpcore.db.csOk(CSLog)) {
                            cpcore.db.csSet(CSLog, "Name", "System Email Send " + encodeText(DateTime.Now));
                            cpcore.db.csSet(CSLog, "LogType", EmailLogTypeImmediateSend);
                            cpcore.db.csSet(CSLog, "SendStatus", returnStatus);
                            cpcore.db.csSet(CSLog, "toaddress", ToAddress);
                            cpcore.db.csSet(CSLog, "fromaddress", FromAddress);
                            cpcore.db.csSet(CSLog, "Subject", SubjectMessage);
                            if (emailIdOrZeroForLog != 0) {
                                cpcore.db.csSet(CSLog, "emailid", emailIdOrZeroForLog);
                            }
                        }
                        cpcore.db.csClose(ref CSLog);
                    }
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return returnStatus;
        }
        //        '
        //        '
        //        '
        //        Public Function getStyles(ByVal EmailID As Integer) As String
        //            On Error GoTo ErrorTrap 'Const Tn = "getEmailStyles": 'Dim th as integer: th = profileLogMethodEnter(Tn)
        //            '
        //            getStyles = cpcore.html.html_getStyleSheet2(csv_contentTypeEnum.contentTypeEmail, 0, genericController.EncodeInteger(EmailID))
        //            If getStyles <> "" Then
        //                getStyles = "" _
        //                    & vbCrLf & StyleSheetStart _
        //                    & vbCrLf & getStyles _
        //                    & vbCrLf & StyleSheetEnd
        //            End If
        //            '
        //            '
        //            Exit Function
        ////ErrorTrap:
        //            cpcore.handleException(New Exception("Unexpected exception"))
        //        End Function
        //  
        //========================================================================
        /// <summary>
        /// Send email to a memberId, returns ok if send is successful, otherwise returns the principle issue as a user error.
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="FromAddress"></param>
        /// <param name="subject"></param>
        /// <param name="Body"></param>
        /// <param name="Immediate"></param>
        /// <param name="HTML"></param>
        /// <param name="emailIdOrZeroForLog"></param>
        /// <param name="template"></param>
        /// <param name="EmailAllowLinkEID"></param>
        /// <returns> returns ok if send is successful, otherwise returns the principle issue as a user error</returns>
        public string sendPerson(int personId, string FromAddress, string subject, string Body, bool Immediate, bool HTML, int emailIdOrZeroForLog, string template, bool EmailAllowLinkEID) {
            string returnStatus = "";
            try {
                int CS = 0;
                string ToAddress = null;
                string layoutError = "";
                string subjectEncoded = null;
                string bodyEncoded = null;
                string templateEncoded = null;
                //
                subjectEncoded = subject;
                bodyEncoded = Body;
                templateEncoded = template;
                //
                CS = cpcore.db.cs_openContentRecord("People", personId, 0, false, false, "email");
                if (cpcore.db.csOk(CS)) {
                    ToAddress = encodeText(cpcore.db.csGetText(CS, "email")).Trim(' ');
                    if (string.IsNullOrEmpty(ToAddress)) {
                        returnStatus = "The email was not sent because the to-address was blank.";
                    } else if ((ToAddress.IndexOf("@")  == -1) || (ToAddress.IndexOf(".")  == -1)) {
                        returnStatus = "The email was not sent because the to-address [" + ToAddress + "] was not valid.";
                    } else if (string.IsNullOrEmpty(FromAddress)) {
                        returnStatus = "The email was not sent because the from-address was blank.";
                    } else if ((FromAddress.IndexOf("@")  == -1) || (FromAddress.IndexOf(".")  == -1)) {
                        returnStatus = "The email was not sent because the from-address [" + FromAddress + "] was not valid.";
                    } else {
                        //
                        // encode subject
                        //
                        subjectEncoded = cpcore.html.executeContentCommands(null, subjectEncoded, CPUtilsBaseClass.addonContext.ContextEmail, personId, true, ref layoutError);
                        subjectEncoded = cpcore.html.convertActiveContentToHtmlForEmailSend(subjectEncoded, personId, "");
                        //subjectEncoded = cpcore.html.convertActiveContent_internal(subjectEncoded, personId, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, "", "http://" & cpcore.serverConfig.appConfig.domainList[0], True, 0, "", CPUtilsBaseClass.addonContext.ContextEmail, True, Nothing, False)
                        //
                        // encode Body
                        //
                        bodyEncoded = cpcore.html.executeContentCommands(null, bodyEncoded, CPUtilsBaseClass.addonContext.ContextEmail, personId, true, ref layoutError);
                        bodyEncoded = cpcore.html.convertActiveContentToHtmlForEmailSend(bodyEncoded, personId, "");
                        //bodyEncoded = cpcore.html.convertActiveContent_internal(bodyEncoded, personId, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, "", "http://" & cpcore.serverConfig.appConfig.domainList[0], True, 0, "", CPUtilsBaseClass.addonContext.ContextEmail, True, Nothing, False)
                        //
                        // encode template
                        //
                        if (!string.IsNullOrEmpty(templateEncoded)) {
                            templateEncoded = cpcore.html.executeContentCommands(null, templateEncoded, CPUtilsBaseClass.addonContext.ContextEmail, personId, true, ref layoutError);
                            templateEncoded = cpcore.html.convertActiveContentToHtmlForEmailSend(templateEncoded, personId, "");
                            //templateEncoded = cpcore.html.convertActiveContent_internal(templateEncoded, personId, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, "", "http://" & cpcore.serverConfig.appConfig.domainList[0], True, 0, "", CPUtilsBaseClass.addonContext.ContextEmail, True, Nothing, False)
                            //
                            if (templateEncoded.IndexOf(fpoContentBox)  != -1) {
                                bodyEncoded = genericController.vbReplace(templateEncoded, fpoContentBox, bodyEncoded);
                            } else {
                                bodyEncoded = templateEncoded + bodyEncoded;
                            }
                        }
                        bodyEncoded = genericController.vbReplace(bodyEncoded, "#member_id#", personId.ToString());
                        bodyEncoded = genericController.vbReplace(bodyEncoded, "#member_email#", ToAddress);
                        //
                        returnStatus = send(ToAddress, FromAddress, subjectEncoded, bodyEncoded, "", "", "", Immediate, HTML, emailIdOrZeroForLog);
                    }
                }
                cpcore.db.csClose(ref CS);
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return returnStatus;
        }
        //
        //========================================================================
        // Set the email sql for all members marked to receive the email
        //   Used to send the email and as body on the email test
        //========================================================================
        //
        public string getGroupSql(int EmailID) {
            return "SELECT "
                    + " u.ID AS ID"
                    + " ,u.Name AS Name"
                    + " ,u.Email AS Email "
                    + " "
                    + " from "
                    + " (((ccMembers u"
                    + " left join ccMemberRules mr on mr.memberid=u.id)"
                    + " left join ccGroups g on g.id=mr.groupid)"
                    + " left join ccEmailGroups r on r.groupid=g.id)"
                    + " "
                    + " where "
                    + " (r.EmailID=1) "
                    + " and(r.Active<>0) "
                    + " and(g.Active<>0) "
                    + " and(g.AllowBulkEmail<>0) "
                    + " and(mr.Active<>0) "
                    + " and(u.Active<>0) "
                    + " and(u.AllowBulkEmail<>0)"
                    + " AND((mr.DateExpires is null)OR(mr.DateExpires>'20161205 22:40:58:184')) "
                    + " "
                    + " group by "
                    + " u.ID, u.Name, u.Email "
                    + " "
                    + " having ((u.Email Is Not Null) and(u.Email<>'')) "
                    + " "
                    + " order by u.Email,u.ID"
                    + " ";
        }
        //
        // ----- Need to test this and make it public
        //
        //   This is what the admin site should call for both test and group email
        //   Making it public lets developers send email that administrators can control
        //
        public string sendSystem(string EMailName, string AdditionalCopy, int AdditionalMemberIDOrZero) {
            string returnString = "";
            try {
                //
                bool isAdmin = false;
                int iAdditionalMemberID = 0;
                string layoutError = null;
                string emailstyles = null;
                int EmailRecordID = 0;
                int CSPeople = 0;
                int CSEmail = 0;
                int CSLog = 0;
                string EmailToAddress = null;
                string EmailToName = null;
                string SQL = null;
                string EmailFrom = null;
                string EmailSubjectSource = null;
                string EmailBodySource = null;
                string ConfirmBody = string.Empty;
                bool EmailAllowLinkEID = false;
                int EmailToConfirmationMemberID = 0;
                string EmailStatusMessage = string.Empty;
                int EMailToMemberID = 0;
                string EmailSubject = null;
                string ClickFlagQuery = null;
                string EmailBody = null;
                string EmailStatus = null;
                string BounceAddress = null;
                string SelectList = null;
                int EMailTemplateID = 0;
                string EmailTemplate = null;
                string EmailTemplateSource = string.Empty;
                int CS = 0;
                bool isValid = false;
                //
                returnString = "";
                iAdditionalMemberID = AdditionalMemberIDOrZero;
                //
                if (true) {
                    SelectList = "ID,TestMemberID,FromAddress,Subject,copyfilename,AddLinkEID,AllowSpamFooter,EmailTemplateID";
                } else {
                    SelectList = "ID,TestMemberID,FromAddress,Subject,copyfilename,AddLinkEID,AllowSpamFooter,0 as EmailTemplateID";
                }
                CSEmail = cpcore.db.csOpen("System Email", "name=" + cpcore.db.encodeSQLText(EMailName), "ID", false, 0, false, false, SelectList);
                if (!cpcore.db.csOk(CSEmail)) {
                    //
                    // ----- Email was not found
                    //
                    cpcore.db.csClose(ref CSEmail);
                    CSEmail = cpcore.db.csInsertRecord("System Email");
                    cpcore.db.csSet(CSEmail, "name", EMailName);
                    cpcore.db.csSet(CSEmail, "Subject", EMailName);
                    cpcore.db.csSet(CSEmail, "FromAddress", cpcore.siteProperties.getText("EmailAdmin", "webmaster@" + cpcore.serverConfig.appConfig.domainList[0]));
                    //Call app.csv_SetCS(CSEmail, "caption", EmailName)
                    cpcore.db.csClose(ref CSEmail);
                    cpcore.handleException(new ApplicationException("No system email was found with the name [" + EMailName + "]. A new email blank was created but not sent."));
                } else {
                    //
                    // --- collect values needed for send
                    //
                    EmailRecordID = cpcore.db.csGetInteger(CSEmail, "ID");
                    EmailToConfirmationMemberID = cpcore.db.csGetInteger(CSEmail, "testmemberid");
                    EmailFrom = cpcore.db.csGetText(CSEmail, "FromAddress");
                    EmailSubjectSource = cpcore.db.csGetText(CSEmail, "Subject");
                    EmailBodySource = cpcore.db.csGet(CSEmail, "copyfilename") + AdditionalCopy;
                    EmailAllowLinkEID = cpcore.db.csGetBoolean(CSEmail, "AddLinkEID");
                    BounceAddress = cpcore.siteProperties.getText("EmailBounceAddress", "");
                    if (string.IsNullOrEmpty(BounceAddress)) {
                        BounceAddress = EmailFrom;
                    }
                    EMailTemplateID = cpcore.db.csGetInteger(CSEmail, "EmailTemplateID");
                    //
                    // Get the Email Template
                    //
                    if (EMailTemplateID != 0) {
                        CS = cpcore.db.cs_openContentRecord("Email Templates", EMailTemplateID);
                        if (cpcore.db.csOk(CS)) {
                            EmailTemplateSource = cpcore.db.csGet(CS, "BodyHTML");
                        }
                        cpcore.db.csClose(ref CS);
                    }
                    if (string.IsNullOrEmpty(EmailTemplateSource)) {
                        EmailTemplateSource = "<div style=\"padding:10px\"><ac type=content></div>";
                    }
                    //
                    // add styles to the template
                    //
                    //emailstyles = getStyles(EmailRecordID)
                    //EmailTemplateSource = emailstyles & EmailTemplateSource
                    //
                    // Spam Footer
                    //
                    if (cpcore.db.csGetBoolean(CSEmail, "AllowSpamFooter")) {
                        //
                        // This field is default true, and non-authorable
                        // It will be true in all cases, except a possible unforseen exception
                        //
                        EmailTemplateSource = EmailTemplateSource + "<div style=\"clear: both;padding:10px;\">" + genericController.csv_GetLinkedText("<a href=\"" + genericController.encodeHTML("http://" + cpcore.serverConfig.appConfig.domainList[0] + "/" + cpcore.siteProperties.serverPageDefault + "?" + rnEmailBlockRecipientEmail + "=#member_email#") + "\">", cpcore.siteProperties.getText("EmailSpamFooter", DefaultSpamFooter)) + "</div>";
                    }
                    //
                    // --- Send message to the additional member
                    //
                    if (iAdditionalMemberID != 0) {
                        EmailStatusMessage = EmailStatusMessage + BR + "Primary Recipient:" + BR;
                        CSPeople = cpcore.db.cs_openContentRecord("People", iAdditionalMemberID, 0, false, false, "ID,Name,Email");
                        if (cpcore.db.csOk(CSPeople)) {
                            EMailToMemberID = cpcore.db.csGetInteger(CSPeople, "ID");
                            EmailToName = cpcore.db.csGetText(CSPeople, "name");
                            EmailToAddress = cpcore.db.csGetText(CSPeople, "email");
                            if (string.IsNullOrEmpty(EmailToAddress)) {
                                EmailStatusMessage = EmailStatusMessage + "&nbsp;&nbsp;Error: Not Sent to " + EmailToName + " (people #" + EMailToMemberID + ") because their email address was blank." + BR;
                            } else {
                                EmailStatus = sendPerson(iAdditionalMemberID, EmailFrom, EmailSubjectSource, EmailBodySource, false, true, EmailRecordID, EmailTemplateSource, EmailAllowLinkEID);
                                if (string.IsNullOrEmpty(EmailStatus)) {
                                    EmailStatus = "ok";
                                }
                                EmailStatusMessage = EmailStatusMessage + "&nbsp;&nbsp;Sent to " + EmailToName + " at " + EmailToAddress + ", Status = " + EmailStatus + BR;
                            }
                        }
                        cpcore.db.csClose(ref CSPeople);
                    }
                    //
                    // --- Send message to everyone selected
                    //
                    EmailStatusMessage = EmailStatusMessage + BR + "Recipients in selected System Email groups:" + BR;
                    SQL = getGroupSql(EmailRecordID);
                    CSPeople = cpcore.db.csOpenSql_rev("default", SQL);
                    while (cpcore.db.csOk(CSPeople)) {
                        EMailToMemberID = cpcore.db.csGetInteger(CSPeople, "ID");
                        EmailToName = cpcore.db.csGetText(CSPeople, "name");
                        EmailToAddress = cpcore.db.csGetText(CSPeople, "email");
                        if (string.IsNullOrEmpty(EmailToAddress)) {
                            EmailStatusMessage = EmailStatusMessage + "&nbsp;&nbsp;Not Sent to " + EmailToName + ", people #" + EMailToMemberID + " because their email address was blank." + BR;
                        } else {
                            EmailStatus = sendPerson(EMailToMemberID, EmailFrom, EmailSubjectSource, EmailBodySource, false, true, EmailRecordID, EmailTemplateSource, EmailAllowLinkEID);
                            if (string.IsNullOrEmpty(EmailStatus)) {
                                EmailStatus = "ok";
                            }
                            EmailStatusMessage = EmailStatusMessage + "&nbsp;&nbsp;Sent to " + EmailToName + " at " + EmailToAddress + ", Status = " + EmailStatus + BR;
                            cpcore.db.csGoNext(CSPeople);
                        }
                    }
                    cpcore.db.csClose(ref CSPeople);
                    //
                    // --- Send the completion message to the administrator
                    //
                    if (EmailToConfirmationMemberID == 0) {
                        // AddUserError ("No confirmation email was sent because no confirmation member was selected")
                    } else {
                        //
                        // get the confirmation info
                        //
                        isValid = false;
                        CSPeople = cpcore.db.cs_openContentRecord("people", EmailToConfirmationMemberID);
                        if (cpcore.db.csOk(CSPeople)) {
                            isValid = cpcore.db.csGetBoolean(CSPeople, "active");
                            EMailToMemberID = cpcore.db.csGetInteger(CSPeople, "ID");
                            EmailToName = cpcore.db.csGetText(CSPeople, "name");
                            EmailToAddress = cpcore.db.csGetText(CSPeople, "email");
                            isAdmin = cpcore.db.csGetBoolean(CSPeople, "admin");
                        }
                        cpcore.db.csClose(ref CSPeople);
                        //
                        if (!isValid) {
                            //returnString = "Administrator: The confirmation email was not sent because the confirmation email person is not selected or inactive, " & EmailStatus
                        } else {
                            //
                            // Encode the body
                            //
                            EmailBody = EmailBodySource + "";
                            //
                            // Encode the template
                            //
                            EmailTemplate = EmailTemplateSource;
                            //
                            EmailSubject = EmailSubjectSource;
                            //
                            ConfirmBody = ConfirmBody + "<div style=\"padding:10px;\">" + BR;
                            ConfirmBody = ConfirmBody + "The follow System Email was sent." + BR;
                            ConfirmBody = ConfirmBody + "" + BR;
                            ConfirmBody = ConfirmBody + "If this email includes personalization, each email sent was personalized to it's recipient. This confirmation has been personalized to you." + BR;
                            ConfirmBody = ConfirmBody + "" + BR;
                            ConfirmBody = ConfirmBody + "Subject: " + EmailSubject + BR;
                            ConfirmBody = ConfirmBody + "From: " + EmailFrom + BR;
                            ConfirmBody = ConfirmBody + "Bounces return to: " + BounceAddress + BR;
                            ConfirmBody = ConfirmBody + "Body:" + BR;
                            ConfirmBody = ConfirmBody + "<div style=\"clear:all\">----------------------------------------------------------------------</div>" + BR;
                            ConfirmBody = ConfirmBody + EmailBody + BR;
                            ConfirmBody = ConfirmBody + "<div style=\"clear:all\">----------------------------------------------------------------------</div>" + BR;
                            ConfirmBody = ConfirmBody + "--- recipient list ---" + BR;
                            ConfirmBody = ConfirmBody + EmailStatusMessage + BR;
                            ConfirmBody = ConfirmBody + "--- end of list ---" + BR;
                            ConfirmBody = ConfirmBody + "</div>";
                            //
                            EmailStatus = sendPerson(EmailToConfirmationMemberID, EmailFrom, "System Email confirmation from " + cpcore.serverConfig.appConfig.domainList[0], ConfirmBody, false, true, EmailRecordID, "", false);
                            if (isAdmin && (!string.IsNullOrEmpty(EmailStatus))) {
                                returnString = "Administrator: There was a problem sending the confirmation email, " + EmailStatus;
                            }
                        }
                    }
                    //
                    // ----- Done
                    //
                    cpcore.db.csClose(ref CSPeople);
                }
                cpcore.db.csClose(ref CSEmail);
                //
                //
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                cpcore.handleException(ex);
            }
            return returnString;
        }
        //
        //========================================================================
        /// <summary>
        /// Send Email to address
        /// </summary>
        /// <param name="ToAddress"></param>
        /// <param name="FromAddress"></param>
        /// <param name="SubjectMessage"></param>
        /// <param name="BodyMessage"></param>
        /// <param name="optionalEmailIdForLog"></param>
        /// <param name="Immediate"></param>
        /// <param name="HTML"></param>
        /// <returns>Returns OK if successful, otherwise returns user status</returns>
        public string send_Legacy(string ToAddress, string FromAddress, string SubjectMessage, string BodyMessage, int optionalEmailIdForLog = 0, bool Immediate = true, bool HTML = false) {
            string returnStatus = "";
            try {
                returnStatus = send(genericController.encodeText(ToAddress), genericController.encodeText(FromAddress), genericController.encodeText(SubjectMessage), genericController.encodeText(BodyMessage), "", "", "", Immediate, genericController.encodeBoolean(HTML), genericController.EncodeInteger(optionalEmailIdForLog));
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return returnStatus;
        }
        //
        //====================================================================================================
        /// <summary>
        /// send the confirmation email as a test
        /// </summary>
        /// <param name="EmailID"></param>
        /// <param name="ConfirmationMemberID"></param>
        public void sendConfirmationTest(int EmailID, int ConfirmationMemberID) {
            try {
                string ConfirmFooter = string.Empty;
                int TotalCnt = 0;
                int BlankCnt = 0;
                int DupCnt = 0;
                string DupList = string.Empty;
                int BadCnt = 0;
                string BadList = string.Empty;
                int EmailLen = 0;
                string LastEmail = null;
                string Emailtext = null;
                string LastDupEmail = string.Empty;
                string EmailLine = null;
                string TotalList = string.Empty;
                string EMailName = null;
                int EmailMemberID = 0;
                int Posat = 0;
                int PosDot = 0;
                int CS = 0;
                string EmailSubject = null;
                string EmailBody = null;
                string EmailTemplate = null;
                int EMailTemplateID = 0;
                int CSTemplate = 0;
                int CSPeople = 0;
                string SQL = null;
                string EmailStatus = null;
                // Dim emailstyles As String
                //
                CS = cpcore.db.csOpenRecord("email", EmailID);
                if (!cpcore.db.csOk(CS)) {
                    errorController.error_AddUserError(cpcore, "There was a problem sending the email confirmation. The email record could not be found.");
                } else {
                    EmailSubject = cpcore.db.csGet(CS, "Subject");
                    EmailBody = cpcore.db.csGet(CS, "copyFilename");
                    //
                    // merge in template
                    //
                    EmailTemplate = "";
                    EMailTemplateID = cpcore.db.csGetInteger(CS, "EmailTemplateID");
                    if (EMailTemplateID != 0) {
                        CSTemplate = cpcore.db.csOpenRecord("Email Templates", EMailTemplateID, false, false, "BodyHTML");
                        if (cpcore.db.csOk(CSTemplate)) {
                            EmailTemplate = cpcore.db.csGet(CSTemplate, "BodyHTML");
                        }
                        cpcore.db.csClose(ref CSTemplate);
                    }
                    //
                    // styles
                    //
                    //emailstyles = getStyles(EmailID)
                    //EmailBody = emailstyles & EmailBody
                    //
                    // spam footer
                    //
                    if (cpcore.db.csGetBoolean(CS, "AllowSpamFooter")) {
                        //
                        // This field is default true, and non-authorable
                        // It will be true in all cases, except a possible unforseen exception
                        //
                        EmailBody = EmailBody + "<div style=\"clear:both;padding:10px;\">" + genericController.csv_GetLinkedText("<a href=\"" + genericController.encodeHTML(cpcore.webServer.requestProtocol + cpcore.webServer.requestDomain + requestAppRootPath + cpcore.siteProperties.serverPageDefault + "?" + rnEmailBlockRecipientEmail + "=#member_email#") + "\">", cpcore.siteProperties.getText("EmailSpamFooter", DefaultSpamFooter)) + "</div>";
                        EmailBody = genericController.vbReplace(EmailBody, "#member_email#", "UserEmailAddress");
                    }
                    //
                    // Confirm footer
                    //
                    SQL = getGroupSql(EmailID);
                    CSPeople = cpcore.db.csOpenSql(SQL);
                    if (!cpcore.db.csOk(CSPeople)) {
                        errorController.error_AddUserError(cpcore, "There are no valid recipients of this email, other than the confirmation address. Either no groups or topics were selected, or those selections contain no people with both a valid email addresses and 'Allow Group Email' enabled.");
                    } else {
                        //TotalList = TotalList & "--- all recipients ---" & BR
                        LastEmail = "empty";
                        while (cpcore.db.csOk(CSPeople)) {
                            Emailtext = cpcore.db.csGet(CSPeople, "email");
                            EMailName = cpcore.db.csGet(CSPeople, "name");
                            EmailMemberID = cpcore.db.csGetInteger(CSPeople, "ID");
                            if (string.IsNullOrEmpty(EMailName)) {
                                EMailName = "no name (member id " + EmailMemberID + ")";
                            }
                            EmailLine = Emailtext + " for " + EMailName;
                            if (string.IsNullOrEmpty(Emailtext)) {
                                BlankCnt = BlankCnt + 1;
                            } else {
                                if (Emailtext == LastEmail) {
                                    DupCnt = DupCnt + 1;
                                    if (Emailtext != LastDupEmail) {
                                        DupList = DupList + "<div class=i>" + Emailtext + "</div>" + BR;
                                        LastDupEmail = Emailtext;
                                    }
                                }
                            }
                            EmailLen = Emailtext.Length;
                            Posat = genericController.vbInstr(1, Emailtext, "@");
                            PosDot = Emailtext.LastIndexOf(".") + 1;
                            if (EmailLen < 6) {
                                BadCnt = BadCnt + 1;
                                BadList = BadList + EmailLine + BR;
                            } else if ((Posat < 2) || (Posat > (EmailLen - 4))) {
                                BadCnt = BadCnt + 1;
                                BadList = BadList + EmailLine + BR;
                            } else if ((PosDot < 4) || (PosDot > (EmailLen - 2))) {
                                BadCnt = BadCnt + 1;
                                BadList = BadList + EmailLine + BR;
                            }
                            TotalList = TotalList + EmailLine + BR;
                            LastEmail = Emailtext;
                            TotalCnt = TotalCnt + 1;
                            cpcore.db.csGoNext(CSPeople);
                        }
                        //TotalList = TotalList & "--- end all recipients ---" & BR
                    }
                    cpcore.db.csClose(ref CSPeople);
                    //
                    if (DupCnt == 1) {
                        errorController.error_AddUserError(cpcore, "There is 1 duplicate email address. See the test email for details.");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There is 1 duplicate email address. Only one email will be sent to each address. If the email includes personalization, or if you are using link authentication to automatically log in the user, you may want to correct duplicates to be sure the email is created correctly.<div style=\"margin:20px;\">" + DupList + "</div></div>";
                    } else if (DupCnt > 1) {
                        errorController.error_AddUserError(cpcore, "There are " + DupCnt + " duplicate email addresses. See the test email for details");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There are " + DupCnt + " duplicate email addresses. Only one email will be sent to each address. If the email includes personalization, or if you are using link authentication to automatically log in the user, you may want to correct duplicates to be sure the email is created correctly.<div style=\"margin:20px;\">" + DupList + "</div></div>";
                    }
                    //
                    if (BadCnt == 1) {
                        errorController.error_AddUserError(cpcore, "There is 1 invalid email address. See the test email for details.");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There is 1 invalid email address<div style=\"margin:20px;\">" + BadList + "</div></div>";
                    } else if (BadCnt > 1) {
                        errorController.error_AddUserError(cpcore, "There are " + BadCnt + " invalid email addresses. See the test email for details");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There are " + BadCnt + " invalid email addresses<div style=\"margin:20px;\">" + BadList + "</div></div>";
                    }
                    //
                    if (BlankCnt == 1) {
                        errorController.error_AddUserError(cpcore, "There is 1 blank email address. See the test email for details");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There is 1 blank email address.</div>";
                    } else if (BlankCnt > 1) {
                        errorController.error_AddUserError(cpcore, "There are " + DupCnt + " blank email addresses. See the test email for details.");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There are " + BlankCnt + " blank email addresses.</div>";
                    }
                    //
                    if (TotalCnt == 0) {
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There are no recipients for this email.</div>";
                    } else if (TotalCnt == 1) {
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">There is 1 recipient<div style=\"margin:20px;\">" + TotalList + "</div></div>";
                    } else {
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">There are " + TotalCnt + " recipients<div style=\"margin:20px;\">" + TotalList + "</div></div>";
                    }
                    //
                    if (ConfirmationMemberID == 0) {
                        errorController.error_AddUserError(cpcore, "No confirmation email was send because a Confirmation member is not selected");
                    } else {
                        EmailBody = EmailBody + "<div style=\"clear:both;padding:10px;margin:10px;border:1px dashed #888;\">Administrator<br><br>" + ConfirmFooter + "</div>";
                        EmailStatus = sendPerson(ConfirmationMemberID, cpcore.db.csGetText(CS, "FromAddress"), EmailSubject, EmailBody, true, true, EmailID, EmailTemplate, false);
                        if (EmailStatus != "ok") {
                            errorController.error_AddUserError(cpcore, EmailStatus);
                        }
                    }
                }
                cpcore.db.csClose(ref CS);
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
        }
        //
        //========================================================================
        // main_SendFormEmail
        //   sends an email with the contents of a form
        //========================================================================
        //
        public void sendForm(string SendTo, string SendFrom, string SendSubject) {
            try {
                string Message = string.Empty;
                string iSendTo = null;
                string iSendFrom = null;
                string iSendSubject = null;
                //
                iSendTo = genericController.encodeText(SendTo);
                iSendFrom = genericController.encodeText(SendFrom);
                iSendSubject = genericController.encodeText(SendSubject);
                //
                if ((iSendTo.IndexOf("@")  == -1)) {
                    iSendTo = cpcore.siteProperties.getText("TrapEmail");
                    iSendSubject = "EmailForm with bad Sendto address";
                    Message = "Subject: " + iSendSubject;
                    Message = Message + "\r\n";
                }
                Message = Message + "The form was submitted " + cpcore.doc.profileStartTime + "\r\n";
                Message = Message + "\r\n";
                Message = Message + "All text fields are included, completed or not.\r\n";
                Message = Message + "Only those checkboxes that are checked are included.\r\n";
                Message = Message + "Entries are not in the order they appeared on the form.\r\n";
                Message = Message + "\r\n";
                foreach (string key in cpcore.docProperties.getKeyList()) {
                    var tempVar = cpcore.docProperties.getProperty(key);
                    if (tempVar.IsForm) {
                        if (genericController.vbUCase(tempVar.Value) == "ON") {
                            Message = Message + tempVar.Name + ": Yes\r\n\r\n";
                        } else {
                            Message = Message + tempVar.Name + ": " + tempVar.Value + "\r\n\r\n";
                        }
                    }
                }
                //
                send_Legacy(iSendTo, iSendFrom, iSendSubject, Message, 0, false, false);
            } catch (Exception ex) {
                cpcore.handleException(ex);
            }
        }
        //
        //
        //
        public void sendGroup(string GroupList, string FromAddress, string subject, string Body, bool Immediate, bool HTML) {
            try {
                string rootUrl = null;
                string[] Groups = { };
                int GroupCount = 0;
                int GroupPointer = 0;
                string iiGroupList = null;
                int ParsePosition = 0;
                string iGroupList = null;
                string iFromAddress = null;
                string iSubjectSource = null;
                string iSubject = null;
                string iBodySource = null;
                string iBody = null;
                bool iImmediate = false;
                bool iHTML = false;
                string SQL = null;
                int CSPointer = 0;
                int ToMemberID = 0;
                //
                iGroupList = genericController.encodeText(GroupList);
                iFromAddress = genericController.encodeText(FromAddress);
                iSubjectSource = genericController.encodeText(subject);
                iBodySource = genericController.encodeText(Body);
                iImmediate = genericController.encodeBoolean(Immediate);
                iHTML = genericController.encodeBoolean(HTML);
                //
                // Fix links for HTML send - must do it now before encodehtml so eid links will attach
                //
                rootUrl = "http://" + cpcore.webServer.requestDomain + requestAppRootPath;
                iBodySource = genericController.ConvertLinksToAbsolute(iBodySource, rootUrl);
                //
                // Build the list of groups
                //
                if (!string.IsNullOrEmpty(iGroupList)) {
                    iiGroupList = iGroupList;
                    while (!string.IsNullOrEmpty(iiGroupList)) {
                        Array.Resize(ref Groups, GroupCount + 1);
                        ParsePosition = genericController.vbInstr(1, iiGroupList, ",");
                        if (ParsePosition == 0) {
                            Groups[GroupCount] = iiGroupList;
                            iiGroupList = "";
                        } else {
                            Groups[GroupCount] = iiGroupList.Left( ParsePosition - 1);
                            iiGroupList = iiGroupList.Substring(ParsePosition);
                        }
                        GroupCount = GroupCount + 1;
                    }
                }
                if (GroupCount > 0) {
                    //
                    // Build the SQL statement
                    //
                    SQL = "SELECT DISTINCT ccMembers.ID"
                        + " FROM (ccMembers LEFT JOIN ccMemberRules ON ccMembers.ID = ccMemberRules.MemberID) LEFT JOIN ccgroups ON ccMemberRules.GroupID = ccgroups.ID"
                        + " WHERE (((ccMembers.Active)<>0) AND ((ccMembers.AllowBulkEmail)<>0) AND ((ccMemberRules.Active)<>0) AND ((ccgroups.Active)<>0) AND ((ccgroups.AllowBulkEmail)<>0)AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" + cpcore.db.encodeSQLDate(cpcore.doc.profileStartTime) + ")) AND (";
                    for (GroupPointer = 0; GroupPointer < GroupCount; GroupPointer++) {
                        if (GroupPointer == 0) {
                            SQL += "(ccgroups.Name=" + cpcore.db.encodeSQLText(Groups[GroupPointer]) + ")";
                        } else {
                            SQL += "OR(ccgroups.Name=" + cpcore.db.encodeSQLText(Groups[GroupPointer]) + ")";
                        }
                    }
                    SQL += "));";
                    CSPointer = cpcore.db.csOpenSql(SQL);
                    while (cpcore.db.csOk(CSPointer)) {
                        ToMemberID = genericController.EncodeInteger(cpcore.db.csGetInteger(CSPointer, "ID"));
                        iSubject = iSubjectSource;
                        iBody = iBodySource;
                        //


                        // send
                        //
                        sendPerson(ToMemberID, iFromAddress, iSubject, iBody, iImmediate, iHTML, 0, "", false);
                        cpcore.db.csGoNext(CSPointer);
                    }
                }
                //
                return;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                cpcore.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
            //
        }
        //
        // ----- Need to test this and make it public
        //
        //   This is what the admin site should call for both test and group email
        //   Making it public lets developers send email that administrators can control
        //
        public void sendSystem_Legacy(string EMailName, string AdditionalCopy = "", int AdditionalMemberID = 0) {
            string EmailStatus;
            //
            EmailStatus = sendSystem(genericController.encodeText(EMailName), genericController.encodeText(AdditionalCopy), genericController.EncodeInteger(AdditionalMemberID));
            if (cpcore.doc.authContext.isAuthenticatedAdmin(cpcore) & (!string.IsNullOrEmpty(EmailStatus))) {
                errorController.error_AddUserError(cpcore, "Administrator: There was a problem sending the confirmation email, " + EmailStatus);
            }
            return;
        }
        //
        //=============================================================================
        // Send the Member his username and password
        //=============================================================================
        //
        public bool sendPassword(string Email) {
            bool returnREsult = false;
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
                returnREsult = false;
                if (string.IsNullOrEmpty(workingEmail)) {
                    //hint = "110"
                    errorController.error_AddUserError(cpcore, "Please enter your email address before requesting your username and password.");
                } else {
                    //hint = "120"
                    atPtr = genericController.vbInstr(1, workingEmail, "@");
                    if (atPtr < 2) {
                        //
                        // email not valid
                        //
                        //hint = "130"
                        errorController.error_AddUserError(cpcore, "Please enter a valid email address before requesting your username and password.");
                    } else {
                        //hint = "140"
                        EMailName = vbMid(workingEmail, 1, atPtr - 1);
                        //
                        logController.logActivity2(cpcore, "password request for email " + workingEmail, cpcore.doc.authContext.user.id, cpcore.doc.authContext.user.OrganizationID);
                        //
                        allowEmailLogin = cpcore.siteProperties.getBoolean("allowEmailLogin", false);
                        recordCnt = 0;
                        sqlCriteria = "(email=" + cpcore.db.encodeSQLText(workingEmail) + ")";
                        if (true) {
                            sqlCriteria = sqlCriteria + "and((dateExpires is null)or(dateExpires>" + cpcore.db.encodeSQLDate(DateTime.Now) + "))";
                        }
                        CS = cpcore.db.csOpen("People", sqlCriteria, "ID", SelectFieldList: "username,password", PageSize: 1);
                        if (!cpcore.db.csOk(CS)) {
                            //
                            // valid login account for this email not found
                            //
                            if (encodeText(vbMid(workingEmail, atPtr + 1)).ToLower() == "contensive.com") {
                                //
                                // look for expired account to renew
                                //
                                cpcore.db.csClose(ref CS);
                                CS = cpcore.db.csOpen("People", "((email=" + cpcore.db.encodeSQLText(workingEmail) + "))", "ID", PageSize: 1);
                                if (cpcore.db.csOk(CS)) {
                                    //
                                    // renew this old record
                                    //
                                    //hint = "150"
                                    cpcore.db.csSet(CS, "developer", "1");
                                    cpcore.db.csSet(CS, "admin", "1");
                                    cpcore.db.csSet(CS, "dateExpires", DateTime.Now.AddDays(7).Date.ToString());
                                } else {
                                    //
                                    // inject support record
                                    //
                                    //hint = "150"
                                    cpcore.db.csClose(ref CS);
                                    CS = cpcore.db.csInsertRecord("people");
                                    cpcore.db.csSet(CS, "name", "Contensive Support");
                                    cpcore.db.csSet(CS, "email", workingEmail);
                                    cpcore.db.csSet(CS, "developer", "1");
                                    cpcore.db.csSet(CS, "admin", "1");
                                    cpcore.db.csSet(CS, "dateExpires", DateTime.Now.AddDays(7).Date.ToString());
                                }
                                cpcore.db.csSave2(CS);
                            } else {
                                //hint = "155"
                                errorController.error_AddUserError(cpcore, "No current user was found matching this email address. Please try again. ");
                            }
                        }
                        if (cpcore.db.csOk(CS)) {
                            //hint = "160"
                            FromAddress = cpcore.siteProperties.getText("EmailFromAddress", "info@" + cpcore.webServer.requestDomain);
                            subject = "Password Request at " + cpcore.webServer.requestDomain;
                            Message = "";
                            while (cpcore.db.csOk(CS)) {
                                //hint = "170"
                                updateUser = false;
                                if (string.IsNullOrEmpty(Message)) {
                                    //hint = "180"
                                    Message = "This email was sent in reply to a request at " + cpcore.webServer.requestDomain + " for the username and password associated with this email address. ";
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
                                Username = cpcore.db.csGetText(CS, "Username");
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
                                            usernameOK = !cpcore.doc.authContext.isLoginOK(cpcore, Username, "test");
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
                                    Password = cpcore.db.csGetText(CS, "Password");
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
                                    returnREsult = true;
                                    if (updateUser) {
                                        //hint = "350"
                                        cpcore.db.csSet(CS, "username", Username);
                                        cpcore.db.csSet(CS, "password", Password);
                                    }
                                    recordCnt = recordCnt + 1;
                                }
                                cpcore.db.csGoNext(CS);
                            }
                        }
                    }
                }
                //hint = "360"
                if (returnREsult) {
                    cpcore.email.send_Legacy(workingEmail, FromAddress, subject, Message, 0, true, false);
                    //    main_ClosePageHTML = main_ClosePageHTML & main_GetPopupMessage(app.publicFiles.ReadFile("ccLib\Popup\PasswordSent.htm"), 300, 300, "no")
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return returnREsult;
        }

        //
        //====================================================================================================
        #region  IDisposable Support 
        //
        // this class must implement System.IDisposable
        // never throw an exception in dispose
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        //====================================================================================================
        //
        protected bool disposed = false;

        public emailController(coreClass cpCore) {
            this.cpcore = cpCore;
        }
        //
        public void Dispose() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~emailController() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(false);
            //INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        //
        //====================================================================================================
        /// <summary>
        /// dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    //If (cacheClient IsNot Nothing) Then
                    //    cacheClient.Dispose()
                    //End If
                }
                //
                // cleanup non-managed objects
                //
            }
        }
        #endregion
    }
    //
}