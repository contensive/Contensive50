
Option Explicit On
Option Strict On

Imports Contensive.Core.constants
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Namespace Contensive.Core

    Module tasksModule

        Public Sub Main()
            Dim Tasks As tasksClass
            Dim cp As New CPClass() ' move tasks to background processes
            '
            If PrevInstance Then
                Exit Sub
            Else
                Tasks = New tasksClass(cp.core)
                Call Tasks.Process()
                Tasks = Nothing
            End If

        End Sub

    End Module
End Namespace
