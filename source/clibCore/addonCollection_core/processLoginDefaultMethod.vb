
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.Core
    Public Class processLoginDefaultMethodClass
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
                ' -- default login page
                cpCore.continueProcessing = False
                Dim addonArguments As New Dictionary(Of String, String)
                addonArguments.Add("Force Default Login", "true")
                Return cpCore.addon.execute(
                addonModel.create(cpCore, addonGuidLoginPage),
                    New CPUtilsBaseClass.addonExecuteContext() With {
                        .addonType = CPUtilsBaseClass.addonContext.ContextPage,
                        .instanceArguments = addonArguments
                    }
                )
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
