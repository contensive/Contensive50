
Option Explicit On
Option Strict On

Imports Contensive.Core.Models.Context
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

'
Namespace Contensive.Core.Controllers
    Public Class uploadFileController
        Friend Property binaryHeader As Byte()
        Private BinaryHeaderLocal As Object
        Private ItemStorageCollection As Dictionary(Of String, uploadFileModel)
        Private ItemNames() As String
        Private ItemCount As Integer
        '
        '
        '
        Public ReadOnly Property Form(ByVal Key As String) As uploadFileModel
            Get
                Dim returnForm As uploadFileModel = Nothing
                If Not ItemStorageCollection Is Nothing Then
                    If ItemStorageCollection.Count > 0 Then
                        If FieldExists(Key) Then
                            returnForm = ItemStorageCollection(Key)
                        Else
                            returnForm = ItemStorageCollection("EMPTY")
                        End If
                    End If
                End If
                Return returnForm
            End Get
        End Property
        '
        ' Get the Count of Form collection
        '
        Public ReadOnly Property Count() As Integer
            Get
                If ItemStorageCollection Is Nothing Then
                    Count = 0
                Else
                    Count = ItemCount
                    'Count = ItemStorageCollection.Count
                End If

            End Get
        End Property
        '
        ' Test if a key exists in the form collection
        '
        Public Function FieldExists(ByVal Key As String) As Boolean
            Dim result As Boolean = False
            Try
                Dim ItemPointer As Integer
                Dim UcaseKey As String
                '
                FieldExists = False
                If (ItemCount > 0) And Not IsNull(Key) Then
                    UcaseKey = genericController.vbUCase(Key)
                    For ItemPointer = 0 To ItemCount - 1
                        If ItemNames(ItemPointer) = UcaseKey Then
                            FieldExists = True
                            Exit For
                        End If
                    Next
                End If
            Catch ex As Exception
                Throw
            End Try
        End Function


        Public Function Key(ByVal Index As Integer) As String
            Dim result As String = ""
            Try
                If Index < ItemCount Then
                    Key = ItemNames(Index)
                End If
            Catch ex As Exception
                Throw
            End Try
            Return result
        End Function
    End Class
End Namespace
