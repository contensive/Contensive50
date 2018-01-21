
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
using Contensive.Core.Models.Complex;
using Contensive.BaseClasses;
//
namespace Contensive.Core.Addons.Email {
    public class processEmailClass  : Contensive.BaseClasses.AddonBaseClass{
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
                coreController cpCore = ((CPClass)cp).core;
                emailController.procesQueue(cpCore);
                //
                // Send Submitted Group Email (submitted, not sent, no conditions)
                //
                ProcessEmail_GroupEmail(cpCore);
                //
                // Send Conditional Email - Offset days after Joining
                //
                DateTime EmailServiceLastCheck = (cpCore.siteProperties.getDate("EmailServiceLastCheck"));
                cpCore.siteProperties.setProperty("EmailServiceLastCheck", encodeText(DateTime.Now));
                bool IsNewHour = (DateTime.Now - EmailServiceLastCheck).TotalHours > 1;
                bool IsNewDay = EmailServiceLastCheck.Date != DateTime.Now.Date;
                ProcessEmail_ConditionalEmail(cpCore, IsNewHour, IsNewDay);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //   Process Group Email
        //
        private void ProcessEmail_GroupEmail(coreController cpCore) {
            try {
                //
                //Dim siteStyles As String
                DateTime ScheduleDate = default(DateTime);
                string EmailCopy = null;
                string SQL = null;
                int CSEmail = 0;
                int CSPeople = 0;
                string SQLDateNow = null;
                int emailID = 0;
                string Criteria = null;
                int CSDrop = 0;
                int EmailDropID = 0;
                int PeopleID = 0;
                int ConfirmationMemberID = 0;
                string EmailSubject = null;
                string EmailStatusList = null;
                int EmailMemberID = 0;
                string SQLTablePeople = null;
                string SQLTableMemberRules = null;
                string SQLTableGroups = null;
                string BounceAddress = null;
                string EmailTemplate = null;
                string PrimaryLink = null;
                bool EmailAddLinkEID = false;
                int EmailTemplateID = 0;
                string EmailFrom = null;
                //
                SQLDateNow = cpCore.db.encodeSQLDate(DateTime.Now);
                PrimaryLink = "http://" + cpCore.serverConfig.appConfig.domainList[0];
                //
                // Open the email records
                //
                Criteria = "(ccemail.active<>0)"
                    + " and (ccemail.Sent=0)"
                    + " and (ccemail.submitted<>0)"
                    + " and ((ccemail.scheduledate is null)or(ccemail.scheduledate<" + SQLDateNow + "))"
                    + " and ((ccemail.ConditionID is null)OR(ccemail.ConditionID=0))"
                    + "";
                CSEmail = cpCore.db.csOpen("Email", Criteria);
                if (cpCore.db.csOk(CSEmail)) {
                    //
                    SQLTablePeople = cdefModel.getContentTablename(cpCore, "People");
                    SQLTableMemberRules =  cdefModel.getContentTablename(cpCore, "Member Rules");
                    SQLTableGroups = cdefModel.getContentTablename(cpCore, "Groups");
                    BounceAddress = cpCore.siteProperties.getText("EmailBounceAddress", "");
                    //siteStyles = cpCore.html.html_getStyleSheet2(0, 0)
                    //
                    while (cpCore.db.csOk(CSEmail)) {
                        emailID = cpCore.db.csGetInteger(CSEmail, "ID");
                        EmailMemberID = cpCore.db.csGetInteger(CSEmail, "ModifiedBy");
                        EmailTemplateID = cpCore.db.csGetInteger(CSEmail, "EmailTemplateID");
                        EmailTemplate = GetEmailTemplate(cpCore, EmailTemplateID);
                        EmailAddLinkEID = cpCore.db.csGetBoolean(CSEmail, "AddLinkEID");
                        //exclusiveStyles = cpCore.asv.csv_cs_getText(CSEmail, "exclusiveStyles")
                        EmailFrom = cpCore.db.csGetText(CSEmail, "FromAddress");
                        EmailSubject = cpCore.db.csGetText(CSEmail, "Subject");
                        //emailStyles = emailController.getStyles(emailID)
                        //
                        // Mark this email sent and go to the next
                        //
                        cpCore.db.csSet(CSEmail, "sent", true);
                        cpCore.db.csSave2(CSEmail);
                        //
                        // Create Drop Record
                        //
                        CSDrop = cpCore.db.csInsertRecord("Email Drops", EmailMemberID);
                        if (cpCore.db.csOk(CSDrop)) {
                            EmailDropID = cpCore.db.csGetInteger(CSDrop, "ID");
                            ScheduleDate = cpCore.db.csGetDate(CSEmail, "ScheduleDate");
                            if (ScheduleDate < DateTime.Parse("1/1/2000")) {
                                ScheduleDate = DateTime.Parse("1/1/2000");
                            }
                            cpCore.db.csSet(CSDrop, "Name", "Drop " + EmailDropID + " - Scheduled for " + ScheduleDate.ToString("") + " " + ScheduleDate.ToString(""));
                            cpCore.db.csSet(CSDrop, "EmailID", emailID);
                            //Call cpCore.asv.csv_SetCSField(CSDrop, "CreatedBy", EmailMemberID)
                        }
                        cpCore.db.csClose(ref CSDrop);
                        //
                        // Select all people in the groups for this email
                        SQL = "select Distinct " + SQLTablePeople + ".ID as MemberID," + SQLTablePeople + ".email"
                            + " From ((((ccemail"
                            + " left join ccEmailGroups on ccEmailGroups.EmailID=ccEmail.ID)"
                            + " left join " + SQLTableGroups + " on " + SQLTableGroups + ".ID = ccEmailGroups.GroupID)"
                            + " left join " + SQLTableMemberRules + " on " + SQLTableGroups + ".ID = " + SQLTableMemberRules + ".GroupID)"
                            + " left join " + SQLTablePeople + " on " + SQLTablePeople + ".ID = " + SQLTableMemberRules + ".MemberID)"
                            + " Where (ccEmail.ID=" + emailID + ")"
                            + " and (" + SQLTableGroups + ".active<>0)"
                            + " and (" + SQLTableGroups + ".AllowBulkEmail<>0)"
                            + " and (" + SQLTablePeople + ".active<>0)"
                            + " and (" + SQLTablePeople + ".AllowBulkEmail<>0)"
                            + " and (" + SQLTablePeople + ".email<>'')"
                            + " and ((" + SQLTableMemberRules + ".DateExpires is null)or(" + SQLTableMemberRules + ".DateExpires>" + SQLDateNow + "))"
                            + " order by " + SQLTablePeople + ".email," + SQLTablePeople + ".id";
                        CSPeople = cpCore.db.csOpenSql_rev("default", SQL);
                        //
                        // Send the email to all selected people
                        //
                        string LastEmail = null;
                        string Email = null;
                        string PeopleName = null;
                        EmailStatusList = "";
                        LastEmail = "empty";
                        while (cpCore.db.csOk(CSPeople)) {
                            PeopleID = cpCore.db.csGetInteger(CSPeople, "MemberID");
                            Email = cpCore.db.csGetText(CSPeople, "Email");
                            if (Email == LastEmail) {
                                PeopleName = cpCore.db.getRecordName("people", PeopleID);
                                if (string.IsNullOrEmpty(PeopleName)) {
                                    PeopleName = "user #" + PeopleID;
                                }
                                EmailStatusList = EmailStatusList + "Not Sent to " + PeopleName + ", duplicate email address (" + Email + ")" + BR;
                            } else {
                                EmailStatusList = EmailStatusList + SendEmailRecord(cpCore, PeopleID, emailID, DateTime.MinValue, EmailDropID, BounceAddress, EmailFrom, EmailTemplate, EmailFrom, EmailSubject, cpCore.db.csGet(CSEmail, "CopyFilename"), cpCore.db.csGetBoolean(CSEmail, "AllowSpamFooter"), cpCore.db.csGetBoolean(CSEmail, "AddLinkEID"), "") + BR;
                                //EmailStatusList = EmailStatusList & SendEmailRecord( PeopleID, EmailID, 0, EmailDropID, BounceAddress, EmailFrom, EmailTemplate, EmailFrom, cpCore.csv_cs_get(CSEmail, "Subject"), cpCore.csv_cs_get(CSEmail, "CopyFilename"), cpCore.csv_cs_getBoolean(CSEmail, "AllowSpamFooter"), cpCore.csv_cs_getBoolean(CSEmail, "AddLinkEID"), "") & BR
                            }
                            LastEmail = Email;
                            cpCore.db.csGoNext(CSPeople);
                        }
                        cpCore.db.csClose(ref CSPeople);
                        //
                        // Send the confirmation
                        //
                        EmailCopy = cpCore.db.csGet(CSEmail, "copyfilename");
                        ConfirmationMemberID = cpCore.db.csGetInteger(CSEmail, "testmemberid");
                        SendConfirmationEmail(cpCore, ConfirmationMemberID, EmailDropID, EmailTemplate, EmailAddLinkEID, PrimaryLink, EmailSubject, EmailCopy, "", EmailFrom, EmailStatusList);
                        //            CSPeople = cpCore.asv.csOpenRecord("people", ConfirmationMemberID)
                        //            If cpCore.asv.csv_IsCSOK(CSPeople) Then
                        //                ClickFlagQuery = RequestNameEmailClickFlag & "=" & EmailDropID & "&" & RequestNameEmailMemberID & "=" & ConfirmationMemberID
                        //                EmailTemplate = cpCore.csv_EncodeContent(EmailTemplate, ConfirmationMemberID, -1, False, EmailAddLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink)
                        //                EmailSubject = cpCore.csv_EncodeContent(cpCore.csv_cs_get(CSEmail, "Subject"), ConfirmationMemberID, , True, False, False, False, False, True, , "http://" & GetPrimaryDomainName())
                        //                EmailBody = cpCore.csv_EncodeContent(cpCore.csv_cs_get(CSEmail, "CopyFilename"), ConfirmationMemberID, , False, EmailAddLinkEID, True, True, False, True, , "http://" & GetPrimaryDomainName())
                        //                'EmailFrom = cpCore.csv_cs_get(CSEmail, "FromAddress")
                        //                Confirmation = "<HTML><Head>" _
                        //                    & "<Title>Email Confirmation</Title>" _
                        //                    & "<Base href=""http://" & GetPrimaryDomainName() & cpCore.csv_RootPath & """>" _
                        //                    & emailStyles _
                        //                    & "</Head><BODY>" _
                        //                    & "The follow email has been sent" & BR & BR _
                        //                    & "Subject: " & EmailSubject & BR _
                        //                    & "From: " & EmailFrom & BR _
                        //                    & "Body" & BR _
                        //                    & "----------------------------------------------------------------------" & BR _
                        //                    & cpCore.csv_MergeTemplate(EmailTemplate, EmailBody, ConfirmationMemberID) & BR _
                        //                    & "----------------------------------------------------------------------" & BR _
                        //                    & "--- email list ---" & BR _
                        //                    & EmailStatusList _
                        //                    & "--- end email list ---" & BR _
                        //                    & "</BODY></HTML>"
                        //                Confirmation = ConvertLinksToAbsolute(Confirmation, PrimaryLink & "/")
                        //                Call cpCore.csv_SendEmail2(cpCore.asv.csv_cs_getText(CSPeople, "Email"), EmailFrom, "Email Confirmation from " & GetPrimaryDomainName(), Confirmation, "", "", , True, True)
                        //                End If
                        //            Call cpCore.asv.csv_CloseCS(CSPeople)
                        //
                        cpCore.db.csGoNext(CSEmail);
                    }
                }
                cpCore.db.csClose(ref CSEmail);
                //
                return;
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "trap error", "App.EXEName", "ProcessEmailClass", "ProcessEmail_GroupEmail", Err.Number, Err.Source, Err.Description, True, True, "")
                                                                      //todo  TASK: Calls to the VB 'Err' function are not converted by Instant C#:
            //Microsoft.VisualBasic.Information.Err().Clear();
        }
        //
        //====================================================================================================
        //
        private void ProcessEmail_ConditionalEmail(coreController cpCore, bool IsNewHour, bool IsNewDay) {
            try {
                //
                //
                bool EmailAddLinkEID = false;
                string EmailSubject = null;
                string EmailCopy = null;
                string EmailStatus = null;
                string SQL = null;
                int CSEmailBig = 0;
                int CSEmail = 0;
                int emailID = 0;
                int EmailDropID = 0;
                int ConfirmationMemberID = 0;
                string SQLTablePeople = null;
                string SQLTableMemberRules = null;
                string SQLTableGroups = null;
                string BounceAddress = null;
                int EmailTemplateID = 0;
                string EmailTemplate = null;
                string FieldList = null;
                string FromAddress = null;
                int EmailMemberID = 0;
                DateTime EmailDateExpires = default(DateTime);
                DateTime rightNow = default(DateTime);
                DateTime rightNowDate = default(DateTime);
                string SQLDateNow = null;
                int dataSourceType = 0;
                string sqlDateTest = null;
                //
                dataSourceType = cpCore.db.getDataSourceType("default");
                SQLTablePeople = cdefModel.getContentTablename(cpCore, "People");
                SQLTableMemberRules = cdefModel.getContentTablename(cpCore, "Member Rules");
                SQLTableGroups = cdefModel.getContentTablename(cpCore, "Groups");
                BounceAddress = cpCore.siteProperties.getText("EmailBounceAddress", "");
                // siteStyles = cpCore.html.html_getStyleSheet2(0, 0)
                //
                rightNow = DateTime.Now;
                rightNowDate = rightNow.Date;
                SQLDateNow = cpCore.db.encodeSQLDate(DateTime.Now);
                //
                // Send Conditional Email - Offset days after Joining
                //   sends email between the condition period date and date +1. if a conditional email is setup and there are already
                //   peope in the group, they do not get the email if they are past the one day threshhold.
                //   To keep them from only getting one, the log is used for the one day.
                //   Housekeep logs far > 1 day
                //
                if (IsNewDay) {
                    FieldList = "ccEmail.TestMemberID AS TestMemberID,ccEmail.ID as EmailID," + SQLTablePeople + ".ID AS MemberID, " + SQLTableMemberRules + ".DateExpires AS DateExpires,ccEmail.BlockSiteStyles,ccEmail.stylesFilename";
                    if (dataSourceType == DataSourceTypeODBCSQLServer) {
                        sqlDateTest = ""
                            + " AND (CAST(" + SQLTableMemberRules + ".DateAdded as datetime)+ccEmail.ConditionPeriod < " + SQLDateNow + ")"
                            + " AND (CAST(" + SQLTableMemberRules + ".DateAdded as datetime)+ccEmail.ConditionPeriod+1.0 > " + SQLDateNow + ")"
                            + "";
                    } else {
                        sqlDateTest = ""
                            + " AND (" + SQLTableMemberRules + ".DateAdded+ccEmail.ConditionPeriod < " + SQLDateNow + ")"
                            + " AND (" + SQLTableMemberRules + ".DateAdded+ccEmail.ConditionPeriod+1.0 > " + SQLDateNow + ")"
                            + "";
                    }
                    SQL = "SELECT Distinct " + FieldList + " FROM ((((ccEmail"
                        + " LEFT JOIN ccEmailGroups ON ccEmail.ID = ccEmailGroups.EmailID)"
                        + " LEFT JOIN " + SQLTableGroups + " ON ccEmailGroups.GroupID = " + SQLTableGroups + ".ID)"
                        + " LEFT JOIN " + SQLTableMemberRules + " ON " + SQLTableGroups + ".ID = " + SQLTableMemberRules + ".GroupID)"
                        + " LEFT JOIN " + SQLTablePeople + " ON " + SQLTableMemberRules + ".MemberID = " + SQLTablePeople + ".ID)"
                        + " Where (ccEmail.id Is Not Null)"
                        + sqlDateTest + " AND (ccEmail.ConditionExpireDate > " + SQLDateNow + " OR ccEmail.ConditionExpireDate IS NULL)"
                        + " AND (ccEmail.ScheduleDate < " + SQLDateNow + " OR ccEmail.ScheduleDate IS NULL)"
                        + " AND (ccEmail.Submitted <> 0)"
                        + " AND (ccEmail.ConditionID = 2)"
                        + " AND (ccEmail.ConditionPeriod IS NOT NULL)"
                        + " AND (" + SQLTableGroups + ".Active <> 0)"
                        + " AND (" + SQLTableGroups + ".AllowBulkEmail <> 0)"
                        + " AND (" + SQLTablePeople + ".ID IS NOT NULL)"
                        + " AND (" + SQLTablePeople + ".Active <> 0)"
                        + " AND (" + SQLTablePeople + ".AllowBulkEmail <> 0)"
                        + " AND (ccEmail.ID Not In (Select ccEmailLog.EmailID from ccEmailLog where ccEmailLog.MemberID=" + SQLTablePeople + ".ID))";
                    CSEmailBig = cpCore.db.csOpenSql_rev("Default", SQL);
                    while (cpCore.db.csOk(CSEmailBig)) {
                        emailID = cpCore.db.csGetInteger(CSEmailBig, "EmailID");
                        EmailMemberID = cpCore.db.csGetInteger(CSEmailBig, "MemberID");
                        EmailDateExpires = cpCore.db.csGetDate(CSEmailBig, "DateExpires");
                        CSEmail = cpCore.db.cs_openContentRecord("Conditional Email", emailID);
                        if (cpCore.db.csOk(CSEmail)) {
                            EmailTemplateID = cpCore.db.csGetInteger(CSEmail, "EmailTemplateID");
                            EmailTemplate = GetEmailTemplate(cpCore, EmailTemplateID);
                            FromAddress = cpCore.db.csGetText(CSEmail, "FromAddress");
                            ConfirmationMemberID = cpCore.db.csGetInteger(CSEmail, "testmemberid");
                            EmailAddLinkEID = cpCore.db.csGetBoolean(CSEmail, "AddLinkEID");
                            EmailSubject = cpCore.db.csGet(CSEmail, "Subject");
                            EmailCopy = cpCore.db.csGet(CSEmail, "CopyFilename");
                            //emailStyles = emailController.getStyles(emailID)
                            EmailStatus = SendEmailRecord(cpCore, EmailMemberID, emailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, EmailSubject, EmailCopy, cpCore.db.csGetBoolean(CSEmail, "AllowSpamFooter"), EmailAddLinkEID, "");
                            //EmailStatus = SendEmailRecord( EmailMemberID, EmailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, EmailSubject, EmailCopy, cpCore.csv_cs_getBoolean(CSEmail, "AllowSpamFooter"), EmailAddLinkEID, EmailInlineStyles)
                            SendConfirmationEmail(cpCore, ConfirmationMemberID, EmailDropID, EmailTemplate, EmailAddLinkEID, "", EmailSubject, EmailCopy, "", FromAddress, EmailStatus + "<BR>");
                        }
                        cpCore.db.csClose(ref CSEmail);
                        cpCore.db.csGoNext(CSEmailBig);
                    }
                    cpCore.db.csClose(ref CSEmailBig);
                }
                //
                // Send Conditional Email - Offset days Before Expiration
                //
                if (IsNewDay) {
                    FieldList = "ccEmail.TestMemberID AS TestMemberID,ccEmail.ID AS EmailID, " + SQLTablePeople + ".ID AS MemberID, " + SQLTableMemberRules + ".DateExpires AS DateExpires,ccEmail.BlockSiteStyles,ccEmail.stylesFilename";
                    if (dataSourceType == DataSourceTypeODBCSQLServer) {
                        sqlDateTest = ""
                            + " AND (CAST(" + SQLTableMemberRules + ".DateExpires as datetime)-ccEmail.ConditionPeriod > " + SQLDateNow + ")"
                            + " AND (CAST(" + SQLTableMemberRules + ".DateExpires as datetime)-ccEmail.ConditionPeriod-1.0 < " + SQLDateNow + ")"
                            + "";
                    } else {
                        sqlDateTest = ""
                            + " AND (" + SQLTableMemberRules + ".DateExpires-ccEmail.ConditionPeriod > " + SQLDateNow + ")"
                            + " AND (" + SQLTableMemberRules + ".DateExpires-ccEmail.ConditionPeriod-1.0 < " + SQLDateNow + ")"
                            + "";
                    }
                    SQL = "SELECT DISTINCT " + FieldList + " FROM ((((ccEmail"
                        + " LEFT JOIN ccEmailGroups ON ccEmail.ID = ccEmailGroups.EmailID)"
                        + " LEFT JOIN " + SQLTableGroups + " ON ccEmailGroups.GroupID = " + SQLTableGroups + ".ID)"
                        + " LEFT JOIN " + SQLTableMemberRules + " ON " + SQLTableGroups + ".ID = " + SQLTableMemberRules + ".GroupID)"
                        + " LEFT JOIN " + SQLTablePeople + " ON " + SQLTableMemberRules + ".MemberID = " + SQLTablePeople + ".ID)"
                        + " Where (ccEmail.id Is Not Null)"
                        + sqlDateTest + " AND (ccEmail.ConditionExpireDate > " + SQLDateNow + " OR ccEmail.ConditionExpireDate IS NULL)"
                        + " AND (ccEmail.ScheduleDate < " + SQLDateNow + " OR ccEmail.ScheduleDate IS NULL)"
                        + " AND (ccEmail.Submitted <> 0)"
                        + " AND (ccEmail.ConditionID = 1)"
                        + " AND (ccEmail.ConditionPeriod IS NOT NULL)"
                        + " AND (" + SQLTableGroups + ".Active <> 0)"
                        + " AND (" + SQLTableGroups + ".AllowBulkEmail <> 0)"
                        + " AND (" + SQLTablePeople + ".ID IS NOT NULL)"
                        + " AND (" + SQLTablePeople + ".Active <> 0)"
                        + " AND (" + SQLTablePeople + ".AllowBulkEmail <> 0)"
                        + " AND (ccEmail.ID Not In (Select ccEmailLog.EmailID from ccEmailLog where ccEmailLog.MemberID=" + SQLTablePeople + ".ID))";
                    CSEmailBig = cpCore.db.csOpenSql_rev("Default", SQL);
                    while (cpCore.db.csOk(CSEmailBig)) {
                        emailID = cpCore.db.csGetInteger(CSEmailBig, "EmailID");
                        EmailMemberID = cpCore.db.csGetInteger(CSEmailBig, "MemberID");
                        EmailDateExpires = cpCore.db.csGetDate(CSEmailBig, "DateExpires");
                        CSEmail = cpCore.db.cs_openContentRecord("Conditional Email", emailID);
                        if (cpCore.db.csOk(CSEmail)) {
                            EmailTemplateID = cpCore.db.csGetInteger(CSEmail, "EmailTemplateID");
                            EmailTemplate = GetEmailTemplate(cpCore, EmailTemplateID);
                            FromAddress = cpCore.db.csGetText(CSEmail, "FromAddress");
                            ConfirmationMemberID = cpCore.db.csGetInteger(CSEmail, "testmemberid");
                            EmailAddLinkEID = cpCore.db.csGetBoolean(CSEmail, "AddLinkEID");
                            EmailSubject = cpCore.db.csGet(CSEmail, "Subject");
                            EmailCopy = cpCore.db.csGet(CSEmail, "CopyFilename");
                            //emailStyles = emailController.getStyles(emailID)
                            EmailStatus = SendEmailRecord(cpCore, EmailMemberID, emailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, cpCore.db.csGet(CSEmail, "Subject"), cpCore.db.csGet(CSEmail, "CopyFilename"), cpCore.db.csGetBoolean(CSEmail, "AllowSpamFooter"), cpCore.db.csGetBoolean(CSEmail, "AddLinkEID"), "");
                            //EmailStatus = SendEmailRecord( EmailMemberID, EmailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, cpCore.csv_cs_get(CSEmail, "Subject"), cpCore.csv_cs_get(CSEmail, "CopyFilename"), cpCore.csv_cs_getBoolean(CSEmail, "AllowSpamFooter"), cpCore.csv_cs_getBoolean(CSEmail, "AddLinkEID"), EmailInlineStyles)
                            SendConfirmationEmail(cpCore, ConfirmationMemberID, EmailDropID, EmailTemplate, EmailAddLinkEID, "", EmailSubject, EmailCopy, "", FromAddress, EmailStatus + "<BR>");
                        }
                        cpCore.db.csClose(ref CSEmail);
                        cpCore.db.csGoNext(CSEmailBig);
                    }
                    cpCore.db.csClose(ref CSEmailBig);
                }
                //
                // Send Conditional Email - Birthday
                //
                if (IsNewDay) {
                    FieldList = "ccEmail.TestMemberID AS TestMemberID,ccEmail.ID AS EmailID, " + SQLTablePeople + ".ID AS MemberID, " + SQLTableMemberRules + ".DateExpires AS DateExpires,ccEmail.BlockSiteStyles,ccEmail.stylesFilename";
                    SQL = "SELECT DISTINCT " + FieldList + " FROM ((((ccEmail"
                        + " LEFT JOIN ccEmailGroups ON ccEmail.ID = ccEmailGroups.EmailID)"
                        + " LEFT JOIN " + SQLTableGroups + " ON ccEmailGroups.GroupID = " + SQLTableGroups + ".ID)"
                        + " LEFT JOIN " + SQLTableMemberRules + " ON " + SQLTableGroups + ".ID = " + SQLTableMemberRules + ".GroupID)"
                        + " LEFT JOIN " + SQLTablePeople + " ON " + SQLTableMemberRules + ".MemberID = " + SQLTablePeople + ".ID)"
                        + " Where (ccEmail.id Is Not Null)"
                        + " AND (ccEmail.ConditionExpireDate > " + SQLDateNow + " OR ccEmail.ConditionExpireDate IS NULL)"
                        + " AND (ccEmail.ScheduleDate < " + SQLDateNow + " OR ccEmail.ScheduleDate IS NULL)"
                        + " AND (ccEmail.Submitted <> 0)"
                        + " AND (ccEmail.ConditionID = 3)"
                        + " AND (" + SQLTableGroups + ".Active <> 0)"
                        + " AND (" + SQLTableGroups + ".AllowBulkEmail <> 0)"
                        + " AND ((" + SQLTableMemberRules + ".DateExpires is null)or(" + SQLTableMemberRules + ".DateExpires > " + SQLDateNow + "))"
                        + " AND (" + SQLTablePeople + ".ID IS NOT NULL)"
                        + " AND (" + SQLTablePeople + ".Active <> 0)"
                        + " AND (" + SQLTablePeople + ".AllowBulkEmail <> 0)"
                        + " AND (" + SQLTablePeople + ".BirthdayMonth=" + DateTime.Now.Month + ")"
                        + " AND (" + SQLTablePeople + ".BirthdayDay=" + DateTime.Now.Day + ")"
                        + " AND (ccEmail.ID Not In (Select ccEmailLog.EmailID from ccEmailLog where ccEmailLog.MemberID=" + SQLTablePeople + ".ID and ccEmailLog.DateAdded>=" + cpCore.db.encodeSQLDate(DateTime.Now.Date) + "))";
                    CSEmailBig = cpCore.db.csOpenSql_rev("Default", SQL);
                    while (cpCore.db.csOk(CSEmailBig)) {
                        emailID = cpCore.db.csGetInteger(CSEmailBig, "EmailID");
                        EmailMemberID = cpCore.db.csGetInteger(CSEmailBig, "MemberID");
                        EmailDateExpires = cpCore.db.csGetDate(CSEmailBig, "DateExpires");
                        CSEmail = cpCore.db.cs_openContentRecord("Conditional Email", emailID);
                        if (cpCore.db.csOk(CSEmail)) {
                            EmailTemplateID = cpCore.db.csGetInteger(CSEmail, "EmailTemplateID");
                            EmailTemplate = GetEmailTemplate(cpCore, EmailTemplateID);
                            FromAddress = cpCore.db.csGetText(CSEmail, "FromAddress");
                            ConfirmationMemberID = cpCore.db.csGetInteger(CSEmail, "testmemberid");
                            EmailAddLinkEID = cpCore.db.csGetBoolean(CSEmail, "AddLinkEID");
                            EmailSubject = cpCore.db.csGet(CSEmail, "Subject");
                            EmailCopy = cpCore.db.csGet(CSEmail, "CopyFilename");
                            //emailStyles = emailController.getStyles(emailID)
                            EmailStatus = SendEmailRecord(cpCore, EmailMemberID, emailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, cpCore.db.csGet(CSEmail, "Subject"), cpCore.db.csGet(CSEmail, "CopyFilename"), cpCore.db.csGetBoolean(CSEmail, "AllowSpamFooter"), cpCore.db.csGetBoolean(CSEmail, "AddLinkEID"), "");
                            //EmailStatus = SendEmailRecord( EmailMemberID, EmailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, cpCore.csv_cs_get(CSEmail, "Subject"), cpCore.csv_cs_get(CSEmail, "CopyFilename"), cpCore.csv_cs_getBoolean(CSEmail, "AllowSpamFooter"), cpCore.csv_cs_getBoolean(CSEmail, "AddLinkEID"), EmailInlineStyles)
                            SendConfirmationEmail(cpCore, ConfirmationMemberID, EmailDropID, EmailTemplate, EmailAddLinkEID, "", EmailSubject, EmailCopy, "", FromAddress, EmailStatus + "<BR>");
                        }
                        cpCore.db.csClose(ref CSEmail);
                        cpCore.db.csGoNext(CSEmailBig);
                    }
                    cpCore.db.csClose(ref CSEmailBig);
                }
                //
                return;
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "trap error", "App.EXEName", "ProcessEmailClass", "ProcessEmail_ConditionalEmail", Err.Number, Err.Source, Err.Description, True, True, "")
                                                                      //todo  TASK: Calls to the VB 'Err' function are not converted by Instant C#:
            //Microsoft.VisualBasic.Information.Err().Clear();
        }
        //
        //====================================================================================================
        /// <summary>
        /// Send email to a memberid
        /// </summary>
        /// <param name="MemberID"></param>
        /// <param name="emailID"></param>
        /// <param name="DateBlockExpires"></param>
        /// <param name="EmailDropID"></param>
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
        private string SendEmailRecord(coreController cpCore, int MemberID, int emailID, DateTime DateBlockExpires, int EmailDropID, string BounceAddress, string ReplyToAddress, string EmailTemplate, string FromAddress, string EmailSubject, string EmailBody, bool AllowSpamFooter, bool EmailAllowLinkEID, string emailStyles) {
            string returnStatus = "";
            int CSPeople = 0;
            int CSLog = 0;
            try {
                //
                //Dim CS as integer
                //Dim EmailFrom As String
                //Dim HTMLHead As String
                string ServerPageDefault = null;
                string EmailToName = null;
                string ClickFlagQuery = null;
                string EmailStatus = null;
                //Dim FieldList As String
                //Dim InlineStyles As String
                string emailWorkingStyles = null;
                string RootURL = null;
                string protocolHostLink = null;
                string ToAddress = null;
                //Dim ToAddressName As String
                string EmailBodyEncoded = null;
                string EmailSubjectEncoded = null;
                //
                string errorMessage = "";
                string EmailTemplateEncoded = "";
                string OpenTriggerCode = "";
                //
                EmailBodyEncoded = EmailBody;
                EmailSubjectEncoded = EmailSubject;
                //buildversion = cpCore.app.dataBuildVersion
                CSLog = cpCore.db.csInsertRecord("Email Log", 0);
                if (cpCore.db.csOk(CSLog)) {
                    cpCore.db.csSet(CSLog, "Name", "Sent " + encodeText(DateTime.Now));
                    cpCore.db.csSet(CSLog, "EmailDropID", EmailDropID);
                    cpCore.db.csSet(CSLog, "EmailID", emailID);
                    cpCore.db.csSet(CSLog, "MemberID", MemberID);
                    cpCore.db.csSet(CSLog, "LogType", EmailLogTypeDrop);
                    cpCore.db.csSet(CSLog, "DateBlockExpires", DateBlockExpires);
                    cpCore.db.csSet(CSLog, "SendStatus", "Send attempted but not completed");
                    if (true) {
                        cpCore.db.csSet(CSLog, "fromaddress", FromAddress);
                        cpCore.db.csSet(CSLog, "Subject", EmailSubject);
                    }
                    cpCore.db.csSave2(CSLog);
                    //
                    // Get the Template
                    //
                    protocolHostLink = "http://" + GetPrimaryDomainName(cpCore);
                    //
                    // Get the Member
                    //
                    CSPeople = cpCore.db.cs_openContentRecord("People", MemberID, 0, false, false, "Email,Name");
                    if (cpCore.db.csOk(CSPeople)) {
                        ToAddress = cpCore.db.csGet(CSPeople, "Email");
                        EmailToName = cpCore.db.csGet(CSPeople, "Name");
                        ServerPageDefault = cpCore.siteProperties.getText(siteproperty_serverPageDefault_name, siteproperty_serverPageDefault_defaultValue);
                        RootURL = protocolHostLink + requestAppRootPath;
                        if (EmailDropID != 0) {
                            switch (cpCore.siteProperties.getInteger("GroupEmailOpenTriggerMethod", 0)) {
                                case 1:
                                    OpenTriggerCode = "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + RootURL + ServerPageDefault + "?" + RequestNameEmailOpenCssFlag + "=" + EmailDropID + "&" + rnEmailMemberID + "=#member_id#\">";
                                    break;
                                default:
                                    OpenTriggerCode = "<img src=\"" + RootURL + ServerPageDefault + "?" + rnEmailOpenFlag + "=" + EmailDropID + "&" + rnEmailMemberID + "=#member_id#\">";
                                    break;
                            }
                        }
                        //
                        emailWorkingStyles = emailStyles;
                        emailWorkingStyles = genericController.vbReplace(emailWorkingStyles, StyleSheetStart, StyleSheetStart + "<!-- ", 1, 99, 1);
                        emailWorkingStyles = genericController.vbReplace(emailWorkingStyles, StyleSheetEnd, " // -->" + StyleSheetEnd, 1, 99, 1);
                        //
                        // Create the clickflag to be added to all anchors
                        //
                        ClickFlagQuery = rnEmailClickFlag + "=" + EmailDropID + "&" + rnEmailMemberID + "=" + MemberID;
                        //
                        // Encode body and subject
                        //
                        EmailBodyEncoded = contentCmdController.executeContentCommands(cpCore, EmailBodyEncoded, CPUtilsClass.addonContext.ContextEmail, MemberID, true, ref errorMessage);
                        EmailBodyEncoded = activeContentController.convertActiveContentToHtmlForEmailSend(cpCore, EmailBodyEncoded, MemberID, ClickFlagQuery);
                        //EmailBodyEncoded = cpCore.html.convertActiveContent_internal(EmailBodyEncoded, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, 0, "", CPUtilsClass.addonContext.ContextEmail, True, Nothing, False)
                        //EmailBodyEncoded = cpCore.csv_EncodeContent8(Nothing, EmailBodyEncoded, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, "", 0, "", True, CPUtilsClass.addonContext.contextEmail)
                        //
                        EmailSubjectEncoded = contentCmdController.executeContentCommands(cpCore, EmailSubjectEncoded, CPUtilsClass.addonContext.ContextEmail, MemberID, true, ref errorMessage);
                        EmailSubjectEncoded = activeContentController.convertActiveContentToHtmlForEmailSend(cpCore, EmailSubjectEncoded, MemberID, ClickFlagQuery);
                        //EmailSubjectEncoded = cpCore.html.convertActiveContent_internal(EmailSubjectEncoded, MemberID, "", 0, 0, True, False, False, False, False, True, "", PrimaryLink, True, 0, "", CPUtilsClass.addonContext.ContextEmail, True, Nothing, False)
                        //EmailSubjectEncoded = cpCore.csv_EncodeContent8(Nothing, EmailSubjectEncoded, MemberID, "", 0, 0, True, False, False, False, False, True, "", PrimaryLink, True, "", 0, "", True, CPUtilsClass.addonContext.contextEmail)
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
                            EmailTemplateEncoded = contentCmdController.executeContentCommands(cpCore, EmailTemplateEncoded, CPUtilsClass.addonContext.ContextEmail, MemberID, true, ref errorMessage);
                            EmailTemplateEncoded = activeContentController.convertActiveContentToHtmlForEmailSend(cpCore, EmailTemplate, MemberID, ClickFlagQuery);
                            //EmailTemplateEncoded = cpCore.html.convertActiveContent_internal(EmailTemplate, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, protocolHostLink, True, 0, "", CPUtilsClass.addonContext.ContextEmail, True, Nothing, False)
                            //EmailTemplateEncoded = cpCore.csv_EncodeContent8(Nothing, EmailTemplate, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, "", 0, "", True, CPUtilsClass.addonContext.contextEmail)
                            //EmailTemplateEncoded = cpCore.csv_encodecontent8(Nothing, EmailTemplate, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, "", 0, ContentPlaceHolder, True, CPUtilsClass.addonContext.contextemail)
                            if (genericController.vbInstr(1, EmailTemplateEncoded, fpoContentBox) != 0) {
                                EmailBodyEncoded = genericController.vbReplace(EmailTemplateEncoded, fpoContentBox, EmailBodyEncoded);
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
                        EmailBodyEncoded = genericController.vbReplace(EmailBodyEncoded, rnEmailBlockRecipientEmail, "", 1, 99, 1);
                        if (AllowSpamFooter) {
                            //
                            // non-authorable, default true - leave it as an option in case there is an important exception
                            //
                            EmailBodyEncoded = EmailBodyEncoded + "<div style=\"padding:10px;\">" + GetLinkedText("<a href=\"" + RootURL + ServerPageDefault + "?" + rnEmailBlockRecipientEmail + "=#member_email#&" + rnEmailBlockRequestDropID + "=" + EmailDropID + "\">", cpCore.siteProperties.getText("EmailSpamFooter", DefaultSpamFooter)) + "</div>";
                        }
                        //
                        // open trigger under footer (so it does not shake as the image comes in)
                        //
                        EmailBodyEncoded = EmailBodyEncoded + OpenTriggerCode;
                        EmailBodyEncoded = genericController.vbReplace(EmailBodyEncoded, "#member_id#", MemberID);
                        EmailBodyEncoded = genericController.vbReplace(EmailBodyEncoded, "#member_email#", ToAddress);
                        //
                        // Now convert URLS to absolute
                        //
                        EmailBodyEncoded = ConvertLinksToAbsolute(EmailBodyEncoded, RootURL);
                        //
                        EmailBodyEncoded = ""
                            + "<HTML>"
                            + "<Head>"
                            + "<Title>" + EmailSubjectEncoded + "</Title>"
                            + "<Base href=\"" + RootURL + "\">"
                            + "</Head>"
                            + "<BODY class=ccBodyEmail>"
                            + "<Base href=\"" + RootURL + "\">"
                            + emailWorkingStyles + EmailBodyEncoded + "</BODY>"
                            + "</HTML>";
                        //
                        // Send
                        //
                        emailController.sendAdHoc(cpCore, ToAddress, FromAddress, EmailSubjectEncoded, EmailBodyEncoded, BounceAddress, ReplyToAddress, "", true, true, 0, ref EmailStatus);
                        if (string.IsNullOrEmpty(EmailStatus)) {
                            EmailStatus = "ok";
                        }
                        returnStatus = returnStatus + "Send to " + EmailToName + " at " + ToAddress + ", Status = " + EmailStatus;
                        //
                        // ----- Log the send
                        //
                        cpCore.db.csSet(CSLog, "SendStatus", EmailStatus);
                        if (true) {
                            cpCore.db.csSet(CSLog, "toaddress", ToAddress);
                        }
                        cpCore.db.csSave2(CSLog);
                    }
                    //Call cpCore.app.closeCS(CSPeople)
                }
                //Call cpCore.app.closeCS(CSLog)
            } catch (Exception) {
                throw (new ApplicationException("Unexpected exception")); 
            } finally {
                cpCore.db.csClose(ref CSPeople);
                cpCore.db.csClose(ref CSLog);
            }

            return returnStatus;
        }
        //
        //====================================================================================================
        //
        private string GetPrimaryDomainName(coreController cpCore) {
            return cpCore.serverConfig.appConfig.domainList[0];
        }
        //
        //====================================================================================================
        //
        private string GetEmailTemplate(coreController cpCore, int EmailTemplateID) {
            string tempGetEmailTemplate = "";
            try {
                //
                // Get the Template
                //
                if (EmailTemplateID != 0) {
                    int CS = cpCore.db.cs_openContentRecord("Email Templates", EmailTemplateID, 0, false, false, "BodyHTML");
                    if (cpCore.db.csOk(CS)) {
                        tempGetEmailTemplate = cpCore.db.csGet(CS, "BodyHTML");
                    }
                    cpCore.db.csClose(ref CS);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "trap error", "App.EXEName", "ProcessEmailClass", "GetEmailTemplate", Err.Number, Err.Source, Err.Description, True, True, "")
            }
            return tempGetEmailTemplate;
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
        private void SendConfirmationEmail(coreController cpCore, int ConfirmationMemberID, int EmailDropID, string EmailTemplate, bool EmailAllowLinkEID, string PrimaryLink, string EmailSubject, string emailBody, string emailStyles, string EmailFrom, string EmailStatusList) {
            try {
                personModel person = personModel.create(cpCore, ConfirmationMemberID);
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
                    emailController.sendPerson(cpCore, person, EmailFrom, "Email confirmation from " + GetPrimaryDomainName(cpCore), ConfirmBody,true, true, EmailDropID, EmailTemplate, EmailAllowLinkEID, ref sendStatus, queryStringForLinkAppend);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
    }
}