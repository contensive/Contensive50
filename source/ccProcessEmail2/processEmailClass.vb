
Namespace Contensive.Core
    Public Class processEmailClass
        '
        Private cpCore As cpCoreClass
        '
        ' copy of object from csv
        '
        Private Enum AddonContextEnum
            ' should have been addonContextPage, etc
            ContextPage = 1
            ContextAdmin = 2
            ContextTemplate = 3
            contextEmail = 4
            ContextRemoteMethod = 5
            ContextOnNewVisit = 6
            ContextOnPageEnd = 7
            ContextOnPageStart = 8
            ContextEditor = 9
            ContextHelpUser = 10
            ContextHelpAdmin = 11
            ContextHelpDeveloper = 12
            ContextOnContentChange = 13
            ContextFilter = 14
            ContextSimple = 15
            ContextOnBodyStart = 16
            ContextOnBodyEnd = 17
        End Enum
        '
        '
        '
        Public Sub New(cpCore As cpCoreClass)
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
            Dim SMTPHandler As coreSmtpHandlerClass
            'Dim AppService As appServicesClass
            'Dim KernelService As KernelServicesClass
            Dim EmailHandlerFolder As String
            '
            'KernelService = New KernelServicesClass
            If (False) Then
                '
                ' log error and exit
                '
                cpCore.appendLogWithLegacyRow("server", "KernelService nothing after new KernelServicesClass", "App.EXEName", "ProcessEmailClass", "ProcessEmail", Err.Number, Err.Source, Err.Description, False, True, "", "", "")
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
                SMTPHandler = New coreSmtpHandlerClass(cpCore)
                Call SMTPHandler.SendEmailQueue(EmailHandlerFolder)
                SMTPHandler = Nothing
            End If
            'KernelService = Nothing
            '
            Exit Sub
ErrorTrap:
            cpCore.handleLegacyError3("unknown", "trap error", "App.EXEName", "ProcessEmailClass", "ProcessEmail", Err.Number, Err.Source, Err.Description, True, True, "")
            Err.Clear()
        End Sub
        '
        '==========================================================================================
        '   Process Group and Conditional Email
        '==========================================================================================
        '
        Private Sub ProcessEmailForApp()
            '
            On Error GoTo ErrorTrap
            '
            'dim buildversion As String
            Dim EmailServiceLastCheck As Date
            Dim IsNewHour As Boolean
            Dim IsNewDay As Boolean
            Dim appName As String
            Dim hint As String
            Dim appStatus As Integer
            '
            'hint = "1"
            appName = cpCore.appConfig.name
            appStatus = cpCore.appStatus
            'hint = "3"
            If (appStatus = applicationStatusEnum.ApplicationStatusReady) Or (appStatus = applicationStatusEnum.ApplicationStatusUpgrading) Then
                Using cp As New CPClass(appName)
                    If True Then
                        'End If
                        '20151109 - removed but unsure what in initApp can/must be moved to cp constructor
                        'If cp.execute_initContext() Then
                        '
                        'hint = "4"
                        cpCore.db.db_SQLCommandTimeout = 120
                        EmailServiceLastCheck = EncodeDate(cpCore.siteProperties.getText("EmailServiceLastCheck", 0))
                        Call cpCore.siteProperties.setProperty("EmailServiceLastCheck", CStr(Now()))
                        'buildversion = cpCore.app.GetSiteProperty("BuildVersion", 0, 0)
                        IsNewHour = (Now - EmailServiceLastCheck).TotalHours > 1
                        IsNewDay = Int(EmailServiceLastCheck) <> Int(Now())
                        '
                        ' Send Submitted Group Email (submitted, not sent, no conditions)
                        '
                        'hint = "5"
                        Call ProcessEmail_GroupEmail(cpCore.db.dataBuildVersion)
                        '
                        ' Send Conditional Email - Offset days after Joining
                        '
                        'hint = "6"
                        Call ProcessEmail_ConditionalEmail(cpCore.db.dataBuildVersion, IsNewHour, IsNewDay)
                        '
                    End If
                End Using
            End If
            '
            Exit Sub
