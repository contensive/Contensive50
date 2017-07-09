
Option Explicit On
Option Strict On
'
Namespace Contensive.Core
    Public Class genericLegacyView
        '
        '
        Public Shared Function CloseFormTable(cpCore As coreClass, ByVal ButtonList As String) As String
            If ButtonList <> "" Then
                CloseFormTable = "</td></tr></TABLE>" & cpCore.html.main_GetPanelButtons(ButtonList, "Button") & "</form>"
            Else
                CloseFormTable = "</td></tr></TABLE></form>"
            End If
        End Function
        '
        '
        Public Shared Function OpenFormTable(cpCore As coreClass, ByVal ButtonList As String) As String
            Dim result As String = ""
            Try
                result = cpCore.html.html_GetFormStart()
                If ButtonList <> "" Then
                    result = result & cpCore.html.main_GetPanelButtons(ButtonList, "Button")
                End If
                result = result & "<table border=""0"" cellpadding=""10"" cellspacing=""0"" width=""100%""><tr><TD>"
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function

    End Class
End Namespace
