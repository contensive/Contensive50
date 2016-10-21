
Imports Contensive.Core.ccCommonModule
'Imports Interop.adodb
Imports System.Xml

Namespace Contensive.Core


    Public Class cmdTrackClass
        Implements IDisposable
        '
        ''==== usage - bas module ========================
        'The idea is to create an instance of the class immediately on app
        'startup and keep it until the app terminates. Typically that would be
        'done in the same bas module as Sub Main.
        'Ex:
        '
        '
        ''since it's in a bas module, the PI instance has app lifetime...
        'Dim PI As clMutexPI
        '
        '
        'Sub Main()
        '        Set PI = New clMutexPI
        '        If Not PI.PrevInst Then
        '                '..do app startup...
        '        End If
        'End Sub
        '
        '
        '-Tom
        'MVP - Visual Basic
        '(please post replies to the newsgroup)
        '
        ' run build > run code analysis on solution to figure this out 
        Private Declare Function ReleaseMutex Lib "kernel32" (ByVal hMutex As Int64) As Int64
        'Private Declare Function ReleaseMutex Lib "kernel32" (ByVal hMutex as integer) as integer
        Private Declare Function WaitForSingleObject Lib "kernel32" (ByVal hHandle As Int64, ByVal dwMilliseconds As Int64) As Int64
        'Private Declare Function WaitForSingleObject Lib "kernel32" (ByVal hHandle as integer, ByVal dwMilliseconds as integer) as integer
        Private Declare Function CreateMutex Lib "kernel32" Alias "CreateMutexA" (ByVal lpMutexAttributes as integer, ByVal bInitialOwner as integer, ByVal lpName As String) as integer
        Private Declare Function CloseHandle Lib "kernel32" (ByVal hObject as integer) as integer
        Private Const WAIT_FAILED = -1&
        Private Const WAIT_OBJECT_0 = 0
        Private Const WAIT_TIMEOUT = &H102&
        Private m_hMutex as integer


        Public Function AllowInstance(ByVal maxInstanceCnt as integer) As Boolean
            '
            '
            ' REWRITE WITH THIS
            '
            ' Get all instances of Notepad running on the local computer. 
            ' This will return an empty array if notepad isn't running. 
            Dim localByName As Process() = Process.GetProcessesByName("notepad")
            '
            '            Get
            Dim lRet as integer, hM as integer
            Dim InstancePtr as integer
            '
            m_hMutex = 0&
            For InstancePtr = 0 To maxInstanceCnt - 1
                hM = CreateMutex(0&, 0&, "ccCmsInstance" & InstancePtr)
                If hM <> 0& Then
                    lRet = WaitForSingleObject(hM, 1&) 'request ownership
                    If (lRet <> WAIT_TIMEOUT) And (lRet <> WAIT_FAILED) Then 'got it
                        m_hMutex = hM
                        Exit For
                    End If
                End If
            Next
            AllowInstance = (m_hMutex <> 0&)

            '           End Get
        End Function

#Region " IDisposable Support "
        '
        ' dispose
        '
        Protected disposed As Boolean = False
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Me.disposed = True
                'Call appendDebugLog(".dispose, call dispose on all created objects and dereference main, csv")
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                End If
                '
                ' cp  creates and destroys cmc
                '
                If m_hMutex <> 0& Then
                    ReleaseMutex(m_hMutex)
                    CloseHandle(m_hMutex)
                End If                '
                GC.Collect()
            End If
        End Sub
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            'appendDebugLog("public dispose")
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Protected Overrides Sub Finalize()
            'appendDebugLog("finalize")
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region



    End Class
End Namespace