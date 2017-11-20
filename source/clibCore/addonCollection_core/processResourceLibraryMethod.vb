
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.Core
    Public Class processResourceLibraryMethodClass
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
                ' -- resource library
                Call cpCore.doc.addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageResourceLibrary)
                Dim EditorObjectName As String = cpCore.docProperties.getText("EditorObjectName")
                Dim LinkObjectName As String = cpCore.docProperties.getText("LinkObjectName")
                If EditorObjectName <> "" Then
                    '
                    ' Open a page compatible with a dialog
                    '
                    Call cpCore.doc.addRefreshQueryString("EditorObjectName", EditorObjectName)
                    Call cpCore.html.addScriptLink_Head("/ccLib/ClientSide/dialogs.js", "Resource Library")
                    'Call AddHeadScript("<script type=""text/javascript"" src=""/ccLib/ClientSide/dialogs.js""></script>")
                    Call cpCore.doc.setMetaContent(0, 0)
                    Call cpCore.html.addScript_onLoad("document.body.style.overflow='scroll';", "Resource Library")
                    Dim Copy As String = cpCore.html.main_GetResourceLibrary2("", True, EditorObjectName, LinkObjectName, True)
                    Dim htmlBody As String = "" _
                        & genericController.htmlIndent(cpCore.html.main_GetPanelHeader("Contensive Resource Library")) _
                        & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td>" _
                        & cr2 & "<div style=""border-top:1px solid white;border-bottom:1px solid black;height:2px""><img alt=""spacer"" src=""/ccLib/images/spacer.gif"" width=1 height=1></div>" _
                        & genericController.htmlIndent(Copy) _
                        & cr & "</td></tr>" _
                        & cr & "<tr><td>" _
                        & genericController.htmlIndent(cpCore.html.getHtmlDoc_beforeEndOfBodyHtml(False, False, False)) _
                        & cr & "</td></tr></table>" _
                        & cr & "<script language=javascript type=""text/javascript"">fixDialog();</script>" _
                        & ""
                    Dim htmlBodyTag As String = "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">"
                    result = cpCore.html.getHtmlDoc(htmlBody, htmlBodyTag, False, False, False)
                    cpCore.doc.continueProcessing = False
                ElseIf LinkObjectName <> "" Then
                    '
                    ' Open a page compatible with a dialog
                    Call cpCore.doc.addRefreshQueryString("LinkObjectName", LinkObjectName)
                    Call cpCore.html.addScriptLink_Head("/ccLib/ClientSide/dialogs.js", "Resource Library")
                    Call cpCore.doc.setMetaContent(0, 0)
                    Call cpCore.html.addScript_onLoad("document.body.style.overflow='scroll';", "Resource Library")
                    Dim htmlBody As String = "" _
                        & cpCore.html.main_GetPanelHeader("Contensive Resource Library") _
                        & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td>" _
                        & cpCore.html.main_GetResourceLibrary2("", True, EditorObjectName, LinkObjectName, True) _
                        & cr & "</td></tr></table>" _
                        & cr & "<script language=javascript type=text/javascript>fixDialog();</script>" _
                        & ""
                    Dim htmlBodyTag As String = "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">"
                    result = cpCore.html.getHtmlDoc(htmlBody, htmlBodyTag, False, False, False)
                    cpCore.doc.continueProcessing = False
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
