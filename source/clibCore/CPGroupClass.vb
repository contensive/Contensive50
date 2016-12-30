
Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)> _
    <ComClass(CPGroupClass.ClassId, CPGroupClass.InterfaceId, CPGroupClass.EventsId)> _
    Public Class CPGroupClass
        Inherits BaseClasses.CPGroupBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "C0000B6E-5ABF-4C67-9F22-EF52D73FC54B"
        Public Const InterfaceId As String = "DB74B7D9-73BE-40C1-B488-ACC098E8B9C1"
        Public Const EventsId As String = "B9E3C450-CDC4-4590-8BCD-FEDDF7338D4B"
#End Region
        '
        Private cpCore As Contensive.Core.cpCoreClass
        Private cp As CPClass
        Protected disposed As Boolean = False
        '
        ' Constructor
        '
        Public Sub New(ByRef cpParent As CPClass)
            MyBase.New()
            cp = cpParent
            cpCore = cp.core
        End Sub
        '
        ' dispose
        '
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Call appendDebugLog(".dispose, dereference cp, main, csv")
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    cp = Nothing
                    cpCore = Nothing
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        '
        ' Add
        '
        Public Overrides Sub Add(ByVal GroupNameOrGuid As String, Optional ByVal groupCaption As String = "")
            Try
                Call cpCore.group_add2(GroupNameOrGuid, groupCaption)
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected error in cp.group.add()")
            End Try
        End Sub
        '
        ' Add User
        '
        Public Overrides Sub AddUser(ByVal GroupNameIdOrGuid As String, Optional ByVal UserId As Integer = 0, Optional ByVal DateExpires As Date = #12:00:00 AM#)
            Try
                Call cpCore.group_AddUser(GroupNameIdOrGuid, UserId, DateExpires)
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        ' Delete Group
        '
        Public Overrides Sub Delete(ByVal GroupNameIdOrGuid As String)
            Try
                cpCore.group_delete(GroupNameIdOrGuid)
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        ' Get Group ID
        '
        Public Overrides Function GetId(ByVal GroupNameIdOrGuid As String) As Integer
            Dim returnInteger As Integer = 0
            Try
                returnInteger = cpCore.db_GetRecordID("groups", GroupNameIdOrGuid)
            Catch ex As Exception

            End Try
            Return returnInteger
        End Function
        '
        ' Get Group Name
        '
        Public Overrides Function GetName(ByVal GroupNameIdOrGuid As String) As String
            Dim returnText As String = ""
            Try
                If IsNumeric(GroupNameIdOrGuid) Then
                    returnText = cpCore.db_GetRecordName("groups", EncodeInteger(GroupNameIdOrGuid))
                Else
                    Dim sqlCriteria As String = cpCore.db.getNameIdOrGuidSqlCriteria(GroupNameIdOrGuid)
                    Dim cs As CPCSClass = cp.CSNew()
                    If cs.Open("groups", sqlCriteria, , , "name") Then
                        returnText = cs.GetText("name")
                    End If
                    Call cs.Close()
                End If
            Catch ex As Exception

            End Try
            Return returnText
        End Function
        '
        ' Remove User from Group
        '
        Public Overrides Sub RemoveUser(ByVal GroupNameIdOrGuid As String, Optional ByVal removeUserId As Integer = 0) 'Inherits BaseClasses.CPGroupBaseClass.RemoveUser
            Dim GroupID As Integer
            Dim userId As Integer = removeUserId
            '
            GroupID = GetId(GroupNameIdOrGuid)
            If GroupID <> 0 Then
                If userId = 0 Then
                    userId = cp.User.Id
                End If
                Call cp.Content.DeleteRecords("Member Rules", "((memberid=" & removeUserId.ToString & ")and(groupid=" & GroupID.ToString & "))")
            End If
        End Sub
        '
        '
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.group, " & copy & vbCrLf, True)
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