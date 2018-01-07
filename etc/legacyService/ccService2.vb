
'Imports Contensive.Core
'Imports Contensive.Core

Imports Contensive.Core.Controllers

Namespace Contensive.Core
    Public Class Services
        Dim taskScheduler As taskSchedulerController = Nothing
        Dim taskRunner As taskRunnerController = Nothing
        '
        '====================================================================================================
        ''' <summary>
        ''' start the taskrunner and or taskScheduler
        ''' </summary>
        ''' <param name="args"></param>
        Protected Overrides Sub OnStart(ByVal args() As String)
            Dim cp As New CPClass()
            Try
                '
                logController.appendLog(cp.core, "Services.OnStart enter")
                '
                If (True) Then
                    '
                    ' -- start scheduler
                    logController.appendLog(cp.core, "Services.OnStart, call taskScheduler.startTimerEvents")
                    taskScheduler = New taskSchedulerController()
                    Call taskScheduler.startTimerEvents(True, False)
                End If
                If (True) Then
                    '
                    ' -- start runner
                    logController.appendLog(cp.core, "Services.OnStart, call taskRunner.startTimerEvents")
                    taskRunner = New taskRunnerController()
                    Call taskRunner.startTimerEvents()
                End If
                logController.appendLog(cp.core, "Services.OnStart exit")
            Catch ex As Exception
                Call handleExceptionResume(cp.core, ex, "OnStart", "Unexpected Error")
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' stop the taskrunner and or taskScheduler
        ''' </summary>
        Protected Overrides Sub OnStop()
            Dim cp As New CPClass()
            Try
                '
                logController.appendLog(cp.core, "Services.OnStop enter")
                '
                If (Not taskScheduler Is Nothing) Then
                    '
                    ' stop taskscheduler
                    '
                    logController.appendLog(cp.core, "Services.OnStop, call taskScheduler.stopTimerEvents")
                    Call taskScheduler.stopTimerEvents()
                    Call taskScheduler.Dispose()
                End If
                If (Not taskRunner Is Nothing) Then
                    '
                    ' stop taskrunner
                    '
                    logController.appendLog(cp.core, "Services.OnStop, call taskRunner.stopTimerEvents")
                    Call taskRunner.stopTimerEvents()
                    Call taskRunner.Dispose()
                End If
                logController.appendLog(cp.core, "Services.OnStop exit")
            Catch ex As Exception
                Call handleExceptionResume(cp.core, ex, "OnStop", "Unexpected Error")
            End Try
        End Sub
        '
        '======================================================================================
        '   Log a reported error
        '======================================================================================
        '
        Public Sub handleExceptionResume(cpCore As coreClass, ByVal ex As Exception, ByVal MethodName As String, ByVal LogCopy As String)
            logController.appendLogWithLegacyRow(cpCore, "(service)", LogCopy, "server", "Services", MethodName, -1, ex.Source, ex.ToString, True, True, "", "", "")
        End Sub
        '
    End Class
End Namespace
