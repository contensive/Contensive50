
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.AdminSite
    Public Class openAjaxIndexFilterGetContentClass
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
                Call cpCore.visitProperty.setProperty("IndexFilterOpen", "1")
                Dim adminSite As New Contensive.Addons.AdminSite.getAdminSiteClass(cpCore.cp_forAddonExecutionOnly)
                Dim ContentID As Integer = cpCore.docProperties.getInteger("cid")
                If ContentID = 0 Then
                    result = "No filter is available"
                Else
                    Dim cdef As Models.Complex.cdefModel = Models.Complex.cdefModel.getCdef(cpCore, ContentID)
                    result = adminSite.GetForm_IndexFilterContent(cdef)
                End If
                adminSite = Nothing


            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
