
Option Explicit On
Option Strict On

Namespace Contensive.Core.Controllers
    Public Class stringBuilderLegacyController
        '
        Private iSize As Integer
        Private Const iChunk = 100
        Private iCount As Integer
        Private Holder() As String
        '
        '==========================================================================================
        ''' <summary>
        ''' add a string to the stringbuilder
        ''' </summary>
        ''' <param name="NewString"></param>
        Public Sub Add(ByVal NewString As String)
            Try
                If iCount >= iSize Then
                    iSize = iSize + iChunk
                    ReDim Preserve Holder(iSize)
                End If
                Holder(iCount) = NewString
                iCount = iCount + 1
            Catch ex As Exception
                Throw New ApplicationException("Exception in coreFastString.Add()", ex)
            End Try
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' read the string out of the string builder
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Text As String
            Get
                Return Join(Holder, "") & ""
            End Get
        End Property
    End Class
End Namespace