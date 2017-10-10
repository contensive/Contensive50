
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.PageManager
    Public Class childPageResortClass
        Inherits AddonBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' pageManager addon interface
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Overrides Function execute(cp As Contensive.BaseClasses.CPBaseClass) As Object
            Dim returnHtml As String = ""
            Try
                '
                ' -- allowed only for addons included in this project
                Dim cpCore As coreClass = DirectCast(cp, CPClass).core
                '
                Dim pageCommaList As String = cp.Doc.GetText("sortlist")
                Dim pageList As List(Of String) = New List(Of String)(Split(pageCommaList, ","))
                Dim ParentPageValues As String()
                If pageList.Count > 1 Then
                    '
                    ' -- first element is parent page (listName_id)
                    ParentPageValues = Split(pageList(0), "_")
                    If (ParentPageValues.Count < 2) Then
                        '
                        ' -- parent page is not valid
                        cp.Site.ErrorReport(New ArgumentException("pageResort requires first value to identify the parent page"))
                    Else

                        Dim parentPageList As String = ParentPageValues(0)
                        Dim parentPageId As Integer = EncodeInteger(ParentPageValues(1))
                        If (parentPageId = 0) Then
                            '
                            ' -- parent page is not valid
                            cp.Site.ErrorReport(New ArgumentException("pageResort requires a parent page id"))
                        Else
                            '
                            ' -- verify parent page sort method is set correctly
                            Dim parentPage As Models.Entity.pageContentModel = pageContentModel.create(cpCore, parentPageId, New List(Of String))
                            If (parentPage Is Nothing) Then
                                '
                                ' -- parent page is not valid
                                cp.Site.ErrorReport(New ArgumentException("pageResort requires a parent page id"))
                            Else
                                '
                                ' -- verify page set to required sort method Id
                                Dim sortMethod As Models.Entity.sortMethodModel = sortMethodModel.createByName(cpCore, "By Alpha Sort Order Field")
                                If (sortMethod Is Nothing) Then
                                    sortMethod = sortMethodModel.createByName(cpCore, "Alpha Sort Order Field")
                                End If
                                If (sortMethod Is Nothing) Then
                                    '
                                    ' -- create the required sortMethod
                                    sortMethod = sortMethodModel.add(cpCore)
                                    sortMethod.name = "By Alpha Sort Order Field"
                                    sortMethod.OrderByClause = "sortOrder"
                                    sortMethod.save(cpCore)
                                End If
                                If (parentPage.ChildListSortMethodID <> sortMethod.id) Then
                                    '
                                    ' -- update page if not set correctly
                                    parentPage.ChildListSortMethodID = sortMethod.id
                                    parentPage.save(cpCore)
                                End If
                                Dim pagePtr As Integer = 0
                                For Each PageIDText As String In pageList
                                    If pagePtr > 0 Then
                                        '
                                        ' -- non-first elementa are a list of page Ids
                                        Dim pageId As Integer = EncodeInteger(PageIDText)
                                        If pageId = 0 Then
                                            '
                                            ' -- invalid child page
                                            cp.Site.ErrorReport(New ApplicationException("child page id is invalid from remote request [" & pageCommaList & "]"))
                                        Else
                                            Dim SortOrder As String = CStr(100000 + (pagePtr * 10))
                                            Dim childPage As Models.Entity.pageContentModel = pageContentModel.create(cpCore, pageId, New List(Of String))
                                            If (childPage.SortOrder <> SortOrder) Then
                                                childPage.SortOrder = SortOrder
                                                childPage.save(cpCore)
                                            End If
                                        End If
                                    End If
                                    pagePtr += 1
                                Next
                            End If
                        End If
                    End If
                End If
                execute = ""
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return returnHtml
        End Function
    End Class
End Namespace
