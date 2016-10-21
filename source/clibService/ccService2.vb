
'Imports Contensive.Core
'Imports Contensive.Core

Namespace Contensive.Core
    Public Class ccService2
        Dim srvr As workerClass
        '
        Protected Overrides Sub OnStart(ByVal args() As String)
            '
            ' 
            '
            Dim cp As New CPClass("cluster-mode-not-implemented-yet")
            Try
                '
                cp.core.appendLog("ccService2.OnStart enter")
                srvr = New workerClass(cp.core)
                Call srvr.StartServer(True, False)
                cp.core.appendLog("ccService2.OnStart exit")
            Catch ex As Exception
                Call handleExceptionResume(cp.core, ex, "OnStart", "Unexpected Error")
            End Try
        End Sub
        '
        Protected Overrides Sub OnStop()
            ' convert to cluster-level object, then do applicaiton work by enumerating applications and using cp for each app
            Dim cp As New CPClass("cluster-mode-not-implemented-yet")
            Try
                '
                cp.core.appendLog("ccService2.OnStop enter")
                Call srvr.stopServer()
                Call srvr.Dispose()
                srvr = Nothing
                cp.core.appendLog("ccService2.OnStop exit")
            Catch ex As Exception
                Call handleExceptionResume(cp.core, ex, "OnStop", "Unexpected Error")
            End Try
        End Sub
        '
        '======================================================================================
        '   Log a reported error
        '======================================================================================
        '
        Public Sub handleExceptionResume(cpCore As cpCoreClass, ByVal ex As Exception, ByVal MethodName As String, ByVal LogCopy As String)
            cpCore.appendLogWithLegacyRow("(service)", LogCopy, "server", "ccService2", MethodName, -1, ex.Source, ex.ToString, True, True, "", "", "")
        End Sub
        '
    End Class
End Namespace
