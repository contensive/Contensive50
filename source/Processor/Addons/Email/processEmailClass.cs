
using System;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
using Contensive.Processor.Models.Domain;
using Contensive.BaseClasses;
//
namespace Contensive.Addons.Email {
    public class ProcessEmailClass  : AddonBaseClass{
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
                string SQLDateNow = core.db.encodeSQLDate(DateTime.Now);
                string Criteria = "(ccemail.active<>0)"
                    + " and ((ccemail.Sent is null)or(ccemail.Sent=0))"
                    + " and (ccemail.submitted<>0)"
                    + " and ((ccemail.scheduledate is null)or(ccemail.scheduledate<" + SQLDateNow + "))"
                    + " and ((ccemail.ConditionID is null)OR(ccemail.ConditionID=0))"
                    + "";
                int CSEmail = core.db.csOpen("Email", Criteria);
                if (core.db.csOk(CSEmail)) {
                    string SQLTablePeople = CdefController.getContentTablename(core, "People");
                    string SQLTableMemberRules =  CdefController.getContentTablename(core, "Member Rules");
                    string SQLTableGroups = CdefController.getContentTablename(core, "Groups");
                    string BounceAddress = core.siteProperties.getText("EmailBounceAddress", "");
                    string PrimaryLink = "http://" + core.appConfig.domainList[0];
                    while (core.db.csOk(CSEmail)) {
                        int emailID = core.db.csGetInteger(CSEmail, "ID");
                        int EmailMemberID = core.db.csGetInteger(CSEmail, "ModifiedBy");
                        int EmailTemplateID = core.db.csGetInteger(CSEmail, "EmailTemplateID");
                        string EmailTemplate = getEmailTemplate(core, EmailTemplateID);
                        bool EmailAddLinkEID = core.db.csGetBoolean(CSEmail, "AddLinkEID");
                        string EmailFrom = core.db.csGetText(CSEmail, "FromAddress");
                        string EmailSubject = core.db.csGetText(CSEmail, "Subject");
                        //
                        // Mark this email sent and go to the next
                        core.db.csSet(CSEmail, "sent", true);
                        core.db.csSave(CSEmail);
                        //
                        // Create Drop Record
                        int EmailDropID = 0;
                        int CSDrop = core.db.csInsertRecord("Email Drops", EmailMemberID);
                        if (core.db.csOk(CSDrop)) {
                            EmailDropID = core.db.csGetInteger(CSDrop, "ID");
                            DateTime ScheduleDate = core.db.csGetDate(CSEmail, "ScheduleDate");
                            if (ScheduleDate < DateTime.Parse("1/1/2000")) {
                                ScheduleDate = DateTime.Parse("1/1/2000");
                            }
                            core.db.csSet(CSDrop, "Name", "Drop " + EmailDropID + " - Scheduled for " + ScheduleDate.ToString("") + " " + ScheduleDate.ToString(""));
                            core.db.csSet(CSDrop, "EmailID", emailID);
                        }
                        core.db.csClose(ref CSDrop);
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
                        int CSPeople = core.db.csOpenSql(SQL,"Default");
                        //
                        // Send the email to all selected people
                        //
                        string LastEmail = null;
                        string EmailStatusList = "";
                        LastEmail = "empty";
                        while (core.db.csOk(CSPeople)) {
                            int PeopleID = core.db.csGetInteger(CSPeople, "MemberID");
                            string Email = core.db.csGetText(CSPeople, "Email");
                            if (Email == LastEmail) {
                                string PeopleName = core.db.getRecordName("people", PeopleID);
                                if (string.IsNullOrEmpty(PeopleName)) {
                                    PeopleName = "user #" + PeopleID;
                                }
                                EmailStatusList = EmailStatusList + "Not Sent to " + PeopleName + ", duplicate email address (" + Email + ")" + BR;
                            } else {
                                EmailStatusList = EmailStatusList + queueEmailRecord(core, PeopleID, emailID, DateTime.MinValue, EmailDropID, BounceAddress, EmailFrom, EmailTemplate, EmailFrom, EmailSubject, core.db.csGet(CSEmail, "CopyFilename"), core.db.csGetBoolean(CSEmail, "AllowSpamFooter"), core.db.csGetBoolean(CSEmail, "AddLinkEID"), "") + BR;
                                //EmailStatusList = EmailStatusList & SendEmailRecord( PeopleID, EmailID, 0, EmailDropID, BounceAddress, EmailFrom, EmailTemplate, EmailFrom, core.csv_cs_get(CSEmail, "Subject"), core.csv_cs_get(CSEmail, "CopyFilename"), core.csv_cs_getBoolean(CSEmail, "AllowSpamFooter"), core.csv_cs_getBoolean(CSEmail, "AddLinkEID"), "") & BR
                            }
                            LastEmail = Email;
                            core.db.csGoNext(CSPeople);
                        }
                        core.db.csClose(ref CSPeople);
                        //
                        // Send the confirmation
                        //
                        string EmailCopy = core.db.csGet(CSEmail, "copyfilename");
                        int ConfirmationMemberID = core.db.csGetInteger(CSEmail, "testmemberid");
                        queueConfirmationEmail(core, ConfirmationMemberID, EmailDropID, EmailTemplate, EmailAddLinkEID, PrimaryLink, EmailSubject, EmailCopy, "", EmailFrom, EmailStatusList);
                        core.db.csGoNext(CSEmail);
                    }
                }
                core.db.csClose(ref CSEmail);
                //
                return;
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            //ErrorTrap:
            throw (new ApplicationException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "trap error", "App.EXEName", "ProcessEmailClass", "ProcessEmail_GroupEmail", Err.Number, Err.Source, Err.Description, True, True, "")
                                                                      //todo  TASK: Calls to the VB 'Err' function are not converted by Instant C#:
            //Microsoft.VisualBasic.Information.Err().Clear();
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
                int dataSourceType = core.db.getDataSourceType("default");
                string BounceAddress = core.siteProperties.getText("EmailBounceAddress", "");
                DateTime rightNow = DateTime.Now;
                DateTime rightNowDate = rightNow.Date;
                string SQLDateNow = core.db.encodeSQLDate(DateTime.Now);
                //
                // Send Conditional Email - Offset days after Joining
                //   sends email between the condition period date and date +1. if a conditional email is setup and there are already
                //   peope in the group, they do not get the email if they are past the one day threshhold.
                //   To keep them from only getting one, the log is used for the one day.
                //   Housekeep logs far > 1 day
                //
                if (true) {
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
                    int CSEmailBig = core.db.csOpenSql(SQL,"Default");
                    while (core.db.csOk(CSEmailBig)) {
                        int emailID = core.db.csGetInteger(CSEmailBig, "EmailID");
                        int EmailMemberID = core.db.csGetInteger(CSEmailBig, "MemberID");
                        DateTime EmailDateExpires = core.db.csGetDate(CSEmailBig, "DateExpires");
                        int CSEmail = core.db.csOpenContentRecord("Conditional Email", emailID);
                        if (core.db.csOk(CSEmail)) {
                            int EmailTemplateID = core.db.csGetInteger(CSEmail, "EmailTemplateID");
                            string EmailTemplate = getEmailTemplate(core, EmailTemplateID);
                            string FromAddress = core.db.csGetText(CSEmail, "FromAddress");
                            int ConfirmationMemberID = core.db.csGetInteger(CSEmail, "testmemberid");
                            bool EmailAddLinkEID = core.db.csGetBoolean(CSEmail, "AddLinkEID");
                            string EmailSubject = core.db.csGet(CSEmail, "Subject");
                            string EmailCopy = core.db.csGet(CSEmail, "CopyFilename");
                            string EmailStatus = queueEmailRecord(core, EmailMemberID, emailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, EmailSubject, EmailCopy, core.db.csGetBoolean(CSEmail, "AllowSpamFooter"), EmailAddLinkEID, "");
                            queueConfirmationEmail(core, ConfirmationMemberID, 0, EmailTemplate, EmailAddLinkEID, "", EmailSubject, EmailCopy, "", FromAddress, EmailStatus + "<BR>");
                        }
                        core.db.csClose(ref CSEmail);
                        core.db.csGoNext(CSEmailBig);
                    }
                    core.db.csClose(ref CSEmailBig);
                }
                //
                // Send Conditional Email - Offset days Before Expiration
                //
                if (true) {
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
                    int CSEmailBig = core.db.csOpenSql(SQL,"Default");
                    while (core.db.csOk(CSEmailBig)) {
                        int emailID = core.db.csGetInteger(CSEmailBig, "EmailID");
                        int EmailMemberID = core.db.csGetInteger(CSEmailBig, "MemberID");
                        DateTime EmailDateExpires = core.db.csGetDate(CSEmailBig, "DateExpires");
                        int CSEmail = core.db.csOpenContentRecord("Conditional Email", emailID);
                        if (core.db.csOk(CSEmail)) {
                            int EmailTemplateID = core.db.csGetInteger(CSEmail, "EmailTemplateID");
                            string EmailTemplate = getEmailTemplate(core, EmailTemplateID);
                            string FromAddress = core.db.csGetText(CSEmail, "FromAddress");
                            int ConfirmationMemberID = core.db.csGetInteger(CSEmail, "testmemberid");
                            bool EmailAddLinkEID = core.db.csGetBoolean(CSEmail, "AddLinkEID");
                            string EmailSubject = core.db.csGet(CSEmail, "Subject");
                            string EmailCopy = core.db.csGet(CSEmail, "CopyFilename");
                            string EmailStatus = queueEmailRecord(core, EmailMemberID, emailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, core.db.csGet(CSEmail, "Subject"), core.db.csGet(CSEmail, "CopyFilename"), core.db.csGetBoolean(CSEmail, "AllowSpamFooter"), core.db.csGetBoolean(CSEmail, "AddLinkEID"), "");
                            queueConfirmationEmail(core, ConfirmationMemberID, 0, EmailTemplate, EmailAddLinkEID, "", EmailSubject, EmailCopy, "", FromAddress, EmailStatus + "<BR>");
                        }
                        core.db.csClose(ref CSEmail);
                        core.db.csGoNext(CSEmailBig);
                    }
                    core.db.csClose(ref CSEmailBig);
                }
                //
                // Send Conditional Email - Birthday
                //
                if (true) {
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
                        + " AND (ccEmail.ID Not In (Select ccEmailLog.EmailID from ccEmailLog where ccEmailLog.MemberID=ccMembers.ID and ccEmailLog.DateAdded>=" + core.db.encodeSQLDate(DateTime.Now.Date) + "))";
                    int CSEmailBig = core.db.csOpenSql(SQL,"Default");
                    while (core.db.csOk(CSEmailBig)) {
                        int emailID = core.db.csGetInteger(CSEmailBig, "EmailID");
                        int EmailMemberID = core.db.csGetInteger(CSEmailBig, "MemberID");
                        DateTime EmailDateExpires = core.db.csGetDate(CSEmailBig, "DateExpires");
                        int CSEmail = core.db.csOpenContentRecord("Conditional Email", emailID);
                        if (core.db.csOk(CSEmail)) {
                            int EmailTemplateID = core.db.csGetInteger(CSEmail, "EmailTemplateID");
                            string EmailTemplate = getEmailTemplate(core, EmailTemplateID);
                            string FromAddress = core.db.csGetText(CSEmail, "FromAddress");
                            int ConfirmationMemberID = core.db.csGetInteger(CSEmail, "testmemberid");
                            bool EmailAddLinkEID = core.db.csGetBoolean(CSEmail, "AddLinkEID");
                            string EmailSubject = core.db.csGet(CSEmail, "Subject");
                            string EmailCopy = core.db.csGet(CSEmail, "CopyFilename");
                            string EmailStatus = queueEmailRecord(core, EmailMemberID, emailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, core.db.csGet(CSEmail, "Subject"), core.db.csGet(CSEmail, "CopyFilename"), core.db.csGetBoolean(CSEmail, "AllowSpamFooter"), core.db.csGetBoolean(CSEmail, "AddLinkEID"), "");
                            queueConfirmationEmail(core, ConfirmationMemberID, 0, EmailTemplate, EmailAddLinkEID, "", EmailSubject, EmailCopy, "", FromAddress, EmailStatus + "<BR>");
                        }
                        core.db.csClose(ref CSEmail);
                        core.db.csGoNext(CSEmailBig);
                    }
                    core.db.csClose(ref CSEmailBig);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
            int CSPeople = 0;
            int CSLog = 0;
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
                CSLog = core.db.csInsertRecord("Email Log", 0);
                if (core.db.csOk(CSLog)) {
                    core.db.csSet(CSLog, "Name", "Sent " + encodeText(DateTime.Now));
                    core.db.csSet(CSLog, "EmailDropID", emailDropID);
                    core.db.csSet(CSLog, "EmailID", emailID);
                    core.db.csSet(CSLog, "MemberID", MemberID);
                    core.db.csSet(CSLog, "LogType", EmailLogTypeDrop);
                    core.db.csSet(CSLog, "DateBlockExpires", DateBlockExpires);
                    core.db.csSet(CSLog, "SendStatus", "Send attempted but not completed");
                    if (true) {
                        core.db.csSet(CSLog, "fromaddress", FromAddress);
                        core.db.csSet(CSLog, "Subject", EmailSubject);
                    }
                    core.db.csSave(CSLog);
                    //
                    // Get the Template
                    //
                    protocolHostLink = "http://" + core.appConfig.domainList[0];
                    //
                    // Get the Member
                    //
                    CSPeople = core.db.csOpenContentRecord("People", MemberID, 0, false, false, "Email,Name");
                    if (core.db.csOk(CSPeople)) {
                        emailToAddress = core.db.csGet(CSPeople, "Email");
                        emailToName = core.db.csGet(CSPeople, "Name");
                        defaultPage = core.siteProperties.serverPageDefault;
                        urlProtocolDomainSlash = protocolHostLink;
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
                            //EmailTemplateEncoded = contentCmdController.executeContentCommands(core, EmailTemplateEncoded, CPUtilsClass.addonContext.ContextEmail, MemberID, true, ref errorMessage);
                            EmailTemplateEncoded = ActiveContentController.renderHtmlForEmail(core, EmailTemplate, MemberID, ClickFlagQuery);
                            //EmailTemplateEncoded = core.html.convertActiveContent_internal(EmailTemplate, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, protocolHostLink, True, 0, "", CPUtilsClass.addonContext.ContextEmail, True, Nothing, False)
                            //EmailTemplateEncoded = core.csv_EncodeContent8(Nothing, EmailTemplate, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, "", 0, "", True, CPUtilsClass.addonContext.contextEmail)
                            //EmailTemplateEncoded = core.csv_encodecontent8(Nothing, EmailTemplate, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, "", 0, ContentPlaceHolder, True, CPUtilsClass.addonContext.contextemail)
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
                        core.db.csSet(CSLog, "SendStatus", EmailStatus);
                        if (true) {
                            core.db.csSet(CSLog, "toaddress", emailToAddress);
                        }
                        core.db.csSave(CSLog);
                    }
                    //Call core.app.closeCS(CSPeople)
                }
                //Call core.app.closeCS(CSLog)
            } catch (Exception) {
                throw (new ApplicationException("Unexpected exception")); 
            } finally {
                core.db.csClose(ref CSPeople);
                core.db.csClose(ref CSLog);
            }

            return returnStatus;
        }
        //
        //====================================================================================================
        //
        private string getEmailTemplate(CoreController core, int EmailTemplateID) {
            var emailTemplate = Processor.Models.Db.EmailTemplateModel.create(core, EmailTemplateID);
            if ( emailTemplate != null ) {
                return emailTemplate.bodyHTML;
            }
            return "";
            //string tempGetEmailTemplate = "";
            //try {
            //    if (EmailTemplateID != 0) {
            //        int CS = core.db.csOpenContentRecord("Email Templates", EmailTemplateID, 0, false, false, "BodyHTML");
            //        if (core.db.csOk(CS)) {
            //            tempGetEmailTemplate = core.db.csGet(CS, "BodyHTML");
            //        }
            //        core.db.csClose(ref CS);
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
                if ( person != null ) {
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
                    EmailController.queuePersonEmail(core, person, EmailFrom, "Email confirmation from " + core.appConfig.domainList[0], ConfirmBody,"","",true, true, EmailDropID, EmailTemplate, EmailAllowLinkEID, ref sendStatus, queryStringForLinkAppend);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
    }
}