
Option Explicit On
Option Strict On
'
Namespace Contensive.Core
    Public Class coreMenuFlyoutClass
        '
        '==============================================================================
        '
        '   Creates custom menus
        '   Stores caches of the menus
        '   Stores the menu data, and can generate different kind
        '
        '==============================================================================
        '
        Const MenuStyleFlyoutDown = 4
        Const MenuStyleFlyoutRight = 5
        Const MenuStyleFlyoutUp = 6
        Const MenuStyleFlyoutLeft = 7
        Const MenuStyleHoverDown = 8
        Const MenuStyleHoverRight = 9
        Const MenuStyleHoverUp = 10
        Const MenuStyleHoverLeft = 11
        '
        ' ----- Each menu item has an MenuEntry
        '
        Private Structure MenuEntryType
            Dim Caption As String           ' What is displayed for this entry (does not need to be unique)
            Dim CaptionImage As String      ' If present, this is an image in front of the caption
            Dim Name As String              ' Unique name for this entry
            Dim ParentName As String        ' Unique name of the parent entry
            Dim Link As String              ' URL
            Dim Image As String             ' Image
            Dim ImageOver As String         ' Image Over
            Dim NewWindow As Boolean        ' True opens link in a new window
        End Structure
        '
        ' ----- Local storage
        '
        Private iMenuFilePath As String
        '
        ' ----- Menu Entry storage
        '
        Private iEntryCount As Integer          ' Count of Menus in the object
        Private iEntrySize As Integer
        Private iEntry() As MenuEntryType
        '
        Private iTreeCount As Integer          ' Count of Tree Menus for this instance
        Private iMenuCloseString As String  ' String returned for closing menus
        '
        Private UsedEntries As String       ' String of EntryNames that have been used (for unique test)
        Private EntryIndexName As coreKeyPtrIndexClass
        'Private EntryIndexID As keyPtrIndex8Class
        '
        ' ----- RollOverFlyout storage
        '
        Private MenuFlyoutNamePrefix As String    ' Random prefix added to element IDs to avoid namespace collision
        Private MenuFlyoutIcon_Local As String      ' string used to mark a button that has a non-hover flyout
        Private cpCore As cpCoreClass
        '
        '==================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(cpCore As cpCoreClass)
            MyBase.New()
            Me.cpCore = cpCore
            '
            EntryIndexName = New coreKeyPtrIndexClass
            Randomize()
            MenuFlyoutNamePrefix = "id" & CStr(Int(9999 * Rnd()))
            MenuFlyoutIcon_Local = "&nbsp;&#187;"
        End Sub
        '
        '===============================================================================
        '   Returns the menu specified, if it is in local storage
        '===============================================================================
        '
        Public Function GetMenu(ByVal MenuName As String, ByVal ClickToOpen As Boolean, ByVal Direction As Integer, Optional ByVal StyleSheetPrefix As String = "") As String
            On Error GoTo ErrorTrap
            '
            Dim MenuSource As String
            Dim MenuPointer As Integer
            Dim MenuStyle As Integer
            '
            ' ----- Search local storage for this MenuName
            '
            If Not ClickToOpen Then
                Select Case Direction
                    Case 1
                        MenuStyle = MenuStyleHoverRight
                    Case 2
                        MenuStyle = MenuStyleHoverUp
                    Case 3
                        MenuStyle = MenuStyleHoverLeft
                    Case Else
                        MenuStyle = MenuStyleHoverDown
                End Select
            Else
                Select Case Direction
                    Case 1
                        MenuStyle = MenuStyleFlyoutRight
                    Case 2
                        MenuStyle = MenuStyleFlyoutUp
                    Case 3
                        MenuStyle = MenuStyleFlyoutLeft
                    Case Else
                        MenuStyle = MenuStyleFlyoutDown
                End Select
            End If
            GetMenu = GetMenuFlyout(MenuName, MenuStyle, StyleSheetPrefix)
            '
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("GetMenu", Err.Number, Err.Source, Err.Description)
        End Function
        '
        '===============================================================================
        '   Create a new Menu Entry
        '===============================================================================
        '
        Public Sub AddEntry(ByVal EntryName As String, Optional ByVal ParentiEntryName As String = "", Optional ByVal ImageLink As String = "", Optional ByVal ImageOverLink As String = "", Optional ByVal Link As String = "", Optional ByVal Caption As String = "", Optional ByVal CaptionImageLink As String = "", Optional ByVal NewWindow As Boolean = False)
            On Error GoTo ErrorTrap
            '
            Dim MenuEntrySize As Integer
            Dim iEntryName As String
            Dim UcaseEntryName As String
            Dim iNewWindow As Boolean
            '
            iEntryName = Replace(encodeEmptyText(EntryName, ""), ",", " ")
            UcaseEntryName = UCase(iEntryName)
            '
            If (iEntryName <> "") And (InStr(1, UsedEntries & ",", "," & UcaseEntryName & ",", vbBinaryCompare) = 0) Then
                UsedEntries = UsedEntries & "," & UcaseEntryName
                If iEntryCount >= iEntrySize Then
                    iEntrySize = iEntrySize + 10
                    ReDim Preserve iEntry(iEntrySize)
                End If
                With iEntry(iEntryCount)
                    .Link = encodeEmptyText(Link, "")
                    .Image = encodeEmptyText(ImageLink, "")
                    If .Image = "" Then
                        '
                        ' No image, must have a caption
                        '
                        .Caption = encodeEmptyText(Caption, iEntryName)
                    Else
                        '
                        ' Image present, caption is extra
                        '
                        .Caption = encodeEmptyText(Caption, "")
                    End If
                    .CaptionImage = encodeEmptyText(CaptionImageLink, "")
                    .Name = UcaseEntryName
                    .ParentName = UCase(encodeEmptyText(ParentiEntryName, ""))
                    .ImageOver = ImageOverLink
                    .NewWindow = NewWindow
                End With
                Call EntryIndexName.setPtr(UcaseEntryName, iEntryCount)
                iEntryCount = iEntryCount + 1
            End If
            '
            Exit Sub
            '
