
Option Explicit On
Option Strict On

Imports System.Web.SessionState
Imports System.Web.Routing
Imports Contensive.Processor.Controllers

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
            logController.forceNLog("Global.asax, Application_Start [" & configurationClass.getAppName() & "]", logController.logLevel.Trace)
            Using cp As New Contensive.Processor.CPClass(configurationClass.getAppName())
                DefaultSite.configurationClass.loadRouteMap(cp)
            End Using
        Catch ex As Exception
            Contensive.Processor.Controllers.logController.forceNLog("Global.asax, Application_Start exception [" & configurationClass.getAppName() & "]" & getAppDescription("Application_Start ERROR exit") + ", ex [" & ex.ToString() & "]", Contensive.Processor.Controllers.logController.logLevel.Fatal)
        End Try
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the session is started
        logController.forceNLog("Global.asax, Session_Start [" + e.ToString() + "]", logController.logLevel.Trace)
    End Sub

    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires at the beginning of each request
        logController.forceNLog("Global.asax, Application_BeginRequest [" + e.ToString() + "]", logController.logLevel.Trace)
    End Sub

    Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires upon attempting to authenticate the use
        logController.forceNLog("Global.asax, Application_AuthenticateRequest [" + e.ToString() + "]", logController.logLevel.Trace)
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when an error occurs
        'logController.forceNLog("Global.asax, Application_Error [" + e.ToString() + "]", logController.logLevel.Error)
        logController.forceNLog("Global.asax, Application_Error, Server.GetLastError().InnerException [" + Server.GetLastError().InnerException.ToString() + "]", logController.logLevel.Error)
        '
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the session ends
        logController.forceNLog("Global.asax, Session_End [" + e.ToString() + "]", logController.logLevel.Trace)
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the application ends
        logController.forceNLog("Global.asax, Application_End [" + e.ToString() + "]", logController.logLevel.Trace)
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
