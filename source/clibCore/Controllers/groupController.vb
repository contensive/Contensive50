
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
    Public Class groupController
        Implements IDisposable
        '
        ' ----- constants
        '
        'Private Const invalidationDaysDefault As Double = 365
        '
        ' ----- objects constructed that must be disposed
        '
        'Private cacheClient As Enyim.Caching.MemcachedClient
        '
        ' ----- private instance storage
        '
        'Private remoteCacheDisabled As Boolean

        '
        '====================================================================================================
        ''' <summary>
        ''' Create a group and return its Id. If the group already exists, the groups Id is returned. If the group cannot be added a 0 is returned.
        ''' </summary>
        ''' <param name="groupName"></param>
        ''' <returns></returns>
        Public Shared Function group_add(cpCore As coreClass, ByVal groupName As String) As Integer
            Dim returnGroupId As Integer = 0
            Try
                Dim dt As DataTable
                Dim sql As String
                Dim createkey As Integer
                Dim cid As Integer
                Dim sqlGroupName As String = cpCore.db.encodeSQLText(groupName)
                '
                dt = cpCore.db.executeSql("SELECT ID FROM CCGROUPS WHERE NAME=" & sqlGroupName & "")
                If dt.Rows.Count > 0 Then
                    returnGroupId = genericController.EncodeInteger(dt.Rows(0).Item("ID"))
                Else
                    cid = cpCore.metaData.getContentId("groups")
                    createkey = genericController.GetRandomInteger()
                    sql = "insert into ccgroups (contentcontrolid,active,createkey,name,caption) values (" & cid & ",1," & createkey & "," & sqlGroupName & "," & sqlGroupName & ")"
                    Call cpCore.db.executeSql(sql)
                    '
                    sql = "select top 1 id from ccgroups where createkey=" & createkey & " order by id desc"
                    dt = cpCore.db.executeSql(sql)
                    If dt.Rows.Count > 0 Then
                        returnGroupId = genericController.EncodeInteger(dt.Rows(0).Item(0))
                    End If
                End If
                dt.Dispose()
            Catch ex As Exception
                Throw (ex)
            End Try
            Return returnGroupId
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Create a group and return its Id. If the group already exists, the groups Id is returned. If the group cannot be added a 0 is returned.
        ''' </summary>
        ''' <param name="GroupNameOrGuid"></param>
        ''' <param name="groupCaption"></param>
        ''' <returns></returns>
        Public Shared Function group_add2(cpCore As coreClass, ByVal GroupNameOrGuid As String, Optional ByVal groupCaption As String = "") As Integer
            Dim returnGroupId As Integer = 0
            Try
                '
                Dim cs As New csController(cpCore)
                Dim IsAlreadyThere As Boolean = False
                Dim sqlCriteria As String = cpCore.db.getNameIdOrGuidSqlCriteria(GroupNameOrGuid)
                Dim groupName As String
                Dim groupGuid As String
                '
                If (GroupNameOrGuid = "") Then
                    Throw (New ApplicationException("A group cannot be added with a blank name"))
                Else
                    cs.open("Groups", sqlCriteria, , False, "id")
                    IsAlreadyThere = cs.ok
                    Call cs.Close()
                    If Not IsAlreadyThere Then
                        Call cs.Insert("Groups")
                        If Not cs.ok Then
                            Throw (New ApplicationException("There was an error inserting a new group record"))
                        Else
                            returnGroupId = cs.getInteger("id")
                            If genericController.isGuid(GroupNameOrGuid) Then
                                groupName = "Group " & cs.getInteger("id")
                                groupGuid = GroupNameOrGuid
                            Else
                                groupName = GroupNameOrGuid
                                groupGuid = Guid.NewGuid().ToString()
                            End If
                            If groupCaption = "" Then
                                groupCaption = groupName
                            End If
                            Call cs.setField("name", groupName)
                            Call cs.setField("caption", groupCaption)
                            Call cs.setField("ccGuid", groupGuid)
                            Call cs.setField("active", "1")
                        End If
                        Call cs.Close()
                    End If
                End If
            Catch ex As Exception
                Throw New ApplicationException("Unexpected error in cp.group.add()", ex)
            End Try
            Return returnGroupId
        End Function
        '
        '====================================================================================================
        '
        ' Add User
        '
        Public Shared Sub group_addUser(cpCore As coreClass, ByVal groupId As Integer, Optional ByVal userid As Integer = 0, Optional ByVal dateExpires As Date = #12:00:00 AM#)
            Try
                '
                Dim groupName As String
                '
                If True Then
                    If (groupId < 1) Then
                        Throw (New ApplicationException("Could not find or create the group with id [" & groupId & "]"))
                    Else
                        If userid = 0 Then
                            userid = cpCore.authContext.user.ID
                        End If
                        Using cs As New csController(cpCore)
                            cs.open("Member Rules", "(MemberID=" & userid.ToString & ")and(GroupID=" & groupId.ToString & ")", , False)
                            If Not cs.ok Then
                                Call cs.Close()
                                Call cs.Insert("Member Rules")
                            End If
                            If Not cs.ok Then
                                groupName = cpCore.db.getRecordName("groups", groupId)
                                Throw (New ApplicationException("Could not find or create the Member Rule to add this member [" & userid & "] to the Group [" & groupId & ", " & groupName & "]"))
                            Else
                                Call cs.setField("active", "1")
                                Call cs.setField("memberid", userid.ToString)
                                Call cs.setField("groupid", groupId.ToString)
                                If dateExpires <> #12:00:00 AM# Then
                                    Call cs.setField("DateExpires", dateExpires.ToString)
                                Else
                                    Call cs.setField("DateExpires", "")
                                End If
                            End If
                            Call cs.Close()
                        End Using
                    End If
                End If
            Catch ex As Exception
                Throw (ex)
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Shared Sub group_AddUser(cpCore As coreClass, ByVal groupNameOrGuid As String, Optional ByVal userid As Integer = 0, Optional ByVal dateExpires As Date = #12:00:00 AM#)
            Try
                '
                Dim GroupID As Integer
                '
                If groupNameOrGuid <> "" Then
                    GroupID = cpCore.db.getRecordID("groups", groupNameOrGuid)
                    If (GroupID < 1) Then
                        Call group_add2(cpCore, groupNameOrGuid)
                        GroupID = cpCore.db.getRecordID("groups", groupNameOrGuid)
                    End If
                    If (GroupID < 1) Then
                        Throw (New ApplicationException("Could not find or create the group [" & groupNameOrGuid & "]"))
                    Else
                        If userid = 0 Then
                            userid = cpCore.authContext.user.ID
                        End If
                        Using cs As New csController(cpCore)
                            cs.open("Member Rules", "(MemberID=" & userid.ToString & ")and(GroupID=" & GroupID.ToString & ")", , False)
                            If Not cs.ok Then
                                Call cs.Close()
                                Call cs.Insert("Member Rules")
                            End If
                            If Not cs.ok Then
                                Throw (New ApplicationException("Could not find or create the Member Rule to add this member [" & userid & "] to the Group [" & GroupID & ", " & groupNameOrGuid & "]"))
                            Else
                                Call cs.setField("active", "1")
                                Call cs.setField("memberid", userid.ToString)
                                Call cs.setField("groupid", GroupID.ToString)
                                If dateExpires <> #12:00:00 AM# Then
                                    Call cs.setField("DateExpires", dateExpires.ToString)
                                Else
                                    Call cs.setField("DateExpires", "")
                                End If
                            End If
                            Call cs.Close()
                        End Using
                    End If
                End If
            Catch ex As Exception
                Throw (ex)
            End Try
        End Sub

        '
        '====================================================================================================
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