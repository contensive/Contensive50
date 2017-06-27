VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "ResortClass"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True

Option Explicit
'
'========================================================================
'
'========================================================================
'
'Private Main As ccWeb3.MainClass
Private Csv As Object
Private Main As Object
'
'=================================================================================
'   Execute Method, v3.4 Interface
'=================================================================================
'
Public Function Execute(CsvObject As Object, MainObject As Object, OptionString As String, FilterInput As String) As String
    On Error GoTo ErrorTrap
    '
    Dim PageID As Long
    Dim PageList As String
    Dim Pages() As String
    Dim PageCnt As Long
    Dim ParentPage As String
    Dim Pos As Long
    Dim ParentListName As String
    Dim ParentID As Long
    Dim SortMethodID As Long
    Dim CS As Long
    Dim Ptr As Long
    Dim SortOrder As String
    '
    Set Main = MainObject
    Set Csv = CsvObject
    '
    PageList = Main.getstreamtext("sortlist")
    Pages = Split(PageList, ",")
    PageCnt = UBound(Pages) + 1
    If PageCnt > 1 Then '
        ' childPageList6_name
        ParentPage = Pages(0)
        Pos = InStr(1, ParentPage, "_")
        If Pos > 0 Then
            ParentListName = Mid(ParentPage, Pos + 1)
            ParentID = kmaEncodeInteger(Mid(ParentPage, 14, Pos - 14))
            If ParentID > 0 Then
                '
                ' Verify Sort Method
                '
                SortMethodID = Main.getrecordid("Sort Methods", "By Alpha Sort Order Field")
                If SortMethodID = 0 Then
                    SortMethodID = Main.getrecordid("Sort Methods", "Alpha Sort Order Field")
                End If
                If SortMethodID > 0 Then
                    CS = Main.opencscontentrecord("page Content", ParentID)
                    If Main.iscsok(CS) Then
                        If Main.getcsinteger(CS, "ChildListSortMethodID") <> SortMethodID Then
                            Call Main.setcs(CS, "ChildListSortMethodID", SortMethodID)
                        End If
                    End If
                    Call Main.closecs(CS)
                    '
                    ' update the sort order in all the child pages
                    '
                    For Ptr = 1 To PageCnt - 1
                        PageID = kmaEncodeInteger(Mid(Pages(Ptr), 5))
                        If PageID > 0 Then
                            SortOrder = CStr(100000 + (Ptr * 10))
                            CS = Main.opencscontentrecord("page Content", PageID)
                            If Main.iscsok(CS) Then
                                Call Main.setcs(CS, "sortorder", SortOrder)
                            End If
                            Call Main.closecs(CS)
                        End If
                    Next
                    Call Main.ClearPageContentCache
                    Call Main.ClearPageTemplateCache
                    Call Main.ClearSiteSectionCache
                    Call Main.ClearBake("")
                End If
            End If
        End If
    End If
    '
    ' For Page Addons, return the result
    '
    Execute = ""
    '
    Exit Function
ErrorTrap:
    'HandleError
End Function
