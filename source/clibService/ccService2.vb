﻿
'Imports Contensive.Core
'Imports Contensive.Core

Namespace Contensive.Core
    Public Class ccService2
        Dim taskScheduler As taskSchedulerClass = Nothing
        Dim taskRunner As taskRunnerClass = Nothing
        '
        '====================================================================================================
        ''' <summary>
        ''' start the taskrunner and or taskScheduler
        ''' </summary>
        ''' <param name="args"></param>
        Protected Overrides Sub OnStart(ByVal args() As String)
            Dim cp As New CPClass("cluster-mode-not-implemented-yet")
            Try
                '
                cp.core.appendLog("ccService2.OnStart enter")
                '
                If (True) Then
                    '
                    ' this server is the scheduler
                    '
                    cp.core.appendLog("ccService2.OnStart, start taskScheduler")
                    taskScheduler = New taskSchedulerClass(cp.core)
                    Call taskScheduler.StartServer(True, False)
                End If
                If (True) Then
                    '
                    ' this server is a runner
                    '
                    cp.core.appendLog("ccService2.OnStart, start taskRunner")
                    taskRunner = New taskRunnerClass(cp.core)
                    Call taskRunner.StartServer(True, False)
                End If
                cp.core.appendLog("ccService2.OnStart exit")
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
            Dim cp As New CPClass("cluster-mode-not-implemented-yet")
            Try
                '
                cp.core.appendLog("ccService2.OnStop enter")
                '
                If (Not taskScheduler Is Nothing) Then
                    '
                    ' stop taskscheduler
                    '
                    cp.core.appendLog("ccService2.OnStop, stop taskScheduler")
                    Call taskScheduler.stopServer()
                    Call taskScheduler.Dispose()
                End If
                If (Not taskRunner Is Nothing) Then
                    '
                    ' stop taskrunner
                    '
                    cp.core.appendLog("ccService2.OnStop, stop taskRunner")
                    Call taskRunner.stopServer()
                    Call taskRunner.Dispose()
                End If
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
