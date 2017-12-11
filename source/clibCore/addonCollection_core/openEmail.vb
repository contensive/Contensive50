
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.Core
    Public Class openEmailClass
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
                Dim emailDropId As Integer = cpCore.docProperties.getInteger(rnEmailOpenFlag)
                If (emailDropId <> 0) Then
                    '
                    ' -- Email open detected. Log it and redirect to a 1x1 spacer
                    Dim emailDrop As emailDropModel = emailDropModel.create(cpCore, emailDropId)
                    If (emailDrop IsNot Nothing) Then
                        Dim recipient As personModel = personModel.create(cpCore, cpCore.docProperties.getInteger(rnEmailMemberID), New List(Of String))
                        If (recipient IsNot Nothing) Then
                            Dim log As New emailLogModel() With {
                                .name = "User " & recipient.name & " opened email drop " & emailDrop.name & " at " & cpCore.doc.profileStartTime.ToString(),
                                .EmailDropID = emailDrop.id,
                                .MemberID = recipient.id,
                                .LogType = EmailLogTypeOpen
                            }
                        End If
                        Call cpCore.webServer.redirect(
                            NonEncodedLink:=cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & "/ccLib/images/spacer.gif",
                            RedirectReason:="Group Email Open hit, redirecting to a dummy image",
                            IsPageNotFound:=False,
                            allowDebugMessage:=False
                        )
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
