
Option Explicit On
Option Strict On

Imports System.Web.SessionState
Imports System.Web.Routing

Public Class Global_asax
    Inherits System.Web.HttpApplication
    '
    Public AppId As Guid = Guid.NewGuid()
    '
    '====================================================================================================
    ''' <summary>
    ''' application load -- build routing
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Trace.WriteLine(getAppDescription("Application_Start"))
            Using cp As New Contensive.Core.CPClass(ConfigurationManager.AppSettings("ContensiveAppName"))
                DefaultSite.configurationClass.appendLog(cp, getAppDescription("Application_Start"))
                If (cp.appOk) Then
                    DefaultSite.configurationClass.RegisterRoutes(cp, RouteTable.Routes)
                End If
            End Using
        Catch ex As Exception
            Trace.WriteLine(getAppDescription("Application_Start ERROR exit"))
        End Try
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
        builder.AppendFormat("Event: {0}", eventName)
        builder.AppendFormat(", Guid: {0}", AppId)
        builder.AppendFormat(", Thread Id: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId)
        builder.AppendFormat(", Appdomain: {0}", AppDomain.CurrentDomain.FriendlyName)
        builder.Append(IIf(System.Threading.Thread.CurrentThread.IsThreadPoolThread, ", Pool Thread", ", No Thread").ToString())
        Return builder.ToString()
    End Function

End Class
