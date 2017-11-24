
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.Core
    Public Class faviconIcoClass
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
                Dim Filename As String = cpCore.siteProperties.getText("FaviconFilename", "")
                If Filename = "" Then
                    '
                    ' no favicon, 404 the call
                    '
                    Call cpCore.webServer.setResponseStatus("404 Not Found")
                    Call cpCore.webServer.setResponseContentType("image/gif")
                    cpCore.doc.continueProcessing = False
                    Return String.Empty
                Else
                    cpCore.doc.continueProcessing = False
                    Return cpCore.webServer.redirect(genericController.getCdnFileLink(cpCore, Filename), "favicon request", False, False)
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
