
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.Core
    Public Class processSiteStyleEditorClass
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
                If cpCore.doc.authContext.isAuthenticated() And cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                    '
                    ' Save the site sites
                    '
                    Call cpCore.appRootFiles.saveFile(DynamicStylesFilename, cpCore.docProperties.getText("SiteStyles"))
                    If cpCore.docProperties.getBoolean(RequestNameInlineStyles) Then
                        '
                        ' Inline Styles
                        '
                        Call cpCore.siteProperties.setProperty("StylesheetSerialNumber", "0")
                    Else
                        '
                        ' Linked Styles
                        ' Bump the Style Serial Number so next fetch is not cached
                        '
                        Dim StyleSN As Integer = cpCore.siteProperties.getinteger("StylesheetSerialNumber", 0)
                        StyleSN = StyleSN + 1
                        Call cpCore.siteProperties.setProperty("StylesheetSerialNumber", genericController.encodeText(StyleSN))
                        '
                        ' Save new public stylesheet
                        '
                        'Call appRootFiles.saveFile("templates\Public" & StyleSN & ".css", html.html_getStyleSheet2(0, 0))
                        'Call appRootFiles.saveFile("templates\Admin" & StyleSN & ".css", html.getStyleSheetDefault())
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
