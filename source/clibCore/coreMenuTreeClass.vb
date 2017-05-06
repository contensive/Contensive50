
Option Explicit On
Option Strict On

Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Core
    Public Class coreMenuTreeClass
        '
        '==============================================================================
        '
        '   Creates custom menus
        '   Stores caches of the menus
        '   Stores the menu data, and can generate different kind
        '
        '==============================================================================
        '
        Const MenuStyleRollOverFlyout = 1
        ' Const MenuStyleDropDown = 1
        Const MenuStyleTree = 2
        Const MenuStyleTreeList = 3
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
            Dim Name As String              ' Unique name for this entry
            Dim ParentName As String        ' Unique name of the parent entry
            Dim Link As String              ' URL
            Dim Image As String             ' Image
            Dim ImageOver As String         ' Image Over
            Dim ImageOpen As String         ' Image when menu is open
            'StyleSheet As String        ' Stylesheet
            'StyleSheetHover As String   ' Hover Stylesheet
            Dim NewWindow As Boolean        ' True opens link in a new window
            Dim OnClick As String           ' Holds action for onClick
        End Structure
        '
        ' ----- A collection of menu entries that have a single function (unique name)
        '
        'private structure MenuType
        '    Name As String              ' Unique name for this menu
        '    Link As String              ' The linked text at the top of this menu
        '    LinkLabel As String         ' The linked text at the top of this menu
        '    CreateDate As Date          ' DateTime when this panel was created
        '    PositionX as integer           ' pixel position on the screen, (default -1)
        '    PositionY as integer           ' pixel position on the screen, (default -1)
        '    StyleSheet As String        ' Stylesheet to put on the whole menu
        '    StyleSheetHover As String   ' Hover Stylesheet to put on the whole menu
        '    EntryCount as integer          ' Number of Entries in this panel
        '    EntrySize as integer           ' Number of Entries in this panel
        '    Entries() As MenuEntryType  ' The menu entries
        '    End structure
        '
        ' ----- Local storage
        '
        Private cpCore As coreClass
        Private iMenuFilePath As String
        '
        ' ----- Menu Entry storage
        '
        Private iEntryCount As Integer          ' Count of Menus in the object
        Private iEntrySize As Integer
        Private iEntry() As MenuEntryType
        '
        ' Private iDQMCount as integer           ' Count of Default Menus for this instance
        ' Private iDQMCLosed As Boolean       ' true if the menu has been closed
        '
        Private iTreeCount As Integer          ' Count of Tree Menus for this instance
        Private iMenuCloseString As String  ' String returned for closing menus
        '
        Private UsedEntries As String       ' String of EntryNames that have been used (for unique test)
        Private EntryIndexName As coreKeyPtrIndexClass
        ' Private EntryIndexID As keyPtrIndex8Class
        '
        ' ----- RollOverFlyout storage
        '
        'Private MenuFlyoutCount as integer           ' Count of Default Menus for this instance
        Private MenuFlyoutNamePrefix As String    ' Random prefix added to element IDs to avoid namespace collision
        Private MenuFlyoutIcon_Local As String      ' string used to mark a button that has a non-hover flyout
        ' Private RollOverFlyoutClosed As Boolean       ' true if the menu has been closed
        Const newmode = True
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
            EntryIndexName = New coreKeyPtrIndexClass
            Randomize()
            MenuFlyoutNamePrefix = "id" & CStr(Int(9999 * Rnd()))
        End Sub
        '
        '===============================================================================
        '   Create a new Menu Entry
        '===============================================================================
        '
        Public Sub AddEntry(ByVal EntryName As String, Optional ByVal ParentiEntryName As String = "", Optional ByVal ImageLink As String = "", Optional ByVal ImageOverLink As String = "", Optional ByVal Link As String = "", Optional ByVal Caption As String = "", Optional ByVal OnClickJavascript As String = "", Optional ByVal Ignore1 As String = "", Optional ByVal ImageOpenLink As String = "", Optional ByVal NewWindow As Boolean = False)
            On Error GoTo ErrorTrap
            '
            Dim MenuEntrySize As Integer
            Dim iEntryName As String
            Dim UcaseEntryName As String
            Dim iNewWindow As Boolean
            '
            iEntryName = genericController.vbReplace(encodeEmptyText(EntryName, ""), ",", " ")
            UcaseEntryName = genericController.vbUCase(iEntryName)
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
                    .OnClick = encodeEmptyText(OnClickJavascript, "")
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
                    .Name = UcaseEntryName
                    .ParentName = genericController.vbUCase(encodeEmptyText(ParentiEntryName, ""))
                    .ImageOver = encodeEmptyText(ImageOverLink, "")
                    .ImageOpen = encodeEmptyText(ImageOpenLink, "")
                    .NewWindow = NewWindow
                End With
                Call EntryIndexName.setPtr(UcaseEntryName, iEntryCount)
                iEntryCount = iEntryCount + 1
            End If
            '
            Exit Sub
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New Exception("Unexpected exception")) ' Call HandleClassError("AddEntry", Err.Number, Err.Source, Err.Description)
        End Sub
        '        '
        '        '===============================================================================
        '        '   Returns the menu specified, if it is in local storage
        '        '
        '        '   It also creates the menu data in a close string that is returned in GetMenuClose.
        '        '   It must be done there so the link buttons height can be calculated.
        '        '===============================================================================
        '        '
        '        Public Function GetMenu(ByVal MenuName As String, Optional ByVal StyleSheetPrefix As String = "") As String
        '            On Error GoTo ErrorTrap
        '            GetMenu = GetTree(MenuName, "", encodeEmptyText(StyleSheetPrefix, "ccTree"))
        '            Exit Function
        '            '
        '            Dim Link As String
        '            Dim EntryPointer As Integer
        '            Dim UcaseMenuName As String
        '            Dim LocalStyleSheetPrefix As String
        '            '
        '            ' ----- Get the menu pointer
        '            '
        '            If iEntryCount > 0 Then
        '                UcaseMenuName = MenuName
        '                LocalStyleSheetPrefix = encodeEmptyText(StyleSheetPrefix, "ccTree")
        '                For EntryPointer = 0 To iEntryCount - 1
        '                    If iEntry(EntryPointer).Name = UcaseMenuName Then
        '                        Exit For
        '                    End If
        '                Next
        '                If EntryPointer < iEntryCount Then
        '                    '
        '                    ' ----- Build the linked -button-
        '                    '
        '                    Link = iEntry(EntryPointer).Link
        '                    If Link = "" Then
        '                        Link = "javascript: ;"
        '                    End If
        '                    '
        '                    GetMenu = vbCrLf _
        '                        & "<DIV id=""tree"" class=""" & LocalStyleSheetPrefix & "Root"" ></DIV>" & vbCrLf
        '                    '
        '                    '   Find the Menu Entry, and create the top element here
        '                    '
        '                    For EntryPointer = 0 To iEntryCount - 1
        '                        With iEntry(EntryPointer)
        '                            If .Name = UcaseMenuName Then
        '                                'iMenuCloseString = iMenuCloseString
        '                                GetMenu = GetMenu _
        '                                    & "<SCRIPT Language=""JavaScript"" type=""text/javascript"">" & vbCrLf _
        '                                    & "var DivLeft,DivTop,ElementObject; " & vbCrLf _
        '                                    & "DivTop = -18; " & vbCrLf _
        '                                    & "DivLeft = 0; " & vbCrLf _
        '                                    & "for (ElementObject=tree;  ElementObject.tagName!='BODY'; ElementObject = ElementObject.offsetParent) { " & vbCrLf _
        '                                    & "    DivTop = DivTop+ElementObject.offsetTop; " & vbCrLf _
        '                                    & "    DivLeft = DivLeft+ElementObject.offsetLeft; " & vbCrLf _
        '                                    & "    } " & vbCrLf _
        '                                    & "var menuBase = new  menuObject(DivTop,DivLeft); " & vbCrLf _
        '                                    & "menuBase.s[0] = new so(0,'" & .Caption & "','" & .Link & "','_blank',''); " & vbCrLf _
        '                                    & GetMenuTreeBranch(.Name, "menuBase.s[0]", "," & EntryPointer) _
        '                                    & "</SCRIPT>" & vbCrLf
        '                                ' & "<SCRIPT LANGUAGE=""JavaScript"" src=""/ccLib/ClientSide/tree30.js""></SCRIPT>" & vbCrLf
        '                                Exit For
        '                            End If
        '                        End With
        '                    Next
        '                    '
        '                    ' ----- Add what is needed to the close string, be carefull of the order
        '                    '
        '                    '
        '                    ' increment the menu count
        '                    '
        '                    iTreeCount = iTreeCount + 1
        '                End If
        '            End If
        '            Exit Function
        '            '
        'ErrorTrap:
        '            Call cpCore.handleExceptionAndRethrow(New Exception("Unexpected exception")) ' Call HandleClassError("GetMenu", Err.Number, Err.Source, Err.Description)
        '        End Function
        '
        '===============================================================================
        '   Gets the Menu Branch for the Tree Menu
        '===============================================================================
        '
        Private Function GetMenuTreeBranch(ByVal ParentName As String, ByVal JSObject As String, ByVal UsedEntries As String) As String
            Dim result As String = ""
            Try
                Dim EntryPointer As Integer
                Dim iUsedEntries As String
                Dim JSChildObject As String
                Dim SubMenuCount As Integer
                '
                iUsedEntries = UsedEntries
                SubMenuCount = 0
                For EntryPointer = 0 To iEntryCount - 1
                    With iEntry(EntryPointer)
                        If .ParentName = ParentName Then
                            If (InStr(1, iUsedEntries & ",", "," & EntryPointer & ",") = 0) Then
                                JSChildObject = JSObject & ".s[" & SubMenuCount & "]"
                                iUsedEntries = iUsedEntries & "," & EntryPointer
                                result = result _
                                    & JSChildObject & " = new so(0,'" & .Caption & "','" & .Link & "','_blank',''); " & vbCrLf _
                                    & GetMenuTreeBranch(.Name, JSChildObject, iUsedEntries)
                                SubMenuCount = SubMenuCount + 1
                            End If
                        End If
                    End With
                Next
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '
        ' Temp log file solution
        '
        Private Sub AppendLog(ByVal Message As String)
            cpCore.log_appendLog(Message, "menuing")
        End Sub
        '
        '===============================================================================
        '   Returns the menu specified, if it is in local storage
        '
        '   It also creates the menu data in a close string that is returned in GetMenuClose.
        '   It must be done there so the link buttons height can be calculated.
        '   Uses a simple UL/Stylesheet method, returning to the server with every click
        '===============================================================================
        '
        Private Function GetMenuTreeList(ByVal MenuName As String, ByVal OpenNodesList As String) As String
            On Error GoTo ErrorTrap
            '
            Dim EntryPointer As Integer
            Dim UcaseMenuName As String
            '
            ' ----- Get the menu pointer
            '
            If iEntryCount > 0 Then
                UcaseMenuName = genericController.vbUCase(MenuName)
                EntryPointer = EntryIndexName.getPtr(UcaseMenuName)
                GetMenuTreeList = GetMenuTreeListBranch2(EntryPointer, "", OpenNodesList)
                Exit Function
            End If
            Exit Function
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New Exception("Unexpected exception")) ' Call HandleClassError("GetMenuTreeList", Err.Number, Err.Source, Err.Description)
        End Function
        '
        '===============================================================================
        '   Gets the Menu Branch for the Tree Menu
        '===============================================================================
        '
        Private Function GetMenuTreeListBranch2(ByVal NodePointer As Integer, ByVal UsedEntriesList As String, ByVal OpenNodesList As String) As String
            Dim result As String = ""
            Try
                '
                Dim Link As String
                Dim EntryPointer As Integer
                Dim UcaseNodeName As String
                Dim Image As String
                Dim Caption As String
                '
                If iEntryCount > 0 Then
                    '
                    ' Output this node
                    '
                    If genericController.vbInstr(1, "," & CStr(NodePointer) & ",", "," & UsedEntriesList & ",") = 0 Then
                        result = result & "<ul Style=""list-style-type: none; margin-left: 20px"">"
                        '
                        ' The Node has not already been used in this branch
                        '
                        Caption = iEntry(NodePointer).Caption
                        Link = cpCore.html.html_EncodeHTML(iEntry(NodePointer).Link)
                        If Link <> "" Then
                            Caption = "<A TARGET=""_blank"" HREF=""" & Link & """>" & Caption & "</A>"
                        End If
                        '
                        If genericController.vbInstr(1, "," & OpenNodesList & ",", "," & CStr(NodePointer) & ",") = 0 Then
                            '
                            ' The branch is closed
                            '
                            Image = iEntry(NodePointer).Image
                            result = result & "<li><A HREF=""?OpenNodesList=" & OpenNodesList & "&OpenNode=" & NodePointer & """><IMG SRC=""" & Image & """ HEIGHT=""18"" WIDTH=""18"" BORDER=0 ALT=""Open Folder""></A>&nbsp;" & Caption & "</li>"
                        Else
                            '
                            ' The branch is open
                            '
                            Image = iEntry(NodePointer).ImageOpen
                            If Image = "" Then
                                Image = iEntry(NodePointer).Image
                            End If
                            result = result _
                            & "<li>" _
                            & "<A HREF=""?OpenNodesList=" & OpenNodesList & "&CloseNode=" & NodePointer & """>" _
                            & "<IMG SRC=""" & Image & """ HEIGHT=""18"" WIDTH=""18"" BORDER=0 ALT=""Close Folder"">" _
                            & "</A>&nbsp;" & Caption & "</li>"
                            '
                            ' Now output any child branches of this node
                            '
                            UcaseNodeName = genericController.vbUCase(iEntry(NodePointer).Name)
                            For EntryPointer = 0 To iEntryCount - 1
                                If (iEntry(EntryPointer).ParentName = UcaseNodeName) Then
                                    result = result & GetMenuTreeListBranch2(EntryPointer, UsedEntriesList & "," & NodePointer, OpenNodesList)
                                End If
                            Next
                        End If
                        result = result & "</ul>" & vbCrLf
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '        '
        '        '===============================================================================
        '        '   Returns the menu specified, if it is in local storage
        '        '
        '        '   It also creates the menu data in a close string that is returned in GetTreeClose.
        '        '   It must be done there so the link buttons height can be calculated.
        '        '===============================================================================
        '        '
        '        Public Function GetTree(ByVal MenuName As String, ByVal OpenMenuName As String, Optional ByVal StyleSheetPrefix As String = "") As String
        '            On Error GoTo ErrorTrap
        '            '
        '            Dim Link As String
        '            Dim EntryPointer As Integer
        '            Dim UcaseMenuName As String
        '            Dim RootFound As Boolean
        '            Dim UsedEntries As String
        '            Dim Caption As String
        '            Dim JSString As String
        '            '
        '            ' ----- Get the menu pointer
        '            '
        '            If iEntryCount > 0 Then
        '                UcaseMenuName = genericController.vbUCase(MenuName)
        '                If StyleSheetPrefix = "" Then
        '                    StyleSheetPrefix = "ccTree"
        '                End If
        '                If True Then
        '                    '
        '                    ' ----- Build the linked -button-
        '                    '
        '                    Link = iEntry(EntryPointer).Link
        '                    If Link = "" Then
        '                        Link = "javascript: ;"
        '                    End If
        '                    '
        '                    '   Find the Menu Entry, and create the top element here
        '                    '
        '                    UsedEntries = ""
        '                    For EntryPointer = 0 To iEntryCount - 1
        '                        With iEntry(EntryPointer)
        '                            If .Name = UcaseMenuName Then
        '                                Caption = .Caption
        '                                If .Link <> "" Then
        '                                    Caption = "<a href=""" & cpcore.html.html_EncodeHTML(.Link) & """>" & Caption & "</a>"
        '                                End If
        '                                UsedEntries = UsedEntries & "," & CStr(EntryPointer)
        '                                GetTree = "" _
        '                                    & vbCrLf & "<ul class=mktree id=tree" & iTreeCount & ">" & vbCrLf _
        '                                    & vbCrLf & " <li id=""" & .Name & """><span class=mkc>" & Caption & "</span>" _
        '                                    & vbCrLf & " <ul>" & vbCrLf _
        '                                    & GetMKTreeBranch(UcaseMenuName, UsedEntries, 2) _
        '                                    & vbCrLf & " </ul>" & vbCrLf _
        '                                    & vbCrLf & "</li></ul>" & vbCrLf
        '                                Exit For
        '                            End If
        '                        End With
        '                    Next
        '                    If UsedEntries = "" Then
        '                        GetTree = "" _
        '                            & vbCrLf & "<ul class=mktree id=tree" & iTreeCount & ">" _
        '                            & GetMKTreeBranch(UcaseMenuName, UsedEntries, 1) _
        '                            & vbCrLf & "</ul>" & vbCrLf
        '                    End If
        '                    '
        '                    'Call cmc.main_AddStylesheetLink("/ccLib/mktree/mktree.css")
        '                    'Call cmc.main_AddHeadScriptLink("/ccLib/mktree/mktree.js", "mktree")
        '                    'Call cmc.main_AddOnLoadJavascript("convertTrees();")
        '                    GetTree = "" _
        '                        & vbCrLf & "<link rel=stylesheet href=/ccLib/mktree/mktree.css type=text/css>" _
        '                        & vbCrLf & "<script type=""text/javascript"" src=/ccLib/mktree/mktree.js></script>" _
        '                        & GetTree
        '                    GetTree = GetTree & "<script type=""text/javascript"">convertTrees();"
        '                    If OpenMenuName <> "" Then
        '                        JSString = genericController.vbUCase(OpenMenuName)
        '                        JSString = genericController.vbReplace(JSString, "\", "\\")
        '                        JSString = genericController.vbReplace(JSString, vbCrLf, "\n")
        '                        JSString = genericController.vbReplace(JSString, "'", "\'")
        '                        'Call cmc.main_AddOnLoadJavascript("expandToItem('tree" & iTreeCount & "','" & JSString & "');")
        '                        GetTree = GetTree & "expandToItem('tree" & iTreeCount & "','" & JSString & "');"
        '                    End If
        '                    GetTree = GetTree & "</script>"
        '                    '
        '                    ' increment the menu count
        '                    '
        '                    iTreeCount = iTreeCount + 1
        '                End If
        '            End If


        '            Exit Function
        '            '
        'ErrorTrap:
        '            Call cpCore.handleExceptionAndRethrow(New Exception("Unexpected exception")) ' Call HandleClassError("GetTree", Err.Number, Err.Source, Err.Description)
        '        End Function
        '        '
        '        '===============================================================================
        '        '   Gets the Menu Branch for the Tree Menu
        '        '===============================================================================
        '        '
        '        Private Function GetMKTreeBranch(ByVal ParentName As String, ByVal UsedEntries As String, ByVal Depth As Integer) As String
        '            On Error GoTo ErrorTrap
        '            '
        '            Dim EntryPointer As Integer
        '            Dim iUsedEntries As String
        '            Dim SubMenuCount As Integer
        '            Dim ChildMenu As String
        '            Dim Caption As String
        '            '
        '            iUsedEntries = UsedEntries
        '            SubMenuCount = 0
        '            For EntryPointer = 0 To iEntryCount - 1
        '                With iEntry(EntryPointer)
        '                    If .ParentName = ParentName Then
        '                        If (InStr(1, iUsedEntries & ",", "," & EntryPointer & ",") = 0) Then
        '                            Caption = .Caption
        '                            If .OnClick <> "" And .Link <> "" Then
        '                                Caption = "<a href=""" & cpcore.html.html_EncodeHTML(.Link) & """ onClick=""" & .OnClick & """>" & Caption & "</a>"
        '                            ElseIf .OnClick <> "" Then
        '                                Caption = "<a href=""#"" onClick=""" & .OnClick & """>" & Caption & "</a>"
        '                            ElseIf .Link <> "" Then
        '                                Caption = "<a href=""" & cpcore.html.html_EncodeHTML(.Link) & """>" & Caption & "</a>"
        '                            Else
        '                                Caption = Caption
        '                            End If
        '                            iUsedEntries = iUsedEntries & "," & EntryPointer

        '                            ChildMenu = GetMKTreeBranch(.Name, iUsedEntries, Depth + 1)
        '                            If newmode Then
        '                                If ChildMenu = "" Then
        '                                    GetMKTreeBranch = GetMKTreeBranch _
        '                                        & vbCrLf & New String(" "c, Depth) & "<li class=mklb id=""" & .Name & """ >" _
        '                                        & "<div class=""mkd"">" _
        '                                        & "<span class=mkb>&nbsp;</span>" _
        '                                        & "</div>" _
        '                                        & Caption _
        '                                        & "</li>"
        '                                Else
        '                                    '
        '                                    ' 3/18/2010 changes to keep firefox from blocking clicks
        '                                    '
        '                                    GetMKTreeBranch = GetMKTreeBranch _
        '                                        & vbCrLf & New String(" "c, Depth) & "<li class=""mklc"" id=""" & .Name & """ >" _
        '                                        & "<div class=""mkd"" >" _
        '                                        & "<span class=mkb onclick=""mkClick(this)"">&nbsp;</span>" _
        '                                        & "</div>" _
        '                                        & Caption _
        '                                        & vbCrLf & New String(" "c, Depth + 1) & "<ul>" _
        '                                        & ChildMenu _
        '                                        & vbCrLf & New String(" "c, Depth + 1) & "</ul>" _
        '                                        & "</li>"
        '                                End If
        '                            Else
        '                                If ChildMenu <> "" Then
        '                                    ChildMenu = "" _
        '                                        & vbCrLf & New String(" "c, Depth + 1) & "<ul>" _
        '                                        & ChildMenu _
        '                                        & vbCrLf & New String(" "c, Depth + 1) & "</ul>" _
        '                                        & ""
        '                                End If
        '                                GetMKTreeBranch = GetMKTreeBranch _
        '                                    & vbCrLf & New String(" "c, Depth) & "<li class=mklc id=""" & .Name & """>" _
        '                                    & Caption _
        '                                    & ChildMenu _
        '                                    & vbCrLf & New String(" "c, Depth) & "</li>"
        '                            End If
        '                            SubMenuCount = SubMenuCount + 1
        '                        End If
        '                    End If
        '                End With
        '            Next
        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            Call cpCore.handleExceptionAndRethrow(New Exception("Unexpected exception")) ' Call HandleClassError("GetMKTreeBranch", Err.Number, Err.Source, Err.Description)
        '        End Function
        '
        '========================================================================
        ''' <summary>
        ''' handle legacy errors in the is class
        ''' </summary>
        ''' <param name="MethodName"></param>
        ''' <param name="ErrNumber"></param>
        ''' <param name="ErrSource"></param>
        ''' <param name="ErrDescription"></param>
        ''' <remarks></remarks>
        Private Sub handleLegacyClassError(ByVal MethodName As String, ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String)
            '
            Call cpCore.handleExceptionAndRethrow(New Exception("unexpected exception in method [" & MethodName & "], errDescription [" & ErrDescription & "]"))
            '
        End Sub
    End Class
End Namespace
