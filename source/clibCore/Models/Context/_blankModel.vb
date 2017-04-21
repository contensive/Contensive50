
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Contensive.Core.Models.Entity
    '
    '====================================================================================================
    ' simple entity model pattern
    '   constructor loads object, not a factory pattern
    '   new() - to allow deserialization (so all methods must pass in cp)
    '   new( cp, id ) - to loads instance properties
    '   saveObject( cp ) - saves instance properties
    '
    Public Class _blankModel
        '
        ' -- instance properties
        Public id As Integer
        Public name As String
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
        Public Sub New(cp As CPBaseClass, recordId As Integer)
            Try
                If recordId <> 0 Then
                    Dim cs As CPCSBaseClass = cp.CSNew()
                    If Not cs.Open(cnBlank, "id=" & recordId) Then
                        Call cs.Close()
                        Throw New ApplicationException("Failed to open record [" & recordId & "]")
                    Else
                        id = recordId
                        name = cs.GetText("name")
                        Call cs.Close()
                    End If
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Function saveObject(cp As CPBaseClass) As Integer
            Try
                Dim cs As CPCSBaseClass = cp.CSNew()
                If (id > 0) Then
                    If Not cs.Open(cnBlank, "id=" & id) Then
                        id = 0
                        cs.Close()
                        Throw New ApplicationException("Unable to open record [" & id & "]")
                    End If
                Else
                    If Not cs.Insert(cnBlank) Then
                        cs.Close()
                        id = 0
                        Throw New ApplicationException("Unable to insert record")
                    End If
                End If
                If cs.OK() Then
                    id = cs.GetInteger("id")
                    Call cs.SetField("name", name)
                End If
                Call cs.Close()
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
            Return id
        End Function
    End Class
End Namespace
