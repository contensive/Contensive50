
Option Explicit On
Option Strict On

Imports System.Web.SessionState
Imports System.Web.Routing
Imports Contensive.Processor.Controllers
Imports Contensive

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
            Application("asdf") = ""
            LogController.logRaw("Global.asax, Application_Start [" & ConfigurationClass.getAppName() & "]", BaseClasses.CPLogBaseClass.LogLevel.Trace)
            Using cp As New Contensive.Processor.CPClass(ConfigurationClass.getAppName())
                DefaultSite.ConfigurationClass.loadRouteMap(cp)
            End Using
        Catch ex As Exception
            LogController.logRaw("Global.asax, Application_Start exception [" & ConfigurationClass.getAppName() & "]" & getAppDescription("Application_Start ERROR exit") + ", ex [" & ex.ToString() & "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Fatal)
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
        LogController.logRaw("Global.asax, Session_Start [" + e.ToString() + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Trace)
    End Sub
    '
    '====================================================================================================
    ''' <summary>
    ''' Fires at the beginning of each request
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
        LogController.logRaw("Global.asax, Application_BeginRequest [" + e.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Trace)
    End Sub
    '
    '====================================================================================================
    ''' <summary>
    ''' Fires when iis attempts to authenticate the use
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
        LogController.logRaw("Global.asax, Application_AuthenticateRequest [" + e.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Trace)
    End Sub
    '
    '====================================================================================================
    ''' <summary>
    ''' Fires when an error occurs
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        LogController.logRaw("Global.asax, Application_Error, Server.GetLastError().InnerException [" + Server.GetLastError().InnerException.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Error)
    End Sub
    '
    '====================================================================================================
    ''' <summary>
    ''' Fires when the session ends
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        LogController.logRaw("Global.asax, Session_End [" + e.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Trace)
    End Sub
    '
    '====================================================================================================
    ''' <summary>
    ''' Fires when the application ends
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        LogController.logRaw("Global.asax, Application_End [" + e.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Trace)
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
