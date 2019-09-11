
using System;
using System.Linq;
using System.Collections.Generic;

using static Contensive.Processor.Constants;
using Contensive.Processor.Exceptions;
using Contensive.Processor.Models.Domain;
using static Newtonsoft.Json.JsonConvert;
using Contensive.Models.Db;
//
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// manage email send and receive
    /// </summary>
    public class EmailController {
        //
        private const string emailBlockListFilename = "Config\\SMTPBlockList.txt";
        //
        //====================================================================================================
        //
        public static string getBlockList(CoreController core) {
            if (!core.doc.emailBlockListStoreLoaded) {
                core.doc.emailBlockListStore = core.privateFiles.readFileText(emailBlockListFilename);
                core.doc.emailBlockListStoreLoaded = true;
            }
            return core.doc.emailBlockListStore;
            //
        }
        //
        //====================================================================================================
        //
        public static bool isOnBlockedList(CoreController core, string emailAddress) {
            return (getBlockList(core).IndexOf(Environment.NewLine + emailAddress + "\t", StringComparison.CurrentCultureIgnoreCase) >= 0);
        }
        //
        //====================================================================================================
        //
        public static void addToBlockList(CoreController core, string EmailAddress) {
            var blockList = getBlockList(core);
            if (!verifyEmailAddress(core, EmailAddress)) {
                //
                // bad email address
                //
            } else if (isOnBlockedList(core, EmailAddress)) {
                //
                // They are already in the list
                //
            } else {
                //
                // add them to the list
                //
                core.doc.emailBlockListStore = blockList + Environment.NewLine + EmailAddress + "\t" + DateTime.Now;
                core.privateFiles.saveFile(emailBlockListFilename, core.doc.emailBlockListStore);
                core.doc.emailBlockListStoreLoaded = false;
            }
        }
        //
        //====================================================================================================
        //
        public static bool verifyEmail(CoreController core, EmailClass email, ref string returnUserWarning) {
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
                LogController.logError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// email address must have at least one character before the @, and have a valid email domain
        /// </summary>
        public static bool verifyEmailAddress(CoreController core, string EmailAddress) {
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
                LogController.logError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Server must have at least 3 digits, and one dot in the middle
        /// </summary>
        public static bool verifyEmailDomain(CoreController core, string emailDomain) {
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
                LogController.logError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Add an email to the queue
        /// </summary>
        /// <returns>false if the email is not sent successfully and the returnUserWarning argument contains a user compatible message. If true, the returnUserWanting may contain a user compatible message about email issues.</returns>
        public static bool queueAdHocEmail(CoreController core, string emailContextMessage, int loggedPersonId, string toAddress, string fromAddress, string subject, string body, string bounceAddress, string replyToAddress, string ignore, bool isImmediate, bool isHTML, int loggedEmailId, ref string returnSendStatus) {
            bool result = false;
            try {
                if (!verifyEmailAddress(core, toAddress)) {
                    //
                    returnSendStatus = "Email not sent because the to-address is not valid.";
                    LogController.logInfo(core, "queueAdHocEmail, NOT SENT [" + returnSendStatus + "], toAddress [" + toAddress + "], fromAddress [" + fromAddress + "], subject [" + subject + "]");
                } else if (!verifyEmailAddress(core, fromAddress)) {
                    //
                    returnSendStatus = "Email not sent because the from-address is not valid.";
                    LogController.logInfo(core, "queueAdHocEmail, NOT SENT [" + returnSendStatus + "], toAddress [" + toAddress + "], fromAddress [" + fromAddress + "], subject [" + subject + "]");
                } else if (0 != GenericController.vbInstr(1, getBlockList(core), Environment.NewLine + toAddress + Environment.NewLine, 1)) {
                    //
                    returnSendStatus = "Email not sent because the to-address is blocked by this application. See the Blocked Email Report.";
                    LogController.logInfo(core, "queueAdHocEmail, NOT SENT [" + returnSendStatus + "], toAddress [" + toAddress + "], fromAddress [" + fromAddress + "], subject [" + subject + "]");
                } else {
                    //
                    // Test for from-address / to-address matches
                    if (GenericController.vbLCase(fromAddress) == GenericController.vbLCase(toAddress)) {
                        fromAddress = core.siteProperties.getText("EmailFromAddress", "");
                        if (string.IsNullOrEmpty(fromAddress)) {
                            //
                            //
                            //
                            fromAddress = toAddress;
                            returnSendStatus = "The from-address matches the to-address. This email was sent, but may be blocked by spam filtering.";
                            LogController.logInfo(core, "queueAdHocEmail, sent with warning [" + returnSendStatus + "], toAddress [" + toAddress + "], fromAddress [" + fromAddress + "], subject [" + subject + "]");
                        } else if (GenericController.vbLCase(fromAddress) == GenericController.vbLCase(toAddress)) {
                            //
                            //
                            //
                            returnSendStatus = "The from-address matches the to-address [" + fromAddress + "] . This email was sent, but may be blocked by spam filtering.";
                            LogController.logInfo(core, "queueAdHocEmail, sent with warning [" + returnSendStatus + "], toAddress [" + toAddress + "], fromAddress [" + fromAddress + "], subject [" + subject + "]");
                        } else {
                            //
                            //
                            //
                            returnSendStatus = "The from-address matches the to-address. The from-address was changed to [" + fromAddress + "] to prevent it from being blocked by spam filtering.";
                            LogController.logInfo(core, "queueAdHocEmail, sent with warning [" + returnSendStatus + "], toAddress [" + toAddress + "], fromAddress [" + fromAddress + "], subject [" + subject + "]");
                        }
                    }
                    string htmlBody = encodeEmailHtmlBody(core, isHTML, body, "", subject, null, "" );
                    string textBody = encodeEmailTextBody(core, isHTML, body, null);
                    queueEmail(core, isImmediate, emailContextMessage, new EmailClass() {
                        attempts = 0,
                        BounceAddress = bounceAddress,
                        fromAddress = fromAddress,
                        htmlBody = htmlBody,
                        replyToAddress = replyToAddress,
                        subject = subject,
                        textBody = textBody,
                        toAddress = toAddress
                    });
                    LogController.logInfo(core, "queueAdHocEmail, added to queue, toAddress [" + toAddress + "], fromAddress [" + fromAddress + "], subject [" + subject + "]");
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return result;
        }
        //
        public static bool queueAdHocEmail(CoreController core, string emailContextMessage, int loggedPersonId, string toAddress, string fromAddress, string subject, string body, string bounceAddress, string replyToAddress, string ResultLogFilename, bool isImmediate, bool isHTML, int loggedEmailId) {
            string returnSendStatus = "";
            return queueAdHocEmail(core, emailContextMessage, loggedPersonId, toAddress, fromAddress, subject, body, bounceAddress, replyToAddress, ResultLogFilename, isImmediate, isHTML, loggedEmailId, ref returnSendStatus);
        }
        //
        public static bool queueAdHocEmail(CoreController core, string emailContextMessage, int loggedPersonId, string toAddress, string fromAddress, string subject, string body, string bounceAddress, string replyToAddress, string ResultLogFilename, bool isImmediate, bool isHTML) {
            string returnSendStatus = "";
            return queueAdHocEmail(core, emailContextMessage, loggedPersonId, toAddress, fromAddress, subject, body, bounceAddress, replyToAddress, ResultLogFilename, isImmediate, isHTML, 0, ref returnSendStatus);
        }
        //
        public static bool queueAdHocEmail(CoreController core, string emailContextMessage, int loggedPersonId, string toAddress, string fromAddress, string subject, string body, string bounceAddress, string replyToAddress, string ResultLogFilename, bool isImmediate) {
            string returnSendStatus = "";
            return queueAdHocEmail(core, emailContextMessage, loggedPersonId, toAddress, fromAddress, subject, body, bounceAddress, replyToAddress, ResultLogFilename, isImmediate, true, 0, ref returnSendStatus);
        }
        //
        public static bool queueAdHocEmail(CoreController core, string emailContextMessage, int loggedPersonId, string toAddress, string fromAddress, string subject, string body, string bounceAddress, string replyToAddress, string ResultLogFilename) {
            string returnSendStatus = "";
            return queueAdHocEmail(core, emailContextMessage, loggedPersonId, toAddress, fromAddress, subject, body, bounceAddress, replyToAddress, ResultLogFilename, false, true, 0, ref returnSendStatus);
        }
        //
        public static bool queueAdHocEmail(CoreController core, string emailContextMessage, int loggedPersonId, string toAddress, string fromAddress, string subject, string body, string bounceAddress, string replyToAddress) {
            string returnSendStatus = "";
            return queueAdHocEmail(core, emailContextMessage, loggedPersonId, toAddress, fromAddress, subject, body, bounceAddress, replyToAddress, "", false, true, 0, ref returnSendStatus);
        }
        //
        public static bool queueAdHocEmail(CoreController core, string emailContextMessage, int loggedPersonId, string toAddress, string fromAddress, string subject, string body, string bounceAddress) {
            string returnSendStatus = "";
            return queueAdHocEmail(core, emailContextMessage, loggedPersonId, toAddress, fromAddress, subject, body, bounceAddress, fromAddress, "", false, true, 0, ref returnSendStatus);
        }
        //
        public static bool queueAdHocEmail(CoreController core, string emailContextMessage, int loggedPersonId, string toAddress, string fromAddress, string subject, string body) {
            string returnSendStatus = "";
            return queueAdHocEmail(core, emailContextMessage, loggedPersonId, toAddress, fromAddress, subject, body, fromAddress, fromAddress, "", false, true, 0, ref returnSendStatus);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Send email to a memberId, returns ok if send is successful, otherwise returns the principle issue as a user error.
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="fromAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="Immediate"></param>
        /// <param name="isHTML"></param>
        /// <param name="emailIdOrZeroForLog"></param>
        /// <param name="template"></param>
        /// <param name="EmailAllowLinkEID"></param>
        /// <param name="queryStringForLinkAppend"></param>
        /// <param name="emailContextMessage">Brief description for the log entry (Conditional Email, etc)</param>
        /// <returns> returns ok if send is successful, otherwise returns the principle issue as a user error</returns>
        public static bool queuePersonEmail(CoreController core, PersonModel recipient, string fromAddress, string subject, string body, string bounceAddress, string replyToAddress, bool Immediate, bool isHTML, int emailIdOrZeroForLog, string template, bool EmailAllowLinkEID, ref string userErrorMessage, string queryStringForLinkAppend, string emailContextMessage) {
            bool result = false;
            try {
                if (recipient == null) {
                    userErrorMessage = "The email was not sent because the recipient could not be found by thier id [" + recipient.id.ToString() + "]";
                } else if (!verifyEmailAddress(core, recipient.email)) {
                    //
                    userErrorMessage = "Email not sent because the to-address is not valid.";
                } else if (!verifyEmailAddress(core, fromAddress)) {
                    //
                    userErrorMessage = "Email not sent because the from-address is not valid.";
                } else if (0 != GenericController.vbInstr(1, getBlockList(core), Environment.NewLine + recipient.email + Environment.NewLine, 1)) {
                    //
                    userErrorMessage = "Email not sent because the to-address is blocked by this application. See the Blocked Email Report.";
                } else {

                    string htmlBody = encodeEmailHtmlBody(core, isHTML, body, template, subject, recipient, queryStringForLinkAppend);
                    string textBody = encodeEmailTextBody(core, isHTML, body, recipient);
                    var email = new EmailClass() {
                        attempts = 0,
                        BounceAddress = bounceAddress,
                        emailId = 0,
                        fromAddress = fromAddress,
                        htmlBody = htmlBody,
                        replyToAddress = replyToAddress,
                        subject = subject,
                        textBody = textBody,
                        toAddress = recipient.email,
                        toMemberId = recipient.id
                    };
                    if (verifyEmail(core, email, ref userErrorMessage)) {
                        queueEmail(core, Immediate, emailContextMessage, email);
                        result = true;
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return result;
        }
        //
        public static bool queuePersonEmail(CoreController core, string emailContextMessage, PersonModel person, string fromAddress, string subject, string body, string bounceAddress, string replyToAddress, bool Immediate, bool isHTML, int emailIdOrZeroForLog, string template, bool EmailAllowLinkEID, ref string returnSendStatus) {
            return queuePersonEmail(core, person, fromAddress, subject, body, bounceAddress, replyToAddress, Immediate, isHTML, emailIdOrZeroForLog, template, EmailAllowLinkEID, ref returnSendStatus, "", emailContextMessage);
        }
        //
        public static bool queuePersonEmail(CoreController core, string emailContextMessage, PersonModel person, string fromAddress, string subject, string body, string bounceAddress, string replyToAddress, bool Immediate, bool isHTML, int emailIdOrZeroForLog, string template, bool EmailAllowLinkEID) {
            string returnSendStatus = "";
            return queuePersonEmail(core, person, fromAddress, subject, body, bounceAddress, replyToAddress, Immediate, isHTML, emailIdOrZeroForLog, template, EmailAllowLinkEID, ref returnSendStatus, "", emailContextMessage);
        }
        //
        public static bool queuePersonEmail(CoreController core, string emailContextMessage, PersonModel person, string fromAddress, string subject, string body, string bounceAddress, string replyToAddress, bool Immediate, bool isHTML, int emailIdOrZeroForLog, string template) {
            string returnSendStatus = "";
            return queuePersonEmail(core, person, fromAddress, subject, body, bounceAddress, replyToAddress, Immediate, isHTML, emailIdOrZeroForLog, template, false, ref returnSendStatus, "", emailContextMessage);
        }
        //
        public static bool queuePersonEmail(CoreController core, string emailContextMessage, PersonModel person, string fromAddress, string subject, string body, string bounceAddress, string replyToAddress, bool Immediate, bool isHTML, int emailIdOrZeroForLog) {
            string returnSendStatus = "";
            return queuePersonEmail(core, person, fromAddress, subject, body, bounceAddress, replyToAddress, Immediate, isHTML, emailIdOrZeroForLog, "", false, ref returnSendStatus, "", emailContextMessage);
        }
        //
        public static bool queuePersonEmail(CoreController core, string emailContextMessage, PersonModel person, string fromAddress, string subject, string body, string bounceAddress, string replyToAddress, bool Immediate, bool isHTML) {
            string returnSendStatus = "";
            return queuePersonEmail(core, person, fromAddress, subject, body, bounceAddress, replyToAddress, Immediate, isHTML, 0, "", false, ref returnSendStatus, "", emailContextMessage);
        }
        //
        public static bool queuePersonEmail(CoreController core, string emailContextMessage, PersonModel person, string fromAddress, string subject, string body, string bounceAddress, string replyToAddress, bool Immediate) {
            string returnSendStatus = "";
            return queuePersonEmail(core, person, fromAddress, subject, body, bounceAddress, replyToAddress, Immediate, true, 0, "", false, ref returnSendStatus, "", emailContextMessage);
        }
        //
        public static bool queuePersonEmail(CoreController core, string emailContextMessage, PersonModel person, string fromAddress, string subject, string body, string bounceAddress, string replyToAddress) {
            string returnSendStatus = "";
            return queuePersonEmail(core, person, fromAddress, subject, body, bounceAddress, replyToAddress, true, true, 0, "", false, ref returnSendStatus, "", emailContextMessage);
        }
        //
        public static bool queuePersonEmail(CoreController core, string emailContextMessage, PersonModel person, string fromAddress, string subject, string body, string bounceAddress) {
            string returnSendStatus = "";
            return queuePersonEmail(core, person, fromAddress, subject, body, bounceAddress, fromAddress, true, true, 0, "", false, ref returnSendStatus, "", emailContextMessage);
        }
        //
        public static bool queuePersonEmail(CoreController core, string emailContextMessage, PersonModel person, string fromAddress, string subject, string body) {
            string returnSendStatus = "";
            return queuePersonEmail(core, person, fromAddress, subject, body, fromAddress, fromAddress, true, true, 0, "", false, ref returnSendStatus, "", emailContextMessage);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Send System Email. System emails are admin editable emails that can be programmatically sent, or sent by application events like page visits.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="emailName"></param>
        /// <param name="appendedCopy"></param>
        /// <param name="additionalMemberID"></param>
        /// <returns>Admin message if something went wrong (email addresses checked, etc.</returns>
        public static bool queueSystemEmail(CoreController core, string emailName, string appendedCopy, int additionalMemberID, ref string userErrorMessage) {
            SystemEmailModel email = DbBaseModel.createByUniqueName<SystemEmailModel>(core.cpParent, emailName);
            if (email == null) {
                if (emailName.IsNumeric()) {
                    //
                    // -- compatibility for really ugly legacy nonsense where old interface has argument "EmailIdOrName".
                    email = DbBaseModel.create<SystemEmailModel>(core.cpParent, GenericController.encodeInteger(emailName));
                }
                if (email == null) {
                    //
                    // -- create new system email with this name - exposure of possible integer used as name
                    email = DbBaseModel.addDefault<SystemEmailModel>(core.cpParent, ContentMetadataModel.getDefaultValueDict(core, SystemEmailModel.contentName));
                    email.name = emailName;
                    email.subject = emailName;
                    email.fromAddress = core.siteProperties.getText("EmailAdmin", "webmaster@" + core.appConfig.domainList[0]);
                    email.save(core.cpParent);
                    LogController.logError(core, new GenericException("No system email was found with the name [" + emailName + "]. A new email blank was created but not sent."));
                }
            }
            return queueSystemEmail(core, email, appendedCopy, additionalMemberID, ref userErrorMessage);
        }
        //
        public static bool queueSystemEmail(CoreController core, string emailName, string appendedCopy, int additionalMemberID) {
            string userErrorMessage = "";
            return queueSystemEmail(core, emailName, appendedCopy, additionalMemberID, ref userErrorMessage);
        }
        //
        public static bool queueSystemEmail(CoreController core, string emailName, string appendedCopy) {
            string userErrorMessage = "";
            return queueSystemEmail(core, emailName, appendedCopy, 0, ref userErrorMessage);
        }
        //
        public static bool queueSystemEmail(CoreController core, string emailName) {
            string userErrorMessage = "";
            return queueSystemEmail(core, emailName, "", 0, ref userErrorMessage);
        }
        //
        public static bool queueSystemEmail(CoreController core, int emailid, string appendedCopy, int additionalMemberID, ref string userErrorMessage) {
            SystemEmailModel email = DbBaseModel.create<SystemEmailModel>(core.cpParent, emailid);
            if (email == null) {
                userErrorMessage = "The notification email could not be sent.";
                LogController.logError(core, new GenericException("No system email was found with the id [" + emailid + "]"));
                return false;
            }
            return queueSystemEmail(core, email, appendedCopy, additionalMemberID, ref userErrorMessage);
        }
        //
        public static bool queueSystemEmail(CoreController core, int emailid, string appendedCopy, int additionalMemberID) {
            string userErrorMessage = "";
            return queueSystemEmail(core, emailid, appendedCopy, additionalMemberID, ref userErrorMessage);
        }
        //
        public static bool queueSystemEmail(CoreController core, int emailid, string appendedCopy) {
            string userErrorMessage = "";
            return queueSystemEmail(core, emailid, appendedCopy, 0, ref userErrorMessage);
        }
        //
        public static bool queueSystemEmail(CoreController core, int emailid) {
            string userErrorMessage = "";
            return queueSystemEmail(core, emailid, "", 0, ref userErrorMessage);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Send a system email
        /// </summary>
        /// <param name="core"></param>
        /// <param name="email"></param>
        /// <param name="appendedCopy"></param>
        /// <param name="additionalMemberID"></param>
        /// <returns>Admin message if something went wrong (email addresses checked, etc.</returns>
        public static bool queueSystemEmail(CoreController core, SystemEmailModel email, string appendedCopy, int additionalMemberID, ref string userErrorMessage) {
            try {
                string BounceAddress = core.siteProperties.getText("EmailBounceAddress", "");
                if (string.IsNullOrEmpty(BounceAddress)) {
                    BounceAddress = email.fromAddress;
                }
                EmailTemplateModel emailTemplate = DbBaseModel.create<EmailTemplateModel>(core.cpParent, email.emailTemplateID);
                string EmailTemplateSource = "";
                if (emailTemplate != null) {
                    EmailTemplateSource = emailTemplate.bodyHTML;
                }
                if (string.IsNullOrWhiteSpace(EmailTemplateSource)) {
                    EmailTemplateSource = "<div style=\"padding:10px\"><ac type=content></div>";
                }
                //
                // Spam Footer
                //
                if (email.allowSpamFooter) {
                    //
                    // This field is default true, and non-authorable
                    // It will be true in all cases, except a possible unforseen exception
                    //
                    EmailTemplateSource = EmailTemplateSource + "<div style=\"clear: both;padding:10px;\">" + GenericController.csv_GetLinkedText("<a href=\"" + HtmlController.encodeHtml("http://" + core.appConfig.domainList[0] + "/" + core.siteProperties.serverPageDefault + "?" + rnEmailBlockRecipientEmail + "=#member_email#") + "\">", core.siteProperties.getText("EmailSpamFooter", DefaultSpamFooter)) + "</div>";
                }
                string confirmationMessage = "";
                //
                // --- collect values needed for send
                int EmailRecordID = email.id;
                string EmailSubjectSource = email.subject;
                // 20180703 -- textFilename fields when configured in the model as a text are read with .getText() which populates the property with the content, not the filename
                string EmailBodySource = email.copyFilename.content + appendedCopy;
                bool EmailAllowLinkEID = email.addLinkEID;
                //
                // --- Send message to the additional member
                if (additionalMemberID != 0) {
                    confirmationMessage += BR + "Primary Recipient:" + BR;
                    PersonModel person = DbBaseModel.create<PersonModel>(core.cpParent, additionalMemberID);
                    if (person == null) {
                        confirmationMessage += "&nbsp;&nbsp;Error: Not sent to additional user [#" + additionalMemberID + "] because the user record could not be found." + BR;
                    } else {
                        if (string.IsNullOrWhiteSpace(person.email)) {
                            confirmationMessage += "&nbsp;&nbsp;Error: Not sent to additional user [#" + additionalMemberID + "] because their email address was blank." + BR;
                        } else {
                            string EmailStatus = "";
                            string queryStringForLinkAppend = "";
                            queuePersonEmail(core, person, email.fromAddress, EmailSubjectSource, EmailBodySource, "", "", false, true, EmailRecordID, EmailTemplateSource, EmailAllowLinkEID, ref EmailStatus, queryStringForLinkAppend, "System Email");
                            confirmationMessage += "&nbsp;&nbsp;Sent to " + person.name + " at " + person.email + ", Status = " + EmailStatus + BR;
                        }
                    }
                }
                //
                // --- Send message to everyone selected
                //
                confirmationMessage += BR + "Recipients in selected System Email groups:" + BR;
                List<int> peopleIdList = PersonModel.createidListForEmail(core.cpParent, EmailRecordID);
                foreach (var personId in peopleIdList) {
                    var person = DbBaseModel.create<PersonModel>(core.cpParent, personId);
                    if (person == null) {
                        confirmationMessage += "&nbsp;&nbsp;Error: Not sent to user [#" + additionalMemberID + "] because the user record could not be found." + BR;
                    } else {
                        if (string.IsNullOrWhiteSpace(person.email)) {
                            confirmationMessage += "&nbsp;&nbsp;Error: Not sent to user [#" + additionalMemberID + "] because their email address was blank." + BR;
                        } else {
                            string EmailStatus = "";
                            string queryStringForLinkAppend = "";
                            queuePersonEmail(core, person, email.fromAddress, EmailSubjectSource, EmailBodySource, "", "", false, true, EmailRecordID, EmailTemplateSource, EmailAllowLinkEID, ref EmailStatus, queryStringForLinkAppend, "System Email");
                            confirmationMessage += "&nbsp;&nbsp;Sent to " + person.name + " at " + person.email + ", Status = " + EmailStatus + BR;
                        }
                    }
                }
                int emailConfirmationMemberId = email.testMemberID;
                //
                // --- Send the completion message to the administrator
                //
                if (emailConfirmationMemberId != 0) {
                    PersonModel person = DbBaseModel.create<PersonModel>(core.cpParent, emailConfirmationMemberId);
                    if (person != null) {
                        string ConfirmBody = "<div style=\"padding:10px;\">" + BR;
                        ConfirmBody += "The follow System Email was sent." + BR;
                        ConfirmBody += "" + BR;
                        ConfirmBody += "If this email includes personalization, each email sent was personalized to it's recipient. This confirmation has been personalized to you." + BR;
                        ConfirmBody += "" + BR;
                        ConfirmBody += "Subject: " + EmailSubjectSource + BR;
                        ConfirmBody += "From: " + email.fromAddress + BR;
                        ConfirmBody += "Bounces return to: " + BounceAddress + BR;
                        ConfirmBody += "Body:" + BR;
                        ConfirmBody += "<div style=\"clear:all\">----------------------------------------------------------------------</div>" + BR;
                        ConfirmBody += EmailBodySource + BR;
                        ConfirmBody += "<div style=\"clear:all\">----------------------------------------------------------------------</div>" + BR;
                        ConfirmBody += "--- recipient list ---" + BR;
                        ConfirmBody += confirmationMessage + BR;
                        ConfirmBody += "--- end of list ---" + BR;
                        ConfirmBody += "</div>";
                        //
                        string EmailStatus = "";
                        string queryStringForLinkAppend = "";
                        queuePersonEmail(core, person, email.fromAddress, "System Email confirmation from " + core.appConfig.domainList[0], ConfirmBody, "", "", false, true, EmailRecordID, "", false, ref EmailStatus, queryStringForLinkAppend, "System Email");
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return true;
        }
        //
        public static bool queueSystemEmail(CoreController core, SystemEmailModel email, string appendedCopy, int additionalMemberID) {
            string userErrorMessage = "";
            return queueSystemEmail(core, email, appendedCopy, additionalMemberID, ref userErrorMessage);
        }
        //
        public static bool queueSystemEmail(CoreController core, SystemEmailModel email, string appendedCopy) {
            string userErrorMessage = "";
            return queueSystemEmail(core, email, appendedCopy, 0, ref userErrorMessage);
        }
        //
        public static bool queueSystemEmail(CoreController core, SystemEmailModel email) {
            string userErrorMessage = "";
            return queueSystemEmail(core, email, "", 0, ref userErrorMessage);
        }
        //
        //====================================================================================================
        /// <summary>
        /// send the confirmation email as a test. 
        /// </summary>
        /// <param name="EmailID"></param>
        /// <param name="ConfirmationMemberID"></param>
        public static void queueConfirmationTestEmail(CoreController core, int EmailID, int ConfirmationMemberID) {
            try {
                //
                using (var csData = new CsModel(core)) {
                    csData.openRecord("email", EmailID);
                    if (!csData.ok()) {
                        //
                        // -- email not found
                        ErrorController.addUserError(core, "There was a problem sending the email confirmation. The email record could not be found.");
                        return;
                    }
                    //
                    // merge in template
                    string EmailTemplate = "";
                    int EMailTemplateID = csData.getInteger("EmailTemplateID");
                    if (EMailTemplateID != 0) {
                        using (var CSTemplate = new CsModel(core)) {
                            CSTemplate.openRecord("Email Templates", EMailTemplateID, "BodyHTML");
                            if (csData.ok()) {
                                EmailTemplate = CSTemplate.getText("BodyHTML");
                            }
                        }
                    }
                    //
                    // spam footer
                    string EmailBody = csData.getText("copyFilename");
                    if (csData.getBoolean("AllowSpamFooter")) {
                        //
                        // AllowSpamFooter is default true, and non-authorable
                        // It will be true in all cases, except a possible unforseen exception
                        //
                        EmailBody = EmailBody + "<div style=\"clear:both;padding:10px;\">" + GenericController.csv_GetLinkedText("<a href=\"" + HtmlController.encodeHtml(core.webServer.requestProtocol + core.webServer.requestDomain + "/" + core.siteProperties.serverPageDefault + "?" + rnEmailBlockRecipientEmail + "=#member_email#") + "\">", core.siteProperties.getText("EmailSpamFooter", DefaultSpamFooter)) + "</div>";
                        EmailBody = GenericController.vbReplace(EmailBody, "#member_email#", "UserEmailAddress");
                    }
                    //
                    // Confirm footer
                    //
                    int TotalCnt = 0;
                    int BlankCnt = 0;
                    int DupCnt = 0;
                    string DupList = "";
                    int BadCnt = 0;
                    string BadList = "";
                    string TotalList = "";
                    int contentControlId = csData.getInteger("contentControlId");
                    bool isGroupEmail = contentControlId.Equals(ContentMetadataModel.getContentId(core, "Group Email"));
                    var personIdList = PersonModel.createidListForEmail(core.cpParent, EmailID);
                    if (isGroupEmail && personIdList.Count.Equals(0)) {
                        ErrorController.addUserError(core, "There are no valid recipients of this email other than the confirmation address. Either no groups or topics were selected, or those selections contain no people with both a valid email addresses and 'Allow Group Email' enabled.");
                    } else {
                        foreach (var personId in personIdList) {
                            var person = DbBaseModel.create<PersonModel>(core.cpParent, personId);
                            string Emailtext = person.email;
                            string EMailName = person.name;
                            int EmailMemberID = person.id;
                            if (string.IsNullOrEmpty(EMailName)) {
                                EMailName = "no name (member id " + EmailMemberID + ")";
                            }
                            string EmailLine = Emailtext + " for " + EMailName;
                            string LastEmail = null;
                            if (string.IsNullOrEmpty(Emailtext)) {
                                BlankCnt = BlankCnt + 1;
                            } else {
                                if (Emailtext == LastEmail) {
                                    DupCnt = DupCnt + 1;
                                    string LastDupEmail = "";
                                    if (Emailtext != LastDupEmail) {
                                        DupList = DupList + "<div class=i>" + Emailtext + "</div>" + BR;
                                        LastDupEmail = Emailtext;
                                    }
                                }
                            }
                            int EmailLen = Emailtext.Length;
                            int Posat = GenericController.vbInstr(1, Emailtext, "@");
                            int PosDot = Emailtext.LastIndexOf(".") + 1;
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
                    string ConfirmFooter = "";
                    //
                    if (DupCnt == 1) {
                        ErrorController.addUserError(core, "There is 1 duplicate email address. See the test email for details.");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There is 1 duplicate email address. Only one email will be sent to each address. If the email includes personalization, or if you are using link authentication to automatically log in the user, you may want to correct duplicates to be sure the email is created correctly.<div style=\"margin:20px;\">" + DupList + "</div></div>";
                    } else if (DupCnt > 1) {
                        ErrorController.addUserError(core, "There are " + DupCnt + " duplicate email addresses. See the test email for details");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There are " + DupCnt + " duplicate email addresses. Only one email will be sent to each address. If the email includes personalization, or if you are using link authentication to automatically log in the user, you may want to correct duplicates to be sure the email is created correctly.<div style=\"margin:20px;\">" + DupList + "</div></div>";
                    }
                    //
                    if (BadCnt == 1) {
                        ErrorController.addUserError(core, "There is 1 invalid email address. See the test email for details.");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There is 1 invalid email address<div style=\"margin:20px;\">" + BadList + "</div></div>";
                    } else if (BadCnt > 1) {
                        ErrorController.addUserError(core, "There are " + BadCnt + " invalid email addresses. See the test email for details");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There are " + BadCnt + " invalid email addresses<div style=\"margin:20px;\">" + BadList + "</div></div>";
                    }
                    //
                    if (BlankCnt == 1) {
                        ErrorController.addUserError(core, "There is 1 blank email address. See the test email for details");
                        ConfirmFooter = ConfirmFooter + "<div style=\"clear:all\">WARNING: There is 1 blank email address.</div>";
                    } else if (BlankCnt > 1) {
                        ErrorController.addUserError(core, "There are " + DupCnt + " blank email addresses. See the test email for details.");
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
                        ErrorController.addUserError(core, "No confirmation email was send because a Confirmation member is not selected");
                    } else {
                        PersonModel person = DbBaseModel.create<PersonModel>(core.cpParent, ConfirmationMemberID);
                        if (person == null) {
                            ErrorController.addUserError(core, "No confirmation email was send because a Confirmation member is not selected");
                        } else {
                            EmailBody = EmailBody + "<div style=\"clear:both;padding:10px;margin:10px;border:1px dashed #888;\">Administrator<br><br>" + ConfirmFooter + "</div>";
                            string queryStringForLinkAppend = "";
                            string sendStatus = "";
                            if (!queuePersonEmail(core, person, csData.getText("FromAddress"), csData.getText("Subject"), EmailBody, "", "", true, true, EmailID, EmailTemplate, false, ref sendStatus, queryStringForLinkAppend, "Test Email")) {
                                ErrorController.addUserError(core, sendStatus);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Send all doc.propoerties in an email. This could represent form submissions
        /// </summary>
        /// <param name="core"></param>
        /// <param name="toAddress"></param>
        /// <param name="fromAddress"></param>
        /// <param name="emailSubject"></param>
        public static bool queueFormEmail(CoreController core, string toAddress, string fromAddress, string emailSubject, ref string userErrorMessage) {
            try {
                string Message = "";
                if ((toAddress.IndexOf("@") == -1)) {
                    toAddress = core.siteProperties.getText("TrapEmail");
                    emailSubject = "EmailForm with bad to-address";
                    Message = "Subject: " + emailSubject;
                    Message += Environment.NewLine;
                }
                Message += "The form was submitted " + core.doc.profileStartTime + Environment.NewLine;
                Message += Environment.NewLine;
                Message += "All text fields are included, completed or not.\r\n";
                Message += "Only those checkboxes that are checked are included.\r\n";
                Message += "Entries are not in the order they appeared on the form.\r\n";
                Message += Environment.NewLine;
                foreach (string key in core.docProperties.getKeyList()) {
                    var tempVar = core.docProperties.getProperty(key);
                    if (tempVar.propertyType == DocPropertyController.DocPropertyTypesEnum.form) {
                        if (GenericController.vbUCase(tempVar.Value) == "ON") {
                            Message += tempVar.Name + ": Yes\r\n\r\n";
                        } else {
                            Message += tempVar.Name + ": " + tempVar.Value + Environment.NewLine + "\r\n";
                        }
                    }
                }
                return queueAdHocEmail(core, "Form Submission Email", core.session.user.id, toAddress, fromAddress, emailSubject, Message, "", "", "", false, false, 0, ref userErrorMessage);
            } catch (Exception ex) {
                LogController.logError(core, ex);
                userErrorMessage += " The form could not be delivered due to an unknown error.";
                return false;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// send an email to a group of people, each customized
        /// </summary>
        /// <param name="core"></param>
        /// <param name="groupCommaList"></param>
        /// <param name="fromAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isImmediate"></param>
        /// <param name="isHtml"></param>
        public static bool queueGroupEmail(CoreController core, string groupCommaList, string fromAddress, string subject, string body, bool isImmediate, bool isHtml, ref string userErrorMessage) {
            try {
                if (string.IsNullOrWhiteSpace(groupCommaList)) { return true; }
                return queueGroupEmail(core, groupCommaList.Split(',').ToList<string>().FindAll(t => !string.IsNullOrWhiteSpace(t)), fromAddress, subject, body, isImmediate, isHtml, ref userErrorMessage);
            } catch (Exception ex) {
                LogController.logError(core, ex);
                userErrorMessage = "There was an unknown error sending the email;";
                return false;
            }
        }
        //
        public static bool queueGroupEmail(CoreController core, string groupCommaList, string fromAddress, string subject, string body, bool isImmediate, bool isHtml) {
            string userErrorMessage = "";
            return queueGroupEmail(core, groupCommaList, fromAddress, subject, body, isImmediate, isHtml, ref userErrorMessage);
        }
        //
        public static bool queueGroupEmail(CoreController core, string groupCommaList, string fromAddress, string subject, string body, bool isImmediate) {
            string userErrorMessage = "";
            return queueGroupEmail(core, groupCommaList, fromAddress, subject, body, isImmediate, true, ref userErrorMessage);
        }
        //
        public static bool queueGroupEmail(CoreController core, string groupCommaList, string fromAddress, string subject, string body) {
            string userErrorMessage = "";
            return queueGroupEmail(core, groupCommaList, fromAddress, subject, body, true, true, ref userErrorMessage);
        }
        //
        //====================================================================================================
        //
        public static bool queueGroupEmail(CoreController core, List<string> groupNameList, string fromAddress, string subject, string body, bool isImmediate, bool isHtml, ref string userErrorMessage) {
            try {
                if (groupNameList.Count <= 0) { return true; }
                foreach (var person in PersonModel.createListFromGroupNameList(core.cpParent, groupNameList, true)) {
                    queuePersonEmail(core, "Group Email", person, fromAddress, subject, body, "", "", isImmediate, isHtml, 0, "", false);
                }
                return true;
            } catch (Exception ex) {
                LogController.logError(core, ex);
                userErrorMessage = "There was an unknown error sending the email;";
                return false;
            }
        }
        //
        public static bool queueGroupEmail(CoreController core, List<string> groupNameList, string fromAddress, string subject, string body, bool isImmediate, bool isHtml) {
            string userErrorMessage = "";
            return queueGroupEmail(core, groupNameList, fromAddress, subject, body, isImmediate, isHtml, ref userErrorMessage);
        }
        //
        public static bool queueGroupEmail(CoreController core, List<string> groupNameList, string fromAddress, string subject, string body, bool isImmediate) {
            string userErrorMessage = "";
            return queueGroupEmail(core, groupNameList, fromAddress, subject, body, isImmediate, true, ref userErrorMessage);
        }
        //
        public static bool queueGroupEmail(CoreController core, List<string> groupNameList, string fromAddress, string subject, string body) {
            string userErrorMessage = "";
            return queueGroupEmail(core, groupNameList, fromAddress, subject, body, true, true, ref userErrorMessage);
        }
        //
        //====================================================================================================
        //
        public static bool queueGroupEmail(CoreController core, List<int> groupIdList, string fromAddress, string subject, string body, bool isImmediate, bool isHtml, ref string userErrorMessage) {
            try {
                if (groupIdList.Count <= 0) { return true; }
                foreach (var person in PersonModel.createListFromGroupIdList(core.cpParent, groupIdList, true)) {
                    queuePersonEmail(core, "Group Email", person, fromAddress, subject, body, "", "", isImmediate, isHtml, 0, "", false);
                }
                return true;
            } catch (Exception ex) {
                LogController.logError(core, ex);
                userErrorMessage = "There was an unknown error sending the email;";
                return false;
            }
        }
        //
        public static bool queueGroupEmail(CoreController core, List<int> groupIdList, string fromAddress, string subject, string body, bool isImmediate, bool isHtml) {
            string userErrorMessage = "";
            return queueGroupEmail(core, groupIdList, fromAddress, subject, body, isImmediate, isHtml, ref userErrorMessage);
        }
        //
        public static bool queueGroupEmail(CoreController core, List<int> groupIdList, string fromAddress, string subject, string body, bool isImmediate) {
            string userErrorMessage = "";
            return queueGroupEmail(core, groupIdList, fromAddress, subject, body, isImmediate, true, ref userErrorMessage);
        }
        //
        public static bool queueGroupEmail(CoreController core, List<int> groupIdList, string fromAddress, string subject, string body) {
            string userErrorMessage = "";
            return queueGroupEmail(core, groupIdList, fromAddress, subject, body, true, true, ref userErrorMessage);
        }
        //
        //====================================================================================================
        /// <summary>
        /// add email to the email queue
        /// </summary>
        /// <param name="core"></param>
        /// <param name="immediate"></param>
        /// <param name="email"></param>
        /// <param name="emailContextMessage">A short description of the email (Conditional Email, Group Email, Confirmation for Group Email, etc.)</param>
        private static void queueEmail(CoreController core, bool immediate, string emailContextMessage, EmailClass email) {
            try {
                var emailQueue = EmailQueueModel.addEmpty<EmailQueueModel>(core.cpParent);
                emailQueue.name = emailContextMessage;
                emailQueue.immediate = immediate;
                emailQueue.toAddress = email.toAddress;
                emailQueue.subject = email.subject;
                emailQueue.content = SerializeObject(email);
                emailQueue.attempts = email.attempts;
                emailQueue.save(core.cpParent);
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Send the emails in the current Queue
        /// </summary>
        public static void sendEmailInQueue(CoreController core) {
            try {
                List<EmailQueueModel> queue = EmailQueueModel.createList<EmailQueueModel>(core.cpParent, "", "immediate,id desc");
                bool sendWithSES = core.siteProperties.getBoolean(Constants.spSendEmailWithAmazonSES);
                string awsSecretAccessKey = core.siteProperties.getText(Constants.spAwsSecretAccessKey);
                string awsAccessKeyId = core.siteProperties.getText(Constants.spAwsAccessKeyId);
                foreach (EmailQueueModel queueRecord in queue) {
                    EmailQueueModel.delete<EmailQueueModel>(core.cpParent, queueRecord.id);
                    bool sendSuccess = false;
                    EmailClass email = DeserializeObject<EmailClass>(queueRecord.content);
                    string reasonForFail = "";
                    if (sendWithSES) {
                        //
                        // -- send with Amazon SES
                        sendSuccess = EmailAmazonSESController.send(core, email, ref reasonForFail, awsAccessKeyId, awsSecretAccessKey);
                    } else {
                        //
                        // --fall back to SMTP
                        sendSuccess = EmailSmtpController.send(core, email, ref reasonForFail);
                    }

                    if (sendSuccess) {
                        //
                        // -- success, log the send
                        var log = EmailLogModel.addEmpty<EmailLogModel>(core.cpParent);
                        log.name = "Successfully sent: " + queueRecord.name;
                        log.toAddress = email.toAddress;
                        log.fromAddress = email.fromAddress;
                        log.subject = email.subject;
                        log.body = email.htmlBody;
                        log.sendStatus = "ok";
                        log.logType = EmailLogTypeImmediateSend;
                        log.emailID = email.emailId;
                        log.memberID = email.toMemberId;
                        log.save(core.cpParent);
                        LogController.logInfo(core, "sendEmailInQueue, send successful, toAddress [" + email.toAddress + "], fromAddress [" + email.fromAddress + "], subject [" + email.subject + "]");
                    } else {
                        //
                        // -- fail, retry
                        if (email.attempts >= 3) {
                            //
                            // -- too many retries, log error
                            var log = EmailLogModel.addEmpty<EmailLogModel>(core.cpParent);
                            log.name = "Aborting unsuccessful send: " + queueRecord.name;
                            log.toAddress = email.toAddress;
                            log.fromAddress = email.fromAddress;
                            log.subject = email.subject;
                            log.body = email.htmlBody;
                            log.sendStatus = "failed after 3 retries, reason [" + reasonForFail + "]";
                            log.logType = EmailLogTypeImmediateSend;
                            log.emailID = email.emailId;
                            log.memberID = email.toMemberId;
                            log.save(core.cpParent);
                            LogController.logInfo(core, "sendEmailInQueue, send FAILED [" + reasonForFail + "], NOT resent because too many retries, toAddress [" + email.toAddress + "], fromAddress [" + email.fromAddress + "], subject [" + email.subject + "], attempts [" + email.attempts + "]");
                        } else {
                            //
                            // -- fail, add back to end of queue for retry
                            string sendStatus = "Retrying unsuccessful send (" + email.attempts.ToString() + " of 3), reason [" + reasonForFail + "]";
                            sendStatus = sendStatus.Substring(0, (sendStatus.Length > 254) ? 254 : sendStatus.Length);
                            email.attempts += 1;
                            var log = EmailLogModel.addEmpty<EmailLogModel>(core.cpParent);
                            log.name = "Failed send queued for retry: " + queueRecord.name;
                            log.toAddress = email.toAddress;
                            log.fromAddress = email.fromAddress;
                            log.subject = email.subject;
                            log.body = email.htmlBody;
                            log.sendStatus = sendStatus;
                            log.logType = EmailLogTypeImmediateSend;
                            log.emailID = email.emailId;
                            log.memberID = email.toMemberId;
                            log.save(core.cpParent);
                            queueEmail(core, false, queueRecord.name, email);
                            LogController.logInfo(core, "sendEmailInQueue, failed attempt (" + email.attempts.ToString() + " of 3), reason [" + reasonForFail + "], added to end of queue, toAddress [" + email.toAddress + "], fromAddress [" + email.fromAddress + "], subject [" + email.subject + "], attempts [" + email.attempts + "]");
                        }
                    }
                }
                return;
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
        //
        //====================================================================================================        
        /// <summary>
        /// create a simple text version of the email
        /// </summary>
        /// <param name="core"></param>
        /// <param name="isHTML"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static string encodeEmailTextBody(CoreController core, bool isHTML, string body, PersonModel recipient) {
            int recipientId = ( recipient == null ) ? 0 : recipient.id;
            //
            // -- body
            if (!string.IsNullOrWhiteSpace(body)) {
                body = ActiveContentController.renderHtmlForEmail(core, body, recipientId, "");
            }
            //
            if (!isHTML) {
                return body;
            } else if (body.ToLower().IndexOf("<html") >= 0) {
                //
                // -- isHtml, if the body includes an html tag, this is the entire body, just send it
                return NUglify.Uglify.HtmlToText("<body>" + body + "</body>").Code.Trim();
            } else {
                //
                // -- isHtml but no body tag, add an html wrapper
                return NUglify.Uglify.HtmlToText("<body>" + body + "</body>").Code.Trim();
            }
        }
        //
        //====================================================================================================        
        /// <summary>
        /// create the final html document to be sent in the email body
        /// </summary>
        /// <param name="core"></param>
        /// <param name="isHTML"></param>
        /// <param name="body"></param>
        /// <param name="template"></param>
        /// <param name="subject"></param>
        /// <param name="recipient"></param>
        /// <param name="queryStringForLinkAppend"></param>
        /// <returns></returns>
        public static string encodeEmailHtmlBody(CoreController core, bool isHTML, string body, string template, string subject, PersonModel recipient, string queryStringForLinkAppend) {
            int recipientId = (recipient == null) ? 0 : recipient.id;
            string recipientEmail = (recipient == null) ? "" : recipient.email;
            //
            // -- hotfix - move template merge before link conversion to update template links also
            string rootUrl = "http://" + core.appConfig.domainList[0] + "/";
            //
            // -- subject
            if (!string.IsNullOrWhiteSpace(subject)) {
                subject = ActiveContentController.renderHtmlForEmail(core, subject, recipientId, queryStringForLinkAppend);
                subject = GenericController.convertLinksToAbsolute(subject, rootUrl);
                subject = NUglify.Uglify.HtmlToText("<body>" + subject + "</body>").Code;
                if ( subject == null ) { subject = string.Empty; }
                subject = subject.Trim();
            }
            //
            // -- body
            if (!string.IsNullOrWhiteSpace(body)) {
                body = ActiveContentController.renderHtmlForEmail(core, body, recipientId, queryStringForLinkAppend);
                body = GenericController.convertLinksToAbsolute(body, rootUrl);
                body = GenericController.vbReplace(body, "#member_id#", recipientId.ToString());
                body = GenericController.vbReplace(body, "#member_email#", recipientEmail);
            }
            //
            // -- encode and merge template
            if(!string.IsNullOrWhiteSpace(template)) {
                //
                // hotfix - templates no longer have wysiwyg editors, so content may not be saved correctly - preprocess to convert wysiwyg content
                template = ActiveContentController.processWysiwygResponseForSave(core, template);
                //
                template = ActiveContentController.renderHtmlForEmail(core, template, recipientId, queryStringForLinkAppend);
                if (template.IndexOf(fpoContentBox) != -1) {
                    body = GenericController.vbReplace(template, fpoContentBox, body);
                } else {
                    body = template + body;
                }
            }
            if (!isHTML) {
                //
                // -- non html email, return a text version of the finished document
                return core.html.convertTextToHtml(body);
            }
            if (body.ToLower().IndexOf("<html") >= 0) {
                //
                // -- isHtml and the document includes an html tag -- return as-is
                return body;
            }
            //
            // -- html without an html tag. wrap it
            return "<html>"
                + "<head>"
                + "<Title>" + subject + "</Title>"
                + "<Base href=\"" + rootUrl + "\" >"
                + "</head>"
                + "<body class=\"ccBodyEmail\">" + body + "</body>"
                + "</html>";
        }
        //
        //====================================================================================================        
        /// <summary>
        /// object for email queue serialization
        /// </summary>
        public class EmailClass {
            public int toMemberId;
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
    }
}