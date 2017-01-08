
Option Explicit On
Option Strict On

Imports System.Text

Namespace Contensive.Core
    '
    '====================================================================================================
    ''' <summary>
    ''' classSummary
    ''' </summary>
    Public Class coreEncodingBase64Class
        '
        Private cpCore As coreClass
        '
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
        '
        '====================================================================================================
        '
        Public Shared Function UTF8ToBase64(source As String) As String
            Dim textAsBytes As Byte()
            textAsBytes = Encoding.UTF8.GetBytes(source)
            Return System.Convert.ToBase64String(textAsBytes)
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function base64ToUTF8(source As String) As String
            Dim textAsBytes As Byte()
            textAsBytes = System.Convert.FromBase64String(source)
            Return Encoding.UTF8.GetString(textAsBytes)
        End Function
    End Class
End Namespace
