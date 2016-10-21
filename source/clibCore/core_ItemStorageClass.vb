
Imports Contensive.Core.ccCommonModule
'Imports Contensive.Core.cpCommonUtilsClass
'Imports Interop.adodb

'
' findReplace as integer to as integer
' just the document -- replace out 
' if 'Imports Interop.adodb, replace in ObjectStateEnum.adState...
' findreplace encode to encode
' findreplace ''DoEvents to '''DoEvents
' runProcess becomes runProcess
' Sleep becomes Threading.Thread.Sleep(
' as object to as object
'
Namespace Contensive.Core
    Public Class itemStorageClass

        Private FileNameLocal As String
        Private FileSizeLocal As String
        Private ValueLocal As Object
        Private ContentTypeLocal As Object
        '
        '
        '
        Friend Property Filename As String
            Get
                Filename = FileNameLocal
            End Get
            Set(ByVal value As String)
                FileNameLocal = value
            End Set
        End Property
        '
        '
        '
        Public Property FileSize() As Integer
            Get
                FileSize = FileSizeLocal
            End Get
            Set(ByVal value As Integer)
                FileSizeLocal = value
            End Set
        End Property
        '
        '
        '
        Public Property Value As Byte()
            Get
                Value = ValueLocal
            End Get
            Set(ByVal value As Byte())
                ValueLocal = value
            End Set
        End Property
        '
        '
        '
        Public Property ContentType As String
            Get
                ContentType = ContentTypeLocal
            End Get
            Set(ByVal value As String)
                ContentTypeLocal = value
            End Set
        End Property
        '
        '
        '
        Public ReadOnly Property IsFile() As Boolean
            Get
                IsFile = False
                If FileNameLocal <> "" Then
                    IsFile = True
                End If

            End Get
        End Property
    End Class
End Namespace
