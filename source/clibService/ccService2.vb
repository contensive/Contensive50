
'Imports Contensive.Core
'Imports Contensive.Core

Namespace Contensive.Core
    Public Class ccService2
        Dim taskScheduler As coreTaskSchedulerServiceClass = Nothing
        Dim taskRunner As coreTaskRunnerServiceClass = Nothing
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
                cp.core.log_appendLog("ccService2.OnStart enter")
                '
                If (True) Then
                    '
                    ' this server is the scheduler
                    '
                    cp.core.log_appendLog("ccService2.OnStart, start taskScheduler")
                    taskScheduler = New coreTaskSchedulerServiceClass()
                    Call taskScheduler.StartService(True, False)
                End If
                If (True) Then
                    '
                    ' this server is a runner
                    '
                    cp.core.log_appendLog("ccService2.OnStart, start taskRunner")
                    taskRunner = New coreTaskRunnerServiceClass()
                    Call taskRunner.StartService()
                End If
                cp.core.log_appendLog("ccService2.OnStart exit")
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
                cp.core.log_appendLog("ccService2.OnStop enter")
                '
                If (Not taskScheduler Is Nothing) Then
                    '
                    ' stop taskscheduler
                    '
                    cp.core.log_appendLog("ccService2.OnStop, stop taskScheduler")
                    Call taskScheduler.stopService()
                    Call taskScheduler.Dispose()
                End If
                If (Not taskRunner Is Nothing) Then
                    '
                    ' stop taskrunner
                    '
                    cp.core.log_appendLog("ccService2.OnStop, stop taskRunner")
                    Call taskRunner.stopService()
                    Call taskRunner.Dispose()
                End If
                cp.core.log_appendLog("ccService2.OnStop exit")
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