ErrorTrap:
            cpCore.handleLegacyError3(appName, "trap error", "App.EXEName", "ProcessEmailClass", "ProcessEmailForApp, hint=" & hint, Err.Number, Err.Source, Err.Description, True, True, "")
            Err.Clear()
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
            SQLDateNow = cpCore.db.db_EncodeSQLDate(Now)
            PrimaryLink = "http://" & GetPrimaryDomainName()
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
            CSEmail = cpCore.db.csOpen("Email", Criteria, , , , , , FieldList)
            If cpCore.db.cs_Ok(CSEmail) Then
                '
                SQLTablePeople = cpCore.metaData.getContentTablename("People")
                SQLTableMemberRules = cpCore.metaData.getContentTablename("Member Rules")
                SQLTableGroups = cpCore.metaData.getContentTablename("Groups")
                BounceAddress = cpCore.siteProperties.getText("EmailBounceAddress", "")
                siteStyles = cpCore.csv_getStyleSheetProcessed()
                '
                Do While cpCore.db.cs_Ok(CSEmail)
                    emailID = cpCore.db.cs_getInteger(CSEmail, "ID")
                    EmailMemberID = cpCore.db.cs_getInteger(CSEmail, "ModifiedBy")
                    EmailTemplateID = cpCore.db.cs_getInteger(CSEmail, "EmailTemplateID")
                    EmailTemplate = GetEmailTemplate(EmailTemplateID)
                    EmailAddLinkEID = cpCore.db.cs_getBoolean(CSEmail, "AddLinkEID")
                    'exclusiveStyles = cpCore.asv.csv_GetCSText(CSEmail, "exclusiveStyles")
                    EmailFrom = cpCore.db.cs_getText(CSEmail, "FromAddress")
                    EmailSubject = cpCore.db.cs_getText(CSEmail, "Subject")
                    emailStyles = cpCore.email_getEmailStyles(emailID)
                    '
                    ' Mark this email sent and go to the next
                    '
                    Call cpCore.db.db_SetCSField(CSEmail, "sent", True)
                    Call cpCore.db.db_SaveCSRecord(CSEmail)
                    '
                    ' Create Drop Record
                    '
                    CSDrop = cpCore.db.db_csInsertRecord("Email Drops", EmailMemberID)
                    If cpCore.db.cs_Ok(CSDrop) Then
                        EmailDropID = cpCore.db.cs_getInteger(CSDrop, "ID")
                        ScheduleDate = cpCore.db.db_GetCSDate(CSEmail, "ScheduleDate")
                        If ScheduleDate < CDate("1/1/2000") Then
                            ScheduleDate = CDate("1/1/2000")
                        End If
                        Call cpCore.db.db_SetCSField(CSDrop, "Name", "Drop " & EmailDropID & " - Scheduled for " & FormatDateTime(ScheduleDate, vbShortDate) & " " & FormatDateTime(ScheduleDate, vbShortTime))
                        Call cpCore.db.db_SetCSField(CSDrop, "EmailID", emailID)
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
                    CSPeople = cpCore.db.db_openCsSql_rev("default", SQL)
                    '
                    ' Send the email to all selected people
                    '
                    Dim LastEmail As String
                    Dim Email As String
                    Dim PeopleName As String
                    EmailStatusList = ""
                    LastEmail = "empty"
                    Do While cpCore.db.cs_Ok(CSPeople)
                        PeopleID = cpCore.db.cs_getInteger(CSPeople, "MemberID")
                        Email = cpCore.db.cs_getText(CSPeople, "Email")
                        If (Email = LastEmail) Then
                            PeopleName = cpCore.db_GetRecordName("people", PeopleID)
                            If PeopleName = "" Then
                                PeopleName = "user #" & PeopleID
                            End If
                            EmailStatusList = EmailStatusList & "Not Sent to " & PeopleName & ", duplicate email address (" & Email & ")" & BR
                        Else
                            EmailStatusList = EmailStatusList & SendEmailRecord(PeopleID, emailID, Date.MinValue, EmailDropID, BounceAddress, EmailFrom, EmailTemplate, EmailFrom, EmailSubject, cpCore.db.db_GetCS(CSEmail, "CopyFilename"), cpCore.db.cs_getBoolean(CSEmail, "AllowSpamFooter"), cpCore.db.cs_getBoolean(CSEmail, "AddLinkEID"), emailStyles) & BR
                            'EmailStatusList = EmailStatusList & SendEmailRecord( PeopleID, EmailID, 0, EmailDropID, BounceAddress, EmailFrom, EmailTemplate, EmailFrom, cpCore.csv_GetCS(CSEmail, "Subject"), cpCore.csv_GetCS(CSEmail, "CopyFilename"), cpCore.csv_GetCSBoolean(CSEmail, "AllowSpamFooter"), cpCore.csv_GetCSBoolean(CSEmail, "AddLinkEID"), emailStyles) & BR
                        End If
                        LastEmail = Email
                        Call cpCore.db.db_csGoNext(CSPeople)
                    Loop
                    Call cpCore.db.cs_Close(CSPeople)
                    '
                    ' Send the confirmation
                    '
                    EmailCopy = cpCore.db.db_GetCS(CSEmail, "copyfilename")
                    ConfirmationMemberID = cpCore.db.cs_getInteger(CSEmail, "testmemberid")
                    Call SendConfirmationEmail(ConfirmationMemberID, EmailDropID, EmailTemplate, EmailAddLinkEID, PrimaryLink, EmailSubject, EmailCopy, emailStyles, EmailFrom, EmailStatusList)
                    '            CSPeople = cpCore.asv.db_csOpenRecord("people", ConfirmationMemberID)
                    '            If cpCore.asv.csv_IsCSOK(CSPeople) Then
                    '                ClickFlagQuery = RequestNameEmailClickFlag & "=" & EmailDropID & "&" & RequestNameEmailMemberID & "=" & ConfirmationMemberID
                    '                EmailTemplate = cpCore.csv_EncodeContent(EmailTemplate, ConfirmationMemberID, -1, False, EmailAddLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink)
                    '                EmailSubject = cpCore.csv_EncodeContent(cpCore.csv_GetCS(CSEmail, "Subject"), ConfirmationMemberID, , True, False, False, False, False, True, , "http://" & GetPrimaryDomainName())
                    '                EmailBody = cpCore.csv_EncodeContent(cpCore.csv_GetCS(CSEmail, "CopyFilename"), ConfirmationMemberID, , False, EmailAddLinkEID, True, True, False, True, , "http://" & GetPrimaryDomainName())
                    '                'EmailFrom = cpCore.csv_GetCS(CSEmail, "FromAddress")
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
                    '                Call cpCore.csv_SendEmail2(cpCore.asv.csv_GetCSText(CSPeople, "Email"), EmailFrom, "Email Confirmation from " & GetPrimaryDomainName(), Confirmation, "", "", , True, True)
                    '                End If
                    '            Call cpCore.asv.csv_CloseCS(CSPeople)
                    '
                    Call cpCore.db.db_csGoNext(CSEmail)
                Loop
            End If
            Call cpCore.db.cs_Close(CSEmail)
            '
            Exit Sub
