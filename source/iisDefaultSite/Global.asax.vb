
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
            LogController.forceNLog("Global.asax, Application_Start [" & configurationClass.getAppName() & "]", LogController.logLevel.Trace)
            Using cp As New Contensive.Processor.CPClass(configurationClass.getAppName())
                DefaultSite.configurationClass.loadRouteMap(cp)
            End Using
        Catch ex As Exception
            LogController.forceNLog("Global.asax, Application_Start exception [" & configurationClass.getAppName() & "]" & getAppDescription("Application_Start ERROR exit") + ", ex [" & ex.ToString() & "]", Contensive.Processor.Controllers.LogController.logLevel.Fatal)
        End Try
    End Sub
    '
    '====================================================================================================
    ''' <summary>
    ''' Fires when the session is started
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        LogController.forceNLog("Global.asax, Session_Start [" + e.ToString() + "]", LogController.logLevel.Trace)
    End Sub
    '
    '====================================================================================================
    ''' <summary>
    ''' Fires at the beginning of each request
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
        LogController.forceNLog("Global.asax, Application_BeginRequest [" + e.ToString() + "]", LogController.logLevel.Trace)
    End Sub
    '
    '====================================================================================================
    ''' <summary>
    ''' Fires when iis attempts to authenticate the use
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
        LogController.forceNLog("Global.asax, Application_AuthenticateRequest [" + e.ToString() + "]", LogController.logLevel.Trace)
    End Sub
    '
    '====================================================================================================
    ''' <summary>
    ''' Fires when an error occurs
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        LogController.forceNLog("Global.asax, Application_Error, Server.GetLastError().InnerException [" + Server.GetLastError().InnerException.ToString() + "]", LogController.logLevel.Error)
    End Sub
    '
    '====================================================================================================
    ''' <summary>
    ''' Fires when the session ends
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        LogController.forceNLog("Global.asax, Session_End [" + e.ToString() + "]", LogController.logLevel.Trace)
    End Sub
    '
    '====================================================================================================
    ''' <summary>
    ''' Fires when the application ends
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        LogController.forceNLog("Global.asax, Application_End [" + e.ToString() + "]", LogController.logLevel.Trace)
    End Sub
    '
    '====================================================================================================
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
