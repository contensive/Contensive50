
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    '
    '====================================================================================================
    ' simple entity model pattern
    '   factory pattern load because if a record is not found, must rturn nothing
    '   new() - to allow deserialization (so all methods must pass in cp)
    '   create( cp, id ) - to loads instance properties
    '   saveObject( cp ) - saves instance properties
    '
    Public Class groupModel
        '
        '-- const
        Public Const cnPrimaryContent As String = "groups"
        '
        ' -- instance properties
        Public id As Integer
        Public name As String
        Public requestKey As String ' SPECIAL EXCEPTION - this is field ccGuid
        '
        ' -- publics not exposed to the UI (test/internal data)
        <JsonIgnore> Public createKey As Integer
        '
        '====================================================================================================
        ''' <summary>
        ''' Create an empty object. needed for deserialization
        ''' </summary>
        Public Sub New()
            '
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Function create(cpCore As coreClass, recordId As Integer) As groupModel
            Dim result As groupModel = Nothing
            Try
                If recordId <> 0 Then
                    result = loadObject2(cpCore, "id=" & recordId.ToString())
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordGuid"></param>
        Public Shared Function create(cpCore As coreClass, recordGuid As String) As groupModel
            Dim result As groupModel = Nothing
            Try
                If Not String.IsNullOrEmpty(recordGuid) Then
                    result = loadObject2(cpCore, "ccGuid=" & cpCore.db.encodeSQLText(recordGuid))
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="sqlCriteria"></param>
        Private Shared Function loadObject2(cpCore As coreClass, sqlCriteria As String) As groupModel
            Dim result As groupModel = Nothing
            Try
                Dim cs As New csController(cpCore)
                If cs.Open(cnPrimaryContent, sqlCriteria) Then
                    result = New groupModel
                    With result
                        .id = cs.GetInteger("id")
                        .name = cs.GetText("name")
                        .requestKey = cs.GetText("ccGuid")
                        .createKey = cs.GetInteger("createKey")
                    End With
                End If
                Call cs.Close()
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Function saveObject(cpCore As coreClass) As Integer
            Try
                Dim cs As New csController(cpCore)
                If (id > 0) Then
                    If Not cs.open(cnPrimaryContent, "id=" & id) Then
                        id = 0
                        cs.Close()
                        Throw New ApplicationException("Unable to open record in content [" & cnPrimaryContent & "], with id [" & id & "]")
                    End If
                Else
                    If Not cs.Insert(cnPrimaryContent) Then
                        cs.Close()
                        id = 0
                        Throw New ApplicationException("Unable to insert record in content [" & cnPrimaryContent & "]")
                    End If
                End If
                If cs.ok() Then
                    id = cs.getInteger("id")
                    Call cs.SetField("name", name)
                    Call cs.SetField("ccGuid", requestKey)
                    Call cs.SetField("createKey", createKey.ToString())
                End If
                Call cs.Close()
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return id
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' delete an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Sub delete(cpCore As coreClass, recordId As Integer)
            Try
                If (recordId > 0) Then
                    cpCore.db.deleteContentRecords(cnPrimaryContent, "id=" & recordId.ToString)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' delete an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Sub delete(cpCore As coreClass, guid As String)
            Try
                If (Not String.IsNullOrEmpty(guid)) Then
                    cpCore.db.deleteContentRecords(cnPrimaryContent, "(ccguid=" & cpCore.db.encodeSQLText(guid) & ")")
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' get a list of objects from this model
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="someCriteria"></param>
        ''' <returns></returns>
        Public Shared Function getObjectList(cpCore As coreClass, someCriteria As Integer) As List(Of groupModel)
            Dim result As New List(Of groupModel)
            Try
                Dim cs As New csController(cpCore)
                If (cs.Open(cnPrimaryContent, "(someCriteria=" & someCriteria & ")", "name", True, "id")) Then
                    Dim instance As groupModel
                    Do
                        instance = groupModel.create(cpCore, cs.GetInteger("id"))
                        If (instance IsNot Nothing) Then
                            result.Add(instance)
                        End If
                        cs.GoNext()
                    Loop While cs.OK()
                End If
                cs.Close()
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
