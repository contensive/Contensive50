
Option Strict On
Option Explicit On

Imports Contensive
Imports Contensive.Core
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController


Namespace Contensive.Core
    Public Class processEmailClass
        '
        Private cpCore As coreClass
        '
        ' copy of object from csv
        '
        'Private Enum AddonContextEnum
        '    ' should have been addonContextPage, etc
        '    ContextPage = 1
        '    ContextAdmin = 2
        '    ContextTemplate = 3
        '    contextEmail = 4
        '    ContextRemoteMethod = 5
        '    ContextOnNewVisit = 6
        '    ContextOnPageEnd = 7
        '    ContextOnPageStart = 8
        '    ContextEditor = 9
        '    ContextHelpUser = 10
        '    ContextHelpAdmin = 11
        '    ContextHelpDeveloper = 12
        '    ContextOnContentChange = 13
        '    ContextFilter = 14
        '    ContextSimple = 15
        '    ContextOnBodyStart = 16
        '    ContextOnBodyEnd = 17
        'End Enum
        '
        '
        '
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            cpCore = Me.cpCore
        End Sub
        '
        '==========================================================================================
        '   Process Group and Conditional Email
        '==========================================================================================
        '
        Public Sub ProcessEmail()
            '
            On Error GoTo ErrorTrap
            '
            Dim SMTPHandler As smtpController
            'Dim AppService As appServicesClass
            'Dim KernelService As KernelServicesClass
            Dim EmailHandlerFolder As String
            '
            'KernelService = New KernelServicesClass
            If (False) Then
                '
                ' log error and exit
                '
                logController.appendLogWithLegacyRow(cpCore, "server", "KernelService nothing after new KernelServicesClass", "App.EXEName", "ProcessEmailClass", "ProcessEmail", Err.Number, Err.Source, Err.Description, False, True, "", "", "")
            Else
                ' !!!!! THIS WILL BE AN ADDON THAT RUNS FOR EACH ADDON
                'For Each AppService In KernelService.AppServices
                '    If cpCore.app.status = ApplicationStatusRunning Then
                '        Call ProcessEmailForApp(AppService)
                '    End If
                'Next
                'AppService = Nothing
                '
                ' ----- check for email in the send queue
                '
                EmailHandlerFolder = "EmailOut\"
                SMTPHandler = New smtpController(cpCore)
                Call SMTPHandler.SendEmailQueue(EmailHandlerFolder)
                SMTPHandler = Nothing
            End If
            'KernelService = Nothing
            '
            Exit Sub