ErrorTrap:
            cpCore.handleLegacyError3(cpCore.appConfig.name, "trap error", "App.EXEName", "ProcessEmailClass", "ProcessEmail_GroupEmail", Err.Number, Err.Source, Err.Description, True, True, "")
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
            dataSourceType = cpCore.db.db_GetDataSourceType("default")
            SQLTablePeople = cpCore.metaData.getContentTablename("People")
            SQLTableMemberRules = cpCore.metaData.getContentTablename("Member Rules")
            SQLTableGroups = cpCore.metaData.getContentTablename("Groups")
            BounceAddress = cpCore.siteProperties.getText("EmailBounceAddress", "")
            siteStyles = cpCore.csv_getStyleSheetProcessed()
            '
            rightNow = Now()
            rightNowDate = Int(rightNow)



            SQLDateNow = cpCore.db.db_EncodeSQLDate(Now)
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
                CSEmailBig = cpCore.db.db_openCsSql_rev("Default", SQL)
                Do While cpCore.db.cs_Ok(CSEmailBig)
                    emailID = cpCore.db.cs_getInteger(CSEmailBig, "EmailID")
                    EmailMemberID = cpCore.db.cs_getInteger(CSEmailBig, "MemberID")
                    EmailDateExpires = cpCore.db.db_GetCSDate(CSEmailBig, "DateExpires")
                    CSEmail = cpCore.db.db_OpenCSContentRecord("Conditional Email", emailID)
                    If cpCore.db.cs_Ok(CSEmail) Then
                        EmailTemplateID = cpCore.db.cs_getInteger(CSEmail, "EmailTemplateID")
                        EmailTemplate = GetEmailTemplate(EmailTemplateID)
                        FromAddress = cpCore.db.cs_getText(CSEmail, "FromAddress")
                        ConfirmationMemberID = cpCore.db.cs_getInteger(CSEmail, "testmemberid")
                        EmailAddLinkEID = cpCore.db.cs_getBoolean(CSEmail, "AddLinkEID")
                        EmailSubject = cpCore.db.db_GetCS(CSEmail, "Subject")
                        EmailCopy = cpCore.db.db_GetCS(CSEmail, "CopyFilename")
                        emailStyles = cpCore.email_getEmailStyles(emailID)
                        EmailStatus = SendEmailRecord(EmailMemberID, emailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, EmailSubject, EmailCopy, cpCore.db.cs_getBoolean(CSEmail, "AllowSpamFooter"), EmailAddLinkEID, emailStyles)
                        'EmailStatus = SendEmailRecord( EmailMemberID, EmailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, EmailSubject, EmailCopy, cpCore.csv_GetCSBoolean(CSEmail, "AllowSpamFooter"), EmailAddLinkEID, EmailInlineStyles)
                        Call SendConfirmationEmail(ConfirmationMemberID, EmailDropID, EmailTemplate, EmailAddLinkEID, "", EmailSubject, EmailCopy, emailStyles, FromAddress, EmailStatus & "<BR>")
                    End If
                    Call cpCore.db.cs_Close(CSEmail)
                    Call cpCore.db.db_csGoNext(CSEmailBig)
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
                CSEmailBig = cpCore.db.db_openCsSql_rev("Default", SQL)
                Do While cpCore.db.cs_Ok(CSEmailBig)
                    emailID = cpCore.db.cs_getInteger(CSEmailBig, "EmailID")
                    EmailMemberID = cpCore.db.cs_getInteger(CSEmailBig, "MemberID")
                    EmailDateExpires = cpCore.db.db_GetCSDate(CSEmailBig, "DateExpires")
                    CSEmail = cpCore.db.db_OpenCSContentRecord("Conditional Email", emailID)
                    If cpCore.db.cs_Ok(CSEmail) Then
                        EmailTemplateID = cpCore.db.cs_getInteger(CSEmail, "EmailTemplateID")
                        EmailTemplate = GetEmailTemplate(EmailTemplateID)
                        FromAddress = cpCore.db.cs_getText(CSEmail, "FromAddress")
                        ConfirmationMemberID = cpCore.db.cs_getInteger(CSEmail, "testmemberid")
                        EmailAddLinkEID = cpCore.db.cs_getBoolean(CSEmail, "AddLinkEID")
                        EmailSubject = cpCore.db.db_GetCS(CSEmail, "Subject")
                        EmailCopy = cpCore.db.db_GetCS(CSEmail, "CopyFilename")
                        emailStyles = cpCore.email_getEmailStyles(emailID)
                        EmailStatus = SendEmailRecord(EmailMemberID, emailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, cpCore.db.db_GetCS(CSEmail, "Subject"), cpCore.db.db_GetCS(CSEmail, "CopyFilename"), cpCore.db.cs_getBoolean(CSEmail, "AllowSpamFooter"), cpCore.db.cs_getBoolean(CSEmail, "AddLinkEID"), emailStyles)
                        'EmailStatus = SendEmailRecord( EmailMemberID, EmailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, cpCore.csv_GetCS(CSEmail, "Subject"), cpCore.csv_GetCS(CSEmail, "CopyFilename"), cpCore.csv_GetCSBoolean(CSEmail, "AllowSpamFooter"), cpCore.csv_GetCSBoolean(CSEmail, "AddLinkEID"), EmailInlineStyles)
                        Call SendConfirmationEmail(ConfirmationMemberID, EmailDropID, EmailTemplate, EmailAddLinkEID, "", EmailSubject, EmailCopy, emailStyles, FromAddress, EmailStatus & "<BR>")
                    End If
                    Call cpCore.db.cs_Close(CSEmail)
                    Call cpCore.db.db_csGoNext(CSEmailBig)
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
                        & " AND (ccEmail.ID Not In (Select ccEmailLog.EmailID from ccEmailLog where ccEmailLog.MemberID=" & SQLTablePeople & ".ID and ccEmailLog.DateAdded>=" & cpCore.db.db_EncodeSQLDate(Int(Now())) & "))"
                    CSEmailBig = cpCore.db.db_openCsSql_rev("Default", SQL)
                    Do While cpCore.db.cs_Ok(CSEmailBig)
                        emailID = cpCore.db.cs_getInteger(CSEmailBig, "EmailID")
                        EmailMemberID = cpCore.db.cs_getInteger(CSEmailBig, "MemberID")
                        EmailDateExpires = cpCore.db.db_GetCSDate(CSEmailBig, "DateExpires")
                        CSEmail = cpCore.db.db_OpenCSContentRecord("Conditional Email", emailID)
                        If cpCore.db.cs_Ok(CSEmail) Then
                            EmailTemplateID = cpCore.db.cs_getInteger(CSEmail, "EmailTemplateID")
                            EmailTemplate = GetEmailTemplate(EmailTemplateID)
                            FromAddress = cpCore.db.cs_getText(CSEmail, "FromAddress")
                            ConfirmationMemberID = cpCore.db.cs_getInteger(CSEmail, "testmemberid")
                            EmailAddLinkEID = cpCore.db.cs_getBoolean(CSEmail, "AddLinkEID")
                            EmailSubject = cpCore.db.db_GetCS(CSEmail, "Subject")
                            EmailCopy = cpCore.db.db_GetCS(CSEmail, "CopyFilename")
                            emailStyles = cpCore.email_getEmailStyles(emailID)
                            EmailStatus = SendEmailRecord(EmailMemberID, emailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, cpCore.db.db_GetCS(CSEmail, "Subject"), cpCore.db.db_GetCS(CSEmail, "CopyFilename"), cpCore.db.cs_getBoolean(CSEmail, "AllowSpamFooter"), cpCore.db.cs_getBoolean(CSEmail, "AddLinkEID"), emailStyles)
                            'EmailStatus = SendEmailRecord( EmailMemberID, EmailID, EmailDateExpires, 0, BounceAddress, FromAddress, EmailTemplate, FromAddress, cpCore.csv_GetCS(CSEmail, "Subject"), cpCore.csv_GetCS(CSEmail, "CopyFilename"), cpCore.csv_GetCSBoolean(CSEmail, "AllowSpamFooter"), cpCore.csv_GetCSBoolean(CSEmail, "AddLinkEID"), EmailInlineStyles)
                            Call SendConfirmationEmail(ConfirmationMemberID, EmailDropID, EmailTemplate, EmailAddLinkEID, "", EmailSubject, EmailCopy, emailStyles, FromAddress, EmailStatus & "<BR>")
                        End If
                        Call cpCore.db.cs_Close(CSEmail)
                        Call cpCore.db.db_csGoNext(CSEmailBig)
                    Loop
                    Call cpCore.db.cs_Close(CSEmailBig)
                End If
            End If
            '
            Exit Sub
