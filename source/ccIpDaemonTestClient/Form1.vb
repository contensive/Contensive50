
'Imports Contensive.Core
Imports Contensive.Core

Public Class Form1
    '
    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        Try
            Dim cp As New CPClass()
            Dim link As String
            Dim http As New http4Class(cp)
            '
            link = "http://127.0.0.1:8080/testCmd?copy=helloWorld"
            Label1.Text = http.getURL(link)
        Catch ex As Exception
            '
            '
            '
        End Try
    End Sub

    Private Sub Label1_Click(sender As System.Object, e As System.EventArgs) Handles Label1.Click

    End Sub
End Class
