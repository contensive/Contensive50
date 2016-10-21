Public Class frmServer

    'Public IsClosing As Boolean = False
    Private _Server As New TcpCommServer(AddressOf UpdateUI)

    Private Function BytesToString(ByVal data() As Byte) As String
        Dim enc As New System.Text.UTF8Encoding()
        BytesToString = enc.GetString(data)
    End Function

    Private Function StrToByteArray(ByVal text As String) As Byte()
        Dim encoding As New System.Text.UTF8Encoding()
        StrToByteArray = encoding.GetBytes(text)
    End Function
    '
    ' Update the UI, called from within tcp server
    ' must match callback delegate defined in tcpCommServer
    '
    Public Sub UpdateUI(ByVal bytes() As Byte, ByVal connectionId As Int32, ByVal dataChannel As Integer)

        If Me.InvokeRequired() Then
            ' InvokeRequired: We're running on the background thread. Invoke the delegate.
            Me.Invoke(_Server.ServerCallbackObject, bytes, connectionId, dataChannel)
        Else
            ' We're on the main UI thread now.
            If dataChannel = 1 Then
                Me.lbTextInput.Items.Add("Connection " & connectionId.ToString & ": " & BytesToString(bytes))
            ElseIf dataChannel = 255 Then
                Dim tmp = ""
                Dim msg As String = BytesToString(bytes)
                Dim dontReport As Boolean = False

                ' _Server as finished sending the bytes you put into sendBytes()
                If msg.Length > 3 Then tmp = msg.Substring(0, 3)
                If tmp = "UBS" Then ' User Bytes Sent.
                    Dim parts() As String = Split(msg, "UBS:")
                    msg = "Data sent to connection: " & parts(1)
                End If

                If Not dontReport Then Me.ToolStripStatusLabel1.Text = msg
            End If
        End If

    End Sub

    Private Sub frmServer_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        _Server.stopListeningCloseConnections()
        'IsClosing = True
    End Sub

    Private Sub frmServer_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.ToolStripStatusLabel1.Text = "Idle."
        frmClient.Show()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btStartServer.Click

        If btStartServer.Text = "Start Server" Then
            _Server.startListening(22490)
            btStartServer.Text = "Stop Server"
        Else
            _Server.stopListeningCloseConnections()
            btStartServer.Text = "Start Server"
        End If

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btSendText.Click
        If Me.tbSendText.Text.Trim.Length > 0 Then
            _Server.SendBytes(StrToByteArray(Me.tbSendText.Text.Trim))
        End If
    End Sub

    Private Sub btStartNewClient_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btStartNewClient.Click
        Dim newClient As New frmClient
        newClient.Show()
    End Sub
End Class