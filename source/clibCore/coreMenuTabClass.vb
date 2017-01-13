﻿
Option Explicit On
Option Strict On

Imports Xunit

Namespace Contensive.Core
    '
    '====================================================================================================
    ''' <summary>
    ''' classSummary
    ''' - first routine should be constructor
    ''' - disposable region at end
    ''' - if disposable is not needed add: not IDisposable - not contained classes that need to be disposed
    ''' </summary>
    Public Class coreMenuTabClass
        Implements IDisposable
        '
        ' ----- objects passed in constructor, do not dispose
        '
        Private cpCore As coreClass
        '
        ' ----- objects constructed that must be disposed
        '
        Private localObject As Object
        '
        ' ----- constants
        '
        Private Const localConstant As Integer = 100
        '
        ' ----- shared globals
        '
        '
        ' ----- private globals
        '
        '
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
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
                        TabLink = cpCore.html.html_EncodeHTML(Tabs(TabPtr).Link)
                    Else
                        '
                        ' This tab has a visible body
                        '
                        TabLink = cpCore.html.html_EncodeHTML(Tabs(TabPtr).Link)
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

        '
        '====================================================================================================
        ''' <summary>
        ''' sample function
        ''' </summary>
        ''' <param name="sampleArg"></param>
        ''' <returns></returns>
        Public Function sampleFunction(sampleArg As String) As String
            Dim returnValue As String = ""
            Try
                '
                ' code
                '
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
#Region " IDisposable Support "
        '
        ' this class must implement System.IDisposable
        ' never throw an exception in dispose
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        '====================================================================================================
        '
        Protected disposed As Boolean = False
        '
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        '
        Protected Overrides Sub Finalize()
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(False)
            MyBase.Finalize()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' dispose.
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Me.disposed = True
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    'If Not (AddonObj Is Nothing) Then AddonObj.Dispose()
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region
    End Class
    '
    '====================================================================================================
    ''' <summary>
    ''' unit tests
    ''' </summary>
    Public Class coreMenuTabClass_UnitTests
        '
        <Fact> Public Sub sampleMethod_unit()
            ' arrange
            ' act
            ' assert
            Assert.Equal(True, True)
        End Sub
    End Class
End Namespace
