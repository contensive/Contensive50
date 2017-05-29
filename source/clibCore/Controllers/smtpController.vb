
Option Explicit On
Option Strict On

Imports System.Net.Mail
Imports System.Net.Mime
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Core.Controllers
    Public Class smtpController
        '
        Public ErrorNumber As Integer
        Public ErrorSource As String
        Public ErrorDescription As String
        '
        '========================================================================
        ' This page and its contents are copyright by Kidwell McGowan Associates.
        '   See Common Module for descriptions
        '
        '   This module handles the common email interface for the Content Server
        '========================================================================
        '
        Private cpCore As coreClass
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
        '
        '========================================================================
        '   Compatibility
        '========================================================================
        '
        Public Function SendEmail4(ByVal ToAddress As Object, ByVal FromAddress As Object, ByVal SubjectMessage As Object, ByVal BodyMessage As Object, ByVal ResultLogPathPage As Object, ByVal SMTPServer As Object, ByVal Immediate As Boolean, ByVal HTML As Boolean, Optional ByVal EmailOutPath As String = "") As Boolean
            Return String.IsNullOrEmpty(sendEmail5(genericController.encodeText(ToAddress) _
                , genericController.encodeText(FromAddress) _
                , genericController.encodeText(SubjectMessage) _
                , genericController.encodeText(BodyMessage) _
                , "" _
                , "" _
                , genericController.encodeText(ResultLogPathPage) _
                , genericController.encodeText(SMTPServer) _
                , Immediate _
                , HTML _
                , encodeEmptyText(EmailOutPath, "")))
        End Function
        '
        '========================================================================
        '   Send an email, by queue or immediately
        '
        '   Return all errors in the return string, and set the Error Publics if
        '   someone wants to know more.
        '========================================================================
        '
        Public Function sendEmail5(ByVal EmailTo As String, ByVal EmailFrom As String, ByVal EmailSubject As String, ByVal EmailBody As String, ByVal BounceAddress As String, ByVal ReplyToAddress As String, ByVal ResultLogPathPage As String, ByVal EmailSMTPServer As String, ByVal Immediate As Boolean, ByVal HTML As Boolean, ByVal EmailOutPath As String) As String
            Try
                Dim LogLine As String
                Dim converthtmlToText As coreHtmlToTextClass
                'Dim Mailer As SMTP5Class
                Dim EmailBodyText As String
                Dim EmailBodyHTML As String
                Dim SendResult As String
                '
                If Not CheckAddress(EmailTo) Then
                    sendEmail5 = "The to-address [" & EmailTo & "] is not valid"
                ElseIf Not CheckAddress(EmailFrom) Then
                    sendEmail5 = "The from-address [" & EmailFrom & "] is not valid"
                ElseIf Not CheckServer(EmailSMTPServer) Then
                    sendEmail5 = "The email server [" & EmailSMTPServer & "] is not valid"
                Else
                    If Not Immediate Then
                        '
                        ' ----- Add the email to the queue
                        '
                        sendEmail5 = addEmailQueue(EmailTo, EmailFrom, EmailSubject, EmailBody, BounceAddress, ReplyToAddress, ResultLogPathPage, EmailSMTPServer, HTML, EmailOutPath)
                    Else
                        '
                        ' ----- Send the email now
                        '
                        'kma() 'fs = New fileSystemClass
                        'Mailer = New SMTP5Class
                        ReplyToAddress = ReplyToAddress
                        ReturnAddress = BounceAddress
                        If HTML Then
                            '
                            ' ----- send HTML email (and plain text conversion)
                            '
                            converthtmlToText = New coreHtmlToTextClass(cpCore)
                            EmailBodyHTML = EmailBody
                            If genericController.vbInstr(1, EmailBodyHTML, "<BODY", vbTextCompare) = 0 Then
                                EmailBodyHTML = "<BODY>" & EmailBodyHTML & "</BODY>"
                            End If
                            If genericController.vbInstr(1, EmailBodyHTML, "<HTML>", vbTextCompare) = 0 Then
                                EmailBodyHTML = "<HTML>" & EmailBodyHTML & "</HTML>"
                            End If
                            EmailBodyText = converthtmlToText.convert(EmailBody)
                            sendEmail5 = sendEmail6(EmailSMTPServer, EmailTo, EmailFrom, EmailSubject, EmailBodyText, "", EmailBodyHTML)
                            'converthtmlToText = Nothing
                        Else
                            '
                            ' ----- send plain text email
                            '
                            sendEmail5 = sendEmail6(EmailSMTPServer, EmailTo, EmailFrom, EmailSubject, EmailBody, "")
                        End If
                        'Mailer = Nothing
                        '
                        ' ----- clean up the result code for logging (change empty to "ok")
                        '
                        sendEmail5 = genericController.vbReplace(sendEmail5, vbCrLf, "")
                        If sendEmail5 = "" Then
                            SendResult = "ok"
                        Else
                            SendResult = sendEmail5
                        End If
                        '
                        ' ----- Update Email Result Log
                        '
                        If ResultLogPathPage <> "" Then
                            Call cpCore.appRootFiles.appendFile(ResultLogPathPage, CStr(Now()) & " delivery attempted to " & EmailTo & "," & SendResult & vbCrLf)
                        End If
                        '
                        ' ----- Update the System Email Log
                        '
                        LogLine = """" & CStr(Now()) & """,""To[" & EmailTo & "]"",""From[" & EmailFrom & "]"",""Bounce[" & BounceAddress & "]"",""Subject[" & EmailSubject & "]"",""Result[" & SendResult & "]""" & vbCrLf
                        Call logController.appendLog(cpCore, LogLine, "email")
                    End If
                End If
                '
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                'Mailer = Nothing
                'converthtmlToText = Nothing
                sendEmail5 = "There was an unexpected error sending the email."
            End Try
        End Function
        '
        '========================================================================
        '   add this email to the email queue
        '========================================================================
        '
        Private Function addEmailQueue(ByVal ToAddress As Object, ByVal FromAddress As Object, ByVal SubjectMessage As Object, ByVal BodyMessage As Object, ByVal BounceAddress As Object, ByVal ReplyToAddress As Object, ByVal ResultLogPathPage As Object, ByVal SMTPServer As Object, ByVal HTML As Boolean, Optional ByVal EmailOutPath As String = "") As String
            addEmailQueue = ""
            On Error GoTo ErrorTrap
            '
            Dim Filename As String
            Dim MethodName As String
            Dim Copy As String
            'Dim kmafs As fileSystemClass
            Dim iEmailOutPath As String
            '
            MethodName = "AddQueue"
            '
            ' ----- Get the email folder
            '
            If EmailOutPath <> "" Then
                If (InStr(1, EmailOutPath, "\") <> Len(EmailOutPath)) Then
                    iEmailOutPath = EmailOutPath & "\"
                Else
                    iEmailOutPath = EmailOutPath
                End If
            Else
                iEmailOutPath = "emailout\"
            End If
            '
            ' ----- write the email to the email queue folder for delivery later
            '
            Copy = ""
            Copy = Copy & "Contensive Handler " & My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & "." & My.Application.Info.Version.Revision & vbCrLf
            Copy = Copy & genericController.encodeText(SMTPServer) & vbCrLf
            Copy = Copy & genericController.encodeText(ResultLogPathPage) & vbCrLf
            Copy = Copy & genericController.encodeText(ToAddress) & vbCrLf
            Copy = Copy & genericController.encodeText(FromAddress) & vbCrLf
            Copy = Copy & genericController.encodeText(BounceAddress) & vbCrLf
            Copy = Copy & genericController.encodeText(ReplyToAddress) & vbCrLf
            Copy = Copy & genericController.encodeText(SubjectMessage) & vbCrLf
            Copy = Copy & genericController.encodeText(HTML) & vbCrLf
            Copy = Copy & genericController.encodeText(BodyMessage)
            Filename = "Out" & CStr(genericController.GetRandomInteger()) & ".txt"
            '
            Call cpCore.appRootFiles.saveFile(iEmailOutPath & Filename, Copy)
            '
            If Err.Number <> 0 Then
                Call HandleClassTrapError("AddQueue", True)
                addEmailQueue = "There was an unexpected error detected exiting the SMTPHandler AddQueue method [" & Err.Description & "]."
                Err.Clear()
            End If
            Exit Function
            '
ErrorTrap:
            ErrorNumber = Err.Number
            ErrorSource = Err.Source
            ErrorDescription = Err.Description
            '
            '
            Call HandleClassTrapError(MethodName, True)
            Err.Clear()
            '
            addEmailQueue = "There was an unexpected error saving the email to the email queue [" & Err.Description & "]."
        End Function
        '
        '========================================================================
        '   Send the emails in the current Queue
        '
        '   Errors here should be logged, but do not bubblethe error up, as the host
        '   here is not the user, but the Service.
        '========================================================================
        '
        Public Sub SendEmailQueue(Optional ByVal EmailOutPath As String = "")
            On Error GoTo ErrorTrap
            '
            'Dim SMTP As SMTP5Class
            Dim HTML As Boolean
            Dim LogFile As Object
            Dim LogFilename As String
            Dim iEmailOutPath As String
            Dim MethodName As String
            Dim FileList As IO.FileInfo()
            Dim EOL As Integer
            Dim CommaPosition As Integer
            Dim Filename As String = ""
            Dim Copy As String
            Dim EmailSMTP As String
            Dim EmailTo As String
            Dim EmailFrom As String
            Dim EmailSubject As String
            Dim EmailBody As String
            Dim ResultLogPathPage As String
            Dim iiEmailOutPath As String
            Dim BounceAddress As String
            Dim ReplyToAddress As String
            '
            MethodName = "SendQueue"
            '
            ' ----- Get the email folder
            '
            If EmailOutPath <> "" Then
                If (Right(EmailOutPath, 1) <> "\") Then
                    iEmailOutPath = EmailOutPath & "\"
                Else
                    iEmailOutPath = EmailOutPath
                End If
            Else
                iEmailOutPath = "emailout\"
            End If
            '
            FileList = cpCore.appRootFiles.getFileList(iEmailOutPath)
            For Each file As IO.FileInfo In FileList
                Copy = cpCore.appRootFiles.readFile(iEmailOutPath & Filename)
                '
                ' No - no way to manage all the files for now. Later work up something
                'Call cpCore.app.publicFiles.CopyFile(iEmailOutPath & Filename, iEmailOutPath & "sent\" & Filename)
                cpCore.appRootFiles.deleteFile(iEmailOutPath & Filename)
                '
                ' Decode the file into the email arguments
                '
                Dim Line0 As String
                Line0 = ReadLine(Copy)
                If genericController.vbUCase(Mid(Line0, 1, 11)) = "CONTENSIVE " Then
                    '
                    ' Email record (LINE0 IS CONENSIVE AND VERSION)
                    '
                    EmailSMTP = ReadLine(Copy)
                    ResultLogPathPage = ReadLine(Copy)
                    EmailTo = ReadLine(Copy)
                    EmailFrom = ReadLine(Copy)
                    BounceAddress = ReadLine(Copy)
                    ReplyToAddress = ReadLine(Copy)
                    EmailSubject = ReadLine(Copy)
                    HTML = genericController.EncodeBoolean(ReadLine(Copy))
                    '
                    ' removed this because the addqueue did not put it in
                    '
                    '                Call ReadLine(Copy)
                    EmailBody = Copy
                Else
                    '
                    ' Legacy record
                    '
                    EmailSMTP = Line0
                    ResultLogPathPage = ReadLine(Copy)
                    EmailTo = ReadLine(Copy)
                    EmailFrom = ReadLine(Copy)
                    EmailSubject = ReadLine(Copy)
                    HTML = genericController.EncodeBoolean(ReadLine(Copy))
                    EmailBody = Copy
                End If
                If (EmailSMTP <> "") _
                    And (EmailTo <> "") _
                    And (EmailFrom <> "") Then
                    '
                    ' Send email
                    '
                    Call SendEmail4(EmailTo, EmailFrom, EmailSubject, EmailBody, ResultLogPathPage, EmailSMTP, True, HTML, EmailOutPath)
                Else
                    '
                    ' Error, log the problem
                    '
                    Call HandleClassInternalError(ignoreInteger, "App.EXEName", "Invalid email in send queue [" & Filename & "] was removed", MethodName, True)
                End If
            Next
            '
            If Err.Number <> 0 Then
                Call HandleClassTrapError("SendQueue", True)
                Err.Clear()
            End If
            Exit Sub
ErrorTrap:
            'kmafs = Nothing
            Call HandleClassTrapError(MethodName, False)
        End Sub
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function ReadLine(ByRef Body As String) As String
            Dim line As String = ""
            Try
                Dim EOL As Integer = genericController.vbInstr(1, Body, vbCrLf)
                If EOL <> 0 Then
                    line = Mid(Body, 1, EOL - 1)
                    Body = Mid(Body, EOL + 2)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return line
        End Function
        ''
        ''
        ''
        'Public sub ErrorExit(ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String, ByVal MethodName As String, ByVal ErrorTrap As Boolean, ByVal ResumeNext As Boolean)
        '    cpCore.handleLegacyError3("", "unknown", "ccEmail4", "SMTPHandler", MethodName, ErrNumber, ErrSource, ErrDescription, ErrorTrap, ResumeNext, "")
        'End Function
        '
        '
        '
        Private Sub HandleClassTrapError(ByVal MethodName As String, ByVal ResumeNext As Boolean)
            cpCore.handleLegacyError3("", "trap error", "ccEmail4", "SMTPHandlerClass", MethodName, Err.Number, Err.Source, Err.Description, True, ResumeNext, "unknown")
        End Sub
        '
        '
        '
        Private Sub HandleClassInternalError(ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String, ByVal MethodName As String, ByVal ResumeNext As Boolean)
            cpCore.handleLegacyError3("", "internal error", "ccEmail4", "SMTPHandlerClass", MethodName, ErrNumber, ErrSource, ErrDescription, True, ResumeNext, "unknown")
        End Sub
        '
        '
        '
        Private Function CheckAddress(ByVal EmailAddress As String) As Boolean
            On Error GoTo ErrorTrap
            '
            Dim SplitArray() As String
            CheckAddress = False
            If EmailAddress <> "" Then
                If genericController.vbInstr(1, EmailAddress, "@") <> 0 Then
                    SplitArray = Split(EmailAddress, "@")
                    If UBound(SplitArray) = 1 Then
                        If Len(SplitArray(0)) > 0 Then
                            CheckAddress = CheckServer(SplitArray(1))
                        End If
                    End If
                End If
            End If
            Exit Function
            '
ErrorTrap:
            CheckAddress = False
            Err.Clear()
        End Function
        '
        ' Server must have at least 3 digits, and one dot in the middle
        '
        Private Function CheckServer(ByVal EmailServer As String) As Boolean
            On Error GoTo ErrorTrap
            '
            Dim SplitArray() As String
            '
            If EmailServer = "" Then
                CheckServer = False
            ElseIf genericController.vbInstr(1, EmailServer, "SMTP.YourServer.Com", vbTextCompare) <> 0 Then
                CheckServer = False
            Else
                SplitArray = Split(EmailServer, ".")
                If UBound(SplitArray) > 0 Then
                    If (Len(SplitArray(0)) > 0) And (Len(SplitArray(1)) > 0) Then
                        CheckServer = True
                    End If
                End If
            End If
            Exit Function
            '
ErrorTrap:
            CheckServer = False
            Err.Clear()
        End Function
        '
        '
        '
        '
        '
        Private LocalReturnAddress As String = ""
        Public Property ReturnAddress As String
            Get
                Return LocalReturnAddress
            End Get
            Set(ByVal value As String)
                LocalReturnAddress = value
            End Set
        End Property
        '
        Private LocalReplyToAddress As String = ""
        Public Property ReplyToAddress As String
            Get
                Return LocalReplyToAddress
            End Get
            Set(ByVal value As String)
                LocalReplyToAddress = value
            End Set
        End Property
        '
        'Public ReplyToAddress As String
        '
        Public Function sendEmail6(ByVal SMTPServer As String, ByVal ToAddress As String, ByVal fromAddress As String, ByVal subject As String, ByVal Body As String, Optional ByVal AttachmentFilename As String = "", Optional ByVal HTMLBody As String = "") As String
            Dim status As String = ""
            Try
                'this is an error
                Dim client As SmtpClient = New SmtpClient(SMTPServer)
                Dim mailMessage As MailMessage = New MailMessage()
                Dim fromAddresses As MailAddress = New MailAddress(fromAddress.Trim())
                Dim data As Attachment
                Dim disposition As ContentDisposition
                Dim mimeType As ContentType
                Dim alternate As AlternateView
                '
                mailMessage.From = fromAddresses
                mailMessage.To.Add(New MailAddress(ToAddress.Trim()))
                mailMessage.Subject = subject
                client.EnableSsl = False
                client.UseDefaultCredentials = False
                '
                If (Body = "") And (HTMLBody <> "") Then
                    '
                    ' html only
                    '
                    mailMessage.Body = HTMLBody
                    mailMessage.IsBodyHtml = True
                ElseIf (Body <> "") And (HTMLBody = "") Then
                    '
                    ' text body only
                    '
                    mailMessage.Body = Body
                    mailMessage.IsBodyHtml = False
                Else
                    '
                    ' both html and text
                    '
                    mailMessage.Body = Body
                    mailMessage.IsBodyHtml = False
                    mimeType = New System.Net.Mime.ContentType("text/html")
                    alternate = AlternateView.CreateAlternateViewFromString(HTMLBody, mimeType)
                    mailMessage.AlternateViews.Add(alternate)
                End If
                '
                ' Create  the file attachment for this e-mail message.
                '
                If (AttachmentFilename <> "") Then
                    data = New Attachment(AttachmentFilename, MediaTypeNames.Application.Octet)
                    disposition = data.ContentDisposition
                    disposition.CreationDate = System.IO.File.GetCreationTime(AttachmentFilename)
                    disposition.ModificationDate = System.IO.File.GetLastWriteTime(AttachmentFilename)
                    disposition.ReadDate = System.IO.File.GetLastAccessTime(AttachmentFilename)
                    mailMessage.Attachments.Add(data)
                End If
                '
                ' Send the message.
                '
                'Add credentials if the SMTP server requires them.
                'client.Credentials = CredentialCache.DefaultNetworkCredentials;
                'client.Credentials = basicCredential;
                '
                ' if there is an error sending, an exception is thrown
                '
                Call client.Send(mailMessage)
                status = "ok"
            Catch ex As Exception
                '
                '
                '
                status = ex.Message
            End Try
            Return status
        End Function        '
        '
    End Class
End Namespace