ErrorTrap:
            cpCore.handleLegacyError3(cpCore.appConfig.name, "trap error", "App.EXEName", "ProcessEmailClass", "ProcessEmail_ConditionalEmail", Err.Number, Err.Source, Err.Description, True, True, "")
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
                CSLog = cpCore.db.db_csInsertRecord("Email Log", 0)
                If cpCore.db.cs_Ok(CSLog) Then
                    Call cpCore.db.db_SetCSField(CSLog, "Name", "Sent " & CStr(Now()))
                    Call cpCore.db.db_SetCSField(CSLog, "EmailDropID", EmailDropID)
                    Call cpCore.db.db_SetCSField(CSLog, "EmailID", emailID)
                    Call cpCore.db.db_SetCSField(CSLog, "MemberID", MemberID)
                    Call cpCore.db.db_SetCSField(CSLog, "LogType", EmailLogTypeDrop)
                    Call cpCore.db.db_SetCSField(CSLog, "DateBlockExpires", DateBlockExpires)
                    Call cpCore.db.db_SetCSField(CSLog, "SendStatus", "Send attempted but not completed")
                    If True Then
                        Call cpCore.db.db_setCS(CSLog, "fromaddress", FromAddress)
                        Call cpCore.db.db_setCS(CSLog, "Subject", EmailSubject)
                    End If
                    Call cpCore.db.db_SaveCS(CSLog)
                    '
                    ' Get the Template
                    '
                    PrimaryLink = "http://" & GetPrimaryDomainName()
                    '
                    ' Get the Member
                    '
                    CSPeople = cpCore.db.db_OpenCSContentRecord("People", MemberID, , , , "Email,Name")
                    If cpCore.db.cs_Ok(CSPeople) Then
                        ToAddress = cpCore.db.db_GetCS(CSPeople, "Email")
                        EmailToName = cpCore.db.db_GetCS(CSPeople, "Name")
                        ServerPageDefault = cpCore.siteProperties.getText(siteproperty_serverPageDefault_name, siteproperty_serverPageDefault_defaultValue)
                        RootURL = PrimaryLink & cpCore.app_rootWebPath
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
                        emailWorkingStyles = Replace(emailWorkingStyles, StyleSheetStart, StyleSheetStart & "<!-- ", , , vbTextCompare)
                        emailWorkingStyles = Replace(emailWorkingStyles, StyleSheetEnd, " // -->" & StyleSheetEnd, , , vbTextCompare)
                        '
                        ' Create the clickflag to be added to all anchors
                        '
                        ClickFlagQuery = RequestNameEmailClickFlag & "=" & EmailDropID & "&" & RequestNameEmailMemberID & "=" & MemberID
                        '
                        ' Encode body and subject
                        '
                        EmailBodyEncoded = cpCore.html_executeContentCommands(Nothing, EmailBodyEncoded, AddonContextEnum.contextEmail, MemberID, True, errorMessage)
                        EmailBodyEncoded = cpCore.html_encodeContent10(EmailBodyEncoded, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, 0, "", AddonContextEnum.contextEmail, True, Nothing, False)
                        'EmailBodyEncoded = cpCore.csv_EncodeContent8(Nothing, EmailBodyEncoded, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, "", 0, "", True, AddonContextEnum.contextEmail)
                        '
                        EmailSubjectEncoded = cpCore.html_executeContentCommands(Nothing, EmailSubjectEncoded, AddonContextEnum.contextEmail, MemberID, True, errorMessage)
                        EmailSubjectEncoded = cpCore.html_encodeContent10(EmailSubjectEncoded, MemberID, "", 0, 0, True, False, False, False, False, True, "", PrimaryLink, True, 0, "", AddonContextEnum.contextEmail, True, Nothing, False)
                        'EmailSubjectEncoded = cpCore.csv_EncodeContent8(Nothing, EmailSubjectEncoded, MemberID, "", 0, 0, True, False, False, False, False, True, "", PrimaryLink, True, "", 0, "", True, AddonContextEnum.contextEmail)
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
                            EmailTemplateEncoded = cpCore.html_executeContentCommands(Nothing, EmailTemplateEncoded, AddonContextEnum.contextEmail, MemberID, True, errorMessage)
                            EmailTemplateEncoded = cpCore.html_encodeContent10(EmailTemplate, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, 0, "", AddonContextEnum.contextEmail, True, Nothing, False)
                            'EmailTemplateEncoded = cpCore.csv_EncodeContent8(Nothing, EmailTemplate, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, "", 0, "", True, AddonContextEnum.contextEmail)
                            'EmailTemplateEncoded = cpCore.csv_encodecontent8(Nothing, EmailTemplate, MemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, ClickFlagQuery, PrimaryLink, True, "", 0, ContentPlaceHolder, True, addonContextEnum.contextemail)
                            If InStr(1, EmailTemplateEncoded, fpoContentBox) <> 0 Then
                                EmailBodyEncoded = Replace(EmailTemplateEncoded, fpoContentBox, EmailBodyEncoded)
                            Else
                                EmailBodyEncoded = EmailTemplateEncoded & "<div style=""padding:10px;"">" & EmailBodyEncoded & "</div>"
                            End If
                            '                If InStr(1, EmailTemplateEncoded, ContentPlaceHolder) <> 0 Then
                            '                    EmailBodyEncoded = Replace(EmailTemplateEncoded, ContentPlaceHolder, EmailBodyEncoded)
                            '                Else
                            '                    EmailBodyEncoded = EmailTemplateEncoded & "<div style=""padding:10px;"">" & EmailBodyEncoded & "</div>"
                            '                End If
                        End If
                        '
                        ' Spam Footer under template
                        ' remove the marker for any other place in the email then add it as needed
                        '
                        EmailBodyEncoded = Replace(EmailBodyEncoded, RequestNameEmailSpamFlag, "", , , vbTextCompare)
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
                        EmailBodyEncoded = Replace(EmailBodyEncoded, "#member_id#", MemberID)
                        EmailBodyEncoded = Replace(EmailBodyEncoded, "#member_email#", ToAddress)
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
                        EmailStatus = cpCore.email_send3(ToAddress, FromAddress, EmailSubjectEncoded, EmailBodyEncoded, BounceAddress, ReplyToAddress, "", True, True, 0)
                        If EmailStatus = "" Then
                            EmailStatus = "ok"
                        End If
                        returnStatus = returnStatus & "Send to " & EmailToName & " at " & ToAddress & ", Status = " & EmailStatus
                        '
                        ' ----- Log the send
                        '
                        Call cpCore.db.db_setCS(CSLog, "SendStatus", EmailStatus)
                        If True Then
                            Call cpCore.db.db_setCS(CSLog, "toaddress", ToAddress)
                        End If
                        Call cpCore.db.db_SaveCS(CSLog)
                    End If
                    'Call cpCore.app.db_closeCS(CSPeople)
                End If
                'Call cpCore.app.db_closeCS(CSLog)
            Catch ex As Exception
                cpCore.handleLegacyError3(cpCore.appConfig.name, "trap error", "App.EXEName", "ProcessEmailClass", "SendEmailRecord", Err.Number, Err.Source, Err.Description, True, True, "")
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
            On Error GoTo ErrorTrap
            '
            Dim CopySplit
            '
            GetPrimaryDomainName = cpCore.app_domainList
            If InStr(1, GetPrimaryDomainName, ",", 1) <> 0 Then
                CopySplit = Split(GetPrimaryDomainName, ",")
                GetPrimaryDomainName = CopySplit(0)
            End If
            '
            Exit Function
            '
