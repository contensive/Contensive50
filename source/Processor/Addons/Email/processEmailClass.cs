﻿
using System;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using Contensive.BaseClasses;
using Contensive.Processor.Exceptions;
//
namespace Contensive.Addons.Email {
    public class ProcessEmailClass : AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// addon interface
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(CPBaseClass cp) {
            string result = "";
            try {
                //
                // -- ok to cast cpbase to cp because they build from the same solution
                CoreController core = ((CPClass)cp).core;
                //
                // Send Submitted Group Email (submitted, not sent, no conditions)
                ProcessGroupEmail(core);
                //
                // Send Conditional Email - Offset days after Joining
                ProcessConditionalEmail(core);
                //
                // -- send queue
                EmailController.sendEmailInQueue(core);
                //
                core.siteProperties.setProperty("EmailServiceLastCheck", encodeText(DateTime.Now));
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// process group email, adding each to the email queue
        /// </summary>
        /// <param name="core"></param>
        private void ProcessGroupEmail(CoreController core) {
            try {
                //
                // Open the email records
                string SQLDateNow = DbController.encodeSQLDate(DateTime.Now);
                string Criteria = "(ccemail.active<>0)"
                    + " and ((ccemail.Sent is null)or(ccemail.Sent=0))"
                    + " and (ccemail.submitted<>0)"
                    + " and ((ccemail.scheduledate is null)or(ccemail.scheduledate<" + SQLDateNow + "))"
                    + " and ((ccemail.ConditionID is null)OR(ccemail.ConditionID=0))"
                    + "";
                using (var CSEmail = new CsModel(core)) {
                    CSEmail.open("Email", Criteria);
                    if (CSEmail.ok()) {
                        string SQLTablePeople = MetadataController.getContentTablename(core, "People");
                        string SQLTableMemberRules = MetadataController.getContentTablename(core, "Member Rules");
                        string SQLTableGroups = MetadataController.getContentTablename(core, "Groups");
                        string BounceAddress = core.siteProperties.getText("EmailBounceAddress", "");
                        string PrimaryLink = "http://" + core.appConfig.domainList[0];
                        while (CSEmail.ok()) {
                            int emailID = CSEmail.getInteger("ID");
                            int EmailMemberID = CSEmail.getInteger("ModifiedBy");
                            int EmailTemplateID = CSEmail.getInteger("EmailTemplateID");
                            string EmailTemplate = getEmailTemplate(core, EmailTemplateID);
                            bool EmailAddLinkEID = CSEmail.getBoolean("AddLinkEID");
                            string EmailFrom = CSEmail.getText("FromAddress");
                            string EmailSubject = CSEmail.getText("Subject");
                            //
                            // Mark this email sent and go to the next
                            CSEmail.set("sent", true);
                            CSEmail.save();
                            //
                            // Create Drop Record
                            int EmailDropID = 0;
                            using (var csDrop = new CsModel(core)) {
                                csDrop.insert("Email Drops");
                                if (csDrop.ok()) {
                                    EmailDropID = csDrop.getInteger("ID");
                                    DateTime ScheduleDate = csDrop.getDate("ScheduleDate");
                                    if (ScheduleDate < DateTime.Parse("1/1/2000")) {
                                        ScheduleDate = DateTime.Parse("1/1/2000");
                                    }
                                    csDrop.set("Name", "Drop " + EmailDropID + " - Scheduled for " + ScheduleDate.ToString("") + " " + ScheduleDate.ToString(""));
                                    csDrop.set("EmailID", emailID);
                                }
                                csDrop.close();
                            }
                            string EmailStatusList = "";
                            using (var csPerson = new CsModel(core)) {
                                //
                                // Select all people in the groups for this email
                                string SQL = "select Distinct ccMembers.ID as MemberID,ccMembers.email"
                                    + " From ((((ccemail"
                                    + " left join ccEmailGroups on ccEmailGroups.EmailID=ccEmail.ID)"
                                    + " left join ccGroups on ccGroups.ID = ccEmailGroups.GroupID)"
                                    + " left join ccMemberRules on ccGroups.ID = ccMemberRules.GroupID)"
                                    + " left join ccMembers on ccMembers.ID = ccMemberRules.MemberID)"
                                    + " Where (ccEmail.ID=" + emailID + ")"
                                    + " and (ccGroups.active<>0)"
                                    + " and (ccGroups.AllowBulkEmail<>0)"
                                    + " and (ccMembers.active<>0)"
                                    + " and (ccMembers.AllowBulkEmail<>0)"
                                    + " and (ccMembers.email<>'')"
                                    + " and ((ccMemberRules.DateExpires is null)or(ccMemberRules.DateExpires>" + SQLDateNow + "))"
                                    + " order by ccMembers.email,ccMembers.id";
                                csPerson.openSql(SQL, "Default");
                                //
                                // Send the email to all selected people
                                //
                                string LastEmail = null;
                                LastEmail = "empty";
                                while (csPerson.ok()) {
                                    int PeopleID = csPerson.getInteger("MemberID");
                                    string Email = csPerson.getText("Email");
                                    if (Email == LastEmail) {
                                        string PeopleName = MetadataController.getRecordName(core, "people", PeopleID);
                                        if (string.IsNullOrEmpty(PeopleName)) {
                                            PeopleName = "user #" + PeopleID;
                                        }
                                        EmailStatusList = EmailStatusList + "Not Sent to " + PeopleName + ", duplicate email address (" + Email + ")" + BR;
                                    } else {
                                        EmailStatusList = EmailStatusList + queueEmailRecord(core, PeopleID, emailID, DateTime.MinValue, EmailDropID, BounceAddress, EmailFrom, EmailTemplate, EmailFrom, EmailSubject, csPerson.getText("CopyFilename"), csPerson.getBoolean("AllowSpamFooter"), csPerson.getBoolean("AddLinkEID"), "") + BR;
                                    }
                                    LastEmail = Email;
                                    csPerson.goNext();
                                }
                                csPerson.close();
                            }
                            //
                            // Send the confirmation
                            //
                            string EmailCopy = CSEmail.getText("copyfilename");
                            int ConfirmationMemberID = CSEmail.getInteger("testmemberid");
                            queueConfirmationEmail(core, ConfirmationMemberID, EmailDropID, EmailTemplate, EmailAddLinkEID, PrimaryLink, EmailSubject, EmailCopy, "", EmailFrom, EmailStatusList);
                            CSEmail.goNext();
                        }
                    }
                    CSEmail.close();
                }
                return;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw (new GenericException("Unexpected exception"));
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// process conditional email, adding each to the email queue
        /// </summary>
        /// <param name="core"></param>
        /// <param name="IsNewHour"></param>
        /// <param name="IsNewDay"></param>
        private void ProcessConditionalEmail(CoreController core) {
            try {
                int dataSourceType = core.db.getDataSourceType();
                string BounceAddress = core.siteProperties.getText("EmailBounceAddress", "");
                DateTime rightNow = DateTime.Now;
                DateTime rightNowDate = rightNow.Date;
                string SQLDateNow = DbController.encodeSQLDate(DateTime.Now);
                //
                // Send Conditional Email - Offset days after Joining
                //   sends email between the condition period date and date +1. if a conditional email is setup and there are already
                //   peope in the group, they do not get the email if they are past the one day threshhold.
                //   To keep them from only getting one, the log is used for the one day.
                //   Housekeep logs far > 1 day
                //
                using (var csEmailList = new CsModel(core)) {
                    string FieldList = "ccEmail.TestMemberID AS TestMemberID,ccEmail.ID as EmailID,ccMembers.ID AS MemberID, ccMemberRules.DateExpires AS DateExpires,ccEmail.BlockSiteStyles,ccEmail.stylesFilename";
                    string SQL = "SELECT Distinct " + FieldList + " FROM ((((ccEmail"
                        + " LEFT JOIN ccEmailGroups ON ccEmail.ID = ccEmailGroups.EmailID)"
                        + " LEFT JOIN ccGroups ON ccEmailGroups.GroupID = ccGroups.ID)"
                        + " LEFT JOIN ccMemberRules ON ccGroups.ID = ccMemberRules.GroupID)"
                        + " LEFT JOIN ccMembers ON ccMemberRules.MemberID = ccMembers.ID)"
                        + " Where (ccEmail.id Is Not Null)"
                        + " and(DATEADD(day, ccEmail.ConditionPeriod, ccMemberRules.dateAdded) < " + SQLDateNow + ")" // dont send before
                        + " and(DATEADD(day, ccEmail.ConditionPeriod+1.0, ccMemberRules.dateAdded) > " + SQLDateNow + ")" // don't send after 1-day
                        + " AND (ccEmail.ConditionExpireDate > " + SQLDateNow + " OR ccEmail.ConditionExpireDate IS NULL)"
                        + " AND (ccEmail.ScheduleDate < " + SQLDateNow + " OR ccEmail.ScheduleDate IS NULL)"
                        + " AND (ccEmail.Submitted <> 0)"
                        + " AND (ccEmail.ConditionID = 2)"
                        + " AND (ccEmail.ConditionPeriod IS NOT NULL)"
                        + " AND (ccGroups.Active <> 0)"
                        + " AND (ccGroups.AllowBulkEmail <> 0)"
                        + " AND (ccMembers.ID IS NOT NULL)"
                        + " AND (ccMembers.Active <> 0)"
                        + " AND (ccMembers.AllowBulkEmail <> 0)"
                        + " AND (ccEmail.ID Not In (Select ccEmailLog.EmailID from ccEmailLog where ccEmailLog.MemberID=ccMembers.ID))";
                    csEmailList.openSql(SQL, "Default");
                    while (csEmailList.ok()) {
                        int emailID = csEmailList.getInteger("EmailID");
                        int EmailMemberID = csEmailList.getInteger("MemberID");
                        DateTime EmailDateExpires = csEmailList.getDate("DateExpires");
                        //
                        using (var csEmail = new CsModel(core)) {
                            csEmail.openRecord("Conditional Email", emailID);
                            if (csEmail.ok()) {
                                int EmailTemplateID = csEmail.getInteger("EmailTemplateID");
                                string EmailTemplate = getEmailTemplate(core, EmailTemplateID);
                                string FromAddress = csEmail.getText("FromAddress");
                                int ConfirmationMemberID = csEmail.getInteger("testmemberid");
                                bool EmailAddLinkEID = csEmail.getBoolean("AddLinkEID");
                                string EmailSubject = csEmail.getText("Subject");
                                string EmailCopy = csEmail.getText("CopyFilename");
                                string EmailStatus = queueEmailRecord(core, EmailMemberID, emailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, EmailSubject, EmailCopy, csEmail.getBoolean("AllowSpamFooter"), EmailAddLinkEID, "");
                                queueConfirmationEmail(core, ConfirmationMemberID, 0, EmailTemplate, EmailAddLinkEID, "", EmailSubject, EmailCopy, "", FromAddress, EmailStatus + "<BR>");
                            }
                            csEmail.close();
                        }
                        //
                        csEmailList.goNext();
                    }
                    csEmailList.close();
                }
                //
                // Send Conditional Email - Offset days Before Expiration
                //
                {
                    using (var csList = new CsModel(core)) {
                        string FieldList = "ccEmail.TestMemberID AS TestMemberID,ccEmail.ID AS EmailID, ccMembers.ID AS MemberID, ccMemberRules.DateExpires AS DateExpires,ccEmail.BlockSiteStyles,ccEmail.stylesFilename";
                        string sqlDateTest = "";
                        if (dataSourceType == DataSourceTypeODBCSQLServer) {
                            sqlDateTest = ""
                                + " AND (CAST(ccMemberRules.DateExpires as datetime)-ccEmail.ConditionPeriod > " + SQLDateNow + ")"
                                + " AND (CAST(ccMemberRules.DateExpires as datetime)-ccEmail.ConditionPeriod-1.0 < " + SQLDateNow + ")"
                                + "";
                        } else {
                            sqlDateTest = ""
                                + " AND (ccMemberRules.DateExpires-ccEmail.ConditionPeriod > " + SQLDateNow + ")"
                                + " AND (ccMemberRules.DateExpires-ccEmail.ConditionPeriod-1.0 < " + SQLDateNow + ")"
                                + "";
                        }
                        string SQL = "SELECT DISTINCT " + FieldList + " FROM ((((ccEmail"
                            + " LEFT JOIN ccEmailGroups ON ccEmail.ID = ccEmailGroups.EmailID)"
                            + " LEFT JOIN ccGroups ON ccEmailGroups.GroupID = ccGroups.ID)"
                            + " LEFT JOIN ccMemberRules ON ccGroups.ID = ccMemberRules.GroupID)"
                            + " LEFT JOIN ccMembers ON ccMemberRules.MemberID = ccMembers.ID)"
                            + " Where (ccEmail.id Is Not Null)"
                            + " and(DATEADD(day, -ccEmail.ConditionPeriod, ccMemberRules.DateExpires) < " + SQLDateNow + ")" // dont send before
                            + " and(DATEADD(day, -ccEmail.ConditionPeriod-1.0, ccMemberRules.DateExpires) > " + SQLDateNow + ")" // don't send after 1-day
                            + " AND (ccEmail.ConditionExpireDate > " + SQLDateNow + " OR ccEmail.ConditionExpireDate IS NULL)"
                            + " AND (ccEmail.ScheduleDate < " + SQLDateNow + " OR ccEmail.ScheduleDate IS NULL)"
                            + " AND (ccEmail.Submitted <> 0)"
                            + " AND (ccEmail.ConditionID = 1)"
                            + " AND (ccEmail.ConditionPeriod IS NOT NULL)"
                            + " AND (ccGroups.Active <> 0)"
                            + " AND (ccGroups.AllowBulkEmail <> 0)"
                            + " AND (ccMembers.ID IS NOT NULL)"
                            + " AND (ccMembers.Active <> 0)"
                            + " AND (ccMembers.AllowBulkEmail <> 0)"
                            + " AND (ccEmail.ID Not In (Select ccEmailLog.EmailID from ccEmailLog where ccEmailLog.MemberID=ccMembers.ID))";
                        csList.openSql(SQL, "Default");
                        while (csList.ok()) {
                            int emailID = csList.getInteger("EmailID");
                            int EmailMemberID = csList.getInteger("MemberID");
                            DateTime EmailDateExpires = csList.getDate("DateExpires");
                            //
                            using (var csEmail = new CsModel(core)) {
                                csEmail.openRecord("Conditional Email", emailID);
                                if (csEmail.ok()) {
                                    int EmailTemplateID = csEmail.getInteger("EmailTemplateID");
                                    string EmailTemplate = getEmailTemplate(core, EmailTemplateID);
                                    string FromAddress = csEmail.getText("FromAddress");
                                    int ConfirmationMemberID = csEmail.getInteger("testmemberid");
                                    bool EmailAddLinkEID = csEmail.getBoolean("AddLinkEID");
                                    string EmailSubject = csEmail.getText("Subject");
                                    string EmailCopy = csEmail.getText("CopyFilename");
                                    string EmailStatus = queueEmailRecord(core, EmailMemberID, emailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, csEmail.getText("Subject"), csEmail.getText("CopyFilename"), csEmail.getBoolean("AllowSpamFooter"), csEmail.getBoolean("AddLinkEID"), "");
                                    queueConfirmationEmail(core, ConfirmationMemberID, 0, EmailTemplate, EmailAddLinkEID, "", EmailSubject, EmailCopy, "", FromAddress, EmailStatus + "<BR>");
                                }
                                csEmail.close();
                            }
                            //
                            csList.goNext();
                        }
                        csList.close();
                    }
                    //
                    // Send Conditional Email - Birthday
                    //
                    using (var CSEmailBig = new CsModel(core)) {
                        string FieldList = "ccEmail.TestMemberID AS TestMemberID,ccEmail.ID AS EmailID, ccMembers.ID AS MemberID, ccMemberRules.DateExpires AS DateExpires,ccEmail.BlockSiteStyles,ccEmail.stylesFilename";
                        string SQL = "SELECT DISTINCT " + FieldList + " FROM ((((ccEmail"
                            + " LEFT JOIN ccEmailGroups ON ccEmail.ID = ccEmailGroups.EmailID)"
                            + " LEFT JOIN ccGroups ON ccEmailGroups.GroupID = ccGroups.ID)"
                            + " LEFT JOIN ccMemberRules ON ccGroups.ID = ccMemberRules.GroupID)"
                            + " LEFT JOIN ccMembers ON ccMemberRules.MemberID = ccMembers.ID)"
                            + " Where (ccEmail.id Is Not Null)"
                            + " AND (ccEmail.ConditionExpireDate > " + SQLDateNow + " OR ccEmail.ConditionExpireDate IS NULL)"
                            + " AND (ccEmail.ScheduleDate < " + SQLDateNow + " OR ccEmail.ScheduleDate IS NULL)"
                            + " AND (ccEmail.Submitted <> 0)"
                            + " AND (ccEmail.ConditionID = 3)"
                            + " AND (ccGroups.Active <> 0)"
                            + " AND (ccGroups.AllowBulkEmail <> 0)"
                            + " AND ((ccMemberRules.DateExpires is null)or(ccMemberRules.DateExpires > " + SQLDateNow + "))"
                            + " AND (ccMembers.ID IS NOT NULL)"
                            + " AND (ccMembers.Active <> 0)"
                            + " AND (ccMembers.AllowBulkEmail <> 0)"
                            + " AND (ccMembers.BirthdayMonth=" + DateTime.Now.Month + ")"
                            + " AND (ccMembers.BirthdayDay=" + DateTime.Now.Day + ")"
                            + " AND (ccEmail.ID Not In (Select ccEmailLog.EmailID from ccEmailLog where ccEmailLog.MemberID=ccMembers.ID and ccEmailLog.DateAdded>=" + DbController.encodeSQLDate(DateTime.Now.Date) + "))";
                        CSEmailBig.openSql(SQL, "Default");
                        while (CSEmailBig.ok()) {
                            int emailID = CSEmailBig.getInteger("EmailID");
                            int EmailMemberID = CSEmailBig.getInteger("MemberID");
                            DateTime EmailDateExpires = CSEmailBig.getDate( "DateExpires");
                            //
                            //

                            using (var CSEmail = new CsModel(core)) {
                                CSEmail.openRecord("Conditional Email", emailID);
                                if (CSEmail.ok()) {
                                    int EmailTemplateID = CSEmail.getInteger("EmailTemplateID");
                                    string EmailTemplate = getEmailTemplate(core, EmailTemplateID);
                                    string FromAddress = CSEmail.getText("FromAddress");
                                    int ConfirmationMemberID = CSEmail.getInteger("testmemberid");
                                    bool EmailAddLinkEID = CSEmail.getBoolean("AddLinkEID");
                                    string EmailSubject = CSEmail.getText("Subject");
                                    string EmailCopy = CSEmail.getText("CopyFilename");
                                    string EmailStatus = queueEmailRecord(core, EmailMemberID, emailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, CSEmail.getText("Subject"), CSEmail.getText("CopyFilename"), CSEmail.getBoolean("AllowSpamFooter"), CSEmail.getBoolean("AddLinkEID"), "");
                                    queueConfirmationEmail(core, ConfirmationMemberID, 0, EmailTemplate, EmailAddLinkEID, "", EmailSubject, EmailCopy, "", FromAddress, EmailStatus + "<BR>");
                                }
                            }
                            CSEmailBig.goNext();
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Send email to a memberid
        /// </summary>
        /// <param name="MemberID"></param>
        /// <param name="emailID"></param>
        /// <param name="DateBlockExpires"></param>
        /// <param name="emailDropID"></param>
        /// <param name="BounceAddress"></param>
        /// <param name="ReplyToAddress"></param>
        /// <param name="EmailTemplate"></param>
        /// <param name="FromAddress"></param>
        /// <param name="EmailSubject"></param>
        /// <param name="EmailBody"></param>
        /// <param name="AllowSpamFooter"></param>
        /// <param name="EmailAllowLinkEID"></param>
        /// <param name="emailStyles"></param>
        /// <returns>OK if successful, else returns user error.</returns>
        private string queueEmailRecord(CoreController core, int MemberID, int emailID, DateTime DateBlockExpires, int emailDropID, string BounceAddress, string ReplyToAddress, string EmailTemplate, string FromAddress, string EmailSubject, string EmailBody, bool AllowSpamFooter, bool EmailAllowLinkEID, string emailStyles) {
            string returnStatus = "";
            try {
                string defaultPage = null;
                string emailToName = null;
                string ClickFlagQuery = null;
                string EmailStatus = null;
                string emailWorkingStyles = null;
                string urlProtocolDomainSlash = null;
                string protocolHostLink = null;
                string emailToAddress = null;
                string EmailBodyEncoded = null;
                string EmailSubjectEncoded = null;
                string EmailTemplateEncoded = "";
                string openTriggerCode = "";
                //
                EmailBodyEncoded = EmailBody;
                EmailSubjectEncoded = EmailSubject;
                using (var CSLog = new CsModel(core)) {
                    CSLog.insert("Email Log");
                    if (CSLog.ok()) {
                        CSLog.set("Name", "Sent " + encodeText(DateTime.Now));
                        CSLog.set("EmailDropID", emailDropID);
                        CSLog.set("EmailID", emailID);
                        CSLog.set("MemberID", MemberID);
                        CSLog.set("LogType", EmailLogTypeDrop);
                        CSLog.set("DateBlockExpires", DateBlockExpires);
                        CSLog.set("SendStatus", "Send attempted but not completed");
                        if (true) {
                            CSLog.set("fromaddress", FromAddress);
                            CSLog.set("Subject", EmailSubject);
                        }
                        CSLog.save();
                        //
                        // Get the Template
                        //
                        protocolHostLink = "http://" + core.appConfig.domainList[0];
                        //
                        // Get the Member
                        //
                        using (var CSPeople = new CsModel(core)) {
                            CSPeople.openRecord("People", MemberID, "Email,Name");
                            if (CSPeople.ok()) {
                                emailToAddress = CSPeople.getText("Email");
                                emailToName = CSPeople.getText("Name");
                                defaultPage = core.siteProperties.serverPageDefault;
                                urlProtocolDomainSlash = protocolHostLink + "/";
                                if (emailDropID != 0) {
                                    switch (core.siteProperties.getInteger("GroupEmailOpenTriggerMethod", 0)) {
                                        case 1:
                                            openTriggerCode = "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + urlProtocolDomainSlash + defaultPage + "?" + rnEmailOpenCssFlag + "=" + emailDropID + "&" + rnEmailMemberID + "=#member_id#\">";
                                            break;
                                        default:
                                            openTriggerCode = "<img src=\"" + urlProtocolDomainSlash + defaultPage + "?" + rnEmailOpenFlag + "=" + emailDropID + "&" + rnEmailMemberID + "=#member_id#\">";
                                            break;
                                    }
                                }
                                //
                                emailWorkingStyles = emailStyles;
                                emailWorkingStyles = GenericController.vbReplace(emailWorkingStyles, StyleSheetStart, StyleSheetStart + "<!-- ", 1, 99, 1);
                                emailWorkingStyles = GenericController.vbReplace(emailWorkingStyles, StyleSheetEnd, " // -->" + StyleSheetEnd, 1, 99, 1);
                                //
                                // Create the clickflag to be added to all anchors
                                //
                                ClickFlagQuery = rnEmailClickFlag + "=" + emailDropID + "&" + rnEmailMemberID + "=" + MemberID;
                                //
                                // Encode body and subject
                                //
                                //EmailBodyEncoded = contentCmdController.executeContentCommands(core, EmailBodyEncoded, CPUtilsClass.addonContext.ContextEmail, MemberID, true, ref errorMessage);
                                EmailBodyEncoded = ActiveContentController.renderHtmlForEmail(core, EmailBodyEncoded, MemberID, ClickFlagQuery);
                                //EmailBodyEncoded = core.html.convertActiveContent_internal(EmailBodyEncoded, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, 0, "", CPUtilsClass.addonContext.ContextEmail, True, Nothing, False)
                                //EmailBodyEncoded = core.csv_EncodeContent8(Nothing, EmailBodyEncoded, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, "", 0, "", True, CPUtilsClass.addonContext.contextEmail)
                                //
                                //EmailSubjectEncoded = contentCmdController.executeContentCommands(core, EmailSubjectEncoded, CPUtilsClass.addonContext.ContextEmail, MemberID, true, ref errorMessage);
                                EmailSubjectEncoded = ActiveContentController.renderHtmlForEmail(core, EmailSubjectEncoded, MemberID, ClickFlagQuery);
                                //EmailSubjectEncoded = core.html.convertActiveContent_internal(EmailSubjectEncoded, MemberID, "", 0, 0, True, False, False, False, False, True, "", PrimaryLink, True, 0, "", CPUtilsClass.addonContext.ContextEmail, True, Nothing, False)
                                //EmailSubjectEncoded = core.csv_EncodeContent8(Nothing, EmailSubjectEncoded, MemberID, "", 0, 0, True, False, False, False, False, True, "", PrimaryLink, True, "", 0, "", True, CPUtilsClass.addonContext.contextEmail)
                                //
                                // Encode/Merge Template
                                //
                                if (string.IsNullOrEmpty(EmailTemplate)) {
                                    //
                                    // create 20px padding template
                                    //
                                    EmailBodyEncoded = "<div style=\"padding:10px;\">" + EmailBodyEncoded + "</div>";
                                } else {
                                    //
                                    // use provided template
                                    //
                                    // hotfix - templates no longer have wysiwyg editors, so content may not be saved correctly - preprocess to convert wysiwyg content
                                    EmailTemplate = ActiveContentController.processWysiwygResponseForSave(core, EmailTemplate);
                                    EmailTemplateEncoded = ActiveContentController.renderHtmlForEmail(core, EmailTemplate, MemberID, ClickFlagQuery);
                                    if (GenericController.vbInstr(1, EmailTemplateEncoded, fpoContentBox) != 0) {
                                        EmailBodyEncoded = GenericController.vbReplace(EmailTemplateEncoded, fpoContentBox, EmailBodyEncoded);
                                    } else {
                                        EmailBodyEncoded = EmailTemplateEncoded + "<div style=\"padding:10px;\">" + EmailBodyEncoded + "</div>";
                                    }
                                    //                If genericController.vbInstr(1, EmailTemplateEncoded, ContentPlaceHolder) <> 0 Then
                                    //                    EmailBodyEncoded = genericController.vbReplace(EmailTemplateEncoded, ContentPlaceHolder, EmailBodyEncoded)
                                    //                Else
                                    //                    EmailBodyEncoded = EmailTemplateEncoded & "<div style=""padding:10px;"">" & EmailBodyEncoded & "</div>"
                                    //                End If
                                }
                                //
                                // Spam Footer under template
                                // remove the marker for any other place in the email then add it as needed
                                //
                                EmailBodyEncoded = GenericController.vbReplace(EmailBodyEncoded, rnEmailBlockRecipientEmail, "", 1, 99, 1);
                                if (AllowSpamFooter) {
                                    //
                                    // non-authorable, default true - leave it as an option in case there is an important exception
                                    //
                                    EmailBodyEncoded = EmailBodyEncoded + "<div style=\"padding:10px;\">" + getLinkedText("<a href=\"" + urlProtocolDomainSlash + defaultPage + "?" + rnEmailBlockRecipientEmail + "=#member_email#&" + rnEmailBlockRequestDropID + "=" + emailDropID + "\">", core.siteProperties.getText("EmailSpamFooter", DefaultSpamFooter)) + "</div>";
                                }
                                //
                                // open trigger under footer (so it does not shake as the image comes in)
                                //
                                EmailBodyEncoded = EmailBodyEncoded + openTriggerCode;
                                EmailBodyEncoded = GenericController.vbReplace(EmailBodyEncoded, "#member_id#", MemberID);
                                EmailBodyEncoded = GenericController.vbReplace(EmailBodyEncoded, "#member_email#", emailToAddress);
                                //
                                // Now convert URLS to absolute
                                //
                                EmailBodyEncoded = convertLinksToAbsolute(EmailBodyEncoded, urlProtocolDomainSlash);
                                //
                                EmailBodyEncoded = ""
                                    + "<HTML>"
                                    + "<Head>"
                                    + "<Title>" + EmailSubjectEncoded + "</Title>"
                                    + "<Base href=\"" + urlProtocolDomainSlash + "\">"
                                    + "</Head>"
                                    + "<BODY class=ccBodyEmail>"
                                    + "<Base href=\"" + urlProtocolDomainSlash + "\">"
                                    + emailWorkingStyles + EmailBodyEncoded + "</BODY>"
                                    + "</HTML>";
                                //
                                // Send
                                //
                                EmailController.queueAdHocEmail(core, emailToAddress, FromAddress, EmailSubjectEncoded, EmailBodyEncoded, BounceAddress, ReplyToAddress, "", true, true, 0, ref EmailStatus);
                                if (string.IsNullOrEmpty(EmailStatus)) {
                                    EmailStatus = "ok";
                                }
                                returnStatus = returnStatus + "Send to " + emailToName + " at " + emailToAddress + ", Status = " + EmailStatus;
                                //
                                // ----- Log the send
                                //
                                CSLog.set("SendStatus", EmailStatus);
                                if (true) {
                                    CSLog.set("toaddress", emailToAddress);
                                }
                                CSLog.save();
                            }
                        }
                    }
                }
            } catch (Exception) {
                throw (new GenericException("Unexpected exception"));
            }

            return returnStatus;
        }
        //
        //====================================================================================================
        //
        private string getEmailTemplate(CoreController core, int EmailTemplateID) {
            var emailTemplate = Processor.Models.Db.EmailTemplateModel.create(core, EmailTemplateID);
            if (emailTemplate != null) {
                return emailTemplate.bodyHTML;
            }
            return "";
            //string tempGetEmailTemplate = "";
            //try {
            //    if (EmailTemplateID != 0) {
            //        csData.csOpenContentRecord("Email Templates", EmailTemplateID, 0, false, false, "BodyHTML");
            //        if (csData.csOk()) {
            //            tempGetEmailTemplate = csData.csGet("BodyHTML");
            //        }
            //        csData.csClose();
            //    }
            //} catch (Exception ex) {
            //    logController.handleException( core,ex);
            //    throw
            //}
            //return tempGetEmailTemplate;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Send confirmation email 
        /// </summary>
        /// <param name="ConfirmationMemberID"></param>
        /// <param name="EmailDropID"></param>
        /// <param name="EmailTemplate"></param>
        /// <param name="EmailAllowLinkEID"></param>
        /// <param name="PrimaryLink"></param>
        /// <param name="EmailSubject"></param>
        /// <param name="emailBody"></param>
        /// <param name="emailStyles"></param>
        /// <param name="EmailFrom"></param>
        /// <param name="EmailStatusList"></param>
        private void queueConfirmationEmail(CoreController core, int ConfirmationMemberID, int EmailDropID, string EmailTemplate, bool EmailAllowLinkEID, string PrimaryLink, string EmailSubject, string emailBody, string emailStyles, string EmailFrom, string EmailStatusList) {
            try {
                PersonModel person = PersonModel.create(core, ConfirmationMemberID);
                if (person != null) {
                    string ConfirmBody = ""
                        + "The follow email has been sent."
                        + BR
                        + BR + "If this email includes personalization, each email sent was personalized to its recipient. This confirmation has been personalized to you."
                        + BR
                        + BR + "Subject: " + EmailSubject
                        + BR + "From: "
                        + EmailFrom
                        + BR + "Body"
                        + BR + "----------------------------------------------------------------------"
                        + BR + emailBody
                        + BR + "----------------------------------------------------------------------"
                        + BR + "--- recipient list ---"
                        + BR + EmailStatusList
                        + "--- end of list ---"
                        + BR;
                    string queryStringForLinkAppend = rnEmailClickFlag + "=" + EmailDropID + "&" + rnEmailMemberID + "=" + person.id;
                    string sendStatus = "";
                    EmailController.queuePersonEmail(core, person, EmailFrom, "Email confirmation from " + core.appConfig.domainList[0], ConfirmBody, "", "", true, true, EmailDropID, EmailTemplate, EmailAllowLinkEID, ref sendStatus, queryStringForLinkAppend);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
    }
}