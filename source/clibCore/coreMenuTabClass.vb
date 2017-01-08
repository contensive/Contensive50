
Option Explicit On
Option Strict On
'
Namespace Contensive.Core
    Public Class coreMenuTabClass
        '
        Private Structure TabType
            Dim Caption As String
            Dim Link As String
            Dim StylePrefix As String
            Dim IsHit As Boolean
            Dim LiveBody As String
        End Structure
        Private Tabs() As TabType
        Private TabsCnt As Integer
        Private TabsSize As Integer
        '
        '
        '
        Public Sub AddEntry(ByVal Caption As String, ByVal Link As String, ByVal IsHit As Boolean, Optional ByVal StylePrefix As String = "")
            On Error GoTo ErrorTrap
            '
            If TabsCnt <= TabsSize Then
                TabsSize = TabsSize + 10
                ReDim Preserve Tabs(TabsSize)
            End If
            With Tabs(TabsCnt)
                .Caption = Caption
                .Link = Link
                .IsHit = IsHit
                If StylePrefix = "" Then
                    .StylePrefix = "ccTab"
                Else
                    .StylePrefix = StylePrefix
                End If
                '.StylePrefix = encodeMissingText(StylePrefix, "ccTab")
                '.LiveBody = encodeMissingText(LiveBody, "")
            End With
            TabsCnt = TabsCnt + 1
            '
            Exit Sub
            '
ErrorTrap:
            Call Err.Raise(Err.Number, Err.Source, "Error in AddTabEntry-" & Err.Description)
        End Sub
        '
        '
        '
        Public Function GetTabs() As String
            On Error GoTo ErrorTrap
            '
            GetTabs = GetTab2s()
            Exit Function
            '
ErrorTrap:
            Call Err.Raise(Err.Number, Err.Source, "Error in GetTabs-" & Err.Description)
        End Function
        '
        '
        '
        Private Function GetTab2s() As String
            On Error GoTo ErrorTrap
            '
            Dim TabPtr As Integer
            Dim HitPtr As Integer
            Dim IsLiveTab As Boolean
            Dim TabBody As String
            Dim TabLink As String
            Dim TabID As String
            Dim FirstLiveBodyShown As Boolean
            Dim TabEdgeStyle As String
            Dim LiveTab As New coreMenuLiveTabClass
            Dim TabBlank As String
            Dim TabCurrent As String
            Dim TabStyle As String
            Dim TabHitStyle As String
            Dim TabLinkStyle As String
            Dim TabHitLinkStyle As String
            Dim TabEndStyle As String
            Dim TabBodyStyle As String
            '
            If TabsCnt > 0 Then
                '
                ' Create TabBar
                '
                HitPtr = 0
                '
                TabBlank = LiveTab.GetTabBlank()
                TabEdgeStyle = "ccTabEdge"
                GetTab2s = GetTab2s & "<table border=0 cellspacing=0 cellpadding=0><tr>"
                For TabPtr = 0 To TabsCnt - 1
                    TabID = "Tab" & CStr(GetRandomInteger())
                    TabStyle = Tabs(TabPtr).StylePrefix
                    TabHitStyle = TabStyle & "Hit"
                    TabLinkStyle = TabStyle & "Link"
                    TabHitLinkStyle = TabStyle & "HitLink"
                    TabEndStyle = TabStyle & "End"
                    TabEdgeStyle = TabStyle & "Edge"
                    TabBodyStyle = TabStyle & "Body"
                    If Tabs(TabPtr).LiveBody = "" Then
                        '
                        ' This tab is linked to a page
                        '
                        TabLink = html_EncodeHTML(Tabs(TabPtr).Link)
                    Else
                        '
                        ' This tab has a visible body
                        '
                        TabLink = html_EncodeHTML(Tabs(TabPtr).Link)
                        If Not FirstLiveBodyShown Then
                            FirstLiveBodyShown = True
                            TabBody = TabBody & "<div style=""visibility: visible; position: absolute; left: 0px;"" class=""" & Tabs(TabPtr).StylePrefix & "Body"" id=""" & TabID & """></div>"
                        Else
                            TabBody = TabBody & "<div style=""visibility: hidden; position: absolute; left: 0px;"" class=""" & Tabs(TabPtr).StylePrefix & "Body"" id=""" & TabID & """></div>"
                        End If
                    End If
                    TabCurrent = TabBlank
                    TabCurrent = vbReplace(TabCurrent, "Replace-TabID", TabID)
                    TabCurrent = vbReplace(TabCurrent, "Replace-StyleEdge", TabEdgeStyle)

                    If Tabs(TabPtr).IsHit And (HitPtr = 0) Then
                        '
                        ' This tab is hit
                        '

                        TabCurrent = vbReplace(TabCurrent, "Replace-HotSpot", "<a href=""" & TabLink & """ Class=""" & TabHitLinkStyle & """>" & Tabs(TabPtr).Caption & "</a>")
                        TabCurrent = vbReplace(TabCurrent, "Replace-StyleHit", TabHitStyle)
                    Else

                        TabCurrent = vbReplace(TabCurrent, "Replace-HotSpot", "<a href=""" & TabLink & """ Class=""" & TabLinkStyle & """>" & Tabs(TabPtr).Caption & "</a>")
                        TabCurrent = vbReplace(TabCurrent, "Replace-StyleHit", TabStyle)
                    End If
                    GetTab2s = GetTab2s & "<td valign=bottom>" & TabCurrent & "</td>"
                Next
                GetTab2s = GetTab2s & "<td class=ccTabEnd>&nbsp;</td></tr>"
                If TabBody <> "" Then
                    GetTab2s = GetTab2s & "<tr><td colspan=6>" & TabBody & "</td></tr>"
                End If
                GetTab2s = GetTab2s & "</table>"
                TabsCnt = 0
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call Err.Raise(Err.Number, Err.Source, "Error in GetTab2s-" & Err.Description)
        End Function



    End Class
End Namespace
