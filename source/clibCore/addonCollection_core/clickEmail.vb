
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.Core
    Public Class clickEmailClass
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
                ' -- Email click detected
                Dim emailDrop As emailDropModel = emailDropModel.create(cpCore, cpCore.docProperties.getInteger(rnEmailClickFlag))
                If (emailDrop IsNot Nothing) Then
                    Dim recipient As personModel = personModel.create(cpCore, cpCore.docProperties.getInteger(rnEmailMemberID), New List(Of String))
                    If (recipient IsNot Nothing) Then
                        Dim log As New Models.Entity.emailLogModel() With {
                            .name = "User " & recipient.name & " clicked link from email drop " & emailDrop.name & " at " & cpCore.profileStartTime.ToString(),
                            .EmailDropID = emailDrop.id,
                            .MemberID = recipient.id,
                            .LogType = EmailLogTypeOpen
                        }
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
