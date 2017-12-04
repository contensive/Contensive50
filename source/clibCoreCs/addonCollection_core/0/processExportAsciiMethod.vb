
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.Core
    Public Class processExportAsciiMethodClass
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
                ' -- Should be a remote method in commerce
                If Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                    '
                    ' Administrator required
                    '
                    cpCore.doc.userErrorList.Add("Error: You must be an administrator to use the ExportAscii method")
                Else
                    Dim ContentName As String = cpCore.docProperties.getText("content")
                    Dim PageSize As Integer = cpCore.docProperties.getInteger("PageSize")
                    If PageSize = 0 Then
                        PageSize = 20
                    End If
                    Dim PageNumber As Integer = cpCore.docProperties.getInteger("PageNumber")
                    If PageNumber = 0 Then
                        PageNumber = 1
                    End If
                    If (ContentName = "") Then
                        cpCore.doc.userErrorList.Add("Error: ExportAscii method requires ContentName")
                    Else
                        result = Controllers.exportAsciiController.exportAscii_GetAsciiExport(cpCore, ContentName, PageSize, PageNumber)
                        cpCore.doc.continueProcessing = False
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
