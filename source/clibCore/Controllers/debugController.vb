
Option Explicit On
Option Strict On
'
Imports System.Text.RegularExpressions
'
Namespace Contensive.Core.Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' static class controller
    ''' </summary>
    Public Class debugController
        Implements IDisposable
        '
        '
        '========================================================================
        '   Test Point
        '       If main_PageTestPointPrinting print a string, value paior
        '========================================================================
        '
        Public Shared Sub debug_testPoint(cpcore As coreClass, Message As String)
            '
            Dim ElapsedTime As Single
            Dim iMessage As String
            '
            If cpcore.testPointPrinting Then
                '
                ' write to stream
                '
                ElapsedTime = CSng(GetTickCount - cpcore.profileStartTickCount) / 1000
                iMessage = genericController.encodeText(Message)
                iMessage = Format((ElapsedTime), "00.000") & " - " & iMessage
                cpcore.testPointMessage = cpcore.testPointMessage & "<nobr>" & iMessage & "</nobr><br >"
                'writeAltBuffer ("<nobr>" & iMessage & "</nobr><br >")
            End If
            If cpcore.siteProperties.allowTestPointLogging Then
                '
                ' write to debug log in virtual files - to read from a test verbose viewer
                '
                iMessage = genericController.encodeText(Message)
                iMessage = genericController.vbReplace(iMessage, vbCrLf, " ")
                iMessage = genericController.vbReplace(iMessage, vbCr, " ")
                iMessage = genericController.vbReplace(iMessage, vbLf, " ")
                iMessage = FormatDateTime(Now, vbShortTime) & vbTab & Format((ElapsedTime), "00.000") & vbTab & cpcore.authContext.visit.id & vbTab & iMessage
                '
                logController.appendLog(cpcore, iMessage, "", "testPoints_" & cpcore.serverConfig.appConfig.name)
            End If
        End Sub        '====================================================================================================
#Region " IDisposable Support "
        '
        ' this class must implement System.IDisposable
        ' never throw an exception in dispose
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        '====================================================================================================
        '
        Protected disposed As Boolean = False
        '
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        '
        Protected Overrides Sub Finalize()
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(False)
            MyBase.Finalize()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' dispose.
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Me.disposed = True
                If disposing Then
                    'If (cacheClient IsNot Nothing) Then
                    '    cacheClient.Dispose()
                    'End If
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region
    End Class
    '
End Namespace