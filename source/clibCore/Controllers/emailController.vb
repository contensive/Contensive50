
Option Explicit On
Option Strict On
'
Imports System.Text.RegularExpressions
'
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports Contensive.Core.Models
Imports Contensive.Core.Models.Context
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Core.Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' static class controller
    ''' </summary>
    Public Class emailController
        Implements IDisposable
        '
        ' ----- constants
        '
        'Private Const invalidationDaysDefault As Double = 365
        '
        ' ----- objects constructed that must be disposed
        '
        'Private cacheClient As Enyim.Caching.MemcachedClient
        '
        ' ----- private instance storage
        '
        Dim cpcore As coreClass
        '
        ' Email Block List - these are people who have asked to not have email sent to them from this site
        '   Loaded ondemand by csv_GetEmailBlockList
        '
        Private email_BlockList_Local As String = ""
        Private email_BlockList_LocalLoaded As Boolean
        '
        Private Function getBlockList() As String
            '
            Dim Filename As String
            '
            If Not email_BlockList_LocalLoaded Then
                Filename = "Config\SMTPBlockList.txt"
                email_BlockList_Local = cpcore.privateFiles.readFile(Filename)
                email_BlockList_LocalLoaded = True
            End If
            getBlockList = email_BlockList_Local
            '
        End Function

        '
        '
        '
        Public Sub addToBlockList(ByVal EmailAddress As String)
            If EmailAddress = "" Then
                '
                ' bad email address
                '
            ElseIf (InStr(1, EmailAddress, "@") = 0) Or (InStr(1, EmailAddress, ".") = 0) Then
                '
                ' bad email address
                '
            ElseIf genericController.vbInstr(1, getBlockList(), vbCrLf & EmailAddress & vbTab, vbTextCompare) <> 0 Then
                '
                ' They are already in the list
                '
            Else
                '
                ' add them to the list
                '
                email_BlockList_Local = getBlockList() & vbCrLf & EmailAddress & vbTab & Now()
                Call cpcore.privateFiles.saveFile("Config\SMTPBlockList.txt", email_BlockList_Local)
                email_BlockList_LocalLoaded = False
            End If
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Send Email
        ''' </summary>
        ''' <param name="ToAddress"></param>
        ''' <param name="FromAddress"></param>
        ''' <param name="SubjectMessage"></param>
        ''' <param name="BodyMessage"></param>
        ''' <param name="BounceAddress"></param>
        ''' <param name="ReplyToAddress"></param>
        ''' <param name="ResultLogFilename"></param>
        ''' <param name="isImmediate"></param>
        ''' <param name="isHTML"></param>
        ''' <param name="emailIdOrZeroForLog"></param>
        ''' <returns>OK if send is successful, otherwise returns the principle issue as a user error.</returns>
        Public Function send(ByVal ToAddress As String, ByVal FromAddress As String, ByVal SubjectMessage As String, ByVal BodyMessage As String, ByVal BounceAddress As String, ByVal ReplyToAddress As String, ByVal ResultLogFilename As String, ByVal isImmediate As Boolean, ByVal isHTML As Boolean, ByVal emailIdOrZeroForLog As Integer) As String
            Dim returnStatus As String = ""
            Try
                '
                Dim htmlBody As String
                Dim rootUrl As String
                Dim EmailHandler As New smtpController(cpcore)
                Dim iResultLogPathPage As String
                Dim WarningMsg As String = ""
                Dim CSLog As Integer
                '
                If ToAddress = "" Then
                    ' block
                ElseIf (InStr(1, ToAddress, "@") = 0) Or (InStr(1, ToAddress, ".") = 0) Then
                    ' block
                ElseIf FromAddress = "" Then
                    ' block
                ElseIf (InStr(1, FromAddress, "@") = 0) Or (InStr(1, FromAddress, ".") = 0) Then
                    ' block
                ElseIf 0 <> genericController.vbInstr(1, getBlockList, vbCrLf & ToAddress & vbCrLf, vbTextCompare) Then
                    '
                    ' They are in the block list
                    '
                    returnStatus = "Recipient has blocked this email"
                Else
                    '
                    iResultLogPathPage = ResultLogFilename
                    '
                    ' Test for from-address / to-address matches
                    '
                    If genericController.vbLCase(FromAddress) = genericController.vbLCase(ToAddress) Then
                        FromAddress = cpcore.siteProperties.getText("EmailFromAddress", "")
                        If FromAddress = "" Then
                            '
                            '
                            '
                            FromAddress = ToAddress
                            WarningMsg = "The from-address matches the to-address. This email was sent, but may be blocked by spam filtering."
                        ElseIf genericController.vbLCase(FromAddress) = genericController.vbLCase(ToAddress) Then
                            '
                            '
                            '
                            WarningMsg = "The from-address matches the to-address. This email was sent, but may be blocked by spam filtering."
                        Else
                            '
                            '
                            '
                            WarningMsg = "The from-address matches the to-address. The from-address was changed to " & FromAddress & " to prevent it from being blocked by spam filtering."
                        End If
                    End If
                    '
                    If isHTML Then
                        '
                        ' Fix links for HTML send
                        '
                        rootUrl = "http://" & cpcore.serverConfig.appConfig.domainList(0) & "/"
                        BodyMessage = genericController.ConvertLinksToAbsolute(BodyMessage, rootUrl)
                        '
                        ' compose body
                        '
                        htmlBody = "" _
                            & "<html>" _
                            & "<head>" _
                            & "<Title>" & SubjectMessage & "</Title>" _
                            & "<Base href=""" & rootUrl & """ >" _
                            & "</head>" _
                            & "<body class=""ccBodyEmail"">" _
                            & "<Base href=""" & rootUrl & """ >" _
                            & BodyMessage _
                            & "</body>" _
                            & "</html>"
                        returnStatus = EmailHandler.sendEmail5(ToAddress, FromAddress, SubjectMessage, BodyMessage, BounceAddress, ReplyToAddress, iResultLogPathPage, cpcore.siteProperties.getText("SMTPServer", "SMTP.YourServer.Com"), isImmediate, isHTML, "")
                    Else
                        returnStatus = EmailHandler.sendEmail5(ToAddress, FromAddress, SubjectMessage, BodyMessage, BounceAddress, ReplyToAddress, iResultLogPathPage, cpcore.siteProperties.getText("SMTPServer", "SMTP.YourServer.Com"), isImmediate, isHTML, "")
                    End If
                    If (returnStatus = "") Then
                        returnStatus = WarningMsg
                    End If
                    '
                    ' ----- Log the send
                    '
                    If True Then
                        CSLog = cpcore.db.cs_insertRecord("Email Log", 0)
                        If cpcore.db.cs_ok(CSLog) Then
                            Call cpcore.db.cs_set(CSLog, "Name", "System Email Send " & CStr(Now()))
                            Call cpcore.db.cs_set(CSLog, "LogType", EmailLogTypeImmediateSend)
                            Call cpcore.db.cs_set(CSLog, "SendStatus", returnStatus)
                            Call cpcore.db.cs_set(CSLog, "toaddress", ToAddress)
                            Call cpcore.db.cs_set(CSLog, "fromaddress", FromAddress)
                            Call cpcore.db.cs_set(CSLog, "Subject", SubjectMessage)
                            If emailIdOrZeroForLog <> 0 Then
                                Call cpcore.db.cs_set(CSLog, "emailid", emailIdOrZeroForLog)
                            End If
                        End If
                        Call cpcore.db.cs_Close(CSLog)
                    End If
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
            End Try
            Return returnStatus
        End Function
        '
        '
        '
        Public Function getStyles(ByVal EmailID As Integer) As String
            On Error GoTo ErrorTrap 'Const Tn = "getEmailStyles": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            getStyles = cpcore.htmlDoc.html_getStyleSheet2(csv_contentTypeEnum.contentTypeEmail, 0, genericController.EncodeInteger(EmailID))
            If getStyles <> "" Then
                getStyles = "" _
                    & vbCrLf & StyleSheetStart _
                    & vbCrLf & getStyles _
                    & vbCrLf & StyleSheetEnd
            End If
            '
            '
            Exit Function
ErrorTrap:
            cpcore.handleExceptionAndRethrow(New Exception("Unexpected exception"))
        End Function
        '  
        '========================================================================
        ''' <summary>
        ''' Send email to a memberId, returns ok if send is successful, otherwise returns the principle issue as a user error.
        ''' </summary>
        ''' <param name="personId"></param>
        ''' <param name="FromAddress"></param>
        ''' <param name="subject"></param>
        ''' <param name="Body"></param>
        ''' <param name="Immediate"></param>
        ''' <param name="HTML"></param>
        ''' <param name="emailIdOrZeroForLog"></param>
        ''' <param name="template"></param>
        ''' <param name="EmailAllowLinkEID"></param>
        ''' <returns> returns ok if send is successful, otherwise returns the principle issue as a user error</returns>
        Public Function sendPerson(ByVal personId As Integer, ByVal FromAddress As String, ByVal subject As String, ByVal Body As String, ByVal Immediate As Boolean, ByVal HTML As Boolean, ByVal emailIdOrZeroForLog As Integer, ByVal template As String, ByVal EmailAllowLinkEID As Boolean) As String
            Dim returnStatus As String = ""
            Try
                Dim CS As Integer
                Dim ToAddress As String
                'Dim MethodName As String
                Dim rootUrl As String
                Dim layoutError As String = ""
                Dim subjectEncoded As String
                Dim bodyEncoded As String
                Dim templateEncoded As String
                '
                subjectEncoded = subject
                bodyEncoded = Body
                templateEncoded = template
                '
                CS = cpcore.db.cs_openContentRecord("People", personId, , , , "email")
                If cpcore.db.cs_ok(CS) Then
                    ToAddress = Trim(cpcore.db.cs_getText(CS, "email"))
                    If ToAddress = "" Then
                        returnStatus = "The email was not sent because the to-address was blank."
                    ElseIf (InStr(1, ToAddress, "@") = 0) Or (InStr(1, ToAddress, ".") = 0) Then
                        returnStatus = "The email was not sent because the to-address [" & ToAddress & "] was not valid."
                    ElseIf FromAddress = "" Then
                        returnStatus = "The email was not sent because the from-address was blank."
                    ElseIf (InStr(1, FromAddress, "@") = 0) Or (InStr(1, FromAddress, ".") = 0) Then
                        returnStatus = "The email was not sent because the from-address [" & FromAddress & "] was not valid."
                    Else
                        '
                        ' encode subject
                        '
                        subjectEncoded = cpcore.htmlDoc.html_executeContentCommands(Nothing, subjectEncoded, CPUtilsBaseClass.addonContext.ContextEmail, personId, True, layoutError)
                        subjectEncoded = cpcore.htmlDoc.html_encodeContent10(subjectEncoded, personId, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, "", "http://" & cpcore.serverConfig.appConfig.domainList(0), True, 0, "", CPUtilsBaseClass.addonContext.ContextEmail, True, Nothing, False)
                        '
                        ' encode Body
                        '
                        bodyEncoded = cpcore.htmlDoc.html_executeContentCommands(Nothing, bodyEncoded, CPUtilsBaseClass.addonContext.ContextEmail, personId, True, layoutError)
                        bodyEncoded = cpcore.htmlDoc.html_encodeContent10(bodyEncoded, personId, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, "", "http://" & cpcore.serverConfig.appConfig.domainList(0), True, 0, "", CPUtilsBaseClass.addonContext.ContextEmail, True, Nothing, False)
                        '
                        ' encode template
                        '
                        If (templateEncoded <> "") Then
                            templateEncoded = cpcore.htmlDoc.html_executeContentCommands(Nothing, templateEncoded, CPUtilsBaseClass.addonContext.ContextEmail, personId, True, layoutError)
                            templateEncoded = cpcore.htmlDoc.html_encodeContent10(templateEncoded, personId, "", 0, 0, False, EmailAllowLinkEID, True, True, False, True, "", "http://" & cpcore.serverConfig.appConfig.domainList(0), True, 0, "", CPUtilsBaseClass.addonContext.ContextEmail, True, Nothing, False)
                            '
                            If (InStr(1, templateEncoded, fpoContentBox) <> 0) Then
                                bodyEncoded = genericController.vbReplace(templateEncoded, fpoContentBox, bodyEncoded)
                            Else
                                bodyEncoded = templateEncoded & bodyEncoded
                            End If
                        End If
                        bodyEncoded = genericController.vbReplace(bodyEncoded, "#member_id#", personId.ToString)
                        bodyEncoded = genericController.vbReplace(bodyEncoded, "#member_email#", ToAddress)
                        '
                        returnStatus = send(ToAddress, FromAddress, subjectEncoded, bodyEncoded, "", "", "", Immediate, HTML, emailIdOrZeroForLog)
                    End If
                End If
                Call cpcore.db.cs_Close(CS)
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
            End Try
            Return returnStatus
        End Function
        '
        '========================================================================
        ' Set the email sql for all members marked to receive the email
        '   Used to send the email and as body on the email test
        '========================================================================
        '
        Public Function getGroupSql(ByVal EmailID As Integer) As String
            Return "SELECT " _
                    & " u.ID AS ID" _
                    & " ,u.Name AS Name" _
                    & " ,u.Email AS Email " _
                    & " " _
                    & " from " _
                    & " (((ccMembers u" _
                    & " left join ccMemberRules mr on mr.memberid=u.id)" _
                    & " left join ccGroups g on g.id=mr.groupid)" _
                    & " left join ccEmailGroups r on r.groupid=g.id)" _
                    & " " _
                    & " where " _
                    & " (r.EmailID=1) " _
                    & " and(r.Active<>0) " _
                    & " and(g.Active<>0) " _
                    & " and(g.AllowBulkEmail<>0) " _
                    & " and(mr.Active<>0) " _
                    & " and(u.Active<>0) " _
                    & " and(u.AllowBulkEmail<>0)" _
                    & " AND((mr.DateExpires is null)OR(mr.DateExpires>'20161205 22:40:58:184')) " _
                    & " " _
                    & " group by " _
                    & " u.ID, u.Name, u.Email " _
                    & " " _
                    & " having ((u.Email Is Not Null) and(u.Email<>'')) " _
                    & " " _
                    & " order by u.Email,u.ID" _
                    & " "
        End Function
        '
        ' ----- Need to test this and make it public
        '
        '   This is what the admin site should call for both test and group email
        '   Making it public lets developers send email that administrators can control
        '
        Public Function sendSystem(ByVal EMailName As String, ByVal AdditionalCopy As String, ByVal AdditionalMemberIDOrZero As Integer) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SendSystemEmail")
            '
            Dim returnString As String
            Dim isAdmin As Boolean
            Dim iAdditionalMemberID As Integer
            Dim layoutError As String
            Dim emailstyles As String
            Dim EmailRecordID As Integer
            Dim CSPeople As Integer
            Dim CSEmail As Integer
            Dim CSLog As Integer
            Dim EmailToAddress As String
            Dim EmailToName As String
            Dim SQL As String
            Dim EmailFrom As String
            Dim EmailSubjectSource As String
            Dim EmailBodySource As String
            Dim ConfirmBody As String
            Dim EmailAllowLinkEID As Boolean
            Dim EmailToConfirmationMemberID As Integer
            Dim EmailStatusMessage As String
            Dim EMailToMemberID As Integer
            Dim EmailSubject As String
            Dim ClickFlagQuery As String
            Dim EmailBody As String
            Dim EmailStatus As String
            Dim BounceAddress As String
            Dim SelectList As String
            Dim EMailTemplateID As Integer
            Dim EmailTemplate As String
            Dim EmailTemplateSource As String
            Dim CS As Integer
            Dim isValid As Boolean
            '
            returnString = ""
            iAdditionalMemberID = AdditionalMemberIDOrZero
            '
            If True Then
                SelectList = "ID,TestMemberID,FromAddress,Subject,copyfilename,AddLinkEID,AllowSpamFooter,EmailTemplateID"
            Else
                SelectList = "ID,TestMemberID,FromAddress,Subject,copyfilename,AddLinkEID,AllowSpamFooter,0 as EmailTemplateID"
            End If
            CSEmail = cpcore.db.cs_open("System Email", "name=" & cpcore.db.encodeSQLText(EMailName), "ID", , , , , SelectList)
            If Not cpcore.db.cs_ok(CSEmail) Then
                '
                ' ----- Email was not found
                '
                Call cpcore.db.cs_Close(CSEmail)
                CSEmail = cpcore.db.cs_insertRecord("System Email")
                Call cpcore.db.cs_set(CSEmail, "name", EMailName)
                Call cpcore.db.cs_set(CSEmail, "Subject", EMailName)
                Call cpcore.db.cs_set(CSEmail, "FromAddress", cpcore.siteProperties.getText("EmailAdmin", "webmaster@" & cpcore.serverConfig.appConfig.domainList(0)))
                'Call app.csv_SetCS(CSEmail, "caption", EmailName)
                Call cpcore.db.cs_Close(CSEmail)
                Call Err.Raise(ignoreInteger, "dll", "No system email was found with the name [" & EMailName & "]. A new email blank was created but not sent.")
            Else
                '
                ' --- collect values needed for send
                '
                EmailRecordID = cpcore.db.cs_getInteger(CSEmail, "ID")
                EmailToConfirmationMemberID = cpcore.db.cs_getInteger(CSEmail, "testmemberid")
                EmailFrom = cpcore.db.cs_getText(CSEmail, "FromAddress")
                EmailSubjectSource = cpcore.db.cs_getText(CSEmail, "Subject")
                EmailBodySource = cpcore.db.cs_get(CSEmail, "copyfilename") & AdditionalCopy
                EmailAllowLinkEID = cpcore.db.cs_getBoolean(CSEmail, "AddLinkEID")
                BounceAddress = cpcore.siteProperties.getText("EmailBounceAddress", "")
                If BounceAddress = "" Then
                    BounceAddress = EmailFrom
                End If
                EMailTemplateID = cpcore.db.cs_getInteger(CSEmail, "EmailTemplateID")
                '
                ' Get the Email Template
                '
                If EMailTemplateID <> 0 Then
                    CS = cpcore.db.cs_openContentRecord("Email Templates", EMailTemplateID)
                    If cpcore.db.cs_ok(CS) Then
                        EmailTemplateSource = cpcore.db.cs_get(CS, "BodyHTML")
                    End If
                    Call cpcore.db.cs_Close(CS)
                End If
                If EmailTemplateSource = "" Then
                    EmailTemplateSource = "<div style=""padding:10px""><ac type=content></div>"
                End If
                '
                ' add styles to the template
                '
                emailstyles = getStyles(EmailRecordID)
                EmailTemplateSource = emailstyles & EmailTemplateSource
                '
                ' Spam Footer
                '
                If cpcore.db.cs_getBoolean(CSEmail, "AllowSpamFooter") Then
                    '
                    ' This field is default true, and non-authorable
                    ' It will be true in all cases, except a possible unforseen exception
                    '
                    EmailTemplateSource = EmailTemplateSource & "<div style=""clear: both;padding:10px;"">" & cpcore.csv_GetLinkedText("<a href=""" & cpcore.htmlDoc.html_EncodeHTML("http://" & cpcore.serverConfig.appConfig.domainList(0) & "/" & cpcore.siteProperties.serverPageDefault & "?" & RequestNameEmailSpamFlag & "=#member_email#") & """>", cpcore.siteProperties.getText("EmailSpamFooter", DefaultSpamFooter)) & "</div>"
                End If
                '
                ' --- Send message to the additional member
                '
                If iAdditionalMemberID <> 0 Then
                    EmailStatusMessage = EmailStatusMessage & BR & "Primary Recipient:" & BR
                    CSPeople = cpcore.db.cs_openContentRecord("People", iAdditionalMemberID, , , , "ID,Name,Email")
                    If cpcore.db.cs_ok(CSPeople) Then
                        EMailToMemberID = cpcore.db.cs_getInteger(CSPeople, "ID")
                        EmailToName = cpcore.db.cs_getText(CSPeople, "name")
                        EmailToAddress = cpcore.db.cs_getText(CSPeople, "email")
                        If EmailToAddress = "" Then
                            EmailStatusMessage = EmailStatusMessage & "&nbsp;&nbsp;Error: Not Sent to " & EmailToName & " (people #" & EMailToMemberID & ") because their email address was blank." & BR
                        Else
                            EmailStatus = sendPerson(iAdditionalMemberID, EmailFrom, EmailSubjectSource, EmailBodySource, False, True, EmailRecordID, EmailTemplateSource, EmailAllowLinkEID)
                            If EmailStatus = "" Then
                                EmailStatus = "ok"
                            End If
                            EmailStatusMessage = EmailStatusMessage & "&nbsp;&nbsp;Sent to " & EmailToName & " at " & EmailToAddress & ", Status = " & EmailStatus & BR
                        End If
                    End If
                    Call cpcore.db.cs_Close(CSPeople)
                End If
                '
                ' --- Send message to everyone selected
                '
                EmailStatusMessage = EmailStatusMessage & BR & "Recipients in selected System Email groups:" & BR
                SQL = getGroupSql(EmailRecordID)
                CSPeople = cpcore.db.cs_openCsSql_rev("default", SQL)
                Do While cpcore.db.cs_ok(CSPeople)
                    EMailToMemberID = cpcore.db.cs_getInteger(CSPeople, "ID")
                    EmailToName = cpcore.db.cs_getText(CSPeople, "name")
                    EmailToAddress = cpcore.db.cs_getText(CSPeople, "email")
                    If EmailToAddress = "" Then
                        EmailStatusMessage = EmailStatusMessage & "&nbsp;&nbsp;Not Sent to " & EmailToName & ", people #" & EMailToMemberID & " because their email address was blank." & BR
                    Else
                        EmailStatus = sendPerson(EMailToMemberID, EmailFrom, EmailSubjectSource, EmailBodySource, False, True, EmailRecordID, EmailTemplateSource, EmailAllowLinkEID)
                        If EmailStatus = "" Then
                            EmailStatus = "ok"
                        End If
                        EmailStatusMessage = EmailStatusMessage & "&nbsp;&nbsp;Sent to " & EmailToName & " at " & EmailToAddress & ", Status = " & EmailStatus & BR
                        Call cpcore.db.cs_goNext(CSPeople)
                    End If
                Loop
                Call cpcore.db.cs_Close(CSPeople)
                '
                ' --- Send the completion message to the administrator
                '
                If EmailToConfirmationMemberID = 0 Then
                    ' AddUserError ("No confirmation email was sent because no confirmation member was selected")
                Else
                    '
                    ' get the confirmation info
                    '
                    isValid = False
                    CSPeople = cpcore.db.cs_openContentRecord("people", EmailToConfirmationMemberID)
                    If cpcore.db.cs_ok(CSPeople) Then
                        isValid = cpcore.db.cs_getBoolean(CSPeople, "active")
                        EMailToMemberID = cpcore.db.cs_getInteger(CSPeople, "ID")
                        EmailToName = cpcore.db.cs_getText(CSPeople, "name")
                        EmailToAddress = cpcore.db.cs_getText(CSPeople, "email")
                        isAdmin = cpcore.db.cs_getBoolean(CSPeople, "admin")
                    End If
                    Call cpcore.db.cs_Close(CSPeople)
                    '
                    If Not isValid Then
                        'returnString = "Administrator: The confirmation email was not sent because the confirmation email person is not selected or inactive, " & EmailStatus
                    Else
                        '
                        ' Encode the body
                        '
                        EmailBody = EmailBodySource & ""
                        '
                        ' Encode the template
                        '
                        EmailTemplate = EmailTemplateSource
                        '
                        EmailSubject = EmailSubjectSource
                        '
                        ConfirmBody = ConfirmBody & "<div style=""padding:10px;"">" & BR
                        ConfirmBody = ConfirmBody & "The follow System Email was sent." & BR
                        ConfirmBody = ConfirmBody & "" & BR
                        ConfirmBody = ConfirmBody & "If this email includes personalization, each email sent was personalized to it's recipient. This confirmation has been personalized to you." & BR
                        ConfirmBody = ConfirmBody & "" & BR
                        ConfirmBody = ConfirmBody & "Subject: " & EmailSubject & BR
                        ConfirmBody = ConfirmBody & "From: " & EmailFrom & BR
                        ConfirmBody = ConfirmBody & "Bounces return to: " & BounceAddress & BR
                        ConfirmBody = ConfirmBody & "Body:" & BR
                        ConfirmBody = ConfirmBody & "<div style=""clear:all"">----------------------------------------------------------------------</div>" & BR
                        ConfirmBody = ConfirmBody & EmailBody & BR
                        ConfirmBody = ConfirmBody & "<div style=""clear:all"">----------------------------------------------------------------------</div>" & BR
                        ConfirmBody = ConfirmBody & "--- recipient list ---" & BR
                        ConfirmBody = ConfirmBody & EmailStatusMessage & BR
                        ConfirmBody = ConfirmBody & "--- end of list ---" & BR
                        ConfirmBody = ConfirmBody & "</div>"
                        '
                        EmailStatus = sendPerson(EmailToConfirmationMemberID, EmailFrom, "System Email confirmation from " & cpcore.serverConfig.appConfig.domainList(0), ConfirmBody, False, True, EmailRecordID, "", False)
                        If isAdmin And (EmailStatus <> "") Then
                            returnString = "Administrator: There was a problem sending the confirmation email, " & EmailStatus
                        End If
                    End If
                End If
                '
                ' ----- Done
                '
                Call cpcore.db.cs_Close(CSPeople)
            End If
            Call cpcore.db.cs_Close(CSEmail)
            '
            sendSystem = returnString
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpcore.handleLegacyError7("csv_SendSystemEmail", "Unexpected Trap")
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Send Email to address
        ''' </summary>
        ''' <param name="ToAddress"></param>
        ''' <param name="FromAddress"></param>
        ''' <param name="SubjectMessage"></param>
        ''' <param name="BodyMessage"></param>
        ''' <param name="optionalEmailIdForLog"></param>
        ''' <param name="Immediate"></param>
        ''' <param name="HTML"></param>
        ''' <returns>Returns OK if successful, otherwise returns user status</returns>
        Public Function send_Legacy(ByVal ToAddress As String, ByVal FromAddress As String, ByVal SubjectMessage As String, ByVal BodyMessage As String, Optional ByVal optionalEmailIdForLog As Integer = 0, Optional ByVal Immediate As Boolean = True, Optional ByVal HTML As Boolean = False) As String
            Dim returnStatus As String = ""
            Try
                returnStatus = send(genericController.encodeText(ToAddress), genericController.encodeText(FromAddress), genericController.encodeText(SubjectMessage), genericController.encodeText(BodyMessage), "", "", "", Immediate, genericController.EncodeBoolean(HTML), genericController.EncodeInteger(optionalEmailIdForLog))
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
            End Try
            Return returnStatus
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' send the confirmation email as a test
        ''' </summary>
        ''' <param name="EmailID"></param>
        ''' <param name="ConfirmationMemberID"></param>
        Public Sub sendConfirmationTest(ByVal EmailID As Integer, ByVal ConfirmationMemberID As Integer)
            Try
                Dim ConfirmFooter As String
                Dim TotalCnt As Integer
                Dim BlankCnt As Integer
                Dim DupCnt As Integer
                Dim DupList As String
                Dim BadCnt As Integer
                Dim BadList As String

                Dim EmailLen As Integer
                Dim Pos As Integer

                Dim LastEmail As String
                Dim Emailtext As String
                Dim LastDupEmail As String
                Dim EmailLine As String
                Dim TotalList As String
                Dim EMailName As String
                Dim EmailMemberID As Integer
                Dim Posat As Integer
                Dim PosDot As Integer
                Dim CS As Integer
                Dim EmailSubject As String
                Dim EmailBody As String
                Dim EmailTemplate As String
                Dim EMailTemplateID As Integer
                Dim CSTemplate As Integer
                Dim SpamFooter As String
                Dim CSPeople As Integer
                Dim SQL As String
                Dim EmailStatus As String
                Dim emailstyles As String
                Dim layoutError As String
                '
                CS = cpcore.csOpen("email", EmailID)
                If Not cpcore.db.cs_ok(CS) Then
                    Call cpcore.error_AddUserError("There was a problem sending the email confirmation. The email record could not be found.")
                Else
                    EmailSubject = cpcore.db.cs_get(CS, "Subject")
                    EmailBody = cpcore.db.cs_get(CS, "copyFilename")
                    '
                    ' merge in template
                    '
                    EmailTemplate = ""
                    EMailTemplateID = cpcore.db.cs_getInteger(CS, "EmailTemplateID")
                    If EMailTemplateID <> 0 Then
                        CSTemplate = cpcore.csOpen("Email Templates", EMailTemplateID, , , "BodyHTML")
                        If cpcore.db.cs_ok(CSTemplate) Then
                            EmailTemplate = cpcore.db.cs_get(CSTemplate, "BodyHTML")
                        End If
                        Call cpcore.db.cs_Close(CSTemplate)
                    End If
                    '
                    ' styles
                    '
                    emailstyles = getStyles(EmailID)
                    EmailBody = emailstyles & EmailBody
                    '
                    ' spam footer
                    '
                    If cpcore.db.cs_getBoolean(CS, "AllowSpamFooter") Then
                        '
                        ' This field is default true, and non-authorable
                        ' It will be true in all cases, except a possible unforseen exception
                        '
                        EmailBody = EmailBody & "<div style=""clear:both;padding:10px;"">" & cpcore.csv_GetLinkedText("<a href=""" & cpcore.htmlDoc.html_EncodeHTML(cpcore.webServer.webServerIO_requestProtocol & cpcore.webServer.requestDomain & requestAppRootPath & cpcore.siteProperties.serverPageDefault & "?" & RequestNameEmailSpamFlag & "=#member_email#") & """>", cpcore.siteProperties.getText("EmailSpamFooter", DefaultSpamFooter)) & "</div>"
                        EmailBody = genericController.vbReplace(EmailBody, "#member_email#", "UserEmailAddress")
                    End If
                    '
                    ' Confirm footer
                    '
                    SQL = getGroupSql(EmailID)
                    CSPeople = cpcore.db.cs_openSql(SQL)
                    If Not cpcore.db.cs_ok(CSPeople) Then
                        cpcore.error_AddUserError("There are no valid recipients of this email, other than the confirmation address. Either no groups or topics were selected, or those selections contain no people with both a valid email addresses and 'Allow Group Email' enabled.")
                    Else
                        'TotalList = TotalList & "--- all recipients ---" & BR
                        LastEmail = "empty"
                        Do While cpcore.db.cs_ok(CSPeople)
                            Emailtext = cpcore.db.cs_get(CSPeople, "email")
                            EMailName = cpcore.db.cs_get(CSPeople, "name")
                            EmailMemberID = cpcore.db.cs_getInteger(CSPeople, "ID")
                            If EMailName = "" Then
                                EMailName = "no name (member id " & EmailMemberID & ")"
                            End If
                            EmailLine = Emailtext & " for " & EMailName
                            If Emailtext = "" Then
                                BlankCnt = BlankCnt + 1
                            Else
                                If Emailtext = LastEmail Then
                                    DupCnt = DupCnt + 1
                                    If Emailtext <> LastDupEmail Then
                                        DupList = DupList & "<div class=i>" & Emailtext & "</div>" & BR
                                        LastDupEmail = Emailtext
                                    End If
                                End If
                            End If
                            EmailLen = Len(Emailtext)
                            Posat = genericController.vbInstr(1, Emailtext, "@")
                            PosDot = InStrRev(Emailtext, ".")
                            If EmailLen < 6 Then
                                BadCnt = BadCnt + 1
                                BadList = BadList & EmailLine & BR
                            ElseIf (Posat < 2) Or (Posat > (EmailLen - 4)) Then
                                BadCnt = BadCnt + 1
                                BadList = BadList & EmailLine & BR
                            ElseIf (PosDot < 4) Or (PosDot > (EmailLen - 2)) Then
                                BadCnt = BadCnt + 1
                                BadList = BadList & EmailLine & BR
                            End If
                            TotalList = TotalList & EmailLine & BR
                            LastEmail = Emailtext
                            TotalCnt = TotalCnt + 1
                            Call cpcore.db.cs_goNext(CSPeople)
                        Loop
                        'TotalList = TotalList & "--- end all recipients ---" & BR
                    End If
                    Call cpcore.db.cs_Close(CSPeople)
                    '
                    If DupCnt = 1 Then
                        Call cpcore.error_AddUserError("There is 1 duplicate email address. See the test email for details.")
                        ConfirmFooter = ConfirmFooter & "<div style=""clear:all"">WARNING: There is 1 duplicate email address. Only one email will be sent to each address. If the email includes personalization, or if you are using link authentication to automatically log in the user, you may want to correct duplicates to be sure the email is created correctly.<div style=""margin:20px;"">" & DupList & "</div></div>"
                    ElseIf DupCnt > 1 Then
                        Call cpcore.error_AddUserError("There are " & DupCnt & " duplicate email addresses. See the test email for details")
                        ConfirmFooter = ConfirmFooter & "<div style=""clear:all"">WARNING: There are " & DupCnt & " duplicate email addresses. Only one email will be sent to each address. If the email includes personalization, or if you are using link authentication to automatically log in the user, you may want to correct duplicates to be sure the email is created correctly.<div style=""margin:20px;"">" & DupList & "</div></div>"
                    End If
                    '
                    If BadCnt = 1 Then
                        Call cpcore.error_AddUserError("There is 1 invalid email address. See the test email for details.")
                        ConfirmFooter = ConfirmFooter & "<div style=""clear:all"">WARNING: There is 1 invalid email address<div style=""margin:20px;"">" & BadList & "</div></div>"
                    ElseIf BadCnt > 1 Then
                        Call cpcore.error_AddUserError("There are " & BadCnt & " invalid email addresses. See the test email for details")
                        ConfirmFooter = ConfirmFooter & "<div style=""clear:all"">WARNING: There are " & BadCnt & " invalid email addresses<div style=""margin:20px;"">" & BadList & "</div></div>"
                    End If
                    '
                    If BlankCnt = 1 Then
                        Call cpcore.error_AddUserError("There is 1 blank email address. See the test email for details")
                        ConfirmFooter = ConfirmFooter & "<div style=""clear:all"">WARNING: There is 1 blank email address.</div>"
                    ElseIf BlankCnt > 1 Then
                        Call cpcore.error_AddUserError("There are " & DupCnt & " blank email addresses. See the test email for details.")
                        ConfirmFooter = ConfirmFooter & "<div style=""clear:all"">WARNING: There are " & BlankCnt & " blank email addresses.</div>"
                    End If
                    '
                    If TotalCnt = 0 Then
                        ConfirmFooter = ConfirmFooter & "<div style=""clear:all"">WARNING: There are no recipients for this email.</div>"
                    ElseIf TotalCnt = 1 Then
                        ConfirmFooter = ConfirmFooter & "<div style=""clear:all"">There is 1 recipient<div style=""margin:20px;"">" & TotalList & "</div></div>"
                    Else
                        ConfirmFooter = ConfirmFooter & "<div style=""clear:all"">There are " & TotalCnt & " recipients<div style=""margin:20px;"">" & TotalList & "</div></div>"
                    End If
                    '
                    If ConfirmationMemberID = 0 Then
                        cpcore.error_AddUserError("No confirmation email was send because a Confirmation member is not selected")
                    Else
                        EmailBody = EmailBody & "<div style=""clear:both;padding:10px;margin:10px;border:1px dashed #888;"">Administrator<br><br>" & ConfirmFooter & "</div>"
                        EmailStatus = sendPerson(ConfirmationMemberID, cpcore.db.cs_getText(CS, "FromAddress"), EmailSubject, EmailBody, True, True, EmailID, EmailTemplate, False)
                        If EmailStatus <> "ok" Then
                            cpcore.error_AddUserError(EmailStatus)
                        End If
                    End If
                End If
                Call cpcore.db.cs_Close(CS)
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '========================================================================
        ' main_SendFormEmail
        '   sends an email with the contents of a form
        '========================================================================
        '
        Public Sub sendForm(ByVal SendTo As String, ByVal SendFrom As String, ByVal SendSubject As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SendFormEmail")
            '
            'If Not (true) Then Exit Sub
            '
            Dim Message As String
            Dim subject As String
            Dim Result As String
            Dim RequestFormElementVariant As Object
            Dim MethodName As String
            Dim iSendTo As String
            Dim iSendFrom As String
            Dim iSendSubject As String
            Dim Pointer As Integer
            '
            iSendTo = genericController.encodeText(SendTo)
            iSendFrom = genericController.encodeText(SendFrom)
            iSendSubject = genericController.encodeText(SendSubject)
            '
            MethodName = "main_SendFormEmail"
            '
            If ((InStr(iSendTo, "@") = 0)) Then
                iSendTo = cpcore.siteProperties.getText("TrapEmail")
                iSendSubject = "EmailForm with bad Sendto address"
                Message = "Subject: " & iSendSubject
                Message = Message & vbCrLf
            End If
            Message = Message & "The form was submitted " & cpcore.app_startTime & vbCrLf
            Message = Message & vbCrLf
            Message = Message & "All text fields are included, completed or not." & vbCrLf
            Message = Message & "Only those checkboxes that are checked are included." & vbCrLf
            Message = Message & "Entries are not in the order they appeared on the form." & vbCrLf
            Message = Message & vbCrLf
            For Each key As String In cpcore.docProperties.getKeyList
                With cpcore.docProperties.getProperty(key)
                    If .IsForm Then
                        If genericController.vbUCase(.Value) = "ON" Then
                            Message = Message & .Name & ": Yes" & vbCrLf & vbCrLf
                        Else
                            Message = Message & .Name & ": " & .Value & vbCrLf & vbCrLf
                        End If
                    End If
                End With
            Next
            '
            Call send_Legacy(iSendTo, iSendFrom, iSendSubject, Message, , False, False)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpcore.handleLegacyError18(MethodName)
            '
        End Sub
        '
        '
        '
        Public Sub sendGroup(ByVal GroupList As String, ByVal FromAddress As String, ByVal subject As String, ByVal Body As String, ByVal Immediate As Boolean, ByVal HTML As Boolean)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00271")
            '
            'If Not (true) Then Exit Sub
            '
            Dim rootUrl As String
            Dim MethodName As String
            Dim Groups() As String
            Dim GroupCount As Integer
            Dim GroupPointer As Integer
            Dim iiGroupList As String
            Dim ParsePosition As Integer
            Dim iGroupList As String
            Dim iFromAddress As String
            Dim iSubjectSource As String
            Dim iSubject As String
            Dim iBodySource As String
            Dim iBody As String
            Dim iImmediate As Boolean
            Dim iHTML As Boolean
            Dim SQL As String
            Dim CSPointer As Integer
            Dim ToMemberID As Integer
            '
            MethodName = "main_SendGroupEmail"
            '
            iGroupList = genericController.encodeText(GroupList)
            iFromAddress = genericController.encodeText(FromAddress)
            iSubjectSource = genericController.encodeText(subject)
            iBodySource = genericController.encodeText(Body)
            iImmediate = genericController.EncodeBoolean(Immediate)
            iHTML = genericController.EncodeBoolean(HTML)
            '
            ' Fix links for HTML send - must do it now before encodehtml so eid links will attach
            '
            rootUrl = "http://" & cpcore.webServer.webServerIO_requestDomain & requestAppRootPath
            iBodySource = genericController.ConvertLinksToAbsolute(iBodySource, rootUrl)
            '
            ' Build the list of groups
            '
            If iGroupList <> "" Then
                iiGroupList = iGroupList
                Do While iiGroupList <> ""
                    ReDim Preserve Groups(GroupCount)
                    ParsePosition = genericController.vbInstr(1, iiGroupList, ",")
                    If ParsePosition = 0 Then
                        Groups(GroupCount) = iiGroupList
                        iiGroupList = ""
                    Else
                        Groups(GroupCount) = Mid(iiGroupList, 1, ParsePosition - 1)
                        iiGroupList = Mid(iiGroupList, ParsePosition + 1)
                    End If
                    GroupCount = GroupCount + 1
                Loop
            End If
            If GroupCount > 0 Then
                '
                ' Build the SQL statement
                '
                SQL = "SELECT DISTINCT ccMembers.ID" _
                    & " FROM (ccMembers LEFT JOIN ccMemberRules ON ccMembers.ID = ccMemberRules.MemberID) LEFT JOIN ccgroups ON ccMemberRules.GroupID = ccgroups.ID" _
                    & " WHERE (((ccMembers.Active)<>0) AND ((ccMembers.AllowBulkEmail)<>0) AND ((ccMemberRules.Active)<>0) AND ((ccgroups.Active)<>0) AND ((ccgroups.AllowBulkEmail)<>0)AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" & cpcore.db.encodeSQLDate(cpcore.app_startTime) & ")) AND ("
                For GroupPointer = 0 To GroupCount - 1
                    If GroupPointer = 0 Then
                        SQL &= "(ccgroups.Name=" & cpcore.db.encodeSQLText(Groups(GroupPointer)) & ")"
                    Else
                        SQL &= "OR(ccgroups.Name=" & cpcore.db.encodeSQLText(Groups(GroupPointer)) & ")"
                    End If
                Next
                SQL &= "));"
                CSPointer = cpcore.db.cs_openSql(SQL)
                Do While cpcore.db.cs_ok(CSPointer)
                    ToMemberID = genericController.EncodeInteger(cpcore.db.cs_getInteger(CSPointer, "ID"))
                    iSubject = iSubjectSource
                    iBody = iBodySource
                    '


                    ' send
                    '
                    Call sendPerson(ToMemberID, iFromAddress, iSubject, iBody, iImmediate, iHTML, 0, "", False)
                    Call cpcore.db.cs_goNext(CSPointer)
                Loop
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpcore.handleLegacyError18(MethodName)
            '
        End Sub
        '
        ' ----- Need to test this and make it public
        '
        '   This is what the admin site should call for both test and group email
        '   Making it public lets developers send email that administrators can control
        '
        Public Sub sendSystem_Legacy(ByVal EMailName As String, Optional ByVal AdditionalCopy As String = "", Optional ByVal AdditionalMemberID As Integer = 0)
            Dim EmailStatus As String
            '
            EmailStatus = sendSystem(genericController.encodeText(EMailName), genericController.encodeText(AdditionalCopy), genericController.EncodeInteger(AdditionalMemberID))
            If cpcore.authContext.isAuthenticatedAdmin(cpcore) And (EmailStatus <> "") Then
                cpcore.error_AddUserError("Administrator: There was a problem sending the confirmation email, " & EmailStatus)
            End If
            Exit Sub
        End Sub

        '
        '   2.1 compatibility
        '
        Public Function getPasswordRecoveryForm() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00278")
            '
            'If Not (true) Then Exit Function
            '
            getPasswordRecoveryForm = cpcore.htmlDoc.getSendPasswordForm()
            '
            Exit Function
ErrorTrap:
            Call cpcore.handleLegacyError18("main_GetFormSendPassword")
        End Function
        '
        '=============================================================================
        ' Send the Member his username and password
        '=============================================================================
        '
        Public Function sendPassword(ByVal Email As Object) As Boolean
            Dim returnREsult As Boolean = False
            Try
                Dim sqlCriteria As String
                Dim Message As String = ""
                Dim CS As Integer
                Dim workingEmail As String
                Dim FromAddress As String = ""
                Dim subject As String = ""
                Dim allowEmailLogin As Boolean
                Dim Password As String
                Dim Username As String
                Dim updateUser As Boolean
                Dim atPtr As Integer
                Dim Index As Integer
                Dim EMailName As String
                Dim usernameOK As Boolean
                Dim recordCnt As Integer
                Dim Ptr As Integer
                '
                Const passwordChrs As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ012345678999999"
                Const passwordChrsLength As Integer = 62
                '
                workingEmail = genericController.encodeText(Email)
                '
                returnREsult = False
                If workingEmail = "" Then
                    'hint = "110"
                    cpcore.error_AddUserError("Please enter your email address before requesting your username and password.")
                Else
                    'hint = "120"
                    atPtr = genericController.vbInstr(1, workingEmail, "@")
                    If atPtr < 2 Then
                        '
                        ' email not valid
                        '
                        'hint = "130"
                        cpcore.error_AddUserError("Please enter a valid email address before requesting your username and password.")
                    Else
                        'hint = "140"
                        EMailName = vbMid(workingEmail, 1, atPtr - 1)
                        '
                        Call cpcore.log_LogActivity2("password request for email " & workingEmail, cpcore.authContext.authContextUser.id, cpcore.authContext.authContextUser.organizationId)
                        '
                        allowEmailLogin = cpcore.siteProperties.getBoolean("allowEmailLogin", False)
                        recordCnt = 0
                        sqlCriteria = "(email=" & cpcore.db.encodeSQLText(workingEmail) & ")"
                        If True Then
                            sqlCriteria = sqlCriteria & "and((dateExpires is null)or(dateExpires>" & cpcore.db.encodeSQLDate(DateTime.Now) & "))"
                        End If
                        CS = cpcore.db.cs_open("People", sqlCriteria, "ID", SelectFieldList:="username,password", PageSize:=1)
                        If Not cpcore.db.cs_ok(CS) Then
                            '
                            ' valid login account for this email not found
                            '
                            If (LCase(vbMid(workingEmail, atPtr + 1)) = "contensive.com") Then
                                '
                                ' look for expired account to renew
                                '
                                Call cpcore.db.cs_Close(CS)
                                CS = cpcore.db.cs_open("People", "((email=" & cpcore.db.encodeSQLText(workingEmail) & "))", "ID", PageSize:=1)
                                If cpcore.db.cs_ok(CS) Then
                                    '
                                    ' renew this old record
                                    '
                                    'hint = "150"
                                    Call cpcore.db.cs_setField(CS, "developer", "1")
                                    Call cpcore.db.cs_setField(CS, "admin", "1")
                                    Call cpcore.db.cs_setField(CS, "dateExpires", DateTime.Now.AddDays(7).Date.ToString())
                                Else
                                    '
                                    ' inject support record
                                    '
                                    'hint = "150"
                                    Call cpcore.db.cs_Close(CS)
                                    CS = cpcore.db.cs_insertRecord("people")
                                    Call cpcore.db.cs_setField(CS, "name", "Contensive Support")
                                    Call cpcore.db.cs_setField(CS, "email", workingEmail)
                                    Call cpcore.db.cs_setField(CS, "developer", "1")
                                    Call cpcore.db.cs_setField(CS, "admin", "1")
                                    Call cpcore.db.cs_setField(CS, "dateExpires", DateTime.Now.AddDays(7).Date.ToString())
                                End If
                                Call cpcore.db.cs_save2(CS)
                            Else
                                'hint = "155"
                                cpcore.error_AddUserError("No current user was found matching this email address. Please try again. ")
                            End If
                        End If
                        If cpcore.db.cs_ok(CS) Then
                            'hint = "160"
                            FromAddress = cpcore.siteProperties.getText("EmailFromAddress", "info@" & cpcore.webServer.webServerIO_requestDomain)
                            subject = "Password Request at " & cpcore.webServer.webServerIO_requestDomain
                            Message = ""
                            Do While cpcore.db.cs_ok(CS)
                                'hint = "170"
                                updateUser = False
                                If Message = "" Then
                                    'hint = "180"
                                    Message = "This email was sent in reply to a request at " & cpcore.webServer.webServerIO_requestDomain & " for the username and password associated with this email address. "
                                    Message = Message & "If this request was made by you, please return to the login screen and use the following:" & vbCrLf
                                    Message = Message & vbCrLf
                                Else
                                    'hint = "190"
                                    Message = Message & vbCrLf
                                    Message = Message & "Additional user accounts with the same email address: " & vbCrLf
                                End If
                                '
                                ' username
                                '
                                'hint = "200"
                                Username = cpcore.db.cs_getText(CS, "Username")
                                usernameOK = True
                                If Not allowEmailLogin Then
                                    'hint = "210"
                                    If Username <> Username.Trim() Then
                                        'hint = "220"
                                        Username = Username.Trim()
                                        updateUser = True
                                    End If
                                    If Username = "" Then
                                        'hint = "230"
                                        'username = emailName & Int(Rnd() * 9999)
                                        usernameOK = False
                                        Ptr = 0
                                        Do While Not usernameOK And (Ptr < 100)
                                            'hint = "240"
                                            Username = EMailName & Int(Rnd() * 9999)
                                            usernameOK = Not cpcore.main_IsLoginOK(Username, "test")
                                            Ptr = Ptr + 1
                                        Loop
                                        'hint = "250"
                                        If usernameOK Then
                                            updateUser = True
                                        End If
                                    End If
                                    'hint = "260"
                                    Message = Message & " username: " & Username & vbCrLf
                                End If
                                'hint = "270"
                                If usernameOK Then
                                    '
                                    ' password
                                    '
                                    'hint = "280"
                                    Password = cpcore.db.cs_getText(CS, "Password")
                                    If Password.Trim() <> Password Then
                                        'hint = "290"
                                        Password = Password.Trim()
                                        updateUser = True
                                    End If
                                    'hint = "300"
                                    If Password = "" Then
                                        'hint = "310"
                                        For Ptr = 0 To 8
                                            'hint = "320"
                                            Index = CInt(Rnd() * passwordChrsLength)
                                            Password = Password & vbMid(passwordChrs, Index, 1)
                                        Next
                                        'hint = "330"
                                        updateUser = True
                                    End If
                                    'hint = "340"
                                    Message = Message & " password: " & Password & vbCrLf
                                    returnREsult = True
                                    If updateUser Then
                                        'hint = "350"
                                        Call cpcore.db.cs_set(CS, "username", Username)
                                        Call cpcore.db.cs_set(CS, "password", Password)
                                    End If
                                    recordCnt = recordCnt + 1
                                End If
                                cpcore.db.cs_goNext(CS)
                            Loop
                        End If
                    End If
                End If
                'hint = "360"
                If returnREsult Then
                    Call cpcore.email.send_Legacy(workingEmail, FromAddress, subject, Message, 0, True, False)
                    '    main_ClosePageHTML = main_ClosePageHTML & main_GetPopupMessage(app.publicFiles.ReadFile("ccLib\Popup\PasswordSent.htm"), 300, 300, "no")
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function

        '
        '====================================================================================================
#Region " IDisposable Support "
        '
        ' this class must implement System.IDisposable
        ' never throw an exception in dispose
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        '====================================================================================================
        '
        Protected disposed As Boolean = False

        Public Sub New(cpCore As coreClass)
            Me.cpcore = cpCore
        End Sub
        '
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        '
        Protected Overrides Sub Finalize()
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(False)
            MyBase.Finalize()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' dispose.
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Me.disposed = True
                If disposing Then
                    'If (cacheClient IsNot Nothing) Then
                    '    cacheClient.Dispose()
                    'End If
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region
    End Class
    '
End Namespace