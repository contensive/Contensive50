
Imports Contensive.Core.ccCommonModule
'Imports Contensive.Core
'Imports Contensive.Core
'
Namespace Contensive.Core
    Public Class smtpHandlerClass
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
        Private cpCore As cpCoreClass
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As cpCoreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
        '
        '========================================================================
        '   Compatibility
        '========================================================================
        '
        Public Function Send(ByVal ToAddress As Object, ByVal FromAddress As Object, ByVal SubjectMessage As Object, ByVal BodyMessage As Object, ByVal ResultLogPathPage As Object, ByVal SMTPServer As Object, ByVal Immediate As Boolean, ByVal HTML As Boolean, Optional ByVal EmailOutPath As String = "") As Boolean
            Send = Send2( _
                  EncodeText(ToAddress) _
                , EncodeText(FromAddress) _
                , EncodeText(SubjectMessage) _
                , EncodeText(BodyMessage) _
                , "" _
                , "" _
                , EncodeText(ResultLogPathPage) _
                , EncodeText(SMTPServer) _
                , Immediate _
                , HTML _
                , encodeEmptyText(EmailOutPath, ""))

        End Function
        '
        '========================================================================
        '   Send an email, by queue or immediately
        '
        '   Return all errors in the return string, and set the Error Publics if
        '   someone wants to know more.
        '========================================================================
        '
        Public Function Send2(ByVal EmailTo As String, ByVal EmailFrom As String, ByVal EmailSubject As String, ByVal EmailBody As String, ByVal BounceAddress As String, ByVal ReplyToAddress As String, ByVal ResultLogPathPage As String, ByVal EmailSMTPServer As String, ByVal Immediate As Boolean, ByVal HTML As Boolean, ByVal EmailOutPath As String) As String
            Try
                Dim LogLine As String
                Dim MonthNumber As Integer
                Dim DayNumber As Integer
                Dim FilenameNoExt As String
                'Dim ErrorMessage As String
                'Dim LogLine As String
                'Dim ResumeMessage As String
                Dim FolderFileList As IO.FileInfo()
                'Dim FolderFiles() As String
                'Dim Ptr as integer
                Dim PathFilenameNoExt As String
                'Dim FileDetails() As String
                Dim FileSize As Integer
                'Dim RetryCnt as integer
                'Dim SaveOK As Boolean
                'Dim FileSuffix As String
                Dim LogNamePrefix As String
                Dim LogFolder As String
                Dim Ptr As Integer
                '
                Dim converthtmlToText As converthtmlToTextClass
                Dim Mailer As SMTP5Class
                'Dim kmafs As fileSystemClass
                '
                Dim EmailBodyText As String
                Dim EmailBodyHTML As String
                Dim LogFilename As String
                Dim SendResult As String
                Dim MethodName As String
                '
                MethodName = "Send2"
                '
                '---cpCore.AppendLog("Enter Send2(" & EmailTo & "," & EmailFrom & "," & EmailSubject & "," & EmailBody & "," & BounceAddress & "," & ReplyToAddress & "," & ResultLogPathPage & "," & EmailSMTPServer & "," & Immediate & "," & HTML & "," & EmailOutPath & ")")
                If Not CheckAddress(EmailTo) Then
                    Send2 = "The to-address [" & EmailTo & "] is not valid"
                ElseIf Not CheckAddress(EmailFrom) Then
                    Send2 = "The from-address [" & EmailFrom & "] is not valid"
                ElseIf Not CheckServer(EmailSMTPServer) Then
                    Send2 = "The email server [" & EmailSMTPServer & "] is not valid"
                Else
                    If Not Immediate Then
                        '
                        ' ----- Add the email to the queue
                        '
                        Send2 = AddQueue(EmailTo, EmailFrom, EmailSubject, EmailBody, BounceAddress, ReplyToAddress, ResultLogPathPage, EmailSMTPServer, HTML, EmailOutPath)
                    Else
                        '
                        ' ----- Send the email now
                        '
                        'kma() 'fs = New fileSystemClass
                        Mailer = New SMTP5Class
                        Mailer.ReplyToAddress = ReplyToAddress
                        Mailer.ReturnAddress = BounceAddress
                        If HTML Then
                            '
                            ' ----- send HTML email (and plain text conversion)
                            '
                            converthtmlToText = New converthtmlToTextClass(cpCore)
                            EmailBodyHTML = EmailBody
                            If InStr(1, EmailBodyHTML, "<BODY", vbTextCompare) = 0 Then
                                EmailBodyHTML = "<BODY>" & EmailBodyHTML & "</BODY>"
                            End If
                            If InStr(1, EmailBodyHTML, "<HTML>", vbTextCompare) = 0 Then
                                EmailBodyHTML = "<HTML>" & EmailBodyHTML & "</HTML>"
                            End If
                            EmailBodyText = converthtmlToText.convert(EmailBody)
                            Send2 = Mailer.send(EmailSMTPServer, EmailTo, EmailFrom, EmailSubject, EmailBodyText, "", EmailBodyHTML)
                            converthtmlToText = Nothing
                        Else
                            '
                            ' ----- send plain text email
                            '
                            Send2 = Mailer.send(EmailSMTPServer, EmailTo, EmailFrom, EmailSubject, EmailBody, "")
                        End If
                        Mailer = Nothing
                        '
                        ' ----- clean up the result code for logging (change empty to "OK")
                        '
                        Send2 = Replace(Send2, vbCrLf, "")
                        If Send2 = "" Then
                            SendResult = "OK"
                        Else
                            SendResult = Send2
                        End If
                        '
                        ' ----- Update Email Result Log
                        '
                        If ResultLogPathPage <> "" Then
                            Call cpCore.app.appRootFiles.appendFile(ResultLogPathPage, CStr(Now()) & " delivery attempted to " & EmailTo & "," & SendResult & vbCrLf)
                        End If
                        '
                        ' ----- Update the System Email Log
                        '
                        LogLine = """" & CStr(Now()) & """,""To[" & EmailTo & "]"",""From[" & EmailFrom & "]"",""Bounce[" & BounceAddress & "]"",""Subject[" & EmailSubject & "]"",""Result[" & SendResult & "]""" & vbCrLf
                        Call cpCore.appendLog(LogLine, "email")
                    End If
                End If
                '
            Catch ex As Exception
                cpCore.handleException( ex)
                'Mailer = Nothing
                'converthtmlToText = Nothing
                Send2 = "There was an unexpected error sending the email."
            End Try
        End Function
        '
        '========================================================================
        '   add this email to the email queue
        '========================================================================
        '
        Private Function AddQueue(ByVal ToAddress As Object, ByVal FromAddress As Object, ByVal SubjectMessage As Object, ByVal BodyMessage As Object, ByVal BounceAddress As Object, ByVal ReplyToAddress As Object, ByVal ResultLogPathPage As Object, ByVal SMTPServer As Object, ByVal HTML As Boolean, Optional ByVal EmailOutPath As String = "") As String
            AddQueue = ""
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
            Copy = Copy & EncodeText(SMTPServer) & vbCrLf
            Copy = Copy & EncodeText(ResultLogPathPage) & vbCrLf
            Copy = Copy & EncodeText(ToAddress) & vbCrLf
            Copy = Copy & EncodeText(FromAddress) & vbCrLf
            Copy = Copy & EncodeText(BounceAddress) & vbCrLf
            Copy = Copy & EncodeText(ReplyToAddress) & vbCrLf
            Copy = Copy & EncodeText(SubjectMessage) & vbCrLf
            Copy = Copy & EncodeText(HTML) & vbCrLf
            Copy = Copy & EncodeText(BodyMessage)
            Filename = "Out" & CStr(GetRandomInteger()) & ".txt"
            '
            Call cpCore.app.appRootFiles.SaveFile(iEmailOutPath & Filename, Copy)
            '
            If Err.Number <> 0 Then
                Call HandleClassTrapError("AddQueue", True)
                AddQueue = "There was an unexpected error detected exiting the SMTPHandler AddQueue method [" & Err.Description & "]."
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
            AddQueue = "There was an unexpected error saving the email to the email queue [" & Err.Description & "]."
        End Function
        '
        '========================================================================
        '   Send the emails in the current Queue
        '
        '   Errors here should be logged, but do not bubblethe error up, as the host
        '   here is not the user, but the Service.
        '========================================================================
        '
        Public Sub SendQueue(Optional ByVal EmailOutPath As String = "")
            On Error GoTo ErrorTrap
            '
            Dim SMTP As SMTP5Class
            Dim HTML As Boolean
            Dim LogFile As Object
            Dim LogFilename As String
            Dim iEmailOutPath As String
            Dim MethodName As String
            Dim FileList As IO.FileInfo()
            Dim EOL As Integer
            Dim CommaPosition As Integer
            Dim Filename As String
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
            FileList = cpCore.app.appRootFiles.GetFolderFiles(iEmailOutPath)
            For Each file As IO.FileInfo In FileList
                Copy = cpCore.app.appRootFiles.ReadFile(iEmailOutPath & Filename)
                '
                ' No - no way to manage all the files for now. Later work up something
                'Call cpCore.app.publicFiles.CopyFile(iEmailOutPath & Filename, iEmailOutPath & "sent\" & Filename)
                cpCore.app.appRootFiles.DeleteFile(iEmailOutPath & Filename)
                '
                ' Decode the file into the email arguments
                '
                Dim Line0 As String
                Line0 = EncodeText(ReadLine(Copy))
                If UCase(Mid(Line0, 1, 11)) = "CONTENSIVE " Then
                    '
                    ' Email record (LINE0 IS CONENSIVE AND VERSION)
                    '
                    EmailSMTP = EncodeText(ReadLine(Copy))
                    ResultLogPathPage = EncodeText(ReadLine(Copy))
                    EmailTo = EncodeText(ReadLine(Copy))
                    EmailFrom = EncodeText(ReadLine(Copy))
                    BounceAddress = EncodeText(ReadLine(Copy))
                    ReplyToAddress = EncodeText(ReadLine(Copy))
                    EmailSubject = EncodeText(ReadLine(Copy))
                    HTML = EncodeBoolean(ReadLine(Copy))
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
                    ResultLogPathPage = EncodeText(ReadLine(Copy))
                    EmailTo = EncodeText(ReadLine(Copy))
                    EmailFrom = EncodeText(ReadLine(Copy))
                    EmailSubject = EncodeText(ReadLine(Copy))
                    HTML = EncodeBoolean(ReadLine(Copy))
                    EmailBody = Copy
                End If
                If (EmailSMTP <> "") _
                    And (EmailTo <> "") _
                    And (EmailFrom <> "") Then
                    '
                    ' Send email
                    '
                    Call Send(EmailTo, EmailFrom, EmailSubject, EmailBody, ResultLogPathPage, EmailSMTP, True, HTML, EmailOutPath)
                Else
                    '
                    ' Error, log the problem
                    '
                    Call HandleClassInternalError(KmaErrorUser, "App.EXEName", "Invalid email in send queue [" & Filename & "] was removed", MethodName, True)
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
        Private Function ReadLine(ByVal Body As String) As Object
            On Error GoTo ErrorTrap
            Dim MethodName As String
            Dim EOL As String
            '
            MethodName = "ReadLine"
            '
            EOL = InStr(1, Body, vbCrLf)
            If EOL <> 0 Then
                ReadLine = Mid(Body, 1, EOL - 1)
                Body = Mid(Body, EOL + 2)
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call HandleClassTrapError(MethodName, False)
        End Function
        '
        '
        '
        Public Function ErrorExit(ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String, ByVal MethodName As String, ByVal ErrorTrap As Boolean, ByVal ResumeNext As Boolean) As String
            cpCore.handleLegacyError3("", "unknown", "ccEmail4", "SMTPHandler", MethodName, ErrNumber, ErrSource, ErrDescription, ErrorTrap, ResumeNext, "")
        End Function
        '
        '
        '
        Private Function HandleClassTrapError(ByVal MethodName As String, ByVal ResumeNext As Boolean) As String
            cpCore.handleLegacyError3("", "trap error", "ccEmail4", "SMTPHandlerClass", MethodName, Err.Number, Err.Source, Err.Description, True, ResumeNext, "unknown")
        End Function
        '
        '
        '
        Private Function HandleClassInternalError(ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String, ByVal MethodName As String, ByVal ResumeNext As Boolean) As String
            cpCore.handleLegacyError3("", "internal error", "ccEmail4", "SMTPHandlerClass", MethodName, ErrNumber, ErrSource, ErrDescription, True, ResumeNext, "unknown")
        End Function
        '
        '
        '
        Private Function CheckAddress(ByVal EmailAddress As String) As Boolean
            On Error GoTo ErrorTrap
            '
            Dim SplitArray() As String
            CheckAddress = False
            If EmailAddress <> "" Then
                If InStr(1, EmailAddress, "@") <> 0 Then
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
            ElseIf InStr(1, EmailServer, "SMTP.YourServer.Com", vbTextCompare) <> 0 Then
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
    End Class
End Namespace
