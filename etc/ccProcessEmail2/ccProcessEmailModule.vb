
Option Explicit On
Option Strict On

Imports Contensive.Core
Imports Contensive.Core.constants
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Namespace Contensive.Core
    Module ccProcessEmailModule

        Sub Main()
            Dim ProcessEmail As processEmailClass
            Dim cp As CPClass
            '
            If Not PrevInstance() Then
                ' convert to cluster-level object, then do applicaiton work by enumerating applications and using cp for each app
                cp = New CPClass()
                ProcessEmail = New processEmailClass(cp.core)
                Call ProcessEmail.ProcessEmail()
                cp.Dispose()
            End If
        End Sub
    End Module
End Namespace
