
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.Core
    Public Class processRedirectMethodClass
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
                ' ----- Redirect with RC and RI
                '
                cpCore.doc.redirectContentID = cpCore.docProperties.getInteger(rnRedirectContentId)
                cpCore.doc.redirectRecordID = cpCore.docProperties.getInteger(rnRedirectRecordId)
                If cpCore.doc.redirectContentID <> 0 And cpCore.doc.redirectRecordID <> 0 Then
                    Dim ContentName As String = cpCore.metaData.getContentNameByID(cpCore.doc.redirectContentID)
                    If ContentName <> "" Then
                        Call iisController.main_RedirectByRecord_ReturnStatus(cpCore, ContentName, cpCore.doc.redirectRecordID)
                        result = ""
                        cpCore.continueProcessing = False
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