ErrorTrap:
            Call handleLegacyClassError("AddEntry", Err.Number, Err.Source, Err.Description)
        End Sub
        '
        '===============================================================================
        '   Returns javascripts, etc. required after all menus on a page are complete
        '===============================================================================
        '
        Public Function GetMenuClose() As String
            On Error GoTo ErrorTrap
            '
            GetMenuClose = GetMenuClose & iMenuCloseString
            iMenuCloseString = ""
            '
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("GetMenuClose", Err.Number, Err.Source, Err.Description)
        End Function
        '
        '===============================================================================
        '   Returns from the cursor position to the end of line
        '   Moves the Start Position to the next line
        '===============================================================================
        '
        Private Function ReadLine(ByVal StartPosition As Integer, ByVal Source As String) As String
            On Error GoTo ErrorTrap
            '
            Dim EndOfLine As Integer
            '
            ReadLine = ""
            EndOfLine = InStr(StartPosition, Source, vbCrLf)
            If EndOfLine <> 0 Then
                ReadLine = Mid(Source, StartPosition, EndOfLine)
                StartPosition = EndOfLine + 2
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("ReadLine", Err.Number, Err.Source, Err.Description)
        End Function
        '
        '===============================================================================
        '   Returns the menu specified, if it is in local storage
        '       It also creates the menu data in a close string that is returned in GetMenuClose.
        '===============================================================================
        '
        Private Function GetMenuFlyout(ByVal MenuName As String, ByVal MenuStyle As Integer, Optional ByVal StyleSheetPrefix As String = "") As String
            On Error GoTo ErrorTrap
            '
            Dim Link As String
            Dim EntryPointer As Integer
            Dim UcaseMenuName As String
            Dim MenuEntries As String
            Dim target As String
            Dim FlyoutStyle As String
            Dim HotSpotHTML As String
            Dim HotSpotHTMLHover As String
            Dim FlyoutPanel As String
            Dim FlyoutDirection As Integer
            Dim LocalStyleSheetPrefix As String
            Dim FlyoutHover As Boolean
            Dim MouseClickCode As String
            Dim MouseOverCode As String
            Dim MouseOutCode As String
            Dim ImageID As String
            Dim JavaCode As String
            Dim PanelButtonCount As Integer
            Dim IsTextHotSpot As Boolean
            '
            If iEntryCount > 0 Then
                '
                ' ----- Get the menu pointer
                '
                LocalStyleSheetPrefix = encodeEmptyText(StyleSheetPrefix, "ccFlyout")
                If LocalStyleSheetPrefix = "" Then
                    LocalStyleSheetPrefix = "ccFlyout"
                End If
                UcaseMenuName = UCase(MenuName)
                For EntryPointer = 0 To iEntryCount - 1
                    If iEntry(EntryPointer).Name = UcaseMenuName Then
                        Exit For
                    End If
                Next
                If EntryPointer < iEntryCount Then
                    MouseClickCode = ""
                    MouseOverCode = ""
                    MouseOutCode = ""
                    ImageID = "img" & CStr(GetRandomInteger()) & "s"
                    FlyoutStyle = LocalStyleSheetPrefix & "Button"
                    '
                    Select Case MenuStyle
                        Case MenuStyleFlyoutRight, MenuStyleFlyoutUp, MenuStyleFlyoutDown, MenuStyleFlyoutLeft
                            FlyoutHover = False
                        Case Else
                            FlyoutHover = True
                    End Select
                    '
                    With iEntry(EntryPointer)
                        Link = html_EncodeHTML(.Link)
                        If .Image <> "" Then
                            '
                            ' Create hotspot from image
                            '
                            HotSpotHTML = "<img src=""" & .Image & """ border=""0"" alt=""" & .Caption & """ ID=" & ImageID & " Name=" & ImageID & ">"
                            If .ImageOver <> "" Then
                                JavaCode = JavaCode _
                                    & "var " & ImageID & "n=new Image; " _
                                    & ImageID & "n.src='" & .Image & "'; " _
                                    & "var " & ImageID & "o=new Image; " _
                                    & ImageID & "o.src='" & .ImageOver & "'; "
                                MouseOverCode = MouseOverCode & " document." & ImageID & ".src=" & ImageID & "o.src;"
                                MouseOutCode = MouseOutCode & " document." & ImageID & ".src=" & ImageID & "n.src;"
                            End If
                        ElseIf .Caption <> "" Then
                            '
                            ' Create hotspot text
                            '
                            If .CaptionImage <> "" Then
                                HotSpotHTML = "<img alt=""" & .Caption & """ src=""" & .CaptionImage & """ border=""0"">"
                            End If
                            HotSpotHTML = HotSpotHTML & .Caption
                            IsTextHotSpot = True
                        Else
                            '
                            ' Create hotspot from name
                            '
                            HotSpotHTML = .Name
                            IsTextHotSpot = True
                        End If
                    End With
                    '
                    FlyoutPanel = GetMenuFlyoutPanel(UcaseMenuName, "", LocalStyleSheetPrefix, FlyoutHover, PanelButtonCount)
                    '
                    ' do not fix the navigation menus by making an exception with the menu object. It is also used for Record Add tags, which need a flyout of 1.
                    '   make the exception in the menuing code above this.
                    '
                    If PanelButtonCount > 0 Then
                        'If PanelButtonCount = 1 Then
                        '    '
                        '    ' Single panel entry, just put the link on the button
                        '    '
                        '    FlyoutPanel = ""
                        '    MouseOverCode = ""
                        '    MouseOutCode = ""
                        'ElseIf PanelButtonCount > 1 Then
                        'If FlyoutPanel <> "" Then
                        '
                        ' Panel exists, create flyout/hover link
                        '
                        Select Case MenuStyle
                            '
                            ' Set direction flag based on style
                            '
                            Case MenuStyleFlyoutRight, MenuStyleHoverRight
                                FlyoutDirection = 1
                            Case MenuStyleFlyoutUp, MenuStyleHoverUp
                                FlyoutDirection = 2
                            Case MenuStyleFlyoutLeft, MenuStyleHoverLeft
                                FlyoutDirection = 3
                            Case Else
                                FlyoutDirection = 0
                        End Select
                        If FlyoutHover Then
                            MouseOverCode = MouseOverCode & " ccFlyoutHoverMode(1); return ccFlyoutButtonClick(event, '" & MenuFlyoutNamePrefix & "_" & UcaseMenuName & "','" & FlyoutDirection & "','" & LocalStyleSheetPrefix & "','true');"
                            MouseOutCode = MouseOutCode & " ccFlyoutHoverMode(0);"
                        Else
                            If IsTextHotSpot Then
                                HotSpotHTML = HotSpotHTML & MenuFlyoutIcon_Local
                            End If
                            MouseClickCode = MouseClickCode & " return ccFlyoutButtonClick(event, '" & MenuFlyoutNamePrefix & "_" & UcaseMenuName & "','" & FlyoutDirection & "','" & LocalStyleSheetPrefix & "');"
                            MouseOverCode = MouseOverCode & " ccFlyoutButtonHover(event,'" & MenuFlyoutNamePrefix & "_" & UcaseMenuName & "','" & FlyoutDirection & "','false');"
                        End If

                    End If
                    '
                    ' Convert js code to action
                    '
                    If MouseClickCode <> "" Then
                        MouseClickCode = " onClick=""" & MouseClickCode & """ "
                    End If
                    If MouseOverCode <> "" Then
                        MouseOverCode = " onMouseOver=""" & MouseOverCode & """ "
                    End If
                    If MouseOutCode <> "" Then
                        MouseOutCode = " onMouseOut=""" & MouseOutCode & """ "
                    End If
                    '
                    If FlyoutPanel <> "" Then
                        '
                        ' Create a flyout link
                        '
                        GetMenuFlyout = "<a class=""" & FlyoutStyle & """ " & MouseOutCode & " " & MouseOverCode & " " & MouseClickCode & " HREF=""" & Link & """>" & HotSpotHTML & "</a>"
                        iMenuCloseString = iMenuCloseString & FlyoutPanel
                    ElseIf Link <> "" Then
                        '
                        ' Create a linked element
                        '
                        GetMenuFlyout = "<a class=""" & FlyoutStyle & """ " & MouseOutCode & " " & MouseOverCode & " " & MouseClickCode & " HREF=""" & Link & """>" & HotSpotHTML & "</a>"
                    Else
                        '
                        ' no links and no flyouts, create just the caption
                        '
                    End If
                    '
                    ' Add in the inline java code if required
                    '
                    If JavaCode <> "" Then
                        GetMenuFlyout = "" _
                            & "<SCRIPT language=javascript type=text/javascript>" _
                            & JavaCode _
                            & "</script>" _
                            & GetMenuFlyout
                    End If
                End If
            End If
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("GetMenuFlyout", Err.Number, Err.Source, Err.Description)
        End Function
        '
        '===============================================================================
        '   Gets the Menu Branch for the Default Menu
        '===============================================================================
        '
        Private Function GetMenuFlyoutPanel(ByVal PanelName As String, ByVal UsedEntries As String, ByVal StyleSheetPrefix As String, ByVal FlyoutHover As Boolean, ByVal PanelButtonCount As Integer) As String
            On Error GoTo ErrorTrap
            '
            Dim EntryPointer As Integer
            Dim iUsedEntries As String
            Dim SubMenuName As String
            Dim SubMenuCount As Integer
            Dim target As String
            Dim SubMenus As String
            Dim PanelChildren As String
            Dim PanelButtons As String
            Dim PanelButtonStyle As String
            Dim HotSpotHTML As String
            '
            iUsedEntries = UsedEntries
            'EntryPointer = EntryIndexName.GetPointer(PanelName)
            For EntryPointer = 0 To iEntryCount - 1
                With iEntry(EntryPointer)
                    If (.ParentName = PanelName) And (.Caption <> "") Then
                        If Not (InStr(1, iUsedEntries & ",", "," & EntryPointer & ",") < 0) Then
                            PanelButtonCount = PanelButtonCount + 1
                            iUsedEntries = iUsedEntries & "," & EntryPointer
                            PanelButtonStyle = StyleSheetPrefix & "PanelButton"
                            'PanelButtonStyle = "ccFlyoutPanelButton"
                            target = ""
                            If .NewWindow Then
                                target = " target=""_blank"""
                            End If
                            PanelChildren = GetMenuFlyoutPanel(.Name, iUsedEntries, StyleSheetPrefix, FlyoutHover, PanelButtonCount)
                            If .Image <> "" Then
                                HotSpotHTML = "<img src=""" & .Image & """ border=""0"" alt=""" & .Caption & """>"
                            Else
                                HotSpotHTML = .Caption
                            End If
                            'HotSpotHTML = .Caption
                            'If (.StyleSheet <> "") And (.StyleSheet <> "ccFlyoutPanelButton") Then
                            '    HotSpotHTML = "<SPAN class=""" & .StyleSheet & """>" & HotSpotHTML & "</SPAN>"
                            '    End If
                            If PanelChildren = "" Then
                                If .Link = "" Then
                                    '
                                    ' ----- no link and no child panel
                                    '
                                    'PanelButtons = PanelButtons & "<SPAN class=""" & PanelButtonStyle & """>" & HotSpotHTML & "</SPAN>"
                                Else
                                    '
                                    ' ----- Link but no child panel
                                    '
                                    PanelButtons = PanelButtons & "<a class=""" & PanelButtonStyle & """ href=""" & html_EncodeHTML(.Link) & """" & target & " onmouseover=""ccFlyoutHoverMode(1); ccFlyoutPanelButtonHover(event,'','" & StyleSheetPrefix & "');"">" & HotSpotHTML & "</a>"
                                    'PanelButtons = PanelButtons & "<a class=""" & PanelButtonStyle & """ href=""" & encodeHTML(.Link) & """" & Target & ">" & HotSpotHTML & "</a>"
                                End If
                            Else
                                If .Link = "" Then
                                    '
                                    ' ----- Child Panel and no link, block the href so menu "parent" buttons will not be clickable
                                    '
                                    If FlyoutHover Then
                                        PanelButtons = PanelButtons & "<a class=""" & PanelButtonStyle & """ onmouseover=""ccFlyoutHoverMode(1); ccFlyoutPanelButtonHover(event,'" & MenuFlyoutNamePrefix & "_" & .Name & "','" & StyleSheetPrefix & "');"" onmouseout=""ccFlyoutHoverMode(0);"" onclick=""return false;"" href=""#""" & target & ">" & HotSpotHTML & MenuFlyoutIcon_Local & "</a>"
                                        'PanelButtons = PanelButtons & "<a class=""" & PanelButtonStyle & """ onmouseover=""ccFlyoutHoverMode(1); ccFlyoutPanelButtonHover(event,'" & MenuFlyoutNamePrefix & "_" & .Name & "','" & StyleSheetPrefix & "');"" onmouseout=""ccFlyoutHoverMode(0);"" onclick=""return false;"" href=""#""" & Target & ">" & HotSpotHTML & "&nbsp;<font face=""webdings"">4</font></a>"
                                    Else
                                        PanelButtons = PanelButtons & "<a class=""" & PanelButtonStyle & """ onmouseover=""ccFlyoutPanelButtonHover(event,'" & MenuFlyoutNamePrefix & "_" & .Name & "','" & StyleSheetPrefix & "');"" onclick=""return false;"" href=""#""" & target & ">" & HotSpotHTML & MenuFlyoutIcon_Local & "</a>"
                                        'PanelButtons = PanelButtons & "<a class=""" & PanelButtonStyle & """ onmouseover=""ccFlyoutPanelButtonHover(event,'" & MenuFlyoutNamePrefix & "_" & .Name & "','" & StyleSheetPrefix & "');"" onclick=""return false;"" href=""#""" & Target & ">" & HotSpotHTML & "&nbsp;<font face=""webdings"">4</font></a>"
                                    End If
                                    'PanelButtons = PanelButtons & "<a class=""" & PanelButtonStyle & """ onmouseover=""ccFlyoutPanelButtonHover(event,'" & MenuFlyoutNamePrefix & "_" & .Name & "');"" onclick=""return false;"" href=""#""" & Target & ">" & HotSpotHTML & "</a>"
                                    'PanelButtons = PanelButtons & "<a class=""" & PanelButtonStyle & """ onmouseover=""ccFlyoutPanelButtonHover(event,'" & MenuFlyoutNamePrefix & "_" & .Name & "');"" onclick=""return false;"" href=""#""" & Target & "><span style=""font-family: dingbats"">4</SPAN>" & HotSpotHTML & "</a>"
                                Else
                                    '
                                    ' ----- Child Panel and a link
                                    '
                                    If FlyoutHover Then
                                        PanelButtons = PanelButtons & "<a class=""" & PanelButtonStyle & """ onmouseover=""ccFlyoutHoverMode(1); ccFlyoutPanelButtonHover(event,'" & MenuFlyoutNamePrefix & "_" & .Name & "','" & StyleSheetPrefix & "');"" onmouseout=""ccFlyoutHoverMode(0);"" href=""" & html_EncodeHTML(.Link) & """" & target & ">" & HotSpotHTML & MenuFlyoutIcon_Local & "</a>"
                                        'PanelButtons = PanelButtons & "<a class=""" & PanelButtonStyle & """ onmouseover=""ccFlyoutHoverMode(1); ccFlyoutPanelButtonHover(event,'" & MenuFlyoutNamePrefix & "_" & .Name & "','" & StyleSheetPrefix & "');"" onmouseout=""ccFlyoutHoverMode(0);"" href=""" & .Link & """" & Target & ">" & HotSpotHTML & "&nbsp;<font face=""webdings"">4</font></a>"
                                    Else
                                        PanelButtons = PanelButtons & "<a class=""" & PanelButtonStyle & """ onmouseover=""ccFlyoutPanelButtonHover(event,'" & MenuFlyoutNamePrefix & "_" & .Name & "','" & StyleSheetPrefix & "');"" href=""" & html_EncodeHTML(.Link) & """" & target & ">" & HotSpotHTML & MenuFlyoutIcon_Local & "</a>"
                                        'PanelButtons = PanelButtons & "<a class=""" & PanelButtonStyle & """ onmouseover=""ccFlyoutPanelButtonHover(event,'" & MenuFlyoutNamePrefix & "_" & .Name & "','" & StyleSheetPrefix & "');"" href=""" & .Link & """" & Target & ">" & HotSpotHTML & "&nbsp;<font face=""webdings"">4</font></a>"
                                    End If
                                End If
                            End If
                            SubMenus = SubMenus & PanelChildren
                        End If
                    End If
                End With
            Next
            If PanelButtons <> "" Then
                '
                ' ----- If panel buttons are returned, wrap them in a DIV
                '
                If FlyoutHover Then
                    GetMenuFlyoutPanel = "<div style=""position: absolute; left: 0px;visibility:hidden;"" class=""kmaMenu " & StyleSheetPrefix & "Panel"" id=""" & MenuFlyoutNamePrefix & "_" & PanelName & """ onmouseover=""ccFlyoutHoverMode(1); ccFlyoutPanelHover(event,'" & StyleSheetPrefix & "');"" onmouseout=""ccFlyoutHoverMode(0);"">" _
                        & PanelButtons _
                        & SubMenus _
                        & "</div>" _
                        & ""
                Else
                    GetMenuFlyoutPanel = "<div style=""position: absolute; left: 0px;visibility:hidden;"" class=""kmaMenu " & StyleSheetPrefix & "Panel"" id=""" & MenuFlyoutNamePrefix & "_" & PanelName & """ onmouseover=""ccFlyoutPanelHover(event,'" & StyleSheetPrefix & "')"">" _
                        & PanelButtons _
                        & SubMenus _
                        & "</div>" _
                        & ""
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("GetMenuFlyoutPanel", Err.Number, Err.Source, Err.Description)
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Property MenuFlyoutIcon() As String
            Get
                MenuFlyoutIcon = MenuFlyoutIcon_Local
            End Get
            Set(ByVal value As String)
                MenuFlyoutIcon_Local = value
            End Set
        End Property
        '
        '========================================================================
        ''' <summary>
        ''' handle legacy errors in this class
        ''' </summary>
        ''' <param name="MethodName"></param>
        ''' <param name="ErrNumber"></param>
        ''' <param name="ErrSource"></param>
        ''' <param name="ErrDescription"></param>
        ''' <remarks></remarks>
        Private Sub handleLegacyClassError(ByVal MethodName As String, ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String)
            '
            Call cpCore.handleException(New Exception("unexpected error in method [" & MethodName & "], errDescription [" & ErrDescription & "]"))
            '
        End Sub
    End Class
End Namespace
