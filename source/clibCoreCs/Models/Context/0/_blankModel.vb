
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
    Public Class _blankModel
        '
        Private cpCore As coreClass
        '
        '====================================================================================================
        ''' <summary>
        ''' new
        ''' </summary>
        ''' <param name="cpCore"></param>
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
        '
        '
        '
    End Class
End Namespace