ErrorTrap:
            cpCore.handleLegacyError3(cpCore.appConfig.name, "trap error", "App.EXEName", "ProcessEmailClass", "GetPrimaryDomainName", Err.Number, Err.Source, Err.Description, True, True, "")
            Err.Clear()
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
                CS = cpCore.db.db_OpenCSContentRecord("Email Templates", EmailTemplateID, , , , "BodyHTML")
                If cpCore.db.cs_Ok(CS) Then
                    GetEmailTemplate = cpCore.db.db_GetCS(CS, "BodyHTML")
                End If
                Call cpCore.db.cs_Close(CS)
            End If
            '
            Exit Function
            '
ErrorTrap:
            cpCore.handleLegacyError3(cpCore.appConfig.name, "trap error", "App.EXEName", "ProcessEmailClass", "GetEmailTemplate", Err.Number, Err.Source, Err.Description, True, True, "")
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
                CSPeople = cpCore.db.db_OpenCSContentRecord("people", ConfirmationMemberID)
                If cpCore.db.cs_Ok(CSPeople) Then
                    ClickFlagQuery = RequestNameEmailClickFlag & "=" & EmailDropID & "&" & RequestNameEmailMemberID & "=" & ConfirmationMemberID
                    '
                    EmailSubject = cpCore.html_executeContentCommands(Nothing, EmailSubject, AddonContextEnum.contextEmail, ConfirmationMemberID, True, errorMessage)
                    EmailSubject = cpCore.html_encodeContent10(EmailSubject, ConfirmationMemberID, "", 0, 0, True, False, False, False, False, True, "", "http://" & GetPrimaryDomainName(), True, 0, "", AddonContextEnum.contextEmail, True, Nothing, False)
                    'EmailSubject = cpCore.csv_EncodeContent8(Nothing, EmailSubject, ConfirmationMemberID, "", 0, 0, True, False, False, False, False, True, "", "http://" & GetPrimaryDomainName(), True, "", 0, "", True, AddonContextEnum.contextEmail)
                    '
                    EmailBody = cpCore.html_executeContentCommands(Nothing, EmailBody, AddonContextEnum.contextEmail, ConfirmationMemberID, True, errorMessage)
                    EmailBody = cpCore.html_encodeContent10(EmailCopy, ConfirmationMemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, "", "http://" & GetPrimaryDomainName(), True, 0, "", AddonContextEnum.contextEmail, True, Nothing, False)
                    'EmailBody = cpCore.csv_EncodeContent8(Nothing, EmailCopy, ConfirmationMemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, "", "http://" & GetPrimaryDomainName(), True, "", 0, "", True, AddonContextEnum.contextEmail)
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
                        WorkingTemplate = cpCore.html_executeContentCommands(Nothing, WorkingTemplate, AddonContextEnum.contextEmail, ConfirmationMemberID, True, errorMessage)
                        WorkingTemplate = cpCore.html_encodeContent10(WorkingTemplate, ConfirmationMemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, False, "http://" & GetPrimaryDomainName(), True, 0, "", AddonContextEnum.contextEmail, True, Nothing, False)
                        'WorkingTemplate = cpCore.csv_encodecontent8(Nothing, EmailTemplate, ConfirmationMemberID, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, False, "http://" & GetPrimaryDomainName(), True, "", 0, ContentPlaceHolder, True, addonContextEnum.contextemail)
                        If InStr(1, WorkingTemplate, fpoContentBox) <> 0 Then
                            EmailBody = Replace(WorkingTemplate, fpoContentBox, EmailBody)
                        Else
                            EmailBody = WorkingTemplate & "<div style=""padding:10px"">" & EmailBody & "</div>"
                        End If
                        '            If InStr(1, WorkingTemplate, ContentPlaceHolder) <> 0 Then
                        '                EmailBody = Replace(WorkingTemplate, ContentPlaceHolder, EmailBody)
                        '            Else
                        '                EmailBody = WorkingTemplate & "<div style=""padding:10px"">" & EmailBody & "</div>"
                        '            End If
                    End If
                    '
                    ConfirmBody = "<HTML><Head>" _
                    & "<Title>Email Confirmation</Title>" _
                    & "<Base href=""http://" & GetPrimaryDomainName() & cpCore.app_rootWebPath & """>" _
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
                    Call cpCore.email_send3(cpCore.db.cs_getText(CSPeople, "Email"), EmailFrom, "Email confirmation from " & GetPrimaryDomainName(), ConfirmBody, "", "", "", True, True, 0)
                End If
                Call cpCore.db.cs_Close(CSPeople)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
    End Class
End Namespace