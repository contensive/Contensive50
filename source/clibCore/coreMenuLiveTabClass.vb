
Option Explicit On
Option Strict On
'
Namespace Contensive.Core
    Public Class coreMenuLiveTabClass
        '
        Private Structure TabType
            Dim Caption As String
            Dim StylePrefix As String
            Dim LiveBody As String
        End Structure
        Private Tabs() As TabType
        Private TabsCnt As Integer
        Private TabsSize As Integer
        '
        Private comboTab As New coreMenuComboTabClass
        '
        Public Sub AddEntry(ByVal Caption As String, ByVal LiveBody As String, Optional ByVal StylePrefix As String = "")
            Call comboTab.AddEntry(Caption, "", "", LiveBody, False, "ccAdminTab")
        End Sub
        '
        Public Function GetTabs() As String
            GetTabs = comboTab.GetTabs
        End Function
        '
        Public Function GetTabBlank() As String
            GetTabBlank = comboTab.GetTabBlank
        End Function
    End Class
End Namespace
