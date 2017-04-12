Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)>
    <ComClass(CPUserErrorClass.ClassId, CPUserErrorClass.InterfaceId, CPUserErrorClass.EventsId)>
    Public Class CPUserErrorClass
        Inherits BaseClasses.CPUserErrorBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "C175C292-0130-409E-9621-B618F89F4EEC"
        Public Const InterfaceId As String = "C06DB080-41AE-4F1B-A477-B3CF74F61708"
        Public Const EventsId As String = "B784BFEF-127B-48D5-8C99-B075984227DB"
#End Region
        '
        Private cpCore As Contensive.Core.coreClass
        Protected disposed As Boolean = False
        '
        Public Sub New(ByVal cpCoreObj As Contensive.Core.coreClass)
            MyBase.New()
            cpCore = cpCoreObj
        End Sub
        '
        ' dispose
        '
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Call appendDebugLog(".dispose, dereference main, csv")
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    cpCore = Nothing
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        '
        '
        '
        Public Overrides Sub Add(ByVal Message As String) 'Inherits BaseClasses.CPUserErrorBaseClass.Add
            If True Then
                Call cpCore.error_AddUserError(Message)
            End If
        End Sub

        Public Overrides Function GetList() As String 'Inherits BaseClasses.CPUserErrorBaseClass.GetList
            If True Then
                Return cpCore.error_GetUserError()
            Else
                Return ""
            End If
        End Function

        Public Overrides Function OK() As Boolean 'Inherits BaseClasses.CPUserErrorBaseClass.OK
            If True Then
                Return Not cpCore.error_IsUserError()
            Else
                Return True
            End If
        End Function
        '
        '
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.userError, " & copy & vbCrLf, True)
            ' 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
        End Sub
        '
        ' testpoint
        '
        Private Sub tp(ByVal msg As String)
            'Call appendDebugLog(msg)
        End Sub
#Region " IDisposable Support "
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region
    End Class
End Namespace