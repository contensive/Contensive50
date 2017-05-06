
Option Explicit On
Option Strict On

Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

'
Namespace Contensive.Core
    Public Class coreUploadClass
        Friend Property binaryHeader As Byte()
        Private BinaryHeaderLocal As Object
        Private ItemStorageCollection As Dictionary(Of String, coreUploadItemStorageClass)
        Private ItemNames() As String
        Private ItemCount As Integer
        '
        '
        '
        Public ReadOnly Property Form(ByVal Key As String) As coreUploadItemStorageClass
            Get
                Dim returnForm As coreUploadItemStorageClass = Nothing
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
            On Error GoTo ErrorTrap
            '
            Dim ItemPointer As Integer
            Dim UcaseKey As String
            Dim ErrMessage As String
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
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("FieldExists", Err.Number, Err.Source, Err.Description)
        End Function


        Public Function Key(ByVal Index As Integer) As String
            On Error GoTo ErrorTrap
            '
            If Index < ItemCount Then
                Key = ItemNames(Index)
            End If
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("FieldExists", Err.Number, Err.Source, Err.Description)
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' handle legacy errors in this class
        ''' </summary>
        ''' <param name="MethodName"></param>
        ''' <param name="ErrNumber"></param>
        ''' <param name="ErrSource"></param>
        ''' <param name="ErrDescription"></param>
        ''' <remarks></remarks>
        Private Sub handleLegacyClassError(ByVal MethodName As String, ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String)
            '
            On Error GoTo 0
            Call Err.Raise(ErrNumber, ErrSource, "App.EXEName" & ".Upload." & MethodName & " encountered and error: " & ErrDescription)
            '
        End Sub
    End Class
End Namespace
