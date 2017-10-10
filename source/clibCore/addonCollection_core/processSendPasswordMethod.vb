
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.Core
    Public Class processSendPasswordMethodClass
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
                ' -- send password
                Dim Emailtext As String = cpCore.docProperties.getText("email")
                If Emailtext <> "" Then
                    Call cpCore.email.sendPassword(Emailtext)
                    result &= "" _
                        & "<div style=""width:300px;margin:100px auto 0 auto;"">" _
                        & "<p>An attempt to send login information for email address '" & Emailtext & "' has been made.</p>" _
                        & "<p><a href=""?" & cpCore.doc.refreshQueryString & """>Return to the Site.</a></p>" _
                        & "</div>"
                    cpCore.continueProcessing = False
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