ErrorTrap:
            throw (New ApplicationException("Unexpected exception")) 'cpCore.handleLegacyError3("unknown", "trap error", "App.EXEName", "ProcessEmailClass", "ProcessEmail", Err.Number, Err.Source, Err.Description, True, True, "")
            Err.Clear()
        End Sub
        '
        '==========================================================================================
        '   Process Group and Conditional Email
        '==========================================================================================
        '
        Private Sub ProcessEmailForApp()
            Dim EmailServiceLastCheck As Date
            Dim IsNewHour As Boolean
            Dim IsNewDay As Boolean
            Dim appConfig As Core.Models.Entity.serverConfigModel.appConfigModel = cpCore.serverConfig.appConfig
            '
            If (appConfig.appStatus = Models.Entity.serverConfigModel.appStatusEnum.ready) And (appConfig.appMode = Models.Entity.serverConfigModel.appModeEnum.normal) Then
                Using cp As New CPClass(appConfig.name)
                    cpCore.db.sqlCommandTimeout = 120
                    EmailServiceLastCheck = (cpCore.siteProperties.getDate("EmailServiceLastCheck"))
                    Call cpCore.siteProperties.setProperty("EmailServiceLastCheck", CStr(Now()))
                    IsNewHour = (Now - EmailServiceLastCheck).TotalHours > 1
                    IsNewDay = EmailServiceLastCheck.Date <> Now().Date
                    '
                    ' Send Submitted Group Email (submitted, not sent, no conditions)
                    '
                    Call ProcessEmail_GroupEmail(cpCore.siteProperties.dataBuildVersion)
                    '
                    ' Send Conditional Email - Offset days after Joining
                    '
                    Call ProcessEmail_ConditionalEmail(cpCore.siteProperties.dataBuildVersion, IsNewHour, IsNewDay)
                End Using
            End If
        End Sub
        '
        '==========================================================================================
        '   Process Group Email
        '       Opens all email records, and calls send with each email, each person
        '==========================================================================================
        '
        Private Sub ProcessEmail_GroupEmail(ByVal ignore_buildversion As String)
            On Error GoTo ErrorTrap
            '
            Dim siteStyles As String
            Dim ScheduleDate As Date
            Dim EmailCopy As String
            Dim SQL As String
            Dim CSEmail As Integer
            Dim CSPeople As Integer
            Dim SQLDateNow As String
            Dim emailID As Integer
            Dim Criteria As String
            Dim CSDrop As Integer
            Dim EmailDropID As Integer
            Dim PeopleID As Integer
            Dim Confirmation As String
            Dim ConfirmationMemberID As Integer
            Dim EmailSubject As String
            Dim EmailBody As String
            Dim EmailStatusList As String
            Dim EmailMemberID As Integer
            Dim SQLTablePeople As String
            Dim SQLTableMemberRules As String
            Dim SQLTableGroups As String
            Dim BounceAddress As String
            Dim EmailTemplate As String
            Dim PrimaryLink As String
            Dim EmailAddLinkEID As Boolean
            Dim ClickFlagQuery As String
            Dim FieldList As String
            Dim EmailTemplateID As Integer
            Dim emailStyles As String
            Dim EmailFrom As String
            '
            SQLDateNow = cpCore.db.encodeSQLDate(Now)
            PrimaryLink = "http://" & cpCore.serverConfig.appConfig.domainList(0)
            '
            ' Open the email records
            '
            If True Then
                FieldList = "TestMemberID,ToAll,ScheduleDate,Sent,ModifiedBy,AddLinkEID,AllowSpamFooter,CopyFilename,Subject,FromAddress,EmailTemplateID,BlockSiteStyles,stylesFilename"
                'FieldList = "TestMemberID,ToAll,ScheduleDate,Sent,ModifiedBy,AddLinkEID,AllowSpamFooter,CopyFilename,Subject,FromAddress,EmailTemplateID"
                'FieldList = "TestMemberID,ToAll,ScheduleDate,Sent,ModifiedBy,AddLinkEID,AllowSpamFooter,CopyFilename,Subject,FromAddress,InlineStyles,EmailTemplateID"
            Else
                FieldList = "TestMemberID,ToAll,ScheduleDate,Sent,ModifiedBy,AddLinkEID,AllowSpamFooter,CopyFilename,Subject,FromAddress,EmailTemplateID,0 as BlockSiteStyles,'' as stylesFilename"
                'FieldList = "TestMemberID,ToAll,ScheduleDate,Sent,ModifiedBy,AddLinkEID,AllowSpamFooter,CopyFilename,Subject,FromAddress,EmailTemplateID"
                'FieldList = "TestMemberID,ToAll,ScheduleDate,Sent,ModifiedBy,AddLinkEID,AllowSpamFooter,CopyFilename,Subject,FromAddress,InlineStyles,EmailTemplateID"
            End If
            Criteria = "(ccemail.active<>0)" _
                & " and (ccemail.Sent=0)" _
                & " and (ccemail.submitted<>0)" _
                & " and ((ccemail.scheduledate is null)or(ccemail.scheduledate<" & SQLDateNow & "))" _
                & " and ((ccemail.ConditionID is null)OR(ccemail.ConditionID=0))" _
                & ""
            CSEmail = cpCore.db.cs_open("Email", Criteria, , , , , , FieldList)
            If cpCore.db.cs_ok(CSEmail) Then
                '
                SQLTablePeople = cpCore.metaData.getContentTablename("People")
                SQLTableMemberRules = cpCore.metaData.getContentTablename("Member Rules")
                SQLTableGroups = cpCore.metaData.getContentTablename("Groups")
                BounceAddress = cpCore.siteProperties.getText("EmailBounceAddress", "")
                siteStyles = cpCore.html.html_getStyleSheet2(0, 0)
                '
                Do While cpCore.db.cs_ok(CSEmail)
                    emailID = cpCore.db.cs_getInteger(CSEmail, "ID")
                    EmailMemberID = cpCore.db.cs_getInteger(CSEmail, "ModifiedBy")
                    EmailTemplateID = cpCore.db.cs_getInteger(CSEmail, "EmailTemplateID")
                    EmailTemplate = GetEmailTemplate(EmailTemplateID)
                    EmailAddLinkEID = cpCore.db.cs_getBoolean(CSEmail, "AddLinkEID")
                    'exclusiveStyles = cpCore.asv.csv_cs_getText(CSEmail, "exclusiveStyles")
                    EmailFrom = cpCore.db.cs_getText(CSEmail, "FromAddress")
                    EmailSubject = cpCore.db.cs_getText(CSEmail, "Subject")
                    emailStyles = cpCore.email.getStyles(emailID)
                    '
                    ' Mark this email sent and go to the next
                    '
                    Call cpCore.db.cs_set(CSEmail, "sent", True)
                    Call cpCore.db.cs_save2(CSEmail)
                    '
                    ' Create Drop Record
                    '
                    CSDrop = cpCore.db.cs_insertRecord("Email Drops", EmailMemberID)
                    If cpCore.db.cs_ok(CSDrop) Then
                        EmailDropID = cpCore.db.cs_getInteger(CSDrop, "ID")
                        ScheduleDate = cpCore.db.cs_getDate(CSEmail, "ScheduleDate")
                        If ScheduleDate < CDate("1/1/2000") Then
                            ScheduleDate = CDate("1/1/2000")
                        End If
                        Call cpCore.db.cs_set(CSDrop, "Name", "Drop " & EmailDropID & " - Scheduled for " & FormatDateTime(ScheduleDate, vbShortDate) & " " & FormatDateTime(ScheduleDate, vbShortTime))
                        Call cpCore.db.cs_set(CSDrop, "EmailID", emailID)
                        'Call cpCore.asv.csv_SetCSField(CSDrop, "CreatedBy", EmailMemberID)
                    End If
                    Call cpCore.db.cs_Close(CSDrop)
                    '
                    ' Select the people
                    '
                    If False Then
                        '
                        ' Select all people for this email
                        '
                        SQL = "select " & SQLTablePeople & ".ID as MemberID" _
                            & " From " & SQLTablePeople & "" _
                            & " where (" & SQLTablePeople & ".active<>0)" _
                            & " and (" & SQLTablePeople & ".AllowBulkEmail<>0)" _
                            & " and (" & SQLTablePeople & ".email<>'')" _
                            & " order by " & SQLTablePeople & ".email"
                    Else
                        '
                        ' Select all people in the groups for this email
                        '
                        SQL = "select Distinct " & SQLTablePeople & ".ID as MemberID," & SQLTablePeople & ".email" _
                            & " From ((((ccemail" _
                            & " left join ccEmailGroups on ccEmailGroups.EmailID=ccEmail.ID)" _
                            & " left join " & SQLTableGroups & " on " & SQLTableGroups & ".ID = ccEmailGroups.GroupID)" _
                            & " left join " & SQLTableMemberRules & " on " & SQLTableGroups & ".ID = " & SQLTableMemberRules & ".GroupID)" _
                            & " left join " & SQLTablePeople & " on " & SQLTablePeople & ".ID = " & SQLTableMemberRules & ".MemberID)" _
                            & " Where (ccEmail.ID=" & emailID & ")" _
                            & " and (" & SQLTableGroups & ".active<>0)" _
                            & " and (" & SQLTableGroups & ".AllowBulkEmail<>0)" _
                            & " and (" & SQLTablePeople & ".active<>0)" _
                            & " and (" & SQLTablePeople & ".AllowBulkEmail<>0)" _
                            & " and (" & SQLTablePeople & ".email<>'')" _
                            & " and ((" & SQLTableMemberRules & ".DateExpires is null)or(" & SQLTableMemberRules & ".DateExpires>" & SQLDateNow & "))" _
                            & " order by " & SQLTablePeople & ".email," & SQLTablePeople & ".id"
                    End If
                    CSPeople = cpCore.db.cs_openCsSql_rev("default", SQL)
                    '
                    ' Send the email to all selected people
                    '
                    Dim LastEmail As String
                    Dim Email As String
                    Dim PeopleName As String
                    EmailStatusList = ""
                    LastEmail = "empty"
                    Do While cpCore.db.cs_ok(CSPeople)
                        PeopleID = cpCore.db.cs_getInteger(CSPeople, "MemberID")
                        Email = cpCore.db.cs_getText(CSPeople, "Email")
                        If (Email = LastEmail) Then
                            PeopleName = cpCore.db.getRecordName("people", PeopleID)
                            If PeopleName = "" Then
                                PeopleName = "user #" & PeopleID
                            End If
                            EmailStatusList = EmailStatusList & "Not Sent to " & PeopleName & ", duplicate email address (" & Email & ")" & BR
                        Else
                            EmailStatusList = EmailStatusList & SendEmailRecord(PeopleID, emailID, Date.MinValue, EmailDropID, BounceAddress, EmailFrom, EmailTemplate, EmailFrom, EmailSubject, cpCore.db.cs_get(CSEmail, "CopyFilename"), cpCore.db.cs_getBoolean(CSEmail, "AllowSpamFooter"), cpCore.db.cs_getBoolean(CSEmail, "AddLinkEID"), emailStyles) & BR
                            'EmailStatusList = EmailStatusList & SendEmailRecord( PeopleID, EmailID, 0, EmailDropID, BounceAddress, EmailFrom, EmailTemplate, EmailFrom, cpCore.csv_cs_get(CSEmail, "Subject"), cpCore.csv_cs_get(CSEmail, "CopyFilename"), cpCore.csv_cs_getBoolean(CSEmail, "AllowSpamFooter"), cpCore.csv_cs_getBoolean(CSEmail, "AddLinkEID"), emailStyles) & BR
                        End If
                        LastEmail = Email
                        Call cpCore.db.cs_goNext(CSPeople)
                    Loop
                    Call cpCore.db.cs_Close(CSPeople)
                    '
                    ' Send the confirmation
                    '
                    EmailCopy = cpCore.db.cs_get(CSEmail, "copyfilename")
                    ConfirmationMemberID = cpCore.db.cs_getInteger(CSEmail, "testmemberid")
                    Call SendConfirmationEmail(ConfirmationMemberID, EmailDropID, EmailTemplate, EmailAddLinkEID, PrimaryLink, EmailSubject, EmailCopy, emailStyles, EmailFrom, EmailStatusList)
                    '            CSPeople = cpCore.asv.csOpenRecord("people", ConfirmationMemberID)
                    '            If cpCore.asv.csv_IsCSOK(CSPeople) Then
                    '                ClickFlagQuery = RequestNameEmailClickFlag & "=" & EmailDropID & "&" & RequestNameEmailMemberID & "=" & ConfirmationMemberID
                    '                EmailTemplate = cpCore.csv_EncodeContent(EmailTemplate, ConfirmationMemberID, -1, False, EmailAddLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink)
                    '                EmailSubject = cpCore.csv_EncodeContent(cpCore.csv_cs_get(CSEmail, "Subject"), ConfirmationMemberID, , True, False, False, False, False, True, , "http://" & GetPrimaryDomainName())
                    '                EmailBody = cpCore.csv_EncodeContent(cpCore.csv_cs_get(CSEmail, "CopyFilename"), ConfirmationMemberID, , False, EmailAddLinkEID, True, True, False, True, , "http://" & GetPrimaryDomainName())
                    '                'EmailFrom = cpCore.csv_cs_get(CSEmail, "FromAddress")
                    '                Confirmation = "<HTML><Head>" _
                    '                    & "<Title>Email Confirmation</Title>" _
                    '                    & "<Base href=""http://" & GetPrimaryDomainName() & cpCore.csv_RootPath & """>" _
                    '                    & emailStyles _
                    '                    & "</Head><BODY>" _
                    '                    & "The follow email has been sent" & BR & BR _
                    '                    & "Subject: " & EmailSubject & BR _
                    '                    & "From: " & EmailFrom & BR _
                    '                    & "Body" & BR _
                    '                    & "----------------------------------------------------------------------" & BR _
                    '                    & cpCore.csv_MergeTemplate(EmailTemplate, EmailBody, ConfirmationMemberID) & BR _
                    '                    & "----------------------------------------------------------------------" & BR _
                    '                    & "--- email list ---" & BR _
                    '                    & EmailStatusList _
                    '                    & "--- end email list ---" & BR _
                    '                    & "</BODY></HTML>"
                    '                Confirmation = ConvertLinksToAbsolute(Confirmation, PrimaryLink & "/")
                    '                Call cpCore.csv_SendEmail2(cpCore.asv.csv_cs_getText(CSPeople, "Email"), EmailFrom, "Email Confirmation from " & GetPrimaryDomainName(), Confirmation, "", "", , True, True)
                    '                End If
                    '            Call cpCore.asv.csv_CloseCS(CSPeople)
                    '
                    Call cpCore.db.cs_goNext(CSEmail)
                Loop
            End If
            Call cpCore.db.cs_Close(CSEmail)
            '
            Exit Sub
