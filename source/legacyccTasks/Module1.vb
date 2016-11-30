
'Imports Contensive.Core
Imports Contensive.Core

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
