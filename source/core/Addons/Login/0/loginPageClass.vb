
Option Explicit On
Option Strict On
'
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Imports System.Xml
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Addons
    '
    Public Class loginPageClass
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' addon method, deliver complete Html admin site
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Overrides Function execute(cp As Contensive.BaseClasses.CPBaseClass) As Object
            Dim returnHtml As String = ""
            Try
                Dim processor As CPClass = DirectCast(cp, CPClass)
                Dim cpCore As coreClass = processor.core
                returnHtml = Controllers.loginController.getLoginPage(cpCore, False)
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return returnHtml
        End Function
    End Class
End Namespace
