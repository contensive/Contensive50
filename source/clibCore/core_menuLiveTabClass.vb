
Imports Contensive.Core.ccCommonModule
'Imports Contensive.Core.cpCommonUtilsClass
'Imports Interop.adodb

'
' findReplace as integer to as integer
' just the document -- replace out 
' if 'Imports Interop.adodb, replace in ObjectStateEnum.adState...
' findreplace encode to encode
' findreplace ''DoEvents to '''DoEvents
' runProcess becomes runProcess
' Sleep becomes Threading.Thread.Sleep(
' as object to as object
' convert types to structures (private structure, public structure, End structure)
'
Namespace Contensive.Core
    Public Class menuLiveTabClass
        '
        Private Structure TabType
            Dim Caption As String
            '    Link As String
            Dim StylePrefix As String
            '    IsHit As Boolean
            Dim LiveBody As String
        End Structure
        Private Tabs() As TabType
        Private TabsCnt As Integer
        Private TabsSize As Integer
        '
        Private comboTab As New MenuComboTabClass
        '
        '
        '

        '
        '
        '
        Public Sub AddEntry(ByVal Caption As String, ByVal LiveBody As String, Optional ByVal StylePrefix As String = "")
            Call comboTab.AddEntry(Caption, "", "", LiveBody, False, "ccAdminTab")
            Exit Sub
            '-----------------------------
            '    On Error GoTo ErrorTrap
            '    '
            '    If TabsCnt <= TabsSize Then
            '        TabsSize = TabsSize + 10
            '        ReDim Preserve Tabs(TabsSize)
            '    End If
            '    With Tabs(TabsCnt)
            '        .Caption = Caption
            '        '.Link = Link
            '        '.IsHit = IsHit
            '        If StylePrefix = "" Then
            '            .StylePrefix = "ccLiveTab"
            '        Else
            '            .StylePrefix = StylePrefix
            '        End If
            '        .LiveBody = encodeMissingText(LiveBody, "")
            '    End With
            '    TabsCnt = TabsCnt + 1
            '    '
            '    Exit Sub
            '    '
            'ErrorTrap:
            '    Call Err.Raise(Err.Number, Err.Source, "Error in AddTabEntry-" & Err.Description)
        End Sub
        '
        '
        '
        Public Function GetTabs() As String
            GetTabs = comboTab.GetTabs
            Exit Function
            '    '-----------------------------
            '    On Error GoTo ErrorTrap
            '    '
            '    Dim TabPtr as integer
            '    Dim HitPtr as integer
            '    Dim IsLiveTab As Boolean
            '    Dim TabBody As String
            '    Dim TabLink As String
            '    Dim TabID As String
            '    Dim LiveBodyID As String
            '    Dim FirstLiveBodyShown As Boolean
            '    Dim TabClass As String
            '    Dim iStylePrefix As String
            '    Dim TabStyle As String
            '    Dim TabHitStyle As String
            '    Dim TabLinkStyle As String
            '    Dim TabHitLinkStyle As String
            '    Dim TabBodyStyle As String
            '    Dim TabEndStyle As String
            '    Dim TabEdgeStyle As String
            '    Dim JSClose As String
            '    Dim TabWrapperID As String
            '    Dim TabBlank As String
            '    Dim IDNumber as integer
            '    '
            '    If TabsCnt > 0 Then
            '        '
            '        ' Create TabBar
            '        '
            '        HitPtr = 0
            '        TabWrapperID = "TabWrapper" & GetRandomInteger()
            '        TabBlank = GetTabBlank()
            '        GetTabs = GetTabs & "<script language=""JavaScript"" src=""/ccLib/clientside/ccDynamicTab.js"" type=""text/javascript""></script>" & vbCrLf
            '        GetTabs = GetTabs & "<table border=0 cellspacing=0 cellpadding=0><tr>"
            '        For TabPtr = 0 To TabsCnt - 1
            '            TabStyle = Tabs(TabPtr).StylePrefix
            '            TabHitStyle = TabStyle & "Hit"
            '            TabLinkStyle = TabStyle & "Link"
            '            TabHitLinkStyle = TabStyle & "HitLink"
            '            TabEndStyle = TabStyle & "End"
            '            TabEdgeStyle = TabStyle & "Edge"
            '            TabBodyStyle = TabStyle & "Body"
            '            IDNumber = CStr(GetRandomInteger)
            '            LiveBodyID = "TabContent" & IDNumber
            '            TabID = "Tab" & IDNumber
            '            '
            '            ' This tab is hit
            '            '
            '            GetTabs = GetTabs & "<td valign=bottom>" & TabBlank & "</td>"
            '            GetTabs = Replace(GetTabs, "Replace-TabID", TabID)
            '            GetTabs = Replace(GetTabs, "Replace-StyleEdge", TabEdgeStyle)
            '            If Not FirstLiveBodyShown Then
            '                FirstLiveBodyShown = True
            '                GetTabs = Replace(GetTabs, "Replace-HotSpot", "<a href=# Class=""" & TabHitLinkStyle & """ name=tabLink onClick=""switchLiveTab2('" & LiveBodyID & "', this,'" & TabID & "','" & TabStyle & "','" & TabWrapperID & "');return false;"">" & Tabs(TabPtr).Caption & "</a>")
            '                GetTabs = Replace(GetTabs, "Replace-StyleHit", TabHitStyle)
            '                JSClose = JSClose & "ActiveTabTableID=""" & TabID & """;ActiveContentDivID=""" & LiveBodyID & """;"
            '                TabBody = TabBody & "<div id=""" & LiveBodyID & """ class=""" & TabBodyStyle & """>" & Tabs(TabPtr).LiveBody & "</div>"
            '            Else
            '                GetTabs = Replace(GetTabs, "Replace-HotSpot", "<a href=# Class=""" & TabLinkStyle & """ name=tabLink onClick=""switchLiveTab2('" & LiveBodyID & "', this,'" & TabID & "','" & TabStyle & "','" & TabWrapperID & "');return false;"">" & Tabs(TabPtr).Caption & "</a>")
            '                GetTabs = Replace(GetTabs, "Replace-StyleHit", TabStyle)
            '                'TabBody = TabBody & "<div id=""" & LiveBodyID & """ class=""" & TabBodyStyle & """ style=""position:absolute;top:-5000px;"">" & Tabs(TabPtr).LiveBody & "</div>"
            '                TabBody = TabBody & "<div id=""" & LiveBodyID & """ class=""" & TabBodyStyle & """ style=""display:none;"">" & Tabs(TabPtr).LiveBody & "</div>"
            '            End If
            '            HitPtr = TabPtr
            '        Next
            '        GetTabs = GetTabs & "<td width=""100%"" class=""" & TabEndStyle & """>&nbsp;</td></tr></table>"
            '        GetTabs = GetTabs & "<div ID=""" & TabWrapperID & """>" & TabBody & "</div>"
            '        GetTabs = GetTabs & "<script type=text/javascript>" & JSClose & "</script>" & vbCrLf
            '        TabsCnt = 0
            '    End If
            '    '
            '    Exit Function
            '    '
            'ErrorTrap:
            '    Call Err.Raise(Err.Number, Err.Source, "Error in GetTabs-" & Err.Description)
        End Function
        '
        '
        '
        Friend Function GetTabBlank() As String
            GetTabBlank = comboTab.GetTabBlank
            Exit Function
            '-----------------------------
            '    On Error GoTo ErrorTrap
            '    '
            '    Dim TabPtr as integer
            '    '
            '    GetTabBlank = GetTabBlank _
            '        & "<!--" & vbCrLf & "Tab Replace-TabID" & vbCrLf & "-->" _
            '        & "<table cellspacing=0 cellPadding=0 border=0 id=Replace-TabID>"
            '    GetTabBlank = GetTabBlank _
            '        & vbCrLf & "<tr>" _
            '        & vbCrLf & "<td id=Replace-TabIDR00 colspan=2 class="""" height=1 width=2><img src=""/cclib/images/spacer.gif"" width=2 height=1></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR01 colspan=1 class=""Replace-StyleEdge"" height=1></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR02 colspan=3 class="""" height=1 width=3><img src=""/cclib/images/spacer.gif"" width=3 height=1></td>" _
            '        & vbCrLf & "</tr>"
            '    GetTabBlank = GetTabBlank _
            '        & vbCrLf & "<tr>" _
            '        & vbCrLf & "<td id=Replace-TabIDR10 colspan=1 class="""" height=1 width=1></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR11 colspan=1 class=""Replace-StyleEdge"" height=1 width=1></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR12 colspan=1 class=""Replace-StyleHit"" height=1></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR13 colspan=1 class=""Replace-StyleEdge"" height=1 width=1></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR14 colspan=2 class="""" height=1 width=2></td>" _
            '        & vbCrLf & "</tr>"
            '    GetTabBlank = GetTabBlank _
            '        & vbCrLf & "<tr>" _
            '        & vbCrLf & "<td id=Replace-TabIDR20 colspan=1 height=2 class=""Replace-StyleEdge""></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR21 colspan=1 height=2 Class=""Replace-StyleHit""></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR22 colspan=1 height=2 Class=""Replace-StyleHit""></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR23 colspan=1 height=2 Class=""Replace-StyleHit""></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR24 colspan=1 height=2 width=1 class=""Replace-StyleEdge""></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR25 colspan=1 height=2 width=1 Class=""""></td>" _
            '        & vbCrLf & "</tr>"
            '    GetTabBlank = GetTabBlank _
            '        & vbCrLf & "<tr>" _
            '        & vbCrLf & "<td id=Replace-TabIDR30 class=""Replace-StyleEdge""></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR31 Class=""Replace-StyleHit""></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR32 Class=""Replace-StyleHit"" style=""padding-right:10px;padding-left:10px;padding-bottom:2px;"">Replace-HotSpot</td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR33 Class=""Replace-StyleHit""></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR34 class=""Replace-StyleEdge""></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR35 class=""""></td>" _
            '        & vbCrLf & "</tr>"
            '    GetTabBlank = GetTabBlank _
            '        & vbCrLf & "<tr>" _
            '        & vbCrLf & "<td id=Replace-TabIDR40 class=""Replace-StyleEdge""></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR41 Class=""Replace-StyleHit""></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR42 Class=""Replace-StyleHit""></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR43 Class=""Replace-StyleHit""></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR44 class=""Replace-StyleEdge""></td>" _
            '        & vbCrLf & "<td id=Replace-TabIDR45 class="""" ></td>" _
            '        & vbCrLf & "</tr>" _
            '        & vbCrLf & "</table>"
            ''    GetTabBlank = GetTabBlank _
            ''        & "<!--" & vbCrLf & "Tab Replace-TabID" & vbCrLf & "--><style>. {height:1px} .tw1 {width:1px} .tw2 {width:2px} .tw3 {width:3px}</style>" _
            ''        & "<table cellspacing=0 cellPadding=0 border=0 id=Replace-TabID>"
            ''    GetTabBlank = GetTabBlank _
            ''        & vbCrLf & "<tr>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR00 colspan=2 class="" tw2"" height=1 width=2><img src=""/cclib/images/spacer.gif"" width=2 height=1></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR01 colspan=1 class="" Replace-StyleEdge"" height=1></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR02 colspan=3 class="" tw3"" height=1 width=3><img src=""/cclib/images/spacer.gif"" width=3 height=1></td>" _
            ''        & vbCrLf & "</tr>"
            ''    GetTabBlank = GetTabBlank _
            ''        & vbCrLf & "<tr>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR10 colspan=1 class="" tw1"" height=1 width=1></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR11 colspan=1 class="" tw1 Replace-StyleEdge"" height=1 width=1></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR12 colspan=1 class="" Replace-StyleHit"" height=1></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR13 colspan=1 class="" tw1 Replace-StyleEdge"" height=1 width=1></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR14 colspan=2 class="" tw2"" height=1 width=2></td>" _
            ''        & vbCrLf & "</tr>"
            ''    GetTabBlank = GetTabBlank _
            ''        & vbCrLf & "<tr>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR20 colspan=1 height=2 class=""th2 Replace-StyleEdge""></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR21 colspan=1 height=2 Class=""th2 Replace-StyleHit""></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR22 colspan=1 height=2 Class=""th2 Replace-StyleHit""></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR23 colspan=1 height=2 Class=""th2 Replace-StyleHit""></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR24 colspan=1 height=2 width=1 class=""th2 tw1 Replace-StyleEdge""></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR25 colspan=1 height=2 width=1 Class=""th2 tw1""></td>" _
            ''        & vbCrLf & "</tr>"
            ''    GetTabBlank = GetTabBlank _
            ''        & vbCrLf & "<tr>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR30 class=""th2 Replace-StyleEdge""></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR31 Class=""th2 Replace-StyleHit""></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR32 Class=""th2 Replace-StyleHit"" style=""padding-right:10px;padding-left:10px;padding-bottom:2px;"">Replace-HotSpot</td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR33 Class=""th2 Replace-StyleHit""></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR34 class=""th2 Replace-StyleEdge""></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR35 class=""th2""></td>" _
            ''        & vbCrLf & "</tr>"
            ''    GetTabBlank = GetTabBlank _
            ''        & vbCrLf & "<tr>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR40 class=""th2 Replace-StyleEdge""></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR41 Class=""th2 Replace-StyleHit""></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR42 Class=""th2 Replace-StyleHit""></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR43 Class=""th2 Replace-StyleHit""></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR44 class=""th2 Replace-StyleEdge""></td>" _
            ''        & vbCrLf & "<td id=Replace-TabIDR45 class=""th2"" ></td>" _
            ''        & vbCrLf & "</tr>" _
            ''        & vbCrLf & "</table>"
            '    '
            '    Exit Function
            '    '
            'ErrorTrap:
            '    Call Err.Raise(Err.Number, Err.Source, "Error in GetTabBlank-" & Err.Description)
        End Function
    End Class
End Namespace