ErrorTrap:
            throw (New ApplicationException("Unexpected exception")) 'cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "trap error", "App.EXEName", "ProcessEmailClass", "ProcessEmail_GroupEmail", Err.Number, Err.Source, Err.Description, True, True, "")
            Err.Clear()
        End Sub
        '
        '==========================================================================================
        '
        '==========================================================================================
        '
        Private Sub ProcessEmail_ConditionalEmail(ByVal ignore_buildversion As String, ByVal IsNewHour As Boolean, ByVal IsNewDay As Boolean)
            '
            On Error GoTo ErrorTrap
            '
            Dim EmailAddLinkEID As Boolean
            Dim EmailSubject As String
            Dim EmailCopy As String
            Dim EmailStatus As String
            Dim SMTPHandler As Object
            Dim AppService As Object
            Dim KernelService As Object
            Dim CSConnection As Object
            Dim SQL As String
            Dim CSEmailBig As Integer
            Dim CSEmail As Integer
            Dim CSPeople As Integer
            Dim emailID As Integer
            Dim Criteria As String
            Dim CSDrop As Integer
            Dim EmailDropID As Integer
            Dim CSLog As Integer
            Dim DataBuild As String
            Dim PeopleID As Integer
            Dim Confirmation As String
            Dim ConfirmationMemberID As Integer
            Dim SQLTablePeople As String
            Dim SQLTableMemberRules As String
            Dim SQLTableGroups As String
            Dim BounceAddress As String
            Dim EmailTemplateID As Integer
            Dim EmailTemplate As String
            Dim FieldList As String
            Dim FromAddress As String
            Dim EmailBody As String
            Dim emailStyles As String
            Dim EmailMemberID As Integer
            Dim EmailDateExpires As Date
            Dim siteStyles As String
            Dim rightNow As Date
            Dim rightNowDate As Date
            Dim yesterdayDate As Date
            Dim tomorrowDate As Date
            Dim SQLDateNow As String
            Dim dataSourceType As Integer
            Dim sqlDateTest As String
            '
            dataSourceType = cpCore.db.getDataSourceType("default")
            SQLTablePeople = cpCore.metaData.getContentTablename("People")
            SQLTableMemberRules = cpCore.metaData.getContentTablename("Member Rules")
            SQLTableGroups = cpCore.metaData.getContentTablename("Groups")
            BounceAddress = cpCore.siteProperties.getText("EmailBounceAddress", "")
            siteStyles = cpCore.html.html_getStyleSheet2(0, 0)
            '
            rightNow = DateTime.Now()
            rightNowDate = rightNow.Date



            SQLDateNow = cpCore.db.encodeSQLDate(Now)
            '
            ' Send Conditional Email - Offset days after Joining
            '   sends email between the condition period date and date +1. if a conditional email is setup and there are already
            '   peope in the group, they do not get the email if they are past the one day threshhold.
            '   To keep them from only getting one, the log is used for the one day.
            '   Housekeep logs far > 1 day
            '
            If IsNewDay Then
                If True Then
                    FieldList = "ccEmail.TestMemberID AS TestMemberID,ccEmail.ID as EmailID," & SQLTablePeople & ".ID AS MemberID, " & SQLTableMemberRules & ".DateExpires AS DateExpires,ccEmail.BlockSiteStyles,ccEmail.stylesFilename"
                Else
                    FieldList = "ccEmail.TestMemberID AS TestMemberID,ccEmail.ID as EmailID," & SQLTablePeople & ".ID AS MemberID, " & SQLTableMemberRules & ".DateExpires AS DateExpires,0 as BlockSiteStyles,'' as stylesFilename"
                End If
                If dataSourceType = DataSourceTypeODBCSQLServer Then
                    sqlDateTest = "" _
                        & " AND (CAST(" & SQLTableMemberRules & ".DateAdded as datetime)+ccEmail.ConditionPeriod < " & SQLDateNow & ")" _
                        & " AND (CAST(" & SQLTableMemberRules & ".DateAdded as datetime)+ccEmail.ConditionPeriod+1.0 > " & SQLDateNow & ")" _
                        & ""
                Else
                    sqlDateTest = "" _
                        & " AND (" & SQLTableMemberRules & ".DateAdded+ccEmail.ConditionPeriod < " & SQLDateNow & ")" _
                        & " AND (" & SQLTableMemberRules & ".DateAdded+ccEmail.ConditionPeriod+1.0 > " & SQLDateNow & ")" _
                        & ""
                End If
                SQL = "SELECT Distinct " & FieldList _
                    & " FROM ((((ccEmail" _
                    & " LEFT JOIN ccEmailGroups ON ccEmail.ID = ccEmailGroups.EmailID)" _
                    & " LEFT JOIN " & SQLTableGroups & " ON ccEmailGroups.GroupID = " & SQLTableGroups & ".ID)" _
                    & " LEFT JOIN " & SQLTableMemberRules & " ON " & SQLTableGroups & ".ID = " & SQLTableMemberRules & ".GroupID)" _
                    & " LEFT JOIN " & SQLTablePeople & " ON " & SQLTableMemberRules & ".MemberID = " & SQLTablePeople & ".ID)" _
                    & " Where (ccEmail.id Is Not Null)" _
                    & sqlDateTest _
                    & " AND (ccEmail.ConditionExpireDate > " & SQLDateNow & " OR ccEmail.ConditionExpireDate IS NULL)" _
                    & " AND (ccEmail.ScheduleDate < " & SQLDateNow & " OR ccEmail.ScheduleDate IS NULL)" _
                    & " AND (ccEmail.Submitted <> 0)" _
                    & " AND (ccEmail.ConditionID = 2)" _
                    & " AND (ccEmail.ConditionPeriod IS NOT NULL)" _
                    & " AND (" & SQLTableGroups & ".Active <> 0)" _
                    & " AND (" & SQLTableGroups & ".AllowBulkEmail <> 0)" _
                    & " AND (" & SQLTablePeople & ".ID IS NOT NULL)" _
                    & " AND (" & SQLTablePeople & ".Active <> 0)" _
                    & " AND (" & SQLTablePeople & ".AllowBulkEmail <> 0)" _
                    & " AND (ccEmail.ID Not In (Select ccEmailLog.EmailID from ccEmailLog where ccEmailLog.MemberID=" & SQLTablePeople & ".ID))"
                CSEmailBig = cpCore.db.cs_openCsSql_rev("Default", SQL)
                Do While cpCore.db.cs_ok(CSEmailBig)
                    emailID = cpCore.db.cs_getInteger(CSEmailBig, "EmailID")
                    EmailMemberID = cpCore.db.cs_getInteger(CSEmailBig, "MemberID")
                    EmailDateExpires = cpCore.db.cs_getDate(CSEmailBig, "DateExpires")
                    CSEmail = cpCore.db.cs_openContentRecord("Conditional Email", emailID)
                    If cpCore.db.cs_ok(CSEmail) Then
                        EmailTemplateID = cpCore.db.cs_getInteger(CSEmail, "EmailTemplateID")
                        EmailTemplate = GetEmailTemplate(EmailTemplateID)
                        FromAddress = cpCore.db.cs_getText(CSEmail, "FromAddress")
                        ConfirmationMemberID = cpCore.db.cs_getInteger(CSEmail, "testmemberid")
                        EmailAddLinkEID = cpCore.db.cs_getBoolean(CSEmail, "AddLinkEID")
                        EmailSubject = cpCore.db.cs_get(CSEmail, "Subject")
                        EmailCopy = cpCore.db.cs_get(CSEmail, "CopyFilename")
                        emailStyles = cpCore.email.getStyles(emailID)
                        EmailStatus = SendEmailRecord(EmailMemberID, emailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, EmailSubject, EmailCopy, cpCore.db.cs_getBoolean(CSEmail, "AllowSpamFooter"), EmailAddLinkEID, emailStyles)
                        'EmailStatus = SendEmailRecord( EmailMemberID, EmailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, EmailSubject, EmailCopy, cpCore.csv_cs_getBoolean(CSEmail, "AllowSpamFooter"), EmailAddLinkEID, EmailInlineStyles)
                        Call SendConfirmationEmail(ConfirmationMemberID, EmailDropID, EmailTemplate, EmailAddLinkEID, "", EmailSubject, EmailCopy, emailStyles, FromAddress, EmailStatus & "<BR>")
                    End If
                    Call cpCore.db.cs_Close(CSEmail)
                    Call cpCore.db.cs_goNext(CSEmailBig)
                Loop
                Call cpCore.db.cs_Close(CSEmailBig)
            End If
            '
            ' Send Conditional Email - Offset days Before Expiration
            '
            If IsNewDay Then

                If True Then
                    FieldList = "ccEmail.TestMemberID AS TestMemberID,ccEmail.ID AS EmailID, " & SQLTablePeople & ".ID AS MemberID, " & SQLTableMemberRules & ".DateExpires AS DateExpires,ccEmail.BlockSiteStyles,ccEmail.stylesFilename"
                Else
                    FieldList = "ccEmail.TestMemberID AS TestMemberID,ccEmail.ID AS EmailID, " & SQLTablePeople & ".ID AS MemberID, " & SQLTableMemberRules & ".DateExpires AS DateExpires,0 as BlockSiteStyles,'' as stylesFilename"
                End If
                If dataSourceType = DataSourceTypeODBCSQLServer Then
                    sqlDateTest = "" _
                        & " AND (CAST(" & SQLTableMemberRules & ".DateExpires as datetime)-ccEmail.ConditionPeriod > " & SQLDateNow & ")" _
                        & " AND (CAST(" & SQLTableMemberRules & ".DateExpires as datetime)-ccEmail.ConditionPeriod-1.0 < " & SQLDateNow & ")" _
                        & ""
                Else
                    sqlDateTest = "" _
                        & " AND (" & SQLTableMemberRules & ".DateExpires-ccEmail.ConditionPeriod > " & SQLDateNow & ")" _
                        & " AND (" & SQLTableMemberRules & ".DateExpires-ccEmail.ConditionPeriod-1.0 < " & SQLDateNow & ")" _
                        & ""
                End If
                SQL = "SELECT DISTINCT " & FieldList _
                    & " FROM ((((ccEmail" _
                    & " LEFT JOIN ccEmailGroups ON ccEmail.ID = ccEmailGroups.EmailID)" _
                    & " LEFT JOIN " & SQLTableGroups & " ON ccEmailGroups.GroupID = " & SQLTableGroups & ".ID)" _
                    & " LEFT JOIN " & SQLTableMemberRules & " ON " & SQLTableGroups & ".ID = " & SQLTableMemberRules & ".GroupID)" _
                    & " LEFT JOIN " & SQLTablePeople & " ON " & SQLTableMemberRules & ".MemberID = " & SQLTablePeople & ".ID)" _
                    & " Where (ccEmail.id Is Not Null)" _
                    & sqlDateTest _
                    & " AND (ccEmail.ConditionExpireDate > " & SQLDateNow & " OR ccEmail.ConditionExpireDate IS NULL)" _
                    & " AND (ccEmail.ScheduleDate < " & SQLDateNow & " OR ccEmail.ScheduleDate IS NULL)" _
                    & " AND (ccEmail.Submitted <> 0)" _
                    & " AND (ccEmail.ConditionID = 1)" _
                    & " AND (ccEmail.ConditionPeriod IS NOT NULL)" _
                    & " AND (" & SQLTableGroups & ".Active <> 0)" _
                    & " AND (" & SQLTableGroups & ".AllowBulkEmail <> 0)" _
                    & " AND (" & SQLTablePeople & ".ID IS NOT NULL)" _
                    & " AND (" & SQLTablePeople & ".Active <> 0)" _
                    & " AND (" & SQLTablePeople & ".AllowBulkEmail <> 0)" _
                    & " AND (ccEmail.ID Not In (Select ccEmailLog.EmailID from ccEmailLog where ccEmailLog.MemberID=" & SQLTablePeople & ".ID))"
                CSEmailBig = cpCore.db.cs_openCsSql_rev("Default", SQL)
                Do While cpCore.db.cs_ok(CSEmailBig)
                    emailID = cpCore.db.cs_getInteger(CSEmailBig, "EmailID")
                    EmailMemberID = cpCore.db.cs_getInteger(CSEmailBig, "MemberID")
                    EmailDateExpires = cpCore.db.cs_getDate(CSEmailBig, "DateExpires")
                    CSEmail = cpCore.db.cs_openContentRecord("Conditional Email", emailID)
                    If cpCore.db.cs_ok(CSEmail) Then
                        EmailTemplateID = cpCore.db.cs_getInteger(CSEmail, "EmailTemplateID")
                        EmailTemplate = GetEmailTemplate(EmailTemplateID)
                        FromAddress = cpCore.db.cs_getText(CSEmail, "FromAddress")
                        ConfirmationMemberID = cpCore.db.cs_getInteger(CSEmail, "testmemberid")
                        EmailAddLinkEID = cpCore.db.cs_getBoolean(CSEmail, "AddLinkEID")
                        EmailSubject = cpCore.db.cs_get(CSEmail, "Subject")
                        EmailCopy = cpCore.db.cs_get(CSEmail, "CopyFilename")
                        emailStyles = cpCore.email.getStyles(emailID)
                        EmailStatus = SendEmailRecord(EmailMemberID, emailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, cpCore.db.cs_get(CSEmail, "Subject"), cpCore.db.cs_get(CSEmail, "CopyFilename"), cpCore.db.cs_getBoolean(CSEmail, "AllowSpamFooter"), cpCore.db.cs_getBoolean(CSEmail, "AddLinkEID"), emailStyles)
                        'EmailStatus = SendEmailRecord( EmailMemberID, EmailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, cpCore.csv_cs_get(CSEmail, "Subject"), cpCore.csv_cs_get(CSEmail, "CopyFilename"), cpCore.csv_cs_getBoolean(CSEmail, "AllowSpamFooter"), cpCore.csv_cs_getBoolean(CSEmail, "AddLinkEID"), EmailInlineStyles)
                        Call SendConfirmationEmail(ConfirmationMemberID, EmailDropID, EmailTemplate, EmailAddLinkEID, "", EmailSubject, EmailCopy, emailStyles, FromAddress, EmailStatus & "<BR>")
                    End If
                    Call cpCore.db.cs_Close(CSEmail)
                    Call cpCore.db.cs_goNext(CSEmailBig)
                Loop
                Call cpCore.db.cs_Close(CSEmailBig)
            End If
            '
            ' Send Conditional Email - Birthday
            '
            If IsNewDay Then
                If True Then
                    If True Then
                        FieldList = "ccEmail.TestMemberID AS TestMemberID,ccEmail.ID AS EmailID, " & SQLTablePeople & ".ID AS MemberID, " & SQLTableMemberRules & ".DateExpires AS DateExpires,ccEmail.BlockSiteStyles,ccEmail.stylesFilename"
                    Else
                        FieldList = "ccEmail.TestMemberID AS TestMemberID,ccEmail.ID AS EmailID, " & SQLTablePeople & ".ID AS MemberID, " & SQLTableMemberRules & ".DateExpires AS DateExpires,0 as BlockSiteStyles,'' as stylesFilename"
                    End If
                    SQL = "SELECT DISTINCT " & FieldList _
                        & " FROM ((((ccEmail" _
                        & " LEFT JOIN ccEmailGroups ON ccEmail.ID = ccEmailGroups.EmailID)" _
                        & " LEFT JOIN " & SQLTableGroups & " ON ccEmailGroups.GroupID = " & SQLTableGroups & ".ID)" _
                        & " LEFT JOIN " & SQLTableMemberRules & " ON " & SQLTableGroups & ".ID = " & SQLTableMemberRules & ".GroupID)" _
                        & " LEFT JOIN " & SQLTablePeople & " ON " & SQLTableMemberRules & ".MemberID = " & SQLTablePeople & ".ID)" _
                        & " Where (ccEmail.id Is Not Null)" _
                        & " AND (ccEmail.ConditionExpireDate > " & SQLDateNow & " OR ccEmail.ConditionExpireDate IS NULL)" _
                        & " AND (ccEmail.ScheduleDate < " & SQLDateNow & " OR ccEmail.ScheduleDate IS NULL)" _
                        & " AND (ccEmail.Submitted <> 0)" _
                        & " AND (ccEmail.ConditionID = 3)" _
                        & " AND (" & SQLTableGroups & ".Active <> 0)" _
                        & " AND (" & SQLTableGroups & ".AllowBulkEmail <> 0)" _
                        & " AND ((" & SQLTableMemberRules & ".DateExpires is null)or(" & SQLTableMemberRules & ".DateExpires > " & SQLDateNow & "))" _
                        & " AND (" & SQLTablePeople & ".ID IS NOT NULL)" _
                        & " AND (" & SQLTablePeople & ".Active <> 0)" _
                        & " AND (" & SQLTablePeople & ".AllowBulkEmail <> 0)" _
                        & " AND (" & SQLTablePeople & ".BirthdayMonth=" & Month(Now) & ")" _
                        & " AND (" & SQLTablePeople & ".BirthdayDay=" & Day(Now) & ")" _
                        & " AND (ccEmail.ID Not In (Select ccEmailLog.EmailID from ccEmailLog where ccEmailLog.MemberID=" & SQLTablePeople & ".ID and ccEmailLog.DateAdded>=" & cpCore.db.encodeSQLDate(Now.Date) & "))"
                    CSEmailBig = cpCore.db.cs_openCsSql_rev("Default", SQL)
                    Do While cpCore.db.cs_ok(CSEmailBig)
                        emailID = cpCore.db.cs_getInteger(CSEmailBig, "EmailID")
                        EmailMemberID = cpCore.db.cs_getInteger(CSEmailBig, "MemberID")
                        EmailDateExpires = cpCore.db.cs_getDate(CSEmailBig, "DateExpires")
                        CSEmail = cpCore.db.cs_openContentRecord("Conditional Email", emailID)
                        If cpCore.db.cs_ok(CSEmail) Then
                            EmailTemplateID = cpCore.db.cs_getInteger(CSEmail, "EmailTemplateID")
                            EmailTemplate = GetEmailTemplate(EmailTemplateID)
                            FromAddress = cpCore.db.cs_getText(CSEmail, "FromAddress")
                            ConfirmationMemberID = cpCore.db.cs_getInteger(CSEmail, "testmemberid")
                            EmailAddLinkEID = cpCore.db.cs_getBoolean(CSEmail, "AddLinkEID")
                            EmailSubject = cpCore.db.cs_get(CSEmail, "Subject")
                            EmailCopy = cpCore.db.cs_get(CSEmail, "CopyFilename")
                            emailStyles = cpCore.email.getStyles(emailID)
                            EmailStatus = SendEmailRecord(EmailMemberID, emailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, cpCore.db.cs_get(CSEmail, "Subject"), cpCore.db.cs_get(CSEmail, "CopyFilename"), cpCore.db.cs_getBoolean(CSEmail, "AllowSpamFooter"), cpCore.db.cs_getBoolean(CSEmail, "AddLinkEID"), emailStyles)
                            'EmailStatus = SendEmailRecord( EmailMemberID, EmailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, cpCore.csv_cs_get(CSEmail, "Subject"), cpCore.csv_cs_get(CSEmail, "CopyFilename"), cpCore.csv_cs_getBoolean(CSEmail, "AllowSpamFooter"), cpCore.csv_cs_getBoolean(CSEmail, "AddLinkEID"), EmailInlineStyles)
                            Call SendConfirmationEmail(ConfirmationMemberID, EmailDropID, EmailTemplate, EmailAddLinkEID, "", EmailSubject, EmailCopy, emailStyles, FromAddress, EmailStatus & "<BR>")
                        End If
                        Call cpCore.db.cs_Close(CSEmail)
                        Call cpCore.db.cs_goNext(CSEmailBig)
                    Loop
                    Call cpCore.db.cs_Close(CSEmailBig)
                End If
            End If
            '
            Exit Sub
