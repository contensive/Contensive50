
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
using Contensive.BaseClasses;
//
namespace Contensive.Core.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// manage email send and receive
    /// </summary>
    public class emailController : IDisposable {
        //
        // ----- private instance storage
        //
        private coreController core;
        //
        //====================================================================================================
        //
        private static void appendEmailLog( coreController core, string logCopy ) {
            logController.logError(core, "emailController:" + logCopy);
        }
        //
        //====================================================================================================
        //
        public static string getBlockList(coreController core) {
            //
            string privatePathFilename = null;
            //
            if (!core.doc.emailBlockListLocalLoaded) {
                privatePathFilename = "etc\\SMTPBlockList.txt";
                core.doc.emailBlockList_Local = core.privateFiles.readFileText(privatePathFilename);
                core.doc.emailBlockListLocalLoaded = true;
            }
            return core.doc.emailBlockList_Local;
            //
        }
        //
        //====================================================================================================
        //
        public static bool isOnBlockedList(coreController core, string emailAddress) {
            return (getBlockList(core).IndexOf("\r\n" + emailAddress + "\t", StringComparison.CurrentCultureIgnoreCase) >= 0);
        }
        //
        //====================================================================================================
        //
        public static void addToBlockList( coreController core, string EmailAddress) {
            var blockList = getBlockList(core);
            if (!verifyEmailAddress(core, EmailAddress)) {
                //
                // bad email address
                //
            } else if (isOnBlockedList( core, EmailAddress )) {
                //
                // They are already in the list
                //
            } else {
                //
                // add them to the list
                //
                core.doc.emailBlockList_Local = blockList + "\r\n" + EmailAddress + "\t" + DateTime.Now;
                core.privateFiles.saveFile("Config\\SMTPBlockList.txt", core.doc.emailBlockList_Local);
                core.doc.emailBlockListLocalLoaded = false;
            }
        }
        //
        //====================================================================================================
        //
        private static bool verifyEmail(coreController core, emailClass email, string returnUserWarning) {
            bool result = false;
            try {
                if (!verifyEmailAddress(core, email.toAddress)) {
                    //
                    returnUserWarning = "The to-address is not valid.";
                } else if (!verifyEmailAddress(core, email.fromAddress)) {
                    //
                    returnUserWarning = "The from-address is not valid.";
                } else {
                    result = true;
                }
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// email address must have at least one character before the @, and have a valid email domain
        /// </summary>
        private static bool verifyEmailAddress(coreController core, string EmailAddress) {
            bool result = false;
            try {
                if (!string.IsNullOrWhiteSpace(EmailAddress)) {
                    string[] SplitArray = null;
                    if (EmailAddress.IndexOf("@") > 0) {
                        SplitArray = EmailAddress.Split('@');
                        if (SplitArray.GetUpperBound(0) == 1) {
                            result = verifyEmailDomain(core, SplitArray[1]);
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Server must have at least 3 digits, and one dot in the middle
        /// </summary>
        public static bool verifyEmailDomain(coreController core, string emailDomain) {
            bool result = false;
            try {
                //
                string[] SplitArray = null;
                //
                if (!string.IsNullOrWhiteSpace(emailDomain)) {
                    SplitArray = emailDomain.Split('.');
                    if (SplitArray.GetUpperBound(0) > 0) {
                        if ((SplitArray[0].Length > 0) && (SplitArray[1].Length > 0)) {
                            result = true;
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Send Email
        /// </summary>
        /// <returns>false if the email is not sent successfully and the returnUserWarning argument contains a user compatible message. If true, the returnUserWanting may contain a user compatible message about email issues.</returns>
        public static bool sendAdHoc(coreController core, string ToAddress, string FromAddress, string SubjectMessage, string BodyMessage, string BounceAddress, string ReplyToAddress, string ResultLogFilename, bool isImmediate, bool isHTML, int emailIdOrZeroForLog, ref string returnSendStatus) {
            bool result = false;
            try {
                //
                string htmlBody = null;
                string rootUrl = null;
                string iResultLogPathPage = null;
                //
                if (!verifyEmailAddress( core, ToAddress)) {
                    //
                    returnSendStatus = "Email not sent because the to-address is not valid.";
                } else if (!verifyEmailAddress(core, FromAddress)) {
                    //
                    returnSendStatus = "Email not sent because the from-address is not valid.";
                } else if (0 != genericController.vbInstr(1, getBlockList(core), "\r\n" + ToAddress + "\r\n", 1)) {
                    //
                    returnSendStatus = "Email not sent because the to-address is blocked by this application. See the Blocked Email Report.";
                } else {
                    //
                    iResultLogPathPage = ResultLogFilename;
                    //
                    // Test for from-address / to-address matches
                    //
                    if (genericController.vbLCase(FromAddress) == genericController.vbLCase(ToAddress)) {
                        FromAddress = core.siteProperties.getText("EmailFromAddress", "");
                        if (string.IsNullOrEmpty(FromAddress)) {
                            //
                            //
                            //
                            FromAddress = ToAddress;
                            returnSendStatus = "The from-address matches the to-address. This email was sent, but may be blocked by spam filtering.";
                        } else if (genericController.vbLCase(FromAddress) == genericController.vbLCase(ToAddress)) {
                            //
                            //
                            //
                            returnSendStatus = "The from-address matches the to-address [" + FromAddress + "] . This email was sent, but may be blocked by spam filtering.";
                        } else {
                            //
                            //
                            //
                            returnSendStatus = "The from-address matches the to-address. The from-address was changed to [" + FromAddress + "] to prevent it from being blocked by spam filtering.";
                        }
                    }
                    //
                    if (isHTML) {
                        //
                        // Fix links for HTML send
                        //
                        rootUrl = "http://" + core.appConfig.domainList[0] + "/";
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
                    }
                    addToQueue(core, isImmediate, new emailClass() {
                         attempts = 0,
                         BounceAddress = BounceAddress,
                         fromAddress = FromAddress,
                         htmlBody = htmlBody,
                         replyToAddress = ReplyToAddress,
                         subject = SubjectMessage,
                         textBody = BodyMessage,
                         toAddress = ToAddress
                    });
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
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
        public static bool sendPerson(coreController core, personModel person, string FromAddress, string subject, string Body, bool Immediate, bool HTML, int emailIdOrZeroForLog, string template, bool EmailAllowLinkEID, ref string returnSendStatus, string queryStringForLinkAppend ) {
            bool result = false;
            try {
                if (person == null) {
                    returnSendStatus = "The email was not sent because the recipient could not be found by thier id [" + person.id.ToString() + "]";
                } else {
                    //
                    // -- personalize subject
                    string personalizedSubject = subject;
                    //personalizedSubject  = contentCmdController.executeContentCommands(core, personalizedSubject, CPUtilsBaseClass.addonContext.ContextEmail, person.id, true, ref returnSendStatus);
                    personalizedSubject = activeContentController.renderHtmlForEmail(core, personalizedSubject, person.id, queryStringForLinkAppend);
                    //
                    // -- personalize body
                    string personalizedBody = Body;
                    //personalizedBody = contentCmdController.executeContentCommands(core, personalizedBody, CPUtilsBaseClass.addonContext.ContextEmail, person.id, true, ref returnSendStatus);
                    personalizedBody = activeContentController.renderHtmlForEmail(core, personalizedBody, person.id, queryStringForLinkAppend);
                    //
                    // -- encode template
                    if (!string.IsNullOrWhiteSpace(template)) {
                        string personalizedTemplate = template;
                        //personalizedTemplate = contentCmdController.executeContentCommands(core, personalizedTemplate, CPUtilsBaseClass.addonContext.ContextEmail, person.id, true, ref returnSendStatus);
                        personalizedTemplate = activeContentController.renderHtmlForEmail(core, personalizedTemplate, person.id, queryStringForLinkAppend);
                        //
                        // -- merge body into template
                        if (personalizedTemplate.IndexOf(fpoContentBox) != -1) {
                            personalizedBody = genericController.vbReplace(personalizedTemplate, fpoContentBox, personalizedBody);
                        } else {
                            personalizedBody = personalizedTemplate + personalizedBody;
                        }
                    }
                    //
                    // -- customized fields added by trigger code
                    personalizedBody = genericController.vbReplace(personalizedBody, "#member_id#", person.id.ToString());
                    personalizedBody = genericController.vbReplace(personalizedBody, "#member_email#", person.Email);

                    var email = new emailClass() {
                        attempts = 0,
                        BounceAddress = "",
                        emailId = 0,
                        fromAddress = FromAddress,
                        htmlBody = personalizedBody,
                        replyToAddress = "",
                        subject = personalizedSubject,
                        textBody = personalizedBody,
                        toAddress = person.Email
                    };
                    if (verifyEmail(core, email, returnSendStatus)) {
                        addToQueue(core, Immediate, email);
                        result = true;
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Send System Email. System emails are admin editable emails that can be programmatically sent, or sent by application events like page visits.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="emailName"></param>
        /// <param name="appendedCopy"></param>
        /// <param name="AdditionalMemberIDOrZero"></param>
        /// <returns></returns>
        public static string sendSystem(coreController core, string emailName, string appendedCopy = "", int AdditionalMemberIDOrZero = 0) {
            string returnString = "";
            try {
                //
                bool isAdmin = false;
                int iAdditionalMemberID = 0;
                int EmailRecordID = 0;
                string EmailFrom = null;
                string EmailSubjectSource = null;
                string EmailBodySource = null;
                string ConfirmBody = "";
                bool EmailAllowLinkEID = false;
                int EmailToConfirmationMemberID = 0;
                string EmailStatusMessage = "";
                string BounceAddress = null;
                string EmailTemplateSource = "";
                //
                returnString = "";
                iAdditionalMemberID = AdditionalMemberIDOrZero;
                //
                systemEmailModel email = systemEmailModel.createByName(core, emailName);
                if ( email == null ) {
                    email = systemEmailModel.add(core);
                    email.name = emailName; 
                    email.Subject = emailName;
                    email.FromAddress = core.siteProperties.getText("EmailAdmin", "webmaster@" + core.appConfig.domainList[0]);
                    email.save(core);
                    core.handleException(new ApplicationException("No system email was found with the name [" + emailName + "]. A new email blank was created but not sent."));
                } else {
                    //
                    // --- collect values needed for send
                    //
                    EmailRecordID = email.id;
                    EmailToConfirmationMemberID = email.TestMemberID;
                    EmailFrom = email.FromAddress;
                    EmailSubjectSource = email.Subject;
                    EmailBodySource = core.cdnFiles.readFileText( email.CopyFilename ) + appendedCopy;
                    EmailAllowLinkEID = email.AddLinkEID;
                    BounceAddress = core.siteProperties.getText("EmailBounceAddress", "");
                    if (string.IsNullOrEmpty(BounceAddress)) {
                        BounceAddress = EmailFrom;
                    }
                    emailTemplateModel emailTemplate = emailTemplateModel.create(core, email.EmailTemplateID);
                    if ( emailTemplate!=null) {
                        EmailTemplateSource = emailTemplate.BodyHTML;
                    }
                    if (string.IsNullOrWhiteSpace(EmailTemplateSource)) {
                        EmailTemplateSource = "<div style=\"padding:10px\"><ac type=content></div>";
                    }
                    //
                    // Spam Footer
                    //
                    if (email.AllowSpamFooter) {
                        //
                        // This field is default true, and non-authorable
                        // It will be true in all cases, except a possible unforseen exception
                        //
                        EmailTemplateSource = EmailTemplateSource + "<div style=\"clear: both;padding:10px;\">" + genericController.csv_GetLinkedText("<a href=\"" + genericController.encodeHTML("http://" + core.appConfig.domainList[0] + "/" + core.siteProperties.serverPageDefault + "?" + rnEmailBlockRecipientEmail + "=#member_email#") + "\">", core.siteProperties.getText("EmailSpamFooter", DefaultSpamFooter)) + "</div>";
                    }
                    //
                    // --- Send message to the additional member
                    //
                    if (iAdditionalMemberID != 0) {
                        EmailStatusMessage +=  BR + "Primary Recipient:" + BR;
                        personModel person = personModel.create(core, iAdditionalMemberID);
                        if (person == null) {
                            EmailStatusMessage += "&nbsp;&nbsp;Error: Not sent additional user [#" + iAdditionalMemberID + "] because the user record could not be found." + BR;
                        } else {
                            if (string.IsNullOrWhiteSpace(person.Email)) {
                                EmailStatusMessage += "&nbsp;&nbsp;Error: Not sent additional user [#" + iAdditionalMemberID + "] because their email address was blank." + BR;
                            } else {
                                string EmailStatus = "";
                                string queryStringForLinkAppend = "";
                                sendPerson(core, person, EmailFrom, EmailSubjectSource, EmailBodySource, false, true, EmailRecordID, EmailTemplateSource, EmailAllowLinkEID, ref EmailStatus, queryStringForLinkAppend);
                                EmailStatusMessage += "&nbsp;&nbsp;Sent to " + person.name + " at " + person.Email + ", Status = " + EmailStatus + BR;
                            }
                        }
                    }
                    //
                    // --- Send message to everyone selected
                    //
                    EmailStatusMessage += BR + "Recipients in selected System Email groups:" + BR;
                    List<int> peopleIdList = personModel.createidListForEmail(core, EmailRecordID);
                    foreach (var personId in peopleIdList) {
                        var person = personModel.create(core, personId);
                        if (person == null) {
                            EmailStatusMessage += "&nbsp;&nbsp;Error: Not sent user [#" + iAdditionalMemberID + "] because the user record could not be found." + BR;
                        } else {
                            if (string.IsNullOrWhiteSpace(person.Email)) {
                                EmailStatusMessage += "&nbsp;&nbsp;Error: Not sent user [#" + iAdditionalMemberID + "] because their email address was blank." + BR;
                            } else {
                                string EmailStatus = "";
                                string queryStringForLinkAppend = "";
                                sendPerson(core, person, EmailFrom, EmailSubjectSource, EmailBodySource, false, true, EmailRecordID, EmailTemplateSource, EmailAllowLinkEID, ref EmailStatus, queryStringForLinkAppend);
                                EmailStatusMessage += "&nbsp;&nbsp;Sent to " + person.name + " at " + person.Email + ", Status = " + EmailStatus + BR;
                            }
                        }
                    }
                    //
                    // --- Send the completion message to the administrator
                    //
                    if (EmailToConfirmationMemberID == 0) {
                        // 
                    } else {
                        personModel person = personModel.create(core, EmailToConfirmationMemberID);
                        if ( person!=null ) {
                            ConfirmBody =  "<div style=\"padding:10px;\">" + BR;
                            ConfirmBody +=  "The follow System Email was sent." + BR;
                            ConfirmBody +=  "" + BR;
                            ConfirmBody +=  "If this email includes personalization, each email sent was personalized to it's recipient. This confirmation has been personalized to you." + BR;
                            ConfirmBody +=  "" + BR;
                            ConfirmBody +=  "Subject: " + EmailSubjectSource + BR;
                            ConfirmBody +=  "From: " + EmailFrom + BR;
                            ConfirmBody +=  "Bounces return to: " + BounceAddress + BR;
                            ConfirmBody +=  "Body:" + BR;
                            ConfirmBody +=  "<div style=\"clear:all\">----------------------------------------------------------------------</div>" + BR;
                            ConfirmBody +=  EmailBodySource + BR;
                            ConfirmBody +=  "<div style=\"clear:all\">----------------------------------------------------------------------</div>" + BR;
                            ConfirmBody +=  "--- recipient list ---" + BR;
                            ConfirmBody +=  EmailStatusMessage + BR;
                            ConfirmBody +=  "--- end of list ---" + BR;
                            ConfirmBody +=  "</div>";
                            //
                            string EmailStatus = "";
                            string queryStringForLinkAppend = "";
                            sendPerson(core, person, EmailFrom, "System Email confirmation from " + core.appConfig.domainList[0], ConfirmBody, false, true, EmailRecordID, "", false, ref EmailStatus, queryStringForLinkAppend );
                            if (isAdmin && (!string.IsNullOrEmpty(EmailStatus))) {
                                returnString = "Administrator: There was a problem sending the confirmation email, " + EmailStatus;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return returnString;
        }
        //
        //====================================================================================================
        /// <summary>
        /// send the confirmation email as a test. 
        /// </summary>
        /// <param name="EmailID"></param>
        /// <param name="ConfirmationMemberID"></param>
        public static void sendConfirmationTest(coreController core, int EmailID, int ConfirmationMemberID) {
            try {
                string ConfirmFooter = "";
                int TotalCnt = 0;
                int BlankCnt = 0;
                int DupCnt = 0;
                string DupList = "";
                int BadCnt = 0;
                string BadList = "";
                int EmailLen = 0;
                string LastEmail = null;
                string Emailtext = null;
                string LastDupEmail = "";
                string EmailLine = null;
                string TotalList = "";
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
                string EmailStatus = null;
                //
                CS = core.db.csOpenRecord("email", EmailID);
                if (!core.db.csOk(CS)) {
                    errorController.addUserError(core, "There was a problem sending the email confirmation. The email record could not be found.");
                } else {
                    EmailSubject = core.db.csGet(CS, "Subject");
                    EmailBody = core.db.csGet(CS, "copyFilename");
                    //
                    // merge in template
                    //
                    EmailTemplate = "";
                    EMailTemplateID = core.db.csGetInteger(CS, "EmailTemplateID");
                    if (EMailTemplateID != 0) {
                        CSTemplate = core.db.csOpenRecord("Email Templates", EMailTemplateID, false, false, "BodyHTML");
                        if (core.db.csOk(CSTemplate)) {
                            EmailTemplate = core.db.csGet(CSTemplate, "BodyHTML");
                        }
                        core.db.csClose(ref CSTemplate);
                    }
                    //
                    // styles
                    //
                    //emailstyles = getStyles(EmailID)
                    //EmailBody = emailstyles & EmailBody
                    //
                    // spam footer
                    //
                    if (core.db.csGetBoolean(CS, "AllowSpamFooter")) {
                        //
                        // This field is default true, and non-authorable
                        // It will be true in all cases, except a possible unforseen exception
                        //
                        EmailBody = EmailBody + "<div style=\"clear:both;padding:10px;\">" + genericController.csv_GetLinkedText("<a href=\"" + genericController.encodeHTML(core.webServer.requestProtocol + core.webServer.requestDomain + requestAppRootPath + core.siteProperties.serverPageDefault + "?" + rnEmailBlockRecipientEmail + "=#member_email#") + "\">", core.siteProperties.getText("EmailSpamFooter", DefaultSpamFooter)) + "</div>";
                        EmailBody = genericController.vbReplace(EmailBody, "#member_email#", "UserEmailAddress");
                    }
                    //
                    // Confirm footer
                    //
                    var personIdList = personModel.createidListForEmail(core, EmailID);
                    if ( personIdList.Count==0) {
                        errorController.addUserError(core, "There are no valid recipients of this email, other than the confirmation address. Either no groups or topics were selected, or those selections contain no people with both a valid email addresses and 'Allow Group Email' enabled.");
                    } else {
                        foreach (var personId in personIdList) {
                            var person = personModel.create(core, personId);
                            Emailtext = person.Email;
                            EMailName = person.name;
                            EmailMemberID = person.id;
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
                        }
                    }
                    //
                    if (DupCnt == 1) {
                        errorController.addUserError(core, "There is 1 duplicate email address. See the test email for details.");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There is 1 duplicate email address. Only one email will be sent to each address. If the email includes personalization, or if you are using link authentication to automatically log in the user, you may want to correct duplicates to be sure the email is created correctly.<div style=\"margin:20px;\">" + DupList + "</div></div>";
                    } else if (DupCnt > 1) {
                        errorController.addUserError(core, "There are " + DupCnt + " duplicate email addresses. See the test email for details");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There are " + DupCnt + " duplicate email addresses. Only one email will be sent to each address. If the email includes personalization, or if you are using link authentication to automatically log in the user, you may want to correct duplicates to be sure the email is created correctly.<div style=\"margin:20px;\">" + DupList + "</div></div>";
                    }
                    //
                    if (BadCnt == 1) {
                        errorController.addUserError(core, "There is 1 invalid email address. See the test email for details.");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There is 1 invalid email address<div style=\"margin:20px;\">" + BadList + "</div></div>";
                    } else if (BadCnt > 1) {
                        errorController.addUserError(core, "There are " + BadCnt + " invalid email addresses. See the test email for details");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There are " + BadCnt + " invalid email addresses<div style=\"margin:20px;\">" + BadList + "</div></div>";
                    }
                    //
                    if (BlankCnt == 1) {
                        errorController.addUserError(core, "There is 1 blank email address. See the test email for details");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There is 1 blank email address.</div>";
                    } else if (BlankCnt > 1) {
                        errorController.addUserError(core, "There are " + DupCnt + " blank email addresses. See the test email for details.");
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
                        errorController.addUserError(core, "No confirmation email was send because a Confirmation member is not selected");
                    } else {
                        personModel person = personModel.create(core, ConfirmationMemberID);
                        if (person == null) {
                            errorController.addUserError(core, "No confirmation email was send because a Confirmation member is not selected");
                        } else { 
                            EmailBody = EmailBody + "<div style=\"clear:both;padding:10px;margin:10px;border:1px dashed #888;\">Administrator<br><br>" + ConfirmFooter + "</div>";
                            string queryStringForLinkAppend = "";
                            string sendStatus = "";
                            if (!sendPerson(core, person, core.db.csGetText(CS, "FromAddress"), EmailSubject, EmailBody, true, true, EmailID, EmailTemplate, false, ref sendStatus, queryStringForLinkAppend)) {
                                errorController.addUserError(core, EmailStatus);
                            }
                        }
                    }
                }
                core.db.csClose(ref CS);
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public static void sendForm( coreController core,string SendTo, string SendFrom, string SendSubject) {
            try {
                string Message = "";
                string iSendTo = null;
                string iSendFrom = null;
                string iSendSubject = null;
                //
                iSendTo = genericController.encodeText(SendTo);
                iSendFrom = genericController.encodeText(SendFrom);
                iSendSubject = genericController.encodeText(SendSubject);
                //
                if ((iSendTo.IndexOf("@")  == -1)) {
                    iSendTo = core.siteProperties.getText("TrapEmail");
                    iSendSubject = "EmailForm with bad Sendto address";
                    Message = "Subject: " + iSendSubject;
                    Message = Message + "\r\n";
                }
                Message = Message + "The form was submitted " + core.doc.profileStartTime + "\r\n";
                Message = Message + "\r\n";
                Message = Message + "All text fields are included, completed or not.\r\n";
                Message = Message + "Only those checkboxes that are checked are included.\r\n";
                Message = Message + "Entries are not in the order they appeared on the form.\r\n";
                Message = Message + "\r\n";
                foreach (string key in core.docProperties.getKeyList()) {
                    var tempVar = core.docProperties.getProperty(key);
                    if (tempVar.IsForm) {
                        if (genericController.vbUCase(tempVar.Value) == "ON") {
                            Message = Message + tempVar.Name + ": Yes\r\n\r\n";
                        } else {
                            Message = Message + tempVar.Name + ": " + tempVar.Value + "\r\n\r\n";
                        }
                    }
                }
                string sendStatus = "";
                sendAdHoc(core, iSendTo, iSendFrom, iSendSubject, Message,"","","", false, false,0, ref sendStatus);
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //====================================================================================================
        //
        public static void sendGroup(coreController core, string groupCommaList, string FromAddress, string subject, string Body, bool Immediate, bool HTML) {
            try {
                if (!string.IsNullOrWhiteSpace( groupCommaList)) {
                    List<string> groupList = groupCommaList.Split(',').ToList<string>().FindAll(t => !string.IsNullOrWhiteSpace(t));
                    if (groupList.Count > 0) {
                        List<personModel> personList = personModel.createListFromGroupList( core, groupList, true );
                        foreach (var person in personList) {
                            string emailStatus = "";
                            string queryStringForLinkAppend = "";
                            sendPerson(core, person, FromAddress, subject, Body, Immediate, HTML, 0, "", false, ref emailStatus, queryStringForLinkAppend);
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// add this email to the email queue
        /// </summary>
        private static void addToQueue(coreController core, bool immediate, emailClass email) {
            try {
                var record = emailQueueModel.add(core);
                record.immediate = immediate;
                record.toAddress = email.toAddress;
                record.subject = email.subject;
                record.content = Newtonsoft.Json.JsonConvert.SerializeObject(email);
                record.attempts = email.attempts;
                record.save(core);
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Send the emails in the current Queue
        /// </summary>
        public static void procesQueue(coreController core) {
            try {
                List<emailQueueModel> queue = emailQueueModel.createList(core, "", "immediate,id desc");
                foreach (emailQueueModel queueRecord in queue) {
                    emailQueueModel.delete(core, queueRecord.id);
                    emailClass email = Newtonsoft.Json.JsonConvert.DeserializeObject<emailClass>(queueRecord.content);
                    string ignoreMessage = "";
                    if (smtpController.sendSmtp(core, email, ref ignoreMessage)) {
                        //
                        // -- success, log the send
                        var log = emailLogModel.add(core);
                        log.ToAddress = email.toAddress;
                        log.FromAddress = email.fromAddress;
                        log.Subject = email.subject;
                        log.SendStatus = "ok";
                        log.LogType = EmailLogTypeImmediateSend;
                        log.EmailID = email.emailId;
                        log.save(core);
                    } else {
                        //
                        // -- fail, retry
                        if (email.attempts > 3) {
                            //
                            // -- too many retries, log error
                            var log = emailLogModel.add(core);
                            log.ToAddress = email.toAddress;
                            log.FromAddress = email.fromAddress;
                            log.Subject = email.subject;
                            log.SendStatus = "failed";
                            log.LogType = EmailLogTypeImmediateSend;
                            log.EmailID = email.emailId;
                            log.save(core);
                        } else {
                            //
                            // -- fail, add back to end of queue for retry
                            email.attempts += 1;
                            addToQueue(core, false, email);
                        }
                    }
                }
                return;
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //====================================================================================================        
        /// <summary>
        /// object for email queue serialization
        /// </summary>
        public class emailClass {
            public string toAddress;
            public string fromAddress;
            public string BounceAddress;
            public string replyToAddress;
            public string subject;
            public string textBody;
            public string htmlBody;
            public int attempts;
            public int emailId;
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

        public emailController(coreController core) {
            this.core = core;
        }
        //
        public void Dispose() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~emailController() {
            Dispose(false);
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