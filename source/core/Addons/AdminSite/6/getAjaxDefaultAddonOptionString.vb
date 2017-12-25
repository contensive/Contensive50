
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.AdminSite
    '
    Public Class getAjaxDefaultAddonOptionStringClass
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' getFieldEditorPreference remote method
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Overrides Function execute(cp As Contensive.BaseClasses.CPBaseClass) As Object
            Dim returnHtml As String = ""
            Try
                Dim processor As CPClass = DirectCast(cp, CPClass)
                Dim cpCore As coreClass = processor.core
                '
                ' return the addons defult AddonOption_String
                ' used in wysiwyg editor - addons in select list have no defaultOption_String
                ' because created it is expensive (lookuplists, etc). This is only called
                ' when the addon is double-clicked in the editor after being dropped
                '
                Dim AddonGuid As String = cpCore.docProperties.getText("guid")
                '$$$$$ cache this
                Dim CS As Integer = cpCore.db.csOpen(cnAddons, "ccguid=" & cpCore.db.encodeSQLText(AddonGuid))
                Dim addonArgumentList As String = ""
                Dim addonIsInline As Boolean = False
                If cpCore.db.csOk(CS) Then
                    addonArgumentList = cpCore.db.csGetText(CS, "argumentlist")
                    addonIsInline = cpCore.db.csGetBoolean(CS, "IsInline")
                    returnHtml = addonController.main_GetDefaultAddonOption_String(cpCore, addonArgumentList, AddonGuid, addonIsInline)
                End If
                Call cpCore.db.csClose(CS)
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return returnHtml
        End Function
    End Class
End Namespace
