VERSION 5.00
Begin VB.Form Form1 
   Caption         =   "Form1"
   ClientHeight    =   3135
   ClientLeft      =   60
   ClientTop       =   405
   ClientWidth     =   4680
   LinkTopic       =   "Form1"
   ScaleHeight     =   3135
   ScaleWidth      =   4680
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton Command1 
      BackColor       =   &H0000C000&
      Caption         =   "Command1"
      Height          =   855
      Left            =   960
      TabIndex        =   0
      Top             =   720
      Width           =   2295
   End
End
Attribute VB_Name = "Form1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Private Sub Command1_Click()
    '
    '
    '
    Me.BackColor = &HFF&
    Dim cp As CPClass
    Dim returnResult As String
    '
    Set cp = New CPClass
    returnResult = ""
    If cp.init("test82", returnResult) Then
        '
    End If
    Call cp.Dispose
    Me.BackColor = &HC000&
End Sub
