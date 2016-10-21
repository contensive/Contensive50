Imports Contensive.Core

Public Class Form1

    Private listen As New ipDaemonClass
    Private localVarMessage As String = "localVar Message"
    '
    Public Function ipDaemonCallback(cmd As String, queryString As String, remoteIP As String) As String
        'Label1.Text = "cmd=" & cmd & ",querystring=" & queryString & ", remoteIp=" & remoteIP
        'Label1.Refresh()
        localVarMessage = "last hit " & Now()
        'Label2.Text = localVarMessage
        'Label2.Refresh()
        'Refresh()
        'Threading.Thread.Sleep(1)
        Return localVarMessage
    End Function
    '
    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        '
    End Sub
    '
    Protected Overrides Sub Finalize()
        MyBase.Finalize()
        listen.stopListening()

    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        Label1.Text = "server is running..."
        localVarMessage = "Server was started " & Now()
        Call listen.startListening(Me, 8080)
    End Sub

    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        Label1.Text = "server is stopped."
        listen.stopListening()
    End Sub

    Public Sub New()
        InitializeComponent()
        Label1.Text = "server has not been started yet."
    End Sub
End Class
