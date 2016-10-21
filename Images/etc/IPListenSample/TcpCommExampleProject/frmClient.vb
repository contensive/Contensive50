Public Class frmClient

    Public IsClosing As Boolean = False
    Public _Client As New tcpCommClient(AddressOf UpdateUI)

    Private Function BytesToString(ByVal data() As Byte) As String
        Dim enc As New System.Text.UTF8Encoding()
        BytesToString = enc.GetString(data)
    End Function

    Private Function StrToByteArray(ByVal text As String) As Byte()
        Dim encoding As New System.Text.UTF8Encoding()
        StrToByteArray = encoding.GetBytes(text)
    End Function

    Public Sub UpdateUI(ByVal bytes() As Byte, ByVal dataChannel As Integer)

        If Me.InvokeRequired() Then
            ' InvokeRequired: We're running on the background thread. Invoke the delegate.
            Me.Invoke(_Client.ClientCallbackObject, bytes, dataChannel)
        Else
            ' We're on the main UI thread now.
            Dim dontReport As Boolean = False

            If dataChannel < 251 Then
                Me.ListBox1.Items.Add(BytesToString(bytes))
            ElseIf dataChannel = 255 Then
                Dim msg As String = BytesToString(bytes)
                Dim tmp As String = ""

                ' Display info about the incoming file:
                If msg.Length > 15 Then tmp = msg.Substring(0, 15)
                If tmp = "Receiving file:" Then
                    gbGetFilePregress.Text = "Receiving: " & _Client.GetIncomingFileName
                    dontReport = True
                End If

                ' Display info about the outgoing file:
                If msg.Length > 13 Then tmp = msg.Substring(0, 13)
                If tmp = "Sending file:" Then
                    gbSendFileProgress.Text = "Sending: " & _Client.GetOutgoingFileName
                    dontReport = True
                End If

                ' The file being sent to the client is complete.
                If msg = "->Done" Then
                    gbGetFilePregress.Text = "File->Client: Transfer complete."
                    btGetFile.Text = "Get File"
                    dontReport = True
                End If

                ' The file being sent to the server is complete.
                If msg = "<-Done" Then
                    gbSendFileProgress.Text = "File->Server: Transfer complete."
                    btSendFile.Text = "Send File"
                    dontReport = True
                End If

                ' The file transfer to the client has been aborted.
                If msg = "->Aborted." Then
                    gbGetFilePregress.Text = "File->Client: Transfer aborted."
                    dontReport = True
                End If

                ' The file transfer to the server has been aborted.
                If msg = "<-Aborted." Then
                    gbSendFileProgress.Text = "File->Server: Transfer aborted."
                    dontReport = True
                End If

                ' _Client as finished sending the bytes you put into sendBytes()
                If msg.Length > 4 Then tmp = msg.Substring(0, 4)
                If tmp = "UBS:" Then ' User Bytes Sent on channel:???.
                    btSendText.Enabled = True
                    dontReport = True
                End If

                ' We have an error message. Could be local, or from the server.
                If msg.Length > 4 Then tmp = msg.Substring(0, 5)
                If tmp = "ERR: " Then
                    Dim msgParts() As String
                    msgParts = Split(msg, ": ")
                    MsgBox("" & msgParts(1), MsgBoxStyle.Critical, "Test Tcp Communications App")
                    dontReport = True
                End If

                ' Display all other messages in the status strip.
                If Not dontReport Then Me.ToolStripStatusLabel1.Text = BytesToString(bytes)
            End If
        End If

    End Sub

    Private Sub frmClient_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        IsClosing = True
        _Client.StopRunning()
        While _Client.isClientRunning
            Application.DoEvents()
        End While
    End Sub

    Private Sub frmClient_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        IsClosing = True
        _Client.StopRunning()
        While _Client.isClientRunning
            Application.DoEvents()
        End While
    End Sub

    Private Sub frmClient_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        TextBox2.Text = _Client.GetLocalIpAddress.ToString
        _Client.SetReceivedFilesFolder(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "\ClientReceivedFiles")
        tmrPoll.Start()
    End Sub

    Private Sub connectButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles connectButton.Click
        If Me.connectButton.Text = "Connect" Then
            Dim s As String
            s = Me.TextBox3.Text.Trim
            _Client.Connect(Me.TextBox2.Text.Trim, Convert.ToInt32(s))
            Me.connectButton.Text = "Disconnect"
        Else
            _Client.StopRunning()
            Me.connectButton.Text = "Connect"
        End If
    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btGetFileBrowse.Click
        Dim ofd As New OpenFileDialog
        Dim _path As String
        ofd.ShowDialog()
        _path = ofd.FileName
        tbGetFileReq.Text = _path
    End Sub

    Private Sub btGetFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btGetFile.Click
        If btGetFile.Text = "Get File" Then
            If tbGetFileReq.Text.Trim <> "" Then
                If _Client.isClientRunning Then _Client.GetFile(tbGetFileReq.Text.Trim)
                btGetFile.Text = "Cancel"
            End If
        Else
            _Client.CancelIncomingFileTransfer()
            btGetFile.Text = "Get File"
        End If
    End Sub

    Private Sub tmrPoll_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrPoll.Tick
        ' Update progress bars
        pbIncomingFile.Value = _Client.GetPercentOfFileReceived
        pbOutgoingFile.Value = _Client.GetPercentOfFileSent

        ' Update Mbps
        Me.Text = "Test Client (" & _Client.GetMbps & ")"
    End Sub

    Private Sub btSendFileBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btSendFileBrowse.Click
        Dim ofd As New OpenFileDialog
        ofd.ShowDialog()
        tbSendFile.Text = ofd.FileName
    End Sub

    Private Sub btSendFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btSendFile.Click
        If btSendFile.Text = "Send File" Then
            If tbSendFile.Text.Trim <> "" Then
                If _Client.isClientRunning Then _Client.SendFile(tbSendFile.Text.Trim)
                btSendFile.Text = "Cancel"
            End If
        Else
            _Client.CancelOutgoingFileTransfer()
            btSendFile.Text = "Send File"
        End If
    End Sub

    Private Sub btSendText_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btSendText.Click
        If Me.tbSendText.Text.Trim.Length > 0 Then
            _Client.SendBytes(StrToByteArray(Me.tbSendText.Text.Trim))
            btSendText.Enabled = False
        End If
    End Sub
End Class