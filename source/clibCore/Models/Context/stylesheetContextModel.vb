
Option Explicit On
Option Strict On

Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Namespace Contensive.Core.Models.Context
    '
    '====================================================================================================
    ''' <summary>
    ''' Site Properties
    ''' </summary>
    Public Class stylesheetContextModel
        Public Property templateId As Integer
        Public Property EmailID As Integer
        Public Property StyleSheet As String
    End Class
End Namespace