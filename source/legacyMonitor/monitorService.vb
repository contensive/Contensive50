
Imports Contensive.Core

Namespace Contensive.Monitor
    Public Class monitorService
        '
        Private serverStatus As statusServerClass
        Private siteCheck As siteCheckClass
        Private cp As CPClass

        Protected Overrides Sub OnStart(ByVal args() As String)
            ' Add code here to start your service. This method should set things
            ' in motion so your service can do its work.
            '
            ' store auth token in a config file
            '
            cp = New CPClass("cluster-mode-not-implemented-yet")
            serverStatus = New statusServerClass(cp.core)
            Call serverStatus.startListening()
            siteCheck = New siteCheckClass(cp.core)
            Call siteCheck.StartMonitoring()
        End Sub

        Protected Overrides Sub OnStop()
            ' Add code here to perform any tear-down necessary to stop your service.
            Call serverStatus.stopListening()
            Call siteCheck.StopMonitoring()
            siteCheck = Nothing
            serverStatus = Nothing
            cp.Dispose()
            cp = Nothing
        End Sub

    End Class
End Namespace
