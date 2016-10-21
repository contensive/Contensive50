<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmServer
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip
        Me.ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel
        Me.btSendText = New System.Windows.Forms.Button
        Me.gbTextInput = New System.Windows.Forms.GroupBox
        Me.lbTextInput = New System.Windows.Forms.ListBox
        Me.tbSendText = New System.Windows.Forms.TextBox
        Me.btStartServer = New System.Windows.Forms.Button
        Me.gbSentText = New System.Windows.Forms.GroupBox
        Me.btStartNewClient = New System.Windows.Forms.Button
        Me.StatusStrip1.SuspendLayout()
        Me.gbTextInput.SuspendLayout()
        Me.gbSentText.SuspendLayout()
        Me.SuspendLayout()
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripStatusLabel1})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 301)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(358, 25)
        Me.StatusStrip1.TabIndex = 0
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'ToolStripStatusLabel1
        '
        Me.ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        Me.ToolStripStatusLabel1.Size = New System.Drawing.Size(154, 20)
        Me.ToolStripStatusLabel1.Text = "ToolStripStatusLabel1"
        '
        'btSendText
        '
        Me.btSendText.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btSendText.Location = New System.Drawing.Point(243, 17)
        Me.btSendText.Name = "btSendText"
        Me.btSendText.Size = New System.Drawing.Size(85, 29)
        Me.btSendText.TabIndex = 1
        Me.btSendText.Text = "Send Text"
        Me.btSendText.UseVisualStyleBackColor = True
        '
        'gbTextInput
        '
        Me.gbTextInput.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.gbTextInput.Controls.Add(Me.lbTextInput)
        Me.gbTextInput.Location = New System.Drawing.Point(12, 12)
        Me.gbTextInput.Name = "gbTextInput"
        Me.gbTextInput.Size = New System.Drawing.Size(334, 180)
        Me.gbTextInput.TabIndex = 2
        Me.gbTextInput.TabStop = False
        Me.gbTextInput.Text = "Data in:"
        '
        'lbTextInput
        '
        Me.lbTextInput.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lbTextInput.FormattingEnabled = True
        Me.lbTextInput.HorizontalScrollbar = True
        Me.lbTextInput.ItemHeight = 16
        Me.lbTextInput.Location = New System.Drawing.Point(3, 18)
        Me.lbTextInput.Name = "lbTextInput"
        Me.lbTextInput.Size = New System.Drawing.Size(328, 148)
        Me.lbTextInput.TabIndex = 0
        '
        'tbSendText
        '
        Me.tbSendText.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tbSendText.Location = New System.Drawing.Point(6, 20)
        Me.tbSendText.Multiline = True
        Me.tbSendText.Name = "tbSendText"
        Me.tbSendText.Size = New System.Drawing.Size(231, 29)
        Me.tbSendText.TabIndex = 3
        '
        'btStartServer
        '
        Me.btStartServer.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btStartServer.Location = New System.Drawing.Point(255, 259)
        Me.btStartServer.Name = "btStartServer"
        Me.btStartServer.Size = New System.Drawing.Size(92, 29)
        Me.btStartServer.TabIndex = 1
        Me.btStartServer.Text = "Start Server"
        Me.btStartServer.UseVisualStyleBackColor = True
        '
        'gbSentText
        '
        Me.gbSentText.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.gbSentText.Controls.Add(Me.tbSendText)
        Me.gbSentText.Controls.Add(Me.btSendText)
        Me.gbSentText.Location = New System.Drawing.Point(15, 198)
        Me.gbSentText.Name = "gbSentText"
        Me.gbSentText.Size = New System.Drawing.Size(334, 55)
        Me.gbSentText.TabIndex = 1
        Me.gbSentText.TabStop = False
        Me.gbSentText.Text = "Send Text"
        '
        'btStartNewClient
        '
        Me.btStartNewClient.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btStartNewClient.Location = New System.Drawing.Point(126, 259)
        Me.btStartNewClient.Name = "btStartNewClient"
        Me.btStartNewClient.Size = New System.Drawing.Size(124, 29)
        Me.btStartNewClient.TabIndex = 3
        Me.btStartNewClient.Text = "Start New Client"
        Me.btStartNewClient.UseVisualStyleBackColor = True
        '
        'frmServer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(358, 326)
        Me.Controls.Add(Me.btStartNewClient)
        Me.Controls.Add(Me.gbSentText)
        Me.Controls.Add(Me.btStartServer)
        Me.Controls.Add(Me.gbTextInput)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Name = "frmServer"
        Me.Text = "Test Server"
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.gbTextInput.ResumeLayout(False)
        Me.gbSentText.ResumeLayout(False)
        Me.gbSentText.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents ToolStripStatusLabel1 As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents btSendText As System.Windows.Forms.Button
    Friend WithEvents gbTextInput As System.Windows.Forms.GroupBox
    Friend WithEvents lbTextInput As System.Windows.Forms.ListBox
    Friend WithEvents tbSendText As System.Windows.Forms.TextBox
    Friend WithEvents btStartServer As System.Windows.Forms.Button
    Friend WithEvents gbSentText As System.Windows.Forms.GroupBox
    Friend WithEvents btStartNewClient As System.Windows.Forms.Button

End Class
