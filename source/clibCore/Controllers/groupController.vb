
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
                dt = cpCore.db.executeQuery("SELECT ID FROM CCGROUPS WHERE NAME=" & sqlGroupName & "")
                If dt.Rows.Count > 0 Then
                    returnGroupId = genericController.EncodeInteger(dt.Rows(0).Item("ID"))
                Else
                    cid = cpCore.metaData.getContentId("groups")
                    createkey = genericController.GetRandomInteger()
                    sql = "insert into ccgroups (contentcontrolid,active,createkey,name,caption) values (" & cid & ",1," & createkey & "," & sqlGroupName & "," & sqlGroupName & ")"
                    Call cpCore.db.executeQuery(sql)
                    '
                    sql = "select top 1 id from ccgroups where createkey=" & createkey & " order by id desc"
                    dt = cpCore.db.executeQuery(sql)
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
                            userid = cpCore.authContext.user.id
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
                            userid = cpCore.authContext.user.id
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
        '=============================================================================
        ' main_Get the GroupID from iGroupName
        '=============================================================================
        '
        Public Shared Function group_GetGroupID(cpcore As coreClass, ByVal GroupName As String) As Integer
            Dim dt As DataTable
            Dim MethodName As String
            Dim iGroupName As String
            '
            iGroupName = genericController.encodeText(GroupName)
            '
            MethodName = "main_GetGroupID"
            '
            group_GetGroupID = 0
            If (iGroupName <> "") Then
                '
                ' ----- main_Get the Group ID
                '
                dt = cpcore.db.executeQuery("select top 1 id from ccGroups where name=" & cpcore.db.encodeSQLText(iGroupName))
                If dt.Rows.Count > 0 Then
                    group_GetGroupID = genericController.EncodeInteger(dt.Rows(0).Item(0))
                End If
            End If
        End Function
        '
        '=============================================================================
        ' main_Get the GroupName from iGroupID
        '=============================================================================
        '
        Public Shared Function group_GetGroupName(cpcore As coreClass, GroupID As Integer) As String
            '
            Dim CS As Integer
            Dim MethodName As String
            Dim iGroupID As Integer
            '
            iGroupID = genericController.EncodeInteger(GroupID)
            '
            MethodName = "main_GetGroupByID"
            '
            group_GetGroupName = ""
            If (iGroupID > 0) Then
                '
                ' ----- main_Get the Group name
                '
                CS = cpcore.db.cs_open2("Groups", iGroupID)
                If cpcore.db.cs_ok(CS) Then
                    group_GetGroupName = genericController.encodeText(cpcore.db.cs_getValue(CS, "Name"))
                End If
                Call cpcore.db.cs_Close(CS)
            End If
        End Function
        '
        '=============================================================================
        ' Add a new group, return its GroupID
        '=============================================================================
        '
        Public Shared Function group_Add(cpcore As coreClass, ByVal GroupName As String, Optional ByVal GroupCaption As String = "") As Integer
            Dim CS As Integer
            Dim MethodName As String
            Dim iGroupName As String
            Dim iGroupCaption As String
            '
            MethodName = "main_AddGroup"
            '
            iGroupName = genericController.encodeText(GroupName)
            iGroupCaption = genericController.encodeEmptyText(GroupCaption, iGroupName)
            '
            group_Add = -1
            Dim dt As DataTable
            dt = cpcore.db.executeQuery("SELECT ID FROM ccgroups WHERE NAME=" & cpcore.db.encodeSQLText(iGroupName))
            If dt.Rows.Count > 0 Then
                group_Add = genericController.EncodeInteger(dt.Rows(0).Item(0))
            Else
                CS = cpcore.db.cs_insertRecord("Groups", SystemMemberID)
                If cpcore.db.cs_ok(CS) Then
                    group_Add = genericController.EncodeInteger(cpcore.db.cs_getValue(CS, "ID"))
                    Call cpcore.db.cs_set(CS, "name", iGroupName)
                    Call cpcore.db.cs_set(CS, "caption", iGroupCaption)
                    Call cpcore.db.cs_set(CS, "active", True)
                End If
                Call cpcore.db.cs_Close(CS)
            End If
        End Function

        '
        '=============================================================================
        ' Add a new group, return its GroupID
        '=============================================================================
        '
        Public Shared Sub group_DeleteGroup(cpcore As coreClass, ByVal GroupName As String)
            Call cpcore.db.deleteContentRecords("Groups", "name=" & cpcore.db.encodeSQLText(GroupName))
        End Sub
        '
        '=============================================================================
        ' Add a member to a group
        '=============================================================================
        '
        Public Shared Sub group_AddGroupMember(cpcore As coreClass, ByVal GroupName As String, Optional ByVal NewMemberID As Integer = SystemMemberID, Optional ByVal DateExpires As Date = Nothing)
            '
            Dim CS As Integer
            Dim GroupID As Integer
            Dim MethodName As String
            Dim iGroupName As String
            Dim iDateExpires As Date
            '
            MethodName = "main_AddGroupMember"
            '
            iGroupName = genericController.encodeText(GroupName)
            iDateExpires = DateExpires 'encodeMissingDate(DateExpires, Date.MinValue)
            '
            If iGroupName <> "" Then
                GroupID = group_GetGroupID(cpcore, iGroupName)
                If (GroupID < 1) Then
                    GroupID = group_Add(cpcore, GroupName, GroupName)
                End If
                If (GroupID < 1) Then
                    Throw (New ApplicationException("main_AddGroupMember could not find or add Group [" & GroupName & "]")) ' handleLegacyError14(MethodName, "")
                Else
                    CS = cpcore.db.cs_open("Member Rules", "(MemberID=" & cpcore.db.encodeSQLNumber(NewMemberID) & ")and(GroupID=" & cpcore.db.encodeSQLNumber(GroupID) & ")", , False)
                    If Not cpcore.db.cs_ok(CS) Then
                        Call cpcore.db.cs_Close(CS)
                        CS = cpcore.db.cs_insertRecord("Member Rules")
                    End If
                    If Not cpcore.db.cs_ok(CS) Then
                        Throw (New ApplicationException("main_AddGroupMember could not add this member to the Group [" & GroupName & "]")) ' handleLegacyError14(MethodName, "")
                    Else
                        Call cpcore.db.cs_set(CS, "active", True)
                        Call cpcore.db.cs_set(CS, "memberid", NewMemberID)
                        Call cpcore.db.cs_set(CS, "groupid", GroupID)
                        If iDateExpires <> Date.MinValue Then
                            Call cpcore.db.cs_set(CS, "DateExpires", iDateExpires)
                        Else
                            Call cpcore.db.cs_set(CS, "DateExpires", "")
                        End If
                    End If
                    Call cpcore.db.cs_Close(CS)
                End If
            End If
        End Sub
        '
        '=============================================================================
        ' Delete a member from a group
        '=============================================================================
        '
        Public Shared Sub group_DeleteGroupMember(cpcore As coreClass, ByVal GroupName As String, Optional ByVal NewMemberID As Integer = SystemMemberID)
            '
            Dim GroupID As Integer
            Dim MethodName As String
            Dim iGroupName As String
            '
            iGroupName = genericController.encodeText(GroupName)
            '
            MethodName = "main_DeleteGroupMember"
            '
            If iGroupName <> "" Then
                GroupID = group_GetGroupID(cpcore, iGroupName)
                If (GroupID < 1) Then
                ElseIf (NewMemberID < 1) Then
                    Throw (New ApplicationException("Member ID is invalid")) ' handleLegacyError14(MethodName, "")
                Else
                    Call cpcore.db.deleteContentRecords("Member Rules", "(MemberID=" & cpcore.db.encodeSQLNumber(NewMemberID) & ")AND(groupid=" & cpcore.db.encodeSQLNumber(GroupID) & ")")
                End If
            End If
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