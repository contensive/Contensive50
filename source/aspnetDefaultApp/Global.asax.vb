Imports System.Web.SessionState

Public Class Global_asax
    Inherits System.Web.HttpApplication
    '
    Public AppId As Guid = Guid.NewGuid()
    '
    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the application is started
        Trace.WriteLine(getAppDescription("Application_Start"))
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the session is started
        Trace.WriteLine(getAppDescription("Session_Start"))
    End Sub

    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires at the beginning of each request
        Trace.WriteLine(getAppDescription("Application_BeginRequest"))
    End Sub

    Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires upon attempting to authenticate the use
        Trace.WriteLine(getAppDescription("Application_AuthenticateRequest"))
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when an error occurs
        Trace.WriteLine(getAppDescription("Application_Error"))
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the session ends
        Trace.WriteLine(getAppDescription("Session_End"))
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the application ends
        Trace.WriteLine(getAppDescription("Application_End"))
    End Sub
    '
    '
    Private Function getAppDescription(eventName As String) As String
        Dim builder As New StringBuilder
        '
        builder.AppendFormat("-------------------------------------------{0}", Environment.NewLine)
        builder.AppendFormat("Event: {0}{1}", eventName, Environment.NewLine)
        builder.AppendFormat("Guid: {0}{1}", AppId, Environment.NewLine)
        builder.AppendFormat("Thread Id: {0}{1}",
              System.Threading.Thread.CurrentThread.ManagedThreadId, Environment.NewLine)
        builder.AppendFormat("Appdomain: {0}{1}",
              AppDomain.CurrentDomain.FriendlyName, Environment.NewLine)
        builder.Append(IIf(System.Threading.Thread.CurrentThread.IsThreadPoolThread, "Pool Thread", "No Thread").ToString() & Environment.NewLine)
        Return builder.ToString()
    End Function

End Class
