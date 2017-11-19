
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.Core
    Public Class blockEmailClass
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' getFieldEditorPreference remote method
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Overrides Function execute(cp As Contensive.BaseClasses.CPBaseClass) As Object
            Dim result As String = ""
            Try
                Dim processor As CPClass = DirectCast(cp, CPClass)
                Dim cpCore As coreClass = processor.core
                '
                ' -- click spam block detected
                If True Then
                    '
                    Dim recipientEmailToBlock As String = cpCore.docProperties.getText(rnEmailBlockRecipientEmail)
                    If String.IsNullOrEmpty(recipientEmailToBlock) Then
                        Dim recipientList As List(Of personModel) = personModel.createList(cpCore, "(email=" & cpCore.db.encodeSQLText(recipientEmailToBlock) & ")")
                        For Each recipient In recipientList
                            recipient.AllowBulkEmail = False
                            recipient.save(cpCore)
                            '
                            ' -- Email spam footer was clicked, clear the AllowBulkEmail field
                            Call cpCore.email.addToBlockList(recipientEmailToBlock)
                            '
                            ' -- log entry to track the result of this email drop
                            Dim emailDropId As Integer = cpCore.docProperties.getInteger(rnEmailBlockRequestDropID)
                            If emailDropId <> 0 Then
                                Dim emailDrop As emailDropModel = emailDropModel.create(cpCore, emailDropId)
                                If (emailDrop IsNot Nothing) Then
                                    Dim log As New Models.Entity.emailLogModel() With {
                                        .name = "User " & recipient.name & " clicked linked spam block from email drop " & emailDrop.name & " at " & cpCore.doc.profileStartTime.ToString(),
                                        .EmailDropID = emailDrop.id,
                                        .MemberID = recipient.id,
                                        .LogType = EmailLogTypeBlockRequest
                                    }
                                End If
                            End If
                            Call cpCore.webServer.redirect(cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & "/ccLib/popup/EmailBlocked.htm", "Group Email Spam Block hit. Redirecting to EmailBlocked page.", False)
                        Next
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
