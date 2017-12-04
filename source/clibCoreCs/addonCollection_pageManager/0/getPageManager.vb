
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.PageManager
    '
    Public Class getPageManagerClass
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' pageManager addon interface
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Overrides Function execute(cp As Contensive.BaseClasses.CPBaseClass) As Object
            Dim returnHtml As String = ""
            Try
                Dim processor As CPClass = DirectCast(cp, CPClass)
                Dim cpCore As coreClass = processor.core
                returnHtml = "<div class=""ccBodyWeb"">" & pageContentController.getHtmlBody(cpCore) & "</div>"
                'returnHtml = cpCore.html.getHtmlDoc(htmlBody, TemplateDefaultBodyTag, True, True, False)
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return returnHtml
        End Function
    End Class
End Namespace
