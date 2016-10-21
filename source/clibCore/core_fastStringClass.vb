
Option Explicit On
Option Strict On


Namespace Contensive.Core
    Public Class fastStringClass
        '
        Private iSize As Integer
        Private Const iChunk = 100
        Private iCount As Integer
        Private Holder() As String
        '
        '
        '
        Public Sub Add(ByVal NewString As String)
            If iCount >= iSize Then
                iSize = iSize + iChunk
                ReDim Preserve Holder(iSize)
            End If
            Holder(iCount) = NewString
            iCount = iCount + 1
        End Sub
        '
        '
        '
        Public ReadOnly Property Text As String
            Get
                Return Join(Holder, "")
            End Get
        End Property
    End Class
End Namespace