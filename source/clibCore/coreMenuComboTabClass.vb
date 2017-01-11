﻿
Option Explicit On
Option Strict On

Imports Contensive.Core.coreCommonModule
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
'
Namespace Contensive.Core
    Public Class coreMenuComboTabClass


        Private Structure TabType
            Dim Caption As String
            Dim Link As String
            Dim AjaxLink As String
            Dim ContainerClass As String
            Dim IsHit As Boolean
            Dim LiveBody As String
        End Structure
        Private Tabs() As TabType
        Private TabsCnt As Integer
        Private TabsSize As Integer
        '
        '
        '
        Public Sub AddEntry(ByVal Caption As String, ByVal Link As String, ByVal AjaxLink As String, ByVal LiveBody As String, ByVal IsHit As Boolean, ByVal ContainerClass As String)
            On Error GoTo ErrorTrap
            '
            If TabsCnt <= TabsSize Then
                TabsSize = TabsSize + 10
                ReDim Preserve Tabs(TabsSize)
            End If
            With Tabs(TabsCnt)
                .Caption = Caption
                .Link = Link
                .AjaxLink = AjaxLink
                .IsHit = IsHit
                If ContainerClass = "" Then
                    .ContainerClass = "ccLiveTab"
                Else
                    .ContainerClass = ContainerClass
                End If
                .LiveBody = encodeEmptyText(LiveBody, "")
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
            Dim TabBodyCollectionWrapStyle As String
            Dim TabStyle As String
            Dim TabHitStyle As String
            Dim TabLinkStyle As String
            Dim TabHitLinkStyle As String
            Dim TabBodyWrapHideStyle As String
            Dim TabBodyWrapShowStyle As String
            Dim TabEndStyle As String
            Dim TabEdgeStyle As String
            '
            Dim TabPtr As Integer
            Dim HitPtr As Integer
            Dim IsLiveTab As Boolean
            Dim TabBody As String
            Dim TabLink As String
            Dim TabAjaxLink As String
            Dim TabID As String
            Dim LiveBodyID As String
            Dim FirstLiveBodyShown As Boolean
            Dim TabClass As String
            Dim iContainerClass As String
            Dim TabBodyStyle As String
            Dim JSClose As String
            Dim TabWrapperID As String
            Dim TabBlank As String
            Dim IDNumber As Integer
            '
            If TabsCnt > 0 Then
                HitPtr = 0
                '
                ' Create TabBar
                '
                TabWrapperID = "TabWrapper" & GetRandomInteger()
                TabBlank = GetTabBlank()
                GetTabs = GetTabs & "<script language=""JavaScript"" src=""/ccLib/clientside/ccDynamicTab.js"" type=""text/javascript""></script>" & vbCrLf
                GetTabs = GetTabs & "<table border=0 cellspacing=0 cellpadding=0 width=""100%""><tr>"
                For TabPtr = 0 To TabsCnt - 1
                    TabStyle = Tabs(TabPtr).ContainerClass
                    TabLink = Tabs(TabPtr).Link
                    TabAjaxLink = Tabs(TabPtr).AjaxLink
                    TabHitStyle = TabStyle & "Hit"
                    TabLinkStyle = TabStyle & "Link"
                    TabHitLinkStyle = TabStyle & "HitLink"
                    TabEndStyle = TabStyle & "End"
                    TabEdgeStyle = TabStyle & "Edge"
                    TabBodyStyle = TabStyle & "Body"
                    TabBodyWrapShowStyle = TabStyle & "BodyWrapShow"
                    TabBodyWrapHideStyle = TabStyle & "BodyWrapHide"
                    TabBodyCollectionWrapStyle = TabStyle & "BodyCollectionWrap"
                    IDNumber = GetRandomInteger()
                    LiveBodyID = "TabContent" & IDNumber
                    TabID = "Tab" & IDNumber
                    '
                    ' This tab is hit
                    '
                    GetTabs = GetTabs & "<td valign=bottom>" & TabBlank & "</td>"
                    GetTabs = vbReplace(GetTabs, "Replace-TabID", TabID)
                    GetTabs = vbReplace(GetTabs, "Replace-StyleEdge", TabEdgeStyle)
                    If TabAjaxLink <> "" Then
                        '
                        ' Ajax tab
                        '
                        GetTabs = vbReplace(GetTabs, "Replace-HotSpot", "<a href=# Class=""" & TabLinkStyle & """ name=tabLink onClick=""if(document.getElementById('unloaded_" & LiveBodyID & "')){GetURLAjax('" & TabAjaxLink & "','','" & LiveBodyID & "','','')};switchLiveTab2('" & LiveBodyID & "', this,'" & TabID & "','" & TabStyle & "','" & TabWrapperID & "');return false;"">" & Tabs(TabPtr).Caption & "</a>")
                        GetTabs = vbReplace(GetTabs, "Replace-StyleHit", TabStyle)
                        TabBody = TabBody & "<div id=""" & LiveBodyID & """ class=""" & TabBodyStyle & """ style=""display:none;text-align:center""><div id=""unloaded_" & LiveBodyID & """  style=""text-align:center;padding-top:50px;""><img src=""/ccLib/images/ajax-loader-big.gif"" border=0 width=32 height=32></div></div>"
                        'TabBody = TabBody & "<div onload=""alert('" & LiveBodyID & " onload');"" id=""" & LiveBodyID & """ class=""" & TabBodyStyle & """ style=""display:none;text-align:center""><div id=""unloaded_" & LiveBodyID & """  style=""text-align:center;padding-top:50px;""><img src=""/ccLib/images/ajax-loader-big.gif"" border=0 width=32 height=32></div></div>"
                    ElseIf TabLink <> "" Then
                        '
                        ' Link back to server tab
                        '
                        GetTabs = vbReplace(GetTabs, "Replace-HotSpot", "<a href=""" & TabLink & """ Class=""" & TabHitLinkStyle & """>" & Tabs(TabPtr).Caption & "</a>")
                        'GetTabs = vbReplace(GetTabs, "Replace-HotSpot", "<a href=# Class=""" & TabLinkStyle & """ name=tabLink onClick=""switchLiveTab2('" & LiveBodyID & "', this,'" & TabID & "','" & TabStyle & "','" & TabWrapperID & "');return false;"">" & Tabs(TabPtr).Caption & "</a>")
                        GetTabs = vbReplace(GetTabs, "Replace-StyleHit", TabStyle)
                    Else
                        '
                        ' Live Tab
                        '
                        If Not FirstLiveBodyShown Then
                            FirstLiveBodyShown = True
                            GetTabs = vbReplace(GetTabs, "Replace-HotSpot", "<a href=# Class=""" & TabHitLinkStyle & """ name=tabLink onClick=""switchLiveTab2('" & LiveBodyID & "', this,'" & TabID & "','" & TabStyle & "','" & TabWrapperID & "');return false;"">" & Tabs(TabPtr).Caption & "</a>")
                            GetTabs = vbReplace(GetTabs, "Replace-StyleHit", TabHitStyle)
                            JSClose = JSClose & "ActiveTabTableID=""" & TabID & """;ActiveContentDivID=""" & LiveBodyID & """;"
                            TabBody = TabBody _
                                & "<div id=""" & LiveBodyID & """ class=""" & TabBodyWrapShowStyle & """>" _
                                & "<div class=""" & TabBodyStyle & """>" _
                                & Tabs(TabPtr).LiveBody _
                                & "</div>" _
                                & "</div>" _
                                & ""
                        Else
                            GetTabs = vbReplace(GetTabs, "Replace-HotSpot", "<a href=# Class=""" & TabLinkStyle & """ name=tabLink onClick=""switchLiveTab2('" & LiveBodyID & "', this,'" & TabID & "','" & TabStyle & "','" & TabWrapperID & "');return false;"">" & Tabs(TabPtr).Caption & "</a>")
                            GetTabs = vbReplace(GetTabs, "Replace-StyleHit", TabStyle)
                            TabBody = TabBody _
                                & "<div id=""" & LiveBodyID & """ class=""" & TabBodyWrapHideStyle & """>" _
                                & "<div class=""" & TabBodyStyle & """>" _
                                & Tabs(TabPtr).LiveBody _
                                & "</div>" _
                                & "</div>" _
                                & ""
                        End If
                    End If
                    HitPtr = TabPtr
                Next
                GetTabs = GetTabs & "<td width=""100%"" class=""" & TabEndStyle & """>&nbsp;</td></tr></table>"
                GetTabs = GetTabs & "<div ID=""" & TabWrapperID & """ class=""" & TabBodyCollectionWrapStyle & """>" & TabBody & "</div>"
                GetTabs = GetTabs & "<script type=text/javascript>" & JSClose & "</script>" & vbCrLf
                TabsCnt = 0
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call Err.Raise(Err.Number, Err.Source, "Error in GetTabs-" & Err.Description)
        End Function
        '
        '
        '
        public Function GetTabBlank() As String
            On Error GoTo ErrorTrap
            '
            Dim TabPtr As Integer
            '
            GetTabBlank = GetTabBlank _
                & "<!--" & vbCrLf & "Tab Replace-TabID" & vbCrLf & "-->" _
                & "<table cellspacing=0 cellPadding=0 border=0 id=Replace-TabID>"
            GetTabBlank = GetTabBlank _
                & vbCrLf & "<tr>" _
                & vbCrLf & "<td id=Replace-TabIDR00 colspan=2 class="""" height=1 width=2><img src=""/ccLib/images/spacer.gif"" width=2 height=1></td>" _
                & vbCrLf & "<td id=Replace-TabIDR01 colspan=1 class=""Replace-StyleEdge"" height=1></td>" _
                & vbCrLf & "<td id=Replace-TabIDR02 colspan=3 class="""" height=1 width=3><img src=""/ccLib/images/spacer.gif"" width=3 height=1></td>" _
                & vbCrLf & "</tr>"
            GetTabBlank = GetTabBlank _
                & vbCrLf & "<tr>" _
                & vbCrLf & "<td id=Replace-TabIDR10 colspan=1 class="""" height=1 width=1></td>" _
                & vbCrLf & "<td id=Replace-TabIDR11 colspan=1 class=""Replace-StyleEdge"" height=1 width=1></td>" _
                & vbCrLf & "<td id=Replace-TabIDR12 colspan=1 class=""Replace-StyleHit"" height=1></td>" _
                & vbCrLf & "<td id=Replace-TabIDR13 colspan=1 class=""Replace-StyleEdge"" height=1 width=1></td>" _
                & vbCrLf & "<td id=Replace-TabIDR14 colspan=2 class="""" height=1 width=2></td>" _
                & vbCrLf & "</tr>"
            GetTabBlank = GetTabBlank _
                & vbCrLf & "<tr>" _
                & vbCrLf & "<td id=Replace-TabIDR20 colspan=1 height=2 class=""Replace-StyleEdge""></td>" _
                & vbCrLf & "<td id=Replace-TabIDR21 colspan=1 height=2 Class=""Replace-StyleHit""></td>" _
                & vbCrLf & "<td id=Replace-TabIDR22 colspan=1 height=2 Class=""Replace-StyleHit""></td>" _
                & vbCrLf & "<td id=Replace-TabIDR23 colspan=1 height=2 Class=""Replace-StyleHit""></td>" _
                & vbCrLf & "<td id=Replace-TabIDR24 colspan=1 height=2 width=1 class=""Replace-StyleEdge""></td>" _
                & vbCrLf & "<td id=Replace-TabIDR25 colspan=1 height=2 width=1 Class=""""></td>" _
                & vbCrLf & "</tr>"
            GetTabBlank = GetTabBlank _
                & vbCrLf & "<tr>" _
                & vbCrLf & "<td id=Replace-TabIDR30 class=""Replace-StyleEdge""></td>" _
                & vbCrLf & "<td id=Replace-TabIDR31 Class=""Replace-StyleHit""></td>" _
                & vbCrLf & "<td id=Replace-TabIDR32 Class=""Replace-StyleHit"" style=""padding-right:10px;padding-left:10px;padding-bottom:2px;"">Replace-HotSpot</td>" _
                & vbCrLf & "<td id=Replace-TabIDR33 Class=""Replace-StyleHit""></td>" _
                & vbCrLf & "<td id=Replace-TabIDR34 class=""Replace-StyleEdge""></td>" _
                & vbCrLf & "<td id=Replace-TabIDR35 class=""""></td>" _
                & vbCrLf & "</tr>"
            GetTabBlank = GetTabBlank _
                & vbCrLf & "<tr>" _
                & vbCrLf & "<td id=Replace-TabIDR40 class=""Replace-StyleEdge""></td>" _
                & vbCrLf & "<td id=Replace-TabIDR41 Class=""Replace-StyleHit""></td>" _
                & vbCrLf & "<td id=Replace-TabIDR42 Class=""Replace-StyleHit""></td>" _
                & vbCrLf & "<td id=Replace-TabIDR43 Class=""Replace-StyleHit""></td>" _
                & vbCrLf & "<td id=Replace-TabIDR44 class=""Replace-StyleEdge""></td>" _
                & vbCrLf & "<td id=Replace-TabIDR45 class="""" ></td>" _
                & vbCrLf & "</tr>" _
                & vbCrLf & "</table>"
            '
            Exit Function
            '
ErrorTrap:
            Call Err.Raise(Err.Number, Err.Source, "Error in GetTabBlank-" & Err.Description)
        End Function
    End Class
End Namespace
