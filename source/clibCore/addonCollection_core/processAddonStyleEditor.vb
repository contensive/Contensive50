
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.Core
    Public Class processAddonStyleEditorClass
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
                ' save custom styles
                If cpCore.doc.authContext.isAuthenticated() And cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                    Dim addonId As Integer
                    addonId = cpCore.docProperties.getInteger("AddonID")
                    If (addonId > 0) Then
                        Dim styleAddon As addonModel = addonModel.create(cpCore, addonId)
                        If (styleAddon.StylesFilename.content <> cpCore.docProperties.getText("CustomStyles")) Then
                            styleAddon.StylesFilename.content = cpCore.docProperties.getText("CustomStyles")
                            styleAddon.save(cpCore)
                            '
                            ' Clear Caches
                            '
                            Call cpCore.cache.invalidateAllObjectsInContent(addonModel.contentName)
                        End If
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
