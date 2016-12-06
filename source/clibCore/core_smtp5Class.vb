
'Imports System
'Imports System.IO
'Imports System.Reflection
'Imports System.Runtime.InteropServices
'Imports System.Text
'Imports System.Net.Mail
'Imports System.Net.Mime
'Imports System.Net

'Namespace Contensive.Core
'    '<ComVisible(True)> _
'    '<ProgId("smtp5Class")> _
'    '<ComClass(SMTP5Class.ClassId, SMTP5Class.InterfaceId, SMTP5Class.EventsId)> _
'    Public Class SMTP5Class
'        ''
'        ''#Region "COM GUIDs"
'        ''        ' These  GUIDs provide the COM identity for this class 
'        ''        ' and its COM interfaces. If you change them, existing 
'        ''        ' clients will no longer be able to access the class.
'        ''        Public Const ClassId As String = "525c570f-3287-4a0c-a96c-d0934c73a6ec"
'        ''        Public Const InterfaceId As String = "5f8d922e-c0c8-4279-810e-04893e15236a"
'        ''        Public Const EventsId As String = "c77cb3c9-06ab-45eb-a0bf-484ee923035f"
'        ''#End Region
'        ''
'        'Public ErrorNumber As Integer
'        'Public ErrorSource As String
'        'Public ErrorDescription As String
'        '
'        'Private LocalReturnAddress As String = ""
'        'Public Property ReturnAddress As String
'        '    Get
'        '        Return LocalReturnAddress
'        '    End Get
'        '    Set(ByVal value As String)
'        '        LocalReturnAddress = value
'        '    End Set
'        'End Property
'        ''
'        'Private LocalReplyToAddress As String = ""
'        'Public Property ReplyToAddress As String
'        '    Get
'        '        Return LocalReplyToAddress
'        '    End Get
'        '    Set(ByVal value As String)
'        '        LocalReplyToAddress = value
'        '    End Set
'        'End Property
'        ''
'        ''Public ReplyToAddress As String
'        ''
'        'Public Function sendEmail5(ByVal SMTPServer As String, ByVal ToAddress As String, ByVal fromAddress As String, ByVal subject As String, ByVal Body As String, Optional ByVal AttachmentFilename As String = "", Optional ByVal HTMLBody As String = "") As String
'        '    Dim status As String = ""
'        '    Try
'        '        'this is an error
'        '        Dim client As SmtpClient = New SmtpClient(SMTPServer)
'        '        Dim mailMessage As MailMessage = New MailMessage()
'        '        Dim fromAddresses As MailAddress = New MailAddress(fromAddress.Trim())
'        '        Dim data As Attachment
'        '        Dim disposition As ContentDisposition
'        '        Dim mimeType As ContentType
'        '        Dim alternate As AlternateView
'        '        '
'        '        mailMessage.From = fromAddresses
'        '        mailMessage.To.Add(New MailAddress(ToAddress.Trim()))
'        '        mailMessage.Subject = subject
'        '        client.EnableSsl = False
'        '        client.UseDefaultCredentials = False
'        '        '
'        '        If (Body = "") And (HTMLBody <> "") Then
'        '            '
'        '            ' html only
'        '            '
'        '            mailMessage.Body = HTMLBody
'        '            mailMessage.IsBodyHtml = True
'        '        ElseIf (Body <> "") And (HTMLBody = "") Then
'        '            '
'        '            ' text body only
'        '            '
'        '            mailMessage.Body = Body
'        '            mailMessage.IsBodyHtml = False
'        '        Else
'        '            '
'        '            ' both html and text
'        '            '
'        '            mailMessage.Body = Body
'        '            mailMessage.IsBodyHtml = False
'        '            mimeType = New System.Net.Mime.ContentType("text/html")
'        '            alternate = AlternateView.CreateAlternateViewFromString(HTMLBody, mimeType)
'        '            mailMessage.AlternateViews.Add(alternate)
'        '        End If
'        '        '
'        '        ' Create  the file attachment for this e-mail message.
'        '        '
'        '        If (AttachmentFilename <> "") Then
'        '            data = New Attachment(AttachmentFilename, MediaTypeNames.Application.Octet)
'        '            disposition = data.ContentDisposition
'        '            disposition.CreationDate = System.IO.File.GetCreationTime(AttachmentFilename)
'        '            disposition.ModificationDate = System.IO.File.GetLastWriteTime(AttachmentFilename)
'        '            disposition.ReadDate = System.IO.File.GetLastAccessTime(AttachmentFilename)
'        '            mailMessage.Attachments.Add(data)
'        '        End If
'        '        '
'        '        ' Send the message.
'        '        '
'        '        'Add credentials if the SMTP server requires them.
'        '        'client.Credentials = CredentialCache.DefaultNetworkCredentials;
'        '        'client.Credentials = basicCredential;
'        '        Call client.Send(mailMessage)
'        '    Catch ex As Exception
'        '        '
'        '        '
'        '        '
'        '        status = ex.Message
'        '    End Try
'        '    Return status
'        'End Function
'    End Class
'End Namespace