ErrorTrap:
            throw (New ApplicationException("Unexpected exception")) 'cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "trap error", "App.EXEName", "ProcessEmailClass", "ProcessEmail_ConditionalEmail", Err.Number, Err.Source, Err.Description, True, True, "")
            Err.Clear()
        End Sub
        '
        '=======================================================================================
        ''' <summary>
        ''' Send email to a memberid
        ''' </summary>
        ''' <param name="MemberID"></param>
        ''' <param name="emailID"></param>
        ''' <param name="DateBlockExpires"></param>
        ''' <param name="EmailDropID"></param>
        ''' <param name="BounceAddress"></param>
        ''' <param name="ReplyToAddress"></param>
        ''' <param name="EmailTemplate"></param>
        ''' <param name="FromAddress"></param>
        ''' <param name="EmailSubject"></param>
        ''' <param name="EmailBody"></param>
        ''' <param name="AllowSpamFooter"></param>
        ''' <param name="EmailAllowLinkEID"></param>
        ''' <param name="emailStyles"></param>
        ''' <returns>OK if successful, else returns user error.</returns>
        Private Function SendEmailRecord(ByVal MemberID As Integer, ByVal emailID As Integer, ByVal DateBlockExpires As Date, ByVal EmailDropID As Integer, ByVal BounceAddress As String, ByVal ReplyToAddress As String, ByVal EmailTemplate As String, ByVal FromAddress As String, ByVal EmailSubject As String, ByVal EmailBody As String, ByVal AllowSpamFooter As Boolean, ByVal EmailAllowLinkEID As Boolean, ByVal emailStyles As String) As String
            Dim returnStatus As String = ""
            Dim CSPeople As Integer
            Dim CSLog As Integer
            Try
                '
                'Dim CS as integer
                'Dim EmailFrom As String
                'Dim HTMLHead As String
                Dim ServerPageDefault As String
                Dim EmailToName As String
                Dim ClickFlagQuery As String
                Dim EmailStatus As String
                'Dim FieldList As String
                'Dim InlineStyles As String
                Dim emailWorkingStyles As String
                Dim RootURL As String
                Dim PrimaryLink As String
                Dim ToAddress As String
                'Dim ToAddressName As String
                Dim EmailBodyEncoded As String
                Dim EmailSubjectEncoded As String
                '
                Dim errorMessage As String = ""
                Dim EmailTemplateEncoded As String = ""
                Dim OpenTriggerCode As String = ""
                '
                EmailBodyEncoded = EmailBody
                EmailSubjectEncoded = EmailSubject
                'buildversion = cpCore.app.dataBuildVersion
                CSLog = cpCore.db.cs_insertRecord("Email Log", 0)
                If cpCore.db.cs_ok(CSLog) Then
                    Call cpCore.db.cs_set(CSLog, "Name", "Sent " & CStr(Now()))
                    Call cpCore.db.cs_set(CSLog, "EmailDropID", EmailDropID)
                    Call cpCore.db.cs_set(CSLog, "EmailID", emailID)
                    Call cpCore.db.cs_set(CSLog, "MemberID", MemberID)
                    Call cpCore.db.cs_set(CSLog, "LogType", EmailLogTypeDrop)
                    Call cpCore.db.cs_set(CSLog, "DateBlockExpires", DateBlockExpires)
                    Call cpCore.db.cs_set(CSLog, "SendStatus", "Send attempted but not completed")
                    If True Then
                        Call cpCore.db.cs_set(CSLog, "fromaddress", FromAddress)
                        Call cpCore.db.cs_set(CSLog, "Subject", EmailSubject)
                    End If
                    Call cpCore.db.cs_save2(CSLog)
                    '
                    ' Get the Template
                    '
                    PrimaryLink = "http://" & GetPrimaryDomainName()
                    '
                    ' Get the Member
                    '
                    CSPeople = cpCore.db.cs_openContentRecord("People", MemberID, , , , "Email,Name")
                    If cpCore.db.cs_ok(CSPeople) Then
                        ToAddress = cpCore.db.cs_get(CSPeople, "Email")
                        EmailToName = cpCore.db.cs_get(CSPeople, "Name")
                        ServerPageDefault = cpCore.siteProperties.getText(siteproperty_serverPageDefault_name, siteproperty_serverPageDefault_defaultValue)
                        RootURL = PrimaryLink & requestAppRootPath
                        If EmailDropID <> 0 Then
                            Select Case (cpCore.siteProperties.getinteger("GroupEmailOpenTriggerMethod", 0))
                                Case 1
                                    OpenTriggerCode = "<link rel=""stylesheet"" type=""text/css"" href=""" & RootURL & ServerPageDefault & "?" & RequestNameEmailOpenCssFlag & "=" & EmailDropID & "&" & RequestNameEmailMemberID & "=#member_id#"">"
                                Case Else
                                    OpenTriggerCode = "<img src=""" & RootURL & ServerPageDefault & "?" & RequestNameEmailOpenFlag & "=" & EmailDropID & "&" & RequestNameEmailMemberID & "=#member_id#"">"
                            End Select
                        End If
                        '
                        emailWorkingStyles = emailStyles
                        emailWorkingStyles = genericController.vbReplace(emailWorkingStyles, StyleSheetStart, StyleSheetStart & "<!-- ", 1, 99, vbTextCompare)
                        emailWorkingStyles = genericController.vbReplace(emailWorkingStyles, StyleSheetEnd, " // -->" & StyleSheetEnd, 1, 99, vbTextCompare)
                        '
                        ' Create the clickflag to be added to all anchors
                        '
                        ClickFlagQuery = RequestNameEmailClickFlag & "=" & EmailDropID & "&" & RequestNameEmailMemberID & "=" & MemberID
                        '
                        ' Encode body and subject
                        '
                        EmailBodyEncoded = cpCore.html.html_executeContentCommands(Nothing, EmailBodyEncoded, CPUtilsClass.addonContext.ContextEmail, MemberID, True, errorMessage)
                        EmailBodyEncoded = cpCore.html.html_encodeContent10(EmailBodyEncoded, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, 0, "", CPUtilsClass.addonContext.ContextEmail, True, Nothing, False)
                        'EmailBodyEncoded = cpCore.csv_EncodeContent8(Nothing, EmailBodyEncoded, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, "", 0, "", True, CPUtilsClass.addonContext.contextEmail)
                        '
                        EmailSubjectEncoded = cpCore.html.html_executeContentCommands(Nothing, EmailSubjectEncoded, CPUtilsClass.addonContext.ContextEmail, MemberID, True, errorMessage)
                        EmailSubjectEncoded = cpCore.html.html_encodeContent10(EmailSubjectEncoded, MemberID, "", 0, 0, True, False, False, False, False, True, "", PrimaryLink, True, 0, "", CPUtilsClass.addonContext.ContextEmail, True, Nothing, False)
                        'EmailSubjectEncoded = cpCore.csv_EncodeContent8(Nothing, EmailSubjectEncoded, MemberID, "", 0, 0, True, False, False, False, False, True, "", PrimaryLink, True, "", 0, "", True, CPUtilsClass.addonContext.contextEmail)
                        '
                        ' Encode/Merge Template
                        '
                        If EmailTemplate = "" Then
                            '
                            ' create 20px padding template
                            '
                            EmailBodyEncoded = "<div style=""padding:10px;"">" & EmailBodyEncoded & "</div>"
                        Else
                            '
                            ' use provided template
                            '
                            EmailTemplateEncoded = cpCore.html.html_executeContentCommands(Nothing, EmailTemplateEncoded, CPUtilsClass.addonContext.ContextEmail, MemberID, True, errorMessage)
                            EmailTemplateEncoded = cpCore.html.html_encodeContent10(EmailTemplate, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, 0, "", CPUtilsClass.addonContext.ContextEmail, True, Nothing, False)
                            'EmailTemplateEncoded = cpCore.csv_EncodeContent8(Nothing, EmailTemplate, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, "", 0, "", True, CPUtilsClass.addonContext.contextEmail)
                            'EmailTemplateEncoded = cpCore.csv_encodecontent8(Nothing, EmailTemplate, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, "", 0, ContentPlaceHolder, True, CPUtilsClass.addonContext.contextemail)
                            If genericController.vbInstr(1, EmailTemplateEncoded, fpoContentBox) <> 0 Then
                                EmailBodyEncoded = genericController.vbReplace(EmailTemplateEncoded, fpoContentBox, EmailBodyEncoded)
                            Else
                                EmailBodyEncoded = EmailTemplateEncoded & "<div style=""padding:10px;"">" & EmailBodyEncoded & "</div>"
                            End If
                            '                If genericController.vbInstr(1, EmailTemplateEncoded, ContentPlaceHolder) <> 0 Then
                            '                    EmailBodyEncoded = genericController.vbReplace(EmailTemplateEncoded, ContentPlaceHolder, EmailBodyEncoded)
                            '                Else
                            '                    EmailBodyEncoded = EmailTemplateEncoded & "<div style=""padding:10px;"">" & EmailBodyEncoded & "</div>"
                            '                End If
                        End If
                        '
                        ' Spam Footer under template
                        ' remove the marker for any other place in the email then add it as needed
                        '
                        EmailBodyEncoded = genericController.vbReplace(EmailBodyEncoded, RequestNameEmailSpamFlag, "", 1, 99, vbTextCompare)
                        If AllowSpamFooter Then
                            '
                            ' non-authorable, default true - leave it as an option in case there is an important exception
                            '
                            EmailBodyEncoded = EmailBodyEncoded & "<div style=""padding:10px;"">" & GetLinkedText("<a href=""" & RootURL & ServerPageDefault & "?" & RequestNameEmailSpamFlag & "=#member_email#&" & RequestNameEmailBlockRequestDropID & "=" & EmailDropID & """>", cpCore.siteProperties.getText("EmailSpamFooter", DefaultSpamFooter)) & "</div>"
                        End If
                        '
                        ' open trigger under footer (so it does not shake as the image comes in)
                        '
                        EmailBodyEncoded = EmailBodyEncoded & OpenTriggerCode
                        EmailBodyEncoded = genericController.vbReplace(EmailBodyEncoded, "#member_id#", MemberID)
                        EmailBodyEncoded = genericController.vbReplace(EmailBodyEncoded, "#member_email#", ToAddress)
                        '
                        ' Now convert URLS to absolute
                        '
                        EmailBodyEncoded = ConvertLinksToAbsolute(EmailBodyEncoded, RootURL)
                        '
                        EmailBodyEncoded = "" _
                            & "<HTML>" _
                            & "<Head>" _
                            & "<Title>" & EmailSubjectEncoded & "</Title>" _
                            & "<Base href=""" & RootURL & """>" _
                            & "</Head>" _
                            & "<BODY class=ccBodyEmail>" _
                            & "<Base href=""" & RootURL & """>" _
                            & emailWorkingStyles _
                            & EmailBodyEncoded _
                            & "</BODY>" _
                            & "</HTML>"
                        '
                        ' Send
                        '
                        EmailStatus = cpCore.email.send(ToAddress, FromAddress, EmailSubjectEncoded, EmailBodyEncoded, BounceAddress, ReplyToAddress, "", True, True, 0)
                        If EmailStatus = "" Then
                            EmailStatus = "ok"
                        End If
                        returnStatus = returnStatus & "Send to " & EmailToName & " at " & ToAddress & ", Status = " & EmailStatus
                        '
                        ' ----- Log the send
                        '
                        Call cpCore.db.cs_set(CSLog, "SendStatus", EmailStatus)
                        If True Then
                            Call cpCore.db.cs_set(CSLog, "toaddress", ToAddress)
                        End If
                        Call cpCore.db.cs_save2(CSLog)
                    End If
                    'Call cpCore.app.closeCS(CSPeople)
                End If
                'Call cpCore.app.closeCS(CSLog)
            Catch ex As Exception
                throw (New ApplicationException("Unexpected exception")) 'cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "trap error", "App.EXEName", "ProcessEmailClass", "SendEmailRecord", Err.Number, Err.Source, Err.Description, True, True, "")
                Err.Clear()
            Finally
                Call cpCore.db.cs_Close(CSPeople)
                Call cpCore.db.cs_Close(CSLog)
            End Try

            Return returnStatus
        End Function
        '
        '======================================================================================
        '
        '======================================================================================
        '
        Private Function GetPrimaryDomainName() As String
            Return cpCore.serverConfig.appConfig.domainList(0)
        End Function
        '
        '
        '
        Private Function GetEmailTemplate(ByVal EmailTemplateID As Integer) As String
            GetEmailTemplate = ""
            On Error GoTo ErrorTrap
            '
            Dim CS As Integer
            '
            ' Get the Template
            '
            If EmailTemplateID <> 0 Then
                CS = cpCore.db.cs_openContentRecord("Email Templates", EmailTemplateID, , , , "BodyHTML")
                If cpCore.db.cs_ok(CS) Then
                    GetEmailTemplate = cpCore.db.cs_get(CS, "BodyHTML")
                End If
                Call cpCore.db.cs_Close(CS)
            End If
            '
            Exit Function
            '
ErrorTrap:
            throw (New ApplicationException("Unexpected exception")) 'cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "trap error", "App.EXEName", "ProcessEmailClass", "GetEmailTemplate", Err.Number, Err.Source, Err.Description, True, True, "")
            Err.Clear()
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Send confirmation email 
        ''' </summary>
        ''' <param name="ConfirmationMemberID"></param>
        ''' <param name="EmailDropID"></param>
        ''' <param name="EmailTemplate"></param>
        ''' <param name="EmailAllowLinkEID"></param>
        ''' <param name="PrimaryLink"></param>
        ''' <param name="EmailSubject"></param>
        ''' <param name="EmailCopy"></param>
        ''' <param name="emailStyles"></param>
        ''' <param name="EmailFrom"></param>
        ''' <param name="EmailStatusList"></param>
        Private Sub SendConfirmationEmail(ByVal ConfirmationMemberID As Integer, ByVal EmailDropID As Integer, ByVal EmailTemplate As String, ByVal EmailAllowLinkEID As Boolean, ByVal PrimaryLink As String, ByVal EmailSubject As String, ByVal EmailCopy As String, ByVal emailStyles As String, ByVal EmailFrom As String, ByVal EmailStatusList As String)
            Try
                '
                Dim CSPeople As Integer
                Dim ClickFlagQuery As String
                Dim WorkingTemplate As String
                Dim ConfirmBody As String
                Dim errorMessage As String = ""
                Dim EmailBody As String = ""
                '
                CSPeople = cpCore.db.cs_openContentRecord("people", ConfirmationMemberID)
                If cpCore.db.cs_ok(CSPeople) Then
                    ClickFlagQuery = RequestNameEmailClickFlag & "=" & EmailDropID & "&" & RequestNameEmailMemberID & "=" & ConfirmationMemberID
                    '
                    EmailSubject = cpCore.html.html_executeContentCommands(Nothing, EmailSubject, CPUtilsClass.addonContext.ContextEmail, ConfirmationMemberID, True, errorMessage)
                    EmailSubject = cpCore.html.html_encodeContent10(EmailSubject, ConfirmationMemberID, "", 0, 0, True, False, False, False, False, True, "", "http://" & GetPrimaryDomainName(), True, 0, "", CPUtilsClass.addonContext.ContextEmail, True, Nothing, False)
                    'EmailSubject = cpCore.csv_EncodeContent8(Nothing, EmailSubject, ConfirmationMemberID, "", 0, 0, True, False, False, False, False, True, "", "http://" & GetPrimaryDomainName(), True, "", 0, "", True, CPUtilsClass.addonContext.contextEmail)
                    '
                    EmailBody = cpCore.html.html_executeContentCommands(Nothing, EmailBody, CPUtilsClass.addonContext.ContextEmail, ConfirmationMemberID, True, errorMessage)
                    EmailBody = cpCore.html.html_encodeContent10(EmailCopy, ConfirmationMemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, "", "http://" & GetPrimaryDomainName(), True, 0, "", CPUtilsClass.addonContext.ContextEmail, True, Nothing, False)
                    'EmailBody = cpCore.csv_EncodeContent8(Nothing, EmailCopy, ConfirmationMemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, "", "http://" & GetPrimaryDomainName(), True, "", 0, "", True, CPUtilsClass.addonContext.contextEmail)
                    '
                    ' Encode the template
                    '
                    If EmailTemplate = "" Then
                        '
                        ' create 20px padding template
                        '
                        EmailBody = "<div style=""padding:10px"">" & EmailBody & "</div>"
                    Else
                        WorkingTemplate = EmailTemplate
                        WorkingTemplate = cpCore.html.html_executeContentCommands(Nothing, WorkingTemplate, CPUtilsClass.addonContext.ContextEmail, ConfirmationMemberID, True, errorMessage)
                        WorkingTemplate = cpCore.html.html_encodeContent10(WorkingTemplate, ConfirmationMemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, "", "http://" & GetPrimaryDomainName(), True, 0, "", CPUtilsClass.addonContext.ContextEmail, True, Nothing, False)
                        'WorkingTemplate = cpCore.csv_encodecontent8(Nothing, EmailTemplate, ConfirmationMemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, False, "http://" & GetPrimaryDomainName(), True, "", 0, ContentPlaceHolder, True, CPUtilsClass.addonContext.contextemail)
                        If genericController.vbInstr(1, WorkingTemplate, fpoContentBox) <> 0 Then
                            EmailBody = genericController.vbReplace(WorkingTemplate, fpoContentBox, EmailBody)
                        Else
                            EmailBody = WorkingTemplate & "<div style=""padding:10px"">" & EmailBody & "</div>"
                        End If
                        '            If genericController.vbInstr(1, WorkingTemplate, ContentPlaceHolder) <> 0 Then
                        '                EmailBody = genericController.vbReplace(WorkingTemplate, ContentPlaceHolder, EmailBody)
                        '            Else
                        '                EmailBody = WorkingTemplate & "<div style=""padding:10px"">" & EmailBody & "</div>"
                        '            End If
                    End If
                    '
                    ConfirmBody = "<HTML><Head>" _
                    & "<Title>Email Confirmation</Title>" _
                    & "<Base href=""http://" & GetPrimaryDomainName() & requestAppRootPath & """>" _
                    & emailStyles _
                    & "</Head><BODY><div style=""padding:10px;"">" _
                    & "The follow email has been sent." & BR _
                    & BR _
                    & "If this email includes personalization, each email sent was personalized to its recipient. This confirmation has been personalized to you." & BR _
                    & BR _
                    & "Subject: " & EmailSubject & BR _
                    & "From: " & EmailFrom & BR _
                    & "Body" & BR _
                    & "----------------------------------------------------------------------" & BR _
                    & EmailBody & BR _
                    & "----------------------------------------------------------------------" & BR _
                    & "--- recipient list ---" & BR _
                    & EmailStatusList _
                    & "--- end of list ---" & BR _
                    & "</div></BODY></HTML>"
                    ConfirmBody = ConvertLinksToAbsolute(ConfirmBody, PrimaryLink & "/")
                    Call cpCore.email.send(cpCore.db.cs_getText(CSPeople, "Email"), EmailFrom, "Email confirmation from " & GetPrimaryDomainName(), ConfirmBody, "", "", "", True, True, 0)
                End If
                Call cpCore.db.cs_Close(CSPeople)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
    End Class
End Namespace