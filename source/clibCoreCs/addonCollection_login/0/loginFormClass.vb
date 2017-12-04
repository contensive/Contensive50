
Option Explicit On
Option Strict On
'
Imports Contensive.Core
'
Namespace Contensive.Addons
    Public Class loginFormClass
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' Login Form
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Overrides Function execute(cp As Contensive.BaseClasses.CPBaseClass) As Object
            Dim returnHtml As String = ""
            Try
                Dim processor As CPClass = DirectCast(cp, CPClass)
                Dim forceDefaultLogin As Boolean = cp.Doc.GetBoolean("Force Default Login")
                returnHtml = Controllers.loginController.getLoginForm(processor.core, forceDefaultLogin)
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return returnHtml
        End Function
    End Class
End Namespace
