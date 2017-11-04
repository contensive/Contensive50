
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.Core
    Public Class processSiteExplorerMethodClass
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
                Call cpCore.doc.addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageSiteExplorer)
                Dim LinkObjectName As String = cpCore.docProperties.getText("LinkObjectName")
                If LinkObjectName <> "" Then
                    '
                    ' Open a page compatible with a dialog
                    '
                    Call cpCore.doc.addRefreshQueryString("LinkObjectName", LinkObjectName)
                    Call cpCore.html.addTitle("Site Explorer")
                    Call cpCore.doc.setMetaContent(0, 0)
                    Dim copy As String = cpCore.addon.execute(
                        addonModel.createByName(cpCore, "Site Explorer"),
                        New CPUtilsBaseClass.addonExecuteContext() With {
                            .addonType = CPUtilsBaseClass.addonContext.ContextPage
                        }
                    )
                    Call cpCore.html.addOnLoadJs("document.body.style.overflow='scroll';", "Site Explorer")
                    Dim htmlBodyTag As String = "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">"
                    Dim htmlBody As String = "" _
                        & genericController.htmlIndent(cpCore.html.main_GetPanelHeader("Contensive Site Explorer")) _
                        & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td>" _
                        & genericController.htmlIndent(copy) _
                        & cr & "</td></tr></table>" _
                        & ""
                    result = cpCore.html.getHtmlDoc(htmlBody, htmlBodyTag, False, False, False)
                    cpCore.continueProcessing = False
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
