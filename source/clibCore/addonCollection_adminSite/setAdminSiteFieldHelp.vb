
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
    Public Class setAdminSiteFieldHelpClass
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' setAdminSiteFieldHelp remote method
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Overrides Function execute(cp As Contensive.BaseClasses.CPBaseClass) As Object
            Dim result As String = ""
            Try
                Dim processor As CPClass = DirectCast(cp, CPClass)
                Dim cpCore As coreClass = processor.core
                If (cp.User.IsAdmin) Then
                    Dim fieldId As Integer = cp.Doc.GetInteger("fieldId")
                    Dim help As Models.Entity.ContentFieldHelpModel = Models.Entity.ContentFieldHelpModel.createByFieldId(cpCore, fieldId)
                    If (help Is Nothing) Then
                        help = ContentFieldHelpModel.add(cpCore)
                        help.FieldID = fieldId
                    End If
                    help.HelpCustom = cp.Doc.GetText("helpcustom")
                    help.save(cpCore)
                    Dim contentField As Models.Entity.contentFieldModel = Models.Entity.contentFieldModel.create(cpCore, fieldId)
                    If (contentField IsNot Nothing) Then
                        Models.Complex.cdefModel.invalidateCache(cpCore, contentField.ContentID)
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
