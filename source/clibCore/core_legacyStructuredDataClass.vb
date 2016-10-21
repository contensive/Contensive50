
Imports Contensive.BaseClasses
Imports Contensive.Core.ccCommonModule

'
' just the document -- replace out 
' if 'Imports Interop.adodb, replace in ObjectStateEnum.adState...
' findreplace encode to encode
' findreplace ''DoEvents to '''DoEvents
' runProcess becomes runProcess
' Sleep becomes Threading.Thread.Sleep(
' as object to as object

Namespace Contensive.Core
    Public Class core_primitivesStructuredDataClass
        '
        Private cpCore As cpCoreClass
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As cpCoreClass)
            Me.cpCore = cpCore
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' get site structure
        ''' </summary>
        ''' <param name="IsWorkflowRendering"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetSiteStructure(IsWorkflowRendering As Boolean) As String
            Dim returnValue As String = ""
            Try
                Dim PCC As Object
                '
                PCC = cpCore.cache_pageContent_get(False, False)
                '
                returnValue = "" _
                    & vbCrLf & vbTab & "<sitestructure version=""" & cpCore.version & """>" _
                    & kmaIndent(GetMenusNode(IsWorkflowRendering, PCC, "")) _
                    & kmaIndent(GetSectionsNode(IsWorkflowRendering, PCC, "")) _
                    & kmaIndent(GetPagesNode(IsWorkflowRendering, PCC, "")) _
                    & vbCrLf & vbTab & "</sitestructure>"
                '
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' GetMenusNode
        ''' </summary>
        ''' <param name="IsWorkflowRendering"></param>
        ''' <param name="PCC"></param>
        ''' <param name="BuildVersion"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetMenusNode(IsWorkflowRendering As Boolean, PCC As Object, BuildVersion As String) As String
            Dim returnValue As String = ""
            Try
                returnValue = "" _
                    & vbCrLf & vbTab & "<menus>" _
                    & kmaIndent(GetMenuNodes(IsWorkflowRendering, PCC, BuildVersion)) _
                    & vbCrLf & vbTab & "</menus>"
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Get the Menu Node
        ''' </summary>
        ''' <param name="IsWorkflowRendering"></param>
        ''' <param name="PCC"></param>
        ''' <param name="BuildVersion"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function GetMenuNodes(IsWorkflowRendering As Boolean, PCC As Object, BuildVersion As String) As String
            Dim returnValue As String = ""
            Try
                Dim SubNodes As String
                Dim DefaultTemplateID As Integer
                Dim IsEditingMenus As Boolean
                Dim PageActive As Boolean
                Dim TCPtr As Integer
                Dim PCCPtr As Integer
                Dim RootPageID As Integer
                Dim CSMenus As CPCSBaseClass
                Dim CSTemplates As CPCSBaseClass
                Dim CSPage As CPCSBaseClass
                Dim MenuName As String
                Dim TemplateID As Integer
                Dim ContentID As Integer
                Dim ContentName As String
                Dim PageList_ParentBranchPointer As Integer
                Dim Link As String
                Dim MenuID As Integer
                Dim AuthoringTag As String
                Dim MenuImage As String
                Dim MenuImageOver As String
                Dim LandingLink As String
                Dim MenuString As String
                Dim MenuCaption As String
                Dim MenuTemplateID As Integer
                Dim Criteria As String
                Dim SelectFieldList As String
                Dim ShowHiddenMenu As Boolean
                Dim HideMenu As Boolean
                Dim PageContentCID As Integer
                Dim BlockPage As Boolean
                Dim BlockMenu As Boolean
                Dim SQL As String
                Dim IsAllMenusMenuMode As Boolean
                Dim CS As CPCSBaseClass
                '
                '
                SelectFieldList = "ID, Name,Depth,Layout,Delimiter,FlyoutOnHover,FlyoutDirection,StylePrefix,StylesFilename"
                CSMenus.open("Dynamic Menus", , , , , , SelectFieldList)
                Do While CSMenus.OK()
                    MenuID = CSMenus.GetInteger("ID")
                    If True Then
                        MenuName = Trim(CSMenus.GetText("Name"))
                        If MenuName = "" Then
                            MenuName = "Menu " & MenuID
                            Call cpCore.app.executeSql("default", "update ccMenus set Name=" & EncodeSQLText(MenuName) & " where ID=" & MenuID)
                        End If
                        '
                        ' Get MenuSection Nodes
                        '
                        SubNodes = ""
                        CS.Open("Dynamic Menu Section Rules", "(DynamicMenuID=" & MenuID & ")and(sectionid is not null)", , , , , "SectionID")
                        Do While CS.OK()
                            '
                            SubNodes = SubNodes & vbCrLf & vbTab & "<menusection sectionid=""" & cs.getInteger("sectionid") & """/>"
                            cs.goNext()
                        Loop
                        '
                        ' Get Menu, remove crlf, and parse the line with crlf
                        '
                        returnValue &= "" _
                            & vbCrLf & vbTab & "<menu" _
                            & " id=""m" & MenuID & """" _
                            & " menuid=""" & MenuID & """" _
                            & " name=""" & MenuName & """" _
                            & " depth=""" & csMenus.getInteger( "depth") & """" _
                            & " layout=""" & csMenus.getInteger( "Layout") & """" _
                            & " delimiter=""" & encodeHTML(csMenus.getText( "Delimiter")) & """" _
                            & " flyoutonhover=""" & CSMenus.GetBoolean( "FlyoutOnHover") & """" _
                            & " flyoutdirection=""" & csMenus.getInteger( "FlyoutDirection") & """" _
                            & " styleprefix=""" & encodeHTML(csMenus.getText( "StylePrefix")) & """" _
                            & " stylesfilename=""" & encodeHTML(csMenus.getText( "StylesFilename")) & """" _
                            & ""
                        If SubNodes <> "" Then
                            returnValue = returnValue & ">" _
                                & kmaIndent(SubNodes) _
                                & vbCrLf & vbTab & "</menu>"
                        Else
                            returnValue = returnValue & "/>"
                        End If
                    End If
                    Call CSMenus.GoNext()
                Loop
                Call CSMenus.Close()
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' GetSectionsNode
        ''' </summary>
        ''' <param name="IsWorkflowRendering"></param>
        ''' <param name="PCC"></param>
        ''' <param name="BuildVersion"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetSectionsNode(IsWorkflowRendering As Boolean, PCC As Object, BuildVersion As String) As String
            Dim returnValue As String = ""
            Try
                returnValue = "" _
                    & vbCrLf & vbTab & "<sections>" _
                    & kmaIndent(GetSectionNodes(IsWorkflowRendering, PCC, BuildVersion)) _
                    & vbCrLf & vbTab & "</sections>"
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Get the Section Menu  - MenuName blank reverse menu to legacy mode (all sections on menu)
        ''' </summary>
        ''' <param name="IsWorkflowRendering"></param>
        ''' <param name="PCC"></param>
        ''' <param name="BuildVersion"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Function GetSectionNodes(IsWorkflowRendering As Boolean, PCC As Object, BuildVersion As String) As String
            Dim returnValue As String = ""
            Try
                Dim Ptr As Integer
                Dim Rows2 As Object
                Dim Rows() As String
                Dim GroupIDAccessList As String
                Dim CS As CPCSBaseClass
                Dim SubNodes As String
                Dim DefaultTemplateID As Integer
                Dim IsEditingSections As Boolean
                Dim PageActive As Boolean
                Dim TCPtr As Integer
                Dim PCCPtr As Integer
                Dim RootPageID As Integer
                Dim CSSections As CPCSBaseClass
                Dim CSTemplates As CPCSBaseClass
                Dim CSPage As CPCSBaseClass
                Dim SectionName As String
                Dim TemplateID As Integer
                Dim ContentID As Integer
                'Dim ContentName As String
                Dim PageList_ParentBranchPointer As Integer
                Dim Link As String
                Dim SectionID As Integer
                Dim AuthoringTag As String
                Dim MenuImage As String
                Dim MenuImageOver As String
                Dim LandingLink As String
                Dim MenuString As String
                Dim SectionCaption As String
                'Dim SectionTemplateID as integer
                Dim Criteria As String
                Dim SelectFieldList As String
                Dim ShowHiddenMenu As Boolean
                Dim HideMenu As Boolean
                Dim PageContentCID As Integer
                Dim BlockPage As Boolean
                Dim BlockSection As Boolean
                Dim SQL As String
                Dim IsAllSectionsMenuMode As Boolean
                '
                '
                PageContentCID = cpCore.csv_GetContentID("Page Content")
                SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection,RootPageID"
                ShowHiddenMenu = cpCore.user_isEditingAnything()
                CSSections.Open("Site Sections", , , , , , SelectFieldList)
                Do While CSSections.OK()
                    HideMenu = CSSections.GetBoolean("HideMenu")
                    BlockSection = CSSections.GetBoolean("BlockSection")
                    SectionID = CSSections.GetInteger("ID")
                    SectionName = Trim(CSSections.GetText("Name"))
                    If SectionName = "" Then
                        SectionName = "Section " & SectionID
                    End If
                    SectionCaption = CSSections.GetText("Caption")
                    If SectionCaption = "" Then
                        SectionCaption = SectionName
                    End If
                    GroupIDAccessList = ""
                    If BlockSection Then
                        If CS.OpenSQL("select groupid from ccSectionBlockRules where sectionid=" & SectionID) Then
                            Do
                                GroupIDAccessList &= "," & Rows2(0, Ptr)
                                CS.GoNext()
                            Loop While CS.OK
                        End If
                        CS.Close()
                    End If
                    MenuImage = CSSections.GetText("MenuImageFilename")
                    If MenuImage <> "" Then
                        MenuImage = cpCore.app.config.cdnFilesNetprefix & MenuImage
                    End If
                    MenuImageOver = CSSections.GetText("MenuImageOverFilename")
                    If MenuImageOver <> "" Then
                        MenuImageOver = cpCore.app.config.cdnFilesNetprefix & MenuImageOver
                    End If
                    '
                    ' Get Root Page for templateID
                    '
                    TemplateID = 0
                    BlockPage = False
                    Link = ""
                    RootPageID = CSSections.GetInteger("rootpageid")
                    PCCPtr = cpCore.cache_pageContent_getPtr(RootPageID, IsWorkflowRendering, False)
                    If PCCPtr < 0 Then
                        RootPageID = 0
                    End If
                    '
                    ' Get MenuSection Nodes
                    '
                    SubNodes = ""
                    CS.Open("Dynamic Menu Section Rules", "(SectionID=" & SectionID & ")and(DynamicMenuID is not null)", , , , , "DynamicMenuID")
                    Do While CS.OK()
                        '
                        SubNodes = SubNodes & vbCrLf & vbTab & "<menusection menuid=""" & CS.GetInteger("DynamicMenuID") & """/>"
                        CS.GoNext()
                    Loop
                    '
                    ' Get Menu, remove crlf, and parse the line with crlf
                    '
                    'SubNodes = GetPageNode(RootPageID, Link, 99, 0, "", "", "", SectionCaption, SectionID, False, BuildVersion, PCC, IsWorkflowRendering)
                    GetSectionNodes = GetSectionNodes _
                        & vbCrLf & vbTab & "<section" _
                        & " id=""s" & SectionID & """" _
                        & " sectionid=""" & SectionID & """" _
                        & " name=""" & SectionName & """" _
                        & " caption=""" & SectionCaption & """" _
                        & " hide=""" & HideMenu & """" _
                        & " block=""" & BlockSection & """" _
                        & " groupaccesslist=""" & GroupIDAccessList & """" _
                        & " menuimage=""" & EncodeHTML(MenuImage) & """" _
                        & " menuimageover=""" & EncodeHTML(MenuImageOver) & """" _
                        & " pageid=""" & RootPageID & """"
                    If SubNodes <> "" Then
                        GetSectionNodes = GetSectionNodes & ">" _
                            & kmaIndent(SubNodes) _
                            & vbCrLf & vbTab & "</section>"
                    Else
                        GetSectionNodes = GetSectionNodes & "/>"
                    End If
                    Call CSSections.GoNext()
                Loop
                Call CSSections.Close()
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Get all the root page nodes (with their children)
        ''' </summary>
        ''' <param name="IsWorkflowRendering"></param>
        ''' <param name="PCC"></param>
        ''' <param name="BuildVersion"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetPagesNode(IsWorkflowRendering As Boolean, PCC As Object, BuildVersion As String) As String
            Dim returnValue As String = ""
            Try
                '
                '
                'Dim Overview As String
                'Dim Active As Boolean
                'Dim PseudoChildChildPagesFound As Boolean
                'Dim PCCRowPtr As Integer
                'Dim SortForward As Boolean
                'Dim SortFieldName As String
                'Dim SortPtr As Integer
                Dim Ptr As Integer
                'Dim ChildPageCount As Integer
                'Dim ChildPagesFoundTest As String
                'Dim FieldList As String
                'Dim ChildCountWithNoPubs As Integer
                'Dim MenuID As Integer
                'Dim MenuCaption As String
                'Dim ChildCount As Integer
                'Dim ChildSize As Integer
                'Dim ChildPointer As Integer
                'Dim ChildID() As Integer
                'Dim ChildAllowChild() As Boolean
                'Dim ChildCaption() As String
                'Dim ChildLink() As String
                'Dim ChildOverview() As String
                'Dim ChildSortMethodID() As Integer
                'Dim ChildChildPagesFound() As Boolean
                'Dim ContentID As Integer
                'Dim MenuLinkOverRide As String
                'Dim PageID As Integer
                'Dim UsedPageIDStringLocal As String
                'Dim Criteria As String
                'Dim MenuDepthLocal As Integer
                Dim OrderByCriteria As String
                'Dim WorkingLink As String
                'Dim TemplateID As Integer
                'Dim ContentControlID As Integer
                'Dim Link As String
                'Dim PubDate As Date
                Dim PCCPtr As Integer
                'Dim DateExpires As Date
                'Dim DateArchive As Date
                'Dim IsIncludedInMenu As Boolean
                Dim PCCPtrs() As Integer
                Dim PtrCnt As Integer
                'Dim SortSplit() As String
                'Dim SortSplitCnt As Integer
                'Dim Index As keyPtrIndexClass
                'Dim PCCColPtr As Integer
                Dim PCCPtrsSorted As Object
                'Dim AllowInMenus As Boolean
                '
                '   Determine default orderby for pages
                '
                OrderByCriteria = cpCore.main_GetContentProperty("page content", "defaultsortmethod")
                If OrderByCriteria = "" Then
                    OrderByCriteria = "ID"
                End If
                '
                '   Get list of root pages and sort them
                '
                PCCPtr = cpCore.cache_pageContent_getFirstChildPtr(0, IsWorkflowRendering, False)
                PtrCnt = 0
                Do While PCCPtr >= 0
                    ReDim Preserve PCCPtrs(PtrCnt)
                    PCCPtrs(PtrCnt) = PCCPtr
                    PtrCnt = PtrCnt + 1
                    PCCPtr = cpCore.cache_pageContent_parentIdIndex.getPtr("0")
                Loop
                If PtrCnt > 0 Then
                    PCCPtrsSorted = cpCore.cache_pageContent_getPtrsSorted(PCCPtrs, OrderByCriteria)
                End If
                '
                '   Get Nodes from list of root pages
                '
                Ptr = 0
                Do While Ptr < PtrCnt
                    PCCPtr = PCCPtrsSorted(Ptr)
                    returnValue = returnValue & GetPageNode(PCCPtr, IsWorkflowRendering, PCC, BuildVersion, OrderByCriteria)
                    Ptr = Ptr + 1
                Loop
                '
                ' wrap it in the pages node
                '
                If returnValue = "" Then
                    returnValue = "<pages/>"
                Else
                    returnValue = "" _
                        & vbCrLf & vbTab & "<pages>" _
                        & kmaIndent(returnValue) _
                        & vbCrLf & vbTab & "</pages>"
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnValue
        End Function
        '
        '======================================================================================
        '   Get a PageNode for the PCCPtr given with its child nodes
        '
        '====================================================================================================
        Private Function GetPageNode(PCCPtr As Integer, IsWorkflowRendering As Boolean, PCC As Object, BuildVersion As String, DefaultChildListOrderByCriteria As String) As String
            Dim returnValue As String = ""
            Try
                Dim Ptr As Integer
                Dim Rows2 As Object
                Dim CS As CPCSBaseClass
                Dim GroupIDAccessList As String
                Dim PageContentBlock As Boolean
                Dim PagePageBlock As Boolean
                Dim PCCChildPtr As Integer
                Dim PageMenuImage As String
                Dim PageMenuImageOver As String
                Dim PageMenuImageDown As String
                Dim PageMenuImageDownOver As String
                Dim PageMenuNewWindow As Boolean
                Dim PageOverview As String
                Dim ChildNodes As String
                Dim PCCPtrsSorted As Object
                Dim ChildNodePtr As Integer
                Dim ChildListOrderByCriteria As String
                Dim PCCPtrs() As Integer
                Dim ChildNodeCnt As Integer
                Dim PageName As String
                Dim Overview As String
                Dim CSSection As Integer
                Dim PseudoChildPagesFound As Boolean
                Dim IsValidPage As Boolean
                Dim DateExpires As Date
                Dim DateArchive As Date
                Dim PubDate As Date
                Dim ChildPageCount As Integer
                Dim ContentName As String
                Dim AddRootButton As Boolean
                Dim TopMenuCaption As String
                Dim PageMenuCaption As String
                '
                Dim PageID As Integer
                Dim BakeName As String
                Dim Criteria As String
                Dim NodeIDPrefix As String
                Dim PageChildListSortMethodID As Integer
                Dim PageLink As String
                Dim PageLinkNoRedirect As String
                Dim PageParentID As Integer
                Dim PageTemplateID As Integer
                Dim PageCCID As Integer
                Dim PageAllowChildListDisplay As Boolean
                Dim PageMenuLinkOverRide As String
                Dim ChildPagesFound As Boolean
                Dim FieldList As String
                Dim ChildPagesFoundTest As String
                '
                ContentName = "Page Content"
                '
                ' Skip if expired, archive or non-published
                '
                DateExpires = encodeDateMinValue(EncodeDate(PCC(PCC_DateExpires, PCCPtr)))
                DateArchive = encodeDateMinValue(EncodeDate(PCC(PCC_DateArchive, PCCPtr)))
                PubDate = encodeDate(pcc(PCC_PubDate, PCCPtr))
                IsValidPage = ((DateExpires = Date.MinValue) Or (DateExpires > Now)) And ((PubDate = Date.MinValue) Or (PubDate < Now))
                If IsValidPage Then
                    '
                    ' Get page values
                    '
                    PageID = encodeInteger(pcc(PCC_ID, PCCPtr))
                    PageName = encodeText(pcc(PCC_Name, PCCPtr))
                    PageChildListSortMethodID = encodeInteger(pcc(PCC_ChildListSortMethodID, PCCPtr))
                    PageMenuCaption = encodeText(pcc(PCC_MenuHeadline, PCCPtr))
                    If PageMenuCaption = "" Then
                        PageMenuCaption = PageName
                        If PageMenuCaption = "" Then
                            PageMenuCaption = "Page " & CStr(PageID)
                        End If
                    End If
                    PageCCID = encodeInteger(pcc(PCC_ContentControlID, PCCPtr))
                    PageTemplateID = encodeInteger(pcc(PCC_TemplateID, PCCPtr))
                    PageAllowChildListDisplay = encodeBoolean(pcc(PCC_AllowChildListDisplay, PCCPtr))
                    PageMenuLinkOverRide = encodeText(pcc(PCC_Link, PCCPtr))
                    PageParentID = encodeInteger(pcc(PCC_ParentID, PCCPtr))
                    PageLink = cpCore.main_GetPageLink(PageID)
                    PageContentBlock = encodeBoolean(pcc(PCC_BlockContent, PCCPtr))
                    PagePageBlock = encodeBoolean(pcc(PCC_BlockPage, PCCPtr))

                    GroupIDAccessList = ""
                    If PageContentBlock Or PagePageBlock Then
                        If CS.opensql("select groupid from ccPageContentBlockRules where recordid=" & PageID) Then
                            Do
                                GroupIDAccessList &= "," & CS.GetInteger("groupId").ToString
                                CS.GoNext()
                            Loop While CS.OK
                            GroupIDAccessList = Mid(GroupIDAccessList, 2)
                        End If
                    End If
                    '
                    '   Child Nodes
                    '
                    ChildListOrderByCriteria = ""
                    If PageChildListSortMethodID > 0 Then
                        ChildListOrderByCriteria = cpCore.main_GetSortMethodByID(PageChildListSortMethodID)
                    End If
                    If ChildListOrderByCriteria = "" Then
                        ChildListOrderByCriteria = DefaultChildListOrderByCriteria
                    End If
                    If ChildListOrderByCriteria = "" Then
                        ChildListOrderByCriteria = "ID"
                    End If
                    PCCChildPtr = cpCore.cache_pageContent_parentIdIndex.getPtr(PageID.ToString)
                    PCCChildPtr = cpCore.cache_pageContent_getFirstChildPtr(PageID, IsWorkflowRendering, False)
                    ChildNodeCnt = 0
                    Do While PCCChildPtr >= 0
                        ReDim Preserve PCCPtrs(ChildNodeCnt)
                        PCCPtrs(ChildNodeCnt) = PCCChildPtr
                        ChildNodeCnt = ChildNodeCnt + 1
                        PCCChildPtr = cpCore.cache_pageContent_parentIdIndex.getNextPtrMatch(PageID.ToString)
                    Loop
                    If ChildNodeCnt > 0 Then
                        PCCPtrsSorted = cpCore.cache_pageContent_getPtrsSorted(PCCPtrs, ChildListOrderByCriteria)
                        '
                        ChildNodePtr = 0
                        Do While ChildNodePtr < ChildNodeCnt
                            PCCPtr = PCCPtrsSorted(ChildNodePtr)
                            ChildNodes = ChildNodes & GetPageNode(PCCPtr, IsWorkflowRendering, PCC, BuildVersion, ChildListOrderByCriteria)
                            ChildNodePtr = ChildNodePtr + 1
                        Loop
                    End If
                    '
                    ' Create Page Node
                    '
                    GetPageNode = "" _
                        & vbCrLf & vbTab & "<page" _
                        & " id=""p" & CStr(PageID) & """" _
                        & " PageID=""" & CStr(PageID) & """" _
                        & " Caption=""" & PageMenuCaption & """" _
                        & " link=""" & encodeHTML(PageLink) & """" _
                        & " name=""" & encodeHTML(PageName) & """" _
                        & " newwindow=""" & encodeBoolean(PageMenuNewWindow) & """" _
                        & " overview=""" & encodeHTML(PageOverview) & """" _
                        & " contentblock=""" & PageContentBlock & """" _
                        & " groupaccesslist=""" & encodeHTML(GroupIDAccessList) & """" _
                        & ""
                    '& " ImageSrc=""" & encodeHTML(PageMenuImage) & """" _
                    '& " ImageOverSrc=""" & encodeHTML(PageMenuImageOver) & """" _
                    '& " ImageDownSrc=""" & encodeHTML(PageMenuImageDown) & """" _
                    '& " ImageDownOverSrc=""" & encodeHTML(PageMenuImageDownOver) & """" _
                    '& " pageblock=""" & PagePageBlock & """" _

                    If ChildNodes = "" Then
                        GetPageNode = GetPageNode & "/>"
                    Else
                        GetPageNode = GetPageNode & ">" _
                            & kmaIndent(ChildNodes) _
                            & vbCrLf & vbTab & "</page>"
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnValue
        End Function
    End Class
End Namespace


