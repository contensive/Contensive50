

Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports Contensive.Core.Models.Entity

Namespace Contensive.Core.Controllers
    ''' <summary>
    ''' Tools used to assemble html document elements. This is not a storage for assembling a document (see docController)
    ''' </summary>
    Public Class htmlController
        '
        '========================================================================
        ' main_GetRecordEditLink2( iContentName, iRecordID, AllowCut, RecordName )
        '
        '   ContentName The content for this link
        '   RecordID    The ID of the record in the Table
        '   AllowCut
        '   RecordName
        '   IsEditing
        '========================================================================
        '
        Public Function main_GetRecordEditLink2(ByVal ContentName As String, ByVal RecordID As Integer, ByVal AllowCut As Boolean, ByVal RecordName As String, ByVal IsEditing As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetRecordEditLink2")
            '
            'If Not (true) Then Exit Function
            '
            Dim CS As Integer
            Dim SQL As String
            Dim ContentID As Integer
            Dim Link As String
            Dim MethodName As String
            Dim iContentName As String
            Dim iRecordID As Integer
            Dim RootEntryName As String
            Dim ClipBoard As String
            Dim WorkingLink As String
            Dim iAllowCut As Boolean
            Dim Icon As String
            Dim ContentCaption As String
            '
            iContentName = genericController.encodeText(ContentName)
            iRecordID = genericController.EncodeInteger(RecordID)
            iAllowCut = genericController.EncodeBoolean(AllowCut)
            ContentCaption = genericController.encodeHTML(iContentName)
            If genericController.vbLCase(ContentCaption) = "aggregate functions" Then
                ContentCaption = "Add-on"
            End If
            If genericController.vbLCase(ContentCaption) = "aggregate function objects" Then
                ContentCaption = "Add-on"
            End If
            ContentCaption = ContentCaption & " record"
            If RecordName <> "" Then
                ContentCaption = ContentCaption & ", named '" & RecordName & "'"
            End If
            '
            MethodName = "main_GetRecordEditLink2"
            '
            main_GetRecordEditLink2 = ""
            If (iContentName = "") Then
                Throw (New ApplicationException("ContentName [" & ContentName & "] is invalid")) ' handleLegacyError14(MethodName, "")
            Else
                If (iRecordID < 1) Then
                    Throw (New ApplicationException("RecordID [" & RecordID & "] is invalid")) ' handleLegacyError14(MethodName, "")
                Else
                    If IsEditing Then
                        '
                        ' Edit link, main_Get the CID
                        '
                        ContentID = Models.Complex.cdefModel.getContentId(cpCore, iContentName)
                        '
                        main_GetRecordEditLink2 = main_GetRecordEditLink2 _
                            & "<a" _
                            & " class=""ccRecordEditLink"" " _
                            & " TabIndex=-1" _
                            & " href=""" & genericController.encodeHTML("/" & cpCore.serverConfig.appConfig.adminRoute & "?cid=" & ContentID & "&id=" & iRecordID & "&af=4&aa=2&ad=1") & """"
                        main_GetRecordEditLink2 = main_GetRecordEditLink2 _
                            & "><img" _
                            & " src=""/ccLib/images/IconContentEdit.gif""" _
                            & " border=""0""" _
                            & " alt=""Edit this " & genericController.encodeHTML(ContentCaption) & """" _
                            & " title=""Edit this " & genericController.encodeHTML(ContentCaption) & """" _
                            & " align=""absmiddle""" _
                            & "></a>"
                        '
                        ' Cut Link if enabled
                        '
                        If iAllowCut Then
                            WorkingLink = genericController.modifyLinkQuery(cpCore.webServer.requestPage & "?" & cpCore.doc.refreshQueryString, RequestNameCut, genericController.encodeText(ContentID) & "." & genericController.encodeText(RecordID), True)
                            main_GetRecordEditLink2 = "" _
                                & main_GetRecordEditLink2 _
                                & "<a class=""ccRecordCutLink"" TabIndex=""-1"" href=""" & genericController.encodeHTML(WorkingLink) & """><img src=""/ccLib/images/Contentcut.gif"" border=""0"" alt=""Cut this " & ContentCaption & " to clipboard"" title=""Cut this " & ContentCaption & " to clipboard"" align=""absmiddle""></a>"
                        End If
                        '
                        ' Help link if enabled
                        '
                        Dim helpLink As String
                        helpLink = ""
                        'helpLink = main_GetHelpLink(5, "Editing " & ContentCaption, "Turn on Edit icons by checking 'Edit' in the tools panel, and click apply.<br><br><img src=""/ccLib/images/IconContentEdit.gif"" style=""vertical-align:middle""> Edit-Content icon<br><br>Edit-Content icons appear in your content. Click them to edit your content.")
                        main_GetRecordEditLink2 = "" _
                            & main_GetRecordEditLink2 _
                            & helpLink
                        '
                        main_GetRecordEditLink2 = "<span class=""ccRecordLinkCon"" style=""white-space:nowrap;"">" & main_GetRecordEditLink2 & "</span>"
                        ''
                        'main_GetRecordEditLink2 = "" _
                        '    & cr & "<div style=""position:absolute;"">" _
                        '    & genericController.kmaIndent(main_GetRecordEditLink2) _
                        '    & cr & "</div>"
                        '
                        'main_GetRecordEditLink2 = "" _
                        '    & cr & "<div style=""position:relative;display:inline;"">" _
                        '    & genericController.kmaIndent(main_GetRecordEditLink2) _
                        '    & cr & "</div>"
                    End If

                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' Print an add link for the current ContentSet
        '   iCSPointer is the content set to be added to
        '   PresetNameValueList is a name=value pair to force in the added record
        '========================================================================
        '
        Public Function main_cs_getRecordAddLink(ByVal CSPointer As Integer, Optional ByVal PresetNameValueList As String = "", Optional ByVal AllowPaste As Boolean = False) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("cs_getRecordAddLink")
            '
            'If Not (true) Then Exit Function
            '
            Dim ContentName As String
            Dim iPresetNameValueList As String
            Dim MethodName As String
            Dim iCSPointer As Integer
            '
            iCSPointer = genericController.EncodeInteger(CSPointer)
            iPresetNameValueList = genericController.encodeEmptyText(PresetNameValueList, "")
            '
            MethodName = "main_cs_getRecordAddLink"
            '
            If iCSPointer < 0 Then
                Throw (New ApplicationException("invalid ContentSet pointer [" & iCSPointer & "]")) ' handleLegacyError14(MethodName, "main_cs_getRecordAddLink was called with ")
            Else
                '
                ' Print an add tag to the iCSPointers Content
                '
                ContentName = cpCore.db.cs_getContentName(iCSPointer)
                If ContentName = "" Then
                    Throw (New ApplicationException("main_cs_getRecordAddLink was called with a ContentSet that was created with an SQL statement. The function requires a ContentSet opened with an OpenCSContent.")) ' handleLegacyError14(MethodName, "")
                Else
                    main_cs_getRecordAddLink = main_GetRecordAddLink(ContentName, iPresetNameValueList, AllowPaste)
                End If
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' main_GetRecordAddLink( iContentName, iPresetNameValueList )
        '
        '   Returns a string of add tags for the Content Definition included, and all
        '   child contents of that area.
        '
        '   iContentName The content for this link
        '   iPresetNameValueList The sql equivalent used to select the record.
        '           translates to name0=value0,name1=value1.. pairs separated by ,
        '
        '   LowestRootMenu - The Menu in the flyout structure that is the furthest down
        '   in the chain that the user has content access to. This is so a content manager
        '   does not have to navigate deep into a structure to main_Get to content he can
        '   edit.
        '   Basically, the entire menu is created down from the MenuName, and populated
        '   with all the entiries this user has access to. The LowestRequiredMenuName is
        '   is returned from the _branch routine, and that is to root on-which the
        '   main_GetMenu uses
        '========================================================================
        '
        Public Function main_GetRecordAddLink(ByVal ContentName As String, ByVal PresetNameValueList As String, Optional ByVal AllowPaste As Boolean = False) As String
            main_GetRecordAddLink = main_GetRecordAddLink2(genericController.encodeText(ContentName), genericController.encodeText(PresetNameValueList), AllowPaste, cpCore.doc.authContext.isEditing(ContentName))
        End Function
        '
        '========================================================================
        ' main_GetRecordAddLink2
        '
        '   Returns a string of add tags for the Content Definition included, and all
        '   child contents of that area.
        '
        '   iContentName The content for this link
        '   iPresetNameValueList The sql equivalent used to select the record.
        '           translates to name0=value0,name1=value1.. pairs separated by ,
        '
        '   LowestRootMenu - The Menu in the flyout structure that is the furthest down
        '   in the chain that the user has content access to. This is so a content manager
        '   does not have to navigate deep into a structure to main_Get to content he can
        '   edit.
        '   Basically, the entire menu is created down from the MenuName, and populated
        '   with all the entiries this user has access to. The LowestRequiredMenuName is
        '   is returned from the _branch routine, and that is to root on-which the
        '   main_GetMenu uses
        '========================================================================
        '
        Public Function main_GetRecordAddLink2(ByVal ContentName As String, ByVal PresetNameValueList As String, ByVal AllowPaste As Boolean, ByVal IsEditing As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetRecordAddLink")
            '
            'If Not (true) Then Exit Function
            '
            Dim ParentID As Integer
            Dim BufferString As String
            Dim MethodName As String
            Dim iContentName As String
            Dim iContentID As Integer
            Dim iPresetNameValueList As String
            Dim MenuName As String
            Dim MenuHasBranches As Boolean
            Dim LowestRequiredMenuName As String = String.Empty
            Dim ClipBoard As String
            Dim PasteLink As String = String.Empty
            Dim Position As Integer
            Dim ClipBoardArray As String()
            Dim ClipboardContentID As Integer
            Dim ClipChildRecordID As Integer
            Dim iAllowPaste As Boolean
            Dim useFlyout As Boolean
            Dim csChildContent As Integer
            Dim Link As String
            '
            MethodName = "main_GetRecordAddLink"
            '
            main_GetRecordAddLink2 = ""
            If IsEditing Then
                iContentName = genericController.encodeText(ContentName)
                iPresetNameValueList = genericController.encodeText(PresetNameValueList)
                iPresetNameValueList = genericController.vbReplace(iPresetNameValueList, "&", ",")
                iAllowPaste = genericController.EncodeBoolean(AllowPaste)

                If iContentName = "" Then
                    Throw (New ApplicationException("Method called with blank ContentName")) ' handleLegacyError14(MethodName, "")
                Else
                    iContentID = Models.Complex.cdefModel.getContentId(cpCore, iContentName)
                    csChildContent = cpCore.db.csOpen("Content", "ParentID=" & iContentID, , , , , , "id")
                    useFlyout = cpCore.db.csOk(csChildContent)
                    Call cpCore.db.csClose(csChildContent)
                    '
                    If Not useFlyout Then
                        Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?cid=" & iContentID & "&af=4&aa=2&ad=1"
                        If PresetNameValueList <> "" Then
                            Link = Link & "&wc=" & genericController.EncodeRequestVariable(PresetNameValueList)
                        End If
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 _
                            & "<a" _
                            & " TabIndex=-1" _
                            & " href=""" & genericController.encodeHTML(Link) & """"
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 _
                            & "><img" _
                            & " src=""/ccLib/images/IconContentAdd.gif""" _
                            & " border=""0""" _
                            & " alt=""Add record""" _
                            & " title=""Add record""" _
                            & " align=""absmiddle""" _
                            & "></a>"
                    Else
                        '
                        MenuName = genericController.GetRandomInteger().ToString
                        Call cpCore.menuFlyout.menu_AddEntry(MenuName, , "/ccLib/images/IconContentAdd.gif", , , , "stylesheet", "stylesheethover")
                        LowestRequiredMenuName = main_GetRecordAddLink_AddMenuEntry(iContentName, iPresetNameValueList, "", MenuName, MenuName)
                    End If
                    '
                    ' Add in the paste entry, if needed
                    '
                    If iAllowPaste Then
                        ClipBoard = cpCore.visitProperty.getText("Clipboard", "")
                        If ClipBoard <> "" Then
                            Position = genericController.vbInstr(1, ClipBoard, ".")
                            If Position <> 0 Then
                                ClipBoardArray = Split(ClipBoard, ".")
                                If UBound(ClipBoardArray) > 0 Then
                                    ClipboardContentID = genericController.EncodeInteger(ClipBoardArray(0))
                                    ClipChildRecordID = genericController.EncodeInteger(ClipBoardArray(1))
                                    'iContentID = main_GetContentID(iContentName)
                                    If Models.Complex.cdefModel.isWithinContent(cpCore, ClipboardContentID, iContentID) Then
                                        If genericController.vbInstr(1, iPresetNameValueList, "PARENTID=", vbTextCompare) <> 0 Then
                                            '
                                            ' must test for main_IsChildRecord
                                            '
                                            BufferString = iPresetNameValueList
                                            BufferString = genericController.vbReplace(BufferString, "(", "")
                                            BufferString = genericController.vbReplace(BufferString, ")", "")
                                            BufferString = genericController.vbReplace(BufferString, ",", "&")
                                            ParentID = genericController.EncodeInteger(genericController.main_GetNameValue_Internal(cpCore, BufferString, "Parentid"))
                                        End If


                                        If (ParentID <> 0) And (Not pageContentController.isChildRecord(cpCore, iContentName, ParentID, ClipChildRecordID)) Then
                                            '
                                            ' Can not paste as child of itself
                                            '
                                            PasteLink = cpCore.webServer.requestPage & "?" & cpCore.doc.refreshQueryString
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePaste, "1", True)
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentContentID, CStr(iContentID), True)
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentRecordID, CStr(ParentID), True)
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteFieldList, iPresetNameValueList, True)
                                            main_GetRecordAddLink2 = main_GetRecordAddLink2 _
                                                & "<a class=""ccRecordCutLink"" TabIndex=""-1"" href=""" & genericController.encodeHTML(PasteLink) & """><img src=""/ccLib/images/ContentPaste.gif"" border=""0"" alt=""Paste record in clipboard here"" title=""Paste record in clipboard here"" align=""absmiddle""></a>"
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                    '
                    ' Add in the available flyout Navigator Entries
                    '
                    If LowestRequiredMenuName <> "" Then
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 & cpCore.menuFlyout.getMenu(LowestRequiredMenuName, 0)
                        main_GetRecordAddLink2 = genericController.vbReplace(main_GetRecordAddLink2, "class=""ccFlyoutButton"" ", "", 1, 99, vbTextCompare)
                        If PasteLink <> "" Then
                            main_GetRecordAddLink2 = main_GetRecordAddLink2 & "<a TabIndex=-1 href=""" & genericController.encodeHTML(PasteLink) & """><img src=""/ccLib/images/ContentPaste.gif"" border=""0"" alt=""Paste content from clipboard"" align=""absmiddle""></a>"
                        End If
                    End If
                    '
                    ' Help link if enabled
                    '
                    Dim helpLink As String
                    helpLink = ""
                    'helpLink = main_GetHelpLink(6, "Adding " & iContentName, "Turn on Edit icons by checking 'Edit' in the tools panel, and click apply.<br><br><img src=""/ccLib/images/IconContentAdd.gif"" " & IconWidthHeight & " style=""vertical-align:middle""> Add-Content icon<br><br>Add-Content icons appear in your content. Click them to add content.")
                    main_GetRecordAddLink2 = main_GetRecordAddLink2 & helpLink                '
                    If main_GetRecordAddLink2 <> "" Then
                        main_GetRecordAddLink2 = "" _
                            & vbCrLf & vbTab & "<div style=""display:inline;"">" _
                            & genericController.htmlIndent(main_GetRecordAddLink2) _
                            & vbCrLf & vbTab & "</div>"
                    End If
                    '
                    ' ----- Add the flyout panels to the content to return
                    '       This must be here so if the call is made after main_ClosePage, the panels will still deliver
                    '
                    If LowestRequiredMenuName <> "" Then
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 & cpCore.menuFlyout.menu_GetClose()
                        If genericController.vbInstr(1, main_GetRecordAddLink2, "IconContentAdd.gif", vbTextCompare) <> 0 Then
                            main_GetRecordAddLink2 = genericController.vbReplace(main_GetRecordAddLink2, "IconContentAdd.gif"" ", "IconContentAdd.gif"" align=""absmiddle"" ")
                        End If
                    End If
                    main_GetRecordAddLink2 = genericController.vbReplace(main_GetRecordAddLink2, "target=", "xtarget=", 1, 99, vbTextCompare)
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' main_GetRecordAddLink_AddMenuEntry( ContentName, PresetNameValueList, ContentNameList, MenuName )
        '
        '   adds an add entry for the content name, and all the child content
        '   returns the MenuName of the lowest branch that has valid
        '   Navigator Entries.
        '
        '   ContentName The content for this link
        '   PresetNameValueList The sql equivalent used to select the record.
        '           translates to (name0=value0)&(name1=value1).. pairs separated by &
        '   ContentNameList is a comma separated list of names of the content included so far
        '   MenuName is the name of the root branch, for flyout menu
        '
        '   IsMember(), main_IsAuthenticated() And Member_AllowLinkAuthoring must already be checked
        '========================================================================
        '
        Private Function main_GetRecordAddLink_AddMenuEntry(ByVal ContentName As String, ByVal PresetNameValueList As String, ByVal ContentNameList As String, ByVal MenuName As String, ByVal ParentMenuName As String) As String
            Dim result As String = ""
            Dim Copy As String
            Dim CS As Integer
            Dim SQL As String
            Dim csChildContent As Integer
            Dim ContentID As Integer
            Dim Link As String
            Dim MyContentNameList As String
            Dim ButtonCaption As String
            Dim ContentRecordFound As Boolean
            Dim ContentAllowAdd As Boolean
            Dim GroupRulesAllowAdd As Boolean
            Dim MemberRulesDateExpires As Date
            Dim MemberRulesAllow As Boolean
            Dim ChildMenuButtonCount As Integer
            Dim ChildMenuName As String
            Dim ChildContentName As String
            '
            Link = ""
            MyContentNameList = ContentNameList
            If (ContentName = "") Then
                Throw (New ApplicationException("main_GetRecordAddLink, ContentName is empty")) ' handleLegacyError14(MethodName, "")
            Else
                If (InStr(1, MyContentNameList, "," & genericController.vbUCase(ContentName) & ",") >= 0) Then
                    Throw (New ApplicationException("result , Content Child [" & ContentName & "] is one of its own parents")) ' handleLegacyError14(MethodName, "")
                Else
                    MyContentNameList = MyContentNameList & "," & genericController.vbUCase(ContentName) & ","
                    '
                    ' ----- Select the Content Record for the Menu Entry selected
                    '
                    ContentRecordFound = False
                    If cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                        '
                        ' ----- admin member, they have access, main_Get ContentID and set markers true
                        '
                        SQL = "SELECT ID as ContentID, AllowAdd as ContentAllowAdd, 1 as GroupRulesAllowAdd, null as MemberRulesDateExpires" _
                            & " FROM ccContent" _
                            & " WHERE (" _
                            & " (ccContent.Name=" & cpCore.db.encodeSQLText(ContentName) & ")" _
                            & " AND(ccContent.active<>0)" _
                            & " );"
                        CS = cpCore.db.csOpenSql(SQL)
                        If cpCore.db.csOk(CS) Then
                            '
                            ' Entry was found
                            '
                            ContentRecordFound = True
                            ContentID = cpCore.db.csGetInteger(CS, "ContentID")
                            ContentAllowAdd = cpCore.db.csGetBoolean(CS, "ContentAllowAdd")
                            GroupRulesAllowAdd = True
                            MemberRulesDateExpires = Date.MinValue
                            MemberRulesAllow = True
                        End If
                        Call cpCore.db.csClose(CS)
                    Else
                        '
                        ' non-admin member, first check if they have access and main_Get true markers
                        '
                        SQL = "SELECT ccContent.ID as ContentID, ccContent.AllowAdd as ContentAllowAdd, ccGroupRules.AllowAdd as GroupRulesAllowAdd, ccMemberRules.DateExpires as MemberRulesDateExpires" _
                            & " FROM (((ccContent" _
                                & " LEFT JOIN ccGroupRules ON ccGroupRules.ContentID=ccContent.ID)" _
                                & " LEFT JOIN ccgroups ON ccGroupRules.GroupID=ccgroups.ID)" _
                                & " LEFT JOIN ccMemberRules ON ccgroups.ID=ccMemberRules.GroupID)" _
                                & " LEFT JOIN ccMembers ON ccMemberRules.MemberID=ccMembers.ID" _
                            & " WHERE (" _
                            & " (ccContent.Name=" & cpCore.db.encodeSQLText(ContentName) & ")" _
                            & " AND(ccContent.active<>0)" _
                            & " AND(ccGroupRules.active<>0)" _
                            & " AND(ccMemberRules.active<>0)" _
                            & " AND((ccMemberRules.DateExpires is Null)or(ccMemberRules.DateExpires>" & cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) & "))" _
                            & " AND(ccgroups.active<>0)" _
                            & " AND(ccMembers.active<>0)" _
                            & " AND(ccMembers.ID=" & cpCore.doc.authContext.user.id & ")" _
                            & " );"
                        CS = cpCore.db.csOpenSql(SQL)
                        If cpCore.db.csOk(CS) Then
                            '
                            ' ----- Entry was found, member has some kind of access
                            '
                            ContentRecordFound = True
                            ContentID = cpCore.db.csGetInteger(CS, "ContentID")
                            ContentAllowAdd = cpCore.db.csGetBoolean(CS, "ContentAllowAdd")
                            GroupRulesAllowAdd = cpCore.db.csGetBoolean(CS, "GroupRulesAllowAdd")
                            MemberRulesDateExpires = cpCore.db.csGetDate(CS, "MemberRulesDateExpires")
                            MemberRulesAllow = False
                            If MemberRulesDateExpires = Date.MinValue Then
                                MemberRulesAllow = True
                            ElseIf (MemberRulesDateExpires > cpCore.doc.profileStartTime) Then
                                MemberRulesAllow = True
                            End If
                        Else
                            '
                            ' ----- No entry found, this member does not have access, just main_Get ContentID
                            '
                            ContentRecordFound = True
                            ContentID = Models.Complex.cdefModel.getContentId(cpCore, ContentName)
                            ContentAllowAdd = False
                            GroupRulesAllowAdd = False
                            MemberRulesAllow = False
                        End If
                        Call cpCore.db.csClose(CS)
                    End If
                    If ContentRecordFound Then
                        '
                        ' Add the Menu Entry* to the current menu (MenuName)
                        '
                        Link = ""
                        ButtonCaption = ContentName
                        result = MenuName
                        If ContentAllowAdd And GroupRulesAllowAdd And MemberRulesAllow Then
                            Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?cid=" & ContentID & "&af=4&aa=2&ad=1"
                            If PresetNameValueList <> "" Then
                                Dim NameValueList As String
                                NameValueList = PresetNameValueList
                                Link = Link & "&wc=" & genericController.EncodeRequestVariable(PresetNameValueList)
                            End If
                        End If
                        Call cpCore.menuFlyout.menu_AddEntry(MenuName & ":" & ContentName, ParentMenuName, , , Link, ButtonCaption, "", "", True)
                        '
                        ' Create child submenu if Child Entries found
                        '
                        csChildContent = cpCore.db.csOpen("Content", "ParentID=" & ContentID, , , , , , "name")
                        If Not cpCore.db.csOk(csChildContent) Then
                            '
                            ' No child menu
                            '
                        Else
                            '
                            ' Add the child menu
                            '
                            ChildMenuName = MenuName & ":" & ContentName
                            ChildMenuButtonCount = 0
                            '
                            ' ----- Create the ChildPanel with all Children found
                            '
                            Do While cpCore.db.csOk(csChildContent)
                                ChildContentName = cpCore.db.csGetText(csChildContent, "name")
                                Copy = main_GetRecordAddLink_AddMenuEntry(ChildContentName, PresetNameValueList, MyContentNameList, MenuName, ParentMenuName)
                                If Copy <> "" Then
                                    ChildMenuButtonCount = ChildMenuButtonCount + 1
                                End If
                                If (result = "") And (Copy <> "") Then
                                    result = Copy
                                End If
                                cpCore.db.csGoNext(csChildContent)
                            Loop
                        End If
                    End If
                End If
                Call cpCore.db.csClose(csChildContent)
            End If
            Return result
        End Function
        '
        '========================================================================
        '   main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
        ' Return a panel with the input as center
        '========================================================================
        '
        Public Function main_GetPanel(ByVal Panel As String, Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "ccPanelHilite", Optional ByVal StyleShadow As String = "ccPanelShadow", Optional ByVal Width As String = "100%", Optional ByVal Padding As Integer = 5, Optional ByVal HeightMin As Integer = 1) As String
            Dim ContentPanelWidth As String
            Dim MethodName As String
            Dim MyStylePanel As String
            Dim MyStyleHilite As String
            Dim MyStyleShadow As String
            Dim MyWidth As String
            Dim MyPadding As String
            Dim MyHeightMin As String
            Dim s0 As String
            Dim s1 As String
            Dim s2 As String
            Dim s3 As String
            Dim s4 As String
            Dim contentPanelWidthStyle As String
            '
            MethodName = "main_GetPanelTop"
            '
            MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel")
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite")
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow")
            MyWidth = genericController.encodeEmptyText(Width, "100%")
            MyPadding = Padding.ToString
            MyHeightMin = HeightMin.ToString
            '
            If genericController.vbIsNumeric(MyWidth) Then
                ContentPanelWidth = (CInt(MyWidth) - 2).ToString
                contentPanelWidthStyle = ContentPanelWidth & "px"
            Else
                ContentPanelWidth = "100%"
                contentPanelWidthStyle = ContentPanelWidth
            End If
            '
            '
            '
            s0 = "" _
                & cr & "<td style=""padding:" & MyPadding & "px;vertical-align:top"" class=""" & MyStylePanel & """>" _
                & genericController.htmlIndent(genericController.encodeText(Panel)) _
                & cr & "</td>" _
                & ""
            '
            s1 = "" _
                & cr & "<tr>" _
                & genericController.htmlIndent(s0) _
                & cr & "</tr>" _
                & ""
            s2 = "" _
                & cr & "<table style=""width:" & contentPanelWidthStyle & ";border:0px;"" class=""" & MyStylePanel & """ cellspacing=""0"">" _
                & genericController.htmlIndent(s1) _
                & cr & "</table>" _
                & ""
            s3 = "" _
                & cr & "<td width=""1"" height=""" & MyHeightMin & """ class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""" & MyHeightMin & """ width=""1"" ></td>" _
                & cr & "<td width=""" & ContentPanelWidth & """ valign=""top"" align=""left"" class=""" & MyStylePanel & """>" _
                & genericController.htmlIndent(s2) _
                & cr & "</td>" _
                & cr & "<td width=""1"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""1"" ></td>" _
                & ""
            s4 = "" _
                & cr & "<tr>" _
                & cr2 & "<td colspan=""3"" class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                & cr & "</tr>" _
                & cr & "<tr>" _
                & genericController.htmlIndent(s3) _
                & cr & "</tr>" _
                & cr & "<tr>" _
                & cr2 & "<td colspan=""3"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                & cr & "</tr>" _
                & ""
            main_GetPanel = "" _
                & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""" & MyWidth & """ class=""" & MyStylePanel & """>" _
                & genericController.htmlIndent(s4) _
                & cr & "</table>" _
                & ""
        End Function
        '
        '========================================================================
        '   main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
        ' Return a panel with the input as center
        '========================================================================
        '
        Public Function main_GetReversePanel(ByVal Panel As String, Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "ccPanelShadow", Optional ByVal StyleShadow As String = "ccPanelHilite", Optional ByVal Width As String = "", Optional ByVal Padding As String = "", Optional ByVal HeightMin As String = "") As String
            Dim MyStyleHilite As String
            Dim MyStyleShadow As String
            '
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelShadow")
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelHilite")

            main_GetReversePanel = main_GetPanelTop(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding, HeightMin) _
                & genericController.encodeText(Panel) _
                & main_GetPanelBottom(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding)
        End Function
        '
        '========================================================================
        ' Return a panel header with the header message reversed out of the left
        '========================================================================
        '
        Public Function main_GetPanelHeader(ByVal HeaderMessage As String, Optional ByVal RightSideMessage As String = "") As String
            Dim iHeaderMessage As String
            Dim iRightSideMessage As String
            Dim Adminui As New adminUIController(cpCore)
            '
            'If Not (true) Then Exit Function
            '
            iHeaderMessage = genericController.encodeText(HeaderMessage)
            iRightSideMessage = genericController.encodeEmptyText(RightSideMessage, FormatDateTime(cpCore.doc.profileStartTime))
            main_GetPanelHeader = Adminui.GetHeader(iHeaderMessage, iRightSideMessage)
        End Function

        '
        '========================================================================
        ' Prints the top of display panel
        '   Must be closed with PrintPanelBottom
        '========================================================================
        '
        Public Function main_GetPanelTop(Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "", Optional ByVal StyleShadow As String = "", Optional ByVal Width As String = "", Optional ByVal Padding As String = "", Optional ByVal HeightMin As String = "") As String
            Dim ContentPanelWidth As String
            Dim MethodName As String
            Dim MyStylePanel As String
            Dim MyStyleHilite As String
            Dim MyStyleShadow As String
            Dim MyWidth As String
            Dim MyPadding As String
            Dim MyHeightMin As String
            '
            main_GetPanelTop = ""
            MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel")
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite")
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow")
            MyWidth = genericController.encodeEmptyText(Width, "100%")
            MyPadding = genericController.encodeEmptyText(Padding, "5")
            MyHeightMin = genericController.encodeEmptyText(HeightMin, "1")
            MethodName = "main_GetPanelTop"
            If genericController.vbIsNumeric(MyWidth) Then
                ContentPanelWidth = (CInt(MyWidth) - 2).ToString
            Else
                ContentPanelWidth = "100%"
            End If
            main_GetPanelTop = main_GetPanelTop _
                & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""" & MyWidth & """ class=""" & MyStylePanel & """>"
            '
            ' --- top hilite row
            '
            main_GetPanelTop = main_GetPanelTop _
                & cr2 & "<tr>" _
                & cr3 & "<td colspan=""3"" class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                & cr2 & "</tr>"
            '
            ' --- center row with Panel
            '
            main_GetPanelTop = main_GetPanelTop _
                & cr2 & "<tr>" _
                & cr3 & "<td width=""1"" height=""" & MyHeightMin & """ class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""" & MyHeightMin & """ width=""1"" ></td>" _
                & cr3 & "<td width=""" & ContentPanelWidth & """ valign=""top"" align=""left"" class=""" & MyStylePanel & """>" _
                & cr4 & "<table border=""0"" cellpadding=""" & MyPadding & """ cellspacing=""0"" width=""" & ContentPanelWidth & """ class=""" & MyStylePanel & """>" _
                & cr5 & "<tr>" _
                & cr6 & "<td valign=""top"" class=""" & MyStylePanel & """><Span class=""" & MyStylePanel & """>"
        End Function
        '
        '========================================================================
        ' Return a panel with the input as center
        '========================================================================
        '
        Public Function main_GetPanelBottom(Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "", Optional ByVal StyleShadow As String = "", Optional ByVal Width As String = "", Optional ByVal Padding As String = "") As String
            Dim result As String = String.Empty
            Try
                'Dim MyStylePanel As String
                'Dim MyStyleHilite As String
                Dim MyStyleShadow As String
                Dim MyWidth As String
                'Dim MyPadding As String
                '
                'MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel")
                'MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite")
                MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow")
                MyWidth = genericController.encodeEmptyText(Width, "100%")
                'MyPadding = genericController.encodeEmptyText(Padding, "5")
                '
                result = result _
                    & cr6 & "</span></td>" _
                    & cr5 & "</tr>" _
                    & cr4 & "</table>" _
                    & cr3 & "</td>" _
                    & cr3 & "<td width=""1"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""1"" ></td>" _
                    & cr2 & "</tr>" _
                    & cr2 & "<tr>" _
                    & cr3 & "<td colspan=""3"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                    & cr2 & "</tr>" _
                    & cr & "</table>"
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function main_GetPanelButtons(ByVal ButtonValueList As String, ByVal ButtonName As String, Optional ByVal PanelWidth As String = "", Optional ByVal PanelHeightMin As String = "") As String
            Dim Adminui As New adminUIController(cpCore)
            main_GetPanelButtons = Adminui.GetButtonBar(Adminui.GetButtonsFromList(ButtonValueList, True, True, ButtonName), "")
        End Function
        '
        '
        '
        Public Function main_GetPanelRev(ByVal PanelContent As String, Optional ByVal PanelWidth As String = "", Optional ByVal PanelHeightMin As String = "") As String
            main_GetPanelRev = main_GetPanel(PanelContent, "ccPanel", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, genericController.EncodeInteger(PanelHeightMin))
        End Function
        '
        '
        '
        Public Function main_GetPanelInput(ByVal PanelContent As String, Optional ByVal PanelWidth As String = "", Optional ByVal PanelHeightMin As String = "1") As String
            main_GetPanelInput = main_GetPanel(PanelContent, "ccPanelInput", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, genericController.EncodeInteger(PanelHeightMin))
        End Function
        '
        '========================================================================
        ' Print the tools panel at the bottom of the page
        '========================================================================
        '
        Public Function main_GetToolsPanel() As String
            Dim result As String = String.Empty
            Try
                Dim copyNameValue As String
                Dim CopyName As String
                Dim copyValue As String
                Dim copyNameValueSplit() As String
                Dim VisitMin As Integer
                Dim VisitHrs As Integer
                Dim VisitSec As Integer
                Dim DebugPanel As String = String.Empty
                Dim Copy As String
                Dim CopySplit() As String
                Dim Ptr As Integer
                Dim EditTagID As String
                Dim QuickEditTagID As String
                Dim AdvancedEditTagID As String
                Dim WorkflowTagID As String
                Dim Tag As String
                Dim MethodName As String
                Dim TagID As String
                Dim ToolsPanel As stringBuilderLegacyController
                Dim OptionsPanel As String = String.Empty
                Dim LinkPanel As stringBuilderLegacyController
                Dim LoginPanel As String = String.Empty
                Dim iValueBoolean As Boolean
                Dim WorkingQueryString As String
                Dim BubbleCopy As String
                Dim AnotherPanel As stringBuilderLegacyController
                Dim Adminui As New adminUIController(cpCore)
                Dim ShowLegacyToolsPanel As Boolean
                Dim QS As String
                '
                MethodName = "main_GetToolsPanel"
                '
                If cpCore.doc.authContext.user.AllowToolsPanel Then
                    ShowLegacyToolsPanel = cpCore.siteProperties.getBoolean("AllowLegacyToolsPanel", True)
                    '
                    ' --- Link Panel - used for both Legacy Tools Panel, and without it
                    '
                    LinkPanel = New stringBuilderLegacyController
                    LinkPanel.Add(SpanClassAdminSmall)
                    LinkPanel.Add("Contensive " & cpCore.codeVersion() & " | ")
                    LinkPanel.Add(FormatDateTime(cpCore.doc.profileStartTime) & " | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""http://support.Contensive.com/"">Support</A> | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" href=""" & genericController.encodeHTML("/" & cpCore.serverConfig.appConfig.adminRoute) & """>Admin Home</A> | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" href=""" & genericController.encodeHTML("http://" & cpCore.webServer.requestDomain) & """>Public Home</A> | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""" & genericController.encodeHTML("/" & cpCore.serverConfig.appConfig.adminRoute & "?" & RequestNameHardCodedPage & "=" & HardCodedPageMyProfile) & """>My Profile</A> | ")
                    If cpCore.siteProperties.getBoolean("AllowMobileTemplates", False) Then
                        If cpCore.doc.authContext.visit.Mobile Then
                            QS = cpCore.doc.refreshQueryString
                            QS = genericController.ModifyQueryString(QS, "method", "forcenonmobile")
                            LinkPanel.Add("<a class=""ccAdminLink"" href=""?" & QS & """>Non-Mobile Version</A> | ")
                        Else
                            QS = cpCore.doc.refreshQueryString
                            QS = genericController.ModifyQueryString(QS, "method", "forcemobile")
                            LinkPanel.Add("<a class=""ccAdminLink"" href=""?" & QS & """>Mobile Version</A> | ")
                        End If
                    End If
                    LinkPanel.Add("</span>")
                    '
                    If ShowLegacyToolsPanel Then
                        ToolsPanel = New stringBuilderLegacyController
                        WorkingQueryString = genericController.ModifyQueryString(cpCore.doc.refreshQueryString, "ma", "", False)
                        '
                        ' ----- Tools Panel Caption
                        '
                        Dim helpLink As String
                        helpLink = ""
                        'helpLink = main_GetHelpLink("2", "Contensive Tools Panel", BubbleCopy)
                        BubbleCopy = "Use the Tools Panel to enable features such as editing and debugging tools. It also includes links to the admin site, the support site and the My Profile page."
                        result = result & main_GetPanelHeader("Contensive Tools Panel" & helpLink)
                        '
                        ToolsPanel.Add(cpCore.html.html_GetFormStart(WorkingQueryString))
                        ToolsPanel.Add(cpCore.html.html_GetFormInputHidden("Type", FormTypeToolsPanel))
                        '
                        If True Then
                            '
                            ' ----- Create the Options Panel
                            '
                            'PathsContentID = main_GetContentID("Paths")
                            '                '
                            '                ' Allow Help Links
                            '                '
                            '                iValueBoolean = visitProperty.getboolean("AllowHelpIcon")
                            '                TagID =  "AllowHelpIcon"
                            '                OptionsPanel = OptionsPanel & "" _
                            '                    & CR & "<div class=""ccAdminSmall"">" _
                            '                    & cr2 & "<LABEL for=""" & TagID & """>" & main_GetFormInputCheckBox2(TagID, iValueBoolean, TagID) & "&nbsp;Help</LABEL>" _
                            '                    & CR & "</div>"
                            '
                            EditTagID = "AllowEditing"
                            QuickEditTagID = "AllowQuickEditor"
                            AdvancedEditTagID = "AllowAdvancedEditor"
                            WorkflowTagID = "AllowWorkflowRendering"
                            '
                            ' Edit
                            '
                            helpLink = ""
                            'helpLink = main_GetHelpLink(7, "Enable Editing", "Display the edit tools for basic content, such as pages, copy and sections. ")
                            iValueBoolean = cpCore.visitProperty.getBoolean("AllowEditing")
                            Tag = cpCore.html.html_GetFormInputCheckBox2(EditTagID, iValueBoolean, EditTagID)
                            Tag = genericController.vbReplace(Tag, ">", " onClick=""document.getElementById('" & QuickEditTagID & "').checked=false;document.getElementById('" & AdvancedEditTagID & "').checked=false;"">")
                            OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & EditTagID & """>" & Tag & "&nbsp;Edit</LABEL>" & helpLink _
                            & cr & "</div>"
                            '
                            ' Quick Edit
                            '
                            helpLink = ""
                            'helpLink = main_GetHelpLink(8, "Enable Quick Edit", "Display the quick editor to edit the main page content.")
                            iValueBoolean = cpCore.visitProperty.getBoolean("AllowQuickEditor")
                            Tag = cpCore.html.html_GetFormInputCheckBox2(QuickEditTagID, iValueBoolean, QuickEditTagID)
                            Tag = genericController.vbReplace(Tag, ">", " onClick=""document.getElementById('" & EditTagID & "').checked=false;document.getElementById('" & AdvancedEditTagID & "').checked=false;"">")
                            OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & QuickEditTagID & """>" & Tag & "&nbsp;Quick Edit</LABEL>" & helpLink _
                            & cr & "</div>"
                            '
                            ' Advanced Edit
                            '
                            helpLink = ""
                            'helpLink = main_GetHelpLink(0, "Enable Advanced Edit", "Display the edit tools for advanced content, such as templates and add-ons. Basic content edit tools are also displayed.")
                            iValueBoolean = cpCore.visitProperty.getBoolean("AllowAdvancedEditor")
                            Tag = cpCore.html.html_GetFormInputCheckBox2(AdvancedEditTagID, iValueBoolean, AdvancedEditTagID)
                            Tag = genericController.vbReplace(Tag, ">", " onClick=""document.getElementById('" & QuickEditTagID & "').checked=false;document.getElementById('" & EditTagID & "').checked=false;"">")
                            OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & AdvancedEditTagID & """>" & Tag & "&nbsp;Advanced Edit</LABEL>" & helpLink _
                            & cr & "</div>"
                            '
                            ' Workflow Authoring Render Mode
                            '
                            helpLink = ""
                            'helpLink = main_GetHelpLink(9, "Enable Workflow Rendering", "Control the display of workflow rendering. With workflow rendering enabled, any changes saved to content records that have not been published will be visible for your review.")
                            'If cpCore.siteProperties.allowWorkflowAuthoring Then
                            '    iValueBoolean = cpCore.visitProperty.getBoolean("AllowWorkflowRendering")
                            '    Tag = cpCore.html.html_GetFormInputCheckBox2(WorkflowTagID, iValueBoolean, WorkflowTagID)
                            '    OptionsPanel = OptionsPanel _
                            '    & cr & "<div class=""ccAdminSmall"">" _
                            '    & cr2 & "<LABEL for=""" & WorkflowTagID & """>" & Tag & "&nbsp;Render Workflow Authoring Changes</LABEL>" & helpLink _
                            '    & cr & "</div>"
                            'End If
                            helpLink = ""
                            iValueBoolean = cpCore.visitProperty.getBoolean("AllowDebugging")
                            TagID = "AllowDebugging"
                            Tag = cpCore.html.html_GetFormInputCheckBox2(TagID, iValueBoolean, TagID)
                            OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & TagID & """>" & Tag & "&nbsp;Debug</LABEL>" & helpLink _
                            & cr & "</div>"
                            ''
                            '' Create Path Block Row
                            ''
                            'If cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore) Then
                            '    TagID = "CreatePathBlock"
                            '    If cpCore.siteProperties.allowPathBlocking Then
                            '        '
                            '        ' Path blocking allowed
                            '        '
                            '        'OptionsPanel = OptionsPanel & SpanClassAdminSmall & "<LABEL for=""" & TagID & """>"
                            '        CS = cpCore.db.cs_open("Paths", "name=" & cpCore.db.encodeSQLText(cpCore.webServer.requestPath), , , , , , "ID")
                            '        If cpCore.db.cs_ok(CS) Then
                            '            PathID = (cpCore.db.cs_getInteger(CS, "ID"))
                            '        End If
                            '        Call cpCore.db.cs_Close(CS)
                            '        If PathID <> 0 Then
                            '            '
                            '            ' Path is blocked
                            '            '
                            '            Tag = cpCore.html.html_GetFormInputCheckBox2(TagID, True, TagID) & "&nbsp;Path is blocked [" & cpCore.webServer.requestPath & "] [<a href=""" & genericController.encodeHTML("/" & cpCore.serverconfig.appconfig.adminRoute & "?af=" & AdminFormEdit & "&id=" & PathID & "&cid=" & models.complex.cdefmodel.getcontentid(cpcore,"paths") & "&ad=1") & """ target=""_blank"">edit</a>]</LABEL>"
                            '        Else
                            '            '
                            '            ' Path is not blocked
                            '            '
                            '            Tag = cpCore.html.html_GetFormInputCheckBox2(TagID, False, TagID) & "&nbsp;Block this path [" & cpCore.webServer.requestPath & "]</LABEL>"
                            '        End If
                            '        helpLink = ""
                            '        'helpLink = main_GetHelpLink(10, "Enable Debugging", "Debugging is a developer only debugging tool. With Debugging enabled, ccLib.TestPoints(...) will print, ErrorTrapping will be displayed, redirections are blocked, and more.")
                            '        OptionsPanel = OptionsPanel _
                            '        & cr & "<div class=""ccAdminSmall"">" _
                            '        & cr2 & "<LABEL for=""" & TagID & """>" & Tag & "</LABEL>" & helpLink _
                            '        & cr & "</div>"
                            '    End If
                            'End If
                            '
                            ' Buttons
                            '
                            OptionsPanel = OptionsPanel & "" _
                            & cr & "<div class=""ccButtonCon"">" _
                            & cr2 & "<input type=submit name=" & "mb value=""" & ButtonApply & """>" _
                            & cr & "</div>" _
                            & ""
                        End If
                        '
                        ' ----- Create the Login Panel
                        '
                        If Trim(cpCore.doc.authContext.user.name) = "" Then
                            Copy = "You are logged in as member #" & cpCore.doc.authContext.user.id & "."
                        Else
                            Copy = "You are logged in as " & cpCore.doc.authContext.user.name & "."
                        End If
                        LoginPanel = LoginPanel & "" _
                        & cr & "<div class=""ccAdminSmall"">" _
                        & cr2 & Copy & "" _
                        & cr & "</div>"
                        '
                        ' Username
                        '
                        Dim Caption As String
                        If cpCore.siteProperties.getBoolean("allowEmailLogin", False) Then
                            Caption = "Username&nbsp;or&nbsp;Email"
                        Else
                            Caption = "Username"
                        End If
                        TagID = "Username"
                        LoginPanel = LoginPanel & "" _
                        & cr & "<div class=""ccAdminSmall"">" _
                        & cr2 & "<LABEL for=""" & TagID & """>" & cpCore.html.html_GetFormInputText2(TagID, "", 1, 30, TagID, False) & "&nbsp;" & Caption & "</LABEL>" _
                        & cr & "</div>"
                        '
                        ' Username
                        '
                        If cpCore.siteProperties.getBoolean("allownopasswordLogin", False) Then
                            Caption = "Password&nbsp;(optional)"
                        Else
                            Caption = "Password"
                        End If
                        TagID = "Password"
                        LoginPanel = LoginPanel & "" _
                        & cr & "<div class=""ccAdminSmall"">" _
                        & cr2 & "<LABEL for=""" & TagID & """>" & cpCore.html.html_GetFormInputText2(TagID, "", 1, 30, TagID, True) & "&nbsp;" & Caption & "</LABEL>" _
                        & cr & "</div>"
                        '
                        ' Autologin checkbox
                        '
                        If cpCore.siteProperties.getBoolean("AllowAutoLogin", False) Then
                            If cpCore.doc.authContext.visit.CookieSupport Then
                                TagID = "autologin"
                                LoginPanel = LoginPanel & "" _
                                & cr & "<div class=""ccAdminSmall"">" _
                                & cr2 & "<LABEL for=""" & TagID & """>" & cpCore.html.html_GetFormInputCheckBox2(TagID, True, TagID) & "&nbsp;Login automatically from this computer</LABEL>" _
                                & cr & "</div>"
                            End If
                        End If
                        '
                        ' Buttons
                        '
                        LoginPanel = LoginPanel & Adminui.GetButtonBar(Adminui.GetButtonsFromList(ButtonLogin & "," & ButtonLogout, True, True, "mb"), "")
                        '
                        ' ----- assemble tools panel
                        '
                        Copy = "" _
                        & cr & "<td width=""50%"" class=""ccPanelInput"" style=""vertical-align:bottom;"">" _
                        & genericController.htmlIndent(LoginPanel) _
                        & cr & "</td>" _
                        & cr & "<td width=""50%"" class=""ccPanelInput"" style=""vertical-align:bottom;"">" _
                        & genericController.htmlIndent(OptionsPanel) _
                        & cr & "</td>"
                        Copy = "" _
                        & cr & "<tr>" _
                        & genericController.htmlIndent(Copy) _
                        & cr & "</tr>" _
                        & ""
                        Copy = "" _
                        & cr & "<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%"">" _
                        & genericController.htmlIndent(Copy) _
                        & cr & "</table>"
                        ToolsPanel.Add(main_GetPanelInput(Copy))
                        ToolsPanel.Add(cpCore.html.html_GetFormEnd)
                        result = result & main_GetPanel(ToolsPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                        '
                        result = result & main_GetPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                        '
                        LinkPanel = Nothing
                        ToolsPanel = Nothing
                        AnotherPanel = Nothing
                    End If
                    '
                    ' --- Developer Debug Panel
                    '
                    If cpCore.visitProperty.getBoolean("AllowDebugging") Then
                        '
                        ' --- Debug Panel Header
                        '
                        LinkPanel = New stringBuilderLegacyController
                        LinkPanel.Add(SpanClassAdminSmall)
                        'LinkPanel.Add( "WebClient " & main_WebClientVersion & " | "
                        LinkPanel.Add("Contensive " & cpCore.codeVersion() & " | ")
                        LinkPanel.Add(FormatDateTime(cpCore.doc.profileStartTime) & " | ")
                        LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""http: //support.Contensive.com/"">Support</A> | ")
                        LinkPanel.Add("<a class=""ccAdminLink"" href=""" & genericController.encodeHTML("/" & cpCore.serverConfig.appConfig.adminRoute) & """>Admin Home</A> | ")
                        LinkPanel.Add("<a class=""ccAdminLink"" href=""" & genericController.encodeHTML("http://" & cpCore.webServer.requestDomain) & """>Public Home</A> | ")
                        LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""" & genericController.encodeHTML("/" & cpCore.serverConfig.appConfig.adminRoute & "?" & RequestNameHardCodedPage & "=" & HardCodedPageMyProfile) & """>My Profile</A> | ")
                        LinkPanel.Add("</span>")
                        '
                        '
                        '
                        'DebugPanel = DebugPanel & main_GetPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", "5")
                        '
                        DebugPanel = DebugPanel _
                        & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">" _
                        & cr2 & "<tr>" _
                        & cr3 & "<td width=""100"" class=""ccPanel""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100"" height=""1"" ></td>" _
                        & cr3 & "<td width=""100%"" class=""ccPanel""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td>" _
                        & cr2 & "</tr>"
                        '
                        DebugPanel = DebugPanel & getDebugPanelRow("DOM", "<a class=""ccAdminLink"" href=""/ccLib/clientside/DOMViewer.htm"" target=""_blank"">Click</A>")
                        DebugPanel = DebugPanel & getDebugPanelRow("Trap Errors", genericController.encodeHTML(cpCore.siteProperties.trapErrors.ToString))
                        DebugPanel = DebugPanel & getDebugPanelRow("Trap Email", genericController.encodeHTML(cpCore.siteProperties.getText("TrapEmail")))
                        DebugPanel = DebugPanel & getDebugPanelRow("main_ServerLink", genericController.encodeHTML(cpCore.webServer.requestUrl))
                        DebugPanel = DebugPanel & getDebugPanelRow("main_ServerDomain", genericController.encodeHTML(cpCore.webServer.requestDomain))
                        DebugPanel = DebugPanel & getDebugPanelRow("main_ServerProtocol", genericController.encodeHTML(cpCore.webServer.requestProtocol))
                        DebugPanel = DebugPanel & getDebugPanelRow("main_ServerHost", genericController.encodeHTML(cpCore.webServer.requestDomain))
                        DebugPanel = DebugPanel & getDebugPanelRow("main_ServerPath", genericController.encodeHTML(cpCore.webServer.requestPath))
                        DebugPanel = DebugPanel & getDebugPanelRow("main_ServerPage", genericController.encodeHTML(cpCore.webServer.requestPage))
                        Copy = ""
                        If cpCore.webServer.requestQueryString <> "" Then
                            CopySplit = Split(cpCore.webServer.requestQueryString, "&")
                            For Ptr = 0 To UBound(CopySplit)
                                copyNameValue = CopySplit(Ptr)
                                If copyNameValue <> "" Then
                                    copyNameValueSplit = Split(copyNameValue, "=")
                                    CopyName = genericController.DecodeResponseVariable(copyNameValueSplit(0))
                                    copyValue = ""
                                    If UBound(copyNameValueSplit) > 0 Then
                                        copyValue = genericController.DecodeResponseVariable(copyNameValueSplit(1))
                                    End If
                                    Copy = Copy & cr & "<br>" & genericController.encodeHTML(CopyName & "=" & copyValue)
                                End If
                            Next
                            Copy = Mid(Copy, 8)
                        End If
                        DebugPanel = DebugPanel & getDebugPanelRow("main_ServerQueryString", Copy)
                        Copy = ""
                        For Each key As String In cpCore.docProperties.getKeyList()
                            Dim docProperty As docPropertiesClass = cpCore.docProperties.getProperty(key)
                            If docProperty.IsForm Then
                                Copy = Copy & cr & "<br>" & genericController.encodeHTML(docProperty.NameValue)
                            End If
                        Next
                        DebugPanel = DebugPanel & getDebugPanelRow("Render Time &gt;= ", Format((cpCore.doc.appStopWatch.ElapsedMilliseconds) / 1000, "0.000") & " sec")
                        If True Then
                            VisitHrs = CInt(cpCore.doc.authContext.visit.TimeToLastHit / 3600)
                            VisitMin = CInt(cpCore.doc.authContext.visit.TimeToLastHit / 60) - (60 * VisitHrs)
                            VisitSec = cpCore.doc.authContext.visit.TimeToLastHit Mod 60
                            DebugPanel = DebugPanel & getDebugPanelRow("Visit Length", CStr(cpCore.doc.authContext.visit.TimeToLastHit) & " sec, (" & VisitHrs & " hrs " & VisitMin & " mins " & VisitSec & " secs)")
                            'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Length", CStr(main_VisitTimeToLastHit) & " sec, (" & Int(main_VisitTimeToLastHit / 60) & " min " & (main_VisitTimeToLastHit Mod 60) & " sec)")
                        End If
                        DebugPanel = DebugPanel & getDebugPanelRow("Addon Profile", "<hr><ul class=""ccPanel"">" & "<li>tbd</li>" & cr & "</ul>")
                        '
                        DebugPanel = DebugPanel & "</table>"
                        '
                        If ShowLegacyToolsPanel Then
                            '
                            ' Debug Panel as part of legacy tools panel
                            '
                            result = result _
                            & main_GetPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                        Else
                            '
                            ' Debug Panel without Legacy Tools panel
                            '
                            result = result _
                            & main_GetPanelHeader("Debug Panel") _
                            & main_GetPanel(LinkPanel.Text) _
                            & main_GetPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                        End If
                    End If
                    result = cr & "<div class=""ccCon"">" & genericController.htmlIndent(result) & cr & "</div>"
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '
        '
        Private Function getDebugPanelRow(Label As String, Value As String) As String
            Return cr2 & "<tr><td valign=""top"" class=""ccPanel ccAdminSmall"">" & Label & "</td><td valign=""top"" class=""ccPanel ccAdminSmall"">" & Value & "</td></tr>"
        End Function

        '
        '=================================================================================================================
        '   csv_GetAddonOptionStringValue
        '
        '   gets the value from a list matching the name
        '
        '   InstanceOptionstring is an "AddonEncoded" name=AddonEncodedValue[selector]descriptor&name=value string
        '=================================================================================================================
        '
        Public Shared Function getAddonOptionStringValue(OptionName As String, addonOptionString As String) As String
            Dim result As String = genericController.getSimpleNameValue(OptionName, addonOptionString, "", "&")
            Dim Pos As Integer = genericController.vbInstr(1, result, "[")
            If Pos > 0 Then
                result = Left(result, Pos - 1)
            End If
            Return Trim(genericController.decodeNvaArgument(result))
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Create the full html doc from the accumulated elements
        ''' </summary>
        ''' <param name="htmlBody"></param>
        ''' <param name="htmlBodyTag"></param>
        ''' <param name="allowLogin"></param>
        ''' <param name="allowTools"></param>
        ''' <param name="blockNonContentExtras"></param>
        ''' <param name="isAdminSite"></param>
        ''' <returns></returns>
        Public Function getHtmlDoc(htmlBody As String, htmlBodyTag As String, Optional allowLogin As Boolean = True, Optional allowTools As Boolean = True) As String
            Dim result As String = ""
            Try
                Dim htmlHead As String = getHtmlHead()
                Dim htmlBeforeEndOfBody As String = getHtmlDoc_beforeEndOfBodyHtml(allowLogin, allowTools)

                result = "" _
                    & cpCore.siteProperties.docTypeDeclaration _
                    & vbCrLf & "<html>" _
                    & vbCrLf & "<head>" _
                    & htmlHead _
                    & vbCrLf & "</head>" _
                    & vbCrLf & htmlBodyTag _
                    & htmlBody _
                    & htmlBeforeEndOfBody _
                    & vbCrLf & "</body>" _
                    & vbCrLf & "</html>" _
                    & ""
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        ''
        '' assemble all the html parts
        ''
        'Public Function assembleHtmlDoc(ByVal head As String, ByVal bodyTag As String, ByVal Body As String) As String
        '    Return "" _
        '        & cpCore.siteProperties.docTypeDeclarationAdmin _
        '        & cr & "<html>" _
        '        & cr2 & "<head>" _
        '        & genericController.htmlIndent(head) _
        '        & cr2 & "</head>" _
        '        & cr2 & bodyTag _
        '        & genericController.htmlIndent(Body) _
        '        & cr2 & "</body>" _
        '        & cr & "</html>"
        'End Function
        ''
        ''========================================================================
        '' ----- Starts an HTML page (for an admin page -- not a public page)
        ''========================================================================
        ''
        'Public Function getHtmlDoc_beforeBodyHtml(Optional ByVal Title As String = "", Optional ByVal PageMargin As Integer = 0) As String
        '    If Title <> "" Then
        '        Call main_AddPagetitle(Title)
        '    End If
        '    If main_MetaContent_Title = "" Then
        '        Call main_AddPagetitle("Admin-" & cpCore.webServer.webServerIO_requestDomain)
        '    End If
        '    cpCore.webServer.webServerIO_response_NoFollow = True
        '    Call main_SetMetaContent(0, 0)
        '    '
        '    Return "" _
        '        & cpCore.siteProperties.docTypeDeclarationAdmin _
        '        & vbCrLf & "<html>" _
        '        & vbCrLf & "<head>" _
        '        & getHTMLInternalHead(True) _
        '        & vbCrLf & "</head>" _
        '        & vbCrLf & "<body class=""ccBodyAdmin ccCon"">"
        'End Function

        '
        '====================================================================================================
        ''' <summary>
        ''' legacy compatibility
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="ButtonList"></param>
        ''' <returns></returns>
        Public Shared Function legacy_closeFormTable(cpCore As coreClass, ByVal ButtonList As String) As String
            If ButtonList <> "" Then
                legacy_closeFormTable = "</td></tr></TABLE>" & cpCore.html.main_GetPanelButtons(ButtonList, "Button") & "</form>"
            Else
                legacy_closeFormTable = "</td></tr></TABLE></form>"
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' legacy compatibility
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="ButtonList"></param>
        ''' <returns></returns>
        Public Shared Function legacy_openFormTable(cpCore As coreClass, ByVal ButtonList As String) As String
            Dim result As String = ""
            Try
                result = cpCore.html.html_GetFormStart()
                If ButtonList <> "" Then
                    result = result & cpCore.html.main_GetPanelButtons(ButtonList, "Button")
                End If
                result = result & "<table border=""0"" cellpadding=""10"" cellspacing=""0"" width=""100%""><tr><TD>"
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Function getHtmlHead() As String
            Dim headList As New List(Of String)
            Try
                '
                ' -- meta content
                If (cpCore.doc.htmlMetaContent_TitleList.Count > 0) Then
                    Dim content As String = ""
                    For Each asset In cpCore.doc.htmlMetaContent_TitleList
                        If (cpCore.doc.visitPropertyAllowDebugging) And (Not String.IsNullOrEmpty(asset.addedByMessage)) Then
                            headList.Add("<!-- added by " & asset.addedByMessage & " -->")
                        End If
                        content &= " | " & asset.content
                    Next
                    headList.Add("<title>" & encodeHTML(content.Substring(3)) & "</title>")
                End If
                If (cpCore.doc.htmlMetaContent_KeyWordList.Count > 0) Then
                    Dim content As String = ""
                    For Each asset In cpCore.doc.htmlMetaContent_KeyWordList.FindAll(Function(a) (Not String.IsNullOrEmpty(a.content)))
                        If (cpCore.doc.visitPropertyAllowDebugging) And (Not String.IsNullOrEmpty(asset.addedByMessage)) Then
                            headList.Add("<!-- '" & encodeHTML(asset.content & "' added by " & asset.addedByMessage) & " -->")
                        End If
                        content &= "," & asset.content
                    Next
                    If (Not String.IsNullOrEmpty(content)) Then
                        headList.Add("<meta name=""keywords"" content=""" & encodeHTML(content.Substring(1)) & """ >")
                    End If
                End If
                If (cpCore.doc.htmlMetaContent_Description.Count > 0) Then
                    Dim content As String = ""
                    For Each asset In cpCore.doc.htmlMetaContent_Description
                        If (cpCore.doc.visitPropertyAllowDebugging) And (Not String.IsNullOrEmpty(asset.addedByMessage)) Then
                            headList.Add("<!-- '" & encodeHTML(asset.content & "' added by " & asset.addedByMessage) & " -->")
                        End If
                        content &= "," & asset.content
                    Next
                    headList.Add("<meta name=""description"" content=""" & encodeHTML(content.Substring(1)) & """ >")
                End If
                '
                ' -- favicon
                Dim VirtualFilename As String = cpCore.siteProperties.getText("faviconfilename")
                Select Case IO.Path.GetExtension(VirtualFilename).ToLower
                    Case ".ico"
                        headList.Add("<link rel=""icon"" type=""image/vnd.microsoft.icon"" href=""" & genericController.getCdnFileLink(cpCore, VirtualFilename) & """ >")
                    Case ".png"
                        headList.Add("<link rel=""icon"" type=""image/png"" href=""" & genericController.getCdnFileLink(cpCore, VirtualFilename) & """ >")
                    Case ".gif"
                        headList.Add("<link rel=""icon"" type=""image/gif"" href=""" & genericController.getCdnFileLink(cpCore, VirtualFilename) & """ >")
                    Case ".jpg"
                        headList.Add("<link rel=""icon"" type=""image/jpg"" href=""" & genericController.getCdnFileLink(cpCore, VirtualFilename) & """ >")
                End Select
                '
                ' -- misc caching, etc
                Dim encoding As String = genericController.encodeHTML(cpCore.siteProperties.getText("Site Character Encoding", "utf-8"))
                headList.Add("<meta http-equiv=""content-type"" content=""text/html; charset=" & encoding & """>")
                headList.Add("<meta http-equiv=""content-language"" content=""en-us"">")
                headList.Add("<meta http-equiv=""cache-control"" content=""no-cache"">")
                headList.Add("<meta http-equiv=""expires"" content=""-1"">")
                headList.Add("<meta http-equiv=""pragma"" content=""no-cache"">")
                headList.Add("<meta name=""generator"" content=""Contensive"">")
                '
                ' -- no-follow
                If cpCore.webServer.response_NoFollow Then
                    headList.Add("<meta name=""robots"" content=""nofollow"" >")
                    headList.Add("<meta name=""mssmarttagspreventparsing"" content=""true"" >")
                End If
                '
                ' -- base is needed for Link Alias case where a slash is in the URL (page named 1/2/3/4/5)
                If (Not String.IsNullOrEmpty(cpCore.webServer.serverFormActionURL)) Then
                    Dim BaseHref As String = cpCore.webServer.serverFormActionURL
                    If (Not String.IsNullOrEmpty(cpCore.doc.refreshQueryString)) Then
                        BaseHref &= "?" & cpCore.doc.refreshQueryString
                    End If
                    headList.Add("<base href=""" & BaseHref & """ >")
                End If
                '
                If (cpCore.doc.htmlAssetList.Count > 0) Then
                    Dim scriptList As New List(Of String)
                    Dim styleList As New List(Of String)
                    For Each asset In cpCore.doc.htmlAssetList.FindAll(Function(item As htmlAssetClass) (item.inHead))
                        If (cpCore.doc.allowDebugLog) Then
                            If (cpCore.doc.visitPropertyAllowDebugging) And (Not String.IsNullOrEmpty(asset.addedByMessage)) Then
                                headList.Add("<!-- '" & encodeHTML(asset.content & "' added by " & asset.addedByMessage) & " -->")
                            End If
                        End If
                        If asset.assetType.Equals(htmlAssetTypeEnum.style) Then
                            If asset.isLink Then
                                styleList.Add("<link rel=""stylesheet"" type=""text/css"" href=""" & asset.content & """ >")
                            Else
                                styleList.Add("<style>" & asset.content & "</style>")
                            End If
                        ElseIf asset.assetType.Equals(htmlAssetTypeEnum.script) Then

                            If asset.isLink Then
                                scriptList.Add("<script type=""text/javascript"" src=""" & asset.content & """></script>")
                            Else
                                scriptList.Add("<script type=""text/javascript"">" & asset.content & "</script>")
                            End If
                        End If
                    Next
                    headList.AddRange(styleList)
                    headList.AddRange(scriptList)
                End If
                '
                ' -- other head tags - always last
                For Each asset In cpCore.doc.htmlMetaContent_OtherTags.FindAll(Function(a) (Not String.IsNullOrEmpty(a.content)))
                    If (cpCore.doc.allowDebugLog) Then
                        If (cpCore.doc.visitPropertyAllowDebugging) And (Not String.IsNullOrEmpty(asset.addedByMessage)) Then
                            headList.Add("<!-- '" & encodeHTML(asset.content & "' added by " & asset.addedByMessage) & " -->")
                        End If
                    End If
                    headList.Add(asset.content)
                Next
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return String.Join(cr, headList)
        End Function

    End Class
End Namespace
